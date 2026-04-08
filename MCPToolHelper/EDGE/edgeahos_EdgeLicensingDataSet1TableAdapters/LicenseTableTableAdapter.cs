// Decompiled with JetBrains decompiler
// Type: EDGE.edgeahos_EdgeLicensingDataSet1TableAdapters.LicenseTableTableAdapter
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
public class LicenseTableTableAdapter : Component
{
  private SqlDataAdapter _adapter;
  private SqlConnection _connection;
  private SqlTransaction _transaction;
  private SqlCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public LicenseTableTableAdapter() => this.ClearBeforeFill = true;

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
      DataSetTable = "LicenseTable",
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
          "LicenseStatus",
          "LicenseStatus"
        },
        {
          "PhoneNum",
          "PhoneNum"
        },
        {
          "Email",
          "Email"
        }
      }
    });
    this._adapter.InsertCommand = new SqlCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO [edgeahos_ptacdevelopment].[LicenseTable] ([MachineCode], [Username], [CompanyName], [Product], [LicenseKey], [LicenseStatus], [PhoneNum], [Email]) VALUES (@MachineCode, @Username, @CompanyName, @Product, @LicenseKey, @LicenseStatus, @PhoneNum, @Email)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@MachineCode", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineCode", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@Username", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Username", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@CompanyName", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@Product", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Product", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@LicenseKey", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseKey", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@LicenseStatus", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "LicenseStatus", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@PhoneNum", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PhoneNum", DataRowVersion.Current, false, (object) null, "", "", ""));
    this._adapter.InsertCommand.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Email", DataRowVersion.Current, false, (object) null, "", "", ""));
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
    this._commandCollection[0].CommandText = "SELECT MachineCode, Username, CompanyName, Product, LicenseKey, LicenseStatus, PhoneNum, Email FROM edgeahos_ptacdevelopment.LicenseTable";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  public virtual int Fill(
    edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable dataTable)
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
  public virtual edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable data = new edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(
    edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(edgeahos_EdgeLicensingDataSet1 dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "LicenseTable");
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
  [DataObjectMethod(DataObjectMethodType.Insert, true)]
  public virtual int Insert(
    string MachineCode,
    string Username,
    string CompanyName,
    string Product,
    string LicenseKey,
    string LicenseStatus,
    string PhoneNum,
    string Email)
  {
    this.Adapter.InsertCommand.Parameters[0].Value = MachineCode != null ? (object) MachineCode : throw new ArgumentNullException(nameof (MachineCode));
    this.Adapter.InsertCommand.Parameters[1].Value = Username != null ? (object) Username : throw new ArgumentNullException(nameof (Username));
    if (CompanyName == null)
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[2].Value = (object) CompanyName;
    this.Adapter.InsertCommand.Parameters[3].Value = Product != null ? (object) Product : throw new ArgumentNullException(nameof (Product));
    this.Adapter.InsertCommand.Parameters[4].Value = LicenseKey != null ? (object) LicenseKey : throw new ArgumentNullException(nameof (LicenseKey));
    this.Adapter.InsertCommand.Parameters[5].Value = LicenseStatus != null ? (object) LicenseStatus : throw new ArgumentNullException(nameof (LicenseStatus));
    if (PhoneNum == null)
      this.Adapter.InsertCommand.Parameters[6].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[6].Value = (object) PhoneNum;
    if (Email == null)
      this.Adapter.InsertCommand.Parameters[7].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[7].Value = (object) Email;
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
}
