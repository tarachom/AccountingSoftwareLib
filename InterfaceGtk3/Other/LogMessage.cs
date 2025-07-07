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

/*

Повідомлення для логу

*/

using Gtk;

namespace InterfaceGtk3;

public class LogMessage : Box
{
    ScrolledWindow scrollMessage;
    Box vBox;

    TextView? textTerminal;
    ScrolledWindow? scrollTextTerminal;

    public LogMessage(bool visibleTextTerminal = true) : base(Orientation.Vertical, 0)
    {
        Paned vPaned = new Paned(Orientation.Vertical) { BorderWidth = 2 };

        //Верх
        {
            vBox = new Box(Orientation.Vertical, 0);

            scrollMessage = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollMessage.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollMessage.Add(vBox);

            vPaned.Pack1(scrollMessage, true, true);
        }

        //Низ
        if (visibleTextTerminal)
        {
            textTerminal = new TextView() { Editable = false, CursorVisible = true, BorderWidth = 5 };
            textTerminal.StyleContext.AddClass("text_terminal");

            scrollTextTerminal = new ScrolledWindow() { ShadowType = ShadowType.In, HeightRequest = 100 };
            scrollTextTerminal.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollTextTerminal.Add(textTerminal);

            vPaned.Pack2(scrollTextTerminal, false, false);
        }

        PackStart(vPaned, true, true, 0);
    }

    void AddImage(Box hBoxInfo, TypeMessage typeMsg)
    {
        switch (typeMsg)
        {
            case TypeMessage.Ok:
                {
                    hBoxInfo.PackStart(new Image(Іконки.ДляІнформування.Ok), false, false, 10);
                    break;
                }
            case TypeMessage.Error:
                {
                    hBoxInfo.PackStart(new Image(Іконки.ДляІнформування.Error), false, false, 10);
                    break;
                }
            case TypeMessage.Info:
                {
                    hBoxInfo.PackStart(new Image(Іконки.ДляІнформування.Info), false, false, 10);
                    break;
                }
            case TypeMessage.None:
                {
                    hBoxInfo.PackStart(new Label(""), false, false, 17);
                    break;
                }
        }
    }

    Label AddLabel(string text)
    {
        return new Label(text) { Wrap = true, LineWrapMode = Pango.WrapMode.Char, Selectable = true, UseMarkup = true, UseUnderline = false };
    }

    public Box CreateMessage(string message, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
    {
        TrimMessage();

        Box hBoxInfo = new Box(Orientation.Horizontal, 0);
        vBox.PackStart(hBoxInfo, false, false, 2);

        AddImage(hBoxInfo, typeMsg);

        hBoxInfo.PackStart(AddLabel(message), false, false, 0);
        hBoxInfo.ShowAll();

        if (appendEmpty)
            CreateEmptyMsg();

        scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;

        return hBoxInfo;
    }

    public void AppendMessage(Box hBoxInfo, string message, TypeMessage typeMsg = TypeMessage.Ok)
    {
        TrimMessage();

        AddImage(hBoxInfo, typeMsg);

        hBoxInfo.PackStart(AddLabel(message), false, false, 0);
        hBoxInfo.ShowAll();

        scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;
    }

    public void CreateEmptyMsg()
    {
        CreateMessage("", TypeMessage.None);
    }

    public void AppendLine(string message = "")
    {
        if (textTerminal != null && scrollTextTerminal != null)
        {
            if (textTerminal.Buffer.LineCount > MaxLineTextTerminal)
            {
                TextIter iterStart = textTerminal.Buffer.GetIterAtLine(0);
                TextIter iterEnd = textTerminal.Buffer.GetIterAtLine(100);

                textTerminal.Buffer.Delete(ref iterStart, ref iterEnd);
            }

            textTerminal.Buffer.PlaceCursor(textTerminal.Buffer.EndIter);
            textTerminal.Buffer.InsertAtCursor(message + "\n");
            scrollTextTerminal.Vadjustment.Value = scrollTextTerminal.Vadjustment.Upper;
        }
    }

    public Box CreateWidget(Widget? widget, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
    {
        TrimMessage();

        Box hBoxInfo = new Box(Orientation.Horizontal, 0);
        vBox.PackStart(hBoxInfo, false, false, 2);

        AddImage(hBoxInfo, typeMsg);

        hBoxInfo.PackStart(widget ?? new Label("Error: Widget null"), false, false, 0);
        hBoxInfo.ShowAll();

        if (appendEmpty)
            CreateEmptyMsg();

        scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;

        return hBoxInfo;
    }

    public Box CreateWidget(Widget[]? widgets, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
    {
        TrimMessage();

        Box hBoxInfo = new Box(Orientation.Horizontal, 0);
        vBox.PackStart(hBoxInfo, false, false, 2);

        AddImage(hBoxInfo, typeMsg);

        if (widgets != null)
            foreach (var widget in widgets)
                hBoxInfo.PackStart(widget, false, false, 1);

        hBoxInfo.ShowAll();

        if (appendEmpty)
            CreateEmptyMsg();

        scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;

        return hBoxInfo;
    }

    public void AppendWidget(Box hBoxInfo, Widget widget, TypeMessage typeMsg = TypeMessage.Ok)
    {
        TrimMessage();

        AddImage(hBoxInfo, typeMsg);

        hBoxInfo.PackStart(widget, false, false, 0);
        hBoxInfo.ShowAll();

        scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;
    }

    public void ClearMessage()
    {
        foreach (Widget Child in vBox.Children)
            vBox.Remove(Child);

        if (textTerminal != null)
            textTerminal.Buffer.Text = "";
    }

    /// <summary>
    /// Максимальна кількість рядків в Лог
    /// </summary>
    public int MaxLine { get; set; } = 100;

    /// <summary>
    ///  Максимальна кількість рядків в текстовому терміналі
    /// </summary>
    public int MaxLineTextTerminal { get; set; } = 500;

    void TrimMessage()
    {
        int maxChildren = MaxLine;

        if (vBox.Children.Length > maxChildren)
            for (int i = 0; i < vBox.Children.Length - maxChildren; i++)
                vBox.Remove(vBox.Children[i]);
    }

    public enum TypeMessage
    {
        Ok,
        Error,
        Info,
        None
    }
}