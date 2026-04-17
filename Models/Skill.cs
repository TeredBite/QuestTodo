namespace QuestTodoMaui.Models;

public class Skill
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid CampaignId { get; set; }

	public string Name { get; set; } = "";

	public int Level { get; set; } = 1;
	public int CurrentXp { get; set; } = 0;
}

