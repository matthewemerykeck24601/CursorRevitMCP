// Decompiled with JetBrains decompiler
// Type: EDGE.EdgePrefsReader
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.ApplicationRibbon;
using System;
using System.IO;
using Utils;

#nullable disable
namespace EDGE;

internal class EdgePrefsReader
{
  public static void ReadEdgePrefsFile()
  {
    string path = !QAUtils.IsJournalPlayback ? "C:\\EDGEforRevit\\EDGE_Preferences.txt" : App.JournalPlaybackDirectoryPath + "\\EDGE_Preferences.txt";
    if (!File.Exists(path))
      return;
    try
    {
      StreamReader streamReader = new StreamReader(path);
      string str1;
      while ((str1 = streamReader.ReadLine()) != null)
      {
        if (!string.IsNullOrWhiteSpace(str1))
        {
          string[] strArray = str1.Split(new string[1]
          {
            ","
          }, StringSplitOptions.RemoveEmptyEntries);
          string str2 = strArray[0];
          if (str2 != null)
          {
            switch (str2.Length)
            {
              case 12:
                if (str2 == "*PTAC_Tools*")
                {
                  AppRibbonSetup.PTACTools = !strArray[1].Equals("FALSE");
                  continue;
                }
                continue;
              case 13:
                switch (str2[1])
                {
                  case 'A':
                    if (str2 == "*Admin_Panel*")
                    {
                      AppRibbonSetup.RibbonAdminPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'E':
                    if (str2 == "*EDGEBrowser*")
                    {
                      if (strArray[1].Equals("FALSE"))
                      {
                        AppRibbonSetup.RibbonAdminEdgeBrowser = false;
                        App.DockablePane = false;
                        continue;
                      }
                      AppRibbonSetup.RibbonAdminEdgeBrowser = true;
                      App.DockablePane = true;
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 15:
                switch (str2[1])
                {
                  case 'E':
                    if (str2 == "*Edge_Cloud_Id*")
                    {
                      if (strArray.Length < 2)
                      {
                        App.Edge_Cloud_Id = -1;
                        continue;
                      }
                      int result = -1;
                      if (int.TryParse(strArray[1], out result))
                      {
                        App.Edge_Cloud_Id = result;
                        continue;
                      }
                      continue;
                    }
                    continue;
                  case 'L':
                    if (str2 == "*LogRebarTrace*" && strArray[1].Equals("TRUE"))
                    {
                      QA.SetTraceLogging(true);
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 16 /*0x10*/:
                switch (str2[2])
                {
                  case 'd':
                    if (str2 == "*Admin_PEExport*")
                    {
                      AppRibbonSetup.RibbonAdminShowExcelExport = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'e':
                    if (str2 == "*Geometry_Panel*")
                    {
                      AppRibbonSetup.RibbonGeometryPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 's':
                    if (str2 == "*Assembly_Panel*")
                    {
                      AppRibbonSetup.RibbonAssemblyPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 17:
                switch (str2[7])
                {
                  case 'C':
                    if (str2 == "*Admin_CAMExport*")
                    {
                      AppRibbonSetup.RibbonAdminShowCamExport = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'P':
                    if (str2 == "*Admin_PDMExport*")
                    {
                      AppRibbonSetup.RibbonAdminShowPDMExport = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'o':
                    if (str2 == "*HWBOMFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.HWBOMFolderPath = strArray[1].Trim();
                      if (App.HWBOMFolderPath.Substring(App.HWBOMFolderPath.Length - 1).Contains("\\"))
                        App.HWBOMFolderPath = App.HWBOMFolderPath.Remove(App.HWBOMFolderPath.Length - 1);
                      QA.LogLine("Hardware BOM Settings Folder Path: " + App.HWBOMFolderPath);
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 18:
                switch (str2[5])
                {
                  case 'b':
                    if (str2 == "*Visibility_Panel*")
                    {
                      AppRibbonSetup.RibbonVisibilityPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'd':
                    if (str2 == "*Scheduling_Panel*")
                    {
                      AppRibbonSetup.RibbonSchedulingPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'l':
                    if (str2 == "*Insulation_Panel*")
                    {
                      AppRibbonSetup.RibbonInsulationPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'n':
                    if (str2 == "*Admin_EDGEBrowser")
                    {
                      if (strArray[1].Equals("FALSE"))
                      {
                        AppRibbonSetup.RibbonAdminEdgeBrowser = false;
                        App.DockablePane = false;
                        continue;
                      }
                      AppRibbonSetup.RibbonAdminEdgeBrowser = true;
                      App.DockablePane = true;
                      continue;
                    }
                    continue;
                  case 't':
                    if (str2 == "*Annotation_Panel*")
                    {
                      AppRibbonSetup.RibbonAnnotationPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 19:
                switch (str2[1])
                {
                  case 'F':
                    if (str2 == "*FamilyTools_Panel*")
                    {
                      AppRibbonSetup.RibbonFamilyToolsPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'H':
                    if (str2 == "*HWTBPopFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.HWTBPopFolderPath = strArray[1].Trim();
                      if (App.HWTBPopFolderPath.Substring(App.HWTBPopFolderPath.Length - 1).Contains("\\"))
                        App.HWTBPopFolderPath = App.HWTBPopFolderPath.Remove(App.HWTBPopFolderPath.Length - 1);
                      QA.LogLine("Hardware Titleblock Populator Path: " + App.HWTBPopFolderPath);
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTools_Panel*")
                    {
                      AppRibbonSetup.RibbonTicketToolsPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 20:
                switch (str2[3])
                {
                  case 'g':
                    if (str2 == "*Edge_Model_Locking*")
                    {
                      App.ModelLocking = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'm':
                    if (str2 == "*Admin_TicketExport*")
                    {
                      AppRibbonSetup.RibbonTicketManagerExport = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 's':
                    if (str2 == "*Assembly_TopAsCast*")
                    {
                      AppRibbonSetup.RibbonAssemblyTopAsCast = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 21:
                switch (str2[7])
                {
                  case 'C':
                    if (str2 == "*PushToCloudDemoSite*" && strArray[1].Equals("TRUE"))
                    {
                      QA.PushToDemoSiteAnyway = true;
                      continue;
                    }
                    continue;
                  case 'M':
                    if (str2 == "*TicketManager_Panel*")
                    {
                      AppRibbonSetup.RibbonTicketManagerPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTools_AutoDim*")
                    {
                      AppRibbonSetup.RibbonTicketToolsAutoDim = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'r':
                    if (str2 == "*HardwareTools_Panel*")
                    {
                      AppRibbonSetup.RibbonHardwareToolsPanel = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 22:
                switch (str2[10])
                {
                  case 'A':
                    if (str2 == "*Geometry_AutoWarping*")
                    {
                      AppRibbonSetup.RibbonGeometryAutoWarping = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'G':
                    if (str2 == "*Geometry_GetCentroid*")
                    {
                      AppRibbonSetup.RibbonGeometryGetCentroid = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 't':
                    if (str2 == "*Edge_CountMultiplier*" && strArray[1].Equals("FALSE"))
                    {
                      App.ProcessCountMultiplierOnDocumentOpen = false;
                      continue;
                    }
                    continue;
                  case 'x':
                    if (str2 == "*MarkPrefixFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.MarkPrefixFolderPath = strArray[1].Trim();
                      if (App.MarkPrefixFolderPath.Substring(App.MarkPrefixFolderPath.Length - 1).Contains("\\"))
                        App.MarkPrefixFolderPath = App.MarkPrefixFolderPath.Remove(App.MarkPrefixFolderPath.Length - 1);
                      QA.LogLine("Mark Prefix Folder Path: " + App.MarkPrefixFolderPath);
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 23:
                switch (str2[1])
                {
                  case 'A':
                    if (str2 == "*Admin_WarningAnalyzer*")
                    {
                      AppRibbonSetup.RibbonAdminShowWarningAnalyzer = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'G':
                    if (str2 == "*Geometry_MarkOppTools*")
                    {
                      AppRibbonSetup.RibbonGeometryMarkOppTools = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'S':
                    if (str2 == "*SpecialRebarScenario1*")
                    {
                      App.SpecialRebarScenario1 = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 24:
                switch (str2[1])
                {
                  case 'A':
                    if (str2 == "*Admin_TicketExportFile*" && AppRibbonSetup.RibbonTicketManagerExport)
                    {
                      App.TicketManagerExportFile = !strArray[1].Contains(".") || !(strArray[1].ToUpper().Substring(strArray[1].Length - 3) != "CSV") ? (strArray[1].Contains(".") ? strArray[1] : strArray[1] + ".csv") : strArray[1].Substring(0, strArray[1].IndexOf(".")) + ".csv";
                      continue;
                    }
                    continue;
                  case 'S':
                    if (str2 == "*ShowSendToCloudOnAdmin*" && strArray[1].Equals("TRUE"))
                    {
                      QA.ShowSendToCloudOnAdmin = true;
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTools_AutoTicket*")
                    {
                      AppRibbonSetup.RibbonTicketToolsAutoTicketGeneration = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 25:
                switch (str2[13])
                {
                  case 'B':
                    if (str2 == "*Admin_TicketBOMSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsShowTicketBOMSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'C':
                    if (str2 == "*TicketTools_CloneTicket*")
                    {
                      AppRibbonSetup.RibbonTicketToolsCloneTicket = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'L':
                    if (str2 == "*TicketTools_LaserExport*")
                    {
                      AppRibbonSetup.RibbonTicketToolsLaserExport = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'a':
                    if (str2 == "*Edge_Rebar_Mark_Updater*")
                    {
                      App.RebarMarkAutomation = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'c':
                    if (str2 == "*Assembly_MatchTopAsCast*")
                    {
                      AppRibbonSetup.RibbonAssemblyMatchTopAsCast = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 's':
                    if (str2 == "*RebarSettingsFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.RebarSettingsFolderPath = strArray[1].Trim();
                      if (App.RebarSettingsFolderPath.Substring(App.RebarSettingsFolderPath.Length - 1).Contains("\\"))
                        App.RebarSettingsFolderPath = App.RebarSettingsFolderPath.Remove(App.RebarSettingsFolderPath.Length - 1);
                      QA.LogLine("Rebar Settings Folder Path: " + App.RebarSettingsFolderPath);
                      continue;
                    }
                    continue;
                  case 't':
                    if (str2 == "*Geometry_MultiVoidTools*")
                    {
                      AppRibbonSetup.RibbonGeometryMultiVoidTools = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'u':
                    if (str2 == "*Annotation_AutoDim_EDWG*")
                    {
                      AppRibbonSetup.RibbonAnnotationToolsEDrawingAutoDim = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 26:
                switch (str2[2])
                {
                  case 'i':
                    if (str2 == "*TicketSettingsFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.AutoTicketFolderPath = strArray[1].Trim();
                      if (App.AutoTicketFolderPath.Substring(App.AutoTicketFolderPath.Length - 1).Contains("\\"))
                        App.AutoTicketFolderPath = App.AutoTicketFolderPath.Remove(App.AutoTicketFolderPath.Length - 1);
                      QA.LogLine("Ticket Settings Folder Path: " + App.AutoTicketFolderPath);
                      continue;
                    }
                    continue;
                  case 'n':
                    if (str2 == "*Annotation_TextUtilities*")
                    {
                      AppRibbonSetup.RibbonAnnotationTextUtilities = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 's':
                    if (str2 == "*Assembly_CopyReinforcing*")
                    {
                      AppRibbonSetup.RibbonAssemblyCopyReinforcing = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 27:
                switch (str2[21])
                {
                  case 'G':
                    if (str2 == "*SelectionAndPinning_Grids*")
                    {
                      AppRibbonSetup.RibbonSelectionAndPinningGrids = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'P':
                    if (str2 == "*SelectionAndPinning_Panel*")
                    {
                      AppRibbonSetup.RibbonSelectionAndPinningPanel = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'a':
                    if (str2 == "*Insulation_CopyInsulation*")
                    {
                      AppRibbonSetup.RibbonInsulationCopyInsulation = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'd':
                    if (str2 == "*Admin_Bulk_Family_Updater*")
                    {
                      AppRibbonSetup.RibbonAdminShowBulkFamilyUpdater = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'e':
                    if (str2 == "*TicketTools_HrdwareDetail*")
                    {
                      AppRibbonSetup.RibbonHardwareToolsHardwareDetail = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'o':
                    if (str2 == "*Assembly_MarkAsReinforced*")
                    {
                      AppRibbonSetup.RibbonAssemblyMarkAsReinforced = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 28:
                switch (str2[1])
                {
                  case 'G':
                    if (str2 == "*Geometry_NewReferencePoint*")
                    {
                      AppRibbonSetup.RibbonGeometryNewReferencePoint = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'L':
                    if (str2 == "*LogVerboseRebarInformation*" && strArray[1].Equals("TRUE"))
                    {
                      QA.SetVerboseRebarLogging(true);
                      continue;
                    }
                    continue;
                  case 'S':
                    if (str2 == "*SelectionAndPinning_Levels*")
                    {
                      AppRibbonSetup.RibbonSelectionAndPinningLevels = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTemplateSettingsFile*" && !strArray[1].Equals(""))
                    {
                      App.TicketTemplateSettingsPath = strArray[1];
                      continue;
                    }
                    continue;
                  case 'U':
                    if (str2 == "*UserSettings_RebarSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsRebarSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 29:
                switch (str2[16 /*0x10*/])
                {
                  case 'P':
                    if (str2 == "*Insulation_AutoPinPlacement*")
                    {
                      AppRibbonSetup.RibbonInsulationPinPlacement = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'S':
                    if (str2 == "*HardwareDetailsSettingsFile*" && !strArray[1].Equals(""))
                    {
                      App.HardwareDetailTemplateSettingsPath = strArray[1];
                      continue;
                    }
                    continue;
                  case 'g':
                    switch (str2)
                    {
                      case "*TicketBOMSettingsFolderPath*":
                        if (!strArray[1].Equals(""))
                        {
                          App.TKTBOMFolderPath = strArray[1].Trim();
                          if (App.TKTBOMFolderPath.Substring(App.TKTBOMFolderPath.Length - 1).Contains("\\"))
                            App.TKTBOMFolderPath = App.TKTBOMFolderPath.Remove(App.TKTBOMFolderPath.Length - 1);
                          QA.LogLine("Ticket BOM Settings Folder Path: " + App.TKTBOMFolderPath);
                          continue;
                        }
                        continue;
                      case "*CADExportSettingsFolderPath*":
                        if (!strArray[1].Equals(""))
                        {
                          App.CADExFolderPath = strArray[1].Trim();
                          if (App.CADExFolderPath.Substring(App.CADExFolderPath.Length - 1).Contains("\\"))
                          {
                            App.CADExFolderPath = App.CADExFolderPath.Remove(App.CADExFolderPath.Length - 1);
                            continue;
                          }
                          continue;
                        }
                        continue;
                      default:
                        continue;
                    }
                  case 'k':
                    if (str2 == "*TicketTools_TicketPopulator*")
                    {
                      AppRibbonSetup.RibbonTicketToolsTicketPopulator = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'l':
                    if (str2 == "*Insulation_InsulationExport*")
                    {
                      AppRibbonSetup.RibbonInsulationExport = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'p':
                    if (str2 == "*Admin_TicketTemplateCreator*")
                    {
                      AppRibbonSetup.RibbonAdminShowTicketTemplateCreator = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 's':
                    if (str2 == "*Geometry_VoidHostingUpdater*")
                    {
                      AppRibbonSetup.RibbonGeometryVoidHostingUpdater = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 't':
                    if (str2 == "*Scheduling_ErectionSequence*")
                    {
                      AppRibbonSetup.RibbonSchedulingErectionSequence = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'y':
                    if (str2 == "*TicketTools_CopyTicketViews*")
                    {
                      AppRibbonSetup.RibbonTicketToolsCopyTicketViews = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 30:
                switch (str2[9])
                {
                  case 'T':
                    if (str2 == "*HardwareTools_HardwareDetail*")
                    {
                      AppRibbonSetup.RibbonHardwareToolsHardwareDetail = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case '_':
                    if (str2 == "*Geometry_AddonHostingUpdater*")
                    {
                      AppRibbonSetup.RibbonGeometryAddonHostingUpdater = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'a':
                    if (str2 == "*Admin_SharedParameterUpdater*")
                    {
                      AppRibbonSetup.RibbonAdminShowSharedParams = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'b':
                    if (str2 == "*Admin_EmbedClashVerification*")
                    {
                      AppRibbonSetup.RibbonAdminShowEmbedClashVerification = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'n':
                    if (str2 == "*Scheduling_BomProductHosting*")
                    {
                      AppRibbonSetup.RibbonSchedulingBomProductHosting = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'o':
                    switch (str2)
                    {
                      case "*Insulation_InsulationRemoval*":
                        AppRibbonSetup.RibbonInsulationRemoval = !strArray[1].Equals("FALSE");
                        continue;
                      case "*Insulation_InsulationMarking*":
                        AppRibbonSetup.RibbonInsulationMarking = !strArray[1].Equals("FALSE");
                        continue;
                      default:
                        continue;
                    }
                  default:
                    continue;
                }
              case 31 /*0x1F*/:
                switch (str2[12])
                {
                  case 'D':
                    if (str2 == "*Annotation_DimensionUtilities*")
                    {
                      AppRibbonSetup.RibbonAnnotationDimensionUtilities = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'F':
                    if (str2 == "*Annotation_Freeze_Active_View*")
                    {
                      AppRibbonSetup.RibbonAnnotationFreezeActiveView = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'M':
                    if (str2 == "*Scheduling_MarkRebarByProduct*")
                    {
                      AppRibbonSetup.RibbonSchedulingMarkRebarByProduct = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case '_':
                    if (str2 == "*TicketTools_AnnotateEmbedLong*")
                    {
                      AppRibbonSetup.RibbonTicketToolsAnnotateEmbedLong = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 32 /*0x20*/:
                switch (str2[14])
                {
                  case 'C':
                    if (str2 == "*UserSettings_CADExportSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsShowCADExportSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*UserSettings_TicketBOMSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsShowTicketBOMSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'e':
                    if (str2 == "*Geometry_WarpedParameterCopier*")
                    {
                      AppRibbonSetup.RibbonGeometryWarpedParameterCopier = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'i':
                    if (str2 == "*TicketTools_FindReferringViews*")
                    {
                      AppRibbonSetup.RibbonTicketToolsFindReferringViews = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'n':
                    if (str2 == "*TicketTools_AnnotateEmbedShort*")
                    {
                      AppRibbonSetup.RibbonTicketToolsAnnotateEmbedShort = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 't':
                    if (str2 == "*Assembly_CreationUpdatingRevit*")
                    {
                      AppRibbonSetup.RibbonAssemblyCreationUpdatingRevit = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 33:
                if (str2 == "*TitleBlockPopSettingsFolderPath*" && !strArray[1].Equals(""))
                {
                  App.TBPopFolderPath = strArray[1].Trim();
                  if (App.TBPopFolderPath.Substring(App.TBPopFolderPath.Length - 1).Contains("\\"))
                    App.TBPopFolderPath = App.TBPopFolderPath.Remove(App.TBPopFolderPath.Length - 1);
                  QA.LogLine("Titleblock Populator Settings Folder Path: " + App.TBPopFolderPath);
                  continue;
                }
                continue;
              case 34:
                switch (str2[1])
                {
                  case 'A':
                    if (str2 == "*Admin_BphCphParameterErrorFinder*")
                    {
                      AppRibbonSetup.RibbonAdminShowBPHCPHErrorFinder = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'G':
                    if (str2 == "*Geometry_MarkVerificationInitial*")
                    {
                      AppRibbonSetup.RibbonGeometryMarkVerificationInitial = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'I':
                    if (str2 == "*Insulation_InsulationDrawingMark*")
                    {
                      AppRibbonSetup.RibbonInsulationDrawing = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'M':
                    if (str2 == "*ModelLockingSuperadminFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.MLFolderPath = strArray[1].Trim();
                      if (App.MLFolderPath.Substring(App.MLFolderPath.Length - 1).Contains("\\"))
                        App.MLFolderPath = App.MLFolderPath.Remove(App.MLFolderPath.Length - 1);
                      QA.LogLine("Model Locking Producer Admin Folder Path: " + App.MLFolderPath);
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTools_BatchTicketPopulator*")
                    {
                      AppRibbonSetup.RibbonTicketToolsBatchTicketPopulator = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 35:
                switch (str2[2])
                {
                  case 'c':
                    if (str2 == "*Scheduling_ScheduleSequenceByView*")
                    {
                      AppRibbonSetup.RibbonSchedulingScheduleSequenceByView = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'd':
                    if (str2 == "*Admin_TitleBlockPopulatorSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsTBPSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'e':
                    if (str2 == "*Geometry_MarkVerificationExisting*")
                    {
                      AppRibbonSetup.RibbonGeometryMarkVerificationExisting = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'i':
                    if (str2 == "*TicketTools_CopyTicketAnnotations*")
                    {
                      AppRibbonSetup.RibbonTicketToolsCopyTicketAnnotations = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 's':
                    if (str2 == "*Assembly_CreationUpdatingRenaming*")
                    {
                      AppRibbonSetup.RibbonAssemblyCreationUpdatingRenaming = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 36:
                switch (str2[12])
                {
                  case 'A':
                    if (str2 == "*Insulation_AutoInsulationPlacement*")
                    {
                      AppRibbonSetup.RibbonInsulationAutoPlacement = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'I':
                    if (str2 == "*Insulation_InsulationDrawingMaster*")
                    {
                      AppRibbonSetup.RibbonInsulationDrawingMaster = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case '_':
                    if (str2 == "*TicketTools_ControlNumberPopulator*")
                    {
                      AppRibbonSetup.RibbonTicketToolsControlNumberPopulator = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 37:
                switch (str2[12])
                {
                  case 'C':
                    if (str2 == "*Scheduling_ControlNumberIncrementor*")
                    {
                      AppRibbonSetup.RibbonSchedulingControlNumberIncrementor = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'I':
                    if (str2 == "*Insulation_InsulationMarkingPerWall*")
                    {
                      AppRibbonSetup.RibbonInsulationMarkingPerWall = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'r':
                    if (str2 == "*InsulationDrawingSettingsFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.InsulationDrawingPath = strArray[1].Trim();
                      if (App.InsulationDrawingPath.Substring(App.InsulationDrawingPath.Length - 1).Contains("\\"))
                      {
                        App.InsulationDrawingPath = App.InsulationDrawingPath.Remove(App.InsulationDrawingPath.Length - 1);
                        continue;
                      }
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 38:
                switch (str2[12])
                {
                  case 'I':
                    if (str2 == "*Insulation_InsulationDrawingAssembly*")
                    {
                      AppRibbonSetup.RibbonInsulationDrawingPerPiece = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'M':
                    if (str2 == "*Insulation_ManualInsulationPlacement*")
                    {
                      AppRibbonSetup.RibbonInsulationManualPlacement = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'd':
                    if (str2 == "*SelectionAndPinning_SelectSFElements*")
                    {
                      AppRibbonSetup.RibbonSelectionAndPinningSelectSfElements = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'o':
                    if (str2 == "*Edge_ModelLocking_WorksharingSupport*" && strArray[1].Equals("FALSE"))
                    {
                      App.ModelLocking_WorksharingSupport = false;
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 39:
                switch (str2[7])
                {
                  case 'M':
                    if (str2 == "*TicketManagerCustomSettingsFolderPath*" && !strArray[1].Equals(""))
                    {
                      App.TMCFolderPath = strArray[1].Trim();
                      if (App.TMCFolderPath.Substring(App.TMCFolderPath.Length - 1).Contains("\\"))
                        App.TMCFolderPath = App.TMCFolderPath.Remove(App.TMCFolderPath.Length - 1);
                      QA.LogLine("Ticket  Manager Customization Settings Folder Path: " + App.TMCFolderPath);
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTools_TicketTitleBlockPopulator*")
                    {
                      AppRibbonSetup.RibbonTicketToolsTicketTitleBlockPopulator = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'l':
                    if (str2 == "*Scheduling_ConstructionProductHosting*")
                    {
                      AppRibbonSetup.RibbonSchedulingConstructionProductHosting = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 't':
                    if (str2 == "*UserSettings_MarkVerificationSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsMarkVerificationSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 40:
                if (str2 == "*UserSettings_InsulationDrawingSettings*")
                {
                  AppRibbonSetup.RibbonUserSettingsShowInsulationDrawingSettings = !strArray[1].Equals("FALSE");
                  continue;
                }
                continue;
              case 41:
                switch (str2[1])
                {
                  case 'E':
                    if (str2 == "*Edge_RebarAutomation_WorksharingSupport*")
                    {
                      App.RebarMarkAutomation_WorksharingSupport = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'H':
                    if (str2 == "*HardwareTools_HardwareDetailBomSettings*")
                    {
                      AppRibbonSetup.RibbonHardwareToolsShowHWDTicketBOMSettings = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'T':
                    if (str2 == "*TicketTools_TicketBomDuplicatesSchedule*")
                    {
                      AppRibbonSetup.RibbonTicketToolsTicketBomDuplicatesSchedule = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  case 'U':
                    if (str2 == "*UserSettings_TicketAndDimensionSettings*")
                    {
                      AppRibbonSetup.RibbonUserSettingsTicketAndDimensionSettings = !strArray[1].Equals("FALSE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 42:
                if (str2 == "*UserSettings_TitleBlockPopulatorSettings*")
                {
                  AppRibbonSetup.RibbonUserSettingsTBPSettings = !strArray[1].Equals("FALSE");
                  continue;
                }
                continue;
              case 44:
                if (str2 == "*TicketTools_BatchTicketTitleBlockPopulator*")
                {
                  AppRibbonSetup.RibbonTicketToolsBatchTicketTitleBlockPopulator = !strArray[1].Equals("FALSE");
                  continue;
                }
                continue;
              case 45:
                switch (str2[16 /*0x10*/])
                {
                  case 'W':
                    if (str2 == "*HardwareTools_HWTitleBlockPopulatorSettings*")
                    {
                      AppRibbonSetup.RibbonHardwareToolsShowHWDTBPSettings = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  case 'a':
                    if (str2 == "*HardwareTools_HardwareDetailTemplateManager*")
                    {
                      AppRibbonSetup.RibbonHardwareToolsShowHWDetailTemplateCreator = strArray[1].Equals("TRUE");
                      continue;
                    }
                    continue;
                  default:
                    continue;
                }
              case 49:
                if (str2 == "*UserSettings_TicketManagerCustomizationSettings*")
                {
                  AppRibbonSetup.RibbonUserSettingsTicketManagerCustomizationSettings = !strArray[1].Equals("FALSE");
                  continue;
                }
                continue;
              default:
                continue;
            }
          }
        }
      }
      streamReader.Close();
    }
    catch (Exception ex)
    {
    }
  }
}
