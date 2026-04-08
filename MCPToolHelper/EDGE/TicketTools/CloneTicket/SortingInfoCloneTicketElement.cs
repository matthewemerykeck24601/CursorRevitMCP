// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.SortingInfoCloneTicketElement
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Utils.MiscUtils;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class SortingInfoCloneTicketElement
{
  public string sortingName;
  public FormLocation locationInForm;

  public SortingInfoCloneTicketElement(string controlMark, FormLocation location)
  {
    this.sortingName = controlMark;
    this.locationInForm = location;
  }

  public override bool Equals(object obj)
  {
    SortingInfoCloneTicketElement object2 = obj as SortingInfoCloneTicketElement;
    return (object) object2 != null && this.Equals(object2);
  }

  public bool Equals(SortingInfoCloneTicketElement object2)
  {
    return (object) object2 != null && this.sortingName == object2.sortingName && this.locationInForm == object2.locationInForm;
  }

  public static bool operator ==(
    SortingInfoCloneTicketElement object1,
    SortingInfoCloneTicketElement object2)
  {
    if ((object) object1 == null && (object) object2 == null)
      return true;
    return (object) object1 != null && (object) object2 != null && object1.Equals(object2);
  }

  public static bool operator !=(
    SortingInfoCloneTicketElement object1,
    SortingInfoCloneTicketElement object2)
  {
    return !(object1 == object2);
  }
}
