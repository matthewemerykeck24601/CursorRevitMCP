// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.SpreadSheetData
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning;

public class SpreadSheetData : SSNotifier
{
  private bool? _TopBottom;
  private bool? _FrontBack;
  private bool? _LeftRight;
  private bool? _CalloutAlways;
  private bool? _CalloutDim;
  private bool _bCallOutReadOnly;
  private bool _bAlwaysCalloutReadOnly;

  public AssemblyInstance Assembly { get; set; }

  public string AssemblyName { get; set; }

  public string Name { get; set; }

  public Element Element { get; set; }

  public bool? DimTopBottom
  {
    get => this._TopBottom;
    set
    {
      this._TopBottom = value;
      this.OnPropertyChanged(nameof (DimTopBottom));
    }
  }

  public bool? DimFrontBack
  {
    get => this._FrontBack;
    set
    {
      this._FrontBack = value;
      this.OnPropertyChanged(nameof (DimFrontBack));
    }
  }

  public bool? DimLeftRight
  {
    get => this._LeftRight;
    set
    {
      this._LeftRight = value;
      this.OnPropertyChanged(nameof (DimLeftRight));
    }
  }

  public bool? CalloutAlways
  {
    get => this._CalloutAlways;
    set
    {
      this._CalloutAlways = this.AlwaysCalloutReadOnly ? new bool?(false) : value;
      this.OnPropertyChanged(nameof (CalloutAlways));
    }
  }

  public bool? CalloutDim
  {
    get => this._CalloutDim;
    set
    {
      this._CalloutDim = this.CalloutReadOnly ? new bool?(false) : value;
      this.OnPropertyChanged(nameof (CalloutDim));
    }
  }

  public bool bTopBottomDiscrepency { get; set; }

  public bool bFrontBackDiscrepency { get; set; }

  public bool bLeftRightDiscrepency { get; set; }

  public bool bCalloutAlwaysDiscrepency { get; set; }

  public bool bCalloutDimDiscrepency { get; set; }

  public bool CalloutReadOnly
  {
    get => this._bCallOutReadOnly;
    set
    {
      this._bCallOutReadOnly = value;
      this.OnPropertyChanged(nameof (CalloutReadOnly));
    }
  }

  public bool AlwaysCalloutReadOnly
  {
    get => this._bAlwaysCalloutReadOnly;
    set
    {
      this._bAlwaysCalloutReadOnly = value;
      this.OnPropertyChanged(nameof (AlwaysCalloutReadOnly));
    }
  }

  public static Dictionary<AssemblyInstance, List<SpreadSheetData>> assignType(
    List<SpreadSheetData> sheetData,
    Document revitDoc)
  {
    Dictionary<AssemblyInstance, List<SpreadSheetData>> dictionary = new Dictionary<AssemblyInstance, List<SpreadSheetData>>();
    foreach (IGrouping<AssemblyInstance, SpreadSheetData> source1 in sheetData.GroupBy<SpreadSheetData, AssemblyInstance>((Func<SpreadSheetData, AssemblyInstance>) (e => e.Assembly)))
    {
      foreach (IGrouping<string, SpreadSheetData> source2 in source1.GroupBy<SpreadSheetData, string>((Func<SpreadSheetData, string>) (e => e.Name)))
      {
        bool? tb = new bool?();
        bool tbMixed = false;
        if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.DimTopBottom.GetValueOrDefault())))
          tb = new bool?(true);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
        {
          bool? dimTopBottom = e.DimTopBottom;
          bool flag = false;
          return dimTopBottom.GetValueOrDefault() == flag & dimTopBottom.HasValue;
        })))
          tb = new bool?(false);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => !e.DimTopBottom.HasValue)))
        {
          tb = new bool?();
        }
        else
        {
          if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.DimTopBottom.GetValueOrDefault())))
            tb = new bool?(true);
          else if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
          {
            bool? dimTopBottom = e.DimTopBottom;
            bool flag = false;
            return dimTopBottom.GetValueOrDefault() == flag & dimTopBottom.HasValue;
          })))
            tb = new bool?(false);
          tbMixed = true;
        }
        bool? fb = new bool?();
        bool fbMixed = false;
        if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.DimFrontBack.GetValueOrDefault())))
          fb = new bool?(true);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
        {
          bool? dimFrontBack = e.DimFrontBack;
          bool flag = false;
          return dimFrontBack.GetValueOrDefault() == flag & dimFrontBack.HasValue;
        })))
          fb = new bool?(false);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => !e.DimFrontBack.HasValue)))
        {
          fb = new bool?();
        }
        else
        {
          if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.DimFrontBack.GetValueOrDefault())))
            fb = new bool?(true);
          else if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
          {
            bool? dimFrontBack = e.DimFrontBack;
            bool flag = false;
            return dimFrontBack.GetValueOrDefault() == flag & dimFrontBack.HasValue;
          })))
            fb = new bool?(false);
          fbMixed = true;
        }
        bool? lr = new bool?();
        bool lrMixed = false;
        if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.DimLeftRight.GetValueOrDefault())))
          lr = new bool?(true);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
        {
          bool? dimLeftRight = e.DimLeftRight;
          bool flag = false;
          return dimLeftRight.GetValueOrDefault() == flag & dimLeftRight.HasValue;
        })))
          lr = new bool?(false);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => !e.DimLeftRight.HasValue)))
        {
          lr = new bool?();
        }
        else
        {
          if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.DimLeftRight.GetValueOrDefault())))
            lr = new bool?(true);
          else if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
          {
            bool? dimLeftRight = e.DimLeftRight;
            bool flag = false;
            return dimLeftRight.GetValueOrDefault() == flag & dimLeftRight.HasValue;
          })))
            lr = new bool?(false);
          lrMixed = true;
        }
        bool? calloutAll = new bool?();
        bool caMixed = false;
        if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.CalloutAlways.GetValueOrDefault())))
          calloutAll = new bool?(true);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
        {
          bool? calloutAlways = e.CalloutAlways;
          bool flag = false;
          return calloutAlways.GetValueOrDefault() == flag & calloutAlways.HasValue;
        })))
          calloutAll = new bool?(false);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => !e.CalloutAlways.HasValue)))
        {
          calloutAll = new bool?();
        }
        else
        {
          if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.CalloutAlways.GetValueOrDefault())))
            calloutAll = new bool?(true);
          else if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
          {
            bool? calloutAlways = e.CalloutAlways;
            bool flag = false;
            return calloutAlways.GetValueOrDefault() == flag & calloutAlways.HasValue;
          })))
            calloutAll = new bool?(false);
          caMixed = true;
        }
        bool? calloutdim = new bool?();
        bool cdMixed = false;
        if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.CalloutDim.GetValueOrDefault())))
          calloutdim = new bool?(true);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
        {
          bool? calloutDim = e.CalloutDim;
          bool flag = false;
          return calloutDim.GetValueOrDefault() == flag & calloutDim.HasValue;
        })))
          calloutdim = new bool?(false);
        else if (source2.All<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => !e.CalloutDim.HasValue)))
        {
          calloutdim = new bool?();
        }
        else
        {
          if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e => e.CalloutDim.GetValueOrDefault())))
            calloutdim = new bool?(true);
          else if (source2.Any<SpreadSheetData>((Func<SpreadSheetData, bool>) (e =>
          {
            bool? calloutDim = e.CalloutDim;
            bool flag = false;
            return calloutDim.GetValueOrDefault() == flag & calloutDim.HasValue;
          })))
            calloutdim = new bool?(false);
          cdMixed = true;
        }
        bool aCRO = false;
        bool cRO = false;
        foreach (SpreadSheetData spreadSheetData in sheetData)
        {
          if (spreadSheetData.Name.ToUpper().Equals(source2.Key.ToUpper()) && Oracle.IsAddonFamily(spreadSheetData.Element))
          {
            calloutAll = new bool?(false);
            calloutdim = new bool?(false);
            aCRO = true;
            cRO = true;
            break;
          }
        }
        SpreadSheetData spreadSheetData1 = new SpreadSheetData(source1.Key, source2.Key, tb, fb, lr, calloutAll, calloutdim, tbMixed, fbMixed, lrMixed, caMixed, cdMixed, aCRO, cRO);
        if (dictionary.ContainsKey(source1.Key))
          dictionary[source1.Key].Add(spreadSheetData1);
        else
          dictionary.Add(source1.Key, new List<SpreadSheetData>()
          {
            spreadSheetData1
          });
      }
    }
    return dictionary;
  }

  public SpreadSheetData(
    AssemblyInstance assInst,
    Element elem,
    bool? tb,
    bool? fb,
    bool? lr,
    bool? calloutAll,
    bool? calloutdim)
  {
    this.Assembly = assInst;
    this.AssemblyName = assInst.Name;
    Parameter parameter = Parameters.LookupParameter(elem, "CONTROL_MARK");
    this.Name = parameter == null || string.IsNullOrEmpty(parameter.AsString()) ? (elem as FamilyInstance).Symbol.FamilyName : parameter.AsString();
    this.Element = elem;
    this.DimFrontBack = fb;
    this.DimTopBottom = tb;
    this.DimLeftRight = lr;
    this.CalloutAlways = calloutAll;
    this.CalloutDim = calloutdim;
  }

  public SpreadSheetData(
    AssemblyInstance assInst,
    string controlMark,
    bool? tb,
    bool? fb,
    bool? lr,
    bool? calloutAll,
    bool? calloutdim,
    bool tbMixed,
    bool fbMixed,
    bool lrMixed,
    bool caMixed,
    bool cdMixed,
    bool aCRO,
    bool cRO)
  {
    this.Assembly = assInst;
    this.AssemblyName = assInst.Name;
    this.Name = controlMark;
    this.DimFrontBack = fb;
    this.DimTopBottom = tb;
    this.DimLeftRight = lr;
    this.CalloutAlways = calloutAll;
    this.CalloutDim = calloutdim;
    this.bTopBottomDiscrepency = tbMixed;
    this.bFrontBackDiscrepency = fbMixed;
    this.bLeftRightDiscrepency = lrMixed;
    this.bCalloutAlwaysDiscrepency = caMixed;
    this.bCalloutDimDiscrepency = cdMixed;
    this.AlwaysCalloutReadOnly = aCRO;
    this.CalloutReadOnly = cRO;
  }
}
