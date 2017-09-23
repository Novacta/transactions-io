using System;
using System.IO;

namespace Novacta.Transactions.IO
{
    /// <summary>
    /// Represents a resource manager which creates a new file 
    /// when a transaction is successfully committed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="CreateFileManager"/> instance can be created 
    /// with or without the ability of overwriting the managed file. 
    /// The decision is based on the value of the 
    /// <c>bool</c> parameter in the constructor. 
    /// Such value is returned by  
    /// property <see cref="CanOverwrite"/>. 
    /// When an instance is notified
    /// that a transaction is being prepared
    /// for commitment, it checks if a file having the specified 
    /// path already exists. In such case, 
    /// if <see cref="CanOverwrite"/> returns <c>false</c>,
    /// the operation cannot be executed and the transaction is 
    /// forced to roll back.
    /// </para>
    /// <para><b>Notes to Inheritors</b></para>
    /// <para>
    /// By default, a <see cref="CreateFileManager"/> instance 
    /// creates an empty file. A class derived 
    /// from <see cref="CreateFileManager"/> must override 
    /// method <see cref="FileManager.OnCommit"/> to modify 
    /// such behavior. 
    /// </para>
    /// </remarks>
    /// <seealso cref="Novacta.Transactions.IO.FileManager" />
    public class CreateFileManager : FileManager
    {
        private bool canOverwrite;

        /// <summary>
        /// Gets a value indicating whether this 
        /// <see cref="CreateFileManager"/> is 
        /// authorized to overwrite the managed file.
        /// </summary>
        /// <value>
        /// <c>true</c> if the managed file can be overwritten; 
        /// otherwise, <c>false</c>.
        /// </value>
        public bool CanOverwrite
        {
            get { return this.canOverwrite; }
        }

        private bool fileAlreadyExists;

        /// <summary>
        /// Gets a value indicating whether the managed file 
        /// already exists at the time at the time of 
        /// transaction preparation.
        /// </summary>
        /// <value>
        /// <c>true</c> if the managed already exists 
        /// at the time of transaction preparation; 
        /// otherwise, <c>false</c>.
        /// </value>
        public bool FileAlreadyExists
        {
            get { return this.fileAlreadyExists; }
        }

        /// <summary>
        /// The initial content of the managed file.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is not <b>null</b> if and only if both <see cref="FileAlreadyExists"/> 
        /// and <see cref="CanOverwrite"/> evaluate to <c>true</c>.
        /// </para>
        /// </remarks>
        private byte[] initialContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFileManager"/> class.
        /// </summary>
        /// <param name="managedPath">The path of the managed file.</param>
        /// <param name="overwrite">
        /// <c>true</c> if the managed file can be overwritten; otherwise, <c>false</c>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="managedPath"/> is <b>null</b>.
        /// </exception>
        public CreateFileManager(
            string managedPath,
            bool overwrite) : base(managedPath)
        {
            this.canOverwrite = overwrite;
        }

        /// <inheritdoc/>
        public override FileStream OnPrepareFileStream(string managedPath)
        {
            this.fileAlreadyExists = File.Exists(managedPath);
            if (this.fileAlreadyExists && this.canOverwrite)
            {
                // If the managed file already exists and the  
                // the manager can overwrite, then the file is truncated 
                // when its stream is instantiated. 
                // Saving now its initial content enables the 
                // manager to restore the file in case of a 
                // rolled back transaction.
                this.initialContent = File.ReadAllBytes(managedPath);
            }

            FileMode mode = this.canOverwrite ? FileMode.Create : FileMode.CreateNew;

            return new FileStream(managedPath, mode, FileAccess.ReadWrite, FileShare.Delete);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// <para>
        /// If the manager voted for rolling back the transaction, 
        /// this method does nothing.  Otherwise, the method operates 
        /// as follows.
        /// </para>
        /// <para>
        /// If the manager can overwrite the file, i.e. 
        /// <see cref="CanOverwrite"/> evaluates to <c>true</c>, and 
        /// the file already exists when the manager 
        /// prepares for the transaction 
        /// (<see cref="FileAlreadyExists"/> returns <c>true</c>),  
        /// then the file is truncated 
        /// when the stream for it is instantiated. In such case, 
        /// this method will restore the initial content of the managed 
        /// file. 
        /// </para>
        /// <para>
        /// If the manager cannot overwrite the file and the file did not 
        /// exist when the manager 
        /// prepares for the transaction,  
        /// then the file is newly created 
        /// when the stream for it is instantiated. In such case, 
        /// this method will mark the managed 
        /// file for deletion. 
        /// </para>
        /// </remarks>
        public override void OnRollback()
        {
            if (this.Stream != null)
            {
#if DEBUG
            Console.WriteLine(
                "{0}.OnRollback(...) - Stream: {1}", this.GetType(), this.Stream);
#endif
                if (this.fileAlreadyExists)
                {
                    if (this.canOverwrite)
                    {
                        using (BinaryWriter writer = new BinaryWriter(this.Stream))
                        {
                            writer.Write(
                                this.initialContent,
                                0,
                                this.initialContent.Length);
                        }
                    }
                }
                else
                {
                    File.Delete(this.Stream.Name);
                    this.Stream.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public override void OnCommit()
        {
            this.Stream.Dispose();
        }
    }
}