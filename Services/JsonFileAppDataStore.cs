using System.Text.Json;
using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class JsonFileAppDataStore : IAppDataStore
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	private readonly string _path;

	public JsonFileAppDataStore()
	{
		_path = Path.Combine(FileSystem.AppDataDirectory, "appdata.json");
	}

	public async Task<AppData> LoadAsync(CancellationToken ct = default)
	{
		if (!File.Exists(_path))
		{
			return CreateDefault();
		}

		await using var stream = File.OpenRead(_path);
		var data = await JsonSerializer.DeserializeAsync<AppData>(stream, JsonOptions, ct);
		if (data is null)
		{
			return CreateDefault();
		}

		EnsureDefaults(data);
		return data;
	}

	public async Task SaveAsync(AppData data, CancellationToken ct = default)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

		await using var stream = File.Create(_path);
		await JsonSerializer.SerializeAsync(stream, data, JsonOptions, ct);
	}

	private static AppData CreateDefault()
	{
		var campaign = new Campaign();
		var skills = SkillCatalog.DefaultSkillNames
			.Select(n => new Skill
			{
				CampaignId = campaign.Id,
				Name = n,
				Level = 1,
				CurrentXp = 0,
			})
			.ToList();

		return new AppData
		{
			Campaign = campaign,
			Skills = skills,
			Tasks = [],
			Users = [],
			CurrentUserId = Guid.Empty,
		};
	}

	private static void EnsureDefaults(AppData data)
	{
		if (data.Campaign is null)
		{
			data.Campaign = new Campaign();
		}

		data.Skills ??= [];
		data.Tasks ??= [];
		data.Users ??= [];
		if (data.CurrentUserId == default)
		{
			data.CurrentUserId = Guid.Empty;
		}

		var existing = new HashSet<string>(
			data.Skills.Select(s => s.Name),
			StringComparer.OrdinalIgnoreCase);

		foreach (var name in SkillCatalog.DefaultSkillNames)
		{
			if (existing.Contains(name))
				continue;

			data.Skills.Add(new Skill
			{
				CampaignId = data.Campaign.Id,
				Name = name,
				Level = 1,
				CurrentXp = 0,
			});
		}

		foreach (var skill in data.Skills)
		{
			skill.CampaignId = data.Campaign.Id;
			skill.Level = Math.Max(1, skill.Level);
			skill.CurrentXp = Math.Max(0, skill.CurrentXp);
		}

		foreach (var task in data.Tasks)
		{
			task.CampaignId = data.Campaign.Id;
			task.Difficulty = Math.Clamp(task.Difficulty, 1, 3);
		}
	}
}

