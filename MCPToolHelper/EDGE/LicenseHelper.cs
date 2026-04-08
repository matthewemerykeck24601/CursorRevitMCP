// Decompiled with JetBrains decompiler
// Type: EDGE.LicenseHelper
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using LogicNP.CryptoLicensing;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using Utils.AdminUtils;
using Utils.Models;

#nullable disable
namespace EDGE;

public class LicenseHelper
{
  private static string appPath = Path.GetDirectoryName(typeof (App).Assembly.Location);
  private static string LicenseKey = "AMAAMACaF4EZszMfaiNochXjrLznUQHV89DMVX0Ouca6XhjX64ZeSz9mIgFNEvDWW77qSGMDAAEAAQ==";
  private static string ProductYear = "2024";
  private static string HelpFileLocation = "http://www.edgeforrevit.com/licensing";
  private static string ProductConfigName = "EDGEforRevit";
  private static string ProductPrintName = "EDGE^R";
  private static string LicenseAgreementPath = LicenseHelper.appPath + "\\Enduser_License_Agreement.rtf";
  private static string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData).ToString() + "\\PTAC\\EdgeR\\Beta\\";
  private static string CompanyName = "PTAC Consulting Engineers";
  private static string CompanyEmailAddress = "edge@PTAC.com";
  private static string EdgeVersion = "12.2.0";
  private static AesManaged aes = new AesManaged();
  public static bool bInHouseMode = false;
  private static string fullHelpPath = LicenseHelper.HelpFileLocation;

  public static bool ValidateLicense()
  {
    CryptoLicense cryptoLicense = new CryptoLicense(LicenseStorageMode.ToRegistry, "AMAAMACDmtRL9ua5AQ3wKYGBO27DAs5QQf81svH2IkA2sHuuCMmhqxK9QUoISkn3UDbsjOMDAAEAAQ==");
    return cryptoLicense.Load() && cryptoLicense.Status != LicenseStatus.Deactivated;
  }

  public static EdgeLicenseStatus CloudValidationLicense()
  {
    int num1 = 7;
    CryptoLicense cryptoLicense = new CryptoLicense(LicenseStorageMode.ToFile, $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic", LicenseHelper.LicenseKey);
    EdgeLicense edgeLicense = (EdgeLicense) null;
    cryptoLicense.ID.ToString();
    int num2 = LicenseHelper.CheckInternetConnection() ? 1 : 0;
    string str1 = "";
    if (cryptoLicense.Load())
      str1 = cryptoLicense.LicenseCode;
    string machineCodeAsString = cryptoLicense.GetLocalMachineCodeAsString();
    string machineName = Environment.MachineName;
    string name = WindowsIdentity.GetCurrent().Name;
    string product = "EdgeForRevit";
    if (num2 != 0)
    {
      string s1 = "";
      string s2 = "";
      string str2 = "";
      string str3 = "";
      bool flag1 = false;
      if (!cryptoLicense.Load())
        return EdgeLicenseStatus.NoLicense;
      try
      {
        RestClient restClient = new RestClient("http://edgelicensingservice.azurewebsites.net");
        RestRequest request1 = new RestRequest("/api/getlicense", Method.POST)
        {
          RequestFormat = DataFormat.Json
        };
        request1.AddHeader("content-type", "application/json");
        EDGELicenseModel edgeLicenseModel1 = new EDGELicenseModel(machineCodeAsString, name, product, str1, LicenseHelper.ProductYear, machineName);
        request1.AddJsonBody((object) edgeLicenseModel1);
        JsonObject jsonObject1 = SimpleJson.DeserializeObject(restClient.Execute((IRestRequest) request1).Content) as JsonObject;
        List<string> stringList1 = new List<string>();
        string usename;
        string str4;
        string edgeVer;
        bool flag2;
        if (jsonObject1 != null)
        {
          usename = jsonObject1["Username"].ToString();
          jsonObject1["Product"].ToString();
          str2 = jsonObject1["LicenseKey"].ToString();
          str4 = jsonObject1["LicenseStatus"].ToString();
          edgeVer = jsonObject1["EdgeVersion"].ToString();
          jsonObject1["EULAAccepted"].ToString();
          s1 = jsonObject1["ExpirationDate"].ToString();
          jsonObject1["Postpone"].ToString();
          s2 = jsonObject1["PostponeDate"].ToString();
          str3 = jsonObject1["Perpetual"].ToString();
          flag2 = true;
        }
        else
        {
          RestRequest request2 = new RestRequest("/api/getlicense", Method.POST)
          {
            RequestFormat = DataFormat.Json
          };
          request2.AddHeader("content-type", "application/json");
          EDGELicenseModel edgeLicenseModel2 = new EDGELicenseModel(machineCodeAsString, product, str1, LicenseHelper.ProductYear);
          request2.AddJsonBody((object) edgeLicenseModel2);
          if (!(SimpleJson.DeserializeObject(restClient.Execute((IRestRequest) request2).Content) is JsonObject jsonObject2))
            return EdgeLicenseStatus.NoLicense;
          usename = name;
          jsonObject2["Product"].ToString();
          str2 = jsonObject2["LicenseKey"].ToString();
          str4 = jsonObject2["LicenseStatus"].ToString();
          edgeVer = jsonObject2["EdgeVersion"].ToString();
          jsonObject2["EULAAccepted"].ToString();
          s1 = jsonObject2["ExpirationDate"].ToString();
          jsonObject2["Postpone"].ToString();
          s2 = jsonObject2["PostponeDate"].ToString();
          str3 = jsonObject2["Perpetual"].ToString();
          flag2 = true;
          if (jsonObject2["Username"] != null && jsonObject2["Username"].ToString().Trim() != "" && jsonObject2["Username"].ToString().Trim() != name.Trim() || jsonObject2["NetBIOS"] != null && jsonObject2["NetBIOS"].ToString().Trim() != "" && jsonObject2["NetBIOS"].ToString().Trim() != machineName.Trim())
            return EdgeLicenseStatus.NoLicense;
          LicenseHelper.WriteToFields(str1);
        }
        LicenseHelper.SaveEDGEBuild(str1);
        List<string> stringList2 = new List<string>();
        LicenseStatus result1 = LicenseStatus.Valid;
        EdgeLicenseStatus result2 = EdgeLicenseStatus.Deactivated;
        System.Enum.TryParse<LicenseStatus>(str4, out result1);
        System.Enum.TryParse<EdgeLicenseStatus>(str4, out result2);
        if (!System.IO.File.Exists($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic"))
          return EdgeLicenseStatus.EULANotAccepted;
        if (flag2)
          edgeLicense = new EdgeLicense(machineCodeAsString, str2, usename, result2, edgeVer);
      }
      catch (Exception ex)
      {
        int num3 = (int) MessageBox.Show(ex.StackTrace);
      }
      if (edgeLicense == null)
        return EdgeLicenseStatus.NoLicense;
      if (edgeLicense.Status == EdgeLicenseStatus.Deactivated)
        return EdgeLicenseStatus.Deactivated;
      if (edgeLicense.Status == EdgeLicenseStatus.Invalid)
        return EdgeLicenseStatus.Invalid;
      if (edgeLicense.Status == EdgeLicenseStatus.Expired)
        return EdgeLicenseStatus.Expired;
      if (edgeLicense.Status == EdgeLicenseStatus.ExpiredEvaluation)
        return EdgeLicenseStatus.ExpiredEvaluation;
      if (edgeLicense.Status == EdgeLicenseStatus.EULANotAccepted)
        return EdgeLicenseStatus.EULANotAccepted;
      if (edgeLicense.Status == EdgeLicenseStatus.Error)
        return EdgeLicenseStatus.Error;
      if (edgeLicense.Status == EdgeLicenseStatus.NoLicense)
        return EdgeLicenseStatus.NoLicense;
      if (!str3.ToUpper().Equals("TRUE") && !str3.Equals("1"))
      {
        if (s1 != null && s1 != "")
        {
          DateTime dateTime1 = DateTime.Parse(s1);
          DateTime onlineTime = LicenseHelper.GetOnlineTime();
          DateTime dateTime2 = new DateTime();
          if (s2 != null && s2 != "")
            dateTime2 = DateTime.Parse(s2);
          bool flag3 = dateTime2.Date > onlineTime.Date;
          if (dateTime1.Date < onlineTime.Date)
            return EdgeLicenseStatus.Expired;
          if ((dateTime1.Date - onlineTime.Date).Days < 30 && (dateTime1.Date - onlineTime.Date).Days > 5 && !flag3)
          {
            TaskDialog taskDialog = new TaskDialog("EDGE^R - License Days Remaining");
            taskDialog.Id = "ID_License_Days_Remaining";
            taskDialog.Title = "EDGE^R - License Days Remaining";
            taskDialog.TitleAutoPrefix = false;
            taskDialog.AllowCancellation = true;
            taskDialog.MainInstruction = "License Days Remaining";
            taskDialog.MainContent = $"There are {(dateTime1.Date - onlineTime.Date).Days.ToString()} days remaining on your EDGE^R License.";
            taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Postpone Notification");
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
            taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
            taskDialog.DefaultButton = (TaskDialogResult) 1;
            TaskDialogResult taskDialogResult = taskDialog.Show();
            if (taskDialogResult == 1001)
              LicenseHelper.PostPoneMessage(str2);
            else if (taskDialogResult == 1002)
            {
              string eulaGuid = Guid.NewGuid().ToString();
              if (LicenseHelper.AcceptEULA(eulaGuid))
                LicenseHelper.RequestNewLicense(LicenseHelper.GetOnlineTime().Date.ToString("MM/dd/yyyy"), eulaGuid);
            }
          }
          else if ((dateTime1.Date - onlineTime.Date).Days <= 5)
          {
            TaskDialog taskDialog = new TaskDialog("EDGE^R - License Days Remaining");
            taskDialog.Id = "ID_License_Days_Remaining";
            taskDialog.Title = "EDGE^R - License Days Remaining";
            taskDialog.TitleAutoPrefix = false;
            taskDialog.AllowCancellation = true;
            taskDialog.MainInstruction = "License Days Remaining";
            taskDialog.MainContent = $"There are {(dateTime1.Date - onlineTime.Date).Days.ToString()} day(s) remaining on your EDGE^R License.";
            taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Request New License");
            taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
            taskDialog.DefaultButton = (TaskDialogResult) 1;
            if (taskDialog.Show() == 1001)
            {
              string eulaGuid = Guid.NewGuid().ToString();
              if (LicenseHelper.AcceptEULA(eulaGuid))
                LicenseHelper.RequestNewLicense(LicenseHelper.GetOnlineTime().Date.ToString("MM/dd/yyyy"), eulaGuid);
            }
          }
          else if ((dateTime1.Date - onlineTime.Date).Days == 0)
          {
            TaskDialog taskDialog = new TaskDialog("EDGE^R - License Days Remaining");
            taskDialog.Id = "ID_License_Days_Remaining";
            taskDialog.Title = "EDGE^R - License Days Remaining";
            taskDialog.TitleAutoPrefix = false;
            taskDialog.AllowCancellation = true;
            taskDialog.MainInstruction = "License Days Remaining";
            taskDialog.MainContent = "There are 0 days remaining on your EDGE^R License.";
            taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
            taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Request New License");
            taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
            taskDialog.DefaultButton = (TaskDialogResult) 1;
            if (taskDialog.Show() == 1001)
            {
              string eulaGuid = Guid.NewGuid().ToString();
              if (LicenseHelper.AcceptEULA(eulaGuid))
                LicenseHelper.RequestNewLicense(LicenseHelper.GetOnlineTime().Date.ToString("MM/dd/yyyy"), eulaGuid);
            }
          }
        }
        if (s1 != null && s1 != "")
        {
          DateTime dateTime = DateTime.Parse(s1);
          DateTime onlineTime = LicenseHelper.GetOnlineTime();
          TimeSpan timeSpan = dateTime.Date - onlineTime.Date;
          if (timeSpan.Days < 7)
          {
            timeSpan = dateTime.Date - onlineTime.Date;
            num1 = timeSpan.Days;
          }
        }
      }
      else
      {
        if (s1 != null && s1 != "")
        {
          DateTime dateTime3 = DateTime.Parse(s1);
          DateTime dateTime4 = new DateTime();
          DateTime dateTime5 = DateTime.Parse("2025.06.10");
          DateTime dateTime6 = new DateTime();
          if (s2 != null && s2 != "")
            dateTime6 = DateTime.Parse(s2);
          flag1 = dateTime6.Date > dateTime5.Date;
          if (dateTime3.Date < dateTime5.Date)
            return EdgeLicenseStatus.Expired;
        }
        if (s1 != null && s1 != "")
        {
          DateTime dateTime7 = DateTime.Parse(s1);
          DateTime dateTime8 = new DateTime();
          DateTime dateTime9 = DateTime.Parse("2025.06.10");
          TimeSpan timeSpan = dateTime7.Date - dateTime9.Date;
          if (timeSpan.Days < 7)
          {
            timeSpan = dateTime7.Date - dateTime9.Date;
            num1 = timeSpan.Days;
          }
        }
      }
      LicenseHelper.SaveTempLic(LicenseHelper.GetOnlineTime().AddDays((double) num1), LicenseHelper.GetOnlineTime());
      return EdgeLicenseStatus.Valid;
    }
    if (!System.IO.File.Exists($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevitTemp.txt"))
      return EdgeLicenseStatus.NoOffline;
    string str5;
    try
    {
      str5 = LicenseHelper.LoadTempLic();
    }
    catch
    {
      return EdgeLicenseStatus.InvalidOffline;
    }
    string[] strArray = str5.Split((char[]) null);
    DateTime dateTime10 = DateTime.Parse(strArray[1]);
    DateTime expDate = DateTime.Parse(strArray[0]);
    DateTime now = DateTime.Now;
    if (now.Date < dateTime10.Date)
      return EdgeLicenseStatus.InvalidOffline;
    if (expDate.Date < now.Date)
      return EdgeLicenseStatus.ExpiredOffline;
    new TaskDialog("EDGE^R - License Days Remaining")
    {
      Id = "ID_License_Days_Remaining",
      Title = "EDGE^R - License Days Remaining",
      TitleAutoPrefix = false,
      AllowCancellation = true,
      MainInstruction = "License Days Remaining",
      MainContent = $"There are {(expDate.Date - now.Date).Days.ToString()} days remaining on your offline EDGE^R License.",
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      CommonButtons = ((TaskDialogCommonButtons) 1),
      DefaultButton = ((TaskDialogResult) 1)
    }.Show();
    LicenseHelper.SaveTempLic(expDate, now);
    return EdgeLicenseStatus.Valid;
  }

  public static bool EnterNewLicense()
  {
    int num1 = 7;
    int num2 = LicenseHelper.CheckInternetConnection() ? 1 : 0;
    string product = "EdgeForRevit";
    CryptoLicense cryptoLicense = new CryptoLicense(LicenseStorageMode.ToFile, LicenseHelper.LicenseKey);
    if (num2 != 0)
    {
      string machineCodeAsString = cryptoLicense.GetLocalMachineCodeAsString();
      string machineName = Environment.MachineName;
      string name = WindowsIdentity.GetCurrent().Name;
      bool flag1 = false;
      while (!flag1)
      {
        LicenseForm licenseForm = new LicenseForm();
        int num3 = (int) licenseForm.ShowDialog();
        if (licenseForm.cancelled)
          return false;
        cryptoLicense.LicenseCode = licenseForm.licenseCode;
        string licenseCode = cryptoLicense.LicenseCode;
        cryptoLicense.FileStoragePath = $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic";
        if (LicenseHelper.CheckInternetConnection())
        {
          try
          {
            bool flag2 = false;
            bool flag3 = false;
            EdgeLicense edgeLicense = (EdgeLicense) null;
            string usename = "";
            string str1 = "";
            string str2 = "";
            string edgeVer = "";
            string s1 = "";
            string s2 = "";
            string str3 = "";
            if (licenseCode == null)
              return false;
            RestClient restClient = new RestClient("http://edgelicensingservice.azurewebsites.net");
            RestRequest request1 = new RestRequest("/api/getlicense", Method.POST)
            {
              RequestFormat = DataFormat.Json
            };
            request1.AddHeader("content-type", "application/json");
            EDGELicenseModel edgeLicenseModel1 = new EDGELicenseModel(machineCodeAsString, name, product, licenseCode, LicenseHelper.ProductYear, machineName);
            request1.AddJsonBody((object) edgeLicenseModel1);
            JsonObject jsonObject1 = SimpleJson.DeserializeObject(restClient.Execute((IRestRequest) request1).Content) as JsonObject;
            List<string> stringList1 = new List<string>();
            if (jsonObject1 != null)
            {
              usename = jsonObject1["Username"].ToString();
              jsonObject1["Product"].ToString();
              str1 = jsonObject1["LicenseKey"].ToString();
              str2 = jsonObject1["LicenseStatus"].ToString();
              edgeVer = jsonObject1["EdgeVersion"].ToString();
              jsonObject1["EULAAccepted"].ToString();
              s1 = jsonObject1["ExpirationDate"].ToString();
              jsonObject1["Postpone"].ToString();
              s2 = jsonObject1["PostponeDate"].ToString();
              str3 = jsonObject1["Perpetual"].ToString();
              flag2 = true;
            }
            else
            {
              bool flag4 = false;
              RestRequest request2 = new RestRequest("/api/getlicense", Method.POST)
              {
                RequestFormat = DataFormat.Json
              };
              request2.AddHeader("content-type", "application/json");
              EDGELicenseModel edgeLicenseModel2 = new EDGELicenseModel(machineCodeAsString, product, licenseCode, LicenseHelper.ProductYear);
              request2.AddJsonBody((object) edgeLicenseModel2);
              if (SimpleJson.DeserializeObject(restClient.Execute((IRestRequest) request2).Content) is JsonObject jsonObject2)
              {
                usename = name;
                jsonObject2["Product"].ToString();
                str1 = jsonObject2["LicenseKey"].ToString();
                str2 = jsonObject2["LicenseStatus"].ToString();
                edgeVer = jsonObject2["EdgeVersion"].ToString();
                jsonObject2["EULAAccepted"].ToString();
                s1 = jsonObject2["ExpirationDate"].ToString();
                jsonObject2["Postpone"].ToString();
                s2 = jsonObject2["PostponeDate"].ToString();
                str3 = jsonObject2["Perpetual"].ToString();
                flag2 = true;
                if (jsonObject2["Username"] != null && jsonObject2["Username"].ToString().Trim() != "" && jsonObject2["Username"].ToString().Trim() != name.Trim() || jsonObject2["NetBIOS"] != null && jsonObject2["NetBIOS"].ToString().Trim() != "" && jsonObject2["NetBIOS"].ToString().Trim() != machineName.Trim())
                  flag4 = true;
              }
              else
                flag4 = true;
              if (flag4)
              {
                new TaskDialog("License Code Not Found")
                {
                  FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                  TitleAutoPrefix = false,
                  AllowCancellation = false,
                  MainInstruction = "License Code Not Found.",
                  MainContent = "License Code was not found in our database. Please try again or contact EDGE Support."
                }.Show();
                continue;
              }
              LicenseHelper.WriteToFields(licenseCode);
            }
            LicenseHelper.SaveEDGEBuild(licenseCode);
            List<string> stringList2 = new List<string>();
            LicenseStatus result1 = LicenseStatus.Valid;
            EdgeLicenseStatus result2 = EdgeLicenseStatus.Deactivated;
            System.Enum.TryParse<LicenseStatus>(str2, out result1);
            System.Enum.TryParse<EdgeLicenseStatus>(str2, out result2);
            if (flag2)
              edgeLicense = new EdgeLicense(machineCodeAsString, str1, usename, result2, edgeVer);
            if (edgeLicense.Status == EdgeLicenseStatus.Valid)
            {
              cryptoLicense.ResetStatus();
              cryptoLicense.Save();
              if (!str3.ToUpper().Equals("TRUE") && !str3.Equals("1"))
              {
                if (s1 != null && s1 != "")
                {
                  DateTime dateTime1 = DateTime.Parse(s1);
                  DateTime onlineTime = LicenseHelper.GetOnlineTime();
                  DateTime dateTime2 = new DateTime();
                  if (s2 != null && s2 != "")
                    dateTime2 = DateTime.Parse(s2);
                  bool flag5 = dateTime2.Date > onlineTime.Date;
                  if (dateTime1.Date < onlineTime.Date)
                  {
                    new TaskDialog("License Code Expired")
                    {
                      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                      TitleAutoPrefix = false,
                      AllowCancellation = false,
                      MainInstruction = "License Code Expired. ",
                      MainContent = "License Code is Expired. Please try again with another license code or contact EDGE Support."
                    }.Show();
                    continue;
                  }
                  if ((dateTime1.Date - onlineTime.Date).Days < 30 && (dateTime1.Date - onlineTime.Date).Days > 5 && !flag5)
                  {
                    TaskDialog taskDialog = new TaskDialog("EDGE^R - License Days Remaining");
                    taskDialog.Id = "ID_License_Days_Remaining";
                    taskDialog.Title = "EDGE^R - License Days Remaining";
                    taskDialog.TitleAutoPrefix = false;
                    taskDialog.AllowCancellation = true;
                    taskDialog.MainInstruction = "License Days Remaining";
                    taskDialog.MainContent = $"There are {(dateTime1.Date - onlineTime.Date).Days.ToString()} days remaining on your EDGE^R License";
                    taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Postpone Notification");
                    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1002, "Request New License");
                    taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
                    taskDialog.DefaultButton = (TaskDialogResult) 1;
                    TaskDialogResult taskDialogResult = taskDialog.Show();
                    if (taskDialogResult == 1001)
                      LicenseHelper.PostPoneMessage(str1);
                    else if (taskDialogResult == 1002)
                    {
                      string eulaGuid = Guid.NewGuid().ToString();
                      if (LicenseHelper.AcceptEULA(eulaGuid))
                        LicenseHelper.RequestNewLicense(LicenseHelper.GetOnlineTime().Date.ToString("MM/dd/yyyy"), eulaGuid);
                    }
                  }
                  else if ((dateTime1.Date - onlineTime.Date).Days <= 5)
                  {
                    TaskDialog taskDialog = new TaskDialog("EDGE^R - License Days Remaining");
                    taskDialog.Id = "ID_License_Days_Remaining";
                    taskDialog.Title = "EDGE^R - License Days Remaining";
                    taskDialog.TitleAutoPrefix = false;
                    taskDialog.AllowCancellation = true;
                    taskDialog.MainInstruction = "License Days Remaining";
                    taskDialog.MainContent = $"There are {(dateTime1.Date - onlineTime.Date).Days.ToString()} day(s) remaining on your EDGE^R License";
                    taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Request New License");
                    taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
                    taskDialog.DefaultButton = (TaskDialogResult) 1;
                    if (taskDialog.Show() == 1001)
                    {
                      string eulaGuid = Guid.NewGuid().ToString();
                      if (LicenseHelper.AcceptEULA(eulaGuid))
                        LicenseHelper.RequestNewLicense(LicenseHelper.GetOnlineTime().Date.ToString("MM/dd/yyyy"), eulaGuid);
                    }
                  }
                  else if ((dateTime1.Date - onlineTime.Date).Days == 0)
                  {
                    TaskDialog taskDialog = new TaskDialog("EDGE^R - License Days Remaining");
                    taskDialog.Id = "ID_License_Days_Remaining";
                    taskDialog.Title = "EDGE^R - License Days Remaining";
                    taskDialog.TitleAutoPrefix = false;
                    taskDialog.AllowCancellation = true;
                    taskDialog.MainInstruction = "License Days Remaining";
                    taskDialog.MainContent = "There are 0 days remaining on your EDGE^R License.";
                    taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
                    taskDialog.AddCommandLink((TaskDialogCommandLinkId) 1001, "Request New License");
                    taskDialog.CommonButtons = (TaskDialogCommonButtons) 1;
                    taskDialog.DefaultButton = (TaskDialogResult) 1;
                    if (taskDialog.Show() == 1001)
                    {
                      string eulaGuid = Guid.NewGuid().ToString();
                      if (LicenseHelper.AcceptEULA(eulaGuid))
                        LicenseHelper.RequestNewLicense(LicenseHelper.GetOnlineTime().Date.ToString("MM/dd/yyyy"), eulaGuid);
                    }
                  }
                }
                if (s1 != null && s1 != "")
                {
                  DateTime dateTime = DateTime.Parse(s1);
                  DateTime onlineTime = LicenseHelper.GetOnlineTime();
                  if ((dateTime.Date - onlineTime.Date).Days < 7)
                    num1 = (dateTime - onlineTime).Days;
                }
              }
              else
              {
                if (s1 != null && s1 != "")
                {
                  DateTime dateTime3 = DateTime.Parse(s1);
                  DateTime dateTime4 = new DateTime();
                  dateTime4 = DateTime.Parse("2025.06.10");
                  DateTime dateTime5 = new DateTime();
                  if (s2 != null && s2 != "")
                    dateTime5 = DateTime.Parse(s2);
                  flag3 = dateTime5.Date > dateTime4.Date;
                  if (dateTime3.Date < dateTime4.Date)
                  {
                    new TaskDialog("License Code Expired")
                    {
                      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
                      TitleAutoPrefix = false,
                      AllowCancellation = false,
                      MainInstruction = "License Code Expired. ",
                      MainContent = "License Code is Expired. Please try again with another license code or contact EDGE Support."
                    }.Show();
                    continue;
                  }
                }
                if (s1 != null && s1 != "")
                {
                  DateTime dateTime6 = DateTime.Parse(s1);
                  DateTime dateTime7 = new DateTime();
                  DateTime dateTime8 = DateTime.Parse("2025.06.10");
                  TimeSpan timeSpan = dateTime6.Date - dateTime8.Date;
                  if (timeSpan.Days < 7)
                  {
                    timeSpan = dateTime6.Date - dateTime8.Date;
                    num1 = timeSpan.Days;
                  }
                }
              }
              LicenseHelper.SaveTempLic(LicenseHelper.GetOnlineTime().AddDays((double) num1), LicenseHelper.GetOnlineTime());
              return true;
            }
            new TaskDialog("License Code Not Valid")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              TitleAutoPrefix = false,
              AllowCancellation = false,
              MainInstruction = "License Code Not Valid.",
              MainContent = "License Code is not valid. Request a valid license from EDGE Support or try again with a valid license."
            }.Show();
          }
          catch (Exception ex)
          {
            int num4 = (int) MessageBox.Show(ex.Message);
            return false;
          }
        }
        else
        {
          new TaskDialog("No Internet Connection")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            TitleAutoPrefix = false,
            AllowCancellation = false,
            MainInstruction = "No Internet Connection",
            MainContent = "Unable to connect to the internet to validate license code against our database. Please reconnect to the internet and try again."
          }.Show();
          return false;
        }
      }
      return true;
    }
    new TaskDialog("No Internet Connection")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      TitleAutoPrefix = false,
      AllowCancellation = false,
      MainInstruction = "No Internet Connection",
      MainContent = "Unable to connect to the internet to validate license code against our database. Please reconnect to the internet and try again."
    }.Show();
    return false;
  }

  public static void RequestNewLicense(string eulaDate, string eulaGuid)
  {
    CryptoLicense cryptoLicense = new CryptoLicense(LicenseStorageMode.ToFile, $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic", LicenseHelper.LicenseKey);
    string machineCodeAsString = cryptoLicense.GetLocalMachineCodeAsString();
    string machineName = Environment.MachineName;
    string str1 = "EDGE^R " + LicenseHelper.ProductYear;
    string name1 = WindowsIdentity.GetCurrent().Name;
    string str2 = "";
    LicenseRequestForm licenseRequestForm = new LicenseRequestForm();
    int num1 = (int) licenseRequestForm.ShowDialog();
    if (licenseRequestForm.cancelled)
      return;
    string name2 = licenseRequestForm.name;
    string companyName = licenseRequestForm.companyName;
    string phone = licenseRequestForm.phone;
    if (cryptoLicense.Load())
    {
      string licenseCode = cryptoLicense.LicenseCode;
      if (System.IO.File.Exists($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic"))
      {
        string str3 = System.IO.File.ReadLines($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic").ElementAtOrDefault<string>(1);
        string str4 = System.IO.File.ReadLines($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic").Last<string>();
        LicenseRequestInfoForm licenseRequestInfoForm = new LicenseRequestInfoForm($" MailTo: edge@PTAC.com \n Subject: New License Request \n\n Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n {str3}\n {str4}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n License Key: {licenseCode}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. \n NOTE: license will be locked to this machine code and username.");
        Process.Start(Uri.EscapeUriString($"mailto:edge@ptac.com?subject=Request License EDGE^R&body= Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n EULA Accepted Date: {eulaDate}\n {eulaGuid}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n License Key: {licenseCode}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. NOTE: license will be locked to this machine code and username."));
        int num2 = (int) licenseRequestInfoForm.ShowDialog();
      }
      else
      {
        LicenseRequestInfoForm licenseRequestInfoForm = new LicenseRequestInfoForm($" MailTo: edge@PTAC.com \n Subject: New License Request \n\n Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n EULA Accepted Date: {eulaDate}\n {eulaGuid}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n License Key: {licenseCode}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. \n NOTE: license will be locked to this machine code and username.");
        Process.Start(Uri.EscapeUriString($"mailto:edge@ptac.com?subject=Request License EDGE^R&body= Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n EULA Accepted Date: {eulaDate}\n {eulaGuid}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n License Key: {licenseCode}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. NOTE: license will be locked to this machine code and username."));
        int num3 = (int) licenseRequestInfoForm.ShowDialog();
      }
    }
    else if (System.IO.File.Exists($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic"))
    {
      string str5 = System.IO.File.ReadLines($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic").ElementAtOrDefault<string>(1);
      string str6 = System.IO.File.ReadLines($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic").Last<string>();
      LicenseRequestInfoForm licenseRequestInfoForm = new LicenseRequestInfoForm($" MailTo: edge@PTAC.com \n Subject: New License Request \n\n Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n {str5}\n {str6}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n License Key: {str2}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. \n NOTE: license will be locked to this machine code and username.");
      Process.Start(Uri.EscapeUriString($"mailto:edge@ptac.com?subject=Request License EDGE^R&body= Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n {str5}\n {str6}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. NOTE: license will be locked to this machine code and username."));
      int num4 = (int) licenseRequestInfoForm.ShowDialog();
    }
    else
    {
      LicenseRequestInfoForm licenseRequestInfoForm = new LicenseRequestInfoForm($" MailTo: edge@PTAC.com \n Subject: New License Request \n\n Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n EULA Accepted Date: {eulaDate}\n {eulaGuid}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n License Key: {str2}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. \n NOTE: license will be locked to this machine code and username.");
      Process.Start(Uri.EscapeUriString($"mailto:edge@ptac.com?subject=Request License EDGE^R&body= Customer: {name2}\n Company Name: {companyName}\n Phone Number: {phone}\n EULA Accepted Date: {eulaDate}\n {eulaGuid}\n Machine Code: {machineCodeAsString}\n Machine Name: {machineName}\n {str1}\n User: {name1}\n I would like to request a new license or reactivate an old license for EDGE^R. NOTE: license will be locked to this machine code and username."));
      int num5 = (int) licenseRequestInfoForm.ShowDialog();
    }
  }

  public static bool CheckInternetConnection() => Util.CheckInternetConnection();

  public static void ShowHelpFile()
  {
    try
    {
      Process.Start(LicenseHelper.fullHelpPath);
    }
    catch (Win32Exception ex)
    {
      if (ex.ErrorCode != -2147467259 /*0x80004005*/)
        return;
      int num = (int) MessageBox.Show(ex.Message);
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show($"{ex.Message} location: {LicenseHelper.fullHelpPath}");
    }
  }

  public static bool AcceptEULA(string eulaGuid)
  {
    bool flag = LicenseHelper.CheckInternetConnection();
    CryptoLicense cryptoLicense = new CryptoLicense(LicenseStorageMode.ToFile, $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic", LicenseHelper.LicenseKey);
    if (System.IO.File.Exists($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic"))
      return true;
    if (flag)
    {
      string machineCodeAsString = cryptoLicense.GetLocalMachineCodeAsString();
      string name = WindowsIdentity.GetCurrent().Name;
      string machineName = Environment.MachineName;
      string str1 = "EDGE^R " + LicenseHelper.ProductYear;
      if (cryptoLicense.Load())
      {
        string licenseCode = cryptoLicense.LicenseCode;
      }
      if (!System.IO.File.Exists($"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\Enduser_License_Agreement.rtf"))
      {
        new TaskDialog("End User Licensing Agreement File Not Found")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          TitleAutoPrefix = false,
          AllowCancellation = false,
          MainInstruction = "End User Licensing Agreement File Not Found",
          MainContent = "End User Licensing Agreement file has been deleted or removed from its original location. Please contact EDGE support."
        }.Show();
        return false;
      }
      EULAForm eulaForm = new EULAForm();
      int num1 = (int) eulaForm.ShowDialog();
      int num2 = eulaForm.accepted ? 1 : 0;
      string str2 = DateTime.Now.Date.ToString("MM/dd/yyyy");
      if (num2 == 0)
      {
        new TaskDialog("End User Licensing Agreement Not Accepted")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          TitleAutoPrefix = false,
          AllowCancellation = false,
          MainInstruction = "End User Licensing Agreement Not Accepted",
          MainContent = "End User Licensing Agreement must be accepted before running EDGE^R or requesting a license."
        }.Show();
        return false;
      }
      string[] contents = new string[4]
      {
        machineCodeAsString,
        $"EULA Accepted {str2} User {name}",
        machineName,
        eulaGuid
      };
      try
      {
        System.IO.File.WriteAllLines($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit_config.lic", contents);
      }
      catch
      {
        new TaskDialog("Unable to Save License")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          TitleAutoPrefix = false,
          AllowCancellation = false,
          MainInstruction = "Unable to Save License",
          MainContent = "Unable to save license. Please contact your administrator or EDGE support for assistance."
        }.Show();
        return false;
      }
      return true;
    }
    new TaskDialog("No Internet Connection")
    {
      FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
      TitleAutoPrefix = false,
      AllowCancellation = false,
      MainInstruction = "No Internet Connection",
      MainContent = "Unable to connect to the internet to validate EULA Activation against our database. Please reconnect to the internet and try again."
    }.Show();
    return false;
  }

  public static void SaveEDGEBuild(string licenseCode)
  {
    string machineCodeAsString = new CryptoLicense(LicenseStorageMode.ToFile, $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic", LicenseHelper.LicenseKey).GetLocalMachineCodeAsString();
    string machineName = Environment.MachineName;
    string name = WindowsIdentity.GetCurrent().Name;
    string product = "EdgeForRevit";
    string str = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    if (!LicenseHelper.CheckInternetConnection())
      return;
    try
    {
      RestClient restClient = new RestClient("http://edgelicensingservice.azurewebsites.net");
      RestRequest restRequest = new RestRequest("/api/updatebuild", Method.POST)
      {
        RequestFormat = DataFormat.Json
      };
      restRequest.AddHeader("content-type", "application/json");
      restRequest.AddJsonBody((object) new EDGELicenseModel(machineCodeAsString, name, product, licenseCode, LicenseHelper.ProductYear, machineName)
      {
        EdgeBuild = str
      });
      RestRequest request = restRequest;
      restClient.Execute((IRestRequest) request);
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.Message);
    }
  }

  public static void PostPoneMessage(string licenseCode)
  {
    string machineCodeAsString = new CryptoLicense(LicenseStorageMode.ToFile, $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic", LicenseHelper.LicenseKey).GetLocalMachineCodeAsString();
    string machineName = Environment.MachineName;
    string name = WindowsIdentity.GetCurrent().Name;
    string product = "EdgeForRevit";
    DateTime onlineTime = LicenseHelper.GetOnlineTime();
    LicensePostponeForm licensePostponeForm = new LicensePostponeForm();
    int num1 = (int) licensePostponeForm.ShowDialog();
    if (licensePostponeForm.cancelled)
      return;
    int postPoneDays = licensePostponeForm.PostPoneDays;
    string s = onlineTime.AddDays((double) postPoneDays).Date.ToString("yyyy-MM-dd");
    bool flag = true;
    if (!LicenseHelper.CheckInternetConnection())
      return;
    try
    {
      RestClient restClient = new RestClient("http://edgelicensingservice.azurewebsites.net");
      RestRequest restRequest = new RestRequest("/api/postponelicense", Method.POST)
      {
        RequestFormat = DataFormat.Json
      };
      restRequest.AddHeader("content-type", "application/json");
      restRequest.AddJsonBody((object) new EDGELicenseModel(machineCodeAsString, name, product, licenseCode, LicenseHelper.ProductYear, machineName)
      {
        Postpone = flag,
        PostponeDate = DateTime.Parse(s)
      });
      RestRequest request = restRequest;
      restClient.Execute((IRestRequest) request);
    }
    catch (Exception ex)
    {
      int num2 = (int) MessageBox.Show(ex.Message);
    }
  }

  public static void WriteToFields(string licenseCode)
  {
    string machineCodeAsString = new CryptoLicense(LicenseStorageMode.ToFile, $"c:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevit.lic", LicenseHelper.LicenseKey).GetLocalMachineCodeAsString();
    string machineName = Environment.MachineName;
    string name = WindowsIdentity.GetCurrent().Name;
    string product = "EdgeForRevit";
    if (!LicenseHelper.CheckInternetConnection())
      return;
    try
    {
      RestClient restClient = new RestClient("http://edgelicensingservice.azurewebsites.net");
      RestRequest restRequest = new RestRequest("/api/updatelicense", Method.POST)
      {
        RequestFormat = DataFormat.Json
      };
      restRequest.AddHeader("content-type", "application/json");
      EDGELicenseModel edgeLicenseModel = new EDGELicenseModel(machineCodeAsString, name, product, licenseCode, LicenseHelper.ProductYear, machineName);
      restRequest.AddJsonBody((object) edgeLicenseModel);
      RestRequest request = restRequest;
      SimpleJson.DeserializeObject(restClient.Execute((IRestRequest) request).Content);
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.Message);
    }
  }

  public static DateTime GetOnlineTime()
  {
    if (!LicenseHelper.CheckInternetConnection())
      return DateTime.Now;
    WebResponse response = WebRequest.Create("http://edgelicensingservice.azurewebsites.net").GetResponse();
    string header = response.Headers["date"];
    response.Close();
    return DateTime.Parse(header);
  }

  public static void SaveTempLic(DateTime expDate, DateTime currTime)
  {
    string text = $"{expDate.Date.ToString("MM/dd/yyyy")} {currTime.ToString()}";
    LicenseHelper.aes.KeySize = 128 /*0x80*/;
    LicenseHelper.aes.Padding = PaddingMode.PKCS7;
    LicenseHelper.aes.Mode = CipherMode.CBC;
    using (LicenseHelper.aes)
    {
      LicenseHelper.aes.Key = new byte[16 /*0x10*/]
      {
        (byte) 0,
        (byte) 1,
        (byte) 2,
        (byte) 3,
        (byte) 4,
        (byte) 5,
        (byte) 6,
        (byte) 7,
        (byte) 8,
        (byte) 9,
        (byte) 10,
        (byte) 11,
        (byte) 12,
        (byte) 13,
        (byte) 14,
        (byte) 15
      };
      LicenseHelper.aes.IV = new byte[16 /*0x10*/]
      {
        (byte) 0,
        (byte) 1,
        (byte) 2,
        (byte) 3,
        (byte) 4,
        (byte) 5,
        (byte) 6,
        (byte) 7,
        (byte) 8,
        (byte) 9,
        (byte) 10,
        (byte) 11,
        (byte) 12,
        (byte) 13,
        (byte) 14,
        (byte) 15
      };
      byte[] bytes = LicenseHelper.EncryptString(text, LicenseHelper.aes.Key, LicenseHelper.aes.IV);
      try
      {
        System.IO.File.WriteAllBytes($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevitTemp.txt", bytes);
      }
      catch
      {
        new TaskDialog("Unable to Save License")
        {
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
          TitleAutoPrefix = false,
          AllowCancellation = false,
          MainInstruction = "Unable to Save License",
          MainContent = "Unable to save license. Please contact your administrator or EDGE support for assistance."
        }.Show();
      }
    }
  }

  public static string LoadTempLic()
  {
    if (!System.IO.File.Exists($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevitTemp.txt"))
      return "";
    LicenseHelper.aes.KeySize = 128 /*0x80*/;
    LicenseHelper.aes.Padding = PaddingMode.PKCS7;
    LicenseHelper.aes.Mode = CipherMode.CBC;
    LicenseHelper.aes.Key = new byte[16 /*0x10*/]
    {
      (byte) 0,
      (byte) 1,
      (byte) 2,
      (byte) 3,
      (byte) 4,
      (byte) 5,
      (byte) 6,
      (byte) 7,
      (byte) 8,
      (byte) 9,
      (byte) 10,
      (byte) 11,
      (byte) 12,
      (byte) 13,
      (byte) 14,
      (byte) 15
    };
    LicenseHelper.aes.IV = new byte[16 /*0x10*/]
    {
      (byte) 0,
      (byte) 1,
      (byte) 2,
      (byte) 3,
      (byte) 4,
      (byte) 5,
      (byte) 6,
      (byte) 7,
      (byte) 8,
      (byte) 9,
      (byte) 10,
      (byte) 11,
      (byte) 12,
      (byte) 13,
      (byte) 14,
      (byte) 15
    };
    byte[] textArr = System.IO.File.ReadAllBytes($"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{LicenseHelper.ProductYear}\\PTAC_EDGE_BUNDLE\\EDGEforRevitTemp.txt");
    using (LicenseHelper.aes)
      return LicenseHelper.DecryptString(textArr, LicenseHelper.aes.Key, LicenseHelper.aes.IV);
  }

  private static byte[] EncryptString(string text, byte[] Key, byte[] IV)
  {
    byte[] bytes = Encoding.Unicode.GetBytes(text);
    MemoryStream memoryStream = new MemoryStream();
    Rijndael rijndael = Rijndael.Create();
    rijndael.Key = Key;
    rijndael.IV = IV;
    CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write);
    cryptoStream.Write(bytes, 0, bytes.Length);
    cryptoStream.Close();
    return memoryStream.ToArray();
  }

  private static string DecryptString(byte[] textArr, byte[] Key, byte[] IV)
  {
    MemoryStream memoryStream = new MemoryStream();
    Rijndael rijndael = Rijndael.Create();
    rijndael.Key = Key;
    rijndael.IV = IV;
    CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write);
    cryptoStream.Write(textArr, 0, textArr.Length);
    cryptoStream.Close();
    return Encoding.Unicode.GetString(memoryStream.ToArray());
  }
}
