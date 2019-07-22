using AffinitiExtract.Keys;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AffinitiExtract.Encrypt
{
    /// <summary>
    /// Public class to encrypt and compress files to a zip archive using PGP
    /// </summary>
    public class PGPEncrpyt
    {
        /// <summary>
        /// instansiate a PGPKeys object
        /// </summary>
        private PGPKeys _pgpKeys;
        private const int BufferSize = 0x10000;
        public PGPEncrpyt(string _pubKeyPath, string _privKeyPath, string _password, long _keyID)
        {
            _pgpKeys = new PGPKeys(_pubKeyPath, _privKeyPath, _password, _keyID);
        }
        private static void writeAndSign(Stream ouput, Stream literalout, FileStream inputFile, PgpSignatureGenerator sigGen)
        {
            int length = 0;
            byte[] buf = new byte[BufferSize];
            while ((length = inputFile.Read(buf, 0, buf.Length)) > 0)
            {
                literalout.Write(buf, 0, length);
                sigGen.Update(buf, 0, length);
            }
            sigGen.Generate().Encode(ouput);
        }
        private Stream encrypt(Stream output)
        {
            PgpEncryptedDataGenerator pgpEncDataGen = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Twofish, new SecureRandom());
            pgpEncDataGen.AddMethod(_pgpKeys.PGPPublicKey);
            Stream encryptedOutput = pgpEncDataGen.Open(output, new byte[BufferSize]);
            return encryptedOutput;
        }
        private Stream compress(Stream output)
        {
            PgpCompressedDataGenerator pgpCompDataGen = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
            Stream compressedEncryptedOut = pgpCompDataGen.Open(output);
            return compressedEncryptedOut;
        }
        private Stream literalOutput(Stream compressedOut, FileInfo file)
        {
            PgpLiteralDataGenerator pgpLiteralDataGen = new PgpLiteralDataGenerator();
            Stream literal = pgpLiteralDataGen.Open(compressedOut, PgpLiteralData.Binary, file);
            return literal;
        }
        private PgpSignatureGenerator sigGen(Stream compressedOut)
        {
            const bool Iscritical = false;
            const bool IsNested = false;
            PublicKeyAlgorithmTag tag = _pgpKeys.PGPSecretKey.PublicKey.Algorithm;
            PgpSignatureGenerator pgpSigGen = new PgpSignatureGenerator(tag, HashAlgorithmTag.Sha1);
            pgpSigGen.InitSign(PgpSignature.BinaryDocument, _pgpKeys.PGPPrivateKey);
            foreach (string userID in _pgpKeys.PGPSecretKey.PublicKey.GetUserIds())
            {
                PgpSignatureSubpacketGenerator subPackGen = new PgpSignatureSubpacketGenerator();
                subPackGen.SetSignerUserId(Iscritical, userID);
                pgpSigGen.SetHashedSubpackets(subPackGen.Generate());
                break;
            }
            pgpSigGen.GenerateOnePassVersion(IsNested).Encode(compressedOut);
            return pgpSigGen;
        }
        public void EncryptSignAndZip(Stream output, FileInfo unencryptedinput)
        {
            if (output == null)
            {
                throw new ArgumentNullException("The output stream cannot be null");
            }
            if (unencryptedinput == null)
            {
                throw new ArgumentNullException("You must supply a filename to encrypt");
            }
            if (!File.Exists(unencryptedinput.FullName))
            {
                throw new ArgumentNullException(unencryptedinput + " does not exist");
            }
            using (Stream encryptedout = encrypt(output))
            using (Stream compressedOut = compress(encryptedout))
            {
                PgpSignatureGenerator signature = sigGen(compressedOut);
                using (Stream literalOut = literalOutput(compressedOut, unencryptedinput))
                using (FileStream inputfile = unencryptedinput.OpenRead())
                {
                    writeAndSign(compressedOut, literalOut, inputfile, signature);
                }
            }
        }
    }
}