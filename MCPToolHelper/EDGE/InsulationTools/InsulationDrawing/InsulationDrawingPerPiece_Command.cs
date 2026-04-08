// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingPerPiece_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.InsulationTools.InsulationDrawing.Views;
using EDGE.UserSettingTools.Insulation_Drawing_Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

[Transaction(TransactionMode.Manual)]
public class InsulationDrawingPerPiece_Command : IExternalCommand
{
  private ObservableCollection<InsulationDrawingPerPieceObject> ppoList = new ObservableCollection<InsulationDrawingPerPieceObject>();

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    string str = "Insulation Drawing - Assembly";
    if (document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = (str + " Must be run in the Project Environment"),
        MainContent = $"You are currently in the family editor, {str} must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    string manufacturer = "";
    Parameter parameter = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && parameter.StorageType == StorageType.String)
      manufacturer = parameter.AsString();
    if (string.IsNullOrWhiteSpace(manufacturer))
      TaskDialog.Show("Insulaion Drawing Settings", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to run Insulation Drawing - Assembly again.");
    InsulationDrawingSettings insulationDrawingSettings1 = new InsulationDrawingSettings(manufacturer);
    InsulationDrawingSettingsObject settingsObject;
    if (!insulationDrawingSettings1.SettingsRead())
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
          MainContent = "Insulation Drawing Settings file not found. Insulation Drawing Per Piece will use default settings."
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
      settingsObject = insulationDrawingSettings1.GetSettings(document);
    if (settingsObject == null)
    {
      new TaskDialog("Insulation Drawing Settings")
      {
        MainInstruction = "Insulation Drawing Settings not found.",
        MainContent = "One or more insulation drawing settings are not present the project. Please update settings and try again."
      }.Show();
      return (Result) 1;
    }
    if (this.CheckForLegends(document) == 0)
    {
      TaskDialog.Show("Insulation Drawing Settings", "At least one legend must exist in the project for the Insulation Drawing - Assembly tool to run. Please create a legend and run the tool again.");
      return (Result) 1;
    }
    List<AssemblyInstance> assemblyInstanceList1 = new List<AssemblyInstance>();
    ObservableCollection<InsulationDrawingPerPieceObject> Ppo = new ObservableCollection<InsulationDrawingPerPieceObject>();
    List<ElementId> list1 = activeUiDocument.Selection.GetElementIds().ToList<ElementId>();
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    if (list1.Count != 0)
    {
      foreach (AssemblyInstance assemblyInstance in new FilteredElementCollector(document, (ICollection<ElementId>) list1).OfCategory(BuiltInCategory.OST_Assemblies).Cast<AssemblyInstance>().ToList<AssemblyInstance>())
      {
        if (!Parameters.GetParameterAsBool((Element) assemblyInstance, "HARDWARE_DETAIL"))
        {
          List<ElementId> list2 = assemblyInstance.GetMemberIds().ToList<ElementId>();
          if (new FilteredElementCollector(document, (ICollection<ElementId>) list2).OfCategory(BuiltInCategory.OST_StructuralFraming).Any<Element>())
          {
            List<ViewSheet> sheets = this.GatherSheets(assemblyInstance, document);
            if (sheets.Count != 0)
            {
              List<FamilyInstance> list3 = new FilteredElementCollector(document, (ICollection<ElementId>) list2).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (x => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(x))).Cast<FamilyInstance>().ToList<FamilyInstance>();
              Dictionary<string, FamilyInstance> insulationDict = new Dictionary<string, FamilyInstance>();
              List<string> stringList = new List<string>();
              bool containsUnmarked = false;
              foreach (FamilyInstance familyInstance in list3)
              {
                string parameterAsString = Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_MARK");
                if (string.IsNullOrWhiteSpace(parameterAsString))
                  containsUnmarked = true;
                else if (InsulationDrawingUtils.InsulationLocked((Element) familyInstance) && !stringList.Contains(parameterAsString))
                {
                  stringList.Add(parameterAsString);
                  if (InsulationDrawingUtils.checkVolumeDifferenceInsulation(familyInstance))
                    insulationDict.Add(parameterAsString, familyInstance);
                }
              }
              if (insulationDict.Count != 0)
              {
                InsulationDrawingPerPieceObject drawingPerPieceObject = new InsulationDrawingPerPieceObject(assemblyInstance, sheets, insulationDict, containsUnmarked);
                Ppo.Add(drawingPerPieceObject);
              }
            }
          }
        }
      }
    }
    if (Ppo.Count == 0)
    {
      List<AssemblyInstance> assemblyInstanceList2 = new List<AssemblyInstance>();
      try
      {
        ICollection<Reference> references = (ICollection<Reference>) activeUiDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new AssemblyInstancesAndStructuralFraming(), "Pick Assemblies to draw insulation for.");
        if (references.Count > 0)
        {
          foreach (Reference reference in (IEnumerable<Reference>) references)
          {
            if (document.GetElement(reference) != null)
            {
              Element element = document.GetElement(reference);
              if (element is AssemblyInstance)
                assemblyInstanceList2.Add(element as AssemblyInstance);
            }
          }
        }
      }
      catch (Exception ex)
      {
        return (Result) 1;
      }
      if (assemblyInstanceList2.Count == 0)
        return (Result) 1;
      foreach (AssemblyInstance assembly in assemblyInstanceList2)
      {
        List<ViewSheet> sheets = this.GatherSheets(assembly, document);
        List<ElementId> list4 = assembly.GetMemberIds().ToList<ElementId>();
        List<FamilyInstance> list5 = new FilteredElementCollector(document, (ICollection<ElementId>) list4).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (x => Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent(x))).Cast<FamilyInstance>().ToList<FamilyInstance>();
        Dictionary<string, FamilyInstance> insulationDict = new Dictionary<string, FamilyInstance>();
        List<string> stringList = new List<string>();
        bool containsUnmarked = false;
        foreach (FamilyInstance familyInstance in list5)
        {
          string parameterAsString = Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_MARK");
          if (string.IsNullOrWhiteSpace(parameterAsString))
            containsUnmarked = true;
          else if (!stringList.Contains(parameterAsString))
          {
            stringList.Add(parameterAsString);
            if (InsulationDrawingUtils.checkVolumeDifferenceInsulation(familyInstance))
              insulationDict.Add(parameterAsString, familyInstance);
          }
        }
        if (insulationDict.Count != 0)
        {
          InsulationDrawingPerPieceObject drawingPerPieceObject = new InsulationDrawingPerPieceObject(assembly, sheets, insulationDict, containsUnmarked);
          Ppo.Add(drawingPerPieceObject);
        }
      }
    }
    if (Ppo.Count == 0)
      return (Result) 1;
    InsulationDrawingPerPieceWindow drawingPerPieceWindow = new InsulationDrawingPerPieceWindow(document, Ppo, settingsObject.InsulationDrawingScaleFactorPerPiece);
    drawingPerPieceWindow.ShowDialog();
    if (drawingPerPieceWindow.perPieceList == null)
      return (Result) 1;
    InsulationDrawingSettings insulationDrawingSettings2 = new InsulationDrawingSettings();
    ObservableCollection<InsulationDrawingPerPieceObject> perPieceList = drawingPerPieceWindow.perPieceList;
    settingsObject.InsulationDrawingScaleFactorPerPiece = drawingPerPieceWindow.ScaleFactor;
    ViewSheet legends = InsulationDrawingPerPiece.CreateLegends(perPieceList, settingsObject.InsulationDrawingScaleFactorPerPiece, settingsObject);
    if (legends != null)
      activeUiDocument.RequestViewChange((View) legends);
    return (Result) 0;
  }

  private int CheckForLegends(Document revitDoc)
  {
    return new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Views).Where<Element>((Func<Element, bool>) (v => (v as View).ViewType == ViewType.Legend)).Cast<View>().ToList<View>().Count;
  }

  private List<ViewSheet> GatherSheets(AssemblyInstance assembly, Document revitDoc)
  {
    List<ViewSheet> list = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Sheets).OfClass(typeof (ViewSheet)).Where<Element>((Func<Element, bool>) (s => (s as ViewSheet).AssociatedAssemblyInstanceId.Equals((object) assembly.Id))).Cast<ViewSheet>().ToList<ViewSheet>();
    List<ViewSheet> viewSheetList = new List<ViewSheet>();
    foreach (ViewSheet viewSheet in list)
    {
      if (new FilteredElementCollector(revitDoc, viewSheet.Id).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilyInstance)).Select<Element, FamilyInstance>((Func<Element, FamilyInstance>) (e => e as FamilyInstance)).Any<FamilyInstance>())
        viewSheetList.Add(viewSheet);
    }
    return viewSheetList;
  }

  private bool ValidForInsulationDrawing(
    Element checkElem,
    out bool containsUnmarked,
    out string insulationMark)
  {
    containsUnmarked = false;
    insulationMark = string.Empty;
    if (!(checkElem is FamilyInstance elem) || !Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent((Element) elem))
      return false;
    insulationMark = Parameters.GetParameterAsString((Element) elem, "INSULATION_MARK");
    if (string.IsNullOrWhiteSpace(insulationMark))
    {
      containsUnmarked = true;
      return false;
    }
    double parameterAsDouble1 = Parameters.GetParameterAsDouble((Element) elem, "DIM_LENGTH");
    if (parameterAsDouble1 <= 0.0)
      return false;
    double parameterAsDouble2 = Parameters.GetParameterAsDouble((Element) elem, "DIM_WIDTH");
    if (parameterAsDouble2 <= 0.0)
      return false;
    double parameterAsDouble3 = Parameters.GetParameterAsDouble((Element) elem, "DIM_THICKNESS");
    if (parameterAsDouble3 <= 0.0)
      return false;
    double other = parameterAsDouble1 * parameterAsDouble2 * parameterAsDouble3;
    Solid solid1 = (Solid) null;
    foreach (Solid instanceSolid in Solids.GetInstanceSolids((Element) elem))
      solid1 = !((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(instanceSolid, solid1, BooleanOperationsType.Union) : instanceSolid;
    return elem != null && !solid1.Volume.ApproximatelyEquals(other, 0.001);
  }
}
