// Decompiled with JetBrains decompiler
// Type: EDGE.QATools.GeometryViewerForm
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace EDGE.QATools;

public class GeometryViewerForm : System.Windows.Forms.Form
{
  private Element familyElement;
  private Document revitDoc;
  private Transform transToApply;
  private IContainer components;
  private TreeView treeView1;

  public GeometryViewerForm() => this.InitializeComponent();

  public GeometryViewerForm(Element elem)
  {
    this.InitializeComponent();
    this.familyElement = elem;
    this.revitDoc = elem.Document;
  }

  public GeometryViewerForm(Element elem, Transform trans)
  {
    this.InitializeComponent();
    this.familyElement = elem;
    this.revitDoc = elem.Document;
    this.transToApply = trans;
  }

  private void GeometryViewerForm_Load(object sender, EventArgs e)
  {
    try
    {
      this.PopulateTree();
    }
    catch (Exception ex)
    {
    }
  }

  private void PopulateTree()
  {
    if (this.familyElement == null)
      return;
    this.FillNode(this.treeView1.Nodes.Add("Element"), this.familyElement.get_Geometry(new Options()
    {
      ComputeReferences = true,
      View = this.revitDoc.ActiveView
    }));
  }

  private void FillNode(TreeNode branch, GeometryElement geomElement)
  {
    foreach (GeometryObject geometryObject in geomElement)
    {
      switch (geometryObject)
      {
        case GeometryInstance _:
          TreeNode treeNode1 = branch.Nodes.Add("GeometryInstance");
          this.FillTransformInfo(treeNode1.Nodes.Add("Transform:"), (geometryObject as GeometryInstance).Transform);
          this.FillNode(treeNode1.Nodes.Add("Symbol"), (geometryObject as GeometryInstance).GetSymbolGeometry());
          this.FillNode(treeNode1.Nodes.Add("Instance"), (geometryObject as GeometryInstance).GetInstanceGeometry());
          continue;
        case Solid _:
          Solid solid = geometryObject as Solid;
          TreeNode treeNode2 = branch.Nodes.Add("Solid");
          TreeNodeCollection nodes1 = treeNode2.Nodes;
          double num = solid.Volume;
          string text1 = "Volume: " + num.ToString();
          nodes1.Add(text1);
          TreeNodeCollection nodes2 = treeNode2.Nodes;
          num = solid.SurfaceArea;
          string text2 = "Surface Area: " + num.ToString();
          nodes2.Add(text2);
          TreeNode branch1 = treeNode2.Nodes.Add("Faces:");
          TreeNode branch2 = treeNode2.Nodes.Add("Edges:");
          foreach (Face face in solid.Faces)
            this.FillFaceInfo(branch1, face);
          IEnumerator enumerator = solid.Edges.GetEnumerator();
          try
          {
            while (enumerator.MoveNext())
            {
              Edge current = (Edge) enumerator.Current;
              this.FillLineInfo(branch2, current);
            }
            continue;
          }
          finally
          {
            if (enumerator is IDisposable disposable)
              disposable.Dispose();
          }
        case Mesh _:
        case Curve _:
        case Autodesk.Revit.DB.Point _:
          continue;
        default:
          PolyLine polyLine = geometryObject as PolyLine;
          continue;
      }
    }
  }

  private void FillTransformInfo(TreeNode transformNode, Transform transform)
  {
  }

  private void FillLineInfo(TreeNode branch, Curve curve)
  {
    branch.Nodes.Add("Appr. Length: " + curve.ApproximateLength.ToString());
    branch.Nodes.Add("   End(0): " + curve.GetEndPoint(0).ToString());
    branch.Nodes.Add("   End(1): " + curve.GetEndPoint(1).ToString());
    if (this.transToApply == null)
      return;
    branch.Nodes.Add("   Transformed End(0): " + this.transToApply.OfPoint(curve.GetEndPoint(0)).ToString());
    branch.Nodes.Add("   Transformed End(1): " + this.transToApply.OfPoint(curve.GetEndPoint(1)).ToString());
  }

  private void FillLineInfo(TreeNode branch, Edge edge)
  {
    this.FillLineInfo(branch, edge.AsCurve());
  }

  private void FillFaceInfo(TreeNode branch, Face face)
  {
    branch.Nodes.Add("Area: " + face.Area.ToString());
    branch.Nodes.Add("   Face Normal (UV0,0): " + face.ComputeNormal(new UV(0.0, 0.0)).ToString());
    if (this.transToApply != null)
      branch.Nodes.Add("   Transformed Face Normal (UV0,0): " + this.transToApply.OfVector(face.ComputeNormal(new UV(0.0, 0.0))).ToString());
    TreeNode treeNode1 = branch.Nodes.Add("   Loops:");
    int num1 = 0;
    foreach (CurveLoop edgesAsCurveLoop in (IEnumerable<CurveLoop>) face.GetEdgesAsCurveLoops())
    {
      TreeNode treeNode2 = treeNode1.Nodes.Add($"<{num1.ToString()}>");
      int num2 = 0;
      foreach (Curve curve in edgesAsCurveLoop)
      {
        this.FillLineInfo(treeNode2.Nodes.Add($"<{num2.ToString()}>"), curve);
        ++num2;
      }
      ++num1;
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.treeView1 = new TreeView();
    this.SuspendLayout();
    this.treeView1.Location = new System.Drawing.Point(12, 12);
    this.treeView1.Name = "treeView1";
    this.treeView1.Size = new Size(711, 764);
    this.treeView1.TabIndex = 0;
    this.AutoScaleDimensions = new SizeF(9f, 20f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(1303, 883);
    this.Controls.Add((System.Windows.Forms.Control) this.treeView1);
    this.Name = nameof (GeometryViewerForm);
    this.Text = nameof (GeometryViewerForm);
    this.Load += new EventHandler(this.GeometryViewerForm_Load);
    this.ResumeLayout(false);
  }
}
