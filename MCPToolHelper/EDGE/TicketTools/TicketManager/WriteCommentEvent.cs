// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.WriteCommentEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils.CollectionUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

public class WriteCommentEvent : IExternalEventHandler
{
  public void Execute(UIApplication app)
  {
    UIDocument activeUiDocument = app.ActiveUIDocument;
    List<AssemblyInstance> forQuantityIssue = App.GlobalValueForQuantityIssue;
    HashSet<AssemblyInstance> assemblyInstanceSet1 = new HashSet<AssemblyInstance>();
    List<AssemblyInstance> forViewSheetIssue = App.GlobalValueForViewSheetIssue;
    HashSet<AssemblyInstance> assemblyInstanceSet2 = new HashSet<AssemblyInstance>();
    List<AssemblyInstance> viewSheetCountIssue = App.GlobalValueForViewSheetCountIssue;
    HashSet<AssemblyInstance> assemblyInstanceSet3 = new HashSet<AssemblyInstance>();
    using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "setComment"))
    {
      int num1 = (int) transaction.Start();
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        foreach (AssemblyInstance assemblyInstance in viewSheetCountIssue)
          assemblyInstanceSet3.Add(assemblyInstance);
        foreach (AssemblyInstance assemblyInstance in forViewSheetIssue)
          assemblyInstanceSet2.Add(assemblyInstance);
        foreach (AssemblyInstance assemblyInstance in forQuantityIssue)
          assemblyInstanceSet1.Add(assemblyInstance);
        foreach (AssemblyInstance assemblyInstance in assemblyInstanceSet3)
        {
          Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter((Element) assemblyInstance, "TICKET_EDIT_COMMENT");
          string str = "";
          if (parameter != null)
            str = parameter.AsString() ?? "";
          if (str.Contains("FLAG ACCEPTED BY USER"))
          {
            int startIndex = str.LastIndexOf("USER");
            if (!str.Substring(startIndex).Contains("View Sheet Count has changed for this Mark Number"))
              Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) assemblyInstance, "VIEW SHEET COUNT CHANGED");
          }
          else if (!str.Contains("View Sheet Count has changed for this Mark Number"))
            Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) assemblyInstance, "VIEW SHEET COUNT CHANGED");
        }
        FilteredElementCollector elementCollector = new FilteredElementCollector(activeUiDocument.Document);
        FilteredElementCollector allElements = StructuralFraming.GetAllElements();
        ElementClassFilter filter = new ElementClassFilter(typeof (AssemblyInstance));
        foreach (AssemblyInstance assemblyInstance in assemblyInstanceSet2)
        {
          AssemblyInstance Assembly = assemblyInstance;
          Parameter parameter1 = Assembly.LookupParameter("TICKET_CREATED_DATE_INITIAL");
          Parameter parameter2 = Assembly.LookupParameter("TICKET_CREATED_DATE_CURRENT");
          parameter1.Set("");
          parameter2.Set("");
          foreach (Element element in elementCollector.WherePasses((ElementFilter) filter).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.GetAssemblyMarkNumber() == Assembly.GetAssemblyMarkNumber())))
            element.LookupParameter("TICKET_IS_CREATED")?.Set(0);
          foreach (Element element in allElements.Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "CONTROL_MARK") == Assembly.GetAssemblyMarkNumber())))
            element.LookupParameter("TICKET_IS_CREATED")?.Set(0);
          Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter((Element) Assembly, "TICKET_EDIT_COMMENT");
          string str = "";
          if (parameter3 != null)
            str = parameter3.AsString() ?? "";
          if (str.Contains("FLAG ACCEPTED BY USER"))
          {
            int startIndex = str.LastIndexOf("USER");
            if (!str.Substring(startIndex).Contains("User has deleted all assembly views"))
              Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) Assembly, "VIEW SHEET DELETED");
          }
          else if (!str.Contains("User has deleted all assembly views"))
            Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) Assembly, "VIEW SHEET DELETED");
        }
        foreach (AssemblyInstance assemblyInstance in assemblyInstanceSet1)
        {
          Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter((Element) assemblyInstance, "TICKET_EDIT_COMMENT");
          string str = "";
          if (parameter != null)
            str = parameter.AsString() ?? "";
          if (str.Contains("FLAG ACCEPTED BY USER"))
          {
            int startIndex = str.LastIndexOf("USER");
            if (!str.Substring(startIndex).Contains("Count Required has changed for this Mark Number"))
              Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) assemblyInstance, "QUANTITY CHANGED");
          }
          else if (!str.Contains("Count Required has changed for this Mark Number"))
            Utils.AssemblyUtils.Parameters.ShowEditCommentForm(app.Application, (Element) assemblyInstance, "QUANTITY CHANGED");
        }
        App.GlobalValueForQuantityIssue = new List<AssemblyInstance>();
        App.GlobalValueForViewSheetIssue = new List<AssemblyInstance>();
        App.GlobalValueForViewSheetCountIssue = new List<AssemblyInstance>();
      }
      catch (Exception ex)
      {
      }
      finally
      {
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          int num2 = (int) MessageBox.Show($"There are one or more assemblies owned by another user that you have made modifications to which has caused view sheet count issues or quantity issues; therefore, the assembly parameters cannot be updated.{Environment.NewLine.Repeat(2)}Please coordinate with project members to allow for ownership of the elements or try reloading latest from central.{Environment.NewLine.Repeat(2)}The Ticket Manager window will be closed.", "Ticket Manager Warning");
          App.DialogSwitches.SuspendModelLockingforOperation = false;
          App.TicketManagerWindow.Close();
        }
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }

  public string GetName() => "write comment event.";
}
