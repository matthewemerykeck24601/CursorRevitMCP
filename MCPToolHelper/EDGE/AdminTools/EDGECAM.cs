// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.EDGECAM
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using CNCExport;
using CNCExport.Entity;
using CNCExport.Factory;
using CNCExport.UniCAMEntity700;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
internal class EDGECAM : IExternalCommand
{
  public const double slopeTolerance = 0.01;

  public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
  {
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    List<BoundingBoxXYZ> boundingBoxXyzList = new List<BoundingBoxXYZ>();
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    if (revitDoc.IsFamilyDocument)
    {
      new TaskDialog("Warning")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "EDGE^CAM must be run in the Project Environment",
        MainContent = "You are currently in the family editor, EDGE^CAM Export must be run in the project environment.  Please return to the project environment or open a project before running this tool."
      }.Show();
      return (Result) 1;
    }
    if (!revitDoc.ActiveView.IsAssemblyView && (revitDoc.ActiveView is View3D || revitDoc.ActiveView is ViewSection || revitDoc.ActiveView is ViewPlan) || revitDoc.ActiveView is ViewSheet)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^CAM Export");
      taskDialog.TitleAutoPrefix = false;
      taskDialog.MainInstruction = "Choose export method";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "All assembly elements in model");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Only for selected assembly elements");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1003, "All assembly elements in view");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 2)
        return (Result) 1;
      if (taskDialogResult == 1002)
        flag1 = true;
      else if (taskDialogResult == 1003)
        flag2 = true;
    }
    else if (revitDoc.ActiveView.IsAssemblyView && (revitDoc.ActiveView is View3D || revitDoc.ActiveView is ViewSection || revitDoc.ActiveView is ViewPlan))
    {
      TaskDialog taskDialog = new TaskDialog("EDGE^CAM Export");
      taskDialog.TitleAutoPrefix = false;
      taskDialog.MainInstruction = "Choose export method";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "All elements in Assembly");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Elements visible in Assembly view");
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 2)
        return (Result) 1;
      if (taskDialogResult == 1001)
        flag3 = true;
      else if (taskDialogResult == 1002)
        flag4 = true;
    }
    else
    {
      TaskDialog.Show("Invalid View", "This tool must be run from a valid view. Please try again.");
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(revitDoc, "EDGE^CAM"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        List<AssemblyInstance> assemblyInstanceList = new List<AssemblyInstance>();
        List<ElementId> elementsInAssemblyView = new List<ElementId>();
        if (flag1)
        {
          ICollection<ElementId> list = (ICollection<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds().Where<ElementId>((Func<ElementId, bool>) (eid => revitDoc.GetElement(eid) is AssemblyInstance)).ToList<ElementId>();
          if (list.Count < 1)
          {
            TaskDialog.Show("No Valid Assemblies", "You must select at least one valid assembly to use this option.");
            return (Result) 1;
          }
          foreach (ElementId id in (IEnumerable<ElementId>) list)
          {
            Element element = revitDoc.GetElement(id);
            AssemblyInstance assemblyInstance = (AssemblyInstance) null;
            if (element is AssemblyInstance)
              assemblyInstance = element as AssemblyInstance;
            assemblyInstanceList.Add(assemblyInstance);
          }
        }
        else if (flag2)
        {
          assemblyInstanceList = new FilteredElementCollector(revitDoc, revitDoc.ActiveView.Id).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().GroupBy<AssemblyInstance, string>((Func<AssemblyInstance, string>) (x => x.Name)).Select<IGrouping<string, AssemblyInstance>, AssemblyInstance>((Func<IGrouping<string, AssemblyInstance>, AssemblyInstance>) (x => x.First<AssemblyInstance>())).ToList<AssemblyInstance>();
          if (assemblyInstanceList.Count < 1)
          {
            TaskDialog.Show("No Valid Assemblies", "There must be at least one valid assembly in the current view to use this option.");
            return (Result) 1;
          }
        }
        else if (flag3)
          assemblyInstanceList.Add(revitDoc.GetElement(revitDoc.ActiveView.AssociatedAssemblyInstanceId) as AssemblyInstance);
        else if (flag4)
        {
          View activeView = revitDoc.ActiveView;
          AssemblyInstance element1 = revitDoc.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance;
          assemblyInstanceList.Add(element1);
          List<ElementId> assemblyIds = element1.GetMemberIds().ToList<ElementId>();
          List<Element> list = new FilteredElementCollector(revitDoc, activeView.Id).ToList<Element>().Where<Element>((Func<Element, bool>) (e => !(e is AssemblyInstance) && assemblyIds.Contains(e.Id))).ToList<Element>();
          if (list.Count < 1)
          {
            TaskDialog.Show("No Valid Elements", "There must be at least one valid element visible in this assembly view to use this option.");
            return (Result) 1;
          }
          foreach (Element element2 in list)
            elementsInAssemblyView.Add(element2.Id);
        }
        else
        {
          assemblyInstanceList = new FilteredElementCollector(revitDoc).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().GroupBy<AssemblyInstance, string>((Func<AssemblyInstance, string>) (x => x.Name)).Select<IGrouping<string, AssemblyInstance>, AssemblyInstance>((Func<IGrouping<string, AssemblyInstance>, AssemblyInstance>) (x => x.First<AssemblyInstance>())).ToList<AssemblyInstance>();
          if (assemblyInstanceList.Count < 1)
          {
            TaskDialog.Show("No Valid Assemblies", "There must be at least one valid assembly in the project to use this tool.");
            return (Result) 1;
          }
        }
        List<AssemblyInstance> hwdAssemblies = assemblyInstanceList.Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => CNCExport.Utils.ElementUtils.Parameters.GetParameterAsBool((Element) e, "HARDWARE_DETAIL"))).ToList<AssemblyInstance>();
        if (hwdAssemblies.Count == assemblyInstanceList.Count)
        {
          TaskDialog.Show("CAM Export", "All assemblies to be exported were flagged as hardware details. These assemblies are not compatible with the CAM Export tool.");
          int num2 = (int) transaction.RollBack();
          return (Result) -1;
        }
        if (hwdAssemblies.Count > 0)
        {
          assemblyInstanceList.RemoveAll((Predicate<AssemblyInstance>) (e => hwdAssemblies.Contains(e)));
          TaskDialog.Show("CAM Export", "One or more assemblies to be exported were flagged as hardware details. These assemblies are not compatible with the CAM Export tool and will be ignored.");
        }
        if (elementsInAssemblyView.Count > 0)
          this.ExportRaw(assemblyInstanceList, revitDoc, 0.01, elementsInAssemblyView);
        else
          this.ExportRaw(assemblyInstanceList, revitDoc, 0.01);
        int num3 = (int) transaction.Commit();
      }
      catch (Exception ex)
      {
        TaskDialog.Show("ERROR", ex.StackTrace);
        int num = (int) transaction.RollBack();
        return (Result) -1;
      }
    }
    return (Result) 0;
  }

  public bool SaveUnitechnik(
    AssemblyInstance assembly,
    Document revitDoc,
    double slopeTolerance,
    out List<EDGECAM.errorCondition> errors,
    List<Element> problemBars,
    string exportType,
    List<ElementId> elementsInAssemblyView = null)
  {
    errors = new List<EDGECAM.errorCondition>();
    DataFactory dataFactory = new DataFactory(assembly);
    List<ElementId> elementIdList1 = new List<ElementId>();
    List<ElementId> elementIdList2 = elementsInAssemblyView != null ? elementsInAssemblyView : assembly.GetMemberIds().ToList<ElementId>();
    List<ElementId> list1 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).OfCategory(BuiltInCategory.OST_Walls).ToElementIds().ToList<ElementId>();
    if (elementIdList2.Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320)).Count<Element>() > 1)
    {
      errors.Add(EDGECAM.errorCondition.TooManyStructuralFraming);
      return false;
    }
    Element structuralFramingElement = assembly.GetStructuralFramingElement();
    if (structuralFramingElement == null || structuralFramingElement.Category.Name != "Structural Framing")
    {
      errors.Add(EDGECAM.errorCondition.NoStructuralFraming);
      return false;
    }
    Element flatElement = (Element) AssemblyInstances.GetFlatElement(revitDoc, structuralFramingElement as FamilyInstance);
    Element elem1 = (structuralFramingElement as FamilyInstance).SuperComponent == null ? structuralFramingElement : (flatElement as FamilyInstance).SuperComponent;
    BoundingBoxXYZ transformedBoundingBox1 = Util.getTransformedBoundingBox(dataFactory.transform, flatElement);
    StructuralMaterialTypeFilter materialTypeFilter = new StructuralMaterialTypeFilter(StructuralMaterialType.PrecastConcrete);
    if (!(structuralFramingElement is FamilyInstance) || !materialTypeFilter.PassesFilter(structuralFramingElement))
    {
      errors.Add(EDGECAM.errorCondition.InvalidStructuralFraming);
      return false;
    }
    Dictionary<Solid, ElementId> dictionary = new Dictionary<Solid, ElementId>();
    List<ElementId> source = new List<ElementId>()
    {
      flatElement.Id
    };
    ElementId elementId = source.First<ElementId>();
    Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary = new Dictionary<Solid, Tuple<string, double, bool>>();
    List<double> zIndexMapping = new List<double>();
    double slabWeight1 = 0.0;
    if (PartUtils.AreElementsValidForCreateParts(revitDoc, (ICollection<ElementId>) source))
    {
      PartUtils.CreateParts(revitDoc, (ICollection<ElementId>) source);
      revitDoc.Regenerate();
      if (PartUtils.HasAssociatedParts(revitDoc, elementId))
      {
        foreach (ElementId associatedPart in (IEnumerable<ElementId>) PartUtils.GetAssociatedParts(revitDoc, elementId, false, false))
        {
          Element element = revitDoc.GetElement(associatedPart);
          ICollection<ElementId> materialIds = element.GetMaterialIds(false);
          List<Solid> instanceSolids = Solids.GetInstanceSolids(element);
          if (materialIds.ElementAt<ElementId>(0).IntegerValue != -1)
            dictionary.Add(instanceSolids.ElementAt<Solid>(0), materialIds.ElementAt<ElementId>(0));
        }
      }
    }
    else
    {
      Element element = revitDoc.GetElement(elementId);
      ICollection<ElementId> materialIds = element.GetMaterialIds(false);
      foreach (Solid instanceSolid in Solids.GetInstanceSolids(element))
        dictionary.Add(instanceSolid, materialIds.FirstOrDefault<ElementId>());
    }
    double num1 = -1.0;
    if (flatElement.LookupParameter("Type").Element.Category.Name == "Structural Framing")
    {
      List<Solid> instanceSolids1 = Solids.GetInstanceSolids(flatElement);
      foreach (ElementId id in list1)
      {
        List<Solid> instanceSolids2 = Solids.GetInstanceSolids(revitDoc.GetElement(id));
        instanceSolids1.AddRange((IEnumerable<Solid>) instanceSolids2);
        dictionary.Add(instanceSolids2.ElementAt<Solid>(0), revitDoc.GetElement(id).GetMaterialIds(false).ElementAt<ElementId>(0));
      }
      int num2 = assembly.LookupParameter("UNIT_WEIGHT") != null ? 1 : 0;
      double num3 = num2 == 0 ? Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) assembly, "WEIGHT_PER_UNIT") : Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) assembly, "UNIT_WEIGHT");
      double slabWeight2 = num2 == 0 ? UnitUtils.Convert(num3, UnitTypeId.PoundsMassPerCubicFoot, UnitTypeId.KilogramsPerCubicMeter) : UnitUtils.Convert(UnitUtils.ConvertFromInternalUnits(num3, UnitTypeId.PoundsForcePerCubicFoot), UnitTypeId.PoundsMassPerCubicFoot, UnitTypeId.KilogramsPerCubicMeter);
      Solid solid1 = (Solid) null;
      foreach (Solid solid2 in instanceSolids1)
      {
        try
        {
          if ((GeometryObject) solid1 == (GeometryObject) null)
            solid1 = SolidUtils.Clone(solid2);
          else
            BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid1, solid2, BooleanOperationsType.Union);
        }
        catch (Exception ex)
        {
          errors.Add(EDGECAM.errorCondition.InvalidGeometry);
          return false;
        }
      }
      int num4 = 0;
      for (int index = 0; index < instanceSolids1.Count; ++index)
      {
        int count1 = LayerDictionary.Count;
        Solid solid3 = instanceSolids1[index];
        ElementId materialElementId = solid3.Faces.get_Item(0).MaterialElementId;
        Element element = revitDoc.GetElement(materialElementId);
        List<Tuple<CurveLoop, XYZ, bool>> outerCurves = new List<Tuple<CurveLoop, XYZ, bool>>();
        List<Tuple<CurveLoop, XYZ>> upwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
        List<Tuple<CurveLoop, XYZ>> downwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
        double minZ1 = double.MaxValue;
        double maxZ1 = double.MinValue;
        CAM_Utils.GetCurveLoops(solid1, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ1, out minZ1, dataFactory.transform, slopeTolerance);
        double[] minMaxZ = CAM_Utils.GetMinMaxZ(solid3, dataFactory.transform, slopeTolerance);
        double minZ2 = minMaxZ[0];
        double maxZ2 = minMaxZ[1];
        double height = Math.Abs(maxZ2 - minZ2);
        if (Utils.ElementUtils.Parameters.GetParameterAsBool(elem1, "CAM_FLANGE_ONLY") && Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "CONSTRUCTION_PRODUCT").Contains("DOUBLE TEE"))
        {
          double parameterAsDouble1 = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem1, "DT_Flange_Thickness_Form");
          double parameterAsDouble2 = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem1, "Additional_Flange_Thickness");
          double num5 = parameterAsDouble2;
          double num6 = parameterAsDouble1 + num5;
          if (parameterAsDouble2 != -1.0 && num6 > 0.0)
          {
            num1 = height - num6;
            height = num6;
          }
        }
        string name = element == null ? "" : element.Name;
        double num7 = UnitUtils.ConvertFromInternalUnits(solid3.Volume, UnitTypeId.CubicMeters);
        slabWeight1 += slabWeight2 * num7;
        bool overallSplineError = false;
        if (!CAM_Utils.GetLayerDictionary(LayerDictionary, outerCurves, upwardFacingCurves, downwardFacingCurves, revitDoc, name, slabWeight2, minZ2, maxZ2, height, out overallSplineError, dataFactory.transform))
        {
          errors.Add(EDGECAM.errorCondition.InvalidGeometry);
          return false;
        }
        int count2 = LayerDictionary.Count;
        if (LayerDictionary.Count - count1 > 1)
        {
          for (; count2 > count1 + 1; count2 = LayerDictionary.Count)
            LayerDictionary.Remove(LayerDictionary.Keys.Last<Solid>());
        }
        zIndexMapping.Add(minZ2);
        num4 = LayerDictionary.Count;
        if (overallSplineError && !errors.Contains(EDGECAM.errorCondition.Splines))
          errors.Add(EDGECAM.errorCondition.Splines);
      }
    }
    if (slabWeight1 < 0.0)
    {
      errors.Add(EDGECAM.errorCondition.NegativeWeight);
      return false;
    }
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    List<Element> list2 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("REBAR"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID"))).Where<Element>((Func<Element, bool>) (e => !e.Name.ToUpper().Contains("CENTROID"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("MESH"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("WWF"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SHEARGRID"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SHEAR GRID"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("CGRID"))).ToList<Element>();
    if (structuralFramingElement is FamilyInstance)
    {
      foreach (ElementId subComponentId in (IEnumerable<ElementId>) (structuralFramingElement as FamilyInstance).GetSubComponentIds())
      {
        Element element = revitDoc.GetElement(subComponentId);
        if (Utils.ElementUtils.Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("INSULATION") && !list2.Contains(element))
          list2.Add(element);
      }
    }
    List<Element> list3 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SPIRAL"))).ToList<Element>();
    list2.AddRange((IEnumerable<Element>) list3);
    ItMountparts mountparts = new ItMountparts();
    List<Element> meshList = new List<Element>();
    List<Element> meshFromElements = CAM_Reinforcement_Utils.GetMeshFromElements(elementIdList2, revitDoc);
    bool flag = false;
    foreach (Element elem2 in meshFromElements)
    {
      foreach (Solid instanceSolid in Solids.GetInstanceSolids(elem2, useViewDetail: true))
      {
        if (!Util.IsZero(instanceSolid.Volume, 1E-06))
        {
          flag = true;
          break;
        }
      }
      if (flag)
        list2.Add(elem2);
      else
        meshList.Add(elem2);
    }
    foreach (Element element in list2)
    {
      dataFactory.CreateUXMLMountPartRaw(element);
      if (!Utils.ElementUtils.Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))
      {
        BoundingBoxXYZ transformedBboxFromElement = CAM_Utils.CreateTransformedBBoxFromElement(element, dataFactory.transform);
        ItMountpart uxmlMountPartRaw = dataFactory.CreateUXMLMountPartRaw(element, transformedBboxFromElement);
        if (uxmlMountPartRaw.AREA < 0.001)
          uxmlMountPartRaw.AREA = 0.0;
        if (uxmlMountPartRaw != null && uxmlMountPartRaw.COORDS.Z < 0.0)
        {
          errors.Add(EDGECAM.errorCondition.ElementsBelowPallet);
          return false;
        }
        mountparts.MOUNTPART.Add(uxmlMountPartRaw);
      }
    }
    List<BoundingBoxXYZ> meshBoundingBox = CAM_Reinforcement_Utils.GetMeshBoundingBox(meshList, revitDoc, dataFactory.transform);
    for (int index = 0; index < meshList.Count; ++index)
    {
      Element mountpart = meshList[index];
      BoundingBoxXYZ bb = meshBoundingBox[index];
      ItMountpart uxmlMountPartRaw = dataFactory.CreateUXMLMountPartRaw(mountpart, bb);
      if (uxmlMountPartRaw != null && uxmlMountPartRaw.COORDS.Z < 0.0)
      {
        errors.Add(EDGECAM.errorCondition.ElementsBelowPallet);
        return false;
      }
      mountparts.MOUNTPART.Add(uxmlMountPartRaw);
    }
    List<Element> list4 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("REBAR"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SPIRAL"))).ToList<Element>();
    List<Tuple<double, List<Curve>>> barsGeometry = CAM_Reinforcement_Utils.getBarsGeometry(revitDoc, list4, problemBars);
    if (problemBars.Count > 0)
    {
      errors.Add(EDGECAM.errorCondition.ProblemBars);
      return false;
    }
    BoundingBoxXYZ boundingBoxXyz = (BoundingBoxXYZ) null;
    foreach (Element element in list4)
    {
      BoundingBoxXYZ transformedBoundingBox2 = Util.getTransformedBoundingBox(dataFactory.transform, element, true);
      boundingBoxXyz = boundingBoxXyz != null ? Utils.MiscUtils.MiscUtils.AdjustBBox(boundingBoxXyz, transformedBoundingBox2) : transformedBoundingBox2;
    }
    if (boundingBoxXyz == null)
    {
      boundingBoxXyz = new BoundingBoxXYZ();
      boundingBoxXyz.Min = XYZ.Zero;
      boundingBoxXyz.Max = XYZ.Zero;
    }
    ItRodstocks rodstocks = new ItRodstocks();
    foreach (Tuple<double, List<Curve>> tuple in barsGeometry)
    {
      bool bMountpart;
      ItRodstock uxmlRodstockRaw = dataFactory.CreateUXMLRodstockRaw((IList<Curve>) tuple.Item2, 0.0, tuple.Item1, "GR60", out bMountpart);
      if (uxmlRodstockRaw != null && uxmlRodstockRaw.COORDS.Z < 0.0)
      {
        errors.Add(EDGECAM.errorCondition.ElementsBelowPallet);
        return false;
      }
      if (!bMountpart)
      {
        rodstocks.RODSTOCK.Add(uxmlRodstockRaw);
      }
      else
      {
        ItMountpart uxmlMountPartRaw = dataFactory.CreateUXMLMountPartRaw(list4[barsGeometry.IndexOf(tuple)]);
        if (uxmlMountPartRaw != null && uxmlMountPartRaw.COORDS.Z < 0.0)
        {
          errors.Add(EDGECAM.errorCondition.ElementsBelowPallet);
          return false;
        }
        mountparts.MOUNTPART.Add(uxmlMountPartRaw);
      }
    }
    string productType = "06";
    string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "CONSTRUCTION_PRODUCT");
    if (parameterAsString1.ToUpper().Contains("COLUMN") || parameterAsString1.ToUpper().Contains("COLUMN") || parameterAsString1.ToUpper().Contains("WALL"))
      productType = "09";
    else if (parameterAsString1.ToUpper().Contains("HOLLOW"))
      productType = "10";
    string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "Work Plane");
    string str = parameterAsString2;
    string[] separator = new string[1]{ "LEVEL" };
    foreach (string s in str.Split(separator, StringSplitOptions.RemoveEmptyEntries))
    {
      int num8 = -1;
      ref int local = ref num8;
      if (int.TryParse(s, out local))
        parameterAsString2 = num8.ToString();
    }
    string prodNo = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assembly, "CAM_PRODNO");
    string orderNo = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assembly, "CAM_ORDERNO");
    if (prodNo.Replace(" ", string.Empty) == "")
    {
      prodNo = "ProdNo";
      errors.Add(EDGECAM.errorCondition.NoProdNo);
    }
    if (orderNo.Replace(" ", string.Empty) == "")
    {
      orderNo = "OrderNo";
      errors.Add(EDGECAM.errorCondition.NoOrderNo);
    }
    if (Utils.ElementUtils.Parameters.GetParameterAsBool(elem1, "CAM_FLANGE_ONLY") && Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "CONSTRUCTION_PRODUCT").Contains("DOUBLE TEE"))
    {
      double a = UnitUtils.ConvertFromInternalUnits(num1, UnitTypeId.Millimeters);
      transformedBoundingBox1.Max = new XYZ(transformedBoundingBox1.Max.X, transformedBoundingBox1.Max.Y, transformedBoundingBox1.Max.Z - num1);
      foreach (ItMountpart itMountpart in mountparts.MOUNTPART.ToList<ItMountpart>())
      {
        itMountpart.COORDS.Z -= Math.Round(a);
        mountparts.MOUNTPART.Remove(itMountpart);
        if (itMountpart.COORDS.Z >= 0.0)
          mountparts.MOUNTPART.Add(itMountpart);
      }
      foreach (ItRodstock itRodstock in rodstocks.RODSTOCK.ToList<ItRodstock>())
      {
        itRodstock.COORDS.Z -= Math.Round(a);
        rodstocks.RODSTOCK.Remove(itRodstock);
        if (itRodstock.COORDS.Z >= 0.0)
          rodstocks.RODSTOCK.Add(itRodstock);
      }
    }
    ItContours itContours = new ItContours();
    ItCutouts itCutouts = new ItCutouts();
    List<ItLayer> layers = new List<ItLayer>();
    dataFactory.CreateUXMLContoursCutoutsRaw(LayerDictionary, itContours, itCutouts, layers);
    List<ItLayer> list5 = layers.OrderBy<ItLayer, double>((Func<ItLayer, double>) (layer => zIndexMapping[layers.IndexOf(layer)])).ToList<ItLayer>();
    ItSlabdatePart uxmlSlabdatePartRaw = dataFactory.CreateUXMLSlabdatePartRaw(productType, slabWeight1, transformedBoundingBox1, boundingBoxXyz, itContours, itCutouts, mountparts, rodstocks, new ItReinforcement(), list5);
    ItSlabDate uxmlSlabDateRaw = dataFactory.CreateUXMLSlabDateRaw(prodNo, parameterAsString2, uxmlSlabdatePartRaw);
    ItOrder uxmlOrderDataRaw = dataFactory.CreateUXMLOrderDataRaw(orderNo, uxmlSlabDateRaw);
    dataFactory.ORDER = uxmlOrderDataRaw;
    dataFactory.DOCUMENT = new ItDocument(exportType);
    bool UXML52Negative = false;
    bool UXMLTooManyDigits = false;
    if (!CNCExport_Command.SaveUnitechnikRaw(assembly, dataFactory, exportType, out UXML52Negative, out UXMLTooManyDigits, "C:\\EDGEforREVIT"))
      throw new Exception("Failed to Export to UXML");
    if (UXML52Negative)
    {
      errors.Add(EDGECAM.errorCondition.UXML52Negative);
      return false;
    }
    if (UXMLTooManyDigits)
      errors.Add(EDGECAM.errorCondition.TooManyDigits);
    errors.Add(EDGECAM.errorCondition.Success);
    return true;
  }

  public bool SavePXML(
    AssemblyInstance assembly,
    Document revitDoc,
    double slopeTolerance,
    out List<EDGECAM.errorCondition> errors,
    List<Element> problemBars,
    List<Element> problemMesh,
    List<ElementId> elementsInAssemblyView = null)
  {
    errors = new List<EDGECAM.errorCondition>();
    List<ElementId> elementIdList1 = new List<ElementId>();
    List<ElementId> elementIdList2 = elementsInAssemblyView != null ? elementsInAssemblyView : assembly.GetMemberIds().ToList<ElementId>();
    List<ElementId> list1 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).OfCategory(BuiltInCategory.OST_Walls).ToElementIds().ToList<ElementId>();
    DataFactory dataFactory = new DataFactory(assembly);
    if (elementIdList2.Select<ElementId, Element>(new Func<ElementId, Element>(revitDoc.GetElement)).Where<Element>((Func<Element, bool>) (s => s.Category.Id.IntegerValue == -2001320)).Count<Element>() > 1)
    {
      errors.Add(EDGECAM.errorCondition.TooManyStructuralFraming);
      return false;
    }
    Element structuralFramingElement = assembly.GetStructuralFramingElement();
    if (structuralFramingElement == null || structuralFramingElement.Category.Name != "Structural Framing")
    {
      errors.Add(EDGECAM.errorCondition.NoStructuralFraming);
      return false;
    }
    Element flatElement = (Element) AssemblyInstances.GetFlatElement(revitDoc, structuralFramingElement as FamilyInstance);
    Element elem1 = (structuralFramingElement as FamilyInstance).SuperComponent == null ? structuralFramingElement : (flatElement as FamilyInstance).SuperComponent;
    BoundingBoxXYZ transformedBoundingBox1 = Util.getTransformedBoundingBox(dataFactory.transform, flatElement);
    StructuralMaterialTypeFilter materialTypeFilter = new StructuralMaterialTypeFilter(StructuralMaterialType.PrecastConcrete);
    if (!(structuralFramingElement is FamilyInstance) || !materialTypeFilter.PassesFilter(structuralFramingElement))
    {
      errors.Add(EDGECAM.errorCondition.InvalidStructuralFraming);
      return false;
    }
    Dictionary<Solid, ElementId> dictionary = new Dictionary<Solid, ElementId>();
    List<ElementId> source1 = new List<ElementId>()
    {
      flatElement.Id
    };
    ElementId elementId = source1.First<ElementId>();
    if (PartUtils.AreElementsValidForCreateParts(revitDoc, (ICollection<ElementId>) source1))
    {
      PartUtils.CreateParts(revitDoc, (ICollection<ElementId>) source1);
      revitDoc.Regenerate();
      if (PartUtils.HasAssociatedParts(revitDoc, elementId))
      {
        foreach (ElementId associatedPart in (IEnumerable<ElementId>) PartUtils.GetAssociatedParts(revitDoc, elementId, false, false))
        {
          Element element = revitDoc.GetElement(associatedPart);
          ICollection<ElementId> materialIds = element.GetMaterialIds(false);
          List<Solid> instanceSolids = Solids.GetInstanceSolids(element);
          if (materialIds.ElementAt<ElementId>(0).IntegerValue != -1)
            dictionary.Add(instanceSolids.ElementAt<Solid>(0), materialIds.ElementAt<ElementId>(0));
        }
      }
    }
    else
    {
      Element element = revitDoc.GetElement(elementId);
      ICollection<ElementId> materialIds = element.GetMaterialIds(false);
      foreach (Solid instanceSolid in Solids.GetInstanceSolids(element))
        dictionary.Add(instanceSolid, materialIds.FirstOrDefault<ElementId>());
    }
    if (flatElement.LookupParameter("Type").Element.Category.Name == "Structural Framing")
    {
      List<Solid> instanceSolids1 = Solids.GetInstanceSolids(flatElement);
      foreach (ElementId id in list1)
      {
        List<Solid> instanceSolids2 = Solids.GetInstanceSolids(revitDoc.GetElement(id));
        instanceSolids1.AddRange((IEnumerable<Solid>) instanceSolids2);
        dictionary.Add(instanceSolids2.ElementAt<Solid>(0), revitDoc.GetElement(id).GetMaterialIds(false).ElementAt<ElementId>(0));
      }
      Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary = new Dictionary<Solid, Tuple<string, double, bool>>();
      int num1 = Utils.ElementUtils.Parameters.LookupParameter((Element) assembly, "UNIT_WEIGHT") != null ? 1 : 0;
      double num2 = num1 == 0 ? Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) assembly, "WEIGHT_PER_UNIT") : Utils.ElementUtils.Parameters.GetParameterAsDouble((Element) assembly, "UNIT_WEIGHT");
      double slabWeight1 = num1 == 0 ? UnitUtils.Convert(num2, UnitTypeId.PoundsMassPerCubicFoot, UnitTypeId.KilogramsPerCubicMeter) : UnitUtils.Convert(UnitUtils.ConvertFromInternalUnits(num2, UnitTypeId.PoundsForcePerCubicFoot), UnitTypeId.PoundsMassPerCubicFoot, UnitTypeId.KilogramsPerCubicMeter);
      double num3 = 0.0;
      double num4 = -1.0;
      for (int index = 0; index < instanceSolids1.Count; ++index)
      {
        Solid solid = instanceSolids1[index];
        ElementId materialElementId = solid.Faces.get_Item(0).MaterialElementId;
        Element element = revitDoc.GetElement(materialElementId);
        List<Tuple<CurveLoop, XYZ, bool>> outerCurves = new List<Tuple<CurveLoop, XYZ, bool>>();
        List<Tuple<CurveLoop, XYZ>> upwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
        List<Tuple<CurveLoop, XYZ>> downwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
        double minZ = double.MaxValue;
        double maxZ = double.MinValue;
        CAM_Utils.GetCurveLoops(solid, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ, out minZ, dataFactory.transform, slopeTolerance);
        double height = Math.Abs(maxZ - minZ);
        if (Utils.ElementUtils.Parameters.GetParameterAsBool(elem1, "CAM_FLANGE_ONLY") && Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "CONSTRUCTION_PRODUCT").Contains("DOUBLE TEE"))
        {
          double parameterAsDouble1 = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem1, "DT_Flange_Thickness_Form");
          double parameterAsDouble2 = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem1, "Additional_Flange_Thickness");
          double num5 = parameterAsDouble2;
          double num6 = parameterAsDouble1 + num5;
          if (parameterAsDouble2 != -1.0 && num6 > 0.0)
          {
            num4 = height - num6;
            height = num6;
          }
        }
        string name = element == null ? "" : element.Name;
        double num7 = UnitUtils.ConvertFromInternalUnits(solid.Volume, UnitTypeId.CubicMeters);
        num3 += slabWeight1 * num7;
        bool overallSplineError = false;
        if (!CAM_Utils.GetLayerDictionary(LayerDictionary, outerCurves, upwardFacingCurves, downwardFacingCurves, revitDoc, name, slabWeight1, minZ, maxZ, height, out overallSplineError, dataFactory.transform))
        {
          errors.Add(EDGECAM.errorCondition.InvalidGeometry);
          return false;
        }
        if (overallSplineError && !errors.Contains(EDGECAM.errorCondition.Splines))
          errors.Add(EDGECAM.errorCondition.Splines);
      }
      double slabWeight2 = 0.0;
      List<CNCOutline> pxmlLotOutlinesRaw = dataFactory.CreatePXMLLotOutlinesRaw(flatElement.Name, out slabWeight2, LayerDictionary);
      if (slabWeight2 < 0.0)
      {
        errors.Add(EDGECAM.errorCondition.NegativeWeight);
        return false;
      }
      ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
      {
        BuiltInCategory.OST_GenericModel,
        BuiltInCategory.OST_SpecialityEquipment
      });
      List<Element> list2 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("REBAR"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID"))).Where<Element>((Func<Element, bool>) (e => !e.Name.ToUpper().Contains("CENTROID"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("MESH"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("WWF"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SHEARGRID"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SHEAR GRID"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("CGRID"))).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MESH_SHEET") == "")).ToList<Element>();
      if (structuralFramingElement is FamilyInstance)
      {
        foreach (ElementId subComponentId in (IEnumerable<ElementId>) (structuralFramingElement as FamilyInstance).GetSubComponentIds())
        {
          Element element = revitDoc.GetElement(subComponentId);
          if (Utils.ElementUtils.Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("INSULATION") && !list2.Contains(element))
            list2.Add(element);
        }
      }
      List<Element> list3 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).WherePasses((ElementFilter) filter).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SPIRAL"))).ToList<Element>();
      list2.AddRange((IEnumerable<Element>) list3);
      List<Element> meshList = new List<Element>();
      List<Element> meshFromElements = CAM_Reinforcement_Utils.GetMeshFromElements(elementIdList2, revitDoc);
      bool flag = false;
      foreach (Element elem2 in meshFromElements)
      {
        foreach (Solid instanceSolid in Solids.GetInstanceSolids(elem2, useViewDetail: true))
        {
          if (!Util.IsZero(instanceSolid.Volume, 1E-06))
          {
            flag = true;
            break;
          }
        }
        if (flag)
          list2.Add(elem2);
        else
          meshList.Add(elem2);
      }
      foreach (Element element in list2)
      {
        if (!Utils.ElementUtils.Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("RAW CONSUMABLE"))
        {
          BoundingBoxXYZ transformedBboxFromElement = CAM_Utils.CreateTransformedBBoxFromElement(element, dataFactory.transform);
          CNCOutline pxmlMountPartRaw = dataFactory.CreatePXMLMountPartRaw(element, transformedBboxFromElement);
          pxmlLotOutlinesRaw.Add(pxmlMountPartRaw);
        }
      }
      List<BoundingBoxXYZ> meshBoundingBox = CAM_Reinforcement_Utils.GetMeshBoundingBox(meshList, revitDoc, dataFactory.transform);
      for (int index = 0; index < meshList.Count; ++index)
      {
        Element mountpart = meshList[index];
        BoundingBoxXYZ bb = meshBoundingBox[index];
        CNCOutline pxmlMountPartRaw = dataFactory.CreatePXMLMountPartRaw(mountpart, bb);
        pxmlLotOutlinesRaw.Add(pxmlMountPartRaw);
      }
      List<EDGECAM_Steel> edgecamSteelList = new List<EDGECAM_Steel>();
      List<CNCSteel> rebar = new List<CNCSteel>();
      List<Element> list4 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("REBAR"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW CONSUMABLE"))).Where<Element>((Func<Element, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsString(e, "MANUFACTURE_COMPONENT").ToUpper().Contains("SPIRAL"))).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MESH_SHEET") == "")).ToList<Element>();
      List<IGrouping<string, Element>> list5 = new FilteredElementCollector(revitDoc, (ICollection<ElementId>) elementIdList2).OfCategory(BuiltInCategory.OST_SpecialityEquipment).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "MESH_SHEET") != "")).ToList<Element>().GroupBy<Element, string>((Func<Element, string>) (elem => Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MESH_SHEET"))).ToList<IGrouping<string, Element>>();
      BoundingBoxXYZ boundingBoxXyz = (BoundingBoxXYZ) null;
      if (list4 != null)
      {
        foreach (Element element in list4)
        {
          EDGECAM_Steel edgecamSteel = new EDGECAM_Steel();
          bool problematic = false;
          edgecamSteel.Bars = new List<Tuple<double, List<Curve>>>()
          {
            CAM_Reinforcement_Utils.getBarGeometry(revitDoc, element, out problematic)
          };
          if (problematic)
          {
            problemBars.Add(element);
          }
          else
          {
            BoundingBoxXYZ transformedBoundingBox2 = Util.getTransformedBoundingBox(dataFactory.transform, element, true);
            edgecamSteel.Bbox = transformedBoundingBox2;
            boundingBoxXyz = boundingBoxXyz != null ? Utils.MiscUtils.MiscUtils.AdjustBBox(boundingBoxXyz, transformedBoundingBox2) : transformedBoundingBox2;
            string str = Utils.ElementUtils.Parameters.GetParameterAsString(element, "CONTROL_MARK");
            if (str == "")
              str = element.Name;
            edgecamSteel.Name = str;
            double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "BAR_BEND_RADIUS");
            edgecamSteel.BendRadius = parameterAsDouble;
            edgecamSteel.bMesh = false;
            edgecamSteelList.Add(edgecamSteel);
          }
        }
      }
      foreach (IGrouping<string, Element> source2 in list5)
      {
        if (!(source2.Key == ""))
        {
          EDGECAM_Steel edgecamSteel = new EDGECAM_Steel();
          List<Element> elementList = new List<Element>();
          edgecamSteel.Bars = CAM_Reinforcement_Utils.getBarsGeometry(revitDoc, source2.ToList<Element>(), elementList);
          if (elementList.Count > 0)
          {
            problemMesh.AddRange((IEnumerable<Element>) elementList);
          }
          else
          {
            foreach (Element element in (IEnumerable<Element>) source2)
            {
              BoundingBoxXYZ transformedBoundingBox3 = Util.getTransformedBoundingBox(dataFactory.transform, element, true);
              edgecamSteel.Bbox = edgecamSteel.Bbox != null ? Utils.MiscUtils.MiscUtils.AdjustBBox(boundingBoxXyz, transformedBoundingBox3) : transformedBoundingBox3;
              boundingBoxXyz = boundingBoxXyz != null ? Utils.MiscUtils.MiscUtils.AdjustBBox(boundingBoxXyz, transformedBoundingBox3) : transformedBoundingBox3;
            }
            edgecamSteel.Name = source2.Key;
            edgecamSteel.BendRadius = Utils.ElementUtils.Parameters.GetParameterAsDouble(source2.FirstOrDefault<Element>(), "BAR_BEND_RADIUS");
            edgecamSteel.bMesh = true;
            edgecamSteelList.Add(edgecamSteel);
          }
        }
      }
      if (problemBars.Count > 0)
      {
        errors.Add(EDGECAM.errorCondition.ProblemBars);
        return false;
      }
      if (problemMesh.Count > 0)
      {
        errors.Add(EDGECAM.errorCondition.ProblemMesh);
        return false;
      }
      if (boundingBoxXyz == null)
      {
        boundingBoxXyz = new BoundingBoxXYZ();
        boundingBoxXyz.Min = XYZ.Zero;
        boundingBoxXyz.Max = XYZ.Zero;
      }
      foreach (EDGECAM_Steel edgecamSteel in edgecamSteelList)
      {
        CNCSteel pxmlMeshRaw = dataFactory.CreatePXMLMeshRaw(edgecamSteel.Bars, edgecamSteel.BendRadius, edgecamSteel.Name, edgecamSteel.bMesh);
        rebar.Add(pxmlMeshRaw);
      }
      if (Utils.ElementUtils.Parameters.GetParameterAsBool(elem1, "CAM_FLANGE_ONLY") && Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "CONSTRUCTION_PRODUCT").Contains("DOUBLE TEE"))
      {
        double num8 = UnitUtils.ConvertFromInternalUnits(num4, UnitTypeId.Millimeters);
        transformedBoundingBox1.Max = new XYZ(transformedBoundingBox1.Max.X, transformedBoundingBox1.Max.Y, transformedBoundingBox1.Max.Z - num4);
        foreach (CNCOutline cncOutline in pxmlLotOutlinesRaw.ToList<CNCOutline>())
        {
          if (cncOutline.MountPartTypeSpecified)
          {
            cncOutline.Z -= num8;
            pxmlLotOutlinesRaw.Remove(cncOutline);
            if (cncOutline.Z >= 0.0)
              pxmlLotOutlinesRaw.Add(cncOutline);
          }
        }
        foreach (CNCSteel cncSteel in rebar)
        {
          foreach (CNCBar cncBar in cncSteel.barList.ToList<CNCBar>())
          {
            cncBar.Z -= num8;
            cncSteel.barList.Remove(cncBar);
            if (cncBar.Z >= 0.0)
              cncSteel.barList.Add(cncBar);
          }
        }
      }
      CNCProduct cncProduct = new CNCProduct(dataFactory.CreatePXMLCNCSlabDataRaw(pxmlLotOutlinesRaw, rebar, transformedBoundingBox1, boundingBoxXyz, num3 / 1000.0));
      string CONTROL_MARK = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "CONTROL_MARK");
      List<Element> list6 = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).Where<Element>((Func<Element, bool>) (e => Utils.ElementUtils.Parameters.GetParameterAsString(e, "CONTROL_MARK").Equals(CONTROL_MARK))).ToList<Element>();
      string productType = "06";
      string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "CONSTRUCTION_PRODUCT");
      if (parameterAsString1.ToUpper().Contains("COLUMN") || parameterAsString1.ToUpper().Contains("COLUMN") || parameterAsString1.ToUpper().Contains("WALL"))
        productType = "09";
      else if (parameterAsString1.ToUpper().Contains("HOLLOW"))
        productType = "10";
      string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString(structuralFramingElement, "Work Plane");
      string str1 = parameterAsString2;
      string[] separator = new string[1]{ "LEVEL" };
      foreach (string s in str1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
      {
        int num9 = -1;
        ref int local = ref num9;
        if (int.TryParse(s, out local))
          parameterAsString2 = num9.ToString();
      }
      string prodNo = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assembly, "CAM_PRODNO");
      string OrderNo = Utils.ElementUtils.Parameters.GetParameterAsString((Element) assembly, "CAM_ORDERNO");
      if (prodNo.Replace(" ", string.Empty) == "")
      {
        prodNo = "ProdNo";
        errors.Add(EDGECAM.errorCondition.NoProdNo);
      }
      if (OrderNo.Replace(" ", string.Empty) == "")
      {
        OrderNo = "OrderNo";
        errors.Add(EDGECAM.errorCondition.NoOrderNo);
      }
      dataFactory.CreatePXMLCNCProductRaw(cncProduct, prodNo, productType, list6.Count);
      CNCOrder orderData = new CNCOrder(cncProduct);
      dataFactory.Order = dataFactory.CreatePXMLOrderRaw(orderData, OrderNo, parameterAsString2, "", (string[]) null);
      CNCDocInfo cncDocInfo = new CNCDocInfo("1", "3");
      dataFactory.documentInfo = cncDocInfo;
      CNCExport_Command.SavePXMLRaw(assembly, dataFactory, "C:\\EDGEforRevit");
      if (true)
      {
        string path = $"C:\\EDGEforRevit\\PXML V1.3\\{assembly.Name + ".pxml"}";
        string str2 = "<PXML_Document xmlns=\"http://progress-m.com/ProgressXML/Version1\">";
        string[] contents = File.ReadAllLines(path);
        for (int index = 0; index < contents.Length; ++index)
        {
          if (contents[index].Contains("PXML_Document"))
          {
            contents[index] = str2;
            break;
          }
        }
        File.WriteAllLines(path, contents);
      }
    }
    errors.Add(EDGECAM.errorCondition.Success);
    return true;
  }

  public void ExportRaw(
    List<AssemblyInstance> assemblyToProcess,
    Document revitDoc,
    double slopeTolerance,
    List<ElementId> elementsInAssemblyView = null)
  {
    TaskDialog taskDialog1 = new TaskDialog("EDGE^CAM Export");
    taskDialog1.TitleAutoPrefix = false;
    taskDialog1.MainInstruction = "Choose export method";
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "PXML");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Unitechnik");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "All");
    taskDialog1.DefaultButton = (TaskDialogResult) 1003;
    TaskDialogResult taskDialogResult1 = taskDialog1.Show();
    if (taskDialogResult1 == 2)
      return;
    int num1 = taskDialogResult1 != 1001 ? (taskDialogResult1 != 1002 ? 2 : 1) : 0;
    SubTransaction subTransaction = new SubTransaction(revitDoc);
    List<AssemblyInstance> assemblyInstanceList1 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList2 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList3 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList4 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList5 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList6 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList7 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList8 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList9 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList10 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList11 = new List<AssemblyInstance>();
    List<Element> source1 = new List<Element>();
    List<Element> source2 = new List<Element>();
    List<AssemblyInstance> assemblyInstanceList12 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList13 = new List<AssemblyInstance>();
    List<AssemblyInstance> assemblyInstanceList14 = new List<AssemblyInstance>();
    switch (num1)
    {
      case 0:
        using (List<AssemblyInstance>.Enumerator enumerator = assemblyToProcess.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            AssemblyInstance current = enumerator.Current;
            int num2 = (int) subTransaction.Start();
            List<Element> elementList1 = new List<Element>();
            List<Element> elementList2 = new List<Element>();
            List<EDGECAM.errorCondition> errors;
            this.SavePXML(current, revitDoc, slopeTolerance, out errors, elementList1, elementList2, elementsInAssemblyView);
            foreach (int num3 in errors)
            {
              switch (num3)
              {
                case 0:
                  assemblyInstanceList12.Add(current);
                  continue;
                case 1:
                  assemblyInstanceList3.Add(current);
                  continue;
                case 2:
                  assemblyInstanceList4.Add(current);
                  continue;
                case 3:
                  assemblyInstanceList5.Add(current);
                  continue;
                case 4:
                  assemblyInstanceList9.Add(current);
                  continue;
                case 7:
                  assemblyInstanceList2.Add(current);
                  continue;
                case 8:
                  assemblyInstanceList7.Add(current);
                  source1.AddRange((IEnumerable<Element>) elementList1);
                  source1 = source1.Distinct<Element>().ToList<Element>();
                  continue;
                case 9:
                  assemblyInstanceList8.Add(current);
                  source2.AddRange((IEnumerable<Element>) elementList2);
                  source2 = source2.Distinct<Element>().ToList<Element>();
                  continue;
                case 10:
                  assemblyInstanceList6.Add(current);
                  continue;
                case 11:
                  assemblyInstanceList11.Add(current);
                  continue;
                case 12:
                  assemblyInstanceList10.Add(current);
                  continue;
                case 14:
                  assemblyInstanceList1.Add(current);
                  continue;
                default:
                  continue;
              }
            }
            int num4 = (int) subTransaction.RollBack();
          }
          break;
        }
      case 1:
        TaskDialog taskDialog2 = new TaskDialog("EDGE^CAM Export");
        taskDialog2.TitleAutoPrefix = false;
        taskDialog2.MainInstruction = "Choose export method:";
        taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1001, "Unitechnik 5.2.c");
        taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1002, "Unitechnik 6.0.0");
        taskDialog2.AddCommandLink((TaskDialogCommandLinkId) 1003, "Unitechnik 7.0.0");
        TaskDialogResult taskDialogResult2 = taskDialog2.Show();
        if (taskDialogResult2 != 2 && taskDialogResult2 != 8)
        {
          string exportType = taskDialogResult2 != 1001 ? (taskDialogResult2 != 1002 ? "700" : "600") : "502";
          using (List<AssemblyInstance>.Enumerator enumerator = assemblyToProcess.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              AssemblyInstance current = enumerator.Current;
              int num5 = (int) subTransaction.Start();
              List<Element> elementList = new List<Element>();
              List<EDGECAM.errorCondition> errors = new List<EDGECAM.errorCondition>();
              this.SaveUnitechnik(current, revitDoc, slopeTolerance, out errors, elementList, exportType, elementsInAssemblyView);
              foreach (int num6 in errors)
              {
                switch (num6)
                {
                  case 0:
                    assemblyInstanceList12.Add(current);
                    continue;
                  case 1:
                    assemblyInstanceList3.Add(current);
                    continue;
                  case 2:
                    assemblyInstanceList4.Add(current);
                    continue;
                  case 3:
                    assemblyInstanceList5.Add(current);
                    continue;
                  case 4:
                    assemblyInstanceList9.Add(current);
                    continue;
                  case 5:
                    assemblyInstanceList14.Add(current);
                    continue;
                  case 6:
                    assemblyInstanceList13.Add(current);
                    continue;
                  case 7:
                    assemblyInstanceList2.Add(current);
                    continue;
                  case 8:
                    assemblyInstanceList7.Add(current);
                    source1.AddRange((IEnumerable<Element>) elementList);
                    source1 = source1.Distinct<Element>().ToList<Element>();
                    continue;
                  case 10:
                    assemblyInstanceList6.Add(current);
                    continue;
                  case 11:
                    assemblyInstanceList11.Add(current);
                    continue;
                  case 12:
                    assemblyInstanceList10.Add(current);
                    continue;
                  case 14:
                    assemblyInstanceList1.Add(current);
                    continue;
                  default:
                    continue;
                }
              }
              int num7 = (int) subTransaction.RollBack();
            }
            break;
          }
        }
        break;
      case 2:
        using (List<AssemblyInstance>.Enumerator enumerator = assemblyToProcess.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            AssemblyInstance current = enumerator.Current;
            List<EDGECAM.errorCondition> source3 = new List<EDGECAM.errorCondition>();
            List<EDGECAM.errorCondition> errors = new List<EDGECAM.errorCondition>();
            List<Element> elementList3 = new List<Element>();
            List<Element> elementList4 = new List<Element>();
            int num8 = (int) subTransaction.Start();
            this.SavePXML(current, revitDoc, slopeTolerance, out errors, elementList3, elementList4, elementsInAssemblyView);
            source3.AddRange((IEnumerable<EDGECAM.errorCondition>) errors);
            int num9 = (int) subTransaction.RollBack();
            int num10 = (int) subTransaction.Start();
            this.SaveUnitechnik(current, revitDoc, slopeTolerance, out errors, elementList3, "502", elementsInAssemblyView);
            source3.AddRange((IEnumerable<EDGECAM.errorCondition>) errors);
            int num11 = (int) subTransaction.RollBack();
            int num12 = (int) subTransaction.Start();
            this.SaveUnitechnik(current, revitDoc, slopeTolerance, out errors, elementList3, "600", elementsInAssemblyView);
            source3.AddRange((IEnumerable<EDGECAM.errorCondition>) errors);
            int num13 = (int) subTransaction.RollBack();
            int num14 = (int) subTransaction.Start();
            this.SaveUnitechnik(current, revitDoc, slopeTolerance, out errors, elementList3, "700", elementsInAssemblyView);
            source3.AddRange((IEnumerable<EDGECAM.errorCondition>) errors);
            int num15 = (int) subTransaction.RollBack();
            foreach (int num16 in source3.Distinct<EDGECAM.errorCondition>())
            {
              switch (num16)
              {
                case 0:
                  assemblyInstanceList12.Add(current);
                  continue;
                case 1:
                  assemblyInstanceList3.Add(current);
                  continue;
                case 2:
                  assemblyInstanceList4.Add(current);
                  continue;
                case 3:
                  assemblyInstanceList5.Add(current);
                  continue;
                case 4:
                  assemblyInstanceList9.Add(current);
                  continue;
                case 5:
                  assemblyInstanceList14.Add(current);
                  continue;
                case 6:
                  assemblyInstanceList13.Add(current);
                  continue;
                case 7:
                  assemblyInstanceList2.Add(current);
                  continue;
                case 8:
                  assemblyInstanceList7.Add(current);
                  source1.AddRange((IEnumerable<Element>) elementList3);
                  source1 = source1.Distinct<Element>().ToList<Element>();
                  continue;
                case 9:
                  assemblyInstanceList8.Add(current);
                  source2.AddRange((IEnumerable<Element>) elementList4);
                  source2 = source2.Distinct<Element>().ToList<Element>();
                  continue;
                case 10:
                  assemblyInstanceList6.Add(current);
                  continue;
                case 11:
                  assemblyInstanceList11.Add(current);
                  continue;
                case 12:
                  assemblyInstanceList10.Add(current);
                  continue;
                case 14:
                  assemblyInstanceList1.Add(current);
                  continue;
                default:
                  continue;
              }
            }
          }
          break;
        }
      default:
        throw new Exception("Cancelled");
    }
    if (assemblyInstanceList12.Count > 0)
    {
      assemblyInstanceList12.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog3 = new TaskDialog("Elements Below Pallet");
      taskDialog3.MainInstruction = "One or more assemblies had elements below the pallet and could not be exported. Expand for details.";
      taskDialog3.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList12)
      {
        TaskDialog taskDialog4 = taskDialog3;
        taskDialog4.ExpandedContent = $"{taskDialog4.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog3.Show();
    }
    if (assemblyInstanceList2.Count > 0)
    {
      assemblyInstanceList2.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog5 = new TaskDialog("Invalid Geometry");
      taskDialog5.MainInstruction = "One or more assemblies had invalid geometry for export. Expand for details.";
      taskDialog5.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList2)
      {
        TaskDialog taskDialog6 = taskDialog5;
        taskDialog6.ExpandedContent = $"{taskDialog6.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog5.Show();
    }
    if (assemblyInstanceList3.Count > 0)
    {
      assemblyInstanceList3.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog7 = new TaskDialog("Invalid Structural Framing");
      taskDialog7.MainInstruction = "One or more assemblies had more than one Structural Framing element and could not be exported. Expand for details.";
      taskDialog7.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList3)
      {
        TaskDialog taskDialog8 = taskDialog7;
        taskDialog8.ExpandedContent = $"{taskDialog8.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog7.Show();
    }
    if (assemblyInstanceList4.Count > 0)
    {
      assemblyInstanceList4.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog9 = new TaskDialog("Invalid Structural Framing");
      taskDialog9.MainInstruction = "One or more assemblies had no Structural Framing element and could not be exported. Expand for details.";
      taskDialog9.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList4)
      {
        TaskDialog taskDialog10 = taskDialog9;
        taskDialog10.ExpandedContent = $"{taskDialog10.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog9.Show();
    }
    if (assemblyInstanceList5.Count > 0)
    {
      assemblyInstanceList5.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog11 = new TaskDialog("Invalid Structural Framing");
      taskDialog11.MainInstruction = "One or more assemblies had no valid Structural Framing elements and could not be exported. Valid structural framing elements must be families of category \"Structural Framing\" with \"PRECAST CONCRETE\" material for model behavior. Expand for details.";
      taskDialog11.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList5)
      {
        TaskDialog taskDialog12 = taskDialog11;
        taskDialog12.ExpandedContent = $"{taskDialog12.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog11.Show();
    }
    if (assemblyInstanceList6.Count > 0)
    {
      assemblyInstanceList6.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog13 = new TaskDialog("Negative Piece Weight");
      taskDialog13.MainInstruction = "One or more assemblies had a negative value for piece weight and could not be exported. Expand for details.";
      taskDialog13.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList6)
      {
        TaskDialog taskDialog14 = taskDialog13;
        taskDialog14.ExpandedContent = $"{taskDialog14.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog13.Show();
    }
    if (assemblyInstanceList9.Count > 0)
    {
      assemblyInstanceList9.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog15 = new TaskDialog("Spline Geometry");
      taskDialog15.MainInstruction = "One or more assemblies contained spline edges in edge geometry (most likely caused by an arc cutout intersecting a sloped planar face). This could lead to inaccuracies in your export's geometry. Expand for details.";
      taskDialog15.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList9)
      {
        TaskDialog taskDialog16 = taskDialog15;
        taskDialog16.ExpandedContent = $"{taskDialog16.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog15.Show();
    }
    if (assemblyInstanceList13.Count > 0)
    {
      assemblyInstanceList13.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog17 = new TaskDialog("Too Many Digits");
      taskDialog17.MainInstruction = "One or more fields in the following exports had more than the acceptable number of digits for their format. This could lead to inaccuracies in your export. Expand for details.";
      taskDialog17.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList13)
      {
        TaskDialog taskDialog18 = taskDialog17;
        taskDialog18.ExpandedContent = $"{taskDialog18.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog17.Show();
    }
    if (assemblyInstanceList14.Count > 0)
    {
      assemblyInstanceList14.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog19 = new TaskDialog("Invalid Signs");
      taskDialog19.MainInstruction = "One or more fields in the following exports had negative values that the UXML 5.2 format cannot accept and could not be exported. Expand for details.";
      taskDialog19.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList14)
      {
        TaskDialog taskDialog20 = taskDialog19;
        taskDialog20.ExpandedContent = $"{taskDialog20.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog19.Show();
    }
    if (assemblyInstanceList7.Count > 0)
    {
      assemblyInstanceList7.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog21 = new TaskDialog("Invalid Rebar");
      taskDialog21.MainInstruction = "One or more assemblies contained rebar families that contain invalid geometry and could not be exported. Expand for details.";
      taskDialog21.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList7)
      {
        TaskDialog taskDialog22 = taskDialog21;
        taskDialog22.ExpandedContent = $"{taskDialog22.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog21.ExpandedContent += "\nBar families flagged as invalid:\n";
      List<string> source4 = new List<string>();
      foreach (Element element in source1)
      {
        if (element is FamilyInstance familyInstance)
          source4.Add(familyInstance.Symbol.FamilyName);
      }
      List<string> list = source4.Distinct<string>().ToList<string>();
      list.Sort((Comparison<string>) ((p, q) => CAM_Utils.CompareStrings(p, q)));
      foreach (string str in list)
      {
        TaskDialog taskDialog23 = taskDialog21;
        taskDialog23.ExpandedContent = $"{taskDialog23.ExpandedContent}{str}\n";
      }
      taskDialog21.Show();
    }
    if (assemblyInstanceList8.Count > 0)
    {
      assemblyInstanceList8.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog24 = new TaskDialog("Invalid Mesh");
      taskDialog24.MainInstruction = "One or more assemblies contained mesh/wire that contains invalid geometry and could not be exported. Expand for details.";
      taskDialog24.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList8)
      {
        TaskDialog taskDialog25 = taskDialog24;
        taskDialog25.ExpandedContent = $"{taskDialog25.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog24.ExpandedContent += "\nWire families flagged as invalid:\n";
      List<string> source5 = new List<string>();
      foreach (Element element in source2)
      {
        if (element is FamilyInstance familyInstance)
          source5.Add(familyInstance.Symbol.FamilyName);
      }
      List<string> list = source5.Distinct<string>().ToList<string>();
      list.Sort((Comparison<string>) ((p, q) => CAM_Utils.CompareStrings(p, q)));
      foreach (string str in list)
      {
        TaskDialog taskDialog26 = taskDialog24;
        taskDialog26.ExpandedContent = $"{taskDialog26.ExpandedContent}{str}\n";
      }
      taskDialog24.Show();
    }
    if (assemblyInstanceList10.Count > 0)
    {
      assemblyInstanceList10.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog27 = new TaskDialog("No OrderNo");
      taskDialog27.MainInstruction = "One or more assemblies had no assigned OrderNo field. Check your assembly for this parameter, you may need to run Project Shared Parameters if it does not exist. A placeholder has been added to your export. Expand for details.";
      taskDialog27.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList10)
      {
        TaskDialog taskDialog28 = taskDialog27;
        taskDialog28.ExpandedContent = $"{taskDialog28.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog27.Show();
    }
    if (assemblyInstanceList11.Count > 0)
    {
      assemblyInstanceList11.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
      TaskDialog taskDialog29 = new TaskDialog("No ProdNo");
      taskDialog29.MainInstruction = "One or more assemblies had no assigned ProdNo field. Check your assembly for this parameter, you may need to run Project Shared Parameters if it does not exist. A placeholder has been added to your export. Expand for details.";
      taskDialog29.ExpandedContent = "";
      foreach (AssemblyInstance assemblyInstance in assemblyInstanceList11)
      {
        TaskDialog taskDialog30 = taskDialog29;
        taskDialog30.ExpandedContent = $"{taskDialog30.ExpandedContent}{assemblyInstance.Name}\n";
      }
      taskDialog29.Show();
    }
    if (assemblyInstanceList1.Count <= 0)
      return;
    assemblyInstanceList1.Sort((Comparison<AssemblyInstance>) ((p, q) => CAM_Utils.CompareStrings(p.Name, q.Name)));
    TaskDialog taskDialog31 = new TaskDialog("Success");
    taskDialog31.MainInstruction = "One or more assemblies were exported successfully. Expand for details.";
    taskDialog31.ExpandedContent = "";
    foreach (AssemblyInstance assemblyInstance in assemblyInstanceList1)
    {
      TaskDialog taskDialog32 = taskDialog31;
      taskDialog32.ExpandedContent = $"{taskDialog32.ExpandedContent}{assemblyInstance.Name}\n";
    }
    taskDialog31.Show();
  }

  public enum errorCondition
  {
    ElementsBelowPallet,
    TooManyStructuralFraming,
    NoStructuralFraming,
    InvalidStructuralFraming,
    Splines,
    UXML52Negative,
    TooManyDigits,
    InvalidGeometry,
    ProblemBars,
    ProblemMesh,
    NegativeWeight,
    NoProdNo,
    NoOrderNo,
    Other,
    Success,
  }
}
