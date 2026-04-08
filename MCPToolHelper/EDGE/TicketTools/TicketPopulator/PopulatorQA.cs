// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.PopulatorQA
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Windows.Forms;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

internal class PopulatorQA
{
  public static bool inHouse;

  public static void DebugMessage(string message)
  {
    if (!PopulatorQA.inHouse)
      return;
    int num = (int) MessageBox.Show(message);
  }
}
