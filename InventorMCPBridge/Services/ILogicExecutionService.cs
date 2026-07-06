namespace InventorMCPBridge.Services;

public interface ILogicExecutionService
{
    object GetActiveDocumentInfo();

    object RunRule(string ruleName, string runMode);

    object ShowForm(string formName);

    object WriteRule(string ruleName, string code, bool overwrite);

    List<ParameterRecord> GetParameters(GetParametersRequest request);

    object SetParameters(SetParametersRequest request);

    object ListDrawingViews();

    object AddViewOverallDimensions(AddDrawingViewOverallDimensionsRequest request);

    object AddAllViewsOverallDimensions(AddAllDrawingViewsOverallDimensionsRequest request);

    object AddDiameterDimension(AddDiameterDimensionRequest request);

    object AddLinearDimension(AddLinearDimensionRequest request);

    DocumentSettingsRecord GetDocumentSettings();

    object SetLengthUnits(SetLengthUnitsRequest request);

    object SetAngleUnits(SetAngleUnitsRequest request);

    object SetMassUnits(SetMassUnitsRequest request);

    object SetDimensionPrecision(SetDimensionPrecisionRequest request);

    object SetModelingDisplay(SetModelingDisplayRequest request);
}
