using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using QuestTodoMaui.Services;
using QuestTodoMaui.ViewModels;
using QuestTodoMaui.Views;

namespace QuestTodoMaui;

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

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Services
		builder.Services.AddSingleton<IAppDataStore, JsonFileAppDataStore>();
		builder.Services.AddSingleton<AppState>();
		builder.Services.AddSingleton<ISkillService, SkillService>();
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<IDataExportService, DataExportService>();
		builder.Services.AddSingleton<IErrorHandler, ErrorHandlerService>();
		builder.Services.AddSingleton<IQuestGeneratorService>(sp =>
		{
			var logger = sp.GetRequiredService<ILogger<OpenRouterQuestGeneratorService>>();
			var httpClient = new HttpClient();
			return new OpenRouterQuestGeneratorService(httpClient, logger);
		});

		// ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<TasksViewModel>();
		builder.Services.AddTransient<SkillsViewModel>();
		builder.Services.AddTransient<CampaignViewModel>();
		builder.Services.AddTransient<TaskEditViewModel>();
		builder.Services.AddTransient<ProfileViewModel>();
		builder.Services.AddTransient<ReportsViewModel>();

		// Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<TasksPage>();
		builder.Services.AddTransient<SkillsPage>();
		builder.Services.AddTransient<CampaignPage>();
		builder.Services.AddTransient<TaskEditPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddTransient<ReportsPage>();

		return builder.Build();
	}
}
