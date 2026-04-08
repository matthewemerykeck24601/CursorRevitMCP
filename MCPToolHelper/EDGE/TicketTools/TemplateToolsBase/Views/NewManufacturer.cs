// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.Views.NewManufacturer
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

public class NewManufacturer : Window, IComponentConnector
{
  internal Label newManufacturerNameLabel;
  internal TextBox manufacturerNameTextBox;
  internal Button addButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public NewManufacturer(
    ManufacturerManagementWindowViewModel managementViewModel)
  {
    this.InitializeComponent();
    this.DataContext = (object) new NewManufacturerViewModel(this, managementViewModel);
    this.manufacturerNameTextBox.Text = managementViewModel.ManufacturerString;
    this.manufacturerNameTextBox.Focus();
  }

  public NewManufacturer(TemplateCreatorViewModel templateCreatorViewModel)
  {
    this.InitializeComponent();
    this.DataContext = (object) new NewManufacturerViewModel(this, templateCreatorViewModel);
    this.manufacturerNameTextBox.Focus();
  }

  private void manufacturerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
  {
    (this.DataContext as NewManufacturerViewModel).AdditionString = this.manufacturerNameTextBox.Text;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/tickettools/templatetoolsbase/views/newmanufacturer.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.newManufacturerNameLabel = (Label) target;
        break;
      case 2:
        this.manufacturerNameTextBox = (TextBox) target;
        this.manufacturerNameTextBox.TextChanged += new TextChangedEventHandler(this.manufacturerNameTextBox_TextChanged);
        break;
      case 3:
        this.addButton = (Button) target;
        break;
      case 4:
        this.cancelButton = (Button) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
