using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public interface IAuthService
{
	User? CurrentUser { get; }
	bool IsAuthenticated => CurrentUser != null;

	Task<bool> RegisterAsync(string username, string password);
	Task<bool> LoginAsync(string username, string password);
	Task LogoutAsync();
}
