// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.ViewSheetCopyToDialog
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

public class ViewSheetCopyToDialog : Window, IComponentConnector
{
  private List<ViewSheetItemEntry> Items = new List<ViewSheetItemEntry>();
  private bool byAssembly = true;
  private string toolname = "";
  public bool isContinue;
  public List<ViewSheetItemEntry> SelectedItems = new List<ViewSheetItemEntry>();
  internal TextBlock selectionToInstruction;
  internal TextBlock selectionToInstruction2;
  internal ListBox lbTodoList;
  private bool _contentLoaded;

  public ViewSheetCopyToDialog(
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
      this.selectionToInstruction.TextAlignment = TextAlignment.Justify;
      this.selectionToInstruction.Text = "Please select one or more view sheets that you want to";
      this.byAssembly = false;
    }
    else
    {
      this.selectionToInstruction.TextAlignment = TextAlignment.Justify;
      this.selectionToInstruction.Text = "Please select one or more assemblies that you want to";
      this.byAssembly = true;
    }
    this.selectionToInstruction2.Text = $"copy {this.toolname} to:";
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    foreach (ViewSheetItemEntry viewSheetItemEntry in this.Items)
    {
      if (viewSheetItemEntry.IsSelected)
        this.SelectedItems.Add(viewSheetItemEntry);
    }
    if (this.SelectedItems.Count == 0)
    {
      if (this.byAssembly)
      {
        int num1 = (int) MessageBox.Show($"Please select one or more Mark Numbers to copy {this.toolname} to. Current selection is empty!", "Warning");
      }
      else
      {
        int num2 = (int) MessageBox.Show($"Please select one or more view sheets to copy {this.toolname} to. Current selection is empty!", "Warning");
      }
    }
    else
    {
      this.isContinue = true;
      this.Close();
    }
  }

  private void Button_Click_1(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/copyticketannotation/viewsheetcopytodialog.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.selectionToInstruction = (TextBlock) target;
        break;
      case 2:
        this.selectionToInstruction2 = (TextBlock) target;
        break;
      case 3:
        this.lbTodoList = (ListBox) target;
        break;
      case 4:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
        break;
      case 5:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click_1);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
