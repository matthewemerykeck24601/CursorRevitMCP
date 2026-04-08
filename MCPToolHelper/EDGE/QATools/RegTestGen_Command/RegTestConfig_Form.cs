// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.RegTestGen_Command.RegTestConfig_Form
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.QATools.RegTestGen_Command;

public class RegTestConfig_Form : Window, IComponentConnector
{
  private UIApplication revitApp;
  private List<string> existingTestNames = new List<string>();
  private string jrnFileCopyPath = "";
  internal System.Windows.Controls.TextBox txtRegTestsRootFolder;
  internal System.Windows.Controls.Button btnBrowseMainFolder;
  internal System.Windows.Controls.TextBox txtNewRegTestsFolder;
  internal System.Windows.Controls.Button btnBrowseTestFolder;
  internal System.Windows.Controls.TextBox txtNewTestName;
  internal System.Windows.Controls.TextBox txtNewTestDescription;
  internal System.Windows.Controls.TextBox txtTags;
  internal System.Windows.Controls.TextBox txtTimeout;
  internal System.Windows.Controls.Button btnMakeTest;
  private bool _contentLoaded;

  public RegTestConfig_Form(UIApplication app)
  {
    this.InitializeComponent();
    this.revitApp = app;
    this.FillExistingTestData();
    Queue<string> stringQueue = new Queue<string>();
    FileInfo fileInfo = new FileInfo(this.revitApp.Application.RecordingJournalFilename);
    this.jrnFileCopyPath = $"{fileInfo.Directory.FullName}\\regTestCopy_{fileInfo.Name}";
    File.Copy(fileInfo.FullName, this.jrnFileCopyPath);
    using (StreamReader streamReader = new StreamReader(this.jrnFileCopyPath))
    {
      DateTime? nullable1 = new DateTime?();
      DateTime? nullable2 = new DateTime?();
      while (!streamReader.EndOfStream)
      {
        string line = streamReader.ReadLine();
        stringQueue.Enqueue(line);
        if (line.Contains("started recording journal file"))
          nullable1 = this.GetTimeFromString(line);
        if (stringQueue.Count > 3)
          stringQueue.Dequeue();
      }
      foreach (string line in stringQueue)
      {
        DateTime? timeFromString = this.GetTimeFromString(line);
        if (timeFromString.HasValue)
        {
          nullable2 = timeFromString;
          break;
        }
      }
      streamReader.Close();
      if (!nullable1.HasValue || !nullable2.HasValue)
        return;
      System.Windows.Controls.TextBox txtTimeout = this.txtTimeout;
      DateTime? nullable3 = nullable2;
      DateTime? nullable4 = nullable1;
      string str = (nullable3.HasValue & nullable4.HasValue ? new TimeSpan?(nullable3.GetValueOrDefault() - nullable4.GetValueOrDefault()) : new TimeSpan?()).Value.TotalSeconds.ToString();
      txtTimeout.Text = str;
    }
  }

  private DateTime? GetTimeFromString(string line)
  {
    DateTime now = DateTime.Now;
    Match match = new Regex(".* (\\d+):(\\d+):(\\d+).\\d+;").Match(line);
    if (match.Success)
    {
      string s1 = match.Groups[1].Value;
      string s2 = match.Groups[2].Value;
      string s3 = match.Groups[3].Value;
      int hour;
      ref int local = ref hour;
      int result1;
      int result2;
      if (int.TryParse(s1, out local) && int.TryParse(s2, out result1) && int.TryParse(s3, out result2))
        return new DateTime?(new DateTime(now.Year, now.Month, now.Day, hour, result1, result2));
    }
    return new DateTime?();
  }

  private void FillExistingTestData()
  {
    DirectoryInfo directoryInfo = new DirectoryInfo(this.txtRegTestsRootFolder.Text);
    if (!directoryInfo.Exists || !this.ValidateRegTestFolderContents(directoryInfo.FullName))
      return;
    this.existingTestNames.Clear();
    foreach (FileSystemInfo file in directoryInfo.GetFiles("*.txt", SearchOption.AllDirectories))
      this.existingTestNames.Add(file.Name);
    this.existingTestNames = this.existingTestNames.Distinct<string>().ToList<string>();
  }

  private void btnBrowseTestFolder_Click(object sender, RoutedEventArgs e)
  {
    if (!this.ValidateRegTestFolderContents(this.txtRegTestsRootFolder.Text))
      return;
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    folderBrowserDialog.SelectedPath = Strings.GetRegTestsFolderPath(this.txtRegTestsRootFolder.Text);
    folderBrowserDialog.ShowNewFolderButton = false;
    folderBrowserDialog.Description = "Select Parent Folder for Test (folder in which test folder will be created)";
    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
      return;
    this.txtNewRegTestsFolder.Text = folderBrowserDialog.SelectedPath;
  }

  private void btnBrowseMainFolder_Click(object sender, RoutedEventArgs e)
  {
    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
    for (bool flag = false; !flag; flag = this.ValidateRegTestFolderContents(this.txtRegTestsRootFolder.Text))
    {
      if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
        return;
      this.txtRegTestsRootFolder.Text = folderBrowserDialog.SelectedPath;
    }
    this.FillExistingTestData();
  }

  private bool ValidateRegTestFolderContents(string folderPath)
  {
    bool flag = false;
    DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
    if (!directoryInfo.Exists)
    {
      TaskDialog.Show("Error", $"Specified directory: {folderPath} Could Not be Found.  Please check location");
      return false;
    }
    DirectoryInfo[] directories = directoryInfo.GetDirectories();
    FileInfo[] files = directoryInfo.GetFiles();
    List<string> list1 = ((IEnumerable<DirectoryInfo>) directories).Select<DirectoryInfo, string>((Func<DirectoryInfo, string>) (s => s.Name)).ToList<string>();
    List<string> list2 = ((IEnumerable<FileInfo>) files).Select<FileInfo, string>((Func<FileInfo, string>) (s => s.Name)).ToList<string>();
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("Error List");
    if (!list1.Contains("_BuildsToTest"))
    {
      stringBuilder.AppendLine("Missing _BuildsToTest Folder");
      flag = true;
    }
    if (!list1.Contains("Tests"))
    {
      stringBuilder.AppendLine("Missing Tests Folder");
      flag = true;
    }
    if (!list2.Contains("RegTests.txt"))
    {
      flag = true;
      stringBuilder.AppendLine("Missing RegTests.txt");
    }
    if (!list2.Contains("regTest.lic"))
    {
      flag = true;
      stringBuilder.AppendLine("Missing regTest.lic file");
    }
    if (!flag)
      return true;
    TaskDialog.Show("Error", stringBuilder.ToString());
    return false;
  }

  private void btnMakeTest_Click(object sender, RoutedEventArgs e)
  {
    if (!this.ValidateTestCreationData())
      return;
    DirectoryInfo directoryInfo = new DirectoryInfo($"{this.txtNewRegTestsFolder.Text}\\{this.txtNewTestName.Text}");
    if (!directoryInfo.Exists)
      directoryInfo.Create();
    foreach (string str in this.AdaptJournalForTest(directoryInfo.FullName))
    {
      FileInfo fileInfo = new FileInfo(str);
      if (fileInfo.Exists)
        File.Copy(str, $"{directoryInfo.FullName}\\{fileInfo.Name}");
    }
    FileInfo fileInfo1 = new FileInfo(Strings.GetRegTestsFilePath(this.txtRegTestsRootFolder.Text));
    if (fileInfo1.Exists)
    {
      using (StreamWriter streamWriter = new StreamWriter(fileInfo1.FullName, true))
      {
        streamWriter.WriteLine("# " + this.txtNewTestDescription.Text);
        string str = $"{this.txtNewRegTestsFolder.Text.Remove(0, Strings.GetRegTestsFolderPath(this.txtRegTestsRootFolder.Text).Length)}\\{this.txtNewTestName.Text}\\{this.txtNewTestName.Text}_Test.txt";
        streamWriter.WriteLine($"{str};{this.txtTags.Text};{this.txtTimeout.Text}");
        streamWriter.Close();
      }
    }
    File.Copy(this.txtRegTestsRootFolder.Text + "\\regTest.lic", directoryInfo.FullName + "\\regTest.lic");
    string str1 = "EDGE_Preferences.txt";
    File.Copy("C:\\EDGEforRevit\\" + str1, $"{directoryInfo.FullName}\\{str1}");
    string settingsFolderPath = App.RebarSettingsFolderPath;
    string parameterAsString = Parameters.GetParameterAsString((Element) this.revitApp.ActiveUIDocument.Document.ProjectInformation, "PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (!string.IsNullOrWhiteSpace(parameterAsString) && App.RebarManager.IsAutomatedDocument(this.revitApp.ActiveUIDocument.Document.PathName))
      File.Copy($"{settingsFolderPath}\\{parameterAsString}.txt", $"{directoryInfo.FullName}\\{parameterAsString}.txt");
    this.Close();
  }

  private List<string> AdaptJournalForTest(string fullName)
  {
    List<string> stringList = new List<string>();
    FileInfo fileInfo = new FileInfo(this.jrnFileCopyPath);
    if (fileInfo.Exists)
    {
      Queue<string> stringQueue = new Queue<string>();
      using (StreamReader streamReader = new StreamReader(fileInfo.FullName))
      {
        while (!streamReader.EndOfStream)
        {
          string str = streamReader.ReadLine();
          stringQueue.Enqueue(str);
        }
        streamReader.Close();
      }
      using (StreamWriter streamWriter = new StreamWriter($"{fullName}\\{this.txtNewTestName.Text}_Test.txt"))
      {
        int num = 0;
        while (stringQueue.Count > 1)
        {
          string str1 = stringQueue.Dequeue();
          streamWriter.WriteLine(str1);
          if (str1.Contains("Jrn.Data \"MRUFileName\"") || str1.Contains("Jrn.Data \"File Name\""))
          {
            string input = stringQueue.Dequeue();
            string pattern1 = "(.*\")(.*)(\\\\.+\\.rvt)|(.*\")(.*)(\\\\.+\\.rfa)";
            if (new Regex(pattern1).Match(input).Success)
            {
              string str2 = Regex.Replace(input, pattern1, "$1.$3");
              streamWriter.WriteLine(str2);
              string pattern2 = ".*\"(.*\\.rvt)\"|.*\"(.*\\.rvt)\"";
              string str3 = Strings.ResolveFilePath(Regex.Match(input, pattern2).Groups[1].Value, this.revitApp.Application.RecordingJournalFilename);
              stringList.Add(str3);
            }
          }
          else if (str1.Contains("EDGE.QATools.RegTestGen_Command.RegTestGen_Command"))
          {
            if (num <= 0)
              ++num;
            else
              break;
          }
        }
        streamWriter.Write(" Jrn.Command \"SystemMenu\" , \"Quit the application; prompts to save projects, ID_APP_EXIT\"\nJrn.Data \"TaskDialogResult\"  _\n, \"Do you want to save changes to TestRegressionCoverage.rvt?\",  _\n\"No\", \"IDNO\"");
        streamWriter.Close();
      }
    }
    return stringList;
  }

  private bool ValidateTestCreationData()
  {
    bool flag = false;
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.AppendLine("Error List");
    if (!((IEnumerable<DirectoryInfo>) new DirectoryInfo(Strings.GetRegTestsFolderPath(this.txtRegTestsRootFolder.Text)).GetDirectories("*", SearchOption.AllDirectories)).Where<DirectoryInfo>((Func<DirectoryInfo, bool>) (s => s.FullName == this.txtNewRegTestsFolder.Text)).Any<DirectoryInfo>())
    {
      flag = true;
      stringBuilder.AppendLine($"Proposed Test Folder: {this.txtNewRegTestsFolder.Text} not found under Tests folder");
    }
    if (string.IsNullOrWhiteSpace(this.txtNewTestName.Text))
    {
      flag = true;
      stringBuilder.AppendLine("New test name is empty");
    }
    if (this.txtNewTestName.Text.IndexOf(' ') > 0)
    {
      flag = true;
      stringBuilder.AppendLine("New test name cannot contain spaces");
    }
    if (string.IsNullOrWhiteSpace(this.txtNewTestDescription.Text))
    {
      flag = true;
      stringBuilder.AppendLine("New test description cannot be empty.  This will help us understand what the test is intended to cover");
    }
    if (string.IsNullOrWhiteSpace(this.txtTags.Text))
    {
      flag = true;
      stringBuilder.AppendLine("Tags cannot be empty.  Provide at least one tag");
    }
    if (!flag)
      return true;
    TaskDialog.Show("Error", stringBuilder.ToString());
    return false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/qatools/regtestgen_command/regtestconfig_form.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.txtRegTestsRootFolder = (System.Windows.Controls.TextBox) target;
        break;
      case 2:
        this.btnBrowseMainFolder = (System.Windows.Controls.Button) target;
        this.btnBrowseMainFolder.Click += new RoutedEventHandler(this.btnBrowseMainFolder_Click);
        break;
      case 3:
        this.txtNewRegTestsFolder = (System.Windows.Controls.TextBox) target;
        break;
      case 4:
        this.btnBrowseTestFolder = (System.Windows.Controls.Button) target;
        this.btnBrowseTestFolder.Click += new RoutedEventHandler(this.btnBrowseTestFolder_Click);
        break;
      case 5:
        this.txtNewTestName = (System.Windows.Controls.TextBox) target;
        break;
      case 6:
        this.txtNewTestDescription = (System.Windows.Controls.TextBox) target;
        break;
      case 7:
        this.txtTags = (System.Windows.Controls.TextBox) target;
        break;
      case 8:
        this.txtTimeout = (System.Windows.Controls.TextBox) target;
        break;
      case 9:
        this.btnMakeTest = (System.Windows.Controls.Button) target;
        this.btnMakeTest.Click += new RoutedEventHandler(this.btnMakeTest_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
