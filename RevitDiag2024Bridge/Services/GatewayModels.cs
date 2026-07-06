using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RevitDiag2024Bridge.Services;

// Wire envelope returned by every route: { success, message, data } — matches the 2025
// bridge so the shared TypeScript bridgeClient parses both identically.
public sealed class GatewayEnvelope<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }
}

// Output point in millimetres (Revit internal feet * 304.8).
public sealed class GeomPoint
{
    public double X { get; init; }

    public double Y { get; init; }

    public double Z { get; init; }
}

// ─── get_document_metadata ──────────────────────────────────────────────────
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

    public int LinkCount { get; init; }

    public string Notes { get; init; } = string.Empty;

    public ProjectInformationRecord ProjectInformation { get; init; } = new();
}

public sealed class ProjectInformationRecord
{
    public string Name { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string ClientName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

// ─── get_active_document_context ────────────────────────────────────────────
public sealed class GetActiveContextResult
{
    // "project" | "family"
    public string Context { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    public bool IsFamilyDocument { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Category { get; init; }
}

// ─── get_element_by_id / get_element_metadata ───────────────────────────────
public sealed class GetElementByIdRequest
{
    public long ElementId { get; set; }

    public bool IncludeParameters { get; set; } = true;
}

public sealed class ElementMetadataRequest
{
    public List<long>? ElementIds { get; set; }

    public bool UseSelection { get; set; }

    public bool IncludeParameters { get; set; } = true;
}

public sealed class ElementDetailRecord
{
    public bool Found { get; init; }

    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public GeomPoint Location { get; init; } = new();

    public Dictionary<string, string> Parameters { get; init; } = new();

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Note { get; init; }
}

public sealed class ElementMetadataResult
{
    public List<ElementDetailRecord> Elements { get; init; } = new();
}

// ─── get_element_types ──────────────────────────────────────────────────────
public sealed class GetElementTypesRequest
{
    public string? Category { get; set; }

    public int Limit { get; set; } = 100;
}

public sealed class GetElementTypesResult
{
    public List<ElementTypeRecord> Types { get; init; } = new();

    public int TotalCount { get; init; }
}

public sealed class ElementTypeRecord
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}

// ─── get_levels ─────────────────────────────────────────────────────────────
public sealed class GetLevelsResult
{
    public List<LevelRecord> Levels { get; init; } = new();
}

public sealed class LevelRecord
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public double ElevationMm { get; init; }

    public bool IsGroundFloor { get; init; }
}

// ─── get_grids ──────────────────────────────────────────────────────────────
public sealed class GetGridsResult
{
    public List<GridRecord> Grids { get; init; } = new();
}

public sealed class GridRecord
{
    public long Id { get; init; }

    public string Name { get; init; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? Start { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public GeomPoint? End { get; init; }
}

// ─── query_elements ─────────────────────────────────────────────────────────
public sealed class QueryElementsRequest
{
    public string? Category { get; set; }

    public string? FamilyName { get; set; }

    public long? LevelId { get; set; }

    public List<ParameterFilter>? ParameterFilters { get; set; }

    public int Limit { get; set; } = 100;
}

// A single AND'd parameter predicate. Name matching is case-insensitive and resolves
// against any readable instance OR type parameter; missing parameters coerce to "".
public sealed class ParameterFilter
{
    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    // equals | contains | startsWith | notEquals (case-insensitive). Defaults to equals.
    public string Operator { get; set; } = "equals";
}

public sealed class QueryElementsResult
{
    public List<QueriedElementRecord> Elements { get; init; } = new();

    // Number of elements matching every filter (may exceed limit / Elements.Count).
    public int TotalCount { get; init; }
}

public sealed class QueriedElementRecord
{
    public long ElementId { get; init; }

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public string Level { get; init; } = string.Empty;

    // Every readable instance parameter, string-coerced (name -> value).
    public Dictionary<string, string> Parameters { get; init; } = new();
}

// ─── get_schedules ──────────────────────────────────────────────────────────
public sealed class GetSchedulesResult
{
    public List<ScheduleSummaryRecord> Schedules { get; init; } = new();

    public int TotalCount { get; init; }
}

public sealed class ScheduleSummaryRecord
{
    public long ScheduleId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public int RowCount { get; init; }
}

// ─── get_schedule_data ──────────────────────────────────────────────────────
public sealed class GetScheduleDataRequest
{
    // Provide either ScheduleId (positive) or ScheduleName. ScheduleName takes precedence and
    // resolves to the first non-template ViewSchedule whose Name matches (case-insensitive).
    public long ScheduleId { get; set; }

    public string? ScheduleName { get; set; }

    // Row cap; defaults to 1000, clamped to 5000.
    public int? MaxRows { get; set; }
}

public sealed class GetScheduleDataResult
{
    public long ScheduleId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public List<string> Headers { get; init; } = new();

    public List<Dictionary<string, string>> Rows { get; init; } = new();

    public int RowCount { get; init; }

    public int ColumnCount { get; init; }

    public bool Truncated { get; init; }

    public int TotalRows { get; init; }
}

// ─── get_current_view_elements ──────────────────────────────────────────────
public sealed class CurrentViewElementsRequest
{
    public string[]? ModelCategoryList { get; set; }

    public int? Limit { get; set; }
}

public sealed class CurrentViewElementRecord
{
    public long ElementId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string FamilyName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public string LevelName { get; init; } = string.Empty;
}

// ─── diag/get_warnings ──────────────────────────────────────────────────────
public sealed class GetWarningsResult
{
    public List<WarningRecord> Warnings { get; init; } = new();

    public int TotalCount { get; init; }

    public bool Truncated { get; init; }
}

public sealed class WarningRecord
{
    public string WarningText { get; init; } = string.Empty;

    public string Severity { get; init; } = string.Empty;

    public List<long> FailingElementIds { get; init; } = new();

    public string ResolutionDescription { get; init; } = string.Empty;
}

// ─── diag/get_missing_links ─────────────────────────────────────────────────
public sealed class GetMissingLinksResult
{
    public List<LinkRecord> Links { get; init; } = new();

    public int TotalCount { get; init; }
}

public sealed class LinkRecord
{
    public long ElementId { get; init; }

    public string LinkName { get; init; } = string.Empty;

    public string LinkPath { get; init; } = string.Empty;

    public bool IsLoaded { get; init; }

    public string LinkedFileStatus { get; init; } = string.Empty;
}

// ─── diag/get_journal_tail ──────────────────────────────────────────────────
public sealed class JournalTailRequest
{
    public int Lines { get; set; } = 50;
}

public sealed class JournalTailResult
{
    public string JournalPath { get; init; } = string.Empty;

    public string LastWriteTimeUtc { get; init; } = string.Empty;

    public int LineCount { get; init; }

    public List<string> Lines { get; init; } = new();
}

// ─── diag/get_corrupt_elements ──────────────────────────────────────────────
public sealed class GetCorruptElementsResult
{
    public int ScannedCount { get; init; }

    public int AccessibleCount { get; init; }

    public List<CorruptElementRecord> Failed { get; init; } = new();

    public bool Truncated { get; init; }

    public string Note { get; init; } = string.Empty;
}

public sealed class CorruptElementRecord
{
    public long ElementId { get; init; }

    public string Error { get; init; } = string.Empty;
}
