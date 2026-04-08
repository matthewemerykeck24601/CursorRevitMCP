// Decompiled with JetBrains decompiler
// Type: EDGE.SelectionAndPinningTools.UnpinAllVoids
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.SelectionAndPinningTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class UnpinAllVoids : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document doc = activeUiDocument.Document;
    ICollection<ElementId> voidIds = Components.GetVoidIds(doc);
    using (Transaction transaction = new Transaction(doc, "Pin All Voids"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        foreach (Element element in voidIds.Select<ElementId, Element>((Func<ElementId, Element>) (voidId => doc.GetElement(voidId))).Where<Element>((Func<Element, bool>) (voidEle => voidEle.Pinned)))
          element.Pinned = false;
        int num2 = (int) transaction.Commit();
        return (Result) 0;
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
