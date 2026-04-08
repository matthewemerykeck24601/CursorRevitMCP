// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoTicketHandler
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

internal class AutoTicketHandler
{
  public static Dictionary<ElementId, Tuple<Tuple<ElementId, double>, Tuple<ElementId, double>>> GetAlignmentsByMasterView(
    ViewSheet sheet,
    ElementId masterViewId,
    List<View> CreatedViews,
    out Viewport masterViewport,
    out Dictionary<ElementId, XYZ> masterOffsetDictionary)
  {
    List<ElementId> viewIDs = CreatedViews.Select<View, ElementId>((Func<View, ElementId>) (v => v.Id)).ToList<ElementId>();
    List<ElementId> elementIdList1 = new List<ElementId>();
    List<ElementId> elementIdList2 = new List<ElementId>();
    masterOffsetDictionary = new Dictionary<ElementId, XYZ>();
    Dictionary<ElementId, Tuple<ElementId, double>> dictionary1 = new Dictionary<ElementId, Tuple<ElementId, double>>();
    Dictionary<ElementId, Tuple<ElementId, double>> dictionary2 = new Dictionary<ElementId, Tuple<ElementId, double>>();
    Dictionary<ElementId, Tuple<Tuple<ElementId, double>, Tuple<ElementId, double>>> alignmentsByMasterView = new Dictionary<ElementId, Tuple<Tuple<ElementId, double>, Tuple<ElementId, double>>>();
    Document revitDoc = sheet.Document;
    revitDoc.GetElement(masterViewId);
    List<Viewport> list = new FilteredElementCollector(revitDoc, sheet.Id).OfClass(typeof (Viewport)).Where<Element>((Func<Element, bool>) (vp => viewIDs.Contains((vp as Viewport).ViewId))).Cast<Viewport>().ToList<Viewport>();
    list.RemoveAll((Predicate<Viewport>) (vp => revitDoc.GetElement(vp.ViewId) is View3D));
    masterViewport = list.First<Viewport>((Func<Viewport, bool>) (vp => vp.ViewId.IntegerValue == masterViewId.IntegerValue));
    Outline boxOutline1 = masterViewport.GetBoxOutline();
    XYZ minimumPoint1 = boxOutline1.MinimumPoint;
    XYZ maximumPoint1 = boxOutline1.MaximumPoint;
    foreach (Viewport viewport in list)
    {
      if (viewport.Id.IntegerValue != masterViewport.Id.IntegerValue)
      {
        Outline boxOutline2 = viewport.GetBoxOutline();
        XYZ minimumPoint2 = boxOutline2.MinimumPoint;
        XYZ maximumPoint2 = boxOutline2.MaximumPoint;
        double x = 0.0;
        if (maximumPoint2.X < minimumPoint1.X)
          x = maximumPoint2.X - minimumPoint1.X;
        else if (minimumPoint2.X > maximumPoint1.X)
          x = minimumPoint2.X - maximumPoint1.X;
        double y = 0.0;
        if (maximumPoint2.Y < minimumPoint1.Y)
          y = maximumPoint2.Y - minimumPoint1.Y;
        if (minimumPoint2.Y > maximumPoint1.Y)
          y = minimumPoint2.Y - maximumPoint1.Y;
        if (Math.Abs(x) > 0.0)
          elementIdList1.Add(viewport.Id);
        if (Math.Abs(y) > 0.0)
          elementIdList2.Add(viewport.Id);
        XYZ xyz = new XYZ(x, y, 0.0);
        masterOffsetDictionary.Add(viewport.Id, xyz);
      }
    }
    foreach (ElementId elementId in elementIdList1)
    {
      Outline boxOutline3 = (revitDoc.GetElement(elementId) as Viewport).GetBoxOutline();
      XYZ minimumPoint3 = boxOutline3.MinimumPoint;
      XYZ maximumPoint3 = boxOutline3.MaximumPoint;
      List<Tuple<ElementId, double>> source = new List<Tuple<ElementId, double>>();
      double x = masterOffsetDictionary[elementId].X;
      source.Add(new Tuple<ElementId, double>(masterViewport.Id, x));
      bool flag = x < 0.0;
      foreach (ElementId id in elementIdList1)
      {
        Viewport element = revitDoc.GetElement(id) as Viewport;
        if (element.Id.IntegerValue != elementId.IntegerValue && element.Id.IntegerValue != masterViewport.Id.IntegerValue)
        {
          Outline boxOutline4 = element.GetBoxOutline();
          XYZ minimumPoint4 = boxOutline4.MinimumPoint;
          XYZ maximumPoint4 = boxOutline4.MaximumPoint;
          double num = 0.0;
          if (flag)
          {
            if (maximumPoint3.X < minimumPoint4.X)
              num = maximumPoint3.X - minimumPoint4.X;
          }
          else if (minimumPoint3.X > maximumPoint4.X)
            num = minimumPoint3.X - maximumPoint4.X;
          if (num != 0.0)
            source.Add(new Tuple<ElementId, double>(id, num));
        }
      }
      dictionary1.Add(elementId, source.OrderBy<Tuple<ElementId, double>, double>((Func<Tuple<ElementId, double>, double>) (tp => Math.Abs(tp.Item2))).First<Tuple<ElementId, double>>());
    }
    foreach (ElementId elementId in elementIdList2)
    {
      Outline boxOutline5 = (revitDoc.GetElement(elementId) as Viewport).GetBoxOutline();
      XYZ minimumPoint5 = boxOutline5.MinimumPoint;
      XYZ maximumPoint5 = boxOutline5.MaximumPoint;
      List<Tuple<ElementId, double>> source = new List<Tuple<ElementId, double>>();
      double y = masterOffsetDictionary[elementId].Y;
      source.Add(new Tuple<ElementId, double>(masterViewport.Id, y));
      bool flag = y < 0.0;
      foreach (ElementId id in elementIdList2)
      {
        Viewport element = revitDoc.GetElement(id) as Viewport;
        if (element.Id.IntegerValue != elementId.IntegerValue && element.Id.IntegerValue != masterViewport.Id.IntegerValue)
        {
          Outline boxOutline6 = element.GetBoxOutline();
          XYZ minimumPoint6 = boxOutline6.MinimumPoint;
          XYZ maximumPoint6 = boxOutline6.MaximumPoint;
          double num = 0.0;
          if (flag)
          {
            if (maximumPoint5.Y < minimumPoint6.Y)
              num = maximumPoint5.Y - minimumPoint6.Y;
          }
          else if (minimumPoint5.Y > maximumPoint6.Y)
            num = minimumPoint5.Y - maximumPoint6.Y;
          if (num != 0.0)
            source.Add(new Tuple<ElementId, double>(id, num));
        }
      }
      dictionary2.Add(elementId, source.OrderBy<Tuple<ElementId, double>, double>((Func<Tuple<ElementId, double>, double>) (tp => Math.Abs(tp.Item2))).First<Tuple<ElementId, double>>());
    }
    foreach (ElementId key in masterOffsetDictionary.Keys)
    {
      Tuple<ElementId, double> tuple1 = new Tuple<ElementId, double>(masterViewport.Id, 0.0);
      Tuple<ElementId, double> tuple2 = new Tuple<ElementId, double>(masterViewport.Id, 0.0);
      if (dictionary1.Keys.Contains<ElementId>(key))
        tuple1 = dictionary1[key];
      if (dictionary2.Keys.Contains<ElementId>(key))
        tuple2 = dictionary2[key];
      alignmentsByMasterView.Add(key, new Tuple<Tuple<ElementId, double>, Tuple<ElementId, double>>(tuple1, tuple2));
    }
    return alignmentsByMasterView;
  }

  public static Result Execute(
    UIApplication revitApp,
    string strLastUsedScale,
    bool bMultiTicket,
    int intMultiSheetCurrentSheetNumber,
    string message)
  {
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_StructConnections
    });
    UIDocument activeUiDocument = revitApp.ActiveUIDocument;
    Document revitDoc = activeUiDocument.Document;
    List<AutoTicketAppendStringParameterData> appendStringData1;
    List<AutoTicketSettingsTools> ticketSettingsToolsList = AutoTicketSettingsReader.ReaderforAutoTicketSettings(revitDoc, "AUTO-TICKET GENERATION", false, out appendStringData1);
    using (Transaction transaction = new Transaction(revitDoc, "Auto-Ticket Generation"))
    {
      CalloutStyle autoTicketStyleSettings = (CalloutStyle) null;
      AutoTicketCustomValues customValues = (AutoTicketCustomValues) null;
      AutoTicketMinimumDimension minDimData = (AutoTicketMinimumDimension) null;
      AutoTicketAppendString appendStringData2 = (AutoTicketAppendString) null;
      AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(revitDoc);
      foreach (AutoTicketSettingsTools ticketSettingsTools in ticketSettingsToolsList)
      {
        if (ticketSettingsTools is AutoTicketCalloutAndDimensionTexts)
        {
          AutoTicketCalloutAndDimensionTexts andDimensionTexts = ticketSettingsTools as AutoTicketCalloutAndDimensionTexts;
          autoTicketStyleSettings = new CalloutStyle(revitDoc, AutoTicketEnhancementWorkflow.AutoTicketGeneration, andDimensionTexts.CalloutFamily, andDimensionTexts.OverallDimension, andDimensionTexts.GeneralDimension, andDimensionTexts.TextStyle, andDimensionTexts.UseEQ);
        }
        if (ticketSettingsTools is AutoTicketAppendString)
          appendStringData2 = ticketSettingsTools as AutoTicketAppendString;
        if (ticketSettingsTools is AutoTicketMinimumDimension)
          minDimData = ticketSettingsTools as AutoTicketMinimumDimension;
        if (ticketSettingsTools is AutoTicketCustomValues)
          customValues = ticketSettingsTools as AutoTicketCustomValues;
      }
      FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType));
      List<DimensionType> source1 = new List<DimensionType>();
      if (autoTicketStyleSettings != null)
      {
        foreach (DimensionType dimensionType in elementCollector)
        {
          if (autoTicketStyleSettings.dimensionStyle != null && dimensionType.Name == autoTicketStyleSettings.dimensionStyle.Name && dimensionType.StyleType == DimensionStyleType.Linear)
            source1.Add(dimensionType);
          else if (autoTicketStyleSettings.otherDimensionStyle != null && dimensionType.Name == autoTicketStyleSettings.otherDimensionStyle.Name && dimensionType.StyleType == DimensionStyleType.Linear)
            source1.Add(dimensionType);
        }
      }
      if (source1.Count == 0)
      {
        foreach (DimensionType dimensionType in elementCollector)
        {
          if (dimensionType.Name == "PTAC - TICKET (GAP TO ELEMENT)" || dimensionType.Name == "PTAC - TICKET (FIXED TO DIM. LINE)")
            source1.Add(dimensionType);
          if (source1.Count == 2)
            break;
        }
      }
      if (source1.Count < 2 && revitDoc.GetElement(revitDoc.GetDefaultElementTypeId(ElementTypeGroup.LinearDimensionType)) is DimensionType element1)
        source1.Add(element1);
      List<DimensionType> dimensionTypeList = new List<DimensionType>();
      foreach (DimensionType dimensionType in source1.ToList<DimensionType>())
      {
        if (dimensionType.StyleType != DimensionStyleType.Linear)
          dimensionTypeList.Add(dimensionType);
      }
      if (dimensionTypeList.Count > 0)
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (DimensionType dimensionType in dimensionTypeList)
        {
          stringBuilder.Append(dimensionType.Name);
          stringBuilder.AppendLine($" {{{dimensionType.Id.IntegerValue.ToString()}}}");
        }
        TaskDialog taskDialog = new TaskDialog("Auto Ticket Generation")
        {
          MainContent = "One or more dimension styles that would have been placed have a \"Dimension String Type\" of Non-Linear. Only linear dimension strings are supported at this time. The operation will be cancelled."
        };
        taskDialog.MainContent += Environment.NewLine;
        taskDialog.MainContent += Environment.NewLine;
        taskDialog.MainContent += "Expand to view problematic styles.";
        taskDialog.ExpandedContent = stringBuilder.ToString();
        taskDialog.Show();
        return (Result) 1;
      }
      string error;
      List<ElementId> elementIdList1 = BatchPopulatorCore.CullSelectionForTicketPopulator(revitDoc, activeUiDocument.Selection.GetElementIds().ToList<ElementId>(), out error);
      if (elementIdList1.Count == 0)
      {
        TaskDialog.Show("Auto-Ticket Generation", error);
        return (Result) 1;
      }
      List<SpreadSheetData> sheetsDataToReturn;
      AutoTicketHandler.checkParams(elementIdList1.ToList<ElementId>(), revitDoc, out sheetsDataToReturn);
      if (transaction.Start() != TransactionStatus.Started)
      {
        TaskDialog.Show("Error", "Unable to start transaction group, please contact support");
        return (Result) 1;
      }
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        Dictionary<ElementId, Dictionary<ElementId, Tuple<Tuple<ElementId, double>, Tuple<ElementId, double>>>> dictionary1 = new Dictionary<ElementId, Dictionary<ElementId, Tuple<Tuple<ElementId, double>, Tuple<ElementId, double>>>>();
        Dictionary<ElementId, Dictionary<ElementId, XYZ>> masterOffsetDict = new Dictionary<ElementId, Dictionary<ElementId, XYZ>>();
        Dictionary<ElementId, ElementId> dictionary2 = new Dictionary<ElementId, ElementId>();
        ViewSheet AssemblySheet;
        Dictionary<ElementId, Dictionary<ViewSheet, List<View>>> CreatedSheets;
        List<ElementId> relinquishList;
        bool viewSpreadSheetEnhancement;
        if (BatchPopulatorCore.Execute(revitApp, strLastUsedScale, bMultiTicket, intMultiSheetCurrentSheetNumber, elementIdList1, message, out AssemblySheet, out CreatedSheets, out relinquishList, out viewSpreadSheetEnhancement, true) != null)
        {
          int num = (int) transaction.RollBack();
          if (relinquishList.Count > 0)
          {
            RelinquishOptions generalCategories = new RelinquishOptions(false);
            generalCategories.CheckedOutElements = true;
            TransactWithCentralOptions options = new TransactWithCentralOptions();
            WorksharingUtils.RelinquishOwnership(revitDoc, generalCategories, options);
          }
          return (Result) 1;
        }
        Dictionary<ElementId, Dictionary<ElementId, Dictionary<AutoTicketHandler.SectionType, List<ElementId>>>> dictionary3 = new Dictionary<ElementId, Dictionary<ElementId, Dictionary<AutoTicketHandler.SectionType, List<ElementId>>>>();
        foreach (ElementId key1 in CreatedSheets.Keys)
        {
          ElementId assemblyKey = key1;
          dictionary3.Add(assemblyKey, new Dictionary<ElementId, Dictionary<AutoTicketHandler.SectionType, List<ElementId>>>());
          foreach (ViewSheet key2 in CreatedSheets[assemblyKey].Keys)
          {
            ElementId masterViewId = (ElementId) null;
            IEnumerable<View> source2 = CreatedSheets[assemblyKey][key2].Where<View>((Func<View, bool>) (s =>
            {
              if (!(s.AssociatedAssemblyInstanceId == assemblyKey))
                return false;
              return s.ViewType == ViewType.Detail || s.ViewType == ViewType.Section;
            }));
            source2.OrderBy<View, string>((Func<View, string>) (v => v.Name));
            dictionary3[assemblyKey].Add(key2.Id, new Dictionary<AutoTicketHandler.SectionType, List<ElementId>>()
            {
              {
                AutoTicketHandler.SectionType.ElevationTop,
                new List<ElementId>()
              },
              {
                AutoTicketHandler.SectionType.ElevationBottom,
                new List<ElementId>()
              },
              {
                AutoTicketHandler.SectionType.ElevationLeft,
                new List<ElementId>()
              },
              {
                AutoTicketHandler.SectionType.ElevationRight,
                new List<ElementId>()
              },
              {
                AutoTicketHandler.SectionType.ElevationFront,
                new List<ElementId>()
              },
              {
                AutoTicketHandler.SectionType.ElevationBack,
                new List<ElementId>()
              },
              {
                AutoTicketHandler.SectionType.None,
                new List<ElementId>()
              }
            });
            foreach (View view in source2)
            {
              if (!(view is View3D))
              {
                if (view.Name.Contains("Elevation Top"))
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationTop].Add(view.Id);
                else if (view.Name.Contains("Elevation Bottom"))
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationBottom].Add(view.Id);
                else if (view.Name.Contains("Elevation Left"))
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationLeft].Add(view.Id);
                else if (view.Name.Contains("Elevation Right"))
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationRight].Add(view.Id);
                else if (view.Name.Contains("Elevation Front"))
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationFront].Add(view.Id);
                else if (view.Name.Contains("Elevation Back"))
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationBack].Add(view.Id);
                else
                  dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.None].Add(view.Id);
              }
            }
            if (dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationTop].Count > 0)
              masterViewId = dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationTop].First<ElementId>();
            else if (dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationBottom].Count > 0)
              masterViewId = dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationBottom].First<ElementId>();
            else if (dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationLeft].Count > 0)
              masterViewId = dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationLeft].First<ElementId>();
            else if (dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationRight].Count > 0)
              masterViewId = dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationRight].First<ElementId>();
            else if (dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationFront].Count > 0)
              masterViewId = dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationFront].First<ElementId>();
            else if (dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationBack].Count > 0)
              masterViewId = dictionary3[assemblyKey][key2.Id][AutoTicketHandler.SectionType.ElevationBack].First<ElementId>();
            if (masterViewId != (ElementId) null)
            {
              Viewport masterViewport;
              Dictionary<ElementId, XYZ> masterOffsetDictionary;
              dictionary1.Add(key2.Id, AutoTicketHandler.GetAlignmentsByMasterView(key2, masterViewId, CreatedSheets[assemblyKey][key2], out masterViewport, out masterOffsetDictionary));
              masterOffsetDict.Add(key2.Id, masterOffsetDictionary);
              dictionary2.Add(key2.Id, masterViewport.Id);
            }
          }
        }
        Dictionary<string, List<string>> dictionary4 = new Dictionary<string, List<string>>();
        bool flag1 = true;
        FilteredElementCollector source3 = new FilteredElementCollector(revitDoc);
        source3.OfCategory(BuiltInCategory.OST_MultiCategoryTags);
        source3.OfClass(typeof (FamilySymbol));
        if (source3.Count<Element>() == 0)
          flag1 = false;
        FamilySymbol leftTagFamily = (FamilySymbol) null;
        FamilySymbol rightTagFamily = (FamilySymbol) null;
        FamilySymbol leftTagFamilyTYP = (FamilySymbol) null;
        FamilySymbol rightTagFamilyTYP = (FamilySymbol) null;
        foreach (FamilySymbol familySymbol in source3)
        {
          string str = autoTicketStyleSettings != null ? autoTicketStyleSettings.calloutStyle : "AUTO_TICKET_CALLOUT";
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
        Dictionary<AssemblyInstance, List<SpreadSheetData>> dictionary5 = new Dictionary<AssemblyInstance, List<SpreadSheetData>>();
        Dictionary<AssemblyInstance, List<SpreadSheetData>> dictionary6 = SpreadSheetData.assignType(sheetsDataToReturn, revitDoc);
        bool? nullable;
        if (viewSpreadSheetEnhancement)
        {
          nullable = new EnhancementSpreadSheet(dictionary6).ShowDialog();
          bool flag2 = false;
          if (nullable.GetValueOrDefault() == flag2 & nullable.HasValue)
            return (Result) 1;
        }
        foreach (AssemblyInstance assemblyInstance in elementIdList1.Select<ElementId, AssemblyInstance>((Func<ElementId, AssemblyInstance>) (id => revitDoc.GetElement(id) as AssemblyInstance)))
        {
          foreach (ViewSheet key3 in CreatedSheets[assemblyInstance.Id].Keys)
          {
            ViewSheet sheet = key3;
            foreach (AutoTicketHandler.SectionType key4 in dictionary3[assemblyInstance.Id][sheet.Id].Keys)
            {
              foreach (ElementId elementId in dictionary3[assemblyInstance.Id][sheet.Id][key4])
              {
                View element2 = revitDoc.GetElement(elementId) as View;
                ICollection<ElementId> list = (ICollection<ElementId>) new FilteredElementCollector(revitDoc, element2.Id).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (elem => !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).Symbol.FamilyName != "CONNECTOR_COMPONENT")).Select<Element, ElementId>((Func<Element, ElementId>) (elem => elem.Id)).ToList<ElementId>();
                Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dictionary7 = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
                Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dictionary8 = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
                Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dictionary9 = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
                List<ElementId> elementIdList2 = new List<ElementId>((IEnumerable<ElementId>) list);
                List<ElementId> AllElementIds1 = new List<ElementId>();
                AllElementIds1.AddRange((IEnumerable<ElementId>) list);
                List<ElementId> AllElementIds2 = new List<ElementId>();
                AllElementIds2.AddRange((IEnumerable<ElementId>) list);
                AssemblyInstance assInst = revitDoc.GetElement(element2.AssociatedAssemblyInstanceId) as AssemblyInstance;
                Element structuralFramingElement = assInst.GetStructuralFramingElement();
                LocationInFormAnalyzer locationAnalyzer = new LocationInFormAnalyzer(assInst, 1.0);
                foreach (ElementId id in list.ToList<ElementId>())
                {
                  string controlMark = revitDoc.GetElement(id).GetControlMark();
                  if (controlMark == "" && revitDoc.GetElement(id) is FamilyInstance element3)
                    controlMark = element3.Symbol.FamilyName;
                  if (controlMark != "" && !(id == structuralFramingElement.Id))
                  {
                    bool flag3 = false;
                    bool flag4 = false;
                    bool flag5 = false;
                    SpreadSheetData spreadSheetData = dictionary6.Where<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>((Func<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>, bool>) (e => e.Key.Name == assInst.Name)).FirstOrDefault<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>().Value.Where<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.Name == controlMark)).FirstOrDefault<SpreadSheetData>();
                    if (spreadSheetData != null)
                    {
                      string name = element2.Name;
                      nullable = spreadSheetData.CalloutAlways;
                      if (!nullable.HasValue)
                      {
                        nullable = spreadSheetData.CalloutDim;
                        if (!nullable.HasValue)
                        {
                          flag3 = true;
                          flag4 = true;
                          goto label_154;
                        }
                      }
                      nullable = spreadSheetData.CalloutAlways;
                      bool flag6 = false;
                      if (nullable.GetValueOrDefault() == flag6 & nullable.HasValue)
                      {
                        nullable = spreadSheetData.CalloutDim;
                        if (!nullable.GetValueOrDefault())
                        {
                          nullable = spreadSheetData.CalloutDim;
                          if (nullable.HasValue)
                          {
                            nullable = spreadSheetData.CalloutDim;
                            bool flag7 = false;
                            if (nullable.GetValueOrDefault() == flag7 & nullable.HasValue)
                            {
                              list.Remove(id);
                              AllElementIds1.Remove(id);
                              goto label_154;
                            }
                            goto label_154;
                          }
                        }
                        if (name.Contains("Top") || name.Contains("Bottom"))
                        {
                          nullable = spreadSheetData.DimTopBottom;
                          bool flag8 = false;
                          if (nullable.GetValueOrDefault() == flag8 & nullable.HasValue)
                          {
                            list.Remove(id);
                            AllElementIds1.Remove(id);
                          }
                        }
                        else if (name.Contains("Front") || name.Contains("Back"))
                        {
                          nullable = spreadSheetData.DimFrontBack;
                          bool flag9 = false;
                          if (nullable.GetValueOrDefault() == flag9 & nullable.HasValue)
                          {
                            list.Remove(id);
                            AllElementIds1.Remove(id);
                          }
                        }
                        else if (name.Contains("Left") || name.Contains("Right"))
                        {
                          nullable = spreadSheetData.DimLeftRight;
                          bool flag10 = false;
                          if (nullable.GetValueOrDefault() == flag10 & nullable.HasValue)
                          {
                            list.Remove(id);
                            AllElementIds1.Remove(id);
                          }
                        }
                      }
label_154:
                      if (name.Contains("Top") || name.Contains("Bottom"))
                      {
                        nullable = spreadSheetData.DimTopBottom;
                        bool flag11 = false;
                        if (nullable.GetValueOrDefault() == flag11 & nullable.HasValue)
                        {
                          list.Remove(id);
                          AllElementIds2.Remove(id);
                        }
                        else
                        {
                          nullable = spreadSheetData.DimTopBottom;
                          if (!nullable.HasValue)
                          {
                            flag3 = true;
                            flag5 = true;
                          }
                        }
                      }
                      else if (name.Contains("Front") || name.Contains("Back"))
                      {
                        nullable = spreadSheetData.DimFrontBack;
                        bool flag12 = false;
                        if (nullable.GetValueOrDefault() == flag12 & nullable.HasValue)
                        {
                          list.Remove(id);
                          AllElementIds2.Remove(id);
                        }
                        else
                        {
                          nullable = spreadSheetData.DimFrontBack;
                          if (!nullable.HasValue)
                          {
                            flag3 = true;
                            flag5 = true;
                          }
                        }
                      }
                      else if (name.Contains("Left") || name.Contains("Right"))
                      {
                        nullable = spreadSheetData.DimLeftRight;
                        bool flag13 = false;
                        if (nullable.GetValueOrDefault() == flag13 & nullable.HasValue)
                        {
                          list.Remove(id);
                          AllElementIds2.Remove(id);
                        }
                        else
                        {
                          nullable = spreadSheetData.DimLeftRight;
                          if (!nullable.HasValue)
                          {
                            flag3 = true;
                            flag5 = true;
                          }
                        }
                      }
                    }
                    else
                    {
                      flag3 = true;
                      flag4 = true;
                      flag5 = true;
                    }
                    if (flag3 && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(revitDoc.GetElement(id) as FamilyInstance))
                    {
                      if (!dictionary4.ContainsKey(assemblyInstance.Name))
                        dictionary4.Add(assemblyInstance.Name, new List<string>()
                        {
                          controlMark
                        });
                      else if (!dictionary4[assemblyInstance.Name].Contains(controlMark) && id != structuralFramingElement.Id)
                        dictionary4[assemblyInstance.Name].Add(controlMark);
                      list.Remove(id);
                      if (flag5)
                        AllElementIds2.Remove(id);
                      if (flag4)
                        AllElementIds1.Remove(id);
                    }
                  }
                }
                List<SpreadSheetData> spreadSheetDataList = dictionary6.Where<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>((Func<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>, bool>) (e => e.Key.Name == assInst.Name)).FirstOrDefault<KeyValuePair<AssemblyInstance, List<SpreadSheetData>>>().Value;
                Dictionary<string, Dictionary<FormLocation, List<ElementId>>> lifDict1 = AutoTicketHandler.createLIFDict(revitDoc, locationAnalyzer, (ICollection<ElementId>) AllElementIds1, locationInFormValues);
                Dictionary<string, Dictionary<FormLocation, List<ElementId>>> lifDict2 = AutoTicketHandler.createLIFDict(revitDoc, locationAnalyzer, (ICollection<ElementId>) AllElementIds2, locationInFormValues);
                revitDoc.Regenerate();
                List<ElementId> viewVisible = new FilteredElementCollector(revitDoc, elementId).ToElementIds().ToList<ElementId>();
                foreach (string key5 in lifDict1.Keys)
                {
                  foreach (FormLocation key6 in lifDict1[key5].Keys)
                    lifDict1[key5][key6].RemoveAll((Predicate<ElementId>) (eid => !viewVisible.Contains(eid)));
                }
                lifDict1.Remove(structuralFramingElement.GetControlMark());
                foreach (string key7 in lifDict2.Keys)
                {
                  foreach (FormLocation key8 in lifDict2[key7].Keys)
                    lifDict2[key7][key8].RemoveAll((Predicate<ElementId>) (eid => !viewVisible.Contains(eid)));
                }
                lifDict2.Remove(structuralFramingElement.GetControlMark());
                if (flag1)
                  flag1 = new AutoTicketCalloutHandler(element2, revitDoc, lifDict1).processAllCallOuts(leftTagFamily, rightTagFamily, leftTagFamilyTYP, rightTagFamilyTYP);
                bool bFirst = dictionary3[assemblyInstance.Id][sheet.Id][key4].IndexOf(element2.Id) == 0;
                BatchAutoDimension.DimensionView(revitDoc, element2, bFirst, lifDict2, autoTicketStyleSettings, appendStringData2, customValues, minDimData, appendStringData1);
              }
            }
            if (dictionary1.ContainsKey(sheet.Id))
            {
              FamilyInstance familyInstance = new FilteredElementCollector(revitDoc, sheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).ToElements().Cast<FamilyInstance>().FirstOrDefault<FamilyInstance>();
              Viewport element4 = revitDoc.GetElement(dictionary2[sheet.Id]) as Viewport;
              Outline boxOutline1 = element4.GetBoxOutline();
              XYZ xyz1 = new XYZ(boxOutline1.MinimumPoint.X, boxOutline1.MaximumPoint.Y, 0.0);
              foreach (ElementId elementId in (IEnumerable<ElementId>) dictionary1[sheet.Id].Keys.OrderBy<ElementId, double>((Func<ElementId, double>) (key => Math.Abs(masterOffsetDict[sheet.Id][key].X))))
              {
                Viewport element5 = revitDoc.GetElement(elementId) as Viewport;
                element5.GetBoxCenter();
                Outline boxOutline2 = element5.GetBoxOutline();
                double x1 = 0.0;
                if (dictionary1[sheet.Id][elementId].Item1.Item2 != 0.0)
                {
                  Outline boxOutline3 = (revitDoc.GetElement(dictionary1[sheet.Id][elementId].Item1.Item1) as Viewport).GetBoxOutline();
                  x1 = dictionary1[sheet.Id][elementId].Item1.Item2;
                  if (x1 < 0.0)
                  {
                    double num = boxOutline2.MaximumPoint.X - boxOutline3.MinimumPoint.X;
                    x1 -= num;
                  }
                  else if (x1 > 0.0)
                  {
                    double num = boxOutline3.MaximumPoint.X - boxOutline2.MinimumPoint.X;
                    x1 += num;
                  }
                }
                XYZ xyz2 = new XYZ(x1, 0.0, 0.0);
                element5.SetBoxCenter(element5.GetBoxCenter() + xyz2);
                double x2 = element5.GetBoxOutline().MinimumPoint.X;
                if (x2 < xyz1.X)
                  xyz1 = new XYZ(x2, xyz1.Y, 0.0);
              }
              foreach (ElementId elementId in (IEnumerable<ElementId>) dictionary1[sheet.Id].Keys.OrderBy<ElementId, double>((Func<ElementId, double>) (key => Math.Abs(masterOffsetDict[sheet.Id][key].Y))))
              {
                Viewport element6 = revitDoc.GetElement(elementId) as Viewport;
                element6.GetBoxCenter();
                Outline boxOutline4 = element6.GetBoxOutline();
                double y1 = 0.0;
                if (dictionary1[sheet.Id][elementId].Item2.Item2 != 0.0)
                {
                  Outline boxOutline5 = (revitDoc.GetElement(dictionary1[sheet.Id][elementId].Item2.Item1) as Viewport).GetBoxOutline();
                  y1 = dictionary1[sheet.Id][elementId].Item2.Item2;
                  if (y1 < 0.0)
                  {
                    double num = boxOutline4.MaximumPoint.Y - boxOutline5.MinimumPoint.Y;
                    y1 -= num;
                  }
                  else if (y1 > 0.0)
                  {
                    double num = boxOutline5.MaximumPoint.Y - boxOutline4.MinimumPoint.Y;
                    y1 += num;
                  }
                }
                XYZ xyz3 = new XYZ(0.0, y1, 0.0);
                element6.SetBoxCenter(element6.GetBoxCenter() + xyz3);
                double y2 = element6.GetBoxOutline().MaximumPoint.Y;
                if (y2 > xyz1.Y)
                  xyz1 = new XYZ(xyz1.X, y2, 0.0);
              }
              if (familyInstance != null)
              {
                BoundingBoxXYZ boundingBoxXyz = familyInstance.get_BoundingBox((View) sheet);
                XYZ xyz4 = new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, 0.0) - xyz1;
                element4.SetBoxCenter(element4.GetBoxCenter() + xyz4);
                foreach (Viewport viewport in dictionary1[sheet.Id].Keys.Select<ElementId, Element>((Func<ElementId, Element>) (k => revitDoc.GetElement(k))))
                  viewport.SetBoxCenter(viewport.GetBoxCenter() + xyz4);
              }
            }
          }
        }
        if (!flag1)
          new TaskDialog("Tag Error")
          {
            MainInstruction = "No callout families were found in this document, therefore no callouts were placed."
          }.Show();
        if (transaction.Commit() != TransactionStatus.Committed)
        {
          TaskDialog.Show("Error", "Unable to commit transaction group.  Please contact support");
          return (Result) 1;
        }
        revitApp.ActiveUIDocument.RequestViewChange((View) AssemblySheet);
        if (dictionary4.Count > 0)
        {
          TaskDialog taskDialog1 = new TaskDialog("Auto-Ticket");
          taskDialog1.MainInstruction = "One or more family instances in the view did not have EDGE Dim Lines and thus were not dimensioned to.";
          taskDialog1.MainContent = "Please use the Bulk Updater on these families if you wish to dimension to them.";
          taskDialog1.ExpandedContent = "The following control marks were not dimensioned to: \n";
          foreach (string key in dictionary4.Keys)
          {
            TaskDialog taskDialog2 = taskDialog1;
            taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}Assembly: {key}\n";
            foreach (string str in dictionary4[key])
            {
              TaskDialog taskDialog3 = taskDialog1;
              taskDialog3.ExpandedContent = $"{taskDialog3.ExpandedContent}     {str}\n";
            }
          }
          taskDialog1.Show();
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        TaskDialog.Show("Auto Ticket", $"General Exception occurred in Auto Ticket: {ex.Message}{ex.StackTrace}");
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }

  public static Dictionary<string, Dictionary<FormLocation, List<ElementId>>> createLIFDict(
    Document revitDoc,
    LocationInFormAnalyzer locationAnalyzer,
    ICollection<ElementId> AllElementIds,
    AutoTicketLIF lifSettings)
  {
    Dictionary<string, Dictionary<FormLocation, List<ElementId>>> lifDict = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
    foreach (ElementId allElementId in (IEnumerable<ElementId>) AllElementIds)
    {
      string key = revitDoc.GetElement(allElementId).GetControlMark();
      if (key == "" && revitDoc.GetElement(allElementId) is FamilyInstance element)
        key = element.Symbol.FamilyName;
      if (key != "")
      {
        if (!lifDict.Keys.Contains<string>(key))
        {
          lifDict.Add(key, new Dictionary<FormLocation, List<ElementId>>());
          lifDict[key].Add(FormLocation.TIF, new List<ElementId>());
          lifDict[key].Add(FormLocation.BIF, new List<ElementId>());
          lifDict[key].Add(FormLocation.SIF, new List<ElementId>());
          lifDict[key].Add(FormLocation.None, new List<ElementId>());
        }
        string upper = Utils.ElementUtils.Parameters.GetParameterAsString(revitDoc.GetElement(allElementId), "MANUFACTURE_COMPONENT").ToUpper();
        if (!upper.Contains("RAW CONSUMABLE"))
        {
          if (!upper.Contains("EMBED"))
          {
            lifDict[key][FormLocation.None].Add(allElementId);
          }
          else
          {
            string str = LocationInFormAnalyzer.ProcessAndAssignLIF(revitDoc, lifSettings, allElementId, locationAnalyzer);
            if (str == lifSettings.TIF)
              lifDict[key][FormLocation.TIF].Add(allElementId);
            else if (str == lifSettings.BIF)
              lifDict[key][FormLocation.BIF].Add(allElementId);
            else if (str == lifSettings.SIF)
              lifDict[key][FormLocation.SIF].Add(allElementId);
            else
              lifDict[key][FormLocation.None].Add(allElementId);
          }
        }
      }
    }
    return lifDict;
  }

  private static ICollection<ElementId> checkDimValuesForEnahcement(
    ICollection<ElementId> AllElementIds,
    List<SpreadSheetData> listOfSpreadsheet,
    Document revitDoc,
    Element sfElem,
    View view,
    Dictionary<string, List<string>> noDimLinesForControlMarks,
    out Dictionary<string, List<string>> noDimLinesFound)
  {
    noDimLinesFound = new Dictionary<string, List<string>>();
    Dictionary<string, Dictionary<FormLocation, List<ElementId>>> dictionary = new Dictionary<string, Dictionary<FormLocation, List<ElementId>>>();
    foreach (ElementId id in AllElementIds.ToList<ElementId>())
    {
      string controlMark = revitDoc.GetElement(id).GetControlMark();
      if (controlMark == "" && revitDoc.GetElement(id) is FamilyInstance element)
        controlMark = element.Symbol.FamilyName;
      if (controlMark != "" && !(id == sfElem.Id))
      {
        SpreadSheetData spreadSheetData = listOfSpreadsheet.Where<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.Name == controlMark)).FirstOrDefault<SpreadSheetData>();
        string assemblyName = spreadSheetData.AssemblyName;
        if (spreadSheetData != null)
        {
          bool flag1 = false;
          string name = view.Name;
          bool? nullable;
          if (name.Contains("Top") || name.Contains("Bottom"))
          {
            nullable = spreadSheetData.DimTopBottom;
            bool flag2 = false;
            if (nullable.GetValueOrDefault() == flag2 & nullable.HasValue)
            {
              AllElementIds.Remove(id);
            }
            else
            {
              nullable = spreadSheetData.DimTopBottom;
              if (!nullable.HasValue && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(revitDoc.GetElement(id) as FamilyInstance))
                flag1 = true;
            }
          }
          else if (name.Contains("Front") || name.Contains("Back"))
          {
            nullable = spreadSheetData.DimFrontBack;
            bool flag3 = false;
            if (nullable.GetValueOrDefault() == flag3 & nullable.HasValue)
            {
              AllElementIds.Remove(id);
            }
            else
            {
              nullable = spreadSheetData.DimFrontBack;
              if (!nullable.HasValue && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(revitDoc.GetElement(id) as FamilyInstance))
                flag1 = true;
            }
          }
          else if (name.Contains("Left") || name.Contains("Right"))
          {
            nullable = spreadSheetData.DimLeftRight;
            bool flag4 = false;
            if (nullable.GetValueOrDefault() == flag4 & nullable.HasValue)
            {
              AllElementIds.Remove(id);
            }
            else
            {
              nullable = spreadSheetData.DimLeftRight;
              if (!nullable.HasValue && !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(revitDoc.GetElement(id) as FamilyInstance))
                flag1 = true;
            }
          }
          if (flag1)
          {
            if (!noDimLinesForControlMarks.ContainsKey(assemblyName))
              noDimLinesForControlMarks.Add(assemblyName, new List<string>()
              {
                controlMark
              });
            else if (!noDimLinesForControlMarks[assemblyName].Contains(controlMark) && id != sfElem.Id)
              noDimLinesForControlMarks[assemblyName].Add(controlMark);
            AllElementIds.Remove(id);
          }
        }
      }
    }
    noDimLinesFound = noDimLinesForControlMarks;
    return AllElementIds;
  }

  public static void checkParams(
    List<ElementId> allIds,
    Document revitDoc,
    out List<SpreadSheetData> sheetsDataToReturn)
  {
    bool flag = false;
    Dictionary<string, List<Element>> dictionary1 = new Dictionary<string, List<Element>>();
    Dictionary<string, List<Element>> dictionary2 = new Dictionary<string, List<Element>>();
    Dictionary<string, List<Element>> dictionary3 = new Dictionary<string, List<Element>>();
    Dictionary<string, List<Element>> dictionary4 = new Dictionary<string, List<Element>>();
    Dictionary<string, List<Element>> dictionary5 = new Dictionary<string, List<Element>>();
    List<SpreadSheetData> spreadSheetDataList = new List<SpreadSheetData>();
    foreach (ElementId allId in allIds)
    {
      AssemblyInstance element = revitDoc.GetElement(allId) as AssemblyInstance;
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
      {
        BuiltInCategory.OST_SpecialityEquipment,
        BuiltInCategory.OST_GenericModel
      });
      foreach (Element elem in (IEnumerable<Element>) new FilteredElementCollector(revitDoc, element.GetMemberIds()).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (elem => !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (elem => (elem as FamilyInstance).Symbol.FamilyName != "CONNECTOR_COMPONENT")).Select<Element, Element>((Func<Element, Element>) (elem => elem)).ToList<Element>())
      {
        Parameter parameter1 = Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_T_B");
        Parameter parameter2 = Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_F_B");
        Parameter parameter3 = Utils.ElementUtils.Parameters.LookupParameter(elem, "DIM_L_R");
        Parameter parameter4 = Utils.ElementUtils.Parameters.LookupParameter(elem, "CALLOUT_ALWAYS");
        Parameter parameter5 = Utils.ElementUtils.Parameters.LookupParameter(elem, "CALLOUT_DIM");
        bool? tb = new bool?(false);
        bool? fb = new bool?(false);
        bool? lr = new bool?(false);
        bool? calloutAll = new bool?(false);
        bool? calloutdim = new bool?(false);
        if (parameter1 != null)
        {
          if (parameter1.Definition.GetDataType() != SpecTypeId.Boolean.YesNo)
          {
            if (!dictionary1.ContainsKey(element.Name))
              dictionary1.Add(element.Name, new List<Element>()
              {
                elem
              });
            else
              dictionary1[element.Name].Add(elem);
            flag = true;
          }
          else
            tb = parameter1.HasValue ? (parameter1.AsInteger() != 0 ? new bool?(true) : new bool?(false)) : new bool?();
        }
        else
          tb = new bool?();
        if (parameter2 != null)
        {
          if (parameter2.Definition.GetDataType() != SpecTypeId.Boolean.YesNo)
          {
            if (!dictionary2.ContainsKey(element.Name))
              dictionary2.Add(element.Name, new List<Element>()
              {
                elem
              });
            else
              dictionary2[element.Name].Add(elem);
            flag = true;
          }
          else
            fb = parameter2.HasValue ? (parameter2.AsInteger() != 0 ? new bool?(true) : new bool?(false)) : new bool?();
        }
        else
          fb = new bool?();
        if (parameter3 != null)
        {
          if (parameter3.Definition.GetDataType() != SpecTypeId.Boolean.YesNo)
          {
            if (!dictionary3.ContainsKey(element.Name))
              dictionary3.Add(element.Name, new List<Element>()
              {
                elem
              });
            else
              dictionary3[element.Name].Add(elem);
            flag = true;
          }
          else
            lr = parameter3.HasValue ? (parameter3.AsInteger() != 0 ? new bool?(true) : new bool?(false)) : new bool?();
        }
        else
          lr = new bool?();
        if (parameter4 != null)
        {
          if (parameter4.Definition.GetDataType() != SpecTypeId.Boolean.YesNo)
          {
            if (!dictionary4.ContainsKey(element.Name))
              dictionary4.Add(element.Name, new List<Element>()
              {
                elem
              });
            else
              dictionary4[element.Name].Add(elem);
            flag = true;
          }
          else
            calloutAll = parameter4.HasValue ? (parameter4.AsInteger() != 0 ? new bool?(true) : new bool?(false)) : new bool?();
        }
        else
          calloutAll = new bool?();
        if (parameter5 != null)
        {
          if (parameter5.Definition.GetDataType() != SpecTypeId.Boolean.YesNo)
          {
            if (!dictionary5.ContainsKey(element.Name))
              dictionary5.Add(element.Name, new List<Element>()
              {
                elem
              });
            else
              dictionary5[element.Name].Add(elem);
            flag = true;
          }
          else
            calloutdim = parameter5.HasValue ? (parameter5.AsInteger() != 0 ? new bool?(true) : new bool?(false)) : new bool?();
        }
        else
          calloutdim = new bool?();
        spreadSheetDataList.Add(new SpreadSheetData(element, elem, tb, fb, lr, calloutAll, calloutdim));
      }
    }
    if (flag)
    {
      TaskDialog taskDialog = new TaskDialog("Auto Ticket");
      taskDialog.MainContent = "The following parameters are not set up as Yes/No parameter types for the structural framing elements mentioned below:";
      string str1 = "";
      string str2 = "";
      string str3 = "";
      string str4 = "";
      string str5 = "";
      string str6 = "";
      foreach (ElementId allId in allIds)
      {
        AssemblyInstance element1 = revitDoc.GetElement(allId) as AssemblyInstance;
        if (dictionary1.ContainsKey(element1.Name))
        {
          if (string.IsNullOrEmpty(str1))
            str1 += "DIM_T_B:\n";
          str1 = $"{str1}   {element1.Name}\n";
          foreach (Element element2 in dictionary1[element1.Name])
          {
            str1 += "      ";
            FamilyInstance familyInstance = element2 as FamilyInstance;
            str1 += familyInstance.Symbol.FamilyName != familyInstance.Name ? $"   {familyInstance.Symbol.FamilyName} - {familyInstance.Name}" : "   " + familyInstance.Name;
            str1 = $"{str1} - {familyInstance.Id?.ToString()}\n";
          }
        }
        if (dictionary2.ContainsKey(element1.Name))
        {
          if (string.IsNullOrEmpty(str3))
            str3 += "DIM_F_B:\n";
          str3 = $"{str3}   {element1.Name}\n";
          foreach (Element element3 in dictionary2[element1.Name])
          {
            str3 += "      ";
            FamilyInstance familyInstance = element3 as FamilyInstance;
            str3 += familyInstance.Symbol.FamilyName != familyInstance.Name ? $"   {familyInstance.Symbol.FamilyName} - {familyInstance.Name}" : "   " + familyInstance.Name;
            str3 = $"{str3} - {familyInstance.Id?.ToString()}\n";
          }
        }
        if (dictionary3.ContainsKey(element1.Name))
        {
          if (string.IsNullOrEmpty(str2))
            str2 += "DIM_L_R:\n";
          str2 = $"{str2}   {element1.Name}\n";
          foreach (Element element4 in dictionary3[element1.Name])
          {
            str2 += "      ";
            FamilyInstance familyInstance = element4 as FamilyInstance;
            str2 += familyInstance.Symbol.FamilyName != familyInstance.Name ? $"   {familyInstance.Symbol.FamilyName} - {familyInstance.Name}" : "   " + familyInstance.Name;
            str2 = $"{str2} - {familyInstance.Id?.ToString()}\n";
          }
        }
        if (dictionary4.ContainsKey(element1.Name))
        {
          if (string.IsNullOrEmpty(str4))
            str4 += "CALLOUT_ALWAYS:\n";
          str4 = $"{str4}   {element1.Name}\n";
          foreach (Element element5 in dictionary4[element1.Name])
          {
            str4 += "      ";
            FamilyInstance familyInstance = element5 as FamilyInstance;
            str4 += familyInstance.Symbol.FamilyName != familyInstance.Name ? $"   {familyInstance.Symbol.FamilyName} - {familyInstance.Name}" : "   " + familyInstance.Name;
            str4 = $"{str4} - {familyInstance.Id?.ToString()}\n";
          }
        }
        if (dictionary5.ContainsKey(element1.Name))
        {
          if (string.IsNullOrEmpty(str5))
            str5 += "CALLOUT_DIM:\n";
          str5 = $"{str5}   {element1.Name}\n";
          foreach (Element element6 in dictionary5[element1.Name])
          {
            str5 += "      ";
            FamilyInstance familyInstance = element6 as FamilyInstance;
            str5 += familyInstance.Symbol.FamilyName != familyInstance.Name ? $"   {familyInstance.Symbol.FamilyName} - {familyInstance.Name}" : "   " + familyInstance.Name;
            str5 = $"{str5} - {familyInstance.Id?.ToString()}\n";
          }
        }
      }
      if (!string.IsNullOrEmpty(str1))
        str6 += str1;
      if (!string.IsNullOrEmpty(str3))
        str6 += str3;
      if (!string.IsNullOrEmpty(str2))
        str6 += str2;
      if (!string.IsNullOrEmpty(str4))
        str6 += str4;
      if (!string.IsNullOrEmpty(str5))
        str6 += str5;
      taskDialog.ExpandedContent = str6;
      taskDialog.Show();
    }
    sheetsDataToReturn = spreadSheetDataList;
  }

  private enum SectionType
  {
    ElevationTop,
    ElevationBottom,
    ElevationLeft,
    ElevationRight,
    ElevationFront,
    ElevationBack,
    None,
  }
}
