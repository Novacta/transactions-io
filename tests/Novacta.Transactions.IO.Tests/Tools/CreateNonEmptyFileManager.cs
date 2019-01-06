// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System.IO;
using System.Text;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Represents a file manager that creates a non empty file.
    /// </summary>
    /// <seealso cref="Novacta.Transactions.IO.CreateFileManager" />
    public class CreateNonEmptyFileManager : CreateFileManager
    {
        public CreateNonEmptyFileManager(string path, bool overwrite) : base(path, overwrite)
        {
        }

        private readonly string writtenOnCreation = "written-on-creation";

        /// <summary>
        /// Gets the content written on creation.
        /// </summary>
        /// <value>The content written on creation.</value>
        public string ContentWrittenOnCreation
        {
            get { return this.writtenOnCreation; }            
        }

        /// <inherithdoc/>
        protected override void OnCommit()
        {
            using (BinaryWriter writer = new BinaryWriter(this.ManagedFileStream, Encoding.UTF8))
            {
                // Each character in a System.String is defined by a Unicode scalar value, 
                // encoded by using UTF-16 encoding.
                // Method BinaryWriter.Write(string) writes a length-prefixed string 
                // to the underlying stream. This implies that 2 additional bytes are 
                // written before the actual string.
                writer.Write(this.ContentWrittenOnCreation);
            }
        }
    }
}
