// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.RebarShapeSelectionWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class RebarShapeSelectionWindow : Window, IComponentConnector
{
  private bool _saved;
  internal ListBox rebarShapeListBox;
  internal TextBox TextBox;
  private bool _contentLoaded;

  public string Foo => this.TextBox.Text;

  public bool isSaved => this._saved;

  public RebarShapeSelectionWindow(IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.rebarShapeListBox.ItemsSource = (IEnumerable) new List<RebarShape>()
    {
      new RebarShape("S1"),
      new RebarShape("S2"),
      new RebarShape("S3"),
      new RebarShape("S4"),
      new RebarShape("S5"),
      new RebarShape("S6"),
      new RebarShape("S11"),
      new RebarShape("T1"),
      new RebarShape("T2"),
      new RebarShape("T3"),
      new RebarShape("T6"),
      new RebarShape("T7"),
      new RebarShape("T8"),
      new RebarShape("T9"),
      new RebarShape("1"),
      new RebarShape("1A"),
      new RebarShape("2"),
      new RebarShape("3"),
      new RebarShape("3A"),
      new RebarShape("4"),
      new RebarShape("4A"),
      new RebarShape("5"),
      new RebarShape("6"),
      new RebarShape("7"),
      new RebarShape("8"),
      new RebarShape("9"),
      new RebarShape("10"),
      new RebarShape("11"),
      new RebarShape("12"),
      new RebarShape("12A"),
      new RebarShape("12B"),
      new RebarShape("13"),
      new RebarShape("14"),
      new RebarShape("14A"),
      new RebarShape("14B"),
      new RebarShape("16"),
      new RebarShape("16A"),
      new RebarShape("17"),
      new RebarShape("17A"),
      new RebarShape("18"),
      new RebarShape("19"),
      new RebarShape("19A"),
      new RebarShape("20"),
      new RebarShape("22"),
      new RebarShape("23"),
      new RebarShape("24"),
      new RebarShape("25"),
      new RebarShape("26"),
      new RebarShape("26A"),
      new RebarShape("26B"),
      new RebarShape("26C")
    };
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    if (this.rebarShapeListBox.SelectedItem is RebarShape selectedItem)
      this.TextBox.Text = selectedItem.RebarName;
    this._saved = true;
    this.Close();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/rebarshapeselectionwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.rebarShapeListBox = (ListBox) target;
        break;
      case 2:
        this.TextBox = (TextBox) target;
        break;
      case 3:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
