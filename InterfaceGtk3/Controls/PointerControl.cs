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

namespace InterfaceGtk3;

public abstract class PointerControl : Box
{
    Label labelCaption = new Label();
    Entry entryText = new Entry() { IsEditable = false };
    Button bClear = new Button(new Image(Іконки.ДляКнопок.Clean));

    public PointerControl() : base(Orientation.Horizontal, 0)
    {
        PackStart(labelCaption, false, false, 5);
        PackStart(entryText, false, false, 1);

        Button bOpen = new Button(new Image(Іконки.ДляКнопок.Find));
        bOpen.Clicked += OpenSelect;
        PackStart(bOpen, false, false, 1);

        bClear.Clicked += OnClear;
        PackEnd(bClear, false, false, 1);
    }

    /// <summary>
    /// Відкрита папка.
    /// Використовується при загрузці дерева щоб приховати вітку.
    /// Актуально у випадку вибору родича, щоб не можна було вибрати у якості родича відкриту папку
    /// </summary>
    public UnigueID? OpenFolder { get; set; }

    protected virtual void OpenSelect(object? sender, EventArgs args) { }
    protected virtual void OnClear(object? sender, EventArgs args) { }

    /// <summary>
    /// Функція яка викликається перед відкриттям вибору
    /// </summary>
    public System.Action? BeforeClickOpenFunc { get; set; }

    /// <summary>
    /// Функція яка викликається після вибору.
    /// </summary>
    public System.Action? AfterSelectFunc { get; set; }

    /// <summary>
    /// Функція яка викликається після очищення.
    /// </summary>
    public System.Action? AfterClearFunc { get; set; }

    public string Caption
    {
        get
        {
            return labelCaption.Text;
        }
        set
        {
            labelCaption.Text = value;
        }
    }

    public int WidthPresentation
    {
        get
        {
            return entryText.WidthRequest;
        }
        set
        {
            entryText.WidthRequest = value;
        }
    }

    protected string Presentation
    {
        get
        {
            return entryText.Text;
        }
        set
        {
            entryText.Text = value;
            entryText.TooltipText = value;
        }
    }

    public string CssClass
    {
        set
        {
            foreach (var className in entryText.StyleContext.ListClasses())
                entryText.StyleContext.RemoveClass(className);

            if (!string.IsNullOrEmpty(value))
                entryText.StyleContext.AddClass(value);
        }
    }

    public bool ClearSensetive
    {
        set
        {
            bClear.Sensitive = value;
        }
    }
}