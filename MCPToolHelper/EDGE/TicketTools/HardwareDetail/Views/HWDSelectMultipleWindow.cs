// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HWDSelectMultipleWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HWDSelectMultipleWindow : Window, IComponentConnector, IStyleConnector
{
  public List<TemplateDataGridItem> dataGridItems = new List<TemplateDataGridItem>();
  private int numberSelected;
  internal DataGrid templateDataGrid;
  internal Button OKButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public HWDSelectMultipleWindow(List<string> TemplateNames, List<string> slectedTemplates)
  {
    this.InitializeComponent();
    this.dataGridItems.Clear();
    int num = 1;
    foreach (string slectedTemplate in slectedTemplates)
      this.dataGridItems.Add(new TemplateDataGridItem(slectedTemplate, true, num++.ToString()));
    List<string> list = this.dataGridItems.Select<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Template)).ToList<string>();
    foreach (string name in TemplateNames.Distinct<string>().NatrualSort<string>((Func<string, string>) (x => x)))
    {
      if (!list.Contains(name))
        this.dataGridItems.Add(new TemplateDataGridItem(name));
    }
    this.templateDataGrid.ItemsSource = (IEnumerable) this.dataGridItems;
  }

  public List<string> GetSelectedTemplates()
  {
    return ((IEnumerable<TemplateDataGridItem>) this.templateDataGrid.ItemsSource).Where<TemplateDataGridItem>((Func<TemplateDataGridItem, bool>) (x => x.Selected)).ToList<TemplateDataGridItem>().OrderBy<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Order)).ToList<TemplateDataGridItem>().Select<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Template)).ToList<string>();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e)
  {
    this.dataGridItems.Clear();
    this.Close();
  }

  private void CheckBox_Checked(object sender, RoutedEventArgs e)
  {
    IEnumerable<TemplateDataGridItem> itemsSource = (IEnumerable<TemplateDataGridItem>) this.templateDataGrid.ItemsSource;
    List<TemplateDataGridItem> list1 = itemsSource.Where<TemplateDataGridItem>((Func<TemplateDataGridItem, bool>) (x => x.Selected)).ToList<TemplateDataGridItem>();
    List<TemplateDataGridItem> list2 = itemsSource.Except<TemplateDataGridItem>((IEnumerable<TemplateDataGridItem>) list1).ToList<TemplateDataGridItem>();
    int num = list1.Count<TemplateDataGridItem>();
    if (this.numberSelected == num)
      return;
    this.numberSelected = num;
    foreach (TemplateDataGridItem templateDataGridItem in list1)
    {
      if (string.IsNullOrWhiteSpace(templateDataGridItem.Order))
      {
        templateDataGridItem.Order = num.ToString();
        break;
      }
    }
    List<TemplateDataGridItem> list3 = list1.OrderBy<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Order)).ToList<TemplateDataGridItem>();
    List<TemplateDataGridItem> list4 = list2.OrderBy<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Template)).ToList<TemplateDataGridItem>();
    List<TemplateDataGridItem> templateDataGridItemList = new List<TemplateDataGridItem>();
    templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list3);
    templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list4);
    this.templateDataGrid.ItemsSource = (IEnumerable) templateDataGridItemList;
  }

  private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    IEnumerable<TemplateDataGridItem> itemsSource = (IEnumerable<TemplateDataGridItem>) this.templateDataGrid.ItemsSource;
    List<TemplateDataGridItem> list1 = itemsSource.Where<TemplateDataGridItem>((Func<TemplateDataGridItem, bool>) (x => x.Selected)).ToList<TemplateDataGridItem>();
    List<TemplateDataGridItem> list2 = itemsSource.Except<TemplateDataGridItem>((IEnumerable<TemplateDataGridItem>) list1).ToList<TemplateDataGridItem>();
    int num1 = list1.Count<TemplateDataGridItem>();
    if (this.numberSelected == num1)
      return;
    this.numberSelected = num1;
    foreach (TemplateDataGridItem templateDataGridItem in list2)
      templateDataGridItem.Order = " ";
    int num2 = 1;
    foreach (TemplateDataGridItem templateDataGridItem in (IEnumerable<TemplateDataGridItem>) list1.OrderBy<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Order)))
      templateDataGridItem.Order = num2++.ToString();
    List<TemplateDataGridItem> list3 = list1.OrderBy<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Order)).ToList<TemplateDataGridItem>();
    List<TemplateDataGridItem> list4 = list2.OrderBy<TemplateDataGridItem, string>((Func<TemplateDataGridItem, string>) (x => x.Template)).ToList<TemplateDataGridItem>();
    List<TemplateDataGridItem> templateDataGridItemList = new List<TemplateDataGridItem>();
    templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list3);
    templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list4);
    this.templateDataGrid.ItemsSource = (IEnumerable) templateDataGridItemList;
  }

  private void OKButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void templateDataGrid_KeyDown(object sender, KeyEventArgs e)
  {
    IEnumerable<TemplateDataGridItem> itemsSource = (IEnumerable<TemplateDataGridItem>) this.templateDataGrid.ItemsSource;
    List<TemplateDataGridItem> list1 = itemsSource.Where<TemplateDataGridItem>((Func<TemplateDataGridItem, bool>) (x => x.Selected)).ToList<TemplateDataGridItem>();
    int count = list1.Count;
    if (count <= 1)
      return;
    if (e.Key.Equals((object) Key.Prior))
    {
      int selectedIndex = this.templateDataGrid.SelectedIndex;
      if (selectedIndex >= count || selectedIndex == 0)
        return;
      TemplateDataGridItem templateDataGridItem1 = list1[selectedIndex];
      list1.RemoveAt(selectedIndex);
      list1.Insert(selectedIndex - 1, templateDataGridItem1);
      int num = 1;
      foreach (TemplateDataGridItem templateDataGridItem2 in list1)
        templateDataGridItem2.Order = num++.ToString();
      List<TemplateDataGridItem> list2 = itemsSource.Except<TemplateDataGridItem>((IEnumerable<TemplateDataGridItem>) list1).ToList<TemplateDataGridItem>();
      List<TemplateDataGridItem> templateDataGridItemList = new List<TemplateDataGridItem>();
      templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list1);
      templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list2);
      this.templateDataGrid.ItemsSource = (IEnumerable) templateDataGridItemList;
      this.templateDataGrid.SelectedItem = (object) templateDataGridItem1;
    }
    else
    {
      if (!e.Key.Equals((object) Key.Next))
        return;
      int selectedIndex = this.templateDataGrid.SelectedIndex;
      if (selectedIndex >= count - 1)
        return;
      TemplateDataGridItem templateDataGridItem3 = list1[selectedIndex];
      list1.RemoveAt(selectedIndex);
      list1.Insert(selectedIndex + 1, templateDataGridItem3);
      int num = 1;
      foreach (TemplateDataGridItem templateDataGridItem4 in list1)
        templateDataGridItem4.Order = num++.ToString();
      List<TemplateDataGridItem> list3 = itemsSource.Except<TemplateDataGridItem>((IEnumerable<TemplateDataGridItem>) list1).ToList<TemplateDataGridItem>();
      List<TemplateDataGridItem> templateDataGridItemList = new List<TemplateDataGridItem>();
      templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list1);
      templateDataGridItemList.AddRange((IEnumerable<TemplateDataGridItem>) list3);
      this.templateDataGrid.ItemsSource = (IEnumerable) templateDataGridItemList;
      this.templateDataGrid.SelectedItem = (object) templateDataGridItem3;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/hardwaredetail/views/hwdselectmultiplewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.templateDataGrid = (DataGrid) target;
        this.templateDataGrid.PreviewKeyDown += new KeyEventHandler(this.templateDataGrid_KeyDown);
        break;
      case 3:
        this.OKButton = (Button) target;
        this.OKButton.Click += new RoutedEventHandler(this.OKButton_Click);
        break;
      case 4:
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
    if (connectionId != 2)
      return;
    ((Style) target).Setters.Add((SetterBase) new EventSetter()
    {
      Event = ToggleButton.CheckedEvent,
      Handler = (Delegate) new RoutedEventHandler(this.CheckBox_Checked)
    });
    ((Style) target).Setters.Add((SetterBase) new EventSetter()
    {
      Event = ToggleButton.UncheckedEvent,
      Handler = (Delegate) new RoutedEventHandler(this.CheckBox_Unchecked)
    });
  }
}
