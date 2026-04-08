// Decompiled with JetBrains decompiler
// Type: EDGE.VisibilityTools.EditPrecastOpacity
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;

#nullable disable
namespace EDGE.VisibilityTools;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
internal class EditPrecastOpacity : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    int opacity = 100;
    if (VisibilityToggleFunctions.getPrecastOpacity(commandData, ref message, out opacity) == -1)
      opacity = 100;
    PrecastForm precastForm = new PrecastForm(opacity);
    int num = (int) precastForm.ShowDialog();
    if (precastForm.ParseOpacity() <= -1 || precastForm.DialogResult != DialogResult.OK)
      return (Result) 1;
    VisibilityToggleFunctions.SetPrecastOpacity(commandData, ref message, precastForm.ParseOpacity());
    return (Result) 0;
  }
}
