// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.ResultsPresentation.PlateWarnings
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
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.ResultsPresentation;

public class PlateWarnings : Window, IComponentConnector
{
  private UIDocument uiDoc;
  private UIApplication uiApp;
  private MKVerificationResults_Existing SourceForm;
  internal System.Windows.Controls.Grid theGrid;
  internal RowDefinition plateRotationRow;
  internal RowDefinition MultiplyByMarksRow;
  internal TextBlock PlateRotationWarning;
  internal ScrollViewer FamilyScrollBar;
  internal TreeView FamilyName;
  internal TextBlock MultiplyByMarksWarning;
  internal ScrollViewer MultiplyByMarksFamilyScrollBar;
  internal TreeView MultiplyByMarksFamilyName;
  private bool _contentLoaded;

  public PlateWarnings(
    MarkResult selectedMarkResult,
    UIApplication uiapp,
    IntPtr parentWindowHandler,
    MKVerificationResults_Existing source_form)
  {
    this.uiApp = uiapp;
    this.uiDoc = uiapp.ActiveUIDocument;
    this.SourceForm = source_form;
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    List<Plates> failedRotationList = selectedMarkResult.FailedRotationList;
    if (failedRotationList != null)
    {
      failedRotationList.Sort((Comparison<Plates>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Names, q.Names)));
      foreach (Plates plates in failedRotationList)
      {
        this.FamilyScrollBar.Visibility = System.Windows.Visibility.Visible;
        this.FamilyScrollBar.IsEnabled = true;
        this.FamilyName.Visibility = System.Windows.Visibility.Visible;
        this.FamilyName.IsEnabled = true;
        plates.Ids.Sort((Comparison<ElementId>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.ToString(), q.ToString())));
        TreeViewItem newItem1 = new TreeViewItem();
        newItem1.Header = (object) plates.Names;
        foreach (ElementId id in plates.Ids)
        {
          TreeViewItem newItem2 = new TreeViewItem();
          newItem2.Header = (object) id;
          newItem2.Selected += new RoutedEventHandler(this.selection);
          newItem1.Items.Add((object) newItem2);
        }
        newItem1.Selected += new RoutedEventHandler(this.parent);
        this.FamilyName.Items.Add((object) newItem1);
      }
    }
    if (this.FamilyName.Items.Count > 0)
      this.PlateRotationWarning.Visibility = System.Windows.Visibility.Visible;
    else
      this.plateRotationRow.Height = new GridLength(0.0);
    List<Plates> countMultiplierList = selectedMarkResult.CountMultiplierList;
    if (countMultiplierList != null)
    {
      countMultiplierList.Sort((Comparison<Plates>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Names, q.Names)));
      foreach (Plates plates in countMultiplierList)
      {
        this.MultiplyByMarksFamilyScrollBar.Visibility = System.Windows.Visibility.Visible;
        this.MultiplyByMarksFamilyScrollBar.IsEnabled = true;
        this.MultiplyByMarksFamilyName.Visibility = System.Windows.Visibility.Visible;
        this.MultiplyByMarksFamilyName.IsEnabled = true;
        plates.Ids.Sort((Comparison<ElementId>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.ToString(), q.ToString())));
        TreeViewItem newItem3 = new TreeViewItem();
        newItem3.Header = (object) plates.Names;
        foreach (ElementId id in plates.Ids)
        {
          TreeViewItem newItem4 = new TreeViewItem();
          newItem4.Header = (object) id;
          newItem4.Selected += new RoutedEventHandler(this.selection);
          newItem3.Items.Add((object) newItem4);
        }
        newItem3.Selected += new RoutedEventHandler(this.parent);
        this.MultiplyByMarksFamilyName.Items.Add((object) newItem3);
      }
    }
    if (this.MultiplyByMarksFamilyName.Items.Count > 0)
      this.MultiplyByMarksWarning.Visibility = System.Windows.Visibility.Visible;
    else
      this.MultiplyByMarksRow.Height = new GridLength(0.0);
  }

  private void Window_Closed(object sender, EventArgs e)
  {
    this.SourceForm.WarningsBUtton.IsEnabled = true;
    this.FamilyScrollBar.Visibility = System.Windows.Visibility.Collapsed;
    this.FamilyScrollBar.IsEnabled = false;
    this.FamilyName.Visibility = System.Windows.Visibility.Collapsed;
    this.FamilyName.IsEnabled = false;
  }

  public void selection(object sender, RoutedEventArgs e)
  {
    TreeViewItem treeViewItem = sender as TreeViewItem;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    if (!treeViewItem.IsSelectionActive)
      return;
    if (!treeViewItem.HasItems)
    {
      int result;
      int.TryParse(treeViewItem.Header.ToString(), out result);
      Element topLevelElement = this.uiDoc.Document.GetElement(new ElementId(result)).GetTopLevelElement();
      if (topLevelElement != null)
      {
        elementIds.Add(topLevelElement.Id);
        if (!e.Handled)
          e.Handled = true;
      }
    }
    e.Handled = true;
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    this.uiDoc.Selection.SetElementIds(elementIds);
    treeViewItem.IsSelected = false;
  }

  public void parent(object sender, RoutedEventArgs e)
  {
    TreeViewItem treeViewItem = sender as TreeViewItem;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    if (treeViewItem.HasItems && !e.Handled)
    {
      foreach (HeaderedItemsControl headeredItemsControl in (IEnumerable) treeViewItem.Items)
      {
        int result;
        int.TryParse(headeredItemsControl.Header.ToString(), out result);
        Element topLevelElement = this.uiDoc.Document.GetElement(new ElementId(result)).GetTopLevelElement();
        if (topLevelElement != null)
          elementIds.Add(topLevelElement.Id);
      }
    }
    if (elementIds.Count <= 0)
      return;
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    this.uiDoc.Selection.SetElementIds(elementIds);
    treeViewItem.IsSelected = false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/resultspresentation/platewarnings.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Closed += new EventHandler(this.Window_Closed);
        break;
      case 2:
        this.theGrid = (System.Windows.Controls.Grid) target;
        break;
      case 3:
        this.plateRotationRow = (RowDefinition) target;
        break;
      case 4:
        this.MultiplyByMarksRow = (RowDefinition) target;
        break;
      case 5:
        this.PlateRotationWarning = (TextBlock) target;
        break;
      case 6:
        this.FamilyScrollBar = (ScrollViewer) target;
        break;
      case 7:
        this.FamilyName = (TreeView) target;
        break;
      case 8:
        this.MultiplyByMarksWarning = (TextBlock) target;
        break;
      case 9:
        this.MultiplyByMarksFamilyScrollBar = (ScrollViewer) target;
        break;
      case 10:
        this.MultiplyByMarksFamilyName = (TreeView) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
