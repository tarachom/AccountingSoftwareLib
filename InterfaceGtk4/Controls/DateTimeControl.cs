/*
Copyright (C) 2019-2025 TARAKHOMYN YURIY IVANOVYCH
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

public class DateTimeControl : Box
{
    Entry entryDateTimeValue = new();
    Box hBoxInfoValid = New(Orientation.Horizontal, 0);
    Button bOpenCalendar;

    public DateTimeControl()
    {
        SetOrientation(Orientation.Horizontal);

        //Info box valid
        hBoxInfoValid.WidthRequest = 16;
        hBoxInfoValid.MarginEnd = 2;
        Append(hBoxInfoValid);

        //Entry
        entryDateTimeValue.OnChanged += (_, _) => IsValidValue();
        entryDateTimeValue.MarginEnd = 2;
        Append(entryDateTimeValue);

        //Button
        bOpenCalendar = Button.New();
        bOpenCalendar.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
        bOpenCalendar.OnClicked += OnOpenCalendar;
        Append(bOpenCalendar);
    }

    /// <summary>
    /// Тільки дата, без часу
    /// </summary>
    public bool OnlyDate { get; set; } = false;

    /// <summary>
    /// Не відображати мінімальне значення дати DateTime.MinValue
    /// </summary>
    public bool HideMinValue { get; set; } = false;

    DateTime mValue;
    public DateTime Value
    {
        get => mValue;
        set
        {
            mValue = value;

            if (HideMinValue && mValue.Date == DateTime.MinValue.Date)
                entryDateTimeValue.Text_ = "";
            else if (OnlyDate)
            {
                mValue = mValue.Date;
                entryDateTimeValue.Text_ = mValue.ToString("dd.MM.yyyy");
            }
            else
                entryDateTimeValue.Text_ = mValue.ToString("dd.MM.yyyy HH:mm:ss");

            //Підказка
            entryDateTimeValue.TooltipText = entryDateTimeValue.Text_;
        }
    }

    /// <summary>
    /// Початок дня
    /// </summary>
    /// <returns>Дата початку дня</returns>
    public DateTime DayBeginning() => new(Value.Year, Value.Month, Value.Day, 0, 0, 0);

    /// <summary>
    /// Кінець дня
    /// </summary>
    /// <returns>Дата кінця дня</returns>
    public DateTime DayEnd() => new(Value.Year, Value.Month, Value.Day, 23, 59, 59);

    void ClearHBoxInfoValid()
    {
        Widget? child = hBoxInfoValid.GetFirstChild();
        if (child != null) hBoxInfoValid.Remove(child);
    }

    public bool IsValidValue()
    {
        ClearHBoxInfoValid();

        if (string.IsNullOrEmpty(entryDateTimeValue.Text_))
        {
            mValue = DateTime.MinValue;
            return true;
        }

        if (DateTime.TryParse(entryDateTimeValue.Text_, out DateTime value))
        {
            mValue = value;

            hBoxInfoValid.Append(Image.NewFromPixbuf(Icon.ForInformation.Ok));
            return true;
        }
        else
        {
            hBoxInfoValid.Append(Image.NewFromPixbuf(Icon.ForInformation.Error));
            return false;
        }
    }

    /// <summary>
    /// Нова дата із поля Value 
    /// 
    /// Message.Info(null, null, datetime?.Format("%d.%m.%Y %H:%M:%S") ?? "");
    /// </summary>
    /// <returns>GLib.DateTime?</returns>
    GLib.DateTime? GetGLibDateTime() => GLib.DateTime.NewLocal(Value.Year, Value.Month, Value.Day, Value.Hour, Value.Minute, Value.Second);

    void OnOpenCalendar(object? sender, EventArgs args)
    {
        Popover popoverCalendar = Popover.New();
        popoverCalendar.SetParent(bOpenCalendar);
        popoverCalendar.MarginTop = popoverCalendar.MarginEnd = popoverCalendar.MarginBottom = popoverCalendar.MarginStart = 5;

        Box vBox = New(Orientation.Vertical, 0);

        Calendar calendar = Calendar.New();

        GLib.DateTime? datetime = GetGLibDateTime();
        if (datetime != null) calendar.Date = datetime;

        calendar.OnDaySelected += (_, _) =>
        {
            try
            {
                Value = new DateTime(calendar.Date.GetYear(), calendar.Date.GetMonth(), calendar.Date.GetDayOfMonth(),
                    Value.Hour, Value.Minute, Value.Second);
            }
            catch
            {
                Value = DateTime.MinValue;
            }
        };

        GestureClick gesture = GestureClick.New();
        gesture.OnPressed += (_, args) => { if (args.NPress >= 2) popoverCalendar.Hide(); };
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
                    calendar.Date.GetYear(), calendar.Date.GetMonth(), calendar.Date.GetDayOfMonth(),
                    (int)hourSpin.Value, Value.Minute, Value.Second);

                hBoxTime.Append(hourSpin);
            }

            hBoxTime.Append(AddSeparatorLabel());

            //Minute
            {
                minuteSpin.Value = TimeOnly.FromDateTime(Value).Minute;
                minuteSpin.OnValueChanged += (_, _) => Value = new DateTime(
                    calendar.Date.GetYear(), calendar.Date.GetMonth(), calendar.Date.GetDayOfMonth(),
                    Value.Hour, (int)minuteSpin.Value, Value.Second);

                hBoxTime.Append(minuteSpin);
            }

            hBoxTime.Append(AddSeparatorLabel());

            //Second
            {
                secondSpin.Value = TimeOnly.FromDateTime(Value).Second;
                secondSpin.OnValueChanged += (_, _) => Value = new DateTime(
                    calendar.Date.GetYear(), calendar.Date.GetMonth(), calendar.Date.GetDayOfMonth(),
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
                GLib.DateTime? datetime = GetGLibDateTime();
                if (datetime != null) calendar.Date = datetime;

                hourSpin.Value = TimeOnly.FromDateTime(Value).Hour;
                minuteSpin.Value = TimeOnly.FromDateTime(Value).Minute;
                secondSpin.Value = TimeOnly.FromDateTime(Value).Second;

                popoverCalendar.Hide();
                return true;
            };

            vBox.Append(lbCurrentDate);
        }

        popoverCalendar.SetChild(vBox);
        popoverCalendar.Show();
    }
}