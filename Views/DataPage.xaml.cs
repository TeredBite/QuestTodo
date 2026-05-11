using QuestTodoMaui.Helpers;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

public partial class DataPage : ContentPage
{
	public DataPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<DataViewModel>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		if (BindingContext is DataViewModel vm)
		{
			await vm.LoadCommand.ExecuteAsync(null);
		}
	}
}
