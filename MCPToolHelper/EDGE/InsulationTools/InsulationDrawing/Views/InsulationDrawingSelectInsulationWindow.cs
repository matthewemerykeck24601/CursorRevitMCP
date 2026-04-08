// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.Views.InsulationDrawingSelectInsulationWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing.Views;

public class InsulationDrawingSelectInsulationWindow : Window, IComponentConnector, IStyleConnector
{
  private List<string> SelectedInsulationMarks = new List<string>();
  private List<string> insulationMarks = new List<string>();
  public bool cancel = true;
  internal TextBlock DescriptionTextBlock;
  internal TextBox SearchBox;
  internal Label WaterMarkLabel;
  internal ListBox CheckBoxList;
  internal Button ContinueButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public InsulationDrawingSelectInsulationWindow(IntPtr parentWindowHandler, List<string> marks)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.KeyDown += new KeyEventHandler(this.HandleEsc);
    List<InsulationMarkListObject> insulationMarkListObjectList = new List<InsulationMarkListObject>();
    foreach (string str in marks.NaturalSort())
    {
      InsulationMarkListObject insulationMarkListObject = new InsulationMarkListObject(str);
      insulationMarkListObjectList.Add(insulationMarkListObject);
      this.insulationMarks.Add(str);
    }
    this.CheckBoxList.ItemsSource = (IEnumerable) insulationMarkListObjectList;
  }

  private void CheckBox_Checked(object sender, RoutedEventArgs e)
  {
    if (!(sender is CheckBox checkBox))
      return;
    string str = checkBox.Content.ToString();
    if (!this.SelectedInsulationMarks.Contains(str))
      this.SelectedInsulationMarks.Add(str);
    if (this.SelectedInsulationMarks.Count == 0)
      this.ContinueButton.IsEnabled = false;
    else
      this.ContinueButton.IsEnabled = true;
  }

  private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    if (!(sender is CheckBox checkBox))
      return;
    string str = checkBox.Content.ToString();
    if (this.SelectedInsulationMarks.Contains(str))
      this.SelectedInsulationMarks.Remove(str);
    if (this.SelectedInsulationMarks.Count == 0)
      this.ContinueButton.IsEnabled = false;
    else
      this.ContinueButton.IsEnabled = true;
  }

  private void HandleEsc(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Escape)
      return;
    this.Close();
  }

  private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    string text = (sender as TextBox).Text;
    if (string.IsNullOrEmpty(text))
    {
      this.WaterMarkLabel.Visibility = Visibility.Visible;
      List<InsulationMarkListObject> insulationMarkListObjectList = new List<InsulationMarkListObject>();
      foreach (string insulationMark in this.insulationMarks)
        insulationMarkListObjectList.Add(new InsulationMarkListObject(insulationMark)
        {
          IsChecked = this.SelectedInsulationMarks.Contains(insulationMark)
        });
      this.CheckBoxList.ItemsSource = (IEnumerable) insulationMarkListObjectList;
    }
    else
    {
      this.WaterMarkLabel.Visibility = Visibility.Collapsed;
      List<InsulationMarkListObject> insulationMarkListObjectList = new List<InsulationMarkListObject>();
      foreach (string insulationMark in this.insulationMarks)
      {
        if (insulationMark.ToUpper().Contains(text.Trim().ToUpper()))
          insulationMarkListObjectList.Add(new InsulationMarkListObject(insulationMark)
          {
            IsChecked = this.SelectedInsulationMarks.Contains(insulationMark)
          });
      }
      this.CheckBoxList.ItemsSource = (IEnumerable) insulationMarkListObjectList;
    }
  }

  public List<string> GetSelectedMarks()
  {
    return this.SelectedInsulationMarks.NaturalSort().ToList<string>();
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    this.cancel = false;
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/insulationtools/insulationdrawing/views/insulationdrawingselectinsulationwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.DescriptionTextBlock = (TextBlock) target;
        break;
      case 2:
        this.SearchBox = (TextBox) target;
        this.SearchBox.TextChanged += new TextChangedEventHandler(this.SearchBox_TextChanged);
        break;
      case 3:
        this.WaterMarkLabel = (Label) target;
        break;
      case 4:
        this.CheckBoxList = (ListBox) target;
        break;
      case 6:
        this.ContinueButton = (Button) target;
        this.ContinueButton.Click += new RoutedEventHandler(this.ContinueButton_Click);
        break;
      case 7:
        this.CancelButton = (Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 5)
      return;
    ((ToggleButton) target).Checked += new RoutedEventHandler(this.CheckBox_Checked);
    ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.CheckBox_Unchecked);
  }
}
