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
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class ДовідникЕлемент : ФормаЕлемент
    {
        /// <summary>
        /// Функція зворотнього виклику для вибору елементу
        /// Використовується коли потрібно новий елемент зразу вибрати
        /// </summary>
        public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

        /// <summary>
        /// Горизонтальний бокс для кнопок
        /// </summary>
        protected Box HBoxTop = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Панель з двох колонок для полів
        /// </summary>
        protected Paned HPanedTop = new Paned(Orientation.Horizontal) { BorderWidth = 5, Position = 500 };

        /// <summary>
        /// Індикатор стану блокування
        /// </summary>
        protected Label LabelLock = new Label() { UseMarkup = true, UseUnderline = false };

        public ДовідникЕлемент()
        {
            Button bSaveAndClose = new Button("Зберегти та закрити");
            bSaveAndClose.Clicked += (sender, args) => BeforeAndAfterSave(true);
            HBoxTop.PackStart(bSaveAndClose, false, false, 10);

            Button bSave = new Button("Зберегти");
            bSave.Clicked += (sender, args) => BeforeAndAfterSave();
            HBoxTop.PackStart(bSave, false, false, 10);

            Button bLock = new Button
            {
                Label = "Заблокувати",
                ImagePosition = PositionType.Left,
                AlwaysShowImage = true,
                Image = Image.NewFromIconName(Stock.Add, IconSize.Button),
            };

            bLock.Image.MarginEnd = 5;
            bLock.Clicked += (sender, args) => Lock();
            HBoxTop.PackEnd(bLock, false, false, 10);

            //Індикатор стану блокування
            HBoxTop.PackEnd(LabelLock, false, false, 10);

            PackStart(HBoxTop, false, false, 10);

            //Pack1
            Box vBox1 = new Box(Orientation.Vertical, 0);
            HPanedTop.Pack1(vBox1, false, false);
            CreatePack1(vBox1);

            //Pack2
            Box vBox2 = new Box(Orientation.Vertical, 0);
            HPanedTop.Pack2(vBox2, false, false);
            CreatePack2(vBox2);

            PackStart(HPanedTop, false, false, 5);

            ShowAll();
        }

        #region Virtual Function

        /// <summary>
        /// Лівий Блок
        /// </summary>
        protected virtual void CreatePack1(Box vBox) { }

        /// <summary>
        /// Правий Блок
        /// </summary>
        protected virtual void CreatePack2(Box vBox) { }

        /// <summary>
        /// Заблокувати
        /// </summary>
        protected virtual void Lock() { }

        #endregion

        protected void LockInfo(LockedObject_Record record)
        {
            LabelLock.Markup = record.Result ?
                $"<span color='green'>Заблоковано</span>" : //{record.UserName}, {record.DateLock}
                "<span color='red'>Тільки для читання</span>";
        }

        /// <summary>
        /// Функція обробки перед збереження та після збереження
        /// </summary>
        /// <param name="closePage"></param>
        async void BeforeAndAfterSave(bool closePage = false)
        {
            GetValue();

            Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);

            NotebookFunction.SensitiveNotebookPageToCode(notebook, this.Name, false);
            bool isSave = await Save();
            NotebookFunction.SensitiveNotebookPageToCode(notebook, this.Name, true);

            if (isSave)
            {
                if (CallBack_OnSelectPointer != null && UnigueID != null)
                    CallBack_OnSelectPointer.Invoke(UnigueID);

                CallBack_LoadRecords?.Invoke(UnigueID);

                if (closePage)
                    NotebookFunction.CloseNotebookPageToCode(notebook, this.Name);
                else
                    NotebookFunction.RenameNotebookPageToCode(notebook, Caption, this.Name);
            }
        }
    }
}