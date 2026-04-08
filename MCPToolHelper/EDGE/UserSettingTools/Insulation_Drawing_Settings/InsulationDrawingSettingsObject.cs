// Decompiled with JetBrains decompiler
// Type: EDGE.UserSettingTools.Insulation_Drawing_Settings.InsulationDrawingSettingsObject
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace EDGE.UserSettingTools.Insulation_Drawing_Settings;

public class InsulationDrawingSettingsObject
{
  public Category InsulationDetailLineStyle;
  public Category MarkCircleDetailLineStyle;
  public TextNoteType RecessCalloutsTextStyle;
  public TextNoteType InsulationMarkTextStyle;
  public DimensionType OverallDimensionStyle;
  public DimensionType GeneralDimensionStyle;
  public FamilySymbol TitleBlockFamily;
  public int InsulationDrawingScaleFactorMaster;
  public int InsulationDrawingScaleFactorPerPiece;

  public InsulationDrawingSettingsObject(Document revitDoc, InsulationDrawingSettings settings)
  {
    Category category = Category.GetCategory(revitDoc, BuiltInCategory.OST_Lines);
    List<Category> source = new List<Category>();
    foreach (Category subCategory in category.SubCategories)
      source.Add(subCategory);
    this.InsulationDetailLineStyle = source.Where<Category>((Func<Category, bool>) (x => x.Name == settings.InsulationDetailLineStyleName)).FirstOrDefault<Category>();
    this.MarkCircleDetailLineStyle = source.Where<Category>((Func<Category, bool>) (x => x.Name == settings.MarkCircleDetailLineStyleName)).FirstOrDefault<Category>();
    List<string> stringList1 = new List<string>();
    List<TextNoteType> list1 = new FilteredElementCollector(revitDoc).OfClass(typeof (TextNoteType)).Cast<TextNoteType>().ToList<TextNoteType>();
    this.RecessCalloutsTextStyle = list1.Where<TextNoteType>((Func<TextNoteType, bool>) (x => x.Name == settings.RecessCalloutsTextStyleName)).FirstOrDefault<TextNoteType>();
    this.InsulationMarkTextStyle = list1.Where<TextNoteType>((Func<TextNoteType, bool>) (x => x.Name == settings.InsulationMarkTextStyleName)).FirstOrDefault<TextNoteType>();
    List<string> stringList2 = new List<string>();
    List<DimensionType> list2 = new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType)).Cast<DimensionType>().ToList<DimensionType>();
    this.OverallDimensionStyle = list2.Where<DimensionType>((Func<DimensionType, bool>) (x => x.Name == settings.OverallDimensionStyleName)).FirstOrDefault<DimensionType>();
    this.GeneralDimensionStyle = list2.Where<DimensionType>((Func<DimensionType, bool>) (x => x.Name == settings.GeneralDimensionStyleName)).FirstOrDefault<DimensionType>();
    this.TitleBlockFamily = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).Cast<FamilySymbol>().ToList<FamilySymbol>().FirstOrDefault<FamilySymbol>((Func<FamilySymbol, bool>) (s => s.Name == settings.TitleBlockName));
    this.InsulationDrawingScaleFactorMaster = settings.InsulationDrawingScaleFactorMaster;
    this.InsulationDrawingScaleFactorPerPiece = settings.InsulationDrawingScaleFactorPerPiece;
  }

  public InsulationDrawingSettingsObject(Document revitDoc)
  {
    Category category = Category.GetCategory(revitDoc, BuiltInCategory.OST_Lines);
    List<Category> source = new List<Category>();
    foreach (Category subCategory in category.SubCategories)
      source.Add(subCategory);
    this.InsulationDetailLineStyle = source.Where<Category>((Func<Category, bool>) (x => x.Name == "04-NCS Medium")).FirstOrDefault<Category>();
    this.MarkCircleDetailLineStyle = source.Where<Category>((Func<Category, bool>) (x => x.Name == "01-NCS Extra Fine")).FirstOrDefault<Category>();
    List<string> stringList1 = new List<string>();
    List<TextNoteType> list1 = new FilteredElementCollector(revitDoc).OfClass(typeof (TextNoteType)).Cast<TextNoteType>().ToList<TextNoteType>();
    this.RecessCalloutsTextStyle = list1.Where<TextNoteType>((Func<TextNoteType, bool>) (x => x.Name == "PTAC - TICKET TEXT")).FirstOrDefault<TextNoteType>();
    this.InsulationMarkTextStyle = list1.Where<TextNoteType>((Func<TextNoteType, bool>) (x => x.Name == "PTAC - TICKET TEXT")).FirstOrDefault<TextNoteType>();
    List<string> stringList2 = new List<string>();
    List<DimensionType> list2 = new FilteredElementCollector(revitDoc).OfClass(typeof (DimensionType)).Cast<DimensionType>().ToList<DimensionType>();
    this.OverallDimensionStyle = list2.Where<DimensionType>((Func<DimensionType, bool>) (x => x.Name == "PTAC - TICKET (GAP TO ELEMENT)")).FirstOrDefault<DimensionType>();
    this.GeneralDimensionStyle = list2.Where<DimensionType>((Func<DimensionType, bool>) (x => x.Name == "PTAC - TICKET (FIXED TO DIM. LINE)")).FirstOrDefault<DimensionType>();
    this.TitleBlockFamily = new FilteredElementCollector(revitDoc).OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof (FamilySymbol)).Cast<FamilySymbol>().ToList<FamilySymbol>().FirstOrDefault<FamilySymbol>((Func<FamilySymbol, bool>) (s => s.Name == "PS-30X42-CD"));
    this.InsulationDrawingScaleFactorMaster = 32 /*0x20*/;
    this.InsulationDrawingScaleFactorPerPiece = 24;
  }
}
