﻿/*
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
        public UuidAndText() { }

        public UuidAndText(Guid uuid)
        {
            Uuid = uuid;
        }

        public UuidAndText(string text)
        {
            Uuid = Guid.Empty;
            Text = text;
        }

        public UuidAndText(Guid uuid, string text)
        {
            Uuid = uuid;
            Text = text;
        }

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

        public bool IsEmpty()
        {
            return Uuid == Guid.Empty;
        }

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