// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ReleaseTicketEvent
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

public class ReleaseTicketEvent : IExternalEventHandler
{
  public void Execute(UIApplication app)
  {
    IEnumerable<AssemblyViewModel> list1 = (IEnumerable<AssemblyViewModel>) App.TicketManagerWindow.DataGrid.SelectedItems.Cast<AssemblyViewModel>().ToList<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList1 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList2 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList3 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList4 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList5 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList6 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList7 = new List<AssemblyViewModel>();
    List<AssemblyViewModel> assemblyViewModelList8 = new List<AssemblyViewModel>();
    Document document = app.ActiveUIDocument.Document;
    IEnumerable<AssemblyViewModel> assemblyViewModels1 = (IEnumerable<AssemblyViewModel>) new List<AssemblyViewModel>();
    IEnumerable<AssemblyViewModel> assemblyViewModels2;
    if (document.IsWorkshared)
    {
      ICollection<ElementId> elementIds1 = (ICollection<ElementId>) new List<ElementId>();
      ICollection<ElementId> elementIds2 = (ICollection<ElementId>) new List<ElementId>();
      ICollection<ElementId> elementIds3 = (ICollection<ElementId>) new List<ElementId>();
      Dictionary<ElementId, AssemblyViewModel> dictionary = new Dictionary<ElementId, AssemblyViewModel>();
      foreach (AssemblyViewModel assemblyViewModel in list1)
        dictionary.Add(assemblyViewModel.Assembly.Id, assemblyViewModel);
      ICollection<ElementId> keys = (ICollection<ElementId>) dictionary.Keys;
      ICollection<ElementId> elementIds4 = WorksharingUtils.CheckoutElements(document, keys);
      foreach (ElementId elementId in (IEnumerable<ElementId>) keys)
      {
        if (!elementIds4.Contains(elementId))
        {
          elementIds1.Add(elementId);
        }
        else
        {
          switch (WorksharingUtils.GetModelUpdatesStatus(document, elementId))
          {
            case ModelUpdatesStatus.DeletedInCentral:
            case ModelUpdatesStatus.UpdatedInCentral:
              elementIds2.Add(elementId);
              continue;
            default:
              if (WorksharingUtils.GetCheckoutStatus(document, elementId) != CheckoutStatus.OwnedByCurrentUser)
              {
                elementIds1.Add(elementId);
                continue;
              }
              continue;
          }
        }
      }
      int num = elementIds1.Count + elementIds2.Count;
      if (num > 0)
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("The following ");
        stringBuilder.Append(num > 1 ? "assemblies" : "assembly");
        stringBuilder.Append(" cannot be processed. ");
        if (elementIds1.Count != 0)
        {
          stringBuilder.Append(elementIds1.Count > 1 ? "The assemblies with Mark Numbers: " : "The assembly with Mark Number: ");
          foreach (ElementId key in (IEnumerable<ElementId>) elementIds1)
            stringBuilder.Append(dictionary[key].MarkNumber + ", ");
          stringBuilder.Remove(stringBuilder.Length - 2, 2);
          stringBuilder.Append(elementIds1.Count > 1 ? " are " : " is ");
          stringBuilder.Append("owned by others. Please coordinate with project members to allow for ownership of the");
          stringBuilder.Append(elementIds1.Count > 1 ? " elements." : " element.");
          foreach (ElementId key in (IEnumerable<ElementId>) elementIds1)
            dictionary.Remove(key);
        }
        if (elementIds2.Count != 0)
        {
          stringBuilder.Append(elementIds2.Count > 1 ? "The assemblies with Mark Numbers: " : "The assembly with Mark Number: ");
          foreach (ElementId key in (IEnumerable<ElementId>) elementIds2)
            stringBuilder.Append(dictionary[key].MarkNumber + ", ");
          stringBuilder.Remove(stringBuilder.Length - 2, 2);
          stringBuilder.Append(elementIds2.Count > 1 ? " are " : " is ");
          stringBuilder.Append("out of date. Please reload the latest version from central model.");
          foreach (ElementId key in (IEnumerable<ElementId>) elementIds2)
            dictionary.Remove(key);
        }
        new TaskDialog("Warning")
        {
          MainContent = stringBuilder.ToString()
        }.Show();
      }
      assemblyViewModels2 = (IEnumerable<AssemblyViewModel>) dictionary.Values;
    }
    else
      assemblyViewModels2 = list1;
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (AssemblyViewModel assemblyViewModel in assemblyViewModels2)
    {
      if (assemblyViewModel.Assemblied == "No")
      {
        assemblyViewModelList1.Add(assemblyViewModel);
      }
      else
      {
        bool flag1 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) assemblyViewModel.Assembly, "TICKET_FLAGGED") == 1;
        bool flag2 = true;
        bool flag3 = true;
        bool flag4 = true;
        bool flag5 = true;
        bool flag6 = false;
        bool flag7 = assemblyViewModel.OnHold == "Yes";
        if (assemblyViewModel.Quantity <= 0)
          flag2 = false;
        if (string.IsNullOrEmpty(Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyViewModel.Assembly, "TICKET_REINFORCED_DATE_INITIAL")))
          flag3 = false;
        if (string.IsNullOrEmpty(Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyViewModel.Assembly, "TICKET_CREATED_DATE_INITIAL")))
          flag4 = false;
        if (string.IsNullOrEmpty(Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyViewModel.Assembly, "TICKET_DETAILED_DATE_INITIAL")))
          flag5 = false;
        string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyViewModel.Assembly, "TKT_TOTAL_RELEASED");
        int parameterAsInt1 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) assemblyViewModel.Assembly, "TKT_TOTAL_CREATED");
        int parameterAsInt2 = Utils.ElementUtils.Parameters.GetParameterAsInt((Element) assemblyViewModel.Assembly, "TKT_TOTAL_DETAILED");
        bool flag8 = !string.IsNullOrWhiteSpace(parameterAsString) && !assemblyViewModel.Quantity.ToString().Equals(parameterAsString) || parameterAsInt1 != -1 && !assemblyViewModel.Quantity.Equals(parameterAsInt1) || parameterAsInt2 != -1 && !assemblyViewModel.Quantity.Equals(parameterAsInt2);
        if (flag1 | flag8)
          assemblyViewModelList3.Add(assemblyViewModel);
        else if (!flag6 && !flag2)
          assemblyViewModelList4.Add(assemblyViewModel);
        else if (!flag6 && !flag3)
          assemblyViewModelList5.Add(assemblyViewModel);
        else if (!flag6 && !flag4)
          assemblyViewModelList6.Add(assemblyViewModel);
        else if (!flag6 && !flag5)
          assemblyViewModelList7.Add(assemblyViewModel);
        else if (flag7)
        {
          assemblyViewModelList8.Add(assemblyViewModel);
        }
        else
        {
          if (!flag1 & flag2 & flag3 & flag4 & flag5)
          {
            assemblyViewModel.ReleasedDate = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            assemblyViewModel.ReleasedBy = app.Application.Username.FormatUsername();
            assemblyViewModelList2.Add(assemblyViewModel);
          }
          elementIdList.Add(assemblyViewModel.Assembly.Id);
        }
      }
    }
    StringBuilder stringBuilder1 = new StringBuilder();
    int count1 = assemblyViewModelList1.Count;
    int count2 = assemblyViewModelList3.Count;
    int count3 = assemblyViewModelList4.Count;
    int count4 = assemblyViewModelList5.Count;
    int count5 = assemblyViewModelList6.Count;
    int count6 = assemblyViewModelList7.Count;
    int count7 = assemblyViewModelList8.Count;
    int num1 = count1 + count2 + count3 + count4 + count5 + count6 + count7;
    int count8 = assemblyViewModelList2.Count;
    if (count8 > 0)
    {
      string str = count8 > 1 ? "elements (shown with Mark Number) have" : "element (shown with Mark Number) has";
      stringBuilder1.Append($"Congratulations! The following {str} been released: ");
      foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList2)
        stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
      stringBuilder1.AppendLine().AppendLine();
    }
    if (num1 > 0)
    {
      string str1 = num1 > 1 ? " elements (shown with Mark Number) were" : " element (shown with Mark Number) was";
      stringBuilder1.Append($"The following {num1.ToString()}{str1} not released: ").AppendLine();
      if (count1 > 0)
      {
        stringBuilder1.Append("Not an assembly: ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList1)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      if (count2 > 0)
      {
        stringBuilder1.Append("Warning Flags which must be reviewed before releasing. Please review the Edit History in the Ticket Manager and Accept Edits before attempting to release this Ticket: ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList3)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      if (count3 > 0)
      {
        stringBuilder1.Append("Assembly quantity issue not being resolved: ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList4)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      if (count4 > 0)
      {
        stringBuilder1.Append("Not being marked as Reinforced: ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList5)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      if (count5 > 0)
      {
        stringBuilder1.Append("Not being marked as having work begun (Created): ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList6)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      if (count6 > 0)
      {
        stringBuilder1.Append("Not being marked as complete (Detailed): ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList7)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      if (count7 > 0)
      {
        stringBuilder1.Append("Placed on hold: ");
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList8)
          stringBuilder1.Append(assemblyViewModel.MarkNumber + " ");
        stringBuilder1.AppendLine();
      }
      string str2 = num1 > 1 ? "these issues" : "this issue";
      stringBuilder1.AppendLine().Append($"Please address {str2} before attempting to release Ticket.");
    }
    using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "Release Ticket - "))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        int num2 = (int) transaction.Start();
        foreach (AssemblyViewModel assemblyViewModel in assemblyViewModelList2)
        {
          Utils.AssemblyUtils.Parameters.UpdateStatusParameters((Element) assemblyViewModel.Assembly, "RELEASED");
          Utils.AssemblyUtils.Parameters.AddEditComment((Element) assemblyViewModel.Assembly, "RELEASED", "TICKET RELEASED");
          assemblyViewModel.Assembly.LookupParameter("TKT_TOTAL_RELEASED")?.Set(assemblyViewModel.Quantity.ToString());
          string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assemblyViewModel.Assembly, "ASSEMBLY_MARK_NUMBER");
          List<IGrouping<string, Element>> list2 = UpdtTktDetailedInformation.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetFilteredElements(document).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).SuperComponent == null)).ToList<Element>()).ToList<IGrouping<string, Element>>();
          assemblyViewModel.Assembly.LookupParameter("TICKET_IS_RELEASED")?.Set(1);
          foreach (IGrouping<string, Element> source in (IEnumerable<IGrouping<string, Element>>) list2)
          {
            if (source.Key.Equals(parameterAsString))
            {
              foreach (Element element in source.ToList<Element>())
                element.LookupParameter("TICKET_IS_RELEASED")?.Set(1);
            }
          }
        }
        App.releasedAssemblies = assemblyViewModelList2;
        ActiveModel.GetInformation(app.ActiveUIDocument);
        ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
        if (transaction.Commit() == TransactionStatus.Committed)
        {
          if (app.ActiveUIDocument.Document.IsWorkshared)
          {
            ICollection<ElementId> elementsToCheckout = (ICollection<ElementId>) new List<ElementId>();
            foreach (AssemblyViewModel assemblyViewModel in assemblyViewModels2)
              elementsToCheckout.Add(assemblyViewModel.Assembly.Id);
            WorksharingUtils.CheckoutElements(app.ActiveUIDocument.Document, elementsToCheckout);
          }
        }
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Exception", ex.ToString());
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    app.ActiveUIDocument.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
    if (string.IsNullOrEmpty(stringBuilder1.ToString()))
      return;
    int num3 = (int) MessageBox.Show(stringBuilder1.ToString(), "Release Ticket Message");
  }

  private static IEnumerable<IGrouping<string, Element>> GroupByControlMark(
    IEnumerable<Element> structFramingElems)
  {
    Document doc = ActiveModel.Document;
    return structFramingElems.Where<Element>((Func<Element, bool>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")) != null)).Where<Element>((Func<Element, bool>) (s => !string.IsNullOrWhiteSpace(Utils.ElementUtils.Parameters.GetParameterAsString(s, "CONTROL_MARK")))).GroupBy<Element, string>((Func<Element, string>) (elem => (elem.LookupParameter("CONTROL_MARK") ?? doc.GetElement(elem.GetTypeId()).LookupParameter("CONTROL_MARK")).AsString()));
  }

  public int GetQuantity(string markNumber)
  {
    return ReleaseTicketEvent.GroupByControlMark((IEnumerable<Element>) StructuralFraming.GetAllElements()).Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (group => group.Key.Equals(markNumber))).Select<IGrouping<string, Element>, int>((Func<IGrouping<string, Element>, int>) (group => group.ToList<Element>().Count)).FirstOrDefault<int>();
  }

  public string GetName() => "Accept Changes Event";
}
