// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.ShopTicketCheckingTool.TicketCheckingUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.__Testing.ShopTicketCheckingTool;

public class TicketCheckingUtils
{
  public static bool IsPartsListElement(Element partElem)
  {
    string manufactureComponent = partElem.GetManufactureComponent();
    return string.IsNullOrWhiteSpace(partElem.GetConstructionProduct()) && !string.IsNullOrWhiteSpace(manufactureComponent) && !manufactureComponent.Contains("WWF") && !manufactureComponent.Contains("INSULATION") && !manufactureComponent.Contains("SHEARGRID") && !manufactureComponent.Contains("SPIRAL") && !string.IsNullOrWhiteSpace(partElem.GetControlMark());
  }
}
