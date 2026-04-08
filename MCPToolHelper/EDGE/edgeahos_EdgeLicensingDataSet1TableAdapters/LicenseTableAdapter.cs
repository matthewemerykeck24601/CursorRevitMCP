// Decompiled with JetBrains decompiler
// Type: EDGE.edgeahos_EdgeLicensingDataSet1TableAdapters.LicenseTableAdapter
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
public class LicenseTableAdapter : Component
{
  private SqlDataAdapter _adapter;
  private SqlConnection _connection;
  private SqlTransaction _transaction;
  private SqlCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public LicenseTableAdapter() => this.ClearBeforeFill = true;

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
      DataSetTable = "License",
      ColumnMappings = {
        {
          "MachineCode",
          "MachineCode"
        },
        {
          "Username",
          "Username"
        },
        {
          "CompanyName",
          "CompanyName"
        },
        {
          "Product",
          "Product"
        },
        {
          "LicenseKey",
          "LicenseKey"
        },
        {
          "InProgress",
          "InProgress"
        },
        {
          "LicenseStatus",
          "LicenseStatus"
        }
      }
    });
    this._adapter.DeleteCommand = new SqlCommand();
    this._adapter.DeleteCommand.Connection = this.Connection;
    this._adapter.DeleteCommand.CommandText = "DELETE FROM [edgeahos_ptacdevelopment].[License] WHERE (([MachineCode] = @Original_MachineCode) AND ([Username] = @Original_Username) AND ((@IsNull_CompanyName = 1 AND [CompanyName] IS NULL) OR ([CompanyName] = @Original_CompanyName)) AND ([Product] = @Original_Product) AND ([InProgress] = @Original_InProgress) AND ((@IsNull_LicenseStatus = 1 AND [LicenseStatus] IS NULL) OR ([LicenseStatus] = @Original_LicenseStatus)))";
    this._adapter.DeleteCommand.CommandType = CommandType.Text;
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_MachineCode", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineCode", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_Username", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Username", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@IsNull_CompanyName", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Original, true, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_CompanyName", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_Product", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Product", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_InProgress", SqlDbType.Bit, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "InProgress", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@IsNull_LicenseStatus", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Original, true, (object) null, "", "", ""));
    this._adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Original_LicenseStatus", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.InsertCommand = new SqlCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO [edgeahos_ptacdevelopment].[License] ([MachineCode], [Username], [CompanyName], [Product], [LicenseKey], [InProgress], [LicenseStatus]) VALUES (@MachineCode, @Username, @CompanyName, @Product, @LicenseKey, @InProgress, @LicenseStatus);\r\nSELECT MachineCode, Username, CompanyName, Product, LicenseKey, InProgress, LicenseStatus FROM License WHERE (MachineCode = @MachineCode)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@MachineCode", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineCode", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@Username", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Username", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@CompanyName", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@Product", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Product", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@LicenseKey", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseKey", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@InProgress", SqlDbType.Bit, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "InProgress", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@LicenseStatus", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand = new SqlCommand();
    this._adapter.UpdateCommand.Connection = this.Connection;
    this._adapter.UpdateCommand.CommandText = "UPDATE [edgeahos_ptacdevelopment].[License] SET [MachineCode] = @MachineCode, [Username] = @Username, [CompanyName] = @CompanyName, [Product] = @Product, [LicenseKey] = @LicenseKey, [InProgress] = @InProgress, [LicenseStatus] = @LicenseStatus WHERE (([MachineCode] = @Original_MachineCode) AND ([Username] = @Original_Username) AND ((@IsNull_CompanyName = 1 AND [CompanyName] IS NULL) OR ([CompanyName] = @Original_CompanyName)) AND ([Product] = @Original_Product) AND ([InProgress] = @Original_InProgress) AND ((@IsNull_LicenseStatus = 1 AND [LicenseStatus] IS NULL) OR ([LicenseStatus] = @Original_LicenseStatus)));\r\nSELECT MachineCode, Username, CompanyName, Product, LicenseKey, InProgress, LicenseStatus FROM License WHERE (MachineCode = @MachineCode)";
    this._adapter.UpdateCommand.CommandType = CommandType.Text;
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@MachineCode", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineCode", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Username", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Username", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@CompanyName", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Product", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Product", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@LicenseKey", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseKey", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@InProgress", SqlDbType.Bit, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "InProgress", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@LicenseStatus", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_MachineCode", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineCode", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_Username", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Username", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@IsNull_CompanyName", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Original, true, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_CompanyName", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_Product", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Product", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_InProgress", SqlDbType.Bit, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "InProgress", DataRowVersion.Original, false, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@IsNull_LicenseStatus", SqlDbType.Int, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Original, true, (object) null, "", "", ""));
    this._adapter.UpdateCommand.Parameters.Add(new SqlParameter("@Original_LicenseStatus", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Original, false, (object) null, "", "", ""));
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
    this._commandCollection[0].CommandText = "SELECT MachineCode, Username, CompanyName, Product, LicenseKey, InProgress, LicenseStatus FROM edgeahos_ptacdevelopment.License";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  public virtual int Fill(
    edgeahos_EdgeLicensingDataSet1.LicenseDataTable dataTable)
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
  public virtual edgeahos_EdgeLicensingDataSet1.LicenseDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    edgeahos_EdgeLicensingDataSet1.LicenseDataTable data = new edgeahos_EdgeLicensingDataSet1.LicenseDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(
    edgeahos_EdgeLicensingDataSet1.LicenseDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(edgeahos_EdgeLicensingDataSet1 dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "License");
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
  public virtual int Delete(
    string Original_MachineCode,
    string Original_Username,
    string Original_CompanyName,
    string Original_Product,
    bool Original_InProgress,
    string Original_LicenseStatus)
  {
    this.Adapter.DeleteCommand.Parameters[0].Value = Original_MachineCode != null ? (object) Original_MachineCode : throw new ArgumentNullException(nameof (Original_MachineCode));
    this.Adapter.DeleteCommand.Parameters[1].Value = Original_Username != null ? (object) Original_Username : throw new ArgumentNullException(nameof (Original_Username));
    if (Original_CompanyName == null)
    {
      this.Adapter.DeleteCommand.Parameters[2].Value = (object) 1;
      this.Adapter.DeleteCommand.Parameters[3].Value = (object) DBNull.Value;
    }
    else
    {
      this.Adapter.DeleteCommand.Parameters[2].Value = (object) 0;
      this.Adapter.DeleteCommand.Parameters[3].Value = (object) Original_CompanyName;
    }
    this.Adapter.DeleteCommand.Parameters[4].Value = Original_Product != null ? (object) Original_Product : throw new ArgumentNullException(nameof (Original_Product));
    this.Adapter.DeleteCommand.Parameters[5].Value = (object) Original_InProgress;
    if (Original_LicenseStatus == null)
    {
      this.Adapter.DeleteCommand.Parameters[6].Value = (object) 1;
      this.Adapter.DeleteCommand.Parameters[7].Value = (object) DBNull.Value;
    }
    else
    {
      this.Adapter.DeleteCommand.Parameters[6].Value = (object) 0;
      this.Adapter.DeleteCommand.Parameters[7].Value = (object) Original_LicenseStatus;
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
  public virtual int Insert(
    string MachineCode,
    string Username,
    string CompanyName,
    string Product,
    string LicenseKey,
    bool InProgress,
    string LicenseStatus)
  {
    this.Adapter.InsertCommand.Parameters[0].Value = MachineCode != null ? (object) MachineCode : throw new ArgumentNullException(nameof (MachineCode));
    this.Adapter.InsertCommand.Parameters[1].Value = Username != null ? (object) Username : throw new ArgumentNullException(nameof (Username));
    if (CompanyName == null)
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[2].Value = (object) CompanyName;
    this.Adapter.InsertCommand.Parameters[3].Value = Product != null ? (object) Product : throw new ArgumentNullException(nameof (Product));
    this.Adapter.InsertCommand.Parameters[4].Value = LicenseKey != null ? (object) LicenseKey : throw new ArgumentNullException(nameof (LicenseKey));
    this.Adapter.InsertCommand.Parameters[5].Value = (object) InProgress;
    if (LicenseStatus == null)
      this.Adapter.InsertCommand.Parameters[6].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[6].Value = (object) LicenseStatus;
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
  public virtual int Update(
    string MachineCode,
    string Username,
    string CompanyName,
    string Product,
    string LicenseKey,
    bool InProgress,
    string LicenseStatus,
    string Original_MachineCode,
    string Original_Username,
    string Original_CompanyName,
    string Original_Product,
    bool Original_InProgress,
    string Original_LicenseStatus)
  {
    this.Adapter.UpdateCommand.Parameters[0].Value = MachineCode != null ? (object) MachineCode : throw new ArgumentNullException(nameof (MachineCode));
    this.Adapter.UpdateCommand.Parameters[1].Value = Username != null ? (object) Username : throw new ArgumentNullException(nameof (Username));
    if (CompanyName == null)
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) CompanyName;
    this.Adapter.UpdateCommand.Parameters[3].Value = Product != null ? (object) Product : throw new ArgumentNullException(nameof (Product));
    this.Adapter.UpdateCommand.Parameters[4].Value = LicenseKey != null ? (object) LicenseKey : throw new ArgumentNullException(nameof (LicenseKey));
    this.Adapter.UpdateCommand.Parameters[5].Value = (object) InProgress;
    if (LicenseStatus == null)
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) LicenseStatus;
    this.Adapter.UpdateCommand.Parameters[7].Value = Original_MachineCode != null ? (object) Original_MachineCode : throw new ArgumentNullException(nameof (Original_MachineCode));
    this.Adapter.UpdateCommand.Parameters[8].Value = Original_Username != null ? (object) Original_Username : throw new ArgumentNullException(nameof (Original_Username));
    if (Original_CompanyName == null)
    {
      this.Adapter.UpdateCommand.Parameters[9].Value = (object) 1;
      this.Adapter.UpdateCommand.Parameters[10].Value = (object) DBNull.Value;
    }
    else
    {
      this.Adapter.UpdateCommand.Parameters[9].Value = (object) 0;
      this.Adapter.UpdateCommand.Parameters[10].Value = (object) Original_CompanyName;
    }
    this.Adapter.UpdateCommand.Parameters[11].Value = Original_Product != null ? (object) Original_Product : throw new ArgumentNullException(nameof (Original_Product));
    this.Adapter.UpdateCommand.Parameters[12].Value = (object) Original_InProgress;
    if (Original_LicenseStatus == null)
    {
      this.Adapter.UpdateCommand.Parameters[13].Value = (object) 1;
      this.Adapter.UpdateCommand.Parameters[14].Value = (object) DBNull.Value;
    }
    else
    {
      this.Adapter.UpdateCommand.Parameters[13].Value = (object) 0;
      this.Adapter.UpdateCommand.Parameters[14].Value = (object) Original_LicenseStatus;
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
  public virtual int Update(
    string Username,
    string CompanyName,
    string Product,
    string LicenseKey,
    bool InProgress,
    string LicenseStatus,
    string Original_MachineCode,
    string Original_Username,
    string Original_CompanyName,
    string Original_Product,
    bool Original_InProgress,
    string Original_LicenseStatus)
  {
    return this.Update(Original_MachineCode, Username, CompanyName, Product, LicenseKey, InProgress, LicenseStatus, Original_MachineCode, Original_Username, Original_CompanyName, Original_Product, Original_InProgress, Original_LicenseStatus);
  }
}
