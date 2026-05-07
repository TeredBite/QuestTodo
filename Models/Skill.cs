namespace QuestTodoMaui.Models;

public class Skill
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid CampaignId { get; set; }

	public string Name { get; set; } = "";

	public int Level { get; set; } = 1;
	public int CurrentXp { get; set; } = 0;

	// XP needed for next level (100 * level)
	public int XpNeeded => Level * 100;

	// Progress to next level (0-1)
	public double Progress => CurrentXp >= XpNeeded ? 1 : (double)CurrentXp / XpNeeded;
}

