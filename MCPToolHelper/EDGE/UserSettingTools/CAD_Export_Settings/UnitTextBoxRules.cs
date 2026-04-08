// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.UnitTextBoxRules
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Globalization;
using System.Windows.Controls;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class UnitTextBoxRules : ValidationRule
{
  public override ValidationResult Validate(object value, CultureInfo cultureInfo)
  {
    if (UnitValidationParameters.units == null)
      return ValidationResult.ValidResult;
    if (!(value is string stringToParse))
      return new ValidationResult(false, (object) "");
    if (string.IsNullOrWhiteSpace(stringToParse))
      return ValidationResult.ValidResult;
    if (UnitValidationParameters.imperialUnits)
    {
      ValueParsingOptions valueParsingOptions1 = new ValueParsingOptions();
      FormatOptions formatOptions1 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.InchDoubleQuote);
      valueParsingOptions1.SetFormatOptions(formatOptions1);
      ValueParsingOptions valueParsingOptions2 = new ValueParsingOptions();
      FormatOptions formatOptions2 = new FormatOptions(UnitTypeId.Inches, SymbolTypeId.In);
      valueParsingOptions2.SetFormatOptions(formatOptions2);
      double num;
      if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions1, out num))
        return num > 0.0 ? ValidationResult.ValidResult : new ValidationResult(false, (object) "");
      if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions2, out num))
        return num > 0.0 ? ValidationResult.ValidResult : new ValidationResult(false, (object) "");
    }
    else
    {
      ValueParsingOptions valueParsingOptions = new ValueParsingOptions();
      FormatOptions formatOptions = new FormatOptions(UnitTypeId.Millimeters, SymbolTypeId.Mm);
      valueParsingOptions.SetFormatOptions(formatOptions);
      double num;
      if (UnitFormatUtils.TryParse(UnitValidationParameters.units, SpecTypeId.Length, stringToParse, valueParsingOptions, out num))
        return num > 0.0 ? ValidationResult.ValidResult : new ValidationResult(false, (object) "");
    }
    return new ValidationResult(false, (object) "");
  }
}
