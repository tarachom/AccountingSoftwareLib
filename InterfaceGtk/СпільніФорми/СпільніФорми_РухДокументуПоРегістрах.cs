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

namespace InterfaceGtk
{
    public abstract class СпільніФорми_РухДокументуПоРегістрах : Box
    {
        public СпільніФорми_РухДокументуПоРегістрах() : base(Orientation.Vertical, 0)
        {
            ShowAll();
        }

        protected abstract Widget Документ_PointerControl(DocumentPointer ДокументВказівник);

        protected void ДодатиДокументНаФорму(DocumentPointer ДокументВказівник)
        {
            Box vBox = new Box(Orientation.Vertical, 0);
            Box hBox = new Box(Orientation.Horizontal, 0);
            hBox.PackStart(Документ_PointerControl(ДокументВказівник), false, false, 5);
            vBox.PackStart(hBox, false, false, 5);

            PackStart(vBox, false, false, 10);
        }

        protected virtual void ДодатиБлокНаФорму(string blockName, TreeView treeView)
        {
            Box vBox = new Box(Orientation.Vertical, 0);

            Expander expander = new Expander(blockName) { Expanded = true };
            expander.Add(vBox);

            ScrolledWindow scrollTree = new ScrolledWindow();
            scrollTree.SetPolicy(PolicyType.Automatic, PolicyType.Never);
            scrollTree.Add(treeView);

            Box hBox = new Box(Orientation.Horizontal, 0);
            hBox.PackStart(scrollTree, true, true, 5);
            vBox.PackStart(hBox, false, false, 5);

            PackStart(expander, false, false, 10);

            ShowAll();
        }
    }
}