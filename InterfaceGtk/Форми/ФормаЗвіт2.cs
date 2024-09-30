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

using Gtk;

namespace InterfaceGtk
{
    public abstract class ФормаЗвіт2 : Форма
    {
        /// <summary>
        /// Верхній блок для кнопок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Верхній блок для періоду
        /// </summary>
        protected Box HBoxPeriod = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Верхній блок фільтрів
        /// </summary>
        protected Box HBoxFilter = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Верхній блок Notebook
        /// </summary>
        protected Box HBoxNotebook = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Період
        /// </summary>
        protected PeriodControl Період = new PeriodControl() { SensitiveSelectButton = false };

        /// <summary>
        /// Блокнот для виводу звітів
        /// </summary>
        protected Notebook Notebook { get; }

        public ФормаЗвіт2()
        {
            //Кнопки
            PackStart(HBoxTop, false, false, 10);

            //Період
            PackStart(HBoxPeriod, false, false, 5);

            Період.Changed = PeriodChanged;
            HBoxPeriod.PackStart(Період, false, false, 5);

            //Фільтри
            PackStart(HBoxFilter, false, false, 5);
            CreateFilters();

            //Блокнот
            PackStart(HBoxNotebook, true, true, 5);

            Notebook = NotebookFunction.CreateNotebook();
            HBoxNotebook.PackStart(Notebook, true, true, 5);
        }

        void CreateFilters()
        {
            Box hBox = new Box(Orientation.Horizontal, 0);

            Expander expander = new Expander("Відбори") { Expanded = true };
            expander.Add(hBox);

            //Container1
            Box vBox1 = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            hBox.PackStart(vBox1, false, false, 5);

            CreateContainer1(vBox1);

            //Container2
            Box vBox2 = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            hBox.PackStart(vBox2, false, false, 5);

            CreateContainer2(vBox2);

            HBoxFilter.PackStart(expander, false, false, 10);
        }

        #region Virtual & Abstract Function

        protected virtual void CreateContainer1(Box vBox) { }
        protected virtual void CreateContainer2(Box vBox) { }
        public abstract ValueTask SetValue();
        protected abstract void PeriodChanged();

        #endregion
    }
}