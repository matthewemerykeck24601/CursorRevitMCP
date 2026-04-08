// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.Views.HWTemplateCreatorWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.HardwareDetail.ViewModels;
using EDGE.TicketTools.TemplateToolsBase;
using EDGE.TicketTools.TicketTemplateTools.Views;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail.Views;

public class HWTemplateCreatorWindow : Window, IComponentConnector
{
  public AssemblyInstance assemblyInstance;
  public View ActiveView;
  public Document RevitDocument;
  public UIDocument UiDoc;
  public UIApplication app;
  internal TextBox TemplateSettingsFile;
  internal Button FileSelection;
  internal Button manageManufacturers;
  internal Label manufacturerNameLabel;
  internal ComboBox manufacturerComboBox;
  internal Label tamplateNameSelectLabel;
  internal ComboBox templateNameComboBox;
  internal Button createButon;
  internal Button editTitleblockButon;
  internal Button renameButon;
  internal Button deleteButon;
  internal Button helpButton;
  internal Button saveButton;
  internal Button saveCloseButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  [DllImport("User32.dll")]
  private static extern bool SetForegroundWindow(IntPtr handle);

  public HWTemplateCreatorWindow() => this.InitializeComponent();

  public HWTemplateCreatorWindow(
    AssemblyInstance assembly,
    View activeViewTemp,
    Document revitDocuemnt,
    UIDocument uidoc,
    UIApplication uiApp,
    IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    this.DataContext = (object) new HWTemplateCreatorViewModel(this, assembly);
    App.hwTemplateCreatorWindow = this;
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    HWTemplateCreatorViewModel dataContext = this.DataContext as HWTemplateCreatorViewModel;
    dataContext.ActiveView = activeViewTemp;
    this.ActiveView = activeViewTemp;
    dataContext.assembly = assembly;
    this.assemblyInstance = assembly;
    dataContext.revitDoc = revitDocuemnt;
    this.RevitDocument = revitDocuemnt;
    dataContext.uiDoc = uidoc;
    this.UiDoc = uidoc;
    this.app = uiApp;
  }

  private void templateComboBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).UpdateTemplateString(this.templateNameComboBox.Text);
  }

  private void OnClosing(object sender, CancelEventArgs e)
  {
    EDGERCreateTemplate.windowExists = false;
    App.templateCreatorWindow = (TicketTemplateCreatorWindow) null;
    try
    {
      HWTemplateCreatorWindow.SetForegroundWindow(this.app.MainWindowHandle);
    }
    catch (Exception ex)
    {
    }
  }

  private void manageManufacturers_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteManageManufacturer();
  }

  private void OnPageLoaded(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).LoadForm();
  }

  public void setAssemblyInstance(AssemblyInstance assIn)
  {
    this.assemblyInstance = assIn;
    (this.DataContext as HWTemplateCreatorViewModel).assembly = this.assemblyInstance;
  }

  public void setActiveView(View av)
  {
    this.ActiveView = av;
    (this.DataContext as HWTemplateCreatorViewModel).ActiveView = this.ActiveView;
  }

  public void setRevitDoc(Document revitDocument)
  {
    this.RevitDocument = revitDocument;
    (this.DataContext as HWTemplateCreatorViewModel).revitDoc = this.RevitDocument;
  }

  public void setUiDoc(UIDocument uidoc)
  {
    this.UiDoc = uidoc;
    (this.DataContext as HWTemplateCreatorViewModel).uiDoc = this.UiDoc;
  }

  public void setUiApp(UIApplication uiApp) => this.app = uiApp;

  private void FileSelection_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteFileSelectionCommand();
  }

  private void createButon_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteCreate();
  }

  private void editTitleblockButon_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteEditTitleblockCommand();
  }

  private void renameButon_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteRenameTemplateCommand();
  }

  private void deleteButon_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteDeleteTemplateCommand();
  }

  private void helpButton_Click(object sender, RoutedEventArgs e)
  {
    Process.Start("https://www.edge.ptac.com/hardware-detail-template-creator");
  }

  private void saveButton_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteSaveCommand();
  }

  private void saveCloseButton_Click(object sender, RoutedEventArgs e)
  {
    (this.DataContext as HWTemplateCreatorViewModel).ExecuteSaveCommand();
    this.Close();
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/hardwaredetail/views/hwtemplatecreatorwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnPageLoaded);
        ((Window) target).Closing += new CancelEventHandler(this.OnClosing);
        break;
      case 2:
        this.TemplateSettingsFile = (TextBox) target;
        break;
      case 3:
        this.FileSelection = (Button) target;
        this.FileSelection.Click += new RoutedEventHandler(this.FileSelection_Click);
        break;
      case 4:
        this.manageManufacturers = (Button) target;
        this.manageManufacturers.Click += new RoutedEventHandler(this.manageManufacturers_Click);
        break;
      case 5:
        this.manufacturerNameLabel = (Label) target;
        break;
      case 6:
        this.manufacturerComboBox = (ComboBox) target;
        break;
      case 7:
        this.tamplateNameSelectLabel = (Label) target;
        break;
      case 8:
        this.templateNameComboBox = (ComboBox) target;
        this.templateNameComboBox.AddHandler(TextBoxBase.TextChangedEvent, (Delegate) new TextChangedEventHandler(this.templateComboBox_TextChanged));
        break;
      case 9:
        this.createButon = (Button) target;
        this.createButon.Click += new RoutedEventHandler(this.createButon_Click);
        break;
      case 10:
        this.editTitleblockButon = (Button) target;
        this.editTitleblockButon.Click += new RoutedEventHandler(this.editTitleblockButon_Click);
        break;
      case 11:
        this.renameButon = (Button) target;
        this.renameButon.Click += new RoutedEventHandler(this.renameButon_Click);
        break;
      case 12:
        this.deleteButon = (Button) target;
        this.deleteButon.Click += new RoutedEventHandler(this.deleteButon_Click);
        break;
      case 13:
        this.helpButton = (Button) target;
        this.helpButton.Click += new RoutedEventHandler(this.helpButton_Click);
        break;
      case 14:
        this.saveButton = (Button) target;
        this.saveButton.Click += new RoutedEventHandler(this.saveButton_Click);
        break;
      case 15:
        this.saveCloseButton = (Button) target;
        this.saveCloseButton.Click += new RoutedEventHandler(this.saveCloseButton_Click);
        break;
      case 16 /*0x10*/:
        this.cancelButton = (Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
