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
        DateTimeControl dateStart = new DateTimeControl() { OnlyDate = true/*, Value = DateTime.Now*/, Sensitive = false };
        DateTimeControl dateStop = new DateTimeControl() { OnlyDate = true, Value = DateTime.Now, Sensitive = false };
        Button bSelect = new Button(new Image(Stock.GoForward, IconSize.Menu)) { Sensitive = false };
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
            };

            PackStart(bSelect, false, false, 1);

            comboBoxPeriod = ПеріодДляЖурналу.СписокВідбірПоПеріоду();
            comboBoxPeriod.Changed += (object? sender, EventArgs args) =>
            {
                bool ОсобливийПеріод = Period == ПеріодДляЖурналу.ТипПеріоду.Особливий;
                dateStart.Sensitive = dateStop.Sensitive = bSelect.Sensitive = ОсобливийПеріод;

                DateTime? dateTime = ПеріодДляЖурналу.ДатаПочатокЗПеріоду(Period);

                if (dateTime != null)
                    dateStart.Value = dateTime.Value;

                if (!ОсобливийПеріод)
                    dateStop.Value = DateTime.Now;

                Changed?.Invoke();
            };

            PackStart(comboBoxPeriod, false, false, 2);
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

        /*
        #region Caption

        public string Caption
        {
            get
            {
                return labelCaption.Text;
            }
            set
            {
                labelCaption.Text = value;
            }
        }

        public string CaptionDateStart
        {
            get
            {
                return labelCaptionStart.Text;
            }
            set
            {
                labelCaptionStart.Text = value;
            }
        }

        public string CaptionDateStop
        {
            get
            {
                return labelCaptionStop.Text;
            }
            set
            {
                labelCaptionStop.Text = value;
            }
        }

        #endregion
        */
    }
}