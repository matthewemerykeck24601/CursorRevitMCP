// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketBomPartsList
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.HostingUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TicketBomPartsList : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    string scheduleName = "";
    string name = "PROJECT_CLIENT_PRECAST_MANUFACTURER";
    ICollection<ElementId> elementIds1 = (ICollection<ElementId>) new List<ElementId>();
    ICollection<ElementId> elementIds2 = (ICollection<ElementId>) new List<ElementId>();
    if (!document.ActiveView.IsAssemblyView)
    {
      TaskDialog.Show("Error", "Error: Run this command in an Assembly Sheet View");
      return (Result) 1;
    }
    List<ViewSchedule> list1 = new FilteredElementCollector(document).OfClass(typeof (ViewSchedule)).Cast<ViewSchedule>().ToList<ViewSchedule>();
    List<ViewSchedule> list2 = list1.Where<ViewSchedule>((Func<ViewSchedule, bool>) (schedule => schedule.Name.Equals("z_parts_list_template") || schedule.Name.Equals("z_wwf_schedule_template") || schedule.Name.Equals("z_insulation_schedule_template") || schedule.Name.Equals("z_mdk_insulation_schedule_template") || schedule.Name.Equals("z_cgrid_schedule_template") || schedule.Name.Equals("z_spiral_wire_schedule_template"))).ToList<ViewSchedule>();
    using (Transaction transaction = new Transaction(document, "Create Schedules"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        string str = document.ProjectInformation.LookupParameter(name) != null ? document.ProjectInformation.LookupParameter(name).AsString().ToUpper() : "default";
        List<ElementId> selectedIdList = new List<ElementId>();
        selectedIdList.Add((document.ActiveView as ViewSheet).AssociatedAssemblyInstanceId);
        ConstructionProductHosting.SelectedElements((ICollection<ElementId>) selectedIdList);
        for (int index1 = 0; index1 < 6; ++index1)
        {
          switch (str)
          {
            case "default":
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
            case "METROMONT":
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
            case "TINDALL":
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
            case "CORESLAB":
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
            case "UNISTRESS":
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
            case "WELLS":
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
            default:
              switch (index1)
              {
                case 0:
                  scheduleName = "PARTS_LIST";
                  break;
                case 1:
                  scheduleName = "WWF_SCHEDULE";
                  break;
                case 2:
                  scheduleName = "INSULATION_SCHEDULE";
                  break;
                case 3:
                  scheduleName = "MDK_INSULATION_SCHEDULE";
                  break;
                case 4:
                  scheduleName = "CGRID_SCHEDULE";
                  break;
                case 5:
                  scheduleName = "SPIRAL_WIRE_SCHEDULE";
                  break;
              }
              break;
          }
          if (list2.FirstOrDefault<ViewSchedule>((Func<ViewSchedule, bool>) (schedule => schedule.Name.Contains(scheduleName.ToLower()))) != null)
          {
            ViewSchedule newSchedule = AssemblyViewUtils.CreatePartList(document, document.ActiveView.AssociatedAssemblyInstanceId);
            newSchedule.Name = scheduleName;
            ViewSchedule otherView = list1.FirstOrDefault<ViewSchedule>((Func<ViewSchedule, bool>) (schedule => schedule.Name.Equals($"z_{scheduleName.ToLower()}_template"))) ?? newSchedule;
            foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) newSchedule.Definition.GetFieldOrder())
              newSchedule.Definition.RemoveField(fieldId);
            ICollection<ScheduleFieldId> fieldOrder = (ICollection<ScheduleFieldId>) otherView.Definition.GetFieldOrder();
            foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) fieldOrder)
            {
              ScheduleField field = otherView.Definition.GetField(fieldId);
              ScheduleFieldType fieldType = field.FieldType;
              if (!fieldType.Equals((object) ScheduleFieldType.Formula))
                newSchedule.Definition.AddField(fieldType, field.ParameterId);
            }
            foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) newSchedule.Definition.GetFieldOrder())
            {
              ScheduleField field = newSchedule.Definition.GetField(fieldId);
              field.HorizontalAlignment = !field.GetName().Contains("IDENTITY_DESCRIPTION") ? ScheduleHorizontalAlignment.Center : ScheduleHorizontalAlignment.Left;
            }
            int index2 = 0;
            int index3 = 0;
            while (index2 < fieldOrder.Count)
            {
              ScheduleField field = otherView.Definition.GetField(index2);
              bool isHidden = field.IsHidden;
              if (field.FieldType.Equals((object) ScheduleFieldType.Formula))
                --index3;
              else
                newSchedule.Definition.GetField(index3).IsHidden = isHidden;
              ++index2;
              ++index3;
            }
            IList<ScheduleSortGroupField> sortGroupFields = otherView.Definition.GetSortGroupFields();
            for (int index4 = 0; index4 < sortGroupFields.Count; ++index4)
            {
              ScheduleSortGroupField sortGroupField1 = otherView.Definition.GetSortGroupField(index4);
              ScheduleSortOrder sortOrder = sortGroupField1.SortOrder;
              ScheduleFieldId fieldId = sortGroupField1.FieldId;
              ElementId parameterId = otherView.Definition.GetField(fieldId).ParameterId;
              ICollection<ScheduleField> list3 = (ICollection<ScheduleField>) newSchedule.Definition.GetFieldOrder().Where<ScheduleFieldId>((Func<ScheduleFieldId, bool>) (partsListFieldId => newSchedule.Definition.GetField(partsListFieldId).ParameterId == parameterId)).Select<ScheduleFieldId, ScheduleField>((Func<ScheduleFieldId, ScheduleField>) (partsListFieldId => newSchedule.Definition.GetField(partsListFieldId))).ToList<ScheduleField>();
              if (list3.Count == 1)
              {
                ScheduleSortGroupField sortGroupField2 = new ScheduleSortGroupField(list3.First<ScheduleField>().FieldId, sortOrder);
                newSchedule.Definition.AddSortGroupField(sortGroupField2);
              }
            }
            this.AddFilters(newSchedule, scheduleName);
            newSchedule.Definition.IsItemized = false;
            newSchedule.ApplyViewTemplateParameters((View) otherView);
            BoundingBoxUV outline = newSchedule.Outline;
            if (outline.Min.IsAlmostEqualTo(outline.Max))
              elementIds2.Add(newSchedule.Id);
            else
              elementIds1.Add(newSchedule.Id);
          }
        }
        document.Delete(elementIds2);
        Line bound = Line.CreateBound(XYZ.Zero, XYZ.BasisX);
        ElementId id = document.Create.NewDetailCurve(document.ActiveView, (Curve) bound).Id;
        foreach (ElementId elementId in (IEnumerable<ElementId>) elementIds1)
        {
          document.GetElement(elementId);
          XYZ origin = activeUiDocument.Selection.PickPoint($"Pick the upper left corner where {document.GetElement(elementId).Name} shall be placed.");
          ScheduleSheetInstance.Create(document, document.ActiveView.Id, elementId, origin);
          document.Regenerate();
        }
        document.Delete(id);
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("Name must be unique."))
        {
          new TaskDialog("Error")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Schedules have already been created for this Assembly."
          }.Show();
          return (Result) 1;
        }
        if (ex.ToString().Contains("The user aborted the pick operation."))
        {
          int num = (int) transaction.Commit();
          return (Result) 0;
        }
        message = ex.ToString();
        int num3 = (int) transaction.RollBack();
        return (Result) -1;
      }
    }
  }

  private void AddFilters(ViewSchedule partsList, string scheduleName)
  {
    ICollection<ScheduleFieldId> fieldOrder = (ICollection<ScheduleFieldId>) partsList.Definition.GetFieldOrder();
    if (scheduleName.Equals("PARTS_LIST"))
    {
      foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) fieldOrder)
      {
        if (partsList.Definition.GetField(fieldId).GetName().Equals("CONSTRUCTION_PRODUCT"))
        {
          partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.LessThan, ""));
        }
        else
        {
          if (partsList.Definition.GetField(fieldId).GetName().Equals("MANUFACTURE_COMPONENT"))
          {
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.HasParameter));
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.NotContains, "WWF"));
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.NotContains, "INSULATION"));
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.NotContains, "CGRID"));
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.NotContains, "SPIRAL"));
          }
          if (partsList.Definition.GetField(fieldId).GetName().Equals("IDENTITY_DESCRIPTION"))
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.GreaterThan, ""));
        }
      }
    }
    else
    {
      if (scheduleName.Equals("WWF_SCHEDULE"))
      {
        foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) fieldOrder)
        {
          if (partsList.Definition.GetField(fieldId).GetName().Contains("MANUFACTURE_COMPONENT"))
          {
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.Contains, "WWF"));
            return;
          }
        }
      }
      if (scheduleName.Equals("INSULATION_SCHEDULE"))
      {
        foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) fieldOrder)
        {
          if (partsList.Definition.GetField(fieldId).GetName().Contains("MANUFACTURE_COMPONENT"))
          {
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.Contains, "INSULATION"));
            return;
          }
        }
      }
      if (scheduleName.Equals("CGRID_SCHEDULE"))
      {
        foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) fieldOrder)
        {
          if (partsList.Definition.GetField(fieldId).GetName().Contains("MANUFACTURE_COMPONENT"))
          {
            partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.Contains, "CGRID"));
            return;
          }
        }
      }
      if (!scheduleName.Equals("SPIRAL_WIRE_SCHEDULE"))
        return;
      foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) fieldOrder)
      {
        if (partsList.Definition.GetField(fieldId).GetName().Contains("MANUFACTURE_COMPONENT"))
        {
          partsList.Definition.AddFilter(new ScheduleFilter(fieldId, ScheduleFilterType.Contains, "REBAR SPIRAL"));
          break;
        }
      }
    }
  }
}
