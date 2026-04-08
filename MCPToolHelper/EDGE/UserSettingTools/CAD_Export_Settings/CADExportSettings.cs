// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.CADExportSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Utils.MiscUtils;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class CADExportSettings
{
  public XMLDictionary<int, CADExportLayer> ExportLayersDictionary = new XMLDictionary<int, CADExportLayer>();
  public List<int> CADExportLayers = new List<int>();
  public List<int> InsulationExportLayers = new List<int>();
  public int[] SelectedLayersIndexArray = new int[14]
  {
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1,
    -1
  };
  public string CADExportFolderPath;
  public string CADFileNameSuffix;
  public string InsulationExportFolderPath;
  public string InsulationFileNameSuffix;
  public bool CADExportDWGFiles;
  public bool CADExportDXFFiles;
  public bool InsulationExportDWGFiles;
  public bool InsulationExportDXFFiles;
  public bool TIFExportShape;
  public bool BIFExportShape;
  public bool UseEdgeCadLines;
  public double fixedCircleRadiusInternalUnits;
  public double tickMarkLengthInternalUnits;
  private int nextAvailableId;

  [XmlIgnore]
  public bool TIFExportBoundingBox
  {
    get => this.TIFExportShape;
    set => this.TIFExportShape = value;
  }

  [XmlIgnore]
  public bool TIFExportFixedCircle
  {
    get => !this.TIFExportShape;
    set => this.TIFExportShape = !value;
  }

  [XmlIgnore]
  public bool BIFExportBoundingBox
  {
    get => this.BIFExportShape;
    set => this.BIFExportShape = value;
  }

  [XmlIgnore]
  public bool BIFExportFixedCircle
  {
    get => !this.BIFExportShape;
    set => this.BIFExportShape = !value;
  }

  [XmlIgnore]
  public bool TIFLayerEnabled => this.SelectedLayersIndexArray[0] != -1;

  [XmlIgnore]
  public bool BIFLayerEnabled => this.SelectedLayersIndexArray[1] != -1;

  [XmlIgnore]
  public bool SIFLayerEnabled => this.SelectedLayersIndexArray[2] != -1;

  [XmlIgnore]
  public bool FormLayerEnabled => this.SelectedLayersIndexArray[3] != -1;

  [XmlIgnore]
  public bool CorbelLayerEnabled => this.SelectedLayersIndexArray[4] != -1;

  [XmlIgnore]
  public bool ArchLayerEnabled => this.SelectedLayersIndexArray[5] != -1;

  [XmlIgnore]
  public bool StrandLayerEnabled => this.SelectedLayersIndexArray[6] != -1;

  [XmlIgnore]
  public bool InsulationLayerEnabled => this.SelectedLayersIndexArray[7] != -1;

  [XmlIgnore]
  public bool CutoutLayerEnabled => this.SelectedLayersIndexArray[8] != -1;

  [XmlIgnore]
  public bool PinsLayerEnabled => this.SelectedLayersIndexArray[9] != -1;

  [XmlIgnore]
  public bool SlotsLayerEnabled => this.SelectedLayersIndexArray[10] != -1;

  [XmlIgnore]
  public bool ExtraTiesLayerEnabled => this.SelectedLayersIndexArray[11] != -1;

  [XmlIgnore]
  public bool MarkSymbolLayerEnabled => this.SelectedLayersIndexArray[12] != -1;

  [XmlIgnore]
  public bool MarkTextLayerEnabled => this.SelectedLayersIndexArray[13] != -1;

  [XmlIgnore]
  public double fixedCircleRadiusInches
  {
    get
    {
      return this.fixedCircleRadiusInternalUnits >= 0.0 ? Math.Round(UnitUtils.ConvertFromInternalUnits(this.fixedCircleRadiusInternalUnits, UnitTypeId.Inches), 10) : 0.0;
    }
    set
    {
      if (value > 0.0)
        this.fixedCircleRadiusInternalUnits = UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Inches);
      else
        this.fixedCircleRadiusInternalUnits = 0.0;
    }
  }

  [XmlIgnore]
  public double fixedCircleRadiusMillimeter
  {
    get
    {
      return this.fixedCircleRadiusInternalUnits >= 0.0 ? Math.Round(UnitUtils.ConvertFromInternalUnits(this.fixedCircleRadiusInternalUnits, UnitTypeId.Millimeters), 10) : 0.0;
    }
    set
    {
      if (value > 0.0)
        this.fixedCircleRadiusInternalUnits = UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
      else
        this.fixedCircleRadiusInternalUnits = 0.0;
    }
  }

  [XmlIgnore]
  public double tickMarkLengthInches
  {
    get
    {
      return this.tickMarkLengthInternalUnits >= 0.0 ? Math.Round(UnitUtils.ConvertFromInternalUnits(this.tickMarkLengthInternalUnits, UnitTypeId.Inches), 10) : 0.0;
    }
    set
    {
      if (value > 0.0)
        this.tickMarkLengthInternalUnits = UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Inches);
      else
        this.tickMarkLengthInternalUnits = 0.0;
    }
  }

  [XmlIgnore]
  public double tickMarkLengthMillimeter
  {
    get
    {
      return this.tickMarkLengthInternalUnits >= 0.0 ? Math.Round(UnitUtils.ConvertFromInternalUnits(this.tickMarkLengthInternalUnits, UnitTypeId.Millimeters), 10) : 0.0;
    }
    set
    {
      if (value > 0.0)
        this.tickMarkLengthInternalUnits = UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
      else
        this.tickMarkLengthInternalUnits = 0.0;
    }
  }

  public CADExportSettings()
  {
  }

  public CADExportSettings(bool defaults)
  {
    if (!defaults)
      return;
    this.SetDefaultSettings();
  }

  public bool SaveCADExportSettings(string manufacturer)
  {
    return !string.IsNullOrWhiteSpace(App.CADExFolderPath) && !string.IsNullOrWhiteSpace(manufacturer) && this.SaveCadExportSettingsFile($"{App.CADExFolderPath}\\{manufacturer}_CAD_Settings.xml");
  }

  private bool SaveCadExportSettingsFile(string filePath)
  {
    StreamWriter streamWriter1 = (StreamWriter) null;
    bool flag = false;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (CADExportSettings));
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
          MainInstruction = "CAD Export Settings File is Read Only",
          MainContent = $"Unable to read the CAD Export Settings File in the specified location {filePath} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
        }.Show();
        return false;
      }
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "CAD Export Settings File Path Access Denied",
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
          MainInstruction = "CAD Export Settings File Error.",
          MainContent = $"Check CAD Export Settings File: {filePath}. Please ensure the file is not in use by another application and try again."
        }.Show();
        return false;
      }
      new TaskDialog("EDGE^R")
      {
        AllowCancellation = false,
        MainInstruction = "CAD Export Settings File Error.",
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

  public bool GetManufacturerSettings(string manufacturer = "")
  {
    return !string.IsNullOrWhiteSpace(App.CADExFolderPath) && !string.IsNullOrWhiteSpace(manufacturer) && this.ReadSettings($"{App.CADExFolderPath}\\{manufacturer}_CAD_Settings.xml");
  }

  private bool ReadSettings(string filePath)
  {
    FileStream fileStream = (FileStream) null;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (CADExportSettings));
      FileInfo fileInfo = new FileInfo(filePath);
      if (!fileInfo.Exists)
        return false;
      fileStream = fileInfo.OpenRead();
      CADExportSettings cadExportSettings = (CADExportSettings) xmlSerializer.Deserialize((Stream) fileStream);
      if (cadExportSettings == null)
        return false;
      this.ExportLayersDictionary.Clear();
      if (cadExportSettings.ExportLayersDictionary != null)
      {
        foreach (int key in cadExportSettings.ExportLayersDictionary.Keys)
        {
          if (!this.ExportLayersDictionary.ContainsKey(key))
          {
            cadExportSettings.ExportLayersDictionary[key].DeleteEnabled = true;
            this.ExportLayersDictionary.Add(key, cadExportSettings.ExportLayersDictionary[key]);
          }
        }
      }
      this.CADExportLayers.Clear();
      if (cadExportSettings.CADExportLayers != null)
      {
        foreach (int cadExportLayer in cadExportSettings.CADExportLayers)
          this.CADExportLayers.Add(cadExportLayer);
      }
      this.InsulationExportLayers.Clear();
      if (cadExportSettings.InsulationExportLayers != null)
      {
        foreach (int insulationExportLayer in cadExportSettings.InsulationExportLayers)
          this.InsulationExportLayers.Add(insulationExportLayer);
      }
      if (cadExportSettings.InsulationExportLayers != null)
      {
        for (int index = 0; index < this.SelectedLayersIndexArray.Length; ++index)
          this.SelectedLayersIndexArray[index] = cadExportSettings.SelectedLayersIndexArray[index];
      }
      if (!string.IsNullOrWhiteSpace(cadExportSettings.CADExportFolderPath))
        this.CADExportFolderPath = cadExportSettings.CADExportFolderPath;
      if (!string.IsNullOrWhiteSpace(cadExportSettings.CADFileNameSuffix))
        this.CADFileNameSuffix = cadExportSettings.CADFileNameSuffix;
      if (!string.IsNullOrWhiteSpace(cadExportSettings.InsulationExportFolderPath))
        this.InsulationExportFolderPath = cadExportSettings.InsulationExportFolderPath;
      if (!string.IsNullOrEmpty(cadExportSettings.InsulationFileNameSuffix))
        this.InsulationFileNameSuffix = cadExportSettings.InsulationFileNameSuffix;
      this.CADExportDWGFiles = cadExportSettings.CADExportDWGFiles;
      this.CADExportDXFFiles = cadExportSettings.CADExportDXFFiles;
      this.InsulationExportDWGFiles = cadExportSettings.InsulationExportDWGFiles;
      this.InsulationExportDXFFiles = cadExportSettings.InsulationExportDXFFiles;
      this.TIFExportShape = cadExportSettings.TIFExportShape;
      this.BIFExportShape = cadExportSettings.BIFExportShape;
      this.UseEdgeCadLines = cadExportSettings.UseEdgeCadLines;
      this.fixedCircleRadiusInternalUnits = cadExportSettings.fixedCircleRadiusInternalUnits;
      this.tickMarkLengthInternalUnits = cadExportSettings.tickMarkLengthInternalUnits;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "CAD Export Settings File Error.",
          MainContent = $"Check CAD Export Settings File: {filePath}. Please ensure the file is not in use by another application and try again."
        }.Show();
      return false;
    }
    finally
    {
      fileStream?.Close();
    }
    return true;
  }

  private void SetDefaultSettings()
  {
    this.SetCADExportDefaultSettings();
    this.SetInsulationExportDefaultSettings();
  }

  private void SetCADExportDefaultSettings()
  {
    this.CADExportFolderPath = DefaultCADExportSettings.CADExportFolderPath;
    this.CADFileNameSuffix = DefaultCADExportSettings.CADExportFileNameSuffix;
    this.CADExportDWGFiles = DefaultCADExportSettings.CADExportDWF;
    this.CADExportDXFFiles = DefaultCADExportSettings.CADExportDXF;
    this.TIFExportBoundingBox = DefaultCADExportSettings.TIFExportBoundingBox;
    this.BIFExportBoundingBox = DefaultCADExportSettings.BIFExportBoundingBox;
    this.fixedCircleRadiusInternalUnits = DefaultCADExportSettings.FixedCircleRadius;
    this.tickMarkLengthInternalUnits = DefaultCADExportSettings.TickMarkLength;
    this.UseEdgeCadLines = DefaultCADExportSettings.UseEDGECADLines;
    this.ClearCADExportLayers();
    this.setExportGroupLayer(CADExportSettings.ExportGroup.TIF, DefaultCADExportSettings.TIFLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.BIF, DefaultCADExportSettings.BIFLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.SIF, DefaultCADExportSettings.SIFLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Form, DefaultCADExportSettings.FormLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Corbel, DefaultCADExportSettings.CorbelLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Arch, DefaultCADExportSettings.ArchLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Strand, DefaultCADExportSettings.StrandLayer.Clone());
  }

  private void SetInsulationExportDefaultSettings()
  {
    this.InsulationExportFolderPath = DefaultCADExportSettings.InsulationExportFolderPath;
    this.InsulationFileNameSuffix = DefaultCADExportSettings.InsulationExportFileNamesSuffix;
    this.InsulationExportDWGFiles = DefaultCADExportSettings.InsulationExportDWG;
    this.InsulationExportDXFFiles = DefaultCADExportSettings.InsulationExportDXF;
    this.ClearInsulationExportLayers();
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Insulation, DefaultCADExportSettings.InsulationLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Cutout, DefaultCADExportSettings.CutoutsLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Pins, DefaultCADExportSettings.PinsLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.Slots, DefaultCADExportSettings.SlotsLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.ExtraTies, DefaultCADExportSettings.ExtraTiesLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.MarkSymbol, DefaultCADExportSettings.MarkSymbolLayer.Clone());
    this.setExportGroupLayer(CADExportSettings.ExportGroup.MarkText, DefaultCADExportSettings.MarkTextLayer.Clone());
  }

  private void AddLayer(CADExportLayer layer, out int id)
  {
    id = -1;
    if (layer == null)
      return;
    if (this.ExportLayersDictionary.ContainsValue(layer))
    {
      foreach (int key in this.ExportLayersDictionary.Keys)
      {
        if (this.ExportLayersDictionary[key] != null && this.ExportLayersDictionary[key].Equals(layer))
        {
          id = key;
          break;
        }
      }
    }
    else
    {
      id = this.nextAvailableId;
      while (this.ExportLayersDictionary.ContainsKey(id))
        ++id;
      this.ExportLayersDictionary.Add(id, layer);
      this.nextAvailableId = id + 1;
      while (this.ExportLayersDictionary.ContainsKey(this.nextAvailableId))
        ++this.nextAvailableId;
    }
  }

  public bool AddCADExportLayer(CADExportLayer layer)
  {
    int id = -1;
    return this.AddCADExportLayer(layer, out id);
  }

  private bool AddCADExportLayer(CADExportLayer layer, out int id)
  {
    id = -1;
    if (layer == null)
      return false;
    this.AddLayer(layer, out id);
    if (id == -1)
      return false;
    if (!this.CADExportLayers.Contains(id))
      this.CADExportLayers.Add(id);
    return true;
  }

  public bool AddInsulationExportLayer(CADExportLayer layer)
  {
    int id = -1;
    return this.AddInsulationExportLayer(layer, out id);
  }

  private bool AddInsulationExportLayer(CADExportLayer layer, out int id)
  {
    id = -1;
    if (layer == null)
      return false;
    this.AddLayer(layer, out id);
    if (id == -1)
      return false;
    if (!this.InsulationExportLayers.Contains(id))
      this.InsulationExportLayers.Add(id);
    return true;
  }

  public void setExportGroupLayer(CADExportSettings.ExportGroup exportGroup, CADExportLayer layer)
  {
    if (layer == null)
      return;
    int index = (int) exportGroup;
    int id = -1;
    if (index < 7)
      this.AddCADExportLayer(layer, out id);
    else
      this.AddInsulationExportLayer(layer, out id);
    this.SelectedLayersIndexArray[index] = id;
  }

  public void DisableExportGroupLayer(CADExportSettings.ExportGroup group)
  {
    this.SelectedLayersIndexArray[(int) group] = -1;
  }

  public List<CADExportLayer> GetCADExportLayers()
  {
    List<CADExportLayer> cadExportLayers = new List<CADExportLayer>();
    List<int> intList = new List<int>();
    foreach (int cadExportLayer in this.CADExportLayers)
    {
      if (this.ExportLayersDictionary.ContainsKey(cadExportLayer))
      {
        CADExportLayer exportLayers = this.ExportLayersDictionary[cadExportLayer];
        if (exportLayers != null && !cadExportLayers.Contains(exportLayers))
          cadExportLayers.Add(exportLayers);
      }
    }
    return cadExportLayers;
  }

  public List<CADExportLayer> GetInsulationExportLayers()
  {
    List<CADExportLayer> insulationExportLayers = new List<CADExportLayer>();
    List<int> intList = new List<int>();
    foreach (int insulationExportLayer in this.InsulationExportLayers)
    {
      if (this.ExportLayersDictionary.ContainsKey(insulationExportLayer))
      {
        CADExportLayer exportLayers = this.ExportLayersDictionary[insulationExportLayer];
        if (exportLayers != null && !insulationExportLayers.Contains(exportLayers))
          insulationExportLayers.Add(exportLayers);
      }
    }
    return insulationExportLayers;
  }

  public CADExportLayer GetSelectedLayer(CADExportSettings.ExportGroup group)
  {
    int index = (int) group;
    CADExportLayer selectedLayer = (CADExportLayer) null;
    if (this.ExportLayersDictionary.ContainsKey(this.SelectedLayersIndexArray[index]))
      selectedLayer = this.ExportLayersDictionary[this.SelectedLayersIndexArray[index]];
    return selectedLayer;
  }

  public void ClearCADExportLayers()
  {
    foreach (int key in this.CADExportLayers.ToList<int>())
    {
      if (!this.InsulationExportLayers.Contains(key))
      {
        this.ExportLayersDictionary.Remove(key);
        if (key < this.nextAvailableId)
          this.nextAvailableId = key;
      }
    }
    this.CADExportLayers.Clear();
  }

  public void ClearInsulationExportLayers()
  {
    foreach (int num in this.InsulationExportLayers.ToList<int>())
    {
      if (!this.CADExportLayers.Contains(num))
      {
        this.InsulationExportLayers.Remove(num);
        if (num < this.nextAvailableId)
          this.nextAvailableId = num;
      }
    }
    this.InsulationExportLayers.Clear();
  }

  public void ClearLayerDictionary() => this.ExportLayersDictionary.Clear();

  public void ClearExportGroupLayers()
  {
    for (int index = 0; index < this.SelectedLayersIndexArray.Length; ++index)
      this.SelectedLayersIndexArray[index] = -1;
  }

  public void ClearLayerSettings()
  {
    this.ClearCADExportLayers();
    this.ClearInsulationExportLayers();
    this.ClearLayerDictionary();
    this.ClearExportGroupLayers();
  }

  public CADExportSettings Clone()
  {
    CADExportSettings cadExportSettings = new CADExportSettings(false);
    foreach (int key in this.ExportLayersDictionary.Keys)
      cadExportSettings.ExportLayersDictionary.Add(key, new CADExportLayer(this.ExportLayersDictionary[key].LayerName, this.ExportLayersDictionary[key].LayerColor));
    foreach (int cadExportLayer in this.CADExportLayers)
      cadExportSettings.CADExportLayers.Add(cadExportLayer);
    foreach (int insulationExportLayer in this.InsulationExportLayers)
      cadExportSettings.InsulationExportLayers.Add(insulationExportLayer);
    for (int index = 0; index < ((IEnumerable<int>) this.SelectedLayersIndexArray).Count<int>(); ++index)
      cadExportSettings.SelectedLayersIndexArray[index] = this.SelectedLayersIndexArray[index];
    cadExportSettings.CADExportFolderPath = this.CADExportFolderPath;
    cadExportSettings.InsulationExportFolderPath = this.InsulationExportFolderPath;
    cadExportSettings.CADFileNameSuffix = this.CADFileNameSuffix;
    cadExportSettings.InsulationFileNameSuffix = this.InsulationFileNameSuffix;
    cadExportSettings.CADExportDWGFiles = this.CADExportDWGFiles;
    cadExportSettings.CADExportDXFFiles = this.CADExportDXFFiles;
    cadExportSettings.InsulationExportDWGFiles = this.InsulationExportDWGFiles;
    cadExportSettings.InsulationExportDXFFiles = this.InsulationExportDXFFiles;
    cadExportSettings.TIFExportShape = this.TIFExportShape;
    cadExportSettings.BIFExportShape = this.BIFExportShape;
    cadExportSettings.fixedCircleRadiusInternalUnits = this.fixedCircleRadiusInternalUnits;
    cadExportSettings.tickMarkLengthInternalUnits = this.tickMarkLengthInternalUnits;
    cadExportSettings.UseEdgeCadLines = this.UseEdgeCadLines;
    return cadExportSettings;
  }

  public override bool Equals(object obj) => base.Equals((object) (obj as CADExportSettings));

  public bool Equals(CADExportSettings other) => this.Equals(other, out List<string> _);

  public bool Equals(CADExportSettings other, out List<string> fieldsChanged, bool needFields = false)
  {
    fieldsChanged = new List<string>();
    if (other == null)
      return false;
    bool flag = false;
    string str1 = "CAD Export Settings - ";
    string str2 = "Insulation Export Settings - ";
    if (this.CADExportFolderPath != other.CADExportFolderPath)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Export Folder Path");
    }
    if (this.InsulationExportFolderPath != other.InsulationExportFolderPath)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Export Folder Path");
    }
    if (this.CADFileNameSuffix != other.CADFileNameSuffix)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "File Name Suffix");
    }
    if (this.InsulationFileNameSuffix != other.InsulationFileNameSuffix)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "File Name Suffix");
    }
    if (this.CADExportDWGFiles != other.CADExportDWGFiles || this.CADExportDXFFiles != other.CADExportDXFFiles)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "File Type Exported");
    }
    if (this.InsulationExportDWGFiles != other.InsulationExportDWGFiles || this.InsulationExportDXFFiles != other.InsulationExportDXFFiles)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "File Type Exported");
    }
    if (this.TIFExportShape != other.TIFExportShape)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "TIF Layer Export Shape");
    }
    if (this.BIFExportShape != other.BIFExportShape)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "BIF Layer Export Shape");
    }
    if (!this.fixedCircleRadiusInternalUnits.ApproximatelyEquals(other.fixedCircleRadiusInternalUnits))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Fixed Circle Radius");
    }
    if (!this.tickMarkLengthInternalUnits.ApproximatelyEquals(other.tickMarkLengthInternalUnits))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Tick Mark Length");
    }
    if (this.UseEdgeCadLines != other.UseEdgeCadLines)
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "EDGE CAD Lines Settings");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.TIF, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "TIF Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.BIF, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "BIF Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.SIF, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "SIF Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Form, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Form Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Corbel, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Corbel Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Arch, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Arch Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Strand, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str1 + "Strand Elements Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Insulation, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Insulation Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Cutout, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Cutouts and Recesses Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Pins, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Pins Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.Slots, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Slots Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.ExtraTies, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Extra Ties Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.MarkSymbol, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Mark Symbol Layer");
    }
    if (!this.CompareSelectedLayer(CADExportSettings.ExportGroup.MarkText, other))
    {
      if (!needFields)
        return false;
      flag = true;
      fieldsChanged.Add(str2 + "Mark Text Layer");
    }
    if (!this.ExportLayersDictionary.Values.OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName)).SequenceEqual<CADExportLayer>((IEnumerable<CADExportLayer>) other.ExportLayersDictionary.Values.OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName))))
    {
      if (!needFields)
        return false;
      flag = true;
      if (!this.GetCADExportLayers().OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName)).SequenceEqual<CADExportLayer>((IEnumerable<CADExportLayer>) other.GetCADExportLayers().OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName))))
        fieldsChanged.Add(str1 + "Layer Setup");
      if (!this.GetInsulationExportLayers().OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName)).SequenceEqual<CADExportLayer>((IEnumerable<CADExportLayer>) other.GetInsulationExportLayers().OrderBy<CADExportLayer, string>((Func<CADExportLayer, string>) (x => x.LayerName))))
        fieldsChanged.Add(str2 + "Layer Setup");
    }
    return !flag;
  }

  private bool CompareSelectedLayer(CADExportSettings.ExportGroup group, CADExportSettings other)
  {
    CADExportLayer selectedLayer1 = this.GetSelectedLayer(group);
    CADExportLayer selectedLayer2 = other.GetSelectedLayer(group);
    if (selectedLayer1 == null && selectedLayer2 == null)
      return true;
    return selectedLayer1 != null && selectedLayer2 != null && selectedLayer1.Equals(selectedLayer2);
  }

  public List<string> GetFullFilePaths(
    string mark,
    string projectNumber,
    string ProjectName,
    bool? CADExport = null,
    bool? DWG = null,
    bool? DXF = null,
    string folderPath = null,
    string fileSuffix = null)
  {
    List<string> fullFilePaths = new List<string>();
    if (!CADExport.HasValue || CADExport.GetValueOrDefault())
    {
      string str1 = folderPath ?? this.CADExportFolderPath;
      string str2 = fileSuffix ?? this.CADFileNameSuffix;
      string str3 = $"{str1}/{projectNumber} - {ProjectName}/{projectNumber}-{mark}{str2}";
      if (DWG.GetValueOrDefault() || !DWG.GetValueOrDefault() && this.CADExportDWGFiles)
        fullFilePaths.Add(str3 + ".dwg");
      if (DXF.GetValueOrDefault() || !DXF.GetValueOrDefault() && this.CADExportDXFFiles)
        fullFilePaths.Add(str3 + ".dxf");
    }
    if (CADExport.HasValue)
    {
      bool? nullable = CADExport;
      bool flag = false;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        goto label_11;
    }
    string str4 = folderPath ?? this.InsulationExportFolderPath;
    string str5 = fileSuffix ?? this.InsulationFileNameSuffix;
    string str6 = $"{str4}/{projectNumber} - {ProjectName}/{projectNumber}-{mark}{str5}";
    if (DWG.GetValueOrDefault() || !DWG.GetValueOrDefault() && this.InsulationExportDWGFiles)
      fullFilePaths.Add(str6 + ".dwg");
    if (DXF.GetValueOrDefault() || !DXF.GetValueOrDefault() && this.InsulationExportDXFFiles)
      fullFilePaths.Add(str6 + ".dxf");
label_11:
    return fullFilePaths;
  }

  public string GetShortFilePath(string projectName, string projectNumber, bool CADExport)
  {
    return CADExport ? $"{this.CADExportFolderPath}\\{projectNumber} - {projectName}\\" : $"{this.InsulationExportFolderPath}\\{projectNumber} - {projectName}\\";
  }

  public string GetFileName(string mark, string projectNumber, bool CADExport)
  {
    return CADExport ? $"{projectNumber}-{mark}{this.CADFileNameSuffix}" : $"{projectNumber}-{mark}{this.InsulationFileNameSuffix}";
  }

  public string GetFullFilePath(
    string mark,
    string projectName,
    string projectNumber,
    bool CADExport)
  {
    return this.GetShortFilePath(projectName, projectNumber, CADExport) + this.GetFileName(mark, projectNumber, CADExport);
  }

  public enum ExportGroup
  {
    TIF,
    BIF,
    SIF,
    Form,
    Corbel,
    Arch,
    Strand,
    Insulation,
    Cutout,
    Pins,
    Slots,
    ExtraTies,
    MarkSymbol,
    MarkText,
  }
}
