// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.UpdtTktAcceptEdits
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class UpdtTktAcceptEdits : IExternalCommand
{
  public static string ElementName { get; set; }

  public static AssemblyInstance Assembly { get; set; }

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    using (Transaction transaction = new Transaction(activeUiDocument.Document, "Accept Edits - " + UpdtTktAcceptEdits.ElementName))
    {
      try
      {
        int num = (int) transaction.Start();
        return UpdtTktFlagStatus.Update(activeUiDocument, transaction);
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = ex.ToString();
        return (Result) -1;
      }
    }
  }
}
