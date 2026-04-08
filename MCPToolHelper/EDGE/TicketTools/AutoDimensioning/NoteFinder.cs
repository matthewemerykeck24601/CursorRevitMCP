// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.NoteFinder
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using Utils.GeometryUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class NoteFinder
{
  public double _noteWidth;
  public XYZ _noteMin;
  public XYZ _noteMax;
  public TextNote _currNote;
  public XYZ _principalAxis;
  public XYZ _perpendicularAxis;
  public XYZ _coord;
  public XYZ _midPoint;
  public double _MPProj;
  public double _vertProj;
  public double _minHProj;
  public double _maxHProj;
  public double _minWProj;
  public double _maxWProj;
  public double _height;
  public double _width;
  public HorizontalTextAlignment _textAlignment;

  public NoteFinder(TextNote note, XYZ principalAxis, XYZ perpendicularAxis, int viewScale)
  {
    this._currNote = note;
    this._principalAxis = principalAxis;
    this._perpendicularAxis = perpendicularAxis;
    this._height = note.Height * (double) viewScale;
    this._width = note.Width * (double) viewScale;
    this._textAlignment = note.HorizontalAlignment;
    this._coord = note.Coord;
    this._midPoint = this._coord - note.UpDirection * (note.Height / 2.0 * (double) viewScale);
    this._maxHProj = ProjectionUtils.ProjectPointOnDirection(this._currNote.Coord, this._principalAxis);
    this._minHProj = !this._principalAxis.IsAlmostEqualTo(this._currNote.UpDirection) ? this._maxHProj + this._height : this._maxHProj - this._height;
    this._MPProj = ProjectionUtils.ProjectPointOnDirection(this._midPoint, this._principalAxis);
    double num = ProjectionUtils.ProjectPointOnDirection(this._currNote.Coord, this._perpendicularAxis);
    bool flag = note.BaseDirection.IsAlmostEqualTo(this._perpendicularAxis);
    switch (this._textAlignment)
    {
      case HorizontalTextAlignment.Left:
        this._maxWProj = flag ? num + this._width : num - this._width;
        this._minWProj = num;
        break;
      case HorizontalTextAlignment.Right:
        this._minWProj = flag ? num - this._width : num + this._width;
        this._maxWProj = num;
        break;
      case HorizontalTextAlignment.Center:
        this._minWProj = flag ? num - this._width / 2.0 : num + this._width / 2.0;
        this._maxWProj = flag ? num + this._width / 2.0 : num - this._width / 2.0;
        break;
    }
  }

  public double GetVertical(double projection, bool signed = false)
  {
    return !signed ? Math.Abs(projection - this._MPProj) : projection - this._MPProj;
  }

  public bool DetectHorizontalOverlap(double minW, double maxW)
  {
    return minW <= this._minWProj && maxW >= this._maxWProj || minW >= this._minWProj && minW <= this._maxWProj || maxW >= this._minWProj && maxW <= this._maxWProj;
  }
}
