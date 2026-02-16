
using Gtk;
using AccountingSoftware;

namespace InterfaceGtk4;

public class LockControl : Box
{
    Label label = new();
    public AccountingSoftware.Object? Element { get; set; }

    public LockControl()
    {
        SetOrientation(Orientation.Horizontal);
        MarginStart = MarginEnd = 10;

        label.MarginEnd = 5;
        Append(label);

        Button button = Button.NewFromIconName("go-down");
        button.OnClicked += async (_, _) =>
        {
            if (Element != null)
            {
                LockedObject_Record recordResult = await Element.LockInfo();

                Box vBox = New(Orientation.Vertical, 0);
                Box hBox = New(Orientation.Horizontal, 0);
                vBox.Append(hBox);

                string info = recordResult.Result ? 
                    $"Заблоковано користувачем {recordResult.UserName} - {recordResult.DateLock:dd:MM:yyyy} о {recordResult.DateLock:HH:mm:ss}" : 
                    "Не заблоковано";
                hBox.Append(Label.New(info));

                Popover popover = Popover.New();
                popover.MarginStart = popover.MarginEnd = popover.MarginTop = popover.MarginBottom = 5;
                popover.SetParent(button);
                popover.SetChild(vBox);
                popover.Show();
            }
        };
        Append(button);
    }

    /// <summary>
    /// Функція для відображення інформації про блокування
    /// </summary>
    public async ValueTask LockInfo()
    {
        if (Element != null)
        {
            bool isLock = await Element.IsLock();

            string color = isLock ? "green" : "red";
            string text = isLock ? "Редагування" : "Тільки для читання";

            label.SetMarkup($"<span color='{color}'>{text}</span>");
        }
    }
}