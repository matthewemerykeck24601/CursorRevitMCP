// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.EntourageWarning
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.IUpdaters;

internal class EntourageWarning : IUpdater
{
  private readonly UpdaterId _updaterId;

  public EntourageWarning(AddInId addInId)
  {
    this._updaterId = new UpdaterId(addInId, new Guid("F687C3C9-AE7C-4715-B198-30E3C7851277"));
  }

  public void Execute(UpdaterData data)
  {
    if (!new FilteredElementCollector(data.GetDocument(), (ICollection<ElementId>) data.GetAddedElementIds().Concat<ElementId>((IEnumerable<ElementId>) data.GetModifiedElementIds()).ToList<ElementId>()).OfCategory(BuiltInCategory.OST_Entourage).Any<Element>() || !App.DialogSwitches.ShowEntourageWarning || App.DialogSwitches.SuspendEntourageWarning)
      return;
    TaskDialog taskDialog = new TaskDialog("EDGE Warning");
    taskDialog.FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)";
    taskDialog.MainInstruction = "Modified or Added Entourage";
    taskDialog.MainContent = "Entourage is used in EDGE to represent warped element visualization.  Moving or modifying these elements will not affect modeled specialty equipment.";
    taskDialog.ExpandedContent = "The Auto Warping feature creates Revit Direct Shape elements to represent the location of connecting elements in their warped configuration.  You are free to move or modify them but realize that this will have no affect on the actual connection elements in the specialty equipment category.";
    taskDialog.VerificationText = "Do Not Show This Warning Again";
    taskDialog.Show();
    if (!taskDialog.WasVerificationChecked())
      return;
    App.DialogSwitches.ShowEntourageWarning = false;
  }

  public string GetAdditionalInformation()
  {
    return "Ignorable warning about moving/modifying entourage category as it is used by EDGE for visualization";
  }

  public ChangePriority GetChangePriority() => ChangePriority.Connections;

  public UpdaterId GetUpdaterId() => this._updaterId;

  public string GetUpdaterName() => nameof (EntourageWarning);
}
