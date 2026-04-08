// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.CompareEngine_Selection_Window
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification;

public class CompareEngine_Selection_Window : Window, IComponentConnector
{
  public bool isContinue;
  private bool _traditionalApproach;
  private bool _originalStateIsTraditional;
  private bool _addonLocation;
  private bool _embededLocation;
  private bool _solid;
  private bool _familyType = true;
  private bool _familyParameter;
  private bool _mainMaterialVolumn;
  private bool _addonFamily;
  private bool _plateFamily;
  internal TextBlock textBlock;
  internal CheckBox CheckAllBox;
  internal CheckBox familyParametercheckBox;
  internal CheckBox mainMaterialVolumncheckBox;
  internal CheckBox addonFamilycheckBox;
  internal CheckBox plateFamilycheckBox;
  internal CheckBox memberGeometry_AddonLocation_checkBox;
  internal CheckBox memberGeometry_EmbededLocation_checkBox;
  internal CheckBox memberGeometry_Solid_checkBox;
  internal TextBlock detailTitle;
  internal RadioButton DetailBox;
  internal RadioButton traditionalBox;
  internal Button okButton;
  internal Button cancelButton;
  private bool _contentLoaded;

  public CompareEngine_Selection_Window(IntPtr parentWindowHandler, string type)
  {
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.InitializeComponent();
    if (type.Equals("existing"))
    {
      this.textBlock.Text = "Please select all of the comparisons that you would like Mark Verification Existing to process:";
      this.detailTitle.Visibility = Visibility.Visible;
      this.DetailBox.Visibility = Visibility.Visible;
      this.traditionalBox.Visibility = Visibility.Visible;
    }
    else
    {
      this.textBlock.Text = "Please select all of the comparisons that you would like Mark Verification Initial to process:";
      this.detailTitle.Visibility = Visibility.Collapsed;
      this.DetailBox.Visibility = Visibility.Collapsed;
      this.traditionalBox.Visibility = Visibility.Collapsed;
    }
  }

  private void cancelButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void okButton_Click(object sender, RoutedEventArgs e)
  {
    bool[] flagArray = new bool[9]
    {
      this._familyType,
      this._familyParameter,
      this._mainMaterialVolumn,
      this._addonFamily,
      this._plateFamily,
      this._addonLocation,
      this._embededLocation,
      this._solid,
      this._traditionalApproach
    };
    this.isContinue = true;
    this.Close();
  }

  private void memberGeometry_AddonLocation_checkBox_Checked(object sender, RoutedEventArgs e)
  {
    this._addonLocation = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void memberGeometry_AddonLocation_checkBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._addonLocation = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void memberGeometry_EmbededLocation_checkBox_Checked(object sender, RoutedEventArgs e)
  {
    this._embededLocation = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void memberGeometry_EmbededLocation_checkBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._embededLocation = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void memberGeometry_Solid_checkBox_Checked(object sender, RoutedEventArgs e)
  {
    this._solid = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void memberGeometry_Solid_checkBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._solid = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void familyParametercheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this._familyParameter = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void familyParametercheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._familyParameter = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void mainMaterialVolumncheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this._mainMaterialVolumn = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void mainMaterialVolumncheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._mainMaterialVolumn = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void addonFamilycheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this._addonFamily = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void addonFamilycheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._addonFamily = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void plateFamilycheckBox_Checked(object sender, RoutedEventArgs e)
  {
    this._plateFamily = true;
    if (this._addonLocation && this._embededLocation && this._solid && this._familyParameter && this._mainMaterialVolumn && this._addonFamily && this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(true);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void plateFamilycheckBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this._plateFamily = false;
    if (!this._addonLocation && !this._embededLocation && !this._solid && !this._familyParameter && !this._mainMaterialVolumn && !this._addonFamily && !this._plateFamily)
      this.CheckAllBox.IsChecked = new bool?(false);
    else
      this.CheckAllBox.IsChecked = new bool?();
  }

  private void CheckAllBox_Checked(object sender, RoutedEventArgs e)
  {
    this.memberGeometry_AddonLocation_checkBox.IsChecked = new bool?(true);
    this.memberGeometry_EmbededLocation_checkBox.IsChecked = new bool?(true);
    this.memberGeometry_Solid_checkBox.IsChecked = new bool?(true);
    this.familyParametercheckBox.IsChecked = new bool?(true);
    this.mainMaterialVolumncheckBox.IsChecked = new bool?(true);
    this.addonFamilycheckBox.IsChecked = new bool?(true);
    this.plateFamilycheckBox.IsChecked = new bool?(true);
  }

  private void CheckAllBox_Unchecked(object sender, RoutedEventArgs e)
  {
    this.memberGeometry_AddonLocation_checkBox.IsChecked = new bool?(false);
    this.memberGeometry_EmbededLocation_checkBox.IsChecked = new bool?(false);
    this.memberGeometry_Solid_checkBox.IsChecked = new bool?(false);
    this.familyParametercheckBox.IsChecked = new bool?(false);
    this.mainMaterialVolumncheckBox.IsChecked = new bool?(false);
    this.addonFamilycheckBox.IsChecked = new bool?(false);
    this.plateFamilycheckBox.IsChecked = new bool?(false);
  }

  private void RadioButton_Checked(object sender, RoutedEventArgs e)
  {
    if ((sender as RadioButton).Content.Equals((object) "Traditional Comparison"))
    {
      this._traditionalApproach = true;
      this._originalStateIsTraditional = false;
    }
    if (!(sender as RadioButton).IsChecked.Equals((object) true))
      return;
    this._originalStateIsTraditional = false;
    this._traditionalApproach = false;
  }

  private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
  {
    if ((sender as RadioButton).Content.Equals((object) "Traditional Comparison"))
    {
      this._originalStateIsTraditional = false;
      this._traditionalApproach = false;
    }
    if (!(sender as RadioButton).IsChecked.Equals((object) false))
      return;
    this._originalStateIsTraditional = true;
    this._traditionalApproach = true;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/assemblytools/markverification/compareengine_selection_window.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.textBlock = (TextBlock) target;
        break;
      case 2:
        this.CheckAllBox = (CheckBox) target;
        this.CheckAllBox.Checked += new RoutedEventHandler(this.CheckAllBox_Checked);
        this.CheckAllBox.Unchecked += new RoutedEventHandler(this.CheckAllBox_Unchecked);
        break;
      case 3:
        this.familyParametercheckBox = (CheckBox) target;
        this.familyParametercheckBox.Checked += new RoutedEventHandler(this.familyParametercheckBox_Checked);
        this.familyParametercheckBox.Unchecked += new RoutedEventHandler(this.familyParametercheckBox_Unchecked);
        break;
      case 4:
        this.mainMaterialVolumncheckBox = (CheckBox) target;
        this.mainMaterialVolumncheckBox.Checked += new RoutedEventHandler(this.mainMaterialVolumncheckBox_Checked);
        this.mainMaterialVolumncheckBox.Unchecked += new RoutedEventHandler(this.mainMaterialVolumncheckBox_Unchecked);
        break;
      case 5:
        this.addonFamilycheckBox = (CheckBox) target;
        this.addonFamilycheckBox.Checked += new RoutedEventHandler(this.addonFamilycheckBox_Checked);
        this.addonFamilycheckBox.Unchecked += new RoutedEventHandler(this.addonFamilycheckBox_Unchecked);
        break;
      case 6:
        this.plateFamilycheckBox = (CheckBox) target;
        this.plateFamilycheckBox.Checked += new RoutedEventHandler(this.plateFamilycheckBox_Checked);
        this.plateFamilycheckBox.Unchecked += new RoutedEventHandler(this.plateFamilycheckBox_Unchecked);
        break;
      case 7:
        this.memberGeometry_AddonLocation_checkBox = (CheckBox) target;
        this.memberGeometry_AddonLocation_checkBox.Checked += new RoutedEventHandler(this.memberGeometry_AddonLocation_checkBox_Checked);
        this.memberGeometry_AddonLocation_checkBox.Unchecked += new RoutedEventHandler(this.memberGeometry_AddonLocation_checkBox_Unchecked);
        break;
      case 8:
        this.memberGeometry_EmbededLocation_checkBox = (CheckBox) target;
        this.memberGeometry_EmbededLocation_checkBox.Checked += new RoutedEventHandler(this.memberGeometry_EmbededLocation_checkBox_Checked);
        this.memberGeometry_EmbededLocation_checkBox.Unchecked += new RoutedEventHandler(this.memberGeometry_EmbededLocation_checkBox_Unchecked);
        break;
      case 9:
        this.memberGeometry_Solid_checkBox = (CheckBox) target;
        this.memberGeometry_Solid_checkBox.Checked += new RoutedEventHandler(this.memberGeometry_Solid_checkBox_Checked);
        this.memberGeometry_Solid_checkBox.Unchecked += new RoutedEventHandler(this.memberGeometry_Solid_checkBox_Unchecked);
        break;
      case 10:
        this.detailTitle = (TextBlock) target;
        break;
      case 11:
        this.DetailBox = (RadioButton) target;
        this.DetailBox.Checked += new RoutedEventHandler(this.RadioButton_Checked);
        this.DetailBox.Unchecked += new RoutedEventHandler(this.RadioButton_Unchecked);
        break;
      case 12:
        this.traditionalBox = (RadioButton) target;
        break;
      case 13:
        this.okButton = (Button) target;
        this.okButton.Click += new RoutedEventHandler(this.okButton_Click);
        break;
      case 14:
        this.cancelButton = (Button) target;
        this.cancelButton.Click += new RoutedEventHandler(this.cancelButton_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
