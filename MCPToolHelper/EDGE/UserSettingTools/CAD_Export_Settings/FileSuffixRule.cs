// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.FileSuffixRule
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Controls;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class FileSuffixRule : ValidationRule
{
  public bool EmptyAllowed { get; set; }

  public FileSuffixRule() => this.EmptyAllowed = true;

  public override ValidationResult Validate(object value, CultureInfo cultureInfo)
  {
    if (!(value is string source))
      return new ValidationResult(false, (object) "");
    if (!this.EmptyAllowed && string.IsNullOrWhiteSpace(source))
      return new ValidationResult(false, (object) "Required Field");
    foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
    {
      if (source.Contains<char>(invalidFileNameChar))
        return new ValidationResult(false, (object) "Illegal Characters");
    }
    return ValidationResult.ValidResult;
  }
}
