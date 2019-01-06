// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Provides methods to test use cases of class 
    /// <see cref="CreateFileManager"/>.
    /// </summary>
    static class CreateFileManagerTester
    {
        /// <summary>
        /// Provides methods to test the constructor 
        /// of class <see cref="CreateFileManager"/>.
        /// </summary>
        public static class Constructor
        {
            /// <summary>
            /// Tests the constructor when parameter <i>managedPath</i> is <b>null</b>.
            /// </summary>
            public static void ManagedPathIsNull()
            {
#if DEBUG
                Console.WriteLine("Constructor_ManagedPathIsNull");
#endif
                string managedPath = null;
                ArgumentExceptionAssert.IsThrown(
                    () => { var manager = new CreateNonEmptyFileManager(managedPath, overwrite: true); },
                    expectedType: typeof(ArgumentNullException),
                    expectedPartialMessage: ArgumentExceptionAssert.NullPartialMessage,
                    expectedParameterName: "managedPath");
            }
        }

        /// <summary>
        /// Provides methods to test use cases of class 
        /// <see cref="CreateFileManager"/> when the managed file 
        /// already exists at the time the transaction is prepared 
        /// for commitment.
        /// </summary>
        public static class FileAlreadyExists
        {
            public static void OnPrepareCannotOverwrite()
            {
#if DEBUG
                Console.WriteLine("FileAlreadyExists_CannotOverwrite");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-already-exists-on-prepare-cannot-overwrite.txt";

                List<FileManager> managers = new List<FileManager>
                {
                    new CreateNonEmptyFileManager(
                        managedPath, overwrite: false)
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    using (Stream stream = File.OpenRead(managedPath))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            var content = reader.ReadLine();
                            Assert.AreEqual("existing-file", content);
                        }
                    }
                }

                void rolledBack(Exception e)
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        typeof(TransactionAbortedException),
                        "The transaction has aborted.");
                }

                TransactionScopeHelper.Using(
                    managers,
                    results,
                    rolledBack);
            }

            public static void OnPrepareFileIsNotLockable()
            {
#if DEBUG
                Console.WriteLine("FileAlreadyExists_FileIsNotLockable");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-already-exists-on-prepare-file-is-not-lockable.txt";

                // Create a stream so that the manager cannot 
                // lock the existing file and hence will force
                // a rollback.
                FileStream existingStream = new FileStream(
                    managedPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);

                List<FileManager> managers = new List<FileManager>
                {
                    new CreateNonEmptyFileManager(
                        managedPath, overwrite: true)
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));

                    // Dispose the existing stream in order to 
                    // enable file access.
                    existingStream.Dispose();
                    using (Stream stream = File.OpenRead(managedPath))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            var content = reader.ReadLine();
                            Assert.AreEqual("existing-file", content);
                        }
                    }
                }

                void rolledBack(Exception e)
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        expectedType: typeof(TransactionAbortedException),
                        expectedMessage: "The transaction has aborted.",
                        expectedInnerType: typeof(IOException),
                        expectedInnerMessage: "The process cannot access the file '" +
                                              existingStream.Name +
                                              "' because it is being used by another process.");
                }

                TransactionScopeHelper.Using(
                    managers,
                    results,
                    rolledBack);
            }

            public static void OnCommit()
            {
#if DEBUG
                Console.WriteLine("FileAlreadyExists_OnCommit");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-already-exists-on-commit.txt";

                var createManager = new CreateNonEmptyFileManager(
                    managedPath, overwrite: true);

                List<FileManager> managers = new List<FileManager>
                {
                    createManager
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    using (Stream stream = File.OpenRead(managedPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            var content = reader.ReadString();
                            Assert.AreEqual(createManager.ContentWrittenOnCreation, content);
                        }
                    }
                }

                void rolledBack(Exception e)
                {
                }

                TransactionScopeHelper.Using(
                    managers,
                    results,
                    rolledBack);
            }

            public static void OnRollback()
            {
#if DEBUG
                Console.WriteLine("FileAlreadyExists_OnRollback");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-already-exists-on-rollback.txt";

                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(
                        "Data" +
                        Path.DirectorySeparatorChar +
                        "create-file-already-exists-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>
                {
                    forcingRollbackManager,

                    new CreateNonEmptyFileManager(
                        managedPath, overwrite: true)
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    using (Stream stream = File.OpenRead(managedPath))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            var content = reader.ReadLine();
                            Assert.AreEqual("existing-file", content);
                        }
                    }
                }

                void rolledBack(Exception e)
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        expectedType: typeof(TransactionAbortedException),
                        expectedMessage: "The transaction has aborted.",
                        expectedInnerType: onPrepareException.GetType(),
                        expectedInnerMessage: onPrepareException.Message);
                }

                TransactionScopeHelper.Using(
                    managers,
                    results,
                    rolledBack);
            }

            public static void OnRollbackNoScope()
            {
#if DEBUG
                Console.WriteLine("FileAlreadyExists_OnRollbackNoScope");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-already-exists-on-rollback-no-scope.txt";

                // Simulate a preparation

                bool overwrite = true;
                var manager = new CreateNonEmptyFileManager(
                                managedPath, overwrite: overwrite);

                FileStream stream = (FileStream)FileManagerReflection.Invoke(
                    manager,
                    "OnPrepareFileStream",
                    new string[] { managedPath });

                FileManagerReflection.SetStream(manager, stream);
                stream = null;

                Assert.AreEqual(overwrite, manager.CanOverwrite);
                Assert.AreEqual(true, manager.FileAlreadyExists);

                // Simulate a rollback

                FileManagerReflection.Invoke(
                    manager,
                    "OnRollback",
                    null);

                // Expected results

                Assert.IsTrue(File.Exists(managedPath));
                using (stream = File.OpenRead(managedPath))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var content = reader.ReadLine();
                        Assert.AreEqual("existing-file", content);
                    }
                }
            }
        }

        internal static void CurrentTransactionIsNull()
        {
#if DEBUG
            Console.WriteLine("CurrentTransactionIsNull");
#endif
            var managedPath = 
                "Data" +
                Path.DirectorySeparatorChar +
                "create-file-is-new-no-current-transaction.txt";

            var manager = new CreateNonEmptyFileManager(
                managedPath, overwrite: true);

            FileStream stream = (FileStream)FileManagerReflection.Invoke(
                manager,
                "OnPrepareFileStream",
                new string[] { managedPath });

            ExceptionAssert.IsThrown(
                () => { manager.EnlistVolatile(EnlistmentOptions.None); },
                expectedType: typeof(InvalidOperationException),
                expectedMessage: String.Format(
                        "Cannot enlist resource {0}: no ambient transaction detected.",
                        managedPath));
        }

        public static void DefaultOnCommit()
        {
#if DEBUG
            Console.WriteLine("DefaultOnCommit");
#endif
            var managedPath = 
                "Data" +
                Path.DirectorySeparatorChar +
                "create-file-is-new-default-on-commit.txt";

            var createManager = new CreateFileManager(
                managedPath, overwrite: true);

            List<FileManager> managers = new List<FileManager>
            {
                createManager
            };

            void results()
            {
                // Dispose the manager, so that
                // the following call to File.Exists
                // has the required permissions 
                // to investigate the managed file.
                createManager.Dispose();

                Assert.IsTrue(File.Exists(managedPath));
                using (Stream stream = File.OpenRead(managedPath))
                {
                    Assert.AreEqual(0, stream.Length);
                }
            }

            void rolledBack(Exception e)
            {
            }

            TransactionScopeHelper.Using(
                managers,
                results,
                rolledBack);
        }

        public static void Dispose(bool overwrite)
        {
#if DEBUG
            Console.WriteLine("Dispose");
#endif
            string managedPath = 
                overwrite 
                ?
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-is-new-dispose.txt"
                :   
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-is-new-dispose-cannot-overwrite.txt";

            // Assure that the managed file is new 
            // in case of overwrite: false
            if (!overwrite)
            {
                File.Delete(managedPath);
            }

            // Simulate a preparation

            var manager = new CreateNonEmptyFileManager(
                            managedPath, overwrite: overwrite);

            Assert.AreEqual(false, FileManagerReflection.GetField(manager, "disposed"));

            FileStream stream = (FileStream)FileManagerReflection.Invoke(
                manager,
                "OnPrepareFileStream",
                new string[] { managedPath });

            FileManagerReflection.SetStream(manager, stream);
            stream = null;

            // Dispose the manager

            manager.Dispose();

            // Expected results

            Assert.AreEqual(true, FileManagerReflection.GetField(manager, "disposed"));

            // Dispose the manager, again

            ExceptionAssert.IsNotThrown(
                () => { manager.Dispose(); }
                );
        }

        /// <summary>
        /// Provides methods to test use cases of class 
        /// <see cref="CreateFileManager"/> when the managed file 
        /// does not exists at the time the transaction is prepared 
        /// for commitment.
        /// </summary>
        public static class FileIsNew
        {
            public static void OnCommit(bool overwrite)
            {
#if DEBUG
                Console.WriteLine("FileIsNew_OnCommit");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-is-new-on-commit.txt";

                var createManager = new CreateNonEmptyFileManager(
                    managedPath, overwrite: overwrite);

                List<FileManager> managers = new List<FileManager>
                {
                    createManager
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    using (Stream stream = File.OpenRead(managedPath))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            var content = reader.ReadString();
                            Assert.AreEqual(createManager.ContentWrittenOnCreation, content);
                        }
                    }
                }

                void rolledBack(Exception e)
                {
                }

                TransactionScopeHelper.Using(
                    managers,
                    results,
                    rolledBack);
            }

            public static void OnRollback(bool overwrite)
            {
#if DEBUG
                Console.WriteLine("FileIsNew_OnRollback");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-is-new-on-rollback.txt";

                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(
                        "Data" +
                        Path.DirectorySeparatorChar +
                        "create-file-is-new-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>
                {
                    forcingRollbackManager,

                    new CreateNonEmptyFileManager(
                        managedPath, overwrite: overwrite)
                };

                void results()
                {
                    Assert.IsTrue(!File.Exists(managedPath));
                }

                void rolledBack(Exception e)
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        expectedType: typeof(TransactionAbortedException),
                        expectedMessage: "The transaction has aborted.",
                        expectedInnerType: onPrepareException.GetType(),
                        expectedInnerMessage: onPrepareException.Message);
                }

                TransactionScopeHelper.Using(
                    managers,
                    results,
                    rolledBack);
            }

            public static void OnRollbackNoScope(bool overwrite)
            {
#if DEBUG
                Console.WriteLine("FileIsNew_OnRollbackNoScope");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "create-file-is-new-on-rollback-no-scope.txt";

                // Simulate a preparation

                var manager = new CreateNonEmptyFileManager(
                                managedPath, overwrite: overwrite);

                FileStream stream = (FileStream)FileManagerReflection.Invoke(
                    manager,
                    "OnPrepareFileStream",
                    new string[] { managedPath });

                FileManagerReflection.SetStream(manager, stream);
                stream = null;

                Assert.AreEqual(overwrite, manager.CanOverwrite);
                Assert.AreEqual(false, manager.FileAlreadyExists);

                // Simulate a rollback

                FileManagerReflection.Invoke(
                    manager,
                    "OnRollback",
                    null);

                // Expected results

                // Dispose the manager, so that
                // the following call to File.Exists
                // has the required permissions 
                // to investigate the managed file.
                manager.Dispose();

                Assert.IsTrue(!File.Exists(managedPath));
            }
        }
    }
}
