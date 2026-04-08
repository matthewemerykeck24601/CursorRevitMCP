// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.RebarUserSettingWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.RebarTools;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class RebarUserSettingWindow : Window, IComponentConnector
{
  private Document RevitDoc;
  private static List<StandardRebarItem> _results = new List<StandardRebarItem>();
  private List<StandardRebarItem> standardBarList = new List<StandardRebarItem>();
  private List<StandardRebarItem> standardBarListNotUseMetric = new List<StandardRebarItem>();
  private string manufacturerName = "";
  private List<string> USBarSize = new List<string>();
  private List<string> MatrixBarSize = new List<string>();
  private bool straightBarVisible;
  private List<TextBox> FeetTextBoxes = new List<TextBox>();
  private List<TextBox> InchTextBoxes = new List<TextBox>();
  private List<TextBlock> FeetLabels = new List<TextBlock>();
  private List<TextBlock> InchLabels = new List<TextBlock>();
  private bool projectInMetric;
  private bool changeLengthComboBox;
  private ManufacturerKeying manufacturerKey;
  private RebarShapeSelectionWindow selectionWinodow;
  private bool padBarSizesWithLeadingZeroParam;
  private bool UseMatrix;
  private bool useMetricEuroBool;
  internal ListBox Setting;
  internal Border GeneralSettingPanelBorder;
  internal System.Windows.Controls.Grid GeneralSettingPanel;
  internal TextBox manufacturer;
  internal TextBlock UseMetricLabel;
  internal CheckBox useMetricCanadian;
  internal TextBox markPrefix;
  internal TextBox markSuffix;
  internal CheckBox padBarSizesWithLeadingZero;
  internal TextBox markPrefixIntMinValue;
  internal TextBox markFinishGalvanized;
  internal TextBox markFinishEpoxy;
  internal Border StraightBarSettingPanelBorder;
  internal System.Windows.Controls.Grid StraightBarSettingPanel;
  internal TextBox straightBarMarkPrefix;
  internal ComboBox straightBarLengthOption;
  internal TextBlock ToolTipLine1;
  internal TextBlock ToolTipLine2;
  internal TextBlock ToolTipLine3;
  internal TextBlock ToolTipLine4;
  internal TextBlock straightBarLengthZeroPaddingTextblock;
  internal TextBox straightBarLengthZeroPadding;
  internal Border StandardBarSettingPanelBorder;
  internal System.Windows.Controls.Grid StandardBarSettingPanel;
  internal Button DeleteItem;
  internal DataGrid existingSettingNotUseMetric;
  internal Border editingItemBorder;
  internal System.Windows.Controls.Grid editingItemPanelNotUseMatric;
  internal ComboBox barSizeBoxNotUseMetric;
  internal TextBox ShapTextBoxNotUseMetric;
  internal Button ShapeButtonNotUseMetric;
  internal TextBox MarkTextBoxNotUseMetric;
  internal TextBox ATextBoxFeet;
  internal TextBlock ALabelFeet;
  internal TextBox ATextBoxInch;
  internal TextBlock ALabelInch;
  internal TextBox BTextBoxFeet;
  internal TextBlock BLabelFeet;
  internal TextBox BTextBoxInch;
  internal TextBlock BLabelInch;
  internal TextBox CTextBoxFeet;
  internal TextBlock CLabelFeet;
  internal TextBox CTextBoxInch;
  internal TextBlock CLabelInch;
  internal TextBox DTextBoxFeet;
  internal TextBlock DLabelFeet;
  internal TextBox DTextBoxInch;
  internal TextBlock DLabelInch;
  internal TextBox ETextBoxFeet;
  internal TextBlock ELabelFeet;
  internal TextBox ETextBoxInch;
  internal TextBlock ELabelInch;
  internal TextBox FTextBoxFeet;
  internal TextBlock FLabelFeet;
  internal TextBox FTextBoxInch;
  internal TextBlock FLabelInch;
  internal TextBox GTextBoxFeet;
  internal TextBlock GLabelFeet;
  internal TextBox GTextBoxInch;
  internal TextBlock GLabelInch;
  internal TextBox HTextBoxFeet;
  internal TextBlock HLabelFeet;
  internal TextBox HTextBoxInch;
  internal TextBlock HLabelInch;
  internal TextBox JTextBoxFeet;
  internal TextBlock JLabelFeet;
  internal TextBox JTextBoxInch;
  internal TextBlock JLabelInch;
  internal TextBox KTextBoxFeet;
  internal TextBlock KLabelFeet;
  internal TextBox KTextBoxInch;
  internal TextBlock KLabelInch;
  internal TextBox OTextBoxFeet;
  internal TextBlock OLabelFeet;
  internal TextBox OTextBoxInch;
  internal TextBlock OLabelInch;
  private bool _contentLoaded;

  public RebarUserSettingWindow(Document revitDoc, IntPtr parentWindowHandler, out bool ifContinue)
  {
    ifContinue = true;
    this.RevitDoc = revitDoc;
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.FeetTextBoxes.Add(this.ATextBoxFeet);
    this.FeetTextBoxes.Add(this.BTextBoxFeet);
    this.FeetTextBoxes.Add(this.CTextBoxFeet);
    this.FeetTextBoxes.Add(this.DTextBoxFeet);
    this.FeetTextBoxes.Add(this.ETextBoxFeet);
    this.FeetTextBoxes.Add(this.FTextBoxFeet);
    this.FeetTextBoxes.Add(this.GTextBoxFeet);
    this.FeetTextBoxes.Add(this.HTextBoxFeet);
    this.FeetTextBoxes.Add(this.JTextBoxFeet);
    this.FeetTextBoxes.Add(this.KTextBoxFeet);
    this.FeetTextBoxes.Add(this.OTextBoxFeet);
    this.InchTextBoxes.Add(this.ATextBoxInch);
    this.InchTextBoxes.Add(this.BTextBoxInch);
    this.InchTextBoxes.Add(this.CTextBoxInch);
    this.InchTextBoxes.Add(this.DTextBoxInch);
    this.InchTextBoxes.Add(this.ETextBoxInch);
    this.InchTextBoxes.Add(this.FTextBoxInch);
    this.InchTextBoxes.Add(this.GTextBoxInch);
    this.InchTextBoxes.Add(this.HTextBoxInch);
    this.InchTextBoxes.Add(this.JTextBoxInch);
    this.InchTextBoxes.Add(this.KTextBoxInch);
    this.InchTextBoxes.Add(this.OTextBoxInch);
    this.FeetLabels.Add(this.ALabelFeet);
    this.FeetLabels.Add(this.BLabelFeet);
    this.FeetLabels.Add(this.CLabelFeet);
    this.FeetLabels.Add(this.DLabelFeet);
    this.FeetLabels.Add(this.ELabelFeet);
    this.FeetLabels.Add(this.FLabelFeet);
    this.FeetLabels.Add(this.GLabelFeet);
    this.FeetLabels.Add(this.HLabelFeet);
    this.FeetLabels.Add(this.JLabelFeet);
    this.FeetLabels.Add(this.KLabelFeet);
    this.FeetLabels.Add(this.OLabelFeet);
    this.InchLabels.Add(this.ALabelInch);
    this.InchLabels.Add(this.BLabelInch);
    this.InchLabels.Add(this.CLabelInch);
    this.InchLabels.Add(this.DLabelInch);
    this.InchLabels.Add(this.ELabelInch);
    this.InchLabels.Add(this.FLabelInch);
    this.InchLabels.Add(this.GLabelInch);
    this.InchLabels.Add(this.HLabelInch);
    this.InchLabels.Add(this.JLabelInch);
    this.InchLabels.Add(this.KLabelInch);
    this.InchLabels.Add(this.OLabelInch);
    this.USBarSize.Add("#3");
    this.USBarSize.Add("#4");
    this.USBarSize.Add("#5");
    this.USBarSize.Add("#6");
    this.USBarSize.Add("#7");
    this.USBarSize.Add("#8");
    this.USBarSize.Add("#9");
    this.USBarSize.Add("#10");
    this.USBarSize.Add("#11");
    this.MatrixBarSize.Add("#6");
    this.MatrixBarSize.Add("#8");
    this.MatrixBarSize.Add("#10");
    this.MatrixBarSize.Add("#12");
    this.MatrixBarSize.Add("#14");
    this.MatrixBarSize.Add("#16");
    this.MatrixBarSize.Add("#20");
    this.MatrixBarSize.Add("#25");
    this.MatrixBarSize.Add("#28");
    this.MatrixBarSize.Add("#32");
    this.MatrixBarSize.Add("#40");
    this.MatrixBarSize.Add("#50");
    RebarUserSettingWindow._results = new List<StandardRebarItem>();
    string settingsFolderPath = App.RebarSettingsFolderPath;
    if (revitDoc.ProjectInformation == null)
    {
      ifContinue = false;
      int num = (int) MessageBox.Show("Rebar Setting Dialog cannot be used in the family view, transaction will be cancelled.");
    }
    else
    {
      Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null && !string.IsNullOrEmpty(parameter.AsString()))
        this.manufacturerName = parameter.AsString();
      if (this.manufacturerName.ToUpper() == "ROCKY MOUNTAIN PRESTRESS")
        this.manufacturerKey = ManufacturerKeying.Rocky;
      else if (this.manufacturerName.ToUpper() == "KERKSTRA")
        this.manufacturerKey = ManufacturerKeying.Kerkstra;
      else if (this.manufacturerName.ToUpper() == "WELLS")
        this.manufacturerKey = ManufacturerKeying.Wells;
      else if (this.manufacturerName.ToUpper() == "CORESLAB ONT")
        this.manufacturerKey = ManufacturerKeying.CoreslabONT;
      else if (this.manufacturerName.ToUpper() == "MIDSTATES")
        this.manufacturerKey = ManufacturerKeying.MidStates;
      else if (this.manufacturerName.ToUpper() == "ILLINI")
        this.manufacturerKey = ManufacturerKeying.Illini;
      else if (this.manufacturerName.ToUpper() == "SPANCRETE")
        this.manufacturerKey = ManufacturerKeying.Spancrete;
      else if (this.manufacturerName.ToUpper() == "TINDALL")
      {
        this.manufacturerKey = ManufacturerKeying.Tindall;
      }
      else
      {
        this.manufacturerKey = ManufacturerKeying.Standard;
        if (Utils.MiscUtils.MiscUtils.CheckMetricLengthUnit(revitDoc))
        {
          this.projectInMetric = true;
          this.useMetricCanadian.IsChecked = new bool?(true);
          this.useMetricCanadian.Visibility = System.Windows.Visibility.Collapsed;
          this.UseMetricLabel.Visibility = System.Windows.Visibility.Collapsed;
        }
        else
          this.useMetricCanadian.IsChecked = new bool?(false);
      }
      this.manufacturer.Text = this.manufacturerName;
      string path = !File.Exists($"{settingsFolderPath}\\{this.manufacturerName}.txt") ? $"C:\\EDGEforRevit\\{this.manufacturerName}.txt" : $"{settingsFolderPath}\\{this.manufacturerName}.txt";
      if (File.Exists(path))
      {
        try
        {
          using (StreamReader streamReader = new StreamReader(path))
          {
            string str1;
            while ((str1 = streamReader.ReadLine()) != null)
            {
              if (!string.IsNullOrWhiteSpace(str1))
              {
                string[] strArray1 = str1.Split(new string[1]
                {
                  ","
                }, StringSplitOptions.RemoveEmptyEntries);
                if (strArray1.Length >= 2 && !string.IsNullOrWhiteSpace(strArray1[1]) && !strArray1[1].Equals("null"))
                {
                  string str2 = strArray1[0];
                  if (str2 != null)
                  {
                    switch (str2.Length)
                    {
                      case 7:
                        if (str2 == "*rebar*")
                        {
                          string[] strArray2 = str1.Split(",".ToCharArray());
                          string diameter = strArray2[1];
                          string shape = strArray2[2];
                          string mark = strArray2[3];
                          string a = strArray2[4];
                          string b = strArray2[5];
                          string c = strArray2[6];
                          string d = strArray2[7];
                          string e = strArray2[8];
                          string f = strArray2[9];
                          string g = strArray2[10];
                          string h = strArray2[11];
                          string j = strArray2[12];
                          string k = strArray2[13];
                          string o = strArray2[14];
                          RebarUserSettingWindow._results.Add(new StandardRebarItem(diameter, shape, mark, a, b, c, d, e, f, g, h, j, k, o));
                          continue;
                        }
                        continue;
                      case 11:
                        if (str2 == "*useMetric*" && strArray1[1].Equals("TRUE"))
                        {
                          this.useMetricCanadian.IsChecked = new bool?(true);
                          continue;
                        }
                        continue;
                      case 12:
                        switch (str2[5])
                        {
                          case 'P':
                            if (str2 == "*markPrefix*" && !strArray1[1].Equals(""))
                            {
                              this.markPrefix.Text = strArray1[1].Trim();
                              continue;
                            }
                            continue;
                          case 'S':
                            if (str2 == "*markSuffix*" && !strArray1[1].Equals(""))
                            {
                              this.markSuffix.Text = strArray1[1].Trim();
                              continue;
                            }
                            continue;
                          default:
                            continue;
                        }
                      case 14:
                        if (str2 == "*manufacturer*" && !strArray1[1].Equals(""))
                        {
                          this.manufacturer.Text = strArray1[1].Trim();
                          continue;
                        }
                        continue;
                      case 17:
                        if (str2 == "*markFinishEpoxy*" && !strArray1[1].Equals(""))
                        {
                          this.markFinishEpoxy.Text = strArray1[1].Trim();
                          continue;
                        }
                        continue;
                      case 19:
                        if (str2 == "*useMetricCanadian*" && strArray1[1].Equals("TRUE"))
                        {
                          this.useMetricCanadian.IsChecked = new bool?(true);
                          continue;
                        }
                        continue;
                      case 22:
                        if (str2 == "*markFinishGalvanized*" && !strArray1[1].Equals(""))
                        {
                          this.markFinishGalvanized.Text = strArray1[1].Trim();
                          continue;
                        }
                        continue;
                      case 23:
                        switch (str2[1])
                        {
                          case 'm':
                            if (str2 == "*markPrefixIntMinValue*" && !strArray1[1].Equals(""))
                            {
                              this.markPrefixIntMinValue.Text = strArray1[1].Trim();
                              continue;
                            }
                            continue;
                          case 's':
                            if (str2 == "*straightBarMarkPrefix*" && !strArray1[1].Equals(""))
                            {
                              this.straightBarMarkPrefix.Text = strArray1[1].Trim();
                              continue;
                            }
                            continue;
                          default:
                            continue;
                        }
                      case 25:
                        if (str2 == "*straightBarLengthOption*" && !strArray1[1].Equals(""))
                        {
                          this.straightBarLengthOption.SelectedItem = (object) strArray1[1].Trim();
                          continue;
                        }
                        continue;
                      case 28:
                        if (str2 == "*padBarSizesWithLeadingZero*" && strArray1[1].Equals("TRUE"))
                        {
                          this.padBarSizesWithLeadingZero.IsChecked = new bool?(true);
                          continue;
                        }
                        continue;
                      case 30:
                        if (str2 == "*straightBarLengthZeroPadding*" && !strArray1[1].Equals(""))
                        {
                          this.straightBarLengthZeroPadding.Text = strArray1[1].Trim();
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
          this.standardBarListNotUseMetric = this.checkStandardBarDefinitions(RebarUserSettingWindow._results);
          IOrderedEnumerable<StandardRebarItem> orderedEnumerable = this.standardBarListNotUseMetric.OrderBy<StandardRebarItem, string>((Func<StandardRebarItem, string>) (entry => entry.rebarMark));
          this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) "";
          this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) orderedEnumerable;
        }
        catch (IOException ex)
        {
          if (ex.Message.Contains("process"))
          {
            new TaskDialog("EDGE^R")
            {
              AllowCancellation = false,
              MainInstruction = "Rebar Settings File Error.",
              MainContent = $"Check Rebar Settings File: {this.manufacturerName}.txt. Please ensure the file is not in use by another application and try again."
            }.Show();
            ifContinue = false;
          }
        }
      }
      if (this.manufacturerKey == ManufacturerKeying.Standard)
        return;
      if (this.manufacturerKey == ManufacturerKeying.Kerkstra || this.manufacturerKey == ManufacturerKeying.Wells || this.manufacturerKey == ManufacturerKeying.Tindall)
        this.markPrefix.IsEnabled = true;
      else
        this.markPrefix.IsEnabled = false;
      this.markSuffix.Text = "";
      this.markSuffix.IsEnabled = false;
      if (this.manufacturerKey == ManufacturerKeying.Rocky || this.manufacturerKey == ManufacturerKeying.Kerkstra || this.manufacturerKey == ManufacturerKeying.MidStates || this.manufacturerKey == ManufacturerKeying.Illini || this.manufacturerKey == ManufacturerKeying.Spancrete)
      {
        this.markPrefixIntMinValue.Text = "1";
        this.markPrefixIntMinValue.IsEnabled = false;
      }
      if (this.manufacturerKey == ManufacturerKeying.Wells)
      {
        this.markPrefixIntMinValue.Text = "0";
        this.markPrefixIntMinValue.IsEnabled = false;
      }
      if (this.manufacturerKey == ManufacturerKeying.MidStates || this.manufacturerKey == ManufacturerKeying.Illini || this.manufacturerKey == ManufacturerKeying.Spancrete || this.manufacturerKey == ManufacturerKeying.Tindall)
      {
        this.markFinishGalvanized.Text = "G";
        this.markFinishEpoxy.Text = "E";
        this.markFinishGalvanized.IsEnabled = false;
        this.markFinishEpoxy.IsEnabled = false;
      }
      if (this.manufacturerKey == ManufacturerKeying.Kerkstra)
      {
        this.markFinishGalvanized.Text = "g";
        this.markFinishEpoxy.Text = "e";
        this.markFinishGalvanized.IsEnabled = false;
        this.markFinishEpoxy.IsEnabled = false;
      }
      if (this.manufacturerKey == ManufacturerKeying.Tindall)
        return;
      this.straightBarLengthOption.SelectedIndex = -1;
      this.straightBarLengthOption.IsEnabled = false;
    }
  }

  private void Setting_Selected(object sender, RoutedEventArgs e)
  {
    string str1 = this.Setting.SelectedItem.ToString();
    string str2 = "";
    if (str1.Contains("Bent Rebar Settings"))
      str2 = "set1";
    else if (str1.Contains("Straight Rebar Settings"))
      str2 = "set2";
    else if (str1.Contains("Standard Rebar Settings"))
      str2 = "set3";
    switch (str2)
    {
      case "set1":
        this.GeneralSettingPanel.Visibility = System.Windows.Visibility.Visible;
        this.GeneralSettingPanelBorder.Visibility = System.Windows.Visibility.Visible;
        this.StraightBarSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.StraightBarSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        this.StandardBarSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.StandardBarSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        break;
      case "set2":
        this.GeneralSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.GeneralSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        this.StraightBarSettingPanel.Visibility = System.Windows.Visibility.Visible;
        this.StraightBarSettingPanelBorder.Visibility = System.Windows.Visibility.Visible;
        this.StandardBarSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.StandardBarSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        break;
      case "set3":
        this.GeneralSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.GeneralSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        this.StraightBarSettingPanel.Visibility = System.Windows.Visibility.Hidden;
        this.StraightBarSettingPanelBorder.Visibility = System.Windows.Visibility.Hidden;
        this.StandardBarSettingPanel.Visibility = System.Windows.Visibility.Visible;
        this.StandardBarSettingPanelBorder.Visibility = System.Windows.Visibility.Visible;
        if (this.useMetricEuroBool)
        {
          this.displayMetric();
          break;
        }
        this.displayImperial();
        break;
    }
  }

  private void barSizeBoxNotUseMetric_Loaded(object sender, RoutedEventArgs e)
  {
    ComboBox comboBox = sender as ComboBox;
    comboBox.ItemsSource = (IEnumerable) "";
    comboBox.ItemsSource = (IEnumerable) this.USBarSize;
  }

  private void DataGrid_MouseDown(object sender, MouseButtonEventArgs e)
  {
    (sender as DataGrid).SelectedItem = (object) null;
    this.barSizeBoxNotUseMetric.SelectedIndex = -1;
    this.ShapTextBoxNotUseMetric.Text = "";
    this.MarkTextBoxNotUseMetric.Text = "";
    this.ATextBoxFeet.Text = "";
    this.BTextBoxFeet.Text = "";
    this.CTextBoxFeet.Text = "";
    this.DTextBoxFeet.Text = "";
    this.ETextBoxFeet.Text = "";
    this.FTextBoxFeet.Text = "";
    this.GTextBoxFeet.Text = "";
    this.HTextBoxFeet.Text = "";
    this.JTextBoxFeet.Text = "";
    this.KTextBoxFeet.Text = "";
    this.OTextBoxFeet.Text = "";
    this.ATextBoxInch.Text = "";
    this.BTextBoxInch.Text = "";
    this.CTextBoxInch.Text = "";
    this.DTextBoxInch.Text = "";
    this.ETextBoxInch.Text = "";
    this.FTextBoxInch.Text = "";
    this.GTextBoxInch.Text = "";
    this.HTextBoxInch.Text = "";
    this.JTextBoxInch.Text = "";
    this.KTextBoxInch.Text = "";
    this.OTextBoxInch.Text = "";
  }

  private void NotUseMetricDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
  {
    if (this.existingSettingNotUseMetric.SelectedItem == null)
      return;
    StandardRebarItem selectedItem = this.existingSettingNotUseMetric.SelectedItem as StandardRebarItem;
    if (!this.useMetricEuroBool)
      this.barSizeBoxNotUseMetric.SelectedItem = (object) BarDiameterOracle.BarDiameterToBarNumber(selectedItem.rebarDiameter);
    else
      this.barSizeBoxNotUseMetric.SelectedItem = (object) BarDiameterOracle.BarDiameterToBarNumberEuropean(selectedItem.rebarDiameter);
    this.ShapTextBoxNotUseMetric.Text = selectedItem.rebarShape;
    this.MarkTextBoxNotUseMetric.Text = selectedItem.rebarMark;
    if (!selectedItem.rebarA.Equals("null") && !string.IsNullOrWhiteSpace(selectedItem.rebarA))
    {
      string rebarA = selectedItem.rebarA;
      if (rebarA.Contains("'") && rebarA.Contains("\""))
      {
        int num1 = rebarA.IndexOf("'");
        this.ATextBoxFeet.Text = rebarA.Substring(0, num1);
        int length = rebarA.Length - 1;
        string str = rebarA.Substring(0, length).Substring(num1).Substring(1);
        int num2 = str.IndexOf("-");
        this.ATextBoxInch.Text = str.Substring(num2 + 2);
      }
      else if (rebarA.Contains("\""))
      {
        int length = rebarA.IndexOf("\"");
        int num = rebarA.IndexOf("-");
        this.ATextBoxInch.Text = rebarA.Substring(num + 2, length);
      }
      else if (rebarA.Contains("mm"))
      {
        int length = rebarA.IndexOf("mm");
        this.ATextBoxFeet.Text = rebarA.Substring(0, length);
        this.ATextBoxInch.Text = "";
      }
      else
      {
        int length = rebarA.IndexOf("'");
        this.ATextBoxFeet.Text = rebarA.Substring(0, length);
      }
    }
    else
    {
      this.ATextBoxFeet.Text = "";
      this.ATextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarB) && !selectedItem.rebarB.Equals("null"))
    {
      string rebarB = selectedItem.rebarB;
      if (rebarB.Contains("'") && rebarB.Contains("\""))
      {
        int num3 = rebarB.IndexOf("'");
        this.BTextBoxFeet.Text = rebarB.Substring(0, num3);
        int length = rebarB.Length - 1;
        string str = rebarB.Substring(0, length).Substring(num3).Substring(1);
        int num4 = str.IndexOf("-");
        this.BTextBoxInch.Text = str.Substring(num4 + 2);
      }
      else if (rebarB.Contains("\""))
      {
        int length = rebarB.IndexOf("\"");
        rebarB.IndexOf("-");
        this.BTextBoxInch.Text = rebarB.Substring(length + 2, length);
      }
      else if (rebarB.Contains("mm"))
      {
        int length = rebarB.IndexOf("mm");
        this.BTextBoxFeet.Text = rebarB.Substring(0, length);
        this.BTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarB.IndexOf("'");
        this.BTextBoxFeet.Text = rebarB.Substring(0, length);
      }
    }
    else
    {
      this.BTextBoxFeet.Text = "";
      this.BTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarC) && !selectedItem.rebarC.Equals("null"))
    {
      string rebarC = selectedItem.rebarC;
      if (rebarC.Contains("'") && rebarC.Contains("\""))
      {
        int num5 = rebarC.IndexOf("'");
        this.CTextBoxFeet.Text = rebarC.Substring(0, num5);
        int length = rebarC.Length - 1;
        string str = rebarC.Substring(0, length).Substring(num5).Substring(1);
        int num6 = str.IndexOf("-");
        this.CTextBoxInch.Text = str.Substring(num6 + 2);
      }
      else if (rebarC.Contains("\""))
      {
        int length = rebarC.IndexOf("\"");
        int num = rebarC.IndexOf("-");
        this.CTextBoxInch.Text = rebarC.Substring(num + 2, length);
      }
      else if (rebarC.Contains("mm"))
      {
        int length = rebarC.IndexOf("mm");
        this.CTextBoxFeet.Text = rebarC.Substring(0, length);
        this.CTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarC.IndexOf("'");
        this.CTextBoxFeet.Text = rebarC.Substring(0, length);
      }
    }
    else
    {
      this.CTextBoxFeet.Text = "";
      this.CTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarD) && !selectedItem.rebarD.Equals("null"))
    {
      string rebarD = selectedItem.rebarD;
      if (rebarD.Contains("'") && rebarD.Contains("\""))
      {
        int num7 = rebarD.IndexOf("'");
        this.DTextBoxFeet.Text = rebarD.Substring(0, num7);
        int length = rebarD.Length - 1;
        string str = rebarD.Substring(0, length).Substring(num7).Substring(1);
        int num8 = str.IndexOf("-");
        this.DTextBoxInch.Text = str.Substring(num8 + 2);
      }
      else if (rebarD.Contains("\""))
      {
        int length = rebarD.IndexOf("\"");
        int num = rebarD.IndexOf("-");
        this.DTextBoxInch.Text = rebarD.Substring(num + 2, length);
      }
      else if (rebarD.Contains("mm"))
      {
        int length = rebarD.IndexOf("mm");
        this.DTextBoxFeet.Text = rebarD.Substring(0, length);
        this.DTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarD.IndexOf("'");
        this.DTextBoxFeet.Text = rebarD.Substring(0, length);
      }
    }
    else
    {
      this.DTextBoxFeet.Text = "";
      this.DTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarE) && !selectedItem.rebarE.Equals("null"))
    {
      string rebarE = selectedItem.rebarE;
      if (rebarE.Contains("'") && rebarE.Contains("\""))
      {
        int num9 = rebarE.IndexOf("'");
        this.ETextBoxFeet.Text = rebarE.Substring(0, num9);
        int length = rebarE.Length - 1;
        string str = rebarE.Substring(0, length).Substring(num9).Substring(1);
        int num10 = str.IndexOf("-");
        this.ETextBoxInch.Text = str.Substring(num10 + 2);
      }
      else if (rebarE.Contains("\""))
      {
        int length = rebarE.IndexOf("\"");
        int num = rebarE.IndexOf("-");
        this.ETextBoxInch.Text = rebarE.Substring(num + 2, length);
      }
      else if (rebarE.Contains("mm"))
      {
        int length = rebarE.IndexOf("mm");
        this.ETextBoxFeet.Text = rebarE.Substring(0, length);
        this.ETextBoxInch.Text = "";
      }
      else
      {
        int length = rebarE.IndexOf("'");
        this.ETextBoxFeet.Text = rebarE.Substring(0, length);
      }
    }
    else
    {
      this.ETextBoxFeet.Text = "";
      this.ETextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarF) && !selectedItem.rebarF.Equals("null"))
    {
      string rebarF = selectedItem.rebarF;
      if (rebarF.Contains("'") && rebarF.Contains("\""))
      {
        int num11 = rebarF.IndexOf("'");
        this.FTextBoxFeet.Text = rebarF.Substring(0, num11);
        int length = rebarF.Length - 1;
        string str = rebarF.Substring(0, length).Substring(num11).Substring(1);
        int num12 = str.IndexOf("-");
        this.FTextBoxInch.Text = str.Substring(num12 + 2);
      }
      else if (rebarF.Contains("\""))
      {
        int length = rebarF.IndexOf("\"");
        int num = rebarF.IndexOf("-");
        this.FTextBoxInch.Text = rebarF.Substring(num + 2, length);
      }
      else if (rebarF.Contains("mm"))
      {
        int length = rebarF.IndexOf("mm");
        this.FTextBoxFeet.Text = rebarF.Substring(0, length);
        this.FTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarF.IndexOf("'");
        this.FTextBoxFeet.Text = rebarF.Substring(0, length);
      }
    }
    else
    {
      this.FTextBoxFeet.Text = "";
      this.FTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarG) && !selectedItem.rebarG.Equals("null"))
    {
      string rebarG = selectedItem.rebarG;
      if (rebarG.Contains("'") && rebarG.Contains("\""))
      {
        int num13 = rebarG.IndexOf("'");
        this.GTextBoxFeet.Text = rebarG.Substring(0, num13);
        int length = rebarG.Length - 1;
        string str = rebarG.Substring(0, length).Substring(num13).Substring(1);
        int num14 = str.IndexOf("-");
        this.GTextBoxInch.Text = str.Substring(num14 + 2);
      }
      else if (rebarG.Contains("\""))
      {
        int length = rebarG.IndexOf("\"");
        int num = rebarG.IndexOf("-");
        this.GTextBoxInch.Text = rebarG.Substring(num + 2, length);
      }
      else if (rebarG.Contains("mm"))
      {
        int length = rebarG.IndexOf("mm");
        this.GTextBoxFeet.Text = rebarG.Substring(0, length);
        this.GTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarG.IndexOf("'");
        this.GTextBoxFeet.Text = rebarG.Substring(0, length);
      }
    }
    else
    {
      this.GTextBoxFeet.Text = "";
      this.GTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarH) && !selectedItem.rebarH.Equals("null"))
    {
      string rebarH = selectedItem.rebarH;
      if (rebarH.Contains("'") && rebarH.Contains("\""))
      {
        int num15 = rebarH.IndexOf("'");
        this.HTextBoxFeet.Text = rebarH.Substring(0, num15);
        int length = rebarH.Length - 1;
        string str = rebarH.Substring(0, length).Substring(num15).Substring(1);
        int num16 = str.IndexOf("-");
        this.HTextBoxInch.Text = str.Substring(num16 + 2);
      }
      else if (rebarH.Contains("\""))
      {
        int length = rebarH.IndexOf("\"");
        int num = rebarH.IndexOf("-");
        this.HTextBoxInch.Text = rebarH.Substring(num + 2, length);
      }
      else if (rebarH.Contains("mm"))
      {
        int length = rebarH.IndexOf("mm");
        this.HTextBoxFeet.Text = rebarH.Substring(0, length);
        this.HTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarH.IndexOf("'");
        this.HTextBoxFeet.Text = rebarH.Substring(0, length);
      }
    }
    else
    {
      this.HTextBoxFeet.Text = "";
      this.HTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarJ) && !selectedItem.rebarJ.Equals("null"))
    {
      string rebarJ = selectedItem.rebarJ;
      if (rebarJ.Contains("'") && rebarJ.Contains("\""))
      {
        int num17 = rebarJ.IndexOf("'");
        this.JTextBoxFeet.Text = rebarJ.Substring(0, num17);
        int length = rebarJ.Length - 1;
        string str = rebarJ.Substring(0, length).Substring(num17).Substring(1);
        int num18 = str.IndexOf("-");
        this.JTextBoxInch.Text = str.Substring(num18 + 2);
      }
      else if (rebarJ.Contains("\""))
      {
        int length = rebarJ.IndexOf("\"");
        int num = rebarJ.IndexOf("-");
        this.JTextBoxInch.Text = rebarJ.Substring(num + 2, length);
      }
      else if (rebarJ.Contains("mm"))
      {
        int length = rebarJ.IndexOf("mm");
        this.JTextBoxFeet.Text = rebarJ.Substring(0, length);
        this.JTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarJ.IndexOf("'");
        this.JTextBoxFeet.Text = rebarJ.Substring(0, length);
      }
    }
    else
    {
      this.JTextBoxFeet.Text = "";
      this.JTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarK) && !selectedItem.rebarK.Equals("null"))
    {
      string rebarK = selectedItem.rebarK;
      if (rebarK.Contains("'") && rebarK.Contains("\""))
      {
        int num19 = rebarK.IndexOf("'");
        this.KTextBoxFeet.Text = rebarK.Substring(0, num19);
        int length = rebarK.Length - 1;
        string str = rebarK.Substring(0, length).Substring(num19).Substring(1);
        int num20 = str.IndexOf("-");
        this.KTextBoxInch.Text = str.Substring(num20 + 2);
      }
      else if (rebarK.Contains("\""))
      {
        int length = rebarK.IndexOf("\"");
        int num = rebarK.IndexOf("-");
        this.KTextBoxInch.Text = rebarK.Substring(num + 2, length);
      }
      else if (rebarK.Contains("mm"))
      {
        int length = rebarK.IndexOf("mm");
        this.KTextBoxFeet.Text = rebarK.Substring(0, length);
        this.KTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarK.IndexOf("'");
        this.KTextBoxFeet.Text = rebarK.Substring(0, length);
      }
    }
    else
    {
      this.KTextBoxFeet.Text = "";
      this.KTextBoxInch.Text = "";
    }
    if (!string.IsNullOrWhiteSpace(selectedItem.rebarO) && !selectedItem.rebarO.Equals("null"))
    {
      string rebarO = selectedItem.rebarO;
      if (rebarO.Contains("'") && rebarO.Contains("\""))
      {
        int num21 = rebarO.IndexOf("'");
        this.OTextBoxFeet.Text = rebarO.Substring(0, num21);
        int length = rebarO.Length - 1;
        string str = rebarO.Substring(0, length).Substring(num21).Substring(1);
        int num22 = str.IndexOf("-");
        this.OTextBoxInch.Text = str.Substring(num22 + 2);
      }
      else if (rebarO.Contains("\""))
      {
        int length = rebarO.IndexOf("\"");
        int num = rebarO.IndexOf("-");
        this.OTextBoxInch.Text = rebarO.Substring(num + 2, length);
      }
      else if (rebarO.Contains("mm"))
      {
        int length = rebarO.IndexOf("mm");
        this.OTextBoxFeet.Text = rebarO.Substring(0, length);
        this.OTextBoxInch.Text = "";
      }
      else
      {
        int length = rebarO.IndexOf("'");
        this.OTextBoxFeet.Text = rebarO.Substring(0, length);
      }
    }
    else
    {
      this.OTextBoxFeet.Text = "";
      this.OTextBoxInch.Text = "";
    }
  }

  private void ShapeButton_Click(object sender, RoutedEventArgs e)
  {
    this.selectionWinodow = new RebarShapeSelectionWindow(Process.GetCurrentProcess().MainWindowHandle);
    this.selectionWinodow.ShowDialog();
  }

  private void App_Activated(object sender, EventArgs e)
  {
    if (this.selectionWinodow == null || !this.selectionWinodow.isSaved)
      return;
    this.ShapTextBoxNotUseMetric.Text = this.selectionWinodow.Foo;
  }

  private void DeleteItem_Click(object sender, RoutedEventArgs e)
  {
    foreach (StandardRebarItem selectedItem in (IEnumerable) this.existingSettingNotUseMetric.SelectedItems)
      this.standardBarListNotUseMetric.Remove(selectedItem);
    this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) "";
    this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) this.standardBarListNotUseMetric;
  }

  private void SaveAndAddButton_Click(object sender, RoutedEventArgs e)
  {
    List<string> stringList = new List<string>();
    bool flag1 = false;
    if (this.barSizeBoxNotUseMetric.SelectedItem == null)
    {
      flag1 = true;
      this.barSizeBoxNotUseMetric.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
      this.barSizeBoxNotUseMetric.BorderThickness = new Thickness(1.0);
    }
    else
    {
      this.barSizeBoxNotUseMetric.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      this.barSizeBoxNotUseMetric.BorderThickness = new Thickness(0.5);
      stringList.Add(BarDiameterOracle.BarNumberToBarDiameter(this.barSizeBoxNotUseMetric.SelectedItem.ToString(), this.useMetricEuroBool));
    }
    if (string.IsNullOrWhiteSpace(this.ShapTextBoxNotUseMetric.Text))
    {
      flag1 = true;
      this.ShapTextBoxNotUseMetric.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
      this.ShapTextBoxNotUseMetric.BorderThickness = new Thickness(1.0);
    }
    else
    {
      this.ShapTextBoxNotUseMetric.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      this.ShapTextBoxNotUseMetric.BorderThickness = new Thickness(0.5);
      stringList.Add(this.ShapTextBoxNotUseMetric.Text);
    }
    if (string.IsNullOrWhiteSpace(this.MarkTextBoxNotUseMetric.Text))
    {
      flag1 = true;
      this.MarkTextBoxNotUseMetric.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
      this.MarkTextBoxNotUseMetric.BorderThickness = new Thickness(1.0);
    }
    else
    {
      this.MarkTextBoxNotUseMetric.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      this.MarkTextBoxNotUseMetric.BorderThickness = new Thickness(0.5);
      stringList.Add(this.MarkTextBoxNotUseMetric.Text);
    }
    if (!this.checkLegLength(this.ATextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.ATextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.BTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.BTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.CTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.CTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.DTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.DTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.ETextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.ETextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.FTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.FTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.GTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.GTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.HTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.HTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.JTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.JTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.KTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.KTextBoxInch))
      flag1 = true;
    if (!this.checkLegLength(this.OTextBoxFeet))
      flag1 = true;
    if (!this.checkLegInchLength(this.OTextBoxInch))
      flag1 = true;
    if (flag1)
    {
      int num = (int) MessageBox.Show("One or more of your inputs are invalid, please correct the input shown in red!", "Warning");
    }
    else
    {
      if (!this.useMetricEuroBool)
      {
        stringList.Add(this.addLegLength(this.ATextBoxFeet.Text, this.ATextBoxInch.Text));
        stringList.Add(this.addLegLength(this.BTextBoxFeet.Text, this.BTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.CTextBoxFeet.Text, this.CTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.DTextBoxFeet.Text, this.DTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.ETextBoxFeet.Text, this.ETextBoxInch.Text));
        stringList.Add(this.addLegLength(this.FTextBoxFeet.Text, this.FTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.GTextBoxFeet.Text, this.GTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.HTextBoxFeet.Text, this.HTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.JTextBoxFeet.Text, this.JTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.KTextBoxFeet.Text, this.KTextBoxInch.Text));
        stringList.Add(this.addLegLength(this.OTextBoxFeet.Text, this.OTextBoxInch.Text));
      }
      else
      {
        if (string.IsNullOrWhiteSpace(this.ATextBoxFeet.Text) && !this.checkMetricInput(this.ATextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.ATextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.ATextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.BTextBoxFeet.Text) && !this.checkMetricInput(this.BTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.BTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.BTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.CTextBoxFeet.Text) && !this.checkMetricInput(this.CTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.CTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.CTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.DTextBoxFeet.Text) && !this.checkMetricInput(this.DTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.DTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.DTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.ETextBoxFeet.Text) && !this.checkMetricInput(this.ETextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.ETextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.ETextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.FTextBoxFeet.Text) && !this.checkMetricInput(this.FTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.FTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.FTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.GTextBoxFeet.Text) && !this.checkMetricInput(this.GTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.GTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.GTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.HTextBoxFeet.Text) && !this.checkMetricInput(this.HTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.HTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.HTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.JTextBoxFeet.Text) && !this.checkMetricInput(this.JTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.JTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.JTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.KTextBoxFeet.Text) && !this.checkMetricInput(this.KTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.KTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.KTextBoxFeet.Text + "mm");
        if (string.IsNullOrWhiteSpace(this.OTextBoxFeet.Text) && !this.checkMetricInput(this.OTextBoxFeet.Text))
          stringList.Add(" ");
        else if (this.OTextBoxFeet.Text == "0")
          stringList.Add(" ");
        else
          stringList.Add(this.OTextBoxFeet.Text + "mm");
      }
      StandardRebarItem standardRebarItem1 = new StandardRebarItem(stringList[0], stringList[1], stringList[2], stringList[3], stringList[4], stringList[5], stringList[6], stringList[7], stringList[8], stringList[9], stringList[10], stringList[11], stringList[12], stringList[13]);
      if (this.existingSettingNotUseMetric.SelectedItem == null)
      {
        bool flag2 = false;
        foreach (StandardRebarItem standardRebarItem2 in this.standardBarListNotUseMetric)
        {
          if (standardRebarItem2.rebarMark.Equals(standardRebarItem1.rebarMark))
          {
            flag2 = true;
            if (MessageBox.Show("This Mark already exists in the list. Your input will overwrite the existing one.", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
              this.standardBarListNotUseMetric.Remove(standardRebarItem2);
              this.standardBarListNotUseMetric.Add(standardRebarItem1);
              break;
            }
            break;
          }
        }
        if (!flag2)
          this.standardBarListNotUseMetric.Add(standardRebarItem1);
      }
      else
      {
        this.standardBarListNotUseMetric.Remove(this.existingSettingNotUseMetric.SelectedItem as StandardRebarItem);
        this.standardBarListNotUseMetric.Add(standardRebarItem1);
      }
      this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) "";
      this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) this.standardBarListNotUseMetric.OrderBy<StandardRebarItem, string>((Func<StandardRebarItem, string>) (entry => entry.rebarMark));
      this.barSizeBoxNotUseMetric.SelectedIndex = -1;
      this.ShapTextBoxNotUseMetric.Text = "";
      this.MarkTextBoxNotUseMetric.Text = "";
      this.ATextBoxFeet.Text = "";
      this.BTextBoxFeet.Text = "";
      this.CTextBoxFeet.Text = "";
      this.DTextBoxFeet.Text = "";
      this.ETextBoxFeet.Text = "";
      this.FTextBoxFeet.Text = "";
      this.GTextBoxFeet.Text = "";
      this.HTextBoxFeet.Text = "";
      this.JTextBoxFeet.Text = "";
      this.KTextBoxFeet.Text = "";
      this.OTextBoxFeet.Text = "";
      this.ATextBoxInch.Text = "";
      this.BTextBoxInch.Text = "";
      this.CTextBoxInch.Text = "";
      this.DTextBoxInch.Text = "";
      this.ETextBoxInch.Text = "";
      this.FTextBoxInch.Text = "";
      this.GTextBoxInch.Text = "";
      this.HTextBoxInch.Text = "";
      this.JTextBoxInch.Text = "";
      this.KTextBoxInch.Text = "";
      this.OTextBoxInch.Text = "";
    }
  }

  private bool checkMetricInput(string millimeters)
  {
    millimeters = millimeters.Trim();
    if (millimeters == "null")
      return this.useMetricEuroBool;
    if (millimeters.Contains("mm"))
    {
      int length = millimeters.IndexOf("mm");
      millimeters = millimeters.Substring(0, length);
    }
    bool flag = true;
    if (millimeters.Contains("'") || millimeters.Contains("\"") || millimeters.Contains("/") || millimeters.Contains("-") || millimeters.Contains(".") || string.IsNullOrWhiteSpace(millimeters) || string.IsNullOrEmpty(millimeters))
      flag = false;
    return flag;
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

  private bool checkLegLength(TextBox txBox)
  {
    if (string.IsNullOrWhiteSpace(txBox.Text))
    {
      txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      txBox.BorderThickness = new Thickness(0.5);
      return true;
    }
    if (int.TryParse(txBox.Text, out int _))
    {
      txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      txBox.BorderThickness = new Thickness(0.5);
      return true;
    }
    txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
    txBox.BorderThickness = new Thickness(1.0);
    return false;
  }

  private bool checkLegInchLength(TextBox txBox)
  {
    if (string.IsNullOrWhiteSpace(txBox.Text))
    {
      txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
      txBox.BorderThickness = new Thickness(0.5);
      return true;
    }
    if (txBox.Text.TrimStart().TrimEnd().Contains(" ") && !txBox.Text.TrimStart().TrimEnd().Contains("/"))
    {
      txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
      txBox.BorderThickness = new Thickness(1.0);
      return false;
    }
    foreach (char c in txBox.Text)
    {
      if (!char.IsNumber(c))
      {
        if (c != ' ' && c != '/')
        {
          txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
          txBox.BorderThickness = new Thickness(1.0);
          return false;
        }
        if (c == '/')
        {
          int num1 = txBox.Text.IndexOf(c.ToString());
          int num2 = txBox.Text.LastIndexOf(c.ToString());
          if (num1 != num2)
          {
            txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
            txBox.BorderThickness = new Thickness(1.0);
            return false;
          }
          if (num1 == 0 || num1 == txBox.Text.Length - 1 || txBox.Text.ToCharArray()[num1 + 1].ToString().Equals(" ") || txBox.Text.ToCharArray()[num1 - 1].ToString().Equals(" ") || txBox.Text.ToCharArray()[num1 + 1].ToString().Equals("0") || txBox.Text.ToCharArray()[num1 - 1].ToString().Equals("0"))
          {
            txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
            txBox.BorderThickness = new Thickness(1.0);
            return false;
          }
        }
      }
    }
    txBox.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
    txBox.BorderThickness = new Thickness(0.5);
    return true;
  }

  private void ApplyButton_Click(object sender, RoutedEventArgs e)
  {
    if (!this.checkStandardBarSave(this.standardBarListNotUseMetric))
      return;
    string text1 = this.manufacturer.Text;
    string text2 = this.markPrefix.Text;
    string text3 = this.markSuffix.Text;
    string text4 = this.markPrefixIntMinValue.Text;
    string text5 = this.markFinishGalvanized.Text;
    string text6 = this.markFinishEpoxy.Text;
    string text7 = this.straightBarMarkPrefix.Text;
    string str1 = "";
    if (this.straightBarLengthOption.SelectedItem != null)
      str1 = this.straightBarLengthOption.SelectedItem.ToString();
    string str2 = "";
    if (this.straightBarVisible)
      str2 = this.straightBarLengthZeroPadding.Text;
    StringBuilder stringBuilder = new StringBuilder();
    bool flag1 = false;
    if (!string.IsNullOrEmpty(text1))
    {
      string str3 = "";
      Parameter parameter = this.RevitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
      if (parameter != null)
        str3 = parameter.AsString();
      if (str3.Equals(text1))
      {
        stringBuilder.AppendLine("*manufacturer*," + text1);
        this.manufacturer.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.manufacturer.BorderThickness = new Thickness(1.0);
      }
      else
      {
        flag1 = true;
        this.manufacturer.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        this.manufacturer.BorderThickness = new Thickness(2.0);
      }
    }
    stringBuilder.AppendLine("*useMetric*," + this.useMetricCanadian.IsChecked.ToString().ToUpper());
    if (!string.IsNullOrEmpty(text2))
    {
      if (text2.Contains(","))
      {
        flag1 = true;
        this.markPrefix.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        this.markPrefix.BorderThickness = new Thickness(2.0);
      }
      else
      {
        this.markPrefix.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.markPrefix.BorderThickness = new Thickness(1.0);
        stringBuilder.AppendLine("*markPrefix*," + text2);
      }
    }
    if (!string.IsNullOrEmpty(text3))
    {
      if (text3.Contains(","))
      {
        flag1 = true;
        this.markSuffix.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        this.markSuffix.BorderThickness = new Thickness(2.0);
      }
      else
      {
        this.markSuffix.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.markSuffix.BorderThickness = new Thickness(1.0);
        stringBuilder.AppendLine("*markSuffix*," + text3);
      }
    }
    stringBuilder.AppendLine("*padBarSizesWithLeadingZero*," + this.padBarSizesWithLeadingZeroParam.ToString().ToUpper());
    if (!string.IsNullOrEmpty(text4))
    {
      bool flag2 = true;
      foreach (char c in text4)
      {
        if (!char.IsDigit(c))
        {
          flag2 = false;
          flag1 = true;
          this.markPrefixIntMinValue.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
          this.markPrefixIntMinValue.BorderThickness = new Thickness(2.0);
          break;
        }
      }
      if (flag2)
      {
        stringBuilder.AppendLine("*markPrefixIntMinValue*," + text4);
        this.markPrefixIntMinValue.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.markPrefixIntMinValue.BorderThickness = new Thickness(1.0);
      }
    }
    if (!string.IsNullOrEmpty(text5))
    {
      if (char.IsLetter(text5[0]) && !text5.Contains(","))
      {
        stringBuilder.AppendLine("*markFinishGalvanized*," + text5);
        this.markFinishGalvanized.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.markFinishGalvanized.BorderThickness = new Thickness(1.0);
      }
      else
      {
        flag1 = true;
        this.markFinishGalvanized.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        this.markFinishGalvanized.BorderThickness = new Thickness(2.0);
      }
    }
    if (!string.IsNullOrEmpty(text6))
    {
      if (char.IsLetter(text6[0]) && !text6.Contains(","))
      {
        stringBuilder.AppendLine("*markFinishEpoxy*," + text6);
        this.markFinishEpoxy.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.markFinishEpoxy.BorderThickness = new Thickness(1.0);
      }
      else
      {
        flag1 = true;
        this.markFinishEpoxy.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        this.markFinishEpoxy.BorderThickness = new Thickness(2.0);
      }
    }
    if (!string.IsNullOrEmpty(text7))
    {
      if (text7.Contains(","))
      {
        flag1 = true;
        this.straightBarMarkPrefix.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
        this.straightBarMarkPrefix.BorderThickness = new Thickness(2.0);
      }
      else
      {
        this.straightBarMarkPrefix.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.straightBarMarkPrefix.BorderThickness = new Thickness(1.0);
        stringBuilder.AppendLine("*straightBarMarkPrefix*," + text7);
      }
    }
    if (!string.IsNullOrEmpty(str1))
      stringBuilder.AppendLine("*straightBarLengthOption*," + str1);
    if (this.straightBarVisible && !string.IsNullOrEmpty(str2))
    {
      bool flag3 = true;
      foreach (char c in str2)
      {
        if (!char.IsDigit(c))
        {
          flag3 = false;
          flag1 = true;
          this.straightBarLengthZeroPadding.BorderBrush = (Brush) new SolidColorBrush(Colors.Red);
          this.straightBarLengthZeroPadding.BorderThickness = new Thickness(2.0);
          break;
        }
      }
      if (flag3)
      {
        stringBuilder.AppendLine("*straightBarLengthZeroPadding*," + str2);
        this.straightBarLengthZeroPadding.BorderBrush = (Brush) new SolidColorBrush(Colors.Black);
        this.straightBarLengthZeroPadding.BorderThickness = new Thickness(1.0);
      }
    }
    foreach (StandardRebarItem standardRebarItem in this.standardBarListNotUseMetric)
      stringBuilder.AppendLine($"*rebar*,{standardRebarItem.rebarDiameter},{standardRebarItem.rebarShape},{standardRebarItem.rebarMark},{standardRebarItem.rebarA},{standardRebarItem.rebarB},{standardRebarItem.rebarC},{standardRebarItem.rebarD},{standardRebarItem.rebarE},{standardRebarItem.rebarF},{standardRebarItem.rebarG},{standardRebarItem.rebarH},{standardRebarItem.rebarJ},{standardRebarItem.rebarK},{standardRebarItem.rebarO}");
    if (flag1)
    {
      int num1 = (int) MessageBox.Show("One or more of your inputs are invalid, please correct your input shown in red!", "Warning");
    }
    else
    {
      string path = App.RebarSettingsFolderPath;
      if (!Directory.Exists(path))
      {
        path = "C:\\EDGEforRevit";
        int num2 = (int) MessageBox.Show("The default rebar setting folder path does not exist. The setting file will be saved to C:\\EDGEforRevit\\ folder instead.", "Warning");
      }
      if (!File.Exists($"{path}\\{this.manufacturerName}.txt"))
        File.Create($"{path}\\{this.manufacturerName}.txt").Close();
      string str4 = $"{path}\\{this.manufacturerName}.txt";
      FileInfo fileInfo = new FileInfo(str4);
      try
      {
        using (StreamWriter streamWriter = new StreamWriter(str4))
          streamWriter.WriteLine(stringBuilder.ToString());
        this.standardBarList = new List<StandardRebarItem>();
        this.standardBarListNotUseMetric = new List<StandardRebarItem>();
        this.Close();
      }
      catch (Exception ex)
      {
        if (fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
          new TaskDialog("EDGE^R")
          {
            AllowCancellation = false,
            MainInstruction = "Rebar Settings File is Read Only",
            MainContent = $"Unable to read the Rebar Settings File in the specified location {str4} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
          }.Show();
        if (ex.Message.Contains("process"))
          new TaskDialog("EDGE^R")
          {
            AllowCancellation = false,
            MainInstruction = "Rebar Settings File Error.",
            MainContent = $"Check Rebar Settings File: {this.manufacturerName}.txt. Please ensure the file is not in use by another application and try again."
          }.Show();
        this.Close();
      }
    }
  }

  private void padBarSizesWithLeadingZero_Checked(object sender, RoutedEventArgs e)
  {
    this.padBarSizesWithLeadingZeroParam = true;
  }

  private void padBarSizesWithLeadingZero_Unchecked(object sender, RoutedEventArgs e)
  {
    this.padBarSizesWithLeadingZeroParam = false;
  }

  private void useMetricCanadian_Checked(object sender, RoutedEventArgs e) => this.UseMatrix = true;

  private void useMetricEuropean_Checked(object sender, RoutedEventArgs e)
  {
    string text = this.straightBarLengthOption.Text;
    this.useMetricEuroBool = this.projectInMetric;
    List<string> stringList = new List<string>();
    if (this.useMetricEuroBool)
    {
      stringList.Add("METERS");
      stringList.Add("CENTIMETERS");
      stringList.Add("MILLIMETERS");
      stringList.Add("NONE");
      this.ToolTipLine1.Text = "METERS - show straight bar control marks with meters. E.g., a 5 Meter meter #6 bar = 65";
      this.ToolTipLine2.Text = "CENTIMETERS - show straight bar control marks in centimeters. E.g., a 5 meter #6 bar = 6500";
      this.ToolTipLine3.Text = "MILLIMETERS - show straight bar control marks in millimeters. E.g., a 5 meter #6 bar = 65000";
      this.ToolTipLine4.Visibility = System.Windows.Visibility.Visible;
    }
    else
    {
      stringList.Add("FEETANDINCHES");
      stringList.Add("INCHES");
      stringList.Add("NONE");
      this.ToolTipLine1.Text = "FEETANDINCHES = show straight bar control marks with feet and inches. E.g., a 10 foot #3 bar = 31000 (10 feet and 0 inches).";
      this.ToolTipLine2.Text = "INCHES = show straight bars in inches only. E.g., a 10 foot #3 bar = 3120.";
      this.ToolTipLine3.Text = "NONE = show only the bar size. E.g., a 10 foot #3 bar = 3.";
      this.ToolTipLine4.Visibility = System.Windows.Visibility.Collapsed;
    }
    this.straightBarLengthOption.ItemsSource = (IEnumerable) stringList;
    if (stringList.Contains(text))
      this.straightBarLengthOption.SelectedItem = (object) text;
    else
      this.straightBarLengthOption.SelectedItem = (object) null;
    if (!this.useMetricEuroBool)
      return;
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) this.MatrixBarSize;
    this.standardBarListNotUseMetric = this.checkStandardBarDefinitions(this.standardBarListNotUseMetric);
    IOrderedEnumerable<StandardRebarItem> orderedEnumerable = this.standardBarListNotUseMetric.OrderBy<StandardRebarItem, string>((Func<StandardRebarItem, string>) (entry => entry.rebarMark));
    this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) "";
    this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) orderedEnumerable;
  }

  private void useMetricCanadian_Unchecked(object sender, RoutedEventArgs e)
  {
    this.UseMatrix = false;
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) "";
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) this.USBarSize;
    List<string> stringList = new List<string>();
  }

  private void useMetricEuropean_Unchecked(object sender, RoutedEventArgs e)
  {
    this.useMetricEuroBool = false;
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) "";
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) this.USBarSize;
    this.straightBarLengthOption.ItemsSource = (IEnumerable) new List<string>()
    {
      "FEETANDINCHES",
      "INCHES",
      "NONE"
    };
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) this.USBarSize;
    this.standardBarListNotUseMetric = this.checkStandardBarDefinitions(this.standardBarListNotUseMetric);
    IOrderedEnumerable<StandardRebarItem> orderedEnumerable = this.standardBarListNotUseMetric.OrderBy<StandardRebarItem, string>((Func<StandardRebarItem, string>) (entry => entry.rebarMark));
    this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) "";
    this.existingSettingNotUseMetric.ItemsSource = (IEnumerable) orderedEnumerable;
  }

  private void straightBarLengthOption_Loaded(object sender, RoutedEventArgs e)
  {
    List<string> stringList = new List<string>();
    if (this.useMetricEuroBool)
    {
      stringList.Add("METERS");
      stringList.Add("CENTIMETERS");
      stringList.Add("MILLIMETERS");
      stringList.Add("NONE");
    }
    else
    {
      stringList.Add("FEETANDINCHES");
      stringList.Add("INCHES");
      stringList.Add("NONE");
    }
    (sender as ComboBox).ItemsSource = (IEnumerable) stringList;
  }

  private void straightBarLengthOption_Selected(object sender, RoutedEventArgs e)
  {
    ComboBox comboBox = sender as ComboBox;
    if (comboBox.SelectedItem != null && comboBox.SelectedItem.ToString().Equals("INCHES"))
    {
      this.straightBarVisible = true;
      this.straightBarLengthZeroPaddingTextblock.Visibility = System.Windows.Visibility.Visible;
      this.straightBarLengthZeroPadding.Visibility = System.Windows.Visibility.Visible;
    }
    else
    {
      this.straightBarVisible = false;
      this.straightBarLengthZeroPaddingTextblock.Visibility = System.Windows.Visibility.Hidden;
      this.straightBarLengthZeroPadding.Visibility = System.Windows.Visibility.Hidden;
    }
  }

  private void existingSettingNotUseMetric_TargetUpdated(object sender, DataTransferEventArgs e)
  {
    for (int index = 1; index < this.existingSettingNotUseMetric.Columns.Count<DataGridColumn>(); ++index)
    {
      this.existingSettingNotUseMetric.Columns[index].Width = (DataGridLength) 0.0;
      this.existingSettingNotUseMetric.UpdateLayout();
      this.existingSettingNotUseMetric.Columns[index].Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);
    }
  }

  private void displayMetric()
  {
    foreach (TextBlock feetLabel in this.FeetLabels)
      feetLabel.Text = "mm";
    foreach (UIElement inchTextBox in this.InchTextBoxes)
      inchTextBox.Visibility = System.Windows.Visibility.Hidden;
    foreach (UIElement inchLabel in this.InchLabels)
      inchLabel.Visibility = System.Windows.Visibility.Hidden;
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) this.MatrixBarSize;
  }

  private void displayImperial()
  {
    foreach (TextBlock feetLabel in this.FeetLabels)
      feetLabel.Text = "ft";
    foreach (UIElement inchTextBox in this.InchTextBoxes)
      inchTextBox.Visibility = System.Windows.Visibility.Visible;
    foreach (UIElement inchLabel in this.InchLabels)
      inchLabel.Visibility = System.Windows.Visibility.Visible;
    this.barSizeBoxNotUseMetric.ItemsSource = (IEnumerable) this.USBarSize;
  }

  private List<StandardRebarItem> checkStandardBarDefinitions(
    List<StandardRebarItem> rebarDefintions)
  {
    List<string> stringList = new List<string>();
    if (this.useMetricEuroBool)
    {
      foreach (StandardRebarItem rebarDefintion in rebarDefintions)
      {
        if (!this.checkMetricInput(rebarDefintion.rebarDiameter) && !string.IsNullOrEmpty(rebarDefintion.rebarA.Trim()))
        {
          string rebarDiameter = rebarDefintion.rebarDiameter;
          string feetValueString = "0";
          string input = "0";
          if (rebarDiameter.Contains("'") && rebarDiameter.Contains("\""))
          {
            int num = rebarDiameter.IndexOf("'");
            feetValueString = rebarDiameter.Substring(0, num);
            int length = rebarDiameter.Length - 1;
            string str = rebarDiameter.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarDiameter.Contains("\""))
          {
            int length = rebarDiameter.IndexOf("\"");
            int num = rebarDiameter.IndexOf("-");
            input = rebarDiameter.Substring(num + 2, length);
          }
          else
          {
            int length = rebarDiameter.IndexOf("'");
            feetValueString = rebarDiameter.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarDiameter = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarA) && !string.IsNullOrEmpty(rebarDefintion.rebarA.Trim()) && rebarDefintion.rebarA.Trim() != "null")
        {
          string rebarA = rebarDefintion.rebarA;
          string feetValueString = "0";
          string input = "0";
          if (rebarA.Contains("'") && rebarA.Contains("\""))
          {
            int num = rebarA.IndexOf("'");
            feetValueString = rebarA.Substring(0, num);
            int length = rebarA.Length - 1;
            string str = rebarA.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarA.Contains("\""))
          {
            int length = rebarA.IndexOf("\"");
            int num = rebarA.IndexOf("-");
            input = rebarA.Substring(num + 2, length);
          }
          else
          {
            int length = rebarA.IndexOf("'");
            feetValueString = rebarA.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarA = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarB) && !string.IsNullOrEmpty(rebarDefintion.rebarB.Trim()) && rebarDefintion.rebarB.Trim() != "null")
        {
          string rebarB = rebarDefintion.rebarB;
          string feetValueString = "0";
          string input = "0";
          if (rebarB.Contains("'") && rebarB.Contains("\""))
          {
            int num = rebarB.IndexOf("'");
            feetValueString = rebarB.Substring(0, num);
            int length = rebarB.Length - 1;
            string str = rebarB.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarB.Contains("\""))
          {
            int length = rebarB.IndexOf("\"");
            int num = rebarB.IndexOf("-");
            input = rebarB.Substring(num + 2, length);
          }
          else
          {
            int length = rebarB.IndexOf("'");
            feetValueString = rebarB.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarB = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarC) && !string.IsNullOrEmpty(rebarDefintion.rebarC.Trim()) && rebarDefintion.rebarC.Trim() != "null")
        {
          string rebarC = rebarDefintion.rebarC;
          string feetValueString = "0";
          string input = "0";
          if (rebarC.Contains("'") && rebarC.Contains("\""))
          {
            int num = rebarC.IndexOf("'");
            feetValueString = rebarC.Substring(0, num);
            int length = rebarC.Length - 1;
            string str = rebarC.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarC.Contains("\""))
          {
            int length = rebarC.IndexOf("\"");
            int num = rebarC.IndexOf("-");
            input = rebarC.Substring(num + 2, length);
          }
          else
          {
            int length = rebarC.IndexOf("'");
            feetValueString = rebarC.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarC = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarD) && !string.IsNullOrEmpty(rebarDefintion.rebarD.Trim()) && rebarDefintion.rebarD.Trim() != "null")
        {
          string rebarD = rebarDefintion.rebarD;
          string feetValueString = "0";
          string input = "0";
          if (rebarD.Contains("'") && rebarD.Contains("\""))
          {
            int num = rebarD.IndexOf("'");
            feetValueString = rebarD.Substring(0, num);
            int length = rebarD.Length - 1;
            string str = rebarD.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarD.Contains("\""))
          {
            int length = rebarD.IndexOf("\"");
            int num = rebarD.IndexOf("-");
            input = rebarD.Substring(num + 2, length);
          }
          else
          {
            int length = rebarD.IndexOf("'");
            feetValueString = rebarD.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarD = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarE) && !string.IsNullOrEmpty(rebarDefintion.rebarE.Trim()) && rebarDefintion.rebarE.Trim() != "null")
        {
          string rebarE = rebarDefintion.rebarE;
          string feetValueString = "0";
          string input = "0";
          if (rebarE.Contains("'") && rebarE.Contains("\""))
          {
            int num = rebarE.IndexOf("'");
            feetValueString = rebarE.Substring(0, num);
            int length = rebarE.Length - 1;
            string str = rebarE.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarE.Contains("\""))
          {
            int length = rebarE.IndexOf("\"");
            int num = rebarE.IndexOf("-");
            input = rebarE.Substring(num + 2, length);
          }
          else
          {
            int length = rebarE.IndexOf("'");
            feetValueString = rebarE.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarE = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarF) && !string.IsNullOrEmpty(rebarDefintion.rebarF.Trim()) && rebarDefintion.rebarF.Trim() != "null")
        {
          string rebarF = rebarDefintion.rebarF;
          string feetValueString = "0";
          string input = "0";
          if (rebarF.Contains("'") && rebarF.Contains("\""))
          {
            int num = rebarF.IndexOf("'");
            feetValueString = rebarF.Substring(0, num);
            int length = rebarF.Length - 1;
            string str = rebarF.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarF.Contains("\""))
          {
            int length = rebarF.IndexOf("\"");
            int num = rebarF.IndexOf("-");
            input = rebarF.Substring(num + 2, length);
          }
          else
          {
            int length = rebarF.IndexOf("'");
            feetValueString = rebarF.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarF = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarG) && !string.IsNullOrEmpty(rebarDefintion.rebarG.Trim()) && rebarDefintion.rebarG.Trim() != "null")
        {
          string rebarG = rebarDefintion.rebarG;
          string feetValueString = "0";
          string input = "0";
          if (rebarG.Contains("'") && rebarG.Contains("\""))
          {
            int num = rebarG.IndexOf("'");
            feetValueString = rebarG.Substring(0, num);
            int length = rebarG.Length - 1;
            string str = rebarG.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarG.Contains("\""))
          {
            int length = rebarG.IndexOf("\"");
            int num = rebarG.IndexOf("-");
            input = rebarG.Substring(num + 2, length);
          }
          else
          {
            int length = rebarG.IndexOf("'");
            feetValueString = rebarG.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarG = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarH) && !string.IsNullOrEmpty(rebarDefintion.rebarH.Trim()) && rebarDefintion.rebarH.Trim() != "null")
        {
          string rebarH = rebarDefintion.rebarH;
          string feetValueString = "0";
          string input = "0";
          if (rebarH.Contains("'") && rebarH.Contains("\""))
          {
            int num = rebarH.IndexOf("'");
            feetValueString = rebarH.Substring(0, num);
            int length = rebarH.Length - 1;
            string str = rebarH.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarH.Contains("\""))
          {
            int length = rebarH.IndexOf("\"");
            int num = rebarH.IndexOf("-");
            input = rebarH.Substring(num + 2, length);
          }
          else
          {
            int length = rebarH.IndexOf("'");
            feetValueString = rebarH.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarH = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarJ) && !string.IsNullOrEmpty(rebarDefintion.rebarJ.Trim()) && rebarDefintion.rebarJ.Trim() != "null")
        {
          string rebarJ = rebarDefintion.rebarJ;
          string feetValueString = "0";
          string input = "0";
          if (rebarJ.Contains("'") && rebarJ.Contains("\""))
          {
            int num = rebarJ.IndexOf("'");
            feetValueString = rebarJ.Substring(0, num);
            int length = rebarJ.Length - 1;
            string str = rebarJ.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarJ.Contains("\""))
          {
            int length = rebarJ.IndexOf("\"");
            int num = rebarJ.IndexOf("-");
            input = rebarJ.Substring(num + 2, length);
          }
          else
          {
            int length = rebarJ.IndexOf("'");
            feetValueString = rebarJ.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarJ = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarK) && !string.IsNullOrEmpty(rebarDefintion.rebarK.Trim()) && rebarDefintion.rebarK.Trim() != "null")
        {
          string rebarK = rebarDefintion.rebarK;
          string feetValueString = "0";
          string input = "0";
          if (rebarK.Contains("'") && rebarK.Contains("\""))
          {
            int num = rebarK.IndexOf("'");
            feetValueString = rebarK.Substring(0, num);
            int length = rebarK.Length - 1;
            string str = rebarK.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarK.Contains("\""))
          {
            int length = rebarK.IndexOf("\"");
            int num = rebarK.IndexOf("-");
            input = rebarK.Substring(num + 2, length);
          }
          else
          {
            int length = rebarK.IndexOf("'");
            feetValueString = rebarK.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarK = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (!this.checkMetricInput(rebarDefintion.rebarO) && !string.IsNullOrEmpty(rebarDefintion.rebarO.Trim()) && rebarDefintion.rebarO.Trim() != "null")
        {
          string rebarO = rebarDefintion.rebarO;
          string feetValueString = "0";
          string input = "0";
          if (rebarO.Contains("'") && rebarO.Contains("\""))
          {
            int num = rebarO.IndexOf("'");
            feetValueString = rebarO.Substring(0, num);
            int length = rebarO.Length - 1;
            string str = rebarO.Substring(0, length).Substring(num).Substring(1);
            input = str.Substring(str.IndexOf("-") + 2);
          }
          else if (rebarO.Contains("\""))
          {
            int length = rebarO.IndexOf("\"");
            int num = rebarO.IndexOf("-");
            input = rebarO.Substring(num + 2, length);
          }
          else
          {
            int length = rebarO.IndexOf("'");
            feetValueString = rebarO.Substring(0, length);
          }
          string inchValueString = FeetAndInchesRounding.FractionToDouble(input).ToString();
          rebarDefintion.rebarO = UnitConversion.roundFloorString(UnitConversion.feetAndInchesToMillimetersString(feetValueString, inchValueString)) + "mm";
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
      }
    }
    else
    {
      foreach (StandardRebarItem rebarDefintion in rebarDefintions)
      {
        if (rebarDefintion.rebarDiameter.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarDiameter.Trim()))
        {
          string rebarDiameter = rebarDefintion.rebarDiameter;
          string valueString = rebarDiameter.Substring(0, rebarDiameter.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local1 = ref feet;
          ref string local2 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local1, out local2);
          double result = 0.0;
          double.TryParse(s, out result);
          double num1 = Math.Floor(result);
          double num2 = Math.Floor(result % 1.0 * 8.0);
          string str = num1 == 0.0 && num2 == 0.0 || num2 == 0.0 ? this.addLegLength(feet, num1.ToString()) : (num1 != 0.0 ? this.addLegLength(feet, $"{num1.ToString()} {FeetAndInchesRounding.DecimalToFraction(num2 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num2 / 8.0)));
          rebarDefintion.rebarDiameter = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarA.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarA.Trim()) && rebarDefintion.rebarA.Trim() != "null")
        {
          string rebarA = rebarDefintion.rebarA;
          string valueString = rebarA.Substring(0, rebarA.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local3 = ref feet;
          ref string local4 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local3, out local4);
          double result = 0.0;
          double.TryParse(s, out result);
          double num3 = Math.Floor(result);
          double num4 = Math.Floor(result % 1.0 * 8.0);
          string str = num3 == 0.0 && num4 == 0.0 || num4 == 0.0 ? this.addLegLength(feet, num3.ToString()) : (num3 != 0.0 ? this.addLegLength(feet, $"{num3.ToString()} {FeetAndInchesRounding.DecimalToFraction(num4 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num4 / 8.0)));
          rebarDefintion.rebarA = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarB.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarB.Trim()) && rebarDefintion.rebarB.Trim() != "null")
        {
          string rebarB = rebarDefintion.rebarB;
          string valueString = rebarB.Substring(0, rebarB.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local5 = ref feet;
          ref string local6 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local5, out local6);
          double result = 0.0;
          double.TryParse(s, out result);
          double num5 = Math.Floor(result);
          double num6 = Math.Floor(result % 1.0 * 8.0);
          string str = num5 == 0.0 && num6 == 0.0 || num6 == 0.0 ? this.addLegLength(feet, num5.ToString()) : (num5 != 0.0 ? this.addLegLength(feet, $"{num5.ToString()} {FeetAndInchesRounding.DecimalToFraction(num6 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num6 / 8.0)));
          rebarDefintion.rebarB = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarC.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarC.Trim()) && rebarDefintion.rebarC.Trim() != "null")
        {
          string rebarC = rebarDefintion.rebarC;
          string valueString = rebarC.Substring(0, rebarC.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local7 = ref feet;
          ref string local8 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local7, out local8);
          double result = 0.0;
          double.TryParse(s, out result);
          double num7 = Math.Floor(result);
          double num8 = Math.Floor(result % 1.0 * 8.0);
          string str = num7 == 0.0 && num8 == 0.0 || num8 == 0.0 ? this.addLegLength(feet, num7.ToString()) : (num7 != 0.0 ? this.addLegLength(feet, $"{num7.ToString()} {FeetAndInchesRounding.DecimalToFraction(num8 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num8 / 8.0)));
          rebarDefintion.rebarC = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarD.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarD.Trim()) && rebarDefintion.rebarD.Trim() != "null")
        {
          string rebarD = rebarDefintion.rebarD;
          string valueString = rebarD.Substring(0, rebarD.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local9 = ref feet;
          ref string local10 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local9, out local10);
          double result = 0.0;
          double.TryParse(s, out result);
          double num9 = Math.Floor(result);
          double num10 = Math.Floor(result % 1.0 * 8.0);
          string str = num9 == 0.0 && num10 == 0.0 || num10 == 0.0 ? this.addLegLength(feet, num9.ToString()) : (num9 != 0.0 ? this.addLegLength(feet, $"{num9.ToString()} {FeetAndInchesRounding.DecimalToFraction(num10 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num10 / 8.0)));
          rebarDefintion.rebarD = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarE.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarE.Trim()) && rebarDefintion.rebarE.Trim() != "null")
        {
          string rebarE = rebarDefintion.rebarE;
          string valueString = rebarE.Substring(0, rebarE.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local11 = ref feet;
          ref string local12 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local11, out local12);
          double result = 0.0;
          double.TryParse(s, out result);
          double num11 = Math.Floor(result);
          double num12 = Math.Floor(result % 1.0 * 8.0);
          string str = num11 == 0.0 && num12 == 0.0 || num12 == 0.0 ? this.addLegLength(feet, num11.ToString()) : (num11 != 0.0 ? this.addLegLength(feet, $"{num11.ToString()} {FeetAndInchesRounding.DecimalToFraction(num12 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num12 / 8.0)));
          rebarDefintion.rebarE = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarF.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarF.Trim()) && rebarDefintion.rebarF.Trim() != "null")
        {
          string rebarF = rebarDefintion.rebarF;
          string valueString = rebarF.Substring(0, rebarF.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local13 = ref feet;
          ref string local14 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local13, out local14);
          double result = 0.0;
          double.TryParse(s, out result);
          double num13 = Math.Floor(result);
          double num14 = Math.Floor(result % 1.0 * 8.0);
          string str = num13 == 0.0 && num14 == 0.0 || num14 == 0.0 ? this.addLegLength(feet, num13.ToString()) : (num13 != 0.0 ? this.addLegLength(feet, $"{num13.ToString()} {FeetAndInchesRounding.DecimalToFraction(num14 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num14 / 8.0)));
          rebarDefintion.rebarF = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarG.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarG.Trim()) && rebarDefintion.rebarG.Trim() != "null")
        {
          string rebarG = rebarDefintion.rebarG;
          string valueString = rebarG.Substring(0, rebarG.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local15 = ref feet;
          ref string local16 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local15, out local16);
          double result = 0.0;
          double.TryParse(s, out result);
          double num15 = Math.Floor(result);
          double num16 = Math.Floor(result % 1.0 * 8.0);
          string str = num15 == 0.0 && num16 == 0.0 || num16 == 0.0 ? this.addLegLength(feet, num15.ToString()) : (num15 != 0.0 ? this.addLegLength(feet, $"{num15.ToString()} {FeetAndInchesRounding.DecimalToFraction(num16 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num16 / 8.0)));
          rebarDefintion.rebarG = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarH.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarH.Trim()) && rebarDefintion.rebarH.Trim() != "null")
        {
          string rebarH = rebarDefintion.rebarH;
          string valueString = rebarH.Substring(0, rebarH.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local17 = ref feet;
          ref string local18 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local17, out local18);
          double result = 0.0;
          double.TryParse(s, out result);
          double num17 = Math.Floor(result);
          double num18 = Math.Floor(result % 1.0 * 8.0);
          string str = num17 == 0.0 && num18 == 0.0 || num18 == 0.0 ? this.addLegLength(feet, num17.ToString()) : (num17 != 0.0 ? this.addLegLength(feet, $"{num17.ToString()} {FeetAndInchesRounding.DecimalToFraction(num18 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num18 / 8.0)));
          rebarDefintion.rebarH = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarJ.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarJ.Trim()) && rebarDefintion.rebarJ.Trim() != "null")
        {
          string rebarJ = rebarDefintion.rebarJ;
          string valueString = rebarJ.Substring(0, rebarJ.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local19 = ref feet;
          ref string local20 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local19, out local20);
          double result = 0.0;
          double.TryParse(s, out result);
          double num19 = Math.Floor(result);
          double num20 = Math.Floor(result % 1.0 * 8.0);
          string str = num19 == 0.0 && num20 == 0.0 || num20 == 0.0 ? this.addLegLength(feet, num19.ToString()) : (num19 != 0.0 ? this.addLegLength(feet, $"{num19.ToString()} {FeetAndInchesRounding.DecimalToFraction(num20 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num20 / 8.0)));
          rebarDefintion.rebarJ = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarK.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarK.Trim()) && rebarDefintion.rebarK.Trim() != "null")
        {
          string rebarK = rebarDefintion.rebarK;
          string valueString = rebarK.Substring(0, rebarK.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local21 = ref feet;
          ref string local22 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local21, out local22);
          double result = 0.0;
          double.TryParse(s, out result);
          double num21 = Math.Floor(result);
          double num22 = Math.Floor(result % 1.0 * 8.0);
          string str = num21 == 0.0 && num22 == 0.0 || num22 == 0.0 ? this.addLegLength(feet, num21.ToString()) : (num21 != 0.0 ? this.addLegLength(feet, $"{num21.ToString()} {FeetAndInchesRounding.DecimalToFraction(num22 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num22 / 8.0)));
          rebarDefintion.rebarK = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
        if (rebarDefintion.rebarO.Contains("mm") && !string.IsNullOrEmpty(rebarDefintion.rebarO.Trim()) && rebarDefintion.rebarO.Trim() != "null")
        {
          string rebarO = rebarDefintion.rebarO;
          string valueString = rebarO.Substring(0, rebarO.IndexOf("mm"));
          string feet = "";
          string s = "";
          ref string local23 = ref feet;
          ref string local24 = ref s;
          UnitConversion.millimetersToFeetAndInchesString(valueString, out local23, out local24);
          double result = 0.0;
          double.TryParse(s, out result);
          double num23 = Math.Floor(result);
          double num24 = Math.Floor(result % 1.0 * 8.0);
          string str = num23 == 0.0 && num24 == 0.0 || num24 == 0.0 ? this.addLegLength(feet, num23.ToString()) : (num23 != 0.0 ? this.addLegLength(feet, $"{num23.ToString()} {FeetAndInchesRounding.DecimalToFraction(num24 / 8.0)}") : this.addLegLength(feet, FeetAndInchesRounding.DecimalToFraction(num24 / 8.0)));
          rebarDefintion.rebarO = str;
          if (!stringList.Contains(rebarDefintion.rebarMark))
            stringList.Add(rebarDefintion.rebarMark);
        }
      }
    }
    if (stringList.Count > 0)
    {
      int num = 1;
      TaskDialog taskDialog1 = new TaskDialog("Standard Bar Definitions changed");
      taskDialog1.MainContent = "One or more Standard Bar Definitions were changed to match the Unit settings. All conversions are rounded down to nearest millimeter or 1/8th inch. Should be noted that the Bar Diameter conversion may not result in a standard bar diameter. Please consult settings and click apply to save new definitions.";
      taskDialog1.ExpandedContent = "Effected Bar Marks: ";
      foreach (string str in stringList)
      {
        if (num < 2)
        {
          taskDialog1.ExpandedContent += str;
        }
        else
        {
          TaskDialog taskDialog2 = taskDialog1;
          taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}, {str}";
        }
        ++num;
      }
      taskDialog1.Show();
    }
    return rebarDefintions;
  }

  private bool checkStandardBarSave(List<StandardRebarItem> rebarDefinitions)
  {
    List<string> stringList = new List<string>();
    foreach (StandardRebarItem rebarDefinition in rebarDefinitions)
    {
      if (this.useMetricEuroBool)
      {
        if (BarDiameterOracle.BarDiameterToBarNumberEuropean(rebarDefinition.rebarDiameter) == " " && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarA) || !rebarDefinition.rebarA.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarA) && rebarDefinition.rebarA.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarB) || !rebarDefinition.rebarB.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarB) && rebarDefinition.rebarB.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarC) || !rebarDefinition.rebarC.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarC) && rebarDefinition.rebarC.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarD) || !rebarDefinition.rebarD.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarD) && rebarDefinition.rebarD.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarE) || !rebarDefinition.rebarE.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarE) && rebarDefinition.rebarE.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarF) || !rebarDefinition.rebarF.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarF) && rebarDefinition.rebarF.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarG) || !rebarDefinition.rebarG.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarG) && rebarDefinition.rebarG.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarH) || !rebarDefinition.rebarH.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarH) && rebarDefinition.rebarH.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarJ) || !rebarDefinition.rebarJ.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarJ) && rebarDefinition.rebarJ.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarK) || !rebarDefinition.rebarK.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarK) && rebarDefinition.rebarK.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((!this.checkMetricInput(rebarDefinition.rebarO) || !rebarDefinition.rebarO.Contains("mm")) && !string.IsNullOrWhiteSpace(rebarDefinition.rebarO) && rebarDefinition.rebarO.Trim() != "null" && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
      }
      else
      {
        if (BarDiameterOracle.BarDiameterToBarNumber(rebarDefinition.rebarDiameter) == " " && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarA) || rebarDefinition.rebarA.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarB) || rebarDefinition.rebarB.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarC) || rebarDefinition.rebarC.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarD) || rebarDefinition.rebarD.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarE) || rebarDefinition.rebarE.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarF) || rebarDefinition.rebarF.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarG) || rebarDefinition.rebarG.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarH) || rebarDefinition.rebarH.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarJ) || rebarDefinition.rebarJ.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarK) || rebarDefinition.rebarK.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
        if ((this.checkMetricInput(rebarDefinition.rebarO) || rebarDefinition.rebarO.Contains("mm")) && !stringList.Contains(rebarDefinition.rebarMark))
          stringList.Add(rebarDefinition.rebarMark);
      }
    }
    if (stringList.Count <= 0)
      return true;
    int num = 1;
    TaskDialog taskDialog1 = new TaskDialog("Standard Bar Definitions changed");
    taskDialog1.MainContent = "One or more Standard Definition(s) located in the standard rebar settings are invalid. Once corrected click apply changed again.";
    taskDialog1.ExpandedContent = "Invalid Bar Marks: ";
    foreach (string str in stringList)
    {
      if (num < 2)
      {
        taskDialog1.ExpandedContent += str;
      }
      else
      {
        TaskDialog taskDialog2 = taskDialog1;
        taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}, {str}";
      }
      ++num;
    }
    taskDialog1.Show();
    return false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/usersettingtools/views/rebarusersettingwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((Window) target).Activated += new EventHandler(this.App_Activated);
        break;
      case 2:
        this.Setting = (ListBox) target;
        this.Setting.SelectionChanged += new SelectionChangedEventHandler(this.Setting_Selected);
        break;
      case 3:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.ApplyButton_Click);
        break;
      case 4:
        this.GeneralSettingPanelBorder = (Border) target;
        break;
      case 5:
        this.GeneralSettingPanel = (System.Windows.Controls.Grid) target;
        break;
      case 6:
        this.manufacturer = (TextBox) target;
        break;
      case 7:
        this.UseMetricLabel = (TextBlock) target;
        break;
      case 8:
        this.useMetricCanadian = (CheckBox) target;
        this.useMetricCanadian.Checked += new RoutedEventHandler(this.useMetricEuropean_Checked);
        this.useMetricCanadian.Unchecked += new RoutedEventHandler(this.useMetricEuropean_Unchecked);
        break;
      case 9:
        this.markPrefix = (TextBox) target;
        break;
      case 10:
        this.markSuffix = (TextBox) target;
        break;
      case 11:
        this.padBarSizesWithLeadingZero = (CheckBox) target;
        this.padBarSizesWithLeadingZero.Checked += new RoutedEventHandler(this.padBarSizesWithLeadingZero_Checked);
        this.padBarSizesWithLeadingZero.Unchecked += new RoutedEventHandler(this.padBarSizesWithLeadingZero_Unchecked);
        break;
      case 12:
        this.markPrefixIntMinValue = (TextBox) target;
        break;
      case 13:
        this.markFinishGalvanized = (TextBox) target;
        break;
      case 14:
        this.markFinishEpoxy = (TextBox) target;
        break;
      case 15:
        this.StraightBarSettingPanelBorder = (Border) target;
        break;
      case 16 /*0x10*/:
        this.StraightBarSettingPanel = (System.Windows.Controls.Grid) target;
        break;
      case 17:
        this.straightBarMarkPrefix = (TextBox) target;
        break;
      case 18:
        this.straightBarLengthOption = (ComboBox) target;
        this.straightBarLengthOption.Loaded += new RoutedEventHandler(this.straightBarLengthOption_Loaded);
        this.straightBarLengthOption.SelectionChanged += new SelectionChangedEventHandler(this.straightBarLengthOption_Selected);
        break;
      case 19:
        this.ToolTipLine1 = (TextBlock) target;
        break;
      case 20:
        this.ToolTipLine2 = (TextBlock) target;
        break;
      case 21:
        this.ToolTipLine3 = (TextBlock) target;
        break;
      case 22:
        this.ToolTipLine4 = (TextBlock) target;
        break;
      case 23:
        this.straightBarLengthZeroPaddingTextblock = (TextBlock) target;
        break;
      case 24:
        this.straightBarLengthZeroPadding = (TextBox) target;
        break;
      case 25:
        this.StandardBarSettingPanelBorder = (Border) target;
        break;
      case 26:
        this.StandardBarSettingPanel = (System.Windows.Controls.Grid) target;
        break;
      case 27:
        this.DeleteItem = (Button) target;
        this.DeleteItem.Click += new RoutedEventHandler(this.DeleteItem_Click);
        break;
      case 28:
        this.existingSettingNotUseMetric = (DataGrid) target;
        this.existingSettingNotUseMetric.SelectionChanged += new SelectionChangedEventHandler(this.NotUseMetricDataGrid_SelectionChanged);
        this.existingSettingNotUseMetric.MouseDown += new MouseButtonEventHandler(this.DataGrid_MouseDown);
        this.existingSettingNotUseMetric.TargetUpdated += new EventHandler<DataTransferEventArgs>(this.existingSettingNotUseMetric_TargetUpdated);
        break;
      case 29:
        this.editingItemBorder = (Border) target;
        break;
      case 30:
        this.editingItemPanelNotUseMatric = (System.Windows.Controls.Grid) target;
        break;
      case 31 /*0x1F*/:
        this.barSizeBoxNotUseMetric = (ComboBox) target;
        this.barSizeBoxNotUseMetric.Loaded += new RoutedEventHandler(this.barSizeBoxNotUseMetric_Loaded);
        break;
      case 32 /*0x20*/:
        this.ShapTextBoxNotUseMetric = (TextBox) target;
        break;
      case 33:
        this.ShapeButtonNotUseMetric = (Button) target;
        this.ShapeButtonNotUseMetric.Click += new RoutedEventHandler(this.ShapeButton_Click);
        break;
      case 34:
        this.MarkTextBoxNotUseMetric = (TextBox) target;
        break;
      case 35:
        this.ATextBoxFeet = (TextBox) target;
        break;
      case 36:
        this.ALabelFeet = (TextBlock) target;
        break;
      case 37:
        this.ATextBoxInch = (TextBox) target;
        break;
      case 38:
        this.ALabelInch = (TextBlock) target;
        break;
      case 39:
        this.BTextBoxFeet = (TextBox) target;
        break;
      case 40:
        this.BLabelFeet = (TextBlock) target;
        break;
      case 41:
        this.BTextBoxInch = (TextBox) target;
        break;
      case 42:
        this.BLabelInch = (TextBlock) target;
        break;
      case 43:
        this.CTextBoxFeet = (TextBox) target;
        break;
      case 44:
        this.CLabelFeet = (TextBlock) target;
        break;
      case 45:
        this.CTextBoxInch = (TextBox) target;
        break;
      case 46:
        this.CLabelInch = (TextBlock) target;
        break;
      case 47:
        this.DTextBoxFeet = (TextBox) target;
        break;
      case 48 /*0x30*/:
        this.DLabelFeet = (TextBlock) target;
        break;
      case 49:
        this.DTextBoxInch = (TextBox) target;
        break;
      case 50:
        this.DLabelInch = (TextBlock) target;
        break;
      case 51:
        this.ETextBoxFeet = (TextBox) target;
        break;
      case 52:
        this.ELabelFeet = (TextBlock) target;
        break;
      case 53:
        this.ETextBoxInch = (TextBox) target;
        break;
      case 54:
        this.ELabelInch = (TextBlock) target;
        break;
      case 55:
        this.FTextBoxFeet = (TextBox) target;
        break;
      case 56:
        this.FLabelFeet = (TextBlock) target;
        break;
      case 57:
        this.FTextBoxInch = (TextBox) target;
        break;
      case 58:
        this.FLabelInch = (TextBlock) target;
        break;
      case 59:
        this.GTextBoxFeet = (TextBox) target;
        break;
      case 60:
        this.GLabelFeet = (TextBlock) target;
        break;
      case 61:
        this.GTextBoxInch = (TextBox) target;
        break;
      case 62:
        this.GLabelInch = (TextBlock) target;
        break;
      case 63 /*0x3F*/:
        this.HTextBoxFeet = (TextBox) target;
        break;
      case 64 /*0x40*/:
        this.HLabelFeet = (TextBlock) target;
        break;
      case 65:
        this.HTextBoxInch = (TextBox) target;
        break;
      case 66:
        this.HLabelInch = (TextBlock) target;
        break;
      case 67:
        this.JTextBoxFeet = (TextBox) target;
        break;
      case 68:
        this.JLabelFeet = (TextBlock) target;
        break;
      case 69:
        this.JTextBoxInch = (TextBox) target;
        break;
      case 70:
        this.JLabelInch = (TextBlock) target;
        break;
      case 71:
        this.KTextBoxFeet = (TextBox) target;
        break;
      case 72:
        this.KLabelFeet = (TextBlock) target;
        break;
      case 73:
        this.KTextBoxInch = (TextBox) target;
        break;
      case 74:
        this.KLabelInch = (TextBlock) target;
        break;
      case 75:
        this.OTextBoxFeet = (TextBox) target;
        break;
      case 76:
        this.OLabelFeet = (TextBlock) target;
        break;
      case 77:
        this.OTextBoxInch = (TextBox) target;
        break;
      case 78:
        this.OLabelInch = (TextBlock) target;
        break;
      case 79:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.SaveAndAddButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
