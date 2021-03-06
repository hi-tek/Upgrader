﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upgrader.Schema;

namespace Upgrader.Test
{
    [TestClass]
    public abstract class ColumnCollectionTest
    {
        protected readonly Database Database;

        protected ColumnCollectionTest(Database database)
        {
            this.Database = database;
        }

        [TestCleanup]
        public void Cleanup()
        {
            Database.Dispose();
        }

        [TestMethod]
        public virtual void AddAddsColumn()
        {
            Database.Tables.Add("AddColumn", new Column<int>("AddColumnId"));

            Database.Tables["AddColumn"].Columns.Add<int>("AddedColumn");

            Assert.AreEqual(2, Database.Tables["AddColumn"].Columns.Count());
        }

        [TestMethod]
        public virtual void RenameRenamesColumn()
        {
            Database.Tables.Add("RenameColumn", new Column<int>("RenameColumnId"));

            Database.Tables["RenameColumn"].Columns.Rename("RenameColumnId", "NewColumnNameId");

            Assert.IsNotNull(Database.Tables["RenameColumn"].Columns["NewColumnNameId"]);
        }

        [TestMethod]
        public virtual void RemoveDropsColumn()
        {
            Database.Tables.Add("RemoveColumn", new Column<int>("RemoveColumnId"), new Column<int>("RemovedColumn"));

            Database.Tables["RemoveColumn"].Columns.Remove("RemovedColumn");

            Assert.AreEqual(1, Database.Tables["RemoveColumn"].Columns.Count());
        }

        [TestMethod]
        public void ColumnsCanBeEnumerated()
        {
            Database.Tables.Add("EnumerateColumn", new Column<int>("EnumerateColumnId"));

            Assert.AreEqual(1, Database.Tables["EnumerateColumn"].Columns.Count(column => column.ColumnName == "EnumerateColumnId"));
        }

        [TestMethod]
        public void ColumnsCanBeAccessedByName()
        {
            Database.Tables.Add("SpecificColumn", new Column<int>("SpecificColumnId"));

            Assert.AreEqual("SpecificColumnId", Database.Tables["SpecificColumn"].Columns["SpecificColumnId"].ColumnName);
        }

        [TestMethod]
        public void ReturnsNullWhenColumnDoesNotExist()
        {
            Database.Tables.Add("DoesNotContainColumn", new Column<int>("ContainsColumnId"));

            Assert.IsNull(Database.Tables["DoesNotContainColumn"].Columns["ContainsColumnId2"]);
        }

        [TestMethod]
        public void ReturnsNotNullTrueWhenColumnDoesExist()
        {
            Database.Tables.Add("ContainsColumn", new Column<int>("ContainsColumnId"));

            Assert.IsNotNull(Database.Tables["ContainsColumn"].Columns["ContainsColumnId"]);
        }

        [TestMethod]
        public virtual void CanAddNonNullColumn()
        {
            Database.Tables.Add("CanAddNotNullColumn", new Column<int>("CanAddNotNullColumnId"));
            Database.Tables["CanAddNotNullColumn"].Rows.Add(new { CanAddNotNullColumnId  = 1 });
            Database.Tables["CanAddNotNullColumn"].Columns.Add("NewNotNullColumn", 5);

            Assert.IsFalse(Database.Tables["CanAddNotNullColumn"].Columns["NewNotNullColumn"].Nullable);
        }

        [TestMethod]
        public void AddNullableAddsNullableColumn()
        {
            Database.Tables.Add("CanAddNullableColumn", new Column<int?>("CanAddNullableColumnId"));
            Database.Tables["CanAddNullableColumn"].Columns.Add<int?>("NullableColumn");

            Assert.IsTrue(Database.Tables["CanAddNullableColumn"].Columns["NullableColumn"].Nullable);
        }
    }
}
