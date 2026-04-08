// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.GroupTestResult
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.ComponentModel;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class GroupTestResult : INotifyPropertyChanged
{
  private int _quantity;
  private List<FamilyInstance> _groups;
  private List<TestResult> _groupResult;
  private List<MemberDetails> _detail;

  public int GroupQuantity
  {
    get => this._quantity;
    set
    {
      this._quantity = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (GroupQuantity)));
    }
  }

  public List<FamilyInstance> GroupMembers
  {
    get => this._groups;
    set
    {
      this._groups = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (GroupMembers)));
    }
  }

  public List<TestResult> GroupResults
  {
    get => this._groupResult;
    set
    {
      this._groupResult = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (GroupResults)));
    }
  }

  public List<MemberDetails> GroupDetails
  {
    get => this._detail;
    set
    {
      this._detail = value;
      this.OnPropertyChanged(new PropertyChangedEventArgs(nameof (GroupDetails)));
    }
  }

  public GroupTestResult(List<FamilyInstance> members, List<TestResult> results)
  {
    this.GroupMembers = members;
    this.GroupResults = results;
    this.GroupQuantity = members.Count;
    this.GroupDetails = new List<MemberDetails>();
    foreach (FamilyInstance member in members)
      this.GroupDetails.Add(new MemberDetails(member.Id.ToString(), Parameters.GetParameterAsString((Element) member, "CONTROL_MARK"), Parameters.GetParameterAsString((Element) member, "CONTROL_NUMBER")));
  }

  public GroupTestResult(List<FamilyInstance> members)
  {
    this.GroupMembers = members;
    this.GroupQuantity = members.Count;
    this.GroupResults = new List<TestResult>();
    this.GroupDetails = new List<MemberDetails>();
    foreach (FamilyInstance member in members)
      this.GroupDetails.Add(new MemberDetails(member.Id.ToString(), Parameters.GetParameterAsString((Element) member, "CONTROL_MARK"), Parameters.GetParameterAsString((Element) member, "CONTROL_NUMBER")));
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public void OnPropertyChanged(PropertyChangedEventArgs e)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, e);
  }
}
