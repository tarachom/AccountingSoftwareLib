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

public abstract class PointerControl : Box
{
    Label labelCaption = new();
    Entry entryText = new() { Editable = false };
    Button bClear = Button.New();

    public PointerControl()
    {
        SetOrientation(Orientation.Horizontal);

        labelCaption.MarginEnd = 5;
        Append(labelCaption);

        Append(entryText);
        entryText.MarginEnd = 2;

        Button bOpen = Button.New();
        bOpen.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
        bOpen.MarginEnd = 2;
        bOpen.OnClicked += OpenSelect;
        Append(bOpen);

        bClear.Child = Image.NewFromPixbuf(Icon.ForButton.Clean);
        bClear.OnClicked += OnClear;
        Append(bClear);
    }

    /// <summary>
    /// Відкрита папка.
    /// Використовується при загрузці дерева щоб приховати вітку.
    /// Актуально у випадку вибору родича, щоб не можна було вибрати у якості родича відкриту папку
    /// </summary>
    public UniqueID? OpenFolder { get; set; }

    #region Virtual Function

    protected virtual void OpenSelect(Button button, EventArgs args) { }
    protected virtual void OnClear(Button button, EventArgs args) { }

    #endregion

    /// <summary>
    /// Функція яка викликається перед відкриттям вибору
    /// </summary>
    public Action? BeforeClickOpenFunc { get; set; }

    /// <summary>
    /// Функція яка викликається після вибору.
    /// </summary>
    public Action? AfterSelectFunc { get; set; }

    /// <summary>
    /// Функція яка викликається після очищення.
    /// </summary>
    public Action? AfterClearFunc { get; set; }

    public string Caption
    {
        get => labelCaption.GetText();
        set
        {
            string txt = value.Trim();
            if (!string.IsNullOrEmpty(txt) && !txt.EndsWith(':')) txt += ":";

            labelCaption.SetText(txt);
        }
    }

    public int WidthPresentation
    {
        get => entryText.WidthRequest;
        set => entryText.WidthRequest = value;
    }

    protected string Presentation
    {
        get => entryText.GetText();
        set
        {
            entryText.SetText(value);
            entryText.TooltipText = value;
        }
    }

    public string CssClass
    {
        set
        {
            foreach (var className in entryText.GetCssClasses())
                entryText.RemoveCssClass(className);

            if (!string.IsNullOrEmpty(value))
                entryText.AddCssClass(value);
        }
    }

    public bool ClearSensetive
    {
        set => bClear.Sensitive = value;
    }
}