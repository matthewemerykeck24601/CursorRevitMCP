// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.Views.BFUParameterTaskViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater.Views;

public class BFUParameterTaskViewModel : INotifyPropertyChanged
{
  private string _parameterName;
  private string _currentParameterName;
  private ObservableCollection<ComboBoxItemString> _disciplineItems = new ObservableCollection<ComboBoxItemString>();
  private string _selectedDisciplineItem;
  private ObservableCollection<ComboBoxItemString> _typeItems = new ObservableCollection<ComboBoxItemString>();
  private string _selectedTypeItem;
  private ObservableCollection<ComboBoxItemString> _groupUnderItems = new ObservableCollection<ComboBoxItemString>();
  private string _selectedGroupUnderItems;
  private string _parameterValue;
  private string _parameterValueUnits;
  private string _valueTextBoxHint = "";
  private bool? _typeCheckBoxBool = new bool?(false);
  private bool? _instanceCheckBoxBool = new bool?(false);
  private bool? _formulaCheckBoxBool = new bool?(false);
  private bool? _clearValueCheckBoxBool = new bool?(false);
  private bool? _yesValueCheckBoxBool = new bool?(false);
  private bool? _noValueCheckBoxBool = new bool?(false);

  public string ParameterName
  {
    get => this._parameterName;
    set
    {
      if (!(value != this._parameterName))
        return;
      this._parameterName = value;
      this.OnPropertyChanged(nameof (ParameterName));
    }
  }

  public string CurrentParmaeterName
  {
    get => this._currentParameterName;
    set
    {
      if (!(this._currentParameterName != value))
        return;
      this._currentParameterName = value;
      this.OnPropertyChanged(nameof (CurrentParmaeterName));
    }
  }

  public ObservableCollection<ComboBoxItemString> DisciplineItems
  {
    get => this._disciplineItems;
    set
    {
      if (value == this._disciplineItems)
        return;
      this._disciplineItems = value;
      this.OnPropertyChanged(nameof (DisciplineItems));
    }
  }

  public string SelectedDisciplineItem
  {
    get => this._selectedDisciplineItem;
    set
    {
      if (!(this._selectedDisciplineItem != value))
        return;
      this._selectedDisciplineItem = value;
      this.OnPropertyChanged(nameof (SelectedDisciplineItem));
    }
  }

  public ObservableCollection<ComboBoxItemString> TypeItems
  {
    get => this._typeItems;
    set
    {
      if (value == this._typeItems)
        return;
      this._typeItems = value;
      this.OnPropertyChanged(nameof (TypeItems));
    }
  }

  public string SelectedTypeItem
  {
    get => this._selectedTypeItem;
    set
    {
      if (!(value != this._selectedTypeItem))
        return;
      this._selectedTypeItem = value;
      this.OnPropertyChanged(nameof (SelectedTypeItem));
    }
  }

  public ObservableCollection<ComboBoxItemString> GroupUnderItems
  {
    get => this._groupUnderItems;
    set
    {
      if (value == this._groupUnderItems)
        return;
      this._groupUnderItems = value;
      this.OnPropertyChanged(nameof (GroupUnderItems));
    }
  }

  public string SelectedGroupUnderItem
  {
    get => this._selectedGroupUnderItems;
    set
    {
      if (!(value != this._selectedGroupUnderItems))
        return;
      this._selectedGroupUnderItems = value;
      this.OnPropertyChanged(nameof (SelectedGroupUnderItem));
    }
  }

  public string ParameterValue
  {
    get => this._parameterValue;
    set
    {
      if (!(value != this._parameterValue))
        return;
      this._parameterValue = value;
      this.OnPropertyChanged(nameof (ParameterValue));
    }
  }

  public string ParameterValueUnits
  {
    get => this._parameterValueUnits;
    set
    {
      if (!(value != this._parameterValueUnits))
        return;
      this._parameterValueUnits = value;
      this.OnPropertyChanged(nameof (ParameterValueUnits));
    }
  }

  public string ValueTextBoxHint
  {
    get => this._valueTextBoxHint;
    set
    {
      if (!(value != this._valueTextBoxHint))
        return;
      this._valueTextBoxHint = value;
      this.OnPropertyChanged(nameof (ValueTextBoxHint));
    }
  }

  public bool? TypeCheckBoxBool
  {
    get => this._typeCheckBoxBool;
    set
    {
      bool? nullable = value;
      bool? typeCheckBoxBool = this._typeCheckBoxBool;
      if (nullable.GetValueOrDefault() == typeCheckBoxBool.GetValueOrDefault() & nullable.HasValue == typeCheckBoxBool.HasValue)
        return;
      this._typeCheckBoxBool = value;
      this.OnPropertyChanged(nameof (TypeCheckBoxBool));
    }
  }

  public bool? InstanceCheckBoxBool
  {
    get => this._instanceCheckBoxBool;
    set
    {
      bool? nullable = value;
      bool? instanceCheckBoxBool = this._instanceCheckBoxBool;
      if (nullable.GetValueOrDefault() == instanceCheckBoxBool.GetValueOrDefault() & nullable.HasValue == instanceCheckBoxBool.HasValue)
        return;
      this._instanceCheckBoxBool = value;
      this.OnPropertyChanged("TypeCheckBoxBool");
    }
  }

  public bool? FormulaCheckBoxBool
  {
    get => this._formulaCheckBoxBool;
    set
    {
      bool? nullable = value;
      bool? formulaCheckBoxBool = this._formulaCheckBoxBool;
      if (nullable.GetValueOrDefault() == formulaCheckBoxBool.GetValueOrDefault() & nullable.HasValue == formulaCheckBoxBool.HasValue)
        return;
      this._formulaCheckBoxBool = value;
      this.OnPropertyChanged(nameof (FormulaCheckBoxBool));
    }
  }

  public bool? ClearValueCheckBoxBool
  {
    get => this._clearValueCheckBoxBool;
    set
    {
      bool? nullable = value;
      bool? valueCheckBoxBool = this._clearValueCheckBoxBool;
      if (nullable.GetValueOrDefault() == valueCheckBoxBool.GetValueOrDefault() & nullable.HasValue == valueCheckBoxBool.HasValue)
        return;
      this._clearValueCheckBoxBool = value;
      this.OnPropertyChanged("TypeCheckBoxBool");
    }
  }

  public bool? YesCheckBoxBool
  {
    get => this._yesValueCheckBoxBool;
    set
    {
      bool? nullable = value;
      bool? valueCheckBoxBool = this._yesValueCheckBoxBool;
      if (nullable.GetValueOrDefault() == valueCheckBoxBool.GetValueOrDefault() & nullable.HasValue == valueCheckBoxBool.HasValue)
        return;
      this._yesValueCheckBoxBool = value;
      this.OnPropertyChanged(nameof (YesCheckBoxBool));
    }
  }

  public bool? NoCheckBoxBool
  {
    get => this._noValueCheckBoxBool;
    set
    {
      bool? nullable = value;
      bool? valueCheckBoxBool = this._noValueCheckBoxBool;
      if (nullable.GetValueOrDefault() == valueCheckBoxBool.GetValueOrDefault() & nullable.HasValue == valueCheckBoxBool.HasValue)
        return;
      this._noValueCheckBoxBool = value;
      this.OnPropertyChanged(nameof (NoCheckBoxBool));
    }
  }

  public BFUParameterTaskViewModel(BulkFamilyUpdatersUtils.taskActions action)
  {
    switch (action)
    {
      case BulkFamilyUpdatersUtils.taskActions.Add:
        this.ValueTextBoxHint = "Unassigned";
        break;
      case BulkFamilyUpdatersUtils.taskActions.Modify:
        this.ValueTextBoxHint = "Unchanged";
        break;
    }
    this.DisciplineItems.Clear();
    if (action != BulkFamilyUpdatersUtils.taskActions.Add)
      this.DisciplineItems.Add(new ComboBoxItemString(""));
    foreach (KeyValuePair<ForgeTypeId, List<ForgeTypeId>> parameterSpecTypeId in BulkFamilyUpdatersUtils.disciplineForgeTypeIdToParameterSpecTypeIdDictionary)
    {
      if (parameterSpecTypeId.Value.Count<ForgeTypeId>() != 0)
        this.DisciplineItems.Add(new ComboBoxItemString(LabelUtils.GetLabelForDiscipline(parameterSpecTypeId.Key)));
    }
    if (action == BulkFamilyUpdatersUtils.taskActions.Add)
      this.SelectedDisciplineItem = "Common";
    this.GroupUnderItems.Clear();
    if (action != BulkFamilyUpdatersUtils.taskActions.Add)
      this.GroupUnderItems.Add(new ComboBoxItemString(""));
    foreach (KeyValuePair<BulkFamilyUpdatersUtils.groupParameterUnder, string> parameterUnderToString in BulkFamilyUpdatersUtils.groupParameterUnderToStringDictionary)
      this.GroupUnderItems.Add(new ComboBoxItemString(parameterUnderToString.Value));
    if (action != BulkFamilyUpdatersUtils.taskActions.Add)
      return;
    this.SelectedGroupUnderItem = BulkFamilyUpdatersUtils.groupParameterUnderToStringDictionary[BulkFamilyUpdatersUtils.groupParameterUnder.General];
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
