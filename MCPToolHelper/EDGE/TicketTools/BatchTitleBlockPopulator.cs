// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.BatchTitleBlockPopulator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools;

public class BatchTitleBlockPopulator : Window, IComponentConnector, IStyleConnector
{
  internal Label TitleLabel;
  internal Label InstructionLabel;
  internal TreeView treeView;
  internal Button ContinueButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public BatchTitleBlockPopulator() => this.InitializeComponent();

  public BatchTitleBlockPopulator(List<ParentClass> items, IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    this.DataContext = (object) new BatchTBViewModel(items);
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
  }

  public List<ParentClass> getReturnedList() => (this.DataContext as BatchTBViewModel).ItemsList;

  private void CheckBox_Checked_Parent(object sender, RoutedEventArgs e)
  {
    foreach (ParentClass parentClass in (IEnumerable) this.treeView.Items)
    {
      parentClass.ExpandedBool = true;
      foreach (HierarchyCheckBoxItems member1 in parentClass.Members)
      {
        member1.CheckedItemBool = true;
        foreach (Sheets member2 in member1.Members)
          member2.CheckedSheetBool = true;
      }
    }
    this.treeView.Items.Refresh();
  }

  private void CheckBox_Unchecked_Parent(object sender, RoutedEventArgs e)
  {
    foreach (ParentClass parentClass in (IEnumerable) this.treeView.Items)
    {
      parentClass.checkedItemBoolParent = false;
      foreach (HierarchyCheckBoxItems member1 in parentClass.Members)
      {
        member1.CheckedItemBool = false;
        foreach (Sheets member2 in member1.Members)
          member2.CheckedSheetBool = false;
      }
    }
    this.treeView.Items.Refresh();
  }

  private void CheckBox_Checked(object sender, RoutedEventArgs e)
  {
    string str = (sender as CheckBox).Content.ToString();
    foreach (ParentClass parentClass in (IEnumerable) this.treeView.Items)
    {
      bool flag = true;
      foreach (HierarchyCheckBoxItems member1 in parentClass.Members)
      {
        if (member1.AssemblyName.Trim().Equals(str.Trim()))
        {
          member1.ExpandedBool = true;
          member1.CheckedItemBool = true;
          foreach (Sheets member2 in member1.Members)
            member2.CheckedSheetBool = true;
        }
        if (!member1.CheckedItemBool)
          flag = false;
      }
      parentClass.checkedItemBoolParent = flag;
    }
    this.treeView.Items.Refresh();
  }

  private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    string str = (sender as CheckBox).Content.ToString();
    foreach (ParentClass parentClass in (IEnumerable) this.treeView.Items)
    {
      parentClass.checkedItemBoolParent = false;
      foreach (HierarchyCheckBoxItems member1 in parentClass.Members)
      {
        if (member1.AssemblyName.Trim().Equals(str.Trim()))
        {
          member1.CheckedItemBool = false;
          foreach (Sheets member2 in member1.Members)
            member2.CheckedSheetBool = false;
        }
      }
    }
    this.treeView.Items.Refresh();
  }

  private void CheckBox_Checked_Child(object sender, RoutedEventArgs e)
  {
    string name = (sender as CheckBox).Content.ToString();
    foreach (ParentClass parentClass in (IEnumerable) this.treeView.Items)
    {
      parentClass.checkedItemBoolParent = false;
      foreach (HierarchyCheckBoxItems member1 in parentClass.Members)
      {
        if (member1.Members.FindIndex((Predicate<Sheets>) (s => s.SheetName == name)) != -1)
        {
          member1.ExpandedBool = true;
          bool flag = true;
          foreach (Sheets member2 in member1.Members)
          {
            if (!member2.CheckedSheetBool)
              flag = false;
          }
          member1.CheckedItemBool = flag;
          this.treeView.Items.Refresh();
          break;
        }
      }
    }
    foreach (ParentClass parentClass in (IEnumerable) this.treeView.Items)
    {
      bool flag = true;
      foreach (HierarchyCheckBoxItems member in parentClass.Members)
      {
        if (!member.CheckedItemBool)
          flag = false;
      }
      if (flag)
        parentClass.checkedItemBoolParent = true;
    }
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    this.DialogResult = new bool?(true);
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void ExpandTree()
  {
  }

  private void Window_MouseDown(object sender, MouseButtonEventArgs e)
  {
    if (e.ChangedButton != MouseButton.Left)
      return;
    this.DragMove();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/batchtitleblockpopulator.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((UIElement) target).MouseDown += new MouseButtonEventHandler(this.Window_MouseDown);
        break;
      case 2:
        this.TitleLabel = (Label) target;
        break;
      case 3:
        this.InstructionLabel = (Label) target;
        break;
      case 4:
        this.treeView = (TreeView) target;
        break;
      case 8:
        this.ContinueButton = (Button) target;
        this.ContinueButton.Click += new RoutedEventHandler(this.ContinueButton_Click);
        break;
      case 9:
        this.CancelButton = (Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
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
    switch (connectionId)
    {
      case 5:
        ((ToggleButton) target).Checked += new RoutedEventHandler(this.CheckBox_Checked_Parent);
        ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.CheckBox_Unchecked_Parent);
        break;
      case 6:
        ((ToggleButton) target).Checked += new RoutedEventHandler(this.CheckBox_Checked);
        ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.CheckBox_Unchecked);
        break;
      case 7:
        ((ToggleButton) target).Checked += new RoutedEventHandler(this.CheckBox_Checked_Child);
        ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.CheckBox_Checked_Child);
        break;
    }
  }
}
