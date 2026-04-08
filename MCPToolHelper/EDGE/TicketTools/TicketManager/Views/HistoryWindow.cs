// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.Views.HistoryWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.TicketManager.Views;

public class HistoryWindow : Window, IComponentConnector
{
  internal Label DataGridTitle;
  internal DataGrid DataGrid;
  private bool _contentLoaded;

  public HistoryWindow()
  {
    this.InitializeComponent();
    this.DataContext = (object) new HistoryWindowViewModel();
  }

  public HistoryWindow(AssemblyViewModel model, IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    HistoryWindowViewModel historyWindowViewModel = new HistoryWindowViewModel(model);
    this.DataContext = (object) historyWindowViewModel;
    this.DataGrid.ItemsSource = (IEnumerable) historyWindowViewModel.Comments;
    this.DataGrid.DataContext = (object) new Comment();
    this.DataGridTitle.DataContext = (object) historyWindowViewModel;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/ticketmanager/views/historywindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 1)
    {
      if (connectionId == 2)
        this.DataGrid = (DataGrid) target;
      else
        this._contentLoaded = true;
    }
    else
      this.DataGridTitle = (Label) target;
  }
}
