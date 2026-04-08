// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.Views.EditWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Utils.AssemblyUtils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.Views;

public class EditWindow : Window, IComponentConnector
{
  private Document revitDoc;
  private string ticketManagerCustomizationPath = "";
  private string manufacturer = "";
  public bool closed;
  public List<string> RowContents = new List<string>();
  public List<string> CustomHeads = new List<string>();
  public List<string> parameterNames = new List<string>();
  public AssemblyViewModel assemViewModel;
  internal Label DataGridTitle;
  internal Label label;
  internal TextBox custom1TextBox;
  internal TextBox custom2TextBox;
  internal Label label1;
  internal Label custom1Label;
  internal Label custom2Label;
  internal Label label2_Copy3;
  internal Label label2_Copy4;
  internal Button custom2ClearButton;
  internal Button custom1ClearButton;
  internal Label custom3Label;
  internal TextBox custom3TextBox;
  internal Button custom3ClearButton;
  internal Label custom4Label;
  internal TextBox custom4TextBox;
  internal Button custom4ClearButton;
  internal ComboBox custom1ComboBox;
  internal ComboBox custom2ComboBox;
  internal ComboBox custom3ComboBox;
  internal ComboBox custom4ComboBox;
  private bool _contentLoaded;

  private void PrimeTextFields(
    string name,
    TextBox textBox,
    ComboBox comboBox,
    Button clearButton,
    Label label)
  {
  }

  public EditWindow(
    UIApplication app,
    List<string> rowContents,
    AssemblyViewModel dataContext,
    List<string> customHeaders,
    List<string> paramNames)
  {
    this.RowContents = rowContents;
    this.CustomHeads = customHeaders;
    this.InitializeComponent();
    this.revitDoc = app.ActiveUIDocument.Document;
    this.parameterNames = paramNames;
    this.assemViewModel = dataContext;
    this.Title = "Edit Row - " + this.assemViewModel.MarkNumber.ToString();
    this.ResizeMode = ResizeMode.NoResize;
    this.label.Content = (object) $"Edit row contents for {this.assemViewModel.MarkNumber.ToString()} and click save to commit changes:";
    this.custom1Label.Visibility = System.Windows.Visibility.Collapsed;
    this.custom1TextBox.Visibility = System.Windows.Visibility.Collapsed;
    this.custom1ClearButton.Visibility = System.Windows.Visibility.Collapsed;
    this.custom2Label.Visibility = System.Windows.Visibility.Collapsed;
    this.custom2TextBox.Visibility = System.Windows.Visibility.Collapsed;
    this.custom2ClearButton.Visibility = System.Windows.Visibility.Collapsed;
    this.custom3Label.Visibility = System.Windows.Visibility.Collapsed;
    this.custom3TextBox.Visibility = System.Windows.Visibility.Collapsed;
    this.custom3ClearButton.Visibility = System.Windows.Visibility.Collapsed;
    this.custom4Label.Visibility = System.Windows.Visibility.Collapsed;
    this.custom4TextBox.Visibility = System.Windows.Visibility.Collapsed;
    this.custom4ClearButton.Visibility = System.Windows.Visibility.Collapsed;
    this.custom1ComboBox.ItemsSource = (IEnumerable) new List<object>()
    {
      (object) "Yes",
      (object) "No"
    };
    this.custom2ComboBox.ItemsSource = (IEnumerable) new List<object>()
    {
      (object) "Yes",
      (object) "No"
    };
    this.custom3ComboBox.ItemsSource = (IEnumerable) new List<object>()
    {
      (object) "Yes",
      (object) "No"
    };
    this.custom4ComboBox.ItemsSource = (IEnumerable) new List<object>()
    {
      (object) "Yes",
      (object) "No"
    };
    List<string> stringList1 = new List<string>();
    stringList1.AddRange((IEnumerable<string>) this.RowContents);
    int result = 0;
    int.TryParse(this.assemViewModel.ElementId, out result);
    Element element = this.revitDoc.GetElement(new ElementId(result));
    ElementId typeId = element.GetTypeId();
    if (customHeaders.Count > 0)
    {
      this.custom1Label.Visibility = System.Windows.Visibility.Visible;
      this.custom1TextBox.Visibility = System.Windows.Visibility.Visible;
      this.custom1ClearButton.Visibility = System.Windows.Visibility.Visible;
      this.custom1Label.Content = (object) customHeaders[0];
      if (this.assemViewModel.Assembly != null)
      {
        Parameter parameter1 = this.assemViewModel.Assembly.LookupParameter(this.parameterNames[0]) ?? this.assemViewModel.Assembly.GetStructuralFramingElement().GetTopLevelElement().LookupParameter(this.parameterNames[0]);
        if (parameter1 != null)
        {
          if (parameter1.IsReadOnly)
          {
            this.custom1TextBox.IsEnabled = false;
            this.custom1ComboBox.IsEnabled = false;
            this.custom1ClearButton.IsEnabled = false;
          }
          if (parameter1.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom1TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom1ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom1ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter1 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter2 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[0]);
          if (parameter2 != null)
          {
            if (parameter2.IsReadOnly)
            {
              this.custom1TextBox.IsEnabled = false;
              this.custom1ComboBox.IsEnabled = false;
              this.custom1ClearButton.IsEnabled = false;
            }
            if (parameter2.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom1TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom1ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom1ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      else if (element != null)
      {
        Parameter parameter3 = element.GetTopLevelElement().LookupParameter(this.parameterNames[0]);
        if (parameter3 != null)
        {
          if (parameter3.IsReadOnly)
          {
            this.custom1TextBox.IsEnabled = false;
            this.custom1ComboBox.IsEnabled = false;
            this.custom1ClearButton.IsEnabled = false;
          }
          else
          {
            foreach (Element elem in this.assemViewModel.SFElementsOfThisControlMark)
            {
              Parameter parameter4 = elem.GetTopLevelElement().LookupParameter(this.parameterNames[0]);
              if (parameter4 != null && parameter4.IsReadOnly)
              {
                this.custom1TextBox.IsEnabled = false;
                this.custom1ComboBox.IsEnabled = false;
                this.custom1ClearButton.IsEnabled = false;
                break;
              }
            }
          }
          if (parameter3.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom1TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom1ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom1ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter3 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter5 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[0]);
          if (parameter5 != null)
          {
            if (parameter5.IsReadOnly)
            {
              this.custom1TextBox.IsEnabled = false;
              this.custom1ComboBox.IsEnabled = false;
              this.custom1ClearButton.IsEnabled = false;
            }
            if (parameter5.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom1TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom1ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom1ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      this.custom1TextBox.Text = stringList1[2];
      if (stringList1[2].ToUpper().Equals("YES"))
        this.custom1ComboBox.SelectedIndex = 0;
      else if (stringList1[2].ToUpper().Equals("NO"))
        this.custom1ComboBox.SelectedIndex = 1;
      else
        this.custom1ComboBox.SelectedIndex = -1;
    }
    if (customHeaders.Count > 1)
    {
      this.custom2Label.Visibility = System.Windows.Visibility.Visible;
      this.custom2TextBox.Visibility = System.Windows.Visibility.Visible;
      this.custom2ClearButton.Visibility = System.Windows.Visibility.Visible;
      this.custom2Label.Content = (object) customHeaders[1];
      if (this.assemViewModel.Assembly != null)
      {
        Parameter parameter6 = this.assemViewModel.Assembly.LookupParameter(this.parameterNames[1]) ?? this.assemViewModel.Assembly.GetStructuralFramingElement().GetTopLevelElement().LookupParameter(this.parameterNames[1]);
        if (parameter6 != null)
        {
          if (parameter6.IsReadOnly)
          {
            this.custom2TextBox.IsEnabled = false;
            this.custom2ComboBox.IsEnabled = false;
            this.custom2ClearButton.IsEnabled = false;
          }
          if (parameter6.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom2TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom2ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom2ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter6 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter7 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[1]);
          if (parameter7 != null)
          {
            if (parameter7.IsReadOnly)
            {
              this.custom2TextBox.IsEnabled = false;
              this.custom2ComboBox.IsEnabled = false;
              this.custom2ClearButton.IsEnabled = false;
            }
            if (parameter7.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom2TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom2ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom2ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      else if (element != null)
      {
        Parameter parameter8 = element.GetTopLevelElement().LookupParameter(this.parameterNames[1]);
        if (parameter8 != null)
        {
          if (parameter8.IsReadOnly)
          {
            this.custom2TextBox.IsEnabled = false;
            this.custom2ComboBox.IsEnabled = false;
            this.custom2ClearButton.IsEnabled = false;
          }
          else
          {
            foreach (Element elem in this.assemViewModel.SFElementsOfThisControlMark)
            {
              Parameter parameter9 = elem.GetTopLevelElement().LookupParameter(this.parameterNames[1]);
              if (parameter9 != null && parameter9.IsReadOnly)
              {
                this.custom2TextBox.IsEnabled = false;
                this.custom2ComboBox.IsEnabled = false;
                this.custom2ClearButton.IsEnabled = false;
                break;
              }
            }
          }
          if (parameter8.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom2TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom2ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom2ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter8 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter10 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[1]);
          if (parameter10 != null)
          {
            if (parameter10.IsReadOnly)
            {
              this.custom2TextBox.IsEnabled = false;
              this.custom2ComboBox.IsEnabled = false;
              this.custom2ClearButton.IsEnabled = false;
            }
            if (parameter10.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom2TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom2ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom2ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      this.custom2TextBox.Text = stringList1[3];
      if (stringList1[3].ToUpper().Equals("YES"))
        this.custom2ComboBox.SelectedIndex = 0;
      else if (stringList1[3].ToUpper().Equals("NO"))
        this.custom2ComboBox.SelectedIndex = 1;
      else
        this.custom2ComboBox.SelectedIndex = -1;
    }
    if (customHeaders.Count > 2)
    {
      this.custom3Label.Visibility = System.Windows.Visibility.Visible;
      this.custom3TextBox.Visibility = System.Windows.Visibility.Visible;
      this.custom3ClearButton.Visibility = System.Windows.Visibility.Visible;
      this.custom3Label.Content = (object) customHeaders[2];
      if (this.assemViewModel.Assembly != null)
      {
        Parameter parameter11 = this.assemViewModel.Assembly.LookupParameter(this.parameterNames[2]) ?? this.assemViewModel.Assembly.GetStructuralFramingElement().GetTopLevelElement().LookupParameter(this.parameterNames[2]);
        if (parameter11 != null)
        {
          if (parameter11.IsReadOnly)
          {
            this.custom3TextBox.IsEnabled = false;
            this.custom3ComboBox.IsEnabled = false;
            this.custom3ClearButton.IsEnabled = false;
          }
          if (parameter11.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom3TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom3ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom3ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter11 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter12 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[2]);
          if (parameter12 != null)
          {
            if (parameter12.IsReadOnly)
            {
              this.custom3TextBox.IsEnabled = false;
              this.custom3ComboBox.IsEnabled = false;
              this.custom3ClearButton.IsEnabled = false;
            }
            else
            {
              foreach (Element elem in this.assemViewModel.SFElementsOfThisControlMark)
              {
                Parameter parameter13 = elem.GetTopLevelElement().LookupParameter(this.parameterNames[2]);
                if (parameter13 != null && parameter13.IsReadOnly)
                {
                  this.custom3TextBox.IsEnabled = false;
                  this.custom3ComboBox.IsEnabled = false;
                  this.custom3ClearButton.IsEnabled = false;
                  break;
                }
              }
            }
            if (parameter12.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom3TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom3ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom3ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      else if (element != null)
      {
        Parameter parameter14 = element.GetTopLevelElement().LookupParameter(this.parameterNames[2]);
        if (parameter14 != null)
        {
          if (parameter14.IsReadOnly)
          {
            this.custom3TextBox.IsEnabled = false;
            this.custom3ComboBox.IsEnabled = false;
            this.custom3ClearButton.IsEnabled = false;
          }
          else
          {
            foreach (Element elem in this.assemViewModel.SFElementsOfThisControlMark)
            {
              Parameter parameter15 = elem.GetTopLevelElement().LookupParameter(this.parameterNames[2]);
              if (parameter15 != null && parameter15.IsReadOnly)
              {
                this.custom3TextBox.IsEnabled = false;
                this.custom3ComboBox.IsEnabled = false;
                this.custom3ClearButton.IsEnabled = false;
                break;
              }
            }
          }
          if (parameter14.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom3TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom3ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom3ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter14 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter16 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[2]);
          if (parameter16 != null)
          {
            if (parameter16.IsReadOnly)
            {
              this.custom3TextBox.IsEnabled = false;
              this.custom3ComboBox.IsEnabled = false;
              this.custom3ClearButton.IsEnabled = false;
            }
            if (parameter16.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom3TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom3ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom3ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      this.custom3TextBox.Text = stringList1[4];
      if (stringList1[4].ToUpper().Equals("YES"))
        this.custom3ComboBox.SelectedIndex = 0;
      else if (stringList1[4].ToUpper().Equals("NO"))
        this.custom3ComboBox.SelectedIndex = 1;
      else
        this.custom3ComboBox.SelectedIndex = -1;
    }
    if (customHeaders.Count > 3)
    {
      this.custom4Label.Visibility = System.Windows.Visibility.Visible;
      this.custom4TextBox.Visibility = System.Windows.Visibility.Visible;
      this.custom4ClearButton.Visibility = System.Windows.Visibility.Visible;
      this.custom4Label.Content = (object) customHeaders[3];
      if (this.assemViewModel.Assembly != null)
      {
        Parameter parameter17 = this.assemViewModel.Assembly.LookupParameter(this.parameterNames[3]) ?? this.assemViewModel.Assembly.GetStructuralFramingElement().GetTopLevelElement().LookupParameter(this.parameterNames[3]);
        if (parameter17 != null)
        {
          if (parameter17.IsReadOnly)
          {
            this.custom4TextBox.IsEnabled = false;
            this.custom4ComboBox.IsEnabled = false;
            this.custom4ClearButton.IsEnabled = false;
          }
          if (parameter17.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom4TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom4ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom4ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter17 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter18 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[3]);
          if (parameter18 != null)
          {
            if (parameter18.IsReadOnly)
            {
              this.custom4TextBox.IsEnabled = false;
              this.custom4ComboBox.IsEnabled = false;
              this.custom4ClearButton.IsEnabled = false;
            }
            if (parameter18.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom4TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom4ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom4ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      else if (element != null)
      {
        Parameter parameter19 = element.GetTopLevelElement().LookupParameter(this.parameterNames[3]);
        if (parameter19 != null)
        {
          if (parameter19.IsReadOnly)
          {
            this.custom4TextBox.IsEnabled = false;
            this.custom4ComboBox.IsEnabled = false;
            this.custom4ClearButton.IsEnabled = false;
          }
          else
          {
            foreach (Element elem in this.assemViewModel.SFElementsOfThisControlMark)
            {
              Parameter parameter20 = elem.GetTopLevelElement().LookupParameter(this.parameterNames[3]);
              if (parameter20 != null && parameter20.IsReadOnly)
              {
                this.custom4TextBox.IsEnabled = false;
                this.custom4ComboBox.IsEnabled = false;
                this.custom4ClearButton.IsEnabled = false;
                break;
              }
            }
          }
          if (parameter19.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
          {
            this.custom4TextBox.Visibility = System.Windows.Visibility.Hidden;
            this.custom4ClearButton.Visibility = System.Windows.Visibility.Hidden;
            this.custom4ComboBox.Visibility = System.Windows.Visibility.Visible;
          }
        }
        else if (parameter19 == null && typeId != ElementId.InvalidElementId)
        {
          Parameter parameter21 = this.revitDoc.GetElement(typeId).GetTopLevelElement().LookupParameter(this.parameterNames[3]);
          if (parameter21 != null)
          {
            if (parameter21.IsReadOnly)
            {
              this.custom4TextBox.IsEnabled = false;
              this.custom4ComboBox.IsEnabled = false;
              this.custom4ClearButton.IsEnabled = false;
            }
            if (parameter21.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
            {
              this.custom4TextBox.Visibility = System.Windows.Visibility.Hidden;
              this.custom4ClearButton.Visibility = System.Windows.Visibility.Hidden;
              this.custom4ComboBox.Visibility = System.Windows.Visibility.Visible;
            }
          }
        }
      }
      this.custom4TextBox.Text = stringList1[5];
      if (stringList1[5].ToUpper().Equals("YES"))
        this.custom4ComboBox.SelectedIndex = 0;
      else if (stringList1[5].ToUpper().Equals("NO"))
        this.custom4ComboBox.SelectedIndex = 1;
      else
        this.custom4ComboBox.SelectedIndex = -1;
    }
    List<string> stringList2 = new List<string>();
    List<string> stringList3 = new List<string>();
    List<string> stringList4 = new List<string>()
    {
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
          if (category.Name.Equals("Assemblies") && !stringList2.Contains(bindingMapIterator.Key.Name) && !stringList4.Contains(bindingMapIterator.Key.Name.ToUpper()) && (bindingMapIterator.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
            stringList2.Add(bindingMapIterator.Key.Name);
          if (category.Name.Equals("Structural Framing") && !stringList3.Contains(bindingMapIterator.Key.Name) && !stringList4.Contains(bindingMapIterator.Key.Name.ToUpper()) && (bindingMapIterator.Key.GetDataType() == SpecTypeId.String.Text || bindingMapIterator.Key.GetDataType() == SpecTypeId.Boolean.YesNo))
            stringList3.Add(bindingMapIterator.Key.Name);
        }
      }
    }
    if (this.assemViewModel.Assemblied.ToUpper().Equals("YES"))
    {
      if (!stringList2.Contains(this.assemViewModel.CustomColumnName1) && !stringList3.Contains(this.assemViewModel.CustomColumnName1))
      {
        this.custom1ComboBox.IsEnabled = false;
        this.custom1TextBox.IsEnabled = false;
        this.custom1ComboBox.SelectedIndex = -1;
        this.custom1TextBox.Text = "";
        this.custom1ClearButton.IsEnabled = false;
        this.custom1TextBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom1ComboBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom1Label.Visibility = System.Windows.Visibility.Hidden;
        this.custom1ClearButton.Visibility = System.Windows.Visibility.Hidden;
      }
      if (!stringList2.Contains(this.assemViewModel.CustomColumnName2) && !stringList3.Contains(this.assemViewModel.CustomColumnName2))
      {
        this.custom2ComboBox.IsEnabled = false;
        this.custom2TextBox.IsEnabled = false;
        this.custom2ComboBox.SelectedIndex = -1;
        this.custom2TextBox.Text = "";
        this.custom2ClearButton.IsEnabled = false;
        this.custom2TextBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom2ComboBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom2Label.Visibility = System.Windows.Visibility.Hidden;
        this.custom2ClearButton.Visibility = System.Windows.Visibility.Hidden;
      }
      if (!stringList2.Contains(this.assemViewModel.CustomColumnName3) && !stringList3.Contains(this.assemViewModel.CustomColumnName3))
      {
        this.custom3ComboBox.IsEnabled = false;
        this.custom3TextBox.IsEnabled = false;
        this.custom3ComboBox.SelectedIndex = -1;
        this.custom3TextBox.Text = "";
        this.custom3ClearButton.IsEnabled = false;
        this.custom3TextBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom3ComboBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom3Label.Visibility = System.Windows.Visibility.Hidden;
        this.custom3ClearButton.Visibility = System.Windows.Visibility.Hidden;
      }
      if (stringList2.Contains(this.assemViewModel.CustomColumnName4) || stringList3.Contains(this.assemViewModel.CustomColumnName4))
        return;
      this.custom4ComboBox.IsEnabled = false;
      this.custom4TextBox.IsEnabled = false;
      this.custom4ComboBox.SelectedIndex = -1;
      this.custom4TextBox.Text = "";
      this.custom4ClearButton.IsEnabled = false;
      this.custom4TextBox.Visibility = System.Windows.Visibility.Hidden;
      this.custom4ComboBox.Visibility = System.Windows.Visibility.Hidden;
      this.custom4Label.Visibility = System.Windows.Visibility.Hidden;
      this.custom4ClearButton.Visibility = System.Windows.Visibility.Hidden;
    }
    else
    {
      if (!this.assemViewModel.Assemblied.ToUpper().Equals("NO"))
        return;
      if (!stringList3.Contains(this.assemViewModel.CustomColumnName1))
      {
        this.custom1ComboBox.IsEnabled = false;
        this.custom1TextBox.IsEnabled = false;
        this.custom1ComboBox.SelectedIndex = -1;
        this.custom1TextBox.Text = "";
        this.custom1ClearButton.IsEnabled = false;
        this.custom1TextBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom1ComboBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom1Label.Visibility = System.Windows.Visibility.Hidden;
        this.custom1ClearButton.Visibility = System.Windows.Visibility.Hidden;
      }
      if (!stringList3.Contains(this.assemViewModel.CustomColumnName2))
      {
        this.custom2ComboBox.IsEnabled = false;
        this.custom2TextBox.IsEnabled = false;
        this.custom2ComboBox.SelectedIndex = -1;
        this.custom2TextBox.Text = "";
        this.custom2ClearButton.IsEnabled = false;
        this.custom2TextBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom2ComboBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom2Label.Visibility = System.Windows.Visibility.Hidden;
        this.custom2ClearButton.Visibility = System.Windows.Visibility.Hidden;
      }
      if (!stringList3.Contains(this.assemViewModel.CustomColumnName3))
      {
        this.custom3ComboBox.IsEnabled = false;
        this.custom3TextBox.IsEnabled = false;
        this.custom3ComboBox.SelectedIndex = -1;
        this.custom3TextBox.Text = "";
        this.custom3ClearButton.IsEnabled = false;
        this.custom3TextBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom3ComboBox.Visibility = System.Windows.Visibility.Hidden;
        this.custom3Label.Visibility = System.Windows.Visibility.Hidden;
        this.custom3ClearButton.Visibility = System.Windows.Visibility.Hidden;
      }
      if (stringList3.Contains(this.assemViewModel.CustomColumnName4))
        return;
      this.custom4ComboBox.IsEnabled = false;
      this.custom4TextBox.IsEnabled = false;
      this.custom4ComboBox.SelectedIndex = -1;
      this.custom4TextBox.Text = "";
      this.custom4ClearButton.IsEnabled = false;
      this.custom4TextBox.Visibility = System.Windows.Visibility.Hidden;
      this.custom4ComboBox.Visibility = System.Windows.Visibility.Hidden;
      this.custom4Label.Visibility = System.Windows.Visibility.Hidden;
      this.custom4ClearButton.Visibility = System.Windows.Visibility.Hidden;
    }
  }

  private void ConfirmButton_Click(object sender, RoutedEventArgs e)
  {
    List<string> stringList1 = new List<string>();
    string str1 = "";
    string str2 = "";
    string str3 = "";
    string str4 = "";
    Parameter parameter1 = (Parameter) null;
    Parameter parameter2 = (Parameter) null;
    Parameter parameter3 = (Parameter) null;
    Parameter parameter4 = (Parameter) null;
    int result = 0;
    int.TryParse(this.assemViewModel.ElementId, out result);
    Element element = this.revitDoc.GetElement(new ElementId(result));
    if (this.assemViewModel.Assembly != null)
    {
      AssemblyInstance assembly = this.assemViewModel.Assembly;
      if (this.parameterNames.Count > 0)
        parameter1 = assembly.LookupParameter(this.parameterNames[0]);
      if (this.parameterNames.Count > 1)
        parameter2 = assembly.LookupParameter(this.parameterNames[1]);
      if (this.parameterNames.Count > 2)
        parameter3 = assembly.LookupParameter(this.parameterNames[2]);
      if (this.parameterNames.Count > 3)
        parameter4 = assembly.LookupParameter(this.parameterNames[3]);
      if (parameter1 == null && this.parameterNames.Count > 0)
        parameter1 = this.assemViewModel.Assembly.GetStructuralFramingElement().LookupParameter(this.parameterNames[0]);
      if (parameter2 == null && this.parameterNames.Count > 1)
        parameter2 = this.assemViewModel.Assembly.GetStructuralFramingElement().LookupParameter(this.parameterNames[1]);
      if (parameter3 == null && this.parameterNames.Count > 2)
        parameter3 = this.assemViewModel.Assembly.GetStructuralFramingElement().LookupParameter(this.parameterNames[2]);
      if (parameter4 == null && this.parameterNames.Count > 3)
        parameter4 = this.assemViewModel.Assembly.GetStructuralFramingElement().LookupParameter(this.parameterNames[3]);
    }
    else if (element != null)
    {
      if (this.parameterNames.Count > 0)
        parameter1 = element.LookupParameter(this.parameterNames[0]);
      if (this.parameterNames.Count > 1)
        parameter2 = element.LookupParameter(this.parameterNames[1]);
      if (this.parameterNames.Count > 2)
        parameter3 = element.LookupParameter(this.parameterNames[2]);
      if (this.parameterNames.Count > 3)
        parameter4 = element.LookupParameter(this.parameterNames[3]);
    }
    if (parameter1 != null && parameter1.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
      str1 = this.custom1ComboBox.SelectedIndex != 0 || !this.custom1ComboBox.IsEnabled ? (this.custom1ComboBox.SelectedIndex != 1 || !this.custom1ComboBox.IsEnabled ? (string) null : "0") : "1";
    else if (!this.custom1TextBox.Text.Equals("") && this.custom1TextBox.IsEnabled)
    {
      if (!this.custom1TextBox.Text.Equals("") || !this.custom1TextBox.Text.Equals((string) null))
      {
        stringList1.Add(this.custom1TextBox.Text);
        str1 = this.custom1TextBox.Text;
      }
    }
    else if (this.custom1TextBox.Visibility == System.Windows.Visibility.Visible && this.custom1TextBox.IsEnabled)
    {
      stringList1.Add("");
      str1 = "";
    }
    else
      str1 = (string) null;
    if (parameter2 != null && parameter2.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
      str2 = this.custom2ComboBox.SelectedIndex != 0 || !this.custom2ComboBox.IsEnabled ? (this.custom2ComboBox.SelectedIndex != 1 || !this.custom2ComboBox.IsEnabled ? (string) null : "0") : "1";
    else if (!this.custom2TextBox.Text.Equals("") && this.custom2TextBox.IsEnabled)
    {
      if (!this.custom2TextBox.Text.Equals("") || !this.custom2TextBox.Text.Equals((string) null))
      {
        stringList1.Add(this.custom2TextBox.Text);
        str2 = this.custom2TextBox.Text;
      }
    }
    else if (this.custom2TextBox.Visibility == System.Windows.Visibility.Visible && this.custom2TextBox.IsEnabled)
    {
      stringList1.Add("");
      str2 = "";
    }
    else
      str2 = (string) null;
    if (parameter3 != null && parameter3.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
      str3 = this.custom3ComboBox.SelectedIndex != 0 || !this.custom3ComboBox.IsEnabled ? (this.custom3ComboBox.SelectedIndex != 1 || !this.custom3ComboBox.IsEnabled ? (string) null : "0") : "1";
    else if (!this.custom3TextBox.Text.Equals("") && this.custom3TextBox.IsEnabled)
    {
      if (!this.custom3TextBox.Text.Equals("") || !this.custom3TextBox.Text.Equals((string) null))
      {
        stringList1.Add(this.custom3TextBox.Text);
        str3 = this.custom3TextBox.Text;
      }
    }
    else if (this.custom3TextBox.Visibility == System.Windows.Visibility.Visible && this.custom3TextBox.IsEnabled)
    {
      stringList1.Add("");
      str3 = "";
    }
    else
      str3 = (string) null;
    if (parameter4 != null && parameter4.Definition.GetDataType() == SpecTypeId.Boolean.YesNo)
      str4 = this.custom4ComboBox.SelectedIndex != 0 || !this.custom4ComboBox.IsEnabled ? (this.custom4ComboBox.SelectedIndex != 1 || !this.custom4ComboBox.IsEnabled ? (string) null : "0") : "1";
    else if (!this.custom4TextBox.Text.Equals("") && this.custom4TextBox.IsEnabled)
    {
      if (!this.custom4TextBox.Text.Equals("") || !this.custom4TextBox.Text.Equals((string) null))
      {
        stringList1.Add(this.custom4TextBox.Text);
        str4 = this.custom4TextBox.Text;
      }
    }
    else if (this.custom4TextBox.Visibility == System.Windows.Visibility.Visible && this.custom4TextBox.IsEnabled)
    {
      stringList1.Add("");
      str4 = "";
    }
    else
      str4 = (string) null;
    int num = 0;
    List<string> stringList2 = new List<string>();
    if (num == 0)
    {
      this.assemViewModel.Custom1 = str1;
      this.assemViewModel.Custom2 = str2;
      this.assemViewModel.Custom3 = str3;
      this.assemViewModel.Custom4 = str4;
      ((MainViewModel) App.TicketManagerWindow.DataContext).EditingItem = this.assemViewModel;
      ((MainViewModel) App.TicketManagerWindow.DataContext).UpdateParametersCommand.Execute((object) null);
    }
    if (num != 0)
      return;
    this.closed = true;
    this.Close();
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e)
  {
    this.closed = true;
    this.Close();
    App.TicketManagerWindow.DataGrid.IsEnabled = true;
  }

  private void ClearAllButton_Click(object sender, RoutedEventArgs e)
  {
    this.custom1TextBox.Text = "";
    this.custom1ComboBox.SelectedIndex = 1;
    this.custom2TextBox.Text = "";
    this.custom2ComboBox.SelectedIndex = 1;
    this.custom3TextBox.Text = "";
    this.custom3ComboBox.SelectedIndex = 1;
    this.custom4TextBox.Text = "";
    this.custom4ComboBox.SelectedIndex = 1;
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton_Click(object sender, RoutedEventArgs e)
  {
    List<string> stringList = new List<string>();
    stringList.AddRange((IEnumerable<string>) this.RowContents);
    if (this.CustomHeads.Count > 0)
    {
      this.custom1TextBox.Text = stringList[2];
      if (stringList[2].ToUpper().Equals("YES"))
        this.custom1ComboBox.SelectedIndex = 0;
      else if (stringList[2].ToUpper().Equals("NO"))
        this.custom1ComboBox.SelectedIndex = 1;
      else
        this.custom1ComboBox.SelectedIndex = -1;
    }
    if (this.CustomHeads.Count > 1)
    {
      this.custom2TextBox.Text = stringList[3];
      if (stringList[3].ToUpper().Equals("YES"))
        this.custom2ComboBox.SelectedIndex = 0;
      else if (stringList[3].ToUpper().Equals("NO"))
        this.custom2ComboBox.SelectedIndex = 1;
      else
        this.custom2ComboBox.SelectedIndex = -1;
    }
    if (this.CustomHeads.Count > 2)
    {
      this.custom3TextBox.Text = stringList[4];
      if (stringList[4].ToUpper().Equals("YES"))
        this.custom3ComboBox.SelectedIndex = 0;
      else if (stringList[4].ToUpper().Equals("NO"))
        this.custom3ComboBox.SelectedIndex = 1;
      else
        this.custom3ComboBox.SelectedIndex = -1;
    }
    if (this.CustomHeads.Count <= 3)
      return;
    this.custom4TextBox.Text = stringList[5];
    if (stringList[5].ToUpper().Equals("YES"))
      this.custom4ComboBox.SelectedIndex = 0;
    else if (stringList[5].ToUpper().Equals("NO"))
      this.custom4ComboBox.SelectedIndex = 1;
    else
      this.custom4ComboBox.SelectedIndex = -1;
  }

  private void ResetButton2_Click(object sender, RoutedEventArgs e)
  {
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton3_Click(object sender, RoutedEventArgs e)
  {
    this.custom1TextBox.Text = "";
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton4_Click(object sender, RoutedEventArgs e)
  {
    this.custom2TextBox.Text = "";
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton5_Click(object sender, RoutedEventArgs e)
  {
    this.custom3TextBox.Text = "";
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
    this.label2_Copy4.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton6_Click(object sender, RoutedEventArgs e)
  {
    this.custom4TextBox.Text = "";
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
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/ticketmanager/views/editwindow.xaml", UriKind.Relative));
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
        ((ButtonBase) target).Click += new RoutedEventHandler(this.ConfirmButton_Click);
        break;
      case 3:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.CancelButton_Click);
        break;
      case 4:
        this.label = (Label) target;
        break;
      case 5:
        this.custom1TextBox = (TextBox) target;
        break;
      case 6:
        this.custom2TextBox = (TextBox) target;
        break;
      case 7:
        this.label1 = (Label) target;
        break;
      case 8:
        this.custom1Label = (Label) target;
        break;
      case 9:
        this.custom2Label = (Label) target;
        break;
      case 10:
        this.label2_Copy3 = (Label) target;
        break;
      case 11:
        this.label2_Copy4 = (Label) target;
        break;
      case 12:
        this.custom2ClearButton = (Button) target;
        this.custom2ClearButton.Click += new RoutedEventHandler(this.ResetButton4_Click);
        break;
      case 13:
        this.custom1ClearButton = (Button) target;
        this.custom1ClearButton.Click += new RoutedEventHandler(this.ResetButton3_Click);
        break;
      case 14:
        this.custom3Label = (Label) target;
        break;
      case 15:
        this.custom3TextBox = (TextBox) target;
        break;
      case 16 /*0x10*/:
        this.custom3ClearButton = (Button) target;
        this.custom3ClearButton.Click += new RoutedEventHandler(this.ResetButton5_Click);
        break;
      case 17:
        this.custom4Label = (Label) target;
        break;
      case 18:
        this.custom4TextBox = (TextBox) target;
        break;
      case 19:
        this.custom4ClearButton = (Button) target;
        this.custom4ClearButton.Click += new RoutedEventHandler(this.ResetButton6_Click);
        break;
      case 20:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.ClearAllButton_Click);
        break;
      case 21:
        this.custom1ComboBox = (ComboBox) target;
        break;
      case 22:
        this.custom2ComboBox = (ComboBox) target;
        break;
      case 23:
        this.custom3ComboBox = (ComboBox) target;
        break;
      case 24:
        this.custom4ComboBox = (ComboBox) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
