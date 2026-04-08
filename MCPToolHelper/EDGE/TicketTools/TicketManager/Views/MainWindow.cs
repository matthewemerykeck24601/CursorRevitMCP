// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.Views.MainWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.IUpdaters.ModelLocking;
using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.Views;

public class MainWindow : Window, IComponentConnector
{
  public UIDocument myDocky;
  private UIApplication app;
  public List<string> paramNames = new List<string>();
  public List<string> friendlyNames = new List<string>();
  public List<string> projParams = new List<string>();
  public List<string> readOnlyColumnNames = new List<string>();
  public string sampleElemId = "";
  public bool closed;
  public DateTime previousTime = DateTime.Now;
  public string pathName = "";
  public string CurrentManufacturer = "";
  public string LastManufacturerRefreshed = "";
  public static bool needToRefreshSettingFile;
  internal System.Windows.Controls.Grid GridForm;
  internal TextBox SearchBox;
  internal DataGrid DataGrid;
  internal ContextMenu ContextMenu;
  internal MenuItem OnHoldToggle;
  internal MenuItem IdentCommentEdit;
  internal MenuItem EditButton;
  internal MenuItem SheetsMenuItem;
  internal MenuItem RefreshButton;
  internal DataGridTextColumn ReinforcedByColumn;
  internal DataGridTextColumn CreatedByColumn;
  internal DataGridTextColumn DetailedByColumn;
  internal DataGridTextColumn ReleasedByColumn;
  internal DataGridTextColumn OnHoldColumn;
  internal DataGridTextColumn IdentityCommentColumn;
  internal DataGridTextColumn Custom1Column;
  internal DataGridTextColumn Custom2Column;
  internal DataGridTextColumn Custom3Column;
  internal DataGridTextColumn Custom4Column;
  internal DataGridTextColumn ElementIdColumn;
  private bool _contentLoaded;

  public MainWindow()
  {
    this.InitializeComponent();
    this.DataContext = (object) new MainViewModel(this, this.myDocky);
  }

  public MainWindow(
    ExternalEvent[] externalEvents,
    UIApplication application,
    IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    this.previousTime = DateTime.Now;
    this.DataGrid.Sorting += new DataGridSortingEventHandler(this.DataGrid_Sorting);
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.app = application;
    this.myDocky = application.ActiveUIDocument;
    Document document = application.ActiveUIDocument.Document;
    string str1 = "";
    MainViewModel mainViewModel = new MainViewModel(this, this.myDocky)
    {
      AcceptChangesEvent = externalEvents[0],
      ReleaseTicketEvent = externalEvents[1],
      WriteCommentsEvent = externalEvents[2],
      TicketPopulatorEvent = externalEvents[3],
      UpdateParametersEvent = externalEvents[4],
      getWindow = this
    };
    this.DataContext = (object) mainViewModel;
    System.Windows.Data.Binding binding = new System.Windows.Data.Binding()
    {
      Converter = (IValueConverter) new BooleanToVisibilityConverter(),
      Source = (object) mainViewModel,
      Path = new PropertyPath("ShowUsers", Array.Empty<object>())
    };
    BindingOperations.SetBinding((DependencyObject) this.ReinforcedByColumn, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.CreatedByColumn, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.DetailedByColumn, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.ReleasedByColumn, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.Custom1Column, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.Custom2Column, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.Custom3Column, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    BindingOperations.SetBinding((DependencyObject) this.Custom4Column, DataGridColumn.VisibilityProperty, (BindingBase) binding);
    this.SheetsMenuItem.DataContext = (object) mainViewModel;
    this.OnHoldToggle.Visibility = System.Windows.Visibility.Collapsed;
    this.IdentCommentEdit.Visibility = System.Windows.Visibility.Collapsed;
    this.EditButton.Visibility = System.Windows.Visibility.Visible;
    this.DataGrid.Columns[11].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[12].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[13].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[14].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[15].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[16 /*0x10*/].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[17].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[18].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[19].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[20].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[21].Visibility = System.Windows.Visibility.Collapsed;
    this.DataGrid.Columns[22].Visibility = System.Windows.Visibility.Collapsed;
    bool flag1 = false;
    bool flag2 = false;
    DefinitionBindingMapIterator bindingMapIterator1 = application.ActiveUIDocument.Document.ParameterBindings.ForwardIterator();
    while (bindingMapIterator1.MoveNext())
    {
      if (bindingMapIterator1.Key.Name.Equals("IDENTITY_COMMENT"))
        flag1 = true;
      if (bindingMapIterator1.Key.Name.Equals("ON_HOLD"))
        flag2 = true;
    }
    if (mainViewModel.AssemblyList.AssemblyList.Count > 0)
      this.sampleElemId = mainViewModel.AssemblyList.AssemblyList.First<AssemblyViewModel>().ElementId.ToString();
    this.readOnlyColumnNames.AddRange((IEnumerable<string>) new List<string>()
    {
      "Mark #",
      "Assembled",
      "Quantity",
      "Reinforced Date",
      "Created Date",
      "Detailed Date",
      "Date Released"
    });
    if (flag2)
    {
      this.DataGrid.Columns[11].Visibility = System.Windows.Visibility.Visible;
      this.DataGrid.Columns[11].IsReadOnly = false;
      this.OnHoldToggle.Visibility = System.Windows.Visibility.Visible;
      if (this.CheckReadOnly(document, this.sampleElemId, "ON_HOLD"))
        this.readOnlyColumnNames.Add(this.DataGrid.Columns[11].Header.ToString());
    }
    if (flag1)
    {
      this.DataGrid.Columns[13].Visibility = System.Windows.Visibility.Visible;
      this.DataGrid.Columns[13].IsReadOnly = false;
      this.IdentCommentEdit.Visibility = System.Windows.Visibility.Visible;
      if (this.CheckReadOnly(document, this.sampleElemId, "IDENTITY_COMMENT"))
        this.readOnlyColumnNames.Add(this.DataGrid.Columns[13].Header.ToString());
    }
    string str2 = App.TMCFolderPath;
    if (str2.Equals(""))
      str2 = "C:\\EDGEforRevit";
    string str3 = "";
    Parameter parameter = application.ActiveUIDocument.Document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
    {
      str3 = parameter.AsString();
      this.CurrentManufacturer = str3;
    }
    try
    {
      if (!str3.Equals("") || str3 != null)
      {
        if (File.Exists($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt"))
        {
          int num = new FileInfo($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
          str1 = $"{str2}\\{str3}_TicketManagerCustomizationSettings.txt";
        }
        else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
        {
          int num = new FileInfo(str2 + "\\TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
          str1 = str2 + "\\TicketManagerCustomizationSettings.txt";
        }
      }
      else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
      {
        int num = new FileInfo(str2 + "\\TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
        str1 = str2 + "\\TicketManagerCustomizationSettings.txt";
      }
    }
    catch
    {
      this.Close();
      new TaskDialog("Ticket Manager Settings File Error")
      {
        MainInstruction = "Ticket Manager Settings File is unavailable.",
        MainContent = $"Unable to read in the Ticket Manager Settings File available at {str1}. Please ensure the file is available to be read and not in use by another application and try again."
      }.Show();
      this.closed = true;
      return;
    }
    this.friendlyNames = new List<string>();
    this.paramNames = new List<string>();
    if (!str3.Equals("") || str3 != null)
    {
      if (File.Exists($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          this.friendlyNames.Add(strArray[0].Trim());
          this.paramNames.Add(strArray[1].Trim());
        }
      }
      else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines(str2 + "\\TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          this.friendlyNames.Add(strArray[0].Trim());
          this.paramNames.Add(strArray[1].Trim());
        }
      }
    }
    else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
    {
      foreach (string readAllLine in File.ReadAllLines(str2 + "\\TicketManagerCustomizationSettings.txt"))
      {
        char[] chArray = new char[1]{ ':' };
        string[] strArray = readAllLine.Split(chArray)[1].Split('|');
        this.friendlyNames.Add(strArray[0].Trim());
        this.paramNames.Add(strArray[1].Trim());
      }
    }
    this.projParams = new List<string>();
    List<string> stringList = new List<string>()
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
    DefinitionBindingMapIterator bindingMapIterator2 = document.ParameterBindings.ForwardIterator();
    while (bindingMapIterator2.MoveNext())
    {
      InstanceBinding current1 = bindingMapIterator2.Current as InstanceBinding;
      TypeBinding current2 = bindingMapIterator2.Current as TypeBinding;
      if (bindingMapIterator2.Current != null)
      {
        CategorySet categories;
        if (bindingMapIterator2.Current is InstanceBinding)
          categories = current1.Categories;
        else if (bindingMapIterator2.Current is TypeBinding)
          categories = current2.Categories;
        else
          continue;
        foreach (Category category in categories)
        {
          if ((category.Name.Equals("Assemblies") || category.Name.Equals("Structural Framing")) && !this.projParams.Contains(bindingMapIterator2.Key.Name) && !stringList.Contains(bindingMapIterator2.Key.Name.ToUpper()) && (bindingMapIterator2.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator2.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
            this.projParams.Add(bindingMapIterator2.Key.Name);
        }
      }
      if (this.friendlyNames.Count > 0 && this.projParams.Contains(this.paramNames[0]))
      {
        this.DataGrid.Columns[15].Header = (object) this.friendlyNames[0].Substring(0, Math.Min(this.friendlyNames[0].Length, 25));
        this.DataGrid.Columns[15].Visibility = System.Windows.Visibility.Visible;
        if (!this.sampleElemId.Equals("") && this.CheckReadOnly(document, this.sampleElemId, this.paramNames[0]))
          this.readOnlyColumnNames.Add(this.DataGrid.Columns[15].Header.ToString());
        this.DataGrid.Columns[16 /*0x10*/].Header = (object) this.friendlyNames[0].Substring(0, Math.Min(this.friendlyNames[0].Length, 25));
      }
      if (this.friendlyNames.Count > 1 && this.projParams.Contains(this.paramNames[1]))
      {
        this.DataGrid.Columns[17].Header = (object) this.friendlyNames[1].Substring(0, Math.Min(this.friendlyNames[1].Length, 25));
        this.DataGrid.Columns[17].Visibility = System.Windows.Visibility.Visible;
        if (!this.sampleElemId.Equals("") && this.CheckReadOnly(document, this.sampleElemId, this.paramNames[1]))
          this.readOnlyColumnNames.Add(this.DataGrid.Columns[17].Header.ToString());
        this.DataGrid.Columns[18].Header = (object) this.friendlyNames[1].Substring(0, Math.Min(this.friendlyNames[1].Length, 25));
      }
      if (this.friendlyNames.Count > 2 && this.projParams.Contains(this.paramNames[2]))
      {
        this.DataGrid.Columns[19].Header = (object) this.friendlyNames[2].Substring(0, Math.Min(this.friendlyNames[2].Length, 25));
        this.DataGrid.Columns[19].Visibility = System.Windows.Visibility.Visible;
        if (!this.sampleElemId.Equals("") && this.CheckReadOnly(document, this.sampleElemId, this.paramNames[2]))
          this.readOnlyColumnNames.Add(this.DataGrid.Columns[19].Header.ToString());
        this.DataGrid.Columns[20].Header = (object) this.friendlyNames[2].Substring(0, Math.Min(this.friendlyNames[2].Length, 25));
      }
      if (this.friendlyNames.Count > 3 && this.projParams.Contains(this.paramNames[3]))
      {
        this.DataGrid.Columns[21].Header = (object) this.friendlyNames[3].Substring(0, Math.Min(this.friendlyNames[3].Length, 25));
        this.DataGrid.Columns[21].Visibility = System.Windows.Visibility.Visible;
        if (!this.sampleElemId.Equals("") && this.CheckReadOnly(document, this.sampleElemId, this.paramNames[3]))
          this.readOnlyColumnNames.Add(this.DataGrid.Columns[21].Header.ToString());
        this.DataGrid.Columns[22].Header = (object) this.friendlyNames[3].Substring(0, Math.Min(this.friendlyNames[3].Length, 25));
      }
      this.pathName = str1;
      this.ShowHideCustomParamsOption();
    }
  }

  private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
  {
    if (!e.Column.Equals((object) this.DataGrid.Columns[0]))
      return;
    DataGridColumn column = e.Column;
    e.Handled = true;
    this.SortByMark(column);
  }

  private void SortByMark(DataGridColumn column, ListSortDirection? optional = null)
  {
    ListSortDirection listSortDirection1;
    if (!optional.HasValue)
    {
      ListSortDirection? sortDirection = column.SortDirection;
      ListSortDirection listSortDirection2 = ListSortDirection.Ascending;
      listSortDirection1 = !(sortDirection.GetValueOrDefault() == listSortDirection2 & sortDirection.HasValue) ? ListSortDirection.Ascending : ListSortDirection.Descending;
    }
    else
      listSortDirection1 = optional.Value;
    column.SortDirection = new ListSortDirection?(listSortDirection1);
    ListCollectionView defaultView = (ListCollectionView) CollectionViewSource.GetDefaultView((object) this.DataGrid.ItemsSource);
    Comparer<AssemblyViewModel> comparer1 = Comparer<AssemblyViewModel>.Create((Comparison<AssemblyViewModel>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.MarkNumber, q.MarkNumber)));
    if (listSortDirection1 != ListSortDirection.Ascending)
      comparer1 = Comparer<AssemblyViewModel>.Create((Comparison<AssemblyViewModel>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(q.MarkNumber, p.MarkNumber)));
    Comparer<AssemblyViewModel> comparer2 = comparer1;
    defaultView.CustomSort = (IComparer) comparer2;
  }

  private bool CheckReadOnly(Document revitDoc, string elemId, string paramName)
  {
    bool flag = false;
    int result = 0;
    int.TryParse(elemId, out result);
    ElementId id = new ElementId(result);
    Element element = revitDoc.GetElement(id);
    if (element != null)
    {
      Parameter parameter = Utils.ElementUtils.Parameters.LookupParameter(element, paramName);
      if (parameter != null)
        flag = parameter.IsReadOnly;
    }
    return flag;
  }

  private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    List<AssemblyViewModel> list = (sender as DataGrid).SelectedItems.Cast<AssemblyViewModel>().ToList<AssemblyViewModel>();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (AssemblyViewModel assemblyViewModel in (IEnumerable<AssemblyViewModel>) list)
    {
      if (assemblyViewModel.Assembly != null)
      {
        foreach (Element element in assemblyViewModel.SFElementsOfThisControlMark)
          elementIdList.Add(element.Id);
        assemblyViewModel.isSelected = true;
      }
      else
      {
        assemblyViewModel.isSelected = true;
        foreach (ElementId structuralFramingElemId in assemblyViewModel.StructuralFramingElemIds)
          elementIdList.Add(structuralFramingElemId);
      }
    }
    try
    {
      this.app.ActiveUIDocument.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
    }
  }

  private void App_Activated(object sender, EventArgs e)
  {
    if (this.app.ActiveUIDocument.Document.IsFamilyDocument)
    {
      this.Close();
      new TaskDialog("Family Editor")
      {
        AllowCancellation = false,
        CommonButtons = ((TaskDialogCommonButtons) 1),
        MainInstruction = "Ticket Manager must be run in the Project Environment",
        MainContent = "You are currently in the family editor, Ticket Manager must be run in the project environment.  Please return to the project environment or open a project before running this tool.",
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
      }.Show();
    }
    else
    {
      try
      {
        if (DateTime.Now.Subtract(this.previousTime).TotalSeconds < 0.25)
          return;
        this.previousTime = DateTime.Now;
        App.TicketManagerWindow.Topmost = true;
        ActiveModel.GetInformation(this.app.ActiveUIDocument);
        string propertyName = (string) null;
        ListSortDirection? optional = new ListSortDirection?();
        int index = 0;
        foreach (DataGridColumn column in (Collection<DataGridColumn>) this.DataGrid.Columns)
        {
          if (column.SortDirection.HasValue)
          {
            optional = column.SortDirection;
            propertyName = column.SortMemberPath;
            break;
          }
          ++index;
        }
        try
        {
          ((MainViewModel) App.TicketManagerWindow.DataContext).RefreshCommand.Execute((object) null);
          if (((MainViewModel) App.TicketManagerWindow.DataContext).IsInFamilyEditor)
          {
            this.Close();
            return;
          }
          this.Title = ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle;
          if (!string.IsNullOrEmpty(propertyName) && optional.HasValue)
          {
            SortDescription sortDescription = new SortDescription(propertyName, optional.Value);
            CollectionView defaultView = (CollectionView) CollectionViewSource.GetDefaultView((object) this.DataGrid.ItemsSource);
            if (index != 0)
              defaultView.SortDescriptions.Add(sortDescription);
            else
              this.SortByMark(this.DataGrid.Columns[index], optional);
            this.DataGrid.Columns[index].SortDirection = optional;
          }
          string str1 = "";
          if (!MainWindow.needToRefreshSettingFile && this.app.ActiveUIDocument.Document != null)
          {
            Parameter parameter = this.app.ActiveUIDocument.Document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
            if (parameter != null)
              str1 = parameter.AsString();
            else
              MainWindow.needToRefreshSettingFile = true;
            if (this.CurrentManufacturer != str1 || str1 != this.LastManufacturerRefreshed)
              MainWindow.needToRefreshSettingFile = true;
          }
          if (MainWindow.needToRefreshSettingFile)
          {
            MainWindow.needToRefreshSettingFile = false;
            string str2 = App.TMCFolderPath;
            if (str2.Equals(""))
              str2 = "C:\\EDGEforRevit";
            string str3 = "";
            string str4 = "";
            Parameter parameter = this.app.ActiveUIDocument.Document.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
            if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
              str3 = parameter.AsString();
            this.LastManufacturerRefreshed = str3;
            try
            {
              string str5;
              if (!str3.Equals("") || str3 != null)
              {
                if (File.Exists($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt"))
                {
                  int num = new FileInfo($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
                  str5 = $"{str2}\\{str3}_TicketManagerCustomizationSettings.txt";
                }
                else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
                {
                  int num = new FileInfo(str2 + "\\TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
                  str5 = str2 + "\\TicketManagerCustomizationSettings.txt";
                }
              }
              else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
              {
                int num = new FileInfo(str2 + "\\TicketManagerCustomizationSettings.txt").IsReadOnly ? 1 : 0;
                str5 = str2 + "\\TicketManagerCustomizationSettings.txt";
              }
            }
            catch
            {
              this.Close();
              new TaskDialog("Ticket Manager Settings File Error")
              {
                MainInstruction = "Ticket Manager Settings File is unavailable.",
                MainContent = $"Unable to read in the Ticket Manager Settings File available at {str4}. Please ensure the file is available to be read and not in use by another application and try again."
              }.Show();
              this.closed = true;
              return;
            }
            this.friendlyNames = new List<string>();
            this.paramNames = new List<string>();
            if (!str3.Equals("") || str3 != null)
            {
              if (File.Exists($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt"))
              {
                foreach (string readAllLine in File.ReadAllLines($"{str2}\\{str3}_TicketManagerCustomizationSettings.txt"))
                {
                  char[] chArray = new char[1]{ ':' };
                  string[] strArray = readAllLine.Split(chArray)[1].Split('|');
                  this.friendlyNames.Add(strArray[0].Trim());
                  this.paramNames.Add(strArray[1].Trim());
                }
              }
              else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
              {
                foreach (string readAllLine in File.ReadAllLines(str2 + "\\TicketManagerCustomizationSettings.txt"))
                {
                  char[] chArray = new char[1]{ ':' };
                  string[] strArray = readAllLine.Split(chArray)[1].Split('|');
                  this.friendlyNames.Add(strArray[0].Trim());
                  this.paramNames.Add(strArray[1].Trim());
                }
              }
            }
            else if (File.Exists(str2 + "\\TicketManagerCustomizationSettings.txt"))
            {
              foreach (string readAllLine in File.ReadAllLines(str2 + "\\TicketManagerCustomizationSettings.txt"))
              {
                char[] chArray = new char[1]{ ':' };
                string[] strArray = readAllLine.Split(chArray)[1].Split('|');
                this.friendlyNames.Add(strArray[0].Trim());
                this.paramNames.Add(strArray[1].Trim());
              }
            }
          }
        }
        catch
        {
          this.Close();
          new TaskDialog("Ticket Manager Settings File Error")
          {
            MainInstruction = "Unable to read in Ticket Manager Settings File.",
            MainContent = $"Unable to read in the Ticket Manager Settings File available at {this.pathName}. Please ensure the file is available to be read and in the valid format and try again."
          }.Show();
          this.closed = true;
          return;
        }
        this.projParams = new List<string>();
        List<string> stringList = new List<string>()
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
        DefinitionBindingMapIterator bindingMapIterator1 = this.app.ActiveUIDocument.Document.ParameterBindings.ForwardIterator();
        while (bindingMapIterator1.MoveNext())
        {
          InstanceBinding current1 = bindingMapIterator1.Current as InstanceBinding;
          TypeBinding current2 = bindingMapIterator1.Current as TypeBinding;
          if (bindingMapIterator1.Current != null)
          {
            CategorySet categories;
            if (bindingMapIterator1.Current is InstanceBinding)
              categories = current1.Categories;
            else if (bindingMapIterator1.Current is TypeBinding)
              categories = current2.Categories;
            else
              continue;
            foreach (Category category in categories)
            {
              if ((category.Name.Equals("Assemblies") || category.Name.Equals("Structural Framing")) && !this.projParams.Contains(bindingMapIterator1.Key.Name) && !stringList.Contains(bindingMapIterator1.Key.Name.ToUpper()) && (bindingMapIterator1.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator1.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
                this.projParams.Add(bindingMapIterator1.Key.Name);
            }
          }
        }
        this.DataGrid.Columns[11].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[12].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[13].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[14].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[15].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[17].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[19].Visibility = System.Windows.Visibility.Collapsed;
        this.DataGrid.Columns[21].Visibility = System.Windows.Visibility.Collapsed;
        bool flag1 = false;
        bool flag2 = false;
        DefinitionBindingMapIterator bindingMapIterator2 = this.app.ActiveUIDocument.Document.ParameterBindings.ForwardIterator();
        while (bindingMapIterator2.MoveNext())
        {
          if (bindingMapIterator2.Key.Name.Equals("IDENTITY_COMMENT"))
            flag1 = true;
          if (bindingMapIterator2.Key.Name.Equals("ON_HOLD"))
            flag2 = true;
        }
        this.readOnlyColumnNames.AddRange((IEnumerable<string>) new List<string>()
        {
          "Mark #",
          "Assembled",
          "Quantity",
          "Reinforced Date",
          "Created Date",
          "Detailed Date",
          "Date Released"
        });
        if (flag2)
        {
          this.DataGrid.Columns[11].Visibility = System.Windows.Visibility.Visible;
          this.DataGrid.Columns[11].IsReadOnly = false;
          this.OnHoldToggle.Visibility = System.Windows.Visibility.Visible;
        }
        if (flag1)
        {
          this.DataGrid.Columns[13].Visibility = System.Windows.Visibility.Visible;
          this.DataGrid.Columns[13].IsReadOnly = false;
          this.IdentCommentEdit.Visibility = System.Windows.Visibility.Visible;
        }
        if (this.friendlyNames.Count > 0 && this.projParams.Contains(this.paramNames[0]))
        {
          this.DataGrid.Columns[15].Header = (object) this.friendlyNames[0].Substring(0, Math.Min(this.friendlyNames[0].Length, 25));
          this.DataGrid.Columns[15].Visibility = System.Windows.Visibility.Visible;
          this.DataGrid.Columns[16 /*0x10*/].Header = (object) this.friendlyNames[0].Substring(0, Math.Min(this.friendlyNames[0].Length, 25));
        }
        if (this.friendlyNames.Count > 1 && this.projParams.Contains(this.paramNames[1]))
        {
          this.DataGrid.Columns[17].Header = (object) this.friendlyNames[1].Substring(0, Math.Min(this.friendlyNames[1].Length, 25));
          this.DataGrid.Columns[17].Visibility = System.Windows.Visibility.Visible;
          this.DataGrid.Columns[18].Header = (object) this.friendlyNames[1].Substring(0, Math.Min(this.friendlyNames[1].Length, 25));
        }
        if (this.friendlyNames.Count > 2 && this.projParams.Contains(this.paramNames[2]))
        {
          this.DataGrid.Columns[19].Header = (object) this.friendlyNames[2].Substring(0, Math.Min(this.friendlyNames[2].Length, 25));
          this.DataGrid.Columns[19].Visibility = System.Windows.Visibility.Visible;
          this.DataGrid.Columns[20].Header = (object) this.friendlyNames[2].Substring(0, Math.Min(this.friendlyNames[2].Length, 25));
        }
        if (this.friendlyNames.Count > 3 && this.projParams.Contains(this.paramNames[3]))
        {
          this.DataGrid.Columns[21].Header = (object) this.friendlyNames[3].Substring(0, Math.Min(this.friendlyNames[3].Length, 25));
          this.DataGrid.Columns[21].Visibility = System.Windows.Visibility.Visible;
          this.DataGrid.Columns[22].Header = (object) this.friendlyNames[3].Substring(0, Math.Min(this.friendlyNames[3].Length, 25));
        }
        foreach (DataGridColumn column in (Collection<DataGridColumn>) this.DataGrid.Columns)
          column.Width = new DataGridLength(1.0, DataGridLengthUnitType.SizeToCells);
        ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel = this.app.ActiveUIDocument.Document.PathName;
        ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle = ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel;
        this.ShowHideCustomParamsOption();
        App.TicketManagerWindow.Topmost = false;
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.ToString(), "Error");
      }
    }
  }

  private void ShowHideCustomParamsOption()
  {
    if (this.paramNames.Count > 0)
      this.EditButton.Visibility = System.Windows.Visibility.Visible;
    else
      this.EditButton.Visibility = System.Windows.Visibility.Collapsed;
  }

  private void SheetsMenuItem_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      IEnumerable<AssemblyViewModel> list = (IEnumerable<AssemblyViewModel>) App.TicketManagerWindow.DataGrid.SelectedItems.Cast<AssemblyViewModel>().ToList<AssemblyViewModel>();
      if (list.Count<AssemblyViewModel>() <= 0)
        return;
      AssemblyViewModel assemblyViewModel = list.First<AssemblyViewModel>();
      if (list.Count<AssemblyViewModel>() > 1)
      {
        if (assemblyViewModel.Assemblied == "No")
        {
          int num1 = (int) MessageBox.Show("The first Mark Number in the selection is not an assembly, and no ticket view sheet has been created for this ticket yet. Please select one assembly. ", "Error");
        }
        else if (assemblyViewModel.AssociatedViewSheets.Count == 0)
        {
          int num2 = (int) MessageBox.Show("A ticket view sheet has not been created yet for the first Mark Number in the selection. Please select one assembly.", "Error");
        }
        else
        {
          int num3 = (int) MessageBox.Show("Please select one assembly. Ticket View Sheets will be provided for the first assembly you selected.", "Error");
        }
      }
      else if (assemblyViewModel.Assemblied == "No")
      {
        int num4 = (int) MessageBox.Show("This is not an assembly and no view sheet has been created for this ticket yet.", "Error");
      }
      else
      {
        if (assemblyViewModel.AssociatedViewSheets.Count != 0)
          return;
        int num5 = (int) MessageBox.Show("There is no view sheet created for this ticket yet.", "Error");
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString(), "Error");
    }
  }

  private void ToggleOnHold_Click(object sender, RoutedEventArgs e)
  {
    if (ActiveModel.UIDoc != null)
      this.myDocky = ActiveModel.UIDoc;
    else if (ActiveModel.UIDoc == null)
    {
      ActiveModel.GetInformation(this.app.ActiveUIDocument);
      this.myDocky = ActiveModel.UIDoc;
    }
    if (this.DataGrid.SelectedItems.Count > 1)
    {
      int num1 = (int) MessageBox.Show("On Hold can only be toggled for one control mark at a time. Please select one control mark grouping and try again.", "Error");
    }
    MainViewModel dataContext = ((FrameworkElement) sender).DataContext as MainViewModel;
    if (dataContext.SelectedItem == null)
      this.DataGrid.IsEnabled = true;
    else if (dataContext.SelectedItem == null)
    {
      this.DataGrid.IsEnabled = true;
    }
    else
    {
      AssemblyViewModel selectedItem = dataContext.SelectedItem;
      List<string> stringList1 = new List<string>();
      List<string> stringList2 = new List<string>();
      int result = 0;
      int.TryParse(selectedItem.ElementId, out result);
      Element element = this.myDocky.Document.GetElement(new ElementId(result));
      string str1;
      string str2;
      if (selectedItem.OnHold.ToUpper().Equals("YES"))
      {
        str1 = "0";
        str2 = "No";
      }
      else if (selectedItem.OnHold.ToUpper().Equals("NO"))
      {
        str1 = "1";
        str2 = "Yes";
      }
      else
      {
        str1 = "1";
        str2 = "Yes";
      }
      try
      {
        selectedItem.OnHold = str1;
        Utils.AssemblyUtils.Parameters.AddEditComment(element, "UPDATED", "ON_HOLD has been updated to " + str2);
        ((MainViewModel) App.TicketManagerWindow.DataContext).EditingItem = selectedItem;
        ((MainViewModel) App.TicketManagerWindow.DataContext).UpdateParametersCommand.Execute((object) null);
      }
      catch (Exception ex)
      {
        int num2 = (int) MessageBox.Show(ex.ToString());
      }
      this.Title = ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle;
      ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel = this.app.ActiveUIDocument.Document.PathName;
      ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle = ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel;
    }
  }

  private void EditIdentityComment_Click(object sender, RoutedEventArgs e)
  {
    this.DataGrid.IsEnabled = false;
    if (this.DataGrid.SelectedItems.Count > 1)
    {
      int num1 = (int) MessageBox.Show("Identity Comment can only be edited for one control mark at a time. Please select one control mark grouping and try again.", "Error");
    }
    MainViewModel dataContext = ((FrameworkElement) sender).DataContext as MainViewModel;
    if (dataContext.SelectedItem == null)
      this.DataGrid.IsEnabled = true;
    else if (dataContext.SelectedItem == null)
    {
      this.DataGrid.IsEnabled = true;
    }
    else
    {
      AssemblyViewModel selectedItem = dataContext.SelectedItem;
      List<string> rowContents = new List<string>();
      rowContents.Add(selectedItem.IdentityComment);
      try
      {
        new EditIdentCommentWindow(this.app, rowContents, selectedItem, this.friendlyNames, this.paramNames).ShowDialog();
      }
      catch (Exception ex)
      {
        int num2 = (int) MessageBox.Show(ex.ToString());
      }
      this.Title = ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle;
      ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel = this.app.ActiveUIDocument.Document.PathName;
      ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle = ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel;
      this.DataGrid.IsEnabled = true;
    }
  }

  private void EditButton_Click(object sender, RoutedEventArgs e)
  {
    this.DataGrid.IsEnabled = false;
    if (this.DataGrid.SelectedItems.Count > 1)
    {
      int num1 = (int) MessageBox.Show("Custom parameters can only be edited for one control mark at a time. Please select one control mark grouping and try again.", "Error");
    }
    MainViewModel dataContext = ((FrameworkElement) sender).DataContext as MainViewModel;
    if (dataContext.SelectedItem == null)
      this.DataGrid.IsEnabled = true;
    else if (dataContext.SelectedItem == null)
    {
      this.DataGrid.IsEnabled = true;
    }
    else
    {
      AssemblyViewModel selectedItem = dataContext.SelectedItem;
      if (!ModelLockingUtils.ShowPermissionsDialog(selectedItem.revitDoc, ModelLockingToolPermissions.TicketManagerParams))
      {
        this.DataGrid.IsEnabled = true;
      }
      else
      {
        List<string> rowContents = new List<string>();
        rowContents.Add(selectedItem.OnHold);
        rowContents.Add(selectedItem.IdentityComment);
        rowContents.Add(selectedItem.Custom1);
        rowContents.Add(selectedItem.Custom2);
        rowContents.Add(selectedItem.Custom3);
        rowContents.Add(selectedItem.Custom4);
        try
        {
          new EditWindow(this.app, rowContents, selectedItem, this.friendlyNames, this.paramNames).ShowDialog();
        }
        catch (Exception ex)
        {
          int num2 = (int) MessageBox.Show(ex.ToString());
        }
        this.Title = ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle;
        ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel = this.app.ActiveUIDocument.Document.PathName;
        ((MainViewModel) App.TicketManagerWindow.DataContext).WindowTitle = ((MainViewModel) App.TicketManagerWindow.DataContext).CurrentModel;
        this.DataGrid.IsEnabled = true;
      }
    }
  }

  private void TicketManager_Closing(object sender, CancelEventArgs e)
  {
    App.TicketManagerWindow = (MainWindow) null;
  }

  public void checkOnHoldEditComment(Document UiDoc)
  {
    bool flag1 = false;
    bool flag2 = false;
    DefinitionBindingMapIterator bindingMapIterator = UiDoc.ParameterBindings.ForwardIterator();
    while (bindingMapIterator.MoveNext())
    {
      if (bindingMapIterator.Key.Name.Equals("IDENTITY_COMMENT"))
        flag1 = true;
      if (bindingMapIterator.Key.Name.Equals("ON_HOLD"))
        flag2 = true;
    }
    if (flag2)
      this.OnHoldToggle.Visibility = System.Windows.Visibility.Visible;
    else
      this.OnHoldToggle.Visibility = System.Windows.Visibility.Collapsed;
    if (flag1)
      this.IdentCommentEdit.Visibility = System.Windows.Visibility.Visible;
    else
      this.IdentCommentEdit.Visibility = System.Windows.Visibility.Collapsed;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/ticketmanager/views/mainwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Activated += new EventHandler(this.App_Activated);
        ((Window) target).Closing += new CancelEventHandler(this.TicketManager_Closing);
        break;
      case 2:
        this.GridForm = (System.Windows.Controls.Grid) target;
        break;
      case 3:
        this.SearchBox = (TextBox) target;
        break;
      case 4:
        this.DataGrid = (DataGrid) target;
        this.DataGrid.SelectionChanged += new SelectionChangedEventHandler(this.DataGrid_SelectionChanged);
        break;
      case 5:
        this.ContextMenu = (ContextMenu) target;
        break;
      case 6:
        this.OnHoldToggle = (MenuItem) target;
        this.OnHoldToggle.Click += new RoutedEventHandler(this.ToggleOnHold_Click);
        break;
      case 7:
        this.IdentCommentEdit = (MenuItem) target;
        this.IdentCommentEdit.Click += new RoutedEventHandler(this.EditIdentityComment_Click);
        break;
      case 8:
        this.EditButton = (MenuItem) target;
        this.EditButton.Click += new RoutedEventHandler(this.EditButton_Click);
        break;
      case 9:
        this.SheetsMenuItem = (MenuItem) target;
        this.SheetsMenuItem.Click += new RoutedEventHandler(this.SheetsMenuItem_Click);
        break;
      case 10:
        this.RefreshButton = (MenuItem) target;
        break;
      case 11:
        this.ReinforcedByColumn = (DataGridTextColumn) target;
        break;
      case 12:
        this.CreatedByColumn = (DataGridTextColumn) target;
        break;
      case 13:
        this.DetailedByColumn = (DataGridTextColumn) target;
        break;
      case 14:
        this.ReleasedByColumn = (DataGridTextColumn) target;
        break;
      case 15:
        this.OnHoldColumn = (DataGridTextColumn) target;
        break;
      case 16 /*0x10*/:
        this.IdentityCommentColumn = (DataGridTextColumn) target;
        break;
      case 17:
        this.Custom1Column = (DataGridTextColumn) target;
        break;
      case 18:
        this.Custom2Column = (DataGridTextColumn) target;
        break;
      case 19:
        this.Custom3Column = (DataGridTextColumn) target;
        break;
      case 20:
        this.Custom4Column = (DataGridTextColumn) target;
        break;
      case 21:
        this.ElementIdColumn = (DataGridTextColumn) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
