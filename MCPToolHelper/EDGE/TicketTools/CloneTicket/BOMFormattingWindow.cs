// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.BOMFormattingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class BOMFormattingWindow : Window, IComponentConnector
{
  internal DataGrid dataGrid;
  internal Button TopButton;
  internal Button BottomButton;
  internal Button StackButton;
  internal Button IndependentButton;
  internal Button ContinueButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public BOMFormattingWindow(List<ViewSheet> viewSheets)
  {
    this.InitializeComponent();
    BOMFormattingViewModel formattingViewModel = new BOMFormattingViewModel();
    foreach (ViewSheet viewSheet in viewSheets)
      formattingViewModel.SheetList.Add(new sheetListItem(EDGE.TicketTools.CloneTicket.CloneTicket.getSheetTitle(viewSheet.SheetNumber, viewSheet.Name)));
    this.DataContext = (object) formattingViewModel;
  }

  private void TopButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (sheetListItem sheet in (this.DataContext as BOMFormattingViewModel).SheetList)
      sheet.AlignmentSelection = "Top";
    this.dataGrid.Items.Refresh();
  }

  private void BottomButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (sheetListItem sheet in (this.DataContext as BOMFormattingViewModel).SheetList)
      sheet.AlignmentSelection = "Bottom";
    this.dataGrid.Items.Refresh();
  }

  private void StackButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (sheetListItem sheet in (this.DataContext as BOMFormattingViewModel).SheetList)
      sheet.StackingSelection = "Stack";
    this.dataGrid.Items.Refresh();
  }

  private void IndependentButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (sheetListItem sheet in (this.DataContext as BOMFormattingViewModel).SheetList)
      sheet.StackingSelection = "Independent";
    this.dataGrid.Items.Refresh();
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    this.DialogResult = new bool?(true);
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
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/cloneticket/bomformattingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.dataGrid = (DataGrid) target;
        break;
      case 2:
        this.TopButton = (Button) target;
        this.TopButton.Click += new RoutedEventHandler(this.TopButton_Click);
        break;
      case 3:
        this.BottomButton = (Button) target;
        this.BottomButton.Click += new RoutedEventHandler(this.BottomButton_Click);
        break;
      case 4:
        this.StackButton = (Button) target;
        this.StackButton.Click += new RoutedEventHandler(this.StackButton_Click);
        break;
      case 5:
        this.IndependentButton = (Button) target;
        this.IndependentButton.Click += new RoutedEventHandler(this.IndependentButton_Click);
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
}
