// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.ProgressReporter.LongRunningTask
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Threading;

#nullable disable
namespace EDGE.__Testing.ProgressReporter;

public class LongRunningTask
{
  private UIApplication revitApp;
  private Document revitDoc;

  public LongRunningTask(ExternalCommandData commandData)
  {
    this.revitApp = commandData.Application;
    this.revitDoc = this.revitApp.ActiveUIDocument.Document;
  }

  public void StartJob()
  {
    ProgressMonitor progressMonitor = new ProgressMonitor();
    progressMonitor.ContentRendered += new EventHandler(this.RevitCallWithProgressBar);
    progressMonitor.ShowDialog();
  }

  private void RevitCallWithProgressBar(object sender, EventArgs e)
  {
    ProgressMonitor progressMonitor = sender as ProgressMonitor;
    for (int i = 0; i < 100; ++i)
    {
      Thread.Sleep(100);
      progressMonitor.UpdateStatus($"Update parameter {i.ToString()}...", i);
      if (progressMonitor.ProcessCancelled)
        break;
    }
    progressMonitor.JobCompleted();
  }
}
