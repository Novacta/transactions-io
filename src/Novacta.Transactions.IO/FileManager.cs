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
    /// <see cref="FileManager"/> represents a 
    /// file manager which can be enlisted in a transaction. 
    /// It is the abstract base class of 
    /// <see cref="CreateFileManager"/>, <see cref="EditFileManager"/> and  
    /// <see cref="DeleteFileManager"/>, 
    /// which manage how to create, update, or delete files, respectively. 
    /// </para>
    /// <para>
    /// Given that <see cref="FileManager"/> is an abstract class,  
    /// you do not instantiate it in your code. Define a class 
    /// which derives from <see cref="FileManager"/> 
    /// to add new files or modify existing ones inside a given 
    /// transaction. The constructor of class <see cref="FileManager"/> 
    /// needs a <see cref="String"/> and a <see cref="System.IO.FileMode"/> 
    /// representing, respectively, the path and the mode for accessing the managed file. 
    /// When method <see cref="IEnlistmentNotification.Prepare(PreparingEnlistment)"/> is 
    /// executed, a <see cref="System.IO.FileStream"/> for the managed file is created 
    /// using such information. The stream is always opened with read/write access 
    /// and no sharing allowed, and can be accessed through 
    /// property <see cref="FileManager.Stream"/>.
    /// </para>
    /// <para><b>Notes to Inheritors</b></para>
    /// <para>
    /// A class derived from <see cref="FileManager"/> must implement a constructor 
    /// that needs to pass information to  
    /// <see cref="FileManager.FileManager(string)"/> about the path and 
    /// the access mode for the file. In addition, the following 
    /// methods need to be implemented.
    /// </para>
    /// <para>
    /// When a transaction is being prepared for commitment, 
    /// method <see cref="OnPrepareFileStream"/> is executed inside a 
    /// <b>try/catch</b> block: throw an exception if conditions hold 
    /// under which the transaction need to be rolled back.
    /// Implement method <see cref="OnRollback"/> to state how the 
    /// manager should react to a rolled back transaction. If, 
    /// on the contrary, the transaction is successfully committed, 
    /// method <see cref="OnCommit"/> is executed.
    /// </para>
    /// <para>
    /// It is recommended to define your own classes deriving from one of the 
    /// specialized subclasses of <see cref="FileManager"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="System.Transactions.IEnlistmentNotification" />
    /// <seealso cref="System.Transactions"/>
    public abstract class FileManager : IEnlistmentNotification, IDisposable
    {
        #region State

        /// <summary>
        /// Provides the stream for the managed file.
        /// </summary>
        /// <value>The stream for the managed file.</value>
        /// <remarks>
        /// <para>
        /// This property evaluates to <b>null</b> if 
        /// the manager voted for rolling back transaction.
        /// </para>
        /// </remarks>
        public FileStream Stream { get; private set; }

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
        /// <para>
        /// You should throw an exception from this method in case the
        /// manager cannot prepare for the transaction to complete.
        /// The call to <see cref="OnPrepareFileStream" /> is
        /// wrapped within the
        /// <see cref="IEnlistmentNotification.Prepare(PreparingEnlistment)" /> method
        /// with a try/catch block:
        /// the catch block will pass it on via
        /// the <see cref="PreparingEnlistment.ForceRollback(Exception)" /> method.
        /// </para>
        /// </remarks>
        public abstract FileStream OnPrepareFileStream(string managedPath);

        /// <summary>
        /// Called when the transaction is successfully committed.
        /// </summary>
        public abstract void OnCommit();

        /// <summary>
        /// Called when the transaction is rolled back.
        /// </summary>
        public abstract void OnRollback();

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
        /// The instance is enlisted as a volatile resource.
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
        /// <exception cref="NotImplementedException"></exception>
        public void InDoubt(Enlistment enlistment)
        {
#if DEBUG
            Console.WriteLine(
                "{0}.InDoubt(...) - {1}", this.GetType(), this.managedPath);
#endif

            throw new NotImplementedException();
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being prepared 
        /// for commitment.
        /// </summary>
        /// <param name="preparingEnlistment">
        /// A <see cref="T:System.Transactions.PreparingEnlistment" /> object used 
        /// to send a response to the transaction manager.</param>
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
#if DEBUG
                Console.WriteLine(
                    "{0}.Prepare(...) (try) - {1}", this.GetType(), this.managedPath);
#endif
                this.Stream = this.OnPrepareFileStream(this.managedPath);

                preparingEnlistment.Prepared();
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(
                    "{0}.Prepare(...) (catch) - {1}", this.GetType(), this.managedPath);
#endif
                // Rollback() is not called on a resource manager if its Prepare() 
                // method votes to rollback.
                this.OnRollback();

                preparingEnlistment.ForceRollback(e);
            }
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being committed.
        /// </summary>
        /// <param name="enlistment">
        /// An <see cref="T:System.Transactions.Enlistment" /> object used to send 
        /// a response to the transaction manager.</param>
        public void Commit(Enlistment enlistment)
        {
#if DEBUG
            Console.WriteLine(
                "{0}.Commit(...) - {1}", this.GetType(), this.managedPath);
#endif

            this.OnCommit();

            enlistment.Done();
        }

        /// <summary>
        /// Notifies an enlisted object that a transaction is being rolled 
        /// back (aborted).
        /// </summary>
        /// <param name="enlistment">
        /// A <see cref="T:System.Transactions.Enlistment" /> object used to send 
        /// a response to the transaction manager.</param>
        /// <remarks>
        /// <para>
        /// 
        /// </para>
        /// </remarks>
        public void Rollback(Enlistment enlistment)
        {
#if DEBUG
            Console.WriteLine(
                "{0}.Rollback(...) - {1}", this.GetType(), this.managedPath);
#endif

            this.OnRollback();

            enlistment.Done();
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and 
        /// unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (this.Stream != null)
                    {
                        this.Stream.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and 
                // override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(disposing) above 
        // has code to free unmanaged resources.
        // ~FileManager() {
        //   // Do not change this code. Put cleanup code in Dispose(disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.


        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}