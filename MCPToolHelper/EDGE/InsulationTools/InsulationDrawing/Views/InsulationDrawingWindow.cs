// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.Views.InsulationDrawingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing.Views;

public class InsulationDrawingWindow : Window, IComponentConnector
{
  public bool isClosed;
  private List<string> InsulationDetailList = new List<string>();
  private View activeView;
  private UIDocument uiDoc;
  private ExternalEvent m_ExEvent;
  private InsulationDrawingExternalEvent m_Handler;
  private string documentPathName;
  private bool viewSelected;
  internal TextBlock ActiveViewTextBlock;
  internal TextBlock SelectedMarkTextBlock;
  internal TextBlock SheetWarningTextBlock;
  internal Button ContinueButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public InsulationDrawingWindow(
    IntPtr parentWindowHandler,
    UIDocument uiDoc,
    List<string> details,
    ExternalEvent exEvent,
    InsulationDrawingExternalEvent handler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.m_ExEvent = exEvent;
    this.m_Handler = handler;
    this.documentPathName = uiDoc.Document.PathName;
    this.UpdateActiveViewLabel(uiDoc);
    this.InsulationDetailList.AddRange((IEnumerable<string>) details);
    string str = "Selected Marks: " + details[0];
    for (int index = 1; index < details.Count; ++index)
    {
      string detail = details[index];
      str = $"{str}, {detail}";
    }
    this.SelectedMarkTextBlock.Text = str.Trim();
  }

  public void UpdateActiveViewLabel(UIDocument uiDoc)
  {
    if (this.viewSelected)
    {
      this.Close();
    }
    else
    {
      this.uiDoc = uiDoc;
      this.activeView = uiDoc.ActiveView;
      if (!uiDoc.Document.IsFamilyDocument)
      {
        if (uiDoc.Document.PathName != this.documentPathName)
          this.ContinueButton.IsEnabled = false;
        else if (InsulationDrawingUtils.ValidInsulationDrawingView(this.activeView))
          this.ContinueButton.IsEnabled = true;
        else
          this.ContinueButton.IsEnabled = false;
      }
      else
        this.ContinueButton.IsEnabled = false;
      if (this.activeView.ViewType == ViewType.DrawingSheet)
        this.SheetWarningTextBlock.Visibility = System.Windows.Visibility.Visible;
      else
        this.SheetWarningTextBlock.Visibility = System.Windows.Visibility.Collapsed;
      this.ActiveViewTextBlock.Text = "Active View: " + this.activeView.Name;
    }
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    View activeView = this.uiDoc.ActiveView;
    if (activeView.ViewType == ViewType.DrawingSheet)
      new TaskDialog("Warning")
      {
        MainInstruction = "Insulation drawings cannot be placed directly onto sheets.",
        MainContent = "Activate a legend or detail view on the sheet to place insulation drawings."
      }.Show();
    else if (this.activeView.ViewType == ViewType.DrawingSheet && activeView.ViewType != ViewType.Legend && activeView.ViewType != ViewType.Detail)
      new TaskDialog("Warning")
      {
        MainInstruction = "Insulation drawings cannot be placed directly onto sheets.",
        MainContent = "Activate a legend or detail view on the sheet to place insulation drawings."
      }.Show();
    else if (!InsulationDrawingUtils.ValidInsulationDrawingView(activeView))
    {
      new TaskDialog("Warning")
      {
        MainInstruction = "Insulation drawings cannot be placed on the current view.",
        MainContent = "Activate a legend or detail view to place insulation drawings."
      }.Show();
    }
    else
    {
      if (this.uiDoc != null && this.uiDoc.Document.IsWorkshared)
      {
        ICollection<ElementId> UniqueElementIds;
        if (CheckElementsOwnership.CheckOwnership(this.uiDoc.Document, (ICollection<ElementId>) new List<ElementId>()
        {
          this.activeView.Id
        }, out UniqueElementIds, out ICollection<ElementId> _, out ICollection<ElementId> _) && UniqueElementIds.Count != 0)
        {
          new TaskDialog("EDGE Worksharing Error - Insulation Drawing - Mark")
          {
            MainInstruction = "The current view is not editable.",
            MainContent = "The current active view is owned by another user and is not editable. Please coordinate with project members to allow for ownership of the elements or try reloading latest from central."
          }.Show();
          return;
        }
      }
      this.viewSelected = true;
      this.ContinueButton.IsEnabled = false;
      this.m_ExEvent.Raise();
      this.Visibility = System.Windows.Visibility.Hidden;
    }
  }

  private void HandleEsc(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Escape)
      return;
    this.Close();
  }

  private void Window_Closed(object sender, EventArgs e)
  {
    App.insulationDrawingWindow = (InsulationDrawingWindow) null;
    this.isClosed = true;
    if (this.activeView != null)
      this.activeView.Document.Application.WriteJournalComment("Insulation Drawing - Mark window closing. External Event not Disposed Of.", true);
    try
    {
      this.m_ExEvent.Dispose();
      this.m_ExEvent = (ExternalEvent) null;
      this.m_Handler = (InsulationDrawingExternalEvent) null;
    }
    catch (Exception ex)
    {
      if (this.activeView != null)
        this.activeView.Document.Application.WriteJournalComment("Insulation Drawing - Mark window closing. Exception disposing of External Event. \n" + ex.Message, true);
    }
    if (this.activeView == null)
      return;
    this.activeView.Document.Application.WriteJournalComment("Insulation Drawing - Mark window closing. External Event Has been Disposed Of.", true);
  }

  private void Window_MouseDown(object sender, MouseButtonEventArgs e)
  {
    Window window = sender as Window;
    if (e.ChangedButton != MouseButton.Left)
      return;
    try
    {
      window.DragMove();
    }
    catch (Exception ex)
    {
    }
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/insulationtools/insulationdrawing/views/insulationdrawingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.Window_MouseDown);
        ((Window) target).Closed += new EventHandler(this.Window_Closed);
        break;
      case 2:
        this.ActiveViewTextBlock = (TextBlock) target;
        break;
      case 3:
        this.SelectedMarkTextBlock = (TextBlock) target;
        break;
      case 4:
        this.SheetWarningTextBlock = (TextBlock) target;
        break;
      case 5:
        this.ContinueButton = (Button) target;
        this.ContinueButton.Click += new RoutedEventHandler(this.ContinueButton_Click);
        break;
      case 6:
        this.CancelButton = (Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
