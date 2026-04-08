// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.Views.EditTitleblockWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.HardwareDetail.ViewModels;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TicketTemplateTools.ViewModels;
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
namespace EDGE.TicketTools.TicketManager.Views;

public class EditTitleblockWindow : Window, IComponentConnector
{
  private TicketTemplateCreatorViewModel masterWindow;
  private HWTemplateCreatorViewModel hwMasterWindow;
  internal TextBox currentTitleBlockLabel;
  internal ListBox titleblockListBox;
  internal Button selectButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public EditTitleblockWindow() => this.InitializeComponent();

  public EditTitleblockWindow(
    TicketTemplateCreatorViewModel window,
    List<ComboBoxItemString> titleblocks,
    string currentTitleBlock)
  {
    this.InitializeComponent();
    List<ComboBoxItemString> comboBoxItemStringList = new List<ComboBoxItemString>((IEnumerable<ComboBoxItemString>) titleblocks.ToList<ComboBoxItemString>());
    int index = comboBoxItemStringList.FindIndex((Predicate<ComboBoxItemString>) (p => p.ValueString == currentTitleBlock));
    if (index != -1)
      comboBoxItemStringList.RemoveAt(index);
    titleblocks = new List<ComboBoxItemString>((IEnumerable<ComboBoxItemString>) comboBoxItemStringList.OrderBy<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (p => p.ValueString)).ToList<ComboBoxItemString>());
    this.DataContext = (object) new EditTitleblockViewModel(this, comboBoxItemStringList);
    this.masterWindow = window;
    if (string.IsNullOrWhiteSpace(currentTitleBlock))
      currentTitleBlock = "Not Available";
    this.currentTitleBlockLabel.Text += currentTitleBlock;
  }

  public EditTitleblockWindow(
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
    this.DataContext = (object) new EditTitleblockViewModel(this, comboBoxItemStringList);
    this.masterWindow = (TicketTemplateCreatorViewModel) null;
    this.hwMasterWindow = window;
    if (string.IsNullOrWhiteSpace(currentTitleBlock))
      currentTitleBlock = "Not Available";
    this.currentTitleBlockLabel.Text += currentTitleBlock;
  }

  public void SetTittleBlockMainWindow(string titleBlock)
  {
    if (this.masterWindow != null)
      this.masterWindow.SetTittleBlock(titleBlock);
    else
      this.hwMasterWindow.SetTittleBlock(titleBlock);
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
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/tickettemplatetools/views/edittitleblockwindow.xaml", UriKind.Relative));
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
