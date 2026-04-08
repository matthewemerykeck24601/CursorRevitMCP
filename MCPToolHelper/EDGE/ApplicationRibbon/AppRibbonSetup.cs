// Decompiled with JetBrains decompiler
// Type: EDGE.ApplicationRibbon.AppRibbonSetup
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.VisibilityTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utils;

#nullable disable
namespace EDGE.ApplicationRibbon;

internal class AppRibbonSetup
{
  public static bool PTACTools = false;
  public static bool RibbonAnnotationPanel = true;
  public static bool RibbonAnnotationTextUtilities = true;
  public static bool RibbonAnnotationDimensionUtilities = true;
  public static bool RibbonAnnotationToolsEDrawingAutoDim = true;
  public static bool RibbonAnnotationFreezeActiveView = true;
  public static bool RibbonGeometryPanel = true;
  public static bool RibbonGeometryAddonHostingUpdater = true;
  public static bool RibbonGeometryMultiVoidTools = true;
  public static bool RibbonGeometryWarpedParameterCopier = true;
  public static bool RibbonGeometryMarkOppTools = true;
  public static bool RibbonGeometryMarkVerificationInitial = true;
  public static bool RibbonGeometryMarkVerificationExisting = true;
  public static bool RibbonGeometryVoidHostingUpdater = true;
  public static bool RibbonGeometryGetCentroid = true;
  public static bool RibbonGeometryNewReferencePoint = true;
  public static bool RibbonGeometryAutoWarping = true;
  public static bool RibbonInsulationPanel = true;
  public static bool RibbonInsulationRemoval = true;
  public static bool RibbonInsulationAutoPlacement = true;
  public static bool RibbonInsulationManualPlacement = true;
  public static bool RibbonInsulationCopyInsulation = true;
  public static bool RibbonInsulationPinPlacement = true;
  public static bool RibbonInsulationMarking = true;
  public static bool RibbonInsulationMarkingPerWall = false;
  public static bool RibbonInsulationExport = true;
  public static bool RibbonInsulationDrawingMaster = true;
  public static bool RibbonInsulationDrawingPerPiece = true;
  public static bool RibbonInsulationDrawing = true;
  public static bool RibbonVisibilityPanel = true;
  public static bool RibbonSelectionAndPinningPanel = true;
  public static bool RibbonSelectionAndPinningGrids = true;
  public static bool RibbonSelectionAndPinningLevels = true;
  public static bool RibbonSelectionAndPinningSelectSfElements = true;
  public static bool RibbonSchedulingPanel = true;
  public static bool RibbonSchedulingControlNumberIncrementor = true;
  public static bool RibbonSchedulingBomProductHosting = true;
  public static bool RibbonSchedulingConstructionProductHosting = true;
  public static bool RibbonSchedulingScheduleSequenceByView = true;
  public static bool RibbonSchedulingMarkRebarByProduct = false;
  public static bool RibbonSchedulingErectionSequence = false;
  public static bool RibbonAssemblyPanel = true;
  public static bool RibbonAssemblyCreationUpdatingRenaming = true;
  public static bool RibbonAssemblyCreationUpdatingRevit = true;
  public static bool RibbonAssemblyCopyReinforcing = true;
  public static bool RibbonAssemblyMarkAsReinforced = true;
  public static bool RibbonAssemblyTopAsCast = true;
  public static bool RibbonAssemblyMatchTopAsCast = true;
  public static bool RibbonTicketToolsPanel = true;
  public static bool RibbonTicketToolsAnnotateEmbedLong = true;
  public static bool RibbonTicketToolsAnnotateEmbedShort = true;
  public static bool RibbonTicketToolsControlNumberPopulator = true;
  public static bool RibbonTicketToolsTicketBomDuplicatesSchedule = true;
  public static bool RibbonTicketToolsTicketBomPartsList = false;
  public static bool RibbonTicketToolsTicketTitleBlockPopulator = true;
  public static bool RibbonTicketToolsBatchTicketTitleBlockPopulator = true;
  public static bool RibbonTicketToolsCopyTicketViews = true;
  public static bool RibbonTicketToolsCopyTicketAnnotations = true;
  public static bool RibbonTicketToolsCopyTicketDimensions = true;
  public static bool RibbonTicketToolsFindReferringViews = true;
  public static bool RibbonTicketToolsTicketPopulator = true;
  public static bool RibbonTicketToolsBatchTicketPopulator = true;
  public static bool RibbonTicketToolsAutoDim = true;
  public static bool RibbonTicketToolsAutoTicketGeneration = true;
  public static bool RibbonTicketToolsCloneTicket = true;
  public static bool RibbonTicketToolsLaserExport = true;
  public static bool RibbonTicketManagerPanel = true;
  public static bool RibbonTicketManagerExport = false;
  public static bool RibbonFamilyToolsPanel = true;
  public static bool RibbonUserSettingsTicketAndDimensionSettings = true;
  public static bool RibbonUserSettingsRebarSettings = true;
  public static bool RibbonUserSettingsMarkVerificationSettings = true;
  public static bool RibbonUserSettingsTicketManagerCustomizationSettings = true;
  public static bool RibbonUserSettingsTBPSettings = true;
  public static bool RibbonUserSettingsShowTicketBOMSettings = true;
  public static bool RibbonUserSettingsShowCADExportSettings = true;
  public static bool RibbonUserSettingsShowInsulationDrawingSettings = true;
  public static bool RibbonHardwareToolsPanel = true;
  public static bool RibbonHardwareToolsHardwareDetail = true;
  public static bool RibbonHardwareToolsShowHWDetailTemplateCreator = true;
  public static bool RibbonHardwareToolsShowHWDTBPSettings = true;
  public static bool RibbonHardwareToolsShowHWDTicketBOMSettings = true;
  public static bool RibbonAdminPanel = true;
  public static bool RibbonAdminShowSharedParams = true;
  public static bool RibbonAdminShowBulkFamilyUpdater = true;
  public static bool RibbonAdminShowWarningAnalyzer = true;
  public static bool RibbonAdminShowEmbedClashVerification = true;
  public static bool RibbonAdminShowBPHCPHErrorFinder = true;
  public static bool RibbonAdminShowTicketTemplateCreator = true;
  public static bool RibbonAdminShowExcelExport = false;
  public static bool RibbonAdminShowCamExport = false;
  public static bool RibbonAdminShowPDMExport = false;
  public static bool RibbonAdminEdgeBrowser = false;
  private static bool s_bIsDebug;
  private static string userName = Environment.UserName;
  private static bool adminCheck = AppRibbonSetup.userName.Equals("Ken") || AppRibbonSetup.userName.Equals("kmarsh") || AppRibbonSetup.userName.Equals("Jordan") || AppRibbonSetup.userName.Equals("jwatkins");
  private static bool testerCheck = AppRibbonSetup.userName.Equals("Ken") || AppRibbonSetup.userName.Equals("kmarsh") || AppRibbonSetup.userName.Equals("Jordan") || AppRibbonSetup.userName.Equals("jwatkins");
  private static readonly string ExecutingAssemblyPath = App.ExecutingAssemblyPath;
  private static string Prefix;

  public static void SetupRibbonCommands(UIControlledApplication revitApp)
  {
    AppRibbonSetup.s_bIsDebug = QAUtils.IsInHouse();
    AppRibbonSetup.Prefix = AppRibbonSetup.s_bIsDebug ? "d_" : "";
    string tabName = !AppRibbonSetup.s_bIsDebug ? "EDGE^R" : "d_EDGE^R";
    revitApp.CreateRibbonTab(tabName);
    if (AppRibbonSetup.s_bIsDebug)
    {
      AppRibbonSetup.AddAnnotationMenu(revitApp, tabName);
      AppRibbonSetup.AddGeometryMenu(revitApp, tabName);
      AppRibbonSetup.AddInsulationMenu(revitApp, tabName);
      AppRibbonSetup.AddVisibilityMenu(revitApp, tabName);
      AppRibbonSetup.AddSelectionAndPinMenu(revitApp, tabName);
      AppRibbonSetup.AddSchedulingMenu(revitApp, tabName);
      AppRibbonSetup.AddAssemblyMenu(revitApp, tabName);
      AppRibbonSetup.AddTicketToolsMenu(revitApp, tabName);
      AppRibbonSetup.AddHardwareDetailMenu(revitApp, tabName);
      AppRibbonSetup.AddTicketManagerMenu(revitApp, tabName);
      AppRibbonSetup.AddFamilyUpdaterMenu(revitApp, tabName);
      AppRibbonSetup.AddUserSettingMenu(revitApp, tabName);
      AppRibbonSetup.AddAdminMenu(revitApp, tabName);
      AppRibbonSetup.AddDebugMenu(revitApp, tabName);
    }
    else
    {
      if (AppRibbonSetup.RibbonAnnotationPanel)
        AppRibbonSetup.AddAnnotationMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonGeometryPanel)
        AppRibbonSetup.AddGeometryMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonInsulationPanel)
        AppRibbonSetup.AddInsulationMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonVisibilityPanel)
        AppRibbonSetup.AddVisibilityMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonSelectionAndPinningPanel)
        AppRibbonSetup.AddSelectionAndPinMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonSchedulingPanel)
        AppRibbonSetup.AddSchedulingMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonAssemblyPanel)
        AppRibbonSetup.AddAssemblyMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonTicketToolsPanel)
        AppRibbonSetup.AddTicketToolsMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonHardwareToolsPanel)
        AppRibbonSetup.AddHardwareDetailMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonTicketManagerPanel)
        AppRibbonSetup.AddTicketManagerMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonFamilyToolsPanel)
        AppRibbonSetup.AddFamilyUpdaterMenu(revitApp, tabName);
      if (AppRibbonSetup.RibbonAdminPanel)
        AppRibbonSetup.AddAdminMenu(revitApp, tabName);
      AppRibbonSetup.AddUserSettingMenu(revitApp, tabName);
    }
    if (!AppRibbonSetup.s_bIsDebug || !AppRibbonSetup.testerCheck)
      return;
    AppRibbonSetup.AddTestingMenu(revitApp, tabName);
  }

  private static void AddDebugMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Debug").AddItem((RibbonItemData) new PulldownButtonData("Debug", "Debug")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "EDGE^R Debugging Tools";
    pulldownButton.AddPushButton(new PushButtonData("Write Edge Journal Comment", "Write Edge Journal Comment", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.WriteEdgeJournalComment"));
    pulldownButton.AddPushButton(new PushButtonData("Verify Rebar Mark Lists", "Verify Rebar Mark Lists", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.VerifyRebarMarksList_Command"));
    pulldownButton.AddPushButton(new PushButtonData("Journal Rebar Control Mark", "Journal Rebar Control Mark", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.RebarMarkJournaling_Command"));
    pulldownButton.AddSeparator();
    pulldownButton.AddPushButton(new PushButtonData("Enumerate Geometry", "Enumerate Geometry", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.GetElementGeometryMap_Command"));
    pulldownButton.AddPushButton(new PushButtonData("TransformVisualizer", "Transform Visualizer", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.TransformVisualizer"));
    pulldownButton.AddPushButton(new PushButtonData("PlanDimTesting", "PlanDimTesting", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.AutoDimensioning.PlanGeometryTest_Command"));
    pulldownButton.AddPushButton(new PushButtonData("CheckoutTest", "CheckoutTest", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.CheckoutDebugging"));
    pulldownButton.AddSeparator();
    pulldownButton.AddPushButton(new PushButtonData("CopyFilledRegionTest", "CopyFilledRegionTest", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.CopyAnnotation"));
    pulldownButton.AddSeparator();
    pulldownButton.AddPushButton(new PushButtonData("ShowElementBoundingBox", "ShowElementBoundingBox", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.ElementBoundingBox_Command"));
    pulldownButton.AddPushButton(new PushButtonData("Compute Centroid", "Compute Centroid", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.ComputeCentroid"));
    pulldownButton.AddSeparator();
    pulldownButton.AddPushButton(new PushButtonData("Reg_Test_Generator", "Reg Test Generator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.RegTestGen_Command.RegTestGen_Command"));
    pulldownButton.AddSeparator();
    pulldownButton.AddPushButton(new PushButtonData("LocationInForm_Command", "Reg LocationInForm_Command Generator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.LocationInFormTool.LocationInForm_Command"));
    pulldownButton.AddPushButton(new PushButtonData("CloudLogin_Command", "CloudLogin_Command", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.ViewAndDataExport.CloudLogin_Command"));
    pulldownButton.AddSeparator();
    PushButtonData pushButtonData1 = new PushButtonData("Intersection Test", "Intersection Test", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.QATools.IntersectsElementFilter_Test");
    ((RibbonItemData) pushButtonData1).ToolTip = "Intersection Test";
    pulldownButton.AddPushButton(pushButtonData1);
    PushButtonData pushButtonData2 = new PushButtonData("Copy Ticket Annotation and Dimension Per View", "Copy Ticket Annotation and Dimension Per View", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CopyTicketAnnotation.CopyTicketAnnotation_Command");
    ((RibbonItemData) pushButtonData2).ToolTip = "Copy Ticket Annotations and Dimensions from one view to another";
    pulldownButton.AddPushButton(pushButtonData2);
    PushButtonData pushButtonData3 = new PushButtonData("Copy Ticket Dimensions", "Copy Ticket Dimensions", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CopyTicketDimensions.CopyTicketDimensions_Command");
    ((RibbonItemData) pushButtonData3).ToolTip = "Copy Ticket Dimensions from one assembly view sheet to another";
    pulldownButton.AddPushButton(pushButtonData3);
    pulldownButton.AddPushButton(new PushButtonData("PointTest3D", "PointTest3D", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.PointTest3D"));
    pulldownButton.AddPushButton(new PushButtonData("showDockable", "showDockable", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.ShowDockableWindow"));
  }

  public static BitmapImage GetBitmapImage(string imageName)
  {
    Assembly.GetExecutingAssembly().GetManifestResourceNames();
    Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof (App).Namespace}.IconResources.{imageName}");
    BitmapImage bitmapImage = new BitmapImage();
    bitmapImage.BeginInit();
    bitmapImage.StreamSource = manifestResourceStream;
    bitmapImage.EndInit();
    return bitmapImage;
  }

  private static void AddAboutMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "About").AddItem((RibbonItemData) new PulldownButtonData("About", "About")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "EDGE^R Information";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("iconTemp.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("iconTemp_Small.png");
    pulldownButton.AddPushButton(new PushButtonData("About", "About EDGE^R", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.About.AboutEdge"));
    pulldownButton.AddPushButton(new PushButtonData("Change Log", "EDGE^R Change Log", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.About.ChangeLog"));
  }

  private static void AddUserSettingMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "User Settings").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "UserSetting", "User Settings")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "User Settings for edge functionalities.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Setting_Tools.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Setting_Tools_Sm.png");
    PushButtonData pushButtonData1 = new PushButtonData("TICKET SETTINGS", "Ticket and Dimension Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.AutoTicketUserSetting_Command");
    ((RibbonItemData) pushButtonData1).ToolTip = "Ticket and Dimension setting.";
    ((RibbonItemData) pushButtonData1).LongDescription = "";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_and_Dimension_Settings.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_and_Dimension_Settings.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-and-dimension-settings");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("MARK VERIFICATION SETTINGS", "Mark Verification Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.MarkPrefixUserSetting_Command");
    ((RibbonItemData) pushButtonData2).ToolTip = "Initial Mark verification setting.";
    ((RibbonItemData) pushButtonData2).LongDescription = "";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Mark_Verification_Settings.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Mark_Verification_Settings_Sm.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/mark-prefix-settings");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("EDGE PREFERENCES", "EDGE Preferences", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.EdgePreferenceUserSetting_Command");
    ((RibbonItemData) pushButtonData3).ToolTip = "Edge Preference Settings.";
    ((RibbonItemData) pushButtonData3).LongDescription = "";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Rebar_Settings.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Rebar_Settings_Sm.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/edge-preferences");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("REBAR SETTINGS", "Rebar Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.RebarUserSetting_Command");
    ((RibbonItemData) pushButtonData4).ToolTip = "Rebar Settings.";
    ((RibbonItemData) pushButtonData4).LongDescription = "";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Reference_Settings.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Reference_Settings_Sm.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/rebar-settings");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("TICKET MANAGER CUSTOMIZATION SETTINGS", "Ticket Manager Customization Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.TicketManagerCustomizationSetting_Command");
    ((RibbonItemData) pushButtonData5).ToolTip = "Ticket Manager Customization Settings.";
    ((RibbonItemData) pushButtonData5).LongDescription = "";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_Manager_Settings.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_Manager_Settings_Sm.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-manager-settings");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData6 = new PushButtonData("Title Block Populator Settings", "Title Block Populator Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.TitleBlockPopulatorUserSetting_Command");
    ((RibbonItemData) pushButtonData6).ToolTip = "Title Block Populator Settings Dialog";
    ((RibbonItemData) pushButtonData6).LongDescription = "Allows user to map parameters from the structural framing or assembly element to parameters on the titleblock";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Title_Block_Popular_Settings.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Title_Block_Popular_Settings_Sm.png");
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/title-block-populator-settings");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp6);
    PushButtonData pushButtonData7 = new PushButtonData("Ticket BOM Settings", "Ticket BOM Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.TicketBOMUserSetting_Command");
    ((RibbonItemData) pushButtonData7).ToolTip = "Ticket BOM User Settings Dialog";
    ((RibbonItemData) pushButtonData7).LongDescription = "Allows user to specify which schedules and note schedules should be automatically duplicated by the Ticket BOM Duplicate Schedules command";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_BOM_Settings.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_BOM_Settings_Sm.png");
    ContextualHelp contextualHelp7 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-bom-settings");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp7);
    PushButtonData pushButtonData8 = new PushButtonData("CAD Export Settings", "CAD Export Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.CAD_Export_Settings.CADExportSettings_Command");
    ((RibbonItemData) pushButtonData8).ToolTip = "CAD Export Settings Dialog";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CAD_Export_Settings.png");
    ContextualHelp contextualHelp8 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/cad-export-settings");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp8);
    PushButtonData pushButtonData9 = new PushButtonData("Insulation Drawing Settings", "Insulation Drawing Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.Insulation_Drawing_Settings.InsulationDrawingSetting_Command");
    ((RibbonItemData) pushButtonData9).ToolTip = "Insulation Drawing Settings Dialog";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Insulation_Drawing_Settings.png");
    ContextualHelp contextualHelp9 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/insulation-drawing-settings");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp9);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
    }
    else
    {
      if (AppRibbonSetup.RibbonUserSettingsTicketAndDimensionSettings)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonUserSettingsRebarSettings)
        pulldownButton.AddPushButton(pushButtonData4);
      if (AppRibbonSetup.RibbonUserSettingsMarkVerificationSettings)
        pulldownButton.AddPushButton(pushButtonData2);
      if (AppRibbonSetup.RibbonUserSettingsTicketManagerCustomizationSettings)
        pulldownButton.AddPushButton(pushButtonData5);
      if (AppRibbonSetup.RibbonUserSettingsTBPSettings)
        pulldownButton.AddPushButton(pushButtonData6);
      if (AppRibbonSetup.RibbonUserSettingsShowTicketBOMSettings)
        pulldownButton.AddPushButton(pushButtonData7);
      if (AppRibbonSetup.RibbonUserSettingsShowCADExportSettings)
        pulldownButton.AddPushButton(pushButtonData8);
      if (AppRibbonSetup.RibbonUserSettingsShowInsulationDrawingSettings)
        pulldownButton.AddPushButton(pushButtonData9);
    }
    pulldownButton.AddPushButton(pushButtonData3);
    pulldownButton.AddSeparator();
    pulldownButton.AddPushButton(new PushButtonData("About", "About EDGE^R", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.About.AboutEdge"));
    pulldownButton.AddPushButton(new PushButtonData("Change Log", "EDGE^R Change Log", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.About.ChangeLog"));
  }

  private static void AddAdminMenu(UIControlledApplication application, string tabName)
  {
    RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Admin");
    PulldownButtonData pulldownButtonData1 = new PulldownButtonData(AppRibbonSetup.Prefix + "Admin", "Admin");
    ((ButtonData) pulldownButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin.png");
    ((ButtonData) pulldownButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin_Small.png");
    PulldownButtonData pulldownButtonData2 = pulldownButtonData1;
    PulldownButton pulldownButton = ribbonPanel.AddItem((RibbonItemData) pulldownButtonData2) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Administrator tools related to the Resetting of Parameters and Testing of Methods.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("Warning Analyzer", "Warning Analyzer", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.WarningAnalyzer.WarningAnalyzer");
    ((RibbonItemData) pushButtonData1).ToolTip = "Analyzes an exported Error Log from the Warnings Manager.";
    ((RibbonItemData) pushButtonData1).LongDescription = "Display all elements joining Cutting, Joining, and/or Duplicate errors. This also provides the ability to automatically uncut and unjoin all elements generating errors.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("WarningAnalyzer.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("WarningAnalyzer_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/warning-analyzer");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Embed Clash Verification", "Embed Clash Verification", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.EmbedClashVerification");
    ((RibbonItemData) pushButtonData2).ToolTip = "Analyzes embeds to check for multiple clashes";
    ((RibbonItemData) pushButtonData2).LongDescription = "Analyzes embeds to check for clashes with multiple precast members (Structural Framing) which will cause errors with Auto-Assembly creation and Billof Material Schedules";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("EmbedWarningAnalyzer.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("EmbedWarningAnalyzer_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/embed-clash-verification");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("BPH/CPH Parameter Error Finder", "BPH/CPH Parameter Error Finder", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.BphCphParameterErrorFinder");
    ((RibbonItemData) pushButtonData3).ToolTip = "";
    ((RibbonItemData) pushButtonData3).LongDescription = "";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("BPH_CPH_ErrorFinder.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("BPH_CPH_ErrorFinder_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/bph-cph-param-error");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("Project Shared Parameters", "Project Shared Parameters", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.ProjectSharedParameters");
    ((RibbonItemData) pushButtonData4).ToolTip = "Adds standard Shared Parameters to the project";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ProjectSharedParameters.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ProjectSharedParameters_Small.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/project-parameter");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("Remove Templates and Filters", "Remove Templates/Filters", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.RemoveTemplatesAndFilters");
    PushButtonData pushButtonData6 = new PushButtonData("Model Locking Permissions", "Model Lock Permissions", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.IUpdaters.ModelLocking.ModelLockingAdmin_Command");
    ((RibbonItemData) pushButtonData6).ToolTip = "Launches Model Lock Permissions Dialog.";
    ((RibbonItemData) pushButtonData6).LongDescription = "Allows users to view and admins to edit model locking permissions.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Model_Locking.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Model_Locking_Sm.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/model-locking");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData7 = new PushButtonData("TicketTemplateCreator", "Ticket Template Manager", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TemplateToolsBase.EDGERCreateTemplate");
    ((RibbonItemData) pushButtonData7).ToolTip = "Create Ticket Template for the Current Sheet.";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TemplateCreatorLarge.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TemplateCreatorSmall.png");
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-template-creator");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp6);
    PushButtonData pushButtonData8 = new PushButtonData("BulkFamilyUpdater_Command", "Bulk Family Updater", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.BulkFamilyUpdater.BulkFamilyUpdater_Command");
    ((RibbonItemData) pushButtonData8).ToolTip = "Perform family update on an entire folder of Revit Family files.";
    ((RibbonItemData) pushButtonData8).LongDescription = "This command will automatically perform update operations for all families located in a predetermined folder.";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Bulk_Family_Updater.png");
    ((ButtonData) pushButtonData8).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Bulk_Family_Updater_Sm.png");
    ContextualHelp contextualHelp7 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/bulk-family-updater");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp7);
    PushButtonData pushButtonData9 = new PushButtonData("Send to EDGE^Cloud", "Send to EDGE^Cloud", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.Cloud.CloudExportCommand");
    ((RibbonItemData) pushButtonData9).ToolTip = "Send to EDGE^Cloud Dialog";
    ((RibbonItemData) pushButtonData9).LongDescription = "Allows user to export the current active 3D view to EDGE^Cloud to view online";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CloudExport.png");
    ((ButtonData) pushButtonData9).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("CloudExport_Small.png");
    ContextualHelp contextualHelp8 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/send-to-cloud");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp8);
    PushButtonData pushButtonData10 = new PushButtonData("pe_export", "PE Export", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.ExcelExport");
    ((RibbonItemData) pushButtonData10).ToolTip = "Takes void information from current document and exports it to excel.";
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("PEExport.png");
    ((ButtonData) pushButtonData10).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("PEExport_Small.png");
    ContextualHelp contextualHelp9 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/estimating-export");
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp9);
    PushButtonData pushButtonData11 = new PushButtonData("cam_export", "CAM Export", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.EDGECAM");
    ((RibbonItemData) pushButtonData11).ToolTip = "Exports assemblies to CAM formats.";
    ((ButtonData) pushButtonData11).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("EdgeCAM.png");
    ((ButtonData) pushButtonData11).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("EdgeCAM_Small.png");
    ContextualHelp contextualHelp10 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/edge-cam");
    ((RibbonItemData) pushButtonData11).SetContextualHelp(contextualHelp10);
    PushButtonData pushButtonData12 = new PushButtonData("PDM Export", "PDM Export", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.PDMExport");
    ((RibbonItemData) pushButtonData12).ToolTip = "Exports a Lisp compatible file with assembly information for PDM";
    ContextualHelp contextualHelp11 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/pdm-export");
    ((ButtonData) pushButtonData12).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("PDMExport.png");
    ((ButtonData) pushButtonData12).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("PDMExport_Small.png");
    ((RibbonItemData) pushButtonData12).SetContextualHelp(contextualHelp11);
    PushButtonData pushButtonData13 = new PushButtonData("EDGE^R Browser", "EDGE^R Browser", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.EDGEBrowser.EDGEBrowser");
    ((ButtonData) pushButtonData13).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin.png");
    ((ButtonData) pushButtonData13).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin_Small.png");
    ContextualHelp contextualHelp12 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/edge-panel");
    ((RibbonItemData) pushButtonData13).SetContextualHelp(contextualHelp12);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData7);
      if (App.ModelLocking)
        pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddPushButton(pushButtonData11);
      pulldownButton.AddPushButton(pushButtonData12);
      pulldownButton.AddPushButton(pushButtonData13);
    }
    else
    {
      if (!AppRibbonSetup.RibbonAdminPanel)
        return;
      if (AppRibbonSetup.RibbonAdminShowSharedParams)
      {
        pulldownButton.AddPushButton(pushButtonData4);
        pulldownButton.AddSeparator();
      }
      if (AppRibbonSetup.RibbonAdminShowWarningAnalyzer)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonAdminShowEmbedClashVerification)
      {
        pulldownButton.AddPushButton(pushButtonData2);
        pulldownButton.AddSeparator();
      }
      if (AppRibbonSetup.RibbonAdminShowBPHCPHErrorFinder)
      {
        pulldownButton.AddPushButton(pushButtonData3);
        pulldownButton.AddSeparator();
      }
      if (AppRibbonSetup.RibbonAdminShowTicketTemplateCreator)
        pulldownButton.AddPushButton(pushButtonData7);
      if (App.ModelLocking)
        pulldownButton.AddPushButton(pushButtonData6);
      if (AppRibbonSetup.RibbonAdminShowBulkFamilyUpdater)
        pulldownButton.AddPushButton(pushButtonData8);
      if (App.Edge_Cloud_Id != -1)
        pulldownButton.AddPushButton(pushButtonData9);
      if (AppRibbonSetup.RibbonAdminShowExcelExport)
        pulldownButton.AddPushButton(pushButtonData10);
      if (AppRibbonSetup.RibbonAdminShowCamExport)
        pulldownButton.AddPushButton(pushButtonData11);
      if (AppRibbonSetup.RibbonAdminShowPDMExport)
        pulldownButton.AddPushButton(pushButtonData12);
      if (!AppRibbonSetup.RibbonAdminEdgeBrowser)
        return;
      pulldownButton.AddPushButton(pushButtonData13);
    }
  }

  private static void AddPaneMenu(UIControlledApplication application, string tabName)
  {
    RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "EDGE^R Browser");
    PushButtonData pushButtonData1 = new PushButtonData("EDGE^R Browser", "EDGE^R Browser", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.EDGEBrowser.EDGEBrowser");
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Admin_Small.png");
    ContextualHelp contextualHelp = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/edge-panel");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData2 = pushButtonData1;
    ribbonPanel.AddItem((RibbonItemData) pushButtonData2);
  }

  private static void AddAnnotationMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Annotation").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Drafting", "Drafting")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to the Incrementing and Copying of Parameters, Turning On/Off Mark_Opp_Indicators, and Formatting Text.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Annotation.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Annotation_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("FormatTextNote_Upper_Selected", "Uppercase Selected", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.FormatTextNoteUpperSelectedElements");
    ((RibbonItemData) pushButtonData1).ToolTip = "Format all selected Text Notes to be uppercase.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TextUpperSelected.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TextUpperSelected_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/text-upper-case-selected");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("FormatTextNote_Upper_ActiveView", "Uppercase View", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.FormatTextNoteUpperActiveView");
    ((RibbonItemData) pushButtonData2).ToolTip = "Format all Text Notes in the Active View to be uppercase.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TextUpperView.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TextUpperView_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/text-upper-case-view");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("FormatTextNote_Lower_Selected", "Lowercase Selected", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.FormatTextNoteLowerSelectedElements");
    ((RibbonItemData) pushButtonData3).ToolTip = "Format all selected Text Notes to be lowercase.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TextLowerSelected.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TextLowerSelected_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/text-lower-case-selected");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("FormatTextNote_Lower_ActiveView", "Lowercase View", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.FormatTextNoteLowerActiveView");
    ((RibbonItemData) pushButtonData4).ToolTip = "Format all Text Notes in the Active View to be lowercase.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TextLowerView.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TextLowerView_Small.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/text-lower-case-view");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("Dimension_Prefix", "Add Dimension Prefix", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.DimensionAddPrefix");
    ((RibbonItemData) pushButtonData5).ToolTip = "Add user provided text string to Dimension as a Prefix.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextPrefix.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextPrefix_Small.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/dimensions-prefix");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData6 = new PushButtonData("Dimension_Suffix", "Add Dimension Suffix", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.DimensionAddSuffix");
    ((RibbonItemData) pushButtonData6).ToolTip = "Add user provided text string to Dimension as a Suffix.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextSuffix.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextSuffix_Small.png");
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/dimensions-suffix");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp6);
    PushButtonData pushButtonData7 = new PushButtonData("Dimension_Above", "Add Dimension Above", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.DimensionAddAbove");
    ((RibbonItemData) pushButtonData7).ToolTip = "Add user provided text string to Dimension as a Above String.";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextAbove.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextAbove_Small.png");
    ContextualHelp contextualHelp7 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/dimensions-above");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp7);
    PushButtonData pushButtonData8 = new PushButtonData("Dimension_Below", "Add Dimension Below", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.DimensionAddBelow");
    ((RibbonItemData) pushButtonData8).ToolTip = "Add user provided text string to Dimension as a Below String.";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextBelow.png");
    ((ButtonData) pushButtonData8).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("DimensionTextBelow_Small.png");
    ContextualHelp contextualHelp8 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/dimensions-below");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp8);
    PushButtonData pushButtonData9 = new PushButtonData("Erection Drawing Auto-dimension", "Erection Drawing Auto-dimension", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.AutoDimensioning.EDrawingAutoDim_Command");
    ((RibbonItemData) pushButtonData9).ToolTip = "Automatically place a dimension based on user selection of element(s) upon grid or level direction. Used in Plan Views, Elevation Views, Detail Views, and Section Views";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Auto_Dim_E_Drawing_Large.png");
    ((ButtonData) pushButtonData9).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Auto_Dim_E_Drawing_Small.png");
    ContextualHelp contextualHelp9 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/e-drawing-auto-dimension");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp9);
    PushButtonData pushButtonData10 = new PushButtonData("Freeze Active View", "Freeze Active View", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AnnotationTools.FreezeDrawingTools.FreezeDrawingCommand");
    ((RibbonItemData) pushButtonData10).ToolTip = "Generate Drafting View with a 2D projection of the active view.";
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Freeze_Active_View.png");
    ContextualHelp contextualHelp10 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/freeze-active-view");
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp10);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData10);
    }
    else
    {
      if (!AppRibbonSetup.RibbonAnnotationPanel)
        return;
      if (AppRibbonSetup.RibbonAnnotationTextUtilities)
      {
        pulldownButton.AddPushButton(pushButtonData2);
        pulldownButton.AddPushButton(pushButtonData1);
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData4);
        pulldownButton.AddPushButton(pushButtonData3);
      }
      if (AppRibbonSetup.RibbonAnnotationTextUtilities && AppRibbonSetup.RibbonAnnotationDimensionUtilities)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonAnnotationDimensionUtilities)
      {
        pulldownButton.AddPushButton(pushButtonData5);
        pulldownButton.AddPushButton(pushButtonData6);
        pulldownButton.AddPushButton(pushButtonData7);
        pulldownButton.AddPushButton(pushButtonData8);
      }
      if (AppRibbonSetup.RibbonAnnotationToolsEDrawingAutoDim)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData9);
      }
      if (!AppRibbonSetup.RibbonAnnotationFreezeActiveView)
        return;
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData10);
    }
  }

  private static void AddAssemblyMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Assembly").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Assembly", "Assembly")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to the Creation and Updating of Assemblies, and Updating Hosting Parameters.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyPulldown.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyPulldown_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("Assembly_Creation_And_Updating", "Assembly Creation, Updating & Renaming", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.AssemblyCreationAndUpdating");
    ((RibbonItemData) pushButtonData1).ToolTip = "Using the selected Structural Framing Elements and/or Assemblies, create Assemblies and/or update the existing Assemblies and then rename the Assembly to match the Mark Number.";
    ((RibbonItemData) pushButtonData1).LongDescription = "Create Assemblies or update existing Assemblies by adding intersecting elements (excluding Voids, Rebar, Cast, and Erection elements). Update all members of the new Assemblies with the appropriate CONSTRUCTION_PRODUCT_HOST and BOM_PRODUCT_HOST Parameter values and then rename the new/updated assembly to match its Mark Number.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyEDGE.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyEDGE_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/assembly-creation-renaming");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Assembly_Creation_Revit", "Assembly Creation & Updating (Revit)", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.AssemblyCreationRevit");
    ((RibbonItemData) pushButtonData2).ToolTip = "Using the selected Structural Framing Elements and/or Assemblies, create Assemblies and/or update the existing Assemblies using the Out-of-the-Box Revit Assembly process.";
    ((RibbonItemData) pushButtonData2).LongDescription = "Create Assemblies or update existing Assemblies by adding intersecting elements (excluding Voids, Rebar, Cast, and Erection elements). Update all members of the new Assemblies with the appropriate CONSTRUCTION_PRODUCT_HOST and BOM_PRODUCT_HOST Parameter values using the Out-of-the_Box Revit Assembly process.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyRevit.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyRevit_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/assembly-creation");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("Assembly_Reinforced", "Mark Assembly as Reinforced", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.AssemblyReinforcedUpdateStatus");
    ((RibbonItemData) pushButtonData3).ToolTip = "Mark the selected Assemblies as Reinforced and ready for Ticketing";
    ((RibbonItemData) pushButtonData3).LongDescription = "Mark the Assembly as Reinforced and update Ticket Manager parameters.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyReinforced.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AssemblyReinforced_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/mark-assembly-reinforced");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("TopAsCastTool", "Top as Cast Tool", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.Assembly_TopAsCast_Command");
    ((RibbonItemData) pushButtonData4).ToolTip = "Select Top As Cast Face";
    ((RibbonItemData) pushButtonData4).LongDescription = "Select Top As Cast face to accurately position the Assembly origin relative to the selected face.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Top_as_Cast.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Top_as_Cast_Sm.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/top-as-cast");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("MatchTopAsCastTool", "Match Top as Cast Tool", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.MatchTopAsCast");
    ((RibbonItemData) pushButtonData5).ToolTip = "Copy Assembly from ";
    ((RibbonItemData) pushButtonData5).LongDescription = "Select to Match Top As Cast of source Assembly origin and then accurately position the Assembly Origin on the Target.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Top_as_Cast_w_Check.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Top_as_Cast_w_Check_sm.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/match-top-as-cast-tool");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData6 = new PushButtonData("Copy Reinforcing", "Copy Reinforcing", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.CopyReinforcing_Command");
    ((RibbonItemData) pushButtonData6).ToolTip = "Copy Reinforcing from one assembly to another";
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/copy-reinforcing");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp6);
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Copy_Reinforcing.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Copy_Reinforcing_Sm.png");
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddPushButton(pushButtonData6);
    }
    else
    {
      if (!AppRibbonSetup.RibbonAssemblyPanel)
        return;
      if (AppRibbonSetup.RibbonAssemblyCreationUpdatingRenaming)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonAssemblyCreationUpdatingRenaming && AppRibbonSetup.RibbonAssemblyCreationUpdatingRevit)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonAssemblyCreationUpdatingRevit)
        pulldownButton.AddPushButton(pushButtonData2);
      if ((AppRibbonSetup.RibbonAssemblyCreationUpdatingRenaming || AppRibbonSetup.RibbonAssemblyCreationUpdatingRevit) && AppRibbonSetup.RibbonAssemblyMarkAsReinforced)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonAssemblyMarkAsReinforced)
        pulldownButton.AddPushButton(pushButtonData3);
      if (AppRibbonSetup.RibbonAssemblyTopAsCast)
        pulldownButton.AddPushButton(pushButtonData4);
      if (AppRibbonSetup.RibbonAssemblyMatchTopAsCast)
        pulldownButton.AddPushButton(pushButtonData5);
      if (!AppRibbonSetup.RibbonAssemblyCopyReinforcing)
        return;
      pulldownButton.AddPushButton(pushButtonData6);
    }
  }

  private static void AddFamilyUpdaterMenu(UIControlledApplication application, string tabName)
  {
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/family-tools-typ");
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Family Tools").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Family Tools", "Family Tools")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to the updating of new of families for use in Precast projects. (NOT FOR STRUCTURAL FRAMING FAMILIES)";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Family.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Family_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("EMBED_Family_Updating", "Embed Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterEmbed");
    ((RibbonItemData) pushButtonData1).ToolTip = "Updates new or existing Embed Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData1).LongDescription = "Updates new or existing Embed Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Embed Family of the current version.  Finishes are also able to be applied to the family with appropriate formula values";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("EmbedFamilyUpdater.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("EmbedFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("ERECTION_Family_Updating", "Erection Family (Loose Item) Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterErection");
    ((RibbonItemData) pushButtonData2).ToolTip = "Updates new or existing Erection Family (Loose item) to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData2).LongDescription = "Updates new or existing Erection family (Loose Item) to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Erection Family (Loose Item) of the current version.  Finishes are also able to be applied to the family with appropriate formula values";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ErectionFamilyUpdater.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ErectionFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData3 = new PushButtonData("CAST_Family_Updating", "Cast Family (CIP) Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterCast");
    ((RibbonItemData) pushButtonData3).ToolTip = "Updates new or existing Cast Family (CIP) to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData3).LongDescription = "Updates new or existing Cast Family (CIP) to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Cast Family (CIP) of the current version.  Finishes are also able to be applied to the family with appropriate formula values";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CIPFamilyUpdater.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("CIPFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData4 = new PushButtonData("LIFT_Family_Updating", "Lifting and Handling Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterLift");
    ((RibbonItemData) pushButtonData4).ToolTip = "Updates new or existing Lifting and Handling Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData4).LongDescription = "Updates new or existing Lifting and Handling Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Lifting and Handling Family of the current version.  Finishes are also able to be applied to the family with appropriate formula values";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("LiftingFamilyUpdater.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("LiftingFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData5 = new PushButtonData("PRODUCT_BY_OTHERS_Updating", "Products by Others Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterPBO");
    ((RibbonItemData) pushButtonData5).ToolTip = "Updates new or existing Product By Others Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData5).LongDescription = "Updates new or existing Product By Others Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Product By Others Family of the current version.  Finishes are also able to be applied to the family with appropriate formula values";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("PBOFamilyUpdater.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("PBOFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData6 = new PushButtonData("REBAR_Updating", "Rebar Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterRebar");
    ((RibbonItemData) pushButtonData6).ToolTip = "Updates new or existing Rebar Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData6).LongDescription = "Updates new or existing Rebar Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Rebar Family of the current version.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("RebarFamilyUpdater.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("RebarFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData7 = new PushButtonData("REBAR_Assembly_Updating", "Rebar Assembly Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterRebarAssemblyEmbed");
    ((RibbonItemData) pushButtonData7).ToolTip = "Updates new or existing Rebar Assembly Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData7).LongDescription = "Updates new or existing Rebar Assembly Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Rebar Assembly Family of the current version.";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("RebarAssemblyFamilyUpdater.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("RebarAssemblyFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData8 = new PushButtonData("STEM_Mesh_Updating", "Stem Mesh Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterStemMesh");
    ((RibbonItemData) pushButtonData8).ToolTip = "Updates new or existing Stem Mesh Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData8).LongDescription = "Updates new or existing Stem Mesh Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to a Stem Mesh Family of the current version.";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("MeshFamilyUpdater.png");
    ((ButtonData) pushButtonData8).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("MeshFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData9 = new PushButtonData("WWF_Updating", "WWF Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterWwf");
    ((RibbonItemData) pushButtonData9).ToolTip = "Updates new or existing WWF Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData9).LongDescription = "Updates new or existing WWF Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Mesh Family of the current version.";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("WWFFamilyUpdater.png");
    ((ButtonData) pushButtonData9).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("WWFFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData10 = new PushButtonData("INSULATION_Updating", "Insulation Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterInsulation");
    ((RibbonItemData) pushButtonData10).ToolTip = "Updates new or existing Insulation Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData10).LongDescription = "Updates new or existing Insulation Family to current version with appropriate parameters and values.This tool can also be used to convert an existing family to an Insulation Family of the current version.";
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationFamilyUpdater.png");
    ((ButtonData) pushButtonData10).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData11 = new PushButtonData("CONNECTION_Updating", "Connection Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterConnection");
    ((RibbonItemData) pushButtonData11).ToolTip = "Updates new or existing Connection Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData11).LongDescription = "Updates new or existing Connection Family to current version with appropriate parameters and values.";
    ((ButtonData) pushButtonData11).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ConnectionFamilyUpdater.png");
    ((ButtonData) pushButtonData11).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ConnectionFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData11).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData12 = new PushButtonData("PRODUCT_Updating", "Product Family Updating", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyUpdaterProducts");
    ((RibbonItemData) pushButtonData12).ToolTip = "Updates new or existing Product Family to current version with appropriate parameters and values.";
    ((RibbonItemData) pushButtonData12).LongDescription = "Updates new or existing Product Family to current version with appropriate parameters and values.";
    ((ButtonData) pushButtonData12).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("PrecastProductFamilyUpdater.png");
    ((ButtonData) pushButtonData12).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("PrecastProductFamilyUpdater_Small.png");
    ((RibbonItemData) pushButtonData12).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData13 = new PushButtonData("FINISH_Remover", "Finish Parameters Removal", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.FamilyTools.FamilyFinishesRemover");
    ((RibbonItemData) pushButtonData13).ToolTip = "Removes all known parameters related to finishes within the family and sets the standard parameters to (Type) rather than (Instance) where appropriate.";
    ((RibbonItemData) pushButtonData13).LongDescription = "Removes all known parameters related to finishes within the family and sets the standard parameters to (Type) rather than (Instance) where appropriate.";
    ((ButtonData) pushButtonData13).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("FinsihRemover.png");
    ((ButtonData) pushButtonData13).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("FinsihRemover_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/finish-param-removal");
    ((RibbonItemData) pushButtonData13).SetContextualHelp(contextualHelp2);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddSeparator();
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData11);
      pulldownButton.AddPushButton(pushButtonData12);
      pulldownButton.AddSeparator();
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData13);
      pulldownButton.AddSeparator();
    }
    else
    {
      if (!AppRibbonSetup.RibbonFamilyToolsPanel)
        return;
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddSeparator();
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData11);
      pulldownButton.AddPushButton(pushButtonData12);
      pulldownButton.AddSeparator();
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData13);
      pulldownButton.AddSeparator();
    }
  }

  private static void AddInsulationMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Insulation").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Insulation", "Insulation")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to the manipulation of Insulation.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Insulation_Tab.png");
    PushButtonData pushButtonData1 = new PushButtonData("Insulation Removal", "Insulation Removal", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.InsulationRemoval.MainInsulationRemoval");
    ((RibbonItemData) pushButtonData1).ToolTip = "Removes Insulation from Whole Project, Active View, or a Selection Group";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Insulation_Removal.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/insulation-removal");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Automatic Insulation Placement", "Automatic Insulation Placement", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.InsulationPlacement.AutomaticPlacement");
    ((RibbonItemData) pushButtonData2).ToolTip = "Automatically place insulation for selected wall panels.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Auto_Ins_Placement.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/auto-insulation-placement");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("Manual Insulation Placement", "Manual Insulation Placement", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.InsulationPlacement.ManualPlacement");
    ((RibbonItemData) pushButtonData3).ToolTip = "Manually place insulation for selected wall panels.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Manual_Ins_Placement.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/manual-insulation-placement");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("Automatic Pin Placement", "Automatic Pin Placement", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.PinPlacement.MainPlacement");
    ((RibbonItemData) pushButtonData4).ToolTip = "Automatically place pins in wall panel insulation.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Auto_Pin_Placement.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/auto-pin-placement");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("InsulationMarking", "Insulation Marking", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.InsulationMarking");
    ((RibbonItemData) pushButtonData5).ToolTip = "Marks all insulation in the project based on similarity.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationFamilyUpdater.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationFamilyUpdater_Small.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/insulation-marking");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData6 = new PushButtonData("InsulationMarkingPerWall", "Insulation Marking Per Wall", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.HKInsulationMarking");
    ((RibbonItemData) pushButtonData6).ToolTip = "Marks all insulation in the project per panel based on similarity.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationFamilyUpdater.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationFamilyUpdater_Small.png");
    PushButtonData pushButtonData7 = new PushButtonData("Insulation Export", "Insulation Export", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.CAM.InsulationExport");
    ((RibbonItemData) pushButtonData7).ToolTip = "Export DWG for insulation automation.";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CNC.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("CNC_Small.png");
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/insulation-dwg-export");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp6);
    PushButtonData pushButtonData8 = new PushButtonData("Insulation Drawing - Master", "Insulation Drawing - Master", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.InsulationDrawing.InsulationDrawingMaster_Command");
    ((RibbonItemData) pushButtonData8).ToolTip = "Creates Insulation Detail Legends";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Insulation_Drawing_Master.png");
    PushButtonData pushButtonData9 = new PushButtonData("Insulation Drawing - Assembly", "Insulation Drawing - Assembly", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.InsulationDrawing.InsulationDrawingPerPiece_Command");
    ((RibbonItemData) pushButtonData9).ToolTip = "Creates Insulation Detail Legends per Selected Assemblies";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Insulation_Drawing_Assembly.png");
    PushButtonData pushButtonData10 = new PushButtonData("Insulation Drawing - Mark", "Insulation Drawing - Mark", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.InsulationTools.InsulationDrawing.InsulationDrawing_Command");
    ((RibbonItemData) pushButtonData10).ToolTip = "Create Insulation Drawing";
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Insulation_Drawing.png");
    ContextualHelp contextualHelp7 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/insulation-drawing");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp7);
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp7);
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp7);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
    }
    else
    {
      if (!AppRibbonSetup.RibbonInsulationPanel)
        return;
      if (AppRibbonSetup.RibbonInsulationRemoval)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonInsulationAutoPlacement || AppRibbonSetup.RibbonInsulationManualPlacement)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonInsulationAutoPlacement)
        pulldownButton.AddPushButton(pushButtonData2);
      if (AppRibbonSetup.RibbonInsulationManualPlacement)
        pulldownButton.AddPushButton(pushButtonData3);
      if (AppRibbonSetup.RibbonInsulationAutoPlacement || AppRibbonSetup.RibbonInsulationManualPlacement)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonInsulationPinPlacement)
        pulldownButton.AddPushButton(pushButtonData4);
      if (AppRibbonSetup.RibbonInsulationMarking)
        pulldownButton.AddPushButton(pushButtonData5);
      if (AppRibbonSetup.RibbonInsulationMarkingPerWall)
        pulldownButton.AddPushButton(pushButtonData6);
      if (AppRibbonSetup.RibbonInsulationExport)
        pulldownButton.AddPushButton(pushButtonData7);
      if (AppRibbonSetup.RibbonInsulationDrawingMaster || AppRibbonSetup.RibbonInsulationDrawingPerPiece || AppRibbonSetup.RibbonInsulationDrawing)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonInsulationDrawingMaster)
        pulldownButton.AddPushButton(pushButtonData8);
      if (AppRibbonSetup.RibbonInsulationDrawingPerPiece)
        pulldownButton.AddPushButton(pushButtonData9);
      if (!AppRibbonSetup.RibbonInsulationDrawing)
        return;
      pulldownButton.AddPushButton(pushButtonData10);
    }
  }

  private static void AddGeometryMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Geometry").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Geometry", "Geometry")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to the manipulation of geometry.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Geometry.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Geometry_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("MultiVoidCutting", "Multi Void Cutting", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.MultiVoidCutting");
    ((RibbonItemData) pushButtonData1).ToolTip = "Using the selected Voids, cut all following elements.";
    ((RibbonItemData) pushButtonData1).LongDescription = "First select all Voids to cut with. Then select the elements to be cut. If the Void does not intersect the element, no cut will be made.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("MultiVoidCutting.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("MultiVoidCutting_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/multi-void-cutting");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("MultiVoidUncutting", "Multi Void Uncutting", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.MultiVoidUncutting");
    ((RibbonItemData) pushButtonData2).ToolTip = "Using the selected Voids, uncut all following elements.";
    ((RibbonItemData) pushButtonData2).LongDescription = "First select all Voids to uncut with. Then select the elements to be uncut. If the Void does not intersect the element, no uncut will be made.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("MultiVoidUnCutting.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("MultiVoidUnCutting_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/multi-void-cutting");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("Add-on Hosting Updater", "Add-on Hosting Updater", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.AddonHostingUpdater");
    ((RibbonItemData) pushButtonData3).ToolTip = "Update the CONSTRUCTION_PRODUCT, Material, and CONTROL_MARK Parameter values of all Add-ons with the appropriate values from the Structural Framing element they intersect.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AddonHostingUpdater.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AddonHostingUpdater_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/addon-hosting-updater");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("WarpedParameterCopier", "Warped Parameter Copier", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.WarpedParameterUpdater");
    ((RibbonItemData) pushButtonData4).ToolTip = "Copy the first selected element's Warped Parameters to all following elements.";
    ((RibbonItemData) pushButtonData4).LongDescription = "Copy the Vertical_Offset_MarkEnd, Vertical_Offset_OppEnd, Warp_Angle_MarkEnd, and Warp_Angle_OppEnd Parameters from the first selected element to all following elements.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("WarpParamUpdater.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("WarpParamUpdater_Small.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/warped-param-copier");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("MarkOppIndicatorsOn", "MarkOpp On", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.MarkOppIndicatorsOnActiveView");
    ((RibbonItemData) pushButtonData5).ToolTip = "Turn ON Mark_Opp_Indicators for all Structural Framing elements in the Active View.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("MarkOppON.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("MarkOppON_Small.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/markopp-on-off");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData6 = new PushButtonData("MarkOppIndicatorsOff", "MarkOpp Off", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.MarkOppIndicatorsOffActiveView");
    ((RibbonItemData) pushButtonData6).ToolTip = "Turn OFF Mark_Opp_Indicators for all Structural Framing elements in the Active View.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("MarkOppOFF.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("MarkOppOFF_Small.png");
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/markopp-on-off");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp6);
    PushButtonData pushButtonData7 = new PushButtonData("MarkVerificationExisting", "Mark Verification Existing", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.MarkVerification.MarkVerification_Existing_Command");
    ((RibbonItemData) pushButtonData7).ToolTip = "Verify existing marks match.";
    ((RibbonItemData) pushButtonData7).LongDescription = "Long Description";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Existing_Mark_Verification.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Existing_Mark_Verification_Sm.png");
    ContextualHelp contextualHelp7 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/mark-verification-existing");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp7);
    PushButtonData pushButtonData8 = new PushButtonData("MarkVerification_Initial_Command", "Mark Verification Initial", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AssemblyTools.MarkVerification.MarkVerification_Initial_Command");
    ((RibbonItemData) pushButtonData8).ToolTip = "Initial Categorization of Marks";
    ((RibbonItemData) pushButtonData8).LongDescription = "Long Description";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Initial_Mark_Verification.png");
    ((ButtonData) pushButtonData8).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Initial_Mark_Verification_Sm.png");
    ContextualHelp contextualHelp8 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/mark-verification-initial");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp8);
    PushButtonData pushButtonData9 = new PushButtonData("New_Reference_Point", "New Reference Point", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.NewReferencePoint");
    ((RibbonItemData) pushButtonData9).ToolTip = "Moves a selected element to a new reference point.";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Move.png");
    ((ButtonData) pushButtonData9).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Move_Sm.png");
    ContextualHelp contextualHelp9 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/new-reference-point");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp9);
    PushButtonData pushButtonData10 = new PushButtonData("Get_Centroid", "Get Centroid", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.GetCentroid");
    ((RibbonItemData) pushButtonData10).ToolTip = "Gets the center of gravity for selected item.";
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Centroid_Tool.png");
    ((ButtonData) pushButtonData10).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Centroid_Tool_Sm.png");
    ContextualHelp contextualHelp10 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/get-centroid");
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp10);
    PushButtonData pushButtonData11 = new PushButtonData("Void_Hosting_Updater", "Void Hosting Updater", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.VoidHostingUpdater");
    ((RibbonItemData) pushButtonData11).ToolTip = "Hosts the values of a void with the appropriate value from the structural framing element it intersects.";
    ((ButtonData) pushButtonData11).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("VoidHostingUpdater.png");
    ((ButtonData) pushButtonData11).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("VoidHostingUpdater_Small.png");
    ContextualHelp contextualHelp11 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/void-hosting-updater");
    ((RibbonItemData) pushButtonData11).SetContextualHelp(contextualHelp11);
    PushButtonData pushButtonData12 = new PushButtonData("Auto Warping", "Auto Warping", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.GeometryTools.AutoWarping.AutoWarping");
    ((RibbonItemData) pushButtonData12).ToolTip = "Warps all elements within a loop created by user-placed spot elevation points.";
    ((ButtonData) pushButtonData12).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AutoWarping.png");
    ((ButtonData) pushButtonData12).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AutoWarping_Small.png");
    ContextualHelp contextualHelp12 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/auto-warping");
    ((RibbonItemData) pushButtonData12).SetContextualHelp(contextualHelp12);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData3);
      if (AppRibbonSetup.RibbonGeometryVoidHostingUpdater)
        pulldownButton.AddPushButton(pushButtonData11);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData12);
      pulldownButton.AddSeparator();
    }
    else
    {
      if (!AppRibbonSetup.RibbonGeometryPanel)
        return;
      if (AppRibbonSetup.RibbonGeometryAddonHostingUpdater)
        pulldownButton.AddPushButton(pushButtonData3);
      if (AppRibbonSetup.RibbonGeometryVoidHostingUpdater)
        pulldownButton.AddPushButton(pushButtonData11);
      if (AppRibbonSetup.RibbonGeometryAddonHostingUpdater && AppRibbonSetup.RibbonGeometryMultiVoidTools)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonGeometryMultiVoidTools)
      {
        pulldownButton.AddPushButton(pushButtonData1);
        pulldownButton.AddPushButton(pushButtonData2);
      }
      if ((AppRibbonSetup.RibbonGeometryAddonHostingUpdater || AppRibbonSetup.RibbonGeometryMultiVoidTools) && AppRibbonSetup.RibbonGeometryWarpedParameterCopier)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonGeometryWarpedParameterCopier)
        pulldownButton.AddPushButton(pushButtonData4);
      if (AppRibbonSetup.RibbonGeometryMarkOppTools)
      {
        pulldownButton.AddPushButton(pushButtonData5);
        pulldownButton.AddPushButton(pushButtonData6);
      }
      if (AppRibbonSetup.RibbonGeometryMarkVerificationExisting && AppRibbonSetup.RibbonGeometryMarkVerificationInitial)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData7);
        pulldownButton.AddPushButton(pushButtonData8);
      }
      else if (AppRibbonSetup.RibbonGeometryMarkVerificationExisting)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData7);
      }
      else if (AppRibbonSetup.RibbonGeometryMarkVerificationInitial)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData8);
      }
      if (AppRibbonSetup.RibbonGeometryNewReferencePoint)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData9);
      }
      if (AppRibbonSetup.RibbonGeometryGetCentroid)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData10);
      }
      if (!AppRibbonSetup.RibbonGeometryAutoWarping)
        return;
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData12);
    }
  }

  private static void AddSelectionAndPinMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Selection and Pinning").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Selection and Pinning", "Selection and Pinning")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to the Selection, Pinning, and Unpinning of elements in the model.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("SelectionAndPinning.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("SelectionAndPinning_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("SelectAll_StructFraming", "Select All Structural Framing Elements", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SelectionAndPinningTools.SelectAllStructFraming");
    ((RibbonItemData) pushButtonData1).ToolTip = "Select all Structural Framing elements in the model.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("SelectStructuralFraming.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("SelectStructuralFraming_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/select-structural-framing-elements");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Pin_All_Grids", "Pin all Grids", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SelectionAndPinningTools.GridsPinAll");
    ((RibbonItemData) pushButtonData2).ToolTip = "Pins all Grids within the model to prevent accidental changes.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("GridsPinned.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("GridsPinned_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/pin-unpin-all-grids");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("Unpin_All_Grids", "Unpin all Grids", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SelectionAndPinningTools.GridsUnpinAll");
    ((RibbonItemData) pushButtonData3).ToolTip = "Unpins all Grids within the model.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("GridsUnpinned.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("GridsUnpinned_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/pin-unpin-all-grids");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("Pin_All_Levels", "Pin all Levels", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SelectionAndPinningTools.LevelsPinAll");
    ((RibbonItemData) pushButtonData4).ToolTip = "Pins all Levels within the model to prevent accidental changes.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("LevelsPinned.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("LevelsPinned_Small.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/pin-unpin-all-levels");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData5 = new PushButtonData("Unpin_All_Levels", "Unpin all Levels", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SelectionAndPinningTools.LevelsUnpinAll");
    ((RibbonItemData) pushButtonData5).ToolTip = "Unpins all Levels within the model.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("LevelsUnpinned.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("LevelsUnpinned_Small.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/pin-unpin-all-levels");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp5);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
    }
    else
    {
      if (!AppRibbonSetup.RibbonSelectionAndPinningPanel)
        return;
      if (AppRibbonSetup.RibbonSelectionAndPinningGrids)
      {
        pulldownButton.AddPushButton(pushButtonData2);
        pulldownButton.AddPushButton(pushButtonData3);
        pulldownButton.AddSeparator();
      }
      if (AppRibbonSetup.RibbonSelectionAndPinningGrids && AppRibbonSetup.RibbonSelectionAndPinningLevels)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonSelectionAndPinningLevels)
      {
        pulldownButton.AddPushButton(pushButtonData4);
        pulldownButton.AddPushButton(pushButtonData5);
      }
      if ((AppRibbonSetup.RibbonSelectionAndPinningLevels || AppRibbonSetup.RibbonSelectionAndPinningGrids) && AppRibbonSetup.RibbonSelectionAndPinningSelectSfElements)
        pulldownButton.AddSeparator();
      if (!AppRibbonSetup.RibbonSelectionAndPinningSelectSfElements)
        return;
      pulldownButton.AddPushButton(pushButtonData1);
    }
  }

  private static void AddSchedulingMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Scheduling").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Scheduling", "Scheduling")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to scheduling.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Scheduling.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Scheduling_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("Control Number Incrementor", "Control# Incrementor", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.ControlNumberIncrementor");
    ((RibbonItemData) pushButtonData1).ToolTip = "Starting with the first element selected, increment the CONTROL_NUMBER Parameter of the following elements.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlIncrementorDialog.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlIncrementorDialog_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/control-number-incrementor");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Control Number Incrementor 3", "Control# Incrementor 3", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.ControlNumberIncrementorThreeDigits");
    ((RibbonItemData) pushButtonData2).ToolTip = "Starting with the first element selected, increment the CONTROL_NUMBER Parameter of the following elements.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlIncrementor.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlIncrementor_Small.png");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData3 = new PushButtonData("Control Number Incrementor 4", "Control# Incrementor 4", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.ControlNumberIncrementorFourDigits");
    ((RibbonItemData) pushButtonData3).ToolTip = "Starting with the first element selected, increment the CONTROL_NUMBER Parameter of the following elements.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlIncrementor.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlIncrementor_Small.png");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/control-number-incrementor"));
    PushButtonData pushButtonData4 = new PushButtonData("BOM_Product_Hosting_Updater", "BOM Product Hosting", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.BphUpdater");
    ((RibbonItemData) pushButtonData4).ToolTip = "Update the BOM_PRODUCT_HOST Parameter values for elements in the model.";
    ((RibbonItemData) pushButtonData4).LongDescription = "Update the BOM_PRODUCT_HOST Parameter values of Generic Model and Specialty Equipment (excluding Voids, Rebar, Cast, and Erection elements) with CONTROL_MARK of the Structural Framing they are intersecting.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("BPH_Updater.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("BPH_Updater_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/bom-product-hosting");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData5 = new PushButtonData("Construction_Product_Hosting_Updater", "Construction Product Hosting", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.CphUpdater");
    ((RibbonItemData) pushButtonData5).ToolTip = "Update the CONSTRUCTION_PRODUCT_HOST Parameter values for elements in the model.";
    ((RibbonItemData) pushButtonData5).LongDescription = "Update the CONSTRUCTION_PRODUCT_HOST Parameter values of Generic Model and Specialty Equipment (excluding Voids, Rebar, Cast, and Erection elements) with CONTROL_MARK of the Structural Framing they are intersecting.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CPH_Updater.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("CPH_Updater_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/construction-product-hosting");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData6 = new PushButtonData("Schedule Sequence By View", "Schedule Sequence By View", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.ScheduleSequenceByView");
    ((RibbonItemData) pushButtonData6).ToolTip = "Add a test String to PROD_EREC_SEQUENCE Parameter to the visible elements in the view for schedule filtering";
    ((RibbonItemData) pushButtonData6).LongDescription = "Add a text string to the elements visible in the view for schedule filtering for customized schedules of special production, erection or other sequencing";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("SSBV.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("SSBV_Small.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/schedule-sequence-by-view");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData7 = new PushButtonData("Mark Rebar by Product", "Mark Rebar by Product", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.CoreslabRebarUpdater_Command");
    ((RibbonItemData) pushButtonData7).ToolTip = "";
    ((RibbonItemData) pushButtonData7).LongDescription = "";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Rebar.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Rebar_Small.png");
    PushButtonData pushButtonData8 = new PushButtonData("Erection Sequence", "Erection Sequence", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.SchedulingTools.ErectionSequenceTool.ErectionSequence_Command");
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Erection_Sequence.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/erection-sequence");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp5);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
    }
    else
    {
      if (!AppRibbonSetup.RibbonSchedulingPanel)
        return;
      if (AppRibbonSetup.RibbonSchedulingControlNumberIncrementor)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonSchedulingControlNumberIncrementor && AppRibbonSetup.RibbonSchedulingBomProductHosting)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonSchedulingBomProductHosting)
        pulldownButton.AddPushButton(pushButtonData4);
      if (AppRibbonSetup.RibbonSchedulingConstructionProductHosting)
        pulldownButton.AddPushButton(pushButtonData5);
      if ((AppRibbonSetup.RibbonSchedulingControlNumberIncrementor || AppRibbonSetup.RibbonSchedulingBomProductHosting) && AppRibbonSetup.RibbonSchedulingScheduleSequenceByView)
        pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonSchedulingScheduleSequenceByView)
        pulldownButton.AddPushButton(pushButtonData6);
      if (AppRibbonSetup.RibbonSchedulingMarkRebarByProduct)
      {
        pulldownButton.AddSeparator();
        pulldownButton.AddPushButton(pushButtonData7);
      }
      if (!AppRibbonSetup.RibbonSchedulingErectionSequence)
        return;
      pulldownButton.AddPushButton(pushButtonData8);
    }
  }

  private static void AddTestingMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Testing - DO NOT USE").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Testing", "Testing - DO NOT USE")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Testing Tools - DO NOT USE";
    pulldownButton.AddPushButton(new PushButtonData("PopulateAssemblyFields", "PopulateAssemblyFields", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.PopulateAssemblyFields"));
    pulldownButton.AddPushButton(new PushButtonData("RefineNestedFamiliesTest", "RefineNestedFamiliesTest", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.RefineNestedFamiliesTest"));
    pulldownButton.AddPushButton(new PushButtonData("MoveFromReference", "MoveFromReference", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.NrfMove"));
    pulldownButton.AddPushButton(new PushButtonData("CopyFromReference", "CopyFromReference", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.NrfCopy"));
    pulldownButton.AddPushButton(new PushButtonData("MultiVoidCutUncutTest", "MultiVoidCutUncutTest", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.__Testing.MultiVoidCutUncutTest"));
  }

  private static void AddTicketToolsMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Ticket Tools").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Ticket Tools", "Ticket Tools")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to Tickets.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticketing.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticketing_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("Control Number Populator", "Control Number Populator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.ControlNumberPopulator");
    ((RibbonItemData) pushButtonData1).ToolTip = "Get the CONTROL_NUMBER of all Elements matching the input CONTROL_MARK value.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlNumberPopulator.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ControlNumberPopulator.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/control-number-populator");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Annotate Embed Long", "Annotate Embed Long", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.AnnotateEmbedLong");
    ((RibbonItemData) pushButtonData2).ToolTip = "Create a TextNote with Leaders describing the selected Embeds in a Sheet View using the Long Description of the Embed.";
    ((RibbonItemData) pushButtonData2).LongDescription = "Uses the IDENTITY_DESCRIPTION_SHORT and the IDENTITY_DESCRIPTION Parameter values to create a new TextNote with Leaders to describe the selected Element.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AnnotateEmbedLong.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AnnotateEmbedLong_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/annotate-embed-long");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("Annotate Embed Short", "Annotate Embed Short", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.AnnotateEmbedShort");
    ((RibbonItemData) pushButtonData3).ToolTip = "Create a TextNote with Leaders describing the selected Embeds in a Sheet View using the Short Description of the Embed.";
    ((RibbonItemData) pushButtonData3).LongDescription = "Uses the IDENTITY_DESCRIPTION_SHORT Parameter value to create a new TextNote with Leaders to describe the selected Element.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("AnnotateEmbedShort.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("AnnotateEmbedShort_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/annotate-embed-short");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("TicketBOM", "Ticket BOM (Parts List)", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TicketBomPartsList");
    ((RibbonItemData) pushButtonData4).ToolTip = "Create Schedules for the Assembly associated with the active Ticket View using Assembly Parts Lists.";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketBOMPL.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketBOMPL_Small.png");
    PushButtonData pushButtonData5 = new PushButtonData("TicketPopulator", "Ticket Populator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TicketPopulator.EDGERTicketPopulator");
    ((RibbonItemData) pushButtonData5).ToolTip = "Create Ticket for Currently Selected Assembly.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketPopulatorLarge.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketPopulatorSmall.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-populator");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp4);
    PushButtonData pushButtonData6 = new PushButtonData("BatchTicketPopulator", "Batch Ticket Populator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TicketPopulator.BatchTicketPopulator");
    ((RibbonItemData) pushButtonData6).ToolTip = "Create Ticket(s) for Currently Selected Assembly or assemblies.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketPopulatorLarge.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketPopulatorSmall.png");
    ContextualHelp contextualHelp5 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-populator");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp5);
    PushButtonData pushButtonData7 = new PushButtonData("TicketBOM_Backup", "Ticket BOM (Duplicates Schedules)", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TicketBomDuplicateSchedules");
    ((RibbonItemData) pushButtonData7).ToolTip = "Create Schedules for the Assembly associated with the active Ticket View by duplicating template Schedules.";
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketBOMDS.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketBOMDS_Small.png");
    ContextualHelp contextualHelp6 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-bom-duplicate");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp6);
    PushButtonData pushButtonData8 = new PushButtonData("TicketTitleBlockPopulator", "Ticket Title Block Populator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TitleBlockPopulator");
    ((RibbonItemData) pushButtonData8).ToolTip = "Update the Ticket Titleblock with information from the Product";
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TitleBlockUpdater.png");
    ((ButtonData) pushButtonData8).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TitleBlockUpdater_Small.png");
    ContextualHelp contextualHelp7 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/title-block-populator");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp7);
    PushButtonData pushButtonData9 = new PushButtonData("BatchTicketTitleBlockPopulator", "Batch Ticket Title Block Populator", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TitleBlockPopulatorBatch");
    ((RibbonItemData) pushButtonData9).ToolTip = "Update the Ticket Titleblock with information from the Product";
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TitleBlockUpdater.png");
    ((ButtonData) pushButtonData9).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TitleBlockUpdater_Small.png");
    ContextualHelp contextualHelp8 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/batch-title-block-populator");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp8);
    PushButtonData pushButtonData10 = new PushButtonData("Copy Ticket Views", "Copy Ticket Views", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CopyTicketViews.CopyTicketViews");
    ((RibbonItemData) pushButtonData10).ToolTip = "Copies selected Legends and Symbols to the designated Sheets.";
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CopyTicketViews.png");
    ((ButtonData) pushButtonData10).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("CopyTicketViews_Small.png");
    ContextualHelp contextualHelp9 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/copy-ticket-views");
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp9);
    PushButtonData pushButtonData11 = new PushButtonData("Find Referring Views", "Find Referring Views", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.FindReferringViews.FindReferringViews");
    ((RibbonItemData) pushButtonData11).ToolTip = "Find the Sheets that any selected Legend or Symbol exists on.";
    ((ButtonData) pushButtonData11).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("FindReferringViews.png");
    ((ButtonData) pushButtonData11).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("FindReferringViews_Small.png");
    ContextualHelp contextualHelp10 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/find-referring-views");
    ((RibbonItemData) pushButtonData11).SetContextualHelp(contextualHelp10);
    PushButtonData pushButtonData12 = new PushButtonData("Copy Ticket Annotation", "Copy Ticket Annotation", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CopyTicketAnnotation.CopyTicketAnnotations_Command");
    ((RibbonItemData) pushButtonData12).ToolTip = "Copy Ticket Annotation from one assembly view sheet to another";
    ContextualHelp contextualHelp11 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/copy-ticket-annotation");
    ((RibbonItemData) pushButtonData12).SetContextualHelp(contextualHelp11);
    ((ButtonData) pushButtonData12).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Copy_Ticket_Annotation.png");
    ((ButtonData) pushButtonData12).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Copy_Ticket_Annotation_Sm.png");
    PushButtonData pushButtonData13 = new PushButtonData("Ticket Auto-dimension", "Ticket Auto-dimension", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.AutoDimensioning.TicketAutoDim_Command");
    ((RibbonItemData) pushButtonData13).ToolTip = "Automatically dimension user selection of element with the same control mark.";
    ((ButtonData) pushButtonData13).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_Auto_Dimension.png");
    ((ButtonData) pushButtonData13).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_Auto_Dimension_Sm.png");
    ContextualHelp contextualHelp12 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-auto-dimension");
    ((RibbonItemData) pushButtonData13).SetContextualHelp(contextualHelp12);
    PushButtonData pushButtonData14 = new PushButtonData("Auto Ticket Generation", "Auto Ticket Generation", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.AutoDimensioning.BatchAutoDimension");
    ((RibbonItemData) pushButtonData14).ToolTip = "Generate shop tickets for assemblies.";
    ((ButtonData) pushButtonData14).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_refresh.png");
    ((ButtonData) pushButtonData14).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Ticket_refresh_sm.png");
    ContextualHelp contextualHelp13 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/auto-ticket-generation");
    ((RibbonItemData) pushButtonData14).SetContextualHelp(contextualHelp13);
    PushButtonData pushButtonData15 = new PushButtonData("Clone Ticket", "Clone Ticket", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CloneTicket.CloneTicket");
    ((RibbonItemData) pushButtonData15).ToolTip = "Generate shop tickets for assemblies based on formatting of existing sheets";
    ((ButtonData) pushButtonData15).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Clone_Ticket.png");
    ((ButtonData) pushButtonData15).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Clone_Ticket_Small.png");
    ContextualHelp contextualHelp14 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/clone-ticket");
    ((RibbonItemData) pushButtonData15).SetContextualHelp(contextualHelp14);
    PushButtonData pushButtonData16 = new PushButtonData("CAD Export", "CAD Export", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.AdminTools.CAM.LaserProjection");
    ((RibbonItemData) pushButtonData16).ToolTip = "Export CAD files.";
    ((ButtonData) pushButtonData16).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Laser.png");
    ((ButtonData) pushButtonData16).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Laser_Small.png");
    ContextualHelp contextualHelp15 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/laser-export");
    ((RibbonItemData) pushButtonData16).SetContextualHelp(contextualHelp15);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddPushButton(pushButtonData5);
      pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddPushButton(pushButtonData11);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData13);
      pulldownButton.AddPushButton(pushButtonData12);
      pulldownButton.AddPushButton(pushButtonData14);
      pulldownButton.AddPushButton(pushButtonData15);
      pulldownButton.AddPushButton(pushButtonData16);
    }
    else
    {
      if (!AppRibbonSetup.RibbonTicketToolsPanel)
        return;
      if (AppRibbonSetup.RibbonTicketToolsAnnotateEmbedLong)
        pulldownButton.AddPushButton(pushButtonData2);
      if (AppRibbonSetup.RibbonTicketToolsAnnotateEmbedShort)
        pulldownButton.AddPushButton(pushButtonData3);
      if (AppRibbonSetup.RibbonTicketToolsControlNumberPopulator)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonTicketToolsTicketBomDuplicatesSchedule)
        pulldownButton.AddPushButton(pushButtonData7);
      if (AppRibbonSetup.RibbonTicketToolsTicketBomPartsList)
        pulldownButton.AddPushButton(pushButtonData4);
      if (AppRibbonSetup.RibbonTicketToolsTicketTitleBlockPopulator)
        pulldownButton.AddPushButton(pushButtonData8);
      if (AppRibbonSetup.RibbonTicketToolsBatchTicketTitleBlockPopulator)
        pulldownButton.AddPushButton(pushButtonData9);
      if (AppRibbonSetup.RibbonTicketToolsCopyTicketViews)
        pulldownButton.AddPushButton(pushButtonData10);
      if (AppRibbonSetup.RibbonTicketToolsFindReferringViews)
        pulldownButton.AddPushButton(pushButtonData11);
      if (AppRibbonSetup.RibbonTicketToolsTicketPopulator)
        pulldownButton.AddPushButton(pushButtonData5);
      if (AppRibbonSetup.RibbonTicketToolsBatchTicketPopulator)
        pulldownButton.AddPushButton(pushButtonData6);
      pulldownButton.AddSeparator();
      if (AppRibbonSetup.RibbonTicketToolsAutoDim)
        pulldownButton.AddPushButton(pushButtonData13);
      if (AppRibbonSetup.RibbonTicketToolsCopyTicketAnnotations)
        pulldownButton.AddPushButton(pushButtonData12);
      if (AppRibbonSetup.RibbonTicketToolsAutoTicketGeneration)
        pulldownButton.AddPushButton(pushButtonData14);
      if (AppRibbonSetup.RibbonTicketToolsCloneTicket)
        pulldownButton.AddPushButton(pushButtonData15);
      if (!AppRibbonSetup.RibbonTicketToolsLaserExport)
        return;
      pulldownButton.AddPushButton(pushButtonData16);
    }
  }

  private static void AddHardwareDetailMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Hardware Tools").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Hardware Tools", "Hardware Tools")) as PulldownButton;
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Hardware_Tools.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Hardware_Tools_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("Hardware Detail", "Hardware Detail", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.HardwareDetail.HardwareDetail");
    ((RibbonItemData) pushButtonData1).ToolTip = "Generate hardware detail drawings.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("Hardware_Detail.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("Hardware_Detail_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/hardware-detail");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("HardwareDetailTemplateCreator", "Hardware Detail Template Manager", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.HardwareDetail.HWDCreateTemplate");
    ((RibbonItemData) pushButtonData2).ToolTip = "Create Hardware Detail Template for the Current Sheet.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("HardwareDetailTemplateManager.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("HardwareDetailTemplateManager_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/hardware-detail-template-creator");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData3 = new PushButtonData("HW Title Block Populator Settings", "HW Title Block Populator Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.HWTitleBlockPopulatorUserSetting_Command");
    ((RibbonItemData) pushButtonData3).ToolTip = "Hardware Detail Title Block Populator Settings Dialog";
    ((RibbonItemData) pushButtonData3).LongDescription = "Allows user to map parameters from the hardware detail elements to parameters on the titleblock";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("HW_Title_Block_Popular_Settings.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("HW_Title_Block_Popular_Settings_Small.png");
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/hw-title-block-populator-settings");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData4 = new PushButtonData("Hardware Detail BOM Settings", "Hardware Detail BOM Settings", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.UserSettingTools.HWDBOMUserSetting_Command");
    ((RibbonItemData) pushButtonData4).ToolTip = "Hardware Detail BOM User Settings Dialog";
    ((RibbonItemData) pushButtonData4).LongDescription = "Allows user to specify which note schedules should be automatically duplicated by the Hardware Detail BOM Duplicate Schedules command";
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("HW_BOM_Settings.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("HW_BOM_Settings_Small.png");
    ContextualHelp contextualHelp4 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/hardware-detail-bom-settings");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp4);
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddPushButton(pushButtonData2);
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddPushButton(pushButtonData4);
    }
    else
    {
      if (AppRibbonSetup.RibbonHardwareToolsHardwareDetail)
        pulldownButton.AddPushButton(pushButtonData1);
      if (AppRibbonSetup.RibbonHardwareToolsShowHWDetailTemplateCreator)
        pulldownButton.AddPushButton(pushButtonData2);
      if (AppRibbonSetup.RibbonHardwareToolsShowHWDTBPSettings)
        pulldownButton.AddPushButton(pushButtonData3);
      if (!AppRibbonSetup.RibbonHardwareToolsShowHWDTicketBOMSettings)
        return;
      pulldownButton.AddPushButton(pushButtonData4);
    }
  }

  private static void AddTicketManagerMenu(UIControlledApplication application, string tabName)
  {
    PulldownButton pulldownButton = application.CreateRibbonPanel(tabName, "Ticket Manager").AddItem((RibbonItemData) new PulldownButtonData(AppRibbonSetup.Prefix + "Ticket Manager", "Ticket Manager")) as PulldownButton;
    ((RibbonItem) pulldownButton).ToolTip = "Tools related to Ticket Management.";
    ((RibbonButton) pulldownButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketManager.png");
    ((RibbonButton) pulldownButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketManager_Small.png");
    PushButtonData pushButtonData1 = new PushButtonData("Ticket Manager", "Ticket Manager", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TicketManager.TicketManager");
    ((RibbonItemData) pushButtonData1).ToolTip = "View Ticket information for the entire model in a single table driven view.";
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketManager.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketManager_Small.png");
    ContextualHelp contextualHelp1 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-manager");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp1);
    PushButtonData pushButtonData2 = new PushButtonData("Ticket Created", "Ticket Created", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.UpdtTktCreatedInformation");
    ((RibbonItemData) pushButtonData2).ToolTip = "Add information to the Ticket to mark that it has been Created.";
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketCreated.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketCreated_Small.png");
    PushButtonData pushButtonData3 = new PushButtonData("Ticket Detailed", "Ticket Detailed", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.UpdtTktDetailedInformation");
    ((RibbonItemData) pushButtonData3).ToolTip = "Add information to the Ticket to mark that it has been Detailed and is ready for checking.";
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketCompleted.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketCompleted_Small.png");
    ContextualHelp contextualHelp2 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-detailed");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp2);
    PushButtonData pushButtonData4 = new PushButtonData("Ticket Export", "Ticket Export", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.TicketManager.TicketManagerExport");
    ((RibbonItemData) pushButtonData4).ToolTip = "Export ticket information.";
    ContextualHelp contextualHelp3 = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/ticket-manager");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp3);
    PushButtonData pushButtonData5 = new PushButtonData("Ticket Released", "Ticket Released", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.UpdtTktReleasedStatus");
    ((RibbonItemData) pushButtonData5).ToolTip = "Add information to the Ticket to mark that is has been Released for production.";
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketReleased.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketReleased_Small.png");
    PushButtonData pushButtonData6 = new PushButtonData("Ticket Flag Review", "Ticket Flag Review", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.UpdtTktFlagStatus");
    ((RibbonItemData) pushButtonData6).ToolTip = "Review Ticket flags.";
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketFlagReview.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("TicketFlagReview_Small.png");
    PushButtonData pushButtonData7 = new PushButtonData("Eng/Draft Check Utility", "Engineer/Drafting Check Utility", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CheckFlagger.FlagUtilCommand");
    ((RibbonItemData) pushButtonData7).ToolTip = "Manage Check status of assemblies.";
    PushButtonData pushButtonData8 = new PushButtonData("Engineer Check All", "Engineer Check All", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CheckFlagger.EngCheckCommand");
    ((RibbonItemData) pushButtonData7).ToolTip = "Engineering check all selected pieces.";
    PushButtonData pushButtonData9 = new PushButtonData("Drafting Check All", "Drafting Check All", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CheckFlagger.DraftCheckCommand");
    ((RibbonItemData) pushButtonData7).ToolTip = "Drafting check all selected pieces.";
    PushButtonData pushButtonData10 = new PushButtonData("Request Revision All", "Request Revision All", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CheckFlagger.NeedRevAllCommand");
    ((RibbonItemData) pushButtonData7).ToolTip = "Mark all selected pieces as needing revision.";
    PushButtonData pushButtonData11 = new PushButtonData("Issue Revision All", "Issue Revision All", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.TicketTools.CheckFlagger.IssueRevAllCommand");
    ((RibbonItemData) pushButtonData7).ToolTip = "Issue revision for all selected pieces.";
    if (AppRibbonSetup.s_bIsDebug)
    {
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData3);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddPushButton(pushButtonData11);
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
    }
    else
    {
      if (!AppRibbonSetup.RibbonTicketManagerPanel)
        return;
      pulldownButton.AddPushButton(pushButtonData1);
      pulldownButton.AddSeparator();
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData3);
      if (!AppRibbonSetup.PTACTools)
        return;
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData7);
      pulldownButton.AddPushButton(pushButtonData8);
      pulldownButton.AddPushButton(pushButtonData9);
      pulldownButton.AddPushButton(pushButtonData10);
      pulldownButton.AddPushButton(pushButtonData11);
      if (!AppRibbonSetup.RibbonTicketManagerExport)
        return;
      pulldownButton.AddSeparator();
      pulldownButton.AddPushButton(pushButtonData4);
    }
  }

  private static void AddVisibilityMenu(UIControlledApplication application, string tabName)
  {
    ContextualHelp contextualHelp = new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/visibility-on-off");
    RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "Visibility");
    PushButtonData pushButtonData1 = new PushButtonData(AppRibbonSetup.Prefix + "CIP", "CIP", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleCIP");
    ((ButtonData) pushButtonData1).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("CIPON.png");
    ((ButtonData) pushButtonData1).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("CIPON_Small.png");
    ((RibbonItemData) pushButtonData1).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData2 = new PushButtonData(AppRibbonSetup.Prefix + "Embeds", "Embeds", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleEmbeds");
    ((ButtonData) pushButtonData2).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("EmbedON.png");
    ((ButtonData) pushButtonData2).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("EmbedON_Small.png");
    ((RibbonItemData) pushButtonData2).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData3 = new PushButtonData(AppRibbonSetup.Prefix + "Erection", "Erection", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleErection");
    ((ButtonData) pushButtonData3).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("ErectionON.png");
    ((ButtonData) pushButtonData3).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("ErectionON_Small.png");
    ((RibbonItemData) pushButtonData3).SetContextualHelp(contextualHelp);
    ribbonPanel.AddStackedItems((RibbonItemData) pushButtonData1, (RibbonItemData) pushButtonData2, (RibbonItemData) pushButtonData3);
    PushButtonData pushButtonData4 = new PushButtonData(AppRibbonSetup.Prefix + "Foundation", "Foundation", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleFoundation");
    ((ButtonData) pushButtonData4).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("FootingON.png");
    ((ButtonData) pushButtonData4).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("FootingON_Small.png");
    ((RibbonItemData) pushButtonData4).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData5 = new PushButtonData(AppRibbonSetup.Prefix + "Grout", "Grout", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleGrout");
    ((ButtonData) pushButtonData5).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("GroutON.png");
    ((ButtonData) pushButtonData5).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("GroutON_Small.png");
    ((RibbonItemData) pushButtonData5).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData6 = new PushButtonData(AppRibbonSetup.Prefix + "Insulation", "Insulation", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleInsulation");
    ((ButtonData) pushButtonData6).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationON.png");
    ((ButtonData) pushButtonData6).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("InsulationON_Small.png");
    ((RibbonItemData) pushButtonData6).SetContextualHelp(contextualHelp);
    ribbonPanel.AddStackedItems((RibbonItemData) pushButtonData4, (RibbonItemData) pushButtonData5, (RibbonItemData) pushButtonData6);
    PushButtonData pushButtonData7 = new PushButtonData(AppRibbonSetup.Prefix + "Lifting", "Lifting", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleLifting");
    ((ButtonData) pushButtonData7).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("LiftingON.png");
    ((ButtonData) pushButtonData7).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("LiftingON_Small.png");
    ((RibbonItemData) pushButtonData7).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData8 = new PushButtonData(AppRibbonSetup.Prefix + "Mesh", "Mesh", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleMesh");
    ((ButtonData) pushButtonData8).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("MeshON.png");
    ((ButtonData) pushButtonData8).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("MeshON_Small.png");
    ((RibbonItemData) pushButtonData8).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData9 = new PushButtonData(AppRibbonSetup.Prefix + "Rebar", "Rebar", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleRebar");
    ((ButtonData) pushButtonData9).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("RebarON.png");
    ((ButtonData) pushButtonData9).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("RebarON_Small.png");
    ((RibbonItemData) pushButtonData9).SetContextualHelp(contextualHelp);
    ribbonPanel.AddStackedItems((RibbonItemData) pushButtonData7, (RibbonItemData) pushButtonData8, (RibbonItemData) pushButtonData9);
    PushButtonData pushButtonData10 = new PushButtonData(AppRibbonSetup.Prefix + "Flat", "Flat", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleFlat");
    ((ButtonData) pushButtonData10).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("FlatON.png");
    ((ButtonData) pushButtonData10).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("FlatON_Small.png");
    ((RibbonItemData) pushButtonData10).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData11 = new PushButtonData(AppRibbonSetup.Prefix + "Warped", "Warped", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleWarped");
    ((ButtonData) pushButtonData11).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("WarpedON.png");
    ((ButtonData) pushButtonData11).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("WarpedON_Small.png");
    ((RibbonItemData) pushButtonData11).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData12 = new PushButtonData(AppRibbonSetup.Prefix + "WWF", "WWF", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleWWF");
    ((ButtonData) pushButtonData12).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("WWFON.png");
    ((ButtonData) pushButtonData12).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("WWFON_Small.png");
    ((RibbonItemData) pushButtonData12).SetContextualHelp(contextualHelp);
    ribbonPanel.AddStackedItems((RibbonItemData) pushButtonData12, (RibbonItemData) pushButtonData10, (RibbonItemData) pushButtonData11);
    PushButtonData pushButtonData13 = new PushButtonData(AppRibbonSetup.Prefix + "Voids", "Voids", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.ToggleVoids");
    ((ButtonData) pushButtonData13).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("VoidON.png");
    ((ButtonData) pushButtonData13).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("VoidON_Small.png");
    ((RibbonItemData) pushButtonData13).SetContextualHelp(contextualHelp);
    PushButtonData pushButtonData14 = new PushButtonData(AppRibbonSetup.Prefix + "Opacity", "Opacity", AppRibbonSetup.ExecutingAssemblyPath, "EDGE.VisibilityTools.EditPrecastOpacity");
    ((ButtonData) pushButtonData14).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage("PrecastProduct.png");
    ((ButtonData) pushButtonData14).Image = (ImageSource) AppRibbonSetup.GetBitmapImage("PrecastProduct_Small.png");
    ((RibbonItemData) pushButtonData14).SetContextualHelp(new ContextualHelp((ContextualHelpType) 2, "http://www.edge.ptac.com/precast-product-transparency"));
    ribbonPanel.AddStackedItems((RibbonItemData) pushButtonData13, (RibbonItemData) pushButtonData14);
  }

  public static PushButton SelectVisibilityButton(
    ExternalCommandData commandData,
    string categoryName)
  {
    string str = !AppRibbonSetup.s_bIsDebug ? "EDGE^R" : "d_EDGE^R";
    RibbonPanel ribbonPanel1 = (RibbonPanel) null;
    foreach (RibbonPanel ribbonPanel2 in commandData.Application.GetRibbonPanels(str))
    {
      if (ribbonPanel2.Name == "Visibility")
      {
        ribbonPanel1 = ribbonPanel2;
        break;
      }
    }
    if (ribbonPanel1 == null)
    {
      TaskDialog.Show("ERROR", "No Visibility Ribbon Panel Found");
      return (PushButton) null;
    }
    foreach (RibbonItem ribbonItem in (IEnumerable<RibbonItem>) ribbonPanel1.GetItems())
    {
      if (ribbonItem.ItemText == categoryName)
        return ribbonItem as PushButton;
    }
    TaskDialog.Show("ERROR", "Requested ribbon button not found.");
    return (PushButton) null;
  }

  public static void ToggleVisibilityButtonStatus(
    ExternalCommandData commandData,
    string categoryName)
  {
    PushButton pushButton = AppRibbonSetup.SelectVisibilityButton(commandData, categoryName);
    string currentImage = VisibilityToggles.visTogglesDict[categoryName].getCurrentImage();
    ((RibbonButton) pushButton).LargeImage = (ImageSource) AppRibbonSetup.GetBitmapImage(currentImage);
    ((RibbonButton) pushButton).Image = (ImageSource) AppRibbonSetup.GetBitmapImage($"{currentImage.Substring(0, currentImage.Length - 4)}_Small{currentImage.Substring(currentImage.Length - 4, 4)}");
  }
}
