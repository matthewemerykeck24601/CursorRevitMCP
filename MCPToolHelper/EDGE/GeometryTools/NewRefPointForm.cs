// Decompiled with JetBrains decompiler
// Type: EDGE.GeometryTools.NewRefPointForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;

#nullable disable
namespace EDGE.GeometryTools;

public class NewRefPointForm : System.Windows.Forms.Form
{
  public Action<string, string, string> ExportModel;
  public static double xValue;
  public static double yValue;
  public static double zValue;
  public static string xTextFt;
  public static string yTextFt;
  public static string zTextFt;
  public static string xTextIn;
  public static string yTextIn;
  public static string zTextIn;
  public XYZ newXYZ;
  private XYZ newRefPt;
  public bool xNullFlag;
  public bool yNullFlag;
  public bool zNullFlag;
  private bool useImperialUnits = true;
  private IContainer components;
  private TextBox xTextBox;
  private Label label1;
  private Label label2;
  private TextBox yTextBox;
  private Label label3;
  private TextBox zTextBox;
  private Button refUpdate;
  private Label label4;
  private Label label5;
  private Button button1;
  private Label label6;
  private Label label7;
  private Label label8;
  private Label label9;

  public NewRefPointForm(bool noZ, string planeName, bool imperialUnits)
  {
    this.InitializeComponent();
    this.FormClosing += new FormClosingEventHandler(this.NewRefPointForm_FormClosing);
    this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.enter_KeyPress);
    if (!planeName.Equals(""))
      this.label9.Text = planeName;
    else
      this.label9.Visible = false;
    if (noZ)
      this.zTextBox.Enabled = false;
    this.useImperialUnits = imperialUnits;
    if (imperialUnits)
      return;
    this.label5.Text = "Enter offset XYZ values in millimeters. \r\nAny axes left blank will not be moved on upon that axis.\r\n";
  }

  public NewRefPointForm(XYZ newRefPt)
  {
    this.InitializeComponent();
    this.newRefPt = newRefPt;
    this.FormClosing += new FormClosingEventHandler(this.NewRefPointForm_FormClosing);
  }

  private void displayRefPointValues(string message)
  {
    message = this.newRefPt.ToString();
    this.label4.Text = "Reference Point Coordinates: " + message;
  }

  private void enter_KeyPress(object sender, System.Windows.Forms.KeyEventArgs e)
  {
    if (!Keyboard.IsKeyDown(Key.Return))
      return;
    this.refUpdate_Click_1(sender, (EventArgs) e);
  }

  private void NewRefPointForm_Load(object sender, EventArgs e)
  {
  }

  private void NewRefPointForm_FormClosing(object sender, FormClosingEventArgs e)
  {
    if (MessageBox.Show("Do you want to exit? No offset values will be entered", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes)
      this.newXYZ = new XYZ(0.0, 0.0, 0.0);
    else
      e.Cancel = true;
  }

  private void NewRefPointForm_FormClosingInvInput(object sender, FormClosingEventArgs e)
  {
    if (MessageBox.Show("Invalid Input detected. Do you wish to continue entering offset values?", "Invalid Input", MessageBoxButtons.YesNo) == DialogResult.No)
      this.newXYZ = new XYZ(0.0, 0.0, 0.0);
    else
      e.Cancel = true;
  }

  private void NewRefPointForm_Cancel(object sender, EventArgs e)
  {
    this.newXYZ = new XYZ(0.0, 0.0, 0.0);
    this.Close();
  }

  private void refUpdate_Click_1(object sender, EventArgs e)
  {
    this.FormClosing -= new FormClosingEventHandler(this.NewRefPointForm_FormClosing);
    this.label6.Visible = false;
    this.label7.Visible = false;
    this.label8.Visible = false;
    NewRefPointForm.xTextFt = this.xTextBox.Text;
    NewRefPointForm.yTextFt = this.yTextBox.Text;
    NewRefPointForm.zTextFt = this.zTextBox.Text;
    if (this.useImperialUnits)
    {
      NewRefPointForm.xTextFt = this.ConvertInputString(NewRefPointForm.xTextFt);
      NewRefPointForm.yTextFt = this.ConvertInputString(NewRefPointForm.yTextFt);
      NewRefPointForm.zTextFt = this.ConvertInputString(NewRefPointForm.zTextFt);
    }
    else
    {
      NewRefPointForm.xTextFt = this.ConvertInputStringMetric(NewRefPointForm.xTextFt);
      NewRefPointForm.yTextFt = this.ConvertInputStringMetric(NewRefPointForm.yTextFt);
      NewRefPointForm.zTextFt = this.ConvertInputStringMetric(NewRefPointForm.zTextFt);
    }
    bool flag = false;
    if (NewRefPointForm.xTextFt.Equals("Invalid Input"))
    {
      this.label6.Visible = true;
      flag = true;
    }
    if (NewRefPointForm.yTextFt.Equals("Invalid Input"))
    {
      this.label7.Visible = true;
      flag = true;
    }
    if (NewRefPointForm.zTextFt.Equals("Invalid Input"))
    {
      this.label8.Visible = true;
      flag = true;
    }
    if (flag)
    {
      this.FormClosing += new FormClosingEventHandler(this.NewRefPointForm_FormClosing);
    }
    else
    {
      if (NewRefPointForm.xTextFt.Equals(""))
        this.xNullFlag = true;
      if (NewRefPointForm.yTextFt.Equals(""))
        this.yNullFlag = true;
      if (NewRefPointForm.zTextFt.Equals(""))
        this.zNullFlag = true;
      NewRefPointForm.xValue = this.StringToDouble(NewRefPointForm.xTextFt);
      NewRefPointForm.yValue = this.StringToDouble(NewRefPointForm.yTextFt);
      NewRefPointForm.zValue = this.StringToDouble(NewRefPointForm.zTextFt);
      this.newXYZ = new XYZ(NewRefPointForm.xValue, NewRefPointForm.yValue, NewRefPointForm.zValue);
      int num = (int) MessageBox.Show("Offset values entered. Moving element.");
      this.Close();
    }
  }

  private string ConvertInputString(string fraction)
  {
    fraction = fraction.Trim();
    string str1 = (string) null;
    Regex regex1 = new Regex("^-?\\d+\\s+-?\\d+\\s+\\d+[/]?\\d+$");
    Regex regex2 = new Regex("^-?\\d+[']\\s+-?\\d+\\s+\\d+[/]?\\d+[\"]$");
    Regex regex3 = new Regex("^-?\\d+['][-]\\d+\\s+\\d+[/]?\\d+[\"]$");
    Regex regex4 = new Regex("^-?\\d+[']\\d+\\s+\\d+[/]?\\d+[\"]$");
    Regex regex5 = new Regex("^-?\\d+[-]\\d+\\s+\\d+[/]?\\d+$");
    Regex regex6 = new Regex("^-?\\d+\\s+-?\\d+[.]?\\d+$");
    Regex regex7 = new Regex("^\\d+[-]\\d+[.]?\\d+$");
    Regex regex8 = new Regex("^[-]\\d+[-]\\d+[.]?\\d+$");
    Regex regex9 = new Regex("^-?\\d+[']\\s+-?\\d+[.]?\\d+[\"]$");
    Regex regex10 = new Regex("^\\d+['][-]-?\\d+[.]?\\d+[\"]$");
    Regex regex11 = new Regex("^[-]\\d+['][-]-?\\d+[.]?\\d+[\"]$");
    Regex regex12 = new Regex("^-?\\d+[']\\d+[.]?\\d+[\"]$");
    Regex regex13 = new Regex("^-?\\d+\\s+-?\\d+/\\d+$");
    Regex regex14 = new Regex("^-?\\d+/\\d+$");
    Regex regex15 = new Regex("^-?\\d+/\\d+[']$");
    Regex regex16 = new Regex("^-?\\d+/\\d+[\"]$");
    Regex regex17 = new Regex("^-?\\d+/\\d+\\s+-?\\d+/\\d+$");
    Regex regex18 = new Regex("^-?\\d+/\\d+[']\\s+-?\\d+/\\d+[\"]$");
    Regex regex19 = new Regex("^-?\\d+\\s+\\d+/\\d+\\s+-?\\d+\\s+\\d+/\\d+$");
    Regex regex20 = new Regex("^-?\\d+\\s+\\d+/\\d+[']\\s+-?\\d+\\s+\\d+/\\d+[\"]$");
    Regex regex21 = new Regex("^-?\\d+\\s+\\d+/\\d+\\s+-?\\d+/\\d+$");
    Regex regex22 = new Regex("^-?\\d+\\s+\\d+/\\d+[']\\s+-?\\d+/\\d+[\"]$");
    Regex regex23 = new Regex("^-?\\d+[.]\\d+$");
    Regex regex24 = new Regex("^-?[.]\\d+$");
    Regex regex25 = new Regex("^-?[.]\\d+[']$");
    Regex regex26 = new Regex("^-?\\d+[.]\\d+[']$");
    Regex regex27 = new Regex("^-?[.]\\d+[\"]$");
    Regex regex28 = new Regex("^-?\\d+[.]\\d+[\"]$");
    Regex regex29 = new Regex("^-?\\d+[']\\s+-?\\[.]?\\d+[\"]$");
    Regex regex30 = new Regex("^-?\\d+$");
    Regex regex31 = new Regex("^-?\\d+\\s+-?\\d+$");
    Regex regex32 = new Regex("^-?\\d+[']\\s+-?\\d+[\"]$");
    Regex regex33 = new Regex("^-?\\d+[-]\\d+$");
    Regex regex34 = new Regex("^-?\\d+[']$");
    Regex regex35 = new Regex("^-?\\d+[\"]$");
    Regex regex36 = new Regex("^[-][0][']\\s+-?\\d+[\"]$");
    Regex regex37 = new Regex("^[-][0][']\\s+-?\\d+[.]\\d+[\"]$");
    Regex regex38 = new Regex("^[-][0][']\\s+-?\\d+/\\d+[\"]$");
    Regex regex39 = new Regex("^[-][0]\\s+-?\\d+$");
    Regex regex40 = new Regex("^[-][0]\\s+-?\\d+[.]\\d+$");
    Regex regex41 = new Regex("^[-][0]\\s+-?\\d+/\\d+$");
    Regex regex42 = new Regex("^[-]\\d+[']\\s+-?\\d+[\"]$");
    Regex regex43 = new Regex("^[-]\\d+[']\\s+-?\\d+[.]\\d+[\"]$");
    Regex regex44 = new Regex("^[-]\\d+[']\\s+-?\\d+/\\d+[\"]$");
    Regex regex45 = new Regex("^[-]\\d+\\s+-?\\d+$");
    Regex regex46 = new Regex("^[-]\\d+\\s+-?\\d+[.]\\d+$");
    Regex regex47 = new Regex("^[-]\\d+\\s+-?\\d+/\\d+$");
    Regex regex48 = new Regex("^-?\\d+\\s+-?[.]\\d+$");
    Regex regex49 = new Regex("^-?\\d+['][.]\\d+[\"]$");
    Regex regex50 = new Regex("^\\d+[-][.]\\d+$");
    Regex regex51 = new Regex("^[-]\\d+[-][.]\\d+$");
    Regex regex52 = new Regex("^\\d+['][-][.]\\d+[\"]$");
    Regex regex53 = new Regex("^[-]\\d+['][-][.]\\d+[\"]$");
    Regex regex54 = new Regex("^-?\\d+[']-?[.]\\d+$");
    Regex regex55 = new Regex("^-?\\d+[']\\s+-?[.]\\d+$");
    Regex regex56 = new Regex("^-?\\d+[']-?\\d+[\"]$");
    Regex regex57 = new Regex("^-?\\d+[']-?\\d+$");
    Regex regex58 = new Regex("^-?\\d+[']\\s+-?\\d+$");
    Regex regex59 = new Regex("^-?\\d+[']\\s+-?[.]\\d+[\"]$");
    Regex regex60 = new Regex("^-?\\d+[']\\s+-?\\d+/\\d+[\"]$");
    Regex regex61 = new Regex("^-?\\d+[']\\d+/\\d+[\"]$");
    Regex regex62 = new Regex("^\\d+['][-]\\d+[\\/]\\d+[\"]$");
    Regex regex63 = new Regex("^[-]\\d+['][-]\\d+[\\/]\\d+[\"]$");
    Regex regex64 = new Regex("^\\d+[-]\\d+/\\d+$");
    Regex regex65 = new Regex("^[-]\\d+[-]\\d+/\\d+$");
    string input = fraction;
    if (regex1.IsMatch(input))
    {
      string[] strArray1 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s1 = strArray1[0];
      string s2 = strArray1[1];
      string str2 = strArray1[2];
      double result1;
      double.TryParse(s1, out result1);
      double result2;
      double.TryParse(s2, out result2);
      string[] separator = new string[1]{ "/" };
      string[] strArray2 = str2.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray2.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result3;
      double result4;
      if (double.TryParse(strArray2[0], out result3) && double.TryParse(strArray2[1], out result4))
      {
        if (result4 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = (result3 / result4 + result2) / 12.0;
          str1 = (result1 + num).ToString();
        }
      }
    }
    else if (regex36.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("0", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result * -1.0 / 12.0).ToString();
    }
    else if (regex37.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("0", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result * -1.0 / 12.0).ToString();
    }
    else if (regex38.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("0", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result5;
      double result6;
      if (double.TryParse(strArray[0], out result5) && double.TryParse(strArray[1], out result6))
        str1 = result6 != 0.0 ? (result5 / result6 * -1.0).ToString() : "Invalid Input";
    }
    else if (regex39.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("0", "");
      fraction = fraction.Replace(" ", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result / 12.0 * -1.0).ToString();
    }
    else if (regex40.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("0", "");
      fraction = fraction.Replace(" ", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result / 12.0 * -1.0).ToString();
    }
    else if (regex41.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("0", "");
      fraction = fraction.Replace(" ", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result7;
      double result8;
      if (double.TryParse(strArray[0], out result7) && double.TryParse(strArray[1], out result8))
        str1 = result8 != 0.0 ? (result7 / result8 / 12.0 * -1.0).ToString() : "Invalid Input";
    }
    else if (regex42.IsMatch(fraction) || regex46.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s3 = strArray[0];
      string s4 = strArray[1];
      double result9;
      double.TryParse(s3, out result9);
      double result10;
      double.TryParse(s4, out result10);
      double num = result10 / 12.0;
      str1 = ((result9 + num) * -1.0).ToString();
    }
    else if (regex44.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray3 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray3[0];
      string str3 = strArray3[1];
      double result11;
      double.TryParse(s, out result11);
      string[] strArray4 = str3.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray4.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result12;
      double result13;
      if (double.TryParse(strArray4[0], out result12) && double.TryParse(strArray4[1], out result13))
        str1 = result13 != 0.0 ? ((result12 / result13 / 12.0 + result11) * -1.0).ToString() : "Invalid Input";
    }
    else if (regex45.IsMatch(fraction) || regex46.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s5 = strArray[0];
      string s6 = strArray[1];
      double result14;
      double.TryParse(s5, out result14);
      double result15;
      double.TryParse(s6, out result15);
      double num = result15 / 12.0;
      str1 = ((result14 + num) * -1.0).ToString();
    }
    else if (regex47.IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      string[] strArray5 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray5[0];
      string str4 = strArray5[1];
      double result16;
      double.TryParse(s, out result16);
      string[] strArray6 = str4.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray6.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result17;
      double result18;
      if (double.TryParse(strArray6[0], out result17) && double.TryParse(strArray6[1], out result18))
        str1 = result18 != 0.0 ? ((result17 / result18 / 12.0 + result16) * -1.0).ToString() : "Invalid Input";
    }
    else if (regex48.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s7 = strArray[0];
      string s8 = strArray[1];
      double result19;
      double.TryParse(s7, out result19);
      double result20;
      double.TryParse(s8, out result20);
      double num = result20 / 12.0;
      if (fraction.ToLower().Contains("-"))
        num *= -1.0;
      str1 = (result19 + num).ToString();
    }
    else if (regex49.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", " ");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s9 = strArray[0];
      string s10 = strArray[1];
      double result21;
      double.TryParse(s9, out result21);
      double result22;
      double.TryParse(s10, out result22);
      double num = result22 / 12.0;
      if (fraction.ToLower().Contains("-"))
        num *= -1.0;
      str1 = (result21 + num).ToString();
    }
    else if (regex50.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s11 = strArray[0];
      string s12 = strArray[1];
      double result23;
      double.TryParse(s11, out result23);
      double result24;
      double.TryParse(s12, out result24);
      double num = result24 / 12.0;
      str1 = (result23 + num).ToString();
    }
    else if (regex51.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s13 = strArray[0];
      string s14 = strArray[1];
      double result25;
      double.TryParse(s13, out result25);
      double result26;
      double.TryParse(s14, out result26);
      double num = result26 / 12.0;
      str1 = ((result25 + num) * -1.0).ToString();
    }
    else if (regex52.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s15 = strArray[0];
      string s16 = strArray[1];
      double result27;
      double.TryParse(s15, out result27);
      double result28;
      double.TryParse(s16, out result28);
      double num = result28 / 12.0;
      str1 = (result27 + num).ToString();
    }
    else if (regex53.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s17 = strArray[0];
      string s18 = strArray[1];
      double result29;
      double.TryParse(s17, out result29);
      double result30;
      double.TryParse(s18, out result30);
      double num = result30 / 12.0;
      str1 = ((result29 + num) * -1.0).ToString();
    }
    else if (regex54.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", " ");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s19 = strArray[0];
      string s20 = strArray[1];
      double result31;
      double.TryParse(s19, out result31);
      double result32;
      double.TryParse(s20, out result32);
      double num = result32 / 12.0;
      str1 = (result31 + num).ToString();
    }
    else if (regex55.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s21 = strArray[0];
      string s22 = strArray[1];
      double result33;
      double.TryParse(s21, out result33);
      double result34;
      double.TryParse(s22, out result34);
      double num = result34 / 12.0;
      str1 = (result33 + num).ToString();
    }
    else if (regex56.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", " ");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s23 = strArray[0];
      string s24 = strArray[1];
      double result35;
      double.TryParse(s23, out result35);
      double result36;
      double.TryParse(s24, out result36);
      double num = result36 / 12.0;
      str1 = (result35 + num).ToString();
    }
    else if (regex57.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", " ");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s25 = strArray[0];
      string s26 = strArray[1];
      double result37;
      double.TryParse(s25, out result37);
      double result38;
      double.TryParse(s26, out result38);
      double num = result38 / 12.0;
      str1 = (result37 + num).ToString();
    }
    else if (regex58.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s27 = strArray[0];
      string s28 = strArray[1];
      double result39;
      double.TryParse(s27, out result39);
      double result40;
      double.TryParse(s28, out result40);
      double num = result40 / 12.0;
      str1 = (result39 + num).ToString();
    }
    else if (regex59.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s29 = strArray[0];
      string s30 = strArray[1];
      double result41;
      double.TryParse(s29, out result41);
      double result42;
      double.TryParse(s30, out result42);
      double num = result42 / 12.0;
      if (fraction.ToLower().Contains("-"))
        num *= -1.0;
      str1 = (result41 + num).ToString();
    }
    else if (regex60.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray7 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray7[0];
      string str5 = strArray7[1];
      double result43;
      double.TryParse(s, out result43);
      string[] strArray8 = str5.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray8.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result44;
      double result45;
      if (double.TryParse(strArray8[0], out result44) && double.TryParse(strArray8[1], out result45))
      {
        if (result45 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result44 / result45 / 12.0;
          str1 = (result43 + num).ToString();
        }
      }
    }
    else if (regex61.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", " ");
      string[] strArray9 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray9[0];
      string str6 = strArray9[1];
      double result46;
      double.TryParse(s, out result46);
      string[] strArray10 = str6.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray10.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result47;
      double result48;
      if (double.TryParse(strArray10[0], out result47) && double.TryParse(strArray10[1], out result48))
      {
        if (result48 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result47 / result48 / 12.0;
          str1 = (result46 + num).ToString();
        }
      }
    }
    else if (regex62.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray11 = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray11[0];
      string str7 = strArray11[1];
      double result49;
      double.TryParse(s, out result49);
      string[] strArray12 = str7.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray12.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result50;
      double result51;
      if (double.TryParse(strArray12[0], out result50) && double.TryParse(strArray12[1], out result51))
      {
        if (result51 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result50 / result51 / 12.0;
          str1 = (result49 + num).ToString();
        }
      }
    }
    else if (regex63.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray13 = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray13[0];
      string str8 = strArray13[1];
      double result52;
      double.TryParse(s, out result52);
      string[] strArray14 = str8.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray14.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result53;
      double result54;
      if (double.TryParse(strArray14[0], out result53) && double.TryParse(strArray14[1], out result54))
      {
        if (result54 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result53 / result54 / 12.0;
          str1 = ((result52 + num) * -1.0).ToString();
        }
      }
    }
    else if (regex64.IsMatch(fraction))
    {
      string[] strArray15 = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray15[0];
      string str9 = strArray15[1];
      double result55;
      double.TryParse(s, out result55);
      string[] strArray16 = str9.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray16.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result56;
      double result57;
      if (double.TryParse(strArray16[0], out result56) && double.TryParse(strArray16[1], out result57))
      {
        if (result57 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result56 / result57 / 12.0;
          str1 = (result55 + num).ToString();
        }
      }
    }
    else if (regex65.IsMatch(fraction))
    {
      string[] strArray17 = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray17[0];
      string str10 = strArray17[1];
      double result58;
      double.TryParse(s, out result58);
      string[] strArray18 = str10.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray18.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result59;
      double result60;
      if (double.TryParse(strArray18[0], out result59) && double.TryParse(strArray18[1], out result60))
      {
        if (result60 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result59 / result60 / 12.0;
          str1 = ((result58 + num) * -1.0).ToString();
        }
      }
    }
    else if (regex2.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray19 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s31 = strArray19[0];
      string s32 = strArray19[1];
      string str11 = strArray19[2];
      double result61;
      double.TryParse(s31, out result61);
      double result62;
      double.TryParse(s32, out result62);
      string[] separator = new string[1]{ "/" };
      string[] strArray20 = str11.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray20.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result63;
      double result64;
      if (double.TryParse(strArray20[0], out result63) && double.TryParse(strArray20[1], out result64))
      {
        if (result64 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = (result63 / result64 + result62) / 12.0;
          str1 = (result61 + num).ToString();
        }
      }
    }
    else if (regex3.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      fraction = fraction.Replace("-", " ");
      string[] strArray21 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s33 = strArray21[0];
      string s34 = strArray21[1];
      string str12 = strArray21[2];
      double result65;
      double.TryParse(s33, out result65);
      double result66;
      double.TryParse(s34, out result66);
      string[] separator = new string[1]{ "/" };
      string[] strArray22 = str12.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray22.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result67;
      double result68;
      if (double.TryParse(strArray22[0], out result67) && double.TryParse(strArray22[1], out result68))
      {
        if (result68 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = (result67 / result68 + result66) / 12.0;
          str1 = (result65 + num).ToString();
        }
      }
    }
    else if (regex4.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", " ");
      string[] strArray23 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s35 = strArray23[0];
      string s36 = strArray23[1];
      string str13 = strArray23[2];
      double result69;
      double.TryParse(s35, out result69);
      double result70;
      double.TryParse(s36, out result70);
      string[] separator = new string[1]{ "/" };
      string[] strArray24 = str13.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray24.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result71;
      double result72;
      if (double.TryParse(strArray24[0], out result71) && double.TryParse(strArray24[1], out result72))
      {
        if (result72 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = (result71 / result72 + result70) / 12.0;
          str1 = (result69 + num).ToString();
        }
      }
    }
    else if (regex5.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      fraction = fraction.Replace("-", " ");
      string[] strArray25 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s37 = strArray25[0];
      string s38 = strArray25[1];
      string str14 = strArray25[2];
      double result73;
      double.TryParse(s37, out result73);
      double result74;
      double.TryParse(s38, out result74);
      string[] separator = new string[1]{ "/" };
      string[] strArray26 = str14.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray26.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result75;
      double result76;
      if (double.TryParse(strArray26[0], out result75) && double.TryParse(strArray26[1], out result76))
      {
        if (result76 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = (result75 / result76 + result74) / 12.0;
          str1 = (result73 + num).ToString();
        }
      }
    }
    else if (regex6.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s39 = strArray[0];
      string s40 = strArray[1];
      double result77;
      double.TryParse(s39, out result77);
      double result78;
      double.TryParse(s40, out result78);
      double num = result78 / 12.0;
      str1 = (result77 + num).ToString();
    }
    else if (regex7.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s41 = strArray[0];
      string s42 = strArray[1];
      double result79;
      double.TryParse(s41, out result79);
      double result80;
      double.TryParse(s42, out result80);
      double num = result80 / 12.0;
      str1 = (result79 + num).ToString();
    }
    else if (regex8.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s43 = strArray[0];
      string s44 = strArray[1];
      double result81;
      double.TryParse(s43, out result81);
      double result82;
      double.TryParse(s44, out result82);
      double num = result82 / 12.0;
      str1 = ((result81 + num) * -1.0).ToString();
    }
    else if (regex33.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s45 = strArray[0];
      string s46 = strArray[1];
      double result83;
      double.TryParse(s45, out result83);
      double result84;
      double.TryParse(s46, out result84);
      double num = result84 / 12.0;
      str1 = (result83 + num).ToString();
    }
    else if (regex9.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s47 = strArray[0];
      string s48 = strArray[1];
      double result85;
      double.TryParse(s47, out result85);
      double result86;
      double.TryParse(s48, out result86);
      double num = result86 / 12.0;
      str1 = (result85 + num).ToString();
    }
    else if (regex10.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s49 = strArray[0];
      string s50 = strArray[1];
      double result87;
      double.TryParse(s49, out result87);
      double result88;
      double.TryParse(s50, out result88);
      double num = result88 / 12.0;
      str1 = (result87 + num).ToString();
    }
    else if (regex11.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s51 = strArray[0];
      string s52 = strArray[1];
      double result89;
      double.TryParse(s51, out result89);
      double result90;
      double.TryParse(s52, out result90);
      double num = result90 / 12.0;
      str1 = ((result89 + num) * -1.0).ToString();
    }
    else if (regex12.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", " ");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s53 = strArray[0];
      string s54 = strArray[1];
      double result91;
      double.TryParse(s53, out result91);
      double result92;
      double.TryParse(s54, out result92);
      double num = result92 / 12.0;
      str1 = (result91 + num).ToString();
    }
    else if (regex32.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", " ");
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s55 = strArray[0];
      string s56 = strArray[1];
      double result93;
      double.TryParse(s55, out result93);
      double result94;
      double.TryParse(s56, out result94);
      double num = result94 / 12.0;
      str1 = (result93 + num).ToString();
    }
    else if (regex34.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", "");
      str1 = fraction;
    }
    else if (regex35.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result / 12.0).ToString();
    }
    else if (regex26.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", "");
      str1 = fraction;
    }
    else if (regex24.IsMatch(fraction))
      str1 = fraction;
    else if (regex25.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", "");
      str1 = fraction;
    }
    else if (regex27.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result / 12.0).ToString();
    }
    else if (regex28.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      double result;
      double.TryParse(fraction, out result);
      str1 = (result / 12.0).ToString();
    }
    else if (regex15.IsMatch(fraction))
    {
      fraction = fraction.Replace("'", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result95;
      double result96;
      if (double.TryParse(strArray[0], out result95) && double.TryParse(strArray[1], out result96))
        str1 = result96 != 0.0 ? (result95 / result96).ToString() : "Invalid Input";
    }
    else if (regex16.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      string[] strArray = fraction.Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result97;
      double result98;
      if (double.TryParse(strArray[0], out result97) && double.TryParse(strArray[1], out result98))
        str1 = result98 != 0.0 ? (result97 / result98 / 12.0).ToString() : "Invalid Input";
    }
    else if (regex17.IsMatch(fraction))
    {
      string[] strArray27 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray28 = strArray27[0].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray29 = strArray27[1].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray28.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      if (strArray29.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result99;
      double result100;
      double result101;
      double result102;
      if (double.TryParse(strArray28[0], out result99) && double.TryParse(strArray28[1], out result100) && double.TryParse(strArray29[0], out result101) && double.TryParse(strArray29[1], out result102))
        str1 = result100 == 0.0 || result102 == 0.0 ? "Invalid Input" : (result99 / result100 + result101 / result102 / 12.0).ToString();
    }
    else if (regex18.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray30 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray31 = strArray30[0].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray32 = strArray30[1].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray31.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      if (strArray32.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result103;
      double result104;
      double result105;
      double result106;
      if (double.TryParse(strArray31[0], out result103) && double.TryParse(strArray31[1], out result104) && double.TryParse(strArray32[0], out result105) && double.TryParse(strArray32[1], out result106))
        str1 = result104 == 0.0 || result106 == 0.0 ? "Invalid Input" : (result103 / result104 + result105 / result106 / 12.0).ToString();
    }
    else if (regex19.IsMatch(fraction))
    {
      string[] strArray33 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray34 = strArray33[1].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray35 = strArray33[3].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      double result107;
      double.TryParse(strArray33[0], out result107);
      double result108;
      double.TryParse(strArray33[2], out result108);
      if (strArray34.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      if (strArray35.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result109;
      double result110;
      double result111;
      double result112;
      if (double.TryParse(strArray34[0], out result109) && double.TryParse(strArray34[1], out result110) && double.TryParse(strArray35[0], out result111) && double.TryParse(strArray35[1], out result112))
      {
        if (result110 == 0.0 || result112 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num1 = result109 / result110;
          double num2 = result111 / result112;
          str1 = (result107 + num1 + (result108 + num2) / 12.0).ToString();
        }
      }
    }
    else if (regex20.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray36 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray37 = strArray36[1].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray38 = strArray36[3].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      double result113;
      double.TryParse(strArray36[0], out result113);
      double result114;
      double.TryParse(strArray36[2], out result114);
      if (strArray37.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      if (strArray38.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result115;
      double result116;
      double result117;
      double result118;
      if (double.TryParse(strArray37[0], out result115) && double.TryParse(strArray37[1], out result116) && double.TryParse(strArray38[0], out result117) && double.TryParse(strArray38[1], out result118))
      {
        if (result116 == 0.0 || result118 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num3 = result115 / result116;
          double num4 = result117 / result118;
          str1 = (result113 + num3 + (result114 + num4) / 12.0).ToString();
        }
      }
    }
    else if (regex21.IsMatch(fraction))
    {
      string[] strArray39 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray40 = strArray39[1].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray41 = strArray39[2].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      double result119;
      double.TryParse(strArray39[0], out result119);
      if (strArray40.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      if (strArray41.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result120;
      double result121;
      double result122;
      double result123;
      if (double.TryParse(strArray40[0], out result120) && double.TryParse(strArray40[1], out result121) && double.TryParse(strArray41[0], out result122) && double.TryParse(strArray41[1], out result123))
      {
        if (result121 == 0.0 || result123 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num5 = result120 / result121;
          double num6 = result122 / result123;
          str1 = (result119 + num5 + num6 / 12.0).ToString();
        }
      }
    }
    else if (regex22.IsMatch(fraction))
    {
      fraction = fraction.Replace("\"", "");
      fraction = fraction.Replace("'", "");
      string[] strArray42 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray43 = strArray42[1].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      string[] strArray44 = strArray42[2].Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      double result124;
      double.TryParse(strArray42[0], out result124);
      if (strArray43.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      if (strArray44.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result125;
      double result126;
      double result127;
      double result128;
      if (double.TryParse(strArray43[0], out result125) && double.TryParse(strArray43[1], out result126) && double.TryParse(strArray44[0], out result127) && double.TryParse(strArray44[1], out result128))
      {
        if (result126 == 0.0 || result128 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num7 = result125 / result126;
          double num8 = result127 / result128;
          str1 = (result124 + num7 + num8 / 12.0).ToString();
        }
      }
    }
    else if (regex13.IsMatch(fraction))
    {
      string[] strArray45 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray45[0];
      string str15 = strArray45[1];
      double result129;
      double.TryParse(s, out result129);
      string[] strArray46 = str15.Split(new string[1]{ "/" }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray46.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result130;
      double result131;
      if (double.TryParse(strArray46[0], out result130) && double.TryParse(strArray46[1], out result131))
      {
        if (result131 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result130 / result131 / 12.0;
          str1 = (result129 + num).ToString();
        }
      }
    }
    else if (regex23.IsMatch(fraction))
      str1 = fraction;
    else if (regex14.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result132;
      double result133;
      if (double.TryParse(strArray[0], out result132) && double.TryParse(strArray[1], out result133))
        str1 = result133 != 0.0 ? (result132 / result133).ToString() : "Invalid Input";
    }
    else if (regex31.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s57 = strArray[0];
      string s58 = strArray[1];
      double result134;
      double.TryParse(s57, out result134);
      double result135;
      double.TryParse(s58, out result135);
      double num = result135 / 12.0;
      str1 = (result134 + num).ToString();
    }
    else
      str1 = regex30.IsMatch(fraction) || fraction == "" ? fraction : "Invalid Input";
    return str1;
  }

  private string ConvertInputStringMetric(string fraction)
  {
    fraction = fraction.Trim();
    string str1 = (string) null;
    Regex regex1 = new Regex("^-?\\d+\\s+-?\\d+/\\d+$");
    Regex regex2 = new Regex("^-?\\d+/\\d+$");
    Regex regex3 = new Regex("^\\d+[-]\\d+/\\d+$");
    Regex regex4 = new Regex("^[-]\\d+[-]\\d+/\\d+$");
    Regex regex5 = new Regex("^-?\\d+$");
    Regex regex6 = new Regex("^-?\\d+[.]\\d+$");
    Regex regex7 = new Regex("^-?[.]\\d+$");
    if (new Regex("^[-]\\d+\\s+-?\\d+/\\d+$").IsMatch(fraction))
    {
      fraction = fraction.Replace("-", "");
      string[] strArray1 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray1[0];
      string str2 = strArray1[1];
      double result1;
      double.TryParse(s, out result1);
      string[] separator = new string[1]{ "/" };
      string[] strArray2 = str2.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray2.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result2;
      double result3;
      if (double.TryParse(strArray2[0], out result2) && double.TryParse(strArray2[1], out result3))
        str1 = result3 != 0.0 ? ((result2 / result3 + result1) * -1.0).ToString() : "Invalid Input";
    }
    else if (regex1.IsMatch(fraction))
    {
      string[] strArray3 = fraction.Split(new string[1]
      {
        " "
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray3[0];
      string str3 = strArray3[1];
      double result4;
      double.TryParse(s, out result4);
      string[] separator = new string[1]{ "/" };
      string[] strArray4 = str3.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray4.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result5;
      double result6;
      if (double.TryParse(strArray4[0], out result5) && double.TryParse(strArray4[1], out result6))
      {
        if (result6 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result5 / result6;
          str1 = (result4 + num).ToString();
        }
      }
    }
    else if (regex2.IsMatch(fraction))
    {
      string[] strArray = fraction.Split(new string[1]
      {
        "/"
      }, StringSplitOptions.RemoveEmptyEntries);
      if (strArray.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result7;
      double result8;
      if (double.TryParse(strArray[0], out result7) && double.TryParse(strArray[1], out result8))
        str1 = result8 != 0.0 ? (result7 / result8).ToString() : "Invalid Input";
    }
    else if (regex3.IsMatch(fraction))
    {
      string[] strArray5 = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray5[0];
      string str4 = strArray5[1];
      double result9;
      double.TryParse(s, out result9);
      string[] separator = new string[1]{ "/" };
      string[] strArray6 = str4.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray6.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result10;
      double result11;
      if (double.TryParse(strArray6[0], out result10) && double.TryParse(strArray6[1], out result11))
      {
        if (result11 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result10 / result11;
          str1 = (result9 + num).ToString();
        }
      }
    }
    else if (regex4.IsMatch(fraction))
    {
      string[] strArray7 = fraction.Split(new string[1]
      {
        "-"
      }, StringSplitOptions.RemoveEmptyEntries);
      string s = strArray7[0];
      string str5 = strArray7[1];
      double result12;
      double.TryParse(s, out result12);
      string[] separator = new string[1]{ "/" };
      string[] strArray8 = str5.Split(separator, StringSplitOptions.RemoveEmptyEntries);
      if (strArray8.Length != 2)
      {
        int num = (int) MessageBox.Show("Invalid fraction entered.");
        str1 = "0";
      }
      double result13;
      double result14;
      if (double.TryParse(strArray8[0], out result13) && double.TryParse(strArray8[1], out result14))
      {
        if (result14 == 0.0)
        {
          str1 = "Invalid Input";
        }
        else
        {
          double num = result13 / result14;
          str1 = ((result12 + num) * -1.0).ToString();
        }
      }
    }
    else
      str1 = regex5.IsMatch(fraction) || fraction == "" ? fraction : (!regex6.IsMatch(fraction) ? (!regex7.IsMatch(fraction) ? "Invalid Input" : fraction) : fraction);
    return str1;
  }

  private double StringToDouble(string feet)
  {
    double num = !(feet == "") ? Convert.ToDouble(feet) : 0.0;
    if (!this.useImperialUnits)
      num = UnitUtils.Convert(num, UnitTypeId.Millimeters, UnitTypeId.Feet);
    return num;
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.xTextBox = new TextBox();
    this.label1 = new Label();
    this.label2 = new Label();
    this.yTextBox = new TextBox();
    this.label3 = new Label();
    this.zTextBox = new TextBox();
    this.refUpdate = new Button();
    this.label4 = new Label();
    this.label5 = new Label();
    this.button1 = new Button();
    this.label6 = new Label();
    this.label7 = new Label();
    this.label8 = new Label();
    this.label9 = new Label();
    this.SuspendLayout();
    this.xTextBox.Location = new System.Drawing.Point(15, 101);
    this.xTextBox.Name = "xTextBox";
    this.xTextBox.Size = new Size(187, 22);
    this.xTextBox.TabIndex = 0;
    this.xTextBox.Text = "0";
    this.xTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.enter_KeyPress);
    this.label1.AutoSize = true;
    this.label1.Location = new System.Drawing.Point(12, 81);
    this.label1.Name = "label1";
    this.label1.Size = new Size(90, 16 /*0x10*/);
    this.label1.TabIndex = 16 /*0x10*/;
    this.label1.Text = "Offset X Value";
    this.label2.AutoSize = true;
    this.label2.Location = new System.Drawing.Point(12, 126);
    this.label2.Name = "label2";
    this.label2.Size = new Size(91, 16 /*0x10*/);
    this.label2.TabIndex = 15;
    this.label2.Text = "Offset Y Value";
    this.yTextBox.Location = new System.Drawing.Point(15, 146);
    this.yTextBox.Name = "yTextBox";
    this.yTextBox.Size = new Size(187, 22);
    this.yTextBox.TabIndex = 2;
    this.yTextBox.Text = "0";
    this.yTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.enter_KeyPress);
    this.label3.AutoSize = true;
    this.label3.Location = new System.Drawing.Point(12, 171);
    this.label3.Name = "label3";
    this.label3.Size = new Size(90, 16 /*0x10*/);
    this.label3.TabIndex = 14;
    this.label3.Text = "Offset Z Value";
    this.zTextBox.Location = new System.Drawing.Point(15, 191);
    this.zTextBox.Name = "zTextBox";
    this.zTextBox.Size = new Size(187, 22);
    this.zTextBox.TabIndex = 4;
    this.zTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.enter_KeyPress);
    this.refUpdate.Location = new System.Drawing.Point(258, 138);
    this.refUpdate.Name = "refUpdate";
    this.refUpdate.Size = new Size(75, 38);
    this.refUpdate.TabIndex = 6;
    this.refUpdate.Text = "Update";
    this.refUpdate.UseVisualStyleBackColor = true;
    this.refUpdate.Click += new EventHandler(this.refUpdate_Click_1);
    this.label4.AutoSize = true;
    this.label4.Location = new System.Drawing.Point(12, 22);
    this.label4.Name = "label4";
    this.label4.Size = new Size(0, 16 /*0x10*/);
    this.label4.TabIndex = 13;
    this.label5.AutoSize = true;
    this.label5.Location = new System.Drawing.Point(12, 22);
    this.label5.Name = "label5";
    this.label5.Size = new Size(329, 32 /*0x20*/);
    this.label5.TabIndex = 12;
    this.label5.Text = "Enter offset XYZ values in feet and inches. \r\nAny axes left blank will not be moved on upon that axis.";
    this.button1.DialogResult = DialogResult.Cancel;
    this.button1.Location = new System.Drawing.Point(263, 142);
    this.button1.Name = "button1";
    this.button1.Size = new Size(75, 38);
    this.button1.TabIndex = 17;
    this.button1.Text = "Cancel";
    this.button1.UseVisualStyleBackColor = true;
    this.button1.Click += new EventHandler(this.NewRefPointForm_Cancel);
    this.label6.AutoSize = true;
    this.label6.ForeColor = System.Drawing.Color.Red;
    this.label6.Location = new System.Drawing.Point(117, 81);
    this.label6.Name = "label6";
    this.label6.Size = new Size(85, 16 /*0x10*/);
    this.label6.TabIndex = 17;
    this.label6.Text = "* Invalid Input";
    this.label6.Visible = false;
    this.label7.AutoSize = true;
    this.label7.ForeColor = System.Drawing.Color.Red;
    this.label7.Location = new System.Drawing.Point(117, 126);
    this.label7.Name = "label7";
    this.label7.Size = new Size(85, 16 /*0x10*/);
    this.label7.TabIndex = 18;
    this.label7.Text = "* Invalid Input";
    this.label7.Visible = false;
    this.label8.AutoSize = true;
    this.label8.ForeColor = System.Drawing.Color.Red;
    this.label8.Location = new System.Drawing.Point(117, 171);
    this.label8.Name = "label8";
    this.label8.Size = new Size(85, 16 /*0x10*/);
    this.label8.TabIndex = 19;
    this.label8.Text = "* Invalid Input";
    this.label8.Visible = false;
    this.label9.AutoSize = true;
    this.label9.Location = new System.Drawing.Point(12, 56);
    this.label9.Name = "label9";
    this.label9.Size = new Size(128 /*0x80*/, 16 /*0x10*/);
    this.label9.TabIndex = 20;
    this.label9.Text = "Current Work Plane: ";
    this.AutoScaleDimensions = new SizeF(8f, 16f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(368, 245);
    this.Controls.Add((System.Windows.Forms.Control) this.label9);
    this.Controls.Add((System.Windows.Forms.Control) this.label8);
    this.Controls.Add((System.Windows.Forms.Control) this.label7);
    this.Controls.Add((System.Windows.Forms.Control) this.label6);
    this.Controls.Add((System.Windows.Forms.Control) this.label5);
    this.Controls.Add((System.Windows.Forms.Control) this.label4);
    this.Controls.Add((System.Windows.Forms.Control) this.refUpdate);
    this.Controls.Add((System.Windows.Forms.Control) this.zTextBox);
    this.Controls.Add((System.Windows.Forms.Control) this.label3);
    this.Controls.Add((System.Windows.Forms.Control) this.yTextBox);
    this.Controls.Add((System.Windows.Forms.Control) this.label2);
    this.Controls.Add((System.Windows.Forms.Control) this.label1);
    this.Controls.Add((System.Windows.Forms.Control) this.xTextBox);
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.Name = nameof (NewRefPointForm);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Text = "Enter Offset Values";
    this.Load += new EventHandler(this.NewRefPointForm_Load);
    this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.enter_KeyPress);
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
