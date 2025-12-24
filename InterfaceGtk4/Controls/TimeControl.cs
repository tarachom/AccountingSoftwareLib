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

public class TimeControl : Box
{
    Entry entryTime = new();
    Box hBoxInfoValid = New(Orientation.Horizontal, 0);
    Button bOpenPopover;

    public TimeControl()
    {
        SetOrientation(Orientation.Horizontal);

        //Info box valid
        hBoxInfoValid.WidthRequest = 16;
        hBoxInfoValid.MarginEnd = 2;
        Append(hBoxInfoValid);

        //Entry
        entryTime.OnChanged += (_, _) => IsValidValue();
        entryTime.MarginEnd = 2;
        Append(entryTime);

        //Button
        bOpenPopover = Button.New();
        bOpenPopover.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
        bOpenPopover.OnClicked += OnOpenPopover;
        Append(bOpenPopover);
    }

    TimeSpan mValue;
    public TimeSpan Value
    {
        get => mValue;
        set
        {
            mValue = value;
            entryTime.Text_ = mValue.ToString();
            entryTime.TooltipText = entryTime.Text_;
        }
    }

    void ClearHBoxInfoValid()
    {
        Widget? child = hBoxInfoValid.GetFirstChild();
        if (child != null) hBoxInfoValid.Remove(child);
    }

    public bool IsValidValue()
    {
        ClearHBoxInfoValid();

        if (string.IsNullOrEmpty(entryTime.Text_))
        {
            mValue = TimeSpan.MinValue;
            return true;
        }

        if (TimeSpan.TryParse(entryTime.Text_, out TimeSpan value))
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

    void OnOpenPopover(object? sender, EventArgs args)
    {
        Popover popoverCalendar = Popover.New();
        popoverCalendar.SetParent(bOpenPopover);
        popoverCalendar.MarginTop = popoverCalendar.MarginEnd = popoverCalendar.MarginBottom = popoverCalendar.MarginStart = 5;

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

                popoverCalendar.Hide();
                return true;
            };

            vBox.Append(lbCurrentDate);
        }

        popoverCalendar.SetChild(vBox);
        popoverCalendar.Show();
    }

}