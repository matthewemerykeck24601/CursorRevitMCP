// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.FindReferringViews.Views.MainWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.FindReferringViews.Models;
using EDGE.TicketTools.FindReferringViews.ViewModels;
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
using System.Windows.Markup;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.FindReferringViews.Views;

public class MainWindow : Window, IComponentConnector, IStyleConnector
{
  internal TabItem selectionTab;
  internal TreeView legendTree;
  internal TreeViewItem legendsItem;
  internal TreeViewItem symbolsItem;
  internal TabItem viewsTab;
  internal TreeView referringViewTree;
  internal TreeViewItem referringLegendTreeItem;
  internal TreeViewItem referringSymbolTreeItem;
  internal Button closeButton;
  internal Button activateViewButton;
  private bool _contentLoaded;

  public MainWindow()
  {
    this.InitializeComponent();
    MainViewModel mainViewModel = new MainViewModel();
    this.legendsItem.ItemsSource = (IEnumerable) mainViewModel.AllLegends;
    this.symbolsItem.ItemsSource = (IEnumerable) mainViewModel.AllSymbols;
  }

  private void closeButton_Click(object sender, RoutedEventArgs e)
  {
    (Window.GetWindow((DependencyObject) this) as MainWindow).Close();
  }

  private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (!this.viewsTab.IsSelected)
      return;
    List<LegendModel> associatedViewSheets1 = MainViewModel.GetAssociatedViewSheets(MainViewModel.CheckedLegends);
    associatedViewSheets1.Sort((Comparison<LegendModel>) ((x, y) => string.Compare(x.Name, y.Name)));
    this.referringLegendTreeItem.ItemsSource = (IEnumerable) null;
    this.referringLegendTreeItem.ItemsSource = (IEnumerable) associatedViewSheets1;
    List<SymbolFamilyModel> associatedViewSheets2 = MainViewModel.GetAssociatedViewSheets(MainViewModel.CheckedSymbols);
    associatedViewSheets2.Sort((Comparison<SymbolFamilyModel>) ((x, y) => string.Compare(x.Name, y.Name)));
    this.referringSymbolTreeItem.ItemsSource = (IEnumerable) null;
    this.referringSymbolTreeItem.ItemsSource = (IEnumerable) associatedViewSheets2;
  }

  private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    if (e.ClickCount < 2)
      return;
    ViewSheetModel dataContext = (sender as TextBlock).DataContext as ViewSheetModel;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    if (dataContext.ParentSymbol != null)
    {
      elementIds.Add(dataContext.ParentSymbol.Id);
      List<Element> list = new FilteredElementCollector(uiDoc.Document).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_GenericAnnotation).ToList<Element>();
      string name = uiDoc.Document.GetElement(dataContext.ParentSymbol.Id).Name;
      foreach (Element element in list)
      {
        if (element.Name.Equals(name) && dataContext.ViewSheet.Name.Equals(uiDoc.Document.GetElement(element.OwnerViewId).Name))
          elementIds.Add(element.Id);
      }
    }
    else
      elementIds.Add(dataContext.ParentLegend.Legend.Id);
    uiDoc.ActiveView = dataContext.ViewSheet;
    uiDoc.Selection.SetElementIds(elementIds);
  }

  private void activateViewButton_Click(object sender, RoutedEventArgs e)
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    ViewSheetModel selectedItem = this.referringViewTree.SelectedItem as ViewSheetModel;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    if (selectedItem.ParentSymbol != null)
      elementIds.Add(selectedItem.ParentSymbol.Id);
    else
      elementIds.Add(selectedItem.ParentLegend.Legend.Id);
    uiDoc.ActiveView = selectedItem.ViewSheet;
    uiDoc.Selection.SetElementIds(elementIds);
  }

  private void referringViewTree_SelectedItemChanged(
    object sender,
    RoutedPropertyChangedEventArgs<object> e)
  {
    if (this.referringViewTree.SelectedItem is ViewSheetModel)
    {
      this.activateViewButton.IsEnabled = true;
      this.activateViewButton.Content = (object) "Activate Selected Sheet";
    }
    else
    {
      this.activateViewButton.IsEnabled = false;
      this.activateViewButton.Content = (object) "Sheet Selection Required";
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/findreferringviews/views/mainwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.TabControl_SelectionChanged);
        break;
      case 2:
        this.selectionTab = (TabItem) target;
        break;
      case 3:
        this.legendTree = (TreeView) target;
        break;
      case 4:
        this.legendsItem = (TreeViewItem) target;
        break;
      case 5:
        this.symbolsItem = (TreeViewItem) target;
        break;
      case 6:
        this.viewsTab = (TabItem) target;
        break;
      case 7:
        this.referringViewTree = (TreeView) target;
        this.referringViewTree.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(this.referringViewTree_SelectedItemChanged);
        break;
      case 8:
        this.referringLegendTreeItem = (TreeViewItem) target;
        break;
      case 10:
        this.referringSymbolTreeItem = (TreeViewItem) target;
        break;
      case 12:
        this.closeButton = (Button) target;
        this.closeButton.Click += new RoutedEventHandler(this.closeButton_Click);
        break;
      case 13:
        this.activateViewButton = (Button) target;
        this.activateViewButton.Click += new RoutedEventHandler(this.activateViewButton_Click);
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
    if (connectionId != 9)
    {
      if (connectionId != 11)
        return;
      ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.TextBlock_MouseDown);
    }
    else
      ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.TextBlock_MouseDown);
  }
}
