using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BudgetManager.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PrimaryNavigation.SelectedItem = PrimaryNavigation.MenuItems[0];
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        PageTitle.Text = args.IsSettingsSelected
            ? "Settings"
            : (args.SelectedItemContainer?.Content?.ToString() ?? "Budget Manager");
    }
}
