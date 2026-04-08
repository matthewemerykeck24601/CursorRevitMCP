// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.WarningAnalyzer.ViewModel.ViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.AdminTools.WarningAnalyzer.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AdminTools.WarningAnalyzer.ViewModel;

internal class ViewModel : INotifyPropertyChanged
{
  private string _idList;
  private string _fileName;
  private bool _cutCheck;
  private bool _cutShowOnly;
  private bool _cutUncut;
  private bool _joinCheck;
  private bool _joinShowOnly;
  private bool _joinUnjoin;
  private bool _duplicateCheck;
  private bool _isolateElemCheck;
  private int _cutCount;
  private int _joinCount;

  public string IdList
  {
    get => this._idList;
    set => this._idList = value;
  }

  public string FileName
  {
    get => this._fileName;
    set
    {
      if (value.Equals(this._fileName))
        return;
      this._fileName = value;
      this.OnPropertyChanged(nameof (FileName));
    }
  }

  public bool CutChecked
  {
    get => this._cutCheck;
    set
    {
      if (this._cutCheck == value)
        return;
      this._cutCheck = value;
      this.OnPropertyChanged(nameof (CutChecked));
    }
  }

  public bool CutShowOnly
  {
    get => this._cutShowOnly;
    set
    {
      if (value == this._cutShowOnly)
        return;
      this._cutShowOnly = value;
      this.OnPropertyChanged(nameof (CutShowOnly));
    }
  }

  public bool CutUncut
  {
    get => this._cutUncut;
    set
    {
      if (value == this._cutUncut)
        return;
      this._cutUncut = value;
      this.OnPropertyChanged(nameof (CutUncut));
    }
  }

  public int CutCount
  {
    get => this._cutCount;
    set
    {
      if (value == this._cutCount)
        return;
      this._cutCount = value;
    }
  }

  public bool JoinChecked
  {
    get => this._joinCheck;
    set
    {
      this._joinCheck = value;
      this.OnPropertyChanged(nameof (JoinChecked));
    }
  }

  public bool JoinShowOnly
  {
    get => this._joinShowOnly;
    set
    {
      if (value == this._joinShowOnly)
        return;
      this._joinShowOnly = value;
      this.OnPropertyChanged(nameof (JoinShowOnly));
    }
  }

  public bool JoinUnjoin
  {
    get => this._joinUnjoin;
    set
    {
      if (value == this._joinUnjoin)
        return;
      this._joinUnjoin = value;
      this.OnPropertyChanged(nameof (JoinUnjoin));
    }
  }

  public int JoinCount
  {
    get => this._joinCount;
    set
    {
      if (value == this._joinCount)
        return;
      this._joinCount = value;
    }
  }

  public bool DuplicateChecked
  {
    get => this._duplicateCheck;
    set
    {
      if (value == this._duplicateCheck)
        return;
      this._duplicateCheck = value;
      this.OnPropertyChanged(nameof (DuplicateChecked));
    }
  }

  public bool IsolateChecked
  {
    get => this._isolateElemCheck;
    set
    {
      if (value == this._isolateElemCheck)
        return;
      this._isolateElemCheck = value;
      this.OnPropertyChanged(nameof (IsolateChecked));
    }
  }

  public ICommand AnalyzeCommand { get; set; }

  public ICommand OpenFile { get; set; }

  public ICommand SelectCommand { get; set; }

  public ICommand Close { get; set; }

  private View CurrentView { get; set; }

  public ViewModel()
  {
    Document document = ActiveModel.Document;
    this.IdList = string.Empty;
    this.FileName = "No file chosen";
    this.CutChecked = false;
    this.CutShowOnly = true;
    this.CutUncut = false;
    this.CutCount = 0;
    this.JoinChecked = false;
    this.JoinShowOnly = true;
    this.JoinUnjoin = false;
    this.JoinCount = 0;
    this.DuplicateChecked = false;
    this.IsolateChecked = false;
    this.AnalyzeCommand = (ICommand) new Command(new Command.ICommandOnExecute(this.ExecuteAnalyzeWarnings), new Command.ICommandOnCanExecute(this.CanExecuteAnalyzeWarnings));
    this.OpenFile = (ICommand) new Command(new Command.ICommandOnExecute(this.ExecuteOpenFile), new Command.ICommandOnCanExecute(this.CanExecuteOpenFile));
    this.Close = (ICommand) new Command(new Command.ICommandOnExecute(this.ExecuteClose), new Command.ICommandOnCanExecute(this.CanExecuteClose));
    this.CurrentView = document.ActiveView;
  }

  public bool CanExecuteAnalyzeWarnings(object parameter)
  {
    return !string.IsNullOrWhiteSpace(this.FileName) && !this.FileName.Contains("No file chosen") && File.Exists(this.FileName);
  }

  public void ExecuteAnalyzeWarnings(object parameter)
  {
    this.IdList = this.Parser();
    this.SelectElements();
    if ((!this.JoinChecked || !this.JoinUnjoin) && (!this.CutChecked || !this.CutUncut))
      return;
    new ResultsWindow(this.JoinCount, this.CutCount).ShowDialog();
  }

  public bool CanExecuteOpenFile(object parameter) => true;

  public void ExecuteOpenFile(object parameter)
  {
    OpenFileDialog openFileDialog = new OpenFileDialog();
    openFileDialog.DefaultExt = ".html";
    openFileDialog.Filter = "HTML Files|*.html|All Files (*.*)|*.*";
    if (!openFileDialog.ShowDialog().GetValueOrDefault())
      return;
    this.FileName = openFileDialog.FileName;
  }

  public bool CanExecuteClose(object parameter) => true;

  public void ExecuteClose(object parameter)
  {
    if (!(parameter is MainWindow))
      return;
    (parameter as MainWindow).Close();
  }

  public string Parser()
  {
    Document document = ActiveModel.Document;
    bool flag1 = false;
    int num1 = 0;
    string firstIdString = "";
    string secondIdString = "";
    string str1 = "";
    bool flag2 = false;
    string[] separator = new string[4]
    {
      " id ",
      "  </td>",
      "  <br>",
      "<h1>  "
    };
    StringBuilder stringBuilder = new StringBuilder();
    if (!new FileInfo(this.FileName).Exists)
      return string.Empty;
    this.JoinCount = 0;
    this.CutCount = 0;
    using (Transaction transaction = new Transaction(document, "Analyze Warnings"))
    {
      int num2 = (int) transaction.Start();
      using (StreamReader streamReader = new StreamReader(this.FileName))
      {
        string empty = string.Empty;
        string str2;
        while ((str2 = streamReader.ReadLine()) != null)
        {
          if (flag1)
            flag1 = false;
          else if (!this.JoinChecked && str2.Contains("Highlighted elements are joined but do not intersect."))
            flag1 = true;
          else if (!this.CutChecked && str2.Contains("Elements do not intersect."))
            flag1 = true;
          else if (!this.DuplicateChecked && str2.Contains("There are identical instances in the same place."))
          {
            flag1 = true;
          }
          else
          {
            foreach (string s in str2.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
              int result = 0;
              if (int.TryParse(s, out result))
              {
                if (str1.Contains("Highlighted elements are joined but do not intersect.") && this.JoinUnjoin)
                {
                  switch (num1)
                  {
                    case 0:
                      stringBuilder.Append(s + ";");
                      firstIdString = s;
                      ++num1;
                      continue;
                    case 1:
                      stringBuilder.Append(s + ";");
                      secondIdString = s;
                      num1 = 0;
                      flag2 = true;
                      break;
                  }
                  if (flag2)
                  {
                    this.Unjoin(firstIdString, secondIdString);
                    flag2 = false;
                    continue;
                  }
                }
                if (str1.Contains("Elements do not intersect") && this.CutUncut)
                {
                  switch (num1)
                  {
                    case 0:
                      stringBuilder.Append(s + ";");
                      firstIdString = s;
                      ++num1;
                      continue;
                    case 1:
                      stringBuilder.Append(s + ";");
                      secondIdString = s;
                      num1 = 0;
                      flag2 = true;
                      break;
                  }
                  if (flag2)
                  {
                    this.Uncut(firstIdString, secondIdString);
                    flag2 = false;
                    continue;
                  }
                }
                stringBuilder.Append(s + ";");
              }
            }
            str1 = str2;
          }
        }
      }
      int num3 = (int) transaction.Commit();
    }
    return stringBuilder.ToString();
  }

  private void SelectElements()
  {
    UIDocument uiDoc = ActiveModel.UIDoc;
    Document document = uiDoc.Document;
    ICollection<ElementId> elementIds = (ICollection<ElementId>) new List<ElementId>();
    string[] source = this.IdList.Split(new string[1]{ ";" }, StringSplitOptions.RemoveEmptyEntries);
    for (int index = 0; index < source.Length; ++index)
      elementIds.Add(new ElementId(Convert.ToInt32(((IEnumerable<string>) source).ElementAt<string>(index))));
    if (!this.IsolateChecked)
    {
      using (Transaction transaction = new Transaction(document, "Select Elements"))
      {
        uiDoc.ActiveView = this.CurrentView;
        int num1 = (int) transaction.Start();
        uiDoc.Selection.SetElementIds(elementIds);
        int num2 = (int) transaction.Commit();
      }
    }
    else
    {
      using (Transaction transaction = new Transaction(document, "Isolate Elements"))
      {
        int num3 = (int) transaction.Start();
        document.ActiveView.IsolateElementsTemporary(elementIds);
        int num4 = (int) transaction.Commit();
      }
    }
  }

  private void Unjoin(string firstIdString, string secondIdString)
  {
    Document document = ActiveModel.Document;
    ElementId id1 = new ElementId(Convert.ToInt32(firstIdString));
    ElementId id2 = new ElementId(Convert.ToInt32(secondIdString));
    try
    {
      if (!JoinGeometryUtils.AreElementsJoined(document, document.GetElement(id1), document.GetElement(id2)))
        return;
      JoinGeometryUtils.UnjoinGeometry(document, document.GetElement(id1), document.GetElement(id2));
      ++this.JoinCount;
    }
    catch
    {
    }
  }

  private void Uncut(string firstIdString, string secondIdString)
  {
    Document document = ActiveModel.Document;
    ElementId id1 = new ElementId(Convert.ToInt32(firstIdString));
    ElementId id2 = new ElementId(Convert.ToInt32(secondIdString));
    try
    {
      bool firstCutsSecond = false;
      bool flag = SolidSolidCutUtils.CutExistsBetweenElements(document.GetElement(id1), document.GetElement(id2), out firstCutsSecond);
      if (firstCutsSecond | flag)
      {
        SolidSolidCutUtils.RemoveCutBetweenSolids(document, document.GetElement(id1), document.GetElement(id2));
        ++this.CutCount;
        return;
      }
    }
    catch
    {
    }
    try
    {
      if (!InstanceVoidCutUtils.GetCuttingVoidInstances(document.GetElement(id2)).Contains(id1))
        return;
      InstanceVoidCutUtils.RemoveInstanceVoidCut(document, document.GetElement(id1), document.GetElement(id2));
      ++this.CutCount;
    }
    catch
    {
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  protected void OnPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
