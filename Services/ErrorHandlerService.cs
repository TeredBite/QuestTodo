using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace QuestTodoMaui.Services;

public class ErrorHandlerService : IErrorHandler
{
	private readonly ILogger<ErrorHandlerService>? _logger;

	public ErrorHandlerService(ILogger<ErrorHandlerService>? logger = null)
	{
		_logger = logger;
	}

	public void HandleException(Exception ex, string? userContext = null)
	{
		_logger?.LogError(ex, "Runtime error: {Context}", userContext ?? "Unknown");

		var message = ex switch
		{
			HttpRequestException => "Проблема с подключением к сети. Проверьте интернет-соединение и попробуйте снова.",
			TaskCanceledException => "Запрос занял слишком много времени. Попробуйте позже.",
			JsonException => "Ошибка чтения данных. Возможно, файл повреждён.",
			IOException => "Ошибка работы с файлами. Проверьте доступ к хранилищу.",
			UnauthorizedAccessException => "Нет прав доступа к файлам приложения.",
			_ => $"Произошла ошибка: {ex.Message}"
		};

		MainThread.BeginInvokeOnMainThread(async () =>
		{
			if (Shell.Current != null)
			{
				await Shell.Current.DisplayAlert("Ошибка", message, "OK");
			}
		});
	}

	public string GetUserFriendlyMessage(string errorKey) => errorKey switch
	{
		"empty_title" => "Название дела не может быть пустым.",
		"no_skills" => "Нет доступных навыков. Сначала создайте скилл в кампании.",
		"network_error" => "Не удалось подключиться к серверу. Проверьте интернет.",
		"quest_parse_error" => "Не удалось прочитать данные квеста.",
		"task_not_found" => "Задача не найдена.",
		"already_exists" => "Пользователь с таким именем уже существует.",
		"password_mismatch" => "Пароли не совпадают.",
		"password_too_short" => "Пароль должен быть не короче 4 символов.",
		"invalid_credentials" => "Неверное имя пользователя или пароль.",
		"not_authenticated" => "Требуется авторизация.",
		_ => "Произошла неизвестная ошибка."
	};

	public string GetRecommendation(string errorKey) => errorKey switch
	{
		"empty_title" => "Введите название дела — это поможет сгенерировать квест.",
		"no_skills" => "Перейдите во вкладку 'Кампания' и создайте хотя бы один навык.",
		"network_error" => "Проверьте подключение Wi-Fi или мобильного интернета.",
		"quest_parse_error" => "Попробуйте сгенерировать квест заново.",
		"task_not_found" => "Возможно, задача была удалена. Вернитесь к списку.",
		"already_exists" => "Выберите другое имя пользователя.",
		"password_mismatch" => "Убедитесь, что оба поля пароля заполнены одинаково.",
		"password_too_short" => "Добавьте больше символов в пароль для безопасности.",
		"invalid_credentials" => "Проверьте правильность ввода логина и пароля.",
		"not_authenticated" => "Войдите в аккаунт или создайте новый.",
		_ => "Если ошибка повторяется, перезапустите приложение."
	};
}
