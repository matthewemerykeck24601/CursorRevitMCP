// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.TicketManagerExport
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class TicketManagerExport : IExternalCommand
{
  private Document doc;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIApplication application = commandData.Application;
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    this.doc = activeUiDocument.Document;
    if (activeUiDocument.Application.ActiveUIDocument.Document.IsFamilyDocument)
    {
      TaskDialog.Show("Ticket Export", "Ticket Export cannot be run from a family view. Please exit the family view and try running the Ticket Export again.");
      return (Result) 1;
    }
    List<string> stringList = new List<string>();
    string filePath = string.Empty;
    using (new Transaction(this.doc, "Ticket Manager Export"))
    {
      try
      {
        List<string> fromExistingFile = this.GetExportDataFromExistingFile(out filePath);
        string errMessage;
        if (new MainViewModel().TicketExport(activeUiDocument, fromExistingFile, filePath, out errMessage))
        {
          TaskDialog.Show("Ticket Export", "Export completed successfully");
          return (Result) 0;
        }
        TaskDialog.Show("Ticket Export", errMessage);
        return (Result) 1;
      }
      catch (DirectoryNotFoundException ex)
      {
        TaskDialog.Show("Data Extraction Exception", ex.Message);
        return (Result) 1;
      }
      catch (Exception ex)
      {
        if (ex.Message.ToUpper().Contains("PARAMETER") && ex.Message.ToUpper().Contains("CONTENTS") || ex.Message.ToUpper().Contains("PATH") && ex.Message.ToUpper().Contains("FORM"))
        {
          if (!ex.Message.ToUpper().Contains("UNABLE TO DETERMINE IF THE EXPORT FILE PATH IS ACCESSIBLE."))
            TaskDialog.Show("Export Exception", "The export file path cannot be empty.\nPlease verify that the export path is correct.");
          else
            TaskDialog.Show("Data Extraction Exception", ex.Message);
        }
        else
          TaskDialog.Show("Data Extraction Exception", ex.Message);
        return (Result) 1;
      }
    }
  }

  private List<string> GetExportDataFromExistingFile(out string filePath)
  {
    List<string> fromExistingFile = new List<string>();
    int num = 0;
    string empty1 = string.Empty;
    filePath = App.TicketManagerExportFile;
    try
    {
      if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
    }
    catch
    {
    }
    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
      throw new Exception("Unable to determine if the export file path is accessible.\nPlease check that the path name is accessible and formatted correctly.");
    if (!File.Exists(filePath))
      return (List<string>) null;
    string s = File.ReadAllText(filePath);
    string str1 = Path.GetDirectoryName(filePath) + "\\archive\\";
    if (!File.Exists(str1))
      Directory.CreateDirectory(str1);
    StringReader stringReader = new StringReader(s);
    string str2 = $"export_{DateTime.Now.ToString("ss_fffffffK")}.csv".Replace("-", "_").Replace(":", "_");
    File.Copy(filePath, str1 + str2);
    this.ArchiveMaintenance(str1);
    string empty2 = string.Empty;
    string str3;
    while ((str3 = stringReader.ReadLine()) != null)
    {
      if (num > 0)
        fromExistingFile.Add(str3);
      ++num;
    }
    return fromExistingFile;
  }

  private void ArchiveMaintenance(string archivePath)
  {
    foreach (FileSystemInfo fileSystemInfo in ((IEnumerable<FileInfo>) new DirectoryInfo(archivePath).GetFiles()).OrderByDescending<FileInfo, DateTime>((Func<FileInfo, DateTime>) (x => x.LastWriteTime)).Skip<FileInfo>(5))
      fileSystemInfo.Delete();
  }

  private List<string> MaintainExportData(List<string> existingData)
  {
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    string empty1 = string.Empty;
    string empty2 = string.Empty;
    DateTime newest = DateTime.Today.AddDays(-365.0);
    List<string> stringList3 = new List<string>();
    int num = 0;
    foreach (string str in existingData)
    {
      List<string> list = ((IEnumerable<string>) str.Split(',')).ToList<string>();
      if (list.Count > 1)
      {
        if ((empty1 != list[0] || empty2 != list[1]) && num > 0)
        {
          stringList2.Add($"{empty1}|{empty2}|{(DateTime.Today - newest).TotalDays.ToString()}");
          newest = DateTime.Today.AddDays(-365.0);
        }
        empty1 = list[0];
        empty2 = list[1];
        newest = this.CheckDate(list[4], list[6], list[8], list[10], newest);
      }
      stringList1.Add(str);
      ++num;
    }
    stringList2.Add($"{empty1}|{empty2}|{(DateTime.Today - newest).TotalDays.ToString()}");
    return stringList1;
  }

  private DateTime CheckDate(
    string date1,
    string date2,
    string date3,
    string date4,
    DateTime newest)
  {
    if (!string.IsNullOrEmpty(date1) && DateTime.Parse(date1) > newest)
      newest = DateTime.Parse(date1);
    if (!string.IsNullOrEmpty(date2) && DateTime.Parse(date2) > newest)
      newest = DateTime.Parse(date2);
    if (!string.IsNullOrEmpty(date3) && DateTime.Parse(date3) > newest)
      newest = DateTime.Parse(date3);
    if (!string.IsNullOrEmpty(date4) && DateTime.Parse(date4) > newest)
      newest = DateTime.Parse(date4);
    return newest;
  }

  private List<string> DetermineNewest(List<string> projectList, List<string> dataSet)
  {
    List<string> newest = new List<string>();
    List<string> stringList = new List<string>();
    string empty1 = string.Empty;
    string empty2 = string.Empty;
    int num1 = 0;
    bool flag = true;
    foreach (string project in projectList)
    {
      num1 = 0;
      char[] chArray = new char[1]{ '|' };
      List<string> list = ((IEnumerable<string>) project.Split(chArray)).ToList<string>();
      string name = list[0];
      string num2 = list[1];
      if (int.Parse(list[2]) >= 22)
      {
        int num3 = this.HasStrikes(name, num2);
        if (num3 < 2)
        {
          this.AddStrikes(name, num2, num3 + 1);
        }
        else
        {
          flag = false;
          this.AddStrikes(name, num2, num3 + 1);
        }
      }
      else if (this.HasStrikes(name, num2) > 0)
        this.AddStrikes(name, num2, 99);
      if (flag)
      {
        foreach (string data in dataSet)
        {
          if (data.Split(',')[0] == name)
          {
            if (data.Split(',')[1] == num2)
              newest.Add(data);
          }
        }
      }
    }
    return newest;
  }

  private int HasStrikes(string name, string num)
  {
    int num1 = 0;
    string empty1 = string.Empty;
    string path = Path.GetDirectoryName(App.TicketManagerExportFile) + "\\strikes.txt";
    if (File.Exists(path))
    {
      StringReader stringReader = new StringReader(File.ReadAllText(path));
      string empty2 = string.Empty;
      string str;
      while ((str = stringReader.ReadLine()) != null)
      {
        if (str.Split(',')[0] == name)
        {
          if (str.Split(',')[1] == num)
          {
            num1 = int.Parse(str.Split(',')[2]);
            break;
          }
        }
      }
    }
    return num1;
  }

  private void AddStrikes(string name, string num, int strikes)
  {
    string path = Path.GetDirectoryName(App.TicketManagerExportFile) + "\\strikes.txt";
    string empty1 = string.Empty;
    StringBuilder stringBuilder = new StringBuilder();
    bool flag = false;
    string str1 = $"{name},{num},{strikes.ToString()},{DateTime.Today.ToString()}";
    if (!File.Exists(path))
    {
      stringBuilder.Append(str1);
    }
    else
    {
      string s = File.ReadAllText(path);
      string empty2 = string.Empty;
      StringReader stringReader = new StringReader(s);
      string str2;
      while ((str2 = stringReader.ReadLine()) != null)
      {
        if (!string.IsNullOrEmpty(str2))
        {
          if (str2.Contains(name) && str2.Contains(num))
          {
            flag = true;
            if (strikes != 99)
            {
              if ((DateTime.Today - DateTime.Parse(str2.Split(',')[3])).TotalDays > 0.0 && strikes <= 2)
                stringBuilder.AppendLine(str1);
              else if ((DateTime.Today - DateTime.Parse(str2.Split(',')[3])).TotalDays == 0.0)
                stringBuilder.AppendLine(str2);
            }
          }
          else if (!string.IsNullOrEmpty(str2))
            stringBuilder.AppendLine(str2);
        }
      }
    }
    if (!flag && strikes <= 2)
      stringBuilder.Append(str1);
    using (StreamWriter streamWriter = new StreamWriter(path))
      streamWriter.WriteLine(stringBuilder.ToString());
  }
}
