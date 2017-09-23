using System;
using System.IO;

namespace Novacta.Transactions.IO
{
    /// <summary>
    /// Represents a resource manager which copies a file 
    /// when a transaction is successfully committed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="CopyFileManager"/> instance can be created 
    /// with or without the ability of overwriting the managed  
    /// destination file. 
    /// This ability is signaled be the value returned by its 
    /// property <see cref="CreateFileManager.CanOverwrite"/>. 
    /// When an instance is notified
    /// that a transaction is being prepared
    /// for commitment, it checks if a file having the specified 
    /// path already exists. In such case, 
    /// if <see cref="CreateFileManager.CanOverwrite"/> returns <c>false</c>,
    /// the operation cannot be executed and the transaction is 
    /// forced to roll back.
    /// </para>
    /// </remarks>
    /// <seealso cref="Novacta.Transactions.IO.FileManager" />
    public sealed class CopyFileManager : CreateFileManager
    {
        /// <summary>
        /// Represents the content of the source file.
        /// </summary>
        private byte[] sourceContent;

        /// <summary>
        /// Represents the path of the source file.
        /// </summary>
        private string sourcePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyFileManager"/> class.
        /// </summary>
        /// <param name="sourcePath">The path of the file to copy.</param>
        /// <param name="managedPath">
        /// The name of the destination file. This cannot be a directory.
        /// </param>
        /// <param name="overwrite">
        /// <c>true</c> if the managed (destination) file can be overwritten; 
        /// otherwise, <c>false</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Parameter <paramref name="sourcePath" /> is <b>null</b>.<br/>
        /// -or-<br/>
        /// Parameter <paramref name="managedPath" /> is <b>null</b>.
        /// </exception>
        public CopyFileManager(
            string sourcePath,
            string managedPath,
            bool overwrite) : base(managedPath, overwrite)
        {
            this.sourcePath = sourcePath ?? throw
                new ArgumentNullException(nameof(sourcePath));
        }

        /// <inheritdoc/>
        public override FileStream OnPrepareFileStream(string managedPath)
        {
            this.sourceContent = File.ReadAllBytes(this.sourcePath);

            return base.OnPrepareFileStream(managedPath);
        }

        /// <inheritdoc/>
        public override void OnCommit()
        {
            using (BinaryWriter writer = new BinaryWriter(this.Stream))
            {
                writer.Write(this.sourceContent);
            }
        }
    }
}
