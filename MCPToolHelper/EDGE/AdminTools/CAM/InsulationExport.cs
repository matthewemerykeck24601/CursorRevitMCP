// Decompiled with JetBrains decompiler
// Type: EDGE.AdminTools.CAM.InsulationExport
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
using Utils.AdminUtils;
using Utils.AssemblyUtils;
using Utils.GeometryUtils;
using Utils.MiscUtils;
using Utils.SelectionUtils;
using Utils.WorkSharingUtils;

#nullable disable
namespace EDGE.AdminTools.CAM;

[Transaction(TransactionMode.Manual)]
internal class InsulationExport : IExternalCommand
{
  private const double compositeLength = 0.25;
  private const double pinDiameter = 0.041666666666666664;
  private const double diamondWidth = 0.33333333333333331;

  public static ElementId GenerateTextNoteType(Document revitDoc, Color color)
  {
    TextNoteType textNoteType = (TextNoteType) null;
    FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfClass(typeof (TextNoteType));
    Dictionary<InsulationExport.paramsEnum, bool> dictionary = new Dictionary<InsulationExport.paramsEnum, bool>()
    {
      {
        InsulationExport.paramsEnum.Color,
        false
      },
      {
        InsulationExport.paramsEnum.LineWeight,
        false
      },
      {
        InsulationExport.paramsEnum.Background,
        false
      },
      {
        InsulationExport.paramsEnum.ShowBorder,
        false
      },
      {
        InsulationExport.paramsEnum.TextFont,
        false
      },
      {
        InsulationExport.paramsEnum.TextSize,
        false
      }
    };
    bool flag = false;
    int num1 = (int) color.Red + (int) color.Green * 256 /*0x0100*/ + (int) color.Blue * 65536 /*0x010000*/;
    foreach (Element element in elementCollector)
    {
      if (element is TextNoteType elem && elem.Name.Contains("INSULATION_EXPORT_ANNOTATION"))
      {
        if (elem.LookupParameter("Color").AsInteger() != num1)
          dictionary[InsulationExport.paramsEnum.Color] = true;
        if (Utils.ElementUtils.Parameters.GetParameterAsInt((Element) elem, "Line Weight") != 1)
          dictionary[InsulationExport.paramsEnum.LineWeight] = true;
        if (Utils.ElementUtils.Parameters.GetParameterAsInt((Element) elem, "Background") != 1)
          dictionary[InsulationExport.paramsEnum.Background] = true;
        if (Utils.ElementUtils.Parameters.GetParameterAsBool((Element) elem, "Show Border"))
          dictionary[InsulationExport.paramsEnum.ShowBorder] = true;
        if (elem.LookupParameter("Text Font").AsString() != "Arial")
          dictionary[InsulationExport.paramsEnum.TextFont] = true;
        if (elem.LookupParameter("Text Size").AsDouble() != 1.0 / 384.0)
          dictionary[InsulationExport.paramsEnum.TextSize] = true;
        flag = true;
        textNoteType = elem;
        break;
      }
    }
    if (!flag)
    {
      using (Transaction transaction = new Transaction(revitDoc, "Create Text Note Style"))
      {
        int num2 = (int) transaction.Start();
        textNoteType = (revitDoc.GetElement(revitDoc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType)) as TextNoteType).Duplicate("INSULATION_EXPORT_ANNOTATION") as TextNoteType;
        textNoteType.LookupParameter("Color").Set(num1);
        textNoteType.LookupParameter("Line Weight").Set(1);
        textNoteType.LookupParameter("Background").Set(1);
        textNoteType.LookupParameter("Show Border").Set(0);
        textNoteType.LookupParameter("Text Font").Set("Arial");
        textNoteType.LookupParameter("Text Size").Set(1.0 / 384.0);
        int num3 = (int) transaction.Commit();
      }
    }
    else if (dictionary.Values.Any<bool>((Func<bool, bool>) (e => e)))
    {
      using (Transaction transaction = new Transaction(revitDoc, "Modify Text Note Style"))
      {
        int num4 = (int) transaction.Start();
        if (dictionary[InsulationExport.paramsEnum.Color])
          textNoteType.LookupParameter("Color").Set(num1);
        if (dictionary[InsulationExport.paramsEnum.LineWeight])
          textNoteType.LookupParameter("Line Weight").Set(1);
        if (dictionary[InsulationExport.paramsEnum.Background])
          textNoteType.LookupParameter("Background").Set(1);
        if (dictionary[InsulationExport.paramsEnum.ShowBorder])
          textNoteType.LookupParameter("Show Border").Set(0);
        if (dictionary[InsulationExport.paramsEnum.TextFont])
          textNoteType.LookupParameter("Text Font").Set("Arial");
        if (dictionary[InsulationExport.paramsEnum.TextSize])
          textNoteType.LookupParameter("Text Size").Set(1.0 / 384.0);
        int num5 = (int) transaction.Commit();
      }
    }
    return textNoteType?.Id;
  }

  public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
  {
    List<ElementId> elementIdList1 = new List<ElementId>();
    Document revitDoc = commandData.Application.ActiveUIDocument.Document;
    if (revitDoc.IsFamilyDocument)
    {
      TaskDialog.Show("Error", "Insulation export cannot be run from the family editor.");
      return (Result) 1;
    }
    string manufacturer1 = "";
    Parameter parameter = revitDoc.ProjectInformation.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER");
    if (parameter != null && parameter.StorageType == StorageType.String)
      manufacturer1 = parameter.AsString();
    if (string.IsNullOrWhiteSpace(manufacturer1))
    {
      TaskDialog.Show("Insulation Export", "Please ensure that the project has a valid PROJECT_CLIENT_PRECAST_MANUFACTURER parameter value and try to open the Insulation Export tool again.");
      return (Result) 1;
    }
    CADExportSettings cadExportSettings = new CADExportSettings(true);
    if (!cadExportSettings.GetManufacturerSettings(manufacturer1))
    {
      TaskDialog taskDialog = new TaskDialog("Insulation Export")
      {
        MainInstruction = "Unable to read CAD Export Settings file."
      };
      taskDialog.MainInstruction += "The Insulation Export will use the default Insulation Export settings.";
      taskDialog.CommonButtons = (TaskDialogCommonButtons) 9;
      if (taskDialog.Show() != 1)
        return (Result) 1;
    }
    string projectName1 = revitDoc.ProjectInformation.Name.Replace('/', '_');
    Dictionary<string, Category> dictionary1 = new Dictionary<string, Category>();
    ViewFamilyType viewFamilyType = new FilteredElementCollector(revitDoc).OfClass(typeof (ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>((Func<ViewFamilyType, bool>) (x => x.ViewFamily == ViewFamily.ThreeDimensional));
    ExportDWGSettings exportDwgSettings1 = (ExportDWGSettings) null;
    View3D view3D1 = (View3D) null;
    ElementId typeId = (ElementId) null;
    Dictionary<CADExportSettings.ExportGroup, CADExportLayer> layerDict = new Dictionary<CADExportSettings.ExportGroup, CADExportLayer>();
    if (cadExportSettings.InsulationLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Insulation, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Insulation));
    if (cadExportSettings.CutoutLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Cutout, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Cutout));
    if (cadExportSettings.PinsLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Pins, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Pins));
    if (cadExportSettings.SlotsLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.Slots, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.Slots));
    if (cadExportSettings.ExtraTiesLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.ExtraTies, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.ExtraTies));
    if (cadExportSettings.MarkSymbolLayerEnabled)
      layerDict.Add(CADExportSettings.ExportGroup.MarkSymbol, cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.MarkSymbol));
    if (cadExportSettings.MarkTextLayerEnabled)
    {
      CADExportLayer selectedLayer = cadExportSettings.GetSelectedLayer(CADExportSettings.ExportGroup.MarkText);
      layerDict.Add(CADExportSettings.ExportGroup.MarkText, selectedLayer);
      typeId = InsulationExport.GenerateTextNoteType(revitDoc, LaserUtils.WindowsToAutodeskColor(selectedLayer.LayerColor.color));
    }
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
      return (Result) 1;
    using (Transaction transaction = new Transaction(revitDoc, "Settings Update"))
    {
      bool flag1 = false;
      bool flag2 = false;
      Category category1 = Category.GetCategory(revitDoc, BuiltInCategory.OST_Lines);
      FilteredElementCollector elementCollector = new FilteredElementCollector(revitDoc).OfClass(typeof (ExportDWGSettings));
      Category category2 = Category.GetCategory(revitDoc, BuiltInCategory.OST_TextNotes);
      foreach (Element element in (IEnumerable<Element>) elementCollector.ToElements())
      {
        if (element is ExportDWGSettings exportDwgSettings2 && exportDwgSettings2.Name == "EDGE_Insulation_Export")
        {
          exportDwgSettings1 = exportDwgSettings2;
          DWGExportOptions dwgExportOptions = exportDwgSettings1.GetDWGExportOptions();
          ExportLayerTable exportLayerTable = dwgExportOptions.GetExportLayerTable();
          if (dwgExportOptions.PropOverrides != PropOverrideMode.ByLayer)
          {
            flag2 = true;
            break;
          }
          if (cadExportSettings.MarkTextLayerEnabled)
          {
            ExportLayerKey layerKey = new ExportLayerKey(category2.Name, "", SpecialType.Default);
            ExportLayerInfo exportLayerInfo = exportLayerTable[layerKey];
            if (exportLayerInfo.LayerName != layerDict[CADExportSettings.ExportGroup.MarkText].LayerName || exportLayerInfo.ColorNumber != dictionary2[layerDict[CADExportSettings.ExportGroup.MarkText].LayerName])
            {
              flag2 = true;
              break;
            }
          }
          using (Dictionary<string, Category>.KeyCollection.Enumerator enumerator = dictionary3.Keys.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              string current = enumerator.Current;
              Category category3 = dictionary3[current];
              if (category3 != null)
              {
                ExportLayerKey exportLayerKey = new ExportLayerKey(category1.Name, category3.Name, SpecialType.Default);
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
          exportDwgSettings1 = ExportDWGSettings.Create(revitDoc, "EDGE_Insulation_Export");
        if (exportDwgSettings1 != null)
        {
          DWGExportOptions dwgExportOptions = exportDwgSettings1.GetDWGExportOptions();
          ExportLayerTable exportLayerTable = dwgExportOptions.GetExportLayerTable();
          if (cadExportSettings.MarkTextLayerEnabled)
          {
            ExportLayerKey exportLayerKey = new ExportLayerKey(category2.Name, "", SpecialType.Default);
            ExportLayerInfo exportLayerInfo = new ExportLayerInfo();
            exportLayerInfo.LayerName = layerDict[CADExportSettings.ExportGroup.MarkText].LayerName;
            exportLayerInfo.ColorNumber = dictionary2[layerDict[CADExportSettings.ExportGroup.MarkText].LayerName];
            if (exportLayerTable.ContainsKey(exportLayerKey))
              exportLayerTable[exportLayerKey] = exportLayerInfo;
          }
          foreach (string key in dictionary3.Keys)
          {
            Category category4 = dictionary3[key];
            if (category4 != null)
            {
              ExportLayerKey exportLayerKey = new ExportLayerKey(category1.Name, category4.Name, SpecialType.Default);
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
            if (!CheckElementsOwnership.CheckOwnership("Insulation Export", new List<ElementId>()
            {
              view3D2.Id
            }, revitDoc, commandData.Application.ActiveUIDocument, out List<ElementId> _))
              return (Result) 1;
          }
          flag3 = true;
          bool flag4 = false;
          if (view3D2.ViewTemplateId != (ElementId) null && !view3D2.ViewTemplateId.Equals((object) ElementId.InvalidElementId))
            flag4 = true;
          if (view3D2.Scale != 96 /*0x60*/)
            flag4 = true;
          List<ElementId> elementIdList2 = new List<ElementId>();
          List<ElementId> source2 = new List<ElementId>()
          {
            new ElementId(BuiltInCategory.OST_Lines),
            new ElementId(BuiltInCategory.OST_TextNotes)
          };
          source2.AddRange(dictionary3.Values.Select<Category, ElementId>((Func<Category, ElementId>) (v => v.Id)));
          foreach (ElementId categoryId in source2)
          {
            if (view3D2.GetCategoryHidden(categoryId))
            {
              elementIdList2.Add(categoryId);
              flag4 = true;
            }
          }
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
        view3D1.Scale = 96 /*0x60*/;
        view3D1.SetOrientation(new ViewOrientation3D(eyePosition, XYZ.BasisY, XYZ.BasisZ.Negate()));
        view3D1.ViewTemplateId = ElementId.InvalidElementId;
        foreach (string key in dictionary3.Keys)
          view3D1.SetCategoryHidden(dictionary3[key].Id, false);
        List<ElementId> source3 = new List<ElementId>()
        {
          new ElementId(BuiltInCategory.OST_Lines),
          new ElementId(BuiltInCategory.OST_TextNotes)
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
      List<string> stringList2 = new List<string>();
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
          TaskDialog.Show("Error", "Current view is not valid for insulation export. Please activate a graphical view before trying again.");
          int num8 = (int) transactionGroup.RollBack();
          return (Result) 1;
        }
      }
      catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
      {
        int num9 = (int) transactionGroup.RollBack();
        return (Result) 0;
      }
      if (source4.Count < 1)
      {
        int num10 = (int) transactionGroup.RollBack();
        return (Result) 1;
      }
      IntPtr mainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
      List<string> list1 = source4.Select<AssemblyInstance, string>((Func<AssemblyInstance, string>) (x => x.Name)).ToList<string>();
      CADExportSettings settings = cadExportSettings.Clone();
      List<string> ElementMarks = list1;
      string projectName2 = projectName1;
      string number = revitDoc.ProjectInformation.Number;
      string manufacturer2 = manufacturer1;
      CADExportWindow cadExportWindow = new CADExportWindow(mainWindowHandle, settings, ElementMarks, projectName2, number, manufacturer2, false);
      cadExportWindow.ShowDialog();
      if (cadExportWindow.Cancel)
      {
        App.laserExportErrorCheck(false);
        return (Result) 1;
      }
      CADExportSettings newSettings = cadExportWindow.GetNewSettings();
      List<string> stringList3 = new List<string>();
      foreach (AssemblyInstance assemblyInstance in source4)
      {
        if (assemblyInstance.GetStructuralFramingElement() == null || Utils.ElementUtils.Parameters.GetParameterAsBool((Element) assemblyInstance, "HARDWARE_DETAIL"))
        {
          stringList3.Add(assemblyInstance.AssemblyTypeName);
        }
        else
        {
          List<InsulationRectangle> rectangles = new List<InsulationRectangle>();
          using (Transaction transaction = new Transaction(revitDoc, "Generate Lines"))
          {
            int num11 = (int) transaction.Start();
            Transform transform1 = assemblyInstance.GetTransform();
            List<Element> list2 = assemblyInstance.GetMemberIds().Select<ElementId, Element>((Func<ElementId, Element>) (e => revitDoc.GetElement(e))).Where<Element>((Func<Element, bool>) (i => Utils.ElementUtils.Parameters.GetParameterAsString(i, "MANUFACTURE_COMPONENT").ToUpper().Contains("INSULATION"))).ToList<Element>();
            List<Element> elementList1 = new List<Element>();
            if (newSettings.PinsLayerEnabled || newSettings.ExtraTiesLayerEnabled || newSettings.SlotsLayerEnabled)
              elementList1 = assemblyInstance.GetMemberIds().Select<ElementId, Element>((Func<ElementId, Element>) (e => revitDoc.GetElement(e))).Where<Element>((Func<Element, bool>) (p => Utils.ElementUtils.Parameters.GetParameterAsString(p, "MANUFACTURE_COMPONENT").Contains("PIN"))).ToList<Element>();
            if (list2.Count != 0)
            {
              foreach (Element elem1 in list2)
              {
                List<CurveArray> source5 = new List<CurveArray>();
                List<CurveArray> source6 = new List<CurveArray>();
                List<CurveArray> source7 = new List<CurveArray>();
                List<CurveArray> source8 = new List<CurveArray>();
                List<CurveArray> source9 = new List<CurveArray>();
                List<Solid> instanceSolids = Solids.GetInstanceSolids(elem1);
                Solid solid1 = (Solid) null;
                foreach (Solid solid2 in instanceSolids)
                {
                  if (solid2.Volume > 0.0)
                    solid1 = !((GeometryObject) solid1 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(solid2, solid1, BooleanOperationsType.Union) : SolidUtils.Clone(solid2);
                }
                List<Tuple<CurveLoop, XYZ, bool>> outerCurves = new List<Tuple<CurveLoop, XYZ, bool>>();
                List<Tuple<CurveLoop, XYZ>> upwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
                List<Tuple<CurveLoop, XYZ>> downwardFacingCurves = new List<Tuple<CurveLoop, XYZ>>();
                double maxZ;
                double minZ;
                CAM_Utils.GetCurveLoops(solid1, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ, out minZ, transform1);
                List<CurveLoop> recesses = LaserUtils.GetRecesses(solid1, transform1, maxZ, minZ);
                Dictionary<Solid, Tuple<string, double, bool>> LayerDictionary = new Dictionary<Solid, Tuple<string, double, bool>>();
                if (CAM_Utils.GetLayerDictionary(LayerDictionary, outerCurves, upwardFacingCurves, downwardFacingCurves, revitDoc, "", 0.0, minZ, maxZ, Math.Abs(maxZ - minZ), out bool _, transform1))
                {
                  Solid solid3 = (Solid) null;
                  foreach (Solid key in LayerDictionary.Keys)
                    solid3 = !((GeometryObject) solid3 == (GeometryObject) null) ? BooleanOperationsUtils.ExecuteBooleanOperation(key, solid3, BooleanOperationsType.Union) : SolidUtils.Clone(key);
                  PlanarFace planarFace1 = (PlanarFace) null;
                  foreach (Face face in SolidUtils.CreateTransformed(solid3, transform1.Inverse).Faces)
                  {
                    if (face is PlanarFace planarFace2 && planarFace2.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ))
                      planarFace1 = planarFace2;
                  }
                  outerCurves.Clear();
                  upwardFacingCurves.Clear();
                  downwardFacingCurves.Clear();
                  CAM_Utils.GetCurveLoops(solid3, outerCurves, upwardFacingCurves, downwardFacingCurves, out maxZ, out minZ, transform1);
                  List<CurveLoop> originalCurveLoopList = new List<CurveLoop>();
                  foreach (Tuple<CurveLoop, XYZ, bool> tuple in outerCurves)
                  {
                    CurveLoop curveLoop = tuple.Item1;
                    if (transform1.Inverse.OfPoint(tuple.Item2).Z.ApproximatelyEquals(maxZ))
                      originalCurveLoopList.Add(curveLoop);
                  }
                  bool OverallSplines;
                  foreach (CurveLoop key in CAM_Utils.GetFlattenedToZ(originalCurveLoopList, transform1, 0.0, 1.0 / 384.0, out OverallSplines).Keys)
                  {
                    CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform1);
                    source5.Add(curveArrayFromLoop);
                  }
                  if (newSettings.CutoutLayerEnabled)
                  {
                    List<CurveLoop> second = new List<CurveLoop>();
                    foreach (Tuple<CurveLoop, XYZ> tuple in upwardFacingCurves)
                      second.Add(tuple.Item1);
                    Dictionary<CurveLoop, CurveLoop> flattenedToZ = CAM_Utils.GetFlattenedToZ(recesses.Union<CurveLoop>((IEnumerable<CurveLoop>) second).ToList<CurveLoop>(), transform1, 0.0, 1.0 / 384.0, out OverallSplines);
                    if (newSettings.CutoutLayerEnabled)
                    {
                      foreach (CurveLoop key in flattenedToZ.Keys)
                      {
                        CurveArray curveArrayFromLoop = LaserUtils.GetCurveArrayFromLoop(key, transform1);
                        source6.Add(curveArrayFromLoop);
                      }
                    }
                  }
                  if (newSettings.PinsLayerEnabled || newSettings.SlotsLayerEnabled || newSettings.ExtraTiesLayerEnabled)
                  {
                    List<Element> elementList2 = new List<Element>();
                    foreach (Element element in elementList1)
                    {
                      if (element.Location is LocationPoint location && (GeometryObject) planarFace1 != (GeometryObject) null && planarFace1.Project(transform1.Inverse.OfPoint(location.Point)) != null)
                        elementList2.Add(element);
                    }
                    foreach (Element elem2 in elementList2)
                    {
                      if (elem2 is FamilyInstance elem3)
                      {
                        Transform transform2 = elem3.GetTransform();
                        XYZ zero = XYZ.Zero;
                        XYZ point = !(elem2.Location is LocationPoint) ? transform2.Origin : (elem2.Location as LocationPoint).Point;
                        XYZ xyz1 = transform1.Inverse.OfPoint(point);
                        XYZ center = new XYZ(xyz1.X, xyz1.Y, 0.0);
                        if (Utils.ElementUtils.Parameters.GetParameterAsString(elem2, "MANUFACTURE_COMPONENT").Contains("COMPOSITE"))
                        {
                          if (newSettings.SlotsLayerEnabled)
                          {
                            bool flag = false;
                            if (Util.IsParallel(transform2.BasisX, transform1.BasisX))
                              flag = true;
                            XYZ xyz2 = flag ? XYZ.BasisY : XYZ.BasisX;
                            Line bound = Line.CreateBound(center - xyz2 * 0.125, center + xyz2 * 0.125);
                            CurveArray curveArray = new CurveArray();
                            curveArray.Append((Curve) bound);
                            source7.Add(curveArray);
                          }
                        }
                        else if (newSettings.PinsLayerEnabled)
                        {
                          Arc arc = Arc.Create(center, 1.0 / 48.0, 0.0, 2.0 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                          CurveArray curveArray = new CurveArray();
                          curveArray.Append((Curve) arc);
                          source8.Add(curveArray);
                        }
                        if (newSettings.ExtraTiesLayerEnabled && Utils.ElementUtils.Parameters.GetParameterAsBool((Element) elem3, "Extra_Ties"))
                        {
                          XYZ xyz3 = center - XYZ.BasisX * (1.0 / 3.0) / 2.0;
                          XYZ xyz4 = center + XYZ.BasisY * (1.0 / 3.0) / 2.0;
                          XYZ xyz5 = center + XYZ.BasisX * (1.0 / 3.0) / 2.0;
                          XYZ xyz6 = center - XYZ.BasisY * (1.0 / 3.0) / 2.0;
                          List<Line> lineList = new List<Line>();
                          lineList.Add(Line.CreateBound(xyz3, xyz4));
                          lineList.Add(Line.CreateBound(xyz4, xyz5));
                          lineList.Add(Line.CreateBound(xyz5, xyz6));
                          lineList.Add(Line.CreateBound(xyz6, xyz3));
                          CurveArray curveArray = new CurveArray();
                          foreach (Line line in lineList)
                            curveArray.Append((Curve) line);
                          source9.Add(curveArray);
                        }
                      }
                    }
                  }
                }
                double num12 = double.MaxValue;
                double num13 = double.MaxValue;
                double num14 = double.MinValue;
                double num15 = double.MinValue;
                foreach (CurveArray curveArray in source5)
                {
                  foreach (Curve curve in curveArray)
                  {
                    num12 = Math.Min(curve.GetEndPoint(0).X, num12);
                    num13 = Math.Min(curve.GetEndPoint(0).Y, num13);
                    num14 = Math.Max(curve.GetEndPoint(0).X, num14);
                    num15 = Math.Max(curve.GetEndPoint(0).Y, num15);
                  }
                }
                InsulationRectangle insulationRectangle = new InsulationRectangle(new XYZ(num12, num13, 0.0), new XYZ(num14, num15, 0.0))
                {
                  insulationCurveArray = source5.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (cA => LaserUtils.OverkillCurveArray(cA))).ToList<CurveArray>(),
                  reductionCurveArray = source6.Select<CurveArray, CurveArray>((Func<CurveArray, CurveArray>) (cA => LaserUtils.OverkillCurveArray(cA))).ToList<CurveArray>(),
                  compositePinLines = source7.ToList<CurveArray>(),
                  normalPinLines = source8.ToList<CurveArray>(),
                  pinDiamonds = source9.ToList<CurveArray>()
                };
                insulationRectangle.GenerateNote(Utils.ElementUtils.Parameters.GetParameterAsString(elem1, "INSULATION_MARK"));
                rectangles.Add(insulationRectangle);
              }
              Plane byNormalAndOrigin = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0.0, 0.0, 0.0));
              SketchPlane sketchPlane = SketchPlane.Create(revitDoc, byNormalAndOrigin);
              List<ModelCurveArray> mca1 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca2 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca3 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca4 = new List<ModelCurveArray>();
              List<ModelCurveArray> mca5 = new List<ModelCurveArray>();
              Dictionary<InsulationRectangle, XYZ> dictionary4 = InsulationRectangle.Expand(rectangles);
              List<ElementId> collection = new List<ElementId>();
              List<ModelCurveArray> mca6 = new List<ModelCurveArray>();
              foreach (InsulationRectangle key in rectangles)
              {
                if (newSettings.InsulationLayerEnabled)
                {
                  key.InsulationCurveArrayOffset(dictionary4[key]);
                  foreach (CurveArray insulationCurve in key.insulationCurveArray)
                    mca1.Add(revitDoc.Create.NewModelCurveArray(insulationCurve, sketchPlane));
                }
                if (newSettings.CutoutLayerEnabled)
                {
                  key.ReductionCurveArrayOffset(dictionary4[key]);
                  foreach (CurveArray reductionCurve in key.reductionCurveArray)
                    mca2.Add(revitDoc.Create.NewModelCurveArray(reductionCurve, sketchPlane));
                }
                if (newSettings.PinsLayerEnabled)
                {
                  key.NormalPinLinesOffset(dictionary4[key]);
                  foreach (CurveArray normalPinLine in key.normalPinLines)
                    mca3.Add(revitDoc.Create.NewModelCurveArray(normalPinLine, sketchPlane));
                }
                if (newSettings.SlotsLayerEnabled)
                {
                  key.CompositePinLinesOffset(dictionary4[key]);
                  foreach (CurveArray compositePinLine in key.compositePinLines)
                    mca4.Add(revitDoc.Create.NewModelCurveArray(compositePinLine, sketchPlane));
                }
                if (newSettings.ExtraTiesLayerEnabled)
                {
                  key.PinDiamondsOffset(dictionary4[key]);
                  foreach (CurveArray pinDiamond in key.pinDiamonds)
                    mca5.Add(revitDoc.Create.NewModelCurveArray(pinDiamond, sketchPlane));
                }
                if (newSettings.MarkSymbolLayerEnabled || newSettings.MarkTextLayerEnabled)
                  key.TextNoteLocationOffset(dictionary4[key]);
                if (newSettings.MarkTextLayerEnabled)
                  collection.Add(TextNote.Create(revitDoc, view3D1.Id, key.TextNote.Location, key.TextNote.Text, new TextNoteOptions(typeId)
                  {
                    VerticalAlignment = VerticalTextAlignment.Middle,
                    HorizontalAlignment = HorizontalTextAlignment.Center
                  }).Id);
                if (newSettings.MarkSymbolLayerEnabled)
                {
                  CurveArray geometryCurveArray = new CurveArray();
                  geometryCurveArray.Append((Curve) Arc.Create(Plane.CreateByNormalAndOrigin(XYZ.BasisZ, key.TextNote.Location), 31.0 / 96.0, 0.0, 2.0 * Math.PI));
                  ModelCurveArray modelCurveArray = revitDoc.Create.NewModelCurveArray(geometryCurveArray, sketchPlane);
                  mca6.Add(modelCurveArray);
                }
              }
              if (newSettings.CutoutLayerEnabled)
                LaserUtils.AssignLineStyle(mca2, dictionary3[layerDict[CADExportSettings.ExportGroup.Cutout].LayerName], revitDoc);
              if (newSettings.InsulationLayerEnabled)
                LaserUtils.AssignLineStyle(mca1, dictionary3[layerDict[CADExportSettings.ExportGroup.Insulation].LayerName], revitDoc);
              if (newSettings.PinsLayerEnabled)
                LaserUtils.AssignLineStyle(mca3, dictionary3[layerDict[CADExportSettings.ExportGroup.Pins].LayerName], revitDoc);
              if (newSettings.SlotsLayerEnabled)
                LaserUtils.AssignLineStyle(mca4, dictionary3[layerDict[CADExportSettings.ExportGroup.Slots].LayerName], revitDoc);
              if (newSettings.ExtraTiesLayerEnabled)
                LaserUtils.AssignLineStyle(mca5, dictionary3[layerDict[CADExportSettings.ExportGroup.ExtraTies].LayerName], revitDoc);
              if (newSettings.MarkSymbolLayerEnabled)
                LaserUtils.AssignLineStyle(mca6, dictionary3[layerDict[CADExportSettings.ExportGroup.MarkSymbol].LayerName], revitDoc);
              List<ElementId> elementIdList5 = LaserUtils.CollectCurveIds(new List<List<ModelCurveArray>>()
              {
                mca2,
                mca1,
                mca3,
                mca4,
                mca5,
                mca6
              });
              elementIdList5.AddRange((IEnumerable<ElementId>) collection);
              foreach (ElementId filter in (IEnumerable<ElementId>) view3D1.GetFilters())
                view3D1.RemoveFilter(filter);
              view3D1.IsolateElementsTemporary((ICollection<ElementId>) elementIdList5);
              view3D1.ConvertTemporaryHideIsolateToPermanent();
              view3D1.SetOrientation(new ViewOrientation3D(new XYZ(0.0, 0.0, 10.0), XYZ.BasisY, XYZ.BasisZ.Negate()));
              int num16 = (int) transaction.Commit();
              if (exportDwgSettings1 != null)
              {
                string shortFilePath = newSettings.GetShortFilePath(projectName1, revitDoc.ProjectInformation.Number, false);
                LaserUtils.CheckDirectory(shortFilePath);
                string fileName = newSettings.GetFileName(assemblyInstance.Name, revitDoc.ProjectInformation.Number, false);
                if (newSettings.InsulationExportDWGFiles)
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
                        stringList2.Add(fileName + ".dwg");
                    }
                    else
                      stringList2.Add(fileName + ".dwg");
                  }
                  else
                    stringList2.Add(fileName + ".dwg");
                }
                if (newSettings.InsulationExportDXFFiles)
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
                        stringList2.Add(fileName + ".dxf");
                    }
                    else
                      stringList2.Add(fileName + ".dxf");
                  }
                  else
                    stringList2.Add(fileName + ".dxf");
                }
              }
            }
          }
        }
      }
      int num17 = (int) transactionGroup.RollBack();
      if (stringList3.Count > 0)
      {
        TaskDialog taskDialog1 = new TaskDialog("Insulation Export");
        taskDialog1.MainContent = "One or more assemblies was invalid for Insulation Export. Insulation Export only supports structural framing assemblies. Expand for details.";
        taskDialog1.ExpandedContent = "These assemblies were not exported: \n";
        foreach (string str in stringList3)
        {
          TaskDialog taskDialog2 = taskDialog1;
          taskDialog2.ExpandedContent = $"{taskDialog2.ExpandedContent}{str}, ";
        }
        taskDialog1.Show();
      }
      if (stringList2.Count > 0)
      {
        TaskDialog taskDialog3 = new TaskDialog("Insulation Export");
        taskDialog3.MainContent = $"Insulation Export was unable to export one or more CAD files to {newSettings.GetShortFilePath(projectName1, revitDoc.ProjectInformation.Number, true)}. Expand for details.";
        taskDialog3.ExpandedContent = "";
        foreach (string str in stringList2)
        {
          TaskDialog taskDialog4 = taskDialog3;
          taskDialog4.ExpandedContent = taskDialog4.ExpandedContent + str + Environment.NewLine;
        }
        taskDialog3.Show();
      }
      if (stringList1.Count > 0)
      {
        TaskDialog taskDialog5 = new TaskDialog("Insulation Export");
        taskDialog5.MainContent = $"CAD files successfully exported to {newSettings.GetShortFilePath(projectName1, revitDoc.ProjectInformation.Number, false)}. Expand for details.";
        taskDialog5.ExpandedContent = "";
        foreach (string str in stringList1)
        {
          TaskDialog taskDialog6 = taskDialog5;
          taskDialog6.ExpandedContent = taskDialog6.ExpandedContent + str + Environment.NewLine;
        }
        taskDialog5.Show();
      }
    }
    return (Result) 0;
  }

  private enum paramsEnum
  {
    Color,
    LineWeight,
    Background,
    ShowBorder,
    TextFont,
    TextSize,
  }
}
