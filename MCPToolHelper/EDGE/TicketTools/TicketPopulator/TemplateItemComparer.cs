// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TemplateItemComparer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class TemplateItemComparer : IComparer
{
  public int Compare(object x, object y)
  {
    return (x as TemplateItem).CompareTo((object) (y as TemplateItem));
  }
}
