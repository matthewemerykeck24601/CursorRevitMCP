// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopCreationUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketPopulator;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools;

public static class TicketPopCreationUtils
{
  public static Result PlaceBOM(
    Document revitDoc,
    UIApplication revitApp,
    ViewSheet AssemblySheet,
    AssemblyInstance assInstance,
    TicketTemplate template,
    Dictionary<string, List<TicketScheduleInfo>> schedulesDictionary,
    out string createSchedulesErrorMessage)
  {
    Dictionary<string, string> schedulesToCopy = new Dictionary<string, string>();
    foreach (TicketScheduleInfo scheduleInfo in template.ScheduleInfos)
    {
      foreach (string key in schedulesDictionary.Keys)
      {
        if (schedulesDictionary[key].Select<TicketScheduleInfo, string>((Func<TicketScheduleInfo, string>) (x => x.BOMScheduleNameString)).Contains<string>(scheduleInfo.BOMScheduleNameString))
        {
          schedulesToCopy.Add(scheduleInfo.BOMScheduleNameString, key);
          break;
        }
      }
    }
    Result functionResult;
    Dictionary<ElementId, string> instancesForAssembly = TicketBOMCore.GetScheduleInstancesForAssembly(revitDoc, revitApp.ActiveUIDocument, assInstance.Id, out createSchedulesErrorMessage, out functionResult, true, schedulesToCopy);
    if (instancesForAssembly == null)
      return functionResult;
    IEnumerable<FamilyInstance> source1 = new FilteredElementCollector(revitDoc, AssemblySheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements().Cast<FamilyInstance>();
    XYZ point = XYZ.Zero;
    if (source1.Any<FamilyInstance>())
    {
      FamilyInstance familyInstance = source1.First<FamilyInstance>();
      point = (familyInstance.Location as LocationPoint).Point;
      try
      {
        familyInstance.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
        familyInstance.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();
      }
      catch
      {
      }
      if (PopulatorQA.inHouse)
        TemplateDebugUtils.DebugDrawCircleAtPoint(point, revitDoc, (View) AssemblySheet, "TitleBlock Location", false);
    }
    ScheduleSheetInstance scheduleSheetInstance1 = (ScheduleSheetInstance) null;
    XYZ xyz = point;
    double num = 1.0 / 72.0;
    List<TicketScheduleInfo> list = template.ScheduleInfos.OrderBy<TicketScheduleInfo, int>((Func<TicketScheduleInfo, int>) (s => s.iScheduleOrderIndex)).ToList<TicketScheduleInfo>();
    IEnumerable<ViewSchedule> source2 = instancesForAssembly.Keys.Select<ElementId, Element>((Func<ElementId, Element>) (o => revitDoc.GetElement(o))).Cast<ViewSchedule>();
    list.Select<TicketScheduleInfo, string>((Func<TicketScheduleInfo, string>) (s => s.BOMScheduleNameString.ToUpper()));
    HashSet<ElementId> elementIdSet = new HashSet<ElementId>();
    Dictionary<TicketScheduleInfo, ScheduleSheetInstance> dictionary = new Dictionary<TicketScheduleInfo, ScheduleSheetInstance>();
    foreach (TicketScheduleInfo key in list)
    {
      string scheduleNameString = key.BOMScheduleNameString;
      source2.Select<ViewSchedule, string>((Func<ViewSchedule, string>) (s => s.Name)).ToList<string>();
      string schedInfoNewScheduleName = key.GetNewScheduleName(assInstance.Name).ToUpper();
      if (schedulesToCopy[key.BOMScheduleNameString] == key.BOMScheduleNameString)
        schedInfoNewScheduleName = key.BOMScheduleNameString;
      IEnumerable<ViewSchedule> source3 = source2.Where<ViewSchedule>((Func<ViewSchedule, bool>) (o => o.Name.ToUpper().Contains(schedInfoNewScheduleName.ToUpper())));
      if (source3.Any<ViewSchedule>())
      {
        if (source3.Count<ViewSchedule>() > 1)
        {
          TaskDialog taskDialog = new TaskDialog("Multiple Schedule Matches");
          taskDialog.MainInstruction = "More than one schedule has been returned by BOMCore for schedule type " + key.BOMScheduleNameString.ToString();
          taskDialog.MainContent = "Only the first schedule will be placed in this situation.";
          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "OK");
          taskDialog.Show();
        }
        ViewSchedule viewSchedule = source3.First<ViewSchedule>();
        elementIdSet.Add(viewSchedule.Id);
        ScheduleSheetInstance scheduleSheetInstance2 = ScheduleSheetInstance.Create(revitDoc, AssemblySheet.Id, viewSchedule.Id, XYZ.Zero);
        dictionary.Add(key, scheduleSheetInstance2);
      }
    }
    foreach (TicketScheduleInfo key in dictionary.Keys)
    {
      if (template.BOMJustification == BOMJustification.Bottom)
      {
        BoundingBoxXYZ boundingBoxXyz = dictionary[key].get_BoundingBox((View) AssemblySheet);
        dictionary[key].Point += new XYZ(0.0, boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num, 0.0);
      }
      else
      {
        int bomJustification = (int) template.BOMJustification;
      }
      if (template.bStackSchedules)
      {
        if (scheduleSheetInstance1 == null)
        {
          xyz = point + template.BOMAnchorPosition;
        }
        else
        {
          BoundingBoxXYZ boundingBoxXyz = scheduleSheetInstance1.get_BoundingBox((View) AssemblySheet);
          if (template.BOMJustification == BOMJustification.Bottom)
            xyz += new XYZ(0.0, boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num, 0.0);
          else if (template.BOMJustification == BOMJustification.Top)
            xyz -= new XYZ(0.0, boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num, 0.0);
        }
        dictionary[key].Point += xyz;
      }
      else
        dictionary[key].Point += key.vectorToAnchorPoint;
      if (PopulatorQA.inHouse)
        TemplateDebugUtils.DebugDrawCircleAtPoint(dictionary[key].Point, revitDoc, (View) AssemblySheet, "CurrentInstancePoint", false);
      scheduleSheetInstance1 = dictionary[key];
    }
    return (Result) 0;
  }

  public static bool WithinViewAlignmentTolerance(XYZ point1, XYZ point2, double tolerance)
  {
    return point1.DistanceTo(point2) < tolerance;
  }
}
