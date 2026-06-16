using System.Windows;
using System.Windows.Input;

namespace GenshinMacro;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CloseButton_Click(object sender, MouseButtonEventArgs e)
    {
        Close();
    }
}
