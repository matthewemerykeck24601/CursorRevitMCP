// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingPermissionsSchema
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingPermissionsSchema
{
  private static Guid s_EDGEModelLockingPermissionsSchemaGuid_Ver1 = new Guid("A33D123A-F9D5-4BD4-A368-6731DFD3F2CE");
  private static Guid s_EDGEModelLockingPermissionsSchemaGuid_Ver2 = new Guid("4D114706-10DA-4808-941E-320815A3FCBB");
  private static string s_strFieldNameAllPermissions = "AllPermissions";
  private static string s_strFieldNameGeometryPermissions = "GeometryPermissions";
  private static string s_strFieldNameConnectionsHardware = "ConnectionsHardware";
  private static string s_strFieldNameRebarHandling = "RebarHandling";
  private static string s_strFieldNameAssemblies = "Assemblies";
  private static ModelPermissionsCategory All = ModelPermissionsCategory.NonTracked | ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies | ModelPermissionsCategory.Admin;

  private static Schema CreateSchema()
  {
    SchemaBuilder schemaBuilder = new SchemaBuilder(ModelLockingPermissionsSchema.s_EDGEModelLockingPermissionsSchemaGuid_Ver2);
    schemaBuilder.SetSchemaName("EDGEModelLockingPermissions");
    schemaBuilder.AddSimpleField(ModelLockingPermissionsSchema.s_strFieldNameAllPermissions, typeof (string));
    schemaBuilder.AddSimpleField(ModelLockingPermissionsSchema.s_strFieldNameGeometryPermissions, typeof (string));
    schemaBuilder.AddSimpleField(ModelLockingPermissionsSchema.s_strFieldNameConnectionsHardware, typeof (string));
    schemaBuilder.AddSimpleField(ModelLockingPermissionsSchema.s_strFieldNameRebarHandling, typeof (string));
    schemaBuilder.AddSimpleField(ModelLockingPermissionsSchema.s_strFieldNameAssemblies, typeof (string));
    return schemaBuilder.Finish();
  }

  private static Schema GetSchema()
  {
    return Schema.Lookup(ModelLockingPermissionsSchema.s_EDGEModelLockingPermissionsSchemaGuid_Ver2) ?? ModelLockingPermissionsSchema.CreateSchema();
  }

  private static Entity GetCurrentVersionEntityFromProjectInfo(Element projectInfo)
  {
    Schema schema1 = ModelLockingPermissionsSchema.GetSchema();
    Entity entityFromProjectInfo = schema1 != null ? projectInfo.GetEntity(schema1) : throw new Exception("Current Version Schema for Model Locking is Null, this should not happen, requires investigation.  Please contact support.");
    if (entityFromProjectInfo.IsValid())
      return entityFromProjectInfo;
    Schema schema2 = Schema.Lookup(ModelLockingPermissionsSchema.s_EDGEModelLockingPermissionsSchemaGuid_Ver1);
    if (schema2 == null)
      return ModelLockingPermissionsSchema.GetEntityBySchema(projectInfo, schema1);
    Entity entity1 = projectInfo.GetEntity(schema2);
    if (!entity1.IsValid())
      return ModelLockingPermissionsSchema.GetEntityBySchema(projectInfo, schema1);
    Entity entity2 = new Entity(schema1);
    List<string> stringList1 = ModelLockingPermissionsSchema.TryGet(entity1, ModelLockingPermissionsSchema.s_strFieldNameAllPermissions);
    List<string> stringList2 = ModelLockingPermissionsSchema.TryGet(entity1, ModelLockingPermissionsSchema.s_strFieldNameGeometryPermissions);
    List<string> stringList3 = ModelLockingPermissionsSchema.TryGet(entity1, ModelLockingPermissionsSchema.s_strFieldNameConnectionsHardware);
    List<string> stringList4 = ModelLockingPermissionsSchema.TryGet(entity1, ModelLockingPermissionsSchema.s_strFieldNameRebarHandling);
    List<string> stringList5 = ModelLockingPermissionsSchema.TryGet(entity1, ModelLockingPermissionsSchema.s_strFieldNameAssemblies);
    ModelLockingPermissionsSchema.TrySet(entity2, ModelLockingPermissionsSchema.s_strFieldNameAllPermissions, stringList1);
    ModelLockingPermissionsSchema.TrySet(entity2, ModelLockingPermissionsSchema.s_strFieldNameGeometryPermissions, stringList2);
    ModelLockingPermissionsSchema.TrySet(entity2, ModelLockingPermissionsSchema.s_strFieldNameConnectionsHardware, stringList3);
    ModelLockingPermissionsSchema.TrySet(entity2, ModelLockingPermissionsSchema.s_strFieldNameRebarHandling, stringList4);
    ModelLockingPermissionsSchema.TrySet(entity2, ModelLockingPermissionsSchema.s_strFieldNameAssemblies, stringList5);
    return entity2;
  }

  private static Entity GetEntityBySchema(Element elem, Schema schema)
  {
    Entity entityBySchema = elem.GetEntity(schema);
    if (!entityBySchema.IsValid())
      entityBySchema = new Entity(schema);
    return entityBySchema;
  }

  public static void SetModelPermissions(
    ModelPermissionsCategory category,
    Element element,
    List<string> permissionsList)
  {
    Entity entityFromProjectInfo = ModelLockingPermissionsSchema.GetCurrentVersionEntityFromProjectInfo(element);
    int all = (int) ModelLockingPermissionsSchema.All;
    if ((category & ModelPermissionsCategory.Admin) == ModelPermissionsCategory.Admin)
      ModelLockingPermissionsSchema.TrySet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameAllPermissions, permissionsList);
    if ((category & ModelPermissionsCategory.Geometry) == ModelPermissionsCategory.Geometry)
      ModelLockingPermissionsSchema.TrySet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameGeometryPermissions, permissionsList);
    if ((category & ModelPermissionsCategory.ConnectionsHardware) == ModelPermissionsCategory.ConnectionsHardware)
      ModelLockingPermissionsSchema.TrySet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameConnectionsHardware, permissionsList);
    if ((category & ModelPermissionsCategory.RebarHandling) == ModelPermissionsCategory.RebarHandling)
      ModelLockingPermissionsSchema.TrySet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameRebarHandling, permissionsList);
    if ((category & ModelPermissionsCategory.Assemblies) == ModelPermissionsCategory.Assemblies)
      ModelLockingPermissionsSchema.TrySet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameAssemblies, permissionsList);
    element.SetEntity(entityFromProjectInfo);
  }

  public static List<string> GetModelPermissions(
    ModelPermissionsCategory category,
    Element projInfo)
  {
    Entity entityFromProjectInfo = ModelLockingPermissionsSchema.GetCurrentVersionEntityFromProjectInfo(projInfo);
    int all = (int) ModelLockingPermissionsSchema.All;
    if (!entityFromProjectInfo.IsValid())
      return new List<string>();
    if ((category & ModelPermissionsCategory.Admin) == ModelPermissionsCategory.Admin)
      return ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameAllPermissions);
    if ((category & ModelPermissionsCategory.Geometry) == ModelPermissionsCategory.Geometry)
      return ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameGeometryPermissions);
    if ((category & ModelPermissionsCategory.ConnectionsHardware) == ModelPermissionsCategory.ConnectionsHardware)
      return ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameConnectionsHardware);
    if ((category & ModelPermissionsCategory.RebarHandling) == ModelPermissionsCategory.RebarHandling)
      return ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameRebarHandling);
    return (category & ModelPermissionsCategory.Assemblies) == ModelPermissionsCategory.Assemblies ? ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameAssemblies) : new List<string>();
  }

  internal static List<string> TryGet(Entity entity, string name)
  {
    try
    {
      return ModelLockingPermissionsSchema.SchemaToList(entity.Get<string>(name));
    }
    catch
    {
      return new List<string>();
    }
  }

  internal static void TrySet(Entity entity, string name, List<string> value)
  {
    try
    {
      entity.Set<string>(name, ModelLockingPermissionsSchema.ListToSchema(value));
    }
    catch
    {
      new TaskDialog("Edge Schema Error")
      {
        FooterText = "EDGE^R 2024 12.2.0 (build date: 2025.06.10)",
        MainInstruction = "Incompatible Schema for Model Locking",
        MainContent = ("This file was possibly created with an earlier version of the Edge Model Permissions Schema.  Assembly permissions cannot be configured.  Field Name: " + name)
      }.Show();
    }
  }

  public static List<string> SchemaToList(string userString, char delimiter = ';')
  {
    List<string> list = new List<string>();
    foreach (Match match in new Regex($"[^{delimiter.ToString()}]+").Matches(userString))
      list.Add(match.Value);
    return list;
  }

  public static string ListToSchema(List<string> userList, char delimiter = ';')
  {
    string schema = "";
    foreach (string user in userList)
      schema = schema + user + delimiter.ToString();
    return schema;
  }

  public static bool isProducerAdmin(string strUserName)
  {
    return App.ModelLockingAdminList.Contains(strUserName.ToUpper());
  }

  internal static ModelPermissionsCategory GetModelPermissionsForUser(
    string strUserName,
    ProjectInfo projectInfo)
  {
    if (ModelLockingPermissionsSchema.isProducerAdmin(strUserName))
      return ModelLockingPermissionsSchema.All;
    if (!ModelLockingPermissionsSchema.ModelHasPermissions(projectInfo))
      return ModelLockingPermissionsSchema.All & ~ModelPermissionsCategory.Admin & ~ModelPermissionsCategory.NonTracked;
    Entity entityFromProjectInfo = ModelLockingPermissionsSchema.GetCurrentVersionEntityFromProjectInfo((Element) projectInfo);
    ModelPermissionsCategory permissionsForUser = ModelPermissionsCategory.NonTracked;
    if (!entityFromProjectInfo.IsValid() || ModelLockingPermissionsSchema.UserPermitted(strUserName, ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameAllPermissions)))
      return ModelLockingPermissionsSchema.All;
    if (ModelLockingPermissionsSchema.UserPermitted(strUserName, ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameGeometryPermissions)))
      permissionsForUser |= ModelPermissionsCategory.Geometry;
    if (ModelLockingPermissionsSchema.UserPermitted(strUserName, ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameConnectionsHardware)))
      permissionsForUser |= ModelPermissionsCategory.ConnectionsHardware;
    if (ModelLockingPermissionsSchema.UserPermitted(strUserName, ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameRebarHandling)))
      permissionsForUser |= ModelPermissionsCategory.RebarHandling;
    if (ModelLockingPermissionsSchema.UserPermitted(strUserName, ModelLockingPermissionsSchema.TryGet(entityFromProjectInfo, ModelLockingPermissionsSchema.s_strFieldNameAssemblies)))
      permissionsForUser |= ModelPermissionsCategory.Assemblies;
    return permissionsForUser;
  }

  private static bool ModelHasPermissions(ProjectInfo projectInfo)
  {
    IList<Guid> entitySchemaGuids = projectInfo.GetEntitySchemaGuids();
    HashSet<string> stringSet = new HashSet<string>()
    {
      ModelLockingPermissionsSchema.s_EDGEModelLockingPermissionsSchemaGuid_Ver2.ToString(),
      ModelLockingPermissionsSchema.s_EDGEModelLockingPermissionsSchemaGuid_Ver1.ToString()
    };
    foreach (Guid guid in (IEnumerable<Guid>) entitySchemaGuids)
    {
      if (stringSet.Contains(guid.ToString()))
        return true;
    }
    return false;
  }

  private static bool UserPermitted(string strUserName, List<string> allowedPermissions)
  {
    return allowedPermissions.Contains(strUserName.ToUpper());
  }
}
