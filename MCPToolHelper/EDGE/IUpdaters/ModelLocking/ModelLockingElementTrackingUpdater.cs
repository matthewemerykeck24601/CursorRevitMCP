// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingElementTrackingUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

internal class ModelLockingElementTrackingUpdater : IUpdater
{
  private readonly UpdaterId _updaterId;
  private readonly FailureDefinitionId _failureDefinitionId_FileNotSaved;

  public ModelLockingElementTrackingUpdater(AddInId addInId)
  {
    this._updaterId = new UpdaterId(addInId, new Guid("5954DB86-30D6-46DD-B674-C839B81A9CED"));
    this._failureDefinitionId_FileNotSaved = new FailureDefinitionId(Guid.NewGuid());
    FailureDefinition.CreateFailureDefinition(this._failureDefinitionId_FileNotSaved, FailureSeverity.Error, "Error: Model Locking is Enabled in EDGE Preferences.  You must save this document in order to use model locking features.");
  }

  public void Execute(UpdaterData data)
  {
    try
    {
      Document document = data.GetDocument();
      ActiveModel.GetInformation(document);
      if (!ModelLockingUtils.ModelLockingEnabled(document) || !string.IsNullOrEmpty(document.PathName))
        return;
      FailureMessage failureMessage = new FailureMessage(this._failureDefinitionId_FileNotSaved);
    }
    catch (Exception ex)
    {
      throw;
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public ChangePriority GetChangePriority() => ChangePriority.Annotations;

  public string GetUpdaterName() => "Model Locking Element Tracking Updater";

  public string GetAdditionalInformation()
  {
    return "This Updater keeps track of all Elements with the CONSTRUCTION_PRODUCTor MANUFACTURE_COMPONENT Parameter.";
  }
}
