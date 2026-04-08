// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TemplateToolsBase.TicketTemplateSettings
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
namespace EDGE.TicketTools.TemplateToolsBase;

public class TicketTemplateSettings
{
  public List<TicketTemplate> Templates;
  public ObservableCollection<ComboBoxItemString> manufacurerList;

  public void Clear()
  {
    if (this.Templates == null)
      return;
    this.Templates.Clear();
  }

  public TicketTemplateSettings()
  {
    this.Templates = new List<TicketTemplate>();
    this.manufacurerList = new ObservableCollection<ComboBoxItemString>();
  }

  public bool SaveTemplateSettings(
    string fileSavePath,
    ObservableCollection<ComboBoxItemString> manuList = null)
  {
    if (manuList == null)
      manuList = this.manufacurerList;
    else
      this.manufacurerList = manuList;
    StreamWriter streamWriter1 = (StreamWriter) null;
    bool flag = false;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (TicketTemplateSettings));
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
          MainInstruction = "Ticket Template Creator File is Read Only",
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

  public bool LoadTicketTemplateSettings(string fileOpenPath)
  {
    FileStream fileStream = (FileStream) null;
    try
    {
      XmlSerializer xmlSerializer = new XmlSerializer(typeof (TicketTemplateSettings));
      FileInfo fileInfo = new FileInfo(fileOpenPath);
      if (!fileInfo.Exists)
        return false;
      fileStream = fileInfo.OpenRead();
      TicketTemplateSettings templateSettings = (TicketTemplateSettings) xmlSerializer.Deserialize((Stream) fileStream);
      if (templateSettings.manufacurerList != null && templateSettings.manufacurerList.Any<ComboBoxItemString>())
      {
        foreach (ComboBoxItemString manufacurer in (Collection<ComboBoxItemString>) templateSettings.manufacurerList)
          this.manufacurerList.Add(manufacurer);
      }
      if (templateSettings.Templates == null || !templateSettings.Templates.Any<TicketTemplate>())
        return true;
      this.Templates.AddRange((IEnumerable<TicketTemplate>) templateSettings.Templates);
      return true;
    }
    catch (Exception ex)
    {
      if (ex.Message.Contains("process"))
        new TaskDialog("EDGE^R")
        {
          AllowCancellation = false,
          MainInstruction = "Ticket Template Settings File Error.",
          MainContent = $"Check Ticket Template Settings File: {fileOpenPath}. Please ensure the file is not in use by another application and try again."
        }.Show();
      return false;
    }
    finally
    {
      fileStream?.Close();
    }
  }
}
