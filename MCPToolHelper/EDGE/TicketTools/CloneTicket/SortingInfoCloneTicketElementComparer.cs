// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.SortingInfoCloneTicketElementComparer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class SortingInfoCloneTicketElementComparer : IEqualityComparer<SortingInfoCloneTicketElement>
{
  public bool Equals(SortingInfoCloneTicketElement x, SortingInfoCloneTicketElement y)
  {
    return y.Equals(x);
  }

  public int GetHashCode(SortingInfoCloneTicketElement obj)
  {
    int num = 23;
    if (obj.sortingName != null)
      num ^= obj.sortingName.GetHashCode();
    return num ^ obj.locationInForm.GetHashCode();
  }
}
