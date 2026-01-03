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

Пошук з виводом результатів у таб частину

*/

using Gtk;
using Gdk;

namespace InterfaceGtk4;

public class SearchControl : Box
{
    SearchEntry entrySearch = SearchEntry.New();

    public SearchControl()
    {
        SetOrientation(Orientation.Horizontal);

        EventControllerKey controller = EventControllerKey.New();
        entrySearch.AddController(controller);
        controller.OnKeyReleased += (sender, args) =>
        {
            if (args.Keyval == (uint)Key.Return || args.Keyval == (uint)Key.KP_Enter)
                Search();
        };

        entrySearch.WidthRequest = 200;
        entrySearch.MarginEnd = 2;
        Append(entrySearch);

        Button bSearch = Button.New();
        bSearch.Child = Image.NewFromPixbuf(Icon.ForButton.Find);
        bSearch.MarginEnd = 2;
        bSearch.OnClicked += (_, _) => Search();
        Append(bSearch);

        Button bClear = Button.New();
        bClear.Child = Image.NewFromPixbuf(Icon.ForButton.Clean);
        bClear.OnClicked += (_, _) => Clear?.Invoke();
        Append(bClear);
    }

    void Search()
    {
        string txt = entrySearch.GetText().Trim();

        if (ToLower)
            txt = txt.ToLower();

        if (txt.Length >= MinLength)
            Select?.Invoke("%" + txt.Replace(" ", "%") + "%");
    }

    public Action<string>? Select { get; set; }
    public Action? Clear { get; set; }

    public int MinLength { get; set; } = 1;
    public bool ToLower { get; set; } = true;

    public string Text
    {
        get => entrySearch.GetText();
    }
}