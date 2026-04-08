// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.StandardRebarUpdater
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.AdminUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.IUpdaters;

internal class StandardRebarUpdater : IUpdater
{
  private readonly UpdaterId _updaterId;

  public StandardRebarUpdater(AddInId id)
  {
    this._updaterId = new UpdaterId(id, new Guid("ba1ecfd8-6a1b-43c8-9f33-b20e0e9b5366"));
  }

  public void Execute(UpdaterData data)
  {
    Document document = data.GetDocument();
    ActiveModel.GetInformation(document);
    using (new SubTransaction(document))
    {
      try
      {
        if (!string.IsNullOrEmpty(document.PathName) || !EdgeBuildInformation.IsDebugCheck)
          return;
        new TaskDialog("Error")
        {
          MainContent = "The file is has not been saved.",
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
        }.Show();
      }
      catch (Exception ex)
      {
        new TaskDialog("Error")
        {
          MainContent = ex.ToString(),
          FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
        }.Show();
      }
    }
  }

  public UpdaterId GetUpdaterId() => this._updaterId;

  public ChangePriority GetChangePriority() => ChangePriority.Structure;

  public string GetUpdaterName() => "Rebar Standards Updater";

  public string GetAdditionalInformation()
  {
    return "Updates manufacturer standard rebar parameter array with to match the opened revit model settings.";
  }
}
