using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface IDataExportService
{
	Task<string> ExportSingleEventReportAsync(TodoTask task);
	Task<string> ExportAllEventsReportAsync(AppState state);
	Task<string> ExportStatisticalReportAsync(AppState state);
}
