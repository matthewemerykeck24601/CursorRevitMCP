// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Insulation_Drawing_Settings.InsulationDrawingSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EDGE.TicketTools.TicketPopulator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.UserSettingTools.Insulation_Drawing_Settings;

public class InsulationDrawingSettings
{
  public string InsulationDetailLineStyleName;
  public string MarkCircleDetailLineStyleName;
  public string RecessCalloutsTextStyleName;
  public string InsulationMarkTextStyleName;
  public string OverallDimensionStyleName;
  public string GeneralDimensionStyleName;
  public string TitleBlockName;
  public int InsulationDrawingScaleFactorMaster = 32 /*0x20*/;
  public int InsulationDrawingScaleFactorPerPiece = 24;
  private bool fileReadSucess;

  public InsulationDrawingSettings(string manufacturer)
  {
    string fileName = "";
    if (!string.IsNullOrWhiteSpace(App.InsulationDrawingPath) && !string.IsNullOrWhiteSpace(manufacturer))
      fileName = $"{App.InsulationDrawingPath}\\{manufacturer}_Insulation_Drawing_Settings.xml";
    if (string.IsNullOrWhiteSpace(fileName))
      return;
    FileStream fileStream = (FileStream) null;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (InsulationDrawingSettings));
      FileInfo fileInfo = new FileInfo(fileName);
      if (!fileInfo.Exists)
        return;
      fileStream = fileInfo.OpenRead();
      InsulationDrawingSettings insulationDrawingSettings = (InsulationDrawingSettings) xmlSerializer.Deserialize((Stream) fileStream);
      if (insulationDrawingSettings == null || string.IsNullOrWhiteSpace(insulationDrawingSettings.InsulationDetailLineStyleName) || string.IsNullOrWhiteSpace(insulationDrawingSettings.MarkCircleDetailLineStyleName) || string.IsNullOrWhiteSpace(insulationDrawingSettings.RecessCalloutsTextStyleName) || string.IsNullOrWhiteSpace(insulationDrawingSettings.InsulationMarkTextStyleName) || string.IsNullOrWhiteSpace(insulationDrawingSettings.OverallDimensionStyleName) || string.IsNullOrWhiteSpace(insulationDrawingSettings.GeneralDimensionStyleName) || string.IsNullOrWhiteSpace(insulationDrawingSettings.TitleBlockName))
        return;
      this.InsulationDetailLineStyleName = insulationDrawingSettings.InsulationDetailLineStyleName;
      this.MarkCircleDetailLineStyleName = insulationDrawingSettings.MarkCircleDetailLineStyleName;
      this.RecessCalloutsTextStyleName = insulationDrawingSettings.RecessCalloutsTextStyleName;
      this.InsulationMarkTextStyleName = insulationDrawingSettings.InsulationMarkTextStyleName;
      this.OverallDimensionStyleName = insulationDrawingSettings.OverallDimensionStyleName;
      this.GeneralDimensionStyleName = insulationDrawingSettings.GeneralDimensionStyleName;
      this.TitleBlockName = insulationDrawingSettings.TitleBlockName;
      this.InsulationDrawingScaleFactorMaster = insulationDrawingSettings.InsulationDrawingScaleFactorMaster;
      this.InsulationDrawingScaleFactorPerPiece = insulationDrawingSettings.InsulationDrawingScaleFactorPerPiece;
      this.fileReadSucess = true;
    }
    catch (Exception ex)
    {
      if (!ex.Message.Contains("process"))
        return;
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "Insulation Drawing Settings File Error.",
        MainContent = $"Check Insulation Drawing Settings File: {fileName}. Please ensure the file is not in use by another application and try again."
      }.Show();
    }
    finally
    {
      fileStream?.Close();
    }
  }

  public InsulationDrawingSettings()
  {
  }

  public bool SaveFile(string manufacturer)
  {
    if (!string.IsNullOrWhiteSpace(App.InsulationDrawingPath) && !string.IsNullOrWhiteSpace(manufacturer))
      return this.SaveInsulationDrawingSettingsFile($"{App.InsulationDrawingPath}\\{manufacturer}_Insulation_Drawing_Settings.xml");
    TaskDialog.Show("Unable to Save Insulation Settings", "Please make sure that a file path has been saved in preferences and that a manufacturer is associated.");
    return false;
  }

  public int ConvertScaleStringToFactor(string scaleString, Document revitDoc)
  {
    int factor = 0;
    Dictionary<string, int> scalesDictionary = ScalesManager.GetScalesDictionary(ScalesManager.GetScaleUnitsForDocument(revitDoc));
    if (scalesDictionary.ContainsKey(scaleString))
      scalesDictionary.TryGetValue(scaleString, out factor);
    return factor;
  }

  private bool SaveInsulationDrawingSettingsFile(string filePath)
  {
    StreamWriter streamWriter1 = (StreamWriter) null;
    bool flag = false;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (InsulationDrawingSettings));
      DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
      if (!directoryInfo.Exists)
        directoryInfo.Create();
      FileInfo fileInfo = new FileInfo(filePath);
      if (fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
        flag = true;
      if (!fileInfo.Exists)
        fileInfo.Create()?.Close();
      streamWriter1 = new StreamWriter(filePath, false);
      StreamWriter streamWriter2 = streamWriter1;
      xmlSerializer.Serialize((TextWriter) streamWriter2, (object) this);
      streamWriter1?.Close();
    }
    catch (UnauthorizedAccessException ex)
    {
      if (flag)
      {
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Insulation Drawing Settings File is Read Only",
          MainContent = $"Unable to read the Insulation Drawing Settings File in the specified location {filePath} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
        }.Show();
        return false;
      }
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "Insulation Drawing Settings File Path Access Denied",
        MainContent = $"Unauthorized Access to save path: {filePath}.  Unable to save to this location.  Please ensure that you have write permission to this location or run Revit as administrator by right-clicking the Revit icon and choosing \"Run as Administrator\"{ex.Message}"
      }.Show();
      return false;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
      {
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Insulation Drawing Settings File Error.",
          MainContent = $"Check Insulation Drawing Settings File: {filePath}. Please ensure the file is not in use by another application and try again."
        }.Show();
        return false;
      }
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "Insulation Drawing Settings File Error.",
        MainContent = ("Unable to save template settings to path: " + filePath)
      }.Show();
      return false;
    }
    finally
    {
      streamWriter1?.Close();
    }
    return true;
  }

  public bool SettingsRead() => this.fileReadSucess;

  public InsulationDrawingSettingsObject GetSettings(Document revitDoc)
  {
    InsulationDrawingSettingsObject drawingSettingsObject = new InsulationDrawingSettingsObject(revitDoc, this);
    if (drawingSettingsObject == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.InsulationDetailLineStyle == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.MarkCircleDetailLineStyle == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.RecessCalloutsTextStyle == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.InsulationMarkTextStyle == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.OverallDimensionStyle == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.GeneralDimensionStyle == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.TitleBlockFamily == null)
      return (InsulationDrawingSettingsObject) null;
    if (drawingSettingsObject.InsulationDrawingScaleFactorMaster == 0)
      return (InsulationDrawingSettingsObject) null;
    return drawingSettingsObject.InsulationDrawingScaleFactorPerPiece == 0 ? (InsulationDrawingSettingsObject) null : drawingSettingsObject;
  }
}
