// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.ExportDWGTestWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Utils.IEnumerable_Extensions;

#nullable disable
namespace EDGE.QATools;

public class ExportDWGTestWindow : Window, IComponentConnector
{
  private Document document;
  private Autodesk.Revit.ApplicationServices.Application revitApp;
  internal TextBlock subCatNameTextBlock;
  internal TextBox subCatNameTextBox;
  internal TextBlock layerNameTextBlock;
  internal TextBox layerNameTextBox;
  internal TextBlock layerColorTextBlock;
  internal TextBox layerColorTextBox;
  internal Button AddToListButton;
  internal TextBlock familyNameTextBlock;
  internal ComboBox familyNameComboBox;
  internal Button AddToSubCatButton;
  internal DataGrid SubCatList;
  private bool _contentLoaded;

  public ExportDWGTestWindow(UIApplication uiApp)
  {
    this.revitApp = uiApp.Application;
    this.document = uiApp.ActiveUIDocument.Document;
    this.InitializeComponent();
  }

  private void Button_Click(object sender, RoutedEventArgs e)
  {
    this.familyUpdate();
    DWGExportOptions predefinedOptions = DWGExportOptions.GetPredefinedOptions(this.document, "AIA");
    ExportLayerTable exportLayerTable = predefinedOptions.GetExportLayerTable();
    string path = "C:\\EDGEforREVIT\\NewSubCategory.txt";
    if (File.Exists(path))
    {
      StreamReader streamReader = new StreamReader(path);
      string str;
      while ((str = streamReader.ReadLine()) != null)
      {
        if (!string.IsNullOrWhiteSpace(str))
        {
          string[] strArray = str.Split(new string[1]{ "," }, StringSplitOptions.RemoveEmptyEntries);
          bool flag = false;
          foreach (ExportLayerKey key in (IEnumerable<ExportLayerKey>) exportLayerTable.GetKeys())
          {
            if (key.CategoryName.Contains(strArray[0]) && key.SubCategoryName == strArray[1])
            {
              ExportLayerInfo exportLayerInfo = exportLayerTable.GetExportLayerInfo(key);
              exportLayerInfo.LayerName = strArray[2];
              exportLayerTable[key] = exportLayerInfo;
              flag = true;
            }
          }
          if (!flag)
          {
            ExportLayerInfo exportLayerInfo = new ExportLayerInfo();
            exportLayerInfo.LayerName = strArray[2];
            exportLayerInfo.ColorNumber = 30;
            ExportLayerKey exportLayerKey = new ExportLayerKey(strArray[0], strArray[1], SpecialType.Default);
            exportLayerTable.Add(exportLayerKey, exportLayerInfo);
          }
          predefinedOptions.SetExportLayerTable(exportLayerTable);
        }
      }
    }
    ICollection<ElementId> views = (ICollection<ElementId>) new List<ElementId>();
    views.Add(this.document.ActiveView.Id);
    if (this.document.Export("C:/EDGEforREVIT/", "ExportedDWGFile", views, predefinedOptions))
    {
      int num1 = (int) MessageBox.Show("Successfully exported to C:/EDGEforREVIT/ExportedDWGFile.dwg");
    }
    else
    {
      int num2 = (int) MessageBox.Show("Export fails!");
    }
    this.Close();
  }

  private void familyUpdate()
  {
    ExportDWGTestWindow.FamilyOperation familyOperation = new ExportDWGTestWindow.FamilyOperation(this.UpdateSubCategory);
    DirectoryInfo directoryInfo = new DirectoryInfo("C:\\test\\bulkUpdater");
    if (!directoryInfo.Exists)
      return;
    foreach (FileInfo file in directoryInfo.GetFiles("*.rfa"))
    {
      Document document1 = this.revitApp.OpenDocumentFile(file.FullName);
      if (document1 != null)
      {
        using (Transaction transaction1 = new Transaction(document1, "Update Family"))
        {
          if (transaction1.Start() == TransactionStatus.Started)
          {
            if (!familyOperation(document1, (object) null))
              QA.LogError("Bulk Family Updater", $"Family file: {file.FullName} failed to update");
            int num1 = (int) transaction1.Commit();
            foreach (FamilyInstance familyInstance in new FilteredElementCollector(document1).OfClass(typeof (FamilyInstance)).Cast<FamilyInstance>().DistinctBy<FamilyInstance, ElementId>((Func<FamilyInstance, ElementId>) (s => s.Symbol.Family.Id)).ToList<FamilyInstance>())
            {
              Family family = familyInstance.Symbol.Family;
              if (family != null && family.FamilyCategory.CategoryType == CategoryType.Model)
              {
                Document document2 = document1.EditFamily(family);
                if (document2 != null)
                {
                  using (Transaction transaction2 = new Transaction(document2, "UpdateNestedFamily"))
                  {
                    int num2 = (int) transaction2.Start();
                    if (familyOperation(document2, (object) null))
                    {
                      int num3 = (int) transaction2.Commit();
                      ExportDWGTestWindow.FamilyLoadOptions familyLoadOptions = new ExportDWGTestWindow.FamilyLoadOptions();
                      document2.LoadFamily(document1, (IFamilyLoadOptions) familyLoadOptions);
                    }
                  }
                }
              }
            }
          }
          else
            continue;
        }
        document1?.Close(true);
      }
    }
  }

  private bool UpdateSubCategory(Document familyDoc, object data)
  {
    try
    {
      if (!familyDoc.IsFamilyDocument)
        return false;
      string str = "SubCatRebarTest";
      Category familyCategory = familyDoc.OwnerFamily.FamilyCategory;
      Category category = (Category) null;
      if (familyCategory.SubCategories.Contains(str))
        category = familyCategory.SubCategories.get_Item(str);
      if (category == null && familyCategory != null && familyCategory.CanAddSubcategory)
        category = familyDoc.Settings.Categories.NewSubcategory(familyCategory, str);
      if (category == null)
        return false;
      foreach (GenericForm genericForm in (IEnumerable<Element>) new FilteredElementCollector(familyDoc).OfClass(typeof (GenericForm)).ToList<Element>())
      {
        switch (genericForm)
        {
          case Sweep _:
            (genericForm as Sweep).Subcategory = category;
            continue;
          case Extrusion _:
            (genericForm as Extrusion).Subcategory = category;
            continue;
          case Revolution _:
            (genericForm as Revolution).Subcategory = category;
            continue;
          default:
            continue;
        }
      }
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append($"{familyCategory.Name},{category.Name}");
      stringBuilder.Append(familyCategory.Name + ",SubCatRebar");
      string path = "C:\\EDGEforREVIT\\NewSubCategory.txt";
      if (!File.Exists(path))
        File.Create(path).Close();
      using (StreamWriter streamWriter = new StreamWriter(path))
        streamWriter.WriteLine(stringBuilder.ToString());
      return true;
    }
    catch (Exception ex)
    {
      int num = (int) MessageBox.Show(ex.ToString());
      return false;
    }
  }

  private void AddToSubCatButton_Click(object sender, RoutedEventArgs e)
  {
  }

  private void AddToListButton_Click(object sender, RoutedEventArgs e)
  {
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    System.Windows.Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/qatools/exportdwgtestwindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.subCatNameTextBlock = (TextBlock) target;
        break;
      case 2:
        this.subCatNameTextBox = (TextBox) target;
        break;
      case 3:
        this.layerNameTextBlock = (TextBlock) target;
        break;
      case 4:
        this.layerNameTextBox = (TextBox) target;
        break;
      case 5:
        this.layerColorTextBlock = (TextBlock) target;
        break;
      case 6:
        this.layerColorTextBox = (TextBox) target;
        break;
      case 7:
        this.AddToListButton = (Button) target;
        this.AddToListButton.Click += new RoutedEventHandler(this.AddToListButton_Click);
        break;
      case 8:
        this.familyNameTextBlock = (TextBlock) target;
        break;
      case 9:
        this.familyNameComboBox = (ComboBox) target;
        break;
      case 10:
        this.AddToSubCatButton = (Button) target;
        this.AddToSubCatButton.Click += new RoutedEventHandler(this.AddToSubCatButton_Click);
        break;
      case 11:
        this.SubCatList = (DataGrid) target;
        break;
      case 12:
        ((ButtonBase) target).Click += new RoutedEventHandler(this.Button_Click);
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }

  private class FamilyLoadOptions : IFamilyLoadOptions
  {
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
    {
      overwriteParameterValues = true;
      return true;
    }

    public bool OnSharedFamilyFound(
      Family sharedFamily,
      bool familyInUse,
      out FamilySource source,
      out bool overwriteParameterValues)
    {
      source = FamilySource.Family;
      overwriteParameterValues = true;
      return true;
    }
  }

  private delegate bool FamilyOperation(Document doc, object data);
}
