// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.BOMUserSettingWindow
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

public class BOMUserSettingWindow : Window, IComponentConnector
{
  private TKTBOMSettings ListItems = new TKTBOMSettings();
  private List<string> illegalChar;
  private int emptyTemplateAttempt;
  private string manufacturerName = "";
  private string path = "";
  public bool isSaved;
  internal System.Windows.Controls.Button AddRow;
  internal System.Windows.Controls.Button Delete;
  internal System.Windows.Controls.DataGrid Ticket_BOM_List;
  private bool _contentLoaded;

  public BOMUserSettingWindow(Document revitDoc, IntPtr parentWindowHandler, out bool ifContinue)
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
      int num = (int) System.Windows.MessageBox.Show("Ticket BOM Setting Dialog cannot be used in the family view, transaction will be cancelled.");
    }
    else
    {
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        this.manufacturerName = parameter.AsString();
      this.path = string.IsNullOrEmpty(App.TKTBOMFolderPath) ? $"C:/EDGEforREVIT/{this.manufacturerName}_TICKET_BOM.xml" : $"{App.TKTBOMFolderPath}\\{this.manufacturerName}_Ticket_BOM.xml";
      if (File.Exists(this.path))
      {
        if (this.ListItems.LoadTicketTemplateSettings(this.path))
          this.Ticket_BOM_List.ItemsSource = (IEnumerable) this.ListItems.TicketBOMList;
        else
          ifContinue = false;
      }
      else
        this.Ticket_BOM_List.ItemsSource = (IEnumerable) new List<ItemEntry>();
    }
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    if (!this.CheckforBlankTemplate())
      return;
    HashSet<string> stringSet1 = new HashSet<string>();
    HashSet<string> stringSet2 = new HashSet<string>();
    foreach (ItemEntry ticketBom in this.ListItems.TicketBOMList)
    {
      if (stringSet1.Contains(ticketBom.templateSchedule))
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("The list contains duplicate Template Schedule names, please review and modify!", "Warning");
        return;
      }
      stringSet1.Add(ticketBom.templateSchedule);
      if (stringSet2.Contains(ticketBom.scheduleSuffix) && (!string.IsNullOrEmpty(ticketBom.scheduleSuffix) || !string.IsNullOrWhiteSpace(ticketBom.scheduleSuffix)))
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("The list contains duplicate Assembly Schedule Suffixes, please review and modify!", "Warning");
        return;
      }
      stringSet2.Add(ticketBom.scheduleSuffix);
      foreach (string str in this.illegalChar)
      {
        if (ticketBom.templateSchedule.Contains(str) || ticketBom.scheduleSuffix.Contains(str))
        {
          int num = (int) System.Windows.Forms.MessageBox.Show("Please check your input. Your ticket BOM list contains one or more following illegal characters <>;:[]{}\\|`~? ", "Warning");
          return;
        }
      }
    }
    try
    {
      this.ListItems.SaveTempalteSettings(this.path);
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
    foreach (ItemEntry selectedItem in (IEnumerable) this.Ticket_BOM_List.SelectedItems)
      this.ListItems.TicketBOMList.Remove(selectedItem);
    this.Ticket_BOM_List.ItemsSource = (IEnumerable) null;
    this.Ticket_BOM_List.ItemsSource = (IEnumerable) this.ListItems.TicketBOMList;
    this.Delete.IsEnabled = false;
  }

  private void Ticket_BOM_List_SelectionChanged(object sender, RoutedEventArgs e)
  {
    this.Delete.IsEnabled = true;
  }

  private void AddRow_Click(object sender, RoutedEventArgs e)
  {
    this.ListItems.TicketBOMList.Add(new ItemEntry());
    this.Ticket_BOM_List.ItemsSource = (IEnumerable) null;
    this.Ticket_BOM_List.ItemsSource = (IEnumerable) this.ListItems.TicketBOMList;
  }

  private bool CheckforBlankTemplate()
  {
    for (int index = 0; index < this.ListItems.TicketBOMList.Count; ++index)
    {
      if (this.ListItems.TicketBOMList[index].templateSchedule.Trim() == string.Empty)
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("Template schedule is a required field and should not be left blank.", "Warning");
        return false;
      }
    }
    return true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/bomusersettingwindow.xaml", UriKind.Relative));
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
        this.Ticket_BOM_List = (System.Windows.Controls.DataGrid) target;
        this.Ticket_BOM_List.SelectionChanged += new SelectionChangedEventHandler(this.Ticket_BOM_List_SelectionChanged);
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
