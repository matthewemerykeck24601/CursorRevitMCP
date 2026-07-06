namespace InventorMCPBridge.Services;

public sealed class GatewayEnvelope<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }
}

public sealed class RunILogicRuleRequest
{
    public string RuleName { get; set; } = string.Empty;

    public string RunMode { get; set; } = "activeDocument";
}

public sealed class ShowILogicFormRequest
{
    public string FormName { get; set; } = string.Empty;
}

public sealed class WriteILogicRuleRequest
{
    public string RuleName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool Overwrite { get; set; }
}

public sealed class ParameterRecord
{
    public string Name { get; set; } = string.Empty;

    public string? Units { get; set; }

    public string? Expression { get; set; }

    public object? Value { get; set; }

    public bool? IsKey { get; set; }

    public string? Comment { get; set; }
}

public sealed class GetParametersRequest
{
    public bool IncludeModelParameters { get; set; } = false;

    public bool IncludeUserParameters { get; set; } = true;
}

public sealed class SetParametersRequest
{
    public List<ParameterRecord> Parameters { get; set; } = [];

    public string? RunRuleAfterSet { get; set; }
}

public sealed class RevitFamilyParameterPayload
{
    public string Source { get; set; } = "unknown";

    public string? FamilyName { get; set; }

    public string? TypeName { get; set; }

    public List<RevitFamilyParameterRecord> Parameters { get; set; } = [];
}

public sealed class RevitFamilyParameterRecord
{
    public string Name { get; set; } = string.Empty;

    public string? DataType { get; set; }

    public string? UnitType { get; set; }

    public string? Group { get; set; }

    public string? Formula { get; set; }

    public bool? IsInstance { get; set; }

    public object? Value { get; set; }
}

public sealed class ReplicateParametersRequest
{
    public RevitFamilyParameterPayload Payload { get; set; } = new();

    public string? RunRuleAfterSet { get; set; }
}

public sealed class PreparePublishRequest
{
    public string? RunLabel { get; set; }
}

public sealed class PreparePublishResult
{
    public string DocumentName { get; set; } = string.Empty;

    public PublishValidation Validation { get; set; } = new();

    public List<string> Checklist { get; set; } = [];
}

public sealed class PublishValidation
{
    public bool HasParameters { get; set; }

    public bool HasModelStateHint { get; set; }

    public bool HasRunLabel { get; set; }
}

public sealed class AddDrawingViewOverallDimensionsRequest
{
    public int? ViewIndex { get; set; }

    public string? ViewName { get; set; }

    public double OffsetInSheet { get; set; } = 1.0;

    public bool AddWidth { get; set; } = true;

    public bool AddHeight { get; set; } = true;
}

public sealed class AddAllDrawingViewsOverallDimensionsRequest
{
    public double OffsetInSheet { get; set; } = 1.0;

    public bool AddWidth { get; set; } = true;

    public bool AddHeight { get; set; } = true;
}

public sealed class AddDiameterDimensionRequest
{
    public int? ViewIndex { get; set; }

    public string? ViewName { get; set; }

    public int CircleIndex { get; set; } = 0;

    public double? TextOffsetX { get; set; }

    public double? TextOffsetY { get; set; }
}

public sealed class AddLinearDimensionRequest
{
    public int? ViewIndex { get; set; }

    public string? ViewName { get; set; }

    public double FromX { get; set; }

    public double FromY { get; set; }

    public double ToX { get; set; }

    public double ToY { get; set; }

    public string DimensionType { get; set; } = "horizontal"; // horizontal | vertical | aligned

    /// <summary>
    /// If specified, dimension from the center of this circle index (preferred over FromX/FromY).
    /// </summary>
    public int? FromCircleIndex { get; set; }

    /// <summary>
    /// If specified, dimension to the center of this circle index (preferred over ToX/ToY).
    /// </summary>
    public int? ToCircleIndex { get; set; }

    /// <summary>
    /// Optional X offset for dimension text placement (relative to view.Left).
    /// Use negative values to place the dimension string to the left of the view.
    /// </summary>
    public double? TextOffsetX { get; set; }

    /// <summary>
    /// Optional Y offset for dimension text placement (relative to view.Top).
    /// </summary>
    public double? TextOffsetY { get; set; }
}

public sealed class DocumentSettingsRecord
{
    public string? LengthUnits { get; set; }
    public string? AngleUnits { get; set; }
    public string? MassUnits { get; set; }
    public int? LinearPrecision { get; set; }
    public int? AngularPrecision { get; set; }
    public string? ModelingDisplay { get; set; } // Shaded, Wireframe, HiddenLine, etc.
    public bool? ShowSketches { get; set; }
    public bool? ShowWorkFeatures { get; set; }
}

public sealed class SetLengthUnitsRequest
{
    public string Units { get; set; } = string.Empty; // "in", "mm", "cm", "m", "ft"
}

public sealed class SetAngleUnitsRequest
{
    public string Units { get; set; } = string.Empty; // "deg", "rad"
}

public sealed class SetMassUnitsRequest
{
    public string Units { get; set; } = string.Empty; // "kg", "lb", "g", "slug"
}

public sealed class SetDimensionPrecisionRequest
{
    public int? LinearPrecision { get; set; }
    public int? AngularPrecision { get; set; }
}

public sealed class SetModelingDisplayRequest
{
    public string DisplayMode { get; set; } = string.Empty; // "Shaded", "Wireframe", "HiddenLine"
    public bool? ShowSketches { get; set; }
    public bool? ShowWorkFeatures { get; set; }
}
