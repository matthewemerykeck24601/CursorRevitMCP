// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AutoDimensioning.AutoDimUtils.AutoDimforEDWGsManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.TicketTools.AutoDimensioning.AutoDimUtils;

internal class AutoDimforEDWGsManager
{
  public Dictionary<string, List<Element>> groupingDictionary = new Dictionary<string, List<Element>>();
  public List<Element> _preSelectedElements = new List<Element>();
  public List<ElementId> preSelectedElementIds = new List<ElementId>();
  public List<Element> listSF = new List<Element>();
  public List<Element> listVoid = new List<Element>();
  public List<Element> listAutoDim = new List<Element>();
  public FilteredElementCollector fec;
  public bool missingDimLines;
  public List<string> missingDimLinesElements = new List<string>();
  public Document _revitDoc;
  public int itemCt;

  public AutoDimforEDWGsManager(Document revitDoc, List<Element> preSelectedElements)
  {
    this._revitDoc = revitDoc;
    this._preSelectedElements = preSelectedElements;
    this.ParseAutoDimItems();
    this.ParseVoid();
    this.ParseSF();
  }

  public void ParseSF()
  {
    foreach (Element preSelectedElement in this._preSelectedElements)
    {
      if (preSelectedElement.Category.Id.Equals((object) new ElementId(BuiltInCategory.OST_StructuralFraming)) && !Parameters.GetParameterAsString(preSelectedElement, "CONSTRUCTION_PRODUCT").Equals(""))
      {
        if (!this.groupingDictionary.ContainsKey("SF"))
        {
          this.groupingDictionary.Add("SF", new List<Element>()
          {
            preSelectedElement
          });
          ++this.itemCt;
        }
        else
        {
          this.groupingDictionary["SF"].Add(preSelectedElement);
          ++this.itemCt;
        }
      }
    }
  }

  public void ParseVoid()
  {
    foreach (Element preSelectedElement in this._preSelectedElements)
    {
      if ((preSelectedElement.Category.Id.Equals((object) new ElementId(BuiltInCategory.OST_GenericModel)) || preSelectedElement.Category.Id.Equals((object) new ElementId(BuiltInCategory.OST_SpecialityEquipment))) && Parameters.GetParameterAsString(preSelectedElement, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID"))
      {
        if (!(preSelectedElement is FamilyInstance) || !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(preSelectedElement as FamilyInstance))
        {
          this.missingDimLines = true;
          string str = !(preSelectedElement is FamilyInstance) ? preSelectedElement.Name : (!(preSelectedElement as FamilyInstance).Name.Equals((preSelectedElement as FamilyInstance).Symbol.Family.Name) ? $"{(preSelectedElement as FamilyInstance).Symbol.Family.Name} - {(preSelectedElement as FamilyInstance).Name}" : (preSelectedElement as FamilyInstance).Name);
          this.missingDimLinesElements.Add($"{preSelectedElement.Id?.ToString()} - {str}");
          ++this.itemCt;
        }
        else if (!this.groupingDictionary.ContainsKey("VOID"))
        {
          this.groupingDictionary.Add("VOID", new List<Element>()
          {
            preSelectedElement
          });
          ++this.itemCt;
        }
        else
        {
          this.groupingDictionary["VOID"].Add(preSelectedElement);
          ++this.itemCt;
        }
      }
    }
  }

  public void ParseAutoDimItems()
  {
    List<ElementId> list = new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_StructConnections
    }.Select<BuiltInCategory, ElementId>((Func<BuiltInCategory, ElementId>) (id => new ElementId(id))).ToList<ElementId>();
    foreach (Element preSelectedElement in this._preSelectedElements)
    {
      if (list.Contains(preSelectedElement.Category.Id) && !Parameters.GetParameterAsString(preSelectedElement, "MANUFACTURE_COMPONENT").ToUpper().Contains("VOID") && !Parameters.GetParameterAsString(preSelectedElement, "MANUFACTURE_COMPONENT").ToUpper().Contains("RAW") && !Parameters.GetParameterAsString(preSelectedElement, "MANUFACTURE_COMPONENT").ToUpper().Contains("CONSUMABLE") && !(preSelectedElement as FamilyInstance).Symbol.FamilyName.Contains("CONNECTOR") && !(preSelectedElement as FamilyInstance).Symbol.FamilyName.Contains("COMPONENT"))
      {
        if (!(preSelectedElement is FamilyInstance) || !HiddenGeomReferenceCalculator.FamilyInstanceContainsEdgeDimLines(preSelectedElement as FamilyInstance))
        {
          this.missingDimLines = true;
          string str = !(preSelectedElement is FamilyInstance) ? preSelectedElement.Name : (!(preSelectedElement as FamilyInstance).Name.Equals((preSelectedElement as FamilyInstance).Symbol.Family.Name) ? $"{(preSelectedElement as FamilyInstance).Symbol.Family.Name} - {(preSelectedElement as FamilyInstance).Name}" : (preSelectedElement as FamilyInstance).Name);
          this.missingDimLinesElements.Add($"{preSelectedElement.Id?.ToString()} - {str}");
          ++this.itemCt;
        }
        else
        {
          string key = "";
          if (!Parameters.GetParameterAsString(preSelectedElement, "CONTROL_MARK").Equals(""))
            key = Parameters.GetParameterAsString(preSelectedElement, "CONTROL_MARK");
          else if ((preSelectedElement as FamilyInstance).Symbol.FamilyName != null && !(preSelectedElement as FamilyInstance).Symbol.FamilyName.Equals(""))
            key = (preSelectedElement as FamilyInstance).Symbol.FamilyName;
          if (!key.Equals(""))
          {
            if (!this.groupingDictionary.ContainsKey(key))
            {
              this.groupingDictionary.Add(key, new List<Element>()
              {
                preSelectedElement
              });
              ++this.itemCt;
            }
            else
            {
              this.groupingDictionary[key].Add(preSelectedElement);
              ++this.itemCt;
            }
          }
        }
      }
    }
  }
}
