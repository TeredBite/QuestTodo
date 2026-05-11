namespace QuestTodoMaui.Services;

public interface IErrorHandler
{
	void HandleException(Exception ex, string? userContext = null);
	string GetUserFriendlyMessage(string errorKey);
	string GetRecommendation(string errorKey);
}
