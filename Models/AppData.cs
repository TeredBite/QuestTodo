namespace QuestTodoMaui.Models;

public class AppData
{
	public Campaign Campaign { get; set; } = new();
	public List<Skill> Skills { get; set; } = [];
	public List<TodoTask> Tasks { get; set; } = [];
	public List<User> Users { get; set; } = [];
	public Guid CurrentUserId { get; set; } = Guid.Empty;
}

