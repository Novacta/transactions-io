﻿// Copyright (c) Giovanni Lafratta. All rights reserved.
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
    /// <see cref="CopyFileManager"/>.
    /// </summary>
    static class CopyFileManagerTester
    {
        /// <summary>
        /// Provides methods to test the constructor 
        /// of class <see cref="CopyFileManager"/>.
        /// </summary>
        public static class Constructor
        {
            /// <summary>
            /// Tests the constructor when parameter <i>sourcePath</i> is <b>null</b>.
            /// </summary>
            public static void SourcePathIsNull()
            {
#if DEBUG
                Console.WriteLine("Constructor_SourcePathIsNull");
#endif
                string sourcePath = null;

                string managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy -file-source-path-is-null.txt";

                ArgumentExceptionAssert.IsThrown(
                    () =>
                    {
                        var manager =
                    new CopyFileManager(sourcePath, managedPath, overwrite: true);
                    },
                    expectedType: typeof(ArgumentNullException),
                    expectedPartialMessage: ArgumentExceptionAssert.NullPartialMessage,
                    expectedParameterName: "sourcePath");
            }

            /// <summary>
            /// Tests the constructor when parameter <i>managedPath</i> is <b>null</b>.
            /// </summary>
            public static void ManagedPathIsNull()
            {
#if DEBUG
                Console.WriteLine("Constructor_DestinationFileNameIsNull");
#endif
                string sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-managed-path-is-null.txt";

                string managedPath = null;

                ArgumentExceptionAssert.IsThrown(
                    () =>
                    {
                        var manager =
                    new CopyFileManager(sourcePath, managedPath, overwrite: true);
                    },
                    expectedType: typeof(ArgumentNullException),
                    expectedPartialMessage: ArgumentExceptionAssert.NullPartialMessage,
                    expectedParameterName: "managedPath");
            }
        }

        /// <summary>
        /// Provides methods to test use cases of class 
        /// <see cref="CopyFileManager"/> when the managed file 
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-already-exists-on-prepare-cannot-overwrite.txt";

                List<FileManager> managers = new List<FileManager>
                {
                    new CopyFileManager(
                        sourcePath,
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-already-exists-on-prepare-file-is-not-lockable.txt";

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
                    new CopyFileManager(
                        sourcePath,
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
                var sourcePath =
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath =
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-already-exists-on-commit.txt";

                var manager = new CopyFileManager(sourcePath,
                    managedPath, overwrite: true);

                List<FileManager> managers = new List<FileManager>
                {
                    manager
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    var sourceBytes = File.ReadAllBytes(sourcePath);
                    var destBytes = File.ReadAllBytes(managedPath);

                    Assert.AreEqual(sourceBytes.Length, destBytes.Length);

                    for (int i = 0; i < sourceBytes.Length; i++)
                    {
                        Assert.AreEqual(sourceBytes[i], destBytes[i]);
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-already-exists-on-rollback.txt";
                
                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(
                        "Data" +
                        Path.DirectorySeparatorChar +
                        "copy-file-already-exists-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>
                {
                    forcingRollbackManager,

                    new CopyFileManager(
                        sourcePath,
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-already-exists-on-rollback-no-scope.txt";

                // Simulate a preparation

                bool overwrite = true;
                var manager = new CopyFileManager(sourcePath,
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
            var sourcePath = 
                "Data" +
                Path.DirectorySeparatorChar +
                "copy-file-source.txt";

            var managedPath = 
                "Data" +
                Path.DirectorySeparatorChar +
                "copy-file-is-new-no-current-transaction.txt";

            var manager = new CopyFileManager(sourcePath,
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

        public static void Dispose(bool overwrite)
        {
#if DEBUG
            Console.WriteLine("Dispose");
#endif
            var sourcePath = 
                "Data" +
                Path.DirectorySeparatorChar +
                "copy-file-source.txt";

            string managedPath =
                overwrite
                ?
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-is-new-dispose.txt"
                : 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-is-new-dispose-cannot-overwrite.txt";

            // Assure that the managed file is new 
            // in case of overwrite: false
            if (!overwrite)
            {
                File.Delete(managedPath);
            }

            // Simulate a preparation

            var manager = new CopyFileManager(sourcePath,
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

            // Delete the managed file in case of overwrite: false
            if (!overwrite)
            {
                File.Delete(managedPath);
            }
        }

        /// <summary>
        /// Provides methods to test use cases of class 
        /// <see cref="CopyFileManager"/> when the managed file 
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-is-new-on-commit.txt";

                var manager = new CopyFileManager(sourcePath,
                    managedPath, overwrite: overwrite);

                List<FileManager> managers = new List<FileManager>
                {
                    manager
                };

                void results()
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    var sourceBytes = File.ReadAllBytes(sourcePath);
                    var destBytes = File.ReadAllBytes(managedPath);

                    Assert.AreEqual(sourceBytes.Length, destBytes.Length);

                    for (int i = 0; i < sourceBytes.Length; i++)
                    {
                        Assert.AreEqual(sourceBytes[i], destBytes[i]);
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-is-new-on-rollback.txt";
                
                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(
                        "Data" +
                        Path.DirectorySeparatorChar +
                        "copy-file-is-new-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>
                {
                    forcingRollbackManager,

                    new CopyFileManager(
                        sourcePath,
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
                var sourcePath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-source.txt";

                var managedPath = 
                    "Data" +
                    Path.DirectorySeparatorChar +
                    "copy-file-is-new-on-rollback-no-scope.txt";

                // Simulate a preparation

                var manager = new CopyFileManager(sourcePath,
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
