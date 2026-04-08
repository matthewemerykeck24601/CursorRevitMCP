// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.ScalesManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

public class ScalesManager
{
  private static Dictionary<string, int> Scales = new Dictionary<string, int>()
  {
    {
      "12\" = 1'-0\"",
      1
    },
    {
      "6\" = 1'-0\"",
      2
    },
    {
      "3\" = 1'-0\"",
      4
    },
    {
      "1 1/2\" = 1'-0\"",
      8
    },
    {
      "1\" = 1'-0\"",
      12
    },
    {
      "3/4\" = 1'-0\"",
      16 /*0x10*/
    },
    {
      "1/2\" = 1'-0\"",
      24
    },
    {
      "3/8\" = 1'-0\"",
      32 /*0x20*/
    },
    {
      "1/4\" = 1'-0\"",
      48 /*0x30*/
    },
    {
      "3/16\" = 1'-0\"",
      64 /*0x40*/
    },
    {
      "1/8\" = 1'-0\"",
      96 /*0x60*/
    },
    {
      "1\" = 10'-0\"",
      120
    },
    {
      "3/32\" = 1'-0\"",
      128 /*0x80*/
    },
    {
      "1/16\" = 1'-0\"",
      192 /*0xC0*/
    },
    {
      "1\" = 20'-0\"",
      240 /*0xF0*/
    },
    {
      "3/64\" = 1'-0\"",
      256 /*0x0100*/
    },
    {
      "1\" = 30'-0\"",
      360
    },
    {
      "1/32\" = 1'-0\"",
      384
    },
    {
      "1\" = 40'-0\"",
      480
    },
    {
      "1\" = 50'-0\"",
      600
    },
    {
      "1\" = 60'-0\"",
      720
    },
    {
      "1/64\" = 1'-0\"",
      768 /*0x0300*/
    },
    {
      "1\" = 80'-0\"",
      960
    },
    {
      "1\" = 100'-0\"",
      1200
    },
    {
      "1\" = 160'-0\"",
      1920
    },
    {
      "1\" = 200'-0\"",
      2400
    },
    {
      "1\" = 300'-0\"",
      3600
    },
    {
      "1\" = 400'-0\"",
      4800
    }
  };
  private static Dictionary<string, int> ScalesMetric = new Dictionary<string, int>()
  {
    {
      "1:1",
      1
    },
    {
      "1:2",
      2
    },
    {
      "1:5",
      5
    },
    {
      "1:10",
      10
    },
    {
      "1:20",
      20
    },
    {
      "1:25",
      25
    },
    {
      "1:50",
      50
    },
    {
      "1:100",
      100
    },
    {
      "1:200",
      200
    },
    {
      "1:500",
      500
    },
    {
      "1:1000",
      1000
    },
    {
      "1:2000",
      2000
    },
    {
      "1:5000",
      5000
    }
  };

  public static int GetRatioForScale(string strScale, ScaleUnits unitType)
  {
    if (unitType == ScaleUnits.US)
    {
      int ratioForScale = 1;
      if (ScalesManager.Scales.TryGetValue(strScale, out ratioForScale))
        return ratioForScale;
      throw new Exception("GetRatioForScale() - Unable to parse scale: " + strScale);
    }
    int ratioForScale1 = 1;
    if (ScalesManager.ScalesMetric.TryGetValue(strScale, out ratioForScale1))
      return ratioForScale1;
    throw new Exception("GetRatioForScale() - Unable to parse scale: " + strScale);
  }

  public static bool GetScaleStringForScaleFactor(
    int scaleFactor,
    ScaleUnits unitType,
    out string stringRep)
  {
    if (unitType == ScaleUnits.US)
    {
      if (ScalesManager.Scales.ContainsValue(scaleFactor))
      {
        stringRep = ScalesManager.Scales.FirstOrDefault<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (x => x.Value == scaleFactor)).Key;
        return true;
      }
      stringRep = "1:" + scaleFactor.ToString();
      return false;
    }
    if (ScalesManager.ScalesMetric.ContainsValue(scaleFactor))
    {
      stringRep = ScalesManager.ScalesMetric.FirstOrDefault<KeyValuePair<string, int>>((Func<KeyValuePair<string, int>, bool>) (x => x.Value == scaleFactor)).Key;
      return true;
    }
    stringRep = "1:" + scaleFactor.ToString();
    return false;
  }

  public static ScaleUnits GetScaleUnitsForDocument(Document revitDoc)
  {
    ScaleUnits unitsForDocument = ScaleUnits.US;
    ForgeTypeId unitTypeId = revitDoc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
    if (unitTypeId == UnitTypeId.Meters || unitTypeId == UnitTypeId.Millimeters || unitTypeId == UnitTypeId.Centimeters || unitTypeId == UnitTypeId.Decimeters)
      unitsForDocument = ScaleUnits.Metric;
    return unitsForDocument;
  }

  public static List<string> GetScalesList(ScaleUnits unitType)
  {
    return unitType != ScaleUnits.US ? ScalesManager.ScalesMetric.Keys.ToList<string>() : ScalesManager.Scales.Keys.ToList<string>();
  }

  public static Dictionary<string, int> GetScalesDictionary(ScaleUnits unitType)
  {
    return unitType != ScaleUnits.US ? ScalesManager.ScalesMetric : ScalesManager.Scales;
  }
}
