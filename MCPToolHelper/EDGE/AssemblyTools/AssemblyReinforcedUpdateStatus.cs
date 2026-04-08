// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.AssemblyReinforcedUpdateStatus
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.IUpdaters.ModelLocking;
using EDGE.TicketTools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.SelectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AssemblyTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class AssemblyReinforcedUpdateStatus : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    if (!ModelLockingUtils.ShowPermissionsDialog(document, ModelLockingToolPermissions.MarkAsReinforced))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "Update Assembly Reinforced Status"))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        int num = (int) transaction.Start();
        return AssemblyReinforcedUpdateStatus.Update(activeUiDocument, transaction);
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = $"There was a problem marking the Assembly/Ticket as being reinforced{Environment.NewLine}{ex.Message}";
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }

  public static Result Update(UIDocument document, Transaction transaction)
  {
    UIDocument uidoc = document;
    ActiveModel.GetInformation(uidoc);
    Document document1 = uidoc.Document;
    Application application = uidoc.Application.Application;
    string secondDate = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    string str1 = application.Username.FormatUsername();
    ICollection<ElementId> elementIds = uidoc.Selection.GetElementIds();
    if (elementIds.Count == 0)
    {
      ISelectionFilter selFilter = (ISelectionFilter) new OnlyAssemblies();
      elementIds = References.PickNewReferences(uidoc, selFilter, "Select existing Assemblies to Mark as reinforced.");
    }
    if (elementIds == null || elementIds.Count == 0)
      return (Result) 1;
    foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
    {
      if (!(document1.GetElement(id) is AssemblyInstance))
      {
        TaskDialog.Show("Error", "Selection contains elements which are not assemblies.  Please refine your selection prior to running Mark Assembly Reinforced.");
        return (Result) 1;
      }
    }
    string b1 = "";
    string b2 = "";
    string b3 = "";
    int num1 = 0;
    int num2 = 0;
    int num3 = 0;
    foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
    {
      Element element1 = document1.GetElement(id);
      Parameter parameter1 = element1.LookupParameter("TICKET_REINFORCED_DATE_INITIAL");
      Parameter parameter2 = element1.LookupParameter("TICKET_REINFORCED_USER_INITIAL");
      Parameter parameter3 = element1.LookupParameter("TICKET_REINFORCED_DATE_CURRENT");
      Parameter parameter4 = element1.LookupParameter("TICKET_REINFORCED_USER_CURRENT");
      Parameter parameter5 = element1.LookupParameter("TICKET_IS_REINFORCED");
      string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString(element1, "TICKET_CREATED_DATE_CURRENT");
      string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString(element1, "TICKET_DETAILED_DATE_CURRENT");
      string parameterAsString3 = Utils.ElementUtils.Parameters.GetParameterAsString(element1, "TICKET_RELEASED_DATE_CURRENT");
      Parameter parameter6 = element1.LookupParameter("TICKET_FLAGGED");
      parameter6.AsInteger();
      string str2 = parameter1.AsString();
      string parameterAsString4 = Utils.ElementUtils.Parameters.GetParameterAsString(element1, "ASSEMBLY_MARK_NUMBER");
      IEnumerable<IGrouping<string, Element>> list = (IEnumerable<IGrouping<string, Element>>) UpdtTktDetailedInformation.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetFilteredElements(document1).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>()).ToList<IGrouping<string, Element>>();
      if (string.IsNullOrWhiteSpace(str2))
      {
        if (string.IsNullOrWhiteSpace(b2))
        {
          b2 = element1.LookupParameter("ASSEMBLY_MARK_NUMBER").AsString();
          ++num2;
        }
        else
        {
          b2 = $"{b2}, {element1.LookupParameter("ASSEMBLY_MARK_NUMBER").AsString()}";
          ++num2;
        }
        parameter1.Set(secondDate);
        parameter2.Set(str1);
        parameter3.Set(secondDate);
        parameter4.Set(str1);
        Utils.AssemblyUtils.Parameters.AddEditComment(element1, "REINFORCED", "TICKET REINFORCED");
        parameter5?.Set(1);
        foreach (IGrouping<string, Element> source in list)
        {
          if (source.Key.Equals(parameterAsString4))
          {
            foreach (Element element2 in source.ToList<Element>())
              element2.LookupParameter("TICKET_IS_REINFORCED")?.Set(1);
          }
        }
      }
      else
      {
        if (string.IsNullOrWhiteSpace(b1))
        {
          b1 = element1.LookupParameter("ASSEMBLY_MARK_NUMBER").AsString();
          ++num1;
        }
        else
        {
          b1 = $"{b1}, {element1.LookupParameter("ASSEMBLY_MARK_NUMBER").AsString()}";
          ++num1;
        }
        parameter3.Set(secondDate);
        parameter4.Set(str1);
        Utils.AssemblyUtils.Parameters.AddEditComment(element1, "REINFORCED", "TICKET REINFORCING UPDATED");
        parameter5?.Set(1);
        foreach (IGrouping<string, Element> source in list)
        {
          if (source.Key.Equals(parameterAsString4))
          {
            foreach (Element element3 in source.ToList<Element>())
              element3.LookupParameter("TICKET_IS_REINFORCED")?.Set(1);
          }
        }
      }
      int num4 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString1, secondDate, "yyyy-MM-dd-HH-mm-ss");
      int num5 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString2, secondDate, "yyyy-MM-dd-HH-mm-ss");
      int num6 = Utils.MiscUtils.MiscUtils.CompareDateStrings(parameterAsString3, secondDate, "yyyy-MM-dd-HH-mm-ss");
      int num7 = num4 < 0 ? 1 : 0;
      bool flag1 = num5 < 0;
      bool flag2 = num6 < 0;
      int num8 = flag1 ? 1 : 0;
      if ((num7 | num8 | (flag2 ? 1 : 0)) != 0)
      {
        parameter6.Set(1);
        if (string.Equals("", b3))
        {
          b3 = element1.LookupParameter("ASSEMBLY_MARK_NUMBER").AsString();
          ++num3;
        }
        else
        {
          b3 = $"{b3}, {element1.LookupParameter("ASSEMBLY_MARK_NUMBER").AsString()}";
          ++num3;
        }
      }
    }
    TransactionStatus transactionStatus = transaction.Commit();
    string b4 = "";
    if (!string.Equals("", b1))
      b4 = $"{b4}There were {num1.ToString()} Assemblies which were previously marked as Reinforced in the selection set. {Environment.NewLine}{Environment.NewLine}{b1}";
    if (!string.Equals("", b3))
      b4 = $"{b4}{Environment.NewLine}{Environment.NewLine}There were {num3.ToString()} Assemblies which have already been Ticketed and therefore the Ticket will be Flagged in the Ticket Manager Dialog.{Environment.NewLine}{Environment.NewLine}{b3}";
    if (!string.Equals("", b2))
    {
      if (!string.Equals("", b4))
        b4 = b4 + Environment.NewLine + Environment.NewLine;
      b4 = $"{b4}There were {num2.ToString()} Assemblies that were marked as reinforced for the first time in the selection set. {Environment.NewLine}{Environment.NewLine}{b2}";
    }
    if (transactionStatus != TransactionStatus.Committed)
      return (Result) -1;
    new TaskDialog("Success")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainContent = b4
    }.Show();
    if (uidoc.Document.IsWorkshared)
    {
      ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
      foreach (ElementId elementId in (IEnumerable<ElementId>) elementIds)
        elementsToCheckout.Add(elementId);
      WorksharingUtils.CheckoutElements(document1, elementsToCheckout);
    }
    return (Result) 0;
  }
}
