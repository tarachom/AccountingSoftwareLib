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

namespace AccountingSoftware
{
    /// <summary>
    /// Інтерфейс для роботи з базою даних.
    /// </summary>
    public interface IDataBase
    {
        #region Open

        //ValueTask Open(string connectionString, bool startScript = true);
        ValueTask<bool> Open(string Server, string UserId, string Password, int Port, string Database);
        ValueTask<bool> TryConnectToServer(string Server, string UserId, string Password, int Port, string Database);
        ValueTask<bool> IfExistDatabase(string Server, string UserId, string Password, int Port, string Database);
        ValueTask<bool> CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database);
        void Close();

        #endregion

        #region Exception
        Exception? Exception { get; }

        #endregion

        #region SpetialTable RegAccumTriger

        ValueTask SpetialTableRegAccumTrigerAdd(DateTime period, Guid document, string regAccumName, string info, byte transactionID = 0);
        ValueTask SpetialTableRegAccumTrigerExecute(Guid session, Action<DateTime, string> ExecuteСalculation, Action<List<string>> ExecuteFinalСalculation);

        #endregion

        #region SpetialTable Users

        ValueTask<Guid?> SpetialTableUsersAddOrUpdate(bool isNew, Guid? uid, string name, string fullname, string password, string info);
        ValueTask<Dictionary<string, string>> SpetialTableUsersShortSelect();
        List<Dictionary<string, object>> SpetialTableUsersExtendetList();
        Dictionary<string, object>? SpetialTableUsersExtendetUser(Guid user_uid);
        ValueTask<bool> SpetialTableUsersIsExistUser(string name, Guid? uid = null, Guid? not_uid = null);
        ValueTask<bool> SpetialTableUsersDelete(Guid user_uid, string name);
        ValueTask<string> SpetialTableUsersGetFullName(Guid user_uid);
        ValueTask<(Guid, Guid)?> SpetialTableUsersLogIn(string user, string password);

        #endregion

        #region SpetialTable ActiveUsers

        ValueTask<bool> SpetialTableActiveUsersUpdateSession(Guid session_uid);
        ValueTask SpetialTableActiveUsersCloseSession(Guid session_uid);
        List<Dictionary<string, object>> SpetialTableActiveUsersSelect();

        #endregion

        #region SpetialTable FullTextSearch

        void SpetialTableFullTextSearchAddValue(UuidAndText obj, string value);
        void SpetialTableFullTextSearchDelete(UnigueID uid, byte transactionID = 0);
        List<Dictionary<string, object>>? SpetialTableFullTextSearchSelect(string findtext, uint offset = 0);

        #endregion

        #region Transaction

        ValueTask<byte> BeginTransaction();
        ValueTask CommitTransaction(byte transactionID);
        ValueTask RollbackTransaction(byte transactionID);

        #endregion

        #region Constants

        ValueTask<bool> SelectAllConstants(string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<bool> SelectConstants(string table, string field, Dictionary<string, object> fieldValue);
        ValueTask SaveConstants(string table, string field, object fieldValue);

        ValueTask SelectConstantsTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        ValueTask InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask DeleteConstantsTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region Func (Directory, Document)

        ValueTask<bool> IsExistUniqueID(UnigueID unigueID, string table);

        #endregion 

        #region Directory

        ValueTask<bool> InsertDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<bool> UpdateDirectoryObject(UnigueID unigueID, bool deletion_label, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue);
        ValueTask<SelectDirectoryObject_Record> SelectDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask DeleteDirectoryObject(UnigueID unigueID, string table, byte transactionID = 0);

        ValueTask SelectDirectoryPointers(Query QuerySelect, List<DirectoryPointer> listDirectoryPointer);
        ValueTask<FindDirectoryPointer_Record> FindDirectoryPointer(Query QuerySelect, DirectoryPointer directoryPointer);
        ValueTask<string> GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation);
        ValueTask DeleteDirectoryTempTable(DirectorySelect directorySelect);

        ValueTask SelectDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        ValueTask SelectDirectoryTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);

        ValueTask InsertDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask DeleteDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Document

        ValueTask<bool> InsertDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<bool> UpdateDocumentObject(UnigueID unigueID, bool? deletion_label, bool? spend, DateTime? spend_date, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue);
        ValueTask<SelectDocumentObject_Record> SelectDocumentObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask DeleteDocumentObject(UnigueID unigueID, string table, byte transactionID = 0);

        void SelectDocumentPointer(Query QuerySelect, List<DocumentPointer> listDocumentPointer);
        ValueTask<string> GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation);

        void SelectDocumentTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        void SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);

        ValueTask InsertDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask DeleteDocumentTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Journal

        void SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listDocumentPointer,
            DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null);

        #endregion

        #region RegisterInformation

        void SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);
        ValueTask InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask DeleteRegisterInformationRecords(string table, Guid owner, byte transactionID = 0);

        ValueTask InsertRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask UpdateRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        bool SelectRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask DeleteRegisterInformationObject(string table, UnigueID uid);

        #endregion

        #region RegisterAccumulation

        void SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);
        ValueTask InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask<List<DateTime>?> SelectRegisterAccumulationRecordPeriodForOwner(string table, Guid owner, DateTime? periodCurrent = null, byte transactionID = 0);
        ValueTask DeleteRegisterAccumulationRecords(string table, Guid owner, byte transactionID = 0);

        void SelectRegisterAccumulationTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        void InsertRegisterAccumulationTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        void DeleteRegisterAccumulationTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region InformationShema

        ConfigurationInformationSchema SelectInformationSchema();
        bool IfExistsTable(string tableName);
        bool IfExistsColumn(string tableName, string columnName);

        #endregion

        #region SQL

        int InsertSQL(string table, Dictionary<string, object> paramQuery, byte transactionID = 0);
        ValueTask<int> ExecuteSQL(string query, byte transactionID = 0);
        ValueTask<int> ExecuteSQL(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0);
        ValueTask<object?> ExecuteSQLScalar(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0);
        void SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery, out string[] columnsName, out List<object[]> listRow);
        void SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery, out string[] columnsName, out List<Dictionary<string, object>> listRow);

        #endregion
    }
}