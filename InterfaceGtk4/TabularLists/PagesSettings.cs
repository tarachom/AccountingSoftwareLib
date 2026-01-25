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

using AccountingSoftware;

namespace InterfaceGtk4;

/// <summary>
/// Сторінки
/// </summary>
public class Pages
{
    /// <summary>
    /// Початкове позиціювання
    /// </summary>
    public enum StartingPosition
    {
        Start,
        End
    }

    /// <summary>
    /// Налаштування сторінок
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Проведені початкові обчислення вибірки
        /// </summary>
        public bool Calculated { get; set; } = false;

        /// <summary>
        /// Результат обчислень
        /// </summary>
        public SplitSelectToPages_Record Record { get; set; } = new();

        /// <summary>
        /// Поточна сторінка
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// Розмір сторінки
        /// </summary>
        public int PageSize { get; set; } = 1000;

        /// <summary>
        /// Позиціювання при відкритті (на початку чи кінці вибірки)
        /// </summary>
        public StartingPosition Position { get; set; } = StartingPosition.Start;

        /// <summary>
        /// Очистити
        /// </summary>
        public void Clear()
        {
            Calculated = false;
            CurrentPage = 1;
            Record = new();
        }
    }
}