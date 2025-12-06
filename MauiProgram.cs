using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Syncfusion.Maui.Toolkit.Hosting;
using OsintCompanion.Services;
using OsintCompanion.Views;



namespace OsintCompanion;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureSyncfusionToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
				fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemibold");
			});


		// Register Services (as Singleton since HttpClient/Cache are managed internally)
        builder.Services.AddSingleton<IDomainInfoService, DomainInfoService>();
        
        // Register ViewModels (as Transient or Singleton, depending on app scope)
        builder.Services.AddTransient<B1DomainInfoVM>();

        // Register Views, injecting the ViewModel
        builder.Services.AddTransient<DomainInfoPage>();


#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
