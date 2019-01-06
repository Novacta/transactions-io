// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    [DeploymentItem("Data", "Data")]
    public class EditFileManagerTests
    {
        [TestMethod()]
        public void EditFileManagerTest()
        {
            EditFileManagerTester.Constructor.ManagedPathIsNull();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            EditFileManagerTester.Dispose();
        }

        [TestMethod()]
        public void OnPrepareTest()
        {
            EditFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        public void OnCommitTest()
        {
            EditFileManagerTester.FileAlreadyExists.OnCommit();
        }

        [TestMethod()]
        public void OnRollbackTest()
        {
            EditFileManagerTester.FileAlreadyExists.OnRollback();
            EditFileManagerTester.FileAlreadyExists.OnRollbackNoScope();
            EditFileManagerTester.FileIsNew.OnRollback();
            EditFileManagerTester.FileIsNew.OnRollbackNoScope();
        }

        [TestMethod()]
        public void EnlistVolatileTest()
        {
            EditFileManagerTester.CurrentTransactionIsNull();
        }
    }
}