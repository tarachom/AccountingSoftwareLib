/*
Copyright (C) 2019-2026 TARAKHOMYN YURIY IVANOVYCH
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

namespace InterfaceGtk4;

public abstract class DirectoryElement : FormElement
{
    /// <summary>
    /// Функція зворотнього виклику для вибору елементу
    /// Використовується коли потрібно новий елемент зразу вибрати
    /// </summary>
    public Action<UnigueID>? CallBack_OnSelectPointer { get; set; }

    /// <summary>
    /// Горизонтальний бокс для кнопок
    /// </summary>
    protected Box HBoxTop = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Панель з двох колонок для полів
    /// </summary>
    protected Paned HPanedTop = Paned.New(Orientation.Horizontal);

    /// <summary>
    /// Кнопки "Зберегти та закрити", "Зберегти"
    /// </summary>
    Button bSaveAndClose = Button.NewWithLabel("Зберегти та закрити");
    Button bSave = Button.NewWithLabel("Зберегти");

    /// <summary>
    /// Індикатор стану блокування
    /// </summary>
    Label LabelLock = Label.New(null);

    /// <summary>
    /// Функція для отримання інформації про блокування
    /// </summary>
    Func<ValueTask<LockedObject_Record>>? FuncLockInfo;

    public DirectoryElement(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        bSaveAndClose.MarginEnd = 10;
        bSaveAndClose.OnClicked += (_, _) => BeforeAndAfterSave(true);
        HBoxTop.Append(bSaveAndClose);

        bSave.MarginEnd = 10;
        bSave.OnClicked += (_, _) => BeforeAndAfterSave();
        HBoxTop.Append(bSave);

        //Інформація про блокування
        {
            Button bLock = Button.NewFromIconName("doc");
            bLock.OnClicked += async (_, _) =>
            {
                if (FuncLockInfo != null)
                {
                    LockedObject_Record recordResult = await FuncLockInfo.Invoke();

                    Popover popover = Popover.New();
                    popover.MarginStart = popover.MarginEnd = popover.MarginTop = popover.MarginBottom = 5;
                    popover.SetParent(bLock);
                    popover.Position = PositionType.Left;

                    Box vBox = New(Orientation.Vertical, 0);
                    Box hBox = New(Orientation.Horizontal, 0);
                    vBox.Append(hBox);

                    string info = "";
                    if (recordResult.Result)
                    {
                        info += "Заблоковано" + "\n\n" +
                            "Користувач: " + recordResult.UserName + "\n" +
                            "Дата: " + recordResult.DateLock.ToString("HH:mm:ss");
                    }
                    else
                        info += "Не заблоковано";

                    hBox.Append(Label.New(info));

                    popover.SetChild(vBox);
                    popover.Show();
                }
            };

            HBoxTop.Append(bLock);

            //Індикатор стану блокування
            LabelLock.UseMarkup = true;
            LabelLock.UseUnderline = false;
            HBoxTop.Append(LabelLock);
        }

        Append(HBoxTop);

        //StarBloc
        Box vBoxStart = New(Orientation.Vertical, 0);
        HPanedTop.SetStartChild(vBoxStart);
        CreateStartBloc(vBoxStart);

        //EndBloc
        Box vBoxEnd = New(Orientation.Vertical, 0);
        HPanedTop.SetEndChild(vBoxEnd);
        CreateEndBloc(vBoxEnd);
    }

    #region Virtual Function

    /// <summary>
    /// Лівий Блок
    /// </summary>
    protected virtual void CreateStartBloc(Box vBox) { }

    /// <summary>
    /// Правий Блок
    /// </summary>
    protected virtual void CreateEndBloc(Box vBox) { }

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
        LabelLock.SetMarkup($"<span color='{color}'>{text}</span>");

        FuncLockInfo = accountingObject.LockInfo;
    }

    /// <summary>
    /// Функція обробки перед збереження та після збереження
    /// </summary>
    /// <param name="closePage"></param>
    async void BeforeAndAfterSave(bool closePage = false)
    {
        GetValue();

        NotebookFunc?.SensitivePage(GetName(), false);
        NotebookFunc?.SpinnerOn(GetName());

        bool isSave = await Save();

        NotebookFunc?.SpinnerOff(GetName());
        NotebookFunc?.SensitivePage(GetName(), true);

        if (isSave)
        {
            if (CallBack_OnSelectPointer != null && UnigueID != null)
                CallBack_OnSelectPointer.Invoke(UnigueID);

            if (IsNew)
                CallBack_LoadRecords?.Invoke(UnigueID);

            if (closePage)
                NotebookFunc?.ClosePage(GetName());
            else
                NotebookFunc?.RenamePage(Caption, GetName());
        }
    }
}