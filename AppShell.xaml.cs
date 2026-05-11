namespace QuestTodoMaui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("task/edit", typeof(Views.TaskEditPage));
		Routing.RegisterRoute("reports", typeof(Views.ReportsPage));
	}
}
