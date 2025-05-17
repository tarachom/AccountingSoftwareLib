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

using System.Text.RegularExpressions;
using Npgsql;

namespace AccountingSoftware
{
    public class PostgreSQL : IDataBase
    {
        #region Connect
        NpgsqlDataSource? DataSource { get; set; }

        public async ValueTask<bool> Open(string Server, string UserId, string Password, int Port, string Database)
        {
            Exception = null;

            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};Database={Database};SSLMode=Prefer;";

            try
            {
                await Open(conString);
                return true;
            }
            catch (Exception e)
            {
                Exception = e;
                return false;
            }
        }

        async ValueTask Open(string connectionString, bool startScript = true)
        {
            NpgsqlDataSourceBuilder dataBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataBuilder.MapComposite<UuidAndText>("uuidtext");
            dataBuilder.MapComposite<NameAndText>("nametext");

            DataSource = dataBuilder.Build();

            if (startScript)
                await StartScript();
        }

        public async ValueTask<bool> TryConnectToServer(string Server, string UserId, string Password, int Port, string Database)
        {
            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};Database={Database};SSLMode=Prefer;";

            Exception = null;

            try
            {
                await Open(conString, false);
                return true;
            }
            catch (Exception e)
            {
                Exception = e;
                return false;
            }
            finally
            {
                Close();
            }
        }

        public async ValueTask<bool> IfExistDatabase(string Server, string UserId, string Password, int Port, string Database)
        {
            Exception = null;

            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};SSLMode=Prefer;";

            try
            {
                await Open(conString, false);
            }
            catch (Exception e)
            {
                Exception = e;
                return false;
            }

            return await IfExistDatabase(Database);
        }

        async ValueTask<bool> IfExistDatabase(string Database)
        {
            if (DataSource != null)
            {
                string sql = @"
SELECT EXISTS
(
    SELECT 
        datname 
    FROM 
        pg_catalog.pg_database 
    WHERE 
        lower(datname) = lower(@databasename)
)";

                NpgsqlCommand command = DataSource.CreateCommand(sql);
                command.Parameters.AddWithValue("databasename", Database);

                try
                {
                    object? result = await command.ExecuteScalarAsync();
                    return bool.Parse(result?.ToString() ?? "false");
                }
                catch (Exception e)
                {
                    Exception = e;
                    return false;
                }
            }
            else
                return false;
        }

        public async ValueTask<bool> CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database)
        {
            if (!Regex.IsMatch(Database, "^[0-9a-z_]+$"))
            {
                Exception = new Exception(
                    "Назва бази даних не відповідає критерію.\n" +
                    "Назва має складатися тільки з маленьких англійських букв, цифр або знаку _\n\n" +
                    "Наприклад: mydata, my_data, data2023, storage_and_trade");

                return false;
            }

            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};SSLMode=Prefer;";

            try
            {
                await Open(conString, false);
            }
            catch (Exception e)
            {
                Exception = e;
                return false;
            }

            if (DataSource != null && !await IfExistDatabase(Database))
            {
                NpgsqlCommand command = DataSource.CreateCommand($"CREATE DATABASE {Database}");

                try
                {
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception e)
                {
                    Exception = e;
                    return false;
                }
            }
            else
                return false;
        }

        public void Close()
        {
            if (DataSource != null)
            {
                OpenTransaction.Clear();
                DataSource.Dispose();
            }
        }

        async ValueTask StartScript()
        {
            bool reloadTypes = false;

            //Перевірка наявності композитного типу uuidtext
            {
                object? result = await ExecuteSQLScalar("SELECT 'exist' FROM pg_type WHERE typname = 'uuidtext'", null);
                if (!(result != null && result.ToString() == "exist"))
                {
                    //Створення композитного типу uuidtext, даний тип відповідає класу UuidAndText
                    await ExecuteSQL($@"CREATE TYPE uuidtext AS (uuid uuid, text text)");
                    reloadTypes = true;
                }
            }

            //Перевірка наявності композитного типу nametext
            {
                object? result = await ExecuteSQLScalar("SELECT 'exist' FROM pg_type WHERE typname = 'nametext'", null);
                if (!(result != null && result.ToString() == "exist"))
                {
                    //Створення композитного типу nametext, даний тип відповідає класу NameAndText
                    await ExecuteSQL($@"CREATE TYPE nametext AS (name text, text text)");
                    reloadTypes = true;
                }
            }

            //Перезагрузка типів
            if (reloadTypes && DataSource != null)
            {
                await using NpgsqlConnection conn = await DataSource.OpenConnectionAsync();
                await conn.ReloadTypesAsync();
            }

            //Підключити Додаток_UUID_OSSP
            await ExecuteSQL(@"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp""");
        }

        public async ValueTask CreateSpecialTables()
        {
            //Прибрати пізніше
            await ExecuteSQL($@"ALTER TABLE {SpecialTables.ObjectUpdateTriger} ADD COLUMN IF NOT EXISTS operation ""char"" NOT NULL DEFAULT ''");

            //await ExecuteSQL($"DROP TABLE IF EXISTS {SpecialTables.ObjectVersionsHistory}");
            //await ExecuteSQL($"DROP TABLE IF EXISTS {SpecialTables.TablePartVersionsHistory}");

            //Список системних таблиць
            List<string> specialTable = await GetSpecialTableList();

            //
            // Системні таблиці
            //

            if (!specialTable.Contains(SpecialTables.RegAccumTriger))
            {
                /*

                Таблиця для запису інформації про зміни в регістрах накопичення.
                На основі цієї інформації розраховуються віртуальні таблиці регістрів

                @datewrite - дата запису
                @period - дата на яку потрібно зробити розрахунки регістру
                @regname - назва регістру
                @document - документ який зробив запис в регістр
                @execute - признак виконання обчислень
                @info - додаткова інформація

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.RegAccumTriger} 
(
    uid serial NOT NULL,
    datewrite timestamp without time zone NOT NULL,
    period timestamp without time zone NOT NULL,
    regname text NOT NULL,
    document uuid NOT NULL,
    execute boolean NOT NULL,
    info text,
    PRIMARY KEY(uid)
)");
            }

            if (!specialTable.Contains(SpecialTables.RegAccumTrigerDocIgnore))
            {
                /*

                Таблиця для запису інформації про документи які потрібно проігнорувати 
                при обробці тригерів для розрахунку регістрів накопичення.

                Це потрібно для оптимізації розрахунку регістрів накопичення, 
                щоб тимчасово проігнорувати розрахунки для певних документів 
                допоки наприклад не будуть проведені документи за певну дату, а тоді
                вже пачкою розрахувати 

                @datewrite - дата запису
                @user - користувач
                @session - сесія
                @document - документ
                @info - додаткова інформація

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.RegAccumTrigerDocIgnore} 
(
    uid serial NOT NULL,
    datewrite timestamp without time zone NOT NULL,
    users uuid NOT NULL,
    session uuid NOT NULL,
    document uuid NOT NULL,
    info text,
    PRIMARY KEY(uid)
)");
            }

            if (!specialTable.Contains(SpecialTables.LockedObject))
            {
                /*

                Таблиця заблокованих обєктів

                @uid - PRIMARY KEY
                @session - сесія
                @users - користувач
                @datelock - дата блокування
                @dateupdate - дата підтвердження блокування
                @obj - обєкт конфігурації (Довідники, Документи)

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.LockedObject} 
(
    uid uuid NOT NULL,
    session uuid NOT NULL,
    users uuid NOT NULL,
    datelock timestamp without time zone NOT NULL,
    obj uuidtext NOT NULL,
    PRIMARY KEY(uid)
)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.LockedObject}_session_idx ON {SpecialTables.LockedObject}(session)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.LockedObject}_users_idx ON {SpecialTables.LockedObject}(users)");
            }

            if (!specialTable.Contains(SpecialTables.Users))
            {
                /*

                Таблиця користувачів

                @name - унікальна назва користувача у нижньому регістрі
                @fullname - назва користувача
                @password - пароль
                @dateadd - коли доданий в базу
                @dateupdate - коли обновлена інформація
                @info - додаткова інформація

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.Users} 
(
    uid uuid NOT NULL,
    name text NOT NULL,
    fullname text,
    password text NOT NULL,
    dateadd timestamp without time zone NOT NULL,
    dateupdate timestamp without time zone NOT NULL,
    info text NOT NULL DEFAULT '',
    PRIMARY KEY(uid),
    CONSTRAINT name_unique_idx UNIQUE (name)
)");
                /* Користувач Admin */
                await SpetialTableUsersAddSuperUser();
            }

            if (!specialTable.Contains(SpecialTables.ActiveUsers))
            {
                /*

                Таблиця активних користувачів

                @usersuid - користувач ід
                @datelogin - дата входу в систему
                @dateupdate - дата підтвердження активного стану
                @master - головний. Він виконує обчислення

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.ActiveUsers} 
(
    uid uuid NOT NULL,
    usersuid uuid NOT NULL,
    datelogin timestamp without time zone NOT NULL,
    dateupdate timestamp without time zone NOT NULL,
    master boolean NOT NULL DEFAULT FALSE,
    type_form integer NOT NULL DEFAULT 0,
    PRIMARY KEY(uid)
)");
            }

            if (!specialTable.Contains(SpecialTables.FullTextSearch))
            {
                /*

                Таблиця для повнотекстового пошуку

                @uidobj - ід обєкту конфігурації
                @obj - обєкт конфігурації
                @value - текстове значення полів для яких використовується повнотекстовий пошук
                @vector - вектор на основі @value
                @groupname - група пріоритету (A, B, C) - сортується для виводу
                @dateadd - дата додавання (не використовується)
                @dateupdate - дата останнього обновлення (не використовується)
                @processed - запис опрацьований (не використовується)

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.FullTextSearch} 
(
    uidobj uuid NOT NULL,
    obj uuidtext NOT NULL,
    value text NOT NULL DEFAULT '',
    vector tsvector,
    groupname ""char"" NOT NULL DEFAULT '',
    dateadd timestamp without time zone NOT NULL,
    dateupdate timestamp without time zone NOT NULL,
    PRIMARY KEY(uidobj)
)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.FullTextSearch}_vector_idx ON {SpecialTables.FullTextSearch} USING gin(vector)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.FullTextSearch}_groupname_idx ON {SpecialTables.FullTextSearch}(groupname)");
            }

            if (!specialTable.Contains(SpecialTables.MessageError))
            {
                /*

                Таблиця для запису інформації про помилки

                @users - Користувач
                @datewrite - Дата запису
                @processname - Проведення, запис, видалення і т.д
                @objectuid - Об'єкт
                @objecttype - Тип так як задано в конфігураторі
                @objectname - Назва
                @message - Повідомлення
                @message_type - Тип повідомлення, задається одним символом (Помилка E, Інформація I і т.д)

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.MessageError} 
(
    uid serial NOT NULL,
    users uuid NOT NULL,
    datewrite timestamp without time zone NOT NULL,
    processname text NOT NULL,
    objectuid uuid NOT NULL,
    objecttype text NOT NULL,
    objectname text NOT NULL,
    message text NOT NULL,
    message_type ""char"" NOT NULL DEFAULT '',
    PRIMARY KEY(uid)
)");
                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.MessageError}_users_idx ON {SpecialTables.MessageError}(users)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.MessageError}_objectuid_idx ON {SpecialTables.MessageError}(objectuid)");
            }

            if (!specialTable.Contains(SpecialTables.ObjectUpdateTriger))
            {
                /*

                Таблиця для тригерів оновлення об’єктів (Довідники, Документи)

                @uid - PRIMARY KEY
                @datewrite - час запису
                @obj - обєкт конфігурації
                @operation - тип операції (A, D, U) - Add, Delete, Update

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.ObjectUpdateTriger} 
(
    uid serial NOT NULL,
    datewrite timestamp without time zone NOT NULL,
    obj uuidtext NOT NULL,
    operation ""char"" NOT NULL DEFAULT '',
    PRIMARY KEY(uid)
)");
                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.ObjectUpdateTriger}_datewrite_idx ON {SpecialTables.ObjectUpdateTriger}(datewrite)");
            }

            if (!specialTable.Contains(SpecialTables.ObjectVersionsHistory))
            {
                /*

                Таблиця для запису історії зміни даних для обєктів довідників та документів

                @uid - PRIMARY KEY
                @datewrite - дата запису
                @users - користувач
                @obj - обєкт
                @fields - масив полів
                @operation - тип операції (A, U, E) - Add, Update, Empty
                @info - додаткова інформація

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.ObjectVersionsHistory} 
(
    uid uuid NOT NULL,
    datewrite timestamp without time zone NOT NULL,
    users uuid NOT NULL,
    obj uuidtext NOT NULL,
    fields nametext[] NOT NULL,
    operation ""char"" NOT NULL DEFAULT '',
    info text NOT NULL DEFAULT '',
    PRIMARY KEY(uid)
)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.ObjectVersionsHistory}_datewrite_idx ON {SpecialTables.ObjectVersionsHistory}(datewrite)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.ObjectVersionsHistory}_users_idx ON {SpecialTables.ObjectVersionsHistory}(users)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.ObjectVersionsHistory}_objuuid_idx ON {SpecialTables.ObjectVersionsHistory}(((obj).uuid))");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.ObjectVersionsHistory}_objtext_idx ON {SpecialTables.ObjectVersionsHistory}(((obj).text))");

            }

            if (!specialTable.Contains(SpecialTables.TablePartVersionsHistory))
            {
                /*

                Таблиця для запису історії зміни в табличних частинах для довідників та документів

                @uid - PRIMARY KEY
                @objversionid - uid з таб {SpecialTables.ObjectVersionsHistory} 
                @datewrite - дата запису
                @users - користувач
                @objowner - обєкт власник
                @tablepart - назва табличної частини
                @fields - масив полів

                */

                await ExecuteSQL($@"
CREATE TABLE IF NOT EXISTS {SpecialTables.TablePartVersionsHistory} 
(
    uid uuid NOT NULL,
    objversionid uuid NOT NULL,
    datewrite timestamp without time zone NOT NULL,
    users uuid NOT NULL,
    objowner uuidtext NOT NULL,
    tablepart text NOT NULL DEFAULT '',
    fields nametext[] NOT NULL,
    PRIMARY KEY(uid)
)");
                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.TablePartVersionsHistory}_objversionid_idx ON {SpecialTables.TablePartVersionsHistory}(objversionid)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.TablePartVersionsHistory}_datewrite_idx ON {SpecialTables.TablePartVersionsHistory}(datewrite)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.TablePartVersionsHistory}_users_idx ON {SpecialTables.TablePartVersionsHistory}(users)");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.TablePartVersionsHistory}_objowneruuid_idx ON {SpecialTables.TablePartVersionsHistory}(((objowner).uuid))");

                await ExecuteSQL($@"
CREATE INDEX IF NOT EXISTS {SpecialTables.TablePartVersionsHistory}_objownertext_idx ON {SpecialTables.TablePartVersionsHistory}(((objowner).text))");

            }
        }

        #endregion

        #region Exception

        public Exception? Exception { get; private set; }

        #endregion

        #region SpetialTable MessageError

        public async ValueTask SpetialTableMessageErrorAdd(Guid user_uid, string nameProcess, Guid uidObject, string typeObject, string nameObject, string message, char message_type, byte transactionID = 0)
        {
            await ExecuteSQL($@"
INSERT INTO {SpecialTables.MessageError} 
(
    users,
    datewrite,
    processname,
    objectuid,
    objecttype,
    objectname,
    message,
    message_type
)
VALUES
(
    @users,
    CURRENT_TIMESTAMP,
    @processname,
    @objectuid,
    @objecttype,
    @objectname,
    @message,
    @message_type
)",
new Dictionary<string, object>
{
    { "users", user_uid },
    { "processname", nameProcess },
    { "objectuid", uidObject },
    { "objecttype", typeObject },
    { "objectname", nameObject },
    { "message", message },
    { "message_type", message_type }
},
transactionID);
        }

        public async ValueTask<SelectRequest_Record> SpetialTableMessageErrorSelect(Guid user_uid, UnigueID? unigueIDObjectWhere = null, int? limit = null)
        {
            Dictionary<string, object> queryParam = new() { { "users", user_uid } };

            string query = $@"
SELECT
    datewrite AS date,
    to_char(datewrite, 'HH24:MI:SS') AS time,
    processname AS process,
    objectuid AS uid,
    objecttype AS type,
    objectname AS name,
    message,
    message_type
FROM {SpecialTables.MessageError}
WHERE users = @users
";
            if (unigueIDObjectWhere != null && !unigueIDObjectWhere.IsEmpty())
                query += $@"AND objectuid = '{unigueIDObjectWhere}'";

            query += $@"
ORDER BY date DESC
LIMIT {limit ?? 100}
";
            return await SelectRequest(query, queryParam);
        }

        public async ValueTask SpetialTableMessageErrorClear(Guid user_uid)
        {
            Dictionary<string, object> queryParam = new() { { "users", user_uid } };

            await ExecuteSQL($@"DELETE FROM {SpecialTables.MessageError} WHERE users = @users", queryParam);
        }

        public async ValueTask SpetialTableMessageErrorClearOld(Guid user_uid)
        {
            Dictionary<string, object> queryParam = new() { { "users", user_uid } };

            await ExecuteSQL($@"
DELETE FROM {SpecialTables.MessageError}
WHERE users = @users AND datewrite < (CURRENT_TIMESTAMP::timestamp - INTERVAL '7 day')", queryParam);
        }

        #endregion

        #region SpetialTable RegAccumTriger

        /// <summary>
        /// Добавляє запис в таблицю тригерів для обчислення віртуальних залишків
        /// </summary>
        /// <param name="period">Дата на яку потрібно розрахувати регістри</param>
        /// <param name="document">Документ</param>
        /// <param name="regAccumName">Назва регістру</param>
        /// <param name="info">Додаткова інфа</param>
        /// <param name="transactionID">Ід транзакції</param>
        public async ValueTask SpetialTableRegAccumTrigerAdd(DateTime period, Guid document, string regAccumName, string info, byte transactionID = 0)
        {
            await ExecuteSQL($@"
INSERT INTO {SpecialTables.RegAccumTriger} 
(
    datewrite,
    period,
    regname,
    document,
    execute,
    info
)
VALUES
(
    CURRENT_TIMESTAMP,
    @period,
    @regname,
    @document,
    false,
    @info
)",
new Dictionary<string, object>
{
    { "period", period },
    { "regname", regAccumName },
    { "document", document },
    { "info", info }
},
transactionID);
        }

        /// <summary>
        /// Запуск виконання обчислень віртуальних залишків
        /// </summary>
        /// <param name="session">Сесія з якої викликана дана процедура</param>
        /// <param name="ExecuteСalculation">Процедура обчислень</param>
        /// <param name="ExecuteFinalСalculation">Фінальна процедура обчислень</param>
        public async ValueTask SpetialTableRegAccumTrigerExecute(Guid session,
            Func<DateTime, string, ValueTask> ExecuteСalculation, Func<List<string>, ValueTask> ExecuteFinalСalculation)
        {
            if (DataSource != null)
            {
                /*
                Перевірка чи дана сесія є головною в розрахунках
                */
                if (!await SpetialTableActiveUsersIsMaster(session))
                    return;

                /*
                1. Вибираються всі завдання для обчислень
                    1.1. Завдання фільтруються по документах які потрібно проігнорувати (документ вважається устарівшим якщо він знаходиться у таблиці більше 5 хв)
                2. Помічаються признаком execute = true. Після обчислень завдання з признаком execute = true будуть видалені
                3. Завдання групуються по періоду і регістру
                */
                string query = @$"
WITH trigers AS
(
    SELECT
        uid,
        date_trunc('day', period::timestamp) AS period,
        regname
    FROM {SpecialTables.RegAccumTriger}
    WHERE 
        document NOT IN (
            SELECT document 
            FROM {SpecialTables.RegAccumTrigerDocIgnore}
            WHERE datewrite > (CURRENT_TIMESTAMP::timestamp - INTERVAL '5 minute')
        )
),
trigers_update AS
(
    UPDATE {SpecialTables.RegAccumTriger} SET execute = true
    WHERE uid IN (SELECT uid FROM trigers)
)
SELECT period, regname 
FROM trigers
GROUP BY period, regname
ORDER BY period
";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                bool hasRows = reader.HasRows;
                List<string> regAccumNameList = [];

                while (await reader.ReadAsync())
                {
                    DateTime period = (DateTime)reader["period"];
                    string regname = (string)reader["regname"];

                    //
                    // ExecuteСalculation
                    //

                    await ExecuteСalculation.Invoke(period, regname);

                    if (!regAccumNameList.Contains(regname))
                        regAccumNameList.Add(regname);
                }
                await reader.CloseAsync();

                //
                // ExecuteFinalСalculation
                //

                if (hasRows)
                    await ExecuteFinalСalculation.Invoke(regAccumNameList);

                //
                // Clear
                //

                if (hasRows)
                {
                    /* Очищення виконаних завдань */
                    query = $"DELETE FROM {SpecialTables.RegAccumTriger} WHERE execute = true";
                    await ExecuteSQL(query);
                }
            }
        }

        /// <summary>
        /// Очищення завдань. Пока не використовується.
        /// </summary>
        public async ValueTask ClearSpetialTableRegAccumTriger()
        {
            string query = $"DELETE FROM {SpecialTables.RegAccumTriger}";
            await ExecuteSQL(query);
        }

        /// <summary>
        /// Додати документ який потрібно проігнорувати при виконанні тригерів
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="info">Додаткова інформація</param>
        /// <param name="transactionID">Ід транзакції</param>
        public async ValueTask SpetialTableRegAccumTrigerDocIgnoreAdd(Guid users, Guid session, Guid document, string info, byte transactionID = 0)
        {
            //Очистка запису для документу, щоб не було лишніх дублів
            await SpetialTableRegAccumTrigerDocIgnoreClear(users, session, document);

            Dictionary<string, object> queryParam = new()
            {
                { "users", users },
                { "session", session },
                { "document", document },
                { "info", info }
            };

            //Добавлення запису про документ
            await ExecuteSQL(@$"
INSERT INTO {SpecialTables.RegAccumTrigerDocIgnore} (datewrite, users, session, document, info) 
VALUES(CURRENT_TIMESTAMP, @users, @session, @document, @info)", queryParam, transactionID);
        }

        /// <summary>
        /// Очистка таблиці документів які потрібно проігнорувати при виконанні тригерів
        /// </summary>
        public async ValueTask SpetialTableRegAccumTrigerDocIgnoreClear(Guid users, Guid session, Guid? document = null, byte transactionID = 0)
        {
            Dictionary<string, object> queryParam = new()
            {
                { "users", users },
                { "session", session }
            };

            if (document != null)
                queryParam.Add("document", document);

            await ExecuteSQL(@$"
DELETE FROM {SpecialTables.RegAccumTrigerDocIgnore}
WHERE users = @users AND session = @session" + (document != null ? " AND document = @document" : ""), queryParam, transactionID);
        }

        #endregion

        #region SpetialTable Users

        async ValueTask SpetialTableUsersAddSuperUser()
        {
            await ExecuteSQL($@"
INSERT INTO {SpecialTables.Users} (uid, name, fullname, dateadd, dateupdate, password) 
VALUES 
(
    uuid_generate_v4(),
    'admin',
    'Admin',
    CURRENT_TIMESTAMP::timestamp,
    CURRENT_TIMESTAMP::timestamp,
    '' /* password */
)
ON CONFLICT (name) DO NOTHING;
");

        }

        public async ValueTask<Guid?> SpetialTableUsersAddOrUpdate(bool isNew, Guid? uid, string name, string fullname, string password, string info)
        {
            Dictionary<string, object> paramQuery = new Dictionary<string, object>
            {
                { "name", name.Trim().ToLower() },
                { "fullname", fullname },
                { "password", password },
                { "info", info }
            };

            if (isNew)
            {
                Guid user_uid = Guid.NewGuid();
                paramQuery.Add("uid", user_uid);

                await ExecuteSQL($@"
INSERT INTO {SpecialTables.Users} (uid, name, fullname, dateadd, dateupdate, password, info) 
VALUES 
(
    @uid,
    @name,
    @fullname,
    CURRENT_TIMESTAMP::timestamp,
    CURRENT_TIMESTAMP::timestamp,
    @password,
    @info
)
ON CONFLICT (name) 
DO UPDATE SET 
    password = @password,
    fullname = @fullname,
    dateupdate = CURRENT_TIMESTAMP::timestamp,
    info = @info
", paramQuery);

                return user_uid;
            }
            else
            {
                if (uid != null)
                {
                    paramQuery.Add("uid", uid);

                    await ExecuteSQL($@"
UPDATE {SpecialTables.Users} SET
    name = @name,
    fullname = @fullname,
    password = @password,
    dateupdate = CURRENT_TIMESTAMP::timestamp,
    info = @info
WHERE
    uid = @uid
", paramQuery);
                }

                return uid;
            }
        }

        public async ValueTask<Dictionary<string, string>> SpetialTableUsersShortSelect()
        {
            Dictionary<string, string> users = [];

            if (DataSource != null)
            {
                string query = $"SELECT name, fullname FROM {SpecialTables.Users} ORDER BY name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    users.Add((string)reader["name"], (string)reader["fullname"]);

                await reader.CloseAsync();
            }

            return users;
        }

        public async ValueTask<SelectRequest_Record> SpetialTableUsersExtendetList()
        {
            string query = $@"
SELECT 
    uid,
    name,
    fullname,
    dateadd,
    dateupdate,
    info
FROM 
    {SpecialTables.Users}
ORDER BY
    name
";
            return await SelectRequest(query, null);
        }

        public async ValueTask<SelectRequest_Record?> SpetialTableUsersExtendetUser(Guid user_uid)
        {
            string query = $@"
SELECT 
    name,
    fullname,
 /* dateadd,
    dateupdate, */
    info
FROM 
    {SpecialTables.Users}
WHERE
    uid = @user_uid
ORDER BY
    name DESC
";

            var recordResult = await SelectRequest(query, new() { { "user_uid", user_uid } });
            return recordResult.Result ? recordResult : null;
        }

        public async ValueTask<bool> SpetialTableUsersIsExistUser(string name, Guid? uid = null, Guid? not_uid = null)
        {
            if (DataSource != null)
            {
                string query = $"SELECT count(uid) AS count FROM {SpecialTables.Users} WHERE name = @name " +
                    (uid.HasValue ? " AND uid = @uid" : "") +
                    (not_uid.HasValue ? " AND uid != @uid" : "");

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("name", name);

                if (uid.HasValue)
                    command.Parameters.AddWithValue("uid", uid.Value);

                if (not_uid.HasValue)
                    command.Parameters.AddWithValue("uid", not_uid.Value);

                object? count_user = await command.ExecuteScalarAsync();

                if (count_user != null && (long)count_user == 1)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public async ValueTask<string> SpetialTableUsersGetFullName(Guid user_uid)
        {
            if (DataSource != null)
            {
                string query = $"SELECT fullname FROM {SpecialTables.Users} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", user_uid);

                object? resultat = await command.ExecuteScalarAsync();
                return (resultat != null) ? (string)resultat : "";
            }
            else
                return "";
        }

        public async ValueTask<bool> SpetialTableUsersDelete(Guid user_uid, string name)
        {
            if (DataSource != null)
            {
                if (await SpetialTableUsersIsExistUser(name, user_uid))
                {
                    string query = $"DELETE FROM {SpecialTables.Users} WHERE uid = @uid";

                    NpgsqlCommand command = DataSource.CreateCommand(query);
                    command.Parameters.AddWithValue("uid", user_uid);

                    await command.ExecuteNonQueryAsync();

                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public async ValueTask<(Guid User, Guid Session)?> SpetialTableUsersLogIn(string user, string password, TypeForm typeForm)
        {
            if (DataSource != null)
            {
                string query = $"SELECT uid FROM {SpecialTables.Users} WHERE name = @name AND password = @password";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("name", user);
                command.Parameters.AddWithValue("password", password);

                object? uid = command.ExecuteScalar();

                if (uid != null)
                {
                    Guid user_uid = (Guid)uid;
                    Guid session_uid = await SpetialTableActiveUsersAddSession(user_uid, typeForm);

                    //Додаткова перевірка всіх наявних сесій на актуальність
                    await SpetialTableActiveUsersUpdateSession(session_uid);

                    return (user_uid, session_uid);
                }
                else
                    return null;
            }
            else
                return null;
        }

        #endregion

        #region SpetialTable ActiveUsers

        async ValueTask<Guid> SpetialTableActiveUsersAddSession(Guid user_uid, TypeForm typeForm)
        {
            Guid session_uid = Guid.NewGuid();

            Dictionary<string, object> paramQuery = new()
            {
                { "user", user_uid },
                { "session", session_uid },
                { "type_form", (int)typeForm }
            };

            await ExecuteSQL($@"
INSERT INTO {SpecialTables.ActiveUsers} (uid, usersuid, datelogin, dateupdate, type_form) 
VALUES 
(
    @session,
    @user,
    CURRENT_TIMESTAMP::timestamp,
    CURRENT_TIMESTAMP::timestamp,
    @type_form
)
", paramQuery);

            return session_uid;
        }

        async ValueTask<bool> SpetialTableActiveUsersIsMaster(Guid session_uid)
        {
            if (DataSource != null)
            {
                string query = $"SELECT master FROM {SpecialTables.ActiveUsers} WHERE uid = @session";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("session", session_uid);

                object? master = await command.ExecuteScalarAsync();
                return master != null && (bool)master;
            }
            else
                return false;
        }

        async ValueTask<bool> SpetialTableActiveUsersIsExistSessionToUpdate(Guid session_uid)
        {
            string query = $@"
WITH update_session AS (
    UPDATE {SpecialTables.ActiveUsers} 
        SET dateupdate = CURRENT_TIMESTAMP::timestamp 
    WHERE 
        uid = @session
    RETURNING 1
)
SELECT count(*) FROM update_session
";

            object? count_session = await ExecuteSQLScalar(query, new() { { "session", session_uid } });
            if (count_session != null && (long)count_session == 1)
                return true;
            else
                return false;
        }

        public async ValueTask<bool> SpetialTableActiveUsersUpdateSession(Guid session_uid)
        {
            int life_old = 60; //Устарівша сесія якщо останнє обновлення більше заданого часу
            int life_active = 8; //Активна сесія якщо останнє обновлення більше заданого часу

            //Обновлення сесії
            bool session_update = await SpetialTableActiveUsersIsExistSessionToUpdate(session_uid);

            //Обробка всіх сесій
            try
            {
                /*

                Стратегія:
                
                1. Очищення устарівших сесій
                2. Пошук головної сесії з признаком master = true
                3. Якщо є головна сесія вона дальше залишається головною, 
                інакше головною сесією стає перша в списку

                Тільки головна сесія виконує фонові обчислення віртуальних залишків
                Головною може бути тільки Робоча програма (type_form = TypeForm.WorkingProgram)

                #1 - Очистка сесій та заблокованих обєктів які повязані із сесією
                #2 - Пошук головної сесії

                */

                await ExecuteSQL($@"
BEGIN;
LOCK TABLE {SpecialTables.ActiveUsers};

-- #1
WITH
old_session AS
(
    SELECT uid FROM {SpecialTables.ActiveUsers}
    WHERE dateupdate < (CURRENT_TIMESTAMP::timestamp - INTERVAL '{life_old} seconds')
),
del_session AS
(
    DELETE FROM {SpecialTables.ActiveUsers}
    WHERE uid IN (SELECT uid FROM old_session)
    RETURNING uid
)
DELETE FROM {SpecialTables.LockedObject}
WHERE session IN (SELECT uid FROM del_session);

-- #2
WITH
clear AS
(
    UPDATE {SpecialTables.ActiveUsers} SET master = false
    WHERE dateupdate <= (CURRENT_TIMESTAMP::timestamp - INTERVAL '{life_active} seconds')
),
master AS
(
    SELECT uid FROM {SpecialTables.ActiveUsers}
    WHERE dateupdate > (CURRENT_TIMESTAMP::timestamp - INTERVAL '{life_active} seconds') AND
        master = true AND type_form = {(int)TypeForm.WorkingProgram}
),
record AS
(
    SELECT 
        CASE WHEN (SELECT count(uid) FROM master) != 0 THEN
            (SELECT uid FROM master LIMIT 1)
        ELSE
            (
                SELECT uid FROM {SpecialTables.ActiveUsers} 
                WHERE dateupdate > (CURRENT_TIMESTAMP::timestamp - INTERVAL '{life_active} seconds') AND
                      type_form = {(int)TypeForm.WorkingProgram}
                LIMIT 1
            )
        END
),
update AS
(
    UPDATE {SpecialTables.ActiveUsers} SET master = true
    WHERE uid = (SELECT uid FROM record)
)
SELECT 1;

COMMIT;
");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(AppContext.BaseDirectory, "error.txt"), ex.Message);
            }

            return session_update;
        }

        public async ValueTask SpetialTableActiveUsersCloseSession(Guid session_uid)
        {
            if (DataSource != null)
            {
                string query = $@"
DELETE FROM {SpecialTables.ActiveUsers} WHERE uid = @session";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("session", session_uid);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask<SelectRequest_Record> SpetialTableActiveUsersSelect()
        {
            string query = $@"
SELECT 
    ActiveUsers.uid,
    ActiveUsers.usersuid,
    Users.fullname AS username,
    ActiveUsers.datelogin,
    ActiveUsers.dateupdate, 
    ActiveUsers.master,
    ActiveUsers.type_form
FROM 
    {SpecialTables.ActiveUsers} AS ActiveUsers
    JOIN {SpecialTables.Users} AS Users ON Users.uid =
        ActiveUsers.usersuid
ORDER BY
    ActiveUsers.dateupdate DESC
";
            return await SelectRequest(query);
        }

        #endregion

        #region SpetialTable FullTextSearch

        public async ValueTask SpetialTableFullTextSearchAddValue(UuidAndText obj, string value, string dictTSearch = Configuration.DefaultDictTSearch)
        {
            if (DataSource != null)
            {
                if (obj.IsEmpty() || string.IsNullOrEmpty(obj.Text) || !obj.Text.Contains('.'))
                    return;

                string[] pointer_and_type = obj.Text.Split(".", StringSplitOptions.None);
                string pointer = pointer_and_type[0];
                string type = pointer_and_type[1];
                string groupname = "";

                switch (pointer)
                {
                    case "Довідники":
                        {
                            groupname = "A";
                            break;
                        }
                    case "Документи":
                        {
                            groupname = "B";
                            break;
                        }
                }

                string query = $@"
INSERT INTO {SpecialTables.FullTextSearch} (uidobj, obj, value, vector, groupname, dateadd, dateupdate) 
VALUES 
(
    @uidobj,
    @obj,
    @value,
    to_tsvector('{dictTSearch}', @value),
    @groupname,
    CURRENT_TIMESTAMP::timestamp,
    CURRENT_TIMESTAMP::timestamp
)
ON CONFLICT (uidobj) DO UPDATE SET 
    value = @value,
    vector = to_tsvector('{dictTSearch}', @value),
    dateupdate = CURRENT_TIMESTAMP::timestamp
";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uidobj", obj.Uuid);
                command.Parameters.AddWithValue("obj", obj);
                command.Parameters.AddWithValue("value", pointer + " (" + type + "): " + value);
                command.Parameters.AddWithValue("groupname", groupname);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask SpetialTableFullTextSearchDelete(UnigueID uid, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $@"
DELETE FROM {SpecialTables.FullTextSearch} WHERE uidobj = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", uid.UGuid);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask<SelectRequest_Record?> SpetialTableFullTextSearchSelect(string findtext, uint offset = 0, string dictTSearch = Configuration.DefaultDictTSearch)
        {
            if (DataSource != null)
            {
                string query = $@"
WITH find_rows AS 
(
    SELECT 
        uidobj,
        obj,
        groupname,
        value,
        dateadd
    FROM 
        {SpecialTables.FullTextSearch}
    WHERE
        vector @@ plainto_tsquery('{dictTSearch}', @findtext)
    ORDER BY
        groupname ASC, 
        dateadd DESC
    LIMIT 10 OFFSET {offset}
)
SELECT
    obj,
    ts_headline('{dictTSearch}', value, plainto_tsquery('{dictTSearch}', @findtext)) AS value,
    dateadd
FROM 
    find_rows
";
                return await SelectRequest(query, new() { { "findtext", findtext } });
            }
            else
                return null;
        }

        public async ValueTask<SelectRequest_Record> SpetialTableFullTextSearchDictList()
        {
            return await SelectRequest("SELECT cfgname FROM pg_ts_config");
        }

        public async ValueTask<bool> SpetialTableFullTextSearchIfExistDict(string dictTSearch)
        {
            object? count = await ExecuteSQLScalar(@$"
SELECT 
    count(cfgname) 
FROM 
    pg_ts_config
WHERE
    cfgname = '{dictTSearch}'", null);

            return count != null && (long)count != 0;
        }

        #endregion

        #region SpetialTable ObjectUpdateTriger

        public async ValueTask SpetialTableObjectUpdateTrigerAdd(UuidAndText obj, char operation)
        {
            Dictionary<string, object> paramQuery = new()
            {
                { "obj", obj },
                { "operation", operation }
            };

            await ExecuteSQL($@"
INSERT INTO {SpecialTables.ObjectUpdateTriger} 
(
    datewrite,
    obj,
    operation
)
VALUES
(
    CURRENT_TIMESTAMP::timestamp,
    @obj,
    @operation
)", paramQuery);
        }

        public async ValueTask<SelectRequest_Record> SpetialTableObjectUpdateTrigerSelect(DateTime after)
        {
            Dictionary<string, object> queryParam = new() { { "after", after } };

            string query = $@"
WITH S AS 
(
    SELECT
        datewrite,
        obj,
        operation
    FROM 
        {SpecialTables.ObjectUpdateTriger}
    WHERE 
        datewrite > @after
    ORDER BY
        datewrite
)
SELECT 
    obj,
    operation,
    MAX(datewrite) AS after
FROM S
GROUP BY
    obj, operation
";
            return await SelectRequest(query, queryParam);
        }

        public async ValueTask SpetialTableObjectUpdateTrigerClear()
        {
            await ExecuteSQL($@"DELETE FROM {SpecialTables.ObjectUpdateTriger}");
        }

        public async ValueTask SpetialTableObjectUpdateTrigerClearOld()
        {
            int life_old = 1; //Устарівші дані

            await ExecuteSQL($@"
DELETE FROM {SpecialTables.ObjectUpdateTriger}
WHERE datewrite < (CURRENT_TIMESTAMP::timestamp - INTERVAL '{life_old} minutes')");
        }

        #endregion

        #region SpetialTable LockedObject

        public async ValueTask<UnigueID> SpetialTableLockedObjectAdd(Guid user_uid, Guid session_uid, UuidAndText obj)
        {
            UnigueID unigueID = new UnigueID();

            //Перевірка наявності блокування об'єкту
            string query = $@"SELECT count(uid) FROM {SpecialTables.LockedObject} WHERE (obj).uuid = @uid";
            object? count = await ExecuteSQLScalar(query, new() { { "uid", obj.Uuid } });

            //Якщо блокування немає то додаємо
            if (count == null || (long)count == 0)
            {
                unigueID.New();

                Dictionary<string, object> paramQuery = new()
                {
                    { "uid", unigueID.UGuid },
                    { "session", session_uid },
                    { "user", user_uid },
                    { "obj", obj }
                };

                await ExecuteSQL($@"
INSERT INTO {SpecialTables.LockedObject} 
(
    uid,
    session,
    users,
    datelock,
    obj
)
VALUES
(
    @uid,
    @session,
    @user,
    CURRENT_TIMESTAMP::timestamp,
    @obj
)", paramQuery);
            }

            return unigueID;
        }

        public async ValueTask<bool> SpetialTableLockedObjectIsLock(UnigueID lockKey)
        {
            if (!lockKey.IsEmpty())
            {
                string query = $@"SELECT count(uid) FROM {SpecialTables.LockedObject} WHERE uid = @uid";
                object? count = await ExecuteSQLScalar(query, new() { { "uid", lockKey.UGuid } });

                if (count != null && (long)count == 1)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public async ValueTask<SelectRequest_Record> SpetialTableLockedObjectSelect()
        {
            string query = $@"
SELECT 
    LockedObject.session,
    LockedObject.users,
    Users.fullname AS username,
    LockedObject.datelock,
    LockedObject.obj
FROM {SpecialTables.LockedObject} AS LockedObject
    JOIN {SpecialTables.Users} AS Users ON Users.uid = LockedObject.users
ORDER BY
    LockedObject.datelock
";
            return await SelectRequest(query);
        }

        public async ValueTask<LockedObject_Record> SpetialTableLockedObjectLockInfo(UuidAndText obj)
        {
            LockedObject_Record record = new();

            if (DataSource != null && !obj.IsEmpty())
            {
                string query = $@"
SELECT 
    LockedObject.uid,
    LockedObject.datelock,
    Users.fullname AS username
FROM {SpecialTables.LockedObject} AS LockedObject
    JOIN {SpecialTables.Users} AS Users ON Users.uid = LockedObject.users
WHERE (LockedObject.obj).uuid = @obj
";
                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("obj", obj.Uuid);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;

                while (await reader.ReadAsync())
                {
                    record.LockKey = new UnigueID(reader["uid"]);
                    record.UserName = reader["username"].ToString() ?? "";
                    record.DateLock = (DateTime)reader["datelock"];
                }
                await reader.CloseAsync();
            }

            return record;
        }

        public async ValueTask SpetialTableLockedObjectClear(UnigueID lockKey)
        {
            if (!lockKey.IsEmpty())
            {
                Dictionary<string, object> paramQuery = new() { { "uid", lockKey.UGuid } };
                await ExecuteSQL($@"DELETE FROM {SpecialTables.LockedObject} WHERE uid = @uid", paramQuery);
            }
        }

        #endregion

        #region SpetialTable VersionsHistory

        #region ObjectVersionsHistory

        /// <summary>
        /// Добаляє новий запис в історію змін обєктів
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="user_uid">Юзер</param>
        /// <param name="obj">Обєкт для якого записується історія</param>
        /// <param name="fieldValue">Поля із значеннями</param>
        /// <param name="operation">Операція</param>
        /// <param name="info">Додаткова інформація</param>
        public async ValueTask SpetialTableObjectVersionsHistoryAdd(Guid version_id, Guid user_uid, UuidAndText obj, Dictionary<string, object>? fieldValue, char operation, string info = "", byte transactionID = 0)
        {
            List<NameAndText> nameAndText = [];

            if (fieldValue != null)
            {
                nameAndText = new(fieldValue.Count);
                foreach (var field in fieldValue)
                    nameAndText.Add(new NameAndText(field.Key, field.Value?.ToString() ?? ""));
            }

            Dictionary<string, object> paramQuery = new()
            {
                { "uid", version_id },
                { "user", user_uid },
                { "obj", obj },
                { "fields", nameAndText.ToArray() },
                { "operation", operation },
                { "info", info }
            };

            await ExecuteSQL($@"
INSERT INTO {SpecialTables.ObjectVersionsHistory} 
(
    uid,
    datewrite,
    users,
    obj,
    fields,
    operation,
    info
)
VALUES
(
    @uid,
    CURRENT_TIMESTAMP::timestamp,
    @user,
    @obj,
    @fields,
    @operation,
    @info
)
ON CONFLICT (uid) DO UPDATE SET
    datewrite = CURRENT_TIMESTAMP::timestamp,
    users = @user,
    obj = @obj,
    fields = @fields,
    operation = @operation,
    info = @info
", paramQuery, transactionID);

        }

        /// <summary>
        /// Список всіх змін для обєкту
        /// </summary>
        /// <param name="obj">Обєкт</param>
        /// <returns>Колекція змін</returns>
        public async ValueTask<SelectVersionsHistoryList_Record> SpetialTableObjectVersionsHistoryList(UuidAndText obj)
        {
            SelectVersionsHistoryList_Record record = new();

            if (DataSource != null)
            {
                string query = $@"
SELECT
    VersionsHistory.uid,
    VersionsHistory.datewrite,
    VersionsHistory.users,
    VersionsHistory.operation,
    VersionsHistory.info,
    Users.fullname AS username
FROM {SpecialTables.ObjectVersionsHistory} AS VersionsHistory
    JOIN {SpecialTables.Users} AS Users ON Users.uid = VersionsHistory.users
WHERE 
    (VersionsHistory.obj).uuid = @uuid AND
    (VersionsHistory.obj).text = @text
ORDER BY
    VersionsHistory.datewrite DESC
";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uuid", obj.Uuid);
                command.Parameters.AddWithValue("text", obj.Text);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;
                record.Obj = obj;
                while (await reader.ReadAsync())
                    record.ListRow.Add(new()
                    {
                        VersionID = (Guid)reader["uid"],
                        DateWrite = (DateTime)reader["datewrite"],
                        UserID = (Guid)reader["users"],
                        Operation = (char)reader["operation"],
                        Info = reader["info"]?.ToString() ?? "",
                        UserName = reader["username"]?.ToString() ?? ""
                    });
                await reader.CloseAsync();
            }

            return record;
        }

        /// <summary>
        /// Один запис з історії змін для обєкту
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="obj">Обєкт</param>
        /// <returns>Колекція</returns>
        public async ValueTask<SelectVersionsHistoryItem_Record> SpetialTableObjectVersionsHistorySelect(Guid version_id, UuidAndText obj)
        {
            SelectVersionsHistoryItem_Record record = new();

            if (DataSource != null)
            {
                string query = $@"
SELECT
    VersionsHistory.datewrite,
    VersionsHistory.users,
    Users.fullname AS username,
    VersionsHistory.obj,
    VersionsHistory.fields
FROM
    {SpecialTables.ObjectVersionsHistory} AS VersionsHistory
    JOIN {SpecialTables.Users} AS Users ON Users.uid = VersionsHistory.users
WHERE
    VersionsHistory.uid = @version_id AND
    (VersionsHistory.obj).uuid = @uuid AND
    (VersionsHistory.obj).text = @text
";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("version_id", version_id);
                command.Parameters.AddWithValue("uuid", obj.Uuid);
                command.Parameters.AddWithValue("text", obj.Text);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;
                while (await reader.ReadAsync())
                {
                    record.VersionID = version_id;
                    record.DateWrite = (DateTime)reader["datewrite"];
                    record.UserID = (Guid)reader["users"];
                    record.UserName = reader["username"]?.ToString() ?? "";
                    record.Obj = (UuidAndText)reader["obj"];
                    record.Fields = (NameAndText[])reader["fields"];
                }
                await reader.CloseAsync();
            }

            return record;
        }

        /// <summary>
        /// Видалення запису з істрії змін
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="obj">Обєкт</param>
        public async ValueTask SpetialTableObjectVersionsHistoryRemove(Guid version_id, UuidAndText obj, byte transactionID = 0)
        {
            if (!obj.IsEmpty())
            {
                Dictionary<string, object> paramQuery = new()
                {
                    { "uuid", obj.Uuid },
                    { "text", obj.Text },
                    { "version_id", version_id }
                };

                await ExecuteSQL($@"
DELETE FROM {SpecialTables.ObjectVersionsHistory} 
WHERE 
    uid = @version_id AND
    (obj).uuid = @uuid AND
    (obj).text = @text
",
paramQuery, transactionID);

                //Видалення історії для табличних частин
                await SpetialTableTablePartVersionsHistoryRemove(version_id, obj, transactionID);
            }
        }

        /// <summary>
        /// Видалення всіх записів в історії для обєкту
        /// </summary>
        /// <param name="obj">Обєкт</param>
        public async ValueTask SpetialTableObjectVersionsHistoryClear(UuidAndText obj, byte transactionID = 0)
        {
            if (!obj.IsEmpty())
            {
                Dictionary<string, object> paramQuery = new()
                {
                    { "uuid", obj.Uuid },
                    { "text", obj.Text },
                };

                await ExecuteSQL($@"
DELETE FROM {SpecialTables.ObjectVersionsHistory} 
WHERE 
    (obj).uuid = @uuid AND
    (obj).text = @text
",
paramQuery, transactionID);

                //Очистка історії для табличних частин
                await SpetialTableTablePartVersionsHistoryClear(obj, transactionID);
            }
        }

        /// <summary>
        /// Функція перевіряє наявність запису для об'єкту і якщо немає - добавляє новий, без полів і з міткою E (пустий)
        /// Використовується для табличних частин, коли є зміни в таб частині але немає змін в самому об'єкті
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="user_uid">Юзер</param>
        /// <param name="obj">Обєкт</param>
        public async ValueTask SpetialTableObjectVersionsHistoryAddIfNotExist(Guid version_id, Guid user_uid, UuidAndText obj, byte transactionID = 0)
        {
            string query = $@"SELECT count(uid) FROM {SpecialTables.ObjectVersionsHistory} WHERE uid = @uid";
            object? count = await ExecuteSQLScalar(query, new() { { "uid", version_id } });

            if (count != null && (long)count == 0)
                await SpetialTableObjectVersionsHistoryAdd(version_id, user_uid, obj, null, 'E', "", transactionID);
        }

        #endregion

        #region TablePartVersionsHistory

        /// <summary>
        /// Добавляє запис в історію змін для табличної частини один рядок
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="user_uid">Юзер</param>
        /// <param name="objowner">Обєкт власник</param>
        /// <param name="tablepart">Таблиця таб частини</param>
        /// <param name="fieldValue">Поля</param>
        public async ValueTask SpetialTableTablePartVersionsHistoryAdd(Guid version_id, Guid user_uid, UuidAndText objowner, string tablepart, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            List<NameAndText> nameAndText = new(fieldValue.Count);
            foreach (var field in fieldValue)
                nameAndText.Add(new NameAndText(field.Key, field.Value?.ToString() ?? ""));

            Dictionary<string, object> paramQuery = new()
            {
                { "uid", Guid.NewGuid() },
                { "objversionid", version_id },
                { "user", user_uid },
                { "objowner", objowner },
                { "tablepart", tablepart },
                { "fields", nameAndText.ToArray() }
            };

            await ExecuteSQL($@"
INSERT INTO {SpecialTables.TablePartVersionsHistory} 
(
    uid,
    objversionid,
    datewrite,
    users,
    objowner,
    tablepart,
    fields
)
VALUES
(
    @uid,
    @objversionid,
    CURRENT_TIMESTAMP::timestamp,
    @user,
    @objowner,
    @tablepart,
    @fields
)
", paramQuery, transactionID);

        }

        /// <summary>
        /// Зчитує всі записи версії для обєкту-власника
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="objowner">Обєкт-власник</param>
        public async ValueTask<SelectVersionsHistoryTablePart_Record> SpetialTableTablePartVersionsHistorySelect(Guid version_id, UuidAndText objowner)
        {
            SelectVersionsHistoryTablePart_Record record = new();

            if (DataSource != null)
            {
                string query = $@"
SELECT
    datewrite,
    users,
    tablepart,
    fields
FROM
    {SpecialTables.TablePartVersionsHistory}
WHERE
    objversionid = @version_id AND
    (objowner).uuid = @uuid AND
    (objowner).text = @text
";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("version_id", version_id);
                command.Parameters.AddWithValue("uuid", objowner.Uuid);
                command.Parameters.AddWithValue("text", objowner.Text);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;
                record.VersionID = version_id;
                record.ObjOwner = objowner;
                while (await reader.ReadAsync())
                {
                    record.ListRow.Add(new()
                    {
                        DateWrite = (DateTime)reader["datewrite"],
                        UserID = (Guid)reader["users"],
                        TablePart = reader["tablepart"]?.ToString() ?? "",
                        Fields = (NameAndText[])reader["fields"],
                    });
                }
                await reader.CloseAsync();
            }

            return record;
        }

        /// <summary>
        /// Видаляє всі записи версії для обєкту-власника 
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="objowner">Обєкт-власник</param>
        async ValueTask SpetialTableTablePartVersionsHistoryRemove(Guid version_id, UuidAndText objowner, byte transactionID = 0)
        {
            if (!objowner.IsEmpty())
            {
                Dictionary<string, object> paramQuery = new()
                {
                    { "uuid", objowner.Uuid },
                    { "text", objowner.Text },
                    { "version_id", version_id }
                };

                await ExecuteSQL($@"
DELETE FROM {SpecialTables.TablePartVersionsHistory} 
WHERE 
    objversionid = @version_id AND
    (objowner).uuid = @uuid AND
    (objowner).text = @text
",
paramQuery, transactionID);
            }
        }

        /// <summary>
        /// Очищає всі записи для обєкту-вланика
        /// </summary>
        /// <param name="objowner">Обєкт-власник</param>
        async ValueTask SpetialTableTablePartVersionsHistoryClear(UuidAndText objowner, byte transactionID = 0)
        {
            if (!objowner.IsEmpty())
            {
                Dictionary<string, object> paramQuery = new()
                {
                    { "uuid", objowner.Uuid },
                    { "text", objowner.Text },
                };

                await ExecuteSQL($@"
DELETE FROM {SpecialTables.TablePartVersionsHistory} 
WHERE 
    (objowner).uuid = @uuid AND
    (objowner).text = @text
",
paramQuery, transactionID);
            }
        }

        /// <summary>
        /// Очищає всі записи версії для обєкту-власника і для даної таб частини.
        /// Використувється для перезапису версії, коли таб частина зберізається більше одного разу
        /// </summary>
        /// <param name="version_id">Версія</param>
        /// <param name="obj">Обєкт-власник</param>
        /// <param name="tablepart">Таблиця таб частини</param>
        public async ValueTask SpetialTableTablePartVersionsHistoryRemoveBeforeSave(Guid version_id, UuidAndText obj, string tablepart, byte transactionID = 0)
        {
            if (!obj.IsEmpty())
            {
                Dictionary<string, object> paramQuery = new()
                {
                    { "uuid", obj.Uuid },
                    { "text", obj.Text },
                    { "version_id", version_id },
                    { "tablepart", tablepart }
                };

                await ExecuteSQL($@"
DELETE FROM {SpecialTables.TablePartVersionsHistory} 
WHERE 
    objversionid = @version_id AND
    tablepart = @tablepart AND
    (objowner).uuid = @uuid AND
    (objowner).text = @text
",
paramQuery, transactionID);
            }
        }

        #endregion

        #endregion

        #region Transaction
        readonly Lock Loсked = new();
        readonly Dictionary<byte, NpgsqlTransaction> OpenTransaction = [];
        volatile byte TransactionCounter = 0;
        private static readonly string[] sourceArray = new string[] { "row_number", "row_count" };

        public async ValueTask<byte> BeginTransaction()
        {
            if (DataSource != null)
            {
                NpgsqlConnection Conn = await DataSource.OpenConnectionAsync();
                NpgsqlTransaction transaction = await Conn.BeginTransactionAsync();

                byte transactionID;

                lock (Loсked)
                {
                    if (TransactionCounter >= byte.MaxValue) TransactionCounter = 0;
                    transactionID = ++TransactionCounter;
                    OpenTransaction.Add(transactionID, transaction);
                }

                return transactionID;
            }
            else
                return 0;
        }

        public async ValueTask CommitTransaction(byte transactionID)
        {
            if (transactionID == 0)
                throw new IndexOutOfRangeException("Не задана транзація");

            if (OpenTransaction.TryGetValue(transactionID, out NpgsqlTransaction? transaction))
            {
                lock (Loсked)
                {
                    OpenTransaction.Remove(transactionID);
                }

                await transaction.CommitAsync();

                if (transaction.Connection != null)
                    await transaction.Connection.CloseAsync();
            }
            else
                throw new IndexOutOfRangeException("Невірний номер транзації");
        }

        public async ValueTask RollbackTransaction(byte transactionID)
        {
            if (transactionID == 0)
                throw new IndexOutOfRangeException("Не задана транзація");

            if (OpenTransaction.TryGetValue(transactionID, out NpgsqlTransaction? transaction))
            {
                lock (Loсked)
                {
                    OpenTransaction.Remove(transactionID);
                }

                await transaction.RollbackAsync();

                if (transaction.Connection != null)
                    await transaction.Connection.CloseAsync();
            }
            else
                throw new IndexOutOfRangeException("Невірний номер транзації");
        }

        private NpgsqlTransaction? GetTransactionByID(byte transactionID)
        {
            if (transactionID == 0) return null;
            return OpenTransaction.TryGetValue(transactionID, out NpgsqlTransaction? transaction) ? transaction : null;
        }

        #endregion

        #region Constants

        public async ValueTask<SelectConstants_Record> SelectConstants(string table, string field)
        {
            SelectConstants_Record recordResult = new();

            if (DataSource != null)
            {
                string query = $"SELECT {field} FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", Guid.Empty);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                recordResult.Result = reader.HasRows;

                if (reader.HasRows && await reader.ReadAsync())
                    recordResult.Value = reader[field];

                await reader.CloseAsync();
            }

            return recordResult;
        }

        public async ValueTask SaveConstants(string table, string field, object fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"INSERT INTO {table} (uid, {field}) VALUES (@uid, @{field}) " +
                               $"ON CONFLICT (uid) DO UPDATE SET {field} = @{field}";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", Guid.Empty);
                command.Parameters.AddWithValue(field, fieldValue);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask SelectConstantsTablePartRecords(Query QuerySelect, string[] fieldArray, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> fieldValue = [];
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);

                    if (QuerySelect.FieldAndAlias.Count > 0)
                    {
                        Dictionary<string, string> joinValue = [];
                        joinValueList.Add(reader["uid"].ToString()!, joinValue);

                        foreach (ValueName<string> fieldAndAlias in QuerySelect.FieldAndAlias)
                            joinValue.Add(fieldAndAlias.Name, reader[fieldAndAlias.Name].ToString() ?? "");
                    }
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid";
                string query_values = "@uid";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask RemoveConstantsTablePartRecords(Guid UID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask DeleteConstantsTablePartRecords(string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table}";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region Func (Directory, Document)

        public async ValueTask<bool> IsExistUniqueID(UnigueID unigueID, string table)
        {
            if (DataSource != null)
            {
                object? result = await ExecuteSQLScalar($"SELECT count(uid) FROM {table} WHERE uid = @uid", new() { { "uid", unigueID.UGuid } });
                return result != null && (long)result == 1;
            }
            else
                return false;
        }

        /// <summary>
        /// Функція обчислює розмір вибірки і кількість сторінок на яку можна розбити вибірку
        /// </summary>
        /// <param name="QuerySelect">Запит</param>
        /// <param name="unigueID">Елемент на який треба спозиціонуватися</param>
        /// <param name="pageSize">Розмір сторінки</param>
        public async ValueTask<SplitSelectToPages_Record> SplitSelectToPages(Query QuerySelect, UnigueID? unigueID, int pageSize = 1000)
        {
            SplitSelectToPages_Record record = new SplitSelectToPages_Record() { PageSize = pageSize };

            if (DataSource != null)
            {
                string query = "";
                UnigueID unigueIDLocal = unigueID ?? new UnigueID();

                bool existUnigueID = !unigueIDLocal.IsEmpty() && await IsExistUniqueID(unigueIDLocal, QuerySelect.Table);
                if (existUnigueID)
                {
                    /*
                    Обчислюється позиція вибраного елементу у вибірці і розмір вибірки
                    */

                    string order = "";
                    if (QuerySelect.Order.Count > 0)
                    {
                        int len = 0;
                        order = "ORDER BY ";

                        foreach (KeyValuePair<string, SelectOrder> field in QuerySelect.Order)
                        {
                            /* 
                            так як для віконної фукнкції row_number() потрібно задавати сортування вибірки,
                            щоб вона коректно працювала, витягую цю інформацію з QuerySelect
                            */

                            string table = "";
                            string fieldKey = field.Key;
                            if (QuerySelect.FieldAndAlias.Count > 0)
                                if (QuerySelect.Field.Contains(field.Key))
                                    table = QuerySelect.Table;
                                else
                                {
                                    ValueName<string>? valueName = QuerySelect.FieldAndAlias.Find(x => x.Name == field.Key);
                                    if (valueName != null) fieldKey = valueName.Value ?? "";
                                }

                            order += (len > 0 ? ", " : "") + (!string.IsNullOrEmpty(table) ? table + "." : "") + fieldKey + " " + field.Value;
                            len++;
                        }
                    }

                    //Порядковий номер та розмір вибірки
                    QuerySelect.FieldAndAlias.AddRange(
                        new ValueName<string>($"row_number() OVER({order})", "row_number"),
                        new ValueName<string>($"count(*) OVER()", "row_count")
                    );

                    query = $@"
WITH S AS 
(
    {QuerySelect.Construct()}
),
all_count AS
(
    SELECT count(*) AS row_count FROM S
),
uid_exist AS
(
    SELECT row_number FROM S WHERE uid = '{unigueIDLocal.UGuid}'
)
SELECT 
    (CASE WHEN (SELECT row_number FROM uid_exist) IS NOT NULL THEN (SELECT row_number FROM uid_exist) ELSE 0 END) AS row_number,
    all_count.row_count AS row_count
FROM
    all_count
";
                }
                else
                {
                    /*
                    Обчислюється тільки розмір вибірки для даного запиту
                    */

                    query = $@"
SELECT 
    count(*) AS row_count
FROM 
(
    {QuerySelect.Construct()}
) AS S";
                }

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;

                while (await reader.ReadAsync())
                {
                    record.Position = existUnigueID ? (long)reader["row_number"] : 0;
                    record.Count = (long)reader["row_count"];
                }
                await reader.CloseAsync();

                if (record.Result && record.Count > 0)
                {
                    //Кількість сторінок
                    record.Pages = (int)(record.Count / record.PageSize);
                    if (record.Pages * record.PageSize < record.Count) record.Pages++;

                    //Поточна сторінка
                    if (record.Position > 0)
                        for (int i = 1; i <= record.Pages; i++)
                        {
                            //Межі сторінки
                            long rightRange = i * record.PageSize;
                            long leftRange = rightRange - record.PageSize + 1;

                            if (record.Position >= leftRange && record.Position <= rightRange)
                                record.CurrentPage = i;
                        }
                }

                //Очищення
                if (existUnigueID)
                    QuerySelect.FieldAndAlias.RemoveAll(x => sourceArray.Contains(x.Value));
            }

            return record;
        }

        /// <summary>
        /// Функція обчислює розмір вибірки і кількість сторінок для журналів
        /// </summary>
        /// <param name="query">Запит</param>
        /// <param name="paramQuery">Параметри</param>
        /// <param name="pageSize">Розмір сторінки</param>
        /// <returns></returns>
        public async ValueTask<SplitSelectToPages_Record> SplitSelectToPagesForJournal(string query, Dictionary<string, object> paramQuery, int pageSize = 1000)
        {
            SplitSelectToPages_Record record = new SplitSelectToPages_Record() { PageSize = pageSize };

            if (DataSource != null)
            {
                NpgsqlCommand command = DataSource.CreateCommand($"SELECT count(*) FROM ({query}) AS S");

                foreach (KeyValuePair<string, object> param in paramQuery)
                    command.Parameters.AddWithValue(param.Key, param.Value);

                object? count = await command.ExecuteScalarAsync();

                record.Result = count != null;
                record.Count = count != null ? (long)count : 0;

                if (record.Result && record.Count > 0)
                {
                    //Кількість сторінок
                    record.Pages = (int)(record.Count / record.PageSize);
                    if (record.Pages * record.PageSize < record.Count) record.Pages++;
                }
            }

            return record;
        }

        #endregion

        #region Directory

        public async ValueTask<bool> InsertDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query_field = "uid, deletion_label";
                string query_values = "@uid, @deletion_label";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlCommand nCommand = DataSource.CreateCommand(query);
                nCommand.Parameters.AddWithValue("uid", unigueID.UGuid);
                nCommand.Parameters.AddWithValue("deletion_label", false);

                foreach (string field in fieldArray)
                    nCommand.Parameters.AddWithValue(field, fieldValue[field]);

                await nCommand.ExecuteNonQueryAsync();

                return true;
            }
            else
                return false;
        }

        public async ValueTask<bool> UpdateDirectoryObject(UnigueID unigueID, bool deletion_label, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"UPDATE {table} SET deletion_label = @deletion_label";

                if (fieldArray != null)
                    foreach (string field in fieldArray)
                        query += $", {field} = @{field}";

                query += " WHERE uid = @uid";

                NpgsqlCommand nCommand = DataSource.CreateCommand(query);
                nCommand.Parameters.AddWithValue("uid", unigueID.UGuid);
                nCommand.Parameters.AddWithValue("deletion_label", deletion_label);

                if (fieldArray != null && fieldValue != null)
                    foreach (string field in fieldArray)
                        nCommand.Parameters.AddWithValue(field, fieldValue[field]);

                await nCommand.ExecuteNonQueryAsync();

                return true;
            }
            else
                return false;
        }

        public async ValueTask<SelectDirectoryObject_Record> SelectDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object>? fieldValue = null)
        {
            SelectDirectoryObject_Record record = new();

            if (DataSource != null)
            {
                string query = $"SELECT deletion_label";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += $" FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;
                while (await reader.ReadAsync())
                {
                    record.DeletionLabel = (bool)reader["deletion_label"];

                    if (fieldArray.Length != 0 && fieldValue != null)
                        foreach (string field in fieldArray)
                            fieldValue[field] = reader[field];
                }
                await reader.CloseAsync();
            }

            return record;
        }

        public async ValueTask DeleteDirectoryObject(UnigueID unigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask SelectDirectoryPointers(Query QuerySelect, List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> listPointers)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object>? fields = null;

                    if (QuerySelect.Field.Count > 0 || QuerySelect.FieldAndAlias.Count > 0)
                    {
                        fields = [];

                        foreach (string field in QuerySelect.Field)
                            fields.Add(field, reader[field]);

                        foreach (ValueName<string> field in QuerySelect.FieldAndAlias)
                            fields.Add(field.Name, reader[field.Name]);
                    }

                    listPointers.Add((new UnigueID(reader["uid"]), fields));
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask SelectDirectoryPointersHierarchical(Query QuerySelect, List<(UnigueID UnigueID, UnigueID Parent, int Level, Dictionary<string, object>? Fields)> listPointers)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.ConstructHierarchical();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    UnigueID unigueID = new(reader["uid"]);
                    UnigueID parent = new(reader["parent"]);
                    int level = (int)reader["level"];

                    Dictionary<string, object>? fields = null;

                    if (QuerySelect.Field.Count > 0 || QuerySelect.FieldAndAlias.Count > 0)
                    {
                        fields = [];

                        foreach (string field in QuerySelect.Field)
                            fields.Add(field, reader[field]);

                        foreach (ValueName<string> field in QuerySelect.FieldAndAlias)
                            fields.Add(field.Name, reader[field.Name]);
                    }

                    listPointers.Add((unigueID, parent, level, fields));
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask<UnigueID?> FindDirectoryPointer(Query QuerySelect)
        {
            UnigueID? directoryPointer = null;

            if (DataSource != null)
            {
                QuerySelect.Limit = 1;

                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                if (QuerySelect.Where.Count > 0)
                    foreach (Where field in QuerySelect.Where)
                        command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    directoryPointer = new UnigueID(reader["uid"]);

                await reader.CloseAsync();
            }

            return directoryPointer;
        }

        public async ValueTask<string> GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                string presentation = "";

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                    for (int i = 0; i < fieldPresentation.Length; i++)
                        presentation += (i > 0 ? ", " : "") + reader[fieldPresentation[i]].ToString();

                await reader.CloseAsync();

                return presentation;
            }
            else
                return "";
        }

        public async ValueTask DeleteDirectoryTempTable(DirectorySelect directorySelect)
        {
            /*
            Створення тимчасових таблиць на даний момент не використовується,
            але така можливість є в класі Query якщо (CreateTempTable == true)
            */

            if (DataSource != null)
            {
                if (directorySelect.QuerySelect.UseTempTable == true &&
                    !string.IsNullOrEmpty(directorySelect.QuerySelect.TempTable) && directorySelect.QuerySelect.TempTable[..4] == "tmp_")
                {
                    string query = $"DROP TABLE IF EXISTS {directorySelect.QuerySelect.TempTable}";

                    NpgsqlCommand command = DataSource.CreateCommand(query);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async ValueTask SelectDirectoryTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> fieldValue = [];
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    if (QuerySelect.FieldAndAlias.Count > 0)
                    {
                        Dictionary<string, string> joinValue = [];
                        joinValueList.Add(reader["uid"].ToString()!, joinValue);

                        foreach (ValueName<string> fieldAndAlias in QuerySelect.FieldAndAlias)
                            joinValue.Add(fieldAndAlias.Name, reader[fieldAndAlias.Name].ToString() ?? "");
                    }
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask InsertDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid, owner";
                string query_values = "@uid, @owner";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask RemoveDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner AND uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);
                command.Parameters.AddWithValue("uid", UID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask DeleteDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region Document

        public async ValueTask<SelectDocumentObject_Record> SelectDocumentObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object>? fieldValue = null)
        {
            SelectDocumentObject_Record record = new();

            if (DataSource != null)
            {
                string query = "SELECT uid, deletion_label, spend, spend_date";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += $" FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;

                while (await reader.ReadAsync())
                {
                    record.DeletionLabel = (bool)reader["deletion_label"];
                    record.Spend = (bool)reader["spend"];
                    record.SpendDate = (DateTime)reader["spend_date"];

                    if (fieldArray.Length != 0 && fieldValue != null)
                        foreach (string field in fieldArray)
                            fieldValue[field] = reader[field];
                }
                await reader.CloseAsync();
            }

            return record;
        }

        public async ValueTask<bool> InsertDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query_field = "uid, deletion_label, spend, spend_date";
                string query_values = "@uid, @deletion_label, @spend, @spend_date";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);
                command.Parameters.AddWithValue("deletion_label", false);
                command.Parameters.AddWithValue("spend", spend);
                command.Parameters.AddWithValue("spend_date", spend_date);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();

                return true;
            }
            else
                return false;
        }

        public async ValueTask<bool> UpdateDocumentObject(UnigueID unigueID, bool? deletion_label, bool? spend, DateTime? spend_date, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"UPDATE {table} SET ";
                List<string> allfield = [];

                if (deletion_label != null)
                    allfield.Add("deletion_label = @deletion_label");

                if (spend != null)
                    allfield.Add("spend = @spend");

                if (spend_date != null)
                    allfield.Add("spend_date = @spend_date");

                if (fieldArray != null)
                    foreach (string field in fieldArray)
                        allfield.Add(field + " = @" + field);

                query += string.Join(", ", allfield.ToList()) + " WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                if (deletion_label != null)
                    command.Parameters.AddWithValue("deletion_label", deletion_label);

                if (spend != null)
                    command.Parameters.AddWithValue("spend", spend);

                if (spend_date != null)
                    command.Parameters.AddWithValue("spend_date", spend_date);

                if (fieldArray != null && fieldValue != null)
                    foreach (string field in fieldArray)
                        command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();

                return true;
            }
            else
                return false;
        }

        public async ValueTask DeleteDocumentObject(UnigueID unigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask SelectDocumentPointer(Query QuerySelect, List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> listPointers)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object>? fields = null;

                    if (QuerySelect.Field.Count > 0 || QuerySelect.FieldAndAlias.Count > 0)
                    {
                        fields = [];

                        foreach (string field in QuerySelect.Field)
                            fields.Add(field, reader[field]);

                        foreach (ValueName<string> field in QuerySelect.FieldAndAlias)
                            fields.Add(field.Name, reader[field.Name]);
                    }

                    listPointers.Add((new UnigueID(reader["uid"]), fields));
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask<UnigueID?> FindDocumentPointer(Query QuerySelect)
        {
            UnigueID? documentPointer = null;

            if (DataSource != null)
            {
                QuerySelect.Limit = 1;

                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                if (QuerySelect.Where.Count > 0)
                    foreach (Where field in QuerySelect.Where)
                        command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                    documentPointer = new UnigueID(reader["uid"]);

                await reader.CloseAsync();
            }

            return documentPointer;
        }

        public async ValueTask<string> GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                string presentation = "";

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                    for (int i = 0; i < fieldPresentation.Length; i++)
                        presentation += (i > 0 ? ", " : "") + reader[fieldPresentation[i]].ToString();

                await reader.CloseAsync();

                return presentation;
            }
            else
                return "";
        }

        public async ValueTask SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> fieldValue = [];
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    if (QuerySelect.FieldAndAlias.Count > 0)
                    {
                        Dictionary<string, string> joinValue = [];
                        joinValueList.Add(reader["uid"].ToString()!, joinValue);

                        foreach (ValueName<string> fieldAndAlias in QuerySelect.FieldAndAlias)
                            joinValue.Add(fieldAndAlias.Name, reader[fieldAndAlias.Name].ToString() ?? "");
                    }
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask InsertDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid, owner";
                string query_values = "@uid, @owner";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask RemoveDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner AND uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);
                command.Parameters.AddWithValue("uid", UID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask DeleteDocumentTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region Journal

        public async ValueTask SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listJournalDocument,
            DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null, bool? spendDocSelect = null)
        {
            if (DataSource != null)
            {
                string query = "";
                int counter = 0;

                /*
                   масив typeDocSelect має складатися із елементів ВидДокументу:Таблиця
                */

                //Масив таблиць для фільтрування
                List<string> typeAllowDoc = [];

                if (typeDocSelect != null)
                    foreach (string typeDocItem in typeDocSelect)
                        if (typeDocItem.Contains(':'))
                        {
                            string[] typeAndTable = typeDocItem.Split(":");

                            //Додаю таблицю у фільтр
                            typeAllowDoc.Add(typeAndTable[1]);
                        }

                if (tables.Length > 0)
                {
                    foreach (string table in tables)
                    {
                        //Фільтрування у випадку якщо є задані фільтри
                        if (typeAllowDoc.Count > 0 && !typeAllowDoc.Contains(table))
                            continue;

                        string whereSpendDoc = "";

                        //Відбір тільки проведених або не проведених
                        if (spendDocSelect.HasValue)
                        {
                            whereSpendDoc = $"AND spend = {spendDocSelect.Value}";

                            //Якщо відбір тільки проведених, тоді додатковий фільтр щоб туди не попали помічені на видалення
                            if (spendDocSelect.Value)
                                whereSpendDoc += " AND deletion_label = false";
                        }

                        query += (counter > 0 ? "\nUNION " : "") +
                            $"(SELECT uid, docname, docdate, docnomer, deletion_label, spend, spend_date, '{typeDocument[counter]}' AS type_doc FROM {table} \n" +
                            $"WHERE docdate >= @periodstart AND docdate <= @periodend {whereSpendDoc})";

                        counter++;
                    }

                    query += "\nORDER BY docdate";

                    NpgsqlCommand command = DataSource.CreateCommand(query);
                    command.Parameters.AddWithValue("periodstart", periodStart);
                    command.Parameters.AddWithValue("periodend", periodEnd);

                    NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        JournalDocument document = new JournalDocument()
                        {
                            UnigueID = new UnigueID(reader["uid"]),
                            DocName = reader["docname"].ToString() ?? "",
                            DocDate = DateTime.Parse(reader["docdate"].ToString() ?? DateTime.MinValue.ToString()),
                            DocNomer = reader["docnomer"].ToString() ?? "",
                            DeletionLabel = (bool)reader["deletion_label"],
                            Spend = (bool)reader["spend"],
                            SpendDate = (DateTime)reader["spend_date"],
                            TypeDocument = reader["type_doc"].ToString() ?? ""
                        };

                        listJournalDocument.Add(document);
                    }
                    await reader.CloseAsync();
                }
            }
        }

        #endregion

        #region RegistersInformation

        public async ValueTask SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                if (QuerySelect.Where.Count > 0)
                {
                    foreach (Where ItemFilter in QuerySelect.Where)
                        command.Parameters.AddWithValue(ItemFilter.Alias, ItemFilter.Value);
                }

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> fieldValue = [];
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    if (QuerySelect.FieldAndAlias.Count > 0)
                    {
                        Dictionary<string, string> joinValue = [];
                        joinValueList.Add(reader["uid"].ToString()!, joinValue);

                        foreach (ValueName<string> fieldAndAlias in QuerySelect.FieldAndAlias)
                            joinValue.Add(fieldAndAlias.Name, reader[fieldAndAlias.Name].ToString() ?? "");
                    }
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, NameAndText ownertype, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid, period, owner, ownertype";
                string query_values = "@uid, @period, @owner, @ownertype";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("period", period);
                command.Parameters.AddWithValue("owner", owner);
                command.Parameters.AddWithValue("ownertype", ownertype);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask DeleteRegisterInformationRecords(string table, Guid owner, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", owner);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask<bool> InsertRegisterInformationObject(UnigueID unigueID, DateTime period, Guid owner, NameAndText ownertype, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query_field = "uid, period, owner, ownertype";
                string query_values = "@uid, @period, @owner, @ownertype";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);
                command.Parameters.AddWithValue("period", period);
                command.Parameters.AddWithValue("owner", owner);
                command.Parameters.AddWithValue("ownertype", ownertype);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();

                return true;
            }
            else
                return false;
        }

        public async ValueTask<bool> UpdateRegisterInformationObject(UnigueID unigueID, DateTime period, Guid owner, NameAndText ownertype, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"UPDATE {table} SET period = @period, owner = @owner, ownertype = @ownertype";

                foreach (string field in fieldArray)
                    query += ", " + field + " = @" + field;

                query += " WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);
                command.Parameters.AddWithValue("period", period);
                command.Parameters.AddWithValue("owner", owner);
                command.Parameters.AddWithValue("ownertype", ownertype);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();

                return true;
            }
            else
                return false;
        }

        public async ValueTask<SelectRegisterInformationObject_Record> SelectRegisterInformationObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            SelectRegisterInformationObject_Record record = new();

            if (DataSource != null)
            {
                string query = "SELECT uid, period, owner, ownertype";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += $" FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                record.Result = reader.HasRows;

                while (await reader.ReadAsync())
                {
                    record.Period = (DateTime)reader["period"];
                    record.Owner = (Guid)reader["owner"];
                    record.OwnerType = reader["ownertype"] != DBNull.Value ? (NameAndText)reader["ownertype"] : new NameAndText();

                    foreach (string field in fieldArray)
                        fieldValue[field] = reader[field];
                }
                await reader.CloseAsync();
            }

            return record;
        }

        public async ValueTask RemoveRegisterInformationRecords(Guid UID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask DeleteRegisterInformationObject(string table, UnigueID uid)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", uid.UGuid);

                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region RegistersAccumulation

        public async ValueTask SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                if (QuerySelect.Where.Count > 0)
                {
                    foreach (Where ItemFilter in QuerySelect.Where)
                        command.Parameters.AddWithValue(ItemFilter.Alias, ItemFilter.Value);
                }

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> fieldValue = [];
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    if (QuerySelect.FieldAndAlias.Count > 0)
                    {
                        Dictionary<string, string> joinValue = [];
                        joinValueList.Add(reader["uid"].ToString()!, joinValue);

                        foreach (ValueName<string> fieldAndAlias in QuerySelect.FieldAndAlias)
                            joinValue.Add(fieldAndAlias.Name, reader[fieldAndAlias.Name].ToString() ?? "");
                    }
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, NameAndText ownertype, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid, period, income, owner, ownertype";
                string query_values = "@uid, @period, @income, @owner, @ownertype";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("period", period);
                command.Parameters.AddWithValue("income", income);
                command.Parameters.AddWithValue("owner", owner);
                command.Parameters.AddWithValue("ownertype", ownertype);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask<List<DateTime>?> SelectRegisterAccumulationRecordPeriodForOwner(string table, Guid owner, DateTime? periodCurrent = null, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"SELECT DISTINCT period FROM {table} WHERE owner = @owner";

                if (periodCurrent != null)
                    query += " AND date_trunc('day', period::timestamp) != date_trunc('day', @period_current::timestamp)";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", owner);

                if (periodCurrent != null)
                    command.Parameters.AddWithValue("period_current", periodCurrent);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    List<DateTime> result = [];

                    while (await reader.ReadAsync())
                        result.Add((DateTime)reader["period"]);

                    await reader.CloseAsync();
                    return result;
                }
                else
                {
                    await reader.CloseAsync();
                    return null;
                }
            }
            else
                return null;
        }

        public async ValueTask DeleteRegisterAccumulationRecords(string table, Guid owner, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", owner);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask SelectRegisterAccumulationTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = "SELECT uid";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += " FROM " + table;

                NpgsqlCommand command = DataSource.CreateCommand(query);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> fieldValue = [];
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);
                }
                await reader.CloseAsync();
            }
        }

        public async ValueTask InsertRegisterAccumulationTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid";
                string query_values = "@uid";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask RemoveRegisterAccumulationTablePartRecords(Guid UID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async ValueTask DeleteRegisterAccumulationTablePartRecords(string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table}";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region InformationShema

        public async ValueTask<bool> IfExistsTable(string tableName)
        {
            if (DataSource != null)
            {
                string query = "SELECT table_name " +
                               "FROM information_schema.tables " +
                               "WHERE table_schema = 'public' AND table_type = 'BASE TABLE' AND table_name = @table_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("table_name", tableName);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                bool hasRows = reader.HasRows;

                await reader.CloseAsync();

                return hasRows;
            }
            else
                return false;
        }

        public async ValueTask<bool> IfExistsColumn(string tableName, string columnName)
        {
            if (DataSource != null)
            {
                string query = "SELECT column_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_schema = 'public' AND table_name = @table_name AND column_name = @column_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("table_name", tableName);
                command.Parameters.AddWithValue("column_name", columnName);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                bool hasRows = reader.HasRows;

                await reader.CloseAsync();

                return hasRows;
            }
            else
                return false;
        }

        public async ValueTask<ConfigurationInformationSchema> SelectInformationSchema()
        {
            ConfigurationInformationSchema informationSchema = new ConfigurationInformationSchema();

            if (DataSource != null)
            {
                //
                // Таблиці та стовпчики
                //

                string query = "SELECT table_name, column_name, data_type, udt_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_schema = 'public' " +
                               "ORDER BY table_name, column_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    informationSchema.Append(
                        reader["table_name"].ToString()?.ToLower() ?? "",
                        reader["column_name"].ToString()?.ToLower() ?? "",
                        reader["data_type"].ToString() ?? "",
                        reader["udt_name"].ToString() ?? "");
                }
                await reader.CloseAsync();

                //
                // Індекси
                //

                query = "SELECT tablename, indexname FROM pg_indexes WHERE schemaname = 'public' ORDER BY indexname";

                command = DataSource.CreateCommand(query);
                reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    informationSchema.AppendIndex(
                        reader["tablename"].ToString()?.ToLower() ?? "",
                        reader["indexname"].ToString()?.ToLower() ?? "");
                }
                await reader.CloseAsync();
            }

            return informationSchema;
        }

        public async ValueTask<List<string>> GetTableList()
        {
            List<string> tables = [];

            if (DataSource != null)
            {
                string query = "SELECT table_name " +
                               "FROM information_schema.tables " +
                               "WHERE table_schema = 'public' AND table_type = 'BASE TABLE'" +
                               "ORDER BY table_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    tables.Add(reader["table_name"].ToString()?.ToLower() ?? "");

                await reader.CloseAsync();
            }

            return tables;
        }

        public async ValueTask<List<string>> GetSpecialTableList()
        {
            List<string> tables = [];

            if (DataSource != null)
            {
                string query = "SELECT table_name " +
                               "FROM information_schema.tables " +
                               "WHERE table_schema = 'public' AND table_type = 'BASE TABLE' AND substr(table_name, 0, 12) = 'tab_special' " +
                               "ORDER BY table_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    tables.Add(reader["table_name"].ToString()?.ToLower() ?? "");

                await reader.CloseAsync();
            }

            return tables;
        }

        #endregion

        #region SQL

        /// <summary>
        /// Вставка даних.
        /// </summary>
        /// <param name="table">Таблиця</param>
        /// <param name="paramQuery">Поля і значення</param>
        /// <returns></returns>
        public async ValueTask<int> InsertSQL(string table, Dictionary<string, object> paramQuery, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                if (paramQuery.Count == 0) return -1;

                string query_field = string.Join(", ", paramQuery.Keys);
                string query_values = "@" + string.Join(", @", paramQuery.Keys);

                string insertQuery = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})"; ;

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(insertQuery, transaction.Connection, transaction) : DataSource.CreateCommand(insertQuery);

                foreach (KeyValuePair<string, object> param in paramQuery)
                    command.Parameters.AddWithValue(param.Key, param.Value);

                return await command.ExecuteNonQueryAsync();
            }
            else
                return -1;
        }

        /// <summary>
        /// Виконує запит без повернення даних
        /// </summary>
        /// <param name="sqlQuery">Запит</param>
        /// <returns></returns>
        public async ValueTask<int> ExecuteSQL(string query, byte transactionID = 0)
        {
            return await ExecuteSQL(query, null, transactionID);
        }

        /// <summary>
        /// Виконує запит з параметрами без повернення даних
        /// </summary>
        /// <param name="sqlQuery">Запит</param>
        /// <param name="paramQuery">Параметри</param>
        /// <returns></returns>
        public async ValueTask<int> ExecuteSQL(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                await using NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                if (paramQuery != null)
                    foreach (KeyValuePair<string, object> param in paramQuery)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                return await command.ExecuteNonQueryAsync();
            }
            else
                return -1;
        }

        public async ValueTask<object?> ExecuteSQLScalar(string query, Dictionary<string, object>? paramQuery = null, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ? new NpgsqlCommand(query, transaction.Connection, transaction) : DataSource.CreateCommand(query);

                if (paramQuery != null)
                    foreach (KeyValuePair<string, object> param in paramQuery)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                return await command.ExecuteScalarAsync();
            }
            else
                return null;
        }

        public async ValueTask<SelectRequest_Record> SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery = null)
        {
            SelectRequest_Record record = new();

            if (DataSource != null)
            {
                NpgsqlCommand command = DataSource.CreateCommand(selectQuery);

                if (paramQuery != null)
                    foreach (KeyValuePair<string, object> param in paramQuery)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                NpgsqlDataReader reader = await command.ExecuteReaderAsync();

                record.Result = reader.HasRows;

                int columnsCount = reader.FieldCount;
                record.ColumnsName = new string[columnsCount];

                for (int n = 0; n < columnsCount; n++)
                    record.ColumnsName[n] = reader.GetName(n);

                while (await reader.ReadAsync())
                {
                    Dictionary<string, object> objRow = [];

                    for (int i = 0; i < columnsCount; i++)
                        objRow.Add(record.ColumnsName[i], reader[i]);

                    record.ListRow.Add(objRow);
                }
                await reader.CloseAsync();
            }

            return record;
        }

        public async ValueTask<DateTime> SelectCurrentTimestamp()
        {
            if (DataSource != null)
            {
                NpgsqlCommand command = DataSource.CreateCommand("SELECT CURRENT_TIMESTAMP::timestamp");
                object? result = await command.ExecuteScalarAsync();

                return result != null ? (DateTime)result : DateTime.Now;
            }
            else
                return DateTime.Now;
        }

        #endregion
    }
}