using QuestTodoMaui.Helpers;
using QuestTodoMaui.Models;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

public partial class TasksPage : ContentPage
{
	private TasksViewModel Vm => (TasksViewModel)BindingContext;

	public TasksPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<TasksViewModel>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Vm.InitializeCommand.ExecuteAsync(null);
	}

	private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		var task = e.CurrentSelection?.FirstOrDefault() as TodoTask;
		if (sender is CollectionView cv)
			cv.SelectedItem = null;

		await Vm.OpenTaskCommand.ExecuteAsync(task);
	}
}

