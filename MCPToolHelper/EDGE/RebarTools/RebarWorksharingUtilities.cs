// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarWorksharingUtilities
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.RebarTools;

internal class RebarWorksharingUtilities
{
  public static List<ElementId> UserCanEditElements(
    Document revitDoc,
    List<ElementId> elementIdsToEdit)
  {
    List<ElementId> eids = (List<ElementId>) null;
    List<ElementId> list;
    try
    {
      list = WorksharingUtils.CheckoutElements(revitDoc, (ICollection<ElementId>) elementIdsToEdit).ToList<ElementId>();
    }
    catch (Exception ex)
    {
      QA.LogVerboseRebarSWCTrace($" --------  Model Update Status: *THREW EXCEPTION* -> {ex.GetType().ToString()}: {ex.Message}");
      return RebarWorksharingUtilities.BatchCheckModelUpdatesStatus(revitDoc, eids);
    }
    return RebarWorksharingUtilities.BatchCheckModelUpdatesStatus(revitDoc, list);
  }

  public static List<ElementId> UserCanEditElements(Document revitDoc, List<Element> elementsToEdit)
  {
    List<ElementId> list = elementsToEdit.Select<Element, ElementId>((Func<Element, ElementId>) (e => e.Id)).ToList<ElementId>();
    return RebarWorksharingUtilities.UserCanEditElements(revitDoc, list);
  }

  public static bool UserCanEditElement(Document revitDoc, ElementId elementIdToEdit)
  {
    QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: Getting Model Update Status for ElementId: " + elementIdToEdit.ToString());
    ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>()
    {
      elementIdToEdit
    };
    ICollection<ElementId> elementIds;
    try
    {
      if (revitDoc.GetElement(elementIdToEdit) == null)
      {
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: ELEMENT DOES NOT EXIST IN MODEL!  returning true?");
        return true;
      }
      elementIds = WorksharingUtils.CheckoutElements(revitDoc, elementsToCheckout);
    }
    catch (Exception ex)
    {
      QA.LogVerboseRebarSWCTrace($" --------  Model Update Status: *THREW EXCEPTION* -> {ex.GetType().ToString()}: {ex.Message}");
      return RebarWorksharingUtilities.CheckModelUpdatesStatusForEditability(revitDoc, elementIdToEdit);
    }
    if (elementIds != null && elementIds.Count > 0)
    {
      if (elementIds.Contains(elementIdToEdit))
      {
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: Successfully checked out element");
        return RebarWorksharingUtilities.CheckModelUpdatesStatusForEditability(revitDoc, elementIdToEdit);
      }
      QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: Checked out elements did not contain this element, cannot edit this element");
      return false;
    }
    QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: No Elements could be checked out.  Cannot edit this element");
    return false;
  }

  private static List<ElementId> BatchCheckModelUpdatesStatus(
    Document revitDoc,
    List<ElementId> eids)
  {
    if (eids == null || eids.Count == 0)
      return new List<ElementId>();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (ElementId eid in eids)
    {
      if (RebarWorksharingUtilities.CheckModelUpdatesStatusForEditability(revitDoc, eid))
        elementIdList.Add(eid);
    }
    return elementIdList;
  }

  private static bool CheckModelUpdatesStatusForEditability(
    Document revitDoc,
    ElementId elementIdToEdit)
  {
    if (elementIdToEdit == (ElementId) null || elementIdToEdit == ElementId.InvalidElementId)
      return false;
    switch (WorksharingUtils.GetModelUpdatesStatus(revitDoc, elementIdToEdit))
    {
      case ModelUpdatesStatus.CurrentWithCentral:
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: Current With Central: Can Edit");
        return true;
      case ModelUpdatesStatus.NotYetInCentral:
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: Not Yet In Central: Can Edit");
        return true;
      case ModelUpdatesStatus.DeletedInCentral:
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: DELETED in Central: CANNOT Edit");
        return false;
      case ModelUpdatesStatus.UpdatedInCentral:
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: UPDATED In Central: CANNOT Edit");
        return false;
      default:
        QA.LogVerboseRebarSWCTrace(" --------  Model Update Status: Did not return a value!  Cannot edit this element!");
        return false;
    }
  }
}
