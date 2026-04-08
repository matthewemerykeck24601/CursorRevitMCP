// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingPerPiece
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.UserSettingTools.Insulation_Drawing_Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils.IEnumerable_Extensions;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

internal class InsulationDrawingPerPiece
{
  public ObservableCollection<InsulationDrawingPerPieceObject> PerPieceList;

  public InsulationDrawingPerPiece()
  {
    this.PerPieceList = new ObservableCollection<InsulationDrawingPerPieceObject>();
  }

  public static ViewSheet CreateLegends(
    ObservableCollection<InsulationDrawingPerPieceObject> ppoList,
    int scale,
    InsulationDrawingSettingsObject settingsObject)
  {
    Document document = (Document) null;
    if (ppoList.Count <= 0)
      return (ViewSheet) null;
    if (ppoList[0].assemblyInstance != null)
      document = ppoList[0].assemblyInstance.Document;
    if (document == null)
      return (ViewSheet) null;
    List<string> stringList1 = new List<string>();
    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
    ViewSheet legends = (ViewSheet) null;
    using (Transaction transaction = new Transaction(document, "Insulation Marking - Assembly"))
    {
      int num1 = (int) transaction.Start();
      List<View> list1 = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Views).Cast<View>().ToList<View>();
      if (document.IsWorkshared)
      {
        List<ElementId> ElementIds = new List<ElementId>();
        foreach (InsulationDrawingPerPieceObject ppo in (Collection<InsulationDrawingPerPieceObject>) ppoList)
        {
          if (ppo.assemblyInstance != null)
          {
            string str = "INSULATION DETAIL - " + ppo.AssemblyName;
            foreach (View view in list1)
            {
              if (view.ViewType == ViewType.Legend && view.Name == str)
              {
                ElementIds.Add(view.Id);
                break;
              }
            }
            ViewSheet viewSheet = (ViewSheet) null;
            if (ppo.SheetsDictionary.ContainsKey(ppo.SheetName))
              viewSheet = ppo.SheetsDictionary[ppo.SheetName];
            if (viewSheet != null)
              ElementIds.Add(viewSheet.Id);
          }
        }
        ICollection<ElementId> UniqueElementIds;
        if (CheckElementsOwnership.CheckOwnership(document, (ICollection<ElementId>) ElementIds, out UniqueElementIds, out ICollection<ElementId> _, out ICollection<ElementId> _) && UniqueElementIds.Count != 0)
        {
          TaskDialog taskDialog1 = new TaskDialog("EDGE Worksharing Error - Insulation Drawing - Assembly");
          taskDialog1.MainInstruction = "The sheets and legends needed to draw insulation for the selected assemblies and sheets are not editable";
          taskDialog1.MainContent = "The sheets and legends needed to draw insulation for the selected assemblies and sheets are owned by another user and are not editable. Please coordinate with project members to allow for ownership of the elements or try reloading latest from central. Insulation Drawing - Assembly will be canceled.";
          taskDialog1.ExpandedContent = "View Ids:\n";
          foreach (ElementId elementId in (IEnumerable<ElementId>) UniqueElementIds)
          {
            TaskDialog taskDialog2 = taskDialog1;
            taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{elementId.ToString()}\n";
          }
          taskDialog1.Show();
          int num2 = (int) transaction.RollBack();
          return (ViewSheet) null;
        }
      }
      foreach (InsulationDrawingPerPieceObject ppo in (Collection<InsulationDrawingPerPieceObject>) ppoList)
      {
        AssemblyInstance assemblyInstance = ppo.assemblyInstance;
        if (assemblyInstance != null)
        {
          ViewSheet A_0 = (ViewSheet) null;
          if (ppo.SheetsDictionary.ContainsKey(ppo.SheetName))
            A_0 = ppo.SheetsDictionary[ppo.SheetName];
          if (A_0 == null)
          {
            stringList1.Add(ppo.AssemblyName);
          }
          else
          {
            string str1 = "INSULATION DETAIL - " + ppo.AssemblyName;
            bool flag = false;
            View legend = (View) null;
            foreach (View view in list1)
            {
              if (view.IsValidObject && view.ViewType == ViewType.Legend)
              {
                legend = view;
                if (view.Name == str1)
                {
                  flag = true;
                  break;
                }
              }
            }
            if (legend == null)
              return (ViewSheet) null;
            View newLegend = InsulationDrawingUtils.CreateNewLegend(legend, scale);
            if (newLegend == null)
            {
              stringList1.Add(ppo.AssemblyName);
            }
            else
            {
              if (flag)
                assemblyInstance.Document.Delete(legend.Id);
              newLegend.Name = str1;
              List<string> list2 = ppo.insulationMarkToElementDictionary.Keys.ToList<string>().NaturalSort().ToList<string>();
              double num3 = 0.2 * (double) newLegend.Scale;
              XYZ location = XYZ.Zero;
              List<string> stringList2 = new List<string>();
              foreach (string str2 in list2)
              {
                if (ppo.insulationMarkToElementDictionary.ContainsKey(str2))
                {
                  FamilyInstance insulationMarkToElement = ppo.insulationMarkToElementDictionary[str2];
                  if (insulationMarkToElement != null)
                  {
                    try
                    {
                      InsulationDetail insulationDetail = new InsulationDetail(insulationMarkToElement, settingsObject, str2);
                      if (insulationDetail.DrawDetail(newLegend, location, false))
                        location = new XYZ(location.X + insulationDetail.width + num3, location.Y, 0.0);
                      else
                        stringList2.Add(str2);
                    }
                    catch
                    {
                      stringList2.Add(str2);
                    }
                  }
                  else
                    stringList2.Add(str2);
                }
              }
              if (stringList2.Count > 0)
              {
                if (dictionary.ContainsKey(ppo.AssemblyName))
                  dictionary[ppo.AssemblyName] = stringList2;
                else
                  dictionary.Add(ppo.AssemblyName, stringList2);
              }
              XYZ point = XYZ.Zero;
              FamilyInstance familyInstance = new FilteredElementCollector(document, A_0.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
              if (familyInstance != null)
              {
                BoundingBoxXYZ boundingBoxXyz = familyInstance.get_BoundingBox((View) A_0);
                if (boundingBoxXyz != null)
                {
                  XYZ xyz = new XYZ(boundingBoxXyz.Max.X, boundingBoxXyz.Min.Y, 0.0);
                  BoundingBoxUV outline = newLegend.Outline;
                  double num4 = (outline.Max.U - outline.Min.U) / 2.0;
                  double num5 = (outline.Max.V - outline.Min.V) / 2.0;
                  point = new XYZ(xyz.X + num4 + 0.25, xyz.Y + num5, 0.0);
                }
                Viewport.Create(document, A_0.Id, newLegend.Id, point);
                legends = A_0;
              }
            }
          }
        }
      }
      int num6 = (int) transaction.Commit();
    }
    if (stringList1.Count > 0)
    {
      TaskDialog taskDialog3 = new TaskDialog("Insulation Drawing - Assembly");
      taskDialog3.MainInstruction = "Failed to Create Legends";
      taskDialog3.MainContent = "Unable to create a new legend and draw insulation for one or more of the selected assemblies.";
      taskDialog3.ExpandedContent = "Failed Assemblies:";
      foreach (string str in stringList1)
      {
        TaskDialog taskDialog4 = taskDialog3;
        taskDialog4.ExpandedContent = $"{taskDialog4.ExpandedContent}\n{str}";
      }
      taskDialog3.Show();
    }
    if (dictionary.Count > 0)
    {
      TaskDialog taskDialog5 = new TaskDialog("Insulation Drawing - Assembly");
      taskDialog5.MainInstruction = "Failed to Draw Insulation for Certain Insulation Marks";
      taskDialog5.MainContent = "Unable to draw certain insulation marks. Legends were still created and placed.";
      taskDialog5.ExpandedContent = "Failed Assemblies and Insulation Marks:";
      foreach (string key in dictionary.Keys)
      {
        TaskDialog taskDialog6 = taskDialog5;
        taskDialog6.ExpandedContent = $"{taskDialog6.ExpandedContent}\n{key}: ";
        int num = 1;
        foreach (string str in dictionary[key])
        {
          taskDialog5.ExpandedContent += str;
          if (dictionary[key].Count != num)
            taskDialog5.ExpandedContent += ", ";
          ++num;
        }
      }
      taskDialog5.Show();
    }
    if (stringList1.Count != ppoList.Count)
      new TaskDialog("Insulation Drawing - Assembly")
      {
        MainInstruction = "Insulation Drawing - Assembly Finished Successfully."
      }.Show();
    return legends;
  }
}
