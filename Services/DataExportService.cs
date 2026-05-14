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

	private readonly string _reportsPath;

	public DataExportService()
	{
		_reportsPath = Path.Combine(FileSystem.AppDataDirectory, "QuestTodo", "Reports");
		Directory.CreateDirectory(_reportsPath);
	}

	public async Task<string> ExportSingleEventReportAsync(TodoTask task)
	{
		var fileName = $"single_event_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.doc";
		var filePath = Path.Combine(_reportsPath, fileName);

		var html = GenerateSingleEventHtml(task);
		await File.WriteAllTextAsync(filePath, html);

		return filePath;
	}

	public async Task<string> ExportAllEventsReportAsync(AppState state)
	{
		var fileName = $"all_events_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.doc";
		var filePath = Path.Combine(_reportsPath, fileName);

		var html = GenerateAllEventsHtml(state);
		await File.WriteAllTextAsync(filePath, html);

		return filePath;
	}

	public async Task<string> ExportStatisticalReportAsync(AppState state)
	{
		var fileName = $"statistical_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.doc";
		var filePath = Path.Combine(_reportsPath, fileName);

		var html = GenerateStatisticalHtml(state);
		await File.WriteAllTextAsync(filePath, html);

		return filePath;
	}

	private string GenerateSingleEventHtml(TodoTask task)
	{
		var quest = TryParseQuest(task.QuestJson);
		var statusText = task.Status == TodoTaskStatus.Completed ? "Выполнено" : "В ожидании";

		return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Отчёт по событию</title>
<style>
body {{ font-family: Arial, sans-serif; margin: 40px; }}
h1 {{ color: #7F49B4; }}
table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
th {{ background-color: #7F49B4; color: white; }}
tr:nth-child(even) {{ background-color: #f2f2f2; }}
</style>
</head>
<body>
<h1>Отчёт по одному событию</h1>
<p><strong>Дата генерации:</strong> {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}</p>

<h2>Информация о задаче</h2>
<table>
<tr><th>Параметр</th><th>Значение</th></tr>
<tr><td>Название</td><td>{task.Title}</td></tr>
<tr><td>Статус</td><td>{statusText}</td></tr>
<tr><td>Сложность</td><td>{task.Difficulty} (⭐)</td></tr>
<tr><td>Создано</td><td>{task.CreatedAt:yyyy-MM-dd HH:mm}</td></tr>
{(task.CompletedAt.HasValue ? $"<tr><td>Выполнено</td><td>{task.CompletedAt.Value:yyyy-MM-dd HH:mm}</td></tr>" : "")}
{(task.Note != null ? $"<tr><td>Заметки</td><td>{task.Note}</td></tr>" : "")}
</table>

{(quest != null ? $@"
<h2>Детали квеста</h2>
<table>
<tr><th>Параметр</th><th>Значение</th></tr>
<tr><td>Название квеста</td><td>{quest.Title}</td></tr>
<tr><td>Описание</td><td>{quest.Quest}</td></tr>
<tr><td>Что будет если отложить</td><td>{quest.Stakes}</td></tr>
<tr><td>Награда</td><td>{quest.Reward}</td></tr>
<tr><td>Опыт</td><td>+{quest.Experience} XP</td></tr>
<tr><td>Навыки</td><td>{string.Join(", ", quest.Skills)}</td></tr>
</table>
" : "")}

</body>
</html>";
	}

	private string GenerateAllEventsHtml(AppState state)
	{
		var completedTasks = state.Tasks
			.Where(t => t.Status == TodoTaskStatus.Completed)
			.OrderByDescending(t => t.CompletedAt)
			.ToList();

		var rows = string.Join("", completedTasks.Select(t =>
		{
			var quest = TryParseQuest(t.QuestJson);
			return $"<tr><td>{t.Title}</td><td>{t.CompletedAt:yyyy-MM-dd HH:mm}</td><td>{t.Difficulty} ⭐</td><td>{quest?.Experience ?? 0} XP</td><td>{string.Join(", ", quest?.Skills ?? [])}</td></tr>";
		}));

		return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Отчёт по всем событиям</title>
<style>
body {{ font-family: Arial, sans-serif; margin: 40px; }}
h1 {{ color: #7F49B4; }}
table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
th {{ background-color: #7F49B4; color: white; }}
tr:nth-child(even) {{ background-color: #f2f2f2; }}
</style>
</head>
<body>
<h1>Отчёт по всем выполненным событиям</h1>
<p><strong>Дата генерации:</strong> {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}</p>
<p><strong>Всего выполнено:</strong> {completedTasks.Count}</p>

<table>
<tr><th>Название</th><th>Дата выполнения</th><th>Сложность</th><th>Опыт</th><th>Навыки</th></tr>
{rows}
</table>

</body>
</html>";
	}

	private string GenerateStatisticalHtml(AppState state)
	{
		var total = state.Tasks.Count;
		var completed = state.Tasks.Count(t => t.Status == TodoTaskStatus.Completed);
		var pending = state.Tasks.Count(t => t.Status == TodoTaskStatus.Pending);
		var completionRate = total > 0 ? (completed * 100.0 / total) : 0;

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

		var skillsRows = string.Join("", state.Skills.Select(s =>
			$"<tr><td>{s.Name}</td><td>{s.Level}</td><td>{s.CurrentXp}</td><td>{s.XpNeeded}</td></tr>"));

		return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<title>Статистический отчёт</title>
<style>
body {{ font-family: Arial, sans-serif; margin: 40px; }}
h1 {{ color: #7F49B4; }}
table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
th {{ background-color: #7F49B4; color: white; }}
tr:nth-child(even) {{ background-color: #f2f2f2; }}
</style>
</head>
<body>
<h1>Статистический отчёт</h1>
<p><strong>Дата генерации:</strong> {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}</p>

<h2>Общая статистика</h2>
<table>
<tr><th>Параметр</th><th>Значение</th></tr>
<tr><td>Всего задач</td><td>{total}</td></tr>
<tr><td>Выполнено</td><td>{completed}</td></tr>
<tr><td>В ожидании</td><td>{pending}</td></tr>
<tr><td>Процент выполнения</td><td>{completionRate:F1}%</td></tr>
<tr><td>Всего получено XP</td><td>{totalXp}</td></tr>
</table>

<h2>Навыки</h2>
<table>
<tr><th>Название</th><th>Уровень</th><th>Текущий XP</th><th>XP до следующего уровня</th></tr>
{skillsRows}
</table>

</body>
</html>";
	}

	private static QuestResult? TryParseQuest(string? json)
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
