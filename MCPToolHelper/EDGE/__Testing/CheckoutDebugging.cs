// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.CheckoutDebugging
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Text;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class CheckoutDebugging : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    Document document = commandData.Application.ActiveUIDocument.Document;
    StringBuilder stringBuilder = new StringBuilder();
    foreach (ElementId elementId in (IEnumerable<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds())
    {
      CheckoutStatus checkoutStatus = WorksharingUtils.GetCheckoutStatus(document, elementId);
      ModelUpdatesStatus modelUpdatesStatus = WorksharingUtils.GetModelUpdatesStatus(document, elementId);
      stringBuilder.AppendLine($"id: {elementId?.ToString()} - {document.GetElement(elementId).Name}");
      stringBuilder.AppendLine("        -CheckOut: " + checkoutStatus.ToString());
      stringBuilder.AppendLine("        -ModUpdat: " + modelUpdatesStatus.ToString());
    }
    TaskDialog.Show("PreCheckout Status:", stringBuilder.ToString());
    stringBuilder.Clear();
    Transaction transaction = new Transaction(document, "TestCheckout");
    int num1 = (int) transaction.Start();
    ICollection<ElementId> elementIds = WorksharingUtils.CheckoutElements(document, commandData.Application.ActiveUIDocument.Selection.GetElementIds());
    stringBuilder.AppendLine("Attempted to Check out:");
    foreach (ElementId elementId in (IEnumerable<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds())
      stringBuilder.AppendLine($"     id: {elementId?.ToString()} - {document.GetElement(elementId).Name}");
    stringBuilder.AppendLine("CheckedOut:");
    foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
      stringBuilder.AppendLine($"     id: {id?.ToString()} - {document.GetElement(id).Name}");
    TaskDialog.Show("CheckedOUt:", stringBuilder.ToString());
    int num2 = (int) transaction.Commit();
    stringBuilder.Clear();
    foreach (ElementId elementId in (IEnumerable<ElementId>) elementIds)
    {
      CheckoutStatus checkoutStatus = WorksharingUtils.GetCheckoutStatus(document, elementId);
      ModelUpdatesStatus modelUpdatesStatus = WorksharingUtils.GetModelUpdatesStatus(document, elementId);
      stringBuilder.AppendLine($"id: {elementId?.ToString()} - {document.GetElement(elementId).Name}");
      stringBuilder.AppendLine("        -CheckOut: " + checkoutStatus.ToString());
      stringBuilder.AppendLine("        -ModUpdat: " + modelUpdatesStatus.ToString());
    }
    TaskDialog.Show("Status:", stringBuilder.ToString());
    return (Result) 0;
  }
}
