// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.MarkVerificationSettingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using Utils.MiscUtils;
using Utils.SettingsUtils;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class MarkVerificationSettingWindow : Window, IComponentConnector
{
  private Document RevitDoc;
  private MarkVerificationSettings ListItems = new MarkVerificationSettings();
  private List<string> illegalChar;
  private string manufacturerName = "";
  private string path = "";
  private bool imperialUnits = true;
  private double feetValue1;
  private double inchValue1;
  private double metricValue1;
  private double feetValue2;
  private double inchValue2;
  private double metricValue2;
  private double feetDecValue1;
  private double metricDecValue1;
  private double feetDecValue2;
  private double metricDecValue2;
  private double feetDecValue4;
  private double metricDecValue4;
  private double feetDecValue5;
  private double metricDecValue5;
  private bool DecimalOptionBool = true;
  private bool DecimalOption2Bool = true;
  private bool DecimalOption3Bool = true;
  private bool DecimalOption4Bool = true;
  private bool DecimalOption5Bool = true;
  private bool DecimalOption6Bool = true;
  private bool FracOptionBool = true;
  private bool FracOption2Bool = true;
  private bool FracOption3Bool = true;
  private bool FracOption4Bool = true;
  private bool FracOptionInchBool = true;
  private bool FracOptionInch2Bool = true;
  private bool FracOptionInch3Bool = true;
  private bool FracOptionInch4Bool = true;
  internal System.Windows.Controls.ListBox Setting;
  internal Border GeneralSettingPanelBorder;
  internal System.Windows.Controls.Grid GeneralSettingPanel;
  internal RowDefinition Row1;
  internal System.Windows.Controls.RadioButton NumRadioButton;
  internal System.Windows.Controls.RadioButton AlphaRadioButton;
  internal System.Windows.Controls.TextBox NewProductName;
  internal System.Windows.Controls.TextBox IncrementorTxt;
  internal System.Windows.Controls.Button FakeBtn;
  internal System.Windows.Controls.Button AddNewAdmin;
  internal System.Windows.Controls.Button DeleteAdmin;
  internal System.Windows.Controls.DataGrid Mark_Prefix_List;
  internal Border StraightBarSettingPanelBorder;
  internal System.Windows.Controls.Grid StraightBarSettingPanel;
  internal TextBlock FractionToleranceLabel;
  internal System.Windows.Controls.TextBox FracOption;
  internal TextBlock FeetLabel;
  internal System.Windows.Controls.TextBox FracOptionInch;
  internal TextBlock InchLabel;
  internal System.Windows.Controls.TextBox FracOption2;
  internal TextBlock FeetLabel2;
  internal System.Windows.Controls.TextBox FracOption2Inch;
  internal TextBlock InchLabel2;
  internal TextBlock DecimalToleranceLabel;
  internal System.Windows.Controls.TextBox DecimalOption;
  internal TextBlock VolumeUnitLabel;
  internal System.Windows.Controls.TextBox DecimalOption2;
  internal TextBlock VolumeUnitLabel2;
  internal System.Windows.Controls.TextBox DecimalOption4;
  internal TextBlock AreaUnitLabel;
  internal System.Windows.Controls.TextBox DecimalOption5;
  internal TextBlock WeightUnitLabel;
  internal System.Windows.Controls.TextBox DecimalOption6;
  internal System.Windows.Controls.Button UnitChangeButton;
  private bool _contentLoaded;

  public MarkVerificationSettingWindow(
    Document revitDoc,
    IntPtr parentWindowHandler,
    out bool ifContinue)
  {
    ifContinue = true;
    this.RevitDoc = revitDoc;
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    if (Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(revitDoc))
      this.imperialUnits = false;
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
    if (revitDoc.ProjectInformation == null)
    {
      ifContinue = false;
      int num = (int) System.Windows.MessageBox.Show("Mark Prefix User Setting Dialog cannot be used in the family view, transaction will be cancelled.");
    }
    else
    {
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        this.manufacturerName = parameter.AsString();
      this.path = string.IsNullOrEmpty(App.MarkPrefixFolderPath) ? $"C:/EDGEforREVIT/{this.manufacturerName}_MARK_PREFIX.xml" : $"{App.MarkPrefixFolderPath}\\{this.manufacturerName}_MARK_PREFIX.xml";
      if (File.Exists(this.path))
      {
        try
        {
          using (new StreamReader(this.path))
          {
            if (this.ListItems.LoadMarkPrefixSettings(this.path))
              this.Mark_Prefix_List.ItemsSource = (IEnumerable) this.ListItems.PrefixList;
            if (!string.IsNullOrEmpty(this.ListItems.Incrementor))
              this.IncrementorTxt.Text = this.ListItems.Incrementor;
          }
          if (this.ListItems.AlphabeticNumbering)
            this.AlphaRadioButton.IsChecked = new bool?(true);
          else
            this.NumRadioButton.IsChecked = new bool?(true);
        }
        catch (IOException ex)
        {
          if (ex.Message.Contains("process"))
          {
            new TaskDialog("EDGE^R")
            {
              AllowCancellation = false,
              MainInstruction = "Mark Verification Settings File Error.",
              MainContent = $"Check Mark Verification Settings File: {this.manufacturerName}_MARK_PREFIX.xml . Please ensure the file is not in use by another application and try again."
            }.Show();
            ifContinue = false;
          }
        }
      }
      else
      {
        List<Utils.SettingsUtils.ItemEntry> itemEntryList = new List<Utils.SettingsUtils.ItemEntry>();
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("COLUMN", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("DOUBLE TEE", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("FLAT SLAB", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("LGIRDER", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("LITEWALL HORIZONTAL", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("LITEWALL VERTICAL", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("RBEAM", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("SHEARWALL HORIZONTAL", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("SPANDREL BEARING", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("SPANDREL NONBEARING", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("STAIR", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("TGIRDER", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("WALL COLUMN", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("WALL PANEL VERTICAL", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("WALL PANEL HORIZONTAL", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("WALL PANEL INSULATED NON-THERMAL", "", ""));
        itemEntryList.Add(new Utils.SettingsUtils.ItemEntry("WALL PANEL INSULATED TROUGH", "", ""));
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) itemEntryList;
        this.ListItems.PrefixList = itemEntryList;
        this.ListItems.AlphabeticNumbering = false;
        this.NumRadioButton.IsChecked = new bool?(true);
      }
      string path = App.MarkPrefixFolderPath;
      if (!Directory.Exists(path))
        path = "C:\\EDGEforRevit";
      string str1 = $"{path}\\{this.manufacturerName}_Mark_Tolerance.txt";
      if (File.Exists(str1))
      {
        try
        {
          using (StreamReader streamReader = new StreamReader(str1))
          {
            FileInfo fileInfo = new FileInfo(str1);
            string str2;
            while ((str2 = streamReader.ReadLine()) != null)
            {
              if (!string.IsNullOrWhiteSpace(str2))
              {
                string[] source = str2.Split(new string[1]
                {
                  ","
                }, StringSplitOptions.RemoveEmptyEntries);
                if (((IEnumerable<string>) source).Count<string>() == 2)
                {
                  string str3 = source[0];
                  if (str3 != null)
                  {
                    switch (str3.Length)
                    {
                      case 5:
                        if (str3 == "Angle")
                        {
                          this.DecimalOption6.Text = source[1];
                          continue;
                        }
                        continue;
                      case 6:
                        switch (str3[0])
                        {
                          case 'V':
                            if (str3 == "Volume" && !string.IsNullOrWhiteSpace(source[1]))
                            {
                              source[1] = this.checkFraction(source[1]);
                              this.DecimalOption.Text = UnitConversion.roundToDecimalPointString(source[1], 4);
                              double.TryParse(source[1], out this.feetDecValue1);
                              continue;
                            }
                            continue;
                          case 'W':
                            if (str3 == "Weight" && !string.IsNullOrWhiteSpace(source[1]))
                            {
                              source[1] = this.checkFraction(source[1]);
                              this.DecimalOption5.Text = UnitConversion.roundToDecimalPointString(source[1], 4);
                              double.TryParse(source[1], out this.feetDecValue5);
                              continue;
                            }
                            continue;
                          default:
                            continue;
                        }
                      case 8:
                        if (str3 == "Geometry")
                        {
                          string[] strArray = FeetAndInchesRounding.readFeetInchValue(source[1]);
                          if (!string.IsNullOrWhiteSpace(strArray[0]))
                          {
                            strArray[0] = this.checkFraction(strArray[0]);
                            this.FracOption.Text = UnitConversion.roundToDecimalPointString(strArray[0], 4);
                            double.TryParse(strArray[0], out this.feetValue1);
                          }
                          if (!string.IsNullOrWhiteSpace(strArray[1]))
                          {
                            strArray[1] = this.checkFraction(strArray[1]);
                            this.FracOptionInch.Text = UnitConversion.roundToDecimalPointString(strArray[1], 4);
                            double.TryParse(strArray[1], out this.inchValue1);
                            continue;
                          }
                          continue;
                        }
                        continue;
                      case 10:
                        if (str3 == "FinishArea" && !string.IsNullOrWhiteSpace(source[1]))
                        {
                          source[1] = this.checkFraction(source[1]);
                          this.DecimalOption4.Text = UnitConversion.roundToDecimalPointString(source[1], 4);
                          double.TryParse(source[1], out this.feetDecValue4);
                          continue;
                        }
                        continue;
                      case 11:
                        if (str3 == "AddonVolume" && !string.IsNullOrWhiteSpace(source[1]))
                        {
                          source[1] = this.checkFraction(source[1]);
                          this.DecimalOption2.Text = UnitConversion.roundToDecimalPointString(source[1], 4);
                          double.TryParse(source[1], out this.feetDecValue2);
                          continue;
                        }
                        continue;
                      case 16 /*0x10*/:
                        if (str3 == "EmbeddedPosition")
                        {
                          string[] strArray = FeetAndInchesRounding.readFeetInchValue(source[1]);
                          if (!string.IsNullOrWhiteSpace(strArray[0]))
                          {
                            strArray[0] = this.checkFraction(strArray[0]);
                            this.FracOption2.Text = UnitConversion.roundToDecimalPointString(strArray[0], 4);
                            double.TryParse(strArray[0], out this.feetValue2);
                          }
                          if (!string.IsNullOrEmpty(strArray[1]))
                          {
                            strArray[1] = this.checkFraction(strArray[1]);
                            this.FracOption2Inch.Text = UnitConversion.roundToDecimalPointString(strArray[1], 4);
                            double.TryParse(strArray[1], out this.inchValue2);
                            continue;
                          }
                          continue;
                        }
                        continue;
                      default:
                        continue;
                    }
                  }
                }
              }
            }
            streamReader.Close();
          }
        }
        catch (IOException ex)
        {
          if (ex.Message.Contains("process"))
          {
            new TaskDialog("EDGE^R")
            {
              AllowCancellation = false,
              MainInstruction = "Mark Verification Settings File Error.",
              MainContent = $"Check Mark Verification Settings File: {this.manufacturerName}_Mark_Tolerance.txt . Please ensure the file is not in use by another application and try again."
            }.Show();
            ifContinue = false;
          }
        }
      }
      if (!this.imperialUnits)
        this.switchToMetricView();
      this.UnitChangeButton.Visibility = System.Windows.Visibility.Hidden;
    }
  }

  private int ToFractionRoundingIndex(string str)
  {
    if (str != null)
    {
      switch (str.Length)
      {
        case 4:
          if (str == "Feet")
            return 0;
          break;
        case 6:
          if (str == "Inches")
            return 1;
          break;
        case 9:
          if (str == "Half Inch")
            return 2;
          break;
        case 11:
          switch (str[0])
          {
            case 'E':
              if (str == "Eighth Inch")
                return 4;
              break;
            case 'Q':
              if (str == "Quater Inch")
                return 3;
              break;
          }
          break;
        case 14:
          if (str == "Sixteenth Inch")
            return 5;
          break;
        case 16 /*0x10*/:
          if (str == "SixtyFourth Inch")
            return 7;
          break;
        case 17:
          if (str == "ThirtySecond Inch")
            return 6;
          break;
        case 20:
          if (str == "OneTwentyEighth Inch")
            return 8;
          break;
      }
    }
    return 0;
  }

  private void Setting_Selected(object sender, RoutedEventArgs e)
  {
    string str1 = this.Setting.SelectedItem.ToString();
    string str2 = "";
    if (str1.Contains("Mark Setting"))
      str2 = "set1";
    else if (str1.Contains("Mark Tolerance Setting"))
      str2 = "set2";
    switch (str2)
    {
      case "set1":
        this.GeneralSettingPanel.Visibility = System.Windows.Visibility.Visible;
        this.GeneralSettingPanelBorder.Visibility = System.Windows.Visibility.Visible;
        this.StraightBarSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.StraightBarSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        break;
      case "set2":
        this.GeneralSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.GeneralSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        this.StraightBarSettingPanel.Visibility = System.Windows.Visibility.Visible;
        this.StraightBarSettingPanelBorder.Visibility = System.Windows.Visibility.Visible;
        break;
    }
  }

  private void MarkPrefix_PreviewTextInput(object sender, TextCompositionEventArgs e)
  {
    try
    {
      Convert.ToInt32(e.Text);
    }
    catch
    {
      e.Handled = true;
    }
  }

  private void ApplyButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (Utils.SettingsUtils.ItemEntry prefix in this.ListItems.PrefixList)
    {
      foreach (string str in this.illegalChar)
      {
        if (prefix.markPrefix.Contains(str))
        {
          int num = (int) System.Windows.Forms.MessageBox.Show("Please check your input. Your mark prefix list contains one or more following illegal characters <>;:[]{}\\|`~? ", "Warning");
          return;
        }
      }
    }
    try
    {
      this.ListItems.Incrementor = this.ListItems.AlphabeticNumbering ? string.Empty : this.IncrementorTxt.Text.Replace(" ", string.Empty);
      this.ListItems.SaveTemplateSettings(this.path);
    }
    catch (Exception ex)
    {
      int num = (int) System.Windows.Forms.MessageBox.Show(ex.ToString(), "warning");
    }
    bool flag = true;
    this.checkInput();
    if (!this.FracOptionBool || !this.FracOption2Bool || !this.FracOption3Bool || !this.FracOption4Bool || !this.FracOptionInchBool || !this.FracOptionInch2Bool || !this.FracOptionInch3Bool || !this.FracOptionInch4Bool)
      flag = false;
    if (!this.DecimalOptionBool || !this.DecimalOption2Bool || !this.DecimalOption3Bool || !this.DecimalOption4Bool || !this.DecimalOption5Bool || !this.DecimalOption6Bool)
      flag = false;
    if (flag)
    {
      string str1;
      string str2;
      string str3;
      string str4;
      string str5;
      string str6;
      if (this.imperialUnits)
      {
        str1 = !UnitConversion.roundToDecimalPointString(this.feetValue1.ToString(), 4).Equals(this.FracOption.Text) || !UnitConversion.roundToDecimalPointString(this.inchValue1.ToString(), 4).Equals(this.FracOptionInch.Text) ? this.addLegLength(this.FracOption.Text, this.FracOptionInch.Text) : this.addLegLength(this.feetValue1.ToString(), this.inchValue1.ToString());
        str2 = !UnitConversion.roundToDecimalPointString(this.feetValue2.ToString(), 4).Equals(this.FracOption2.Text) || !UnitConversion.roundToDecimalPointString(this.inchValue2.ToString(), 4).Equals(this.FracOption2Inch.Text) ? this.addLegLength(this.FracOption2.Text, this.FracOption2Inch.Text) : this.addLegLength(this.feetValue2.ToString(), this.inchValue2.ToString());
        str3 = !UnitConversion.roundToDecimalPointString(this.feetDecValue1.ToString(), 4).Equals(this.DecimalOption.Text) ? this.DecimalOption.Text : this.feetDecValue1.ToString();
        str4 = !UnitConversion.roundToDecimalPointString(this.feetDecValue2.ToString(), 4).Equals(this.DecimalOption2.Text) ? this.DecimalOption2.Text : this.feetDecValue2.ToString();
        str5 = !UnitConversion.roundToDecimalPointString(this.feetDecValue4.ToString(), 4).Equals(this.DecimalOption4.Text) ? this.DecimalOption4.Text : this.feetDecValue4.ToString();
        str6 = !UnitConversion.roundToDecimalPointString(this.feetDecValue5.ToString(), 4).Equals(this.DecimalOption5.Text) ? this.DecimalOption5.Text : this.feetDecValue5.ToString();
      }
      else
      {
        str1 = !UnitConversion.roundToDecimalPointString(this.metricValue1.ToString(), 4).Equals(this.FracOption.Text) ? this.metricAddLegLength(this.FracOption.Text) : this.metricAddLegLength(this.metricValue1.ToString());
        if (string.IsNullOrWhiteSpace(this.FracOption.Text))
          str1 = (string) null;
        str2 = !UnitConversion.roundToDecimalPointString(this.metricValue2.ToString(), 4).Equals(this.FracOption2.Text) ? this.metricAddLegLength(this.FracOption2.Text) : this.metricAddLegLength(this.metricValue2.ToString());
        if (string.IsNullOrWhiteSpace(this.FracOption2.Text))
          str2 = (string) null;
        str3 = !UnitConversion.roundToDecimalPointString(this.metricDecValue1.ToString(), 4).Equals(this.DecimalOption.Text) ? UnitConversion.cubicMillimetersToCubicFeetString(this.DecimalOption.Text) : UnitConversion.cubicMillimetersToCubicFeetString(this.metricDecValue1.ToString());
        if (string.IsNullOrWhiteSpace(this.DecimalOption.Text))
          str3 = (string) null;
        str4 = !UnitConversion.roundToDecimalPointString(this.metricDecValue2.ToString(), 4).Equals(this.DecimalOption2.Text) ? UnitConversion.cubicMillimetersToCubicFeetString(this.DecimalOption2.Text) : UnitConversion.cubicMillimetersToCubicFeetString(this.metricDecValue2.ToString());
        if (string.IsNullOrWhiteSpace(this.DecimalOption2.Text))
          str4 = (string) null;
        str5 = !UnitConversion.roundToDecimalPointString(this.metricDecValue4.ToString(), 4).Equals(this.DecimalOption4.Text) ? UnitConversion.squareMillimetersToSquareFeetString(this.DecimalOption4.Text) : UnitConversion.squareMillimetersToSquareFeetString(this.metricDecValue4.ToString());
        if (string.IsNullOrWhiteSpace(this.DecimalOption4.Text))
          str5 = (string) null;
        str6 = !UnitConversion.roundToDecimalPointString(this.metricDecValue5.ToString(), 4).Equals(this.DecimalOption5.Text) ? UnitConversion.kilogramsToPoundsString(this.DecimalOption5.Text) : UnitConversion.kilogramsToPoundsString(this.metricDecValue5.ToString());
        if (string.IsNullOrWhiteSpace(this.DecimalOption5.Text))
          str6 = (string) null;
      }
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("Geometry," + str1);
      stringBuilder.AppendLine("EmbeddedPosition," + str2);
      stringBuilder.AppendLine("Volume," + str3);
      stringBuilder.AppendLine("AddonVolume," + str4);
      stringBuilder.AppendLine("FinishArea," + str5);
      stringBuilder.AppendLine("Weight," + str6);
      stringBuilder.AppendLine("Angle," + this.DecimalOption6.Text);
      string path = App.MarkPrefixFolderPath;
      if (!Directory.Exists(path))
      {
        path = "C:\\EDGEforRevit";
        int num = (int) System.Windows.MessageBox.Show("The default mark setting folder path does not exist. The setting file will be saved to C:\\EDGEforRevit\\ folder instead.", "Warning");
      }
      if (!File.Exists($"{path}\\{this.manufacturerName}_Mark_Tolerance.txt"))
        File.Create($"{path}\\{this.manufacturerName}_Mark_Tolerance.txt").Close();
      string str7 = $"{path}\\{this.manufacturerName}_Mark_Tolerance.txt";
      string fileName = $"{path}\\{this.manufacturerName}_MARK_PREFIX.xml";
      FileInfo fileInfo1 = new FileInfo(str7);
      FileInfo fileInfo2 = new FileInfo(fileName);
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(str7))
          streamWriter.WriteLine(stringBuilder.ToString());
        this.Close();
      }
      catch (Exception ex)
      {
        if (fileInfo1.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
          new TaskDialog("EDGE^R")
          {
            AllowCancellation = false,
            MainInstruction = "Mark Verification Settings File is Read Only",
            MainContent = $"Unable to read the Mark Verification Settings File in the specified location {str7} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
          }.Show();
        else if (ex.Message.Contains("process"))
          new TaskDialog("EDGE^R")
          {
            AllowCancellation = false,
            MainInstruction = "Mark Verification Settings File Error.",
            MainContent = $"Check Mark Verification Settings File: {this.manufacturerName}_Mark_Tolerance.txt . Please ensure the file is not in use by another application and try again."
          }.Show();
        else
          this.Close();
      }
    }
    else
    {
      int num1 = (int) System.Windows.MessageBox.Show("Please check your input. Your Mark Tolerance Setting has an illegal input.");
    }
  }

  private string addLegLength(string feet, string inch)
  {
    string s1 = inch.TrimStart().TrimEnd();
    string s2 = feet.Trim();
    string str = "";
    if (s1.Contains("/") && s1.Contains(" ") || !s1.Contains("/"))
    {
      if (s1.Contains("/"))
      {
        int length = s1.IndexOf(" ");
        str = s1.Substring(length + 1);
        s1 = s1.Substring(0, length);
      }
      int result1;
      int.TryParse(s1, out result1);
      if (result1 >= 12)
      {
        int num1 = result1 % 12;
        int num2 = result1 / 12;
        s1 = !inch.Contains("/") ? num1.ToString() : $"{num1.ToString()} {str}";
        int result2;
        int.TryParse(s2, out result2);
        s2 = (result2 + num2).ToString();
      }
      else if (inch.Contains("/"))
        s1 = inch.TrimStart().TrimEnd();
    }
    if (string.IsNullOrWhiteSpace(s2) && string.IsNullOrWhiteSpace(s1))
      return " ";
    if (!string.IsNullOrWhiteSpace(s2) && !string.IsNullOrWhiteSpace(s1))
      return $"{s2.Trim()}' - {s1.TrimStart().TrimEnd()}\"";
    return string.IsNullOrWhiteSpace(s2) ? $"0' - {s1.TrimStart().TrimEnd()}\"" : s2.Trim() + "' - 0\"";
  }

  private string metricAddLegLength(string metricValueString)
  {
    string feetValueString;
    string inchValueString;
    UnitConversion.millimetersToFeetAndInchesString(metricValueString, out feetValueString, out inchValueString);
    return feetValueString == "0" && (inchValueString == "0." || inchValueString == "0") ? (string) null : this.addLegLength(feetValueString, inchValueString);
  }

  private void Mark_Prefix_List_SelectionChanged(object sender, RoutedEventArgs e)
  {
    this.DeleteAdmin.Visibility = System.Windows.Visibility.Visible;
  }

  private void AddNewButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (string str in this.illegalChar)
    {
      if (this.NewProductName.Text.Contains(str))
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("Please check your input. The new product name contains one or more following illegal characters <>;:[]{}\\|'~? ", "Warning");
        return;
      }
    }
    if (!this.NewProductName.Text.Equals(""))
    {
      bool flag = false;
      if (this.Mark_Prefix_List.ItemsSource != null)
      {
        foreach (Utils.SettingsUtils.ItemEntry itemEntry in this.Mark_Prefix_List.ItemsSource)
        {
          if (itemEntry.productName.Equals(this.NewProductName.Text))
            flag = true;
        }
      }
      if (flag)
      {
        int num = (int) System.Windows.Forms.MessageBox.Show("The product name you entered exists in the current list!", "Warning");
      }
      else
      {
        this.ListItems.PrefixList.Add(new Utils.SettingsUtils.ItemEntry(this.NewProductName.Text, "", ""));
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) null;
        this.Mark_Prefix_List.ItemsSource = (IEnumerable) this.ListItems.PrefixList;
      }
      this.NewProductName.Text = "";
    }
    this.DeleteAdmin.Visibility = System.Windows.Visibility.Hidden;
  }

  private void DeleteButton_Click(object sender, RoutedEventArgs e)
  {
    foreach (Utils.SettingsUtils.ItemEntry selectedItem in (IEnumerable) this.Mark_Prefix_List.SelectedItems)
      this.ListItems.PrefixList.Remove(selectedItem);
    this.Mark_Prefix_List.ItemsSource = (IEnumerable) null;
    this.Mark_Prefix_List.ItemsSource = (IEnumerable) this.ListItems.PrefixList;
  }

  private void DecimalOption_TextChanged(object sender, TextChangedEventArgs e)
  {
    System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
    if (string.IsNullOrWhiteSpace(textBox.Text))
    {
      textBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      this.setBooleanTrue(textBox.Name);
    }
    else if (double.TryParse(textBox.Text, out double _))
    {
      textBox.BorderBrush = (Brush) Brushes.Black;
      this.setBooleanTrue(textBox.Name);
    }
    else
    {
      textBox.BorderBrush = (Brush) Brushes.Red;
      this.setBooleanFalse(textBox.Name);
    }
  }

  private void setBooleanFalse(string textBoxName)
  {
    if (textBoxName == null)
      return;
    switch (textBoxName.Length)
    {
      case 10:
        if (!(textBoxName == "FracOption"))
          break;
        this.FracOptionBool = false;
        break;
      case 11:
        switch (textBoxName[10])
        {
          case '2':
            if (!(textBoxName == "FracOption2"))
              return;
            this.FracOption2Bool = false;
            return;
          case '3':
            if (!(textBoxName == "FracOption3"))
              return;
            this.FracOption3Bool = false;
            return;
          case '4':
            if (!(textBoxName == "FracOption4"))
              return;
            this.FracOption4Bool = false;
            return;
          default:
            return;
        }
      case 13:
        if (!(textBoxName == "DecimalOption"))
          break;
        this.DecimalOptionBool = false;
        break;
      case 14:
        switch (textBoxName[13])
        {
          case '2':
            if (!(textBoxName == "DecimalOption2"))
              return;
            this.DecimalOption2Bool = false;
            return;
          case '3':
            if (!(textBoxName == "DecimalOption3"))
              return;
            this.DecimalOption3Bool = false;
            return;
          case '4':
            if (!(textBoxName == "DecimalOption4"))
              return;
            this.DecimalOption4Bool = false;
            return;
          case '5':
            if (!(textBoxName == "DecimalOption5"))
              return;
            this.DecimalOption5Bool = false;
            return;
          case '6':
            if (!(textBoxName == "DecimalOption6"))
              return;
            this.DecimalOption6Bool = false;
            return;
          case 'h':
            if (!(textBoxName == "FracOptionInch"))
              return;
            this.FracOptionInchBool = false;
            return;
          default:
            return;
        }
      case 15:
        switch (textBoxName[14])
        {
          case '2':
            if (!(textBoxName == "FracOptionInch2"))
              return;
            this.FracOptionInch2Bool = false;
            return;
          case '3':
            if (!(textBoxName == "FracOptionInch3"))
              return;
            this.FracOptionInch3Bool = false;
            return;
          case '4':
            if (!(textBoxName == "FracOptionInch4"))
              return;
            this.FracOptionInch4Bool = false;
            return;
          default:
            return;
        }
    }
  }

  private void setBooleanTrue(string textBoxName)
  {
    if (textBoxName == null)
      return;
    switch (textBoxName.Length)
    {
      case 10:
        if (!(textBoxName == "FracOption"))
          break;
        this.FracOptionBool = true;
        break;
      case 11:
        switch (textBoxName[10])
        {
          case '2':
            if (!(textBoxName == "FracOption2"))
              return;
            this.FracOption2Bool = true;
            return;
          case '3':
            if (!(textBoxName == "FracOption3"))
              return;
            this.FracOption3Bool = true;
            return;
          case '4':
            if (!(textBoxName == "FracOption4"))
              return;
            this.FracOption4Bool = true;
            return;
          default:
            return;
        }
      case 13:
        if (!(textBoxName == "DecimalOption"))
          break;
        this.DecimalOptionBool = true;
        break;
      case 14:
        switch (textBoxName[13])
        {
          case '2':
            if (!(textBoxName == "DecimalOption2"))
              return;
            this.DecimalOption2Bool = true;
            return;
          case '3':
            if (!(textBoxName == "DecimalOption3"))
              return;
            this.DecimalOption3Bool = true;
            return;
          case '4':
            if (!(textBoxName == "DecimalOption4"))
              return;
            this.DecimalOption4Bool = true;
            return;
          case '5':
            if (!(textBoxName == "DecimalOption5"))
              return;
            this.DecimalOption5Bool = true;
            return;
          case '6':
            if (!(textBoxName == "DecimalOption6"))
              return;
            this.DecimalOption6Bool = true;
            return;
          case 'h':
            if (!(textBoxName == "FracOptionInch"))
              return;
            this.FracOptionInchBool = true;
            return;
          default:
            return;
        }
      case 15:
        switch (textBoxName[14])
        {
          case '2':
            if (!(textBoxName == "FracOptionInch2"))
              return;
            this.FracOptionInch2Bool = true;
            return;
          case '3':
            if (!(textBoxName == "FracOptionInch3"))
              return;
            this.FracOptionInch3Bool = true;
            return;
          case '4':
            if (!(textBoxName == "FracOptionInch4"))
              return;
            this.FracOptionInch4Bool = true;
            return;
          default:
            return;
        }
    }
  }

  private void FracOption_TextChanged(object sender, TextChangedEventArgs e)
  {
    System.Windows.Controls.TextBox txBox = sender as System.Windows.Controls.TextBox;
    if (this.checkFtInput(txBox))
      this.setBooleanTrue(txBox.Name);
    else
      this.setBooleanFalse(txBox.Name);
  }

  private void FracOptionInch_TextChanged(object sender, TextChangedEventArgs e)
  {
    System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
    if (string.IsNullOrWhiteSpace(textBox.Text))
    {
      textBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      this.setBooleanTrue(textBox.Name);
    }
    else if (new Regex("^(\\d+/\\d+|\\d+(\\s\\d+/\\d+)?|\\d*.\\d+|\\d+(.\\d+)?)$").Match(textBox.Text).Success)
    {
      string text = textBox.Text;
      if (text.ToString().IndexOf("/") >= text.Length - 1)
      {
        textBox.BorderBrush = (Brush) Brushes.Red;
        this.setBooleanFalse(textBox.Name);
      }
      else
      {
        textBox.BorderBrush = (Brush) Brushes.Black;
        this.setBooleanTrue(textBox.Name);
      }
    }
    else
    {
      textBox.BorderBrush = (Brush) Brushes.Red;
      this.setBooleanFalse(textBox.Name);
    }
  }

  private bool checkFtInput(System.Windows.Controls.TextBox txBox)
  {
    if (string.IsNullOrWhiteSpace(txBox.Text))
    {
      txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      return true;
    }
    if (double.TryParse(txBox.Text, out double _))
    {
      txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      return true;
    }
    txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
    return false;
  }

  private void UnitChangeButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.imperialUnits)
      this.switchToMetricView();
    else
      this.switchToImperialView();
  }

  private void switchToMetricView()
  {
    this.FracOptionInch.Visibility = System.Windows.Visibility.Collapsed;
    this.InchLabel.Visibility = System.Windows.Visibility.Collapsed;
    this.FracOption2Inch.Visibility = System.Windows.Visibility.Collapsed;
    this.InchLabel2.Visibility = System.Windows.Visibility.Collapsed;
    this.DecimalToleranceLabel.Visibility = System.Windows.Visibility.Collapsed;
    this.FeetLabel.Text = "mm";
    this.FeetLabel2.Text = "mm";
    this.VolumeUnitLabel.Text = "cubic millimeters";
    this.VolumeUnitLabel2.Text = "cubic millimeters";
    this.AreaUnitLabel.Text = "sq millimeters";
    this.WeightUnitLabel.Text = "kilograms";
    this.FractionToleranceLabel.Text = "Decimal Tolerance";
    if (!string.IsNullOrWhiteSpace(this.FracOption.Text) && !string.IsNullOrWhiteSpace(this.FracOptionInch.Text))
    {
      this.FracOption.Text = this.checkFraction(this.FracOption.Text);
      this.FracOptionInch.Text = this.checkFraction(this.FracOptionInch.Text);
      string str = !UnitConversion.roundToDecimalPointString(this.feetValue1.ToString(), 4).Equals(this.FracOption.Text) || !UnitConversion.roundToDecimalPointString(this.inchValue1.ToString(), 4).Equals(this.FracOptionInch.Text) ? UnitConversion.feetAndInchesToMillimetersString(this.FracOption.Text, this.FracOptionInch.Text) : UnitConversion.feetAndInchesToMillimetersString(this.feetValue1.ToString(), this.inchValue1.ToString());
      this.FracOption.Text = UnitConversion.roundToDecimalPointString(str, 4);
      double.TryParse(str, out this.metricValue1);
    }
    if (!string.IsNullOrWhiteSpace(this.FracOption2.Text) && !string.IsNullOrWhiteSpace(this.FracOption2Inch.Text))
    {
      this.FracOption2.Text = this.checkFraction(this.FracOption2.Text);
      this.FracOption2Inch.Text = this.checkFraction(this.FracOption2Inch.Text);
      string str = !UnitConversion.roundToDecimalPointString(this.feetValue2.ToString(), 4).Equals(this.FracOption2.Text) || !UnitConversion.roundToDecimalPointString(this.inchValue2.ToString(), 4).Equals(this.FracOption2Inch.Text) ? UnitConversion.feetAndInchesToMillimetersString(this.FracOption2.Text, this.FracOption2Inch.Text) : UnitConversion.feetAndInchesToMillimetersString(this.feetValue2.ToString(), this.inchValue2.ToString());
      this.FracOption2.Text = UnitConversion.roundToDecimalPointString(str, 4);
      double.TryParse(str, out this.metricValue2);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption.Text))
    {
      this.DecimalOption.Text = this.checkFraction(this.DecimalOption.Text);
      string str = !UnitConversion.roundToDecimalPointString(this.feetDecValue1.ToString(), 4).Equals(this.DecimalOption.Text) ? UnitConversion.cubicFeetStringToCubicMillimetersString(this.DecimalOption.Text) : UnitConversion.cubicFeetStringToCubicMillimetersString(this.feetDecValue1.ToString());
      this.DecimalOption.Text = UnitConversion.roundToDecimalPointString(str, 4);
      double.TryParse(str, out this.metricDecValue1);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption2.Text))
    {
      this.DecimalOption2.Text = this.checkFraction(this.DecimalOption2.Text);
      string str = !UnitConversion.roundToDecimalPointString(this.feetDecValue2.ToString(), 4).Equals(this.DecimalOption2.Text) ? UnitConversion.cubicFeetStringToCubicMillimetersString(this.DecimalOption2.Text) : UnitConversion.cubicFeetStringToCubicMillimetersString(this.feetDecValue2.ToString());
      this.DecimalOption2.Text = UnitConversion.roundToDecimalPointString(str, 4);
      double.TryParse(str, out this.metricDecValue2);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption4.Text))
    {
      this.DecimalOption4.Text = this.checkFraction(this.DecimalOption4.Text);
      string str = !UnitConversion.roundToDecimalPointString(this.feetDecValue4.ToString(), 4).Equals(this.DecimalOption4.Text) ? UnitConversion.squareFeetToSquareMillimetersString(this.DecimalOption4.Text) : UnitConversion.squareFeetToSquareMillimetersString(this.feetDecValue4.ToString());
      this.DecimalOption4.Text = UnitConversion.roundToDecimalPointString(str, 4);
      double.TryParse(str, out this.metricDecValue4);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption5.Text))
    {
      this.DecimalOption5.Text = this.checkFraction(this.DecimalOption5.Text);
      string str = !UnitConversion.roundToDecimalPointString(this.feetDecValue5.ToString(), 4).Equals(this.DecimalOption5.Text) ? UnitConversion.poundsToKilogramsString(this.DecimalOption5.Text) : UnitConversion.poundsToKilogramsString(this.feetDecValue5.ToString());
      this.DecimalOption5.Text = UnitConversion.roundToDecimalPointString(str, 4);
      double.TryParse(str, out this.metricDecValue5);
    }
    this.imperialUnits = false;
  }

  private void switchToImperialView()
  {
    this.FracOptionInch.Visibility = System.Windows.Visibility.Visible;
    this.InchLabel.Visibility = System.Windows.Visibility.Visible;
    this.FracOption2Inch.Visibility = System.Windows.Visibility.Visible;
    this.InchLabel2.Visibility = System.Windows.Visibility.Visible;
    this.DecimalToleranceLabel.Visibility = System.Windows.Visibility.Visible;
    this.FeetLabel.Text = "ft";
    this.FeetLabel2.Text = "ft";
    this.VolumeUnitLabel.Text = "cubic feet";
    this.VolumeUnitLabel2.Text = "cubic feet";
    this.AreaUnitLabel.Text = "sq feet";
    this.WeightUnitLabel.Text = "pounds";
    this.FractionToleranceLabel.Text = "Fraction Tolerance";
    string feetValueString;
    string inchValueString;
    if (!string.IsNullOrWhiteSpace(this.FracOption.Text))
    {
      this.FracOption.Text = this.checkFraction(this.FracOption.Text);
      if (UnitConversion.roundToDecimalPointString(this.metricValue1.ToString(), 4).Equals(this.FracOption.Text))
      {
        UnitConversion.millimetersToFeetAndInchesString(this.metricValue1.ToString(), out feetValueString, out inchValueString);
        this.FracOption.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
        this.FracOptionInch.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      }
      else
      {
        UnitConversion.millimetersToFeetAndInchesString(this.FracOption.Text, out feetValueString, out inchValueString);
        this.FracOption.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
        this.FracOptionInch.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      }
      double.TryParse(feetValueString, out this.feetValue1);
      double.TryParse(inchValueString, out this.inchValue1);
    }
    if (!string.IsNullOrWhiteSpace(this.FracOption2.Text))
    {
      this.FracOption2.Text = this.checkFraction(this.FracOption2.Text);
      if (UnitConversion.roundToDecimalPointString(this.metricValue2.ToString(), 4).Equals(this.FracOption2.Text))
      {
        UnitConversion.millimetersToFeetAndInchesString(this.metricValue2.ToString(), out feetValueString, out inchValueString);
        this.FracOption2.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
        this.FracOption2Inch.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      }
      else
      {
        UnitConversion.millimetersToFeetAndInchesString(this.FracOption2.Text, out feetValueString, out inchValueString);
        this.FracOption2.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
        this.FracOption2Inch.Text = UnitConversion.roundToDecimalPointString(inchValueString, 4);
      }
      double.TryParse(feetValueString, out this.feetValue2);
      double.TryParse(inchValueString, out this.inchValue2);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption.Text))
    {
      this.DecimalOption.Text = this.checkFraction(this.DecimalOption.Text);
      feetValueString = !UnitConversion.roundToDecimalPointString(this.metricDecValue1.ToString(), 4).Equals(this.DecimalOption.Text) ? UnitConversion.cubicMillimetersToCubicFeetString(this.DecimalOption.Text) : UnitConversion.cubicMillimetersToCubicFeetString(this.metricDecValue1.ToString());
      this.DecimalOption.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      double.TryParse(feetValueString, out this.feetDecValue1);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption2.Text))
    {
      this.DecimalOption2.Text = this.checkFraction(this.DecimalOption2.Text);
      feetValueString = !UnitConversion.roundToDecimalPointString(this.metricDecValue2.ToString(), 4).Equals(this.DecimalOption2.Text) ? UnitConversion.cubicMillimetersToCubicFeetString(this.DecimalOption2.Text) : UnitConversion.cubicMillimetersToCubicFeetString(this.metricDecValue2.ToString());
      this.DecimalOption2.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      double.TryParse(feetValueString, out this.feetDecValue2);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption4.Text))
    {
      this.DecimalOption4.Text = this.checkFraction(this.DecimalOption4.Text);
      feetValueString = !UnitConversion.roundToDecimalPointString(this.metricDecValue4.ToString(), 4).Equals(this.DecimalOption4.Text) ? UnitConversion.squareMillimetersToSquareFeetString(this.DecimalOption4.Text) : UnitConversion.squareMillimetersToSquareFeetString(this.metricDecValue4.ToString());
      this.DecimalOption4.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      double.TryParse(feetValueString, out this.feetDecValue4);
    }
    if (!string.IsNullOrWhiteSpace(this.DecimalOption5.Text))
    {
      this.DecimalOption5.Text = this.checkFraction(this.DecimalOption5.Text);
      feetValueString = !UnitConversion.roundToDecimalPointString(this.metricDecValue5.ToString(), 4).Equals(this.DecimalOption5.Text) ? UnitConversion.kilogramsToPoundsString(this.DecimalOption5.Text) : UnitConversion.kilogramsToPoundsString(this.metricDecValue5.ToString());
      this.DecimalOption5.Text = UnitConversion.roundToDecimalPointString(feetValueString, 4);
      double.TryParse(feetValueString, out this.feetDecValue5);
    }
    this.imperialUnits = true;
  }

  private void checkInput()
  {
    this.checkFraction(this.FracOption.Text);
    this.checkFraction(this.FracOptionInch.Text);
    this.checkFraction(this.FracOption2.Text);
    this.checkFraction(this.FracOption2Inch.Text);
    this.checkFraction(this.DecimalOption.Text);
    this.checkFraction(this.DecimalOption2.Text);
    this.checkFraction(this.DecimalOption4.Text);
    this.checkFraction(this.DecimalOption5.Text);
    double result;
    if (this.imperialUnits)
    {
      if (UnitConversion.roundToDecimalPointString(this.feetValue1.ToString(), 4).Equals(this.FracOption.Text) && UnitConversion.roundToDecimalPointString(this.inchValue1.ToString(), 4).Equals(this.FracOptionInch.Text))
      {
        if (this.feetValue1 < 0.0 || this.inchValue1 < 0.0)
          this.setBooleanFalse("FracOption");
      }
      else
      {
        double.TryParse(this.FracOption.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("FracOption");
        double.TryParse(this.FracOptionInch.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("FracOptionInch");
      }
      if (UnitConversion.roundToDecimalPointString(this.feetValue2.ToString(), 4).Equals(this.FracOption2.Text) && UnitConversion.roundToDecimalPointString(this.inchValue2.ToString(), 4).Equals(this.FracOption2Inch.Text))
      {
        if (this.feetValue2 < 0.0 || this.inchValue2 < 0.0)
          this.setBooleanFalse("FracOption2");
      }
      else
      {
        double.TryParse(this.FracOption2.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("FracOption2");
        double.TryParse(this.FracOption2Inch.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("FracOptionInch2");
      }
      if (UnitConversion.roundToDecimalPointString(this.feetDecValue1.ToString(), 4).Equals(this.DecimalOption.Text))
      {
        if (this.feetDecValue1 < 0.0)
          this.setBooleanFalse("DecimalOption");
      }
      else
      {
        double.TryParse(this.DecimalOption.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption");
      }
      if (UnitConversion.roundToDecimalPointString(this.feetDecValue2.ToString(), 4).Equals(this.DecimalOption2.Text))
      {
        if (this.feetDecValue2 < 0.0)
          this.setBooleanFalse("DecimalOption2");
      }
      else
      {
        double.TryParse(this.DecimalOption2.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption2");
      }
      if (UnitConversion.roundToDecimalPointString(this.feetDecValue4.ToString(), 4).Equals(this.DecimalOption4.Text))
      {
        if (this.feetDecValue4 < 0.0)
          this.setBooleanFalse("DecimalOption4");
      }
      else
      {
        double.TryParse(this.DecimalOption4.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption4");
      }
      if (UnitConversion.roundToDecimalPointString(this.feetDecValue5.ToString(), 4).Equals(this.DecimalOption5.Text))
      {
        if (this.feetDecValue5 < 0.0)
          this.setBooleanFalse("DecimalOption5");
      }
      else
      {
        double.TryParse(this.DecimalOption5.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption5");
      }
    }
    else
    {
      if (UnitConversion.roundToDecimalPointString(this.metricValue1.ToString(), 4).Equals(this.FracOption.Text))
      {
        if (this.metricValue1 < 0.0)
          this.setBooleanFalse("FracOption");
      }
      else
      {
        double.TryParse(this.FracOption.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("FracOption");
      }
      if (UnitConversion.roundToDecimalPointString(this.metricValue2.ToString(), 4).Equals(this.FracOption2.Text))
      {
        if (this.feetValue2 < 0.0)
          this.setBooleanFalse("FracOption2");
      }
      else
      {
        double.TryParse(this.FracOption2.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("FracOption2");
      }
      if (UnitConversion.roundToDecimalPointString(this.metricDecValue1.ToString(), 4).Equals(this.DecimalOption.Text))
      {
        if (this.metricDecValue1 < 0.0)
          this.setBooleanFalse("DecimalOption");
      }
      else
      {
        double.TryParse(this.DecimalOption.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption");
      }
      if (UnitConversion.roundToDecimalPointString(this.metricDecValue2.ToString(), 4).Equals(this.DecimalOption2.Text))
      {
        if (this.metricDecValue2 < 0.0)
          this.setBooleanFalse("DecimalOption2");
      }
      else
      {
        double.TryParse(this.DecimalOption2.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption2");
      }
      if (UnitConversion.roundToDecimalPointString(this.metricDecValue4.ToString(), 4).Equals(this.DecimalOption4.Text))
      {
        if (this.metricDecValue4 < 0.0)
          this.setBooleanFalse("DecimalOption4");
      }
      else
      {
        double.TryParse(this.DecimalOption4.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption4");
      }
      if (UnitConversion.roundToDecimalPointString(this.metricDecValue5.ToString(), 4).Equals(this.DecimalOption5.Text))
      {
        if (this.metricDecValue5 < 0.0)
          this.setBooleanFalse("DecimalOption5");
      }
      else
      {
        double.TryParse(this.DecimalOption5.Text, out result);
        if (result < 0.0)
          this.setBooleanFalse("DecimalOption5");
      }
    }
    double.TryParse(this.DecimalOption6.Text, out result);
    if (result >= 0.0 && result <= 360.0)
      return;
    this.setBooleanFalse("DecimalOption6");
  }

  private string checkFraction(string fracValue)
  {
    Regex regex1 = new Regex("^-?\\d+/\\d+$");
    Regex regex2 = new Regex("^-?\\d+\\s+-?\\d+/\\d+$");
    if (new Regex("^[-]\\d+\\s+-?\\d+/\\d+$").IsMatch(fracValue))
    {
      this.setBooleanFalse("FracOption");
      return "";
    }
    if (regex1.IsMatch(fracValue))
      return FeetAndInchesRounding.FractionToDouble(fracValue).ToString();
    if (regex2.IsMatch(fracValue))
    {
      string[] strArray1 = fracValue.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray1[0];
      string str1 = strArray1[1];
      string str2 = "";
      double result1;
      double.TryParse(s, out result1);
      string[] separator = new string[1]{ "/" };
      string[] strArray2 = str1.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray2.Length != 2)
      {
        this.setBooleanFalse("FracOption");
        str2 = "";
      }
      double result2;
      double result3;
      if (double.TryParse(strArray2[0], out result2) && double.TryParse(strArray2[1], out result3))
      {
        if (result3 == 0.0)
        {
          str2 = "Invalid Input";
        }
        else
        {
          double num1 = result2 / result3;
          double num2 = !this.imperialUnits ? num1 / 12.0 : num1;
          str2 = (result1 + num2).ToString();
        }
      }
      return str2;
    }
    if (!fracValue.Contains("/"))
      return fracValue;
    this.setBooleanFalse("FracOption");
    return "";
  }

  private void NumRadioButton_Checked(object sender, RoutedEventArgs e)
  {
    this.Row1.Height = GridLength.Auto;
    if (this.ListItems == null)
      return;
    this.ListItems.AlphabeticNumbering = false;
  }

  private void AlphaRadioButton_Checked(object sender, RoutedEventArgs e)
  {
    this.Row1.Height = new GridLength(0.0);
    if (this.ListItems == null)
      return;
    this.ListItems.AlphabeticNumbering = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/markverificationsettingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.Setting = (System.Windows.Controls.ListBox) target;
        this.Setting.SelectionChanged += new SelectionChangedEventHandler(this.Setting_Selected);
        break;
      case 2:
        ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.ApplyButton_Click);
        break;
      case 3:
        this.GeneralSettingPanelBorder = (Border) target;
        break;
      case 4:
        this.GeneralSettingPanel = (System.Windows.Controls.Grid) target;
        break;
      case 5:
        this.Row1 = (RowDefinition) target;
        break;
      case 6:
        this.NumRadioButton = (System.Windows.Controls.RadioButton) target;
        this.NumRadioButton.Checked += new RoutedEventHandler(this.NumRadioButton_Checked);
        break;
      case 7:
        this.AlphaRadioButton = (System.Windows.Controls.RadioButton) target;
        this.AlphaRadioButton.Checked += new RoutedEventHandler(this.AlphaRadioButton_Checked);
        break;
      case 8:
        this.NewProductName = (System.Windows.Controls.TextBox) target;
        break;
      case 9:
        this.IncrementorTxt = (System.Windows.Controls.TextBox) target;
        this.IncrementorTxt.PreviewTextInput += new TextCompositionEventHandler(this.MarkPrefix_PreviewTextInput);
        break;
      case 10:
        this.FakeBtn = (System.Windows.Controls.Button) target;
        break;
      case 11:
        this.AddNewAdmin = (System.Windows.Controls.Button) target;
        this.AddNewAdmin.Click += new RoutedEventHandler(this.AddNewButton_Click);
        break;
      case 12:
        this.DeleteAdmin = (System.Windows.Controls.Button) target;
        this.DeleteAdmin.Click += new RoutedEventHandler(this.DeleteButton_Click);
        break;
      case 13:
        this.Mark_Prefix_List = (System.Windows.Controls.DataGrid) target;
        this.Mark_Prefix_List.SelectionChanged += new SelectionChangedEventHandler(this.Mark_Prefix_List_SelectionChanged);
        break;
      case 14:
        this.StraightBarSettingPanelBorder = (Border) target;
        break;
      case 15:
        this.StraightBarSettingPanel = (System.Windows.Controls.Grid) target;
        break;
      case 16 /*0x10*/:
        this.FractionToleranceLabel = (TextBlock) target;
        break;
      case 17:
        this.FracOption = (System.Windows.Controls.TextBox) target;
        this.FracOption.TextChanged += new TextChangedEventHandler(this.FracOption_TextChanged);
        break;
      case 18:
        this.FeetLabel = (TextBlock) target;
        break;
      case 19:
        this.FracOptionInch = (System.Windows.Controls.TextBox) target;
        this.FracOptionInch.TextChanged += new TextChangedEventHandler(this.FracOptionInch_TextChanged);
        break;
      case 20:
        this.InchLabel = (TextBlock) target;
        break;
      case 21:
        this.FracOption2 = (System.Windows.Controls.TextBox) target;
        this.FracOption2.TextChanged += new TextChangedEventHandler(this.FracOption_TextChanged);
        break;
      case 22:
        this.FeetLabel2 = (TextBlock) target;
        break;
      case 23:
        this.FracOption2Inch = (System.Windows.Controls.TextBox) target;
        this.FracOption2Inch.TextChanged += new TextChangedEventHandler(this.FracOptionInch_TextChanged);
        break;
      case 24:
        this.InchLabel2 = (TextBlock) target;
        break;
      case 25:
        this.DecimalToleranceLabel = (TextBlock) target;
        break;
      case 26:
        this.DecimalOption = (System.Windows.Controls.TextBox) target;
        this.DecimalOption.TextChanged += new TextChangedEventHandler(this.DecimalOption_TextChanged);
        break;
      case 27:
        this.VolumeUnitLabel = (TextBlock) target;
        break;
      case 28:
        this.DecimalOption2 = (System.Windows.Controls.TextBox) target;
        this.DecimalOption2.TextChanged += new TextChangedEventHandler(this.DecimalOption_TextChanged);
        break;
      case 29:
        this.VolumeUnitLabel2 = (TextBlock) target;
        break;
      case 30:
        this.DecimalOption4 = (System.Windows.Controls.TextBox) target;
        this.DecimalOption4.TextChanged += new TextChangedEventHandler(this.DecimalOption_TextChanged);
        break;
      case 31 /*0x1F*/:
        this.AreaUnitLabel = (TextBlock) target;
        break;
      case 32 /*0x20*/:
        this.DecimalOption5 = (System.Windows.Controls.TextBox) target;
        this.DecimalOption5.TextChanged += new TextChangedEventHandler(this.DecimalOption_TextChanged);
        break;
      case 33:
        this.WeightUnitLabel = (TextBlock) target;
        break;
      case 34:
        this.DecimalOption6 = (System.Windows.Controls.TextBox) target;
        this.DecimalOption6.TextChanged += new TextChangedEventHandler(this.DecimalOption_TextChanged);
        break;
      case 35:
        this.UnitChangeButton = (System.Windows.Controls.Button) target;
        this.UnitChangeButton.Click += new RoutedEventHandler(this.UnitChangeButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
