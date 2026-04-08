// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.AvailabilityNoOpenDocument
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#nullable disable
namespace EDGE.Cloud;

public class AvailabilityNoOpenDocument : IExternalCommandAvailability
{
  public bool IsCommandAvailable(UIApplication application, CategorySet categorySet)
  {
    return application.ActiveUIDocument != null;
  }
}
