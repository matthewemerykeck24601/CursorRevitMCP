// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.HWDetailUserSettingWindow
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class HWDetailUserSettingWindow : Window, IComponentConnector
{
  private HWDBOMSettings bomSettings = new HWDBOMSettings();
  private string scheduleName = string.Empty;
  private List<string> illegalChar;
  private string manufacturerName = "";
  private string path = "";
  public bool isSaved;
  internal System.Windows.Controls.Button AddRow;
  internal System.Windows.Controls.Button Delete;
  internal System.Windows.Controls.DataGrid scheduleList;
  private bool _contentLoaded;

  public HWDetailUserSettingWindow(
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
    this.illegalChar = new List<string>()
    {
      "<",
      ">",
      ";",
      "[",
      "]",
      "{",
      "}",
      "\\",
      "|",
      "`",
      "~",
      "?",
      ":"
    };
    ifContinue = true;
    if (revitDoc.ProjectInformation == null)
    {
      ifContinue = false;
      int num = (int) System.Windows.MessageBox.Show("Hardware Detail BOM Setting Dialog cannot be used in the family view, transaction will be cancelled.");
    }
    else
    {
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        this.manufacturerName = parameter.AsString();
      this.path = string.IsNullOrEmpty(App.HWBOMFolderPath) ? $"C:/EDGEforREVIT/{this.manufacturerName}_HWDetail_BOM.xml" : $"{App.HWBOMFolderPath}\\{this.manufacturerName}_HWDetail_BOM.xml";
      if (File.Exists(this.path))
      {
        if (this.bomSettings.LoadTicketTemplateSettings(this.path))
          this.scheduleList.ItemsSource = (IEnumerable) this.bomSettings.schedule_List;
        else
          ifContinue = false;
      }
      else
        this.scheduleList.ItemsSource = (IEnumerable) new List<ItemEntry>();
      if (this.bomSettings.schedule_List.Count != 0)
        return;
      this.bomSettings.schedule_List.Add(new DGEntry("<Please click 'Add' button>"));
      this.scheduleList.ItemsSource = (IEnumerable) this.bomSettings.schedule_List;
    }
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    HashSet<string> stringSet = new HashSet<string>();
    if (!this.RemoveBlankValues())
      return;
    foreach (DGEntry schedule in this.bomSettings.schedule_List)
    {
      if (stringSet.Contains(schedule.scheduleName))
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("The list contains duplicate Template Schedule names, please review and modify!", "Warning");
        return;
      }
      stringSet.Add(schedule.scheduleName);
      foreach (string str in this.illegalChar)
      {
        if (schedule.scheduleName.Contains(str))
        {
          int num = (int) System.Windows.Forms.MessageBox.Show("Please check your input. Your ticket BOM list contains one or more following illegal characters <>;:[]{}\\|`~? ", "Warning");
          return;
        }
      }
    }
    try
    {
      this.bomSettings.SaveTemplateSettings(this.path);
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.Forms.MessageBox.Show(ex.ToString(), "Warning");
    }
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void DeleteButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (DGEntry selectedItem in (IEnumerable) this.scheduleList.SelectedItems)
    {
      if (this.bomSettings.schedule_List[0].scheduleName.Contains("<Please click 'Add' button>"))
        this.bomSettings.schedule_List.Clear();
      else
        this.bomSettings.schedule_List.Remove(selectedItem);
    }
    this.scheduleList.ItemsSource = (IEnumerable) null;
    this.scheduleList.ItemsSource = (IEnumerable) this.bomSettings.schedule_List;
    this.Delete.IsEnabled = false;
  }

  private void AddRow_Click(object sender, RoutedEventArgs e)
  {
    if (this.bomSettings.schedule_List.Count > 0 && this.bomSettings.schedule_List[0].scheduleName.Contains("<Please click 'Add' button>"))
      this.bomSettings.schedule_List.Clear();
    this.bomSettings.schedule_List.Add(new DGEntry());
    this.scheduleList.ItemsSource = (IEnumerable) null;
    this.scheduleList.ItemsSource = (IEnumerable) this.bomSettings.schedule_List;
  }

  private bool RemoveBlankValues()
  {
    for (int index = 0; index < this.bomSettings.schedule_List.Count; ++index)
    {
      if (this.bomSettings.schedule_List[index].scheduleName.Trim() == string.Empty)
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("Empty schedule names are invalid.  Please remove any blank entries.", "Warning");
        return false;
      }
    }
    return true;
  }

  private void HWD_BOM_List_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    this.Delete.IsEnabled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/hwdetailusersettingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.AddRow = (System.Windows.Controls.Button) target;
        this.AddRow.Click += new RoutedEventHandler(this.AddRow_Click);
        break;
      case 2:
        this.Delete = (System.Windows.Controls.Button) target;
        this.Delete.Click += new RoutedEventHandler(this.DeleteButton_Click);
        break;
      case 3:
        this.scheduleList = (System.Windows.Controls.DataGrid) target;
        this.scheduleList.SelectionChanged += new SelectionChangedEventHandler(this.HWD_BOM_List_SelectionChanged);
        break;
      case 4:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.SaveButton_Click);
        break;
      case 5:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
