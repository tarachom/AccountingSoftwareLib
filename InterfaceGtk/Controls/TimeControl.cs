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
    public class TimeControl : Box
    {
        Entry entryTime = new Entry();
        Box hBoxInfoValid = new Box(Orientation.Horizontal, 0) { WidthRequest = 16 };
        Button bOpenPopover;

        public TimeControl() : base(Orientation.Horizontal, 0)
        {
            PackStart(hBoxInfoValid, false, false, 1);

            entryTime.Changed += OnEntryTimeChanged;
            PackStart(entryTime, false, false, 2);

            //Button
            bOpenPopover = new Button(new Image(Іконки.ДляКнопок.Find));
            bOpenPopover.Clicked += OnOpenPopover;

            PackStart(bOpenPopover, false, false, 1);
        }

        TimeSpan mValue;
        public TimeSpan Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                entryTime.Text = mValue.ToString();
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

            if (TimeSpan.TryParse(entryTime.Text, out TimeSpan value))
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

        void OnOpenPopover(object? sender, EventArgs args)
        {
            Popover popoverCalendar = new Popover(bOpenPopover) { BorderWidth = 5 };

            Box vBox = new Box(Orientation.Vertical, 0);

            Box hBoxTime = new Box(Orientation.Horizontal, 0) { Halign = Align.Center };
            vBox.PackStart(hBoxTime, false, false, 5);

            SpinButton hourSpin = new SpinButton(0, 23, 1) { Orientation = Orientation.Vertical };
            SpinButton minuteSpin = new SpinButton(0, 59, 1) { Orientation = Orientation.Vertical };
            SpinButton secondSpin = new SpinButton(0, 59, 1) { Orientation = Orientation.Vertical };

            //Hour
            {
                hourSpin.Value = Value.Hours;
                hourSpin.ValueChanged += (object? sender, EventArgs args) =>
                {
                    Value = new TimeSpan((int)hourSpin.Value, Value.Minutes, Value.Seconds);
                };

                hBoxTime.PackStart(hourSpin, false, false, 0);
            }

            hBoxTime.PackStart(new Label(":"), false, false, 5);

            //Minute
            {
                minuteSpin.Value = Value.Minutes;
                minuteSpin.ValueChanged += (object? sender, EventArgs args) =>
                {
                    Value = new TimeSpan(Value.Hours, (int)minuteSpin.Value, Value.Seconds);
                };

                hBoxTime.PackStart(minuteSpin, false, false, 0);
            }

            hBoxTime.PackStart(new Label(":"), false, false, 5);

            //Second
            {
                secondSpin.Value = Value.Seconds;
                secondSpin.ValueChanged += (object? sender, EventArgs args) =>
                {
                    Value = new TimeSpan(Value.Hours, Value.Minutes, (int)secondSpin.Value);
                };

                hBoxTime.PackStart(secondSpin, false, false, 0);
            }

            //Поточний час
            {
                LinkButton lbCurrentDate = new LinkButton("", "Поточний час");
                lbCurrentDate.Clicked += (object? sender, EventArgs args) =>
                {
                    Value = DateTime.Now.TimeOfDay;

                    hourSpin.Value = Value.Hours;
                    minuteSpin.Value = Value.Minutes;
                    secondSpin.Value = Value.Seconds;

                    popoverCalendar.Hide();
                };

                vBox.PackStart(lbCurrentDate, false, false, 0);
            }

            popoverCalendar.Add(vBox);
            popoverCalendar.ShowAll();
        }

        void OnEntryTimeChanged(object? sender, EventArgs args)
        {
            IsValidValue();
        }
    }
}