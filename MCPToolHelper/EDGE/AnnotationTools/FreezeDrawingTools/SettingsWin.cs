// Decompiled with JetBrains decompiler
// Type: EDGE.AnnotationTools.FreezeDrawingTools.SettingsWin
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AnnotationTools.FreezeDrawingTools;

public class SettingsWin : Window, IComponentConnector
{
  public int lineScaleIDX;
  public bool bCoordShared;
  public int dwgUnitIDX;
  public int colorIDX;
  public DWGExportOptions dwgOptionsEX;
  public DWGImportOptions dwgOptionsIM;
  internal ComboBox LineScaleCmb;
  internal ComboBox CoordSysCmb;
  internal ComboBox DWGUnitsCmb;
  internal RadioButton BWRadio;
  internal RadioButton ColorsRadio;
  internal RadioButton InvColorsRadio;
  internal Button OKBtn;
  internal Button CancelBtn;
  private bool _contentLoaded;

  public SettingsWin(
    DWGExportOptions exOptions,
    DWGImportOptions impOptions,
    IntPtr parentWindowHandler)
  {
    this.InitializeComponent();
    WindowInteropHelper windowInteropHelper = new WindowInteropHelper((Window) this);
    windowInteropHelper.EnsureHandle();
    windowInteropHelper.Owner = parentWindowHandler;
    this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    this.dwgOptionsEX = exOptions;
    this.dwgOptionsIM = impOptions;
    this.LineScaleCmb.SelectedIndex = (int) exOptions.LineScaling;
    this.CoordSysCmb.SelectedIndex = exOptions.SharedCoords ? 1 : 0;
    this.DWGUnitsCmb.SelectedIndex = (int) exOptions.TargetUnit;
    this.BWRadio.IsChecked = new bool?(impOptions.ColorMode == ImportColorMode.BlackAndWhite);
    this.ColorsRadio.IsChecked = new bool?(impOptions.ColorMode == ImportColorMode.Preserved);
    this.InvColorsRadio.IsChecked = new bool?(impOptions.ColorMode == ImportColorMode.Inverted);
  }

  private void ParseUI()
  {
    this.lineScaleIDX = this.LineScaleCmb.SelectedIndex;
    this.bCoordShared = this.CoordSysCmb.SelectedIndex != 0;
    this.dwgUnitIDX = this.DWGUnitsCmb.SelectedIndex;
    if (this.BWRadio.IsChecked.GetValueOrDefault())
    {
      this.colorIDX = 2;
    }
    else
    {
      bool? isChecked = this.ColorsRadio.IsChecked;
      if (isChecked.GetValueOrDefault())
      {
        this.colorIDX = 0;
      }
      else
      {
        isChecked = this.InvColorsRadio.IsChecked;
        if (!isChecked.GetValueOrDefault())
          return;
        this.colorIDX = 1;
      }
    }
  }

  private void CancelBtn_Click(object sender, RoutedEventArgs e) => this.Close();

  private void OKBtn_Click(object sender, RoutedEventArgs e)
  {
    this.ParseUI();
    this.dwgOptionsEX.LineScaling = (LineScaling) this.lineScaleIDX;
    this.dwgOptionsEX.SharedCoords = this.bCoordShared;
    this.dwgOptionsEX.TargetUnit = (ExportUnit) this.dwgUnitIDX;
    this.dwgOptionsIM.Unit = (ImportUnit) this.ExpImpUnitConversion(this.dwgUnitIDX);
    this.dwgOptionsIM.ColorMode = (ImportColorMode) this.colorIDX;
    this.Close();
  }

  private int ExpImpUnitConversion(int idx)
  {
    switch (idx)
    {
      case 1:
        return 2;
      case 2:
        return 1;
      case 3:
        return 6;
      case 4:
        return 5;
      case 5:
        return 3;
      default:
        return 0;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/annotationtools/freezedrawingtools/settingswin.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.LineScaleCmb = (ComboBox) target;
        break;
      case 2:
        this.CoordSysCmb = (ComboBox) target;
        break;
      case 3:
        this.DWGUnitsCmb = (ComboBox) target;
        break;
      case 4:
        this.BWRadio = (RadioButton) target;
        break;
      case 5:
        this.ColorsRadio = (RadioButton) target;
        break;
      case 6:
        this.InvColorsRadio = (RadioButton) target;
        break;
      case 7:
        this.OKBtn = (Button) target;
        this.OKBtn.Click += new RoutedEventHandler(this.OKBtn_Click);
        break;
      case 8:
        this.CancelBtn = (Button) target;
        this.CancelBtn.Click += new RoutedEventHandler(this.CancelBtn_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
