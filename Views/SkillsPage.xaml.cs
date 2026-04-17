using QuestTodoMaui.Helpers;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

public partial class SkillsPage : ContentPage
{
	private SkillsViewModel Vm => (SkillsViewModel)BindingContext;

	public SkillsPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<SkillsViewModel>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Vm.InitializeCommand.ExecuteAsync(null);
	}
}

