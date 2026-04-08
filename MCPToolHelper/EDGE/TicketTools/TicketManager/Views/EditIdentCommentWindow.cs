// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.Views.EditIdentCommentWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketManager.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Utils.AssemblyUtils;

#nullable disable
namespace EDGE.TicketTools.TicketManager.Views;

public class EditIdentCommentWindow : Window, IComponentConnector
{
  private Document revitDoc;
  private string ticketManagerCustomizationPath = "";
  private string manufacturer = "";
  public bool closed;
  public List<string> RowContents = new List<string>();
  public List<string> CustomHeads = new List<string>();
  public List<string> parameterNames = new List<string>();
  public AssemblyViewModel assemViewModel;
  public string oldvalue = "";
  internal Label DataGridTitle;
  internal Label label;
  internal TextBox identCommentTextBox;
  internal Label label1;
  internal Label identCommentLabel;
  internal Label label2_Copy3;
  private bool _contentLoaded;

  public EditIdentCommentWindow(
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
    this.label.Content = (object) $"Edit Identity Comment for {this.assemViewModel.MarkNumber.ToString()} and click save to commit changes:";
    List<string> stringList1 = new List<string>();
    stringList1.AddRange((IEnumerable<string>) this.RowContents);
    this.identCommentTextBox.Text = !stringList1[0].Equals("Parameter values are not the same across mark numbers.") ? stringList1[0] : "";
    int result = 0;
    int.TryParse(this.assemViewModel.ElementId, out result);
    Element element = this.revitDoc.GetElement(new ElementId(result));
    element.GetTypeId();
    if (element.LookupParameter("IDENTITY_COMMENT") != null)
      this.oldvalue = element.LookupParameter("IDENTITY_COMMENT").AsString();
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
      if (stringList2.Contains("IDENTITY_COMMENT") || stringList3.Contains("IDENTITY_COMMENT"))
        return;
      this.identCommentTextBox.IsEnabled = false;
      this.identCommentTextBox.Text = "";
    }
    else
    {
      if (!this.assemViewModel.Assemblied.ToUpper().Equals("NO") || stringList3.Contains("IDENTITY_COMMENT"))
        return;
      this.identCommentTextBox.IsEnabled = false;
      this.identCommentTextBox.Text = "";
    }
  }

  private void ConfirmButton_Click(object sender, RoutedEventArgs e)
  {
    List<string> stringList1 = new List<string>();
    string str1 = "";
    int result = 0;
    int.TryParse(this.assemViewModel.ElementId, out result);
    Element element = this.revitDoc.GetElement(new ElementId(result));
    if (!this.identCommentTextBox.Text.Equals("") && this.identCommentTextBox.IsEnabled)
    {
      if (!this.identCommentTextBox.Text.Equals("") || !this.identCommentTextBox.Text.Equals((string) null))
      {
        stringList1.Add(this.identCommentTextBox.Text);
        str1 = this.identCommentTextBox.Text;
      }
    }
    else if (this.identCommentTextBox.Visibility == System.Windows.Visibility.Visible && this.identCommentTextBox.IsEnabled)
    {
      stringList1.Add("");
      str1 = "";
    }
    string str2 = str1;
    string str3 = this.oldvalue;
    int num = 0;
    if (string.IsNullOrEmpty(str3))
      str3 = "null";
    if (string.IsNullOrEmpty(str2))
      str2 = "null";
    Parameters.AddEditComment(element, "UPDATED", $"IDENTITY_COMMENT has been updated from {str3} to {str2}");
    List<string> stringList2 = new List<string>();
    if (num == 0)
    {
      this.assemViewModel.IdentityComment = str1;
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
    this.identCommentTextBox.Text = "";
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton_Click(object sender, RoutedEventArgs e)
  {
    List<string> stringList = new List<string>();
    stringList.AddRange((IEnumerable<string>) this.RowContents);
    if (stringList[0].Equals("Parameter values are not the same across mark numbers."))
      this.identCommentTextBox.Text = "";
    else
      this.identCommentTextBox.Text = stringList[0];
  }

  private void ResetButton2_Click(object sender, RoutedEventArgs e)
  {
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton3_Click(object sender, RoutedEventArgs e)
  {
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton4_Click(object sender, RoutedEventArgs e)
  {
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton5_Click(object sender, RoutedEventArgs e)
  {
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
  }

  private void ResetButton6_Click(object sender, RoutedEventArgs e)
  {
    this.label2_Copy3.Visibility = System.Windows.Visibility.Hidden;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/ticketmanager/views/editidentcommentwindow.xaml", UriKind.Relative));
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
        this.identCommentTextBox = (TextBox) target;
        break;
      case 6:
        this.label1 = (Label) target;
        break;
      case 7:
        this.identCommentLabel = (Label) target;
        break;
      case 8:
        this.label2_Copy3 = (Label) target;
        break;
      case 9:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.ClearAllButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
