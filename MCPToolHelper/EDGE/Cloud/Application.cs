// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Application
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

#nullable disable
namespace EDGE.Cloud;

public class Application : IExternalApplication
{
  private string _ribbonPanelName = "EDGE^Cloud";

  public Result OnStartup(UIControlledApplication application) => (Result) 0;

  public Result OnShutdown(UIControlledApplication application) => (Result) 0;

  private void AddRibbonItems(UIControlledApplication application)
  {
    try
    {
      if (!(application.CreateRibbonPanel(this._ribbonPanelName).AddItem((RibbonItemData) new PushButtonData("EDGE_Command", "EDGE", Assembly.GetExecutingAssembly().Location, "EdgeCloud.Revit.ExportCommand")
      {
        AvailabilityClassName = "EdgeCloud.Revit.AvailabilityNoOpenDocument"
      }) is PushButton pushButton))
        return;
      ((RibbonItem) pushButton).ItemText = "Export Model";
      ((RibbonItem) pushButton).ToolTip = "Export to EDGE Cloud";
    }
    catch (Exception ex)
    {
    }
  }

  private BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
  {
    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
  }
}
