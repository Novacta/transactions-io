﻿// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System.IO;

namespace Novacta.Transactions.IO
{
    /// <summary>
    /// Represents a resource manager which deletes an existing file 
    /// when a transaction is successfully committed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When a <see cref="DeleteFileManager"/> instance is notified
    /// that a transaction is being prepared
    /// for commitment, it checks if a file having the specified 
    /// path exists. If not, the  
    /// operation cannot be executed and the transaction is 
    /// forced to roll back.
    /// </para>
    /// </remarks>
    /// <seealso cref="Novacta.Transactions.IO.FileManager" />
    public sealed class DeleteFileManager : FileManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteFileManager"/> class.
        /// </summary>
        /// <param name="managedPath">The path of the managed file.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="managedPath"/> is <b>null</b>.
        /// </exception>
        public DeleteFileManager(string managedPath)
            : base(managedPath)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>
        /// This method marks the managed file for deletion.
        /// </para>
        /// </remarks>
        protected override void OnCommit()
        {
            File.Delete(this.ManagedFileStream.Name);
        }

        /// <inheritdoc/>
        protected override FileStream OnPrepareFileStream(string managedPath)
        {
            return new FileStream(managedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
        }

        /// <inheritdoc/>
        protected override void OnRollback()
        {
        }
    }
}
