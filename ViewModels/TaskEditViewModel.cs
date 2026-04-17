using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class TaskEditViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly ISkillService _skillService;
	private readonly IQuestGeneratorService _questGenerator;

	private TodoTask? _model;

	[ObservableProperty] private string title = "";
	[ObservableProperty] private string? note;
	[ObservableProperty] private int difficulty = 1;
	[ObservableProperty] private Skill? selectedSkill;

	[ObservableProperty] private string questTitle = "";
	[ObservableProperty] private string questText = "";
	[ObservableProperty] private string questStakes = "";
	[ObservableProperty] private string questReward = "";

	[ObservableProperty] private bool isCompleted;

	public IReadOnlyList<Skill> Skills => _state.Skills;

	public TaskEditViewModel(
		AppState state,
		ISkillService skillService,
		IQuestGeneratorService questGenerator)
	{
		_state = state;
		_skillService = skillService;
		_questGenerator = questGenerator;
	}

	public async Task LoadAsync(Guid? taskId)
	{
		await _state.InitializeAsync();

		if (taskId is null || taskId == Guid.Empty)
		{
			_model = null;
			Title = "";
			Note = "";
			Difficulty = 1;
			SelectedSkill = _state.Skills.FirstOrDefault();
			IsCompleted = false;
			ApplyQuest(null);
			return;
		}

		var task = _state.Tasks.FirstOrDefault(t => t.Id == taskId.Value);
		_model = task;

		if (task is null)
		{
			Title = "";
			Note = "";
			Difficulty = 1;
			SelectedSkill = _state.Skills.FirstOrDefault();
			IsCompleted = false;
			ApplyQuest(null);
			return;
		}

		Title = task.Title;
		Note = task.Note;
		Difficulty = task.Difficulty;
		SelectedSkill = _state.Skills.FirstOrDefault(s => s.Id == task.SkillId) ?? _state.Skills.FirstOrDefault();
		IsCompleted = task.Status == TodoTaskStatus.Completed;

		ApplyQuest(task.QuestJson);
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		Error = null;
		var titleTrimmed = Title?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(titleTrimmed))
		{
			Error = "Название дела не может быть пустым.";
			return;
		}

		var skill = SelectedSkill ?? _state.Skills.FirstOrDefault();
		if (skill is null)
		{
			Error = "Нет доступных скиллов.";
			return;
		}

		if (_model is null)
		{
			var task = new TodoTask
			{
				CampaignId = _state.Campaign.Id,
				SkillId = skill.Id,
				Title = titleTrimmed,
				Note = Note?.Trim(),
				Difficulty = Math.Clamp(Difficulty, 1, 3),
				Status = IsCompleted ? TodoTaskStatus.Completed : TodoTaskStatus.Pending,
				CreatedAt = DateTimeOffset.UtcNow,
			};
			_state.Tasks.Insert(0, task);
			_model = task;
		}
		else
		{
			_model.Title = titleTrimmed;
			_model.Note = Note?.Trim();
			_model.Difficulty = Math.Clamp(Difficulty, 1, 3);
			_model.SkillId = skill.Id;
			_model.Status = IsCompleted ? TodoTaskStatus.Completed : TodoTaskStatus.Pending;
		}

		await _state.SaveAsync();
		await Shell.Current.GoToAsync("..");
	}

	[RelayCommand]
	private async Task GenerateQuestAsync()
	{
		Error = null;
		var titleTrimmed = Title?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(titleTrimmed))
		{
			Error = "Сначала введи название дела.";
			return;
		}

		var skill = SelectedSkill ?? _state.Skills.FirstOrDefault();
		if (skill is null)
		{
			Error = "Нет доступных скиллов.";
			return;
		}

		IsBusy = true;
		try
		{
			var tempTask = _model ?? new TodoTask
			{
				CampaignId = _state.Campaign.Id,
				SkillId = skill.Id,
				Title = titleTrimmed,
				Note = Note?.Trim(),
				Difficulty = Math.Clamp(Difficulty, 1, 3),
			};

			var quest = await _questGenerator.GenerateQuestAsync(_state.Campaign, tempTask, skill);
			var json = JsonSerializer.Serialize(quest);
			ApplyQuest(json);

			if (_model is not null)
			{
				_model.QuestJson = json;
				await _state.SaveAsync();
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task CompleteAsync()
	{
		if (_model is null)
		{
			Error = "Сначала сохрани задачу.";
			return;
		}

		if (_model.Status == TodoTaskStatus.Completed)
		{
			await Shell.Current.GoToAsync("..");
			return;
		}

		var skill = _state.Skills.FirstOrDefault(s => s.Id == _model.SkillId);
		if (skill is null)
		{
			Error = "Скилл не найден.";
			return;
		}

		_model.Status = TodoTaskStatus.Completed;
		_model.CompletedAt = DateTimeOffset.UtcNow;
		IsCompleted = true;

		var xp = _skillService.CalculateXpForTask(_model);
		_skillService.AddXp(skill, xp);

		await _state.SaveAsync();
		await Shell.Current.GoToAsync("..");
	}

	private void ApplyQuest(string? questJson)
	{
		if (string.IsNullOrWhiteSpace(questJson))
		{
			QuestTitle = "";
			QuestText = "";
			QuestStakes = "";
			QuestReward = "";
			return;
		}

		try
		{
			var quest = JsonSerializer.Deserialize<QuestResult>(questJson);
			QuestTitle = quest?.Title ?? "";
			QuestText = quest?.Quest ?? "";
			QuestStakes = quest?.Stakes ?? "";
			QuestReward = quest?.Reward ?? "";
		}
		catch
		{
			QuestTitle = "";
			QuestText = questJson;
			QuestStakes = "";
			QuestReward = "";
		}
	}
}

