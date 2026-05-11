using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly IAuthService _authService;

	public ProfileViewModel(AppState state, IAuthService authService)
	{
		_state = state;
		_authService = authService;
	}

	public string Username => _authService.CurrentUser?.Username ?? "Гость";
	public int TotalTasks => _state.Tasks.Count;
	public int CompletedTasks => _state.Tasks.Count(t => t.Status == Models.TodoTaskStatus.Completed);
	public int PendingTasks => _state.Tasks.Count(t => t.Status == Models.TodoTaskStatus.Pending);
	public int TotalSkills => _state.Skills.Count;

	[RelayCommand]
	private async Task LogoutAsync()
	{
		await _authService.LogoutAsync();
		await Shell.Current.GoToAsync("//login");
	}

	[RelayCommand]
	private async Task OpenReportsAsync()
	{
		await Shell.Current.GoToAsync("reports");
	}
}
