using QuestTodoMaui.Helpers;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<LoginViewModel>();
	}
}
