// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ErectionSequenceTool.ErectionSequenceFilter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.SchedulingTools.ErectionSequenceTool;

public class ErectionSequenceFilter : ISelectionFilter
{
  private int zoneIndex;

  public ErectionSequenceFilter()
  {
  }

  public ErectionSequenceFilter(int Zone) => this.zoneIndex = Zone;

  public bool AllowElement(Element element)
  {
    if (element.Category == null || element.Category.Id.IntegerValue != -2001320)
      return false;
    if (element.HasSuperComponent())
    {
      Element superComponent = element.GetSuperComponent();
      while (superComponent.HasSuperComponent())
        superComponent = superComponent.GetSuperComponent();
      if (superComponent.Category.Id.IntegerValue == -2001320)
        element = superComponent;
    }
    int parameterAsInt = Parameters.GetParameterAsInt(element, "ERECTION_SEQUENCE_NUMBER");
    return (parameterAsInt <= 0 || parameterAsInt == this.zoneIndex) && Parameters.GetParameterAsInt(element, "ERECTION_SEQUENCE_NUMBER") < 1;
  }

  public bool AllowReference(Reference refer, XYZ point) => true;
}
