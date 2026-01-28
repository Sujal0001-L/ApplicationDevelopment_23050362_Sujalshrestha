using Microsoft.Extensions.Logging;
using YourJournal.Services;

namespace YourJournal;

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
			});

		builder.Services.AddMauiBlazorWebView();

		// Register Services - Singleton pattern for application lifetime
		builder.Services.AddSingleton<LoggingService>();       // Logging for error tracking
		builder.Services.AddSingleton<DatabaseService>();       // Database access layer
		builder.Services.AddSingleton<AuthService>();           // Authentication & session
		builder.Services.AddSingleton<AnalyticsService>();      // Analytics & statistics
		builder.Services.AddSingleton<PdfExportService>();      // PDF generation

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
