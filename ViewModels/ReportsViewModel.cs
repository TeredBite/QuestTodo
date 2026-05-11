using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly IDataExportService _exportService;

	[ObservableProperty] private string reportContent = "";
	[ObservableProperty] private string selectedReportType = "Статистический отчёт";
	[ObservableProperty] private TodoTask? selectedTask;

	public List<string> ReportTypes { get; } = new()
	{
		"Статистический отчёт",
		"Отчёт по всем событиям",
		"Отчёт по одному событию",
	};

	public ObservableCollection<TodoTask> CompletedTasks =>
		new(_state.Tasks.Where(t => t.Status == TodoTaskStatus.Completed).OrderByDescending(t => t.CompletedAt));

	public bool IsSingleEventReport => SelectedReportType == "Отчёт по одному событию";

	public ReportsViewModel(AppState state, IDataExportService exportService)
	{
		_state = state;
		_exportService = exportService;
	}

	partial void OnSelectedReportTypeChanged(string value)
	{
		ReportContent = "";
		OnPropertyChanged(nameof(IsSingleEventReport));
		if (value != "Отчёт по одному событию")
		{
			GenerateReport();
		}
	}

	[RelayCommand]
	private void GenerateSingleReport()
	{
		if (SelectedTask is null)
		{
			Error = "Выберите выполненное дело.";
			return;
		}
		Error = null;
		ReportContent = BuildSingleEventReport(SelectedTask);
	}

	private void GenerateReport()
	{
		Error = null;
		ReportContent = SelectedReportType switch
		{
			"Статистический отчёт" => BuildStatisticalReport(),
			"Отчёт по всем событиям" => BuildAllEventsReport(),
			_ => "",
		};
	}

	private string BuildStatisticalReport()
	{
		var sb = new StringBuilder();
		sb.AppendLine("═ СТАТИСТИЧЕСКИЙ ОТЧЁТ ═");
		sb.AppendLine();

		var total = _state.Tasks.Count;
		var completed = _state.Tasks.Count(t => t.Status == TodoTaskStatus.Completed);
		var pending = _state.Tasks.Count(t => t.Status == TodoTaskStatus.Pending);
		var completionRate = total > 0 ? (completed * 100.0 / total) : 0;

		sb.AppendLine($"Всего дел:        {total}");
		sb.AppendLine($"Выполнено:        {completed}");
		sb.AppendLine($"В процессе:       {pending}");
		sb.AppendLine($"Процент выполнения: {completionRate:F1}%");
		sb.AppendLine();

		// XP stats
		int totalXp = 0;
		foreach (var task in _state.Tasks.Where(t => t.Status == TodoTaskStatus.Completed))
		{
			if (!string.IsNullOrWhiteSpace(task.QuestJson))
			{
				try
				{
					var quest = JsonSerializer.Deserialize<QuestResult>(task.QuestJson);
					if (quest?.Experience > 0)
						totalXp += quest.Experience;
				}
				catch { }
			}
		}

		sb.AppendLine($"Всего опыта получено: {totalXp} XP");
		sb.AppendLine();

		// Skills
		sb.AppendLine("─ Навыки ─");
		foreach (var skill in _state.Skills.OrderByDescending(s => s.Level).ThenByDescending(s => s.CurrentXp))
		{
			sb.AppendLine($"  {skill.Name}: Уровень {skill.Level} ({skill.CurrentXp}/{skill.XpNeeded} XP)");
		}

		return sb.ToString();
	}

	private string BuildAllEventsReport()
	{
		var sb = new StringBuilder();
		sb.AppendLine("═ ОТЧЁТ ПО ВСЕМ СОБЫТИЯМ ═");
		sb.AppendLine();

		var completedTasks = _state.Tasks
			.Where(t => t.Status == TodoTaskStatus.Completed)
			.OrderByDescending(t => t.CompletedAt)
			.ToList();

		if (completedTasks.Count == 0)
		{
			sb.AppendLine("Нет выполненных дел.");
			return sb.ToString();
		}

		sb.AppendLine($"Выполнено дел: {completedTasks.Count}");
		sb.AppendLine();

		foreach (var task in completedTasks)
		{
			sb.AppendLine($"▸ {task.Title}");
			sb.AppendLine($"   Дата выполнения: {task.CompletedAt:dd.MM.yyyy HH:mm}");

			if (!string.IsNullOrWhiteSpace(task.QuestJson))
			{
				try
				{
					var quest = JsonSerializer.Deserialize<QuestResult>(task.QuestJson);
					if (quest != null)
					{
						sb.AppendLine($"   Квест: {quest.Title}");
						sb.AppendLine($"   Навыки: {string.Join(", ", quest.Skills ?? [])}");
						sb.AppendLine($"   Опыт: +{quest.Experience} XP");
						sb.AppendLine($"   Сложность: {quest.Difficulty}/3");
					}
				}
				catch { }
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	private string BuildSingleEventReport(TodoTask task)
	{
		var sb = new StringBuilder();
		sb.AppendLine("═ ОТЧЁТ ПО СОБЫТИЮ ═");
		sb.AppendLine();
		sb.AppendLine($"Название: {task.Title}");
		sb.AppendLine($"Заметки: {task.Note ?? "—"}");
		sb.AppendLine($"Статус: {(task.Status == TodoTaskStatus.Completed ? "Выполнено" : "В процессе")}");
		sb.AppendLine($"Создано: {task.CreatedAt:dd.MM.yyyy HH:mm}");

		if (task.Status == TodoTaskStatus.Completed && task.CompletedAt.HasValue)
		{
			sb.AppendLine($"Выполнено: {task.CompletedAt.Value:dd.MM.yyyy HH:mm}");
		}

		sb.AppendLine();

		if (!string.IsNullOrWhiteSpace(task.QuestJson))
		{
			try
			{
				var quest = JsonSerializer.Deserialize<QuestResult>(task.QuestJson);
				if (quest != null)
				{
					sb.AppendLine("─ Квест ─");
					sb.AppendLine($"Заголовок: {quest.Title}");
					sb.AppendLine($"Описание: {quest.Quest}");
					sb.AppendLine($"Ставки: {quest.Stakes}");
					sb.AppendLine($"Награда: {quest.Reward}");
					sb.AppendLine($"Сложность: {quest.Difficulty}/3");
					sb.AppendLine($"Навыки: {string.Join(", ", quest.Skills ?? [])}");
					sb.AppendLine($"Опыт: +{quest.Experience} XP");
				}
			}
			catch (Exception ex)
			{
				sb.AppendLine($"Ошибка чтения квеста: {ex.Message}");
			}
		}
		else
		{
			sb.AppendLine("Квест не был сгенерирован.");
		}

		return sb.ToString();
	}

	[RelayCommand]
	private async Task ExportJsonAsync()
	{
		if (string.IsNullOrWhiteSpace(ReportContent))
			return;

		try
		{
			var fileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.json";
			var data = SelectedReportType switch
			{
				"Статистический отчёт" => _exportService.ExportStatisticalReport(_state),
				"Отчёт по всем событиям" => _exportService.ExportAllEventsReport(_state),
				"Отчёт по одному событию" => SelectedTask != null
					? _exportService.ExportSingleEventReport(SelectedTask)
					: null,
				_ => null,
			};

			if (data != null)
			{
				var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
				await File.WriteAllTextAsync(path, data);
				await Shell.Current.DisplayAlert("Готово", $"Отчёт сохранён:\n{path}", "OK");
			}
		}
		catch (Exception ex)
		{
			Error = $"Ошибка экспорта: {ex.Message}";
		}
	}
}
