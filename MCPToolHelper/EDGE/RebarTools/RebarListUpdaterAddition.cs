// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.RebarListUpdaterAddition
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using EDGE.IUpdaters.ModelLocking;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.RebarTools;

internal class RebarListUpdaterAddition : IUpdater
{
  private readonly UpdaterId _updaterId;

  public RebarListUpdaterAddition(AddInId addinInId)
  {
    this._updaterId = new UpdaterId(addinInId, new Guid("46BB1135-1C17-46C1-83A6-DCAC18A04F01"));
  }

  public void Execute(UpdaterData data)
  {
    try
    {
      Document document = data.GetDocument();
      if (!App.RebarMarkAutomation || !App.RebarManager.IsAutomatedDocument(document.PathName))
        return;
      if (QA.LogVerboseRebarInfo)
        QA.LogDemarcation("in REBAR List ADDITION");
      QA.Trace(document.PathName);
      ActiveModel.GetInformation(document);
      if (App.bSynchronizingWithCentral || ModelLockingUtils.ModelLockingEnabled(document) && !App.LockingManger.UserIsAllowedForCategory(document, ModelPermissionsCategory.RebarHandling))
        return;
      IEnumerable<Element> elements = data.GetAddedElementIds().Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).Where<Element>((Func<Element, bool>) (elem => Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("REBAR"))).Where<Element>((Func<Element, bool>) (elem => !elem.HasSuperComponent()));
      foreach (Element element in elements.Where<Element>((Func<Element, bool>) (elem => Parameters.LookupParameter(elem, "BAR_SHAPE") != null)).ToList<Element>())
      {
        Parameter parameter1 = element.LookupParameter("CONTROL_MARK");
        Parameter parameter2 = Parameters.LookupParameter(element, "BAR_DIAMETER");
        if (parameter1 != null && parameter2 != null)
        {
          string currentMark = parameter1.AsString();
          RebarControlMarkManager3 controlMarkManager3 = App.RebarManager.Manager(document.PathName);
          controlMarkManager3.UpdateGuidToElementIdMap(element.UniqueId, element.Id, currentMark);
          controlMarkManager3.UpdateGuidToBarInfoMap(element);
        }
      }
      RebarEditingUpdateUtility.RebarMarkUpdaterCore(elements, document, true);
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public string GetUpdaterName() => "EDGE Rebar List Addition Updater";

  public ChangePriority GetChangePriority() => ChangePriority.Annotations;

  public string GetAdditionalInformation()
  {
    return "This updater manages control mark lists in response to added Rebar Elements.";
  }
}
