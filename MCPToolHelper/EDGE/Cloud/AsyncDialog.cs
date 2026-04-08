// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.AsyncDialog
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#nullable disable
namespace EDGE.Cloud;

internal class AsyncDialog
{
  public Action<bool> LockControls;
  private int _count;
  private int _steps;
  private List<string> _names;
  private List<string> _infoNames;
  private List<string> _infoMessages;
  private List<string> _errorNames;
  private List<string> _errorMessages;

  public static bool Done { get; set; }

  public static bool Error { get; set; }

  public static bool Cancel { get; set; }

  public AssemblyInfo AssemblyInfo { get; set; }

  public Form Form { get; set; }

  public List<string> NonAssemblies { get; set; }

  public string WebUrl { get; set; }

  public AsyncDialog(int count)
  {
    this._count = count;
    this._names = new List<string>();
    this._infoNames = new List<string>();
    this._infoMessages = new List<string>();
    this._errorNames = new List<string>();
    this._errorMessages = new List<string>();
    this.NonAssemblies = new List<string>();
    AsyncDialog.Error = false;
    AsyncDialog.Cancel = false;
  }

  public void AddStep()
  {
    ++this._steps;
    if (this._steps != this._count)
      return;
    this.ShowFinalResult();
  }

  public void Add(string name, bool isError = false)
  {
    this._names.Add(name);
    if (isError)
      this._errorNames.Add(name);
    else
      this._infoNames.Add(name);
    if (this._names.Count == this._count)
      this.ShowFinalResult();
    this.PurgeUsedMemory();
  }

  public void AddInfo(string message) => this._infoMessages.Add(message);

  public void AddError(string message) => this._errorMessages.Add(message);

  private static List<string> Sort(List<string> list)
  {
    List<string> stringList = new List<string>();
    list.Sort();
    Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
    string str1 = string.Empty;
    foreach (string key1 in list)
    {
      int length = key1.IndexOf("-");
      if (length == 0)
      {
        stringList.Add(key1);
      }
      else
      {
        string key2 = key1.Substring(0, length);
        string str2 = key1.Substring(length + 1);
        if (Regex.IsMatch(str2, "^\\d+$"))
        {
          int.Parse(str2);
          if (!dictionary.ContainsKey(key2))
            dictionary.Add(key1, new List<string>() { str2 });
          else if (!(key2 != str1))
            dictionary[key1].Add(key1);
          str1 = key2;
        }
        else
          stringList.Add(key1);
      }
    }
    return stringList;
  }

  private void ShowFinalResult()
  {
    string empty = string.Empty;
    string str1 = string.Empty;
    string str2 = string.Empty;
    TaskDialog taskDialog = new TaskDialog("EDGE^Cloud Exporter");
    if (this.Form != null)
    {
      AsyncDialog.Done = true;
      this.Form.Close();
    }
    this.NonAssemblies.Sort();
    foreach (string nonAssembly in this.NonAssemblies)
      str1 = $"{str1}{nonAssembly}\n";
    if (!string.IsNullOrEmpty(str1))
      str1 = "\nFound non-assemblies were not exported:\n" + str1;
    if (this._names.Count == 1)
    {
      string str3 = this._errorNames.Count == 0 ? $"{this._names[0]} sent to EDGE Cloud successfully at {this.WebUrl}" : $"{this._errorNames[0]} failed to export to EDGE Cloud";
      taskDialog.MainInstruction = str3;
      taskDialog.ExpandedContent = str1;
    }
    else if (this._names.Count > 1)
    {
      string str4 = string.Empty;
      this._names.Sort((IComparer<string>) new NaturalComparer());
      this._infoNames.Sort((IComparer<string>) new NaturalComparer());
      this._errorNames.Sort((IComparer<string>) new NaturalComparer());
      foreach (string infoName in this._infoNames)
        str4 = $"{str4}{infoName}\n";
      if (this._errorNames.Count > 0)
      {
        str2 = "\nThe folowing assemblies failed to send to EDGE Cloud:\n";
        foreach (string errorName in this._errorNames)
          str2 = $"{str2}{errorName}\n";
      }
      string str5 = this._infoNames.Count == 0 ? "No assembly sent to EDGE Cloud. Please check and try again." : $"The following assemblies were sent to EDGE Cloud successfully at {this.WebUrl}";
      taskDialog.MainInstruction = str5;
      taskDialog.ExpandedContent = str4 + str2 + str1;
    }
    if (this._infoMessages.Count > 0)
    {
      this._infoMessages.Sort();
      this._infoMessages = this._infoMessages.Distinct<string>().ToList<string>();
      string str6 = "\n";
      foreach (string errorMessage in this._errorMessages)
        str6 = $"{str6}{errorMessage}\n";
      taskDialog.ExpandedContent += str6;
    }
    if (this._errorMessages.Count > 0)
    {
      this._errorMessages.Sort();
      this._errorMessages = this._errorMessages.Distinct<string>().ToList<string>();
      string str7 = "\nFound errors:\n";
      foreach (string errorMessage in this._errorMessages)
        str7 = $"{str7}{errorMessage}\n";
      if (string.IsNullOrEmpty(taskDialog.MainInstruction))
        taskDialog.MainInstruction = string.Format("Errors happened when exporting to EDGE Cloud");
      taskDialog.ExpandedContent += str7;
    }
    this._names.Clear();
    this._steps = 0;
    this.PurgeUsedMemory();
    AsyncDialog.Done = true;
    Action<bool> lockControls = this.LockControls;
    if (lockControls != null)
      lockControls(false);
    taskDialog.Show();
  }

  private void PurgeUsedMemory()
  {
    CloudExportCommand.UIApplication.Application.PurgeReleasedAPIObjects();
    GC.Collect();
    GC.WaitForPendingFinalizers();
  }
}
