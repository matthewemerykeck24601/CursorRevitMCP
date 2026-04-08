// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.QATimer
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace EDGE.QATools;

public class QATimer : Stopwatch
{
  private static List<QATimer> StopWatches { get; set; } = new List<QATimer>();

  public string Name { get; set; }

  public QATimer(string name)
  {
    this.Name = name;
    QATimer.StopWatches.Add(this);
    this.Start();
  }

  public new void Stop() => base.Stop();

  public void Flush(string title = "")
  {
    QA.LogDemarcation(title);
    foreach (QATimer stopWatch in QATimer.StopWatches)
    {
      if (!stopWatch.IsRunning)
        QA.LogLine(stopWatch.ToString());
      else
        QA.LogLine(stopWatch.Name + " Still Running...");
    }
    QATimer.StopWatches.Clear();
  }

  public override string ToString()
  {
    TimeSpan elapsed = this.Elapsed;
    return $"{this.Name:60}- Elapsed Time-> {elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
  }
}
