using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class SkillService : ISkillService
{
	public int CalculateXpForTask(TodoTask task)
	{
		var difficulty = Math.Clamp(task.Difficulty, 1, 3);
		return difficulty * 10;
	}

	public void AddXp(Skill skill, int xp)
	{
		if (xp <= 0)
			return;

		skill.CurrentXp += xp;
		while (skill.CurrentXp >= XpNeededForNextLevel(skill.Level))
		{
			skill.CurrentXp -= XpNeededForNextLevel(skill.Level);
			skill.Level += 1;
		}
	}

	public int XpNeededForNextLevel(int level)
	{
		level = Math.Max(1, level);
		return 50 * level;
	}
}

