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

Пошук з виводом результатів у таб частину

*/

using Gtk;

namespace InterfaceGtk
{
    public class SearchControl : Box
    {
        SearchEntry entrySearch = new SearchEntry() { WidthRequest = 200 };

        public SearchControl() : base(Orientation.Horizontal, 0)
        {
            entrySearch.KeyReleaseEvent += OnKeyReleaseEntrySearch;
            entrySearch.TextDeleted += OnClear;
            PackStart(entrySearch, false, false, 2);
        }

        public System.Action<string>? Select { get; set; }
        public System.Action? Clear { get; set; }

        public int MinLength { get; set; } = 1;
        public bool ToLower { get; set; } = true;

        void OnKeyReleaseEntrySearch(object? sender, KeyReleaseEventArgs args)
        {
            if (args.Event.Key == Gdk.Key.Return || args.Event.Key == Gdk.Key.KP_Enter)
            {
                string txt = entrySearch.Text.Trim();

                if (ToLower)
                    txt = txt.ToLower();

                if (txt.Length >= MinLength)
                    Select?.Invoke("%" + txt.Replace(" ", "%") + "%");
            }
        }

        void OnClear(object? sender, EventArgs args)
        {
            Clear?.Invoke();
        }

        public string Text
        {
            get
            {
                return entrySearch.Text;
            }
        }
    }
}