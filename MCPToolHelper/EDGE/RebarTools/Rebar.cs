// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.Rebar
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

#nullable disable
namespace EDGE.RebarTools;

public class Rebar
{
  public string BarDiameter;
  public double dblBarDiameter;
  public string BarShape;
  public string BarMark;
  public string BarGrade;
  public bool bWeldable;
  public bool bFinishMark_Galvanized;
  public bool bFinishMark_Epoxy;
  public Dictionary<BarSide, string> HashedBarDims;
  private string _dim_lengthString = "";
  private string _identityDescriptionLong;
  public ElementId Id;
  public string UniqueId;
  public bool bStandardRebar;
  public BarMarkInfo MarkInfo;
  private bool _bUseMetricCanadian;
  private bool _bUseMetricEuropean;
  private bool _bUseZeroPadding;
  private ManufacturerRebarSettings _rebarSettings;
  private bool _canParseBarMark;
  private ManufacturerKeying _bucketKeying;
  private ParameterMap _instanceParams;
  private ParameterMap _typeParams;
  private readonly FamilyInstance _rebarInstance;

  public string ManufactureComponent { get; private set; }

  public string IdentityDescriptionLong
  {
    get => $"{this._identityDescriptionLong} {this._dim_lengthString}";
    set
    {
      if (string.IsNullOrWhiteSpace(value))
        return;
      int num = value.ToUpper().LastIndexOf('X');
      if (num == -1)
        this._identityDescriptionLong = value + " X";
      else
        this._identityDescriptionLong = value.Substring(0, num + 1);
    }
  }

  public bool CanUseMarkInfo
  {
    get => this._canParseBarMark && this.MarkInfo != null && this.MarkInfo.IsValidParse;
  }

  public FamilyInstance FamilyInstance => this._rebarInstance;

  public Rebar(FamilyInstance rebarInstance, ManufacturerRebarSettings rebarSettings)
  {
    this._bUseMetricCanadian = rebarSettings.bUseMetricCanadian;
    this._bUseMetricEuropean = rebarSettings.bUseMetricEuropean;
    this._bUseZeroPadding = rebarSettings.useZeroPaddingForBarSize;
    this._bucketKeying = rebarSettings._bucketKeying;
    this._rebarSettings = rebarSettings;
    this._canParseBarMark = true;
    this._rebarInstance = rebarInstance;
    this._instanceParams = this._rebarInstance.ParametersMap;
    this._typeParams = this._rebarInstance.Symbol.ParametersMap;
    this.Init();
  }

  public Rebar(FamilyInstance rebarInstance, ManufacturerKeying keyType)
  {
    this._bucketKeying = keyType;
    this._rebarInstance = rebarInstance;
    this._instanceParams = this._rebarInstance.ParametersMap;
    this._typeParams = this._rebarInstance.Symbol.ParametersMap;
    this.Init();
  }

  public Rebar(
    Queue<string> standardBarInfo,
    ManufacturerKeying keyType,
    ManufacturerRebarSettings settings)
  {
    this.bStandardRebar = true;
    this._bucketKeying = keyType;
    this._rebarSettings = settings;
    this._bUseMetricEuropean = settings.bUseMetricEuropean;
    this.HashedBarDims = new Dictionary<BarSide, string>();
    string barDimension = standardBarInfo.Count == 14 ? standardBarInfo.Dequeue() : throw new Exception("Too many or too few parameters for rebar ctor");
    this.BarDiameter = barDimension;
    this.dblBarDiameter = BarHasher.ParseBarDim(barDimension);
    this.BarShape = standardBarInfo.Dequeue();
    this.BarMark = standardBarInfo.Dequeue();
    double barDim1 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim2 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim3 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim4 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim5 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim6 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim7 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim8 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim9 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim10 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double barDim11 = BarHasher.ParseBarDim(standardBarInfo.Dequeue());
    double lengthInFeet = barDim1 + barDim2 + barDim3 + barDim4 + barDim5 + barDim6 + barDim7 + barDim8 + barDim9 + barDim10 + barDim11;
    if (!this._bUseMetricEuropean)
    {
      this.HashedBarDims.Clear();
      this.HashedBarDims.Add(BarSide.BarLengthA, BarHasher.HashLength(barDim1, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthB, BarHasher.HashLength(barDim2, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthC, BarHasher.HashLength(barDim3, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthD, BarHasher.HashLength(barDim4, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthE, BarHasher.HashLength(barDim5, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthF, BarHasher.HashLength(barDim6, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthG, BarHasher.HashLength(barDim7, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthH, BarHasher.HashLength(barDim8, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthJ, BarHasher.HashLength(barDim9, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthK, BarHasher.HashLength(barDim10, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthO, BarHasher.HashLength(barDim11, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarAngleBc, BarHasher.HashAngle(0.0));
      this.HashedBarDims.Add(BarSide.BarAngleCd, BarHasher.HashAngle(0.0));
      this.HashedBarDims.Add(BarSide.BarAngleDe, BarHasher.HashAngle(0.0));
      if (this._bucketKeying == ManufacturerKeying.Rocky)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
      else if (this._bucketKeying == ManufacturerKeying.Kerkstra)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundUp));
      else if (this._bucketKeying == ManufacturerKeying.MidStates)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.MidStatesFeet_InchesToQuarter));
      else if (this._bucketKeying == ManufacturerKeying.Illini)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
      else if (this._bucketKeying == ManufacturerKeying.Tindall)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
      else if (this._bucketKeying == ManufacturerKeying.Spancrete)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundUp));
      else
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
    }
    else
    {
      this.HashedBarDims.Clear();
      this.HashedBarDims.Add(BarSide.BarLengthA, BarHasher.HashLength(barDim1, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthB, BarHasher.HashLength(barDim2, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthC, BarHasher.HashLength(barDim3, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthD, BarHasher.HashLength(barDim4, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthE, BarHasher.HashLength(barDim5, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthF, BarHasher.HashLength(barDim6, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthG, BarHasher.HashLength(barDim7, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthH, BarHasher.HashLength(barDim8, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthJ, BarHasher.HashLength(barDim9, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthK, BarHasher.HashLength(barDim10, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthO, BarHasher.HashLength(barDim11, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarAngleBc, BarHasher.HashAngle(0.0));
      this.HashedBarDims.Add(BarSide.BarAngleCd, BarHasher.HashAngle(0.0));
      this.HashedBarDims.Add(BarSide.BarAngleDe, BarHasher.HashAngle(0.0));
      this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.Millimeters));
    }
    if (this._bucketKeying != ManufacturerKeying.Kerkstra)
      return;
    BarMarkInfo barMarkInfo = new BarMarkInfo(this.BarMark, settings, this.BarShape);
    if (!barMarkInfo.IsValidParse)
      return;
    this.BarGrade = barMarkInfo.BarGrade;
    this.bWeldable = barMarkInfo.Weldable == "W";
    this.bFinishMark_Epoxy = barMarkInfo.FinishMark == "E";
    this.bFinishMark_Galvanized = barMarkInfo.FinishMark == "G";
  }

  private void Init()
  {
    this.bStandardRebar = false;
    this.HashedBarDims = new Dictionary<BarSide, string>();
    this.Update();
  }

  public void Update()
  {
    FamilySymbol symbol = this._rebarInstance.Symbol;
    this.BarDiameter = this.GetParamAsValueString("BAR_DIAMETER");
    this.dblBarDiameter = this.GetParameterAsZeroBasedDouble("BAR_DIAMETER");
    if (BarHasher.useMetricEuro)
    {
      this.dblBarDiameter = Math.Floor(UnitUtils.ConvertFromInternalUnits(this.GetParameterAsZeroBasedDouble("BAR_DIAMETER"), UnitTypeId.Millimeters));
      this.BarDiameter = this.dblBarDiameter.ToString() + "mm";
    }
    this.BarShape = this.GetParameterAsString("BAR_SHAPE");
    this.BarMark = this.GetParameterAsString("CONTROL_MARK");
    this.IdentityDescriptionLong = this.GetParameterAsString("IDENTITY_DESCRIPTION");
    if (this._canParseBarMark)
      this.MarkInfo = new BarMarkInfo(this.BarMark, this._rebarSettings, this.BarShape);
    this._dim_lengthString = this.GetParamAsValueString("DIM_LENGTH");
    if (BarHasher.useMetricEuro)
      this._dim_lengthString = UnitUtils.Convert(this.GetParameterAsZeroBasedDouble("DIM_LENGTH"), UnitTypeId.Feet, UnitTypeId.Millimeters).ToString() + "mm";
    this.bFinishMark_Galvanized = this.GetParameterAsBool("FINISH_GALVANIZED");
    this.bFinishMark_Epoxy = this.GetParameterAsBool("FINISH_EPOXY_COATED");
    this.bWeldable = this.GetParameterAsBool("REBAR_WELDABLE_MATERIAL");
    int num = this.GetParameterAsBool("REBAR_GRADE_40") ? 1 : 0;
    bool parameterAsBool = this.GetParameterAsBool("REBAR_GRADE_60");
    this.BarGrade = "60";
    if (num != 0 && !parameterAsBool)
      this.BarGrade = "40";
    this.Id = this._rebarInstance.Id;
    this.UniqueId = this._rebarInstance.UniqueId;
    this.FillDictioary();
  }

  private string GetParamAsValueString(string paramName)
  {
    Parameter parameter = this.GetParameter(paramName);
    return parameter != null ? parameter.AsValueString() : "";
  }

  private string GetParameterAsString(string paramName)
  {
    Parameter parameter = this.GetParameter(paramName);
    return parameter != null ? parameter.AsString() : "";
  }

  private double GetParameterAsZeroBasedDouble(string paramName)
  {
    Parameter parameter = this.GetParameter(paramName);
    return parameter != null ? parameter.AsDouble() : 0.0;
  }

  private bool GetParameterAsBool(string paramName)
  {
    Parameter parameter = this.GetParameter(paramName);
    return parameter != null && parameter.AsInteger() == 1;
  }

  private Parameter GetParameter(string paramName)
  {
    if (this._instanceParams.Contains(paramName))
      return this._instanceParams.get_Item(paramName);
    return !this._typeParams.Contains(paramName) ? (Parameter) null : this._typeParams.get_Item(paramName);
  }

  private void FillDictioary()
  {
    double asZeroBasedDouble1 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_A");
    double asZeroBasedDouble2 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_B");
    double asZeroBasedDouble3 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_C");
    double asZeroBasedDouble4 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_D");
    double asZeroBasedDouble5 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_E");
    double asZeroBasedDouble6 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_F");
    double asZeroBasedDouble7 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_G");
    double asZeroBasedDouble8 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_H");
    double asZeroBasedDouble9 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_J");
    double asZeroBasedDouble10 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_K");
    double asZeroBasedDouble11 = this.GetParameterAsZeroBasedDouble("BAR_LENGTH_O");
    if (BarHasher.useMetricEuro)
    {
      double lengthInFeet1 = UnitUtils.Convert(asZeroBasedDouble1, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet2 = UnitUtils.Convert(asZeroBasedDouble2, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet3 = UnitUtils.Convert(asZeroBasedDouble3, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet4 = UnitUtils.Convert(asZeroBasedDouble4, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet5 = UnitUtils.Convert(asZeroBasedDouble5, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet6 = UnitUtils.Convert(asZeroBasedDouble6, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet7 = UnitUtils.Convert(asZeroBasedDouble7, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet8 = UnitUtils.Convert(asZeroBasedDouble8, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet9 = UnitUtils.Convert(asZeroBasedDouble9, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet10 = UnitUtils.Convert(asZeroBasedDouble10, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet11 = UnitUtils.Convert(asZeroBasedDouble11, UnitTypeId.Feet, UnitTypeId.Millimeters);
      double lengthInFeet12 = lengthInFeet1 + lengthInFeet2 + lengthInFeet3 + lengthInFeet4 + lengthInFeet5 + lengthInFeet6 + lengthInFeet7 + lengthInFeet8 + lengthInFeet9 + lengthInFeet10 + lengthInFeet11;
      this.HashedBarDims.Clear();
      this.HashedBarDims.Add(BarSide.BarLengthA, BarHasher.HashLength(lengthInFeet1, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthB, BarHasher.HashLength(lengthInFeet2, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthC, BarHasher.HashLength(lengthInFeet3, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthD, BarHasher.HashLength(lengthInFeet4, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthE, BarHasher.HashLength(lengthInFeet5, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthF, BarHasher.HashLength(lengthInFeet6, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthG, BarHasher.HashLength(lengthInFeet7, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthH, BarHasher.HashLength(lengthInFeet8, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthJ, BarHasher.HashLength(lengthInFeet9, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthK, BarHasher.HashLength(lengthInFeet10, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarLengthO, BarHasher.HashLength(lengthInFeet11, LengthHashOption.Millimeters));
      this.HashedBarDims.Add(BarSide.BarAngleBc, BarHasher.HashAngle(this.GetParameterAsZeroBasedDouble("BAR_ANGLE_BC")));
      this.HashedBarDims.Add(BarSide.BarAngleCd, BarHasher.HashAngle(this.GetParameterAsZeroBasedDouble("BAR_ANGLE_CD")));
      this.HashedBarDims.Add(BarSide.BarAngleDe, BarHasher.HashAngle(this.GetParameterAsZeroBasedDouble("BAR_ANGLE_DE")));
      this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet12, LengthHashOption.Millimeters));
    }
    else
    {
      double lengthInFeet = asZeroBasedDouble1 + asZeroBasedDouble2 + asZeroBasedDouble3 + asZeroBasedDouble4 + asZeroBasedDouble5 + asZeroBasedDouble6 + asZeroBasedDouble7 + asZeroBasedDouble8 + asZeroBasedDouble9 + asZeroBasedDouble10 + asZeroBasedDouble11;
      this.HashedBarDims.Clear();
      this.HashedBarDims.Add(BarSide.BarLengthA, BarHasher.HashLength(asZeroBasedDouble1, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthB, BarHasher.HashLength(asZeroBasedDouble2, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthC, BarHasher.HashLength(asZeroBasedDouble3, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthD, BarHasher.HashLength(asZeroBasedDouble4, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthE, BarHasher.HashLength(asZeroBasedDouble5, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthF, BarHasher.HashLength(asZeroBasedDouble6, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthG, BarHasher.HashLength(asZeroBasedDouble7, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthH, BarHasher.HashLength(asZeroBasedDouble8, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthJ, BarHasher.HashLength(asZeroBasedDouble9, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthK, BarHasher.HashLength(asZeroBasedDouble10, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarLengthO, BarHasher.HashLength(asZeroBasedDouble11, LengthHashOption.FeetAndInchesToEights));
      this.HashedBarDims.Add(BarSide.BarAngleBc, BarHasher.HashAngle(this.GetParameterAsZeroBasedDouble("BAR_ANGLE_BC")));
      this.HashedBarDims.Add(BarSide.BarAngleCd, BarHasher.HashAngle(this.GetParameterAsZeroBasedDouble("BAR_ANGLE_CD")));
      this.HashedBarDims.Add(BarSide.BarAngleDe, BarHasher.HashAngle(this.GetParameterAsZeroBasedDouble("BAR_ANGLE_DE")));
      if (this._bucketKeying == ManufacturerKeying.Rocky)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
      else if (this._bucketKeying == ManufacturerKeying.Kerkstra)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundUp));
      else if (this._bucketKeying == ManufacturerKeying.MidStates)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.MidStatesFeet_InchesToQuarter));
      else if (this._bucketKeying == ManufacturerKeying.Illini)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
      else if (this._bucketKeying == ManufacturerKeying.Spancrete)
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundUp));
      else
        this.HashedBarDims.Add(BarSide.BarTotalLength, BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown));
    }
  }

  public bool IsModified(Rebar barToTest)
  {
    if (this.BarShape != barToTest.BarShape || !this.dblBarDiameter.ApproximatelyEqual(barToTest.dblBarDiameter))
      return true;
    foreach (BarSide key in this.HashedBarDims.Keys)
    {
      if (this._bucketKeying == ManufacturerKeying.Rocky || this._bucketKeying == ManufacturerKeying.Kerkstra || this._bucketKeying == ManufacturerKeying.Tindall || this._bucketKeying == ManufacturerKeying.MidStates || this._bucketKeying == ManufacturerKeying.Illini || this._bucketKeying == ManufacturerKeying.Spancrete)
      {
        if (barToTest.HashedBarDims.ContainsKey(key) && this.HashedBarDims[key] != barToTest.HashedBarDims[key])
          return true;
      }
      else if (this.HashedBarDims[key] != barToTest.HashedBarDims[key])
        return true;
    }
    if (this.CanUseMarkInfo && barToTest.CanUseMarkInfo)
    {
      if (this.MarkInfo.CoreMark != barToTest.MarkInfo.CoreMark)
        return true;
    }
    else if (this.BarMark != barToTest.BarMark)
      return true;
    return false;
  }
}
