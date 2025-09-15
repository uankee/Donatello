namespace Donatello.Models;

public enum ToastType { success, info, warning, danger }

public class ToastModel
{
    public string Message { get; set; }
    public ToastType Type { get; set; }

    public ToastModel(string message = "Action completed", ToastType type = ToastType.success)
    {
        Message = message;
        Type = type;
    }
}
