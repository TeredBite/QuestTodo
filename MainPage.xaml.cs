namespace QuestTodoMaui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnOpenTasksClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//tasks");
	}
}
