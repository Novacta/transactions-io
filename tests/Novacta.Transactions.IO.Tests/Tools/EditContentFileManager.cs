using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacta.Transactions.IO.Tests.Tools
{
    public class EditContentFileManager : EditFileManager
    {

        private string newContent = "edited-existing-file";

        public EditContentFileManager(string managedPath) : base(managedPath)
        {
        }

        public string NewContent
        {
            get { return this.newContent; }
        }

        public override void OnCommit()
        {
            using (BinaryWriter writer = new BinaryWriter(this.Stream, Encoding.UTF8))
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
