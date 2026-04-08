// Decompiled with JetBrains decompiler
// Type: EDGE.edgeahos_EdgeLicensingDataSet1
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#nullable disable
namespace EDGE;

[DesignerCategory("code")]
[ToolboxItem(true)]
[XmlSchemaProvider("GetTypedDataSetSchema")]
[XmlRoot("edgeahos_EdgeLicensingDataSet1")]
[HelpKeyword("vs.data.DataSet")]
[Serializable]
public class edgeahos_EdgeLicensingDataSet1 : DataSet
{
  private edgeahos_EdgeLicensingDataSet1.LicenseDataTable tableLicense;
  private edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable tableLicenseTable;
  private edgeahos_EdgeLicensingDataSet1.MachinesDataTable tableMachines;
  private SchemaSerializationMode _schemaSerializationMode = SchemaSerializationMode.IncludeSchema;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public edgeahos_EdgeLicensingDataSet1()
  {
    this.BeginInit();
    this.InitClass();
    CollectionChangeEventHandler changeEventHandler = new CollectionChangeEventHandler(this.SchemaChanged);
    base.Tables.CollectionChanged += changeEventHandler;
    base.Relations.CollectionChanged += changeEventHandler;
    this.EndInit();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected edgeahos_EdgeLicensingDataSet1(SerializationInfo info, StreamingContext context)
    : base(info, context, false)
  {
    if (this.IsBinarySerialized(info, context))
    {
      this.InitVars(false);
      CollectionChangeEventHandler changeEventHandler = new CollectionChangeEventHandler(this.SchemaChanged);
      this.Tables.CollectionChanged += changeEventHandler;
      this.Relations.CollectionChanged += changeEventHandler;
    }
    else
    {
      string s = (string) info.GetValue("XmlSchema", typeof (string));
      if (this.DetermineSchemaSerializationMode(info, context) == SchemaSerializationMode.IncludeSchema)
      {
        DataSet dataSet = new DataSet();
        dataSet.ReadXmlSchema((XmlReader) new XmlTextReader((TextReader) new StringReader(s)));
        if (dataSet.Tables[nameof (License)] != null)
          base.Tables.Add((DataTable) new edgeahos_EdgeLicensingDataSet1.LicenseDataTable(dataSet.Tables[nameof (License)]));
        if (dataSet.Tables[nameof (LicenseTable)] != null)
          base.Tables.Add((DataTable) new edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable(dataSet.Tables[nameof (LicenseTable)]));
        if (dataSet.Tables[nameof (Machines)] != null)
          base.Tables.Add((DataTable) new edgeahos_EdgeLicensingDataSet1.MachinesDataTable(dataSet.Tables[nameof (Machines)]));
        this.DataSetName = dataSet.DataSetName;
        this.Prefix = dataSet.Prefix;
        this.Namespace = dataSet.Namespace;
        this.Locale = dataSet.Locale;
        this.CaseSensitive = dataSet.CaseSensitive;
        this.EnforceConstraints = dataSet.EnforceConstraints;
        this.Merge(dataSet, false, MissingSchemaAction.Add);
        this.InitVars();
      }
      else
        this.ReadXmlSchema((XmlReader) new XmlTextReader((TextReader) new StringReader(s)));
      this.GetSerializationData(info, context);
      CollectionChangeEventHandler changeEventHandler = new CollectionChangeEventHandler(this.SchemaChanged);
      base.Tables.CollectionChanged += changeEventHandler;
      this.Relations.CollectionChanged += changeEventHandler;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public edgeahos_EdgeLicensingDataSet1.LicenseDataTable License => this.tableLicense;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable LicenseTable
  {
    get => this.tableLicenseTable;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Browsable(false)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  public edgeahos_EdgeLicensingDataSet1.MachinesDataTable Machines => this.tableMachines;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [Browsable(true)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
  public override SchemaSerializationMode SchemaSerializationMode
  {
    get => this._schemaSerializationMode;
    set => this._schemaSerializationMode = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public new DataTableCollection Tables => base.Tables;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public new DataRelationCollection Relations => base.Relations;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected override void InitializeDerivedDataSet()
  {
    this.BeginInit();
    this.InitClass();
    this.EndInit();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public override DataSet Clone()
  {
    edgeahos_EdgeLicensingDataSet1 licensingDataSet1 = (edgeahos_EdgeLicensingDataSet1) base.Clone();
    licensingDataSet1.InitVars();
    licensingDataSet1.SchemaSerializationMode = this.SchemaSerializationMode;
    return (DataSet) licensingDataSet1;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected override bool ShouldSerializeTables() => false;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected override bool ShouldSerializeRelations() => false;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected override void ReadXmlSerializable(XmlReader reader)
  {
    if (this.DetermineSchemaSerializationMode(reader) == SchemaSerializationMode.IncludeSchema)
    {
      this.Reset();
      DataSet dataSet = new DataSet();
      int num = (int) dataSet.ReadXml(reader);
      if (dataSet.Tables["License"] != null)
        base.Tables.Add((DataTable) new edgeahos_EdgeLicensingDataSet1.LicenseDataTable(dataSet.Tables["License"]));
      if (dataSet.Tables["LicenseTable"] != null)
        base.Tables.Add((DataTable) new edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable(dataSet.Tables["LicenseTable"]));
      if (dataSet.Tables["Machines"] != null)
        base.Tables.Add((DataTable) new edgeahos_EdgeLicensingDataSet1.MachinesDataTable(dataSet.Tables["Machines"]));
      this.DataSetName = dataSet.DataSetName;
      this.Prefix = dataSet.Prefix;
      this.Namespace = dataSet.Namespace;
      this.Locale = dataSet.Locale;
      this.CaseSensitive = dataSet.CaseSensitive;
      this.EnforceConstraints = dataSet.EnforceConstraints;
      this.Merge(dataSet, false, MissingSchemaAction.Add);
      this.InitVars();
    }
    else
    {
      int num = (int) this.ReadXml(reader);
      this.InitVars();
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  protected override XmlSchema GetSchemaSerializable()
  {
    MemoryStream memoryStream = new MemoryStream();
    this.WriteXmlSchema((XmlWriter) new XmlTextWriter((Stream) memoryStream, (Encoding) null));
    memoryStream.Position = 0L;
    return XmlSchema.Read((XmlReader) new XmlTextReader((Stream) memoryStream), (ValidationEventHandler) null);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  internal void InitVars() => this.InitVars(true);

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  internal void InitVars(bool initTable)
  {
    this.tableLicense = (edgeahos_EdgeLicensingDataSet1.LicenseDataTable) base.Tables["License"];
    if (initTable && this.tableLicense != null)
      this.tableLicense.InitVars();
    this.tableLicenseTable = (edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable) base.Tables["LicenseTable"];
    if (initTable && this.tableLicenseTable != null)
      this.tableLicenseTable.InitVars();
    this.tableMachines = (edgeahos_EdgeLicensingDataSet1.MachinesDataTable) base.Tables["Machines"];
    if (!initTable || this.tableMachines == null)
      return;
    this.tableMachines.InitVars();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private void InitClass()
  {
    this.DataSetName = nameof (edgeahos_EdgeLicensingDataSet1);
    this.Prefix = "";
    this.Namespace = "http://tempuri.org/edgeahos_EdgeLicensingDataSet1.xsd";
    this.EnforceConstraints = true;
    this.SchemaSerializationMode = SchemaSerializationMode.IncludeSchema;
    this.tableLicense = new edgeahos_EdgeLicensingDataSet1.LicenseDataTable();
    base.Tables.Add((DataTable) this.tableLicense);
    this.tableLicenseTable = new edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable();
    base.Tables.Add((DataTable) this.tableLicenseTable);
    this.tableMachines = new edgeahos_EdgeLicensingDataSet1.MachinesDataTable();
    base.Tables.Add((DataTable) this.tableMachines);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private bool ShouldSerializeLicense() => false;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private bool ShouldSerializeLicenseTable() => false;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private bool ShouldSerializeMachines() => false;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  private void SchemaChanged(object sender, CollectionChangeEventArgs e)
  {
    if (e.Action != CollectionChangeAction.Remove)
      return;
    this.InitVars();
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public static XmlSchemaComplexType GetTypedDataSetSchema(XmlSchemaSet xs)
  {
    edgeahos_EdgeLicensingDataSet1 licensingDataSet1 = new edgeahos_EdgeLicensingDataSet1();
    XmlSchemaComplexType typedDataSetSchema = new XmlSchemaComplexType();
    XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
    xmlSchemaSequence.Items.Add((XmlSchemaObject) new XmlSchemaAny()
    {
      Namespace = licensingDataSet1.Namespace
    });
    typedDataSetSchema.Particle = (XmlSchemaParticle) xmlSchemaSequence;
    XmlSchema schemaSerializable = licensingDataSet1.GetSchemaSerializable();
    if (xs.Contains(schemaSerializable.TargetNamespace))
    {
      MemoryStream memoryStream1 = new MemoryStream();
      MemoryStream memoryStream2 = new MemoryStream();
      try
      {
        schemaSerializable.Write((Stream) memoryStream1);
        IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
        while (enumerator.MoveNext())
        {
          XmlSchema current = (XmlSchema) enumerator.Current;
          memoryStream2.SetLength(0L);
          MemoryStream memoryStream3 = memoryStream2;
          current.Write((Stream) memoryStream3);
          if (memoryStream1.Length == memoryStream2.Length)
          {
            memoryStream1.Position = 0L;
            memoryStream2.Position = 0L;
            do
              ;
            while (memoryStream1.Position != memoryStream1.Length && memoryStream1.ReadByte() == memoryStream2.ReadByte());
            if (memoryStream1.Position == memoryStream1.Length)
              return typedDataSetSchema;
          }
        }
      }
      finally
      {
        memoryStream1?.Close();
        memoryStream2?.Close();
      }
    }
    xs.Add(schemaSerializable);
    return typedDataSetSchema;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public delegate void LicenseRowChangeEventHandler(
    object sender,
    edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEvent e);

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public delegate void LicenseTableRowChangeEventHandler(
    object sender,
    edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEvent e);

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public delegate void MachinesRowChangeEventHandler(
    object sender,
    edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEvent e);

  [XmlSchemaProvider("GetTypedTableSchema")]
  [Serializable]
  public class LicenseDataTable : TypedTableBase<edgeahos_EdgeLicensingDataSet1.LicenseRow>
  {
    private DataColumn columnMachineCode;
    private DataColumn columnUsername;
    private DataColumn columnCompanyName;
    private DataColumn columnProduct;
    private DataColumn columnLicenseKey;
    private DataColumn columnInProgress;
    private DataColumn columnLicenseStatus;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public LicenseDataTable()
    {
      this.TableName = "License";
      this.BeginInit();
      this.InitClass();
      this.EndInit();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal LicenseDataTable(DataTable table)
    {
      this.TableName = table.TableName;
      if (table.CaseSensitive != table.DataSet.CaseSensitive)
        this.CaseSensitive = table.CaseSensitive;
      if (table.Locale.ToString() != table.DataSet.Locale.ToString())
        this.Locale = table.Locale;
      if (table.Namespace != table.DataSet.Namespace)
        this.Namespace = table.Namespace;
      this.Prefix = table.Prefix;
      this.MinimumCapacity = table.MinimumCapacity;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected LicenseDataTable(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.InitVars();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn MachineCodeColumn => this.columnMachineCode;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn UsernameColumn => this.columnUsername;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn CompanyNameColumn => this.columnCompanyName;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn ProductColumn => this.columnProduct;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn LicenseKeyColumn => this.columnLicenseKey;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn InProgressColumn => this.columnInProgress;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn LicenseStatusColumn => this.columnLicenseStatus;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    [Browsable(false)]
    public int Count => this.Rows.Count;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseRow this[int index]
    {
      get => (edgeahos_EdgeLicensingDataSet1.LicenseRow) this.Rows[index];
    }

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEventHandler LicenseRowChanging;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEventHandler LicenseRowChanged;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEventHandler LicenseRowDeleting;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEventHandler LicenseRowDeleted;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void AddLicenseRow(edgeahos_EdgeLicensingDataSet1.LicenseRow row)
    {
      this.Rows.Add((DataRow) row);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseRow AddLicenseRow(
      string MachineCode,
      string Username,
      string CompanyName,
      string Product,
      string LicenseKey,
      bool InProgress,
      string LicenseStatus)
    {
      edgeahos_EdgeLicensingDataSet1.LicenseRow row = (edgeahos_EdgeLicensingDataSet1.LicenseRow) this.NewRow();
      object[] objArray = new object[7]
      {
        (object) MachineCode,
        (object) Username,
        (object) CompanyName,
        (object) Product,
        (object) LicenseKey,
        (object) InProgress,
        (object) LicenseStatus
      };
      row.ItemArray = objArray;
      this.Rows.Add((DataRow) row);
      return row;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseRow FindByMachineCode(string MachineCode)
    {
      return (edgeahos_EdgeLicensingDataSet1.LicenseRow) this.Rows.Find(new object[1]
      {
        (object) MachineCode
      });
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public override DataTable Clone()
    {
      edgeahos_EdgeLicensingDataSet1.LicenseDataTable licenseDataTable = (edgeahos_EdgeLicensingDataSet1.LicenseDataTable) base.Clone();
      licenseDataTable.InitVars();
      return (DataTable) licenseDataTable;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override DataTable CreateInstance()
    {
      return (DataTable) new edgeahos_EdgeLicensingDataSet1.LicenseDataTable();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal void InitVars()
    {
      this.columnMachineCode = this.Columns["MachineCode"];
      this.columnUsername = this.Columns["Username"];
      this.columnCompanyName = this.Columns["CompanyName"];
      this.columnProduct = this.Columns["Product"];
      this.columnLicenseKey = this.Columns["LicenseKey"];
      this.columnInProgress = this.Columns["InProgress"];
      this.columnLicenseStatus = this.Columns["LicenseStatus"];
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    private void InitClass()
    {
      this.columnMachineCode = new DataColumn("MachineCode", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnMachineCode);
      this.columnUsername = new DataColumn("Username", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnUsername);
      this.columnCompanyName = new DataColumn("CompanyName", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnCompanyName);
      this.columnProduct = new DataColumn("Product", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnProduct);
      this.columnLicenseKey = new DataColumn("LicenseKey", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnLicenseKey);
      this.columnInProgress = new DataColumn("InProgress", typeof (bool), (string) null, MappingType.Element);
      this.Columns.Add(this.columnInProgress);
      this.columnLicenseStatus = new DataColumn("LicenseStatus", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnLicenseStatus);
      this.Constraints.Add((Constraint) new UniqueConstraint("Constraint1", new DataColumn[1]
      {
        this.columnMachineCode
      }, true));
      this.columnMachineCode.AllowDBNull = false;
      this.columnMachineCode.Unique = true;
      this.columnMachineCode.MaxLength = 250;
      this.columnUsername.AllowDBNull = false;
      this.columnUsername.MaxLength = 250;
      this.columnCompanyName.MaxLength = 250;
      this.columnProduct.AllowDBNull = false;
      this.columnProduct.MaxLength = 250;
      this.columnLicenseKey.AllowDBNull = false;
      this.columnLicenseKey.MaxLength = int.MaxValue;
      this.columnInProgress.AllowDBNull = false;
      this.columnLicenseStatus.MaxLength = 50;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseRow NewLicenseRow()
    {
      return (edgeahos_EdgeLicensingDataSet1.LicenseRow) this.NewRow();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      return (DataRow) new edgeahos_EdgeLicensingDataSet1.LicenseRow(builder);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override Type GetRowType() => typeof (edgeahos_EdgeLicensingDataSet1.LicenseRow);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowChanged(DataRowChangeEventArgs e)
    {
      base.OnRowChanged(e);
      if (this.LicenseRowChanged == null)
        return;
      this.LicenseRowChanged((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowChanging(DataRowChangeEventArgs e)
    {
      base.OnRowChanging(e);
      if (this.LicenseRowChanging == null)
        return;
      this.LicenseRowChanging((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowDeleted(DataRowChangeEventArgs e)
    {
      base.OnRowDeleted(e);
      if (this.LicenseRowDeleted == null)
        return;
      this.LicenseRowDeleted((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowDeleting(DataRowChangeEventArgs e)
    {
      base.OnRowDeleting(e);
      if (this.LicenseRowDeleting == null)
        return;
      this.LicenseRowDeleting((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void RemoveLicenseRow(edgeahos_EdgeLicensingDataSet1.LicenseRow row)
    {
      this.Rows.Remove((DataRow) row);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
    {
      XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
      XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
      edgeahos_EdgeLicensingDataSet1 licensingDataSet1 = new edgeahos_EdgeLicensingDataSet1();
      XmlSchemaAny xmlSchemaAny1 = new XmlSchemaAny();
      xmlSchemaAny1.Namespace = "http://www.w3.org/2001/XMLSchema";
      xmlSchemaAny1.MinOccurs = 0M;
      xmlSchemaAny1.MaxOccurs = Decimal.MaxValue;
      xmlSchemaAny1.ProcessContents = XmlSchemaContentProcessing.Lax;
      xmlSchemaSequence.Items.Add((XmlSchemaObject) xmlSchemaAny1);
      XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
      xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
      xmlSchemaAny2.MinOccurs = 1M;
      xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
      xmlSchemaSequence.Items.Add((XmlSchemaObject) xmlSchemaAny2);
      typedTableSchema.Attributes.Add((XmlSchemaObject) new XmlSchemaAttribute()
      {
        Name = "namespace",
        FixedValue = licensingDataSet1.Namespace
      });
      typedTableSchema.Attributes.Add((XmlSchemaObject) new XmlSchemaAttribute()
      {
        Name = "tableTypeName",
        FixedValue = nameof (LicenseDataTable)
      });
      typedTableSchema.Particle = (XmlSchemaParticle) xmlSchemaSequence;
      XmlSchema schemaSerializable = licensingDataSet1.GetSchemaSerializable();
      if (xs.Contains(schemaSerializable.TargetNamespace))
      {
        MemoryStream memoryStream1 = new MemoryStream();
        MemoryStream memoryStream2 = new MemoryStream();
        try
        {
          schemaSerializable.Write((Stream) memoryStream1);
          IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
          while (enumerator.MoveNext())
          {
            XmlSchema current = (XmlSchema) enumerator.Current;
            memoryStream2.SetLength(0L);
            MemoryStream memoryStream3 = memoryStream2;
            current.Write((Stream) memoryStream3);
            if (memoryStream1.Length == memoryStream2.Length)
            {
              memoryStream1.Position = 0L;
              memoryStream2.Position = 0L;
              do
                ;
              while (memoryStream1.Position != memoryStream1.Length && memoryStream1.ReadByte() == memoryStream2.ReadByte());
              if (memoryStream1.Position == memoryStream1.Length)
                return typedTableSchema;
            }
          }
        }
        finally
        {
          memoryStream1?.Close();
          memoryStream2?.Close();
        }
      }
      xs.Add(schemaSerializable);
      return typedTableSchema;
    }
  }

  [XmlSchemaProvider("GetTypedTableSchema")]
  [Serializable]
  public class LicenseTableDataTable : TypedTableBase<edgeahos_EdgeLicensingDataSet1.LicenseTableRow>
  {
    private DataColumn columnMachineCode;
    private DataColumn columnUsername;
    private DataColumn columnCompanyName;
    private DataColumn columnProduct;
    private DataColumn columnLicenseKey;
    private DataColumn columnLicenseStatus;
    private DataColumn columnPhoneNum;
    private DataColumn columnEmail;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public LicenseTableDataTable()
    {
      this.TableName = "LicenseTable";
      this.BeginInit();
      this.InitClass();
      this.EndInit();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal LicenseTableDataTable(DataTable table)
    {
      this.TableName = table.TableName;
      if (table.CaseSensitive != table.DataSet.CaseSensitive)
        this.CaseSensitive = table.CaseSensitive;
      if (table.Locale.ToString() != table.DataSet.Locale.ToString())
        this.Locale = table.Locale;
      if (table.Namespace != table.DataSet.Namespace)
        this.Namespace = table.Namespace;
      this.Prefix = table.Prefix;
      this.MinimumCapacity = table.MinimumCapacity;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected LicenseTableDataTable(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.InitVars();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn MachineCodeColumn => this.columnMachineCode;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn UsernameColumn => this.columnUsername;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn CompanyNameColumn => this.columnCompanyName;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn ProductColumn => this.columnProduct;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn LicenseKeyColumn => this.columnLicenseKey;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn LicenseStatusColumn => this.columnLicenseStatus;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn PhoneNumColumn => this.columnPhoneNum;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn EmailColumn => this.columnEmail;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    [Browsable(false)]
    public int Count => this.Rows.Count;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseTableRow this[int index]
    {
      get => (edgeahos_EdgeLicensingDataSet1.LicenseTableRow) this.Rows[index];
    }

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEventHandler LicenseTableRowChanging;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEventHandler LicenseTableRowChanged;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEventHandler LicenseTableRowDeleting;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEventHandler LicenseTableRowDeleted;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void AddLicenseTableRow(edgeahos_EdgeLicensingDataSet1.LicenseTableRow row)
    {
      this.Rows.Add((DataRow) row);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseTableRow AddLicenseTableRow(
      string MachineCode,
      string Username,
      string CompanyName,
      string Product,
      string LicenseKey,
      string LicenseStatus,
      string PhoneNum,
      string Email)
    {
      edgeahos_EdgeLicensingDataSet1.LicenseTableRow row = (edgeahos_EdgeLicensingDataSet1.LicenseTableRow) this.NewRow();
      object[] objArray = new object[8]
      {
        (object) MachineCode,
        (object) Username,
        (object) CompanyName,
        (object) Product,
        (object) LicenseKey,
        (object) LicenseStatus,
        (object) PhoneNum,
        (object) Email
      };
      row.ItemArray = objArray;
      this.Rows.Add((DataRow) row);
      return row;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public override DataTable Clone()
    {
      edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable licenseTableDataTable = (edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable) base.Clone();
      licenseTableDataTable.InitVars();
      return (DataTable) licenseTableDataTable;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override DataTable CreateInstance()
    {
      return (DataTable) new edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal void InitVars()
    {
      this.columnMachineCode = this.Columns["MachineCode"];
      this.columnUsername = this.Columns["Username"];
      this.columnCompanyName = this.Columns["CompanyName"];
      this.columnProduct = this.Columns["Product"];
      this.columnLicenseKey = this.Columns["LicenseKey"];
      this.columnLicenseStatus = this.Columns["LicenseStatus"];
      this.columnPhoneNum = this.Columns["PhoneNum"];
      this.columnEmail = this.Columns["Email"];
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    private void InitClass()
    {
      this.columnMachineCode = new DataColumn("MachineCode", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnMachineCode);
      this.columnUsername = new DataColumn("Username", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnUsername);
      this.columnCompanyName = new DataColumn("CompanyName", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnCompanyName);
      this.columnProduct = new DataColumn("Product", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnProduct);
      this.columnLicenseKey = new DataColumn("LicenseKey", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnLicenseKey);
      this.columnLicenseStatus = new DataColumn("LicenseStatus", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnLicenseStatus);
      this.columnPhoneNum = new DataColumn("PhoneNum", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnPhoneNum);
      this.columnEmail = new DataColumn("Email", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnEmail);
      this.columnMachineCode.AllowDBNull = false;
      this.columnMachineCode.MaxLength = 50;
      this.columnUsername.AllowDBNull = false;
      this.columnUsername.MaxLength = 50;
      this.columnCompanyName.MaxLength = 50;
      this.columnProduct.AllowDBNull = false;
      this.columnProduct.MaxLength = 50;
      this.columnLicenseKey.AllowDBNull = false;
      this.columnLicenseKey.MaxLength = int.MaxValue;
      this.columnLicenseStatus.AllowDBNull = false;
      this.columnLicenseStatus.MaxLength = 50;
      this.columnPhoneNum.MaxLength = 50;
      this.columnEmail.MaxLength = 50;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseTableRow NewLicenseTableRow()
    {
      return (edgeahos_EdgeLicensingDataSet1.LicenseTableRow) this.NewRow();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      return (DataRow) new edgeahos_EdgeLicensingDataSet1.LicenseTableRow(builder);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override Type GetRowType() => typeof (edgeahos_EdgeLicensingDataSet1.LicenseTableRow);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowChanged(DataRowChangeEventArgs e)
    {
      base.OnRowChanged(e);
      if (this.LicenseTableRowChanged == null)
        return;
      this.LicenseTableRowChanged((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseTableRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowChanging(DataRowChangeEventArgs e)
    {
      base.OnRowChanging(e);
      if (this.LicenseTableRowChanging == null)
        return;
      this.LicenseTableRowChanging((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseTableRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowDeleted(DataRowChangeEventArgs e)
    {
      base.OnRowDeleted(e);
      if (this.LicenseTableRowDeleted == null)
        return;
      this.LicenseTableRowDeleted((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseTableRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowDeleting(DataRowChangeEventArgs e)
    {
      base.OnRowDeleting(e);
      if (this.LicenseTableRowDeleting == null)
        return;
      this.LicenseTableRowDeleting((object) this, new edgeahos_EdgeLicensingDataSet1.LicenseTableRowChangeEvent((edgeahos_EdgeLicensingDataSet1.LicenseTableRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void RemoveLicenseTableRow(edgeahos_EdgeLicensingDataSet1.LicenseTableRow row)
    {
      this.Rows.Remove((DataRow) row);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
    {
      XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
      XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
      edgeahos_EdgeLicensingDataSet1 licensingDataSet1 = new edgeahos_EdgeLicensingDataSet1();
      XmlSchemaAny xmlSchemaAny1 = new XmlSchemaAny();
      xmlSchemaAny1.Namespace = "http://www.w3.org/2001/XMLSchema";
      xmlSchemaAny1.MinOccurs = 0M;
      xmlSchemaAny1.MaxOccurs = Decimal.MaxValue;
      xmlSchemaAny1.ProcessContents = XmlSchemaContentProcessing.Lax;
      xmlSchemaSequence.Items.Add((XmlSchemaObject) xmlSchemaAny1);
      XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
      xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
      xmlSchemaAny2.MinOccurs = 1M;
      xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
      xmlSchemaSequence.Items.Add((XmlSchemaObject) xmlSchemaAny2);
      typedTableSchema.Attributes.Add((XmlSchemaObject) new XmlSchemaAttribute()
      {
        Name = "namespace",
        FixedValue = licensingDataSet1.Namespace
      });
      typedTableSchema.Attributes.Add((XmlSchemaObject) new XmlSchemaAttribute()
      {
        Name = "tableTypeName",
        FixedValue = nameof (LicenseTableDataTable)
      });
      typedTableSchema.Particle = (XmlSchemaParticle) xmlSchemaSequence;
      XmlSchema schemaSerializable = licensingDataSet1.GetSchemaSerializable();
      if (xs.Contains(schemaSerializable.TargetNamespace))
      {
        MemoryStream memoryStream1 = new MemoryStream();
        MemoryStream memoryStream2 = new MemoryStream();
        try
        {
          schemaSerializable.Write((Stream) memoryStream1);
          IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
          while (enumerator.MoveNext())
          {
            XmlSchema current = (XmlSchema) enumerator.Current;
            memoryStream2.SetLength(0L);
            MemoryStream memoryStream3 = memoryStream2;
            current.Write((Stream) memoryStream3);
            if (memoryStream1.Length == memoryStream2.Length)
            {
              memoryStream1.Position = 0L;
              memoryStream2.Position = 0L;
              do
                ;
              while (memoryStream1.Position != memoryStream1.Length && memoryStream1.ReadByte() == memoryStream2.ReadByte());
              if (memoryStream1.Position == memoryStream1.Length)
                return typedTableSchema;
            }
          }
        }
        finally
        {
          memoryStream1?.Close();
          memoryStream2?.Close();
        }
      }
      xs.Add(schemaSerializable);
      return typedTableSchema;
    }
  }

  [XmlSchemaProvider("GetTypedTableSchema")]
  [Serializable]
  public class MachinesDataTable : TypedTableBase<edgeahos_EdgeLicensingDataSet1.MachinesRow>
  {
    private DataColumn columnId;
    private DataColumn columnMachineID;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public MachinesDataTable()
    {
      this.TableName = "Machines";
      this.BeginInit();
      this.InitClass();
      this.EndInit();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal MachinesDataTable(DataTable table)
    {
      this.TableName = table.TableName;
      if (table.CaseSensitive != table.DataSet.CaseSensitive)
        this.CaseSensitive = table.CaseSensitive;
      if (table.Locale.ToString() != table.DataSet.Locale.ToString())
        this.Locale = table.Locale;
      if (table.Namespace != table.DataSet.Namespace)
        this.Namespace = table.Namespace;
      this.Prefix = table.Prefix;
      this.MinimumCapacity = table.MinimumCapacity;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected MachinesDataTable(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      this.InitVars();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn IdColumn => this.columnId;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataColumn MachineIDColumn => this.columnMachineID;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    [Browsable(false)]
    public int Count => this.Rows.Count;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.MachinesRow this[int index]
    {
      get => (edgeahos_EdgeLicensingDataSet1.MachinesRow) this.Rows[index];
    }

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEventHandler MachinesRowChanging;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEventHandler MachinesRowChanged;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEventHandler MachinesRowDeleting;

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public event edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEventHandler MachinesRowDeleted;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void AddMachinesRow(edgeahos_EdgeLicensingDataSet1.MachinesRow row)
    {
      this.Rows.Add((DataRow) row);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.MachinesRow AddMachinesRow(int Id, string MachineID)
    {
      edgeahos_EdgeLicensingDataSet1.MachinesRow row = (edgeahos_EdgeLicensingDataSet1.MachinesRow) this.NewRow();
      object[] objArray = new object[2]
      {
        (object) Id,
        (object) MachineID
      };
      row.ItemArray = objArray;
      this.Rows.Add((DataRow) row);
      return row;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.MachinesRow FindById(int Id)
    {
      return (edgeahos_EdgeLicensingDataSet1.MachinesRow) this.Rows.Find(new object[1]
      {
        (object) Id
      });
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public override DataTable Clone()
    {
      edgeahos_EdgeLicensingDataSet1.MachinesDataTable machinesDataTable = (edgeahos_EdgeLicensingDataSet1.MachinesDataTable) base.Clone();
      machinesDataTable.InitVars();
      return (DataTable) machinesDataTable;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override DataTable CreateInstance()
    {
      return (DataTable) new edgeahos_EdgeLicensingDataSet1.MachinesDataTable();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal void InitVars()
    {
      this.columnId = this.Columns["Id"];
      this.columnMachineID = this.Columns["MachineID"];
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    private void InitClass()
    {
      this.columnId = new DataColumn("Id", typeof (int), (string) null, MappingType.Element);
      this.Columns.Add(this.columnId);
      this.columnMachineID = new DataColumn("MachineID", typeof (string), (string) null, MappingType.Element);
      this.Columns.Add(this.columnMachineID);
      this.Constraints.Add((Constraint) new UniqueConstraint("Constraint1", new DataColumn[1]
      {
        this.columnId
      }, true));
      this.columnId.AllowDBNull = false;
      this.columnId.Unique = true;
      this.columnMachineID.MaxLength = 200;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.MachinesRow NewMachinesRow()
    {
      return (edgeahos_EdgeLicensingDataSet1.MachinesRow) this.NewRow();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      return (DataRow) new edgeahos_EdgeLicensingDataSet1.MachinesRow(builder);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override Type GetRowType() => typeof (edgeahos_EdgeLicensingDataSet1.MachinesRow);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowChanged(DataRowChangeEventArgs e)
    {
      base.OnRowChanged(e);
      if (this.MachinesRowChanged == null)
        return;
      this.MachinesRowChanged((object) this, new edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEvent((edgeahos_EdgeLicensingDataSet1.MachinesRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowChanging(DataRowChangeEventArgs e)
    {
      base.OnRowChanging(e);
      if (this.MachinesRowChanging == null)
        return;
      this.MachinesRowChanging((object) this, new edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEvent((edgeahos_EdgeLicensingDataSet1.MachinesRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowDeleted(DataRowChangeEventArgs e)
    {
      base.OnRowDeleted(e);
      if (this.MachinesRowDeleted == null)
        return;
      this.MachinesRowDeleted((object) this, new edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEvent((edgeahos_EdgeLicensingDataSet1.MachinesRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    protected override void OnRowDeleting(DataRowChangeEventArgs e)
    {
      base.OnRowDeleting(e);
      if (this.MachinesRowDeleting == null)
        return;
      this.MachinesRowDeleting((object) this, new edgeahos_EdgeLicensingDataSet1.MachinesRowChangeEvent((edgeahos_EdgeLicensingDataSet1.MachinesRow) e.Row, e.Action));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void RemoveMachinesRow(edgeahos_EdgeLicensingDataSet1.MachinesRow row)
    {
      this.Rows.Remove((DataRow) row);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public static XmlSchemaComplexType GetTypedTableSchema(XmlSchemaSet xs)
    {
      XmlSchemaComplexType typedTableSchema = new XmlSchemaComplexType();
      XmlSchemaSequence xmlSchemaSequence = new XmlSchemaSequence();
      edgeahos_EdgeLicensingDataSet1 licensingDataSet1 = new edgeahos_EdgeLicensingDataSet1();
      XmlSchemaAny xmlSchemaAny1 = new XmlSchemaAny();
      xmlSchemaAny1.Namespace = "http://www.w3.org/2001/XMLSchema";
      xmlSchemaAny1.MinOccurs = 0M;
      xmlSchemaAny1.MaxOccurs = Decimal.MaxValue;
      xmlSchemaAny1.ProcessContents = XmlSchemaContentProcessing.Lax;
      xmlSchemaSequence.Items.Add((XmlSchemaObject) xmlSchemaAny1);
      XmlSchemaAny xmlSchemaAny2 = new XmlSchemaAny();
      xmlSchemaAny2.Namespace = "urn:schemas-microsoft-com:xml-diffgram-v1";
      xmlSchemaAny2.MinOccurs = 1M;
      xmlSchemaAny2.ProcessContents = XmlSchemaContentProcessing.Lax;
      xmlSchemaSequence.Items.Add((XmlSchemaObject) xmlSchemaAny2);
      typedTableSchema.Attributes.Add((XmlSchemaObject) new XmlSchemaAttribute()
      {
        Name = "namespace",
        FixedValue = licensingDataSet1.Namespace
      });
      typedTableSchema.Attributes.Add((XmlSchemaObject) new XmlSchemaAttribute()
      {
        Name = "tableTypeName",
        FixedValue = nameof (MachinesDataTable)
      });
      typedTableSchema.Particle = (XmlSchemaParticle) xmlSchemaSequence;
      XmlSchema schemaSerializable = licensingDataSet1.GetSchemaSerializable();
      if (xs.Contains(schemaSerializable.TargetNamespace))
      {
        MemoryStream memoryStream1 = new MemoryStream();
        MemoryStream memoryStream2 = new MemoryStream();
        try
        {
          schemaSerializable.Write((Stream) memoryStream1);
          IEnumerator enumerator = xs.Schemas(schemaSerializable.TargetNamespace).GetEnumerator();
          while (enumerator.MoveNext())
          {
            XmlSchema current = (XmlSchema) enumerator.Current;
            memoryStream2.SetLength(0L);
            MemoryStream memoryStream3 = memoryStream2;
            current.Write((Stream) memoryStream3);
            if (memoryStream1.Length == memoryStream2.Length)
            {
              memoryStream1.Position = 0L;
              memoryStream2.Position = 0L;
              do
                ;
              while (memoryStream1.Position != memoryStream1.Length && memoryStream1.ReadByte() == memoryStream2.ReadByte());
              if (memoryStream1.Position == memoryStream1.Length)
                return typedTableSchema;
            }
          }
        }
        finally
        {
          memoryStream1?.Close();
          memoryStream2?.Close();
        }
      }
      xs.Add(schemaSerializable);
      return typedTableSchema;
    }
  }

  public class LicenseRow : DataRow
  {
    private edgeahos_EdgeLicensingDataSet1.LicenseDataTable tableLicense;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal LicenseRow(DataRowBuilder rb)
      : base(rb)
    {
      this.tableLicense = (edgeahos_EdgeLicensingDataSet1.LicenseDataTable) this.Table;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string MachineCode
    {
      get => (string) this[this.tableLicense.MachineCodeColumn];
      set => this[this.tableLicense.MachineCodeColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string Username
    {
      get => (string) this[this.tableLicense.UsernameColumn];
      set => this[this.tableLicense.UsernameColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string CompanyName
    {
      get
      {
        try
        {
          return (string) this[this.tableLicense.CompanyNameColumn];
        }
        catch (InvalidCastException ex)
        {
          throw new StrongTypingException("The value for column 'CompanyName' in table 'License' is DBNull.", (Exception) ex);
        }
      }
      set => this[this.tableLicense.CompanyNameColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string Product
    {
      get => (string) this[this.tableLicense.ProductColumn];
      set => this[this.tableLicense.ProductColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string LicenseKey
    {
      get => (string) this[this.tableLicense.LicenseKeyColumn];
      set => this[this.tableLicense.LicenseKeyColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool InProgress
    {
      get => (bool) this[this.tableLicense.InProgressColumn];
      set => this[this.tableLicense.InProgressColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string LicenseStatus
    {
      get
      {
        try
        {
          return (string) this[this.tableLicense.LicenseStatusColumn];
        }
        catch (InvalidCastException ex)
        {
          throw new StrongTypingException("The value for column 'LicenseStatus' in table 'License' is DBNull.", (Exception) ex);
        }
      }
      set => this[this.tableLicense.LicenseStatusColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool IsCompanyNameNull() => this.IsNull(this.tableLicense.CompanyNameColumn);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void SetCompanyNameNull() => this[this.tableLicense.CompanyNameColumn] = Convert.DBNull;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool IsLicenseStatusNull() => this.IsNull(this.tableLicense.LicenseStatusColumn);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void SetLicenseStatusNull()
    {
      this[this.tableLicense.LicenseStatusColumn] = Convert.DBNull;
    }
  }

  public class LicenseTableRow : DataRow
  {
    private edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable tableLicenseTable;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal LicenseTableRow(DataRowBuilder rb)
      : base(rb)
    {
      this.tableLicenseTable = (edgeahos_EdgeLicensingDataSet1.LicenseTableDataTable) this.Table;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string MachineCode
    {
      get => (string) this[this.tableLicenseTable.MachineCodeColumn];
      set => this[this.tableLicenseTable.MachineCodeColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string Username
    {
      get => (string) this[this.tableLicenseTable.UsernameColumn];
      set => this[this.tableLicenseTable.UsernameColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string CompanyName
    {
      get
      {
        try
        {
          return (string) this[this.tableLicenseTable.CompanyNameColumn];
        }
        catch (InvalidCastException ex)
        {
          throw new StrongTypingException("The value for column 'CompanyName' in table 'LicenseTable' is DBNull.", (Exception) ex);
        }
      }
      set => this[this.tableLicenseTable.CompanyNameColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string Product
    {
      get => (string) this[this.tableLicenseTable.ProductColumn];
      set => this[this.tableLicenseTable.ProductColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string LicenseKey
    {
      get => (string) this[this.tableLicenseTable.LicenseKeyColumn];
      set => this[this.tableLicenseTable.LicenseKeyColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string LicenseStatus
    {
      get => (string) this[this.tableLicenseTable.LicenseStatusColumn];
      set => this[this.tableLicenseTable.LicenseStatusColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string PhoneNum
    {
      get
      {
        try
        {
          return (string) this[this.tableLicenseTable.PhoneNumColumn];
        }
        catch (InvalidCastException ex)
        {
          throw new StrongTypingException("The value for column 'PhoneNum' in table 'LicenseTable' is DBNull.", (Exception) ex);
        }
      }
      set => this[this.tableLicenseTable.PhoneNumColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string Email
    {
      get
      {
        try
        {
          return (string) this[this.tableLicenseTable.EmailColumn];
        }
        catch (InvalidCastException ex)
        {
          throw new StrongTypingException("The value for column 'Email' in table 'LicenseTable' is DBNull.", (Exception) ex);
        }
      }
      set => this[this.tableLicenseTable.EmailColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool IsCompanyNameNull() => this.IsNull(this.tableLicenseTable.CompanyNameColumn);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void SetCompanyNameNull()
    {
      this[this.tableLicenseTable.CompanyNameColumn] = Convert.DBNull;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool IsPhoneNumNull() => this.IsNull(this.tableLicenseTable.PhoneNumColumn);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void SetPhoneNumNull() => this[this.tableLicenseTable.PhoneNumColumn] = Convert.DBNull;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool IsEmailNull() => this.IsNull(this.tableLicenseTable.EmailColumn);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void SetEmailNull() => this[this.tableLicenseTable.EmailColumn] = Convert.DBNull;
  }

  public class MachinesRow : DataRow
  {
    private edgeahos_EdgeLicensingDataSet1.MachinesDataTable tableMachines;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    internal MachinesRow(DataRowBuilder rb)
      : base(rb)
    {
      this.tableMachines = (edgeahos_EdgeLicensingDataSet1.MachinesDataTable) this.Table;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public int Id
    {
      get => (int) this[this.tableMachines.IdColumn];
      set => this[this.tableMachines.IdColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public string MachineID
    {
      get
      {
        try
        {
          return (string) this[this.tableMachines.MachineIDColumn];
        }
        catch (InvalidCastException ex)
        {
          throw new StrongTypingException("The value for column 'MachineID' in table 'Machines' is DBNull.", (Exception) ex);
        }
      }
      set => this[this.tableMachines.MachineIDColumn] = (object) value;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public bool IsMachineIDNull() => this.IsNull(this.tableMachines.MachineIDColumn);

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public void SetMachineIDNull() => this[this.tableMachines.MachineIDColumn] = Convert.DBNull;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public class LicenseRowChangeEvent : EventArgs
  {
    private edgeahos_EdgeLicensingDataSet1.LicenseRow eventRow;
    private DataRowAction eventAction;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public LicenseRowChangeEvent(
      edgeahos_EdgeLicensingDataSet1.LicenseRow row,
      DataRowAction action)
    {
      this.eventRow = row;
      this.eventAction = action;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseRow Row => this.eventRow;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataRowAction Action => this.eventAction;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public class LicenseTableRowChangeEvent : EventArgs
  {
    private edgeahos_EdgeLicensingDataSet1.LicenseTableRow eventRow;
    private DataRowAction eventAction;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public LicenseTableRowChangeEvent(
      edgeahos_EdgeLicensingDataSet1.LicenseTableRow row,
      DataRowAction action)
    {
      this.eventRow = row;
      this.eventAction = action;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.LicenseTableRow Row => this.eventRow;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataRowAction Action => this.eventAction;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
  public class MachinesRowChangeEvent : EventArgs
  {
    private edgeahos_EdgeLicensingDataSet1.MachinesRow eventRow;
    private DataRowAction eventAction;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public MachinesRowChangeEvent(
      edgeahos_EdgeLicensingDataSet1.MachinesRow row,
      DataRowAction action)
    {
      this.eventRow = row;
      this.eventAction = action;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public edgeahos_EdgeLicensingDataSet1.MachinesRow Row => this.eventRow;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "17.0.0.0")]
    public DataRowAction Action => this.eventAction;
  }
}
