// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.WarningSwallower
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

public class WarningSwallower : IFailuresPreprocessor
{
  public FailureProcessingResult PreprocessFailures(FailuresAccessor accessor)
  {
    foreach (FailureMessageAccessor failureMessage in (IEnumerable<FailureMessageAccessor>) accessor.GetFailureMessages())
    {
      ICollection<ElementId> failingElementIds = failureMessage.GetFailingElementIds();
      string path = "C:\\EDGEforREVIT\\ElementsNotCopied.txt";
      if (!File.Exists(path))
      {
        using (StreamWriter text = File.CreateText(path))
        {
          text.WriteLine("The following Annotration Elements(shown in ElementId) cannot be copied:");
          foreach (ElementId elementId in (IEnumerable<ElementId>) failingElementIds)
            text.WriteLine(elementId.ToString());
        }
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        using (StreamReader streamReader = File.OpenText(path))
        {
          string str;
          while ((str = streamReader.ReadLine()) != null)
            stringBuilder.Append(str);
        }
        using (StreamWriter streamWriter = File.AppendText(path))
        {
          foreach (ElementId elementId in (IEnumerable<ElementId>) failingElementIds)
          {
            if (!stringBuilder.ToString().Contains(elementId.ToString()))
              streamWriter.WriteLine(elementId.ToString());
          }
        }
      }
    }
    return FailureProcessingResult.Continue;
  }
}
