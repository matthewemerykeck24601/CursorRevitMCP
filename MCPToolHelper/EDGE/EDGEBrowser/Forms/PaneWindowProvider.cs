// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.Forms.PaneWindowProvider
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows;

#nullable disable
namespace EDGE.EDGEBrowser.Forms;

public class PaneWindowProvider : IDockablePaneProvider, IFrameworkElementCreator
{
  public static DockablePaneId paneId = new DockablePaneId(new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));
  public PaneWindow paneWindow;
  public ExternalEvent ProviderLoadFamilyEvent;
  public ExternalEvent ProviderDownloadFamilyEvent;
  private Document doc;

  public void Register(UIControlledApplication app)
  {
    app.RegisterDockablePane(PaneWindowProvider.paneId, "EDGE^R Browser", (IDockablePaneProvider) this);
    this.ProviderLoadFamilyEvent = ExternalEvent.Create((IExternalEventHandler) new LoadFamily());
  }

  public void SetupDockablePane(DockablePaneProviderData data)
  {
    data.FrameworkElementCreator = (IFrameworkElementCreator) this;
    data.InitialState = new DockablePaneState();
    data.InitialState.DockPosition = (DockPosition) 59424;
    data.InitialState.TabBehind = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
  }

  public FrameworkElement CreateFrameworkElement()
  {
    if (App.ParentPaneWindowRef == null || !App.ParentPaneWindowRef.LoadedTreeViews)
    {
      this.paneWindow = new PaneWindow();
      this.paneWindow.famLoadEvent = this.ProviderLoadFamilyEvent;
      this.paneWindow.famDownloadEvent = this.ProviderDownloadFamilyEvent;
      return (FrameworkElement) this.paneWindow;
    }
    this.paneWindow = new PaneWindow(false);
    this.paneWindow.famLoadEvent = this.ProviderLoadFamilyEvent;
    this.paneWindow.famDownloadEvent = this.ProviderDownloadFamilyEvent;
    return (FrameworkElement) this.paneWindow;
  }
}
