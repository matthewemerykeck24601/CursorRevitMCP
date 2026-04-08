// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationDrawing.InsulationDrawingUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.IEnumerable_Extensions;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationDrawing;

public static class InsulationDrawingUtils
{
  public static bool ValidForInsulationDrawing(Element elem, string upperBound = "")
  {
    if (!(elem is FamilyInstance familyInstance) || !Utils.SelectionUtils.SelectionUtils.CheckforInsulationManufactureComponent((Element) familyInstance))
      return false;
    string parameterAsString = Parameters.GetParameterAsString((Element) familyInstance, "INSULATION_MARK");
    if (string.IsNullOrWhiteSpace(parameterAsString))
      return false;
    if (upperBound != "")
    {
      if (upperBound == parameterAsString)
        return false;
      if (new List<string>()
      {
        upperBound,
        parameterAsString
      }.NaturalSort().ToList<string>()[0] == parameterAsString)
        return false;
    }
    return InsulationDrawingUtils.checkVolumeDifferenceInsulation(familyInstance);
  }

  public static bool checkVolumeDifferenceInsulation(FamilyInstance insulationInstance)
  {
    double parameterAsDouble1 = Parameters.GetParameterAsDouble((Element) insulationInstance, "DIM_LENGTH");
    if (parameterAsDouble1 <= 0.0)
      return false;
    double parameterAsDouble2 = Parameters.GetParameterAsDouble((Element) insulationInstance, "DIM_WIDTH");
    if (parameterAsDouble2 <= 0.0)
      return false;
    double parameterAsDouble3 = Parameters.GetParameterAsDouble((Element) insulationInstance, "DIM_THICKNESS");
    if (parameterAsDouble3 <= 0.0)
      return false;
    double other = parameterAsDouble1 * parameterAsDouble2 * parameterAsDouble3;
    Solid solid1 = (Solid) null;
    foreach (Solid instanceSolid in Solids.GetInstanceSolids((Element) insulationInstance))
      solid1 = !((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(instanceSolid, solid1, BooleanOperationsType.Union) : instanceSolid;
    return insulationInstance != null && !solid1.Volume.ApproximatelyEquals(other, 0.001);
  }

  public static bool ValidInsulationDrawingView(View view)
  {
    if (view == null)
      return false;
    int viewType = (int) view.ViewType;
    return view.ViewType == ViewType.DrawingSheet || view.ViewType == ViewType.Legend || view.ViewType == ViewType.Detail;
  }

  public static bool InsulationLocked(Element insulation)
  {
    return Parameters.GetParameterAsBool(insulation, "INSULATION_LOCK") && Parameters.GetParameterAsString(insulation, "INSULATION_HOST_GUID") == insulation.UniqueId.ToString();
  }

  public static View CreateNewLegend(View legend, int scale = -1, string name = "")
  {
    ICollection<ElementId> elementIds = ElementTransformUtils.CopyElement(legend.Document, legend.Id, XYZ.Zero);
    View newLegend = (View) null;
    foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
    {
      if (legend.Document.GetElement(id) is View element && element.ViewType == ViewType.Legend)
        newLegend = element;
    }
    if (!string.IsNullOrWhiteSpace(name))
    {
      string str = string.Empty;
      int num = 1;
      while (newLegend.Name != name)
      {
        try
        {
          newLegend.Name = name + str;
        }
        catch (Exception ex)
        {
          str = $" ({num++.ToString()})";
        }
      }
    }
    newLegend.ViewTemplateId = ElementId.InvalidElementId;
    newLegend.DetailLevel = ViewDetailLevel.Fine;
    if (scale != -1)
      newLegend.Scale = scale;
    IList<Element> elements = new FilteredElementCollector(legend.Document, newLegend.Id).ToElements();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (Element element in (IEnumerable<Element>) elements)
    {
      if (!(element.Name == "ExtentElem") && element.OwnerViewId.IntegerValue == newLegend.Id.IntegerValue)
        elementIdList.Add(element.Id);
    }
    legend.Document.Delete((ICollection<ElementId>) elementIdList);
    return newLegend;
  }
}
