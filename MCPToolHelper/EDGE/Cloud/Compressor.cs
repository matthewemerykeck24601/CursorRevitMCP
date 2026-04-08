// Decompiled with JetBrains decompiler
// Type: EDGE.Cloud.Compressor
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

#nullable disable
namespace EDGE.Cloud;

internal class Compressor
{
  public static byte[] Compress(object value)
  {
    return Compressor.Compress(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value)));
  }

  public static byte[] Compress(byte[] bytes)
  {
    byte[] numArray = (byte[]) null;
    if (bytes == null)
      return numArray;
    using (MemoryStream memoryStream = new MemoryStream())
    {
      using (GZipStream gzipStream = new GZipStream((Stream) memoryStream, CompressionMode.Compress, true))
        gzipStream.Write(bytes, 0, bytes.Length);
      return memoryStream.ToArray();
    }
  }
}
