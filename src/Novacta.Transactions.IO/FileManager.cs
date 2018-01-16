// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System;
using System.IO;
using System.Transactions;

namespace Novacta.Transactions.IO
{
    /// <summary>
    /// Represents a file manager which can be enlisted in 
    /// a transaction. This class is abstract.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Given that <see cref="FileManager"/> is an abstract class,  
    /// you do not instantiate it in your code. Define a class 
    /// which derives from <see cref="FileManager"/> 
    /// to add new files or modify existing ones inside a given 
    /// transaction. You can also use one of its specialized subclasses, 
    /// such as 
    /// <see cref="CreateFileManager"/>, 
    /// <see cref="EditFileManager"/>, 
    /// <see cref="DeleteFileManager"/>, and <see cref="CopyFileManager"/>, 
    /// to create, update, delete, and copy files, respectively. 
    /// </para>
    /// <para>
    /// In order for a file manager to participate in a transaction, it 
    /// must be explicitly enlisted in the current transaction   
    /// using the <see cref="EnlistVolatile(EnlistmentOptions)"/> method. 
    /// In this way, the manager can be notified at the different 
    /// phases of the transaction as follows.
    /// </para>
    /// <para>
    /// When the transaction is being prepared for commitment,
    /// the manager is notified by calling its method 
    /// <see cref="IEnlistmentNotification.Prepare(PreparingEnlistment)"/>, 
    /// which will create a <see cref="System.IO.FileStream"/> for the 
    /// managed file by calling 
    /// <see cref="OnPrepareFileStream(string)"/>. 
    /// The returned stream can be accessed through 
    /// property <see cref="FileManager.ManagedFileStream"/>.
    /// </para>
    /// <para>
    /// When the transaction is being rolled back (aborted),  
    /// an enlisted file manager is notified by calling its method 
    /// <see cref="Rollback(Enlistment)"/>, which
    /// will in turn call method <see cref="OnRollback"/> to do 
    /// the work necessary to finish the aborted transaction.
    /// </para>
    /// <para>
    /// When the transaction is being committed, 
    /// an enlisted object is notified by calling its method 
    /// <see cref="Commit(Enlistment)"/>. A  
    /// <see cref="FileManager"/> instance will in turn call 
    /// method <see cref="OnCommit"/> to do the work 
    /// necessary to finish the committed transaction.
    /// </para>
    /// <para><b>Notes to Inheritors</b></para>
    /// <para>
    /// A class derived from <see cref="FileManager"/> must implement a 
    /// constructor passing information to  
    /// <see cref="FileManager.FileManager(string)"/> about the path 
    /// of the managed file. In addition, 
    /// the following abstract methods need to be implemented.
    /// </para>
    /// <para>
    ///   <list type="table">
    ///     <listheader>
    ///        <term>Method</term>
    ///        <term>Description</term>
    ///     </listheader>
    ///     <item>
    ///        <term><see cref="OnPrepareFileStream(string)"/></term>
    ///        <term>
    ///        Called by <see cref="Prepare(PreparingEnlistment)"/> when 
    ///        a transaction is being prepared for commitment. 
    ///        It is executed inside a 
    ///        <b>try/catch</b> block: throw an exception if conditions hold 
    ///        under which the transaction need to be rolled back. Otherwise, 
    ///        return a stream for the managed file.</term>
    ///     </item>
    ///     <item>
    ///        <term><see cref="OnRollback"/></term>
    ///        <term>
    ///        Called by <see cref="Rollback(Enlistment)"/> when 
    ///        a transaction is being rolled back. 
    ///        Must be implemented to state how the 
    ///        manager should react to a rolled back transaction.
    ///        </term>
    ///     </item>
    ///     <item>
    ///        <term><see cref="OnCommit"/></term>
    ///        <term>
    ///        Called by <see cref="Commit(Enlistment)"/> when 
    ///        a transaction is being committed. 
    ///        Must be implemented to state how the 
    ///        manager should operate in case of a 
    ///        successfully committed transaction.
    ///        </term>
    ///     </item>    
    ///   </list>
    /// </para>
    /// <para>
    /// An enlisted <see cref="FileManager"/> instance can also eventually 
    /// be notified that the status of a transaction is in doubt. 
    /// In such case, the virtual method <see cref="OnInDoubt"/> is called 
    /// by <see cref="InDoubt(Enlistment)"/>. 
    /// By default, <see cref="OnInDoubt"/> does nothing. You 
    /// should override it to perform whatever recovery or containment 
    /// operation it understands on the affected file.
    /// </para> 
    /// </remarks>
    /// <seealso cref="System.Transactions.IEnlistmentNotification" />
    /// <seealso cref="System.Transactions"/>
    public abstract class FileManager : IEnlistmentNotification, IDisposable
    {
        #region State

        private FileStream stream;

        /// <summary>
        /// Provides the stream for the managed file.
        /// </summary>
        /// <value>The stream for the managed file.</value>
        /// <remarks>This property evaluates to <b>null</b> if
        /// the manager voted for rolling back the transaction.</remarks>
        /// <exception cref="ObjectDisposedException">
        /// An attempt to get the property value on a file manager 
        /// that has been disposed.
        /// </exception>
        public FileStream ManagedFileStream
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(this.GetType().Name);
                }
                return this.stream;
            }
            private set
            {
                this.stream = value;
            }
        }

        /// <summary>
        /// Represents the path of the managed file.
        /// </summary>
        private string managedPath;

        #endregion

        #region Deferred implementation

        /// <summary>
        /// Prepares the stream for managing the file.
        /// </summary>
        /// <param name="managedPath">The path of the managed file.</param>
        /// <returns>The stream for the managed file.</returns>
        /// <remarks>
        /// <para>
        /// This method is called when the <see cref="FileManager" /> is
        /// notified that a transaction is being prepared
        /// for commitment.
        /// </para>
        /// <para><b>Notes to Inheritors</b></para>
        /// <para>
        /// You should throw an exception from this method in case the
        /// manager cannot prepare for the transaction to complete.
        /// The call to <see cref="OnPrepareFileStream" /> is
        /// wrapped within the
        /// <see cref="IEnlistmentNotification.Prepare(PreparingEnlistment)" /> 
        /// method with a <b>try/catch</b> block:
        /// the catch block will pass any exception on via
        /// the <see cref="PreparingEnlistment.ForceRollback(Exception)" /> 
        /// method.
        /// </para>
        /// </remarks>
        protected abstract FileStream OnPrepareFileStream(string managedPath);

        /// <summary>
        /// Called when the transaction is successfully committed.
        /// </summary>
        protected abstract void OnCommit();

        /// <summary>
        /// Called when the transaction is rolled back.
        /// </summary>
        protected abstract void OnRollback();

        /// <summary>
        /// Called when the transaction is in doubt.
        /// </summary>
        protected virtual void OnInDoubt() { }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FileManager" /> class.
        /// </summary>
        /// <param name="managedPath">The path of the managed file.</param>
        /// <exception cref="ArgumentNullException">
        /// Parameter <paramref name="managedPath" /> is <b>null</b>.
        /// </exception>
        public FileManager(
            string managedPath)
        {
            this.managedPath = managedPath ?? throw new ArgumentNullException(nameof(managedPath));
        }

        #endregion

        #region Enlistment helper

        /// <summary>
        /// Enlists this instance as a volatile resource 
        /// manager using the specified enlistment 
        /// options.
        /// </summary>
        /// <param name="enlistmentOptions">The enlistment options.</param>
        /// <remarks>
        /// <para>
        /// The instance is enlisted as a volatile resource 
        /// to receive two phase commit notifications.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// No ambient transaction detected. The instance cannot enlist 
        /// a <b>null</b> transaction.
        /// </exception>
        /// <seealso cref="System.Transactions.Transaction.Current"/>
        public void EnlistVolatile(EnlistmentOptions enlistmentOptions)
        {
            if (Transaction.Current == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Cannot enlist resource {0}: no ambient transaction detected.",
                        this.managedPath));
            }
            Transaction.Current.EnlistVolatile(this, enlistmentOptions);
        }

        #endregion

        #region IEnlistmentNotification

        /// <summary>
        /// Notifies an enlisted object that the status of a transaction is 
        /// in doubt.
        /// </summary>
        /// <param name="enlistment">An <see cref="T:System.Transactions.Enlistment" /> object 
        /// used to send a response to the transaction manager.</param>
        public void InDoubt(Enlistment enlistment)
        {
#if DEBUG
            Console.WriteLine(
                "{0}.InDoubt(...) - {1}",
                this.GetType(),
                this.managedPath);
#endif

            this.OnInDoubt();

            enlistment.Done();
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being prepared 
        /// for commitment.
        /// </summary>
        /// <param name="preparingEnlistment">
        /// A <see cref="T:System.Transactions.PreparingEnlistment" /> object 
        /// used to send a response to the transaction manager.
        /// </param>
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
#if DEBUG
                Console.WriteLine(
                    "{0}.Prepare(...) (try) - {1}",
                    this.GetType(),
                    this.managedPath);
#endif

                this.ManagedFileStream = this.OnPrepareFileStream(this.managedPath);

                preparingEnlistment.Prepared();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(
                    "{0}.Prepare(...) (catch) - {1}",
                    this.GetType(),
                    this.managedPath);
#endif
                // Rollback() is not called on a resource manager 
                // if its Prepare() method votes for rollback.
                if (!this.disposed)
                {
                    this.OnRollback();
                }

                preparingEnlistment.ForceRollback(e);
            }
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being committed.
        /// </summary>
        /// <param name="enlistment">
        /// An <see cref="T:System.Transactions.Enlistment" /> object used 
        /// to send a response to the transaction manager.
        /// </param>
        public void Commit(Enlistment enlistment)
        {
#if DEBUG
            Console.WriteLine(
                "{0}.Commit(...) - {1}",
                this.GetType(),
                this.managedPath);
#endif
            this.OnCommit();

            enlistment.Done();
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being rolled 
        /// back (aborted).
        /// </summary>
        /// <param name="enlistment">
        /// An <see cref="T:System.Transactions.Enlistment" /> object used 
        /// to send a response to the transaction manager.
        /// </param>
        public void Rollback(Enlistment enlistment)
        {
#if DEBUG
            Console.WriteLine(
                "{0}.Rollback(...) - {1}",
                this.GetType(),
                this.managedPath);
#endif
            this.OnRollback();

            enlistment.Done();
        }

        #endregion

        #region IDisposable

        private bool disposed = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            if (disposing)
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                }
            }

            this.disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}