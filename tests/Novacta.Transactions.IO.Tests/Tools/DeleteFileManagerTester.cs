﻿// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Transactions;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Provides methods to test use cases of class 
    /// <see cref="DeleteFileManager"/>.
    /// </summary>
    static class DeleteFileManagerTester
    {
        /// <summary>
        /// Provides methods to test the constructor 
        /// of class <see cref="DeleteFileManager"/>.
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
                    () => { var manager = new DeleteFileManager(managedPath); },
                    expectedType: typeof(ArgumentNullException),
                    expectedPartialMessage: ArgumentExceptionAssert.NullPartialMessage,
                    expectedParameterName: "managedPath");
            }
        }

        /// <summary>
        /// Provides methods to test use cases of class 
        /// <see cref="DeleteFileManager"/> when the managed file 
        /// already exists at the time the transaction is prepared 
        /// for commitment.
        /// </summary>
        public static class FileAlreadyExists
        {
            public static void OnPrepareFileIsNotLockable()
            {
#if DEBUG
                Console.WriteLine("FileAlreadyExists_FileIsNotLockable");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "delete-file-already-exists-on-prepare-file-is-not-lockable.txt";

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
                    new DeleteFileManager(
                        managedPath)
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
                    "delete-file-already-exists-on-commit.txt";

                var deleteManager = new DeleteFileManager(
                    managedPath);

                List<FileManager> managers = new List<FileManager>
                {
                    deleteManager
                };

                void results()
                {
                    // Dispose the manager, so that
                    // the following call to File.Exists
                    // has the required permissions 
                    // to investigate the managed file.
                    deleteManager.Dispose();

                    Assert.IsTrue(!File.Exists(managedPath));
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
                    "delete-file-already-exists-on-rollback.txt";

                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(
                        "Data" +
                        Path.DirectorySeparatorChar +
                        "delete-file-already-exists-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>
                {
                    forcingRollbackManager,

                    new DeleteFileManager(
                        managedPath)
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
                    "delete-file-already-exists-on-rollback-no-scope.txt";

                // Simulate a preparation

                var manager = new DeleteFileManager(
                                managedPath);

                FileStream stream = (FileStream)FileManagerReflection.Invoke(
                    manager,
                    "OnPrepareFileStream",
                    new string[] { managedPath });

                FileManagerReflection.SetStream(manager, stream);
                stream = null;

                // Simulate a rollback (NOP)

                // We want to access the managed file to inspect its 
                // edited contents, hence we need to dispose 
                // its manager.
                manager.Dispose();

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
                "delete-file-already-exists-no-current-transaction.txt";

            var manager = new DeleteFileManager(
                managedPath);

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

        public static void Dispose()
        {
#if DEBUG
            Console.WriteLine("Dispose");
#endif
            var managedPath = 
                "Data" +
                Path.DirectorySeparatorChar +
                "delete-file-already-exists-dispose.txt";

            // Simulate a preparation

            var manager = new DeleteFileManager(
                            managedPath);

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
        /// <see cref="DeleteFileManager"/> when the managed file 
        /// does not exists at the time the transaction is prepared 
        /// for commitment.
        /// </summary>
        public static class FileIsNew
        {
            public static void OnRollback()
            {
#if DEBUG
                Console.WriteLine("FileIsNew_OnRollback");
#endif
                var managedPath =
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "delete-file-is-new-on-rollback.txt";

                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(
                        "Data" +
                        Path.DirectorySeparatorChar +
                        "delete-file-is-new-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>
                {
                    forcingRollbackManager,

                    new DeleteFileManager(
                        managedPath)
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

            public static void OnRollbackNoScope()
            {
#if DEBUG
                Console.WriteLine("FileIsNew_OnRollbackNoScope");
#endif
                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "delete-file-is-new-on-rollback-no-scope.txt";

                // Simulate a preparation

                var manager = new DeleteFileManager(
                                managedPath);

                ExceptionAssert.IsThrown(
                    () => {
                        FileStream stream = (FileStream)FileManagerReflection.Invoke(
                            manager,
                            "OnPrepareFileStream",
                            new string[] { managedPath });
                    },
                    expectedType: typeof(TargetInvocationException),
                    expectedMessage: "Exception has been thrown by the target of an invocation.",
                    expectedInnerType: typeof(FileNotFoundException),
                    expectedInnerMessage:  "Could not find file '" + 
                        Path.Combine(Environment.CurrentDirectory, managedPath) +
                        "'.");
            }
        }
    }
}
