using Microsoft.Extensions.Logging;
using Pos.Mobile.Maui.ViewModels;
using Pos.Mobile.Maui.Views;
using Pos.Mobile.Maui.Converters;

namespace Pos.Mobile.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Registrar ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<MainPageViewModel>();

		// Registrar Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MainPage>();

		// Registrar Converters
		builder.Services.AddSingleton<StringToBoolConverter>();
		builder.Services.AddSingleton<InvertedBoolConverter>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
