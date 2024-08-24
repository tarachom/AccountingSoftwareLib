/*
Copyright (C) 2019-2024 TARAKHOMYN YURIY IVANOVYCH
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

Діалогове повідомлення

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

        public Box CreateMessage(string message, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
        {
            Box hBoxInfo = new Box(Orientation.Horizontal, 0);
            vBox.PackStart(hBoxInfo, false, false, 2);

            AddImage(hBoxInfo, typeMsg);

            hBoxInfo.PackStart(new Label(message) { Wrap = true, Selectable = true, UseMarkup = true, UseUnderline = false }, false, false, 0);
            hBoxInfo.ShowAll();

            if (appendEmpty)
                CreateEmptyMsg();

            scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;

            return hBoxInfo;
        }

        public void AppendMessage(Box hBoxInfo, string message, TypeMessage typeMsg = TypeMessage.Ok)
        {
            AddImage(hBoxInfo, typeMsg);

            hBoxInfo.PackStart(new Label(message) { Wrap = true, Selectable = true }, false, false, 0);
            hBoxInfo.ShowAll();

            scrollMessage.Vadjustment.Value = scrollMessage.Vadjustment.Upper;
        }

        public void CreateEmptyMsg()
        {
            CreateMessage("", TypeMessage.None);
        }

        public Box CreateWidget(Widget? widget, TypeMessage typeMsg = TypeMessage.Ok, bool appendEmpty = false)
        {
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
        
        public void AppendWidget(Box hBoxInfo, Widget widget, TypeMessage typeMsg = TypeMessage.Ok)
        {
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

        public enum TypeMessage
        {
            Ok,
            Error,
            Info,
            None
        }
    }
}