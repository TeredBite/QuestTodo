using QuestTodoMaui.Models;
using System.Globalization;

namespace QuestTodoMaui.Converters;

public class TaskStatusConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is TodoTaskStatus status)
		{
			return status switch
			{
				TodoTaskStatus.Pending => "В ожидании",
				TodoTaskStatus.Completed => "Выполнено",
				_ => "Неизвестно"
			};
		}
		return "Неизвестно";
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
