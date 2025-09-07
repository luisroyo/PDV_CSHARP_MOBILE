using Pos.Mobile.Maui.ViewModels;

namespace Pos.Mobile.Maui.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
