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

/*

Форма для перегляду дати для таб частини

*/

using Gtk;

namespace InterfaceGtk3;

public class СпільніФорми_РедагуванняДати : ФормаСпливаючеВікно
{
    Entry entryDateTimeValue = new Entry();
    Box hBoxInfoValid = new Box(Orientation.Horizontal, 0) { WidthRequest = 16 };

    Calendar calendar = new Calendar();
    SpinButton hourSpin = new SpinButton(0, 23, 1) { Orientation = Orientation.Vertical };
    SpinButton minuteSpin = new SpinButton(0, 59, 1) { Orientation = Orientation.Vertical };
    SpinButton secondSpin = new SpinButton(0, 59, 1) { Orientation = Orientation.Vertical };

    public System.Action<DateTime>? CallBack_Update = null;

    public СпільніФорми_РедагуванняДати()
    {
        //Entry
        {
            Box hBox = new Box(Orientation.Horizontal, 2);
            PackStart(hBox, false, false, 5);

            hBox.PackStart(hBoxInfoValid, false, false, 1);

            entryDateTimeValue.Changed += OnEntryDateTimeChanged;
            hBox.PackStart(entryDateTimeValue, false, false, 1);
        }

        //Календар
        {
            Box hBox = new Box(Orientation.Horizontal, 0);
            PackStart(hBox, true, true, 0);

            Box vBox = new Box(Orientation.Vertical, 0);
            hBox.PackStart(vBox, true, true, 0);

            calendar.DaySelected += (sender, args) =>
            {
                try
                {
                    Value = new DateTime(calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                        Value.Hour, Value.Minute, Value.Second);
                }
                catch
                {
                    Value = DateTime.MinValue;
                }
            };

            calendar.DaySelectedDoubleClick += (sender, args) =>
            {
                CallBack_Update?.Invoke(this.Value);
                PopoverParent?.Hide();
            };

            vBox.PackStart(calendar, true, true, 0);

            Box hBoxTime = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            vBox.PackStart(hBoxTime, false, false, 5);

            //Hour
            {
                hourSpin.ValueChanged += (sender, args) => Value = new DateTime(
                    calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                    (int)hourSpin.Value, Value.Minute, Value.Second);

                hBoxTime.PackStart(hourSpin, false, false, 0);
            }

            hBoxTime.PackStart(new Label(":"), false, false, 5);

            //Minute
            {
                minuteSpin.ValueChanged += (sender, args) => Value = new DateTime(
                    calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                    Value.Hour, (int)minuteSpin.Value, Value.Second);

                hBoxTime.PackStart(minuteSpin, false, false, 0);
            }

            hBoxTime.PackStart(new Label(":"), false, false, 5);

            //Second
            {
                secondSpin.ValueChanged += (sender, args) => Value = new DateTime(
                    calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                    Value.Hour, Value.Minute, (int)secondSpin.Value);

                hBoxTime.PackStart(secondSpin, false, false, 0);
            }

            //Поточна дата
            {
                LinkButton lbCurrentDate = new LinkButton("", "Поточна дата");
                lbCurrentDate.Clicked += (sender, args) =>
                {
                    calendar.Date = Value = DateTime.Now;

                    hourSpin.Value = TimeOnly.FromDateTime(Value).Hour;
                    minuteSpin.Value = TimeOnly.FromDateTime(Value).Minute;
                    secondSpin.Value = TimeOnly.FromDateTime(Value).Second;
                };

                vBox.PackStart(lbCurrentDate, false, false, 0);
            }
        }

        //Сепаратор
        {
            PackStart(new Separator(Orientation.Horizontal), false, false, 2);
        }

        //Кнопка
        {
            Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            PackStart(hBox, false, false, 2);

            Button bSave = new Button("Зберегти");
            bSave.Clicked += (sender, args) =>
            {
                CallBack_Update?.Invoke(this.Value);
                PopoverParent?.Hide();
            };
            hBox.PackStart(bSave, false, false, 0);
        }
    }

    bool isInit = true;

    DateTime _Value;
    public DateTime Value
    {
        get
        {
            return _Value;
        }
        set
        {
            _Value = value;

            entryDateTimeValue.Text = _Value.ToString("dd.MM.yyyy HH:mm:ss");

            if (isInit)
            {
                isInit = false;

                calendar.Date = _Value;
                hourSpin.Value = TimeOnly.FromDateTime(_Value).Hour;
                minuteSpin.Value = TimeOnly.FromDateTime(_Value).Minute;
                secondSpin.Value = TimeOnly.FromDateTime(_Value).Second;
            }
        }
    }

    void ClearHBoxInfoValid()
    {
        foreach (Widget item in hBoxInfoValid.Children)
            hBoxInfoValid.Remove(item);
    }

    public bool IsValidValue()
    {
        ClearHBoxInfoValid();

        if (string.IsNullOrEmpty(entryDateTimeValue.Text))
        {
            _Value = DateTime.MinValue;
            return false;
        }

        if (DateTime.TryParse(entryDateTimeValue.Text, out DateTime value))
        {
            _Value = value;

            hBoxInfoValid.Add(new Image(Іконки.ДляІнформування.Ok));
            hBoxInfoValid.ShowAll();

            return true;
        }
        else
        {
            hBoxInfoValid.Add(new Image(Іконки.ДляІнформування.Error));
            hBoxInfoValid.ShowAll();

            return false;
        }
    }

    void OnEntryDateTimeChanged(object? sender, EventArgs args)
    {
        IsValidValue();
    }
}