namespace QuestTodoMaui.Models;

public enum TodoTaskStatus
{
	Pending = 0,
	Completed = 1,
}

public class TodoTask
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid CampaignId { get; set; }
	public Guid SkillId { get; set; }

	public string Title { get; set; } = "";
	public string? Note { get; set; }

	/// <summary>
	/// Difficulty 1..3 (easy/medium/hard).
	/// </summary>
	public int Difficulty { get; set; } = 1;

	public TodoTaskStatus Status { get; set; } = TodoTaskStatus.Pending;
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? CompletedAt { get; set; }

	public string? QuestJson { get; set; }
}

