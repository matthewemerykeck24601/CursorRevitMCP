// Decompiled with JetBrains decompiler
// Type: EDGE.About.AboutEdge
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Utils.Forms;

#nullable disable
namespace EDGE.About;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class AboutEdge : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    try
    {
      this.AboutDialog("EDGE^R 2024 12.2.0 (build date: 2025.06.10)");
      return (Result) 0;
    }
    catch (Exception ex)
    {
      message = ex.ToString();
      return (Result) -1;
    }
  }

  private void AboutDialog(string versionInfo)
  {
    string str1 = "EDGE^R copyright 2015-2025 EDGE Software";
    string str2 = $"A suite of tools and family content custom built for the Precast Concrete Industry.{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{$"Contact Information: {Environment.NewLine}  EDGE Software{Environment.NewLine}  28490 2nd Street{Environment.NewLine}  Daphne, Alabama 36526{Environment.NewLine}{Environment.NewLine}Phone: {Environment.NewLine}  1 (251) 340-2473{Environment.NewLine}{Environment.NewLine}Email: {Environment.NewLine}  Support:{Environment.NewLine}  edgesupport@ptac.com{Environment.NewLine}  Inquiries:{Environment.NewLine}  edge@ptac.com"}";
    AboutEdgeForm aboutEdgeForm = new AboutEdgeForm();
    aboutEdgeForm.lblDevelopmentInformation.Text = str2;
    aboutEdgeForm.lblCopyright.Text = str1;
    aboutEdgeForm.lblBuildDate.Text = "2025.06.10";
    aboutEdgeForm.lblVersionNo.Text = "12.2.0";
    int num = (int) aboutEdgeForm.ShowDialog();
  }
}
