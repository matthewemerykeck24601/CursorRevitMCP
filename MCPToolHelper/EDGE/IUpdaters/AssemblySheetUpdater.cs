// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.AssemblySheetUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.IUpdaters;

public class AssemblySheetUpdater : IUpdater
{
  private static AddInId _appId;
  private static UpdaterId _updaterId;
  private static bool prevSuspendStatus;

  public AssemblySheetUpdater(AddInId id)
  {
    AssemblySheetUpdater._appId = id;
    AssemblySheetUpdater._updaterId = new UpdaterId(AssemblySheetUpdater._appId, new Guid("FBFBF6B2-4C06-42d4-97C1-D1B4EB593EFF"));
  }

  public void Execute(UpdaterData data)
  {
    if (App.bSynchronizingWithCentral || App.bSyncingWithCentral)
      return;
    Document document = data.GetDocument();
    ActiveModel.GetInformation(document);
    foreach (ElementId id in data.GetAddedElementIds().ToList<ElementId>())
    {
      if (id != (ElementId) null && document.GetElement(id) is ViewSheet)
      {
        ViewSheet element1 = document.GetElement(id) as ViewSheet;
        Element element2 = document.GetElement(element1.AssociatedAssemblyInstanceId);
        App.UpdateSheetAndAssemblyIdDictionary(document);
        using (SubTransaction subTransaction = new SubTransaction(document))
        {
          try
          {
            AssemblySheetUpdater.prevSuspendStatus = App.DialogSwitches.SuspendModelLockingforOperation;
            App.DialogSwitches.SuspendModelLockingforOperation = true;
            if (element2 != null)
            {
              int num1 = (int) subTransaction.Start();
              Utils.AssemblyUtils.Parameters.UpdateStatusParameters(element2, "CREATED");
              App.GlobalValueForViewSheetIssue = new List<AssemblyInstance>();
              Utils.AssemblyUtils.Parameters.AddEditComment(element2, "CREATED", "N/A");
              string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element2, "ASSEMBLY_MARK_NUMBER");
              IEnumerable<IGrouping<string, Element>> list = (IEnumerable<IGrouping<string, Element>>) AssemblySheetUpdater.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetFilteredElements(document).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>()).ToList<IGrouping<string, Element>>();
              int quantity = AssemblySheetUpdater.GetQuantity(parameterAsString, list);
              element2.LookupParameter("TKT_TOTAL_CREATED")?.Set(quantity);
              element2.LookupParameter("TICKET_IS_CREATED")?.Set(1);
              foreach (IGrouping<string, Element> source in list)
              {
                if (source.Key.Equals(parameterAsString))
                {
                  foreach (Element element3 in source.ToList<Element>())
                    element3.LookupParameter("TICKET_IS_CREATED")?.Set(1);
                }
              }
              int num2 = (int) subTransaction.Commit();
            }
          }
          catch (Exception ex)
          {
            TaskDialog.Show("Error: ", ex.ToString());
          }
          finally
          {
            App.DialogSwitches.SuspendModelLockingforOperation = AssemblySheetUpdater.prevSuspendStatus;
          }
        }
      }
    }
    foreach (ElementId deletedElementId in (IEnumerable<ElementId>) data.GetDeletedElementIds())
    {
      if (deletedElementId != (ElementId) null && App.sheetAndAsssemblyIds.ContainsKey(deletedElementId.ToString()))
      {
        int result;
        int.TryParse(App.sheetAndAsssemblyIds[deletedElementId.ToString()], out result);
        ElementId id = new ElementId(result);
        Element element4 = document.GetElement(id);
        App.sheetAndAsssemblyIds.Remove(deletedElementId.ToString());
        using (SubTransaction subTransaction = new SubTransaction(document))
        {
          try
          {
            AssemblySheetUpdater.prevSuspendStatus = App.DialogSwitches.SuspendModelLockingforOperation;
            App.DialogSwitches.SuspendModelLockingforOperation = true;
            if (element4 != null)
            {
              int num3 = (int) subTransaction.Start();
              App.GlobalValueForViewSheetIssue = new List<AssemblyInstance>();
              string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element4, "ASSEMBLY_MARK_NUMBER");
              IEnumerable<IGrouping<string, Element>> list = (IEnumerable<IGrouping<string, Element>>) AssemblySheetUpdater.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetFilteredElements(document).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>()).ToList<IGrouping<string, Element>>();
              int quantity = AssemblySheetUpdater.GetQuantity(parameterAsString, list);
              element4.LookupParameter("TKT_TOTAL_CREATED")?.Set(quantity);
              if (!App.sheetAndAsssemblyIds.ContainsValue(id.ToString()))
              {
                element4.LookupParameter("TICKET_IS_CREATED")?.Set(0);
                foreach (IGrouping<string, Element> source in list)
                {
                  if (source.Key.Equals(parameterAsString))
                  {
                    foreach (Element element5 in source.ToList<Element>())
                      element5.LookupParameter("TICKET_IS_CREATED")?.Set(0);
                  }
                }
              }
              int num4 = (int) subTransaction.Commit();
            }
          }
          catch (Exception ex)
          {
            TaskDialog.Show("Error: ", ex.ToString());
          }
          finally
          {
            App.DialogSwitches.SuspendModelLockingforOperation = AssemblySheetUpdater.prevSuspendStatus;
          }
        }
      }
    }
    App.checkSheetAndAssemblyIds = false;
  }

  public string GetAdditionalInformation()
  {
    return "Updates TICKET_CREATED_USER/DATE_INITIAL/CURRENT Parameters for an Assembly upon ViewSheet creation or deletion";
  }

  public ChangePriority GetChangePriority() => ChangePriority.Views;

  public UpdaterId GetUpdaterId() => AssemblySheetUpdater._updaterId;

  public string GetUpdaterName() => "Assembly Parameter Updater";

  private static int GetQuantity(
    string assemblyMarkNumber,
    IEnumerable<IGrouping<string, Element>> structFramingGroups)
  {
    using (IEnumerator<IGrouping<string, Element>> enumerator = structFramingGroups.Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (group => group.Key.Equals(assemblyMarkNumber))).GetEnumerator())
    {
      if (enumerator.MoveNext())
        return enumerator.Current.ToList<Element>().Count;
    }
    return 0;
  }

  private static IEnumerable<IGrouping<string, Element>> GroupByControlMark(
    IEnumerable<Element> structFramingElems)
  {
    Document doc = ActiveModel.Document;
    return structFramingElems.Where<Element>((Func<Element, bool>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")) != null)).GroupBy<Element, string>((Func<Element, string>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")).AsString()));
  }
}
