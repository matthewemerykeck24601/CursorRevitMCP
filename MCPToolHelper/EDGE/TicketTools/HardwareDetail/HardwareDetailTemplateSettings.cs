// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.HardwareDetail.HardwareDetailTemplateSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using EDGE.TicketTools.TemplateToolsBase.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.TicketTools.HardwareDetail;

public class HardwareDetailTemplateSettings
{
  public List<HWDetailTemplate> Templates;
  public ObservableCollection<ComboBoxItemString> manufacturerList;

  public void Clear()
  {
    if (this.Templates == null)
      return;
    this.Templates.Clear();
  }

  public HardwareDetailTemplateSettings()
  {
    this.Templates = new List<HWDetailTemplate>();
    this.manufacturerList = new ObservableCollection<ComboBoxItemString>();
  }

  public bool SaveHardwareDetailTempalteSettings(
    string fileSavePath,
    ObservableCollection<ComboBoxItemString> manuList = null)
  {
    if (manuList == null)
      manuList = this.manufacturerList;
    else
      this.manufacturerList = manuList;
    StreamWriter streamWriter1 = (StreamWriter) null;
    bool flag = false;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (HardwareDetailTemplateSettings));
      DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(fileSavePath));
      if (!directoryInfo.Exists)
        directoryInfo.Create();
      FileInfo fileInfo = new FileInfo(fileSavePath);
      if (fileInfo.Attributes.HasFlag((Enum) FileAttributes.ReadOnly))
        flag = true;
      if (!fileInfo.Exists)
        fileInfo.Create()?.Close();
      streamWriter1 = new StreamWriter(fileSavePath, false);
      StreamWriter streamWriter2 = streamWriter1;
      xmlSerializer.Serialize((TextWriter) streamWriter2, (object) this);
    }
    catch (UnauthorizedAccessException ex)
    {
      if (flag)
      {
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Ticket Tempplate Creator File is Read Only",
          MainContent = $"Unable to read the Ticket Tempplate Creator File in the specified location {fileSavePath} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
        }.Show();
        return false;
      }
      int num = (int) MessageBox.Show($"Unauthorized Access to save path: {fileSavePath}.  Unable to save to this location.  Please ensure that you have write permission to this location or run Revit as administrator by right-clicking the Revit icon and choosing \"Run as Administrator\"{ex.Message}");
      return false;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
      {
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Ticket Tempplate Creator File Error.",
          MainContent = $"Check Ticket Tempplate Creator File: {fileSavePath}. Please ensure the file is not in use by another application and try again."
        }.Show();
        return false;
      }
      int num = (int) MessageBox.Show("Unable to save template settings to path: " + fileSavePath);
      return false;
    }
    finally
    {
      streamWriter1?.Close();
    }
    return true;
  }

  public bool LoadHardwareDetailTemplateSettings(string fileOpenPath)
  {
    FileStream fileStream = (FileStream) null;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (HardwareDetailTemplateSettings));
      FileInfo fileInfo = new FileInfo(fileOpenPath);
      if (!fileInfo.Exists)
        return false;
      fileStream = fileInfo.OpenRead();
      HardwareDetailTemplateSettings templateSettings = (HardwareDetailTemplateSettings) xmlSerializer.Deserialize((Stream) fileStream);
      if (templateSettings.manufacturerList != null && templateSettings.manufacturerList.Any<ComboBoxItemString>())
      {
        foreach (ComboBoxItemString manufacturer in (Collection<ComboBoxItemString>) templateSettings.manufacturerList)
          this.manufacturerList.Add(manufacturer);
      }
      if (templateSettings.Templates == null || !templateSettings.Templates.Any<HWDetailTemplate>())
        return true;
      this.Templates.AddRange((IEnumerable<HWDetailTemplate>) templateSettings.Templates);
      return true;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Hardware Detail Template Settings File Error.",
          MainContent = $"Check Hardware Detail Settings File: {fileOpenPath}. Please ensure the file is not in use by another application and try again."
        }.Show();
      return false;
    }
    finally
    {
      fileStream?.Close();
    }
  }
}
