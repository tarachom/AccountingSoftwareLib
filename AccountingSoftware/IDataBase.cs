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

        Task<bool> Open(string Server, string UserId, string Password, int Port, string Database);
        Task<bool> TryConnectToServer(string Server, string UserId, string Password, int Port, string Database);
        Task<bool> IfExistDatabase(string Server, string UserId, string Password, int Port, string Database);
        Task<bool> CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database);
        Task CreateSpecialTables();
        void Close();

        #endregion

        #region Exception
        Exception? Exception { get; }

        #endregion

        #region SpetialTable MessageError

        Task SpetialTableMessageErrorAdd(Guid user_uid, string nameProcess, Guid uidObject, string typeObject, string nameObject, string message, char message_type, byte transactionID = 0);
        Task<SelectRequest_Record> SpetialTableMessageErrorSelect(Guid user_uid, UniqueID? unigueIDObjectWhere = null, int? limit = null);
        Task SpetialTableMessageErrorClear(Guid user_uid);
        Task SpetialTableMessageErrorClearOld(Guid user_uid);

        #endregion

        #region SpetialTable RegAccumTriger

        Task SpetialTableRegAccumTrigerAdd(DateTime period, Guid document, string regAccumName, string info, byte transactionID = 0);
        Task SpetialTableRegAccumTrigerExecute(Guid session, Func<DateTime, string, Task> ExecuteСalculation, Func<List<string>, Task> ExecuteFinalСalculation);

        Task SpetialTableRegAccumTrigerDocIgnoreAdd(Guid users, Guid session, Guid document, string info, byte transactionID = 0);
        Task SpetialTableRegAccumTrigerDocIgnoreClear(Guid users, Guid session, Guid? document = null, byte transactionID = 0);

        #endregion

        #region SpetialTable Users

        Task<Guid?> SpetialTableUsersAddOrUpdate(bool isNew, Guid? uid, string name, string fullname, string password, string info);
        Task<Dictionary<string, string>> SpetialTableUsersShortSelect();
        Task<SelectRequest_Record> SpetialTableUsersExtendetList();
        Task<SelectRequest_Record?> SpetialTableUsersExtendetUser(Guid user_uid);
        Task<bool> SpetialTableUsersIsExistUser(string name, Guid? uid = null, Guid? not_uid = null);
        Task<bool> SpetialTableUsersDelete(Guid user_uid, string name);
        Task<string> SpetialTableUsersGetFullName(Guid user_uid);
        Task<(Guid User, Guid Session)?> SpetialTableUsersLogIn(string user, string password, TypeForm typeForm);

        #endregion

        #region SpetialTable ActiveUsers

        Task<bool> SpetialTableActiveUsersUpdateSession(Guid session_uid);
        Task SpetialTableActiveUsersCloseSession(Guid session_uid);
        Task<SelectRequest_Record> SpetialTableActiveUsersSelect();

        #endregion

        #region SpetialTable FullTextSearch

        Task SpetialTableFullTextSearchAddValue(UuidAndText obj, string value, string dictTSearch = Configuration.DefaultDictTSearch);
        Task SpetialTableFullTextSearchDelete(UniqueID uid, byte transactionID = 0);
        Task<SelectRequest_Record?> SpetialTableFullTextSearchSelect(string findtext, uint offset = 0, string dictTSearch = Configuration.DefaultDictTSearch);
        Task<SelectRequest_Record> SpetialTableFullTextSearchDictList();
        Task<bool> SpetialTableFullTextSearchIfExistDict(string dictTSearch);

        #endregion

        #region SpetialTable ObjectUpdateTriger

        Task SpetialTableObjectUpdateTrigerAdd(UuidAndText obj, char operation);
        Task<SelectRequest_Record> SpetialTableObjectUpdateTrigerSelect(DateTime after);
        Task SpetialTableObjectUpdateTrigerClear();
        Task SpetialTableObjectUpdateTrigerClearOld();

        #endregion

        #region SpetialTable LockedObject

        Task<UniqueID> SpetialTableLockedObjectAdd(Guid user_uid, Guid session_uid, UuidAndText obj);
        Task<SelectRequest_Record> SpetialTableLockedObjectSelect();
        Task<bool> SpetialTableLockedObjectIsLock(UniqueID lockKey);
        Task<LockedObject_Record> SpetialTableLockedObjectLockInfo(UuidAndText obj);
        Task SpetialTableLockedObjectClear(UniqueID uid);

        #endregion

        #region SpetialTable VersionsHistory

        Task SpetialTableObjectVersionsHistoryAdd(Guid version_id, Guid user_uid, UuidAndText obj, Dictionary<string, object>? fieldValue, char operation, string info = "", byte transactionID = 0);
        Task<SelectVersionsHistoryList_Record> SpetialTableObjectVersionsHistoryList(UuidAndText obj);
        Task<SelectVersionsHistoryItem_Record> SpetialTableObjectVersionsHistorySelect(Guid version_id, UuidAndText obj);
        Task SpetialTableObjectVersionsHistoryRemove(Guid version_id, UuidAndText obj, byte transactionID = 0);
        Task SpetialTableObjectVersionsHistoryRemoveAll(UuidAndText obj, byte transactionID = 0);

        Task SpetialTableTablePartVersionsHistoryAdd(Guid version_id, Guid user_uid, UuidAndText objowner, string tablepart, Dictionary<Guid, Dictionary<string, object>> listFieldValue, byte transactionID = 0);
        Task<SelectVersionsHistoryTablePart_Record> SpetialTableTablePartVersionsHistorySelect(Guid version_id, UuidAndText obj);

        #endregion

        #region Transaction

        Task<byte> BeginTransaction();
        Task CommitTransaction(byte transactionID);
        Task RollbackTransaction(byte transactionID);

        #endregion

        #region Constants

        Task<SelectConstants_Record> SelectConstants(string table, string field);
        Task SaveConstants(string table, string field, object fieldValue);

        Task SelectConstantsTablePartRecords(Query QuerySelect, string[] fieldArray, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);
        Task InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        Task RemoveConstantsTablePartRecords(Guid UID, string table, byte transactionID = 0);
        Task DeleteConstantsTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region Func (Directory, Document)

        Task<bool> IsExistUniqueID(UniqueID uniqueID, string table);
        Task<SplitSelectToPages_Record> SplitSelectToPages(Query QuerySelect, UniqueID? uniqueID, int pageSize = 1000);
        Task<SplitSelectToPages_Record> SplitSelectToPagesForJournal(string query, Dictionary<string, object> paramQuery, int pageSize = 1000);

        #endregion

        #region Directory

        Task<bool> InsertDirectoryObject(UniqueID uniqueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        Task<bool> UpdateDirectoryObject(UniqueID uniqueID, bool deletion_label, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue);
        Task<SelectDirectoryObject_Record> SelectDirectoryObject(UniqueID uniqueID, string table, string[] fieldArray, Dictionary<string, object>? fieldValue = null);
        Task DeleteDirectoryObject(UniqueID uniqueID, string table, byte transactionID = 0);

        Task SelectDirectoryPointers(Query QuerySelect, List<(UniqueID UniqueID, Dictionary<string, object>? Fields)> listPointers);
        Task SelectDirectoryPointersHierarchical(Query QuerySelect, List<(UniqueID UniqueID, UniqueID Parent, int Level, bool IsFolder, Dictionary<string, object>? Fields)> listPointers);
        Task<UniqueID?> FindDirectoryPointer(Query QuerySelect);
        Task<string> GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation);
        Task DeleteDirectoryTempTable(DirectorySelect directorySelect);

        Task SelectDirectoryTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);

        Task InsertDirectoryTablePartRecords(Guid UID, UniqueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        Task RemoveDirectoryTablePartRecords(Guid UID, UniqueID ownerUnigueID, string table, byte transactionID = 0);
        Task DeleteDirectoryTablePartRecords(UniqueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Document

        Task<bool> InsertDocumentObject(UniqueID uniqueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        Task<bool> UpdateDocumentObject(UniqueID uniqueID, bool? deletion_label, bool? spend, DateTime? spend_date, string table, string[]? fieldArray, Dictionary<string, object>? fieldValue);
        Task<SelectDocumentObject_Record> SelectDocumentObject(UniqueID uniqueID, string table, string[] fieldArray, Dictionary<string, object>? fieldValue = null);
        Task DeleteDocumentObject(UniqueID uniqueID, string table, byte transactionID = 0);

        Task SelectDocumentPointer(Query QuerySelect, List<(UniqueID UniqueID, Dictionary<string, object>? Fields)> listPointers);
        Task<UniqueID?> FindDocumentPointer(Query QuerySelect);
        Task<string> GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation);

        Task SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);

        Task InsertDocumentTablePartRecords(Guid UID, UniqueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        Task RemoveDocumentTablePartRecords(Guid UID, UniqueID ownerUnigueID, string table, byte transactionID = 0);
        Task DeleteDocumentTablePartRecords(UniqueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Journal

        Task SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listDocumentPointer,
            DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null, bool? spendDocSelect = null);

        #endregion

        #region RegisterInformation

        Task SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);
        Task InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, NameAndText ownertype, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        Task DeleteRegisterInformationRecords(string table, Guid owner, byte transactionID = 0);

        Task<bool> InsertRegisterInformationObject(UniqueID uniqueID, DateTime period, Guid owner, NameAndText ownertype, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        Task<bool> UpdateRegisterInformationObject(UniqueID uniqueID, DateTime period, Guid owner, NameAndText ownertype, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        Task<SelectRegisterInformationObject_Record> SelectRegisterInformationObject(UniqueID uniqueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        Task RemoveRegisterInformationRecords(Guid UID, string table, byte transactionID = 0);
        Task DeleteRegisterInformationObject(string table, UniqueID uid);

        #endregion

        #region RegisterAccumulation

        Task SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList, Dictionary<string, Dictionary<string, string>> joinValueList);
        Task InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, NameAndText ownertype, int ownerlinenum, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        Task<List<DateTime>?> SelectRegisterAccumulationRecordPeriodForOwner(string table, Guid owner, DateTime? periodCurrent = null, byte transactionID = 0);
        Task DeleteRegisterAccumulationRecords(string table, Guid owner, byte transactionID = 0);

        Task SelectRegisterAccumulationTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        Task InsertRegisterAccumulationTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        Task RemoveRegisterAccumulationTablePartRecords(Guid UID, string table, byte transactionID = 0);
        Task DeleteRegisterAccumulationTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region InformationShema

        Task<ConfigurationInformationSchema> SelectInformationSchema();
        Task<bool> IfExistsTable(string tableName);
        Task<bool> IfExistsColumn(string tableName, string columnName);
        Task<List<string>> GetTableList();
        Task<List<string>> GetSpecialTableList();

        #endregion

        #region SQL

        Task<int> InsertSQL(string table, Dictionary<string, object> paramQuery, byte transactionID = 0, int commandTimeout = 0);
        Task<int> ExecuteSQL(string query, byte transactionID = 0, int commandTimeout = 0);
        Task<int> ExecuteSQL(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0, int commandTimeout = 0);
        Task<object?> ExecuteSQLScalar(string query, Dictionary<string, object>? paramQuery = null, byte transactionID = 0, int commandTimeout = 0);
        Task<SelectRequest_Record> SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery = null, int commandTimeout = 0);
        Task<DateTime> SelectCurrentTimestamp();

        #endregion
    }
}