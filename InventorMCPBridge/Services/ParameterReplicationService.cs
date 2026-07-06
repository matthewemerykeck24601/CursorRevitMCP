namespace InventorMCPBridge.Services;

public sealed class ParameterReplicationService
{
    public SetParametersRequest BuildSetRequestFromRevitPayload(ReplicateParametersRequest request)
    {
        var mapped = request.Payload.Parameters.Select(p => new ParameterRecord
        {
            Name = p.Name,
            Units = MapUnits(p.UnitType),
            Expression = string.IsNullOrWhiteSpace(p.Formula)
                ? p.Value?.ToString()
                : p.Formula,
            IsKey = p.IsInstance.HasValue ? !p.IsInstance.Value : null,
            Comment = $"Imported from Revit {request.Payload.FamilyName ?? "unknown"}",
        }).ToList();

        return new SetParametersRequest
        {
            Parameters = mapped,
            RunRuleAfterSet = request.RunRuleAfterSet,
        };
    }

    private static string? MapUnits(string? unitType)
    {
        if (string.IsNullOrWhiteSpace(unitType))
        {
            return null;
        }

        var token = unitType.ToLowerInvariant();
        if (token.Contains("length"))
        {
            return "mm";
        }

        if (token.Contains("area"))
        {
            return "mm^2";
        }

        if (token.Contains("volume"))
        {
            return "mm^3";
        }

        if (token.Contains("angle"))
        {
            return "deg";
        }

        return null;
    }
}
