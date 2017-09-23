using System.IO;
using System.Text;

namespace Novacta.Transactions.IO.Tests.Tools
{
    public class CreateWithContentFileManager : CreateFileManager
    {
        public CreateWithContentFileManager(string path, bool overwrite) : base(path, overwrite)
        {
        }

        private string writtenOnCreation = "written-on-creation";

        public string WrittenOnCreation
        {
            get { return this.writtenOnCreation; }            
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
                writer.Write(this.WrittenOnCreation);
            }
        }
    }
}
