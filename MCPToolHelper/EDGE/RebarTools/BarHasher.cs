// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.BarHasher
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable disable
namespace EDGE.RebarTools;

public class BarHasher
{
  public static bool useMetricEuro = false;
  private static Dictionary<string, string> WellsOppHand = new Dictionary<string, string>()
  {
    {
      "S1",
      "G,D,C,B,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "S3",
      "G,D,C,B,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "S4",
      "G,D,C,B,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "S6",
      "G,D,C,B,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "6",
      "G,D,C,B,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "T1",
      "G,C,B,E,D,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "T2",
      "G,C,B,E,D,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "1",
      "G,B,C,D,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "2",
      "G,B,C,D,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "8",
      "G,B,C,D,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "24",
      "G,B,C,D,E,F,A,H,J,K,O,Bc,Cd,De"
    },
    {
      "3",
      "G,F,E,D,C,B,A,H,J,K,O,Bc,Dc,De"
    },
    {
      "4",
      "G,F,E,D,C,B,A,H,J,K,O,Bc,Dc,De"
    },
    {
      "22",
      "G,F,E,D,C,B,A,H,J,K,O,Bc,Dc,De"
    },
    {
      "23",
      "G,F,E,D,C,B,A,H,J,K,O,Bc,Dc,De"
    },
    {
      "5",
      "G,D,C,B,E,F,A,H,J,K,O,Bc,Dc,De"
    },
    {
      "7",
      "G,B,E,D,C,F,A,H,J,K,O,Bc,Dc,De"
    },
    {
      "10",
      "C,B,A,D,E,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "14",
      "E,D,C,B,A,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "17",
      "A,D,C,B,E,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "20",
      "A,D,C,B,E,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "17A",
      "A,C,B,D,E,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "19",
      "A,D,C,B,E,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "19A",
      "A,C,B,D,E,F,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "25",
      "A,F,E,D,C,B,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "26",
      "A,F,E,D,C,B,G,H,J,K,O,Bc,Dc,De"
    },
    {
      "26A",
      "A,F,E,D,C,B,G,H,J,K,O,Bc,Dc,De"
    }
  };
  private static Dictionary<string, string> KerkstraOppHand = new Dictionary<string, string>()
  {
    {
      "1",
      "B,A,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "2",
      "C,B,A,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "3",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "4",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "5",
      "B,A,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "6",
      "B,A,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "7",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "8",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "9",
      "C,B,A,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "10",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "11",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "12",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "13",
      "A,F,E,D,C,B,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "14",
      "C,B,A,E,D,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "15",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "16",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "17",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "18",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "19",
      "C,B,A,E,D,G,F,H,J,K,O,Bc,Cd,De"
    },
    {
      "20",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "21",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    },
    {
      "22",
      "A,B,C,D,E,F,G,H,J,K,O,Bc,Cd,De"
    }
  };

  public static string QuickHash(Rebar bar, ManufacturerKeying keying)
  {
    return BarHasher.GetHashForBarShape(bar, false, keying);
  }

  public static string OppHash(Rebar bar, ManufacturerKeying keying)
  {
    return BarHasher.GetHashForBarShape(bar, true, keying);
  }

  public static List<string> GetBothBarHashes(Rebar bar, ManufacturerKeying keying)
  {
    string hashForBarShape1 = BarHasher.GetHashForBarShape(bar, false, keying);
    string hashForBarShape2 = BarHasher.GetHashForBarShape(bar, true, keying);
    List<string> bothBarHashes = new List<string>()
    {
      hashForBarShape1
    };
    if (!bothBarHashes.Contains(hashForBarShape2))
      bothBarHashes.Add(hashForBarShape2);
    return bothBarHashes;
  }

  public static string HashLength(
    double lengthInFeet,
    LengthHashOption hashOption,
    string zerosFormat = "00")
  {
    string input = "ERROR: RebarEditingUpdateUtility.cs:HashLength() failed;";
    if (lengthInFeet.ApproximatelyEqual(0.0))
      return "null";
    if (hashOption == LengthHashOption.None)
      return "";
    double num1 = lengthInFeet;
    double num2 = lengthInFeet % 1.0 * 12.0;
    double d1 = Math.Round(num2 % 1.0 * 8.0, MidpointRounding.AwayFromZero);
    double d2 = Math.Round(num2 % 1.0 * 4.0, MidpointRounding.AwayFromZero);
    if (hashOption == LengthHashOption.FeetAndInchesToEights)
    {
      if (d1 > 7.5)
      {
        d1 = 0.0;
        num2 = Math.Ceiling(num2);
        if (num2 > 11.0)
        {
          num2 = 0.0;
          num1 = Math.Ceiling(num1);
        }
      }
    }
    else if (hashOption == LengthHashOption.RockyFeetAndInchesToQuarter || hashOption == LengthHashOption.MidStatesFeet_InchesToQuarter)
    {
      if (d2 > 3.5)
      {
        d2 = 0.0;
        num2 = Math.Ceiling(num2);
        if (num2 > 11.0)
        {
          num2 = 0.0;
          num1 = Math.Ceiling(num1);
        }
      }
    }
    else if (hashOption == LengthHashOption.FeetAndInches_RoundDown || hashOption == LengthHashOption.MidStatesFeetAndInches)
    {
      num2 = Math.Floor(Math.Round(num2, 1));
      if (num2 > 11.5)
      {
        num2 = 0.0;
        num1 = Math.Ceiling(num1);
      }
    }
    else if (hashOption == LengthHashOption.FeetAndInches_RoundUp)
    {
      num2 = Math.Ceiling(Math.Round(num2, 2));
      if (num2 > 11.5)
      {
        num2 = 0.0;
        num1 = Math.Ceiling(num1);
      }
    }
    else if (hashOption == LengthHashOption.Millimeters || hashOption == LengthHashOption.Centimeters || hashOption == LengthHashOption.Meters || BarHasher.useMetricEuro)
      num1 = Math.Floor(Math.Round(lengthInFeet, 1));
    else
      num2 = Math.Floor(Math.Round(lengthInFeet * 12.0, 1));
    switch (hashOption)
    {
      case LengthHashOption.FeetAndInches_RoundDown:
      case LengthHashOption.FeetAndInchesToEights:
      case LengthHashOption.FeetAndInches_RoundUp:
        input = $"{Convert.ToInt32(Math.Truncate(num1)):00}" + $"{Convert.ToInt32(Math.Truncate(num2)):00}";
        if (hashOption == LengthHashOption.FeetAndInchesToEights)
        {
          input += $"{Convert.ToInt32(Math.Truncate(d1)):00}";
          break;
        }
        break;
      case LengthHashOption.Inches:
        input = string.Format($"{{0:{zerosFormat}}}", (object) Convert.ToInt32(Math.Truncate(num2)));
        break;
      case LengthHashOption.RockyFeetAndInchesToQuarter:
        input = $"{Convert.ToInt32(Math.Truncate(num1)):00}" + $"{Convert.ToInt32(Math.Truncate(num2)):00}";
        if (Convert.ToInt32(Math.Truncate(d2)) != 0)
        {
          input += $"{Convert.ToInt32(Math.Truncate(d2)):0}";
          break;
        }
        break;
      case LengthHashOption.MidStatesFeetAndInches:
        input = $"{Convert.ToInt32(Math.Truncate(num1)):00}-{Convert.ToInt32(Math.Truncate(num2)):00}";
        break;
      case LengthHashOption.MidStatesFeet_InchesToQuarter:
        input = $"{Convert.ToInt32(Math.Truncate(num1)):00}-{Convert.ToInt32(Math.Truncate(num2)):00}";
        if (Convert.ToInt32(Math.Truncate(d2)) != 0)
        {
          input += $"{Convert.ToInt32(Math.Truncate(d2)):0}";
          break;
        }
        break;
      case LengthHashOption.Millimeters:
        input = num1.ToString();
        break;
      case LengthHashOption.Centimeters:
        input = num1.ToString();
        break;
      case LengthHashOption.Meters:
        input = num1.ToString();
        break;
    }
    return !new Regex("[^a-zA-Z0-9](-)").Match(input).Success ? input : throw new Exception($"Length Hash failed for length: {lengthInFeet.ToString()} option: {hashOption.ToString()} resulted in hash: {input} which contains illegal characters.  It is possible that an edit you made to the family has caused one of the bar length dimensions to become negative.  Please cancel this edit and determine the issue with the family.");
  }

  public static string HashAngle(double angle)
  {
    double d = Math.Round(angle * 180.0 / Math.PI, MidpointRounding.AwayFromZero);
    if (d > 360.0)
      d -= 360.0;
    return $"{Convert.ToInt32(Math.Truncate(d)):000}";
  }

  public static double ParseBarDim(string barDimension)
  {
    if (barDimension.Contains("mm"))
    {
      barDimension = barDimension.Trim();
      int startIndex = barDimension.IndexOf("mm");
      barDimension = barDimension.Remove(startIndex);
      double result;
      double.TryParse(barDimension, out result);
      return result;
    }
    Match match = new Regex("^\\s*(?<neg>-)?\\s*(((?<feet>[\\d.]+)')?[\\s-]*((?<inch>\\d+)?[\\s-]*((?<num>\\d+)/(?<den>\\d+))?\\\")?)\\s*$").Match(barDimension);
    return !match.Success || barDimension.Trim() == "" || barDimension.Trim() == "null" ? 0.0 : (match.Groups["neg"].Success ? -1.0 : 1.0) * (match.Groups["feet"].Success ? Convert.ToDouble(match.Groups["feet"].Value) : 0.0) + ((match.Groups["inch"].Success ? (double) Convert.ToInt32(match.Groups["inch"].Value) : 0.0) + (match.Groups["num"].Success ? (double) Convert.ToInt32(match.Groups["num"].Value) : 0.0) / Convert.ToDouble(match.Groups["den"].Success ? Convert.ToInt32(match.Groups["den"].Value) : 1)) / 12.0;
  }

  private static string GetHashForBarShape(Rebar bar, bool oppHand, ManufacturerKeying keying)
  {
    switch (keying)
    {
      case ManufacturerKeying.Kerkstra:
        return BarHasher.GetHashForKerkstraBarShape(bar, oppHand);
      case ManufacturerKeying.Wells:
        return BarHasher.GetHashForWellsBarShape(bar, oppHand);
      case ManufacturerKeying.Tindall:
        return BarHasher.GetHashForTindallBarShape(bar, oppHand);
      default:
        return BarHasher.GetHashForStandardBarShape(bar, oppHand);
    }
  }

  private static string GetHashForWellsBarShape(Rebar bar, bool oppHand)
  {
    if (!oppHand)
      return bar.BarShape + BarHasher.HashLength(bar.dblBarDiameter, LengthHashOption.FeetAndInchesToEights) + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
    string forWellsBarShape = bar.BarShape + BarHasher.HashLength(bar.dblBarDiameter, LengthHashOption.FeetAndInchesToEights);
    if (BarHasher.WellsOppHand.ContainsKey(bar.BarShape))
    {
      string str = BarHasher.WellsOppHand[bar.BarShape];
      char[] chArray = new char[1]{ ',' };
      foreach (string side in str.Split(chArray))
        forWellsBarShape += bar.HashedBarDims[BarHasher.GetBarside(side)];
    }
    else
      forWellsBarShape = forWellsBarShape + BarHasher.HashLength(bar.dblBarDiameter, LengthHashOption.FeetAndInchesToEights) + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
    return forWellsBarShape;
  }

  private static string GetHashForKerkstraBarShape(Rebar bar, bool oppHand)
  {
    if (oppHand)
      return bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
    string kerkstraBarShape = bar.BarShape;
    if (BarHasher.KerkstraOppHand.ContainsKey(bar.BarShape))
    {
      string str = BarHasher.KerkstraOppHand[bar.BarShape];
      char[] chArray = new char[1]{ ',' };
      foreach (string side in str.Split(chArray))
        kerkstraBarShape += bar.HashedBarDims[BarHasher.GetBarside(side)];
    }
    else
      kerkstraBarShape = kerkstraBarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
    return kerkstraBarShape;
  }

  private static BarSide GetBarside(string side)
  {
    if (side != null)
    {
      switch (side.Length)
      {
        case 1:
          switch (side[0])
          {
            case 'A':
              return BarSide.BarLengthA;
            case 'B':
              return BarSide.BarLengthB;
            case 'C':
              return BarSide.BarLengthC;
            case 'D':
              return BarSide.BarLengthD;
            case 'E':
              return BarSide.BarLengthE;
            case 'F':
              return BarSide.BarLengthF;
            case 'G':
              return BarSide.BarLengthG;
            case 'H':
              return BarSide.BarLengthH;
            case 'J':
              return BarSide.BarLengthJ;
            case 'K':
              return BarSide.BarLengthK;
            case 'O':
              return BarSide.BarLengthO;
          }
          break;
        case 2:
          switch (side[0])
          {
            case 'B':
              if (side == "Bc")
                return BarSide.BarAngleBc;
              break;
            case 'C':
              if (side == "Cd")
                return BarSide.BarAngleCd;
              break;
            case 'D':
              if (side == "De")
                return BarSide.BarAngleDe;
              break;
          }
          break;
      }
    }
    return BarSide.BarLengthA;
  }

  private static string GetHashForTindallBarShape(Rebar bar, bool oppHand)
  {
    string upper = bar.BarShape.ToUpper();
    if (upper != null)
    {
      switch (upper.Length)
      {
        case 1:
          switch (upper[0])
          {
            case '0':
              return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
            case '1':
              return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
            case '2':
              return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
            case '5':
              return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
          }
          break;
        case 2:
          switch (upper[1])
          {
            case '0':
              if (upper == "10")
                break;
              break;
            case '1':
              if (upper == "11")
                break;
              break;
          }
          break;
      }
    }
    return bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
  }

  private static string GetHashForStandardBarShape(Rebar bar, bool oppHand)
  {
    string upper = bar.BarShape.ToUpper();
    if (upper != null)
    {
      switch (upper.Length)
      {
        case 1:
          switch (upper[0])
          {
            case '1':
            case '2':
            case '8':
              goto label_23;
            case '3':
            case '4':
              goto label_26;
            case '5':
              return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleDe];
            case '6':
              break;
            case '7':
              return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleDe] + bar.HashedBarDims[BarSide.BarAngleCd];
            default:
              goto label_56;
          }
          break;
        case 2:
          switch (upper[1])
          {
            case '0':
              switch (upper)
              {
                case "10":
                  return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
                case "20":
                  goto label_41;
                default:
                  goto label_56;
              }
            case '1':
              switch (upper)
              {
                case "S1":
                  goto label_17;
                case "T1":
                  break;
                default:
                  goto label_56;
              }
              break;
            case '2':
              switch (upper)
              {
                case "T2":
                  break;
                case "22":
                  goto label_26;
                default:
                  goto label_56;
              }
              break;
            case '3':
              switch (upper)
              {
                case "S3":
                  goto label_17;
                case "23":
                  goto label_26;
                default:
                  goto label_56;
              }
            case '4':
              switch (upper)
              {
                case "S4":
                  goto label_17;
                case "24":
                  goto label_23;
                case "14":
                  return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
                default:
                  goto label_56;
              }
            case '5':
              if (upper == "25")
                goto label_53;
              goto label_56;
            case '6':
              switch (upper)
              {
                case "S6":
                  goto label_17;
                case "26":
                  goto label_53;
                default:
                  goto label_56;
              }
            case '7':
              if (upper == "17")
                goto label_41;
              goto label_56;
            case '9':
              if (upper == "19")
                return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleDe];
              goto label_56;
            default:
              goto label_56;
          }
          return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
label_41:
          return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
        case 3:
          switch (upper[1])
          {
            case '6':
              if (upper == "26A")
                goto label_53;
              goto label_56;
            case '7':
              if (upper == "17A")
                return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
              goto label_56;
            case '9':
              if (upper == "19A")
                return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
              goto label_56;
            default:
              goto label_56;
          }
        default:
          goto label_56;
      }
label_17:
      return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
label_23:
      return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
label_26:
      return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleDe] + bar.HashedBarDims[BarSide.BarAngleCd];
label_53:
      return !oppHand ? bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe] : bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
    }
label_56:
    return bar.BarShape + bar.HashedBarDims[BarSide.BarLengthA] + bar.HashedBarDims[BarSide.BarLengthB] + bar.HashedBarDims[BarSide.BarLengthC] + bar.HashedBarDims[BarSide.BarLengthD] + bar.HashedBarDims[BarSide.BarLengthE] + bar.HashedBarDims[BarSide.BarLengthF] + bar.HashedBarDims[BarSide.BarLengthG] + bar.HashedBarDims[BarSide.BarLengthH] + bar.HashedBarDims[BarSide.BarLengthJ] + bar.HashedBarDims[BarSide.BarLengthK] + bar.HashedBarDims[BarSide.BarLengthO] + bar.HashedBarDims[BarSide.BarAngleBc] + bar.HashedBarDims[BarSide.BarAngleCd] + bar.HashedBarDims[BarSide.BarAngleDe];
  }
}
