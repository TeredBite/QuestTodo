using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface IQuestGeneratorService
{
	Task<QuestResult> GenerateQuestAsync(
		Campaign campaign,
		TodoTask task,
		IReadOnlyList<Skill> skills,
		CancellationToken ct = default);
}

