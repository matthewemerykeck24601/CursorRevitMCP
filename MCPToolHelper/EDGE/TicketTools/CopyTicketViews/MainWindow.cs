// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketViews.MainWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.CopyTicketViews;

public class MainWindow : Window, IComponentConnector
{
  private UIDocument _uiDoc;
  internal TextBlock textBlock;
  internal ListBox ListBox;
  private bool _contentLoaded;

  private MainWindow() => this.InitializeComponent();

  public MainWindow(
    UIDocument uidoc,
    ICollection<ElementId> selectedIdList,
    IntPtr parentWindowHandler,
    bool hardwareDetail)
  {
    this._uiDoc = uidoc;
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.textBlock.Text = hardwareDetail ? "Please select the hardware detail sheets the selected views should be copied to." : "Please select the ticket sheets the selected views should be copied to.";
    ViewModel viewModel = new ViewModel(this._uiDoc, selectedIdList, hardwareDetail);
    this.DataContext = (object) viewModel;
    this.ListBox.DataContext = (object) viewModel;
    this.ListBox.ItemsSource = (IEnumerable) viewModel.ViewSheets;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/copyticketviews/mainwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.ListBox = (ListBox) target;
      else
        this._contentLoaded = true;
    }
    else
      this.textBlock = (TextBlock) target;
  }
}
