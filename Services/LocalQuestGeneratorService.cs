using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class LocalQuestGeneratorService : IQuestGeneratorService
{
	public Task<QuestResult> GenerateQuestAsync(
		Campaign campaign,
		TodoTask task,
		Skill skill,
		CancellationToken ct = default)
	{
		// Черновик: простая "сюжетная" склейка без внешнего API.
		// Позже можно заменить на OpenRouter реализацию с тем же интерфейсом.
		var tone = (campaign.Tone ?? "serious").Trim().ToLowerInvariant();

		var title = $"{skill.Name}: {task.Title}";
		var quest = tone switch
		{
			"humorous" => $"Твой герой берёт миссию: «{task.Title}». Даже в мире, где всё решают корпораты, {skill.Name.ToLowerInvariant()} — это сила.",
			_ => $"Задание: «{task.Title}». Это напрямую связано с твоей целью: {campaign.Goal}",
		};

		var stakes = tone switch
		{
			"humorous" => "Если затянешь — мир не рухнет, но драматичное музыкальное сопровождение включится само.",
			_ => "Если отложить, давление со стороны мира кампании вырастет — станет сложнее двигаться к цели.",
		};

		var reward = tone switch
		{
			"humorous" => $"Награда: +репутация в ветке «{skill.Name}», и немного уважения от твоего внутреннего NPC.",
			_ => $"Награда: укрепление навыка «{skill.Name}» и ещё один шаг к большой цели.",
		};

		var tags = new[]
		{
			skill.Name,
			task.Difficulty switch { 1 => "easy", 2 => "medium", _ => "hard" }
		};

		return Task.FromResult(new QuestResult(title, quest, stakes, reward, tags));
	}
}

