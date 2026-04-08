// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.HWTitleBlockPopulatorSettingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class HWTitleBlockPopulatorSettingWindow : Window, IComponentConnector
{
  private TBParameterSettings ListItems = new TBParameterSettings();
  private List<DatagridItemEntry> listToShow = new List<DatagridItemEntry>();
  private string manufacturerName = "";
  private string path = "";
  public bool isSaved;
  private object selectedItem;
  private object selectedItemTo;
  internal System.Windows.Controls.ComboBox MappingFromList;
  internal System.Windows.Controls.ComboBox MappingToList;
  internal System.Windows.Controls.Button AddRow;
  internal System.Windows.Controls.Button Delete;
  internal System.Windows.Controls.DataGrid TBMapping_List;
  private bool _contentLoaded;

  public HWTitleBlockPopulatorSettingWindow(
    Document revitDoc,
    IntPtr parentWindowHandler,
    out bool ifContinue)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.DataContext = (object) this;
    ifContinue = true;
    if (revitDoc.ProjectInformation == null)
    {
      ifContinue = false;
      int num = (int) System.Windows.MessageBox.Show("Hardware Detail Title Block Poplulator Setting Dialog cannot be used in the family view, transaction will be cancelled.");
    }
    else
    {
      List<string> source = new List<string>();
      DefinitionBindingMapIterator bindingMapIterator1 = revitDoc.ParameterBindings.ForwardIterator();
      while (bindingMapIterator1.MoveNext())
      {
        InstanceBinding current1 = bindingMapIterator1.Current as InstanceBinding;
        TypeBinding current2 = bindingMapIterator1.Current as TypeBinding;
        if (current1 != null)
        {
          foreach (Category category in current1.Categories)
          {
            if (category.Name.Equals("Specialty Equipment") || category.Name.Equals("Generic Models") || category.Name.Equals("Assemblies"))
              source.Add(bindingMapIterator1.Key.Name);
          }
        }
        else if (current2 != null)
        {
          foreach (Category category in current2.Categories)
          {
            if (category.Name.Equals("Specialty Equipment") || category.Name.Equals("Generic Models") || category.Name.Equals("Assemblies"))
              source.Add(bindingMapIterator1.Key.Name);
          }
        }
      }
      List<string> list = source.Distinct<string>().ToList<string>();
      list.Sort();
      this.MappingFromList.ItemsSource = (IEnumerable) list;
      List<string> stringList1 = new List<string>()
      {
        "TKT_HW_WEIGHT",
        "Sheet Name"
      };
      List<string> stringList2 = new List<string>();
      DefinitionBindingMapIterator bindingMapIterator2 = revitDoc.ParameterBindings.ForwardIterator();
      while (bindingMapIterator2.MoveNext())
      {
        if (bindingMapIterator2.Current is InstanceBinding current)
        {
          foreach (Category category in current.Categories)
          {
            if (category.Name.Equals("Sheets") && !stringList1.Contains(bindingMapIterator2.Key.Name))
              stringList2.Add(bindingMapIterator2.Key.Name);
          }
        }
      }
      stringList2.Sort();
      this.MappingToList.ItemsSource = (IEnumerable) stringList2;
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        this.manufacturerName = parameter.AsString();
      this.path = string.IsNullOrEmpty(App.HWTBPopFolderPath) ? $"C:/EDGEforREVIT/{this.manufacturerName}_HWTitleBlock_Mapping.xml" : $"{App.HWTBPopFolderPath}\\{this.manufacturerName}_HWTitleBlock_Mapping.xml";
      if (File.Exists(this.path))
      {
        if (this.ListItems.LoadTicketTemplateSettings(this.path))
          this.TBMapping_List.ItemsSource = (IEnumerable) this.ListItems.TBParameterList;
        else
          ifContinue = false;
      }
      else
      {
        this.listToShow.Add(new DatagridItemEntry("Combined Hardware Weight", "TKT_HW_WEIGHT", false));
        this.listToShow.Add(new DatagridItemEntry("CONTROL_MARK", "Sheet Name", false));
        this.ListItems.TBParameterList.AddRange((IEnumerable<DatagridItemEntry>) this.listToShow);
        this.TBMapping_List.ItemsSource = (IEnumerable) this.ListItems.TBParameterList;
      }
    }
  }

  private void AddRow_Click(object sender, RoutedEventArgs e)
  {
    if (this.selectedItem == null || this.selectedItemTo == null)
    {
      int num1 = (int) System.Windows.MessageBox.Show("Invalid entry. Please make a selection for Parameters Mapping From and Parameters Mapping To before adding the mapping.", "Warning");
    }
    else
    {
      foreach (DatagridItemEntry tbParameter in this.ListItems.TBParameterList)
      {
        if (tbParameter.mappingToParam.Equals(this.selectedItemTo.ToString()))
        {
          int num2 = (int) System.Windows.Forms.MessageBox.Show("The list contains duplicate parameters that attemps to mapping to, please review and modify!", "Warning");
          return;
        }
      }
      this.ListItems.TBParameterList.Add(new DatagridItemEntry(this.selectedItem.ToString(), this.selectedItemTo.ToString(), true));
      this.TBMapping_List.ItemsSource = (IEnumerable) null;
      this.TBMapping_List.ItemsSource = (IEnumerable) this.ListItems.TBParameterList;
    }
  }

  private void DeleteButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (DatagridItemEntry selectedItem in (IEnumerable) this.TBMapping_List.SelectedItems)
    {
      if (selectedItem.canDeleteParam)
        this.ListItems.TBParameterList.Remove(selectedItem);
    }
    this.TBMapping_List.ItemsSource = (IEnumerable) null;
    this.TBMapping_List.ItemsSource = (IEnumerable) this.ListItems.TBParameterList;
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    FileInfo fileInfo = new FileInfo(this.path);
    try
    {
      this.ListItems.SaveTempalteSettings(this.path);
      this.isSaved = true;
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.Forms.MessageBox.Show(ex.ToString(), "Warning");
    }
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void MappingFromList_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    this.selectedItem = (sender as System.Windows.Controls.ComboBox).SelectedItem;
  }

  private void MappingToList_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    this.selectedItemTo = (sender as System.Windows.Controls.ComboBox).SelectedItem;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/hwtitleblockpopulatorsettingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.MappingFromList = (System.Windows.Controls.ComboBox) target;
        this.MappingFromList.SelectionChanged += new SelectionChangedEventHandler(this.MappingFromList_SelectionChanged);
        break;
      case 2:
        this.MappingToList = (System.Windows.Controls.ComboBox) target;
        this.MappingToList.SelectionChanged += new SelectionChangedEventHandler(this.MappingToList_SelectionChanged);
        break;
      case 3:
        this.AddRow = (System.Windows.Controls.Button) target;
        this.AddRow.Click += new RoutedEventHandler(this.AddRow_Click);
        break;
      case 4:
        this.Delete = (System.Windows.Controls.Button) target;
        this.Delete.Click += new RoutedEventHandler(this.DeleteButton_Click);
        break;
      case 5:
        this.TBMapping_List = (System.Windows.Controls.DataGrid) target;
        break;
      case 6:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.SaveButton_Click);
        break;
      case 7:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
