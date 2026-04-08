// Decompiled with JetBrains decompiler
// Type: EDGE.edgeahos_EdgeLicensingDataSet1TableAdapters.MachinesTableAdapter
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using EDGE.Properties;
using System;
using System.CodeDom.Compiler;
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
[DataObject(true)]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[HelpKeyword("vs.data.TableAdapter")]
public class MachinesTableAdapter : Component
{
  private SqlDataAdapter _adapter;
  private SqlConnection _connection;
  private SqlTransaction _transaction;
  private SqlCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public MachinesTableAdapter() => this.ClearBeforeFill = true;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected internal SqlDataAdapter Adapter
  {
    get
    {
      if (this._adapter == null)
        this.InitAdapter();
      return this._adapter;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  internal SqlConnection Connection
  {
    get
    {
      if (this._connection == null)
        this.InitConnection();
      return this._connection;
    }
    set
    {
      this._connection = value;
      if (this.Adapter.InsertCommand != null)
        this.Adapter.InsertCommand.Connection = value;
      if (this.Adapter.DeleteCommand != null)
        this.Adapter.DeleteCommand.Connection = value;
      if (this.Adapter.UpdateCommand != null)
        this.Adapter.UpdateCommand.Connection = value;
      for (int index = 0; index < this.CommandCollection.Length; ++index)
      {
        if (this.CommandCollection[index] != null)
          this.CommandCollection[index].Connection = value;
      }
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  internal SqlTransaction Transaction
  {
    get => this._transaction;
    set
    {
      this._transaction = value;
      for (int index = 0; index < this.CommandCollection.Length; ++index)
        this.CommandCollection[index].Transaction = this._transaction;
      if (this.Adapter != null && this.Adapter.DeleteCommand != null)
        this.Adapter.DeleteCommand.Transaction = this._transaction;
      if (this.Adapter != null && this.Adapter.InsertCommand != null)
        this.Adapter.InsertCommand.Transaction = this._transaction;
      if (this.Adapter == null || this.Adapter.UpdateCommand == null)
        return;
      this.Adapter.UpdateCommand.Transaction = this._transaction;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected SqlCommand[] CommandCollection
  {
    get
    {
      if (this._commandCollection == null)
        this.InitCommandCollection();
      return this._commandCollection;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public bool ClearBeforeFill
  {
    get => this._clearBeforeFill;
    set => this._clearBeforeFill = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private void InitAdapter()
  {
    this._adapter = new SqlDataAdapter();
    this._adapter.TableMappings.Add((object) new DataTableMapping()
    {
      SourceTable = "Table",
      DataSetTable = "Machines",
      ColumnMappings = {
        {
          "Id",
          "Id"
        },
        {
          "MachineID",
          "MachineID"
        }
      }
    });
    this._adapter.DeleteCommand = new SqlCommand();
    this._adapter.DeleteCommand.Connection = this.Connection;
    this._adapter.DeleteCommand.CommandText = "DELETE FROM [edgeahos_ptacdevelopment].[Machines] WHERE (([Id] = @Original_Id) AND ((@IsNull_MachineID = 1 AND [MachineID] IS NULL) OR ([MachineID] = @Original_MachineID)))";
    this._adapter.DeleteCommand.CommandType = CommandType.Text;
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_Id", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Id", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@IsNull_MachineID", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineID", DataRowVersion.Original, true, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_MachineID", SqlDbType.NChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineID", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.InsertCommand = new SqlCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO [edgeahos_ptacdevelopment].[Machines] ([Id], [MachineID]) VALUES (@Id, @MachineID);\r\nSELECT Id, MachineID FROM Machines WHERE (Id = @Id)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Id", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@MachineID", SqlDbType.NChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineID", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand = new SqlCommand();
    this._adapter.UpdateCommand.Connection = this.Connection;
    this._adapter.UpdateCommand.CommandText = "UPDATE [edgeahos_ptacdevelopment].[Machines] SET [Id] = @Id, [MachineID] = @MachineID WHERE (([Id] = @Original_Id) AND ((@IsNull_MachineID = 1 AND [MachineID] IS NULL) OR ([MachineID] = @Original_MachineID)));\r\nSELECT Id, MachineID FROM Machines WHERE (Id = @Id)";
    this._adapter.UpdateCommand.CommandType = CommandType.Text;
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Id", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@MachineID", SqlDbType.NChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineID", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_Id", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Id", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@IsNull_MachineID", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineID", DataRowVersion.Original, true, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_MachineID", SqlDbType.NChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineID", DataRowVersion.Original, false, (object) null, "", "", ""));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private void InitConnection()
  {
    this._connection = new SqlConnection();
    this._connection.ConnectionString = Settings.Default.edgeahos_EdgeLicensingConnectionString;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private void InitCommandCollection()
  {
    this._commandCollection = new SqlCommand[1];
    this._commandCollection[0] = new SqlCommand();
    this._commandCollection[0].Connection = this.Connection;
    this._commandCollection[0].CommandText = "SELECT Id, MachineID FROM edgeahos_ptacdevelopment.Machines";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  public virtual int Fill(
    edgeahos_EdgeLicensingDataSet1.MachinesDataTable dataTable)
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    if (this.ClearBeforeFill)
      dataTable.Clear();
    return this.Adapter.Fill((DataTable) dataTable);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Select, true)]
  public virtual edgeahos_EdgeLicensingDataSet1.MachinesDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    edgeahos_EdgeLicensingDataSet1.MachinesDataTable data = new edgeahos_EdgeLicensingDataSet1.MachinesDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(
    edgeahos_EdgeLicensingDataSet1.MachinesDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(edgeahos_EdgeLicensingDataSet1 dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "Machines");
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(DataRow dataRow)
  {
    return this.Adapter.Update(new DataRow[1]{ dataRow });
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(DataRow[] dataRows) => this.Adapter.Update(dataRows);

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Delete, true)]
  public virtual int Delete(int Original_Id, string Original_MachineID)
  {
    this.Adapter.DeleteCommand.Parameters[0].Value = (object) Original_Id;
    if (Original_MachineID == null)
    {
      this.Adapter.DeleteCommand.Parameters[1].Value = (object) 1;
      this.Adapter.DeleteCommand.Parameters[2].Value = (object) DBNull.Value;
    }
    else
    {
      this.Adapter.DeleteCommand.Parameters[1].Value = (object) 0;
      this.Adapter.DeleteCommand.Parameters[2].Value = (object) Original_MachineID;
    }
    ConnectionState state = this.Adapter.DeleteCommand.Connection.State;
    if ((this.Adapter.DeleteCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      this.Adapter.DeleteCommand.Connection.Open();
    try
    {
      return this.Adapter.DeleteCommand.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        this.Adapter.DeleteCommand.Connection.Close();
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Insert, true)]
  public virtual int Insert(int Id, string MachineID)
  {
    this.Adapter.InsertCommand.Parameters[0].Value = (object) Id;
    if (MachineID == null)
      this.Adapter.InsertCommand.Parameters[1].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[1].Value = (object) MachineID;
    ConnectionState state = this.Adapter.InsertCommand.Connection.State;
    if ((this.Adapter.InsertCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      this.Adapter.InsertCommand.Connection.Open();
    try
    {
      return this.Adapter.InsertCommand.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        this.Adapter.InsertCommand.Connection.Close();
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Update, true)]
  public virtual int Update(int Id, string MachineID, int Original_Id, string Original_MachineID)
  {
    this.Adapter.UpdateCommand.Parameters[0].Value = (object) Id;
    if (MachineID == null)
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) MachineID;
    this.Adapter.UpdateCommand.Parameters[2].Value = (object) Original_Id;
    if (Original_MachineID == null)
    {
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) 1;
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) DBNull.Value;
    }
    else
    {
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) 0;
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) Original_MachineID;
    }
    ConnectionState state = this.Adapter.UpdateCommand.Connection.State;
    if ((this.Adapter.UpdateCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      this.Adapter.UpdateCommand.Connection.Open();
    try
    {
      return this.Adapter.UpdateCommand.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        this.Adapter.UpdateCommand.Connection.Close();
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Update, true)]
  public virtual int Update(string MachineID, int Original_Id, string Original_MachineID)
  {
    return this.Update(Original_Id, MachineID, Original_Id, Original_MachineID);
  }
}
