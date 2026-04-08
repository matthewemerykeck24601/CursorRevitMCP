// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.BulkFamilyUpdater.FailurePreproccessor
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.__Testing.BulkFamilyUpdater;

public class FailurePreproccessor : IFailuresPreprocessor
{
  public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
  {
    IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
    if (failureMessages.Count == 0)
      return FailureProcessingResult.Continue;
    foreach (FailureMessageAccessor failure in (IEnumerable<FailureMessageAccessor>) failureMessages)
    {
      if (failure.GetFailureDefinitionId() == BuiltInFailures.OverlapFailures.CurvesOverlap)
      {
        failuresAccessor.DeleteWarning(failure);
      }
      else
      {
        string output = failure.GetDescriptionText().Replace("\r\n", "");
        TaskManager.currentTaskCopy.addOutputMessage(BulkFamilyUpdatersUtils.messageType.fail, output);
        FailureHandlingOptions failureHandlingOptions = failuresAccessor.GetFailureHandlingOptions();
        failureHandlingOptions.SetClearAfterRollback(true);
        failuresAccessor.SetFailureHandlingOptions(failureHandlingOptions);
        int num = (int) failuresAccessor.RollBackPendingTransaction();
        return FailureProcessingResult.ProceedWithRollBack;
      }
    }
    return FailureProcessingResult.ProceedWithCommit;
  }
}
