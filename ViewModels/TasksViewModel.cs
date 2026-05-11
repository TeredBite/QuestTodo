using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class TasksViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly IErrorHandler _errorHandler;

	public ObservableCollection<TodoTask> Tasks => _state.Tasks;

	public TasksViewModel(AppState state, IErrorHandler errorHandler)
	{
		_state = state;
		_errorHandler = errorHandler;
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
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TasksViewModel.Initialize");
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task AddTaskAsync()
	{
		try
		{
			await Shell.Current.GoToAsync("task/edit");
		}
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TasksViewModel.AddTask");
		}
	}

	[RelayCommand]
	private async Task OpenTaskAsync(TodoTask? task)
	{
		if (task is null)
		{
			Error = _errorHandler.GetUserFriendlyMessage("task_not_found");
			return;
		}

		try
		{
			await Shell.Current.GoToAsync($"task/edit?taskId={task.Id}");
		}
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TasksViewModel.OpenTask");
		}
	}

	[RelayCommand]
	private async Task DeleteTaskAsync(TodoTask? task)
	{
		if (task is null)
		{
			Error = _errorHandler.GetUserFriendlyMessage("task_not_found");
			return;
		}

		bool confirm = await Shell.Current.DisplayAlert(
			"Удалить дело?",
			$"Удалить \"{task.Title}\"? Это действие нельзя отменить.",
			"Удалить",
			"Отмена");

		if (!confirm)
			return;

		try
		{
			Tasks.Remove(task);
			await _state.SaveAsync();
		}
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TasksViewModel.DeleteTask");
		}
	}
}

