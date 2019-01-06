// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    [DeploymentItem("Data", "Data")]
    public class DeleteFileManagerTests
    {
        [TestMethod()]
        public void DeleteFileManagerTest()
        {
            DeleteFileManagerTester.Constructor.ManagedPathIsNull();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            DeleteFileManagerTester.Dispose();
        }

        [TestMethod()]
        public void OnPrepareTest()
        {
            DeleteFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        public void OnCommitTest()
        {
            DeleteFileManagerTester.FileAlreadyExists.OnCommit();
        }

        [TestMethod()]
        public void OnRollbackTest()
        {
            DeleteFileManagerTester.FileAlreadyExists.OnRollback();
            DeleteFileManagerTester.FileAlreadyExists.OnRollbackNoScope();
            DeleteFileManagerTester.FileIsNew.OnRollback();
            DeleteFileManagerTester.FileIsNew.OnRollbackNoScope();
        }

        [TestMethod()]
        public void EnlistVolatileTest()
        {
            DeleteFileManagerTester.CurrentTransactionIsNull();
        }
    }
}