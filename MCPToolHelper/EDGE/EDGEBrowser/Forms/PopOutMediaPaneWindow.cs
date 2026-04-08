// Decompiled with JetBrains decompiler
// Type: EDGE.EDGEBrowser.Forms.PopOutMediaPaneWindow
// Assembly: EDGEforREVIT, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8F55B9C3-A92E-41C0-AD06-820A67FFC8AF
// Assembly location: C:\ProgramData\Autodesk\Revit\Addins\2024\PTAC_EDGE_BUNDLE\EDGEforREVIT.dll

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

#nullable disable
namespace EDGE.EDGEBrowser.Forms;

public class PopOutMediaPaneWindow : Window, IComponentConnector
{
  private DispatcherTimer timer;
  private bool isFullScreen;
  private List<string> videoQualityPaths = new List<string>();
  private double wid = 800.0;
  private double heigh = 800.0;
  private string downloadingFileName = "";
  private bool isDragging;
  internal Grid MediaGrid;
  internal MediaElement MediaElement;
  internal TextBlock CurrentTime;
  internal TextBlock TotalTime;
  internal StackPanel sliderPanel;
  internal ProgressBar mediaProgressBar;
  internal Slider sliderBar;
  internal WrapPanel MediaButtonPanel;
  internal Button btnPlay;
  internal Button btnPause;
  internal Button btnStop;
  internal Button btnMute;
  internal ComboBox qualityBox;
  internal Button btnFullscreen;
  internal Button btnUnFullscreen;
  internal TextBlock dlTextBlock;
  private bool _contentLoaded;

  public PopOutMediaPaneWindow(string source)
  {
    this.InitializeComponent();
    this.KeyDown += new KeyEventHandler(this.Esc_IsPressed);
    this.timer = new DispatcherTimer();
    this.timer.Interval = TimeSpan.FromMilliseconds(1000.0);
    this.timer.Tick += new EventHandler(this.timer_Tick);
    this.SetUpVideoSource(source);
  }

  private void Esc_IsPressed(object sender, KeyEventArgs e)
  {
    if (!this.isFullScreen)
      return;
    this.Width = this.wid;
    this.Height = this.heigh;
    this.WindowStyle = WindowStyle.SingleBorderWindow;
    this.WindowState = System.Windows.WindowState.Normal;
    this.btnUnFullscreen.Visibility = Visibility.Collapsed;
    this.btnFullscreen.Visibility = Visibility.Visible;
    this.isFullScreen = false;
  }

  private void Esc_IsPressed2(object sender, KeyEventArgs e)
  {
    if (!this.isFullScreen)
      return;
    this.Width = this.wid;
    this.Height = this.heigh;
    this.WindowStyle = WindowStyle.SingleBorderWindow;
    this.WindowState = System.Windows.WindowState.Normal;
    this.btnUnFullscreen.Visibility = Visibility.Collapsed;
    this.btnFullscreen.Visibility = Visibility.Visible;
    this.isFullScreen = false;
  }

  private void SetUpVideoSource(string source)
  {
    Uri uri = new Uri(source);
    this.videoQualityPaths = new List<string>();
    string[] strArray = source.Split('_');
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
    this.MediaElement.Source = uri;
    this.MediaElement.LoadedBehavior = MediaState.Manual;
    this.MediaElement.Play();
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
    string uriString = "";
    foreach (string videoQualityPath in this.videoQualityPaths)
    {
      if (videoQualityPath.Contains(str))
        uriString = videoQualityPath;
    }
    this.qualityBox.IsDropDownOpen = false;
    this.MediaElement.Source = new Uri(uriString);
    this.MediaElement.LoadedBehavior = MediaState.Manual;
    this.MediaElement.Play();
    string[] strArray = uriString.Split('_');
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

  private void btnFullscreen_Click(object sender, RoutedEventArgs e)
  {
    if (!this.isFullScreen)
    {
      this.wid = this.Width;
      this.heigh = this.Height;
      this.WindowStyle = WindowStyle.None;
      this.WindowState = System.Windows.WindowState.Maximized;
      this.btnFullscreen.Visibility = Visibility.Collapsed;
      this.btnUnFullscreen.Visibility = Visibility.Visible;
      this.MediaGrid.Focus();
      this.isFullScreen = true;
    }
    else
    {
      this.Width = this.wid;
      this.Height = this.heigh;
      this.WindowStyle = WindowStyle.SingleBorderWindow;
      this.WindowState = System.Windows.WindowState.Normal;
      this.btnUnFullscreen.Visibility = Visibility.Collapsed;
      this.btnFullscreen.Visibility = Visibility.Visible;
      this.MediaGrid.Focus();
      this.isFullScreen = false;
    }
  }

  private void btnDownloadVideo_Click(object sender, RoutedEventArgs e)
  {
    string[] strArray = this.MediaElement.Source.ToString().Split('/');
    string fName = strArray[strArray.Length - 1];
    this.DownloadVideo(this.MediaElement.Source.ToString(), fName);
  }

  private void DownloadVideo(string dlLink, string fName)
  {
    this.downloadingFileName = fName;
    this.dlTextBlock.Text = $"Downloading {fName} , please wait . . . ";
    Uri address = new Uri(dlLink);
    try
    {
      using (WebClient webClient = new WebClient())
      {
        if (!Directory.Exists("C:\\EDGEforRevit\\Training Videos"))
          Directory.CreateDirectory("C:\\EDGEforRevit\\Training Videos");
        webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(this.WebClient_DownloadDataCompleted);
        webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.WebClient_DownloadDataChanged);
        this.dlTextBlock.Visibility = Visibility.Visible;
        webClient.DownloadFileAsync(address, "C:\\EDGEforRevit\\Training Videos\\" + fName);
      }
    }
    catch
    {
      int num = (int) MessageBox.Show("Cannot download video. Please check your internet connection and try again.", "Download Error");
    }
  }

  private void WebClient_DownloadDataChanged(object sender, DownloadProgressChangedEventArgs e)
  {
    this.dlTextBlock.Text = $"{this.downloadingFileName} - Download Progress: {Math.Round((double) e.BytesReceived / (double) e.TotalBytesToReceive * 100.0, 0).ToString()}%";
  }

  private void WebClient_DownloadDataCompleted(object sender, AsyncCompletedEventArgs e)
  {
    int num = (int) MessageBox.Show("Video has been saved to C:\\EDGEforRevit\\Training Videos\\" + this.downloadingFileName, "Video Saved");
    this.dlTextBlock.Visibility = Visibility.Collapsed;
    this.dlTextBlock.Text = "";
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

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  public void InitializeComponent()
  {
    if (this._contentLoaded)
      return;
    this._contentLoaded = true;
    Application.LoadComponent((object) this, new Uri("/EDGEforREVIT;component/edgebrowser/forms/popoutmediapanewindow.xaml", UriKind.Relative));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
  [EditorBrowsable(EditorBrowsableState.Never)]
  void IComponentConnector.Connect(int connectionId, object target)
  {
    switch (connectionId)
    {
      case 1:
        this.MediaGrid = (Grid) target;
        break;
      case 2:
        this.MediaElement = (MediaElement) target;
        this.MediaElement.MediaOpened += new RoutedEventHandler(this.MediaElement_MediaOpened);
        break;
      case 3:
        this.CurrentTime = (TextBlock) target;
        break;
      case 4:
        this.TotalTime = (TextBlock) target;
        break;
      case 5:
        this.sliderPanel = (StackPanel) target;
        break;
      case 6:
        this.mediaProgressBar = (ProgressBar) target;
        break;
      case 7:
        this.sliderBar = (Slider) target;
        this.sliderBar.AddHandler(Thumb.DragStartedEvent, (Delegate) new DragStartedEventHandler(this.sliderBar_DragStarted));
        this.sliderBar.AddHandler(Thumb.DragCompletedEvent, (Delegate) new DragCompletedEventHandler(this.sliderBar_DragCompleted));
        break;
      case 8:
        this.MediaButtonPanel = (WrapPanel) target;
        break;
      case 9:
        this.btnPlay = (Button) target;
        this.btnPlay.Click += new RoutedEventHandler(this.btnPlay_Click);
        break;
      case 10:
        this.btnPause = (Button) target;
        this.btnPause.Click += new RoutedEventHandler(this.btnPause_Click);
        break;
      case 11:
        this.btnStop = (Button) target;
        this.btnStop.Click += new RoutedEventHandler(this.btnStop_Click);
        break;
      case 12:
        this.btnMute = (Button) target;
        this.btnMute.Click += new RoutedEventHandler(this.btnMute_Click);
        break;
      case 13:
        this.qualityBox = (ComboBox) target;
        this.qualityBox.SelectionChanged += new SelectionChangedEventHandler(this.QualityBox_SelectionChanged);
        break;
      case 14:
        this.btnFullscreen = (Button) target;
        this.btnFullscreen.Click += new RoutedEventHandler(this.btnFullscreen_Click);
        break;
      case 15:
        this.btnUnFullscreen = (Button) target;
        this.btnUnFullscreen.Click += new RoutedEventHandler(this.btnFullscreen_Click);
        break;
      case 16 /*0x10*/:
        this.dlTextBlock = (TextBlock) target;
        break;
      default:
        this._contentLoaded = true;
        break;
    }
  }
}
