// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CloneTicket.AssemblySelectionWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.CloneTicket;

public class AssemblySelectionWindow : Window, IComponentConnector, IStyleConnector
{
  public ElementId selectedSource;
  private string sourceName = string.Empty;
  public List<ElementId> selectedTargets = new List<ElementId>();
  private List<AssemblyInstance> sourceList = new List<AssemblyInstance>();
  private List<AssemblyInstance> originalList = new List<AssemblyInstance>();
  private bool sourceSelected;
  internal TextBlock DescriptionTextBlock;
  internal TextBox SearchBox;
  internal Label WaterMarkLabel;
  internal ListBox CheckBoxList;
  internal Button ContinueButton;
  internal Button CancelButton;
  private bool _contentLoaded;

  public AssemblySelectionWindow(
    List<AssemblyInstance> assemblies,
    List<AssemblyInstance> possibleSources,
    Document revitDoc,
    bool preselectedSource)
  {
    this.InitializeComponent();
    AssemblySelectionViewModel dataContext = new AssemblySelectionViewModel();
    possibleSources.Sort((Comparison<AssemblyInstance>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Name, q.Name)));
    assemblies.Sort((Comparison<AssemblyInstance>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Name, q.Name)));
    this.sourceList = possibleSources;
    this.originalList = assemblies;
    if (preselectedSource)
    {
      this.selectedSource = possibleSources.FirstOrDefault<AssemblyInstance>().Id;
      this.sourceName = possibleSources.FirstOrDefault<AssemblyInstance>().Name;
      this.sourceSelected = true;
      this.DisplayOriginalList(dataContext);
    }
    else
    {
      List<AssemblyListObject> assemblyListObjectList = new List<AssemblyListObject>();
      foreach (Element possibleSource in possibleSources)
      {
        string assemblyName = possibleSource.Name.Replace("_", "__");
        assemblyListObjectList.Add(new AssemblyListObject(assemblyName, possibleSource.Id));
      }
      assemblyListObjectList.Sort((Comparison<AssemblyListObject>) ((p, q) => Utils.MiscUtils.MiscUtils.CompareStrings(p.Name, q.Name)));
      assemblyListObjectList.ForEach((Action<AssemblyListObject>) (e => dataContext.AssemblyList.Add(e)));
    }
    this.DataContext = (object) dataContext;
  }

  private void CheckBox_Checked(object sender, RoutedEventArgs e)
  {
    if (this.sourceSelected)
      return;
    CheckBox checkBox = sender as CheckBox;
    foreach (AssemblyListObject assembly in (Collection<AssemblyListObject>) (this.DataContext as AssemblySelectionViewModel).AssemblyList)
    {
      if (assembly.IsChecked && (object) assembly.Name != checkBox.Content)
        assembly.IsChecked = false;
    }
    this.CheckBoxList.Items.Refresh();
  }

  private void ContinueButton_Click(object sender, RoutedEventArgs e)
  {
    AssemblySelectionViewModel dataContext = this.DataContext as AssemblySelectionViewModel;
    if (!this.sourceSelected)
    {
      foreach (AssemblyListObject assembly in (Collection<AssemblyListObject>) dataContext.AssemblyList)
      {
        if (assembly.IsChecked)
        {
          this.selectedSource = assembly.assemblyId;
          this.sourceName = assembly.Name;
          this.sourceSelected = true;
          break;
        }
      }
      if (!this.sourceSelected)
      {
        int num = (int) MessageBox.Show("Please select an assembly.");
      }
      else
        this.DisplayOriginalList(dataContext);
    }
    else
    {
      foreach (AssemblyListObject assembly in (Collection<AssemblyListObject>) dataContext.AssemblyList)
      {
        if (assembly.IsChecked)
          this.selectedTargets.Add(assembly.assemblyId);
      }
      if (this.selectedTargets.Count<ElementId>() == 0)
      {
        int num = (int) MessageBox.Show("Please select one or more assemblies.");
      }
      else
      {
        this.DialogResult = new bool?(true);
        this.Close();
      }
    }
  }

  private void DisplayOriginalList(AssemblySelectionViewModel dataContext)
  {
    this.SearchBox.Text = "";
    dataContext.AssemblyList.Clear();
    List<AssemblyInstance> assemblyInstanceList = new List<AssemblyInstance>();
    foreach (AssemblyInstance original in this.originalList)
    {
      if (original.Id != this.selectedSource)
        assemblyInstanceList.Add(original);
    }
    foreach (Element element in assemblyInstanceList)
      this.AddToAssemblyList(dataContext, element.Name, element.Id);
    this.DescriptionTextBlock.Text = "Please select assemblies:\nCopying from: " + this.sourceName;
  }

  private void CancelButton_Click(object sender, RoutedEventArgs e)
  {
    this.DialogResult = new bool?(false);
    this.Close();
  }

  private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    AssemblySelectionViewModel dataContext = this.DataContext as AssemblySelectionViewModel;
    string text = (sender as TextBox).Text;
    if (string.IsNullOrEmpty(text))
    {
      this.WaterMarkLabel.Visibility = System.Windows.Visibility.Visible;
      List<AssemblyInstance> assemblyInstanceList = new List<AssemblyInstance>();
      if (!this.sourceSelected)
      {
        assemblyInstanceList = this.sourceList;
      }
      else
      {
        foreach (AssemblyInstance original in this.originalList)
        {
          if (original.Id != this.selectedSource)
            assemblyInstanceList.Add(original);
        }
      }
      dataContext.AssemblyList.Clear();
      foreach (Element element in assemblyInstanceList)
        this.AddToAssemblyList(dataContext, element.Name, element.Id);
    }
    else
    {
      this.WaterMarkLabel.Visibility = System.Windows.Visibility.Collapsed;
      List<AssemblyInstance> assemblyInstanceList = new List<AssemblyInstance>();
      if (!this.sourceSelected)
      {
        assemblyInstanceList = this.sourceList;
      }
      else
      {
        foreach (AssemblyInstance original in this.originalList)
        {
          if (original.Id != this.selectedSource)
            assemblyInstanceList.Add(original);
        }
      }
      dataContext.AssemblyList.Clear();
      foreach (Element element in assemblyInstanceList)
      {
        if (element.Name.ToUpper().Contains(text.Trim().ToUpper()))
          this.AddToAssemblyList(dataContext, element.Name, element.Id);
      }
    }
  }

  private void AddToAssemblyList(
    AssemblySelectionViewModel dataContext,
    string name,
    ElementId elemId)
  {
    name = name.Replace("_", "__");
    dataContext.AssemblyList.Add(new AssemblyListObject(name, elemId));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/cloneticket/assemblyselectionwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.DescriptionTextBlock = (TextBlock) target;
        break;
      case 2:
        this.SearchBox = (TextBox) target;
        this.SearchBox.TextChanged += new TextChangedEventHandler(this.SearchBox_TextChanged);
        break;
      case 3:
        this.WaterMarkLabel = (Label) target;
        break;
      case 4:
        this.CheckBoxList = (ListBox) target;
        break;
      case 6:
        this.ContinueButton = (Button) target;
        this.ContinueButton.Click += new RoutedEventHandler(this.ContinueButton_Click);
        break;
      case 7:
        this.CancelButton = (Button) target;
        this.CancelButton.Click += new RoutedEventHandler(this.CancelButton_Click);
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
    if (connectionId != 5)
      return;
    ((ToggleButton) target).Checked += new RoutedEventHandler(this.CheckBox_Checked);
  }
}
