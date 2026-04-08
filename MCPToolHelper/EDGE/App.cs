// Decompiled with JetBrains decompiler
// Type: EDGE.App
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using EDGE.AdminTools;
using EDGE.ApplicationRibbon;
using EDGE.AssemblyTools;
using EDGE.AssemblyTools.CreateAssemblyViews;
using EDGE.AssemblyTools.MarkVerification.InitialPresentation;
using EDGE.AssemblyTools.MarkVerification.ResultsPresentation;
using EDGE.EDGEBrowser.Forms;
using EDGE.EDGECollections;
using EDGE.InsulationTools.InsulationDrawing.Views;
using EDGE.IUpdaters;
using EDGE.IUpdaters.ModelLocking;
using EDGE.QATools;
using EDGE.RebarTools;
using EDGE.TicketTools.HardwareDetail.Views;
using EDGE.TicketTools.TicketManager.ViewModels;
using EDGE.TicketTools.TicketManager.Views;
using EDGE.TicketTools.TicketPopulator;
using EDGE.TicketTools.TicketTemplateTools.Views;
using EDGE.VisibilityTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Utils;
using Utils.AdminUtils;
using Utils.ElementUtils;
using Utils.Forms;
using Utils.ProjectUtils;
using Utils.RebarTools;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE;

internal class App : IExternalApplication
{
  public static bool bSyncingWithCentral = false;
  public static MainWindow TicketManagerWindow = (MainWindow) null;
  public static MKVerificationResult_Initial MarkVerificationInitialWindow = (MKVerificationResult_Initial) null;
  public static MKVerificationResults_Existing MarkVerificationExistingWindow = (MKVerificationResults_Existing) null;
  public static InsulationDrawingWindow insulationDrawingWindow = (InsulationDrawingWindow) null;
  public static List<AssemblyInstance> GlobalValueForQuantityIssue = new List<AssemblyInstance>();
  public static List<AssemblyInstance> GlobalValueForViewSheetIssue = new List<AssemblyInstance>();
  public static List<AssemblyInstance> GlobalValueForViewSheetCountIssue = new List<AssemblyInstance>();
  public static Dictionary<string, string> sheetAndAsssemblyIds = new Dictionary<string, string>();
  public static bool checkSheetAndAssemblyIds = true;
  public static List<AssemblyViewModel> releasedAssemblies = new List<AssemblyViewModel>();
  public static PaneWindow ParentPaneWindowRef;
  public static bool PaneOpen = true;
  public static HighlightEvent MarkVerificationSolidToBeHighlighted;
  public static string JournalPlaybackDirectoryPath = "";
  public static bool RebarMarkAutomation = false;
  public static bool ModelLocking = false;
  public static bool ViewSpreadsheet = true;
  public static bool DockablePane = false;
  public static string TicketTemplateSettingsPath = "";
  public static string HardwareDetailTemplateSettingsPath = "";
  public static string RebarSettingsFolderPath = "";
  public static string MarkPrefixFolderPath = "";
  public static string TKTBOMFolderPath = "";
  public static string HWBOMFolderPath = "";
  public static string TBPopFolderPath = "";
  public static string HWTBPopFolderPath = "";
  public static string TMCFolderPath = "";
  public static string MLFolderPath = "";
  public static string AutoTicketFolderPath = "";
  public static string CADExFolderPath = "";
  public static string InsulationDrawingPath = "";
  public static int Edge_Cloud_Id = -1;
  public static List<string> ModelLockingAdminList = new List<string>();
  public static RebarSessionManager RebarManager;
  public static TicketTemplatesHistoryManager TemplateHistoryManager;
  public static string CloudTokenForRevitSession;
  public static ModelLockingManager LockingManger;
  public static AppSwitches DialogSwitches;
  public static Queue<AVFViewTimer> AVFViewTimers;
  private static string s_commandToDisable_SWCModify = "ID_FILE_SAVE_TO_MASTER";
  private static string s_commandToDisable_RL = "ID_WORKSETS_RELOAD_LATEST";
  private static string s_commandToDisable_SWCQuick = "ID_FILE_SAVE_TO_MASTER_SHORTCUT";
  private static string s_commandToDisable_CreateAssemblyViews = "ID_CREATE_SHOP_DRAWING";
  public static DockablePaneId paneId = new DockablePaneId(new Guid("{D7C963CE-B7CA-426A-8D51-6E8254D21157}"));
  public static UIControlledApplication appForPane = (UIControlledApplication) null;
  public static Document currentDocument = (Document) null;
  public static Dictionary<string, string> autoTicketSettingLongShortNames = new Dictionary<string, string>()
  {
    {
      "{{QTY}}",
      "QUANTITY"
    },
    {
      "{{LIF}}",
      "LOCATION_IN_FORM"
    },
    {
      "{{DESC}}",
      "IDENTITY_DESCRIPTION_SHORT"
    }
  };
  public static bool firstDocumentOpeningEdgeBrowser = true;
  public static TicketTemplateCreatorWindow templateCreatorWindow = (TicketTemplateCreatorWindow) null;
  public static HardwareDetailWindow hardwareDetailWindow = (HardwareDetailWindow) null;
  public static HWTemplateCreatorWindow hwTemplateCreatorWindow = (HWTemplateCreatorWindow) null;
  public static string TicketManagerExportFile = "";
  public static bool bSynchronizingWithCentral = false;
  public static readonly string ExecutingAssemblyPath = Assembly.GetExecutingAssembly().Location;
  public static IList<string> ProjectParameterWarningDialogList = (IList<string>) new List<string>();
  public static IList<string> ProjectManufacturerStandardsMissingList = (IList<string>) new List<string>();
  public static int MaximumStandardBars = 1200;
  internal static bool RebarMarkAutomation_WorksharingSupport;
  internal static bool ModelLocking_WorksharingSupport = true;
  internal static bool ProcessCountMultiplierOnDocumentOpen = true;
  private static bool ReloadingLatest;
  public static bool SpecialRebarScenario1 = false;
  public static bool default_insulation_set = false;
  public static bool defaultInsulationUseFalse = false;
  public static bool defaultInsulationUseTrue = false;
  public static bool aspectRatioSet = false;
  public static bool horizontalPlacement = false;
  public static bool ifCompositePinsExist = false;
  public static bool ifCompositePinsHorizontal = true;
  public static Dictionary<string, string> CADExportFolderPathDictionary = new Dictionary<string, string>();
  public static Dictionary<string, string> CADExportFileSuffixDictionary = new Dictionary<string, string>();
  public static Dictionary<string, bool> CADExportDWGDictionary = new Dictionary<string, bool>();
  public static Dictionary<string, bool> CADExportDXFDictionary = new Dictionary<string, bool>();
  public static Dictionary<string, string> InsulationExportFolderPathDictionary = new Dictionary<string, string>();
  public static Dictionary<string, string> InsulationExportFileSuffixDictionary = new Dictionary<string, string>();
  public static Dictionary<string, bool> InsulationExportDWGDictionary = new Dictionary<string, bool>();
  public static Dictionary<string, bool> InsulationExportDXFDictionary = new Dictionary<string, bool>();

  public Result OnStartup(UIControlledApplication application)
  {
    App.appForPane = application;
    QA.InitLog(application.ControlledApplication.RecordingJournalFilename);
    application.DockableFrameVisibilityChanged += new EventHandler<DockableFrameVisibilityChangedEventArgs>(this.ControlledApplication_DockableFrameVisibilityChanged);
    EdgeLicenseStatus edgeLicenseStatus1 = LicenseHelper.CloudValidationLicense();
    if (edgeLicenseStatus1 == EdgeLicenseStatus.NoLicense)
    {
      string eulaGuid = Guid.NewGuid().ToString();
      if (!LicenseHelper.AcceptEULA(eulaGuid))
        return (Result) 1;
      TaskDialog taskDialog = new TaskDialog("EDGE^R - No License Found");
      taskDialog.Id = "ID_License_NoLicense";
      taskDialog.Title = "EDGE^R - No License Found";
      taskDialog.TitleAutoPrefix = false;
      taskDialog.AllowCancellation = true;
      taskDialog.MainInstruction = "No License Found";
      taskDialog.MainContent = "No EDGE^R license found. Please enter your valid license code or contact support to request a license. (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>  ";
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
      {
        if (!LicenseHelper.EnterNewLicense())
          return (Result) 1;
      }
      else
      {
        if (taskDialogResult == 1002)
        {
          LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
          return (Result) 1;
        }
        if (taskDialogResult != 1003)
          return (Result) 1;
        Process.Start("http://www.edge.ptac.com/licensing");
      }
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.Deactivated)
    {
      string eulaGuid = Guid.NewGuid().ToString();
      if (!LicenseHelper.AcceptEULA(eulaGuid))
        return (Result) 1;
      TaskDialog taskDialog = new TaskDialog("EDGE^R - License Deactivated");
      taskDialog.Id = "ID_License_Error";
      taskDialog.Title = "EDGE^R - License Deactivated";
      taskDialog.TitleAutoPrefix = false;
      taskDialog.AllowCancellation = true;
      taskDialog.MainInstruction = "License Deactivated";
      taskDialog.MainContent = "License is Deactivated. Unable to open EDGE^R. Would you like to contact EDGE^R Support to request a new license or enter a valid license code? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a> ";
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
      {
        if (!LicenseHelper.EnterNewLicense())
          return (Result) 1;
      }
      else
      {
        if (taskDialogResult == 1002)
        {
          LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
          return (Result) 1;
        }
        if (taskDialogResult != 1003)
          return (Result) 1;
        Process.Start("http://www.edge.ptac.com/licensing");
      }
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.Error)
    {
      string eulaGuid = Guid.NewGuid().ToString();
      if (!LicenseHelper.AcceptEULA(eulaGuid))
        return (Result) 1;
      TaskDialog taskDialog = new TaskDialog("EDGE^R - License Error");
      taskDialog.Id = "ID_License_Error";
      taskDialog.Title = "EDGE^R - License Error";
      taskDialog.TitleAutoPrefix = false;
      taskDialog.AllowCancellation = true;
      taskDialog.MainInstruction = "License Error";
      taskDialog.MainContent = "Error with License. Unable to open EDGE^R. Would you like to contact EDGE^R Support to request a new license or enter a valid license code? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a> ";
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
      {
        if (!LicenseHelper.EnterNewLicense())
          return (Result) 1;
      }
      else
      {
        if (taskDialogResult == 1002)
        {
          LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
          return (Result) 1;
        }
        if (taskDialogResult != 1003)
          return (Result) 1;
        Process.Start("http://www.edge.ptac.com/licensing");
      }
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.NoOffline)
    {
      new TaskDialog("EDGE^R - No Offline License")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        Id = "ID_License_Expired",
        Title = "EDGE^R - No Offline License",
        TitleAutoPrefix = false,
        AllowCancellation = true,
        MainInstruction = "No Offline License",
        MainContent = "Please go online to validate your license before attempting to use EDGE^R offline. \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>"
      }.Show();
      return (Result) 1;
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.ExpiredOffline)
    {
      new TaskDialog("EDGE^R - Offline License Expired")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        Id = "ID_License_Expired",
        Title = "EDGE^R - Offline License Expired",
        TitleAutoPrefix = false,
        AllowCancellation = true,
        MainInstruction = "Offline License Expired",
        MainContent = "Usage of EDGE^R while offline has expired. Please go online to validate your license against our database. \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>"
      }.Show();
      return (Result) 1;
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.InvalidOffline)
    {
      new TaskDialog("EDGE^R - Offline License Invalid")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        Id = "ID_License_Invalid",
        Title = "EDGE^R - Offline License Invalid",
        TitleAutoPrefix = false,
        AllowCancellation = true,
        MainInstruction = "Offline License Invalid",
        MainContent = "Offline EDGE^R license is invalid. Please do not edit your offline license file or modify the date on your computer. Please go online to restore your offline license. \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>"
      }.Show();
      return (Result) 1;
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.ExpiredEvaluation || edgeLicenseStatus1 == EdgeLicenseStatus.Expired)
    {
      string eulaGuid = Guid.NewGuid().ToString();
      if (!LicenseHelper.AcceptEULA(eulaGuid))
        return (Result) 1;
      TaskDialog taskDialog = new TaskDialog("EDGE^R - License Expired");
      taskDialog.Id = "ID_License_Expired";
      taskDialog.Title = "EDGE^R - License Expired";
      taskDialog.TitleAutoPrefix = false;
      taskDialog.AllowCancellation = true;
      taskDialog.MainInstruction = "License Expired";
      taskDialog.MainContent = "License is Expired. Unable to open EDGE^R. Would you like to contact EDGE^R Support to request a new license? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
      {
        if (!LicenseHelper.EnterNewLicense())
          return (Result) 1;
      }
      else
      {
        if (taskDialogResult == 1002)
        {
          LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
          return (Result) 1;
        }
        if (taskDialogResult != 1003)
          return (Result) 1;
        Process.Start("http://www.edge.ptac.com/licensing");
      }
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.Invalid)
    {
      string eulaGuid = Guid.NewGuid().ToString();
      if (!LicenseHelper.AcceptEULA(eulaGuid))
        return (Result) 1;
      TaskDialog taskDialog = new TaskDialog("EDGE^R - License Expired");
      taskDialog.Id = "ID_License_Expired";
      taskDialog.Title = "EDGE^R - Invalid License";
      taskDialog.TitleAutoPrefix = false;
      taskDialog.AllowCancellation = true;
      taskDialog.MainInstruction = "Invalid License";
      taskDialog.MainContent = "License is Invalid. Unable to open EDGE^R. Would you like to enter a valid license code or contact EDGE^R Support to request a new license? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
      taskDialog.DefaultButton = (TaskDialogResult) 2;
      TaskDialogResult taskDialogResult = taskDialog.Show();
      if (taskDialogResult == 1001)
      {
        if (!LicenseHelper.EnterNewLicense())
          return (Result) 1;
      }
      else
      {
        if (taskDialogResult == 1002)
        {
          LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
          return (Result) 1;
        }
        if (taskDialogResult != 1003)
          return (Result) 1;
        Process.Start("http://wwww.edge.ptac.com/licensing");
      }
    }
    if (edgeLicenseStatus1 == EdgeLicenseStatus.EULANotAccepted)
    {
      string eulaGuid = Guid.NewGuid().ToString();
      if (!LicenseHelper.AcceptEULA(eulaGuid))
        return (Result) 1;
      EdgeLicenseStatus edgeLicenseStatus2 = LicenseHelper.CloudValidationLicense();
      if (edgeLicenseStatus2 == EdgeLicenseStatus.NoLicense)
      {
        if (!LicenseHelper.AcceptEULA(eulaGuid))
          return (Result) 1;
        TaskDialog taskDialog = new TaskDialog("EDGE^R - No License Found");
        taskDialog.Id = "ID_License_NoLicense";
        taskDialog.Title = "EDGE^R - No License Found";
        taskDialog.TitleAutoPrefix = false;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "No License Found";
        taskDialog.MainContent = "No EDGE^R license found. Please enter your valid license code or contact support to request a license. (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          if (!LicenseHelper.EnterNewLicense())
            return (Result) 1;
        }
        else
        {
          if (taskDialogResult == 1002)
          {
            LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
            return (Result) 1;
          }
          if (taskDialogResult != 1003)
            return (Result) 1;
          Process.Start("http://www.edge.ptac.com/licensing");
        }
      }
      if (edgeLicenseStatus2 == EdgeLicenseStatus.Deactivated)
      {
        if (!LicenseHelper.AcceptEULA(eulaGuid))
          return (Result) 1;
        TaskDialog taskDialog = new TaskDialog("EDGE^R - License Deactivated");
        taskDialog.Id = "ID_License_Error";
        taskDialog.Title = "EDGE^R - License Deactivated";
        taskDialog.TitleAutoPrefix = false;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "License Deactivated";
        taskDialog.MainContent = "License is Deactivated. Unable to open EDGE^R. Would you like to contact EDGE^R Support to request a new license or enter a valid license code? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          if (!LicenseHelper.EnterNewLicense())
            return (Result) 1;
        }
        else
        {
          if (taskDialogResult == 1002)
          {
            LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
            return (Result) 1;
          }
          if (taskDialogResult != 1003)
            return (Result) 1;
          Process.Start("http://www.edge.ptac.com/licensing");
        }
      }
      if (edgeLicenseStatus2 == EdgeLicenseStatus.Error)
      {
        if (!LicenseHelper.AcceptEULA(eulaGuid))
          return (Result) 1;
        TaskDialog taskDialog = new TaskDialog("EDGE^R - License Error");
        taskDialog.Id = "ID_License_Error";
        taskDialog.Title = "EDGE^R - License Error";
        taskDialog.TitleAutoPrefix = false;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "License Error";
        taskDialog.MainContent = "Error with License. Unable to open EDGE^R. Would you like to contact EDGE^R Support to request a new license or enter a valid license code? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          if (!LicenseHelper.EnterNewLicense())
            return (Result) 1;
        }
        else
        {
          if (taskDialogResult == 1002)
          {
            LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
            return (Result) 1;
          }
          if (taskDialogResult != 1003)
            return (Result) 1;
          Process.Start("http://www.edge.ptac.com/licensing");
        }
      }
      if (edgeLicenseStatus2 == EdgeLicenseStatus.ExpiredEvaluation || edgeLicenseStatus2 == EdgeLicenseStatus.ExpiredOffline || edgeLicenseStatus2 == EdgeLicenseStatus.Expired)
      {
        if (!LicenseHelper.AcceptEULA(eulaGuid))
          return (Result) 1;
        TaskDialog taskDialog = new TaskDialog("EDGE^R - License Expired");
        taskDialog.Id = "ID_License_Expired";
        taskDialog.Title = "EDGE^R - License Expired";
        taskDialog.TitleAutoPrefix = false;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "License Expired";
        taskDialog.MainContent = "License is Expired. Unable to open EDGE^R. Would you like to contact EDGE^R Support to request a new license? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          if (!LicenseHelper.EnterNewLicense())
            return (Result) 1;
        }
        else
        {
          if (taskDialogResult == 1002)
          {
            LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
            return (Result) 1;
          }
          if (taskDialogResult != 1003)
            return (Result) 1;
          Process.Start("http://www.edge.ptac.com/licensing");
        }
      }
      if (edgeLicenseStatus2 == EdgeLicenseStatus.Invalid)
      {
        if (!LicenseHelper.AcceptEULA(eulaGuid))
          return (Result) 1;
        TaskDialog taskDialog = new TaskDialog("EDGE^R - License Expired");
        taskDialog.Id = "ID_License_Expired";
        taskDialog.Title = "EDGE^R - Invalid License";
        taskDialog.TitleAutoPrefix = false;
        taskDialog.AllowCancellation = true;
        taskDialog.MainInstruction = "Invalid License";
        taskDialog.MainContent = "License is Invalid. Unable to open EDGE^R. Would you like to enter a valid license code or contact EDGE^R Support to request a new license? (Choosing to request a new license will open your default email client) \n<a href=\"http://www.edge.ptac.com/licensing\" >Help</a>";
        taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enter License Code");
        taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
        taskDialog.CommonButtons = (TaskDialogCommonButtons) 8;
        taskDialog.DefaultButton = (TaskDialogResult) 2;
        TaskDialogResult taskDialogResult = taskDialog.Show();
        if (taskDialogResult == 1001)
        {
          if (!LicenseHelper.EnterNewLicense())
            return (Result) 1;
        }
        else
        {
          if (taskDialogResult == 1002)
          {
            LicenseHelper.RequestNewLicense(DateTime.Now.Date.ToString("MM/dd/yyyy"), eulaGuid);
            return (Result) 1;
          }
          if (taskDialogResult != 1003)
            return (Result) 1;
          Process.Start("http://wwww.edge.ptac.com/licensing");
        }
      }
    }
    App.JournalPlaybackDirectoryPath = new FileInfo(application.ControlledApplication.RecordingJournalFilename).Directory.FullName;
    QAUtils.IsJournalPlayback = new FileInfo(App.JournalPlaybackDirectoryPath + "\\regTest.lic").Exists;
    App.ReadPreferences();
    AppRibbonSetup.SetupRibbonCommands(application);
    App.RebarManager = new RebarSessionManager();
    App.TemplateHistoryManager = new TicketTemplatesHistoryManager();
    App.LockingManger = new ModelLockingManager();
    App.DialogSwitches = new AppSwitches();
    App.AVFViewTimers = new Queue<AVFViewTimer>();
    App.IsDebug = EdgeBuildInformation.IsDebugCheck;
    if (!application.ControlledApplication.VersionNumber.Equals("2024"))
    {
      new TaskDialog("Version Error")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainContent = $"This version of EDGE is designed to run on Revit version 2024 only.{Environment.NewLine}{Environment.NewLine}Please contact PTAC Consulting Engineers at 1 (251) 340-2473."
      }.Show();
      return (Result) -1;
    }
    RebarUpdater rebarUpdater = new RebarUpdater(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) rebarUpdater);
    ElementClassFilter filter1 = new ElementClassFilter(typeof (FamilyInstance));
    UpdaterRegistry.AddTrigger(rebarUpdater.GetUpdaterId(), (ElementFilter) filter1, Element.GetChangeTypeAny());
    AssemblySheetUpdater assemblySheetUpdater = new AssemblySheetUpdater(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) assemblySheetUpdater);
    ElementClassFilter filter2 = new ElementClassFilter(typeof (ViewSheet));
    UpdaterRegistry.AddTrigger(assemblySheetUpdater.GetUpdaterId(), (ElementFilter) filter2, Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(assemblySheetUpdater.GetUpdaterId(), (ElementFilter) filter2, Element.GetChangeTypeElementDeletion());
    RebarListUpdaterAddition listUpdaterAddition = new RebarListUpdaterAddition(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) listUpdaterAddition);
    UpdaterRegistry.AddTrigger(listUpdaterAddition.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (FamilyInstance)), Element.GetChangeTypeElementAddition());
    RebarListUpdaterDeletion listUpdaterDeletion = new RebarListUpdaterDeletion(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) listUpdaterDeletion);
    UpdaterRegistry.AddTrigger(listUpdaterDeletion.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (FamilyInstance)), Element.GetChangeTypeElementDeletion());
    RebarListUpdaterModify listUpdaterModify = new RebarListUpdaterModify(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) listUpdaterModify);
    UpdaterRegistry.AddTrigger(listUpdaterModify.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (FamilyInstance)), Element.GetChangeTypeAny());
    ModelLockingElementTrackingUpdater elementTrackingUpdater = new ModelLockingElementTrackingUpdater(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) elementTrackingUpdater);
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (FamilyInstance)), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (FamilyInstance)), Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (FamilyInstance)), Element.GetChangeTypeElementDeletion());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (DatumPlane)), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (DatumPlane)), Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (DatumPlane)), Element.GetChangeTypeElementDeletion());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (Wall)), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (Wall)), Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(elementTrackingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (Wall)), Element.GetChangeTypeElementDeletion());
    ModelLockingUpdater modelLockingUpdater = new ModelLockingUpdater(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) modelLockingUpdater);
    ElementFilter filter3 = (ElementFilter) new ElementMulticlassFilter((IList<Type>) new List<Type>()
    {
      typeof (FamilyInstance),
      typeof (AssemblyInstance),
      typeof (AssemblyType)
    });
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), filter3, Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), filter3, Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), filter3, Element.GetChangeTypeElementDeletion());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementCategoryFilter(BuiltInCategory.OST_AssemblyOrigin), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (Wall)), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (Wall)), Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (Wall)), Element.GetChangeTypeElementDeletion());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementCategoryFilter(BuiltInCategory.OST_IOSModelGroups), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementCategoryFilter(BuiltInCategory.OST_IOSModelGroups), Element.GetChangeTypeElementAddition());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementCategoryFilter(BuiltInCategory.OST_IOSModelGroups), Element.GetChangeTypeElementDeletion());
    UpdaterRegistry.AddTrigger(modelLockingUpdater.GetUpdaterId(), (ElementFilter) new ElementClassFilter(typeof (ProjectInfo)), Element.GetChangeTypeAny());
    EntourageWarning entourageWarning = new EntourageWarning(application.ActiveAddInId);
    UpdaterRegistry.RegisterUpdater((IUpdater) entourageWarning);
    UpdaterRegistry.AddTrigger(entourageWarning.GetUpdaterId(), (ElementFilter) new ElementCategoryFilter(BuiltInCategory.OST_Entourage), Element.GetChangeTypeAny());
    UpdaterRegistry.AddTrigger(entourageWarning.GetUpdaterId(), (ElementFilter) new ElementCategoryFilter(BuiltInCategory.OST_Entourage), Element.GetChangeTypeElementAddition());
    this.OverrideRevitCommand(application, App.s_commandToDisable_RL, new EventHandler<ExecutedEventArgs>(this.OnReloadLatestCommandExecuted));
    application.ControlledApplication.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(this.OnDocumentCreated);
    application.ControlledApplication.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(this.OnDocumentOpened);
    application.ControlledApplication.DocumentOpening += new EventHandler<DocumentOpeningEventArgs>(this.OnDocumentOpening);
    application.ControlledApplication.DocumentClosing += new EventHandler<DocumentClosingEventArgs>(this.OnDocumentClosed);
    application.ControlledApplication.DocumentSaved += new EventHandler<DocumentSavedEventArgs>(this.OnDocumentSaved);
    application.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(this.OnDocumentChanged);
    application.ControlledApplication.DocumentSavedAs += new EventHandler<DocumentSavedAsEventArgs>(this.OnDocumentSavedAs);
    application.ControlledApplication.DocumentSynchronizingWithCentral += new EventHandler<DocumentSynchronizingWithCentralEventArgs>(this.OnDocumentSynchronizingWithCentral);
    application.ControlledApplication.DocumentSynchronizedWithCentral += new EventHandler<DocumentSynchronizedWithCentralEventArgs>(this.OnDocumentSynchronizedWithCentral);
    application.ControlledApplication.DocumentSaving += new EventHandler<DocumentSavingEventArgs>(this.OnDocumentSaving);
    application.ControlledApplication.DocumentSavingAs += new EventHandler<DocumentSavingAsEventArgs>(this.OnDocumentSavingAs);
    application.Idling += new EventHandler<IdlingEventArgs>(this.OnIdling);
    application.ViewActivated += new EventHandler<ViewActivatedEventArgs>(this.OnViewActivated);
    if (App.DockablePane)
      new PaneWindowProvider().Register(application);
    return (Result) 0;
  }

  private void ControlledApplication_DockableFrameVisibilityChanged(
    object sender,
    DockableFrameVisibilityChangedEventArgs e)
  {
    if (!DockablePaneId.op_Equality(new DockablePaneId(((GuidEnum) e.PaneId).Guid), App.paneId))
      return;
    if (!e.DockableFrameShown)
      App.DockablePane = false;
    else
      App.DockablePane = true;
  }

  private void OnDocumentOpening(object sender, DocumentOpeningEventArgs e)
  {
    App.DialogSwitches.SuspendModelLockingforOperation = true;
  }

  private void OnCreateAssemblyViews(object sender, ExecutedEventArgs e)
  {
    string functionOrFile = "App.OnCreateAssemblyViews";
    try
    {
      if (sender is UIApplication uiApplication)
      {
        Document revitDoc = uiApplication.ActiveUIDocument.Document;
        IEnumerable<Element> source = uiApplication.ActiveUIDocument.Selection.GetElementIds().Select<ElementId, Element>((Func<ElementId, Element>) (s => revitDoc.GetElement(s))).Where<Element>((Func<Element, bool>) (s => s is AssemblyInstance || s is AssemblyType));
        ElementId selectedAssemblyId = ElementId.InvalidElementId;
        if (source.Any<Element>())
        {
          Element selectedElem = source.First<Element>();
          if (selectedElem is AssemblyInstance)
            selectedAssemblyId = selectedElem.Id;
          else if (selectedElem is AssemblyType)
          {
            Element element = new FilteredElementCollector(revitDoc).OfClass(typeof (AssemblyInstance)).Where<Element>((Func<Element, bool>) (elem => elem.GetTypeId().IntegerValue == selectedElem.Id.IntegerValue)).FirstOrDefault<Element>();
            if (element != null)
              selectedAssemblyId = element.Id;
          }
        }
        if (selectedAssemblyId == (ElementId) null || selectedAssemblyId.Equals((object) ElementId.InvalidElementId))
        {
          try
          {
            TaskDialog.Show("Select Assembly", "Please select the assembly for which you would like to create assembly views.");
            IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
            SelectAssemblyWindow selectAssemblyWindow = new SelectAssemblyWindow(uiApplication.ActiveUIDocument, mainWindowHandle);
            selectAssemblyWindow.ShowDialog();
            if (selectAssemblyWindow.IsCancelled)
              return;
            selectedAssemblyId = selectAssemblyWindow.selectedAssembly.Id;
          }
          catch (Exception ex)
          {
            QA.LogError(functionOrFile + ":SelectionError", "Exception thrown in selection attempt: " + ex.Message);
            TaskDialog.Show("ERROR", "Unexpected error in Revit selection operation.  Check log for more details");
            return;
          }
        }
        Dictionary<string, int> scalesDictionary = ScalesManager.GetScalesDictionary(ScalesManager.GetScaleUnitsForDocument(revitDoc));
        CreateAssemblyViews_Form assemblyViewsForm = new CreateAssemblyViews_Form(uiApplication.ActiveUIDocument, selectedAssemblyId, scalesDictionary);
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        if (assemblyViewsForm.ShowDialog() == DialogResult.Cancel && assemblyViewsForm.bLoadFamilySelected)
        {
          RevitCommandId revitCommandId = RevitCommandId.LookupCommandId("ID_FAMILY_LOAD");
          if (revitCommandId != null && uiApplication.CanPostCommand(revitCommandId))
            uiApplication.PostCommand(revitCommandId);
        }
        QA.LogError(functionOrFile, "No Selected Assemblies in Create Assembly Views Command");
      }
      else
        QA.LogError(functionOrFile, "Create Views UIApplication was Null");
    }
    catch (Exception ex)
    {
      QA.LogError(functionOrFile, $"Exception thrown in Assembly View Creation: {Environment.NewLine}{ex.Message}");
    }
    finally
    {
      App.DialogSwitches.SuspendModelLockingforOperation = false;
    }
  }

  private Result OverrideRevitCommand(
    UIControlledApplication application,
    string commandToDisable,
    EventHandler<ExecutedEventArgs> commandHandler)
  {
    RevitCommandId revitCommandId = RevitCommandId.LookupCommandId(commandToDisable);
    if (!revitCommandId.CanHaveBinding)
    {
      App.ShowDialog("Error", $"The target command {commandToDisable} selected for disabling cannot be overridden.  Please contact support.");
      QA.LogError(nameof (OverrideRevitCommand), $"The target command {commandToDisable} selected for disabling cannot be overridden");
      return (Result) -1;
    }
    try
    {
      application.CreateAddInCommandBinding(revitCommandId).Executed += commandHandler;
      QA.LogLine("  --Revit Command succesfully overridden: " + commandToDisable);
      return (Result) 0;
    }
    catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
    {
      App.ShowDialog("Error", $"EDGE^R is unable to replace the command {commandToDisable}; another add-in has already overridden this command.  If you use Rebar Control Mark Automation or Model Locking in a workshared environment, you will run the risk of Rebar Control Marks not being properly updated.");
      return (Result) -1;
    }
    catch (Exception ex)
    {
      App.ShowDialog("Error", $"EDGE^R is unable to disable the target command {commandToDisable}; this is a general exception.  Please contact support.  Message: {ex.Message}");
      return (Result) -1;
    }
  }

  public Result OnShutdown(UIControlledApplication application)
  {
    this.DeleteAllTemporaryFamilies();
    if (QA.RebarMarkAutomation_Worksharing_Enabled)
    {
      RevitCommandId revitCommandId1 = RevitCommandId.LookupCommandId(App.s_commandToDisable_RL);
      if (revitCommandId1.HasBinding)
        application.RemoveAddInCommandBinding(revitCommandId1);
      RevitCommandId revitCommandId2 = RevitCommandId.LookupCommandId(App.s_commandToDisable_SWCQuick);
      if (revitCommandId2.HasBinding)
        application.RemoveAddInCommandBinding(revitCommandId2);
      RevitCommandId revitCommandId3 = RevitCommandId.LookupCommandId(App.s_commandToDisable_SWCModify);
      if (revitCommandId3.HasBinding)
        application.RemoveAddInCommandBinding(revitCommandId3);
    }
    RevitCommandId revitCommandId = RevitCommandId.LookupCommandId(App.s_commandToDisable_CreateAssemblyViews);
    if (revitCommandId.HasBinding)
      application.RemoveAddInCommandBinding(revitCommandId);
    application.ControlledApplication.DocumentCreated -= new EventHandler<DocumentCreatedEventArgs>(this.OnDocumentCreated);
    application.ControlledApplication.DocumentOpened -= new EventHandler<DocumentOpenedEventArgs>(this.OnDocumentOpened);
    application.ControlledApplication.DocumentClosing -= new EventHandler<DocumentClosingEventArgs>(this.OnDocumentClosed);
    application.ControlledApplication.DocumentSaved -= new EventHandler<DocumentSavedEventArgs>(this.OnDocumentSaved);
    application.ControlledApplication.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(this.OnDocumentChanged);
    application.ControlledApplication.DocumentSavedAs -= new EventHandler<DocumentSavedAsEventArgs>(this.OnDocumentSavedAs);
    application.ControlledApplication.DocumentSynchronizingWithCentral -= new EventHandler<DocumentSynchronizingWithCentralEventArgs>(this.OnDocumentSynchronizingWithCentral);
    application.ControlledApplication.DocumentSynchronizedWithCentral -= new EventHandler<DocumentSynchronizedWithCentralEventArgs>(this.OnDocumentSynchronizedWithCentral);
    application.ControlledApplication.DocumentSaving -= new EventHandler<DocumentSavingEventArgs>(this.OnDocumentSaving);
    application.ControlledApplication.DocumentSavingAs -= new EventHandler<DocumentSavingAsEventArgs>(this.OnDocumentSavingAs);
    application.Idling -= new EventHandler<IdlingEventArgs>(this.OnIdling);
    return (Result) 0;
  }

  public void DeleteAllTemporaryFamilies()
  {
    if (!Directory.Exists("C:\\EDGEforRevit\\FamiliesTemp\\"))
      return;
    foreach (string file in Directory.GetFiles("C:\\EDGEforREvit\\FamiliesTemp\\"))
      File.Delete(file);
  }

  public void OnDocumentChanged(object sender, DocumentChangedEventArgs args)
  {
    try
    {
      if (App.checkSheetAndAssemblyIds)
      {
        List<ViewSheet> list = new FilteredElementCollector(args.GetDocument()).OfCategory(BuiltInCategory.OST_Sheets).OfClass(typeof (ViewSheet)).ToElements().Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (p => p.AssociatedAssemblyInstanceId.ToString() != "-1")).ToList<ViewSheet>();
        if (list.Count != App.sheetAndAsssemblyIds.Count)
        {
          foreach (ViewSheet viewSheet in list)
          {
            if (!App.sheetAndAsssemblyIds.ContainsKey(viewSheet.Id.ToString()) && viewSheet.AssociatedAssemblyInstanceId.ToString() != "-1")
              App.sheetAndAsssemblyIds.Add(viewSheet.Id.ToString(), viewSheet.AssociatedAssemblyInstanceId.ToString());
          }
        }
      }
      else
        App.checkSheetAndAssemblyIds = true;
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.Message);
    }
    try
    {
      Document document = args.GetDocument();
      ActiveModel.Document = document;
      IEnumerable<ElementId> deletedElementIds = (IEnumerable<ElementId>) args.GetDeletedElementIds();
      IEnumerable<ElementId> addedElementIds = (IEnumerable<ElementId>) args.GetAddedElementIds();
      IEnumerable<ElementId> modifiedElementIds = (IEnumerable<ElementId>) args.GetModifiedElementIds();
      if (ModelLockingUtils.ModelLockingEnabled(document))
      {
        if (!App.ReloadingLatest)
        {
          App.LockingManger.ProcessAddedIds(document, addedElementIds);
          App.LockingManger.ProcessModifiedIds(document, modifiedElementIds);
          App.LockingManger.HandleElementDeletion(document, deletedElementIds);
        }
      }
    }
    catch (Exception ex)
    {
      TaskDialog.Show(ex.Message, ex.StackTrace);
    }
    try
    {
      if (!App.RebarManager.IsAutomatedDocument(args.GetDocument().PathName) || args.Operation != UndoOperation.TransactionUndone && args.Operation != UndoOperation.TransactionRedone)
        return;
      QA.LogDemarcation("Undo / Redo: " + args.Operation.ToString());
      Document document = args.GetDocument();
      if (!App.RebarManager.IsAutomatedDocument(document.PathName))
        return;
      RebarControlMarkManager3 manager = App.RebarManager.Manager(document.PathName);
      IList<ElementId> list = (IList<ElementId>) args.GetDeletedElementIds().ToList<ElementId>();
      ICollection<ElementId> addedElementIds = args.GetAddedElementIds();
      IEnumerable<ElementId> modifiedElementIds = (IEnumerable<ElementId>) args.GetModifiedElementIds();
      Func<ElementId, Element> selector = new Func<ElementId, Element>(document.GetElement);
      this.catalogUndoneElements(addedElementIds.Select<ElementId, Element>(selector).Where<Element>((Func<Element, bool>) (elem => !elem.HasSuperComponent())).Where<Element>((Func<Element, bool>) (elem => Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("REBAR"))).Where<Element>((Func<Element, bool>) (elem => Parameters.LookupParameter(elem, "BAR_SHAPE") != null)).ToList<Element>(), manager);
      foreach (Element element in modifiedElementIds.Select<ElementId, Element>(new Func<ElementId, Element>(document.GetElement)).Where<Element>((Func<Element, bool>) (elem => Parameters.GetParameterAsString(elem, "MANUFACTURE_COMPONENT").Contains("REBAR"))).Where<Element>((Func<Element, bool>) (elem => Parameters.LookupParameter(elem, "BAR_SHAPE") != null)).ToList<Element>())
      {
        Parameter parameter1 = element.LookupParameter("CONTROL_MARK");
        Parameter parameter2 = Parameters.LookupParameter(element, "BAR_DIAMETER");
        string parameterAsString = Parameters.GetParameterAsString(element, "BAR_SHAPE");
        if (parameter1 != null && parameter2 != null)
        {
          double num = parameter2.AsDouble();
          if (BarHasher.useMetricEuro)
            num = UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Millimeters);
          if (!BarDiameterOracle.IsValidDiameter(num))
          {
            manager.HandleBarDeletion(element.Id);
          }
          else
          {
            int ibarDiam = BarDiameterOracle.ResolveBarDiam(num);
            string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(num, manager.mfgSettingsData.bUseMetricCanadian, manager.mfgSettingsData.useZeroPaddingForBarSize);
            string str = parameter1.AsString();
            manager.HandleBarDeletion(element.Id);
            manager.UpdateGuidToElementIdMap(element.UniqueId, element.Id, str);
            if (element is FamilyInstance)
            {
              Rebar bar = new Rebar(element as FamilyInstance, manager.mfgSettingsData);
              manager.UpdateGuidToBarInfoMap(bar);
              if (!parameterAsString.ToUpper().Contains("STRAIGHT"))
              {
                manager.PushToProjectMarkList(str, element.UniqueId, ibarDiam, strForRealDiameter, bar);
                manager.HandleUndoRedoModify(bar);
              }
            }
          }
        }
      }
      foreach (ElementId deletedElementId in (IEnumerable<ElementId>) list)
        App.RebarManager.Manager(document.PathName).HandleBarDeletion(deletedElementId);
    }
    catch (Exception ex)
    {
      QA.InHouseMessage("Error thrown in OnDocumentChanged() message: " + ex.Message);
      throw;
    }
  }

  private static void TraceDocChangedInfo(
    Document revitDoc,
    IEnumerable<ElementId> DeletedIds,
    IEnumerable<ElementId> AddedIds,
    IEnumerable<ElementId> ModifiedIds)
  {
    foreach (ElementId addedId in AddedIds)
      revitDoc.GetElement(addedId);
    foreach (ElementId modifiedId in ModifiedIds)
    {
      try
      {
        revitDoc.GetElement(modifiedId);
      }
      catch (Exception ex)
      {
      }
    }
  }

  private static void TraceDocChangedModelLockingInfo(
    Document revitDoc,
    IEnumerable<ElementId> DeletedIds,
    IEnumerable<ElementId> AddedIds,
    IEnumerable<ElementId> ModifiedIds)
  {
    QA.LogLine("///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////");
    QA.LogLine("              DeletedIDs: " + DeletedIds.Count<ElementId>().ToString());
    foreach (object deletedId in DeletedIds)
      QA.LogLine(deletedId.ToString());
    QA.LogLine("              AddedIDs: " + AddedIds.Count<ElementId>().ToString());
    foreach (ElementId addedId in AddedIds)
    {
      Element element = revitDoc.GetElement(addedId);
      try
      {
        QA.LogLine($"{addedId.ToString()} {(element == null ? "null" : (element.Category != null ? element.Category.Name : "null"))} {(element == null ? "null" : element.Name)} {(element == null ? "null" : element.UniqueId)}");
      }
      catch (Exception ex)
      {
        QA.LogLine($"threw on attempt to get element: {addedId.ToString()} from document.  This is an added element from Undo");
      }
    }
    QA.LogLine("              ModifiedIDs: " + ModifiedIds.Count<ElementId>().ToString());
    foreach (ElementId modifiedId in ModifiedIds)
    {
      try
      {
        Element element = revitDoc.GetElement(modifiedId);
        QA.LogLine($"{modifiedId.ToString()} {(element == null ? "null" : (element.Category != null ? element.Category.Name : "null"))} {(element == null ? "null" : element.Name)} {(element == null ? "null" : element.UniqueId)}");
      }
      catch (Exception ex)
      {
        QA.LogLine($"threw on attempt to get element: {modifiedId.ToString()} from document.  This is a modified element from Undo");
      }
    }
    QA.LogLine("///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////");
  }

  public void OnIdling(object sender, IdlingEventArgs args)
  {
    if (App.AVFViewTimers.Count == 0)
      return;
    foreach (AVFViewTimer avfViewTimer in App.AVFViewTimers)
      ++avfViewTimer.timerCounter;
    if (App.AVFViewTimers.Peek().timerCounter <= 50)
      return;
    ExternalEvent.Create((IExternalEventHandler) new App.ClearAVFResultsFromView()).Raise();
  }

  public void OnDocumentCreated(object sender, DocumentCreatedEventArgs args)
  {
    QATimer qaTimer = new QATimer("OnDocumentOpened");
    UIControlledApplication appForPane = App.appForPane;
    App.currentDocument = args.Document;
    ActiveModel.GetInformation(args.Document);
    if (!App.DockablePane)
      return;
    Autodesk.Revit.UI.DockablePane dockablePane = appForPane.GetDockablePane(App.paneId);
    if (dockablePane == null || dockablePane.IsShown())
      return;
    dockablePane.Show();
  }

  public void OnDocumentOpened(object sender, DocumentOpenedEventArgs args)
  {
    QATimer qaTimer1 = new QATimer(nameof (OnDocumentOpened));
    UIControlledApplication appForPane = App.appForPane;
    App.currentDocument = args.Document;
    Document document = args.Document;
    ActiveModel.GetInformation(document);
    if (App.DockablePane)
    {
      Autodesk.Revit.UI.DockablePane dockablePane = appForPane.GetDockablePane(App.paneId);
      if (dockablePane != null && !dockablePane.IsShown())
        dockablePane.Show();
    }
    if (document.IsFamilyDocument)
    {
      App.DialogSwitches.SuspendModelLockingforOperation = false;
    }
    else
    {
      QATimer qaTimer2 = new QATimer("Count Multiplier Initialization");
      if (App.ProcessCountMultiplierOnDocumentOpen)
      {
        using (Transaction transaction = new Transaction(document, "Initialize Count Multiplier"))
        {
          try
          {
            if (transaction.Start() == TransactionStatus.Started)
            {
              ProjectSharedParameters.InitializeCountMultiplierToOneForAllEdgeElements(document);
              if (transaction.Commit() != TransactionStatus.Committed)
              {
                QAUtils.LogLine("  ERROR:  Unable to commit count multiplier initialization transaction.  Rolling back initialization transaction.");
                if (transaction.HasStarted())
                {
                  int num = (int) transaction.RollBack();
                }
              }
            }
            else
            {
              TaskDialog.Show("EDGE Error", "Unable to start transaction to initialize count multiplier.  Item counts will not be correct until BOM Product Hosting is run for the entire model.");
              QAUtils.LogLine("Unable to start transaction to initialize count multiplier.  Item counts will not be correct until BOM Product Hosting is run for the entire model.");
            }
          }
          catch (Exception ex)
          {
            TaskDialog.Show("EDGE Error", "Count multiplier could not be initialized. This is probably due to the COUNT_MULTIPLIER parameter being read-only for some or all elements in the model.");
            QAUtils.LogError("OnDocumentOpen", "Exception thrown: " + ex.Message);
            if (transaction.HasStarted())
            {
              int num = (int) transaction.RollBack();
            }
            App.DialogSwitches.SuspendModelLockingforOperation = false;
          }
        }
      }
      qaTimer2.Stop();
      QATimer qaTimer3 = new QATimer("Track document for model locking");
      try
      {
        string path = App.MLFolderPath + "\\ModelLockingAdminRoster.txt";
        if (File.Exists(path))
        {
          foreach (string readAllLine in File.ReadAllLines(path))
          {
            if (!App.ModelLockingAdminList.Contains(readAllLine.ToUpper()))
              App.ModelLockingAdminList.Add(readAllLine.ToUpper());
          }
        }
        if (ModelLockingUtils.ModelLockingEnabled(args.Document))
          App.LockingManger.TrackOpenedDocument(document);
      }
      catch (Exception ex)
      {
        if (ModelLockingUtils.ModelLockingEnabled(document))
          TaskDialog.Show("Edge Error", "Track opened document threw: " + ex.Message);
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
      qaTimer3.Stop();
      if (App.RebarMarkAutomation)
      {
        try
        {
          int parameterAsInt = Parameters.GetParameterAsInt((Element) document.ProjectInformation, "REBAR_UPDATER");
          if (parameterAsInt == -1)
          {
            ProjectParameters.AddRebarUpdaterToProjectInfo(document);
            TaskDialog taskDialog = new TaskDialog("EDGE^R Rebar Control");
            taskDialog.MainInstruction = "Enable EDGE^R Control Mark Automation?";
            taskDialog.MainContent = "This Project has not been setup for Rebar Control Mark Automation.  Do you want to enable Control Mark automation now?  Your choice will be reflected in the Yes/No Project Information parameter REBAR_UPDATER.  To enable/disable at a later time, change this setting and reopen the project.";
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Enable Now", "Enable Rebar Control Mark Automation for this document.");
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Disable For Now", "Disable Rebar Control Mark Automation for this document.");
            if (!string.IsNullOrWhiteSpace(document.PathName))
            {
              if (taskDialog.Show() == 1001)
              {
                ProjectParameters.SetRebarUpdaterParameter(document, true);
                App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, true);
              }
              else
              {
                ProjectParameters.SetRebarUpdaterParameter(document, false);
                App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, false);
              }
            }
            else if (taskDialog.Show() == 1001)
            {
              ProjectParameters.SetRebarUpdaterParameter(document, true);
              TaskDialog.Show("EDGE Information", "Rebar Mark Automation has been enabled for this document but it must be saved and re-opened before you can use mark automation.");
            }
            else
              ProjectParameters.SetRebarUpdaterParameter(document, false);
          }
          else
            App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, parameterAsInt == 1);
        }
        catch (Exception ex)
        {
          TaskDialog.Show("EDGE REBAR ERROR", ex.Message + Environment.NewLine + ex.StackTrace);
        }
      }
      if (App.RebarMarkAutomation)
      {
        if (App.RebarManager.IsAutomatedDocument(document.PathName))
        {
          try
          {
            Parameter parameter1 = document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
            if (parameter1 != null && !string.IsNullOrEmpty(parameter1.AsString()))
            {
              string manufactuerName = parameter1.AsString();
              QATimer qaTimer4 = new QATimer("Add New Rebar Tracker");
              AddDocTrackerResult docTrackerResult = App.RebarManager.AddNewDocumentRebarTracker(document, manufactuerName);
              qaTimer4.Stop();
              switch (docTrackerResult)
              {
                case AddDocTrackerResult.OK:
                  QA.ToggleRebarDebug();
                  RebarControlMarkManager3 manager = App.RebarManager.Manager(document.PathName);
                  QATimer qaTimer5 = new QATimer("build Initial list of Rebar");
                  List<Rebar> list = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof (FamilyInstance)).ToElements().Cast<FamilyInstance>().Where<FamilyInstance>((Func<FamilyInstance, bool>) (s => s.LookupParameter("CONTROL_MARK") != null && Parameters.LookupParameter((Element) s, "BAR_SHAPE") != null)).Where<FamilyInstance>((Func<FamilyInstance, bool>) (famInst => famInst.SuperComponent == null)).Select<FamilyInstance, Rebar>((Func<FamilyInstance, Rebar>) (s => new Rebar(s, manager.mfgSettingsData._bucketKeying))).ToList<Rebar>();
                  qaTimer5.Stop();
                  StringBuilder stringBuilder = new StringBuilder();
                  stringBuilder.AppendLine("Catalog Existing Bars:");
                  bool flag1 = false;
                  bool flag2 = true;
                  string errorMsg = "";
                  List<Rebar> rebarList1 = new List<Rebar>();
                  List<Rebar> source = new List<Rebar>();
                  List<Rebar> rebarList2 = new List<Rebar>();
                  QATimer qaTimer6 = new QATimer("Bucketize Bars");
                  foreach (Rebar bar in list)
                  {
                    if (bar.BarShape.Contains("STRAIGHT"))
                      rebarList1.Add(bar);
                    else if (manager.IsConformingMark(bar, out errorMsg))
                      rebarList2.Add(bar);
                    else
                      source.Add(bar);
                  }
                  qaTimer6.Stop();
                  if (source.Count > 0)
                  {
                    Rebar bar = source.First<Rebar>();
                    string strippedMark = "";
                    string str = !manager.MarkIsStandardBarMark(bar.BarMark, out strippedMark) ? manager.mfgSettingsData.GetSampleMark(source.First<Rebar>()) : manager.AddFinishMarks(bar, strippedMark);
                    TaskDialog taskDialog = new TaskDialog("Edge Warning");
                    if (ModelLockingUtils.ModelLockingEnabled(document) && !App.LockingManger.UserIsAllowedForCategory(document, ModelPermissionsCategory.RebarHandling, bOverride: true))
                    {
                      taskDialog.MainInstruction = source.Count<Rebar>().ToString() + " non-conforming marks found in project.";
                      taskDialog.MainContent = $"Based on current rebar settings for {manager.mfgSettingsData.MfgName} non-conforming bars have been found in the model.  For example: expected bar mark <{bar.BarMark}> to look more like <{str}> based on the current rebar settings file.  This may be expected if you have purposefully changed the manufacturer settings and intend to update all control marks in the project to match new settings.  See expanded content for a detailed analysis of one bar mark";
                      taskDialog.ExpandedContent = errorMsg;
                      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Disable Bar Mark Automation", "Your model locking permissions do not include Rebar/Handling, therefore you cannot resolve these rebar mark issues. Automatic rebar marking will be disabled.");
                      taskDialog.AllowCancellation = false;
                    }
                    else
                    {
                      taskDialog.MainInstruction = source.Count<Rebar>().ToString() + " non-conforming marks found in project.  Continue?";
                      taskDialog.MainContent = $"Based on current rebar settings for {manager.mfgSettingsData.MfgName} non-conforming bars have been found in the model.  For example: expected bar mark <{bar.BarMark}> to look more like <{str}> based on the current rebar settings file.  This may be expected if you have purposefully changed the manufacturer settings and intend to update all control marks in the project to match new settings.  See expanded content for a detailed analysis of one bar mark";
                      taskDialog.ExpandedContent = errorMsg;
                      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Continue Processing", "Rebar Mark Automation will be left on and you can deal with each non-conforming bar.");
                      taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Disable Bar Mark Automation", "Temporarily disable bar mark automation for this document.  Marks will be static and will not respond to any changes in the model.");
                      taskDialog.AllowCancellation = false;
                    }
                    if (taskDialog.Show() == 1001)
                    {
                      flag2 = true;
                    }
                    else
                    {
                      flag2 = false;
                      App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, false);
                    }
                  }
                  if (flag2)
                  {
                    QATimer qaTimer7 = new QATimer("Overall Rebar Processing");
                    CatalogAction action = CatalogAction.NoActionSelected;
                    TransactionGroup transactionGroup = new TransactionGroup(document, "Build Rebar Automation Dictionary");
                    if (transactionGroup.Start() == TransactionStatus.Started)
                    {
                      IEnumerable<Rebar> rebars1 = (IEnumerable<Rebar>) rebarList1;
                      Transaction transaction1 = new Transaction(document, "Process straight bar marks");
                      if (transaction1.Start() == TransactionStatus.Started)
                      {
                        QATimer qaTimer8 = new QATimer("Process Straight Bars");
                        foreach (Rebar rebar in rebars1)
                        {
                          Element familyInstance = (Element) rebar.FamilyInstance;
                          Parameter parameter2 = familyInstance.LookupParameter("CONTROL_MARK");
                          Parameter parameter3 = familyInstance.LookupParameter("IDENTITY_DESCRIPTION_SHORT");
                          Parameter parameter4 = familyInstance.LookupParameter("IDENTITY_DESCRIPTION");
                          if (parameter2 != null && !parameter2.IsReadOnly)
                          {
                            string straightBarMark = App.RebarManager.Manager(document.PathName).GetStraightBarMark(rebar);
                            App.RebarManager.Manager(document.PathName).UpdateGuidToElementIdMap(familyInstance.UniqueId, familyInstance.Id, straightBarMark);
                            App.RebarManager.Manager(document.PathName).UpdateGuidToBarInfoMap(rebar);
                            parameter2.AsString();
                            string str = parameter4 != null ? parameter4.AsString() : "";
                            if (straightBarMark != parameter2.AsString() || str != rebar.IdentityDescriptionLong)
                            {
                              parameter2.Set(straightBarMark);
                              if (parameter3 != null && !parameter3.IsReadOnly)
                                parameter3.Set(straightBarMark);
                              if (parameter4 != null && !parameter4.IsReadOnly)
                                parameter4.Set(rebar.IdentityDescriptionLong);
                            }
                          }
                        }
                        if (transaction1.Commit() != TransactionStatus.Committed)
                          QA.InHouseMessage("update transaction failed in Document Opening Build Rebar Dictionary for straight Bars:");
                        qaTimer8.Stop();
                      }
                      IEnumerable<Rebar> rebars2 = (IEnumerable<Rebar>) rebarList2;
                      QATimer qaTimer9 = new QATimer("Process Conforming Bars");
                      Transaction transaction2 = new Transaction(document, "Process conforming bar marks");
                      if (transaction2.Start() == TransactionStatus.Started)
                      {
                        action = CatalogAction.NoActionSelected;
                        foreach (Rebar bar in rebars2)
                        {
                          string ErrorMessage = "";
                          string newMark = "";
                          if (!App.RebarManager.Manager(document.PathName).CatalogExistingBar(bar, ref action, out newMark, out ErrorMessage))
                          {
                            flag1 = true;
                            stringBuilder.AppendLine(ErrorMessage);
                            if (action.HasFlag((Enum) CatalogAction.DisableBarMarkAutomation))
                            {
                              flag2 = false;
                              App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, false);
                              break;
                            }
                          }
                          if (action.HasFlag((Enum) CatalogAction.ReplaceWithDictionaryMark) || action.HasFlag((Enum) CatalogAction.ReplaceWithDictionaryMarkForALL) || action.HasFlag((Enum) CatalogAction.UpdateWithNewMarkNumber))
                          {
                            Element element = document.GetElement(bar.Id);
                            Parameter parameter5 = element.LookupParameter("CONTROL_MARK");
                            Parameter parameter6 = element.LookupParameter("IDENTITY_DESCRIPTION_SHORT");
                            Parameter parameter7 = element.LookupParameter("IDENTITY_DESCRIPTION");
                            if (parameter5 != null && !parameter5.IsReadOnly)
                            {
                              parameter5.AsString();
                              string str = parameter7 != null ? parameter7.AsString() : "";
                              if (newMark != parameter5.AsString() || str != bar.IdentityDescriptionLong)
                              {
                                parameter5.Set(newMark);
                                if (parameter6 != null && !parameter6.IsReadOnly)
                                  parameter6.Set(newMark);
                                if (parameter7 != null && !parameter7.IsReadOnly)
                                  parameter7.Set(bar.IdentityDescriptionLong);
                              }
                            }
                          }
                          action &= ~CatalogAction.UpdateWithNewMarkNumber;
                        }
                        if (!flag2)
                        {
                          int num = (int) transaction2.RollBack();
                        }
                        else if (transaction2.Commit() != TransactionStatus.Committed)
                        {
                          flag2 = false;
                          TaskDialog taskDialog = new TaskDialog("EDGE Error");
                          taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                          taskDialog.AllowCancellation = false;
                          taskDialog.MainInstruction = "Failed To Commit Catalog Conforming Marks Transaction";
                          taskDialog.MainContent = "There has been an error in committing this transaction.  Rebar bar mark automation must be disabled until the error can be resolved.  Please contact support for assistance resolving this error.";
                          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Disable Bar Mark Automation", "Bar mark automation will be disabled for this document.");
                          taskDialog.Show();
                        }
                        qaTimer9.Stop();
                      }
                      QATimer qaTimer10 = new QATimer("Process NonConforming Bars");
                      Transaction transaction3 = new Transaction(document, "Process conforming bar marks");
                      if (flag2 && transaction3.Start() == TransactionStatus.Started)
                      {
                        foreach (Rebar bar in source)
                        {
                          string ErrorMessage = "";
                          string newMark = "";
                          if (!App.RebarManager.Manager(document.PathName).CatalogExistingBar(bar, ref action, out newMark, out ErrorMessage))
                          {
                            flag1 = true;
                            stringBuilder.AppendLine(ErrorMessage);
                            if (action.HasFlag((Enum) CatalogAction.DisableBarMarkAutomation))
                            {
                              flag2 = false;
                              App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, false);
                              break;
                            }
                          }
                          if ((action & CatalogAction.CreateConformingMark) == CatalogAction.CreateConformingMark || (action & CatalogAction.CreateConformingMarkForALL) == CatalogAction.CreateConformingMarkForALL || (action & CatalogAction.ReplaceWithDictionaryMark) == CatalogAction.ReplaceWithDictionaryMark || (action & CatalogAction.ReplaceWithDictionaryMarkForALL) == CatalogAction.ReplaceWithDictionaryMarkForALL || action.HasFlag((Enum) CatalogAction.UpdateWithNewMarkNumber))
                          {
                            Element element = document.GetElement(bar.Id);
                            Parameter parameter8 = element.LookupParameter("CONTROL_MARK");
                            Parameter parameter9 = element.LookupParameter("IDENTITY_DESCRIPTION_SHORT");
                            Parameter parameter10 = element.LookupParameter("IDENTITY_DESCRIPTION");
                            if (parameter8 != null && !parameter8.IsReadOnly)
                            {
                              parameter8.AsString();
                              string str = parameter10 != null ? parameter10.AsString() : "";
                              if (newMark != parameter8.AsString() || str != bar.IdentityDescriptionLong)
                              {
                                parameter8.Set(newMark);
                                if (parameter9 != null && !parameter9.IsReadOnly)
                                  parameter9.Set(newMark);
                                if (parameter10 != null && !parameter10.IsReadOnly)
                                  parameter10.Set(bar.IdentityDescriptionLong);
                              }
                            }
                          }
                          action &= ~CatalogAction.UpdateWithNewMarkNumber;
                        }
                        if (!flag2)
                        {
                          if (transaction2.HasStarted())
                          {
                            int num = (int) transaction2.RollBack();
                          }
                        }
                        else if (transaction3.Commit() != TransactionStatus.Committed)
                        {
                          flag2 = false;
                          TaskDialog taskDialog = new TaskDialog("EDGE Error");
                          taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                          taskDialog.AllowCancellation = false;
                          taskDialog.MainInstruction = "Failed To Commit Catalog NON Conforming Marks Transaction";
                          taskDialog.MainContent = "There has been an error in committing this transaction.  Rebar bar mark automation must be disabled until the error can be resolved.  Please contact support for assistance resolving this error.";
                          taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Disable Bar Mark Automation", "Bar mark automation will be disabled for this document.");
                          taskDialog.Show();
                        }
                        qaTimer10.Stop();
                      }
                      if (flag2 & flag1)
                        TaskDialog.Show("Error", stringBuilder.ToString());
                    }
                    if (flag2)
                    {
                      TransactionStatus transactionStatus = transactionGroup.GetStatus();
                      if (transactionGroup.HasStarted())
                        transactionStatus = transactionGroup.Commit();
                      qaTimer7.Stop();
                      if (transactionStatus != TransactionStatus.Committed)
                      {
                        QA.InHouseMessage("update transaction failed in Document Opening Build Rebar Dictionary");
                        int num = (int) transactionGroup.RollBack();
                        break;
                      }
                      break;
                    }
                    int num1 = (int) transactionGroup.RollBack();
                    break;
                  }
                  break;
                case AddDocTrackerResult.DisableBarMarkAutomation:
                  App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, false);
                  break;
              }
            }
            else
            {
              App.RebarManager.SetBarMarkAutomationForDocument(document.PathName, false);
              new TaskDialog("Missing Manufacturer Parameter Setting")
              {
                MainInstruction = "Manufacturer Parameter not Set",
                MainContent = "In order to use Rebar Mark Automation the Manufacturer parameter PROJECT_CLIENT_PRECAST_MANUFACTURER must be set in project info.  Set this parameter, save and reopen the file to use mark automation"
              }.Show();
            }
          }
          catch (Exception ex)
          {
            TaskDialog.Show("EDGE REBAR ERROR", ex.Message + Environment.NewLine + ex.StackTrace);
          }
          finally
          {
            App.DialogSwitches.SuspendModelLockingforOperation = false;
          }
        }
      }
      App.UpdateSheetAndAssemblyIdDictionary(document);
      App.DialogSwitches.SuspendModelLockingforOperation = false;
      qaTimer1.Stop();
      qaTimer1.Flush("On Document Opened");
    }
  }

  public void OnDocumentClosed(object sender, DocumentClosingEventArgs args)
  {
    Document document = args.Document;
    if (App.TicketManagerWindow != null)
      App.TicketManagerWindow.Close();
    if (App.RebarMarkAutomation && App.RebarManager.IsAutomatedDocument(document.PathName))
      App.RebarManager.DisposeOfMarkManager(document.PathName);
    if (ModelLockingUtils.ModelLockingEnabled(document) && !document.IsFamilyDocument)
      App.LockingManger.DisposeOfLockingManagerForDocument(document);
    if (App.insulationDrawingWindow == null)
      return;
    App.insulationDrawingWindow.Close();
  }

  public void OnDocumentSaved(object sender, DocumentSavedEventArgs args)
  {
  }

  public void OnDocumentSaving(object sender, DocumentSavingEventArgs args)
  {
  }

  public void OnDocumentSavingAs(object sender, DocumentSavingAsEventArgs args)
  {
  }

  public void OnDocumentSavedAs(object sender, DocumentSavedAsEventArgs args)
  {
    if (ModelLockingUtils.ModelLockingEnabled(args.Document))
      App.LockingManger.HandleModelSavedAs(args.OriginalPath, args.Document.PathName);
    if (!App.RebarManager.IsAutomatedDocument(args.OriginalPath))
      return;
    App.RebarManager.HandleModelSavedAs(args.OriginalPath, args.Document.PathName);
  }

  public void OnDocumentSynchronizingWithCentral(
    object sender,
    DocumentSynchronizingWithCentralEventArgs args)
  {
    App.bSyncingWithCentral = true;
    App.DialogSwitches.SuspendModelLockingforOperation = true;
  }

  public void OnDocumentSynchronizedWithCentral(
    object sender,
    DocumentSynchronizedWithCentralEventArgs args)
  {
    App.bSyncingWithCentral = false;
    App.DialogSwitches.SuspendModelLockingforOperation = false;
  }

  private void OnReloadLatestCommandExecuted(object sender, ExecutedEventArgs args)
  {
    if (QA.inHouseShowDebugMessages)
      App.ShowDialog("EDGE Custom RL Handler", "This is EDGE Custom Reload Latest Command Handler.");
    Document activeDocument = ((CommandEventArgs) args).ActiveDocument;
    try
    {
      App.DialogSwitches.SuspendModelLockingforOperation = true;
      App.bSynchronizingWithCentral = true;
      this.EDGEReloadLatest(activeDocument);
      if (!QA.RebarMarkAutomation_Worksharing_Enabled || App.RebarManager.Manager(activeDocument.PathName).VerifyMarkLists(activeDocument))
        return;
      TaskDialog.Show("EDGE Error", "Project Mark List Verification Failed, see log for detail at time-stamp: " + DateTime.Now.ToString());
    }
    catch (Exception ex)
    {
      App.bSynchronizingWithCentral = false;
      TaskDialog.Show("ERROR", ex.Message);
      throw;
    }
    finally
    {
      App.bSynchronizingWithCentral = false;
      App.DialogSwitches.SuspendModelLockingforOperation = false;
    }
  }

  private void OnSynchonizeWithCentral_MODIFY_CommandExecuted(object sender, ExecutedEventArgs args)
  {
    if (QA.inHouseShowDebugMessages)
      App.ShowDialog("EDGE Custom SWC Handler", "This is EDGE Custom Synchronize with Central Command Handler.");
    QA.LogVerboseRebarSWCTrace("______________________________________________________________________________________________________________________________");
    QA.LogVerboseRebarSWCTrace("*                                                                                                                            *");
    QA.LogVerboseRebarSWCTrace("*                                        EDGE Sync With Central Begin                                                        *");
    QA.LogVerboseRebarSWCTrace("*                                                                                                                            *");
    QA.LogVerboseRebarSWCTrace("------------------------------------------------------------------------------------------------------------------------------");
    QA.LogVerboseRebarSWCTrace("");
    Document activeDocument = ((CommandEventArgs) args).ActiveDocument;
    EDGESyncWithCentral_Form syncWithCentralForm = new EDGESyncWithCentral_Form(activeDocument);
    syncWithCentralForm.bSaveBeforeAndAfter = App.DialogSwitches.SWCSaveBeforeAndAfter;
    if (syncWithCentralForm.ShowDialog() == DialogResult.Cancel)
      return;
    App.DialogSwitches.SWCSaveBeforeAndAfter = syncWithCentralForm.bSaveBeforeAndAfter;
    SynchronizeWithCentralOptions swcOptions = syncWithCentralForm.SWCOptions;
    this.SWCCoreFunctionality(activeDocument, swcOptions);
  }

  private void OnSynchonizeWithCentral_QUICK_CommandExecuted(object sender, ExecutedEventArgs args)
  {
    if (QA.inHouseShowDebugMessages)
      App.ShowDialog("EDGE Custom SWC Handler", "This is EDGE Custom Synchronize with Central Command Handler.");
    QA.LogVerboseRebarSWCTrace("______________________________________________________________________________________________________________________________");
    QA.LogVerboseRebarSWCTrace("*                                                                                                                            *");
    QA.LogVerboseRebarSWCTrace("*                                        EDGE Sync With Central Begin                                                        *");
    QA.LogVerboseRebarSWCTrace("*                                                                                                                            *");
    QA.LogVerboseRebarSWCTrace("------------------------------------------------------------------------------------------------------------------------------");
    QA.LogVerboseRebarSWCTrace("");
    Document activeDocument = ((CommandEventArgs) args).ActiveDocument;
    SynchronizeWithCentralOptions SWCOptions = new SynchronizeWithCentralOptions();
    SWCOptions.SetRelinquishOptions(new RelinquishOptions(false)
    {
      CheckedOutElements = true,
      FamilyWorksets = true,
      StandardWorksets = true,
      ViewWorksets = true,
      UserWorksets = false
    });
    this.SWCCoreFunctionality(activeDocument, SWCOptions);
  }

  private void SWCCoreFunctionality(Document revitDoc, SynchronizeWithCentralOptions SWCOptions)
  {
    try
    {
      App.bSynchronizingWithCentral = true;
      App.DialogSwitches.SuspendModelLockingforOperation = true;
      if (App.CanReloadLatest(revitDoc))
      {
        do
        {
          this.EDGEReloadLatest(revitDoc);
        }
        while (!revitDoc.HasAllChangesFromCentral());
      }
      TransactWithCentralOptions transactOptions = new TransactWithCentralOptions();
      SWCOptions.SaveLocalBefore = true;
      SWCOptions.SaveLocalAfter = true;
      revitDoc.SynchronizeWithCentral(transactOptions, SWCOptions);
      if (QA.RebarMarkAutomation_Worksharing_Enabled && !App.RebarManager.Manager(revitDoc.PathName).VerifyMarkLists(revitDoc))
        TaskDialog.Show("EDGE Error", "Project Mark List Verification Failed, see log for detail at time-stamp: " + DateTime.Now.ToString());
      App.bSynchronizingWithCentral = false;
    }
    catch (Exception ex)
    {
      App.bSynchronizingWithCentral = false;
      TaskDialog.Show("ERROR", ex.Message);
      throw;
    }
    finally
    {
      App.bSynchronizingWithCentral = false;
      App.DialogSwitches.SuspendModelLockingforOperation = false;
    }
  }

  private void OnViewActivated(object sender, ViewActivatedEventArgs args)
  {
    if (!(sender is UIApplication uiApplication))
      return;
    UIDocument activeUiDocument = uiApplication.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    if (App.currentDocument == null || !App.currentDocument.IsValidObject)
      App.currentDocument = document;
    else if (!document.Equals((object) App.currentDocument))
    {
      MainWindow.needToRefreshSettingFile = true;
      App.currentDocument = document;
      ActiveModel.GetInformation(document);
      if (!document.IsFamilyDocument)
        this.updateRebarSettings(document);
    }
    if (App.TicketManagerWindow != null)
      App.TicketManagerWindow.checkOnHoldEditComment(document);
    if (App.insulationDrawingWindow != null)
      App.insulationDrawingWindow.UpdateActiveViewLabel(activeUiDocument);
    VisibilityToggleFunctions.CheckVisibilityFilters(uiApplication, args);
    try
    {
      if (App.templateCreatorWindow != null)
      {
        Autodesk.Revit.DB.View activeView = activeUiDocument.ActiveView;
        AssemblyInstance element = document.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance;
        App.templateCreatorWindow.setAssemblyInstance(element);
        App.templateCreatorWindow.setActiveView(activeView);
        App.templateCreatorWindow.setRevitDoc(document);
        App.templateCreatorWindow.setUiDoc(uiApplication.ActiveUIDocument);
        App.templateCreatorWindow.setUiApp(uiApplication);
      }
    }
    catch (Exception ex)
    {
    }
    try
    {
      if (App.hwTemplateCreatorWindow == null)
        return;
      Autodesk.Revit.DB.View activeView = activeUiDocument.ActiveView;
      AssemblyInstance element = document.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance;
      App.hwTemplateCreatorWindow.setAssemblyInstance(element);
      App.hwTemplateCreatorWindow.setActiveView(activeView);
      App.hwTemplateCreatorWindow.setRevitDoc(document);
      App.hwTemplateCreatorWindow.setUiDoc(uiApplication.ActiveUIDocument);
      App.hwTemplateCreatorWindow.setUiApp(uiApplication);
    }
    catch (Exception ex)
    {
    }
  }

  private void updateRebarSettings(Document revitDoc)
  {
    if (revitDoc == null || revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER") == null)
      return;
    string str = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER").AsString();
    if (str.ToUpper() == "ROCKY MOUNTAIN PRESTRESS" || str.ToUpper() == "KERKSTRA" || str.ToUpper() == "WELLS" || str.ToUpper() == "CORESLAB ONT" || str.ToUpper() == "MIDSTATES" || str.ToUpper() == "ILLINI" || str.ToUpper() == "SPANCRETE" || str.ToUpper() == "TINDALL")
    {
      BarHasher.useMetricEuro = false;
      if (App.RebarManager == null || !App.RebarManager.BarMarkManagers.ContainsKey(revitDoc.PathName))
        return;
      App.RebarManager.BarMarkManagers[revitDoc.PathName].mfgSettingsData.bUseMetricEuropean = false;
    }
    else
    {
      if (revitDoc == null)
        return;
      if (Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(revitDoc))
      {
        BarHasher.useMetricEuro = true;
        if (App.RebarManager == null || !App.RebarManager.BarMarkManagers.ContainsKey(revitDoc.PathName))
          return;
        App.RebarManager.BarMarkManagers[revitDoc.PathName].mfgSettingsData.bUseMetricEuropean = true;
      }
      else
      {
        BarHasher.useMetricEuro = false;
        if (App.RebarManager == null || !App.RebarManager.BarMarkManagers.ContainsKey(revitDoc.PathName))
          return;
        App.RebarManager.BarMarkManagers[revitDoc.PathName].mfgSettingsData.bUseMetricEuropean = false;
      }
    }
  }

  private static void laserExportFailureCheck(object sender, FailuresProcessingEventArgs args)
  {
    FailuresAccessor failuresAccessor = args.GetFailuresAccessor();
    IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
    int num = 0;
    foreach (FailureMessageAccessor failure in (IEnumerable<FailureMessageAccessor>) failureMessages)
    {
      if (failure.GetDescriptionText().Contains("Line is slightly off axis and may cause inaccuracies."))
        failuresAccessor.DeleteWarning(failure);
      else
        ++num;
    }
    if (0 >= num || args.GetProcessingResult() == FailureProcessingResult.Continue)
      return;
    args.SetProcessingResult(FailureProcessingResult.Continue);
  }

  public static void laserExportErrorCheck(bool on)
  {
    if (on)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      App.appForPane.ControlledApplication.FailuresProcessing += App.\u003C\u003EO.\u003C0\u003E__laserExportFailureCheck ?? (App.\u003C\u003EO.\u003C0\u003E__laserExportFailureCheck = new EventHandler<FailuresProcessingEventArgs>(App.laserExportFailureCheck));
    }
    else
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      App.appForPane.ControlledApplication.FailuresProcessing -= App.\u003C\u003EO.\u003C0\u003E__laserExportFailureCheck ?? (App.\u003C\u003EO.\u003C0\u003E__laserExportFailureCheck = new EventHandler<FailuresProcessingEventArgs>(App.laserExportFailureCheck));
    }
  }

  private static void HardwareDetailWarningSuppression(
    object sender,
    FailuresProcessingEventArgs args)
  {
    FailuresAccessor failuresAccessor = args.GetFailuresAccessor();
    IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
    int num = 0;
    foreach (FailureMessageAccessor failure in (IEnumerable<FailureMessageAccessor>) failureMessages)
    {
      if (failure.GetDescriptionText().Contains("Instance origin does not lie on host face. Instance will lose association to host."))
        failuresAccessor.DeleteWarning(failure);
      else if (failure.GetDescriptionText().Contains("Edits caused the last instance of an assembly type to be deleted. The assembly type was removed from the Project Browser."))
        failuresAccessor.DeleteWarning(failure);
      else if (failure.GetDescriptionText().Contains("There are identical instances in the same place. This will result in double counting in schedules."))
        failuresAccessor.DeleteWarning(failure);
      else
        ++num;
    }
    if (0 >= num || args.GetProcessingResult() == FailureProcessingResult.Continue)
      return;
    args.SetProcessingResult(FailureProcessingResult.Continue);
  }

  public static void hardwareDetailErrorCheck(bool on)
  {
    if (on)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      App.appForPane.ControlledApplication.FailuresProcessing += App.\u003C\u003EO.\u003C1\u003E__HardwareDetailWarningSuppression ?? (App.\u003C\u003EO.\u003C1\u003E__HardwareDetailWarningSuppression = new EventHandler<FailuresProcessingEventArgs>(App.HardwareDetailWarningSuppression));
    }
    else
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      App.appForPane.ControlledApplication.FailuresProcessing -= App.\u003C\u003EO.\u003C1\u003E__HardwareDetailWarningSuppression ?? (App.\u003C\u003EO.\u003C1\u003E__HardwareDetailWarningSuppression = new EventHandler<FailuresProcessingEventArgs>(App.HardwareDetailWarningSuppression));
    }
  }

  private void EDGEReloadLatest(Document revitDoc)
  {
    App.RebarManager.Manager(revitDoc.PathName);
    Dictionary<string, Rebar> dictionary1 = new Dictionary<string, Rebar>();
    HashSet<string> stringSet1 = new HashSet<string>();
    Dictionary<string, Rebar> dictionary2 = new Dictionary<string, Rebar>();
    HashSet<string> stringSet2 = new HashSet<string>();
    List<Rebar> rebarList = new List<Rebar>();
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    try
    {
      QA.LogLine("####################################################################################################################################################");
      QA.LogLine("##############        Before Reload Latest");
      try
      {
        App.CustomReloadLatest(revitDoc);
      }
      catch (Exception ex)
      {
        new TaskDialog("Edge Information")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = "Revit Reload Latest Failed",
          MainContent = $"Attempting to reload latest threw an exception: {ex.Message} EDGE Sync With Central Cannot Continue."
        }.Show();
        return;
      }
      if (!App.ModelLocking_WorksharingSupport)
        return;
      App.ReloadingLatest = true;
      App.LockingManger.GroomOnReloadLatest(revitDoc);
      QA.LogLine("##############        After Reload Latest");
      QA.LogLine(App.LockingManger.ToString());
      QA.LogLine("####################################################################################################################################################");
    }
    catch (Exception ex)
    {
      App.bSynchronizingWithCentral = false;
      TaskDialog.Show("ERROR", ex.Message);
      throw;
    }
    finally
    {
      App.ReloadingLatest = false;
    }
  }

  private void UpdateLocalModelWithChanges(
    Document revitDoc,
    RebarControlMarkManager3 currentManager,
    Dictionary<string, Rebar> Pre_RL_GUIDRebarDictionary,
    List<string> bumpedMarks,
    IList<string> modifiedGuidsList)
  {
    if (bumpedMarks == null)
      return;
    using (Transaction transaction = new Transaction(revitDoc, "Edge Rebar Reload Latest"))
    {
      TransactionStatus transactionStatus1 = transaction.Start();
      if (transactionStatus1 == TransactionStatus.Started)
      {
        foreach (KeyValuePair<string, Rebar> preRlGuidRebar in Pre_RL_GUIDRebarDictionary)
        {
          if (!modifiedGuidsList.Contains(preRlGuidRebar.Key) && preRlGuidRebar.Value.CanUseMarkInfo && bumpedMarks.Contains(preRlGuidRebar.Value.MarkInfo.CoreMark))
          {
            Element element = revitDoc.GetElement(preRlGuidRebar.Key);
            if (element != null)
            {
              string barMark = currentManager.GetBarMark(preRlGuidRebar.Value, bBumpedBar: true);
              Parameter parameter1 = element.LookupParameter("CONTROL_MARK");
              Parameter parameter2 = element.LookupParameter("IDENTITY_DESCRIPTION_SHORT");
              Parameter parameter3 = element.LookupParameter("IDENTITY_DESCRIPTION");
              if (parameter1 != null && !parameter1.IsReadOnly)
              {
                string str1 = parameter1.AsString();
                string str2 = parameter3.AsString();
                if (barMark != str1 || preRlGuidRebar.Value.IdentityDescriptionLong != str2)
                {
                  if (this.AttemptToCheckoutInAdvance(element))
                  {
                    parameter1.Set(barMark);
                    if (parameter2 != null && !parameter2.IsReadOnly)
                      parameter2.Set(barMark);
                    if (parameter3 != null && !parameter3.IsReadOnly)
                      parameter3.Set(preRlGuidRebar.Value.IdentityDescriptionLong);
                  }
                  else
                    new StackTrace().GetFrame(0).GetFileLineNumber();
                }
                else
                  QA.InHouseMessage($"------ We're processing Bumped bars but we're not updating the mark for this one, we probably should be, why aren't we? Revised mark: {barMark} existingBarMark: {str1} guid: {preRlGuidRebar.Key}");
              }
            }
            else
              QA.InHouseMessage("------ we couldn't get the bar from the model by it's unique id.. why???" + preRlGuidRebar.Key);
          }
        }
        TransactionStatus transactionStatus2 = transaction.Commit();
        if (transactionStatus2 == TransactionStatus.Committed)
          return;
        new TaskDialog("Edge Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = ("Unable to commit Edge Rebar Automation Reload Latest Processor Transaction.  Status: " + transactionStatus2.ToString())
        }.Show();
      }
      else
        new TaskDialog("Edge Error")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          MainInstruction = ("Unable to start Edge Rebar Automation Reload Latest Processor Transaction.  Status: " + transactionStatus1.ToString())
        }.Show();
    }
  }

  private static List<string> ProcessChangesFromCentral(
    RebarControlMarkManager3 currentManager,
    Dictionary<string, Rebar> Post_RL_GUIDRebarDictionary,
    IList<string> ModifiedBarGuids,
    IList<Rebar> AddedBars,
    IList<string> DeletedBarGuids)
  {
    List<string> bumpedMarksList = new List<string>();
    StringBuilder stringBuilder1 = new StringBuilder();
    foreach (string modifiedBarGuid in (IEnumerable<string>) ModifiedBarGuids)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(modifiedBarGuid))
        currentManager.HandleBarDeletion(modifiedBarGuid);
      else
        stringBuilder1.AppendLine($"---------------Failed to find Bar: {modifiedBarGuid} in post RL guid<->Rebar dictionary.  Find out why");
    }
    foreach (string modifiedBarGuid in (IEnumerable<string>) ModifiedBarGuids)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(modifiedBarGuid))
      {
        if (!currentManager.AddNewBarFromCentral(Post_RL_GUIDRebarDictionary[modifiedBarGuid], bumpedMarksList))
          stringBuilder1.AppendLine($"----------------Failed to Modify bar: {modifiedBarGuid} with mark: {Post_RL_GUIDRebarDictionary[modifiedBarGuid].BarMark}");
      }
      else
        stringBuilder1.AppendLine($"---------------Failed to find Bar: {modifiedBarGuid} in post RL guid<->Rebar dictionary.  Find out why");
    }
    if (stringBuilder1.Length > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE Error: Reload Latest - MODIFY");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Reload latest operation failed";
      taskDialog.MainContent = "Reload operation resulted in modified bars from the central model.  Unfortunately Edge was unable to process these bars.  See expanded content for more detail";
      taskDialog.ExpandedContent = stringBuilder1.ToString();
      taskDialog.Show();
      taskDialog.ToString();
    }
    foreach (string modifiedBarGuid in (IEnumerable<string>) ModifiedBarGuids)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(modifiedBarGuid))
        currentManager.PushBarFromCentral(Post_RL_GUIDRebarDictionary[modifiedBarGuid]);
      else
        QA.InHouseMessage($"Post_RL_GUIDRebarDictionary.ContainsKey(modGuid) couldn't find bar: {modifiedBarGuid} For Some reason we could not find {modifiedBarGuid} in the Post_RL_GUIDRebarDictionary while trying to process modified elements to call GetBarMark()");
    }
    StringBuilder stringBuilder2 = new StringBuilder();
    foreach (Rebar addedBar in (IEnumerable<Rebar>) AddedBars)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(addedBar.UniqueId))
      {
        if (!currentManager.AddNewBarFromCentral(Post_RL_GUIDRebarDictionary[addedBar.UniqueId], bumpedMarksList))
          stringBuilder2.AppendLine($"----------------Failed to Add bar: {addedBar.UniqueId} with mark: {Post_RL_GUIDRebarDictionary[addedBar.UniqueId].BarMark}");
      }
      else
        stringBuilder2.AppendLine($"---------------Failed to find Bar: {addedBar.Id.ToString()} Unique ID: {addedBar.UniqueId} in post RL guid<->Rebar dictionary.  Find out why");
    }
    if (stringBuilder2.Length > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE Error: Reload Latest - ADD BARS");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Reload latest operation failed";
      taskDialog.MainContent = "Reload operation resulted in new bars from the central model.  Unfortunately Edge was unable to process these bars.  See expanded content for more detail";
      taskDialog.ExpandedContent = stringBuilder2.ToString();
      taskDialog.Show();
      taskDialog.ToString();
    }
    foreach (Rebar addedBar in (IEnumerable<Rebar>) AddedBars)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(addedBar.UniqueId))
        currentManager.PushBarFromCentral(Post_RL_GUIDRebarDictionary[addedBar.UniqueId]);
      else
        QA.InHouseMessage("Post_RL_GUIDRebarDictionary.ContainsKey(addedInstance.UniqueId) couldn't find bar: " + addedBar.UniqueId);
    }
    return bumpedMarksList;
  }

  private static List<string> ProcessChangesFromCentral_AlternateImplementation(
    RebarControlMarkManager3 currentManager,
    Dictionary<string, Rebar> Post_RL_GUIDRebarDictionary,
    IList<FamilyInstance> AddedBars,
    IList<string> DeletedBarGuids)
  {
    foreach (string deletedBarGuid in (IEnumerable<string>) DeletedBarGuids)
      ;
    StringBuilder stringBuilder = new StringBuilder();
    List<string> bumpedMarksList = new List<string>();
    foreach (FamilyInstance addedBar in (IEnumerable<FamilyInstance>) AddedBars)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(addedBar.UniqueId))
      {
        if (!currentManager.AddNewBarFromCentral(Post_RL_GUIDRebarDictionary[addedBar.UniqueId], bumpedMarksList))
          stringBuilder.AppendLine($"----------------Failed to Add bar: {addedBar.UniqueId} with mark: {Post_RL_GUIDRebarDictionary[addedBar.UniqueId].BarMark}");
      }
      else
        stringBuilder.AppendLine($"---------------Failed to find Bar: {addedBar.Id.ToString()} Unique ID: {addedBar.UniqueId} in post RL guid<->Rebar dictionary.  Find out why");
    }
    if (stringBuilder.Length > 0)
    {
      TaskDialog taskDialog = new TaskDialog("EDGE Error: Reload Latest");
      taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
      taskDialog.MainInstruction = "Reload latest operation failed";
      taskDialog.MainContent = "Reload operation resulted in new bars from the central model.  Unfortunately Edge was unable to process these bars.  See expanded content for more detail";
      taskDialog.ExpandedContent = stringBuilder.ToString();
      taskDialog.Show();
      taskDialog.ToString();
    }
    foreach (FamilyInstance addedBar in (IEnumerable<FamilyInstance>) AddedBars)
    {
      if (Post_RL_GUIDRebarDictionary.ContainsKey(addedBar.UniqueId))
        currentManager.GetBarMark(Post_RL_GUIDRebarDictionary[addedBar.UniqueId], bBumpedBar: true);
      else
        QA.InHouseMessage("Post_RL_GUIDRebarDictionary.ContainsKey(addedInstance.UniqueId) couldn't find bar: " + addedBar.UniqueId);
    }
    return bumpedMarksList;
  }

  private static bool CanReloadLatest(Document revitDoc)
  {
    ModelPath centralModelPath = revitDoc.GetWorksharingCentralModelPath();
    return centralModelPath.ServerPath || !(ModelPathUtils.ConvertModelPathToUserVisiblePath(centralModelPath) == revitDoc.PathName);
  }

  private static bool CustomReloadLatest(Document revitDoc)
  {
    try
    {
      if (App.CanReloadLatest(revitDoc))
        revitDoc.ReloadLatest(new ReloadLatestOptions());
    }
    catch (Exception ex)
    {
      QA.InHouseMessage($"Reload Latest failed in Edge SWC Command.  Cannot complete Sync with central command.  Failed with: {ex.GetType().ToString()} {ex.Message}");
      throw new Exception(ex.Message);
    }
    return true;
  }

  private static void GetReloadLatestDiff(
    RebarControlMarkManager3 currentManager,
    Dictionary<string, Rebar> Pre_SyncGUIDRebarDictionary,
    HashSet<string> Pre_SyncModelGuids,
    IEnumerable<Rebar> Post_ReloadBarElementsInModel,
    Dictionary<string, Rebar> Post_RL_GUIDRebarDictionary,
    HashSet<string> Post_RL_GUIDHash,
    IList<Rebar> AddedBars,
    IList<string> DeletedBarGuids,
    IList<string> ModifiedBarGuids)
  {
    foreach (Rebar rebar in Post_ReloadBarElementsInModel)
    {
      if (!Pre_SyncModelGuids.Contains(rebar.UniqueId))
        AddedBars.Add(rebar);
    }
    foreach (string preSyncModelGuid in Pre_SyncModelGuids)
    {
      if (!Post_RL_GUIDHash.Contains(preSyncModelGuid))
        DeletedBarGuids.Add(preSyncModelGuid);
    }
    foreach (KeyValuePair<string, Rebar> preSyncGuidRebar in Pre_SyncGUIDRebarDictionary)
    {
      if (Post_RL_GUIDHash.Contains(preSyncGuidRebar.Key))
      {
        Rebar barToTest = preSyncGuidRebar.Value;
        if (Post_RL_GUIDRebarDictionary[preSyncGuidRebar.Key].IsModified(barToTest))
          ModifiedBarGuids.Add(preSyncGuidRebar.Key);
      }
    }
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("");
    stringBuilder.AppendLine("AddedItems: " + AddedBars.Count.ToString());
    int count = DeletedBarGuids.Count;
    stringBuilder.AppendLine("DeletedItems: " + count.ToString());
    count = ModifiedBarGuids.Count;
    stringBuilder.AppendLine("ModifiedItems: " + count.ToString());
  }

  private static void CatalogExisting_Before_ReloadLatest(
    Document revitDoc,
    RebarControlMarkManager3 currentManager,
    ref IEnumerable<Rebar> Pre_SyncWithCentralRebarStore,
    ref Dictionary<string, Rebar> Pre_SyncGUIDRebarDictionary,
    ref HashSet<string> Pre_SyncModelGuids)
  {
    App.CatalogBarsInModel(revitDoc, currentManager, ref Pre_SyncWithCentralRebarStore, ref Pre_SyncGUIDRebarDictionary, ref Pre_SyncModelGuids);
  }

  private static void CatalogExisting_After_ReloadLatest(
    Document revitDoc,
    RebarControlMarkManager3 currentManager,
    ref IEnumerable<Rebar> Post_RL_RebarStore,
    ref Dictionary<string, Rebar> Post_RL_GUIDRebarDictionary,
    ref HashSet<string> Post_RL_GUIDHash)
  {
    App.CatalogBarsInModel(revitDoc, currentManager, ref Post_RL_RebarStore, ref Post_RL_GUIDRebarDictionary, ref Post_RL_GUIDHash);
  }

  private static void CatalogBarsInModel(
    Document revitDoc,
    RebarControlMarkManager3 currentManager,
    ref IEnumerable<Rebar> RebarStore,
    ref Dictionary<string, Rebar> GUIDRebarDictionary,
    ref HashSet<string> ModelGuids)
  {
    IEnumerable<FamilyInstance> rebarElemsFromDoc = RebarCollectionUtils.GetAllEdgeRebarElemsFromDoc(revitDoc);
    RebarStore = rebarElemsFromDoc.Select<FamilyInstance, Rebar>((Func<FamilyInstance, Rebar>) (s => new Rebar(s, currentManager.mfgSettingsData)));
    if (GUIDRebarDictionary != null)
      GUIDRebarDictionary.Clear();
    else
      GUIDRebarDictionary = new Dictionary<string, Rebar>();
    if (ModelGuids != null)
      ModelGuids.Clear();
    else
      ModelGuids = new HashSet<string>();
    foreach (Rebar rebar in RebarStore)
    {
      GUIDRebarDictionary.Add(rebar.UniqueId, rebar);
      ModelGuids.Add(rebar.UniqueId);
    }
  }

  private bool AttemptToCheckoutInAdvance(Element element)
  {
    Document document = element.Document;
    string name = element.Category.Name;
    if (!WorksharingUtils.CheckoutElements(document, (ICollection<ElementId>) new ElementId[1]
    {
      element.Id
    }).Contains(element.Id))
      return false;
    switch (WorksharingUtils.GetModelUpdatesStatus(document, element.Id))
    {
      case ModelUpdatesStatus.DeletedInCentral:
      case ModelUpdatesStatus.UpdatedInCentral:
        return false;
      default:
        return true;
    }
  }

  private static bool IsDebug { get; set; }

  private void catalogUndoneElements(
    List<Element> recoveredElements,
    RebarControlMarkManager3 manager)
  {
    foreach (Element recoveredElement in recoveredElements)
    {
      Parameter parameter1 = recoveredElement.LookupParameter("CONTROL_MARK");
      Parameter parameter2 = Parameters.LookupParameter(recoveredElement, "BAR_DIAMETER");
      string parameterAsString = Parameters.GetParameterAsString(recoveredElement, "BAR_SHAPE");
      if (parameter1 != null && parameter2 != null)
      {
        string str = parameter1.AsString();
        double num = parameter2.AsDouble();
        if (BarHasher.useMetricEuro)
          num = UnitUtils.ConvertFromInternalUnits(num, UnitTypeId.Millimeters);
        if (BarDiameterOracle.IsValidDiameter(num))
        {
          int ibarDiam = BarDiameterOracle.ResolveBarDiam(num);
          string strForRealDiameter = BarDiameterOracle.GetDiameterStrForRealDiameter(num, manager.mfgSettingsData.bUseMetricCanadian, manager.mfgSettingsData.useZeroPaddingForBarSize);
          manager.UpdateGuidToElementIdMap(recoveredElement.UniqueId, recoveredElement.Id, str);
          if (recoveredElement is FamilyInstance)
          {
            Rebar bar = new Rebar(recoveredElement as FamilyInstance, manager.mfgSettingsData);
            manager.UpdateGuidToBarInfoMap(bar);
            if (!parameterAsString.ToUpper().Contains("STRAIGHT"))
            {
              manager.PushToProjectMarkList(str, recoveredElement.UniqueId, ibarDiam, strForRealDiameter, bar);
              manager.HandleUndoRedoModify(bar);
            }
          }
        }
      }
    }
  }

  private static void ReadPreferences() => EdgePrefsReader.ReadEdgePrefsFile();

  private static void ShowDialog(string title, string message)
  {
    new TaskDialog(title)
    {
      MainInstruction = message,
      TitleAutoPrefix = false
    }.Show();
  }

  public static void UpdateSheetAndAssemblyIdDictionary(Document revitDoc)
  {
    App.sheetAndAsssemblyIds.Clear();
    foreach (ViewSheet viewSheet in new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Sheets).OfClass(typeof (ViewSheet)).ToElements().Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (p => p.AssociatedAssemblyInstanceId.ToString() != "-1")).ToList<ViewSheet>())
    {
      if (viewSheet.AssociatedAssemblyInstanceId.ToString() != "-1")
        App.sheetAndAsssemblyIds.Add(viewSheet.Id.ToString(), viewSheet.AssociatedAssemblyInstanceId.ToString());
    }
  }

  public static void UpdateSheetCountParameter(AssemblyInstance assembInstance, Document doc)
  {
    List<ViewSheet> list = new FilteredElementCollector(doc).OfClass(typeof (ViewSheet)).Cast<ViewSheet>().Where<ViewSheet>((Func<ViewSheet, bool>) (s => s.AssociatedAssemblyInstanceId.Equals((object) assembInstance.Id))).ToList<ViewSheet>();
    int num = list.Count<ViewSheet>();
    foreach (Element elem in list)
    {
      Parameter parameter = Parameters.LookupParameter(elem, "TKT_SHEET_COUNT");
      if (parameter != null && !parameter.IsReadOnly)
        parameter.Set(num);
    }
  }

  public class ClearAVFResultsFromView : IExternalEventHandler
  {
    public void Execute(UIApplication app)
    {
      using (Transaction transaction = new Transaction(app.ActiveUIDocument.Document, "Clear AVF Results From View"))
      {
        int num1 = (int) transaction.Start();
        AVFViewTimer avfViewTimer = App.AVFViewTimers.Dequeue();
        SpatialFieldManager spatialFieldManager = SpatialFieldManager.GetSpatialFieldManager(avfViewTimer.viewWithAVFResults);
        if (spatialFieldManager != null)
        {
          try
          {
            spatialFieldManager.RemoveSpatialFieldPrimitive(avfViewTimer.PointIndex);
            spatialFieldManager.RemoveSpatialFieldPrimitive(avfViewTimer.FaceIndex);
          }
          catch
          {
          }
        }
        else if (avfViewTimer.manager != null)
        {
          try
          {
            avfViewTimer.manager.RemoveSpatialFieldPrimitive(avfViewTimer.PointIndex);
            avfViewTimer.manager.RemoveSpatialFieldPrimitive(avfViewTimer.FaceIndex);
          }
          catch
          {
          }
        }
        int num2 = (int) transaction.Commit();
      }
    }

    public string GetName() => "Clear AVF From View";
  }

  public enum CentralBarType
  {
    StraightOrStandard,
    Bent,
  }
}
