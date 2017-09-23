using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
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
        [DeploymentItem(@"Data\copy-file-source.txt", @"Data")]
        [DeploymentItem(@"Data\copy-file-already-exists-on-prepare-cannot-overwrite.txt", @"Data")]
        [DeploymentItem(@"Data\copy-file-already-exists-on-prepare-file-is-not-lockable.txt", @"Data")]
        public void OnPrepareTest()
        {
            CopyFileManagerTester.FileAlreadyExists.OnPrepareCannotOverwrite();
            CopyFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        [DeploymentItem(@"Data\copy-file-source.txt", @"Data")]
        [DeploymentItem(@"Data\copy-file-already-exists-on-commit.txt", @"Data")]
        public void OnCommitTest()
        {
            CopyFileManagerTester.FileAlreadyExists.OnCommit();
            CopyFileManagerTester.FileIsNew.OnCommit(overwrite: true);
            CopyFileManagerTester.FileIsNew.OnCommit(overwrite: false);
        }

        [TestMethod()]
        [DeploymentItem(@"Data\copy-file-source.txt", @"Data")]
        [DeploymentItem(@"Data\copy-file-already-exists-on-rollback.txt", @"Data")]
        [DeploymentItem(@"Data\copy-file-already-exists-on-rollback-no-scope.txt", @"Data")]
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
        public void InDoubtTest()
        {
            CopyFileManagerTester.InDoubt();
        }

        [TestMethod()]
        public void EnlistVolatileTest()
        {
            CopyFileManagerTester.CurrentTransactionIsNull();
        }
    }
}