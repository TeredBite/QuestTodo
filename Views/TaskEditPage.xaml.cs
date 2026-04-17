using QuestTodoMaui.Helpers;
using QuestTodoMaui.ViewModels;

namespace QuestTodoMaui.Views;

[QueryProperty(nameof(TaskId), "taskId")]
public partial class TaskEditPage : ContentPage
{
	private TaskEditViewModel Vm => (TaskEditViewModel)BindingContext;

	public string? TaskId { get; set; }

	public TaskEditPage()
	{
		InitializeComponent();
		BindingContext = ServiceHelper.GetRequiredService<TaskEditViewModel>();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		Guid? id = null;
		if (!string.IsNullOrWhiteSpace(TaskId) && Guid.TryParse(TaskId, out var guid))
			id = guid;

		await Vm.LoadAsync(id);
	}
}

