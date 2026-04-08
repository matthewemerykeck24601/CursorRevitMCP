// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Models.DataContracts.FileUploadInfo
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.IO;

#nullable disable
namespace EDGE.Cloud.Models.DataContracts;

public class FileUploadInfo
{
  private FileUploadInfo()
  {
  }

  public string Key { get; set; }

  public Stream InputStream { get; set; }

  public long Length { get; set; }

  public static FileUploadInfo CreateFromStream(string key, Stream stream)
  {
    return new FileUploadInfo()
    {
      Key = key,
      Length = stream.Length,
      InputStream = stream
    };
  }

  public static FileUploadInfo CreateFromFile(string key, string filename, out Exception ex)
  {
    ex = (Exception) null;
    try
    {
      FileStream fileStream = (FileStream) null;
      if (File.Exists(filename))
        fileStream = File.Open(filename, FileMode.Open, FileAccess.Read);
      return new FileUploadInfo()
      {
        Key = key,
        Length = fileStream.Length,
        InputStream = (Stream) fileStream
      };
    }
    catch (Exception ex1)
    {
      ex = ex1;
      return (FileUploadInfo) null;
    }
  }
}
