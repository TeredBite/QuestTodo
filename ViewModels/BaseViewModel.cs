using CommunityToolkit.Mvvm.ComponentModel;

namespace QuestTodoMaui.ViewModels;

public partial class BaseViewModel : ObservableObject
{
	[ObservableProperty]
	private bool isBusy;

	[ObservableProperty]
	private string? error;
}

