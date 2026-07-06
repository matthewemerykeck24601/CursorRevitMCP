using System.Windows;
using InventorMCPBridge.Settings;

namespace InventorMCPBridge.UI;

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
        EnableGatewayCheckbox.IsChecked = s.EnableLocalApiGateway;
        AutoStartCheckbox.IsChecked = s.AutoStartOnLaunch;
        VerbosePayloadCheckbox.IsChecked = s.LogVerboseGatewayPayloads;
        GatewayPortTextBox.Text = s.LocalApiGatewayPort.ToString();
        HelpUrlTextBox.Text = s.HelpUrl;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
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
            EnableLocalApiGateway = EnableGatewayCheckbox.IsChecked == true,
            AutoStartOnLaunch = AutoStartCheckbox.IsChecked == true,
            LogVerboseGatewayPayloads = VerbosePayloadCheckbox.IsChecked == true,
            LocalApiGatewayPort = gatewayPort,
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
                $"Failed to save settings.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "Settings",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
