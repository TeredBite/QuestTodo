using QuestTodoMaui.Helpers;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

public partial class ReportsPage : ContentPage
{
	public ReportsPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<ReportsViewModel>();
	}
}
