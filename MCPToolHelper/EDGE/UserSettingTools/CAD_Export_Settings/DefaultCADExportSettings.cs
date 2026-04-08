// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.DefaultCADExportSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Windows.Media;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public static class DefaultCADExportSettings
{
  public static string CADExportFolderPath = "C:\\EDGEforRevit\\LaserExport";
  public static string InsulationExportFolderPath = "C:\\EDGEforRevit\\InsulationExport";
  public static string CADExportFileNameSuffix = "_Laser_file";
  public static string InsulationExportFileNamesSuffix = "_Insulation";
  public static bool CADExportDWF = true;
  public static bool CADExportDXF = false;
  public static bool InsulationExportDWG = true;
  public static bool InsulationExportDXF = false;
  public static bool TIFExportBoundingBox = true;
  public static bool TIFExportFixedCircle = false;
  public static bool BIFExportBoundingBox = true;
  public static bool BIFExportFixedCircle = false;
  public static bool UseEDGECADLines = false;
  public static double FixedCircleRadius = 5.0 / 12.0;
  public static double TickMarkLength = 0.25;
  public static Dictionary<string, ComboBoxExportColorItem> ColorDictionary = new Dictionary<string, ComboBoxExportColorItem>()
  {
    {
      "Magenta",
      new ComboBoxExportColorItem("Magenta", Color.FromRgb(byte.MaxValue, (byte) 0, byte.MaxValue), 6)
    },
    {
      "Yellow",
      new ComboBoxExportColorItem("Yellow", Color.FromRgb(byte.MaxValue, byte.MaxValue, (byte) 0), 2)
    },
    {
      "Orange",
      new ComboBoxExportColorItem("Orange", Color.FromRgb(byte.MaxValue, (byte) 177, (byte) 0), 30)
    },
    {
      "Red",
      new ComboBoxExportColorItem("Red", Color.FromRgb(byte.MaxValue, (byte) 0, (byte) 0), 1)
    },
    {
      "Green",
      new ComboBoxExportColorItem("Green", Color.FromRgb((byte) 0, byte.MaxValue, (byte) 0), 3)
    },
    {
      "Cyan",
      new ComboBoxExportColorItem("Cyan", Color.FromRgb((byte) 0, byte.MaxValue, byte.MaxValue), 4)
    },
    {
      "White",
      new ComboBoxExportColorItem("White", Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue), 7)
    },
    {
      "Blue",
      new ComboBoxExportColorItem("Blue", Color.FromRgb((byte) 0, (byte) 0, byte.MaxValue), 5)
    },
    {
      "Peach",
      new ComboBoxExportColorItem("Peach", Color.FromRgb(byte.MaxValue, (byte) 191, (byte) 170), 21)
    }
  };
  public static CADExportLayer TIFLayer = new CADExportLayer("ZE-TIF", DefaultCADExportSettings.ColorDictionary["Magenta"]);
  public static CADExportLayer BIFLayer = new CADExportLayer("ZC-BIF", DefaultCADExportSettings.ColorDictionary["Yellow"]);
  public static CADExportLayer SIFLayer = new CADExportLayer("ZD-SIF", DefaultCADExportSettings.ColorDictionary["Orange"]);
  public static CADExportLayer ArchLayer = new CADExportLayer("ZB-ARCH", DefaultCADExportSettings.ColorDictionary["Red"]);
  public static CADExportLayer FormLayer = new CADExportLayer("ZA-Form", DefaultCADExportSettings.ColorDictionary["Green"]);
  public static CADExportLayer CorbelLayer = new CADExportLayer("ZF-Corbel", DefaultCADExportSettings.ColorDictionary["Cyan"]);
  public static CADExportLayer StrandLayer = new CADExportLayer("ZG-Strand", DefaultCADExportSettings.ColorDictionary["White"]);
  public static CADExportLayer InsulationLayer = new CADExportLayer("YA-INSULATION", DefaultCADExportSettings.ColorDictionary["Blue"]);
  public static CADExportLayer CutoutsLayer = new CADExportLayer("Y0", DefaultCADExportSettings.ColorDictionary["White"]);
  public static CADExportLayer PinsLayer = new CADExportLayer("YB-PINS", DefaultCADExportSettings.ColorDictionary["Magenta"]);
  public static CADExportLayer SlotsLayer = new CADExportLayer("YC-SLOTS", DefaultCADExportSettings.ColorDictionary["Green"]);
  public static CADExportLayer ExtraTiesLayer = new CADExportLayer("YD-EXTRA TIES", DefaultCADExportSettings.ColorDictionary["Yellow"]);
  public static CADExportLayer MarkSymbolLayer = new CADExportLayer("A-ANNO-NOTE", DefaultCADExportSettings.ColorDictionary["Cyan"]);
  public static CADExportLayer MarkTextLayer = new CADExportLayer("annot", DefaultCADExportSettings.ColorDictionary["Peach"]);
  public static CADExportLayer disabledLayer = new CADExportLayer("(DISABLED)", DefaultCADExportSettings.ColorDictionary["White"]);
}
