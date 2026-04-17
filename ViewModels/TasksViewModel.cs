using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class TasksViewModel : BaseViewModel
{
	private readonly AppState _state;

	public ObservableCollection<TodoTask> Tasks => _state.Tasks;

	public TasksViewModel(AppState state)
	{
		_state = state;
	}

	[RelayCommand]
	private async Task InitializeAsync()
	{
		if (Tasks.Count > 0)
			return;

		IsBusy = true;
		try
		{
			await _state.InitializeAsync();
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task AddTaskAsync()
	{
		await Shell.Current.GoToAsync("task/edit");
	}

	[RelayCommand]
	private async Task OpenTaskAsync(TodoTask? task)
	{
		if (task is null)
			return;

		await Shell.Current.GoToAsync($"task/edit?taskId={task.Id}");
	}
}

