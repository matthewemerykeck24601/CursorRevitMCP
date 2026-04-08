// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.ProgressReporter.ProgressMonitor
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

#nullable disable
namespace EDGE.__Testing.ProgressReporter;

public class ProgressMonitor : Window, IComponentConnector
{
  internal ProgressBar pProgress;
  internal TextBlock tbStatus;
  internal Button btnCancel;
  private bool _contentLoaded;

  public ProgressMonitor() => this.InitializeComponent();

  public bool ProcessCancelled { get; internal set; }

  internal void UpdateStatus(string message, int i)
  {
    this.pProgress.Value = (double) i;
    this.tbStatus.Text = message;
    this.DoEvents();
  }

  internal void JobCompleted() => this.Close();

  public void DoEvents()
  {
    DispatcherFrame frame = new DispatcherFrame();
    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (Delegate) new DispatcherOperationCallback(this.ExitFrame), (object) frame);
    Dispatcher.PushFrame(frame);
  }

  public object ExitFrame(object f)
  {
    ((DispatcherFrame) f).Continue = false;
    return (object) null;
  }

  private void btnCancel_Click(object sender, RoutedEventArgs e)
  {
    this.ProcessCancelled = true;
    this.btnCancel.IsEnabled = false;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/__testing/progressreporter/progressmonitor.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.pProgress = (ProgressBar) target;
        break;
      case 2:
        this.tbStatus = (TextBlock) target;
        break;
      case 3:
        this.btnCancel = (Button) target;
        this.btnCancel.Click += new RoutedEventHandler(this.btnCancel_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
