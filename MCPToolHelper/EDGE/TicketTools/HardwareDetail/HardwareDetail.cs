// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.HardwareDetail
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.TicketTools.AutoDimensioning;
using EDGE.TicketTools.CopyTicketAnnotation;
using EDGE.TicketTools.HardwareDetail.Views;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketPopulator;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using EDGE.UserSettingTools.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Utils.ElementUtils;
using Utils.IEnumerable_Extensions;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail;

[Transaction(TransactionMode.Manual)]
public class HardwareDetail : IExternalCommand
{
  public static bool fallBackToCreateSimilar;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Autodesk.Revit.DB.Document revitDoc = activeUiDocument.Document;
    UIApplication application = commandData.Application;
    App.hardwareDetailErrorCheck(true);
    EDGE.TicketTools.HardwareDetail.HardwareDetail.fallBackToCreateSimilar = false;
    if (revitDoc.IsFamilyDocument)
    {
      int num = (int) MessageBox.Show("Hardware Detail cannot be used within a family document.");
      App.hardwareDetailErrorCheck(false);
      return (Result) 1;
    }
    Dictionary<string, List<Element>> dictionary1 = new Dictionary<string, List<Element>>();
    List<AssemblyInstance> list1 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Assemblies).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (x => Parameters.GetParameterAsInt((Element) x, "HARDWARE_DETAIL") == 1)).ToList<AssemblyInstance>();
    List<Element> source1 = new List<Element>();
    List<ElementId> list2 = activeUiDocument.Selection.GetElementIds().ToList<ElementId>();
    int num1 = 0;
    if (list2.Count<ElementId>() > 0)
    {
      List<Element> list3 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list2).OfCategory(BuiltInCategory.OST_Assemblies).ToList<Element>();
      list3.AddRange((IEnumerable<Element>) new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list2).OfCategory(BuiltInCategory.OST_GenericModel).ToList<Element>());
      list3.AddRange((IEnumerable<Element>) new FilteredElementCollector(revitDoc, (ICollection<ElementId>) list2).OfCategory(BuiltInCategory.OST_SpecialityEquipment).ToList<Element>());
      foreach (Element element in list3)
      {
        Element elem = element;
        string parameterAsString = Parameters.GetParameterAsString(elem, "CONTROL_MARK");
        if (!string.IsNullOrWhiteSpace(parameterAsString))
        {
          foreach (AssemblyInstance assemblyInstance in list1)
          {
            if (assemblyInstance.Name.ToUpper().Equals("_HWD-" + parameterAsString.ToUpper()) && assemblyInstance.GetMemberIds().ToList<ElementId>().Contains(element.Id))
            {
              elem = (Element) assemblyInstance;
              break;
            }
          }
        }
        bool? nullable = new bool?(false);
        string reason = string.Empty;
        string controlMark = (string) null;
        if (elem is AssemblyInstance)
        {
          nullable = EDGE.TicketTools.HardwareDetail.HardwareDetail.checkHardwareDetailStatus(elem as AssemblyInstance, out reason, out controlMark);
          if (nullable.GetValueOrDefault())
            ++num1;
        }
        else
          nullable = EDGE.TicketTools.HardwareDetail.HardwareDetail.checkHardwareDetailStatus(elem, out reason, out controlMark);
        if (dictionary1.ContainsKey(controlMark))
          dictionary1[controlMark].Add(elem);
        if (nullable.GetValueOrDefault())
        {
          source1.Add(elem);
          if (!dictionary1.ContainsKey(controlMark))
            dictionary1.Add(controlMark, new List<Element>()
            {
              elem
            });
        }
        else if (!nullable.HasValue && reason == "HARDWARE DETAIL PARAMETER")
        {
          new TaskDialog("Hardware Detail")
          {
            MainContent = "Required shared parameters were not found on one of the elements selected. Please run Project Shared Paramters and try to run the tool again."
          }.Show();
          App.hardwareDetailErrorCheck(false);
          return (Result) 1;
        }
      }
    }
    if (source1.Count < 1)
    {
      try
      {
        source1 = activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new EDGE.TicketTools.HardwareDetail.HardwareDetail.SelectionFlter(), "Select Elements").Select<Reference, Element>((Func<Reference, Element>) (x => revitDoc.GetElement(x.ElementId))).ToList<Element>();
        if (source1.Count < 1)
        {
          int num2 = (int) MessageBox.Show("No valid elements or assemblies selected.");
          App.hardwareDetailErrorCheck(false);
          return (Result) 1;
        }
        foreach (Element elem in source1)
        {
          if (elem is AssemblyInstance)
          {
            string upper = elem.Name.Replace("_HWD-", "").ToUpper();
            if (dictionary1.ContainsKey(upper))
              dictionary1[upper].Add(elem);
            else
              dictionary1.Add(upper, new List<Element>()
              {
                elem
              });
          }
          else
          {
            string upper = Parameters.GetParameterAsString(elem, "CONTROL_MARK").ToUpper();
            if (dictionary1.ContainsKey(upper))
              dictionary1[upper].Add(elem);
            else
              dictionary1.Add(upper, new List<Element>()
              {
                elem
              });
          }
        }
      }
      catch (Exception ex)
      {
        App.hardwareDetailErrorCheck(false);
        return (Result) 1;
      }
    }
    List<string> stringList1 = new List<string>();
    foreach (KeyValuePair<string, List<Element>> keyValuePair in dictionary1)
    {
      if (keyValuePair.Value.Count > 1)
        stringList1.Add(keyValuePair.Key);
    }
    if (stringList1.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Hardware Detail");
      taskDialog1.MainContent = "You have selected multiple elements or assemblies that have matching control marks. Please only select one element or assembly for each control mark and try to run the tool again.";
      foreach (string key in stringList1)
      {
        if (key != stringList1[0])
          taskDialog1.ExpandedContent += "\n";
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{key}: \n";
        foreach (Element element in dictionary1[key])
        {
          TaskDialog taskDialog3 = taskDialog1;
          taskDialog3.ExpandedContent = $"{taskDialog3.ExpandedContent}{element.Name} : {element.Id.ToString()}\n";
        }
      }
      taskDialog1.Show();
      App.hardwareDetailErrorCheck(false);
      return (Result) 1;
    }
    bool flag1 = true;
    List<ElementId> elemsUnownedWORKSHARED = new List<ElementId>();
    List<Element> elemsOwnedByOtherUser = new List<Element>();
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    if (revitDoc.IsWorkshared)
    {
      foreach (Element element in source1)
      {
        List<ElementId> elemsUnowned;
        List<Element> ownedByOtherUser;
        if (element is AssemblyInstance)
          this.GetCheckOutStatus(revitDoc, element as AssemblyInstance, out elemsUnowned, out ownedByOtherUser);
        else
          this.GetCheckOutStatus(revitDoc, element, out elemsUnowned, out ownedByOtherUser);
        elemsUnownedWORKSHARED.AddRange((IEnumerable<ElementId>) elemsUnowned);
        elemsOwnedByOtherUser.AddRange((IEnumerable<Element>) ownedByOtherUser);
      }
      elemsUnownedWORKSHARED = elemsUnownedWORKSHARED.Distinct<ElementId>().ToList<ElementId>();
      elemsOwnedByOtherUser = elemsOwnedByOtherUser.Distinct<Element>().ToList<Element>();
      if (elemsOwnedByOtherUser.Count > 0)
      {
        TaskDialog taskDialog4 = new TaskDialog("Warning");
        taskDialog4.MainContent = "One or more elements that are needed for the Hardware Detail tool are currently owned by another user. Please have them relinquish them and run the Hardware Detail tool again. Expand for details.";
        foreach (Element element in elemsOwnedByOtherUser)
        {
          TaskDialog taskDialog5 = taskDialog4;
          taskDialog5.ExpandedContent = $"{taskDialog5.ExpandedContent}{element.Name} : {element.Id.ToString()}\n";
        }
        taskDialog4.Show();
        App.hardwareDetailErrorCheck(false);
        if (elementIds.Count > 0)
        {
          RelinquishOptions generalCategories = new RelinquishOptions(false);
          generalCategories.CheckedOutElements = true;
          TransactWithCentralOptions options = new TransactWithCentralOptions();
          WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
        }
        return (Result) 1;
      }
      if (elemsUnownedWORKSHARED.Count > 0)
      {
        try
        {
          elementIds = WorksharingUtils.CheckoutElements(revitDoc, (ICollection<ElementId>) elemsUnownedWORKSHARED);
        }
        catch (Exception ex)
        {
          new TaskDialog("Warning")
          {
            MainContent = "Unable to taake ownership of needed elements to run the Hardware Detail tool."
          }.Show();
          App.hardwareDetailErrorCheck(false);
          return (Result) 1;
        }
      }
    }
    if (source1.Count<Element>() == num1)
    {
      bool flag2 = true;
      foreach (Element element in source1)
      {
        AssemblyInstance assInst = element as AssemblyInstance;
        if (new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (x => x.IsAssemblyView && x.AssociatedAssemblyInstanceId == assInst.Id)).Any<ViewSheet>())
        {
          flag2 = false;
          break;
        }
      }
      if (!flag2)
      {
        TaskDialogResult taskDialogResult = new TaskDialog("Hardware Detail")
        {
          MainInstruction = "Would you like to only update the title blocks of the existing hardware details associated with the assemblies selected instead of creating new hardware details?",
          CommonButtons = ((TaskDialogCommonButtons) 6)
        }.Show();
        if (taskDialogResult == 6)
          flag1 = false;
        else if (taskDialogResult != 7)
        {
          App.hardwareDetailErrorCheck(false);
          if (revitDoc.IsWorkshared && elementIds.Count > 0)
          {
            RelinquishOptions generalCategories = new RelinquishOptions(false);
            generalCategories.CheckedOutElements = true;
            TransactWithCentralOptions options = new TransactWithCentralOptions();
            WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
          }
          return (Result) 1;
        }
      }
    }
    ViewSheet viewSheet1 = (ViewSheet) null;
    Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
    if (flag1)
    {
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      List<string> selected = new List<string>();
      foreach (Element elem in source1)
      {
        if (elem is AssemblyInstance)
        {
          selected.Add(elem.Name);
        }
        else
        {
          string upper = Parameters.GetParameterAsString(elem, "CONTROL_MARK").ToUpper();
          if (upper != null && !string.IsNullOrWhiteSpace(upper))
            selected.Add(upper);
        }
      }
      List<AssemblyInstance> me = new List<AssemblyInstance>();
      Level level1 = (Level) null;
      List<Level> list4 = new FilteredElementCollector(revitDoc).OfClass(typeof (Level)).Cast<Level>().ToList<Level>();
      double num3 = list4[0].Elevation;
      foreach (Level level2 in list4)
      {
        double elevation = level2.Elevation;
        if (elevation < num3)
        {
          num3 = elevation;
          level1 = level2;
        }
      }
      List<AssemblyInstance> list5 = new FilteredElementCollector(revitDoc).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
      using (TransactionGroup transactionGroup = new TransactionGroup(revitDoc, "Hardware Detail"))
      {
        int num4 = (int) transactionGroup.Start();
        Dictionary<string, AssemblyInstance> dictionary3 = new Dictionary<string, AssemblyInstance>();
        Dictionary<AssemblyInstance, Element> dictionary4 = new Dictionary<AssemblyInstance, Element>();
        List<AssemblyInstance> source2 = new List<AssemblyInstance>();
        List<Autodesk.Revit.DB.View> source3 = new List<Autodesk.Revit.DB.View>();
        using (Transaction transaction = new Transaction(revitDoc))
        {
          bool flag3 = true;
          foreach (Element elem in source1)
          {
            int num5 = (int) transaction.Start("Create Assembly");
            if (elem is AssemblyInstance)
            {
              AssemblyInstance key = elem as AssemblyInstance;
              List<AssemblyInstance> assemblyInstanceList = new List<AssemblyInstance>();
              foreach (AssemblyInstance assemblyInstance in list5)
              {
                if (assemblyInstance.AssemblyTypeName.ToUpper().Equals(key.Name.ToUpper()))
                  assemblyInstanceList.Add(assemblyInstance);
              }
              List<Autodesk.Revit.DB.View> viewList = new List<Autodesk.Revit.DB.View>();
              foreach (AssemblyInstance assemblyInstance in assemblyInstanceList)
              {
                AssemblyInstance assembInst = assemblyInstance;
                viewList.AddRange((IEnumerable<Autodesk.Revit.DB.View>) new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (x => x.AssociatedAssemblyInstanceId == assembInst.Id)).ToList<Autodesk.Revit.DB.View>());
              }
              if (revitDoc.IsWorkshared)
              {
                foreach (Element element in viewList)
                {
                  List<ElementId> elemsUnowned;
                  List<Element> ownedByOtherUser;
                  this.GetCheckOutStatus(revitDoc, element, out elemsUnowned, out ownedByOtherUser);
                  elemsUnownedWORKSHARED.AddRange((IEnumerable<ElementId>) elemsUnowned);
                  elemsOwnedByOtherUser.AddRange((IEnumerable<Element>) ownedByOtherUser);
                }
                elemsUnownedWORKSHARED = elemsUnownedWORKSHARED.Distinct<ElementId>().ToList<ElementId>();
                elemsOwnedByOtherUser = elemsOwnedByOtherUser.Distinct<Element>().ToList<Element>();
                if (elemsOwnedByOtherUser.Count > 0)
                {
                  TaskDialog taskDialog6 = new TaskDialog("Warning");
                  taskDialog6.MainContent = "One or more elements that are needed for the Hardware Detail tool are currently owned by another user. Please have them relinquish them and run the Hardware Detail tool again. Expand for details.";
                  foreach (Element element in elemsOwnedByOtherUser)
                  {
                    TaskDialog taskDialog7 = taskDialog6;
                    taskDialog7.ExpandedContent = $"{taskDialog7.ExpandedContent}{element.Name} : {element.Id.ToString()}\n";
                  }
                  taskDialog6.Show();
                  App.hardwareDetailErrorCheck(false);
                  if (transaction.GetStatus() == TransactionStatus.Started)
                  {
                    int num6 = (int) transaction.RollBack();
                  }
                  if (transactionGroup.GetStatus() == TransactionStatus.Started)
                  {
                    int num7 = (int) transactionGroup.RollBack();
                  }
                  if (elementIds.Count > 0)
                  {
                    RelinquishOptions generalCategories = new RelinquishOptions(false);
                    generalCategories.CheckedOutElements = true;
                    TransactWithCentralOptions options = new TransactWithCentralOptions();
                    WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                  }
                  return (Result) 1;
                }
                if (elemsUnownedWORKSHARED.Count > 0)
                {
                  try
                  {
                    elementIds = WorksharingUtils.CheckoutElements(revitDoc, (ICollection<ElementId>) elemsUnownedWORKSHARED);
                  }
                  catch (Exception ex)
                  {
                    new TaskDialog("Warning")
                    {
                      MainContent = "Unable to taake ownership of needed elements to run the Hardware Detail tool."
                    }.Show();
                    App.hardwareDetailErrorCheck(false);
                    return (Result) 1;
                  }
                }
              }
              if (viewList.Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (x => x.Id)).Contains<ElementId>(activeUiDocument.ActiveView.Id))
              {
                new TaskDialog("Hardware Detail")
                {
                  MainContent = "Hardware detail cannot be used within an active view associated with the assembly selected. Please switch views and re-run the tool."
                }.Show();
                int num8 = (int) transaction.RollBack();
                int num9 = (int) transactionGroup.RollBack();
                App.hardwareDetailErrorCheck(false);
                if (revitDoc.IsWorkshared && elementIds.Count > 0)
                {
                  RelinquishOptions generalCategories = new RelinquishOptions(false);
                  generalCategories.CheckedOutElements = true;
                  TransactWithCentralOptions options = new TransactWithCentralOptions();
                  WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                }
                return (Result) 1;
              }
              source3.AddRange((IEnumerable<Autodesk.Revit.DB.View>) viewList);
              bool flag4 = false;
              string upper = key.Name.Replace("_HWD-", "").ToUpper();
              foreach (ElementId memberId in (IEnumerable<ElementId>) key.GetMemberIds())
              {
                Element element = revitDoc.GetElement(memberId);
                if (element != null && Parameters.GetParameterAsString(element, "CONTROL_MARK").ToUpper() == upper)
                {
                  dictionary4.Add(key, element);
                  flag4 = true;
                  break;
                }
              }
              if (flag4)
                me.Add(elem as AssemblyInstance);
            }
            else
            {
              Parameter parameter1 = Parameters.LookupParameter(elem, "CONTROL_MARK");
              if (parameter1 != null)
              {
                string str = parameter1.AsString();
                List<AssemblyInstance> source4 = new List<AssemblyInstance>();
                if (!string.IsNullOrWhiteSpace(str))
                {
                  foreach (AssemblyInstance assemblyInstance in list5)
                  {
                    if (assemblyInstance.AssemblyTypeName.ToUpper().Equals("_HWD-" + str.ToUpper()))
                      source4.Add(assemblyInstance);
                  }
                  if (source4.Count<AssemblyInstance>() > 0)
                  {
                    if (flag3)
                    {
                      if (new TaskDialog("Hardware Detail")
                      {
                        MainContent = "One or more of the elements you selected already has a hardware detail created with its control mark. Would you like to continue? This will delete the existing hardware details and assemblies associated with it.",
                        CommonButtons = ((TaskDialogCommonButtons) 6)
                      }.Show() != 6)
                      {
                        int num10 = (int) transaction.RollBack();
                        int num11 = (int) transactionGroup.RollBack();
                        App.hardwareDetailErrorCheck(false);
                        if (revitDoc.IsWorkshared && elementIds.Count > 0)
                        {
                          RelinquishOptions generalCategories = new RelinquishOptions(false);
                          generalCategories.CheckedOutElements = true;
                          TransactWithCentralOptions options = new TransactWithCentralOptions();
                          WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                        }
                        return (Result) 1;
                      }
                      flag3 = false;
                    }
                    AssemblyInstance key = source4.FirstOrDefault<AssemblyInstance>();
                    List<Autodesk.Revit.DB.View> collection = new List<Autodesk.Revit.DB.View>();
                    foreach (AssemblyInstance assemblyInstance in source4)
                    {
                      AssemblyInstance assembInst = assemblyInstance;
                      collection.AddRange((IEnumerable<Autodesk.Revit.DB.View>) new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (x => x.AssociatedAssemblyInstanceId == assembInst.Id)).ToList<Autodesk.Revit.DB.View>());
                    }
                    if (revitDoc.IsWorkshared)
                    {
                      foreach (AssemblyInstance assembInst in source4)
                      {
                        List<ElementId> elemsUnowned;
                        List<Element> ownedByOtherUser;
                        this.GetCheckOutStatus(revitDoc, assembInst, out elemsUnowned, out ownedByOtherUser);
                        elemsUnownedWORKSHARED.AddRange(elemsUnowned.Where<ElementId>((Func<ElementId, bool>) (x => !elemsUnownedWORKSHARED.Contains(x))));
                        elemsOwnedByOtherUser.AddRange(ownedByOtherUser.Where<Element>((Func<Element, bool>) (x => !elemsOwnedByOtherUser.Contains(x))));
                      }
                      foreach (Element element in collection)
                      {
                        List<ElementId> elemsUnowned;
                        List<Element> ownedByOtherUser;
                        this.GetCheckOutStatus(revitDoc, element, out elemsUnowned, out ownedByOtherUser);
                        elemsUnownedWORKSHARED.AddRange((IEnumerable<ElementId>) elemsUnowned);
                        elemsOwnedByOtherUser.AddRange((IEnumerable<Element>) ownedByOtherUser);
                      }
                      elemsUnownedWORKSHARED = elemsUnownedWORKSHARED.Distinct<ElementId>().ToList<ElementId>();
                      elemsOwnedByOtherUser = elemsOwnedByOtherUser.Distinct<Element>().ToList<Element>();
                      if (elemsOwnedByOtherUser.Count > 0)
                      {
                        TaskDialog taskDialog8 = new TaskDialog("Warning");
                        taskDialog8.MainContent = "One or more elements that are needed for the Hardware Detail tool are currently owned by another user. Please have them relinquish them and run the Hardware Detail tool again. Expand for details.";
                        foreach (Element element in elemsOwnedByOtherUser)
                        {
                          TaskDialog taskDialog9 = taskDialog8;
                          taskDialog9.ExpandedContent = $"{taskDialog9.ExpandedContent}{element.Name} : {element.Id.ToString()}\n";
                        }
                        taskDialog8.Show();
                        App.hardwareDetailErrorCheck(false);
                        if (transaction.GetStatus() == TransactionStatus.Started)
                        {
                          int num12 = (int) transaction.RollBack();
                        }
                        if (transactionGroup.GetStatus() == TransactionStatus.Started)
                        {
                          int num13 = (int) transactionGroup.RollBack();
                        }
                        if (elementIds.Count > 0)
                        {
                          RelinquishOptions generalCategories = new RelinquishOptions(false);
                          generalCategories.CheckedOutElements = true;
                          TransactWithCentralOptions options = new TransactWithCentralOptions();
                          WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                        }
                        return (Result) 1;
                      }
                      if (elemsUnownedWORKSHARED.Count > 0)
                      {
                        try
                        {
                          elementIds = WorksharingUtils.CheckoutElements(revitDoc, (ICollection<ElementId>) elemsUnownedWORKSHARED);
                        }
                        catch (Exception ex)
                        {
                          new TaskDialog("Warning")
                          {
                            MainContent = "Unable to take ownership of needed elements to run the Hardware Detail tool."
                          }.Show();
                          App.hardwareDetailErrorCheck(false);
                          return (Result) 1;
                        }
                      }
                    }
                    source3.AddRange((IEnumerable<Autodesk.Revit.DB.View>) collection);
                    List<ElementId> elementIdList = new List<ElementId>();
                    foreach (AssemblyInstance assemblyInstance in source4)
                      elementIdList.AddRange((IEnumerable<ElementId>) key.GetMemberIds().ToList<ElementId>());
                    if (elementIdList.Contains(elem.Id))
                    {
                      me.Add(key);
                      dictionary4.Add(key, elem);
                      continue;
                    }
                    foreach (AssemblyInstance assemblyInstance in source4)
                    {
                      list5.Remove(assemblyInstance);
                      source2.Add(assemblyInstance);
                    }
                    revitDoc.Regenerate();
                  }
                }
              }
              Level level3 = (Level) null;
              XYZ point1 = (elem.Location as LocationPoint).Point;
              if (elem.HasSuperComponent())
              {
                Element superComponent = elem.GetSuperComponent();
                for (bool flag5 = superComponent.HasSuperComponent(); flag5; flag5 = superComponent.HasSuperComponent())
                  superComponent = superComponent.GetSuperComponent();
                Parameter parameter2 = superComponent.LookupParameter("Schedule Level");
                if (parameter2 != null)
                {
                  ElementId id = parameter2.AsElementId();
                  level3 = revitDoc.GetElement(id) as Level;
                }
                else
                {
                  Parameter parameter3 = superComponent.LookupParameter("Level");
                  if (parameter3 != null)
                  {
                    ElementId id = parameter3.AsElementId();
                    level3 = revitDoc.GetElement(id) as Level;
                  }
                }
              }
              else
              {
                Parameter parameter4 = elem.LookupParameter("Schedule Level");
                if (parameter4 != null)
                {
                  ElementId id = parameter4.AsElementId();
                  level3 = revitDoc.GetElement(id) as Level;
                }
                else
                {
                  Parameter parameter5 = elem.LookupParameter("Level");
                  if (parameter5 != null)
                  {
                    ElementId id = parameter5.AsElementId();
                    level3 = revitDoc.GetElement(id) as Level;
                  }
                }
              }
              FamilyInstance familyInstance1 = elem as FamilyInstance;
              if (level3 == null)
                level3 = level1 ?? list4.FirstOrDefault<Level>();
              FamilyInstance familyInstance2 = ((ItemFactoryBase) revitDoc.Create).NewFamilyInstance(point1, familyInstance1.Symbol, level3, StructuralType.NonStructural);
              if (familyInstance2 == null)
              {
                int num14 = (int) MessageBox.Show("Failed to create hardware details.");
                int num15 = (int) transaction.RollBack();
                int num16 = (int) transactionGroup.RollBack();
                App.hardwareDetailErrorCheck(false);
                if (revitDoc.IsWorkshared && elementIds.Count > 0)
                {
                  RelinquishOptions generalCategories = new RelinquishOptions(false);
                  generalCategories.CheckedOutElements = true;
                  TransactWithCentralOptions options = new TransactWithCentralOptions();
                  WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                }
                return (Result) -1;
              }
              foreach (Parameter parameter6 in familyInstance1.Parameters)
              {
                if (parameter6.Definition.Name != "CONTROL_MARK")
                {
                  Parameter parameter7 = familyInstance2.LookupParameter(parameter6.Definition.Name);
                  if (parameter7 != null && !parameter7.IsReadOnly)
                  {
                    switch (parameter6.StorageType)
                    {
                      case StorageType.Integer:
                        parameter7.Set(parameter6.AsInteger());
                        continue;
                      case StorageType.Double:
                        parameter7.Set(parameter6.AsDouble());
                        continue;
                      case StorageType.String:
                        parameter7.Set(parameter6.AsString());
                        continue;
                      case StorageType.ElementId:
                        parameter7.Set(parameter6.AsElementId());
                        continue;
                      default:
                        continue;
                    }
                  }
                }
              }
              this.setHardwareDetailParameters((Element) familyInstance2);
              Parameters.ClearHostingParameters((Element) familyInstance2);
              Parameter parameter8 = elem.LookupParameter("CONTROL_MARK");
              Parameter parameter9 = familyInstance2.LookupParameter("CONTROL_MARK");
              if (parameter9 != null && parameter8 != null && !parameter9.IsReadOnly)
                parameter9.Set(parameter8.AsString());
              List<ElementId> assemblyMemberIds = new List<ElementId>()
              {
                familyInstance2.Id
              };
              assemblyMemberIds.AddRange((IEnumerable<ElementId>) this.updateSubComponents(revitDoc, familyInstance2));
              revitDoc.Regenerate();
              XYZ point2 = (familyInstance2.Location as LocationPoint).Point;
              double num17 = 1000.0;
              if (point1.Z < num3)
                num17 -= num3 - point2.Z;
              else if (point1.Z > num3)
                num17 += point2.Z - num3;
              ElementTransformUtils.MoveElement(revitDoc, familyInstance2.Id, new XYZ(0.0, 0.0, -num17));
              ElementId id1 = elem.Category.Id;
              AssemblyInstance key1 = AssemblyInstance.Create(revitDoc, (ICollection<ElementId>) assemblyMemberIds, id1);
              XYZ point3 = (familyInstance2.Location as LocationPoint).Point;
              Transform identity = Transform.Identity;
              if (point3 != null)
                identity.Origin = point3;
              key1.SetTransform(identity);
              string str1 = familyInstance2.Name;
              if (parameter1 != null)
                str1 = parameter1.AsString();
              dictionary3.Add("_HWD-" + str1, key1);
              dictionary4.Add(key1, elem);
            }
            if (transaction.Commit() != TransactionStatus.Committed)
            {
              int num18 = (int) transactionGroup.RollBack();
              App.hardwareDetailErrorCheck(false);
              if (revitDoc.IsWorkshared && elementIds.Count > 0)
              {
                RelinquishOptions generalCategories = new RelinquishOptions(false);
                generalCategories.CheckedOutElements = true;
                TransactWithCentralOptions options = new TransactWithCentralOptions();
                WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
              }
              return (Result) 1;
            }
          }
          if (transaction.HasStarted())
          {
            if (!transaction.HasEnded())
            {
              if (transaction.Commit() != TransactionStatus.Committed)
              {
                int num19 = (int) transactionGroup.RollBack();
                App.hardwareDetailErrorCheck(false);
                if (revitDoc.IsWorkshared && elementIds.Count > 0)
                {
                  RelinquishOptions generalCategories = new RelinquishOptions(false);
                  generalCategories.CheckedOutElements = true;
                  TransactWithCentralOptions options = new TransactWithCentralOptions();
                  WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                }
                return (Result) 1;
              }
            }
          }
        }
        List<FamilySymbol> list6 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).Cast<FamilySymbol>().ToList<FamilySymbol>();
        if (list6 == null || list6.Count == 0)
        {
          int num20 = (int) MessageBox.Show("Unable to find title blocks in the project. Please add a titleblock to the project and try again.");
          int num21 = (int) transactionGroup.RollBack();
          App.hardwareDetailErrorCheck(false);
          if (revitDoc.IsWorkshared && elementIds.Count > 0)
          {
            RelinquishOptions generalCategories = new RelinquishOptions(false);
            generalCategories.CheckedOutElements = true;
            TransactWithCentralOptions options = new TransactWithCentralOptions();
            WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
          }
          return (Result) -1;
        }
        HardwareDetailWindow hardwareDetailWindow = new HardwareDetailWindow(revitDoc, mainWindowHandle, selected);
        if (!hardwareDetailWindow.canceled)
          hardwareDetailWindow.ShowDialog();
        if (hardwareDetailWindow.canceled)
        {
          int num22 = (int) transactionGroup.RollBack();
          if (revitDoc.IsWorkshared && elementIds.Count > 0)
          {
            RelinquishOptions generalCategories = new RelinquishOptions(false);
            generalCategories.CheckedOutElements = true;
            TransactWithCentralOptions options = new TransactWithCentralOptions();
            WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
          }
          if (hardwareDetailWindow.error)
          {
            int num23 = (int) MessageBox.Show("Unable to run hardware detail tool.");
            App.hardwareDetailErrorCheck(false);
            return (Result) -1;
          }
          App.hardwareDetailErrorCheck(false);
          return (Result) 1;
        }
        using (Transaction transaction = new Transaction(revitDoc))
        {
          int num24 = (int) transaction.Start("Delete Assmblies");
          List<string> stringList2 = new List<string>();
          foreach (KeyValuePair<string, AssemblyInstance> keyValuePair in dictionary3)
          {
            if (!keyValuePair.Value.IsValidObject)
              stringList2.Add(keyValuePair.Key);
          }
          foreach (KeyValuePair<string, AssemblyInstance> keyValuePair in dictionary3)
          {
            if (!stringList2.Contains(keyValuePair.Key))
            {
              AssemblyInstance elem = keyValuePair.Value;
              this.setHardwareDetailParameters((Element) elem);
              Parameters.ClearHostingParameters((Element) elem);
              me.Add(elem);
            }
          }
          List<AssemblyInstance> list7 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Assemblies).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
          List<AssemblyInstance> source5 = new List<AssemblyInstance>();
          foreach (AssemblyInstance instance1 in me)
          {
            foreach (AssemblyInstance instance2 in list7)
            {
              if (!source5.Contains(instance2) && instance1.Id != instance2.Id && !source2.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (x => x.Id)).Contains<ElementId>(instance2.Id) && AssemblyInstance.CompareAssemblyInstances(instance1, instance2) is AssemblyDifferenceNone)
                source5.Add(instance2);
            }
          }
          source5.Distinct<AssemblyInstance>();
          if (source5.Count<AssemblyInstance>() > 0)
          {
            TaskDialog taskDialog10 = new TaskDialog("Warning");
            taskDialog10.MainContent = "One or more existing assemblies match the hardware detail assemblies the tool has created. If the tool continues it will delete these assemblies.";
            taskDialog10.ExpandedContent = "Assembly IDs:\n";
            foreach (AssemblyInstance assemblyInstance in source5)
            {
              TaskDialog taskDialog11 = taskDialog10;
              taskDialog11.ExpandedContent = $"{taskDialog11.ExpandedContent}{assemblyInstance.Id?.ToString()}\n";
            }
            taskDialog10.CommonButtons = (TaskDialogCommonButtons) 9;
            if (taskDialog10.Show() == 2)
            {
              int num25 = (int) transaction.RollBack();
              int num26 = (int) transactionGroup.RollBack();
              if (revitDoc.IsWorkshared && elementIds.Count > 0)
              {
                RelinquishOptions generalCategories = new RelinquishOptions(false);
                generalCategories.CheckedOutElements = true;
                TransactWithCentralOptions options = new TransactWithCentralOptions();
                WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
              }
              App.hardwareDetailErrorCheck(false);
              return (Result) 1;
            }
            List<Autodesk.Revit.DB.View> source6 = new List<Autodesk.Revit.DB.View>();
            elemsUnownedWORKSHARED.Clear();
            elemsOwnedByOtherUser.Clear();
            foreach (AssemblyInstance assemblyInstance in source5)
            {
              AssemblyInstance assembInst = assemblyInstance;
              List<ElementId> elemsUnowned;
              List<Element> ownedByOtherUser;
              if (revitDoc.IsWorkshared)
              {
                this.GetCheckOutStatus(revitDoc, assembInst, out elemsUnowned, out ownedByOtherUser);
                elemsUnownedWORKSHARED.AddRange((IEnumerable<ElementId>) elemsUnowned);
                elemsOwnedByOtherUser.AddRange((IEnumerable<Element>) ownedByOtherUser);
              }
              source6.AddRange((IEnumerable<Autodesk.Revit.DB.View>) new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (x => x.AssociatedAssemblyInstanceId == assembInst.Id)).ToList<Autodesk.Revit.DB.View>());
              if (revitDoc.IsWorkshared)
              {
                foreach (Element element in source6)
                {
                  elemsUnowned = new List<ElementId>();
                  ownedByOtherUser = new List<Element>();
                  this.GetCheckOutStatus(revitDoc, element, out elemsUnowned, out ownedByOtherUser);
                  elemsUnownedWORKSHARED.AddRange((IEnumerable<ElementId>) elemsUnowned);
                  elemsOwnedByOtherUser.AddRange((IEnumerable<Element>) ownedByOtherUser);
                }
              }
            }
            if (revitDoc.IsWorkshared)
            {
              if (elemsOwnedByOtherUser.Count > 0)
              {
                TaskDialog taskDialog12 = new TaskDialog("Warning");
                taskDialog12.MainContent = "One or more elements that are needed for the Hardware Detail tool are currently owned by another user. Please have them relinquish them and run the Hardware Detail tool again. Expand for details.";
                foreach (Element element in elemsOwnedByOtherUser)
                {
                  TaskDialog taskDialog13 = taskDialog12;
                  taskDialog13.ExpandedContent = $"{taskDialog13.ExpandedContent}{element.Name} : {element.Id.ToString()}\n";
                }
                taskDialog12.Show();
                App.hardwareDetailErrorCheck(false);
                int num27 = (int) transaction.RollBack();
                int num28 = (int) transactionGroup.RollBack();
                if (elementIds.Count > 0)
                {
                  RelinquishOptions generalCategories = new RelinquishOptions(false);
                  generalCategories.CheckedOutElements = true;
                  TransactWithCentralOptions options = new TransactWithCentralOptions();
                  WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                }
                return (Result) 1;
              }
              if (elemsUnownedWORKSHARED.Count > 0)
              {
                try
                {
                  elementIds = WorksharingUtils.CheckoutElements(revitDoc, (ICollection<ElementId>) elemsUnownedWORKSHARED);
                }
                catch (Exception ex)
                {
                  TaskDialog taskDialog14 = new TaskDialog("Warning");
                  taskDialog14.MainContent = "Unable to take ownership of needed elements to run the Hardware Detail tool.";
                  int num29 = (int) transaction.RollBack();
                  taskDialog14.Show();
                  int num30 = (int) transactionGroup.RollBack();
                  if (elementIds.Count > 0)
                  {
                    RelinquishOptions generalCategories = new RelinquishOptions(false);
                    generalCategories.CheckedOutElements = true;
                    TransactWithCentralOptions options = new TransactWithCentralOptions();
                    WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                  }
                  App.hardwareDetailErrorCheck(false);
                  return (Result) 1;
                }
              }
            }
            revitDoc.Delete((ICollection<ElementId>) source6.Distinct<Autodesk.Revit.DB.View>().Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (x => x.Id)).ToList<ElementId>());
            revitDoc.Delete((ICollection<ElementId>) source5.Distinct<AssemblyInstance>().Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (x => x.Id)).ToList<ElementId>());
            revitDoc.Regenerate();
          }
          if (source3.Count<Autodesk.Revit.DB.View>() > 0 || source2.Count<AssemblyInstance>() > 0)
          {
            revitDoc.Delete((ICollection<ElementId>) source3.Select<Autodesk.Revit.DB.View, ElementId>((Func<Autodesk.Revit.DB.View, ElementId>) (x => x.Id)).ToList<ElementId>());
            revitDoc.Delete((ICollection<ElementId>) source2.Select<AssemblyInstance, ElementId>((Func<AssemblyInstance, ElementId>) (x => x.Id)).ToList<ElementId>());
          }
          if (transaction.Commit() != TransactionStatus.Committed)
          {
            int num31 = (int) transactionGroup.RollBack();
            App.hardwareDetailErrorCheck(false);
            if (revitDoc.IsWorkshared && elementIds.Count > 0)
            {
              RelinquishOptions generalCategories = new RelinquishOptions(false);
              generalCategories.CheckedOutElements = true;
              TransactWithCentralOptions options = new TransactWithCentralOptions();
              WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
            }
            return (Result) 1;
          }
        }
        using (Transaction transaction = new Transaction(revitDoc))
        {
          int num32 = (int) transaction.Start("Create Hardware Detail");
          foreach (KeyValuePair<string, AssemblyInstance> keyValuePair in dictionary3)
          {
            if (me.Contains(keyValuePair.Value))
              keyValuePair.Value.AssemblyTypeName = keyValuePair.Key;
          }
          List<AssemblyInstance> list8 = me.DistinctBy<AssemblyInstance, string>((Func<AssemblyInstance, string>) (x => x.AssemblyTypeName)).ToList<AssemblyInstance>();
          double num33 = 1.0 / 72.0;
          CalloutStyle calloutStyle = (CalloutStyle) null;
          LocationInFormAnalyzer.extractLocationInFormValues(revitDoc);
          foreach (AutoTicketSettingsTools autoTicketSetting in AutoTicketSettingsReader.ReaderforAutoTicketSettings(revitDoc, "HARDWARE DETAIL", false, out List<AutoTicketAppendStringParameterData> _))
          {
            if (autoTicketSetting is AutoTicketCalloutAndDimensionTexts)
            {
              AutoTicketCalloutAndDimensionTexts andDimensionTexts = autoTicketSetting as AutoTicketCalloutAndDimensionTexts;
              calloutStyle = new CalloutStyle(revitDoc, AutoTicketEnhancementWorkflow.AutoTicketGeneration, andDimensionTexts.CalloutFamily, andDimensionTexts.OverallDimension, andDimensionTexts.GeneralDimension, andDimensionTexts.TextStyle);
            }
          }
          if (list8.Count != source1.Count)
          {
            TaskDialog taskDialog = new TaskDialog("Hardware Detail");
            if (list8.Count == 0)
            {
              taskDialog.MainContent = "Unable to create hardware details for the selected elements.";
              taskDialog.Show();
              int num34 = (int) transaction.RollBack();
              int num35 = (int) transactionGroup.RollBack();
              if (revitDoc.IsWorkshared && elementIds.Count > 0)
              {
                RelinquishOptions generalCategories = new RelinquishOptions(false);
                generalCategories.CheckedOutElements = true;
                TransactWithCentralOptions options = new TransactWithCentralOptions();
                WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
              }
              App.hardwareDetailErrorCheck(false);
              return (Result) 1;
            }
            taskDialog.MainContent = "Unable to create hardware detail for one or more selected elements. Do you wish to coninue?";
            taskDialog.CommonButtons = (TaskDialogCommonButtons) 6;
            if (taskDialog.Show() != 6)
            {
              int num36 = (int) transaction.RollBack();
              int num37 = (int) transactionGroup.RollBack();
              if (revitDoc.IsWorkshared && elementIds.Count > 0)
              {
                RelinquishOptions generalCategories = new RelinquishOptions(false);
                generalCategories.CheckedOutElements = true;
                TransactWithCentralOptions options = new TransactWithCentralOptions();
                WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
              }
              App.hardwareDetailErrorCheck(false);
              return (Result) 1;
            }
          }
          List<HWDetailTemplate> templates = hardwareDetailWindow.getTemplates(out string _);
          string titleBlock;
          hardwareDetailWindow.getTitleBlock(out titleBlock);
          int selectedScale = hardwareDetailWindow.getSelectedScale();
          string copyAnnotationAssembly = hardwareDetailWindow.getCopyString();
          AssemblyInstance copyAssemblyInstance = new FilteredElementCollector(revitDoc).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (x => x.Name == copyAnnotationAssembly)).FirstOrDefault<AssemblyInstance>();
          List<ViewSheet> source7 = new List<ViewSheet>();
          List<Autodesk.Revit.DB.View> source8 = new List<Autodesk.Revit.DB.View>();
          if (copyAssemblyInstance != null)
          {
            source7 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (x => x.AssociatedAssemblyInstanceId == copyAssemblyInstance.Id)).ToList<ViewSheet>();
            source8 = new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (x => x.AssociatedAssemblyInstanceId == copyAssemblyInstance.Id)).ToList<Autodesk.Revit.DB.View>();
          }
          List<string> collection1 = new List<string>();
          List<string> collection2 = new List<string>();
          foreach (AssemblyInstance assemblyInstance in list8)
          {
            AssemblyInstance assembly = assemblyInstance;
            List<ViewSheet> viewSheetList = new List<ViewSheet>();
            List<Autodesk.Revit.DB.View> viewList = new List<Autodesk.Revit.DB.View>();
            int num38 = 1;
            foreach (HWDetailTemplate hwDetailTemplate in templates)
            {
              int ScaleToUse = selectedScale == -1 ? hwDetailTemplate.TemplateScale : selectedScale;
              ViewSheet hardwareDetail = hwDetailTemplate.CreateHardwareDetail(revitDoc, assembly, titleBlock, num38++.ToString(), ScaleToUse, true);
              if (viewSheet1 == null)
                viewSheet1 = hardwareDetail;
              if (hardwareDetail == null)
              {
                int num39 = (int) MessageBox.Show("Failed to create a new sheet. Hardware detail failed.");
                int num40 = (int) transaction.RollBack();
                int num41 = (int) transactionGroup.RollBack();
                App.hardwareDetailErrorCheck(false);
                if (revitDoc.IsWorkshared && elementIds.Count > 0)
                {
                  RelinquishOptions generalCategories = new RelinquishOptions(false);
                  generalCategories.CheckedOutElements = true;
                  TransactWithCentralOptions options = new TransactWithCentralOptions();
                  WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                }
                return (Result) 1;
              }
              viewSheetList.Add(hardwareDetail);
              bool bWeightSet;
              this.hardwareDetailTitleBlockPopulation(revitDoc, hardwareDetail, dictionary4[assembly], assembly, out bWeightSet);
              if (!bWeightSet)
              {
                if (!dictionary2.ContainsKey(assembly.Name))
                  dictionary2.Add(assembly.Name, new List<string>()
                  {
                    "Sheet " + hardwareDetail.SheetNumber
                  });
                else
                  dictionary2[assembly.Name].Add("Sheet " + hardwareDetail.SheetNumber);
              }
              List<Autodesk.Revit.DB.View> list9 = hardwareDetail.GetAllPlacedViews().ToList<ElementId>().Select<ElementId, Autodesk.Revit.DB.View>((Func<ElementId, Autodesk.Revit.DB.View>) (x => revitDoc.GetElement(x) as Autodesk.Revit.DB.View)).Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (x => x.ViewType == ViewType.Detail)).ToList<Autodesk.Revit.DB.View>();
              viewList.AddRange((IEnumerable<Autodesk.Revit.DB.View>) list9);
              FilteredElementCollector source9 = new FilteredElementCollector(revitDoc);
              source9.OfCategory(BuiltInCategory.OST_MultiCategoryTags);
              source9.OfClass(typeof (FamilySymbol));
              source9.Count<Element>();
              FamilySymbol leftTagFamily = (FamilySymbol) null;
              FamilySymbol rightTagFamily = (FamilySymbol) null;
              FamilySymbol leftTagFamilyTYP = (FamilySymbol) null;
              FamilySymbol rightTagFamilyTYP = (FamilySymbol) null;
              foreach (FamilySymbol familySymbol in source9)
              {
                string str = calloutStyle != null ? calloutStyle.calloutStyle : "AUTO_TICKET_CALLOUT";
                if (familySymbol.FamilyName == str)
                {
                  if (familySymbol.Name == "Left Justified")
                    leftTagFamily = familySymbol;
                  else if (familySymbol.Name == "Left Justified TYP")
                    leftTagFamilyTYP = familySymbol;
                  else if (familySymbol.Name == "Right Justified")
                    rightTagFamily = familySymbol;
                  else if (familySymbol.Name == "Right Justified TYP")
                    rightTagFamilyTYP = familySymbol;
                }
              }
              if (leftTagFamilyTYP == null && leftTagFamily != null)
                leftTagFamilyTYP = leftTagFamily;
              if (rightTagFamilyTYP == null && rightTagFamily != null)
                rightTagFamilyTYP = rightTagFamily;
              foreach (Autodesk.Revit.DB.View view in list9)
              {
                Element selectedElement = dictionary4[assembly];
                List<ElementId> list10 = new FilteredElementCollector(revitDoc, view.Id).ToElementIds().ToList<ElementId>();
                List<Element> elementList = new List<Element>();
                foreach (ElementId id in list10)
                {
                  Element element = revitDoc.GetElement(id);
                  if (element != null && element.Name != selectedElement.Name && !(element is AssemblyInstance))
                    elementList.Add(element);
                }
                Dictionary<string, Dictionary<FormLocation, List<ElementId>>> elementDictionary = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
                foreach (Element elem in elementList)
                {
                  string str = elem.GetControlMark();
                  if (str == "" && elem is FamilyInstance familyInstance)
                    str = familyInstance.Symbol.FamilyName;
                  if (str != "")
                  {
                    if (!elementDictionary.Keys.Contains<string>(str.ToUpper()))
                    {
                      elementDictionary.Add(str.ToUpper(), new Dictionary<FormLocation, List<ElementId>>());
                      elementDictionary[str.ToUpper()].Add(FormLocation.None, new List<ElementId>());
                    }
                    elementDictionary[str.ToUpper()][FormLocation.None].Add(elem.Id);
                  }
                }
                new AutoTicketCalloutHandler(view, revitDoc, elementDictionary, true, selectedElement).processAllCallOuts(leftTagFamily, rightTagFamily, leftTagFamilyTYP, rightTagFamilyTYP, true);
              }
              new HardwareDetailDimension().createDimensionsOnViews(revitDoc, assembly, dictionary4[assembly], list9, out string _);
              IEnumerable<FamilyInstance> source10 = new FilteredElementCollector(revitDoc, hardwareDetail.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements().Cast<FamilyInstance>();
              XYZ point = XYZ.Zero;
              double num42 = 0.0;
              double y = 0.0;
              if (source10.Any<FamilyInstance>())
              {
                FamilyInstance familyInstance = source10.First<FamilyInstance>();
                point = (familyInstance.Location as LocationPoint).Point;
                try
                {
                  num42 = familyInstance.get_Parameter(BuiltInParameter.SHEET_WIDTH).AsDouble();
                  y = familyInstance.get_Parameter(BuiltInParameter.SHEET_HEIGHT).AsDouble();
                }
                catch
                {
                }
                if (PopulatorQA.inHouse)
                  TemplateDebugUtils.DebugDrawCircleAtPoint(point, revitDoc, (Autodesk.Revit.DB.View) hardwareDetail, "TitleBlock Location", false);
              }
              ScheduleSheetInstance scheduleSheetInstance1 = (ScheduleSheetInstance) null;
              XYZ xyz = point;
              List<string> BOMSettingsScheduleList = new List<string>();
              Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
              if (parameter != null)
                BOMSettingsScheduleList = this.GetHeaderScheduleNames(parameter.AsString());
              List<ElementId> elementIdList = new List<ElementId>();
              List<ViewSchedule> list11 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSchedule)).ToElements().Cast<ViewSchedule>().Where<ViewSchedule>((Func<ViewSchedule, bool>) (x => BOMSettingsScheduleList.Contains(x.Name))).ToList<ViewSchedule>();
              foreach (HardwareDetailScheduleInfo schedule in hwDetailTemplate.ScheduleList)
              {
                HardwareDetailScheduleInfo schedInfo = schedule;
                ScheduleSheetInstance scheduleSheetInstance2 = (ScheduleSheetInstance) null;
                if (schedInfo.isNoteSchedule)
                {
                  if (BOMSettingsScheduleList.Contains(schedInfo.BOMScheduleName))
                  {
                    ViewSchedule viewSchedule = list11.Where<ViewSchedule>((Func<ViewSchedule, bool>) (x => x.Name == schedInfo.BOMScheduleName)).FirstOrDefault<ViewSchedule>();
                    if (viewSchedule != null)
                    {
                      elementIdList.Add(viewSchedule.Id);
                      scheduleSheetInstance2 = ScheduleSheetInstance.Create(revitDoc, hardwareDetail.Id, viewSchedule.Id, XYZ.Zero);
                    }
                  }
                  if (scheduleSheetInstance2 == null && !collection1.Contains(schedInfo.BOMScheduleName))
                    collection1.Add(schedInfo.BOMScheduleName);
                }
                else
                {
                  Autodesk.Revit.DB.View elem = (Autodesk.Revit.DB.View) new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSchedule)).Cast<ViewSchedule>().Where<ViewSchedule>((Func<ViewSchedule, bool>) (v => v.Name == schedInfo.BOMScheduleName && v.AssociatedAssemblyInstanceId.Equals((object) assembly.Id))).FirstOrDefault<ViewSchedule>();
                  if (elem != null)
                  {
                    ElementId parameterAsElementId = Parameters.GetParameterAsElementId((Element) elem, "View Template");
                    if (parameterAsElementId != (ElementId) null && (!(revitDoc.GetElement(parameterAsElementId) is Autodesk.Revit.DB.View element) || element.Name != schedInfo.ViewTemplateName))
                      elem = (Autodesk.Revit.DB.View) null;
                  }
                  if (elem == null)
                  {
                    Autodesk.Revit.DB.View view = new FilteredElementCollector(revitDoc).OfClass(typeof (Autodesk.Revit.DB.View)).Cast<Autodesk.Revit.DB.View>().Where<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (v => v.IsTemplate && v.Name == schedInfo.ViewTemplateName)).FirstOrDefault<Autodesk.Revit.DB.View>();
                    if (view == null)
                    {
                      if (!collection2.Contains(schedInfo.BOMScheduleName))
                      {
                        collection2.Add(schedInfo.BOMScheduleName);
                        continue;
                      }
                      continue;
                    }
                    elem = (Autodesk.Revit.DB.View) AssemblyViewUtils.CreatePartList(revitDoc, assembly.Id);
                    if (elem == null)
                    {
                      if (!collection2.Contains(schedInfo.BOMScheduleName))
                      {
                        collection2.Add(schedInfo.BOMScheduleName);
                        continue;
                      }
                      continue;
                    }
                    bool flag6 = true;
                    string str = schedInfo.BOMScheduleName;
                    int num43 = 0;
                    while (flag6)
                    {
                      try
                      {
                        if (schedInfo.BOMScheduleName != "Error:NameStringNotSet")
                          elem.Name = str;
                      }
                      catch
                      {
                        ++num43;
                        str = $"{schedInfo.BOMScheduleName} {num43.ToString()}";
                      }
                      if (elem.Name == str)
                        flag6 = false;
                    }
                    if (view != null)
                      Parameters.LookupParameter((Element) elem, "View Template")?.Set(view.Id);
                    elem.LookupParameter("HARDWARE_DETAIL")?.Set(1);
                  }
                  elementIdList.Add(elem.Id);
                  scheduleSheetInstance2 = ScheduleSheetInstance.Create(revitDoc, hardwareDetail.Id, elem.Id, XYZ.Zero);
                }
                if (scheduleSheetInstance2 != null)
                {
                  BoundingBoxXYZ boundingBoxXyz1 = scheduleSheetInstance2.get_BoundingBox((Autodesk.Revit.DB.View) hardwareDetail);
                  if (hwDetailTemplate.BOMJustification == BOMJustification.Bottom)
                  {
                    scheduleSheetInstance2.Point += new XYZ(0.0, boundingBoxXyz1.Max.Y - boundingBoxXyz1.Min.Y - num33, 0.0);
                  }
                  else
                  {
                    int bomJustification = (int) hwDetailTemplate.BOMJustification;
                  }
                  if (hwDetailTemplate.bStackSchedules)
                  {
                    if (scheduleSheetInstance1 == null)
                    {
                      xyz = point + hwDetailTemplate.BOMAnchorPosition;
                    }
                    else
                    {
                      BoundingBoxXYZ boundingBoxXyz2 = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) hardwareDetail);
                      if (hwDetailTemplate.BOMJustification == BOMJustification.Bottom)
                        xyz += new XYZ(0.0, boundingBoxXyz2.Max.Y - boundingBoxXyz2.Min.Y - num33, 0.0);
                      else if (hwDetailTemplate.BOMJustification == BOMJustification.Top)
                        xyz -= new XYZ(0.0, boundingBoxXyz2.Max.Y - boundingBoxXyz2.Min.Y - num33, 0.0);
                    }
                    scheduleSheetInstance2.Point += xyz;
                  }
                  else
                    scheduleSheetInstance2.Point += schedInfo.vectorToAnchorPoint;
                  scheduleSheetInstance1 = scheduleSheetInstance2;
                }
              }
              if (hwDetailTemplate.ScheduleList.Count<HardwareDetailScheduleInfo>() != 0)
              {
                BoundingBoxXYZ boundingBoxXyz = (BoundingBoxXYZ) null;
                foreach (ViewSchedule viewSchedule in list11)
                {
                  if (!elementIdList.Contains(viewSchedule.Id))
                  {
                    ScheduleSheetInstance scheduleSheetInstance3 = ScheduleSheetInstance.Create(revitDoc, hardwareDetail.Id, viewSchedule.Id, XYZ.Zero);
                    if (boundingBoxXyz == null)
                      xyz = point + new XYZ(num42 * 1.1, y, 0.0);
                    else
                      xyz -= new XYZ(0.0, boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num33, 0.0);
                    scheduleSheetInstance3.Point += xyz;
                    boundingBoxXyz = scheduleSheetInstance3.get_BoundingBox((Autodesk.Revit.DB.View) hardwareDetail);
                  }
                }
              }
            }
            foreach (ViewSheet viewSheet2 in viewSheetList)
            {
              ViewSheet sheet = viewSheet2;
              ViewSheet source11 = source7.FirstOrDefault<ViewSheet>((Func<ViewSheet, bool>) (s => s.SheetNumber.Equals(sheet.SheetNumber)));
              if (source11 != null)
                EDGE.TicketTools.CopyTicketViews.CopyTicketViews.copyTicketViews(source11, sheet);
            }
            foreach (Autodesk.Revit.DB.View view1 in viewList)
            {
              Autodesk.Revit.DB.View createdView = view1;
              Autodesk.Revit.DB.View viewSheetCopyFrom = source8.FirstOrDefault<Autodesk.Revit.DB.View>((Func<Autodesk.Revit.DB.View, bool>) (view => view.Name.Equals(createdView.Name)));
              if (viewSheetCopyFrom != null)
                CopyTicketAnnotations.copyTicketAnnotations((Element) viewSheetCopyFrom, (Element) createdView);
            }
          }
          List<string> stringList3 = new List<string>();
          stringList3.AddRange((IEnumerable<string>) collection1);
          stringList3.AddRange((IEnumerable<string>) collection2);
          stringList3.Sort();
          if (stringList3.Count > 0)
          {
            TaskDialog taskDialog15 = new TaskDialog("Warning");
            taskDialog15.MainContent = "Unable to place some of the schedules in the template. Expand for details.";
            foreach (string str in stringList3)
            {
              TaskDialog taskDialog16 = taskDialog15;
              taskDialog16.ExpandedContent = $"{taskDialog16.ExpandedContent}{str}\n";
            }
            taskDialog15.Show();
          }
          if (transaction.Commit() != TransactionStatus.Committed)
          {
            int num44 = (int) transactionGroup.RollBack();
            App.hardwareDetailErrorCheck(false);
            if (revitDoc.IsWorkshared && elementIds.Count > 0)
            {
              RelinquishOptions generalCategories = new RelinquishOptions(false);
              generalCategories.CheckedOutElements = true;
              TransactWithCentralOptions options = new TransactWithCentralOptions();
              WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
            }
            return (Result) 1;
          }
        }
        int num45 = (int) transactionGroup.Assimilate();
      }
    }
    else
    {
      using (Transaction transaction = new Transaction(revitDoc, "Hardware Detail"))
      {
        int num46 = (int) transaction.Start();
        foreach (Element element1 in source1)
        {
          AssemblyInstance assemblyInstance = element1 as AssemblyInstance;
          List<ViewSheet> list12 = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (x => x.IsAssemblyView && x.AssociatedAssemblyInstanceId == assemblyInstance.Id)).ToList<ViewSheet>();
          if (revitDoc.IsWorkshared)
          {
            List<ElementId> elementsToCheckout = new List<ElementId>();
            List<Element> elementList = new List<Element>();
            foreach (ViewSheet viewSheet3 in list12)
            {
              switch (WorksharingUtils.GetCheckoutStatus(revitDoc, viewSheet3.Id))
              {
                case CheckoutStatus.OwnedByOtherUser:
                  elementList.Add((Element) viewSheet3);
                  continue;
                case CheckoutStatus.NotOwned:
                  elementsToCheckout.Add(viewSheet3.Id);
                  continue;
                default:
                  continue;
              }
            }
            if (elementList.Count > 0)
            {
              TaskDialog taskDialog17 = new TaskDialog("Warning");
              taskDialog17.MainContent = "One or more of the hardware details are currently owned by another user. Please have them relinquish them and run Hardware Detail tool again. Expand for details.";
              foreach (Element element2 in elementList)
              {
                TaskDialog taskDialog18 = taskDialog17;
                taskDialog18.ExpandedContent = $"{taskDialog18.ExpandedContent}{element2.Name} : {element2.Id.ToString()}";
              }
              taskDialog17.Show();
              int num47 = (int) transaction.RollBack();
              if (elementIds.Count > 0)
              {
                RelinquishOptions generalCategories = new RelinquishOptions(false);
                generalCategories.CheckedOutElements = true;
                TransactWithCentralOptions options = new TransactWithCentralOptions();
                WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
              }
              App.hardwareDetailErrorCheck(false);
              return (Result) 1;
            }
            if (elementsToCheckout.Count > 0)
            {
              try
              {
                WorksharingUtils.CheckoutElements(revitDoc, (ICollection<ElementId>) elementsToCheckout);
              }
              catch (Exception ex)
              {
                new TaskDialog("Warning")
                {
                  MainContent = "Unable to take ownership of needed elements to run the Hardware Details tool."
                }.Show();
                int num48 = (int) transaction.RollBack();
                if (elementIds.Count > 0)
                {
                  RelinquishOptions generalCategories = new RelinquishOptions(false);
                  generalCategories.CheckedOutElements = true;
                  TransactWithCentralOptions options = new TransactWithCentralOptions();
                  WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
                }
                App.hardwareDetailErrorCheck(false);
                return (Result) 1;
              }
            }
          }
          string str = assemblyInstance.Name.Replace("_HWD-", "");
          List<ElementId> list13 = assemblyInstance.GetMemberIds().ToList<ElementId>();
          Element element3 = (Element) null;
          foreach (ElementId id in list13)
          {
            Element element4 = revitDoc.GetElement(id);
            if (Parameters.GetParameterAsString(element4, "CONTROL_MARK") == str)
            {
              if (!element4.HasSuperComponent())
                element3 = element4;
              else if (element3 == null)
                element3 = element4;
            }
          }
          if (element3 != null)
          {
            foreach (ViewSheet viewSheet4 in list12)
            {
              bool bWeightSet;
              this.hardwareDetailTitleBlockPopulation(revitDoc, viewSheet4, element3, assemblyInstance, out bWeightSet);
              if (!bWeightSet)
              {
                if (!dictionary2.ContainsKey(assemblyInstance.Name))
                  dictionary2.Add(assemblyInstance.Name, new List<string>()
                  {
                    "Sheet " + viewSheet4.SheetNumber
                  });
                else
                  dictionary2[assemblyInstance.Name].Add("Sheet " + viewSheet4.SheetNumber);
              }
              if (viewSheet1 == null)
                viewSheet1 = viewSheet4;
            }
          }
        }
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          App.hardwareDetailErrorCheck(false);
          int num49 = (int) transaction.RollBack();
          if (revitDoc.IsWorkshared && elementIds.Count > 0)
          {
            RelinquishOptions generalCategories = new RelinquishOptions(false);
            generalCategories.CheckedOutElements = true;
            TransactWithCentralOptions options = new TransactWithCentralOptions();
            WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
          }
          App.hardwareDetailErrorCheck(false);
          return (Result) 1;
        }
      }
    }
    if (viewSheet1 != null && viewSheet1.IsValidObject)
      application.ActiveUIDocument.RequestViewChange((Autodesk.Revit.DB.View) viewSheet1);
    if (dictionary2.Keys.Count > 0)
    {
      TaskDialog taskDialog19 = new TaskDialog("Hardware Details");
      taskDialog19.MainInstruction = "One or more sheets did not have its \"TKT_HW_WEIGHT\" field populated.";
      taskDialog19.MainContent = "This usually results from improperly set up families being used with the tool. Please consult the help file for information on how to configure this option. Expand for details on which elements had the issue.";
      taskDialog19.ExpandedContent = "The following sheets did not have their TKT_HW_WEIGHT set:" + Environment.NewLine;
      foreach (string key in dictionary2.Keys)
      {
        foreach (string str in dictionary2[key])
        {
          TaskDialog taskDialog20 = taskDialog19;
          taskDialog20.ExpandedContent = $"{taskDialog20.ExpandedContent}{key}: {str}{Environment.NewLine}";
        }
      }
      taskDialog19.CommonButtons = (TaskDialogCommonButtons) 1;
      taskDialog19.Show();
    }
    int num50 = (int) MessageBox.Show("Hardware details successfully " + (flag1 ? "created." : "updated."));
    App.hardwareDetailErrorCheck(false);
    return (Result) 0;
  }

  private static bool? getHardwareDatailParameterValue(Element elem)
  {
    Parameter parameter = Parameters.LookupParameter(elem, "HARDWARE_DETAIL");
    if (parameter == null)
      return new bool?();
    int num = -1;
    if (parameter != null && parameter.HasValue)
      num = parameter.AsInteger();
    return num >= 1 ? new bool?(true) : new bool?(false);
  }

  public static bool? checkHardwareDetailStatus(
    Element elem,
    out string reason,
    out string controlMark)
  {
    reason = string.Empty;
    controlMark = string.Empty;
    string parameterAsString = Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT");
    if (parameterAsString.Contains("RAW CONSUMABLE") || parameterAsString.Contains("VOID") || parameterAsString.Contains("MESH") || parameterAsString.Contains("WWF") || parameterAsString.Contains("SHEARGRID") || parameterAsString.Contains("CGRID") || parameterAsString.Contains("MESH_SHEET"))
    {
      reason = "ELEMENT TYPE";
      return new bool?(false);
    }
    if ((elem as FamilyInstance).Symbol.FamilyName == "CONNECTOR_COMPONENT")
    {
      reason = "ELEMENT TYPE";
      return new bool?(false);
    }
    if (Parameters.LookupParameter(elem, "HARDWARE_DETAIL") == null)
    {
      reason = "HARDWARE DETAIL PARAMETER";
      return new bool?(false);
    }
    Parameter parameter = Parameters.LookupParameter(elem, "CONTROL_MARK");
    if (parameter == null)
    {
      reason = "CONTROL MARK";
      return new bool?();
    }
    if (parameter.HasValue)
    {
      string str = parameter.AsString();
      if (string.IsNullOrWhiteSpace(str))
      {
        reason = "CONTROL MARK";
        return new bool?();
      }
      controlMark = str.ToUpper();
      return new bool?(true);
    }
    reason = "CONTROL MARK";
    return new bool?(false);
  }

  public static bool? checkHardwareDetailStatus(
    AssemblyInstance assemblyInstance,
    out string reason,
    out string controlMark)
  {
    reason = string.Empty;
    controlMark = assemblyInstance.Name.Replace("_HWD-", "");
    bool? datailParameterValue = EDGE.TicketTools.HardwareDetail.HardwareDetail.getHardwareDatailParameterValue((Element) assemblyInstance);
    bool flag = false;
    foreach (ElementId id in assemblyInstance.GetMemberIds().ToList<ElementId>())
    {
      Element element = assemblyInstance.Document.GetElement(id);
      if (element != null)
      {
        string parameterAsString = Parameters.GetParameterAsString(element, "CONTROL_MARK");
        if (controlMark.ToUpper() == parameterAsString.ToUpper())
        {
          flag = true;
          break;
        }
      }
    }
    if (!flag)
    {
      reason = "ELEMENT NOT FOUND IN ASSEMBLY";
      return new bool?(false);
    }
    if (!datailParameterValue.GetValueOrDefault())
    {
      reason = datailParameterValue.HasValue ? "HARDWARE DETAIL PARAMETER ASSEMBLY" : "HARDWARE DETAIL PARAMETER";
      return datailParameterValue;
    }
    List<Element> list = assemblyInstance.GetMemberIds().Select<ElementId, Element>(new Func<ElementId, Element>(assemblyInstance.Document.GetElement)).ToList<Element>();
    if (list.Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320)).Count<Element>() < 1 && list.Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2000151)).Count<Element>() + list.Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001350)).Count<Element>() == list.Count<Element>())
      return new bool?(true);
    reason = "STRUCTURAL FRAMING ASSEMBLY";
    return new bool?(false);
  }

  private void setHardwareDetailParameters(Element elem)
  {
    Parameters.LookupParameter(elem, "DO_NOT_SCHEDULE")?.Set(1);
    Parameters.LookupParameter(elem, "HARDWARE_DETAIL")?.Set(1);
  }

  private List<ElementId> updateSubComponents(Autodesk.Revit.DB.Document revitDoc, FamilyInstance famInst)
  {
    List<ElementId> list = famInst.GetSubComponentIds().ToList<ElementId>();
    List<ElementId> source = new List<ElementId>();
    foreach (ElementId id in list)
    {
      Element element = revitDoc.GetElement(id);
      if ((element as FamilyInstance).Symbol.FamilyName != "CONNECTOR_COMPONENT" && !(element as FamilyInstance).Symbol.FamilyName.Contains("VOID"))
      {
        source.Add(id);
        if (element != null)
        {
          if (element is FamilyInstance famInst1)
            source.AddRange((IEnumerable<ElementId>) this.updateSubComponents(revitDoc, famInst1));
          if (element != null)
          {
            this.setHardwareDetailParameters(element);
            Parameters.ClearHostingParameters(element);
          }
        }
      }
    }
    return source.Distinct<ElementId>().ToList<ElementId>();
  }

  private List<string> GetHeaderScheduleNames(string precastManufacturerName)
  {
    string str = string.IsNullOrEmpty(App.TKTBOMFolderPath) ? $"C:/EDGEforREVIT/{precastManufacturerName}_HWDetail_BOM.xml" : $"{App.TKTBOMFolderPath}\\{precastManufacturerName}_HWDetail_BOM.xml";
    HWDBOMSettings hwdbomSettings = new HWDBOMSettings();
    List<string> headerScheduleNames = new List<string>();
    if (File.Exists(str) && hwdbomSettings.LoadTicketTemplateSettings(str))
    {
      foreach (DGEntry schedule in hwdbomSettings.schedule_List)
        headerScheduleNames.Add(schedule.scheduleName);
    }
    return headerScheduleNames;
  }

  private bool hardwareDetailTitleBlockPopulation(
    Autodesk.Revit.DB.Document revitDoc,
    ViewSheet viewSheet,
    Element element,
    AssemblyInstance assmbInst,
    out bool bWeightSet)
  {
    bWeightSet = false;
    using (new SubTransaction(revitDoc))
    {
      string str1 = "";
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        str1 = parameter.AsString();
      string str2 = string.IsNullOrEmpty(App.TBPopFolderPath) ? $"C:/EDGEforREVIT/{str1}_HWTitleBlock_Mapping.xml" : $"{App.TBPopFolderPath}\\{str1}_HWTitleBlock_Mapping.xml";
      TBParameterSettings parameterSettings = new TBParameterSettings();
      List<Element> elementList1 = new List<Element>();
      List<Element> elementList2 = new List<Element>();
      if (File.Exists(str2))
      {
        if (parameterSettings.LoadTicketTemplateSettings(str2))
        {
          List<DatagridItemEntry> tbParameterList = parameterSettings.TBParameterList;
          if (!this.setUserDefinedHardwareDetailTitleBlockParameters(revitDoc, tbParameterList, viewSheet, element, assmbInst, out bWeightSet))
            return false;
        }
      }
      else if (!this.setDefaultTitleBlockParameters(revitDoc, viewSheet, element, assmbInst, out bWeightSet))
        return false;
    }
    return true;
  }

  private bool setDefaultTitleBlockParameters(
    Autodesk.Revit.DB.Document revitDoc,
    ViewSheet viewSheet,
    Element element,
    AssemblyInstance assmbInst,
    out bool bWeightSet)
  {
    bWeightSet = false;
    Parameter parameter1 = viewSheet.LookupParameter("TKT_HW_WEIGHT");
    string controlMark = assmbInst.Name.Replace("_HWD-", "");
    double assemblyWeight = this.getAssemblyWeight(revitDoc, assmbInst, controlMark);
    if (assemblyWeight != -1.0 && parameter1 != null)
    {
      parameter1.Set(assemblyWeight);
      bWeightSet = true;
    }
    Parameter parameter2 = viewSheet.LookupParameter("Sheet name");
    if (parameter2 != null)
    {
      Parameter parameter3 = Parameters.LookupParameter((Element) assmbInst, "CONTROL_MARK") ?? Parameters.LookupParameter(element, "CONTROL_MARK");
      if (parameter3 != null)
        parameter2.Set(parameter3.AsString());
    }
    return true;
  }

  private bool setUserDefinedHardwareDetailTitleBlockParameters(
    Autodesk.Revit.DB.Document revitDoc,
    List<DatagridItemEntry> userDefinedMappings,
    ViewSheet viewSheet,
    Element element,
    AssemblyInstance assmbInst,
    out bool bWeightSet)
  {
    bWeightSet = false;
    foreach (DatagridItemEntry userDefinedMapping in userDefinedMappings)
    {
      Parameter parameter1 = viewSheet.LookupParameter(userDefinedMapping.mappingToParam);
      if (parameter1 != null)
      {
        if (userDefinedMapping.mappingToParam.Equals("TKT_HW_WEIGHT"))
        {
          string controlMark = assmbInst.Name.Replace("_HWD-", "");
          double assemblyWeight = this.getAssemblyWeight(revitDoc, assmbInst, controlMark);
          if (assemblyWeight != -1.0)
          {
            parameter1.Set(assemblyWeight);
            bWeightSet = true;
          }
        }
        else if (userDefinedMapping.mappingToParam.Equals("Sheet Name"))
        {
          Parameter parameter2 = Parameters.LookupParameter((Element) assmbInst, "CONTROL_MARK") ?? Parameters.LookupParameter(element, "CONTROL_MARK");
          if (parameter2 != null)
            parameter1.Set(parameter2.AsString());
        }
        else
        {
          Parameter parameter3 = Parameters.LookupParameter((Element) assmbInst, userDefinedMapping.mappingFromParam) ?? Parameters.LookupParameter(element, userDefinedMapping.mappingFromParam);
          if (parameter3 != null)
          {
            if (parameter3.StorageType == parameter1.StorageType)
            {
              if (parameter1.StorageType == StorageType.String)
              {
                if (userDefinedMapping.mappingFromParam.ToUpper().Contains("DATE"))
                {
                  string str = parameter3.AsString();
                  if (str != null)
                  {
                    if (str.Length > 10)
                      str = str.Substring(0, 10);
                    parameter1.Set(str);
                  }
                  else
                    parameter1.Set("");
                }
                else if (parameter3.AsString() != null)
                  parameter1.Set(parameter3.AsString());
                else
                  parameter1.Set("");
              }
              else if (parameter1.StorageType == StorageType.Double)
                parameter1.Set(parameter3.AsDouble());
              else if (parameter1.StorageType == StorageType.Integer)
                parameter1.Set(parameter3.AsInteger());
            }
            else if (parameter1.StorageType == StorageType.String)
            {
              if (parameter3.StorageType == StorageType.Double)
                parameter1.Set(parameter3.AsDouble().ToString());
              else if (parameter3.StorageType == StorageType.Integer)
                parameter1.Set(parameter3.AsInteger().ToString());
            }
            else if (parameter1.StorageType == StorageType.Double)
            {
              if (parameter3.StorageType == StorageType.Integer)
                parameter1.Set((double) parameter3.AsInteger());
            }
            else
            {
              int storageType = (int) parameter1.StorageType;
            }
          }
        }
      }
    }
    return true;
  }

  private double getAssemblyWeight(
    Autodesk.Revit.DB.Document revitDoc,
    AssemblyInstance assembly,
    string controlMark)
  {
    List<ElementId> list = assembly.GetMemberIds().ToList<ElementId>();
    List<Element> elementList = new List<Element>();
    foreach (ElementId id in list)
    {
      Element element = revitDoc.GetElement(id);
      if (element != null)
        elementList.Add(element);
    }
    double num = 0.0;
    foreach (Element elem in elementList)
    {
      double elementsWeight = this.getElementsWeight(elem);
      if (elementsWeight > 0.0)
        num += elementsWeight;
      this.CalculateSubElementsWeight(elem);
    }
    return num != 0.0 ? num : -1.0;
  }

  private double getElementsWeight(Element elem)
  {
    double elementsWeight = 0.0;
    Autodesk.Revit.DB.Document document = elem.Document;
    List<ElementId> elementIdList = new List<ElementId>();
    Options options = new Options();
    foreach (GeometryObject geometryObject1 in elem.get_Geometry(options))
    {
      GeometryInstance geometryInstance = geometryObject1 as GeometryInstance;
      if ((GeometryObject) geometryInstance != (GeometryObject) null)
      {
        foreach (GeometryObject geometryObject2 in geometryInstance.GetInstanceGeometry())
        {
          if (geometryObject2 is Solid)
          {
            foreach (GeometryObject face in (geometryObject2 as Solid).Faces)
            {
              if (face is PlanarFace)
              {
                PlanarFace planarFace = face as PlanarFace;
                if (planarFace.MaterialElementId != (ElementId) null && !planarFace.MaterialElementId.Equals((object) ElementId.InvalidElementId) && !elementIdList.Contains(planarFace.MaterialElementId) && !planarFace.MaterialElementId.Equals((object) ElementId.InvalidElementId))
                  elementIdList.Add(planarFace.MaterialElementId);
              }
            }
          }
        }
      }
    }
    double num1 = 0.0;
    foreach (ElementId elementId in elementIdList)
    {
      if (elementId != (ElementId) null && !elementId.Equals((object) ElementId.InvalidElementId))
      {
        ElementId structuralAssetId = (document.GetElement(elementId) as Material).StructuralAssetId;
        if (structuralAssetId != ElementId.InvalidElementId && document.GetElement(structuralAssetId) is PropertySetElement element)
        {
          StructuralAsset structuralAsset = element.GetStructuralAsset();
          if (structuralAsset != null && structuralAsset.Density >= 0.0)
            num1 = structuralAsset.Density;
        }
        double materialVolume = elem.GetMaterialVolume(elementId);
        if (num1 > 0.0 && materialVolume > 0.0)
        {
          double num2 = num1 * materialVolume;
          elementsWeight += UnitUtils.ConvertToInternalUnits(num2, UnitTypeId.KilogramsForce);
        }
      }
    }
    if (elementsWeight != 0.0)
      return elementsWeight;
    double parameterAsDouble = Parameters.GetParameterAsDouble(elem, "WEIGHT_PER_UNIT");
    double elementVolume = elem.GetElementVolume();
    if (parameterAsDouble > 0.0 && elementVolume > 0.0)
      elementsWeight = UnitUtils.ConvertToInternalUnits(parameterAsDouble * elementVolume, UnitTypeId.PoundsForce);
    return elementsWeight;
  }

  private void CalculateSubElementsWeight(Element elem)
  {
    Autodesk.Revit.DB.Document document = elem.Document;
    foreach (ElementId subComponentId in elem.GetSubComponentIds())
    {
      Element element = document.GetElement(subComponentId);
      if (element != null)
      {
        this.CalculateSubElementsWeight(element);
        double elementsWeight = this.getElementsWeight(element);
        if (elementsWeight >= 0.0)
          this.writeToComponentWeightParameter(element, elementsWeight);
      }
    }
  }

  private void writeToComponentWeightParameter(Element elem, double weight)
  {
    Parameter parameter = elem.LookupParameter("HW_COMPONENT_WEIGHT");
    if (parameter == null || parameter.StorageType != StorageType.Double)
      return;
    parameter.Set(weight);
  }

  private void GetCheckOutStatus(
    Autodesk.Revit.DB.Document revitDoc,
    AssemblyInstance assembInst,
    out List<ElementId> elemsUnowned,
    out List<Element> ownedByOtherUser)
  {
    elemsUnowned = new List<ElementId>();
    ownedByOtherUser = new List<Element>();
    foreach (ElementId memberId in (IEnumerable<ElementId>) assembInst.GetMemberIds())
    {
      Element element1 = revitDoc.GetElement(memberId);
      if (element1 != null)
      {
        List<ElementId> elemsUnowned1;
        List<Element> ownedByOtherUser1;
        this.GetCheckOutStatus(revitDoc, element1, out elemsUnowned1, out ownedByOtherUser1);
        foreach (ElementId elementId in elemsUnowned1)
        {
          if (!elemsUnowned.Contains(elementId))
            elemsUnowned.Add(elementId);
        }
        foreach (Element element2 in ownedByOtherUser1)
        {
          if (!ownedByOtherUser.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>().Contains(element2.Id))
            ownedByOtherUser.Add(element2);
        }
      }
    }
    switch (WorksharingUtils.GetCheckoutStatus(revitDoc, assembInst.Id))
    {
      case CheckoutStatus.OwnedByOtherUser:
        if (!ownedByOtherUser.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>().Contains(assembInst.Id))
        {
          ownedByOtherUser.Add((Element) assembInst);
          break;
        }
        break;
      case CheckoutStatus.NotOwned:
        if (!elemsUnowned.Contains(assembInst.Id))
        {
          elemsUnowned.Add(assembInst.Id);
          break;
        }
        break;
    }
    elemsUnowned = elemsUnowned.Distinct<ElementId>().ToList<ElementId>();
    ownedByOtherUser = ownedByOtherUser.Distinct<Element>().ToList<Element>();
  }

  private void GetCheckOutStatus(
    Autodesk.Revit.DB.Document revitDoc,
    Element element,
    out List<ElementId> elemsUnowned,
    out List<Element> ownedByOtherUser)
  {
    elemsUnowned = new List<ElementId>();
    ownedByOtherUser = new List<Element>();
    if (element.HasSubComponents())
    {
      foreach (ElementId subComponentId in element.GetSubComponentIds())
      {
        Element element1 = revitDoc.GetElement(subComponentId);
        if (element1 != null)
        {
          List<ElementId> elemsUnowned1;
          List<Element> ownedByOtherUser1;
          this.GetCheckOutStatus(revitDoc, element1, out elemsUnowned1, out ownedByOtherUser1);
          foreach (ElementId elementId in elemsUnowned1)
          {
            if (!elemsUnowned.Contains(elementId))
              elemsUnowned.Add(elementId);
          }
          foreach (Element element2 in ownedByOtherUser1)
          {
            if (!ownedByOtherUser.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>().Contains(element2.Id))
              ownedByOtherUser.Add(element2);
          }
        }
      }
    }
    switch (WorksharingUtils.GetCheckoutStatus(revitDoc, element.Id))
    {
      case CheckoutStatus.OwnedByOtherUser:
        if (!ownedByOtherUser.Select<Element, ElementId>((Func<Element, ElementId>) (x => x.Id)).ToList<ElementId>().Contains(element.Id))
        {
          ownedByOtherUser.Add(element);
          break;
        }
        break;
      case CheckoutStatus.NotOwned:
        if (!elemsUnowned.Contains(element.Id))
        {
          elemsUnowned.Add(element.Id);
          break;
        }
        break;
    }
    elemsUnowned = elemsUnowned.Distinct<ElementId>().ToList<ElementId>();
    ownedByOtherUser = ownedByOtherUser.Distinct<Element>().ToList<Element>();
  }

  internal sealed class SelectionFlter : ISelectionFilter
  {
    public bool AllowElement(Element element)
    {
      BuiltInCategory categoryIdAsInteger = (BuiltInCategory) this.GetCategoryIdAsInteger(element);
      bool? nullable = new bool?(false);
      switch (categoryIdAsInteger)
      {
        case BuiltInCategory.OST_SpecialityEquipment:
        case BuiltInCategory.OST_GenericModel:
          nullable = EDGE.TicketTools.HardwareDetail.HardwareDetail.checkHardwareDetailStatus(element, out string _, out string _);
          break;
        case BuiltInCategory.OST_Assemblies:
          nullable = !(element is AssemblyInstance) ? EDGE.TicketTools.HardwareDetail.HardwareDetail.checkHardwareDetailStatus(element, out string _, out string _) : EDGE.TicketTools.HardwareDetail.HardwareDetail.checkHardwareDetailStatus(element as AssemblyInstance, out string _, out string _);
          break;
      }
      return nullable.GetValueOrDefault();
    }

    public bool AllowReference(Reference refer, XYZ point) => false;

    private int GetCategoryIdAsInteger(Element element)
    {
      return element?.Category?.Id?.IntegerValue ?? -1;
    }
  }
}
