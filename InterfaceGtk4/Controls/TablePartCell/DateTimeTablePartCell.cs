/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

/*
Автор:    Тарахомин Юрій Іванович
Адреса:   Україна, м. Львів
Сайт:     accounting.org.ua
*/

using Gtk;

namespace InterfaceGtk4;

/// <summary>
/// Клітинка табличної частини - Дата час
/// </summary>
public class DateTimeTablePartCell : Box
{
    Box hBox;
    Entry entry = Entry.New();
    Button buttonSelect;

    public DateTimeTablePartCell()
    {
        SetOrientation(Orientation.Vertical);

        hBox = New(Orientation.Horizontal, 0);
        hBox.Vexpand = true;

        entry.OnChanged += (_, _) => IsValid();
        entry.Vexpand = entry.Hexpand = true;
        hBox.Append(entry);

        //Select
        {
            buttonSelect = Button.New();
            buttonSelect.Child = Image.NewFromPixbuf(Icon.ForInformation.Grid);
            buttonSelect.OnClicked += OnOpenSelect;
            buttonSelect.TooltipText = "Вибрати";
            buttonSelect.AddCssClass("button");
            hBox.Append(buttonSelect);
        }

        Append(hBox);
        AddCssClass("datetime");
    }

    public DateTime Value
    {
        get
        {
            return value_;
        }
        set
        {
            if (value_ != value)
            {
                value_ = value;

                if (OnlyDate)
                    entry.SetMaxWidthChars(10);

                if (value_.Date == DateTime.MinValue.Date)
                    entry.SetText("");
                else if (OnlyDate)
                {
                    value_ = value_.Date;
                    entry.SetText(value_.ToString("dd.MM.yyyy"));
                }
                else
                    entry.SetText(value_.ToString("dd.MM.yyyy HH:mm:ss"));

                //Підказка
                entry.TooltipText = entry.GetText();
            }
        }
    }
    DateTime value_ = DateTime.MinValue;

    /// <summary>
    /// Функція яка викликається після зміни
    /// </summary>
    public Action? OnСhanged { get; set; }

    void IsValid()
    {
        foreach (var cssclass in entry.CssClasses)
            entry.RemoveCssClass(cssclass);

        if (string.IsNullOrEmpty(entry.Text_))
        {
            value_ = DateTime.MinValue;
            OnСhanged?.Invoke();
            return;
        }

        if (DateTime.TryParse(entry.Text_, out DateTime value))
        {
            value_ = value;
            OnСhanged?.Invoke();
        }
        else
            entry.AddCssClass("error");
    }

    public static DateTimeTablePartCell New(DateTime? dt)
    {
        DateTimeTablePartCell cell = new() { Value = dt ?? DateTime.MinValue };
        return cell;
    }

    /// <summary>
    /// Тільки дата, без часу
    /// </summary>
    public bool OnlyDate { get; set; } = false;

    /// <summary>
    /// Нова дата із поля Value 
    /// 
    /// Message.Info(null, null, datetime?.Format("%d.%m.%Y %H:%M:%S") ?? "");
    /// </summary>
    /// <returns>GLib.DateTime?</returns>
    GLib.DateTime? GetGLibDateTime() => GLib.DateTime.NewLocal(Value.Year, Value.Month, Value.Day, Value.Hour, Value.Minute, Value.Second);

    void OnOpenSelect(object? sender, EventArgs args)
    {
        Popover popover = Popover.New();
        popover.SetParent(buttonSelect);
        popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 5;

        Box vBox = New(Orientation.Vertical, 0);

        Calendar calendar = Calendar.New();

        GLib.DateTime? datetime = GetGLibDateTime();
        if (datetime != null) calendar.SelectDay(datetime);

        calendar.OnDaySelected += (_, _) =>
        {
            try
            {
                Value = new DateTime(calendar.GetDate().GetYear(), calendar.GetDate().GetMonth(), calendar.GetDate().GetDayOfMonth(), Value.Hour, Value.Minute, Value.Second);
            }
            catch
            {
                Value = DateTime.MinValue;
            }
        };

        GestureClick gesture = GestureClick.New();
        gesture.OnPressed += (_, args) => { if (args.NPress >= 2) popover.Hide(); };
        calendar.AddController(gesture);

        vBox.Append(calendar);

        SpinButton hourSpin = SpinButton.NewWithRange(0, 23, 1);
        hourSpin.SetOrientation(Orientation.Vertical);

        SpinButton minuteSpin = SpinButton.NewWithRange(0, 59, 1);
        minuteSpin.SetOrientation(Orientation.Vertical);

        SpinButton secondSpin = SpinButton.NewWithRange(0, 59, 1);
        secondSpin.SetOrientation(Orientation.Vertical);

        if (!OnlyDate)
        {
            static Label AddSeparatorLabel()
            {
                Label label = Label.New(":");
                label.MarginStart = label.MarginEnd = 5;
                return label;
            }

            Box hBoxTime = New(Orientation.Horizontal, 0);
            hBoxTime.Halign = Align.Center;
            hBoxTime.MarginTop = hBoxTime.MarginEnd = 5;
            vBox.Append(hBoxTime);

            //Hour
            {
                hourSpin.Value = TimeOnly.FromDateTime(Value).Hour;
                hourSpin.OnValueChanged += (_, _) => Value = new DateTime(
                    calendar.GetDate().GetYear(), calendar.GetDate().GetMonth(), calendar.GetDate().GetDayOfMonth(),
                    (int)hourSpin.Value, Value.Minute, Value.Second);

                hBoxTime.Append(hourSpin);
            }

            hBoxTime.Append(AddSeparatorLabel());

            //Minute
            {
                minuteSpin.Value = TimeOnly.FromDateTime(Value).Minute;
                minuteSpin.OnValueChanged += (_, _) => Value = new DateTime(
                    calendar.GetDate().GetYear(), calendar.GetDate().GetMonth(), calendar.GetDate().GetDayOfMonth(),
                    Value.Hour, (int)minuteSpin.Value, Value.Second);

                hBoxTime.Append(minuteSpin);
            }

            hBoxTime.Append(AddSeparatorLabel());

            //Second
            {
                secondSpin.Value = TimeOnly.FromDateTime(Value).Second;
                secondSpin.OnValueChanged += (_, _) => Value = new DateTime(
                    calendar.GetDate().GetYear(), calendar.GetDate().GetMonth(), calendar.GetDate().GetDayOfMonth(),
                    Value.Hour, Value.Minute, (int)secondSpin.Value);

                hBoxTime.Append(secondSpin);
            }
        }

        //Поточна дата
        {
            LinkButton lbCurrentDate = LinkButton.NewWithLabel("", "Поточна дата");
            lbCurrentDate.OnActivateLink += (_, _) =>
            {
                Value = DateTime.Now;
                TimeSpan timeOfDay = Value.TimeOfDay;

                GLib.DateTime? datetime = GetGLibDateTime();
                if (datetime != null) calendar.SelectDay(datetime);

                hourSpin.Value = timeOfDay.Hours;
                minuteSpin.Value = timeOfDay.Minutes;
                secondSpin.Value = timeOfDay.Seconds;

                popover.Hide();
                return true;
            };

            vBox.Append(lbCurrentDate);
        }

        popover.SetChild(vBox);
        popover.Show();
    }
}