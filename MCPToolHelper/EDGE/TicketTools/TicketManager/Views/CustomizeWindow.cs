// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.Views.CustomizeWindow
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.TicketManager.Views;

public class CustomizeWindow : Window, IComponentConnector
{
  public List<Tuple<string, string>> CustomParams = new List<Tuple<string, string>>();
  private Document revitDoc;
  private string ticketManagerCustomizationPath = "";
  private string manufacturer = "";
  public bool closed;
  internal Label DataGridTitle;
  internal Button confirmButton;
  internal Label label;
  internal TextBox parameterTextBox1;
  internal TextBox parameterTextBox2;
  internal TextBox parameterTextBox3;
  internal TextBox parameterTextBox4;
  internal ComboBox parameterComboBox1;
  internal ComboBox parameterComboBox2;
  internal ComboBox parameterComboBox3;
  internal ComboBox parameterComboBox4;
  internal Label label1;
  internal Label label2;
  internal Label label2_Copy;
  internal Label label2_Copy1;
  internal Label label2_Copy2;
  internal Label label2_Copy3;
  internal Label label2_Copy4;
  internal Label label2_Copy5;
  internal Button masterResetButton;
  internal Button resetButton4;
  internal Button resetButton3;
  internal Button resetButton2;
  internal Button resetButton1;
  internal Button resetFileButton;
  private bool _contentLoaded;

  public CustomizeWindow(UIApplication app)
  {
    this.ticketManagerCustomizationPath = App.TMCFolderPath;
    if (this.ticketManagerCustomizationPath.Equals(""))
      this.ticketManagerCustomizationPath = "C:\\EDGEforRevit";
    this.InitializeComponent();
    this.revitDoc = app.ActiveUIDocument.Document;
    this.parameterComboBox1.ItemsSource = (IEnumerable) this.GetParameters();
    this.parameterComboBox2.ItemsSource = (IEnumerable) this.GetParameters();
    this.parameterComboBox3.ItemsSource = (IEnumerable) this.GetParameters();
    this.parameterComboBox4.ItemsSource = (IEnumerable) this.GetParameters();
    this.Title = "Ticket Manager Customization Settings";
    this.ResizeMode = ResizeMode.NoResize;
    Parameter parameter1 = this.revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter1 != null && !string.IsNullOrEmpty(parameter1.AsString()))
      this.manufacturer = parameter1.AsString();
    bool flag = false;
    string str = "";
    try
    {
      if (!this.manufacturer.Equals("") || this.manufacturer != null)
      {
        if (File.Exists($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt"))
        {
          if (new FileInfo($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt").IsReadOnly)
            flag = true;
          str = $"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt";
        }
        else if (File.Exists(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt"))
        {
          if (new FileInfo(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt").IsReadOnly)
            flag = true;
          str = this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt";
        }
      }
      else if (File.Exists(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt"))
      {
        if (new FileInfo(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt").IsReadOnly)
          flag = true;
        str = this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt";
      }
    }
    catch (Exception ex)
    {
      new TaskDialog("Ticket Manager Settings File Error")
      {
        MainInstruction = "Ticket Manager Settings File is unavailable.",
        MainContent = $"Unable to read in the Ticket Manager Settings File available at {this.manufacturer}_TicketManagerCustomizationSettings.txt. Please ensure the file is available to be read and not in use by another application and try again."
      }.Show();
      this.closed = true;
      this.Close();
      return;
    }
    if (flag)
    {
      new TaskDialog("File is Read Only")
      {
        MainInstruction = "Ticket Manager Customization Settings File is Read Only",
        MainContent = $"Unable to modify the Ticket Manager Customization Settings File in the specified location {str} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
      }.Show();
      this.parameterTextBox1.IsEnabled = false;
      this.parameterTextBox2.IsEnabled = false;
      this.parameterTextBox3.IsEnabled = false;
      this.parameterTextBox4.IsEnabled = false;
      this.parameterComboBox1.IsEnabled = false;
      this.parameterComboBox2.IsEnabled = false;
      this.parameterComboBox3.IsEnabled = false;
      this.parameterComboBox4.IsEnabled = false;
      this.confirmButton.IsEnabled = false;
      this.resetButton1.IsEnabled = false;
      this.resetButton2.IsEnabled = false;
      this.resetButton3.IsEnabled = false;
      this.resetButton4.IsEnabled = false;
      this.resetFileButton.IsEnabled = false;
      this.masterResetButton.IsEnabled = false;
    }
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    try
    {
      if (!this.manufacturer.Equals("") || this.manufacturer != null)
      {
        if (File.Exists($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt"))
        {
          foreach (string readAllLine in File.ReadAllLines($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt"))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray)[1].Split('|');
            stringList1.Add(strArray[0].Trim());
            stringList2.Add(strArray[1].Trim());
          }
        }
        else if (File.Exists(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt"))
        {
          foreach (string readAllLine in File.ReadAllLines(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt"))
          {
            char[] chArray = new char[1]{ ':' };
            string[] strArray = readAllLine.Split(chArray)[1].Split('|');
            stringList1.Add(strArray[0].Trim());
            stringList2.Add(strArray[1].Trim());
          }
        }
      }
      else if (File.Exists(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt"))
      {
        foreach (string readAllLine in File.ReadAllLines(this.ticketManagerCustomizationPath + "\\TicketManagerCustomizationSettings.txt"))
        {
          char[] chArray = new char[1]{ ':' };
          string[] strArray = readAllLine.Split(chArray)[1].Split('|');
          stringList1.Add(strArray[0].Trim());
          stringList2.Add(strArray[1].Trim());
        }
      }
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
      {
        new TaskDialog("Ticket Manager Settings File Error")
        {
          MainInstruction = "Ticket Manager Settings File is open.",
          MainContent = $"Check Ticket Manager Settings File at {this.manufacturer}_TicketManagerCustomizationSettings.txt. Please ensure the file is not in use by another application and try again."
        }.Show();
        this.closed = true;
        this.Close();
        return;
      }
      new TaskDialog("Ticket Manager Settings File Error")
      {
        MainInstruction = "Ticket Manager Settings File is unavailable.",
        MainContent = $"Unable to read in the Ticket Manager Settings File available at {str}. Please ensure the file is available to be read and not in use by another application and try again."
      }.Show();
      this.closed = true;
      this.Close();
      return;
    }
    if (stringList1.Count > 0)
    {
      this.parameterTextBox1.Text = stringList1[0];
      foreach (string parameter2 in this.GetParameters())
      {
        if (parameter2.Equals(stringList2[0]))
          this.parameterComboBox1.SelectedIndex = this.GetParameters().IndexOf((object) parameter2);
      }
      if (this.parameterComboBox1.SelectedIndex == -1)
        this.parameterTextBox1.Text = "";
    }
    if (stringList1.Count > 1)
    {
      this.parameterTextBox2.Text = stringList1[1];
      foreach (string parameter3 in this.GetParameters())
      {
        if (parameter3.Equals(stringList2[1]))
          this.parameterComboBox2.SelectedIndex = this.GetParameters().IndexOf((object) parameter3);
      }
      if (this.parameterComboBox2.SelectedIndex == -1)
        this.parameterTextBox2.Text = "";
    }
    if (stringList1.Count > 2)
    {
      this.parameterTextBox3.Text = stringList1[2];
      foreach (string parameter4 in this.GetParameters())
      {
        if (parameter4.Equals(stringList2[2]))
          this.parameterComboBox3.SelectedIndex = this.GetParameters().IndexOf((object) parameter4);
      }
      if (this.parameterComboBox3.SelectedIndex == -1)
        this.parameterTextBox3.Text = "";
    }
    if (stringList1.Count <= 3)
      return;
    this.parameterTextBox4.Text = stringList1[3];
    foreach (string parameter5 in this.GetParameters())
    {
      if (parameter5.Equals(stringList2[3]))
        this.parameterComboBox4.SelectedIndex = this.GetParameters().IndexOf((object) parameter5);
    }
    if (this.parameterComboBox4.SelectedIndex != -1)
      return;
    this.parameterTextBox4.Text = "";
  }

  public List<object> GetParameters()
  {
    List<object> parameters = new List<object>();
    List<string> stringList1 = new List<string>()
    {
      "IDENTITY_COMMENT",
      "ON_HOLD",
      "TICKET_REINFORCED_DATE_CURRENT",
      "TICKET_REINFORCED_USER_CURRENT",
      "TICKET_CREATED_DATE_INITIAL",
      "TICKET_CREATED_USER_INITIAL",
      "TICKET_DETAILED_DATE_CURRENT",
      "TICKET_DETAILED_USER_CURRENT",
      "TICKET_RELEASED_DATE_CURRENT",
      "TICKET_RELEASED_USER_CURRENT",
      "TICKET_REINFORCED_DATE_INITIAL",
      "TICKET_REINFORCED_USER_INITIAL",
      "TICKET_RELEASED_DATE_CURRENT",
      "TICKET_RELEASED_USER_CURRENT",
      "TICKET_CREATED_DATE_CURRENT",
      "TICKET_CREATED_USER_CURRENT",
      "TICKET_DETAILED_DATE_INITIAL",
      "TICKET_DETAILED_USER_INITIAL",
      "ASSEMBLY_MARK_NUMBER",
      "CONTROL_MARK",
      "TICKET_EDIT_COMMENT",
      "TICKET_DESCRIPTION",
      "TICKET_FLAGGED",
      "TICKET_RELEASED_DATE_INITIAL",
      "TICKET_RELEASED_USER_INITIAL",
      "TKT_TOTAL_RELEASED"
    };
    List<string> stringList2 = new List<string>();
    DefinitionBindingMapIterator bindingMapIterator = this.revitDoc.ParameterBindings.ForwardIterator();
    while (bindingMapIterator.MoveNext())
    {
      InstanceBinding current1 = bindingMapIterator.Current as InstanceBinding;
      TypeBinding current2 = bindingMapIterator.Current as TypeBinding;
      if (bindingMapIterator.Current != null)
      {
        CategorySet categories;
        if (bindingMapIterator.Current is InstanceBinding)
          categories = current1.Categories;
        else if (bindingMapIterator.Current is TypeBinding)
          categories = current2.Categories;
        else
          continue;
        foreach (Category category in categories)
        {
          if ((category.Name.Equals("Assemblies") || category.Name.Equals("Structural Framing")) && !stringList2.Contains(bindingMapIterator.Key.Name) && !stringList1.Contains(bindingMapIterator.Key.Name.ToUpper()) && (bindingMapIterator.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
            stringList2.Add(bindingMapIterator.Key.Name);
        }
      }
    }
    stringList2.Sort();
    foreach (string str in stringList2)
    {
      if (!parameters.Contains((object) str))
        parameters.Add((object) str);
    }
    return parameters;
  }

  private void ConfirmButton_Click(object sender, RoutedEventArgs e)
  {
    MainWindow.needToRefreshSettingFile = true;
    List<string> stringList = new List<string>();
    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    if (!this.parameterTextBox1.Text.Equals(""))
    {
      if ((!this.parameterTextBox1.Text.Equals("") || !this.parameterTextBox1.Text.Equals((string) null)) && this.parameterComboBox1.SelectedItem != null)
      {
        this.CustomParams.Add(Tuple.Create<string, string>(this.parameterTextBox1.Text, this.parameterComboBox1.SelectedItem.ToString()));
        if (!stringList.Contains(this.parameterComboBox1.SelectedItem.ToString()))
        {
          stringList.Add(this.parameterComboBox1.SelectedItem.ToString());
          if (this.parameterTextBox1.Text.Contains(";") || this.parameterTextBox1.Text.Contains("_") || this.parameterTextBox1.Text.Contains("|") || this.parameterTextBox1.Text.Contains("[") || this.parameterTextBox1.Text.Contains("]") || this.parameterTextBox1.Text.Contains("/") || this.parameterTextBox1.Text.Contains("\\") || this.parameterTextBox1.Text.Contains(":"))
            flag2 = true;
          if (this.parameterTextBox1.Text.Length > 25)
            flag3 = true;
        }
        else
          flag1 = true;
      }
    }
    else if (this.parameterComboBox1.SelectedItem != null)
    {
      flag4 = true;
      if (!stringList.Contains(this.parameterComboBox1.SelectedItem.ToString()))
        stringList.Add(this.parameterComboBox1.SelectedItem.ToString());
      else
        flag1 = true;
    }
    if (!this.parameterTextBox2.Text.Equals(""))
    {
      if ((!this.parameterTextBox2.Text.Equals("") || !this.parameterTextBox2.Text.Equals((string) null)) && this.parameterComboBox2.SelectedItem != null)
      {
        this.CustomParams.Add(Tuple.Create<string, string>(this.parameterTextBox2.Text, this.parameterComboBox2.SelectedItem.ToString()));
        if (!stringList.Contains(this.parameterComboBox2.SelectedItem.ToString()))
        {
          stringList.Add(this.parameterComboBox2.SelectedItem.ToString());
          if (this.parameterTextBox2.Text.Contains(";") || this.parameterTextBox2.Text.Contains("_") || this.parameterTextBox2.Text.Contains("|") || this.parameterTextBox2.Text.Contains("[") || this.parameterTextBox2.Text.Contains("]") || this.parameterTextBox2.Text.Contains("/") || this.parameterTextBox2.Text.Contains("\\") || this.parameterTextBox2.Text.Contains(":"))
            flag2 = true;
          if (this.parameterTextBox2.Text.Length > 25)
            flag3 = true;
        }
        else
          flag1 = true;
      }
    }
    else if (this.parameterComboBox2.SelectedItem != null)
    {
      flag4 = true;
      if (!stringList.Contains(this.parameterComboBox2.SelectedItem.ToString()))
        stringList.Add(this.parameterComboBox2.SelectedItem.ToString());
      else
        flag1 = true;
    }
    if (!this.parameterTextBox3.Text.Equals(""))
    {
      if ((!this.parameterTextBox3.Text.Equals("") || !this.parameterTextBox3.Text.Equals((string) null)) && this.parameterComboBox3.SelectedItem != null)
      {
        this.CustomParams.Add(Tuple.Create<string, string>(this.parameterTextBox3.Text, this.parameterComboBox3.SelectedItem.ToString()));
        if (!stringList.Contains(this.parameterComboBox3.SelectedItem.ToString()))
        {
          stringList.Add(this.parameterComboBox3.SelectedItem.ToString());
          if (this.parameterTextBox3.Text.Contains(";") || this.parameterTextBox3.Text.Contains("_") || this.parameterTextBox3.Text.Contains("|") || this.parameterTextBox3.Text.Contains("[") || this.parameterTextBox3.Text.Contains("]") || this.parameterTextBox3.Text.Contains("/") || this.parameterTextBox3.Text.Contains("\\") || this.parameterTextBox3.Text.Contains(":"))
            flag2 = true;
          if (this.parameterTextBox3.Text.ToString().Length > 25)
            flag3 = true;
        }
        else
          flag1 = true;
      }
    }
    else if (this.parameterComboBox3.SelectedItem != null)
    {
      flag4 = true;
      if (!stringList.Contains(this.parameterComboBox3.SelectedItem.ToString()))
        stringList.Add(this.parameterComboBox3.SelectedItem.ToString());
      else
        flag1 = true;
    }
    if (!this.parameterTextBox4.Text.Equals(""))
    {
      if ((!this.parameterTextBox4.Text.Equals("") || !this.parameterTextBox4.Text.Equals((string) null)) && this.parameterComboBox4.SelectedItem != null)
      {
        this.CustomParams.Add(Tuple.Create<string, string>(this.parameterTextBox4.Text, this.parameterComboBox4.SelectedItem.ToString()));
        if (!stringList.Contains(this.parameterComboBox4.SelectedItem.ToString()))
        {
          stringList.Add(this.parameterTextBox4.Text.ToString());
          if (this.parameterTextBox4.Text.Contains(";") || this.parameterTextBox4.Text.Contains("_") || this.parameterTextBox4.Text.Contains("|") || this.parameterTextBox4.Text.Contains("[") || this.parameterTextBox4.Text.Contains("]") || this.parameterTextBox4.Text.Contains("/") || this.parameterTextBox4.Text.Contains("\\") || this.parameterTextBox4.Text.Contains(":"))
            flag2 = true;
          if (this.parameterTextBox4.Text.Length > 25)
            flag3 = true;
        }
        else
          flag1 = true;
      }
    }
    else if (this.parameterComboBox4.SelectedItem != null)
    {
      flag4 = true;
      if (!stringList.Contains(this.parameterComboBox4.SelectedItem.ToString()))
        stringList.Add(this.parameterComboBox4.SelectedItem.ToString());
      else
        flag1 = true;
    }
    bool flag5 = false;
    if (flag4)
    {
      this.label2_Copy5.Visibility = System.Windows.Visibility.Visible;
      this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
      flag1 = false;
      flag5 = true;
    }
    if (flag1)
    {
      this.label2_Copy4.Visibility = System.Windows.Visibility.Visible;
      this.label2_Copy5.Visibility = System.Windows.Visibility.Hidden;
      flag5 = true;
    }
    if (flag2 | flag3 && !flag5 && MessageBox.Show("Customer parameter names will be trimmed to 25 characters for display in the Ticket Manager and invalid characters will be removed ('_', '|', '[', ']', '/', '\\', ';', ':') Continue with Save?", "Save Custom Parameters", MessageBoxButton.YesNo) == MessageBoxResult.No)
      flag5 = true;
    if (!flag5)
    {
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < this.CustomParams.Count; ++index)
      {
        Tuple<string, string> customParam = this.CustomParams[index];
        string str1 = customParam.Item1.Replace(";", "").Replace("_", "").Replace("|", "").Replace("[", "").Replace("]", "").Replace("/", "").Replace("\\", "").Replace(":", "");
        string str2 = customParam.Item2;
        stringBuilder.AppendLine($"CustomParameter {index.ToString()} : {str1.Substring(0, Math.Min(str1.Length, 25))} | {str2}");
      }
      try
      {
        DirectoryInfo directoryInfo = new DirectoryInfo(this.ticketManagerCustomizationPath);
        if (!directoryInfo.Exists)
        {
          try
          {
            directoryInfo.Create();
          }
          catch (IOException ex)
          {
            new TaskDialog("UnableToSave")
            {
              MainInstruction = "Unable to save to location specified",
              MainContent = ("Unable to create a new directory in the specified location: " + this.ticketManagerCustomizationPath)
            }.Show();
            flag5 = true;
          }
        }
        if (!this.manufacturer.Equals(""))
          File.WriteAllText($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt", stringBuilder.ToString());
        else
          File.WriteAllText($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt", stringBuilder.ToString());
      }
      catch (IOException ex)
      {
        new TaskDialog("UnableToSave")
        {
          MainInstruction = "Unable to save to location specified",
          MainContent = ("Unable to create a new directory in the specified location: " + this.ticketManagerCustomizationPath)
        }.Show();
        flag5 = true;
      }
    }
    this.CustomParams.Clear();
    if (flag5)
      return;
    this.closed = true;
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e)
  {
    this.closed = true;
    this.Close();
  }

  private void ResetButton_Click(object sender, RoutedEventArgs e)
  {
    this.parameterTextBox1.Text = "";
    this.parameterTextBox2.Text = "";
    this.parameterTextBox3.Text = "";
    this.parameterTextBox4.Text = "";
    this.parameterComboBox1.SelectedIndex = -1;
    this.parameterComboBox2.SelectedIndex = -1;
    this.parameterComboBox3.SelectedIndex = -1;
    this.parameterComboBox4.SelectedIndex = -1;
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetFileButton_Click(object sender, RoutedEventArgs e)
  {
    if (MessageBox.Show("Customization file will be cleared of all current Custom Parameter information and the Ticket Manager Customization Settings Window will close. Continue with Reset?", "Reset Customization File", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
      return;
    bool flag = false;
    DirectoryInfo directoryInfo = new DirectoryInfo(this.ticketManagerCustomizationPath);
    if (!directoryInfo.Exists)
    {
      try
      {
        directoryInfo.Create();
      }
      catch (IOException ex)
      {
        new TaskDialog("UnableToSave")
        {
          MainInstruction = "Unable to save to location specified",
          MainContent = ("Unable to create a new directory in the specified location: " + this.ticketManagerCustomizationPath)
        }.Show();
        flag = true;
      }
    }
    if (!flag)
    {
      try
      {
        if (!this.manufacturer.Equals(""))
          File.WriteAllText($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt", "");
        else
          File.WriteAllText($"{this.ticketManagerCustomizationPath}\\{this.manufacturer}_TicketManagerCustomizationSettings.txt", "");
      }
      catch (Exception ex)
      {
        new TaskDialog("Ticket Manager Settings File Error")
        {
          MainInstruction = "Ticket Manager Settings File is open.",
          MainContent = "Check Ticket Manager Settings File. Please ensure the file is not in use by another application and try again."
        }.Show();
      }
    }
    this.closed = true;
    this.Close();
  }

  private void ResetButton1_Click(object sender, RoutedEventArgs e)
  {
    this.parameterTextBox1.Text = "";
    this.parameterComboBox1.SelectedIndex = -1;
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton2_Click(object sender, RoutedEventArgs e)
  {
    this.parameterTextBox2.Text = "";
    this.parameterComboBox2.SelectedIndex = -1;
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton3_Click(object sender, RoutedEventArgs e)
  {
    this.parameterTextBox3.Text = "";
    this.parameterComboBox3.SelectedIndex = -1;
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton4_Click(object sender, RoutedEventArgs e)
  {
    this.parameterTextBox4.Text = "";
    this.parameterComboBox4.SelectedIndex = -1;
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/ticketmanager/views/customizewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.DataGridTitle = (Label) target;
        break;
      case 2:
        this.confirmButton = (Button) target;
        this.confirmButton.Click += new RoutedEventHandler(this.ConfirmButton_Click);
        break;
      case 3:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      case 4:
        this.label = (Label) target;
        break;
      case 5:
        this.parameterTextBox1 = (TextBox) target;
        break;
      case 6:
        this.parameterTextBox2 = (TextBox) target;
        break;
      case 7:
        this.parameterTextBox3 = (TextBox) target;
        break;
      case 8:
        this.parameterTextBox4 = (TextBox) target;
        break;
      case 9:
        this.parameterComboBox1 = (ComboBox) target;
        break;
      case 10:
        this.parameterComboBox2 = (ComboBox) target;
        break;
      case 11:
        this.parameterComboBox3 = (ComboBox) target;
        break;
      case 12:
        this.parameterComboBox4 = (ComboBox) target;
        break;
      case 13:
        this.label1 = (Label) target;
        break;
      case 14:
        this.label2 = (Label) target;
        break;
      case 15:
        this.label2_Copy = (Label) target;
        break;
      case 16 /*0x10*/:
        this.label2_Copy1 = (Label) target;
        break;
      case 17:
        this.label2_Copy2 = (Label) target;
        break;
      case 18:
        this.label2_Copy3 = (Label) target;
        break;
      case 19:
        this.label2_Copy4 = (Label) target;
        break;
      case 20:
        this.label2_Copy5 = (Label) target;
        break;
      case 21:
        this.masterResetButton = (Button) target;
        this.masterResetButton.Click += new RoutedEventHandler(this.ResetButton_Click);
        break;
      case 22:
        this.resetButton4 = (Button) target;
        this.resetButton4.Click += new RoutedEventHandler(this.ResetButton4_Click);
        break;
      case 23:
        this.resetButton3 = (Button) target;
        this.resetButton3.Click += new RoutedEventHandler(this.ResetButton3_Click);
        break;
      case 24:
        this.resetButton2 = (Button) target;
        this.resetButton2.Click += new RoutedEventHandler(this.ResetButton2_Click);
        break;
      case 25:
        this.resetButton1 = (Button) target;
        this.resetButton1.Click += new RoutedEventHandler(this.ResetButton1_Click);
        break;
      case 26:
        this.resetFileButton = (Button) target;
        this.resetFileButton.Click += new RoutedEventHandler(this.ResetFileButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
