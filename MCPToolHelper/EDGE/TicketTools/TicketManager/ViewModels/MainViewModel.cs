// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.MainViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.Resources;
using EDGE.TicketTools.TicketManager.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Utils.ElementUtils;
using Utils.ExcelUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

internal class MainViewModel : INotifyPropertyChanged
{
  private static int refreshCount;
  private AssemblyListViewModel _assemblyList;
  private int _totalMarks;
  private int _totalPieces;
  private int _marksReinforced;
  private int _piecesReinforced;
  private int _marksCreated;
  private int _piecesCreated;
  private int _marksDetailed;
  private int _piecesDetailed;
  private int _marksReleased;
  private int _piecesReleased;
  private int _onHolds;
  private int _identityComments;
  private int _custom1s;
  private int _custom2s;
  private int _custom3s;
  private int _custom4s;
  private string _search;
  private int _searchCount;
  private bool _showUsers;
  private bool _isSelected;
  public bool IsInFamilyEditor;
  private AssemblyViewModel _selectedItem;
  private int _dataGridMaxWidth;
  private UIDocument mDoc;
  public MainWindow getWindow;
  private ICollectionView view;

  public List<ViewSheet> GetViewSheet { get; set; }

  public ExternalEvent AcceptChangesEvent { get; set; }

  public ExternalEvent ReleaseTicketEvent { get; set; }

  public ExternalEvent WriteCommentsEvent { get; set; }

  public ExternalEvent TicketPopulatorEvent { get; set; }

  public ExternalEvent SendToCloudEvent { get; set; }

  public ExternalEvent UpdateParametersEvent { get; set; }

  public AssemblyListViewModel AssemblyList
  {
    get => this._assemblyList;
    set
    {
      this._assemblyList = value;
      this.OnPropertyChanged(nameof (AssemblyList));
    }
  }

  public bool isSelected
  {
    get => this._isSelected;
    set
    {
      this._isSelected = value;
      this.OnPropertyChanged(nameof (isSelected));
    }
  }

  public AssemblyViewModel EditingItem { get; set; }

  public AssemblyViewModel SelectedItem
  {
    get => this._selectedItem;
    set
    {
      this._selectedItem = value;
      this.OnPropertyChanged(nameof (SelectedItem));
    }
  }

  public int TotalMarks
  {
    get => this._totalMarks;
    set
    {
      this._totalMarks = value;
      this.OnPropertyChanged(nameof (TotalMarks));
    }
  }

  public int TotalPieces
  {
    get => this._totalPieces;
    set
    {
      this._totalPieces = value;
      this.OnPropertyChanged(nameof (TotalPieces));
    }
  }

  public int MarksReinforced
  {
    get => this._marksReinforced;
    set
    {
      this._marksReinforced = value;
      this.OnPropertyChanged(nameof (MarksReinforced));
    }
  }

  public int PiecesReinforced
  {
    get => this._piecesReinforced;
    set
    {
      this._piecesReinforced = value;
      this.OnPropertyChanged(nameof (PiecesReinforced));
    }
  }

  public int MarksDetailed
  {
    get => this._marksDetailed;
    set
    {
      this._marksDetailed = value;
      this.OnPropertyChanged(nameof (MarksDetailed));
    }
  }

  public int PiecesDetailed
  {
    get => this._piecesDetailed;
    set
    {
      this._piecesDetailed = value;
      this.OnPropertyChanged(nameof (PiecesDetailed));
    }
  }

  public int MarksReleased
  {
    get => this._marksReleased;
    set
    {
      this._marksReleased = value;
      this.OnPropertyChanged(nameof (MarksReleased));
    }
  }

  public int PiecesReleased
  {
    get => this._piecesReleased;
    set
    {
      this._piecesReleased = value;
      this.OnPropertyChanged(nameof (PiecesReleased));
    }
  }

  public Command CloseCommand { get; set; }

  public Command ExportCommand { get; set; }

  public Command ViewHistoryCommand { get; set; }

  public Command ShowViewSheetCommand { get; set; }

  public Command AcceptEditsCommand { get; set; }

  public Command ReleaseTicketCommand { get; set; }

  public Command RefreshCommand { get; set; }

  public Command RefreshButtonCommand { get; set; }

  public Command ExportDWFCommand { get; set; }

  public Command PopulatorCommand { get; set; }

  public Command WriteCommentCommand { get; set; }

  public Command SendToCloudCommand { get; set; }

  public Command UpdateParametersCommand { get; set; }

  public string Search
  {
    get => this._search;
    set
    {
      this._search = value;
      if (this._search.Equals((string) null))
        return;
      this.AssemblyList.AssemblyList = this.AssemblyList.AllAssemblyViewModels.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => model.MarkNumber.ToLower().Contains(this._search.ToLower()))).OrderBy<AssemblyViewModel, string>((Func<AssemblyViewModel, string>) (model => model.MarkNumber)).ToList<AssemblyViewModel>();
      if (this.AssemblyList.AssemblyList.Count == 0)
      {
        this.SearchCount = 0;
        this.MarksReinforced = 0;
        this.PiecesReinforced = 0;
        this.MarksDetailed = 0;
        this.PiecesDetailed = 0;
        this.MarksReleased = 0;
        this.PiecesReleased = 0;
      }
      else
      {
        this.SearchCount = this.AssemblyList.AssemblyList.Count;
        this.MarksReinforced = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReinforcedDate.Equals(""))).ToList<AssemblyViewModel>().Count;
        this.PiecesReinforced = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReinforcedDate.Equals(""))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
        this.MarksDetailed = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.DetailedDate.Equals(""))).ToList<AssemblyViewModel>().Count;
        this.PiecesDetailed = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.DetailedDate.Equals(""))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
        this.MarksReleased = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReleasedDate.Equals(""))).ToList<AssemblyViewModel>().Count;
        this.PiecesReleased = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReleasedDate.Equals(""))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
      }
    }
  }

  public int SearchCount
  {
    get => this._searchCount;
    set
    {
      this._searchCount = value;
      this.OnPropertyChanged(nameof (SearchCount));
    }
  }

  public bool ShowUsers
  {
    get => this._showUsers;
    set
    {
      this._showUsers = value;
      this.DataGridMaxWidth = value ? 1120 : 640;
      this.OnPropertyChanged(nameof (ShowUsers));
    }
  }

  public string CurrentModel { get; set; }

  public string WindowTitle { get; set; }

  public int DataGridMaxWidth
  {
    get => this._dataGridMaxWidth;
    set
    {
      this._dataGridMaxWidth = value;
      this.OnPropertyChanged(nameof (DataGridMaxWidth));
    }
  }

  [DebuggerHidden]
  public bool CanExecuteClose(object parameter) => true;

  public void ExecuteClose(object parameter)
  {
    if (!(parameter is MainWindow))
      return;
    App.TicketManagerWindow = (MainWindow) null;
    (parameter as MainWindow).Close();
  }

  [DebuggerHidden]
  public bool CanExecuteExport(object parameter) => !this.SearchCount.Equals(0);

  public bool IsOpen(string wbook)
  {
    string str = wbook.Substring(0, wbook.IndexOf("."));
    foreach (Process process in Process.GetProcessesByName("notepad"))
    {
      if (process.MainWindowTitle.Equals(str + " - Notepad"))
        return true;
    }
    return false;
  }

  private string RemoveCommas(string delimitedField)
  {
    if (delimitedField != null)
      delimitedField = delimitedField.Replace(",", " ");
    return delimitedField;
  }

  public bool TicketExport(
    UIDocument uIDocument,
    List<string> prevFileData,
    string filepath,
    out string errMessage)
  {
    errMessage = string.Empty;
    try
    {
      StringBuilder strBuild = new StringBuilder();
      string str1 = App.TMCFolderPath;
      if (str1.Equals(""))
        str1 = "C:\\EDGEforRevit";
      ProjectInfo projectInformation = ActiveModel.Document.ProjectInformation;
      string parameterAsString1 = Parameters.GetParameterAsString((Element) projectInformation, "PROJECT_CLIENT_PRECAST_MANUFACTURER");
      string parameterAsString2 = Parameters.GetParameterAsString((Element) projectInformation, "Project Name");
      string parameterAsString3 = Parameters.GetParameterAsString((Element) projectInformation, "Project Number");
      List<string> stringList1 = new List<string>();
      strBuild.AppendLine("PROJECT NAME,PROJECT NUMBER,QUANTITY,MARK ID,DESCRIPTION,REINFORCED DATE,REINFORCED USER,CREATED DATE,CREATED USER,DETAILED DATE,DETAILED USER,DRAFT CHECK DATE,DRAFT CHECK USER,ENGINEERING CHECK DATE,ENGINEERING CHECK USER,RELEASED DATE,RELEASED USER,NEEDS REVISION,FLAGGED,ON HOLD,IDENTITY COMMENT");
      if (prevFileData != null)
      {
        List<string> stringList2 = new List<string>();
        string empty = string.Empty;
        Dictionary<int, int> fields = new Dictionary<int, int>();
        foreach (string line in prevFileData)
        {
          ((IEnumerable<string>) line.Split(',')).ToList<string>();
          if ((!line.Contains(parameterAsString2) || !line.Contains(parameterAsString3)) && !string.IsNullOrEmpty(line))
          {
            if (((IEnumerable<string>) line.Split(',')).Count<string>() == 15)
            {
              fields.Clear();
              fields.Add(2, 1);
              fields.Add(10, 4);
              fields.Add(12, 1);
              strBuild = this.AddEmptyFields(line, fields, strBuild);
            }
            else if (((IEnumerable<string>) line.Split(',')).Count<string>() == 16 /*0x10*/)
            {
              fields.Clear();
              fields.Add(11, 4);
              fields.Add(13, 1);
              strBuild = this.AddEmptyFields(line, fields, strBuild);
            }
            else
              strBuild.AppendLine(line);
          }
        }
      }
      if (parameterAsString2.Contains("="))
      {
        int num = (int) MessageBox.Show("Invalid Name! Please do not include \"=\" in the project name.", "Warning");
        return false;
      }
      string empty1 = string.Empty;
      if (this.Search != null && this.Search.Length > 0)
        this.Search.ToUpper();
      List<string> stringList3 = new List<string>();
      List<string> stringList4 = new List<string>();
      if (!parameterAsString1.Equals("") || parameterAsString1 != null)
      {
        if (File.Exists($"{str1}\\{parameterAsString1}_TicketManagerCustomizationSettings.txt"))
        {
          foreach (string readAllLine in File.ReadAllLines($"{str1}\\{parameterAsString1}_TicketManagerCustomizationSettings.txt"))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray)[1].Split('|');
            stringList3.Add(strArray[0].Trim());
            stringList4.Add(strArray[1].Trim());
          }
        }
        else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
        {
          foreach (string readAllLine in File.ReadAllLines(str1 + "\\TicketManagerCustomizationSettings.txt"))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray)[1].Split('|');
            stringList3.Add(strArray[0].Trim());
            stringList4.Add(strArray[1].Trim());
          }
        }
      }
      else if (File.Exists(str1 + "\\TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines(str1 + "\\TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          stringList3.Add(strArray[0].Trim());
          stringList4.Add(strArray[1].Trim());
        }
      }
      List<string> stringList5 = new List<string>();
      List<string> stringList6 = new List<string>()
      {
        "IDENTITY_COMMENT",
        "ON_HOLD",
        "TICKET_REINFORCED_DATE_CURRENT",
        "TICKET_REINFORCED_USER_CURRENT",
        "TICKET_CREATED_DATE_INITIAL",
        "TICKET_CREATED_USER_INITIAL",
        "TICKET_DETAILED_DATE_CURRENT",
        "TICKET_DETAILED_USER_CURRENT",
        "TICKET_RELEASED_DATE_CURRENT",
        "TICKET_RELEASED_USER_CURRENT",
        "TICKET_REINFORCED_DATE_INITIAL",
        "TICKET_REINFORCED_USER_INITIAL",
        "TICKET_RELEASED_DATE_CURRENT",
        "TICKET_RELEASED_USER_CURRENT",
        "TICKET_CREATED_DATE_CURRENT",
        "TICKET_CREATED_USER_CURRENT",
        "TICKET_DETAILED_DATE_INITIAL",
        "TICKET_DETAILED_USER_INITIAL",
        "ASSEMBLY_MARK_NUMBER",
        "CONTROL_MARK",
        "TICKET_EDIT_COMMENT",
        "TICKET_DESCRIPTION",
        "TICKET_FLAGGED",
        "TICKET_RELEASED_DATE_INITIAL",
        "TICKET_RELEASED_USER_INITIAL",
        "TKT_TOTAL_RELEASED",
        "TICKET_DRAFTING_CHECKED_USER_CURRENT",
        "TICKET_DRAFTING_CHECKED_USER_INITIAL",
        "TICKET_DRAFTING_CHECKED_DATE_CURRENT",
        "TICKET_DRAFTING_CHECKED_DATE_INITIAL",
        "TICKET_ENGINEERING_CHECKED_USER_CURRENT",
        "TICKET_ENGINEERING_CHECKED_USER_INITIAL",
        "TICKET_ENGINEERING_CHECKED_DATE_CURRENT",
        "TICKET_ENGINEERING_CHECKED_DATE_INITIAL",
        "TICKET_NEEDS_REVISION"
      };
      DefinitionBindingMapIterator bindingMapIterator = uIDocument.Application.ActiveUIDocument.Document.ParameterBindings.ForwardIterator();
      while (bindingMapIterator.MoveNext())
      {
        InstanceBinding current1 = bindingMapIterator.Current as InstanceBinding;
        TypeBinding current2 = bindingMapIterator.Current as TypeBinding;
        if (bindingMapIterator.Current != null)
        {
          CategorySet categories;
          if (bindingMapIterator.Current is InstanceBinding)
            categories = current1.Categories;
          else if (bindingMapIterator.Current is TypeBinding)
            categories = current2.Categories;
          else
            continue;
          foreach (Category category in categories)
          {
            if ((category.Name.Equals("Assemblies") || category.Name.Equals("Structural Framing")) && !stringList5.Contains(bindingMapIterator.Key.Name) && !stringList6.Contains(bindingMapIterator.Key.Name.ToUpper()) && (bindingMapIterator.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
              stringList5.Add(bindingMapIterator.Key.Name);
          }
        }
      }
      List<string> stringList7 = new List<string>();
      for (int index = 0; index < stringList3.Count; ++index)
      {
        if (stringList5.Contains(stringList4[index]))
          stringList7.Add(stringList3[index]);
      }
      foreach (AssemblyViewModel assembly in this.AssemblyList.AssemblyList)
      {
        string delimitedField1 = assembly.Quantity.ToString();
        string delimitedField2 = !string.IsNullOrWhiteSpace(assembly.MarkNumber) ? assembly.MarkNumber : "INVALID,";
        string delimitedField3 = assembly.Description.Length > 50 ? assembly.Description.Substring(0, 45) + "....." : assembly.Description;
        string reinforcedDateInitial = assembly.ReinforcedDate_Initial;
        string reinforcedByInitial = assembly.ReinforcedBy_Initial;
        string createdDateInitial = assembly.CreatedDate_Initial;
        string createdByInitial = assembly.CreatedBy_Initial;
        string detailedDateInitial = assembly.DetailedDate_Initial;
        string detailedByInitial = assembly.DetailedBy_Initial;
        string draftingCheckedInitial = assembly.DraftingChecked_Initial;
        string draftingDateInitial = assembly.DraftingDate_Initial;
        string engineeringCheckedInitial = assembly.EngineeringChecked_Initial;
        string engineeringDateInitial = assembly.EngineeringDate_Initial;
        string needsRevision = assembly.NeedsRevision;
        string releasedDateInitial = assembly.ReleasedDate_Initial;
        string releasedByInitial = assembly.ReleasedBy_Initial;
        string delimitedField4 = (assembly.IsFlagged || assembly.isInvalidQuantity || assembly.SheetFlagged).ToString();
        string onHold = assembly.OnHold;
        string identityComment = assembly.IdentityComment;
        strBuild.AppendLine($"{this.RemoveCommas(parameterAsString2)},{this.RemoveCommas(parameterAsString3)},{this.RemoveCommas(delimitedField1)},{this.RemoveCommas(delimitedField2)},{this.RemoveCommas(delimitedField3)},{this.RemoveCommas(reinforcedDateInitial)},{this.RemoveCommas(reinforcedByInitial)},{this.RemoveCommas(createdDateInitial)},{this.RemoveCommas(createdByInitial)},{this.RemoveCommas(detailedDateInitial)},{this.RemoveCommas(detailedByInitial)},{this.RemoveCommas(draftingDateInitial)},{this.RemoveCommas(draftingCheckedInitial)},{this.RemoveCommas(engineeringDateInitial)},{this.RemoveCommas(engineeringCheckedInitial)},{this.RemoveCommas(releasedDateInitial)},{this.RemoveCommas(releasedByInitial)},{this.RemoveCommas(needsRevision)},{this.RemoveCommas(delimitedField4)},{this.RemoveCommas(onHold)},{this.RemoveCommas(identityComment)}");
      }
      if (File.Exists(filepath))
      {
        string str2 = filepath;
        char[] charArray = filepath.ToCharArray();
        int num1 = str2.LastIndexOf("\\");
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = num1 + 1; index < charArray.Length; ++index)
          stringBuilder.Append(charArray[index].ToString());
        string wbook = stringBuilder.ToString();
        if (wbook.Contains("txt") && this.IsOpen(wbook))
        {
          int num2 = (int) MessageBox.Show("The file you attempted to save is being used by another process. Please close the file before saving it.", "Warning");
          return false;
        }
      }
      using (StreamWriter streamWriter = new StreamWriter(filepath))
        streamWriter.WriteLine(strBuild.ToString());
      return true;
    }
    catch (DirectoryNotFoundException ex)
    {
      errMessage = "Could not find directory to export successfully";
      return false;
    }
    catch (FileNotFoundException ex)
    {
      errMessage = "Could not find file to export successfully";
      return false;
    }
    catch (Exception ex)
    {
      if (!ex.Message.ToUpper().Contains("ACCESS TO THE PATH"))
      {
        TaskDialog.Show("Ticket Export Exception", ex.Message);
        File.WriteAllLines(filepath, (IEnumerable<string>) prevFileData);
      }
      else
        errMessage = "Unable to determine if the export file path is accessible.\nPlease check that the path name is accessible and formatted correctly.";
      return false;
    }
  }

  private StringBuilder AddEmptyFields(
    string line,
    Dictionary<int, int> fields,
    StringBuilder strBuild)
  {
    string str = string.Empty;
    List<string> list = ((IEnumerable<string>) line.Split(',')).ToList<string>();
    for (int index1 = 0; index1 < list.Count; ++index1)
    {
      if (fields.ContainsKey(index1))
      {
        if (fields[index1] == 1)
        {
          str = $"{str},,{list[index1]}";
        }
        else
        {
          for (int index2 = 1; index2 <= fields[index1]; ++index2)
            str += ",";
          str = $"{str},{list[index1]}";
        }
      }
      else
        str = index1 != 0 ? $"{str},{list[index1]}" : list[index1];
    }
    strBuild.AppendLine(str);
    return strBuild;
  }

  public void ExecuteExport(object parameter)
  {
    try
    {
      StringBuilder stringBuilder1 = new StringBuilder();
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      saveFileDialog1.AddExtension = true;
      saveFileDialog1.DefaultExt = ".txt";
      saveFileDialog1.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx";
      SaveFileDialog saveFileDialog2 = saveFileDialog1;
      if (!saveFileDialog2.ShowDialog().GetValueOrDefault())
        return;
      string fileName = saveFileDialog2.FileName;
      string str1 = App.TMCFolderPath;
      if (str1.Equals(""))
        str1 = "C:\\EDGEforRevit";
      ProjectInfo projectInformation = ActiveModel.Document.ProjectInformation;
      string parameterAsString1 = Parameters.GetParameterAsString((Element) projectInformation, "PROJECT_CLIENT_PRECAST_MANUFACTURER");
      string parameterAsString2 = Parameters.GetParameterAsString((Element) projectInformation, "Project Name");
      if (parameterAsString2.Contains("="))
      {
        int num1 = (int) MessageBox.Show("Invalid Name! Please do not include \"=\" in the project name.", "Warning");
      }
      else
      {
        string str2 = this.Search.Length > 0 ? this.Search.ToUpper() : "All Marks";
        List<string> stringList1 = new List<string>();
        List<string> stringList2 = new List<string>();
        if (!parameterAsString1.Equals("") || parameterAsString1 != null)
        {
          if (File.Exists($"{str1}\\{parameterAsString1}_TicketManagerCustomizationSettings.txt"))
          {
            foreach (string readAllLine in File.ReadAllLines($"{str1}\\{parameterAsString1}_TicketManagerCustomizationSettings.txt"))
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
        List<string> stringList3 = new List<string>();
        List<string> stringList4 = new List<string>()
        {
          "IDENTITY_COMMENT",
          "ON_HOLD",
          "TICKET_REINFORCED_DATE_CURRENT",
          "TICKET_REINFORCED_USER_CURRENT",
          "TICKET_CREATED_DATE_INITIAL",
          "TICKET_CREATED_USER_INITIAL",
          "TICKET_DETAILED_DATE_CURRENT",
          "TICKET_DETAILED_USER_CURRENT",
          "TICKET_RELEASED_DATE_CURRENT",
          "TICKET_RELEASED_USER_CURRENT",
          "TICKET_REINFORCED_DATE_INITIAL",
          "TICKET_REINFORCED_USER_INITIAL",
          "TICKET_RELEASED_DATE_CURRENT",
          "TICKET_RELEASED_USER_CURRENT",
          "TICKET_CREATED_DATE_CURRENT",
          "TICKET_CREATED_USER_CURRENT",
          "TICKET_DETAILED_DATE_INITIAL",
          "TICKET_DETAILED_USER_INITIAL",
          "ASSEMBLY_MARK_NUMBER",
          "CONTROL_MARK",
          "TICKET_EDIT_COMMENT",
          "TICKET_DESCRIPTION",
          "TICKET_FLAGGED",
          "TICKET_RELEASED_DATE_INITIAL",
          "TICKET_RELEASED_USER_INITIAL",
          "TKT_TOTAL_RELEASED"
        };
        DefinitionBindingMapIterator bindingMapIterator = this.mDoc.Application.ActiveUIDocument.Document.ParameterBindings.ForwardIterator();
        while (bindingMapIterator.MoveNext())
        {
          InstanceBinding current1 = bindingMapIterator.Current as InstanceBinding;
          TypeBinding current2 = bindingMapIterator.Current as TypeBinding;
          if (bindingMapIterator.Current != null)
          {
            CategorySet categories;
            if (bindingMapIterator.Current is InstanceBinding)
              categories = current1.Categories;
            else if (bindingMapIterator.Current is TypeBinding)
              categories = current2.Categories;
            else
              continue;
            foreach (Category category in categories)
            {
              if ((category.Name.Equals("Assemblies") || category.Name.Equals("Structural Framing")) && !stringList3.Contains(bindingMapIterator.Key.Name) && !stringList4.Contains(bindingMapIterator.Key.Name.ToUpper()) && (bindingMapIterator.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
                stringList3.Add(bindingMapIterator.Key.Name);
            }
          }
        }
        List<string> stringList5 = new List<string>();
        for (int index = 0; index < stringList1.Count; ++index)
        {
          if (stringList3.Contains(stringList2[index]))
            stringList5.Add(stringList1[index]);
        }
        List<string> stringList6 = stringList5;
        string str3 = "";
        if (stringList6.Count > 0)
          str3 = stringList6[0].Substring(0, Math.Min(stringList6[0].Length, 25));
        string str4 = "";
        if (stringList6.Count > 1)
          str4 = stringList6[1].Substring(0, Math.Min(stringList6[1].Length, 25));
        string str5 = "";
        if (stringList6.Count > 2)
          str5 = stringList6[2].Substring(0, Math.Min(stringList6[2].Length, 25));
        string str6 = "";
        if (stringList6.Count > 3)
          str6 = stringList6[3].Substring(0, Math.Min(stringList6[3].Length, 25));
        string extension = Path.GetExtension(fileName);
        bool flag1 = true;
        List<List<object>> source = new List<List<object>>();
        switch (extension)
        {
          case ".txt":
            stringBuilder1.AppendLine("");
            stringBuilder1.AppendLine("               Client:   " + parameterAsString1);
            stringBuilder1.AppendLine("              Project:   " + parameterAsString2);
            stringBuilder1.AppendLine("");
            stringBuilder1.AppendLine("     Revit Model File:   " + this.CurrentModel);
            stringBuilder1.AppendLine("          Report Date:   " + DateTime.Today.ToString("yyyy-MM-dd"));
            stringBuilder1.AppendLine("");
            stringBuilder1.AppendLine($"         Total Pieces:   {this.TotalPieces.ToString()}{" ".Repeat(12 - this.TotalPieces.ToString().Length)}Detailed Marks:   {this.MarksDetailed.ToString()}");
            stringBuilder1.AppendLine($"          Total Marks:   {this.TotalMarks.ToString()}{" ".Repeat(11 - this.TotalMarks.ToString().Length)}Detailed Pieces:   {this.PiecesDetailed.ToString()}");
            stringBuilder1.AppendLine("");
            StringBuilder stringBuilder2 = stringBuilder1;
            string[] strArray1 = new string[5]
            {
              "     Reinforced Marks:   ",
              null,
              null,
              null,
              null
            };
            int marksReinforced = this.MarksReinforced;
            strArray1[1] = marksReinforced.ToString();
            marksReinforced = this.MarksReinforced;
            strArray1[2] = " ".Repeat(12 - marksReinforced.ToString().Length);
            strArray1[3] = "Released Marks:   ";
            int num2 = this.MarksReleased;
            strArray1[4] = num2.ToString();
            string str7 = string.Concat(strArray1);
            stringBuilder2.AppendLine(str7);
            StringBuilder stringBuilder3 = stringBuilder1;
            string[] strArray2 = new string[5]
            {
              "    Reinforced Pieces:   ",
              null,
              null,
              null,
              null
            };
            num2 = this.PiecesReinforced;
            strArray2[1] = num2.ToString();
            num2 = this.PiecesReinforced;
            strArray2[2] = " ".Repeat(11 - num2.ToString().Length);
            strArray2[3] = "Released Pieces:   ";
            num2 = this.PiecesReleased;
            strArray2[4] = num2.ToString();
            string str8 = string.Concat(strArray2);
            stringBuilder3.AppendLine(str8);
            stringBuilder1.AppendLine("");
            stringBuilder1.AppendLine("          List Filter:   " + str2);
            stringBuilder1.AppendLine("");
            stringBuilder1.AppendLine("");
            Dictionary<AssemblyViewModel, List<string>> dictionary = new Dictionary<AssemblyViewModel, List<string>>();
            int count1 = 7;
            int count2 = 11;
            int count3 = 15;
            int count4 = 15;
            int count5 = 12;
            int count6 = 12;
            int count7 = 13;
            int count8 = 13;
            int count9 = 13;
            int count10 = 13;
            int count11 = 7;
            int count12 = 7;
            int count13 = 16 /*0x10*/;
            int count14 = 30;
            int count15 = 30;
            int count16 = 30;
            int count17 = 30;
            foreach (AssemblyViewModel assembly in this.AssemblyList.AssemblyList)
            {
              string str9 = !string.IsNullOrWhiteSpace(assembly.MarkNumber) ? assembly.MarkNumber : "INVALID";
              string str10 = assembly.Description.Length > 50 ? assembly.Description.Substring(0, 45) + "....." : assembly.Description;
              string reinforcedDate = assembly.ReinforcedDate;
              string reinforcedBy = assembly.ReinforcedBy;
              string createdDate = assembly.CreatedDate;
              string createdBy = assembly.CreatedBy;
              string detailedDate = assembly.DetailedDate;
              string detailedBy = assembly.DetailedBy;
              string releasedDate = assembly.ReleasedDate;
              string releasedBy = assembly.ReleasedBy;
              string onHold = assembly.OnHold;
              string identityComment = assembly.IdentityComment;
              string custom1 = assembly.Custom1;
              string custom2 = assembly.Custom2;
              string custom3 = assembly.Custom3;
              string custom4 = assembly.Custom4;
              string str11 = (assembly.IsFlagged || assembly.isInvalidQuantity || assembly.SheetFlagged).ToString();
              count1 = str9.Length > count1 ? str9.Length : count1;
              count2 = str10.Length > count2 ? str10.Length : count2;
              count3 = reinforcedDate.Length > count3 ? reinforcedDate.Length : count3;
              count4 = reinforcedBy.Length > count4 ? reinforcedBy.Length : count4;
              count5 = createdDate.Length > count5 ? createdDate.Length : count5;
              count6 = createdBy.Length > count6 ? createdBy.Length : count6;
              count7 = detailedDate.Length > count7 ? detailedDate.Length : count7;
              count8 = detailedBy.Length > count8 ? detailedBy.Length : count8;
              count9 = releasedDate.Length > count9 ? releasedDate.Length : count9;
              count10 = releasedBy.Length > count10 ? releasedBy.Length : count10;
              count11 = str11.Length > count11 ? str11.Length : count11;
              count12 = onHold.Length > count12 ? onHold.Length : count12;
              count13 = identityComment.Length > count13 ? identityComment.Length : count13;
              if (stringList6.Count == 4)
              {
                count14 = custom1.Length > count14 ? custom1.Length : count14;
                count15 = custom2.Length > count15 ? custom2.Length : count15;
                count16 = custom3.Length > count16 ? custom3.Length : count16;
                count17 = custom4.Length > count17 ? custom4.Length : count17;
              }
              else if (stringList6.Count == 3)
              {
                count14 = custom1.Length > count14 ? custom1.Length : count14;
                count15 = custom2.Length > count15 ? custom2.Length : count15;
                count16 = custom3.Length > count16 ? custom3.Length : count16;
              }
              else if (stringList6.Count == 2)
              {
                count14 = custom1.Length > count14 ? custom1.Length : count14;
                count15 = custom2.Length > count15 ? custom2.Length : count15;
              }
              else if (stringList6.Count == 1)
                count14 = custom1.Length > count14 ? custom1.Length : count14;
              dictionary.Add(assembly, new List<string>()
              {
                str9,
                str10,
                reinforcedDate,
                reinforcedBy,
                createdDate,
                createdBy,
                detailedDate,
                detailedBy,
                releasedDate,
                releasedBy,
                str11,
                onHold,
                identityComment,
                custom1,
                custom2,
                custom3,
                custom4
              });
            }
            if (stringList6.Count == 4)
            {
              if (this._showUsers)
              {
                stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  REINFORCED BY{" ".Repeat(count4 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  CREATED BY{" ".Repeat(count6 - 15)}  DETAILED DATE{" ".Repeat(count7 - 13)}  DETAILED BY{" ".Repeat(count8 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  RELEASED BY{" ".Repeat(count10 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  {str4.ToUpper()}{" ".Repeat(count15 - str4.Length)}  {str5.ToUpper()}{" ".Repeat(count16 - str5.Length)}  {str6.ToUpper()}{" ".Repeat(count17 - str6.Length)}  ");
                stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count4)}  {"=".Repeat(count5)}  {"=".Repeat(count6)}  {"=".Repeat(count7)}  {"=".Repeat(count8)}  {"=".Repeat(count9)}  {"=".Repeat(count10)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  {"=".Repeat(count15)}  {"=".Repeat(count16)}  {"=".Repeat(count17)}");
                using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    AssemblyViewModel current = enumerator.Current;
                    List<string> stringList7 = dictionary[current];
                    stringBuilder1.AppendLine($"{stringList7[0]}{" ".Repeat(count1 - stringList7[0].Length)}  {stringList7[1]}{" ".Repeat(count2 - stringList7[1].Length)}  {stringList7[2]}{" ".Repeat(count3 - stringList7[2].Length)}  {stringList7[3]}{" ".Repeat(count4 - stringList7[3].Length)}  {stringList7[4]}{" ".Repeat(count5 - stringList7[4].Length)}  {stringList7[5]}{" ".Repeat(count6 - stringList7[5].Length)}  {stringList7[6]}{" ".Repeat(count7 - stringList7[6].Length)}  {stringList7[7]}{" ".Repeat(count9 - stringList7[7].Length)}  {stringList7[8]}{" ".Repeat(count10 - stringList7[8].Length)}  {stringList7[9]}{" ".Repeat(count11 - stringList7[9].Length)}  {stringList7[10]}{" ".Repeat(count12 - stringList7[10].Length)}  {stringList7[11]}{" ".Repeat(count13 - stringList7[11].Length)}  {stringList7[12]}{" ".Repeat(count14 - stringList7[12].Length)}  {stringList7[13]}{" ".Repeat(count15 - stringList7[13].Length)}  {stringList7[14]}{" ".Repeat(count16 - stringList7[14].Length)}  {stringList7[15]}{" ".Repeat(count17 - stringList7[15].Length)}  ");
                  }
                  break;
                }
              }
              stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  DETAILED DATE{" ".Repeat(count7 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  {str4.ToUpper()}{" ".Repeat(count15 - str4.Length)}  {str5.ToUpper()}{" ".Repeat(count16 - str5.Length)}  {str6.ToUpper()}{" ".Repeat(count17 - str6.Length)}  ");
              stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count5)}  {"=".Repeat(count7)}  {"=".Repeat(count9)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  {"=".Repeat(count15)}  {"=".Repeat(count16)}  {"=".Repeat(count17)}  ");
              using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  AssemblyViewModel current = enumerator.Current;
                  List<string> stringList8 = dictionary[current];
                  stringBuilder1.AppendLine($"{stringList8[0]}{" ".Repeat(count1 - stringList8[0].Length)}  {stringList8[1]}{" ".Repeat(count2 - stringList8[1].Length)}  {stringList8[2]}{" ".Repeat(count3 - stringList8[2].Length)}  {stringList8[4]}{" ".Repeat(count5 - stringList8[4].Length)}  {stringList8[6]}{" ".Repeat(count7 - stringList8[6].Length)}  {stringList8[8]}{" ".Repeat(count9 - stringList8[8].Length)}  {stringList8[10]}{" ".Repeat(count11 - stringList8[10].Length)}  {stringList8[11]}{" ".Repeat(count12 - stringList8[11].Length)}  {stringList8[12]}{" ".Repeat(count13 - stringList8[12].Length)}  {stringList8[13]}{" ".Repeat(count14 - stringList8[13].Length)}  {stringList8[14]}{" ".Repeat(count15 - stringList8[14].Length)}  {stringList8[15]}{" ".Repeat(count16 - stringList8[15].Length)}  {stringList8[16 /*0x10*/]}{" ".Repeat(count17 - stringList8[16 /*0x10*/].Length)}  ");
                }
                break;
              }
            }
            if (stringList6.Count == 3)
            {
              if (this._showUsers)
              {
                stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  REINFORCED BY{" ".Repeat(count4 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  CREATED BY{" ".Repeat(count6 - 15)}  DETAILED DATE{" ".Repeat(count7 - 13)}  DETAILED BY{" ".Repeat(count8 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  RELEASED BY{" ".Repeat(count10 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  {str4.ToUpper()}{" ".Repeat(count15 - str4.Length)}  {str5.ToUpper()}{" ".Repeat(count16 - str5.Length)}  ");
                stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count4)}  {"=".Repeat(count5)}  {"=".Repeat(count6)}  {"=".Repeat(count7)}  {"=".Repeat(count8)}  {"=".Repeat(count9)}  {"=".Repeat(count10)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  {"=".Repeat(count15)}  {"=".Repeat(count16)}");
                using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    AssemblyViewModel current = enumerator.Current;
                    List<string> stringList9 = dictionary[current];
                    stringBuilder1.AppendLine($"{stringList9[0]}{" ".Repeat(count1 - stringList9[0].Length)}  {stringList9[1]}{" ".Repeat(count2 - stringList9[1].Length)}  {stringList9[2]}{" ".Repeat(count3 - stringList9[2].Length)}  {stringList9[3]}{" ".Repeat(count4 - stringList9[3].Length)}  {stringList9[4]}{" ".Repeat(count5 - stringList9[4].Length)}  {stringList9[5]}{" ".Repeat(count6 - stringList9[5].Length)}  {stringList9[6]}{" ".Repeat(count7 - stringList9[6].Length)}  {stringList9[7]}{" ".Repeat(count9 - stringList9[7].Length)}  {stringList9[8]}{" ".Repeat(count10 - stringList9[8].Length)}  {stringList9[9]}{" ".Repeat(count11 - stringList9[9].Length)}  {stringList9[10]}{" ".Repeat(count12 - stringList9[10].Length)}  {stringList9[11]}{" ".Repeat(count13 - stringList9[11].Length)}  {stringList9[12]}{" ".Repeat(count14 - stringList9[12].Length)}  {stringList9[13]}{" ".Repeat(count15 - stringList9[13].Length)}  {stringList9[14]}{" ".Repeat(count16 - stringList9[14].Length)}  ");
                  }
                  break;
                }
              }
              stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  DETAILED DATE{" ".Repeat(count7 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  {str4.ToUpper()}{" ".Repeat(count15 - str4.Length)}  {str5.ToUpper()}{" ".Repeat(count16 - str5.Length)}  ");
              stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count5)}  {"=".Repeat(count7)}  {"=".Repeat(count9)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  {"=".Repeat(count15)}  {"=".Repeat(count16)}  ");
              using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  AssemblyViewModel current = enumerator.Current;
                  List<string> stringList10 = dictionary[current];
                  stringBuilder1.AppendLine($"{stringList10[0]}{" ".Repeat(count1 - stringList10[0].Length)}  {stringList10[1]}{" ".Repeat(count2 - stringList10[1].Length)}  {stringList10[2]}{" ".Repeat(count3 - stringList10[2].Length)}  {stringList10[4]}{" ".Repeat(count5 - stringList10[4].Length)}  {stringList10[6]}{" ".Repeat(count7 - stringList10[6].Length)}  {stringList10[8]}{" ".Repeat(count9 - stringList10[8].Length)}  {stringList10[10]}{" ".Repeat(count11 - stringList10[10].Length)}  {stringList10[11]}{" ".Repeat(count12 - stringList10[11].Length)}  {stringList10[12]}{" ".Repeat(count13 - stringList10[12].Length)}  {stringList10[13]}{" ".Repeat(count14 - stringList10[13].Length)}  {stringList10[14]}{" ".Repeat(count15 - stringList10[14].Length)}  {stringList10[15]}{" ".Repeat(count16 - stringList10[15].Length)}  ");
                }
                break;
              }
            }
            if (stringList6.Count == 2)
            {
              if (this._showUsers)
              {
                stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  REINFORCED BY{" ".Repeat(count4 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  CREATED BY{" ".Repeat(count6 - 15)}  DETAILED DATE{" ".Repeat(count7 - 13)}  DETAILED BY{" ".Repeat(count8 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  RELEASED BY{" ".Repeat(count10 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  {str4.ToUpper()}{" ".Repeat(count15 - str4.Length)}  ");
                stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count4)}  {"=".Repeat(count5)}  {"=".Repeat(count6)}  {"=".Repeat(count7)}  {"=".Repeat(count8)}  {"=".Repeat(count9)}  {"=".Repeat(count10)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  {"=".Repeat(count15)}");
                using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    AssemblyViewModel current = enumerator.Current;
                    List<string> stringList11 = dictionary[current];
                    stringBuilder1.AppendLine($"{stringList11[0]}{" ".Repeat(count1 - stringList11[0].Length)}  {stringList11[1]}{" ".Repeat(count2 - stringList11[1].Length)}  {stringList11[2]}{" ".Repeat(count3 - stringList11[2].Length)}  {stringList11[3]}{" ".Repeat(count4 - stringList11[3].Length)}  {stringList11[4]}{" ".Repeat(count5 - stringList11[4].Length)}  {stringList11[5]}{" ".Repeat(count6 - stringList11[5].Length)}  {stringList11[6]}{" ".Repeat(count7 - stringList11[6].Length)}  {stringList11[7]}{" ".Repeat(count9 - stringList11[7].Length)}  {stringList11[8]}{" ".Repeat(count10 - stringList11[8].Length)}  {stringList11[9]}{" ".Repeat(count11 - stringList11[9].Length)}  {stringList11[10]}{" ".Repeat(count12 - stringList11[10].Length)}  {stringList11[11]}{" ".Repeat(count13 - stringList11[11].Length)}  {stringList11[12]}{" ".Repeat(count14 - stringList11[12].Length)}  {stringList11[13]}{" ".Repeat(count15 - stringList11[13].Length)}  ");
                  }
                  break;
                }
              }
              stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  DETAILED DATE{" ".Repeat(count7 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  {str4.ToUpper()}{" ".Repeat(count15 - str4.Length)}  ");
              stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count5)}  {"=".Repeat(count7)}  {"=".Repeat(count9)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  {"=".Repeat(count15)}  ");
              using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  AssemblyViewModel current = enumerator.Current;
                  List<string> stringList12 = dictionary[current];
                  stringBuilder1.AppendLine($"{stringList12[0]}{" ".Repeat(count1 - stringList12[0].Length)}  {stringList12[1]}{" ".Repeat(count2 - stringList12[1].Length)}  {stringList12[2]}{" ".Repeat(count3 - stringList12[2].Length)}  {stringList12[4]}{" ".Repeat(count5 - stringList12[4].Length)}  {stringList12[6]}{" ".Repeat(count7 - stringList12[6].Length)}  {stringList12[8]}{" ".Repeat(count9 - stringList12[8].Length)}  {stringList12[10]}{" ".Repeat(count11 - stringList12[10].Length)}  {stringList12[11]}{" ".Repeat(count12 - stringList12[11].Length)}  {stringList12[12]}{" ".Repeat(count13 - stringList12[12].Length)}  {stringList12[13]}{" ".Repeat(count14 - stringList12[13].Length)}  {stringList12[14]}{" ".Repeat(count15 - stringList12[14].Length)}  ");
                }
                break;
              }
            }
            if (stringList6.Count == 1)
            {
              if (this._showUsers)
              {
                stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  REINFORCED BY{" ".Repeat(count4 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  CREATED BY{" ".Repeat(count6 - 15)}  DETAILED DATE{" ".Repeat(count7 - 13)}  DETAILED BY{" ".Repeat(count8 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  RELEASED BY{" ".Repeat(count10 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  ");
                stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count4)}  {"=".Repeat(count5)}  {"=".Repeat(count6)}  {"=".Repeat(count7)}  {"=".Repeat(count8)}  {"=".Repeat(count9)}  {"=".Repeat(count10)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}");
                using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    AssemblyViewModel current = enumerator.Current;
                    List<string> stringList13 = dictionary[current];
                    stringBuilder1.AppendLine($"{stringList13[0]}{" ".Repeat(count1 - stringList13[0].Length)}  {stringList13[1]}{" ".Repeat(count2 - stringList13[1].Length)}  {stringList13[2]}{" ".Repeat(count3 - stringList13[2].Length)}  {stringList13[3]}{" ".Repeat(count4 - stringList13[3].Length)}  {stringList13[4]}{" ".Repeat(count5 - stringList13[4].Length)}  {stringList13[5]}{" ".Repeat(count6 - stringList13[5].Length)}  {stringList13[6]}{" ".Repeat(count7 - stringList13[6].Length)}  {stringList13[7]}{" ".Repeat(count9 - stringList13[7].Length)}  {stringList13[8]}{" ".Repeat(count10 - stringList13[8].Length)}  {stringList13[9]}{" ".Repeat(count11 - stringList13[9].Length)}  {stringList13[10]}{" ".Repeat(count12 - stringList13[10].Length)}  {stringList13[11]}{" ".Repeat(count13 - stringList13[11].Length)}  {stringList13[12]}{" ".Repeat(count14 - stringList13[12].Length)}  ");
                  }
                  break;
                }
              }
              stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  DETAILED DATE{" ".Repeat(count7 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}  {str3.ToUpper()}{" ".Repeat(count14 - str3.Length)}  ");
              stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count5)}  {"=".Repeat(count7)}  {"=".Repeat(count9)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}  {"=".Repeat(count14)}  ");
              using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  AssemblyViewModel current = enumerator.Current;
                  List<string> stringList14 = dictionary[current];
                  stringBuilder1.AppendLine($"{stringList14[0]}{" ".Repeat(count1 - stringList14[0].Length)}  {stringList14[1]}{" ".Repeat(count2 - stringList14[1].Length)}  {stringList14[2]}{" ".Repeat(count3 - stringList14[2].Length)}  {stringList14[4]}{" ".Repeat(count5 - stringList14[4].Length)}  {stringList14[6]}{" ".Repeat(count7 - stringList14[6].Length)}  {stringList14[8]}{" ".Repeat(count9 - stringList14[8].Length)}  {stringList14[10]}{" ".Repeat(count11 - stringList14[10].Length)}  {stringList14[11]}{" ".Repeat(count12 - stringList14[11].Length)}  {stringList14[12]}{" ".Repeat(count13 - stringList14[12].Length)}  {stringList14[13]}{" ".Repeat(count14 - stringList14[13].Length)}  ");
                }
                break;
              }
            }
            if (this._showUsers)
            {
              stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  REINFORCED BY{" ".Repeat(count4 - 13)}  CREATED DATE{" ".Repeat(count5 - 12)}  CREATED BY{" ".Repeat(count6 - 10)}  DETAILED DATE{" ".Repeat(count7 - 13)}  DETAILED BY{" ".Repeat(count8 - 11)}  RELEASED DATE{" ".Repeat(count9 - 13)}  RELEASED BY{" ".Repeat(count10 - 11)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}");
              stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count4)}  {"=".Repeat(count5)}  {"=".Repeat(count6)}  {"=".Repeat(count7)}  {"=".Repeat(count8)}  {"=".Repeat(count9)}  {"=".Repeat(count10)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}");
              using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  AssemblyViewModel current = enumerator.Current;
                  List<string> stringList15 = dictionary[current];
                  stringBuilder1.AppendLine($"{stringList15[0]}{" ".Repeat(count1 - stringList15[0].Length)}  {stringList15[1]}{" ".Repeat(count2 - stringList15[1].Length)}  {stringList15[2]}{" ".Repeat(count3 - stringList15[2].Length)}  {stringList15[3]}{" ".Repeat(count4 - stringList15[3].Length)}  {stringList15[4]}{" ".Repeat(count5 - stringList15[4].Length)}  {stringList15[5]}{" ".Repeat(count6 - stringList15[5].Length)}  {stringList15[6]}{" ".Repeat(count7 - stringList15[6].Length)}  {stringList15[7]}{" ".Repeat(count8 - stringList15[7].Length)}  {stringList15[8]}{" ".Repeat(count9 - stringList15[8].Length)}  {stringList15[9]}{" ".Repeat(count10 - stringList15[9].Length)}  {stringList15[10]}{" ".Repeat(count11 - stringList15[10].Length)}  {stringList15[11]}{" ".Repeat(count12 - stringList15[11].Length)}  {stringList15[12]}{" ".Repeat(count13 - stringList15[12].Length)}");
                }
                break;
              }
            }
            stringBuilder1.AppendLine($"MARK ID{" ".Repeat(count1 - 7)}  DESCRIPTION{" ".Repeat(count2 - 11)}  REINFORCED DATE{" ".Repeat(count3 - 15)}  CREATED DATE{" ".Repeat(count5 - 12)}  DETAILED DATE{" ".Repeat(count7 - 13)}  RELEASED DATE{" ".Repeat(count9 - 13)}  FLAGGED{" ".Repeat(count11 - 7)}  ON HOLD{" ".Repeat(count12 - 7)}  IDENTITY COMMENT{" ".Repeat(count13 - 16 /*0x10*/)}");
            stringBuilder1.AppendLine($"{"=".Repeat(count1)}  {"=".Repeat(count2)}  {"=".Repeat(count3)}  {"=".Repeat(count5)}  {"=".Repeat(count7)}  {"=".Repeat(count9)}  {"=".Repeat(count11)}  {"=".Repeat(count12)}  {"=".Repeat(count13)}");
            using (Dictionary<AssemblyViewModel, List<string>>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                AssemblyViewModel current = enumerator.Current;
                List<string> stringList16 = dictionary[current];
                stringBuilder1.AppendLine($"{stringList16[0]}{" ".Repeat(count1 - stringList16[0].Length)}  {stringList16[1]}{" ".Repeat(count2 - stringList16[1].Length)}  {stringList16[2]}{" ".Repeat(count3 - stringList16[2].Length)}  {stringList16[4]}{" ".Repeat(count5 - stringList16[4].Length)}  {stringList16[6]}{" ".Repeat(count7 - stringList16[6].Length)}  {stringList16[8]}{" ".Repeat(count9 - stringList16[8].Length)}  {stringList16[10]}{" ".Repeat(count11 - stringList16[10].Length)}  {stringList16[11]}{" ".Repeat(count12 - stringList16[11].Length)}  {stringList16[12]}{" ".Repeat(count13 - stringList16[12].Length)}");
              }
              break;
            }
          case ".csv":
            stringBuilder1.AppendLine(",");
            stringBuilder1.AppendLine($"Client:,{parameterAsString1},");
            string str12 = $"\"{parameterAsString2}\"";
            stringBuilder1.AppendLine($"Project:,{str12},");
            stringBuilder1.AppendLine(",");
            stringBuilder1.AppendLine($"Revit Model File:,,{this.CurrentModel},");
            stringBuilder1.AppendLine($"Report Date:,,{DateTime.Today.ToString("yyyy-MM-dd")},");
            stringBuilder1.AppendLine(",");
            StringBuilder stringBuilder4 = stringBuilder1;
            string[] strArray3 = new string[5]
            {
              "Total Pieces:,,",
              null,
              null,
              null,
              null
            };
            int num3 = this.TotalPieces;
            strArray3[1] = num3.ToString();
            strArray3[2] = ",,Detailed Marks:,,";
            num3 = this.MarksDetailed;
            strArray3[3] = num3.ToString();
            strArray3[4] = ",";
            string str13 = string.Concat(strArray3);
            stringBuilder4.AppendLine(str13);
            StringBuilder stringBuilder5 = stringBuilder1;
            string[] strArray4 = new string[5]
            {
              "Total Marks:,,",
              null,
              null,
              null,
              null
            };
            num3 = this.TotalMarks;
            strArray4[1] = num3.ToString();
            strArray4[2] = ",,Detailed Pieces:,,";
            num3 = this.PiecesDetailed;
            strArray4[3] = num3.ToString();
            strArray4[4] = ",";
            string str14 = string.Concat(strArray4);
            stringBuilder5.AppendLine(str14);
            stringBuilder1.AppendLine(",");
            StringBuilder stringBuilder6 = stringBuilder1;
            string[] strArray5 = new string[5]
            {
              "Reinforced Marks:,,",
              null,
              null,
              null,
              null
            };
            num3 = this.MarksReinforced;
            strArray5[1] = num3.ToString();
            strArray5[2] = ",,Released Marks:,,";
            num3 = this.MarksReleased;
            strArray5[3] = num3.ToString();
            strArray5[4] = ",";
            string str15 = string.Concat(strArray5);
            stringBuilder6.AppendLine(str15);
            StringBuilder stringBuilder7 = stringBuilder1;
            string[] strArray6 = new string[5]
            {
              "Reinforced Pieces:,,",
              null,
              null,
              null,
              null
            };
            num3 = this.PiecesReinforced;
            strArray6[1] = num3.ToString();
            strArray6[2] = ",,Released Pieces:,,";
            num3 = this.PiecesReleased;
            strArray6[3] = num3.ToString();
            strArray6[4] = ",";
            string str16 = string.Concat(strArray6);
            stringBuilder7.AppendLine(str16);
            stringBuilder1.AppendLine(",");
            string str17 = $"=\"{str2}\"";
            stringBuilder1.AppendLine($"List Filter:,,{str17},");
            stringBuilder1.AppendLine(",");
            stringBuilder1.AppendLine(",");
            if (this._showUsers)
              stringBuilder1.AppendLine($"MARK ID,,DESCRIPTION,{",".Repeat(3)}REINFORCED DATE,REINFORCED BY,,CREATED DATE,CREATED BY,DETAILED DATE,DETAILED BY,RELEASED DATE,RELEASED BY,FLAGGED,,ON HOLD,IDENTITY COMMENT,{str3.ToUpper()},{str4.ToUpper()},{str5.ToUpper()},{str6.ToUpper()}");
            else
              stringBuilder1.AppendLine($"MARK ID,,DESCRIPTION,{",".Repeat(3)}REINFORCED DATE,{",".Repeat(2)}CREATED DATE,,DETAILED DATE,,RELEASED DATE,,FLAGGED,,ON HOLD,IDENTITY COMMENT,{str3.ToUpper()},{str4.ToUpper()},{str5.ToUpper()},{str6.ToUpper()}");
            using (List<AssemblyViewModel>.Enumerator enumerator = this.AssemblyList.AssemblyList.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                AssemblyViewModel current = enumerator.Current;
                string str18 = !string.IsNullOrWhiteSpace(current.MarkNumber) ? current.MarkNumber : "INVALID,";
                string str19 = current.Description.Length > 50 ? current.Description.Substring(0, 45) + ".....," : current.Description + ",";
                string reinforcedDate = current.ReinforcedDate;
                string reinforcedBy = current.ReinforcedBy;
                string createdDate = current.CreatedDate;
                string createdBy = current.CreatedBy;
                string detailedDate = current.DetailedDate;
                string detailedBy = current.DetailedBy;
                string releasedDate = current.ReleasedDate;
                string releasedBy = current.ReleasedBy;
                string str20 = (current.IsFlagged || current.isInvalidQuantity || current.SheetFlagged).ToString();
                string onHold = current.OnHold;
                string identityComment = current.IdentityComment;
                string custom1 = current.Custom1;
                string custom2 = current.Custom2;
                string custom3 = current.Custom3;
                string custom4 = current.Custom4;
                if (this._showUsers)
                  stringBuilder1.AppendLine($"{str18},,{str19}{",".Repeat(3)}{reinforcedDate},{reinforcedBy}{",".Repeat(2)}{createdDate},{createdBy}{",".Repeat(1)}{detailedDate},{detailedBy}{",".Repeat(1)}{releasedDate},{releasedBy}{",".Repeat(1)}{str20}{",".Repeat(2)}{onHold},{identityComment},{custom1},{custom2},{custom3},{custom4}");
                else
                  stringBuilder1.AppendLine($"{str18},,{str19}{",".Repeat(3)}{reinforcedDate}{",".Repeat(3)}{createdDate}{",".Repeat(2)}{detailedDate}{",".Repeat(2)}{releasedDate}{",".Repeat(2)}{str20}{",".Repeat(2)}{onHold},{identityComment},{custom1},{custom2},{custom3},{custom4}");
              }
              break;
            }
          case ".xlsx":
            flag1 = false;
            source.Add(new List<object>()
            {
              (object) "Client:",
              (object) parameterAsString1
            });
            List<object> objectList1 = new List<object>();
            objectList1.Add((object) "Project:");
            string str21 = $"=\"{parameterAsString2}\"";
            objectList1.Add((object) str21);
            source.Add(objectList1);
            source.Add(new List<object>() { (object) "" });
            source.Add(new List<object>()
            {
              (object) "Revit Model File:",
              (object) "",
              (object) this.CurrentModel
            });
            source.Add(new List<object>()
            {
              (object) "Report Date:",
              (object) "",
              (object) DateTime.Today.ToString("yyyy-MM-dd")
            });
            source.Add(new List<object>()
            {
              (object) "Total Pieces:",
              (object) "",
              (object) this.TotalPieces,
              (object) "",
              (object) "Detailed Marks:",
              (object) "",
              (object) this.MarksDetailed
            });
            source.Add(new List<object>()
            {
              (object) "Total Marks:",
              (object) "",
              (object) this.TotalMarks,
              (object) "",
              (object) "Detailed Pieces:",
              (object) "",
              (object) this.PiecesDetailed
            });
            source.Add(new List<object>() { (object) "" });
            source.Add(new List<object>()
            {
              (object) "Reinforced Marks:",
              (object) "",
              (object) this.MarksReinforced,
              (object) "",
              (object) "Released Marks:",
              (object) "",
              (object) this.MarksReleased
            });
            source.Add(new List<object>()
            {
              (object) "Reinforced Pieces:",
              (object) "",
              (object) this.PiecesReinforced,
              (object) "",
              (object) "Released Pieces:",
              (object) "",
              (object) this.PiecesReleased
            });
            source.Add(new List<object>() { (object) "" });
            List<object> objectList2 = new List<object>();
            objectList2.Add((object) "List Filter:");
            string str22 = $"=\"{str2}\"";
            objectList2.Add((object) str22);
            source.Add(objectList2);
            source.Add(new List<object>() { (object) "" });
            List<object> objectList3 = new List<object>();
            objectList3.Add((object) "MARK ID");
            objectList3.Add((object) "");
            objectList3.Add((object) "DESCRIPTION");
            objectList3.Add((object) "");
            objectList3.Add((object) "");
            objectList3.Add((object) "");
            objectList3.Add((object) "REINFORCED DATE");
            if (this._showUsers)
            {
              objectList3.Add((object) "");
              objectList3.Add((object) "REINFORCED BY");
            }
            objectList3.Add((object) "");
            objectList3.Add((object) "");
            objectList3.Add((object) "CREATED DATE");
            if (this._showUsers)
            {
              objectList3.Add((object) "");
              objectList3.Add((object) "CREATED BY");
            }
            objectList3.Add((object) "");
            objectList3.Add((object) "DETAILED DATE");
            if (this._showUsers)
            {
              objectList3.Add((object) "");
              objectList3.Add((object) "DETAILED BY");
            }
            objectList3.Add((object) "");
            objectList3.Add((object) "RELEASED DATE");
            if (this._showUsers)
            {
              objectList3.Add((object) "");
              objectList3.Add((object) "RELEASED BY");
            }
            objectList3.Add((object) "");
            objectList3.Add((object) "FLAGGED");
            objectList3.Add((object) "");
            objectList3.Add((object) "ON HOLD");
            objectList3.Add((object) "");
            objectList3.Add((object) "IDENTITY COMMENT");
            objectList3.Add((object) "");
            objectList3.Add((object) str3.ToUpper());
            objectList3.Add((object) "");
            objectList3.Add((object) str4.ToUpper());
            objectList3.Add((object) "");
            objectList3.Add((object) str5.ToUpper());
            objectList3.Add((object) "");
            objectList3.Add((object) str6.ToUpper());
            objectList3.Add((object) "");
            source.Add(objectList3);
            using (List<AssemblyViewModel>.Enumerator enumerator = this.AssemblyList.AssemblyList.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                AssemblyViewModel current = enumerator.Current;
                string str23 = !string.IsNullOrWhiteSpace(current.MarkNumber) ? current.MarkNumber : "INVALID";
                string str24 = current.Description.Length > 50 ? current.Description.Substring(0, 45) : current.Description;
                string reinforcedDate = current.ReinforcedDate;
                string reinforcedBy = current.ReinforcedBy;
                string createdDate = current.CreatedDate;
                string createdBy = current.CreatedBy;
                string detailedDate = current.DetailedDate;
                string detailedBy = current.DetailedBy;
                string releasedDate = current.ReleasedDate;
                string releasedBy = current.ReleasedBy;
                string str25 = (current.IsFlagged || current.isInvalidQuantity || current.SheetFlagged).ToString();
                string onHold = current.OnHold;
                string identityComment = current.IdentityComment;
                string custom1 = current.Custom1;
                string custom2 = current.Custom2;
                string custom3 = current.Custom3;
                string custom4 = current.Custom4;
                List<object> objectList4 = new List<object>();
                objectList4.Add((object) str23);
                objectList4.Add((object) "");
                objectList4.Add((object) str24);
                objectList4.Add((object) "");
                objectList4.Add((object) "");
                objectList4.Add((object) "");
                objectList4.Add((object) reinforcedDate);
                if (this._showUsers)
                {
                  objectList4.Add((object) "");
                  objectList4.Add((object) reinforcedBy);
                }
                objectList4.Add((object) "");
                objectList4.Add((object) "");
                objectList4.Add((object) createdDate);
                if (this._showUsers)
                {
                  objectList4.Add((object) "");
                  objectList4.Add((object) createdBy);
                }
                objectList4.Add((object) "");
                objectList4.Add((object) detailedDate);
                if (this._showUsers)
                {
                  objectList4.Add((object) "");
                  objectList4.Add((object) detailedBy);
                }
                objectList4.Add((object) "");
                objectList4.Add((object) releasedDate);
                if (this._showUsers)
                {
                  objectList4.Add((object) "");
                  objectList4.Add((object) releasedBy);
                }
                objectList4.Add((object) "");
                objectList4.Add((object) str25);
                objectList4.Add((object) "");
                objectList4.Add((object) onHold);
                objectList4.Add((object) "");
                objectList4.Add((object) identityComment);
                objectList4.Add((object) "");
                objectList4.Add((object) custom1);
                objectList4.Add((object) "");
                objectList4.Add((object) custom2);
                objectList4.Add((object) "");
                objectList4.Add((object) custom3);
                objectList4.Add((object) "");
                objectList4.Add((object) custom4);
                objectList4.Add((object) "");
                source.Add(objectList4);
              }
              break;
            }
        }
        if (File.Exists(fileName))
        {
          string str26 = fileName;
          char[] charArray = fileName.ToCharArray();
          int num4 = str26.LastIndexOf("\\");
          StringBuilder stringBuilder8 = new StringBuilder();
          for (int index = num4 + 1; index < charArray.Length; ++index)
            stringBuilder8.Append(charArray[index].ToString());
          string wbook = stringBuilder8.ToString();
          if (wbook.Contains("txt") && this.IsOpen(wbook))
          {
            int num5 = (int) MessageBox.Show("The file you attempted to save is being used by another process. Please close the file before saving it.", "Warning");
            return;
          }
        }
        if (flag1)
        {
          using (StreamWriter streamWriter = new StreamWriter(fileName))
            streamWriter.WriteLine(stringBuilder1.ToString());
        }
        else
        {
          ExcelDocument excelDocument = new ExcelDocument(fileName);
          if (excelDocument.ExcelFileOpen)
          {
            ((IEnumerable<object>) ((IEnumerable<object>) source).ToArray<object>()).Count<object>();
            int rowId = 1;
            foreach (List<object> objectList5 in source)
            {
              int columnId = 1;
              foreach (object obj in objectList5)
              {
                bool flag2 = true;
                switch (obj)
                {
                  case string str27:
                    flag2 = excelDocument.UpdateCellValue(columnId, rowId, str27);
                    break;
                  case int num6:
                    flag2 = excelDocument.UpdateCellValue(columnId, rowId, (double) num6);
                    break;
                  case double num7:
                    flag2 = excelDocument.UpdateCellValue(columnId, rowId, num7);
                    break;
                  case DateTime dateTime:
                    flag2 = excelDocument.GetNumberingFormatIndex("YYYY-MM-DD") >= 0 && excelDocument.UpdateCellValue(columnId, rowId, dateTime);
                    break;
                  case bool flag3:
                    flag2 = excelDocument.UpdateCellValue(columnId, rowId, flag3);
                    break;
                }
                int num8 = flag2 ? 1 : 0;
                ++columnId;
              }
              ++rowId;
            }
            excelDocument.SaveAndClose();
          }
        }
        TaskDialog.Show("Message", $"The file has been exported to {fileName}.");
      }
    }
    catch (Exception ex)
    {
      if (ex == null)
        return;
      TaskDialog.Show("Warning", ex.ToString());
      throw;
    }
  }

  public void ExecuteExport()
  {
    this.MarksReinforced = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReinforcedDate.Equals(""))).ToList<AssemblyViewModel>().Count;
    this.PiecesReinforced = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReinforcedDate.Equals(""))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
    this.MarksDetailed = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.DetailedDate.Equals(""))).ToList<AssemblyViewModel>().Count;
    this.PiecesDetailed = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.DetailedDate.Equals(""))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
    this.MarksReleased = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReleasedDate.Equals(""))).ToList<AssemblyViewModel>().Count;
    this.PiecesReleased = this.AssemblyList.AssemblyList.Where<AssemblyViewModel>((Func<AssemblyViewModel, bool>) (model => !model.ReleasedDate.Equals(""))).Select<AssemblyViewModel, int>((Func<AssemblyViewModel, int>) (model => model.Quantity)).Sum();
    this.TotalPieces = this.AssemblyList.TotalPieces;
  }

  [DebuggerHidden]
  public bool CanExecuteViewHistory(object parameter) => true;

  public void ExecuteViewHistory(object parameter)
  {
    try
    {
      List<AssemblyViewModel> list = App.TicketManagerWindow.DataGrid.SelectedItems.Cast<AssemblyViewModel>().ToList<AssemblyViewModel>();
      if (list.ToList<AssemblyViewModel>().Count > 1)
      {
        int num = (int) MessageBox.Show("You can only view one Ticket Edit History at a time. Ticket Edit History will be provided for the first assembly you selected.", "Warning");
      }
      AssemblyViewModel model = list.First<AssemblyViewModel>();
      if (model.Assemblied == "No")
      {
        new TaskDialog("View History Error")
        {
          Id = "View_History_Status",
          Title = "View History Warning",
          MainIcon = ((TaskDialogIcon) (int) ushort.MaxValue),
          TitleAutoPrefix = true,
          AllowCancellation = true,
          MainContent = $"The Element with MarkNumber: {model.MarkNumber} is not an Assembly yet",
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
        }.Show();
      }
      else
      {
        if (model == new AssemblyViewModel())
          return;
        IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        new HistoryWindow(model, mainWindowHandle).ShowDialog();
        this.AssemblyList = new AssemblyListViewModel();
        this.Search = this.Search;
      }
    }
    catch (Exception ex)
    {
      if (!ex.ToString().Contains("contains no element"))
        return;
      TaskDialog.Show("Warning", "Please select the content in the ticket manager list to review!");
    }
  }

  [DebuggerHidden]
  public bool CanExecuteShowViewSheet(object parameter) => true;

  public void ExecuteShowViewSheet(object parameter)
  {
    if (!(parameter is ViewSheet))
      return;
    ActiveModel.UIDoc.ActiveView = (View) (parameter as ViewSheet);
  }

  [DebuggerHidden]
  public bool CanExecuteAcceptEdits(object parameter) => true;

  public void ExecuteAcceptEdits(object parameter) => this.AcceptChangesEvent.Raise();

  [DebuggerHidden]
  public bool CanExecuteReleaseTicket(object parameter) => true;

  public void ExecuteReleaseTicket(object parameter) => this.ReleaseTicketEvent.Raise();

  [DebuggerHidden]
  public bool CanExecuteRefresh(object parameter) => true;

  public void ExecuteButtonRefresh(object parameter)
  {
    MainWindow.needToRefreshSettingFile = true;
    ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
  }

  public void ExecuteRefresh(object parameter)
  {
    List<SortDescription> sortDescriptionList = new List<SortDescription>();
    foreach (SortDescription sortDescription in (Collection<SortDescription>) this.getWindow.DataGrid.Items.SortDescriptions)
      sortDescriptionList.Add(sortDescription);
    if (this.getWindow.DataGrid.Columns[0].Header.ToString() == "Mark #")
    {
      ListSortDirection? sortDirection = this.getWindow.DataGrid.Columns[0].SortDirection;
      ListSortDirection listSortDirection = ListSortDirection.Ascending;
      if (sortDirection.GetValueOrDefault() == listSortDirection & sortDirection.HasValue)
      {
        sortDescriptionList.Clear();
        sortDescriptionList.Add(new SortDescription(this.getWindow.DataGrid.Columns[0].SortMemberPath, ListSortDirection.Ascending));
      }
      else if (this.getWindow.DataGrid.Columns[0].SortDirection.GetValueOrDefault() == ListSortDirection.Descending)
      {
        sortDescriptionList.Clear();
        sortDescriptionList.Add(new SortDescription(this.getWindow.DataGrid.Columns[0].SortMemberPath, ListSortDirection.Descending));
      }
    }
    if (this.mDoc.Application.ActiveUIDocument.Document.IsFamilyDocument)
    {
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Ticket Manager must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Ticket Manager must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
      this.IsInFamilyEditor = true;
    }
    else
    {
      this.CurrentModel = this.mDoc.Application.ActiveUIDocument.Document.PathName;
      this.WindowTitle = "EDGE^R Ticket Manager - " + this.CurrentModel;
      this.WriteCommentsEvent.Raise();
      this.AssemblyList = new AssemblyListViewModel();
      this.TotalPieces = this.AssemblyList.TotalPieces;
      this.TotalMarks = this.AssemblyList.TotalMarks;
      this.Search = this.Search;
      this.SearchCount = this.SearchCount;
      this.getWindow.DataGrid.Items.SortDescriptions.Clear();
      foreach (SortDescription sortDescription in sortDescriptionList)
        this.getWindow.DataGrid.Items.SortDescriptions.Add(sortDescription);
    }
  }

  public bool CanExecutePopulator(object parameter) => true;

  public void ExecutePopulator(object parameter) => this.TicketPopulatorEvent.Raise();

  public bool CanExecuteWriteComment(object parameter) => true;

  public void ExecuteWriteComment(object parameter) => this.WriteCommentsEvent.Raise();

  public bool CanExecuteSendToCloud(object parameter) => true;

  public void ExecuteSendToCloud(object parameter) => this.SendToCloudEvent.Raise();

  public bool CanExecuteUpdateParameters(object parameter) => true;

  public void ExecuteUpdateParameters(object parameter) => this.UpdateParametersEvent.Raise();

  public event PropertyChangedEventHandler PropertyChanged;

  [DebuggerHidden]
  protected void OnPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }

  public MainViewModel() => this.AssemblyList = new AssemblyListViewModel();

  public MainViewModel(MainWindow window, UIDocument myDocky)
  {
    this.getWindow = window;
    this.CloseCommand = new Command(new Command.ICommandOnExecute(this.ExecuteClose), new Command.ICommandOnCanExecute(this.CanExecuteClose));
    this.ExportCommand = new Command(new Command.ICommandOnExecute(this.ExecuteExport), new Command.ICommandOnCanExecute(this.CanExecuteExport));
    this.ViewHistoryCommand = new Command(new Command.ICommandOnExecute(this.ExecuteViewHistory), new Command.ICommandOnCanExecute(this.CanExecuteViewHistory));
    this.ShowViewSheetCommand = new Command(new Command.ICommandOnExecute(this.ExecuteShowViewSheet), new Command.ICommandOnCanExecute(this.CanExecuteShowViewSheet));
    this.AcceptEditsCommand = new Command(new Command.ICommandOnExecute(this.ExecuteAcceptEdits), new Command.ICommandOnCanExecute(this.CanExecuteAcceptEdits));
    this.ReleaseTicketCommand = new Command(new Command.ICommandOnExecute(this.ExecuteReleaseTicket), new Command.ICommandOnCanExecute(this.CanExecuteReleaseTicket));
    this.RefreshCommand = new Command(new Command.ICommandOnExecute(this.ExecuteRefresh), new Command.ICommandOnCanExecute(this.CanExecuteRefresh));
    this.RefreshButtonCommand = new Command(new Command.ICommandOnExecute(this.ExecuteButtonRefresh), new Command.ICommandOnCanExecute(this.CanExecuteRefresh));
    this.PopulatorCommand = new Command(new Command.ICommandOnExecute(this.ExecutePopulator), new Command.ICommandOnCanExecute(this.CanExecutePopulator));
    this.WriteCommentCommand = new Command(new Command.ICommandOnExecute(this.ExecuteWriteComment), new Command.ICommandOnCanExecute(this.CanExecuteWriteComment));
    this.SendToCloudCommand = new Command(new Command.ICommandOnExecute(this.ExecuteSendToCloud), new Command.ICommandOnCanExecute(this.CanExecuteSendToCloud));
    this.UpdateParametersCommand = new Command(new Command.ICommandOnExecute(this.ExecuteUpdateParameters), new Command.ICommandOnCanExecute(this.CanExecuteUpdateParameters));
    this.AssemblyList = new AssemblyListViewModel();
    this.Search = string.Empty;
    this.TotalPieces = this.AssemblyList.TotalPieces;
    this.TotalMarks = this.AssemblyList.TotalMarks;
    this.ShowUsers = false;
    this.mDoc = myDocky;
    this.CurrentModel = myDocky.Document.PathName;
    this.WindowTitle = "EDGE^R Ticket Manager - " + this.CurrentModel;
    this.view = CollectionViewSource.GetDefaultView((object) this.getWindow.DataGrid.ItemsSource);
  }
}
