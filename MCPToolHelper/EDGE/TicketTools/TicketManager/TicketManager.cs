// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.TicketManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Utils.Exceptions;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TicketManager : IExternalCommand
{
  private Document doc;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    this.doc = activeUiDocument.Document;
    if (this.doc.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Ticket Manager must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Ticket Manager must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      return (Result) 1;
    }
    string str1 = App.TMCFolderPath;
    if (str1.Equals(""))
      str1 = "C:\\EDGEforRevit";
    string str2 = "";
    Parameter parameter = this.doc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
      str2 = parameter.AsString();
    string str3 = "";
    if (!str2.Equals("") || str2 != null)
    {
      if (File.Exists($"{str1}\\{str2}_TicketManagerCustomizationSettings.txt"))
      {
        int num = new FileInfo($"{str1}\\{str2}_TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
        str3 = $"{str1}\\{str2}_TicketManagerCustomizationSettings.txt";
      }
      else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
      {
        int num = new FileInfo(str1 + "\\TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
        str3 = str1 + "\\TicketManagerCustomizationSettings.txt";
      }
    }
    else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
    {
      int num = new FileInfo(str1 + "\\TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
      str3 = str1 + "\\TicketManagerCustomizationSettings.txt";
    }
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    try
    {
      if (!str2.Equals("") || str2 != null)
      {
        if (File.Exists($"{str1}\\{str2}_TicketManagerCustomizationSettings.txt"))
        {
          foreach (string readAllLine in File.ReadAllLines($"{str1}\\{str2}_TicketManagerCustomizationSettings.txt"))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray)[1].Split('|');
            stringList1.Add(strArray[0].Trim());
            stringList2.Add(strArray[1].Trim());
          }
        }
        else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
        {
          foreach (string readAllLine in File.ReadAllLines(str1 + "\\TicketManagerCustomizationSettings.txt"))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray)[1].Split('|');
            stringList1.Add(strArray[0].Trim());
            stringList2.Add(strArray[1].Trim());
          }
        }
      }
      else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines(str1 + "\\TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          stringList1.Add(strArray[0].Trim());
          stringList2.Add(strArray[1].Trim());
        }
      }
    }
    catch
    {
      new TaskDialog("Ticket Manager Settings File Error")
      {
        MainInstruction = "Unable to read in Ticket Manager Settings File.",
        MainContent = $"Unable to read in the Ticket Manager Settings File available at {str3}. Please ensure the file is available to be read and in the valid format and try again."
      }.Show();
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(this.doc, "Ticket Manager"))
    {
      try
      {
        App.DialogSwitches.SuspendModelLockingforOperation = true;
        if (App.TicketManagerWindow != null)
        {
          if (App.TicketManagerWindow.Activate())
          {
            if (App.TicketManagerWindow.WindowState == WindowState.Minimized)
              App.TicketManagerWindow.WindowState = WindowState.Normal;
            if (!App.TicketManagerWindow.Focusable)
            {
              App.TicketManagerWindow.Close();
              this.initializeWindow(application);
            }
          }
          else
            this.initializeWindow(application);
        }
        else
          this.initializeWindow(application);
        App.TicketManagerWindow.Show();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        if (ex is EdgeException)
        {
          message = ex.Message;
          return (Result) -1;
        }
        message = ex.ToString();
        return (Result) -1;
      }
      finally
      {
        App.DialogSwitches.SuspendModelLockingforOperation = false;
      }
    }
  }

  public void initializeWindow(UIApplication application)
  {
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    App.TicketManagerWindow = new MainWindow(new ExternalEvent[5]
    {
      ExternalEvent.Create((IExternalEventHandler) new AcceptChangesEvent()),
      ExternalEvent.Create((IExternalEventHandler) new ReleaseTicketEvent()),
      ExternalEvent.Create((IExternalEventHandler) new WriteCommentEvent()),
      ExternalEvent.Create((IExternalEventHandler) new TicketPopulatorEvent()),
      ExternalEvent.Create((IExternalEventHandler) new UpdateParametersEvent())
    }, application, mainWindowHandle);
  }
}
