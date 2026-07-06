using System.Globalization;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Services;

internal static class ElementProvenanceCollector
{
    public static DocumentMetadataResult CollectDocumentMetadata(UIApplication app)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");

        var centralPath = string.Empty;
        var worksharingNotes = string.Empty;
        if (doc.IsWorkshared)
        {
            try
            {
                var central = doc.GetWorksharingCentralModelPath();
                centralPath = ModelPathUtils.ConvertModelPathToUserVisiblePath(central);
            }
            catch (Exception ex)
            {
                worksharingNotes = AppendNote(worksharingNotes, $"Central path unavailable: {ex.GetBaseException().Message}");
            }
        }

        var fileLastSavedBy = string.Empty;
        string? fileLastSavedTimeUtc = null;
        var fileCreatedBy = string.Empty;
        var fileDocumentVersion = string.Empty;
        if (!string.IsNullOrWhiteSpace(doc.PathName) && File.Exists(doc.PathName))
        {
            try
            {
                var info = BasicFileInfo.Extract(doc.PathName);
                fileLastSavedBy = info.Username ?? string.Empty;
                fileCreatedBy = info.Username ?? string.Empty;
                fileDocumentVersion = info.Format ?? string.Empty;
                fileLastSavedTimeUtc = ToUtcString(File.GetLastWriteTimeUtc(doc.PathName));
            }
            catch (Exception ex)
            {
                worksharingNotes = AppendNote(
                    worksharingNotes,
                    $"BasicFileInfo unavailable: {ex.GetBaseException().Message}");
            }
        }

        return new DocumentMetadataResult
        {
            RevitVersion = app.Application.VersionNumber,
            DocumentTitle = doc.Title,
            DocumentPath = doc.PathName ?? string.Empty,
            IsModified = doc.IsModified,
            IsWorkshared = doc.IsWorkshared,
            IsFamilyDocument = doc.IsFamilyDocument,
            CurrentUser = app.Application.Username,
            ActiveViewName = doc.ActiveView?.Name ?? string.Empty,
            CentralModelPath = centralPath,
            FileLastSavedBy = fileLastSavedBy,
            FileLastSavedTimeUtc = fileLastSavedTimeUtc,
            FileCreatedBy = fileCreatedBy,
            FileDocumentVersion = fileDocumentVersion,
            WorksharingNotes = worksharingNotes,
            ProjectInformation = ReadProjectInformation(doc),
        };
    }

    public static List<ElementMetadataRecord> CollectElementMetadata(
        UIApplication app,
        ElementMetadataRequest request)
    {
        var uiDoc = app.ActiveUIDocument
            ?? throw new InvalidOperationException("No active UI document.");

        var doc = uiDoc.Document;
        var ids = ResolveElementIds(uiDoc, request);
        var includeParameters = request.IncludeParameters;
        var parameterNames = (request.ParameterNames ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var results = new List<ElementMetadataRecord>();
        foreach (var id in ids)
        {
            var element = doc.GetElement(id);
            if (element is null)
            {
                results.Add(new ElementMetadataRecord
                {
                    ElementId = ToInt(id),
                    Found = false,
                    Note = "Element not found in active document.",
                });
                continue;
            }

            results.Add(BuildElementRecord(doc, element, includeParameters, parameterNames));
        }

        return results;
    }

    private static List<ElementId> ResolveElementIds(UIDocument uiDoc, ElementMetadataRequest request)
    {
        if (request.ElementIds is { Count: > 0 })
        {
            return request.ElementIds
                .Where(i => i > 0)
                .Select(i => new ElementId((long)i))
                .Distinct()
                .ToList();
        }

        if (request.UseSelection)
        {
            return uiDoc.Selection.GetElementIds().ToList();
        }

        throw new InvalidOperationException(
            "Provide elementIds or set useSelection=true with elements selected in Revit.");
    }

    private static ElementMetadataRecord BuildElementRecord(
        Document doc,
        Element element,
        bool includeParameters,
        HashSet<string> parameterNames)
    {
        var familyName = string.Empty;
        var typeName = string.Empty;
        if (element is FamilyInstance fi)
        {
            familyName = fi.Symbol?.FamilyName ?? string.Empty;
            typeName = fi.Symbol?.Name ?? string.Empty;
        }
        else if (element is ElementType type)
        {
            familyName = type.FamilyName;
            typeName = type.Name ?? string.Empty;
        }

        return new ElementMetadataRecord
        {
            Found = true,
            ElementId = ToInt(element.Id),
            UniqueId = element.UniqueId,
            Name = element.Name ?? string.Empty,
            Category = element.Category?.Name ?? string.Empty,
            FamilyName = familyName,
            TypeName = typeName,
            VersionGuid = element.VersionGuid.ToString(),
            IsPinned = element.Pinned,
            IsModifiable = element.IsModifiable,
            CreatedPhaseId = NullableElementId(element.CreatedPhaseId),
            DemolishedPhaseId = NullableElementId(element.DemolishedPhaseId),
            WorksetId = element.WorksetId.IntegerValue,
            DesignOptionId = NullableElementId(element.DesignOption?.Id),
            GroupId = NullableElementId(element.GroupId),
            Worksharing = doc.IsWorkshared ? CollectWorksharing(doc, element) : null,
            Parameters = includeParameters ? ReadParameters(element, parameterNames) : [],
        };
    }

    private static ElementWorksharingRecord CollectWorksharing(Document doc, Element element)
    {
        var id = element.Id;
        var creator = string.Empty;
        var lastChangedBy = string.Empty;
        var owner = string.Empty;
        var note = string.Empty;

        try
        {
            var tooltip = WorksharingUtils.GetWorksharingTooltipInfo(doc, id);
            creator = tooltip.Creator ?? string.Empty;
            lastChangedBy = tooltip.LastChangedBy ?? string.Empty;
            owner = tooltip.Owner ?? string.Empty;
        }
        catch (Exception ex)
        {
            note = $"Worksharing tooltip unavailable: {ex.GetBaseException().Message}";
        }

        return new ElementWorksharingRecord
        {
            CheckoutStatus = WorksharingUtils.GetCheckoutStatus(doc, id).ToString(),
            ModelUpdatesStatus = WorksharingUtils.GetModelUpdatesStatus(doc, id).ToString(),
            Creator = creator,
            LastChangedBy = lastChangedBy,
            Owner = owner,
            Note = note,
        };
    }

    private static Dictionary<string, string> ReadParameters(
        Element element,
        HashSet<string> parameterNames)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Parameter param in element.Parameters)
        {
            if (param.Definition is null)
            {
                continue;
            }

            var name = param.Definition.Name;
            if (parameterNames.Count > 0 && !parameterNames.Contains(name))
            {
                continue;
            }

            map[name] = FormatParameterValue(param);
        }

        return map;
    }

    private static string FormatParameterValue(Parameter param)
    {
        if (!param.HasValue)
        {
            return string.Empty;
        }

        return param.StorageType switch
        {
            StorageType.String => param.AsString() ?? string.Empty,
            StorageType.Integer => param.AsInteger().ToString(CultureInfo.InvariantCulture),
            StorageType.Double => param.AsDouble().ToString(CultureInfo.InvariantCulture),
            StorageType.ElementId => FormatElementId(param.AsElementId()),
            _ => param.AsValueString() ?? string.Empty,
        };
    }

    private static ProjectInformationRecord ReadProjectInformation(Document doc)
    {
        var info = doc.ProjectInformation;
        if (info is null)
        {
            return new ProjectInformationRecord();
        }

        return new ProjectInformationRecord
        {
            Name = GetParamString(info, BuiltInParameter.PROJECT_NAME),
            Number = GetParamString(info, BuiltInParameter.PROJECT_NUMBER),
            ClientName = GetParamString(info, BuiltInParameter.CLIENT_NAME),
            Address = GetParamString(info, BuiltInParameter.PROJECT_ADDRESS),
            Status = GetParamString(info, BuiltInParameter.PROJECT_STATUS),
            IssueDate = GetParamString(info, BuiltInParameter.PROJECT_ISSUE_DATE),
            Author = GetParamString(info, BuiltInParameter.PROJECT_AUTHOR),
            BuildingName = GetParamString(info, BuiltInParameter.PROJECT_BUILDING_NAME),
        };
    }

    private static string GetParamString(Element element, BuiltInParameter builtIn)
    {
        var p = element.get_Parameter(builtIn);
        if (p is null || !p.HasValue)
        {
            return string.Empty;
        }

        return p.AsString() ?? p.AsValueString() ?? string.Empty;
    }

    private static int? NullableElementId(ElementId? id)
    {
        if (id is null || id == ElementId.InvalidElementId)
        {
            return null;
        }

        return ToInt(id);
    }

    private static int ToInt(ElementId id) => checked((int)id.Value);

    private static string FormatElementId(ElementId? id)
    {
        if (id is null || id == ElementId.InvalidElementId)
        {
            return string.Empty;
        }

        return id.Value.ToString(CultureInfo.InvariantCulture);
    }

    private static string? ToUtcString(DateTime value)
    {
        var dt = value;
        if (dt.Kind == DateTimeKind.Unspecified)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
        }

        return dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    private static string AppendNote(string existing, string addition)
    {
        return string.IsNullOrWhiteSpace(existing) ? addition : $"{existing}; {addition}";
    }
}
