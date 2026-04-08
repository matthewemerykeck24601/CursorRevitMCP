// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketTemplateTools.ViewModels.TicketTemplateCreatorViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TemplateToolsBase.Views;
using EDGE.TicketTools.TicketManager.Views;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Utils.TicketUtils;

#nullable disable
namespace EDGE.TicketTools.TicketTemplateTools.ViewModels;

public class TicketTemplateCreatorViewModel : TemplateCreatorViewModel
{
  public TicketTemplateCreatorViewModel()
  {
  }

  public TicketTemplateCreatorViewModel(
    TicketTemplateCreatorWindow window,
    AssemblyInstance assembly)
  {
    this.ticketMainWindow = window;
    this.templateType = "TICKET";
  }

  public new string TemplateString
  {
    get => this._templateString;
    set
    {
      if (!(this._templateString != value))
        return;
      this._templateString = value;
      this.NotifyPropertyChanged(nameof (TemplateString));
      if (value != null)
      {
        this.UpdateConstructionProduct(value);
        this.UpdateManufacturer(value);
        this.UpdateTitleBlock(value);
      }
      bool flag = false;
      foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
      {
        if (template.ValueString == value)
          flag = true;
      }
      if (flag && this.ticketMainWindow.templateNameComboBox.SelectedIndex != -1)
        this.ticketMainWindow.templateNameComboBox.SelectedIndex = -1;
      this.RefreshWindow();
    }
  }

  private new void UpdateTitleBlock(string templateName)
  {
    this.TitleBlockString = (string) null;
    this.titleBlockList.Clear();
    this._masterTitleBlockList.ForEach((Action<ComboBoxItemString>) (p => this.titleBlockList.Add(p)));
    if (this.TemplateSettings != null)
    {
      foreach (TicketTemplate template in this.TemplateSettings.Templates)
      {
        if (template.TemplateName == templateName.Trim())
        {
          this.TitleBlockString = template.TitleBlockName;
          return;
        }
      }
    }
    this.TitleBlockString = (string) null;
  }

  private void UpdateConstructionProduct(string templateName)
  {
    if (this.TemplateSettings != null)
    {
      foreach (TicketTemplate template in this.TemplateSettings.Templates)
      {
        if (template.TemplateName == templateName.Trim())
        {
          this.ConstructionProduct = template.ConstructionProduct;
          return;
        }
      }
    }
    this.ConstructionProduct = (string) null;
  }

  private void UpdateManufacturer(string templateName)
  {
    if (this.TemplateSettings == null)
      return;
    foreach (TicketTemplate template in this.TemplateSettings.Templates)
    {
      if (template.TemplateName == templateName.Trim())
      {
        this.ManufacturerString = template.TemplateManufacturerName;
        this.ticketMainWindow.manufacturerComboBox.SelectedValue = (object) this.ManufacturerString;
        break;
      }
    }
  }

  public new void ExecuteManageManufacturer()
  {
    try
    {
      ManufacturerManagementWindow managementWindow = new ManufacturerManagementWindow((TemplateCreatorViewModel) this);
      managementWindow.Owner = (Window) this.ticketMainWindow;
      this.ticketMainWindow.IsEnabled = false;
      managementWindow.ShowDialog();
      this.ticketMainWindow.IsEnabled = true;
      this.ticketMainWindow.manufacturerComboBox.Items.Refresh();
    }
    catch (Exception ex)
    {
      this.ticketMainWindow.IsEnabled = true;
    }
    this.ticketMainWindow.IsEnabled = true;
  }

  public new void ExecuteDeleteTemplateCommand()
  {
    if (!this.CanExecuteDeleteTemplateCommand())
      return;
    bool flag = false;
    try
    {
      flag = this.DeleteTemplate(this.TemplateString.Trim());
      if (flag)
      {
        this.TemplateString = (string) null;
        this.ticketMainWindow.templateNameComboBox.Text = (string) null;
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("There was an exception thrown attempting to delete an template" + ex.Message);
    }
    if (flag)
    {
      int num1 = (int) MessageBox.Show("Successfully Deleted the Template");
    }
    else
    {
      int num2 = (int) MessageBox.Show("The Template was not deleted.");
    }
  }

  public void ExecuteEditTitleblockCommand(object parameter)
  {
    if (!this.CanExecuteEditTitleblockCommand())
      return;
    try
    {
      this.UpdateTitleBlockList();
      EditTitleblockWindow titleblockWindow = new EditTitleblockWindow(this, this.titleBlockList, this.TitleBlockString);
      titleblockWindow.Owner = (Window) this.ticketMainWindow;
      this.ticketMainWindow.IsEnabled = false;
      titleblockWindow.ShowDialog();
      this.ticketMainWindow.IsEnabled = true;
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("Exception thrown editing title block: \n" + ex.Message);
      if (this.ticketMainWindow != null)
        this.ticketMainWindow.IsEnabled = true;
    }
    this.ticketMainWindow.IsEnabled = true;
  }

  public void ExecuteCreate()
  {
    if (!this.CanExecuteCreate())
      return;
    try
    {
      bool bHWD;
      if (this.CheckActiveView(out bHWD) && !bHWD)
      {
        string constructionParamAsString = TicketParamUtils.GetConstructionParamAsString(this.assembly);
        string name = this.ActiveView.Name;
        string templateName = this.TemplateString.Trim();
        CreateWindow createWindow = new CreateWindow(templateName, constructionParamAsString, name, this);
        createWindow.Owner = (Window) this.ticketMainWindow;
        this.ticketMainWindow.IsEnabled = false;
        createWindow.ShowDialog();
        this.ticketMainWindow.IsEnabled = true;
        this.templateList = this.AlhabetizeList(this.templateList);
        this.TemplateString = templateName;
      }
      else
      {
        new TaskDialog("Ticket Template Creator")
        {
          MainInstruction = "The current view is invalid for template creation",
          MainContent = (!bHWD ? "Please open a valid assembly sheet and try again." : "The Ticket Template Creator can only be run on sheets for assemblies that do not have the \"HARDWARE_DETAIL\" flag set. Please open a valid assembly sheet and try again."),
          CommonButtons = ((TaskDialogCommonButtons) 1)
        }.Show();
        this.ticketMainWindow.Focus();
      }
    }
    catch (Exception ex)
    {
      if (this.ticketMainWindow == null)
        return;
      this.ticketMainWindow.IsEnabled = true;
    }
  }
}
