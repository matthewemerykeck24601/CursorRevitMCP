using System.IO;
using Microsoft.Win32;
using System.Windows;
using RevitPublicMCPBridge.Settings;

namespace RevitPublicMCPBridge.UI;

public partial class SettingsWindow : Window
{
    private readonly SettingsStore _settingsStore;

    public SettingsWindow(SettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        InitializeComponent();
        LoadFromSettings();
    }

    private void LoadFromSettings()
    {
        var s = _settingsStore.Current;
        EnableProcessControlCheckbox.IsChecked = s.EnableProcessControl;
        EnableGatewayCheckbox.IsChecked = s.EnableLocalApiGateway;
        AutoStartCheckbox.IsChecked = s.AutoStartOnLaunch;
        StopOnShutdownCheckbox.IsChecked = s.StopOnRevitShutdown;
        AllowStopAnyCheckbox.IsChecked = s.AllowStopAnyInstance;
        GatewayPortTextBox.Text = s.LocalApiGatewayPort.ToString();
        PortTextBox.Text = s.Port.ToString();
        ExecutablePathTextBox.Text = s.ServerExecutablePath;
        ArgsTextBox.Text = s.ServerArguments;
        FamilyLibraryPathTextBox.Text = s.DefaultFamilyLibraryPath;
        FamilyUpgradeOutputPathTextBox.Text = s.DefaultFamilyUpgradeOutputPath;
        HelpUrlTextBox.Text = s.HelpUrl;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PortTextBox.Text?.Trim(), out var port) || port is <= 0 or > 65535)
        {
            MessageBox.Show(
                "Port must be a valid number between 1 and 65535.",
                "Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(GatewayPortTextBox.Text?.Trim(), out var gatewayPort) || gatewayPort is <= 0 or > 65535)
        {
            MessageBox.Show(
                "Gateway port must be a valid number between 1 and 65535.",
                "Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var updated = new BridgeSettings
        {
            EnableProcessControl = EnableProcessControlCheckbox.IsChecked == true,
            EnableLocalApiGateway = EnableGatewayCheckbox.IsChecked == true,
            AutoStartOnLaunch = AutoStartCheckbox.IsChecked == true,
            StopOnRevitShutdown = StopOnShutdownCheckbox.IsChecked == true,
            AllowStopAnyInstance = AllowStopAnyCheckbox.IsChecked == true,
            LocalApiGatewayPort = gatewayPort,
            Port = port,
            ServerExecutablePath = ExecutablePathTextBox.Text?.Trim() ?? string.Empty,
            ServerArguments = ArgsTextBox.Text?.Trim() ?? string.Empty,
            DefaultFamilyLibraryPath = FamilyLibraryPathTextBox.Text?.Trim() ?? string.Empty,
            DefaultFamilyUpgradeOutputPath = FamilyUpgradeOutputPathTextBox.Text?.Trim() ?? string.Empty,
            HelpUrl = HelpUrlTextBox.Text?.Trim() ?? string.Empty,
        };

        try
        {
            _settingsStore.Save(updated);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to save settings.\n\n{ex.Message}",
                "Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnBrowseLibraryPathClick(object sender, RoutedEventArgs e)
    {
        var selected = PickFolder(FamilyLibraryPathTextBox.Text);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            FamilyLibraryPathTextBox.Text = selected;
        }
    }

    private void OnBrowseUpgradePathClick(object sender, RoutedEventArgs e)
    {
        var selected = PickFolder(FamilyUpgradeOutputPathTextBox.Text);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            FamilyUpgradeOutputPathTextBox.Text = selected;
        }
    }

    private static string? PickFolder(string? currentPath)
    {
        var initial = currentPath?.Trim();
        if (!string.IsNullOrWhiteSpace(initial) && !Directory.Exists(initial))
        {
            initial = Path.GetDirectoryName(initial);
        }

        var dialog = new OpenFileDialog
        {
            Title = "Select Folder",
            CheckFileExists = false,
            CheckPathExists = true,
            ValidateNames = false,
            FileName = "Select Folder",
            Filter = "Folders|*.folder",
            InitialDirectory = !string.IsNullOrWhiteSpace(initial) && Directory.Exists(initial)
                ? initial
                : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        };

        var result = dialog.ShowDialog();
        if (result != true)
        {
            return null;
        }

        var folder = Path.GetDirectoryName(dialog.FileName);
        return !string.IsNullOrWhiteSpace(folder) ? folder : null;
    }
}
