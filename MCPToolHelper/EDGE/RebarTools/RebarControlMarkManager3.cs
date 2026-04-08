// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarControlMarkManager3
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.RebarTools;

public class RebarControlMarkManager3
{
  private Dictionary<string, Dictionary<string, string>> BarMarks;
  private Dictionary<string, List<int>> iUsedMarks;
  private Dictionary<string, int> ProjectBarMarkList;
  private Dictionary<string, RebarInfo> GuidToBarInfoMap;
  private Dictionary<string, ElementId> GuidToElementIdMap;
  private List<string> StandardBarMarks;
  private string documentName;
  public ManufacturerRebarSettings mfgSettingsData;
  private const string barMarkMatchPattern = "(?<coreMark>(?<strBarSize>\\d*)-(?<shape>[^-]*.*)-(?<total>\\d+)(?<controlMark>[A-DFH-IK-RT-Z]*))(?<finishMarks>[EGJS]*)";
  private Regex _markMatcher = new Regex("(?<coreMark>(?<strBarSize>\\d*)-(?<shape>[^-]*.*)-(?<total>\\d+)(?<controlMark>[A-DFH-IK-RT-Z]*))(?<finishMarks>[EGJS]*)", RegexOptions.Compiled);
  private static string[] aRockyAlphabet = new string[155]
  {
    "_",
    "A",
    "B",
    "C",
    "D",
    "F",
    "H",
    "I",
    "K",
    "L",
    "M",
    "N",
    "O",
    "P",
    "Q",
    "R",
    "T",
    "U",
    "V",
    "W",
    "X",
    "Y",
    "Z",
    "AA",
    "AB",
    "AC",
    "AD",
    "AF",
    "AH",
    "AI",
    "AK",
    "AL",
    "AM",
    "AN",
    "AO",
    "AP",
    "AQ",
    "AR",
    "AT",
    "AU",
    "AV",
    "AW",
    "AX",
    "AY",
    "AZ",
    "BA",
    "BB",
    "BC",
    "BD",
    "BF",
    "BH",
    "BI",
    "BK",
    "BL",
    "BM",
    "BN",
    "BO",
    "BP",
    "BQ",
    "BR",
    "BT",
    "BU",
    "BV",
    "BW",
    "BX",
    "BY",
    "BZ",
    "CA",
    "CB",
    "CC",
    "CD",
    "CF",
    "CH",
    "CI",
    "CK",
    "CL",
    "CM",
    "CN",
    "CO",
    "CP",
    "CQ",
    "CR",
    "CT",
    "CU",
    "CV",
    "CW",
    "CX",
    "CY",
    "CZ",
    "DA",
    "DB",
    "DC",
    "DD",
    "DF",
    "DH",
    "DI",
    "DK",
    "DL",
    "DM",
    "DN",
    "DO",
    "DP",
    "DQ",
    "DR",
    "DT",
    "DU",
    "DV",
    "DW",
    "DX",
    "DY",
    "DZ",
    "FA",
    "FB",
    "FC",
    "FD",
    "FF",
    "FH",
    "FI",
    "FK",
    "FL",
    "FM",
    "FN",
    "FO",
    "FP",
    "FQ",
    "FR",
    "FT",
    "FU",
    "FV",
    "FW",
    "FX",
    "FY",
    "FZ",
    "HA",
    "HB",
    "HC",
    "HD",
    "HF",
    "HH",
    "HI",
    "HK",
    "HL",
    "HM",
    "HN",
    "HO",
    "HP",
    "HQ",
    "HR",
    "HT",
    "HU",
    "HV",
    "HW",
    "HX",
    "HY",
    "HZ"
  };
  private static string[] aKerkstraAlphabet = new string[182]
  {
    "A",
    "B",
    "C",
    "D",
    "E",
    "F",
    "G",
    "H",
    "I",
    "J",
    "K",
    "L",
    "M",
    "N",
    "O",
    "P",
    "Q",
    "R",
    "S",
    "T",
    "U",
    "V",
    "W",
    "X",
    "Y",
    "Z",
    "AA",
    "AB",
    "AC",
    "AD",
    "AE",
    "AF",
    "AG",
    "AH",
    "AI",
    "AJ",
    "AK",
    "AL",
    "AM",
    "AN",
    "AO",
    "AP",
    "AQ",
    "AR",
    "AS",
    "AT",
    "AU",
    "AV",
    "AW",
    "AX",
    "AY",
    "AZ",
    "BA",
    "BB",
    "BC",
    "BD",
    "BE",
    "BF",
    "BG",
    "BH",
    "BI",
    "BJ",
    "BK",
    "BL",
    "BM",
    "BN",
    "BO",
    "BP",
    "BQ",
    "BR",
    "BS",
    "BT",
    "BU",
    "BV",
    "BW",
    "BX",
    "BY",
    "BZ",
    "CA",
    "CB",
    "CC",
    "CD",
    "CE",
    "CF",
    "CG",
    "CH",
    "CI",
    "CJ",
    "CK",
    "CL",
    "CM",
    "CN",
    "CO",
    "CP",
    "CQ",
    "CR",
    "CS",
    "CT",
    "CU",
    "CV",
    "CW",
    "CX",
    "CY",
    "CZ",
    "DA",
    "DB",
    "DC",
    "DD",
    "DE",
    "DF",
    "DG",
    "DH",
    "DI",
    "DJ",
    "DK",
    "DL",
    "DM",
    "DN",
    "DO",
    "DP",
    "DQ",
    "DR",
    "DS",
    "DT",
    "DU",
    "DV",
    "DW",
    "DX",
    "DY",
    "DZ",
    "EA",
    "EB",
    "EC",
    "ED",
    "EE",
    "EF",
    "EG",
    "EH",
    "EI",
    "EJ",
    "EK",
    "EL",
    "EM",
    "EN",
    "EO",
    "EP",
    "EQ",
    "ER",
    "ES",
    "ET",
    "EU",
    "EV",
    "EW",
    "EX",
    "EY",
    "EZ",
    "FA",
    "FB",
    "FC",
    "FD",
    "FE",
    "FF",
    "FG",
    "FH",
    "FI",
    "FJ",
    "FK",
    "FL",
    "FM",
    "FN",
    "FO",
    "FP",
    "FQ",
    "FR",
    "FS",
    "FT",
    "FU",
    "FV",
    "FW",
    "FX",
    "FY",
    "FZ"
  };
  private static readonly char[] aWellsChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
  private static readonly Dictionary<char, int> wellsDictionary = ((IEnumerable<char>) RebarControlMarkManager3.aWellsChars).Select((c, i) => new
  {
    Character = c,
    Index = i
  }).ToDictionary(c => c.Character, c => c.Index);

  private void Init()
  {
    this.BarMarks = new Dictionary<string, Dictionary<string, string>>();
    this.iUsedMarks = new Dictionary<string, List<int>>();
    this.ProjectBarMarkList = new Dictionary<string, int>();
    this.GuidToBarInfoMap = new Dictionary<string, RebarInfo>();
    this.GuidToElementIdMap = new Dictionary<string, ElementId>();
    this.StandardBarMarks = new List<string>();
  }

  public RebarControlMarkManager3() => this.Init();

  public RebarControlMarkManager3(string documentName, ManufacturerRebarSettings mfgSettingsData)
  {
    this.Init();
    this.documentName = documentName;
    this.mfgSettingsData = mfgSettingsData;
    foreach (Rebar standardBar in mfgSettingsData.StandardBars)
      this.PushStandardBar(standardBar);
  }

  public string GetBarMark(Rebar bar, bool bPushStandardBar = false, bool bBumpedBar = false, bool bAddedBar = false)
  {
    if (bar != null)
    {
      string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
      if (!BarDiameterOracle.IsValidDiameter(bar.dblBarDiameter))
      {
        this.HandleBarDeletion(bar.Id);
        return "invalidBarDiameter";
      }
      int num = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
      string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      string key1 = BarHasher.QuickHash(bar, this.mfgSettingsData._bucketKeying);
      if (!this.BarMarks.ContainsKey(rebarBucketKeyHash))
      {
        this.BarMarks.Add(rebarBucketKeyHash, new Dictionary<string, string>());
        this.iUsedMarks.Add(rebarBucketKeyHash, new List<int>());
      }
      if (!bPushStandardBar && !bBumpedBar && !bAddedBar)
        this.PopFromProjectMarkList(bar.Id);
      if (this.BarMarks.ContainsKey(rebarBucketKeyHash))
      {
        if (this.BarMarks[rebarBucketKeyHash].ContainsKey(key1))
        {
          if (bPushStandardBar)
          {
            string str = bar.BarMark;
            if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall && bar.BarMark.Length > 2 && (bar.BarMark[2].ToString() == "G" || bar.BarMark[2].ToString() == "E" || bar.BarMark[2].ToString() == "X"))
              str = bar.BarMark.Remove(2, 1);
            string key2 = BarHasher.OppHash(bar, this.mfgSettingsData._bucketKeying);
            this.BarMarks[rebarBucketKeyHash][key1] = bar.BarMark;
            if (this.BarMarks[rebarBucketKeyHash].ContainsKey(key2))
              this.BarMarks[rebarBucketKeyHash][key2] = str;
            else
              this.BarMarks[rebarBucketKeyHash].Add(key2, str);
            if (!this.StandardBarMarks.Contains(str))
              this.StandardBarMarks.Add(str);
            return this.AddFinishMarks(bar, this.BarMarks[rebarBucketKeyHash][key1]);
          }
          this.PushToGuidElementIdMap(bar);
          this.PushToProjectMarkList(this.BarMarks[rebarBucketKeyHash][key1], bar.UniqueId, num, strForRealDiameter, bar);
          return this.AddFinishMarks(bar, this.BarMarks[rebarBucketKeyHash][key1]);
        }
        string key3 = BarHasher.OppHash(bar, this.mfgSettingsData._bucketKeying);
        string str1 = bar.BarMark;
        if (bPushStandardBar)
        {
          if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall && bar.BarMark.Length > 2 && (bar.BarMark[2].ToString() == "G" || bar.BarMark[2].ToString() == "E" || bar.BarMark[2].ToString() == "X"))
            str1 = bar.BarMark.Remove(2, 1);
        }
        else
        {
          StringBuilder debugInfo = new StringBuilder();
          str1 = this.GetFormattedBarMark(this.GetAvailableMark(num, strForRealDiameter, rebarBucketKeyHash, bar, ref debugInfo), strForRealDiameter, bar);
          QA.DisplayRebarDebuggingInfo(debugInfo.ToString());
        }
        this.BarMarks[rebarBucketKeyHash].Add(key1, str1);
        if (key3 != key1)
          this.BarMarks[rebarBucketKeyHash].Add(key3, str1);
        if (bPushStandardBar)
        {
          this.StandardBarMarks.Add(str1);
        }
        else
        {
          this.PushToGuidElementIdMap(bar);
          this.PushToProjectMarkList(str1, bar.UniqueId, num, strForRealDiameter, bar);
        }
        return this.AddFinishMarks(bar, str1);
      }
    }
    return "ERROR! RebarControlMarkManager.cs:GetBarMark() Rebar bar was null.";
  }

  public void PushBarFromCentral(Rebar bar)
  {
    if (!BarDiameterOracle.IsValidDiameter(bar.dblBarDiameter))
      return;
    int ibarDiam = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
    string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
    string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
    if (!this.BarMarks.ContainsKey(rebarBucketKeyHash) || !this.BarMarks[rebarBucketKeyHash].ContainsKey(BarHasher.QuickHash(bar, this.mfgSettingsData._bucketKeying)))
      QA.InHouseMessage($"In PushBarFromCentral() we have a bar which is not in the dictionary! {bar.UniqueId} {bar.BarMark}");
    this.PushToGuidElementIdMap(bar);
    this.UpdateGuidToBarInfoMap(bar);
    if (bar.BarShape.ToUpper().Contains("STRAIGHT"))
      return;
    this.PushToProjectMarkList(bar.MarkInfo.CoreMark, bar.UniqueId, ibarDiam, strForRealDiameter, bar);
  }

  private string GetFormattedBarMark(int iAvailableMark, string strBarSize, Rebar bar = null)
  {
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Rocky)
      return $"{strBarSize}-{bar.BarShape}-{bar.HashedBarDims[BarSide.BarTotalLength]}" + this.GetAlphaForIntMark(iAvailableMark, ManufacturerKeying.Rocky);
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.MidStates)
      return $"{strBarSize}-{bar.HashedBarDims[BarSide.BarTotalLength]}-{bar.BarShape}" + this.GetAlphaForIntMark(iAvailableMark, ManufacturerKeying.MidStates);
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
    {
      string format = $"{{0:{this.mfgSettingsData.markDecimalPositionPrepend}}}";
      return this.mfgSettingsData.strMarkPrefix + strBarSize + bar.BarShape + string.Format(format, (object) iAvailableMark);
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Illini)
      return $"{strBarSize}-{bar.HashedBarDims[BarSide.BarTotalLength]}" + this.GetAlphaForIntMark(iAvailableMark, ManufacturerKeying.Illini);
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Spancrete)
    {
      this.GetKerkstraFinishMark(bar.bFinishMark_Epoxy, bar.bFinishMark_Galvanized);
      return $"{strBarSize}-{bar.HashedBarDims[BarSide.BarTotalLength]}" + this.GetAlphaForIntMark(iAvailableMark, ManufacturerKeying.Spancrete);
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.CoreslabONT)
    {
      string format = $"{{0:{this.mfgSettingsData.markDecimalPositionPrepend}}}";
      string canadian = BarDiameterOracle.BarNumberToCanadian(strBarSize, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      if (canadian == "0M")
        return "invalidBarDiameter";
      string strRootMark = $"{canadian}B{string.Format(format, (object) iAvailableMark)}";
      this.AddFinishMarks(bar, strRootMark);
      return strRootMark;
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Kerkstra)
    {
      string kerkstraFinishMark = this.GetKerkstraFinishMark(bar.bFinishMark_Epoxy, bar.bFinishMark_Galvanized);
      return $"{this.mfgSettingsData.strMarkPrefix}{strBarSize}{kerkstraFinishMark}-{bar.HashedBarDims[BarSide.BarTotalLength]}-{this.GetAlphaForIntMark(iAvailableMark, ManufacturerKeying.Kerkstra)}";
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Wells)
      return this.mfgSettingsData.strMarkPrefix + this.mfgSettingsData.ProjectNumber + this.GetAlphaForIntMark(iAvailableMark, ManufacturerKeying.Wells);
    string format1 = $"{{0:{this.mfgSettingsData.markDecimalPositionPrepend}}}";
    return this.mfgSettingsData.strMarkPrefix + strBarSize + string.Format(format1, (object) iAvailableMark) + this.mfgSettingsData.strMarkSuffix;
  }

  private string GetKerkstraFinishMark(bool bFinishMark_Epoxy, bool bFinishMark_Galvanized)
  {
    string kerkstraFinishMark = "n";
    if (!bFinishMark_Epoxy & bFinishMark_Galvanized)
      kerkstraFinishMark = this.mfgSettingsData.GalvanizeFinishMark;
    else if (bFinishMark_Epoxy && !bFinishMark_Galvanized)
      kerkstraFinishMark = this.mfgSettingsData.EpoxyFinishMark;
    return kerkstraFinishMark;
  }

  private string GetWellsFinishMark(bool bFinishMark_Epoxy, bool bFinishMark_Galvanized)
  {
    string wellsFinishMark = "B";
    if (!bFinishMark_Epoxy & bFinishMark_Galvanized)
      wellsFinishMark = this.mfgSettingsData.GalvanizeFinishMark;
    else if (bFinishMark_Epoxy && !bFinishMark_Galvanized)
      wellsFinishMark = this.mfgSettingsData.EpoxyFinishMark;
    return wellsFinishMark;
  }

  private string GetAlphaForIntMark(int markNumber, ManufacturerKeying keying)
  {
    int index = markNumber - 1;
    if ((keying == ManufacturerKeying.Rocky || keying == ManufacturerKeying.Kerkstra) && index == 0)
      return keying != ManufacturerKeying.Rocky ? "A" : "";
    if (keying == ManufacturerKeying.Rocky || keying == ManufacturerKeying.Kerkstra)
    {
      if (keying == ManufacturerKeying.Kerkstra && index < 0)
        index = 0;
      if (index <= (keying != ManufacturerKeying.Kerkstra ? RebarControlMarkManager3.aRockyAlphabet.Length : RebarControlMarkManager3.aKerkstraAlphabet.Length) - 1)
        return keying == ManufacturerKeying.Kerkstra ? RebarControlMarkManager3.aKerkstraAlphabet[index] : RebarControlMarkManager3.aRockyAlphabet[index];
    }
    else if (keying == ManufacturerKeying.MidStates)
    {
      if (index <= RebarControlMarkManager3.aRockyAlphabet.Length - 2)
        return "-" + RebarControlMarkManager3.aRockyAlphabet[index + 1];
    }
    else if (keying == ManufacturerKeying.Spancrete || keying == ManufacturerKeying.Illini)
    {
      if (index <= RebarControlMarkManager3.aRockyAlphabet.Length - 2)
        return RebarControlMarkManager3.aRockyAlphabet[index + 1];
    }
    else
    {
      if (keying == ManufacturerKeying.Wells)
      {
        int length = RebarControlMarkManager3.aWellsChars.Length;
        int num = markNumber;
        List<char> source = new List<char>();
        do
        {
          source.Add(RebarControlMarkManager3.aWellsChars[num % length]);
          num /= length;
        }
        while (num > 0);
        return string.Join<char>("", source.Reverse<char>());
      }
      TaskDialog.Show("EDGE Error", "Requested Rebar Mark Index is Out of Bounds");
    }
    return "_ERR_";
  }

  public static int GetIntForAlphaMark(string mark, ManufacturerKeying keying)
  {
    if (keying == ManufacturerKeying.Rocky)
    {
      if (string.IsNullOrWhiteSpace(mark))
        return 1;
      for (int index = 1; index < RebarControlMarkManager3.aRockyAlphabet.Length; ++index)
      {
        if (RebarControlMarkManager3.aRockyAlphabet[index] == mark)
          return index + 1;
      }
    }
    if (keying == ManufacturerKeying.MidStates || keying == ManufacturerKeying.Illini || keying == ManufacturerKeying.Spancrete)
    {
      if (string.IsNullOrWhiteSpace(mark))
        return 1;
      for (int intForAlphaMark = 1; intForAlphaMark < RebarControlMarkManager3.aRockyAlphabet.Length; ++intForAlphaMark)
      {
        if (RebarControlMarkManager3.aRockyAlphabet[intForAlphaMark] == mark)
          return intForAlphaMark;
      }
    }
    else
    {
      switch (keying)
      {
        case ManufacturerKeying.Kerkstra:
          for (int index = 0; index < RebarControlMarkManager3.aKerkstraAlphabet.Length; ++index)
          {
            if (RebarControlMarkManager3.aKerkstraAlphabet[index] == mark)
              return index + 1;
          }
          break;
        case ManufacturerKeying.Wells:
          int intForAlphaMark1 = 0;
          int length = RebarControlMarkManager3.aWellsChars.Length;
          char[] charArray = mark.ToCharArray();
          int index1 = 0;
          for (int y = charArray.Length - 1; y >= 0; --y)
          {
            if (RebarControlMarkManager3.wellsDictionary.ContainsKey(charArray[index1]))
            {
              int wells = RebarControlMarkManager3.wellsDictionary[charArray[index1]];
              intForAlphaMark1 += wells * (int) Math.Pow((double) length, (double) y);
            }
            ++index1;
          }
          return intForAlphaMark1;
      }
    }
    return 0;
  }

  public void PopFromProjectMarkList(ElementId barId)
  {
    if (QA.LogVerboseRebarInfo)
      QA.LogDemarcation($"Pop barId {barId?.ToString()} From Project Marks List");
    RebarInfo barInfo = (RebarInfo) null;
    if (this.GuidToElementIdMap != null && this.GuidToBarInfoMap != null)
    {
      List<KeyValuePair<string, ElementId>> list = this.GuidToElementIdMap.Where<KeyValuePair<string, ElementId>>((Func<KeyValuePair<string, ElementId>, bool>) (s => s.Value == barId)).ToList<KeyValuePair<string, ElementId>>();
      if (list.Count <= 1 && list.Count != 0)
      {
        string key = list.First<KeyValuePair<string, ElementId>>().Key;
        if (this.GuidToBarInfoMap.ContainsKey(key))
          barInfo = this.GuidToBarInfoMap[key];
      }
    }
    if (this.ProjectBarMarkList != null && this.GuidToElementIdMap != null && this.GuidToBarInfoMap != null && barInfo != null)
    {
      string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(barInfo);
      if (barInfo == null || !this.ProjectBarMarkList.ContainsKey(barInfo.BarMark) || this.ProjectBarMarkList[barInfo.BarMark] <= 0)
        return;
      if (this.ProjectBarMarkList[barInfo.BarMark] > 0)
        --this.ProjectBarMarkList[barInfo.BarMark];
      if (this.ProjectBarMarkList[barInfo.BarMark] != 0)
        return;
      if (this.BarMarks.ContainsKey(rebarBucketKeyHash))
      {
        foreach (KeyValuePair<string, string> keyValuePair in this.BarMarks[rebarBucketKeyHash].Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>) (s => s.Value == barInfo.BarMark)).ToList<KeyValuePair<string, string>>())
          this.BarMarks[rebarBucketKeyHash].Remove(keyValuePair.Key);
      }
      BarMarkInfo barMarkInfo = new BarMarkInfo(barInfo.BarMark, this.mfgSettingsData, barInfo.barShape);
      if (!this.iUsedMarks.ContainsKey(rebarBucketKeyHash))
        return;
      if (this.iUsedMarks[rebarBucketKeyHash].Contains(barMarkInfo.iMarkNumber))
        this.iUsedMarks[rebarBucketKeyHash].Remove(barMarkInfo.iMarkNumber);
      else
        QA.InHouseMessage($"iUsedMarks for iBarDiam: {barInfo.iBarDiam.ToString()} does not contain {barMarkInfo.iMarkNumber.ToString()}from mark: {barInfo.BarMark} {barMarkInfo.errorMessage}");
    }
    else
    {
      if (barInfo != null)
        return;
      if (QA.LogVerboseRebarInfo)
      {
        QA.LogLine("barInfo is null, is that right?");
      }
      else
      {
        if (!QA.LogVerboseRebarInfo)
          return;
        QA.LogLine("ProjectBarMarkList, GuidToElementIdMap, or GuidToBarMarkMap are null in PopFromProjectMarkList. is that OK?");
      }
    }
  }

  public void UpdateGuidToBarInfoMap(Rebar bar)
  {
    if (!BarDiameterOracle.IsValidDiameter(bar.dblBarDiameter))
      return;
    int ibarDiam = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
    string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
    if (bar.CanUseMarkInfo)
    {
      this.PushToGuidBarInfoMap(bar.UniqueId, bar.MarkInfo.CoreMark, ibarDiam, strForRealDiameter, bar);
    }
    else
    {
      string strippedMark = this.StripFinishMarks(bar.BarMark);
      this.PushToGuidBarInfoMap(bar.UniqueId, strippedMark, ibarDiam, strForRealDiameter, bar);
    }
  }

  public void UpdateGuidToBarInfoMap(Element barElem)
  {
    if (!(barElem is FamilyInstance))
      return;
    this.UpdateGuidToBarInfoMap(new Rebar(barElem as FamilyInstance, this.mfgSettingsData));
  }

  public void PushToGuidBarInfoMap(
    string guid,
    string strippedMark,
    int ibarDiam,
    string strBarDiam,
    Rebar bar)
  {
    if (this.GuidToBarInfoMap != null)
    {
      if (this.GuidToBarInfoMap.ContainsKey(guid))
        this.GuidToBarInfoMap[guid] = new RebarInfo(strippedMark, ibarDiam, strBarDiam, bar);
      else
        this.GuidToBarInfoMap.Add(guid, new RebarInfo(strippedMark, ibarDiam, strBarDiam, bar));
    }
    else
      QA.InHouseMessage("We have a problem here.  Why are we pushing to a null GUID to BarInfo map?");
  }

  public void UpdateGuidToElementIdMap(string guid, ElementId barId, string currentMark)
  {
    string str = this.StripFinishMarks(currentMark);
    if (this.GuidToElementIdMap != null)
      this.PushBarIdToGUIDElementIdMap(guid, barId);
    if (this.GuidToBarInfoMap == null || !this.GuidToBarInfoMap.ContainsKey(guid) || !(this.GuidToBarInfoMap[guid].BarMark != str))
      return;
    this.GuidToBarInfoMap[guid].BarMark = str;
  }

  private void PushBarIdToGUIDElementIdMap(string guid, ElementId barId)
  {
    if (this.GuidToElementIdMap.ContainsKey(guid))
    {
      if (!(this.GuidToElementIdMap[guid] != barId))
        return;
      this.GuidToElementIdMap[guid] = barId;
    }
    else
      this.GuidToElementIdMap.Add(guid, barId);
  }

  public void PushToProjectMarkList(
    string mark,
    string guid,
    int ibarDiam,
    string strBarDiam,
    Rebar bar,
    bool bPushNonConforming = false,
    bool baddedBar = true)
  {
    string strippedMark = "";
    bool flag = this.MarkIsStandardBarMark(mark, out strippedMark);
    string str = this.StripFinishMarks(mark);
    string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
    if (QA.LogVerboseRebarInfo)
      QA.LogDemarcation("Push To Project Mark List()");
    if (bPushNonConforming)
    {
      if (this.ProjectBarMarkList != null)
      {
        if (this.ProjectBarMarkList.ContainsKey(mark))
          ++this.ProjectBarMarkList[str];
        else
          this.ProjectBarMarkList.Add(mark, 2);
      }
      if (this.GuidToBarInfoMap == null)
        return;
      if (this.GuidToBarInfoMap.ContainsKey(guid))
        this.GuidToBarInfoMap[guid] = new RebarInfo(mark, ibarDiam, strBarDiam, bar);
      else
        this.GuidToBarInfoMap.Add(guid, new RebarInfo(mark, ibarDiam, strBarDiam, bar));
    }
    else
    {
      if (this.ProjectBarMarkList != null && !flag)
      {
        if (this.ProjectBarMarkList.ContainsKey(str))
          ++this.ProjectBarMarkList[str];
        else
          this.ProjectBarMarkList.Add(str, 1);
      }
      BarMarkInfo barMarkInfo = new BarMarkInfo(mark, this.mfgSettingsData, bar.BarShape);
      if (!flag && barMarkInfo.IsValidParse)
      {
        if (!this.iUsedMarks.ContainsKey(rebarBucketKeyHash))
          this.iUsedMarks.Add(rebarBucketKeyHash, new List<int>());
        if (!this.iUsedMarks[rebarBucketKeyHash].Contains(barMarkInfo.iMarkNumber))
          this.iUsedMarks[rebarBucketKeyHash].Add(barMarkInfo.iMarkNumber);
      }
      if (this.GuidToBarInfoMap == null)
        return;
      if (this.GuidToBarInfoMap.ContainsKey(guid))
        this.GuidToBarInfoMap[guid] = new RebarInfo(str, ibarDiam, strBarDiam, bar);
      else
        this.GuidToBarInfoMap.Add(guid, new RebarInfo(str, ibarDiam, strBarDiam, bar));
    }
  }

  public bool MarkIsStandardBarMark(string mark, out string strippedMark)
  {
    strippedMark = mark;
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
    {
      if (mark.Length <= 2)
        return this.StandardBarMarks.Contains(mark);
      if (mark[2].ToString() == this.mfgSettingsData.escEpoxFinish || mark[2].ToString() == this.mfgSettingsData.escGalvFinish || mark[2].ToString() == "X")
      {
        strippedMark = mark.Remove(2, 1);
        return this.StandardBarMarks.Contains(strippedMark);
      }
    }
    if (!new Regex($"(?:{this.mfgSettingsData.escEpoxFinish}|{this.mfgSettingsData.escGalvFinish})$").Match(mark).Success)
      return this.StandardBarMarks.Contains(mark);
    foreach (string standardBarMark in this.StandardBarMarks)
    {
      Regex regex = new Regex($"{standardBarMark}(?:{this.mfgSettingsData.escEpoxFinish}|{this.mfgSettingsData.escGalvFinish})*$");
      if (regex.Match(mark).Success)
      {
        foreach (Capture match in regex.Matches(mark))
        {
          if (match.Value.Equals(mark))
          {
            strippedMark = standardBarMark;
            return true;
          }
        }
      }
    }
    return false;
  }

  private int GetAvailableMark(
    int iBarDiam,
    string barSizeString,
    string bucketKey,
    Rebar bar,
    ref StringBuilder debugInfo)
  {
    if (!this.iUsedMarks.ContainsKey(bucketKey))
      this.iUsedMarks.Add(bucketKey, new List<int>());
    int minMarkNumber = this.mfgSettingsData.MinMarkNumber;
    bool flag1 = true;
    while (flag1)
    {
      string formattedBarMark = this.GetFormattedBarMark(minMarkNumber, barSizeString, bar);
      int num1 = this.iUsedMarks[bucketKey].Contains(minMarkNumber) ? 1 : 0;
      bool flag2 = this.BarMarks.ContainsKey(bucketKey) && this.BarMarks[bucketKey].Values.Contains<string>(formattedBarMark);
      if (formattedBarMark == "invalidBarDiameter")
        flag2 = false;
      int num2 = flag2 ? 1 : 0;
      if ((num1 | num2) != 0)
      {
        flag1 = true;
        ++minMarkNumber;
      }
      else
        flag1 = false;
    }
    this.iUsedMarks[bucketKey].Add(minMarkNumber);
    int availableMark = minMarkNumber;
    QA.LogVerboseRebarTrace($"------------------  GetAvailableMark(iBarDiam = {iBarDiam.ToString()}) == {availableMark.ToString()}");
    return availableMark;
  }

  internal void PushStandardBar(Rebar rebar) => this.GetBarMark(rebar, true);

  public void HandleBarDeletion(ElementId deletedElementId)
  {
    this.PopFromProjectMarkList(deletedElementId);
    if (this.GuidToElementIdMap == null)
      return;
    foreach (KeyValuePair<string, ElementId> keyValuePair in this.GuidToElementIdMap.Where<KeyValuePair<string, ElementId>>((Func<KeyValuePair<string, ElementId>, bool>) (s => s.Value == deletedElementId)).ToList<KeyValuePair<string, ElementId>>())
      this.CleanEntryFromGuidMaps(keyValuePair.Key);
  }

  public bool HandleBarDeletion(string guid)
  {
    if (this.GuidToElementIdMap == null || !this.GuidToElementIdMap.ContainsKey(guid))
      return false;
    this.HandleBarDeletion(this.GuidToElementIdMap[guid]);
    this.CleanEntryFromGuidMaps(guid);
    return true;
  }

  private void CleanEntryFromGuidMaps(string guid)
  {
    if (this.GuidToElementIdMap != null && this.GuidToElementIdMap.ContainsKey(guid))
      this.GuidToElementIdMap.Remove(guid);
    if (this.GuidToBarInfoMap == null || !this.GuidToBarInfoMap.ContainsKey(guid))
      return;
    this.GuidToBarInfoMap.Remove(guid);
  }

  public bool AddNewBarFromCentral(Rebar addedRebarBar, List<string> bumpedMarksList)
  {
    if (addedRebarBar.BarShape.Contains("STRAIGHT"))
      return true;
    List<string> stringList = new List<string>();
    QA.LogVerboseRebarTrace("               Processing RL Bar Mark: " + addedRebarBar.BarMark);
    Rebar rebar = addedRebarBar;
    BarMarkInfo barMarkInfo = (BarMarkInfo) null;
    barMarkInfo = !rebar.CanUseMarkInfo ? new BarMarkInfo(rebar.BarMark, this.mfgSettingsData, rebar.BarShape) : rebar.MarkInfo;
    if (!barMarkInfo.IsValidParse)
    {
      QA.LogError("MarkInfoFailedParse", $"               --BarMarkInfo for mark: {rebar.BarMark} uniqueId: {rebar.UniqueId} elemId: {rebar.Id?.ToString()} diameter: {rebar.dblBarDiameter.ToString()} Could not be parsed.  This will cause issues in Worksharing rebar sync");
      return false;
    }
    if (this.StandardBarMarks.Contains(barMarkInfo.CoreMark))
    {
      QA.LogVerboseRebarTrace("               --Added Bar Mark is a standard bar mark: " + barMarkInfo.CoreMark);
      return true;
    }
    if (!BarDiameterOracle.IsValidDiameter(rebar.dblBarDiameter))
      return false;
    int barSizeNumber = BarDiameterOracle.ResolveBarDiam(rebar.dblBarDiameter);
    string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(addedRebarBar);
    switch (this.GetListMembership(barSizeNumber, rebar, barMarkInfo.CoreMark))
    {
      case RebarControlMarkManager3.ListMembershipCheck.GeometryExistsAndMarkIsTheSame:
        return true;
      case RebarControlMarkManager3.ListMembershipCheck.GeometryOrMarkExists:
        QA.LogVerboseRebarTrace($"               --Added Bar Mark: {barMarkInfo.CoreMark} Mark or geom exists, bumping bar mark.");
        bumpedMarksList.Add(barMarkInfo.CoreMark);
        stringList.Add(barMarkInfo.CoreMark);
        if (this.BarMarks.ContainsKey(rebarBucketKeyHash))
        {
          foreach (string bothBarHash in BarHasher.GetBothBarHashes(rebar, this.mfgSettingsData._bucketKeying))
          {
            if (this.BarMarks[rebarBucketKeyHash].ContainsKey(bothBarHash) && !bumpedMarksList.Contains(this.BarMarks[rebarBucketKeyHash][bothBarHash]))
            {
              bumpedMarksList.Add(this.BarMarks[rebarBucketKeyHash][bothBarHash]);
              stringList.Add(this.BarMarks[rebarBucketKeyHash][bothBarHash]);
            }
            this.BarMarks[rebarBucketKeyHash].Remove(bothBarHash);
          }
          if (this.BarMarks[rebarBucketKeyHash].Values.Contains<string>(barMarkInfo.CoreMark))
          {
            foreach (KeyValuePair<string, string> keyValuePair in this.BarMarks[rebarBucketKeyHash].Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>) (kvp => kvp.Value == barMarkInfo.CoreMark)).ToList<KeyValuePair<string, string>>())
              this.BarMarks[rebarBucketKeyHash].Remove(keyValuePair.Key);
            if (!bumpedMarksList.Contains(barMarkInfo.CoreMark))
            {
              bumpedMarksList.Add(barMarkInfo.CoreMark);
              stringList.Add(barMarkInfo.CoreMark);
            }
          }
        }
        foreach (string key in stringList)
        {
          if (this.ProjectBarMarkList.ContainsKey(key))
            this.ProjectBarMarkList[key] = 0;
          else
            QA.LogVerboseRebarTrace($"               --Added bar {barMarkInfo.CoreMark} was in dictionary but not in project mark list!  Something is wrong here.");
        }
        using (List<string>.Enumerator enumerator = stringList.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            string current = enumerator.Current;
            BarMarkInfo barMarkInfo1 = !(current == barMarkInfo.CoreMark) ? new BarMarkInfo(current, this.mfgSettingsData, barMarkInfo.Bar.BarShape) : barMarkInfo;
            if (this.iUsedMarks.ContainsKey(rebarBucketKeyHash))
            {
              if (this.iUsedMarks[rebarBucketKeyHash].Contains(barMarkInfo1.iMarkNumber))
                this.iUsedMarks[rebarBucketKeyHash].Remove(barMarkInfo1.iMarkNumber);
              else
                QA.LogVerboseRebarTrace($"Attempting to remove iUsedMark {barMarkInfo1.iMarkNumber.ToString()} for bar size number: {barSizeNumber.ToString()} but iBarSize does not exist in iUsedMarks.  Why?");
            }
          }
          break;
        }
    }
    if (!this.BarMarks.ContainsKey(rebarBucketKeyHash))
      this.BarMarks.Add(rebarBucketKeyHash, new Dictionary<string, string>());
    foreach (string bothBarHash in BarHasher.GetBothBarHashes(rebar, this.mfgSettingsData._bucketKeying))
      this.BarMarks[rebarBucketKeyHash].Add(bothBarHash, barMarkInfo.CoreMark);
    return true;
  }

  private RebarControlMarkManager3.ListMembershipCheck GetListMembership(
    int barSizeNumber,
    Rebar barInfo,
    string coreMark)
  {
    List<string> bothBarHashes = BarHasher.GetBothBarHashes(barInfo, this.mfgSettingsData._bucketKeying);
    string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(barInfo);
    if (this.BarMarks.ContainsKey(rebarBucketKeyHash))
    {
      foreach (string key in bothBarHashes)
      {
        if (this.BarMarks[rebarBucketKeyHash].ContainsKey(key))
        {
          if (this.BarMarks[rebarBucketKeyHash][key] == coreMark)
          {
            QA.LogVerboseRebarTrace("                                --ListMembership: Mark and Geometry are the Same");
            return RebarControlMarkManager3.ListMembershipCheck.GeometryExistsAndMarkIsTheSame;
          }
          QA.LogVerboseRebarTrace("                                --ListMembership: BarMarks Geometry Exists");
          return RebarControlMarkManager3.ListMembershipCheck.GeometryOrMarkExists;
        }
      }
      if (this.BarMarks[rebarBucketKeyHash].Values.Contains<string>(coreMark))
      {
        QA.LogVerboseRebarTrace("                                    --ListMembership: BarMarks Mark Exists");
        return RebarControlMarkManager3.ListMembershipCheck.GeometryOrMarkExists;
      }
      QA.LogVerboseRebarTrace("                                    --ListMembership: Neither mark nor geometry exist");
      return RebarControlMarkManager3.ListMembershipCheck.NeitherMarkNorGeometryExist;
    }
    QA.LogVerboseRebarTrace("                                    --ListMembership: Neither mark nor geometry exist because dictionary doesn't have this bar size");
    return RebarControlMarkManager3.ListMembershipCheck.NeitherMarkNorGeometryExist;
  }

  public void HandleUndoRedoModify(Rebar bar)
  {
    if (!BarDiameterOracle.IsValidDiameter(bar.dblBarDiameter))
      return;
    int num = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
    string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
    BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
    string key1 = BarHasher.QuickHash(bar, this.mfgSettingsData._bucketKeying);
    string key2 = BarHasher.OppHash(bar, this.mfgSettingsData._bucketKeying);
    if (this.BarMarks.ContainsKey(rebarBucketKeyHash))
    {
      if (this.BarMarks[rebarBucketKeyHash].ContainsKey(key1))
        return;
      List<string> stringList = new List<string>();
      foreach (KeyValuePair<string, string> keyValuePair in this.BarMarks[rebarBucketKeyHash])
      {
        if (keyValuePair.Value == bar.MarkInfo.CoreMark && !(keyValuePair.Key == key1) && !(keyValuePair.Key == key1))
          stringList.Add(keyValuePair.Key);
      }
      foreach (string key3 in stringList)
        this.BarMarks[rebarBucketKeyHash].Remove(key3);
      this.BarMarks[rebarBucketKeyHash].Add(key1, bar.MarkInfo.CoreMark);
      if (!(key2 != key1))
        return;
      this.BarMarks[rebarBucketKeyHash].Add(key2, bar.MarkInfo.CoreMark);
    }
    else
    {
      QA.InHouseMessage("UndoRedo but bar marks doesn't contain iBarDiam: " + num.ToString());
      QA.LogVerboseRebarTrace("UndoRedo but bar marks doesn't contain iBarDiam: " + num.ToString());
    }
  }

  internal bool CatalogExistingBar(
    Rebar bar,
    ref CatalogAction action,
    out string newMark,
    out string ErrorMessage)
  {
    ErrorMessage = "Null Bar in CatalogExistingBar()";
    newMark = "ERROR: NEW MARK NOT SET IN RebarControlMarkManager.cs:CatalogExistingBar()";
    if (bar == null)
      return false;
    if (!BarDiameterOracle.IsValidDiameter(bar.dblBarDiameter))
    {
      ErrorMessage = $"Bar ID {bar.Id?.ToString()} has an unsupported diameter.";
      return false;
    }
    ErrorMessage = "failed to catalog bar in CatalogExistingBar()";
    int num = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
    string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
    string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
    string key1 = BarHasher.QuickHash(bar, this.mfgSettingsData._bucketKeying);
    this.PushToGuidElementIdMap(bar);
    this.PushToGuidBarInfoMap(bar.UniqueId, this.StripFinishMarks(bar.BarMark), num, strForRealDiameter, bar);
    if (!this.BarMarks.ContainsKey(rebarBucketKeyHash))
      this.BarMarks.Add(rebarBucketKeyHash, new Dictionary<string, string>());
    if (!this.BarMarks.ContainsKey(rebarBucketKeyHash))
      throw new Exception("Dictionary broken in CatalogExistingBar()");
    if (this.BarMarks[rebarBucketKeyHash].ContainsKey(key1))
    {
      string strRootMark = this.StripFinishMarks(bar.BarMark);
      string str = this.AddFinishMarks(bar, strRootMark);
      bool flag = bar.BarMark != str;
      if (strRootMark != this.BarMarks[rebarBucketKeyHash][key1] | flag)
      {
        TaskDialogResult taskDialogResult;
        if ((action & CatalogAction.ReplaceWithDictionaryMarkForALL) == CatalogAction.ReplaceWithDictionaryMarkForALL)
        {
          taskDialogResult = (TaskDialogResult) 1002;
        }
        else
        {
          TaskDialog taskDialog = new TaskDialog("Bar Mark Mis-match");
          taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog.AllowCancellation = false;
          taskDialog.MainInstruction = $"Bar Mark {bar.BarMark} for Element {bar.Id?.ToString()}:  does not match dictionary mark: {this.AddFinishMarks(bar, this.BarMarks[rebarBucketKeyHash][key1])}";
          taskDialog.MainContent = " NOTE: choosing to do nothing will result in an invalid bar mark in the model and will be subject to update if this bar is modified in the future.";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Replace mark with dictionary listing: " + this.AddFinishMarks(bar, this.BarMarks[rebarBucketKeyHash][key1]));
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Replace mark with Dictionary Setting For All Bars", "Do this for all mark mis-matches to ensure dictionary integrity and don't ask me about individual marks");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "Disable Bar Mark Automation for This Document", "Temporarily disable mark automation allows you to investigate and resolve issues, preserving your marks.  Close and reopen this document to start mark automation again");
          taskDialogResult = taskDialog.Show();
        }
        if (taskDialogResult == 1001 || taskDialogResult == 1002)
        {
          this.PushToProjectMarkList(this.BarMarks[rebarBucketKeyHash][key1], bar.UniqueId, num, strForRealDiameter, bar);
          bar.BarMark = this.AddFinishMarks(bar, this.BarMarks[rebarBucketKeyHash][key1]);
          newMark = bar.BarMark;
          if (taskDialogResult == 1001)
            action |= CatalogAction.ReplaceWithDictionaryMark;
          else
            action |= CatalogAction.ReplaceWithDictionaryMarkForALL;
          action |= CatalogAction.UpdateWithNewMarkNumber;
          return true;
        }
        ErrorMessage = $"found hash for bar mark: {bar.BarMark} in dictionary but dictionary mark is: {this.BarMarks[rebarBucketKeyHash][key1]} ElementId: {bar.Id.ToString()}";
        action = CatalogAction.DisableBarMarkAutomation;
        return false;
      }
      this.PushToProjectMarkList(this.BarMarks[rebarBucketKeyHash][key1], bar.UniqueId, num, strForRealDiameter, bar);
      return true;
    }
    string errorMsg = "";
    if (!this.IsConformingMark(bar, out errorMsg))
    {
      if (!action.HasFlag((Enum) CatalogAction.CreateConformingMarkForALL) && !action.HasFlag((Enum) CatalogAction.CatalogMarkAsIsForALL))
      {
        TaskDialog taskDialog1 = new TaskDialog("Non-standard Bar Mark");
        taskDialog1.AllowCancellation = false;
        taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog1.MainInstruction = $"Bar Mark for Element {bar.Id?.ToString()}: {bar.BarMark} is non-standard per current Rebar Settings.  Do you want to create a standard mark or skip?";
        taskDialog1.MainContent = "Detailed error: " + errorMsg;
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Create a New Standard Bar Mark e.g., " + this.mfgSettingsData.GetSampleMark(bar));
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Catalog This Mark As Is", "This mark will be cataloged and used anytime similar bars are found in the future");
        taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Disable Mark Automation For This Document", "Temporarily disable mark automation allows you to investigate and resolve issues, preserving your marks.  Close and reopen this document to start mark automation again");
        TaskDialogResult taskDialogResult = taskDialog1.Show();
        if (taskDialogResult == 1001)
        {
          TaskDialog taskDialog2 = new TaskDialog("Repeat Action?");
          taskDialog2.AllowCancellation = false;
          taskDialog2.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog2.MainInstruction = "Apply this action for all similar situations?";
          taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1001, "No, Only apply to this bar", "Prompt me for each bar in the model");
          taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1002, "Do this for All", "Don't ask me again for this document");
          if (taskDialog2.Show() == 1001)
            action |= CatalogAction.CreateConformingMark;
          else
            action |= CatalogAction.CreateConformingMarkForALL;
        }
        else if (taskDialogResult == 1002)
        {
          TaskDialog taskDialog3 = new TaskDialog("Repeat Action?");
          taskDialog3.AllowCancellation = false;
          taskDialog3.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog3.MainInstruction = "Apply this action for all similar situations?";
          taskDialog3.AddCommandLink((TaskDialogCommandLinkId) 1001, "No, Only apply to this bar", "Prompt me for each bar in the model");
          taskDialog3.AddCommandLink((TaskDialogCommandLinkId) 1002, "Do this for All", "Don't ask me again for this document");
          if (taskDialog3.Show() == 1001)
            action |= CatalogAction.CatalogMarkAsIs;
          else
            action |= CatalogAction.CatalogMarkAsIsForALL;
        }
        else
        {
          ErrorMessage = "Found Non-conforming mark, user opted to disable rebar automation";
          action = CatalogAction.DisableBarMarkAutomation;
          return false;
        }
      }
      if (action.HasFlag((Enum) CatalogAction.CreateConformingMark) || action.HasFlag((Enum) CatalogAction.CreateConformingMarkForALL))
      {
        bar.BarMark = this.GetBarMark(bar);
        newMark = bar.BarMark;
        action |= CatalogAction.UpdateWithNewMarkNumber;
        return true;
      }
      if (action.HasFlag((Enum) CatalogAction.CatalogMarkAsIs) || action.HasFlag((Enum) CatalogAction.CatalogMarkAsIsForALL))
      {
        string key2 = BarHasher.OppHash(bar, this.mfgSettingsData._bucketKeying);
        this.BarMarks[rebarBucketKeyHash].Add(key1, bar.BarMark);
        if (key2 != key1)
          this.BarMarks[rebarBucketKeyHash].Add(key2, bar.BarMark);
        this.PushToProjectMarkList(bar.BarMark, bar.UniqueId, num, strForRealDiameter, bar, true);
        action &= ~CatalogAction.UpdateWithNewMarkNumber;
        return true;
      }
    }
    string key3 = BarHasher.OppHash(bar, this.mfgSettingsData._bucketKeying);
    BarMarkInfo barMarkInfo = new BarMarkInfo(bar.BarMark, this.mfgSettingsData, bar.BarShape);
    int iAvailableMark = 0;
    bool flag1 = false;
    StringBuilder debugInfo = new StringBuilder();
    debugInfo.AppendLine($"Checking used mark list Availability for {bar.BarMark} number: {barMarkInfo.strMarkNumber} msg: {barMarkInfo.errorMessage}");
    if (this.iUsedMarks.ContainsKey(rebarBucketKeyHash))
    {
      if (this.iUsedMarks[rebarBucketKeyHash].Contains(barMarkInfo.iMarkNumber))
      {
        bool flag2 = false;
        if (App.SpecialRebarScenario1)
        {
          if (new TaskDialog("EDGE^R")
          {
            MainInstruction = "Special Rebar Situation #1",
            MainContent = "Only Answer Yes with extreme caution",
            CommonButtons = ((TaskDialogCommonButtons) 6)
          }.Show() == 6)
            flag2 = true;
        }
        if (!(App.SpecialRebarScenario1 & flag2))
        {
          debugInfo.AppendLine(" this bar mark integer has already been used!  Getting a new mark for mark: " + bar.BarMark);
          iAvailableMark = this.GetAvailableMark(num, strForRealDiameter, rebarBucketKeyHash, bar, ref debugInfo);
          flag1 = true;
        }
      }
      else
        this.iUsedMarks[rebarBucketKeyHash].Add(barMarkInfo.iMarkNumber);
    }
    else
    {
      this.iUsedMarks.Add(rebarBucketKeyHash, new List<int>());
      this.iUsedMarks[rebarBucketKeyHash].Add(barMarkInfo.iMarkNumber);
      debugInfo.AppendLine($"we're looking for a used mark for bar diam {num.ToString()} but iUsedMarks does not have iBarDiam Key!.  why?");
    }
    newMark = !flag1 ? this.GetFormattedBarMark(barMarkInfo.iMarkNumber, strForRealDiameter, bar) : this.GetFormattedBarMark(iAvailableMark, strForRealDiameter, bar);
    debugInfo.AppendLine(" Mark that will be logged in Dictionary: " + newMark);
    QA.DisplayRebarDebuggingInfo(debugInfo.ToString());
    this.BarMarks[rebarBucketKeyHash].Add(key1, newMark);
    if (key3 != key1)
      this.BarMarks[rebarBucketKeyHash].Add(key3, newMark);
    this.PushToProjectMarkList(newMark, bar.UniqueId, num, strForRealDiameter, bar);
    newMark = this.AddFinishMarks(bar, newMark);
    action |= CatalogAction.UpdateWithNewMarkNumber;
    return true;
  }

  public void PushToGuidElementIdMap(Rebar bar)
  {
    if (this.GuidToElementIdMap.ContainsKey(bar.UniqueId))
      this.GuidToElementIdMap[bar.UniqueId] = bar.Id;
    else
      this.GuidToElementIdMap.Add(bar.UniqueId, bar.Id);
  }

  public void PushToGuidElementIdMap(string uniqueID, ElementId elemId)
  {
    if (this.GuidToElementIdMap.ContainsKey(uniqueID))
      this.GuidToElementIdMap[uniqueID] = elemId;
    else
      this.GuidToElementIdMap.Add(uniqueID, elemId);
  }

  public string GetStraightBarMark(Rebar rebarBar)
  {
    string straightBarMark = "HASH ERROR: RebarControlMarkManager.cs:GetStraightBarMark()";
    Element familyInstance = (Element) rebarBar.FamilyInstance;
    Parameter parameter = Parameters.LookupParameter(familyInstance, "DIM_LENGTH");
    if (parameter == null)
      return straightBarMark;
    double lengthInFeet = parameter.AsDouble();
    bool finishMarkGalvanized = rebarBar.bFinishMark_Galvanized;
    bool bFinishMarkEpoxy = rebarBar.bFinishMark_Epoxy;
    string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(rebarBar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Rocky)
      return Oracle.RebarIsRockySpliceBar(familyInstance) ? $"{this.mfgSettingsData.strStraightBarPrefix}{strForRealDiameter}-{BarHasher.HashLength(lengthInFeet, LengthHashOption.RockyFeetAndInchesToQuarter, this.mfgSettingsData.straightLengthZeroPrepend)}" + (finishMarkGalvanized ? this.mfgSettingsData.GalvanizeFinishMark : "") + (bFinishMarkEpoxy ? this.mfgSettingsData.EpoxyFinishMark : "") : $"{this.mfgSettingsData.strStraightBarPrefix}{strForRealDiameter}-{BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown, this.mfgSettingsData.straightLengthZeroPrepend)}" + (finishMarkGalvanized ? this.mfgSettingsData.GalvanizeFinishMark : "") + (bFinishMarkEpoxy ? this.mfgSettingsData.EpoxyFinishMark : "");
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.MidStates)
      return $"{this.mfgSettingsData.strStraightBarPrefix}{strForRealDiameter}-{BarHasher.HashLength(lengthInFeet, LengthHashOption.MidStatesFeetAndInches, this.mfgSettingsData.straightLengthZeroPrepend)}" + (finishMarkGalvanized ? this.mfgSettingsData.GalvanizeFinishMark : "") + (bFinishMarkEpoxy ? this.mfgSettingsData.EpoxyFinishMark : "");
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
    {
      string str = "X";
      if (!(bFinishMarkEpoxy & finishMarkGalvanized))
      {
        if (bFinishMarkEpoxy)
          str = "E";
        else if (finishMarkGalvanized)
          str = "G";
      }
      return this.mfgSettingsData.strStraightBarPrefix + str + strForRealDiameter + BarHasher.HashLength(lengthInFeet, LengthHashOption.Inches, this.mfgSettingsData.straightLengthZeroPrepend);
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Illini)
      return $"{this.mfgSettingsData.strStraightBarPrefix}{strForRealDiameter}-{BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown, this.mfgSettingsData.straightLengthZeroPrepend)}" + (finishMarkGalvanized ? this.mfgSettingsData.GalvanizeFinishMark : "") + (bFinishMarkEpoxy ? this.mfgSettingsData.EpoxyFinishMark : "");
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Spancrete)
    {
      string kerkstraFinishMark = this.GetKerkstraFinishMark(bFinishMarkEpoxy, finishMarkGalvanized);
      return $"{this.mfgSettingsData.strStraightBarPrefix}{strForRealDiameter}{(kerkstraFinishMark == "N" ? "" : kerkstraFinishMark)}-{BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundUp, this.mfgSettingsData.straightLengthZeroPrepend)}";
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Kerkstra)
    {
      int num = rebarBar.bWeldable ? 1 : 0;
      string kerkstraFinishMark = this.GetKerkstraFinishMark(bFinishMarkEpoxy, finishMarkGalvanized);
      return $"{this.mfgSettingsData.strStraightBarPrefix}{strForRealDiameter}{kerkstraFinishMark.ToLower()}-{BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown, this.mfgSettingsData.straightLengthZeroPrepend)}";
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Wells)
    {
      string str = "B";
      if (bFinishMarkEpoxy & finishMarkGalvanized)
        str = "B";
      else if (!bFinishMarkEpoxy && !finishMarkGalvanized)
        str = "B";
      else if (bFinishMarkEpoxy)
        str = this.mfgSettingsData.EpoxyFinishMark;
      else if (finishMarkGalvanized)
        str = this.mfgSettingsData.GalvanizeFinishMark;
      return this.mfgSettingsData.strStraightBarPrefix + str + this.mfgSettingsData.ProjectNumber + BarHasher.HashLength(lengthInFeet, this.mfgSettingsData.straightHashOption, this.mfgSettingsData.straightLengthZeroPrepend);
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.CoreslabONT)
    {
      string canadian = BarDiameterOracle.BarNumberToCanadian(strForRealDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      if (canadian == "0M")
        return "invalidBarDiameter";
      string strRootMark = canadian + BarHasher.HashLength(lengthInFeet, LengthHashOption.FeetAndInches_RoundDown, this.mfgSettingsData.straightLengthZeroPrepend);
      return this.AddFinishMarks(rebarBar, strRootMark);
    }
    if (this.mfgSettingsData.straightHashOption == LengthHashOption.Centimeters)
      lengthInFeet = UnitUtils.ConvertFromInternalUnits(lengthInFeet, UnitTypeId.Centimeters);
    else if (this.mfgSettingsData.straightHashOption == LengthHashOption.Meters)
      lengthInFeet = UnitUtils.ConvertFromInternalUnits(lengthInFeet, UnitTypeId.Meters);
    else if (this.mfgSettingsData.bUseMetricEuropean)
      lengthInFeet = UnitUtils.ConvertFromInternalUnits(lengthInFeet, UnitTypeId.Millimeters);
    return this.mfgSettingsData.strStraightBarPrefix + strForRealDiameter + BarHasher.HashLength(lengthInFeet, this.mfgSettingsData.straightHashOption, this.mfgSettingsData.straightLengthZeroPrepend) + (finishMarkGalvanized ? this.mfgSettingsData.GalvanizeFinishMark : "") + (bFinishMarkEpoxy ? this.mfgSettingsData.EpoxyFinishMark : "");
  }

  public string AddFinishMarks(Rebar bar, string strRootMark)
  {
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Spancrete)
    {
      string str1 = "";
      if (bar.bFinishMark_Epoxy && bar.bFinishMark_Galvanized)
        str1 = "N";
      else if (!bar.bFinishMark_Epoxy && !bar.bFinishMark_Galvanized)
        str1 = "N";
      else if (bar.bFinishMark_Epoxy)
        str1 = "E";
      else if (bar.bFinishMark_Galvanized)
        str1 = "G";
      string strippedMark = "";
      if (this.MarkIsStandardBarMark(strRootMark, out strippedMark))
        return strippedMark + (str1 == "N" ? "" : str1);
      Match match = new Regex("(?<coreMark>(?<strBarSize>\\d*)(?<finishMarks>[EGJS]?)-(?<total>\\d+)(?<controlMark>[A-Z-[EGJS]]*))").Match(strRootMark);
      if (!match.Success)
        return strRootMark + (str1 == "N" ? "" : str1);
      string str2 = match.Groups["strBarSize"].Value;
      string str3 = match.Groups["total"].Value;
      string str4 = match.Groups["controlMark"].Value;
      return $"{str2}{(str1 == "N" ? "" : str1)}-{str3}{str4}";
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Kerkstra)
      return strRootMark;
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Wells)
    {
      string str5 = "B";
      if (bar.bFinishMark_Epoxy && bar.bFinishMark_Galvanized)
        str5 = this.mfgSettingsData.GalvanizeFinishMark;
      else if (!bar.bFinishMark_Epoxy && !bar.bFinishMark_Galvanized)
        str5 = "B";
      else if (bar.bFinishMark_Epoxy)
        str5 = this.mfgSettingsData.EpoxyFinishMark;
      else if (bar.bFinishMark_Galvanized)
        str5 = this.mfgSettingsData.GalvanizeFinishMark;
      string escPrefix = this.mfgSettingsData.escPrefix;
      string escGalvFinish = this.mfgSettingsData.escGalvFinish;
      string escEpoxFinish = this.mfgSettingsData.escEpoxFinish;
      string escProjectNumber = this.mfgSettingsData.escProjectNumber;
      string strippedMark = "";
      Match match = new Regex($"(?<prefix>{escPrefix}){escProjectNumber}(?<markAlpha>[A-Z]*)").Match(strRootMark);
      if (match.Success)
      {
        string str6 = match.Groups["markAlpha"].Value;
        return this.mfgSettingsData.strMarkPrefix + str5 + this.mfgSettingsData.ProjectNumber + str6;
      }
      return this.MarkIsStandardBarMark(strRootMark, out strippedMark) ? strRootMark + (str5 == "B" ? "" : str5) : "<error adding finish marks>";
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
    {
      string str = "X";
      if (!bar.bFinishMark_Epoxy || !bar.bFinishMark_Galvanized)
      {
        if (bar.bFinishMark_Epoxy)
          str = "E";
        else if (bar.bFinishMark_Galvanized)
          str = "G";
      }
      string strippedMark;
      if (this.MarkIsStandardBarMark(strRootMark, out strippedMark))
        return strippedMark.Length > 2 ? strippedMark.Insert(2, str) : strippedMark + str;
      string escPrefix = this.mfgSettingsData.escPrefix;
      Match match = new Regex($"(?<coreMark1>(?<prefix>{escPrefix}))(?<finishMarks>[EGX])?(?<coreMark2>(?<strBarSize>\\d{{2}})(?<barShape>{bar.BarShape})(?<controlMark>\\d{{{this.mfgSettingsData.markDecimalPositionPrepend.Length.ToString()},}}))$").Match(strRootMark);
      if (match.Success)
        return match.Groups["coreMark1"]?.ToString() + str + match.Groups["coreMark2"]?.ToString();
      return escPrefix != "" && escPrefix != null ? strRootMark.Replace(escPrefix, escPrefix + str) : str + strRootMark;
    }
    string str7 = "";
    if (bar.bFinishMark_Epoxy && bar.bFinishMark_Galvanized)
      str7 = "";
    else if (!bar.bFinishMark_Epoxy && !bar.bFinishMark_Galvanized)
      str7 = "";
    else if (bar.bFinishMark_Epoxy)
      str7 = this.mfgSettingsData.EpoxyFinishMark;
    else if (bar.bFinishMark_Galvanized)
      str7 = this.mfgSettingsData.GalvanizeFinishMark;
    return strRootMark + str7;
  }

  private string StripFinishMarks(string inputMark)
  {
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Rocky)
    {
      Match match = new Regex("(?<coreMark>(?<strBarSize>\\d*)-(?<shape>[^-]*.*)-(?<total>\\d+)(?<controlMark>[A-Z-[EGJS]]*))(?<finishMarks>[EGJS]*)").Match(inputMark);
      string strippedMark = "";
      if (this.MarkIsStandardBarMark(inputMark, out strippedMark))
        return strippedMark;
      if (match.Success)
        return match.Groups["coreMark"].Value;
      QA.InHouseMessage($"input mark: {inputMark} in StripFinishMarks() failed to parse.  Please file a bug with this message.");
      return inputMark;
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Kerkstra)
      return inputMark;
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Spancrete)
    {
      string strippedMark = "";
      if (this.MarkIsStandardBarMark(inputMark, out strippedMark))
        return strippedMark;
      Match match = new Regex("(?<coreMark>(?<strBarSize>\\d*)(?<finishMarks>[EGJS]?)-(?<total>\\d+)(?<controlMark>[A-Z-[EGJS]]*))").Match(inputMark);
      if (!match.Success)
        return inputMark;
      string str1 = match.Groups["strBarSize"].Value;
      string str2 = match.Groups["total"].Value;
      string str3 = match.Groups["controlMark"].Value;
      string str4 = str2;
      string str5 = str3;
      return $"{str1}-{str4}{str5}";
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Wells)
    {
      string strippedMark = "";
      if (this.MarkIsStandardBarMark(inputMark, out strippedMark))
        return strippedMark;
      Match match = new Regex($"(?<prefix>{this.mfgSettingsData.escPrefix})(?<finishMark>(?:{this.mfgSettingsData.escGalvFinish}|{this.mfgSettingsData.escEpoxFinish}|B)){this.mfgSettingsData.escProjectNumber}(?<markAlpha>[A-Z]*)").Match(inputMark);
      return match.Success ? this.mfgSettingsData.strMarkPrefix + this.mfgSettingsData.ProjectNumber + match.Groups["markAlpha"].Value : inputMark;
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
    {
      string strippedMark;
      if (this.MarkIsStandardBarMark(inputMark, out strippedMark))
        return strippedMark;
      string str = inputMark;
      if (inputMark.Contains(this.mfgSettingsData.escPrefix + "E"))
        str = inputMark.Replace(this.mfgSettingsData.escPrefix + "E", this.mfgSettingsData.escPrefix);
      if (inputMark.Contains(this.mfgSettingsData.escPrefix + "G"))
        str = inputMark.Replace(this.mfgSettingsData.escPrefix + "G", this.mfgSettingsData.escPrefix);
      if (inputMark.Contains(this.mfgSettingsData.escPrefix + "X"))
        str = inputMark.Replace(this.mfgSettingsData.escPrefix + "X", this.mfgSettingsData.escPrefix);
      return str;
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.CoreslabONT)
    {
      string strippedMark = "";
      if (this.MarkIsStandardBarMark(inputMark, out strippedMark))
        return strippedMark;
      string pattern = "(invalidBarDiameter)";
      Regex regex1 = new Regex("(?<coreMark>(?<strBarSize>\\d*M)B(?<controlMark>\\d+))(?<finishMarks>[A-Za-z]*)", RegexOptions.Compiled);
      Regex regex2 = new Regex(pattern, RegexOptions.Compiled);
      string input = inputMark;
      Match match = regex1.Match(input);
      regex2.Match(inputMark);
      return match.Success ? match.Groups["coreMark"].Value : inputMark;
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.MidStates || this.mfgSettingsData._bucketKeying == ManufacturerKeying.Illini)
    {
      string strippedMark = "";
      Regex regex = new Regex(this.mfgSettingsData._bucketKeying != ManufacturerKeying.MidStates ? "(?<coreMark>(?<strBarSize>\\d*)-(?<total>\\d+)(?<controlMark>[A-Z]*?)(?(?![EGJS]$)$|))(?<finishMark>[EGJS]?)$" : "(?<coreMark>(?<strBarSize>\\d*)-(?<total>\\d+-\\d+)[-]?(?<shape>[A-Za-z0-9]{0,3})[-]?(?<controlMark>[A-Z-[EGJS]]*))(?<finishMarks>[EGJS]*$)");
      if (this.MarkIsStandardBarMark(inputMark, out strippedMark))
        return strippedMark;
      Match match = regex.Match(inputMark);
      if (match.Success)
        return this.mfgSettingsData._bucketKeying == ManufacturerKeying.Spancrete ? Regex.Replace(match.Groups["coreMark"].Value, "(\\d+)(?<finishmarks>[EGJS]*)(-.*)", "$1$2") : match.Groups["coreMark"].Value;
      QA.InHouseMessage($"input mark: {strippedMark} in StripFinishMarks() failed to parse.  Please file a bug with this message.");
      return inputMark;
    }
    string strippedMark1 = "";
    if (this.MarkIsStandardBarMark(inputMark, out strippedMark1))
      return strippedMark1;
    string escPrefix = this.mfgSettingsData.escPrefix;
    string escSuffix = this.mfgSettingsData.escSuffix;
    string escGalvFinish = this.mfgSettingsData.escGalvFinish;
    string escEpoxFinish = this.mfgSettingsData.escEpoxFinish;
    string str6 = $"(?<coreMark>{escPrefix}\\s*(?<strBarSize>(?(?=(0|1))\\d{{2}}|\\d{{1}}))(?<controlMark>\\d+){escSuffix}(?(?![{escGalvFinish}{escEpoxFinish}]$)$|))(?<finishMark>[{escGalvFinish}{escEpoxFinish}]?)$";
    string str7 = $"(?<coreMark>{escPrefix}\\s*(?<strBarSize>\\d{{2}})(?<controlMark>\\d+){escSuffix}(?(?![{escGalvFinish}{escEpoxFinish}]$)$|))(?<finishMark>[{escGalvFinish}{escEpoxFinish}]?)$";
    Match match1 = new Regex(this.mfgSettingsData.bUseMetricCanadian || BarHasher.useMetricEuro ? str7 : str6).Match(inputMark);
    return match1.Success ? match1.Groups["coreMark"].Value : inputMark;
  }

  public bool IsConformingMark(Rebar bar, out string errorMsg)
  {
    string galvanizeFinishMark1 = this.mfgSettingsData.GalvanizeFinishMark;
    string epoxyFinishMark1 = this.mfgSettingsData.EpoxyFinishMark;
    bool finishMarkGalvanized = bar.bFinishMark_Galvanized;
    bool bFinishMarkEpoxy = bar.bFinishMark_Epoxy;
    string str1 = this.mfgSettingsData.markDecimalPositionPrepend.Length.ToString();
    bool flag1 = true;
    errorMsg = $"Bar Mark {bar.BarMark} does not conform to the current rebar settings:{Environment.NewLine}";
    string strippedMark = "";
    bool flag2;
    if (this.MarkIsStandardBarMark(bar.BarMark, out strippedMark))
    {
      if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
      {
        if (!finishMarkGalvanized && !bFinishMarkEpoxy)
          return bar.BarMark[2].ToString() == "X";
        if (finishMarkGalvanized && !bFinishMarkEpoxy)
          return bar.BarMark[2].ToString() == galvanizeFinishMark1;
        if (!finishMarkGalvanized & bFinishMarkEpoxy)
          return bar.BarMark[2].ToString() == epoxyFinishMark1;
        if (finishMarkGalvanized & bFinishMarkEpoxy)
          return bar.BarMark[2].ToString() == "X";
      }
      Match match = new Regex($"{Regex.Escape(strippedMark)}(?<finishMarks>[{this.mfgSettingsData.escEpoxFinish}{this.mfgSettingsData.escGalvFinish}]*)$").Match(bar.BarMark);
      if (!match.Success)
        return true;
      string str2 = match.Groups["finishMarks"].Value;
      if (!finishMarkGalvanized && !bFinishMarkEpoxy)
        return string.IsNullOrWhiteSpace(str2);
      if (finishMarkGalvanized && !bFinishMarkEpoxy)
        return str2.Equals(galvanizeFinishMark1);
      if (!finishMarkGalvanized & bFinishMarkEpoxy)
        return str2.Equals(epoxyFinishMark1);
      if (finishMarkGalvanized & bFinishMarkEpoxy)
        return str2.Equals(galvanizeFinishMark1 + epoxyFinishMark1) || str2.Equals(epoxyFinishMark1 + galvanizeFinishMark1);
      errorMsg = $"{errorMsg}  Finish marks for bar: {str2} did not match any combination of rebar settings: {galvanizeFinishMark1} or {epoxyFinishMark1}{Environment.NewLine}";
      flag2 = false;
      return false;
    }
    if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Rocky)
    {
      string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      Match match = this._markMatcher.Match(bar.BarMark);
      if (match.Success)
      {
        string mark = match.Groups["controlMark"].Value;
        string str3 = match.Groups["total"].Value;
        string str4 = match.Groups["shape"].Value;
        string str5 = match.Groups["strBarSize"].Value;
        string str6 = match.Groups["finishMarks"].Value;
        if (RebarControlMarkManager3.GetIntForAlphaMark(mark, this.mfgSettingsData._bucketKeying) == 0)
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Alpha mark \"{mark}\" is not a valid alpha mark{Environment.NewLine}";
        }
        if (bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) && str3 != bar.HashedBarDims[BarSide.BarTotalLength])
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Marked Total Length: {str3} does not match bar's length hash: {bar.HashedBarDims[BarSide.BarTotalLength]}{Environment.NewLine}";
        }
        if (str4 != bar.BarShape)
        {
          errorMsg = $"{errorMsg}  Marked Bar shape: \"{str4}\" does not match bar's parametric shape: \"{bar.BarShape}\"{Environment.NewLine}";
          flag1 = false;
        }
        if (str5 != strForRealDiameter)
        {
          errorMsg = $"{errorMsg} Marked Bar Size \"{str5}\" does not match the oracle's determination of \"{strForRealDiameter}\"{Environment.NewLine}";
          flag1 = false;
        }
      }
      else
      {
        errorMsg = $"{errorMsg}  BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} bar mark is non-conforming.  Expected bar size to be of the form: {strForRealDiameter}";
        QA.LogVerboseRebarTrace($"BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} looking for bar size string: {strForRealDiameter}");
        flag1 = false;
      }
    }
    else if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.MidStates || this.mfgSettingsData._bucketKeying == ManufacturerKeying.Illini)
    {
      string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      Match match = new Regex(this.mfgSettingsData._bucketKeying != ManufacturerKeying.MidStates ? "(?<coreMark>(?<strBarSize>\\d*)-(?<total>\\d+)(?<controlMark>[A-Z]*?)(?(?![EGJS]$)$|))(?<finishMarks>[EGJS]?)$" : "(?<coreMark>(?<strBarSize>\\d*)-(?<total>\\d+-\\d+)[-]?(?<shape>[A-Za-z0-9]{0,3})[-]?(?<controlMark>[A-Z-[EGJS]]*))(?<finishMarks>[EGJS]*$)").Match(bar.BarMark);
      if (match.Success)
      {
        string mark = match.Groups["controlMark"].Value;
        string str7 = match.Groups["total"].Value;
        string str8 = match.Groups["shape"].Value;
        string str9 = match.Groups["strBarSize"].Value;
        string str10 = match.Groups["finishMarks"].Value;
        if (RebarControlMarkManager3.GetIntForAlphaMark(mark, this.mfgSettingsData._bucketKeying) == 0)
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Alpha mark \"{mark}\" is not a valid alpha mark{Environment.NewLine}";
        }
        if (bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) && str7 != bar.HashedBarDims[BarSide.BarTotalLength])
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Marked Total Length: {str7} does not match bar's length hash: {bar.HashedBarDims[BarSide.BarTotalLength]}{Environment.NewLine}";
        }
        if (str8 != bar.BarShape && this.mfgSettingsData._bucketKeying != ManufacturerKeying.Spancrete && this.mfgSettingsData._bucketKeying != ManufacturerKeying.Illini)
        {
          errorMsg = $"{errorMsg}  Marked Bar shape: \"{str8}\" does not match bar's parametric shape: \"{bar.BarShape}\"{Environment.NewLine}";
          flag1 = false;
        }
        if (str9 != strForRealDiameter)
        {
          errorMsg = $"{errorMsg} Marked Bar Size \"{str9}\" does not match the oracle's determination of \"{strForRealDiameter}\"{Environment.NewLine}";
          flag1 = false;
        }
      }
      else
      {
        errorMsg = $"{errorMsg}  BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} bar mark is non-conforming.  Expected bar size to be of the form: {strForRealDiameter}";
        QA.LogVerboseRebarTrace($"BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} looking for bar size string: {strForRealDiameter}");
        flag1 = false;
      }
    }
    else if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Kerkstra)
    {
      string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      BarMarkInfo barMarkInfo = new BarMarkInfo(bar.BarMark, this.mfgSettingsData, bar.BarShape);
      if (barMarkInfo.IsValidParse)
      {
        string strMarkNumber = barMarkInfo.strMarkNumber;
        string totalLength = barMarkInfo.totalLength;
        string barSize = barMarkInfo.barSize;
        string barGrade = barMarkInfo.BarGrade;
        string finishMark = barMarkInfo.FinishMark;
        string weldable = barMarkInfo.Weldable;
        string prefix = barMarkInfo.Prefix;
        if (RebarControlMarkManager3.GetIntForAlphaMark(strMarkNumber, ManufacturerKeying.Kerkstra) == 0)
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Alpha mark \"{strMarkNumber}\" is not a valid alpha mark{Environment.NewLine}";
        }
        if (bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) && totalLength != bar.HashedBarDims[BarSide.BarTotalLength])
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Marked Total Length: {totalLength} does not match bar's length hash: {bar.HashedBarDims[BarSide.BarTotalLength]}{Environment.NewLine}";
        }
        if (barSize != strForRealDiameter)
        {
          errorMsg = $"{errorMsg} Marked Bar Size \"{barSize}\" does not match the oracle's determination of \"{strForRealDiameter}\"{Environment.NewLine}";
          flag1 = false;
        }
        if (prefix != this.mfgSettingsData.strMarkPrefix)
        {
          errorMsg = $"{errorMsg}  Marked Bar Prefix \"{prefix}\" does not match the manufacturer's settings \"{this.mfgSettingsData.strMarkPrefix}\"{Environment.NewLine}";
          flag1 = false;
        }
      }
      else
      {
        errorMsg = $"{errorMsg}  BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} bar mark is non-conforming.  Expected bar size to be of the form: {strForRealDiameter}";
        QA.LogLine($"BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark}using K7a mark parsing engine");
        flag1 = false;
      }
    }
    else if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Spancrete)
    {
      string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
      BarMarkInfo barMarkInfo = new BarMarkInfo(bar.BarMark, this.mfgSettingsData, bar.BarShape);
      if (barMarkInfo.IsValidParse)
      {
        string strMarkNumber = barMarkInfo.strMarkNumber;
        string totalLength = barMarkInfo.totalLength;
        string barSize = barMarkInfo.barSize;
        string barGrade = barMarkInfo.BarGrade;
        string finishMark = barMarkInfo.FinishMark;
        string weldable = barMarkInfo.Weldable;
        string prefix = barMarkInfo.Prefix;
        if (RebarControlMarkManager3.GetIntForAlphaMark(strMarkNumber, ManufacturerKeying.Spancrete) == 0)
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Alpha mark \"{strMarkNumber}\" is not a valid alpha mark{Environment.NewLine}";
        }
        if (bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) && totalLength != bar.HashedBarDims[BarSide.BarTotalLength])
        {
          flag1 = false;
          errorMsg = $"{errorMsg}  Marked Total Length: {totalLength} does not match bar's length hash: {bar.HashedBarDims[BarSide.BarTotalLength]}{Environment.NewLine}";
        }
        if (barSize != strForRealDiameter)
        {
          errorMsg = $"{errorMsg} Marked Bar Size \"{barSize}\" does not match the oracle's determination of \"{strForRealDiameter}\"{Environment.NewLine}";
          flag1 = false;
        }
        if (prefix != this.mfgSettingsData.strMarkPrefix)
        {
          errorMsg = $"{errorMsg}  Marked Bar Prefix \"{prefix}\" does not match the manufacturer's settings \"{this.mfgSettingsData.strMarkPrefix}\"{Environment.NewLine}";
          flag1 = false;
        }
      }
      else
      {
        errorMsg = $"{errorMsg}  BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} bar mark is non-conforming.  Expected bar size to be of the form: {strForRealDiameter}";
        QA.LogLine($"BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark}using K7a mark parsing engine");
        flag1 = false;
      }
    }
    else
    {
      if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Wells)
      {
        string escProjectNumber = this.mfgSettingsData.escProjectNumber;
        return new Regex($"(?<prefix>{this.mfgSettingsData.escPrefix})(?<finishMark>(?:{this.mfgSettingsData.escGalvFinish}|{this.mfgSettingsData.escEpoxFinish}|B)){this.mfgSettingsData.escProjectNumber}(?<markAlpha>[A-Z]*)").Match(bar.BarMark).Success;
      }
      if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.Tindall)
      {
        string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
        this._markMatcher = new Regex($"(?<coreMark1>(?<prefix>{this.mfgSettingsData.escPrefix}))(?<finishMarks>[EGX])?(?<coreMark2>(?<strBarSize>\\d{{2}})(?<barShape>{bar.BarShape})(?<controlMark>\\d{{{this.mfgSettingsData.markDecimalPositionPrepend.Length.ToString()},}}))$");
        Match match = this._markMatcher.Match(bar.BarMark);
        if (match.Success)
        {
          string s = match.Groups["controlMark"].Value;
          string str11 = match.Groups["barShape"].Value;
          string str12 = match.Groups["strBarSize"].Value;
          string str13 = match.Groups["finishMarks"].Value;
          if (!int.TryParse(s, out int _))
          {
            flag1 = false;
            errorMsg = $"{errorMsg}  Alpha mark \"{s}\" is not a valid alpha mark{Environment.NewLine}";
          }
          if (str11 != bar.BarShape)
          {
            errorMsg = $"{errorMsg}  Marked Bar shape: \"{str11}\" does not match bar's parametric shape: \"{bar.BarShape}\"{Environment.NewLine}";
            flag1 = false;
          }
          if (str12 != strForRealDiameter)
          {
            errorMsg = $"{errorMsg} Marked Bar Size \"{str12}\" does not match the oracle's determination of \"{strForRealDiameter}\"{Environment.NewLine}";
            flag1 = false;
          }
        }
        else
        {
          errorMsg = $"{errorMsg}  BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} bar mark is non-conforming.  Expected bar size to be of the form: {strForRealDiameter}";
          QA.LogVerboseRebarTrace($"BarMarkInfo couldn't get a match in the bar mark: {bar.BarMark} looking for bar size string: {strForRealDiameter}");
          flag1 = false;
        }
      }
      else
      {
        if (this.mfgSettingsData._bucketKeying == ManufacturerKeying.CoreslabONT)
        {
          string pattern1 = $"(?<strBarSize>\\d*M)B(?<controlMark>\\d{{{str1},{str1}}})(?<finishMarks>({this.mfgSettingsData.escGalvFinish}|{this.mfgSettingsData.escEpoxFinish})?)$";
          string pattern2 = "(invalidBarDiameter)";
          Regex regex1 = new Regex(pattern1, RegexOptions.Compiled);
          Regex regex2 = new Regex(pattern2, RegexOptions.Compiled);
          string barMark = bar.BarMark;
          return regex1.Match(barMark).Success || regex2.Match(bar.BarMark).Success;
        }
        string escPrefix = this.mfgSettingsData.escPrefix;
        string escSuffix = this.mfgSettingsData.escSuffix;
        string escGalvFinish = this.mfgSettingsData.escGalvFinish;
        string escEpoxFinish = this.mfgSettingsData.escEpoxFinish;
        string str14 = $"(?<coreMark>{escPrefix}\\s*(?<strBarSize>(?(?=(0|1))\\d{{2}}|\\d{{1}}))(?<controlMark>\\d+){escSuffix}(?(?![{escGalvFinish}{escEpoxFinish}]$)$|))(?<finishMark>[{escGalvFinish}{escEpoxFinish}]?)$";
        string str15 = $"(?<coreMark>{escPrefix}\\s*(?<strBarSize>\\d{{2}})(?<controlMark>\\d+){escSuffix}(?(?![{escGalvFinish}{escEpoxFinish}]$)$|))(?<finishMark>[{escGalvFinish}{escEpoxFinish}]?)$";
        Match match = new Regex(this.mfgSettingsData.bUseMetricCanadian || BarHasher.useMetricEuro ? str15 : str14).Match(bar.BarMark);
        if (match.Success)
        {
          string str16 = match.Groups["controlMark"].Value;
          string str17 = new Regex("^((?<zeros>[0]*)(?<num>\\d+))").Match(str16).Groups["zeros"].Value;
          int result = 0;
          if (!int.TryParse(str16, out result))
          {
            errorMsg = $"{errorMsg}  We couldn't get the integer value of the mark: {str16} from original mark {bar.BarMark}{Environment.NewLine}";
            QA.LogVerboseRebarTrace($"We couldn't get the integer value of the mark: {str16} from original mark {bar.BarMark}");
            flag1 = false;
          }
          else if (result < this.mfgSettingsData.MinMarkNumber)
          {
            errorMsg = $"{errorMsg}  Integer value of the mark: {str16} from original mark {bar.BarMark} is less than the minimum mark number: {this.mfgSettingsData.MinMarkNumber.ToString()}{Environment.NewLine}";
            QA.LogVerboseRebarTrace($" Integer value of the mark: {str16} from original mark {bar.BarMark} is less than the minimum mark number: {this.mfgSettingsData.MinMarkNumber.ToString()}");
            flag1 = false;
          }
          if (str16.Length < this.mfgSettingsData.markDecimalPositionPrepend.Length)
          {
            errorMsg = $"{errorMsg}  Zeros padding for {str16} from original mark {bar.BarMark} is less than the minimum padding: {this.mfgSettingsData.markDecimalPositionPrepend}{Environment.NewLine}";
            QA.LogVerboseRebarTrace($" Zeros padding for {str16} from original mark {bar.BarMark} is less than the minimum padding: {this.mfgSettingsData.markDecimalPositionPrepend.ToString()}");
            flag1 = false;
          }
          else if (str16.Length > this.mfgSettingsData.markDecimalPositionPrepend.Length && str17.Length > 0)
          {
            errorMsg = $"{errorMsg}  Zeros padding for {str16} is: {str17} from original mark {bar.BarMark} and is more than the minimum padding: {this.mfgSettingsData.markDecimalPositionPrepend}{Environment.NewLine}";
            QA.LogVerboseRebarTrace($" Zeros padding for {str16} is: {str17} from original mark {bar.BarMark} and is more than the minimum padding: {this.mfgSettingsData.markDecimalPositionPrepend.ToString()}");
            flag1 = false;
          }
          string str18 = match.Groups["strBarSize"].Value;
          string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.mfgSettingsData.bUseMetricCanadian, this.mfgSettingsData.useZeroPaddingForBarSize);
          if (str18 != strForRealDiameter)
          {
            errorMsg = $"{errorMsg}  Bar Size {str18} did not match the expected bar size designation: {strForRealDiameter}{Environment.NewLine}";
            flag1 = false;
          }
          string str19 = match.Groups["finishMarks"].Value;
          string galvanizeFinishMark2 = this.mfgSettingsData.GalvanizeFinishMark;
          string epoxyFinishMark2 = this.mfgSettingsData.EpoxyFinishMark;
          if (!string.IsNullOrWhiteSpace(str19) && !(str19 == galvanizeFinishMark2) && !(str19 == epoxyFinishMark2) && !(str19 == galvanizeFinishMark2 + epoxyFinishMark2) && !(str19 == epoxyFinishMark2 + galvanizeFinishMark2))
          {
            errorMsg = $"{errorMsg}  Finish marks for bar: {str19} did not match any combination of rebar settings: {galvanizeFinishMark2} or {epoxyFinishMark2}{Environment.NewLine}";
            flag1 = false;
          }
        }
        else
        {
          if (!new Regex($"(?={escPrefix})").Match(bar.BarMark).Success)
          {
            errorMsg = $"{errorMsg}  Required Mark prefix {this.mfgSettingsData.strMarkPrefix} could not be found in bar mark: {bar.BarMark}{Environment.NewLine}";
            flag2 = false;
          }
          if (!string.IsNullOrEmpty(this.mfgSettingsData.strMarkSuffix) && !new Regex($"(?=.*{escSuffix})").Match(bar.BarMark).Success)
          {
            errorMsg = $"{errorMsg}  Required Mark suffix {this.mfgSettingsData.strMarkSuffix} could not be found in bar mark: {bar.BarMark}{Environment.NewLine}";
            flag2 = false;
          }
          flag1 = false;
        }
      }
    }
    return flag1;
  }

  public bool UpdateElementIdsAfterReloadLatest(Document revitDoc)
  {
    QA.LogVerboseRebarTrace("--Updating ElementIds After Reload Latest: " + revitDoc.PathName);
    StringBuilder stringBuilder = new StringBuilder();
    foreach (string str in this.GuidToElementIdMap.Keys.ToList<string>())
    {
      try
      {
        Element element = revitDoc.GetElement(str);
        if (element != null)
        {
          if (!(element.Id == this.GuidToElementIdMap[str]))
          {
            QA.Trace($"           Found ElementId MisMatch for guid: {str} oldId: {this.GuidToElementIdMap[str].ToString()} new: {element.Id.ToString()}");
            this.GuidToElementIdMap[str] = element.Id;
          }
        }
      }
      catch (Exception ex)
      {
        stringBuilder.AppendLine(ex.Message);
      }
    }
    if (QA.LogVerboseRebarInfo)
      QA.LogLine("--END Update GUID ElementId Map");
    if (stringBuilder.ToString().Length <= 0)
      return true;
    QA.LogVerboseRebarTrace(">>>>>>>   Errored in UpdateElementIdsAfterReloadLatest");
    QA.LogVerboseRebarTrace(stringBuilder.ToString());
    TaskDialog td = new TaskDialog("Edge Error");
    td.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    td.MainInstruction = "Exception while updating GuidToElementId Map after Reload Latest";
    td.ExpandedContent = stringBuilder.ToString();
    td.CommonButtons = (TaskDialogCommonButtons) 1;
    td.AllowCancellation = false;
    QA.LogTaskDialog(td);
    td.Show();
    return false;
  }

  private bool BarHasValidDiameter(Element elem)
  {
    return BarDiameterOracle.IsValidDiameter(Parameters.GetParameterAsZeroBasedDouble(elem, "BAR_DIAMETER"));
  }

  public bool VerifyMarkLists(Document revitDoc)
  {
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("");
    stringBuilder.AppendLine("Verify Marks Lists:");
    IEnumerable<Rebar> source1 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilyInstance)).ToElements().Where<Element>((Func<Element, bool>) (s => s.LookupParameter("CONTROL_MARK") != null && Parameters.LookupParameter(s, "BAR_SHAPE") != null && !s.HasSuperComponent())).Where<Element>((Func<Element, bool>) (s => this.BarHasValidDiameter(s))).Cast<FamilyInstance>().Select<FamilyInstance, Rebar>((Func<FamilyInstance, Rebar>) (s => new Rebar(s, this.mfgSettingsData)));
    IEnumerable<Rebar> source2 = source1.Where<Rebar>((Func<Rebar, bool>) (s => !s.BarShape.Contains("STRAIGHT")));
    IEnumerable<IGrouping<string, BarMarkInfo>> list1 = (IEnumerable<IGrouping<string, BarMarkInfo>>) source2.Select<Rebar, BarMarkInfo>((Func<Rebar, BarMarkInfo>) (s => new BarMarkInfo(s.BarMark, this.mfgSettingsData, s.BarShape))).OrderBy<BarMarkInfo, string>((Func<BarMarkInfo, string>) (s => s.CoreMark)).GroupBy<BarMarkInfo, string>((Func<BarMarkInfo, string>) (s => s.CoreMark)).ToList<IGrouping<string, BarMarkInfo>>();
    bool flag1 = true;
    QA.LogLine("Mark Verification Test Summary:");
    string format = "      {0,48}: {1,8}";
    bool flag2 = true;
    foreach (IGrouping<string, BarMarkInfo> source3 in list1)
    {
      string key = source3.Key;
      string strippedMark = "";
      if (!this.MarkIsStandardBarMark(key, out strippedMark))
      {
        int num = source3.Count<BarMarkInfo>();
        stringBuilder.Append($"Bar In Project: {key} count: {num.ToString()} -> ");
        if (this.ProjectBarMarkList.ContainsKey(key))
        {
          if (this.ProjectBarMarkList[key] == num)
          {
            stringBuilder.AppendLine($"PMLCount = {this.ProjectBarMarkList[key].ToString()} OK!");
          }
          else
          {
            stringBuilder.AppendLine($"PMLCount = {this.ProjectBarMarkList[key].ToString()} ** ERROR **");
            flag2 = false;
          }
        }
        else
        {
          stringBuilder.AppendLine("ERROR: Can't find this mark in PML!");
          flag2 = false;
        }
      }
    }
    if (flag2)
    {
      QA.LogLine(string.Format(format, (object) "PML Count Verification", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "PML Count Verification", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag3 = true;
    stringBuilder.AppendLine("-> Check for extra PML entries:");
    List<string> list2 = list1.Select<IGrouping<string, BarMarkInfo>, string>((Func<IGrouping<string, BarMarkInfo>, string>) (s => s.Key)).ToList<string>();
    if (list1.Count<IGrouping<string, BarMarkInfo>>() < this.ProjectBarMarkList.Count)
    {
      foreach (string key in this.ProjectBarMarkList.Keys)
      {
        if (this.ProjectBarMarkList[key] > 0 && !list2.Contains(key))
        {
          stringBuilder.AppendLine($"ERROR: PML contains non-zero key: {key} and no corresponding bars exist in the model");
          flag3 = false;
        }
      }
    }
    if (flag3)
    {
      QA.LogLine(string.Format(format, (object) "PML Extraneous Entry Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "PML Extraneous Entry Test", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag4 = true;
    stringBuilder.AppendLine("----  VERIFY iUsedMarks  -----");
    foreach (Rebar bar in source2)
    {
      string strippedMark = "";
      if (!this.MarkIsStandardBarMark(bar.BarMark, out strippedMark))
      {
        int num = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
        string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
        if (this.iUsedMarks.ContainsKey(rebarBucketKeyHash))
        {
          if (!this.iUsedMarks[rebarBucketKeyHash].Contains(bar.MarkInfo.iMarkNumber))
          {
            stringBuilder.AppendLine($"ERROR: iUsedMarks for iBarDiam: {num.ToString()} does not contain bar control number {bar.MarkInfo.iMarkNumber.ToString()} for Bar Mark: {bar.MarkInfo.CoreMark} guid: {bar.UniqueId}");
            flag4 = false;
          }
        }
        else
        {
          stringBuilder.AppendLine("ERROR: bar exists in model but does not have a bar slot in iUsedMarks");
          flag4 = false;
        }
      }
    }
    if (flag4)
    {
      QA.LogLine(string.Format(format, (object) "iUsedMarks Verification Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "iUsedMarks Verification Test", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag5 = true;
    stringBuilder.AppendLine("----  VERIFY Bar Marks Dictionary  -----");
    foreach (Rebar bar in source2)
    {
      if (BarDiameterOracle.IsValidDiameter(bar.dblBarDiameter))
      {
        int num = BarDiameterOracle.ResolveBarDiam(bar.dblBarDiameter);
        string rebarBucketKeyHash = this.mfgSettingsData.GetRebarBucketKeyHash(bar);
        if (this.BarMarks.ContainsKey(rebarBucketKeyHash))
        {
          foreach (string bothBarHash in BarHasher.GetBothBarHashes(bar, this.mfgSettingsData._bucketKeying))
          {
            if (this.BarMarks[rebarBucketKeyHash].ContainsKey(bothBarHash))
            {
              if (this.BarMarks[rebarBucketKeyHash][bothBarHash] != bar.MarkInfo.CoreMark)
              {
                flag5 = false;
                stringBuilder.AppendLine($"Rebar id: {bar.Id?.ToString()}'s mark: {bar.MarkInfo.CoreMark} does not match Dictionary value: {this.BarMarks[rebarBucketKeyHash][bothBarHash]} hash = {bothBarHash}");
              }
            }
            else
            {
              flag5 = false;
              stringBuilder.AppendLine($"Rebar bar with Id: {bar.Id?.ToString()}'s hash: {bothBarHash} was not found in dictionary at all.");
            }
          }
        }
        else
        {
          flag5 = false;
          stringBuilder.AppendLine($"Diameter number: {num.ToString()} for rebar bar id: {bar.Id?.ToString()} was not found in bar marks dictionary");
        }
      }
    }
    if (flag5)
    {
      QA.LogLine(string.Format(format, (object) "BarMark Dictionary Verification Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "BarMark Dictionary Verification Test", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag6 = true;
    stringBuilder.AppendLine("----  VERIFY Guid To Element Id Map  -----");
    foreach (Rebar rebar in source1)
    {
      if (this.GuidToElementIdMap.ContainsKey(rebar.UniqueId))
      {
        if (this.GuidToElementIdMap[rebar.UniqueId] != rebar.Id)
        {
          flag6 = false;
          stringBuilder.AppendLine($"Rebar bar with Guid: {rebar.UniqueId} with Id: {rebar.Id?.ToString()} did not match guid to element id map value: {this.GuidToElementIdMap[rebar.UniqueId]?.ToString()}");
        }
      }
      else
      {
        flag6 = false;
        stringBuilder.AppendLine($"rebar with unique id: {rebar.UniqueId} is missing from the Guid to ElementId Map");
      }
    }
    if (flag6)
    {
      QA.LogLine(string.Format(format, (object) "Guid to Element Id Verification Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "Guid to Element Id Verification Test", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag7 = true;
    stringBuilder.AppendLine("----  VERIFY No Extraneous Keys in Guid to Element Id Map  -----");
    if (source1.Count<Rebar>() != this.GuidToElementIdMap.Count)
      flag7 = false;
    if (flag7)
    {
      QA.LogLine(string.Format(format, (object) "Guid to Element Id Extraneous Entry Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "Guid to Element Id Extraneous Entry Test", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag8 = true;
    stringBuilder.AppendLine("-> Verify GUID to BarInfo Consistency:");
    foreach (Rebar rebar in source2)
    {
      if (BarDiameterOracle.IsValidDiameter(rebar.dblBarDiameter))
      {
        if (this.GuidToBarInfoMap.ContainsKey(rebar.UniqueId))
        {
          if (this.GuidToBarInfoMap[rebar.UniqueId].BarMark != rebar.MarkInfo.CoreMark)
          {
            flag8 = false;
            stringBuilder.AppendLine($"Rebar bar with Guid: {rebar.UniqueId} with Id: {rebar.Id?.ToString()}'s mark {rebar.MarkInfo.CoreMark} did not match guid to BarInfo id map value: {this.GuidToBarInfoMap[rebar.UniqueId].BarMark}");
          }
          int num = BarDiameterOracle.ResolveBarDiam(rebar.dblBarDiameter);
          if (this.GuidToBarInfoMap[rebar.UniqueId].iBarDiam != num)
          {
            flag8 = false;
            stringBuilder.AppendLine($"Rebar bar with Guid: {rebar.UniqueId} with Id: {rebar.Id?.ToString()}'s iBarDiam {num.ToString()} did not match guid to BarInfo id map value: {this.GuidToBarInfoMap[rebar.UniqueId].iBarDiam.ToString()}");
          }
          string strippedMark = "";
          if (!this.MarkIsStandardBarMark(rebar.BarMark, out strippedMark) && this.mfgSettingsData._bucketKeying != ManufacturerKeying.Wells && this.GuidToBarInfoMap[rebar.UniqueId].strBarDiameter != rebar.MarkInfo.barSize)
          {
            flag8 = false;
            stringBuilder.AppendLine($"Rebar bar with Guid: {rebar.UniqueId} with Id: {rebar.Id?.ToString()}'s strBarDiameter {rebar.MarkInfo.barSize} did not match guid to BarInfo id map value: {this.GuidToBarInfoMap[rebar.UniqueId].strBarDiameter}");
          }
        }
        else
        {
          flag8 = false;
          stringBuilder.AppendLine($"rebar with unique id: {rebar.UniqueId} is missing from the Guid to BarInfo Map");
        }
      }
    }
    if (flag8)
    {
      QA.LogLine(string.Format(format, (object) "GUID to BarInfo Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "GUID to BarInfo Test", (object) "*FAIL*"));
      flag1 = false;
    }
    bool flag9 = true;
    stringBuilder.AppendLine("----  VERIFY No Extraneous Keys in Guid to BarInfo Map  -----");
    if (source1.Count<Rebar>() != this.GuidToBarInfoMap.Count)
      flag9 = false;
    if (flag9)
    {
      QA.LogLine(string.Format(format, (object) "Guid to BarInfo Extraneous Entry Test", (object) "*passed*"));
    }
    else
    {
      QA.LogLine(string.Format(format, (object) "Guid to BarInfo Extraneous Entry Test", (object) "*FAIL*"));
      flag1 = false;
    }
    return flag1;
  }

  public override string ToString() => new StringBuilder().ToString();

  private enum ListMembershipCheck
  {
    GeometryExistsAndMarkIsTheSame,
    GeometryOrMarkExists,
    NeitherMarkNorGeometryExist,
  }
}
