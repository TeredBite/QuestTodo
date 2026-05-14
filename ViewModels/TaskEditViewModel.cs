using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class TaskEditViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly ISkillService _skillService;
	private readonly IQuestGeneratorService _questGenerator;
	private readonly IErrorHandler _errorHandler;
	private readonly ILogger<TaskEditViewModel>? _logger;

	private TodoTask? _model;

	[ObservableProperty] private string title = "";
	[ObservableProperty] private string? note;
	[ObservableProperty] private int difficulty = 1;
	[ObservableProperty] private Skill? selectedSkill;

	[ObservableProperty] private string questTitle = "";
	[ObservableProperty] private string questText = "";
	[ObservableProperty] private string questStakes = "";
	[ObservableProperty] private string questReward = "";
	[ObservableProperty] private string questSkills = "";
	[ObservableProperty] private string questExperience = "";

	[ObservableProperty] private bool isCompleted;
	[ObservableProperty] private bool isQuestGenerated;
	[ObservableProperty] private bool isSaved;

	public bool CanGenerateQuest => !IsSaved && !IsCompleted;
	public bool CanSave => !IsSaved;

	public IReadOnlyList<Skill> Skills => _state.Skills;

	public TaskEditViewModel(
		AppState state,
		ISkillService skillService,
		IQuestGeneratorService questGenerator,
		IErrorHandler errorHandler,
		ILogger<TaskEditViewModel>? logger = null)
	{
		_state = state;
		_skillService = skillService;
		_questGenerator = questGenerator;
		_errorHandler = errorHandler;
		_logger = logger;
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
			IsSaved = false;
			IsQuestGenerated = false;
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
			IsSaved = false;
			IsQuestGenerated = false;
			ApplyQuest(null);
			return;
		}

		Title = task.Title;
		Note = task.Note;
		Difficulty = task.Difficulty;
		SelectedSkill = _state.Skills.FirstOrDefault(s => s.Id == task.SkillId) ?? _state.Skills.FirstOrDefault();
		IsCompleted = task.Status == TodoTaskStatus.Completed;
		IsSaved = true; // Task exists, so it's saved

		// If task has a quest, mark as generated
		if (!string.IsNullOrWhiteSpace(task.QuestJson))
		{
			IsQuestGenerated = true;
		}
		ApplyQuest(task.QuestJson);
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		Error = null;
		var titleTrimmed = Title?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(titleTrimmed))
		{
			Error = _errorHandler.GetUserFriendlyMessage("empty_title");
			return;
		}

		// Use quest-generated skill if available, otherwise use selected or first available
		Guid skillId = Guid.Empty;
		if (!string.IsNullOrWhiteSpace(_model?.QuestJson))
		{
			try
			{
				var quest = JsonSerializer.Deserialize<QuestResult>(_model.QuestJson);
				if (quest?.Skills != null && quest.Skills.Length > 0)
				{
					var skill = _state.Skills.FirstOrDefault(s => 
						s.Name.Equals(quest.Skills[0], StringComparison.OrdinalIgnoreCase));
					if (skill != null)
					{
						skillId = skill.Id;
					}
				}
			}
			catch
			{
				// Ignore parsing errors
			}
		}

		if (skillId == Guid.Empty)
		{
			skillId = SelectedSkill?.Id ?? _state.Skills.FirstOrDefault()?.Id ?? Guid.Empty;
		}

		if (skillId == Guid.Empty)
		{
			Error = _errorHandler.GetUserFriendlyMessage("no_skills");
			return;
		}

		try
		{
		if (_model is null)
		{
			// Serialize current quest if it exists
			string? questJson = null;
			if (!string.IsNullOrWhiteSpace(QuestTitle))
			{
				var quest = new QuestResult(
					QuestTitle,
					QuestText,
					QuestStakes,
					QuestReward,
					new[] { "general" }, // Will be updated by quest generation
					Difficulty,
					QuestSkills.Split(", ", StringSplitOptions.RemoveEmptyEntries),
					int.TryParse(QuestExperience, out int exp) ? exp : 10
				);
				questJson = JsonSerializer.Serialize(quest);
			}

			var task = new TodoTask
			{
				CampaignId = _state.Campaign.Id,
				SkillId = skillId,
				Title = titleTrimmed,
				Note = Note?.Trim(),
				Difficulty = Math.Clamp(Difficulty, 1, 3),
				Status = IsCompleted ? TodoTaskStatus.Completed : TodoTaskStatus.Pending,
				CreatedAt = DateTimeOffset.UtcNow,
				QuestJson = questJson
			};
			_state.Tasks.Insert(0, task);
			_model = task;
		}
		else
		{
			_model.Title = titleTrimmed;
			_model.Note = Note?.Trim();
			_model.Difficulty = Math.Clamp(Difficulty, 1, 3);
			_model.SkillId = skillId;
			_model.Status = IsCompleted ? TodoTaskStatus.Completed : TodoTaskStatus.Pending;
		}

		await _state.SaveAsync();
		IsSaved = true;
		await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TaskEditViewModel.Save");
		}
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

		IsBusy = true;
		try
		{
			var tempTask = _model ?? new TodoTask
			{
				CampaignId = _state.Campaign.Id,
				SkillId = Guid.Empty,
				Title = titleTrimmed,
				Note = Note?.Trim(),
				Difficulty = 1,
			};

			var quest = await _questGenerator.GenerateQuestAsync(_state.Campaign, tempTask, _state.Skills);
			var json = JsonSerializer.Serialize(quest);
			ApplyQuest(json);

			// Update task properties from quest result
			Difficulty = quest.Difficulty;
			
			// Find or create skill based on quest skills
			if (quest.Skills != null && quest.Skills.Length > 0)
			{
				var skillName = quest.Skills[0];
				var skill = _state.Skills.FirstOrDefault(s => 
					s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
				
				if (skill != null)
				{
					SelectedSkill = skill;
					if (_model != null)
					{
						_model.SkillId = skill.Id;
					}
				}
			}

			if (_model is not null)
			{
				_model.QuestJson = json;
				_model.Difficulty = quest.Difficulty;
				await _state.SaveAsync();
			}
		}
		catch (Exception ex)
		{
			Error = _errorHandler.GetUserFriendlyMessage("network_error");
			_errorHandler.HandleException(ex, "TaskEditViewModel.GenerateQuest");
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task CompleteAsync()
	{
		try
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

		_model.Status = TodoTaskStatus.Completed;
		_model.CompletedAt = DateTimeOffset.UtcNow;
		IsCompleted = true;

		// Use experience from quest result if available
		int xp = 10; // default fallback
		if (!string.IsNullOrWhiteSpace(_model.QuestJson))
		{
			try
			{
				var quest = JsonSerializer.Deserialize<QuestResult>(_model.QuestJson);
				if (quest != null && quest.Experience > 0)
				{
					xp = quest.Experience;
				}
			}
			catch
			{
				// Fallback to default if parsing fails
			}
		}

		// Add XP to all skills from the quest
		if (!string.IsNullOrWhiteSpace(_model.QuestJson))
		{
			try
			{
				var quest = JsonSerializer.Deserialize<QuestResult>(_model.QuestJson);
				if (quest?.Skills != null && quest.Skills.Length > 0)
				{
					foreach (var skillName in quest.Skills)
					{
						var skill = _state.Skills.FirstOrDefault(s =>
							s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
						if (skill != null)
						{
							_skillService.AddXp(skill, xp);
							_logger?.LogInformation("Added {Xp} XP to skill {SkillName}", xp, skill.Name);
						}
						else
						{
							_logger?.LogWarning("Skill not found: {SkillName}. Available skills: {AvailableSkills}",
								skillName, string.Join(", ", _state.Skills.Select(s => s.Name)));
						}
					}
				}
				else
				{
					_logger?.LogWarning("Quest has no skills. Using task's skill.");
					var skill = _state.Skills.FirstOrDefault(s => s.Id == _model.SkillId);
					if (skill != null)
					{
						_skillService.AddXp(skill, xp);
					}
				}
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to parse quest for XP distribution. Using task's skill.");
				// If quest parsing fails, use the first skill from the task
				var skill = _state.Skills.FirstOrDefault(s => s.Id == _model.SkillId);
				if (skill != null)
				{
					_skillService.AddXp(skill, xp);
				}
			}
		}
		else
		{
			// Fallback to task's skill if no quest
			_logger?.LogInformation("No quest JSON. Using task's skill.");
			var skill = _state.Skills.FirstOrDefault(s => s.Id == _model.SkillId);
			if (skill != null)
			{
				_skillService.AddXp(skill, xp);
			}
		}

		await _state.SaveAsync();
		await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TaskEditViewModel.Complete");
		}
	}

	[RelayCommand]
	private async Task DeleteAsync()
	{
		try
		{
		if (_model is null)
		{
			await Shell.Current.GoToAsync("..");
			return;
		}

		bool confirm = await Shell.Current.DisplayAlert(
			"Удалить дело?",
			$"Удалить \"{_model.Title}\"? Это действие нельзя отменить.",
			"Удалить",
			"Отмена");

		if (!confirm)
			return;

		_state.Tasks.Remove(_model);
		await _state.SaveAsync();
		await Shell.Current.GoToAsync("..");
		}
		catch (Exception ex)
		{
			_errorHandler.HandleException(ex, "TaskEditViewModel.Delete");
		}
	}

	private void ApplyQuest(string? questJson)
	{
		if (string.IsNullOrWhiteSpace(questJson))
		{
			QuestTitle = "";
			QuestText = "";
			QuestStakes = "";
			QuestReward = "";
			QuestSkills = "";
			QuestExperience = "";
			IsQuestGenerated = false;
			return;
		}

		try
		{
			var quest = JsonSerializer.Deserialize<QuestResult>(questJson);
			QuestTitle = quest?.Title ?? "";
			QuestText = quest?.Quest ?? "";
			QuestStakes = quest?.Stakes ?? "";
			QuestReward = quest?.Reward ?? "";
			QuestSkills = quest?.Skills != null ? string.Join(", ", quest.Skills) : "";
			QuestExperience = quest?.Experience > 0 ? quest.Experience.ToString() : "";
			IsQuestGenerated = true;
		}
		catch
		{
			QuestTitle = "";
			QuestText = questJson;
			QuestStakes = "";
			QuestReward = "";
			QuestSkills = "";
			QuestExperience = "";
			IsQuestGenerated = false;
		}
	}
}

