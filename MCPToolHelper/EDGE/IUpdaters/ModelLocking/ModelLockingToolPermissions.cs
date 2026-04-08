// Decompiled with JetBrains decompiler
// Type: EDGE.IUpdaters.ModelLocking.ModelLockingToolPermissions
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

#nullable disable
namespace EDGE.IUpdaters.ModelLocking;

public class ModelLockingToolPermissions
{
  public static ModelPermissionsCategory AddonHosting = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory AssemblyCreation = ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory AutoWarping = ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware;
  public static ModelPermissionsCategory CtrlNumIncrementor = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory CopyReinforcing = ModelPermissionsCategory.RebarHandling;
  public static ModelPermissionsCategory TicketManagerParams = ModelPermissionsCategory.Geometry | ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory PlaceCentroid = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory MarkAsReinforced = ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory MarkRebarByProduct = ModelPermissionsCategory.RebarHandling;
  public static ModelPermissionsCategory MVInitial = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory MatchTopAsCast = ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory MultiVoid = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory NewReferencePoint = ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory PinUnpin = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory ProjectShared = ModelPermissionsCategory.Admin;
  public static ModelPermissionsCategory ScheduleByView = ModelPermissionsCategory.Geometry | ModelPermissionsCategory.ConnectionsHardware | ModelPermissionsCategory.RebarHandling | ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory TopAsCast = ModelPermissionsCategory.Assemblies;
  public static ModelPermissionsCategory VoidHosting = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory WarpedParameter = ModelPermissionsCategory.Geometry;
  public static ModelPermissionsCategory ControlNumberIncrementor = ModelPermissionsCategory.Geometry;
}
