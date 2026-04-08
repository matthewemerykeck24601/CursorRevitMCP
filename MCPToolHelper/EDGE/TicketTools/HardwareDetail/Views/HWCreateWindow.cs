// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HWCreateWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.HardwareDetail.ViewModels;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HWCreateWindow : Window, IComponentConnector
{
  internal Label createTemplateTitleLabel;
  internal Label templateNameLabel;
  internal Label constructionProductTitleLabel;
  internal Label construcionProductNameLabel;
  internal Label sourceSheetTitleLabel;
  internal Label sourceSheetNameLabel;
  internal Label bomAlignmentSelection;
  internal RadioButton alignTop;
  internal RadioButton alignBottom;
  internal Label bomStackingSelection;
  internal RadioButton stackBOM;
  internal RadioButton independentBOM;
  internal Button createButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public HWCreateWindow()
  {
    this.InitializeComponent();
    this.DataContext = (object) new HWCreateWindowViewModel();
  }

  public HWCreateWindow(
    string templateName,
    string constructionProduct,
    string sourceSheet,
    HWTemplateCreatorViewModel tempViewModel,
    CreateWindow window)
  {
    this.InitializeComponent();
    this.DataContext = (object) new HWCreateWindowViewModel(window, templateName, constructionProduct, sourceSheet, (TemplateCreatorViewModel) tempViewModel);
  }

  private void HandleCheck(object sender, RoutedEventArgs e)
  {
    if (!(this.DataContext is CreateWindowViewModel dataContext))
      return;
    if (this.alignTop.IsChecked.GetValueOrDefault())
      dataContext.BOMjustification = BOMJustification.Top;
    if (!this.alignBottom.IsChecked.GetValueOrDefault())
      return;
    dataContext.BOMjustification = BOMJustification.Bottom;
  }

  private void stackBOM_Checked(object sender, RoutedEventArgs e)
  {
    if (!(this.DataContext is CreateWindowViewModel dataContext))
      return;
    if (this.stackBOM.IsChecked.GetValueOrDefault())
      dataContext.bStackSchedules = true;
    if (!this.independentBOM.IsChecked.GetValueOrDefault())
      return;
    dataContext.bStackSchedules = false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/hardwaredetail/views/hwcreatewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.createTemplateTitleLabel = (Label) target;
        break;
      case 2:
        this.templateNameLabel = (Label) target;
        break;
      case 3:
        this.constructionProductTitleLabel = (Label) target;
        break;
      case 4:
        this.construcionProductNameLabel = (Label) target;
        break;
      case 5:
        this.sourceSheetTitleLabel = (Label) target;
        break;
      case 6:
        this.sourceSheetNameLabel = (Label) target;
        break;
      case 7:
        this.bomAlignmentSelection = (Label) target;
        break;
      case 8:
        this.alignTop = (RadioButton) target;
        this.alignTop.Checked += new RoutedEventHandler(this.HandleCheck);
        break;
      case 9:
        this.alignBottom = (RadioButton) target;
        this.alignBottom.Checked += new RoutedEventHandler(this.HandleCheck);
        break;
      case 10:
        this.bomStackingSelection = (Label) target;
        break;
      case 11:
        this.stackBOM = (RadioButton) target;
        this.stackBOM.Checked += new RoutedEventHandler(this.stackBOM_Checked);
        break;
      case 12:
        this.independentBOM = (RadioButton) target;
        this.independentBOM.Checked += new RoutedEventHandler(this.stackBOM_Checked);
        break;
      case 13:
        this.createButton = (Button) target;
        break;
      case 14:
        this.cancelButton = (Button) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
