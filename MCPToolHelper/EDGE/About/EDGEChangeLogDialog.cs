// Decompiled with JetBrains decompiler
// Type: EDGE.About.EDGEChangeLogDialog
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using Utils.Forms;

#nullable disable
namespace EDGE.About;

public class EDGEChangeLogDialog
{
  public static void ChangeDialog()
  {
    ChangeLogForm changeLogForm = new ChangeLogForm();
    changeLogForm.lblVersionInformation.Text = "12.2.0";
    changeLogForm.lblBuildDate.Text = "2025.06.10";
    changeLogForm.ProductName = "EDGE ^ R";
    changeLogForm.lblChanges.Text = $"{Environment.NewLine}v.12.2.0{Environment.NewLine}-- BOM Product Hosting, Mark Verification Initial, and Mark Verification Existing performance enhancement{Environment.NewLine}{Environment.NewLine}v.12.1.0{Environment.NewLine}-- Add-on Hosting Updater bug fix{Environment.NewLine}{Environment.NewLine}v.12.0.0{Environment.NewLine}-- Enhancement of Laser Export for added customization and renamed to CAD Export{Environment.NewLine}-- Enhancement of Insulation Export for added customization{Environment.NewLine}-- Release of CAD Export Settings Tool for CAD Export and Insulation Export{Environment.NewLine}-- Release of Insulation Drawing Tools: Insulation Drawing - Master, Insulation Drawing - Assembly, Insulation Drawing - Mark{Environment.NewLine}-- Release of Insulation Drawing Settings Tool{Environment.NewLine}-- Release Erection Sequence Tool{Environment.NewLine}-- Enhancement of Control Number Incrementor for automatic and scope based assignment{Environment.NewLine}-- Enhancement of Mark Verification Existing and Mark Verification Initial to warn users if embeds have been ignored due to the MULTIPY_FOR_MARKS parameter{Environment.NewLine}-- Enhancement of Insulation Marking to use a percentage based tolerance for comparison{Environment.NewLine}-- Release of Freeze Active View Tool{Environment.NewLine}-- Enhancement to Top As Cast Tool and other tools that assign the LIF parameter to account for a family's orientation along with its location{Environment.NewLine}-- Clone Ticket bug fixes{Environment.NewLine}-- Fixes for minor bugs{Environment.NewLine}{Environment.NewLine}v.11.0.2{Environment.NewLine}-- Hardware Detail BOM Settings and HW Title Block Populator Settings folder path bug fix{Environment.NewLine}-- Clone Ticket bug fixes{Environment.NewLine}-- Assembly Creation performance enhancement{Environment.NewLine}{Environment.NewLine}v.11.0.1{Environment.NewLine}-- Clone Ticket optimization and bug fixes{Environment.NewLine}-- Hardware Detail Calculates Sub-Component Weight and populates the new HW_COMPONENT_WEIGHT parameter with value{Environment.NewLine}-- Assembly Creation Tools now requires new IS_FINISH parameter to add wall elements to assembly{Environment.NewLine}-- Hardware Detail Bug Fixes{Environment.NewLine}-- Project Shared Parameters Bug Fixes{Environment.NewLine}{Environment.NewLine}v.11.0.0{Environment.NewLine}-- Release of Clone Ticket tool{Environment.NewLine}-- Release of Hardware Detail tool{Environment.NewLine}-- Enhancement of ticketing tools to update new TKT_SHEET_COUNT parameter{Environment.NewLine}-- Enhancement to Mark Verification and Mark Prefix Settings to increment using letters{Environment.NewLine}-- Enhancement to Auto Ticket Generation to support ordinate dimensions{Environment.NewLine}-- Fixes for minor bugs{Environment.NewLine}{Environment.NewLine}v.10.0.4{Environment.NewLine}-- Added support for Revit 2024{Environment.NewLine}-- Updated Laser Export layers and other minor features{Environment.NewLine}-- Fixed incompatibility with PDM Export and other addins that use DataStorage elements{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";
    int num = (int) changeLogForm.ShowDialog();
  }
}
