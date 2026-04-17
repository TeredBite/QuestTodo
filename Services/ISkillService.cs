using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface ISkillService
{
	int CalculateXpForTask(TodoTask task);
	void AddXp(Skill skill, int xp);
	int XpNeededForNextLevel(int level);
}

