// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking.ModelLockingQA;

public class ModelLockingQA
{
  public static void QALogElementChanges(
    Document revitDoc,
    Dictionary<Disposition, List<string>> changeLists)
  {
    try
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("");
      stringBuilder.AppendLine("#######################################################################################################################################################");
      stringBuilder.AppendLine("####                Model Locking Requested Elements                                                                                                ###");
      stringBuilder.AppendLine("-------------------------------------------------------------------------------------------------------------------------------------------------------");
      stringBuilder.AppendLine("ADDED Elements:");
      if (changeLists.ContainsKey(Disposition.Added))
      {
        foreach (string uniqueId in changeLists[Disposition.Added])
        {
          Element element = revitDoc.GetElement(uniqueId);
          stringBuilder.AppendLine(EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.GetInfoString(element, false));
        }
      }
      stringBuilder.AppendLine("MODIFIED Elements:");
      if (changeLists.ContainsKey(Disposition.Modified))
      {
        foreach (string uniqueId in changeLists[Disposition.Modified])
        {
          Element element = revitDoc.GetElement(uniqueId);
          stringBuilder.AppendLine(EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.GetInfoString(element, true));
        }
      }
      stringBuilder.AppendLine("DELETED Elements:");
      if (changeLists.ContainsKey(Disposition.Deleted))
      {
        foreach (string desiredId in changeLists[Disposition.Deleted])
        {
          ModelLockingElementData oldPermissionsData = App.LockingManger.GetOldPermissionsData(revitDoc, desiredId);
          stringBuilder.AppendLine($"    ElementId: {oldPermissionsData.Id,10}                                                                      LockingCategory: {oldPermissionsData.PermissionsCategory,20}");
        }
      }
      stringBuilder.AppendLine("-------------------------------------------------------------------------------------------------------------------------------------------------------");
      stringBuilder.AppendLine("####                Model Locking Requested Elements                                                                                                ###");
      stringBuilder.AppendLine("#######################################################################################################################################################");
      QAUtils.LogLine(stringBuilder.ToString());
    }
    catch (Exception ex)
    {
      QAUtils.LogLine("*****************************  ERRORED in logging util: ModelLockingQA " + ex.Message);
    }
  }

  private static string GetInfoString(Element elem, bool bModified)
  {
    switch (elem)
    {
      case FamilyInstance _:
        return EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.Format(elem as FamilyInstance, bModified);
      case DatumPlane _:
        return EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.Format(elem as DatumPlane, bModified);
      case AssemblyInstance _:
        return EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.Format(elem as AssemblyInstance, bModified);
      default:
        return $"    ElementId: {elem.Id,-10} ElementName: {elem.Name,-20} FamilyType:                                          LockingCategory: {EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.GetModifiedInfo(elem, false),-20}";
    }
  }

  private static string Format(FamilyInstance faminst, bool bModified)
  {
    return $"    ElementId: {faminst.Id,-10} ElementName: {faminst.Name,-20} FamilyType: {$"{faminst.Symbol.FamilyName}:{faminst.Symbol.Name}",-40} LockingCategory: {EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.GetModifiedInfo((Element) faminst, bModified),-20}";
  }

  private static string Format(DatumPlane datPlane, bool bModified)
  {
    return $"    ElementId: {datPlane.Id,-10} ElementName: {datPlane.Name,-20} FamilyType:                                         LockingCategory: {EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.GetModifiedInfo((Element) datPlane, bModified),-20}";
  }

  private static string Format(AssemblyInstance assemInst, bool bModified)
  {
    return $"    ElementId: {assemInst.Id,-10} ElementName: {assemInst.Name,-20} FamilyType: {assemInst.AssemblyTypeName,-40} LockingCategory: {EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.GetModifiedInfo((Element) assemInst, bModified),-20}";
  }

  private static string GetModifiedInfo(Element elem, bool bModified) => "<uncomment for info>";
}
