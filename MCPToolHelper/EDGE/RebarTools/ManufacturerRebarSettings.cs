// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.ManufacturerRebarSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Utils;
using Utils.AdminUtils;
using Utils.ElementUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.RebarTools;

public class ManufacturerRebarSettings
{
  private string _manufacturerName;
  private string filePath = "";
  public List<Rebar> StandardBars;
  public ManufacturerKeying _bucketKeying;
  public string ManufacturerNameForStandardBars = "";
  public bool bHasStandardBars;
  public string strMarkPrefix = "";
  public int MinMarkNumber = 1;
  public string strMarkSuffix = "";
  public bool bUseMetricCanadian;
  public bool bUseMetricEuropean;
  public string EpoxyFinishMark = "E";
  public string GalvanizeFinishMark = "G";
  public bool useMarkSuffix;
  public bool useMarkPrefix;
  public bool useZeroPaddingForBarSize = true;
  public string markDecimalPositionPrepend = "0";
  public bool IsValidSettings = true;
  public bool UserCancelledLoad;
  public string escPrefix;
  public string escSuffix;
  public string escGalvFinish;
  public string escEpoxFinish;
  public string escProjectNumber;
  public string strStraightBarPrefix = "";
  public LengthHashOption straightHashOption;
  public string straightLengthZeroPrepend = "0";
  private static bool ShowDebugDialogs;
  private static bool ShowDebugDialogsInner;

  public string ProjectNumber { get; set; }

  public string MfgName => this._manufacturerName;

  public ManufacturerRebarSettings(Document revitDoc, string manufacturerName)
  {
    this._manufacturerName = manufacturerName;
    this.ProjectNumber = Parameters.GetParameterAsString((Element) revitDoc.ProjectInformation, BuiltInParameter.PROJECT_NUMBER);
    this.Init();
    if (this._manufacturerName.ToUpper() == "ROCKY MOUNTAIN PRESTRESS")
    {
      this.straightHashOption = LengthHashOption.RockyFeetAndInchesToQuarter;
      this._bucketKeying = ManufacturerKeying.Rocky;
    }
    else if (this._manufacturerName.ToUpper() == "KERKSTRA")
    {
      this._bucketKeying = ManufacturerKeying.Kerkstra;
      this.EpoxyFinishMark = "e";
      this.GalvanizeFinishMark = "g";
    }
    else if (this._manufacturerName.ToUpper() == "WELLS")
    {
      this._bucketKeying = ManufacturerKeying.Wells;
      this.MinMarkNumber = 0;
      this.strMarkPrefix = "R";
    }
    else if (this._manufacturerName.ToUpper() == "CORESLAB ONT")
    {
      this._bucketKeying = ManufacturerKeying.CoreslabONT;
      this.MinMarkNumber = 1;
      this.markDecimalPositionPrepend = "00";
      this.straightHashOption = LengthHashOption.FeetAndInches_RoundDown;
    }
    else if (this._manufacturerName.ToUpper() == "MIDSTATES")
    {
      this.straightHashOption = LengthHashOption.MidStatesFeetAndInches;
      this._bucketKeying = ManufacturerKeying.MidStates;
      this.useZeroPaddingForBarSize = false;
    }
    else if (this._manufacturerName.ToUpper() == "ILLINI")
    {
      this.straightHashOption = LengthHashOption.FeetAndInches_RoundDown;
      this._bucketKeying = ManufacturerKeying.Illini;
      this.useZeroPaddingForBarSize = false;
    }
    else if (this._manufacturerName.ToUpper() == "SPANCRETE")
    {
      this.straightHashOption = LengthHashOption.FeetAndInches_RoundUp;
      this._bucketKeying = ManufacturerKeying.Spancrete;
      this.useZeroPaddingForBarSize = true;
    }
    else if (this._manufacturerName.ToUpper() == "TINDALL")
    {
      this.strMarkPrefix = "SB";
      this.strStraightBarPrefix = "RS";
      this.straightLengthZeroPrepend = "000";
      this.MinMarkNumber = 1;
      this.markDecimalPositionPrepend = "00";
      this.straightHashOption = LengthHashOption.Inches;
      this._bucketKeying = ManufacturerKeying.Tindall;
      this.useZeroPaddingForBarSize = true;
    }
    else
    {
      this._bucketKeying = ManufacturerKeying.Standard;
      if (Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(revitDoc))
      {
        this.bUseMetricEuropean = true;
        BarHasher.useMetricEuro = this.bUseMetricEuropean;
      }
      else
      {
        this.bUseMetricEuropean = false;
        BarHasher.useMetricEuro = this.bUseMetricEuropean;
      }
    }
    this.UpdateStandardBarArray();
    this.escPrefix = Regex.Escape(this.strMarkPrefix);
    this.escSuffix = Regex.Escape(this.strMarkSuffix);
    this.escGalvFinish = Regex.Escape(this.GalvanizeFinishMark);
    this.escEpoxFinish = Regex.Escape(this.EpoxyFinishMark);
    this.escProjectNumber = Regex.Escape(this.ProjectNumber);
  }

  public ManufacturerRebarSettings()
  {
  }

  private void Init() => this.StandardBars = new List<Rebar>();

  public bool UpdateMfgSettigns()
  {
    this.UpdateStandardBarArray();
    return true;
  }

  public string GetSampleMark(Rebar bar)
  {
    if (this._bucketKeying == ManufacturerKeying.Rocky)
      return "03-T2-0300";
    if (this._bucketKeying == ManufacturerKeying.Kerkstra)
      return this.strMarkPrefix + "06n-0300-A";
    if (this._bucketKeying == ManufacturerKeying.Wells)
      return $"{this.strMarkPrefix}B{this.ProjectNumber}A";
    if (this._bucketKeying == ManufacturerKeying.CoreslabONT)
      return "10MB" + string.Format($"{{0:{this.markDecimalPositionPrepend}}}", (object) this.MinMarkNumber);
    if (this._bucketKeying == ManufacturerKeying.MidStates)
      return this.strMarkPrefix + "4-05-04-T2-A";
    if (this._bucketKeying == ManufacturerKeying.Illini)
      return this.strMarkPrefix + "5-1402A";
    if (this._bucketKeying == ManufacturerKeying.Spancrete)
      return this.strMarkPrefix + "04-0200";
    if (this._bucketKeying == ManufacturerKeying.Tindall)
      return this.strMarkPrefix + "X05216";
    string format = $"{{0:{this.markDecimalPositionPrepend}}}";
    return this.strMarkPrefix + BarDiameterOracle.GetDiameterStrForRealDiameter(bar.dblBarDiameter, this.bUseMetricCanadian, this.useZeroPaddingForBarSize) + string.Format(format, (object) this.MinMarkNumber) + this.strMarkSuffix + (bar.bFinishMark_Epoxy ? this.EpoxyFinishMark : "") + (bar.bFinishMark_Galvanized ? this.GalvanizeFinishMark : "");
  }

  public string GetRebarBucketKeyHash(Rebar bar)
  {
    string rebarBucketKeyHash;
    if (this.bUseMetricEuropean)
    {
      rebarBucketKeyHash = BarHasher.HashLength(bar.dblBarDiameter, LengthHashOption.Millimeters);
    }
    else
    {
      string str1 = BarHasher.HashLength(bar.dblBarDiameter, LengthHashOption.FeetAndInchesToEights);
      if (this._bucketKeying == ManufacturerKeying.Rocky || this._bucketKeying == ManufacturerKeying.MidStates)
      {
        string barShape = bar.BarShape;
        string str2 = bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) ? bar.HashedBarDims[BarSide.BarTotalLength] : "-ERR-";
        rebarBucketKeyHash = str1 + barShape + str2;
      }
      else if (this._bucketKeying == ManufacturerKeying.Illini || this._bucketKeying == ManufacturerKeying.Spancrete)
      {
        string str3 = bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) ? bar.HashedBarDims[BarSide.BarTotalLength] : "-ERR-";
        rebarBucketKeyHash = str1 + str3;
      }
      else if (this._bucketKeying == ManufacturerKeying.Kerkstra)
      {
        string barShape = bar.BarShape;
        string str4 = "n";
        if (bar.bFinishMark_Epoxy && !bar.bFinishMark_Galvanized)
          str4 = "e";
        else if (!bar.bFinishMark_Epoxy && bar.bFinishMark_Galvanized)
          str4 = "g";
        string str5 = bar.HashedBarDims.ContainsKey(BarSide.BarTotalLength) ? bar.HashedBarDims[BarSide.BarTotalLength] : "-ERR-";
        rebarBucketKeyHash = str1 + str4 + str5;
      }
      else
        rebarBucketKeyHash = this._bucketKeying != ManufacturerKeying.Wells ? (this._bucketKeying != ManufacturerKeying.Tindall ? str1 : str1 + bar.BarShape) : "1";
    }
    return rebarBucketKeyHash;
  }

  public string GetRebarBucketKeyHash(RebarInfo barInfo)
  {
    string str = BarHasher.HashLength(barInfo.dblBarDiameter, LengthHashOption.FeetAndInchesToEights);
    string rebarBucketKeyHash;
    if (this._bucketKeying == ManufacturerKeying.Rocky || this._bucketKeying == ManufacturerKeying.MidStates)
      rebarBucketKeyHash = str + barInfo.barShape + barInfo.totalLength;
    else if (this._bucketKeying == ManufacturerKeying.Illini || this._bucketKeying == ManufacturerKeying.Spancrete)
      rebarBucketKeyHash = str + barInfo.totalLength;
    else if (this._bucketKeying == ManufacturerKeying.Kerkstra)
      rebarBucketKeyHash = str + barInfo.kerkstraGradeWeldFinishInfo + barInfo.totalLength;
    else if (this._bucketKeying == ManufacturerKeying.Wells)
      rebarBucketKeyHash = "1";
    else if (this._bucketKeying == ManufacturerKeying.Tindall)
    {
      rebarBucketKeyHash = str + barInfo.barShape;
    }
    else
    {
      if (BarHasher.useMetricEuro)
        str = BarHasher.HashLength(barInfo.dblBarDiameter, LengthHashOption.Millimeters);
      rebarBucketKeyHash = str;
    }
    return rebarBucketKeyHash;
  }

  private void UpdateStandardBarArray() => this.ReadDataFileAndUpdate();

  private void ReadDataFileAndUpdate()
  {
    if (!string.IsNullOrEmpty(this._manufacturerName))
    {
      if (QAUtils.IsJournalPlayback)
        this.filePath = $"{App.JournalPlaybackDirectoryPath}\\{this._manufacturerName.ToUpper()}.txt";
      else if (!string.IsNullOrWhiteSpace(App.RebarSettingsFolderPath))
      {
        this.filePath = $"{App.RebarSettingsFolderPath}\\{this._manufacturerName.ToUpper()}.txt";
      }
      else
      {
        this.filePath = $"C:\\EDGEforREVIT\\{this._manufacturerName.ToUpper()}.txt";
        TaskDialog.Show("Warning", "Unable to parse Edge Preferences Rebar Settings Folder Path.  Folder Path is empty.  Using: C:\\EDGEforREVIT\\");
      }
      QA.LogLine("--Rebar Settings File Path: " + this.filePath);
      if (EdgeBuildInformation.IsDebugCheck && ManufacturerRebarSettings.ShowDebugDialogs)
        new TaskDialog("Standards File Path")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainContent = ("Manufacturer Standard file path: " + this.filePath)
        }.Show();
      if (File.Exists(this.filePath))
      {
        string[] strArray = new string[App.MaximumStandardBars];
        int index1 = 0;
        StreamReader streamReader = new StreamReader(this.filePath);
        string str1;
        while ((str1 = streamReader.ReadLine()) != null)
        {
          strArray[index1] = str1;
          if (strArray[index1].StartsWith("*rebar*"))
            strArray[index1] = str1.Replace("\"", "");
          ++index1;
          if (EdgeBuildInformation.IsDebugCheck && ManufacturerRebarSettings.ShowDebugDialogsInner)
            new TaskDialog("Standards File Read Lines")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainContent = str1
            }.Show();
        }
        streamReader.Close();
        int index2 = 0;
        bool flag1 = false;
        bool flag2 = false;
        bool invalidOutput = false;
        List<string> source = new List<string>();
        for (; index2 <= index1; ++index2)
        {
          if (!string.IsNullOrWhiteSpace(strArray[index2]))
          {
            string barMark;
            if (!this.ProcessStandardsFileLine(strArray[index2], out invalidOutput, out barMark))
              flag1 = true;
            if (invalidOutput)
              flag2 = true;
            if (barMark != null)
              source.Add(barMark);
          }
        }
        if (this.bUseMetricEuropean && this.straightHashOption != LengthHashOption.Meters && this.straightHashOption != LengthHashOption.Centimeters && this.straightHashOption != LengthHashOption.Millimeters && this.straightHashOption != LengthHashOption.None)
          this.straightHashOption = LengthHashOption.Millimeters;
        if (flag1)
        {
          this.IsValidSettings = false;
          QA.LogError("MfgRebarSettings:ReadUpdate", "Failed to successfully read manufacturer rebar settings.  Please check log for specific error");
          new TaskDialog("EDGE^R")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            AllowCancellation = false,
            MainInstruction = "Rebar Settings File Error",
            MainContent = $"EDGE^R encountered an error while reading the settings file: {this.filePath}  Automated Rebar Marking cannot be enabled for this Document until this issue is resolved.  Please refer to EDGE^R log for more details.  Once the error is resolved, please re-open this document"
          }.Show();
        }
        else
        {
          if (!flag2)
            return;
          int num = 1;
          QA.LogError("MfgRebarSettings:ReadUpdate", "Invalid Standard Bars in settings file.  Please check log for specific error");
          TaskDialog taskDialog1 = new TaskDialog("EDGE^R");
          taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
          taskDialog1.AllowCancellation = false;
          taskDialog1.MainInstruction = "Rebar Settings File Warning";
          taskDialog1.MainContent = $"EDGE^R encountered an error while reading the settings file: {this.filePath} Please refer to EDGE^R log for more details. The unit settings do not match the Standard bent bars in Rebar Settings. Automated Rebar Marking will continue with converted values. This may result in incorrect marks. Please check Rebar settings.";
          if (source.Count<string>() > 0)
          {
            taskDialog1.ExpandedContent = "Effected Bar Marks: ";
            foreach (string str2 in source)
            {
              if (num < 2)
              {
                taskDialog1.ExpandedContent += str2;
              }
              else
              {
                TaskDialog taskDialog2 = taskDialog1;
                taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}, {str2}";
              }
              ++num;
            }
          }
          taskDialog1.Show();
        }
      }
      else
      {
        this.IsValidSettings = false;
        this.DisplayParameterWarning($"Standard Manufacture Settings File: {this.filePath} cannot be found.  Please check this location.");
        QA.LogError("MfgRebarSettings:ReadUpdate", $"Standard Manufacture Settings File: {this.filePath} cannot be found.  Please check this location.");
      }
    }
    else
    {
      if (!EdgeBuildInformation.IsDebugCheck || !ManufacturerRebarSettings.ShowDebugDialogs)
        return;
      new TaskDialog("Manufacturer")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "Manufacturer Standard Bars Parameter Array remained unchanged since both documents used the same manufacturer."
      }.Show();
    }
  }

  private bool ProcessStandardsFileLine(
    string standardsFileLine,
    out bool invalidOutput,
    out string barMark)
  {
    invalidOutput = false;
    barMark = (string) null;
    if (standardsFileLine.Length > 0 && standardsFileLine.First<char>() == '#')
      return true;
    string[] strArray = standardsFileLine.Split(new string[1]
    {
      ","
    }, StringSplitOptions.None);
    if (strArray.Length < 2 || string.IsNullOrWhiteSpace(strArray[1]) || strArray[1].Equals("null"))
      return true;
    string str1 = strArray[0];
    if (str1 != null)
    {
      switch (str1.Length)
      {
        case 7:
          if (str1 == "*rebar*")
          {
            if (!this.checkMetricInput(strArray[1]) && !string.IsNullOrEmpty(strArray[1].Trim()) && !string.Equals(strArray[1], " "))
            {
              // ISSUE: explicit reference operation
              ^ref strArray[1] += "\"";
            }
            if (strArray.Length < 5)
            {
              QA.LogError("Read Manufacturer Rebar Settings", "Unable to parse standard rebar line: " + strArray[1]);
              return true;
            }
            if (!this.checkMetricInput(strArray[1]) && !string.IsNullOrEmpty(strArray[1].Trim()))
            {
              for (int index = 4; index < strArray.Length; ++index)
              {
                if (!string.IsNullOrWhiteSpace(strArray[index]))
                {
                  // ISSUE: explicit reference operation
                  ^ref strArray[index] += "\"";
                }
              }
            }
            Queue<string> stringQueue = new Queue<string>();
            if (!this.bUseMetricEuropean)
            {
              for (int index = 1; index < 15; ++index)
              {
                Match match = new Regex("^\\s*(?<splitString>.*?)\\s*$").Match(strArray[index]);
                if (index >= strArray.Length)
                {
                  stringQueue.Enqueue("null");
                }
                else
                {
                  string str2 = match.Groups["splitString"].Value;
                  if (str2.ToUpper().Contains("MM"))
                    invalidOutput = true;
                  stringQueue.Enqueue(str2);
                }
              }
            }
            else
            {
              for (int index = 1; index < 15; ++index)
              {
                Match match = new Regex("^\\s*(?<splitString>.*?)\\s*$").Match(strArray[index]);
                if (index >= strArray.Length)
                {
                  stringQueue.Enqueue("null");
                }
                else
                {
                  string str3 = match.Groups["splitString"].Value;
                  if (str3.ToUpper().Contains("'") || str3.ToUpper().Contains("\""))
                    invalidOutput = true;
                  stringQueue.Enqueue(str3);
                }
              }
            }
            Queue<string> standardBarInfo = new Queue<string>();
            if (stringQueue.Count > 0)
            {
              int num1 = 0;
              if (!this.bUseMetricEuropean)
              {
                foreach (string str4 in stringQueue)
                {
                  if (str4.Contains("mm") && !string.IsNullOrEmpty(str4.Trim()) && (num1 != 1 || num1 != 2))
                  {
                    string str5 = str4;
                    string valueString = str5.Substring(0, str5.IndexOf("mm"));
                    string feet = "";
                    string s = "";
                    ref string local1 = ref feet;
                    ref string local2 = ref s;
                    UnitConversion.millimetersToFeetAndInchesString(valueString, out local1, out local2);
                    double result = 0.0;
                    double.TryParse(s, out result);
                    double num2 = Math.Floor(result);
                    double num3 = Math.Floor(result % 1.0 * 8.0);
                    string str6 = num2 == 0.0 && num3 == 0.0 || num3 == 0.0 ? this.addLegLengthImperial(feet, num2.ToString()) : (num2 != 0.0 ? this.addLegLengthImperial(feet, $"{num2.ToString()} {FeetAndInchesRounding.DecimalToFraction(num3 / 8.0)}") : this.addLegLengthImperial(feet, FeetAndInchesRounding.DecimalToFraction(num3 / 8.0)));
                    standardBarInfo.Enqueue(str6);
                  }
                  else
                    standardBarInfo.Enqueue(str4);
                  if (num1 == 2)
                    barMark = str4;
                  ++num1;
                }
              }
              else
              {
                foreach (string millimeters in stringQueue)
                {
                  if (!this.checkMetricInput(millimeters) && !string.IsNullOrEmpty(millimeters.Trim()) && num1 != 1 && num1 != 2)
                  {
                    string str7 = millimeters;
                    string feetValueString = "0";
                    string input = "0";
                    if (str7.Contains("'") && str7.Contains("\""))
                    {
                      int num4 = str7.IndexOf("'");
                      feetValueString = str7.Substring(0, num4);
                      int length = str7.Length - 1;
                      string str8 = str7.Substring(0, length).Substring(num4).Substring(1);
                      input = str8.Substring(str8.IndexOf("-") + 2);
                    }
                    else if (str7.Contains("\""))
                    {
                      int length = str7.IndexOf("\"");
                      int num5 = str7.IndexOf("-");
                      input = str7.Substring(num5 + 2, length);
                    }
                    else
                    {
                      int length = str7.IndexOf("'");
                      feetValueString = str7.Substring(0, length);
                    }
                    string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
                    standardBarInfo.Enqueue(UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm");
                  }
                  else
                    standardBarInfo.Enqueue(millimeters);
                  if (num1 == 2)
                    barMark = millimeters;
                  ++num1;
                }
              }
            }
            if (!invalidOutput)
              barMark = (string) null;
            this.StandardBars.Add(new Rebar(standardBarInfo, this._bucketKeying, this));
            break;
          }
          break;
        case 11:
          if (str1 == "*useMetric*")
          {
            this.bUseMetricCanadian = strArray[1].ToUpper().Equals("TRUE");
            if (this.bUseMetricEuropean)
            {
              this.bUseMetricCanadian = false;
              break;
            }
            break;
          }
          break;
        case 12:
          switch (str1[5])
          {
            case 'P':
              if (str1 == "*markPrefix*")
              {
                this.strMarkPrefix = new Regex("^\\s*(?<splitString>.*?)\\s*$").Match(strArray[1]).Groups["splitString"].Value;
                break;
              }
              break;
            case 'S':
              if (str1 == "*markSuffix*")
              {
                this.strMarkSuffix = new Regex("(?<splitString>.*?)\\s*$").Match(strArray[1]).Groups["splitString"].Value;
                break;
              }
              break;
          }
          break;
        case 14:
          if (str1 == "*manufacturer*")
          {
            this.ManufacturerNameForStandardBars = strArray[1];
            break;
          }
          break;
        case 17:
          if (str1 == "*markFinishEpoxy*")
          {
            if (this.FinishMarkStartsWithNumber(this.EpoxyFinishMark))
            {
              QA.LogError("Read Manufacturer Settings", "Epoxy Finish Mark cannot begin with a digit.  Please adjust finish mark value");
              return false;
            }
            this.EpoxyFinishMark = new Regex("(?<splitString>.*?)\\s*$").Match(strArray[1]).Groups["splitString"].Value;
            break;
          }
          break;
        case 19:
          if (str1 == "*useMetricCanadian*")
          {
            this.bUseMetricCanadian = strArray[1].ToUpper().Equals("TRUE");
            if (this.bUseMetricEuropean)
            {
              this.bUseMetricCanadian = false;
              break;
            }
            break;
          }
          break;
        case 21:
          switch (str1[14])
          {
            case 'P':
              if (str1 == "*useMarkFinishPrefix*")
              {
                this.useMarkPrefix = strArray[1].ToUpper().Equals("TRUE");
                break;
              }
              break;
            case 'S':
              if (str1 == "*useMarkFinishSuffix*")
              {
                this.useMarkSuffix = strArray[1].Equals("TRUE");
                break;
              }
              break;
          }
          break;
        case 22:
          if (str1 == "*markFinishGalvanized*")
          {
            if (this.FinishMarkStartsWithNumber(this.GalvanizeFinishMark))
            {
              QA.LogError("Read Manufacturer Settings", "Galavanized Finish Mark cannot begin with a digit.  Please adjust finish mark value");
              return false;
            }
            this.GalvanizeFinishMark = new Regex("(?<splitString>.*?)\\s*$").Match(strArray[1]).Groups["splitString"].Value;
            break;
          }
          break;
        case 23:
          switch (str1[1])
          {
            case 'm':
              if (str1 == "*markPrefixIntMinValue*")
              {
                this.markDecimalPositionPrepend = this.GetZerosFormat(strArray[1]);
                int result = 0;
                if (int.TryParse(strArray[1], out result))
                {
                  this.MinMarkNumber = result;
                  break;
                }
                QA.LogError("Read Rebar Settings", "Reading Rebar Settings File: markPrefixIntMinValue not properly read.  failed to parse int for string: " + strArray[1]);
                return false;
              }
              break;
            case 's':
              if (str1 == "*straightBarMarkPrefix*")
              {
                this.strStraightBarPrefix = new Regex("^\\s*(?<splitString>.*?)\\s*$").Match(strArray[1]).Groups["splitString"].Value;
                break;
              }
              break;
          }
          break;
        case 25:
          if (str1 == "*straightBarLengthOption*")
          {
            this.straightHashOption = this._bucketKeying != ManufacturerKeying.Rocky ? (this._bucketKeying != ManufacturerKeying.Tindall ? (this._bucketKeying != ManufacturerKeying.Illini ? (this._bucketKeying != ManufacturerKeying.MidStates ? (this._bucketKeying != ManufacturerKeying.Spancrete ? (!strArray[1].Trim().ToUpper().Equals("INCHES") ? (!strArray[1].Trim().ToUpper().Equals("FEETANDINCHES") ? (!strArray[1].Trim().ToUpper().Equals("MILLIMETERS") ? (!strArray[1].Trim().ToUpper().Equals("CENTIMETERS") ? (!strArray[1].Trim().ToUpper().Equals("METERS") ? LengthHashOption.None : LengthHashOption.Meters) : LengthHashOption.Centimeters) : LengthHashOption.Millimeters) : LengthHashOption.FeetAndInches_RoundDown) : LengthHashOption.Inches) : LengthHashOption.FeetAndInches_RoundUp) : LengthHashOption.MidStatesFeetAndInches) : LengthHashOption.FeetAndInches_RoundDown) : LengthHashOption.Inches) : LengthHashOption.RockyFeetAndInchesToQuarter;
            break;
          }
          break;
        case 28:
          if (str1 == "*padBarSizesWithLeadingZero*")
          {
            this.useZeroPaddingForBarSize = strArray[1].Equals("TRUE");
            break;
          }
          break;
        case 30:
          if (str1 == "*straightBarLengthZeroPadding*")
          {
            this.straightLengthZeroPrepend = this.GetZerosFormat(strArray[1]);
            break;
          }
          break;
      }
    }
    return true;
  }

  private string addLegLengthImperial(string feet, string inch)
  {
    string s1 = inch.TrimStart().TrimEnd();
    string s2 = feet.Trim();
    string str = "";
    if (s1.Contains("/") && s1.Contains(" ") || !s1.Contains("/"))
    {
      if (s1.Contains("/"))
      {
        int length = s1.IndexOf(" ");
        str = s1.Substring(length + 1);
        s1 = s1.Substring(0, length);
      }
      int result1;
      int.TryParse(s1, out result1);
      if (result1 >= 12)
      {
        int num1 = result1 % 12;
        int num2 = result1 / 12;
        s1 = !inch.Contains("/") ? num1.ToString() : $"{num1.ToString()} {str}";
        int result2;
        int.TryParse(s2, out result2);
        s2 = (result2 + num2).ToString();
      }
      else if (inch.Contains("/"))
        s1 = inch.TrimStart().TrimEnd();
    }
    if (string.IsNullOrWhiteSpace(s2) && string.IsNullOrWhiteSpace(s1))
      return " ";
    if (!string.IsNullOrWhiteSpace(s2) && !string.IsNullOrWhiteSpace(s1))
      return $"{s2.Trim()}' - {s1.TrimStart().TrimEnd()}\"";
    return string.IsNullOrWhiteSpace(s2) ? $"0' - {s1.TrimStart().TrimEnd()}\"" : s2.Trim() + "' - 0\"";
  }

  private bool checkMetricInput(string millimeters)
  {
    millimeters.Trim();
    if (millimeters.Contains("mm"))
    {
      int length = millimeters.IndexOf("mm");
      millimeters = millimeters.Substring(0, length);
    }
    bool flag = true;
    if (millimeters.Contains("'") || millimeters.Contains("\"") || millimeters.Contains("/") || millimeters.Contains("-") || millimeters.Contains(".") || string.IsNullOrWhiteSpace(millimeters) || string.IsNullOrEmpty(millimeters))
      flag = false;
    return flag;
  }

  private bool FinishMarkStartsWithNumber(string FinishMark)
  {
    return new Regex("\\A(?=\\d{1})").Match(FinishMark).Success;
  }

  private string GetZerosFormat(string settingsFileSetting)
  {
    return new string('0', settingsFileSetting.Length);
  }

  private void DisplayParameterWarning(string warningType)
  {
    if (warningType.Equals("empty"))
      new TaskDialog("Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = $"The PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value is empty.  Therefore no standard rebar will be processed and EDGE^R will proceed with project specific automatic rebar marking only without attempting to reference manufacturer standard bars.{Environment.NewLine}{Environment.NewLine}Note: This dialog will only appear once in this session of Revit for this specific project."
      }.Show();
    else if (warningType.Contains("cannot be found"))
    {
      TaskDialog taskDialog = new TaskDialog("Warning");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Manufacturer standard rebar settings file not found.";
      taskDialog.MainContent = $"The manufacturer standard rebar file at {this.filePath} cannot be found.";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Disable Rebar Mark Automation", "Bar Mark Automation will be temporarily disabled for this document");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Proceed With Generic Settings", "A Set of generic manufacturer settings will be used for this document");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1002)
      {
        this.IsValidSettings = true;
      }
      else
      {
        if (taskDialogResult != 1003)
          return;
        this.IsValidSettings = false;
        this.UserCancelledLoad = true;
      }
    }
    else
      new TaskDialog("Warning")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = warningType
      }.Show();
  }
}
