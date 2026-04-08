// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.LineStyleManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System.Collections.Generic;

#nullable disable
namespace EDGE.AdminTools.CAM;

internal class LineStyleManager
{
  private LineStyleDetails[] _details;
  private Document _doc;

  public LineStyleManager(Document revitDoc, LineStyleDetails[] styleDetails)
  {
    this._doc = revitDoc;
    this._details = styleDetails;
  }

  public Dictionary<string, Category> SetupLineStyleCategories(out bool cancelled)
  {
    using (Transaction transaction = new Transaction(this._doc, "Edit Line Styles"))
    {
      cancelled = false;
      List<LineStyleDetails> lineStyleDetailsList = new List<LineStyleDetails>();
      Category category1 = Category.GetCategory(this._doc, BuiltInCategory.OST_Lines);
      Dictionary<string, Category> dictionary = new Dictionary<string, Category>();
      foreach (LineStyleDetails detail in this._details)
      {
        if (category1.SubCategories.Contains(detail._name))
        {
          Category category2 = category1.SubCategories.get_Item(detail._name);
          dictionary.Add(detail._name, category2);
        }
        else
          lineStyleDetailsList.Add(detail);
      }
      if (lineStyleDetailsList.Count > 0)
      {
        int num1 = (int) transaction.Start();
        foreach (LineStyleDetails lineStyleDetails in lineStyleDetailsList)
        {
          Category newLineStyle = Utils.SettingsUtils.Settings.CreateNewLineStyle(this._doc, lineStyleDetails._name, lineStyleDetails._color, lineStyleDetails._weight);
          if (newLineStyle != null)
            dictionary.Add(lineStyleDetails._name, newLineStyle);
        }
        int num2 = (int) transaction.Commit();
      }
      return dictionary;
    }
  }
}
