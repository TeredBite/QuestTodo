using System.Collections.ObjectModel;
using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class AppState
{
	private readonly IAppDataStore _store;

	public Campaign Campaign { get; private set; } = new();
	public ObservableCollection<Skill> Skills { get; } = [];
	public ObservableCollection<TodoTask> Tasks { get; } = [];

	public AppState(IAppDataStore store)
	{
		_store = store;
	}

	public async Task InitializeAsync(CancellationToken ct = default)
	{
		var data = await _store.LoadAsync(ct);
		Apply(data);
	}

	public async Task SaveAsync(CancellationToken ct = default)
	{
		var data = new AppData
		{
			Campaign = Campaign,
			Skills = Skills.ToList(),
			Tasks = Tasks.ToList(),
		};

		await _store.SaveAsync(data, ct);
	}

	public void UpdateCampaign(Campaign updated)
	{
		Campaign = updated;
		foreach (var s in Skills)
			s.CampaignId = Campaign.Id;
		foreach (var t in Tasks)
			t.CampaignId = Campaign.Id;
	}

	private void Apply(AppData data)
	{
		Campaign = data.Campaign;

		Skills.Clear();
		foreach (var s in data.Skills.OrderBy(s => s.Name))
			Skills.Add(s);

		Tasks.Clear();
		foreach (var t in data.Tasks.OrderByDescending(t => t.CreatedAt))
			Tasks.Add(t);
	}
}

