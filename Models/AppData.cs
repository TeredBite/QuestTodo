namespace QuestTodoMaui.Models;

public class AppData
{
	public Campaign Campaign { get; set; } = new();
	public List<Skill> Skills { get; set; } = [];
	public List<TodoTask> Tasks { get; set; } = [];
}

