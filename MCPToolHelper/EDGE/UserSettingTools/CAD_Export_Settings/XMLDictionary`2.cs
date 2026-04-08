// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.CAD_Export_Settings.XMLDictionary`2
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#nullable disable
namespace EDGE.UserSettingTools.CAD_Export_Settings;

public class XMLDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
  public XmlSchema GetSchema() => (XmlSchema) null;

  public void ReadXml(XmlReader reader)
  {
    XmlSerializer xmlSerializer1 = new XmlSerializer(typeof (TKey));
    XmlSerializer xmlSerializer2 = new XmlSerializer(typeof (TValue));
    int num = reader.IsEmptyElement ? 1 : 0;
    reader.Read();
    if (num != 0)
      return;
    while (reader.NodeType != XmlNodeType.EndElement)
    {
      reader.ReadStartElement("item");
      reader.ReadStartElement("key");
      TKey key = (TKey) xmlSerializer1.Deserialize(reader);
      reader.ReadEndElement();
      reader.ReadStartElement("value");
      TValue obj = (TValue) xmlSerializer2.Deserialize(reader);
      reader.ReadEndElement();
      this.Add(key, obj);
      reader.ReadEndElement();
      int content = (int) reader.MoveToContent();
    }
    reader.ReadEndElement();
  }

  public void WriteXml(XmlWriter writer)
  {
    XmlSerializer xmlSerializer1 = new XmlSerializer(typeof (TKey));
    XmlSerializer xmlSerializer2 = new XmlSerializer(typeof (TValue));
    foreach (TKey key in this.Keys)
    {
      writer.WriteStartElement("item");
      writer.WriteStartElement("key");
      xmlSerializer1.Serialize(writer, (object) key);
      writer.WriteEndElement();
      writer.WriteStartElement("value");
      xmlSerializer2.Serialize(writer, (object) this[key]);
      writer.WriteEndElement();
      writer.WriteEndElement();
    }
  }
}
