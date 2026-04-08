// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.ViewModels.HWTemplateCreatorViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.HardwareDetail.Views;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using EDGE.TicketTools.TemplateToolsBase.Views;
using EDGE.TicketTools.TicketPopulator;
using EDGE.TicketTools.TicketTemplateTools.Debugging;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.ViewModels;

public class HWTemplateCreatorViewModel : TemplateCreatorViewModel
{
  public HWTemplateCreatorViewModel()
  {
    this.templateType = "HW";
    this.hwMainWindow = new HWTemplateCreatorWindow();
  }

  public HWTemplateCreatorViewModel(HWTemplateCreatorWindow window, AssemblyInstance assembly)
  {
    this.hwMainWindow = window;
    this.templateType = "HW";
  }

  public void ExecuteCreate()
  {
    if (!this.CanExecuteCreate())
      return;
    try
    {
      bool bHWD;
      if (this.CheckActiveView(out bHWD) & bHWD)
      {
        string empty = string.Empty;
        string name = this.ActiveView.Name;
        string templateName = this.TemplateString.Trim();
        CreateWindow createWindow = new CreateWindow(templateName, empty, name, this);
        createWindow.Owner = (Window) this.hwMainWindow;
        this.hwMainWindow.IsEnabled = false;
        createWindow.ShowDialog();
        this.hwMainWindow.IsEnabled = true;
        this.templateList = this.AlhabetizeList(this.templateList);
        this.TemplateString = templateName;
      }
      else
      {
        new TaskDialog("Hardware Detail Template Creator")
        {
          MainInstruction = "The current view is invalid for template creation",
          MainContent = (bHWD ? "Please open a valid assembly sheet and try again." : "The Hardware Detail Template Creator can only be run on sheets for assemblies with the \"HARDWARE_DETAIL\" flag set. Please open a valid assembly sheet and try again."),
          CommonButtons = ((TaskDialogCommonButtons) 1)
        }.Show();
        this.hwMainWindow.Focus();
      }
    }
    catch (Exception ex)
    {
      this.hwMainWindow.IsEnabled = true;
    }
  }

  public void ExecuteCloseCommand(object parameter)
  {
    try
    {
      this.hwMainWindow.Close();
    }
    catch (Exception ex)
    {
    }
  }

  public override void ExecuteFileSelectionCommand()
  {
    try
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.AddExtension = true;
      saveFileDialog.CheckPathExists = true;
      saveFileDialog.DefaultExt = ".xml";
      if (File.Exists(this.HardwareDetailTemplateSettingsPath))
        saveFileDialog.InitialDirectory = this.HardwareDetailTemplateSettingsPath;
      saveFileDialog.Filter = "Settings Files (*.xml, *.XML)|*.xml;*.XML";
      saveFileDialog.Title = "Select settings file or specify new settings file name";
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
        return;
      this.HardwareDetailTemplateSettingsPath = saveFileDialog.FileName;
      if (this.HWTemplateSettings != null)
      {
        this.HWTemplateSettings.manufacturerList.Clear();
        this.HWTemplateSettings.Templates.Clear();
      }
      this.ReadTemplateSettings();
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("An Exceptions occured while attempting to select or read a new Template Settings File:\n" + ex.Message);
    }
  }

  public override void ExecuteRenameTemplateCommand()
  {
    try
    {
      NewManufacturer newManufacturer = new NewManufacturer((TemplateCreatorViewModel) this);
      newManufacturer.Owner = (Window) this.hwMainWindow;
      this.hwMainWindow.IsEnabled = false;
      newManufacturer.ShowDialog();
      this.hwMainWindow.IsEnabled = true;
      this.templateList = this.AlhabetizeList(this.templateList);
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("An Exception was thrown attempting to rename a template:\n" + ex.Message);
      this.hwMainWindow.IsEnabled = true;
    }
  }

  public new void ExecuteSaveCommand()
  {
    try
    {
      if (this.HWTemplateSettings == null)
        this.HWTemplateSettings = new HardwareDetailTemplateSettings();
      if (!this.CheckHardwareTemplateSettings())
        return;
      this.CheckTemplateManufacturer();
      this.CheckTemplateTitleBlock();
      if (string.IsNullOrEmpty(this.HardwareDetailTemplateSettingsPath))
      {
        TaskDialog taskDialog1 = new TaskDialog("Edge for Revit - EDGE^R")
        {
          Title = "Save Template Error",
          MainContent = "Unable to save the Hardware Detail Template to the path provided.\nPlease check the path to the template file and try again."
        };
      }
      else if (this.HWTemplateSettings.SaveHardwareDetailTempalteSettings(this.HardwareDetailTemplateSettingsPath, this.manufacturerList))
      {
        int num = (int) System.Windows.MessageBox.Show("Saved Settings Successfully");
        TaskDialog taskDialog2 = new TaskDialog("EDGE^R");
        taskDialog2.MainInstruction = "Some schedules were not saved.";
        taskDialog2.MainContent = "One or more schedules could not be saved to the ticket template due to the naming convention. Template schedule names must begin with \"z_\" or \"ZZ_\" in order to be saved to the ticket template. Expand for details.";
        if (this.badNames.Count <= 0)
          return;
        foreach (string badName in this.badNames)
        {
          TaskDialog taskDialog3 = taskDialog2;
          taskDialog3.ExpandedContent = $"{taskDialog3.ExpandedContent}{badName}\n";
        }
        taskDialog2.Show();
        this.badNames.Clear();
      }
      else
      {
        TaskDialog taskDialog4 = new TaskDialog("Edge for Revit - EDGE^R")
        {
          Title = "Save Template Error",
          MainContent = "Unable to save the Hardware Detail Template to the path provided.\nPlease check the path to the template file and try again."
        };
      }
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("Exception Thrown saving tickets: " + ex.Message);
    }
  }

  public new void ExecuteEditTitleblockCommand()
  {
    if (!this.CanExecuteEditTitleblockCommand())
      return;
    try
    {
      this.UpdateTitleBlockList();
      if (this.TitleBlockString == null)
        this.UpdateTitleBlock(this.TemplateString);
      HWEditTitleblockWindow titleblockWindow = new HWEditTitleblockWindow(this, this.titleBlockList, this.TitleBlockString);
      titleblockWindow.Owner = (Window) this.hwMainWindow;
      this.hwMainWindow.IsEnabled = false;
      titleblockWindow.ShowDialog();
      this.hwMainWindow.IsEnabled = true;
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.MessageBox.Show("Exception thrown editing title block: \n" + ex.Message);
      this.hwMainWindow.IsEnabled = true;
    }
    this.hwMainWindow.IsEnabled = true;
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
        this.UpdateManufacturer(value);
        this.UpdateTitleBlock(value);
      }
      bool flag = false;
      foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
      {
        if (template.ValueString == value)
          flag = true;
      }
      if (flag && this.hwMainWindow.templateNameComboBox.SelectedIndex != -1)
        this.hwMainWindow.templateNameComboBox.SelectedIndex = -1;
      else if (flag && this.ticketMainWindow.templateNameComboBox.SelectedIndex != -1)
        this.ticketMainWindow.templateNameComboBox.SelectedIndex = -1;
      this.RefreshWindow();
    }
  }

  public override void UpdateTitleBlock(string templateName)
  {
    this.TitleBlockString = (string) null;
    this.titleBlockList.Clear();
    this._masterTitleBlockList.ForEach((Action<ComboBoxItemString>) (p => this.titleBlockList.Add(p)));
    if (this.HWTemplateSettings != null)
    {
      foreach (HWDetailTemplate template in this.HWTemplateSettings.Templates)
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

  public override void UpdateTitleBlockList()
  {
    this._masterTitleBlockList.Clear();
    this.titleBlockList.Clear();
    List<string> list = new FilteredElementCollector(this.revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).ToElements().Select<Element, string>((Func<Element, string>) (p => p.Name)).ToList<string>();
    list.Sort();
    foreach (string str in list)
      this.titleBlockList.Add(new ComboBoxItemString(str));
    this.titleBlockList.ForEach((Action<ComboBoxItemString>) (p => this._masterTitleBlockList.Add(p)));
  }

  private void UpdateManufacturer(string templateName)
  {
    if (this.HWTemplateSettings == null)
      return;
    foreach (HWDetailTemplate template in this.HWTemplateSettings.Templates)
    {
      if (template.TemplateName == templateName.Trim())
      {
        this.ManufacturerString = template.TemplateManufacturerName;
        if (this.templateType == "HW")
        {
          this.hwMainWindow.manufacturerComboBox.SelectedValue = (object) this.ManufacturerString;
          break;
        }
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
      managementWindow.Owner = (Window) this.hwMainWindow;
      this.hwMainWindow.IsEnabled = false;
      managementWindow.ShowDialog();
      this.hwMainWindow.IsEnabled = true;
      this.hwMainWindow.manufacturerComboBox.Items.Refresh();
    }
    catch (Exception ex)
    {
      this.hwMainWindow.IsEnabled = true;
    }
    this.hwMainWindow.IsEnabled = true;
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
        this.hwMainWindow.templateNameComboBox.Text = (string) null;
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

  public new bool DeleteTemplate(string templateToDelete)
  {
    bool flag = false;
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
    return true;
  }

  public override void LoadForm()
  {
    if (App.HardwareDetailTemplateSettingsPath != "")
    {
      this.HardwareDetailTemplateSettingsPath = App.HardwareDetailTemplateSettingsPath;
      this.ReadTemplateSettings();
    }
    this.UpdateTitleBlockList();
  }

  public override void RenameTemplate(string additionString)
  {
    foreach (HWDetailTemplate template in this.HWTemplateSettings.Templates)
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
    this.hwMainWindow.templateNameComboBox.Items.Refresh();
  }

  public override void UpdateSelectedTemplateListItem(string templateName)
  {
    this.hwMainWindow.templateNameComboBox.SelectedItem = (object) null;
    this.hwMainWindow.templateNameComboBox.SelectedValue = (object) null;
    foreach (ComboBoxItemString template in (Collection<ComboBoxItemString>) this.templateList)
    {
      if (template.ValueString.Equals(templateName))
      {
        this.hwMainWindow.templateNameComboBox.SelectedItem = (object) template;
        this.hwMainWindow.templateNameComboBox.SelectedValue = (object) template.ValueString;
        this.hwMainWindow.templateNameComboBox.Items.Refresh();
      }
    }
  }

  public override bool CreateTemplateFromViewSheet(
    BOMJustification BOMjustification,
    bool bStackSchedules)
  {
    Transform AssemblyTransform = (Transform) null;
    if (this.assembly != null)
      AssemblyTransform = this.assembly.GetTransform();
    if (this.HWTemplateSettings == null)
      this.HWTemplateSettings = new HardwareDetailTemplateSettings();
    string str1 = this.TemplateString.Trim();
    this.BOMJustification = BOMjustification;
    ViewSheet templateSheet = this.ActiveView as ViewSheet;
    string manufacturerString = this.ManufacturerString;
    HWDetailTemplate ticketTemplate = new HWDetailTemplate();
    this.anchorGroups = new List<AnchorGroup>();
    string empty = string.Empty;
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
    ticketTemplate.TemplateName = str1;
    ticketTemplate.TemplateManufacturerName = manufacturerString;
    ticketTemplate.TemplateScale = this.overallTemplateScale;
    ticketTemplate.ScaleUnits = ScalesManager.GetScaleUnitsForDocument(this.revitDoc);
    ticketTemplate.TitleBlockName = titleBlockName;
    ticketTemplate.TitleLocationToUpperLeftCorner = new SimpleVector(EDGERCreateTemplate.GetOutlineUpperLeftCorner(outline1) - titleBlockLocation);
    ticketTemplate.BOMJustification = this.BOMJustification;
    ticketTemplate.bStackSchedules = bStackSchedules;
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
    bool flag = false;
    List<string> stringList = new List<string>();
    if (source7.Any<ScheduleSheetInstance>())
    {
      if (!ticketTemplate.bStackSchedules)
      {
        foreach (ScheduleSheetInstance scheduleSheetInstance in source7)
        {
          HardwareDetailScheduleInfo detailScheduleInfo = new HardwareDetailScheduleInfo();
          detailScheduleInfo.BOMJustification = this.BOMJustification;
          detailScheduleInfo.BOMScheduleName = scheduleSheetInstance.Name;
          if (this.revitDoc.GetElement(scheduleSheetInstance.ScheduleId) is ViewSchedule element4)
          {
            if (element4.AssociatedAssemblyInstanceId == (ElementId) null || element4.AssociatedAssemblyInstanceId == ElementId.InvalidElementId)
            {
              detailScheduleInfo.isNoteSchedule = true;
              flag = true;
            }
            else
            {
              ElementId parameterAsElementId = Parameters.GetParameterAsElementId((Element) element4, "View Template");
              if (parameterAsElementId != (ElementId) null && parameterAsElementId != ElementId.InvalidElementId)
              {
                if (this.revitDoc.GetElement(parameterAsElementId) is Autodesk.Revit.DB.View element && element.Name.StartsWith("_HWD-"))
                {
                  detailScheduleInfo.ViewTemplateName = element.Name;
                }
                else
                {
                  if (!stringList.Contains(element4.Name))
                  {
                    stringList.Add(element4.Name);
                    continue;
                  }
                  continue;
                }
              }
              else
              {
                if (!stringList.Contains(element4.Name))
                {
                  stringList.Add(element4.Name);
                  continue;
                }
                continue;
              }
            }
          }
          detailScheduleInfo.iScheduleOrderIndex = -1;
          XYZ point;
          if (detailScheduleInfo.BOMJustification == BOMJustification.Bottom)
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
          detailScheduleInfo.vectorToAnchorPoint = new SimpleVector(point - titleBlockLocation);
          ticketTemplate.ScheduleList.Add(detailScheduleInfo);
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
            HardwareDetailScheduleInfo detailScheduleInfo = new HardwareDetailScheduleInfo();
            detailScheduleInfo.BOMScheduleName = scheduleSheetInstance.Name;
            if (this.revitDoc.GetElement(scheduleSheetInstance.ScheduleId) is ViewSchedule element5)
            {
              if (element5.AssociatedAssemblyInstanceId == (ElementId) null || element5.AssociatedAssemblyInstanceId == ElementId.InvalidElementId)
              {
                detailScheduleInfo.isNoteSchedule = true;
                flag = true;
              }
              else
              {
                ElementId parameterAsElementId = Parameters.GetParameterAsElementId((Element) element5, "View Template");
                if (parameterAsElementId != (ElementId) null && parameterAsElementId != ElementId.InvalidElementId)
                {
                  if (this.revitDoc.GetElement(parameterAsElementId) is Autodesk.Revit.DB.View element && element.Name.StartsWith("_HWD-"))
                  {
                    detailScheduleInfo.ViewTemplateName = element.Name;
                  }
                  else
                  {
                    if (!stringList.Contains(element5.Name))
                    {
                      stringList.Add(element5.Name);
                      continue;
                    }
                    continue;
                  }
                }
                else
                {
                  if (!stringList.Contains(element5.Name))
                  {
                    stringList.Add(element5.Name);
                    continue;
                  }
                  continue;
                }
              }
            }
            detailScheduleInfo.iScheduleOrderIndex = num2;
            ticketTemplate.ScheduleList.Add(detailScheduleInfo);
            ++num2;
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
    if (flag)
      new TaskDialog("Warning")
      {
        MainContent = "The Hardware Detail template you are creating contains project-level schedules. The Hardware Detail tools only support note schedules at the project level.These will only populate correctly if they are defined in the Hardware Detail BOM Settings and an exact name match exists in the active project."
      }.Show();
    if (stringList.Count > 0)
    {
      TaskDialog taskDialog1 = new TaskDialog("Warning");
      taskDialog1.MainContent = "The Hardware Detail template you are creating contains one or more invalid assembly schedules. Please ensure that all assembly schedules to be populated have a view template assigned whose name begins with \"_HWD-\". The template was saved without these invalid schedules. Expand for details.";
      stringList.Sort();
      taskDialog1.ExpandedContent = "Invalid Schedules:\n";
      foreach (string str2 in stringList)
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str2}\n";
      }
      taskDialog1.Show();
    }
    if (!this.CheckHardwareTemplateSettings())
      return false;
    int index = this.HWTemplateSettings.Templates.FindIndex((Predicate<HWDetailTemplate>) (s => s.TemplateName == ticketTemplate.TemplateName));
    if (index == -1)
    {
      this.HWTemplateSettings.Templates.Add(ticketTemplate);
      this.templateList.Add(new ComboBoxItemString(ticketTemplate.TemplateName));
      this.templateList.OrderBy<ComboBoxItemString, string>((Func<ComboBoxItemString, string>) (s => s.ValueString));
    }
    else
      this.HWTemplateSettings.Templates[index] = ticketTemplate;
    this.TitleBlockString = ticketTemplate.TitleBlockName;
    return true;
  }

  public override void RefreshWindow()
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
    this.hwMainWindow.manufacturerComboBox.Items.Refresh();
    this.hwMainWindow.templateNameComboBox.Items.Refresh();
  }

  protected override bool ReadTemplateSettings()
  {
    bool flag1 = true;
    if (this.HWTemplateSettings == null)
      this.HWTemplateSettings = new HardwareDetailTemplateSettings();
    string templateString = this.TemplateString;
    string manufacturerString = this.ManufacturerString;
    FileInfo fileInfo = new FileInfo(this.HardwareDetailTemplateSettingsPath);
    if (fileInfo.Exists)
    {
      if (!this.HWTemplateSettings.LoadHardwareDetailTemplateSettings(fileInfo.FullName) || fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
        return false;
      if (this.templateList == null)
        this.templateList = new ObservableCollection<ComboBoxItemString>();
      if (this.manufacturerList == null)
        this.manufacturerList = new ObservableCollection<ComboBoxItemString>();
      this.templateList.Clear();
      this.manufacturerList.Clear();
      this.manufacturerList.Add(new ComboBoxItemString((string) null));
      foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) this.HWTemplateSettings.manufacturerList)
        this.manufacturerList.Add(manufacturer);
      HashSet<string> source1 = new HashSet<string>();
      List<string> source2 = new List<string>();
      foreach (HWDetailTemplate template in this.HWTemplateSettings.Templates)
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
      this.HWTemplateSettings.Clear();
    }
    return flag1;
  }

  protected new void CheckTemplateManufacturer()
  {
    if (string.IsNullOrWhiteSpace(this.TemplateString))
      return;
    int index = this.HWTemplateSettings.Templates.FindIndex((Predicate<HWDetailTemplate>) (s => s.TemplateName == this.TemplateString));
    if (index == -1 || !(this.HWTemplateSettings.Templates[index].TemplateManufacturerName != this.ManufacturerString))
      return;
    this.HWTemplateSettings.Templates[index].TemplateManufacturerName = this.ManufacturerString;
  }

  protected new void CheckTemplateTitleBlock()
  {
    if (string.IsNullOrWhiteSpace(this.TemplateString))
      return;
    int index = this.HWTemplateSettings.Templates.FindIndex((Predicate<HWDetailTemplate>) (s => s.TemplateName == this.TemplateString));
    if (index == -1 || !(this.HWTemplateSettings.Templates[index].TitleBlockName != this.TitleBlockString))
      return;
    this.HWTemplateSettings.Templates[index].TitleBlockName = this.TitleBlockString;
  }

  private bool CheckHardwareTemplateSettings()
  {
    if (this.HWTemplateSettings != null)
      return true;
    new TaskDialog("Edge for Revit - EDGE^R")
    {
      Title = "Template Settings Error",
      MainContent = "Unable to determine the Hardware Detail Template Settings.\nPlease check the path to the template file; check the path in User Settings\\EDGE Preferences\\Hardware Template Settings and try again."
    }.Show();
    return false;
  }
}
