// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.ImagePathConverter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.ApplicationRibbon;
using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class ImagePathConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    switch ((string) value)
    {
      case "pass":
        return (object) AppRibbonSetup.GetBitmapImage("checkForMarkVerification.png");
      case "fail":
        return (object) AppRibbonSetup.GetBitmapImage("crossForMarkVerification.png");
      default:
        return (object) AppRibbonSetup.GetBitmapImage("neutralForMarkVerificationExisting.png");
    }
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException("The method or operation is not implemented.");
  }
}
