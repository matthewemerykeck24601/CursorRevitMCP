// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.EDGEBrowser
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Utils.AdminUtils;

#nullable disable
namespace EDGE.EDGEBrowser;

[Transaction(TransactionMode.Manual)]
internal class EDGEBrowser : IExternalCommand
{
  public static string containerString = "edge-browser-container";

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    if (EDGE.EDGEBrowser.EDGEBrowser.CheckInternetConnection())
    {
      DockablePane dockablePane = App.appForPane.GetDockablePane(App.paneId);
      if (dockablePane.IsShown())
      {
        dockablePane.Hide();
        if (App.ParentPaneWindowRef != null)
          App.ParentPaneWindowRef.goHome();
      }
      else
      {
        dockablePane.Show();
        if (App.ParentPaneWindowRef != null)
          App.ParentPaneWindowRef.goHome();
      }
      return (Result) 0;
    }
    TaskDialog.Show("Unable to Connect", "Unable to connect to the Internet. Please check your internet connection and try again.");
    return (Result) 1;
  }

  public static bool CheckInternetConnection() => Util.CheckInternetConnection();
}
