﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Upgrader.Infrastructure;
using Upgrader.Schema;

namespace Upgrader
{
    /// <summary>
    /// Encapsulates high-level data definition language functionality as well as the possibility to reflect on the underlying database.
    /// </summary>
    public abstract class Database : IDisposable
    {
        private readonly DataDefinitionLanguage dataDefinitionLanguage;
        private readonly DataManipulationLanguage dataManipulationLanguage;
        private readonly StructuredQueryLanguage structuredQueryLanguage;
        private readonly InformationSchema informationSchema;

        internal readonly string DatabaseName;
        private readonly string connectionString;
        private readonly string mainConnectionString;

        internal Database(IDbConnection connection, string mainConnectionString, string databaseName = null)
        {
            Connection = connection;
            this.mainConnectionString = mainConnectionString;
            connectionString = connection.ConnectionString;
            this.DatabaseName = databaseName ?? connection.Database;

            Dapper = new Infrastructure.Dapper(connection);
            Tables = new TableCollection(this);
            NamingConvention = new NamingConvention(MaxIdentifierLength);
            dataDefinitionLanguage = new DataDefinitionLanguage(this);
            dataManipulationLanguage = new DataManipulationLanguage(this);
            structuredQueryLanguage = new StructuredQueryLanguage(this);
            informationSchema = new InformationSchema(this);
        }

        internal static string GetConnectionString(string connectionStringOrName)
        {
            return connectionStringOrName.Contains("=") ? connectionStringOrName : ConfigurationManager.ConnectionStrings[connectionStringOrName].ConnectionString;
        }

        internal static string GetMasterConnectionString(string connectionStringOrName, string keyword, string overrideDatabaseName)
        {
            var connectionString = GetConnectionString(connectionStringOrName);

            var connectionStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString,
                [keyword] = overrideDatabaseName
            };

            return connectionStringBuilder.ToString();
        }

        /// <summary>
        /// Underlying ADO.NET connection to the database.
        /// </summary>
        public IDbConnection Connection { get; }

        /// <summary>
        /// Gets or sets the naming convention for this instance.
        /// </summary>
        public NamingConvention NamingConvention { get; set; }

        /// <summary>
        /// Collection of tables in the connected database.
        /// </summary>
        public TableCollection Tables { get; }

        internal abstract string AutoIncrementStatement { get; }

        internal abstract int MaxIdentifierLength { get; }

        internal virtual bool InsertNullForAutoIncrementingPrimaryKey => false;

        internal Infrastructure.Dapper Dapper { get; }

        internal virtual string UnicodeDataType => "NVARCHAR";

        internal virtual string DateTimeDataType => "DATETIME";

        internal virtual void SupportsTransactionalDataDescriptionLanguage()
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "This implementation is enough for now.")]
        public void Dispose()
        {
            Connection.Dispose();
        }

        /// <summary>
        /// Gets if the database exists or not.
        /// </summary>
        public abstract bool Exists
        {
            get;
        }

        /// <summary>
        /// Create database.
        /// </summary>
        public virtual void Create()
        {
            UseMainDatabase();
            dataDefinitionLanguage.CreateDatabase(DatabaseName);
            UseConnectedDatabase();
        }

        /// <summary>
        /// Remove database.
        /// </summary>
        public virtual void Remove()
        {
            UseMainDatabase();
            dataDefinitionLanguage.RemoveDatabase(DatabaseName);
            UseConnectedDatabase();
        }

        internal virtual void UseMainDatabase()
        {
            Connection.Close();
            Connection.ConnectionString = mainConnectionString;
        }

        internal virtual void UseConnectedDatabase()
        {
            Connection.Close();
            Connection.ConnectionString = connectionString;
        }

        internal virtual string[] GetTableNames()
        {
            return informationSchema.GetTableNames();
        }

        internal virtual void AddTable(string tableName, IEnumerable<Column> columns, IEnumerable<ForeignKey> foreignKeys)
        {
            dataDefinitionLanguage.AddTable(tableName, columns, foreignKeys);
        }

        internal void RemoveTable(string tableName)
        {
            dataDefinitionLanguage.RemoveTable(tableName);
        }

        internal void InsertRows<T>(string tableName, IEnumerable<T> rows)
        {
            dataManipulationLanguage.Insert(tableName, rows);
        }

        public void UpdateRows<T>(string tableName, IEnumerable<T> rows)
        {
            dataManipulationLanguage.Update(tableName, rows);
        }

        internal void DeleteRows<T>(string tableName, IEnumerable<T> rows)
        {
            dataManipulationLanguage.Delete(tableName, rows);
        }

        internal virtual string[] GetColumnNames(string tableName)
        {
            return informationSchema.GetColumnNames(tableName);
        }

        internal virtual bool GetColumnNullable(string tableName, string columnName)
        {
            return informationSchema.GetColumnNullable(tableName, columnName);
        }

        internal virtual string GetColumnDataType(string tableName, string columnName)
        {
            return informationSchema.GetColumnDataType(tableName, columnName);
        }

        internal virtual void AddColumn(string tableName, string columnName, string dataType, bool nullable)
        {
            dataDefinitionLanguage.AddColumn(tableName, columnName, dataType, nullable);
        }

        internal virtual void ChangeColumn(string tableName, string columnName, string dataType, bool nullable)
        {
            dataDefinitionLanguage.ChangeColumn(tableName, columnName, dataType, nullable);
        }

        internal virtual void RemoveColumn(string tableName, string columnName)
        {
            dataDefinitionLanguage.RemoveColumn(tableName, columnName);
        }

        internal virtual string GetPrimaryKeyName(string tableName)
        {
            return informationSchema.GetPrimaryKeyName(tableName);
        }

        internal virtual string[] GetPrimaryKeyColumnNames(string tableName, string primaryKeyName)
        {
            return informationSchema.GetPrimaryKeyColumnNames(tableName, primaryKeyName);
        }

        internal virtual void AddPrimaryKey(string tableName, string[] columnNames, string primaryKeyName)
        {
            dataDefinitionLanguage.AddPrimaryKey(tableName, columnNames, primaryKeyName);
        }

        internal virtual void RemovePrimaryKey(string tableName, string primaryKeyName)
        {
            dataDefinitionLanguage.RemovePrimaryKey(tableName, primaryKeyName);
        }

        internal virtual string[] GetForeignKeyNames(string tableName)
        {
            return informationSchema.GetForeignKeyNames(tableName);
        }

        internal virtual string GetForeignKeyForeignTableName(string tableName, string foreignKeyName)
        {
            return informationSchema.GetForeignKeyForeignTableName(tableName, foreignKeyName);
        }

        internal virtual string[] GetForeignKeyColumnNames(string tableName, string foreignKeyName)
        {
            return informationSchema.GetForeignKeyColumnNames(tableName, foreignKeyName);
        }

        internal virtual string[] GetForeignKeyForeignColumnNames(string tableName, string foreignKeyName)
        {
            return informationSchema.GetForeignKeyForeignColumnNames(tableName, foreignKeyName);
        }

        internal virtual void AddForeignKey(string tableName, string[] columnNames, string foreignTableName, string[] foreignColumnNames, string foreignKeyName)
        {
            dataDefinitionLanguage.AddForeignKey(tableName, columnNames, foreignTableName, foreignColumnNames, foreignKeyName);
        }

        internal virtual void RemoveForeignKey(string tableName, string foreignKeyName)
        {
            dataDefinitionLanguage.RemoveForeignKey(tableName, foreignKeyName);
        }

        internal abstract string[] GetIndexNames(string tableName);

        internal abstract bool GetIndexType(string tableName, string indexName);

        internal abstract string[] GetIndexColumnNames(string tableName, string indexName);

        internal void AddIndex(string tableName, string[] columnNames, bool unique, string indexName, string[] includeColumnNames)
        {
            dataDefinitionLanguage.AddIndex(tableName, columnNames, unique, indexName, includeColumnNames);
        }

        internal abstract void RemoveIndex(string tableName, string indexName);

        internal abstract string EscapeIdentifier(string identifier);

        internal abstract string GetSchema(string tableName);

        internal abstract void RenameColumn(string tableName, string columnName, string newColumnName);

        internal virtual void RenameTable(string tableName, string newTableName)
        {
            dataDefinitionLanguage.RenameTable(tableName, newTableName);
        }

        internal void SetColumnValue(string tableName, string columnName, object value)
        {
            dataManipulationLanguage.SetColumnValue(tableName, columnName, value);
        }

        internal abstract bool GetColumnAutoIncrement(string tableName, string columnName);

        internal virtual string GetCatalog()
        {
            return Connection.Database;
        }

        internal abstract string GetLastInsertedAutoIncrementedPrimaryKeyIdentity(string columnName);

        public IEnumerable<object> Select(string tableName, string where)
        {
            return structuredQueryLanguage.Select(tableName, where);
        }

        public IEnumerable<T> Select<T>(string tableName, string where)
        {
            return structuredQueryLanguage.Select<T>(tableName, where);
        }

        internal abstract Database Clone();
    }
}