namespace QuestTodoMaui;

public partial class App : Application
{
	public App(IServiceProvider services)
	{
		InitializeComponent();
		Helpers.ServiceHelper.Services = services;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}