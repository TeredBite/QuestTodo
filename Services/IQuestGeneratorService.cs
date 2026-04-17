using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface IQuestGeneratorService
{
	Task<QuestResult> GenerateQuestAsync(
		Campaign campaign,
		TodoTask task,
		Skill skill,
		CancellationToken ct = default);
}

