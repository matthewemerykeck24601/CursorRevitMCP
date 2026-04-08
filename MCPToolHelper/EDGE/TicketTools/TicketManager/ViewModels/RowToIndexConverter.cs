// Decompiled with JetBrains decompiler
// Type: EDGE.TicketTools.TicketManager.ViewModels.RowToIndexConverter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace EDGE.TicketTools.TicketManager.ViewModels;

public class RowToIndexConverter : MarkupExtension, IValueConverter
{
  private static RowToIndexConverter converter;

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value is DataGridRow dataGridRow ? (object) (dataGridRow.GetIndex() + 1) : (object) -1;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    return (object) RowToIndexConverter.converter ?? (object) (RowToIndexConverter.converter = new RowToIndexConverter());
  }
}
