// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.ViewModels.HWCreateWindowViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TicketManager.Resources;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.ViewModels;

internal class HWCreateWindowViewModel : CreateWindowViewModel
{
  public HWCreateWindowViewModel()
  {
  }

  public HWCreateWindowViewModel(
    CreateWindow window,
    string templateName,
    string constructionProduct,
    string sourceSheet,
    TemplateCreatorViewModel tempViewModel)
  {
    this.parentWindow = window;
    this.CloseCommand = new Command(new Command.ICommandOnExecute(((CreateWindowViewModel) this).ExecuteCloseCommand), new Command.ICommandOnCanExecute(((CreateWindowViewModel) this).CanExecuteCloseCommand));
    this.CreateCommand = new Command(new Command.ICommandOnExecute(((CreateWindowViewModel) this).ExecuteCreateCommand), new Command.ICommandOnCanExecute(((CreateWindowViewModel) this).CanExecuteCreateCommand));
    this.TemplateName = templateName;
    this.ConstructionProduct = constructionProduct;
    this.SourceSheet = sourceSheet;
    this.templateCreatorViewModel = tempViewModel;
    this.parentWindow.constructionProductTitleLabel.Visibility = Visibility.Hidden;
    this.parentWindow.construcionProductNameLabel.Visibility = Visibility.Hidden;
    this.ConstructionProduct = string.Empty;
  }
}
