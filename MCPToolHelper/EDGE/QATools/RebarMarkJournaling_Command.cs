// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.RebarMarkJournaling_Command
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.ElementUtils;

#nullable disable
namespace EDGE.QATools;

[Regeneration(RegenerationOption.Manual)]
[Transaction(TransactionMode.Manual)]
internal class RebarMarkJournaling_Command : IExternalCommand
{
  private bool m_canReadData;
  private ExternalCommandData m_commandData;
  private Document m_revitDoc;

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    this.m_commandData = commandData;
    this.m_revitDoc = commandData.Application.ActiveUIDocument.Document;
    try
    {
      this.m_revitDoc = commandData.Application.ActiveUIDocument.Document;
      using (Transaction transaction = new Transaction(commandData.Application.ActiveUIDocument.Document, "Journaling"))
      {
        if (transaction.Start() != TransactionStatus.Started)
        {
          message = "EDGE: Unable to start transaction.";
          return (Result) -1;
        }
        IEnumerable<ElementId> elementIds = (IEnumerable<ElementId>) commandData.Application.ActiveUIDocument.Selection.GetElementIds();
        if (elementIds.Any<ElementId>())
        {
          if (elementIds.Count<ElementId>() > 1)
          {
            TaskDialog.Show("EDGE: Selection Error", "More than one element selected");
            return (Result) 1;
          }
          Element element = this.m_revitDoc.GetElement(elementIds.First<ElementId>());
          if (element.LookupParameter("CONTROL_MARK") == null || !Parameters.GetParameterAsString(element, "MANUFACTURE_COMPONENT").Contains("REBAR"))
          {
            TaskDialog.Show("EDGE: Selection Error", "Selected element either does not contain a control mark parameter or the manufacturer component does not contain the word REBAR");
            return (Result) 1;
          }
          this.m_canReadData = 0 < commandData.JournalData.Count;
          bool flag;
          if (this.m_canReadData)
          {
            flag = this.ReadJournalData(element);
          }
          else
          {
            this.WriteJournalData(element);
            flag = true;
          }
          if (transaction.Commit() != TransactionStatus.Committed)
          {
            message = "EDGE: Unable to commit transaction.";
            return (Result) -1;
          }
          return this.m_canReadData && !flag ? (Result) -1 : (Result) 0;
        }
      }
    }
    catch (Exception ex)
    {
      message = ex.Message;
      return (Result) -1;
    }
    return (Result) 0;
  }

  private bool ReadJournalData(Element newElement)
  {
    IDictionary<string, string> journalData = this.m_commandData.JournalData;
    if (journalData.ContainsKey("CONTROL_MARK"))
    {
      string str = journalData["CONTROL_MARK"];
      string parameterAsString = Parameters.GetParameterAsString(newElement, "CONTROL_MARK");
      if (str.CompareTo(parameterAsString) != 0)
      {
        TaskDialog.Show("EDGE: Mark Compare Error", $"Control marks failed to compare:  new mark is {parameterAsString} and we expected it to match the previous control mark {str}.  Needs investigation");
        this.m_commandData.Application.Application.WriteJournalComment("################################################################################################################", true);
        this.m_commandData.Application.Application.WriteJournalComment("####         EDGE ERROR:  Failed to compare control marks, previous and new do not match                      ##", true);
        this.m_commandData.Application.Application.WriteJournalComment($"####         previous = {str} and new = {parameterAsString}                          ##", true);
        this.m_commandData.Application.Application.WriteJournalComment("################################################################################################################", true);
        return false;
      }
      this.m_commandData.Application.Application.WriteJournalComment("################################################################################################################", true);
      this.m_commandData.Application.Application.WriteJournalComment("####         EDGE SUCCESS!  Control marks compare correctly                                                   ##", true);
      this.m_commandData.Application.Application.WriteJournalComment($"####         previous = {str} and new = {parameterAsString}                          ##", true);
      this.m_commandData.Application.Application.WriteJournalComment("################################################################################################################", true);
      return true;
    }
    this.m_commandData.Application.Application.WriteJournalComment("################################################################################################################", true);
    this.m_commandData.Application.Application.WriteJournalComment("####         EDGE ERROR:  Failed to compare control marks, data map does not                                  ##", true);
    this.m_commandData.Application.Application.WriteJournalComment("####         contain key CONTROL_MARK                                                                         ##", true);
    this.m_commandData.Application.Application.WriteJournalComment("################################################################################################################", true);
    return false;
  }

  private void WriteJournalData(Element elem)
  {
    IDictionary<string, string> journalData = this.m_commandData.JournalData;
    journalData.Clear();
    journalData.Add("CONTROL_MARK", Parameters.GetParameterAsString(elem, "CONTROL_MARK"));
  }
}
