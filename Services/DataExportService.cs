using System.Text.Json;
using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class DataExportService : IDataExportService
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public string ExportSingleEventReport(TodoTask task)
	{
		var export = new
		{
			Type = "single_event",
			GeneratedAt = DateTimeOffset.UtcNow,
			Task = new
			{
				task.Id,
				task.Title,
				task.Note,
				task.Status,
				task.Difficulty,
				task.CreatedAt,
				task.CompletedAt,
				Quest = TryParseQuest(task.QuestJson),
			},
		};
		return JsonSerializer.Serialize(export, JsonOptions);
	}

	public string ExportAllEventsReport(AppState state)
	{
		var export = new
		{
			Type = "all_events",
			GeneratedAt = DateTimeOffset.UtcNow,
			Tasks = state.Tasks
				.Where(t => t.Status == TodoTaskStatus.Completed)
				.OrderByDescending(t => t.CompletedAt)
				.Select(t => new
				{
					t.Id,
					t.Title,
					t.Status,
					t.Difficulty,
					t.CreatedAt,
					t.CompletedAt,
					Quest = TryParseQuest(t.QuestJson),
				})
				.ToList(),
		};
		return JsonSerializer.Serialize(export, JsonOptions);
	}

	public string ExportStatisticalReport(AppState state)
	{
		var total = state.Tasks.Count;
		var completed = state.Tasks.Count(t => t.Status == TodoTaskStatus.Completed);
		var pending = state.Tasks.Count(t => t.Status == TodoTaskStatus.Pending);

		int totalXp = 0;
		foreach (var task in state.Tasks.Where(t => t.Status == TodoTaskStatus.Completed))
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

		var export = new
		{
			Type = "statistical",
			GeneratedAt = DateTimeOffset.UtcNow,
			Summary = new
			{
				Total = total,
				Completed = completed,
				Pending = pending,
				CompletionRate = total > 0 ? (completed * 100.0 / total) : 0,
				TotalXp = totalXp,
			},
			Skills = state.Skills.Select(s => new
			{
				s.Name,
				s.Level,
				s.CurrentXp,
				s.XpNeeded,
			}).ToList(),
		};
		return JsonSerializer.Serialize(export, JsonOptions);
	}

	private static object? TryParseQuest(string? json)
	{
		if (string.IsNullOrWhiteSpace(json))
			return null;
		try
		{
			return JsonSerializer.Deserialize<QuestResult>(json);
		}
		catch
		{
			return null;
		}
	}
}
