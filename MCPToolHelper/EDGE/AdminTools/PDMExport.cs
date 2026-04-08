// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.PDMExport
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using EDGE.RebarTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools;

[Transaction(TransactionMode.Manual)]
internal class PDMExport : IExternalCommand
{
  private Dictionary<PrecastMaterialType, ElementId> mappedMaterialIndex;
  private IEnumerable<FamilyInstance> allProjectAddons;
  private Document revitDoc;
  private UIDocument uiDoc;
  private List<string> noMarkIds = new List<string>();
  private List<string> quotesParenRemoved = new List<string>();
  private ElementMulticategoryFilter multiCatFilter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
  {
    BuiltInCategory.OST_GenericModel,
    BuiltInCategory.OST_SpecialityEquipment
  });
  private bool extCancelled;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    this.revitDoc = commandData.Application.ActiveUIDocument.Document;
    this.uiDoc = commandData.Application.ActiveUIDocument;
    if (this.revitDoc.IsFamilyDocument)
    {
      new TaskDialog("PDM Export")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "PDM Export must be run in the Project Environment",
        MainContent = "You are currently in the family editor, PDM Export must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    this.mappedMaterialIndex = Materials.GetPrecastMaterialIds(this.revitDoc);
    this.allProjectAddons = Components.GetAllProjectAddons(this.revitDoc);
    ICollection<ElementId> collection1 = commandData.Application.ActiveUIDocument.Selection.GetElementIds();
    TaskDialog taskDialog1 = new TaskDialog("PDM Export");
    taskDialog1.Id = "ID_PDMAssemblyExport";
    taskDialog1.MainIcon = (TaskDialogIcon) (int) ushort.MaxValue;
    taskDialog1.Title = "PDM Assembly Export";
    taskDialog1.TitleAutoPrefix = true;
    taskDialog1.AllowCancellation = true;
    taskDialog1.MainInstruction = "PDM Assembly Export";
    taskDialog1.MainContent = "Select the scope for running the PDM Assembly Export.";
    taskDialog1.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1001, "Run the PDM Export for the Whole Project.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1002, "Run the PDM Export for the Active View.");
    taskDialog1.AddCommandLink((TaskDialogCommandLinkId) 1003, "Run the PDM Export for Selected Assemblies.");
    taskDialog1.CommonButtons = (TaskDialogCommonButtons) 8;
    taskDialog1.DefaultButton = (TaskDialogResult) 2;
    TaskDialogResult taskDialogResult1 = taskDialog1.Show();
    if (taskDialogResult1 == 7 || taskDialogResult1 == 2)
      return (Result) 1;
    List<AssemblyInstance> list1;
    if (taskDialogResult1 == 1001)
      list1 = new FilteredElementCollector(this.revitDoc).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
    else if (taskDialogResult1 == 1002)
    {
      list1 = new FilteredElementCollector(this.revitDoc, this.revitDoc.ActiveView.Id).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
    }
    else
    {
      if (taskDialogResult1 != 1003)
        return (Result) 1;
      if (collection1.Count == 0)
        collection1 = References.PickNewReferencesAssem("Select assemblies to export.", this.uiDoc);
      if (collection1.Count == 0)
        return (Result) 1;
      List<ElementId> collection2 = new List<ElementId>();
      foreach (ElementId id in (IEnumerable<ElementId>) collection1)
      {
        List<ElementId> list2 = this.revitDoc.GetElement(id).GetSubComponentIds().ToList<ElementId>();
        if (list2.Count<ElementId>() == 0)
          collection2.Add(id);
        else
          collection2.AddRange((IEnumerable<ElementId>) list2);
      }
      List<ElementId> elementIdList = new List<ElementId>();
      foreach (ElementId id in (IEnumerable<ElementId>) collection1)
      {
        Element element = this.revitDoc.GetElement(id);
        if (element is AssemblyInstance)
        {
          List<ElementId> list3 = (element as AssemblyInstance).GetMemberIds().ToList<ElementId>();
          elementIdList.AddRange((IEnumerable<ElementId>) list3);
        }
        else
          elementIdList.Add(element.Id);
      }
      elementIdList.AddRange((IEnumerable<ElementId>) collection1);
      elementIdList.AddRange((IEnumerable<ElementId>) collection2);
      if (elementIdList.Count == 0)
      {
        TaskDialog.Show("PDM Export", "No assemblies with valid materials have been selected.");
        return (Result) 1;
      }
      list1 = new FilteredElementCollector(this.revitDoc, (ICollection<ElementId>) elementIdList).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
    }
    List<AssemblyInstance> list4 = list1.Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (a => a.GetStructuralFramingElement() != null)).Where<AssemblyInstance>((Func<AssemblyInstance, bool>) (e => !Utils.ElementUtils.Parameters.GetParameterAsBool((Element) e, "HARDWARE_DETAIL"))).ToList<AssemblyInstance>();
    if (list4.Count == 0)
    {
      new TaskDialog("PDM Export")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = "No assemblies found to export. Only structural framing assemblies are valid for use with this tool."
      }.Show();
      return (Result) 1;
    }
    string str1;
    string str2;
    try
    {
      List<string> stringList1 = new List<string>();
      List<string> stringList2 = new List<string>();
      List<string> fileNames = new List<string>();
      string name1 = this.revitDoc.ProjectInformation.Name;
      List<Element> elementList1 = new List<Element>();
      List<AssemblyInstance> assemblyInstanceList1 = new List<AssemblyInstance>();
      List<AssemblyInstance> assemblyInstanceList2 = new List<AssemblyInstance>();
      foreach (AssemblyInstance assem in list4)
      {
        List<Element> list5 = new FilteredElementCollector(this.revitDoc, assem.GetMemberIds()).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>();
        List<Element> list6 = new FilteredElementCollector(this.revitDoc, assem.GetMemberIds()).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) this.multiCatFilter).ToList<Element>();
        elementList1.AddRange((IEnumerable<Element>) list5);
        if (list5.Count != 0)
        {
          if (list5.Count > 1)
            assemblyInstanceList1.Add(assem);
          else if (Solids.GetInstanceSolids(list5.First<Element>()).Count <= 0)
          {
            assemblyInstanceList2.Add(assem);
          }
          else
          {
            string str3 = assem.Name;
            if (str3.Contains("\"") || str3.Contains("(") || str3.Contains(")"))
            {
              str3 = str3.Replace("\"", "").Replace("(", "").Replace(")", "");
              this.quotesParenRemoved.Add($"Assembly Name: {assem.Name} => {str3}");
            }
            StringBuilder stringBuilder1 = new StringBuilder();
            string str4 = "";
            foreach (Element element in list5)
            {
              element.GetSuperComponent();
              double length1 = 0.0;
              double width1 = 0.0;
              string str5 = "\"INFO-P\"";
              string pieceMark = this.GetPieceMark(element);
              string quantityStructFrame = this.GetQuantityStructFrame(element, assem);
              double volume = this.GetVolume(element, assem);
              string weightTons = this.GetWeightTons(element, volume);
              string cubicYards = this.GetCubicYards(element, volume);
              str4 = this.GetProdCode(element);
              string strandDesign = this.GetStrandDesign(element);
              string designSheet = this.GetDesignSheet(element, assem);
              this.GetSqFt(element);
              string length2 = this.GetLength(element, out length1);
              string width2 = this.GetWidth(element, out width1);
              string depth = this.GetDepth(element);
              string grossArea = this.GetGrossArea(length1, width1);
              string netArea = this.GetNetArea(element, assem);
              string topDepth = this.GetTopDepth(element);
              this.GetBottomDepth(element);
              string str6 = $"({str5} ({pieceMark} {quantityStructFrame} {weightTons} {cubicYards} {str4} {strandDesign} {designSheet} \"\" {length2} {width2} {depth} {grossArea} {netArea} {topDepth} \"\" \"\" \"\" \"\" \"\" \"\")) (\"Finish-P\" nil)";
              stringBuilder1.Append(str6 ?? "");
            }
            string str7 = $"\\SHP-{str4.Replace("\"", "").ToString()}-{str3}.dat";
            string str8 = $"\\SHP-{str4.Replace("\"", "").ToString()}-{str3}.DWG";
            List<Element> elementList2 = new List<Element>();
            List<Element> rebarList = new List<Element>();
            List<Element> elementList3 = new List<Element>();
            List<Element> elementList4 = new List<Element>();
            foreach (Element elem in list6)
            {
              string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT");
              string name2 = (elem as FamilyInstance).Symbol.Family.Name;
              if (!parameterAsString.Contains("RAW") || !parameterAsString.Contains("CONSUMABLE"))
              {
                if (parameterAsString.Contains("EMBED") || parameterAsString.Contains("LIFTING") || parameterAsString.Contains("SHEARGRID"))
                {
                  string controlMark = elem.GetControlMark();
                  if (controlMark.Equals("") || controlMark.Equals((string) null))
                    this.noMarkIds.Add($"{elem.Id.ToString()} {name2}");
                  else
                    elementList2.Add(elem);
                }
                else if (parameterAsString.Contains("REBAR"))
                {
                  string controlMark = elem.GetControlMark();
                  if (controlMark.Equals("") || controlMark.Equals((string) null))
                    this.noMarkIds.Add($"{elem.Id.ToString()} {name2}");
                  else
                    rebarList.Add(elem);
                }
                else if (parameterAsString.Contains("INSULATION") || parameterAsString.Contains("WOOD NAILER") || parameterAsString.Contains("WWF") || parameterAsString.Contains("MESH"))
                {
                  string controlMark = elem.GetControlMark();
                  if (controlMark.Equals("") || controlMark.Equals((string) null))
                    this.noMarkIds.Add($"{elem.Id.ToString()} {name2}");
                  else
                    elementList3.Add(elem);
                }
                else if (parameterAsString.Contains("REVEAL") || name2.ToUpper().Contains("REVEAL"))
                {
                  string controlMark = elem.GetControlMark();
                  if (controlMark.Equals("") || controlMark.Equals((string) null))
                    this.noMarkIds.Add($"{elem.Id.ToString()} {name2}");
                  else
                    elementList4.Add(elem);
                }
              }
            }
            double num1 = Math.Ceiling(Utils.ElementUtils.Parameters.GetParameterAsDouble(list5.First<Element>(), "TOTAL_REVEAL1_LENGTH"));
            double num2 = Math.Ceiling(Utils.ElementUtils.Parameters.GetParameterAsDouble(list5.First<Element>(), "TOTAL_REVEAL2_LENGTH"));
            double num3 = Math.Ceiling(Utils.ElementUtils.Parameters.GetParameterAsDouble(list5.First<Element>(), "TOTAL_REVEAL3_LENGTH"));
            string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString(list5.First<Element>(), "TOTAL_REVEAL1_MARK");
            string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString(list5.First<Element>(), "TOTAL_REVEAL2_MARK");
            string parameterAsString3 = Utils.ElementUtils.Parameters.GetParameterAsString(list5.First<Element>(), "TOTAL_REVEAL3_MARK");
            foreach (Element elem in elementList2)
            {
              string controlMark = elem.GetControlMark();
              string name3 = (elem as FamilyInstance).Symbol.Family.Name;
              if (controlMark.Equals("") || controlMark.Equals((string) null))
                this.noMarkIds.Add($"{elem.Id.ToString()} {name3}");
            }
            foreach (Element elem in rebarList)
            {
              string controlMark = elem.GetControlMark();
              string name4 = (elem as FamilyInstance).Symbol.Family.Name;
              if (controlMark.Equals("") || controlMark.Equals((string) null))
                this.noMarkIds.Add($"{elem.Id.ToString()} {name4}");
            }
            foreach (Element elem in elementList3)
            {
              string controlMark = elem.GetControlMark();
              string name5 = (elem as FamilyInstance).Symbol.Family.Name;
              if (controlMark.Equals("") || controlMark.Equals((string) null))
                this.noMarkIds.Add($"{elem.Id.ToString()} {name5}");
            }
            if (elementList2.Count > 0)
            {
              stringBuilder1.Append(" (\"CIM\" (");
              StringBuilder stringBuilder2 = new StringBuilder();
              foreach (Element sf in elementList2)
              {
                string pieceMark = this.GetPieceMark(sf);
                string name6 = (sf as FamilyInstance).Symbol.Family.Name;
                if (pieceMark.Equals("") || pieceMark.Equals((string) null))
                {
                  if (!this.noMarkIds.Contains(sf.Id.ToString()))
                    this.noMarkIds.Add($"{sf.Id.ToString()} {name6}");
                }
                else
                {
                  string identityDescription = this.GetIdentityDescription(sf);
                  string quantity = this.GetQuantity(sf, assem);
                  string str9 = $"{pieceMark} {identityDescription} {quantity} ";
                  if (!stringBuilder2.ToString().Contains(str9))
                    stringBuilder2.Append(str9);
                }
              }
              stringBuilder1.Append(stringBuilder2.ToString().TrimEnd(' '));
              stringBuilder1.Append("))");
            }
            else if (elementList2.Count <= 0)
              stringBuilder1.Append(" (\"CIM\" (\"\" \"\" \"\"))");
            int count = rebarList.Count;
            if (count > 0)
            {
              stringBuilder1.Append($" (\"REB-TOT\" ({this.GetRebarTotal(rebarList, assem)}))");
              stringBuilder1.Append($" (\"REB\" ({this.GetRebarGrouping(rebarList, assem)}))");
            }
            else if (count <= 0)
            {
              stringBuilder1.Append(" (\"REB-TOT\" (\"\" \"\"))");
              stringBuilder1.Append(" (\"REB\" (\"\" \"\" \"\"))");
            }
            if (elementList3.Count > 0 || elementList4.Count > 0)
            {
              stringBuilder1.Append(" (\"MISC\" (");
              StringBuilder stringBuilder3 = new StringBuilder();
              foreach (Element element in elementList3)
              {
                string pieceMark = this.GetPieceMark(element);
                string name7 = (element as FamilyInstance).Symbol.Family.Name;
                if (pieceMark.Equals("") || pieceMark.Equals((string) null))
                {
                  if (!this.noMarkIds.Contains(element.Id.ToString()))
                    this.noMarkIds.Add($"{element.Id.ToString()} {name7}");
                }
                else
                {
                  string quantityMisc = this.GetQuantityMisc(element, assem);
                  string str10 = $"{pieceMark} {quantityMisc} ";
                  if (!stringBuilder3.ToString().Contains(str10))
                    stringBuilder3.Append(str10);
                }
              }
              if (num1 < 0.0 && num2 < 0.0 && num3 < 0.0)
              {
                foreach (Element element in elementList4)
                {
                  string pieceMark = this.GetPieceMark(element);
                  string name8 = (element as FamilyInstance).Symbol.Family.Name;
                  if (pieceMark.Equals("") || pieceMark.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(element.Id.ToString()))
                      this.noMarkIds.Add($"{element.Id.ToString()} {name8}");
                  }
                  else
                  {
                    string quantityMisc = this.GetQuantityMisc(element, assem);
                    string str11 = $"{pieceMark} {quantityMisc} ";
                    if (!stringBuilder3.ToString().Contains(str11))
                      stringBuilder3.Append(str11);
                  }
                }
              }
              else
              {
                if (num1 >= 0.0)
                {
                  string str12 = parameterAsString1;
                  string name9 = (list5.First<Element>() as FamilyInstance).Symbol.Family.Name;
                  if (str12.Equals("") || str12.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(list5.First<Element>().Id.ToString()))
                      this.noMarkIds.Add($"{list5.First<Element>().Id.ToString()} {name9} TOTAL_REVEAL1_MARK");
                  }
                  else
                  {
                    string str13 = $"\"{str12}\" \"{num1.ToString()}\" ";
                    if (!stringBuilder3.ToString().Contains(str13))
                      stringBuilder3.Append(str13);
                  }
                }
                if (num2 >= 0.0)
                {
                  string str14 = parameterAsString2;
                  string name10 = (list5.First<Element>() as FamilyInstance).Symbol.Family.Name;
                  if (str14.Equals("") || str14.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(list5.First<Element>().Id.ToString()))
                      this.noMarkIds.Add($"{list5.First<Element>().Id.ToString()} {name10} TOTAL_REVEAL2_MARK");
                  }
                  else
                  {
                    string str15 = $"\"{str14}\" \"{num2.ToString()}\" ";
                    if (!stringBuilder3.ToString().Contains(str15))
                      stringBuilder3.Append(str15);
                  }
                }
                if (num3 >= 0.0)
                {
                  string str16 = parameterAsString3;
                  string name11 = (list5.First<Element>() as FamilyInstance).Symbol.Family.Name;
                  if (str16.Equals("") || str16.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(list5.First<Element>().Id.ToString()))
                      this.noMarkIds.Add($"{list5.First<Element>().Id.ToString()} {name11} TOTAL_REVEAL3_MARK");
                  }
                  else
                  {
                    string str17 = $"\"{str16}\" \"{num3.ToString()}\" ";
                    if (!stringBuilder3.ToString().Contains(str17))
                      stringBuilder3.Append(str17);
                  }
                }
              }
              stringBuilder1.Append(stringBuilder3.ToString().TrimEnd(' '));
              stringBuilder1.Append("))");
            }
            else if (elementList3.Count <= 0)
            {
              if (num1 >= 0.0 || num2 >= 0.0 || num3 >= 0.0)
              {
                stringBuilder1.Append(" (\"MISC\" (");
                if (num1 >= 0.0)
                {
                  string str18 = parameterAsString1;
                  string name12 = (list5.First<Element>() as FamilyInstance).Symbol.Family.Name;
                  if (str18.Equals("") || str18.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(list5.First<Element>().Id.ToString()))
                      this.noMarkIds.Add($"{list5.First<Element>().Id.ToString()} {name12} TOTAL_REVEAL1_MARK");
                  }
                  else
                  {
                    string str19 = $"\"{str18}\" \"{num1.ToString()}\" ";
                    stringBuilder1.Append(str19);
                  }
                }
                if (num2 >= 0.0)
                {
                  string str20 = parameterAsString2;
                  string name13 = (list5.First<Element>() as FamilyInstance).Symbol.Family.Name;
                  if (str20.Equals("") || str20.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(list5.First<Element>().Id.ToString()))
                      this.noMarkIds.Add($"{list5.First<Element>().Id.ToString()} {name13} TOTAL_REVEAL2_MARK");
                  }
                  else
                  {
                    string str21 = $"\"{str20}\" \"{num2.ToString()}\" ";
                    stringBuilder1.Append(str21);
                  }
                }
                if (num3 >= 0.0)
                {
                  string str22 = parameterAsString3;
                  string name14 = (list5.First<Element>() as FamilyInstance).Symbol.Family.Name;
                  if (str22.Equals("") || str22.Equals((string) null))
                  {
                    if (!this.noMarkIds.Contains(list5.First<Element>().Id.ToString()))
                      this.noMarkIds.Add($"{list5.First<Element>().Id.ToString()} {name14} TOTAL_REVEAL3_MARK");
                  }
                  else
                  {
                    string str23 = $"\"{str22}\" \"{num3.ToString()}\" ";
                    stringBuilder1.Append(str23);
                  }
                }
                stringBuilder1.Append("))");
              }
              else
                stringBuilder1.Append(" (\"MISC\" (\"\" \"\"))");
            }
            string str24 = $"({stringBuilder1.ToString()}(\"MESH\" nil))";
            stringList1.Add(str24);
            stringList2.Add(str7);
            fileNames.Add(str8);
          }
        }
      }
      if (assemblyInstanceList1.Count > 0)
      {
        TaskDialog taskDialog2 = new TaskDialog("PDM Export");
        taskDialog2.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog2.MainContent = "More than one structural framing has been detected in the following assemblies and will not be exported.";
        StringBuilder stringBuilder = new StringBuilder();
        foreach (AssemblyInstance assemblyInstance in assemblyInstanceList1)
          stringBuilder.AppendLine($"{assemblyInstance.Name} {assemblyInstance.Id?.ToString()}");
        taskDialog2.ExpandedContent = stringBuilder.ToString();
        taskDialog2.Show();
      }
      if (assemblyInstanceList2.Count > 0)
      {
        TaskDialog taskDialog3 = new TaskDialog("PDM Export");
        taskDialog3.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog3.MainContent = "The Structural Framing in the following assemblies has no solid geometry and will not be exported.";
        StringBuilder stringBuilder = new StringBuilder();
        foreach (AssemblyInstance assemblyInstance in assemblyInstanceList2)
          stringBuilder.AppendLine($"{assemblyInstance.Name} {assemblyInstance.Id?.ToString()}");
        taskDialog3.ExpandedContent = stringBuilder.ToString();
        taskDialog3.Show();
      }
      if (this.noMarkIds.Count > 0)
      {
        TaskDialog taskDialog4 = new TaskDialog("PDM Export");
        taskDialog4.MainContent = "Materials with no Control Marks Detected";
        taskDialog4.MainInstruction = "The following materials do not have control marks. Materials must have a control mark in order to be exported. Proceed with export?";
        taskDialog4.AddCommandLink((TaskDialogCommandLinkId) 1001, "Proceed");
        taskDialog4.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel");
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string noMarkId in this.noMarkIds)
          stringBuilder.AppendLine(noMarkId);
        taskDialog4.ExpandedContent = stringBuilder.ToString();
        if (taskDialog4.Show() != 1001)
          return (Result) 1;
      }
      if (this.quotesParenRemoved.Count > 0)
      {
        TaskDialog taskDialog5 = new TaskDialog("PDM Export");
        taskDialog5.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog5.MainContent = "Quotes and/or Parentheses were removed from the Parameter(s) of following Materials. ";
        taskDialog5.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue");
        taskDialog5.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel Export ");
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string str25 in this.quotesParenRemoved)
          stringBuilder.AppendLine(str25);
        taskDialog5.ExpandedContent = stringBuilder.ToString();
        if (taskDialog5.Show() != 1001)
          return (Result) 1;
      }
      if (elementList1.Count == 0)
      {
        new TaskDialog("PDM Export")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainContent = "No Structural Framing members found in the assemblies to export"
        }.Show();
        return (Result) 1;
      }
      if (stringList2.Count == 0)
        return (Result) 1;
      str1 = "C:\\EDGEforRevit";
      str2 = "C:\\EDGEforRevit";
      Parameter parameter1 = this.revitDoc.ProjectInformation.LookupParameter("ERP_SAVE_FOLDER_PATH");
      Parameter parameter2 = this.revitDoc.ProjectInformation.LookupParameter("CAD_SAVE_FOLDER_PATH");
      if (parameter1 != null && parameter1.HasValue)
        str1 = parameter1.AsString();
      if (str1.Equals("") || str1 == null)
        str1 = "C:\\EDGEforRevit";
      if (parameter2 != null && parameter2.HasValue)
        str2 = parameter2.AsString();
      if (str2.Equals("") || str2 == null)
        str2 = "C:\\EDGEforRevit";
      bool flag = false;
      while (!flag)
      {
        TaskDialog taskDialog6 = new TaskDialog("PDM Export");
        taskDialog6.MainInstruction = $"Export (.dat) files will be sent to: {str1}\n\nCAD (.DWG) files will be sent to: {str2}";
        taskDialog6.CommonButtons = (TaskDialogCommonButtons) 9;
        taskDialog6.AddCommandLink((TaskDialogCommandLinkId) 1001, "Browse for new path for Export (.dat) files");
        taskDialog6.AddCommandLink((TaskDialogCommandLinkId) 1002, "Browse for new path for CAD (.DWG) files");
        TaskDialogResult taskDialogResult2 = taskDialog6.Show();
        if (taskDialogResult2 <= 2)
        {
          if (taskDialogResult2 != 1)
          {
            if (taskDialogResult2 == 2)
              return (Result) 1;
          }
          else
          {
            flag = true;
            if (parameter1 != null && !parameter1.IsReadOnly && (!parameter1.HasValue || !parameter1.AsString().Equals(str1)))
            {
              if (new TaskDialog("PDM Export")
              {
                MainInstruction = $"Set {str1} to new default export path?",
                CommonButtons = ((TaskDialogCommonButtons) 6)
              }.Show() == 6)
              {
                using (Transaction transaction = new Transaction(this.revitDoc, "Set new export path"))
                {
                  int num4 = (int) transaction.Start();
                  parameter1.Set(str1);
                  int num5 = (int) transaction.Commit();
                }
              }
            }
            if (parameter2 != null && !parameter2.IsReadOnly && (!parameter2.HasValue || !parameter2.AsString().Equals(str2)))
            {
              if (new TaskDialog("PDM Export")
              {
                MainInstruction = $"Set {str2} to default CAD path?",
                CommonButtons = ((TaskDialogCommonButtons) 6)
              }.Show() == 6)
              {
                using (Transaction transaction = new Transaction(this.revitDoc, "Set new export path"))
                {
                  int num6 = (int) transaction.Start();
                  parameter2.Set(str2);
                  int num7 = (int) transaction.Commit();
                }
              }
            }
          }
        }
        else if (taskDialogResult2 != 1001)
        {
          if (taskDialogResult2 == 1002)
            str2 = this.BrowseNewPath(str2);
        }
        else
          str1 = this.BrowseNewPath(str1);
      }
      DirectoryInfo directoryInfo1 = new DirectoryInfo(str1);
      if (!directoryInfo1.Exists)
      {
        try
        {
          directoryInfo1.Create();
        }
        catch (IOException ex)
        {
          new TaskDialog("PDM Export")
          {
            MainInstruction = "Unable to save to location specified",
            MainContent = ("PDM Export was unable to create a new directory in the specified location: " + str1)
          }.Show();
          return (Result) 1;
        }
      }
      DirectoryInfo directoryInfo2 = new DirectoryInfo(str2);
      if (!directoryInfo2.Exists)
      {
        try
        {
          directoryInfo2.Create();
        }
        catch (IOException ex)
        {
          new TaskDialog("PDM Export")
          {
            MainInstruction = "Unable to save to location specified",
            MainContent = ("PDM Export was unable to create a new directory in the specified location: " + str2)
          }.Show();
          return (Result) 1;
        }
      }
      this.SaveToExtensibleStorage(fileNames, str2);
      if (this.extCancelled)
        return (Result) 1;
      for (int index = 0; index < stringList1.Count; ++index)
      {
        string path1 = str1 + stringList2[index];
        string path2 = str2 + fileNames[index];
        string contents = stringList1[index];
        File.WriteAllText(path1, contents);
        File.WriteAllText(path2, "");
      }
    }
    catch (Exception ex)
    {
      message = ex.StackTrace;
      return (Result) -1;
    }
    new TaskDialog("PDM Export")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      MainContent = $"File(s) successfully saved to {str1}\n\nCAD file(s) successfully saved to {str2}"
    }.Show();
    return (Result) 0;
  }

  private void SaveToExtensibleStorage(List<string> fileNames, string exportPath)
  {
    string path = new DirectoryInfo(exportPath).Parent.FullName + "\\DATA\\";
    List<string> stringList1 = new List<string>();
    foreach (string fileName in fileNames)
      stringList1.Add(fileName.Replace("\\", ""));
    List<AssemblyInstance> list1 = new FilteredElementCollector(this.revitDoc).OfClass(typeof (AssemblyInstance)).Cast<AssemblyInstance>().ToList<AssemblyInstance>();
    List<string> stringList2 = new List<string>();
    foreach (AssemblyInstance assInst in list1)
    {
      string name = assInst.Name;
      if (assInst.GetStructuralFramingElement() != null)
      {
        string str = $"SHP-{this.GetProdCode(assInst.GetStructuralFramingElement()).ToString()}-{name}.DWG".Replace("\"", "");
        if (!stringList2.Contains(str))
          stringList2.Add(str);
      }
    }
    using (Transaction transaction = new Transaction(this.revitDoc, "PDM Export"))
    {
      SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("900aea31-d0e4-460e-bf6d-2f6bd26d346e"));
      schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
      schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
      schemaBuilder.AddSimpleField("FileName", typeof (string)).SetDocumentation("Hidden storage for PDM export manifest.");
      schemaBuilder.SetSchemaName("FileName");
      Schema schema = schemaBuilder.Finish();
      List<ElementId> elementIdList = new List<ElementId>();
      int num1 = (int) transaction.Start();
      FilteredElementCollector source1 = new FilteredElementCollector(this.revitDoc).OfClass(typeof (DataStorage));
      List<string> stringList3 = new List<string>();
      Field field1 = schema.GetField("FileName");
      foreach (Element element in source1.ToList<Element>())
      {
        Entity entity = element.GetEntity(schema);
        if (entity.RecognizedField(field1))
          stringList3.Add(entity.Get<string>(field1));
      }
      List<string> stringList4 = new List<string>();
      try
      {
        if (File.Exists(path + "\\DRAW.LST"))
          stringList4.AddRange((IEnumerable<string>) ((IEnumerable<string>) File.ReadAllLines(path + "\\DRAW.LST")).ToList<string>());
      }
      catch (Exception ex)
      {
        new TaskDialog("PDM Export")
        {
          MainContent = "Unable to access existing DRAW.LST file. Please ensure that you have permissions to access the file.",
          ExpandedContent = new StringBuilder().ToString()
        }.Show();
        this.extCancelled = true;
        int num2 = (int) transaction.RollBack();
        return;
      }
      foreach (string str in stringList1)
      {
        if (!stringList3.Contains(str.Replace("\\", "")))
        {
          DataStorage dataStorage = DataStorage.Create(this.revitDoc);
          Entity entity = new Entity(schema);
          Field field2 = schema.GetField("FileName");
          entity.Set<string>(field2, str.Replace("\\", ""));
          dataStorage.SetEntity(entity);
          elementIdList.Add(dataStorage.Id);
        }
      }
      List<string> source2 = new List<string>();
      List<string> stringList5 = new List<string>();
      foreach (string str in stringList4)
      {
        if (!stringList2.Contains(str) && !stringList3.Contains(str))
          source2.Add(str);
        else
          stringList5.Add(str);
      }
      if (source2.Count > 0)
      {
        source2 = source2.Distinct<string>().ToList<string>();
        TaskDialog taskDialog = new TaskDialog("PDM Export");
        taskDialog.MainContent = "The following filenames were detected in the currently existing DRAW.LST file but not in the current model. Please make sure all users have synced with central to ensure that your model has the latest content. Choosing to continue the export will remove the below assemblies from the DRAW.LST file.";
        StringBuilder stringBuilder = new StringBuilder();
        foreach (string str in source2)
          stringBuilder.AppendLine(str);
        taskDialog.ExpandedContent = stringBuilder.ToString();
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue Export");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Cancel Export");
        if (!taskDialog.Show().Equals((object) (TaskDialogResult) 1001))
        {
          this.extCancelled = true;
          int num3 = (int) transaction.RollBack();
          return;
        }
      }
      try
      {
        if (File.Exists(path + "\\DRAW.LST"))
        {
          File.WriteAllText(path + "\\DRAW.LST", string.Empty);
        }
        else
        {
          Directory.CreateDirectory(path);
          File.WriteAllText(path + "\\DRAW.LST", string.Empty);
        }
      }
      catch (Exception ex)
      {
        new TaskDialog("PDM Export")
        {
          MainContent = "Unable to access existing DRAW.LST file. Please ensure that you have permissions to access the file.",
          ExpandedContent = new StringBuilder().ToString()
        }.Show();
        this.extCancelled = true;
        int num4 = (int) transaction.RollBack();
        return;
      }
      FilteredElementCollector source3 = new FilteredElementCollector(this.revitDoc).OfClass(typeof (DataStorage));
      List<string> source4 = new List<string>();
      if (source3.ToList<Element>().Count > 0)
      {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (Element element in source3.ToList<Element>())
        {
          Entity entity = element.GetEntity(schema);
          if (entity.RecognizedField(field1))
          {
            string str = entity.Get<string>(field1);
            if (stringList1.Contains(str) || stringList2.Contains(str))
              source4.Add(str);
          }
        }
        foreach (string str in source2)
        {
          if (stringList1.Contains(str) || stringList2.Contains(str))
            source4.Add(str);
        }
        foreach (string str in stringList5)
        {
          if (!source4.Contains(str) && (stringList1.Contains(str) || stringList2.Contains(str)))
            source4.Add(str);
        }
        List<string> list2 = source4.Distinct<string>().ToList<string>();
        list2.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
        foreach (string str in list2)
        {
          if (!str.Equals("") && !str.Equals(" ") && !str.Equals("\n") && !str.Trim().ToString().Equals(""))
            stringBuilder.AppendLine(str);
        }
        --stringBuilder.Length;
        try
        {
          File.WriteAllText(path + "\\DRAW.LST", stringBuilder.ToString());
        }
        catch (Exception ex)
        {
          new TaskDialog("PDM Export")
          {
            MainContent = ("Unable to write DRAW.LST file. Please ensure that you have permissions to write to " + path),
            ExpandedContent = new StringBuilder().ToString()
          }.Show();
          this.extCancelled = true;
          int num5 = (int) transaction.RollBack();
          return;
        }
      }
      int num6 = (int) transaction.Commit();
    }
  }

  private string BrowseNewPath(string exportPath)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    folderBrowserDialog.Description = "Select a path for export";
    folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
    folderBrowserDialog.SelectedPath = exportPath;
    return folderBrowserDialog.ShowDialog() == DialogResult.OK ? folderBrowserDialog.SelectedPath : exportPath;
  }

  private string BrowseNewAutoCadPath(string exportPath)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    folderBrowserDialog.Description = "Select a path for the CAD file";
    folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
    folderBrowserDialog.SelectedPath = exportPath;
    return folderBrowserDialog.ShowDialog() == DialogResult.OK ? folderBrowserDialog.SelectedPath : exportPath;
  }

  private string GetBottomDepth(Element sf) => $"\"\"";

  private string GetTopDepth(Element sf)
  {
    double num = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_WYTHE_INSULATION");
    if (num < 0.0)
      num = 0.0;
    string str = Math.Round(UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Inches), 4).ToString();
    if (str.Equals("0"))
      str = "";
    return $"\"{str}\"";
  }

  private string GetGrossArea(double length, double width)
  {
    return $"\"{Math.Round(length * width, 2).ToString()}\"";
  }

  private string GetNetArea(Element sf, AssemblyInstance assem)
  {
    return $"\"{Math.Round(this.CalcNetArea(sf, assem), 2).ToString()}\"";
  }

  private string GetDepth(Element sf)
  {
    double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_THICKNESS");
    if (parameterAsDouble < 0.0)
    {
      parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_DEPTH");
      if (parameterAsDouble < 0.0)
        return "\"\"";
    }
    return $"\"{UnitUtils.ConvertFromInternalUnits(parameterAsDouble, UnitTypeId.FractionalInches).ToString()}\"";
  }

  private string GetWidth(Element sf, out double width)
  {
    Element superComponent = (sf as FamilyInstance).SuperComponent;
    if (superComponent != null)
      sf = superComponent;
    double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_HEIGHT_ACTUAL");
    if (parameterAsDouble < 0.0)
      parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_HEIGHT");
    if (parameterAsDouble < 0.0)
    {
      parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_WIDTH_ACTUAL");
      if (parameterAsDouble < 0.0)
        parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_WIDTH");
      if (parameterAsDouble < 0.0)
      {
        width = 0.0;
        return "\"\"";
      }
    }
    ForgeTypeId unitTypeId = this.revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
    string input = unitTypeId != UnitTypeId.FeetFractionalInches || unitTypeId != UnitTypeId.FractionalInches ? this.ConvertToFeetFractInches(parameterAsDouble.ToString()) : UnitFormatUtils.Format(this.revitDoc.GetUnits(), SpecTypeId.Length, parameterAsDouble, false);
    width = parameterAsDouble;
    string str = Regex.Replace(input, "(\\s\\p{Pd}\\s)", "-");
    int num = unitTypeId == UnitTypeId.FeetFractionalInches ? 1 : 0;
    return "\"" + str;
  }

  private string GetLength(Element sf, out double length)
  {
    Element superComponent = (sf as FamilyInstance).SuperComponent;
    if (superComponent != null)
      sf = superComponent;
    double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_LENGTH_ACTUAL");
    if (parameterAsDouble < 0.0)
      parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "DIM_LENGTH");
    if (parameterAsDouble < 0.0)
    {
      length = 0.0;
      return "\"\"";
    }
    ForgeTypeId unitTypeId = this.revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
    string input = unitTypeId != UnitTypeId.FeetFractionalInches || unitTypeId != UnitTypeId.FractionalInches ? this.ConvertToFeetFractInches(parameterAsDouble.ToString()) : UnitFormatUtils.Format(this.revitDoc.GetUnits(), SpecTypeId.Length, parameterAsDouble, false);
    length = parameterAsDouble;
    string str = Regex.Replace(input, "(\\s\\p{Pd}\\s)", "-");
    int num = unitTypeId == UnitTypeId.FeetFractionalInches ? 1 : 0;
    return "\"" + str;
  }

  private string GetTotalLength(List<Element> rebarList)
  {
    double num = 0.0;
    foreach (Element rebar in rebarList)
    {
      double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(rebar, "DIM_LENGTH");
      num += parameterAsDouble;
    }
    if (num == 0.0)
      return "\"\"";
    string str = UnitFormatUtils.Format(this.revitDoc.GetUnits(), SpecTypeId.Length, num, false);
    return this.revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId() == UnitTypeId.FeetFractionalInches ? "\"" + str : $"\"{str}\"";
  }

  private string GetRebarTotal(List<Element> rebarList, AssemblyInstance assem)
  {
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (Element rebar in rebarList)
      elementIdList.Add(rebar.Id);
    List<IGrouping<double, Element>> list = new FilteredElementCollector(this.revitDoc, (ICollection<ElementId>) elementIdList).WherePasses((ElementFilter) this.multiCatFilter).Where<Element>((Func<Element, bool>) (elem => elem is FamilyInstance)).GroupBy<Element, double>((Func<Element, double>) (s => Utils.ElementUtils.Parameters.GetParameterAsDouble(s, "BAR_DIAMETER"))).Where<IGrouping<double, Element>>((Func<IGrouping<double, Element>, bool>) (grp => grp.Count<Element>() > 0)).ToList<IGrouping<double, Element>>();
    StringBuilder stringBuilder = new StringBuilder();
    foreach (IGrouping<double, Element> grouping in list)
    {
      string barSize = this.GetBarSize(grouping.Key);
      double a = 0.0;
      foreach (Element elem in (IEnumerable<Element>) grouping)
      {
        double num = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_LENGTH");
        if (num <= 0.0)
          num = 0.0;
        a += num;
      }
      a = Math.Ceiling(a);
      stringBuilder.Append($"\"{barSize}\" \"{a.ToString()}\" ");
    }
    return stringBuilder.ToString().TrimEnd(' ');
  }

  private string GetBarSize(double barDiameterFt)
  {
    string barSize = "";
    switch (this.ResolveBarDiam(barDiameterFt))
    {
      case 3:
        barSize = "#3";
        break;
      case 4:
        barSize = "#4";
        break;
      case 5:
        barSize = "#5";
        break;
      case 6:
        barSize = "#6";
        break;
      case 7:
        barSize = "#7";
        break;
      case 8:
        barSize = "#8";
        break;
      case 9:
        barSize = "#9";
        break;
      case 10:
        barSize = "#10";
        break;
      case 11:
        barSize = "#11";
        break;
    }
    return barSize;
  }

  private int ResolveBarDiam(double barDiameter)
  {
    double dbl2 = barDiameter * 12.0;
    if (dbl2 > 1.42 || dbl2 < 0.37)
      return 0;
    for (int BarNum = 3; BarNum < 12; ++BarNum)
    {
      if (this.GetBarDiamForUSBarNumber(BarNum).ApproximatelyEqual(dbl2))
        return BarNum;
    }
    return 0;
  }

  private double GetBarDiamForUSBarNumber(int BarNum)
  {
    if (BarNum < 9)
      return (double) BarNum * 1.0 / 8.0;
    switch (BarNum)
    {
      case 9:
        return 289.0 / 256.0;
      case 10:
        return 1.27;
      case 11:
        return 1.41;
      default:
        return 0.0;
    }
  }

  private string GetRebarGrouping(List<Element> rebarList, AssemblyInstance assem)
  {
    StringBuilder stringBuilder1 = new StringBuilder();
    List<Element> elementList1 = new List<Element>();
    List<Element> elementList2 = new List<Element>();
    foreach (Element rebar in rebarList)
    {
      if (Utils.ElementUtils.Parameters.GetParameterAsString(rebar, "BAR_SHAPE").ToUpper().Contains("STRAIGHT"))
        elementList2.Add(rebar);
      else
        elementList1.Add(rebar);
    }
    foreach (Element element in elementList1)
    {
      string pieceMark = this.GetPieceMark(element);
      string name = (element as FamilyInstance).Symbol.Family.Name;
      if (pieceMark.Equals("") || pieceMark.Equals((string) null))
      {
        if (!this.noMarkIds.Contains(element.Id.ToString()))
          this.noMarkIds.Add($"{element.Id.ToString()} {name}");
      }
      else
      {
        string quantity = this.GetQuantity(element, assem);
        double num1 = Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "DIM_LENGTH");
        if (num1 <= 0.0)
          num1 = 0.0;
        num1 = UnitUtils.ConvertFromInternalUnits(num1, UnitTypeId.Inches);
        string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element, "BAR_SHAPE");
        double num2 = 0.0;
        StringBuilder stringBuilder2 = new StringBuilder();
        stringBuilder2.Append("(");
        for (char ch = 'A'; ch <= 'Z'; ++ch)
        {
          string str = ch.ToString();
          double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "BAR_LENGTH_" + ch.ToString());
          if (parameterAsDouble > 0.0)
          {
            double num3 = UnitUtils.ConvertFromInternalUnits(parameterAsDouble, UnitTypeId.Inches);
            num2 += num3;
            stringBuilder2.Append($"(\"{str}\" {num3.ToString()})");
          }
        }
        stringBuilder2.Append(")");
        if (num2 == 0.0)
        {
          if (!stringBuilder1.ToString().Contains(pieceMark))
            stringBuilder1.Append($"{pieceMark} \"\" {quantity} ");
        }
        else if (!stringBuilder1.ToString().Contains(pieceMark))
          stringBuilder1.Append($"{pieceMark} ({stringBuilder2.ToString()} {num1.ToString()} \"{parameterAsString}\") {quantity} ");
      }
    }
    foreach (Element element in elementList2)
    {
      string pieceMark = this.GetPieceMark(element);
      string name = (element as FamilyInstance).Symbol.Family.Name;
      if (pieceMark.Equals("") || pieceMark.Equals((string) null))
      {
        if (!this.noMarkIds.Contains(element.Id.ToString()))
          this.noMarkIds.Add($"{element.Id.ToString()} {name}");
      }
      else
      {
        string quantity = this.GetQuantity(element, assem);
        double num4 = Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "DIM_LENGTH");
        if (num4 <= 0.0)
          num4 = 0.0;
        num4 = UnitUtils.ConvertFromInternalUnits(num4, UnitTypeId.Inches);
        string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(element, "BAR_SHAPE");
        double num5 = 0.0;
        StringBuilder stringBuilder3 = new StringBuilder();
        stringBuilder3.Append("(");
        for (char ch = 'A'; ch <= 'Z'; ++ch)
        {
          string str = ch.ToString();
          double parameterAsDouble = Utils.ElementUtils.Parameters.GetParameterAsDouble(element, "BAR_LENGTH_" + ch.ToString());
          if (parameterAsDouble > 0.0)
          {
            double num6 = UnitUtils.ConvertFromInternalUnits(parameterAsDouble, UnitTypeId.Inches);
            num5 += num6;
            stringBuilder3.Append($"(\"{str}\" {num6.ToString()})");
          }
        }
        stringBuilder3.Append(")");
        if (num5 == 0.0)
        {
          if (!stringBuilder1.ToString().Contains(pieceMark))
            stringBuilder1.Append($"{pieceMark} \"\" {quantity} ");
        }
        else if (!stringBuilder1.ToString().Contains(pieceMark))
          stringBuilder1.Append($"{pieceMark} ({stringBuilder3.ToString()} {num4.ToString()} \"{parameterAsString}\") {quantity} ");
      }
    }
    stringBuilder1.Append("\"\" \"\" \"\" ");
    return stringBuilder1.ToString().TrimEnd(' ');
  }

  private string GetSqFt(Element sf)
  {
    return $"\"{Math.Round(Utils.ElementUtils.Parameters.GetParameterAsDouble(sf, "WEIGHT_PER_UNIT"), 2).ToString()}\"";
  }

  private string GetDesignSheet(Element sf, AssemblyInstance assem)
  {
    Element superComponent = (sf as FamilyInstance).SuperComponent;
    if (superComponent != null)
      sf = superComponent;
    string str = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "DESIGN_SHEET");
    if (str.Contains("\"") || str.Contains("(") || str.Contains(")"))
    {
      str = str.Replace("\"", "").Replace("(", "").Replace(")", "");
      Family family = (sf as FamilyInstance).Symbol.Family;
      this.quotesParenRemoved.Add($"{sf.Id.ToString()} {family.Name}  Design Sheet");
    }
    return $"\"{str}\"";
  }

  private string GetStrandDesign(Element sf)
  {
    Element superComponent = (sf as FamilyInstance).SuperComponent;
    if (superComponent != null)
      sf = superComponent;
    string str = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "DESIGN_TYPE");
    if (str.Contains("\"") || str.Contains("(") || str.Contains(")"))
    {
      str = str.Replace("\"", "").Replace("(", "").Replace(")", "");
      Family family = (sf as FamilyInstance).Symbol.Family;
      this.quotesParenRemoved.Add($"{sf.Id.ToString()} {family.Name}  Design Type");
    }
    return $"\"{str}\"";
  }

  private string GetProdCode(Element sf)
  {
    Element superComponent = (sf as FamilyInstance).SuperComponent;
    if (superComponent != null)
      sf = superComponent;
    string str = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "PRODUCT_CODE");
    if (str.Contains("\"") || str.Contains("(") || str.Contains(")"))
    {
      str = str.Replace("\"", "").Replace("(", "").Replace(")", "");
      Family family = (sf as FamilyInstance).Symbol.Family;
      this.quotesParenRemoved.Add($"{sf.Id.ToString()} {family.Name}  Product Code");
    }
    return $"\"{str}\"";
  }

  private double GetVolume(Element sf, AssemblyInstance assem)
  {
    double volume = 0.0;
    IEnumerable<Element> elements = assem.GetMemberIds().Select<ElementId, Element>((Func<ElementId, Element>) (s => this.revitDoc.GetElement(s))).Where<Element>((Func<Element, bool>) (elem => elem.Category.Id.IntegerValue != -2001370)).Where<Element>((Func<Element, bool>) (elem => Utils.ElementUtils.Parameters.GetParameterValueStringValue(elem, "Material").ToUpper().Contains("PRECAST CONCRETE") || Utils.ElementUtils.Parameters.GetParameterValueStringValue(elem, "Material").ToUpper().Contains("ARCHITECTURAL PRECAST") || Utils.ElementUtils.Parameters.GetParameterValueStringValue(elem, BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).ToUpper().Contains("PRECAST CONCRETE") || Utils.ElementUtils.Parameters.GetParameterValueStringValue(elem, BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).ToUpper().Contains("ARCHITECTURAL PRECAST") || Utils.ElementUtils.Parameters.GetParameterValueStringValue(this.revitDoc.GetElement(elem.GetTypeId()), BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).ToUpper().Contains("PRECAST CONCRETE") || Utils.ElementUtils.Parameters.GetParameterValueStringValue(this.revitDoc.GetElement(elem.GetTypeId()), BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).ToUpper().Contains("ARCHITECTURAL PRECAST")));
    Solid solid = (Solid) null;
    foreach (Element element in elements)
    {
      if (element.Category.Id.IntegerValue != -2001370)
      {
        GeometryElement geometryElement = element.get_Geometry(new Options());
        foreach (GeometryObject geometryObject in geometryElement)
        {
          solid = geometryObject as Solid;
          if ((GeometryObject) solid != (GeometryObject) null && solid.Faces.Size != 0 && solid.Edges.Size != 0)
            volume += solid.Volume;
        }
        if ((GeometryObject) solid == (GeometryObject) null)
        {
          foreach (GeometryObject geometryObject1 in geometryElement)
          {
            GeometryInstance geometryInstance = geometryObject1 as GeometryInstance;
            if ((GeometryObject) geometryInstance != (GeometryObject) null)
            {
              foreach (GeometryObject geometryObject2 in geometryInstance.SymbolGeometry)
              {
                solid = geometryObject2 as Solid;
                if ((GeometryObject) solid != (GeometryObject) null && solid.Faces.Size != 0 && solid.Edges.Size != 0)
                  volume += solid.Volume;
              }
            }
          }
        }
      }
    }
    return volume;
  }

  private string GetCubicYards(Element sf, double volumeInRevitInteralUnits)
  {
    return $"\"{Math.Round(UnitUtils.ConvertFromInternalUnits(volumeInRevitInteralUnits, UnitTypeId.CubicYards), 2).ToString()}\"";
  }

  private string GetWeightTons(Element sf, double volumeInRevitInteralUnits)
  {
    string str = "";
    Element superComponent = (sf as FamilyInstance).SuperComponent;
    Parameter parameter = superComponent != null ? superComponent.LookupParameter("WEIGHT_PER_UNIT") : sf.LookupParameter("WEIGHT_PER_UNIT");
    if (parameter != null)
      str = Math.Round(parameter.AsDouble() * volumeInRevitInteralUnits / 2000.0, 2).ToString();
    return $"\"{str}\"";
  }

  private string GetQuantityStructFrame(Element sf, AssemblyInstance assem)
  {
    int num = 0;
    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "CONTROL_MARK");
    new FilteredElementCollector(this.revitDoc).OfClass(typeof (FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).ToList<Element>();
    foreach (IGrouping<string, Element> source in new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_StructuralFraming).Where<Element>((Func<Element, bool>) (elem => elem is FamilyInstance)).Where<Element>((Func<Element, bool>) (e => !e.HasSuperComponent())).GroupBy<Element, string>((Func<Element, string>) (s => s.GetControlMark())).Where<IGrouping<string, Element>>((Func<IGrouping<string, Element>, bool>) (grp => grp.Count<Element>() > 0)).ToList<IGrouping<string, Element>>())
    {
      if (source.Key.Equals(parameterAsString))
      {
        num = source.Count<Element>();
        break;
      }
    }
    return $"\"{num.ToString()}\"";
  }

  private string GetQuantity(Element sf, AssemblyInstance assem)
  {
    int num = 0;
    string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "CONTROL_MARK");
    foreach (Element elem in new FilteredElementCollector(this.revitDoc, assem.GetMemberIds()).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) this.multiCatFilter).ToList<Element>())
    {
      if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "CONTROL_MARK").ToUpper().Equals(parameterAsString.ToUpper()))
        ++num;
    }
    return $"\"{num.ToString()}\"";
  }

  private string GetQuantityMisc(Element misc, AssemblyInstance assem)
  {
    double a = 0.0;
    string parameterAsString1 = Utils.ElementUtils.Parameters.GetParameterAsString(misc, "CONTROL_MARK");
    string parameterAsString2 = Utils.ElementUtils.Parameters.GetParameterAsString(misc, "MANUFACTURE_COMPONENT");
    string name = (misc as FamilyInstance).Symbol.Family.Name;
    foreach (Element elem in new FilteredElementCollector(this.revitDoc, assem.GetMemberIds()).OfClass(typeof (FamilyInstance)).WherePasses((ElementFilter) this.multiCatFilter).ToList<Element>())
    {
      if (Utils.ElementUtils.Parameters.GetParameterAsString(elem, "CONTROL_MARK").ToUpper().Equals(parameterAsString1.ToUpper()))
      {
        if (parameterAsString2.Contains("INSULATION"))
        {
          double num1 = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "Volume");
          if (num1 < 0.0)
            num1 = 0.0;
          double num2 = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_THICKNESS");
          if (num2 < 0.0)
            num2 = 0.0;
          double num3 = num2 != 0.0 ? num1 / num2 : 0.0;
          a += num3;
        }
        else if (parameterAsString2.Contains("WWF"))
        {
          double num = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_AREA");
          if (num < 0.0)
            num = 0.0;
          a += num;
        }
        else if (parameterAsString2.Contains("WOOD NAILER") || parameterAsString2.Contains("REVEAL") || name.Contains("REVEAL"))
        {
          double num = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_LENGTH");
          if ((parameterAsString2.Contains("REVEAL") || name.Contains("REVEAL")) && num < 0.0)
            num = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "Length");
          if (num < 0.0)
            num = 0.0;
          a += num;
        }
        else if (parameterAsString2.Contains("MESH"))
        {
          double num = Utils.ElementUtils.Parameters.GetParameterAsDouble(elem, "DIM_WWF_XX");
          if (num < 0.0)
            num = 0.0;
          a += num;
        }
      }
    }
    return $"\"{Math.Ceiling(a).ToString()}\"";
  }

  private string GetPieceMark(Element sf)
  {
    string str = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "CONTROL_MARK");
    if (str.Contains("\"") || str.Contains("(") || str.Contains(")"))
    {
      str = str.Replace("\"", "").Replace("(", "").Replace(")", "");
      Family family = (sf as FamilyInstance).Symbol.Family;
      this.quotesParenRemoved.Add($"{sf.Id.ToString()} {family.Name}  Control Mark");
    }
    return $"\"{str}\"";
  }

  private string GetIdentityDescription(Element sf)
  {
    string str = Utils.ElementUtils.Parameters.GetParameterAsString(sf, "IDENTITY_DESCRIPTION");
    if (str.Contains("\"") || str.Contains("(") || str.Contains(")"))
    {
      str = str.Replace("\"", "").Replace("(", "").Replace(")", "");
      Family family = (sf as FamilyInstance).Symbol.Family;
      this.quotesParenRemoved.Add($"{sf.Id.ToString()} {family.Name}  Identity Description");
    }
    return $"\"{str}\"";
  }

  private double CalcNetArea(Element sf, AssemblyInstance assem)
  {
    Transaction transaction = new Transaction(this.revitDoc, "Create Dummy Solid");
    using (transaction)
    {
      int num1 = (int) transaction.Start();
      List<Tuple<CurveLoop, XYZ, bool>> outerCurves = new List<Tuple<CurveLoop, XYZ, bool>>();
      List<Tuple<CurveLoop, XYZ>> upwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
      List<Tuple<CurveLoop, XYZ>> downwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
      double num2 = 0.0;
      Transform transform = assem.GetTransform();
      Solid solid = (Solid) null;
      if (Solids.GetInstanceSolids(sf).Count > 0)
      {
        foreach (Solid instanceSolid in Solids.GetInstanceSolids(sf))
        {
          try
          {
            if ((GeometryObject) solid == (GeometryObject) null)
              solid = SolidUtils.Clone(instanceSolid);
            else
              BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid, instanceSolid, BooleanOperationsType.Union);
          }
          catch (Exception ex)
          {
            TaskDialog.Show("PDM Export", ex.Message);
            return -1.0;
          }
        }
        double maxZ1 = double.MinValue;
        double minZ = double.MaxValue;
        CAM_Utils.GetCurveLoops(solid, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ1, out minZ, transform);
        double[] minMaxZ = CAM_Utils.GetMinMaxZ(solid, transform);
        minZ = minMaxZ[0];
        double maxZ2 = minMaxZ[1];
        double height = Math.Abs(maxZ2 - minZ);
        bool overallSplineError = false;
        Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary = new Dictionary<Solid, Tuple<string, double, bool>>();
        if (CAM_Utils.GetLayerDictionary(LayerDictionary, outerCurves, upwardFacingCurves, downwardFacingCurves, this.revitDoc, "material", 1.0, minZ, maxZ2, height, out overallSplineError, transform))
        {
          foreach (Face face in LayerDictionary.Keys.First<Solid>().Faces)
          {
            if (face is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(transform.BasisZ))
              num2 += planarFace.Area;
          }
          int num3 = (int) transaction.RollBack();
        }
        else
        {
          int num4 = (int) transaction.RollBack();
          return -1.0;
        }
      }
      else
      {
        int num5 = (int) transaction.Commit();
      }
      return num2;
    }
  }

  private string ConvertToFeetFractInches(string valueStr)
  {
    int num;
    string fraction = PDMExport.ToFraction(12.0 * ((double) (num = (int) Convert.ToDouble(valueStr)) - (double) num));
    return fraction.Equals("12") ? (num + 1).ToString() + "'-0\"" : $"{num.ToString()}'-{fraction}\"";
  }

  private static string ToFraction(double value)
  {
    int num1 = 8;
    int num2 = (int) value;
    int num3;
    for (num3 = (int) ((Math.Abs(value) - (double) Math.Abs(num2)) * (double) num1 + 0.5); num3 % 2 == 0 && num1 % 2 == 0; num1 /= 2)
      num3 /= 2;
    if (num1 > 1)
    {
      if (num2 != 0)
        return $"{num2} {num3}/{num1}";
      return value < 0.0 ? $"-{num3}/{num1}" : $"{num3}/{num1}";
    }
    return num3 == num1 ? (num2 + 1).ToString() : num2.ToString();
  }
}
