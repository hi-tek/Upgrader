﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Upgrader.Infrastructure;

namespace Upgrader.Schema
{
    /// <summary>
    /// Collection of all indexes in the specified table.
    /// </summary>
    public class IndexCollection : IEnumerable<IndexInfo>
    {
        private readonly Database database;
        private readonly string tableName;

        internal IndexCollection(Database database, string tableName)
        {
            this.database = database;
            this.tableName = tableName;
        }

        /// <summary>
        /// Gets index information for the specified index. Returns null if the specified index name does not exist.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        /// <returns>Index information.</returns>
        public IndexInfo this[string indexName]
        {
            get
            {
                Validate.IsNotNullAndNotEmpty(indexName, nameof(indexName));
                Validate.MaxLength(indexName, nameof(indexName), database.MaxIdentifierLength);

                return database.GetIndexColumnNames(tableName, indexName).Any() ? new IndexInfo(database, tableName, indexName) : null;
            }
        }

        /// <summary>
        /// Adds an index to the table.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="indexName">Index name. If not name is given, name is set by convention.</param>
        public void Add(string columnName, string indexName = null)
        {
            Validate.IsNotNullAndNotEmpty(columnName, nameof(columnName));

            Add(new[] { columnName }, false, indexName);
        }

        /// <summary>
        /// Adds an index to the table.
        /// </summary>
        /// <param name="columnNames">Column names.</param>
        /// <param name="indexName">Index name. If not name is given, name is set by convention.</param>
        public void Add(string[] columnNames, string indexName = null)
        {
            Add(columnNames, false, indexName);
        }

        /// <summary>
        /// Adds a unique index to the table.
        /// </summary>
        /// <param name="columnName">Column name.</param>
        /// <param name="indexName">Index name. If not name is given, name is set by convention.</param>
        public void AddUnique(string columnName, string indexName = null)
        {
            Validate.IsNotNullAndNotEmpty(columnName, nameof(columnName));

            Add(new[] { columnName }, true, indexName);
        }

        /// <summary>
        /// Adds a unique index to the table.
        /// </summary>
        /// <param name="columnNames">Column names.</param>
        /// <param name="indexName">Index name. If not name is given, name is set by convention.</param>
        public void AddUnique(string[] columnNames, string indexName = null)
        {
            Add(columnNames, true, indexName);
        }

        /// <summary>
        /// Removes an index from the table.
        /// </summary>
        /// <param name="indexName">Index name.</param>
        public void Remove(string indexName)
        {
            Validate.IsNotNullAndNotEmpty(indexName, nameof(indexName));
            Validate.MaxLength(indexName, nameof(indexName), database.MaxIdentifierLength);

            database.RemoveIndex(tableName, indexName);
        }

        /// <summary>
        /// Removes all indexes from the table.
        /// </summary>
        public void RemoveAll()
        {
            foreach (var index in this)
            {
                Remove(index.IndexName);
            }
        }

        /// <summary>
        /// Gets an IEnumerator of all indexes.
        /// </summary>
        /// <returns>IEnumerator of all indexes.</returns>
        public IEnumerator<IndexInfo> GetEnumerator()
        {
            return database
                .GetIndexNames(tableName)
                .Select(indexName => new IndexInfo(database, tableName, indexName))
                .GetEnumerator();
        }

        /// <summary>
        /// Gets an IEnumerator of all indexes.
        /// </summary>
        /// <returns>IEnumerator of all indexes.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void Add(string[] columnNames, bool unique, string indexName = null)
        {
            Validate.IsNotNullAndNotEmptyEnumerable(columnNames, nameof(columnNames));
            Validate.MaxLength(columnNames, nameof(columnNames), database.MaxIdentifierLength);
            Validate.IsNotEmpty(indexName, nameof(indexName));
            Validate.MaxLength(indexName, nameof(indexName), database.MaxIdentifierLength);

            indexName = indexName ?? database.NamingConvention.GetIndexNamingConvention(tableName, columnNames, unique);
            database.AddIndex(tableName, columnNames, unique, indexName);
        }
    }
}