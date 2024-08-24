/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

Пошук з виводом результатів у таб частину

*/

using Gtk;

namespace InterfaceGtk
{
    public class PeriodControl : Box
    {
        ComboBoxText comboBoxPeriod; //Набір варіантів періодів
        DateTimeControl dateStart = new DateTimeControl() { OnlyDate = true, HideMinValue = true };
        DateTimeControl dateStop = new DateTimeControl() { OnlyDate = true, Value = DateTime.Now };
        Button bSelect = new Button(new Image(Stock.GoForward, IconSize.Menu)) { };
        public System.Action? Changed { get; set; }

        public PeriodControl() : base(Orientation.Horizontal, 0)
        {
            PackStart(new Label("Період з "), false, false, 5);
            PackStart(dateStart, false, false, 2);

            PackStart(new Label(" по "), false, false, 5);
            PackStart(dateStop, false, false, 2);

            bSelect.Clicked += (object? sender, EventArgs args) =>
            {
                if (Period == ПеріодДляЖурналу.ТипПеріоду.Особливий)
                    Changed?.Invoke();
                else
                    Period = ПеріодДляЖурналу.ТипПеріоду.Особливий;
            };

            PackStart(bSelect, false, false, 1);

            comboBoxPeriod = ПеріодДляЖурналу.СписокВідбірПоПеріоду();
            comboBoxPeriod.Changed += (object? sender, EventArgs args) =>
            {
                if (Period == ПеріодДляЖурналу.ТипПеріоду.ВесьПеріод)
                    dateStart.Value = DateTime.MinValue;
                else
                {
                    DateTime? dateTime = ПеріодДляЖурналу.ДатаПочатокЗПеріоду(Period);
                    if (dateTime != null)
                        dateStart.Value = dateTime.Value;
                }

                if (Period != ПеріодДляЖурналу.ТипПеріоду.Особливий)
                    dateStop.Value = DateTime.Now;

                Changed?.Invoke();
            };

            PackStart(comboBoxPeriod, false, false, 2);
        }

        public bool OnlyDate
        {
            set
            {
                dateStart.OnlyDate = value;
                dateStop.OnlyDate = value;
            }
        }

        public DateTime DateStart
        {
            get
            {
                return dateStart.Value;
            }
            set
            {
                dateStart.Value = value;
            }
        }

        public DateTime DateStop
        {
            get
            {
                return dateStop.Value;
            }
            set
            {
                dateStop.Value = value;
            }
        }

        public DateTimeControl DateStartControl
        {
            get
            {
                return dateStart;
            }
        }

        public DateTimeControl DateStopControl
        {
            get
            {
                return dateStop;
            }
        }

        public ПеріодДляЖурналу.ТипПеріоду Period
        {
            get
            {
                return Enum.Parse<ПеріодДляЖурналу.ТипПеріоду>(comboBoxPeriod.ActiveId);
            }
            set
            {
                comboBoxPeriod.ActiveId = value.ToString();
            }
        }

        public bool SensitiveSelectButton
        {
            get
            {
                return bSelect.Sensitive;
            }
            set
            {
                bSelect.Sensitive = value;
            }
        }
    }
}