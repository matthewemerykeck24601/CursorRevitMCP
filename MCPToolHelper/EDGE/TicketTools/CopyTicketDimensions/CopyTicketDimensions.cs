// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketDimensions.CopyTicketDimensions
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.CopyTicketDimensions;

internal class CopyTicketDimensions
{
  public static void copyTicketDimensions(Element viewSheetCopyFrom, Element viewSheetCopyTo)
  {
    try
    {
      Document document = viewSheetCopyFrom.Document;
      View getFrom = viewSheetCopyFrom as View;
      View view = viewSheetCopyTo as View;
      IEnumerable<Dimension> source = new FilteredElementCollector(document).OfClass(typeof (Dimension)).ToElements().Cast<Dimension>().Where<Dimension>((Func<Dimension, bool>) (dim => dim.View != null && dim.View.Id == getFrom.Id));
      source.Count<Dimension>();
      List<Dimension> dimensionList = new List<Dimension>();
      foreach (Element element in source)
      {
        if (element is Dimension)
        {
          Dimension dimension = element as Dimension;
          dimensionList.Add(dimension);
          DimensionType dimensionType = dimension.DimensionType;
          Curve curve = dimension.Curve;
          if (!((GeometryObject) curve == (GeometryObject) null) && !curve.IsCyclic)
          {
            Line unbound = Line.CreateUnbound(dimension.NumberOfSegments <= 1 ? dimension.Origin : dimension.Segments.get_Item(0).Origin, curve.ComputeDerivatives(0.5, false).BasisX);
            document.Create.NewDimension(view, unbound, dimension.References, dimension.DimensionType);
          }
        }
      }
    }
    catch (Exception ex)
    {
    }
  }
}
