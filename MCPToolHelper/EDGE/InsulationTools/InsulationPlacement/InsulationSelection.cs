// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.InsulationSelection
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement;

public class InsulationSelection : System.Windows.Window, IComponentConnector
{
  private bool buttonclicked;
  private List<Family> allAvailable = new List<Family>();
  internal InsulationSelection Window;
  internal TextBlock titleBlock;
  internal ListView insulations;
  internal Button okbutton;
  private bool _contentLoaded;

  public InsulationSelection(List<Element> values, IntPtr parentWindowHandler, bool familyOnly)
  {
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((System.Windows.Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.InitializeComponent();
    if (familyOnly)
    {
      List<Family> source = new List<Family>();
      foreach (Element element1 in values)
      {
        FamilySymbol familySymbol = element1 as FamilySymbol;
        Family family = familySymbol.Family;
        string familyName = familySymbol.FamilyName;
        bool flag = false;
        if (source.Count<Family>() > 0)
        {
          foreach (Element element2 in source)
          {
            if (element2.Name.Equals(family.Name))
              flag = true;
          }
          if (!flag)
            source.Add(family);
        }
        else
          source.Add(family);
      }
      source.Sort((Comparison<Family>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Name, q.Name)));
      this.allAvailable = source;
      foreach (Family family in source)
      {
        ListViewItem newItem = new ListViewItem();
        newItem.DataContext = (object) family.Name;
        newItem.Content = (object) family.Name;
        this.insulations.Items.Add((object) newItem);
      }
    }
    else
    {
      values.Sort((Comparison<Element>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Name, q.Name)));
      List<FamilySymbol> familySymbolList = new List<FamilySymbol>();
      FamilySymbol elem = values.First<Element>() as FamilySymbol;
      string familyName = elem.FamilyName;
      Parameter parameter = Parameters.LookupParameter((Element) elem, "DIM_THICKNESS");
      if (parameter != null)
        this.titleBlock.Text = $"Please select the type of {familyName} : {parameter.AsValueString()} family you would like to place in the structural framing elements in the model";
      else
        this.titleBlock.Text = $"Please select the type of {familyName} family you would like to place in the structural framing elements in the model";
      foreach (Element element in values)
      {
        FamilySymbol familySymbol = element as FamilySymbol;
        ListViewItem newItem = new ListViewItem();
        newItem.DataContext = (object) familySymbol.Name;
        newItem.Content = (object) familySymbol.Name;
        this.insulations.Items.Add((object) newItem);
      }
    }
  }

  private void Window_Closed(object sender, EventArgs e)
  {
    if (!this.buttonclicked)
      this.insulations.SelectedItem = (object) null;
    this.Window.Close();
  }

  private void oKButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.insulations.SelectedItem == null)
      return;
    this.buttonclicked = true;
    this.Window.Close();
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e)
  {
    this.insulations.SelectedItem = (object) null;
    this.Close();
  }

  private void Insulations_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (this.insulations.SelectedItem == null)
      return;
    this.okbutton.IsEnabled = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/insulationtools/insulationplacement/insulationselection.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.Window = (InsulationSelection) target;
        this.Window.Closed += new EventHandler(this.Window_Closed);
        break;
      case 2:
        this.titleBlock = (TextBlock) target;
        break;
      case 3:
        this.insulations = (ListView) target;
        this.insulations.SelectionChanged += new SelectionChangedEventHandler(this.Insulations_SelectionChanged);
        break;
      case 4:
        this.okbutton = (Button) target;
        this.okbutton.Click += new RoutedEventHandler(this.oKButton_Click);
        break;
      case 5:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
