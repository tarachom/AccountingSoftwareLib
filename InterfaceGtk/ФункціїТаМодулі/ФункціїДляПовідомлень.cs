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
 
Повідомлення про помилки

*/

using Gtk;
using AccountingSoftware;

namespace InterfaceGtk
{
    public abstract class ФункціїДляПовідомлень(Kernel kernel) /// !!!! переробити на статичний
    {
        private Kernel Kernel { get; set; } = kernel;

        public async ValueTask ДодатиПовідомленняПроПомилку(string НазваПроцесу, Guid? Обєкт, string ТипОбєкту, string НазваОбєкту, string Повідомлення)
        {
            await Kernel.DataBase.SpetialTableMessageErrorAdd
            (
                 НазваПроцесу,
                 Обєкт != null ? (Guid)Обєкт : Guid.Empty,
                 ТипОбєкту,
                 НазваОбєкту,
                 Повідомлення
            );

            await ОчиститиУстарівшіПовідомлення();
        }

        public async ValueTask ОчиститиВсіПовідомлення()
        {
            await Kernel.DataBase.SpetialTableMessageErrorClear();
        }

        public async ValueTask ОчиститиУстарівшіПовідомлення()
        {
            await Kernel.DataBase.SpetialTableMessageErrorClearOld();
        }

        public async ValueTask<SelectRequest_Record> ПрочитатиПовідомленняПроПомилки(UnigueID? ВідбірПоОбєкту = null, int? limit = null)
        {
            return await Kernel.DataBase.SpetialTableMessageErrorSelect(ВідбірПоОбєкту, limit);
        }
    }
}