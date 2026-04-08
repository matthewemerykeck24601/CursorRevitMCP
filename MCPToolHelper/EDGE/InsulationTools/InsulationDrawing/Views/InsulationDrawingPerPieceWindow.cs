// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.Views.InsulationDrawingPerPieceWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketPopulator;
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
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing.Views;

public class InsulationDrawingPerPieceWindow : Window, IComponentConnector
{
  private Dictionary<string, int> insulationScales = new Dictionary<string, int>();
  private InsulationDrawingPerPiece insulPP = new InsulationDrawingPerPiece();
  private bool isCancelled = true;
  private ScaleUnits insulationScaleUnits;
  public ObservableCollection<InsulationDrawingPerPieceObject> perPieceList = new ObservableCollection<InsulationDrawingPerPieceObject>();
  public int ScaleFactor = 1;
  internal DataGrid PerPieceDataGrid;
  internal ComboBox ScalesComboBox;
  internal TextBlock UnmarkedMessage;
  internal Button btnDrawInsulation;
  internal Button btnCancel;
  private bool _contentLoaded;

  public InsulationDrawingPerPieceWindow(Document revitDoc) => this.InitializeComponent();

  public InsulationDrawingPerPieceWindow(
    Document revitDoc,
    ObservableCollection<InsulationDrawingPerPieceObject> Ppo,
    int scaleFactor)
  {
    this.InitializeComponent();
    this.insulPP.PerPieceList = Ppo;
    this.PerPieceDataGrid.ItemsSource = (IEnumerable) Ppo.NatrualSort<InsulationDrawingPerPieceObject>((Func<InsulationDrawingPerPieceObject, string>) (p => p.AssemblyName));
    this.ScaleFactor = scaleFactor;
    this.insulationScaleUnits = ScalesManager.GetScaleUnitsForDocument(revitDoc);
    if (revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId() == UnitTypeId.MetersCentimeters)
      this.insulationScaleUnits = ScaleUnits.Metric;
    this.insulationScales = ScalesManager.GetScalesDictionary(this.insulationScaleUnits);
    string empty = string.Empty;
    List<string> stringList = new List<string>();
    for (int index = 0; index < this.insulationScales.Count; ++index)
      stringList.Add(this.insulationScales.ElementAt<KeyValuePair<string, int>>(index).Key);
    this.ScalesComboBox.ItemsSource = (IEnumerable) stringList;
    bool flag1 = false;
    if (this.insulationScales.ContainsValue(scaleFactor))
    {
      foreach (string key in this.insulationScales.Keys)
      {
        if (this.insulationScales[key] == scaleFactor)
        {
          this.ScalesComboBox.SelectedValue = (object) key;
          flag1 = true;
        }
      }
      if (!flag1)
      {
        stringList.Add("1:" + scaleFactor.ToString());
        this.ScalesComboBox.ItemsSource = (IEnumerable) stringList;
        this.ScalesComboBox.SelectedValue = (object) ("1:" + scaleFactor.ToString());
      }
    }
    else
    {
      stringList.Add("1:" + scaleFactor.ToString());
      this.ScalesComboBox.ItemsSource = (IEnumerable) stringList;
      this.ScalesComboBox.SelectedValue = (object) ("1:" + scaleFactor.ToString());
    }
    this.ScaleFactor = scaleFactor;
    bool flag2 = false;
    foreach (InsulationDrawingPerPieceObject drawingPerPieceObject in (Collection<InsulationDrawingPerPieceObject>) Ppo)
    {
      if (drawingPerPieceObject.ContainsUnmarked)
      {
        flag2 = true;
        break;
      }
    }
    if (flag2)
      this.UnmarkedMessage.Visibility = System.Windows.Visibility.Visible;
    else
      this.UnmarkedMessage.Visibility = System.Windows.Visibility.Collapsed;
  }

  private void SelectFromScale(string scaleValue)
  {
    for (int index = 0; index < this.ScalesComboBox.Items.Count; ++index)
    {
      if (scaleValue == this.ScalesComboBox.Items[index].ToString())
      {
        this.ScalesComboBox.SelectedItem = this.ScalesComboBox.Items[index];
        break;
      }
    }
  }

  private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();

  private void btnDrawInsulation_Click(object sender, RoutedEventArgs e)
  {
    this.perPieceList = this.insulPP.PerPieceList;
    if (this.insulationScales.ContainsKey(this.ScalesComboBox.SelectedValue as string))
      this.ScaleFactor = this.insulationScales[this.ScalesComboBox.SelectedValue as string];
    this.isCancelled = false;
    this.Close();
  }

  private void Window_Closing(object sender, CancelEventArgs e)
  {
    if (!this.isCancelled)
      return;
    this.perPieceList = (ObservableCollection<InsulationDrawingPerPieceObject>) null;
    this.ScaleFactor = 0;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/insulationtools/insulationdrawing/views/insulationdrawingperpiecewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Closing += new CancelEventHandler(this.Window_Closing);
        break;
      case 2:
        this.PerPieceDataGrid = (DataGrid) target;
        break;
      case 3:
        this.ScalesComboBox = (ComboBox) target;
        break;
      case 4:
        this.UnmarkedMessage = (TextBlock) target;
        break;
      case 5:
        this.btnDrawInsulation = (Button) target;
        this.btnDrawInsulation.Click += new RoutedEventHandler(this.btnDrawInsulation_Click);
        break;
      case 6:
        this.btnCancel = (Button) target;
        this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
