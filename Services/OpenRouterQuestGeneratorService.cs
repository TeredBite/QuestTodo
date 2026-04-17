using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class OpenRouterQuestGeneratorService : IQuestGeneratorService
{
	private const string Endpoint = "https://openrouter.ai/api/v1/chat/completions";
	private const string Model = "openrouter/elephant-alpha";

	// ВАЖНО: подставь сюда свой ключ OpenRouter.
	// Лучше вынести в защищённое хранилище или конфиг.
	private const string ApiKey = "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";

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		PropertyNameCaseInsensitive = true,
	};

	private readonly HttpClient _httpClient;
	private readonly ILogger<OpenRouterQuestGeneratorService> _logger;

	public OpenRouterQuestGeneratorService(HttpClient httpClient, ILogger<OpenRouterQuestGeneratorService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}

	public async Task<QuestResult> GenerateQuestAsync(
		Campaign campaign,
		TodoTask task,
		Skill skill,
		CancellationToken ct = default)
	{
		if (string.IsNullOrWhiteSpace(ApiKey) || ApiKey.Contains("PUT_YOUR_OPENROUTER_API_KEY_HERE", StringComparison.Ordinal))
		{
			throw new InvalidOperationException("OpenRouter API key is not настроен. Задай его в OpenRouterQuestGeneratorService.ApiKey.");
		}

		var systemPrompt =
			"Ты сценарист, который превращает реальные дела пользователя в квесты в вымышленном мире. " +
			"Всегда возвращай ОДИН JSON-объект без пояснений и форматирования кода. " +
			"Структура: {\"title\": string, \"quest\": string, \"stakes\": string, \"reward\": string, \"tags\": string[]}. " +
			"Не добавляй разделители ``` и не пиши ничего кроме JSON.";

		var worldDescription = campaign.Setting;
		var goalDescription = campaign.Goal;
		var tone = (campaign.Tone ?? "serious").Trim().ToLowerInvariant();
		var difficulty = task.Difficulty switch
		{
			1 => "легкое",
			2 => "среднее",
			_ => "сложное",
		};

		var userPrompt =
			"Сеттинг мира:\n" + worldDescription + "\n\n" +
			"Цель героя:\n" + goalDescription + "\n\n" +
			"Текущее дело:\n" +
			$"- Название: \"{task.Title}\"\n" +
			$"- Категория / скилл: \"{skill.Name}\"\n" +
			$"- Сложность: {difficulty}\n\n" +
			"Требования к ответу:\n" +
			"- title: короткий заголовок квеста (1 строка).\n" +
			"- quest: 1–3 предложения, объясняющие, зачем это задание важно в этом мире.\n" +
			"- stakes: мягко опиши, что будет, если отложить (без прямого насилия, реального вреда и инструкций).\n" +
			"- reward: что герой \"получит\" в терминах лора (репутация, влияние и т.п., можно сослаться на скилл).\n" +
			"- tags: массив тегов, включи название скилла и сложность (easy/medium/hard).\n\n" +
			"Пиши на русском.";

		var request = new ChatCompletionRequest
		{
			Model = Model,
			Messages =
			[
				new ChatMessage { Role = "system", Content = systemPrompt },
				new ChatMessage { Role = "user", Content = userPrompt },
			],
			ResponseFormat = new ResponseFormat { Type = "json_object" },
			Temperature = 0.8,
			MaxTokens = 300,
		};

		var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint)
		{
			Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json")
		};

		httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

		// Рекомендуемые заголовки OpenRouter
		if (!httpRequest.Headers.Contains("HTTP-Referer"))
		{
			httpRequest.Headers.Add("HTTP-Referer", "https://localhost/quest-todo-maui");
		}
		if (!httpRequest.Headers.Contains("X-Title"))
		{
			httpRequest.Headers.Add("X-Title", "QuestTodoMaui");
		}

		using var response = await _httpClient.SendAsync(httpRequest, ct);
		var responseText = await response.Content.ReadAsStringAsync(ct);

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("OpenRouter error {Status}: {Body}", response.StatusCode, responseText);
			throw new InvalidOperationException($"OpenRouter API вернул ошибку: {(int)response.StatusCode}");
		}

		var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(responseText, JsonOptions);
		var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;

		if (string.IsNullOrWhiteSpace(content))
		{
			throw new InvalidOperationException("OpenRouter вернул пустой ответ.");
		}

		var jsonPayload = ExtractJson(content);

		try
		{
			var quest = JsonSerializer.Deserialize<QuestResult>(jsonPayload, JsonOptions);
			if (quest is null)
				throw new JsonException("Deserialized QuestResult is null.");

			return quest;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Не удалось распарсить JSON от OpenRouter. Raw: {Content}", content);
			throw;
		}
	}

	private static string ExtractJson(string content)
	{
		content = content.Trim();

		// На случай, если модель вернула ```json ... ```:
		if (content.StartsWith("```", StringComparison.Ordinal))
		{
			var firstNewLine = content.IndexOf('\n');
			var lastFence = content.LastIndexOf("```", StringComparison.Ordinal);
			if (firstNewLine >= 0 && lastFence > firstNewLine)
			{
				content = content.Substring(firstNewLine + 1, lastFence - firstNewLine - 1).Trim();
			}
		}

		return content;
	}

	#region OpenRouter API models

	private sealed class ChatCompletionRequest
	{
		public string Model { get; set; } = "";
		public List<ChatMessage> Messages { get; set; } = [];
		public ResponseFormat? ResponseFormat { get; set; }
		public double Temperature { get; set; } = 0.8;
		public int? MaxTokens { get; set; }
	}

	private sealed class ChatMessage
	{
		public string Role { get; set; } = "user";
		public string Content { get; set; } = "";
	}

	private sealed class ResponseFormat
	{
		public string Type { get; set; } = "text";
	}

	private sealed class ChatCompletionResponse
	{
		public List<ChatChoice> Choices { get; set; } = [];
	}

	private sealed class ChatChoice
	{
		public ChatMessage? Message { get; set; }
	}

	#endregion
}

