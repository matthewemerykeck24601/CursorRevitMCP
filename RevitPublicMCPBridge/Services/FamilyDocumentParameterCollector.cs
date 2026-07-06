using System.Globalization;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitPublicMCPBridge.Services;

internal static class FamilyDocumentParameterCollector
{
    public static GetFamilyParametersResult Collect(UIApplication app, GetFamilyParametersRequest request)
    {
        var doc = app.ActiveUIDocument?.Document
            ?? throw new InvalidOperationException("No active Revit document.");

        if (!doc.IsFamilyDocument)
        {
            throw new InvalidOperationException(
                "Active document is not a family document. Open a .rfa in the Family Editor and try again.");
        }

        var familyManager = doc.FamilyManager
            ?? throw new InvalidOperationException("FamilyManager is not available on the active document.");

        var nameFilter = (request.ParameterNames ?? [])
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var types = familyManager.Types
            .Cast<FamilyType>()
            .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var parameters = familyManager.Parameters
            .Cast<FamilyParameter>()
            .Where(p => p.Definition is not null)
            .Where(p => nameFilter.Count == 0 || nameFilter.Contains(p.Definition.Name))
            .OrderBy(p => p.Definition.Name, StringComparer.OrdinalIgnoreCase)
            .Select(p => BuildParameterRecord(doc, familyManager, p, types))
            .ToList();

        return new GetFamilyParametersResult
        {
            FamilyName = doc.OwnerFamily?.Name ?? doc.Title,
            DocumentTitle = doc.Title,
            DocumentPath = doc.PathName ?? string.Empty,
            CategoryName = doc.OwnerFamily?.FamilyCategory?.Name ?? string.Empty,
            TypeCount = types.Count,
            Parameters = parameters,
        };
    }

    private static FamilyParameterRecord BuildParameterRecord(
        Document familyDoc,
        FamilyManager familyManager,
        FamilyParameter familyParameter,
        List<FamilyType> types)
    {
        var definition = familyParameter.Definition;
        var formula = familyParameter.Formula;
        if (string.IsNullOrWhiteSpace(formula))
        {
            formula = null;
        }

        var values = types
            .Select(type => new FamilyParameterTypeValue
            {
                FamilyType = type.Name,
                Value = FormatFamilyParameterValue(familyDoc, type, familyParameter),
            })
            .ToList();

        return new FamilyParameterRecord
        {
            Name = definition.Name,
            ParameterType = DescribeParameterType(definition),
            StorageType = familyParameter.StorageType.ToString(),
            IsInstance = familyParameter.IsInstance,
            IsShared = familyParameter.IsShared,
            IsDeterminedByFormula = familyParameter.IsDeterminedByFormula,
            Formula = formula,
            Group = DescribeParameterGroup(definition),
            Values = values,
        };
    }

    private static string DescribeParameterType(Definition definition)
    {
        try
        {
            return LabelUtils.GetLabelForSpec(definition.GetDataType());
        }
        catch
        {
            return definition.GetDataType().TypeId;
        }
    }

    private static string DescribeParameterGroup(Definition definition)
    {
        try
        {
            return LabelUtils.GetLabelForGroup(definition.GetGroupTypeId());
        }
        catch
        {
            return definition.GetGroupTypeId().TypeId;
        }
    }

    private static string FormatFamilyParameterValue(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        if (familyParameter.IsDeterminedByFormula)
        {
            try
            {
                return FormatFamilyParameterValueRaw(familyDoc, familyType, familyParameter);
            }
            catch
            {
                return familyParameter.Formula ?? string.Empty;
            }
        }

        return FormatFamilyParameterValueRaw(familyDoc, familyType, familyParameter);
    }

    private static string FormatFamilyParameterValueRaw(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        try
        {
            return familyParameter.StorageType switch
            {
                StorageType.String => familyType.AsString(familyParameter) ?? string.Empty,
                StorageType.Integer => familyType.AsInteger(familyParameter)?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                StorageType.Double => FormatDoubleValue(familyDoc, familyType, familyParameter),
                StorageType.ElementId => FormatElementIdValue(familyDoc, familyType, familyParameter),
                _ => string.Empty,
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string FormatDoubleValue(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        var value = familyType.AsDouble(familyParameter);
        if (!value.HasValue)
        {
            return string.Empty;
        }

        try
        {
            return UnitFormatUtils.Format(
                familyDoc.GetUnits(),
                familyParameter.Definition.GetDataType(),
                value.Value,
                forEditing: false);
        }
        catch
        {
            return value.Value.ToString("G17", CultureInfo.InvariantCulture);
        }
    }

    private static string FormatElementIdValue(
        Document familyDoc,
        FamilyType familyType,
        FamilyParameter familyParameter)
    {
        var id = familyType.AsElementId(familyParameter);
        if (id == ElementId.InvalidElementId || id.Value <= 0)
        {
            return string.Empty;
        }

        var element = familyDoc.GetElement(id);
        return element?.Name ?? id.Value.ToString(CultureInfo.InvariantCulture);
    }
}
