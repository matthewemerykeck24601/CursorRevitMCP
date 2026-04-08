// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketPopulator.TicketTemplatesHistoryManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.TicketTools.TicketPopulator;

internal class TicketTemplatesHistoryManager
{
  private Dictionary<string, Dictionary<string, string>> DefaultTemplateToProductType;
  private Dictionary<string, Dictionary<string, List<string>>> DefaultTemplateToProductType_BATCH;

  public TicketTemplatesHistoryManager()
  {
    this.DefaultTemplateToProductType = new Dictionary<string, Dictionary<string, string>>();
    this.DefaultTemplateToProductType_BATCH = new Dictionary<string, Dictionary<string, List<string>>>();
  }

  public void Push(string documentName, string productType, string templateName)
  {
    if (this.DefaultTemplateToProductType.ContainsKey(documentName))
    {
      if (this.DefaultTemplateToProductType[documentName].ContainsKey(productType))
        this.DefaultTemplateToProductType[documentName][productType] = templateName;
      else
        this.DefaultTemplateToProductType[documentName].Add(productType, templateName);
    }
    else
    {
      this.DefaultTemplateToProductType.Add(documentName, new Dictionary<string, string>());
      this.DefaultTemplateToProductType[documentName].Add(productType, templateName);
    }
  }

  public void Push_BATCH(string documentName, string productType, List<string> templateNames)
  {
    if (this.DefaultTemplateToProductType_BATCH.ContainsKey(documentName))
    {
      if (this.DefaultTemplateToProductType_BATCH[documentName].ContainsKey(productType))
        this.DefaultTemplateToProductType_BATCH[documentName][productType] = templateNames.ToList<string>();
      else
        this.DefaultTemplateToProductType_BATCH[documentName].Add(productType, templateNames.ToList<string>());
    }
    else
    {
      this.DefaultTemplateToProductType_BATCH.Add(documentName, new Dictionary<string, List<string>>());
      this.DefaultTemplateToProductType_BATCH[documentName].Add(productType, templateNames.ToList<string>());
    }
  }

  public string GetDefaultTemplateName(string documentName, string productType)
  {
    return this.DefaultTemplateToProductType.ContainsKey(documentName) && this.DefaultTemplateToProductType[documentName].ContainsKey(productType) ? this.DefaultTemplateToProductType[documentName][productType] : "";
  }

  public List<string> GetDefaultTemplateNames_BATCH(string documentName, string productType)
  {
    if (!this.DefaultTemplateToProductType_BATCH.ContainsKey(documentName))
      return new List<string>();
    return this.DefaultTemplateToProductType_BATCH[documentName].ContainsKey(productType) ? this.DefaultTemplateToProductType_BATCH[documentName][productType].ToList<string>() : new List<string>();
  }
}
