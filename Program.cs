using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AffinitiExtract.Encrypt;
using AffinitiExtract.Keys;
using System.IO;


namespace AffinitiExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            string pubKeyPath = @"C:\temp\pubring.gpg";
            string privKeyPath = @"C:\temp\secring.gpg";
            string password = "password";
            long _keyID = 3699527550217851901;
            PGPEncrpyt test = new PGPEncrpyt(pubKeyPath, privKeyPath, password, _keyID);
            FileInfo myFile = new FileInfo(@"C:\test\app.config");
            FileStream mystream = new FileStream(@"C:\test\somefile.zip", FileMode.Create, FileAccess.Write);
            test.EncryptSignAndZip(mystream, myFile);
        }
    }
}
