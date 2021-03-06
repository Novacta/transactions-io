﻿// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    [DeploymentItem("Data", "Data")]
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
        public void OnPrepareTest()
        {
            CreateFileManagerTester.FileAlreadyExists.OnPrepareCannotOverwrite();
            CreateFileManagerTester.FileAlreadyExists.OnPrepareFileIsNotLockable();
        }

        [TestMethod()]
        public void OnCommitTest()
        {
            CreateFileManagerTester.DefaultOnCommit();
            CreateFileManagerTester.FileAlreadyExists.OnCommit();
            CreateFileManagerTester.FileIsNew.OnCommit(overwrite: true);
            CreateFileManagerTester.FileIsNew.OnCommit(overwrite: false);
        }

        [TestMethod()]
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
        public void EnlistVolatileTest()
        {
            CreateFileManagerTester.CurrentTransactionIsNull();
        }
    }
}