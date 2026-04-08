// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.WarningAnalyzer.Views.MainWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace EDGE.AdminTools.WarningAnalyzer.Views;

public class MainWindow : Window, IComponentConnector
{
  internal Button chooseFileButton;
  internal TextBox chooseFileBox;
  internal Button closeButton;
  internal Button analyzeButton;
  internal Border optionsBorder;
  internal StackPanel optionsPanel;
  private bool _contentLoaded;

  public MainWindow()
  {
    this.InitializeComponent();
    this.PreviewKeyDown += new KeyEventHandler(this.HandleEsc);
  }

  private void chooseFileBox_LostFocus(object sender, RoutedEventArgs e)
  {
    if (!string.IsNullOrWhiteSpace(this.chooseFileBox.Text))
      return;
    this.chooseFileBox.Text = "No file selected";
  }

  private void closeButton_Click(object sender, RoutedEventArgs e)
  {
    (Window.GetWindow((DependencyObject) this) as MainWindow).Close();
  }

  private void HandleEsc(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Escape)
      return;
    this.Close();
  }

  private void analyzeButton_Click(object sender, RoutedEventArgs e)
  {
  }

  private void chooseFileButton_Click(object sender, RoutedEventArgs e)
  {
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/admintools/warninganalyzer/views/mainwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.chooseFileButton = (Button) target;
        break;
      case 2:
        this.chooseFileBox = (TextBox) target;
        this.chooseFileBox.LostFocus += new RoutedEventHandler(this.chooseFileBox_LostFocus);
        break;
      case 3:
        this.closeButton = (Button) target;
        break;
      case 4:
        this.analyzeButton = (Button) target;
        this.analyzeButton.Click += new RoutedEventHandler(this.analyzeButton_Click);
        break;
      case 5:
        this.optionsBorder = (Border) target;
        break;
      case 6:
        this.optionsPanel = (StackPanel) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
