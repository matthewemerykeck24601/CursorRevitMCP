// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.ControlNumberPopulator
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Utils.AssemblyUtils;
using Utils.CollectionUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.TicketTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class ControlNumberPopulator : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    string cm = "CONTROL_MARK";
    string cn = "CONTROL_NUMBER";
    string str1 = "";
    string text1 = "";
    List<ElementId> elementIdList = new List<ElementId>();
    ICollection<Element> source1 = StructuralFraming.RefineNestedFamilies(document);
    using (Transaction transaction = new Transaction(document, "Control Number Populator"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        if (!(document.ActiveView is ViewSheet) || !document.ActiveView.IsAssemblyView.Equals(true))
        {
          new TaskDialog("Error")
          {
            FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
            MainContent = "This tool must be run within an Assembly Sheet."
          }.Show();
          return (Result) 1;
        }
        if (document.ActiveView is ViewSheet activeView)
        {
          AssemblyInstance element = document.GetElement(activeView.AssociatedAssemblyInstanceId) as AssemblyInstance;
          if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) element, "HARDWARE_DETAIL"))
          {
            new TaskDialog("Error")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainContent = "This tool to does not support hardware detail assembly view sheets."
            }.Show();
            return (Result) 1;
          }
          if (element.GetStructuralFramingElement() == null)
          {
            new TaskDialog("Error")
            {
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
              MainContent = "This tool does not support non strucutal framing assembly view sheet."
            }.Show();
            return (Result) 1;
          }
        }
        using (SubTransaction subTransaction = new SubTransaction(document))
        {
          int num2 = (int) subTransaction.Start();
          Line bound = Line.CreateBound(XYZ.Zero, XYZ.BasisX);
          ElementId id = document.Create.NewDetailCurve(document.ActiveView, (Curve) bound).Id;
          document.Delete(id);
          int num3 = (int) subTransaction.Commit();
        }
        Element element1 = document.GetElement((document.ActiveView as ViewSheet).AssociatedAssemblyInstanceId);
        string cmValue = (element1.LookupParameter("ASSEMBLY_MARK_NUMBER") ?? document.GetElement(element1.GetTypeId()).LookupParameter("ASSEMBLY_MARK_NUMBER")).AsString();
        if (cmValue == null)
          cmValue = "";
        IEnumerable<string> source2 = source1.Where<Element>((Func<Element, bool>) (sfElem => sfElem.LookupParameter(cm) != null && sfElem.LookupParameter(cm).HasValue && sfElem.LookupParameter(cm).AsString().Equals(cmValue))).Select<Element, string>((Func<Element, string>) (sfElem => sfElem.LookupParameter(cn).AsString())).Distinct<string>();
        IEnumerable<ElementId> elementIds = source1.Where<Element>((Func<Element, bool>) (sfElem =>
        {
          if (sfElem.LookupParameter(cm) == null || !sfElem.LookupParameter(cm).HasValue || !sfElem.LookupParameter(cm).AsString().Equals(cmValue))
            return false;
          return sfElem.LookupParameter(cn).AsString().Equals("") || sfElem.LookupParameter(cn).AsString().Equals(" ") || !sfElem.LookupParameter(cn).HasValue;
        })).Select<Element, ElementId>((Func<Element, ElementId>) (sfElem => sfElem.Id));
        IEnumerable<string> strings = (IEnumerable<string>) source2.OrderBy<string, string>((Func<string, string>) (controlNum => controlNum));
        foreach (string str2 in strings)
          ;
        foreach (string str3 in strings)
          str1 = $"{str1}{str3}, ";
        foreach (ElementId elementId in elementIds)
          text1 = $"{text1}{elementId?.ToString()}; ";
        if (str1.Length > 0)
        {
          string text2 = $"CONTROL NUMBERS: {Environment.NewLine}{str1.Substring(0, str1.Length - 2)}";
          XYZ xyz = activeUiDocument.Selection.PickPoint("Pick the top left corner of the TextNote to be created.");
          TextNote.Create(document, document.ActiveView.Id, new XYZ(xyz.X + 0.25, xyz.Y, xyz.Z), 0.25, text2, new TextNoteOptions()
          {
            HorizontalAlignment = HorizontalTextAlignment.Right,
            TypeId = document.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType)
          }).get_Parameter(BuiltInParameter.TEXT_ALIGN_HORZ).Set(64 /*0x40*/);
          int num4 = (int) transaction.Commit();
          if (!text1.Equals(""))
          {
            if (new TaskDialog("Empty Values")
            {
              MainInstruction = "Warning: Some matching Elements did not have a CONTROL_NUMBER value. To find the Elements, hit the \"OK\" button below and go to Manage -> Inquiry -> Select by ID, press CTRL + V, and hit \"OK\".",
              CommonButtons = ((TaskDialogCommonButtons) 1),
              FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)"
            }.Show() == 1)
              Clipboard.SetText(text1);
          }
          return (Result) 0;
        }
        TaskDialog.Show("Control Numbers", "There are no matches for the input CONTROL_MARK value.");
        return (Result) 1;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("The user aborted the pick operation."))
          return (Result) 0;
        if (ex.ToString().Contains("View is not valid for detail line creation."))
        {
          TaskDialog.Show("Invalid View", "Invalid view for this tool.  This tool is meant to be run within an Assembly Sheet.");
          return (Result) 1;
        }
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
