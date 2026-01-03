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
/// Основа для класів:
///      Form*
/// </summary>
public abstract class Form : Box
{
    /// <summary>
    /// Оновний блокнот
    /// </summary>
    protected NotebookFunction? NotebookFunc { get; private set; }

    /// <summary>
    /// Основне вікно
    /// </summary>
    protected Window? BasicForm { get => NotebookFunc?.BasicForm; }

    /// <summary>
    /// Основна програма з вікна
    /// </summary>
    protected Application? BasicApp { get => NotebookFunc?.BasicForm?.Application; }

    public Form(NotebookFunction? notebookFunc)
    {
        SetOrientation(Orientation.Vertical);
        
        NotebookFunc = notebookFunc;
    }

    #region Link

    /// <summary>
    /// Створює лінк з іконкою
    /// </summary>
    /// <param name="parent">Бокс куди буде доданий лінк</param>
    /// <param name="caption">Назва</param>
    /// <param name="func">Процедура</param>
    public static void CreateLink(Box parent, string caption, Action? func = null)
    {
        Box hBox = New(Orientation.Horizontal, 0);
        hBox.Append(Image.NewFromIconName("doc"));

        Label label = Label.New(caption);
        label.MarginStart = 5;
        hBox.Append(label);

        LinkButton linkButton = LinkButton.New("");
        linkButton.Halign = Align.Start;
        linkButton.Child = hBox;

        parent.Append(linkButton);

        linkButton.OnActivateLink += (_, _) =>
        {
            func?.Invoke();
            return true;
        };
    }

    /// <summary>
    /// Створює простий заголовок або лінк
    /// </summary>
    /// <param name="parent">Бокс куди буде доданий лінк</param>
    /// <param name="caption"><Назва/param>
    /// <param name="func">Процедура</param>
    public static void CreateCaptionLink(Box parent, string caption, Action? func = null)
    {
        if (func != null)
        {
            LinkButton linkButton = LinkButton.New("");
            linkButton.Label = caption;

            parent.Append(linkButton);

            linkButton.OnActivateLink += (_, _) =>
            {
                func?.Invoke();
                return true;
            };
        }
        else
        {
            Label label = Label.New(caption);
            label.MarginTop = label.MarginBottom = 5;

            parent.Append(label);
        }
    }

    /// <summary>
    /// Створює розділювач
    /// </summary>
    /// <param name="parent">Бокс куди буде доданий розділювач</param>
    /// <param name="orientation">Орієнтація розділювача</param>
    public static void CreateSeparator(Box parent, Orientation orientation = Orientation.Horizontal)
    {
        Separator separator = Separator.New(orientation);
        separator.MarginStart = separator.MarginEnd = separator.MarginTop = separator.MarginBottom = 5;

        parent.Append(separator);
    }

    #endregion

    #region Field

    /// <summary>
    /// Створення поля із заголовком
    /// </summary>
    /// <param name="parent">Контейнер</param>
    /// <param name="caption">Заголовок</param>
    /// <param name="field">Поле</param>
    /// <param name="halign">Положення</param>
    /// <param name="put_vbox">Вкласти в додатковий вертикальний блок</param>
    protected static Box CreateField(Box parent, string? caption, Widget? field = null, Align halign = Align.End, bool put_vbox = false)
    {
        Box box = parent;

        // Якщо parent вертикальний - створюємо додатково горизонтальний бокс
        if (parent.GetOrientation() == Orientation.Vertical)
        {
            Box hBox = New(Orientation.Horizontal, 0);
            hBox.MarginStart = hBox.MarginTop = hBox.MarginBottom = 5;
            hBox.Halign = halign;

            parent.Append(hBox);

            box = hBox;
        }
        // Якщо parent горизонтальний і потрібний додатковий вертикальний бокс
        // Створюємо додатково вертикальний та горизонтальний бокси
        else if (put_vbox)
        {
            Box vBox = New(Orientation.Vertical, 0);
            vBox.Valign = Align.Center;

            parent.Append(vBox);

            Box hBox = New(Orientation.Horizontal, 0);
            hBox.MarginStart = hBox.MarginTop = hBox.MarginBottom = 5;
            hBox.Halign = halign;

            vBox.Append(hBox);

            box = hBox;
        }

        if (caption != null)
        {
            Label label = Label.New(AddColon(caption));
            label.MarginEnd = 5;

            box.Append(label);
        }

        if (field != null)
        {
            field.MarginEnd = 5;

            box.Append(field);
        }

        return box;
    }

    /// <summary>
    /// Добавлення поля з прокруткою
    /// </summary>
    /// <param name="parent">Контейнер</param>
    /// <param name="caption">Заголовок</param>
    /// <param name="field">Поле</param>
    /// <param name="width">Висота</param>
    /// <param name="height">Ширина</param>
    /// <param name="halign">Позиціювання</param>
    protected static Box CreateFieldView(Box parent, string? caption, Widget field, int width = 100, int height = 100, Align halign = Align.End)
    {
        Box hBox = New(Orientation.Horizontal, 0);
        hBox.MarginStart = hBox.MarginTop = hBox.MarginBottom = 5;
        hBox.Halign = halign;

        parent.Append(hBox);

        if (caption != null)
        {
            Label label = Label.New(AddColon(caption));
            label.Valign = Align.Start;
            label.MarginEnd = 5;

            hBox.Append(label);
        }

        ScrolledWindow scroll = new() { HasFrame = true, WidthRequest = width, HeightRequest = height };
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.MarginEnd = 5;
        scroll.Child = field;

        hBox.Append(scroll);

        return hBox;
    }

    /// <summary>
    /// Добавлення табличної частини
    /// </summary>
    /// <param name="parent">Контейнер</param>
    /// <param name="caption">Заголовок</param>
    /// <param name="tablePart">Таб частина</param>
    protected static Box CreateTablePart(Box parent, string? caption, Widget tablePart)
    {
        if (caption != null)
        {
            Box hBoxCaption = New(Orientation.Horizontal, 0);
            hBoxCaption.MarginStart = hBoxCaption.MarginEnd = hBoxCaption.MarginTop = hBoxCaption.MarginBottom = 5;
            parent.Append(hBoxCaption);

            Label label = Label.New(AddColon(caption));
            label.MarginEnd = 5;

            hBoxCaption.Append(label);
        }

        Box hBox = New(Orientation.Horizontal, 0);
        hBox.MarginStart = hBox.MarginEnd = hBox.MarginTop = hBox.MarginBottom = 5;
        hBox.Hexpand = true;

        parent.Append(hBox);

        tablePart.Hexpand = true;
        hBox.Append(tablePart);

        return hBox;
    }

    /// <summary>
    /// Добавляє дві крапки в кінці тексту
    /// </summary>
    /// <param name="text">Текст</param>
    /// <returns>Модифікований текст</returns>
    static string AddColon(string text)
    {
        string txt = text.TrimEnd();
        if (!string.IsNullOrEmpty(txt) && !txt.EndsWith(':')) txt += ":";

        return txt;
    }

    #endregion
}