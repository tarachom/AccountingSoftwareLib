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

Пошук з виводом результатів у таб частину

*/

using Gtk;

namespace InterfaceGtk4;

public class PeriodControl : Box
{
    ComboBoxText comboBoxPeriod; //Набір варіантів періодів
    DateTimeControl dateStart = new() { OnlyDate = true, HideMinValue = true };
    DateTimeControl dateStop = new() { OnlyDate = true, Value = DateTime.Now };
    Button bSelect = Button.NewFromIconName("go-next");
    public Action? Changed { get; set; }

    public PeriodControl()
    {
        SetOrientation(Orientation.Horizontal);

        static Label AddLabel(string text)
        {
            Label label = Label.New(text);
            label.MarginEnd = 2;
            return label;
        }

        Append(AddLabel("Період з "));

        dateStart.MarginEnd = 5;
        Append(dateStart);

        Append(AddLabel(" по "));

        dateStop.MarginEnd = 5;
        Append(dateStop);

        bSelect.MarginEnd = 2;
        bSelect.OnClicked += (_, _) =>
        {
            if (Period == PeriodForJournal.TypePeriod.Special)
                Changed?.Invoke();
            else
                Period = PeriodForJournal.TypePeriod.Special;
        };
        Append(bSelect);

        comboBoxPeriod = PeriodForJournal.СписокВідбірПоПеріоду();
        comboBoxPeriod.MarginEnd = 2;
        comboBoxPeriod.OnChanged += (sender, args) =>
        {
            if (Period == PeriodForJournal.TypePeriod.AllPeriod)
            {
                dateStart.Value = DateTime.MinValue;
                dateStop.Value = DateTime.Now;
            }
            else if (Period != PeriodForJournal.TypePeriod.Special)
            {
                DateTime? dtStart = PeriodForJournal.ДатаПочатокЗПеріоду(Period);
                if (dtStart != null) dateStart.Value = dtStart.Value;

                DateTime? dtStop = PeriodForJournal.ДатаКінецьЗПеріоду(Period);
                dateStop.Value = dtStop != null ? dtStop.Value : DateTime.Now;
            }

            Changed?.Invoke();
        };
        Append(comboBoxPeriod);
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
        get => dateStart.Value;
        set => dateStart.Value = value;
    }

    public DateTime DateStop
    {
        get => dateStop.Value;
        set => dateStop.Value = value;
    }

    public DateTimeControl DateStartControl
    {
        get => dateStart;
    }

    public DateTimeControl DateStopControl
    {
        get => dateStop;
    }

    public PeriodForJournal.TypePeriod Period
    {
        get => Enum.TryParse(comboBoxPeriod.ActiveId, out PeriodForJournal.TypePeriod value) ? value : PeriodForJournal.TypePeriod.AllPeriod;
        set => comboBoxPeriod.ActiveId = value.ToString();
    }

    public bool SensitiveSelectButton
    {
        get => bSelect.Sensitive;
        set => bSelect.Sensitive = value;
    }
}