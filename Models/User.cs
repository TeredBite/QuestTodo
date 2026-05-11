namespace QuestTodoMaui.Models;

public class User
{
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Username { get; set; } = "";
	public string PasswordHash { get; set; } = "";
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
