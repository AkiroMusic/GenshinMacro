using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using AkiMacro.Interop;

namespace AkiMacro;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyMicaEffect();
        }
        catch (Exception)
        {
        }
    }

    private void ApplyMicaEffect()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var attribute = DwmApi.DWMWA_SYSTEMBACKDROP_TYPE;
        var value = DwmApi.DWMSBT_MAINWINDOW;
        DwmApi.DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int));

        var darkMode = DwmApi.DWMWA_USE_IMMERSIVE_DARK_MODE;
        var darkValue = 1;
        DwmApi.DwmSetWindowAttribute(hwnd, darkMode, ref darkValue, sizeof(int));
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
        else
        {
            DragMove();
        }
    }

    private void MinimizeBtn_Click(object sender, MouseButtonEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseBtn_Click(object sender, MouseButtonEventArgs e)
    {
        Close();
    }

    private void Settings_Click(object sender, MouseButtonEventArgs e)
    {
        var window = new SettingsWindow { Owner = this };
        window.ShowDialog();
    }

    private void About_Click(object sender, MouseButtonEventArgs e)
    {
        var window = new AboutWindow { Owner = this };
        window.ShowDialog();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
            viewModel.Shutdown();
        base.OnClosed(e);
    }
}
