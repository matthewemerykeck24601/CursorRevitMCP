// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.InitialPresentation.ExclamationConverters
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.ApplicationRibbon;
using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.InitialPresentation;

public class ExclamationConverters : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return (object) AppRibbonSetup.GetBitmapImage("exclamationForMarkVerification.png");
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException("The method or operation is not implemented.");
  }
}
