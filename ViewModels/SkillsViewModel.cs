using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class SkillsViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly ISkillService _skillService;

	public ObservableCollection<Skill> Skills => _state.Skills;

	public SkillsViewModel(AppState state, ISkillService skillService)
	{
		_state = state;
		_skillService = skillService;
	}

	[RelayCommand]
	private async Task InitializeAsync()
	{
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

	public int XpNeeded(Skill skill) => _skillService.XpNeededForNextLevel(skill.Level);
}

