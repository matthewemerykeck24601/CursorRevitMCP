// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Insulation_Drawing_Settings.InsulationDrawingSettingsWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.UserSettingTools.Insulation_Drawing_Settings;

public class InsulationDrawingSettingsWindow : Window, IComponentConnector
{
  private string Manufacturer = "";
  private Dictionary<string, int> insulationScales = new Dictionary<string, int>();
  internal ComboBox InsulationLineStyleComboBox;
  internal ComboBox MarkCircleLineStyleComboBox;
  internal ComboBox RecessCalloutsTextStyleComboBox;
  internal ComboBox InsulationMarkTextStyleComboBox;
  internal ComboBox OverallDimensionStyleComboBox;
  internal ComboBox GeneralDimensionStyleComboBox;
  internal ComboBox TitleBlockComboBox;
  internal ComboBox MasterScalesComboBox;
  internal ComboBox PerPieceScalesComboBox;
  internal Button ApplySettingsButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public InsulationDrawingSettingsWindow(Document revitDoc, IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    string str = "";
    Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && parameter.StorageType == StorageType.String)
      str = parameter.AsString();
    if (string.IsNullOrWhiteSpace(str))
    {
      TaskDialog.Show("Insulaion Drawing Settings", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to open Insulation Drawings Settings again.");
      this.Close();
    }
    if (string.IsNullOrWhiteSpace(str))
      this.Close();
    else
      this.Manufacturer = str;
    InsulationDrawingSettings insulationDrawingSettings = new InsulationDrawingSettings(this.Manufacturer);
    if (!insulationDrawingSettings.SettingsRead())
      TaskDialog.Show("Insulation Drawing Settings", "Unable to read settings file. Settings tool will create one when saving.");
    Category category = Category.GetCategory(revitDoc, BuiltInCategory.OST_Lines);
    List<string> source1 = new List<string>();
    foreach (Category subCategory in category.SubCategories)
      source1.Add(subCategory.Name);
    source1.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.InsulationLineStyleComboBox.ItemsSource = (IEnumerable) source1;
    this.MarkCircleLineStyleComboBox.ItemsSource = (IEnumerable) source1;
    List<string> source2 = new List<string>();
    foreach (TextNoteType textNoteType in new FilteredElementCollector(revitDoc).OfClass(typeof (TextNoteType)))
      source2.Add(textNoteType.Name);
    source2.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.RecessCalloutsTextStyleComboBox.ItemsSource = (IEnumerable) source2;
    this.InsulationMarkTextStyleComboBox.ItemsSource = (IEnumerable) source2;
    List<string> source3 = new List<string>();
    foreach (DimensionType dimensionType in new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType)))
    {
      if (!(dimensionType.FamilyName == dimensionType.Name) && dimensionType.StyleType == DimensionStyleType.Linear)
        source3.Add(dimensionType.Name);
    }
    source3.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.OverallDimensionStyleComboBox.ItemsSource = (IEnumerable) source3;
    this.GeneralDimensionStyleComboBox.ItemsSource = (IEnumerable) source3;
    List<string> list = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).ToElements().Select<Element, string>((Func<Element, string>) (p => p.Name)).ToList<string>();
    list.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.TitleBlockComboBox.ItemsSource = (IEnumerable) list;
    ScaleUnits unitType = ScalesManager.GetScaleUnitsForDocument(revitDoc);
    if (revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId() == UnitTypeId.MetersCentimeters)
      unitType = ScaleUnits.Metric;
    this.insulationScales = ScalesManager.GetScalesDictionary(unitType);
    List<string> collection = new List<string>();
    for (int index = 0; index < this.insulationScales.Count; ++index)
      collection.Add(this.insulationScales.ElementAt<KeyValuePair<string, int>>(index).Key);
    this.MasterScalesComboBox.ItemsSource = (IEnumerable) collection;
    this.PerPieceScalesComboBox.ItemsSource = (IEnumerable) collection;
    int num1 = 32 /*0x20*/;
    int num2 = 24;
    if (!insulationDrawingSettings.SettingsRead())
    {
      if (source1.Contains("04-NCS Medium"))
        this.InsulationLineStyleComboBox.SelectedValue = (object) "04-NCS Medium";
      else
        this.InsulationLineStyleComboBox.SelectedValue = (object) source1.FirstOrDefault<string>();
      if (source1.Contains("01-NCS Extra Fine"))
        this.MarkCircleLineStyleComboBox.SelectedValue = (object) "01-NCS Extra Fine";
      else
        this.MarkCircleLineStyleComboBox.SelectedValue = (object) source1.FirstOrDefault<string>();
      if (source2.Contains("PTAC - TICKET TEXT"))
      {
        this.RecessCalloutsTextStyleComboBox.SelectedValue = (object) "PTAC - TICKET TEXT";
        this.InsulationMarkTextStyleComboBox.SelectedValue = (object) "PTAC - TICKET TEXT";
      }
      else
      {
        this.RecessCalloutsTextStyleComboBox.SelectedValue = (object) source2.FirstOrDefault<string>();
        this.InsulationMarkTextStyleComboBox.SelectedValue = (object) source2.FirstOrDefault<string>();
      }
      if (source3.Contains("PTAC - TICKET (GAP TO ELEMENT)"))
        this.OverallDimensionStyleComboBox.SelectedValue = (object) "PTAC - TICKET (GAP TO ELEMENT)";
      else
        this.OverallDimensionStyleComboBox.SelectedValue = (object) source3.FirstOrDefault<string>();
      if (source3.Contains("PTAC - TICKET (FIXED TO DIM. LINE)"))
        this.GeneralDimensionStyleComboBox.SelectedValue = (object) "PTAC - TICKET (FIXED TO DIM. LINE)";
      else
        this.GeneralDimensionStyleComboBox.SelectedValue = (object) source3.FirstOrDefault<string>();
      if (list.Contains("PS-30X42-CD"))
        this.TitleBlockComboBox.SelectedValue = (object) "PS-30X42-CD";
      else
        this.TitleBlockComboBox.SelectedValue = (object) list.FirstOrDefault<string>();
    }
    else
    {
      this.InsulationLineStyleComboBox.SelectedValue = (object) insulationDrawingSettings.InsulationDetailLineStyleName;
      this.MarkCircleLineStyleComboBox.SelectedValue = (object) insulationDrawingSettings.MarkCircleDetailLineStyleName;
      this.RecessCalloutsTextStyleComboBox.SelectedValue = (object) insulationDrawingSettings.RecessCalloutsTextStyleName;
      this.InsulationMarkTextStyleComboBox.SelectedValue = (object) insulationDrawingSettings.InsulationMarkTextStyleName;
      this.OverallDimensionStyleComboBox.SelectedValue = (object) insulationDrawingSettings.OverallDimensionStyleName;
      this.GeneralDimensionStyleComboBox.SelectedValue = (object) insulationDrawingSettings.GeneralDimensionStyleName;
      this.TitleBlockComboBox.SelectedValue = (object) insulationDrawingSettings.TitleBlockName;
      num1 = insulationDrawingSettings.InsulationDrawingScaleFactorMaster;
      num2 = insulationDrawingSettings.InsulationDrawingScaleFactorPerPiece;
    }
    bool flag1 = false;
    if (this.insulationScales.ContainsValue(num1))
    {
      foreach (string key in this.insulationScales.Keys)
      {
        if (this.insulationScales[key] == num1)
        {
          this.MasterScalesComboBox.SelectedValue = (object) key;
          flag1 = true;
        }
      }
      if (!flag1)
      {
        List<string> stringList = new List<string>();
        stringList.AddRange((IEnumerable<string>) collection);
        stringList.Add("1:" + num1.ToString());
        this.MasterScalesComboBox.ItemsSource = (IEnumerable) stringList;
        this.MasterScalesComboBox.SelectedValue = (object) ("1:" + num1.ToString());
      }
    }
    else
    {
      List<string> stringList = new List<string>();
      stringList.AddRange((IEnumerable<string>) collection);
      stringList.Add("1:" + num1.ToString());
      this.MasterScalesComboBox.ItemsSource = (IEnumerable) stringList;
      this.MasterScalesComboBox.SelectedValue = (object) ("1:" + num1.ToString());
    }
    bool flag2 = false;
    if (this.insulationScales.ContainsValue(num2))
    {
      foreach (string key in this.insulationScales.Keys)
      {
        if (this.insulationScales[key] == num2)
        {
          this.PerPieceScalesComboBox.SelectedValue = (object) key;
          flag2 = true;
        }
      }
      if (flag2)
        return;
      List<string> stringList = new List<string>();
      stringList.AddRange((IEnumerable<string>) collection);
      stringList.Add("1:" + num2.ToString());
      this.PerPieceScalesComboBox.ItemsSource = (IEnumerable) stringList;
      this.PerPieceScalesComboBox.SelectedValue = (object) ("1:" + num2.ToString());
    }
    else
    {
      List<string> stringList = new List<string>();
      stringList.AddRange((IEnumerable<string>) collection);
      stringList.Add("1:" + num2.ToString());
      this.PerPieceScalesComboBox.ItemsSource = (IEnumerable) stringList;
      this.PerPieceScalesComboBox.SelectedValue = (object) ("1:" + num2.ToString());
    }
  }

  private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
  {
    if (!new InsulationDrawingSettings()
    {
      InsulationDetailLineStyleName = this.InsulationLineStyleComboBox.Text.Trim(),
      MarkCircleDetailLineStyleName = this.MarkCircleLineStyleComboBox.Text.Trim(),
      RecessCalloutsTextStyleName = this.RecessCalloutsTextStyleComboBox.Text.Trim(),
      InsulationMarkTextStyleName = this.InsulationMarkTextStyleComboBox.Text.Trim(),
      OverallDimensionStyleName = this.OverallDimensionStyleComboBox.Text.Trim(),
      GeneralDimensionStyleName = this.GeneralDimensionStyleComboBox.Text.Trim(),
      TitleBlockName = this.TitleBlockComboBox.Text.Trim(),
      InsulationDrawingScaleFactorMaster = this.ConvertScaleStringToFactor(this.MasterScalesComboBox.Text.Trim()),
      InsulationDrawingScaleFactorPerPiece = this.ConvertScaleStringToFactor(this.PerPieceScalesComboBox.Text.Trim())
    }.SaveFile(this.Manufacturer))
      return;
    this.Close();
  }

  private int ConvertScaleStringToFactor(string scaleString)
  {
    int result = 0;
    if (this.insulationScales.ContainsKey(scaleString))
      this.insulationScales.TryGetValue(scaleString, out result);
    if (result == 0)
    {
      scaleString = scaleString.Replace("1:", "");
      if (!int.TryParse(scaleString, out result))
        result = 32 /*0x20*/;
    }
    return result;
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/insulation%20drawing%20settings/insulationdrawingsettingswindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.InsulationLineStyleComboBox = (ComboBox) target;
        break;
      case 2:
        this.MarkCircleLineStyleComboBox = (ComboBox) target;
        break;
      case 3:
        this.RecessCalloutsTextStyleComboBox = (ComboBox) target;
        break;
      case 4:
        this.InsulationMarkTextStyleComboBox = (ComboBox) target;
        break;
      case 5:
        this.OverallDimensionStyleComboBox = (ComboBox) target;
        break;
      case 6:
        this.GeneralDimensionStyleComboBox = (ComboBox) target;
        break;
      case 7:
        this.TitleBlockComboBox = (ComboBox) target;
        break;
      case 8:
        this.MasterScalesComboBox = (ComboBox) target;
        break;
      case 9:
        this.PerPieceScalesComboBox = (ComboBox) target;
        break;
      case 10:
        this.ApplySettingsButton = (Button) target;
        this.ApplySettingsButton.Click += new RoutedEventHandler(this.ApplySettingsButton_Click);
        break;
      case 11:
        this.CancelButton = (Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
