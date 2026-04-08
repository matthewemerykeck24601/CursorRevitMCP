// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.Views.BFUSelectViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater.Views;

public class BFUSelectViewModel : INotifyPropertyChanged
{
  private ObservableCollection<ComboBoxItemString> _parameterGroupsList = new ObservableCollection<ComboBoxItemString>();
  private string _selectedParameterGroup;
  private ObservableCollection<string> _parametersList = new ObservableCollection<string>();
  private string _selectedParameter;

  public ObservableCollection<ComboBoxItemString> ParameterGroupsList
  {
    get => this._parameterGroupsList;
    set
    {
      if (this._parameterGroupsList == value)
        return;
      this._parameterGroupsList = value;
      this.OnPropertyChanged(nameof (ParameterGroupsList));
    }
  }

  public string SelectedParameterGroup
  {
    get => this._selectedParameterGroup;
    set
    {
      if (!(this._selectedParameterGroup != value))
        return;
      this._selectedParameterGroup = value;
      this.OnPropertyChanged(nameof (SelectedParameterGroup));
    }
  }

  public ObservableCollection<string> ParametersList
  {
    get => this._parametersList;
    set
    {
      if (this._parametersList == value)
        return;
      this._parametersList = value;
      this.OnPropertyChanged(nameof (ParametersList));
    }
  }

  public string SelectedParameter
  {
    get => this._selectedParameter;
    set
    {
      if (!(this._selectedParameter != value))
        return;
      this._selectedParameter = value;
      this.OnPropertyChanged(nameof (SelectedParameter));
    }
  }

  public BFUSelectViewModel(IList<FamilyParameter> familyParameters, List<string> usedParameters)
  {
    List<string> stringList = new List<string>();
    foreach (FamilyParameter familyParameter in (IEnumerable<FamilyParameter>) familyParameters)
    {
      if (!usedParameters.Contains(familyParameter.Definition.Name.Trim()) && !familyParameter.IsShared)
        stringList.Add(familyParameter.Definition.Name);
    }
    stringList.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    foreach (string str in stringList)
      this.ParametersList.Add(str);
  }

  public BFUSelectViewModel(List<string> groupNames)
  {
    foreach (string groupName in groupNames)
      this.ParameterGroupsList.Add(new ComboBoxItemString(groupName));
    this.SelectedParameterGroup = this.ParameterGroupsList[0].ValueString;
  }

  public event PropertyChangedEventHandler PropertyChanged;

  [DebuggerHidden]
  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }
}
