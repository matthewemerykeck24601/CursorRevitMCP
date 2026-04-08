// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.EdgePreferencesUserSettingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.ApplicationRibbon;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class EdgePreferencesUserSettingWindow : Window, IComponentConnector
{
  private List<AdminItem> AdminNameItems = new List<AdminItem>();
  private bool edgeRebarMarkUpdater;
  private bool edgeModelLocking;
  private bool adminPanel;
  private bool adminSharedParameterUpdater;
  private bool adminWarningAnalyzer;
  private bool adminEmbedClashVerification;
  private bool adminBphCphParameterErrorFinder;
  private bool adminTicketTemplateCreator;
  private bool adminBulkFamilyUpdater;
  private bool adminCloudSettings;
  private bool adminCAMsettings;
  private bool adminEdgeBrowserSettings;
  private bool familyToolsPanel;
  private bool ticketManagerPanel;
  private bool visibilityPanel;
  private bool annotationPanel;
  private bool annotationTextUtilities;
  private bool annotationDimensionUtilities;
  private bool annotationAutoDimEDWG;
  private bool freezeActiveView;
  private bool geometryPanel;
  private bool geometryAddonHostingUpdater;
  private bool geometryMultiVoidTools;
  private bool geometryWarpedParameterCopier;
  private bool geometryMarkOppTools;
  private bool geometryMarkVerificationInitial;
  private bool geometryMarkVerificationExisting;
  private bool geometryVoidHostingUpdater;
  private bool geometryGetCentroid;
  private bool geometryNewReferencePoint;
  private bool geometryAutoWarping;
  private bool insulationPanel;
  private bool insulationInsulationRemoval;
  private bool insulationAutoPlacement;
  private bool insulationManualPlacement;
  private bool insulationAutoPinPlacement;
  private bool insulationInsulationMarking;
  private bool insulationInsulationExport;
  private bool insulationInsulationDrawingMaster;
  private bool insulationInsulationDrawingPerWall;
  private bool insulationInsulationDrawing;
  private bool assemblyPanel;
  private bool assemblyCreationUpdatingRenaming;
  private bool assemblyCreationUpdatingRevit;
  private bool assemblyCopyReinforcing;
  private bool assemblyMarkAsReinforced;
  private bool assemblyTopAsCast;
  private bool assemblyMatchTopAsCast;
  private bool ticketToolsPanel;
  private bool ticketToolsAnnotateEmbedLong;
  private bool ticketToolsAnnotateEmbedShort;
  private bool ticketToolsControlNumberPopulator;
  private bool ticketToolsTicketBomDuplicatesSchedule;
  private bool ticketToolsTicketTitleBlockPopulator;
  private bool ticketToolsBatchTicketTitleBlockPopulator;
  private bool ticketToolsCopyTicketViews;
  private bool ticketToolsCopyTicketAnnotations;
  private bool ticketToolsFindReferringViews;
  private bool ticketToolsTicketPopulator;
  private bool ticketToolsBatchTicketPopulator;
  private bool ticketToolsAutoDim;
  private bool ticketToolsAutoTicket;
  private bool ticketToolsCloneTicket;
  private bool ticketToolsLaserExport;
  private bool selectionAndPinningPanel;
  private bool selectionAndPinningGrids;
  private bool selectionAndPinningLevels;
  private bool selectionAndPinningSelectSFElements;
  private bool schedulingPanel;
  private bool schedulingControlNumberIncrementor;
  private bool schedulingBomProductHosting;
  private bool schedulingConstructionProductHosting;
  private bool schedulingScheduleSequenceByView;
  private bool schedulingErectionSequence;
  private bool userSettingsTicketAndDimenaionSettings;
  private bool userSettingsRebarSettings;
  private bool userSettingsMarkVerificationSettings;
  private bool userSettingsTicketManagerCustominazationSettings;
  private bool userSettings_TBPSettings;
  private bool userSettingsTicketBOMSettings;
  private bool userSettingsCadExportSettings;
  private bool userSettingsInsulationDrawingSettings;
  private bool hardwareToolsPanel;
  private bool hardwareToolsHardwareDetail;
  private bool hardwareToolsHardwareDetailTemplateManager;
  private bool hardwareToolsHWTitleBlockPopulatorSettings;
  private bool hardwareToolsHardwareDetialBomSettings;
  internal Expander FolderExpander;
  internal System.Windows.Controls.TextBox TicketTemplateSettingsFile;
  internal System.Windows.Controls.Button TicketTemplateBrowse;
  internal System.Windows.Controls.TextBox HardwareTemplateSettingsFile;
  internal System.Windows.Controls.Button HardwareTemplateBrowse;
  internal System.Windows.Controls.TextBox RebarSettingsFolderPath;
  internal System.Windows.Controls.Button RebarSettingFolderBrowse;
  internal System.Windows.Controls.TextBox MarkPrefixFolderPath;
  internal System.Windows.Controls.Button MarkPrefixFolderBrowse;
  internal System.Windows.Controls.TextBox TicketSettingsFolderPath;
  internal System.Windows.Controls.Button AutoTicketSettingsBrowse;
  internal System.Windows.Controls.TextBox TKTBOMFolderPath;
  internal System.Windows.Controls.Button TKTBOMFolderBrowse;
  internal System.Windows.Controls.TextBox TBPopFolderPath;
  internal System.Windows.Controls.Button TBPopFolderBrowse;
  internal System.Windows.Controls.TextBox HWBOMFolderPath;
  internal System.Windows.Controls.Button HWBOMFolderBrowse;
  internal System.Windows.Controls.TextBox HWTBPopFolderPath;
  internal System.Windows.Controls.Button HWTBPopFolderBrowse;
  internal System.Windows.Controls.TextBox TMCFolderPath;
  internal System.Windows.Controls.Button TMCFolderBrowse;
  internal System.Windows.Controls.TextBox CADExFolderPath;
  internal System.Windows.Controls.Button CADExFolderBrowse;
  internal System.Windows.Controls.TextBox InsulationDrawingSettingsFolderPath;
  internal System.Windows.Controls.Button InsulationDrawingSettingsFolderBrowse;
  internal Expander RebarExpander;
  internal System.Windows.Controls.CheckBox Edge_Rebar_Mark_Updater;
  internal Expander RibbonExpander;
  internal System.Windows.Controls.CheckBox Annotation_Panel;
  internal Grid AnnotationPanelItem;
  internal System.Windows.Controls.CheckBox Annotation_TextUtilities;
  internal System.Windows.Controls.CheckBox Annotation_DimensionUtilities;
  internal System.Windows.Controls.CheckBox Annotation_AutoDim_EDWG;
  internal System.Windows.Controls.CheckBox Freeze_Active_View;
  internal System.Windows.Controls.CheckBox Geometry_Panel;
  internal Grid GeometryPanelItem;
  internal System.Windows.Controls.CheckBox Geometry_AddonHostingUpdater;
  internal System.Windows.Controls.CheckBox Geometry_MultiVoidTools;
  internal System.Windows.Controls.CheckBox Geometry_WarpedParameterCopier;
  internal System.Windows.Controls.CheckBox Geometry_MarkOppTools;
  internal System.Windows.Controls.CheckBox Geometry_MarkVerificationInitial;
  internal System.Windows.Controls.CheckBox Geometry_MarkVerificationExisting;
  internal System.Windows.Controls.CheckBox Geometry_VoidHostingUpdater;
  internal System.Windows.Controls.CheckBox Geometry_NewReferencePoint;
  internal System.Windows.Controls.CheckBox Geometry_GetCentroid;
  internal System.Windows.Controls.CheckBox Geometry_AutoWarping;
  internal System.Windows.Controls.CheckBox Insulation_Panel;
  internal Grid InsulationPanelItem;
  internal System.Windows.Controls.CheckBox Insulation_InsulationRemoval;
  internal System.Windows.Controls.CheckBox Insulation_AutoInsulationPlacement;
  internal System.Windows.Controls.CheckBox Insulation_ManualInsulationPlacement;
  internal System.Windows.Controls.CheckBox Insulation_AutoPinPlacement;
  internal System.Windows.Controls.CheckBox Insulation_InsulationMarking;
  internal System.Windows.Controls.CheckBox Insulation_InsulationExport;
  internal System.Windows.Controls.CheckBox Insulation_InsulationDrawingMaster;
  internal System.Windows.Controls.CheckBox Insulation_InsulationDrawingPerWall;
  internal System.Windows.Controls.CheckBox Insulation_InsulationDrawing;
  internal System.Windows.Controls.CheckBox Visibility_Panel;
  internal System.Windows.Controls.CheckBox SelectionAndPinning_Panel;
  internal Grid SelectionAndPinningPanelItem;
  internal System.Windows.Controls.CheckBox SelectionAndPinning_Grids;
  internal System.Windows.Controls.CheckBox SelectionAndPinning_Levels;
  internal System.Windows.Controls.CheckBox SelectionAndPinning_SelectSFElements;
  internal System.Windows.Controls.CheckBox Scheduling_Panel;
  internal Grid SchedulingPanelItem;
  internal System.Windows.Controls.CheckBox Scheduling_ControlNumberIncrementor;
  internal System.Windows.Controls.CheckBox Scheduling_BomProductHosting;
  internal System.Windows.Controls.CheckBox Scheduling_ConstructionProductHosting;
  internal System.Windows.Controls.CheckBox Scheduling_ScheduleSequenceByView;
  internal System.Windows.Controls.CheckBox Scheduling_Erection_Sequence;
  internal System.Windows.Controls.CheckBox Assembly_Panel;
  internal Grid AssemblyPanelItem;
  internal System.Windows.Controls.CheckBox Assembly_CreationUpdatingRenaming;
  internal System.Windows.Controls.CheckBox Assembly_CreationUpdatingRevit;
  internal System.Windows.Controls.CheckBox Assembly_CopyReinforcing;
  internal System.Windows.Controls.CheckBox Assembly_MarkAsReinforced;
  internal System.Windows.Controls.CheckBox Assembly_TopAsCast;
  internal System.Windows.Controls.CheckBox Assembly_MatchTopAsCast;
  internal System.Windows.Controls.CheckBox TicketTools_Panel;
  internal Grid TicketToolsPanelItem;
  internal System.Windows.Controls.CheckBox TicketTools_AnnotateEmbedLong;
  internal System.Windows.Controls.CheckBox TicketTools_AnnotateEmbedShort;
  internal System.Windows.Controls.CheckBox TicketTools_ControlNumberPopulator;
  internal System.Windows.Controls.CheckBox TicketTools_TicketBomDuplicatesSchedule;
  internal System.Windows.Controls.CheckBox TicketTools_TicketTitleBlockPopulator;
  internal System.Windows.Controls.CheckBox TicketTools_BatchTicketTitleBlockPopulator;
  internal System.Windows.Controls.CheckBox TicketTools_CopyTicketViews;
  internal System.Windows.Controls.CheckBox TicketTools_CopyTicketAnnotations;
  internal System.Windows.Controls.CheckBox TicketTools_FindReferringViews;
  internal System.Windows.Controls.CheckBox TicketTools_TicketPopulator;
  internal System.Windows.Controls.CheckBox TicketTools_BatchTicketPopulator;
  internal System.Windows.Controls.CheckBox TicketTools_AutoDimension;
  internal System.Windows.Controls.CheckBox TicketTools_AutoTicket;
  internal System.Windows.Controls.CheckBox TicketTools_CloneTicket;
  internal System.Windows.Controls.CheckBox TicketTools_LaserExport;
  internal System.Windows.Controls.CheckBox HardwareTools_Panel;
  internal Grid HardwareToolsPanelItem;
  internal System.Windows.Controls.CheckBox HardwareTools_HardwareDetail;
  internal System.Windows.Controls.CheckBox HardwareTools_HardwareDetailTemplateManager;
  internal System.Windows.Controls.CheckBox HardwareTools_HWTitleBlockPopulatorSettings;
  internal System.Windows.Controls.CheckBox HardwareTools_HardwareDetailBOMSettings;
  internal System.Windows.Controls.CheckBox TicketManager_Panel;
  internal System.Windows.Controls.CheckBox FamilyTools_Panel;
  internal Grid UserSettingsPanelItem;
  internal System.Windows.Controls.CheckBox UserSettings_TicketAndDimensionSettings;
  internal System.Windows.Controls.CheckBox UserSettings_RebarSettings;
  internal System.Windows.Controls.CheckBox UserSettings_MarkVerificationSettings;
  internal System.Windows.Controls.CheckBox UserSettings_TicketManagerCustomizationSettings;
  internal System.Windows.Controls.CheckBox UserSettings_TBPSettings;
  internal System.Windows.Controls.CheckBox UserSettings_TicketBOMSettings;
  internal System.Windows.Controls.CheckBox UserSettings_CADExportSettings;
  internal System.Windows.Controls.CheckBox UserSettings_InsulationDrawingSettings;
  internal System.Windows.Controls.CheckBox Admin_Panel;
  internal Grid AdminPanelItem;
  internal System.Windows.Controls.CheckBox Admin_SharedParameterUpdater;
  internal System.Windows.Controls.CheckBox Admin_WarningAnalyzer;
  internal System.Windows.Controls.CheckBox Admin_EmbedClashVerification;
  internal System.Windows.Controls.CheckBox Admin_BphCphParameterErrorFinder;
  internal System.Windows.Controls.CheckBox Admin_TicketTemplateCreator;
  internal System.Windows.Controls.CheckBox Admin_BulkFamilyUpdater;
  internal System.Windows.Controls.CheckBox Admin_CloudSettings;
  internal System.Windows.Controls.CheckBox Admin_CAMSettings;
  internal System.Windows.Controls.CheckBox Admin_EDGEBrowserSettings;
  private bool _contentLoaded;

  public EdgePreferencesUserSettingWindow(IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    EdgePrefsReader.ReadEdgePrefsFile();
    this.TicketTemplateSettingsFile.Text = App.TicketTemplateSettingsPath;
    this.HardwareTemplateSettingsFile.Text = App.HardwareDetailTemplateSettingsPath;
    this.RebarSettingsFolderPath.Text = App.RebarSettingsFolderPath;
    this.MarkPrefixFolderPath.Text = App.MarkPrefixFolderPath;
    this.TKTBOMFolderPath.Text = App.TKTBOMFolderPath;
    this.HWBOMFolderPath.Text = App.HWBOMFolderPath;
    this.TBPopFolderPath.Text = App.TBPopFolderPath;
    this.HWTBPopFolderPath.Text = App.HWTBPopFolderPath;
    this.TMCFolderPath.Text = App.TMCFolderPath;
    this.CADExFolderPath.Text = App.CADExFolderPath;
    this.InsulationDrawingSettingsFolderPath.Text = App.InsulationDrawingPath;
    this.TicketSettingsFolderPath.Text = App.AutoTicketFolderPath;
    this.Edge_Rebar_Mark_Updater.IsChecked = new bool?(App.RebarMarkAutomation);
  }

  private void FixPanels(bool handleVisibility = false)
  {
    bool? isChecked1 = this.Annotation_Panel.IsChecked;
    bool flag1 = false;
    this.annotationPanel = !(isChecked1.GetValueOrDefault() == flag1 & isChecked1.HasValue);
    if (handleVisibility && !this.annotationPanel)
      this.AnnotationPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked2 = this.Geometry_Panel.IsChecked;
    bool flag2 = false;
    this.geometryPanel = !(isChecked2.GetValueOrDefault() == flag2 & isChecked2.HasValue);
    if (handleVisibility && !this.geometryPanel)
      this.GeometryPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked3 = this.Insulation_Panel.IsChecked;
    bool flag3 = false;
    this.insulationPanel = !(isChecked3.GetValueOrDefault() == flag3 & isChecked3.HasValue);
    if (handleVisibility && !this.insulationPanel)
      this.InsulationPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked4 = this.Visibility_Panel.IsChecked;
    bool flag4 = false;
    this.visibilityPanel = !(isChecked4.GetValueOrDefault() == flag4 & isChecked4.HasValue);
    bool? isChecked5 = this.SelectionAndPinning_Panel.IsChecked;
    bool flag5 = false;
    this.selectionAndPinningPanel = !(isChecked5.GetValueOrDefault() == flag5 & isChecked5.HasValue);
    if (handleVisibility && !this.selectionAndPinningPanel)
      this.SelectionAndPinningPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked6 = this.Scheduling_Panel.IsChecked;
    bool flag6 = false;
    this.schedulingPanel = !(isChecked6.GetValueOrDefault() == flag6 & isChecked6.HasValue);
    if (handleVisibility && !this.schedulingPanel)
      this.SchedulingPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked7 = this.Assembly_Panel.IsChecked;
    bool flag7 = false;
    this.assemblyPanel = !(isChecked7.GetValueOrDefault() == flag7 & isChecked7.HasValue);
    if (handleVisibility && !this.assemblyPanel)
      this.AssemblyPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked8 = this.TicketTools_Panel.IsChecked;
    bool flag8 = false;
    this.ticketToolsPanel = !(isChecked8.GetValueOrDefault() == flag8 & isChecked8.HasValue);
    if (handleVisibility && !this.ticketToolsPanel)
      this.TicketToolsPanelItem.Visibility = Visibility.Collapsed;
    if (handleVisibility && !this.hardwareToolsPanel)
      this.HardwareToolsPanelItem.Visibility = Visibility.Collapsed;
    bool? isChecked9 = this.TicketManager_Panel.IsChecked;
    bool flag9 = false;
    this.ticketManagerPanel = !(isChecked9.GetValueOrDefault() == flag9 & isChecked9.HasValue);
    bool? isChecked10 = this.FamilyTools_Panel.IsChecked;
    bool flag10 = false;
    this.familyToolsPanel = !(isChecked10.GetValueOrDefault() == flag10 & isChecked10.HasValue);
    bool? isChecked11 = this.Admin_Panel.IsChecked;
    bool flag11 = false;
    this.adminPanel = !(isChecked11.GetValueOrDefault() == flag11 & isChecked11.HasValue);
    if (!handleVisibility || this.adminPanel)
      return;
    this.AdminPanelItem.Visibility = Visibility.Collapsed;
  }

  private void ApplyButton_Click(object sender, RoutedEventArgs e)
  {
    string text1 = this.TicketTemplateSettingsFile.Text;
    string text2 = this.HardwareTemplateSettingsFile.Text;
    string text3 = this.RebarSettingsFolderPath.Text;
    string text4 = this.MarkPrefixFolderPath.Text;
    string text5 = this.TKTBOMFolderPath.Text;
    string text6 = this.HWBOMFolderPath.Text;
    string text7 = this.TBPopFolderPath.Text;
    string text8 = this.HWTBPopFolderPath.Text;
    string text9 = this.TMCFolderPath.Text;
    string text10 = this.CADExFolderPath.Text;
    string text11 = this.InsulationDrawingSettingsFolderPath.Text;
    string text12 = this.TicketSettingsFolderPath.Text;
    this.FixPanels();
    StringBuilder stringBuilder = new StringBuilder();
    if (!string.IsNullOrEmpty(text1))
      stringBuilder.AppendLine("*TicketTemplateSettingsFile*," + text1);
    if (!string.IsNullOrEmpty(text2))
      stringBuilder.AppendLine("*HardwareDetailsSettingsFile*," + text2);
    if (!string.IsNullOrEmpty(text3))
      stringBuilder.AppendLine("*RebarSettingsFolderPath*," + text3);
    if (!string.IsNullOrEmpty(text4))
      stringBuilder.AppendLine("*MarkPrefixFolderPath*," + text4);
    if (!string.IsNullOrEmpty(text5))
      stringBuilder.AppendLine("*TicketBOMSettingsFolderPath*," + text5);
    if (!string.IsNullOrEmpty(text6))
      stringBuilder.AppendLine("*HWBOMFolderPath*," + text6);
    if (!string.IsNullOrEmpty(text7))
      stringBuilder.AppendLine("*TitleBlockPopSettingsFolderPath*," + text7);
    if (!string.IsNullOrEmpty(text8))
      stringBuilder.AppendLine("*HWTBPopFolderPath*," + text8);
    if (!string.IsNullOrEmpty(text9))
      stringBuilder.AppendLine("*TicketManagerCustomSettingsFolderPath*," + text9);
    if (!string.IsNullOrEmpty(text10))
      stringBuilder.AppendLine("*CADExportSettingsFolderPath*," + text10);
    if (!string.IsNullOrEmpty(text11))
      stringBuilder.AppendLine("*InsulationDrawingSettingsFolderPath*," + text11);
    if (!string.IsNullOrEmpty(text12))
      stringBuilder.AppendLine("*TicketSettingsFolderPath*," + text12);
    stringBuilder.AppendLine("*Edge_Rebar_Mark_Updater*," + this.edgeRebarMarkUpdater.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_Panel*," + this.adminPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_SharedParameterUpdater*," + this.adminSharedParameterUpdater.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_WarningAnalyzer*," + this.adminWarningAnalyzer.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_EmbedClashVerification*," + this.adminEmbedClashVerification.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_BphCphParameterErrorFinder*," + this.adminBphCphParameterErrorFinder.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_TicketTemplateCreator*," + this.adminTicketTemplateCreator.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_Bulk_Family_Updater*," + this.adminBulkFamilyUpdater.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_TicketBOMSettings*," + this.userSettingsTicketBOMSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_TitleBlockPopulatorSettings*," + this.userSettings_TBPSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*Admin_CAMExport*," + this.adminCAMsettings.ToString().ToUpper());
    stringBuilder.AppendLine("*FamilyTools_Panel*," + this.familyToolsPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketManager_Panel*," + this.ticketManagerPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Visibility_Panel*," + this.visibilityPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Annotation_Panel*," + this.annotationPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Annotation_TextUtilities*," + this.annotationTextUtilities.ToString().ToUpper());
    stringBuilder.AppendLine("*Annotation_DimensionUtilities*," + this.annotationDimensionUtilities.ToString().ToUpper());
    stringBuilder.AppendLine("*Annotation_AutoDim_EDWG*," + this.annotationAutoDimEDWG.ToString().ToUpper());
    stringBuilder.AppendLine("*Annotation_Freeze_Active_View*," + this.freezeActiveView.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_Panel*," + this.geometryPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_AddonHostingUpdater*," + this.geometryAddonHostingUpdater.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_MultiVoidTools*," + this.geometryMultiVoidTools.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_WarpedParameterCopier*," + this.geometryWarpedParameterCopier.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_MarkOppTools*," + this.geometryMarkOppTools.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_MarkVerificationInitial*," + this.geometryMarkVerificationInitial.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_MarkVerificationExisting*," + this.geometryMarkVerificationExisting.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_VoidHostingUpdater*," + this.geometryVoidHostingUpdater.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_GetCentroid*," + this.geometryGetCentroid.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_NewReferencePoint*," + this.geometryNewReferencePoint.ToString().ToUpper());
    stringBuilder.AppendLine("*Geometry_AutoWarping*," + this.geometryAutoWarping.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_Panel*," + this.insulationPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_InsulationRemoval*," + this.insulationInsulationRemoval.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_AutoInsulationPlacement*," + this.insulationAutoPlacement.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_ManualInsulationPlacement*," + this.insulationManualPlacement.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_AutoPinPlacement*," + this.insulationAutoPinPlacement.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_InsulationMarking*," + this.insulationInsulationMarking.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_InsulationExport*," + this.insulationInsulationExport.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_InsulationDrawingMaster*," + this.insulationInsulationDrawingMaster.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_InsulationDrawingAssembly*," + this.insulationInsulationDrawingPerWall.ToString().ToUpper());
    stringBuilder.AppendLine("*Insulation_InsulationDrawingMark*," + this.insulationInsulationDrawing.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_Panel*," + this.assemblyPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_CreationUpdatingRenaming*," + this.assemblyCreationUpdatingRenaming.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_CreationUpdatingRevit*," + this.assemblyCreationUpdatingRevit.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_CopyReinforcing*," + this.assemblyCopyReinforcing.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_MarkAsReinforced*," + this.assemblyMarkAsReinforced.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_TopAsCast*," + this.assemblyTopAsCast.ToString().ToUpper());
    stringBuilder.AppendLine("*Assembly_MatchTopAsCast*," + this.assemblyMatchTopAsCast.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_Panel*," + this.ticketToolsPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_AnnotateEmbedLong*," + this.ticketToolsAnnotateEmbedLong.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_AnnotateEmbedShort*," + this.ticketToolsAnnotateEmbedShort.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_ControlNumberPopulator*," + this.ticketToolsControlNumberPopulator.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_TicketBomDuplicatesSchedule*," + this.ticketToolsTicketBomDuplicatesSchedule.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_TicketTitleBlockPopulator*," + this.ticketToolsTicketTitleBlockPopulator.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_BatchTicketTitleBlockPopulator*," + this.ticketToolsBatchTicketTitleBlockPopulator.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_CopyTicketViews*," + this.ticketToolsCopyTicketViews.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_CopyTicketAnnotations*," + this.ticketToolsCopyTicketAnnotations.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_FindReferringViews*," + this.ticketToolsFindReferringViews.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_TicketPopulator*," + this.ticketToolsTicketPopulator.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_BatchTicketPopulator*," + this.ticketToolsBatchTicketPopulator.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_AutoDim*," + this.ticketToolsAutoDim.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_AutoTicket*," + this.ticketToolsAutoTicket.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_CloneTicket*," + this.ticketToolsCloneTicket.ToString().ToUpper());
    stringBuilder.AppendLine("*TicketTools_LaserExport*," + this.ticketToolsLaserExport.ToString().ToUpper());
    stringBuilder.AppendLine("*SelectionAndPinning_Panel*," + this.selectionAndPinningPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*SelectionAndPinning_Grids*," + this.selectionAndPinningGrids.ToString().ToUpper());
    stringBuilder.AppendLine("*SelectionAndPinning_Levels*," + this.selectionAndPinningLevels.ToString().ToUpper());
    stringBuilder.AppendLine("*SelectionAndPinning_SelectSFElements*," + this.selectionAndPinningSelectSFElements.ToString().ToUpper());
    stringBuilder.AppendLine("*Scheduling_Panel*," + this.schedulingPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*Scheduling_ControlNumberIncrementor*," + this.schedulingControlNumberIncrementor.ToString().ToUpper());
    stringBuilder.AppendLine("*Scheduling_BomProductHosting*," + this.schedulingBomProductHosting.ToString().ToUpper());
    stringBuilder.AppendLine("*Scheduling_ConstructionProductHosting*," + this.schedulingConstructionProductHosting.ToString().ToUpper());
    stringBuilder.AppendLine("*Scheduling_ScheduleSequenceByView*," + this.schedulingScheduleSequenceByView.ToString().ToUpper());
    stringBuilder.AppendLine("*Scheduling_ErectionSequence*," + this.schedulingErectionSequence.ToString().ToUpper());
    stringBuilder.AppendLine("*EDGEBrowser*," + this.adminEdgeBrowserSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*UserSettings_TicketAndDimensionSettings*," + this.userSettingsTicketAndDimenaionSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*UserSettings_RebarSettings*," + this.userSettingsRebarSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*UserSettings_MarkVerificationSettings*," + this.userSettingsMarkVerificationSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*UserSettings_TicketManagerCustomizationSettings*," + this.userSettingsTicketManagerCustominazationSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*UserSettings_CADExportSettings*," + this.userSettingsCadExportSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*UserSettings_InsulationDrawingSettings*," + this.userSettingsInsulationDrawingSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*HardwareTools_Panel*," + this.hardwareToolsPanel.ToString().ToUpper());
    stringBuilder.AppendLine("*HardwareTools_HardwareDetail*," + this.hardwareToolsHardwareDetail.ToString().ToUpper());
    stringBuilder.AppendLine("*HardwareTools_HardwareDetailTemplateManager*," + this.hardwareToolsHardwareDetailTemplateManager.ToString().ToUpper());
    stringBuilder.AppendLine("*HardwareTools_HWTitleBlockPopulatorSettings*," + this.hardwareToolsHWTitleBlockPopulatorSettings.ToString().ToUpper());
    stringBuilder.AppendLine("*HardwareTools_HardwareDetailBomSettings*," + this.hardwareToolsHardwareDetialBomSettings.ToString().ToUpper());
    string str1 = "C:\\EDGEforRevit\\EDGE_Preferences.txt";
    FileInfo fileInfo = new FileInfo(str1);
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    bool flag5 = false;
    bool flag6 = false;
    bool flag7 = false;
    bool flag8 = false;
    bool flag9 = false;
    bool flag10 = false;
    bool flag11 = false;
    if (!fileInfo.Exists)
      return;
    try
    {
      using (StreamReader streamReader = new StreamReader(str1))
      {
        if (!fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
        {
          while (!streamReader.EndOfStream)
          {
            string str2 = streamReader.ReadLine();
            if (str2.Contains("Scheduling_MarkRebarByProduct"))
              flag1 = true;
            else if (str2.Contains("SpecialRebarScenario1"))
              flag2 = true;
            else if (str2.Contains("ShowSendToCloudOnAdmin"))
              this.adminCloudSettings = true;
            else if (str2.Contains("Edge_Cloud_Id"))
              flag3 = true;
            else if (str2.Contains("Admin_PEExport"))
              flag4 = true;
            else if (str2.Contains("Admin_PDMExport"))
              flag5 = true;
            else if (str2.Contains("*TicketTools_BatchTicketPopulator*"))
              flag7 = true;
            else if (!str2.Contains("*TicketTools_InsulationExport*"))
            {
              if (str2.Contains("*Edge_Model_Locking*"))
                flag6 = true;
              else if (str2.Contains("*Insulation_InsulationMarkingPerWall*"))
                flag9 = true;
              else if (str2.Contains("*Admin_TicketExport*"))
                flag10 = true;
              else if (str2.Contains("*PTAC_Tools*"))
                flag11 = true;
            }
          }
          streamReader.Close();
        }
        else
        {
          new TaskDialog("EDGE^R")
          {
            AllowCancellation = false,
            MainInstruction = "EDGE_Preferences file is Read Only",
            MainContent = $"Unable to read the EDGE_Preferences File in the specified location {str1} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
          }.Show();
          flag8 = true;
        }
      }
      if (flag1)
        stringBuilder.AppendLine("*Scheduling_MarkRebarByProduct*," + (AppRibbonSetup.RibbonSchedulingMarkRebarByProduct ? "TRUE" : "FALSE"));
      if (flag2)
        stringBuilder.Append("*SpecialRebarScenario1*," + (App.SpecialRebarScenario1 ? "TRUE" : "FALSE"));
      if (this.adminCloudSettings)
      {
        if (!flag3)
          stringBuilder.AppendLine("*Edge_Cloud_Id*,7");
        else
          stringBuilder.AppendLine("*Edge_Cloud_Id*," + App.Edge_Cloud_Id.ToString());
      }
      if (flag4)
        stringBuilder.AppendLine("*Admin_PEExport*," + (AppRibbonSetup.RibbonAdminShowExcelExport ? "TRUE" : "FALSE"));
      if (flag5)
        stringBuilder.AppendLine("*Admin_PDMExport*," + (AppRibbonSetup.RibbonAdminShowPDMExport ? "TRUE" : "FALSE"));
      if (flag10)
      {
        stringBuilder.AppendLine("*Admin_TicketExport*," + (AppRibbonSetup.RibbonTicketManagerExport ? "TRUE" : "FALSE"));
        if (AppRibbonSetup.RibbonTicketManagerExport)
          stringBuilder.AppendLine("*Admin_TicketExportFile*," + App.TicketManagerExportFile);
      }
      if (flag11)
        stringBuilder.AppendLine("*PTAC_Tools*," + (AppRibbonSetup.PTACTools ? "TRUE" : "FALSE"));
      if (flag7 && flag6)
        stringBuilder.AppendLine("*Edge_Model_Locking*," + (App.ModelLocking ? "TRUE" : "FALSE"));
      if (flag9)
        stringBuilder.AppendLine("*Insulation_InsulationMarkingPerWall*," + (AppRibbonSetup.RibbonInsulationMarkingPerWall ? "TRUE" : "FALSE"));
      if (!flag8)
      {
        using (StreamWriter streamWriter = new StreamWriter(str1))
          streamWriter.WriteLine(stringBuilder.ToString());
      }
      this.Close();
    }
    catch (Exception ex)
    {
      if (!ex.Message.Contains("process"))
        return;
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "EDGE_Preferences File Error.",
        MainContent = "Check EDGE_Preferences File: EDGE_Preferences.txt. Please ensure the file is not in use by another application and try again."
      }.Show();
    }
  }

  private void TicketTemplateBrowse_Click(object sender, RoutedEventArgs e)
  {
    OpenFileDialog openFileDialog = new OpenFileDialog();
    openFileDialog.Filter = "XML Files(*.xml) | *.xml";
    if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.TicketTemplateSettingsFile.Text = openFileDialog.FileName;
  }

  private void HardwareTemplateBrowse_Click(object sender, RoutedEventArgs e)
  {
    OpenFileDialog openFileDialog = new OpenFileDialog();
    openFileDialog.Filter = "XML Files(*.xml) | *.xml";
    if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.HardwareTemplateSettingsFile.Text = openFileDialog.FileName;
  }

  private void RebarSettingFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.RebarSettingsFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void MarkPrefixFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.MarkPrefixFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void TKTBOMFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.TKTBOMFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void HWBOMFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.HWBOMFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void TBPopFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.TBPopFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void HWTBPopFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.HWTBPopFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void TMCFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.TMCFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void CADExFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.CADExFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  public void InsulationDrawingSettingsFolderBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.InsulationDrawingSettingsFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void ModelLockSuperadminBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    string selectedPath = folderBrowserDialog.SelectedPath;
  }

  private void AutoTicketSettingsBrowse_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    if (folderBrowserDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
      return;
    this.TicketSettingsFolderPath.Text = folderBrowserDialog.SelectedPath;
  }

  private void DataGrid_SelectionChanged(object sender, RoutedEventArgs e)
  {
  }

  private void Edge_Rebar_Mark_Updater_Checked(object sender, RoutedEventArgs e)
  {
    this.edgeRebarMarkUpdater = true;
  }

  private void Edge_Rebar_Mark_Updater_UnChecked(object sender, RoutedEventArgs e)
  {
    this.edgeRebarMarkUpdater = false;
  }

  private void Admin_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.adminPanel = true;
    this.AdminPanelItem.Visibility = Visibility.Visible;
    this.Admin_SharedParameterUpdater.IsChecked = new bool?(true);
    this.Admin_WarningAnalyzer.IsChecked = new bool?(true);
    this.Admin_EmbedClashVerification.IsChecked = new bool?(true);
    this.Admin_BphCphParameterErrorFinder.IsChecked = new bool?(true);
    this.Admin_TicketTemplateCreator.IsChecked = new bool?(true);
    this.Admin_BulkFamilyUpdater.IsChecked = new bool?(true);
    this.Admin_CloudSettings.IsChecked = new bool?(true);
    this.Admin_CAMSettings.IsChecked = new bool?(true);
    this.Admin_EDGEBrowserSettings.IsChecked = new bool?(true);
  }

  private void Admin_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminPanel = false;
    this.AdminPanelItem.Visibility = Visibility.Collapsed;
    this.Admin_SharedParameterUpdater.IsChecked = new bool?(false);
    this.Admin_WarningAnalyzer.IsChecked = new bool?(false);
    this.Admin_EmbedClashVerification.IsChecked = new bool?(false);
    this.Admin_BphCphParameterErrorFinder.IsChecked = new bool?(false);
    this.Admin_TicketTemplateCreator.IsChecked = new bool?(false);
    this.Admin_BulkFamilyUpdater.IsChecked = new bool?(false);
    this.Admin_CloudSettings.IsChecked = new bool?(false);
    this.Admin_CAMSettings.IsChecked = new bool?(false);
    this.Admin_EDGEBrowserSettings.IsChecked = new bool?(false);
  }

  private void Admin_SharedParameterUpdater_Checked(object sender, RoutedEventArgs e)
  {
    this.adminSharedParameterUpdater = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_SharedParameterUpdater_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminSharedParameterUpdater = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_WarningAnalyzer_Checked(object sender, RoutedEventArgs e)
  {
    this.adminWarningAnalyzer = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_WarningAnalyzer_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminWarningAnalyzer = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_EmbedClashVerification_Checked(object sender, RoutedEventArgs e)
  {
    this.adminEmbedClashVerification = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_EmbedClashVerification_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminEmbedClashVerification = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_BphCphParameterErrorFinder_Checked(object sender, RoutedEventArgs e)
  {
    this.adminBphCphParameterErrorFinder = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_BphCphParameterErrorFinder_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminBphCphParameterErrorFinder = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_TicketTemplateCreator_Checked(object sender, RoutedEventArgs e)
  {
    this.adminTicketTemplateCreator = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_TicketTemplateCreator_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminTicketTemplateCreator = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_BulkFamilyUpdater_Checked(object sender, RoutedEventArgs e)
  {
    this.adminBulkFamilyUpdater = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_BulkFamilyUpdater_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminBulkFamilyUpdater = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_CloudSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.adminCloudSettings = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_CloudSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminCloudSettings = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_CAMSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.adminCAMsettings = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_CAMSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminCAMsettings = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_EDGEBrowserSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.adminEdgeBrowserSettings = true;
    if (this.adminTicketTemplateCreator && this.adminBphCphParameterErrorFinder && this.adminEmbedClashVerification && this.adminWarningAnalyzer && this.adminSharedParameterUpdater && this.adminBulkFamilyUpdater && this.adminCloudSettings && this.adminCAMsettings && this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(true);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void Admin_EDGEBrowserSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.adminEdgeBrowserSettings = false;
    if (!this.adminTicketTemplateCreator && !this.adminBphCphParameterErrorFinder && !this.adminEmbedClashVerification && !this.adminWarningAnalyzer && !this.adminSharedParameterUpdater && !this.adminBulkFamilyUpdater && !this.adminCloudSettings && !this.adminCAMsettings && !this.adminEdgeBrowserSettings)
      this.Admin_Panel.IsChecked = new bool?(false);
    else
      this.Admin_Panel.IsChecked = new bool?();
  }

  private void FamilyTools_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.familyToolsPanel = true;
  }

  private void FamilyTools_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.familyToolsPanel = false;
  }

  private void TicketManager_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketManagerPanel = true;
  }

  private void TicketManager_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketManagerPanel = false;
  }

  private void Visibility_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.visibilityPanel = true;
  }

  private void Visibility_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.visibilityPanel = false;
  }

  private void Annotation_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.annotationPanel = true;
    this.AnnotationPanelItem.Visibility = Visibility.Visible;
    this.Annotation_TextUtilities.IsChecked = new bool?(true);
    this.Annotation_DimensionUtilities.IsChecked = new bool?(true);
    this.Annotation_AutoDim_EDWG.IsChecked = new bool?(true);
    this.Freeze_Active_View.IsChecked = new bool?(true);
  }

  private void Annotation_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.annotationPanel = false;
    this.Annotation_TextUtilities.IsChecked = new bool?(false);
    this.Annotation_DimensionUtilities.IsChecked = new bool?(false);
    this.Annotation_AutoDim_EDWG.IsChecked = new bool?(false);
    this.Freeze_Active_View.IsChecked = new bool?(false);
    this.AnnotationPanelItem.Visibility = Visibility.Collapsed;
  }

  private void Annotation_TextUtilities_Checked(object sender, RoutedEventArgs e)
  {
    this.annotationTextUtilities = true;
    if (this.annotationTextUtilities && this.annotationDimensionUtilities && this.annotationAutoDimEDWG && this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(true);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Annotation_TextUtilities_UnChecked(object sender, RoutedEventArgs e)
  {
    this.annotationTextUtilities = false;
    if (!this.annotationTextUtilities && !this.annotationDimensionUtilities && !this.annotationAutoDimEDWG && !this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(false);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Annotation_DimensionUtilities_Checked(object sender, RoutedEventArgs e)
  {
    this.annotationDimensionUtilities = true;
    if (this.annotationTextUtilities && this.annotationDimensionUtilities && this.annotationAutoDimEDWG && this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(true);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Annotation_DimensionUtilities_UnChecked(object sender, RoutedEventArgs e)
  {
    this.annotationDimensionUtilities = false;
    if (!this.annotationTextUtilities && !this.annotationDimensionUtilities && !this.annotationAutoDimEDWG && !this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(false);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Annotation_AutoDim_EDWG_Checked(object sender, RoutedEventArgs e)
  {
    this.annotationAutoDimEDWG = true;
    if (this.annotationTextUtilities && this.annotationDimensionUtilities && this.annotationAutoDimEDWG && this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(true);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Annotation_AutoDim_EDWG_UnChecked(object sender, RoutedEventArgs e)
  {
    this.annotationAutoDimEDWG = false;
    if (!this.annotationTextUtilities && !this.annotationDimensionUtilities && !this.annotationAutoDimEDWG && !this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(false);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Freeze_Active_View_Checked(object sender, RoutedEventArgs e)
  {
    this.freezeActiveView = true;
    if (this.annotationTextUtilities && this.annotationDimensionUtilities && this.annotationAutoDimEDWG && this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(true);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Freeze_Active_View_UnChecked(object sender, RoutedEventArgs e)
  {
    this.freezeActiveView = false;
    if (!this.annotationTextUtilities && !this.annotationDimensionUtilities && !this.annotationAutoDimEDWG && !this.freezeActiveView)
      this.Annotation_Panel.IsChecked = new bool?(false);
    else
      this.Annotation_Panel.IsChecked = new bool?();
  }

  private void Geometry_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryPanel = true;
    this.GeometryPanelItem.Visibility = Visibility.Visible;
    this.Geometry_AddonHostingUpdater.IsChecked = new bool?(true);
    this.Geometry_MultiVoidTools.IsChecked = new bool?(true);
    this.Geometry_WarpedParameterCopier.IsChecked = new bool?(true);
    this.Geometry_MarkOppTools.IsChecked = new bool?(true);
    this.Geometry_MarkVerificationInitial.IsChecked = new bool?(true);
    this.Geometry_MarkVerificationExisting.IsChecked = new bool?(true);
    this.Geometry_VoidHostingUpdater.IsChecked = new bool?(true);
    this.Geometry_GetCentroid.IsChecked = new bool?(true);
    this.Geometry_NewReferencePoint.IsChecked = new bool?(true);
    this.Geometry_AutoWarping.IsChecked = new bool?(true);
  }

  private void Geometry_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryPanel = false;
    this.Geometry_AddonHostingUpdater.IsChecked = new bool?(false);
    this.Geometry_MultiVoidTools.IsChecked = new bool?(false);
    this.Geometry_WarpedParameterCopier.IsChecked = new bool?(false);
    this.Geometry_MarkOppTools.IsChecked = new bool?(false);
    this.Geometry_MarkVerificationInitial.IsChecked = new bool?(false);
    this.Geometry_MarkVerificationExisting.IsChecked = new bool?(false);
    this.Geometry_VoidHostingUpdater.IsChecked = new bool?(false);
    this.Geometry_GetCentroid.IsChecked = new bool?(false);
    this.Geometry_NewReferencePoint.IsChecked = new bool?(false);
    this.Geometry_AutoWarping.IsChecked = new bool?(false);
    this.GeometryPanelItem.Visibility = Visibility.Collapsed;
  }

  private void Geometry_AddonHostingUpdater_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryAddonHostingUpdater = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_AddonHostingUpdater_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryAddonHostingUpdater = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MultiVoidTools_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryMultiVoidTools = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MultiVoidTools_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryMultiVoidTools = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_WarpedParameterCopier_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryWarpedParameterCopier = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_WarpedParameterCopier_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryWarpedParameterCopier = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MarkOppTools_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryMarkOppTools = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MarkOppTools_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryMarkOppTools = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MarkVerificationInitial_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryMarkVerificationInitial = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MarkVerificationInitial_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryMarkVerificationInitial = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MarkVerificationExisting_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryMarkVerificationExisting = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_MarkVerificationExisting_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryMarkVerificationExisting = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_VoidHostingUpdater_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryVoidHostingUpdater = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_VoidHostingUpdater_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryVoidHostingUpdater = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_GetCentroid_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryGetCentroid = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_GetCentroid_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryGetCentroid = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_NewReferencePoint_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryNewReferencePoint = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_NewReferencePoint_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryNewReferencePoint = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_AutoWarping_Checked(object sender, RoutedEventArgs e)
  {
    this.geometryAutoWarping = true;
    if (this.geometryAddonHostingUpdater && this.geometryMultiVoidTools && this.geometryWarpedParameterCopier && this.geometryMarkOppTools && this.geometryMarkVerificationInitial && this.geometryMarkVerificationExisting && this.geometryVoidHostingUpdater && this.geometryGetCentroid && this.geometryNewReferencePoint && this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(true);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Geometry_AutoWarping_UnChecked(object sender, RoutedEventArgs e)
  {
    this.geometryAutoWarping = false;
    if (!this.geometryAddonHostingUpdater && !this.geometryMultiVoidTools && !this.geometryWarpedParameterCopier && !this.geometryMarkOppTools && !this.geometryMarkVerificationInitial && !this.geometryMarkVerificationExisting && !this.geometryVoidHostingUpdater && !this.geometryGetCentroid && !this.geometryNewReferencePoint && !this.geometryAutoWarping)
      this.Geometry_Panel.IsChecked = new bool?(false);
    else
      this.Geometry_Panel.IsChecked = new bool?();
  }

  private void Insulation_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationPanel = true;
    this.InsulationPanelItem.Visibility = Visibility.Visible;
    this.Insulation_InsulationRemoval.IsChecked = new bool?(true);
    this.Insulation_AutoInsulationPlacement.IsChecked = new bool?(true);
    this.Insulation_ManualInsulationPlacement.IsChecked = new bool?(true);
    this.Insulation_AutoPinPlacement.IsChecked = new bool?(true);
    this.Insulation_InsulationMarking.IsChecked = new bool?(true);
    this.Insulation_InsulationExport.IsChecked = new bool?(true);
    this.Insulation_InsulationDrawingMaster.IsChecked = new bool?(true);
    this.Insulation_InsulationDrawingPerWall.IsChecked = new bool?(true);
    this.Insulation_InsulationDrawing.IsChecked = new bool?(true);
  }

  private void Insulation_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.insulationPanel = false;
    this.Insulation_InsulationRemoval.IsChecked = new bool?(false);
    this.Insulation_AutoInsulationPlacement.IsChecked = new bool?(false);
    this.Insulation_ManualInsulationPlacement.IsChecked = new bool?(false);
    this.Insulation_AutoPinPlacement.IsChecked = new bool?(false);
    this.Insulation_InsulationMarking.IsChecked = new bool?(false);
    this.Insulation_InsulationExport.IsChecked = new bool?(false);
    this.Insulation_InsulationDrawingMaster.IsChecked = new bool?(false);
    this.Insulation_InsulationDrawingPerWall.IsChecked = new bool?(false);
    this.Insulation_InsulationDrawing.IsChecked = new bool?(false);
    this.InsulationPanelItem.Visibility = Visibility.Collapsed;
  }

  private void Insulation_InsulationRemoval_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationRemoval = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationRemoval_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationRemoval = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_AutoInsulationPlacement_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationAutoPlacement = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_AutoInsulationPlacement_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationAutoPlacement = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_ManualInsulationPlacement_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationManualPlacement = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_ManualInsulationPlacement_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationManualPlacement = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_AutoPinPlacement_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationAutoPinPlacement = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_AutoPinPlacement_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationAutoPinPlacement = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationMarking_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationMarking = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationMarking_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationMarking = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationExport_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationExport = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationExport_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationExport = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationDrawingMaster_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationDrawingMaster = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationDrawingMaster_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationDrawingMaster = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationDrawingPerWall_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationDrawingPerWall = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationDrawingPerWall_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationDrawingPerWall = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationDrawing_Checked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationDrawing = true;
    if (this.insulationInsulationRemoval && this.insulationAutoPlacement && this.insulationManualPlacement && this.insulationAutoPinPlacement && this.insulationInsulationMarking && this.insulationInsulationExport && this.insulationInsulationDrawingMaster && this.insulationInsulationDrawingPerWall && this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(true);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Insulation_InsulationDrawing_Unchecked(object sender, RoutedEventArgs e)
  {
    this.insulationInsulationDrawing = false;
    if (!this.insulationInsulationRemoval && !this.insulationAutoPlacement && !this.insulationManualPlacement && !this.insulationAutoPinPlacement && !this.insulationInsulationMarking && !this.insulationInsulationExport && !this.insulationInsulationDrawingMaster && !this.insulationInsulationDrawingPerWall && !this.insulationInsulationDrawing)
      this.Insulation_Panel.IsChecked = new bool?(false);
    else
      this.Insulation_Panel.IsChecked = new bool?();
  }

  private void Assembly_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyPanel = true;
    this.AssemblyPanelItem.Visibility = Visibility.Visible;
    this.Assembly_CreationUpdatingRenaming.IsChecked = new bool?(true);
    this.Assembly_CreationUpdatingRevit.IsChecked = new bool?(true);
    this.Assembly_CopyReinforcing.IsChecked = new bool?(true);
    this.Assembly_MarkAsReinforced.IsChecked = new bool?(true);
    this.Assembly_TopAsCast.IsChecked = new bool?(true);
    this.Assembly_MatchTopAsCast.IsChecked = new bool?(true);
  }

  private void Assembly_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyPanel = false;
    this.Assembly_CreationUpdatingRenaming.IsChecked = new bool?(false);
    this.Assembly_CreationUpdatingRevit.IsChecked = new bool?(false);
    this.Assembly_CopyReinforcing.IsChecked = new bool?(false);
    this.Assembly_MarkAsReinforced.IsChecked = new bool?(false);
    this.Assembly_TopAsCast.IsChecked = new bool?(false);
    this.Assembly_MatchTopAsCast.IsChecked = new bool?(false);
    this.AssemblyPanelItem.Visibility = Visibility.Collapsed;
  }

  private void Assembly_CreationUpdatingRenaming_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyCreationUpdatingRenaming = true;
    if (this.assemblyCreationUpdatingRenaming && this.assemblyCreationUpdatingRevit && this.assemblyMarkAsReinforced && this.assemblyTopAsCast && this.assemblyMatchTopAsCast && this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(true);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_CreationUpdatingRenaming_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyCreationUpdatingRenaming = false;
    if (!this.assemblyCreationUpdatingRenaming && !this.assemblyCreationUpdatingRevit && !this.assemblyMarkAsReinforced && !this.assemblyTopAsCast && !this.assemblyMatchTopAsCast && !this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(false);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_CreationUpdatingRevit_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyCreationUpdatingRevit = true;
    if (this.assemblyCreationUpdatingRenaming && this.assemblyCreationUpdatingRevit && this.assemblyMarkAsReinforced && this.assemblyTopAsCast && this.assemblyMatchTopAsCast && this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(true);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_CreationUpdatingRevit_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyCreationUpdatingRevit = false;
    if (!this.assemblyCreationUpdatingRenaming && !this.assemblyCreationUpdatingRevit && !this.assemblyMarkAsReinforced && !this.assemblyTopAsCast && !this.assemblyMatchTopAsCast && !this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(false);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_CopyReinforcing_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyCopyReinforcing = true;
    if (this.assemblyCreationUpdatingRenaming && this.assemblyCreationUpdatingRevit && this.assemblyMarkAsReinforced && this.assemblyTopAsCast && this.assemblyMatchTopAsCast && this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(true);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_CopyReinforcing_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyCopyReinforcing = false;
    if (!this.assemblyCreationUpdatingRenaming && !this.assemblyCreationUpdatingRevit && !this.assemblyMarkAsReinforced && !this.assemblyTopAsCast && !this.assemblyMatchTopAsCast && !this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(false);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_MarkAsReinforced_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyMarkAsReinforced = true;
    if (this.assemblyCreationUpdatingRenaming && this.assemblyCreationUpdatingRevit && this.assemblyMarkAsReinforced && this.assemblyTopAsCast && this.assemblyMatchTopAsCast && this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(true);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_MarkAsReinforced_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyMarkAsReinforced = false;
    if (!this.assemblyCreationUpdatingRenaming && !this.assemblyCreationUpdatingRevit && !this.assemblyMarkAsReinforced && !this.assemblyTopAsCast && !this.assemblyMatchTopAsCast && !this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(false);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_TopAsCast_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyTopAsCast = true;
    if (this.assemblyCreationUpdatingRenaming && this.assemblyCreationUpdatingRevit && this.assemblyMarkAsReinforced && this.assemblyTopAsCast && this.assemblyMatchTopAsCast && this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(true);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_TopAsCast_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyTopAsCast = false;
    if (!this.assemblyCreationUpdatingRenaming && !this.assemblyCreationUpdatingRevit && !this.assemblyMarkAsReinforced && !this.assemblyTopAsCast && !this.assemblyMatchTopAsCast && !this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(false);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_MatchTopAsCast_Checked(object sender, RoutedEventArgs e)
  {
    this.assemblyMatchTopAsCast = true;
    if (this.assemblyCreationUpdatingRenaming && this.assemblyCreationUpdatingRevit && this.assemblyMarkAsReinforced && this.assemblyTopAsCast && this.assemblyMatchTopAsCast && this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(true);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void Assembly_MatchTopAsCast_UnChecked(object sender, RoutedEventArgs e)
  {
    this.assemblyMatchTopAsCast = false;
    if (!this.assemblyCreationUpdatingRenaming && !this.assemblyCreationUpdatingRevit && !this.assemblyMarkAsReinforced && !this.assemblyTopAsCast && !this.assemblyMatchTopAsCast && !this.assemblyCopyReinforcing)
      this.Assembly_Panel.IsChecked = new bool?(false);
    else
      this.Assembly_Panel.IsChecked = new bool?();
  }

  private void setTicketToolPanelCheck()
  {
    if (this.ticketToolsAnnotateEmbedLong && this.ticketToolsAnnotateEmbedShort && this.ticketToolsControlNumberPopulator && this.ticketToolsTicketBomDuplicatesSchedule && this.ticketToolsTicketTitleBlockPopulator && this.ticketToolsBatchTicketTitleBlockPopulator && this.ticketToolsCopyTicketViews && this.ticketToolsFindReferringViews && this.ticketToolsTicketPopulator && this.ticketToolsBatchTicketPopulator && this.ticketToolsCopyTicketAnnotations && this.ticketToolsAutoDim && this.ticketToolsAutoTicket && this.ticketToolsCloneTicket && this.ticketToolsLaserExport)
      this.TicketTools_Panel.IsChecked = new bool?(true);
    else
      this.TicketTools_Panel.IsChecked = new bool?();
  }

  private void setTicketToolPanelUnCheck()
  {
    if (!this.ticketToolsAnnotateEmbedLong && !this.ticketToolsAnnotateEmbedShort && !this.ticketToolsControlNumberPopulator && !this.ticketToolsTicketBomDuplicatesSchedule && !this.ticketToolsTicketTitleBlockPopulator && !this.ticketToolsBatchTicketTitleBlockPopulator && !this.ticketToolsCopyTicketViews && !this.ticketToolsFindReferringViews && !this.ticketToolsTicketPopulator && !this.ticketToolsBatchTicketPopulator && !this.ticketToolsCopyTicketAnnotations && !this.ticketToolsAutoDim && !this.ticketToolsAutoTicket && !this.ticketToolsCloneTicket && !this.ticketToolsLaserExport)
      this.TicketTools_Panel.IsChecked = new bool?(false);
    else
      this.TicketTools_Panel.IsChecked = new bool?();
  }

  private void TicketTools_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsPanel = true;
    this.TicketToolsPanelItem.Visibility = Visibility.Visible;
    this.TicketTools_AnnotateEmbedLong.IsChecked = new bool?(true);
    this.TicketTools_AnnotateEmbedShort.IsChecked = new bool?(true);
    this.TicketTools_ControlNumberPopulator.IsChecked = new bool?(true);
    this.TicketTools_TicketBomDuplicatesSchedule.IsChecked = new bool?(true);
    this.TicketTools_TicketTitleBlockPopulator.IsChecked = new bool?(true);
    this.TicketTools_BatchTicketTitleBlockPopulator.IsChecked = new bool?(true);
    this.TicketTools_CopyTicketViews.IsChecked = new bool?(true);
    this.TicketTools_CopyTicketAnnotations.IsChecked = new bool?(true);
    this.TicketTools_FindReferringViews.IsChecked = new bool?(true);
    this.TicketTools_TicketPopulator.IsChecked = new bool?(true);
    this.TicketTools_BatchTicketPopulator.IsChecked = new bool?(true);
    this.TicketTools_AutoDimension.IsChecked = new bool?(true);
    this.TicketTools_AutoTicket.IsChecked = new bool?(true);
    this.TicketTools_CloneTicket.IsChecked = new bool?(true);
    this.TicketTools_LaserExport.IsChecked = new bool?(true);
  }

  private void TicketTools_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsPanel = false;
    this.TicketTools_AnnotateEmbedLong.IsChecked = new bool?(false);
    this.TicketTools_AnnotateEmbedShort.IsChecked = new bool?(false);
    this.TicketTools_ControlNumberPopulator.IsChecked = new bool?(false);
    this.TicketTools_TicketBomDuplicatesSchedule.IsChecked = new bool?(false);
    this.TicketTools_TicketTitleBlockPopulator.IsChecked = new bool?(false);
    this.TicketTools_BatchTicketTitleBlockPopulator.IsChecked = new bool?(false);
    this.TicketTools_CopyTicketViews.IsChecked = new bool?(false);
    this.TicketTools_CopyTicketAnnotations.IsChecked = new bool?(false);
    this.TicketTools_FindReferringViews.IsChecked = new bool?(false);
    this.TicketTools_TicketPopulator.IsChecked = new bool?(false);
    this.TicketTools_BatchTicketPopulator.IsChecked = new bool?(false);
    this.TicketTools_AutoDimension.IsChecked = new bool?(false);
    this.TicketTools_AutoTicket.IsChecked = new bool?(false);
    this.TicketTools_CloneTicket.IsChecked = new bool?(false);
    this.TicketTools_LaserExport.IsChecked = new bool?(false);
    this.TicketToolsPanelItem.Visibility = Visibility.Collapsed;
  }

  private void TicketTools_AnnotateEmbedLong_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAnnotateEmbedLong = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_AnnotateEmbedLong_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAnnotateEmbedLong = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_AnnotateEmbedShort_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAnnotateEmbedShort = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_AnnotateEmbedShort_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAnnotateEmbedShort = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_ControlNumberPopulator_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsControlNumberPopulator = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_ControlNumberPopulator_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsControlNumberPopulator = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_TicketBomDuplicatesSchedule_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsTicketBomDuplicatesSchedule = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_TicketBomDuplicatesSchedule_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsTicketBomDuplicatesSchedule = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_TicketTitleBlockPopulator_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsTicketTitleBlockPopulator = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_TicketTitleBlockPopulator_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsTicketTitleBlockPopulator = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_BatchTicketTitleBlockPopulator_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsBatchTicketTitleBlockPopulator = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_BatchTicketTitleBlockPopulator_Unchecked(
    object sender,
    RoutedEventArgs e)
  {
    this.ticketToolsBatchTicketTitleBlockPopulator = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_CopyTicketViews_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsCopyTicketViews = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_CopyTicketViews_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsCopyTicketViews = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_CopyTicketAnnotations_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsCopyTicketAnnotations = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_CopyTicketAnnotations_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsCopyTicketAnnotations = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_FindReferringViews_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsFindReferringViews = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_FindReferringViews_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsFindReferringViews = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_TicketPopulator_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsTicketPopulator = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_TicketPopulator_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsTicketPopulator = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_BatchTicketPopulator_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsBatchTicketPopulator = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_BatchTicketPopulator_Unchecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsBatchTicketPopulator = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_AutoDimension_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAutoDim = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_AutoDimension_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAutoDim = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_AutoTicket_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAutoTicket = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_AutoTicket_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsAutoTicket = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_CloneTicket_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsCloneTicket = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_CloneTicket_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsCloneTicket = false;
    this.setTicketToolPanelUnCheck();
  }

  private void TicketTools_LaserExport_Checked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsLaserExport = true;
    this.setTicketToolPanelCheck();
  }

  private void TicketTools_LaserExport_UnChecked(object sender, RoutedEventArgs e)
  {
    this.ticketToolsLaserExport = false;
    this.setTicketToolPanelUnCheck();
  }

  private void setSelectionAndPinningPanelCheck()
  {
    if (this.selectionAndPinningGrids && this.selectionAndPinningLevels && this.selectionAndPinningSelectSFElements)
      this.SelectionAndPinning_Panel.IsChecked = new bool?(true);
    else
      this.SelectionAndPinning_Panel.IsChecked = new bool?();
  }

  private void setSelectionAndPinningPanelUnCheck()
  {
    if (!this.selectionAndPinningGrids && !this.selectionAndPinningLevels && !this.selectionAndPinningSelectSFElements)
      this.SelectionAndPinning_Panel.IsChecked = new bool?(false);
    else
      this.SelectionAndPinning_Panel.IsChecked = new bool?();
  }

  private void SelectionAndPinning_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningPanel = true;
    this.SelectionAndPinningPanelItem.Visibility = Visibility.Visible;
    this.SelectionAndPinning_Grids.IsChecked = new bool?(true);
    this.SelectionAndPinning_Levels.IsChecked = new bool?(true);
    this.SelectionAndPinning_SelectSFElements.IsChecked = new bool?(true);
  }

  private void SelectionAndPinning_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningPanel = false;
    this.SelectionAndPinning_Grids.IsChecked = new bool?(false);
    this.SelectionAndPinning_Levels.IsChecked = new bool?(false);
    this.SelectionAndPinning_SelectSFElements.IsChecked = new bool?(false);
    this.SelectionAndPinningPanelItem.Visibility = Visibility.Collapsed;
  }

  private void SelectionAndPinning_Grids_Checked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningGrids = true;
    this.setSelectionAndPinningPanelCheck();
  }

  private void SelectionAndPinning_Grids_UnChecked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningGrids = false;
    this.setSelectionAndPinningPanelUnCheck();
  }

  private void SelectionAndPinning_Levels_Checked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningLevels = true;
    this.setSelectionAndPinningPanelCheck();
  }

  private void SelectionAndPinning_Levels_UnChecked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningLevels = false;
    this.setSelectionAndPinningPanelUnCheck();
  }

  private void SelectionAndPinning_SelectSFElements_Checked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningSelectSFElements = true;
    this.setSelectionAndPinningPanelCheck();
  }

  private void SelectionAndPinning_SelectSFElements_UnChecked(object sender, RoutedEventArgs e)
  {
    this.selectionAndPinningSelectSFElements = false;
    this.setSelectionAndPinningPanelUnCheck();
  }

  private void setSchedulingPanelCheck()
  {
    if (this.schedulingControlNumberIncrementor && this.schedulingBomProductHosting && this.schedulingConstructionProductHosting && this.schedulingScheduleSequenceByView && this.schedulingErectionSequence)
      this.Scheduling_Panel.IsChecked = new bool?(true);
    else
      this.Scheduling_Panel.IsChecked = new bool?();
  }

  private void setSchedulingPanelUnCheck()
  {
    if (!this.schedulingControlNumberIncrementor && !this.schedulingBomProductHosting && !this.schedulingConstructionProductHosting && !this.schedulingScheduleSequenceByView && !this.schedulingErectionSequence)
      this.Scheduling_Panel.IsChecked = new bool?(false);
    else
      this.Scheduling_Panel.IsChecked = new bool?();
  }

  private void Scheduling_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.schedulingPanel = true;
    this.SchedulingPanelItem.Visibility = Visibility.Visible;
    this.Scheduling_ControlNumberIncrementor.IsChecked = new bool?(true);
    this.Scheduling_BomProductHosting.IsChecked = new bool?(true);
    this.Scheduling_ConstructionProductHosting.IsChecked = new bool?(true);
    this.Scheduling_ScheduleSequenceByView.IsChecked = new bool?(true);
    this.Scheduling_Erection_Sequence.IsChecked = new bool?(true);
  }

  private void Scheduling_Panel_UnChecked(object sender, RoutedEventArgs e)
  {
    this.schedulingPanel = false;
    this.Scheduling_ControlNumberIncrementor.IsChecked = new bool?(false);
    this.Scheduling_BomProductHosting.IsChecked = new bool?(false);
    this.Scheduling_ConstructionProductHosting.IsChecked = new bool?(false);
    this.Scheduling_ScheduleSequenceByView.IsChecked = new bool?(false);
    this.Scheduling_Erection_Sequence.IsChecked = new bool?(false);
    this.SchedulingPanelItem.Visibility = Visibility.Collapsed;
  }

  private void Scheduling_ControlNumberIncrementor_Checked(object sender, RoutedEventArgs e)
  {
    this.schedulingControlNumberIncrementor = true;
    this.setSchedulingPanelCheck();
  }

  private void Scheduling_ControlNumberIncrementor_UnChecked(object sender, RoutedEventArgs e)
  {
    this.schedulingControlNumberIncrementor = false;
    this.setSchedulingPanelUnCheck();
  }

  private void Scheduling_BomProductHosting_Checked(object sender, RoutedEventArgs e)
  {
    this.schedulingBomProductHosting = true;
    this.setSchedulingPanelCheck();
  }

  private void Scheduling_BomProductHosting_UnChecked(object sender, RoutedEventArgs e)
  {
    this.schedulingBomProductHosting = false;
    this.setSchedulingPanelUnCheck();
  }

  private void Scheduling_ConstructionProductHosting_Checked(object sender, RoutedEventArgs e)
  {
    this.schedulingConstructionProductHosting = true;
    this.setSchedulingPanelCheck();
  }

  private void Scheduling_ConstructionProductHosting_UnChecked(object sender, RoutedEventArgs e)
  {
    this.schedulingConstructionProductHosting = false;
    this.setSchedulingPanelUnCheck();
  }

  private void Scheduling_ScheduleSequenceByView_Checked(object sender, RoutedEventArgs e)
  {
    this.schedulingScheduleSequenceByView = true;
    this.setSchedulingPanelCheck();
  }

  private void Scheduling_ScheduleSequenceByView_UnChecked(object sender, RoutedEventArgs e)
  {
    this.schedulingScheduleSequenceByView = false;
    this.setSchedulingPanelUnCheck();
  }

  private void Scheduling_Erection_Sequence_Checked(object sender, RoutedEventArgs e)
  {
    this.schedulingErectionSequence = true;
    this.setSchedulingPanelCheck();
  }

  private void Scheduling_Erection_Sequence_UnChecked(object sender, RoutedEventArgs e)
  {
    this.schedulingErectionSequence = false;
    this.setSchedulingPanelUnCheck();
  }

  private void UserSettings_TicketAndDimensionSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettingsTicketAndDimenaionSettings = true;
  }

  private void UserSettings_TicketAndDimensionSettings_Unchecked(object sender, RoutedEventArgs e)
  {
    this.userSettingsTicketAndDimenaionSettings = false;
  }

  private void UserSettings_RebarSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettingsRebarSettings = true;
  }

  private void UserSettings_RebarSettings_Unchecked(object sender, RoutedEventArgs e)
  {
    this.userSettingsRebarSettings = false;
  }

  private void UserSettings_MarkVerificationSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettingsMarkVerificationSettings = true;
  }

  private void UserSettings_MarkVerificationSettings_Unchecked(object sender, RoutedEventArgs e)
  {
    this.userSettingsMarkVerificationSettings = false;
  }

  private void UserSettings_TicketManagerCustomizationSettings_Checked(
    object sender,
    RoutedEventArgs e)
  {
    this.userSettingsTicketManagerCustominazationSettings = true;
  }

  private void UserSettings_TicketManagerCustomizationSettings_Unchecked(
    object sender,
    RoutedEventArgs e)
  {
    this.userSettingsTicketManagerCustominazationSettings = false;
  }

  private void UserSettings_TBPSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettings_TBPSettings = true;
  }

  private void UserSettings_TBPSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.userSettings_TBPSettings = false;
  }

  private void UserSettings_TicketBOMSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettingsTicketBOMSettings = true;
  }

  private void UserSettings_TicketBOMSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.userSettingsTicketBOMSettings = false;
  }

  private void UserSettings_CADExportSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettingsCadExportSettings = true;
  }

  private void UserSettings_CADExportSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.userSettingsCadExportSettings = false;
  }

  private void UserSettings_InsulationDrawingSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.userSettingsInsulationDrawingSettings = true;
  }

  private void UserSettings_InsulationDrawingSettings_UnChecked(object sender, RoutedEventArgs e)
  {
    this.userSettingsInsulationDrawingSettings = false;
  }

  private void Window_Loaded(object sender, RoutedEventArgs e)
  {
    this.Admin_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonAdminPanel);
    this.Admin_SharedParameterUpdater.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowSharedParams);
    this.Admin_WarningAnalyzer.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowWarningAnalyzer);
    this.Admin_EmbedClashVerification.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowEmbedClashVerification);
    this.Admin_BphCphParameterErrorFinder.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowBPHCPHErrorFinder);
    this.Admin_TicketTemplateCreator.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowTicketTemplateCreator);
    this.Admin_BulkFamilyUpdater.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowBulkFamilyUpdater);
    this.Admin_CloudSettings.IsChecked = new bool?(App.Edge_Cloud_Id != -1);
    this.Admin_CAMSettings.IsChecked = new bool?(AppRibbonSetup.RibbonAdminShowCamExport);
    this.Admin_EDGEBrowserSettings.IsChecked = new bool?(AppRibbonSetup.RibbonAdminEdgeBrowser);
    this.FamilyTools_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonFamilyToolsPanel);
    this.TicketManager_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonTicketManagerPanel);
    this.Visibility_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonVisibilityPanel);
    this.Annotation_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonAnnotationPanel);
    this.Annotation_TextUtilities.IsChecked = new bool?(AppRibbonSetup.RibbonAnnotationTextUtilities);
    this.Annotation_DimensionUtilities.IsChecked = new bool?(AppRibbonSetup.RibbonAnnotationDimensionUtilities);
    this.Annotation_AutoDim_EDWG.IsChecked = new bool?(AppRibbonSetup.RibbonAnnotationToolsEDrawingAutoDim);
    this.Freeze_Active_View.IsChecked = new bool?(AppRibbonSetup.RibbonAnnotationFreezeActiveView);
    this.Geometry_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryPanel);
    this.Geometry_AddonHostingUpdater.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryAddonHostingUpdater);
    this.Geometry_MultiVoidTools.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryMultiVoidTools);
    this.Geometry_WarpedParameterCopier.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryWarpedParameterCopier);
    this.Geometry_MarkOppTools.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryMarkOppTools);
    this.Geometry_MarkVerificationInitial.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryMarkVerificationInitial);
    this.Geometry_MarkVerificationExisting.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryMarkVerificationExisting);
    this.Geometry_VoidHostingUpdater.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryVoidHostingUpdater);
    this.Geometry_GetCentroid.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryGetCentroid);
    this.Geometry_NewReferencePoint.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryNewReferencePoint);
    this.Geometry_AutoWarping.IsChecked = new bool?(AppRibbonSetup.RibbonGeometryAutoWarping);
    this.Insulation_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationPanel);
    this.Insulation_InsulationRemoval.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationRemoval);
    this.Insulation_AutoInsulationPlacement.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationAutoPlacement);
    this.Insulation_ManualInsulationPlacement.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationManualPlacement);
    this.Insulation_AutoPinPlacement.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationPinPlacement);
    this.Insulation_InsulationMarking.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationMarking);
    this.Insulation_InsulationExport.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationExport);
    this.Insulation_InsulationDrawingMaster.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationDrawingMaster);
    this.Insulation_InsulationDrawingPerWall.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationDrawingPerPiece);
    this.Insulation_InsulationDrawing.IsChecked = new bool?(AppRibbonSetup.RibbonInsulationDrawing);
    this.Assembly_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyPanel);
    this.Assembly_CreationUpdatingRenaming.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyCreationUpdatingRenaming);
    this.Assembly_CreationUpdatingRevit.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyCreationUpdatingRevit);
    this.Assembly_CopyReinforcing.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyCopyReinforcing);
    this.Assembly_MarkAsReinforced.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyMarkAsReinforced);
    this.Assembly_TopAsCast.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyTopAsCast);
    this.Assembly_MatchTopAsCast.IsChecked = new bool?(AppRibbonSetup.RibbonAssemblyMatchTopAsCast);
    this.TicketTools_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsPanel);
    this.TicketTools_AnnotateEmbedLong.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsAnnotateEmbedLong);
    this.TicketTools_AnnotateEmbedShort.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsAnnotateEmbedShort);
    this.TicketTools_ControlNumberPopulator.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsControlNumberPopulator);
    this.TicketTools_TicketBomDuplicatesSchedule.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsTicketBomDuplicatesSchedule);
    this.TicketTools_TicketTitleBlockPopulator.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsTicketTitleBlockPopulator);
    this.TicketTools_BatchTicketTitleBlockPopulator.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsBatchTicketTitleBlockPopulator);
    this.TicketTools_CopyTicketViews.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsCopyTicketViews);
    this.TicketTools_CopyTicketAnnotations.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsCopyTicketAnnotations);
    this.TicketTools_FindReferringViews.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsFindReferringViews);
    this.TicketTools_TicketPopulator.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsTicketPopulator);
    this.TicketTools_BatchTicketPopulator.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsBatchTicketPopulator);
    this.TicketTools_AutoDimension.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsAutoDim);
    this.TicketTools_AutoTicket.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsAutoTicketGeneration);
    this.TicketTools_CloneTicket.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsCloneTicket);
    this.TicketTools_LaserExport.IsChecked = new bool?(AppRibbonSetup.RibbonTicketToolsLaserExport);
    this.HardwareTools_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsPanel);
    this.HardwareTools_HardwareDetail.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsHardwareDetail);
    this.HardwareTools_HardwareDetailTemplateManager.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsShowHWDetailTemplateCreator);
    this.HardwareTools_HWTitleBlockPopulatorSettings.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsShowHWDTBPSettings);
    this.HardwareTools_HardwareDetailBOMSettings.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsShowHWDTicketBOMSettings);
    this.SelectionAndPinning_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonSelectionAndPinningPanel);
    this.SelectionAndPinning_Grids.IsChecked = new bool?(AppRibbonSetup.RibbonSelectionAndPinningGrids);
    this.SelectionAndPinning_Levels.IsChecked = new bool?(AppRibbonSetup.RibbonSelectionAndPinningLevels);
    this.SelectionAndPinning_SelectSFElements.IsChecked = new bool?(AppRibbonSetup.RibbonSelectionAndPinningSelectSfElements);
    this.Scheduling_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonSchedulingPanel);
    this.Scheduling_ControlNumberIncrementor.IsChecked = new bool?(AppRibbonSetup.RibbonSchedulingControlNumberIncrementor);
    this.Scheduling_BomProductHosting.IsChecked = new bool?(AppRibbonSetup.RibbonSchedulingBomProductHosting);
    this.Scheduling_ConstructionProductHosting.IsChecked = new bool?(AppRibbonSetup.RibbonSchedulingConstructionProductHosting);
    this.Scheduling_ScheduleSequenceByView.IsChecked = new bool?(AppRibbonSetup.RibbonSchedulingScheduleSequenceByView);
    this.Scheduling_Erection_Sequence.IsChecked = new bool?(AppRibbonSetup.RibbonSchedulingErectionSequence);
    this.UserSettings_TicketAndDimensionSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsTicketAndDimensionSettings);
    this.UserSettings_RebarSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsRebarSettings);
    this.UserSettings_MarkVerificationSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsMarkVerificationSettings);
    this.UserSettings_TicketManagerCustomizationSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsTicketManagerCustomizationSettings);
    this.UserSettings_TicketBOMSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsShowTicketBOMSettings);
    this.UserSettings_TBPSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsTBPSettings);
    this.UserSettings_CADExportSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsShowCADExportSettings);
    this.UserSettings_InsulationDrawingSettings.IsChecked = new bool?(AppRibbonSetup.RibbonUserSettingsShowInsulationDrawingSettings);
    this.HardwareTools_Panel.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsPanel);
    this.HardwareTools_HardwareDetail.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsHardwareDetail);
    this.HardwareTools_HardwareDetailTemplateManager.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsShowHWDetailTemplateCreator);
    this.HardwareTools_HWTitleBlockPopulatorSettings.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsShowHWDTBPSettings);
    this.HardwareTools_HardwareDetailBOMSettings.IsChecked = new bool?(AppRibbonSetup.RibbonHardwareToolsShowHWDTicketBOMSettings);
    this.FixPanels(true);
    this.FolderExpander.IsExpanded = false;
    this.RebarExpander.IsExpanded = false;
    this.RibbonExpander.IsExpanded = false;
  }

  private void HardwareTools_Panel_Checked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsPanel = true;
    this.HardwareToolsPanelItem.Visibility = Visibility.Visible;
    this.HardwareTools_HardwareDetail.IsChecked = new bool?(true);
    this.HardwareTools_HardwareDetailTemplateManager.IsChecked = new bool?(true);
    this.HardwareTools_HWTitleBlockPopulatorSettings.IsChecked = new bool?(true);
    this.HardwareTools_HardwareDetailBOMSettings.IsChecked = new bool?(true);
  }

  private void HardwareTools_Panel_Unchecked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsPanel = false;
    this.HardwareToolsPanelItem.Visibility = Visibility.Collapsed;
    this.HardwareTools_HardwareDetail.IsChecked = new bool?(false);
    this.HardwareTools_HardwareDetailTemplateManager.IsChecked = new bool?(false);
    this.HardwareTools_HWTitleBlockPopulatorSettings.IsChecked = new bool?(false);
    this.HardwareTools_HardwareDetailBOMSettings.IsChecked = new bool?(false);
  }

  private void HardwareTools_HardwareDetail_Checked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsHardwareDetail = true;
    if (this.hardwareToolsHardwareDetail && this.hardwareToolsHardwareDetailTemplateManager && this.hardwareToolsHWTitleBlockPopulatorSettings && this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(true);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HardwareDetail_Unchecked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsHardwareDetail = false;
    if (!this.hardwareToolsHardwareDetail && !this.hardwareToolsHardwareDetailTemplateManager && !this.hardwareToolsHWTitleBlockPopulatorSettings && !this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(false);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HardwareDetailTemplateManager_Checked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsHardwareDetailTemplateManager = true;
    if (this.hardwareToolsHardwareDetail && this.hardwareToolsHardwareDetailTemplateManager && this.hardwareToolsHWTitleBlockPopulatorSettings && this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(true);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HardwareDetailTemplateManager_Unchecked(
    object sender,
    RoutedEventArgs e)
  {
    this.hardwareToolsHardwareDetailTemplateManager = false;
    if (!this.hardwareToolsHardwareDetail && !this.hardwareToolsHardwareDetailTemplateManager && !this.hardwareToolsHWTitleBlockPopulatorSettings && !this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(false);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HWTitleBlockPopulatorSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsHWTitleBlockPopulatorSettings = true;
    if (this.hardwareToolsHardwareDetail && this.hardwareToolsHardwareDetailTemplateManager && this.hardwareToolsHWTitleBlockPopulatorSettings && this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(true);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HWTitleBlockPopulatorSettings_Unchecked(
    object sender,
    RoutedEventArgs e)
  {
    this.hardwareToolsHWTitleBlockPopulatorSettings = false;
    if (!this.hardwareToolsHardwareDetail && !this.hardwareToolsHardwareDetailTemplateManager && !this.hardwareToolsHWTitleBlockPopulatorSettings && !this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(false);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HardwareDetailBOMSettings_Checked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsHardwareDetialBomSettings = true;
    if (this.hardwareToolsHardwareDetail && this.hardwareToolsHardwareDetailTemplateManager && this.hardwareToolsHWTitleBlockPopulatorSettings && this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(true);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  private void HardwareTools_HardwareDetailBOMSettings_Unchecked(object sender, RoutedEventArgs e)
  {
    this.hardwareToolsHardwareDetialBomSettings = false;
    if (!this.hardwareToolsHardwareDetail && !this.hardwareToolsHardwareDetailTemplateManager && !this.hardwareToolsHWTitleBlockPopulatorSettings && !this.hardwareToolsHardwareDetialBomSettings)
      this.HardwareTools_Panel.IsChecked = new bool?(false);
    else
      this.HardwareTools_Panel.IsChecked = new bool?();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/edgepreferencesusersettingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.Window_Loaded);
        break;
      case 2:
        this.FolderExpander = (Expander) target;
        break;
      case 3:
        this.TicketTemplateSettingsFile = (System.Windows.Controls.TextBox) target;
        break;
      case 4:
        this.TicketTemplateBrowse = (System.Windows.Controls.Button) target;
        this.TicketTemplateBrowse.Click += new RoutedEventHandler(this.TicketTemplateBrowse_Click);
        break;
      case 5:
        this.HardwareTemplateSettingsFile = (System.Windows.Controls.TextBox) target;
        break;
      case 6:
        this.HardwareTemplateBrowse = (System.Windows.Controls.Button) target;
        this.HardwareTemplateBrowse.Click += new RoutedEventHandler(this.HardwareTemplateBrowse_Click);
        break;
      case 7:
        this.RebarSettingsFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 8:
        this.RebarSettingFolderBrowse = (System.Windows.Controls.Button) target;
        this.RebarSettingFolderBrowse.Click += new RoutedEventHandler(this.RebarSettingFolderBrowse_Click);
        break;
      case 9:
        this.MarkPrefixFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 10:
        this.MarkPrefixFolderBrowse = (System.Windows.Controls.Button) target;
        this.MarkPrefixFolderBrowse.Click += new RoutedEventHandler(this.MarkPrefixFolderBrowse_Click);
        break;
      case 11:
        this.TicketSettingsFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 12:
        this.AutoTicketSettingsBrowse = (System.Windows.Controls.Button) target;
        this.AutoTicketSettingsBrowse.Click += new RoutedEventHandler(this.AutoTicketSettingsBrowse_Click);
        break;
      case 13:
        this.TKTBOMFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 14:
        this.TKTBOMFolderBrowse = (System.Windows.Controls.Button) target;
        this.TKTBOMFolderBrowse.Click += new RoutedEventHandler(this.TKTBOMFolderBrowse_Click);
        break;
      case 15:
        this.TBPopFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 16 /*0x10*/:
        this.TBPopFolderBrowse = (System.Windows.Controls.Button) target;
        this.TBPopFolderBrowse.Click += new RoutedEventHandler(this.TBPopFolderBrowse_Click);
        break;
      case 17:
        this.HWBOMFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 18:
        this.HWBOMFolderBrowse = (System.Windows.Controls.Button) target;
        this.HWBOMFolderBrowse.Click += new RoutedEventHandler(this.HWBOMFolderBrowse_Click);
        break;
      case 19:
        this.HWTBPopFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 20:
        this.HWTBPopFolderBrowse = (System.Windows.Controls.Button) target;
        this.HWTBPopFolderBrowse.Click += new RoutedEventHandler(this.HWTBPopFolderBrowse_Click);
        break;
      case 21:
        this.TMCFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 22:
        this.TMCFolderBrowse = (System.Windows.Controls.Button) target;
        this.TMCFolderBrowse.Click += new RoutedEventHandler(this.TMCFolderBrowse_Click);
        break;
      case 23:
        this.CADExFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 24:
        this.CADExFolderBrowse = (System.Windows.Controls.Button) target;
        this.CADExFolderBrowse.Click += new RoutedEventHandler(this.CADExFolderBrowse_Click);
        break;
      case 25:
        this.InsulationDrawingSettingsFolderPath = (System.Windows.Controls.TextBox) target;
        break;
      case 26:
        this.InsulationDrawingSettingsFolderBrowse = (System.Windows.Controls.Button) target;
        this.InsulationDrawingSettingsFolderBrowse.Click += new RoutedEventHandler(this.InsulationDrawingSettingsFolderBrowse_Click);
        break;
      case 27:
        this.RebarExpander = (Expander) target;
        break;
      case 28:
        this.Edge_Rebar_Mark_Updater = (System.Windows.Controls.CheckBox) target;
        this.Edge_Rebar_Mark_Updater.Checked += new RoutedEventHandler(this.Edge_Rebar_Mark_Updater_Checked);
        this.Edge_Rebar_Mark_Updater.Unchecked += new RoutedEventHandler(this.Edge_Rebar_Mark_Updater_UnChecked);
        break;
      case 29:
        this.RibbonExpander = (Expander) target;
        break;
      case 30:
        this.Annotation_Panel = (System.Windows.Controls.CheckBox) target;
        this.Annotation_Panel.Checked += new RoutedEventHandler(this.Annotation_Panel_Checked);
        this.Annotation_Panel.Unchecked += new RoutedEventHandler(this.Annotation_Panel_UnChecked);
        break;
      case 31 /*0x1F*/:
        this.AnnotationPanelItem = (Grid) target;
        break;
      case 32 /*0x20*/:
        this.Annotation_TextUtilities = (System.Windows.Controls.CheckBox) target;
        this.Annotation_TextUtilities.Checked += new RoutedEventHandler(this.Annotation_TextUtilities_Checked);
        this.Annotation_TextUtilities.Unchecked += new RoutedEventHandler(this.Annotation_TextUtilities_UnChecked);
        break;
      case 33:
        this.Annotation_DimensionUtilities = (System.Windows.Controls.CheckBox) target;
        this.Annotation_DimensionUtilities.Checked += new RoutedEventHandler(this.Annotation_DimensionUtilities_Checked);
        this.Annotation_DimensionUtilities.Unchecked += new RoutedEventHandler(this.Annotation_DimensionUtilities_UnChecked);
        break;
      case 34:
        this.Annotation_AutoDim_EDWG = (System.Windows.Controls.CheckBox) target;
        this.Annotation_AutoDim_EDWG.Checked += new RoutedEventHandler(this.Annotation_AutoDim_EDWG_Checked);
        this.Annotation_AutoDim_EDWG.Unchecked += new RoutedEventHandler(this.Annotation_AutoDim_EDWG_UnChecked);
        break;
      case 35:
        this.Freeze_Active_View = (System.Windows.Controls.CheckBox) target;
        this.Freeze_Active_View.Checked += new RoutedEventHandler(this.Freeze_Active_View_Checked);
        this.Freeze_Active_View.Unchecked += new RoutedEventHandler(this.Freeze_Active_View_UnChecked);
        break;
      case 36:
        this.Geometry_Panel = (System.Windows.Controls.CheckBox) target;
        this.Geometry_Panel.Checked += new RoutedEventHandler(this.Geometry_Panel_Checked);
        this.Geometry_Panel.Unchecked += new RoutedEventHandler(this.Geometry_Panel_UnChecked);
        break;
      case 37:
        this.GeometryPanelItem = (Grid) target;
        break;
      case 38:
        this.Geometry_AddonHostingUpdater = (System.Windows.Controls.CheckBox) target;
        this.Geometry_AddonHostingUpdater.Checked += new RoutedEventHandler(this.Geometry_AddonHostingUpdater_Checked);
        this.Geometry_AddonHostingUpdater.Unchecked += new RoutedEventHandler(this.Geometry_AddonHostingUpdater_UnChecked);
        break;
      case 39:
        this.Geometry_MultiVoidTools = (System.Windows.Controls.CheckBox) target;
        this.Geometry_MultiVoidTools.Checked += new RoutedEventHandler(this.Geometry_MultiVoidTools_Checked);
        this.Geometry_MultiVoidTools.Unchecked += new RoutedEventHandler(this.Geometry_MultiVoidTools_UnChecked);
        break;
      case 40:
        this.Geometry_WarpedParameterCopier = (System.Windows.Controls.CheckBox) target;
        this.Geometry_WarpedParameterCopier.Checked += new RoutedEventHandler(this.Geometry_WarpedParameterCopier_Checked);
        this.Geometry_WarpedParameterCopier.Unchecked += new RoutedEventHandler(this.Geometry_WarpedParameterCopier_UnChecked);
        break;
      case 41:
        this.Geometry_MarkOppTools = (System.Windows.Controls.CheckBox) target;
        this.Geometry_MarkOppTools.Checked += new RoutedEventHandler(this.Geometry_MarkOppTools_Checked);
        this.Geometry_MarkOppTools.Unchecked += new RoutedEventHandler(this.Geometry_MarkOppTools_UnChecked);
        break;
      case 42:
        this.Geometry_MarkVerificationInitial = (System.Windows.Controls.CheckBox) target;
        this.Geometry_MarkVerificationInitial.Checked += new RoutedEventHandler(this.Geometry_MarkVerificationInitial_Checked);
        this.Geometry_MarkVerificationInitial.Unchecked += new RoutedEventHandler(this.Geometry_MarkVerificationInitial_UnChecked);
        break;
      case 43:
        this.Geometry_MarkVerificationExisting = (System.Windows.Controls.CheckBox) target;
        this.Geometry_MarkVerificationExisting.Checked += new RoutedEventHandler(this.Geometry_MarkVerificationExisting_Checked);
        this.Geometry_MarkVerificationExisting.Unchecked += new RoutedEventHandler(this.Geometry_MarkVerificationExisting_UnChecked);
        break;
      case 44:
        this.Geometry_VoidHostingUpdater = (System.Windows.Controls.CheckBox) target;
        this.Geometry_VoidHostingUpdater.Checked += new RoutedEventHandler(this.Geometry_VoidHostingUpdater_Checked);
        this.Geometry_VoidHostingUpdater.Unchecked += new RoutedEventHandler(this.Geometry_VoidHostingUpdater_UnChecked);
        break;
      case 45:
        this.Geometry_NewReferencePoint = (System.Windows.Controls.CheckBox) target;
        this.Geometry_NewReferencePoint.Checked += new RoutedEventHandler(this.Geometry_NewReferencePoint_Checked);
        this.Geometry_NewReferencePoint.Unchecked += new RoutedEventHandler(this.Geometry_NewReferencePoint_UnChecked);
        break;
      case 46:
        this.Geometry_GetCentroid = (System.Windows.Controls.CheckBox) target;
        this.Geometry_GetCentroid.Checked += new RoutedEventHandler(this.Geometry_GetCentroid_Checked);
        this.Geometry_GetCentroid.Unchecked += new RoutedEventHandler(this.Geometry_GetCentroid_UnChecked);
        break;
      case 47:
        this.Geometry_AutoWarping = (System.Windows.Controls.CheckBox) target;
        this.Geometry_AutoWarping.Checked += new RoutedEventHandler(this.Geometry_AutoWarping_Checked);
        this.Geometry_AutoWarping.Unchecked += new RoutedEventHandler(this.Geometry_AutoWarping_UnChecked);
        break;
      case 48 /*0x30*/:
        this.Insulation_Panel = (System.Windows.Controls.CheckBox) target;
        this.Insulation_Panel.Checked += new RoutedEventHandler(this.Insulation_Panel_Checked);
        this.Insulation_Panel.Unchecked += new RoutedEventHandler(this.Insulation_Panel_UnChecked);
        break;
      case 49:
        this.InsulationPanelItem = (Grid) target;
        break;
      case 50:
        this.Insulation_InsulationRemoval = (System.Windows.Controls.CheckBox) target;
        this.Insulation_InsulationRemoval.Checked += new RoutedEventHandler(this.Insulation_InsulationRemoval_Checked);
        this.Insulation_InsulationRemoval.Unchecked += new RoutedEventHandler(this.Insulation_InsulationRemoval_Unchecked);
        break;
      case 51:
        this.Insulation_AutoInsulationPlacement = (System.Windows.Controls.CheckBox) target;
        this.Insulation_AutoInsulationPlacement.Checked += new RoutedEventHandler(this.Insulation_AutoInsulationPlacement_Checked);
        this.Insulation_AutoInsulationPlacement.Unchecked += new RoutedEventHandler(this.Insulation_AutoInsulationPlacement_Unchecked);
        break;
      case 52:
        this.Insulation_ManualInsulationPlacement = (System.Windows.Controls.CheckBox) target;
        this.Insulation_ManualInsulationPlacement.Checked += new RoutedEventHandler(this.Insulation_ManualInsulationPlacement_Checked);
        this.Insulation_ManualInsulationPlacement.Unchecked += new RoutedEventHandler(this.Insulation_ManualInsulationPlacement_Unchecked);
        break;
      case 53:
        this.Insulation_AutoPinPlacement = (System.Windows.Controls.CheckBox) target;
        this.Insulation_AutoPinPlacement.Checked += new RoutedEventHandler(this.Insulation_AutoPinPlacement_Checked);
        this.Insulation_AutoPinPlacement.Unchecked += new RoutedEventHandler(this.Insulation_AutoPinPlacement_Unchecked);
        break;
      case 54:
        this.Insulation_InsulationMarking = (System.Windows.Controls.CheckBox) target;
        this.Insulation_InsulationMarking.Checked += new RoutedEventHandler(this.Insulation_InsulationMarking_Checked);
        this.Insulation_InsulationMarking.Unchecked += new RoutedEventHandler(this.Insulation_InsulationMarking_Unchecked);
        break;
      case 55:
        this.Insulation_InsulationExport = (System.Windows.Controls.CheckBox) target;
        this.Insulation_InsulationExport.Checked += new RoutedEventHandler(this.Insulation_InsulationExport_Checked);
        this.Insulation_InsulationExport.Unchecked += new RoutedEventHandler(this.Insulation_InsulationExport_Unchecked);
        break;
      case 56:
        this.Insulation_InsulationDrawingMaster = (System.Windows.Controls.CheckBox) target;
        this.Insulation_InsulationDrawingMaster.Checked += new RoutedEventHandler(this.Insulation_InsulationDrawingMaster_Checked);
        this.Insulation_InsulationDrawingMaster.Unchecked += new RoutedEventHandler(this.Insulation_InsulationDrawingMaster_Unchecked);
        break;
      case 57:
        this.Insulation_InsulationDrawingPerWall = (System.Windows.Controls.CheckBox) target;
        this.Insulation_InsulationDrawingPerWall.Checked += new RoutedEventHandler(this.Insulation_InsulationDrawingPerWall_Checked);
        this.Insulation_InsulationDrawingPerWall.Unchecked += new RoutedEventHandler(this.Insulation_InsulationDrawingPerWall_Unchecked);
        break;
      case 58:
        this.Insulation_InsulationDrawing = (System.Windows.Controls.CheckBox) target;
        this.Insulation_InsulationDrawing.Checked += new RoutedEventHandler(this.Insulation_InsulationDrawing_Checked);
        this.Insulation_InsulationDrawing.Unchecked += new RoutedEventHandler(this.Insulation_InsulationDrawing_Unchecked);
        break;
      case 59:
        this.Visibility_Panel = (System.Windows.Controls.CheckBox) target;
        this.Visibility_Panel.Checked += new RoutedEventHandler(this.Visibility_Panel_Checked);
        this.Visibility_Panel.Unchecked += new RoutedEventHandler(this.Visibility_Panel_UnChecked);
        break;
      case 60:
        this.SelectionAndPinning_Panel = (System.Windows.Controls.CheckBox) target;
        this.SelectionAndPinning_Panel.Checked += new RoutedEventHandler(this.SelectionAndPinning_Panel_Checked);
        this.SelectionAndPinning_Panel.Unchecked += new RoutedEventHandler(this.SelectionAndPinning_Panel_UnChecked);
        break;
      case 61:
        this.SelectionAndPinningPanelItem = (Grid) target;
        break;
      case 62:
        this.SelectionAndPinning_Grids = (System.Windows.Controls.CheckBox) target;
        this.SelectionAndPinning_Grids.Checked += new RoutedEventHandler(this.SelectionAndPinning_Grids_Checked);
        this.SelectionAndPinning_Grids.Unchecked += new RoutedEventHandler(this.SelectionAndPinning_Grids_UnChecked);
        break;
      case 63 /*0x3F*/:
        this.SelectionAndPinning_Levels = (System.Windows.Controls.CheckBox) target;
        this.SelectionAndPinning_Levels.Checked += new RoutedEventHandler(this.SelectionAndPinning_Levels_Checked);
        this.SelectionAndPinning_Levels.Unchecked += new RoutedEventHandler(this.SelectionAndPinning_Levels_UnChecked);
        break;
      case 64 /*0x40*/:
        this.SelectionAndPinning_SelectSFElements = (System.Windows.Controls.CheckBox) target;
        this.SelectionAndPinning_SelectSFElements.Checked += new RoutedEventHandler(this.SelectionAndPinning_SelectSFElements_Checked);
        this.SelectionAndPinning_SelectSFElements.Unchecked += new RoutedEventHandler(this.SelectionAndPinning_SelectSFElements_UnChecked);
        break;
      case 65:
        this.Scheduling_Panel = (System.Windows.Controls.CheckBox) target;
        this.Scheduling_Panel.Checked += new RoutedEventHandler(this.Scheduling_Panel_Checked);
        this.Scheduling_Panel.Unchecked += new RoutedEventHandler(this.Scheduling_Panel_UnChecked);
        break;
      case 66:
        this.SchedulingPanelItem = (Grid) target;
        break;
      case 67:
        this.Scheduling_ControlNumberIncrementor = (System.Windows.Controls.CheckBox) target;
        this.Scheduling_ControlNumberIncrementor.Checked += new RoutedEventHandler(this.Scheduling_ControlNumberIncrementor_Checked);
        this.Scheduling_ControlNumberIncrementor.Unchecked += new RoutedEventHandler(this.Scheduling_ControlNumberIncrementor_UnChecked);
        break;
      case 68:
        this.Scheduling_BomProductHosting = (System.Windows.Controls.CheckBox) target;
        this.Scheduling_BomProductHosting.Checked += new RoutedEventHandler(this.Scheduling_BomProductHosting_Checked);
        this.Scheduling_BomProductHosting.Unchecked += new RoutedEventHandler(this.Scheduling_BomProductHosting_UnChecked);
        break;
      case 69:
        this.Scheduling_ConstructionProductHosting = (System.Windows.Controls.CheckBox) target;
        this.Scheduling_ConstructionProductHosting.Checked += new RoutedEventHandler(this.Scheduling_ConstructionProductHosting_Checked);
        this.Scheduling_ConstructionProductHosting.Unchecked += new RoutedEventHandler(this.Scheduling_ConstructionProductHosting_UnChecked);
        break;
      case 70:
        this.Scheduling_ScheduleSequenceByView = (System.Windows.Controls.CheckBox) target;
        this.Scheduling_ScheduleSequenceByView.Checked += new RoutedEventHandler(this.Scheduling_ScheduleSequenceByView_Checked);
        this.Scheduling_ScheduleSequenceByView.Unchecked += new RoutedEventHandler(this.Scheduling_ScheduleSequenceByView_UnChecked);
        break;
      case 71:
        this.Scheduling_Erection_Sequence = (System.Windows.Controls.CheckBox) target;
        this.Scheduling_Erection_Sequence.Checked += new RoutedEventHandler(this.Scheduling_Erection_Sequence_Checked);
        this.Scheduling_Erection_Sequence.Unchecked += new RoutedEventHandler(this.Scheduling_Erection_Sequence_UnChecked);
        break;
      case 72:
        this.Assembly_Panel = (System.Windows.Controls.CheckBox) target;
        this.Assembly_Panel.Checked += new RoutedEventHandler(this.Assembly_Panel_Checked);
        this.Assembly_Panel.Unchecked += new RoutedEventHandler(this.Assembly_Panel_UnChecked);
        break;
      case 73:
        this.AssemblyPanelItem = (Grid) target;
        break;
      case 74:
        this.Assembly_CreationUpdatingRenaming = (System.Windows.Controls.CheckBox) target;
        this.Assembly_CreationUpdatingRenaming.Checked += new RoutedEventHandler(this.Assembly_CreationUpdatingRenaming_Checked);
        this.Assembly_CreationUpdatingRenaming.Unchecked += new RoutedEventHandler(this.Assembly_CreationUpdatingRenaming_UnChecked);
        break;
      case 75:
        this.Assembly_CreationUpdatingRevit = (System.Windows.Controls.CheckBox) target;
        this.Assembly_CreationUpdatingRevit.Checked += new RoutedEventHandler(this.Assembly_CreationUpdatingRevit_Checked);
        this.Assembly_CreationUpdatingRevit.Unchecked += new RoutedEventHandler(this.Assembly_CreationUpdatingRevit_UnChecked);
        break;
      case 76:
        this.Assembly_CopyReinforcing = (System.Windows.Controls.CheckBox) target;
        this.Assembly_CopyReinforcing.Checked += new RoutedEventHandler(this.Assembly_CopyReinforcing_Checked);
        this.Assembly_CopyReinforcing.Unchecked += new RoutedEventHandler(this.Assembly_CopyReinforcing_UnChecked);
        break;
      case 77:
        this.Assembly_MarkAsReinforced = (System.Windows.Controls.CheckBox) target;
        this.Assembly_MarkAsReinforced.Checked += new RoutedEventHandler(this.Assembly_MarkAsReinforced_Checked);
        this.Assembly_MarkAsReinforced.Unchecked += new RoutedEventHandler(this.Assembly_MarkAsReinforced_UnChecked);
        break;
      case 78:
        this.Assembly_TopAsCast = (System.Windows.Controls.CheckBox) target;
        this.Assembly_TopAsCast.Checked += new RoutedEventHandler(this.Assembly_TopAsCast_Checked);
        this.Assembly_TopAsCast.Unchecked += new RoutedEventHandler(this.Assembly_TopAsCast_UnChecked);
        break;
      case 79:
        this.Assembly_MatchTopAsCast = (System.Windows.Controls.CheckBox) target;
        this.Assembly_MatchTopAsCast.Checked += new RoutedEventHandler(this.Assembly_MatchTopAsCast_Checked);
        this.Assembly_MatchTopAsCast.Unchecked += new RoutedEventHandler(this.Assembly_MatchTopAsCast_UnChecked);
        break;
      case 80 /*0x50*/:
        this.TicketTools_Panel = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_Panel.Checked += new RoutedEventHandler(this.TicketTools_Panel_Checked);
        this.TicketTools_Panel.Unchecked += new RoutedEventHandler(this.TicketTools_Panel_UnChecked);
        break;
      case 81:
        this.TicketToolsPanelItem = (Grid) target;
        break;
      case 82:
        this.TicketTools_AnnotateEmbedLong = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_AnnotateEmbedLong.Checked += new RoutedEventHandler(this.TicketTools_AnnotateEmbedLong_Checked);
        this.TicketTools_AnnotateEmbedLong.Unchecked += new RoutedEventHandler(this.TicketTools_AnnotateEmbedLong_UnChecked);
        break;
      case 83:
        this.TicketTools_AnnotateEmbedShort = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_AnnotateEmbedShort.Checked += new RoutedEventHandler(this.TicketTools_AnnotateEmbedShort_Checked);
        this.TicketTools_AnnotateEmbedShort.Unchecked += new RoutedEventHandler(this.TicketTools_AnnotateEmbedShort_UnChecked);
        break;
      case 84:
        this.TicketTools_ControlNumberPopulator = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_ControlNumberPopulator.Checked += new RoutedEventHandler(this.TicketTools_ControlNumberPopulator_Checked);
        this.TicketTools_ControlNumberPopulator.Unchecked += new RoutedEventHandler(this.TicketTools_ControlNumberPopulator_UnChecked);
        break;
      case 85:
        this.TicketTools_TicketBomDuplicatesSchedule = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_TicketBomDuplicatesSchedule.Checked += new RoutedEventHandler(this.TicketTools_TicketBomDuplicatesSchedule_Checked);
        this.TicketTools_TicketBomDuplicatesSchedule.Unchecked += new RoutedEventHandler(this.TicketTools_TicketBomDuplicatesSchedule_UnChecked);
        break;
      case 86:
        this.TicketTools_TicketTitleBlockPopulator = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_TicketTitleBlockPopulator.Checked += new RoutedEventHandler(this.TicketTools_TicketTitleBlockPopulator_Checked);
        this.TicketTools_TicketTitleBlockPopulator.Unchecked += new RoutedEventHandler(this.TicketTools_TicketTitleBlockPopulator_UnChecked);
        break;
      case 87:
        this.TicketTools_BatchTicketTitleBlockPopulator = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_BatchTicketTitleBlockPopulator.Checked += new RoutedEventHandler(this.TicketTools_BatchTicketTitleBlockPopulator_Checked);
        this.TicketTools_BatchTicketTitleBlockPopulator.Unchecked += new RoutedEventHandler(this.TicketTools_BatchTicketTitleBlockPopulator_Unchecked);
        break;
      case 88:
        this.TicketTools_CopyTicketViews = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_CopyTicketViews.Checked += new RoutedEventHandler(this.TicketTools_CopyTicketViews_Checked);
        this.TicketTools_CopyTicketViews.Unchecked += new RoutedEventHandler(this.TicketTools_CopyTicketViews_UnChecked);
        break;
      case 89:
        this.TicketTools_CopyTicketAnnotations = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_CopyTicketAnnotations.Checked += new RoutedEventHandler(this.TicketTools_CopyTicketAnnotations_Checked);
        this.TicketTools_CopyTicketAnnotations.Unchecked += new RoutedEventHandler(this.TicketTools_CopyTicketAnnotations_UnChecked);
        break;
      case 90:
        this.TicketTools_FindReferringViews = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_FindReferringViews.Checked += new RoutedEventHandler(this.TicketTools_FindReferringViews_Checked);
        this.TicketTools_FindReferringViews.Unchecked += new RoutedEventHandler(this.TicketTools_FindReferringViews_UnChecked);
        break;
      case 91:
        this.TicketTools_TicketPopulator = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_TicketPopulator.Checked += new RoutedEventHandler(this.TicketTools_TicketPopulator_Checked);
        this.TicketTools_TicketPopulator.Unchecked += new RoutedEventHandler(this.TicketTools_TicketPopulator_UnChecked);
        break;
      case 92:
        this.TicketTools_BatchTicketPopulator = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_BatchTicketPopulator.Checked += new RoutedEventHandler(this.TicketTools_BatchTicketPopulator_Checked);
        this.TicketTools_BatchTicketPopulator.Unchecked += new RoutedEventHandler(this.TicketTools_BatchTicketPopulator_Unchecked);
        break;
      case 93:
        this.TicketTools_AutoDimension = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_AutoDimension.Checked += new RoutedEventHandler(this.TicketTools_AutoDimension_Checked);
        this.TicketTools_AutoDimension.Unchecked += new RoutedEventHandler(this.TicketTools_AutoDimension_UnChecked);
        break;
      case 94:
        this.TicketTools_AutoTicket = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_AutoTicket.Checked += new RoutedEventHandler(this.TicketTools_AutoTicket_Checked);
        this.TicketTools_AutoTicket.Unchecked += new RoutedEventHandler(this.TicketTools_AutoTicket_UnChecked);
        break;
      case 95:
        this.TicketTools_CloneTicket = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_CloneTicket.Checked += new RoutedEventHandler(this.TicketTools_CloneTicket_Checked);
        this.TicketTools_CloneTicket.Unchecked += new RoutedEventHandler(this.TicketTools_CloneTicket_UnChecked);
        break;
      case 96 /*0x60*/:
        this.TicketTools_LaserExport = (System.Windows.Controls.CheckBox) target;
        this.TicketTools_LaserExport.Checked += new RoutedEventHandler(this.TicketTools_LaserExport_Checked);
        this.TicketTools_LaserExport.Unchecked += new RoutedEventHandler(this.TicketTools_LaserExport_UnChecked);
        break;
      case 97:
        this.HardwareTools_Panel = (System.Windows.Controls.CheckBox) target;
        this.HardwareTools_Panel.Checked += new RoutedEventHandler(this.HardwareTools_Panel_Checked);
        this.HardwareTools_Panel.Unchecked += new RoutedEventHandler(this.HardwareTools_Panel_Unchecked);
        break;
      case 98:
        this.HardwareToolsPanelItem = (Grid) target;
        break;
      case 99:
        this.HardwareTools_HardwareDetail = (System.Windows.Controls.CheckBox) target;
        this.HardwareTools_HardwareDetail.Checked += new RoutedEventHandler(this.HardwareTools_HardwareDetail_Checked);
        this.HardwareTools_HardwareDetail.Unchecked += new RoutedEventHandler(this.HardwareTools_HardwareDetail_Unchecked);
        break;
      case 100:
        this.HardwareTools_HardwareDetailTemplateManager = (System.Windows.Controls.CheckBox) target;
        this.HardwareTools_HardwareDetailTemplateManager.Checked += new RoutedEventHandler(this.HardwareTools_HardwareDetailTemplateManager_Checked);
        this.HardwareTools_HardwareDetailTemplateManager.Unchecked += new RoutedEventHandler(this.HardwareTools_HardwareDetailTemplateManager_Unchecked);
        break;
      case 101:
        this.HardwareTools_HWTitleBlockPopulatorSettings = (System.Windows.Controls.CheckBox) target;
        this.HardwareTools_HWTitleBlockPopulatorSettings.Checked += new RoutedEventHandler(this.HardwareTools_HWTitleBlockPopulatorSettings_Checked);
        this.HardwareTools_HWTitleBlockPopulatorSettings.Unchecked += new RoutedEventHandler(this.HardwareTools_HWTitleBlockPopulatorSettings_Unchecked);
        break;
      case 102:
        this.HardwareTools_HardwareDetailBOMSettings = (System.Windows.Controls.CheckBox) target;
        this.HardwareTools_HardwareDetailBOMSettings.Checked += new RoutedEventHandler(this.HardwareTools_HardwareDetailBOMSettings_Checked);
        this.HardwareTools_HardwareDetailBOMSettings.Unchecked += new RoutedEventHandler(this.HardwareTools_HardwareDetailBOMSettings_Unchecked);
        break;
      case 103:
        this.TicketManager_Panel = (System.Windows.Controls.CheckBox) target;
        this.TicketManager_Panel.Checked += new RoutedEventHandler(this.TicketManager_Panel_Checked);
        this.TicketManager_Panel.Unchecked += new RoutedEventHandler(this.TicketManager_Panel_UnChecked);
        break;
      case 104:
        this.FamilyTools_Panel = (System.Windows.Controls.CheckBox) target;
        this.FamilyTools_Panel.Checked += new RoutedEventHandler(this.FamilyTools_Panel_Checked);
        this.FamilyTools_Panel.Unchecked += new RoutedEventHandler(this.FamilyTools_Panel_UnChecked);
        break;
      case 105:
        this.UserSettingsPanelItem = (Grid) target;
        break;
      case 106:
        this.UserSettings_TicketAndDimensionSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_TicketAndDimensionSettings.Checked += new RoutedEventHandler(this.UserSettings_TicketAndDimensionSettings_Checked);
        this.UserSettings_TicketAndDimensionSettings.Unchecked += new RoutedEventHandler(this.UserSettings_TicketAndDimensionSettings_Unchecked);
        break;
      case 107:
        this.UserSettings_RebarSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_RebarSettings.Checked += new RoutedEventHandler(this.UserSettings_RebarSettings_Checked);
        this.UserSettings_RebarSettings.Unchecked += new RoutedEventHandler(this.UserSettings_RebarSettings_Unchecked);
        break;
      case 108:
        this.UserSettings_MarkVerificationSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_MarkVerificationSettings.Checked += new RoutedEventHandler(this.UserSettings_MarkVerificationSettings_Checked);
        this.UserSettings_MarkVerificationSettings.Unchecked += new RoutedEventHandler(this.UserSettings_MarkVerificationSettings_Unchecked);
        break;
      case 109:
        this.UserSettings_TicketManagerCustomizationSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_TicketManagerCustomizationSettings.Checked += new RoutedEventHandler(this.UserSettings_TicketManagerCustomizationSettings_Checked);
        this.UserSettings_TicketManagerCustomizationSettings.Unchecked += new RoutedEventHandler(this.UserSettings_TicketManagerCustomizationSettings_Unchecked);
        break;
      case 110:
        this.UserSettings_TBPSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_TBPSettings.Checked += new RoutedEventHandler(this.UserSettings_TBPSettings_Checked);
        this.UserSettings_TBPSettings.Unchecked += new RoutedEventHandler(this.UserSettings_TBPSettings_UnChecked);
        break;
      case 111:
        this.UserSettings_TicketBOMSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_TicketBOMSettings.Checked += new RoutedEventHandler(this.UserSettings_TicketBOMSettings_Checked);
        this.UserSettings_TicketBOMSettings.Unchecked += new RoutedEventHandler(this.UserSettings_TicketBOMSettings_UnChecked);
        break;
      case 112 /*0x70*/:
        this.UserSettings_CADExportSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_CADExportSettings.Checked += new RoutedEventHandler(this.UserSettings_CADExportSettings_Checked);
        this.UserSettings_CADExportSettings.Unchecked += new RoutedEventHandler(this.UserSettings_CADExportSettings_UnChecked);
        break;
      case 113:
        this.UserSettings_InsulationDrawingSettings = (System.Windows.Controls.CheckBox) target;
        this.UserSettings_InsulationDrawingSettings.Checked += new RoutedEventHandler(this.UserSettings_InsulationDrawingSettings_Checked);
        this.UserSettings_InsulationDrawingSettings.Unchecked += new RoutedEventHandler(this.UserSettings_InsulationDrawingSettings_UnChecked);
        break;
      case 114:
        this.Admin_Panel = (System.Windows.Controls.CheckBox) target;
        this.Admin_Panel.Checked += new RoutedEventHandler(this.Admin_Panel_Checked);
        this.Admin_Panel.Unchecked += new RoutedEventHandler(this.Admin_Panel_UnChecked);
        break;
      case 115:
        this.AdminPanelItem = (Grid) target;
        break;
      case 116:
        this.Admin_SharedParameterUpdater = (System.Windows.Controls.CheckBox) target;
        this.Admin_SharedParameterUpdater.Checked += new RoutedEventHandler(this.Admin_SharedParameterUpdater_Checked);
        this.Admin_SharedParameterUpdater.Unchecked += new RoutedEventHandler(this.Admin_SharedParameterUpdater_UnChecked);
        break;
      case 117:
        this.Admin_WarningAnalyzer = (System.Windows.Controls.CheckBox) target;
        this.Admin_WarningAnalyzer.Checked += new RoutedEventHandler(this.Admin_WarningAnalyzer_Checked);
        this.Admin_WarningAnalyzer.Unchecked += new RoutedEventHandler(this.Admin_WarningAnalyzer_UnChecked);
        break;
      case 118:
        this.Admin_EmbedClashVerification = (System.Windows.Controls.CheckBox) target;
        this.Admin_EmbedClashVerification.Checked += new RoutedEventHandler(this.Admin_EmbedClashVerification_Checked);
        this.Admin_EmbedClashVerification.Unchecked += new RoutedEventHandler(this.Admin_EmbedClashVerification_UnChecked);
        break;
      case 119:
        this.Admin_BphCphParameterErrorFinder = (System.Windows.Controls.CheckBox) target;
        this.Admin_BphCphParameterErrorFinder.Checked += new RoutedEventHandler(this.Admin_BphCphParameterErrorFinder_Checked);
        this.Admin_BphCphParameterErrorFinder.Unchecked += new RoutedEventHandler(this.Admin_BphCphParameterErrorFinder_UnChecked);
        break;
      case 120:
        this.Admin_TicketTemplateCreator = (System.Windows.Controls.CheckBox) target;
        this.Admin_TicketTemplateCreator.Checked += new RoutedEventHandler(this.Admin_TicketTemplateCreator_Checked);
        this.Admin_TicketTemplateCreator.Unchecked += new RoutedEventHandler(this.Admin_TicketTemplateCreator_UnChecked);
        break;
      case 121:
        this.Admin_BulkFamilyUpdater = (System.Windows.Controls.CheckBox) target;
        this.Admin_BulkFamilyUpdater.Checked += new RoutedEventHandler(this.Admin_BulkFamilyUpdater_Checked);
        this.Admin_BulkFamilyUpdater.Unchecked += new RoutedEventHandler(this.Admin_BulkFamilyUpdater_UnChecked);
        break;
      case 122:
        this.Admin_CloudSettings = (System.Windows.Controls.CheckBox) target;
        this.Admin_CloudSettings.Checked += new RoutedEventHandler(this.Admin_CloudSettings_Checked);
        this.Admin_CloudSettings.Unchecked += new RoutedEventHandler(this.Admin_CloudSettings_UnChecked);
        break;
      case 123:
        this.Admin_CAMSettings = (System.Windows.Controls.CheckBox) target;
        this.Admin_CAMSettings.Checked += new RoutedEventHandler(this.Admin_CAMSettings_Checked);
        this.Admin_CAMSettings.Unchecked += new RoutedEventHandler(this.Admin_CAMSettings_UnChecked);
        break;
      case 124:
        this.Admin_EDGEBrowserSettings = (System.Windows.Controls.CheckBox) target;
        this.Admin_EDGEBrowserSettings.Checked += new RoutedEventHandler(this.Admin_EDGEBrowserSettings_Checked);
        this.Admin_EDGEBrowserSettings.Unchecked += new RoutedEventHandler(this.Admin_EDGEBrowserSettings_UnChecked);
        break;
      case 125:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.ApplyButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
