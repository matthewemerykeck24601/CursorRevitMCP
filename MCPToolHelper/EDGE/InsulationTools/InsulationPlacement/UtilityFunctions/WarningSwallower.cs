// Decompiled with JetBrains decompiler
// Type: EDGE.InsulationTools.InsulationPlacement.UtilityFunctions.WarningSwallower
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.InsulationTools.InsulationPlacement.UtilityFunctions;

public class WarningSwallower : IFailuresPreprocessor
{
  public WarningSwallower.FailureType FailType;

  public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
  {
    IList<FailureMessageAccessor> failureMessages = a.GetFailureMessages();
    IEnumerable<FailureMessageAccessor> list1 = (IEnumerable<FailureMessageAccessor>) failureMessages.Where<FailureMessageAccessor>((Func<FailureMessageAccessor, bool>) (m => m.GetDescriptionText().Contains("intersect"))).ToList<FailureMessageAccessor>();
    IEnumerable<FailureMessageAccessor> list2 = (IEnumerable<FailureMessageAccessor>) failureMessages.Where<FailureMessageAccessor>((Func<FailureMessageAccessor, bool>) (m => m.GetDescriptionText().Contains("identical instances"))).ToList<FailureMessageAccessor>();
    if (list1.Count<FailureMessageAccessor>() > 0)
      this.FailType = WarningSwallower.FailureType.Intersect;
    if (list2.Count<FailureMessageAccessor>() > 0)
      this.FailType = this.FailType == WarningSwallower.FailureType.None ? WarningSwallower.FailureType.DuplicateInstances : this.FailType | WarningSwallower.FailureType.DuplicateInstances;
    foreach (FailureMessageAccessor failure in list1.Union<FailureMessageAccessor>(list2))
      a.DeleteWarning(failure);
    return FailureProcessingResult.Continue;
  }

  public enum FailureType
  {
    None,
    Intersect,
    DuplicateInstances,
  }
}
