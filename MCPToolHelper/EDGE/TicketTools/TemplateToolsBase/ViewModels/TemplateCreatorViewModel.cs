// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.ViewModels.TemplateCreatorViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.HardwareDetail;
using EDGE.TicketTools.HardwareDetail.Views;
using EDGE.TicketTools.TemplateToolsBase.Views;
using EDGE.TicketTools.TicketManager.Resources;
using EDGE.TicketTools.TicketManager.Views;
using EDGE.TicketTools.TicketPopulator;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using EDGE.TicketTools.TicketTemplateTools.ViewModels;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Utils.ElementUtils;
using Utils.TicketUtils;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.ViewModels;

public class TemplateCreatorViewModel : INotifyPropertyChanged
{
  public List<string> badNames = new List<string>();
  protected BOMJustification BOMJustification;
  protected List<TicketViewportInfo> viewportInfos;
  protected int overallTemplateScale = -1;
  protected List<AnchorGroup> anchorGroups;
  protected double viewportAlignmentTolerance = 0.1;
  public AssemblyInstance assembly;
  public Autodesk.Revit.DB.View ActiveView;
  public Document revitDoc;
  public UIDocument uiDoc;
  public TicketTemplateSettings TemplateSettings;
  public HardwareDetailTemplateSettings HWTemplateSettings;
  public TicketTemplateCreatorWindow ticketMainWindow;
  public HWTemplateCreatorWindow hwMainWindow;
  public TemplateCreatorWindow mainWindow;
  public string templateType;
  public ObservableCollection<ComboBoxItemString> _manufacturerList = new ObservableCollection<ComboBoxItemString>();
  public ObservableCollection<ComboBoxItemString> _templateList = new ObservableCollection<ComboBoxItemString>();
  public List<ComboBoxItemString> _titleBlockList = new List<ComboBoxItemString>();
  public List<ComboBoxItemString> _masterTitleBlockList = new List<ComboBoxItemString>();
  public string _manufacturerString = "";
  public string _templateString = "";
  public string _templateSettingsFilePath = "";
  public string _hardwareDetailTemplateSettingsPath = "";
  public string _constructionProduct = "";
  public string _titleBlockString = "";

  public ObservableCollection<ComboBoxItemString> manufacturerList
  {
    get => this._manufacturerList;
    set
    {
      if (this._manufacturerList == value)
        return;
      this._manufacturerList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.RefreshWindow();
      this.NotifyPropertyChanged(nameof (manufacturerList));
    }
  }

  public ObservableCollection<ComboBoxItemString> templateList
  {
    get => this._templateList;
    set
    {
      if (this._templateList == value)
        return;
      this._templateList = value == null ? new ObservableCollection<ComboBoxItemString>() : value;
      this.NotifyPropertyChanged(nameof (templateList));
    }
  }

  public List<ComboBoxItemString> titleBlockList
  {
    get => this._titleBlockList;
    set
    {
      if (this._titleBlockList == value)
        return;
      this._titleBlockList = value == null ? new List<ComboBoxItemString>() : value;
      this.NotifyPropertyChanged(nameof (titleBlockList));
    }
  }

  public string TemplateSettingsFilePath
  {
    get => this._templateSettingsFilePath;
    set
    {
      if (!(this._templateSettingsFilePath != value))
        return;
      this._templateSettingsFilePath = value;
      this.NotifyPropertyChanged(nameof (TemplateSettingsFilePath));
    }
  }

  public string HardwareDetailTemplateSettingsPath
  {
    get => this._hardwareDetailTemplateSettingsPath;
    set
    {
      if (!(this._hardwareDetailTemplateSettingsPath != value))
        return;
      this._hardwareDetailTemplateSettingsPath = value;
      this.NotifyPropertyChanged(nameof (HardwareDetailTemplateSettingsPath));
    }
  }

  public string ManufacturerString
  {
    get => this._manufacturerString;
    set
    {
      if (!(this._manufacturerString != value))
        return;
      this._manufacturerString = value;
      this.NotifyPropertyChanged(nameof (ManufacturerString));
    }
  }

  public string TitleBlockString
  {
    get => this._titleBlockString;
    set
    {
      if (!(this._titleBlockString != value))
        return;
      if (this.titleBlockList.FindIndex((Predicate<ComboBoxItemString>) (p => p.ValueString == value)) == -1 && !string.IsNullOrWhiteSpace(value))
        this.titleBlockList.Add(new ComboBoxItemString(value));
      this._titleBlockString = value;
      this.NotifyPropertyChanged(nameof (TitleBlockString));
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  public Command ManageManufacturerCommand { get; set; }

  public Command CreateCommand { get; set; }

  public Command CloseCommand { get; set; }

  public Command FileSelectionCommand { get; set; }

  public Command RenameTemplateCommand { get; set; }

  public Command DeleteTemplateCommand { get; set; }

  public Command SaveCommand { get; set; }

  public Command SaveAndCloseCommand { get; set; }

  public Command HelpCommand { get; set; }

  public Command EditTitleblockCommand { get; set; }

  public TemplateCreatorViewModel()
  {
  }

  public TemplateCreatorViewModel(HWTemplateCreatorWindow window, AssemblyInstance assembly)
  {
    this.hwMainWindow = window;
  }

  public TemplateCreatorViewModel(TicketTemplateCreatorWindow window, AssemblyInstance assembly)
  {
    this.ticketMainWindow = window;
  }

  public TemplateCreatorViewModel(TemplateCreatorWindow window, AssemblyInstance assembly)
  {
    this.mainWindow = window;
  }

  public bool CanExecuteCreate()
  {
    return !string.IsNullOrWhiteSpace(this.TemplateString) && !this.withinFamilyDocument();
  }

  public void ExecuteCloseCommand()
  {
    try
    {
      this.ticketMainWindow.Close();
    }
    catch (Exception ex)
    {
    }
  }

  public virtual void ExecuteFileSelectionCommand()
  {
    try
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.AddExtension = true;
      saveFileDialog.CheckPathExists = true;
      saveFileDialog.DefaultExt = ".xml";
      if (File.Exists(this.TemplateSettingsFilePath))
        saveFileDialog.InitialDirectory = this.TemplateSettingsFilePath;
      saveFileDialog.Filter = "Settings Files (*.xml, *.XML)|*.xml;*.XML";
      saveFileDialog.Title = "Select settings file or specify new settings file name";
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
        return;
      this.TemplateSettingsFilePath = saveFileDialog.FileName;
      if (this.TemplateSettings != null)
      {
        this.TemplateSettings.manufacurerList.Clear();
        this.TemplateSettings.Templates.Clear();
      }
      this.ReadTemplateSettings();
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("An Exceptions occured while attempting to select or read a new Template Settings File:\n" + ex.Message);
    }
  }

  public virtual void ExecuteRenameTemplateCommand()
  {
    if (!this.CanExecuteRenameTemplateCommand())
      return;
    try
    {
      NewManufacturer newManufacturer = new NewManufacturer(this);
      newManufacturer.Owner = (Window) this.ticketMainWindow;
      this.ticketMainWindow.IsEnabled = false;
      newManufacturer.ShowDialog();
      this.ticketMainWindow.IsEnabled = true;
      this.templateList = this.AlhabetizeList(this.templateList);
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("An Exception was thrown attempting to rename a template:\n" + ex.Message);
      this.ticketMainWindow.IsEnabled = true;
    }
    this.ticketMainWindow.IsEnabled = true;
  }

  public bool CanExecuteRenameTemplateCommand()
  {
    return this.templateList != null && this.TemplateString != null && this.templateList.Any<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (a => a.ValueString == this.TemplateString.Trim()));
  }

  public bool CanExecuteDeleteTemplateCommand()
  {
    return this.templateList != null && this.TemplateString != null && this.templateList.Any<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (a => a.ValueString == this.TemplateString.Trim()));
  }

  public void ExecuteSaveCommand()
  {
    try
    {
      this.CheckTemplateManufacturer();
      this.CheckTemplateTitleBlock();
      if (this.TemplateSettings == null)
        this.TemplateSettings = new TicketTemplateSettings();
      if (this.TemplateSettings.SaveTemplateSettings(this.TemplateSettingsFilePath, this.manufacturerList))
      {
        int num = (int) System.Windows.MessageBox.Show("Saved Settings Successfully");
        TaskDialog taskDialog1 = new TaskDialog("EDGE^R");
        taskDialog1.MainInstruction = "Some schedules were not saved.";
        taskDialog1.MainContent = "One or more schedules could not be saved to the ticket template due to the naming convention. Template schedule names must begin with \"z_\" or \"ZZ_\" in order to be saved to the ticket template. Expand for details.";
        if (this.badNames.Count <= 0)
          return;
        foreach (string badName in this.badNames)
        {
          TaskDialog taskDialog2 = taskDialog1;
          taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{badName}\n";
        }
        taskDialog1.Show();
        this.badNames.Clear();
      }
      else
      {
        int num1 = (int) System.Windows.MessageBox.Show("Ticket Settings failed to save.");
      }
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("Exception Thrown saving tickets: " + ex.Message);
    }
  }

  public void ExecuteSaveAndCloseCommand()
  {
  }

  public void ExecuteHelpCommand(object parameter)
  {
    Process.Start("http://www.edge.ptac.com/#!ticket-template-creator/c43bl");
  }

  public void ExecuteEditTitleblockCommand()
  {
    if (!this.CanExecuteEditTitleblockCommand())
      return;
    try
    {
      this.UpdateTitleBlockList();
      EditTitleblockWindow titleblockWindow = new EditTitleblockWindow((TicketTemplateCreatorViewModel) this, this.titleBlockList, this.TitleBlockString);
      titleblockWindow.Owner = (Window) this.ticketMainWindow;
      this.ticketMainWindow.IsEnabled = false;
      titleblockWindow.ShowDialog();
      this.ticketMainWindow.IsEnabled = true;
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("Exception thrown editing title block: \n" + ex.Message);
      this.ticketMainWindow.IsEnabled = true;
    }
    this.ticketMainWindow.IsEnabled = true;
  }

  public bool CanExecuteEditTitleblockCommand()
  {
    return this.templateList != null && this.TemplateString != null && this.templateList.Any<ComboBoxItemString>((Func<ComboBoxItemString, bool>) (a => a.ValueString == this.TemplateString.Trim())) && !this.withinFamilyDocument();
  }

  public string TemplateString
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

  public virtual void UpdateTitleBlock(string templateName)
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

  public string ConstructionProduct
  {
    get => this._constructionProduct;
    set
    {
      if (!(this._constructionProduct != value))
        return;
      if (string.IsNullOrWhiteSpace(value))
      {
        this._constructionProduct = value;
        this.NotifyPropertyChanged(nameof (ConstructionProduct));
        this.ticketMainWindow.constructionProductTitleLabel.Visibility = System.Windows.Visibility.Hidden;
        this.ticketMainWindow.constructionProductLabel.Visibility = System.Windows.Visibility.Hidden;
      }
      else
      {
        this._constructionProduct = value;
        this.NotifyPropertyChanged(nameof (ConstructionProduct));
        this.ticketMainWindow.constructionProductTitleLabel.Visibility = System.Windows.Visibility.Hidden;
        this.ticketMainWindow.constructionProductLabel.Visibility = System.Windows.Visibility.Hidden;
        this.ticketMainWindow.constructionProductLabel.Content = (object) this._constructionProduct;
      }
    }
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

  public void ExecuteManageManufacturer()
  {
    try
    {
      ManufacturerManagementWindow managementWindow = new ManufacturerManagementWindow(this);
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

  public void ExecuteDeleteTemplateCommand()
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
      int num = (int) System.Windows.MessageBox.Show("There was an exception thrown attempting to delete an template" + ex.Message);
    }
    if (flag)
    {
      int num1 = (int) System.Windows.MessageBox.Show("Successfully Deleted the Template");
    }
    else
    {
      int num2 = (int) System.Windows.MessageBox.Show("The Template was not deleted.");
    }
  }

  public bool DeleteTemplate(string templateToDelete)
  {
    bool flag = false;
    if (this.TemplateSettings != null)
    {
      List<TicketTemplate> ticketTemplateList = new List<TicketTemplate>();
      foreach (TicketTemplate template in this.TemplateSettings.Templates)
      {
        if (template.TemplateName == templateToDelete)
        {
          ticketTemplateList.Add(template);
          break;
        }
      }
      foreach (TicketTemplate ticketTemplate in ticketTemplateList)
      {
        this.TemplateSettings.Templates.Remove(ticketTemplate);
        flag = true;
      }
      if (flag)
      {
        int index = -1;
        int num = 0;
        foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
        {
          if (template.ValueString == templateToDelete)
          {
            index = num;
            break;
          }
          ++num;
        }
        if (index > -1)
        {
          this.templateList.RemoveAt(index);
          this.NotifyPropertyChanged("templateList");
          this.ticketMainWindow.templateNameComboBox.Items.Refresh();
        }
      }
    }
    else
    {
      List<HWDetailTemplate> hwDetailTemplateList = new List<HWDetailTemplate>();
      foreach (HWDetailTemplate template in this.HWTemplateSettings.Templates)
      {
        if (template.TemplateName == templateToDelete)
        {
          hwDetailTemplateList.Add(template);
          break;
        }
      }
      foreach (HWDetailTemplate hwDetailTemplate in hwDetailTemplateList)
      {
        this.HWTemplateSettings.Templates.Remove(hwDetailTemplate);
        flag = true;
      }
      if (flag)
      {
        int index = -1;
        int num = 0;
        foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
        {
          if (template.ValueString == templateToDelete)
          {
            index = num;
            break;
          }
          ++num;
        }
        if (index > -1)
        {
          this.templateList.RemoveAt(index);
          this.NotifyPropertyChanged("templateList");
          this.hwMainWindow.templateNameComboBox.Items.Refresh();
        }
      }
    }
    return true;
  }

  public void UpdateTemplateString(string str) => this._templateString = str;

  public void NotifyPropertyChanged(string name)
  {
    PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
    if (propertyChanged == null)
      return;
    propertyChanged((object) this, new PropertyChangedEventArgs(name));
  }

  public virtual void LoadForm()
  {
    if (App.TicketTemplateSettingsPath != "")
    {
      this.TemplateSettingsFilePath = App.TicketTemplateSettingsPath;
      this.ReadTemplateSettings();
    }
    this.UpdateTitleBlockList();
  }

  public virtual void RenameTemplate(string additionString)
  {
    foreach (TicketTemplate template in this.TemplateSettings.Templates)
    {
      if (template.TemplateName == this.TemplateString.Trim())
        template.TemplateName = additionString.Trim();
    }
    foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
    {
      if (template.ValueString == this.TemplateString.Trim())
        template.ValueString = additionString.Trim();
    }
    this.UpdateSelectedTemplateListItem(additionString.Trim());
    this.ticketMainWindow.templateNameComboBox.Items.Refresh();
  }

  public virtual void UpdateSelectedTemplateListItem(string templateName)
  {
    this.ticketMainWindow.templateNameComboBox.SelectedItem = (object) null;
    this.ticketMainWindow.templateNameComboBox.SelectedValue = (object) null;
    foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
    {
      if (template.ValueString.Equals(templateName))
      {
        this.ticketMainWindow.templateNameComboBox.SelectedItem = (object) template;
        this.ticketMainWindow.templateNameComboBox.SelectedValue = (object) template.ValueString;
        this.ticketMainWindow.templateNameComboBox.Items.Refresh();
      }
    }
  }

  public void DeleteManufacturer(string manufacturerToDelete)
  {
    int index = -1;
    int num = 0;
    foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerList)
    {
      if (manufacturer.ValueString == manufacturerToDelete)
      {
        index = num;
        break;
      }
      ++num;
    }
    if (index <= -1)
      return;
    this.manufacturerList.RemoveAt(index);
    this.NotifyPropertyChanged("manufacturerList");
    this.RefreshWindow();
  }

  public ObservableCollection<ComboBoxItemString> AlhabetizeList(
    ObservableCollection<ComboBoxItemString> manufacturers)
  {
    return new ObservableCollection<ComboBoxItemString>((IEnumerable<ComboBoxItemString>) manufacturers.OrderBy<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (p => p.ValueString)));
  }

  public virtual bool CreateTemplateFromViewSheet(
    BOMJustification BOMjustification,
    bool bStackSchedules)
  {
    Transform AssemblyTransform = (Transform) null;
    if (this.assembly != null)
      AssemblyTransform = this.assembly.GetTransform();
    if (this.TemplateSettings == null)
      this.TemplateSettings = new TicketTemplateSettings();
    string str = this.TemplateString.Trim();
    this.BOMJustification = BOMjustification;
    ViewSheet templateSheet = this.ActiveView as ViewSheet;
    string manufacturerString = this.ManufacturerString;
    TicketTemplate ticketTemplate = new TicketTemplate();
    this.anchorGroups = new List<AnchorGroup>();
    bool flag = true;
    string constructionParamAsString = TicketParamUtils.GetConstructionParamAsString(this.assembly);
    XYZ titleBlockLocation = (XYZ) null;
    string message = "";
    string titleBlockName = "";
    Outline outline1 = TitleBlockGeometryAnalyzer.Analyze(templateSheet, out titleBlockLocation, out titleBlockName, out message);
    if (outline1.IsEmpty)
    {
      int num1 = (int) System.Windows.MessageBox.Show($"Titleblock Analyzer Failed to find usable area: {message}.  All geometry will be referenced to 0,0");
    }
    this.viewportInfos = new List<TicketViewportInfo>();
    ICollection<ElementId> allViewports = templateSheet.GetAllViewports();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (ElementId id in (IEnumerable<ElementId>) allViewports)
    {
      if (this.revitDoc.GetElement(id) is Viewport element1)
      {
        Autodesk.Revit.DB.View element = this.revitDoc.GetElement(element1.ViewId) as Autodesk.Revit.DB.View;
        if (TicketViewportInfo.IsTicketTemplateView(element))
        {
          string name = element.ViewTemplateId == ElementId.InvalidElementId ? "" : this.revitDoc.GetElement(element.ViewTemplateId).Name;
          bool bCompatibility = false;
          double viewCropRotation = EDGERCreateTemplate.GetSectionViewCropRotation(element as ViewSection, AssemblyTransform, bCompatibility);
          TicketViewportInfo ticketViewportInfo = new TicketViewportInfo(element.Name, name, element1.Rotation, viewCropRotation, element.Scale);
          ticketViewportInfo.SetViewOutline(element1.GetBoxOutline());
          ticketViewportInfo.SetBoxCenterPoint(element1.GetBoxCenter());
          this.viewportInfos.Add(ticketViewportInfo);
          if (this.overallTemplateScale == -1)
            this.overallTemplateScale = element.Scale;
        }
      }
    }
    foreach (TicketViewportInfo viewportInfo1 in this.viewportInfos.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.mSectionViewOrientation == TemplateViewOrientation.ElevationTop || s.mSectionViewOrientation == TemplateViewOrientation.ElevationBottom)).ToList<TicketViewportInfo>())
    {
      AnchorGroup anchorGroup = new AnchorGroup();
      this.viewportInfos.Remove(viewportInfo1);
      anchorGroup.SetView(AnchorPosition.Main, viewportInfo1);
      XYZ anchorCenterpoint = viewportInfo1.GetBoxCenterPoint();
      IEnumerable<TicketViewportInfo> source1 = this.viewportInfos.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && s.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom && s.mSectionViewOrientation != TemplateViewOrientation.ThreeDOrtho && TicketPopCreationUtils.WithinViewAlignmentTolerance(new XYZ(0.0, s.GetBoxCenterPoint().Y, 0.0), new XYZ(0.0, anchorCenterpoint.Y, 0.0), this.viewportAlignmentTolerance)));
      IEnumerable<TicketViewportInfo> source2 = (IEnumerable<TicketViewportInfo>) source1.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().X < anchorCenterpoint.X)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().X));
      if (source2.Any<TicketViewportInfo>())
      {
        TicketViewportInfo viewportInfo2 = source2.ToList<TicketViewportInfo>().Last<TicketViewportInfo>();
        if (viewportInfo2.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo2.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
        {
          anchorGroup.SetView(AnchorPosition.Left, viewportInfo2);
          this.viewportInfos.Remove(source2.ToList<TicketViewportInfo>().Last<TicketViewportInfo>());
        }
      }
      IEnumerable<TicketViewportInfo> source3 = (IEnumerable<TicketViewportInfo>) source1.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().X > anchorCenterpoint.X)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().X));
      if (source3.Any<TicketViewportInfo>())
      {
        TicketViewportInfo viewportInfo3 = source3.ToList<TicketViewportInfo>().First<TicketViewportInfo>();
        if (viewportInfo3.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo3.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
        {
          anchorGroup.SetView(AnchorPosition.Right, viewportInfo3);
          this.viewportInfos.Remove(source3.ToList<TicketViewportInfo>().First<TicketViewportInfo>());
        }
      }
      IEnumerable<TicketViewportInfo> source4 = this.viewportInfos.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && s.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom && s.mSectionViewOrientation != TemplateViewOrientation.ThreeDOrtho && TicketPopCreationUtils.WithinViewAlignmentTolerance(new XYZ(0.0, s.GetBoxCenterPoint().X, 0.0), new XYZ(0.0, anchorCenterpoint.X, 0.0), this.viewportAlignmentTolerance)));
      IEnumerable<TicketViewportInfo> source5 = (IEnumerable<TicketViewportInfo>) source4.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().Y > anchorCenterpoint.Y)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().Y));
      if (source5.Any<TicketViewportInfo>())
      {
        TicketViewportInfo viewportInfo4 = source5.ToList<TicketViewportInfo>().First<TicketViewportInfo>();
        if (viewportInfo4.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo4.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
        {
          anchorGroup.SetView(AnchorPosition.Top, viewportInfo4);
          this.viewportInfos.Remove(source5.ToList<TicketViewportInfo>().First<TicketViewportInfo>());
        }
      }
      IEnumerable<TicketViewportInfo> source6 = (IEnumerable<TicketViewportInfo>) source4.Where<TicketViewportInfo>((Func<TicketViewportInfo, bool>) (s => s.GetBoxCenterPoint().Y < anchorCenterpoint.Y)).OrderBy<TicketViewportInfo, double>((Func<TicketViewportInfo, double>) (s => s.GetBoxCenterPoint().Y));
      if (source6.Any<TicketViewportInfo>())
      {
        TicketViewportInfo viewportInfo5 = source6.ToList<TicketViewportInfo>().Last<TicketViewportInfo>();
        if (viewportInfo5.mSectionViewOrientation != TemplateViewOrientation.ElevationTop && viewportInfo5.mSectionViewOrientation != TemplateViewOrientation.ElevationBottom)
        {
          anchorGroup.SetView(AnchorPosition.Bottom, viewportInfo5);
          this.viewportInfos.Remove(source6.ToList<TicketViewportInfo>().Last<TicketViewportInfo>());
        }
      }
      Outline outline2 = anchorGroup.GetOutline();
      if (PopulatorQA.inHouse)
        TemplateDebugUtils.DebugDrawOutline(outline2, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet);
      XYZ RevitVector = EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline2) - EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1);
      anchorGroup.simpleVectorToUperLeft = new SimpleVector(RevitVector);
      this.anchorGroups.Add(anchorGroup);
    }
    foreach (TicketViewportInfo viewportInfo in this.viewportInfos)
    {
      AnchorGroup anchorGroup = new AnchorGroup();
      anchorGroup.SetView(AnchorPosition.Main, viewportInfo);
      viewportInfo.GetBoxCenterPoint();
      Outline outline3 = anchorGroup.GetOutline();
      if (PopulatorQA.inHouse)
        TemplateDebugUtils.DebugDrawOutline(outline3, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet);
      XYZ RevitVector = EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline3) - EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1);
      anchorGroup.simpleVectorToUperLeft = new SimpleVector(RevitVector);
      this.anchorGroups.Add(anchorGroup);
    }
    ticketTemplate.AnchorGroups.AddRange((IEnumerable<AnchorGroup>) this.anchorGroups);
    ticketTemplate.TemplateName = str;
    ticketTemplate.TemplateManufacturerName = manufacturerString;
    ticketTemplate.TemplateScale = this.overallTemplateScale;
    ticketTemplate.ScaleUnits = ScalesManager.GetScaleUnitsForDocument(this.revitDoc);
    ticketTemplate.TitleBlockName = titleBlockName;
    ticketTemplate.TitleLocationToUpperLeftCorner = new SimpleVector(EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1) - titleBlockLocation);
    ticketTemplate.BOMJustification = this.BOMJustification;
    ticketTemplate.bStackSchedules = bStackSchedules;
    ticketTemplate.ConstructionProduct = constructionParamAsString;
    XYZ outlineUpperLeftCorner = EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1);
    foreach (ElementId id in (IEnumerable<ElementId>) allViewports)
    {
      if (this.revitDoc.GetElement(id) is Viewport element2 && this.revitDoc.GetElement(element2.ViewId) is Autodesk.Revit.DB.View element3 && element3.ViewType == ViewType.Legend)
      {
        SimpleVector vectorToUpperLeft = new SimpleVector(EDGERCreateTemplate.GetOutlineUpperLeftCorner(element2.GetBoxOutline()) - outlineUpperLeftCorner);
        bool strandPatternTemplate = element3.Name.ToUpper().Contains("END PATTERN TEMPLATE") || element3.Name.ToUpper().Contains("STRAND PATTERN TEMPLATE") || element3.Name.ToUpper().Contains("REINFORCING PATTERN TEMPLATE");
        ticketTemplate.LegendInfos.Add(new TicketLegendInfo(element3.Name, vectorToUpperLeft, strandPatternTemplate));
      }
    }
    IEnumerable<ScheduleSheetInstance> source7 = new FilteredElementCollector(this.revitDoc, templateSheet.Id).OfClass(typeof (ScheduleSheetInstance)).ToElements().Cast<ScheduleSheetInstance>();
    this.badNames = new List<string>();
    TaskDialog taskDialog = new TaskDialog("EDGE^R")
    {
      MainInstruction = "Some schedules were not saved.",
      MainContent = "One or more schedules could not be saved to the ticket template due to the naming convention. Template schedule names must begin with \"z_\" or \"ZZ_\" in order to be saved to the ticket template. Expand for details."
    };
    if (source7.Any<ScheduleSheetInstance>())
    {
      if (!ticketTemplate.bStackSchedules)
      {
        foreach (ScheduleSheetInstance scheduleSheetInstance in source7)
        {
          TicketScheduleInfo ticketScheduleInfo = new TicketScheduleInfo();
          ticketScheduleInfo.BOMJustification = this.BOMJustification;
          bool Conforms;
          ticketScheduleInfo.SetBOMScheduleNameStringFromScheduleName(scheduleSheetInstance.Name, this.assembly.Name, out Conforms);
          flag = Conforms & flag;
          if (!Conforms)
          {
            this.badNames.Add(scheduleSheetInstance.Name);
          }
          else
          {
            ticketScheduleInfo.iScheduleOrderIndex = -1;
            XYZ point;
            if (ticketScheduleInfo.BOMJustification == BOMJustification.Bottom)
            {
              point = scheduleSheetInstance.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min + new XYZ(1.0 / 144.0, 1.0 / 144.0, 0.0);
              if (PopulatorQA.inHouse)
                TemplateDebugUtils.DebugDrawCircleAtPoint(point, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet, "BOM PlacementPoint");
            }
            else
            {
              BoundingBoxXYZ boundingBoxXyz = scheduleSheetInstance.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet);
              point = new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z) + new XYZ(1.0 / 144.0, -1.0 / 144.0, 0.0);
              if (PopulatorQA.inHouse)
                TemplateDebugUtils.DebugDrawCircleAtPoint(point, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet, "BOM PlacementPoint");
            }
            if (PopulatorQA.inHouse)
              TemplateDebugUtils.DebugDrawCircleAtPoint(titleBlockLocation, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet, "TitleBlockLocationPoint");
            ticketScheduleInfo.vectorToAnchorPoint = new SimpleVector(point - titleBlockLocation);
            ticketTemplate.ScheduleInfos.Add(ticketScheduleInfo);
          }
        }
      }
      else
      {
        IEnumerable<ScheduleSheetInstance> source8 = this.BOMJustification == BOMJustification.Top ? (IEnumerable<ScheduleSheetInstance>) source7.OrderByDescending<ScheduleSheetInstance, double>((Func<ScheduleSheetInstance, double>) (s => s.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min.Y)) : (IEnumerable<ScheduleSheetInstance>) source7.OrderBy<ScheduleSheetInstance, double>((Func<ScheduleSheetInstance, double>) (s => s.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min.Y));
        if (source8.Any<ScheduleSheetInstance>())
        {
          int num2 = 0;
          foreach (ScheduleSheetInstance scheduleSheetInstance in source8)
          {
            TicketScheduleInfo ticketScheduleInfo = new TicketScheduleInfo();
            bool Conforms;
            ticketScheduleInfo.SetBOMScheduleNameStringFromScheduleName(scheduleSheetInstance.Name, this.assembly.Name, out Conforms);
            flag &= Conforms;
            if (!Conforms)
            {
              this.badNames.Add(scheduleSheetInstance.Name);
            }
            else
            {
              ticketScheduleInfo.iScheduleOrderIndex = num2;
              ticketTemplate.ScheduleInfos.Add(ticketScheduleInfo);
              ++num2;
            }
          }
          ScheduleSheetInstance scheduleSheetInstance1 = source8.First<ScheduleSheetInstance>();
          XYZ point;
          if (this.BOMJustification == BOMJustification.Bottom)
          {
            point = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet).Min + new XYZ(1.0 / 144.0, 1.0 / 144.0, 0.0);
            if (PopulatorQA.inHouse)
              TemplateDebugUtils.DebugDrawCircleAtPoint(point, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet, "BOM PlacementPoint");
          }
          else
          {
            BoundingBoxXYZ boundingBoxXyz = scheduleSheetInstance1.get_BoundingBox((Autodesk.Revit.DB.View) templateSheet);
            point = new XYZ(boundingBoxXyz.Min.X, boundingBoxXyz.Max.Y, boundingBoxXyz.Max.Z) + new XYZ(1.0 / 144.0, -1.0 / 144.0, 0.0);
            if (PopulatorQA.inHouse)
              TemplateDebugUtils.DebugDrawCircleAtPoint(point, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet, "BOM PlacementPoint");
          }
          if (PopulatorQA.inHouse)
            TemplateDebugUtils.DebugDrawCircleAtPoint(titleBlockLocation, this.revitDoc, (Autodesk.Revit.DB.View) templateSheet, "TitleBlockLocationPoint");
          ticketTemplate.BOMAnchorPosition = new SimpleVector(point - titleBlockLocation);
        }
      }
    }
    int index = this.TemplateSettings.Templates.FindIndex((Predicate<TicketTemplate>) (s => s.TemplateName == ticketTemplate.TemplateName));
    if (index == -1)
    {
      this.TemplateSettings.Templates.Add(ticketTemplate);
      this.templateList.Add(new ComboBoxItemString(ticketTemplate.TemplateName));
      this.templateList.OrderBy<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (s => s.ValueString));
    }
    else
      this.TemplateSettings.Templates[index] = ticketTemplate;
    this.ConstructionProduct = ticketTemplate.ConstructionProduct;
    this.TitleBlockString = ticketTemplate.TitleBlockName;
    return true;
  }

  public virtual void RefreshWindow()
  {
    bool flag = false;
    foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerList)
    {
      if (manufacturer.ValueString == null)
        flag = true;
    }
    ObservableCollection<ComboBoxItemString> observableCollection = new ObservableCollection<ComboBoxItemString>();
    if (!flag)
    {
      observableCollection.Add(new ComboBoxItemString((string) null));
      foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerList)
        observableCollection.Add(manufacturer);
      this.manufacturerList = observableCollection;
    }
    this.NotifyPropertyChanged("manufacturerList");
    this.ticketMainWindow.manufacturerComboBox.Items.Refresh();
    this.ticketMainWindow.templateNameComboBox.Items.Refresh();
  }

  public void SetTittleBlock(string titleBlock) => this.TitleBlockString = titleBlock;

  protected virtual bool ReadTemplateSettings()
  {
    bool flag1 = true;
    if (this.TemplateSettings == null)
      this.TemplateSettings = new TicketTemplateSettings();
    string templateString = this.TemplateString;
    string manufacturerString = this.ManufacturerString;
    FileInfo fileInfo = new FileInfo(this.TemplateSettingsFilePath);
    if (fileInfo.Exists)
    {
      if (!this.TemplateSettings.LoadTicketTemplateSettings(fileInfo.FullName) || fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
        return false;
      if (this.templateList == null)
        this.templateList = new ObservableCollection<ComboBoxItemString>();
      if (this.manufacturerList == null)
        this.manufacturerList = new ObservableCollection<ComboBoxItemString>();
      this.templateList.Clear();
      this.manufacturerList.Clear();
      this.manufacturerList.Add(new ComboBoxItemString((string) null));
      foreach (ComboBoxItemString manufacurer in (Collection<ComboBoxItemString>) this.TemplateSettings.manufacurerList)
        this.manufacturerList.Add(manufacurer);
      HashSet<string> source1 = new HashSet<string>();
      List<string> source2 = new List<string>();
      foreach (TicketTemplate template in this.TemplateSettings.Templates)
      {
        source2.Add(template.TemplateName);
        if (template.TemplateManufacturerName != null && !source1.Contains(template.TemplateManufacturerName))
          source1.Add(template.TemplateManufacturerName);
      }
      try
      {
        if (source2 != null && source2.Count != 0)
        {
          string[] array = source2.ToArray<string>();
          Array.Sort<string>(array);
          foreach (string str in array)
            this.templateList.Add(new ComboBoxItemString(str));
        }
        if (source1 != null && source1.Count != 0)
        {
          string[] array = source1.ToArray<string>();
          Array.Sort<string>(array);
          foreach (string str in array)
          {
            bool flag2 = true;
            foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.manufacturerList)
            {
              if (manufacturer.ValueString != null && manufacturer.ValueString.ToUpper().Equals(str.ToUpper()))
                flag2 = false;
            }
            if (flag2)
              this.manufacturerList.Add(new ComboBoxItemString(str));
          }
        }
        for (int index = 1; index < this.manufacturerList.Count - 1; ++index)
        {
          if (this.manufacturerList[index].ValueString == null)
            this.manufacturerList.RemoveAt(index);
        }
      }
      catch (Exception ex)
      {
        int num = (int) System.Windows.MessageBox.Show(ex.ToString());
      }
      flag1 = true;
    }
    else
    {
      TaskDialog.Show("Error", $"File not found at Template Settings Path Location: {this.TemplateSettingsFilePath}.New settings will be saved here, or you can specify a new location.");
      App.TicketTemplateSettingsPath = this.TemplateSettingsFilePath;
      this.TemplateSettings.Clear();
    }
    return flag1;
  }

  public virtual void UpdateTitleBlockList()
  {
    this._masterTitleBlockList.Clear();
    this.titleBlockList.Clear();
    List<string> list = new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).ToElements().Select<Element, string>((Func<Element, string>) (p => p.Name)).ToList<string>();
    list.Sort();
    foreach (string str in list)
      this.titleBlockList.Add(new ComboBoxItemString(str));
    this.titleBlockList.ForEach((Action<ComboBoxItemString>) (p => this._masterTitleBlockList.Add(p)));
  }

  public virtual bool CheckActiveView(out bool bHWD)
  {
    bHWD = false;
    if (!(this.ActiveView is ViewSheet) || !this.ActiveView.IsAssemblyView)
      return false;
    if (Parameters.GetParameterAsBool(this.ActiveView.Document.GetElement(this.ActiveView.AssociatedAssemblyInstanceId), "HARDWARE_DETAIL"))
      bHWD = true;
    return true;
  }

  public bool withinFamilyDocument() => this.uiDoc.Document.IsFamilyDocument;

  public void CheckTemplateManufacturer()
  {
    if (string.IsNullOrWhiteSpace(this.TemplateString))
      return;
    int index = this.TemplateSettings.Templates.FindIndex((Predicate<TicketTemplate>) (s => s.TemplateName == this.TemplateString));
    if (index == -1 || !(this.TemplateSettings.Templates[index].TemplateManufacturerName != this.ManufacturerString))
      return;
    this.TemplateSettings.Templates[index].TemplateManufacturerName = this.ManufacturerString;
  }

  public void CheckTemplateTitleBlock()
  {
    if (string.IsNullOrWhiteSpace(this.TemplateString))
      return;
    int index = this.TemplateSettings.Templates.FindIndex((Predicate<TicketTemplate>) (s => s.TemplateName == this.TemplateString));
    if (index == -1 || !(this.TemplateSettings.Templates[index].TitleBlockName != this.TitleBlockString))
      return;
    this.TemplateSettings.Templates[index].TitleBlockName = this.TitleBlockString;
  }
}
