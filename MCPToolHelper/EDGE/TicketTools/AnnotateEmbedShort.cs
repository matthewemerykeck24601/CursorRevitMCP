// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.AnnotateEmbedShort
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using Utils.Exceptions;
using Utils.Forms;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class AnnotateEmbedShort : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    Document document = activeUiDocument.Document;
    while (true)
    {
      Element element;
      try
      {
        element = document.GetElement(activeUiDocument.Selection.PickObject((ObjectType) 1, "Select an element to annotate.").ElementId);
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        return (Result) 0;
      }
      catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
      {
        ExceptionMessages.ShowInvalidViewMessage((Exception) ex);
        return (Result) 1;
      }
      AnnotateEmbedShort.Execute(document, activeUiDocument, element, document.ActiveView);
    }
  }

  public static Result Execute(Document doc, UIDocument uidoc, Element selectedElem, View view)
  {
    string name = "IDENTITY_DESCRIPTION_SHORT";
    ElementMulticategoryFilter filter = new ElementMulticategoryFilter((ICollection<BuiltInCategory>) new List<BuiltInCategory>()
    {
      BuiltInCategory.OST_StructuralFraming,
      BuiltInCategory.OST_StructConnections,
      BuiltInCategory.OST_GenericModel,
      BuiltInCategory.OST_SpecialityEquipment
    });
    new FilteredElementCollector(doc).WherePasses((ElementFilter) filter).OfClass(typeof (FamilyInstance));
    using (Transaction transaction = new Transaction(doc, "Annotate Embed Short"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        Parameter parameter = selectedElem.LookupParameter(name);
        string str = "";
        if (parameter == null)
        {
          str = "";
          parameter = doc.GetElement(selectedElem.GetTypeId()).LookupParameter(name);
          if (parameter == null)
            str = "";
        }
        if (parameter != null)
          str = parameter.AsString();
        string text = str;
        int x = (int) view.UpDirection.X;
        double angle = Math.Atan2((double) ((int) view.UpDirection.Z - 1), (double) x);
        XYZ point1 = (selectedElem.Location as LocationPoint).Point;
        Transform rotationAtPoint = Transform.CreateRotationAtPoint(XYZ.BasisX, angle, point1);
        XYZ xyz1 = new XYZ(1.0, 0.0, -1.76);
        XYZ point2 = point1 - xyz1;
        XYZ point3 = new XYZ(point2.X - 0.75, point2.Y, point2.Z);
        XYZ xyz2 = rotationAtPoint.OfPoint(point2);
        XYZ position = rotationAtPoint.OfPoint(point3);
        if (string.IsNullOrWhiteSpace(text))
        {
          new TaskDialog("Error")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainInstruction = "Error:  The selected Element has no IDENTITY_DESCRIPTION_SHORT Parameter value. Press \"Close\" below to continue annotating."
          }.Show();
          int num2 = (int) transaction.RollBack();
          return (Result) 1;
        }
        TextNote textNote = TextNote.Create(doc, view.Id, position, text, new TextNoteOptions()
        {
          HorizontalAlignment = HorizontalTextAlignment.Left,
          TypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType)
        });
        textNote.get_Parameter(BuiltInParameter.LEADER_LEFT_ATTACHMENT).Set(1);
        textNote.get_Parameter(BuiltInParameter.LEADER_RIGHT_ATTACHMENT).Set(1);
        if (view.UpDirection.IsAlmostEqualTo(new XYZ(0.0, 0.0, -1.0)))
          textNote.get_Parameter(BuiltInParameter.TEXT_ALIGN_HORZ).Set(256 /*0x0100*/);
        else
          textNote.get_Parameter(BuiltInParameter.TEXT_ALIGN_HORZ).Set(64 /*0x40*/);
        Leader leader = textNote.AddLeader(TextNoteLeaderTypes.TNLT_STRAIGHT_L);
        leader.End = point1;
        leader.Elbow = xyz2;
        int num3 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("The user aborted the pick operation."))
          return (Result) 0;
        if (ex.ToString().Contains("Object reference not set to an instance of an object."))
        {
          TaskDialog.Show("Error", "The selected Element is not valid for annotation. Only Structural Framing (excluding Assemblies), Specialty Equipment, and Generic Model Elements can be annotated.");
          int num = (int) transaction.RollBack();
          return (Result) 0;
        }
        int num4 = (int) transaction.RollBack();
        ErrorForm errorForm = new ErrorForm();
        errorForm.textBox1.Text = ex.ToString();
        int num5 = (int) errorForm.ShowDialog();
        return (Result) 0;
      }
    }
  }
}
