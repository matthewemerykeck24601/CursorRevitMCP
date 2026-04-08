// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.UserSettingWindow
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
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

public class UserSettingWindow : Window, IComponentConnector
{
  private MarkVerificationSettings ListItems = new MarkVerificationSettings();
  private List<string> illegalChar;
  private string manufacturerName = "";
  private string path = "";
  public bool isSaved;
  internal System.Windows.Controls.TextBox NewProductName;
  internal System.Windows.Controls.Button AddNewAdmin;
  internal System.Windows.Controls.Button DeleteAdmin;
  internal System.Windows.Controls.DataGrid Mark_Prefix_List;
  private bool _contentLoaded;

  public UserSettingWindow(Document revitDoc, IntPtr parentWindowHandler, out bool ifContinue)
  {
    this.InitializeComponent();
    ifContinue = true;
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
    if (revitDoc.ProjectInformation == null)
    {
      ifContinue = false;
      int num = (int) System.Windows.MessageBox.Show("Mark Prefix User Setting Dialog cannot be used in the family view, transaction will be cancelled.");
    }
    else
    {
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        this.manufacturerName = parameter.AsString();
      this.path = string.IsNullOrEmpty(App.MarkPrefixFolderPath) ? $"C:/EDGEforREVIT/{this.manufacturerName}_MARK_PREFIX.xml" : $"{App.MarkPrefixFolderPath}\\{this.manufacturerName}_MARK_PREFIX.xml";
      if (File.Exists(this.path))
      {
        if (!this.ListItems.LoadMarkPrefixSettings(this.path))
          return;
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) this.ListItems.PrefixList;
      }
      else
      {
        List<ItemEntry> itemEntryList = new List<ItemEntry>();
        itemEntryList.Add(new ItemEntry("COLUMN", "", ""));
        itemEntryList.Add(new ItemEntry("DOUBLE TEE", "", ""));
        itemEntryList.Add(new ItemEntry("FLAT SLAB", "", ""));
        itemEntryList.Add(new ItemEntry("HOLLOW CORE SLAB", "", ""));
        itemEntryList.Add(new ItemEntry("LGIRDER", "", ""));
        itemEntryList.Add(new ItemEntry("LITEWALL HORIZONTAL", "", ""));
        itemEntryList.Add(new ItemEntry("LITEWALL VERTICAL", "", ""));
        itemEntryList.Add(new ItemEntry("RBEAM", "", ""));
        itemEntryList.Add(new ItemEntry("SHEARWALL HORIZONTAL", "", ""));
        itemEntryList.Add(new ItemEntry("SHEARWALL VERTICAL", "", ""));
        itemEntryList.Add(new ItemEntry("SPANDREL BEARING", "", ""));
        itemEntryList.Add(new ItemEntry("SPANDREL NONBEARING", "", ""));
        itemEntryList.Add(new ItemEntry("STAIR", "", ""));
        itemEntryList.Add(new ItemEntry("TGIRDER", "", ""));
        itemEntryList.Add(new ItemEntry("WALL COLUMN", "", ""));
        itemEntryList.Add(new ItemEntry("WALL PANEL VERTICAL", "", ""));
        itemEntryList.Add(new ItemEntry("WALL PANEL HORIZONTAL", "", ""));
        itemEntryList.Add(new ItemEntry("WALL PANEL INSULATED NON-THERMAL", "", ""));
        itemEntryList.Add(new ItemEntry("WALL PANEL INSULATED TROUGH", "", ""));
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) itemEntryList;
        this.ListItems.PrefixList = itemEntryList;
        this.ListItems.AlphabeticNumbering = false;
      }
    }
  }

  private void Mark_Prefix_List_SelectionChanged(object sender, RoutedEventArgs e)
  {
    this.DeleteAdmin.Visibility = System.Windows.Visibility.Visible;
  }

  private void AddNewButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (string str in this.illegalChar)
    {
      if (this.NewProductName.Text.Contains(str))
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("Please check your input. The new product name contains one or more following illegal characters <>;:[]{}\\|'~? ", "Warning");
        return;
      }
    }
    if (!this.NewProductName.Text.Equals(""))
    {
      bool flag = false;
      if (this.Mark_Prefix_List.ItemsSource != null)
      {
        foreach (ItemEntry itemEntry in this.Mark_Prefix_List.ItemsSource)
        {
          if (itemEntry.productName.Equals(this.NewProductName.Text))
            flag = true;
        }
      }
      if (flag)
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("The product name you entered exists in the current list!", "Warning");
      }
      else
      {
        this.ListItems.PrefixList.Add(new ItemEntry(this.NewProductName.Text, "", ""));
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) null;
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) this.ListItems.PrefixList;
      }
      this.NewProductName.Text = "";
    }
    this.DeleteAdmin.Visibility = System.Windows.Visibility.Hidden;
  }

  private void DeleteButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (ItemEntry selectedItem in (IEnumerable) this.Mark_Prefix_List.SelectedItems)
      this.ListItems.PrefixList.Remove(selectedItem);
    this.Mark_Prefix_List.ItemsSource = (IEnumerable) null;
    this.Mark_Prefix_List.ItemsSource = (IEnumerable) this.ListItems.PrefixList;
  }

  private void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (ItemEntry prefix in this.ListItems.PrefixList)
    {
      foreach (string str in this.illegalChar)
      {
        if (prefix.markPrefix.Contains(str))
        {
          int num = (int) System.Windows.Forms.MessageBox.Show("Please check your input. Your mark prefix list contains one or more following illegal characters <>;:[]{}\\|`~? ", "Warning");
          return;
        }
      }
    }
    try
    {
      this.ListItems.SaveTemplateSettings(this.path);
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.Forms.MessageBox.Show(ex.ToString(), "warning");
    }
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/usersettingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.NewProductName = (System.Windows.Controls.TextBox) target;
        break;
      case 2:
        this.AddNewAdmin = (System.Windows.Controls.Button) target;
        this.AddNewAdmin.Click += new RoutedEventHandler(this.AddNewButton_Click);
        break;
      case 3:
        this.DeleteAdmin = (System.Windows.Controls.Button) target;
        this.DeleteAdmin.Click += new RoutedEventHandler(this.DeleteButton_Click);
        break;
      case 4:
        this.Mark_Prefix_List = (System.Windows.Controls.DataGrid) target;
        this.Mark_Prefix_List.SelectionChanged += new SelectionChangedEventHandler(this.Mark_Prefix_List_SelectionChanged);
        break;
      case 5:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      case 6:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.SaveButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
