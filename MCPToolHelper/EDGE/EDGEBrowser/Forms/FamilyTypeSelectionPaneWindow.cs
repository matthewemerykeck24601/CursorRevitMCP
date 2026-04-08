// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.Forms.FamilyTypeSelectionPaneWindow
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
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace EDGE.EDGEBrowser.Forms;

public class FamilyTypeSelectionPaneWindow : Window, IComponentConnector
{
  private List<string> videoQualityPaths = new List<string>();
  private double wid = 800.0;
  private double heigh = 800.0;
  public bool closed;
  private string fullPath = "";
  private Document revitDoc;
  private List<string> dataTypes = new List<string>();
  internal TextBlock txtLabel;
  internal DataGrid FamilyTypeSelectionDataGrid;
  internal Button btnSelectAll;
  internal Button btnDeselectAll;
  internal Button btnConfirm;
  internal Button btnCancel;
  private bool _contentLoaded;

  public FamilyTypeSelectionPaneWindow(
    string[] sourceText,
    string famFileName,
    string fullFilePath,
    Document document)
  {
    this.revitDoc = document;
    this.fullPath = fullFilePath;
    this.InitializeComponent();
    this.ParseTextIntoList(sourceText);
    this.txtLabel.Text = $"Please select the type of {famFileName.Split('.')[0]} you wish to import:";
    this.Closing += new CancelEventHandler(this.OnWindowClosing);
  }

  public void OnWindowClosing(object sender, CancelEventArgs e) => this.closed = true;

  private void ParseTextIntoList(string[] sourceText)
  {
    DataTable dataTable = new DataTable();
    dataTable.Columns.Add(new DataColumn()
    {
      ColumnName = "Type"
    });
    string[] strArray1 = sourceText[0].Split(',');
    this.dataTypes.Add("");
    for (int index = 1; index < strArray1.Length; ++index)
    {
      string str1 = strArray1[index].Replace("##", "#");
      string[] strArray2 = str1.Split('#');
      string str2 = str1.Split('#')[0];
      string str3;
      if (strArray2.Length == 3)
      {
        str3 = str1.Split('#')[2].Replace("other", "").Replace("length", "").Replace("feet", "Feet").Replace("inches", "Inches");
        this.dataTypes.Add(str3);
      }
      else
      {
        str3 = str1.Split('#')[1].Replace("other", "").Replace("length", "").Replace("feet", "Feet").Replace("inches", "Inches");
        this.dataTypes.Add(str3);
      }
      if (!str3.Trim().Equals(""))
        str2 = $"{str2}\n ({str3})";
      if (str2 != null && !str2.Equals(""))
        dataTable.Columns.Add(new DataColumn()
        {
          ColumnName = str2.Replace("_", "__")
        });
    }
    for (int index = 1; index < sourceText.Length; ++index)
    {
      DataRow row = dataTable.NewRow();
      object[] outText = this.ParseOutText(sourceText[index].Split(','));
      row.ItemArray = outText;
      dataTable.Rows.Add(row);
    }
    this.FamilyTypeSelectionDataGrid.ItemsSource = (IEnumerable) dataTable.DefaultView;
  }

  private object[] ParseOutText(string[] textRow)
  {
    string[] outText = new string[textRow.Length];
    for (int index1 = 0; index1 < textRow.Length; ++index1)
    {
      char[] charArray = textRow[index1].ToCharArray();
      StringBuilder stringBuilder = new StringBuilder();
      for (int index2 = 0; index2 < charArray.Length; ++index2)
      {
        if ((index2 != 0 || !charArray[index2].Equals('"')) && (index2 != charArray.Length - 1 || !charArray[index2].Equals('"')))
          stringBuilder.Append(charArray[index2]);
      }
      string str = stringBuilder.ToString().Replace("\"\"", "\"");
      outText[index1] = str;
    }
    return (object[]) outText;
  }

  private void btnConfirm_Click(object sender, RoutedEventArgs e)
  {
    int num1 = 0;
    bool flag = false;
    if (this.FamilyTypeSelectionDataGrid.SelectedItems.Count == 0)
    {
      flag = true;
    }
    else
    {
      try
      {
        List<string> stringList = new List<string>();
        string str1 = "";
        foreach (DataRowView selectedItem in (IEnumerable) this.FamilyTypeSelectionDataGrid.SelectedItems)
        {
          object obj = selectedItem[0];
          if (!obj.ToString().Equals(""))
          {
            IFamilyLoadOptions familyLoadOptions = UIDocument.GetRevitUIFamilyLoadOptions();
            FamilySymbol symbol = (FamilySymbol) null;
            this.revitDoc.LoadFamilySymbol(this.fullPath, obj.ToString(), familyLoadOptions, out symbol);
            if (symbol == null)
            {
              this.closed = true;
              int num2 = (int) MessageBox.Show("Unable to Load Family. Please ensure that the family you are trying to load is valid for the currently active file.", "Unable to Load Family");
              this.Close();
              break;
            }
            this.closed = false;
            ++num1;
            str1 = symbol.FamilyName;
            string str2 = obj.ToString();
            stringList.Add(str2);
          }
        }
        string str3 = string.Join(",\n", stringList.ToArray());
        if (!this.closed && !str1.Equals(""))
        {
          int num3 = (int) MessageBox.Show($"{str1} family has been successfully added to the project for the following types: \n{str3}", "Family Loaded");
        }
        this.Close();
        if (!this.closed)
        {
          int num4 = (int) MessageBox.Show($"{str1} family has been successfully added to the project for the following types: \n{str3}", "Family Loaded");
        }
      }
      catch (Exception ex)
      {
        this.closed = true;
        int num5 = (int) MessageBox.Show("Unable to Load Family. Please ensure that the family you are trying to load is still available to be loaded and valid for the currently active file.", "Unable to Load Family");
        this.Close();
      }
    }
    if (!(num1 == 0 & flag))
      return;
    int num6 = (int) MessageBox.Show("Please select a family type to continue.", "No Family Types Selected");
  }

  private void btnCancel_Click(object sender, RoutedEventArgs e)
  {
    this.closed = true;
    this.Close();
  }

  private void btnSelectAll_Click(object sender, RoutedEventArgs e)
  {
    this.FamilyTypeSelectionDataGrid.SelectAll();
  }

  private void btnDeselectAll_Click(object sender, RoutedEventArgs e)
  {
    this.FamilyTypeSelectionDataGrid.UnselectAll();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/edgebrowser/forms/familytypeselectionpanewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.txtLabel = (TextBlock) target;
        break;
      case 2:
        this.FamilyTypeSelectionDataGrid = (DataGrid) target;
        break;
      case 3:
        this.btnSelectAll = (Button) target;
        this.btnSelectAll.Click += new RoutedEventHandler(this.btnSelectAll_Click);
        break;
      case 4:
        this.btnDeselectAll = (Button) target;
        this.btnDeselectAll.Click += new RoutedEventHandler(this.btnDeselectAll_Click);
        break;
      case 5:
        this.btnConfirm = (Button) target;
        this.btnConfirm.Click += new RoutedEventHandler(this.btnConfirm_Click);
        break;
      case 6:
        this.btnCancel = (Button) target;
        this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
