// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.FontColorConverter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class FontColorConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return (bool) value ? (object) new SolidColorBrush(Colors.Black) : (object) new SolidColorBrush(Colors.DimGray);
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException("The method or operation is not implemented.");
  }
}
