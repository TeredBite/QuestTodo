using QuestTodoMaui.Services;

namespace QuestTodoMaui;

public partial class App : Application
{
	private readonly IServiceProvider _services;

	public App(IServiceProvider services)
	{
		InitializeComponent();
		Helpers.ServiceHelper.Services = services;
		_services = services;

		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	protected override async void OnStart()
	{
		base.OnStart();

		try
		{
			var authService = _services.GetRequiredService<IAuthService>();
			var state = _services.GetRequiredService<AppState>();
			await state.InitializeAsync();

			if (authService.IsAuthenticated)
			{
				await Shell.Current.GoToAsync("//main/tasks");
			}
			else
			{
				await Shell.Current.GoToAsync("//login");
			}
		}
		catch (Exception ex)
		{
			var errorHandler = _services.GetService<IErrorHandler>();
			errorHandler?.HandleException(ex, "Startup");
		}
	}

	private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
		{
			var errorHandler = _services.GetService<IErrorHandler>();
			errorHandler?.HandleException(ex, "UnhandledException");
		}
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		var errorHandler = _services.GetService<IErrorHandler>();
		errorHandler?.HandleException(e.Exception, "UnobservedTaskException");
		e.SetObserved();
	}
}