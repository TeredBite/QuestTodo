using System.Security.Cryptography;
using System.Text;
using QuestTodoMaui.Models;

namespace QuestTodoMaui.Services;

public class AuthService : IAuthService
{
	private readonly AppState _state;

	public User? CurrentUser { get; private set; }

	public AuthService(AppState state)
	{
		_state = state;
		// Restore current user if previously logged in
		if (state.CurrentUserId != Guid.Empty)
		{
			CurrentUser = state.Users.FirstOrDefault(u => u.Id == state.CurrentUserId);
		}
	}

	public async Task<bool> RegisterAsync(string username, string password)
	{
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			return false;

		await _state.InitializeAsync();

		var normalized = username.Trim().ToLowerInvariant();

		if (_state.Users.Any(u => u.Username.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
			return false; // User already exists

		var user = new User
		{
			Username = normalized,
			PasswordHash = HashPassword(password),
		};

		_state.Users.Add(user);
		_state.CurrentUserId = user.Id;
		CurrentUser = user;
		await _state.SaveAsync();

		return true;
	}

	public async Task<bool> LoginAsync(string username, string password)
	{
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			return false;

		await _state.InitializeAsync();

		var normalized = username.Trim().ToLowerInvariant();
		var user = _state.Users.FirstOrDefault(u =>
			u.Username.Equals(normalized, StringComparison.OrdinalIgnoreCase));

		if (user is null)
			return false;

		if (user.PasswordHash != HashPassword(password))
			return false;

		CurrentUser = user;
		_state.CurrentUserId = user.Id;
		await _state.SaveAsync();

		return true;
	}

	public async Task LogoutAsync()
	{
		CurrentUser = null;
		_state.CurrentUserId = Guid.Empty;
		await _state.SaveAsync();
	}

	private static string HashPassword(string password)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
		return Convert.ToHexString(bytes);
	}
}
