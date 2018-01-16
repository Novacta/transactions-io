// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System.IO;

namespace Novacta.Transactions.IO
{
    /// <summary>
    /// Represents a resource manager which edits an existing file 
    /// when a transaction is successfully committed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When an <see cref="EditFileManager"/> instance is notified
    /// that a transaction is being prepared
    /// for commitment, it checks if a file having the specified 
    /// path already exists. If not, 
    /// the operation cannot be executed and the transaction is 
    /// forced to roll back.
    /// </para>
    /// <para><b>Notes to Inheritors</b></para>
    /// <para>
    /// A class derived from <see cref="EditFileManager"/> must implement 
    /// method <see cref="FileManager.OnCommit"/> to state how the 
    /// managed file should be edited.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// In the following example, a file manager edits a 
    /// document in XML format in case 
    /// of a successfully committed transaction.
    /// </para>
    /// <para>
    /// <code source="..\..\docs\Novacta.Transactions.IO.CodeExamples\EditFileManagerExample0.cs.txt" 
    /// language="cs" />
    /// </para>
    /// </example>
    /// <seealso cref="Novacta.Transactions.IO.FileManager" />
    public abstract class EditFileManager : FileManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditFileManager"/> class.
        /// </summary>
        /// <param name="managedPath">The path of the managed file.</param>
        public EditFileManager(string managedPath)
            : base(managedPath)
        {
        }

        /// <inheritdoc/>
        protected override FileStream OnPrepareFileStream(string managedPath)
        {
            return new FileStream(managedPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }

        /// <inheritdoc/>
        protected override void OnRollback()
        {
        }
    }
}
