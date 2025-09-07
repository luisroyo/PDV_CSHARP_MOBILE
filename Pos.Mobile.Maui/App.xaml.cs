using Pos.Mobile.Maui.Views;

namespace Pos.Mobile.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Iniciar com tela de login
		MainPage = new LoginPage(new ViewModels.LoginViewModel());
	}
}
