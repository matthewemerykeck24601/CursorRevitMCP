// Decompiled with JetBrains decompiler
// Type: EDGE.__Testing.DimensionSegmentTest
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.Forms;
using Utils.TextUtils;
using Utils.UIDocUtils;

#nullable disable
namespace EDGE.__Testing;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class DimensionSegmentTest : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    UIDocument activeUiDocument = commandData.Application.ActiveUIDocument;
    ActiveModel.GetInformation(activeUiDocument);
    Document document = activeUiDocument.Document;
    UIApplication application = commandData.Application;
    string str1 = "Format Dimension: Add Text Above";
    string str2 = "Error:  There was an error adding the text above to the selected dimension.";
    string name = str1;
    using (Transaction transaction = new Transaction(document, name))
    {
      try
      {
        DataCollectorForm300x150 collectorForm300x150 = new DataCollectorForm300x150();
        collectorForm300x150.Text = "Dimension Text";
        collectorForm300x150.label.Text = "Enter a Text String to add to the selected dimension string(s) in the Above position. \n \nAn empty string will reset the Above text string.";
        int num1 = (int) collectorForm300x150.ShowDialog();
        string upper = collectorForm300x150.textBox1.Text.ToUpper();
        int num2 = 1;
        while (num2 == 1)
        {
          int num3 = (int) transaction.Start();
          DimensionSegmentUtils.AddDimensionText("Above", upper);
          int num4 = (int) transaction.Commit();
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
        message = str2 + Environment.NewLine + Environment.NewLine + ex?.ToString();
        return (Result) -1;
      }
    }
  }
}
