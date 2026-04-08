// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.Views.ManufacturerManagementWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.TemplateToolsBase.Views;

public class ManufacturerManagementWindow : Window, IComponentConnector
{
  internal Label manufacturerManagementTitle;
  internal ListBox manufacturerListBox;
  internal Button addButton;
  internal Button renameButton;
  internal Button deleteButton;
  internal Button closeButton;
  private bool _contentLoaded;

  public ManufacturerManagementWindow(TemplateCreatorViewModel mainWindowVM)
  {
    this.InitializeComponent();
    this.DataContext = (object) new ManufacturerManagementWindowViewModel(this, mainWindowVM);
  }

  private void manufacturerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/templatetoolsbase/views/manufacturermanagementwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.manufacturerManagementTitle = (Label) target;
        break;
      case 2:
        this.manufacturerListBox = (ListBox) target;
        this.manufacturerListBox.SelectionChanged += new SelectionChangedEventHandler(this.manufacturerListBox_SelectionChanged);
        break;
      case 3:
        this.addButton = (Button) target;
        break;
      case 4:
        this.renameButton = (Button) target;
        break;
      case 5:
        this.deleteButton = (Button) target;
        break;
      case 6:
        this.closeButton = (Button) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
