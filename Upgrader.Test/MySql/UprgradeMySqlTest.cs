﻿using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Upgrader.MySql;

namespace Upgrader.Test.MySql
{
    [TestClass]
    public class UprgradeMySqlTest : UpgradeTest<MySqlDatabase>
    {
        public UprgradeMySqlTest() : base(new MySqlDatabase("MySql"), "MySql")
        {            
        }

        [ExpectedException(typeof(NotSupportedException))]
        public override void PerformUpgradeWithTransactionModeOneTransactionPerStepDoesRollbackChangesWhenExceptionOccurs()
        {
            base.PerformUpgradeWithTransactionModeOneTransactionPerStepDoesRollbackChangesWhenExceptionOccurs();
        }
    }
}
