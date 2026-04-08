// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CheckFlagger.CheckFlaggerToolUtils
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.TicketTools.CheckFlagger;

public class CheckFlaggerToolUtils
{
  public static TaskDialog GenerateSuccessDialog(List<AssemblyInstance> assemblies)
  {
    TaskDialog successDialog = new TaskDialog("Check Utility");
    string str = "Affected Assemblies:" + Environment.NewLine;
    foreach (AssemblyInstance assembly in assemblies)
      str = str + assembly.AssemblyTypeName + Environment.NewLine;
    successDialog.ExpandedContent = str;
    return successDialog;
  }

  public static string GetUsername(Document revitDoc)
  {
    return FlaggerUtils.FormatUsername(revitDoc.Application.Username);
  }

  private static bool ValidateParameters(Document revitDoc, out List<string> missingParameters)
  {
    missingParameters = new List<string>();
    List<SharedParameterElement> list = new FilteredElementCollector(revitDoc).WhereElementIsNotElementType().OfClass(typeof (SharedParameterElement)).Cast<SharedParameterElement>().ToList<SharedParameterElement>();
    foreach (string str in new List<string>()
    {
      "TICKET_IS_ENGINEERING_CHECKED",
      "TICKET_IS_DRAFTING_CHECKED",
      "TICKET_ENGINEERING_CHECKED_USER_INITIAL",
      "TICKET_ENGINEERING_CHECKED_DATE_INITIAL",
      "TICKET_ENGINEERING_CHECKED_USER_CURRENT",
      "TICKET_ENGINEERING_CHECKED_DATE_CURRENT",
      "TICKET_DRAFTING_CHECKED_USER_INITIAL",
      "TICKET_DRAFTING_CHECKED_DATE_INITIAL",
      "TICKET_DRAFTING_CHECKED_USER_CURRENT",
      "TICKET_DRAFTING_CHECKED_DATE_CURRENT",
      "TICKET_NEEDS_REVISION"
    })
    {
      string paramName = str;
      if (!list.Any<SharedParameterElement>((Func<SharedParameterElement, bool>) (spe => spe.Name.Equals(paramName))))
        missingParameters.Add(paramName);
    }
    return missingParameters.Count <= 0;
  }

  public static bool CheckDocument(Document currDoc) => !currDoc.IsFamilyDocument;

  public static bool CheckEnvironmentGeneral(
    UIDocument uiDoc,
    List<ElementId> selectionGroup,
    out string error,
    out List<AssemblyInstance> assemblies)
  {
    if (CheckFlaggerToolUtils.CheckEnvironment(uiDoc, selectionGroup, out error, out assemblies))
      return true;
    if (!error.Contains("Worksharing"))
      TaskDialog.Show("Check Utility Error", error);
    return false;
  }

  public static bool CheckEnvironment(
    UIDocument uiDoc,
    List<ElementId> selectionGroup,
    out string error,
    out List<AssemblyInstance> assemblies)
  {
    Document document = uiDoc.Document;
    assemblies = new List<AssemblyInstance>();
    error = "";
    if (!CheckFlaggerToolUtils.CheckDocument(document))
    {
      error = "The engineering and drafting check utility cannot be run from the family editor. Please run the tool from a project context.";
      return false;
    }
    List<string> missingParameters;
    if (!CheckFlaggerToolUtils.ValidateParameters(document, out missingParameters))
    {
      error = "One or more relevant parameters were missing from the project. Please run Project Shared Parameters and try again." + Environment.NewLine;
      error = $"{error}Missing parameters:{Environment.NewLine}";
      foreach (string str in missingParameters)
        error = error + str + Environment.NewLine;
      return false;
    }
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    foreach (ElementId id in selectionGroup)
    {
      Element element = document.GetElement(id);
      AssemblyInstance assembly = element as AssemblyInstance;
      if (assembly != null)
      {
        flag1 = true;
        if (assembly.GetStructuralFramingElement() != null)
        {
          flag2 = true;
          if (!assemblies.Any<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.UniqueId.Equals(assembly.UniqueId))))
            assemblies.Add(assembly);
        }
      }
      else if (element is FamilyInstance && element.Category.Id.Equals((object) Category.GetCategory(document, BuiltInCategory.OST_StructuralFraming).Id))
      {
        Element flatElement = (Element) AssemblyInstances.GetFlatElement(document, element as FamilyInstance);
        if (flatElement.AssemblyInstanceId != (ElementId) null && !flatElement.AssemblyInstanceId.Equals((object) ElementId.InvalidElementId))
        {
          flag3 = true;
          assembly = document.GetElement(flatElement.AssemblyInstanceId) as AssemblyInstance;
          if (!assemblies.Any<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => e.UniqueId.Equals(assembly.UniqueId))))
            assemblies.Add(assembly);
        }
        else
          flag3 = true;
      }
    }
    if (assemblies.Count != 0)
      return true;
    error = !flag3 || flag2 || flag1 ? (!(!flag3 & flag1) ? "No valid structural framing elements or assemblies were selected. Please try again." : "Selected assembly or assemblies did not contain structural framing elements. This tool must be run on structural framing assemblies.") : "Selected structural framing elements were not assembled. This tool can only be run on structural framing assemblies.";
    return false;
  }
}
