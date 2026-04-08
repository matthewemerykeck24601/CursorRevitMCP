// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarListUpdaterDeletion
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.RebarTools;

public class RebarListUpdaterDeletion : IUpdater
{
  private readonly UpdaterId _updaterId;

  public RebarListUpdaterDeletion(AddInId addinInId)
  {
    this._updaterId = new UpdaterId(addinInId, new Guid("3E7A12F9-4D8A-4B27-A36B-180259499D76"));
  }

  public void Execute(UpdaterData data)
  {
    Document document = data.GetDocument();
    if (!App.RebarMarkAutomation || !App.RebarManager.IsAutomatedDocument(document.PathName))
      return;
    if (QA.LogVerboseRebarInfo)
    {
      QA.LogVerboseRebarTrace("--------------------------------------------------------------------------");
      QA.LogVerboseRebarTrace("----------------------  in REBAR List DELETION ---------------------------");
      QA.LogVerboseRebarTrace("--------------------------------------------------------------------------");
      QA.LogMinimalRebarTrace(document.PathName);
      QA.LogMinimalRebarTrace("        *********     Rebar Deletion Updater Called   ***********");
    }
    ActiveModel.GetInformation(document);
    if (ModelLockingUtils.ModelLockingEnabled(document) && !App.LockingManger.UserIsAllowedForCategory(document, ModelPermissionsCategory.RebarHandling))
      return;
    foreach (ElementId deletedElementId in (IEnumerable<ElementId>) data.GetDeletedElementIds())
    {
      QA.LogMinimalRebarTrace(" Deleting BarId: " + deletedElementId.ToString());
      if (document.IsWorkshared && !RebarWorksharingUtilities.UserCanEditElement(document, deletedElementId))
        QA.LogMinimalRebarTrace("=========  User cannot edit this element, continuing in RebarListUpdater_Deletion");
      else
        App.RebarManager.Manager(document.PathName).HandleBarDeletion(deletedElementId);
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public string GetUpdaterName() => "EDGE Rebar List Deletion Updater";

  public string GetAdditionalInformation()
  {
    return "This updater manages control mark lists in response to deleted Rebar Elements.";
  }

  public ChangePriority GetChangePriority() => ChangePriority.Annotations;
}
