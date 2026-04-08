// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.InitialPresentation.MKInitialUserSelection
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.InitialPresentation;

public class MKInitialUserSelection : Window, IComponentConnector, IStyleConnector
{
  private List<SFEntry> Items = new List<SFEntry>();
  private UIDocument uiDoc;
  public bool isContinue;
  public bool isCanceled;
  internal CheckBox cbAllFeatures;
  internal ListBox lbTodoList;
  internal CheckBox TraditionalApproach;
  internal CheckBox DetailedApproach;
  internal Button btn1;
  internal Button canclebtn;
  private bool _contentLoaded;

  public MKInitialUserSelection(IEnumerable<SFEntry> ids, UIDocument uidoc)
  {
    this.InitializeComponent();
    this.Items.AddRange(ids);
    this.lbTodoList.ItemsSource = (IEnumerable) this.Items;
    this.uiDoc = uidoc;
  }

  public IEnumerable<SFEntry> GetItems() => (IEnumerable<SFEntry>) this.Items;

  private void cbAllFeatures_CheckedChanged(object sender, RoutedEventArgs e)
  {
    bool valueOrDefault = this.cbAllFeatures.IsChecked.GetValueOrDefault();
    IEnumerable<SFEntry> items = this.GetItems();
    if (!valueOrDefault)
    {
      foreach (SFEntry sfEntry in items)
        sfEntry.IsSelected = valueOrDefault;
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) new List<ElementId>());
    }
    else
    {
      List<ElementId> elementIdList = new List<ElementId>();
      foreach (SFEntry sfEntry in items)
      {
        elementIdList.Add(this.uiDoc.Document.GetElement(sfEntry.elemid).GetTopLevelElement().Id);
        sfEntry.IsSelected = valueOrDefault;
      }
      this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
    }
  }

  private void ElementName_Checked(object sender, RoutedEventArgs e)
  {
    IEnumerable<SFEntry> items = this.GetItems();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (SFEntry sfEntry in items)
    {
      if (sfEntry.IsSelected)
        elementIdList.Add(this.uiDoc.Document.GetElement(sfEntry.elemid).GetTopLevelElement().Id);
    }
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  private void ElementName_Unchecked(object sender, RoutedEventArgs e)
  {
    IEnumerable<SFEntry> items = this.GetItems();
    List<ElementId> elementIdList = new List<ElementId>();
    foreach (SFEntry sfEntry in items)
    {
      if (sfEntry.IsSelected)
        elementIdList.Add(this.uiDoc.Document.GetElement(sfEntry.elemid).GetTopLevelElement().Id);
    }
    this.uiDoc.Selection.SetElementIds((ICollection<ElementId>) elementIdList);
  }

  private void btn1_Click(object sender, RoutedEventArgs e)
  {
    this.isContinue = true;
    this.Close();
  }

  private void canclebtn_Click(object sender, RoutedEventArgs e)
  {
    this.isCanceled = true;
    this.Close();
  }

  private void TraditionalApproach_Click(object sender, RoutedEventArgs e)
  {
  }

  private void DetailedApproach_Click(object sender, RoutedEventArgs e)
  {
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/initialpresentation/mkinitialuserselection.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.cbAllFeatures = (CheckBox) target;
        this.cbAllFeatures.Checked += new RoutedEventHandler(this.cbAllFeatures_CheckedChanged);
        this.cbAllFeatures.Unchecked += new RoutedEventHandler(this.cbAllFeatures_CheckedChanged);
        break;
      case 2:
        this.lbTodoList = (ListBox) target;
        break;
      case 4:
        this.TraditionalApproach = (CheckBox) target;
        this.TraditionalApproach.Click += new RoutedEventHandler(this.TraditionalApproach_Click);
        break;
      case 5:
        this.DetailedApproach = (CheckBox) target;
        this.DetailedApproach.Click += new RoutedEventHandler(this.DetailedApproach_Click);
        break;
      case 6:
        this.btn1 = (Button) target;
        this.btn1.Click += new RoutedEventHandler(this.btn1_Click);
        break;
      case 7:
        this.canclebtn = (Button) target;
        this.canclebtn.Click += new RoutedEventHandler(this.canclebtn_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IStyleConnector.Connect(int connectionId, object target)
  {
    if (connectionId != 3)
      return;
    ((ToggleButton) target).Checked += new RoutedEventHandler(this.ElementName_Checked);
    ((ToggleButton) target).Unchecked += new RoutedEventHandler(this.ElementName_Unchecked);
  }
}
