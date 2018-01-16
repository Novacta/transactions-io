// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Novacta.Transactions.IO.Tests.Tools;
using System;
using System.IO;
using System.Transactions;

namespace Novacta.Transactions.IO.Tests
{
    [TestClass()]
    public class FileManagerTests
    {
        [TestMethod()]
        public void StreamGetTest()
        {
            var managedPath = @"Data\create-file-get-stream-on-disposed.txt";

            var manager = new CreateNonEmptyFileManager(
                managedPath,
                overwrite: true);

            // Simulate a preparation

            Assert.AreEqual(false, FileManagerReflection.GetField(manager, "disposed"));

            FileStream stream = (FileStream)FileManagerReflection.Invoke(
                manager,
                "OnPrepareFileStream",
                new string[] { managedPath });

            FileManagerReflection.SetStream(manager, stream);

            // Dispose the manager
            manager.Dispose();

            ExceptionAssert.IsThrown(
                () => { var managerStream = manager.ManagedFileStream; },
                expectedType: typeof(ObjectDisposedException),
                expectedMessage: "Cannot access a disposed object.\r\n" +
                "Object name: " +
                "\'CreateNonEmptyFileManager\'."
                );
        }

        [TestMethod()]
        [DeploymentItem(@"Data\in-doubt.txt", @"Data")]
        public void OnInDoubtTest()
        {
            var managedPath = @"Data\in-doubt.txt";

            var manager = new ConcreteFileManager(managedPath);
            string state = "state";
            Action onInDoubtBody = () => { manager.State = state; };
            manager.OnInDoubtBody = onInDoubtBody;

            using (TransactionScope scope = new TransactionScope())
            {
                var enlistment = Transaction.Current.EnlistVolatile(
                    manager,
                    EnlistmentOptions.None);

                // Simulate an InDoubt notification
                manager.InDoubt(enlistment);

                scope.Complete();
            }

            Assert.AreEqual(state, manager.State);
        }
    }
}
