// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.CopyTicketAnnotation.CopyTicketAnnotations
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

#nullable disable
namespace EDGE.TicketTools.CopyTicketAnnotation;

[Transaction(TransactionMode.Manual)]
internal class CopyTicketAnnotations
{
  public static void copyTicketAnnotations(
    Element viewSheetCopyFrom,
    Element viewSheetCopyTo,
    bool cloneTicketView = false,
    bool cloneTicketSheet = false)
  {
    try
    {
      string g = "f84b5886-3580-4842-9f4b-9f08ce6644d8";
      Schema schema = Schema.Lookup(new Guid(g));
      if (schema == null)
      {
        SchemaBuilder schemaBuilder = new SchemaBuilder(new Guid("f84b5886-3580-4842-9f4b-9f08ce6644d8"));
        schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
        schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
        schemaBuilder.AddSimpleField("PTACDimensionGuid", typeof (string)).SetDocumentation("Identifier for PTAC's dimension labels.");
        schemaBuilder.SetSchemaName("PTAC_Dimensions");
        schema = schemaBuilder.Finish();
      }
      Document document = viewSheetCopyFrom.Document;
      View view = viewSheetCopyFrom as View;
      XYZ origin1 = (document.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance).GetTransform().Origin;
      View destinationView = viewSheetCopyTo as View;
      XYZ origin2 = (document.GetElement(destinationView.AssociatedAssemblyInstanceId) as AssemblyInstance).GetTransform().Origin;
      if (view.ViewType == ViewType.ThreeD || view.ViewType == ViewType.Schedule)
        return;
      Line bound = Line.CreateBound(origin1, origin1 + view.UpDirection);
      DetailCurve detailCurve = document.Create.NewDetailCurve(view, (Curve) bound);
      if (document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories.Contains("EdgeRotationLine"))
      {
        detailCurve.LineStyle = (Element) document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories.get_Item("EdgeRotationLine").GetGraphicsStyle(GraphicsStyleType.Projection);
        FilteredElementCollector elementCollector = new FilteredElementCollector(document, view.Id).OwnedByView(view.Id);
        List<ElementId> elementsToCopy = new List<ElementId>();
        foreach (Element element in elementCollector)
        {
          if (!cloneTicketSheet && !cloneTicketView)
          {
            if (element.Category != null && (element is TextNote || element is FilledRegion || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_DetailComponents).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_GenericAnnotation).Id))
            {
              if (element is TextNote && element.GetEntitySchemaGuids().Contains(new Guid(g)))
              {
                Entity entity = element.GetEntity(schema);
                if (entity != null && entity.Get<string>(schema.GetField("PTACDimensionGuid")).Equals(element.UniqueId))
                  continue;
              }
              elementsToCopy.Add(element.Id);
            }
          }
          else if (cloneTicketView)
          {
            if (element.Category != null && (element is TextNote || element is FilledRegion || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_DetailComponents).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_GenericAnnotation).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_RevisionClouds).Id))
            {
              if (element is TextNote && element.GetEntitySchemaGuids().Contains(new Guid(g)))
              {
                Entity entity = element.GetEntity(schema);
                if (entity != null && entity.Get<string>(schema.GetField("PTACDimensionGuid")).Equals(element.UniqueId))
                  continue;
              }
              if (!(element is AnnotationSymbol))
                elementsToCopy.Add(element.Id);
            }
          }
          else if (element.Category != null && (element is TextNote || element is FilledRegion || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_DetailComponents).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_GenericAnnotation).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_RevisionClouds).Id || element.Category.Id == document.Settings.Categories.get_Item(BuiltInCategory.OST_MultiCategoryTags).Id))
          {
            if (element is TextNote && element.GetEntitySchemaGuids().Contains(new Guid(g)))
            {
              Entity entity = element.GetEntity(schema);
              if (entity != null && entity.Get<string>(schema.GetField("PTACDimensionGuid")).Equals(element.UniqueId))
                continue;
            }
            if (!(element is AnnotationSymbol))
              elementsToCopy.Add(element.Id);
          }
        }
        CopyPasteOptions options = new CopyPasteOptions();
        ICollection<ElementId> elementIds = ElementTransformUtils.CopyElements(view, (ICollection<ElementId>) elementsToCopy, destinationView, Transform.Identity, options);
        XYZ xyz1 = new XYZ();
        XYZ xyz2 = new XYZ();
        foreach (ElementId id in (IEnumerable<ElementId>) elementIds)
        {
          if (document.GetElement(id) is DetailCurve && (document.GetElement(id) as DetailCurve).LineStyle.Name == "EdgeRotationLine")
          {
            DetailCurve element = document.GetElement(id) as DetailCurve;
            Line geometryCurve = element.GeometryCurve as Line;
            XYZ origin3 = geometryCurve.Origin;
            XYZ direction = geometryCurve.Direction;
            double num = destinationView.RightDirection.DotProduct(direction);
            double d = Math.Max(Math.Min(destinationView.UpDirection.DotProduct(direction), 1.0), -1.0);
            double angle = num <= 0.0 ? -Math.Acos(d) : Math.Acos(d);
            ElementTransformUtils.RotateElements(document, elementIds, Line.CreateBound(origin3, origin3 + destinationView.ViewDirection), angle);
            XYZ translation = origin2 - origin3;
            ElementTransformUtils.MoveElements(document, elementIds, translation);
            document.Delete(detailCurve.Id);
            document.Delete(element.Id);
            break;
          }
        }
      }
      else
      {
        TaskDialog.Show("Copy Ticket Annotations", "Line Style \"EdgeRotationLine\" not found, please run the shared parameter updater and try again.");
        document.Delete(detailCurve.Id);
      }
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
    }
  }

  public static void copyTicketAnnotationNew(
    Element viewSheetCopyFrom,
    Element viewSheetCopyTo,
    UIApplication uiApp,
    out StringBuilder sb)
  {
    sb = new StringBuilder();
    try
    {
      Document document = uiApp.ActiveUIDocument.Document;
      View getFrom = viewSheetCopyFrom as View;
      XYZ origin1 = (document.GetElement(getFrom.AssociatedAssemblyInstanceId) as AssemblyInstance).GetTransform().Origin;
      FilteredElementCollector elementCollector = new FilteredElementCollector(document, getFrom.Id).OwnedByView(getFrom.Id);
      List<TextNote> textNoteList = new List<TextNote>();
      List<DetailLine> detailLineList = new List<DetailLine>();
      List<DetailArc> detailArcList = new List<DetailArc>();
      foreach (Element element in elementCollector)
      {
        switch (element)
        {
          case TextNote _:
            textNoteList.Add(element as TextNote);
            continue;
          case DetailLine _:
            detailLineList.Add(element as DetailLine);
            continue;
          case DetailArc _:
            detailArcList.Add(element as DetailArc);
            continue;
          default:
            continue;
        }
      }
      View view = viewSheetCopyTo as View;
      XYZ origin2 = (document.GetElement(view.AssociatedAssemblyInstanceId) as AssemblyInstance).GetTransform().Origin;
      CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
      List<ElementId> elementIdList = new List<ElementId>();
      foreach (TextNote textNote in textNoteList)
        elementIdList.Add(textNote.Id);
      foreach (DetailArc detailArc in detailArcList)
        elementIdList.Add(detailArc.Id);
      foreach (DetailLine detailLine in detailLineList)
        elementIdList.Add(detailLine.Id);
      FilteredElementCollector source1 = new FilteredElementCollector(document, getFrom.Id);
      source1.OfClass(typeof (FilledRegion));
      source1.Count<Element>();
      foreach (FilledRegion filledRegion in source1)
        elementIdList.Add(filledRegion.Id);
      FilteredElementCollector source2 = new FilteredElementCollector(document, getFrom.Id);
      source2.OfCategory(BuiltInCategory.OST_DetailComponents);
      source2.Count<Element>();
      foreach (Element element in source2)
      {
        if (element.Location is LocationPoint)
          elementIdList.Add(element.Id);
      }
      FilteredElementCollector source3 = new FilteredElementCollector(document, getFrom.Id);
      source3.OfCategory(BuiltInCategory.OST_GenericAnnotation);
      source3.Count<Element>();
      foreach (Element element in source3)
        elementIdList.Add(element.Id);
      ViewSection viewSection = getFrom as ViewSection;
      using (Transaction transaction = new Transaction(document, "Set cropbox"))
      {
        int num1 = (int) transaction.Start();
        if (viewSection != null)
        {
          BoundingBoxXYZ boundingBoxXyz = new BoundingBoxXYZ();
          boundingBoxXyz.Max = new XYZ(view.CropBox.Max.X, view.CropBox.Max.Y, view.CropBox.Max.Z);
          boundingBoxXyz.Min = new XYZ(view.CropBox.Min.X, view.CropBox.Min.Y, view.CropBox.Min.Z);
          Transform identity = Transform.Identity;
          identity.Origin = new XYZ(view.CropBox.Transform.Origin.X, view.CropBox.Transform.Origin.Y, view.CropBox.Transform.Origin.Z);
          identity.BasisX = view.CropBox.Transform.BasisX + XYZ.Zero;
          identity.BasisY = view.CropBox.Transform.BasisY + XYZ.Zero;
          identity.BasisZ = view.CropBox.Transform.BasisZ + XYZ.Zero;
          viewSection.CropBox = boundingBoxXyz;
          viewSection.CropBox.Transform = identity;
          document.Regenerate();
        }
        int num2 = (int) transaction.Commit();
      }
      Dictionary<string, string> dictionary = new Dictionary<string, string>();
      using (TransactionGroup transactionGroup = new TransactionGroup(document))
      {
        int num3 = (int) transactionGroup.Start("Copy Annotations and Dimensions per view");
        foreach (ElementId id in elementIdList)
        {
          using (Transaction transaction = new Transaction(document, "Copy Annotations"))
          {
            int num4 = (int) transaction.Start();
            FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
            failureHandlingOptions.SetFailuresPreprocessor((IFailuresPreprocessor) new WarningSwallower());
            transaction.SetFailureHandlingOptions(failureHandlingOptions);
            try
            {
              ViewSection sourceView = viewSection;
              List<ElementId> elementsToCopy = new List<ElementId>();
              elementsToCopy.Add(id);
              View destinationView = view;
              CopyPasteOptions options = copyPasteOptions;
              ICollection<ElementId> source4 = ElementTransformUtils.CopyElements((View) sourceView, (ICollection<ElementId>) elementsToCopy, destinationView, (Transform) null, options);
              Element element1 = document.GetElement(id);
              Element element2 = document.GetElement(source4.First<ElementId>());
              dictionary.Add(element1.UniqueId, element2.UniqueId);
            }
            catch (Exception ex)
            {
              int num5 = (int) MessageBox.Show(ex.ToString());
            }
            try
            {
              int num6 = (int) transaction.Commit();
            }
            catch
            {
              int num7 = (int) transaction.RollBack();
            }
          }
        }
        IEnumerable<Dimension> source5 = new FilteredElementCollector(document).OfClass(typeof (Dimension)).ToElements().Cast<Dimension>().Where<Dimension>((Func<Dimension, bool>) (dim => dim.View != null && dim.View.Id == getFrom.Id));
        source5.Count<Dimension>();
        List<Dimension> dimensionList = new List<Dimension>();
        foreach (Element element3 in source5)
        {
          if (element3 is Dimension)
          {
            Dimension dimension = element3 as Dimension;
            dimensionList.Add(dimension);
            DimensionType dimensionType = dimension.DimensionType;
            Curve curve = dimension.Curve;
            if (!((GeometryObject) curve == (GeometryObject) null) && !curve.IsCyclic)
            {
              Line unbound = Line.CreateUnbound(dimension.NumberOfSegments <= 1 ? dimension.Origin : dimension.Segments.get_Item(0).Origin, curve.ComputeDerivatives(0.5, false).BasisX);
              ReferenceArray references1 = dimension.References;
              Options options = new Options();
              options.ComputeReferences = true;
              options.DetailLevel = ViewDetailLevel.Undefined;
              options.IncludeNonVisibleObjects = false;
              ReferenceArray references2 = new ReferenceArray();
              foreach (Reference reference in references1)
              {
                bool flag = false;
                string stableRepresentation1 = reference.ConvertToStableRepresentation(document);
                Reference stableRepresentation2 = Reference.ParseFromStableRepresentation(document, stableRepresentation1);
                Element element4 = document.GetElement(stableRepresentation2.ElementId);
                GeometryElement source6 = element4.get_Geometry(options);
                if ((GeometryObject) source6 != (GeometryObject) null)
                {
                  foreach (object obj in source6.ToArray<GeometryObject>())
                  {
                    if (obj.GetType().Name.ToUpper() == "LINE")
                    {
                      flag = true;
                      string[] strArray = stableRepresentation1.Split(':');
                      if (dictionary.ContainsKey(element4.UniqueId))
                      {
                        string str = dictionary[element4.UniqueId];
                        strArray[0] = str;
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int index = 0; index < strArray.Length; ++index)
                        {
                          if (index == strArray.Length - 1)
                            stringBuilder.Append(strArray[index]);
                          else
                            stringBuilder.Append(strArray[index] + ":");
                        }
                        Reference stableRepresentation3 = Reference.ParseFromStableRepresentation(document, stringBuilder.ToString());
                        references2.Append(stableRepresentation3);
                      }
                    }
                  }
                  if (!flag)
                    references2.Append(reference);
                }
              }
              using (Transaction transaction = new Transaction(document, "copy dimensions"))
              {
                int num8 = (int) transaction.Start();
                try
                {
                  document.Create.NewDimension(view, unbound, references2, dimension.DimensionType);
                }
                catch
                {
                }
                int num9 = (int) transaction.Commit();
              }
            }
          }
        }
        int num10 = (int) transactionGroup.Assimilate();
      }
    }
    catch (Exception ex)
    {
    }
  }
}
