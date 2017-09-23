using System;
using System.IO;

namespace Novacta.Transactions.IO.Tests.Tools
{
    class ConcreteFileManager : FileManager
    {
        /// <summary>
        /// Gets or sets the <see cref="OnCommit"/> body.
        /// </summary>
        /// <value>The <see cref="OnCommit"/> body.</value>
        public Action OnCommitBody { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OnPrepare"/> body.
        /// </summary>
        /// <value>The <see cref="OnPrepare"/> body.</value>
        public Func<string, FileStream> OnPrepareBody { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OnRollback"/> body.
        /// </summary>
        /// <value>The <see cref="OnRollback"/> body.</value>
        public Action OnRollbackBody { get; set; }

        public ConcreteFileManager(string path) : base(path)
        {
            this.OnPrepareBody = (p) => { return null; };
            this.OnCommitBody = () => { };
            this.OnRollbackBody = () => { };
        }


        public override void OnCommit()
        {
            this.OnCommitBody();
        }

        public override FileStream OnPrepareFileStream(string path)
        {
            return this.OnPrepareBody(path);
        }

        public override void OnRollback()
        {
            this.OnRollbackBody();
        }
    }
}
