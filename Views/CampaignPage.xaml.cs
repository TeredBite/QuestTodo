using QuestTodoMaui.Helpers;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

public partial class CampaignPage : ContentPage
{
	private CampaignViewModel Vm => (CampaignViewModel)BindingContext;

	public CampaignPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<CampaignViewModel>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await Vm.InitializeCommand.ExecuteAsync(null);
	}
}

