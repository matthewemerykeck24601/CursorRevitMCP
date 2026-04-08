// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.BarMarkInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace EDGE.RebarTools;

public class BarMarkInfo
{
  public string OriginalMarkString = "";
  public string CoreMark = "";
  public string FinishMark = "";
  public string Prefix = "";
  public string barSize = "";
  public string strMarkNumber = "";
  public int iMarkNumber;
  public bool IsValidParse;
  public string errorMessage = "";
  public int iBarSize;
  public string Weldable = "";
  public string BarGrade = "";
  public string totalLength = "";
  private ManufacturerRebarSettings _rebarSettings;
  private static string _rockyBarMarkMatchPattern = "(?<coreMark>(?<strBarSize>\\d*)-(?<shape>[^-]*.*)-(?<total>\\d+)(?<controlMark>[A-DFH-IK-RT-Z]*))(?<finishMarks>[EGJS]*)";
  private static Regex _rockyMarkMatcher = new Regex(BarMarkInfo._rockyBarMarkMatchPattern, RegexOptions.Compiled);
  private static string _midstatesBarMarkMatchPattern = "(?<coreMark>(?<strBarSize>\\d*)-(?<total>\\d+-\\d+)[-]?(?<shape>[A-Za-z0-9]{0,3})[-]?(?<controlMark>[A-Z-[EGJS]]*))(?<finishMarks>[EGJS]*$)";
  private static Regex _midstatesMarkMatcher = new Regex(BarMarkInfo._midstatesBarMarkMatchPattern, RegexOptions.Compiled);
  private static string _spancreteBarMarkMatchPattern = "(?<coreMark>(?<strBarSize>\\d*)(?<finishMarks>[EGJS]?)-(?<total>\\d+)(?<controlMark>[A-Z-[EGJS]]*))";
  private static Regex _spancreteMarkMatcher = new Regex(BarMarkInfo._spancreteBarMarkMatchPattern, RegexOptions.Compiled);
  private static string _KerkstraBarMarkMatchPattern = "((?<standardOrBent>[RS]{0,1})(?<strBarSize>\\d{1,2})(?<finishMark>[neg]{0,1})-(?<total>\\d+)-(?<controlMark>[A-Z]{1,2}))|((?<strBarSize>\\d{2})-(?<total>\\d+)-(?<controlMark>[A-Z]))";
  private static Regex _kerkstraMarkMatcher = new Regex(BarMarkInfo._KerkstraBarMarkMatchPattern, RegexOptions.Compiled);
  private static string _ontBarMarkMatchPattern = "(?<coreMark>(?<strBarSize>\\d*M)B(?<controlMark>\\d*))(?<finishMark>(?<finishMarks>[A-Za-z]*))";
  private static Regex _ontMarkMatcher = new Regex(BarMarkInfo._ontBarMarkMatchPattern, RegexOptions.Compiled);
  private static string _ontBarInvalidMatchPattern = "(invalidBarDiameter)";
  private static Regex _ontInvalidMatcher = new Regex(BarMarkInfo._ontBarInvalidMatchPattern, RegexOptions.Compiled);

  public Rebar Bar { get; set; }

  public BarMarkInfo(string barMark, ManufacturerRebarSettings rebarSettings, string barShape)
  {
    this.OriginalMarkString = barMark;
    this._rebarSettings = rebarSettings;
    switch (rebarSettings._bucketKeying)
    {
      case ManufacturerKeying.Rocky:
        Match match1 = BarMarkInfo._rockyMarkMatcher.Match(barMark);
        if (match1.Success)
        {
          this.Prefix = match1.Groups["prefix"].Value;
          this.barSize = match1.Groups["strBarSize"].Value;
          this.FinishMark = match1.Groups["finishMarks"].Value;
          this.strMarkNumber = match1.Groups["controlMark"].Value;
          this.CoreMark = match1.Groups["coreMark"].Value;
          this.totalLength = match1.Groups["total"].Value;
          int result = RebarControlMarkManager3.GetIntForAlphaMark(this.strMarkNumber, ManufacturerKeying.Rocky);
          if (result >= 1)
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
            QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        break;
      case ManufacturerKeying.Kerkstra:
        Match match2 = BarMarkInfo._kerkstraMarkMatcher.Match(barMark);
        if (match2.Success)
        {
          this.Prefix = match2.Groups["standardOrBent"].Value;
          this.barSize = match2.Groups["strBarSize"].Value;
          this.FinishMark = match2.Groups["finishMark"].Value;
          this.strMarkNumber = match2.Groups["controlMark"].Value;
          this.totalLength = match2.Groups["total"].Value;
          this.CoreMark = $"{this.Prefix}{this.barSize}{this.FinishMark}-{match2.Groups["total"].Value}-{this.strMarkNumber}";
          int result = RebarControlMarkManager3.GetIntForAlphaMark(this.strMarkNumber, ManufacturerKeying.Kerkstra);
          if (result >= 1)
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        break;
      case ManufacturerKeying.Wells:
        string escProjectNumber = this._rebarSettings.escProjectNumber;
        string escPrefix1 = this._rebarSettings.escPrefix;
        string escSuffix1 = this._rebarSettings.escSuffix;
        string escGalvFinish1 = this._rebarSettings.escGalvFinish;
        string escEpoxFinish1 = this._rebarSettings.escEpoxFinish;
        Match match3 = new Regex($"(?<prefix>{escPrefix1})(?<finishMark>(?:{escGalvFinish1}|{escEpoxFinish1}|B))*{escProjectNumber}(?<markAlpha>[A-Z]*)").Match(barMark);
        if (match3.Success)
        {
          this.strMarkNumber = match3.Groups["markAlpha"].Value;
          this.CoreMark = this._rebarSettings.strMarkPrefix + this._rebarSettings.ProjectNumber + this.strMarkNumber;
          int intForAlphaMark = RebarControlMarkManager3.GetIntForAlphaMark(this.strMarkNumber, ManufacturerKeying.Wells);
          if (intForAlphaMark >= 0)
          {
            this.IsValidParse = true;
            this.iMarkNumber = intForAlphaMark;
            break;
          }
          this.IsValidParse = false;
          this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        string strippedMark1 = "";
        if (this.MarkIsStandardBarMark(barMark, out strippedMark1))
        {
          this.CoreMark = strippedMark1;
          break;
        }
        this.CoreMark = barMark;
        break;
      case ManufacturerKeying.CoreslabONT:
        Match match4 = BarMarkInfo._ontMarkMatcher.Match(barMark);
        Match match5 = BarMarkInfo._ontInvalidMatcher.Match(barMark);
        if (match4.Success)
        {
          this.barSize = match4.Groups["strBarSize"].Value;
          this.strMarkNumber = match4.Groups["controlMark"].Value;
          this.FinishMark = match4.Groups["finishMark"].Value;
          this.CoreMark = match4.Groups["coreMark"].Value;
          int result;
          if (int.TryParse(this.strMarkNumber, out result))
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
            break;
          }
          this.IsValidParse = false;
          this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
          QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          break;
        }
        if (!match5.Success)
          break;
        this.barSize = "-1";
        this.strMarkNumber = "-1";
        this.FinishMark = "-1";
        this.CoreMark = "-1";
        break;
      case ManufacturerKeying.MidStates:
        Match match6 = BarMarkInfo._midstatesMarkMatcher.Match(barMark);
        if (match6.Success)
        {
          this.Prefix = match6.Groups["prefix"].Value;
          this.barSize = match6.Groups["strBarSize"].Value;
          this.FinishMark = match6.Groups["finishMarks"].Value;
          this.strMarkNumber = match6.Groups["controlMark"].Value;
          this.CoreMark = match6.Groups["coreMark"].Value;
          this.totalLength = match6.Groups["total"].Value;
          int result = RebarControlMarkManager3.GetIntForAlphaMark(this.strMarkNumber, ManufacturerKeying.MidStates);
          if (result >= 1)
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
            QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        break;
      case ManufacturerKeying.Illini:
        Match match7 = new Regex("(?<coreMark>(?<strBarSize>\\d*)-(?<total>\\d+)(?<controlMark>[A-Z]*?)(?(?![EGJS]$)$|))(?<finishMark>[EGJS]?)$", RegexOptions.Compiled).Match(barMark);
        if (match7.Success)
        {
          this.Prefix = match7.Groups["prefix"].Value;
          this.FinishMark = match7.Groups["finishMarks"].Value;
          this.barSize = match7.Groups["strBarSize"].Value;
          this.strMarkNumber = match7.Groups["controlMark"].Value;
          this.CoreMark = match7.Groups["coreMark"].Value;
          this.totalLength = match7.Groups["total"].Value;
          int result = RebarControlMarkManager3.GetIntForAlphaMark(this.strMarkNumber, ManufacturerKeying.Illini);
          if (result >= 1)
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
            QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        break;
      case ManufacturerKeying.Spancrete:
        Match match8 = BarMarkInfo._spancreteMarkMatcher.Match(barMark);
        if (match8.Success)
        {
          this.barSize = match8.Groups["strBarSize"].Value;
          this.FinishMark = match8.Groups["finishMarks"].Value;
          this.totalLength = match8.Groups["total"].Value;
          this.strMarkNumber = match8.Groups["controlMark"].Value;
          this.CoreMark = $"{this.barSize}-{this.totalLength}{this.strMarkNumber}";
          int result = RebarControlMarkManager3.GetIntForAlphaMark(this.strMarkNumber, ManufacturerKeying.Spancrete);
          if (result >= 1)
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
            QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        break;
      case ManufacturerKeying.Tindall:
        Match match9 = new Regex($"(?<coreMark1>(?<prefix>{this._rebarSettings.escPrefix}))(?<finishMarks>[EGX])?(?<coreMark2>(?<strBarSize>\\d{{2}})(?<barShape>{barShape})(?<controlMark>\\d{{{rebarSettings.markDecimalPositionPrepend.Length.ToString()},}}))$", RegexOptions.Compiled).Match(barMark);
        if (match9.Success)
        {
          this.IsValidParse = true;
          this.Prefix = match9.Groups["prefix"].Value;
          this.barSize = match9.Groups["strBarSize"].Value;
          this.FinishMark = match9.Groups["finishMarks"].Value;
          this.strMarkNumber = match9.Groups["controlMark"].Value;
          this.CoreMark = match9.Groups["coreMark1"].Value + match9.Groups["coreMark2"].Value;
          int result = 0;
          if (int.TryParse(this.strMarkNumber, out result))
          {
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
            QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        break;
      default:
        string escPrefix2 = this._rebarSettings.escPrefix;
        string escSuffix2 = this._rebarSettings.escSuffix;
        string escGalvFinish2 = this._rebarSettings.escGalvFinish;
        string escEpoxFinish2 = this._rebarSettings.escEpoxFinish;
        string str1 = $"(?<coreMark>{escPrefix2}\\s*(?<strBarSize>(?(?=(0|1))\\d{{2}}|\\d{{1}}))(?<controlMark>\\d+){escSuffix2}(?(?![{escGalvFinish2}{escEpoxFinish2}]$)$|))(?<finishMark>[{escGalvFinish2}{escEpoxFinish2}]?)$";
        string str2 = $"(?<coreMark>{escPrefix2}\\s*(?<strBarSize>\\d{{2}})(?<controlMark>\\d+){escSuffix2}(?(?![{escGalvFinish2}{escEpoxFinish2}]$)$|))(?<finishMark>[{escGalvFinish2}{escEpoxFinish2}]?)$";
        Match match10 = new Regex(this._rebarSettings.bUseMetricCanadian || BarHasher.useMetricEuro ? str2 : str1).Match(barMark);
        if (match10.Success)
        {
          this.Prefix = match10.Groups["prefix"].Value;
          this.barSize = match10.Groups["strBarSize"].Value;
          this.FinishMark = match10.Groups["finishMarks"].Value;
          this.strMarkNumber = match10.Groups["controlMark"].Value;
          this.CoreMark = match10.Groups["coreMark"].Value;
          int result = 0;
          if (int.TryParse(this.strMarkNumber, out result))
          {
            this.IsValidParse = true;
            this.iMarkNumber = result;
          }
          else
          {
            this.IsValidParse = false;
            this.errorMessage = $"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}";
            QA.Trace($"We couldn't get the integer value of the mark: {this.strMarkNumber} from original mark {barMark}");
          }
          if (int.TryParse(this.barSize, out result))
          {
            this.iBarSize = result;
            break;
          }
          this.IsValidParse = false;
          break;
        }
        this.IsValidParse = false;
        this.errorMessage = "BarMarkInfo couldn't get a match in the barmark: " + barMark;
        QA.Trace("BarMarkInfo couldn't get a match in the barmark: " + barMark);
        string strippedMark2 = "";
        if (this.MarkIsStandardBarMark(barMark, out strippedMark2))
        {
          this.CoreMark = strippedMark2;
          break;
        }
        this.CoreMark = barMark;
        break;
    }
  }

  private bool MarkIsStandardBarMark(string mark, out string strippedMark)
  {
    strippedMark = mark;
    if (!new Regex($"(?:{this._rebarSettings.escEpoxFinish}|{this._rebarSettings.escGalvFinish})+$").Match(mark).Success)
      return this._rebarSettings.StandardBars.Select<Rebar, string>((Func<Rebar, string>) (s => s.BarMark)).ToList<string>().Contains(mark);
    foreach (string str in this._rebarSettings.StandardBars.Select<Rebar, string>((Func<Rebar, string>) (s => s.BarMark)).ToList<string>())
    {
      Regex regex = new Regex($"{str}(?:{this._rebarSettings.escEpoxFinish}|{this._rebarSettings.escGalvFinish})*$");
      if (regex.Match(mark).Success)
      {
        foreach (Capture match in regex.Matches(mark))
        {
          if (match.Value.Equals(mark))
          {
            strippedMark = str;
            return true;
          }
        }
      }
    }
    return false;
  }

  public override string ToString()
  {
    return $"{this.OriginalMarkString} prefix: {this.Prefix} size: {this.barSize} iMarkNum: {this.iMarkNumber.ToString()} Finish: {this.FinishMark} errorMsg: {this.errorMessage}";
  }
}
