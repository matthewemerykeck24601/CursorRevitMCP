// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.EnhancementSpreadSheet
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class EnhancementSpreadSheet : Window, IComponentConnector
{
  private List<string> Assemblies = new List<string>();
  private List<SpreadSheetData> SheetData = new List<SpreadSheetData>();
  private Dictionary<AssemblyInstance, List<SpreadSheetData>> mainListOfAssembliesAndParameterValues = new Dictionary<AssemblyInstance, List<SpreadSheetData>>();
  internal System.Windows.Controls.Grid MainGrid;
  internal ComboBox assemblydropdown;
  internal DataGrid DataGrid;
  internal Button ok;
  internal Button cancel;
  private bool _contentLoaded;

  public EnhancementSpreadSheet(
    Dictionary<AssemblyInstance, List<SpreadSheetData>> sheetData)
  {
    this.InitializeComponent();
    this.mainListOfAssembliesAndParameterValues = sheetData;
    this.Assemblies = sheetData.Keys.Select<AssemblyInstance, string>((Func<AssemblyInstance, string>) (e => e.Name)).ToList<string>();
    this.assemblydropdown.ItemsSource = (IEnumerable) this.Assemblies;
  }

  private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    this.DataGrid.ItemsSource = (IEnumerable) this.retrieveList(this.mainListOfAssembliesAndParameterValues);
    this.DataGrid.Items.Refresh();
  }

  private ObservableCollection<SpreadSheetData> retrieveList(
    Dictionary<AssemblyInstance, List<SpreadSheetData>> sheetData)
  {
    ObservableCollection<SpreadSheetData> observableCollection = new ObservableCollection<SpreadSheetData>();
    List<SpreadSheetData> spreadSheetDataList1 = new List<SpreadSheetData>();
    foreach (AssemblyInstance key in sheetData.Keys)
    {
      if (key.Name == this.assemblydropdown.SelectedItem.ToString())
      {
        List<SpreadSheetData> spreadSheetDataList2 = sheetData[key];
        spreadSheetDataList2.Sort((Comparison<SpreadSheetData>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Name, q.Name)));
        foreach (SpreadSheetData spreadSheetData in spreadSheetDataList2)
          observableCollection.Add(spreadSheetData);
      }
    }
    return observableCollection;
  }

  private void Cancel_Click(object sender, RoutedEventArgs e)
  {
    this.DialogResult = new bool?(false);
    this.Close();
  }

  private void Ok_Click(object sender, RoutedEventArgs e)
  {
    this.DialogResult = new bool?(true);
    this.Close();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/autodimensioning/enhancementspreadsheet.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.MainGrid = (System.Windows.Controls.Grid) target;
        break;
      case 2:
        this.assemblydropdown = (ComboBox) target;
        this.assemblydropdown.SelectionChanged += new SelectionChangedEventHandler(this.DataGrid_SelectionChanged);
        break;
      case 3:
        this.DataGrid = (DataGrid) target;
        break;
      case 4:
        this.ok = (Button) target;
        this.ok.Click += new RoutedEventHandler(this.Ok_Click);
        break;
      case 5:
        this.cancel = (Button) target;
        this.cancel.Click += new RoutedEventHandler(this.Cancel_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
