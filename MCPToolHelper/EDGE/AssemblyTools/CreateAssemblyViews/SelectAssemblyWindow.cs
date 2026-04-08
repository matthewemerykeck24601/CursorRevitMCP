// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.CreateAssemblyViews.SelectAssemblyWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AssemblyTools.CreateAssemblyViews;

public class SelectAssemblyWindow : Window, IComponentConnector
{
  private UIDocument uidoc;
  public AssemblyDataModel selectedAssembly;
  public bool IsCancelled = true;
  internal ListBox lstAssemblyList;
  private bool _contentLoaded;

  public SelectAssemblyWindow(UIDocument doc, IntPtr revitParentWindowHandle)
  {
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = revitParentWindowHandle;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.InitializeComponent();
    this.uidoc = doc;
    ViewModel viewModel = new ViewModel(this.uidoc);
    this.DataContext = (object) viewModel;
    this.lstAssemblyList.DataContext = (object) viewModel;
    this.lstAssemblyList.ItemsSource = (IEnumerable) viewModel.Assemblies;
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    if (this.lstAssemblyList.SelectedItem == null)
    {
      TaskDialog.Show("No Selection", "Please select an assembly for which to create views, or cancel the command");
    }
    else
    {
      this.selectedAssembly = this.lstAssemblyList.SelectedItem as AssemblyDataModel;
      this.IsCancelled = false;
      this.Close();
    }
  }

  private void Button_Click_1(object sender, RoutedEventArgs e)
  {
    this.IsCancelled = true;
    this.Close();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/createassemblyviews/selectassemblywindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
        break;
      case 2:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click_1);
        break;
      case 3:
        this.lstAssemblyList = (ListBox) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
