using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface IDataExportService
{
	string ExportSingleEventReport(TodoTask task);
	string ExportAllEventsReport(AppState state);
	string ExportStatisticalReport(AppState state);
}
