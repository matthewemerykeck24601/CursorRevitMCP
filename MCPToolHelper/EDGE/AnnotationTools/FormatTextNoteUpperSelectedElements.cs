// Decompiled with JetBrains decompiler
// Type: EDGE.AnnotationTools.FormatTextNoteUpperSelectedElements
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using Utils.TextUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AnnotationTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class FormatTextNoteUpperSelectedElements : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    ICollection<ElementId> selectedIdList = activeUiDocument.Selection.GetElementIds();
    if (selectedIdList.Count == 0)
      selectedIdList = References.PickNewReferences("Pick the TextNotes to be formatted to uppercase.");
    if (selectedIdList.Count == 0)
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, "Format TextNotes: To Uppercase"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        FormatTextNote.ToUpper(selectedIdList);
        int num2 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = $"Error:  There was an error formatting the selected TextNotes to uppercase.{Environment.NewLine}{Environment.NewLine}{ex.ToString()}";
        return (Result) -1;
      }
    }
  }
}
