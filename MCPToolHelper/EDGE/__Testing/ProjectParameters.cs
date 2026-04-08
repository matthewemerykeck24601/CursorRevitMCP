// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.ProjectParameters
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.AdminUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing;

public class ProjectParameters
{
  private static List<ProjectParameters.ParameterInfo> AllProjectParameters = new List<ProjectParameters.ParameterInfo>()
  {
    new ProjectParameters.ParameterInfo("28_DAY_STRENGTH", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CHECKED_DATE_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("TICKET_FLAGGED", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CHECKED_DATE_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CHECKED_USER_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CHECKED_USER_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CONTROL_NO", "IDENTITY", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CREATE_DATE_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CREATE_DATE_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CREATE_USER_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_CREATE_USER_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_EDIT_COMMENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_LOCKED", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_LOCKED_DATE", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_LOCKED_USER", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_MARK_NUMBER", "IDENTITY", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_PROCESSED_DATE_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_PROCESSED_DATE_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_PROCESSED_USER_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_PROCESSED_USER_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_RELEASED_DATE_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_RELEASED_DATE_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_RELEASED_USER_CURRENT", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_RELEASED_USER_INITIAL", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_UNLOCKED_DATE", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("ASSEMBLY_UNLOCKED_USER", "TICKET_MANAGER", true, "Assemblies"),
    new ProjectParameters.ParameterInfo("BOM_PRODUCT_HOST", "IDENTITY", true, "AllModelComponents"),
    new ProjectParameters.ParameterInfo("CODE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("CONCRETE_TYPE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("CONSTRUCTION_PRODUCT", "IDENTITY", true, "StructuralFraming"),
    new ProjectParameters.ParameterInfo("CONSTRUCTION_PRODUCT_HOST", "IDENTITY", true, "AllModelComponents"),
    new ProjectParameters.ParameterInfo("AUTO_WARP_DISPLACEMENT", "IDENTITY", true, "AllModelComponents"),
    new ProjectParameters.ParameterInfo("IS_WARPABLE_SHAPE", "IDENTITY", true, "AllModelComponents"),
    new ProjectParameters.ParameterInfo("DS_DESC", "IDENTITY", true, "AllModelComponents"),
    new ProjectParameters.ParameterInfo("MANUFACTURE_COMPONENT", "IDENTITY", true, "Components"),
    new ProjectParameters.ParameterInfo("MILL_DATE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("PROD_ERECT_SEQUENCE", "IDENTITY", true, "AllModelComponents"),
    new ProjectParameters.ParameterInfo("PRODUCT", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("PROJECT_CLIENT_PRECAST_MANUFACTURER", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("TIF", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("BIF", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("SIF", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("INSULATION_VOLUME_TOLERANCE", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("INSULATION_GEOMETRY_TOLERANCE", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("AUTO_WARPING_CREATE_DIRECT_SHAPES", "PROJECT_GENERAL", true, "ProjectInformation"),
    new ProjectParameters.ParameterInfo("REBAR_GRADE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("STRIPPING_STRENGTH", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ADDONS", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_01", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_02", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_03", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_04", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_05", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_06", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_07", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_08", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_09", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_10", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_11", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_12", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_13", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_14", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_15", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_16", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_17", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_18", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_19", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_20", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_21", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_22", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_23", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_24", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_25", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_26", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_27", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_28", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_29", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_30", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_CONTROL_REQD_TOTAL", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_FINISH_BACK", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_FINISH_FACE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_FINISH_LF", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_FINISH_SF", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_BY", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_DATE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_DATE_REVISED", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_DESCRIPTION", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_01", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_01_BY", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_01_DATE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_01_DESCRIPTION", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_02", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_02_BY", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_02_DATE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_02_DESCRIPTION", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_03", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_03_BY", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_03_DATE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_03_DESCRIPTION", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_04", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_04_BY", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_04_DATE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ISSUE_NUMBER_04_DESCRIPTION", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_LENGTH", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_OTHER", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_REVISED", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_TOTAL_REQUIRED", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_WASH_BLAST", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_WEIGHT", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_HW_WEIGHT", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("HW_COMPONENT_WEIGHT", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_WT", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_WIDTH", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_DEPTH", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_PROD_CODE", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_EREC_SEQ", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_STRUCT_MIX", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ARCH_MIX_1", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ARCH_MIX_2", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ARCH_MIX_3", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_ARCH_MIX_4", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("TKT_TOP_FINISH", "TICKETING", true, "Sheets"),
    new ProjectParameters.ParameterInfo("WEIGHT_PER_UNIT", "MODEL_GENERAL", true, "WeightPerUnit"),
    new ProjectParameters.ParameterInfo("WWF_FY", "TICKETING", true, "Sheets")
  };
  private static List<string> MetromontParameterNames = new List<string>()
  {
    "TKT_ADDONS",
    "CODE",
    "TKT_CONTROL_01",
    "TKT_CONTROL_02",
    "TKT_CONTROL_03",
    "TKT_CONTROL_04",
    "TKT_CONTROL_05",
    "TKT_CONTROL_06",
    "TKT_CONTROL_07",
    "TKT_CONTROL_08",
    "TKT_CONTROL_09",
    "TKT_CONTROL_10",
    "TKT_CONTROL_11",
    "TKT_CONTROL_12",
    "TKT_CONTROL_13",
    "TKT_CONTROL_14",
    "TKT_CONTROL_15",
    "TKT_CONTROL_16",
    "TKT_CONTROL_17",
    "TKT_CONTROL_18",
    "TKT_CONTROL_19",
    "TKT_CONTROL_20",
    "TKT_CONTROL_21",
    "TKT_CONTROL_22",
    "TKT_CONTROL_23",
    "TKT_CONTROL_24",
    "TKT_CONTROL_25",
    "TKT_CONTROL_26",
    "TKT_CONTROL_27",
    "TKT_CONTROL_28",
    "TKT_CONTROL_29",
    "TKT_CONTROL_30",
    "TKT_CONTROL_REQD_TOTAL",
    "TKT_FINISH_BACK",
    "TKT_FINISH_FACE",
    "TKT_FINISH_LF",
    "TKT_FINISH_SF",
    "TKT_ISSUE_BY",
    "TKT_ISSUE_DATE",
    "TKT_ISSUE_DATE_REVISED",
    "TKT_ISSUE_DESCRIPTION",
    "TKT_ISSUE_NUMBER",
    "TKT_ISSUE_NUMBER_01",
    "TKT_ISSUE_NUMBER_01_BY",
    "TKT_ISSUE_NUMBER_01_DATE",
    "TKT_ISSUE_NUMBER_01_DESCRIPTION",
    "TKT_ISSUE_NUMBER_02",
    "TKT_ISSUE_NUMBER_02_BY",
    "TKT_ISSUE_NUMBER_02_DATE",
    "TKT_ISSUE_NUMBER_02_DESCRIPTION",
    "TKT_ISSUE_NUMBER_03",
    "TKT_ISSUE_NUMBER_03_BY",
    "TKT_ISSUE_NUMBER_03_DATE",
    "TKT_ISSUE_NUMBER_03_DESCRIPTION",
    "TKT_ISSUE_NUMBER_04",
    "TKT_ISSUE_NUMBER_04_BY",
    "TKT_ISSUE_NUMBER_04_DATE",
    "TKT_ISSUE_NUMBER_04_DESCRIPTION",
    "TKT_LENGTH",
    "TKT_OTHER",
    "TKT_REVISED",
    "TKT_TOTAL_REQUIRED",
    "TKT_WASH_BLAST",
    "TKT_WEIGHT",
    "TKT_WT",
    "TKT_WIDTH",
    "TKT_DEPTH",
    "TKT_PROD_CODE",
    "TKT_EREC_SEQ",
    "TKT_STRUCT_MIX",
    "TKT_ARCH_MIX_1",
    "TKT_ARCH_MIX_2",
    "TKT_ARCH_MIX_3",
    "TKT_ARCH_MIX_4",
    "TKT_TOP_FINISH"
  };
  private static readonly List<string> CoreslabParameterNames = new List<string>()
  {
    "28_DAY_STRENGTH",
    "CODE",
    "CONCRETE_TYPE",
    "MILL_DATE",
    "PRODUCT",
    "REBAR_GRADE",
    "STRIPPING_STRENGTH",
    "TKT_ISSUE_BY",
    "TKT_ISSUE_DATE",
    "TKT_ISSUE_DATE_REVISED",
    "TKT_ISSUE_DESCRIPTION",
    "TKT_ISSUE_NUMBER",
    "TKT_ISSUE_NUMBER_01",
    "TKT_ISSUE_NUMBER_01_BY",
    "TKT_ISSUE_NUMBER_01_DATE",
    "TKT_ISSUE_NUMBER_01_DESCRIPTION",
    "TKT_ISSUE_NUMBER_02",
    "TKT_ISSUE_NUMBER_02_BY",
    "TKT_ISSUE_NUMBER_02_DATE",
    "TKT_ISSUE_NUMBER_02_DESCRIPTION",
    "TKT_ISSUE_NUMBER_03",
    "TKT_ISSUE_NUMBER_03_BY",
    "TKT_ISSUE_NUMBER_03_DATE",
    "TKT_ISSUE_NUMBER_03_DESCRIPTION",
    "TKT_ISSUE_NUMBER_04",
    "TKT_ISSUE_NUMBER_04_BY",
    "TKT_ISSUE_NUMBER_04_DATE",
    "TKT_ISSUE_NUMBER_04_DESCRIPTION",
    "TKT_LENGTH",
    "TKT_OTHER",
    "TKT_REVISED",
    "TKT_TOTAL_REQUIRED",
    "TKT_WEIGHT",
    "TKT_WT",
    "TKT_WIDTH",
    "WWF_FY",
    "TKT_DEPTH",
    "TKT_PROD_CODE",
    "TKT_EREC_SEQ",
    "TKT_STRUCT_MIX",
    "TKT_ARCH_MIX_1",
    "TKT_ARCH_MIX_2",
    "TKT_ARCH_MIX_3",
    "TKT_ARCH_MIX_4",
    "TKT_TOP_FINISH"
  };
  private static readonly List<string> GateParameterNames = new List<string>()
  {
    "TKT_ADDONS",
    "28_DAY_STRENGTH",
    "CODE",
    "CONCRETE_TYPE",
    "MILL_DATE",
    "PRODUCT",
    "REBAR_GRADE",
    "STRIPPING_STRENGTH",
    "TKT_CONTROL_01",
    "TKT_CONTROL_02",
    "TKT_CONTROL_03",
    "TKT_CONTROL_04",
    "TKT_CONTROL_05",
    "TKT_CONTROL_06",
    "TKT_CONTROL_07",
    "TKT_CONTROL_08",
    "TKT_CONTROL_09",
    "TKT_CONTROL_10",
    "TKT_CONTROL_11",
    "TKT_CONTROL_12",
    "TKT_CONTROL_13",
    "TKT_CONTROL_14",
    "TKT_CONTROL_15",
    "TKT_CONTROL_16",
    "TKT_CONTROL_17",
    "TKT_CONTROL_18",
    "TKT_CONTROL_19",
    "TKT_CONTROL_20",
    "TKT_CONTROL_21",
    "TKT_CONTROL_22",
    "TKT_CONTROL_23",
    "TKT_CONTROL_24",
    "TKT_CONTROL_25",
    "TKT_CONTROL_26",
    "TKT_CONTROL_27",
    "TKT_CONTROL_28",
    "TKT_CONTROL_29",
    "TKT_CONTROL_30",
    "TKT_CONTROL_REQD_TOTAL",
    "TKT_FINISH_BACK",
    "TKT_FINISH_FACE",
    "TKT_FINISH_LF",
    "TKT_FINISH_SF",
    "TKT_ISSUE_BY",
    "TKT_ISSUE_DATE",
    "TKT_ISSUE_DATE_REVISED",
    "TKT_ISSUE_DESCRIPTION",
    "TKT_ISSUE_NUMBER",
    "TKT_ISSUE_NUMBER_01",
    "TKT_ISSUE_NUMBER_01_BY",
    "TKT_ISSUE_NUMBER_01_DATE",
    "TKT_ISSUE_NUMBER_01_DESCRIPTION",
    "TKT_ISSUE_NUMBER_02",
    "TKT_ISSUE_NUMBER_02_BY",
    "TKT_ISSUE_NUMBER_02_DATE",
    "TKT_ISSUE_NUMBER_02_DESCRIPTION",
    "TKT_ISSUE_NUMBER_03",
    "TKT_ISSUE_NUMBER_03_BY",
    "TKT_ISSUE_NUMBER_03_DATE",
    "TKT_ISSUE_NUMBER_03_DESCRIPTION",
    "TKT_ISSUE_NUMBER_04",
    "TKT_ISSUE_NUMBER_04_BY",
    "TKT_ISSUE_NUMBER_04_DATE",
    "TKT_ISSUE_NUMBER_04_DESCRIPTION",
    "TKT_LENGTH",
    "TKT_OTHER",
    "TKT_REVISED",
    "TKT_TOTAL_REQUIRED",
    "TKT_WASH_BLAST",
    "TKT_WEIGHT",
    "TKT_WT",
    "TKT_WIDTH",
    "WWF_FY",
    "TKT_DEPTH",
    "TKT_PROD_CODE",
    "TKT_EREC_SEQ",
    "TKT_STRUCT_MIX",
    "TKT_ARCH_MIX_1",
    "TKT_ARCH_MIX_2",
    "TKT_ARCH_MIX_3",
    "TKT_ARCH_MIX_4",
    "TKT_TOP_FINISH"
  };
  private static readonly List<string> UnistressParameterNames = new List<string>();
  private static readonly List<string> TindallParameterNames = new List<string>();
  private static readonly List<string> WellsParameterNames = new List<string>();
  private static readonly List<string> AllParameterNames = ProjectParameters.AllProjectParameters.Select<ProjectParameters.ParameterInfo, string>((Func<ProjectParameters.ParameterInfo, string>) (param => param.ParamName)).ToList<string>();

  public static void AddProjectParameters(string precastManufacturer)
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    Document document = uiDoc.Document;
    Application application = uiDoc.Application.Application;
    string sharedParametersPath = EdgeBuildInformation.GetSharedParametersPath();
    List<string> paramNamesToAdd = new List<string>();
    if (precastManufacturer.Contains("METROMONT"))
      paramNamesToAdd = ProjectParameters.MetromontParameterNames;
    else if (precastManufacturer.Contains("CORESLAB"))
      paramNamesToAdd = ProjectParameters.CoreslabParameterNames;
    else if (precastManufacturer.Contains("GATE"))
      paramNamesToAdd = ProjectParameters.GateParameterNames;
    else if (precastManufacturer.Contains("UNISTRESS"))
      paramNamesToAdd = ProjectParameters.UnistressParameterNames;
    else if (precastManufacturer.Contains("TINDALL"))
      paramNamesToAdd = ProjectParameters.TindallParameterNames;
    else if (precastManufacturer.Contains("WELLS"))
    {
      paramNamesToAdd = ProjectParameters.WellsParameterNames;
    }
    else
    {
      TaskDialog.Show("Default Manufacturer", "The Manufacturer name provided cannot be found within EDGE.  All project parameters will be added to the project.");
      paramNamesToAdd = ProjectParameters.AllParameterNames;
    }
    List<ProjectParameters.ParameterInfo> list1 = ProjectParameters.AllProjectParameters.Where<ProjectParameters.ParameterInfo>((Func<ProjectParameters.ParameterInfo, bool>) (param => paramNamesToAdd.Contains(param.ParamName))).ToList<ProjectParameters.ParameterInfo>();
    List<string> stringList = new List<string>();
    DefinitionBindingMapIterator bindingMapIterator = document.ParameterBindings.ForwardIterator();
    while (bindingMapIterator.MoveNext())
      stringList.Add(bindingMapIterator.Key.Name);
    foreach (string str in stringList)
    {
      string paramName = str;
      ProjectParameters.ParameterInfo parameterInfo1 = list1.FirstOrDefault<ProjectParameters.ParameterInfo>((Func<ProjectParameters.ParameterInfo, bool>) (parameterInfo => parameterInfo.ParamName.Equals(paramName)));
      if (parameterInfo1 != null)
        parameterInfo1.ParamExists = true;
    }
    List<ProjectParameters.ParameterInfo> list2 = list1.Where<ProjectParameters.ParameterInfo>((Func<ProjectParameters.ParameterInfo, bool>) (param => !param.ParamExists)).ToList<ProjectParameters.ParameterInfo>();
    application.SharedParametersFilename = sharedParametersPath;
    DefinitionGroups groups = application.OpenSharedParameterFile().Groups;
    foreach (ProjectParameters.ParameterInfo parameterInfo in list2)
      parameterInfo.ParamDefinition = ProjectParameters.GetDefinition(groups, parameterInfo.ParamGroup, parameterInfo.ParamName);
    List<ProjectParameters.ParameterInfo> wrongParams = list2.Where<ProjectParameters.ParameterInfo>((Func<ProjectParameters.ParameterInfo, bool>) (param => param.ParamDefinition == null)).ToList<ProjectParameters.ParameterInfo>();
    TaskDialog.Show("Error Generating Parameter Names", wrongParams.Aggregate<ProjectParameters.ParameterInfo, string>("", (Func<string, ProjectParameters.ParameterInfo, string>) ((current, p) => current + p.ParamName + Environment.NewLine)));
    foreach (ProjectParameters.ParameterInfo parameterInfo in list2.Where<ProjectParameters.ParameterInfo>((Func<ProjectParameters.ParameterInfo, bool>) (param => !wrongParams.Contains(param))).ToList<ProjectParameters.ParameterInfo>())
      ProjectParameters.AddParameterBinding((Definition) parameterInfo.ParamDefinition, parameterInfo.IsInstance, parameterInfo.ParamSet);
  }

  public static void AddParameterBinding(Definition def, bool isInstance, string set)
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    ActiveModel.GetInformation(uiDoc);
    Document document = uiDoc.Document;
    Application application = uiDoc.Application.Application;
    string parametersFilename = application.SharedParametersFilename;
    ProjectInfo projectInformation = document.ProjectInformation;
    Category category1 = document.Settings.Categories.get_Item(BuiltInCategory.OST_Sheets);
    Category category2 = document.Settings.Categories.get_Item(BuiltInCategory.OST_ProjectInformation);
    Category category3 = document.Settings.Categories.get_Item(BuiltInCategory.OST_GenericModel);
    Category category4 = document.Settings.Categories.get_Item(BuiltInCategory.OST_SpecialityEquipment);
    Category category5 = document.Settings.Categories.get_Item(BuiltInCategory.OST_StructConnections);
    Category category6 = document.Settings.Categories.get_Item(BuiltInCategory.OST_StructuralFraming);
    Category category7 = document.Settings.Categories.get_Item(BuiltInCategory.OST_Assemblies);
    CategorySet categorySet1 = application.Create.NewCategorySet();
    categorySet1.Insert(category1);
    InstanceBinding instanceBinding1 = document.Application.Create.NewInstanceBinding(categorySet1);
    TypeBinding typeBinding1 = document.Application.Create.NewTypeBinding(categorySet1);
    CategorySet categorySet2 = application.Create.NewCategorySet();
    categorySet2.Insert(category3);
    categorySet2.Insert(category4);
    categorySet2.Insert(category5);
    categorySet2.Insert(category7);
    InstanceBinding instanceBinding2 = document.Application.Create.NewInstanceBinding(categorySet2);
    TypeBinding typeBinding2 = document.Application.Create.NewTypeBinding(categorySet2);
    CategorySet categorySet3 = application.Create.NewCategorySet();
    categorySet3.Insert(category2);
    InstanceBinding instanceBinding3 = document.Application.Create.NewInstanceBinding(categorySet3);
    TypeBinding typeBinding3 = document.Application.Create.NewTypeBinding(categorySet3);
    CategorySet categorySet4 = application.Create.NewCategorySet();
    categorySet4.Insert(category3);
    categorySet4.Insert(category4);
    categorySet4.Insert(category5);
    categorySet4.Insert(category6);
    InstanceBinding instanceBinding4 = document.Application.Create.NewInstanceBinding(categorySet4);
    TypeBinding typeBinding4 = document.Application.Create.NewTypeBinding(categorySet4);
    CategorySet categorySet5 = application.Create.NewCategorySet();
    categorySet5.Insert(category3);
    categorySet5.Insert(category6);
    InstanceBinding instanceBinding5 = document.Application.Create.NewInstanceBinding(categorySet5);
    TypeBinding typeBinding5 = document.Application.Create.NewTypeBinding(categorySet5);
    CategorySet categorySet6 = application.Create.NewCategorySet();
    categorySet6.Insert(category7);
    InstanceBinding instanceBinding6 = document.Application.Create.NewInstanceBinding(categorySet6);
    TypeBinding typeBinding6 = document.Application.Create.NewTypeBinding(categorySet6);
    CategorySet categorySet7 = application.Create.NewCategorySet();
    categorySet7.Insert(category7);
    categorySet7.Insert(category6);
    categorySet7.Insert(category3);
    InstanceBinding instanceBinding7 = document.Application.Create.NewInstanceBinding(categorySet7);
    TypeBinding typeBinding7 = document.Application.Create.NewTypeBinding(categorySet7);
    CategorySet categorySet8 = application.Create.NewCategorySet();
    categorySet8.Insert(category3);
    categorySet8.Insert(category4);
    categorySet8.Insert(category5);
    categorySet8.Insert(category6);
    categorySet8.Insert(category7);
    InstanceBinding instanceBinding8 = document.Application.Create.NewInstanceBinding(categorySet8);
    TypeBinding typeBinding8 = document.Application.Create.NewTypeBinding(categorySet8);
    BindingMap parameterBindings = uiDoc.Document.ParameterBindings;
    if (isInstance)
    {
      if (set.Equals("Sheets"))
        parameterBindings.Insert(def, (Binding) instanceBinding1, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("Components"))
        parameterBindings.Insert(def, (Binding) instanceBinding2, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("AllModelComponents"))
        parameterBindings.Insert(def, (Binding) instanceBinding4, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("ProjectInformation"))
        parameterBindings.Insert(def, (Binding) instanceBinding3, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("StructuralFraming"))
        parameterBindings.Insert(def, (Binding) instanceBinding5, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("Assemblies"))
        parameterBindings.Insert(def, (Binding) instanceBinding6, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("AssembliesStructuralFraming"))
        parameterBindings.Insert(def, (Binding) instanceBinding7, BuiltInParameterGroup.PG_DATA);
      if (!set.Equals("WeightPerUnit"))
        return;
      parameterBindings.Insert(def, (Binding) instanceBinding8, BuiltInParameterGroup.PG_DATA);
    }
    else
    {
      if (set.Equals("Sheets"))
        parameterBindings.Insert(def, (Binding) typeBinding1, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("Components"))
        parameterBindings.Insert(def, (Binding) typeBinding2, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("AllModelComponents"))
        parameterBindings.Insert(def, (Binding) typeBinding4, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("ProjectInformation"))
        parameterBindings.Insert(def, (Binding) typeBinding3, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("StructuralFraming"))
        parameterBindings.Insert(def, (Binding) typeBinding5, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("Assemblies"))
        parameterBindings.Insert(def, (Binding) typeBinding6, BuiltInParameterGroup.PG_DATA);
      if (set.Equals("AssembliesStructuralFraming"))
        parameterBindings.Insert(def, (Binding) typeBinding7, BuiltInParameterGroup.PG_DATA);
      if (!set.Equals("WeightPerUnit"))
        return;
      parameterBindings.Insert(def, (Binding) typeBinding8, BuiltInParameterGroup.PG_DATA);
    }
  }

  public static void SetProjectInformationManufacturerName(string precastManufacturer)
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    ActiveModel.GetInformation(uiDoc);
    Document document = uiDoc.Document;
    if (document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER") == null)
      TaskDialog.Show("Missing Parameter", "Error - The Project Parameter PROJECT_CLIENT_PRECAST_MANUFACTURER was not found.  Therefore it's value cannot set.");
    else
      document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER").Set(precastManufacturer);
  }

  private static ExternalDefinition GetDefinition(
    DefinitionGroups defGroups,
    string groupName,
    string paramName)
  {
    return defGroups.get_Item(groupName).Definitions.get_Item(paramName) as ExternalDefinition;
  }

  public class ParameterInfo
  {
    public string ParamName { get; set; }

    public string ParamGroup { get; set; }

    public bool IsInstance { get; set; }

    public string ParamSet { get; set; }

    public bool ParamExists { get; set; }

    public ExternalDefinition ParamDefinition { get; set; }

    public ParameterInfo(string paramName, string paramGroup, bool isInstance, string paramSet)
    {
      this.ParamName = paramName;
      this.ParamGroup = paramGroup;
      this.IsInstance = isInstance;
      this.ParamSet = paramSet;
    }
  }
}
