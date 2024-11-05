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

using Gtk;
using AccountingSoftware;
using System.Reflection;

namespace InterfaceGtk
{
    public abstract class СпільніФорми_ВивідПовідомленняПроПомилки : Форма
    {
        Kernel Kernel { get; set; }
        string NameSpageProgram { get; set; }
        Assembly ExecutingAssembly { get; } = Assembly.GetCallingAssembly();
        Box vBoxMessage = new Box(Orientation.Vertical, 0);

        public СпільніФорми_ВивідПовідомленняПроПомилки(Kernel kernel, string nameSpageProgram) : base()
        {
            Kernel = kernel;
            NameSpageProgram = nameSpageProgram;

            //Кнопки
            Box hBoxTop = new Box(Orientation.Horizontal, 0);
            PackStart(hBoxTop, false, false, 10);

            Button bClear = new Button("Очистити");
            bClear.Clicked += OnClear;
            hBoxTop.PackStart(bClear, false, false, 10);

            Button bReload = new Button("Перечитати");
            bReload.Clicked += OnReload;
            hBoxTop.PackStart(bReload, false, false, 10);

            ScrolledWindow scroll = new ScrolledWindow();
            scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
            scroll.Add(vBoxMessage);

            PackStart(scroll, true, true, 0);

            ShowAll();
        }

        #region Функції

        Widget? CreateCompositControl(string caption, UuidAndText uuidAndText)
        {
            object? compositControlInstance = ExecutingAssembly.CreateInstance($"{NameSpageProgram}.CompositePointerControl");
            if (compositControlInstance != null)
            {
                dynamic compositControl = compositControlInstance;

                compositControl.Caption = caption;
                compositControl.ClearSensetive = false;
                compositControl.TypeSelectSensetive = false;
                compositControl.Pointer = uuidAndText;

                return compositControl;
            }
            else
                return null;
        }

        #endregion

        public async ValueTask LoadRecords(UnigueID? objectUnigueID = null, int? limit = null)
        {
            foreach (Widget Child in vBoxMessage.Children)
                vBoxMessage.Remove(Child);

            SelectRequest_Record record = await Kernel.SelectMessages(objectUnigueID, limit);

            foreach (Dictionary<string, object> row in record.ListRow)
                CreateMessage(row);

            vBoxMessage.ShowAll();
        }

        void CreateMessage(Dictionary<string, object> row)
        {
            Box vBoxInfo = new Box(Orientation.Vertical, 0);

            vBoxMessage.PackStart(new Separator(Orientation.Horizontal), false, false, 5);

            //Image
            {
                var Іконка = row["message_type"] switch
                {
                    'E' => Іконки.ДляІнформуванняВеликі.Error,
                    'I' => Іконки.ДляІнформуванняВеликі.Ok,
                    _ => Іконки.ДляІнформуванняВеликі.Error
                };

                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(new Image(Іконка), false, false, 25);
                hBox.PackStart(vBoxInfo, false, false, 10);
                vBoxMessage.PackStart(hBox, false, false, 10);
            }

            //Перший рядок
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                Label line = new Label("<i>" + row["date"].ToString() + " " + row["process"].ToString() + "</i>") { UseMarkup = true };

                hBox.PackStart(line, false, false, 5);
                vBoxInfo.PackStart(hBox, false, false, 5);
            }

            //Другий рядок
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                Label line = new Label("<b>" + row["name"].ToString() + "</b>") { UseMarkup = true };

                hBox.PackStart(line, false, false, 5);
                vBoxInfo.PackStart(hBox, false, false, 5);
            }

            //Повідомлення
            {
                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(new Label(row["message"].ToString()) { Wrap = true, UseMarkup = true }, false, false, 5);
                vBoxInfo.PackStart(hBox, false, false, 5);
            }

            //Для відкриття
            {
                Widget? Обєкт = CreateCompositControl("", new UuidAndText(new UnigueID(row["uid"]).UGuid, row["type"].ToString() ?? ""));

                Box hBox = new Box(Orientation.Horizontal, 0);
                hBox.PackStart(Обєкт, false, false, 0);
                vBoxInfo.PackStart(hBox, false, false, 0);
            }
        }

        async void OnClear(object? sender, EventArgs args)
        {
            await Kernel.ClearAllMessages();
            await LoadRecords();
        }

        async void OnReload(object? sender, EventArgs args)
        {
            await LoadRecords();
        }
    }
}