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

namespace InterfaceGtk4;

/// <summary>
/// Основа для сторінок в конфігураторі
///     
/// </summary>
public abstract class FormPageConfigurator : Form
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsNew { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ConfName { get; set; } = "";

    /// <summary>
    /// 
    /// </summary>
    public string Caption { get; set; } = "";

    /// <summary>
    /// Верхній блок для кнопок
    /// </summary>
    protected Box HBoxTop = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Середній блок
    /// </summary>
    //protected Box HBoxBody = New(Orientation.Horizontal, 0);

    /// <summary>
    /// Панель з двох колонок для полів
    /// </summary>
    protected Paned HPanedTop = Paned.New(Orientation.Horizontal);

    /// <summary>
    /// Кнопки "Зберегти та закрити", "Зберегти"
    /// </summary>
    protected Button bSaveAndClose = Button.NewWithLabel("Зберегти та закрити");
    protected Button bSave = Button.NewWithLabel("Зберегти");

    public FormPageConfigurator(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        bSaveAndClose.MarginEnd = 10;
        bSaveAndClose.OnClicked += (_, _) => BeforeAndAfterSave(true);
        HBoxTop.Append(bSaveAndClose);

        bSave.MarginEnd = 10;
        bSave.OnClicked += (_, _) => BeforeAndAfterSave();
        HBoxTop.Append(bSave);

        //Кнопки
        HBoxTop.MarginBottom = 10;
        Append(HBoxTop);

        //StarBloc
        Box vBoxStart = New(Orientation.Vertical, 0);
        HPanedTop.SetStartChild(vBoxStart);
        CreateStart(vBoxStart);

        //EndBloc
        Box vBoxEnd = New(Orientation.Vertical, 0);
        HPanedTop.SetEndChild(vBoxEnd);
        CreateEnd(vBoxEnd);

        HPanedTop.SetShrinkStartChild(false);
        HPanedTop.SetShrinkEndChild(false);
        HPanedTop.Position = 500;
        Append(HPanedTop);
    }

    async void BeforeAndAfterSave(bool closePage = false)
    {
        await GetValue();

        NotebookFunc?.SensitivePage(GetName(), false);
        NotebookFunc?.SpinnerOn(GetName());

        bool isSave = await Save();

        NotebookFunc?.SpinnerOff(GetName());
        NotebookFunc?.SensitivePage(GetName(), true);

        if (isSave)
        {
            if (closePage)
                NotebookFunc?.ClosePage(GetName());
            else
                NotebookFunc?.RenamePage(Caption, GetName());
        }
    }

    public async ValueTask SetValue()
    {
        NotebookFunc?.SpinnerOn(GetName());
        await AssignValue();
        NotebookFunc?.SpinnerOff(GetName());
    }

    #region Abstract and Virtual Function

    /// <summary>
    /// Лівий Блок
    /// </summary>
    protected virtual void CreateStart(Box vBox) { }

    /// <summary>
    /// Правий Блок
    /// </summary>
    protected virtual void CreateEnd(Box vBox) { }

    /// <summary>
    /// Додаткова функція яка викликається із SetValue
    /// </summary>
    public abstract ValueTask AssignValue();

    /// <summary>
    /// Фокус за стандартом
    /// </summary>
    public virtual void DefaultGrabFocus() { }

    /// <summary>
    /// Зчитування значень
    /// </summary>
    protected abstract ValueTask GetValue();

    /// <summary>
    /// Збереження
    /// </summary>
    protected abstract ValueTask<bool> Save();

    #endregion
}