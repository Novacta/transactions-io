// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System.IO;
using System.Text;

namespace Novacta.Transactions.IO.Tests.Tools
{
    public class EditExistingFileManager : EditFileManager
    {

        private readonly string newContent = "edited-existing-file";

        public EditExistingFileManager(string managedPath) : base(managedPath)
        {
        }

        public string NewContent
        {
            get { return this.newContent; }
        }

        protected override void OnCommit()
        {
            using (BinaryWriter writer = new BinaryWriter(this.ManagedFileStream, Encoding.UTF8))
            {
                // Each character in a System.String is defined by a Unicode scalar value, 
                // encoded by using UTF-16 encoding.
                // Method BinaryWriter.Write(string) writes a length-prefixed string 
                // to the underlying stream. This implies that 2 additional bytes are 
                // written before the actual string.
                writer.Write(this.NewContent);
            }
        }
    }
}
