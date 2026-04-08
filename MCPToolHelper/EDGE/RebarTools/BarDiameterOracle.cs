// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.BarDiameterOracle
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System.Collections.Generic;

#nullable disable
namespace EDGE.RebarTools;

internal class BarDiameterOracle
{
  public static Dictionary<string, double> BarToDiameterDictionary = new Dictionary<string, double>();

  public static bool IsValidDiameter(double barDiameter)
  {
    if (!BarHasher.useMetricEuro)
    {
      double num = barDiameter * 12.0;
      return num < 1.42 && num > 0.37;
    }
    return barDiameter <= 50.1 && barDiameter >= 5.9;
  }

  public static int ResolveBarDiam(double barDiameter)
  {
    List<int> intList = new List<int>()
    {
      6,
      8,
      10,
      12,
      14,
      16 /*0x10*/,
      20,
      25,
      28,
      32 /*0x20*/,
      40,
      50
    };
    if (!BarHasher.useMetricEuro)
    {
      double dbl2 = barDiameter * 12.0;
      if (dbl2 > 1.42)
      {
        string lineToLog = "Bar Diameter is larger than maximum allowed: " + dbl2.ToString();
        QA.LogError("BarDiameterOracle:ResolveBarDiam", lineToLog);
        if (!App.DialogSwitches.ShownRebarTooLargeMessage)
        {
          TaskDialog.Show("Rebar Error", lineToLog);
          App.DialogSwitches.ShownRebarTooLargeMessage = true;
        }
      }
      else if (dbl2 < 0.37)
      {
        string lineToLog = $"Bar Diameter is {dbl2.ToString()} and cannot be less than 3/8\" inch";
        QA.LogError("BarDiameterOracle:ResolveBarDiam", lineToLog);
        if (!App.DialogSwitches.ShownRebarTooSmallMessage)
        {
          TaskDialog.Show("Rebar Error", lineToLog);
          App.DialogSwitches.ShownRebarTooSmallMessage = true;
        }
      }
      for (int BarNum = 3; BarNum < 12; ++BarNum)
      {
        if (BarDiameterOracle.GetBarDiamForUSBarNumber(BarNum).ApproximatelyEqual(dbl2))
          return BarNum;
      }
    }
    else
    {
      if (barDiameter >= 55.1)
      {
        string lineToLog = "Bar Diameter is larger than maximum allowed: " + barDiameter.ToString();
        QA.LogError("BarDiameterOracle:ResolveBarDiam", lineToLog);
        if (!App.DialogSwitches.ShownRebarTooLargeMessage)
        {
          TaskDialog.Show("Rebar Error", lineToLog);
          App.DialogSwitches.ShownRebarTooLargeMessage = true;
        }
      }
      else if (barDiameter <= 5.9)
      {
        string lineToLog = $"Bar Diameter is {barDiameter.ToString()} and cannot be less than 6 mm";
        QA.LogError("BarDiameterOracle:ResolveBarDiam", lineToLog);
        if (!App.DialogSwitches.ShownRebarTooSmallMessage)
        {
          TaskDialog.Show("Rebar Error", lineToLog);
          App.DialogSwitches.ShownRebarTooSmallMessage = true;
        }
      }
      foreach (int BarNum in intList)
      {
        double forEuroBarNumber = BarDiameterOracle.GetBarDiamForEuroBarNumber(BarNum);
        double num = 0.1;
        if (forEuroBarNumber > barDiameter - num && forEuroBarNumber < barDiameter + num)
          return BarNum;
      }
    }
    return 0;
  }

  private static double GetBarDiamForUSBarNumber(int BarNum)
  {
    if (BarNum < 9)
      return (double) BarNum * 1.0 / 8.0;
    switch (BarNum)
    {
      case 9:
        return 289.0 / 256.0;
      case 10:
        return 1.27;
      case 11:
        return 1.41;
      default:
        return 0.0;
    }
  }

  private static double GetBarDiamForEuroBarNumber(int BarNum) => (double) BarNum * 1.0;

  public static string GetDiameterStrForRealDiameter(
    double barDiameterInFeet,
    bool bUseMetric,
    bool bPadWithLeadingZero)
  {
    string strForRealDiameter = "Diameter Error: This Bar has an illegal bar diameter";
    if (!BarDiameterOracle.IsValidDiameter(barDiameterInFeet))
      return "invalidBarDiameter";
    switch (BarDiameterOracle.ResolveBarDiam(barDiameterInFeet))
    {
      case 3:
        strForRealDiameter = bUseMetric ? "10" : (bPadWithLeadingZero ? "03" : "3");
        break;
      case 4:
        strForRealDiameter = bUseMetric ? "13" : (bPadWithLeadingZero ? "04" : "4");
        break;
      case 5:
        strForRealDiameter = bUseMetric ? "16" : (bPadWithLeadingZero ? "05" : "5");
        break;
      case 6:
        strForRealDiameter = bUseMetric ? "19" : (bPadWithLeadingZero ? "06" : "6");
        break;
      case 7:
        strForRealDiameter = bUseMetric ? "22" : (bPadWithLeadingZero ? "07" : "7");
        break;
      case 8:
        strForRealDiameter = bUseMetric ? "25" : (bPadWithLeadingZero ? "08" : "8");
        break;
      case 9:
        strForRealDiameter = bUseMetric ? "29" : (bPadWithLeadingZero ? "09" : "9");
        break;
      case 10:
        strForRealDiameter = bUseMetric ? "32" : "10";
        break;
      case 11:
        strForRealDiameter = bUseMetric ? "36" : "11";
        break;
      case 12:
        strForRealDiameter = "12";
        break;
      case 14:
        strForRealDiameter = "14";
        break;
      case 16 /*0x10*/:
        strForRealDiameter = "16";
        break;
      case 20:
        strForRealDiameter = "20";
        break;
      case 25:
        strForRealDiameter = "25";
        break;
      case 28:
        strForRealDiameter = "28";
        break;
      case 32 /*0x20*/:
        strForRealDiameter = "32";
        break;
      case 40:
        strForRealDiameter = "40";
        break;
      case 50:
        strForRealDiameter = "50";
        break;
    }
    return strForRealDiameter;
  }

  static BarDiameterOracle()
  {
    BarDiameterOracle.BarToDiameterDictionary.Add("3", 0.375);
    BarDiameterOracle.BarToDiameterDictionary.Add("4", 0.5);
    BarDiameterOracle.BarToDiameterDictionary.Add("5", 0.625);
    BarDiameterOracle.BarToDiameterDictionary.Add("6", 0.75);
    BarDiameterOracle.BarToDiameterDictionary.Add("7", 0.875);
    BarDiameterOracle.BarToDiameterDictionary.Add("8", 1.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("9", 289.0 / 256.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("10", 1.27);
    BarDiameterOracle.BarToDiameterDictionary.Add("11", 1.41);
    BarDiameterOracle.BarToDiameterDictionary.Add("14", 1.639);
    BarDiameterOracle.BarToDiameterDictionary.Add("18", 2.257);
    BarDiameterOracle.BarToDiameterDictionary.Add("18J", 2.337);
    BarDiameterOracle.BarToDiameterDictionary.Add("10M", 113.0 / 254.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("15M", 80.0 / (double) sbyte.MaxValue);
    BarDiameterOracle.BarToDiameterDictionary.Add("20M", 195.0 / 254.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("25M", 126.0 / (double) sbyte.MaxValue);
    BarDiameterOracle.BarToDiameterDictionary.Add("30M", 299.0 / 254.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("35M", 357.0 / 254.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("45M", 437.0 / 254.0);
    BarDiameterOracle.BarToDiameterDictionary.Add("55M", 282.0 / (double) sbyte.MaxValue);
  }

  public static string MetricNumberToCanadian(string number) => "";

  public static string BarNumberToCanadian(string number, bool bUseMetric, bool bPadWithZero)
  {
    string canadian;
    if (bUseMetric)
    {
      switch (number)
      {
        case "13":
          canadian = "10M";
          break;
        case "16":
          canadian = "15M";
          break;
        case "22":
          canadian = "20M";
          break;
        case "25":
          canadian = "25M";
          break;
        case "32":
          canadian = "30M";
          break;
        case "36":
          canadian = "35M";
          break;
        default:
          canadian = "0M";
          break;
      }
    }
    else if (bPadWithZero)
    {
      switch (number)
      {
        case "04":
          canadian = "10M";
          break;
        case "05":
          canadian = "15M";
          break;
        case "07":
          canadian = "20M";
          break;
        case "08":
          canadian = "25M";
          break;
        case "10":
          canadian = "30M";
          break;
        case "11":
          canadian = "35M";
          break;
        default:
          canadian = "0M";
          break;
      }
    }
    else
    {
      switch (number)
      {
        case "4":
          canadian = "10M";
          break;
        case "5":
          canadian = "15M";
          break;
        case "7":
          canadian = "20M";
          break;
        case "8":
          canadian = "25M";
          break;
        case "10":
          canadian = "30M";
          break;
        case "11":
          canadian = "35M";
          break;
        default:
          canadian = "0M";
          break;
      }
    }
    return canadian;
  }

  public static string BarNumberToBarDiameter(string barNumber, bool metric)
  {
    if (!metric)
    {
      if (barNumber != null)
      {
        switch (barNumber.Length)
        {
          case 2:
            switch (barNumber[1])
            {
              case '3':
                if (barNumber == "#3")
                  return "0' - 3/8\"";
                break;
              case '4':
                if (barNumber == "#4")
                  return "0' - 1/2\"";
                break;
              case '5':
                if (barNumber == "#5")
                  return "0' - 5/8\"";
                break;
              case '6':
                if (barNumber == "#6")
                  return "0' - 3/4\"";
                break;
              case '7':
                if (barNumber == "#7")
                  return "0' - 7/8\"";
                break;
              case '8':
                if (barNumber == "#8")
                  return "0' - 1\"";
                break;
              case '9':
                if (barNumber == "#9")
                  return "0' - 1 3/25\"";
                break;
            }
            break;
          case 3:
            switch (barNumber[2])
            {
              case '0':
                if (barNumber == "#10")
                  return "0' - 1 27/100\"";
                break;
              case '1':
                if (barNumber == "#11")
                  return "0' - 1 41/100\"";
                break;
            }
            break;
        }
      }
    }
    else if (barNumber != null)
    {
      switch (barNumber.Length)
      {
        case 2:
          switch (barNumber[1])
          {
            case '6':
              if (barNumber == "#6")
                return "6mm";
              break;
            case '8':
              if (barNumber == "#8")
                return "8mm";
              break;
          }
          break;
        case 3:
          switch (barNumber[2])
          {
            case '0':
              switch (barNumber)
              {
                case "#10":
                  return "10mm";
                case "#20":
                  return "20mm";
                case "#40":
                  return "40mm";
                case "#50":
                  return "50mm";
              }
              break;
            case '2':
              switch (barNumber)
              {
                case "#12":
                  return "12mm";
                case "#32":
                  return "32mm";
              }
              break;
            case '4':
              if (barNumber == "#14")
                return "14mm";
              break;
            case '5':
              if (barNumber == "#25")
                return "25mm";
              break;
            case '6':
              if (barNumber == "#16")
                return "16mm";
              break;
            case '8':
              if (barNumber == "#28")
                return "28mm";
              break;
          }
          break;
      }
    }
    return "";
  }

  public static string MetricBarNumberToBarDiameter(string barNumber)
  {
    if (barNumber != null)
    {
      switch (barNumber.Length)
      {
        case 2:
          if (barNumber == "#6")
            return "0' - 1/4\"";
          break;
        case 3:
          switch (barNumber[2])
          {
            case '0':
              if (barNumber == "#10")
                return "0' - 3/8\"";
              break;
            case '2':
              switch (barNumber)
              {
                case "#22":
                  return "0' - 7/8\"";
                case "#32":
                  return "0' - 1 27/100\"";
              }
              break;
            case '3':
              switch (barNumber)
              {
                case "#13":
                  return "0' - 1/2\"";
                case "#43":
                  return "0' - 1 693/1000\"";
              }
              break;
            case '5':
              if (barNumber == "#25")
                return "0' - 1\"";
              break;
            case '6':
              switch (barNumber)
              {
                case "#16":
                  return "0' - 5/8\"";
                case "#36":
                  return "0' - 1 41/100\"";
              }
              break;
            case '7':
              if (barNumber == "#57")
                return "0' - 2 257/1000\"";
              break;
            case '9':
              switch (barNumber)
              {
                case "#19":
                  return "0' - 3/4\"";
                case "#29":
                  return "0' - 1 16/125\"";
              }
              break;
          }
          break;
      }
    }
    return "";
  }

  public static string BarDiameterToBarNumberEuropean(string barDiameter)
  {
    barDiameter.Trim();
    if (barDiameter != null)
    {
      switch (barDiameter.Length)
      {
        case 3:
          switch (barDiameter[0])
          {
            case '6':
              if (barDiameter == "6mm")
                return "#6";
              break;
            case '8':
              if (barDiameter == "8mm")
                return "#8";
              break;
          }
          break;
        case 4:
          switch (barDiameter[1])
          {
            case '0':
              switch (barDiameter)
              {
                case "10mm":
                  return "#10";
                case "20mm":
                  return "#20";
                case "40mm":
                  return "#40";
                case "50mm":
                  return "#50";
              }
              break;
            case '2':
              switch (barDiameter)
              {
                case "12mm":
                  return "#12";
                case "32mm":
                  return "#32";
              }
              break;
            case '4':
              if (barDiameter == "14mm")
                return "#14";
              break;
            case '5':
              if (barDiameter == "25mm")
                return "#25";
              break;
            case '6':
              if (barDiameter == "16mm")
                return "#16";
              break;
            case '8':
              if (barDiameter == "28mm")
                return "#28";
              break;
          }
          break;
      }
    }
    return "";
  }

  public static string BarDiameterToBarNumber(string barDiameter)
  {
    if (barDiameter != null)
    {
      switch (barDiameter.Length)
      {
        case 7:
          if (barDiameter == "0' - 1\"")
            return "#8";
          break;
        case 9:
          switch (barDiameter[5])
          {
            case '1':
              if (barDiameter == "0' - 1/2\"")
                return "#4";
              break;
            case '3':
              switch (barDiameter)
              {
                case "0' - 3/8\"":
                  return "#3";
                case "0' - 3/4\"":
                  return "#6";
              }
              break;
            case '5':
              if (barDiameter == "0' - 5/8\"")
                return "#5";
              break;
            case '7':
              if (barDiameter == "0' - 7/8\"")
                return "#7";
              break;
          }
          break;
        case 12:
          if (barDiameter == "0' - 1 3/25\"")
            return "#9";
          break;
        case 14:
          switch (barDiameter[7])
          {
            case '2':
              if (barDiameter == "0' - 1 27/100\"")
                return "#10";
              break;
            case '4':
              if (barDiameter == "0' - 1 41/100\"")
                return "#11";
              break;
          }
          break;
      }
    }
    return "";
  }

  public static string MetricBarDiameterToBarNumber(string barDiameter)
  {
    if (barDiameter != null)
    {
      switch (barDiameter.Length)
      {
        case 7:
          if (barDiameter == "0' - 1\"")
            return "#25";
          break;
        case 9:
          switch (barDiameter[5])
          {
            case '1':
              switch (barDiameter)
              {
                case "0' - 1/4\"":
                  return "#6";
                case "0' - 1/2\"":
                  return "#13";
              }
              break;
            case '3':
              switch (barDiameter)
              {
                case "0' - 3/8\"":
                  return "#10";
                case "0' - 3/4\"":
                  return "#19";
              }
              break;
            case '5':
              if (barDiameter == "0' - 5/8\"")
                return "#16";
              break;
            case '7':
              if (barDiameter == "0' - 7/8\"")
                return "#22";
              break;
          }
          break;
        case 14:
          switch (barDiameter[7])
          {
            case '1':
              if (barDiameter == "0' - 1 16/125\"")
                return "#29";
              break;
            case '2':
              if (barDiameter == "0' - 1 27/100\"")
                return "#32";
              break;
            case '4':
              if (barDiameter == "0' - 1 41/100\"")
                return "#36";
              break;
          }
          break;
        case 16 /*0x10*/:
          switch (barDiameter[5])
          {
            case '1':
              if (barDiameter == "0' - 1 693/1000\"")
                return "#43";
              break;
            case '2':
              if (barDiameter == "0' - 2 257/1000\"")
                return "#57";
              break;
          }
          break;
      }
    }
    return "";
  }
}
