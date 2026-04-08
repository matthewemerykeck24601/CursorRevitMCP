// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.UpdtTktDetailedInformation
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utils.CollectionUtils;
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class UpdtTktDetailedInformation : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document doc = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(doc, "Update Ticket Detailing Status"))
    {
      if (doc.ActiveView.IsAssemblyView && doc.ActiveView.ViewType.ToString().Equals("DrawingSheet"))
      {
        DataCollectorForm300x150 collectorForm300x150 = new DataCollectorForm300x150();
        collectorForm300x150.Text = "Username";
        collectorForm300x150.label.Text = "Enter your Initials below for insertion into the title block and to signify the ticket is complete and ready for Checking.";
        if (collectorForm300x150.ShowDialog() == DialogResult.Cancel)
          return (Result) 1;
        if (!string.IsNullOrWhiteSpace(collectorForm300x150.textBox1.Text))
        {
          string text = collectorForm300x150.textBox1.Text;
          try
          {
            App.DialogSwitches.SuspendModelLockingforOperation = true;
            int num = (int) transaction.Start();
            Element assembly = doc.GetElement(doc.ActiveView.AssociatedAssemblyInstanceId);
            Utils.AssemblyUtils.Parameters.UpdateStatusParameters(assembly, "DETAILED");
            Utils.AssemblyUtils.Parameters.AddEditComment(assembly, "DETAILED", "N/A");
            IEnumerable<ViewSheet> viewSheets = new FilteredElementCollector(doc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (sheet => sheet.IsAssemblyView && doc.GetElement(sheet.AssociatedAssemblyInstanceId).Name.Equals(assembly.Name)));
            foreach (Element element in viewSheets)
            {
              foreach (Parameter orderedParameter in (IEnumerable<Parameter>) element.GetOrderedParameters())
              {
                string name = orderedParameter.Definition.Name;
                if (name.Contains("Drawn By"))
                  orderedParameter.Set(text);
                if (name.Contains("Sheet Issue Date"))
                {
                  string str = DateTime.Today.ToString("MM/dd/yy");
                  orderedParameter.Set(str);
                }
              }
            }
            string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(assembly, "ASSEMBLY_MARK_NUMBER");
            IEnumerable<IGrouping<string, Element>> list = (IEnumerable<IGrouping<string, Element>>) UpdtTktDetailedInformation.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetFilteredElements(doc).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>()).ToList<IGrouping<string, Element>>();
            int quantity = UpdtTktDetailedInformation.GetQuantity(parameterAsString, list);
            assembly.LookupParameter("TKT_TOTAL_DETAILED")?.Set(quantity);
            assembly.LookupParameter("TICKET_IS_DETAILED")?.Set(1);
            foreach (IGrouping<string, Element> source in list)
            {
              if (source.Key.Equals(parameterAsString))
              {
                foreach (Element element in source.ToList<Element>())
                  element.LookupParameter("TICKET_IS_DETAILED")?.Set(1);
              }
            }
            if (transaction.Commit() != TransactionStatus.Committed)
              return (Result) -1;
            if (activeUiDocument.Document.IsWorkshared)
            {
              AssemblyInstance assemblyInstance = assembly as AssemblyInstance;
              ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
              elementsToCheckout.Add(assemblyInstance.Id);
              foreach (ViewSheet viewSheet in viewSheets)
                elementsToCheckout.Add(viewSheet.Id);
              WorksharingUtils.CheckoutElements(doc, elementsToCheckout);
            }
            return (Result) 0;
          }
          catch (Exception ex)
          {
            if (transaction.HasStarted())
            {
              int num = (int) transaction.RollBack();
            }
            message = ex.ToString();
            return (Result) -1;
          }
          finally
          {
            App.DialogSwitches.SuspendModelLockingforOperation = false;
          }
        }
        else
        {
          new TaskDialog("Error")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainContent = "You must provide user initials to continue.  Please run this tool again and provide the required user initials"
          }.Show();
          return (Result) 1;
        }
      }
      else
        new TaskDialog("Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainContent = "To mark a Ticket as being Detailed (Completed), you must be in a sheet view of the specific ticket."
        }.Show();
    }
    return (Result) 1;
  }

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

  public static IEnumerable<IGrouping<string, Element>> GroupByControlMark(
    IEnumerable<Element> structFramingElems)
  {
    Document doc = ActiveModel.Document;
    return structFramingElems.Where<Element>((Func<Element, bool>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")) != null)).GroupBy<Element, string>((Func<Element, string>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")).AsString()));
  }
}
