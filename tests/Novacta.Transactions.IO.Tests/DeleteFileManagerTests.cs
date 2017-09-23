using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    public class DeleteFileManagerTests
    {
        [TestMethod()]
        public void DeleteFileManagerTest()
        {
            DeleteFileManagerTester.Constructor.ManagedPathIsNull();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\delete-file-already-exists-dispose.txt", @"Data")]
        public void DisposeTest()
        {
            DeleteFileManagerTester.Dispose();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\delete-file-already-exists-on-prepare-file-is-not-lockable.txt", @"Data")]
        public void OnPrepareTest()
        {
            DeleteFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\delete-file-already-exists-on-commit.txt", @"Data")]
        public void OnCommitTest()
        {
            DeleteFileManagerTester.FileAlreadyExists.OnCommit();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\delete-file-already-exists-on-rollback.txt", @"Data")]
        [DeploymentItem(@"Data\delete-file-already-exists-on-rollback-no-scope.txt", @"Data")]
        public void OnRollbackTest()
        {
            DeleteFileManagerTester.FileAlreadyExists.OnRollback();
            DeleteFileManagerTester.FileAlreadyExists.OnRollbackNoScope();
            DeleteFileManagerTester.FileIsNew.OnRollback();
            DeleteFileManagerTester.FileIsNew.OnRollbackNoScope();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\delete-file-already-exists-in-doubt.txt", @"Data")]
        public void InDoubtTest()
        {
            DeleteFileManagerTester.InDoubt();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\delete-file-already-exists-no-current-transaction.txt", @"Data")]
        public void EnlistVolatileTest()
        {
            DeleteFileManagerTester.CurrentTransactionIsNull();
        }
    }
}