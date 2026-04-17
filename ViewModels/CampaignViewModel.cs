using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class CampaignViewModel : BaseViewModel
{
	private readonly AppState _state;

	[ObservableProperty]
	private string name = "";

	[ObservableProperty]
	private string setting = "";

	[ObservableProperty]
	private string goal = "";

	[ObservableProperty]
	private string tone = "serious";

	public CampaignViewModel(AppState state)
	{
		_state = state;
	}

	[RelayCommand]
	private async Task InitializeAsync()
	{
		IsBusy = true;
		try
		{
			await _state.InitializeAsync();

			Name = _state.Campaign.Name;
			Setting = _state.Campaign.Setting;
			Goal = _state.Campaign.Goal;
			Tone = _state.Campaign.Tone;
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		var updated = new Campaign
		{
			Id = _state.Campaign.Id,
			CreatedAt = _state.Campaign.CreatedAt,
			Name = Name?.Trim() ?? "",
			Setting = Setting?.Trim() ?? "",
			Goal = Goal?.Trim() ?? "",
			Tone = (Tone?.Trim() ?? "serious").ToLowerInvariant(),
		};

		_state.UpdateCampaign(updated);
		await _state.SaveAsync();
	}
}

