// Decompiled with JetBrains decompiler
// Type: EDGE.AnnotationTools.DimensionAddAbove
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Windows.Forms;
using Utils.Forms;
using Utils.TextUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.AnnotationTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class DimensionAddAbove : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    UIApplication application = commandData.Application;
    string name = "Format Dimension: Add Text Above";
    string str = "Error:  There was an error adding the text above to the selected dimension.";
    if (Utils.ViewUtils.ViewUtils.CurrentViewIsProjectBrowser(document))
      return (Result) 1;
    using (Transaction transaction = new Transaction(document, name))
    {
      try
      {
        DataCollectorFormDimText collectorFormDimText = new DataCollectorFormDimText();
        collectorFormDimText.Text = "Dimension Text";
        collectorFormDimText.label.Text = "Enter a Text String to add to the selected dimension string(s) in the Above position. \n \nAn empty string will reset the Above text string.";
        if (collectorFormDimText.ShowDialog() == DialogResult.Cancel)
          return (Result) 1;
        string upper = collectorFormDimText.textBox1.Text.ToUpper();
        int num1 = 1;
        while (num1 == 1)
        {
          int num2 = (int) transaction.Start();
          DimensionSegmentUtils.AddDimensionText("Above", upper);
          int num3 = (int) transaction.Commit();
        }
        return (Result) 0;
      }
      catch (Exception ex)
      {
        if (ex.ToString().Contains("The user aborted the pick operation."))
          return (Result) 0;
        if (transaction.HasStarted())
        {
          int num = (int) transaction.RollBack();
        }
        message = str + Environment.NewLine + Environment.NewLine + ex?.ToString();
        return (Result) -1;
      }
    }
  }
}
