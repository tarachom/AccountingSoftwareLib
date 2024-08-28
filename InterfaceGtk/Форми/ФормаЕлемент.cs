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
    /// <summary>
    /// Основа для класів ДовідникЕлемент, ДокументЕлемент, РегістриЕлемент
    /// </summary>
    public abstract class ФормаЕлемент : Box
    {
        public ФормаЕлемент() : base(Orientation.Vertical, 0) { }

        #region Create Field

        /// <summary>
        /// Створення поля із заголовком
        /// </summary>
        /// <param name="parentBox">Контейнер</param>
        /// <param name="label">Заголовок</param>
        /// <param name="field">Поле</param>
        /// <param name="halign">Положення</param>
        /// <param name="put_vbox">Вкласти в додатковий вертикальний блок</param>
        protected Box CreateField(Box parentBox, string? label, Widget field, Align halign = Align.End, bool put_vbox = false)
        {
            Box usingBox = parentBox;

            // Якщо parentBox вертикальний - створюємо додатково горизонтальний бокс
            if (parentBox.Orientation == Orientation.Vertical)
            {
                Box hBox = new Box(Orientation.Horizontal, 0) { Halign = halign };
                parentBox.PackStart(hBox, false, false, 5);

                usingBox = hBox;
            }
            // Якщо parentBox горизонтальний і потрібний додатковий вертикальний бокс
            // Створюємо додатково вертикальний та горизонтальний бокси
            else if (put_vbox)
            {
                Box vBox = new Box(Orientation.Vertical, 0);
                parentBox.PackStart(vBox, false, false, 0);

                Box hBox = new Box(Orientation.Horizontal, 0) { Halign = halign };
                vBox.PackStart(hBox, false, false, 0);

                usingBox = hBox;
            }

            if (label != null)
                usingBox.PackStart(new Label(label), false, false, 5);

            usingBox.PackStart(field, false, false, 5);

            return usingBox;
        }

        /// <summary>
        /// Добавлення поля з прокруткою
        /// </summary>
        /// <param name="vBox">Контейнер</param>
        /// <param name="label">Заголовок</param>
        /// <param name="field">Поле</param>
        /// <param name="Width">Висота</param>
        /// <param name="Height">Ширина</param>
        protected void CreateFieldView(Box vBox, string? label, Widget field, int Width = 100, int Height = 100, Align Halign = Align.End)
        {
            Box hBox = new Box(Orientation.Horizontal, 0) { Halign = Halign };
            vBox.PackStart(hBox, false, false, 5);

            if (label != null)
                hBox.PackStart(new Label(label) { Valign = Align.Start }, false, false, 5);

            ScrolledWindow scrollTextView = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = Width, HeightRequest = Height };
            scrollTextView.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTextView.Add(field);

            hBox.PackStart(scrollTextView, false, false, 5);
        }

        /// <summary>
        /// Добавлення табличної частини
        /// </summary>
        /// <param name="vBox">Контейнер</param>
        /// <param name="label">Заголовок</param>
        /// <param name="tablePart">Таб частина</param>
        protected void CreateTablePart(Box vBox, string? label, Widget tablePart)
        {
            if (label != null)
            {
                Box hBoxCaption = new Box(Orientation.Horizontal, 0);
                vBox.PackStart(hBoxCaption, false, false, 5);
                hBoxCaption.PackStart(new Label(label), false, false, 5);
            }

            Box hBox = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBox, false, false, 0);
            hBox.PackStart(tablePart, true, true, 5);
        }

        #endregion
    }
}