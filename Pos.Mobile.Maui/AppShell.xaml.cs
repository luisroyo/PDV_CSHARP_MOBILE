using Pos.Mobile.Maui.Views;

namespace Pos.Mobile.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Registrar rotas
		Routing.RegisterRoute("login", typeof(LoginPage));
		Routing.RegisterRoute("main", typeof(MainPage));
	}
}
