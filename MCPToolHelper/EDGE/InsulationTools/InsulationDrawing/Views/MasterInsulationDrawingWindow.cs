// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.Views.MasterInsulationDrawingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing.Views;

public class MasterInsulationDrawingWindow : Window, IComponentConnector
{
  public bool cancel = true;
  public bool DoNotAddToExistingSheet;
  public int selectedScale = 1;
  private Dictionary<string, int> insulationScales = new Dictionary<string, int>();
  internal TextBlock InsulationTitleLable;
  internal ComboBox ScaleComboBox;
  internal CheckBox StartOnNewSheet;
  internal Button ContinueButton;
  private bool _contentLoaded;

  public MasterInsulationDrawingWindow(
    IntPtr parentWindowHandler,
    Document revitDoc,
    int defaultScale,
    string firstInsulationMark = "")
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    if (!string.IsNullOrWhiteSpace(firstInsulationMark))
    {
      this.InsulationTitleLable.Text = $"Insulation starting with {firstInsulationMark} will be drawn.";
      this.StartOnNewSheet.Visibility = System.Windows.Visibility.Visible;
    }
    this.insulationScales = ScalesManager.GetScalesDictionary(ScalesManager.GetScaleUnitsForDocument(revitDoc));
    List<string> stringList = new List<string>();
    for (int index = 0; index < this.insulationScales.Count; ++index)
      stringList.Add(this.insulationScales.ElementAt<KeyValuePair<string, int>>(index).Key);
    this.ScaleComboBox.ItemsSource = (IEnumerable) stringList;
    bool flag = false;
    if (this.insulationScales.ContainsValue(defaultScale))
    {
      foreach (string key in this.insulationScales.Keys)
      {
        if (this.insulationScales[key] == defaultScale)
        {
          this.ScaleComboBox.SelectedValue = (object) key;
          flag = true;
        }
      }
      if (!flag)
      {
        stringList.Add("1:" + defaultScale.ToString());
        this.ScaleComboBox.ItemsSource = (IEnumerable) stringList;
        this.ScaleComboBox.SelectedValue = (object) ("1:" + defaultScale.ToString());
      }
    }
    else
    {
      stringList.Add("1:" + defaultScale.ToString());
      this.ScaleComboBox.ItemsSource = (IEnumerable) stringList;
      this.ScaleComboBox.SelectedValue = (object) ("1:" + defaultScale.ToString());
    }
    this.selectedScale = defaultScale;
  }

  private void StartOnNewSheet_Checked(object sender, RoutedEventArgs e)
  {
    this.DoNotAddToExistingSheet = true;
  }

  private void StartOnNewSheet_Unchecked(object sender, RoutedEventArgs e)
  {
    this.DoNotAddToExistingSheet = false;
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    this.cancel = false;
    if (this.insulationScales.ContainsKey(this.ScaleComboBox.SelectedValue as string))
      this.selectedScale = this.insulationScales[this.ScaleComboBox.SelectedValue as string];
    this.Close();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/insulationtools/insulationdrawing/views/masterinsulationdrawingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.InsulationTitleLable = (TextBlock) target;
        break;
      case 2:
        this.ScaleComboBox = (ComboBox) target;
        break;
      case 3:
        this.StartOnNewSheet = (CheckBox) target;
        this.StartOnNewSheet.Checked += new RoutedEventHandler(this.StartOnNewSheet_Checked);
        this.StartOnNewSheet.Unchecked += new RoutedEventHandler(this.StartOnNewSheet_Unchecked);
        break;
      case 4:
        this.ContinueButton = (Button) target;
        this.ContinueButton.Click += new RoutedEventHandler(this.ContinueButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
