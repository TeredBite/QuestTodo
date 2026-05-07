using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestTodoMaui.Models;
using QuestTodoMaui.Services;

namespace QuestTodoMaui.ViewModels;

public partial class CampaignViewModel : BaseViewModel
{
	private readonly AppState _state;

	[ObservableProperty]
	private string name = "";

	[ObservableProperty]
	private string setting = "";

	[ObservableProperty]
	private string goal = "";

	[ObservableProperty]
	private string tone = "serious";

	[ObservableProperty]
	private ToneOption? selectedTone;

	public string SelectedToneDescription => SelectedTone?.Display ?? "Выберите настроение";

	public List<ToneOption> AvailableTones { get; } = new()
	{
		new ToneOption("serious", "Серьёзное — эпичные и драматичные квесты"),
		new ToneOption("humorous", "Юмористическое — лёгкие и забавные приключения"),
		new ToneOption("dark", "Тёмное — мрачная фэнтези, опасности и жертвы"),
		new ToneOption("epic", "Эпическое — судьбоносные решения, величие и героизм"),
		new ToneOption("casual", "Повседневное — уютные, спокойные истории"),
		new ToneOption("noir", "Нуар — интриги, предательства, моральные дилеммы"),
		new ToneOption("horror", "Хоррор — ужасы, выживание, паранормальное"),
		new ToneOption("romantic", "Романтическое — чувства, отношения, личный рост"),
	};

	partial void OnSelectedToneChanged(ToneOption? value)
	{
		if (value != null)
		{
			Tone = value.Value;
		}
	}

	public CampaignViewModel(AppState state)
	{
		_state = state;
	}

	[RelayCommand]
	private async Task InitializeAsync()
	{
		IsBusy = true;
		try
		{
			await _state.InitializeAsync();

			Name = _state.Campaign.Name;
			Setting = _state.Campaign.Setting;
			Goal = _state.Campaign.Goal;
			Tone = _state.Campaign.Tone;
			SelectedTone = AvailableTones.FirstOrDefault(t => t.Value == Tone);
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task SaveAsync()
	{
		var updated = new Campaign
		{
			Id = _state.Campaign.Id,
			CreatedAt = _state.Campaign.CreatedAt,
			Name = Name?.Trim() ?? "",
			Setting = Setting?.Trim() ?? "",
			Goal = Goal?.Trim() ?? "",
			Tone = (Tone?.Trim() ?? "serious").ToLowerInvariant(),
		};

		_state.UpdateCampaign(updated);
		await _state.SaveAsync();
	}
}

public record ToneOption(string Value, string Display);

