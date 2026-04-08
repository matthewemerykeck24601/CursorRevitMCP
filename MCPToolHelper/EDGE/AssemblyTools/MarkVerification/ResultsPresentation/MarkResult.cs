// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.MarkResult
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class MarkResult : INotifyPropertyChanged, IComparable
{
  private string _controlMark;
  private int _pieceCount;
  private bool _passFail;
  private bool _passPlateRotate;
  private bool _useCountMultiplier;
  private List<TestResult> _testResults;
  private List<GroupTestResult> _groupTestResult;
  private List<string> _elementIds;
  private string _description;
  private string _constructionProduct;
  private string _resultImagePath;
  private string _isselected;
  private string _color;

  public string ControlMark
  {
    get => this._controlMark;
    set
    {
      this._controlMark = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (ControlMark)));
    }
  }

  public int PieceCount
  {
    get => this._pieceCount;
    set
    {
      this._pieceCount = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (PieceCount)));
    }
  }

  public bool Verified
  {
    get => this._passFail;
    set
    {
      this._passFail = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (Verified)));
    }
  }

  public bool PlateRotated
  {
    get => this._passPlateRotate;
    set
    {
      this._passPlateRotate = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (PlateRotated)));
    }
  }

  public bool UseCountMultiplier
  {
    get => this._useCountMultiplier;
    set
    {
      this._useCountMultiplier = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (UseCountMultiplier)));
    }
  }

  public List<TestResult> TestResults
  {
    get => this._testResults;
    set
    {
      this._testResults = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (TestResults)));
    }
  }

  public List<GroupTestResult> GroupTestResults
  {
    get => this._groupTestResult;
    set
    {
      this._groupTestResult = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (GroupTestResults)));
    }
  }

  public List<string> ElementIds
  {
    get => this._elementIds;
    set
    {
      this._elementIds = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (ElementIds)));
    }
  }

  public string Description
  {
    get => this._description;
    set
    {
      this._description = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (Description)));
    }
  }

  public string ConstructionProduct
  {
    get => this._constructionProduct;
    set
    {
      this._constructionProduct = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (ConstructionProduct)));
    }
  }

  public string IsSelected
  {
    get => this._isselected;
    set
    {
      this._isselected = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (IsSelected)));
      if (!value.Equals((object) true))
        return;
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.LightSteelBlue);
    }
  }

  public string Color
  {
    get => this._color;
    set
    {
      this._color = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (Color)));
    }
  }

  public List<Plates> FailedRotationList { get; set; }

  public List<Plates> CountMultiplierList { get; set; }

  public MarkResult(MarkResult result)
  {
    this.Description = result.Description;
    this.ConstructionProduct = result.ConstructionProduct;
    this.ControlMark = result.ControlMark;
    this.TestResults = result.TestResults;
    this.Verified = result.Verified;
    this.PieceCount = result.PieceCount;
    this.PlateRotated = result.PlateRotated;
    this.UseCountMultiplier = result.UseCountMultiplier;
    this.GroupTestResults = result.GroupTestResults;
    this.FailedRotationList = result.FailedRotationList;
    this.CountMultiplierList = result.CountMultiplierList;
  }

  public MarkResult()
  {
    this.TestResults = new List<TestResult>();
    this.GroupTestResults = new List<GroupTestResult>();
    this.Verified = false;
  }

  public string ResultImagePath
  {
    get => this._resultImagePath;
    set => this._resultImagePath = value;
  }

  public override string ToString() => "";

  public event PropertyChangedEventHandler PropertyChanged;

  public void OnPropertyChanged(PropertyChangedEventArgs e)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, e);
  }

  public int CompareTo(object other)
  {
    MarkResult markResult = other as MarkResult;
    int num1 = 0;
    if (markResult == null)
      return -1;
    Regex regex1 = new Regex("\\D+");
    Regex regex2 = new Regex("\\d+");
    MatchCollection matchCollection1 = regex1.Matches(this.ControlMark);
    MatchCollection matchCollection2 = regex1.Matches(markResult.ControlMark);
    MatchCollection matchCollection3 = regex2.Matches(this.ControlMark);
    MatchCollection matchCollection4 = regex2.Matches(markResult.ControlMark);
    List<Match> matchList1 = new List<Match>();
    List<Match> matchList2 = new List<Match>();
    foreach (Match match in matchCollection1)
      matchList1.Add(match);
    foreach (Match match in matchCollection3)
      matchList1.Add(match);
    foreach (Match match in matchCollection2)
      matchList2.Add(match);
    foreach (Match match in matchCollection4)
      matchList2.Add(match);
    matchList1.Sort((Comparison<Match>) ((p, q) => p.Index.CompareTo(q.Index)));
    matchList2.Sort((Comparison<Match>) ((p, q) => p.Index.CompareTo(q.Index)));
    int num2 = matchList1.Count > matchList2.Count ? matchList1.Count : matchList2.Count;
    if (matchList1.Count != matchList2.Count)
      return matchList1.Count >= matchList2.Count ? 1 : -1;
    for (int index = 0; index < num2; ++index)
    {
      int result1 = 0;
      int result2 = 0;
      if (int.TryParse(matchList1[index].ToString(), out result1) && int.TryParse(matchList2[index].ToString(), out result2))
      {
        if (result1 != result2)
          return result1 < result2 ? -1 : 1;
        num1 = 0;
      }
      else
      {
        num1 = matchList1[index].ToString().CompareTo(matchList2[index].ToString());
        if (num1 != 0)
          return num1;
      }
    }
    return num1;
  }
}
