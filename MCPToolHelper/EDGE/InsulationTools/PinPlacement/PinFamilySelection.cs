// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.PinPlacement.PinFamilySelection
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
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.InsulationTools.PinPlacement;

public class PinFamilySelection : System.Windows.Window, IComponentConnector
{
  private bool buttonClicked;
  private List<Family> allAvailable = new List<Family>();
  internal PinFamilySelection Window;
  internal TextBlock txtblock;
  internal ListView pins;
  internal Button okButton;
  internal Button Cancel;
  private bool _contentLoaded;

  public PinFamilySelection(
    FamilySymbol familysymbol,
    List<Element> values,
    IntPtr parentnWindowHandler,
    bool OnlyTypesShown)
  {
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((System.Windows.Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentnWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.InitializeComponent();
    List<Family> familyList = new List<Family>();
    string familyName = (values.First<Element>() as FamilySymbol).FamilyName;
    string str = familysymbol.FamilyName.Equals(familysymbol.Name) ? familysymbol.FamilyName : $"{familysymbol.FamilyName} - {familysymbol.Name}";
    this.txtblock.Text = !OnlyTypesShown ? "Please select the pin family type to be placed for the following insulation family type : " + str : $"Please select the pin family type {familyName} to be placed in the following insulation family type : {str}";
    this.allAvailable = familyList;
    foreach (Element element in values)
    {
      ListViewItem newItem = new ListViewItem();
      if (OnlyTypesShown)
      {
        newItem.DataContext = (object) element.Name;
        newItem.Content = (object) element.Name;
        this.pins.Items.Add((object) newItem);
      }
      else
      {
        newItem.DataContext = (object) $"{(element as FamilySymbol).FamilyName}:{element.Name}";
        newItem.Content = (object) $"{(element as FamilySymbol).FamilyName}:{element.Name}";
        this.pins.Items.Add((object) newItem);
      }
    }
  }

  private void Window_Closed(object sender, EventArgs e)
  {
    if (!this.buttonClicked)
      this.pins.SelectedItem = (object) null;
    this.Window.Close();
  }

  private void Pins_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (this.pins.SelectedItem == null)
      return;
    this.okButton.IsEnabled = true;
  }

  private void OkButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.pins.SelectedItem == null)
      return;
    this.buttonClicked = true;
    this.Window.Close();
  }

  private void Cancel_Click(object sender, RoutedEventArgs e)
  {
    this.pins.SelectedItem = (object) null;
    this.Close();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/insulationtools/pinplacement/pinfamilyselection.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.Window = (PinFamilySelection) target;
        this.Window.Closed += new EventHandler(this.Window_Closed);
        break;
      case 2:
        this.txtblock = (TextBlock) target;
        break;
      case 3:
        this.pins = (ListView) target;
        this.pins.SelectionChanged += new SelectionChangedEventHandler(this.Pins_SelectionChanged);
        break;
      case 4:
        this.okButton = (Button) target;
        this.okButton.Click += new RoutedEventHandler(this.OkButton_Click);
        break;
      case 5:
        this.Cancel = (Button) target;
        this.Cancel.Click += new RoutedEventHandler(this.Cancel_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
