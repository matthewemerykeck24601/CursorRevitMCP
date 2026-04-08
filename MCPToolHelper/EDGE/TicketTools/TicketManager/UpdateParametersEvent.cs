// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.UpdateParametersEvent
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
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

public class UpdateParametersEvent : IExternalEventHandler
{
  private void updateParameter(
    Document revitDoc,
    string paramName,
    string paramValue,
    Element elem,
    AssemblyViewModel assemblyViewModel,
    out List<string> typeParametersWarning)
  {
    if (paramValue == "Parameter values are not the same across mark numbers.")
    {
      typeParametersWarning = new List<string>();
    }
    else
    {
      List<string> stringList = new List<string>();
      AssemblyInstance assemblyInstance = elem as AssemblyInstance;
      if (assemblyInstance != null)
      {
        Parameter parameter = assemblyInstance.LookupParameter(paramName);
        if (parameter != null && !parameter.IsReadOnly)
        {
          if (parameter.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            if (paramValue.Equals("Yes") || paramValue.Equals("1"))
              paramValue = "1";
            else if (paramValue.Equals("No") || paramValue.Equals("0"))
              paramValue = "0";
            int num1;
            switch (paramValue)
            {
              case "1":
                num1 = 1;
                break;
              case "0":
                num1 = 0;
                break;
              default:
                num1 = -1;
                break;
            }
            int num2 = num1;
            if (num2 != -1 && (!parameter.HasValue || parameter.AsInteger() != num2))
            {
              if (paramName == "ON_HOLD")
                App.DialogSwitches.SuspendModelLockingforOperation = true;
              parameter.Set(num2);
            }
          }
          else if (parameter.AsString() != paramValue && (!string.IsNullOrWhiteSpace(paramValue) || parameter.HasValue))
          {
            if (paramName == "IDENTITY_COMMENT")
              App.DialogSwitches.SuspendModelLockingforOperation = true;
            parameter.Set(paramValue);
          }
        }
      }
      foreach (Element elem1 in assemblyViewModel.SFElementsOfThisControlMark)
      {
        Element topLevelElement = elem1.GetTopLevelElement();
        Parameter parameter = topLevelElement.GetTopLevelElement().LookupParameter(paramName);
        Element elem2 = (Element) null;
        topLevelElement.LookupParameter("IDENTITY_COMMENT");
        string str = "";
        if (parameter == null || parameter.IsReadOnly)
        {
          ElementId typeId = topLevelElement.GetTypeId();
          if (typeId != ElementId.InvalidElementId)
          {
            elem2 = revitDoc.GetElement(typeId);
            str = (elem2 as FamilySymbol).Family.Name;
            parameter = elem2.GetTopLevelElement().LookupParameter(paramName);
          }
        }
        if (parameter != null && !parameter.IsReadOnly)
        {
          if (parameter.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            if (paramValue.Equals("Yes") || paramValue.Equals("1"))
              paramValue = "1";
            else if (paramValue.Equals("No") || paramValue.Equals("0"))
              paramValue = "0";
            int num3;
            switch (paramValue)
            {
              case "1":
                num3 = 1;
                break;
              case "0":
                num3 = 0;
                break;
              default:
                num3 = -1;
                break;
            }
            int num4 = num3;
            if (num4 != -1 && (!parameter.HasValue || parameter.AsInteger() != num4))
            {
              if (elem2 != null)
              {
                if (!stringList.Contains($"{str} {elem2.Name} - {paramName}"))
                  stringList.Add($"{str} {elem2.Name} - {paramName}");
              }
              if (paramName == "ON_HOLD")
                App.DialogSwitches.SuspendModelLockingforOperation = true;
              parameter.Set(num4);
            }
          }
          else if (parameter.AsString() != paramValue && (!string.IsNullOrWhiteSpace(paramValue) || parameter.HasValue))
          {
            if (elem2 != null)
            {
              if (!stringList.Contains($"{str} {elem2.Name} - {paramName}"))
                stringList.Add($"{str} {elem2.Name} - {paramName}");
            }
            if (paramName == "IDENTITY_COMMENT")
              App.DialogSwitches.SuspendModelLockingforOperation = true;
            parameter.Set(paramValue);
          }
        }
      }
      typeParametersWarning = stringList;
    }
  }

  public void Execute(UIApplication app)
  {
    List<ElementId> elementIdList1 = new List<ElementId>();
    AssemblyViewModel editingItem = ((MainViewModel) App.TicketManagerWindow.DataContext).EditingItem;
    Element assembly = (Element) editingItem.Assembly;
    List<ElementId> elementIdList2;
    if (assembly != null)
      elementIdList2 = new List<ElementId>() { assembly.Id };
    else
      elementIdList2 = editingItem.StructuralFramingElemIds.ToList<ElementId>();
    List<string> stringList = new List<string>();
    using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "Update Parameters (Ticket Manager)"))
    {
      List<string> typeParametersWarning1 = new List<string>();
      List<string> typeParametersWarning2 = new List<string>();
      List<string> typeParametersWarning3 = new List<string>();
      List<string> typeParametersWarning4 = new List<string>();
      List<string> typeParametersWarning5 = new List<string>();
      List<string> typeParametersWarning6 = new List<string>();
      try
      {
        int num1 = (int) transaction.Start();
        if (editingItem.OnHold != null && editingItem.OnHold != "")
          this.updateParameter(app.ActiveUIDocument.Document, "ON_HOLD", editingItem.OnHold, assembly, editingItem, out typeParametersWarning1);
        if (editingItem.IdentityComment != null)
          this.updateParameter(app.ActiveUIDocument.Document, "IDENTITY_COMMENT", editingItem.IdentityComment, assembly, editingItem, out typeParametersWarning2);
        if (editingItem.Custom1 != null)
          this.updateParameter(app.ActiveUIDocument.Document, editingItem.CustomColumnName1, editingItem.Custom1, assembly, editingItem, out typeParametersWarning3);
        if (editingItem.Custom2 != null)
          this.updateParameter(app.ActiveUIDocument.Document, editingItem.CustomColumnName2, editingItem.Custom2, assembly, editingItem, out typeParametersWarning4);
        if (editingItem.Custom3 != null)
          this.updateParameter(app.ActiveUIDocument.Document, editingItem.CustomColumnName3, editingItem.Custom3, assembly, editingItem, out typeParametersWarning5);
        if (editingItem.Custom4 != null)
          this.updateParameter(app.ActiveUIDocument.Document, editingItem.CustomColumnName4, editingItem.Custom4, assembly, editingItem, out typeParametersWarning6);
        stringList.AddRange((IEnumerable<string>) typeParametersWarning1);
        stringList.AddRange((IEnumerable<string>) typeParametersWarning2);
        stringList.AddRange((IEnumerable<string>) typeParametersWarning3);
        stringList.AddRange((IEnumerable<string>) typeParametersWarning4);
        stringList.AddRange((IEnumerable<string>) typeParametersWarning5);
        stringList.AddRange((IEnumerable<string>) typeParametersWarning6);
        if (stringList.Count > 0)
        {
          TaskDialog taskDialog = new TaskDialog("Type Parameters Modified");
          taskDialog.MainContent = "The following type parameters will be modified. Are you sure you wish to modify these Type Parameters and continue with editing?";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue Edit");
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel Edit");
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string str in stringList)
            stringBuilder.AppendLine(str);
          taskDialog.ExpandedContent = stringBuilder.ToString();
          if (taskDialog.Show().Equals((object) (TaskDialogResult) 1001))
          {
            int num2 = (int) transaction.Commit();
            ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
          }
          else
          {
            int num3 = (int) transaction.RollBack();
          }
        }
        else
        {
          int num4 = (int) transaction.Commit();
          ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
    App.TicketManagerWindow.DataGrid.IsEnabled = true;
    app.ActiveUIDocument.Selection.SetElementIds((ICollection<ElementId>) elementIdList2);
  }

  public string GetName() => "Update Parameters Event";
}
