// Decompiled with JetBrains decompiler
// Type: EDGE.RebarTools.CoreSlabMarkingUtil
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.RebarTools;

public class CoreSlabMarkingUtil
{
  public const string rebarMarkParamName = "REBAR_CONTROL_MARK";

  public static void MarkAssembledRebar(Document revitDoc, IEnumerable<AssemblyInstance> assemblies)
  {
    Dictionary<string, string> dictionary = Utils.SettingsUtils.Settings.LoadMarkVerificationSettings(revitDoc, App.MarkPrefixFolderPath, out string _, out bool _);
    foreach (AssemblyInstance assembly in assemblies)
    {
      foreach (ElementId memberId in (IEnumerable<ElementId>) assembly.GetMemberIds())
      {
        Element element = revitDoc.GetElement(memberId);
        if (CoreSlabMarkingUtil.IsRebarAutomationRebar(element))
        {
          string constructionProduct = assembly.GetStructuralFramingElement().GetConstructionProduct();
          Parameter parameter = element.LookupParameter("REBAR_CONTROL_MARK");
          if (parameter != null && dictionary.ContainsKey(constructionProduct))
            parameter.Set(dictionary[constructionProduct] + element.GetControlMark());
        }
      }
    }
  }

  private static bool IsRebarAutomationRebar(Element elem)
  {
    return elem.GetManufactureComponent().Contains("REBAR") && Utils.ElementUtils.Parameters.LookupParameter(elem, "BAR_SHAPE") != null && Utils.ElementUtils.Parameters.LookupParameter(elem, "BAR_DIAMETER") != null && !Utils.ElementUtils.Parameters.GetParameterAsBool(elem, "HARDWARE_DETAIL") && !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "BAR_SHAPE").Contains("STRAIGHT") && !Utils.ElementUtils.Parameters.GetParameterAsString(elem, "IDENTITY_DESCRIPTION").Contains("STRAIGHT BAR (X LENGTH SCHEDULED)");
  }

  public static void ProcessRebarByMarkNotAssembled(Document revitDoc)
  {
    foreach (Element element in new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilyInstance)).Where<Element>((Func<Element, bool>) (elem => CoreSlabMarkingUtil.IsRebarAutomationRebar(elem) && elem.AssemblyInstanceId == ElementId.InvalidElementId)))
    {
      Parameter parameter = element.LookupParameter("REBAR_CONTROL_MARK");
      if (parameter != null && !parameter.IsReadOnly)
        parameter.Set("NA");
    }
  }
}
