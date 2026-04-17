namespace QuestTodoMaui.Models;

public class Campaign
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; } = "Новая кампания";
	public string Setting { get; set; } = "Опиши мир (например, киберпанк мегакорпы, неон, подпольные рынки).";
	public string Goal { get; set; } = "Опиши цель героя (например, подорвать влияние корпорации).";
	public string Tone { get; set; } = "serious"; // serious | humorous
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

