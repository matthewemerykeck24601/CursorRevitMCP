// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.Views.TemplateCreatorWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.Views;

public class TemplateCreatorWindow : Window
{
  public AssemblyInstance assemblyInstance;
  public View ActiveView;
  public Document RevitDocument;
  public UIDocument UiDoc;
  public UIApplication app;

  [DllImport("User32.dll")]
  public static extern bool SetForegroundWindow(IntPtr handle);

  public TemplateCreatorWindow()
  {
  }

  public TemplateCreatorWindow(
    AssemblyInstance assembly,
    View activeViewTemp,
    Document revitDocuemnt,
    UIDocument uidoc,
    UIApplication uiApp,
    IntPtr parentWindowHandler)
  {
    this.DataContext = (object) new TemplateCreatorViewModel(this, assembly);
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    TemplateCreatorViewModel dataContext = this.DataContext as TemplateCreatorViewModel;
    dataContext.ActiveView = activeViewTemp;
    this.ActiveView = activeViewTemp;
    dataContext.assembly = assembly;
    this.assemblyInstance = assembly;
    dataContext.revitDoc = revitDocuemnt;
    this.RevitDocument = revitDocuemnt;
    dataContext.uiDoc = uidoc;
    this.UiDoc = uidoc;
    this.app = uiApp;
  }

  public virtual void templateComboBox_TextChanged(object sender, TextChangedEventArgs e)
  {
  }

  private void OnClosing(object sender, CancelEventArgs e)
  {
    EDGERCreateTemplate.windowExists = false;
    App.templateCreatorWindow = (TicketTemplateCreatorWindow) null;
    try
    {
      TemplateCreatorWindow.SetForegroundWindow(this.app.MainWindowHandle);
    }
    catch (Exception ex)
    {
    }
  }

  private void OnPageLoaded(object sender, RoutedEventArgs e)
  {
    (this.DataContext as TemplateCreatorViewModel).LoadForm();
  }

  public void setAssemblyInstance(AssemblyInstance assIn)
  {
    this.assemblyInstance = assIn;
    (this.DataContext as TemplateCreatorViewModel).assembly = this.assemblyInstance;
  }

  public void setActiveView(View av)
  {
    this.ActiveView = av;
    (this.DataContext as TemplateCreatorViewModel).ActiveView = this.ActiveView;
  }

  public void setRevitDoc(Document revitDocument)
  {
    this.RevitDocument = revitDocument;
    (this.DataContext as TemplateCreatorViewModel).revitDoc = this.RevitDocument;
  }

  public void setUiDoc(UIDocument uidoc)
  {
    this.UiDoc = uidoc;
    (this.DataContext as TemplateCreatorViewModel).uiDoc = this.UiDoc;
  }

  public void setUiApp(UIApplication uiApp) => this.app = uiApp;
}
