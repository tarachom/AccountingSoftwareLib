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

/// <summary>
/// ДокументФормаЕлемент
/// </summary>
[GObject.Subclass<FormElement>]
public abstract partial class DocumentFormElement : FormElement
{
    /// <summary>
    /// Функція зворотнього виклику для вибору елементу
    /// Використовується коли потрібно новий елемент зразу вибрати
    /// </summary>
    public Action<UniqueID>? CallBack_OnSelectPointer { get; set; } = null;

    /// <summary>
    /// Горизонтальний бокс для кнопок
    /// </summary>
    protected Box HBoxTop = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Горизонтальний бокс для назви
    /// </summary>
    protected Box HBoxName = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Горизонтальний бокс для коментаря
    /// </summary>
    protected Box HBoxComment = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Панель з двох колонок (верх і низ)
    /// </summary>
    protected Paned HPanedTop = Paned.New(Orientation.Vertical);

    /// <summary>
    /// Верхній контейнер
    /// </summary>
    protected Box HBoxTopContainer = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Блокнот для табличних частин і додаткових реквізитів
    /// </summary>
    protected Notebook NotebookTablePart = Notebook.New();

    /// <summary>
    /// Контейнер для додаткових реквізитів який вкладається у вкладку блокноту "Додаткові реквізити"
    /// </summary>
    protected Box HBoxOtherContainer = Box.New(Orientation.Horizontal, 0);

    /// <summary>
    /// Кнопки "Зберегти та провести", "Провести", "Зберегти"
    /// </summary>
    Button bSaveAndSpend = Button.NewWithLabel("Провести та закрити");
    Button bSpend = Button.NewWithLabel("Провести");
    Button bSave = Button.NewWithLabel("Зберегти");

    partial void Initialize()
    {
        bSaveAndSpend.MarginEnd = 10;
        bSaveAndSpend.OnClicked += (_, _) => BeforeAndAfterSave(true, true);
        HBoxTop.Append(bSaveAndSpend);

        bSpend.MarginEnd = 10;
        bSpend.OnClicked += (_, _) => BeforeAndAfterSave(true);
        HBoxTop.Append(bSpend);

        bSave.MarginEnd = 10;
        bSave.OnClicked += (_, _) => BeforeAndAfterSave(false);
        HBoxTop.Append(bSave);

        //Лінки: Проводки та В журналі
        CreateLinks(HBoxTop, [
            new("Проводки", () => { if (UniqueID != null) ReportSpendTheDocument(UniqueID); }),
            new("В журналі", async () => { if (UniqueID != null) await InJournal(UniqueID); })
        ]);

        //Індикатор стану блокування
        HBoxTop.Append(LockInfo);

        HBoxTop.MarginBottom = 10;
        Append(HBoxTop);

        HBoxName.MarginBottom = 5;
        Append(HBoxName);

        BuildInterface();
    }

    public override async Task SetValue()
    {
        //Блокування
        {
            if (NotebookFunc != null && Element != null)
                await NotebookFunc.AddLockObjectFunc(GetName(), Element);

            //Інформування
            LockInfo.Element = Element;
            await LockInfo.LockInfo();

            if (Element != null && !await Element.IsLock())
                bSaveAndSpend.Sensitive = bSpend.Sensitive = bSave.Sensitive = false;
        }

        DefaultGrabFocus();

        NotebookFunc?.SpinnerOn(GetName());
        await AssignValue();
        NotebookFunc?.SpinnerOff(GetName());
    }

    /// <summary>
    /// Назва документу
    /// </summary>
    /// <param name="НазваДок">Назва</param>
    /// <param name="НомерДок">Номер</param>
    /// <param name="ДатаДок">Дата</param>
    protected void CreateDocName(string НазваДок, Widget НомерДок, Widget ДатаДок)
    {
        {
            Label label = Label.New($"{НазваДок} №:");
            label.MarginEnd = 5;
            HBoxName.Append(label);

            НомерДок.MarginEnd = 5;
            HBoxName.Append(НомерДок);
        }

        {
            Label label = Label.New("від:");
            label.MarginEnd = 5;
            HBoxName.Append(label);

            ДатаДок.MarginEnd = 5;
            HBoxName.Append(ДатаДок);
        }
    }

    /// <summary>
    /// Функція обробки перед збереження та після збереження
    /// </summary>
    /// <param name="closePage"></param>
    async void BeforeAndAfterSave(bool spendDoc, bool closePage = false)
    {
        GetValue();

        NotebookFunc?.SensitivePage(GetName(), false);
        NotebookFunc?.SpinnerOn(GetName());

        bool isSave = await Save();
        bool isSpend = (spendDoc || !IsNew) && await SpendTheDocument(isSave && spendDoc);

        NotebookFunc?.SpinnerOff(GetName());
        NotebookFunc?.SensitivePage(GetName(), true);

        if (isSave)
        {
            if (CallBack_OnSelectPointer != null && UniqueID != null)
                CallBack_OnSelectPointer.Invoke(UniqueID);

            if (IsNew)
                CallBack_LoadRecords?.Invoke(UniqueID);

            if (closePage && isSpend)
                NotebookFunc?.ClosePage(GetName());
            else
                NotebookFunc?.RenamePage(Caption, GetName());
        }
    }

    #region Abstract Function

    /// <summary>
    /// Проведення
    /// </summary>
    /// <param name="spendDoc">Провести</param>
    protected abstract Task<bool> SpendTheDocument(bool spendDoc);

    /// <summary>
    /// Для звіту Проводки
    /// </summary>
    protected abstract void ReportSpendTheDocument(UniqueID uniqueID);

    /// <summary>
    /// Знайти в журналі
    /// </summary>
    protected abstract Task InJournal(UniqueID uniqueID);

    #endregion
}