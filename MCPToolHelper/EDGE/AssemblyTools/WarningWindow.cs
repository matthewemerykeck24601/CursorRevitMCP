// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.WarningWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AssemblyTools;

public class WarningWindow : Window, IComponentConnector
{
  public bool getCondition;
  internal CheckBox cbYes;
  internal Button btn1;
  private bool _contentLoaded;

  public WarningWindow(IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    this.DataContext = (object) this;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
  }

  private void cbFeature_CheckedChanged(object sender, RoutedEventArgs e)
  {
  }

  private void btn1_Click(object sender, RoutedEventArgs e)
  {
    if (this.cbYes.IsChecked.GetValueOrDefault())
      this.getCondition = true;
    this.Close();
  }

  public bool boolCheckboxOn { get; set; }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/warningwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
      {
        this.btn1 = (Button) target;
        this.btn1.Click += new RoutedEventHandler(this.btn1_Click);
      }
      else
        this._contentLoaded = true;
    }
    else
    {
      this.cbYes = (CheckBox) target;
      this.cbYes.Checked += new RoutedEventHandler(this.cbFeature_CheckedChanged);
      this.cbYes.Unchecked += new RoutedEventHandler(this.cbFeature_CheckedChanged);
    }
  }
}
