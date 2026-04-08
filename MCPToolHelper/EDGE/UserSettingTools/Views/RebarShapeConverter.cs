// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.RebarShapeConverter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.ApplicationRibbon;
using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class RebarShapeConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    switch ((string) value)
    {
      case "S1":
        return (object) AppRibbonSetup.GetBitmapImage("S1.png");
      case "S2":
        return (object) AppRibbonSetup.GetBitmapImage("S2.png");
      case "S3":
        return (object) AppRibbonSetup.GetBitmapImage("S3.png");
      case "S4":
        return (object) AppRibbonSetup.GetBitmapImage("S4.png");
      case "S5":
        return (object) AppRibbonSetup.GetBitmapImage("S5.png");
      case "S6":
        return (object) AppRibbonSetup.GetBitmapImage("S6.png");
      case "S11":
        return (object) AppRibbonSetup.GetBitmapImage("S11.png");
      case "T1":
        return (object) AppRibbonSetup.GetBitmapImage("T1.png");
      case "T2":
        return (object) AppRibbonSetup.GetBitmapImage("T2.png");
      case "T3":
        return (object) AppRibbonSetup.GetBitmapImage("T3.png");
      case "T6":
        return (object) AppRibbonSetup.GetBitmapImage("T6.png");
      case "T7":
        return (object) AppRibbonSetup.GetBitmapImage("T7.png");
      case "T8":
        return (object) AppRibbonSetup.GetBitmapImage("T8.png");
      case "T9":
        return (object) AppRibbonSetup.GetBitmapImage("T9.png");
      case "1":
        return (object) AppRibbonSetup.GetBitmapImage("1.png");
      case "1A":
        return (object) AppRibbonSetup.GetBitmapImage("1A.png");
      case "2":
        return (object) AppRibbonSetup.GetBitmapImage("2.png");
      case "3":
        return (object) AppRibbonSetup.GetBitmapImage("3.png");
      case "3A":
        return (object) AppRibbonSetup.GetBitmapImage("3A.png");
      case "4":
        return (object) AppRibbonSetup.GetBitmapImage("4.png");
      case "4A":
        return (object) AppRibbonSetup.GetBitmapImage("4A.png");
      case "5":
        return (object) AppRibbonSetup.GetBitmapImage("5.png");
      case "6":
        return (object) AppRibbonSetup.GetBitmapImage("6.png");
      case "7":
        return (object) AppRibbonSetup.GetBitmapImage("7.png");
      case "8":
        return (object) AppRibbonSetup.GetBitmapImage("8.png");
      case "9":
        return (object) AppRibbonSetup.GetBitmapImage("9.png");
      case "10":
        return (object) AppRibbonSetup.GetBitmapImage("10.png");
      case "11":
        return (object) AppRibbonSetup.GetBitmapImage("11.png");
      case "12":
        return (object) AppRibbonSetup.GetBitmapImage("12.png");
      case "12A":
        return (object) AppRibbonSetup.GetBitmapImage("12A.png");
      case "12B":
        return (object) AppRibbonSetup.GetBitmapImage("12B.png");
      case "13":
        return (object) AppRibbonSetup.GetBitmapImage("13.png");
      case "14":
        return (object) AppRibbonSetup.GetBitmapImage("14.png");
      case "14A":
        return (object) AppRibbonSetup.GetBitmapImage("14A.png");
      case "14B":
        return (object) AppRibbonSetup.GetBitmapImage("14B.png");
      case "16":
        return (object) AppRibbonSetup.GetBitmapImage("16.png");
      case "16A":
        return (object) AppRibbonSetup.GetBitmapImage("16A.png");
      case "17":
        return (object) AppRibbonSetup.GetBitmapImage("17.png");
      case "17A":
        return (object) AppRibbonSetup.GetBitmapImage("17A.png");
      case "18":
        return (object) AppRibbonSetup.GetBitmapImage("18.png");
      case "19":
        return (object) AppRibbonSetup.GetBitmapImage("19.png");
      case "19A":
        return (object) AppRibbonSetup.GetBitmapImage("19A.png");
      case "20":
        return (object) AppRibbonSetup.GetBitmapImage("20.png");
      case "22":
        return (object) AppRibbonSetup.GetBitmapImage("22.png");
      case "23":
        return (object) AppRibbonSetup.GetBitmapImage("23.png");
      case "24":
        return (object) AppRibbonSetup.GetBitmapImage("24.png");
      case "25":
        return (object) AppRibbonSetup.GetBitmapImage("25.png");
      case "26":
        return (object) AppRibbonSetup.GetBitmapImage("26.png");
      case "26A":
        return (object) AppRibbonSetup.GetBitmapImage("26A.png");
      case "26B":
        return (object) AppRibbonSetup.GetBitmapImage("26B.png");
      case "26C":
        return (object) AppRibbonSetup.GetBitmapImage("26C.png");
      default:
        return (object) AppRibbonSetup.GetBitmapImage("EdgeforRevit.png");
    }
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException("The method or operation is not implemented.");
  }
}
