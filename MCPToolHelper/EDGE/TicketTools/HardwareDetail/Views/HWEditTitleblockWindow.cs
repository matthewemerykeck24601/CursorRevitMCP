// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HWEditTitleblockWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.HardwareDetail.ViewModels;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HWEditTitleblockWindow : Window, IComponentConnector
{
  private HWTemplateCreatorViewModel masterWindow;
  internal TextBox currentTitleBlockLabel;
  internal ListBox titleblockListBox;
  internal Button selectButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public HWEditTitleblockWindow() => this.InitializeComponent();

  public HWEditTitleblockWindow(
    HWTemplateCreatorViewModel window,
    List<ComboBoxItemString> titleblocks,
    string currentTitleBlock)
  {
    this.InitializeComponent();
    List<ComboBoxItemString> comboBoxItemStringList = new List<ComboBoxItemString>((IEnumerable<ComboBoxItemString>) titleblocks.ToList<ComboBoxItemString>());
    int index = comboBoxItemStringList.FindIndex((Predicate<ComboBoxItemString>) (p => p.ValueString == currentTitleBlock));
    if (index != -1)
      comboBoxItemStringList.RemoveAt(index);
    titleblocks = new List<ComboBoxItemString>((IEnumerable<ComboBoxItemString>) comboBoxItemStringList.OrderBy<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (p => p.ValueString)).ToList<ComboBoxItemString>());
    this.DataContext = (object) new HWEditTitleblockViewModel(this, comboBoxItemStringList);
    this.masterWindow = window;
    if (string.IsNullOrWhiteSpace(currentTitleBlock))
      currentTitleBlock = window.TitleBlockString == null ? "Not Available" : window.TitleBlockString;
    this.currentTitleBlockLabel.Text += currentTitleBlock;
  }

  public void SetTittleBlockMainWindow(string titleBlock)
  {
    this.masterWindow.SetTittleBlock(titleBlock);
  }

  private void titleblockListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/hardwaredetail/views/hwedittitleblockwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.currentTitleBlockLabel = (TextBox) target;
        break;
      case 2:
        this.titleblockListBox = (ListBox) target;
        this.titleblockListBox.SelectionChanged += new SelectionChangedEventHandler(this.titleblockListBox_SelectionChanged);
        break;
      case 3:
        this.selectButton = (Button) target;
        break;
      case 4:
        this.cancelButton = (Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
