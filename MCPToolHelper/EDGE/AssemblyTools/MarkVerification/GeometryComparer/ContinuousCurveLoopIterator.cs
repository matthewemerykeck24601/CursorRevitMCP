// Decompiled with JetBrains decompiler
// Type: EDGE.AssemblyTools.MarkVerification.GeometryComparer.ContinuousCurveLoopIterator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AssemblyTools.MarkVerification.GeometryComparer;

internal class ContinuousCurveLoopIterator
{
  private List<Curve> _curves;
  private int _pointer;
  private bool _bReversed;

  public int Length { get; private set; }

  public int Counter { get; private set; }

  public ContinuousCurveLoopIterator(CurveLoop loop, bool reversed = false)
  {
    this._bReversed = reversed;
    this._curves = new List<Curve>();
    foreach (Curve curve in loop)
    {
      this._curves.Add(curve);
      ++this.Length;
    }
  }

  public Curve Current
  {
    get
    {
      return this._pointer > -1 && this._pointer < this.Length ? this._curves[this._pointer] : (Curve) null;
    }
  }

  public bool MoveNext()
  {
    this._pointer += this._bReversed ? -1 : 1;
    if (!this._bReversed && this._pointer >= this.Length)
      this._pointer = 0;
    else if (this._bReversed && this._pointer < 0)
      this._pointer = this.Length - 1;
    ++this.Counter;
    if (this.Counter >= this.Length)
      this.Counter = 0;
    return true;
  }

  public void Reset() => this._pointer = this._bReversed ? this.Length : -1;

  public void ResetCounter() => this.Counter = 0;
}
