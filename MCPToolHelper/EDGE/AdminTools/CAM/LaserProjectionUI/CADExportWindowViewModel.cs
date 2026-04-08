// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.LaserProjectionUI.CADExportWindowViewModel
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.UserSettingTools.CAD_Export_Settings;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

#nullable disable
namespace EDGE.AdminTools.CAM.LaserProjectionUI;

public class CADExportWindowViewModel : INotifyPropertyChanged
{
  private string _exportFolderPathString;
  private string _fileNameSuffixString;
  private bool _dwgChecked;
  private bool _dxfChecked;
  private string _filePathCore = "#### - Project\\####-MKXX";
  private bool CADExportTool;
  private CADExportSettings CADExportSettings;
  private List<string> ElementMarks;
  private string projectNumber;
  private string projectName;

  public string ExportFolderPathString
  {
    get => this._exportFolderPathString;
    set
    {
      if (!(value.Trim() != this._exportFolderPathString))
        return;
      this._exportFolderPathString = value.Trim();
      this.NotifyPropertyChanged(nameof (ExportFolderPathString));
      this.NotifyPropertyChanged("ExampleFilePathString");
      this.NotifyPropertyChanged("OverwriteWarning");
    }
  }

  public string FileNameSuffixString
  {
    get => this._fileNameSuffixString;
    set
    {
      if (!(value != this._fileNameSuffixString))
        return;
      this._fileNameSuffixString = value;
      this.NotifyPropertyChanged(nameof (FileNameSuffixString));
      this.NotifyPropertyChanged("ExampleFilePathString");
      this.NotifyPropertyChanged("OverwriteWarning");
    }
  }

  public bool DWGChecked
  {
    get => this._dwgChecked;
    set
    {
      if (this._dwgChecked == value)
        return;
      this._dwgChecked = value;
      this.NotifyPropertyChanged(nameof (DWGChecked));
      this.NotifyPropertyChanged("BothUnchecked");
      this.NotifyPropertyChanged("ExampleFilePathString");
      this.NotifyPropertyChanged("OverwriteWarning");
    }
  }

  public bool DXFChecked
  {
    get => this._dxfChecked;
    set
    {
      if (this._dxfChecked == value)
        return;
      this._dxfChecked = value;
      this.NotifyPropertyChanged(nameof (DXFChecked));
      this.NotifyPropertyChanged("BothUnchecked");
      this.NotifyPropertyChanged("ExampleFilePathString");
      this.NotifyPropertyChanged("OverwriteWarning");
    }
  }

  public bool BothUnchecked => !this._dwgChecked && !this._dxfChecked;

  private string _fileExtensionExample => !this.DWGChecked && this.DXFChecked ? ".dxf" : ".dwg";

  public string ExampleFilePathString
  {
    get
    {
      return $"{this._exportFolderPathString}\\{this._filePathCore}{this._fileNameSuffixString}{this._fileExtensionExample}";
    }
  }

  public bool OverwriteWarning
  {
    get
    {
      if (this.ElementMarks == null || this.CADExportSettings == null || this.projectNumber == null || this.projectName == null)
        return false;
      List<string> stringList = new List<string>();
      foreach (string elementMark in this.ElementMarks)
        stringList.AddRange((IEnumerable<string>) this.CADExportSettings.GetFullFilePaths(elementMark, this.projectNumber, this.projectName, new bool?(this.CADExportTool), new bool?(this._dwgChecked), new bool?(this._dxfChecked), this._exportFolderPathString, this._fileNameSuffixString));
      foreach (string path in stringList)
      {
        if (File.Exists(path))
          return true;
      }
      return false;
    }
  }

  public CADExportWindowViewModel(
    CADExportSettings settings,
    bool CADExportTool,
    List<string> ElementsToExport,
    string projectNumber,
    string projectName,
    string manufacturer)
  {
    this.CADExportTool = CADExportTool;
    this.CADExportSettings = settings;
    this.ElementMarks = ElementsToExport.ToList<string>();
    this.projectName = projectName;
    this.projectNumber = projectNumber;
    this._filePathCore = $"{projectNumber} - {projectName}\\{projectNumber}-{ElementsToExport.FirstOrDefault<string>()}";
    if (CADExportTool)
    {
      this.ExportFolderPathString = !App.CADExportFolderPathDictionary.ContainsKey(manufacturer) ? this.CADExportSettings.CADExportFolderPath : App.CADExportFolderPathDictionary[manufacturer];
      this.FileNameSuffixString = !App.CADExportFileSuffixDictionary.ContainsKey(manufacturer) ? this.CADExportSettings.CADFileNameSuffix : App.CADExportFileSuffixDictionary[manufacturer];
      this.DWGChecked = !App.CADExportDWGDictionary.ContainsKey(manufacturer) ? this.CADExportSettings.CADExportDWGFiles : App.CADExportDWGDictionary[manufacturer];
      if (App.CADExportDXFDictionary.ContainsKey(manufacturer))
        this.DXFChecked = App.CADExportDXFDictionary[manufacturer];
      else
        this.DXFChecked = this.CADExportSettings.CADExportDXFFiles;
    }
    else
    {
      this.ExportFolderPathString = !App.InsulationExportFolderPathDictionary.ContainsKey(manufacturer) ? this.CADExportSettings.InsulationExportFolderPath : App.InsulationExportFolderPathDictionary[manufacturer];
      this.FileNameSuffixString = !App.InsulationExportFileSuffixDictionary.ContainsKey(manufacturer) ? this.CADExportSettings.InsulationFileNameSuffix : App.InsulationExportFileSuffixDictionary[manufacturer];
      this.DWGChecked = !App.InsulationExportDWGDictionary.ContainsKey(manufacturer) ? this.CADExportSettings.InsulationExportDWGFiles : App.InsulationExportDWGDictionary[manufacturer];
      if (App.InsulationExportDXFDictionary.ContainsKey(manufacturer))
        this.DXFChecked = App.InsulationExportDXFDictionary[manufacturer];
      else
        this.DXFChecked = this.CADExportSettings.InsulationExportDXFFiles;
    }
  }

  public void SetSettings()
  {
    if (this.CADExportTool)
    {
      this.ExportFolderPathString = this.CADExportSettings.CADExportFolderPath;
      this.FileNameSuffixString = this.CADExportSettings.CADFileNameSuffix;
      this.DWGChecked = this.CADExportSettings.CADExportDWGFiles;
      this.DXFChecked = this.CADExportSettings.CADExportDXFFiles;
    }
    else
    {
      this.ExportFolderPathString = this.CADExportSettings.InsulationExportFolderPath;
      this.FileNameSuffixString = this.CADExportSettings.InsulationFileNameSuffix;
      this.DWGChecked = this.CADExportSettings.InsulationExportDWGFiles;
      this.DXFChecked = this.CADExportSettings.InsulationExportDXFFiles;
    }
  }

  public CADExportSettings GetUpdatedSettings()
  {
    CADExportSettings updatedSettings = this.CADExportSettings.Clone();
    if (this.CADExportTool)
    {
      updatedSettings.CADExportFolderPath = this.ExportFolderPathString;
      updatedSettings.CADFileNameSuffix = this.FileNameSuffixString;
      updatedSettings.CADExportDWGFiles = this.DWGChecked;
      updatedSettings.CADExportDXFFiles = this.DXFChecked;
    }
    else
    {
      updatedSettings.InsulationExportFolderPath = this.ExportFolderPathString;
      updatedSettings.InsulationFileNameSuffix = this.FileNameSuffixString;
      updatedSettings.InsulationExportDWGFiles = this.DWGChecked;
      updatedSettings.InsulationExportDXFFiles = this.DXFChecked;
    }
    return updatedSettings;
  }

  public void UpdateSessionDictionaries(string Manufacturer)
  {
    if (string.IsNullOrWhiteSpace(Manufacturer))
      return;
    if (this.CADExportTool)
    {
      if (App.CADExportFolderPathDictionary.ContainsKey(Manufacturer))
        App.CADExportFolderPathDictionary[Manufacturer] = this.ExportFolderPathString;
      else
        App.CADExportFolderPathDictionary.Add(Manufacturer, this.ExportFolderPathString);
      if (App.CADExportFileSuffixDictionary.ContainsKey(Manufacturer))
        App.CADExportFileSuffixDictionary[Manufacturer] = this.FileNameSuffixString;
      else
        App.CADExportFileSuffixDictionary.Add(Manufacturer, this.FileNameSuffixString);
      if (App.CADExportDWGDictionary.ContainsKey(Manufacturer))
        App.CADExportDWGDictionary[Manufacturer] = this.DWGChecked;
      else
        App.CADExportDWGDictionary.Add(Manufacturer, this.DWGChecked);
      if (App.CADExportDXFDictionary.ContainsKey(Manufacturer))
        App.CADExportDXFDictionary[Manufacturer] = this.DXFChecked;
      else
        App.CADExportDXFDictionary.Add(Manufacturer, this.DXFChecked);
    }
    else
    {
      if (App.InsulationExportFolderPathDictionary.ContainsKey(Manufacturer))
        App.InsulationExportFolderPathDictionary[Manufacturer] = this.ExportFolderPathString;
      else
        App.InsulationExportFolderPathDictionary.Add(Manufacturer, this.ExportFolderPathString);
      if (App.InsulationExportFileSuffixDictionary.ContainsKey(Manufacturer))
        App.InsulationExportFileSuffixDictionary[Manufacturer] = this.FileNameSuffixString;
      else
        App.InsulationExportFileSuffixDictionary.Add(Manufacturer, this.FileNameSuffixString);
      if (App.InsulationExportDWGDictionary.ContainsKey(Manufacturer))
        App.InsulationExportDWGDictionary[Manufacturer] = this.DWGChecked;
      else
        App.InsulationExportDWGDictionary.Add(Manufacturer, this.DWGChecked);
      if (App.InsulationExportDXFDictionary.ContainsKey(Manufacturer))
        App.InsulationExportDXFDictionary[Manufacturer] = this.DXFChecked;
      else
        App.InsulationExportDXFDictionary.Add(Manufacturer, this.DXFChecked);
    }
  }

  public event PropertyChangedEventHandler PropertyChanged;

  private void NotifyPropertyChanged(string propertyName)
  {
    if (this.PropertyChanged == null)
      return;
    this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
  }
}
