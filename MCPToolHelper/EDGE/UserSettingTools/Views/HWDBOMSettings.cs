// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Views.HWDBOMSettings
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.UserSettingTools.Views;

public class HWDBOMSettings
{
  public List<DGEntry> schedule_List = new List<DGEntry>();

  public bool SaveTemplateSettings(string fileSavePath)
  {
    StreamWriter streamWriter1 = (StreamWriter) null;
    bool flag = false;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (HWDBOMSettings));
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
          MainInstruction = "Ticket BOM User Settings File is Read Only",
          MainContent = $"Unable to read the Ticket BOM User Settings File in the specified location {fileSavePath} because the file is Read Only. Please make the file available for editing in order to be able to edit the settings file."
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
          MainInstruction = "Ticket BOM User Settings File Error.",
          MainContent = $"Check Ticket BOM User Settings File: {fileSavePath}. Please ensure the file is not in use by another application and try again."
        }.Show();
        return false;
      }
      int num = (int) MessageBox.Show($"{ex.ToString()}. Unable to save template settings to path: {fileSavePath}");
      return false;
    }
    finally
    {
      streamWriter1?.Close();
    }
    return true;
  }

  public bool LoadTicketTemplateSettings(string fileOpenPath)
  {
    FileStream fileStream = (FileStream) null;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (HWDBOMSettings));
      FileInfo fileInfo = new FileInfo(fileOpenPath);
      if (!fileInfo.Exists)
        return false;
      fileStream = fileInfo.OpenRead();
      HWDBOMSettings hwdbomSettings = (HWDBOMSettings) xmlSerializer.Deserialize((Stream) fileStream);
      if (hwdbomSettings.schedule_List == null || !hwdbomSettings.schedule_List.Any<DGEntry>())
        return true;
      this.schedule_List.AddRange((IEnumerable<DGEntry>) hwdbomSettings.schedule_List);
      return true;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Ticket BOM User Settings File Error.",
          MainContent = $"CheckTicket BOM User Settings File: {fileOpenPath}. Please ensure the file is not in use by another application and try again."
        }.Show();
      return false;
    }
    finally
    {
      fileStream?.Close();
    }
  }
}
