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

namespace InterfaceGtk
{
    public class DateTimeControl : Box
    {
        Entry entryDateTimeValue = new Entry();
        Box hBoxInfoValid = new Box(Orientation.Horizontal, 0) { WidthRequest = 16 };
        Button bOpenCalendar;

        public DateTimeControl() : base(Orientation.Horizontal, 0)
        {
            PackStart(hBoxInfoValid, false, false, 1);

            //Entry
            entryDateTimeValue.Changed += OnEntryDateTimeChanged;
            PackStart(entryDateTimeValue, false, false, 1);

            //Button
            bOpenCalendar = new Button(new Image(Іконки.ДляКнопок.Find));
            bOpenCalendar.Clicked += OnOpenCalendar;

            PackStart(bOpenCalendar, false, false, 1);
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
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;

                if (HideMinValue && mValue.Date == DateTime.MinValue.Date)
                    entryDateTimeValue.Text = "";
                else if (OnlyDate)
                {
                    mValue = mValue.Date;
                    entryDateTimeValue.Text = mValue.ToString("dd.MM.yyyy");
                    entryDateTimeValue.WidthChars = 10;
                }
                else
                    entryDateTimeValue.Text = mValue.ToString("dd.MM.yyyy HH:mm:ss");
            }
        }

        public DateTime ПочатокДня()
        {
            return new DateTime(Value.Year, Value.Month, Value.Day, 0, 0, 0);
        }

        public DateTime КінецьДня()
        {
            return new DateTime(Value.Year, Value.Month, Value.Day, 23, 59, 59);
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
                return false;

            if (DateTime.TryParse(entryDateTimeValue.Text, out DateTime value))
            {
                mValue = value;

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

        void OnOpenCalendar(object? sender, EventArgs args)
        {
            Popover popoverCalendar = new Popover(bOpenCalendar) { BorderWidth = 5 };

            Box vBox = new Box(Orientation.Vertical, 0);

            //Calendar
            Calendar calendar = new Calendar() { Date = Value };

            calendar.DaySelected += (object? sender, EventArgs args) =>
            {
                Value = new DateTime(
                    calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                    Value.Hour, Value.Minute, Value.Second);
            };

            calendar.DaySelectedDoubleClick += (object? sender, EventArgs args) =>
            {
                popoverCalendar.Hide();
            };

            vBox.PackStart(calendar, false, false, 0);

            SpinButton hourSpin = new SpinButton(0, 23, 1) { Orientation = Orientation.Vertical };
            SpinButton minuteSpin = new SpinButton(0, 59, 1) { Orientation = Orientation.Vertical };
            SpinButton secondSpin = new SpinButton(0, 59, 1) { Orientation = Orientation.Vertical };

            if (!OnlyDate)
            {
                Box hBoxTime = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
                vBox.PackStart(hBoxTime, false, false, 5);

                //Hour
                {
                    hourSpin.Value = TimeOnly.FromDateTime(Value).Hour;
                    hourSpin.ValueChanged += (object? sender, EventArgs args) =>
                    {
                        Value = new DateTime(
                            calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                            (int)hourSpin.Value, Value.Minute, Value.Second);
                    };

                    hBoxTime.PackStart(hourSpin, false, false, 0);
                }

                hBoxTime.PackStart(new Label(":"), false, false, 5);

                //Minute
                {
                    minuteSpin.Value = TimeOnly.FromDateTime(Value).Minute;
                    minuteSpin.ValueChanged += (object? sender, EventArgs args) =>
                    {
                        Value = new DateTime(
                            calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                            Value.Hour, (int)minuteSpin.Value, Value.Second);
                    };

                    hBoxTime.PackStart(minuteSpin, false, false, 0);
                }

                hBoxTime.PackStart(new Label(":"), false, false, 5);

                //Second
                {
                    secondSpin.Value = TimeOnly.FromDateTime(Value).Second;
                    secondSpin.ValueChanged += (object? sender, EventArgs args) =>
                    {
                        Value = new DateTime(
                            calendar.Date.Year, calendar.Date.Month, calendar.Date.Day,
                            Value.Hour, Value.Minute, (int)secondSpin.Value);
                    };

                    hBoxTime.PackStart(secondSpin, false, false, 0);
                }
            }

            //Поточна дата
            {
                LinkButton lbCurrentDate = new LinkButton("", "Поточна дата");
                lbCurrentDate.Clicked += (object? sender, EventArgs args) =>
                {
                    calendar.Date = Value = DateTime.Now;

                    hourSpin.Value = TimeOnly.FromDateTime(Value).Hour;
                    minuteSpin.Value = TimeOnly.FromDateTime(Value).Minute;
                    secondSpin.Value = TimeOnly.FromDateTime(Value).Second;

                    popoverCalendar.Hide();
                };

                vBox.PackStart(lbCurrentDate, false, false, 0);
            }

            popoverCalendar.Add(vBox);
            popoverCalendar.ShowAll();
        }

        void OnEntryDateTimeChanged(object? sender, EventArgs args)
        {
            IsValidValue();
        }
    }
}