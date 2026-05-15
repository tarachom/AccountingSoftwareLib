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
/// Рух документу по регістрах
/// </summary>
[GObject.Subclass<Form>]
public partial class CommonForms_ErrorMessageOutput : Form
{
    Kernel? Kernel { get; set; } = null;
    Box vBoxMessage = Box.New(Orientation.Vertical, 0);

    public void Init(Kernel kernel)
    {
        Kernel = kernel;

        //Кнопки
        Box hBoxTop = Box.New(Orientation.Horizontal, 0);
        Append(hBoxTop);

        Button bClear = Button.NewWithLabel("Очистити");
        bClear.OnClicked += OnClear;
        hBoxTop.Append(bClear);

        Button bReload = Button.NewWithLabel("Перечитати");
        bReload.OnClicked += OnReload;
        hBoxTop.Append(bReload);

        ScrolledWindow scroll = ScrolledWindow.New();
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scroll.Vexpand = scroll.Hexpand = true;
        scroll.SetChild(vBoxMessage);

        Append(scroll);
    }

    #region Virtual & Abstract Function

    protected virtual CompositePointerControl CreateCompositeControl(string caption, UuidAndText uuidAndText) { return CompositePointerControl.NewWithProperties([]); }

    #endregion

    public async ValueTask LoadRecords(UniqueID? objectUnigueID = null, int? limit = null)
    {
        if (Kernel != null)
        {
            //Очистка
            {
                Widget? child = vBoxMessage.GetFirstChild();
                while (child != null)
                {
                    Widget? next = child.GetNextSibling();
                    vBoxMessage.Remove(child);
                    child = next;
                }
            }

            SelectRequest_Record record = await Kernel.SelectMessages(objectUnigueID, limit);

            foreach (Dictionary<string, object> row in record.ListRow)
                CreateMessage(row);
        }
    }

    void CreateMessage(Dictionary<string, object> row)
    {
        Box vBoxInfo = Box.New(Orientation.Vertical, 0);

        vBoxMessage.Append(Separator.New(Orientation.Horizontal));

        //Image
        {
            var Іконка = row["message_type"] switch
            {
                'E' => Icon.ForInformationBig.Error,
                'I' => Icon.ForInformationBig.Ok,
                _ => Icon.ForInformationBig.Error
            };

            Box hBox = Box.New(Orientation.Horizontal, 0);
            hBox.Append(Image.NewFromPixbuf(Іконка));
            hBox.Append(vBoxInfo);
            vBoxMessage.Append(hBox);
        }

        //Перший рядок
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            Label line = Label.New("<i>" + row["date"].ToString() + " " + row["process"].ToString() + "</i>");
            line.UseMarkup = true;
            line.UseUnderline = false;
            line.Selectable = true;

            hBox.Append(line);
            vBoxInfo.Append(hBox);
        }

        //Другий рядок
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            Label line = Label.New("<b>" + row["name"].ToString() + "</b>");
            line.UseMarkup = true;
            line.UseUnderline = false;
            line.Selectable = true;

            hBox.Append(line);
            vBoxInfo.Append(hBox);
        }

        //Повідомлення
        {
            Box hBox = Box.New(Orientation.Horizontal, 0);
            Label line = Label.New(row["message"].ToString());
            line.Wrap = true;
            line.UseMarkup = true;
            line.UseUnderline = false;
            line.Selectable = true;

            hBox.Append(line);
            vBoxInfo.Append(hBox);
        }

        //Для відкриття
        {
            UniqueID uniqueID = new(row["uid"]);
            string type = row["type"].ToString() ?? "";

            if (!uniqueID.IsEmpty() && !string.IsNullOrEmpty(type))
            {
                Box hBox = Box.New(Orientation.Horizontal, 0);
                hBox.Append(CreateCompositeControl("", new UuidAndText(uniqueID, type)));
                vBoxInfo.Append(hBox);
            }
        }
    }

    async void OnClear(object? sender, EventArgs args)
    {
        if (Kernel != null)
            await Kernel.ClearAllMessages();

        await LoadRecords();

    }

    async void OnReload(object? sender, EventArgs args)
    {
        await LoadRecords();
    }
}