// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.EDrawingAutoDim_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.TicketTools.AutoDimensioning.AutoDimUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.AdminUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

[Transaction(TransactionMode.Manual)]
public class EDrawingAutoDim_Command : IExternalCommand
{
  public Dictionary<string, List<string>> insufficientDimLinesGridsLevelsList = new Dictionary<string, List<string>>();
  public List<string> invisibleElemsList = new List<string>();
  public Autodesk.Revit.DB.Dimension previousDimension;
  public BoundingBoxXYZ cropBoxBB;
  public BoundingBoxXYZ cropPlusClipBB;
  public BoundingBoxXYZ cropPlusPlanBB;
  public Transform cropBoxTransform;
  public Transform cropBoxInverseTransform;
  public bool shortCurveError;
  public List<AutoTicketSettingsTools> eDrawingSettings = new List<AutoTicketSettingsTools>();

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application1 = commandData.Application;
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Application application2 = commandData.Application.Application;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Erection Drawing Auto-dimensioning must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Erection Drawing Auto-dimensioning must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    if (revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.AreaPlan && revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.CeilingPlan && revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.EngineeringPlan && revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.FloorPlan && revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.Elevation && revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.Section && revitDoc.ActiveView.ViewType != Autodesk.Revit.DB.ViewType.Detail)
    {
      TaskDialog.Show("Error", "Current View is not an Acceptable View for Dimensioning");
      return (Result) 1;
    }
    if (revitDoc.ActiveView.AssociatedAssemblyInstanceId != Autodesk.Revit.DB.ElementId.InvalidElementId)
    {
      TaskDialog.Show("Error", "Current View is not an Acceptable View for Dimensioning");
      return (Result) 1;
    }
    Autodesk.Revit.DB.ElementMulticategoryFilter filter1 = new Autodesk.Revit.DB.ElementMulticategoryFilter((ICollection<Autodesk.Revit.DB.BuiltInCategory>) new List<Autodesk.Revit.DB.BuiltInCategory>()
    {
      Autodesk.Revit.DB.BuiltInCategory.OST_Grids,
      Autodesk.Revit.DB.BuiltInCategory.OST_Levels
    });
    if (new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).WherePasses((Autodesk.Revit.DB.ElementFilter) filter1).ToList<Autodesk.Revit.DB.Element>().Count == 0)
    {
      TaskDialog.Show("Error", "Current View does not conatain any Grids or Levels. Grids and/or Levels are required for dimensioning.");
      return (Result) 1;
    }
    ICollection<Autodesk.Revit.DB.ElementId> elementIds1 = activeUiDocument.Selection.GetElementIds();
    Autodesk.Revit.UI.Selection.Selection selection = application1.ActiveUIDocument.Selection;
    bool flag1 = false;
    if (elementIds1.Count != 0)
      flag1 = true;
    List<Autodesk.Revit.DB.Element> collection = new List<Autodesk.Revit.DB.Element>();
    List<Autodesk.Revit.DB.Element> elementList1 = new List<Autodesk.Revit.DB.Element>();
    Dictionary<string, List<Autodesk.Revit.DB.Element>> dictionary1 = new Dictionary<string, List<Autodesk.Revit.DB.Element>>();
    AutoDimforEDWGsManager dimforEdwGsManager = (AutoDimforEDWGsManager) null;
    List<AutoTicketAppendStringParameterData> appendStringData;
    this.eDrawingSettings = AutoTicketSettingsReader.ReaderforAutoTicketSettings(revitDoc, "AUTO-DIMENSION E-DRAWING", false, out appendStringData);
    AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(revitDoc);
    int num1 = 0;
    if (flag1)
    {
      Autodesk.Revit.DB.ElementMulticategoryFilter filter2 = new Autodesk.Revit.DB.ElementMulticategoryFilter((ICollection<Autodesk.Revit.DB.BuiltInCategory>) new List<Autodesk.Revit.DB.BuiltInCategory>()
      {
        Autodesk.Revit.DB.BuiltInCategory.OST_Grids,
        Autodesk.Revit.DB.BuiltInCategory.OST_Levels
      });
      List<Autodesk.Revit.DB.Element> list1 = new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, elementIds1).WherePasses((Autodesk.Revit.DB.ElementFilter) filter2).ToList<Autodesk.Revit.DB.Element>();
      collection.AddRange((IEnumerable<Autodesk.Revit.DB.Element>) list1);
      num1 = elementIds1.Count - collection.Count;
      Autodesk.Revit.DB.ElementMulticategoryFilter filter3 = new Autodesk.Revit.DB.ElementMulticategoryFilter((ICollection<Autodesk.Revit.DB.BuiltInCategory>) new List<Autodesk.Revit.DB.BuiltInCategory>()
      {
        Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming,
        Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel,
        Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment,
        Autodesk.Revit.DB.BuiltInCategory.OST_StructConnections
      });
      List<Autodesk.Revit.DB.Element> list2 = new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, elementIds1).WherePasses((Autodesk.Revit.DB.ElementFilter) filter3).ToList<Autodesk.Revit.DB.Element>();
      dimforEdwGsManager = new AutoDimforEDWGsManager(revitDoc, list2);
      if (collection.Count + dimforEdwGsManager.groupingDictionary.Keys.Count == 0)
      {
        new TaskDialog("EDGE^R")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "No Valid Elements for Dimensioning",
          MainContent = "Please select valid items for dimensioning as part of your selection group in order to run Auto Dimensioing for Erection Drawings."
        }.Show();
        return (Result) 1;
      }
    }
    else
    {
      GridAndLevelFilter gridAndLevelFilter = new GridAndLevelFilter();
      try
      {
        List<Reference> list = activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) gridAndLevelFilter, "Pick grids and/or levels to dimension to").ToList<Reference>();
        if (list.Count == 0)
        {
          new TaskDialog("EDGE^R")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "No Grid or Level Selected",
            MainContent = "Please select a grid or a level in order to run Auto Dimensioning for Erection Drawings"
          }.Show();
          return (Result) 1;
        }
        foreach (Reference reference in list)
          collection.Add(revitDoc.GetElement(reference));
      }
      catch
      {
        return (Result) 1;
      }
    }
    List<XYZ> xyzList = new List<XYZ>();
    if (collection.Count == 0)
    {
      GridAndLevelFilter gridAndLevelFilter = new GridAndLevelFilter();
      try
      {
        List<Reference> list3 = activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) gridAndLevelFilter, "Pick grids and/or levels to dimension to").ToList<Reference>();
        if (list3.Count == 0)
        {
          new TaskDialog("EDGE^R")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "No Grid or Level Selected",
            MainContent = "Please select a grid or a level in order to run Auto Dimensioning for Erection Drawings"
          }.Show();
          return (Result) 1;
        }
        foreach (Reference reference in list3)
          collection.Add(revitDoc.GetElement(reference));
        foreach (Autodesk.Revit.DB.Element element in collection)
        {
          XYZ xyz1 = new XYZ();
          if (element is Autodesk.Revit.DB.Grid)
          {
            Autodesk.Revit.DB.Grid grid = element as Autodesk.Revit.DB.Grid;
            Curve curve = grid.GetCurvesInView(DatumExtentType.Model, revitDoc.ActiveView).First<Curve>();
            if (grid.CanBeVisibleInView(revitDoc.ActiveView))
            {
              if (!(curve is Arc) && curve is Line)
              {
                XYZ direction = (curve as Line).Direction;
                bool flag2 = true;
                foreach (XYZ xyz2 in xyzList)
                {
                  if (Math.Abs(xyz2.X).ApproximatelyEquals(Math.Abs(direction.X)) && Math.Abs(xyz2.Y).ApproximatelyEquals(Math.Abs(direction.Y)) && Math.Abs(xyz2.Z).ApproximatelyEquals(Math.Abs(direction.Z)))
                    flag2 = false;
                }
                if (flag2)
                  xyzList.Add(direction);
              }
            }
            else if (!this.invisibleElemsList.Contains($"{element.Id?.ToString()} - {element.Name}"))
              this.invisibleElemsList.Add($"{element.Id?.ToString()} - {element.Name}");
          }
          else if (element is Autodesk.Revit.DB.Level)
          {
            Autodesk.Revit.DB.Level level = element as Autodesk.Revit.DB.Level;
            if (level.CanBeVisibleInView(revitDoc.ActiveView))
            {
              List<Curve> list4 = level.GetCurvesInView(DatumExtentType.Model, revitDoc.ActiveView).ToList<Curve>();
              if (list4.Count != 0 && list4.Count <= 1 && list4[0] is Line)
              {
                XYZ direction = (list4[0] as Line).Direction;
                bool flag3 = true;
                foreach (XYZ xyz3 in xyzList)
                {
                  if (Math.Abs(xyz3.X).ApproximatelyEquals(Math.Abs(direction.X)) && Math.Abs(xyz3.Y).ApproximatelyEquals(Math.Abs(direction.Y)) && Math.Abs(xyz3.Z).ApproximatelyEquals(Math.Abs(direction.Z)))
                    flag3 = false;
                }
                if (flag3)
                  xyzList.Add(direction);
              }
            }
            else if (!this.invisibleElemsList.Contains($"{element.Id?.ToString()} - {element.Name}"))
              this.invisibleElemsList.Add($"{element.Id?.ToString()} - {element.Name}");
          }
        }
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        return (Result) 1;
      }
    }
    else
    {
      foreach (Autodesk.Revit.DB.Element element in collection)
      {
        XYZ xyz4 = new XYZ();
        if (element is Autodesk.Revit.DB.Grid)
        {
          Autodesk.Revit.DB.Grid grid = element as Autodesk.Revit.DB.Grid;
          Curve curve = grid.GetCurvesInView(DatumExtentType.Model, revitDoc.ActiveView).First<Curve>();
          if (grid.CanBeVisibleInView(revitDoc.ActiveView))
          {
            if (!(curve is Arc) && curve is Line)
            {
              XYZ direction = (curve as Line).Direction;
              bool flag4 = true;
              foreach (XYZ xyz5 in xyzList)
              {
                if (Math.Abs(xyz5.X).ApproximatelyEquals(Math.Abs(direction.X)) && Math.Abs(xyz5.Y).ApproximatelyEquals(Math.Abs(direction.Y)) && Math.Abs(xyz5.Z).ApproximatelyEquals(Math.Abs(direction.Z)))
                  flag4 = false;
              }
              if (flag4)
                xyzList.Add(direction);
            }
          }
          else if (!this.invisibleElemsList.Contains($"{element.Id?.ToString()} - {element.Name}"))
            this.invisibleElemsList.Add($"{element.Id?.ToString()} - {element.Name}");
        }
        else if (element is Autodesk.Revit.DB.Level)
        {
          Autodesk.Revit.DB.Level level = element as Autodesk.Revit.DB.Level;
          if (level.CanBeVisibleInView(revitDoc.ActiveView))
          {
            List<Curve> list = level.GetCurvesInView(DatumExtentType.Model, revitDoc.ActiveView).ToList<Curve>();
            if (list.Count != 0 && list.Count <= 1 && list[0] is Line)
            {
              XYZ direction = (list[0] as Line).Direction;
              bool flag5 = true;
              foreach (XYZ xyz6 in xyzList)
              {
                if (Math.Abs(xyz6.X).ApproximatelyEquals(Math.Abs(direction.X)) && Math.Abs(xyz6.Y).ApproximatelyEquals(Math.Abs(direction.Y)) && Math.Abs(xyz6.Z).ApproximatelyEquals(Math.Abs(direction.Z)))
                  flag5 = false;
              }
              if (flag5)
                xyzList.Add(direction);
            }
          }
          else if (!this.invisibleElemsList.Contains($"{element.Id?.ToString()} - {element.Name}"))
            this.invisibleElemsList.Add($"{element.Id?.ToString()} - {element.Name}");
        }
      }
    }
    if (xyzList.Count == 0)
    {
      new TaskDialog("EDGE^R")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "No Principal Axes Found",
        MainContent = "Please select grid or levels that contain a direction."
      }.Show();
      return (Result) 1;
    }
    List<Reference> referenceList1 = new List<Reference>();
    bool flag6 = false;
    bool flag7 = false;
    bool flag8 = false;
    bool flag9 = false;
    if (flag1 && dimforEdwGsManager.groupingDictionary.Keys.Count == 0)
    {
      if (collection.Count != 0)
      {
        flag7 = true;
        if (dimforEdwGsManager.itemCt < num1)
          new TaskDialog("EDGE^R")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Ineligible elements in selection removed",
            MainContent = "Ineligible elements were detected in the selection group. Only grids, levels, EDGE structural framing category families, and generic model/ specialty equipment category families containing EDGE Dim Lines are supported. All other elements included in the selection group will not be processed or dimensioned to."
          }.Show();
      }
      else
      {
        new TaskDialog("EDGE^R")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "No Valid Elements for Dimensioning",
          MainContent = "Please select valid items for dimensioning as part of your selection group in order to run Auto Dimensioing for Erection Drawings."
        }.Show();
        return (Result) 1;
      }
    }
    else if (flag1)
    {
      dictionary1 = dimforEdwGsManager.groupingDictionary;
      if (dimforEdwGsManager.itemCt < num1)
        new TaskDialog("EDGE^R")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "Ineligible elements in selection removed",
          MainContent = "Ineligible elements were detected in the selection group. Only grids, levels, EDGE structural framing category families, and generic model/ specialty equipment category families containing EDGE Dim Lines are supported. All other elements included in the selection group will not be processed or dimensioned to."
        }.Show();
      if (dictionary1.Keys.Count > 1)
      {
        flag8 = true;
        TaskDialog taskDialog = new TaskDialog("Multiple Groupingss Selected");
        taskDialog.Id = "ID_MultipleGroupings";
        taskDialog.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
        taskDialog.Title = "Multiple Groupings Selected";
        taskDialog.TitleAutoPrefix = true;
        taskDialog.AllowCancellation = false;
        taskDialog.MainInstruction = "Multiple Groupings Selected";
        taskDialog.MainContent = "Multiple groupings have been detected as a part of your selection group. Choosing to place as one Dimension String will dimension to all selected elements in one dimension string that will not include an append string. Choosing the Separate Dimension Strings option will place a dimension string per grouping of like elements. Please select one of the following options for Dimension String placement:";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Place as one Dimension String");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Place as separate Dimension Strings");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          flag6 = false;
        }
        else
        {
          if (taskDialogResult != 1002)
            return (Result) 1;
          flag6 = true;
        }
      }
    }
    else
    {
      TaskDialog taskDialog = new TaskDialog("Dimension Only To Grids/Levels");
      taskDialog.Id = "ID_MultipleGroupings";
      taskDialog.MainIcon = (TaskDialogIcon) 0;
      taskDialog.Title = "Dimension To Grids/Levels";
      taskDialog.TitleAutoPrefix = true;
      taskDialog.AllowCancellation = false;
      taskDialog.MainInstruction = "Dimension only to grids and/or levels?";
      taskDialog.MainContent = "Would you like to only dimension to the grids/levels?";
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Only Dimension to Grids/Levels");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Select Additional Elements to Dimension");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
        flag9 = true;
      else if (taskDialogResult == 1002)
      {
        bool flag10 = true;
        while (flag10)
        {
          try
          {
            Reference reference = activeUiDocument.Selection.PickObject((ObjectType) 1, (ISelectionFilter) new DimensionableItemsFilterEDrawing(), "Pick additional elements to dimension");
            Autodesk.Revit.DB.Element element = revitDoc.GetElement(reference);
            referenceList1.Add(reference);
            if (referenceList1.Count == 0)
              return (Result) 1;
            IList<Reference> referenceList2 = (IList<Reference>) new List<Reference>();
            if (element is Autodesk.Revit.DB.FamilyInstance)
            {
              Autodesk.Revit.DB.FamilyInstance elem = element as Autodesk.Revit.DB.FamilyInstance;
              string controlMark = elem.GetControlMark();
              if (!string.IsNullOrWhiteSpace(controlMark))
              {
                foreach (Autodesk.Revit.DB.ElementId elementId in (IEnumerable<Autodesk.Revit.DB.ElementId>) new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).OfClass(typeof (Autodesk.Revit.DB.FamilyInstance)).ToElementIds())
                {
                  if (revitDoc.GetElement(elementId).GetControlMark().Equals(controlMark))
                    referenceList2.Add(new Reference(revitDoc.GetElement(elementId)));
                }
              }
              else if (elem.Symbol != null)
              {
                string str = elem.Symbol.Name + elem.Symbol.FamilyName;
              }
            }
            if (true)
            {
              referenceList2.Clear();
              referenceList2.Add(reference);
            }
            List<Autodesk.Revit.DB.ElementId> list = activeUiDocument.Selection.GetElementIds().ToList<Autodesk.Revit.DB.ElementId>();
            list.Add(reference.ElementId);
            activeUiDocument.Selection.SetElementIds((ICollection<Autodesk.Revit.DB.ElementId>) list);
            referenceList1 = revitDoc.GetElement(reference.ElementId) is Autodesk.Revit.DB.Grid || revitDoc.GetElement(reference.ElementId) is Autodesk.Revit.DB.Level ? activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new GridAndLevelFilter(), "Pick additional Grids or Levels To Dimension", (IList<Reference>) new List<Reference>()).ToList<Reference>() : (!revitDoc.GetElement(reference.ElementId).Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming)) ? (!revitDoc.GetElement(reference.ElementId).Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel)) && !revitDoc.GetElement(reference.ElementId).Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment)) || !Parameters.GetParameterAsString(revitDoc.GetElement(reference.ElementId), "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID") ? activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new SameControlMarkFilterEDrawing(revitDoc.GetElement(reference.ElementId)), "Pick additional Elements To Dimension", referenceList2).ToList<Reference>() : activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new ValidVoids(), "Pick additional Voids To Dimension", referenceList2).ToList<Reference>()) : activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new ValidStructFraming(), "Pick additional Structural Framing Elements To Dimension", referenceList2).ToList<Reference>());
            flag10 = false;
            if (referenceList1.Count == 0)
            {
              TaskDialog.Show("Error", "Please select at least one valid element (such as structural framing, voids, or any other valid dimensioning item) for proper dimension placement.");
              return (Result) 1;
            }
          }
          catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
          {
            return (Result) 1;
          }
        }
      }
    }
    List<Autodesk.Revit.DB.ElementId> list5 = new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).OfClass(typeof (Autodesk.Revit.DB.FamilyInstance)).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming).ToElementIds().ToList<Autodesk.Revit.DB.ElementId>();
    Autodesk.Revit.DB.ElementMulticategoryFilter filter4 = new Autodesk.Revit.DB.ElementMulticategoryFilter((ICollection<Autodesk.Revit.DB.BuiltInCategory>) new List<Autodesk.Revit.DB.BuiltInCategory>()
    {
      Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel,
      Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment,
      Autodesk.Revit.DB.BuiltInCategory.OST_StructConnections
    });
    List<Autodesk.Revit.DB.ElementId> list6 = new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).WherePasses((Autodesk.Revit.DB.ElementFilter) filter4).ToElementIds().ToList<Autodesk.Revit.DB.ElementId>();
    Autodesk.Revit.DB.ElementMulticategoryFilter filter5 = new Autodesk.Revit.DB.ElementMulticategoryFilter((ICollection<Autodesk.Revit.DB.BuiltInCategory>) new List<Autodesk.Revit.DB.BuiltInCategory>()
    {
      Autodesk.Revit.DB.BuiltInCategory.OST_Grids,
      Autodesk.Revit.DB.BuiltInCategory.OST_Levels
    });
    List<Autodesk.Revit.DB.ElementId> list7 = new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).WherePasses((Autodesk.Revit.DB.ElementFilter) filter5).ToElementIds().ToList<Autodesk.Revit.DB.ElementId>();
    if (revitDoc.ActiveView.CropBoxActive)
    {
      this.cropBoxBB = revitDoc.ActiveView.CropBox;
      this.cropBoxTransform = revitDoc.ActiveView.CropBox.Transform;
      this.cropBoxInverseTransform = revitDoc.ActiveView.CropBox.Transform.Inverse;
      switch (revitDoc.ActiveView.ViewType)
      {
        case Autodesk.Revit.DB.ViewType.FloorPlan:
        case Autodesk.Revit.DB.ViewType.CeilingPlan:
        case Autodesk.Revit.DB.ViewType.EngineeringPlan:
        case Autodesk.Revit.DB.ViewType.AreaPlan:
          if (revitDoc.ActiveView is ViewPlan)
          {
            Autodesk.Revit.DB.PlanViewRange viewRange = (revitDoc.ActiveView as ViewPlan).GetViewRange();
            Autodesk.Revit.DB.ElementId levelId1 = viewRange.GetLevelId(PlanViewPlane.TopClipPlane);
            Autodesk.Revit.DB.ElementId levelId2 = viewRange.GetLevelId(PlanViewPlane.ViewDepthPlane);
            Autodesk.Revit.DB.Level element1 = revitDoc.GetElement(levelId1) as Autodesk.Revit.DB.Level;
            Autodesk.Revit.DB.Level element2 = revitDoc.GetElement(levelId2) as Autodesk.Revit.DB.Level;
            double z1 = levelId1.IntegerValue <= 0 ? double.MaxValue : element1.ProjectElevation + viewRange.GetOffset(PlanViewPlane.TopClipPlane);
            double z2 = levelId2.IntegerValue <= 0 ? double.MinValue : element2.ProjectElevation + viewRange.GetOffset(PlanViewPlane.ViewDepthPlane);
            XYZ max = this.cropBoxBB.Max;
            XYZ min = this.cropBoxBB.Min;
            this.cropPlusPlanBB = new BoundingBoxXYZ();
            XYZ xyz7 = new XYZ(max.X, max.Y, z1);
            XYZ xyz8 = new XYZ(min.X, min.Y, z2);
            this.cropPlusPlanBB.Max = xyz7;
            this.cropPlusPlanBB.Min = xyz8;
            this.cropBoxInverseTransform = this.cropPlusPlanBB.Transform.Inverse;
            break;
          }
          break;
        case Autodesk.Revit.DB.ViewType.Elevation:
        case Autodesk.Revit.DB.ViewType.Section:
        case Autodesk.Revit.DB.ViewType.Detail:
          XYZ max1 = this.cropBoxBB.Max;
          XYZ min1 = this.cropBoxBB.Min;
          this.cropPlusClipBB = new BoundingBoxXYZ();
          XYZ xyz9 = new XYZ(max1.X, max1.Y, max1.Z);
          XYZ xyz10 = new XYZ(min1.X, min1.Y, min1.Z);
          this.cropPlusClipBB.Max = xyz9;
          this.cropPlusClipBB.Min = xyz10;
          this.cropBoxInverseTransform = this.cropPlusClipBB.Transform.Inverse;
          break;
      }
    }
    List<Autodesk.Revit.DB.Element> elementList2 = new List<Autodesk.Revit.DB.Element>();
    foreach (Reference reference in referenceList1)
    {
      Autodesk.Revit.DB.ElementId elementId = reference.ElementId;
      Autodesk.Revit.DB.Element element = revitDoc.GetElement(elementId);
      elementList2.Add(element);
    }
    if (!flag1 && elementList2.Count == 0)
      elementList2.AddRange((IEnumerable<Autodesk.Revit.DB.Element>) collection);
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    using (Autodesk.Revit.DB.TransactionGroup transactionGroup = new Autodesk.Revit.DB.TransactionGroup(revitDoc, "Erection Drawing Auto-Dimension"))
    {
      int num2 = (int) transactionGroup.Start();
      foreach (XYZ principalAxis in xyzList)
      {
        try
        {
          XYZ perpendicularAxis = principalAxis.CrossProduct(revitDoc.ActiveView.ViewDirection);
          DimPlacementCalculatorEDrawing placementCalculator = new DimPlacementCalculatorEDrawing(principalAxis, revitDoc.ActiveView, revitDoc);
          List<Autodesk.Revit.DB.ElementId> currGridLevels = new List<Autodesk.Revit.DB.ElementId>();
          foreach (Autodesk.Revit.DB.Element element in collection)
          {
            XYZ xyz = new XYZ();
            if (element is Autodesk.Revit.DB.Grid)
            {
              Curve curve = (element as Autodesk.Revit.DB.Grid).GetCurvesInView(DatumExtentType.Model, revitDoc.ActiveView).First<Curve>();
              if (!(curve is Arc) && curve is Line)
              {
                XYZ direction = (curve as Line).Direction;
                if (Math.Abs(direction.X).ApproximatelyEquals(Math.Abs(principalAxis.X)) && Math.Abs(direction.Y).ApproximatelyEquals(Math.Abs(principalAxis.Y)) && Math.Abs(direction.Z).ApproximatelyEquals(Math.Abs(principalAxis.Z)))
                  currGridLevels.Add(element.Id);
              }
            }
            else if (element is Autodesk.Revit.DB.Level)
            {
              List<Curve> list8 = (element as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
              if (list8.Count != 0 && list8.Count <= 1 && list8[0] is Line)
              {
                XYZ direction = (list8[0] as Line).Direction;
                if (Math.Abs(direction.X).ApproximatelyEquals(Math.Abs(principalAxis.X)) && Math.Abs(direction.Y).ApproximatelyEquals(Math.Abs(principalAxis.Y)) && Math.Abs(direction.Z).ApproximatelyEquals(Math.Abs(principalAxis.Z)))
                  currGridLevels.Add(element.Id);
              }
            }
          }
          if (flag9 && currGridLevels.Count < 2)
          {
            stringList1.Add(revitDoc.GetElement(currGridLevels[0]).Name);
          }
          else
          {
            if (flag7)
            {
              elementList2.Clear();
              foreach (Autodesk.Revit.DB.Element element in collection)
              {
                XYZ xyz = new XYZ();
                if (element is Autodesk.Revit.DB.Grid)
                {
                  Curve curve = (element as Autodesk.Revit.DB.Grid).GetCurvesInView(DatumExtentType.Model, revitDoc.ActiveView).First<Curve>();
                  if (!(curve is Arc) && curve is Line)
                  {
                    XYZ direction = (curve as Line).Direction;
                    if (Math.Abs(direction.X).ApproximatelyEquals(Math.Abs(principalAxis.X)) && Math.Abs(direction.Y).ApproximatelyEquals(Math.Abs(principalAxis.Y)) && Math.Abs(direction.Z).ApproximatelyEquals(Math.Abs(principalAxis.Z)))
                      elementList2.Add(element);
                  }
                }
                else if (element is Autodesk.Revit.DB.Level)
                {
                  List<Curve> list9 = (element as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
                  if (list9.Count != 0 && list9.Count <= 1 && list9[0] is Line)
                  {
                    XYZ direction = (list9[0] as Line).Direction;
                    if (Math.Abs(direction.X).ApproximatelyEquals(Math.Abs(principalAxis.X)) && Math.Abs(direction.Y).ApproximatelyEquals(Math.Abs(principalAxis.Y)) && Math.Abs(direction.Z).ApproximatelyEquals(Math.Abs(principalAxis.Z)))
                      elementList2.Add(element);
                  }
                }
              }
              if (currGridLevels.Count < 2)
              {
                stringList2.Add(revitDoc.GetElement(currGridLevels[0]).Name);
                continue;
              }
            }
            else if (flag1)
            {
              elementList2.Clear();
              foreach (string key in dictionary1.Keys)
              {
                foreach (Autodesk.Revit.DB.Element element in dictionary1[key])
                  elementList2.Add(element);
              }
            }
            bool flag11 = false;
            XYZ clickedPoint = XYZ.Zero;
            Dictionary<Autodesk.Revit.DB.ElementId, Autodesk.Revit.DB.OverrideGraphicSettings> dictionary2 = new Dictionary<Autodesk.Revit.DB.ElementId, Autodesk.Revit.DB.OverrideGraphicSettings>();
            using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(revitDoc, "Highlight Grids"))
            {
              int num3 = (int) transaction.Start();
              activeUiDocument.Selection.SetElementIds((ICollection<Autodesk.Revit.DB.ElementId>) currGridLevels);
              foreach (Autodesk.Revit.DB.ElementId elementId in currGridLevels)
              {
                if (!dictionary2.ContainsKey(elementId))
                  dictionary2.Add(elementId, revitDoc.ActiveView.GetElementOverrides(elementId));
              }
              Autodesk.Revit.DB.OverrideGraphicSettings overrideGraphicSettings = new Autodesk.Revit.DB.OverrideGraphicSettings();
              Color color = new Color((byte) 0, byte.MaxValue, (byte) 0);
              overrideGraphicSettings.SetCutForegroundPatternColor(color);
              overrideGraphicSettings.SetCutBackgroundPatternColor(color);
              overrideGraphicSettings.SetCutLineColor(color);
              overrideGraphicSettings.SetCutForegroundPatternVisible(true);
              overrideGraphicSettings.SetCutBackgroundPatternVisible(true);
              overrideGraphicSettings.SetSurfaceForegroundPatternColor(color);
              overrideGraphicSettings.SetSurfaceBackgroundPatternColor(color);
              overrideGraphicSettings.SetProjectionLineColor(color);
              overrideGraphicSettings.SetSurfaceForegroundPatternVisible(true);
              overrideGraphicSettings.SetSurfaceBackgroundPatternVisible(true);
              foreach (Autodesk.Revit.DB.ElementId elementId in currGridLevels)
                revitDoc.ActiveView.SetElementOverrides(elementId, overrideGraphicSettings);
              int num4 = (int) transaction.Commit();
            }
            if (revitDoc.ActiveView.SketchPlane != null)
            {
              try
              {
                clickedPoint = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 0, "Pick location for dimension placement");
              }
              catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
              {
                using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(revitDoc, "Highlight Grid"))
                {
                  int num5 = (int) transaction.Start();
                  foreach (Autodesk.Revit.DB.ElementId elementId in currGridLevels)
                    revitDoc.ActiveView.SetElementOverrides(elementId, dictionary2[elementId]);
                  int num6 = (int) transaction.Commit();
                }
                return (Result) 1;
              }
            }
            else
            {
              using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(revitDoc, " "))
              {
                int num7 = (int) transaction.Start();
                Autodesk.Revit.DB.Plane byNormalAndOrigin = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(activeUiDocument.ActiveView.ViewDirection, activeUiDocument.ActiveView.Origin);
                Autodesk.Revit.DB.SketchPlane sketchPlane = Autodesk.Revit.DB.SketchPlane.Create(revitDoc, byNormalAndOrigin);
                activeUiDocument.ActiveView.SketchPlane = sketchPlane;
                try
                {
                  clickedPoint = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 0, "Pick location for dimension placement");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {
                  int num8 = (int) transactionGroup.RollBack();
                  return (Result) 1;
                }
                int num9 = (int) transaction.RollBack();
              }
            }
            placementCalculator.clickPoint = clickedPoint;
            placementCalculator.CalculateSnapItems(clickedPoint);
            using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(revitDoc, "Highlight Grid"))
            {
              int num10 = (int) transaction.Start();
              foreach (Autodesk.Revit.DB.ElementId elementId in currGridLevels)
                revitDoc.ActiveView.SetElementOverrides(elementId, dictionary2[elementId]);
              int num11 = (int) transaction.Commit();
            }
            if (!flag11)
            {
              List<string> stringList3 = new List<string>();
              if (false)
              {
                new TaskDialog("EDGE^R")
                {
                  MainInstruction = "One or more selected elements did not contain EDGE dim lines that could be used in the chosen view/side.",
                  MainContent = "Try another angle or view or add usable EDGE Dim lines."
                }.Show();
              }
              else
              {
                using (Autodesk.Revit.DB.Transaction transaction = new Autodesk.Revit.DB.Transaction(revitDoc, "Erection Drawing Auto-Dimension"))
                {
                  int num12 = (int) transaction.Start();
                  activeUiDocument.Selection.SetElementIds((ICollection<Autodesk.Revit.DB.ElementId>) currGridLevels);
                  bool eqForm = false;
                  if (new TaskDialog("EDGE^R")
                  {
                    FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                    AllowCancellation = false,
                    MainInstruction = "Use Equality Formula?",
                    ExpandedContent = "If you answer yes to this question, the Equality Display will be set to \"Equality Formula\"",
                    CommonButtons = ((TaskDialogCommonButtons) 6)
                  }.Show() == 6)
                    eqForm = true;
                  if (flag6)
                  {
                    List<ReferencedPoint> overallPointCloud = new List<ReferencedPoint>();
                    foreach (string key in dictionary1.Keys)
                    {
                      List<Autodesk.Revit.DB.ElementId> partIds = new List<Autodesk.Revit.DB.ElementId>();
                      List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
                      List<Autodesk.Revit.DB.ElementId> partIdsStructFraming = new List<Autodesk.Revit.DB.ElementId>();
                      List<Autodesk.Revit.DB.ElementId> partIdsVoids = new List<Autodesk.Revit.DB.ElementId>();
                      foreach (Autodesk.Revit.DB.Element element in dictionary1[key])
                      {
                        Autodesk.Revit.DB.ElementId id = element.Id;
                        switch (this.CheckElementType(element))
                        {
                          case DimElementType.SF:
                            element.GetSubComponentIds();
                            foreach (Autodesk.Revit.DB.ElementId elementId in (IEnumerable<Autodesk.Revit.DB.ElementId>) element.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().OrderBy<Autodesk.Revit.DB.ElementId, string>((Func<Autodesk.Revit.DB.ElementId, string>) (e => (revitDoc.GetElement(e) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name)))
                            {
                              if (list5.Contains(elementId))
                                partIdsStructFraming.Add(elementId);
                            }
                            if (list5.Contains(id))
                            {
                              partIdsStructFraming.Add(id);
                              continue;
                            }
                            continue;
                          case DimElementType.Void:
                            if (list6.Contains(id))
                            {
                              partIdsVoids.Add(id);
                              continue;
                            }
                            continue;
                          case DimElementType.GridLevel:
                            if (list7.Contains(id))
                            {
                              elementIdList.Add(id);
                              continue;
                            }
                            continue;
                          case DimElementType.DimItems:
                            if (list6.Contains(id))
                            {
                              partIds.Add(id);
                              continue;
                            }
                            continue;
                          default:
                            continue;
                        }
                      }
                      overallPointCloud.AddRange((IEnumerable<ReferencedPoint>) this.RetrieveOverallPointCloud(partIds, partIdsStructFraming, partIdsVoids, currGridLevels, principalAxis, perpendicularAxis, revitDoc, revitDoc.ActiveView));
                    }
                    foreach (string key in dictionary1.Keys)
                    {
                      if (!key.Equals("VOID"))
                        key.Equals("SF");
                      List<Autodesk.Revit.DB.ElementId> elementIds2 = new List<Autodesk.Revit.DB.ElementId>();
                      List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
                      List<Autodesk.Revit.DB.ElementId> elementIds3 = new List<Autodesk.Revit.DB.ElementId>();
                      List<Autodesk.Revit.DB.ElementId> elementIds4 = new List<Autodesk.Revit.DB.ElementId>();
                      foreach (Autodesk.Revit.DB.Element element in dictionary1[key])
                      {
                        Autodesk.Revit.DB.ElementId id = element.Id;
                        switch (this.CheckElementType(element))
                        {
                          case DimElementType.SF:
                            element.GetSubComponentIds();
                            foreach (Autodesk.Revit.DB.ElementId elementId in (IEnumerable<Autodesk.Revit.DB.ElementId>) element.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().OrderBy<Autodesk.Revit.DB.ElementId, string>((Func<Autodesk.Revit.DB.ElementId, string>) (e => (revitDoc.GetElement(e) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name)))
                            {
                              if (list5.Contains(elementId))
                                elementIds3.Add(elementId);
                            }
                            if (list5.Contains(id))
                            {
                              elementIds3.Add(id);
                              continue;
                            }
                            string str1 = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                            if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str1}"))
                            {
                              this.invisibleElemsList.Add($"{id?.ToString()} - {str1}");
                              continue;
                            }
                            continue;
                          case DimElementType.Void:
                            if (list6.Contains(id))
                            {
                              elementIds4.Add(id);
                              continue;
                            }
                            string str2 = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                            if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str2}"))
                            {
                              this.invisibleElemsList.Add($"{id?.ToString()} - {str2}");
                              continue;
                            }
                            continue;
                          case DimElementType.GridLevel:
                            if (list7.Contains(id))
                            {
                              elementIdList.Add(id);
                              continue;
                            }
                            if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {revitDoc.GetElement(id).Name}"))
                            {
                              this.invisibleElemsList.Add($"{id?.ToString()} - {revitDoc.GetElement(id).Name}");
                              continue;
                            }
                            continue;
                          case DimElementType.DimItems:
                            if (list6.Contains(id))
                            {
                              elementIds2.Add(id);
                              continue;
                            }
                            string str3 = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                            if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str3}"))
                            {
                              this.invisibleElemsList.Add($"{id?.ToString()} - {str3}");
                              continue;
                            }
                            continue;
                          default:
                            continue;
                        }
                      }
                      if (elementIds2.Count > 0)
                        this.DimensionEDrawingMultiGrouping(revitDoc, revitDoc.ActiveView, elementIds2, DimElementType.DimItems, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, overallPointCloud, placementCalculator, true, locationInFormValues, elementIds2.Count, appendStringData);
                      if (elementIdList.Count > 0)
                        this.DimensionEDrawingMultiGrouping(revitDoc, revitDoc.ActiveView, new List<Autodesk.Revit.DB.ElementId>(), DimElementType.GridLevel, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, overallPointCloud, placementCalculator, false, locationInFormValues, elementIds2.Count, appendStringData);
                      if (elementIds3.Count > 0)
                        this.DimensionEDrawingMultiGrouping(revitDoc, revitDoc.ActiveView, elementIds3, DimElementType.SF, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, overallPointCloud, placementCalculator, false, locationInFormValues, elementIds2.Count, appendStringData);
                      if (elementIds4.Count > 0)
                        this.DimensionEDrawingMultiGrouping(revitDoc, revitDoc.ActiveView, elementIds4, DimElementType.Void, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, overallPointCloud, placementCalculator, false, locationInFormValues, elementIds2.Count, appendStringData);
                      if (dictionary1[key].Count == 0)
                        this.DimensionEDrawingMultiGrouping(revitDoc, revitDoc.ActiveView, new List<Autodesk.Revit.DB.ElementId>(), DimElementType.GridLevel, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, overallPointCloud, placementCalculator, false, locationInFormValues, elementIds2.Count, appendStringData);
                    }
                    int num13 = (int) transaction.Commit();
                  }
                  else
                  {
                    List<Autodesk.Revit.DB.ElementId> elementIds5 = new List<Autodesk.Revit.DB.ElementId>();
                    List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
                    List<Autodesk.Revit.DB.ElementId> elementIds6 = new List<Autodesk.Revit.DB.ElementId>();
                    List<Autodesk.Revit.DB.ElementId> elementIds7 = new List<Autodesk.Revit.DB.ElementId>();
                    List<Autodesk.Revit.DB.ElementId> elementIds8 = new List<Autodesk.Revit.DB.ElementId>();
                    foreach (Autodesk.Revit.DB.Element element in elementList2)
                    {
                      Autodesk.Revit.DB.ElementId id = element.Id;
                      if (((!flag1 ? 0 : (!flag6 ? 1 : 0)) & (flag8 ? 1 : 0)) != 0)
                      {
                        Autodesk.Revit.DB.ElementMulticategoryFilter filter6 = new Autodesk.Revit.DB.ElementMulticategoryFilter((ICollection<Autodesk.Revit.DB.BuiltInCategory>) new List<Autodesk.Revit.DB.BuiltInCategory>()
                        {
                          Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming,
                          Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel,
                          Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment,
                          Autodesk.Revit.DB.BuiltInCategory.OST_StructConnections
                        });
                        if (new Autodesk.Revit.DB.FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).WherePasses((Autodesk.Revit.DB.ElementFilter) filter6).ToElementIds().ToList<Autodesk.Revit.DB.ElementId>().Contains(id))
                        {
                          if (this.CheckElementType(element) == DimElementType.SF)
                          {
                            element.GetSubComponentIds();
                            foreach (Autodesk.Revit.DB.ElementId elementId in (IEnumerable<Autodesk.Revit.DB.ElementId>) element.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().OrderBy<Autodesk.Revit.DB.ElementId, string>((Func<Autodesk.Revit.DB.ElementId, string>) (e => (revitDoc.GetElement(e) as Autodesk.Revit.DB.FamilyInstance).Symbol.Name)))
                            {
                              if (list5.Contains(elementId))
                                elementIds8.Add(elementId);
                            }
                          }
                          elementIds8.Add(id);
                        }
                        else
                        {
                          string str = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                          if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str}"))
                            this.invisibleElemsList.Add($"{id?.ToString()} - {str}");
                        }
                      }
                      switch (this.CheckElementType(element))
                      {
                        case DimElementType.SF:
                          element.GetSubComponentIds();
                          foreach (Autodesk.Revit.DB.ElementId elementId in (IEnumerable<Autodesk.Revit.DB.ElementId>) element.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().OrderByDescending<Autodesk.Revit.DB.ElementId, string>((Func<Autodesk.Revit.DB.ElementId, string>) (e => (revitDoc.GetElement(e) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name)))
                          {
                            if (list5.Contains(elementId))
                              elementIds6.Add(elementId);
                          }
                          if (list5.Contains(id))
                          {
                            elementIds6.Add(id);
                            continue;
                          }
                          string str4 = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                          if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str4}"))
                          {
                            this.invisibleElemsList.Add($"{id?.ToString()} - {str4}");
                            continue;
                          }
                          continue;
                        case DimElementType.Void:
                          if (list6.Contains(id))
                          {
                            elementIds7.Add(id);
                            continue;
                          }
                          string str5 = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                          if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str5}"))
                          {
                            this.invisibleElemsList.Add($"{id?.ToString()} - {str5}");
                            continue;
                          }
                          continue;
                        case DimElementType.GridLevel:
                          if (list7.Contains(id))
                          {
                            elementIdList.Add(id);
                            continue;
                          }
                          if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {revitDoc.GetElement(id).Name}"))
                          {
                            this.invisibleElemsList.Add($"{id?.ToString()} - {revitDoc.GetElement(id).Name}");
                            continue;
                          }
                          continue;
                        case DimElementType.DimItems:
                          if (list6.Contains(id))
                          {
                            elementIds5.Add(id);
                            continue;
                          }
                          string str6 = !(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name.Equals((revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name.ToString()) ? $"{(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Symbol.Family.Name} - {(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name}" : (revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance).Name;
                          if (!this.invisibleElemsList.Contains($"{id?.ToString()} - {str6}"))
                          {
                            this.invisibleElemsList.Add($"{id?.ToString()} - {str6}");
                            continue;
                          }
                          continue;
                        default:
                          continue;
                      }
                    }
                    if (((!flag1 || flag6 ? 0 : (!flag7 ? 1 : 0)) & (flag8 ? 1 : 0)) != 0)
                    {
                      if (elementIds8.Count > 0)
                        this.DimensionEDrawing(revitDoc, revitDoc.ActiveView, elementIds8, DimElementType.SingleDimString, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, placementCalculator, false, locationInFormValues, elementIds5.Count);
                    }
                    else
                    {
                      if (elementIds5.Count > 0)
                        this.DimensionEDrawing(revitDoc, revitDoc.ActiveView, elementIds5, DimElementType.DimItems, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, placementCalculator, true, locationInFormValues, elementIds5.Count, appendStringData);
                      if (elementIdList.Count > 0)
                        this.DimensionEDrawing(revitDoc, revitDoc.ActiveView, new List<Autodesk.Revit.DB.ElementId>(), DimElementType.GridLevel, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, placementCalculator, false, locationInFormValues, elementIds5.Count, appendStringData);
                      if (elementIds6.Count > 0)
                        this.DimensionEDrawing(revitDoc, revitDoc.ActiveView, elementIds6, DimElementType.SF, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, placementCalculator, false, locationInFormValues, elementIds5.Count, appendStringData);
                      if (elementIds7.Count > 0)
                        this.DimensionEDrawing(revitDoc, revitDoc.ActiveView, elementIds7, DimElementType.Void, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, placementCalculator, false, locationInFormValues, elementIds5.Count, appendStringData);
                    }
                    if (elementList2.Count == 0)
                      this.DimensionEDrawing(revitDoc, revitDoc.ActiveView, new List<Autodesk.Revit.DB.ElementId>(), DimElementType.GridLevel, eqForm, FormLocation.None, DimStyle.AutoDim, principalAxis, perpendicularAxis, currGridLevels, placementCalculator, false, locationInFormValues, elementIds5.Count, appendStringData);
                    int num14 = (int) transaction.Commit();
                  }
                }
              }
            }
            else
            {
              activeUiDocument.Selection.SetElementIds((ICollection<Autodesk.Revit.DB.ElementId>) new List<Autodesk.Revit.DB.ElementId>());
              activeUiDocument.Selection.Dispose();
              return (Result) 0;
            }
          }
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
        {
          int num15 = (int) transactionGroup.RollBack();
          return (Result) 1;
        }
      }
      int num16 = (int) transactionGroup.Assimilate();
    }
    bool flag12 = false;
    if (this.insufficientDimLinesGridsLevelsList.Keys.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^R Insufficient Dim Lines");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "The following element(s) did not contain sufficient EDGE Dim Lines for the selected dimensioning directions and will not be dimensioned to in the directions defined by the grid(s)/level(s) below:";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string key in this.insufficientDimLinesGridsLevelsList.Keys)
      {
        stringBuilder.AppendLine("Selected Grid(s) and/or Level(s): " + key);
        foreach (string str in this.insufficientDimLinesGridsLevelsList[key])
          stringBuilder.AppendLine("      " + str);
      }
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.Show();
      flag12 = true;
    }
    if (this.invisibleElemsList.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^R Hidden Elements Detected");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "The following element(s) in the selection group contained hidden geometry. Any geometry not visible in the current view will not be dimensioned to.";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string invisibleElems in this.invisibleElemsList)
        stringBuilder.AppendLine(invisibleElems);
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.Show();
      flag12 = true;
    }
    if (dimforEdwGsManager != null && dimforEdwGsManager.missingDimLines)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^R");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Selected element(s) for dimensioning did not include EdgeDimLines";
      taskDialog.MainContent = "Families without EdgeDimLines were not dimensioned to.";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string missingDimLinesElement in dimforEdwGsManager.missingDimLinesElements)
        stringBuilder.AppendLine(missingDimLinesElement);
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.Show();
      flag12 = true;
    }
    if (this.shortCurveError && !flag12)
      new TaskDialog("EDGE^R")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Dimension String Not Placed.",
        MainContent = "There were not enough reference points to place a Dimension String. The selected item(s) did not contain enough reference points for dimension placement or were in-line with the grid(s) and/or level(s) selected. Please ensure that there is enough space for dimensioning between selected element(s) and the selected grid(s) and/or level(s) and that there is geometry available to dimension to."
      }.Show();
    if (stringList1.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^R Insufficient Grid(s)/Level(s) per Axis");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "The following grid(s) and/or level(s) were the only grid(s) or level(s) selected per axis. In order to dimension properly, more than one grid and/or level must be selected per dimensioning axis.";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in stringList1)
        stringBuilder.AppendLine(str);
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.Show();
    }
    if (stringList2.Count > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^R Insufficient Grid(s)/Level(s) per Axis");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "The following grid(s) and/or level(s) were the only grid(s) or level(s) selected per axis. In order to dimension properly, more than one grid and/or level must be selected per dimensioning axis or additional elements must be selected.";
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str in stringList2)
        stringBuilder.AppendLine(str);
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.Show();
    }
    activeUiDocument.Selection.SetElementIds((ICollection<Autodesk.Revit.DB.ElementId>) new List<Autodesk.Revit.DB.ElementId>());
    activeUiDocument.Selection.Dispose();
    return (Result) 0;
  }

  private List<ReferencedPoint> RetrieveOverallPointCloud(
    List<Autodesk.Revit.DB.ElementId> partIds,
    List<Autodesk.Revit.DB.ElementId> partIdsStructFraming,
    List<Autodesk.Revit.DB.ElementId> partIdsVoids,
    List<Autodesk.Revit.DB.ElementId> currGridLevels,
    XYZ principalAxis,
    XYZ perpendicularAxis,
    Document revitDoc,
    View view)
  {
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    List<ReferencedPoint> collection1 = new List<ReferencedPoint>();
    List<ReferencedPoint> collection2 = new List<ReferencedPoint>();
    List<Autodesk.Revit.DB.ElementId> elementIdList = new List<Autodesk.Revit.DB.ElementId>();
    Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary1 = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
    Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary2 = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
    List<Reference> referenceList = new List<Reference>();
    Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    viewTransform.BasisX = perpendicularAxis;
    viewTransform.BasisY = principalAxis;
    DimensionEdge dimensionEdge = DimensionEdge.Left;
    foreach (Autodesk.Revit.DB.ElementId partId in partIds)
    {
      if (null == null)
        revitDoc.GetElement(partId);
      if (revitDoc.GetElement(new Reference(revitDoc.GetElement(partId)).ElementId) is Autodesk.Revit.DB.FamilyInstance element)
      {
        List<Line> refLines = new List<Line>();
        List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element, principalAxis, view, out refLines);
        int count = dimLineReference.Count;
        foreach (ReferencedPoint referencedPoint in this.CheckRefPointVisibility(dimLineReference, view))
          collection1.Add(referencedPoint);
      }
    }
    foreach (Autodesk.Revit.DB.ElementId id in partIdsStructFraming)
    {
      Autodesk.Revit.DB.FamilyInstance element = revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance;
      if (Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) element, out bool _, options: new Autodesk.Revit.DB.Options()
      {
        ComputeReferences = true
      }).Count != 0)
      {
        Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences1 = DimensioningGeometryUtils.GetDimensioningPointsReferences(element, view, viewTransform);
        Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences2 = DimensioningGeometryUtils.GetDimensioningPointsReferences(element, view, viewTransform.Inverse);
        List<Line> refLines = new List<Line>();
        List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(revitDoc.GetElement(id) as Autodesk.Revit.DB.FamilyInstance, principalAxis, view, out refLines);
        int count = dimLineReference.Count;
        foreach (ReferencedPoint referencedPoint in this.CheckRefPointVisibility(dimLineReference, view))
          collection1.Add(referencedPoint);
        ReferencedPoint referencedPoint1 = pointsReferences1[DimensionEdge.Top][dimensionEdge];
        ReferencedPoint referencedPoint2 = pointsReferences1[DimensionEdge.Top][this.GetOppDimEdge(dimensionEdge)];
        ReferencedPoint referencedPoint3 = pointsReferences1[DimensionEdge.Bottom][dimensionEdge];
        ReferencedPoint referencedPoint4 = pointsReferences1[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionEdge)];
        ReferencedPoint referencedPoint5 = pointsReferences2[DimensionEdge.Top][dimensionEdge];
        ReferencedPoint referencedPoint6 = pointsReferences2[DimensionEdge.Top][this.GetOppDimEdge(dimensionEdge)];
        ReferencedPoint referencedPoint7 = pointsReferences2[DimensionEdge.Bottom][dimensionEdge];
        ReferencedPoint referencedPoint8 = pointsReferences2[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionEdge)];
        List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
        referencedPointList2.Add(referencedPoint1);
        referencedPointList2.Add(referencedPoint2);
        referencedPointList2.Add(referencedPoint3);
        referencedPointList2.Add(referencedPoint4);
        referencedPointList2.Add(referencedPoint5);
        referencedPointList2.Add(referencedPoint6);
        referencedPointList2.Add(referencedPoint7);
        referencedPointList2.Add(referencedPoint8);
        ReferencedPoint referencedPoint9 = pointsReferences1[DimensionEdge.Left][DimensionEdge.Top];
        ReferencedPoint referencedPoint10 = pointsReferences1[DimensionEdge.Left][this.GetOppDimEdge(DimensionEdge.Top)];
        ReferencedPoint referencedPoint11 = pointsReferences1[DimensionEdge.Right][DimensionEdge.Top];
        ReferencedPoint referencedPoint12 = pointsReferences1[DimensionEdge.Right][this.GetOppDimEdge(DimensionEdge.Top)];
        ReferencedPoint referencedPoint13 = pointsReferences2[DimensionEdge.Top][dimensionEdge];
        ReferencedPoint referencedPoint14 = pointsReferences2[DimensionEdge.Top][this.GetOppDimEdge(dimensionEdge)];
        ReferencedPoint referencedPoint15 = pointsReferences2[DimensionEdge.Bottom][dimensionEdge];
        ReferencedPoint referencedPoint16 = pointsReferences2[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionEdge)];
        referencedPointList2.Add(referencedPoint9);
        referencedPointList2.Add(referencedPoint10);
        referencedPointList2.Add(referencedPoint11);
        referencedPointList2.Add(referencedPoint12);
        referencedPointList2.Add(referencedPoint13);
        referencedPointList2.Add(referencedPoint14);
        referencedPointList2.Add(referencedPoint15);
        referencedPointList2.Add(referencedPoint16);
        IEnumerable<double> source = referencedPointList2.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis)));
        double minProjDim = source.Min();
        double maxProjDim = source.Max();
        if (referencedPointList2.Count == 0)
        {
          foreach (ReferencedPoint sfFacePoint in this.FindSFFacePoints(element, view, viewTransform, principalAxis, perpendicularAxis, maxProjDim, minProjDim))
            referencedPointList2.Add(sfFacePoint);
        }
        collection1.AddRange((IEnumerable<ReferencedPoint>) referencedPointList2);
      }
    }
    foreach (Autodesk.Revit.DB.ElementId partIdsVoid in partIdsVoids)
    {
      if (null == null)
        revitDoc.GetElement(partIdsVoid);
      if (revitDoc.GetElement(new Reference(revitDoc.GetElement(partIdsVoid)).ElementId) is Autodesk.Revit.DB.FamilyInstance element)
      {
        List<Line> refLines = new List<Line>();
        List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element, principalAxis, view, out refLines);
        int count = dimLineReference.Count;
        foreach (ReferencedPoint referencedPoint in this.CheckRefPointVisibility(dimLineReference, view))
          collection1.Add(referencedPoint);
      }
    }
    referencedPointList1.AddRange((IEnumerable<ReferencedPoint>) collection1);
    referencedPointList1.AddRange((IEnumerable<ReferencedPoint>) collection2);
    return referencedPointList1;
  }

  public Result DimensionEDrawing(
    Document revitDoc,
    View view,
    List<Autodesk.Revit.DB.ElementId> elementIds,
    DimElementType det,
    bool eqForm,
    FormLocation fLocation,
    DimStyle dimStyle,
    XYZ principalAxis,
    XYZ perpendicularAxis,
    List<Autodesk.Revit.DB.ElementId> currGridLevels,
    DimPlacementCalculatorEDrawing placementCalculator,
    bool addAppendString,
    AutoTicketLIF lifSettings,
    int count = -1,
    List<AutoTicketAppendStringParameterData> appendStringParameterData = null,
    List<ReferencedPoint> sfPoints = null,
    string sfDimName = "")
  {
    SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
    schemaBuilder.SetSchemaName("PTAC_Dimensions");
    Schema schema = schemaBuilder.Finish();
    StringBuilder stringBuilder = new StringBuilder();
    int num1 = 0;
    foreach (Autodesk.Revit.DB.ElementId currGridLevel in currGridLevels)
    {
      string name = revitDoc.GetElement(currGridLevel).Name;
      if (num1 == 0)
        stringBuilder.Append(name);
      else
        stringBuilder.Append(", " + name);
      ++num1;
    }
    try
    {
      Autodesk.Revit.DB.Element element1 = (Autodesk.Revit.DB.Element) null;
      Autodesk.Revit.DB.FamilyInstance famInst = (Autodesk.Revit.DB.FamilyInstance) null;
      DimPlacementInfo dimPlacementInfo;
      try
      {
        dimPlacementInfo = placementCalculator.GetDimPlacementInfo(DimensionEdge.Top);
      }
      catch
      {
        this.shortCurveError = true;
        return (Result) 1;
      }
      DimensionEdge dimensionFromEdge = dimPlacementInfo.DimensionFromEdge;
      Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary1 = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
      Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary2 = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
      List<Reference> referenceList = new List<Reference>();
      List<string> stringList = new List<string>();
      List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
      List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
      List<ReferencedPoint> collection = new List<ReferencedPoint>();
      Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
      viewTransform.BasisX = perpendicularAxis;
      viewTransform.BasisY = principalAxis;
      if (det == DimElementType.SingleDimString)
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in elementIds)
        {
          stringList.Add(revitDoc.GetElement(elementId).Name);
          switch (this.CheckElementType(revitDoc.GetElement(elementId)))
          {
            case DimElementType.SF:
              element1 = revitDoc.GetElement(elementId);
              Autodesk.Revit.DB.FamilyInstance familyInstance = element1 as Autodesk.Revit.DB.FamilyInstance;
              if (Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) familyInstance, out bool _, options: new Autodesk.Revit.DB.Options()
              {
                ComputeReferences = true
              }).Count != 0)
              {
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences1 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform);
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences2 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform.Inverse);
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(revitDoc.GetElement(elementId) as Autodesk.Revit.DB.FamilyInstance, principalAxis, view, out refLines);
                if (dimLineReference.Count > 0)
                {
                  int count1 = dimLineReference.Count;
                  List<ReferencedPoint> referencedPointList3 = this.CheckRefPointVisibility(dimLineReference, view);
                  if (this.CheckRefLineVisibility(refLines, view).Count > 0)
                  {
                    string str = !familyInstance.Name.Equals(familyInstance.Symbol.Family.Name.ToString()) ? $"{familyInstance.Symbol.Family.Name} - {familyInstance.Name}" : familyInstance.Name;
                    if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                      this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                  }
                  using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList3.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      ReferencedPoint current = enumerator.Current;
                      referencedPointList1.Add(current);
                    }
                    continue;
                  }
                }
                ReferencedPoint referencedPoint1 = pointsReferences1[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint2 = pointsReferences1[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint3 = pointsReferences1[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint4 = pointsReferences1[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint5 = pointsReferences2[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint6 = pointsReferences2[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint7 = pointsReferences2[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint8 = pointsReferences2[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                List<ReferencedPoint> referencedPointList4 = new List<ReferencedPoint>();
                List<ReferencedPoint> referencedPointList5 = new List<ReferencedPoint>();
                referencedPointList5.Add(referencedPoint1);
                referencedPointList5.Add(referencedPoint2);
                referencedPointList5.Add(referencedPoint3);
                referencedPointList5.Add(referencedPoint4);
                referencedPointList5.Add(referencedPoint5);
                referencedPointList5.Add(referencedPoint6);
                referencedPointList5.Add(referencedPoint7);
                referencedPointList5.Add(referencedPoint8);
                ReferencedPoint referencedPoint9 = pointsReferences1[DimensionEdge.Left][DimensionEdge.Top];
                ReferencedPoint referencedPoint10 = pointsReferences1[DimensionEdge.Left][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint11 = pointsReferences1[DimensionEdge.Right][DimensionEdge.Top];
                ReferencedPoint referencedPoint12 = pointsReferences1[DimensionEdge.Right][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint13 = pointsReferences2[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint14 = pointsReferences2[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint15 = pointsReferences2[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint16 = pointsReferences2[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                referencedPointList5.Add(referencedPoint9);
                referencedPointList5.Add(referencedPoint10);
                referencedPointList5.Add(referencedPoint11);
                referencedPointList5.Add(referencedPoint12);
                referencedPointList5.Add(referencedPoint13);
                referencedPointList5.Add(referencedPoint14);
                referencedPointList5.Add(referencedPoint15);
                referencedPointList5.Add(referencedPoint16);
                List<ReferencedPoint> list = referencedPointList5.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (rp => this.IsReferencePointVisibleWithinView(view, rp))).ToList<ReferencedPoint>();
                collection.AddRange((IEnumerable<ReferencedPoint>) referencedPointList5);
                List<double> doubleList = new List<double>();
                List<double> source = list.Count != 0 ? list.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>() : referencedPointList5.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>();
                double num2 = source.Min();
                double num3 = source.Max();
                if (list.Count == 0)
                {
                  foreach (ReferencedPoint sfFacePoint in this.FindSFFacePoints(familyInstance, view, viewTransform, principalAxis, perpendicularAxis, num3, num2))
                    list.Add(sfFacePoint);
                }
                foreach (ReferencedPoint referencedPoint17 in list)
                {
                  if (ProjectionUtils.ProjectPointOnDirection(referencedPoint17.Point, perpendicularAxis).ApproximatelyEquals(num3, 0.00032552083333333332) || ProjectionUtils.ProjectPointOnDirection(referencedPoint17.Point, perpendicularAxis).ApproximatelyEquals(num2, 0.00032552083333333332))
                    referencedPointList1.Add(referencedPoint17);
                }
                if (referencedPointList1.Count == 0)
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              if (element1.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().Count > 0)
              {
                int num4 = this.CheckForSubCompsInList(element1, revitDoc, elementIds) ? 1 : 0;
                if (num4 != 0)
                  this.AddToErrorList(familyInstance);
                if (num4 != 0)
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              this.AddToErrorList(familyInstance);
              continue;
            case DimElementType.Void:
            case DimElementType.DimItems:
              if (famInst == null)
              {
                element1 = revitDoc.GetElement(elementId);
                famInst = element1 as Autodesk.Revit.DB.FamilyInstance;
              }
              if (revitDoc.GetElement(new Reference(revitDoc.GetElement(elementId)).ElementId) is Autodesk.Revit.DB.FamilyInstance element2)
              {
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element2, principalAxis, view, out refLines);
                int count2 = dimLineReference.Count;
                List<ReferencedPoint> referencedPointList6 = this.CheckRefPointVisibility(dimLineReference, view);
                List<Line> lineList = this.CheckRefLineVisibility(refLines, view);
                bool flag = false;
                if (lineList.Count > 0)
                {
                  string str = !element2.Name.Equals(element2.Symbol.Family.Name.ToString()) ? $"{element2.Symbol.Family.Name} - {element2.Name}" : element2.Name;
                  if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                  {
                    this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                    flag = true;
                  }
                }
                if ((referencedPointList6 == null || referencedPointList6.Count == 0) && !flag)
                {
                  string str = !element2.Name.Equals(element2.Symbol.Family.Name.ToString()) ? $"{element2.Symbol.Family.Name} - {element2.Name}" : element2.Name;
                  if (!this.insufficientDimLinesGridsLevelsList.Keys.Contains<string>(stringBuilder.ToString()))
                    this.insufficientDimLinesGridsLevelsList.Add(stringBuilder.ToString(), new List<string>()
                    {
                      $"{elementId?.ToString()} - {str}"
                    });
                  else
                    this.insufficientDimLinesGridsLevelsList[stringBuilder.ToString()].Add($"{elementId?.ToString()} - {str}");
                  --count;
                }
                using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList6.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    ReferencedPoint current = enumerator.Current;
                    referencedPointList1.Add(current);
                  }
                  continue;
                }
              }
              continue;
            case DimElementType.GridLevel:
              Autodesk.Revit.DB.Element element3 = revitDoc.GetElement(elementId);
              Line line = (Line) null;
              if (element3 is Autodesk.Revit.DB.Grid)
              {
                Curve curve = (element3 as Autodesk.Revit.DB.Grid).Curve;
                if (curve is Line)
                  line = curve as Line;
              }
              else if (element3 is Autodesk.Revit.DB.Level)
              {
                List<Curve> list = (element3 as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
                if (list[0] is Line)
                  line = list[0] as Line;
              }
              if ((GeometryObject) line != (GeometryObject) null)
              {
                ReferencedPoint referencedPoint = new ReferencedPoint()
                {
                  Reference = new Reference(element3),
                  Point = line.GetEndPoint(0)
                };
                referencedPointList2.Add(referencedPoint);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      else
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in elementIds)
        {
          stringList.Add(revitDoc.GetElement(elementId).Name);
          switch (det)
          {
            case DimElementType.SF:
              element1 = revitDoc.GetElement(elementId);
              Autodesk.Revit.DB.FamilyInstance familyInstance = element1 as Autodesk.Revit.DB.FamilyInstance;
              if (Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) familyInstance, out bool _, options: new Autodesk.Revit.DB.Options()
              {
                ComputeReferences = true
              }).Count != 0)
              {
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences3 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform);
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences4 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform, true);
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(revitDoc.GetElement(elementId) as Autodesk.Revit.DB.FamilyInstance, principalAxis, view, out refLines);
                if (dimLineReference.Count > 0)
                {
                  int count3 = dimLineReference.Count;
                  List<ReferencedPoint> referencedPointList7 = this.CheckRefPointVisibility(dimLineReference, view);
                  if (this.CheckRefLineVisibility(refLines, view).Count > 0)
                  {
                    string str = !familyInstance.Name.Equals(familyInstance.Symbol.Family.Name.ToString()) ? $"{familyInstance.Symbol.Family.Name} - {familyInstance.Name}" : familyInstance.Name;
                    if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                      this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                  }
                  using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList7.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      ReferencedPoint current = enumerator.Current;
                      referencedPointList1.Add(current);
                    }
                    continue;
                  }
                }
                ReferencedPoint referencedPoint18 = pointsReferences3[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint19 = pointsReferences3[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint20 = pointsReferences3[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint21 = pointsReferences3[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint22 = pointsReferences4[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint23 = pointsReferences4[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint24 = pointsReferences4[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint25 = pointsReferences4[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                List<ReferencedPoint> referencedPointList8 = new List<ReferencedPoint>();
                List<ReferencedPoint> referencedPointList9 = new List<ReferencedPoint>();
                referencedPointList9.Add(referencedPoint18);
                referencedPointList9.Add(referencedPoint19);
                referencedPointList9.Add(referencedPoint20);
                referencedPointList9.Add(referencedPoint21);
                referencedPointList9.Add(referencedPoint22);
                referencedPointList9.Add(referencedPoint23);
                referencedPointList9.Add(referencedPoint24);
                referencedPointList9.Add(referencedPoint25);
                ReferencedPoint referencedPoint26 = pointsReferences3[DimensionEdge.Left][DimensionEdge.Top];
                ReferencedPoint referencedPoint27 = pointsReferences3[DimensionEdge.Left][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint28 = pointsReferences3[DimensionEdge.Right][DimensionEdge.Top];
                ReferencedPoint referencedPoint29 = pointsReferences3[DimensionEdge.Right][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint30 = pointsReferences4[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint31 = pointsReferences4[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint32 = pointsReferences4[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint33 = pointsReferences4[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                referencedPointList9.Add(referencedPoint26);
                referencedPointList9.Add(referencedPoint27);
                referencedPointList9.Add(referencedPoint28);
                referencedPointList9.Add(referencedPoint29);
                referencedPointList9.Add(referencedPoint30);
                referencedPointList9.Add(referencedPoint31);
                referencedPointList9.Add(referencedPoint32);
                referencedPointList9.Add(referencedPoint33);
                List<ReferencedPoint> list = referencedPointList9.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (rp => this.IsReferencePointVisibleWithinView(view, rp))).ToList<ReferencedPoint>();
                collection.AddRange((IEnumerable<ReferencedPoint>) referencedPointList9);
                List<double> doubleList = new List<double>();
                List<double> source = list.Count != 0 ? list.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>() : referencedPointList9.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>();
                double num5 = source.Min();
                double num6 = source.Max();
                if (list.Count == 0)
                {
                  foreach (ReferencedPoint sfFacePoint in this.FindSFFacePoints(familyInstance, view, viewTransform, principalAxis, perpendicularAxis, num6, num5))
                    list.Add(sfFacePoint);
                }
                foreach (ReferencedPoint referencedPoint34 in list)
                {
                  if (ProjectionUtils.ProjectPointOnDirection(referencedPoint34.Point, perpendicularAxis).ApproximatelyEquals(num6, 0.00032552083333333332) || ProjectionUtils.ProjectPointOnDirection(referencedPoint34.Point, perpendicularAxis).ApproximatelyEquals(num5, 0.00032552083333333332))
                    referencedPointList1.Add(referencedPoint34);
                }
                if (referencedPointList1.Count == 0)
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              if (element1.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().Count > 0)
              {
                if (this.CheckForSubCompsInList(element1, revitDoc, elementIds))
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              this.AddToErrorList(familyInstance);
              continue;
            case DimElementType.Void:
            case DimElementType.DimItems:
              if (famInst == null)
              {
                element1 = revitDoc.GetElement(elementId);
                famInst = element1 as Autodesk.Revit.DB.FamilyInstance;
              }
              if (revitDoc.GetElement(new Reference(revitDoc.GetElement(elementId)).ElementId) is Autodesk.Revit.DB.FamilyInstance element4)
              {
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element4, principalAxis, view, out refLines);
                int count4 = dimLineReference.Count;
                List<ReferencedPoint> referencedPointList10 = this.CheckRefPointVisibility(dimLineReference, view);
                List<Line> lineList = this.CheckRefLineVisibility(refLines, view);
                bool flag = false;
                if (lineList.Count > 0)
                {
                  string str = !element4.Name.Equals(element4.Symbol.Family.Name.ToString()) ? $"{element4.Symbol.Family.Name} - {element4.Name}" : element4.Name;
                  if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                  {
                    this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                    flag = true;
                  }
                }
                if ((referencedPointList10 == null || referencedPointList10.Count == 0) && !flag)
                {
                  string str = !element4.Name.Equals(element4.Symbol.Family.Name.ToString()) ? $"{element4.Symbol.Family.Name} - {element4.Name}" : element4.Name;
                  if (!this.insufficientDimLinesGridsLevelsList.Keys.Contains<string>(stringBuilder.ToString()))
                    this.insufficientDimLinesGridsLevelsList.Add(stringBuilder.ToString(), new List<string>()
                    {
                      $"{elementId?.ToString()} - {str}"
                    });
                  else
                    this.insufficientDimLinesGridsLevelsList[stringBuilder.ToString()].Add($"{elementId?.ToString()} - {str}");
                  --count;
                }
                using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList10.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    ReferencedPoint current = enumerator.Current;
                    referencedPointList1.Add(current);
                  }
                  continue;
                }
              }
              continue;
            case DimElementType.GridLevel:
              Autodesk.Revit.DB.Element element5 = revitDoc.GetElement(elementId);
              Line line = (Line) null;
              if (element5 is Autodesk.Revit.DB.Grid)
              {
                Curve curve = (element5 as Autodesk.Revit.DB.Grid).Curve;
                if (curve is Line)
                  line = curve as Line;
              }
              else if (element5 is Autodesk.Revit.DB.Level)
              {
                List<Curve> list = (element5 as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
                if (list[0] is Line)
                  line = list[0] as Line;
              }
              if ((GeometryObject) line != (GeometryObject) null)
              {
                ReferencedPoint referencedPoint = new ReferencedPoint()
                {
                  Reference = new Reference(element5),
                  Point = line.GetEndPoint(0)
                };
                referencedPointList2.Add(referencedPoint);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      foreach (Autodesk.Revit.DB.ElementId currGridLevel in currGridLevels)
      {
        Autodesk.Revit.DB.Element element6 = revitDoc.GetElement(currGridLevel);
        Line line = (Line) null;
        if (element6 is Autodesk.Revit.DB.Grid)
        {
          Curve curve = (element6 as Autodesk.Revit.DB.Grid).Curve;
          if (curve is Line)
            line = curve as Line;
        }
        else if (element6 is Autodesk.Revit.DB.Level)
        {
          List<Curve> list = (element6 as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
          if (list[0] is Line)
            line = list[0] as Line;
        }
        if ((GeometryObject) line != (GeometryObject) null)
        {
          ReferencedPoint referencedPoint = new ReferencedPoint()
          {
            Reference = new Reference(element6),
            Point = line.GetEndPoint(0)
          };
          referencedPointList2.Add(referencedPoint);
        }
      }
      DimensionEdge dimensionEdge = DimensionEdge.Top;
      if (referencedPointList1.Count > 0)
      {
        List<ReferencedPoint> referencePoints = new List<ReferencedPoint>();
        referencePoints.AddRange((IEnumerable<ReferencedPoint>) referencedPointList1);
        referencePoints.AddRange((IEnumerable<ReferencedPoint>) collection);
        dimensionEdge = placementCalculator.DetermineDimEdge(referencePoints);
      }
      Autodesk.Revit.DB.TextNote textNote = (Autodesk.Revit.DB.TextNote) null;
      Autodesk.Revit.DB.DimensionType dimensionType = (Autodesk.Revit.DB.DimensionType) null;
      string str1 = "";
      bool flag1 = true;
      Autodesk.Revit.DB.ElementId elementId1 = revitDoc.GetDefaultElementTypeId(Autodesk.Revit.DB.ElementTypeGroup.TextNoteType);
      foreach (AutoTicketSettingsTools eDrawingSetting in this.eDrawingSettings)
      {
        if (eDrawingSetting is AutoTicketCalloutAndDimensionTexts)
        {
          AutoTicketCalloutAndDimensionTexts andDimensionTexts = eDrawingSetting as AutoTicketCalloutAndDimensionTexts;
          CalloutStyle calloutStyle = new CalloutStyle(revitDoc, AutoTicketEnhancementWorkflow.AutoDimensionEDrawing, "", andDimensionTexts.OverallDimension, "", andDimensionTexts.TextStyle);
          if (calloutStyle.textNoteStyle != null)
            elementId1 = calloutStyle.textNoteStyle.Id;
          dimensionType = calloutStyle.dimensionStyle != null ? calloutStyle.dimensionStyle : (Autodesk.Revit.DB.DimensionType) null;
        }
        if (eDrawingSetting is AutoTicketAppendString && addAppendString)
        {
          str1 = new AppendStringEvaluate((eDrawingSetting as AutoTicketAppendString).AppendStringValue, element1, famInst, count, appendStringParameterData, (string) null).resultant;
          if (string.IsNullOrEmpty(str1))
            addAppendString = false;
          flag1 = false;
        }
      }
      if (addAppendString && count != 0)
      {
        Autodesk.Revit.DB.ElementId elementId2 = elementId1;
        Autodesk.Revit.DB.TextNoteOptions options = new Autodesk.Revit.DB.TextNoteOptions();
        options.Rotation = dimPlacementInfo.TextAngle;
        options.TypeId = elementId2;
        string str2 = !string.IsNullOrWhiteSpace(element1.GetIdentityDescriptionShort()) ? element1.GetIdentityDescriptionShort() : (famInst != null ? famInst.Symbol.FamilyName : "");
        textNote = Autodesk.Revit.DB.TextNote.Create(revitDoc, view.Id, new XYZ(0.0, 0.0, 0.0), flag1 ? $"({count.ToString()}) {str2}" : str1, options);
        revitDoc.Regenerate();
      }
      List<ReferencedPoint> referencedPoints = new List<ReferencedPoint>();
      referencedPoints.AddRange((IEnumerable<ReferencedPoint>) referencedPointList1);
      referencedPoints.AddRange((IEnumerable<ReferencedPoint>) collection);
      DimPlacementInfo dimLocation = placementCalculator.DetermineDimLocation(referencedPoints, referencedPointList2, dimPlacementInfo, dimensionEdge, textNote);
      List<ReferencedPoint> list1 = referencedPointList1.Union<ReferencedPoint>((IEnumerable<ReferencedPoint>) referencedPointList2).ToList<ReferencedPoint>();
      DimReferencesManager referencesManager = new DimReferencesManager(revitDoc, dimensionFromEdge, dimLocation.DimAlongDirection, placementCalculator.GetDirectionVectorForDimPlacement(dimensionEdge));
      foreach (ReferencedPoint rP in list1)
      {
        int num7 = (int) referencesManager.AddReferenceED(rP);
      }
      if (referencesManager.DimReferences.Size < 2)
      {
        this.shortCurveError = true;
        return (Result) 1;
      }
      dimLocation.TextPlacementPoint = placementCalculator.GetTextPlacementPoint(dimLocation.PlacementLine, dimensionEdge, list1);
      Autodesk.Revit.DB.Dimension dimension = revitDoc.Create.NewDimension(view, dimLocation.PlacementLine, referencesManager.DimReferences);
      if (dimensionType != null)
        dimension.DimensionType = dimensionType;
      this.previousDimension = dimension;
      if (addAppendString && count != 0)
      {
        ElementTransformUtils.MoveElement(revitDoc, textNote.Id, dimLocation.TextPlacementPoint);
        textNote.Coord = placementCalculator.BumpNote(textNote, dimensionEdge);
        Parameters.GetParameterAsDouble((Autodesk.Revit.DB.Element) textNote.TextNoteType, BuiltInParameter.TEXT_SIZE);
        XYZ upDirection = textNote.UpDirection;
        double num8 = textNote.Height / 2.0 * (double) view.Scale;
        XYZ translation = upDirection.Normalize() * num8;
        ElementTransformUtils.MoveElement(revitDoc, textNote.Id, translation);
        Entity entity = new Entity(schema);
        schema.GetField("FileName");
        entity.Set<string>("PTACDimensionGuid", textNote.UniqueId);
        textNote.SetEntity(entity);
      }
      if (eqForm)
        dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ)?.Set(2);
      return (Result) 0;
    }
    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
    {
      return (Result) -1;
    }
  }

  public Result DimensionEDrawingMultiGrouping(
    Document revitDoc,
    View view,
    List<Autodesk.Revit.DB.ElementId> elementIds,
    DimElementType det,
    bool eqForm,
    FormLocation fLocation,
    DimStyle dimStyle,
    XYZ principalAxis,
    XYZ perpendicularAxis,
    List<Autodesk.Revit.DB.ElementId> currGridLevels,
    List<ReferencedPoint> overallPointCloud,
    DimPlacementCalculatorEDrawing placementCalculator,
    bool addAppendString,
    AutoTicketLIF lifSettings,
    int count = -1,
    List<AutoTicketAppendStringParameterData> appendStringParameterData = null,
    List<ReferencedPoint> sfPoints = null,
    string sfDimName = "")
  {
    SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
    schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
    schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
    schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
    schemaBuilder.SetSchemaName("PTAC_Dimensions");
    Schema schema = schemaBuilder.Finish();
    StringBuilder stringBuilder = new StringBuilder();
    int num1 = 0;
    foreach (Autodesk.Revit.DB.ElementId currGridLevel in currGridLevels)
    {
      string name = revitDoc.GetElement(currGridLevel).Name;
      if (num1 == 0)
        stringBuilder.Append(name);
      else
        stringBuilder.Append(", " + name);
      ++num1;
    }
    try
    {
      Autodesk.Revit.DB.Element element1 = (Autodesk.Revit.DB.Element) null;
      Autodesk.Revit.DB.FamilyInstance famInst = (Autodesk.Revit.DB.FamilyInstance) null;
      DimPlacementInfo dimPlacementInfo;
      try
      {
        dimPlacementInfo = placementCalculator.GetDimPlacementInfo(DimensionEdge.Top);
      }
      catch
      {
        this.shortCurveError = true;
        return (Result) 1;
      }
      DimensionEdge dimensionFromEdge = dimPlacementInfo.DimensionFromEdge;
      Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary1 = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
      Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary2 = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
      List<Reference> referenceList = new List<Reference>();
      List<string> stringList = new List<string>();
      List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
      List<ReferencedPoint> referencedPointList2 = new List<ReferencedPoint>();
      Transform viewTransform = Utils.ViewUtils.ViewUtils.GetViewTransform(view);
      viewTransform.BasisX = perpendicularAxis;
      viewTransform.BasisY = principalAxis;
      if (det == DimElementType.SingleDimString)
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in elementIds)
        {
          stringList.Add(revitDoc.GetElement(elementId).Name);
          switch (this.CheckElementType(revitDoc.GetElement(elementId)))
          {
            case DimElementType.SF:
              element1 = revitDoc.GetElement(elementId);
              Autodesk.Revit.DB.FamilyInstance familyInstance = element1 as Autodesk.Revit.DB.FamilyInstance;
              if (Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) familyInstance, out bool _, options: new Autodesk.Revit.DB.Options()
              {
                ComputeReferences = true
              }).Count != 0)
              {
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences1 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform);
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences2 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform.Inverse);
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(revitDoc.GetElement(elementId) as Autodesk.Revit.DB.FamilyInstance, principalAxis, view, out refLines);
                if (dimLineReference.Count > 0)
                {
                  int count1 = dimLineReference.Count;
                  List<ReferencedPoint> referencedPointList3 = this.CheckRefPointVisibility(dimLineReference, view);
                  if (this.CheckRefLineVisibility(refLines, view).Count > 0)
                  {
                    string str = !familyInstance.Name.Equals(familyInstance.Symbol.Family.Name.ToString()) ? $"{familyInstance.Symbol.Family.Name} - {familyInstance.Name}" : familyInstance.Name;
                    if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                      this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                  }
                  using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList3.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      ReferencedPoint current = enumerator.Current;
                      referencedPointList1.Add(current);
                    }
                    continue;
                  }
                }
                ReferencedPoint referencedPoint1 = pointsReferences1[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint2 = pointsReferences1[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint3 = pointsReferences1[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint4 = pointsReferences1[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint5 = pointsReferences2[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint6 = pointsReferences2[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint7 = pointsReferences2[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint8 = pointsReferences2[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                List<ReferencedPoint> referencedPointList4 = new List<ReferencedPoint>();
                List<ReferencedPoint> source1 = new List<ReferencedPoint>();
                source1.Add(referencedPoint1);
                source1.Add(referencedPoint2);
                source1.Add(referencedPoint3);
                source1.Add(referencedPoint4);
                source1.Add(referencedPoint5);
                source1.Add(referencedPoint6);
                source1.Add(referencedPoint7);
                source1.Add(referencedPoint8);
                ReferencedPoint referencedPoint9 = pointsReferences1[DimensionEdge.Left][DimensionEdge.Top];
                ReferencedPoint referencedPoint10 = pointsReferences1[DimensionEdge.Left][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint11 = pointsReferences1[DimensionEdge.Right][DimensionEdge.Top];
                ReferencedPoint referencedPoint12 = pointsReferences1[DimensionEdge.Right][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint13 = pointsReferences2[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint14 = pointsReferences2[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint15 = pointsReferences2[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint16 = pointsReferences2[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                source1.Add(referencedPoint9);
                source1.Add(referencedPoint10);
                source1.Add(referencedPoint11);
                source1.Add(referencedPoint12);
                source1.Add(referencedPoint13);
                source1.Add(referencedPoint14);
                source1.Add(referencedPoint15);
                source1.Add(referencedPoint16);
                List<ReferencedPoint> list = source1.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (rp => this.IsReferencePointVisibleWithinView(view, rp))).ToList<ReferencedPoint>();
                List<double> doubleList = new List<double>();
                List<double> source2 = list.Count != 0 ? list.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>() : source1.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>();
                double num2 = source2.Min();
                double num3 = source2.Max();
                if (list.Count == 0)
                {
                  foreach (ReferencedPoint sfFacePoint in this.FindSFFacePoints(familyInstance, view, viewTransform, principalAxis, perpendicularAxis, num3, num2))
                    list.Add(sfFacePoint);
                }
                foreach (ReferencedPoint referencedPoint17 in list)
                {
                  if (ProjectionUtils.ProjectPointOnDirection(referencedPoint17.Point, perpendicularAxis).ApproximatelyEquals(num3, 0.00032552083333333332) || ProjectionUtils.ProjectPointOnDirection(referencedPoint17.Point, perpendicularAxis).ApproximatelyEquals(num2, 0.00032552083333333332))
                    referencedPointList1.Add(referencedPoint17);
                }
                if (referencedPointList1.Count == 0)
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              if (element1.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().Count > 0)
              {
                if (this.CheckForSubCompsInList(element1, revitDoc, elementIds))
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              this.AddToErrorList(familyInstance);
              continue;
            case DimElementType.Void:
            case DimElementType.DimItems:
              if (famInst == null)
              {
                element1 = revitDoc.GetElement(elementId);
                famInst = element1 as Autodesk.Revit.DB.FamilyInstance;
              }
              if (revitDoc.GetElement(new Reference(revitDoc.GetElement(elementId)).ElementId) is Autodesk.Revit.DB.FamilyInstance element2)
              {
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element2, principalAxis, view, out refLines);
                int count2 = dimLineReference.Count;
                List<ReferencedPoint> referencedPointList5 = this.CheckRefPointVisibility(dimLineReference, view);
                List<Line> lineList = this.CheckRefLineVisibility(refLines, view);
                bool flag = false;
                if (lineList.Count > 0)
                {
                  string str = !element2.Name.Equals(element2.Symbol.Family.Name.ToString()) ? $"{element2.Symbol.Family.Name} - {element2.Name}" : element2.Name;
                  if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                  {
                    this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                    flag = true;
                  }
                }
                if ((referencedPointList5 == null || referencedPointList5.Count == 0) && !flag)
                {
                  string str = !element2.Name.Equals(element2.Symbol.Family.Name.ToString()) ? $"{element2.Symbol.Family.Name} - {element2.Name}" : element2.Name;
                  if (!this.insufficientDimLinesGridsLevelsList.Keys.Contains<string>(stringBuilder.ToString()))
                    this.insufficientDimLinesGridsLevelsList.Add(stringBuilder.ToString(), new List<string>()
                    {
                      $"{elementId?.ToString()} - {str}"
                    });
                  else
                    this.insufficientDimLinesGridsLevelsList[stringBuilder.ToString()].Add($"{elementId?.ToString()} - {str}");
                  --count;
                }
                using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList5.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    ReferencedPoint current = enumerator.Current;
                    referencedPointList1.Add(current);
                  }
                  continue;
                }
              }
              continue;
            case DimElementType.GridLevel:
              Autodesk.Revit.DB.Element element3 = revitDoc.GetElement(elementId);
              Line line = (Line) null;
              if (element3 is Autodesk.Revit.DB.Grid)
              {
                Curve curve = (element3 as Autodesk.Revit.DB.Grid).Curve;
                if (curve is Line)
                  line = curve as Line;
              }
              else if (element3 is Autodesk.Revit.DB.Level)
              {
                List<Curve> list = (element3 as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
                if (list[0] is Line)
                  line = list[0] as Line;
              }
              if ((GeometryObject) line != (GeometryObject) null)
              {
                ReferencedPoint referencedPoint = new ReferencedPoint()
                {
                  Reference = new Reference(element3),
                  Point = line.GetEndPoint(0)
                };
                referencedPointList2.Add(referencedPoint);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      else
      {
        foreach (Autodesk.Revit.DB.ElementId elementId in elementIds)
        {
          stringList.Add(revitDoc.GetElement(elementId).Name);
          switch (det)
          {
            case DimElementType.SF:
              element1 = revitDoc.GetElement(elementId);
              Autodesk.Revit.DB.FamilyInstance familyInstance = element1 as Autodesk.Revit.DB.FamilyInstance;
              if (Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) familyInstance, out bool _, options: new Autodesk.Revit.DB.Options()
              {
                ComputeReferences = true
              }).Count != 0)
              {
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences3 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform);
                Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> pointsReferences4 = DimensioningGeometryUtils.GetDimensioningPointsReferences(familyInstance, view, viewTransform.Inverse);
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(revitDoc.GetElement(elementId) as Autodesk.Revit.DB.FamilyInstance, principalAxis, view, out refLines);
                if (dimLineReference.Count > 0)
                {
                  int count3 = dimLineReference.Count;
                  List<ReferencedPoint> referencedPointList6 = this.CheckRefPointVisibility(dimLineReference, view);
                  if (this.CheckRefLineVisibility(refLines, view).Count > 0)
                  {
                    string str = !familyInstance.Name.Equals(familyInstance.Symbol.Family.Name.ToString()) ? $"{familyInstance.Symbol.Family.Name} - {familyInstance.Name}" : familyInstance.Name;
                    if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                      this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                  }
                  using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList6.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      ReferencedPoint current = enumerator.Current;
                      referencedPointList1.Add(current);
                    }
                    continue;
                  }
                }
                ReferencedPoint referencedPoint18 = pointsReferences3[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint19 = pointsReferences3[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint20 = pointsReferences3[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint21 = pointsReferences3[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint22 = pointsReferences4[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint23 = pointsReferences4[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint24 = pointsReferences4[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint25 = pointsReferences4[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                List<ReferencedPoint> referencedPointList7 = new List<ReferencedPoint>();
                List<ReferencedPoint> source3 = new List<ReferencedPoint>();
                source3.Add(referencedPoint18);
                source3.Add(referencedPoint19);
                source3.Add(referencedPoint20);
                source3.Add(referencedPoint21);
                source3.Add(referencedPoint22);
                source3.Add(referencedPoint23);
                source3.Add(referencedPoint24);
                source3.Add(referencedPoint25);
                ReferencedPoint referencedPoint26 = pointsReferences3[DimensionEdge.Left][DimensionEdge.Top];
                ReferencedPoint referencedPoint27 = pointsReferences3[DimensionEdge.Left][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint28 = pointsReferences3[DimensionEdge.Right][DimensionEdge.Top];
                ReferencedPoint referencedPoint29 = pointsReferences3[DimensionEdge.Right][this.GetOppDimEdge(DimensionEdge.Top)];
                ReferencedPoint referencedPoint30 = pointsReferences4[DimensionEdge.Top][dimensionFromEdge];
                ReferencedPoint referencedPoint31 = pointsReferences4[DimensionEdge.Top][this.GetOppDimEdge(dimensionFromEdge)];
                ReferencedPoint referencedPoint32 = pointsReferences4[DimensionEdge.Bottom][dimensionFromEdge];
                ReferencedPoint referencedPoint33 = pointsReferences4[DimensionEdge.Bottom][this.GetOppDimEdge(dimensionFromEdge)];
                source3.Add(referencedPoint26);
                source3.Add(referencedPoint27);
                source3.Add(referencedPoint28);
                source3.Add(referencedPoint29);
                source3.Add(referencedPoint30);
                source3.Add(referencedPoint31);
                source3.Add(referencedPoint32);
                source3.Add(referencedPoint33);
                List<ReferencedPoint> list = source3.Where<ReferencedPoint>((Func<ReferencedPoint, bool>) (rp => this.IsReferencePointVisibleWithinView(view, rp))).ToList<ReferencedPoint>();
                List<double> doubleList = new List<double>();
                List<double> source4 = list.Count != 0 ? list.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>() : source3.Select<ReferencedPoint, double>((Func<ReferencedPoint, double>) (rP => ProjectionUtils.ProjectPointOnDirection(rP.Point, perpendicularAxis))).ToList<double>();
                double num4 = source4.Min();
                double num5 = source4.Max();
                if (list.Count == 0)
                {
                  foreach (ReferencedPoint sfFacePoint in this.FindSFFacePoints(familyInstance, view, viewTransform, principalAxis, perpendicularAxis, num5, num4))
                    list.Add(sfFacePoint);
                }
                foreach (ReferencedPoint referencedPoint34 in list)
                {
                  if (ProjectionUtils.ProjectPointOnDirection(referencedPoint34.Point, perpendicularAxis).ApproximatelyEquals(num5, 0.00032552083333333332) || ProjectionUtils.ProjectPointOnDirection(referencedPoint34.Point, perpendicularAxis).ApproximatelyEquals(num4, 0.00032552083333333332))
                    referencedPointList1.Add(referencedPoint34);
                }
                if (referencedPointList1.Count == 0)
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              if (element1.GetSubComponentIds().ToList<Autodesk.Revit.DB.ElementId>().Count > 0)
              {
                if (this.CheckForSubCompsInList(element1, revitDoc, elementIds))
                {
                  this.AddToErrorList(familyInstance);
                  continue;
                }
                continue;
              }
              this.AddToErrorList(familyInstance);
              continue;
            case DimElementType.Void:
            case DimElementType.DimItems:
              if (famInst == null)
              {
                element1 = revitDoc.GetElement(elementId);
                famInst = element1 as Autodesk.Revit.DB.FamilyInstance;
              }
              if (revitDoc.GetElement(new Reference(revitDoc.GetElement(elementId)).ElementId) is Autodesk.Revit.DB.FamilyInstance element4)
              {
                List<Line> refLines = new List<Line>();
                List<ReferencedPoint> dimLineReference = HiddenGeomReferenceCalculatorEDrawing.GetDimLineReference(element4, principalAxis, view, out refLines);
                int count4 = dimLineReference.Count;
                List<ReferencedPoint> referencedPointList8 = this.CheckRefPointVisibility(dimLineReference, view);
                List<Line> lineList = this.CheckRefLineVisibility(refLines, view);
                bool flag = false;
                if (lineList.Count > 0)
                {
                  string str = !element4.Name.Equals(element4.Symbol.Family.Name.ToString()) ? $"{element4.Symbol.Family.Name} - {element4.Name}" : element4.Name;
                  if (!this.invisibleElemsList.Contains($"{elementId?.ToString()} - {str}"))
                  {
                    this.invisibleElemsList.Add($"{elementId?.ToString()} - {str}");
                    flag = true;
                  }
                }
                if ((referencedPointList8 == null || referencedPointList8.Count == 0) && !flag)
                {
                  string str = !element4.Name.Equals(element4.Symbol.Family.Name.ToString()) ? $"{element4.Symbol.Family.Name} - {element4.Name}" : element4.Name;
                  if (!this.insufficientDimLinesGridsLevelsList.Keys.Contains<string>(stringBuilder.ToString()))
                    this.insufficientDimLinesGridsLevelsList.Add(stringBuilder.ToString(), new List<string>()
                    {
                      $"{elementId?.ToString()} - {str}"
                    });
                  else
                    this.insufficientDimLinesGridsLevelsList[stringBuilder.ToString()].Add($"{elementId?.ToString()} - {str}");
                  --count;
                }
                using (List<ReferencedPoint>.Enumerator enumerator = referencedPointList8.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    ReferencedPoint current = enumerator.Current;
                    referencedPointList1.Add(current);
                  }
                  continue;
                }
              }
              continue;
            case DimElementType.GridLevel:
              Autodesk.Revit.DB.Element element5 = revitDoc.GetElement(elementId);
              Line line = (Line) null;
              if (element5 is Autodesk.Revit.DB.Grid)
              {
                Curve curve = (element5 as Autodesk.Revit.DB.Grid).Curve;
                if (curve is Line)
                  line = curve as Line;
              }
              else if (element5 is Autodesk.Revit.DB.Level)
              {
                List<Curve> list = (element5 as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
                if (list[0] is Line)
                  line = list[0] as Line;
              }
              if ((GeometryObject) line != (GeometryObject) null)
              {
                ReferencedPoint referencedPoint = new ReferencedPoint()
                {
                  Reference = new Reference(element5),
                  Point = line.GetEndPoint(0)
                };
                referencedPointList2.Add(referencedPoint);
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      foreach (Autodesk.Revit.DB.ElementId currGridLevel in currGridLevels)
      {
        Autodesk.Revit.DB.Element element6 = revitDoc.GetElement(currGridLevel);
        Line line = (Line) null;
        if (element6 is Autodesk.Revit.DB.Grid)
        {
          Curve curve = (element6 as Autodesk.Revit.DB.Grid).Curve;
          if (curve is Line)
            line = curve as Line;
        }
        else if (element6 is Autodesk.Revit.DB.Level)
        {
          List<Curve> list = (element6 as Autodesk.Revit.DB.Level).GetCurvesInView(DatumExtentType.ViewSpecific, revitDoc.ActiveView).ToList<Curve>();
          if (list[0] is Line)
            line = list[0] as Line;
        }
        if ((GeometryObject) line != (GeometryObject) null)
        {
          ReferencedPoint referencedPoint = new ReferencedPoint()
          {
            Reference = new Reference(element6),
            Point = line.GetEndPoint(0)
          };
          referencedPointList2.Add(referencedPoint);
        }
      }
      DimensionEdge dimensionEdge = DimensionEdge.Top;
      if (referencedPointList1.Count > 0)
        dimensionEdge = placementCalculator.DetermineDimEdge(overallPointCloud);
      Autodesk.Revit.DB.TextNote textNote = (Autodesk.Revit.DB.TextNote) null;
      Autodesk.Revit.DB.DimensionType dimensionType = (Autodesk.Revit.DB.DimensionType) null;
      string str1 = "";
      bool flag1 = true;
      Autodesk.Revit.DB.ElementId elementId1 = revitDoc.GetDefaultElementTypeId(Autodesk.Revit.DB.ElementTypeGroup.TextNoteType);
      foreach (AutoTicketSettingsTools eDrawingSetting in this.eDrawingSettings)
      {
        if (eDrawingSetting is AutoTicketCalloutAndDimensionTexts)
        {
          AutoTicketCalloutAndDimensionTexts andDimensionTexts = eDrawingSetting as AutoTicketCalloutAndDimensionTexts;
          CalloutStyle calloutStyle = new CalloutStyle(revitDoc, AutoTicketEnhancementWorkflow.AutoDimensionEDrawing, "", andDimensionTexts.OverallDimension, "", andDimensionTexts.TextStyle);
          if (calloutStyle.textNoteStyle != null)
            elementId1 = calloutStyle.textNoteStyle.Id;
          dimensionType = calloutStyle.dimensionStyle != null ? calloutStyle.dimensionStyle : (Autodesk.Revit.DB.DimensionType) null;
        }
        if (eDrawingSetting is AutoTicketAppendString)
        {
          str1 = new AppendStringEvaluate((eDrawingSetting as AutoTicketAppendString).AppendStringValue, element1, famInst, count, appendStringParameterData, (string) null).resultant;
          flag1 = false;
        }
      }
      if (addAppendString && count != 0)
      {
        Autodesk.Revit.DB.ElementId elementId2 = elementId1;
        Autodesk.Revit.DB.TextNoteOptions options = new Autodesk.Revit.DB.TextNoteOptions();
        options.Rotation = dimPlacementInfo.TextAngle;
        options.TypeId = elementId2;
        string str2 = !string.IsNullOrWhiteSpace(element1.GetIdentityDescriptionShort()) ? element1.GetIdentityDescriptionShort() : (famInst != null ? famInst.Symbol.FamilyName : "");
        textNote = Autodesk.Revit.DB.TextNote.Create(revitDoc, view.Id, new XYZ(0.0, 0.0, 0.0), flag1 ? $"({count.ToString()}) {str2}" : str1, options);
        revitDoc.Regenerate();
      }
      DimPlacementInfo dimLocation = placementCalculator.DetermineDimLocation(referencedPointList1, referencedPointList2, dimPlacementInfo, dimensionEdge, textNote, overallPointCloud);
      List<ReferencedPoint> list1 = referencedPointList1.Union<ReferencedPoint>((IEnumerable<ReferencedPoint>) referencedPointList2).ToList<ReferencedPoint>();
      DimReferencesManager referencesManager = new DimReferencesManager(revitDoc, dimensionFromEdge, dimLocation.DimAlongDirection, placementCalculator.GetDirectionVectorForDimPlacement(dimensionEdge));
      foreach (ReferencedPoint rP in list1)
      {
        int num6 = (int) referencesManager.AddReferenceED(rP);
      }
      if (referencesManager.DimReferences.Size < 2)
      {
        this.shortCurveError = true;
        return (Result) 1;
      }
      dimLocation.TextPlacementPoint = placementCalculator.GetTextPlacementPoint(dimLocation.PlacementLine, dimensionEdge, list1);
      Autodesk.Revit.DB.Dimension dimension = revitDoc.Create.NewDimension(view, dimLocation.PlacementLine, referencesManager.DimReferences);
      if (dimensionType != null)
        dimension.DimensionType = dimensionType;
      this.previousDimension = dimension;
      if (addAppendString && count != 0)
      {
        ElementTransformUtils.MoveElement(revitDoc, textNote.Id, dimLocation.TextPlacementPoint);
        textNote.Coord = placementCalculator.BumpNote(textNote, dimensionEdge);
        Parameters.GetParameterAsDouble((Autodesk.Revit.DB.Element) textNote.TextNoteType, BuiltInParameter.TEXT_SIZE);
        XYZ upDirection = textNote.UpDirection;
        double num7 = textNote.Height / 2.0 * (double) view.Scale;
        XYZ translation = upDirection.Normalize() * num7;
        ElementTransformUtils.MoveElement(revitDoc, textNote.Id, translation);
        Entity entity = new Entity(schema);
        schema.GetField("FileName");
        entity.Set<string>("PTACDimensionGuid", textNote.UniqueId);
        textNote.SetEntity(entity);
      }
      if (eqForm)
        dimension.get_Parameter(BuiltInParameter.DIM_DISPLAY_EQ)?.Set(2);
      return (Result) 0;
    }
    catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
    {
      return (Result) -1;
    }
  }

  private void AddToErrorList(Autodesk.Revit.DB.FamilyInstance sfFamInst)
  {
    string str = !sfFamInst.Name.Equals(sfFamInst.Symbol.Family.Name.ToString()) ? $"{sfFamInst.Symbol.Family.Name} - {sfFamInst.Name}" : sfFamInst.Name;
    if (this.invisibleElemsList.Contains($"{sfFamInst.Id?.ToString()} - {str}"))
      return;
    this.invisibleElemsList.Add($"{sfFamInst.Id?.ToString()} - {str}");
  }

  private bool CheckForSubCompsInList(
    Autodesk.Revit.DB.Element partElem,
    Document revitDoc,
    List<Autodesk.Revit.DB.ElementId> elementIds)
  {
    bool flag = true;
    foreach (Autodesk.Revit.DB.ElementId subComponentId in partElem.GetSubComponentIds())
    {
      if (revitDoc.GetElement(subComponentId) is Autodesk.Revit.DB.FamilyInstance)
      {
        Autodesk.Revit.DB.FamilyInstance element = revitDoc.GetElement(subComponentId) as Autodesk.Revit.DB.FamilyInstance;
        string str = !element.Name.Equals(element.Symbol.Family.Name.ToString()) ? $"{element.Symbol.Family.Name} - {element.Name}" : element.Name;
        if (!this.invisibleElemsList.Contains($"{subComponentId?.ToString()} - {str}") && elementIds.Contains(subComponentId))
        {
          flag = false;
          break;
        }
      }
    }
    return flag;
  }

  private List<ReferencedPoint> FindSFFacePoints(
    Autodesk.Revit.DB.FamilyInstance sfFamInst,
    View view,
    Transform axisTransform,
    XYZ principalAxis,
    XYZ perpendicularAxis,
    double maxProjDim,
    double minProjDim)
  {
    List<ReferencedPoint> sfFacePoints = new List<ReferencedPoint>();
    Transform transform = axisTransform != null ? axisTransform : Utils.ViewUtils.ViewUtils.GetViewTransform(view);
    Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>> dictionary = new Dictionary<DimensionEdge, Dictionary<DimensionEdge, ReferencedPoint>>();
    List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
    Autodesk.Revit.DB.FamilyInstance elem = sfFamInst;
    bool flag;
    ref bool local = ref flag;
    foreach (Solid symbolSolid in Solids.GetSymbolSolids((Autodesk.Revit.DB.Element) elem, out local, options: new Autodesk.Revit.DB.Options()
    {
      ComputeReferences = true
    }))
    {
      foreach (Autodesk.Revit.DB.Face face in symbolSolid.Faces)
      {
        if (face is PlanarFace)
        {
          PlanarFace pf = face as PlanarFace;
          if (Util.IsParallel(pf.FaceNormal, perpendicularAxis) && (ProjectionUtils.ProjectPointOnDirection(pf.Origin, perpendicularAxis).ApproximatelyEquals(maxProjDim, 0.00032552083333333332) || ProjectionUtils.ProjectPointOnDirection(pf.Origin, perpendicularAxis).ApproximatelyEquals(minProjDim, 0.00032552083333333332)) && this.CheckPlanarFaceVisibility(pf, view))
            sfFacePoints.Add(new ReferencedPoint(pf.Reference, pf.Origin, transform.Inverse.OfPoint(pf.Origin)));
        }
      }
    }
    return sfFacePoints;
  }

  private bool CheckPlanarFaceVisibility(PlanarFace pf, View view)
  {
    if (!view.CropBoxActive)
      return true;
    switch (view.ViewType)
    {
      case Autodesk.Revit.DB.ViewType.FloorPlan:
      case Autodesk.Revit.DB.ViewType.CeilingPlan:
      case Autodesk.Revit.DB.ViewType.EngineeringPlan:
      case Autodesk.Revit.DB.ViewType.AreaPlan:
        foreach (Autodesk.Revit.DB.Face face in Solids.GetBBoxSolid(this.cropPlusPlanBB).Faces)
        {
          Curve result;
          if (face.Intersect((Autodesk.Revit.DB.Face) pf, out result) != FaceIntersectionFaceResult.NonIntersecting && result.IsBound)
          {
            XYZ endPoint1 = result.GetEndPoint(0);
            XYZ endPoint2 = result.GetEndPoint(1);
            IntersectionResult intersectionResult1 = pf.Project(endPoint1);
            IntersectionResult intersectionResult2 = pf.Project(endPoint2);
            if (intersectionResult1 != null && intersectionResult2 != null)
            {
              Autodesk.Revit.DB.UV point1 = new Autodesk.Revit.DB.UV(intersectionResult1.UVPoint.U, intersectionResult1.UVPoint.V);
              Autodesk.Revit.DB.UV point2 = new Autodesk.Revit.DB.UV(intersectionResult2.UVPoint.U, intersectionResult2.UVPoint.V);
              IntersectionResult intersectionResult3 = face.Project(endPoint1);
              IntersectionResult intersectionResult4 = face.Project(endPoint2);
              if (intersectionResult3 != null && intersectionResult4 != null)
              {
                Autodesk.Revit.DB.UV point3 = new Autodesk.Revit.DB.UV(intersectionResult3.UVPoint.U, intersectionResult3.UVPoint.V);
                Autodesk.Revit.DB.UV point4 = new Autodesk.Revit.DB.UV(intersectionResult4.UVPoint.U, intersectionResult4.UVPoint.V);
                if (face.IsInside(point3) & face.IsInside(point4) & pf.IsInside(point1) & pf.IsInside(point2))
                  return true;
              }
            }
          }
        }
        return false;
      case Autodesk.Revit.DB.ViewType.Elevation:
      case Autodesk.Revit.DB.ViewType.Section:
      case Autodesk.Revit.DB.ViewType.Detail:
        foreach (Autodesk.Revit.DB.Face face in Solids.GetBBoxSolid(this.cropPlusClipBB).Faces)
        {
          Curve result;
          if (face.Intersect((Autodesk.Revit.DB.Face) pf, out result) != FaceIntersectionFaceResult.NonIntersecting && result.IsBound)
          {
            XYZ endPoint3 = result.GetEndPoint(0);
            XYZ endPoint4 = result.GetEndPoint(1);
            IntersectionResult intersectionResult5 = pf.Project(endPoint3);
            IntersectionResult intersectionResult6 = pf.Project(endPoint4);
            if (intersectionResult5 != null && intersectionResult6 != null)
            {
              Autodesk.Revit.DB.UV point5 = new Autodesk.Revit.DB.UV(intersectionResult5.UVPoint.U, intersectionResult5.UVPoint.V);
              Autodesk.Revit.DB.UV point6 = new Autodesk.Revit.DB.UV(intersectionResult6.UVPoint.U, intersectionResult6.UVPoint.V);
              IntersectionResult intersectionResult7 = face.Project(endPoint3);
              IntersectionResult intersectionResult8 = face.Project(endPoint4);
              if (intersectionResult7 != null && intersectionResult8 != null)
              {
                Autodesk.Revit.DB.UV point7 = new Autodesk.Revit.DB.UV(intersectionResult7.UVPoint.U, intersectionResult7.UVPoint.V);
                Autodesk.Revit.DB.UV point8 = new Autodesk.Revit.DB.UV(intersectionResult8.UVPoint.U, intersectionResult8.UVPoint.V);
                if (face.IsInside(point7) & face.IsInside(point8) & pf.IsInside(point5) & pf.IsInside(point6))
                  return true;
              }
            }
          }
        }
        return false;
      default:
        return true;
    }
  }

  private DimElementType CheckElementType(Autodesk.Revit.DB.Element part)
  {
    DimElementType dimElementType = DimElementType.SF;
    List<Autodesk.Revit.DB.ElementId> list1 = new List<Autodesk.Revit.DB.BuiltInCategory>()
    {
      Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel,
      Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment,
      Autodesk.Revit.DB.BuiltInCategory.OST_StructConnections
    }.Select<Autodesk.Revit.DB.BuiltInCategory, Autodesk.Revit.DB.ElementId>((Func<Autodesk.Revit.DB.BuiltInCategory, Autodesk.Revit.DB.ElementId>) (id => new Autodesk.Revit.DB.ElementId(id))).ToList<Autodesk.Revit.DB.ElementId>();
    List<Autodesk.Revit.DB.ElementId> list2 = new List<Autodesk.Revit.DB.BuiltInCategory>()
    {
      Autodesk.Revit.DB.BuiltInCategory.OST_Grids,
      Autodesk.Revit.DB.BuiltInCategory.OST_Levels
    }.Select<Autodesk.Revit.DB.BuiltInCategory, Autodesk.Revit.DB.ElementId>((Func<Autodesk.Revit.DB.BuiltInCategory, Autodesk.Revit.DB.ElementId>) (id => new Autodesk.Revit.DB.ElementId(id))).ToList<Autodesk.Revit.DB.ElementId>();
    if (part.Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming)))
      dimElementType = DimElementType.SF;
    else if ((part.Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_GenericModel)) || part.Category.Id.Equals((object) new Autodesk.Revit.DB.ElementId(Autodesk.Revit.DB.BuiltInCategory.OST_SpecialityEquipment))) && Parameters.GetParameterAsString(part, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID"))
      dimElementType = DimElementType.Void;
    else if (list1.Contains(part.Category.Id) && !Parameters.GetParameterAsString(part, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID") && !Parameters.GetParameterAsString(part, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW") && !Parameters.GetParameterAsString(part, "MANUFACTURE_COMPONENT").ToUpper().Contains("CONSUMABLE") && !(part as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName.Contains("CONNECTOR") && !(part as Autodesk.Revit.DB.FamilyInstance).Symbol.FamilyName.Contains("COMPONENT"))
      dimElementType = DimElementType.DimItems;
    else if (list2.Contains(part.Category.Id))
      dimElementType = DimElementType.GridLevel;
    return dimElementType;
  }

  private DimensionEdge GetOppDimEdge(DimensionEdge dEdge)
  {
    switch (dEdge)
    {
      case DimensionEdge.Left:
        return DimensionEdge.Right;
      case DimensionEdge.Right:
        return DimensionEdge.Left;
      case DimensionEdge.Top:
        return DimensionEdge.Bottom;
      case DimensionEdge.Bottom:
        return DimensionEdge.Top;
      default:
        return DimensionEdge.None;
    }
  }

  public bool IsReferencePointVisibleWithinView(View view, ReferencedPoint rp)
  {
    if (view.CropBoxActive)
    {
      XYZ xyz1 = this.cropBoxInverseTransform.OfPoint(rp.Point);
      switch (view.ViewType)
      {
        case Autodesk.Revit.DB.ViewType.FloorPlan:
        case Autodesk.Revit.DB.ViewType.CeilingPlan:
        case Autodesk.Revit.DB.ViewType.EngineeringPlan:
        case Autodesk.Revit.DB.ViewType.AreaPlan:
          XYZ max1 = this.cropPlusPlanBB.Max;
          XYZ min1 = this.cropPlusPlanBB.Min;
          return (min1.X < xyz1.X && xyz1.X < max1.X) & (min1.Y < xyz1.Y && xyz1.Y < max1.Y) & (min1.Z < xyz1.Z && xyz1.Z < max1.Z);
        case Autodesk.Revit.DB.ViewType.Elevation:
        case Autodesk.Revit.DB.ViewType.Section:
        case Autodesk.Revit.DB.ViewType.Detail:
          XYZ xyz2 = view.CropBox.Transform.Inverse.OfPoint(rp.Point);
          XYZ max2 = this.cropPlusClipBB.Max;
          XYZ min2 = this.cropPlusClipBB.Min;
          return (min2.X < xyz2.X && xyz2.X < max2.X) & (min2.Y < xyz2.Y && xyz2.Y < max2.Y) & (min2.Z < xyz2.Z && xyz2.Z < max2.Z);
        default:
          return true;
      }
    }
    else
    {
      if (view.ViewType != Autodesk.Revit.DB.ViewType.AreaPlan && view.ViewType != Autodesk.Revit.DB.ViewType.CeilingPlan && view.ViewType != Autodesk.Revit.DB.ViewType.EngineeringPlan && view.ViewType != Autodesk.Revit.DB.ViewType.FloorPlan)
        return true;
      XYZ point = rp.Point;
      if (!(view is ViewPlan))
        return true;
      Autodesk.Revit.DB.PlanViewRange viewRange = (view as ViewPlan).GetViewRange();
      Autodesk.Revit.DB.ElementId levelId1 = viewRange.GetLevelId(PlanViewPlane.TopClipPlane);
      Autodesk.Revit.DB.ElementId levelId2 = viewRange.GetLevelId(PlanViewPlane.ViewDepthPlane);
      Autodesk.Revit.DB.Level element1 = view.Document.GetElement(levelId1) as Autodesk.Revit.DB.Level;
      Autodesk.Revit.DB.Level element2 = view.Document.GetElement(levelId2) as Autodesk.Revit.DB.Level;
      double num = levelId1.IntegerValue <= 0 ? double.MaxValue : element1.ProjectElevation + viewRange.GetOffset(PlanViewPlane.TopClipPlane);
      return ((levelId2.IntegerValue <= 0 ? double.MinValue : element2.ProjectElevation + viewRange.GetOffset(PlanViewPlane.ViewDepthPlane)) >= point.Z ? 0 : (point.Z < num ? 1 : 0)) != 0;
    }
  }

  public List<ReferencedPoint> CheckRefPointVisibility(List<ReferencedPoint> refPoints, View view)
  {
    if (view.CropBoxActive)
    {
      List<ReferencedPoint> referencedPointList = new List<ReferencedPoint>();
      foreach (ReferencedPoint refPoint in refPoints)
      {
        XYZ xyz1 = this.cropBoxInverseTransform.OfPoint(refPoint.Point);
        switch (view.ViewType)
        {
          case Autodesk.Revit.DB.ViewType.FloorPlan:
          case Autodesk.Revit.DB.ViewType.CeilingPlan:
          case Autodesk.Revit.DB.ViewType.EngineeringPlan:
          case Autodesk.Revit.DB.ViewType.AreaPlan:
            XYZ max1 = this.cropPlusPlanBB.Max;
            XYZ min1 = this.cropPlusPlanBB.Min;
            if ((min1.X < xyz1.X && xyz1.X < max1.X) & (min1.Y < xyz1.Y && xyz1.Y < max1.Y) & (min1.Z < xyz1.Z && xyz1.Z < max1.Z))
            {
              referencedPointList.Add(refPoint);
              continue;
            }
            continue;
          case Autodesk.Revit.DB.ViewType.Elevation:
          case Autodesk.Revit.DB.ViewType.Section:
          case Autodesk.Revit.DB.ViewType.Detail:
            XYZ xyz2 = view.CropBox.Transform.Inverse.OfPoint(refPoint.Point);
            XYZ max2 = this.cropPlusClipBB.Max;
            XYZ min2 = this.cropPlusClipBB.Min;
            if ((min2.X < xyz2.X && xyz2.X < max2.X) & (min2.Y < xyz2.Y && xyz2.Y < max2.Y) & (min2.Z < xyz2.Z && xyz2.Z < max2.Z))
            {
              referencedPointList.Add(refPoint);
              continue;
            }
            continue;
          default:
            continue;
        }
      }
      return referencedPointList;
    }
    if (view.ViewType != Autodesk.Revit.DB.ViewType.AreaPlan && view.ViewType != Autodesk.Revit.DB.ViewType.CeilingPlan && view.ViewType != Autodesk.Revit.DB.ViewType.EngineeringPlan && view.ViewType != Autodesk.Revit.DB.ViewType.FloorPlan)
      return refPoints;
    List<ReferencedPoint> referencedPointList1 = new List<ReferencedPoint>();
    foreach (ReferencedPoint refPoint in refPoints)
    {
      XYZ point = refPoint.Point;
      if (view is ViewPlan)
      {
        Autodesk.Revit.DB.PlanViewRange viewRange = (view as ViewPlan).GetViewRange();
        Autodesk.Revit.DB.ElementId levelId1 = viewRange.GetLevelId(PlanViewPlane.TopClipPlane);
        Autodesk.Revit.DB.ElementId levelId2 = viewRange.GetLevelId(PlanViewPlane.ViewDepthPlane);
        Autodesk.Revit.DB.Level element1 = view.Document.GetElement(levelId1) as Autodesk.Revit.DB.Level;
        Autodesk.Revit.DB.Level element2 = view.Document.GetElement(levelId2) as Autodesk.Revit.DB.Level;
        double num = levelId1.IntegerValue <= 0 ? double.MaxValue : element1.ProjectElevation + viewRange.GetOffset(PlanViewPlane.TopClipPlane);
        if (((levelId2.IntegerValue <= 0 ? double.MinValue : element2.ProjectElevation + viewRange.GetOffset(PlanViewPlane.ViewDepthPlane)) >= point.Z ? 0 : (point.Z < num ? 1 : 0)) != 0)
          referencedPointList1.Add(refPoint);
      }
      else
        referencedPointList1.Add(refPoint);
    }
    return referencedPointList1;
  }

  public List<Line> CheckRefLineVisibility(List<Line> refPoints, View view)
  {
    if (view.CropBoxActive)
    {
      List<Line> lineList = new List<Line>();
      foreach (Line refPoint in refPoints)
      {
        XYZ xyz1 = this.cropBoxInverseTransform.OfPoint(refPoint.GetEndPoint(0));
        XYZ xyz2 = this.cropBoxInverseTransform.OfPoint(refPoint.GetEndPoint(1));
        switch (view.ViewType)
        {
          case Autodesk.Revit.DB.ViewType.FloorPlan:
          case Autodesk.Revit.DB.ViewType.CeilingPlan:
          case Autodesk.Revit.DB.ViewType.EngineeringPlan:
          case Autodesk.Revit.DB.ViewType.AreaPlan:
            XYZ max1 = this.cropPlusPlanBB.Max;
            XYZ min1 = this.cropPlusPlanBB.Min;
            int num1 = min1.X >= xyz1.X || xyz1.X >= max1.X ? (min1.X >= xyz2.X ? 0 : (xyz2.X < max1.X ? 1 : 0)) : 1;
            bool flag1 = min1.Y < xyz1.Y && xyz1.Y < max1.Y || min1.Y < xyz2.Y && xyz2.Y < max1.Y;
            bool flag2 = min1.Z < xyz1.Z && xyz1.Z < max1.Z || min1.Z < xyz2.Z && xyz2.Z < max1.Z;
            int num2 = flag1 ? 1 : 0;
            if ((num1 & num2 & (flag2 ? 1 : 0)) == 0)
            {
              lineList.Add(refPoint);
              continue;
            }
            continue;
          case Autodesk.Revit.DB.ViewType.Elevation:
          case Autodesk.Revit.DB.ViewType.Section:
          case Autodesk.Revit.DB.ViewType.Detail:
            XYZ xyz3 = view.CropBox.Transform.Inverse.OfPoint(refPoint.GetEndPoint(0));
            XYZ xyz4 = view.CropBox.Transform.Inverse.OfPoint(refPoint.GetEndPoint(1));
            XYZ max2 = this.cropPlusClipBB.Max;
            XYZ min2 = this.cropPlusClipBB.Min;
            int num3 = min2.X >= xyz3.X || xyz3.X >= max2.X ? (min2.X >= xyz4.X ? 0 : (xyz4.X < max2.X ? 1 : 0)) : 1;
            bool flag3 = min2.Y < xyz3.Y && xyz3.Y < max2.Y || min2.Y < xyz4.Y && xyz4.Y < max2.Y;
            bool flag4 = min2.Z < xyz3.Z && xyz3.Z < max2.Z || min2.Z < xyz4.Z && xyz4.Z < max2.Z;
            int num4 = flag3 ? 1 : 0;
            if ((num3 & num4 & (flag4 ? 1 : 0)) == 0)
            {
              lineList.Add(refPoint);
              continue;
            }
            continue;
          default:
            continue;
        }
      }
      return lineList;
    }
    if (view.ViewType != Autodesk.Revit.DB.ViewType.AreaPlan && view.ViewType != Autodesk.Revit.DB.ViewType.CeilingPlan && view.ViewType != Autodesk.Revit.DB.ViewType.EngineeringPlan && view.ViewType != Autodesk.Revit.DB.ViewType.FloorPlan)
      return new List<Line>();
    List<Line> lineList1 = new List<Line>();
    foreach (Line refPoint in refPoints)
    {
      XYZ endPoint1 = refPoint.GetEndPoint(0);
      XYZ endPoint2 = refPoint.GetEndPoint(1);
      if (view is ViewPlan)
      {
        Autodesk.Revit.DB.PlanViewRange viewRange = (view as ViewPlan).GetViewRange();
        Autodesk.Revit.DB.ElementId levelId1 = viewRange.GetLevelId(PlanViewPlane.TopClipPlane);
        Autodesk.Revit.DB.ElementId levelId2 = viewRange.GetLevelId(PlanViewPlane.ViewDepthPlane);
        Autodesk.Revit.DB.Level element1 = view.Document.GetElement(levelId1) as Autodesk.Revit.DB.Level;
        Autodesk.Revit.DB.Level element2 = view.Document.GetElement(levelId2) as Autodesk.Revit.DB.Level;
        double num5 = levelId1.IntegerValue <= 0 ? double.MaxValue : element1.ProjectElevation + viewRange.GetOffset(PlanViewPlane.TopClipPlane);
        double num6 = levelId2.IntegerValue <= 0 ? double.MinValue : element2.ProjectElevation + viewRange.GetOffset(PlanViewPlane.ViewDepthPlane);
        if ((num6 >= endPoint1.Z || endPoint1.Z >= num5 ? (num6 >= endPoint2.Z ? 0 : (endPoint2.Z < num5 ? 1 : 0)) : 1) == 0)
          lineList1.Add(refPoint);
      }
    }
    return lineList1;
  }
}
