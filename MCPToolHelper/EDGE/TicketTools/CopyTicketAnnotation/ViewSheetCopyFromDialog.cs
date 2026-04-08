// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.ViewSheetCopyFromDialog
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

public class ViewSheetCopyFromDialog : Window, IComponentConnector, IStyleConnector
{
  private List<ViewSheetItemEntry> Items = new List<ViewSheetItemEntry>();
  private bool byAssembly = true;
  private string toolname = "";
  public bool isContinue;
  public ViewSheetItemEntry SelectedItem;
  internal TextBlock selectionFromInstruction;
  internal TextBlock selectionFromInstruction2;
  internal ListBox lbTodoList;
  private bool _contentLoaded;

  public ViewSheetCopyFromDialog(
    IEnumerable<ViewSheetItemEntry> viewsheets,
    UIDocument uidoc,
    string type,
    string tool)
  {
    this.InitializeComponent();
    this.Items.AddRange(viewsheets);
    this.lbTodoList.ItemsSource = (IEnumerable) this.Items;
    this.toolname = !(tool == "annotations") ? "dimensions" : "annotations";
    if (type != nameof (byAssembly))
    {
      this.selectionFromInstruction.Text = "Please select one view sheet that you want to copy";
      this.byAssembly = false;
    }
    else
    {
      this.selectionFromInstruction.Text = "Please select one assembly that you want to copy";
      this.byAssembly = true;
    }
    this.selectionFromInstruction2.Text = this.toolname + " from:";
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    foreach (ViewSheetItemEntry viewSheetItemEntry in this.Items)
    {
      if (viewSheetItemEntry.IsSelected)
      {
        this.SelectedItem = viewSheetItemEntry;
        break;
      }
    }
    if (this.SelectedItem == null)
    {
      if (this.byAssembly)
      {
        int num1 = (int) MessageBox.Show($"Please select one Mark Number to copy {this.toolname} from. Current selection is empty!", "Warning");
      }
      else
      {
        int num2 = (int) MessageBox.Show($"Please select one view sheet to copy {this.toolname} from. Current selection is empty!", "Warning");
      }
    }
    else
    {
      this.isContinue = true;
      this.Close();
    }
  }

  private void Button_Click_1(object sender, RoutedEventArgs e) => this.Close();

  private void ElementName_Checked(object sender, RoutedEventArgs e)
  {
    string str1 = sender.ToString();
    int num1 = str1.IndexOf(":");
    int num2 = str1.LastIndexOf(" ");
    string str2 = str1.Substring(num1 + 1, num2 - num1 - 1);
    foreach (ViewSheetItemEntry viewSheetItemEntry in this.Items)
    {
      if (!str2.Equals(viewSheetItemEntry.elemName))
        viewSheetItemEntry.IsSelected = false;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/copyticketannotation/viewsheetcopyfromdialog.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.selectionFromInstruction = (TextBlock) target;
        break;
      case 2:
        this.selectionFromInstruction2 = (TextBlock) target;
        break;
      case 3:
        this.lbTodoList = (ListBox) target;
        break;
      case 5:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
        break;
      case 6:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click_1);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 4)
      return;
    ((ToggleButton) target).Checked += new RoutedEventHandler(this.ElementName_Checked);
  }
}
