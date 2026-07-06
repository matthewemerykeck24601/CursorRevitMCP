namespace RevitPublicMCPBridge.Services;

public sealed class GatewayEnvelope<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }
}

public sealed class CurrentViewElementsRequest
{
    public string[]? ModelCategoryList { get; set; }

    public bool IncludeHidden { get; set; }

    public int? Limit { get; set; }
}

public sealed class CurrentViewElementRecord
{
    public int ElementId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;
}

public sealed class FamilyTypesRequest
{
    public string[]? CategoryList { get; set; }

    public string? FamilyNameFilter { get; set; }

    public int? Limit { get; set; }
}

public sealed class FamilyTypeRecord
{
    public int TypeId { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;
}

public sealed class CreatePointElementsRequest
{
    public List<CreatePointElementInput> Data { get; set; } = [];
}

public sealed class CreatePointElementInput
{
    public string Name { get; set; } = string.Empty;

    public int TypeId { get; set; }

    public PointInput LocationPoint { get; set; } = new();

    public double Width { get; set; }

    public double Depth { get; set; }

    public double Height { get; set; }

    public double BaseLevel { get; set; }

    public double BaseOffset { get; set; }

    public double Rotation { get; set; }

    public int? HostWallId { get; set; }

    public bool FacingFlipped { get; set; }
}

public sealed class PointInput
{
    public double X { get; set; }

    public double Y { get; set; }

    public double Z { get; set; }
}

public sealed class CreatePointElementResult
{
    public string Name { get; init; } = string.Empty;

    public int TypeId { get; init; }

    public int ElementId { get; init; }

    public int? HostWallId { get; init; }
}

public sealed class OpenSelectedFamilyEditorRequest
{
}

public sealed class OpenSelectedFamilyEditorResult
{
    public int SelectedElementId { get; init; }

    public string SelectedElementName { get; init; } = string.Empty;

    public string SelectedElementCategory { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string FamilyDocumentTitle { get; init; } = string.Empty;

    public bool ActivatedByPath { get; init; }
}

public sealed class OpenFamilyEditorByElementIdRequest
{
    public int ElementId { get; set; }
}

public sealed class EnsureSharedParametersRequest
{
    public string? SharedParameterFilePath { get; set; }

    public string? GroupName { get; set; }

    public List<SharedParameterDefinitionInput> Parameters { get; set; } = [];
}

public sealed class SharedParameterDefinitionInput
{
    public string Name { get; set; } = string.Empty;

    public string? DataType { get; set; }

    public string? Description { get; set; }

    public bool? Visible { get; set; }

    public bool? UserModifiable { get; set; }
}

public sealed class EnsureSharedParametersResult
{
    public string SharedParameterFilePath { get; init; } = string.Empty;

    public string GroupName { get; init; } = string.Empty;

    public List<SharedParameterDefinitionRecord> Parameters { get; init; } = [];
}

public sealed class SharedParameterDefinitionRecord
{
    public string Name { get; init; } = string.Empty;

    public string DataType { get; init; } = string.Empty;

    public string Guid { get; init; } = string.Empty;

    public bool Created { get; init; }
}

public sealed class BindSharedParametersRequest
{
    public string? SharedParameterFilePath { get; set; }

    public string? GroupName { get; set; }

    public List<string> ParameterNames { get; set; } = [];

    public List<string> CategoryList { get; set; } = [];

    public string? BindingType { get; set; }

    public string? ParameterGroup { get; set; }
}

public sealed class BindSharedParametersResult
{
    public string SharedParameterFilePath { get; init; } = string.Empty;

    public string GroupName { get; init; } = string.Empty;

    public string BindingType { get; init; } = string.Empty;

    public string ParameterGroup { get; init; } = string.Empty;

    public List<string> Categories { get; init; } = [];

    public List<BindSharedParameterRecord> Parameters { get; init; } = [];
}

public sealed class BindSharedParameterRecord
{
    public string Name { get; init; } = string.Empty;

    public bool Inserted { get; init; }

    public bool ReInserted { get; init; }
}

public sealed class SearchFamilyLibraryRequest
{
    public List<string>? LibraryRoots { get; set; }

    public string Query { get; set; } = string.Empty;

    public int? MaxResults { get; set; }
}

public sealed class FamilyLibraryMatch
{
    public string FamilyName { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public string LibraryRoot { get; init; } = string.Empty;

    public bool ExactNameMatch { get; init; }
}

public sealed class SearchFamilyLibraryResult
{
    public string Query { get; init; } = string.Empty;

    public List<string> LibraryRoots { get; init; } = [];

    public List<FamilyLibraryMatch> Matches { get; init; } = [];
}

public sealed class OpenFamilyFromLibraryRequest
{
    public string? FamilyPath { get; set; }

    public List<string>? LibraryRoots { get; set; }

    public string? Query { get; set; }
}

public sealed class OpenFamilyFromLibraryResult
{
    public string FamilyName { get; init; } = string.Empty;

    public string FamilyPath { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    public bool Activated { get; init; }
}

public sealed class LoadFamilyFromLibraryRequest
{
    public string? FamilyPath { get; set; }

    public List<string>? LibraryRoots { get; set; }

    public string? Query { get; set; }

    public bool OverwriteParameterValues { get; set; } = true;
}

public sealed class LoadFamilyFromLibraryResult
{
    public string FamilyName { get; init; } = string.Empty;

    public string FamilyPath { get; init; } = string.Empty;

    public int FamilyId { get; init; }

    public bool Loaded { get; init; }

    public bool FoundExisting { get; init; }
}

public sealed class UpgradeFamilyLibraryVersionRequest
{
    public string? SourceRoot { get; set; }

    public string? TargetRoot { get; set; }

    public string? Query { get; set; }

    public int? SourceYear { get; set; }

    public int? TargetYear { get; set; }

    public int? MaxFiles { get; set; }

    public bool OverwriteExisting { get; set; } = true;
}

public sealed class UpgradeFamilyLibraryVersionResult
{
    public int RunningRevitYear { get; init; }

    public int TargetYear { get; init; }

    public string SourceRoot { get; init; } = string.Empty;

    public string TargetRoot { get; init; } = string.Empty;

    public List<UpgradeFamilyRecord> Processed { get; init; } = [];
}

public sealed class UpgradeFamilyRecord
{
    public string SourcePath { get; init; } = string.Empty;

    public string TargetPath { get; init; } = string.Empty;

    public bool Upgraded { get; init; }

    public string Note { get; init; } = string.Empty;
}

public sealed class ExtractFamilyLibraryParametersRequest
{
    public string? SourceRoot { get; set; }

    public string? Query { get; set; }

    public int? MaxFiles { get; set; }

    public List<string> ParameterNames { get; set; } = [];
}

public sealed class ExtractFamilyLibraryParametersResult
{
    public int RunningRevitYear { get; init; }

    public string SourceRoot { get; init; } = string.Empty;

    public List<string> ParameterNames { get; init; } = [];

    public List<ExtractFamilyLibraryParameterRecord> Processed { get; init; } = [];
}

public sealed class ExtractFamilyLibraryParameterRecord
{
    public string FamilyName { get; init; } = string.Empty;

    public string FamilyPath { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public Dictionary<string, string> ParameterValues { get; init; } = [];

    public bool Extracted { get; init; }

    public string Note { get; init; } = string.Empty;
}

public sealed class AddSharedParametersToFamilyLibraryRequest
{
    public string? SourceRoot { get; set; }

    public string? Query { get; set; }

    public int? MaxFiles { get; set; }

    public string? SharedParameterFilePath { get; set; }

    public string? GroupName { get; set; }

    public List<string> ParameterNames { get; set; } = [];

    public string? ParameterGroup { get; set; }

    public bool IsInstance { get; set; } = true;
}

public sealed class AddSharedParametersToFamilyLibraryResult
{
    public int RunningRevitYear { get; init; }

    public string SourceRoot { get; init; } = string.Empty;

    public string SharedParameterFilePath { get; init; } = string.Empty;

    public string GroupName { get; init; } = string.Empty;

    public string ParameterGroup { get; init; } = string.Empty;

    public bool IsInstance { get; init; }

    public List<string> ParameterNames { get; init; } = [];

    public List<AddSharedParametersToFamilyLibraryRecord> Processed { get; init; } = [];
}

public sealed class AddSharedParametersToFamilyLibraryRecord
{
    public string FamilyName { get; init; } = string.Empty;

    public string FamilyPath { get; init; } = string.Empty;

    public List<string> Added { get; init; } = [];

    public List<string> SkippedExisting { get; init; } = [];

    public List<string> Failed { get; init; } = [];

    public bool Updated { get; init; }

    public string Note { get; init; } = string.Empty;
}

public sealed class ExtractFamilyDescriptionVariantsRequest
{
    public string? SourceRoot { get; set; }

    public string? Query { get; set; }

    public int? MaxFiles { get; set; }

    public string DescriptionParameterName { get; set; } = "IDENTITY_DESCRIPTION";

    public string DescriptionShortParameterName { get; set; } = "IDENTITY_DESCRIPTION_SHORT";

    public string ManufactureComponentParameterName { get; set; } = "MANUFACTURE_COMPONENT";

    public int MaxBooleanDrivers { get; set; } = 8;
}

public sealed class ExtractFamilyDescriptionVariantsResult
{
    public int RunningRevitYear { get; init; }

    public string SourceRoot { get; init; } = string.Empty;

    public List<ExtractFamilyDescriptionVariantRecord> Processed { get; init; } = [];
}

public sealed class ExtractFamilyDescriptionVariantRecord
{
    public string FamilyName { get; init; } = string.Empty;

    public string FamilyPath { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public string IdentityDescription { get; init; } = string.Empty;

    public string IdentityDescriptionShort { get; init; } = string.Empty;

    public string ManufactureComponent { get; init; } = string.Empty;

    public Dictionary<string, bool> DriverState { get; init; } = [];

    public bool Extracted { get; init; }

    public string Note { get; init; } = string.Empty;
}

public sealed class ElementMetadataRequest
{
    public List<int>? ElementIds { get; set; }

    public bool UseSelection { get; set; }

    public bool IncludeParameters { get; set; }

    public List<string>? ParameterNames { get; set; }
}

public sealed class DocumentMetadataResult
{
    public string RevitVersion { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    public string DocumentPath { get; init; } = string.Empty;

    public bool IsModified { get; init; }

    public bool IsWorkshared { get; init; }

    public bool IsFamilyDocument { get; init; }

    public string CurrentUser { get; init; } = string.Empty;

    public string ActiveViewName { get; init; } = string.Empty;

    public string CentralModelPath { get; init; } = string.Empty;

    public string FileLastSavedBy { get; init; } = string.Empty;

    public string? FileLastSavedTimeUtc { get; init; }

    public string FileCreatedBy { get; init; } = string.Empty;

    public string FileDocumentVersion { get; init; } = string.Empty;

    public string WorksharingNotes { get; init; } = string.Empty;

    public ProjectInformationRecord ProjectInformation { get; init; } = new();
}

public sealed class ProjectInformationRecord
{
    public string Name { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string ClientName { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public string IssueDate { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string BuildingName { get; init; } = string.Empty;
}

public sealed class ElementMetadataRecord
{
    public bool Found { get; init; }

    public int ElementId { get; init; }

    public string UniqueId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public string VersionGuid { get; init; } = string.Empty;

    public bool IsPinned { get; init; }

    public bool IsModifiable { get; init; }

    public int? CreatedPhaseId { get; init; }

    public int? DemolishedPhaseId { get; init; }

    public int WorksetId { get; init; }

    public int? DesignOptionId { get; init; }

    public int? GroupId { get; init; }

    public ElementWorksharingRecord? Worksharing { get; init; }

    public Dictionary<string, string> Parameters { get; init; } = [];

    public string Note { get; init; } = string.Empty;
}

public sealed class GetFamilyParametersRequest
{
    public List<string>? ParameterNames { get; set; }
}

public sealed class GetFamilyParametersResult
{
    public string FamilyName { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    public string DocumentPath { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public int TypeCount { get; init; }

    public List<FamilyParameterRecord> Parameters { get; init; } = [];
}

public sealed class FamilyParameterRecord
{
    public string Name { get; init; } = string.Empty;

    public string ParameterType { get; init; } = string.Empty;

    public string StorageType { get; init; } = string.Empty;

    public bool IsInstance { get; init; }

    public bool IsShared { get; init; }

    public bool IsDeterminedByFormula { get; init; }

    public string? Formula { get; init; }

    public string Group { get; init; } = string.Empty;

    public List<FamilyParameterTypeValue> Values { get; init; } = [];
}

public sealed class FamilyParameterTypeValue
{
    public string FamilyType { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}

public sealed class ElementWorksharingRecord
{
    public string CheckoutStatus { get; init; } = string.Empty;

    public string ModelUpdatesStatus { get; init; } = string.Empty;

    public string Creator { get; init; } = string.Empty;

    public string LastChangedBy { get; init; } = string.Empty;

    public string Owner { get; init; } = string.Empty;

    public string Note { get; init; } = string.Empty;
}

// ─── Toolbox expansion: shared result ─────────────────────────────────────
// Shared by every "create one element" action that returns { elementId, success }.
// Spatial inputs below are millimetres at the MCP boundary; the gateway converts
// mm -> Revit internal feet (see RevitApiGatewayService.MmToFeet).
public sealed class ElementCreationResult
{
    public int ElementId { get; init; }

    public bool Success { get; init; }
}

// ─── Toolbox expansion: Family Editor — Sketch & Solid Geometry ───────────
public sealed class CreateExtrusionRequest
{
    // Closed polygon in the work plane's 2D coordinates (mm). The loop is auto-closed.
    public List<PointInput> Profile { get; set; } = [];

    public double Depth { get; set; }

    public bool IsSolid { get; set; } = true;

    // Optional named reference plane to extrude from; defaults to the family XY plane at origin.
    public string? WorkPlaneName { get; set; }
}

// ─── Toolbox expansion: Family Editor — Reference Planes ──────────────────
public sealed class CreateReferencePlaneRequest
{
    public string Name { get; set; } = string.Empty;

    public PointInput BubbleEnd { get; set; } = new();

    public PointInput FreeEnd { get; set; } = new();

    // Optional. Used to derive the third in-plane point for NewReferencePlane2
    // (thirdPnt = bubbleEnd + cutVector). Defaults to bubbleEnd + Z (a vertical plane).
    public PointInput? CutVector { get; set; }

    public string? IsReference { get; set; }
}

// ─── Toolbox expansion: Project — Model Element Creation ──────────────────
// Group 6 (v13): flat mm coordinates at the MCP boundary; the gateway converts
// mm -> Revit internal feet. All level/type references arrive as element ids.
public sealed class CreateWallRequest
{
    public double StartX { get; set; }

    public double StartY { get; set; }

    public double StartZ { get; set; }

    public double EndX { get; set; }

    public double EndY { get; set; }

    public double EndZ { get; set; }

    public int LevelId { get; set; }

    // null -> project default wall type.
    public int? WallTypeId { get; set; }

    // null -> level-to-level (top constraint = next level above base).
    public double? Height { get; set; }

    public bool? Structural { get; set; }

    public bool? Flipped { get; set; }
}

public sealed class CreateWallResult
{
    public int Id { get; init; }

    public string WallTypeName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;

    public double LengthMm { get; init; }

    public double HeightMm { get; init; }
}

public sealed class CreateLevelRequest
{
    public double ElevationMm { get; set; }

    // null/blank -> Revit auto-names the level.
    public string? Name { get; set; }
}

public sealed class CreateLevelResult
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public double ElevationMm { get; init; }
}

public sealed class CreateGridRequest
{
    public double StartX { get; set; }

    public double StartY { get; set; }

    public double EndX { get; set; }

    public double EndY { get; set; }

    // null/blank -> Revit auto-names the grid (next in sequence).
    public string? Name { get; set; }
}

public sealed class CreateGridResult
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? Start { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? End { get; init; }
}

public sealed class CreateFloorRequest
{
    // Closed boundary loop in mm (XY; Z taken from the level elevation).
    public List<PointInput> Profile { get; set; } = [];

    public int LevelId { get; set; }

    // null -> project default floor type.
    public int? FloorTypeId { get; set; }

    public bool? Structural { get; set; }
}

public sealed class CreateFloorResult
{
    public int Id { get; init; }

    public string FloorTypeName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;

    public double AreaMm2 { get; init; }
}

public sealed class CreateRoomRequest
{
    public int LevelId { get; set; }

    public double LocationX { get; set; }

    public double LocationY { get; set; }

    public string? Name { get; set; }

    public string? Number { get; set; }
}

public sealed class CreateRoomResult
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;
}

public sealed class CreateStructuralColumnRequest
{
    public int FamilyTypeId { get; set; }

    public double LocationX { get; set; }

    public double LocationY { get; set; }

    public double LocationZ { get; set; }

    public int LevelId { get; set; }

    // null -> level-to-level (top constraint = next level above base).
    public double? HeightMm { get; set; }
}

public sealed class CreateStructuralColumnResult
{
    public int Id { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;

    public GeomPoint LocationMm { get; init; } = new();
}

public sealed class CreateBeamRequest
{
    public int FamilyTypeId { get; set; }

    public double StartX { get; set; }

    public double StartY { get; set; }

    public double StartZ { get; set; }

    public double EndX { get; set; }

    public double EndY { get; set; }

    public double EndZ { get; set; }

    public int LevelId { get; set; }
}

public sealed class CreateBeamResult
{
    public int Id { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;

    public double LengthMm { get; init; }
}

public sealed class CreatePointBasedElementRequest
{
    public int FamilyTypeId { get; set; }

    public double LocationX { get; set; }

    public double LocationY { get; set; }

    public double LocationZ { get; set; }

    // null -> nearest level by elevation.
    public int? LevelId { get; set; }

    // Rotation around Z (degrees); default 0.
    public double? RotationDeg { get; set; }
}

public sealed class CreatePointBasedElementResult
{
    public int Id { get; init; }

    public string TypeName { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;

    public GeomPoint LocationMm { get; init; } = new();
}

public sealed class CreateOpeningByBoundaryRequest
{
    // Host element id: roof, floor, or ceiling.
    public int HostId { get; set; }

    // Closed boundary loop in mm.
    public List<PointInput> Profile { get; set; } = [];

    // Projects the opening perpendicular onto the host face; default true.
    public bool? ProjectOntHost { get; set; }
}

public sealed class CreateOpeningByBoundaryResult
{
    public int Id { get; init; }

    public int HostId { get; init; }

    public string HostCategory { get; init; } = string.Empty;
}

// ─── Group 7 (v14): Project — Modify & Edit ────────────────────────────────
// All spatial inputs are millimetres; the gateway converts to Revit feet.
public sealed class MoveElementsRequest
{
    public List<int> ElementIds { get; set; } = [];

    public double DeltaX { get; set; }

    public double DeltaY { get; set; }

    public double DeltaZ { get; set; }
}

public sealed class MoveElementsResult
{
    public int MovedCount { get; init; }

    public List<int> ElementIds { get; init; } = [];
}

public sealed class RotateElementsRequest
{
    public List<int> ElementIds { get; set; } = [];

    public double OriginX { get; set; }

    public double OriginY { get; set; }

    public double OriginZ { get; set; }

    public double AngleDeg { get; set; }

    // Axis of rotation through the origin point: "X" | "Y" | "Z" (default Z).
    public string Axis { get; set; } = "Z";
}

public sealed class RotateElementsResult
{
    public int RotatedCount { get; init; }

    public List<int> ElementIds { get; init; } = [];
}

public sealed class MirrorElementsRequest
{
    public List<int> ElementIds { get; set; } = [];

    public double PlaneOriginX { get; set; }

    public double PlaneOriginY { get; set; }

    public double PlaneOriginZ { get; set; }

    // Normal of the mirror plane: "X" | "Y" | "Z".
    public string PlaneNormal { get; set; } = "X";

    // true (default) leaves the originals and creates mirrored copies; false mirrors in place.
    public bool CreateCopy { get; set; } = true;
}

public sealed class MirrorElementsResult
{
    public int MirroredCount { get; init; }

    // The source element ids. When createCopy=true the mirrored copies are new elements;
    // the Revit API's MirrorElements does not return their ids, so they are not listed here.
    public List<int> ElementIds { get; init; } = [];
}

public sealed class CopyElementsToLevelRequest
{
    public List<int> ElementIds { get; set; } = [];

    public int TargetLevelId { get; set; }
}

public sealed class CopyElementsToLevelResult
{
    public int CopiedCount { get; init; }

    public List<int> NewElementIds { get; init; } = [];
}

public sealed class SetElementParametersRequest
{
    public int ElementId { get; set; }

    // Parameter name -> value. Length values are mm, angles are degrees; bool for Yes/No.
    public Dictionary<string, System.Text.Json.JsonElement>? Parameters { get; set; }
}

public sealed class SetElementParametersResult
{
    public int SetCount { get; init; }

    // "name: reason" for each parameter that could not be set (not found / read-only / type).
    public List<string> Skipped { get; init; } = [];
}

public sealed class DeleteElementsRequest
{
    public List<int> ElementIds { get; set; } = [];
}

public sealed class DeleteElementsResult
{
    public int DeletedCount { get; init; }

    // All ids Revit removed, including cascade-deleted dependents.
    public List<int> DeletedIds { get; init; } = [];

    // Elements Revit refused to delete (views/sheets that throw InvalidOperationException),
    // each with the reason so the caller knows which ids require manual deletion.
    public List<DeleteElementFailure> Failed { get; init; } = [];
}

public sealed class DeleteElementFailure
{
    public int Id { get; init; }

    public string Reason { get; init; } = string.Empty;
}

// ─── Group 8 (v15): Project — Views, Sheets & Sheet Audit ──────────────────

// Shared by floor-plan and reflected-ceiling-plan creation.
public sealed class CreatePlanViewRequest
{
    public int LevelId { get; set; }

    public string? Name { get; set; }
}

public sealed class CreatePlanViewResult
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;
}

public sealed class CreateSectionViewRequest
{
    // [x, y, z] millimetres.
    public double[] BoundingBoxMin { get; set; } = [];

    public double[] BoundingBoxMax { get; set; } = [];

    public string? Name { get; set; }
}

public sealed class Create3DViewRequest
{
    public string? Name { get; set; }

    public bool IsOrthographic { get; set; }
}

// Shared by section and 3D view creation.
public sealed class CreateViewResult
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class CreateSheetRequest
{
    public string SheetNumber { get; set; } = string.Empty;

    public string SheetName { get; set; } = string.Empty;

    // null -> sheet created without a title block.
    public int? TitleBlockTypeId { get; set; }
}

public sealed class CreateSheetResult
{
    public int Id { get; init; }

    public string SheetNumber { get; init; } = string.Empty;

    public string SheetName { get; init; } = string.Empty;

    public string TitleBlockName { get; init; } = string.Empty;
}

public sealed class PlaceViewOnSheetRequest
{
    public int SheetId { get; set; }

    public int ViewId { get; set; }

    // Viewport center point on the sheet, millimetres.
    public double LocationX { get; set; }

    public double LocationY { get; set; }
}

public sealed class PlaceViewOnSheetResult
{
    public int ViewportId { get; init; }

    public string SheetNumber { get; init; } = string.Empty;

    public string ViewName { get; init; } = string.Empty;
}

public sealed class SetViewCropRegionRequest
{
    public int ViewId { get; set; }

    // Crop extents in the view plane, millimetres.
    public double MinX { get; set; }

    public double MinY { get; set; }

    public double MaxX { get; set; }

    public double MaxY { get; set; }
}

public sealed class SetViewCropRegionResult
{
    public int ViewId { get; init; }

    public BoundingBox2DData CropBoxMm { get; init; } = new();
}

public sealed class GetSheetsResult
{
    public List<SheetRecord> Sheets { get; init; } = [];

    public int TotalCount { get; init; }
}

public sealed class SheetRecord
{
    public int Id { get; init; }

    public string SheetNumber { get; init; } = string.Empty;

    public string SheetName { get; init; } = string.Empty;

    public string TitleBlock { get; init; } = string.Empty;

    public List<int> ViewportIds { get; init; } = [];
}

public sealed class GetSheetContentsRequest
{
    public int SheetId { get; set; }
}

public sealed class GetSheetContentsResult
{
    public int SheetId { get; init; }

    public string SheetNumber { get; init; } = string.Empty;

    public string SheetName { get; init; } = string.Empty;

    public Dictionary<string, string> TitleBlockParameters { get; init; } = [];

    public List<SheetViewportRecord> Viewports { get; init; } = [];

    public List<SheetTextNoteRecord> TextNotes { get; init; } = [];

    public List<SheetTagRecord> Tags { get; init; } = [];

    public List<SheetFilledRegionRecord> FilledRegions { get; init; } = [];

    public int TotalAnnotationCount { get; init; }
}

public sealed class SheetViewportRecord
{
    public int ViewId { get; init; }

    public string ViewName { get; init; } = string.Empty;

    public string ViewType { get; init; } = string.Empty;

    // Viewport center on the sheet, mm.
    public GeomPoint CenterMm { get; init; } = new();

    // The underlying view's crop box (model-plane extents), mm.
    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public BoundingBox2DData? CropBoxMm { get; init; }
}

public sealed class SheetTextNoteRecord
{
    public string Text { get; init; } = string.Empty;

    public GeomPoint PositionMm { get; init; } = new();

    public double FontSize { get; init; }
}

public sealed class SheetTagRecord
{
    public int TaggedElementId { get; init; }

    public GeomPoint PositionMm { get; init; } = new();
}

public sealed class SheetFilledRegionRecord
{
    public int Id { get; init; }

    public GeomPoint PositionMm { get; init; } = new();
}

public sealed class OpenSheetRequest
{
    public string? SheetNumber { get; set; }

    public int? SheetId { get; set; }
}

public sealed class OpenSheetResult
{
    public int Id { get; init; }

    public string SheetNumber { get; init; } = string.Empty;

    public string SheetName { get; init; } = string.Empty;
}

public sealed class GetViewContentsRequest
{
    public int ViewId { get; set; }

    public bool IncludeElements { get; set; } = true;

    public bool IncludeAnnotations { get; set; } = true;
}

public sealed class GetViewContentsResult
{
    public int ViewId { get; init; }

    public string ViewName { get; init; } = string.Empty;

    public string ViewType { get; init; } = string.Empty;

    public Dictionary<string, CategoryBucket> ElementsByCategory { get; init; } = [];

    public DimensionSummary Dimensions { get; init; } = new();

    public TagSummary Tags { get; init; } = new();

    public List<UntaggedElementRecord> UntaggedElements { get; init; } = [];

    public List<ViewTextNoteRecord> TextNotes { get; init; } = [];

    public int TotalElementCount { get; init; }
}

public sealed class CategoryBucket
{
    public int Count { get; init; }

    public List<int> Ids { get; init; } = [];
}

public sealed class DimensionSummary
{
    public int Total { get; init; }

    public int Labeled { get; init; }

    public int Unlabeled { get; init; }
}

public sealed class TagSummary
{
    public int Total { get; init; }

    public List<int> TaggedElementIds { get; init; } = [];
}

public sealed class UntaggedElementRecord
{
    public int Id { get; init; }

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;
}

public sealed class ViewTextNoteRecord
{
    public string Text { get; init; } = string.Empty;

    public GeomPoint PositionMm { get; init; } = new();
}

public sealed class CompareSheetToTemplateRequest
{
    public int SheetId { get; set; }

    // Serialized SheetTemplate (see SheetTemplate / SheetTemplateAnnotations).
    public string TemplateJson { get; set; } = string.Empty;
}

public sealed class CompareSheetToTemplateResult
{
    public int SheetId { get; init; }

    public string SheetNumber { get; init; } = string.Empty;

    public bool Passed { get; init; }

    public List<TemplateIssue> Issues { get; init; } = [];

    public int IssueCount { get; init; }
}

public sealed class TemplateIssue
{
    public string Severity { get; init; } = "error"; // "error" | "warning"

    public string Category { get; init; } = string.Empty; // titleBlock | viewports | annotations | tags | text

    public string Description { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? ElementId { get; init; }
}

// Deserialized from CompareSheetToTemplateRequest.TemplateJson.
public sealed class SheetTemplate
{
    public List<string>? RequiredTitleBlockFields { get; set; }

    public List<string>? RequiredViewTypes { get; set; }

    public int RequiredViewCount { get; set; }

    public SheetTemplateAnnotations? RequiredAnnotations { get; set; }
}

public sealed class SheetTemplateAnnotations
{
    public int MinimumDimensions { get; set; }

    public bool RequireAllElementsTagged { get; set; }

    public List<string>? RequiredTextNotes { get; set; }
}

// Shared by actions that only report success (no new element id).
public sealed class OperationResult
{
    public bool Success { get; init; }

    // Optional human-readable note (e.g. why a no-op succeeded). Omitted from JSON when null
    // so existing OperationResult responses are byte-for-byte unchanged.
    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; init; }
}

// ─── Toolbox expansion: Family Editor — Sketch & Solid Geometry (group 2) ──
public sealed class CreateBlendRequest
{
    public List<PointInput> BottomProfile { get; set; } = [];

    public List<PointInput> TopProfile { get; set; } = [];

    public double Depth { get; set; }

    public bool IsSolid { get; set; } = true;

    public string? WorkPlaneName { get; set; }
}

public sealed class CreateRevolveRequest
{
    public List<PointInput> Profile { get; set; } = [];

    public string AxisReferenceName { get; set; } = string.Empty;

    public double StartAngleDeg { get; set; }

    public double EndAngleDeg { get; set; }

    public bool IsSolid { get; set; } = true;

    public string? WorkPlaneName { get; set; }
}

public sealed class CreateSweepRequest
{
    // 3D polyline path points (mm). Assumed planar (NewSweep needs a path plane).
    public List<PointInput> Path { get; set; } = [];

    // 2D profile (mm), sketched in its own XY frame; placed normal to the path start.
    public List<PointInput> Profile { get; set; } = [];

    public bool IsSolid { get; set; } = true;

    public string? WorkPlaneName { get; set; }
}

public sealed class CreateSweptBlendRequest
{
    // NewSweptBlend takes a SINGLE-curve path; only the first segment (Path[0]->Path[1]) is used.
    public List<PointInput> Path { get; set; } = [];

    public List<PointInput> StartProfile { get; set; } = [];

    public List<PointInput> EndProfile { get; set; } = [];

    public bool IsSolid { get; set; } = true;

    public string? WorkPlaneName { get; set; }
}

public sealed class SetGeometrySolidVoidRequest
{
    public int ElementId { get; set; }

    public bool IsSolid { get; set; } = true;
}

// Reusable {start,end} line in mm.
public sealed class LineInput
{
    public PointInput Start { get; set; } = new();

    public PointInput End { get; set; } = new();
}

// ─── Toolbox expansion: Family Editor — Dimensions & Constraints (group 3) ─
public sealed class CreateDimensionRequest
{
    // Exactly two element ids to dimension between (currently reference planes).
    public List<int> ReferenceIds { get; set; } = [];

    public LineInput Line { get; set; } = new();

    // Optional: label the dimension with this existing family parameter.
    public string? LabelParameterName { get; set; }
}

public sealed class SetDimensionLabelRequest
{
    public int DimensionId { get; set; }

    public string ParameterName { get; set; } = string.Empty;
}

public sealed class LockConstraintRequest
{
    public int ConstraintId { get; set; }

    public bool Locked { get; set; }
}

// ─── Toolbox expansion: Family Editor — Parameters & Types (group 4) ───────
public sealed class AddFamilyParameterRequest
{
    public string Name { get; set; } = string.Empty;

    public string ParameterType { get; set; } = "Text";

    public string ParameterGroup { get; set; } = "PG_OTHER";

    public bool IsInstance { get; set; }
}

public sealed class AddFamilyParameterResult
{
    public string ParameterId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool Success { get; init; }
}

public sealed class SetFamilyParameterValueRequest
{
    public string ParameterName { get; set; } = string.Empty;

    // string | number | boolean — interpreted against the parameter's storage type.
    public System.Text.Json.JsonElement? Value { get; set; }

    public string? TypeName { get; set; }
}

public sealed class AddFamilyTypeRequest
{
    public string TypeName { get; set; } = string.Empty;

    public string? CloneFromType { get; set; }

    public Dictionary<string, System.Text.Json.JsonElement>? Parameters { get; set; }
}

public sealed class RenameFamilyTypeRequest
{
    public string OldName { get; set; } = string.Empty;

    public string NewName { get; set; } = string.Empty;
}

public sealed class DeleteFamilyTypeRequest
{
    public string TypeName { get; set; } = string.Empty;
}

public sealed class SetFormulaRequest
{
    public string ParameterName { get; set; } = string.Empty;

    public string Formula { get; set; } = string.Empty;
}

// ─── Toolbox expansion: Family Editor — Document Management (group 4) ──────
public sealed class SaveFamilyAsRequest
{
    public string FilePath { get; set; } = string.Empty;
}

public sealed class SaveFamilyResult
{
    public string FilePath { get; init; } = string.Empty;

    public bool Success { get; init; }
}

public sealed class LoadFamilyIntoProjectRequest
{
    public bool OverwriteParameterValues { get; set; } = true;
}

public sealed class LoadFamilyIntoProjectResult
{
    public int FamilyId { get; init; }

    public bool Success { get; init; }
}

public sealed class FamilyDocumentInfoResult
{
    public string FamilyName { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public bool IsConceptual { get; init; }

    public bool IsFaceBased { get; init; }

    public string HostType { get; init; } = string.Empty;

    public int TypeCount { get; init; }

    public string FilePath { get; init; } = string.Empty;
}

// ─── Toolbox expansion: Group 5 — Geometry Read & In-Place Extraction ──────
// Read-only tools routed under /api/project/*. Every handler resolves its document from
// app.ActiveUIDocument.Document (the live active document) so it follows Revit into an
// Edit-In-Place session instead of pinning to the project root. All emitted coordinates,
// lengths, radii are millimetres (Revit internal feet * 304.8); volumes are mm³.

// Output point in millimetres. Named GeomPoint (not Point3D) to avoid any clash with
// System.Windows.Media.Media3D.Point3D pulled in by UseWPF.
public sealed class GeomPoint
{
    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; }
}

public sealed class GetActiveContextResult
{
    // "project" | "family_editor" | "edit_in_place"
    public string Context { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public int? ElementId { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Category { get; init; }

    // Diagnostic note: surfaces detection caveats (e.g. in-place-edit detection limits).
    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; init; }
}

public sealed class GetElementGeometryRequest
{
    // If null and UseSelection is false, the request fails with a clear error.
    public int? ElementId { get; set; }

    public bool IncludeSketches { get; set; } = true;

    public bool IncludeSolids { get; set; } = true;

    public bool UseSelection { get; set; }
}

// A typed curve: line/arc are returned with clean analytic data so reconstruction produces
// crisp families rather than polyline approximations. Splines fall back to sampled points.
public sealed class CurveData
{
    public string Type { get; init; } = "line"; // line | arc | spline

    public GeomPoint Start { get; init; } = new();

    public GeomPoint End { get; init; } = new();

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? Center { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public double? Radius { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public double? StartAngleDeg { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public double? EndAngleDeg { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public List<GeomPoint>? ControlPoints { get; init; }
}

public sealed class CurveLoopData
{
    public List<CurveData> Curves { get; init; } = [];
}

public sealed class SketchProfileData
{
    public int ProfileIndex { get; init; }

    public List<CurveLoopData> Loops { get; init; } = [];
}

public sealed class FaceData
{
    public int FaceIndex { get; init; }

    public GeomPoint Normal { get; init; } = new();

    public List<CurveLoopData> EdgeLoops { get; init; } = [];
}

public sealed class SolidData
{
    public int SolidIndex { get; init; }

    public double Volume { get; init; } // mm³

    public List<FaceData> Faces { get; init; } = [];
}

public sealed class BoundingBoxData
{
    public double MinX { get; init; }

    public double MinY { get; init; }

    public double MinZ { get; init; }

    public double MaxX { get; init; }

    public double MaxY { get; init; }

    public double MaxZ { get; init; }
}

public sealed class GetElementGeometryResult
{
    public int ElementId { get; init; }

    // "project" | "edit_in_place"
    public string Context { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public BoundingBoxData? BoundingBox { get; init; }

    public List<SketchProfileData> Sketches { get; init; } = [];

    public List<SolidData> Solids { get; init; } = [];

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; init; }
}

public sealed class GetInplaceElementsRequest
{
    // OST category token filter (e.g. "OST_GenericModel"); null/empty returns all.
    public string? Category { get; set; }

    public int Limit { get; set; } = 100;
}

public sealed class InplaceElementRecord
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public GeomPoint Location { get; init; } = new(); // bounding-box center, mm
}

public sealed class GetInplaceElementsResult
{
    public List<InplaceElementRecord> Elements { get; init; } = [];

    public int TotalCount { get; init; }
}

// ─── Group 5: profile reconstruction (raw geometry -> create_extrusion input) ──
public sealed class ReconstructProfileRequest
{
    public int ElementId { get; set; }

    // Endpoint-matching tolerance when chaining edge segments into a loop.
    public double EpsilonMm { get; set; } = 0.1;

    // Translate the projected profile so its bounding-box min sits at (0,0).
    public bool NormalizeToOrigin { get; set; } = true;

    // If true, ignore ElementId and use the first element in the active selection.
    public bool UseSelection { get; set; }
}

public sealed class Profile2DPoint
{
    public double X { get; init; }

    public double Y { get; init; }
}

public sealed class BoundingBox2DData
{
    public double MinX { get; init; }

    public double MinY { get; init; }

    public double MaxX { get; init; }

    public double MaxY { get; init; }
}

// One closed loop of the cap face: the outer boundary or an interior void.
public sealed class ReconstructedLoop
{
    public bool IsOuter { get; init; }

    public int PointCount { get; init; }

    // Ordered, mm, origin-normalised, counter-clockwise; auto-closes (last connects to first).
    public List<Profile2DPoint> Profile { get; init; } = [];
}

public sealed class ReconstructProfileResult
{
    public int ElementId { get; init; }

    public double ExtrusionDepth { get; init; } // mm

    public string ExtrusionAxis { get; init; } = string.Empty; // "X" | "Y" | "Z"

    // Outer boundary plus any interior voids. Outer loop has the largest area.
    public List<ReconstructedLoop> Loops { get; init; } = [];

    public BoundingBox2DData BoundingBox2D { get; init; } = new();

    // false when any loop could not be closed; that loop's Profile holds the partial chain.
    public bool Closed { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; init; }
}

// ─── Group 11: read-only project query tools ───────────────────────────────
public sealed class GetLevelsResult
{
    public List<LevelRecord> Levels { get; init; } = [];
}

public sealed class LevelRecord
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public double ElevationMm { get; init; }

    public bool IsGroundFloor { get; init; }
}

public sealed class GetGridsResult
{
    public List<GridRecord> Grids { get; init; } = [];
}

public sealed class GridRecord
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? Start { get; init; }

    [System.Text.Json.Serialization.JsonIgnore(
        Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? End { get; init; }
}

public sealed class GetElementTypesRequest
{
    public string? Category { get; set; }

    public int Limit { get; set; } = 100;
}

public sealed class GetElementTypesResult
{
    public List<ElementTypeRecord> Types { get; init; } = [];

    public int TotalCount { get; init; }
}

public sealed class ElementTypeRecord
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}

public sealed class QueryElementsRequest
{
    public string? Category { get; set; }

    public int? LevelId { get; set; }

    public double? BboxMinX { get; set; }

    public double? BboxMinY { get; set; }

    public double? BboxMinZ { get; set; }

    public double? BboxMaxX { get; set; }

    public double? BboxMaxY { get; set; }

    public double? BboxMaxZ { get; set; }

    public int Limit { get; set; } = 50;
}

public sealed class QueryElementsResult
{
    public List<QueriedElementRecord> Elements { get; init; } = [];

    public int TotalCount { get; init; }
}

public sealed class QueriedElementRecord
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public GeomPoint Location { get; init; } = new(); // bounding-box center, mm
}

public sealed class GetElementByIdRequest
{
    public int ElementId { get; set; }

    public bool IncludeParameters { get; set; } = true;
}

public sealed class GetElementByIdResult
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public GeomPoint Location { get; init; } = new(); // bounding-box center, mm

    public Dictionary<string, string> Parameters { get; init; } = [];
}

// ─── Model Snapshot Export (v18) ────────────────────────────────────────────
// Requests carry a caller-supplied absolute outputPath; the gateway serializes the
// full snapshot to that file directly. Only ExportSummaryResult crosses the MCP boundary.
public sealed class ExportInstancesRequest
{
    public List<string>? Categories { get; set; }

    public string OutputPath { get; set; } = string.Empty;
}

public sealed class ExportTypeParametersRequest
{
    public List<string>? Categories { get; set; }

    public string OutputPath { get; set; } = string.Empty;
}

public sealed class ExportParameterBindingsRequest
{
    public string OutputPath { get; set; } = string.Empty;
}

public sealed class ExportMaterialsRequest
{
    public string OutputPath { get; set; } = string.Empty;
}

public sealed class ExportViewsRequest
{
    public string OutputPath { get; set; } = string.Empty;
}

public sealed class ExportSummaryResult
{
    public string OutputPath { get; init; } = string.Empty;

    public int ElementCount { get; init; }

    public long ByteSize { get; init; }

    public int ErrorCount { get; init; }
}

// ─── v20: Project — Instance Import (Revit 2024 rebuild driver) ─────────────
// Reads instances.json (and sibling type/param datasets) C#-side and rebuilds
// placed instances from the extracted snapshot. NO element data crosses the MCP
// boundary — only ImportSummaryResult returns; a detailed per-element report is
// written to disk next to the remap file. All spatial values in the source
// files are millimetres (the gateway converts to Revit internal feet).
public sealed class ImportInstancesRequest
{
    // Absolute path to instances.json (the export_instances snapshot).
    public string InstancesPath { get; set; } = string.Empty;

    // Optional typemap JSON: [{ srcFamily, srcType, tgtFamilyFile?, tgtFamily,
    // tgtType, createTypeIfMissing, typeParamOverrides? }].
    public string? TypeMapPath { get; set; }

    // Optional sibling datasets. When omitted, defaults to files named
    // levels_grids.json / family_types.json / shared_project_parameters.json in
    // the same directory as instancesPath.
    public string? LevelsGridsPath { get; set; }

    public string? FamilyTypesPath { get; set; }

    public string? SharedParamsPath { get; set; }

    // External remap file (union across passes). Written C#-side; never stamped
    // onto elements.
    public string RemapOutPath { get; set; } = string.Empty;

    // Detailed per-element report path. Defaults to a sibling of remapOutPath.
    public string? ReportPath { get; set; }

    // "dryrun" (resolve only, no transaction) or "execute".
    public string Mode { get; set; } = "dryrun";

    // Optional OST_* category filter; when null/empty, all categories in the file.
    public List<string>? Categories { get; set; }

    // 1 = level-hosted point records; 2 = instance-hosted records (reveals/voids).
    public int HostPass { get; set; } = 1;

    // Resume support: first record index (post-filter order is file order) to process.
    public int? ResumeFromIndex { get; set; }

    // Optional cap on records processed in this call (0/null = no cap). Lets the
    // agent drive predictable per-call durations and resume via NextIndex.
    public int? MaxElements { get; set; }
}

// ─── v20.1: batch datum creation from levels_grids.json (rebuild reference frame) ─
public sealed class CreateDatumsRequest
{
    // Absolute path to levels_grids.json (the export dataset).
    public string LevelsGridsPath { get; set; } = string.Empty;

    // Skip levels/grids whose name already exists (default true).
    public bool? SkipExisting { get; set; }

    public bool? CreateLevels { get; set; }

    public bool? CreateGrids { get; set; }
}

public sealed class CreateDatumsResult
{
    public int LevelsCreated { get; init; }

    public int LevelsSkipped { get; init; }

    public int GridsCreated { get; init; }

    public int GridsSkipped { get; init; }

    public List<string> Errors { get; init; } = new();
}

public sealed class ImportSummaryResult
{
    public string Mode { get; init; } = string.Empty;

    public int HostPass { get; init; }

    public int Attempted { get; init; }

    public int Placed { get; init; }

    public int Skipped { get; init; }

    public int Failed { get; init; }

    // Distinct unresolved (family,type) pairs encountered.
    public int UnresolvedTypeCount { get; init; }

    // Host cross-check disagreements (pass 2).
    public int HostCrossCheckFailures { get; init; }

    public string RemapOutPath { get; init; } = string.Empty;

    public string ReportPath { get; init; } = string.Empty;

    // Last record index committed (execute) or examined (dryrun); -1 if none.
    public int LastCommittedIndex { get; init; }

    // Index to pass as resumeFromIndex on the next call; equals total when done.
    public int NextIndex { get; init; }

    // True when the whole (filtered) record set was processed this call.
    public bool Done { get; init; }
}
