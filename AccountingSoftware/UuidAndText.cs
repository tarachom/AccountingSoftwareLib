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

using NpgsqlTypes;

namespace AccountingSoftware
{
    /// <summary>
    /// Композитний тип даних
    /// </summary>
    public class UuidAndText
    {
        /// <summary>
        /// Новий композитний тип
        /// </summary>
        public UuidAndText() { }

        /// <summary>
        /// Новий композитний тип
        /// </summary>
        /// <param name="uuid">Унікальний ідентифікатор</param>
        public UuidAndText(Guid uuid)
        {
            Uuid = uuid;
        }

        /// <summary>
        /// Новий композитний тип
        /// </summary>
        /// <param name="text">Текст</param>
        public UuidAndText(string text)
        {
            Uuid = Guid.Empty;
            Text = text;
        }

        /// <summary>
        /// Новий композитний тип
        /// </summary>
        /// <param name="uuid">Унікальний ідентифікатор</param>
        /// <param name="text">Текст</param>
        public UuidAndText(Guid uuid, string text)
        {
            Uuid = uuid;
            Text = text;
        }

        /// <summary>
        /// Новий композитний тип
        /// </summary>
        /// <param name="unigueID">Унікальний ідентифікатор</param>
        /// <param name="text">Текст</param>
        public UuidAndText(UnigueID unigueID, string text)
        {
            Uuid = unigueID.UGuid;
            Text = text;
        }

        /// <summary>
        /// Вказівник
        /// </summary>
        [PgName("uuid")]
        public Guid Uuid { get; set; } = Guid.Empty;

        /// <summary>
        /// Додаткова інформація
        /// </summary>
        [PgName("text")]
        public string Text { get; set; } = "";

        /// <summary>
        /// Дані у XML форматі
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            return $"<uuid>{Uuid}</uuid><text>{Text}</text>";
        }

        /// <summary>
        /// Чи пустий?
        /// </summary>
        /// <returns>true якщо пустий</returns>
        public bool IsEmpty()
        {
            return Uuid == Guid.Empty;
        }

        /// <summary>
        /// Унікальний ідентифікатор
        /// </summary>
        /// <returns>Новий UnigueID</returns>
        public UnigueID UnigueID()
        {
            return new UnigueID(Uuid);
        }

        /// <summary>
        /// Функція повертає тип у форматі NameAndText, де Назва це група а Текст це тип даних
        /// </summary>
        public NameAndText GetNameAndText()
        {
            var (result, pointerGroup, pointerType) = Configuration.PointerParse(Text, out Exception? _);
            return result ? new NameAndText(pointerGroup, pointerType) : new NameAndText();
        }

        public override string ToString()
        {
            return $"{Uuid}:{Text}";
        }
    }
}