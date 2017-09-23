using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    public class CreateFileManagerTests
    {
        [TestMethod()]
        public void CreateFileManagerTest()
        {
            CreateFileManagerTester.Constructor.ManagedPathIsNull();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            CreateFileManagerTester.Dispose(overwrite: true);
            CreateFileManagerTester.Dispose(overwrite: false);
        }

        [TestMethod()]
        [DeploymentItem(@"Data\create-file-already-exists-on-prepare-cannot-overwrite.txt", @"Data")]
        [DeploymentItem(@"Data\create-file-already-exists-on-prepare-file-is-not-lockable.txt", @"Data")]
        public void OnPrepareTest()
        {
            CreateFileManagerTester.FileAlreadyExists.OnPrepareCannotOverwrite();
            CreateFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\create-file-already-exists-on-commit.txt", @"Data")]
        public void OnCommitTest()
        {
            CreateFileManagerTester.DefaultOnCommit();
            CreateFileManagerTester.FileAlreadyExists.OnCommit();
            CreateFileManagerTester.FileIsNew.OnCommit(overwrite: true);
            CreateFileManagerTester.FileIsNew.OnCommit(overwrite: false);
        }

        [TestMethod()]
        [DeploymentItem(@"Data\create-file-already-exists-on-rollback.txt", @"Data")]
        [DeploymentItem(@"Data\create-file-already-exists-on-rollback-no-scope.txt", @"Data")]
        public void OnRollbackTest()
        {
            CreateFileManagerTester.FileAlreadyExists.OnRollback();
            CreateFileManagerTester.FileAlreadyExists.OnRollbackNoScope();
            CreateFileManagerTester.FileIsNew.OnRollback(overwrite: true);
            CreateFileManagerTester.FileIsNew.OnRollback(overwrite: false);
            CreateFileManagerTester.FileIsNew.OnRollbackNoScope(overwrite: true);
            CreateFileManagerTester.FileIsNew.OnRollbackNoScope(overwrite: false);
        }

        [TestMethod()]
        public void InDoubtTest()
        {
            CreateFileManagerTester.InDoubt();
        }

        [TestMethod()]
        public void EnlistVolatileTest()
        {
            CreateFileManagerTester.CurrentTransactionIsNull();
        }
    }
}