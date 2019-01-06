// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    [DeploymentItem("Data", "Data")]
    public class CopyFileManagerTests
    {
        [TestMethod()]
        public void CopyFileManagerTest()
        {
            CopyFileManagerTester.Constructor.SourcePathIsNull();
            CopyFileManagerTester.Constructor.ManagedPathIsNull();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            CopyFileManagerTester.Dispose(overwrite: true);
            CopyFileManagerTester.Dispose(overwrite: false);
        }

        [TestMethod()]
        public void OnPrepareTest()
        {
            CopyFileManagerTester.FileAlreadyExists.OnPrepareCannotOverwrite();
            CopyFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        public void OnCommitTest()
        {
            CopyFileManagerTester.FileAlreadyExists.OnCommit();
            CopyFileManagerTester.FileIsNew.OnCommit(overwrite: true);
            CopyFileManagerTester.FileIsNew.OnCommit(overwrite: false);
        }

        [TestMethod()]
        public void OnRollbackTest()
        {
            CopyFileManagerTester.FileAlreadyExists.OnRollback();
            CopyFileManagerTester.FileAlreadyExists.OnRollbackNoScope();
            CopyFileManagerTester.FileIsNew.OnRollback(overwrite: true);
            CopyFileManagerTester.FileIsNew.OnRollback(overwrite: false);
            CopyFileManagerTester.FileIsNew.OnRollbackNoScope(overwrite: true);
            CopyFileManagerTester.FileIsNew.OnRollbackNoScope(overwrite: false);
        }

        [TestMethod()]
        public void EnlistVolatileTest()
        {
            CopyFileManagerTester.CurrentTransactionIsNull();
        }
    }
}