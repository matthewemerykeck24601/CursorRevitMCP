// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.LaserProjection
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using EDGE.AdminTools.CAM.LaserProjectionUI;
using EDGE.UserSettingTools.CAD_Export_Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utils;
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.ElementUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.SelectionUtils;
using Utils.SettingsUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.AdminTools.CAM;

[Transaction(TransactionMode.Manual)]
internal class LaserProjection : IExternalCommand
{
  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    App.laserExportErrorCheck(true);
    List<ElementId> elementIdList1 = new List<ElementId>();
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    if (revitDoc.IsFamilyDocument)
    {
      TaskDialog.Show("Error", "CAD Export cannot be run from the family editor.");
      App.laserExportErrorCheck(false);
      return (Result) 1;
    }
    string manufacturer1 = "";
    Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && parameter.StorageType == StorageType.String)
      manufacturer1 = parameter.AsString();
    if (string.IsNullOrWhiteSpace(manufacturer1))
    {
      TaskDialog.Show("CAD EXPORT", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to open the CAD Export tool again.");
      App.laserExportErrorCheck(false);
      return (Result) 1;
    }
    CADExportSettings cadExportSettings = new CADExportSettings(true);
    if (!cadExportSettings.GetManufacturerSettings(manufacturer1))
    {
      TaskDialog taskDialog = new TaskDialog("CAD Export")
      {
        MainInstruction = "Unable to read CAD Export Settings file."
      };
      taskDialog.MainInstruction += "The CAD Export will use the default CAD Export settings.";
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
      if (taskDialog.Show() != 1)
      {
        App.laserExportErrorCheck(false);
        return (Result) 1;
      }
    }
    string projectName1 = revitDoc.ProjectInformation.Name.Replace('/', '_');
    Dictionary<string, Category> dictionary1 = new Dictionary<string, Category>();
    ViewFamilyType viewFamilyType = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>((Func<ViewFamilyType, bool>) (x => x.ViewFamily == ViewFamily.ThreeDimensional));
    ExportDWGSettings exportDwgSettings1 = (ExportDWGSettings) null;
    View3D view3D1 = (View3D) null;
    Dictionary<CADExportSettings.ExportGroup, CADExportLayer> layerDict = new Dictionary<CADExportSettings.ExportGroup, CADExportLayer>();
    if (cadExportSettings.TIFLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.TIF, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.TIF));
    if (cadExportSettings.BIFLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.BIF, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.BIF));
    if (cadExportSettings.SIFLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.SIF, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.SIF));
    if (cadExportSettings.FormLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Form, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Form));
    if (cadExportSettings.ArchLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Arch, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Arch));
    if (cadExportSettings.CorbelLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Corbel, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Corbel));
    if (cadExportSettings.StrandLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Strand, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Strand));
    List<LineStyleDetails> source1 = new List<LineStyleDetails>();
    Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
    foreach (CADExportSettings.ExportGroup key in layerDict.Keys)
    {
      CADExportSettings.ExportGroup group = key;
      if (!source1.Where<LineStyleDetails>((Func<LineStyleDetails, bool>) (x => x._name == layerDict[group].LayerName)).Any<LineStyleDetails>())
        source1.Add(new LineStyleDetails(layerDict[group].LayerName, 8, LaserUtils.WindowsToAutodeskColor(layerDict[group].LayerColor.color), layerDict[group].LayerColor.cadNumber));
      if (!dictionary2.ContainsKey(layerDict[group].LayerName))
        dictionary2.Add(layerDict[group].LayerName, layerDict[group].LayerColor.cadNumber);
    }
    bool cancelled;
    Dictionary<string, Category> dictionary3 = new LineStyleManager(revitDoc, source1.ToArray()).SetupLineStyleCategories(out cancelled);
    if (cancelled)
    {
      App.laserExportErrorCheck(false);
      return (Result) 1;
    }
    using (Transaction transaction = new Transaction(revitDoc, "Settings Update"))
    {
      bool flag1 = false;
      bool flag2 = false;
      Category category1 = Category.GetCategory(revitDoc, BuiltInCategory.OST_Lines);
      foreach (Element element in (IEnumerable<Element>) new FilteredElementCollector(revitDoc).OfClass(typeof (ExportDWGSettings)).ToElements())
      {
        if (element is ExportDWGSettings exportDwgSettings2 && exportDwgSettings2.Name == "EDGE_CAD_Export")
        {
          exportDwgSettings1 = exportDwgSettings2;
          DWGExportOptions dwgExportOptions = exportDwgSettings1.GetDWGExportOptions();
          if (dwgExportOptions.PropOverrides != PropOverrideMode.ByLayer)
          {
            flag2 = true;
            break;
          }
          ExportLayerTable exportLayerTable = dwgExportOptions.GetExportLayerTable();
          using (Dictionary<string, Category>.KeyCollection.Enumerator enumerator = dictionary3.Keys.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              string current = enumerator.Current;
              Category category2 = dictionary3[current];
              if (category2 != null)
              {
                ExportLayerKey exportLayerKey = new ExportLayerKey(category1.Name, category2.Name, SpecialType.Default);
                if (!exportLayerTable.ContainsKey(exportLayerKey))
                {
                  flag2 = true;
                  break;
                }
                ExportLayerInfo exportLayerInfo = exportLayerTable[exportLayerKey];
                if (exportLayerInfo.LayerName != current || exportLayerInfo.ColorNumber != dictionary2[current])
                {
                  flag2 = true;
                  break;
                }
              }
            }
            break;
          }
        }
      }
      if (exportDwgSettings1 == null)
        flag1 = true;
      if (flag1 | flag2)
      {
        int num1 = (int) transaction.Start();
        if (flag1)
          exportDwgSettings1 = ExportDWGSettings.Create(revitDoc, "EDGE_CAD_Export");
        if (exportDwgSettings1 != null)
        {
          DWGExportOptions dwgExportOptions = exportDwgSettings1.GetDWGExportOptions();
          ExportLayerTable exportLayerTable = dwgExportOptions.GetExportLayerTable();
          foreach (string key in dictionary3.Keys)
          {
            Category category3 = dictionary3[key];
            if (category3 != null)
            {
              ExportLayerKey exportLayerKey = new ExportLayerKey(category1.Name, category3.Name, SpecialType.Default);
              ExportLayerInfo exportLayerInfo = new ExportLayerInfo();
              exportLayerInfo.LayerName = key;
              exportLayerInfo.ColorNumber = dictionary2[key];
              if (exportLayerTable.ContainsKey(exportLayerKey))
                exportLayerTable[exportLayerKey] = exportLayerInfo;
              else
                exportLayerTable.Add(exportLayerKey, exportLayerInfo);
            }
          }
          dwgExportOptions.SetExportLayerTable(exportLayerTable);
          dwgExportOptions.PropOverrides = PropOverrideMode.ByLayer;
          exportDwgSettings1.SetDWGExportOptions(dwgExportOptions);
        }
        int num2 = (int) transaction.Commit();
      }
    }
    using (Transaction transaction = new Transaction(revitDoc, "Update Export View"))
    {
      List<Category> categoryList = new List<Category>();
      foreach (Category category in (CategoryNameMap) revitDoc.Settings.Categories)
      {
        categoryList.Add(category);
        foreach (Category subCategory in category.SubCategories)
          categoryList.Add(subCategory);
      }
      List<Element> list = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_Views).Where<Element>((Func<Element, bool>) (v => v is View3D)).ToList<Element>();
      bool flag3 = false;
      foreach (Element element in list)
      {
        if (element is View3D view3D2 && view3D2.Name.Contains("EXPORT-VIEW"))
        {
          if (revitDoc.IsWorkshared)
          {
            if (!CheckElementsOwnership.CheckOwnership("CAD Export", new List<ElementId>()
            {
              view3D2.Id
            }, revitDoc, commandData.Application.ActiveUIDocument, out List<ElementId> _))
            {
              App.laserExportErrorCheck(false);
              return (Result) 1;
            }
          }
          flag3 = true;
          bool flag4 = false;
          if (view3D2.ViewTemplateId != (ElementId) null && !view3D2.ViewTemplateId.Equals((object) ElementId.InvalidElementId))
            flag4 = true;
          if (view3D2.Scale != 96 /*0x60*/)
            flag4 = true;
          List<ElementId> elementIdList2 = new List<ElementId>();
          foreach (string key in dictionary3.Keys)
          {
            if (view3D2.GetCategoryHidden(dictionary3[key].Id))
            {
              elementIdList2.Add(dictionary3[key].Id);
              flag4 = true;
            }
          }
          List<ElementId> source2 = new List<ElementId>()
          {
            new ElementId(BuiltInCategory.OST_Lines)
          };
          source2.AddRange(dictionary3.Values.Select<Category, ElementId>((Func<Category, ElementId>) (v => v.Id)));
          List<ElementId> elementIdList3 = new List<ElementId>();
          foreach (Category category in categoryList)
          {
            ElementId catID = category.Id;
            if (!source2.Any<ElementId>((Func<ElementId, bool>) (c => c.Equals((object) catID))) && view3D2.CanCategoryBeHidden(catID) && !view3D2.GetCategoryHidden(catID))
            {
              flag4 = true;
              elementIdList3.Add(catID);
            }
          }
          if (flag4)
          {
            int num3 = (int) transaction.Start();
            view3D2.ViewTemplateId = ElementId.InvalidElementId;
            view3D2.Scale = 96 /*0x60*/;
            foreach (ElementId categoryId in elementIdList2)
              view3D2.SetCategoryHidden(categoryId, false);
            foreach (ElementId categoryId in elementIdList3)
              view3D2.SetCategoryHidden(categoryId, true);
            int num4 = (int) transaction.Commit();
          }
          view3D1 = view3D2;
          break;
        }
      }
      if (!flag3)
      {
        int num5 = (int) transaction.Start();
        XYZ eyePosition = new XYZ(0.0, 0.0, 10.0);
        view3D1 = View3D.CreateIsometric(revitDoc, viewFamilyType.Id);
        view3D1.Name = "EXPORT-VIEW";
        view3D1.SetOrientation(new ViewOrientation3D(eyePosition, XYZ.BasisY, XYZ.BasisZ.Negate()));
        view3D1.ViewTemplateId = ElementId.InvalidElementId;
        view3D1.Scale = 96 /*0x60*/;
        foreach (string key in dictionary3.Keys)
          view3D1.SetCategoryHidden(dictionary3[key].Id, false);
        List<ElementId> source3 = new List<ElementId>()
        {
          new ElementId(BuiltInCategory.OST_Lines)
        };
        source3.AddRange(dictionary3.Values.Select<Category, ElementId>((Func<Category, ElementId>) (v => v.Id)));
        List<ElementId> elementIdList4 = new List<ElementId>();
        foreach (Category category in categoryList)
        {
          ElementId catID = category.Id;
          if (!source3.Any<ElementId>((Func<ElementId, bool>) (c => c.Equals((object) catID))) && view3D1.CanCategoryBeHidden(catID))
            view3D1.SetCategoryHidden(catID, true);
        }
        int num6 = (int) transaction.Commit();
      }
    }
    using (TransactionGroup transactionGroup = new TransactionGroup(revitDoc))
    {
      int num7 = (int) transactionGroup.Start();
      List<string> stringList1 = new List<string>();
      List<AssemblyInstance> source4 = new List<AssemblyInstance>();
      try
      {
        List<ElementId> list = commandData.Application.ActiveUIDocument.Selection.GetElementIds().ToList<ElementId>();
        if (list.Count > 0)
        {
          foreach (ElementId id in list)
          {
            if (revitDoc.GetElement(id) is AssemblyInstance element)
              source4.Add(element);
          }
        }
        if (source4.Count == 0)
        {
          foreach (Reference pickObject in (IEnumerable<Reference>) commandData.Application.ActiveUIDocument.Selection.PickObjects((ObjectType) 1, (ISelectionFilter) new OnlyAssemblyInstances()))
          {
            if (revitDoc.GetElement(pickObject) is AssemblyInstance element)
              source4.Add(element);
          }
        }
      }
      catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
      {
        if (ex.Message.Contains("non-graphical"))
        {
          TaskDialog.Show("Error", "Current view is not valid for the CAD export. Please activate a graphical view before trying again.");
          App.laserExportErrorCheck(false);
          return (Result) 1;
        }
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        int num8 = (int) transactionGroup.RollBack();
        App.laserExportErrorCheck(false);
        return (Result) 0;
      }
      if (source4.Count < 1)
      {
        int num9 = (int) transactionGroup.RollBack();
        App.laserExportErrorCheck(false);
        return (Result) 1;
      }
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      List<string> list1 = source4.Select<AssemblyInstance, string>((Func<AssemblyInstance, string>) (x => x.Name)).ToList<string>();
      CADExportSettings settings = cadExportSettings.Clone();
      List<string> ElementMarks = list1;
      string projectName2 = projectName1;
      string number = revitDoc.ProjectInformation.Number;
      string manufacturer2 = manufacturer1;
      CADExportWindow cadExportWindow = new CADExportWindow(mainWindowHandle, settings, ElementMarks, projectName2, number, manufacturer2);
      cadExportWindow.ShowDialog();
      if (cadExportWindow.Cancel)
      {
        int num10 = (int) transactionGroup.RollBack();
        App.laserExportErrorCheck(false);
        return (Result) 1;
      }
      CADExportSettings newSettings = cadExportWindow.GetNewSettings();
      AutoTicketLIF locationInFormValues = LocationInFormAnalyzer.extractLocationInFormValues(revitDoc);
      List<string> stringList2 = new List<string>();
      List<string> stringList3 = new List<string>();
      foreach (AssemblyInstance assemblyInstance in source4)
      {
        LocationInFormAnalyzer locationInFormAnalyzer = (LocationInFormAnalyzer) null;
        Element structuralFramingElement = assemblyInstance.GetStructuralFramingElement();
        if (structuralFramingElement == null || Utils.ElementUtils.Parameters.GetParameterAsBool((Element) assemblyInstance, "HARDWARE_DETAIL"))
        {
          stringList2.Add(assemblyInstance.AssemblyTypeName);
        }
        else
        {
          using (Transaction transaction = new Transaction(revitDoc, "Generate Lines"))
          {
            int num11 = (int) transaction.Start();
            Transform transform = assemblyInstance.GetTransform();
            List<Solid> instanceSolids = Solids.GetInstanceSolids(structuralFramingElement);
            Solid solid1 = (Solid) null;
            foreach (Solid solid2 in instanceSolids)
            {
              if (solid2.Volume > 0.0)
                solid1 = !((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid2, solid1, BooleanOperationsType.Union) : SolidUtils.Clone(solid2);
            }
            Solid solid3 = SolidUtils.Clone(solid1);
            try
            {
              foreach (ElementId joinedElement in (IEnumerable<ElementId>) JoinGeometryUtils.GetJoinedElements(revitDoc, structuralFramingElement))
              {
                Element element = revitDoc.GetElement(joinedElement);
                if (element.Name.ToUpper().Contains("SOLID_ZONE"))
                {
                  foreach (Solid instanceSolid in Solids.GetInstanceSolids(element))
                    BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid1, instanceSolid, BooleanOperationsType.Union);
                }
              }
            }
            catch (Exception ex)
            {
              solid1 = solid3;
            }
            List<Tuple<CurveLoop, XYZ, bool>> tupleList1 = new List<Tuple<CurveLoop, XYZ, bool>>();
            List<Tuple<CurveLoop, XYZ>> upwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
            List<Tuple<CurveLoop, XYZ>> tupleList2 = new List<Tuple<CurveLoop, XYZ>>();
            double maxZ;
            double minZ;
            CAM_Utils.GetCurveLoops(solid1, tupleList1, upwardFacingCurves, tupleList2, out maxZ, out minZ, transform);
            List<CurveLoop> curveLoopList1 = new List<CurveLoop>();
            foreach (Tuple<CurveLoop, XYZ> tuple in tupleList1.Select<Tuple<CurveLoop, XYZ, bool>, Tuple<CurveLoop, XYZ>>((Func<Tuple<CurveLoop, XYZ, bool>, Tuple<CurveLoop, XYZ>>) (c => new Tuple<CurveLoop, XYZ>(c.Item1, c.Item2))).Union<Tuple<CurveLoop, XYZ>>((IEnumerable<Tuple<CurveLoop, XYZ>>) tupleList2))
            {
              CurveLoop original = tuple.Item1;
              XYZ point = tuple.Item2;
              if (original.HasPlane() && Util.IsParallel(original.GetPlane().Normal, transform.BasisZ) && transform.Inverse.OfPoint(point).Z.ApproximatelyEquals(minZ))
                curveLoopList1.Add(CurveLoop.CreateViaCopy(original));
            }
            Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary = new Dictionary<Solid, Tuple<string, double, bool>>();
            if (CAM_Utils.GetLayerDictionary(LayerDictionary, tupleList1, upwardFacingCurves, tupleList2, revitDoc, "", 0.0, minZ, maxZ, Math.Abs(maxZ - minZ), out bool _, transform))
            {
              Solid solid4 = (Solid) null;
              foreach (Solid key in LayerDictionary.Keys)
                solid4 = !((GeometryObject) solid4 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(key, solid4, BooleanOperationsType.Union) : SolidUtils.Clone(key);
              tupleList1.Clear();
              upwardFacingCurves.Clear();
              tupleList2.Clear();
              CAM_Utils.GetCurveLoops(solid4, tupleList1, upwardFacingCurves, tupleList2, out maxZ, out minZ, transform);
              List<CurveLoop> curveLoopList2 = new List<CurveLoop>();
              List<CurveLoop> curveLoopList3 = new List<CurveLoop>();
              if (newSettings.TIFLayerEnabled || newSettings.SIFLayerEnabled || newSettings.ArchLayerEnabled)
              {
                foreach (Tuple<CurveLoop, XYZ, bool> tuple in tupleList1)
                {
                  CurveLoop curveLoop = tuple.Item1;
                  if (transform.Inverse.OfPoint(tuple.Item2).Z.ApproximatelyEquals(maxZ))
                    curveLoopList2.Add(curveLoop);
                  else
                    curveLoopList3.Add(curveLoop);
                }
              }
              List<CurveLoop> second = new List<CurveLoop>();
              if (newSettings.TIFLayerEnabled)
              {
                foreach (Tuple<CurveLoop, XYZ> tuple in upwardFacingCurves)
                  second.Add(tuple.Item1);
              }
              if (newSettings.FormLayerEnabled || newSettings.ArchLayerEnabled)
              {
                foreach (Tuple<CurveLoop, XYZ> tuple in tupleList2)
                  curveLoopList3.Add(tuple.Item1);
              }
              List<CurveLoop> originalCurveLoopList1 = new List<CurveLoop>();
              List<CurveLoop> originalCurveLoopList2 = new List<CurveLoop>();
              List<CurveLoop> originalCurveLoopList3 = new List<CurveLoop>();
              List<CurveLoop> curveLoopList4 = new List<CurveLoop>();
              List<CurveLoop> originalCurveLoopList4 = new List<CurveLoop>();
              List<CurveLoop> curveLoopList5 = new List<CurveLoop>();
              List<CurveLoop> curveLoopList6 = new List<CurveLoop>();
              List<CurveLoop> curveLoopList7 = new List<CurveLoop>();
              List<CurveLoop> curveLoopList8 = new List<CurveLoop>();
              if (newSettings.ArchLayerEnabled)
              {
                List<Curve> curveList = new List<Curve>();
                foreach (CurveLoop curveLoop1 in curveLoopList1)
                {
                  foreach (Curve curve1 in curveLoop1)
                  {
                    bool flag = false;
                    foreach (CurveLoop curveLoop2 in curveLoopList3)
                    {
                      if (!curveLoop2.Equals((object) curveLoop1))
                      {
                        foreach (Curve curve2 in curveLoop2)
                        {
                          switch (curve1.Intersect(curve2, out IntersectionResultArray _))
                          {
                            case SetComparisonResult.Subset:
                            case SetComparisonResult.Superset:
                            case SetComparisonResult.Equal:
                              flag = true;
                              break;
                          }
                          if (flag)
                            break;
                        }
                        if (flag)
                          break;
                      }
                    }
                    if (!flag)
                      curveList.Add(curve1);
                  }
                }
                foreach (Curve curve in curveList)
                {
                  CurveLoop curveLoop = new CurveLoop();
                  curveLoop.Append(curve);
                  originalCurveLoopList4.Add(curveLoop);
                }
              }
              if (newSettings.TIFLayerEnabled || newSettings.BIFLayerEnabled || newSettings.SIFLayerEnabled || newSettings.CorbelLayerEnabled || newSettings.StrandLayerEnabled)
              {
                foreach (Element element in assemblyInstance.GetMemberIds().Select<ElementId, Element>((Func<ElementId, Element>) (id => revitDoc.GetElement(id))))
                {
                  Element member = element;
                  if (member.Id.IntegerValue != structuralFramingElement.Id.IntegerValue && member is FamilyInstance familyInstance)
                  {
                    if (Oracle.IsEmbed(member) || Oracle.IsLifting(member) && (newSettings.TIFLayerEnabled || newSettings.BIFLayerEnabled || newSettings.SIFLayerEnabled))
                    {
                      string parameterAsString = Utils.ElementUtils.Parameters.GetParameterAsString(member, "LOCATION_IN_FORM");
                      bool flag5 = false;
                      bool flag6 = false;
                      bool flag7 = false;
                      if (parameterAsString == locationInFormValues.TIF || parameterAsString == locationInFormValues.defaultTIF)
                        flag5 = true;
                      else if (parameterAsString == locationInFormValues.BIF || parameterAsString == locationInFormValues.defaultBIF)
                        flag6 = true;
                      else if (parameterAsString == locationInFormValues.SIF || parameterAsString == locationInFormValues.defaultSIF)
                      {
                        flag7 = true;
                      }
                      else
                      {
                        if (locationInFormAnalyzer == null)
                          locationInFormAnalyzer = new LocationInFormAnalyzer(assemblyInstance, 1.0);
                        if (locationInFormAnalyzer.ElementsInTopFaces.Any<ElementId>((Func<ElementId, bool>) (id => member.Id.Equals((object) id))))
                          flag5 = true;
                        else if (locationInFormAnalyzer.ElementsInSideFaces.Any<ElementId>((Func<ElementId, bool>) (id => member.Id.Equals((object) id))))
                          flag7 = true;
                        else if (locationInFormAnalyzer.ElementsInDownFaces.Any<ElementId>((Func<ElementId, bool>) (id => member.Id.Equals((object) id))))
                          flag6 = true;
                      }
                      if (flag5 | flag6 && (newSettings.TIFLayerEnabled || newSettings.BIFLayerEnabled))
                      {
                        List<List<Curve>> LIFLinesList;
                        List<List<Curve>> OppLIFLinesList;
                        if (newSettings.UseEdgeCadLines && this.getEdgeLIFLaserLines(member, out LIFLinesList, out OppLIFLinesList))
                        {
                          if (flag5)
                          {
                            if (newSettings.TIFLayerEnabled)
                            {
                              foreach (IList<Curve> curves in LIFLinesList)
                              {
                                CurveLoop curveLoop = CurveLoop.Create(curves);
                                curveLoopList6.Add(curveLoop);
                              }
                            }
                            if (newSettings.BIFLayerEnabled)
                            {
                              foreach (IList<Curve> curves in OppLIFLinesList)
                              {
                                CurveLoop curveLoop = CurveLoop.Create(curves);
                                curveLoopList7.Add(curveLoop);
                              }
                            }
                          }
                          else
                          {
                            if (newSettings.BIFLayerEnabled)
                            {
                              foreach (IList<Curve> curves in LIFLinesList)
                              {
                                CurveLoop curveLoop = CurveLoop.Create(curves);
                                curveLoopList7.Add(curveLoop);
                              }
                            }
                            if (newSettings.TIFLayerEnabled)
                            {
                              foreach (IList<Curve> curves in OppLIFLinesList)
                              {
                                CurveLoop curveLoop = CurveLoop.Create(curves);
                                curveLoopList6.Add(curveLoop);
                              }
                            }
                          }
                        }
                        else
                        {
                          CurveLoop bottomLoop = this.GetBottomLoop(member, transform);
                          if (bottomLoop != null)
                          {
                            if (flag5 && newSettings.TIFLayerEnabled)
                              originalCurveLoopList1.Add(CurveLoop.CreateViaTransform(bottomLoop, transform));
                            else if (flag6 && newSettings.BIFLayerEnabled)
                              originalCurveLoopList2.Add(CurveLoop.CreateViaTransform(bottomLoop, transform));
                          }
                        }
                      }
                      if (flag7 && newSettings.SIFLayerEnabled)
                      {
                        List<List<Curve>> SIFLinesList;
                        if (newSettings.UseEdgeCadLines && this.getEdgeSIFLaserLines(member, out SIFLinesList))
                        {
                          foreach (IList<Curve> curves in SIFLinesList)
                          {
                            CurveLoop curveLoop = CurveLoop.Create(curves);
                            curveLoopList8.Add(curveLoop);
                          }
                        }
                        else
                        {
                          CurveLoop bottomLoop = this.GetBottomLoop(member, transform);
                          if (bottomLoop != null)
                            originalCurveLoopList3.Add(CurveLoop.CreateViaTransform(bottomLoop, transform));
                        }
                      }
                    }
                    else if (newSettings.CorbelLayerEnabled && (familyInstance.Symbol.Name.ToUpper().Contains("CORBEL") || familyInstance.Symbol.FamilyName.ToUpper().Contains("CORBEL") || familyInstance.Symbol.Name.ToUpper().Contains("LEDGE") || familyInstance.Symbol.FamilyName.ToUpper().Contains("LEDGE")))
                    {
                      CurveLoop topLoop;
                      CurveLoop botLoop;
                      this.GetCorbelLoops(familyInstance, transform, out topLoop, out botLoop);
                      if (botLoop != null && topLoop != null && botLoop.First<Curve>().GetEndPoint(0).Z.ApproximatelyEquals(maxZ))
                        curveLoopList4.Add(CurveLoop.CreateViaTransform(topLoop, transform));
                    }
                    else if (newSettings.StrandLayerEnabled && familyInstance.Symbol.Name.ToUpper().Contains("STRAND"))
                    {
                      foreach (IList<Curve> curves in this.getStrandGeometry((Element) familyInstance))
                      {
                        CurveLoop curveLoop = CurveLoop.Create(curves);
                        curveLoopList5.Add(curveLoop);
                      }
                    }
                  }
                }
              }
              bool OverallSplines;
              Dictionary<CurveLoop, CurveLoop> flattenedToZ1 = CAM_Utils.GetFlattenedToZ(originalCurveLoopList1, transform, maxZ, 1.0 / 384.0, out OverallSplines);
              Dictionary<CurveLoop, CurveLoop> flattenedToZ2 = CAM_Utils.GetFlattenedToZ(originalCurveLoopList2, transform, minZ, 1.0 / 384.0, out OverallSplines);
              Dictionary<CurveLoop, CurveLoop> flattenedToZ3 = CAM_Utils.GetFlattenedToZ(originalCurveLoopList3, transform, maxZ, 1.0 / 384.0, out OverallSplines);
              Dictionary<double, List<CurveLoop>> dictionary4 = new Dictionary<double, List<CurveLoop>>();
              foreach (CurveLoop curveLoop in curveLoopList4)
              {
                double num12 = CurveLoop.CreateViaTransform(curveLoop, transform.Inverse).First<Curve>().GetEndPoint(0).Z - minZ;
                Tuple<CurveLoop, CurveLoop> flattenedToZ4 = CAM_Utils.GetFlattenedToZ(curveLoop, transform, num12, 1.0 / 384.0, out OverallSplines);
                if (dictionary4.ContainsKey(num12))
                  dictionary4[num12].Add(flattenedToZ4.Item2);
                else
                  dictionary4.Add(num12, new List<CurveLoop>()
                  {
                    flattenedToZ4.Item2
                  });
              }
              Dictionary<CurveLoop, CurveLoop> flattenedToZ5 = CAM_Utils.GetFlattenedToZ(originalCurveLoopList4, transform, minZ, 1.0 / 384.0, out OverallSplines);
              SketchPlane sketchPlane1 = (SketchPlane) null;
              if (newSettings.TIFLayerEnabled || newSettings.SIFLayerEnabled)
              {
                Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0.0, 0.0, Math.Abs(maxZ - minZ)));
                sketchPlane1 = SketchPlane.Create(revitDoc, byNormalAndOrigin);
              }
              SketchPlane sketchPlane2 = (SketchPlane) null;
              if (newSettings.BIFLayerEnabled || newSettings.ArchLayerEnabled || newSettings.FormLayerEnabled)
              {
                Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                sketchPlane2 = SketchPlane.Create(revitDoc, byNormalAndOrigin);
              }
              List<ModelCurveArray> mca1 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca2 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca3 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca4 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca5 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca6 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca7 = new List<ModelCurveArray>();
              transform.Origin += transform.BasisZ * minZ;
              List<List<ModelCurveArray>> mcaLists = new List<List<ModelCurveArray>>();
              if (newSettings.CorbelLayerEnabled)
              {
                foreach (double key in dictionary4.Keys)
                {
                  Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0.0, 0.0, key));
                  SketchPlane sketchPlane3 = SketchPlane.Create(revitDoc, byNormalAndOrigin);
                  foreach (CurveLoop loop in dictionary4[key])
                  {
                    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(loop, transform);
                    ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(LaserUtils.OverkillCurveArray(curveArrayFromLoop), sketchPlane3);
                    mca6.Add(modelCurveArray);
                  }
                }
                LaserUtils.AssignLineStyle(mca6, dictionary3[layerDict[CADExportSettings.ExportGroup.Corbel].LayerName], revitDoc);
                mcaLists.Add(mca6);
              }
              if (newSettings.TIFLayerEnabled)
              {
                foreach (CurveLoop loop in curveLoopList2.Union<CurveLoop>((IEnumerable<CurveLoop>) second))
                {
                  CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(loop, transform);
                  ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(LaserUtils.OverkillCurveArray(curveArrayFromLoop), sketchPlane1);
                  mca2.Add(modelCurveArray);
                }
                foreach (CurveLoop key in flattenedToZ1.Keys)
                {
                  if (newSettings.TIFExportBoundingBox)
                  {
                    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform);
                    ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(curveArrayFromLoop, sketchPlane1);
                    mca2.Add(modelCurveArray);
                  }
                  else if (newSettings.TIFExportFixedCircle)
                  {
                    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform);
                    double val2_1 = double.MaxValue;
                    double val2_2 = double.MaxValue;
                    double val2_3 = double.MinValue;
                    double val2_4 = double.MinValue;
                    foreach (Curve curve in curveArrayFromLoop)
                    {
                      val2_1 = Math.Min(curve.GetEndPoint(0).X, val2_1);
                      val2_2 = Math.Min(curve.GetEndPoint(0).Y, val2_2);
                      val2_3 = Math.Max(curve.GetEndPoint(0).X, val2_3);
                      val2_4 = Math.Max(curve.GetEndPoint(0).Y, val2_4);
                    }
                    XYZ origin = new XYZ((val2_3 + val2_1) / 2.0, (val2_4 + val2_2) / 2.0, Math.Abs(maxZ - minZ));
                    CurveArray geometryCurveArray = new CurveArray();
                    geometryCurveArray.Append((Curve) Arc.Create(Plane.CreateByNormalAndOrigin(XYZ.BasisZ, origin), newSettings.fixedCircleRadiusInternalUnits, 0.0, 2.0 * Math.PI));
                    ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(geometryCurveArray, sketchPlane1);
                    mca2.Add(modelCurveArray);
                  }
                }
                foreach (CurveLoop curveLoop in curveLoopList6)
                {
                  ModelCurveArray modelCurveArray = this.get3DModelCurveArray(revitDoc, curveLoop, transform);
                  if (modelCurveArray != null)
                    mca2.Add(modelCurveArray);
                }
                LaserUtils.AssignLineStyle(mca2, dictionary3[layerDict[CADExportSettings.ExportGroup.TIF].LayerName], revitDoc);
                mcaLists.Add(mca2);
              }
              if (newSettings.SIFLayerEnabled)
              {
                foreach (Curve sifLine in this.FindSIFLines(flattenedToZ3.Keys.Select<CurveLoop, CurveLoop>((Func<CurveLoop, CurveLoop>) (l => CurveLoop.CreateViaTransform(l, transform.Inverse))).ToList<CurveLoop>(), curveLoopList2, transform, maxZ, minZ, newSettings.tickMarkLengthInternalUnits))
                {
                  CurveArray geometryCurveArray = new CurveArray();
                  geometryCurveArray.Append(sifLine);
                  ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(geometryCurveArray, sketchPlane1);
                  mca5.Add(modelCurveArray);
                }
                foreach (CurveLoop curveLoop in curveLoopList8)
                {
                  ModelCurveArray modelCurveArray = this.get3DModelCurveArray(revitDoc, curveLoop, transform);
                  if (modelCurveArray != null)
                    mca5.Add(modelCurveArray);
                }
                LaserUtils.AssignLineStyle(mca5, dictionary3[layerDict[CADExportSettings.ExportGroup.SIF].LayerName], revitDoc);
                mcaLists.Add(mca5);
              }
              if (newSettings.FormLayerEnabled)
              {
                foreach (CurveLoop loop in curveLoopList3)
                {
                  CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(loop, transform);
                  ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(LaserUtils.OverkillCurveArray(curveArrayFromLoop), sketchPlane2);
                  mca1.Add(modelCurveArray);
                }
                LaserUtils.AssignLineStyle(mca1, dictionary3[layerDict[CADExportSettings.ExportGroup.Form].LayerName], revitDoc);
                mcaLists.Add(mca1);
              }
              if (newSettings.ArchLayerEnabled)
              {
                foreach (CurveLoop key in flattenedToZ5.Keys)
                {
                  CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform);
                  ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(LaserUtils.OverkillCurveArray(curveArrayFromLoop), sketchPlane2);
                  mca4.Add(modelCurveArray);
                }
                LaserUtils.AssignLineStyle(mca4, dictionary3[layerDict[CADExportSettings.ExportGroup.Arch].LayerName], revitDoc);
                mcaLists.Add(mca4);
              }
              if (newSettings.BIFLayerEnabled)
              {
                foreach (CurveLoop key in flattenedToZ2.Keys)
                {
                  if (newSettings.BIFExportBoundingBox)
                  {
                    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform);
                    ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(curveArrayFromLoop, sketchPlane2);
                    mca3.Add(modelCurveArray);
                  }
                  else if (newSettings.BIFExportFixedCircle)
                  {
                    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform);
                    double val2_5 = double.MaxValue;
                    double val2_6 = double.MaxValue;
                    double val2_7 = double.MinValue;
                    double val2_8 = double.MinValue;
                    foreach (Curve curve in curveArrayFromLoop)
                    {
                      val2_5 = Math.Min(curve.GetEndPoint(0).X, val2_5);
                      val2_6 = Math.Min(curve.GetEndPoint(0).Y, val2_6);
                      val2_7 = Math.Max(curve.GetEndPoint(0).X, val2_7);
                      val2_8 = Math.Max(curve.GetEndPoint(0).Y, val2_8);
                    }
                    XYZ origin = new XYZ((val2_7 + val2_5) / 2.0, (val2_8 + val2_6) / 2.0, 0.0);
                    CurveArray geometryCurveArray = new CurveArray();
                    geometryCurveArray.Append((Curve) Arc.Create(Plane.CreateByNormalAndOrigin(XYZ.BasisZ, origin), newSettings.fixedCircleRadiusInternalUnits, 0.0, 2.0 * Math.PI));
                    ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(geometryCurveArray, sketchPlane2);
                    mca3.Add(modelCurveArray);
                  }
                }
                foreach (CurveLoop curveLoop in curveLoopList7)
                {
                  ModelCurveArray modelCurveArray = this.get3DModelCurveArray(revitDoc, curveLoop, transform);
                  if (modelCurveArray != null)
                    mca3.Add(modelCurveArray);
                }
                LaserUtils.AssignLineStyle(mca3, dictionary3[layerDict[CADExportSettings.ExportGroup.BIF].LayerName], revitDoc);
                mcaLists.Add(mca3);
              }
              if (newSettings.StrandLayerEnabled)
              {
                foreach (CurveLoop curveLoop in curveLoopList5)
                {
                  ModelCurveArray modelCurveArray = this.get3DModelCurveArray(revitDoc, curveLoop, transform);
                  if (modelCurveArray != null)
                    mca7.Add(modelCurveArray);
                }
                LaserUtils.AssignLineStyle(mca7, dictionary3[layerDict[CADExportSettings.ExportGroup.Strand].LayerName], revitDoc);
                mcaLists.Add(mca7);
              }
              List<ElementId> elementIdList5 = LaserUtils.CollectCurveIds(mcaLists);
              foreach (ElementId filter in (IEnumerable<ElementId>) view3D1.GetFilters())
                view3D1.RemoveFilter(filter);
              view3D1.IsolateElementsTemporary((ICollection<ElementId>) elementIdList5);
              view3D1.ConvertTemporaryHideIsolateToPermanent();
              view3D1.SetOrientation(new ViewOrientation3D(new XYZ(0.0, 0.0, 10.0), XYZ.BasisY, XYZ.BasisZ.Negate()));
            }
            int num13 = (int) transaction.Commit();
            if (exportDwgSettings1 != null)
            {
              string shortFilePath = newSettings.GetShortFilePath(projectName1, revitDoc.ProjectInformation.Number, true);
              LaserUtils.CheckDirectory(shortFilePath);
              string fileName = newSettings.GetFileName(assemblyInstance.Name, revitDoc.ProjectInformation.Number, true);
              if (newSettings.CADExportDWGFiles)
              {
                DateTime t1 = DateTime.Now.AddMilliseconds(-1000.0);
                Document document = revitDoc;
                string folder = shortFilePath;
                string name = fileName;
                List<ElementId> views = new List<ElementId>();
                views.Add(view3D1.Id);
                DWGExportOptions dwgExportOptions = exportDwgSettings1.GetDWGExportOptions();
                if (document.Export(folder, name, (ICollection<ElementId>) views, dwgExportOptions))
                {
                  if (File.Exists($"{shortFilePath}\\{fileName}.dwg"))
                  {
                    FileInfo fileInfo = new FileInfo($"{shortFilePath}\\{fileName}.dwg");
                    if (fileInfo != null)
                    {
                      DateTime lastWriteTime = fileInfo.LastWriteTime;
                    }
                    else
                      stringList1.Add(fileName + ".dwg");
                    if (DateTime.Compare(t1, fileInfo.LastWriteTime) <= 0)
                      stringList1.Add(fileName + ".dwg");
                    else
                      stringList3.Add(fileName + ".dwg");
                  }
                  else
                    stringList3.Add(fileName + ".dwg");
                }
                else
                  stringList3.Add(fileName + ".dwg");
              }
              if (newSettings.CADExportDXFFiles)
              {
                DateTime t1 = DateTime.Now.AddMilliseconds(-1000.0);
                Document document = revitDoc;
                string folder = shortFilePath;
                string name = fileName;
                List<ElementId> views = new List<ElementId>();
                views.Add(view3D1.Id);
                DXFExportOptions dxfExportOptions = exportDwgSettings1.GetDXFExportOptions();
                if (document.Export(folder, name, (ICollection<ElementId>) views, dxfExportOptions))
                {
                  if (File.Exists($"{shortFilePath}\\{fileName}.dxf"))
                  {
                    FileInfo fileInfo = new FileInfo($"{shortFilePath}\\{fileName}.dxf");
                    if (fileInfo != null)
                    {
                      DateTime lastWriteTime = fileInfo.LastWriteTime;
                    }
                    else
                      stringList1.Add(fileName + ".dxf");
                    if (DateTime.Compare(t1, fileInfo.LastWriteTime) <= 0)
                      stringList1.Add(fileName + ".dxf");
                    else
                      stringList3.Add(fileName + ".dxf");
                  }
                  else
                    stringList3.Add(fileName + ".dxf");
                }
                else
                  stringList3.Add(fileName + ".dxf");
              }
            }
          }
        }
      }
      int num14 = (int) transactionGroup.RollBack();
      if (stringList2.Count > 0)
      {
        TaskDialog taskDialog1 = new TaskDialog("CAD Export");
        taskDialog1.MainContent = "One or more assemblies was invalid for Laser Export. CAD Export only supports structural framing assemblies. Expand for details.";
        taskDialog1.ExpandedContent = "These assemblies were not exported: \n";
        foreach (string str in stringList2)
        {
          TaskDialog taskDialog2 = taskDialog1;
          taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str}, ";
        }
        taskDialog1.Show();
      }
      if (stringList3.Count > 0)
      {
        TaskDialog taskDialog3 = new TaskDialog("CAD Export");
        taskDialog3.MainContent = $"CAD Export was unable to export one or more CAD files to {newSettings.GetShortFilePath(projectName1, revitDoc.ProjectInformation.Number, true)}. Expand for details.";
        taskDialog3.ExpandedContent = "";
        foreach (string str in stringList3)
        {
          TaskDialog taskDialog4 = taskDialog3;
          taskDialog4.ExpandedContent = taskDialog4.ExpandedContent + str + Environment.NewLine;
        }
        taskDialog3.Show();
      }
      if (stringList1.Count > 0)
      {
        TaskDialog taskDialog5 = new TaskDialog("CAD Export");
        taskDialog5.MainContent = $"CAD files successfully exported to {newSettings.GetShortFilePath(projectName1, revitDoc.ProjectInformation.Number, true)}. Expand for details.";
        taskDialog5.ExpandedContent = "";
        foreach (string str in stringList1)
        {
          TaskDialog taskDialog6 = taskDialog5;
          taskDialog6.ExpandedContent = taskDialog6.ExpandedContent + str + Environment.NewLine;
        }
        taskDialog5.Show();
      }
    }
    App.laserExportErrorCheck(false);
    return (Result) 0;
  }

  private CurveLoop LoopFromBBox(BoundingBoxXYZ bbox)
  {
    if (bbox == null)
      return (CurveLoop) null;
    double num = 1.0 / 384.0;
    XYZ min = bbox.Min;
    XYZ max = bbox.Max;
    XYZ xyz1 = new XYZ(min.X, min.Y, min.Z);
    XYZ xyz2 = new XYZ(max.X, min.Y, min.Z);
    XYZ xyz3 = new XYZ(max.X, max.Y, min.Z);
    XYZ xyz4 = new XYZ(min.X, max.Y, min.Z);
    CurveLoop curveLoop = new CurveLoop();
    if (xyz1.DistanceTo(xyz2) < num || xyz2.DistanceTo(xyz3) < num || xyz3.DistanceTo(xyz4) < num || xyz4.DistanceTo(xyz1) < num)
      return (CurveLoop) null;
    curveLoop.Append((Curve) Line.CreateBound(xyz1, xyz2));
    curveLoop.Append((Curve) Line.CreateBound(xyz2, xyz3));
    curveLoop.Append((Curve) Line.CreateBound(xyz3, xyz4));
    curveLoop.Append((Curve) Line.CreateBound(xyz4, xyz1));
    return curveLoop;
  }

  private void GetCorbelLoops(
    FamilyInstance fI,
    Transform transform,
    out CurveLoop topLoop,
    out CurveLoop botLoop)
  {
    topLoop = (CurveLoop) null;
    botLoop = (CurveLoop) null;
    bool bSymbol;
    List<Solid> symbolSolids = Solids.GetSymbolSolids((Element) fI, out bSymbol);
    Solid solid0 = (Solid) null;
    foreach (Solid solid1 in symbolSolids)
    {
      Solid solid2 = SolidUtils.Clone(solid1);
      if (bSymbol)
        solid2 = SolidUtils.CreateTransformed(solid2, fI.GetTransform());
      Solid transformed = SolidUtils.CreateTransformed(solid2, transform.Inverse);
      solid0 = !((GeometryObject) solid0 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid0, transformed, BooleanOperationsType.Union) : SolidUtils.Clone(transformed);
    }
    PlanarFace planarFace1 = (PlanarFace) null;
    PlanarFace planarFace2 = (PlanarFace) null;
    foreach (Face face in solid0.Faces)
    {
      bool flag = false;
      if (face is PlanarFace planarFace3 && (planarFace3.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ) || planarFace3.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate())))
      {
        double z = planarFace3.Origin.Z;
        if ((GeometryObject) planarFace1 == (GeometryObject) null)
        {
          planarFace1 = planarFace3;
          flag = true;
        }
        if ((GeometryObject) planarFace2 == (GeometryObject) null)
        {
          planarFace2 = planarFace3;
          flag = true;
        }
        if (!flag)
        {
          if (z > planarFace1.Origin.Z)
            planarFace1 = planarFace3;
          if (z < planarFace2.Origin.Z)
            planarFace2 = planarFace3;
        }
      }
    }
    if (!((GeometryObject) planarFace1 != (GeometryObject) null) || !((GeometryObject) planarFace2 != (GeometryObject) null))
      return;
    topLoop = planarFace1.GetEdgesAsCurveLoops().First<CurveLoop>();
    botLoop = planarFace2.GetEdgesAsCurveLoops().First<CurveLoop>();
  }

  private List<Curve> FindSIFLines(
    List<CurveLoop> sifs,
    List<CurveLoop> outerLoops,
    Transform transform,
    double maxZ,
    double minZ,
    double tickLength)
  {
    List<Curve> sifLines = new List<Curve>();
    foreach (CurveLoop sif in sifs)
    {
      double x1 = double.MaxValue;
      double y1 = double.MaxValue;
      double x2 = double.MinValue;
      double y2 = double.MinValue;
      foreach (Curve curve in sif)
      {
        XYZ endPoint = curve.GetEndPoint(0);
        if (endPoint.X < x1)
          x1 = endPoint.X;
        if (endPoint.Y < y1)
          y1 = endPoint.Y;
        if (endPoint.X > x2)
          x2 = endPoint.X;
        if (endPoint.Y > y2)
          y2 = endPoint.Y;
      }
      XYZ point = new XYZ((x1 + x2) / 2.0, (y1 + y2) / 2.0, Math.Abs(maxZ - minZ));
      Curve curve1 = (Curve) null;
      XYZ endpoint2_1 = new XYZ((x1 + x2) / 2.0, y2, Math.Abs(maxZ - minZ));
      XYZ endpoint1_1 = new XYZ((x1 + x2) / 2.0, y1, Math.Abs(maxZ - minZ));
      XYZ endpoint1_2 = new XYZ(x1, (y1 + y2) / 2.0, Math.Abs(maxZ - minZ));
      XYZ endpoint2_2 = new XYZ(x2, (y1 + y2) / 2.0, Math.Abs(maxZ - minZ));
      List<Line> source = new List<Line>()
      {
        Line.CreateBound(endpoint1_1, endpoint2_1),
        Line.CreateBound(endpoint1_2, endpoint2_2)
      };
      foreach (CurveLoop outerLoop in outerLoops)
      {
        foreach (Curve curve2 in CurveLoop.CreateViaTransform(outerLoop, transform.Inverse))
        {
          Curve outCv = curve2;
          if (source.Any<Line>((Func<Line, bool>) (c => c.Intersect(outCv) != SetComparisonResult.Disjoint)))
          {
            IntersectionResult intersectionResult = outCv.Project(point);
            double normalizedParameter = outCv.ComputeNormalizedParameter(intersectionResult.Parameter);
            if (normalizedParameter <= 1.0 && normalizedParameter >= 0.0)
            {
              curve1 = outCv;
              break;
            }
          }
        }
        if ((GeometryObject) curve1 != (GeometryObject) null)
          break;
      }
      double num = double.MaxValue;
      XYZ endpoint1_3 = XYZ.Zero;
      XYZ xyz1 = XYZ.Zero;
      foreach (CurveLoop outerLoop in outerLoops)
      {
        CurveLoop viaTransform = CurveLoop.CreateViaTransform(outerLoop, transform.Inverse);
        foreach (Curve curve3 in viaTransform)
        {
          Curve curve4 = (GeometryObject) curve1 != (GeometryObject) null ? curve1 : curve3;
          IntersectionResult intersectionResult = curve4.Project(point);
          double normalizedParameter = curve4.ComputeNormalizedParameter(intersectionResult.Parameter);
          if (normalizedParameter <= 1.0 && normalizedParameter >= 0.0)
          {
            if (intersectionResult.Distance < num)
            {
              num = intersectionResult.Distance;
              endpoint1_3 = intersectionResult.XYZPoint;
              xyz1 = (point - endpoint1_3).Normalize();
            }
            if (intersectionResult.Distance.ApproximatelyEquals(0.0) || (endpoint1_3 - point).Normalize().IsAlmostEqualTo(XYZ.Zero))
            {
              XYZ zero = XYZ.Zero;
              xyz1 = !(curve4 is Line line) ? viaTransform.GetPlane().Normal.CrossProduct(curve4.GetEndPoint(1) - curve4.GetEndPoint(0)) : viaTransform.GetPlane().Normal.CrossProduct(line.Direction);
            }
            if ((GeometryObject) curve1 != (GeometryObject) null)
              break;
          }
        }
        if (!((GeometryObject) curve1 != (GeometryObject) null))
        {
          if (!CAM_Utils.CurveLoopContainsPoint(viaTransform, point))
            xyz1 = xyz1.Negate();
        }
        else
          break;
      }
      XYZ xyz2 = xyz1.Negate();
      sifLines.Add((Curve) Line.CreateBound(endpoint1_3, endpoint1_3 + xyz2 * tickLength));
    }
    return sifLines;
  }

  private List<List<Curve>> getStrandGeometry(Element strand)
  {
    Document document = strand.Document;
    List<Curve> curves = new List<Curve>();
    XYZ q = (XYZ) null;
    Face face1 = (Face) null;
    XYZ source1 = XYZ.Zero;
    XYZ source2 = XYZ.Zero;
    foreach (Solid instanceSolid in Solids.GetInstanceSolids(strand))
    {
      if (instanceSolid.Faces.Size == 4)
      {
        int num1 = 0;
        int num2 = 0;
        List<PlanarFace> planarFaceList = new List<PlanarFace>();
        foreach (Face face2 in instanceSolid.Faces)
        {
          if (face2 is CylindricalFace)
            ++num1;
          if (face2 is PlanarFace)
          {
            ++num2;
            planarFaceList.Add(face2 as PlanarFace);
          }
        }
        if (num1 == 2 && num2 == 2)
        {
          Arc arc1 = planarFaceList[0].GetEdgesAsCurveLoops().First<CurveLoop>().First<Curve>() as Arc;
          Arc arc2 = planarFaceList[1].GetEdgesAsCurveLoops().First<CurveLoop>().First<Curve>() as Arc;
          XYZ center1 = arc1.Center;
          XYZ center2 = arc2.Center;
          curves.Add((Curve) Line.CreateBound(center1, center2));
          continue;
        }
      }
      foreach (Face face3 in instanceSolid.Faces)
      {
        if (face3 is RevolvedFace)
        {
          if (!(face1 is RevolvedFace) || !(face1 as RevolvedFace).Origin.IsAlmostEqualTo((face3 as RevolvedFace).Origin))
          {
            RevolvedFace revolvedFace = face3 as RevolvedFace;
            XYZ end0 = new XYZ();
            XYZ xyz1 = new XYZ();
            XYZ pointOnArc = new XYZ();
            double num = 0.0;
            XYZ xyz2 = new XYZ();
            List<CurveLoop> list1 = revolvedFace.GetEdgesAsCurveLoops().ToList<CurveLoop>();
            bool flag = false;
            foreach (Curve curve1 in list1.FirstOrDefault<CurveLoop>())
            {
              Curve curve2 = curve1;
              if (curve1 is HermiteSpline)
              {
                List<XYZ> list2 = curve1.Tessellate().ToList<XYZ>();
                curve2 = (Curve) Arc.Create(list2[0], list2.LastOrDefault<XYZ>(), curve1.Evaluate(0.5, true));
              }
              if (curve2 is Arc)
              {
                Arc arc = curve2 as Arc;
                if (arc.Normal.CrossProduct(revolvedFace.Axis).IsAlmostEqualTo(XYZ.Zero))
                  pointOnArc = arc.Evaluate(0.5, true);
                else if (!flag)
                {
                  end0 = arc.Center;
                  flag = true;
                }
                else if (!end0.IsAlmostEqualTo(arc.Center) && end0.DistanceTo(arc.Center) > num)
                {
                  xyz1 = arc.Center;
                  num = end0.DistanceTo(xyz1);
                }
              }
            }
            Curve curve = (Curve) Arc.Create(end0, xyz1, pointOnArc);
            if ((GeometryObject) curve != (GeometryObject) null)
            {
              q = (XYZ) null;
              curves.Add(curve);
            }
          }
          else
            continue;
        }
        if (face3 is CylindricalFace)
        {
          CylindricalFace cylindricalFace = face3 as CylindricalFace;
          double num = 0.0;
          XYZ source3 = new XYZ();
          XYZ source4 = new XYZ();
          foreach (Curve curve in cylindricalFace.GetEdgesAsCurveLoops().First<CurveLoop>())
          {
            if (curve is Line && Math.Abs(curve.ApproximateLength) > num)
            {
              num = Math.Abs(curve.ApproximateLength);
              source3 = curve.GetEndPoint(0);
              source4 = curve.GetEndPoint(1);
            }
          }
          XYZ origin = cylindricalFace.Origin;
          XYZ xyz3 = new XYZ();
          XYZ xyz4 = origin.DistanceTo(source3) >= origin.DistanceTo(source4) ? (source3 - source4).Normalize() : (source4 - source3).Normalize();
          XYZ endpoint2 = cylindricalFace.Origin + xyz4 * num;
          if (q == null || !Util.IsParallel(cylindricalFace.Axis, q) || (!endpoint2.IsAlmostEqualTo(source2) || !origin.IsAlmostEqualTo(source1)) && (!origin.IsAlmostEqualTo(source2) || !endpoint2.IsAlmostEqualTo(source1)))
          {
            curves.Add((Curve) Line.CreateBound(origin, endpoint2));
            source1 = origin;
            source2 = endpoint2;
            q = cylindricalFace.Axis;
          }
          else
            continue;
        }
        face1 = face3;
      }
    }
    Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary = new Dictionary<int, Tuple<XYZ, XYZ>>();
    List<List<int>> indexLinesInOrder = new List<List<int>>();
    this.CreateLineGroups(curves, out indexLinesInOrder, out orderedlinesDictionary, true);
    return this.GetCurvesListList(curves, orderedlinesDictionary, indexLinesInOrder);
  }

  private bool getEdgeLIFLaserLines(
    Element element,
    out List<List<Curve>> LIFLinesList,
    out List<List<Curve>> OppLIFLinesList)
  {
    return this.getEdgeLaserLines(element, out LIFLinesList, out OppLIFLinesList, out List<List<Curve>> _, SIF: false);
  }

  private bool getEdgeSIFLaserLines(Element element, out List<List<Curve>> SIFLinesList)
  {
    return this.getEdgeLaserLines(element, out List<List<Curve>> _, out List<List<Curve>> _, out SIFLinesList, false);
  }

  private bool getEdgeLaserLines(
    Element element,
    out List<List<Curve>> LIFLinesList,
    out List<List<Curve>> OppLIFLinesList,
    out List<List<Curve>> SIFLinesList,
    bool LIF = true,
    bool SIF = true)
  {
    LIFLinesList = new List<List<Curve>>();
    OppLIFLinesList = new List<List<Curve>>();
    SIFLinesList = new List<List<Curve>>();
    List<Curve> curves1 = new List<Curve>();
    List<Curve> curves2 = new List<Curve>();
    List<Curve> curves3 = new List<Curve>();
    Document document = element.Document;
    if (!(element is FamilyInstance familyInstance))
      return false;
    Options options = document.Application.Create.NewGeometryOptions();
    if (options != null)
    {
      options.ComputeReferences = true;
      options.DetailLevel = ViewDetailLevel.Fine;
      options.IncludeNonVisibleObjects = true;
    }
    GeometryElement source = element.get_Geometry(options);
    GeometryInstance geometryInstance = source.First<GeometryObject>() as GeometryInstance;
    if ((GeometryObject) geometryInstance == (GeometryObject) null)
    {
      foreach (GeometryObject geometryObject in source)
      {
        if (geometryObject is GeometryInstance)
        {
          geometryInstance = geometryObject as GeometryInstance;
          break;
        }
      }
    }
    if ((GeometryObject) geometryInstance != (GeometryObject) null)
    {
      GeometryElement symbolGeometry = geometryInstance.GetSymbolGeometry();
      if ((GeometryObject) symbolGeometry != (GeometryObject) null)
      {
        foreach (GeometryObject geometryObject in symbolGeometry)
        {
          if (geometryObject is Curve)
          {
            Curve curve = geometryObject as Curve;
            ElementId graphicsStyleId = curve.GraphicsStyleId;
            Element element1 = document.GetElement(graphicsStyleId);
            if (LIF && element1.Name == "EdgeLIFCADLines")
              curves1.Add(curve.CreateTransformed(familyInstance.GetTransform()));
            else if (LIF && element1.Name == "EdgeOppLIFCADLines")
              curves2.Add(curve.CreateTransformed(familyInstance.GetTransform()));
            else if (SIF && element1.Name == "EdgeSIFCADLines")
              curves3.Add(curve.CreateTransformed(familyInstance.GetTransform()));
          }
        }
      }
    }
    if (curves1.Count == 0 && curves2.Count == 0 && curves3.Count == 0)
      return false;
    if (curves1.Count > 0)
    {
      List<List<int>> indexLinesInOrder;
      Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary;
      this.CreateLineGroups(curves1, out indexLinesInOrder, out orderedlinesDictionary);
      LIFLinesList = this.GetCurvesListList(curves1, orderedlinesDictionary, indexLinesInOrder);
    }
    if (curves2.Count > 0)
    {
      List<List<int>> indexLinesInOrder;
      Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary;
      this.CreateLineGroups(curves2, out indexLinesInOrder, out orderedlinesDictionary);
      OppLIFLinesList = this.GetCurvesListList(curves2, orderedlinesDictionary, indexLinesInOrder);
    }
    if (curves3.Count > 0)
    {
      List<List<int>> indexLinesInOrder;
      Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary;
      this.CreateLineGroups(curves3, out indexLinesInOrder, out orderedlinesDictionary);
      SIFLinesList = this.GetCurvesListList(curves3, orderedlinesDictionary, indexLinesInOrder);
    }
    return true;
  }

  private void CreateLineGroups(
    List<Curve> curves,
    out List<List<int>> indexLinesInOrder,
    out Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary,
    bool strand = false)
  {
    orderedlinesDictionary = new Dictionary<int, Tuple<XYZ, XYZ>>();
    indexLinesInOrder = new List<List<int>>();
    List<Curve> curveList = new List<Curve>();
    foreach (Curve curve in curves)
    {
      if (curve.IsBound)
      {
        curveList.Add(curve);
      }
      else
      {
        int key = curves.IndexOf(curve);
        indexLinesInOrder.Add(new List<int>() { key });
        orderedlinesDictionary.Add(key, new Tuple<XYZ, XYZ>((XYZ) null, (XYZ) null));
      }
    }
    foreach (Curve curve1 in curveList)
    {
      int cvIndex = curves.IndexOf(curve1);
      if (!orderedlinesDictionary.ContainsKey(cvIndex) || orderedlinesDictionary[cvIndex].Item2 == null)
      {
        XYZ endPoint1 = curve1.GetEndPoint(0);
        XYZ endPoint2 = curve1.GetEndPoint(1);
        foreach (Curve curve2 in curveList)
        {
          if (!curve2.Equals((object) curve1))
          {
            int cvIndex2 = curves.IndexOf(curve2);
            if (!indexLinesInOrder.Where<List<int>>((Func<List<int>, bool>) (x => x.Contains(cvIndex) && x.Contains(cvIndex2))).Any<List<int>>() && (!orderedlinesDictionary.ContainsKey(cvIndex2) || orderedlinesDictionary[cvIndex2].Item2 == null))
            {
              XYZ endPoint3 = curve2.GetEndPoint(0);
              XYZ endPoint4 = curve2.GetEndPoint(1);
              if (endPoint3.IsAlmostEqualTo(endPoint1) && !endPoint4.IsAlmostEqualTo(endPoint2) || endPoint3.IsAlmostEqualTo(endPoint2) && !endPoint4.IsAlmostEqualTo(endPoint1))
                this.UpdateLineDictionary(cvIndex, cvIndex2, endPoint3, orderedlinesDictionary, indexLinesInOrder);
              else if (endPoint4.IsAlmostEqualTo(endPoint1) && !endPoint3.IsAlmostEqualTo(endPoint2) || endPoint4.IsAlmostEqualTo(endPoint2) && !endPoint3.IsAlmostEqualTo(endPoint1))
                this.UpdateLineDictionary(cvIndex, cvIndex2, endPoint4, orderedlinesDictionary, indexLinesInOrder);
            }
          }
        }
      }
    }
    if (orderedlinesDictionary.Count == 0 & strand)
    {
      foreach (Curve curve3 in curveList)
      {
        int cvIndex = curves.IndexOf(curve3);
        if (!orderedlinesDictionary.ContainsKey(cvIndex) || orderedlinesDictionary[cvIndex].Item2 == null)
        {
          XYZ endPoint5 = curve3.GetEndPoint(0);
          XYZ endPoint6 = curve3.GetEndPoint(1);
          int cvIndex2_1 = -1;
          double num1 = double.MaxValue;
          XYZ endPoint7 = (XYZ) null;
          foreach (Curve curve4 in curves)
          {
            if (!curve4.Equals((object) curve3))
            {
              int cvIndex2 = curves.IndexOf(curve4);
              IntersectionResultArray resultArray;
              if ((!orderedlinesDictionary.ContainsKey(cvIndex2) || orderedlinesDictionary[cvIndex2].Item2 == null) && !indexLinesInOrder.Where<List<int>>((Func<List<int>, bool>) (x => x.Contains(cvIndex) && x.Contains(cvIndex2))).Any<List<int>>() && curve3.Intersect(curve4, out resultArray) == SetComparisonResult.Overlap && resultArray.Size <= 1)
              {
                XYZ source = (XYZ) null;
                foreach (object obj in resultArray)
                {
                  if (obj is IntersectionResult intersectionResult)
                    source = intersectionResult.XYZPoint;
                }
                double num2 = endPoint5.DistanceTo(source);
                double num3 = endPoint6.DistanceTo(source);
                double num4 = num2 >= num3 ? num3 : num2;
                if (num4 < num1)
                {
                  cvIndex2_1 = cvIndex2;
                  num1 = num4;
                  endPoint7 = source;
                }
              }
            }
          }
          if (endPoint7 != null)
            this.UpdateLineDictionary(cvIndex, cvIndex2_1, endPoint7, orderedlinesDictionary, indexLinesInOrder);
        }
      }
    }
    bool flag1;
    for (bool flag2 = true; flag2; flag2 = flag1)
    {
      flag1 = false;
      foreach (Curve curve5 in curveList)
      {
        int cvIndex = curves.IndexOf(curve5);
        if (!orderedlinesDictionary.ContainsKey(cvIndex) || orderedlinesDictionary[cvIndex].Item2 == null)
        {
          XYZ endPoint8 = curve5.GetEndPoint(0);
          XYZ endPoint9 = curve5.GetEndPoint(1);
          int cvIndex2_2 = -1;
          double num5 = double.MaxValue;
          XYZ endPoint10 = (XYZ) null;
          foreach (Curve curve6 in curveList)
          {
            if (!curve6.Equals((object) curve5))
            {
              int cvIndex2 = curves.IndexOf(curve6);
              if ((!orderedlinesDictionary.ContainsKey(cvIndex2) || orderedlinesDictionary[cvIndex2].Item2 == null) && !indexLinesInOrder.Where<List<int>>((Func<List<int>, bool>) (x => x.Contains(cvIndex) && x.Contains(cvIndex2))).Any<List<int>>())
              {
                XYZ endPoint11 = curve6.GetEndPoint(0);
                XYZ endPoint12 = curve6.GetEndPoint(1);
                if (endPoint11.IsAlmostEqualTo(endPoint8) && !endPoint12.IsAlmostEqualTo(endPoint9) || endPoint11.IsAlmostEqualTo(endPoint9) && !endPoint12.IsAlmostEqualTo(endPoint8))
                {
                  this.UpdateLineDictionary(cvIndex, cvIndex2, endPoint11, orderedlinesDictionary, indexLinesInOrder);
                  flag1 = true;
                  break;
                }
                if (endPoint12.IsAlmostEqualTo(endPoint8) && !endPoint11.IsAlmostEqualTo(endPoint9) || endPoint12.IsAlmostEqualTo(endPoint9) && !endPoint11.IsAlmostEqualTo(endPoint8))
                {
                  this.UpdateLineDictionary(cvIndex, cvIndex2, endPoint12, orderedlinesDictionary, indexLinesInOrder);
                  flag1 = true;
                  break;
                }
                IntersectionResultArray resultArray;
                if (strand && curve5.Intersect(curve6, out resultArray) == SetComparisonResult.Overlap && resultArray.Size <= 1)
                {
                  XYZ source = (XYZ) null;
                  foreach (object obj in resultArray)
                  {
                    if (obj is IntersectionResult intersectionResult)
                      source = intersectionResult.XYZPoint;
                  }
                  double num6 = endPoint8.DistanceTo(source);
                  double num7 = endPoint9.DistanceTo(source);
                  double num8 = num6 >= num7 ? num7 : num6;
                  if (num8 < num5)
                  {
                    cvIndex2_2 = cvIndex2;
                    num5 = num8;
                    endPoint10 = source;
                  }
                }
              }
            }
          }
          if (!flag1)
          {
            if (endPoint10 != null)
            {
              this.UpdateLineDictionary(cvIndex, cvIndex2_2, endPoint10, orderedlinesDictionary, indexLinesInOrder);
              flag1 = true;
              break;
            }
          }
          else
            break;
        }
      }
    }
    for (int i = 0; i < curves.Count; i++)
    {
      if (!indexLinesInOrder.Where<List<int>>((Func<List<int>, bool>) (x => x.Contains(i))).Any<List<int>>())
      {
        indexLinesInOrder.Add(new List<int>() { i });
        if (orderedlinesDictionary.ContainsKey(i))
          orderedlinesDictionary[i] = new Tuple<XYZ, XYZ>((XYZ) null, (XYZ) null);
        else
          orderedlinesDictionary.Add(i, new Tuple<XYZ, XYZ>((XYZ) null, (XYZ) null));
      }
    }
  }

  private void UpdateLineDictionary(
    int cvIndex,
    int cvIndex2,
    XYZ endPoint,
    Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary,
    List<List<int>> indexLinesInOrder)
  {
    if (orderedlinesDictionary.ContainsKey(cvIndex))
    {
      orderedlinesDictionary[cvIndex] = new Tuple<XYZ, XYZ>(orderedlinesDictionary[cvIndex].Item1, endPoint);
      if (orderedlinesDictionary.ContainsKey(cvIndex2))
        orderedlinesDictionary[cvIndex2] = new Tuple<XYZ, XYZ>(orderedlinesDictionary[cvIndex2].Item1, endPoint);
      else
        orderedlinesDictionary.Add(cvIndex2, new Tuple<XYZ, XYZ>(endPoint, (XYZ) null));
      int index1 = indexLinesInOrder.FindIndex((Predicate<List<int>>) (x => x.Contains(cvIndex)));
      int index2 = indexLinesInOrder.FindIndex((Predicate<List<int>>) (x => x.Contains(cvIndex2)));
      if (index1 == -1 && index2 == -1)
        indexLinesInOrder.Add(new List<int>()
        {
          cvIndex2,
          cvIndex
        });
      else if (index1 == -1)
      {
        if (indexLinesInOrder[index2].First<int>() == cvIndex2)
          indexLinesInOrder[index2].Insert(0, cvIndex);
        else
          indexLinesInOrder[index2].Add(cvIndex);
      }
      else if (index2 == -1)
      {
        if (indexLinesInOrder[index1].First<int>() == cvIndex)
          indexLinesInOrder[index1].Insert(0, cvIndex2);
        else
          indexLinesInOrder[index1].Add(cvIndex2);
      }
      else if (indexLinesInOrder[index1].First<int>() == cvIndex)
      {
        if (indexLinesInOrder[index2].First<int>() == cvIndex2)
        {
          indexLinesInOrder[index2].Reverse();
          indexLinesInOrder[index2].AddRange((IEnumerable<int>) indexLinesInOrder[index1]);
          indexLinesInOrder.RemoveAt(index1);
        }
        else
        {
          indexLinesInOrder[index2].AddRange((IEnumerable<int>) indexLinesInOrder[index1]);
          indexLinesInOrder.RemoveAt(index1);
        }
      }
      else if (indexLinesInOrder[index2].First<int>() == cvIndex2)
      {
        indexLinesInOrder[index1].AddRange((IEnumerable<int>) indexLinesInOrder[index2]);
        indexLinesInOrder.RemoveAt(index2);
      }
      else
      {
        indexLinesInOrder[index2].Reverse();
        indexLinesInOrder[index1].AddRange((IEnumerable<int>) indexLinesInOrder[index2]);
        indexLinesInOrder.RemoveAt(index2);
      }
    }
    else if (orderedlinesDictionary.ContainsKey(cvIndex2))
    {
      orderedlinesDictionary[cvIndex2] = new Tuple<XYZ, XYZ>(orderedlinesDictionary[cvIndex2].Item1, endPoint);
      orderedlinesDictionary.Add(cvIndex, new Tuple<XYZ, XYZ>(endPoint, (XYZ) null));
      int index = indexLinesInOrder.FindIndex((Predicate<List<int>>) (x => x.Contains(cvIndex2)));
      if (index == -1)
        indexLinesInOrder.Add(new List<int>()
        {
          cvIndex,
          cvIndex2
        });
      else if (indexLinesInOrder[index].First<int>() == cvIndex2)
        indexLinesInOrder[index].Insert(0, cvIndex);
      else
        indexLinesInOrder[index].Add(cvIndex);
    }
    else
    {
      orderedlinesDictionary.Add(cvIndex, new Tuple<XYZ, XYZ>(endPoint, (XYZ) null));
      orderedlinesDictionary.Add(cvIndex2, new Tuple<XYZ, XYZ>(endPoint, (XYZ) null));
      indexLinesInOrder.Add(new List<int>()
      {
        cvIndex,
        cvIndex2
      });
    }
  }

  private List<List<Curve>> GetCurvesListList(
    List<Curve> curves,
    Dictionary<int, Tuple<XYZ, XYZ>> orderedlinesDictionary,
    List<List<int>> indexLinesInOrder)
  {
    List<List<Curve>> curvesListList = new List<List<Curve>>();
    foreach (List<int> intList in indexLinesInOrder)
    {
      List<Curve> curveList = new List<Curve>();
      XYZ xyz1 = (XYZ) null;
      foreach (int num in intList)
      {
        Tuple<XYZ, XYZ> orderedlines = orderedlinesDictionary[num];
        Curve curve = curves[num];
        if (orderedlines.Item1 != null && orderedlines.Item2 != null)
        {
          if (xyz1 == null)
          {
            if (curve is Line)
            {
              Line bound = Line.CreateBound(orderedlines.Item1, orderedlines.Item2);
              curveList.Add((Curve) bound);
              xyz1 = orderedlines.Item2;
            }
            else if (curve is Arc)
            {
              XYZ pointOnArc = curve.Evaluate(0.5, true);
              Arc arc = Arc.Create(orderedlines.Item1, orderedlines.Item2, pointOnArc);
              curveList.Add((Curve) arc);
              xyz1 = orderedlines.Item2;
            }
          }
          else if (!orderedlines.Item1.IsAlmostEqualTo(xyz1))
          {
            if (curve is Line)
            {
              Line bound = Line.CreateBound(xyz1, orderedlines.Item1);
              curveList.Add((Curve) bound);
              xyz1 = orderedlines.Item1;
            }
            else if (curve is Arc)
            {
              XYZ pointOnArc = curve.Evaluate(0.5, true);
              Arc arc = Arc.Create(xyz1, orderedlines.Item1, pointOnArc);
              curveList.Add((Curve) arc);
              xyz1 = orderedlines.Item1;
            }
          }
          else if (curve is Line)
          {
            Line bound = Line.CreateBound(xyz1, orderedlines.Item2);
            curveList.Add((Curve) bound);
            xyz1 = orderedlines.Item2;
          }
          else if (curve is Arc)
          {
            XYZ pointOnArc = curve.Evaluate(0.5, true);
            Arc arc = Arc.Create(xyz1, orderedlines.Item2, pointOnArc);
            curveList.Add((Curve) arc);
            xyz1 = orderedlines.Item2;
          }
        }
        else if (orderedlines.Item1 == null && orderedlines.Item2 == null)
        {
          curveList.Add(curve.Clone());
        }
        else
        {
          XYZ xyz2 = orderedlines.Item1 == null ? orderedlines.Item2 : orderedlines.Item1;
          XYZ endPoint1 = curve.GetEndPoint(0);
          XYZ endPoint2 = curve.GetEndPoint(1);
          if (xyz2.DistanceTo(endPoint1) > xyz2.DistanceTo(endPoint2))
          {
            if (xyz1 == null)
            {
              if (curve is Line)
              {
                Line bound = Line.CreateBound(endPoint1, xyz2);
                curveList.Add((Curve) bound);
                xyz1 = xyz2;
              }
              else if (curve is Arc)
              {
                XYZ pointOnArc = curve.Evaluate(0.5, true);
                Arc arc = Arc.Create(endPoint1, xyz2, pointOnArc);
                curveList.Add((Curve) arc);
                xyz1 = xyz2;
              }
            }
            else if (curve is Line)
            {
              Line bound = Line.CreateBound(xyz2, endPoint1);
              curveList.Add((Curve) bound);
              xyz1 = xyz2;
            }
            else if (curve is Arc)
            {
              XYZ pointOnArc = curve.Evaluate(0.5, true);
              Arc arc = Arc.Create(xyz2, endPoint1, pointOnArc);
              curveList.Add((Curve) arc);
              xyz1 = xyz2;
            }
          }
          else if (xyz1 == null)
          {
            if (curve is Line)
            {
              Line bound = Line.CreateBound(endPoint2, xyz2);
              curveList.Add((Curve) bound);
              xyz1 = xyz2;
            }
            else if (curve is Arc)
            {
              XYZ pointOnArc = curve.Evaluate(0.5, true);
              Arc arc = Arc.Create(endPoint2, xyz2, pointOnArc);
              curveList.Add((Curve) arc);
              xyz1 = xyz2;
            }
          }
          else if (curve is Line)
          {
            Line bound = Line.CreateBound(xyz2, endPoint2);
            curveList.Add((Curve) bound);
            xyz1 = xyz2;
          }
          else if (curve is Arc)
          {
            XYZ pointOnArc = curve.Evaluate(0.5, true);
            Arc arc = Arc.Create(xyz2, endPoint2, pointOnArc);
            curveList.Add((Curve) arc);
            xyz1 = xyz2;
          }
        }
      }
      curvesListList.Add(curveList);
    }
    for (int i = 0; i < curves.Count; i++)
    {
      if (!indexLinesInOrder.Where<List<int>>((Func<List<int>, bool>) (x => x.Contains(i))).Any<List<int>>())
        curvesListList.Add(new List<Curve>() { curves[i] });
    }
    return curvesListList;
  }

  private ModelCurveArray get3DModelCurveArray(
    Document revitDoc,
    CurveLoop curveLoop,
    Transform transform)
  {
    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(curveLoop, transform);
    XYZ normal1 = XYZ.BasisZ;
    bool flag = false;
    XYZ xyz1 = (XYZ) null;
    List<XYZ> source = new List<XYZ>();
    foreach (Curve curve in curveLoop)
    {
      if (curve.IsBound)
      {
        XYZ endpoint1 = transform.Inverse.OfPoint(curve.GetEndPoint(0));
        XYZ endpoint2 = transform.Inverse.OfPoint(curve.GetEndPoint(1));
        if (!source.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(endpoint1))).Any<XYZ>())
          source.Add(endpoint1);
        if (!source.Where<XYZ>((Func<XYZ, bool>) (x => x.IsAlmostEqualTo(endpoint2))).Any<XYZ>())
          source.Add(endpoint2);
      }
      if (curve is Arc && !flag)
      {
        normal1 = transform.Inverse.OfVector((curve as Arc).Normal);
        flag = true;
        xyz1 = transform.Inverse.OfPoint((curve as Arc).Center);
        break;
      }
    }
    if (source.Count < 2 && !flag)
      return (ModelCurveArray) null;
    if (!flag)
    {
      if (source.Count<XYZ>() > 2)
      {
        XYZ pointA = source.FirstOrDefault<XYZ>();
        XYZ pointB = source.LastOrDefault<XYZ>();
        XYZ xyz2 = source[1];
        foreach (XYZ xyz3 in source)
        {
          if (!pointA.IsAlmostEqualTo(xyz3) && !pointB.IsAlmostEqualTo(xyz3))
          {
            XYZ normal2 = this.CalculateNormal(pointA, pointB, xyz3);
            if (!normal2.IsAlmostEqualTo(XYZ.Zero))
            {
              normal1 = normal2;
              flag = true;
              break;
            }
          }
        }
      }
      if (source.Count == 2)
      {
        XYZ pointA = source.FirstOrDefault<XYZ>();
        XYZ pointB = source.LastOrDefault<XYZ>();
        XYZ xyz4 = source.FirstOrDefault<XYZ>() + new XYZ(0.0, 0.0, 10.0);
        if (!pointA.IsAlmostEqualTo(xyz4) && !pointB.IsAlmostEqualTo(xyz4))
        {
          XYZ normal3 = this.CalculateNormal(pointA, pointB, xyz4);
          if (!normal3.IsAlmostEqualTo(XYZ.Zero))
          {
            normal1 = normal3;
            flag = true;
          }
        }
      }
      if (!flag)
      {
        if (source[0].Z.ApproximatelyEquals(source[1].Z))
          normal1 = XYZ.BasisZ;
        else if (source[0].Y.ApproximatelyEquals(source[1].Y))
          normal1 = XYZ.BasisY;
        else if (source[0].X.ApproximatelyEquals(source[1].X))
          normal1 = XYZ.BasisX;
      }
    }
    XYZ origin = (XYZ) null;
    if (source.Count > 0)
      origin = source[0];
    else if (xyz1 != null)
      origin = xyz1;
    if (origin == null)
      return (ModelCurveArray) null;
    Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(normal1, origin);
    SketchPlane sketchPlane = SketchPlane.Create(revitDoc, byNormalAndOrigin);
    return revitDoc.Create.NewModelCurveArray(LaserUtils.OverkillCurveArray(curveArrayFromLoop), sketchPlane);
  }

  private XYZ CalculateNormal(XYZ pointA, XYZ pointB, XYZ pointC)
  {
    return new XYZ(pointB.X - pointA.X, pointB.Y - pointA.Y, pointB.Z - pointA.Z).CrossProduct(new XYZ(pointC.X - pointA.X, pointC.Y - pointA.Y, pointC.Z - pointA.Z));
  }

  private CurveLoop GetBottomLoop(Element member, Transform transform)
  {
    if (!(member is FamilyInstance familyInstance))
      return (CurveLoop) null;
    BoundingBoxXYZ transformedBboxFromElement = CAM_Utils.CreateTransformedBBoxFromElement(member, transform);
    Transform transform1 = member.GetTopLevelElement() is FamilyInstance topLevelElement ? topLevelElement.GetTransform() : familyInstance.GetTransform();
    CurveLoop bottomLoop;
    if (Util.IsParallel(transform1.BasisX, transform.BasisZ) || Util.IsParallel(transform1.BasisY, transform.BasisZ) || Util.IsParallel(transform1.BasisZ, transform.BasisZ))
    {
      BoundingBoxXYZ bbox = topLevelElement == null ? CAM_Utils.CreateTransformedBBoxFromElement(member, familyInstance.GetTransform()) : CAM_Utils.CreateTransformedBBoxFromElement(member, topLevelElement.GetTransform());
      bottomLoop = this.LoopFromBBox(bbox);
      if (bottomLoop == null)
        return (CurveLoop) null;
      if (bbox.Max.Z - bbox.Min.Z <= 1.0 / 384.0)
      {
        bottomLoop = this.LoopFromBBox(transformedBboxFromElement);
      }
      else
      {
        foreach (Face face in SolidUtils.CreateTransformed(SolidUtils.CreateTransformed(GeometryCreationUtilities.CreateExtrusionGeometry((IList<CurveLoop>) new List<CurveLoop>()
        {
          bottomLoop
        }, XYZ.BasisZ, bbox.Max.Z - bbox.Min.Z), topLevelElement != null ? topLevelElement.GetTransform() : familyInstance.GetTransform()), transform.Inverse).Faces)
        {
          if (face is PlanarFace planarFace && planarFace.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ.Negate()))
          {
            bottomLoop = planarFace.GetEdgesAsCurveLoops().First<CurveLoop>();
            break;
          }
        }
      }
    }
    else
      bottomLoop = this.LoopFromBBox(transformedBboxFromElement);
    return bottomLoop;
  }
}
