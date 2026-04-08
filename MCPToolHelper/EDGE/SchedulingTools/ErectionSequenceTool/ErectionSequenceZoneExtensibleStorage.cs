// Decompiled with JetBrains decompiler
// Type: EDGE.SchedulingTools.ErectionSequenceTool.ErectionSequenceZoneExtensibleStorage
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.SchedulingTools.ErectionSequenceTool;

public static class ErectionSequenceZoneExtensibleStorage
{
  private static string guidString = "a4e52a83-df48-48ec-9a08-0e01ab09c848";

  public static bool SaveErectionSequenceZones(
    Document revitDoc,
    List<ErectionSequenceZone> zoneList,
    out bool isException)
  {
    isException = false;
    using (Transaction transaction = new Transaction(revitDoc, "Save Erection Sequence Zones"))
    {
      try
      {
        int num1 = (int) transaction.Start();
        Element projectInformation = (Element) revitDoc.ProjectInformation;
        if (projectInformation == null)
        {
          isException = true;
          return false;
        }
        if (revitDoc.IsWorkshared)
        {
          if (!ErectionSequence_Command.checkElementOwnershipCondition((ICollection<ElementId>) new List<ElementId>()
          {
            projectInformation.Id
          }, revitDoc, true))
            return false;
        }
        Schema schema = Schema.Lookup(new Guid(ErectionSequenceZoneExtensibleStorage.guidString));
        if (schema == null)
        {
          SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid(ErectionSequenceZoneExtensibleStorage.guidString));
          schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
          schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
          schemaBuilder.AddArrayField("ErectionSequenceZoneList", typeof (string));
          schemaBuilder.SetSchemaName("ErectionSequenceZones");
          schema = schemaBuilder.Finish();
        }
        Entity entity = new Entity(schema);
        IList<string> stringList = (IList<string>) new List<string>();
        foreach (ErectionSequenceZone zone in zoneList)
          stringList.Add($"{zone.ZoneIndex.ToString()} , {zone.ZoneName}");
        entity.Set<IList<string>>("ErectionSequenceZoneList", stringList);
        projectInformation.SetEntity(entity);
        int num2 = (int) transaction.Commit();
      }
      catch (Exception ex)
      {
        if (transaction.GetStatus() == TransactionStatus.Started)
        {
          int num = (int) transaction.RollBack();
        }
        isException = true;
        return false;
      }
    }
    return true;
  }

  public static List<ErectionSequenceZone> ReadErectionSequenceZones(Document revitDoc)
  {
    try
    {
      Element projectInformation = (Element) revitDoc.ProjectInformation;
      if (projectInformation == null)
        return (List<ErectionSequenceZone>) null;
      Schema schema = Schema.Lookup(new Guid(ErectionSequenceZoneExtensibleStorage.guidString));
      if (schema == null)
        return (List<ErectionSequenceZone>) null;
      Entity entity = projectInformation.GetEntity(schema);
      if (entity == null)
        return (List<ErectionSequenceZone>) null;
      IList<string> stringList = entity.Get<IList<string>>("ErectionSequenceZoneList");
      if (stringList == null || stringList.Count == 0)
        return (List<ErectionSequenceZone>) null;
      List<ErectionSequenceZone> source1 = new List<ErectionSequenceZone>();
      foreach (string str in (IEnumerable<string>) stringList)
      {
        string[] source2 = str.Split(',');
        int result = 0;
        if (int.TryParse(source2[0], out result))
        {
          string empty = string.Empty;
          for (int index = 1; index < ((IEnumerable<string>) source2).Count<string>(); ++index)
            empty += source2[index];
          if (!string.IsNullOrEmpty(empty))
          {
            ErectionSequenceZone erectionSequenceZone = new ErectionSequenceZone(empty.Trim(), result);
            source1.Add(erectionSequenceZone);
          }
        }
      }
      return source1.Count<ErectionSequenceZone>() < 1 ? (List<ErectionSequenceZone>) null : source1;
    }
    catch (Exception ex)
    {
      return (List<ErectionSequenceZone>) null;
    }
  }
}
