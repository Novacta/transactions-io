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
                string managedPath = @"Data\copy-file-source-path-is-null.txt";

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
                string sourcePath = @"Data\copy-file-managed-path-is-null.txt";
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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-already-exists-on-prepare-cannot-overwrite.txt";

                List<FileManager> managers = new List<FileManager>();
                managers.Add(new CopyFileManager(sourcePath,
                     managedPath, overwrite: false));

                Action results = () =>
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
                };

                Action<Exception> rolledBack = (e) =>
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        typeof(TransactionAbortedException),
                        "The transaction has aborted.");
                };

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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-already-exists-on-prepare-file-is-not-lockable.txt";

                // Create a stream so that the manager cannot 
                // lock the existing file and hence will force
                // a rollback.
                FileStream existingStream = new FileStream(
                    managedPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None);

                List<FileManager> managers = new List<FileManager>();
                managers.Add(new CopyFileManager(sourcePath,
                     managedPath, overwrite: true));

                Action results = () =>
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
                };

                Action<Exception> rolledBack = (e) =>
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        expectedType: typeof(TransactionAbortedException),
                        expectedMessage: "The transaction has aborted.",
                        expectedInnerType: typeof(IOException),
                        expectedInnerMessage: "The process cannot access the file '" +
                                              existingStream.Name +
                                              "' because it is being used by another process.");
                };

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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-already-exists-on-commit.txt";

                var copyManager = new CopyFileManager(sourcePath,
                    managedPath, overwrite: true);

                List<FileManager> managers = new List<FileManager>();
                managers.Add(copyManager);

                Action results = () =>
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    var sourceBytes = File.ReadAllBytes(sourcePath);
                    var destBytes = File.ReadAllBytes(managedPath);

                    Assert.AreEqual(sourceBytes.Length, destBytes.Length);

                    for (int i = 0; i < sourceBytes.Length; i++)
                    {
                        Assert.AreEqual(sourceBytes[i], destBytes[i]);
                    }
                };

                Action<Exception> rolledBack = (e) =>
                {
                };

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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-already-exists-on-rollback.txt";
                
                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(@"Data\copy-file-already-exists-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>();

                managers.Add(forcingRollbackManager);

                managers.Add(new CopyFileManager(sourcePath,
                                managedPath, overwrite: true));

                Action results = () =>
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
                };

                Action<Exception> rolledBack = (e) =>
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        expectedType: typeof(TransactionAbortedException),
                        expectedMessage: "The transaction has aborted.",
                        expectedInnerType: onPrepareException.GetType(),
                        expectedInnerMessage: onPrepareException.Message);
                };

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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-already-exists-on-rollback-no-scope.txt";

                // Simulate a preparation

                bool overwrite = true;
                var manager = new CopyFileManager(sourcePath,
                                managedPath, overwrite: overwrite);

                var stream = manager.OnPrepareFileStream(managedPath);

                FileManagerReflection.SetStream(manager, stream);
                stream = null;

                Assert.AreEqual(overwrite, manager.CanOverwrite);
                Assert.AreEqual(true, manager.FileAlreadyExists);

                // Simulate a rollback

                manager.OnRollback();

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
            var sourcePath = @"Data\copy-file-source.txt";
            var managedPath = @"Data\copy-file-is-new-no-current-transaction.txt";

            var copyManager = new CopyFileManager(sourcePath,
                managedPath, overwrite: true);

            FileStream fileStream = copyManager.OnPrepareFileStream(managedPath);

            ExceptionAssert.IsThrown(
                () => { copyManager.EnlistVolatile(EnlistmentOptions.None); },
                expectedType: typeof(InvalidOperationException),
                expectedMessage: String.Format(
                        "Cannot enlist resource {0}: no ambient transaction detected.",
                        managedPath));
        }

        internal static void InDoubt()
        {
#if DEBUG
            Console.WriteLine("InDoubt");
#endif
            var sourcePath = @"Data\copy-file-source.txt";
            var managedPath = @"Data\copy-file-is-new-in-doubt.txt";

            var copyManager = new CopyFileManager(sourcePath,
                managedPath, overwrite: true);

            ExceptionAssert.IsThrown(
                () => { copyManager.InDoubt(null); },
                expectedType: typeof(NotImplementedException),
                expectedMessage: "The method or operation is not implemented.");
        }

        public static void Dispose(bool overwrite)
        {
#if DEBUG
            Console.WriteLine("Dispose");
#endif
            var sourcePath = @"Data\copy-file-source.txt";
            var managedPath = @"Data\copy-file-is-new-dispose.txt";

            // Simulate a preparation

            var manager = new CopyFileManager(sourcePath,
                            managedPath, overwrite: overwrite);

            Assert.AreEqual(false, FileManagerReflection.GetField(manager, "disposedValue"));

            var stream = manager.OnPrepareFileStream(managedPath);

            FileManagerReflection.SetStream(manager, stream);
            stream = null;

            manager.Dispose();

            // Simulate rollback results

            File.Delete(managedPath);

            // Expected results

            Assert.AreEqual(true, FileManagerReflection.GetField(manager, "disposedValue"));
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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-is-new-on-commit.txt";

                var copyManager = new CopyFileManager(sourcePath,
                    managedPath, overwrite: overwrite);

                List<FileManager> managers = new List<FileManager>();
                managers.Add(copyManager);

                Action results = () =>
                {
                    Assert.IsTrue(File.Exists(managedPath));
                    var sourceBytes = File.ReadAllBytes(sourcePath);
                    var destBytes = File.ReadAllBytes(managedPath);

                    Assert.AreEqual(sourceBytes.Length, destBytes.Length);

                    for (int i = 0; i < sourceBytes.Length; i++)
                    {
                        Assert.AreEqual(sourceBytes[i], destBytes[i]);
                    }
                };

                Action<Exception> rolledBack = (e) =>
                {
                };

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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-is-new-on-rollback.txt";
                
                // Add a manager voting to force a rollback.

                ConcreteFileManager forcingRollbackManager =
                    new ConcreteFileManager(@"Data\copy-file-is-new-on-rollback-0.txt");

                var onPrepareException = new Exception("Voting for a rollback.");

                forcingRollbackManager.OnPrepareBody = (stream) =>
                {
                    throw onPrepareException;
                };

                List<FileManager> managers = new List<FileManager>();

                managers.Add(forcingRollbackManager);

                managers.Add(new CopyFileManager(sourcePath,
                                managedPath, overwrite: overwrite));

                Action results = () =>
                {
                    Assert.IsTrue(!File.Exists(managedPath));
                };

                Action<Exception> rolledBack = (e) =>
                {
                    ExceptionAssert.IsThrown(
                        () => { throw e; },
                        expectedType: typeof(TransactionAbortedException),
                        expectedMessage: "The transaction has aborted.",
                        expectedInnerType: onPrepareException.GetType(),
                        expectedInnerMessage: onPrepareException.Message);
                };

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
                var sourcePath = @"Data\copy-file-source.txt";
                var managedPath = @"Data\copy-file-is-new-on-rollback-no-scope.txt";

                // Simulate a preparation

                var manager = new CopyFileManager(sourcePath,
                                managedPath, overwrite: overwrite);

                var stream = manager.OnPrepareFileStream(managedPath);

                FileManagerReflection.SetStream(manager, stream);
                stream = null;

                Assert.AreEqual(overwrite, manager.CanOverwrite);
                Assert.AreEqual(false, manager.FileAlreadyExists);

                // Simulate a rollback

                manager.OnRollback();

                // Expected results

                Assert.IsTrue(!File.Exists(managedPath));
            }
        }
    }
}
