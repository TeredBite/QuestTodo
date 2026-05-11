using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
	private readonly IAuthService _authService;

	[ObservableProperty] private string username = "";
	[ObservableProperty] private string password = "";
	[ObservableProperty] private string confirmPassword = "";
	[ObservableProperty] private bool isRegistering;

	public LoginViewModel(IAuthService authService)
	{
		_authService = authService;
	}

	[RelayCommand]
	private async Task LoginAsync()
	{
		Error = null;

		if (string.IsNullOrWhiteSpace(Username))
		{
			Error = "Введите имя пользователя.";
			return;
		}

		if (string.IsNullOrWhiteSpace(Password))
		{
			Error = "Введите пароль.";
			return;
		}

		IsBusy = true;
		try
		{
			var success = await _authService.LoginAsync(Username, Password);
			if (success)
			{
				await Shell.Current.GoToAsync("//tasks");
			}
			else
			{
				Error = "Неверное имя пользователя или пароль.";
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task RegisterAsync()
	{
		Error = null;

		if (string.IsNullOrWhiteSpace(Username))
		{
			Error = "Введите имя пользователя.";
			return;
		}

		if (string.IsNullOrWhiteSpace(Password))
		{
			Error = "Введите пароль.";
			return;
		}

		if (Password != ConfirmPassword)
		{
			Error = "Пароли не совпадают.";
			return;
		}

		if (Password.Length < 4)
		{
			Error = "Пароль должен быть не короче 4 символов.";
			return;
		}

		IsBusy = true;
		try
		{
			var success = await _authService.RegisterAsync(Username, Password);
			if (success)
			{
				await Shell.Current.GoToAsync("//tasks");
			}
			else
			{
				Error = "Пользователь с таким именем уже существует.";
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private void ToggleMode()
	{
		IsRegistering = !IsRegistering;
		Error = null;
	}
}
