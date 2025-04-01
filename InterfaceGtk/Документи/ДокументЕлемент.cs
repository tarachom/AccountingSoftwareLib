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
    public abstract class ДокументЕлемент : ФормаЕлемент
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
        /// Горизонтальний бокс для назви
        /// </summary>
        protected Box HBoxName = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Горизонтальний бокс для коментаря
        /// </summary>
        protected Box HBoxComment = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Панель з двох колонок
        /// </summary>
        protected Paned HPanedTop = new Paned(Orientation.Vertical) { BorderWidth = 5 };

        /// <summary>
        /// Верхній контейнер який вкладається в експандер "Реквізити шапки"
        /// </summary>
        protected Box HBoxTopContainer = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Блокнот для табличних частин і додаткових реквізитів
        /// </summary>
        protected Notebook NotebookTablePart = NotebookFunction.CreateNotebook(false);

        /// <summary>
        /// Контейнер для додаткових реквізитів який вкладається у вкладку блокноту "Додаткові реквізити"
        /// </summary>
        protected Box HBoxOtherContainer = new Box(Orientation.Horizontal, 0);

        /// <summary>
        /// Кнопки "Зберегти та провести", "Провести", "Зберегти"
        /// </summary>
        Button bSaveAndSpend, bSpend, bSave;

        /// <summary>
        /// Індикатор стану блокування
        /// </summary>
        Label LabelLock = new Label() { UseMarkup = true, UseUnderline = false };

        /// <summary>
        /// Функція для отримання інформації про блокування
        /// </summary>
        Func<ValueTask<LockedObject_Record>>? FuncLockInfo;

        public ДокументЕлемент()
        {
            bSaveAndSpend = new Button("Провести та закрити");
            bSaveAndSpend.Clicked += (sender, args) => BeforeAndAfterSave(true, true);
            HBoxTop.PackStart(bSaveAndSpend, false, false, 10);

            bSpend = new Button("Провести");
            bSpend.Clicked += (sender, args) => BeforeAndAfterSave(true);
            HBoxTop.PackStart(bSpend, false, false, 10);

            bSave = new Button("Зберегти");
            bSave.Clicked += (sender, args) => BeforeAndAfterSave(false);
            HBoxTop.PackStart(bSave, false, false, 10);

            //Проводки
            {
                LinkButton linkNew = new LinkButton("Проводки") { Halign = Align.Start, Image = new Image(Іконки.ДляКнопок.Doc), AlwaysShowImage = true };
                linkNew.Clicked += (sender, args) => { if (UnigueID != null) ReportSpendTheDocument(UnigueID); };

                HBoxTop.PackStart(linkNew, false, false, 0);
            }

            //Інформація про блокування
            {
                Button bLock = new Button
                {
                    ImagePosition = PositionType.Left,
                    AlwaysShowImage = true,
                    Image = Image.NewFromIconName(Stock.Info, IconSize.Button),
                };

                bLock.Clicked += async (sender, args) =>
                {
                    if (FuncLockInfo != null)
                    {
                        LockedObject_Record recordResult = await FuncLockInfo.Invoke();

                        Popover popover = new Popover((Button)sender!) { Position = PositionType.Left, BorderWidth = 5 };

                        Box vBox = new Box(Orientation.Vertical, 0);
                        Box hBox = new Box(Orientation.Horizontal, 0);
                        vBox.PackStart(hBox, false, false, 10);

                        string info = "";
                        if (recordResult.Result)
                        {
                            info += "Заблоковано" + "\n\n" +
                                "Користувач: " + recordResult.UserName + "\n" +
                                "Дата: " + recordResult.DateLock.ToString("HH:mm:ss");
                        }
                        else
                            info += "Не заблоковано";

                        hBox.PackStart(new Label(info), false, false, 10);

                        popover.Add(vBox);
                        popover.ShowAll();
                    }
                };

                HBoxTop.PackEnd(bLock, false, false, 10);

                //Індикатор стану блокування
                HBoxTop.PackEnd(LabelLock, false, false, 10);
            }

            PackStart(HBoxTop, false, false, 10);

            //Pack1
            Box vBox1 = new Box(Orientation.Vertical, 0);
            HPanedTop.Pack1(vBox1, false, false);
            CreatePack1(vBox1);

            //Pack2
            Box vBox2 = new Box(Orientation.Vertical, 0);
            HPanedTop.Pack2(vBox2, true, false);
            CreatePack2(vBox2);

            PackStart(HPanedTop, true, true, 0);

            ShowAll();
        }

        /// <summary>
        /// Верхній Блок
        /// </summary>
        protected virtual void CreatePack1(Box vBox)
        {
            vBox.PackStart(HBoxName, false, false, 5);

            Expander expanderHead = new Expander("Реквізити шапки") { Expanded = true };
            expanderHead.Add(HBoxTopContainer);

            vBox.PackStart(expanderHead, false, false, 5);

            //Container1
            Box vBoxContainer1 = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            HBoxTopContainer.PackStart(vBoxContainer1, false, false, 5);

            CreateContainer1(vBoxContainer1);

            //Container2
            Box vBoxContainer2 = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            HBoxTopContainer.PackStart(vBoxContainer2, false, false, 5);

            CreateContainer2(vBoxContainer2);
            // <--

            vBox.PackStart(HBoxComment, false, false, 5);
        }

        /// <summary>
        /// Нижній Блок
        /// </summary>
        protected virtual void CreatePack2(Box vBox)
        {
            vBox.PackStart(NotebookTablePart, true, true, 0);

            Box vBoxPage = new Box(Orientation.Vertical, 0);

            ScrolledWindow scroll = new ScrolledWindow();
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(vBoxPage);

            NotebookTablePart.AppendPage(scroll, new Label("Додаткові реквізити"));

            //Два блоки для полів -->
            vBoxPage.PackStart(HBoxOtherContainer, false, false, 5);

            Box vBoxContainer1 = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            HBoxOtherContainer.PackStart(vBoxContainer1, false, false, 5);

            CreateContainer3(vBoxContainer1);

            Box vBoxContainer2 = new Box(Orientation.Vertical, 0) { WidthRequest = 500 };
            HBoxOtherContainer.PackStart(vBoxContainer2, false, false, 5);

            CreateContainer4(vBoxContainer2);
            // <--
        }

        protected virtual void CreateContainer1(Box vBox) { }
        protected virtual void CreateContainer2(Box vBox) { }
        protected virtual void CreateContainer3(Box vBox) { }
        protected virtual void CreateContainer4(Box vBox) { }

        /// <summary>
        /// Назва документу
        /// </summary>
        /// <param name="НазваДок">Назва</param>
        /// <param name="НомерДок">Номер</param>
        /// <param name="ДатаДок">Дата</param>
        protected void CreateDocName(string НазваДок, Widget НомерДок, Widget ДатаДок)
        {
            HBoxName.PackStart(new Label($"{НазваДок} №:"), false, false, 5);
            HBoxName.PackStart(НомерДок, false, false, 5);
            HBoxName.PackStart(new Label("від:"), false, false, 5);
            HBoxName.PackStart(ДатаДок, false, false, 5);
        }

        /// <summary>
        /// Функція для відображення інформації про блокування
        /// </summary>
        /// <param name="accountingObject">Обєкт</param>
        public async ValueTask LockInfo(AccountingSoftware.Object accountingObject)
        {
            bool isLock = await accountingObject.IsLock();
            bSaveAndSpend.Sensitive = bSpend.Sensitive = bSave.Sensitive = isLock;

            string color = isLock ? "green" : "red";
            string text = isLock ? "Заблоковано" : "Тільки для читання";
            LabelLock.Markup = $"<span color='{color}'>{text}</span>";

            FuncLockInfo = accountingObject.LockInfo;
        }

        /// <summary>
        /// Функція обробки перед збереження та після збереження
        /// </summary>
        /// <param name="closePage"></param>
        async void BeforeAndAfterSave(bool spendDoc, bool closePage = false)
        {
            GetValue();

            Notebook? notebook = NotebookFunction.GetNotebookFromWidget(this);

            NotebookFunction.SensitiveNotebookPageToCode(notebook, this.Name, false);
            bool isSave = await Save();
            bool isSpend = (spendDoc || !IsNew) && await SpendTheDocument(isSave && spendDoc);
            NotebookFunction.SensitiveNotebookPageToCode(notebook, this.Name, true);

            if (isSave)
            {
                if (CallBack_OnSelectPointer != null && UnigueID != null)
                    CallBack_OnSelectPointer.Invoke(UnigueID);

                CallBack_LoadRecords?.Invoke(UnigueID);

                if (closePage && isSpend)
                    NotebookFunction.CloseNotebookPageToCode(notebook, this.Name);
                else
                    NotebookFunction.RenameNotebookPageToCode(notebook, Caption, this.Name);
            }
        }

        #region Abstract Function

        /// <summary>
        /// Проведення
        /// </summary>
        /// <param name="spendDoc">Провести</param>
        protected abstract ValueTask<bool> SpendTheDocument(bool spendDoc);

        /// <summary>
        /// Для звіту Проводки
        /// </summary>
        /// <param name="unigueID"></param>
        /// <returns></returns>
        protected abstract void ReportSpendTheDocument(UnigueID unigueID);

        #endregion
    }
}