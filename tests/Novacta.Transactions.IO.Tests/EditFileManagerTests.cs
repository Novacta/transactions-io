// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    public class EditFileManagerTests
    {
        [TestMethod()]
        public void EditFileManagerTest()
        {
            EditFileManagerTester.Constructor.ManagedPathIsNull();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\edit-file-already-exists-dispose.txt", @"Data")]
        public void DisposeTest()
        {
            EditFileManagerTester.Dispose();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\edit-file-already-exists-on-prepare-file-is-not-lockable.txt", @"Data")]
        public void OnPrepareTest()
        {
            EditFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\edit-file-already-exists-on-commit.txt", @"Data")]
        public void OnCommitTest()
        {
            EditFileManagerTester.FileAlreadyExists.OnCommit();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\edit-file-already-exists-on-rollback.txt", @"Data")]
        [DeploymentItem(@"Data\edit-file-already-exists-on-rollback-no-scope.txt", @"Data")]
        public void OnRollbackTest()
        {
            EditFileManagerTester.FileAlreadyExists.OnRollback();
            EditFileManagerTester.FileAlreadyExists.OnRollbackNoScope();
            EditFileManagerTester.FileIsNew.OnRollback();
            EditFileManagerTester.FileIsNew.OnRollbackNoScope();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\edit-file-already-exists-no-current-transaction.txt", @"Data")]
        public void EnlistVolatileTest()
        {
            EditFileManagerTester.CurrentTransactionIsNull();
        }
    }
}