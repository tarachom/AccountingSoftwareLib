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

namespace AccountingSoftware
{
    /// <summary>
    /// Інтерфейс для роботи з базою даних.
    /// </summary>
    public interface IDataBase
    {
        #region Open

        ValueTask<bool> Open(string Server, string UserId, string Password, int Port, string Database);
        ValueTask<bool> TryConnectToServer(string Server, string UserId, string Password, int Port, string Database);
        ValueTask<bool> IfExistDatabase(string Server, string UserId, string Password, int Port, string Database);
        ValueTask<bool> CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database);
        ValueTask CreateSpecialTables();
        void Close();

        #endregion

        #region Exception
        Exception? Exception { get; }

        #endregion

        #region SpetialTable MessageError

        ValueTask SpetialTableMessageErrorAdd(Guid user_uid, string nameProcess, Guid uidObject, string typeObject, string nameObject, string message, char message_type, byte transactionID = 0);
        ValueTask<SelectRequest_Record> SpetialTableMessageErrorSelect(Guid user_uid, UnigueID? unigueIDObjectWhere = null, int? limit = null);
        ValueTask SpetialTableMessageErrorClear(Guid user_uid);
        ValueTask SpetialTableMessageErrorClearOld(Guid user_uid);

        #endregion

        #region SpetialTable RegAccumTriger

        ValueTask SpetialTableRegAccumTrigerAdd(DateTime period, Guid document, string regAccumName, string info, byte transactionID = 0);
        ValueTask SpetialTableRegAccumTrigerExecute(Guid session, Func<DateTime, string, ValueTask> ExecuteСalculation, Func<List<string>, ValueTask> ExecuteFinalСalculation);

        ValueTask SpetialTableRegAccumTrigerDocIgnoreAdd(Guid users, Guid session, Guid document, string info, byte transactionID = 0);
        ValueTask SpetialTableRegAccumTrigerDocIgnoreClear(Guid users, Guid session, Guid? document = null, byte transactionID = 0);

        #endregion

        #region SpetialTable Users

        ValueTask<Guid?> SpetialTableUsersAddOrUpdate(bool isNew, Guid? uid, string name, string fullname, string password, string info);
        ValueTask<Dictionary<string, string>> SpetialTableUsersShortSelect();
        ValueTask<SelectRequest_Record> SpetialTableUsersExtendetList();
        ValueTask<SelectRequest_Record?> SpetialTableUsersExtendetUser(Guid user_uid);
        ValueTask<bool> SpetialTableUsersIsExistUser(string name, Guid? uid = null, Guid? not_uid = null);
        ValueTask<bool> SpetialTableUsersDelete(Guid user_uid, string name);
        ValueTask<string> SpetialTableUsersGetFullName(Guid user_uid);
        ValueTask<(Guid User, Guid Session)?> SpetialTableUsersLogIn(string user, string password, TypeForm typeForm);

        #endregion

        #region SpetialTable ActiveUsers

        ValueTask<bool> SpetialTableActiveUsersUpdateSession(Guid session_uid);
        ValueTask SpetialTableActiveUsersCloseSession(Guid session_uid);
        ValueTask<SelectRequest_Record> SpetialTableActiveUsersSelect();

        #endregion

        #region SpetialTable FullTextSearch

        ValueTask SpetialTableFullTextSearchAddValue(UuidAndText obj, string value, string dictTSearch = Configuration.DefaultDictTSearch);
        ValueTask SpetialTableFullTextSearchDelete(UnigueID uid, byte transactionID = 0);
        ValueTask<SelectRequest_Record?> SpetialTableFullTextSearchSelect(string findtext, uint offset = 0, string dictTSearch = Configuration.DefaultDictTSearch);
        ValueTask<SelectRequest_Record> SpetialTableFullTextSearchDictList();
        ValueTask<bool> SpetialTableFullTextSearchIfExistDict(string dictTSearch);

        #endregion

        #region SpetialTable ObjectUpdateTriger

        ValueTask SpetialTableObjectUpdateTrigerAdd(UuidAndText obj, char operation);
        ValueTask<SelectRequest_Record> SpetialTableObjectUpdateTrigerSelect(DateTime after);
        ValueTask SpetialTableObjectUpdateTrigerClear();
        ValueTask SpetialTableObjectUpdateTrigerClearOld();

        #endregion

        #region SpetialTable LockedObject

        ValueTask<UnigueID> SpetialTableLockedObjectAdd(Guid user_uid, Guid session_uid, UuidAndText obj);
        ValueTask<SelectRequest_Record> SpetialTableLockedObjectSelect();
        ValueTask<bool> SpetialTableLockedObjectIsLock(UnigueID lockKey);
        ValueTask<LockedObject_Record> SpetialTableLockedObjectLockInfo(UuidAndText obj);
        ValueTask SpetialTableLockedObjectClear(UnigueID uid);

        #endregion

        #region SpetialTable VersionsHistory

        ValueTask SpetialTableObjectVersionsHistoryAdd(Guid version_id, Guid user_uid, UuidAndText obj, Dictionary<string, object>? fieldValue, char operation, string info = "", byte transactionID = 0);
        ValueTask<SelectVersionsHistoryList_Record> SpetialTableObjectVersionsHistoryList(UuidAndText obj);
        ValueTask<SelectVersionsHistoryItem_Record> SpetialTableObjectVersionsHistorySelect(Guid version_id, UuidAndText obj);
        ValueTask SpetialTableObjectVersionsHistoryRemove(Guid version_id, UuidAndText obj, byte transactionID = 0);
        ValueTask SpetialTableObjectVersionsHistoryRemoveAll(UuidAndText obj, byte transactionID = 0);

        ValueTask SpetialTableTablePartVersionsHistoryAdd(Guid version_id, Guid user_uid, UuidAndText objowner, string tablepart, Dictionary<Guid, Dictionary<string, object>> listFieldValue, byte transactionID = 0);
        ValueTask<SelectVersionsHistoryTablePart_Record> SpetialTableTablePartVersionsHistorySelect(Guid version_id, UuidAndText obj);

        #endregion

        #region Transaction

        ValueTask<byte> BeginTransaction();
        ValueTask CommitTransaction(byte transactionID);
        ValueTask RollbackTransaction(byte transactionID);

        #endregion

        #region Constants

        ValueTask<SelectConstants_Record> SelectConstants(string table, string field);
        ValueTask SaveConstants(string table, string field, object fieldValue);

        ValueTask SelectConstantsTablePartRecords(Query QuerySelect, string[] fieldArray, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);
        ValueTask InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask RemoveConstantsTablePartRecords(Guid UID, string table, byte transactionID = 0);
        ValueTask DeleteConstantsTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region Func (Directory, Document)

        ValueTask<bool> IsExistUniqueID(UnigueID unigueID, string table);
        ValueTask<SplitSelectToPages_Record> SplitSelectToPages(Query QuerySelect, UnigueID? unigueID, int pageSize = 1000);
        ValueTask<SplitSelectToPages_Record> SplitSelectToPagesForJournal(string query, Dictionary<string, object> paramQuery, int pageSize = 1000);

        #endregion

        #region Directory

        ValueTask<bool> InsertDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<bool> UpdateDirectoryObject(UnigueID unigueID, bool deletion_label, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue);
        ValueTask<SelectDirectoryObject_Record> SelectDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object>? fieldValue = null);
        ValueTask DeleteDirectoryObject(UnigueID unigueID, string table, byte transactionID = 0);

        ValueTask SelectDirectoryPointers(Query QuerySelect, List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> listPointers);
        ValueTask SelectDirectoryPointersHierarchical(Query QuerySelect, List<(UnigueID UnigueID, UnigueID Parent, int Level, Dictionary<string, object>? Fields)> listPointers);
        ValueTask<UnigueID?> FindDirectoryPointer(Query QuerySelect);
        ValueTask<string> GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation);
        ValueTask DeleteDirectoryTempTable(DirectorySelect directorySelect);

        ValueTask SelectDirectoryTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);

        ValueTask InsertDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask RemoveDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, byte transactionID = 0);
        ValueTask DeleteDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Document

        ValueTask<bool> InsertDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<bool> UpdateDocumentObject(UnigueID unigueID, bool? deletion_label, bool? spend, DateTime? spend_date, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue);
        ValueTask<SelectDocumentObject_Record> SelectDocumentObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object>? fieldValue = null);
        ValueTask DeleteDocumentObject(UnigueID unigueID, string table, byte transactionID = 0);

        ValueTask SelectDocumentPointer(Query QuerySelect, List<(UnigueID UnigueID, Dictionary<string, object>? Fields)> listPointers);
        ValueTask<UnigueID?> FindDocumentPointer(Query QuerySelect);
        ValueTask<string> GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation);

        ValueTask SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);

        ValueTask InsertDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask RemoveDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, byte transactionID = 0);
        ValueTask DeleteDocumentTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Journal

        ValueTask SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listDocumentPointer,
            DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null, bool? spendDocSelect = null);

        #endregion

        #region RegisterInformation

        ValueTask SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);
        ValueTask InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, NameAndText ownertype, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask DeleteRegisterInformationRecords(string table, Guid owner, byte transactionID = 0);

        ValueTask<bool> InsertRegisterInformationObject(UnigueID unigueID, DateTime period, Guid owner, NameAndText ownertype, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<bool> UpdateRegisterInformationObject(UnigueID unigueID, DateTime period, Guid owner, NameAndText ownertype, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask<SelectRegisterInformationObject_Record> SelectRegisterInformationObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        ValueTask RemoveRegisterInformationRecords(Guid UID, string table, byte transactionID = 0);
        ValueTask DeleteRegisterInformationObject(string table, UnigueID uid);

        #endregion

        #region RegisterAccumulation

        ValueTask SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);
        ValueTask InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, NameAndText ownertype, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask<List<DateTime>?> SelectRegisterAccumulationRecordPeriodForOwner(string table, Guid owner, DateTime? periodCurrent = null, byte transactionID = 0);
        ValueTask DeleteRegisterAccumulationRecords(string table, Guid owner, byte transactionID = 0);

        ValueTask SelectRegisterAccumulationTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        ValueTask InsertRegisterAccumulationTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        ValueTask RemoveRegisterAccumulationTablePartRecords(Guid UID, string table, byte transactionID = 0);
        ValueTask DeleteRegisterAccumulationTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region InformationShema

        ValueTask<ConfigurationInformationSchema> SelectInformationSchema();
        ValueTask<bool> IfExistsTable(string tableName);
        ValueTask<bool> IfExistsColumn(string tableName, string columnName);
        ValueTask<List<string>> GetTableList();
        ValueTask<List<string>> GetSpecialTableList();

        #endregion

        #region SQL

        ValueTask<int> InsertSQL(string table, Dictionary<string, object> paramQuery, byte transactionID = 0, int commandTimeout = 0);
        ValueTask<int> ExecuteSQL(string query, byte transactionID = 0, int commandTimeout = 0);
        ValueTask<int> ExecuteSQL(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0, int commandTimeout = 0);
        ValueTask<object?> ExecuteSQLScalar(string query, Dictionary<string, object>? paramQuery = null, byte transactionID = 0, int commandTimeout = 0);
        ValueTask<SelectRequest_Record> SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery = null, int commandTimeout = 0);
        ValueTask<DateTime> SelectCurrentTimestamp();

        #endregion
    }
}