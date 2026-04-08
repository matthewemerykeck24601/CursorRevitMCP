// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.WBHelper
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

public class WBHelper
{
  public static List<WBHelper.RunningObject> GetRunningWorkBooks()
  {
    IBindCtx b;
    WBHelper.CreateBindCtx(0, out b);
    IRunningObjectTable pprot;
    b.GetRunningObjectTable(out pprot);
    IEnumMoniker ppenumMoniker;
    pprot.EnumRunning(out ppenumMoniker);
    List<WBHelper.RunningObject> runningWorkBooks = new List<WBHelper.RunningObject>();
    ppenumMoniker.Reset();
    IMoniker[] rgelt = new IMoniker[1];
    IntPtr zero = IntPtr.Zero;
    while (ppenumMoniker.Next(1, rgelt, zero) == 0)
    {
      WBHelper.RunningObject runningObject;
      rgelt[0].GetDisplayName(b, (IMoniker) null, out runningObject.path);
      pprot.GetObject(rgelt[0], out runningObject.obj);
      runningWorkBooks.Add(runningObject);
    }
    return runningWorkBooks;
  }

  [DllImport("ole32.dll")]
  private static extern void CreateBindCtx(int a, out IBindCtx b);

  public struct RunningObject
  {
    public string path;
    public object obj;
  }
}
