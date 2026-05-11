using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class DataViewModel : BaseViewModel
{
	private readonly AppState _state;
	private readonly IAppDataStore _store;

	public string DataPath { get; }
	public string? FolderPath { get; }

	[ObservableProperty]
	private string jsonContent = "Загрузка...";

	public DataViewModel(AppState state, IAppDataStore store)
	{
		_state = state;
		_store = store;

		if (store is JsonFileAppDataStore jfs)
		{
			DataPath = jfs.FilePath;
			FolderPath = Path.GetDirectoryName(jfs.FilePath);
		}
		else
		{
			DataPath = "Неизвестно";
			FolderPath = null;
		}
	}

	[RelayCommand]
	private async Task LoadAsync()
	{
		IsBusy = true;
		try
		{
			await _state.InitializeAsync();

			if (File.Exists(DataPath))
			{
				var json = await File.ReadAllTextAsync(DataPath);
				JsonContent = json;
			}
			else
			{
				JsonContent = $"Файл не найден.\nПуть: {DataPath}";
			}
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task OpenFolderAsync()
	{
		if (FolderPath is null || !Directory.Exists(FolderPath))
		{
			Error = "Папка не найдена. Сначала сохраните хотя бы одну задачу.";
			return;
		}

		try
		{
			await Launcher.OpenAsync(new Uri(FolderPath));
		}
		catch
		{
			// Fallback: copy path to clipboard
			await Clipboard.SetTextAsync(FolderPath);
			await Shell.Current.DisplayAlert("Скопировано", $"Путь скопирован в буфер обмена:\n{FolderPath}", "OK");
		}
	}
}
