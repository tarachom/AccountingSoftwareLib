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

/*

Повідомлення для логу

*/

using System.Text;
using Gtk;

namespace InterfaceGtk4;

[GObject.Subclass<Box>]
public partial class LogMessage : Box
{
    ListBox listBox = ListBox.New();
    ScrolledWindow scrollMessage = ScrolledWindow.New();

    TextView textTerminal = TextView.New();
    ScrolledWindow scrollTextTerminal = ScrolledWindow.New();

    partial void Initialize()
    {
        SetOrientation(Orientation.Vertical);

        Paned vPaned = Paned.New(Orientation.Vertical);
        vPaned.Hexpand = vPaned.Vexpand = true;
        vPaned.Position = 500;
        vPaned.MarginTop = vPaned.MarginEnd = vPaned.MarginBottom = vPaned.MarginStart = 2;

        //Верх
        {
            scrollMessage.Vexpand = true;
            scrollMessage.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollMessage.SetChild(listBox);

            vPaned.SetStartChild(scrollMessage);
        }

        //Низ
        {
            textTerminal.Editable = false;
            textTerminal.CursorVisible = true;
            textTerminal.MarginTop = textTerminal.MarginEnd = textTerminal.MarginBottom = textTerminal.MarginStart = 2;
            textTerminal.AddCssClass("log_text_terminal");

            scrollTextTerminal.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTextTerminal.SetChild(textTerminal);

            vPaned.SetEndChild(scrollTextTerminal);
        }

        Append(vPaned);
    }

    public static LogMessage New(bool visibleTextTerminal = true)
    {
        LogMessage view = NewWithProperties([]);
        view.textTerminal.Visible = visibleTextTerminal;

        return view;
    }

    void AddImage(Box hBoxInfo, TypeMessage typeMsg)
    {
        Image Img()
        {
            Image image = Image.NewFromPixbuf(typeMsg switch
            {
                TypeMessage.Ok => Icon.ForInformation.Ok,
                TypeMessage.Error => Icon.ForInformation.Error,
                TypeMessage.Info => Icon.ForInformation.Info,
                _ => null
            });

            image.MarginEnd = 10;

            return image;
        }

        switch (typeMsg)
        {
            case TypeMessage.Ok:
            case TypeMessage.Error:
            case TypeMessage.Info:
                {
                    hBoxInfo.Append(Img());
                    break;
                }
            case TypeMessage.None:
                {
                    Label label = Label.New(null);
                    label.MarginEnd = 16;

                    hBoxInfo.Append(label);
                    break;
                }
        }
    }

    Label AddLabel(string text)
    {
        Label label = Label.New(text);
        label.Wrap = true;
        label.WrapMode = Pango.WrapMode.Char;
        label.Selectable = true;
        label.UseMarkup = true;
        label.UseUnderline = false;

        return label;
    }

    public Box CreateMessage(string message, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
    {
        TrimMessage();

        Box hBoxInfo = New(Orientation.Horizontal, 0);
        hBoxInfo.MarginEnd = 2;
        AddImage(hBoxInfo, typeMsg);
        hBoxInfo.Append(AddLabel(message));

        ListBoxRow row = ListBoxRow.New();
        row.Child = hBoxInfo;

        listBox.Append(row);

        if (appendEmpty) CreateEmptyMsg();
        scrollMessage.Vadjustment?.Value = scrollMessage.Vadjustment.Upper;

        return hBoxInfo;
    }

    public void AppendMessage(Box hBoxInfo, string message, TypeMessage typeMsg = TypeMessage.Ok)
    {
        TrimMessage();

        AddImage(hBoxInfo, typeMsg);
        hBoxInfo.Append(AddLabel(message));

        scrollMessage.Vadjustment?.Value = scrollMessage.Vadjustment.Upper;
    }

    public void CreateEmptyMsg()
    {
        CreateMessage("", TypeMessage.None);
    }

    public void AppendLine(string message = "")
    {
        if (textTerminal != null && textTerminal.Buffer != null && scrollTextTerminal != null)
        {
            //Очистка
            if (textTerminal.Buffer.GetLineCount() > MaxLineTextTerminal)
            {
                textTerminal.Buffer.GetIterAtLine(out TextIter iterStart, 0);
                textTerminal.Buffer.GetIterAtLine(out TextIter iterEnd, 100);
                textTerminal.Buffer.Delete(iterStart, iterEnd);
            }

            //Поміщення курсору в кінець тексту
            textTerminal.Buffer.GetEndIter(out TextIter iterEndText);
            textTerminal.Buffer.PlaceCursor(iterEndText);

            string text = message + "\n";
            textTerminal.Buffer.InsertAtCursor(text, Encoding.UTF8.GetBytes(text).Length);

            scrollTextTerminal.Vadjustment?.Value = scrollTextTerminal.Vadjustment.Upper;
        }
    }

    public Box CreateWidget(Widget? widget, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
    {
        TrimMessage();

        Box hBoxInfo = New(Orientation.Horizontal, 0);
        hBoxInfo.MarginEnd = 2;
        AddImage(hBoxInfo, typeMsg);
        hBoxInfo.Append(widget ?? Label.New("Error: Widget null"));

        ListBoxRow row = ListBoxRow.New();
        row.Child = hBoxInfo;

        if (appendEmpty) CreateEmptyMsg();
        scrollMessage.Vadjustment?.Value = scrollMessage.Vadjustment.Upper;

        return hBoxInfo;
    }

    public Box CreateWidget(Widget[]? widgets, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
    {
        TrimMessage();

        Box hBoxInfo = New(Orientation.Horizontal, 0);
        hBoxInfo.MarginEnd = 2;
        AddImage(hBoxInfo, typeMsg);

        if (widgets != null)
            foreach (var widget in widgets)
            {
                widget.MarginEnd = 1;
                hBoxInfo.Append(widget);
            }

        ListBoxRow row = ListBoxRow.New();
        row.Child = hBoxInfo;

        if (appendEmpty) CreateEmptyMsg();
        scrollMessage.Vadjustment?.Value = scrollMessage.Vadjustment.Upper;

        return hBoxInfo;
    }

    public void AppendWidget(Box hBoxInfo, Widget widget, TypeMessage typeMsg = TypeMessage.Ok)
    {
        TrimMessage();

        AddImage(hBoxInfo, typeMsg);
        hBoxInfo.Append(widget);

        scrollMessage.Vadjustment?.Value = scrollMessage.Vadjustment.Upper;
    }

    public void ClearMessage()
    {
        //Очистка списку
        {
            Widget? child = listBox.GetFirstChild();
            while (child != null)
            {
                Widget? next = child.GetNextSibling();
                listBox.Remove(child);
                child = next;
            }
        }

        textTerminal?.Buffer?.Text = "";
    }

    void TrimMessage()
    {
        int maxChildren = MaxLine;
        int countChildren = 0;

        Widget? child = listBox.GetLastChild();
        while (child != null)
        {
            Widget? next = child.GetPrevSibling();
            if (++countChildren > maxChildren)
                listBox.Remove(child);
            child = next;
        }
    }

    /// <summary>
    /// Максимальна кількість рядків в Лог
    /// </summary>
    public int MaxLine { get; set; } = 100;

    /// <summary>
    ///  Максимальна кількість рядків в текстовому терміналі
    /// </summary>
    public int MaxLineTextTerminal { get; set; } = 500;

    public enum TypeMessage
    {
        Ok,
        Error,
        Info,
        None
    }
}