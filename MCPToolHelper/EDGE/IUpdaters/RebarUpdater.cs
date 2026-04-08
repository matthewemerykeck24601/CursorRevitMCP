// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.RebarUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.IUpdaters.ModelLocking;
using EDGE.RebarTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.IUpdaters;

internal class RebarUpdater : IUpdater
{
  private readonly UpdaterId _updaterId;

  public RebarUpdater(AddInId id)
  {
    this._updaterId = new UpdaterId(id, new Guid("ba1ecfd8-6a1e-43c8-9f33-a20e0e9b5366"));
  }

  private bool ValidateRebar(Element elem)
  {
    switch ((BuiltInCategory) elem.Category.Id.IntegerValue)
    {
      case BuiltInCategory.OST_SpecialityEquipment:
      case BuiltInCategory.OST_GenericModel:
        string parameterAsString1 = Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT");
        string parameterAsString2 = Parameters.GetParameterAsString(elem, "BAR_SHAPE");
        double parameterAsDouble = Parameters.GetParameterAsDouble(elem, "BAR_DIAMETER");
        return parameterAsString1.Contains("REBAR") && !string.IsNullOrEmpty(parameterAsString2) && parameterAsDouble != -1.0 && parameterAsDouble != 0.0;
      default:
        return false;
    }
  }

  public void Execute(UpdaterData data)
  {
    try
    {
      QA.Trace("--------------------------------------------------------------------------");
      QA.Trace("-------------------   In Rebar Updater  -------------------");
      QA.Trace("--------------------------------------------------------------------------");
      QA.Trace("    ***   REBAR UPDATER CALLED   ***");
      Document revitDoc = data.GetDocument();
      if (App.bSynchronizingWithCentral || !App.RebarMarkAutomation || !App.RebarManager.IsAutomatedDocument(revitDoc.PathName))
        return;
      ActiveModel.GetInformation(revitDoc);
      if (ModelLockingUtils.ModelLockingEnabled(revitDoc) && !App.LockingManger.UserIsAllowedForCategory(revitDoc, ModelPermissionsCategory.RebarHandling))
        return;
      QA.ToggleRebarDebug();
      List<ElementId> elementIdList = data.GetModifiedElementIds().Where<ElementId>((Func<ElementId, bool>) (id => this.ValidateRebar(revitDoc.GetElement(id)))).ToList<ElementId>();
      if (revitDoc.IsWorkshared)
        elementIdList = RebarWorksharingUtilities.UserCanEditElements(revitDoc, elementIdList);
      RebarEditingUpdateUtility.RebarMarkUpdaterCore(elementIdList.Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)), revitDoc);
      QA.Trace("-------------------   *END* Rebar Updater  -------------------");
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public ChangePriority GetChangePriority() => ChangePriority.Structure;

  public string GetUpdaterName() => "Rebar Updater";

  public string GetAdditionalInformation()
  {
    return "Updates Rebar CONTROL_MARK Parameter Values when changes to the rebar instance occur in the project environment.";
  }
}
