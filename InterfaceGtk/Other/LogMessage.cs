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

namespace InterfaceGtk
{
    public class LogMessage : Box
    {
        ScrolledWindow scrollMessage;
        Box vBox;

        public LogMessage() : base(Orientation.Vertical, 0)
        {
            vBox = new Box(Orientation.Vertical, 0);

            scrollMessage = new ScrolledWindow() { ShadowType = ShadowType.In };
            scrollMessage.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scrollMessage.Add(vBox);

            PackStart(scrollMessage, true, true, 0);
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

        public TextView CreateTextTerminal(string message = "", TypeMessage typeMsg = TypeMessage.None)
        {
            CreateMessage(message, typeMsg);

            Box hBox = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBox, false, false, 2);

            AddImage(hBox, TypeMessage.None);

            TextView textTerminal = new TextView() { };
            textTerminal.StyleContext.AddClass("text_terminal");

            ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, HeightRequest = 500, WidthRequest = 1000 };
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(textTerminal);

            hBox.PackStart(scroll, false, false, 1);
            hBox.ShowAll();

            scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;

            return textTerminal;
        }

        public void ApendLineTextTerminal(TextView textTerminal, string message = "")
        {
            textTerminal.Buffer.InsertAtCursor(message + "\n");

            ScrolledWindow scroll = (ScrolledWindow)textTerminal.Parent;
            scroll.Vadjustment.Value = scroll.Vadjustment.Upper;
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
        }

        /// <summary>
        /// Максимальна кількість рядків в Лог
        /// </summary>
        public int MaxLine { get; set; } = 100;

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
}