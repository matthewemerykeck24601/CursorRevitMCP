// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingMaster_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.InsulationTools.InsulationDrawing.Views;
using EDGE.UserSettingTools.Insulation_Drawing_Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Utils.ElementUtils;
using Utils.Forms;
using Utils.IEnumerable_Extensions;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

[Transaction(TransactionMode.Manual)]
public class InsulationDrawingMaster_Command : IExternalCommand
{
  private string userInitials = string.Empty;
  private List<int> SheetNumbers = new List<int>();

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    UIApplication application = commandData.Application;
    string str1 = "Insulation Drawing - Master";
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = (str1 + " Must be run in the Project Environment"),
        MainContent = $"You are currently in the family editor, {str1} must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    string manufacturer = "";
    Parameter parameter1 = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter1 != null && parameter1.StorageType == StorageType.String)
      manufacturer = parameter1.AsString();
    if (string.IsNullOrWhiteSpace(manufacturer))
    {
      TaskDialog.Show("Insulaion Drawing Settings", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to run Insulation Drawing - Assembly again.");
      return (Result) 1;
    }
    InsulationDrawingSettings insulationDrawingSettings = new InsulationDrawingSettings(manufacturer);
    InsulationDrawingSettingsObject settingsObject;
    if (!insulationDrawingSettings.SettingsRead())
    {
      settingsObject = new InsulationDrawingSettingsObject(document);
      bool flag = true;
      if (settingsObject.InsulationDetailLineStyle == null)
        flag = false;
      if (settingsObject.MarkCircleDetailLineStyle == null)
        flag = false;
      if (settingsObject.RecessCalloutsTextStyle == null)
        flag = false;
      if (settingsObject.InsulationMarkTextStyle == null)
        flag = false;
      if (settingsObject.OverallDimensionStyle == null)
        flag = false;
      if (settingsObject.GeneralDimensionStyle == null)
        flag = false;
      if (settingsObject.TitleBlockFamily == null)
        flag = false;
      if (flag)
      {
        new TaskDialog("Insulation Drawing Settings")
        {
          MainInstruction = "Insulation Drawing Settings file not found.",
          MainContent = "Insulation Drawing Settings file not found. Insulation Drawing - Master will use default settings."
        }.Show();
      }
      else
      {
        new TaskDialog("Insulation Drawing Settings")
        {
          MainInstruction = "Insulation Drawing Settings file not found.",
          MainContent = "Insulation Drawing Settings file not found. The default settings styles were not found in the project. Please update settings and try again."
        }.Show();
        return (Result) 1;
      }
    }
    else
      settingsObject = insulationDrawingSettings.GetSettings(document);
    if (settingsObject == null)
    {
      new TaskDialog("Insulation Drawing Settings")
      {
        MainInstruction = "Insulation Drawing Settings not found.",
        MainContent = "One or more insulation drawing settings are not present in the project. Please update the settings and try again."
      }.Show();
      return (Result) 1;
    }
    List<Autodesk.Revit.DB.View> list1 = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Views).Cast<Autodesk.Revit.DB.View>().ToList<Autodesk.Revit.DB.View>();
    Autodesk.Revit.DB.View legend1 = (Autodesk.Revit.DB.View) null;
    foreach (Autodesk.Revit.DB.View view in list1)
    {
      if (view.ViewType == ViewType.Legend)
      {
        legend1 = view;
        break;
      }
    }
    if (legend1 == null)
    {
      TaskDialog.Show("Insulation Drawing Settings", "At least one legend must exist in the project for the Insulation Drawing - Master tool to run. Please create a legend and run the tool again.");
      return (Result) 1;
    }
    int sheetCount = 1;
    int sheetNumber = 1;
    bool flag1 = false;
    List<ViewSheet> list2 = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Sheets).OfType<ViewSheet>().Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (x => x.AssociatedAssemblyInstanceId == (ElementId) null || x.AssociatedAssemblyInstanceId.IntegerValue == -1)).ToList<ViewSheet>();
    ViewSheet viewSheet1 = (ViewSheet) null;
    Autodesk.Revit.DB.View legend2 = (Autodesk.Revit.DB.View) null;
    BoundingBoxXYZ boundingBoxXyz = (BoundingBoxXYZ) null;
    Dictionary<string, ViewSheet> dictionary1 = new Dictionary<string, ViewSheet>();
    Dictionary<string, Autodesk.Revit.DB.View> dictionary2 = new Dictionary<string, Autodesk.Revit.DB.View>();
    Dictionary<ElementId, string> dictionary3 = new Dictionary<ElementId, string>();
    foreach (ViewSheet viewSheet2 in list2)
    {
      int result;
      if (int.TryParse(viewSheet2.SheetNumber, out result))
        this.SheetNumbers.Add(result);
      if (viewSheet2.AssemblyInstanceId.IntegerValue == -1 && viewSheet2.Name.StartsWith("INS -"))
      {
        if (!flag1)
          flag1 = true;
        foreach (ElementId allViewport in (IEnumerable<ElementId>) viewSheet2.GetAllViewports())
        {
          if (document.GetElement(allViewport) is Viewport element1)
          {
            Autodesk.Revit.DB.View element = document.GetElement(element1.ViewId) as Autodesk.Revit.DB.View;
            string name = element.Name;
            if (name.StartsWith("INSULATION DETAIL - "))
            {
              string[] strArray = name.Split('-');
              if (strArray.Length == 3)
              {
                string key = strArray[2].Trim();
                if (!dictionary1.ContainsKey(key))
                  dictionary1.Add(key, viewSheet2);
                if (!dictionary2.ContainsKey(key))
                  dictionary2.Add(key, element);
                string str2 = strArray[1].Trim();
                if (!dictionary3.ContainsKey(element.Id))
                  dictionary3.Add(element.Id, str2);
              }
              else if (strArray.Length == 2)
              {
                string key = strArray[1].Trim();
                if (!dictionary1.ContainsKey(key))
                  dictionary1.Add(key, viewSheet2);
                if (!dictionary2.ContainsKey(key))
                  dictionary2.Add(key, element);
                if (!dictionary3.ContainsKey(element.Id))
                  dictionary3.Add(element.Id, key);
              }
            }
          }
        }
      }
    }
    if (this.SheetNumbers.Count > 0)
    {
      while (this.SheetNumbers.Contains(sheetNumber))
        ++sheetNumber;
    }
    string highMark = "";
    if (dictionary1.Count > 0)
    {
      highMark = dictionary1.Keys.ToList<string>().NaturalSort().ToList<string>().Last<string>();
      viewSheet1 = dictionary1[highMark];
      if (viewSheet1 != null)
      {
        string name = viewSheet1.Name;
        name.Replace("INS -", "");
        int result;
        if (int.TryParse(name, out result))
          sheetCount = result;
      }
      legend2 = dictionary2[highMark];
    }
    string str3 = highMark;
    Dictionary<string, FamilyInstance> dictionary4 = new Dictionary<string, FamilyInstance>();
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> elementList = new List<Element>();
    List<Element> list3 = new FilteredElementCollector(document).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (i => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(i))).ToList<Element>();
    if (list3.Count == 0)
    {
      new TaskDialog("Insulation Drawing - Master")
      {
        MainInstruction = "No Insulation Found",
        MainContent = "No insulation in the model."
      }.Show();
      return (Result) 1;
    }
    List<string> stringList1 = new List<string>();
    foreach (Element element in list3)
    {
      string parameterAsString = Parameters.GetParameterAsString(element, "INSULATION_MARK");
      if (!string.IsNullOrWhiteSpace(parameterAsString) && !stringList1.Contains(parameterAsString) && InsulationDrawingUtils.InsulationLocked(element))
      {
        stringList1.Add(parameterAsString);
        if (InsulationDrawingUtils.ValidForInsulationDrawing(element, highMark) && !dictionary4.ContainsKey(parameterAsString))
          dictionary4.Add(parameterAsString, element as FamilyInstance);
      }
    }
    if (dictionary4.Count == 0 && !flag1)
    {
      new TaskDialog("Insulation Drawing - Master")
      {
        MainInstruction = "No Insulation Required Drawing",
        MainContent = "No insulation in the model required drawing. Insulation must have a valid insulation mark and cuts or recesses to require being drawn. For more information about what insulation is valid for drawing please reference the Insulation Drawing - Master help file."
      }.Show();
      return (Result) 1;
    }
    if (dictionary4.Count == 0 & flag1)
    {
      new TaskDialog("Insulation Drawing - Master")
      {
        MainInstruction = "Master drawings exist for all valid insulation marks.",
        MainContent = $"This tool will only generate drawings for the insulation marks above the current highest mark in the existing master insulation sheets. ({highMark})\n\nReview existing master drawings for correctness and completeness."
      }.Show();
      return (Result) 1;
    }
    List<string> list4 = dictionary4.Keys.ToList<string>();
    List<string> me1 = new List<string>();
    List<string> me2 = new List<string>();
    foreach (string str4 in list4)
    {
      if (this.IsNumeric(str4))
        me1.Add(str4);
      else
        me2.Add(str4);
    }
    List<string> list5 = me1.NaturalSort().ToList<string>();
    List<string> list6 = me2.NaturalSort().ToList<string>();
    list4.Clear();
    list4.AddRange((IEnumerable<string>) list5);
    list4.AddRange((IEnumerable<string>) list6);
    bool flag2 = !this.IsNumeric(list4.FirstOrDefault<string>());
    if (highMark == list4.Last<string>())
    {
      new TaskDialog("Insulation Drawing - Master")
      {
        MainInstruction = "The drawing tool has drawn all insulation.",
        MainContent = "Detail drawings for all existing insulation marks are complete.  When detailing new insulation marks, use the Insulation Drawing Tool if the mark number falls below the highest existing insulation mark number."
      }.Show();
      return (Result) 1;
    }
    int num1 = 0;
    if (!string.IsNullOrWhiteSpace(highMark))
      num1 = list4.FindIndex((Predicate<string>) (x => x == highMark)) + 1;
    List<string> stringList2 = new List<string>();
    List<InsulationDetail> insulationDetailList = new List<InsulationDetail>();
    for (int index = num1; index < list4.Count; ++index)
    {
      string str5 = list4[index];
      try
      {
        InsulationDetail insulationDetail = new InsulationDetail(dictionary4[str5], settingsObject, str5);
        insulationDetailList.Add(insulationDetail);
      }
      catch (Exception ex)
      {
        stringList2.Add(str5);
      }
    }
    if (stringList2.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Insulation Drawing - Master");
      taskDialog1.MainInstruction = "Unable to process insulation geometry.";
      taskDialog1.MainContent = "An error occured processing the geometry for one or more insulation marks. Would you like to continue without drawing the insulation associated with these marks?";
      taskDialog1.ExpandedContent += "Failed Marks:";
      foreach (string str6 in stringList2)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str6}\n";
      }
      taskDialog1.CommonButtons = (TaskDialogCommonButtons) 10;
      if (!taskDialog1.Show().Equals((object) (TaskDialogResult) 6))
        return (Result) 1;
    }
    if (viewSheet1 != null && legend2 != null)
      activeUiDocument.ActiveView = (Autodesk.Revit.DB.View) viewSheet1;
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    string str7 = string.Empty;
    if (!string.IsNullOrEmpty(highMark))
      str7 = insulationDetailList[0].InsulationMark;
    Document revitDoc = document;
    int scaleFactorMaster = settingsObject.InsulationDrawingScaleFactorMaster;
    string firstInsulationMark = str7;
    MasterInsulationDrawingWindow insulationDrawingWindow = new MasterInsulationDrawingWindow(mainWindowHandle, revitDoc, scaleFactorMaster, firstInsulationMark);
    insulationDrawingWindow.ShowDialog();
    if (insulationDrawingWindow.cancel)
      return (Result) 1;
    int selectedScale = insulationDrawingWindow.selectedScale;
    if (!insulationDrawingWindow.DoNotAddToExistingSheet && legend2 != null && viewSheet1 != null)
    {
      if (document.IsWorkshared)
      {
        ICollection<ElementId> UniqueElementIds;
        if (CheckElementsOwnership.CheckOwnership(activeUiDocument.Document, (ICollection<ElementId>) new List<ElementId>()
        {
          legend2.Id,
          viewSheet1.Id
        }, out UniqueElementIds, out ICollection<ElementId> _, out ICollection<ElementId> _) && UniqueElementIds.Count != 0)
        {
          new TaskDialog("EDGE Worksharing Error - Insulation Drawing - Master")
          {
            MainInstruction = "The existing sheet and legend is not editable currently.",
            MainContent = "The existing sheet and legend that contains the highest insulation mark's drawing is owned by another user and is not editable. Please coordinate with project members to allow for ownership of the elements or try reloading latest from central. Insulation Drawing - Master will be canceled."
          }.Show();
          return (Result) 1;
        }
      }
      activeUiDocument.ActiveView = legend2;
    }
    else
    {
      viewSheet1 = (ViewSheet) null;
      legend2 = (Autodesk.Revit.DB.View) null;
    }
    Guid guid = new Guid("5e58424c-974b-4ac3-b7ac-dbe8821cf06b");
    Schema schema = Schema.Lookup(guid);
    if (schema == null)
    {
      SchemaBuilder schemaBuilder = new SchemaBuilder(guid);
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
      FieldBuilder fieldBuilder1 = schemaBuilder.AddSimpleField("TopXCordinate", typeof (string));
      FieldBuilder fieldBuilder2 = schemaBuilder.AddSimpleField("TopYCordinate", typeof (string));
      fieldBuilder1.SetDocumentation("Top X Corrdinator To Use For Insulation Drawing - Master");
      fieldBuilder2.SetDocumentation("Top Y Corrdinator To Use For Insulation Drawing - Master");
      schemaBuilder.SetSchemaName("PTAC_Insulation_Drawings_Coordinates");
      schema = schemaBuilder.Finish();
    }
    double num2 = 0.0;
    double num3 = 0.0;
    double num4 = 0.0;
    double num5 = 0.0;
    List<string> stringList3 = new List<string>();
    Dictionary<string, string> dictionary5 = new Dictionary<string, string>();
    double count = (double) insulationDetailList.Count;
    ViewSheet viewSheet3 = (ViewSheet) null;
    using (Transaction transaction = new Transaction(document, "Insulation Drawing - Master"))
    {
      int num6 = (int) transaction.Start();
      FamilyInstance elem = (FamilyInstance) null;
      InsulationDrawingLegendInfo drawingLegendInfo;
      if (viewSheet1 == null || legend2 == null)
      {
        viewSheet1 = this.CreateNewSheet(settingsObject.TitleBlockFamily, sheetCount, sheetNumber);
        ++sheetCount;
        ++sheetNumber;
        while (this.SheetNumbers.Contains(sheetNumber))
          ++sheetNumber;
        elem = new FilteredElementCollector(document, viewSheet1.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
        boundingBoxXyz = elem.get_BoundingBox((Autodesk.Revit.DB.View) viewSheet1);
        Parameter parameter2 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_TOP");
        Parameter parameter3 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_BOTTOM");
        Parameter parameter4 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_LEFT");
        Parameter parameter5 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_RIGHT");
        if (parameter2 != null && parameter2.HasValue && parameter2.StorageType == StorageType.Double)
          num2 = parameter2.AsDouble() / (double) viewSheet1.Scale;
        if (parameter3 != null && parameter3.HasValue && parameter3.StorageType == StorageType.Double)
          num3 = parameter3.AsDouble() / (double) viewSheet1.Scale;
        if (parameter4 != null && parameter4.HasValue && parameter4.StorageType == StorageType.Double)
          num4 = parameter4.AsDouble() / (double) viewSheet1.Scale;
        if (parameter5 != null && parameter5.HasValue && parameter5.StorageType == StorageType.Double)
          num5 = parameter5.AsDouble() / (double) viewSheet1.Scale;
        double height = boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num2 - num3;
        double width = boundingBoxXyz.Max.X - boundingBoxXyz.Min.X - num4 - num5;
        legend2 = InsulationDrawingUtils.CreateNewLegend(legend1, selectedScale);
        XYZ zero = XYZ.Zero;
        drawingLegendInfo = new InsulationDrawingLegendInfo(legend2, height, width, zero);
        Entity entity = new Entity(schema);
        entity.Set<string>("TopXCordinate", zero.X.ToString());
        entity.Set<string>("TopYCordinate", zero.Y.ToString());
        legend2.SetEntity(entity);
        if (dictionary3.ContainsKey(legend2.Id))
          drawingLegendInfo.minMark = dictionary3[legend2.Id];
      }
      else
      {
        if (elem == null)
        {
          elem = new FilteredElementCollector(document, viewSheet1.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
          Parameter parameter6 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_TOP");
          Parameter parameter7 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_BOTTOM");
          Parameter parameter8 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_LEFT");
          Parameter parameter9 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_RIGHT");
          if (parameter6 != null && parameter6.HasValue && parameter6.StorageType == StorageType.Double)
            num2 = parameter6.AsDouble() / (double) viewSheet1.Scale;
          if (parameter7 != null && parameter7.HasValue && parameter7.StorageType == StorageType.Double)
            num3 = parameter7.AsDouble() / (double) viewSheet1.Scale;
          if (parameter8 != null && parameter8.HasValue && parameter8.StorageType == StorageType.Double)
            num4 = parameter8.AsDouble() / (double) viewSheet1.Scale;
          if (parameter9 != null && parameter9.HasValue && parameter9.StorageType == StorageType.Double)
            num5 = parameter9.AsDouble() / (double) viewSheet1.Scale;
          boundingBoxXyz = elem.get_BoundingBox((Autodesk.Revit.DB.View) viewSheet1);
        }
        double height = boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num2 - num3;
        double width = boundingBoxXyz.Max.X - boundingBoxXyz.Min.X - num4 - num5;
        XYZ TopLeft = XYZ.Zero;
        Entity entity = legend2.GetEntity(schema);
        string s1 = entity.Get<string>(schema.GetField("TopXCordinate"));
        string s2 = entity.Get<string>(schema.GetField("TopYCordinate"));
        if (!string.IsNullOrWhiteSpace(s1) && !string.IsNullOrEmpty(s2))
        {
          double result1;
          if (!double.TryParse(s1, out result1))
            result1 = 0.0;
          double result2;
          if (!double.TryParse(s2, out result2))
            result2 = 0.0;
          TopLeft = new XYZ(result1, result2, 0.0);
        }
        drawingLegendInfo = new InsulationDrawingLegendInfo(legend2, height, width, TopLeft);
        drawingLegendInfo.existingViewPort = true;
        if (dictionary3.ContainsKey(legend2.Id))
          drawingLegendInfo.minMark = dictionary3[legend2.Id];
        XYZ zero = XYZ.Zero;
        XYZ PlacementLocation;
        try
        {
          PlacementLocation = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 0, "Pick Placement Location for Insulation Mark - " + insulationDetailList[0].InsulationMark);
        }
        catch (Exception ex)
        {
          int num7 = (int) transaction.RollBack();
          return (Result) 1;
        }
        List<int> intList = new List<int>();
        for (int index = 0; index < insulationDetailList.Count; ++index)
        {
          InsulationDetail detail = insulationDetailList[index];
          if (!this.IsNumeric(detail.InsulationMark) && !flag2)
          {
            flag2 = true;
            break;
          }
          if (drawingLegendInfo.AddInsulationDrawing(detail, PlacementLocation))
          {
            intList.Add(index);
            break;
          }
          stringList3.Add(detail.InsulationMark);
          intList.Add(index);
        }
        foreach (int index in intList)
          insulationDetailList.RemoveAt(index);
      }
      if (drawingLegendInfo == null)
        return (Result) 0;
      foreach (InsulationDetail detail in insulationDetailList)
      {
        int num8 = this.IsNumeric(detail.InsulationMark) ? 0 : (!flag2 ? 1 : 0);
        if (num8 != 0)
        {
          drawingLegendInfo.CreateNewLegend = true;
          flag2 = true;
        }
        if (num8 != 0 || !drawingLegendInfo.AddInsulationDrawing(detail))
        {
          if (drawingLegendInfo.CreateNewLegend)
          {
            drawingLegendInfo.FinalizePosition();
            this.nameLegend(legend2, drawingLegendInfo.minMark, drawingLegendInfo.maxMark);
            if (!dictionary5.ContainsKey(viewSheet1.Name))
              dictionary5.Add(viewSheet1.Name, legend2.Name);
            if (boundingBoxXyz == null)
            {
              if (elem == null)
                elem = new FilteredElementCollector(document, viewSheet1.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
              boundingBoxXyz = elem.get_BoundingBox((Autodesk.Revit.DB.View) viewSheet1);
              Parameter parameter10 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_TOP");
              Parameter parameter11 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_BOTTOM");
              Parameter parameter12 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_LEFT");
              Parameter parameter13 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_RIGHT");
              if (parameter10 != null && parameter10.HasValue && parameter10.StorageType == StorageType.Double)
                num2 = parameter10.AsDouble() / (double) viewSheet1.Scale;
              if (parameter11 != null && parameter11.HasValue && parameter11.StorageType == StorageType.Double)
                num3 = parameter11.AsDouble() / (double) viewSheet1.Scale;
              if (parameter12 != null && parameter12.HasValue && parameter12.StorageType == StorageType.Double)
                num4 = parameter12.AsDouble() / (double) viewSheet1.Scale;
              if (parameter13 != null && parameter13.HasValue && parameter13.StorageType == StorageType.Double)
                num5 = parameter13.AsDouble() / (double) viewSheet1.Scale;
            }
            BoundingBoxUV outline = legend2.Outline;
            double num9 = boundingBoxXyz.Min.X + num4;
            double num10 = boundingBoxXyz.Max.Y - num2;
            double num11 = drawingLegendInfo.trueWidth / 2.0;
            XYZ point = new XYZ(num9 + num11, num10 - drawingLegendInfo.trueHeight / 2.0, 0.0);
            if (!drawingLegendInfo.existingViewPort)
              Viewport.Create(document, viewSheet1.Id, legend2.Id, point);
            if (elem == null)
            {
              elem = new FilteredElementCollector(document, viewSheet1.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
              Parameter parameter14 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_TOP");
              Parameter parameter15 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_BOTTOM");
              Parameter parameter16 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_LEFT");
              Parameter parameter17 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_RIGHT");
              if (parameter14 != null && parameter14.HasValue && parameter14.StorageType == StorageType.Double)
                num2 = parameter14.AsDouble() / (double) viewSheet1.Scale;
              if (parameter15 != null && parameter15.HasValue && parameter15.StorageType == StorageType.Double)
                num3 = parameter15.AsDouble() / (double) viewSheet1.Scale;
              if (parameter16 != null && parameter16.HasValue && parameter16.StorageType == StorageType.Double)
                num4 = parameter16.AsDouble() / (double) viewSheet1.Scale;
              if (parameter17 != null && parameter17.HasValue && parameter17.StorageType == StorageType.Double)
                num5 = parameter17.AsDouble() / (double) viewSheet1.Scale;
            }
            Parameter parameter18 = Parameters.LookupParameter((Element) viewSheet1, "SHT_DRAWING_COVERS");
            if (parameter18 != null)
            {
              string str8 = "MASTER INSULATION SHEET - ";
              string str9 = !(drawingLegendInfo.minMark != drawingLegendInfo.maxMark) ? str8 + drawingLegendInfo.minMark : $"{str8}{drawingLegendInfo.minMark}-{drawingLegendInfo.maxMark}";
              parameter18.Set(str9);
            }
            if (!this.GetUserInitials(viewSheet1))
            {
              int num12 = (int) transaction.RollBack();
              return (Result) 1;
            }
            viewSheet3 = viewSheet1;
            viewSheet1 = this.CreateNewSheet(settingsObject.TitleBlockFamily, sheetCount, sheetNumber);
            ++sheetCount;
            ++sheetNumber;
            while (this.SheetNumbers.Contains(sheetNumber))
              ++sheetNumber;
            elem = (FamilyInstance) null;
            if (elem == null)
            {
              elem = new FilteredElementCollector(document, viewSheet1.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
              Parameter parameter19 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_TOP");
              Parameter parameter20 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_BOTTOM");
              Parameter parameter21 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_LEFT");
              Parameter parameter22 = Parameters.LookupTypeParameter((Element) elem, "INS_DRAW_PADDING_RIGHT");
              if (parameter19 != null && parameter19.HasValue && parameter19.StorageType == StorageType.Double)
                num2 = parameter19.AsDouble() / (double) viewSheet1.Scale;
              if (parameter20 != null && parameter20.HasValue && parameter20.StorageType == StorageType.Double)
                num3 = parameter20.AsDouble() / (double) viewSheet1.Scale;
              if (parameter21 != null && parameter21.HasValue && parameter21.StorageType == StorageType.Double)
                num4 = parameter21.AsDouble() / (double) viewSheet1.Scale;
              if (parameter22 != null && parameter22.HasValue && parameter22.StorageType == StorageType.Double)
                num5 = parameter22.AsDouble() / (double) viewSheet1.Scale;
            }
            double height = boundingBoxXyz.Max.Y - boundingBoxXyz.Min.Y - num2 - num3;
            double width = boundingBoxXyz.Max.X - boundingBoxXyz.Min.X - num4 - num5;
            XYZ zero = XYZ.Zero;
            legend2 = InsulationDrawingUtils.CreateNewLegend(legend1, selectedScale);
            drawingLegendInfo = new InsulationDrawingLegendInfo(legend2, height, width, zero);
            Entity entity = new Entity(schema);
            entity.Set<string>("TopXCordinate", zero.X.ToString());
            entity.Set<string>("TopYCordinate", zero.Y.ToString());
            legend2.SetEntity(entity);
            if (!drawingLegendInfo.AddInsulationDrawing(detail))
              stringList3.Add(detail.InsulationMark);
          }
          else
          {
            if (drawingLegendInfo.DrawingToLarge)
            {
              new TaskDialog("Insulation Drawing - Master")
              {
                MainInstruction = "Insulation Drawing - Master Failed",
                MainContent = "One or more insulation drawings would not fit on the title block as configured. No drawings were updated or created.\n\nPlease review title block parameters, scale, and insulation drawing settings for correctness. See help page for more information."
              }.Show();
              int num13 = (int) transaction.RollBack();
              return (Result) 1;
            }
            stringList3.Add(detail.InsulationMark);
          }
        }
      }
      if (!drawingLegendInfo.ContainsDrawing())
      {
        document.Delete((ICollection<ElementId>) new List<ElementId>()
        {
          viewSheet1.Id,
          legend2.Id
        });
      }
      else
      {
        viewSheet3 = viewSheet1;
        drawingLegendInfo.FinalizePosition();
        this.nameLegend(legend2, drawingLegendInfo.minMark, drawingLegendInfo.maxMark);
        if (!dictionary5.ContainsKey(viewSheet1.Name))
          dictionary5.Add(viewSheet1.Name, legend2.Name);
        BoundingBoxUV outline = legend2.Outline;
        double num14 = boundingBoxXyz.Min.X + num4;
        double num15 = boundingBoxXyz.Max.Y - num2;
        double num16 = drawingLegendInfo.trueWidth / 2.0;
        XYZ point = new XYZ(num14 + num16, num15 - drawingLegendInfo.trueHeight / 2.0, 0.0);
        if (!drawingLegendInfo.existingViewPort)
          Viewport.Create(document, viewSheet1.Id, legend2.Id, point);
        if (elem == null)
          new FilteredElementCollector(document, viewSheet1.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).ToList<FamilyInstance>().FirstOrDefault<FamilyInstance>();
        Parameter parameter23 = Parameters.LookupParameter((Element) viewSheet1, "SHT_DRAWING_COVERS");
        if (parameter23 != null)
        {
          string str10 = "MASTER INSULATION SHEET - ";
          string str11 = !(drawingLegendInfo.minMark != drawingLegendInfo.maxMark) ? str10 + drawingLegendInfo.minMark : $"{str10}{drawingLegendInfo.minMark}-{drawingLegendInfo.maxMark}";
          parameter23.Set(str11);
        }
        if (!this.GetUserInitials(viewSheet1))
        {
          int num17 = (int) transaction.RollBack();
          return (Result) 1;
        }
      }
      int num18 = (int) transaction.Commit();
    }
    TaskDialog taskDialog3 = new TaskDialog("Insulation Drawing - Master");
    if (stringList3.Count == 0)
    {
      string str12 = string.Empty;
      string empty = string.Empty;
      taskDialog3.MainInstruction = "Insulation Drawing - Master completed successfully.";
      str12 = "Master insulation sheets and legends were generated or updated for all valid insulation in the project.";
      string str13 = string.IsNullOrEmpty(str3) ? "Master insulation sheets and legends were generated or updated for all valid insulation in the project." : $"Master insulation sheets and legends were generated or updated for insulation marks above the previous highest mark. ({str3})";
      taskDialog3.MainContent = str13;
      foreach (string key in dictionary5.Keys)
      {
        string str14 = $"{{{key}}} - {{{dictionary5[key]}}}";
        TaskDialog taskDialog4 = taskDialog3;
        taskDialog4.ExpandedContent = $"{taskDialog4.ExpandedContent}{str14}\n";
      }
    }
    else if ((double) stringList3.Count == count)
    {
      taskDialog3.MainInstruction = "Insulation Drawing - Master Failed";
      taskDialog3.MainContent = "No insulation was drawn and no sheets or legends were created or updated.";
      viewSheet3 = (ViewSheet) null;
    }
    else
    {
      taskDialog3.MainInstruction = "Insulation Drawing - Master completed drawings.";
      taskDialog3.MainContent = "Master insulation sheets and legends were created or updated with insulation drawings. One or more of the insulation marks failed to be drawn.";
      taskDialog3.ExpandedContent = "Failed Marks:\n";
      foreach (string str15 in stringList3)
      {
        TaskDialog taskDialog5 = taskDialog3;
        taskDialog5.ExpandedContent = $"{taskDialog5.ExpandedContent}{str15}\n";
      }
    }
    taskDialog3.Show();
    if (viewSheet3 != null)
      activeUiDocument.RequestViewChange((Autodesk.Revit.DB.View) viewSheet3);
    return (Result) 0;
  }

  private ViewSheet CreateNewSheet(FamilySymbol titleBlockSymbol, int sheetCount, int sheetNumber)
  {
    ViewSheet sheet = ViewSheet.Create(titleBlockSymbol.Document, titleBlockSymbol.Id);
    this.nameSheet(sheet, sheetCount);
    this.setSheetNumber(sheet, sheetNumber);
    return sheet;
  }

  private void nameSheet(ViewSheet sheet, int sheetCount)
  {
    try
    {
      string empty = string.Empty;
      string str = sheetCount >= 10 ? "INS - " + sheetCount.ToString() : "INS - 0" + sheetCount.ToString();
      sheet.Name = str;
    }
    catch (Exception ex)
    {
      this.nameSheet(sheet, sheetCount++);
    }
  }

  private void setSheetNumber(ViewSheet sheet, int sheetNumber)
  {
    try
    {
      sheet.SheetNumber = sheetNumber.ToString();
      this.SheetNumbers.Add(sheetNumber);
    }
    catch (Autodesk.Revit.Exceptions.ArgumentException ex)
    {
      if (!ex.Message.Contains("Sheet number is already in use."))
        return;
      this.setSheetNumber(sheet, sheetNumber + 1);
    }
  }

  private bool GetUserInitials(ViewSheet sheet)
  {
    Parameter parameter = Parameters.LookupParameter((Element) sheet, "Drawn By");
    string parameterAsString = Parameters.GetParameterAsString((Element) sheet, "Drawn By");
    if (parameter != null && parameterAsString == "Author")
    {
      if (string.IsNullOrEmpty(this.userInitials))
      {
        this.userInitials = string.Empty;
        DataCollectorForm300x150 collectorForm300x150 = new DataCollectorForm300x150();
        collectorForm300x150.Text = "User Initials";
        collectorForm300x150.label.Text = "Enter your Initials below for insertion into the title block via the Drawn By parameter on the created sheets.";
        if (collectorForm300x150.ShowDialog() == DialogResult.Cancel)
        {
          new TaskDialog("Error")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainContent = "You must provide user initials to continue.  Please run this tool again and provide the required user initials"
          }.Show();
          this.userInitials = "";
          return false;
        }
        if (!string.IsNullOrWhiteSpace(collectorForm300x150.textBox1.Text))
        {
          this.userInitials = collectorForm300x150.textBox1.Text;
        }
        else
        {
          new TaskDialog("Error")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainContent = "You must provide user initials to continue.  Please run this tool again and provide the required user initials"
          }.Show();
          return false;
        }
      }
      parameter.Set(this.userInitials);
    }
    return true;
  }

  private void nameLegend(Autodesk.Revit.DB.View legend, string min, string max)
  {
    this.GetNameLegend(legend, min, max);
  }

  private string GetNameLegend(Autodesk.Revit.DB.View legend, string min, string max)
  {
    string str1 = "INSULATION DETAIL - ";
    string str2 = !(min != max) ? str1 + min : $"{str1}{min}-{max}";
    string str3 = "";
    int num = 1;
    while (legend.Name != (str2 + str3).Trim())
    {
      try
      {
        legend.Name = (str2 + str3).Trim();
      }
      catch
      {
        str3 = $" - ({num++.ToString()})";
      }
    }
    return legend.Name;
  }

  private bool IsNumeric(string str) => double.TryParse(str, out double _);
}
