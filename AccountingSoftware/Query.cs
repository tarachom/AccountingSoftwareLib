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

namespace AccountingSoftware
{
    /// <summary>
    /// Конструктор SELECT запиту
    /// </summary>
    /// <param name="table">Таблиця</param>
    public class Query(string table)
    {
        private static int ParamGuidState { get; set; }

        /// <summary>
        /// Функція повертає номер параметру.
        /// Використовується для створення псевдоніму параметрів відбору.
        /// </summary>
        /// <returns></returns>
        public static string GetParamGuid()
        {
            if (ParamGuidState == 0 || ParamGuidState >= int.MaxValue - 1) ParamGuidState = 1;
            return "p" + ParamGuidState++.ToString();
        }

        /// <summary>
        /// Назва таблиці
        /// </summary>
        public string Table { get; set; } = table;

        /// <summary>
        /// Назва тимчасової таблиці
        /// </summary>
        public string TempTable { get; set; } = "";

        /// <summary>
        /// Створити тимчасову таблицю на основі запиту
        /// </summary>
        public bool CreateTempTable { get; set; }

        /// <summary>
        /// Поле Родич для ієрархічного довідника
        /// </summary>
        public string ParentField { get; set; } = "";

        /// <summary>
        /// Які поля вибирати
        /// </summary>
        public List<string> Field { get; set; } = [];

        /// <summary>
        /// Поля із псевдонімами
        /// </summary>
        public List<NameValue<string>> FieldAndAlias { get; set; } = [];

        /// <summary>
        /// Таблиці які потрібно приєднати
        /// </summary>
        public List<Join> Joins { get; set; } = [];

        /// <summary>
        /// Умови.
        /// 1. Назва поля
        /// 2. Тип порівняння
        /// 3. Значення
        /// 4. Тип порівняння з наступним блоком (по замовчуванню AND)
        /// Example: Name EQ "Test" AND (Name = "Test" AND ... )
        /// </summary>
        public List<Where> Where { get; set; } = [];

        /// <summary>
        /// Сортування. 
        /// Назва поля, тип сортування
        /// Name ASC, Code Desc
        /// </summary>
        public Dictionary<string, SelectOrder> Order { get; set; } = [];

        /// <summary>
        /// Обмеження вибірки
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Пропустити задану кількість записів
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Збирає запит
        /// </summary>
        /// <returns>Повертає запит</returns>
        public string Construct()
        {
            string query = "";

            if (CreateTempTable)
            {
                TempTable = "tmp_" + Guid.NewGuid().ToString().Replace("-", "");
                query = "CREATE TEMP TABLE " + TempTable + " AS \n";
            }

            query += "SELECT " + (Joins.Count > 0 ? Table + "." : "") + "uid";

            if (Field.Count > 0)
            {
                //Оригінальні поля
                foreach (string field in Field)
                    query += ", " + (Joins.Count > 0 ? Table + "." : "") + field;
            }

            if (FieldAndAlias.Count > 0)
            {
                query += "\n";

                //Поля з псевдонімами
                foreach (NameValue<string> field in FieldAndAlias)
                    query += ", " + field.Name + " AS " + field.Value;
            }

            query += "\nFROM " + Table;

            if (Joins.Count > 0)
            {
                foreach (Join join in Joins)
                {
                    query += "\n";

                    if (join.JoinType == JoinType.LEFT)
                        query += "LEFT ";
                    else if (join.JoinType == JoinType.RIGHT)
                        query += "RIGHT ";
                    else if (join.JoinType == JoinType.INNER)
                        query += "INNER ";

                    //Псевдонім таблиці sql код
                    string aliasTableQueryPart = string.IsNullOrEmpty(join.JoinTableAlias) ? "" : $" AS {join.JoinTableAlias}";

                    //Псевдонім або назва таблиці
                    string aliasTable = string.IsNullOrEmpty(join.JoinTableAlias) ? join.JoinTable : join.JoinTableAlias;

                    query += $"JOIN {join.JoinTable}{aliasTableQueryPart} ON {join.ParentTable}.{join.JoinField} = {aliasTable}.uid";
                }
            }

            if (Where.Count > 0)
            {
                int count = 0;

                query += "\nWHERE";

                foreach (Where field in Where)
                {
                    count++;

                    if (count > 1)
                    {
                        //За замовчуванням відбори об'єднуються AND
                        query += " " + (field.ComparisonPreceding != Comparison.Empty ? field.ComparisonPreceding : Comparison.AND);
                    }

                    //FuncToField && FuncToField_Param1
                    if (!string.IsNullOrEmpty(field.FuncToField))
                    {
                        query += " " + field.FuncToField + "(" + (Joins.Count > 0 ? Table + "." : "") + field.Name;

                        if (!string.IsNullOrEmpty(field.FuncToField_Param1))
                            query += ", " + field.FuncToField_Param1;

                        query += ")";
                    }
                    else
                        query += " " + (Joins.Count > 0 ? Table + "." : "") + field.Name;

                    switch (field.Comparison)
                    {
                        case Comparison.EQ:
                            {
                                query += " = " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }

                        case Comparison.IN:
                        case Comparison.NOT_IN:
                            {
                                query += (field.Comparison == Comparison.NOT_IN ? " NOT" : "") +
                                    " IN (" + (field.UsingSQLToValue ? field.Value : "@" + field.Alias) + ")";
                                break;
                            }

                        case Comparison.NOT:
                            {
                                query += " != " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }

                        case Comparison.ISNULL:
                        case Comparison.NOTNULL:
                            {
                                if (field.UsingSQLToValue)
                                    query += " " + field.Comparison;
                                else
                                    query += " /* For ISNULL and NOTNULL set UsingSQLToValue = true */ ";
                                break;
                            }
                        case Comparison.BETWEEN:
                            {
                                if (field.UsingSQLToValue)
                                    query += " BETWEEN " + field.Value;
                                else
                                    query += " /* For BETWEEN set UsingSQLToValue = true */ ";
                                break;
                            }
                        case Comparison.LIKE:
                            {
                                query += " LIKE " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }
                        case Comparison.QT:
                            {
                                query += " > " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }
                        case Comparison.LT:
                            {
                                query += " < " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }
                        case Comparison.QT_EQ:
                            {
                                query += " >= " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }
                        case Comparison.LT_EQ:
                            {
                                query += " <= " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }

                        default:
                            {
                                query += " " + field.Comparison + " " + (field.UsingSQLToValue ? field.Value : "@" + field.Alias);
                                break;
                            }
                    }
                }
            }

            if (Order.Count > 0)
            {
                int count = 0;
                query += "\nORDER BY ";

                foreach (KeyValuePair<string, SelectOrder> field in Order)
                {
                    query += (count > 0 ? ", " : "") + field.Key + " " + field.Value;
                    count++;
                }
            }

            if (Limit > 0)
                query += "\nLIMIT " + Limit.ToString();

            if (Offset > 0)
                query += "\nOFFSET " + Offset.ToString();

            return query;
        }

        /// <summary>
        /// Функція конструює запит для ієрархічної вибірки
        /// </summary>
        public string ConstructHierarchical()
        {
            string queryFirst = "";
            string queryRecursive = "";

            ///////////////////
            // Перша частина //
            ///////////////////

            //Додаткове поле Родич
            {
                FieldAndAlias.RemoveAll((x) => x.Value == "parent");
                FieldAndAlias.Add(new($"{Table}.{ParentField}", "parent"));
            }

            //Додаткове поле Рівень
            {
                FieldAndAlias.RemoveAll((x) => x.Value == "level");
                FieldAndAlias.Add(new("1", "level"));
            }

            //Відбір по пустому родичу, тобто верхній рівень
            {
                Where whereEmpty = new(ParentField, Comparison.EQ, $"'{Guid.Empty}'", true);
                Where.Add(whereEmpty);

                Where whereIsNull = new(Comparison.OR, ParentField, Comparison.ISNULL, new(), true);
                Where.Add(whereIsNull);

                queryFirst = Construct();

                Where.Remove(whereEmpty);
                Where.Remove(whereIsNull);
            }

            ///////////////////
            // Друга частина //
            ///////////////////

            //Додаткове поле Родич
            {
                FieldAndAlias.RemoveAll((x) => x.Value == "parent");
                FieldAndAlias.Add(new($"{Table}.{ParentField}", "parent"));
            }

            //Наступний рівень
            {
                FieldAndAlias.RemoveAll((x) => x.Value == "level");
                FieldAndAlias.Add(new("r.level + 1", "level"));
            }

            //Приєднання рекурсії
            {
                Join join = new("r", ParentField, Table, "", JoinType.INNER);
                Joins.Add(join);
                queryRecursive = Construct();
                Joins.Remove(join);
            }

            //Очистка
            FieldAndAlias.RemoveAll((x) => x.Value == "level");
            FieldAndAlias.RemoveAll((x) => x.Value == "parent");

            string query = @$"
WITH RECURSIVE r AS
(

({queryFirst})

UNION ALL

({queryRecursive})

) SELECT * FROM r ORDER BY level";
            // Вибірка і так посортована по level бо partFirst видасть level=1, а partRecursive level+1
            // але вихідна вибірка додатково сортується по level

            return query;
        }

        /// <summary>
        /// Очистка колекцій
        /// </summary>
        public void Clear()
        {
            FieldAndAlias = [];
            Joins = [];
            Where = [];
            Order = [];

            ParamGuidState = 0;
        }
    }

    /// <summary>
    /// Умови відбору
    /// </summary>
    public class Where
    {
        /// <summary>
        /// Умова відбору
        /// </summary>
        /// <param name="name">Назва поля</param>
        /// <param name="comparison">Тип порівняння</param>
        /// <param name="value">Значення поля</param>
        /// <param name="usingSQLToValue">Використання запиту SQL в якості значення поля</param>
        /// <param name="comparisonNext">Звязок між блоками відборів</param>
        public Where(string name, Comparison comparison, object value, bool usingSQLToValue = false)
        {
            ComparisonPreceding = Comparison.Empty;
            Name = name;
            Comparison = comparison;
            Value = value;
            UsingSQLToValue = usingSQLToValue;

            Init();
        }

        /// <summary>
        /// Умова відбору
        /// </summary>
        /// <param name="comparisonPreceding">Звязок між блоками відборів</param>
        /// <param name="name">Назва поля</param>
        /// <param name="comparison">Тип порівняння</param>
        /// <param name="value">Значення поля</param>
        /// <param name="usingSQLToValue">Використання запиту SQL в якості значення поля</param>
        public Where(Comparison comparisonPreceding, string name, Comparison comparison, object value, bool usingSQLToValue = false)
        {
            ComparisonPreceding = comparisonPreceding;
            Name = name;
            Comparison = comparison;
            Value = value;
            UsingSQLToValue = usingSQLToValue;

            Init();
        }

        private void Init()
        {
            Alias = Name + "_" + Query.GetParamGuid();
        }

        /// <summary>
        /// Назва поля
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Псевдонім
        /// </summary>
        public string Alias { get; private set; } = "";

        /// <summary>
        /// Значення поля
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Тип порівняння
        /// </summary>
        public Comparison Comparison { get; set; }

        /// <summary>
        /// Звязок між блоками відборів
        /// </summary>
        public Comparison ComparisonPreceding { get; set; }

        /// <summary>
        /// Використання запиту SQL в якості значення поля
        /// </summary>
        public bool UsingSQLToValue { get; set; }

        /// <summary>
        ///  Функція для поля
        /// </summary>
        public string FuncToField { get; set; } = "";

        /// <summary>
        ///  Перший параметр для функції
        /// </summary>
        public string FuncToField_Param1 { get; set; } = "";
    }

    /// <summary>
    /// Приєднання таблиці
    /// </summary>
    public class Join
    {
        /// <summary>
        /// Приєднання таблиці
        /// </summary>
        /// <param name="joinTable">Таблиця яку треба приєднати</param>
        /// <param name="joinField">Поле з основної таблиці (ParentTable) із ключами</param>
        /// <param name="parentTable">Основна таблиця</param>
        /// <param name="joinType">Тип приєднання</param>
        /// <param name="joinTableAlias">Псевдонім для таблиці яку треба приєднати</param>
        public Join(string joinTable, string joinField, string parentTable, string joinTableAlias = "", JoinType joinType = JoinType.LEFT)
        {
            JoinTable = joinTable;
            JoinField = joinField;
            ParentTable = parentTable;
            JoinTableAlias = joinTableAlias;
            JoinType = joinType;
        }

        /// <summary>
        /// Таблиця яку треба приєднати.
        /// </summary>
        public string JoinTable { get; set; } = "";

        /// <summary>
        /// Псевдонім
        /// </summary>
        public string JoinTableAlias { get; set; } = "";

        /// <summary>
        /// Поле з основної таблиці (ParentTable) із ключами
        /// </summary>
        public string JoinField { get; set; } = "";

        /// <summary>
        /// Основна таблиця
        /// </summary>
        public string ParentTable { get; set; } = "";

        /// <summary>
        /// Тип приєднання
        /// </summary>
        public JoinType JoinType { get; set; } = JoinType.LEFT;
    }

    /// <summary>
    /// Тип приєднання
    /// </summary>
    public enum JoinType
    {
        LEFT,
        INNER,
        RIGHT
    }

    /// <summary>
    /// Тип порівняння
    /// </summary>
    public enum Comparison
    {
        /// <summary>
        /// И
        /// </summary>
        AND,

        /// <summary>
        /// ИЛИ
        /// </summary>
        OR,

        /// <summary>
        /// НЕ
        /// </summary>
        NOT,

        /// <summary>
        /// НЕ В Списку
        /// </summary>
        NOT_IN,

        /// <summary>
        /// В списку
        /// </summary>
        IN,

        /// <summary>
        /// Рівне
        /// </summary>
        EQ,

        /// <summary>
        /// Більше
        /// </summary>
        QT,

        /// <summary>
        /// Менше
        /// </summary>
        LT,

        /// <summary>
        /// Більше або рівне
        /// </summary>
        QT_EQ,

        /// <summary>
        /// Менше або рівне
        /// </summary>
        LT_EQ,

        /// <summary>
        /// Тільки для UsingSQLToValue = true
        /// </summary>
        ISNULL,

        /// <summary>
        /// Тільки для UsingSQLToValue = true
        /// </summary>
        NOTNULL,

        /// <summary>
        /// a більше рівне x AND a менше рівне y
        /// </summary>
        BETWEEN,

        /// <summary>
        /// Пустий
        /// </summary>
        Empty,

        /// <summary>
        /// Схожий
        /// </summary>
        LIKE
    }

    /// <summary>
    /// Сортування
    /// </summary>
    public enum SelectOrder
    {
        ASC,
        DESC
    }
}