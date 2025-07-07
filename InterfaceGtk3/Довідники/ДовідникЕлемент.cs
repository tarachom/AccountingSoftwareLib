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

namespace InterfaceGtk3;

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
    /// Кнопки "Зберегти та закрити", "Зберегти"
    /// </summary>
    Button bSaveAndClose, bSave;

    /// <summary>
    /// Індикатор стану блокування
    /// </summary>
    Label LabelLock = new Label() { UseMarkup = true, UseUnderline = false };

    /// <summary>
    /// Функція для отримання інформації про блокування
    /// </summary>
    Func<ValueTask<LockedObject_Record>>? FuncLockInfo;

    public ДовідникЕлемент()
    {
        bSaveAndClose = new Button("Зберегти та закрити");
        bSaveAndClose.Clicked += (sender, args) => BeforeAndAfterSave(true);
        HBoxTop.PackStart(bSaveAndClose, false, false, 10);

        bSave = new Button("Зберегти");
        bSave.Clicked += (sender, args) => BeforeAndAfterSave();
        HBoxTop.PackStart(bSave, false, false, 10);

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

    #endregion

    /// <summary>
    /// Функція для відображення інформації про блокування
    /// </summary>
    /// <param name="accountingObject">Обєкт</param>
    public async ValueTask LockInfo(AccountingSoftware.Object accountingObject)
    {
        bool isLock = await accountingObject.IsLock();
        bSaveAndClose.Sensitive = bSave.Sensitive = isLock;

        string color = isLock ? "green" : "red";
        string text = isLock ? "Заблоковано" : "Тільки для читання";
        LabelLock.Markup = $"<span color='{color}'>{text}</span>";

        FuncLockInfo = accountingObject.LockInfo;
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
        SpinnerOn(notebook);
        bool isSave = await Save();
        SpinnerOff(notebook);
        NotebookFunction.SensitiveNotebookPageToCode(notebook, this.Name, true);

        if (isSave)
        {
            if (CallBack_OnSelectPointer != null && UnigueID != null)
                CallBack_OnSelectPointer.Invoke(UnigueID);

            if (IsNew)
                CallBack_LoadRecords?.Invoke(UnigueID);

            if (closePage)
                NotebookFunction.CloseNotebookPageToCode(notebook, this.Name);
            else
                NotebookFunction.RenameNotebookPageToCode(notebook, Caption, this.Name);
        }
    }
}