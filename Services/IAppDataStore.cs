using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface IAppDataStore
{
	Task<AppData> LoadAsync(CancellationToken ct = default);
	Task SaveAsync(AppData data, CancellationToken ct = default);
}

