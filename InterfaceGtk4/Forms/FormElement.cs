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
/// Основа для класів:
///      ДовідникЕлемент, 
///      ДокументЕлемент, 
///      РегістриВідомостейЕлемент
/// </summary>
public abstract class FormElement : Form
{
    /// <summary>
    /// Об'єкт який привязується до форми
    /// </summary>
    protected AccountingSoftware.Object? Element { get; init; }

    /// <summary>
    /// Чи це новий елемент
    /// </summary>
    public bool IsNew { get => Element?.IsNew ?? throw new NullReferenceException("Element null"); }

    /// <summary>
    /// ІД елементу
    /// </summary>
    public UnigueID? UnigueID { get => Element?.UnigueID; }

    /// <summary>
    /// Назва         
    /// </summary>
    public string Caption { get => Element?.Caption ?? ""; }

    /// <summary>
    /// Функція зворотнього виклику для перевантаження списку
    /// </summary>
    public Action<UnigueID?>? CallBack_LoadRecords { get; set; }

    /// <summary>
    /// Індикатор стану блокування
    /// </summary>
    protected Label LabelLockInfo = Label.New(null);

    /// <summary>
    /// Детальна інформація про блокування
    /// </summary>
    protected Button ButtonLock = Button.NewFromIconName("go-down");

    public FormElement(NotebookFunction? notebookFunc) : base(notebookFunc)
    {
        ButtonLock.OnClicked += async (_, _) =>
        {
            if (Element != null)
            {
                LockedObject_Record recordResult = await Element.LockInfo();

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

                Popover popover = Popover.New();
                popover.MarginStart = popover.MarginEnd = popover.MarginTop = popover.MarginBottom = 5;
                popover.SetParent(ButtonLock);
                popover.SetChild(vBox);
                popover.Show();
            }
        };
    }

    /// <summary>
    /// Функція для відображення інформації про блокування
    /// </summary>
    /// <param name="accountingObject">Об'єкт</param>
    public async ValueTask LockInfo()
    {
        if (Element != null)
        {
            bool isLock = await Element.IsLock();

            string color = isLock ? "green" : "red";
            string text = isLock ? "Заблоковано" : "Тільки для читання";

            LabelLockInfo.MarginStart = LabelLockInfo.MarginEnd = 10;
            LabelLockInfo.SetMarkup($"<span color='{color}'>{text}</span>");
        }
    }

    #region Event Function

    /// <summary>
    /// Обробка зміни Caption
    /// </summary>
    //public void CaptionChanged(object? _, string caption) => Caption = caption;

    #endregion

    #region Abstract Function

    /// <summary>
    /// Присвоєння значень
    /// </summary>
    public abstract ValueTask SetValue();

    /// <summary>
    /// Додаткова функція яка викликається із SetValue
    /// </summary>
    public virtual async ValueTask BeforeSetValue() { await ValueTask.FromResult(true); }

    /// <summary>
    /// Фокус за стандартом
    /// </summary>
    public virtual void DefaultGrabFocus() { }

    /// <summary>
    /// Зчитування значень
    /// </summary>
    protected abstract void GetValue();

    /// <summary>
    /// Збереження
    /// </summary>
    protected abstract ValueTask<bool> Save();

    #endregion
}