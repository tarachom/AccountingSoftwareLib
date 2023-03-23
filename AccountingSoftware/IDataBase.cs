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

        void Open(string connectionString);
        bool Open(string Server, string UserId, string Password, int Port, string Database, out Exception exception);
        bool TryConnectToServer(string Server, string UserId, string Password, int Port, string Database, out Exception exception);
        bool CreateDatabaseIfNotExist(string Server, string UserId, string Password, int Port, string Database, out Exception exception, out bool IsExistsDatabase);
        void Close();

        #endregion

        #region SpetialTable RegAccumTriger

        void SpetialTableRegAccumTrigerAdd(DateTime period, Guid document, string regAccumName, string info, byte transactionID = 0);
        void SpetialTableRegAccumTrigerExecute(Guid session, Action<DateTime, string> ExecuteСalculation, Action<List<string>> ExecuteFinalСalculation);

        #endregion

        #region SpetialTable Users

        Guid? SpetialTableUsersAddOrUpdate(bool isNew, Guid? uid, string name, string fullname, string password, string info);
        Dictionary<string, string> SpetialTableUsersShortSelect();
        List<Dictionary<string, object>> SpetialTableUsersExtendetList();
        Dictionary<string, object>? SpetialTableUsersExtendetUser(Guid user_uid);
        bool SpetialTableUsersIsExistUser(string name, Guid? uid = null, Guid? not_uid = null);
        bool SpetialTableUsersDelete(Guid user_uid, string name);
        string SpetialTableUsersGetFullName(Guid user_uid);
        (Guid, Guid)? SpetialTableUsersLogIn(string user, string password);

        #endregion

        #region SpetialTable ActiveUsers

        bool SpetialTableActiveUsersUpdateSession(Guid session_uid);
        void SpetialTableActiveUsersCloseSession(Guid session_uid);
        List<Dictionary<string, object>> SpetialTableActiveUsersSelect();

        #endregion

        #region Transaction

        byte BeginTransaction();
        void CommitTransaction(byte transactionID);
        void RollbackTransaction(byte transactionID);

        #endregion

        #region Constants

        bool SelectAllConstants(string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        bool SelectConstants(string table, string field, Dictionary<string, object> fieldValue);
        void SaveConstants(string table, string field, object fieldValue);

        void SelectConstantsTablePartRecords(string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        void InsertConstantsTablePartRecords(Guid UID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        void DeleteConstantsTablePartRecords(string table, byte transactionID = 0);

        #endregion

        #region Directory

        void InsertDirectoryObject(DirectoryObject directoryObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        void UpdateDirectoryObject(DirectoryObject directoryObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        bool SelectDirectoryObject(UnigueID unigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        void DeleteDirectoryObject(UnigueID unigueID, string table, byte transactionID = 0);

        void SelectDirectoryPointers(Query QuerySelect, List<DirectoryPointer> listDirectoryPointer);
        bool FindDirectoryPointer(Query QuerySelect, ref DirectoryPointer directoryPointer);
        string GetDirectoryPresentation(Query QuerySelect, string[] fieldPresentation);
        void DeleteDirectoryTempTable(DirectorySelect directorySelect);

        void SelectDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        void SelectDirectoryTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);

        void InsertDirectoryTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        void DeleteDirectoryTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Document

        void InsertDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        void UpdateDocumentObject(UnigueID unigueID, bool spend, DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        bool SelectDocumentObject(UnigueID unigueID, ref bool spend, ref DateTime spend_date, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        void DeleteDocumentObject(UnigueID unigueID, string table, byte transactionID = 0);

        void SelectDocumentPointer(Query QuerySelect, List<DocumentPointer> listDocumentPointer);
        string GetDocumentPresentation(Query QuerySelect, string[] fieldPresentation);

        void SelectDocumentTablePartRecords(UnigueID ownerUnigueID, string table, string[] fieldArray, List<Dictionary<string, object>> fieldValueList);
        void SelectDocumentTablePartRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);

        void InsertDocumentTablePartRecords(Guid UID, UnigueID ownerUnigueID, string table, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        void DeleteDocumentTablePartRecords(UnigueID ownerUnigueID, string table, byte transactionID = 0);

        #endregion

        #region Journal

        void SelectJournalDocumentPointer(string[] tables, string[] typeDocument, List<JournalDocument> listDocumentPointer,
            DateTime periodStart, DateTime periodEnd, string[]? typeDocSelect = null);

        #endregion

        #region RegisterInformation

        void SelectRegisterInformationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);
        void InsertRegisterInformationRecords(Guid UID, string table, DateTime period, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        void DeleteRegisterInformationRecords(string table, Guid owner, byte transactionID = 0);

        void InsertRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        void UpdateRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        bool SelectRegisterInformationObject(RegisterInformationObject registerInformationObject, string table, string[] fieldArray, Dictionary<string, object> fieldValue);
        void DeleteRegisterInformationObject(string table, UnigueID uid);

        #endregion

        #region RegisterAccumulation

        void SelectRegisterAccumulationRecords(Query QuerySelect, List<Dictionary<string, object>> fieldValueList);
        void InsertRegisterAccumulationRecords(Guid UID, string table, DateTime period, bool income, Guid owner, string[] fieldArray, Dictionary<string, object> fieldValue, byte transactionID = 0);
        List<DateTime>? SelectRegisterAccumulationRecordPeriodForOwner(string table, Guid owner, DateTime? periodCurrent = null, byte transactionID = 0);
        void DeleteRegisterAccumulationRecords(string table, Guid owner, byte transactionID = 0);

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
        int ExecuteSQL(string query, byte transactionID = 0);
        int ExecuteSQL(string query, Dictionary<string, object>? paramQuery, byte transactionID = 0);
        void SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery, out string[] columnsName, out List<object[]> listRow);
        void SelectRequest(string selectQuery, Dictionary<string, object>? paramQuery, out string[] columnsName, out List<Dictionary<string, object>> listRow);

        #endregion
    }
}