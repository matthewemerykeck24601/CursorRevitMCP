// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.Views.BFUMainWindow
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
using System.Windows.Interop;
using System.Windows.Markup;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater.Views;

public class BFUMainWindow : Window, IComponentConnector, IStyleConnector
{
  private IList<FamilyParameter> familyParameters;
  private DefinitionGroups sharedParameters;
  private Document revitDoc;
  public bool cancelBool = true;
  internal Button addParameterButton;
  internal Button modifyParameterButton;
  internal Button deleteParameterButton;
  internal DataGrid DataGrid;
  internal CheckBox addEdgeDimLinesCheckBox;
  internal Button OKButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public BFUMainWindow()
  {
    this.InitializeComponent();
    this.DataContext = (object) new BFUMainViewModel();
  }

  public BFUMainWindow(
    IList<FamilyParameter> familyParameters,
    DefinitionGroups sharedParameters,
    IntPtr parentWindowHandler,
    Document doc)
  {
    this.InitializeComponent();
    this.DataContext = (object) new BFUMainViewModel();
    this.revitDoc = doc;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.familyParameters = familyParameters;
    this.sharedParameters = sharedParameters;
  }

  private void addParameterButton_Click(object sender, RoutedEventArgs e)
  {
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    BFUParameterTaskWindow parameterTaskWindow = new BFUParameterTaskWindow(BulkFamilyUpdatersUtils.taskActions.Add, Process.GetCurrentProcess().MainWindowHandle, this.revitDoc, this.getCurrentParametersUsed(), this.familyParameters, this.sharedParameters);
    parameterTaskWindow.ShowDialog();
    FamilyUpdaterTask updaterTask = parameterTaskWindow.getUpdaterTask();
    if (parameterTaskWindow.cancelWindow || updaterTask == null)
      return;
    dataContext.Tasks.Add(updaterTask);
  }

  private void modifyParameterButton_Click(object sender, RoutedEventArgs e)
  {
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    BFUParameterTaskWindow parameterTaskWindow = new BFUParameterTaskWindow(BulkFamilyUpdatersUtils.taskActions.Modify, Process.GetCurrentProcess().MainWindowHandle, this.revitDoc, this.getCurrentParametersUsed(), this.familyParameters, this.sharedParameters);
    parameterTaskWindow.ShowDialog();
    FamilyUpdaterTask updaterTask = parameterTaskWindow.getUpdaterTask();
    if (parameterTaskWindow.cancelWindow || updaterTask == null)
      return;
    dataContext.Tasks.Add(updaterTask);
  }

  private void deleteParameterButton_Click(object sender, RoutedEventArgs e)
  {
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    List<FamilyParameter> familyParameters = new List<FamilyParameter>();
    if (this.familyParameters != null)
    {
      foreach (FamilyParameter familyParameter in (IEnumerable<FamilyParameter>) this.familyParameters)
      {
        if (((InternalDefinition) familyParameter.Definition).BuiltInParameter == BuiltInParameter.INVALID)
          familyParameters.Add(familyParameter);
      }
    }
    BFUParameterTaskWindow parameterTaskWindow = new BFUParameterTaskWindow(BulkFamilyUpdatersUtils.taskActions.Delete, Process.GetCurrentProcess().MainWindowHandle, this.revitDoc, this.getCurrentParametersUsed(), (IList<FamilyParameter>) familyParameters, this.sharedParameters);
    parameterTaskWindow.ShowDialog();
    FamilyUpdaterTask updaterTask = parameterTaskWindow.getUpdaterTask();
    if (parameterTaskWindow.cancelWindow || updaterTask == null)
      return;
    dataContext.Tasks.Add(updaterTask);
  }

  private void OKButton_Click(object sender, RoutedEventArgs e)
  {
    this.cancelBool = false;
    this.Close();
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void editTaskButton_Click(object sender, RoutedEventArgs e)
  {
    IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    BFUParameterTaskWindow parameterTaskWindow = new BFUParameterTaskWindow(dataContext.SelectedTask, mainWindowHandle, this.revitDoc, this.getCurrentParametersUsed(), this.familyParameters);
    parameterTaskWindow.ShowDialog();
    int index = dataContext.Tasks.IndexOf(dataContext.SelectedTask);
    FamilyUpdaterTask updaterTask = parameterTaskWindow.getUpdaterTask();
    dataContext.Tasks[index] = updaterTask;
    this.DataGrid.Items.Refresh();
  }

  private void removeTaskButton_Click(object sender, RoutedEventArgs e)
  {
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    dataContext.Tasks.Remove(dataContext.SelectedTask);
  }

  private List<string> getCurrentParametersUsed()
  {
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    List<string> currentParametersUsed = new List<string>();
    foreach (FamilyUpdaterTask task in (Collection<FamilyUpdaterTask>) dataContext.Tasks)
    {
      currentParametersUsed.Add(task.parameterName.Trim());
      if (task.action == BulkFamilyUpdatersUtils.taskActions.Modify && !currentParametersUsed.Contains(task.oldParameterName.Trim()))
        currentParametersUsed.Add(task.oldParameterName.Trim());
    }
    return currentParametersUsed;
  }

  public TaskManager getTaskManager()
  {
    BFUMainViewModel dataContext = this.DataContext as BFUMainViewModel;
    TaskManager taskManager = new TaskManager();
    taskManager.addTasksToLists(dataContext.Tasks.ToList<FamilyUpdaterTask>());
    if (dataContext.EDGEDimLines.GetValueOrDefault())
      taskManager.runDimLinesBool = true;
    return taskManager;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/__testing/bulkfamilyupdater/views/bfumainwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.addParameterButton = (Button) target;
        this.addParameterButton.Click += new RoutedEventHandler(this.addParameterButton_Click);
        break;
      case 2:
        this.modifyParameterButton = (Button) target;
        this.modifyParameterButton.Click += new RoutedEventHandler(this.modifyParameterButton_Click);
        break;
      case 3:
        this.deleteParameterButton = (Button) target;
        this.deleteParameterButton.Click += new RoutedEventHandler(this.deleteParameterButton_Click);
        break;
      case 4:
        this.DataGrid = (DataGrid) target;
        break;
      case 7:
        this.addEdgeDimLinesCheckBox = (CheckBox) target;
        break;
      case 8:
        this.OKButton = (Button) target;
        this.OKButton.Click += new RoutedEventHandler(this.OKButton_Click);
        break;
      case 9:
        this.cancelButton = (Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
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
    {
      if (connectionId != 6)
        return;
      ((ButtonBase) target).Click += new RoutedEventHandler(this.removeTaskButton_Click);
    }
    else
      ((ButtonBase) target).Click += new RoutedEventHandler(this.editTaskButton_Click);
  }
}
