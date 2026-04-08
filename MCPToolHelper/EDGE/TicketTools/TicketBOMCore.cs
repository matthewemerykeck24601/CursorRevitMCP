// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketBOMCore
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.UserSettingTools.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils.HostingUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

internal class TicketBOMCore
{
  private static ICollection<ElementId> allDocumentScheduleIdsList = (ICollection<ElementId>) new List<ElementId>();
  private static ICollection<ElementId> emptyScheduleIdsToDeleteList = (ICollection<ElementId>) new List<ElementId>();
  private static IEnumerable<ViewSchedule> templateSchedulesFromDocument;
  private static ICollection<ViewSchedule> existingSchedulesForCurrentAssembly;
  private static ICollection<string> cannotDuplicateScheduleList = (ICollection<string>) new List<string>();
  private static ICollection<string> cannotCreateFilterList = (ICollection<string>) new List<string>();
  public static Dictionary<string, string> templateToSuffix = new Dictionary<string, string>();
  private static bool constProdHostNotFound = false;

  public static Dictionary<ElementId, string> GetScheduleInstancesForAssembly(
    Document revitDoc,
    UIDocument uiDoc,
    ElementId currentAssemblyId,
    out string errMessage,
    out Result functionResult,
    bool cloneTicket = false,
    Dictionary<string, string> schedulesToCopy = null)
  {
    bool flag1 = false;
    errMessage = string.Empty;
    if (TicketBOMCore.allDocumentScheduleIdsList != null)
      TicketBOMCore.allDocumentScheduleIdsList.Clear();
    if (TicketBOMCore.emptyScheduleIdsToDeleteList != null)
      TicketBOMCore.emptyScheduleIdsToDeleteList.Clear();
    if (TicketBOMCore.templateSchedulesFromDocument != null)
      TicketBOMCore.templateSchedulesFromDocument = (IEnumerable<ViewSchedule>) null;
    if (TicketBOMCore.existingSchedulesForCurrentAssembly != null)
      TicketBOMCore.existingSchedulesForCurrentAssembly.Clear();
    if (TicketBOMCore.cannotDuplicateScheduleList != null)
      TicketBOMCore.cannotDuplicateScheduleList.Clear();
    if (TicketBOMCore.cannotCreateFilterList != null)
      TicketBOMCore.cannotCreateFilterList.Clear();
    if (TicketBOMCore.templateToSuffix == null)
      TicketBOMCore.templateToSuffix = new Dictionary<string, string>();
    else
      TicketBOMCore.templateToSuffix.Clear();
    Dictionary<ElementId, string> instancesForAssembly = new Dictionary<ElementId, string>();
    string assemblyName = revitDoc.GetElement(currentAssemblyId).Name;
    IEnumerable<ViewSchedule> source = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSchedule)).ToElements().Cast<ViewSchedule>();
    Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    string precastManufacturerName = parameter == null ? "default" : (parameter.AsString() == null ? "default" : parameter.AsString().ToUpper());
    string str1 = string.IsNullOrEmpty(App.TKTBOMFolderPath) ? $"C:/EDGEforREVIT/{precastManufacturerName}_TICKET_BOM.xml" : $"{App.TKTBOMFolderPath}\\{precastManufacturerName}_TICKET_BOM.xml";
    List<ViewSchedule> viewScheduleList = new List<ViewSchedule>();
    TKTBOMSettings tktbomSettings = new TKTBOMSettings();
    if (File.Exists(str1) && tktbomSettings.LoadTicketTemplateSettings(str1))
    {
      foreach (ItemEntry ticketBom in tktbomSettings.TicketBOMList)
      {
        if (!string.IsNullOrWhiteSpace(ticketBom.scheduleSuffix) && !TicketBOMCore.templateToSuffix.ContainsKey(ticketBom.templateSchedule))
          TicketBOMCore.templateToSuffix.Add(ticketBom.templateSchedule, ticketBom.scheduleSuffix);
        if (!string.IsNullOrEmpty(ticketBom.templateSchedule))
        {
          foreach (ViewSchedule viewSchedule in source)
          {
            if (viewSchedule.Name.Equals(ticketBom.templateSchedule))
            {
              viewScheduleList.Add(viewSchedule);
              break;
            }
          }
        }
      }
      TicketBOMCore.templateSchedulesFromDocument = (IEnumerable<ViewSchedule>) viewScheduleList;
    }
    else
      TicketBOMCore.templateSchedulesFromDocument = source.Where<ViewSchedule>((Func<ViewSchedule, bool>) (schedule => schedule.Name.Equals("z_parts_list_template") || schedule.Name.Equals("z_wwf_schedule_template") || schedule.Name.Equals("z_insulation_schedule_template") || schedule.Name.Equals("z_mdk_insulation_schedule_template") || schedule.Name.Equals("z_cgrid_schedule_template") || schedule.Name.Equals("z_spiral_wire_schedule_template") || schedule.Name.Equals("z_strand_schedule_template") || schedule.Name.Equals("z_rebar_sizes_schedule_note") || schedule.Name.Equals("z_handling_schedule_template") || schedule.Name.Equals("z_rebar_schedule_template") || schedule.Name.Equals("z_handling_schedule_note") || schedule.Name.Equals("z_parts_list_schedule_note") || schedule.Name.Equals("z_rebar_schedule_note") || schedule.Name.Equals("z_erection_bom_schedule") || schedule.Name.Equals("z_stem_mesh_schedule_template") || schedule.Name.Equals("z_woodnailer_schedule_template") || schedule.Name.Equals("z_stone_count_template") || schedule.Name.Equals("z_finish_template") || schedule.Name.Equals("z_bent_rebar_schedule_template") || schedule.Name.Equals("z_laminar_tie_template") || schedule.Name.Equals("z_brick_schedule_template") || schedule.Name.Equals("z_brick_count_schedule_template") || schedule.Name.Equals("z_yard_schedule_template")));
    TicketBOMCore.existingSchedulesForCurrentAssembly = (ICollection<ViewSchedule>) source.Where<ViewSchedule>((Func<ViewSchedule, bool>) (s => s.Name.Contains("ZZ_" + assemblyName.ToUpper()))).ToList<ViewSchedule>();
    try
    {
      if (!TicketBOMCore.existingSchedulesForCurrentAssembly.Any<ViewSchedule>())
      {
        List<ElementId> selectedIdList = new List<ElementId>();
        selectedIdList.Add(currentAssemblyId);
        ActiveModel.GetInformation(uiDoc);
        ConstructionProductHosting.SelectedElements((ICollection<ElementId>) selectedIdList);
      }
      instancesForAssembly.Clear();
      QA.LogLine("Beginning GetScheduleInstancesForAssembly() for assembly: " + assemblyName);
      List<TicketBOMCore.BOMScheduleName> scheduleNamesList = TicketBOMCore.GetOrderedScheduleNamesList(precastManufacturerName);
      if (!cloneTicket)
      {
        foreach (TicketBOMCore.BOMScheduleName scheduleName in scheduleNamesList)
        {
          ViewSchedule duplicateSchedule = TicketBOMCore.GetOrDuplicateSchedule(revitDoc, scheduleName, assemblyName, currentAssemblyId);
          if (duplicateSchedule != null)
            instancesForAssembly.Add(duplicateSchedule.Id, $"Pick the upper left corner where {duplicateSchedule.Name} shall be placed.");
          else
            QA.LogLine($"GetScheduleInstancesForAssembly unable to get schedule for assembly: {assemblyName} schedule template: {scheduleName.GetTemplateName()} existing name expected: {scheduleName.GetExsitingScheduleName(assemblyName)}");
        }
      }
      else if (schedulesToCopy != null)
      {
        foreach (string key in schedulesToCopy.Keys)
        {
          string scheduleNameKey = key;
          int index1 = scheduleNamesList.FindIndex((Predicate<TicketBOMCore.BOMScheduleName>) (x => x.GetExistingString().Contains(scheduleNameKey)));
          int index2 = scheduleNamesList.FindIndex((Predicate<TicketBOMCore.BOMScheduleName>) (x => x.GetTemplateName().Contains(scheduleNameKey)));
          int num = index1 >= 0 ? 1 : 0;
          bool flag2 = index2 >= 0;
          TicketBOMCore.BOMScheduleName scheduleName = (TicketBOMCore.BOMScheduleName) null;
          ViewSchedule viewSchedule1 = (ViewSchedule) null;
          string str2 = string.Empty;
          if (num != 0)
          {
            scheduleName = scheduleNamesList[index1];
            str2 = scheduleName.GetTemplateName();
            viewSchedule1 = TicketBOMCore.GetOrDuplicateSchedule(revitDoc, scheduleName, assemblyName, currentAssemblyId);
            foreach (ElementId scheduleIdsToDelete in (IEnumerable<ElementId>) TicketBOMCore.emptyScheduleIdsToDeleteList)
            {
              if (!instancesForAssembly.ContainsKey(scheduleIdsToDelete) && revitDoc.GetElement(scheduleIdsToDelete) != null)
                revitDoc.Delete(scheduleIdsToDelete);
            }
          }
          else if (flag2)
          {
            scheduleName = scheduleNamesList[index2];
            str2 = scheduleName.GetTemplateName();
            viewSchedule1 = TicketBOMCore.GetOrDuplicateSchedule(revitDoc, scheduleName, assemblyName, currentAssemblyId);
            foreach (ElementId scheduleIdsToDelete in (IEnumerable<ElementId>) TicketBOMCore.emptyScheduleIdsToDeleteList)
            {
              if (!instancesForAssembly.ContainsKey(scheduleIdsToDelete) && revitDoc.GetElement(scheduleIdsToDelete) != null)
                revitDoc.Delete(scheduleIdsToDelete);
            }
          }
          ViewSchedule viewSchedule2 = (ViewSchedule) null;
          if (viewSchedule1 != null)
          {
            instancesForAssembly.Add(viewSchedule1.Id, $"Pick the upper left corner where {viewSchedule1.Name} shall be placed.");
          }
          else
          {
            scheduleName = new TicketBOMCore.BOMScheduleName(schedulesToCopy[scheduleNameKey], scheduleNameKey);
            string newName = scheduleName.GetExsitingScheduleName(assemblyName);
            List<ViewSchedule> list1 = TicketBOMCore.existingSchedulesForCurrentAssembly.Where<ViewSchedule>((Func<ViewSchedule, bool>) (x => x.Name == newName)).ToList<ViewSchedule>();
            ViewSchedule viewSchedule3;
            if (list1.Count<ViewSchedule>() > 0)
            {
              viewSchedule3 = list1[0];
            }
            else
            {
              List<ViewSchedule> list2 = source.Where<ViewSchedule>((Func<ViewSchedule, bool>) (x => x.Name == schedulesToCopy[scheduleNameKey])).ToList<ViewSchedule>();
              if (list2.Count > 0)
              {
                viewSchedule2 = list2[0];
                viewSchedule3 = revitDoc.GetElement(viewSchedule2.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
              }
              else
                continue;
            }
            ScheduleField scheduleField = (ScheduleField) null;
            foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) viewSchedule3.Definition.GetFieldOrder())
            {
              ScheduleField field = viewSchedule3.Definition.GetField(fieldId);
              if (field.GetName().Equals("CONSTRUCTION_PRODUCT_HOST"))
              {
                scheduleField = field;
                break;
              }
            }
            if (scheduleField == null)
            {
              revitDoc.Delete(viewSchedule3.Id);
              viewSchedule3 = viewSchedule2;
            }
            else
            {
              ScheduleFilter filter1 = new ScheduleFilter(scheduleField.FieldId, ScheduleFilterType.Equal, assemblyName);
              int index3 = 0;
              bool flag3 = false;
              foreach (ScheduleFilter filter2 in (IEnumerable<ScheduleFilter>) viewSchedule3.Definition.GetFilters())
              {
                if (filter2.FieldId == scheduleField.FieldId)
                {
                  viewSchedule3.Definition.RemoveFilter(index3);
                  flag3 = true;
                  break;
                }
                ++index3;
              }
              if (!flag3)
              {
                revitDoc.Delete(viewSchedule3.Id);
                viewSchedule3 = viewSchedule2;
              }
              else
              {
                viewSchedule3.Name = newName;
                if (viewSchedule3.Definition.GetFilterCount() < 8)
                  viewSchedule3.Definition.AddFilter(filter1);
                else
                  revitDoc.Delete(viewSchedule3.Id);
              }
            }
            viewSchedule1 = viewSchedule3;
            if (viewSchedule1 != null)
            {
              BoundingBoxUV outline = viewSchedule1.Outline;
              if (outline.Min.IsAlmostEqualTo(outline.Max))
              {
                if (!TicketBOMCore.emptyScheduleIdsToDeleteList.Contains(viewSchedule1.Id))
                  TicketBOMCore.emptyScheduleIdsToDeleteList.Add(viewSchedule1.Id);
                viewSchedule1 = (ViewSchedule) null;
              }
            }
            else if (!flag1)
            {
              errMessage = "One or more schedules had too many filters and could not be duplicated. ";
              flag1 = true;
            }
            else if (!errMessage.Contains("One or more schedules had too many filters and could not be duplicated. "))
              errMessage += "\nOne or more schedules had too many filters and could not be duplicated. ";
          }
          if (viewSchedule1 != null)
          {
            if (TicketBOMCore.cannotCreateFilterList.Contains(str2))
              TicketBOMCore.cannotCreateFilterList.Remove(str2);
            if (!instancesForAssembly.ContainsKey(viewSchedule1.Id))
              instancesForAssembly.Add(viewSchedule1.Id, $"Pick the upper left corner where {viewSchedule1.Name} shall be placed.");
          }
          else
            QA.LogLine($"GetScheduleInstancesForAssembly unable to get schedule for assembly: {assemblyName} schedule template: {scheduleName.GetTemplateName()} existing name expected: {scheduleName.GetExsitingScheduleName(assemblyName)}");
        }
      }
      foreach (ElementId scheduleIdsToDelete in (IEnumerable<ElementId>) TicketBOMCore.emptyScheduleIdsToDeleteList)
      {
        if (!instancesForAssembly.ContainsKey(scheduleIdsToDelete) && revitDoc.GetElement(scheduleIdsToDelete) != null)
          revitDoc.Delete(scheduleIdsToDelete);
      }
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref functionResult = 0;
      if (TicketBOMCore.cannotCreateFilterList.Count > 0)
      {
        if (!cloneTicket)
        {
          TaskDialog taskDialog = new TaskDialog("Duplicate Schedule Error");
          taskDialog.MainInstruction = "One or more schedules had too many filters and could not be duplicated. Expand for details.";
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string cannotCreateFilter in (IEnumerable<string>) TicketBOMCore.cannotCreateFilterList)
            stringBuilder.AppendLine(cannotCreateFilter);
          taskDialog.ExpandedContent = stringBuilder.ToString();
          taskDialog.Show();
        }
        else if (!flag1)
        {
          errMessage = "One or more schedules had too many filters and could not be duplicated. ";
          flag1 = true;
        }
        else if (!errMessage.Contains("One or more schedules had too many filters and could not be duplicated. "))
          errMessage += "\nOne or more schedules had too many filters and could not be duplicated. ";
      }
      if (TicketBOMCore.cannotDuplicateScheduleList.Count > 0)
      {
        if (!cloneTicket)
        {
          TaskDialog taskDialog = new TaskDialog("Duplicate Schedule Error");
          taskDialog.MainInstruction = TicketBOMCore.constProdHostNotFound ? "One or more schedules do not contain the CONSTRUCTION_PRODUCT_HOST parameter. Please make sure that the parameter is added to the schedule. Expand for details." : "One or more schedules could not be duplicated since a filter cannot be added to the note type schedule. Please make sure all note type schedule have no suffix assigned. Expand for details.";
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string duplicateSchedule in (IEnumerable<string>) TicketBOMCore.cannotDuplicateScheduleList)
            stringBuilder.AppendLine(duplicateSchedule);
          taskDialog.ExpandedContent = stringBuilder.ToString();
          taskDialog.Show();
        }
        else
        {
          string empty = string.Empty;
          string str3 = TicketBOMCore.constProdHostNotFound ? "One or more schedules do not contain the CONSTRUCTION_PRODUCT_HOST parameter. Please make sure that the parameter is added to the schedule." : "One or more schedules could not be duplicated since a filter cannot be added to the note type schedule. Please make sure all note type schedule have no suffix assigned.";
          if (!flag1)
            errMessage = str3;
          else if (!errMessage.Contains(str3))
            errMessage = $"{errMessage}\n{str3}";
        }
      }
      return instancesForAssembly;
    }
    catch (Exception ex)
    {
      if (!cloneTicket)
        TaskDialog.Show("Edge Exception", "Unknown Exception in TicketBOMCore: " + ex.ToString());
      errMessage = ex.ToString();
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref functionResult = -1;
      return (Dictionary<ElementId, string>) null;
    }
  }

  private static ViewSchedule GetOrDuplicateSchedule(
    Document revitDoc,
    TicketBOMCore.BOMScheduleName scheduleName,
    string assemblyName,
    ElementId currentAssemblyId)
  {
    IEnumerable<ViewSchedule> source1 = TicketBOMCore.existingSchedulesForCurrentAssembly.Where<ViewSchedule>((Func<ViewSchedule, bool>) (s => s.Name.Equals(scheduleName.GetExsitingScheduleName(assemblyName))));
    if (source1.Any<ViewSchedule>())
      return source1.First<ViewSchedule>() ?? (ViewSchedule) null;
    if (scheduleName.CreateDuplicateForAssembly)
      return TicketBOMCore.DuplicateSchedule(revitDoc, scheduleName, currentAssemblyId, assemblyName);
    IEnumerable<ViewSchedule> source2 = TicketBOMCore.templateSchedulesFromDocument.Where<ViewSchedule>((Func<ViewSchedule, bool>) (s => s.Name.Equals(scheduleName.GetTemplateName())));
    if (source2.Any<ViewSchedule>())
      return source2.First<ViewSchedule>() ?? (ViewSchedule) null;
    QA.LogLine("Unable to get matching template schedule for " + scheduleName.GetTemplateName());
    return (ViewSchedule) null;
  }

  private static ViewSchedule DuplicateSchedule(
    Document revitDoc,
    TicketBOMCore.BOMScheduleName scheduleToDuplicate,
    ElementId currentAssemblyId,
    string assemblyName)
  {
    string templateNameToDuplicate = scheduleToDuplicate.GetTemplateName();
    IEnumerable<ViewSchedule> source = TicketBOMCore.templateSchedulesFromDocument.Where<ViewSchedule>((Func<ViewSchedule, bool>) (s => s.Name.Equals(templateNameToDuplicate)));
    if (!source.Any<ViewSchedule>())
      return (ViewSchedule) null;
    ViewSchedule viewSchedule = source.First<ViewSchedule>();
    ViewSchedule element = revitDoc.GetElement(viewSchedule.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
    string exsitingScheduleName = scheduleToDuplicate.GetExsitingScheduleName(assemblyName);
    try
    {
      element.Name = exsitingScheduleName;
    }
    catch
    {
      int num = 0;
      bool flag = true;
      while (flag)
      {
        if (num < 100)
        {
          try
          {
            element.Name = $"Error for Name {scheduleToDuplicate.GetExsitingScheduleName(assemblyName)}{num.ToString()}";
            flag = false;
          }
          catch
          {
            ++num;
            flag = true;
          }
        }
        else
          break;
      }
    }
    ScheduleField scheduleField = (ScheduleField) null;
    foreach (ScheduleFieldId fieldId in (IEnumerable<ScheduleFieldId>) element.Definition.GetFieldOrder())
    {
      ScheduleField field = element.Definition.GetField(fieldId);
      if (field.GetName().Equals("CONSTRUCTION_PRODUCT_HOST"))
      {
        scheduleField = field;
        break;
      }
    }
    if (scheduleField == null)
    {
      TicketBOMCore.cannotDuplicateScheduleList.Add(templateNameToDuplicate);
      revitDoc.Delete(element.Id);
      TicketBOMCore.constProdHostNotFound = true;
      return (ViewSchedule) null;
    }
    ScheduleFilter filter = new ScheduleFilter(scheduleField.FieldId, ScheduleFilterType.Equal, assemblyName);
    if (element.Definition.GetFilterCount() < 8)
    {
      element.Definition.AddFilter(filter);
      BoundingBoxUV outline = element.Outline;
      if (!outline.Min.IsAlmostEqualTo(outline.Max))
        return element;
      TicketBOMCore.emptyScheduleIdsToDeleteList.Add(element.Id);
      QA.LogLine(element.Name + " is empty and will not be placed on the assembly ticket");
      return (ViewSchedule) null;
    }
    TicketBOMCore.emptyScheduleIdsToDeleteList.Add(element.Id);
    TicketBOMCore.cannotCreateFilterList.Add(templateNameToDuplicate);
    return (ViewSchedule) null;
  }

  public static List<TicketBOMCore.BOMScheduleName> GetOrderedScheduleNamesList(
    string precastManufacturerName)
  {
    string str = string.IsNullOrEmpty(App.TKTBOMFolderPath) ? $"C:/EDGEforREVIT/{precastManufacturerName}_TICKET_BOM.xml" : $"{App.TKTBOMFolderPath}\\{precastManufacturerName}_TICKET_BOM.xml";
    TKTBOMSettings tktbomSettings = new TKTBOMSettings();
    if (File.Exists(str) && tktbomSettings.LoadTicketTemplateSettings(str))
    {
      List<TicketBOMCore.BOMScheduleName> scheduleNamesList = new List<TicketBOMCore.BOMScheduleName>();
      foreach (ItemEntry ticketBom in tktbomSettings.TicketBOMList)
      {
        if (!string.IsNullOrEmpty(ticketBom.templateSchedule))
        {
          if (string.IsNullOrEmpty(ticketBom.scheduleSuffix) || string.IsNullOrEmpty(ticketBom.scheduleSuffix))
            scheduleNamesList.Add(new TicketBOMCore.BOMScheduleName(ticketBom.templateSchedule, "", false));
          else
            scheduleNamesList.Add(new TicketBOMCore.BOMScheduleName(ticketBom.templateSchedule, ticketBom.scheduleSuffix));
        }
      }
      return scheduleNamesList;
    }
    string upper = precastManufacturerName.ToUpper();
    if (upper != null)
    {
      switch (upper.Length)
      {
        case 4:
          if (upper == "GATE")
            return new List<TicketBOMCore.BOMScheduleName>()
            {
              new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
              new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_mdk_insulation_schedule_template", "MDK_INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_handling_schedule_template", "HANDLING_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_rebar_schedule_template", "REBAR_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_strand_schedule_template", "STRAND_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE")
            };
          break;
        case 5:
          if (upper == "WELLS")
            return new List<TicketBOMCore.BOMScheduleName>()
            {
              new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
              new TicketBOMCore.BOMScheduleName("z_rebar_schedule_template", "REBAR_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_finish_template", "FINISH_TAG_SCHEDULE")
            };
          break;
        case 7:
          if (upper == "TINDALL")
            return new List<TicketBOMCore.BOMScheduleName>()
            {
              new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
              new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_stem_mesh_schedule_template", "STEM_MESH_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_mdk_insulation_schedule_template", "MDK_INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_strand_schedule_template", "STRAND_SCHEDULE")
            };
          break;
        case 8:
          if (upper == "CORESLAB")
            return new List<TicketBOMCore.BOMScheduleName>()
            {
              new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
              new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_mdk_insulation_schedule_template", "MDK_INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE")
            };
          break;
        case 9:
          switch (upper[0])
          {
            case 'M':
              if (upper == "METROMONT")
                return new List<TicketBOMCore.BOMScheduleName>()
                {
                  new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
                  new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_mdk_insulation_schedule_template", "MDK_INSULATION_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_rebar_sizes_schedule_note", "", false)
                };
              break;
            case 'U':
              if (upper == "UNISTRESS")
                return new List<TicketBOMCore.BOMScheduleName>()
                {
                  new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
                  new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_mdk_insulation_schedule_template", "MDK_INSULATION_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE")
                };
              break;
          }
          break;
        case 12:
          if (upper == "CORESLAB OKC")
            return new List<TicketBOMCore.BOMScheduleName>()
            {
              new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
              new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_laminar_tie_template", "LAMINAR_TIE_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_woodnailer_schedule_template", "WOOD_NAILER"),
              new TicketBOMCore.BOMScheduleName("z_rebar_schedule_template", "REBAR_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_stem_mesh_schedule_template", "STEM_MESH_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_strand_schedule_template", "STRAND_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_yard_schedule_template", "YARD_SCHEDULE")
            };
          break;
        case 13:
          switch (upper[0])
          {
            case 'C':
              if (upper == "CORESLAB INDY")
                return new List<TicketBOMCore.BOMScheduleName>()
                {
                  new TicketBOMCore.BOMScheduleName("z_parts_list_schedule_note", "", false),
                  new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
                  new TicketBOMCore.BOMScheduleName("z_brick_schedule_template", "BRICK_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_brick_count_schedule_template", "SINGLE_BRICK_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_laminar_tie_template", "LAMINAR_TIE_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_woodnailer_schedule_template", "WOOD_NAILER"),
                  new TicketBOMCore.BOMScheduleName("z_rebar_schedule_note", "", false),
                  new TicketBOMCore.BOMScheduleName("z_rebar_schedule_template", "REBAR_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_stem_mesh_schedule_template", "STEM_MESH_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_strand_schedule_template", "STRAND_SCHEDULE")
                };
              break;
            case 'G':
              if (upper == "GAGE BROTHERS")
                return new List<TicketBOMCore.BOMScheduleName>()
                {
                  new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
                  new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_erection_bom_schedule", "ERECTION_BOM"),
                  new TicketBOMCore.BOMScheduleName("z_woodnailer_schedule_template", "WOOD_NAILER"),
                  new TicketBOMCore.BOMScheduleName("z_brick_schedule_template", "BRICK_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_brick_count_schedule_template", "BRICK_COUNT_SCHEDULE"),
                  new TicketBOMCore.BOMScheduleName("z_stone_count_template", "STONE_COUNT_SCHEDULE")
                };
              break;
          }
          break;
        case 24:
          if (upper == "ROCKY MOUNTAIN PRESTRESS")
            return new List<TicketBOMCore.BOMScheduleName>()
            {
              new TicketBOMCore.BOMScheduleName("z_strand_schedule_template", "STRAND_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_stem_mesh_schedule_template", "STEM_MESH_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
              new TicketBOMCore.BOMScheduleName("z_handling_schedule_template", "HANDLING_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_rebar_schedule_template", "REBAR_SCHEDULE"),
              new TicketBOMCore.BOMScheduleName("z_erection_bom_schedule", "ERECTION_BOM"),
              new TicketBOMCore.BOMScheduleName("z_bent_rebar_schedule_template", "BENT_REBAR_BOM")
            };
          break;
      }
    }
    return new List<TicketBOMCore.BOMScheduleName>()
    {
      new TicketBOMCore.BOMScheduleName("z_parts_list_template", "PARTS_LIST"),
      new TicketBOMCore.BOMScheduleName("z_wwf_schedule_template", "WWF_SCHEDULE"),
      new TicketBOMCore.BOMScheduleName("z_insulation_schedule_template", "INSULATION_SCHEDULE"),
      new TicketBOMCore.BOMScheduleName("z_mdk_insulation_schedule_template", "MDK_INSULATION_SCHEDULE"),
      new TicketBOMCore.BOMScheduleName("z_cgrid_schedule_template", "CGRID_SCHEDULE"),
      new TicketBOMCore.BOMScheduleName("z_spiral_wire_schedule_template", "SPIRAL_WIRE_SCHEDULE"),
      new TicketBOMCore.BOMScheduleName("z_strand_schedule_template", "STRAND_SCHEDULE")
    };
  }

  public class BOMScheduleName
  {
    private string _existingString = "";
    private string _templateString = "";
    private bool _bCreateDuplicateForAssembly = true;

    public bool CreateDuplicateForAssembly
    {
      get => this._bCreateDuplicateForAssembly;
      set => this._bCreateDuplicateForAssembly = value;
    }

    public BOMScheduleName(
      string templateString,
      string existingString,
      bool bDuplicateForEachAssembly = true)
    {
      this._existingString = existingString;
      this._templateString = templateString;
      this._bCreateDuplicateForAssembly = bDuplicateForEachAssembly;
    }

    public BOMScheduleName()
    {
    }

    public string GetTemplateName() => this._templateString;

    public string GetExsitingScheduleName(string assemblyName)
    {
      return $"ZZ_{assemblyName}_{this._existingString}".ToUpper();
    }

    public string GetExistingString() => this._existingString;
  }
}
