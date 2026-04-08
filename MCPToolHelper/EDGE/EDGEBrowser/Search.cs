// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.Search
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.EDGEBrowser;

internal class Search
{
  public List<string> getList(CloudBlobContainer container)
  {
    return container.ListBlobs(useFlatBlobListing: true).Select<IListBlobItem, string>((Func<IListBlobItem, string>) (e => e.Uri.ToString())).ToList<string>();
  }

  public Dictionary<string, string> SearchResult(
    List<Tuple<string, string>> training,
    List<Tuple<string, string>> family,
    List<Tuple<string, string>> xlxs,
    List<Tuple<string, string>> txt,
    string searchvalue)
  {
    List<string> stringList = new List<string>();
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    foreach (Tuple<string, string> tuple in training)
    {
      string str1 = tuple.Item1.Substring(tuple.Item1.IndexOf('/') + 1).ToUpper().Trim();
      string str2 = searchvalue.ToUpper().Trim();
      if (str1.Contains(str2) || str1.Replace("_", " ").Contains(str2))
      {
        string key = ((IEnumerable<string>) tuple.Item1.Split('/')).Last<string>().Replace("%20", " ");
        if (!dictionary.ContainsKey(key))
          dictionary.Add(key, tuple.Item1);
        else
          dictionary[key] = tuple.Item1;
      }
    }
    foreach (Tuple<string, string> tuple in family)
    {
      string str3 = tuple.Item1.Substring(tuple.Item1.IndexOf('/') + 2).ToUpper().Trim();
      string str4 = searchvalue.ToUpper().Trim();
      if (str3.Contains(str4) || str3.Replace("_", " ").Contains(str4))
      {
        string key = ((IEnumerable<string>) tuple.Item1.Split('/')).Last<string>().Replace("%20", " ");
        if (!dictionary.ContainsKey(key))
          dictionary.Add(key, tuple.Item1);
        else
          dictionary[key] = tuple.Item1;
      }
    }
    foreach (Tuple<string, string> tuple in xlxs)
    {
      string str5 = tuple.Item1.Substring(tuple.Item1.IndexOf('/') + 2).ToUpper().Trim();
      string str6 = searchvalue.ToUpper().Trim();
      if (str5.Contains(str6) || str5.Replace("_", " ").Contains(str6))
      {
        string key = ((IEnumerable<string>) tuple.Item1.Split('/')).Last<string>().Replace("%20", " ");
        if (!dictionary.ContainsKey(key))
          dictionary.Add(key, tuple.Item1);
        else
          dictionary[key] = tuple.Item1;
      }
    }
    foreach (Tuple<string, string> tuple in txt)
    {
      string str7 = tuple.Item1.Substring(tuple.Item1.IndexOf('/') + 2).ToUpper().Trim();
      string str8 = searchvalue.ToUpper().Trim();
      if (str7.Contains(str8) || str7.Replace("_", " ").Contains(str8))
      {
        string key = ((IEnumerable<string>) tuple.Item1.Split('/')).Last<string>().Replace("%20", " ");
        if (!dictionary.ContainsKey(key))
          dictionary.Add(key, tuple.Item1);
        else
          dictionary[key] = tuple.Item1;
      }
    }
    return dictionary;
  }
}
