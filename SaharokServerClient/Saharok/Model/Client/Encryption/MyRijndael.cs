using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Saharok.Model.Client.Encryption
{
    public class MyRijndael
    {
        public byte[] Key { get; set; } = null;
        public byte[] IV { get; set; } = null;

        public byte[] EncryptBytesToBytes(byte[] plainBytes)
        {
            if (plainBytes == null || plainBytes.Length <= 0)
                throw new ArgumentNullException("plainBytes");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(Key, IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        public byte[] DecryptBytesFromBytes(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length <= 0)
                throw new ArgumentNullException("cipherBytes");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(Key, IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(cipherBytes, 0, cipherBytes.Length);
                        csDecrypt.FlushFinalBlock();
                        return msDecrypt.ToArray();
                    }
                }
            }
        }

        public byte[] EncryptStringToBytes(string plainText, Encoding encoding)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(Key, IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, encoding))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return msEncrypt.ToArray();
                    }
                }
            }
        }

        public string DecryptStringFromBytes(byte[] cipherText, Encoding encoding)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(Key, IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt, encoding))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
