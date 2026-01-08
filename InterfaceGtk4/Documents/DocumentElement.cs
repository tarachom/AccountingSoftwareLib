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

public abstract class DocumentElement : FormElement
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
    /// Горизонтальний бокс для назви
    /// </summary>
    protected Box HBoxName = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Горизонтальний бокс для коментаря
    /// </summary>
    protected Box HBoxComment = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Панель з двох колонок (верх і низ)
    /// </summary>
    protected Paned HPanedTop = Paned.New(Orientation.Vertical);

    /// <summary>
    /// Верхній контейнер який вкладається в експандер "Реквізити шапки"
    /// </summary>
    protected Box HBoxTopContainer = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Блокнот для табличних частин і додаткових реквізитів
    /// </summary>
    protected Notebook NotebookTablePart = Notebook.New();

    /// <summary>
    /// Контейнер для додаткових реквізитів який вкладається у вкладку блокноту "Додаткові реквізити"
    /// </summary>
    protected Box HBoxOtherContainer = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Кнопки "Зберегти та провести", "Провести", "Зберегти"
    /// </summary>
    Button bSaveAndSpend = Button.NewWithLabel("Провести та закрити");
    Button bSpend = Button.NewWithLabel("Провести");
    Button bSave = Button.NewWithLabel("Зберегти");

    public DocumentElement(NotebookFunction? notebookFunc) : base(notebookFunc)
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
            new("Проводки", () =>
            {
                if (UnigueID != null) ReportSpendTheDocument(UnigueID);
            }),

            new("В журналі", async () =>
            {
                if (UnigueID != null) await InJournal(UnigueID);
            })
        ]);

        //Індикатор стану блокування
        HBoxTop.Append(LabelLockInfo);
        HBoxTop.Append(ButtonLock);

        Append(HBoxTop);

        NotebookTablePart.Scrollable = true;
        NotebookTablePart.TabPos = PositionType.Top;
        NotebookTablePart.EnablePopup = true;

        //TopBloc
        Box vBoxTop = New(Orientation.Vertical, 0);
        HPanedTop.SetStartChild(vBoxTop);
        CreateTopBloc(vBoxTop);

        //BottomBloc
        Box vBoxBottom = New(Orientation.Vertical, 0);
        HPanedTop.SetEndChild(vBoxBottom);
        CreateBottomBloc(vBoxBottom);

        HPanedTop.SetShrinkStartChild(false);
        HPanedTop.SetShrinkEndChild(false);
        HPanedTop.Position = 0;
        Append(HPanedTop);
    }

    public override async ValueTask SetValue()
    {
        //Блокування
        if (NotebookFunc != null && Element != null)
            await NotebookFunc.AddLockObjectFunc(GetName(), Element);

        await LockInfo();

        if (Element != null && !await Element.IsLock())
        {
            bSaveAndSpend.Sensitive = bSpend.Sensitive = bSave.Sensitive = false;
        }

        DefaultGrabFocus();
        await BeforeSetValue();
    }

    /// <summary>
    /// Верхній Блок
    /// </summary>
    protected virtual void CreateTopBloc(Box vBox)
    {
        vBox.Append(HBoxName);

        Expander expanderHead = Expander.New("Реквізити шапки");
        expanderHead.Expanded = true;
        expanderHead.SetChild(HBoxTopContainer);

        vBox.Append(expanderHead);

        //StarBloc
        Box vBoxStart = New(Orientation.Vertical, 0);
        vBoxStart.WidthRequest = 500;
        HBoxTopContainer.Append(vBoxStart);

        CreateTopStartBloc(vBoxStart);

        //EndBloc
        Box vBoxEnd = New(Orientation.Vertical, 0);
        vBoxEnd.WidthRequest = 500;
        HBoxTopContainer.Append(vBoxEnd);

        CreateTopEndBloc(vBoxEnd);

        vBox.Append(HBoxComment);
    }

    /// <summary>
    /// Нижній Блок
    /// </summary>
    protected virtual void CreateBottomBloc(Box vBox)
    {
        NotebookTablePart.Vexpand = /*NotebookTablePart.Hexpand=*/ true;
        vBox.Append(NotebookTablePart);

        Box vBoxPage = New(Orientation.Vertical, 0);

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.SetChild(vBoxPage);

        NotebookTablePart.AppendPage(scroll, Label.New("Додаткові реквізити"));

        vBoxPage.Append(HBoxOtherContainer);

        //StarBloc
        Box vBoxStart = New(Orientation.Vertical, 0);
        vBoxStart.WidthRequest = 500;
        HBoxOtherContainer.Append(vBoxStart);

        CreateBottomStartBloc(vBoxStart);

        //EndBloc
        Box vBoxEnd = New(Orientation.Vertical, 0);
        vBoxEnd.WidthRequest = 500;
        HBoxOtherContainer.Append(vBoxEnd);

        CreateBottomEndBloc(vBoxEnd);
    }

    protected virtual void CreateTopStartBloc(Box vBox) { }
    protected virtual void CreateTopEndBloc(Box vBox) { }
    protected virtual void CreateBottomStartBloc(Box vBox) { }
    protected virtual void CreateBottomEndBloc(Box vBox) { }

    /// <summary>
    /// Назва документу
    /// </summary>
    /// <param name="НазваДок">Назва</param>
    /// <param name="НомерДок">Номер</param>
    /// <param name="ДатаДок">Дата</param>
    protected void CreateDocName(string НазваДок, Widget НомерДок, Widget ДатаДок)
    {
        HBoxName.Append(Label.New($"{НазваДок} №:"));
        HBoxName.Append(НомерДок);
        HBoxName.Append(Label.New("від:"));
        HBoxName.Append(ДатаДок);
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
            if (CallBack_OnSelectPointer != null && UnigueID != null)
                CallBack_OnSelectPointer.Invoke(UnigueID);

            if (IsNew)
                CallBack_LoadRecords?.Invoke(UnigueID);

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
    protected abstract ValueTask<bool> SpendTheDocument(bool spendDoc);

    /// <summary>
    /// Для звіту Проводки
    /// </summary>
    protected abstract void ReportSpendTheDocument(UnigueID unigueID);

    /// <summary>
    /// Знайти в журналі
    /// </summary>
    protected virtual async ValueTask InJournal(UnigueID unigueID) { await ValueTask.FromResult(true); }

    #endregion
}