namespace QuestTodoMaui.Models;

public record QuestResult(
	string Title,
	string Quest,
	string Stakes,
	string Reward,
	string[] Tags
);

