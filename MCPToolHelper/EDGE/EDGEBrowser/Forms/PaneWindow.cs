// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.Forms.PaneWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using Autodesk.Revit.UI;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Xps.Packaging;

#nullable disable
namespace EDGE.EDGEBrowser.Forms;

public class PaneWindow : Page, IComponentConnector
{
  public string year = "2024";
  private Guid m_targetGuid;
  private DockPosition m_position = (DockPosition) 59422;
  private int m_left = 1;
  private int m_right = 1;
  private int m_top = 1;
  private int m_bottom = 1;
  private string blobConnectionString = Environment.GetEnvironmentVariable("EDGE_BLOB_CONNECTION_STRING") ?? string.Empty;
  private string trainManualPath = "EDGE^Revit Content/Training/Manuals/EDGE_Training_Manual.xps";
  private string prevFilePath = "";
  private string currentFilePath = "";
  private string prevTrainingFilePath = "";
  private string currentTrainingFilePath = "";
  private string masterFamilyDirectory = "";
  private string masterTrainingDirectory = "";
  private string masterTrainingVideosDirectory = "";
  public string fileName = "";
  public string downloadingFileName = "";
  public ExternalEvent famLoadEvent;
  public ExternalEvent famDownloadEvent;
  private XpsDocument xpsDocument;
  public WindowState state = WindowState.Main;
  private List<string> infolist;
  private bool canOpenNewVideos = true;
  private bool canDownload = true;
  private bool searchresultstatus;
  public List<Tuple<string, string>> trainingFilePaths = new List<Tuple<string, string>>();
  public List<Tuple<string, string>> documentFriendlyNames = new List<Tuple<string, string>>();
  private List<Tuple<string, string>> videoFriendlyNames = new List<Tuple<string, string>>();
  private List<Tuple<string, string>> familyFriendlyNames = new List<Tuple<string, string>>();
  private List<Tuple<string, string>> familyFilePaths = new List<Tuple<string, string>>();
  private List<Tuple<string, string>> xlsxFamilyPaths = new List<Tuple<string, string>>();
  private List<Tuple<string, string>> txtFamilyPaths = new List<Tuple<string, string>>();
  private TreeViewItem selectedTVI;
  private List<string> videoQualityPaths = new List<string>();
  private DispatcherTimer timer;
  private bool cancelDownload;
  private bool somethingIsDownloading;
  private List<TreeViewItem> listwhensomethingisdownloading = new List<TreeViewItem>();
  private double treeViewHorizScrollPos;
  private double treeViewVertScrollPos;
  private bool treeViewResetHorizScrollViwer;
  private bool treeViewResetVerScrollViwer;
  private ScrollViewer treeViewScrollViewer;
  private System.Timers.Timer aTimer;
  private WebClient currentDownloadClient;
  private bool internetLoss;
  private bool contentdisplayed;
  public bool LoadedTreeViews;
  private Queue<string> _LoadUrls = new Queue<string>();
  private string LoadingFileName;
  private string loadFilename;
  private string txtloadfilename;
  private string uriloadfilename;
  private string uritxtloadfilename;
  private Queue<string> _downloadUrls = new Queue<string>();
  private bool isDragging;
  internal PaneWindow MainPaneWindow;
  internal Grid TopGrid;
  internal Grid ContentGrid;
  internal Button MainIcon;
  internal TextBox FindBar;
  internal TextBlock dlTextBlock;
  internal Button CancelDownload;
  internal WebBrowser ContactBrowser;
  internal TextBlock ContactTextBlock;
  internal ScrollViewer WrapScroller;
  internal StackPanel HomeButtonPanel;
  internal Button TrainingExpander;
  internal Button FamilyExpander;
  internal Button ManualsButton;
  internal Button VideosButton;
  internal Button RefreshButton;
  internal Button ContactButton;
  internal MediaElement MediaElement;
  internal ScrollViewer TrainingScrollViewer;
  internal TreeView TrainingTreeView;
  internal ScrollViewer VideoScrollViewer;
  internal TreeView TrainingVideosTreeView;
  internal ScrollViewer SearchScrollViewer;
  internal TreeView SearchTreeView;
  internal TextBlock FindBarValueHolder;
  internal ScrollViewer FamilyScrollViewer;
  internal TreeView FamilyTreeView;
  internal DataGrid TrainingFolderDataGrid;
  internal DataGrid SublFolderDataGrid;
  internal DataGrid ContentDataGrid;
  internal DocumentViewer xps;
  internal TextBlock CurrentTime;
  internal TextBlock TotalTime;
  internal StackPanel sliderPanel;
  internal ProgressBar mediaProgressBar;
  internal Slider sliderBar;
  internal Button MediaBackButton;
  internal Button TrainingBackButton;
  internal Button XPSBackButton;
  internal Button HomeButton;
  internal WrapPanel MediaButtonPanel;
  internal Button btnPlay;
  internal Button btnPause;
  internal Button btnStop;
  internal Button btnMute;
  internal ComboBox qualityBox;
  internal Button btnDownload;
  internal Button btnPopOut;
  internal Label LoadingLbl;
  private bool _contentLoaded;

  public PaneWindow(bool refresh = true)
  {
    this.aTimer = new System.Timers.Timer(2000.0);
    this.aTimer.Elapsed += new ElapsedEventHandler(this.timer_Elapsed2);
    this.aTimer.AutoReset = false;
    this.masterFamilyDirectory = "EDGE^Revit Content/EDGE^Revit " + this.year;
    this.masterTrainingDirectory = "EDGE^Revit Content/Training";
    this.masterTrainingVideosDirectory = "EDGE^Revit Content/Training";
    this.InitializeComponent();
    this.timer = new DispatcherTimer();
    this.timer.Interval = TimeSpan.FromMilliseconds(1000.0);
    this.timer.Tick += new EventHandler(this.timer_Tick);
    if (refresh)
    {
      this.FamilyExpander.IsEnabled = false;
      this.TrainingExpander.IsEnabled = false;
      this.ContactButton.IsEnabled = false;
      this.FindBar.IsEnabled = false;
      this.windowRefresh();
    }
    else
    {
      if (App.ParentPaneWindowRef.trainingFilePaths != null)
        this.trainingFilePaths = App.ParentPaneWindowRef.trainingFilePaths;
      if (App.ParentPaneWindowRef.documentFriendlyNames != null)
        this.documentFriendlyNames = App.ParentPaneWindowRef.documentFriendlyNames;
      if (App.ParentPaneWindowRef.videoFriendlyNames != null)
        this.videoFriendlyNames = App.ParentPaneWindowRef.videoFriendlyNames;
      if (App.ParentPaneWindowRef.familyFilePaths != null)
        this.familyFilePaths = App.ParentPaneWindowRef.familyFilePaths;
      if (App.ParentPaneWindowRef.familyFriendlyNames != null)
        this.familyFriendlyNames = App.ParentPaneWindowRef.familyFriendlyNames;
      if (App.ParentPaneWindowRef.txtFamilyPaths != null)
        this.txtFamilyPaths = App.ParentPaneWindowRef.txtFamilyPaths;
      if (App.ParentPaneWindowRef.TrainingTreeView.Items.Count > 0)
      {
        foreach (TreeViewItem oldItem in (IEnumerable) App.ParentPaneWindowRef.TrainingTreeView.Items)
          this.TrainingTreeView.Items.Add((object) this.copyTreeViewTrainingItem(oldItem));
      }
      if (App.ParentPaneWindowRef.TrainingVideosTreeView.Items.Count > 0)
      {
        foreach (TreeViewItem oldItem in (IEnumerable) App.ParentPaneWindowRef.TrainingVideosTreeView.Items)
          this.TrainingVideosTreeView.Items.Add((object) this.copyTreeViewTrainingVideoItem(oldItem));
      }
      if (App.ParentPaneWindowRef.FamilyTreeView.Items.Count > 0)
      {
        foreach (TreeViewItem oldItem in (IEnumerable) App.ParentPaneWindowRef.FamilyTreeView.Items)
          this.FamilyTreeView.Items.Add((object) this.copyTreeViewFamilyItem(oldItem));
      }
      this.TrainingTreeView.FontSize = 15.0;
      this.TrainingVideosTreeView.FontSize = 15.0;
      this.FamilyTreeView.FontSize = 15.0;
      this.FindBar.Visibility = Visibility.Visible;
      this.LoadedTreeViews = true;
      this.contentdisplayed = true;
    }
  }

  public void windowRefresh()
  {
    this.RefreshAsync();
    this.TrainingTreeView.FontSize = 15.0;
    this.TrainingVideosTreeView.FontSize = 15.0;
    this.FamilyTreeView.FontSize = 15.0;
    this.FindBar.Visibility = Visibility.Visible;
  }

  private TreeViewItem copyTreeViewTrainingItem(TreeViewItem oldItem)
  {
    TreeViewItem treeViewItem = new TreeViewItem();
    treeViewItem.Header = oldItem.Header;
    treeViewItem.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
    if (oldItem.Items.Count > 0)
    {
      foreach (TreeViewItem oldItem1 in (IEnumerable) oldItem.Items)
        treeViewItem.Items.Add((object) this.copyTreeViewTrainingItem(oldItem1));
    }
    else
      treeViewItem.Selected += new RoutedEventHandler(this.TrainingTreeView_Click);
    return treeViewItem;
  }

  private TreeViewItem copyTreeViewTrainingVideoItem(TreeViewItem oldItem)
  {
    TreeViewItem treeViewItem = new TreeViewItem();
    treeViewItem.Header = oldItem.Header;
    treeViewItem.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
    if (oldItem.Items.Count > 0)
    {
      foreach (TreeViewItem oldItem1 in (IEnumerable) oldItem.Items)
        treeViewItem.Items.Add((object) this.copyTreeViewTrainingVideoItem(oldItem1));
    }
    else
      treeViewItem.Selected += new RoutedEventHandler(this.TrainingVideoTreeView_Click);
    return treeViewItem;
  }

  private TreeViewItem copyTreeViewFamilyItem(TreeViewItem oldItem)
  {
    TreeViewItem treeViewItem = new TreeViewItem();
    treeViewItem.Header = oldItem.Header;
    if (oldItem.Items.Count > 0)
    {
      foreach (TreeViewItem oldItem1 in (IEnumerable) oldItem.Items)
        treeViewItem.Items.Add((object) this.copyTreeViewFamilyItem(oldItem1));
    }
    else
    {
      treeViewItem.ContextMenu = this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu;
      treeViewItem.Selected += new RoutedEventHandler(this.FamilyTreeViewSetSelected_Click);
      treeViewItem.MouseUp += new MouseButtonEventHandler(this.FamilyTreeViewSet_MouseUp);
    }
    return treeViewItem;
  }

  private void Window_Loaded(object sender, RoutedEventArgs e) => App.ParentPaneWindowRef = this;

  private void TrainingButton_Click(object sender, RoutedEventArgs e)
  {
    this.ManualsButton.Visibility = Visibility.Visible;
    this.ManualsButton.IsEnabled = true;
    this.VideosButton.Visibility = Visibility.Visible;
    this.VideosButton.IsEnabled = true;
    this.HomeButton.Visibility = Visibility.Visible;
    this.HomeButton.IsEnabled = true;
    this.MainIcon.IsEnabled = true;
    this.FamilyExpander.Visibility = Visibility.Collapsed;
    this.FamilyExpander.IsEnabled = false;
    this.TrainingExpander.Visibility = Visibility.Collapsed;
    this.TrainingExpander.IsEnabled = false;
    this.ContactButton.Visibility = Visibility.Collapsed;
    this.ContactButton.IsEnabled = false;
  }

  private void ToolsButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.CheckInternetConnection())
    {
      this.state = WindowState.TreeView;
      this.HomeButton.Visibility = Visibility.Visible;
      this.HomeButton.IsEnabled = true;
      this.TrainingTreeView.Visibility = Visibility.Visible;
      this.TrainingTreeView.IsEnabled = true;
      this.TrainingScrollViewer.Visibility = Visibility.Visible;
      this.TrainingScrollViewer.IsEnabled = true;
      this.TrainingBackButton.Visibility = Visibility.Visible;
      this.TrainingBackButton.IsEnabled = true;
      this.ManualsButton.Visibility = Visibility.Collapsed;
      this.ManualsButton.IsEnabled = false;
      this.VideosButton.Visibility = Visibility.Collapsed;
      this.VideosButton.IsEnabled = false;
      this.FamilyExpander.Visibility = Visibility.Collapsed;
      this.FamilyExpander.IsEnabled = false;
      this.TrainingExpander.Visibility = Visibility.Collapsed;
      this.TrainingExpander.IsEnabled = false;
      this.ContactButton.Visibility = Visibility.Collapsed;
      this.ContactButton.IsEnabled = false;
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void VideosButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.CheckInternetConnection())
    {
      this.state = WindowState.TreeView;
      this.HomeButton.Visibility = Visibility.Visible;
      this.HomeButton.IsEnabled = true;
      this.TrainingVideosTreeView.Visibility = Visibility.Visible;
      this.TrainingVideosTreeView.IsEnabled = true;
      this.VideoScrollViewer.Visibility = Visibility.Visible;
      this.VideoScrollViewer.IsEnabled = true;
      this.TrainingBackButton.Visibility = Visibility.Visible;
      this.TrainingBackButton.IsEnabled = true;
      this.ManualsButton.Visibility = Visibility.Collapsed;
      this.ManualsButton.IsEnabled = false;
      this.VideosButton.Visibility = Visibility.Collapsed;
      this.VideosButton.IsEnabled = false;
      this.FamilyExpander.Visibility = Visibility.Collapsed;
      this.FamilyExpander.IsEnabled = false;
      this.TrainingExpander.Visibility = Visibility.Collapsed;
      this.TrainingExpander.IsEnabled = false;
      this.ContactButton.Visibility = Visibility.Collapsed;
      this.ContactButton.IsEnabled = false;
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void ManualButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.CheckInternetConnection())
    {
      this.DisplayDocument("", this.trainManualPath, true);
      this.HomeButton.Visibility = Visibility.Visible;
      this.HomeButton.IsEnabled = true;
      this.FamilyExpander.Visibility = Visibility.Collapsed;
      this.FamilyExpander.IsEnabled = false;
      this.TrainingExpander.Visibility = Visibility.Collapsed;
      this.TrainingExpander.IsEnabled = false;
      this.ContactButton.Visibility = Visibility.Collapsed;
      this.ContactButton.IsEnabled = false;
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void FamToolsButton_Click(object sender, RoutedEventArgs e)
  {
    if (this.CheckInternetConnection())
    {
      this.state = WindowState.TreeView;
      this.MainIcon.IsEnabled = true;
      this.FamilyTreeView.Visibility = Visibility.Visible;
      this.FamilyTreeView.IsEnabled = true;
      this.FamilyScrollViewer.Visibility = Visibility.Visible;
      this.FamilyScrollViewer.IsEnabled = true;
      this.HomeButton.Visibility = Visibility.Visible;
      this.HomeButton.IsEnabled = true;
      this.FamilyExpander.Visibility = Visibility.Collapsed;
      this.FamilyExpander.IsEnabled = false;
      this.TrainingExpander.Visibility = Visibility.Collapsed;
      this.TrainingExpander.IsEnabled = false;
      this.ContactButton.Visibility = Visibility.Collapsed;
      this.ContactButton.IsEnabled = false;
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void ContactButton_Click(object sender, RoutedEventArgs e)
  {
    this.MainIcon.IsEnabled = true;
    this.ContactTextBlock.Visibility = Visibility.Visible;
    this.ContactTextBlock.IsEnabled = true;
    this.HomeButton.Visibility = Visibility.Visible;
    this.HomeButton.IsEnabled = true;
    this.FamilyExpander.Visibility = Visibility.Collapsed;
    this.FamilyExpander.IsEnabled = false;
    this.TrainingExpander.Visibility = Visibility.Collapsed;
    this.TrainingExpander.IsEnabled = false;
    this.ContactButton.Visibility = Visibility.Collapsed;
    this.ContactButton.IsEnabled = false;
    this.SearchTreeView.Visibility = Visibility.Collapsed;
    this.FindBar.Visibility = Visibility.Collapsed;
    this.FindBarValueHolder.Visibility = Visibility.Collapsed;
    this.RefreshButton.Visibility = Visibility.Collapsed;
  }

  private void CollapseTVItems(TreeViewItem Item)
  {
    Item.IsExpanded = false;
    foreach (TreeViewItem treeViewItem in (IEnumerable) Item.Items)
    {
      treeViewItem.IsExpanded = false;
      if (treeViewItem.HasItems)
        this.CollapseTVItems(treeViewItem);
    }
  }

  private void TrainingBackButton_Click(object sender, RoutedEventArgs e)
  {
    this.state = WindowState.Main;
    this.TrainingTreeView.Visibility = Visibility.Collapsed;
    foreach (TreeViewItem treeViewItem in (IEnumerable) this.TrainingTreeView.Items)
      this.CollapseTVItems(treeViewItem);
    this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
    this.TrainingVideosTreeView.Visibility = Visibility.Collapsed;
    foreach (TreeViewItem treeViewItem in (IEnumerable) this.TrainingVideosTreeView.Items)
      this.CollapseTVItems(treeViewItem);
    this.VideoScrollViewer.Visibility = Visibility.Collapsed;
    this.TrainingBackButton.Visibility = Visibility.Collapsed;
    this.TrainingBackButton.IsEnabled = false;
    this.VideosButton.Visibility = Visibility.Visible;
    this.VideosButton.IsEnabled = true;
    this.ManualsButton.Visibility = Visibility.Visible;
    this.ManualsButton.IsEnabled = true;
    this.MainIcon.Visibility = Visibility.Visible;
  }

  private void ContactBackButton_Click(object sender, RoutedEventArgs e)
  {
    this.state = WindowState.Main;
    this.ContactTextBlock.Visibility = Visibility.Collapsed;
    this.ContactTextBlock.IsEnabled = false;
    this.MainIcon.IsEnabled = false;
    this.HomeButton.Visibility = Visibility.Collapsed;
    this.HomeButton.IsEnabled = false;
    this.MainIcon.Visibility = Visibility.Visible;
    this.FamilyExpander.Visibility = Visibility.Visible;
    this.FamilyExpander.IsEnabled = true;
    this.TrainingExpander.Visibility = Visibility.Visible;
    this.MainIcon.Visibility = Visibility.Visible;
    this.TrainingExpander.IsEnabled = true;
    this.ContactButton.Visibility = Visibility.Visible;
    this.ContactButton.IsEnabled = true;
  }

  private void XPSBackButton_Click(object sender, RoutedEventArgs e)
  {
    this.xps.Visibility = Visibility.Collapsed;
    this.xps.IsEnabled = false;
    PackageStore.GetPackage(this.xpsDocument.Uri).Close();
    PackageStore.RemovePackage(this.xpsDocument.Uri);
    this.XPSBackButton.Visibility = Visibility.Collapsed;
    this.XPSBackButton.IsEnabled = false;
    this.state = WindowState.TreeView;
    this.TrainingTreeView.Visibility = Visibility.Visible;
    this.TrainingScrollViewer.Visibility = Visibility.Visible;
    this.TrainingScrollViewer.IsEnabled = true;
    this.TrainingTreeView.IsEnabled = true;
    if (this.TrainingTreeView.SelectedItem != null)
    {
      (this.TrainingTreeView.SelectedItem as TreeViewItem).IsSelected = false;
      this.TrainingTreeView.Focus();
    }
    this.TrainingBackButton.Visibility = Visibility.Visible;
    this.MainIcon.Visibility = Visibility.Visible;
    this.FindBar.Visibility = Visibility.Visible;
    if (this.searchresultstatus)
    {
      this.SearchTreeView.Visibility = Visibility.Visible;
      this.FindBarValueHolder.Visibility = Visibility.Visible;
      this.TrainingTreeView.Visibility = Visibility.Collapsed;
      this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
      this.FamilyExpander.Visibility = Visibility.Collapsed;
      this.TrainingExpander.Visibility = Visibility.Collapsed;
      this.TrainingBackButton.Visibility = Visibility.Collapsed;
    }
    this.TrainingBackButton.IsEnabled = true;
  }

  private void HomeButton_Click(object sender, RoutedEventArgs e) => this.goHome();

  public void goHome()
  {
    this.ContentGrid.Visibility = Visibility.Visible;
    this.LoadingLbl.Visibility = Visibility.Hidden;
    this.xps.Visibility = Visibility.Collapsed;
    this.xps.IsEnabled = false;
    if (this.xpsDocument != null && PackageStore.GetPackage(this.xpsDocument.Uri) != null)
    {
      PackageStore.GetPackage(this.xpsDocument.Uri).Close();
      PackageStore.RemovePackage(this.xpsDocument.Uri);
    }
    this.XPSBackButton.Visibility = Visibility.Collapsed;
    this.XPSBackButton.IsEnabled = false;
    this.state = WindowState.Main;
    this.MainIcon.IsEnabled = false;
    this.ContactTextBlock.Visibility = Visibility.Collapsed;
    this.ContactTextBlock.IsEnabled = false;
    this.ManualsButton.Visibility = Visibility.Collapsed;
    this.ManualsButton.IsEnabled = false;
    this.VideosButton.Visibility = Visibility.Collapsed;
    this.VideosButton.IsEnabled = false;
    this.TrainingVideosTreeView.Visibility = Visibility.Collapsed;
    this.TrainingVideosTreeView.IsEnabled = false;
    foreach (TreeViewItem treeViewItem in (IEnumerable) this.TrainingVideosTreeView.Items)
      this.CollapseTVItems(treeViewItem);
    this.VideoScrollViewer.Visibility = Visibility.Collapsed;
    this.VideoScrollViewer.IsEnabled = false;
    if (this.TrainingVideosTreeView.SelectedItem != null)
    {
      (this.TrainingVideosTreeView.SelectedItem as TreeViewItem).IsSelected = false;
      this.TrainingVideosTreeView.Focus();
    }
    this.TrainingTreeView.Visibility = Visibility.Collapsed;
    this.TrainingTreeView.IsEnabled = false;
    foreach (TreeViewItem treeViewItem in (IEnumerable) this.TrainingTreeView.Items)
      this.CollapseTVItems(treeViewItem);
    this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
    this.TrainingScrollViewer.IsEnabled = false;
    if (this.TrainingTreeView.SelectedItem != null)
    {
      (this.TrainingTreeView.SelectedItem as TreeViewItem).IsSelected = false;
      this.TrainingTreeView.Focus();
    }
    this.TrainingFolderDataGrid.Visibility = Visibility.Collapsed;
    this.FamilyTreeView.Visibility = Visibility.Collapsed;
    this.FamilyTreeView.IsEnabled = false;
    foreach (TreeViewItem treeViewItem in (IEnumerable) this.FamilyTreeView.Items)
      this.CollapseTVItems(treeViewItem);
    this.FamilyScrollViewer.Visibility = Visibility.Collapsed;
    this.FamilyScrollViewer.IsEnabled = false;
    this.MediaElement.Visibility = Visibility.Collapsed;
    this.MediaElement.LoadedBehavior = MediaState.Manual;
    this.MediaElement.IsEnabled = false;
    this.MediaElement.Stop();
    this.MediaElement.Close();
    this.MediaElement_MediaEnded();
    this.MediaBackButton.Visibility = Visibility.Collapsed;
    this.MediaBackButton.IsEnabled = false;
    this.TrainingBackButton.Visibility = Visibility.Collapsed;
    this.TrainingBackButton.IsEnabled = false;
    this.MediaButtonPanel.Visibility = Visibility.Collapsed;
    this.MediaButtonPanel.IsEnabled = false;
    this.sliderPanel.Visibility = Visibility.Collapsed;
    this.sliderPanel.IsEnabled = false;
    this.TotalTime.Visibility = Visibility.Collapsed;
    this.CurrentTime.Visibility = Visibility.Collapsed;
    this.sliderBar.Value = 0.0;
    this.timer.Stop();
    this.SublFolderDataGrid.Visibility = Visibility.Collapsed;
    this.TrainingFolderDataGrid.Visibility = Visibility.Collapsed;
    this.HomeButton.Visibility = Visibility.Collapsed;
    this.HomeButton.IsEnabled = false;
    this.SearchTreeView.Visibility = Visibility.Collapsed;
    this.FindBarValueHolder.Visibility = Visibility.Collapsed;
    this.searchresultstatus = false;
    this.SearchScrollViewer.Visibility = Visibility.Collapsed;
    if (this.contentdisplayed)
    {
      this.FindBar.Visibility = Visibility.Visible;
      this.FindBar.Clear();
      this.FindBar.AppendText("Search");
      if (!this.LoadedTreeViews)
        this.FindBar.IsEnabled = false;
    }
    this.MainIcon.Visibility = Visibility.Visible;
    this.MainIcon.Background = (Brush) Brushes.Transparent;
    if (this.contentdisplayed)
    {
      if (this.LoadedTreeViews)
      {
        this.FamilyExpander.Visibility = Visibility.Visible;
        this.FamilyExpander.IsEnabled = true;
        this.TrainingExpander.Visibility = Visibility.Visible;
        this.TrainingExpander.IsEnabled = true;
      }
      else
      {
        this.FamilyExpander.Visibility = Visibility.Collapsed;
        this.FamilyExpander.IsEnabled = false;
        this.TrainingExpander.Visibility = Visibility.Collapsed;
        this.TrainingExpander.IsEnabled = false;
        this.ContactButton.Visibility = Visibility.Collapsed;
        this.ContactButton.IsEnabled = false;
      }
    }
    else
    {
      this.FamilyExpander.Visibility = Visibility.Collapsed;
      this.FamilyExpander.IsEnabled = false;
      this.TrainingExpander.Visibility = Visibility.Collapsed;
      this.TrainingExpander.IsEnabled = false;
      this.ContactButton.Visibility = Visibility.Collapsed;
      this.ContactButton.IsEnabled = false;
      this.RefreshButton.Visibility = Visibility.Visible;
      this.RefreshButton.IsEnabled = true;
      this.FindBar.IsEnabled = false;
    }
    this.ContactButton.Visibility = Visibility.Visible;
    this.ContactButton.IsEnabled = true;
  }

  private void MediaBackButton_Click(object sender, RoutedEventArgs e)
  {
    this.state = WindowState.TreeView;
    this.MediaElement.Visibility = Visibility.Collapsed;
    this.MediaElement.IsEnabled = false;
    this.MediaElement.Stop();
    this.MediaBackButton.Visibility = Visibility.Collapsed;
    this.MediaBackButton.IsEnabled = false;
    this.MediaButtonPanel.Visibility = Visibility.Collapsed;
    this.MediaButtonPanel.IsEnabled = false;
    this.sliderPanel.Visibility = Visibility.Collapsed;
    this.sliderPanel.IsEnabled = false;
    this.TotalTime.Visibility = Visibility.Collapsed;
    this.CurrentTime.Visibility = Visibility.Collapsed;
    this.sliderBar.Value = 0.0;
    this.timer.Stop();
    this.MediaElement.Close();
    this.MediaElement_MediaEnded();
    this.TrainingVideosTreeView.Visibility = Visibility.Visible;
    this.TrainingVideosTreeView.IsEnabled = true;
    this.VideoScrollViewer.Visibility = Visibility.Visible;
    this.VideoScrollViewer.IsEnabled = true;
    if (this.TrainingVideosTreeView.SelectedItem != null)
    {
      (this.TrainingVideosTreeView.SelectedItem as TreeViewItem).IsSelected = false;
      this.TrainingVideosTreeView.Focus();
    }
    this.TrainingBackButton.Visibility = Visibility.Visible;
    this.TrainingBackButton.IsEnabled = true;
    this.MainIcon.Visibility = Visibility.Visible;
    this.FindBar.Visibility = Visibility.Visible;
    if (!this.searchresultstatus)
      return;
    this.SearchTreeView.Visibility = Visibility.Visible;
    this.FindBarValueHolder.Visibility = Visibility.Visible;
    this.TrainingVideosTreeView.Visibility = Visibility.Collapsed;
    this.TrainingBackButton.Visibility = Visibility.Collapsed;
  }

  private void FolderCell_Click(object sender, RoutedEventArgs e)
  {
    DependencyObject reference = (DependencyObject) e.OriginalSource;
    while (true)
    {
      switch (reference)
      {
        case null:
        case DataGridCell _:
        case DataGridColumnHeader _:
          goto label_3;
        default:
          reference = VisualTreeHelper.GetParent(reference);
          continue;
      }
    }
label_3:
    if (!(reference is DataGridCell))
      return;
    if (this.CheckInternetConnection())
    {
      DataGridCell dataGridCell = reference as DataGridCell;
      string folderName;
      try
      {
        folderName = Convert.ToString(((TextBlock) dataGridCell.Content).Text);
      }
      catch
      {
        return;
      }
      if (folderName.Equals(""))
        return;
      if (folderName.ToUpper().Contains(".RFA") && !folderName.Equals(""))
      {
        if (this.currentFilePath.Equals(""))
        {
          string str1 = this.prevFilePath + folderName;
        }
        else if (!this.prevFilePath.Equals(this.currentFilePath))
        {
          string str2 = this.currentFilePath + folderName;
        }
        else
        {
          string str3 = this.prevFilePath + folderName;
        }
        this.fileName = folderName;
      }
      else
      {
        string inputFilePath = !this.currentFilePath.Equals("") ? (this.prevFilePath.Equals(this.currentFilePath) ? $"{this.prevFilePath}{folderName}/" : $"{this.currentFilePath}{folderName}/") : $"{this.prevFilePath}{folderName}/";
        this.CreateSubFolders(folderName, inputFilePath);
      }
      this.SublFolderDataGrid.Visibility = Visibility.Visible;
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void FamilyTreeView_Click(object sender, RoutedEventArgs e)
  {
    sender = (object) this.selectedTVI;
    if (!(sender is TreeViewItem))
      return;
    if (this.CheckInternetConnection())
    {
      TreeViewItem treeViewItem = sender as TreeViewItem;
      treeViewItem.IsSelected = false;
      string str1 = treeViewItem.Header.ToString();
      if (str1.Equals(""))
        return;
      foreach (Tuple<string, string> familyFriendlyName in this.familyFriendlyNames)
      {
        if (str1.Equals(familyFriendlyName.Item2))
        {
          str1 = familyFriendlyName.Item1.ToString();
          break;
        }
      }
      string str2 = "";
      this.uritxtloadfilename = "";
      this.txtloadfilename = "";
      foreach (Tuple<string, string> familyFilePath in this.familyFilePaths)
      {
        if (familyFilePath.Item2.Equals(str1))
          str2 = familyFilePath.Item1;
      }
      foreach (Tuple<string, string> txtFamilyPath in this.txtFamilyPaths)
      {
        if (txtFamilyPath.Item2.Equals(str1.Split('.')[0] + ".txt"))
          this.uritxtloadfilename = txtFamilyPath.Item1;
      }
      if (!str1.ToUpper().Contains(".RFA") || str1.Equals(""))
        return;
      string str3 = str2;
      string str4 = $"http://edgeptacblob.blob.core.windows.net/{EDGE.EDGEBrowser.EDGEBrowser.containerString}/";
      string str5 = new Uri(str4 + str3).ToString();
      this._LoadUrls.Enqueue(str5);
      this.loadFilename = str1;
      this.uriloadfilename = str5;
      if (!string.IsNullOrEmpty(this.uritxtloadfilename))
      {
        string str6 = str1.Split('.')[0] + ".txt";
        string str7 = new Uri(str4 + this.uritxtloadfilename).ToString();
        this._LoadUrls.Enqueue(str7);
        this.txtloadfilename = str6;
        this.uritxtloadfilename = str7;
      }
      if (this.CheckforReadOnly("C:\\EDGEforRevit\\FamiliesTemp"))
      {
        int num = (int) MessageBox.Show("Please check folder attributes. It is set to Read Only mode.", "Folder Access Warning", MessageBoxButton.OK, MessageBoxImage.None);
        this._LoadUrls.Clear();
      }
      else
      {
        foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
        {
          if (obj is MenuItem)
            (obj as MenuItem).IsEnabled = false;
        }
        this.loadFile();
      }
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void loadFile()
  {
    if (this._LoadUrls.Any<string>())
    {
      this.somethingIsDownloading = true;
      WebClient webClient = new WebClient();
      if (!Directory.Exists("C:\\EDGEforRevit\\FamiliesTemp"))
        Directory.CreateDirectory("C:\\EDGEforRevit\\FamiliesTemp");
      webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.client_LoadProgressChanged);
      string uriString = this._LoadUrls.Dequeue();
      this.LoadingFileName = uriString.Substring(uriString.LastIndexOf("/") + 1, uriString.Length - uriString.LastIndexOf("/") - 1);
      webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.client_LoadFileCompleted);
      webClient.DownloadFileAsync(new Uri(uriString), "C:\\EDGEforRevit\\FamiliesTemp\\" + this.LoadingFileName);
      this.btnDownload.IsEnabled = false;
      this.currentDownloadClient = webClient;
      this.cancelDownload = false;
    }
    else
    {
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      this.DataContext = (object) new BrowserViewModel(this.uriloadfilename, this.loadFilename, this.uritxtloadfilename, this.txtloadfilename)
      {
        FamilyLoadEvent = this.famLoadEvent
      };
      App.ParentPaneWindowRef = this;
      ((BrowserViewModel) App.ParentPaneWindowRef.DataContext).FamilyLoadCommand.Execute((object) null);
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Text = "";
      this.btnDownload.IsEnabled = true;
      this.somethingIsDownloading = false;
      if (this.listwhensomethingisdownloading.Count <= 0)
        return;
      this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
      foreach (UIElement uiElement in this.listwhensomethingisdownloading)
        uiElement.IsEnabled = true;
    }
  }

  private void CancelDownload_Click(object sender, RoutedEventArgs e) => this.cancelDownload = true;

  private void client_LoadFileCompleted(object sender, AsyncCompletedEventArgs e)
  {
    if (this.aTimer.Enabled)
      this.aTimer.Stop();
    this.cancelDownload = false;
    try
    {
      if (e.Cancelled || e.Error != null)
        throw e.Error;
      this.loadFile();
      this.CancelDownload.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Visibility = Visibility.Collapsed;
    }
    catch (Exception ex)
    {
      this.loadfiledelete(this.LoadingFileName);
      if (this.internetLoss || ex.Message.Contains("502"))
      {
        int num1 = (int) MessageBox.Show("Please check your internet connection.", "Lost Internet Connection");
        int num2 = (int) MessageBox.Show($"Loading of {this.loadFilename} was canceled.");
        this.internetLoss = false;
      }
      else if (ex.Message.Contains("The request was aborted: The request was canceled."))
      {
        int num3 = (int) MessageBox.Show($"Loading of {this.loadFilename} was canceled.");
      }
      else
      {
        int num4 = (int) MessageBox.Show("Unable to load family. Family either no longer exists or encountered an error while loading.", "Unable to Load Family");
      }
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      this._LoadUrls.Clear();
      this.CancelDownload.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.btnDownload.IsEnabled = true;
      this.somethingIsDownloading = false;
      if (this.listwhensomethingisdownloading.Count <= 0)
        return;
      this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
      foreach (UIElement uiElement in this.listwhensomethingisdownloading)
        uiElement.IsEnabled = true;
    }
  }

  private void client_LoadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
  {
    this.dlTextBlock.Text = $"{this.LoadingFileName} - Download Progress: {Math.Round((double) e.BytesReceived / (double) e.TotalBytesToReceive * 100.0, 0).ToString()}%";
    this.dlTextBlock.Visibility = Visibility.Visible;
    this.CancelDownload.Visibility = Visibility.Visible;
    if (this.aTimer.Enabled)
    {
      this.aTimer.Stop();
      this.aTimer.Start();
    }
    else
      this.aTimer.Start();
    if (!this.cancelDownload)
      return;
    this.cancelDownload = true;
    (sender as WebClient).CancelAsync();
  }

  private void loadfiledelete(string filenameToBeDeleted)
  {
    if (!Directory.Exists("C:\\EDGEforRevit\\FamiliesTemp"))
      return;
    try
    {
      foreach (string enumerateFile in Directory.EnumerateFiles("C:\\EDGEforRevit\\FamiliesTemp"))
      {
        if (((IEnumerable<string>) ((IEnumerable<string>) enumerateFile.Split('\\')).Last<string>().Split('.')).First<string>().Equals(((IEnumerable<string>) filenameToBeDeleted.Split('.')).First<string>()) && System.IO.File.Exists(enumerateFile))
          System.IO.File.Delete(enumerateFile);
      }
    }
    catch
    {
      int num = (int) MessageBox.Show("File Failed to Download. Please ensure specified folder C:\\EDGEforRevit\\Families\\ is not in use for another download at this time.", "Download failed");
    }
  }

  private void DownloadFamily_RightButtonDown(object sender, RoutedEventArgs e)
  {
    List<string> stringList = new List<string>();
    sender = (object) this.selectedTVI;
    if (!(sender is TreeViewItem))
      return;
    if (this.CheckInternetConnection())
    {
      TreeViewItem treeViewItem = sender as TreeViewItem;
      treeViewItem.IsSelected = false;
      string str1 = treeViewItem.Header.ToString();
      if (str1.Equals(""))
        return;
      foreach (Tuple<string, string> familyFilePath in this.familyFilePaths)
      {
        if (familyFilePath.Item2.Equals(str1 + ".rfa"))
          stringList.Add(familyFilePath.Item1);
      }
      foreach (Tuple<string, string> xlsxFamilyPath in this.xlsxFamilyPaths)
      {
        if (xlsxFamilyPath.Item2.Contains(str1 + ".xlsx"))
          stringList.Add(xlsxFamilyPath.Item1);
      }
      foreach (Tuple<string, string> txtFamilyPath in this.txtFamilyPaths)
      {
        if (txtFamilyPath.Item2.Contains(str1 + ".txt"))
          stringList.Add(txtFamilyPath.Item1);
      }
      foreach (string str2 in stringList)
      {
        string str3;
        string str4 = ((IEnumerable<string>) (str3 = str2).Split('/')).Last<string>();
        if (str4.ToUpper().Contains(".RFA") || str4.ToUpper().Contains(".XLSX") || str4.ToUpper().Contains(".TXT"))
        {
          this.dlTextBlock.Text = $"Downloading {str4} , please wait . . . ";
          this._downloadUrls.Enqueue($"http://edgeptacblob.blob.core.windows.net/{EDGE.EDGEBrowser.EDGEBrowser.containerString}/{str3}");
        }
      }
      if (this.CheckforReadOnly("C:\\EDGEforRevit\\Families"))
      {
        int num = (int) MessageBox.Show("Please check folder attributes. It is set to Read Only mode.", "Folder Access Warning", MessageBoxButton.OK, MessageBoxImage.None);
        this._downloadUrls.Clear();
      }
      else
      {
        foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
        {
          if (obj is MenuItem)
            (obj as MenuItem).IsEnabled = false;
        }
        this.downloadfile();
      }
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void downloadfile()
  {
    if (this._downloadUrls.Any<string>())
    {
      this.downloadingFileName = "";
      this.somethingIsDownloading = true;
      WebClient webClient = new WebClient();
      try
      {
        if (!Directory.Exists("C:\\EDGEforRevit\\Families"))
          Directory.CreateDirectory("C:\\EDGEforRevit\\Families");
        string uriString = this._downloadUrls.Dequeue();
        this.downloadingFileName = uriString.Substring(uriString.LastIndexOf('/') + 1, uriString.Length - uriString.LastIndexOf('/') - 1);
        if (this.CheckforReadOnly("C:\\EDGEforRevit\\Families\\" + this.downloadingFileName))
        {
          int num = (int) MessageBox.Show("Please check permissions. This file exists in Read Only Mode.", "Permisssions Error: File Exist");
          this._downloadUrls.Clear();
          this.dlTextBlock.Visibility = Visibility.Collapsed;
          this.dlTextBlock.Text = "";
          this.somethingIsDownloading = false;
          foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
          {
            if (obj is MenuItem)
              (obj as MenuItem).IsEnabled = true;
          }
        }
        else
        {
          webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.client_DownloadProgressChanged);
          webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.client_DownloadFileCompleted);
          webClient.DownloadFileAsync(new Uri(uriString), "C:\\EDGEforRevit\\Families\\" + this.downloadingFileName);
          this.btnDownload.IsEnabled = false;
          this.currentDownloadClient = webClient;
          this.cancelDownload = false;
        }
      }
      catch
      {
        int num = (int) MessageBox.Show($"File Failed to Download. Please ensure specified path C:\\EDGEforRevit\\Families\\{this.downloadingFileName} is not in use at this time.", "Download failed");
      }
    }
    else
    {
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      int length = this.downloadingFileName.IndexOf('.');
      if (length > -1)
        this.downloadingFileName = this.downloadingFileName.Substring(0, length);
      int num = (int) MessageBox.Show("Family has been saved to C:\\EDGEforRevit\\Families\\" + this.downloadingFileName, "Family Saved", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Text = "";
      this.btnDownload.IsEnabled = true;
      this.somethingIsDownloading = false;
      if (this.listwhensomethingisdownloading.Count <= 0)
        return;
      this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
      foreach (UIElement uiElement in this.listwhensomethingisdownloading)
        uiElement.IsEnabled = true;
    }
  }

  private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
  {
    if (this.aTimer.Enabled)
      this.aTimer.Stop();
    this.cancelDownload = false;
    try
    {
      if (e.Cancelled || e.Error != null)
        throw e.Error;
      this.downloadfile();
      this.CancelDownload.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Visibility = Visibility.Collapsed;
    }
    catch (Exception ex)
    {
      this.filedelete(this.downloadingFileName);
      if (this.internetLoss || ex.Message.Contains("502"))
      {
        int num1 = (int) MessageBox.Show("Please check your internet connection.", "Lost Internet Connection");
        int num2 = (int) MessageBox.Show($"Loading of {this.downloadingFileName} was canceled.");
        this.internetLoss = false;
      }
      else if (ex.Message.Contains("The request was aborted: The request was canceled.") || ex.Message.Contains("502"))
      {
        int num3 = (int) MessageBox.Show($"Download of {this.downloadingFileName} was canceled.");
      }
      else
      {
        int num4 = (int) MessageBox.Show("Unable to download family. Family either no longer exists or encountered an error while downloading.", "Unable to Download Family");
      }
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      this._downloadUrls.Clear();
      this.CancelDownload.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.btnDownload.IsEnabled = true;
      this.somethingIsDownloading = false;
      if (this.listwhensomethingisdownloading.Count <= 0)
        return;
      this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
      foreach (UIElement uiElement in this.listwhensomethingisdownloading)
        uiElement.IsEnabled = true;
    }
  }

  private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
  {
    this.dlTextBlock.Text = $"{this.downloadingFileName} - Download Progress: {Math.Round((double) e.BytesReceived / (double) e.TotalBytesToReceive * 100.0, 0).ToString()}%";
    this.dlTextBlock.Visibility = Visibility.Visible;
    this.CancelDownload.Visibility = Visibility.Visible;
    if (this.aTimer.Enabled)
    {
      this.aTimer.Stop();
      this.aTimer.Start();
    }
    else
      this.aTimer.Start();
    if (!this.cancelDownload)
      return;
    this.cancelDownload = true;
    (sender as WebClient).CancelAsync();
  }

  private void filedelete(string filenameToBeDeleted)
  {
    if (!Directory.Exists("C:\\EDGEforRevit\\Families"))
      return;
    foreach (string enumerateFile in Directory.EnumerateFiles("C:\\EDGEforRevit\\Families"))
    {
      if (((IEnumerable<string>) ((IEnumerable<string>) enumerateFile.Split('\\')).Last<string>().Split('.')).First<string>().Equals(((IEnumerable<string>) filenameToBeDeleted.Split('.')).First<string>()) && System.IO.File.Exists(enumerateFile))
        System.IO.File.Delete(enumerateFile);
    }
  }

  private void TrainingTreeView_Click(object sender, RoutedEventArgs e)
  {
    if (!(sender is TreeViewItem))
      return;
    if (this.CheckInternetConnection())
    {
      string folderName = (sender as TreeViewItem).Header.ToString().Replace("&#xE130; ", "");
      if (folderName.Equals(""))
        return;
      foreach (Tuple<string, string> documentFriendlyName in this.documentFriendlyNames)
      {
        if (folderName.Equals(documentFriendlyName.Item2))
        {
          folderName = documentFriendlyName.Item1.ToString();
          break;
        }
      }
      string filePath = "";
      foreach (Tuple<string, string> trainingFilePath in this.trainingFilePaths)
      {
        if (trainingFilePath.Item2.Equals(folderName))
          filePath = trainingFilePath.Item1;
      }
      if (folderName.ToUpper().Contains(".XPS") && !folderName.Equals(""))
      {
        if (this.CheckInternetConnection())
        {
          this.MainIcon.Visibility = Visibility.Collapsed;
          this.FindBar.Visibility = Visibility.Collapsed;
          this.DisplayDocument(folderName, filePath);
          this.TrainingTreeView.Visibility = Visibility.Collapsed;
          this.TrainingTreeView.IsEnabled = false;
          this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
          this.TrainingScrollViewer.IsEnabled = false;
          this.TrainingVideosTreeView.Visibility = Visibility.Collapsed;
          this.TrainingVideosTreeView.IsEnabled = false;
          this.VideoScrollViewer.Visibility = Visibility.Collapsed;
          this.VideoScrollViewer.IsEnabled = false;
          this.TrainingBackButton.Visibility = Visibility.Collapsed;
        }
        else
        {
          int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
          this.contentdisplayed = false;
          this.LoadedTreeViews = false;
          this.goHome();
        }
      }
      else
      {
        int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
        this.contentdisplayed = false;
        this.LoadedTreeViews = false;
        this.goHome();
      }
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void FamilyTreeViewSetSelected_Click(object sender, RoutedEventArgs e)
  {
    if (sender is TreeViewItem && !this.somethingIsDownloading)
    {
      this.selectedTVI = sender as TreeViewItem;
    }
    else
    {
      (sender as TreeViewItem).IsEnabled = false;
      this.listwhensomethingisdownloading.Add(sender as TreeViewItem);
      int num = (int) MessageBox.Show("Please try again. Currently, there is another download in progress.", "Try Again", MessageBoxButton.OK, MessageBoxImage.None);
    }
  }

  private void FamilyTreeViewSet_MouseUp(object sender, MouseButtonEventArgs e)
  {
    if (this.somethingIsDownloading)
      return;
    if (e.ChangedButton.Equals((object) MouseButton.Left))
    {
      this.selectedTVI = sender as TreeViewItem;
      this.selectedTVI.IsSelected = true;
      this.FamilyTreeView_Click(sender, (RoutedEventArgs) e);
    }
    else
    {
      if (!e.ChangedButton.Equals((object) MouseButton.Right))
        return;
      this.selectedTVI = sender as TreeViewItem;
      this.selectedTVI.ContextMenu.IsOpen = true;
    }
  }

  private void TrainingVideoTreeView_Click(object sender, RoutedEventArgs e)
  {
    if (!(sender is TreeViewItem))
      return;
    if (this.CheckInternetConnection())
    {
      string folderName = (sender as TreeViewItem).Header.ToString().Replace("&#xE102; ", "");
      if (folderName.Equals("") || !this.canOpenNewVideos)
        return;
      foreach (Tuple<string, string> videoFriendlyName in this.videoFriendlyNames)
      {
        if (folderName.Equals(videoFriendlyName.Item2))
        {
          folderName = videoFriendlyName.Item1.ToString();
          break;
        }
      }
      string str = "";
      foreach (Tuple<string, string> trainingFilePath in this.trainingFilePaths)
      {
        if (trainingFilePath.Item2.Equals(folderName))
          str = trainingFilePath.Item1;
      }
      string filePath = str.Replace("480p", "720p").Replace("720p", "720p").Replace("1080p", "720p");
      if (folderName.ToUpper().Contains(".MP4") && !folderName.Equals(""))
      {
        if (this.CheckInternetConnection())
        {
          this.MainIcon.Visibility = Visibility.Collapsed;
          this.FindBar.Visibility = Visibility.Collapsed;
          this.DisplayVideo(folderName, filePath);
          this.TrainingTreeView.Visibility = Visibility.Collapsed;
          this.TrainingTreeView.IsEnabled = false;
          this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
          this.TrainingScrollViewer.IsEnabled = false;
          this.TrainingVideosTreeView.Visibility = Visibility.Collapsed;
          this.TrainingVideosTreeView.IsEnabled = false;
          this.VideoScrollViewer.Visibility = Visibility.Collapsed;
          this.VideoScrollViewer.IsEnabled = false;
          this.TrainingBackButton.Visibility = Visibility.Collapsed;
        }
        else
        {
          int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
          this.contentdisplayed = false;
          this.LoadedTreeViews = false;
          this.goHome();
        }
      }
      else
      {
        int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
        this.contentdisplayed = false;
        this.LoadedTreeViews = false;
        this.goHome();
      }
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private static bool CheckValidURI(string uri)
  {
    Uri result;
    if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute) || !Uri.TryCreate(uri, UriKind.Absolute, out result))
      return false;
    return result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps;
  }

  public async Task<bool> CheckInternetConnectionAsync()
  {
    CloudBlobClient blobClient = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient();
    blobClient.GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    BlobRequestOptions blobRequestOptions = new BlobRequestOptions()
    {
      ServerTimeout = new TimeSpan?(TimeSpan.FromSeconds(2.0)),
      MaximumExecutionTime = new TimeSpan?(TimeSpan.FromSeconds(2.0))
    };
    try
    {
      ContainerResultSegment containerResultSegment = await Task.Run<ContainerResultSegment>((Func<Task<ContainerResultSegment>>) (() => blobClient.ListContainersSegmentedAsync(new BlobContinuationToken())));
    }
    catch (Exception ex)
    {
      return false;
    }
    return true;
  }

  public bool CheckInternetConnection()
  {
    CloudBlobClient cloudBlobClient = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient();
    cloudBlobClient.GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    BlobRequestOptions blobRequestOptions = new BlobRequestOptions()
    {
      ServerTimeout = new TimeSpan?(TimeSpan.FromSeconds(2.0)),
      MaximumExecutionTime = new TimeSpan?(TimeSpan.FromSeconds(2.0))
    };
    try
    {
      cloudBlobClient.ListContainersSegmented(new BlobContinuationToken());
    }
    catch (Exception ex)
    {
      return false;
    }
    return true;
  }

  private void btnPlay_Click(object sender, RoutedEventArgs e) => this.MediaElement.Play();

  private void btnPause_Click(object sender, RoutedEventArgs e) => this.MediaElement.Pause();

  private void btnStop_Click(object sender, RoutedEventArgs e) => this.MediaElement.Stop();

  private void btnMute_Click(object sender, RoutedEventArgs e)
  {
    if (this.MediaElement.IsMuted)
      this.MediaElement.IsMuted = false;
    else
      this.MediaElement.IsMuted = true;
  }

  private void btnPopOut_Click(object sender, RoutedEventArgs e)
  {
    this.MediaElement.Stop();
    this.MediaElement.IsEnabled = false;
    this.sliderPanel.IsEnabled = false;
    this.MediaButtonPanel.IsEnabled = false;
    this.mediaProgressBar.Visibility = Visibility.Collapsed;
    this.sliderBar.Visibility = Visibility.Collapsed;
    this.canOpenNewVideos = false;
    PopOutMediaPaneWindow outMediaPaneWindow = new PopOutMediaPaneWindow(this.MediaElement.Source.ToString());
    ElementHost.EnableModelessKeyboardInterop((Window) outMediaPaneWindow);
    outMediaPaneWindow.Show();
    outMediaPaneWindow.Closed += new EventHandler(this.RestartVideo_Player);
  }

  private void btnDownloadVideo_Click(object sender, RoutedEventArgs e)
  {
    string[] strArray = this.MediaElement.Source.ToString().Split('/');
    string fName = strArray[strArray.Length - 1];
    string dlLink = this.MediaElement.Source.ToString();
    if (this.CheckInternetConnection())
    {
      this.btnDownload.IsEnabled = false;
      if (this.CheckforReadOnly("C:\\EDGEforRevit\\Training Videos"))
      {
        int num = (int) MessageBox.Show("Please check folder attributes. It is set to Read Only mode.", "Folder Access Warning", MessageBoxButton.OK, MessageBoxImage.None);
        this.btnDownload.IsEnabled = true;
      }
      else
      {
        try
        {
          this.DownloadVideo(dlLink, fName);
        }
        catch
        {
          int num = (int) MessageBox.Show($"File Failed to Download. Please ensure specified path C:\\EDGEforRevit\\Training Videos\\{fName} is not in use at this time.", "Download failed");
        }
      }
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void DownloadVideo(string dlLink, string fName)
  {
    this.downloadingFileName = fName;
    this.dlTextBlock.Text = $"Downloading {fName} , please wait . . . ";
    this.CancelDownload.Visibility = Visibility.Visible;
    Uri address = new Uri(dlLink);
    try
    {
      this.somethingIsDownloading = true;
      using (WebClient webClient = new WebClient())
      {
        if (!Directory.Exists("C:\\EDGEforRevit\\Training Videos"))
          Directory.CreateDirectory("C:\\EDGEforRevit\\Training Videos");
        try
        {
          webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.WebClient_DownloadDataCompleted);
          webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.WebClient_DownloadDataChanged);
          this.dlTextBlock.Visibility = Visibility.Visible;
          webClient.DownloadFileAsync(address, "C:\\EDGEforRevit\\Training Videos\\" + fName);
          this.cancelDownload = false;
          foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
          {
            if (obj is MenuItem)
              (obj as MenuItem).IsEnabled = false;
          }
          this.currentDownloadClient = webClient;
        }
        catch
        {
          int num = (int) MessageBox.Show($"File Failed to Download. Please ensure specified path C:\\EDGEforRevit\\Training Videos\\{fName} is not in use at this time.", "Download failed");
        }
      }
    }
    catch
    {
      int num = (int) MessageBox.Show("Cannot download video. Please check your internet connection and try again.", "Download Error");
      this.btnDownload.IsEnabled = true;
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      this.somethingIsDownloading = false;
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Text = "";
      this.CancelDownload.Visibility = Visibility.Hidden;
      if (this.listwhensomethingisdownloading.Count <= 0)
        return;
      this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
      foreach (UIElement uiElement in this.listwhensomethingisdownloading)
        uiElement.IsEnabled = true;
    }
  }

  private void WebClient_DownloadDataChanged(object sender, DownloadProgressChangedEventArgs e)
  {
    this.dlTextBlock.Text = $"{this.downloadingFileName} - Download Progress: {Math.Round((double) e.BytesReceived / (double) e.TotalBytesToReceive * 100.0, 0).ToString()}%";
    if (this.aTimer.Enabled)
    {
      this.aTimer.Stop();
      this.aTimer.Start();
    }
    else
      this.aTimer.Start();
    if (!this.cancelDownload)
      return;
    this.cancelDownload = true;
    (sender as WebClient).CancelAsync();
  }

  private void timer_Elapsed2(object sender, ElapsedEventArgs e)
  {
    this.internetLoss = true;
    this.cancelDownload = true;
    if (this.currentDownloadClient != null)
      this.currentDownloadClient.CancelAsync();
    this.aTimer.Stop();
  }

  private void WebClient_DownloadDataCompleted(object sender, AsyncCompletedEventArgs e)
  {
    if (this.aTimer.Enabled)
      this.aTimer.Stop();
    this.cancelDownload = false;
    try
    {
      if (e.Cancelled || e.Error != null)
        throw e.Error;
      int num = (int) MessageBox.Show("Video has been saved to C:\\EDGEforRevit\\Training Videos\\" + this.downloadingFileName, "Video Saved", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Text = "";
      this.btnDownload.IsEnabled = true;
      this.CancelDownload.Visibility = Visibility.Hidden;
      this.somethingIsDownloading = false;
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      if (this.listwhensomethingisdownloading.Count > 0)
      {
        this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
        foreach (UIElement uiElement in this.listwhensomethingisdownloading)
          uiElement.IsEnabled = true;
      }
      this.currentDownloadClient = (WebClient) null;
    }
    catch (Exception ex)
    {
      this.videodelete(this.downloadingFileName);
      if (this.internetLoss || ex.Message.Contains("502"))
      {
        int num1 = (int) MessageBox.Show("Please check your internet connection.", "Lost Internet Connection");
        int num2 = (int) MessageBox.Show($"Loading of {this.downloadingFileName} was canceled.");
        this.internetLoss = false;
      }
      else if (ex.Message.Contains("The request was aborted: The request was canceled.") || ex.Message.Contains("502"))
      {
        int num3 = (int) MessageBox.Show($"Download of {this.downloadingFileName} was canceled.");
      }
      else if (!ex.Message.Contains("WebClient"))
      {
        int num4 = (int) MessageBox.Show(ex.Message);
      }
      this.dlTextBlock.Visibility = Visibility.Collapsed;
      this.dlTextBlock.Text = "";
      this.btnDownload.IsEnabled = true;
      foreach (object obj in (IEnumerable) (this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu).Items)
      {
        if (obj is MenuItem)
          (obj as MenuItem).IsEnabled = true;
      }
      if (this.listwhensomethingisdownloading.Count > 0)
      {
        this.listwhensomethingisdownloading.Distinct<TreeViewItem>();
        foreach (UIElement uiElement in this.listwhensomethingisdownloading)
          uiElement.IsEnabled = true;
      }
      this.CancelDownload.Visibility = Visibility.Hidden;
      this.somethingIsDownloading = false;
    }
  }

  private void videodelete(string filenameToBeDeleted)
  {
    if (!Directory.Exists("C:\\EDGEforRevit\\Training Videos"))
      return;
    try
    {
      foreach (string enumerateFile in Directory.EnumerateFiles("C:\\EDGEforRevit\\Training Videos"))
      {
        if (((IEnumerable<string>) ((IEnumerable<string>) enumerateFile.Split('\\')).Last<string>().Split('.')).First<string>().Equals(((IEnumerable<string>) filenameToBeDeleted.Split('.')).First<string>()) && System.IO.File.Exists(enumerateFile))
          System.IO.File.Delete(enumerateFile);
      }
    }
    catch
    {
      int num = (int) MessageBox.Show("File Failed to Download. Please ensure specified path C:\\EDGEforRevit\\Training Videos\\ is not in use for another download at this time.", "Download failed");
    }
  }

  private void RestartVideo_Player(object sender, EventArgs e)
  {
    this.MediaElement.IsEnabled = true;
    this.sliderPanel.IsEnabled = true;
    this.MediaButtonPanel.IsEnabled = true;
    this.canOpenNewVideos = true;
    this.mediaProgressBar.Visibility = Visibility.Visible;
    this.sliderBar.Visibility = Visibility.Visible;
  }

  private void player_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    this.ContentGrid.Width = e.NewSize.Width * 0.782312925;
    this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
    switch (this.state)
    {
      case WindowState.XPS:
        this.ContentGrid.Width = e.NewSize.Width * 0.782312925;
        this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
        break;
    }
  }

  private void CreateFamilyFolders()
  {
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    List<string> stringList1 = new List<string>();
    CloudBlobDirectory directoryReference = containerReference.GetDirectoryReference("EDGE^Revit Content/EDGE^Revit " + this.year);
    this.prevFilePath = "EDGE^Revit Content/EDGE^Revit " + this.year;
    this.currentFilePath = $"EDGE^Revit Content/EDGE^Revit {this.year}/";
    foreach (IListBlobItem listBlobItem in directoryReference.ListBlobs().Where<IListBlobItem>((System.Func<IListBlobItem, bool>) (b => b is CloudBlobDirectory)).ToList<IListBlobItem>())
      stringList1.Add(listBlobItem.Uri.ToString());
    List<string> stringList2 = new List<string>();
    foreach (string str1 in stringList1)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str1.Split(chArray);
      string str2 = strArray[strArray.Length - 2];
      stringList2.Add(str2);
    }
    DataTable dataTable = new DataTable();
    string columnName = "EDGE^Revit " + this.year;
    dataTable.Columns.Add(new DataColumn(columnName));
    foreach (string str in stringList2)
    {
      stringList2.IndexOf(str);
      DataRow row = dataTable.NewRow();
      row[columnName] = (object) str;
      dataTable.Rows.Add(row);
    }
    dataTable.AcceptChanges();
    this.SublFolderDataGrid.DataContext = (object) dataTable.DefaultView;
  }

  private void CreateSubFolders(string folderName, string inputFilePath)
  {
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    string relativeAddress = inputFilePath;
    CloudBlobDirectory directoryReference = containerReference.GetDirectoryReference(relativeAddress);
    this.prevFilePath = inputFilePath;
    this.currentFilePath = relativeAddress;
    List<IListBlobItem> list = directoryReference.ListBlobs().Where<IListBlobItem>((System.Func<IListBlobItem, bool>) (b => b is CloudBlobDirectory)).ToList<IListBlobItem>();
    IEnumerable<CloudBlockBlob> cloudBlockBlobs = directoryReference.ListBlobs().OfType<CloudBlockBlob>().Where<CloudBlockBlob>((System.Func<CloudBlockBlob, bool>) (b => Path.GetExtension(b.Name).Equals(".rfa")));
    foreach (IListBlobItem listBlobItem in list)
      stringList1.Add(listBlobItem.Uri.ToString());
    foreach (CloudBlockBlob cloudBlockBlob in cloudBlockBlobs)
      stringList2.Add(cloudBlockBlob.Uri.ToString());
    List<string> stringList3 = new List<string>();
    foreach (string str1 in stringList1)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str1.Split(chArray);
      string str2 = strArray[strArray.Length - 2];
      stringList3.Add(str2);
    }
    foreach (string str3 in stringList2)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str3.Split(chArray);
      string str4 = strArray[strArray.Length - 1];
      if (!str4.Equals(""))
        stringList3.Add(str4);
    }
    DataTable dataTable = new DataTable();
    string columnName = "EDGE^Revit " + this.year;
    dataTable.Columns.Add(new DataColumn(columnName));
    foreach (string str in stringList3)
    {
      stringList3.IndexOf(str);
      DataRow row = dataTable.NewRow();
      row[columnName] = (object) str;
      dataTable.Rows.Add(row);
    }
    dataTable.AcceptChanges();
    this.SublFolderDataGrid.DataContext = (object) dataTable.DefaultView;
  }

  private void CreateTrainingFolders()
  {
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    List<string> stringList1 = new List<string>();
    CloudBlobDirectory directoryReference = containerReference.GetDirectoryReference(this.masterTrainingDirectory);
    this.prevTrainingFilePath = this.masterTrainingDirectory;
    this.currentTrainingFilePath = this.masterTrainingDirectory + "/";
    foreach (IListBlobItem listBlobItem in directoryReference.ListBlobs().Where<IListBlobItem>((System.Func<IListBlobItem, bool>) (b => b is CloudBlobDirectory)).ToList<IListBlobItem>())
      stringList1.Add(listBlobItem.Uri.ToString());
    List<string> stringList2 = new List<string>();
    foreach (string str1 in stringList1)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str1.Split(chArray);
      string str2 = strArray[strArray.Length - 2];
      stringList2.Add(str2);
    }
    DataTable dataTable = new DataTable();
    string columnName = "Training Materials";
    dataTable.Columns.Add(new DataColumn(columnName));
    foreach (string str in stringList2)
    {
      stringList2.IndexOf(str);
      DataRow row = dataTable.NewRow();
      row[columnName] = (object) str;
      dataTable.Rows.Add(row);
      TreeViewItem newItem = new TreeViewItem();
      newItem.Header = (object) str;
      this.TrainingTreeView.Items.Add((object) newItem);
    }
    dataTable.AcceptChanges();
    this.TrainingFolderDataGrid.DataContext = (object) dataTable.DefaultView;
  }

  private void CreateTrainingSubFolders(string folderName, string inputFilePath)
  {
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    string relativeAddress = inputFilePath;
    CloudBlobDirectory directoryReference = containerReference.GetDirectoryReference(relativeAddress);
    this.prevTrainingFilePath = inputFilePath;
    this.currentTrainingFilePath = relativeAddress;
    List<IListBlobItem> list = directoryReference.ListBlobs().Where<IListBlobItem>((System.Func<IListBlobItem, bool>) (b => b is CloudBlobDirectory)).ToList<IListBlobItem>();
    IEnumerable<CloudBlockBlob> cloudBlockBlobs = directoryReference.ListBlobs().OfType<CloudBlockBlob>().Where<CloudBlockBlob>((System.Func<CloudBlockBlob, bool>) (b => Path.GetExtension(b.Name).ToUpper().Equals(".XPS") || Path.GetExtension(b.Name).ToUpper().Equals(".MP4")));
    foreach (IListBlobItem listBlobItem in list)
      stringList1.Add(listBlobItem.Uri.ToString());
    foreach (CloudBlockBlob cloudBlockBlob in cloudBlockBlobs)
      stringList2.Add(cloudBlockBlob.Uri.ToString());
    List<string> stringList3 = new List<string>();
    foreach (string str1 in stringList1)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str1.Split(chArray);
      string str2 = strArray[strArray.Length - 2];
      stringList3.Add(str2);
    }
    foreach (string str3 in stringList2)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str3.Split(chArray);
      string str4 = strArray[strArray.Length - 1];
      if (!str4.Equals(""))
        stringList3.Add(str4);
    }
    DataTable dataTable = new DataTable();
    string columnName = "Training Materials";
    dataTable.Columns.Add(new DataColumn(columnName));
    foreach (string str in stringList3)
    {
      stringList3.IndexOf(str);
      DataRow row = dataTable.NewRow();
      row[columnName] = (object) str;
      dataTable.Rows.Add(row);
      TreeViewItem newItem = new TreeViewItem();
      newItem.Header = (object) str;
      (this.TrainingTreeView.SelectedItem as TreeViewItem).Items.Add((object) newItem);
    }
    dataTable.AcceptChanges();
    this.TrainingFolderDataGrid.DataContext = (object) dataTable.DefaultView;
  }

  private void ReturnToPreviousDirectory(string folderName, string inputFilePath)
  {
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    string relativeAddress = inputFilePath;
    CloudBlobDirectory directoryReference = containerReference.GetDirectoryReference(relativeAddress);
    CloudBlobDirectory parent = directoryReference.Parent;
    this.prevFilePath = relativeAddress;
    this.currentFilePath = parent == null ? directoryReference.Prefix : directoryReference.Parent.Prefix;
    List<IListBlobItem> list = directoryReference.Parent.ListBlobs().Where<IListBlobItem>((System.Func<IListBlobItem, bool>) (b => b is CloudBlobDirectory)).ToList<IListBlobItem>();
    IEnumerable<CloudBlockBlob> cloudBlockBlobs = directoryReference.Parent.ListBlobs().OfType<CloudBlockBlob>().Where<CloudBlockBlob>((System.Func<CloudBlockBlob, bool>) (b => Path.GetExtension(b.Name).Equals(".rfa")));
    foreach (IListBlobItem listBlobItem in list)
      stringList1.Add(listBlobItem.Uri.ToString());
    foreach (CloudBlockBlob cloudBlockBlob in cloudBlockBlobs)
      stringList2.Add(cloudBlockBlob.Uri.ToString());
    List<string> stringList3 = new List<string>();
    foreach (string str1 in stringList1)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str1.Split(chArray);
      string str2 = strArray[strArray.Length - 2];
      stringList3.Add(str2);
    }
    foreach (string str3 in stringList2)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str3.Split(chArray);
      string str4 = strArray[strArray.Length - 1];
      if (!str4.Equals(""))
        stringList3.Add(str4);
    }
    DataTable dataTable = new DataTable();
    string columnName = "EDGE^Revit " + this.year;
    dataTable.Columns.Add(new DataColumn(columnName));
    foreach (string str in stringList3)
    {
      stringList3.IndexOf(str);
      DataRow row = dataTable.NewRow();
      row[columnName] = (object) str;
      dataTable.Rows.Add(row);
    }
    dataTable.AcceptChanges();
    this.SublFolderDataGrid.DataContext = (object) dataTable.DefaultView;
  }

  private void ReturnToPreviousDirectoryTraining(string folderName, string inputFilePath)
  {
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    string relativeAddress = inputFilePath;
    CloudBlobDirectory directoryReference = containerReference.GetDirectoryReference(relativeAddress);
    CloudBlobDirectory parent = directoryReference.Parent;
    this.prevTrainingFilePath = relativeAddress;
    this.currentTrainingFilePath = parent == null ? directoryReference.Prefix : directoryReference.Parent.Prefix;
    List<IListBlobItem> list = directoryReference.Parent.ListBlobs().Where<IListBlobItem>((System.Func<IListBlobItem, bool>) (b => b is CloudBlobDirectory)).ToList<IListBlobItem>();
    IEnumerable<CloudBlockBlob> cloudBlockBlobs = directoryReference.Parent.ListBlobs().OfType<CloudBlockBlob>().Where<CloudBlockBlob>((System.Func<CloudBlockBlob, bool>) (b => Path.GetExtension(b.Name).ToUpper().Equals(".XPS") || Path.GetExtension(b.Name).ToUpper().Equals(".MP4")));
    foreach (IListBlobItem listBlobItem in list)
      stringList1.Add(listBlobItem.Uri.ToString());
    foreach (CloudBlockBlob cloudBlockBlob in cloudBlockBlobs)
      stringList2.Add(cloudBlockBlob.Uri.ToString());
    List<string> stringList3 = new List<string>();
    foreach (string str1 in stringList1)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str1.Split(chArray);
      string str2 = strArray[strArray.Length - 2];
      stringList3.Add(str2);
    }
    foreach (string str3 in stringList2)
    {
      char[] chArray = new char[1]{ '/' };
      string[] strArray = str3.Split(chArray);
      string str4 = strArray[strArray.Length - 1];
      if (!str4.Equals(""))
        stringList3.Add(str4);
    }
    DataTable dataTable = new DataTable();
    string columnName = "Training Materials";
    dataTable.Columns.Add(new DataColumn(columnName));
    foreach (string str in stringList3)
    {
      stringList3.IndexOf(str);
      DataRow row = dataTable.NewRow();
      row[columnName] = (object) str;
      dataTable.Rows.Add(row);
    }
    dataTable.AcceptChanges();
    this.TrainingFolderDataGrid.DataContext = (object) dataTable.DefaultView;
  }

  private void DisplayVideo(string folderName, string filePath)
  {
    if (this.CheckInternetConnection())
    {
      this.state = WindowState.MP4;
      CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
      containerReference.CreateIfNotExists();
      containerReference.GetBlockBlobReference("edgeptacblob");
      containerReference.SetPermissions(new BlobContainerPermissions()
      {
        PublicAccess = BlobContainerPublicAccessType.Blob
      });
      this.videoQualityPaths = new List<string>();
      string[] strArray = filePath.Split('_');
      switch (strArray[strArray.Length - 1])
      {
        case "480p.mp4":
          this.qualityBox.SelectedIndex = 0;
          break;
        case "720p.mp4":
          this.qualityBox.SelectedIndex = 1;
          break;
        case "1080p.mp4":
          this.qualityBox.SelectedIndex = 2;
          break;
      }
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < strArray.Length - 1; ++index)
        stringBuilder.Append(strArray[index]);
      this.videoQualityPaths.Add(stringBuilder.ToString() + "_480p.mp4");
      this.videoQualityPaths.Add(stringBuilder.ToString() + "_720p.mp4");
      this.videoQualityPaths.Add(stringBuilder.ToString() + "_1080p.mp4");
      this.MediaElement.Source = new Uri(containerReference.GetBlockBlobReference(filePath).StorageUri.PrimaryUri.ToString());
      this.MediaElement.Visibility = Visibility.Visible;
      this.MediaElement.LoadedBehavior = MediaState.Manual;
      this.MediaElement.Play();
      this.MediaButtonPanel.Visibility = Visibility.Visible;
      this.MediaButtonPanel.IsEnabled = true;
      this.MediaBackButton.Visibility = Visibility.Visible;
      this.sliderPanel.Visibility = Visibility.Visible;
      this.sliderPanel.IsEnabled = true;
      this.TotalTime.Visibility = Visibility.Visible;
      this.CurrentTime.Visibility = Visibility.Visible;
      this.MediaBackButton.IsEnabled = true;
      this.TrainingTreeView.Visibility = Visibility.Collapsed;
      this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
      this.SearchTreeView.Visibility = Visibility.Collapsed;
      this.FindBarValueHolder.Visibility = Visibility.Collapsed;
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void QualityBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (!(sender as ComboBox).IsDropDownOpen || this.qualityBox.Text.ToString().Equals(""))
      return;
    string str = "";
    if (this.qualityBox.SelectedIndex == 0)
      str = "480p";
    else if (this.qualityBox.SelectedIndex == 1)
      str = "720p";
    else if (this.qualityBox.SelectedIndex == 2)
      str = "1080p";
    string blobName = "";
    foreach (string videoQualityPath in this.videoQualityPaths)
    {
      if (videoQualityPath.Contains(str))
        blobName = videoQualityPath;
    }
    this.qualityBox.IsDropDownOpen = false;
    CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
    containerReference.CreateIfNotExists();
    containerReference.GetBlockBlobReference("edgeptacblob");
    containerReference.SetPermissions(new BlobContainerPermissions()
    {
      PublicAccess = BlobContainerPublicAccessType.Blob
    });
    this.MediaElement.Source = new Uri(containerReference.GetBlockBlobReference(blobName).StorageUri.PrimaryUri.ToString());
    this.MediaElement.Visibility = Visibility.Visible;
    this.MediaElement.LoadedBehavior = MediaState.Manual;
    this.MediaElement.Play();
    this.MediaButtonPanel.Visibility = Visibility.Visible;
    this.MediaButtonPanel.IsEnabled = true;
    this.MediaBackButton.Visibility = Visibility.Visible;
    this.sliderPanel.Visibility = Visibility.Visible;
    this.sliderPanel.IsEnabled = true;
    this.TotalTime.Visibility = Visibility.Visible;
    this.CurrentTime.Visibility = Visibility.Visible;
    this.MediaBackButton.IsEnabled = true;
    this.TrainingTreeView.Visibility = Visibility.Collapsed;
    this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
    string[] strArray = blobName.Split('_');
    switch (strArray[strArray.Length - 1])
    {
      case "480p.mp4":
        this.qualityBox.SelectedIndex = 0;
        break;
      case "720p.mp4":
        this.qualityBox.SelectedIndex = 1;
        break;
      case "1080p.mp4":
        this.qualityBox.SelectedIndex = 2;
        break;
    }
  }

  private void DisplayDocument(string folderName, string filePath, bool isTrainManual = false)
  {
    if (this.CheckInternetConnection())
    {
      CloudBlobContainer containerReference = CloudStorageAccount.Parse(this.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
      containerReference.CreateIfNotExists();
      containerReference.GetBlockBlobReference("edgeptacblob");
      containerReference.SetPermissions(new BlobContainerPermissions()
      {
        PublicAccess = BlobContainerPublicAccessType.Blob
      });
      CloudBlockBlob blockBlobReference = containerReference.GetBlockBlobReference(filePath);
      string str = blockBlobReference.StorageUri.PrimaryUri.ToString();
      Stream stream = blockBlobReference.OpenRead((AccessCondition) null, (BlobRequestOptions) null, (OperationContext) null);
      Uri uri = new Uri(str);
      Package package = Package.Open(stream);
      this.state = WindowState.XPS;
      if (isTrainManual)
      {
        this.HomeButton.Visibility = Visibility.Visible;
        this.HomeButton.IsEnabled = true;
      }
      else
      {
        this.XPSBackButton.Visibility = Visibility.Visible;
        this.XPSBackButton.IsEnabled = true;
        this.TrainingTreeView.Visibility = Visibility.Collapsed;
        this.SearchTreeView.Visibility = Visibility.Collapsed;
        this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
      }
      this.xps.Visibility = Visibility.Visible;
      this.xps.Zoom = 75.0;
      this.xps.IsEnabled = true;
      PackageStore.RemovePackage(uri);
      PackageStore.AddPackage(uri, package);
      this.xpsDocument = new XpsDocument(package, CompressionOption.SuperFast, str);
      this.xps.Document = (IDocumentPaginatorSource) this.xpsDocument.GetFixedDocumentSequence();
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      this.contentdisplayed = false;
      this.LoadedTreeViews = false;
      this.goHome();
    }
  }

  private void MainPaneWindow_SizeChanged(object sender, SizeChangedEventArgs e)
  {
    switch (this.state)
    {
      case WindowState.XPS:
        this.ContentGrid.Width = e.NewSize.Width * 0.782312925;
        this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
        break;
      case WindowState.MP4:
        this.ContentGrid.Width = e.NewSize.Width * 0.782312925;
        this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
        break;
      case WindowState.Main:
        double width1 = e.NewSize.Width;
        Size newSize1 = e.NewSize;
        double num1 = newSize1.Height * 1.8;
        if (width1 > num1)
        {
          Grid contentGrid = this.ContentGrid;
          newSize1 = e.NewSize;
          double num2 = newSize1.Height * 0.8;
          contentGrid.Height = num2;
          this.ContentGrid.Width = this.ContentGrid.Height / (9.0 / 16.0);
          break;
        }
        Grid contentGrid1 = this.ContentGrid;
        newSize1 = e.NewSize;
        double num3 = newSize1.Width * 0.8;
        contentGrid1.Width = num3;
        this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
        break;
      case WindowState.TreeView:
        this.ContentGrid.Width = e.NewSize.Width * 0.782312925;
        this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
        break;
      default:
        double width2 = e.NewSize.Width;
        Size newSize2 = e.NewSize;
        double num4 = newSize2.Height * 1.8;
        if (width2 > num4)
        {
          Grid contentGrid2 = this.ContentGrid;
          newSize2 = e.NewSize;
          double num5 = newSize2.Height * 0.8;
          contentGrid2.Height = num5;
          this.ContentGrid.Width = this.ContentGrid.Height / (9.0 / 16.0);
          break;
        }
        Grid contentGrid3 = this.ContentGrid;
        newSize2 = e.NewSize;
        double num6 = newSize2.Width * 0.8;
        contentGrid3.Width = num6;
        this.ContentGrid.Height = this.ContentGrid.Width * (9.0 / 16.0);
        break;
    }
  }

  private void PopulateTrainingTree(CloudBlobContainer container)
  {
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    CloudBlobDirectory directoryReference = container.GetDirectoryReference(this.masterTrainingDirectory);
    this.prevTrainingFilePath = this.masterTrainingDirectory;
    this.currentTrainingFilePath = this.masterTrainingDirectory + "/";
    directoryReference.ListBlobs(true).OfType<CloudBlockBlob>().Where<CloudBlockBlob>((System.Func<CloudBlockBlob, bool>) (b => Path.GetExtension(b.Name).ToUpper().Equals(".XPS") || Path.GetExtension(b.Name).ToUpper().Equals(".MP4")));
    IEnumerable<IListBlobItem> list = (IEnumerable<IListBlobItem>) container.ListBlobs(useFlatBlobListing: true).ToList<IListBlobItem>();
    List<string> blobNamesManuals = list.Select<IListBlobItem, string>((System.Func<IListBlobItem, string>) (blob => (blob as CloudBlockBlob).Name)).Where<string>((System.Func<string, bool>) (n => n.ToString().ToUpper().Contains(".XPS"))).ToList<string>();
    List<string> blobNamesVideos = list.Select<IListBlobItem, string>((System.Func<IListBlobItem, string>) (blob => (blob as CloudBlockBlob).Name)).Where<string>((System.Func<string, bool>) (n => n.ToString().ToUpper().Contains(".MP4"))).ToList<string>();
    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Delegate) (() =>
    {
      List<string[]> strArrayList1 = new List<string[]>();
      foreach (string str1 in blobNamesManuals)
      {
        string[] pArray = str1.Replace("EDGE^Revit Content/Training/", "").Split('/');
        strArrayList1.Add(pArray);
        this.trainingFilePaths.Add(new Tuple<string, string>(str1, pArray[pArray.Length - 1]));
        string str2 = pArray[pArray.Length - 1].Split('.')[0].Replace("_", " ");
        this.documentFriendlyNames.Add(new Tuple<string, string>(pArray[pArray.Length - 1], str2));
        if (pArray.Length == 1)
        {
          TreeViewItem newItem = new TreeViewItem();
          newItem.Header = (object) pArray[0];
          string pHead = "";
          foreach (Tuple<string, string> documentFriendlyName in this.documentFriendlyNames)
          {
            if (pArray[0].Equals(documentFriendlyName.Item1))
              pHead = documentFriendlyName.Item2.ToString();
          }
          if (!this.TrainingTreeView.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pHead)))
          {
            newItem.Selected += new RoutedEventHandler(this.TrainingTreeView_Click);
            newItem.Header = (object) pHead;
            newItem.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
            this.TrainingTreeView.Items.Add((object) newItem);
          }
        }
        else
        {
          TreeViewItem treeViewItem1 = new TreeViewItem();
          treeViewItem1.Header = (object) pArray[0];
          if (!this.TrainingTreeView.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[0])))
          {
            this.TrainingTreeView.Items.Add((object) treeViewItem1);
          }
          else
          {
            foreach (TreeViewItem treeViewItem2 in (IEnumerable) this.TrainingTreeView.Items)
            {
              if (treeViewItem2.Header.Equals(treeViewItem1.Header))
              {
                treeViewItem1 = treeViewItem2;
                break;
              }
            }
          }
          this.AddChildrenToTree("Manual", treeViewItem1, pArray, 1, pArray.Length);
        }
      }
      List<string[]> strArrayList2 = new List<string[]>();
      foreach (string str3 in blobNamesVideos)
      {
        string[] pArray = str3.Replace("EDGE^Revit Content/Training/", "").Split('/');
        strArrayList2.Add(pArray);
        this.trainingFilePaths.Add(new Tuple<string, string>(str3, pArray[pArray.Length - 1]));
        string str4 = pArray[pArray.Length - 1].Split('.')[0].Replace("_", " ").Replace("480p", "").Replace("720p", "").Replace("1080p", "");
        this.videoFriendlyNames.Add(new Tuple<string, string>(pArray[pArray.Length - 1], str4));
        if (pArray.Length == 1)
        {
          TreeViewItem newItem = new TreeViewItem();
          newItem.Header = (object) pArray[0];
          string pHead = "";
          foreach (Tuple<string, string> videoFriendlyName in this.videoFriendlyNames)
          {
            if (pArray[0].Equals(videoFriendlyName.Item1))
              pHead = videoFriendlyName.Item2.ToString();
          }
          if (!this.TrainingVideosTreeView.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pHead)))
          {
            newItem.Selected += new RoutedEventHandler(this.TrainingVideoTreeView_Click);
            newItem.Header = (object) pHead;
            newItem.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
            this.TrainingVideosTreeView.Items.Add((object) newItem);
          }
        }
        else
        {
          TreeViewItem treeViewItem3 = new TreeViewItem();
          treeViewItem3.Header = (object) pArray[0];
          if (!this.TrainingVideosTreeView.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[0])))
          {
            this.TrainingVideosTreeView.Items.Add((object) treeViewItem3);
          }
          else
          {
            foreach (TreeViewItem treeViewItem4 in (IEnumerable) this.TrainingVideosTreeView.Items)
            {
              if (treeViewItem4.Header.Equals(treeViewItem3.Header))
              {
                treeViewItem3 = treeViewItem4;
                break;
              }
            }
          }
          this.AddChildrenToTree("Video", treeViewItem3, pArray, 1, pArray.Length);
        }
      }
    }));
  }

  private void ColorChange_IsMouseDirectlyOverChanged(
    object sender,
    DependencyPropertyChangedEventArgs e)
  {
    if (!(sender is TreeViewItem))
      return;
    TreeViewItem treeViewItem = sender as TreeViewItem;
    if (treeViewItem.IsMouseOver)
      treeViewItem.Background = (Brush) Brushes.LightCyan;
    else
      treeViewItem.Background = (Brush) Brushes.Transparent;
  }

  private void AddChildrenToTree(
    string treeType,
    TreeViewItem parent,
    string[] pArray,
    int currentIndex,
    int length)
  {
    TreeViewItem treeViewItem1 = new TreeViewItem();
    treeViewItem1.Header = (object) pArray[currentIndex];
    string cHead = "";
    if (!parent.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[currentIndex])))
    {
      if (currentIndex == length - 1)
      {
        switch (treeType)
        {
          case "Manual":
            treeViewItem1.Selected += new RoutedEventHandler(this.TrainingTreeView_Click);
            treeViewItem1.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
            treeViewItem1.Cursor = Cursors.Hand;
            using (List<Tuple<string, string>>.Enumerator enumerator = this.documentFriendlyNames.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                Tuple<string, string> current = enumerator.Current;
                if (pArray[currentIndex].Equals(current.Item1))
                {
                  cHead = current.Item2.ToString();
                  treeViewItem1.Header = (object) current.Item2.ToString();
                  break;
                }
              }
              break;
            }
          case "Video":
            treeViewItem1.Selected += new RoutedEventHandler(this.TrainingVideoTreeView_Click);
            using (List<Tuple<string, string>>.Enumerator enumerator = this.videoFriendlyNames.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                Tuple<string, string> current = enumerator.Current;
                if (pArray[currentIndex].Equals(current.Item1))
                {
                  cHead = current.Item2.ToString();
                  treeViewItem1.Header = (object) current.Item2.ToString();
                  treeViewItem1.Cursor = Cursors.Hand;
                  break;
                }
              }
              break;
            }
          case "Family":
            treeViewItem1.ContextMenu = this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu;
            treeViewItem1.Selected += new RoutedEventHandler(this.FamilyTreeViewSetSelected_Click);
            treeViewItem1.MouseUp += new MouseButtonEventHandler(this.FamilyTreeViewSet_MouseUp);
            using (List<Tuple<string, string>>.Enumerator enumerator = this.familyFriendlyNames.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                Tuple<string, string> current = enumerator.Current;
                if (pArray[currentIndex].Equals(current.Item1))
                {
                  cHead = current.Item2.ToString();
                  treeViewItem1.Header = (object) current.Item2.ToString();
                  treeViewItem1.Cursor = Cursors.Hand;
                  break;
                }
              }
              break;
            }
        }
      }
      if (!parent.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == cHead)))
        parent.Items.Add((object) treeViewItem1);
    }
    else
    {
      foreach (TreeViewItem treeViewItem2 in (IEnumerable) parent.Items)
      {
        if (treeViewItem2.Header.Equals(treeViewItem1.Header))
        {
          treeViewItem1 = treeViewItem2;
          break;
        }
      }
    }
    if (currentIndex == length - 1)
      return;
    this.AddChildrenToTree(treeType, treeViewItem1, pArray, currentIndex + 1, length);
  }

  private void PopulateFamilyTree(CloudBlobContainer container)
  {
    List<string> stringList = new List<string>();
    container.GetDirectoryReference("EDGE^Revit Content/EDGE^R " + this.year);
    this.prevFilePath = "EDGE^Revit Content/EDGE^Revit " + this.year;
    this.currentFilePath = $"EDGE^Revit Content/EDGE^Revit {this.year}/";
    IEnumerable<IListBlobItem> list = (IEnumerable<IListBlobItem>) container.ListBlobs(useFlatBlobListing: true).ToList<IListBlobItem>();
    List<string> blobNamesFamilies = list.Select<IListBlobItem, string>((System.Func<IListBlobItem, string>) (blob => (blob as CloudBlockBlob).Name)).Where<string>((System.Func<string, bool>) (n => n.ToString().ToUpper().Contains(".RFA") && n.ToString().ToUpper().Contains(this.year))).ToList<string>();
    List<string> xlxsFamilies = list.Select<IListBlobItem, string>((System.Func<IListBlobItem, string>) (blob => (blob as CloudBlockBlob).Name)).Where<string>((System.Func<string, bool>) (n => n.ToString().ToUpper().Contains(".XLSX") && n.ToString().ToUpper().Contains(this.year))).ToList<string>();
    List<string> txtFamilies = list.Select<IListBlobItem, string>((System.Func<IListBlobItem, string>) (blob => (blob as CloudBlockBlob).Name)).Where<string>((System.Func<string, bool>) (n => n.ToString().ToUpper().Contains(".TXT") && n.ToString().ToUpper().Contains(this.year))).ToList<string>();
    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Delegate) (() =>
    {
      foreach (string str in xlxsFamilies)
      {
        List<string[]> strArrayList = new List<string[]>();
        string[] strArray = str.Replace($"EDGE^Revit Content/EDGE^R {this.year}/", "").Split('/');
        strArrayList.Add(strArray);
        this.xlsxFamilyPaths.Add(new Tuple<string, string>(str, strArray[strArray.Length - 1]));
      }
      foreach (string str in txtFamilies)
      {
        List<string[]> strArrayList = new List<string[]>();
        string[] strArray = str.Replace($"EDGE^Revit Content/EDGE^R {this.year}/", "").Split('/');
        strArrayList.Add(strArray);
        this.txtFamilyPaths.Add(new Tuple<string, string>(str, strArray[strArray.Length - 1]));
      }
      List<string[]> strArrayList1 = new List<string[]>();
      foreach (string str1 in blobNamesFamilies)
      {
        string[] pArray = str1.Replace($"EDGE^Revit Content/EDGE^R {this.year}/", "").Split('/');
        strArrayList1.Add(pArray);
        this.familyFilePaths.Add(new Tuple<string, string>(str1, pArray[pArray.Length - 1]));
        string str2 = pArray[pArray.Length - 1].Split('.')[0];
        this.familyFriendlyNames.Add(new Tuple<string, string>(pArray[pArray.Length - 1], str2));
        if (pArray.Length == 1)
        {
          TreeViewItem newItem = new TreeViewItem();
          newItem.Header = (object) pArray[0];
          string pHead = "";
          foreach (Tuple<string, string> familyFriendlyName in this.familyFriendlyNames)
          {
            if (pArray[0].Equals(familyFriendlyName.Item1))
              pHead = familyFriendlyName.Item2.ToString();
          }
          if (!this.FamilyTreeView.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pHead)))
          {
            newItem.ContextMenu = this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu;
            newItem.Selected += new RoutedEventHandler(this.FamilyTreeViewSetSelected_Click);
            newItem.MouseUp += new MouseButtonEventHandler(this.FamilyTreeViewSet_MouseUp);
            newItem.Header = (object) pHead;
            this.FamilyTreeView.Items.Add((object) newItem);
          }
        }
        else
        {
          TreeViewItem treeViewItem1 = new TreeViewItem();
          treeViewItem1.Header = (object) pArray[0];
          if (!this.FamilyTreeView.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[0])))
          {
            this.FamilyTreeView.Items.Add((object) treeViewItem1);
          }
          else
          {
            foreach (TreeViewItem treeViewItem2 in (IEnumerable) this.FamilyTreeView.Items)
            {
              if (treeViewItem2.Header.Equals(treeViewItem1.Header))
              {
                treeViewItem1 = treeViewItem2;
                break;
              }
            }
          }
          this.AddChildrenToTree("Family", treeViewItem1, pArray, 1, pArray.Length);
        }
      }
      this.LoadedTreeViews = true;
    }));
  }

  private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
  {
    if (this.MediaElement.NaturalDuration.HasTimeSpan)
    {
      TimeSpan timeSpan = this.MediaElement.NaturalDuration.TimeSpan;
      this.sliderBar.Maximum = timeSpan.TotalSeconds;
      this.sliderBar.SmallChange = 1.0;
      this.sliderBar.LargeChange = (double) Math.Min(10, timeSpan.Seconds / 10);
      this.TotalTime.Text = this.MediaElement.NaturalDuration.TimeSpan.ToString("mm\\:ss");
    }
    this.timer.Start();
  }

  private void MediaElement_MediaEnded()
  {
    this.isDragging = false;
    this.timer.Stop();
    this.timer = new DispatcherTimer();
    this.timer.Interval = TimeSpan.FromMilliseconds(1000.0);
    this.timer.Tick += new EventHandler(this.timer_Tick);
  }

  private void timer_Tick(object sender, EventArgs e)
  {
    if (this.isDragging)
      return;
    this.sliderBar.Value = this.MediaElement.Position.TotalSeconds;
    this.mediaProgressBar.Minimum = 0.0;
    this.mediaProgressBar.Maximum = 1.0;
    this.mediaProgressBar.Value = this.MediaElement.DownloadProgress;
    this.CurrentTime.Text = this.MediaElement.Position.ToString("mm\\:ss");
  }

  private void sliderBar_DragStarted(object sender, DragStartedEventArgs e)
  {
    this.isDragging = true;
  }

  private void sliderBar_DragCompleted(object sender, DragCompletedEventArgs e)
  {
    this.isDragging = false;
    this.MediaElement.Position = TimeSpan.FromSeconds(this.sliderBar.Value);
  }

  private void previewMouseWheel(object sender, MouseWheelEventArgs e)
  {
    this.SearchScrollViewer.ScrollToVerticalOffset(this.SearchScrollViewer.VerticalOffset - (double) e.Delta);
    e.Handled = true;
  }

  private void OnKeyHandler(object sender, KeyEventArgs e)
  {
    if (e.Key != Key.Return)
      return;
    this.searchresultstatus = true;
    this.HomeButton.Visibility = Visibility.Visible;
    this.HomeButton.IsEnabled = true;
    this.SearchScrollViewer.ScrollToTop();
    this.SearchScrollViewer.ScrollToLeftEnd();
    this.SearchTreeView.Visibility = Visibility.Visible;
    this.SearchScrollViewer.Visibility = Visibility.Visible;
    this.MainIcon.Visibility = Visibility.Visible;
    this.MainIcon.IsEnabled = true;
    this.FamilyExpander.Visibility = Visibility.Collapsed;
    this.FamilyExpander.IsEnabled = false;
    this.ContactTextBlock.Visibility = Visibility.Collapsed;
    this.TrainingExpander.Visibility = Visibility.Collapsed;
    this.TrainingExpander.IsEnabled = false;
    this.ContactButton.Visibility = Visibility.Collapsed;
    this.ContactButton.IsEnabled = false;
    this.ManualsButton.Visibility = Visibility.Collapsed;
    this.VideosButton.Visibility = Visibility.Collapsed;
    this.TrainingTreeView.Visibility = Visibility.Collapsed;
    this.TrainingVideosTreeView.Visibility = Visibility.Collapsed;
    this.TrainingScrollViewer.Visibility = Visibility.Collapsed;
    this.TrainingBackButton.Visibility = Visibility.Collapsed;
    this.FamilyTreeView.Visibility = Visibility.Collapsed;
    this.FamilyScrollViewer.Visibility = Visibility.Collapsed;
    this.FamilyScrollViewer.IsEnabled = false;
    this.FindBarValueHolder.Visibility = Visibility.Visible;
    this.SearchTreeView.Items.Clear();
    string searchvalue = this.FindBar.Text.Trim();
    Dictionary<string, string> dictionary1 = new Dictionary<string, string>();
    if (string.IsNullOrEmpty(searchvalue))
    {
      this.FindBarValueHolder.Text = "There are no results.";
      this.SearchTreeView.Visibility = Visibility.Hidden;
    }
    else
    {
      Dictionary<string, string> dictionary2 = new Search().SearchResult(this.trainingFilePaths, this.familyFilePaths, this.xlsxFamilyPaths, this.txtFamilyPaths, searchvalue);
      TreeViewItem treeViewItem1 = new TreeViewItem();
      treeViewItem1.Header = (object) "Training";
      TreeViewItem treeViewItem2 = new TreeViewItem();
      treeViewItem2.Header = (object) "Videos";
      TreeViewItem treeViewItem3 = new TreeViewItem();
      treeViewItem3.Header = (object) "Families";
      this.SearchTreeView.Items.Add((object) treeViewItem1);
      this.SearchTreeView.Items.Add((object) treeViewItem2);
      this.SearchTreeView.Items.Add((object) treeViewItem3);
      this.FindBarValueHolder.Text = $"The search results for \"{searchvalue}\" are:";
      foreach (KeyValuePair<string, string> keyValuePair in dictionary2)
      {
        KeyValuePair<string, string> str = keyValuePair;
        string str1 = str.Value.Split('/')[1].Replace("EDGE^R ", "");
        if (str.Key.Contains(".xps"))
        {
          string[] strArray1 = str.Value.Replace("EDGE^Revit Content/Training/", "").Split('/');
          foreach (string[] strArray2 in new List<string[]>()
          {
            strArray1
          })
          {
            string[] pArray = strArray2;
            if (pArray.Length == 1)
            {
              TreeViewItem newItem = new TreeViewItem();
              newItem.Header = (object) pArray[0];
              string pHead = "";
              foreach (Tuple<string, string> documentFriendlyName in this.documentFriendlyNames)
              {
                if (str.Key.Equals(documentFriendlyName.Item1))
                  pHead = documentFriendlyName.Item2.ToString();
              }
              if (!treeViewItem1.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pHead)))
              {
                newItem.Selected += new RoutedEventHandler(this.TrainingTreeView_Click);
                newItem.Header = (object) pHead;
                newItem.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
                treeViewItem1.Items.Add((object) newItem);
              }
            }
            else
            {
              TreeViewItem treeViewItem4 = new TreeViewItem();
              treeViewItem4.Header = (object) pArray[0];
              if (!treeViewItem1.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[0])))
              {
                treeViewItem1.Items.Add((object) treeViewItem4);
              }
              else
              {
                foreach (TreeViewItem treeViewItem5 in (IEnumerable) treeViewItem1.Items)
                {
                  if (treeViewItem5.Header.Equals(treeViewItem4.Header))
                  {
                    treeViewItem4 = treeViewItem5;
                    break;
                  }
                }
              }
              this.AddChildrenToTree("Manual", treeViewItem4, pArray, 1, pArray.Length);
            }
          }
        }
        else if (str.Key.Contains(".mp4"))
        {
          string[] strArray3 = str.Value.Replace("EDGE^Revit Content/Training/", "").Split('/');
          foreach (string[] strArray4 in new List<string[]>()
          {
            strArray3
          })
          {
            string[] pArray = strArray4;
            if (pArray.Length == 1)
            {
              TreeViewItem newItem = new TreeViewItem();
              newItem.Header = (object) pArray[0];
              string pHead = "";
              foreach (Tuple<string, string> videoFriendlyName in this.videoFriendlyNames)
              {
                if (str.Key.Equals(videoFriendlyName.Item1))
                  pHead = videoFriendlyName.Item2.ToString();
              }
              if (!treeViewItem2.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pHead)))
              {
                newItem.Selected += new RoutedEventHandler(this.TrainingVideoTreeView_Click);
                newItem.Header = (object) pHead;
                newItem.IsMouseDirectlyOverChanged += new DependencyPropertyChangedEventHandler(this.ColorChange_IsMouseDirectlyOverChanged);
                treeViewItem2.Items.Add((object) newItem);
              }
            }
            else
            {
              TreeViewItem treeViewItem6 = new TreeViewItem();
              treeViewItem6.Header = (object) pArray[0];
              if (!treeViewItem2.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[0])))
              {
                treeViewItem2.Items.Add((object) treeViewItem6);
              }
              else
              {
                foreach (TreeViewItem treeViewItem7 in (IEnumerable) treeViewItem2.Items)
                {
                  if (treeViewItem7.Header.Equals(treeViewItem6.Header))
                  {
                    treeViewItem6 = treeViewItem7;
                    break;
                  }
                }
              }
              this.AddChildrenToTree("Video", treeViewItem6, pArray, 1, pArray.Length);
            }
          }
        }
        else if (str.Key.Contains(".rfa"))
        {
          string[] strArray5 = str.Value.Replace($"EDGE^Revit Content/EDGE^R {str1}/", "").Split('/');
          foreach (string[] strArray6 in new List<string[]>()
          {
            strArray5
          })
          {
            string[] pArray = strArray6;
            if (pArray.Length == 1)
            {
              TreeViewItem newItem = new TreeViewItem();
              newItem.Header = (object) pArray[0];
              if (treeViewItem3.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == str.Key)))
              {
                newItem.ContextMenu = this.FamilyTreeView.Resources[(object) "DownloadContext"] as ContextMenu;
                newItem.Selected += new RoutedEventHandler(this.FamilyTreeViewSetSelected_Click);
                newItem.MouseUp += new MouseButtonEventHandler(this.FamilyTreeViewSet_MouseUp);
                treeViewItem3.Items.Add((object) newItem);
              }
            }
            else
            {
              TreeViewItem treeViewItem8 = new TreeViewItem();
              treeViewItem8.Header = (object) pArray[0];
              if (!treeViewItem3.Items.Cast<TreeViewItem>().Any<TreeViewItem>((System.Func<TreeViewItem, bool>) (item => item.Header.ToString() == pArray[0])))
              {
                treeViewItem3.Items.Add((object) treeViewItem8);
              }
              else
              {
                foreach (TreeViewItem treeViewItem9 in (IEnumerable) treeViewItem3.Items)
                {
                  if (treeViewItem9.Header.Equals(treeViewItem8.Header))
                  {
                    treeViewItem8 = treeViewItem9;
                    break;
                  }
                }
              }
              this.AddChildrenToTree("Family", treeViewItem8, pArray, 1, pArray.Length);
            }
          }
        }
      }
      this.ExpandNodes(treeViewItem3);
      this.ExpandNodes(treeViewItem2);
      this.ExpandNodes(treeViewItem1);
      if (treeViewItem1.Items.Count == 0 && treeViewItem2.Items.Count == 0 && treeViewItem3.Items.Count == 0)
      {
        this.FindBarValueHolder.Text = $"The are no search results for \"{searchvalue}\" ";
        this.SearchTreeView.Visibility = Visibility.Hidden;
      }
      if (treeViewItem1.Items.Count == 0)
        this.SearchTreeView.Items.Remove((object) treeViewItem1);
      if (treeViewItem2.Items.Count == 0)
        this.SearchTreeView.Items.Remove((object) treeViewItem2);
      if (treeViewItem3.Items.Count != 0)
        return;
      this.SearchTreeView.Items.Remove((object) treeViewItem3);
    }
  }

  private void ExpandNodes(TreeViewItem root)
  {
    root.IsExpanded = true;
    foreach (TreeViewItem root1 in root.Items.OfType<TreeViewItem>())
      this.ExpandNodes(root1);
  }

  private void FindBar_GotFocus(object sender, RoutedEventArgs e) => this.FindBar.Text = "";

  private void FindBar_LostFocus(object sender, RoutedEventArgs e) => this.FindBar.Text = "Search";

  private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
  {
    Process.Start(e.Uri.AbsoluteUri);
    e.Handled = true;
  }

  private async void getInfoListasync(CloudBlobContainer container)
  {
    this.infolist = new Search().getList(container);
  }

  private async void RefreshAsync()
  {
    PaneWindow paneWindow1 = this;
    // ISSUE: reference to a compiler-generated method
    paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshAsync\u003Eb__134_0));
    // ISSUE: reference to a compiler-generated method
    if (await Task.Run<bool>(new Func<Task<bool>>(paneWindow1.\u003CRefreshAsync\u003Eb__134_2)))
    {
      PaneWindow paneWindow = paneWindow1;
      CloudBlobContainer container = CloudStorageAccount.Parse(paneWindow1.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
      container.CreateIfNotExists();
      container.SetPermissions(new BlobContainerPermissions()
      {
        PublicAccess = BlobContainerPublicAccessType.Blob
      });
      await Task.Run((Action) (() => paneWindow.getInfoListasync(container)));
      paneWindow1.state = WindowState.Main;
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshAsync\u003Eb__134_4));
      await Task.Run((Action) (() => paneWindow.PopulateTrainingTree(container)));
      await Task.Run((Action) (() => paneWindow.PopulateFamilyTree(container)));
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshAsync\u003Eb__134_7));
    }
    else
    {
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshAsync\u003Eb__134_1));
    }
  }

  private async void RefreshButton_Click(object sender, RoutedEventArgs e)
  {
    PaneWindow paneWindow1 = this;
    // ISSUE: reference to a compiler-generated method
    paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshButton_Click\u003Eb__135_0));
    // ISSUE: reference to a compiler-generated method
    if (await Task.Run<bool>(new Func<Task<bool>>(paneWindow1.\u003CRefreshButton_Click\u003Eb__135_2)))
    {
      PaneWindow paneWindow = paneWindow1;
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshButton_Click\u003Eb__135_3));
      CloudBlobContainer container = CloudStorageAccount.Parse(paneWindow1.blobConnectionString).CreateCloudBlobClient().GetContainerReference(EDGE.EDGEBrowser.EDGEBrowser.containerString);
      container.CreateIfNotExists();
      container.SetPermissions(new BlobContainerPermissions()
      {
        PublicAccess = BlobContainerPublicAccessType.Blob
      });
      await Task.Run((Action) (() => paneWindow.getInfoListasync(container)));
      paneWindow1.state = WindowState.Main;
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshButton_Click\u003Eb__135_5));
      await Task.Run((Action) (() => paneWindow.PopulateTrainingTree(container)));
      await Task.Run((Action) (() => paneWindow.PopulateFamilyTree(container)));
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshButton_Click\u003Eb__135_8));
    }
    else
    {
      int num = (int) MessageBox.Show("The EDGE^R Browser is unable to connect to the Internet. Please check your internet connection and try again.", "EDGE^R Browser - Unable to Connect");
      // ISSUE: reference to a compiler-generated method
      paneWindow1.Dispatcher.Invoke(new Action(paneWindow1.\u003CRefreshButton_Click\u003Eb__135_1));
    }
  }

  private bool CheckforReadOnly(string path)
  {
    DirectoryInfo directoryInfo = new DirectoryInfo(path);
    FileInfo fileInfo = new FileInfo(path);
    return (directoryInfo.Exists || fileInfo.Exists) && (directoryInfo.Attributes.HasFlag((System.Enum) FileAttributes.ReadOnly) || fileInfo.Attributes.HasFlag((System.Enum) FileAttributes.ReadOnly));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/edgebrowser/forms/panewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.MainPaneWindow = (PaneWindow) target;
        this.MainPaneWindow.Loaded += new RoutedEventHandler(this.Window_Loaded);
        break;
      case 2:
        this.TopGrid = (Grid) target;
        break;
      case 3:
        this.ContentGrid = (Grid) target;
        break;
      case 4:
        this.MainIcon = (Button) target;
        this.MainIcon.Click += new RoutedEventHandler(this.HomeButton_Click);
        break;
      case 5:
        this.FindBar = (TextBox) target;
        this.FindBar.GotFocus += new RoutedEventHandler(this.FindBar_GotFocus);
        this.FindBar.LostFocus += new RoutedEventHandler(this.FindBar_LostFocus);
        this.FindBar.AddHandler(ContentElement.KeyDownEvent, (Delegate) new KeyEventHandler(this.OnKeyHandler));
        break;
      case 6:
        this.dlTextBlock = (TextBlock) target;
        break;
      case 7:
        this.CancelDownload = (Button) target;
        this.CancelDownload.Click += new RoutedEventHandler(this.CancelDownload_Click);
        break;
      case 8:
        this.ContactBrowser = (WebBrowser) target;
        break;
      case 9:
        this.ContactTextBlock = (TextBlock) target;
        break;
      case 10:
        ((Hyperlink) target).RequestNavigate += new RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
        break;
      case 11:
        ((Hyperlink) target).RequestNavigate += new RequestNavigateEventHandler(this.Hyperlink_RequestNavigate);
        break;
      case 12:
        this.WrapScroller = (ScrollViewer) target;
        break;
      case 13:
        this.HomeButtonPanel = (StackPanel) target;
        break;
      case 14:
        this.TrainingExpander = (Button) target;
        this.TrainingExpander.Click += new RoutedEventHandler(this.TrainingButton_Click);
        break;
      case 15:
        this.FamilyExpander = (Button) target;
        this.FamilyExpander.Click += new RoutedEventHandler(this.FamToolsButton_Click);
        break;
      case 16 /*0x10*/:
        this.ManualsButton = (Button) target;
        this.ManualsButton.Click += new RoutedEventHandler(this.ToolsButton_Click);
        break;
      case 17:
        this.VideosButton = (Button) target;
        this.VideosButton.Click += new RoutedEventHandler(this.VideosButton_Click);
        break;
      case 18:
        this.RefreshButton = (Button) target;
        this.RefreshButton.Click += new RoutedEventHandler(this.RefreshButton_Click);
        break;
      case 19:
        this.ContactButton = (Button) target;
        this.ContactButton.Click += new RoutedEventHandler(this.ContactButton_Click);
        break;
      case 20:
        this.MediaElement = (MediaElement) target;
        this.MediaElement.MediaOpened += new RoutedEventHandler(this.MediaElement_MediaOpened);
        break;
      case 21:
        this.TrainingScrollViewer = (ScrollViewer) target;
        break;
      case 22:
        this.TrainingTreeView = (TreeView) target;
        break;
      case 23:
        this.VideoScrollViewer = (ScrollViewer) target;
        break;
      case 24:
        this.TrainingVideosTreeView = (TreeView) target;
        break;
      case 25:
        this.SearchScrollViewer = (ScrollViewer) target;
        this.SearchScrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(this.previewMouseWheel);
        break;
      case 26:
        this.SearchTreeView = (TreeView) target;
        break;
      case 27:
        this.FindBarValueHolder = (TextBlock) target;
        break;
      case 28:
        this.FamilyScrollViewer = (ScrollViewer) target;
        break;
      case 29:
        this.FamilyTreeView = (TreeView) target;
        break;
      case 30:
        ((MenuItem) target).Click += new RoutedEventHandler(this.DownloadFamily_RightButtonDown);
        break;
      case 31 /*0x1F*/:
        ((MenuItem) target).Click += new RoutedEventHandler(this.FamilyTreeView_Click);
        break;
      case 32 /*0x20*/:
        this.TrainingFolderDataGrid = (DataGrid) target;
        break;
      case 33:
        this.SublFolderDataGrid = (DataGrid) target;
        break;
      case 34:
        this.ContentDataGrid = (DataGrid) target;
        break;
      case 35:
        this.xps = (DocumentViewer) target;
        break;
      case 36:
        this.CurrentTime = (TextBlock) target;
        break;
      case 37:
        this.TotalTime = (TextBlock) target;
        break;
      case 38:
        this.sliderPanel = (StackPanel) target;
        break;
      case 39:
        this.mediaProgressBar = (ProgressBar) target;
        break;
      case 40:
        this.sliderBar = (Slider) target;
        this.sliderBar.AddHandler(Thumb.DragStartedEvent, (Delegate) new DragStartedEventHandler(this.sliderBar_DragStarted));
        this.sliderBar.AddHandler(Thumb.DragCompletedEvent, (Delegate) new DragCompletedEventHandler(this.sliderBar_DragCompleted));
        break;
      case 41:
        this.MediaBackButton = (Button) target;
        this.MediaBackButton.Click += new RoutedEventHandler(this.MediaBackButton_Click);
        break;
      case 42:
        this.TrainingBackButton = (Button) target;
        this.TrainingBackButton.Click += new RoutedEventHandler(this.TrainingBackButton_Click);
        break;
      case 43:
        this.XPSBackButton = (Button) target;
        this.XPSBackButton.Click += new RoutedEventHandler(this.XPSBackButton_Click);
        break;
      case 44:
        this.HomeButton = (Button) target;
        this.HomeButton.Click += new RoutedEventHandler(this.HomeButton_Click);
        break;
      case 45:
        this.MediaButtonPanel = (WrapPanel) target;
        break;
      case 46:
        this.btnPlay = (Button) target;
        this.btnPlay.Click += new RoutedEventHandler(this.btnPlay_Click);
        break;
      case 47:
        this.btnPause = (Button) target;
        this.btnPause.Click += new RoutedEventHandler(this.btnPause_Click);
        break;
      case 48 /*0x30*/:
        this.btnStop = (Button) target;
        this.btnStop.Click += new RoutedEventHandler(this.btnStop_Click);
        break;
      case 49:
        this.btnMute = (Button) target;
        this.btnMute.Click += new RoutedEventHandler(this.btnMute_Click);
        break;
      case 50:
        this.qualityBox = (ComboBox) target;
        this.qualityBox.SelectionChanged += new SelectionChangedEventHandler(this.QualityBox_SelectionChanged);
        break;
      case 51:
        this.btnDownload = (Button) target;
        this.btnDownload.Click += new RoutedEventHandler(this.btnDownloadVideo_Click);
        break;
      case 52:
        this.btnPopOut = (Button) target;
        this.btnPopOut.Click += new RoutedEventHandler(this.btnPopOut_Click);
        break;
      case 53:
        this.LoadingLbl = (Label) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
