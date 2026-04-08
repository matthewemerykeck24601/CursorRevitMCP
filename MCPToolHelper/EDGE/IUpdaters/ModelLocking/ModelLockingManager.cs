// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingManager
{
  private Dictionary<string, Dictionary<string, ModelLockingElementData>> m_MasterPermissionsStore;
  private ModelPermissionsCategory All = ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies;
  private ModelLockingOracle _Oracle = new ModelLockingOracle();

  public ModelLockingManager()
  {
    this.m_MasterPermissionsStore = new Dictionary<string, Dictionary<string, ModelLockingElementData>>();
  }

  public IEnumerable<string> GetUniqueIdsForDeletedElementIds(
    Document revitDoc,
    IEnumerable<ElementId> elementIds)
  {
    List<string> deletedElementIds = new List<string>();
    if (this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName))
    {
      foreach (KeyValuePair<string, ModelLockingElementData> keyValuePair in this.m_MasterPermissionsStore[revitDoc.PathName])
      {
        if (elementIds.Contains<ElementId>(keyValuePair.Value.Id))
          deletedElementIds.Add(keyValuePair.Key);
      }
    }
    return (IEnumerable<string>) deletedElementIds;
  }

  public void TrackOpenedDocument(Document revitDoc)
  {
    if (!this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName))
      this.m_MasterPermissionsStore.Add(revitDoc.PathName, new Dictionary<string, ModelLockingElementData>());
    else
      this.m_MasterPermissionsStore[revitDoc.PathName].Clear();
    this.BuildList(revitDoc);
  }

  public void GroomOnReloadLatest(Document revitDoc)
  {
    if (this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName))
      this.m_MasterPermissionsStore[revitDoc.PathName].Clear();
    this.BuildList(revitDoc);
  }

  public void ProcessAddedIds(Document revitDoc, IEnumerable<ElementId> lockedIds)
  {
    if (!this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName))
    {
      this.m_MasterPermissionsStore.Add(revitDoc.PathName, new Dictionary<string, ModelLockingElementData>());
      this.BuildList(revitDoc);
    }
    this.PushToDictionary(revitDoc, lockedIds);
  }

  public void HandleElementDeletion(Document revitDoc, IEnumerable<ElementId> deletedIds)
  {
    if (this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName))
    {
      List<string> stringList = new List<string>();
      foreach (KeyValuePair<string, ModelLockingElementData> keyValuePair in this.m_MasterPermissionsStore[revitDoc.PathName])
      {
        if (deletedIds.Contains<ElementId>(keyValuePair.Value.Id))
          stringList.Add(keyValuePair.Key);
      }
      foreach (string key in stringList)
        this.m_MasterPermissionsStore[revitDoc.PathName].Remove(key);
    }
    else
      QA.LogLine("       *********      Deleting elements in model locked document with no master permissions store: " + revitDoc.PathName);
  }

  internal void ProcessModifiedIds(Document revitDoc, IEnumerable<ElementId> modifiedIds)
  {
    this.HandleElementDeletion(revitDoc, modifiedIds);
    this.PushToDictionary(revitDoc, modifiedIds);
  }

  private void PushToDictionary(Document revitDoc, IEnumerable<ElementId> lockedIds)
  {
    if (lockedIds.Count<ElementId>() == 0)
      return;
    ElementMulticategoryFilter filter1 = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_StructConnections,
      BuiltInCategory.OST_StructuralFraming,
      BuiltInCategory.OST_SpecialityEquipment,
      BuiltInCategory.OST_Walls,
      BuiltInCategory.OST_Assemblies,
      BuiltInCategory.OST_StructuralFoundation,
      BuiltInCategory.OST_Grids,
      BuiltInCategory.OST_Levels,
      BuiltInCategory.OST_ProjectInformation,
      BuiltInCategory.OST_IOSModelGroups
    });
    ElementMulticlassFilter filter2 = new ElementMulticlassFilter((IList<Type>) new List<Type>()
    {
      typeof (AssemblyInstance),
      typeof (FamilyInstance),
      typeof (DatumPlane),
      typeof (Wall),
      typeof (ProjectInfo),
      typeof (Group)
    });
    foreach (Element element in (IEnumerable<Element>) new FilteredElementCollector(revitDoc, (ICollection<ElementId>) lockedIds.ToArray<ElementId>()).WherePasses((ElementFilter) filter2).WherePasses((ElementFilter) filter1).ToElements())
    {
      ModelPermissionsCategory cat = this._Oracle.ResolveModelPermissionsCategory(revitDoc, element.UniqueId);
      if (cat != ModelPermissionsCategory.None)
      {
        if (this.m_MasterPermissionsStore[revitDoc.PathName].ContainsKey(element.UniqueId))
          this.m_MasterPermissionsStore[revitDoc.PathName][element.UniqueId] = new ModelLockingElementData(revitDoc, element.Id, cat);
        else
          this.m_MasterPermissionsStore[revitDoc.PathName].Add(element.UniqueId, new ModelLockingElementData(revitDoc, element.Id, cat));
      }
    }
  }

  public bool UserIsNotAllowedToEdit(
    Document revitDoc,
    Dictionary<Disposition, List<string>> UniqueIds,
    out string failedId,
    out ModelPermissionsCategory requestedCategory)
  {
    string errorMessage = "";
    requestedCategory = ModelPermissionsCategory.NonTracked;
    failedId = "";
    List<string> source = new List<string>();
    List<string> second = new List<string>();
    foreach (string str in UniqueIds[Disposition.Modified])
    {
      Element element = revitDoc.GetElement(str);
      if (element != null && this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName) && this.m_MasterPermissionsStore[revitDoc.PathName].ContainsKey(str))
      {
        ElementId assemblyId = this.m_MasterPermissionsStore[revitDoc.PathName][str].AssemblyId;
        if (assemblyId != (ElementId) null && assemblyId != element.AssemblyInstanceId)
          second.Add(str);
      }
    }
    using (IEnumerator<string> enumerator = UniqueIds[Disposition.Added].Union<string>((IEnumerable<string>) UniqueIds[Disposition.Deleted]).Union<string>((IEnumerable<string>) second).GetEnumerator())
    {
label_17:
      if (enumerator.MoveNext())
      {
        string str1 = enumerator.Current;
        string str2;
        do
        {
          str2 = "";
          if (revitDoc.GetElement(str1) != null)
          {
            Element element = revitDoc.GetElement(str1);
            str2 = element is FamilyInstance ? ((element as FamilyInstance).SuperComponent != null ? (element as FamilyInstance).SuperComponent.UniqueId : "") : "";
          }
          else if (this.m_MasterPermissionsStore[revitDoc.PathName][str1] != null)
            str2 = this.m_MasterPermissionsStore[revitDoc.PathName][str1].SuperComponentID;
          if (!string.IsNullOrEmpty(str2))
            source.Add(str2);
          str1 = str2;
        }
        while (!string.IsNullOrEmpty(str2));
        goto label_17;
      }
    }
    foreach (string str in source.Distinct<string>())
    {
      if (!UniqueIds[Disposition.Added].Contains(str) && !UniqueIds[Disposition.Deleted].Contains(str))
        UniqueIds[Disposition.Modified].Remove(str);
    }
    if (UniqueIds.ContainsKey(Disposition.Added))
    {
      foreach (string desiredId in UniqueIds[Disposition.Added])
      {
        if (!this.UserIsAllowedForElement(revitDoc, desiredId, out errorMessage, out requestedCategory))
        {
          failedId = desiredId;
          return true;
        }
      }
    }
    if (UniqueIds.ContainsKey(Disposition.Modified))
    {
      foreach (string desiredId in UniqueIds[Disposition.Modified])
      {
        if (!this.UserIsAllowedForElement(revitDoc, desiredId, out errorMessage, out requestedCategory, true))
        {
          failedId = desiredId;
          return true;
        }
      }
    }
    if (UniqueIds.ContainsKey(Disposition.Deleted))
    {
      foreach (string desiredId in UniqueIds[Disposition.Deleted])
      {
        if (!this.UserIsAllowedForElement(revitDoc, desiredId, out errorMessage, out requestedCategory))
        {
          failedId = desiredId;
          return true;
        }
      }
    }
    return false;
  }

  public void BuildList(Document revitDoc)
  {
    if (!this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName))
      this.m_MasterPermissionsStore.Add(revitDoc.PathName, new Dictionary<string, ModelLockingElementData>());
    ElementMulticlassFilter filter = new ElementMulticlassFilter((IList<Type>) new List<Type>()
    {
      typeof (AssemblyInstance),
      typeof (FamilyInstance),
      typeof (DatumPlane),
      typeof (Wall),
      typeof (ProjectInfo),
      typeof (Group)
    });
    IEnumerable<ElementId> elementIds = (IEnumerable<ElementId>) new FilteredElementCollector(revitDoc).WherePasses((ElementFilter) filter).ToElementIds();
    if (!elementIds.Any<ElementId>())
      return;
    this.PushToDictionary(revitDoc, elementIds);
  }

  public bool UserIsAllowedForCategory(
    Document revitDoc,
    ModelPermissionsCategory requestedCategory,
    bool bExclusive = false,
    bool bOverride = false)
  {
    if (!ModelLockingUtils.ModelLockingEnabled(revitDoc) || !bOverride && App.DialogSwitches.SuspendModelLockingforOperation)
      return true;
    ModelPermissionsCategory permissionsCategory = ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies;
    if (revitDoc.IsFamilyDocument || !bExclusive && ModelPermissionsCategory.NonTracked == (requestedCategory & ModelPermissionsCategory.NonTracked) || requestedCategory == ModelPermissionsCategory.None)
      return true;
    string upper = revitDoc.Application.Username.ToUpper();
    if (ModelLockingPermissionsSchema.isProducerAdmin(upper))
      return true;
    ModelPermissionsCategory permissionsForUser = ModelLockingPermissionsSchema.GetModelPermissionsForUser(upper, revitDoc.ProjectInformation);
    return (permissionsCategory & requestedCategory) == permissionsCategory ? (permissionsForUser & requestedCategory) == requestedCategory : (!bExclusive ? (permissionsForUser & requestedCategory) > ModelPermissionsCategory.None : (permissionsForUser & requestedCategory) == requestedCategory);
  }

  public void HandleModelSavedAs(string originalPath, string newPath)
  {
    if (this.m_MasterPermissionsStore.ContainsKey(originalPath))
    {
      Dictionary<string, ModelLockingElementData> dictionary = this.m_MasterPermissionsStore[originalPath];
      this.m_MasterPermissionsStore.Remove(originalPath);
      if (this.m_MasterPermissionsStore.ContainsKey(newPath))
        this.m_MasterPermissionsStore[newPath] = dictionary;
      else
        this.m_MasterPermissionsStore.Add(newPath, dictionary);
    }
    else if (this.m_MasterPermissionsStore.ContainsKey(newPath))
      this.m_MasterPermissionsStore[newPath].Clear();
    else
      this.m_MasterPermissionsStore.Add(newPath, new Dictionary<string, ModelLockingElementData>());
  }

  public void DisposeOfLockingManagerForDocument(Document revitDoc)
  {
    string pathName = revitDoc.PathName;
    if (this.m_MasterPermissionsStore.ContainsKey(pathName))
    {
      this.m_MasterPermissionsStore.Remove(pathName);
    }
    else
    {
      if (!ModelLockingUtils.ModelLockingEnabled(revitDoc))
        return;
      QA.InHouseMessage($"Could not find model locking manager for this document: {pathName} Should there be one?");
    }
  }

  public bool UserIsAllowedForElement(Document revitDoc, string desiredId, out string errorMessage)
  {
    errorMessage = "";
    ModelPermissionsCategory requestedPermissionsCategory = ModelPermissionsCategory.None;
    return this.UserIsAllowedForElement(revitDoc, desiredId, out errorMessage, out requestedPermissionsCategory);
  }

  public bool UserIsAllowedForElement(
    Document revitDoc,
    string desiredId,
    out string errorMessage,
    out ModelPermissionsCategory requestedPermissionsCategory,
    bool bModified = false)
  {
    ModelLockingElementData lockingElementData = (ModelLockingElementData) null;
    if (this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName) && this.m_MasterPermissionsStore[revitDoc.PathName].ContainsKey(desiredId))
      lockingElementData = this.m_MasterPermissionsStore[revitDoc.PathName][desiredId];
    bool bExclusive1 = lockingElementData != null && lockingElementData.Exclusive;
    errorMessage = "";
    requestedPermissionsCategory = ModelPermissionsCategory.NonTracked;
    if (App.DialogSwitches.SuspendModelLockingforOperation || !ModelLockingUtils.ModelLockingEnabled(revitDoc) || revitDoc.IsFamilyDocument)
      return true;
    revitDoc.Application.Username.ToUpper();
    requestedPermissionsCategory = ModelPermissionsCategory.None;
    if (bModified)
    {
      Element element = revitDoc.GetElement(desiredId);
      if (element is Group)
        bExclusive1 = true;
      ModelPermissionsCategory permissionsCategory1 = this._Oracle.ResolveModelPermissionsCategory(revitDoc, desiredId);
      if (lockingElementData == null)
      {
        lockingElementData = new ModelLockingElementData(revitDoc, element.Id, permissionsCategory1);
        QA.LogLine("Modified member missing old permissions in master store." + desiredId);
      }
      switch (element)
      {
        case Level _ when !(element as Level).FindAssociatedPlanViewId().Equals((object) lockingElementData.PlanViewID):
          lockingElementData.PlanViewID = (element as Level).FindAssociatedPlanViewId();
          return true;
        case AssemblyInstance _:
          if (!this.TransformsMatch(lockingElementData.transform, element) || lockingElementData.AssemblyMemberCount != (element as AssemblyInstance).GetMemberIds().Count)
          {
            requestedPermissionsCategory = ModelPermissionsCategory.Assemblies;
            return this.UserIsAllowedForCategory(revitDoc, ModelPermissionsCategory.Assemblies);
          }
          if (this.ParametersMatch(lockingElementData.ParameterDict, element))
            return true;
          break;
      }
      ModelPermissionsCategory permissionsCategory2 = lockingElementData.PermissionsCategory;
      bool flag1 = this.UserIsAllowedForCategory(revitDoc, lockingElementData.PermissionsCategory, bExclusive1);
      bool flag2 = this.UserIsAllowedForCategory(revitDoc, permissionsCategory1, bExclusive1);
      bool flag3 = flag1 & flag2;
      if (!flag3)
        requestedPermissionsCategory = flag1 ? permissionsCategory1 : permissionsCategory2;
      if (!lockingElementData.AssemblyId.Equals((object) element.AssemblyInstanceId))
      {
        requestedPermissionsCategory = ModelPermissionsCategory.Assemblies;
        flag3 = this.UserIsAllowedForCategory(revitDoc, ModelPermissionsCategory.Assemblies);
      }
      return flag3;
    }
    bool bExclusive2;
    if (lockingElementData != null)
    {
      requestedPermissionsCategory = this.m_MasterPermissionsStore[revitDoc.PathName][desiredId].PermissionsCategory;
      bExclusive2 = lockingElementData.Exclusive;
    }
    else
    {
      requestedPermissionsCategory = this._Oracle.ResolveModelPermissionsCategory(revitDoc, desiredId);
      bExclusive2 = revitDoc.GetElement(desiredId) is Group;
    }
    return this.UserIsAllowedForCategory(revitDoc, requestedPermissionsCategory, bExclusive2);
  }

  public bool TransformsMatch(Transform oldTrans, Element elem)
  {
    Transform transform = Transform.Identity;
    if (elem is FamilyInstance)
      transform = (elem as FamilyInstance).GetTransform();
    if (elem is AssemblyInstance)
      transform = (elem as AssemblyInstance).GetTransform();
    return oldTrans.Origin.IsAlmostEqualTo(transform.Origin) && oldTrans.BasisX.IsAlmostEqualTo(transform.BasisX) && oldTrans.BasisY.IsAlmostEqualTo(transform.BasisY) && oldTrans.BasisZ.IsAlmostEqualTo(transform.BasisZ);
  }

  public List<string> assemblyParamsToIgnore
  {
    get
    {
      return new List<string>()
      {
        "TICKET_CREATED_USER_INITIAL",
        "TICKET_CREATED_DATE_INITIAL",
        "TICKET_CREATED_USER_CURRENT",
        "TICKET_CREATED_DATE_CURRENT",
        "TKT_VIEWSHEET_COUNT",
        "TICKET_FLAGGED",
        "TKT_TOTAL_CREATED",
        "TICKET_EDIT_COMMENT",
        "Edited by"
      };
    }
  }

  public bool ParametersMatch(Dictionary<string, string> oldParams, Element elem)
  {
    Dictionary<string, string> dictionary = ModelLockingElementData.ConvertParamsToDictionary(elem);
    foreach (string key in dictionary.Keys)
    {
      if (!this.assemblyParamsToIgnore.Contains(key) && (oldParams.ContainsKey(key) && dictionary[key] != oldParams[key] || !oldParams.ContainsKey(key) && dictionary[key] != string.Empty))
        return false;
    }
    return true;
  }

  public ModelLockingElementData GetOldPermissionsData(Document revitDoc, string desiredId)
  {
    ModelLockingElementData oldPermissionsData = new ModelLockingElementData();
    if (this.m_MasterPermissionsStore.ContainsKey(revitDoc.PathName) && this.m_MasterPermissionsStore[revitDoc.PathName].ContainsKey(desiredId))
      oldPermissionsData = this.m_MasterPermissionsStore[revitDoc.PathName][desiredId];
    return oldPermissionsData;
  }

  public Result ConfigureSettings(Document revitDoc)
  {
    Autodesk.Revit.ApplicationServices.Application application = revitDoc.Application;
    ModelLockingSettingsForm lockingSettingsForm = new ModelLockingSettingsForm(revitDoc);
    lockingSettingsForm.Permissions_AllAllowed = ModelLockingPermissionsSchema.GetModelPermissions(ModelPermissionsCategory.Admin, (Element) revitDoc.ProjectInformation);
    lockingSettingsForm.Permissions_GeometryAllowed = ModelLockingPermissionsSchema.GetModelPermissions(ModelPermissionsCategory.Geometry, (Element) revitDoc.ProjectInformation);
    lockingSettingsForm.Permissions_ConnectionHardwareAllowed = ModelLockingPermissionsSchema.GetModelPermissions(ModelPermissionsCategory.ConnectionsHardware, (Element) revitDoc.ProjectInformation);
    lockingSettingsForm.Permissions_RebarHandlingAllowed = ModelLockingPermissionsSchema.GetModelPermissions(ModelPermissionsCategory.RebarHandling, (Element) revitDoc.ProjectInformation);
    lockingSettingsForm.Permissions_AssembliesAllowed = ModelLockingPermissionsSchema.GetModelPermissions(ModelPermissionsCategory.Assemblies, (Element) revitDoc.ProjectInformation);
    int num1 = (int) lockingSettingsForm.ShowDialog();
    if (lockingSettingsForm.DialogResult == DialogResult.Cancel)
      return (Result) 1;
    using (Transaction transaction = new Transaction(revitDoc, "Set Model Locking Permissions"))
    {
      int num2 = (int) transaction.Start();
      if (transaction.HasStarted())
      {
        ModelLockingPermissionsSchema.SetModelPermissions(ModelPermissionsCategory.Admin, (Element) revitDoc.ProjectInformation, lockingSettingsForm.Permissions_AllAllowed);
        ModelLockingPermissionsSchema.SetModelPermissions(ModelPermissionsCategory.Geometry, (Element) revitDoc.ProjectInformation, lockingSettingsForm.Permissions_GeometryAllowed);
        ModelLockingPermissionsSchema.SetModelPermissions(ModelPermissionsCategory.ConnectionsHardware, (Element) revitDoc.ProjectInformation, lockingSettingsForm.Permissions_ConnectionHardwareAllowed);
        ModelLockingPermissionsSchema.SetModelPermissions(ModelPermissionsCategory.RebarHandling, (Element) revitDoc.ProjectInformation, lockingSettingsForm.Permissions_RebarHandlingAllowed);
        ModelLockingPermissionsSchema.SetModelPermissions(ModelPermissionsCategory.Assemblies, (Element) revitDoc.ProjectInformation, lockingSettingsForm.Permissions_AssembliesAllowed);
        return transaction.Commit() == TransactionStatus.Committed ? (Result) 0 : (Result) -1;
      }
    }
    return (Result) -1;
  }

  public override string ToString()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (KeyValuePair<string, Dictionary<string, ModelLockingElementData>> keyValuePair1 in this.m_MasterPermissionsStore)
    {
      stringBuilder.AppendLine("Document: " + keyValuePair1.Key);
      foreach (KeyValuePair<string, ModelLockingElementData> keyValuePair2 in keyValuePair1.Value)
      {
        if (keyValuePair2.Value.PermissionsCategory == ModelPermissionsCategory.Assemblies)
          stringBuilder.AppendFormat("     {0:40}, {1}\n", (object) keyValuePair2.Key, (object) keyValuePair2.Value.ToString());
      }
    }
    return stringBuilder.ToString();
  }
}
