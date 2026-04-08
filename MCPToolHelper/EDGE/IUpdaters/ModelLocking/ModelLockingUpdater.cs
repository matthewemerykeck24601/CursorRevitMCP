// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

internal class ModelLockingUpdater : IUpdater
{
  private readonly UpdaterId _updaterId;
  private readonly FailureDefinitionId _failureDefinitionId;
  private readonly FailureDefinitionId _failureDefinitionId_Geometry;
  private readonly FailureDefinitionId _failureDefinitionId_RebarAndHandling;
  private readonly FailureDefinitionId _failureDefinitionId_ConnectionsHardware;
  private readonly FailureDefinitionId _failureDefinitionId_Assemblies;

  public ModelLockingUpdater(AddInId addInId)
  {
    this._updaterId = new UpdaterId(addInId, new Guid("0ED8134A-709F-4E72-8EEC-BD188C31C71B"));
    this._failureDefinitionId = new FailureDefinitionId(Guid.NewGuid());
    this._failureDefinitionId_Geometry = new FailureDefinitionId(Guid.NewGuid());
    this._failureDefinitionId_RebarAndHandling = new FailureDefinitionId(Guid.NewGuid());
    this._failureDefinitionId_ConnectionsHardware = new FailureDefinitionId(Guid.NewGuid());
    this._failureDefinitionId_Assemblies = new FailureDefinitionId(Guid.NewGuid());
    this.InitializeFailureDefinitions();
  }

  private void InitializeFailureDefinitions()
  {
    FailureDefinition.CreateFailureDefinition(this._failureDefinitionId, FailureSeverity.Error, "Model Locking: You are not authorized to edit this element.");
    FailureDefinition.CreateFailureDefinition(this._failureDefinitionId_Geometry, FailureSeverity.Error, "Model Locking: You have attempted to modify model Geometry.  Your current user Model Locking settings do not allow you to modify this element.");
    FailureDefinition.CreateFailureDefinition(this._failureDefinitionId_RebarAndHandling, FailureSeverity.Error, "Model Locking: You have attempted to modify model Rebar or Handling elements.  Your current user Model Locking settings do not allow you to modify this element.");
    FailureDefinition.CreateFailureDefinition(this._failureDefinitionId_ConnectionsHardware, FailureSeverity.Error, "Model Locking: You have attempted to modify model Connections or Hardware elements.  Your current user Model Locking settings do not allow you to modify this element.");
    FailureDefinition.CreateFailureDefinition(this._failureDefinitionId_Assemblies, FailureSeverity.Error, "Model Locking: You have attempted to modify model Assemblies.  Your current user Model Locking settings do not allow you to modify this element.");
  }

  public void Execute(UpdaterData data)
  {
    QA.LogLine("-----------------------------------------------------------------------------------------------------------------------------------------");
    QA.LogLine("---------    MODEL LOCKING UPDATER CALLED");
    try
    {
      Document revitDoc = data.GetDocument();
      if (!ModelLockingUtils.ModelLockingEnabled(revitDoc) || App.DialogSwitches.SuspendModelLockingforOperation || App.LockingManger.UserIsAllowedForCategory(revitDoc, ModelPermissionsCategory.Admin))
        return;
      IEnumerable<ElementId> deletedElementIds1 = (IEnumerable<ElementId>) data.GetDeletedElementIds();
      IEnumerable<ElementId> addedElementIds = (IEnumerable<ElementId>) data.GetAddedElementIds();
      IEnumerable<ElementId> modifiedElementIds = (IEnumerable<ElementId>) data.GetModifiedElementIds();
      IEnumerable<string> deletedElementIds2 = App.LockingManger.GetUniqueIdsForDeletedElementIds(revitDoc, deletedElementIds1);
      Dictionary<Disposition, List<string>> dictionary = new Dictionary<Disposition, List<string>>();
      dictionary.Add(Disposition.Deleted, deletedElementIds2.ToList<string>());
      dictionary.Add(Disposition.Added, addedElementIds.Select<ElementId, string>((Func<ElementId, string>) (s => revitDoc.GetElement(s).UniqueId)).ToList<string>());
      dictionary.Add(Disposition.Modified, modifiedElementIds.Select<ElementId, string>((Func<ElementId, string>) (s => revitDoc.GetElement(s).UniqueId)).ToList<string>());
      if (QA.SpecialPermission)
        EDGE.IUpdaters.ModelLocking.ModelLockingQA.ModelLockingQA.QALogElementChanges(revitDoc, dictionary);
      string failedId = "";
      ModelPermissionsCategory permissionsForUser = ModelLockingPermissionsSchema.GetModelPermissionsForUser(revitDoc.Application.Username.ToUpper(), revitDoc.ProjectInformation);
      ModelPermissionsCategory requestedCategory;
      if (!App.LockingManger.UserIsNotAllowedToEdit(revitDoc, dictionary, out failedId, out requestedCategory))
        return;
      ModelPermissionsCategory permissionsCategory = (permissionsForUser ^ requestedCategory) & requestedCategory;
      List<FailureMessage> failureMessageList = new List<FailureMessage>();
      if (permissionsCategory.HasFlag((Enum) ModelPermissionsCategory.Geometry))
        failureMessageList.Add(new FailureMessage(this._failureDefinitionId_Geometry));
      if (permissionsCategory.HasFlag((Enum) ModelPermissionsCategory.RebarHandling))
        failureMessageList.Add(new FailureMessage(this._failureDefinitionId_RebarAndHandling));
      if (permissionsCategory.HasFlag((Enum) ModelPermissionsCategory.ConnectionsHardware))
        failureMessageList.Add(new FailureMessage(this._failureDefinitionId_ConnectionsHardware));
      if (permissionsCategory.HasFlag((Enum) ModelPermissionsCategory.Assemblies))
        failureMessageList.Add(new FailureMessage(this._failureDefinitionId_Assemblies));
      if (failureMessageList.Count == 0)
        failureMessageList.Add(new FailureMessage(this._failureDefinitionId));
      foreach (FailureMessage failure in failureMessageList)
        revitDoc.PostFailure(failure);
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public ChangePriority GetChangePriority() => ChangePriority.GridsLevelsReferencePlanes;

  public string GetUpdaterName() => "Model Locking Updater";

  public string GetAdditionalInformation()
  {
    return "This Updater prevents the manipulation of Elements with a CONSTRUCTION_PRODUCT or MANUFACTURE_COMPONENT Parameter value by unauthorized users.";
  }
}
