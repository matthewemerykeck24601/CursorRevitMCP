// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ControlNumberIncrementorTools.ControlNumberIncrementorWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

#nullable disable
namespace EDGE.SchedulingTools.ControlNumberIncrementorTools;

public class ControlNumberIncrementorWindow : Window, IComponentConnector
{
  public string prefix = string.Empty;
  public string suffix = string.Empty;
  public string controlNumber = string.Empty;
  private bool is4Digit;
  internal CheckBox chkBoxPrefix;
  internal TextBox txtBoxPrefix;
  internal CheckBox chkBoxSuffix;
  internal TextBox txtBoxSuffix;
  internal RadioButton radio3Digit;
  internal RadioButton radio4Digit;
  internal TextBox txtBoxControlNumber;
  internal Button ContinueButton;
  private bool _contentLoaded;

  public ControlNumberIncrementorWindow(
    IntPtr parentWindowHandler,
    string startingNum = "000",
    string prefix = "",
    string suffix = "")
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    if (string.IsNullOrWhiteSpace(startingNum))
      startingNum = "000";
    startingNum = startingNum.Trim();
    if (startingNum.Length > 3 || startingNum == "999")
      this.is4Digit = true;
    this.radio4Digit.IsChecked = new bool?(this.is4Digit);
    if (!string.IsNullOrEmpty(startingNum))
    {
      int num1 = int.Parse(startingNum);
      int num2 = this.is4Digit ? 4 : 3;
      string empty = string.Empty;
      for (int index = 1; index <= num2; ++index)
        empty += "0";
      this.txtBoxControlNumber.Text = (num1 + 1).ToString(empty);
    }
    if (!string.IsNullOrEmpty(prefix))
    {
      this.chkBoxPrefix.IsChecked = new bool?(true);
      this.txtBoxPrefix.Text = prefix;
    }
    if (!string.IsNullOrEmpty(suffix))
    {
      this.chkBoxSuffix.IsChecked = new bool?(true);
      this.txtBoxSuffix.Text = suffix;
    }
    this.txtBoxControlNumber.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
    this.txtBoxPrefix.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
    this.txtBoxPrefix.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
  }

  private bool ValidateText(string text)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return string.IsNullOrEmpty(string.Concat<char>(text.Where<char>(ControlNumberIncrementorWindow.\u003C\u003EO.\u003C0\u003E__IsDigit ?? (ControlNumberIncrementorWindow.\u003C\u003EO.\u003C0\u003E__IsDigit = new Func<char, bool>(char.IsDigit)))));
  }

  private bool ValidateNumber(string cnValue)
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    return string.Concat<char>(cnValue.Where<char>(ControlNumberIncrementorWindow.\u003C\u003EO.\u003C0\u003E__IsDigit ?? (ControlNumberIncrementorWindow.\u003C\u003EO.\u003C0\u003E__IsDigit = new Func<char, bool>(char.IsDigit)))).Length == cnValue.Length;
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    int totalWidth = this.radio4Digit.IsChecked.Value ? 4 : 3;
    if (!this.ValidateNumber(this.txtBoxControlNumber.Text))
    {
      this.txtBoxControlNumber.Focus();
      TaskDialog.Show("Invlid Control Number", "Control number must only contain digits.");
    }
    else
    {
      if (string.IsNullOrEmpty(this.txtBoxControlNumber.Text))
        this.txtBoxControlNumber.Text = "1";
      this.controlNumber = this.txtBoxControlNumber.Text.PadLeft(totalWidth, '0');
      if (this.txtBoxPrefix.IsEnabled)
      {
        if (!this.ValidateText(this.txtBoxPrefix.Text))
        {
          this.txtBoxPrefix.Focus();
          TaskDialog.Show("Invlid Prefix", "Prefix must not contain numbers.");
          return;
        }
        this.prefix = this.txtBoxPrefix.Text;
      }
      if (this.txtBoxSuffix.IsEnabled)
      {
        if (!this.ValidateText(this.txtBoxSuffix.Text))
        {
          this.txtBoxSuffix.Focus();
          TaskDialog.Show("Invlid Suffix", "Suffix must not contain numbers.");
          return;
        }
        this.suffix = this.txtBoxSuffix.Text;
      }
      this.Close();
    }
  }

  private void chkBox_Checked(object sender, RoutedEventArgs e)
  {
    this.txtBoxPrefix.IsEnabled = this.chkBoxPrefix.IsChecked.Value;
    this.txtBoxSuffix.IsEnabled = this.chkBoxSuffix.IsChecked.Value;
  }

  private void txtBoxControlNumber_LostFocus(object sender, RoutedEventArgs e)
  {
    if (!this.ValidateNumber(this.txtBoxControlNumber.Text))
    {
      this.txtBoxControlNumber.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb(byte.MaxValue, (byte) 0, (byte) 0));
    }
    else
    {
      this.txtBoxControlNumber.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
      string s = this.txtBoxControlNumber.Text;
      if (string.IsNullOrWhiteSpace(this.txtBoxControlNumber.Text))
        s = "1";
      int result;
      if (int.TryParse(s, out result))
        this.txtBoxControlNumber.Text = string.Format(this.is4Digit ? "{0:0000}" : "{0:000}", (object) result);
      else
        this.txtBoxControlNumber.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb(byte.MaxValue, (byte) 0, (byte) 0));
    }
  }

  private void txtBoxPrefix_LostFocus(object sender, RoutedEventArgs e)
  {
    this.txtBoxPrefix.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
    if (this.ValidateText(this.txtBoxPrefix.Text))
      return;
    this.txtBoxPrefix.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb(byte.MaxValue, (byte) 0, (byte) 0));
  }

  private void radio_Checked(object sender, RoutedEventArgs e)
  {
    this.is4Digit = !this.radio3Digit.IsChecked.GetValueOrDefault();
    if (this.txtBoxControlNumber == null || !this.ValidateNumber(this.txtBoxControlNumber.Text))
      return;
    string s = this.txtBoxControlNumber.Text;
    if (string.IsNullOrWhiteSpace(this.txtBoxControlNumber.Text))
      s = "1";
    int result;
    if (!int.TryParse(s, out result))
      return;
    this.txtBoxControlNumber.Text = string.Format(this.is4Digit ? "{0:0000}" : "{0:000}", (object) result);
  }

  private void txtBoxSuffix_LostFocus(object sender, RoutedEventArgs e)
  {
    this.txtBoxSuffix.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb((byte) 128 /*0x80*/, (byte) 128 /*0x80*/, (byte) 128 /*0x80*/));
    if (this.ValidateText(this.txtBoxSuffix.Text))
      return;
    this.txtBoxSuffix.BorderBrush = (Brush) new SolidColorBrush(Color.FromRgb(byte.MaxValue, (byte) 0, (byte) 0));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/schedulingtools/controlnumberincrementortools/controlnumberincrementor.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.chkBoxPrefix = (CheckBox) target;
        this.chkBoxPrefix.Checked += new RoutedEventHandler(this.chkBox_Checked);
        this.chkBoxPrefix.Unchecked += new RoutedEventHandler(this.chkBox_Checked);
        break;
      case 2:
        this.txtBoxPrefix = (TextBox) target;
        this.txtBoxPrefix.LostFocus += new RoutedEventHandler(this.txtBoxPrefix_LostFocus);
        break;
      case 3:
        this.chkBoxSuffix = (CheckBox) target;
        this.chkBoxSuffix.Checked += new RoutedEventHandler(this.chkBox_Checked);
        this.chkBoxSuffix.Unchecked += new RoutedEventHandler(this.chkBox_Checked);
        break;
      case 4:
        this.txtBoxSuffix = (TextBox) target;
        this.txtBoxSuffix.LostFocus += new RoutedEventHandler(this.txtBoxSuffix_LostFocus);
        break;
      case 5:
        this.radio3Digit = (RadioButton) target;
        this.radio3Digit.Checked += new RoutedEventHandler(this.radio_Checked);
        break;
      case 6:
        this.radio4Digit = (RadioButton) target;
        this.radio4Digit.Checked += new RoutedEventHandler(this.radio_Checked);
        break;
      case 7:
        this.txtBoxControlNumber = (TextBox) target;
        this.txtBoxControlNumber.LostFocus += new RoutedEventHandler(this.txtBoxControlNumber_LostFocus);
        break;
      case 8:
        this.ContinueButton = (Button) target;
        this.ContinueButton.Click += new RoutedEventHandler(this.ContinueButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
