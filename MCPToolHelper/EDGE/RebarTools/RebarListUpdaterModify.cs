// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarListUpdaterModify
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Linq;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.RebarTools;

public class RebarListUpdaterModify : IUpdater
{
  private readonly UpdaterId _updaterId;

  public RebarListUpdaterModify(AddInId addinInId)
  {
    this._updaterId = new UpdaterId(addinInId, new Guid("0681FA1A-FD5C-477D-9792-A1B6F94F89CB"));
  }

  public void Execute(UpdaterData data)
  {
    try
    {
      Document document = data.GetDocument();
      if (!App.RebarMarkAutomation || !App.RebarManager.IsAutomatedDocument(document.PathName))
        return;
      if (QA.LogVerboseRebarInfo)
      {
        QA.LogVerboseRebarTrace("--------------------------------------------------------------------------");
        QA.LogVerboseRebarTrace("----------------------  in REBAR List MODIFY ---------------------------");
        QA.LogVerboseRebarTrace("--------------------------------------------------------------------------");
        QA.LogMinimalRebarTrace("        *********     Rebar List Modify Updater Called   ***********");
      }
      ActiveModel.GetInformation(document);
      if (App.bSynchronizingWithCentral || ModelLockingUtils.ModelLockingEnabled(document) && !App.LockingManger.UserIsAllowedForCategory(document, ModelPermissionsCategory.RebarHandling))
        return;
      foreach (Element element in data.GetModifiedElementIds().Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).Where<Element>((Func<Element, bool>) (elem => Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("REBAR"))).Where<Element>((Func<Element, bool>) (elem => Parameters.LookupParameter(elem, "BAR_SHAPE") != null)).ToList<Element>())
      {
        if (document.IsWorkshared)
        {
          switch (WorksharingUtils.GetModelUpdatesStatus(document, element.Id))
          {
            case ModelUpdatesStatus.DeletedInCentral:
            case ModelUpdatesStatus.UpdatedInCentral:
              QA.LogVerboseRebarSWCTrace("=========  User cannot edit this element, continuing in RebarListUpdaterModify");
              continue;
          }
        }
        if (Parameters.GetParameterAsString(element, "BAR_SHAPE").Contains("STRAIGHT"))
          App.RebarManager.Manager(document.PathName).HandleBarDeletion(element.Id);
        Parameter parameter = element.LookupParameter("CONTROL_MARK");
        double barDiameter = Parameters.GetParameterAsZeroBasedDouble(element, "BAR_DIAMETER");
        if (BarHasher.useMetricEuro)
          barDiameter = UnitUtils.ConvertFromInternalUnits(barDiameter, UnitTypeId.Millimeters);
        if (parameter != null)
        {
          string currentMark = parameter.AsString();
          RebarControlMarkManager3 controlMarkManager3 = App.RebarManager.Manager(document.PathName);
          controlMarkManager3.UpdateGuidToElementIdMap(element.UniqueId, element.Id, currentMark);
          if (BarDiameterOracle.IsValidDiameter(barDiameter))
            controlMarkManager3.UpdateGuidToBarInfoMap(element);
          else
            controlMarkManager3.HandleBarDeletion(element.Id);
        }
      }
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public ChangePriority GetChangePriority() => ChangePriority.Annotations;

  public string GetUpdaterName() => "EDGE Rebar List Modification Updater";

  public string GetAdditionalInformation()
  {
    return "This updater manages control mark lists in response to modifications of Rebar Elements.";
  }
}
