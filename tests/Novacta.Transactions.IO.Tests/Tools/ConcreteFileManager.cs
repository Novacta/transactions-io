// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System;
using System.IO;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Represents a concrete file manager.
    /// </summary>
    /// <seealso cref="Novacta.Transactions.IO.FileManager" />
    class ConcreteFileManager : FileManager
    {
        /// <summary>
        /// Gets or sets the state of this instance.
        /// </summary>
        /// <value>The state.</value>
        public Object State { get; set; }

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

        /// <summary>
        /// Gets or sets the <see cref="OnInDoubt"/> body.
        /// </summary>
        /// <value>The <see cref="OnInDoubt"/> body.</value>
        public Action OnInDoubtBody { get; set; }

        public ConcreteFileManager(string path) : base(path)
        {
            this.OnPrepareBody = (p) => { return null; };
            this.OnCommitBody = () => { };
            this.OnRollbackBody = () => { };
            this.OnInDoubtBody = () => { };
        }

        /// <inherithdoc/>
        protected override void OnCommit()
        {
            this.OnCommitBody();
        }

        /// <inherithdoc/>
        protected override FileStream OnPrepareFileStream(string path)
        {
            return this.OnPrepareBody(path);
        }

        /// <inherithdoc/>
        protected override void OnRollback()
        {
            this.OnRollbackBody();
        }

        /// <inherithdoc/>
        protected override void OnInDoubt()
        {
            this.OnInDoubtBody();
            base.OnInDoubt();
        }
    }
}
