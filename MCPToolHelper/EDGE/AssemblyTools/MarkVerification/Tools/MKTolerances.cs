// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.Tools.MKTolerances
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.Tools;

public class MKTolerances
{
  private ComparisonOption RoundingOption;
  private Dictionary<MKToleranceAspect, MKTolerances.IToleranceSpec> verificationTolerances;

  public MKTolerances(Document revitDoc) => this.Init(revitDoc);

  public MKTolerances(ComparisonOption roundOption, Document revitDoc)
  {
    this.RoundingOption = roundOption;
    this.Init(revitDoc);
  }

  private void Init(Document revitDoc)
  {
    string path1 = App.MarkPrefixFolderPath;
    if (!Directory.Exists(path1))
      path1 = "C:\\EDGEforRevit";
    string str1 = "";
    Parameter parameter1 = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter1 != null && !string.IsNullOrEmpty(parameter1.AsString()))
      str1 = parameter1.AsString();
    string path2 = $"{path1}\\{str1}_Mark_Tolerance.txt";
    this.verificationTolerances = new Dictionary<MKToleranceAspect, MKTolerances.IToleranceSpec>();
    if (File.Exists(path2))
    {
      StreamReader streamReader = new StreamReader(path2);
      string str2;
      while ((str2 = streamReader.ReadLine()) != null)
      {
        if (!string.IsNullOrWhiteSpace(str2))
        {
          string[] strArray = str2.Split(new string[1]
          {
            ","
          }, StringSplitOptions.RemoveEmptyEntries);
          string str3 = strArray[0];
          if (str3 != null)
          {
            switch (str3.Length)
            {
              case 5:
                if (str3 == "Angle")
                {
                  if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                  {
                    this.verificationTolerances.Add(MKToleranceAspect.Angle, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Math.PI / 90.0));
                    continue;
                  }
                  this.verificationTolerances.Add(MKToleranceAspect.Angle, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Convert.ToDouble(strArray[1]) * Math.PI / 180.0));
                  continue;
                }
                continue;
              case 6:
                switch (str3[0])
                {
                  case 'V':
                    if (str3 == "Volume")
                    {
                      if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                      {
                        this.verificationTolerances.Add(MKToleranceAspect.Volume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(0.1));
                        continue;
                      }
                      this.verificationTolerances.Add(MKToleranceAspect.Volume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Convert.ToDouble(strArray[1])));
                      continue;
                    }
                    continue;
                  case 'W':
                    if (str3 == "Weight")
                    {
                      if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                      {
                        this.verificationTolerances.Add(MKToleranceAspect.Weight, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1.0, 0));
                        continue;
                      }
                      this.verificationTolerances.Add(MKToleranceAspect.Weight, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Convert.ToDouble(strArray[1])));
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 8:
                if (str3 == "Geometry")
                {
                  if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                  {
                    this.verificationTolerances.Add(MKToleranceAspect.Geometry, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 192.0, FractionalRounding.SixteenthInch));
                    continue;
                  }
                  this.verificationTolerances.Add(MKToleranceAspect.Geometry, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(strArray[1]));
                  continue;
                }
                continue;
              case 10:
                if (str3 == "FinishArea")
                {
                  if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                  {
                    this.verificationTolerances.Add(MKToleranceAspect.FinishArea, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1.0, 0));
                    continue;
                  }
                  this.verificationTolerances.Add(MKToleranceAspect.FinishArea, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Convert.ToDouble(strArray[1])));
                  continue;
                }
                continue;
              case 11:
                if (str3 == "AddonVolume")
                {
                  if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                  {
                    this.verificationTolerances.Add(MKToleranceAspect.AddonVolume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(0.01, 2));
                    continue;
                  }
                  this.verificationTolerances.Add(MKToleranceAspect.AddonVolume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Convert.ToDouble(strArray[1])));
                  continue;
                }
                continue;
              case 12:
                if (str3 == "VoidLocation")
                {
                  if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                  {
                    this.verificationTolerances.Add(MKToleranceAspect.VoidLocation, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 192.0, FractionalRounding.SixteenthInch));
                    continue;
                  }
                  this.verificationTolerances.Add(MKToleranceAspect.VoidLocation, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(strArray[1]));
                  continue;
                }
                continue;
              case 16 /*0x10*/:
                if (str3 == "EmbeddedPosition")
                {
                  if (strArray.Length < 2 || string.IsNullOrEmpty(strArray[1]) || string.IsNullOrWhiteSpace(strArray[1]))
                  {
                    this.verificationTolerances.Add(MKToleranceAspect.EmbedPosition, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 24.0, FractionalRounding.SixteenthInch));
                    continue;
                  }
                  this.verificationTolerances.Add(MKToleranceAspect.EmbedPosition, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(strArray[1]));
                  continue;
                }
                continue;
              default:
                continue;
            }
          }
        }
      }
      streamReader.Close();
      this.verificationTolerances.Add(MKToleranceAspect.Strength, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1E-08, 8));
    }
    else
    {
      this.verificationTolerances.Add(MKToleranceAspect.Angle, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(Math.PI / 90.0));
      this.verificationTolerances.Add(MKToleranceAspect.Geometry, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 192.0, FractionalRounding.SixteenthInch));
      this.verificationTolerances.Add(MKToleranceAspect.Volume, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(0.1, FractionalRounding.SixteenthInch));
      this.verificationTolerances.Add(MKToleranceAspect.EmbedPosition, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 24.0, FractionalRounding.SixteenthInch));
      this.verificationTolerances.Add(MKToleranceAspect.FinishArea, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1.0, 0));
      this.verificationTolerances.Add(MKToleranceAspect.VoidLocation, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 192.0, FractionalRounding.SixteenthInch));
      this.verificationTolerances.Add(MKToleranceAspect.AddonVolume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(0.01, 2));
      this.verificationTolerances.Add(MKToleranceAspect.Weight, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1.0, 0));
      this.verificationTolerances.Add(MKToleranceAspect.Strength, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1E-08, 8));
    }
    Parameter parameter2 = revitDoc.ProjectInformation.LookupParameter("INSULATION_VOLUME_TOLERANCE_PERCENTAGE");
    if (parameter2 != null)
    {
      double tolerance = parameter2.AsDouble();
      if (tolerance >= 0.0)
        this.verificationTolerances.Add(MKToleranceAspect.Insulation_Volume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(tolerance));
      else
        this.verificationTolerances.Add(MKToleranceAspect.Insulation_Volume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1.0 / 80.0));
    }
    else
      this.verificationTolerances.Add(MKToleranceAspect.Insulation_Volume, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(1.0 / 80.0));
    Parameter parameter3 = revitDoc.ProjectInformation.LookupParameter("INSULATION_GEOMETRY_TOLERANCE");
    if (parameter3 != null)
    {
      double num = parameter3.AsDouble();
      if (num >= 0.0)
        this.verificationTolerances.Add(MKToleranceAspect.Insulation_Geometry, (MKTolerances.IToleranceSpec) new MKTolerances.DecimalTolerance(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Feet)));
      else
        this.verificationTolerances.Add(MKToleranceAspect.Insulation_Geometry, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 192.0, FractionalRounding.SixteenthInch));
    }
    else
      this.verificationTolerances.Add(MKToleranceAspect.Insulation_Geometry, (MKTolerances.IToleranceSpec) new MKTolerances.FractionalTolerance(1.0 / 192.0, FractionalRounding.SixteenthInch));
  }

  private double FractionToDouble(string fraction)
  {
    double result1;
    if (double.TryParse(fraction, out result1))
      return result1;
    string[] strArray = fraction.Split(' ', '/');
    int result2;
    int result3;
    if ((strArray.Length == 2 || strArray.Length == 3) && int.TryParse(strArray[0], out result2) && int.TryParse(strArray[1], out result3))
    {
      if (strArray.Length == 2)
        return (double) result2 / (double) result3;
      int result4;
      if (int.TryParse(strArray[2], out result4))
        return (double) result2 + (double) result3 / (double) result4;
    }
    throw new FormatException("Not a valid fraction.");
  }

  private FractionalRounding ToFractionRounding(string str)
  {
    if (str != null)
    {
      switch (str.Length)
      {
        case 4:
          if (str == "Feet")
            return FractionalRounding.Feet;
          break;
        case 6:
          if (str == "Inches")
            return FractionalRounding.Inches;
          break;
        case 9:
          if (str == "Half Inch")
            return FractionalRounding.HalfInch;
          break;
        case 11:
          switch (str[0])
          {
            case 'E':
              if (str == "Eighth Inch")
                return FractionalRounding.EighthInch;
              break;
            case 'Q':
              if (str == "Quater Inch")
                return FractionalRounding.QuaterInch;
              break;
          }
          break;
        case 14:
          if (str == "Sixteenth Inch")
            return FractionalRounding.SixteenthInch;
          break;
        case 16 /*0x10*/:
          if (str == "SixtyFourth Inch")
            return FractionalRounding.SixtyFourthInch;
          break;
        case 17:
          if (str == "ThirtySecond Inch")
            return FractionalRounding.ThirtySecondInch;
          break;
        case 20:
          if (str == "OneTwentyEighth Inch")
            return FractionalRounding.OneTwentyEighthInch;
          break;
      }
    }
    return FractionalRounding.Feet;
  }

  public double GetTolerance(MKToleranceAspect aspect)
  {
    return this.verificationTolerances.ContainsKey(aspect) ? this.verificationTolerances[aspect].Tolerance() : double.Epsilon;
  }

  private double RoundToTolerance(double value, MKTolerances.IToleranceSpec toleranceSpec)
  {
    switch (toleranceSpec)
    {
      case MKTolerances.FractionalTolerance _:
        return FeetAndInchesRounding.Round(value, (toleranceSpec as MKTolerances.FractionalTolerance).roundingFraction);
      case MKTolerances.DecimalTolerance _:
        return Math.Round(value, (toleranceSpec as MKTolerances.DecimalTolerance).DecimalPlaces);
      default:
        return 0.0;
    }
  }

  private double NewRoundToTolerance(double value, MKTolerances.IToleranceSpec toleranceSpec)
  {
    switch (toleranceSpec)
    {
      case MKTolerances.FractionalTolerance _:
        double fracTol = (toleranceSpec as MKTolerances.FractionalTolerance).fracTol;
        return FeetAndInchesRounding.FractionToleranceRounding(value, fracTol);
      case MKTolerances.DecimalTolerance _:
        double decimalTolerance = (toleranceSpec as MKTolerances.DecimalTolerance).Tolerance();
        return FeetAndInchesRounding.DecimalToleranceRounding(value, decimalTolerance);
      default:
        return 0.0;
    }
  }

  private XYZ NewRoundToTolerance(XYZ value, MKTolerances.IToleranceSpec toleranceSpec)
  {
    switch (toleranceSpec)
    {
      case MKTolerances.FractionalTolerance _:
        double fracTol = (toleranceSpec as MKTolerances.FractionalTolerance).fracTol;
        double x1 = FeetAndInchesRounding.FractionToleranceRounding(value.X, fracTol);
        double num1 = FeetAndInchesRounding.FractionToleranceRounding(value.Y, fracTol);
        double num2 = FeetAndInchesRounding.FractionToleranceRounding(value.Z, fracTol);
        double y1 = num1;
        double z1 = num2;
        return new XYZ(x1, y1, z1);
      case MKTolerances.DecimalTolerance _:
        double decimalTolerance = (toleranceSpec as MKTolerances.DecimalTolerance).Tolerance();
        double x2 = FeetAndInchesRounding.DecimalToleranceRounding(value.X, decimalTolerance);
        double num3 = FeetAndInchesRounding.DecimalToleranceRounding(value.Y, decimalTolerance);
        double num4 = FeetAndInchesRounding.DecimalToleranceRounding(value.Z, decimalTolerance);
        double y2 = num3;
        double z2 = num4;
        return new XYZ(x2, y2, z2);
      default:
        return XYZ.Zero;
    }
  }

  private XYZ RoundToTolerance(XYZ value, MKTolerances.IToleranceSpec toleranceSpec)
  {
    switch (toleranceSpec)
    {
      case MKTolerances.FractionalTolerance _:
        FractionalRounding roundingFraction = (toleranceSpec as MKTolerances.FractionalTolerance).roundingFraction;
        double x1 = FeetAndInchesRounding.Round(value.X, roundingFraction);
        double num1 = FeetAndInchesRounding.Round(value.Y, roundingFraction);
        double num2 = FeetAndInchesRounding.Round(value.Z, roundingFraction);
        double y1 = num1;
        double z1 = num2;
        return new XYZ(x1, y1, z1);
      case MKTolerances.DecimalTolerance _:
        int decimalPlaces = (toleranceSpec as MKTolerances.DecimalTolerance).DecimalPlaces;
        double x2 = Math.Round(value.X, decimalPlaces);
        double num3 = Math.Round(value.Y, decimalPlaces);
        double num4 = Math.Round(value.Z, decimalPlaces);
        double y2 = num3;
        double z2 = num4;
        return new XYZ(x2, y2, z2);
      default:
        return XYZ.Zero;
    }
  }

  public bool DeltaWithinTolerance(double val, MKToleranceAspect aspect)
  {
    return val.ApproximatelyEquals(this.GetTolerance(aspect), true) || val <= this.GetTolerance(aspect);
  }

  public bool NewValuesAreEqual(
    double val1,
    double val2,
    MKToleranceAspect aspect,
    double vectorLength = 0.0)
  {
    int num = QA.SpecialPermission ? 1 : 0;
    MKComparer mkComparer1;
    if (aspect == MKToleranceAspect.Insulation_Volume)
    {
      double tolerance = this.GetTolerance(aspect);
      mkComparer1 = new MKComparer(val1 >= val2 ? val1 * tolerance : val2 * tolerance);
    }
    else
      mkComparer1 = new MKComparer(this.GetTolerance(aspect));
    if (this.RoundingOption == ComparisonOption.DoNotRound)
      return mkComparer1.Compare(val1, val2) == 0;
    MKComparer mkComparer2 = new MKComparer(double.Epsilon);
    double tolerance1 = this.NewRoundToTolerance(val1, this.verificationTolerances[aspect]);
    double tolerance2 = this.NewRoundToTolerance(val2, this.verificationTolerances[aspect]);
    double x = tolerance1;
    double y = tolerance2;
    return mkComparer2.Compare(x, y) == 0;
  }

  public bool ValuesAreEqual(
    double val1,
    double val2,
    MKToleranceAspect aspect,
    double vectorLength = 0.0)
  {
    int num = QA.SpecialPermission ? 1 : 0;
    MKComparer mkComparer = vectorLength <= 0.001 ? new MKComparer(this.GetTolerance(aspect)) : new MKComparer(this.VectorTolerance(vectorLength, aspect));
    if (this.RoundingOption == ComparisonOption.DoNotRound)
      return mkComparer.Compare(val1, val2) == 0;
    this.RoundToTolerance(val1, this.verificationTolerances[aspect]);
    this.RoundToTolerance(val2, this.verificationTolerances[aspect]);
    return mkComparer.Compare(val1, val2) == 0;
  }

  public bool NewPointsAreEqual(XYZ point1, XYZ point2, MKToleranceAspect aspect)
  {
    int num = QA.SpecialPermission ? 1 : 0;
    double tolerance = this.GetTolerance(aspect);
    return this.RoundingOption == ComparisonOption.DoNotRound ? point1.IsWithinTolerance(point2, tolerance) : this.NewRoundToTolerance(point1, this.verificationTolerances[aspect]).IsWithinTolerance(this.NewRoundToTolerance(point2, this.verificationTolerances[aspect]), double.Epsilon);
  }

  public bool PointsAreEqual(XYZ point1, XYZ point2, MKToleranceAspect aspect)
  {
    int num = QA.SpecialPermission ? 1 : 0;
    double tolerance = this.GetTolerance(aspect);
    return this.RoundingOption == ComparisonOption.DoNotRound ? point1.IsWithinTolerance(point2, tolerance) : this.RoundToTolerance(point1, this.verificationTolerances[aspect]).IsWithinTolerance(this.RoundToTolerance(point2, this.verificationTolerances[aspect]), tolerance);
  }

  private double VectorTolerance(double length, MKToleranceAspect aspect)
  {
    return this.verificationTolerances[aspect].Tolerance() / length;
  }

  public bool ZeroSolidComparison(double val1, double val2, MKToleranceAspect aspect)
  {
    double tolerance = this.GetTolerance(aspect);
    return val1.ApproximatelyEquals(val2, tolerance);
  }

  private interface IToleranceSpec
  {
    double Tolerance();
  }

  private class FractionalTolerance : MKTolerances.IToleranceSpec
  {
    public FractionalRounding roundingFraction;
    public double fracTol;

    public FractionalTolerance(double tolerance, FractionalRounding rounding)
    {
      this.fracTol = tolerance;
      this.roundingFraction = rounding;
    }

    public FractionalTolerance(string tolerance)
    {
      string[] feetInchValue = FeetAndInchesRounding.readFeetInchValue(tolerance);
      if (feetInchValue[0] == null && feetInchValue[1] == null)
        this.fracTol = double.Epsilon;
      else
        this.fracTol = FeetAndInchesRounding.covertFeetInchToDouble(feetInchValue);
    }

    public double Tolerance() => this.fracTol;
  }

  private class DecimalTolerance : MKTolerances.IToleranceSpec
  {
    public double Tol;
    public int DecimalPlaces;

    public DecimalTolerance(double tolerance, int decimalPlaces)
    {
      this.Tol = tolerance;
      this.DecimalPlaces = decimalPlaces;
    }

    public DecimalTolerance(double tolerance) => this.Tol = tolerance;

    public double Tolerance() => this.Tol;
  }
}
