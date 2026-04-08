// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.AutoTicketSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class AutoTicketSettings : Window, IComponentConnector
{
  private List<string> illegalChar;
  private string path = App.AutoTicketFolderPath;
  private Document revitDoc;
  private List<AutoTicketSettingsTools> ticketGenerationSettings = new List<AutoTicketSettingsTools>();
  private List<AutoTicketSettingsTools> autoDimensionSettings = new List<AutoTicketSettingsTools>();
  private List<AutoTicketSettingsTools> eDrawingSettings = new List<AutoTicketSettingsTools>();
  private List<AutoTicketSettingsTools> cloneTicketSettings = new List<AutoTicketSettingsTools>();
  private List<AutoTicketSettingsTools> hardwareDetailSettings = new List<AutoTicketSettingsTools>();
  private List<AutoTicketAppendStringParameterData> appendStringParameterTicketGeneration = new List<AutoTicketAppendStringParameterData>();
  private List<AutoTicketAppendStringParameterData> appendStringParameterAutoDimension = new List<AutoTicketAppendStringParameterData>();
  private List<AutoTicketAppendStringParameterData> appendStringParameterEDrawing = new List<AutoTicketAppendStringParameterData>();
  private List<AutoTicketAppendStringParameterData> appendStringParameterCloneTicket = new List<AutoTicketAppendStringParameterData>();
  private List<AutoTicketAppendStringParameterData> appendStringParameterHardwareDetail = new List<AutoTicketAppendStringParameterData>();
  public double TicketGenerationFeetValue;
  public double TicketGenerationInchValue;
  public double AutoDimensionFeetValue;
  public double AutoDimensionInchValue;
  public double CloneTicketFeetValue;
  public double CloneTicketInchValue;
  public double HardwareDetailFeetValue;
  public double HardwareDetailInchValue;
  public double TicketGenerationMetricValue;
  public double AutoDimensionMetricValue;
  public double CloneTicketMetricValue;
  public double HardwareDetailMetricValue;
  public bool bTicketGenerationTabActive;
  public bool bAutoDimensionTabActive;
  public bool bEDrawingTabActive;
  public bool bCloneTicket;
  public bool bHardwareDetail;
  public bool imperialUnits = true;
  private bool doNotUpdatePrefixSuffix;
  private bool changeIsRunningHardwareDetail;
  private bool changeIsRunningAutoDimensionEDrawing;
  private bool changeIsRunningAutoDimension;
  private bool changeIsRunningTicketGeneration;
  internal GridSplitter GridSpliter;
  internal Border AutoTicketTab;
  internal ListBox SettingTicketGeneration;
  internal System.Windows.Controls.Grid StyleTabTicketGeneration;
  internal ComboBox CalloutStyleComboBoxTicketGeneration;
  internal ComboBox DimensionStyleComboBoxTicketGeneration;
  internal ComboBox OtherDimensionStyleComboBoxTicketGeneration;
  internal CheckBox CheckBoxEqualityFormula;
  internal ComboBox TextStyleComboBoxTicketGeneration;
  internal System.Windows.Controls.Grid AppendStringTabTicketGeneration;
  internal RichTextBox AppendStringTextBoxTicketGeneration;
  internal TextBox ExampleResultTextBoxTicketGeneration;
  internal System.Windows.Controls.Grid ParameterDefinitionBlockTicketGeneration;
  internal ComboBox ParameterValuesListTicketGeneration;
  internal Button AddButtonTicketGeneration;
  internal TextBox PrefixTextBoxTicketGeneration;
  internal TextBox SuffixTextBoxTicketGeneration;
  internal TextBox ResultTextBoxTicketGeneration;
  internal TextBlock ApplicableTextTicketGeneration;
  internal System.Windows.Controls.Grid CustomValuesTabTicketGeneration;
  internal TextBox overallTextBoxTicketGeneration;
  internal TextBox contourTextBoxTicketGeneration;
  internal TextBox blockoutTextBoxTicketGeneration;
  internal System.Windows.Controls.Grid MinDimTabTicketGeneration;
  internal TextBox MinDimFeetTextBoxTicketGeneration;
  internal TextBlock MinDimFeetLabelTicketGeneration;
  internal TextBox MinDimInchTextBoxTicketGeneration;
  internal TextBlock MinDimInchLabelTicketGeneration;
  internal Button ChangeUnitsButtonTicketGeneration;
  internal Border AutoDimensionTab;
  internal ListBox SettingAutoDimension;
  internal System.Windows.Controls.Grid StyleTabAutoDimension;
  internal ComboBox DimensionStyleComboBoxAutoDimension;
  internal ComboBox TextStyleComboBoxAutoDimension;
  internal System.Windows.Controls.Grid AppendStringTabAutoDimension;
  internal RichTextBox AppendStringTextBoxAutoDimension;
  internal TextBox ExampleResultTextBoxAutoDimension;
  internal System.Windows.Controls.Grid ParameterDefinitionBockAutoDimension;
  internal ComboBox ParameterValuesListAutoDimension;
  internal Button AddButtonAutoDimension;
  internal TextBox PrefixTextBoxAutoDimension;
  internal TextBox SuffixTextBoxAutoDimension;
  internal TextBox ResultTextBoxAutoDimension;
  internal TextBlock ApplicableTextAutoDimension;
  internal System.Windows.Controls.Grid MinDimTabAutoDimension;
  internal TextBox MinDimFeetTextBoxAutoDimension;
  internal TextBlock MinDimFeetLabelAutoDimension;
  internal TextBox MinDimInchTextBoxAutoDimension;
  internal TextBlock MinDimInchLabelAutoDimension;
  internal Button ChangeUnitsButtonAutoDimension;
  internal Border AutoDimensionEDrawingTab;
  internal ListBox SettingAutoDimensionEDrawing;
  internal System.Windows.Controls.Grid StyleTabAutoDimensionEDrawing;
  internal ComboBox DimensionStyleComboBoxAutoDimensionEDrawing;
  internal ComboBox TextStyleComboBoxAutoDimensionEDrawing;
  internal System.Windows.Controls.Grid AppendStringTabAutoDimensionEDrawing;
  internal RichTextBox AppendStringTextBoxAutoDimensionEDrawing;
  internal TextBox ExampleResultTextBoxAutoDimensionEDrawing;
  internal System.Windows.Controls.Grid ParameterDefinitionBockAutoDimensionEDrawing;
  internal ComboBox ParameterValuesListAutoDimensionEDrawing;
  internal Button AddButtonAutoDimensionEDrawing;
  internal TextBox PrefixTextBoxAutoDimensionEDrawing;
  internal TextBox SuffixTextBoxAutoDimensionEDrawing;
  internal TextBox ResultTextBoxAutoDimensionEDrawing;
  internal TextBlock ApplicableTextAutoDimensionEDrawing;
  internal Border CloneTicketTab;
  internal ListBox SettingCloneTicket;
  internal System.Windows.Controls.Grid SamePointToleranceTabCloneTicket;
  internal TextBox SamePointToleranceFeetTextBox;
  internal TextBlock SamePointToleranceFeetLabel;
  internal TextBox SamePointToleranceInchTextBox;
  internal TextBlock SamePointToleranceInchLabel;
  internal Border HardwareDetailTab;
  internal ListBox SettingHardwareDetail;
  internal System.Windows.Controls.Grid StyleTabHardwareDetail;
  internal ComboBox CalloutStyleComboBoxHardwareDetail;
  internal ComboBox DimensionStyleComboBoxHardwareDetail;
  internal ComboBox OtherDimensionStyleComboBoxHardwareDetail;
  internal ComboBox TextStyleComboBoxHardwareDetail;
  internal CheckBox CheckBoxEqualityFormulaHardwareDetail;
  internal System.Windows.Controls.Grid MinDimTabHardwareDetail;
  internal TextBox MinDimFeetTextBoxHardwareDetail;
  internal TextBlock MinDimFeetLabelHardwareDetail;
  internal TextBox MinDimInchTextBoxHardwareDetail;
  internal TextBlock MinDimInchLabelHardwareDetail;
  internal System.Windows.Controls.Grid AppendStringTabHardwareDetail;
  internal RichTextBox AppendStringTextBoxHardwareDetail;
  internal TextBox ExampleResultTextBoxHardwareDetail;
  internal System.Windows.Controls.Grid ParameterDefinitionBlockHardwareDetail;
  internal ComboBox ParameterValuesListHardwareDetail;
  internal Button AddButtonHardwareDetail;
  internal TextBox PrefixTextBoxHardwareDetail;
  internal TextBox SuffixTextBoxHardwareDetail;
  internal TextBox ResultTextBoxHardwareDetail;
  internal TextBlock ApplicableTextHardwareDetail;
  internal Button okayButton;
  private bool _contentLoaded;

  public AutoTicketSettings(Document document, out bool bCancelled)
  {
    this.InitializeComponent();
    this.revitDoc = document;
    this.illegalChar = new List<string>()
    {
      "<",
      ">",
      ";",
      "[",
      "]",
      "{",
      "}",
      "\\",
      "|",
      "`",
      "~",
      "?",
      ":"
    };
    bCancelled = false;
    if (this.revitDoc.ProjectInformation == null)
    {
      int num = (int) MessageBox.Show("Auto Ticket Setting Dialog cannot be used in the family view, transaction will be cancelled.");
      bCancelled = true;
    }
    if (Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(this.revitDoc))
      this.imperialUnits = false;
    try
    {
      List<AutoTicketAppendStringParameterData> stringParameterDataList = new List<AutoTicketAppendStringParameterData>();
      this.ticketGenerationSettings = AutoTicketSettingsReader.ReaderforAutoTicketSettings(this.revitDoc, "AUTO-TICKET GENERATION", true, out this.appendStringParameterTicketGeneration);
      this.autoDimensionSettings = AutoTicketSettingsReader.ReaderforAutoTicketSettings(this.revitDoc, "AUTO-DIMENSION", true, out this.appendStringParameterAutoDimension);
      this.eDrawingSettings = AutoTicketSettingsReader.ReaderforAutoTicketSettings(this.revitDoc, "AUTO-DIMENSION E-DRAWING", true, out this.appendStringParameterEDrawing);
      this.cloneTicketSettings = AutoTicketSettingsReader.ReaderforAutoTicketSettings(this.revitDoc, "CLONE TICKET", true, out this.appendStringParameterCloneTicket);
      this.hardwareDetailSettings = AutoTicketSettingsReader.ReaderforAutoTicketSettings(this.revitDoc, "HARDWARE DETAIL", true, out this.appendStringParameterHardwareDetail);
      this.appendStringParameterTicketGeneration = this.appendStringParameterTicketGeneration.Where<AutoTicketAppendStringParameterData>((Func<AutoTicketAppendStringParameterData, bool>) (e => e.tool.Equals("AUTO-TICKET GENERATION"))).ToList<AutoTicketAppendStringParameterData>();
      this.appendStringParameterAutoDimension = this.appendStringParameterAutoDimension.Where<AutoTicketAppendStringParameterData>((Func<AutoTicketAppendStringParameterData, bool>) (e => e.tool.Equals("AUTO-DIMENSION"))).ToList<AutoTicketAppendStringParameterData>();
      this.appendStringParameterEDrawing = this.appendStringParameterEDrawing.Where<AutoTicketAppendStringParameterData>((Func<AutoTicketAppendStringParameterData, bool>) (e => e.tool.Equals("AUTO-DIMENSION E-DRAWING"))).ToList<AutoTicketAppendStringParameterData>();
      this.appendStringParameterHardwareDetail = this.appendStringParameterHardwareDetail.Where<AutoTicketAppendStringParameterData>((Func<AutoTicketAppendStringParameterData, bool>) (e => e.tool.Equals("HARDWARE DETAIL"))).ToList<AutoTicketAppendStringParameterData>();
      this.AddButtonTicketGeneration.Content = (object) "+";
      this.AddButtonTicketGeneration.Foreground = (Brush) Brushes.Green;
      this.AddButtonAutoDimension.Content = (object) "+";
      this.AddButtonAutoDimension.Foreground = (Brush) Brushes.Green;
      this.AddButtonAutoDimensionEDrawing.Content = (object) "+";
      this.AddButtonAutoDimensionEDrawing.Foreground = (Brush) Brushes.Green;
      this.DropdownFamiliesinProject(this.revitDoc);
      AutoTicketSettings.populateParameterDropDown(this.appendStringParameterTicketGeneration, this.ParameterValuesListTicketGeneration, "AUTO-TICKET GENERATION");
      AutoTicketSettings.populateParameterDropDown(this.appendStringParameterAutoDimension, this.ParameterValuesListAutoDimension, "AUTO-DIMENSION");
      AutoTicketSettings.populateParameterDropDown(this.appendStringParameterEDrawing, this.ParameterValuesListAutoDimensionEDrawing, "AUTO-DIMENSION E-DRAWING");
      AutoTicketSettings.populateParameterDropDown(this.appendStringParameterHardwareDetail, this.ParameterValuesListHardwareDetail, "HARDWARE DETAIL");
      if (this.ticketGenerationSettings != null)
      {
        foreach (AutoTicketSettingsTools generationSetting in this.ticketGenerationSettings)
        {
          switch (generationSetting)
          {
            case AutoTicketCalloutAndDimensionTexts _:
              AutoTicketCalloutAndDimensionTexts andDimensionTexts = generationSetting as AutoTicketCalloutAndDimensionTexts;
              this.CalloutStyleComboBoxTicketGeneration.Text = andDimensionTexts.CalloutFamily;
              this.DimensionStyleComboBoxTicketGeneration.Text = andDimensionTexts.OverallDimension;
              this.OtherDimensionStyleComboBoxTicketGeneration.Text = andDimensionTexts.GeneralDimension;
              this.TextStyleComboBoxTicketGeneration.Text = andDimensionTexts.TextStyle;
              this.CheckBoxEqualityFormula.IsChecked = new bool?(andDimensionTexts.UseEQ);
              continue;
            case AutoTicketAppendString _:
              new System.Windows.Documents.TextRange(this.AppendStringTextBoxTicketGeneration.Document.ContentStart, this.AppendStringTextBoxTicketGeneration.Document.ContentEnd).Text = (generationSetting as AutoTicketAppendString).AppendStringValue;
              continue;
            case AutoTicketCustomValues _:
              AutoTicketCustomValues ticketCustomValues = generationSetting as AutoTicketCustomValues;
              this.overallTextBoxTicketGeneration.Text = ticketCustomValues.Overall;
              this.contourTextBoxTicketGeneration.Text = ticketCustomValues.Contour;
              this.blockoutTextBoxTicketGeneration.Text = ticketCustomValues.Blockout;
              continue;
            case AutoTicketMinimumDimension _:
              AutoTicketMinimumDimension minimumDimension = generationSetting as AutoTicketMinimumDimension;
              if (!string.IsNullOrWhiteSpace(minimumDimension.FeetValue) && !string.IsNullOrWhiteSpace(minimumDimension.inchesValue))
              {
                double.TryParse(minimumDimension.FeetValue, out this.TicketGenerationFeetValue);
                double.TryParse(minimumDimension.inchesValue, out this.TicketGenerationInchValue);
                this.MinDimFeetTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(minimumDimension.FeetValue, 4);
                this.MinDimInchTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(minimumDimension.inchesValue, 4);
              }
              double.TryParse(minimumDimension.FeetValue.Trim(), out this.TicketGenerationFeetValue);
              double.TryParse(minimumDimension.inchesValue.Trim(), out this.TicketGenerationInchValue);
              continue;
            default:
              continue;
          }
        }
      }
      if (this.autoDimensionSettings != null)
      {
        foreach (AutoTicketSettingsTools dimensionSetting in this.autoDimensionSettings)
        {
          switch (dimensionSetting)
          {
            case AutoTicketCalloutAndDimensionTexts _:
              AutoTicketCalloutAndDimensionTexts andDimensionTexts = dimensionSetting as AutoTicketCalloutAndDimensionTexts;
              this.DimensionStyleComboBoxAutoDimension.Text = andDimensionTexts.OverallDimension;
              this.TextStyleComboBoxAutoDimension.Text = andDimensionTexts.TextStyle;
              continue;
            case AutoTicketAppendString _:
              AutoTicketAppendString ticketAppendString = dimensionSetting as AutoTicketAppendString;
              new System.Windows.Documents.TextRange(this.AppendStringTextBoxAutoDimension.Document.ContentStart, this.AppendStringTextBoxAutoDimension.Document.ContentEnd).Text = ticketAppendString.AppendStringValue;
              this.ExampleResultTextBoxAutoDimension.Text = ticketAppendString.SampleResult;
              continue;
            case AutoTicketMinimumDimension _:
              AutoTicketMinimumDimension minimumDimension = dimensionSetting as AutoTicketMinimumDimension;
              if (!string.IsNullOrWhiteSpace(minimumDimension.FeetValue) && !string.IsNullOrWhiteSpace(minimumDimension.inchesValue))
              {
                double.TryParse(minimumDimension.FeetValue, out this.AutoDimensionFeetValue);
                double.TryParse(minimumDimension.inchesValue, out this.AutoDimensionInchValue);
                this.MinDimFeetTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(minimumDimension.FeetValue, 4);
                this.MinDimInchTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(minimumDimension.inchesValue, 4);
              }
              double.TryParse(minimumDimension.FeetValue.Trim(), out this.AutoDimensionFeetValue);
              double.TryParse(minimumDimension.inchesValue.Trim(), out this.AutoDimensionInchValue);
              continue;
            default:
              continue;
          }
        }
      }
      if (this.eDrawingSettings != null)
      {
        foreach (AutoTicketSettingsTools eDrawingSetting in this.eDrawingSettings)
        {
          if (eDrawingSetting is AutoTicketCalloutAndDimensionTexts)
          {
            AutoTicketCalloutAndDimensionTexts andDimensionTexts = eDrawingSetting as AutoTicketCalloutAndDimensionTexts;
            this.DimensionStyleComboBoxAutoDimensionEDrawing.Text = andDimensionTexts.OverallDimension;
            this.TextStyleComboBoxAutoDimensionEDrawing.Text = andDimensionTexts.TextStyle;
          }
          else if (eDrawingSetting is AutoTicketAppendString)
          {
            AutoTicketAppendString ticketAppendString = eDrawingSetting as AutoTicketAppendString;
            new System.Windows.Documents.TextRange(this.AppendStringTextBoxAutoDimensionEDrawing.Document.ContentStart, this.AppendStringTextBoxAutoDimensionEDrawing.Document.ContentEnd).Text = ticketAppendString.AppendStringValue;
            this.ExampleResultTextBoxAutoDimensionEDrawing.Text = ticketAppendString.SampleResult;
          }
        }
      }
      if (this.cloneTicketSettings != null)
      {
        foreach (AutoTicketSettingsTools cloneTicketSetting in this.cloneTicketSettings)
        {
          if (cloneTicketSetting is CloneTicketSamePointTolerance)
          {
            CloneTicketSamePointTolerance samePointTolerance = cloneTicketSetting as CloneTicketSamePointTolerance;
            if (!string.IsNullOrWhiteSpace(samePointTolerance.FeetValue) && !string.IsNullOrWhiteSpace(samePointTolerance.inchesValue))
            {
              double.TryParse(samePointTolerance.FeetValue, out this.CloneTicketFeetValue);
              double.TryParse(samePointTolerance.inchesValue, out this.CloneTicketInchValue);
              this.SamePointToleranceFeetTextBox.Text = UnitConversion.roundToDecimalPointString(samePointTolerance.FeetValue, 4);
              this.SamePointToleranceInchTextBox.Text = UnitConversion.roundToDecimalPointString(samePointTolerance.inchesValue, 4);
            }
            double.TryParse(samePointTolerance.FeetValue.Trim(), out this.CloneTicketFeetValue);
            double.TryParse(samePointTolerance.inchesValue.Trim(), out this.CloneTicketInchValue);
          }
        }
      }
      if (this.hardwareDetailSettings != null)
      {
        foreach (AutoTicketSettingsTools hardwareDetailSetting in this.hardwareDetailSettings)
        {
          switch (hardwareDetailSetting)
          {
            case AutoTicketCalloutAndDimensionTexts _:
              AutoTicketCalloutAndDimensionTexts andDimensionTexts = hardwareDetailSetting as AutoTicketCalloutAndDimensionTexts;
              this.CalloutStyleComboBoxHardwareDetail.Text = andDimensionTexts.CalloutFamily;
              this.DimensionStyleComboBoxHardwareDetail.Text = andDimensionTexts.OverallDimension;
              this.OtherDimensionStyleComboBoxHardwareDetail.Text = andDimensionTexts.GeneralDimension;
              this.TextStyleComboBoxHardwareDetail.Text = andDimensionTexts.TextStyle;
              this.CheckBoxEqualityFormulaHardwareDetail.IsChecked = new bool?(andDimensionTexts.UseEQ);
              continue;
            case AutoTicketMinimumDimension _:
              AutoTicketMinimumDimension minimumDimension = hardwareDetailSetting as AutoTicketMinimumDimension;
              if (!string.IsNullOrWhiteSpace(minimumDimension.FeetValue) && !string.IsNullOrWhiteSpace(minimumDimension.inchesValue))
              {
                double.TryParse(minimumDimension.FeetValue, out this.HardwareDetailFeetValue);
                double.TryParse(minimumDimension.inchesValue, out this.HardwareDetailInchValue);
                this.MinDimFeetTextBoxHardwareDetail.Text = UnitConversion.roundToDecimalPointString(minimumDimension.FeetValue, 4);
                this.MinDimInchTextBoxHardwareDetail.Text = UnitConversion.roundToDecimalPointString(minimumDimension.inchesValue, 4);
              }
              double.TryParse(minimumDimension.FeetValue.Trim(), out this.HardwareDetailFeetValue);
              double.TryParse(minimumDimension.inchesValue.Trim(), out this.HardwareDetailInchValue);
              continue;
            case AutoTicketAppendString _:
              AutoTicketAppendString ticketAppendString = hardwareDetailSetting as AutoTicketAppendString;
              new System.Windows.Documents.TextRange(this.AppendStringTextBoxHardwareDetail.Document.ContentStart, this.AppendStringTextBoxHardwareDetail.Document.ContentEnd).Text = ticketAppendString.AppendStringValue;
              this.ExampleResultTextBoxHardwareDetail.Text = ticketAppendString.SampleResult;
              continue;
            default:
              continue;
          }
        }
      }
    }
    catch (Exception ex)
    {
    }
    if (!this.imperialUnits)
    {
      string millimetersString1 = UnitConversion.feetAndInchesToMillimetersString(this.TicketGenerationFeetValue.ToString(), this.TicketGenerationInchValue.ToString());
      string millimetersString2 = UnitConversion.feetAndInchesToMillimetersString(this.AutoDimensionFeetValue.ToString(), this.AutoDimensionInchValue.ToString());
      string millimetersString3 = UnitConversion.feetAndInchesToMillimetersString(this.HardwareDetailFeetValue.ToString(), this.HardwareDetailInchValue.ToString());
      double.TryParse(millimetersString1, out this.TicketGenerationMetricValue);
      double.TryParse(millimetersString2, out this.AutoDimensionMetricValue);
      ref double local = ref this.HardwareDetailMetricValue;
      double.TryParse(millimetersString3, out local);
      if (!string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxTicketGeneration.Text) || !string.IsNullOrWhiteSpace(this.MinDimInchTextBoxTicketGeneration.Text))
        this.MinDimFeetTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(millimetersString1, 4);
      if (!string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxAutoDimension.Text) || !string.IsNullOrWhiteSpace(this.MinDimInchTextBoxAutoDimension.Text))
        this.MinDimFeetTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(millimetersString2, 4);
      if (!string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxHardwareDetail.Text) || !string.IsNullOrWhiteSpace(this.MinDimInchTextBoxHardwareDetail.Text))
        this.MinDimFeetTextBoxHardwareDetail.Text = UnitConversion.roundToDecimalPointString(millimetersString2, 4);
    }
    this.ChangeUnitsButtonTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
    this.ChangeUnitsButtonAutoDimension.Visibility = System.Windows.Visibility.Hidden;
  }

  public static void populateParameterDropDown(
    List<AutoTicketAppendStringParameterData> parameterData,
    ComboBox parameterList,
    string tool)
  {
    foreach (AutoTicketAppendStringParameterData stringParameterData in parameterData)
    {
      if (!(stringParameterData.tool != tool))
      {
        ComboBoxItem newItem = new ComboBoxItem();
        newItem.Foreground = (Brush) Brushes.Black;
        if (stringParameterData.shortName.Contains("LIF"))
          newItem.Content = (object) (stringParameterData.shortName + "*");
        else
          newItem.Content = (object) stringParameterData.shortName;
        parameterList.Items.Add((object) newItem);
      }
    }
  }

  private void okayButton_Click(object sender, RoutedEventArgs e)
  {
    try
    {
      string text1 = this.CalloutStyleComboBoxTicketGeneration.Text;
      string text2 = this.DimensionStyleComboBoxTicketGeneration.Text;
      string text3 = this.OtherDimensionStyleComboBoxTicketGeneration.Text;
      string text4 = this.TextStyleComboBoxTicketGeneration.Text;
      bool valueOrDefault1 = this.CheckBoxEqualityFormula.IsChecked.GetValueOrDefault();
      string odimensions1 = text2;
      string gdimensions1 = text3;
      string texts1 = text4;
      bool? EQ1 = new bool?(valueOrDefault1);
      AutoTicketCalloutAndDimensionTexts andDimensionTexts1 = new AutoTicketCalloutAndDimensionTexts(text1, odimensions1, gdimensions1, texts1, EQ1);
      string appendStringV1 = "";
      foreach (Block block in (TextElementCollection<Block>) this.AppendStringTextBoxTicketGeneration.Document.Blocks)
      {
        if (block is Paragraph)
        {
          foreach (Inline inline in (TextElementCollection<Inline>) (block as Paragraph).Inlines)
          {
            System.Windows.Documents.TextRange textRange = new System.Windows.Documents.TextRange(inline.ContentStart, inline.ContentEnd);
            appendStringV1 += textRange.Text;
          }
        }
      }
      string text5 = this.ExampleResultTextBoxTicketGeneration.Text;
      AutoTicketAppendString ticketAppendString1 = new AutoTicketAppendString(appendStringV1, text5);
      string text6 = this.overallTextBoxTicketGeneration.Text;
      string text7 = this.contourTextBoxTicketGeneration.Text;
      string text8 = this.blockoutTextBoxTicketGeneration.Text;
      string contour = text7;
      string blockout = text8;
      AutoTicketCustomValues ticketCustomValues = new AutoTicketCustomValues(text6, contour, blockout);
      string str1 = "";
      string str2 = "";
      if (!FeetAndInchesRounding.verifyMinDim(this.MinDimFeetTextBoxTicketGeneration.Text) || !FeetAndInchesRounding.verifyMinDim(this.MinDimInchTextBoxTicketGeneration.Text))
        return;
      if (this.imperialUnits)
      {
        if (UnitConversion.roundToDecimalPointString(this.TicketGenerationFeetValue.ToString(), 4).Equals(this.MinDimFeetTextBoxTicketGeneration.Text) && UnitConversion.roundToDecimalPointString(this.TicketGenerationInchValue.ToString(), 4).Equals(this.MinDimInchTextBoxTicketGeneration.Text))
          FeetAndInchesRounding.verifyMinDimValues(this.TicketGenerationFeetValue.ToString(), this.TicketGenerationInchValue.ToString(), out str1, out str2);
        else
          FeetAndInchesRounding.verifyMinDimValues(this.MinDimFeetTextBoxTicketGeneration.Text, this.MinDimInchTextBoxTicketGeneration.Text, out str1, out str2);
      }
      else
      {
        if (UnitConversion.roundToDecimalPointString(this.TicketGenerationMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxTicketGeneration.Text.Trim()))
          UnitConversion.millimetersToFeetAndInchesString(this.TicketGenerationMetricValue.ToString(), out str1, out str2);
        else
          UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxTicketGeneration.Text, out str1, out str2);
        FeetAndInchesRounding.verifyMinDimValues(str1, str2, out str1, out str2);
      }
      if (!FeetAndInchesRounding.checkInputSize(str1, str2))
        return;
      AutoTicketMinimumDimension minimumDimension1 = new AutoTicketMinimumDimension(str1, str2);
      AutoTicketCalloutAndDimensionTexts andDimensionTexts2 = new AutoTicketCalloutAndDimensionTexts("", this.DimensionStyleComboBoxAutoDimension.Text, "", this.TextStyleComboBoxAutoDimension.Text);
      string appendStringV2 = "";
      foreach (Block block in (TextElementCollection<Block>) this.AppendStringTextBoxAutoDimension.Document.Blocks)
      {
        if (block is Paragraph)
        {
          foreach (Inline inline in (TextElementCollection<Inline>) (block as Paragraph).Inlines)
          {
            System.Windows.Documents.TextRange textRange = new System.Windows.Documents.TextRange(inline.ContentStart, inline.ContentEnd);
            appendStringV2 += textRange.Text;
          }
        }
      }
      string text9 = this.ExampleResultTextBoxAutoDimension.Text;
      AutoTicketAppendString ticketAppendString2 = new AutoTicketAppendString(appendStringV2, text9);
      string str3 = "";
      string str4 = "";
      if (!FeetAndInchesRounding.verifyMinDim(this.MinDimFeetTextBoxAutoDimension.Text) || !FeetAndInchesRounding.verifyMinDim(this.MinDimInchTextBoxAutoDimension.Text))
        return;
      if (this.imperialUnits)
      {
        if (UnitConversion.roundToDecimalPointString(this.AutoDimensionFeetValue.ToString(), 4).Equals(this.MinDimFeetTextBoxAutoDimension.Text) && UnitConversion.roundToDecimalPointString(this.AutoDimensionInchValue.ToString(), 4).Equals(this.MinDimInchTextBoxAutoDimension.Text))
          FeetAndInchesRounding.verifyMinDimValues(this.AutoDimensionFeetValue.ToString(), this.AutoDimensionInchValue.ToString(), out str3, out str4);
        else
          FeetAndInchesRounding.verifyMinDimValues(this.MinDimFeetTextBoxAutoDimension.Text, this.MinDimInchTextBoxAutoDimension.Text, out str3, out str4);
      }
      else
      {
        if (UnitConversion.roundToDecimalPointString(this.AutoDimensionMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxAutoDimension.Text.Trim()))
          UnitConversion.millimetersToFeetAndInchesString(this.AutoDimensionMetricValue.ToString(), out str3, out str4);
        else
          UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxAutoDimension.Text, out str3, out str4);
        FeetAndInchesRounding.verifyMinDimValues(str3, str4, out str3, out str4);
      }
      if (!FeetAndInchesRounding.checkInputSize(str3, str4))
        return;
      AutoTicketMinimumDimension minimumDimension2 = new AutoTicketMinimumDimension(str3, str4);
      AutoTicketCalloutAndDimensionTexts andDimensionTexts3 = new AutoTicketCalloutAndDimensionTexts("", this.DimensionStyleComboBoxAutoDimensionEDrawing.Text, "", this.TextStyleComboBoxAutoDimensionEDrawing.Text);
      string appendStringV3 = "";
      foreach (Block block in (TextElementCollection<Block>) this.AppendStringTextBoxAutoDimensionEDrawing.Document.Blocks)
      {
        if (block is Paragraph)
        {
          foreach (Inline inline in (TextElementCollection<Inline>) (block as Paragraph).Inlines)
          {
            System.Windows.Documents.TextRange textRange = new System.Windows.Documents.TextRange(inline.ContentStart, inline.ContentEnd);
            appendStringV3 += textRange.Text;
          }
        }
      }
      string text10 = this.ExampleResultTextBoxAutoDimensionEDrawing.Text;
      AutoTicketAppendString ticketAppendString3 = new AutoTicketAppendString(appendStringV3, text10);
      string str5 = "";
      string str6 = "";
      if (!FeetAndInchesRounding.verifyMinDim(this.SamePointToleranceFeetTextBox.Text) || !FeetAndInchesRounding.verifyMinDim(this.SamePointToleranceInchTextBox.Text))
        return;
      if (this.imperialUnits)
      {
        if (UnitConversion.roundToDecimalPointString(this.CloneTicketFeetValue.ToString(), 4).Equals(this.SamePointToleranceFeetTextBox.Text) && UnitConversion.roundToDecimalPointString(this.CloneTicketInchValue.ToString(), 4).Equals(this.SamePointToleranceInchTextBox.Text))
          FeetAndInchesRounding.verifyMinDimValues(this.CloneTicketFeetValue.ToString(), this.CloneTicketInchValue.ToString(), out str5, out str6);
        else
          FeetAndInchesRounding.verifyMinDimValues(this.SamePointToleranceFeetTextBox.Text, this.SamePointToleranceInchTextBox.Text, out str5, out str6);
      }
      else
      {
        if (UnitConversion.roundToDecimalPointString(this.CloneTicketMetricValue.ToString(), 4).Equals(this.SamePointToleranceFeetTextBox.Text.Trim()))
          UnitConversion.millimetersToFeetAndInchesString(this.CloneTicketMetricValue.ToString(), out str5, out str6);
        else
          UnitConversion.millimetersToFeetAndInchesString(this.SamePointToleranceFeetTextBox.Text, out str5, out str6);
        FeetAndInchesRounding.verifyMinDimValues(str5, str6, out str5, out str6);
      }
      if (!FeetAndInchesRounding.checkInputSize(str5, str6))
        return;
      CloneTicketSamePointTolerance samePointTolerance = new CloneTicketSamePointTolerance(str5, str6);
      string text11 = this.CalloutStyleComboBoxHardwareDetail.Text;
      string text12 = this.DimensionStyleComboBoxHardwareDetail.Text;
      string text13 = this.OtherDimensionStyleComboBoxHardwareDetail.Text;
      string text14 = this.TextStyleComboBoxHardwareDetail.Text;
      bool valueOrDefault2 = this.CheckBoxEqualityFormulaHardwareDetail.IsChecked.GetValueOrDefault();
      string odimensions2 = text12;
      string gdimensions2 = text13;
      string texts2 = text14;
      bool? EQ2 = new bool?(valueOrDefault2);
      AutoTicketCalloutAndDimensionTexts andDimensionTexts4 = new AutoTicketCalloutAndDimensionTexts(text11, odimensions2, gdimensions2, texts2, EQ2);
      string appendStringV4 = "";
      foreach (Block block in (TextElementCollection<Block>) this.AppendStringTextBoxHardwareDetail.Document.Blocks)
      {
        if (block is Paragraph)
        {
          foreach (Inline inline in (TextElementCollection<Inline>) (block as Paragraph).Inlines)
          {
            System.Windows.Documents.TextRange textRange = new System.Windows.Documents.TextRange(inline.ContentStart, inline.ContentEnd);
            appendStringV4 += textRange.Text;
          }
        }
      }
      string text15 = this.ExampleResultTextBoxHardwareDetail.Text;
      AutoTicketAppendString ticketAppendString4 = new AutoTicketAppendString(appendStringV4, text15);
      string str7 = "";
      string str8 = "";
      if (!FeetAndInchesRounding.verifyMinDim(this.MinDimFeetTextBoxHardwareDetail.Text) || !FeetAndInchesRounding.verifyMinDim(this.MinDimInchTextBoxHardwareDetail.Text))
        return;
      if (this.imperialUnits)
      {
        if (UnitConversion.roundToDecimalPointString(this.HardwareDetailFeetValue.ToString(), 4).Equals(this.MinDimFeetTextBoxHardwareDetail.Text) && UnitConversion.roundToDecimalPointString(this.HardwareDetailInchValue.ToString(), 4).Equals(this.MinDimInchTextBoxHardwareDetail.Text))
          FeetAndInchesRounding.verifyMinDimValues(this.HardwareDetailFeetValue.ToString(), this.HardwareDetailInchValue.ToString(), out str7, out str8);
        else
          FeetAndInchesRounding.verifyMinDimValues(this.MinDimFeetTextBoxHardwareDetail.Text, this.MinDimInchTextBoxHardwareDetail.Text, out str7, out str8);
      }
      else
      {
        if (UnitConversion.roundToDecimalPointString(this.HardwareDetailMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxHardwareDetail.Text.Trim()))
          UnitConversion.millimetersToFeetAndInchesString(this.HardwareDetailMetricValue.ToString(), out str7, out str8);
        else
          UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxHardwareDetail.Text, out str7, out str8);
        FeetAndInchesRounding.verifyMinDimValues(str7, str8, out str7, out str8);
      }
      if (!FeetAndInchesRounding.checkInputSize(str7, str8))
        return;
      AutoTicketMinimumDimension minimumDimension3 = new AutoTicketMinimumDimension(str7, str8);
      string str9 = $"{$"{$"{$"{$"{$"{$"{$"{$"{$"{"" + "\"AUTO-TICKET GENERATION\","}\"{andDimensionTexts1.CalloutFamily}\","}\"{andDimensionTexts1.OverallDimension}\","}\"{andDimensionTexts1.GeneralDimension}\","}\"{andDimensionTexts1.TextStyle}\","}\"{ticketAppendString1.AppendStringValue.TrimEnd()}\","}\"{ticketAppendString1.SampleResult}\","}{FeetAndInchesRounding.addLegLength(minimumDimension1.FeetValue, minimumDimension1.inchesValue)},"}\"{ticketCustomValues.Overall}\","}\"{ticketCustomValues.Contour}\","}\"{ticketCustomValues.Blockout}\",";
      bool useEq = andDimensionTexts1.UseEQ;
      string upper1 = useEq.ToString().ToUpper();
      string str10 = $"{str9}\"{upper1}\"";
      string str11 = $"{$"{$"{$"{"" + "\"AUTO-DIMENSION\","}\"{andDimensionTexts2.OverallDimension}\","}\"{andDimensionTexts2.TextStyle}\","}\"{ticketAppendString2.AppendStringValue}\","}\"{ticketAppendString2.SampleResult}\"," + FeetAndInchesRounding.addLegLength(minimumDimension2.FeetValue, minimumDimension2.inchesValue);
      string str12 = $"{$"{$"{$"{"" + "\"AUTO-DIMENSION E-DRAWING\","}\"{andDimensionTexts3.OverallDimension}\","}\"{andDimensionTexts3.TextStyle}\","}\"{ticketAppendString3.AppendStringValue}\","}\"{ticketAppendString3.SampleResult}\"";
      string str13 = "" + "\"CLONE TICKET\"," + FeetAndInchesRounding.addLegLength(samePointTolerance.FeetValue, samePointTolerance.inchesValue);
      string str14 = $"{$"{$"{$"{$"{$"{$"{"" + "\"HARDWARE DETAIL\","}\"{andDimensionTexts4.CalloutFamily}\","}\"{andDimensionTexts4.OverallDimension}\","}\"{andDimensionTexts4.GeneralDimension}\","}\"{andDimensionTexts4.TextStyle}\","}\"{ticketAppendString4.AppendStringValue.Trim()}\","}\"{ticketAppendString4.SampleResult}\","}{FeetAndInchesRounding.addLegLength(minimumDimension3.FeetValue, minimumDimension3.inchesValue)},";
      useEq = andDimensionTexts4.UseEQ;
      string upper2 = useEq.ToString().ToUpper();
      string str15 = $"{str14}\"{upper2}\"";
      string str16 = "" + "\"PARAMETERS\"\n";
      List<AutoTicketAppendStringParameterData> source1 = new List<AutoTicketAppendStringParameterData>();
      source1.AddRange((IEnumerable<AutoTicketAppendStringParameterData>) this.appendStringParameterTicketGeneration);
      source1.AddRange((IEnumerable<AutoTicketAppendStringParameterData>) this.appendStringParameterAutoDimension);
      source1.AddRange((IEnumerable<AutoTicketAppendStringParameterData>) this.appendStringParameterEDrawing);
      source1.AddRange((IEnumerable<AutoTicketAppendStringParameterData>) this.appendStringParameterHardwareDetail);
      foreach (IGrouping<string, AutoTicketAppendStringParameterData> source2 in source1.GroupBy<AutoTicketAppendStringParameterData, string>((Func<AutoTicketAppendStringParameterData, string>) (name => name.shortName)))
      {
        string key = source2.Key;
        string longName = source2.FirstOrDefault<AutoTicketAppendStringParameterData>().longName;
        AutoTicketAppendStringParameterData stringParameterData1 = (AutoTicketAppendStringParameterData) null;
        AutoTicketAppendStringParameterData stringParameterData2 = (AutoTicketAppendStringParameterData) null;
        AutoTicketAppendStringParameterData stringParameterData3 = (AutoTicketAppendStringParameterData) null;
        AutoTicketAppendStringParameterData stringParameterData4 = (AutoTicketAppendStringParameterData) null;
        foreach (AutoTicketAppendStringParameterData stringParameterData5 in (IEnumerable<AutoTicketAppendStringParameterData>) source2)
        {
          switch (stringParameterData5.tool)
          {
            case "AUTO-TICKET GENERATION":
              stringParameterData1 = stringParameterData5;
              continue;
            case "AUTO-DIMENSION":
              stringParameterData2 = stringParameterData5;
              continue;
            case "AUTO-DIMENSION E-DRAWING":
              stringParameterData3 = stringParameterData5;
              continue;
            case "HARDWARE DETAIL":
              stringParameterData4 = stringParameterData5;
              continue;
            default:
              continue;
          }
        }
        str16 = $"{str16}\"{key}\",";
        str16 = $"{str16}\"{longName}\",";
        if (stringParameterData1 != null)
          str16 = $"{str16}\"{stringParameterData1.prefix}\",\"{stringParameterData1.suffix}\",";
        if (stringParameterData2 != null)
          str16 = $"{str16}\"{stringParameterData2.prefix}\",\"{stringParameterData2.suffix}\",";
        if (stringParameterData3 != null)
          str16 = $"{str16}\"{stringParameterData3.prefix}\",\"{stringParameterData3.suffix}\",";
        if (stringParameterData4 != null)
          str16 = $"{str16}\"{stringParameterData4.prefix}\",\"{stringParameterData4.suffix}\"\n";
      }
      AutoTicketSettingsReader.SaveFile(new List<string>()
      {
        str10,
        str11,
        str12,
        str13,
        str15,
        str16
      });
      this.Close();
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show("File could not be saved. Please try again.");
    }
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void Setting_SelectedTicketGeneration(object sender, SelectionChangedEventArgs e)
  {
    ListBox listBox = sender as ListBox;
    if (listBox.SelectedItem == null)
      return;
    string str = (listBox.SelectedItem as ListBoxItem).Content.ToString();
    if (str != null)
    {
      switch (str.Length)
      {
        case 13:
          if (str == "Append String")
          {
            switch (listBox.Name)
            {
              case "SettingTicketGeneration":
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Visible;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Visible;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                return;
              case "SettingAutoDimension":
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Visible;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Visible;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
              case "SettingAutoDimensionEDrawing":
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Visible;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Visible;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
              case "SettingHardwareDetail":
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Visible;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockHardwareDetail.Visibility = System.Windows.Visibility.Visible;
                break;
            }
          }
          else
            break;
          break;
        case 20:
          if (str == "Same Point Tolerance")
          {
            this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Visible;
            this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
            break;
          }
          break;
        case 22:
          if (str == "Precast Dimension Text")
          {
            this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Visible;
            this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
            this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
            this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
            this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
            this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
            break;
          }
          break;
        case 23:
          if (str == "Minimum Dimension Value")
          {
            switch (listBox.Name)
            {
              case "SettingTicketGeneration":
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Visible;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
              case "SettingAutoDimension":
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Visible;
                this.ApplicableTextAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
              case "SettingHardwareDetail":
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Visible;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
            }
          }
          else
            break;
          break;
        case 25:
          if (str == "Dimension and Text Styles")
          {
            switch (listBox.Name)
            {
              case "SettingAutoDimension":
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Visible;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
              case "SettingAutoDimensionEDrawing":
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Visible;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
            }
          }
          else
            break;
          break;
        case 29:
          switch (str[9])
          {
            case 'D':
              if (str == "Callout, Dimensions, and Text")
              {
                switch (listBox.Name)
                {
                  case "SettingTicketGeneration":
                    this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Visible;
                    this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                    this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                    this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                    break;
                  case "SettingHardwareDetail":
                    this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                    this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                    this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                    this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                    this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Visible;
                    this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                    break;
                }
              }
              else
                break;
              break;
            case 'T':
              if (str == "Callout, Text, and Dimensions" && listBox.Name == "SettingHardwareDetail")
              {
                this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
                this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
                this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Visible;
                this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
                break;
              }
              break;
          }
          break;
      }
    }
    if (this.MinDimTabTicketGeneration.Visibility == System.Windows.Visibility.Visible)
    {
      if (this.imperialUnits)
      {
        this.MinDimFeetLabelTicketGeneration.Text = "ft";
        this.MinDimInchTextBoxTicketGeneration.Visibility = System.Windows.Visibility.Visible;
        this.MinDimInchLabelTicketGeneration.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        this.MinDimFeetLabelTicketGeneration.Text = "  mm";
        this.MinDimInchTextBoxTicketGeneration.Visibility = System.Windows.Visibility.Collapsed;
        this.MinDimInchLabelTicketGeneration.Visibility = System.Windows.Visibility.Collapsed;
      }
    }
    if (this.MinDimTabAutoDimension.Visibility == System.Windows.Visibility.Visible)
    {
      if (this.imperialUnits)
      {
        this.MinDimFeetLabelAutoDimension.Text = "ft";
        this.MinDimInchTextBoxAutoDimension.Visibility = System.Windows.Visibility.Visible;
        this.MinDimInchLabelAutoDimension.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        this.MinDimFeetLabelAutoDimension.Text = "  mm";
        this.MinDimInchTextBoxAutoDimension.Visibility = System.Windows.Visibility.Collapsed;
        this.MinDimInchLabelAutoDimension.Visibility = System.Windows.Visibility.Collapsed;
      }
    }
    if (this.MinDimTabHardwareDetail.Visibility == System.Windows.Visibility.Visible)
    {
      if (this.imperialUnits)
      {
        this.MinDimFeetLabelHardwareDetail.Text = "ft";
        this.MinDimInchTextBoxHardwareDetail.Visibility = System.Windows.Visibility.Visible;
        this.MinDimInchLabelHardwareDetail.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        this.MinDimFeetLabelHardwareDetail.Text = "  mm";
        this.MinDimInchTextBoxHardwareDetail.Visibility = System.Windows.Visibility.Collapsed;
        this.MinDimInchLabelHardwareDetail.Visibility = System.Windows.Visibility.Collapsed;
      }
    }
    if (this.SamePointToleranceTabCloneTicket.Visibility != System.Windows.Visibility.Visible)
      return;
    if (this.imperialUnits)
    {
      this.SamePointToleranceFeetLabel.Text = "ft";
      this.SamePointToleranceInchTextBox.Visibility = System.Windows.Visibility.Visible;
      this.SamePointToleranceInchLabel.Visibility = System.Windows.Visibility.Visible;
    }
    else
    {
      this.SamePointToleranceFeetLabel.Text = "  mm";
      this.SamePointToleranceInchTextBox.Visibility = System.Windows.Visibility.Collapsed;
      this.SamePointToleranceInchLabel.Visibility = System.Windows.Visibility.Collapsed;
    }
  }

  private void AddButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.bTicketGenerationTabActive)
    {
      if (this.ParameterValuesListTicketGeneration.SelectedItem == null)
        return;
      string str1 = this.AddButtonTicketGeneration.Content.ToString().Replace("+", "");
      string str2 = "";
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterTicketGeneration)
      {
        if (stringParameterData.shortName == str1)
        {
          str2 = str1;
          stringParameterData.prefix = this.PrefixTextBoxTicketGeneration.Text;
          stringParameterData.suffix = this.SuffixTextBoxTicketGeneration.Text;
          break;
        }
      }
      if (string.IsNullOrEmpty(str2))
        return;
      this.AppendStringTextBoxTicketGeneration.CaretPosition = new System.Windows.Documents.TextRange(this.AppendStringTextBoxTicketGeneration.CaretPosition, this.AppendStringTextBoxTicketGeneration.CaretPosition)
      {
        Text = str2
      }.End;
    }
    else if (this.bAutoDimensionTabActive)
    {
      if (this.ParameterValuesListAutoDimension.SelectedItem == null)
        return;
      string str3 = this.AddButtonAutoDimension.Content.ToString().Replace("+", "");
      string str4 = "";
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterAutoDimension)
      {
        if (stringParameterData.shortName == str3)
        {
          str4 = str3;
          stringParameterData.prefix = this.PrefixTextBoxAutoDimension.Text;
          stringParameterData.suffix = this.SuffixTextBoxAutoDimension.Text;
          break;
        }
      }
      if (string.IsNullOrEmpty(str4))
        return;
      this.AppendStringTextBoxAutoDimension.CaretPosition = new System.Windows.Documents.TextRange(this.AppendStringTextBoxAutoDimension.CaretPosition, this.AppendStringTextBoxAutoDimension.CaretPosition)
      {
        Text = str4
      }.End;
    }
    else if (this.bEDrawingTabActive)
    {
      if (this.ParameterValuesListAutoDimensionEDrawing.SelectedItem == null)
        return;
      string str5 = this.AddButtonAutoDimensionEDrawing.Content.ToString().Replace("+", "");
      string str6 = "";
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterEDrawing)
      {
        if (stringParameterData.shortName == str5)
        {
          str6 = str5;
          stringParameterData.prefix = this.PrefixTextBoxAutoDimensionEDrawing.Text;
          stringParameterData.suffix = this.SuffixTextBoxAutoDimensionEDrawing.Text;
          break;
        }
      }
      if (string.IsNullOrEmpty(str6))
        return;
      this.AppendStringTextBoxAutoDimensionEDrawing.CaretPosition = new System.Windows.Documents.TextRange(this.AppendStringTextBoxAutoDimensionEDrawing.CaretPosition, this.AppendStringTextBoxAutoDimensionEDrawing.CaretPosition)
      {
        Text = str6
      }.End;
    }
    else
    {
      if (!this.bHardwareDetail || this.ParameterValuesListHardwareDetail.SelectedItem == null)
        return;
      string str7 = this.AddButtonHardwareDetail.Content.ToString().Replace("+", "");
      string str8 = "";
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterHardwareDetail)
      {
        if (stringParameterData.shortName == str7)
        {
          str8 = str7;
          stringParameterData.prefix = this.PrefixTextBoxHardwareDetail.Text;
          stringParameterData.suffix = this.SuffixTextBoxHardwareDetail.Text;
          break;
        }
      }
      if (string.IsNullOrEmpty(str8))
        return;
      this.AppendStringTextBoxHardwareDetail.CaretPosition = new System.Windows.Documents.TextRange(this.AppendStringTextBoxHardwareDetail.CaretPosition, this.AppendStringTextBoxHardwareDetail.CaretPosition)
      {
        Text = str8
      }.End;
    }
  }

  private void DropdownFamiliesinProject(Document revitDoc)
  {
    FilteredElementCollector source1 = new FilteredElementCollector(revitDoc);
    source1.OfCategory(BuiltInCategory.OST_MultiCategoryTags);
    source1.OfClass(typeof (FamilySymbol));
    List<FamilySymbol> list1 = source1.Select<Element, FamilySymbol>((Func<Element, FamilySymbol>) (e => e as FamilySymbol)).ToList<FamilySymbol>();
    List<string> stringList1 = new List<string>();
    foreach (IEnumerable<FamilySymbol> source2 in list1.GroupBy<FamilySymbol, string>((Func<FamilySymbol, string>) (e => e.FamilyName)))
    {
      List<FamilySymbol> list2 = source2.ToList<FamilySymbol>();
      if (list2.Count > 1 && list2.Any<FamilySymbol>((Func<FamilySymbol, bool>) (name => name.Name == "Left Justified")) && list2.Any<FamilySymbol>((Func<FamilySymbol, bool>) (name => name.Name == "Right Justified")) && !stringList1.Contains(list2.FirstOrDefault<FamilySymbol>().FamilyName))
        stringList1.Add(list2.FirstOrDefault<FamilySymbol>().FamilyName);
    }
    stringList1.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.CalloutStyleComboBoxTicketGeneration.ItemsSource = (IEnumerable) stringList1;
    this.CalloutStyleComboBoxHardwareDetail.ItemsSource = (IEnumerable) stringList1;
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType));
    List<string> stringList2 = new List<string>();
    List<string> stringList3 = new List<string>();
    List<string> stringList4 = new List<string>();
    List<string> stringList5 = new List<string>();
    foreach (DimensionType dimensionType in elementCollector)
    {
      if (!(dimensionType.FamilyName == dimensionType.Name) && dimensionType.StyleType == DimensionStyleType.Linear)
      {
        stringList2.Add(dimensionType.Name);
        stringList3.Add(dimensionType.Name);
        stringList4.Add(dimensionType.Name);
        stringList5.Add(dimensionType.Name);
      }
    }
    if (stringList2.Count > 0)
    {
      stringList2.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
      this.DimensionStyleComboBoxTicketGeneration.ItemsSource = (IEnumerable) stringList2;
      this.OtherDimensionStyleComboBoxTicketGeneration.ItemsSource = (IEnumerable) stringList2;
      stringList3.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
      stringList4.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
      stringList5.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
      this.DimensionStyleComboBoxAutoDimension.ItemsSource = (IEnumerable) stringList3;
      this.DimensionStyleComboBoxAutoDimensionEDrawing.ItemsSource = (IEnumerable) stringList4;
      this.DimensionStyleComboBoxHardwareDetail.ItemsSource = (IEnumerable) stringList5;
      this.OtherDimensionStyleComboBoxHardwareDetail.ItemsSource = (IEnumerable) stringList5;
    }
    List<string> stringList6 = new List<string>();
    foreach (TextNoteType textNoteType in new FilteredElementCollector(revitDoc).OfClass(typeof (TextNoteType)))
      stringList6.Add(textNoteType.Name);
    if (stringList6.Count <= 0)
      return;
    stringList6.Sort((Comparison<string>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p, q)));
    this.TextStyleComboBoxTicketGeneration.ItemsSource = (IEnumerable) stringList6;
    this.TextStyleComboBoxAutoDimension.ItemsSource = (IEnumerable) stringList6;
    this.TextStyleComboBoxAutoDimensionEDrawing.ItemsSource = (IEnumerable) stringList6;
    this.TextStyleComboBoxHardwareDetail.ItemsSource = (IEnumerable) stringList6;
  }

  private void ParameterValuesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (!(sender is ComboBox))
      return;
    ComboBoxItem selectedItem = (sender as ComboBox).SelectedItem as ComboBoxItem;
    if (this.bTicketGenerationTabActive)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterTicketGeneration)
      {
        if (selectedItem.Content.ToString().Contains(stringParameterData.shortName))
        {
          this.AddButtonTicketGeneration.Content = (object) ("+" + stringParameterData.shortName);
          this.AddButtonTicketGeneration.Foreground = (Brush) Brushes.Blue;
          if (stringParameterData.shortName.Contains("LIF"))
            this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Visible;
          else
            this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str1 = "";
          foreach (string str2 in strArray)
            str1 += str2;
          this.ResultTextBoxTicketGeneration.Text = stringParameterData.prefix + str1 + stringParameterData.suffix;
          this.doNotUpdatePrefixSuffix = true;
          this.PrefixTextBoxTicketGeneration.Text = stringParameterData.prefix;
          this.SuffixTextBoxTicketGeneration.Text = stringParameterData.suffix;
          break;
        }
      }
    }
    else if (this.bAutoDimensionTabActive)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterAutoDimension)
      {
        if (selectedItem.Content.ToString().Contains(stringParameterData.shortName))
        {
          this.AddButtonAutoDimension.Content = (object) ("+" + stringParameterData.shortName);
          this.AddButtonAutoDimension.Foreground = (Brush) Brushes.Blue;
          if (stringParameterData.shortName.Contains("LIF"))
            this.ApplicableTextAutoDimension.Visibility = System.Windows.Visibility.Visible;
          else
            this.ApplicableTextAutoDimension.Visibility = System.Windows.Visibility.Hidden;
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str3 = "";
          foreach (string str4 in strArray)
            str3 += str4;
          this.ResultTextBoxAutoDimension.Text = stringParameterData.prefix + str3 + stringParameterData.suffix;
          this.doNotUpdatePrefixSuffix = true;
          this.PrefixTextBoxAutoDimension.Text = stringParameterData.prefix;
          this.SuffixTextBoxAutoDimension.Text = stringParameterData.suffix;
          break;
        }
      }
    }
    else if (this.bEDrawingTabActive)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterEDrawing)
      {
        if (selectedItem.Content.ToString().Contains(stringParameterData.shortName))
        {
          this.AddButtonAutoDimensionEDrawing.Content = (object) ("+" + stringParameterData.shortName);
          this.AddButtonAutoDimensionEDrawing.Foreground = (Brush) Brushes.Blue;
          if (stringParameterData.shortName.Contains("LIF"))
            this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Visible;
          else
            this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str5 = "";
          foreach (string str6 in strArray)
            str5 += str6;
          this.ResultTextBoxAutoDimensionEDrawing.Text = stringParameterData.prefix + str5 + stringParameterData.suffix;
          this.doNotUpdatePrefixSuffix = true;
          this.PrefixTextBoxAutoDimensionEDrawing.Text = stringParameterData.prefix;
          this.SuffixTextBoxAutoDimensionEDrawing.Text = stringParameterData.suffix;
          break;
        }
      }
    }
    else if (this.bHardwareDetail)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterHardwareDetail)
      {
        if (selectedItem.Content.ToString().Contains(stringParameterData.shortName))
        {
          this.AddButtonHardwareDetail.Content = (object) ("+" + stringParameterData.shortName);
          this.AddButtonHardwareDetail.Foreground = (Brush) Brushes.Blue;
          if (stringParameterData.shortName.Contains("LIF"))
            this.ApplicableTextHardwareDetail.Visibility = System.Windows.Visibility.Visible;
          else
            this.ApplicableTextHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str7 = "";
          foreach (string str8 in strArray)
            str7 += str8;
          this.ResultTextBoxHardwareDetail.Text = stringParameterData.prefix + str7 + stringParameterData.suffix;
          this.doNotUpdatePrefixSuffix = true;
          this.PrefixTextBoxHardwareDetail.Text = stringParameterData.prefix;
          this.SuffixTextBoxHardwareDetail.Text = stringParameterData.suffix;
          break;
        }
      }
    }
    this.doNotUpdatePrefixSuffix = false;
  }

  private void AppendStringTextBoxHardwareDetail_TextChanged(object sender, TextChangedEventArgs e)
  {
    if (this.changeIsRunningHardwareDetail)
      return;
    this.changeIsRunningHardwareDetail = true;
    try
    {
      this.ExampleResultTextBoxHardwareDetail.Text = this.processBlocksAndExtractExampleValue(this.AppendStringTextBoxHardwareDetail.Document, this.appendStringParameterHardwareDetail);
    }
    finally
    {
      this.changeIsRunningHardwareDetail = false;
    }
  }

  private void AppendStringTextBoxAutoDimensionEDrawing_TextChanged(
    object sender,
    TextChangedEventArgs e)
  {
    if (this.changeIsRunningAutoDimensionEDrawing)
      return;
    this.changeIsRunningAutoDimensionEDrawing = true;
    try
    {
      this.ExampleResultTextBoxAutoDimensionEDrawing.Text = this.processBlocksAndExtractExampleValue(this.AppendStringTextBoxAutoDimensionEDrawing.Document, this.appendStringParameterEDrawing);
    }
    finally
    {
      this.changeIsRunningAutoDimensionEDrawing = false;
    }
  }

  private void AppendStringTextBoxAutoDimension_TextChanged(object sender, TextChangedEventArgs e)
  {
    if (this.changeIsRunningAutoDimension)
      return;
    this.changeIsRunningAutoDimension = true;
    try
    {
      this.ExampleResultTextBoxAutoDimension.Text = this.processBlocksAndExtractExampleValue(this.AppendStringTextBoxAutoDimension.Document, this.appendStringParameterAutoDimension);
    }
    finally
    {
      this.changeIsRunningAutoDimension = false;
    }
  }

  private void AppendStringTextBoxTicketGeneration_TextChanged(
    object sender,
    TextChangedEventArgs e)
  {
    if (this.changeIsRunningTicketGeneration)
      return;
    this.changeIsRunningTicketGeneration = true;
    try
    {
      this.ExampleResultTextBoxTicketGeneration.Text = this.processBlocksAndExtractExampleValue(this.AppendStringTextBoxTicketGeneration.Document, this.appendStringParameterTicketGeneration);
    }
    finally
    {
      this.changeIsRunningTicketGeneration = false;
    }
  }

  private string processBlocksAndExtractExampleValue(
    FlowDocument doc,
    List<AutoTicketAppendStringParameterData> listOfParameters)
  {
    string exampleValue = "";
    foreach (Block block in (TextElementCollection<Block>) doc.Blocks)
    {
      foreach (Inline inline in (TextElementCollection<Inline>) (block as Paragraph).Inlines)
      {
        string text = new System.Windows.Documents.TextRange(inline.ContentStart, inline.ContentEnd).Text;
        MatchCollection matchCollection = new Regex("{{[^{}]+?}}").Matches(text);
        int num = 0;
        if (matchCollection.Count == 0)
        {
          exampleValue += text;
        }
        else
        {
          foreach (Match match in matchCollection)
          {
            string str1 = match.Value.ToString().Remove(0, 1).Remove(0, 1);
            string str2 = str1.Remove(str1.Length - 1);
            string str3 = str2.Remove(str2.Length - 1);
            bool flag = false;
            foreach (AutoTicketAppendStringParameterData listOfParameter in listOfParameters)
            {
              if (listOfParameter.shortName.Equals(match.Value.ToString()))
              {
                str3 = listOfParameter.prefix + str3 + listOfParameter.suffix;
                flag = true;
                break;
              }
            }
            if (flag)
            {
              if (match.Index == num)
                exampleValue += str3.ToString();
              else if (match.Index > num)
              {
                string str4 = text.Substring(num, match.Index - num);
                exampleValue = exampleValue + str4 + str3;
                num += str4.Length;
              }
              num += match.Length;
            }
            if (match.NextMatch() == Match.Empty && num < text.Length)
            {
              exampleValue += text.Remove(0, num);
              num += text.Remove(0, num).Length;
            }
          }
        }
      }
    }
    return exampleValue;
  }

  private void PrePostTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    if (this.doNotUpdatePrefixSuffix)
      return;
    if (this.bTicketGenerationTabActive)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterTicketGeneration)
      {
        if (this.ParameterValuesListTicketGeneration.SelectedItem != null && this.ParameterValuesListTicketGeneration.SelectedItem.ToString().Contains(stringParameterData.shortName))
        {
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str1 = "";
          foreach (string str2 in strArray)
            str1 += str2;
          this.ResultTextBoxTicketGeneration.Text = this.PrefixTextBoxTicketGeneration.Text + str1 + this.SuffixTextBoxTicketGeneration.Text;
          break;
        }
      }
    }
    else if (this.bAutoDimensionTabActive)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterAutoDimension)
      {
        if (this.ParameterValuesListAutoDimension.SelectedItem != null && this.ParameterValuesListAutoDimension.SelectedItem.ToString().Contains(stringParameterData.shortName))
        {
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str3 = "";
          foreach (string str4 in strArray)
            str3 += str4;
          this.ResultTextBoxAutoDimension.Text = this.PrefixTextBoxAutoDimension.Text + str3 + this.SuffixTextBoxAutoDimension.Text;
          break;
        }
      }
    }
    else if (this.bEDrawingTabActive)
    {
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterEDrawing)
      {
        if (this.ParameterValuesListAutoDimensionEDrawing.SelectedItem != null && this.ParameterValuesListAutoDimensionEDrawing.SelectedItem.ToString().Contains(stringParameterData.shortName))
        {
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str5 = "";
          foreach (string str6 in strArray)
            str5 += str6;
          this.ResultTextBoxAutoDimensionEDrawing.Text = this.PrefixTextBoxAutoDimensionEDrawing.Text + str5 + this.SuffixTextBoxAutoDimensionEDrawing.Text;
          break;
        }
      }
    }
    else
    {
      if (!this.bHardwareDetail)
        return;
      foreach (AutoTicketAppendStringParameterData stringParameterData in this.appendStringParameterHardwareDetail)
      {
        if (this.ParameterValuesListHardwareDetail.SelectedItem != null && this.ParameterValuesListHardwareDetail.SelectedItem.ToString().Contains(stringParameterData.shortName))
        {
          string[] strArray = stringParameterData.shortName.Split(new char[2]
          {
            '{',
            '}'
          }, StringSplitOptions.RemoveEmptyEntries);
          string str7 = "";
          foreach (string str8 in strArray)
            str7 += str8;
          this.ResultTextBoxHardwareDetail.Text = this.PrefixTextBoxHardwareDetail.Text + str7 + this.SuffixTextBoxHardwareDetail.Text;
          break;
        }
      }
    }
  }

  private void TabPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (!(sender is TabControl) || !(((sender as TabControl).SelectedItem as TabItem).Header is string header))
      return;
    switch (header)
    {
      case "Auto Ticket Generation":
        this.AutoTicketTab.Visibility = System.Windows.Visibility.Visible;
        this.AutoDimensionTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionEDrawingTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.CloneTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
        this.HardwareDetailTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.SettingAutoDimension.SelectedItem = (object) null;
        this.SettingAutoDimensionEDrawing.SelectedItem = (object) null;
        this.SettingCloneTicket.SelectedItem = (object) null;
        this.SettingHardwareDetail.SelectedItem = (object) null;
        this.bTicketGenerationTabActive = true;
        this.bAutoDimensionTabActive = false;
        this.bEDrawingTabActive = false;
        this.bCloneTicket = false;
        this.bHardwareDetail = false;
        break;
      case "Ticket Auto-dimension":
        this.AutoTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionTab.Visibility = System.Windows.Visibility.Visible;
        this.AutoDimensionEDrawingTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.CloneTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
        this.HardwareDetailTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.SettingTicketGeneration.SelectedItem = (object) null;
        this.SettingAutoDimensionEDrawing.SelectedItem = (object) null;
        this.SettingCloneTicket.SelectedItem = (object) null;
        this.SettingHardwareDetail.SelectedItem = (object) null;
        this.bTicketGenerationTabActive = false;
        this.bAutoDimensionTabActive = true;
        this.bEDrawingTabActive = false;
        this.bCloneTicket = false;
        this.bHardwareDetail = false;
        break;
      case "Erection Drawing Auto-dimension":
        this.AutoTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.CloneTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
        this.HardwareDetailTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionEDrawingTab.Visibility = System.Windows.Visibility.Visible;
        this.SettingTicketGeneration.SelectedItem = (object) null;
        this.SettingAutoDimension.SelectedItem = (object) null;
        this.SettingCloneTicket.SelectedItem = (object) null;
        this.SettingHardwareDetail.SelectedItem = (object) null;
        this.bTicketGenerationTabActive = false;
        this.bAutoDimensionTabActive = false;
        this.bEDrawingTabActive = true;
        this.bCloneTicket = false;
        this.bHardwareDetail = false;
        break;
      case "Clone Ticket":
        this.AutoTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionEDrawingTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.CloneTicketTab.Visibility = System.Windows.Visibility.Visible;
        this.HardwareDetailTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.SettingTicketGeneration.SelectedItem = (object) null;
        this.SettingAutoDimension.SelectedItem = (object) null;
        this.SettingAutoDimensionEDrawing.SelectedItem = (object) null;
        this.SettingHardwareDetail.SelectedItem = (object) null;
        this.SettingCloneTicket.SelectedItem = (object) (ListBoxItem) this.SettingCloneTicket.ItemContainerGenerator.ContainerFromIndex(0);
        this.bTicketGenerationTabActive = false;
        this.bAutoDimensionTabActive = false;
        this.bEDrawingTabActive = false;
        this.bCloneTicket = true;
        this.bHardwareDetail = false;
        break;
      case "Hardware Details":
        this.AutoTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.CustomValuesTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextTicketGeneration.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabAutoDimension.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AutoDimensionEDrawingTab.Visibility = System.Windows.Visibility.Hidden;
        this.StyleTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBockAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.ApplicableTextAutoDimensionEDrawing.Visibility = System.Windows.Visibility.Hidden;
        this.CloneTicketTab.Visibility = System.Windows.Visibility.Hidden;
        this.SamePointToleranceTabCloneTicket.Visibility = System.Windows.Visibility.Hidden;
        this.HardwareDetailTab.Visibility = System.Windows.Visibility.Visible;
        this.StyleTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.AppendStringTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.ParameterDefinitionBlockHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.MinDimTabHardwareDetail.Visibility = System.Windows.Visibility.Hidden;
        this.SettingTicketGeneration.SelectedItem = (object) null;
        this.SettingAutoDimension.SelectedItem = (object) null;
        this.SettingAutoDimensionEDrawing.SelectedItem = (object) null;
        this.SettingCloneTicket.SelectedItem = (object) null;
        this.bTicketGenerationTabActive = false;
        this.bAutoDimensionTabActive = false;
        this.bEDrawingTabActive = false;
        this.bCloneTicket = false;
        this.bHardwareDetail = true;
        break;
    }
  }

  private void customTextBoxTicketGeneration_PreviewTextInput(
    object sender,
    TextCompositionEventArgs e)
  {
    if (!(e.Text == ","))
      return;
    e.Handled = true;
  }

  private void AppendStringTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
  {
    if (!(e.Text == ","))
      return;
    e.Handled = true;
  }

  private void AppendStringTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
  {
    if (e.FormatToApply == "Bitmap")
      e.CancelCommand();
    if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true) || !(e.SourceDataObject.GetData(DataFormats.UnicodeText) as string).Contains(","))
      return;
    e.CancelCommand();
  }

  private void customTextBoxTicketGeneration_PreviewExecuted(
    object sender,
    ExecutedRoutedEventArgs e)
  {
    if (e.Command != ApplicationCommands.Paste)
      return;
    e.Handled = true;
  }

  private void ChangeUnitsButtonTicketGeneration_Click(object sender, RoutedEventArgs e)
  {
    bool flag1 = false;
    bool flag2 = false;
    if (string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxTicketGeneration.Text) && string.IsNullOrWhiteSpace(this.MinDimInchTextBoxTicketGeneration.Text))
      flag1 = true;
    if (string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxAutoDimension.Text) && string.IsNullOrWhiteSpace(this.MinDimInchTextBoxAutoDimension.Text))
      flag2 = true;
    if (this.imperialUnits)
    {
      this.MinDimFeetLabelTicketGeneration.Text = "  mm";
      this.MinDimInchTextBoxTicketGeneration.Visibility = System.Windows.Visibility.Collapsed;
      this.MinDimInchLabelTicketGeneration.Visibility = System.Windows.Visibility.Collapsed;
      string str1 = !UnitConversion.roundToDecimalPointString(this.TicketGenerationFeetValue.ToString(), 4).Equals(this.MinDimFeetTextBoxTicketGeneration.Text) || !UnitConversion.roundToDecimalPointString(this.TicketGenerationInchValue.ToString(), 4).Equals(this.MinDimInchTextBoxTicketGeneration.Text) ? UnitConversion.feetAndInchesToMillimetersString(this.MinDimFeetTextBoxTicketGeneration.Text, this.MinDimInchTextBoxTicketGeneration.Text) : UnitConversion.feetAndInchesToMillimetersString(this.TicketGenerationFeetValue.ToString(), this.TicketGenerationInchValue.ToString());
      this.MinDimFeetTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(str1, 4);
      double.TryParse(str1, out this.TicketGenerationMetricValue);
      string str2 = !UnitConversion.roundToDecimalPointString(this.AutoDimensionFeetValue.ToString(), 4).Equals(this.MinDimFeetTextBoxAutoDimension.Text) || !UnitConversion.roundToDecimalPointString(this.AutoDimensionInchValue.ToString(), 4).Equals(this.MinDimInchTextBoxAutoDimension.Text) ? UnitConversion.feetAndInchesToMillimetersString(this.MinDimFeetTextBoxAutoDimension.Text, this.MinDimInchTextBoxAutoDimension.Text) : UnitConversion.feetAndInchesToMillimetersString(this.AutoDimensionFeetValue.ToString(), this.AutoDimensionInchValue.ToString());
      this.MinDimFeetTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(str2, 4);
      double.TryParse(str2, out this.AutoDimensionMetricValue);
      this.imperialUnits = !this.imperialUnits;
    }
    else
    {
      this.MinDimFeetLabelTicketGeneration.Text = "ft";
      this.MinDimInchTextBoxTicketGeneration.Visibility = System.Windows.Visibility.Visible;
      this.MinDimInchLabelTicketGeneration.Visibility = System.Windows.Visibility.Visible;
      string feetValueString;
      string inchValueString;
      if (UnitConversion.roundToDecimalPointString(this.TicketGenerationMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxTicketGeneration.Text.Trim()))
        UnitConversion.millimetersToFeetAndInchesString(this.TicketGenerationMetricValue.ToString(), out feetValueString, out inchValueString);
      else
        UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxTicketGeneration.Text, out feetValueString, out inchValueString);
      this.MinDimFeetTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      this.MinDimInchTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      double.TryParse(feetValueString, out this.TicketGenerationFeetValue);
      double.TryParse(inchValueString, out this.TicketGenerationInchValue);
      if (UnitConversion.roundToDecimalPointString(this.AutoDimensionMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxAutoDimension.Text.Trim()))
        UnitConversion.millimetersToFeetAndInchesString(this.AutoDimensionMetricValue.ToString(), out feetValueString, out inchValueString);
      else
        UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxAutoDimension.Text, out feetValueString, out inchValueString);
      this.MinDimFeetTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      this.MinDimInchTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      double.TryParse(feetValueString, out this.AutoDimensionFeetValue);
      double.TryParse(inchValueString, out this.AutoDimensionInchValue);
      this.imperialUnits = !this.imperialUnits;
    }
    if (flag1)
    {
      this.MinDimFeetTextBoxTicketGeneration.Text = "";
      this.MinDimInchTextBoxTicketGeneration.Text = "";
    }
    if (!flag2)
      return;
    this.MinDimFeetTextBoxAutoDimension.Text = "";
    this.MinDimInchTextBoxAutoDimension.Text = "";
  }

  private void ChangeUnitsButtonAutoDimension_Click(object sender, RoutedEventArgs e)
  {
    bool flag1 = false;
    bool flag2 = false;
    if (string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxTicketGeneration.Text) && string.IsNullOrWhiteSpace(this.MinDimInchTextBoxTicketGeneration.Text))
      flag1 = true;
    if (string.IsNullOrWhiteSpace(this.MinDimFeetTextBoxAutoDimension.Text) && string.IsNullOrWhiteSpace(this.MinDimInchTextBoxAutoDimension.Text))
      flag2 = true;
    if (this.imperialUnits)
    {
      this.MinDimFeetLabelAutoDimension.Text = "  mm";
      this.MinDimInchTextBoxAutoDimension.Visibility = System.Windows.Visibility.Collapsed;
      this.MinDimInchLabelAutoDimension.Visibility = System.Windows.Visibility.Collapsed;
      string str1 = !UnitConversion.roundToDecimalPointString(this.TicketGenerationFeetValue.ToString(), 4).Equals(this.MinDimFeetTextBoxTicketGeneration.Text) || !UnitConversion.roundToDecimalPointString(this.TicketGenerationInchValue.ToString().Trim(), 4).Equals(this.MinDimInchTextBoxTicketGeneration.Text) ? UnitConversion.feetAndInchesToMillimetersString(this.MinDimFeetTextBoxTicketGeneration.Text, this.MinDimInchTextBoxTicketGeneration.Text) : UnitConversion.feetAndInchesToMillimetersString(this.TicketGenerationFeetValue.ToString(), this.TicketGenerationInchValue.ToString());
      this.MinDimFeetTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(str1, 4);
      double.TryParse(str1, out this.TicketGenerationMetricValue);
      string str2 = !UnitConversion.roundToDecimalPointString(this.AutoDimensionFeetValue.ToString().Trim(), 4).Equals(this.MinDimFeetTextBoxAutoDimension.Text) || !UnitConversion.roundToDecimalPointString(this.AutoDimensionInchValue.ToString().Trim(), 4).Equals(this.MinDimInchTextBoxAutoDimension.Text) ? UnitConversion.feetAndInchesToMillimetersString(this.MinDimFeetTextBoxAutoDimension.Text, this.MinDimInchTextBoxAutoDimension.Text) : UnitConversion.feetAndInchesToMillimetersString(this.AutoDimensionFeetValue.ToString(), this.AutoDimensionInchValue.ToString());
      this.MinDimFeetTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(str2, 4);
      double.TryParse(str2, out this.AutoDimensionMetricValue);
      this.imperialUnits = !this.imperialUnits;
    }
    else
    {
      this.MinDimFeetLabelAutoDimension.Text = "ft";
      this.MinDimInchTextBoxAutoDimension.Visibility = System.Windows.Visibility.Visible;
      this.MinDimInchLabelAutoDimension.Visibility = System.Windows.Visibility.Visible;
      string feetValueString;
      string inchValueString;
      if (UnitConversion.roundToDecimalPointString(this.TicketGenerationMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxTicketGeneration.Text.Trim()))
        UnitConversion.millimetersToFeetAndInchesString(this.TicketGenerationMetricValue.ToString(), out feetValueString, out inchValueString);
      else
        UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxTicketGeneration.Text, out feetValueString, out inchValueString);
      this.MinDimFeetTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      this.MinDimInchTextBoxTicketGeneration.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      double.TryParse(feetValueString, out this.TicketGenerationFeetValue);
      double.TryParse(inchValueString, out this.TicketGenerationInchValue);
      if (UnitConversion.roundToDecimalPointString(this.AutoDimensionMetricValue.ToString(), 4).Equals(this.MinDimFeetTextBoxAutoDimension.Text.Trim()))
        UnitConversion.millimetersToFeetAndInchesString(this.AutoDimensionMetricValue.ToString(), out feetValueString, out inchValueString);
      else
        UnitConversion.millimetersToFeetAndInchesString(this.MinDimFeetTextBoxAutoDimension.Text, out feetValueString, out inchValueString);
      this.MinDimFeetTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      this.MinDimInchTextBoxAutoDimension.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      double.TryParse(feetValueString, out this.AutoDimensionFeetValue);
      double.TryParse(inchValueString, out this.AutoDimensionInchValue);
      this.imperialUnits = !this.imperialUnits;
    }
    if (flag1)
    {
      this.MinDimFeetTextBoxTicketGeneration.Text = "";
      this.MinDimInchTextBoxTicketGeneration.Text = "";
    }
    if (!flag2)
      return;
    this.MinDimFeetTextBoxAutoDimension.Text = "";
    this.MinDimInchTextBoxAutoDimension.Text = "";
  }

  private void ChangeUnitsButtonCloneTicket_Click(object sender, RoutedEventArgs e)
  {
  }

  private void ChangeUnitsButtonHardwareDetail_Click(object sender, RoutedEventArgs e)
  {
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/autoticketsettings.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Selector) target).SelectionChanged += new SelectionChangedEventHandler(this.TabPanel_SelectionChanged);
        break;
      case 2:
        this.GridSpliter = (GridSplitter) target;
        break;
      case 3:
        this.AutoTicketTab = (Border) target;
        break;
      case 4:
        this.SettingTicketGeneration = (ListBox) target;
        this.SettingTicketGeneration.SelectionChanged += new SelectionChangedEventHandler(this.Setting_SelectedTicketGeneration);
        break;
      case 5:
        this.StyleTabTicketGeneration = (System.Windows.Controls.Grid) target;
        break;
      case 6:
        this.CalloutStyleComboBoxTicketGeneration = (ComboBox) target;
        break;
      case 7:
        this.DimensionStyleComboBoxTicketGeneration = (ComboBox) target;
        break;
      case 8:
        this.OtherDimensionStyleComboBoxTicketGeneration = (ComboBox) target;
        break;
      case 9:
        this.CheckBoxEqualityFormula = (CheckBox) target;
        break;
      case 10:
        this.TextStyleComboBoxTicketGeneration = (ComboBox) target;
        break;
      case 11:
        this.AppendStringTabTicketGeneration = (System.Windows.Controls.Grid) target;
        break;
      case 12:
        this.AppendStringTextBoxTicketGeneration = (RichTextBox) target;
        this.AppendStringTextBoxTicketGeneration.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.AppendStringTextBoxTicketGeneration.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.AppendStringTextBoxTicketGeneration.TextChanged += new TextChangedEventHandler(this.AppendStringTextBoxTicketGeneration_TextChanged);
        break;
      case 13:
        this.ExampleResultTextBoxTicketGeneration = (TextBox) target;
        break;
      case 14:
        this.ParameterDefinitionBlockTicketGeneration = (System.Windows.Controls.Grid) target;
        break;
      case 15:
        this.ParameterValuesListTicketGeneration = (ComboBox) target;
        this.ParameterValuesListTicketGeneration.SelectionChanged += new SelectionChangedEventHandler(this.ParameterValuesList_SelectionChanged);
        break;
      case 16 /*0x10*/:
        this.AddButtonTicketGeneration = (Button) target;
        this.AddButtonTicketGeneration.Click += new RoutedEventHandler(this.AddButton_Click);
        break;
      case 17:
        this.PrefixTextBoxTicketGeneration = (TextBox) target;
        this.PrefixTextBoxTicketGeneration.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.PrefixTextBoxTicketGeneration.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.PrefixTextBoxTicketGeneration.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 18:
        this.SuffixTextBoxTicketGeneration = (TextBox) target;
        this.SuffixTextBoxTicketGeneration.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.SuffixTextBoxTicketGeneration.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.SuffixTextBoxTicketGeneration.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 19:
        this.ResultTextBoxTicketGeneration = (TextBox) target;
        break;
      case 20:
        this.ApplicableTextTicketGeneration = (TextBlock) target;
        break;
      case 21:
        this.CustomValuesTabTicketGeneration = (System.Windows.Controls.Grid) target;
        break;
      case 22:
        this.overallTextBoxTicketGeneration = (TextBox) target;
        this.overallTextBoxTicketGeneration.AddHandler(CommandManager.PreviewExecutedEvent, (Delegate) new ExecutedRoutedEventHandler(this.customTextBoxTicketGeneration_PreviewExecuted));
        this.overallTextBoxTicketGeneration.PreviewTextInput += new TextCompositionEventHandler(this.customTextBoxTicketGeneration_PreviewTextInput);
        break;
      case 23:
        this.contourTextBoxTicketGeneration = (TextBox) target;
        this.contourTextBoxTicketGeneration.AddHandler(CommandManager.PreviewExecutedEvent, (Delegate) new ExecutedRoutedEventHandler(this.customTextBoxTicketGeneration_PreviewExecuted));
        this.contourTextBoxTicketGeneration.PreviewTextInput += new TextCompositionEventHandler(this.customTextBoxTicketGeneration_PreviewTextInput);
        break;
      case 24:
        this.blockoutTextBoxTicketGeneration = (TextBox) target;
        this.blockoutTextBoxTicketGeneration.AddHandler(CommandManager.PreviewExecutedEvent, (Delegate) new ExecutedRoutedEventHandler(this.customTextBoxTicketGeneration_PreviewExecuted));
        this.blockoutTextBoxTicketGeneration.PreviewTextInput += new TextCompositionEventHandler(this.customTextBoxTicketGeneration_PreviewTextInput);
        break;
      case 25:
        this.MinDimTabTicketGeneration = (System.Windows.Controls.Grid) target;
        break;
      case 26:
        this.MinDimFeetTextBoxTicketGeneration = (TextBox) target;
        break;
      case 27:
        this.MinDimFeetLabelTicketGeneration = (TextBlock) target;
        break;
      case 28:
        this.MinDimInchTextBoxTicketGeneration = (TextBox) target;
        break;
      case 29:
        this.MinDimInchLabelTicketGeneration = (TextBlock) target;
        break;
      case 30:
        this.ChangeUnitsButtonTicketGeneration = (Button) target;
        this.ChangeUnitsButtonTicketGeneration.Click += new RoutedEventHandler(this.ChangeUnitsButtonTicketGeneration_Click);
        break;
      case 31 /*0x1F*/:
        this.AutoDimensionTab = (Border) target;
        break;
      case 32 /*0x20*/:
        this.SettingAutoDimension = (ListBox) target;
        this.SettingAutoDimension.SelectionChanged += new SelectionChangedEventHandler(this.Setting_SelectedTicketGeneration);
        break;
      case 33:
        this.StyleTabAutoDimension = (System.Windows.Controls.Grid) target;
        break;
      case 34:
        this.DimensionStyleComboBoxAutoDimension = (ComboBox) target;
        break;
      case 35:
        this.TextStyleComboBoxAutoDimension = (ComboBox) target;
        break;
      case 36:
        this.AppendStringTabAutoDimension = (System.Windows.Controls.Grid) target;
        break;
      case 37:
        this.AppendStringTextBoxAutoDimension = (RichTextBox) target;
        this.AppendStringTextBoxAutoDimension.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.AppendStringTextBoxAutoDimension.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.AppendStringTextBoxAutoDimension.TextChanged += new TextChangedEventHandler(this.AppendStringTextBoxAutoDimension_TextChanged);
        break;
      case 38:
        this.ExampleResultTextBoxAutoDimension = (TextBox) target;
        break;
      case 39:
        this.ParameterDefinitionBockAutoDimension = (System.Windows.Controls.Grid) target;
        break;
      case 40:
        this.ParameterValuesListAutoDimension = (ComboBox) target;
        this.ParameterValuesListAutoDimension.SelectionChanged += new SelectionChangedEventHandler(this.ParameterValuesList_SelectionChanged);
        break;
      case 41:
        this.AddButtonAutoDimension = (Button) target;
        this.AddButtonAutoDimension.Click += new RoutedEventHandler(this.AddButton_Click);
        break;
      case 42:
        this.PrefixTextBoxAutoDimension = (TextBox) target;
        this.PrefixTextBoxAutoDimension.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.PrefixTextBoxAutoDimension.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.PrefixTextBoxAutoDimension.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 43:
        this.SuffixTextBoxAutoDimension = (TextBox) target;
        this.SuffixTextBoxAutoDimension.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.SuffixTextBoxAutoDimension.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.SuffixTextBoxAutoDimension.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 44:
        this.ResultTextBoxAutoDimension = (TextBox) target;
        break;
      case 45:
        this.ApplicableTextAutoDimension = (TextBlock) target;
        break;
      case 46:
        this.MinDimTabAutoDimension = (System.Windows.Controls.Grid) target;
        break;
      case 47:
        this.MinDimFeetTextBoxAutoDimension = (TextBox) target;
        break;
      case 48 /*0x30*/:
        this.MinDimFeetLabelAutoDimension = (TextBlock) target;
        break;
      case 49:
        this.MinDimInchTextBoxAutoDimension = (TextBox) target;
        break;
      case 50:
        this.MinDimInchLabelAutoDimension = (TextBlock) target;
        break;
      case 51:
        this.ChangeUnitsButtonAutoDimension = (Button) target;
        this.ChangeUnitsButtonAutoDimension.Click += new RoutedEventHandler(this.ChangeUnitsButtonAutoDimension_Click);
        break;
      case 52:
        this.AutoDimensionEDrawingTab = (Border) target;
        break;
      case 53:
        this.SettingAutoDimensionEDrawing = (ListBox) target;
        this.SettingAutoDimensionEDrawing.SelectionChanged += new SelectionChangedEventHandler(this.Setting_SelectedTicketGeneration);
        break;
      case 54:
        this.StyleTabAutoDimensionEDrawing = (System.Windows.Controls.Grid) target;
        break;
      case 55:
        this.DimensionStyleComboBoxAutoDimensionEDrawing = (ComboBox) target;
        break;
      case 56:
        this.TextStyleComboBoxAutoDimensionEDrawing = (ComboBox) target;
        break;
      case 57:
        this.AppendStringTabAutoDimensionEDrawing = (System.Windows.Controls.Grid) target;
        break;
      case 58:
        this.AppendStringTextBoxAutoDimensionEDrawing = (RichTextBox) target;
        this.AppendStringTextBoxAutoDimensionEDrawing.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.AppendStringTextBoxAutoDimensionEDrawing.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.AppendStringTextBoxAutoDimensionEDrawing.TextChanged += new TextChangedEventHandler(this.AppendStringTextBoxAutoDimensionEDrawing_TextChanged);
        break;
      case 59:
        this.ExampleResultTextBoxAutoDimensionEDrawing = (TextBox) target;
        break;
      case 60:
        this.ParameterDefinitionBockAutoDimensionEDrawing = (System.Windows.Controls.Grid) target;
        break;
      case 61:
        this.ParameterValuesListAutoDimensionEDrawing = (ComboBox) target;
        this.ParameterValuesListAutoDimensionEDrawing.SelectionChanged += new SelectionChangedEventHandler(this.ParameterValuesList_SelectionChanged);
        break;
      case 62:
        this.AddButtonAutoDimensionEDrawing = (Button) target;
        this.AddButtonAutoDimensionEDrawing.Click += new RoutedEventHandler(this.AddButton_Click);
        break;
      case 63 /*0x3F*/:
        this.PrefixTextBoxAutoDimensionEDrawing = (TextBox) target;
        this.PrefixTextBoxAutoDimensionEDrawing.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.PrefixTextBoxAutoDimensionEDrawing.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.PrefixTextBoxAutoDimensionEDrawing.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 64 /*0x40*/:
        this.SuffixTextBoxAutoDimensionEDrawing = (TextBox) target;
        this.SuffixTextBoxAutoDimensionEDrawing.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.SuffixTextBoxAutoDimensionEDrawing.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.SuffixTextBoxAutoDimensionEDrawing.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 65:
        this.ResultTextBoxAutoDimensionEDrawing = (TextBox) target;
        break;
      case 66:
        this.ApplicableTextAutoDimensionEDrawing = (TextBlock) target;
        break;
      case 67:
        this.CloneTicketTab = (Border) target;
        break;
      case 68:
        this.SettingCloneTicket = (ListBox) target;
        this.SettingCloneTicket.SelectionChanged += new SelectionChangedEventHandler(this.Setting_SelectedTicketGeneration);
        break;
      case 69:
        this.SamePointToleranceTabCloneTicket = (System.Windows.Controls.Grid) target;
        break;
      case 70:
        this.SamePointToleranceFeetTextBox = (TextBox) target;
        break;
      case 71:
        this.SamePointToleranceFeetLabel = (TextBlock) target;
        break;
      case 72:
        this.SamePointToleranceInchTextBox = (TextBox) target;
        break;
      case 73:
        this.SamePointToleranceInchLabel = (TextBlock) target;
        break;
      case 74:
        this.HardwareDetailTab = (Border) target;
        break;
      case 75:
        this.SettingHardwareDetail = (ListBox) target;
        this.SettingHardwareDetail.SelectionChanged += new SelectionChangedEventHandler(this.Setting_SelectedTicketGeneration);
        break;
      case 76:
        this.StyleTabHardwareDetail = (System.Windows.Controls.Grid) target;
        break;
      case 77:
        this.CalloutStyleComboBoxHardwareDetail = (ComboBox) target;
        break;
      case 78:
        this.DimensionStyleComboBoxHardwareDetail = (ComboBox) target;
        break;
      case 79:
        this.OtherDimensionStyleComboBoxHardwareDetail = (ComboBox) target;
        break;
      case 80 /*0x50*/:
        this.TextStyleComboBoxHardwareDetail = (ComboBox) target;
        break;
      case 81:
        this.CheckBoxEqualityFormulaHardwareDetail = (CheckBox) target;
        break;
      case 82:
        this.MinDimTabHardwareDetail = (System.Windows.Controls.Grid) target;
        break;
      case 83:
        this.MinDimFeetTextBoxHardwareDetail = (TextBox) target;
        break;
      case 84:
        this.MinDimFeetLabelHardwareDetail = (TextBlock) target;
        break;
      case 85:
        this.MinDimInchTextBoxHardwareDetail = (TextBox) target;
        break;
      case 86:
        this.MinDimInchLabelHardwareDetail = (TextBlock) target;
        break;
      case 87:
        this.AppendStringTabHardwareDetail = (System.Windows.Controls.Grid) target;
        break;
      case 88:
        this.AppendStringTextBoxHardwareDetail = (RichTextBox) target;
        this.AppendStringTextBoxHardwareDetail.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.AppendStringTextBoxHardwareDetail.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.AppendStringTextBoxHardwareDetail.TextChanged += new TextChangedEventHandler(this.AppendStringTextBoxHardwareDetail_TextChanged);
        break;
      case 89:
        this.ExampleResultTextBoxHardwareDetail = (TextBox) target;
        break;
      case 90:
        this.ParameterDefinitionBlockHardwareDetail = (System.Windows.Controls.Grid) target;
        break;
      case 91:
        this.ParameterValuesListHardwareDetail = (ComboBox) target;
        this.ParameterValuesListHardwareDetail.SelectionChanged += new SelectionChangedEventHandler(this.ParameterValuesList_SelectionChanged);
        break;
      case 92:
        this.AddButtonHardwareDetail = (Button) target;
        this.AddButtonHardwareDetail.Click += new RoutedEventHandler(this.AddButton_Click);
        break;
      case 93:
        this.PrefixTextBoxHardwareDetail = (TextBox) target;
        this.PrefixTextBoxHardwareDetail.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.PrefixTextBoxHardwareDetail.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.PrefixTextBoxHardwareDetail.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 94:
        this.SuffixTextBoxHardwareDetail = (TextBox) target;
        this.SuffixTextBoxHardwareDetail.AddHandler(DataObject.PastingEvent, (Delegate) new DataObjectPastingEventHandler(this.AppendStringTextBox_Pasting));
        this.SuffixTextBoxHardwareDetail.PreviewTextInput += new TextCompositionEventHandler(this.AppendStringTextBox_PreviewTextInput);
        this.SuffixTextBoxHardwareDetail.TextChanged += new TextChangedEventHandler(this.PrePostTextBox_TextChanged);
        break;
      case 95:
        this.ResultTextBoxHardwareDetail = (TextBox) target;
        break;
      case 96 /*0x60*/:
        this.ApplicableTextHardwareDetail = (TextBlock) target;
        break;
      case 97:
        this.okayButton = (Button) target;
        this.okayButton.Click += new RoutedEventHandler(this.okayButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
