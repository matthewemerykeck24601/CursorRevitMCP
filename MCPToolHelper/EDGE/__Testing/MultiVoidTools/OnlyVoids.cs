// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.MultiVoidTools.OnlyVoids
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.__Testing.MultiVoidTools;

public class OnlyVoids : ISelectionFilter
{
  private readonly List<ElementId> _voidIds;

  public OnlyVoids(List<ElementId> voidIds) => this._voidIds = voidIds;

  public bool AllowElement(Element element)
  {
    return new List<ElementId>() { element.Id }.Concat<ElementId>(element.GetSubComponentIds()).Any<ElementId>((Func<ElementId, bool>) (id => this._voidIds.Contains(id)));
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
