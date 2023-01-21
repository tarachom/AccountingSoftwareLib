/*
Copyright (C) 2019-2023 TARAKHOMYN YURIY IVANOVYCH
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

using Npgsql;

namespace AccountingSoftware
{
    public class PostgreSQL : IDataBase
    {
        #region Connect

        private NpgsqlDataSource? DataSource { get; set; }

        public void Open(string connectionString)
        {
            NpgsqlDataSourceBuilder dataBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataBuilder.MapComposite<UuidAndText>("uuidtext");

            DataSource = dataBuilder.Build();

            Start();
        }

        public bool Open2(string Server, string UserId, string Password, int Port, string Database, out Exception exception)
        {
            exception = new Exception();

            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};Database={Database};SSLMode=Prefer;";

            try
            {
                Open(conString);
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        public bool TryConnectToServer(string Server, string UserId, string Password, int Port, string Database, out Exception exception)
        {
            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};Database={Database};SSLMode=Prefer;";

            exception = new Exception();

            try
            {
                Open(conString);
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        public bool CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database, out Exception exception, out bool IsExistsDatabase)
        {
            exception = new Exception();
            IsExistsDatabase = false;

            string conString = $"Server={Server};Username={UserId};Password={Password};Port={Port};SSLMode=Prefer;";

            try
            {
                Open(conString);
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }

            if (DataSource != null)
            {
                string sql = "SELECT EXISTS(" +
                    "SELECT datname FROM pg_catalog.pg_database WHERE lower(datname) = lower(@databasename));";

                NpgsqlCommand command = DataSource.CreateCommand(sql);
                command.Parameters.AddWithValue("databasename", Database);

                bool resultSql = false;

                try
                {
                    IsExistsDatabase = resultSql = Boolean.Parse(command.ExecuteScalar()?.ToString() ?? "false");
                }
                catch (Exception e)
                {
                    exception = e;
                    return false;
                }

                if (!resultSql)
                {
                    sql = "CREATE DATABASE " + Database;
                    command = DataSource.CreateCommand(sql);

                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        exception = e;
                        return false;
                    }
                }

                return true;
            }
            else
                return false;
        }

        private void Start()
        {
            if (DataSource != null)
            {
                //
                // uuidtext
                //

                string query = "SELECT 'Exist' FROM pg_type WHERE typname = 'uuidtext'";
                NpgsqlCommand command = DataSource.CreateCommand(query);
                object? result = command.ExecuteScalar();

                if (!(result != null && result.ToString() == "Exist"))
                {
                    ExecuteSQL($@"
CREATE TYPE uuidtext AS 
(
    uuid uuid, 
    text text
)");
                    DataSource.OpenConnection().ReloadTypes();
                }

                //
                // ПідключитиДодаток_UUID_OSSP
                //

                ExecuteSQL("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\"");

                //
                // Системні таблиці
                //

                /*
                Таблиця для запису інформації про зміни в регістрах накопичення.
                На основі цієї інформації мають розраховуватися віртуальні таблиці регістрів
                */

                ExecuteSQL($@"
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
        }

        #endregion

        #region SpetialTable

        public void SpetialTableRegAccumTrigerAdd(DateTime period, Guid document, string regAccumName, string info, byte transactionID = 0)
        {
            Dictionary<string, object> paramQuery = new Dictionary<string, object>();
            paramQuery.Add("datewrite", DateTime.Now);
            paramQuery.Add("period", period);
            paramQuery.Add("regname", regAccumName);
            paramQuery.Add("document", document);
            paramQuery.Add("execute", false);
            paramQuery.Add("info", info);

            ExecuteSQL($@"
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
    @datewrite,
    @period,
    @regname,
    @document,
    @execute,
    @info
)", paramQuery, transactionID);
        }

        public void SpetialTableRegAccumTrigerExecute(Action<DateTime, string> ExecuteСalculation, Action<List<string>> ExecuteFinalСalculation)
        {
            if (DataSource != null)
            {
                string query = @$"
WITH trigers AS
(
    SELECT
        uid,
        date_trunc('day', period::timestamp) AS period,
        regname
    FROM {SpecialTables.RegAccumTriger}
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
                NpgsqlDataReader reader = command.ExecuteReader();

                bool hasRows = reader.HasRows;
                List<string> regAccumNameList = new List<string>();

                while (reader.Read())
                {
                    DateTime period = (DateTime)reader["period"];
                    string regname = (string)reader["regname"];

                    //
                    // ExecuteСalculation
                    //

                    ExecuteСalculation.Invoke(period, regname);

                    if (!regAccumNameList.Contains(regname))
                        regAccumNameList.Add(regname);
                }
                reader.Close();

                //
                // ExecuteFinalСalculation
                //

                if (hasRows)
                    ExecuteFinalСalculation.Invoke(regAccumNameList);

                //
                // Clear
                //

                if (hasRows)
                {
                    query = $"DELETE FROM {SpecialTables.RegAccumTriger} WHERE execute = true";
                    ExecuteSQL(query);
                }
            }
        }

        public void ClearSpetialTableRegAccumTriger()
        {
            string query = $"DELETE FROM {SpecialTables.RegAccumTriger}";
            ExecuteSQL(query);
        }

        #endregion

        #region Transaction

        private readonly object loсked = new Object();
        private Dictionary<byte, NpgsqlTransaction> OpenTransaction = new Dictionary<byte, NpgsqlTransaction>();
        private volatile byte TransactionCounter = 0;

        public byte BeginTransaction()
        {
            if (DataSource != null)
            {
                byte TransactionID = 0;
                NpgsqlTransaction Transaction = DataSource.OpenConnection().BeginTransaction();

                lock (loсked)
                {
                    if (TransactionCounter >= byte.MaxValue)
                        TransactionCounter = 0;

                    TransactionID = ++TransactionCounter;

                    OpenTransaction.Add(TransactionID, Transaction);
                }

                return TransactionID;
            }
            else
                return 0;
        }

        public void CommitTransaction(byte transactionID)
        {
            if (transactionID == 0)
                throw new IndexOutOfRangeException("Не задана транзація");

            if (OpenTransaction.ContainsKey(transactionID))
            {
                NpgsqlTransaction Transaction = OpenTransaction[transactionID];

                lock (loсked)
                {
                    OpenTransaction.Remove(transactionID);
                }

                Transaction?.Commit();
                Transaction?.Connection?.Close();
            }
            else
                throw new IndexOutOfRangeException("Невірний номер транзації");
        }

        public void RollbackTransaction(byte transactionID)
        {
            if (transactionID == 0)
                throw new IndexOutOfRangeException("Не задана транзація");

            if (OpenTransaction.ContainsKey(transactionID))
            {
                NpgsqlTransaction Transaction = OpenTransaction[transactionID];

                lock (loсked)
                {
                    OpenTransaction.Remove(transactionID);
                }

                Transaction?.Rollback();
                Transaction?.Connection?.Close();
            }
            else
                throw new IndexOutOfRangeException("Невірний номер транзації");
        }

        private NpgsqlTransaction? GetTransactionByID(byte transactionID)
        {
            if (transactionID == 0)
                return null;

            if (OpenTransaction.ContainsKey(transactionID))
                return OpenTransaction[transactionID];
            else
                return null;
        }

        #endregion

        #region Constants

        public bool SelectAllConstants(string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"SELECT {string.Join(", ", fieldArray)} FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", Guid.Empty);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                if (reader.Read())
                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public bool SelectConstants(string table, string field, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"SELECT {field} FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", Guid.Empty);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                if (reader.Read())
                    fieldValue.Add(field, reader[field]);

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public void SaveConstants(string table, string field, object fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"INSERT INTO {table} (uid, {field}) VALUES (@uid, @{field}) " +
                               $"ON CONFLICT (uid) DO UPDATE SET {field} = @{field}";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", Guid.Empty);
                command.Parameters.AddWithValue(field, fieldValue);

                command.ExecuteNonQuery();
            }
        }

        public void SelectConstantsTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = $"SELECT uid, {string.Join(", ", fieldArray)} FROM {table}";
                NpgsqlCommand command = DataSource.CreateCommand(query);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);
                }
                reader.Close();
            }
        }

        public void InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
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
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteConstantsTablePartRecords(string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table}";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Directory

        public void InsertDirectoryObject(DirectoryObject directoryObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
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

                NpgsqlCommand nCommand = DataSource.CreateCommand(query);
                nCommand.Parameters.AddWithValue("uid", directoryObject.UnigueID.UGuid);

                foreach (string field in fieldArray)
                    nCommand.Parameters.AddWithValue(field, fieldValue[field]);

                nCommand.ExecuteNonQuery();
            }
        }

        public void UpdateDirectoryObject(DirectoryObject directoryObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                if (fieldArray.Length == 0)
                    return;

                string query = $"UPDATE {table} SET ";

                int count = 0;
                foreach (string field in fieldArray)
                    query += $"{(count++ > 0 ? ", " : "")}{field} = @{field}";

                query += " WHERE uid = @uid";

                NpgsqlCommand nCommand = DataSource.CreateCommand(query);
                nCommand.Parameters.AddWithValue("uid", directoryObject.UnigueID.UGuid);

                foreach (string field in fieldArray)
                    nCommand.Parameters.AddWithValue(field, fieldValue[field]);

                nCommand.ExecuteNonQuery();
            }
        }

        public bool SelectDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"SELECT uid, {string.Join(", ", fieldArray)} FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                NpgsqlDataReader reader = command.ExecuteReader();

                bool hasRows = reader.HasRows;

                while (reader.Read())
                    foreach (string field in fieldArray)
                        fieldValue[field] = reader[field];

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public void DeleteDirectoryObject(UnigueID unigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                command.ExecuteNonQuery();
            }
        }

        public void SelectDirectoryPointers(Query QuerySelect, List<DirectoryPointer> listDirectoryPointer)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object>? fields = null;

                    if (QuerySelect.Field.Count > 0 || QuerySelect.FieldAndAlias.Count > 0)
                    {
                        fields = new Dictionary<string, object>();

                        foreach (string field in QuerySelect.Field)
                            fields.Add(field, reader[field]);

                        foreach (NameValue<string> field in QuerySelect.FieldAndAlias)
                            fields.Add(field.Value!, reader[field.Value!]);
                    }

                    DirectoryPointer elementPointer = new DirectoryPointer();
                    elementPointer.Init(new UnigueID((Guid)reader["uid"]), fields);

                    listDirectoryPointer.Add(elementPointer);
                }
                reader.Close();
            }
        }

        public bool FindDirectoryPointer(Query QuerySelect, ref DirectoryPointer directoryPointer)
        {
            if (DataSource != null)
            {
                QuerySelect.Limit = 1;

                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                if (QuerySelect.Where.Count > 0)
                    foreach (Where field in QuerySelect.Where)
                        command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                if (reader.Read())
                    directoryPointer.Init(new UnigueID(reader["uid"]), null);

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        /// <summary>
        /// Вибирає значення полів по вказівнику для представлення
        /// </summary>
        /// <param name="QuerySelect">Запит</param>
        /// <param name="fieldPresentation">Поля які використовуються для представлення</param>
        /// <returns></returns>
        public string GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                string presentation = "";

                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    for (int i = 0; i < fieldPresentation.Length; i++)
                        presentation += (i > 0 ? ", " : "") + reader[fieldPresentation[i]].ToString();

                reader.Close();

                return presentation;
            }
            else
                return "";
        }

        public void DeleteDirectoryTempTable(DirectorySelect directorySelect)
        {
            if (DataSource != null)
            {
                if (directorySelect.QuerySelect.CreateTempTable == true &&
                    directorySelect.QuerySelect.TempTable != "" &&
                     directorySelect.QuerySelect.TempTable.Substring(0, 4) == "tmp_")
                {
                    string query = $"DROP TABLE IF EXISTS {directorySelect.QuerySelect.TempTable}";

                    NpgsqlCommand command = DataSource.CreateCommand(query);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void SelectDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = $"SELECT uid, {string.Join(", ", fieldArray)} FROM {table} WHERE owner = @owner";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);
                }
                reader.Close();
            }
        }

        public void SelectDirectoryTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    foreach (NameValue<string> field in QuerySelect.FieldAndAlias)
                        fieldValue.Add(field.Value!, reader[field.Value!]);
                }
                reader.Close();
            }
        }

        public void InsertDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
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
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Document

        public bool SelectDocumentObject(UnigueID unigueID, ref bool spend, ref DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = "SELECT uid, spend, spend_date ";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += $" FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                while (reader.Read())
                {
                    spend = (bool)reader["spend"];
                    spend_date = (DateTime)reader["spend_date"];

                    foreach (string field in fieldArray)
                        fieldValue[field] = reader[field];
                }
                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public void InsertDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query_field = "uid, spend, spend_date";
                string query_values = "@uid, @spend, @spend_date";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);
                command.Parameters.AddWithValue("spend", spend);
                command.Parameters.AddWithValue("spend_date", spend_date);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"UPDATE {table} SET spend = @spend, spend_date = @spend_date";

                foreach (string field in fieldArray)
                    query += ", " + field + " = @" + field;

                query += " WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", unigueID.UGuid);
                command.Parameters.AddWithValue("spend", spend);
                command.Parameters.AddWithValue("spend_date", spend_date);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteDocumentObject(UnigueID unigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", unigueID.UGuid);

                command.ExecuteNonQuery();
            }
        }

        public void SelectDocumentPointer(Query QuerySelect, List<DocumentPointer> listDocumentPointer)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fields = new Dictionary<string, object>();

                    if (QuerySelect.Field.Count > 0 || QuerySelect.FieldAndAlias.Count > 0)
                    {
                        foreach (string field in QuerySelect.Field)
                            fields.Add(field, reader[field]);

                        foreach (NameValue<string> field in QuerySelect.FieldAndAlias)
                            fields.Add(field.Value!, reader[field.Value!]);
                    }

                    DocumentPointer elementPointer = new DocumentPointer();
                    elementPointer.Init(new UnigueID(reader["uid"]), fields);

                    listDocumentPointer.Add(elementPointer);
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Вибирає значення полів по вказівнику для представлення
        /// </summary>
        /// <param name="QuerySelect">Запит</param>
        /// <param name="fieldPresentation">Поля які використовуються для представлення</param>
        /// <returns></returns>
        public string GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                string presentation = "";

                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    for (int i = 0; i < fieldPresentation.Length; i++)
                        presentation += (i > 0 ? ", " : "") + reader[fieldPresentation[i]].ToString();

                reader.Close();

                return presentation;
            }
            else
                return "";
        }

        public void SelectDocumentTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = "SELECT uid";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += $" FROM {table} WHERE owner = @owner";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);
                }
                reader.Close();
            }
        }

        public void SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = QuerySelect.Construct();

                NpgsqlCommand command = DataSource.CreateCommand(query);

                foreach (Where field in QuerySelect.Where)
                    command.Parameters.AddWithValue(field.Alias, field.Value);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    foreach (NameValue<string> field in QuerySelect.FieldAndAlias)
                        fieldValue.Add(field.Value!, reader[field.Value!]);
                }
                reader.Close();
            }
        }

        public void InsertDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
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
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteDocumentTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", ownerUnigueID.UGuid);

                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Journal

        public void SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listJournalDocument,
            DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null)
        {
            if (DataSource != null)
            {
                string query = "";
                int counter = 0;

                foreach (string table in tables)
                {
                    //if (typeDocSelect != null)
                    //{
                    //	bool existTypeDoc = false;

                    //	foreach (string typeDoc in typeDocSelect)
                    //		if (typeDocument[counter] == typeDoc)
                    //		{
                    //			existTypeDoc = true;
                    //			break;
                    //		}

                    //	if (!existTypeDoc)
                    //	{
                    //		counter++;
                    //		continue;
                    //	}
                    //}

                    query += (counter > 0 ? "\nUNION " : "") +
                        $"(SELECT uid, docname, docdate, docnomer, spend, spend_date, '{typeDocument[counter]}' AS type_doc FROM {table} \n" +
                        "WHERE docdate >= @periodstart AND docdate <= @periodend)";

                    counter++;
                }

                query += "\nORDER BY docdate";

                //Console.WriteLine(query);

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("periodstart", periodStart);
                command.Parameters.AddWithValue("periodend", periodEnd);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    JournalDocument document = new JournalDocument()
                    {
                        UnigueID = new UnigueID(reader["uid"]),
                        DocName = reader["docname"]?.ToString() ?? "",
                        DocDate = reader["docdate"]?.ToString() ?? "",
                        DocNomer = reader["docnomer"]?.ToString() ?? "",
                        Spend = (bool)reader["spend"],
                        SpendDate = (DateTime)reader["spend_date"],
                        TypeDocument = reader["type_doc"]?.ToString() ?? ""
                    };

                    listJournalDocument.Add(document);
                }
                reader.Close();
            }
        }

        #endregion

        #region RegistersInformation

        public void SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList)
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

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    foreach (NameValue<string> field in QuerySelect.FieldAndAlias)
                        fieldValue.Add(field.Value!, reader[field.Value!]);
                }
                reader.Close();
            }
        }

        public void InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid, period, owner";
                string query_values = "@uid, @period, @owner";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("period", period);
                command.Parameters.AddWithValue("owner", owner);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteRegisterInformationRecords(string table, Guid owner, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", owner);

                command.ExecuteNonQuery();
            }
        }

        public void InsertRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query_field = "uid, period, owner";
                string query_values = "@uid, @period, @owner";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", registerInformationObject.UnigueID.UGuid);
                command.Parameters.AddWithValue("period", registerInformationObject.Period);
                command.Parameters.AddWithValue("owner", registerInformationObject.Owner);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void UpdateRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = $"UPDATE {table} SET period = @period, owner = @owner";

                foreach (string field in fieldArray)
                    query += ", " + field + " = @" + field;

                query += " WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", registerInformationObject.UnigueID.UGuid);
                command.Parameters.AddWithValue("period", registerInformationObject.Period);
                command.Parameters.AddWithValue("owner", registerInformationObject.Owner);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public bool SelectRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue)
        {
            if (DataSource != null)
            {
                string query = "SELECT uid, period, owner";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += $" FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", registerInformationObject.UnigueID.UGuid);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                while (reader.Read())
                {
                    registerInformationObject.Period = (DateTime)reader["period"];
                    registerInformationObject.Owner = (Guid)reader["owner"];

                    foreach (string field in fieldArray)
                        fieldValue[field] = reader[field];
                }

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public void DeleteRegisterInformationObject(string table, UnigueID uid)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE uid = @uid";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("uid", uid.UGuid);

                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region RegistersAccumulation

        public void SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList)
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

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in QuerySelect.Field)
                        fieldValue.Add(field, reader[field]);

                    foreach (NameValue<string> field in QuerySelect.FieldAndAlias)
                        fieldValue.Add(field.Value!, reader[field.Value!]);
                }
                reader.Close();
            }
        }

        public void InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query_field = "uid, period, income, owner";
                string query_values = "@uid, @period, @income, @owner";

                if (fieldArray.Length != 0)
                {
                    query_field += ", " + string.Join(", ", fieldArray);
                    query_values += ", @" + string.Join(", @", fieldArray);
                }

                string query = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);
                command.Parameters.AddWithValue("period", period);
                command.Parameters.AddWithValue("income", income);
                command.Parameters.AddWithValue("owner", owner);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public List<DateTime>? SelectRegisterAccumulationRecordPeriodForOwner(string table, Guid owner, DateTime? periodCurrent = null, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"SELECT DISTINCT period FROM {table} WHERE owner = @owner ";

                if (periodCurrent != null)
                    query += " AND date_trunc('day', period::timestamp) != date_trunc('day', @period_current::timestamp)";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", owner);

                if (periodCurrent != null)
                    command.Parameters.AddWithValue("period_current", periodCurrent);

                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    List<DateTime> result = new List<DateTime>();

                    while (reader.Read())
                        result.Add((DateTime)reader["period"]);

                    reader.Close();
                    return result;
                }
                else
                {
                    reader.Close();
                    return null;
                }
            }
            else
                return null;
        }

        public void DeleteRegisterAccumulationRecords(string table, Guid owner, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table} WHERE owner = @owner";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("owner", owner);

                command.ExecuteNonQuery();
            }
        }

        public void SelectRegisterAccumulationTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList)
        {
            if (DataSource != null)
            {
                string query = "SELECT uid";

                if (fieldArray.Length != 0)
                    query += ", " + string.Join(", ", fieldArray);

                query += " FROM " + table;

                NpgsqlCommand command = DataSource.CreateCommand(query);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Dictionary<string, object> fieldValue = new Dictionary<string, object>();
                    fieldValueList.Add(fieldValue);

                    fieldValue.Add("uid", reader["uid"]);

                    foreach (string field in fieldArray)
                        fieldValue.Add(field, reader[field]);
                }
                reader.Close();
            }
        }

        public void InsertRegisterAccumulationTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0)
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
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.Parameters.AddWithValue("uid", UID);

                foreach (string field in fieldArray)
                    command.Parameters.AddWithValue(field, fieldValue[field]);

                command.ExecuteNonQuery();
            }
        }

        public void DeleteRegisterAccumulationTablePartRecords(string table, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                string query = $"DELETE FROM {table}";

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region InformationShema

        public bool IfExistsTable(string tableName)
        {
            if (DataSource != null)
            {
                string query = "SELECT table_name " +
                               "FROM information_schema.tables " +
                               "WHERE table_schema = 'public' AND table_type = 'BASE TABLE' AND table_name = @table_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("table_name", tableName);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public bool IfExistsColumn(string tableName, string columnName)
        {
            if (DataSource != null)
            {
                string query = "SELECT column_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_schema = 'public' AND table_name = @table_name AND column_name = @column_name";

                NpgsqlCommand command = DataSource.CreateCommand(query);
                command.Parameters.AddWithValue("table_name", tableName);
                command.Parameters.AddWithValue("column_name", columnName);

                NpgsqlDataReader reader = command.ExecuteReader();
                bool hasRows = reader.HasRows;

                reader.Close();

                return hasRows;
            }
            else
                return false;
        }

        public ConfigurationInformationSchema SelectInformationSchema()
        {
            ConfigurationInformationSchema informationSchema = new ConfigurationInformationSchema();

            if (DataSource != null)
            {
                //
                // Таблиці та стовпчики
                //

                string query = "SELECT table_name, column_name, data_type, udt_name " +
                               "FROM information_schema.columns " +
                               "WHERE table_schema = 'public'";

                NpgsqlCommand command = DataSource.CreateCommand(query);

                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    informationSchema.Append(
                        reader["table_name"].ToString()?.ToLower() ?? "",
                        reader["column_name"].ToString()?.ToLower() ?? "",
                        reader["data_type"].ToString() ?? "",
                        reader["udt_name"].ToString() ?? "");
                }
                reader.Close();

                //
                // Індекси
                //

                query = "SELECT tablename, indexname FROM pg_indexes WHERE schemaname = 'public'";

                command = DataSource.CreateCommand(query);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    informationSchema.AppendIndex(
                        reader["tablename"].ToString()?.ToLower() ?? "",
                        reader["indexname"].ToString()?.ToLower() ?? "");
                }
                reader.Close();
            }

            return informationSchema;
        }

        #endregion

        #region SQL

        /// <summary>
        /// Вставка даних.
        /// </summary>
        /// <param name="table">Таблиця</param>
        /// <param name="paramQuery">Поля і значення</param>
        /// <returns></returns>
        public int InsertSQL(string table, Dictionary<string, object> paramQuery, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                if (paramQuery.Count == 0) return -1;

                string query_field = string.Join(", ", paramQuery.Keys);
                string query_values = "@" + string.Join(", @", paramQuery.Keys);

                string insertQuery = $"INSERT INTO {table} ({query_field}) VALUES ({query_values})"; ;

                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(insertQuery, transaction.Connection, transaction) :
                    DataSource.CreateCommand(insertQuery);

                foreach (KeyValuePair<string, object> param in paramQuery)
                    command.Parameters.AddWithValue(param.Key, param.Value);

                return command.ExecuteNonQuery();
            }
            else
                return -1;
        }

        /// <summary>
        /// Виконує запит без повернення даних
        /// </summary>
        /// <param name="sqlQuery">Запит</param>
        /// <returns></returns>
        public int ExecuteSQL(string query, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                return command.ExecuteNonQuery();
            }
            else
                return -1;
        }

        /// <summary>
        /// Виконує запит з параметрами без повернення даних
        /// </summary>
        /// <param name="sqlQuery">Запит</param>
        /// <param name="paramQuery">Параметри</param>
        /// <returns></returns>
        public int ExecuteSQL(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0)
        {
            if (DataSource != null)
            {
                NpgsqlTransaction? transaction = GetTransactionByID(transactionID);
                NpgsqlCommand command = (transaction != null) ?
                    new NpgsqlCommand(query, transaction.Connection, transaction) :
                    DataSource.CreateCommand(query);

                if (paramQuery != null)
                    foreach (KeyValuePair<string, object> param in paramQuery)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                return command.ExecuteNonQuery();
            }
            else
                return -1;
        }

        /// <summary>
        /// Виконання запиту SELECT
        /// </summary>
        /// <param name="selectQuery">Запит</param>
        /// <param name="paramQuery">Параметри запиту</param>
        /// <param name="columnsName">Масив стовпців даних</param>
        /// <param name="listRow">Список рядочків даних</param>
        public void SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery, out string[] columnsName, out List<object[]> listRow)
        {
            columnsName = new string[] { };
            listRow = new List<object[]>();

            if (DataSource != null)
            {
                NpgsqlCommand command = DataSource.CreateCommand(selectQuery);

                if (paramQuery != null)
                    foreach (KeyValuePair<string, object> param in paramQuery)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                NpgsqlDataReader reader = command.ExecuteReader();

                int columnsCount = reader.FieldCount;
                columnsName = new string[columnsCount];

                for (int n = 0; n < columnsCount; n++)
                    columnsName[n] = reader.GetName(n);

                while (reader.Read())
                {
                    object[] objRow = new object[columnsCount];

                    for (int i = 0; i < columnsCount; i++)
                        objRow[i] = reader[i];

                    listRow.Add(objRow);
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Виконання запиту SELECT
        /// </summary>
        /// <param name="selectQuery">Запит</param>
        /// <param name="paramQuery">Параметри запиту</param>
        /// <param name="columnsName">Масив стовпців даних</param>
        /// <param name="listRow">Список рядочків даних</param>
        public void SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery, out string[] columnsName, out List<Dictionary<string, object>> listRow)
        {
            columnsName = new string[] { };
            listRow = new List<Dictionary<string, object>>();

            if (DataSource != null)
            {
                NpgsqlCommand command = DataSource.CreateCommand(selectQuery);

                if (paramQuery != null)
                    foreach (KeyValuePair<string, object> param in paramQuery)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                NpgsqlDataReader reader = command.ExecuteReader();

                int columnsCount = reader.FieldCount;
                columnsName = new string[columnsCount];

                for (int n = 0; n < columnsCount; n++)
                    columnsName[n] = reader.GetName(n);

                while (reader.Read())
                {
                    Dictionary<string, object> objRow = new Dictionary<string, object>();

                    for (int i = 0; i < columnsCount; i++)
                        objRow.Add(columnsName[i], reader[i]);

                    listRow.Add(objRow);
                }
                reader.Close();
            }
        }

        #endregion
    }
}