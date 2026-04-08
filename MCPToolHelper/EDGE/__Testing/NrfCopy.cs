// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.NrfCopy
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utils.Exceptions;
using Utils.Forms;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class NrfCopy : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    using (Transaction transaction = new Transaction(document, "Copy From Reference"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        ICollection<ElementId> elementIds = activeUiDocument.Selection.GetElementIds();
        if (!elementIds.Any<ElementId>())
          elementIds = References.PickNewReferences("Select the elements to copy");
        if (!elementIds.Any<ElementId>())
          return (Result) 1;
        XYZ xyz1 = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 51, "Select the base point");
        XYZ xyz2 = activeUiDocument.Selection.PickPoint((ObjectSnapTypes) 51, "Select the point to use as the \"From\" reference.");
        DataCollectorForm300x125 collectorForm300x125_1 = new DataCollectorForm300x125();
        collectorForm300x125_1.label.Text = "Enter the X, Y, Z coordinates, separated by a comma. Ex: 3,4,5 => (3, 4, 5)";
        DataCollectorForm300x125 collectorForm300x125_2 = collectorForm300x125_1;
        int num2 = (int) collectorForm300x125_2.ShowDialog();
        Regex regex = new Regex("^(?<sign>-?\\s?) ?(?<feet> (\\d *(\\.\\d+)?) (?! (\\d*\\.\\d+\" | \\d*\" | \\d+/\\d+\"?) ) ) ?(?:')?(?:\\s*-*\\s*)?(?<inch_whole>\\d* (?!\\d*/) ) ?(?<inch_part> ( (?<decimal>\\.\\d+) | (?<frac> (?:\\s?) (?<num>\\d+) / (?<denom>\\d+) ) ) ) ?(?:\")?$", RegexOptions.IgnorePatternWhitespace);
        double[] numArray = new double[3];
        string[] strArray = collectorForm300x125_2.textBox1.Text.Split(',');
        for (int index = 0; index < strArray.Length; ++index)
        {
          Match match = regex.Match(strArray[index].Trim());
          if (!match.Success)
            throw new EdgeException($"Error: input \"{strArray[index]}\" could not be interpreted.", (Exception) null);
          double result1;
          double.TryParse(match.Groups["sign"]?.ToString() + "1", out result1);
          double result2;
          double.TryParse(match.Groups["feet"].ToString(), out result2);
          double result3;
          double.TryParse(match.Groups["inch_whole"].ToString(), out result3);
          if (match.Groups["inch_part"].Success)
          {
            if (match.Groups["decimal"].Success)
            {
              Console.WriteLine($"'{match.Groups["decimal"]?.ToString()}'");
              double result4;
              double.TryParse("0" + match.Groups["decimal"]?.ToString(), out result4);
              result3 += result4;
            }
            else
            {
              double result5;
              double.TryParse(match.Groups["num"].ToString(), out result5);
              double result6;
              double.TryParse(match.Groups["denom"].ToString(), out result6);
              result3 += result5 / result6;
            }
          }
          numArray[index] = result1 * (result2 + result3 / 12.0);
        }
        XYZ xyz3 = new XYZ(numArray[0], numArray[1], numArray[2]);
        XYZ translation = xyz2 - xyz1 + xyz3;
        ElementTransformUtils.CopyElements(document, elementIds, translation);
        int num3 = (int) transaction.Commit();
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        switch (ex)
        {
          case Autodesk.Revit.Exceptions.OperationCanceledException _:
            return (Result) 0;
          case EdgeException _:
            message = ex.Message;
            return (Result) -1;
          default:
            message = ex.ToString();
            return (Result) -1;
        }
      }
    }
  }
}
