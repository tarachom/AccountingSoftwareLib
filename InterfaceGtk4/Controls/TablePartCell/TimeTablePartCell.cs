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
/// Клітинка табличної частини - Час
/// </summary>
public class TimeTablePartCell : Box
{
    Box hBox;
    Entry entry = Entry.New();
    Button buttonSelect;

    public TimeTablePartCell()
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
        AddCssClass("time");
    }

    public TimeSpan Value
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

                if (value_ == DateTime.MinValue.TimeOfDay)
                    entry.SetText("");
                else
                    entry.SetText(value_.ToString());

                entry.TooltipText = entry.GetText();
            }
        }
    }
    TimeSpan value_ = DateTime.MinValue.TimeOfDay;

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
            value_ = DateTime.MinValue.TimeOfDay;
            OnСhanged?.Invoke();
            return;
        }

        if (TimeSpan.TryParse(entry.Text_, out TimeSpan value))
        {
            value_ = value;
            OnСhanged?.Invoke();
        }
        else
            entry.AddCssClass("error");
    }

    /// <summary>
    /// Не відображати мінімальне значення дати DateTime.MinValue
    /// </summary>
    public bool HideMinValue { get; set; } = false;

    public static TimeTablePartCell New(TimeSpan? ts)
    {
        TimeTablePartCell cell = new() { Value = ts ?? DateTime.MinValue.TimeOfDay };
        return cell;
    }

    void OnOpenSelect(object? sender, EventArgs args)
    {
        Popover popover = Popover.New();
        popover.SetParent(buttonSelect);
        popover.MarginTop = popover.MarginEnd = popover.MarginBottom = popover.MarginStart = 5;

        Box vBox = New(Orientation.Vertical, 0);

        SpinButton hourSpin = SpinButton.NewWithRange(0, 23, 1);
        hourSpin.SetOrientation(Orientation.Vertical);

        SpinButton minuteSpin = SpinButton.NewWithRange(0, 59, 1);
        minuteSpin.SetOrientation(Orientation.Vertical);

        SpinButton secondSpin = SpinButton.NewWithRange(0, 59, 1);
        secondSpin.SetOrientation(Orientation.Vertical);

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
            hourSpin.Value = Value.Hours;
            hourSpin.OnValueChanged += (_, _) => Value = new TimeSpan((int)hourSpin.Value, Value.Minutes, Value.Seconds);

            hBoxTime.Append(hourSpin);
        }

        hBoxTime.Append(AddSeparatorLabel());

        //Minute
        {
            minuteSpin.Value = Value.Minutes;
            minuteSpin.OnValueChanged += (_, _) => Value = new TimeSpan(Value.Hours, (int)minuteSpin.Value, Value.Seconds);

            hBoxTime.Append(minuteSpin);
        }

        hBoxTime.Append(AddSeparatorLabel());

        //Second
        {
            secondSpin.Value = Value.Seconds;
            secondSpin.OnValueChanged += (_, _) => Value = new TimeSpan(Value.Hours, Value.Minutes, (int)secondSpin.Value);

            hBoxTime.Append(secondSpin);
        }

        //Поточна дата
        {
            LinkButton lbCurrentDate = LinkButton.NewWithLabel("", "Поточний час");
            lbCurrentDate.OnActivateLink += (_, _) =>
            {
                Value = DateTime.Now.TimeOfDay;

                hourSpin.Value = Value.Hours;
                minuteSpin.Value = Value.Minutes;
                secondSpin.Value = Value.Seconds;

                popover.Hide();
                return true;
            };

            vBox.Append(lbCurrentDate);
        }

        popover.SetChild(vBox);
        popover.Show();
    }
}