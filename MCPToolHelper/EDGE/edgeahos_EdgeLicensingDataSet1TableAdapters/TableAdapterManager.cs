// Decompiled with JetBrains decompiler
// Type: EDGE.edgeahos_EdgeLicensingDataSet1TableAdapters.TableAdapterManager
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

#nullable disable
namespace EDGE.edgeahos_EdgeLicensingDataSet1TableAdapters;

[DesignerCategory("code")]
[ToolboxItem(true)]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[HelpKeyword("vs.data.TableAdapterManager")]
public class TableAdapterManager : Component
{
  private TableAdapterManager.UpdateOrderOption _updateOrder;
  private LicenseTableAdapter _licenseTableAdapter;
  private LicenseTableTableAdapter _licenseTableTableAdapter;
  private MachinesTableAdapter _machinesTableAdapter;
  private bool _backupDataSetBeforeUpdate;
  private IDbConnection _connection;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public TableAdapterManager.UpdateOrderOption UpdateOrder
  {
    get => this._updateOrder;
    set => this._updateOrder = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  public LicenseTableAdapter LicenseTableAdapter
  {
    get => this._licenseTableAdapter;
    set => this._licenseTableAdapter = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  public LicenseTableTableAdapter LicenseTableTableAdapter
  {
    get => this._licenseTableTableAdapter;
    set => this._licenseTableTableAdapter = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  public MachinesTableAdapter MachinesTableAdapter
  {
    get => this._machinesTableAdapter;
    set => this._machinesTableAdapter = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public bool BackupDataSetBeforeUpdate
  {
    get => this._backupDataSetBeforeUpdate;
    set => this._backupDataSetBeforeUpdate = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Browsable(false)]
  public IDbConnection Connection
  {
    get
    {
      if (this._connection != null)
        return this._connection;
      if (this._licenseTableAdapter != null && this._licenseTableAdapter.Connection != null)
        return (IDbConnection) this._licenseTableAdapter.Connection;
      if (this._licenseTableTableAdapter != null && this._licenseTableTableAdapter.Connection != null)
        return (IDbConnection) this._licenseTableTableAdapter.Connection;
      return this._machinesTableAdapter != null && this._machinesTableAdapter.Connection != null ? (IDbConnection) this._machinesTableAdapter.Connection : (IDbConnection) null;
    }
    set => this._connection = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Browsable(false)]
  public int TableAdapterInstanceCount
  {
    get
    {
      int adapterInstanceCount = 0;
      if (this._licenseTableAdapter != null)
        ++adapterInstanceCount;
      if (this._licenseTableTableAdapter != null)
        ++adapterInstanceCount;
      if (this._machinesTableAdapter != null)
        ++adapterInstanceCount;
      return adapterInstanceCount;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private int UpdateUpdatedRows(
    edgeahos_EdgeLicensingDataSet1 dataSet,
    List<DataRow> allChangedRows,
    List<DataRow> allAddedRows)
  {
    int num = 0;
    if (this._licenseTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.License.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && realUpdatedRows.Length != 0)
      {
        num += this._licenseTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    if (this._licenseTableTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.LicenseTable.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && realUpdatedRows.Length != 0)
      {
        num += this._licenseTableTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    if (this._machinesTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.Machines.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && realUpdatedRows.Length != 0)
      {
        num += this._machinesTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    return num;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private int UpdateInsertedRows(edgeahos_EdgeLicensingDataSet1 dataSet, List<DataRow> allAddedRows)
  {
    int num = 0;
    if (this._licenseTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.License.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && dataRowArray.Length != 0)
      {
        num += this._licenseTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._licenseTableTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.LicenseTable.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && dataRowArray.Length != 0)
      {
        num += this._licenseTableTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._machinesTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.Machines.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && dataRowArray.Length != 0)
      {
        num += this._machinesTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    return num;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private int UpdateDeletedRows(
    edgeahos_EdgeLicensingDataSet1 dataSet,
    List<DataRow> allChangedRows)
  {
    int num = 0;
    if (this._machinesTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.Machines.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && dataRowArray.Length != 0)
      {
        num += this._machinesTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._licenseTableTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.LicenseTable.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && dataRowArray.Length != 0)
      {
        num += this._licenseTableTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._licenseTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.License.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && dataRowArray.Length != 0)
      {
        num += this._licenseTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    return num;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private DataRow[] GetRealUpdatedRows(DataRow[] updatedRows, List<DataRow> allAddedRows)
  {
    if (updatedRows == null || updatedRows.Length < 1 || allAddedRows == null || allAddedRows.Count < 1)
      return updatedRows;
    List<DataRow> dataRowList = new List<DataRow>();
    for (int index = 0; index < updatedRows.Length; ++index)
    {
      DataRow updatedRow = updatedRows[index];
      if (!allAddedRows.Contains(updatedRow))
        dataRowList.Add(updatedRow);
    }
    return dataRowList.ToArray();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public virtual int UpdateAll(edgeahos_EdgeLicensingDataSet1 dataSet)
  {
    if (dataSet == null)
      throw new ArgumentNullException(nameof (dataSet));
    if (!dataSet.HasChanges())
      return 0;
    if (this._licenseTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._licenseTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    if (this._licenseTableTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._licenseTableTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    if (this._machinesTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._machinesTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    IDbConnection connection = this.Connection;
    if (connection == null)
      throw new ApplicationException("TableAdapterManager contains no connection information. Set each TableAdapterManager TableAdapter property to a valid TableAdapter instance.");
    bool flag = false;
    if ((connection.State & ConnectionState.Broken) == ConnectionState.Broken)
      connection.Close();
    if (connection.State == ConnectionState.Closed)
    {
      connection.Open();
      flag = true;
    }
    IDbTransaction dbTransaction = connection.BeginTransaction();
    if (dbTransaction == null)
      throw new ApplicationException("The transaction cannot begin. The current data connection does not support transactions or the current state is not allowing the transaction to begin.");
    List<DataRow> allChangedRows = new List<DataRow>();
    List<DataRow> allAddedRows = new List<DataRow>();
    List<DataAdapter> dataAdapterList = new List<DataAdapter>();
    Dictionary<object, IDbConnection> dictionary = new Dictionary<object, IDbConnection>();
    int num = 0;
    DataSet dataSet1 = (DataSet) null;
    if (this.BackupDataSetBeforeUpdate)
    {
      dataSet1 = new DataSet();
      dataSet1.Merge((DataSet) dataSet);
    }
    try
    {
      if (this._licenseTableAdapter != null)
      {
        dictionary.Add((object) this._licenseTableAdapter, (IDbConnection) this._licenseTableAdapter.Connection);
        this._licenseTableAdapter.Connection = (SqlConnection) connection;
        this._licenseTableAdapter.Transaction = (SqlTransaction) dbTransaction;
        if (this._licenseTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._licenseTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._licenseTableAdapter.Adapter);
        }
      }
      if (this._licenseTableTableAdapter != null)
      {
        dictionary.Add((object) this._licenseTableTableAdapter, (IDbConnection) this._licenseTableTableAdapter.Connection);
        this._licenseTableTableAdapter.Connection = (SqlConnection) connection;
        this._licenseTableTableAdapter.Transaction = (SqlTransaction) dbTransaction;
        if (this._licenseTableTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._licenseTableTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._licenseTableTableAdapter.Adapter);
        }
      }
      if (this._machinesTableAdapter != null)
      {
        dictionary.Add((object) this._machinesTableAdapter, (IDbConnection) this._machinesTableAdapter.Connection);
        this._machinesTableAdapter.Connection = (SqlConnection) connection;
        this._machinesTableAdapter.Transaction = (SqlTransaction) dbTransaction;
        if (this._machinesTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._machinesTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._machinesTableAdapter.Adapter);
        }
      }
      if (this.UpdateOrder == TableAdapterManager.UpdateOrderOption.UpdateInsertDelete)
      {
        num += this.UpdateUpdatedRows(dataSet, allChangedRows, allAddedRows);
        num += this.UpdateInsertedRows(dataSet, allAddedRows);
      }
      else
      {
        num += this.UpdateInsertedRows(dataSet, allAddedRows);
        num += this.UpdateUpdatedRows(dataSet, allChangedRows, allAddedRows);
      }
      num += this.UpdateDeletedRows(dataSet, allChangedRows);
      dbTransaction.Commit();
      if (0 < allAddedRows.Count)
      {
        DataRow[] array = new DataRow[allAddedRows.Count];
        allAddedRows.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
          array[index].AcceptChanges();
      }
      if (0 < allChangedRows.Count)
      {
        DataRow[] array = new DataRow[allChangedRows.Count];
        allChangedRows.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
          array[index].AcceptChanges();
      }
    }
    catch (Exception ex)
    {
      dbTransaction.Rollback();
      if (this.BackupDataSetBeforeUpdate)
      {
        dataSet.Clear();
        dataSet.Merge(dataSet1);
      }
      else if (0 < allAddedRows.Count)
      {
        DataRow[] array = new DataRow[allAddedRows.Count];
        allAddedRows.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
        {
          DataRow dataRow = array[index];
          dataRow.AcceptChanges();
          dataRow.SetAdded();
        }
      }
      throw ex;
    }
    finally
    {
      if (flag)
        connection.Close();
      if (this._licenseTableAdapter != null)
      {
        this._licenseTableAdapter.Connection = (SqlConnection) dictionary[(object) this._licenseTableAdapter];
        this._licenseTableAdapter.Transaction = (SqlTransaction) null;
      }
      if (this._licenseTableTableAdapter != null)
      {
        this._licenseTableTableAdapter.Connection = (SqlConnection) dictionary[(object) this._licenseTableTableAdapter];
        this._licenseTableTableAdapter.Transaction = (SqlTransaction) null;
      }
      if (this._machinesTableAdapter != null)
      {
        this._machinesTableAdapter.Connection = (SqlConnection) dictionary[(object) this._machinesTableAdapter];
        this._machinesTableAdapter.Transaction = (SqlTransaction) null;
      }
      if (0 < dataAdapterList.Count)
      {
        DataAdapter[] array = new DataAdapter[dataAdapterList.Count];
        dataAdapterList.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
          array[index].AcceptChangesDuringUpdate = true;
      }
    }
    return num;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected virtual void SortSelfReferenceRows(
    DataRow[] rows,
    DataRelation relation,
    bool childFirst)
  {
    Array.Sort<DataRow>(rows, (IComparer<DataRow>) new TableAdapterManager.SelfReferenceComparer(relation, childFirst));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected virtual bool MatchTableAdapterConnection(IDbConnection inputConnection)
  {
    return this._connection != null || this.Connection == null || inputConnection == null || string.Equals(this.Connection.ConnectionString, inputConnection.ConnectionString, StringComparison.Ordinal);
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public enum UpdateOrderOption
  {
    InsertUpdateDelete,
    UpdateInsertDelete,
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private class SelfReferenceComparer : IComparer<DataRow>
  {
    private DataRelation _relation;
    private int _childFirst;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal SelfReferenceComparer(DataRelation relation, bool childFirst)
    {
      this._relation = relation;
      if (childFirst)
        this._childFirst = -1;
      else
        this._childFirst = 1;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    private DataRow GetRoot(DataRow row, out int distance)
    {
      DataRow root = row;
      distance = 0;
      IDictionary<DataRow, DataRow> dictionary = (IDictionary<DataRow, DataRow>) new Dictionary<DataRow, DataRow>();
      dictionary[row] = row;
      for (DataRow parentRow = row.GetParentRow(this._relation, DataRowVersion.Default); parentRow != null && !dictionary.ContainsKey(parentRow); parentRow = parentRow.GetParentRow(this._relation, DataRowVersion.Default))
      {
        ++distance;
        root = parentRow;
        dictionary[parentRow] = parentRow;
      }
      if (distance == 0)
      {
        dictionary.Clear();
        dictionary[row] = row;
        for (DataRow parentRow = row.GetParentRow(this._relation, DataRowVersion.Original); parentRow != null && !dictionary.ContainsKey(parentRow); parentRow = parentRow.GetParentRow(this._relation, DataRowVersion.Original))
        {
          ++distance;
          root = parentRow;
          dictionary[parentRow] = parentRow;
        }
      }
      return root;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public int Compare(DataRow row1, DataRow row2)
    {
      if (row1 == row2)
        return 0;
      if (row1 == null)
        return -1;
      if (row2 == null)
        return 1;
      int distance1 = 0;
      DataRow root1 = this.GetRoot(row1, out distance1);
      int distance2 = 0;
      DataRow root2 = this.GetRoot(row2, out distance2);
      if (root1 == root2)
        return this._childFirst * distance1.CompareTo(distance2);
      return root1.Table.Rows.IndexOf(root1) < root2.Table.Rows.IndexOf(root2) ? -1 : 1;
    }
  }
}
