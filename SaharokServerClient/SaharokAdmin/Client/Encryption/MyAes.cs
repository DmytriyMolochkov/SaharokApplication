using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaharokAdmin.Client.Encryption
{
    public class MyAes
    {
        private AesManaged AES { get; set; }
        private ICryptoTransform Encryptor { get; set; }
        private ICryptoTransform Decryptor { get; set; }

        BinaryFormatter Formatter = new BinaryFormatter();

        public MyAes()
        {
            AES = new AesManaged();
            //Formatter.Binder = new Type1ToType2DeserializationBinder();
        }

        public void GenerateParameters()
        {
            AES.GenerateKey();
            AES.GenerateIV();
            Encryptor = AES.CreateEncryptor();
            Decryptor = AES.CreateDecryptor();
        }

        public void ExportParameters(Stream stream)
        {
            if (Encryptor == null)
                throw new Exception("Необходимо сгенерировать параметры шифрования");
            if (Decryptor == null)
                throw new Exception("Необходимо сгенерировать параметры шифрования");
            if (stream == null)
                throw new ArgumentNullException("stream");


            MyRijndael myRijndael = new MyRijndael();
            myRijndael.Key = Convert.FromBase64String(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "lL4VnrEhkZuAM", "bo72T", "Cc3", "s3Zk", "B", "vmL2f", "rEm8Z", "drHmF1M", "="));
            myRijndael.IV = Convert.FromBase64String(String.Format("{0}{1}{2}{3}{4}{5}{6}", "S", "7u9tm", "Mree", "Pt", "mwfy", "Hsrb", "MQ==")); /*"S7u9tmMreePtmwfyHsrbMQ=="*/

            MyRSA RSA = new MyRSA();

            using (AesManaged aesManaged = new AesManaged())
            {
                using (MemoryStream msDecrypt = new MemoryStream((byte[])Formatter.Deserialize(stream)))
                {
                    ICryptoTransform decryptor =
                    aesManaged.CreateDecryptor(myRijndael.DecryptBytesFromBytes(Convert.FromBase64String("r7QKucgmbCYjJAfUxViJPGUyhocmBxIFank7BYMBd3FfV4b2kGBhbMeGxoGLMkSc")),
                                               myRijndael.DecryptBytesFromBytes(Convert.FromBase64String("tTEu96R/dV5UyrMVdSSIRqr/RzlJnHA/6NOcZpw50vk=")));


                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        RSA.ImportParameters(csDecrypt);
                    }
                }
            }
            RSA.EncryptToStream(new[] { AES.Key, AES.IV }, stream);
        }

        public void ImportParameters(EchoStream eStream, NetworkStream nStream)
        {
            if (eStream == null)
                throw new ArgumentNullException("eStream");
            if (nStream == null)
                throw new ArgumentNullException("nStream");

            MyRijndael myRijndael = new MyRijndael();
            myRijndael.Key = Convert.FromBase64String(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", "lL4VnrEhkZuAM", "bo72T", "Cc3", "s3Zk", "B", "vmL2f", "rEm8Z", "drHmF1M", "="));
            myRijndael.IV = Convert.FromBase64String(String.Format("{0}{1}{2}{3}{4}{5}{6}", "S", "7u9tm", "Mree", "Pt", "mwfy", "Hsrb", "MQ=="));

            MyRSA RSA = new MyRSA();

            using (AesManaged aesManaged = new AesManaged())
            {
                ICryptoTransform encryptor =
                    aesManaged.CreateEncryptor(myRijndael.DecryptBytesFromBytes(Convert.FromBase64String("r7QKucgmbCYjJAfUxViJPGUyhocmBxIFank7BYMBd3FfV4b2kGBhbMeGxoGLMkSc")),
                                               myRijndael.DecryptBytesFromBytes(Convert.FromBase64String("tTEu96R/dV5UyrMVdSSIRqr/RzlJnHA/6NOcZpw50vk=")));

                using (MemoryStream ms = new MemoryStream())
                {
                    RSA.ExportParameters(ms);
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(ms.ToArray(), 0, ms.ToArray().Length);
                            csEncrypt.FlushFinalBlock();
                            Formatter.Serialize(nStream, msEncrypt.ToArray());
                        }
                    }
                }
            }

            byte[][] parameters = (byte[][])RSA.DencryptFromStream(eStream);
            AES.Key = parameters[0];
            AES.IV = parameters[1];
            Encryptor = AES.CreateEncryptor();
            Decryptor = AES.CreateDecryptor();
        }

        public void EncryptToStream(object data, Stream stream)
        {
            if (Encryptor == null)
                throw new Exception("Необходимо сгенерировать параметры шифрования");
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (data == null)
                throw new ArgumentNullException("data");

            using (AES)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Formatter.Serialize(ms, data);
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, Encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(ms.ToArray(), 0, ms.ToArray().Length);
                            csEncrypt.FlushFinalBlock();
                            Formatter.Serialize(stream, msEncrypt.ToArray());
                        }
                    }
                }
            }
        }

        public object DecryptFromStream(Stream stream)
        {
            if (Decryptor == null)
                throw new Exception("Необходимо сгенерировать параметры шифрования");
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (AES)
            {
                using (MemoryStream msDecrypt = new MemoryStream((byte[])Formatter.Deserialize(stream)))
                {
                    if (msDecrypt.Length == 0)
                        throw new Exception("Пришёл пустой поток");
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, Decryptor, CryptoStreamMode.Read))
                    {
                        return Formatter.Deserialize(csDecrypt);
                    }
                }
            }
        }

        public byte[] EncryptBytesToBytes(byte[] plainBytes)
        {
            if (Encryptor == null)
                throw new Exception("Необходимо сгенерировать параметры шифрования");
            if (plainBytes == null || plainBytes.Length <= 0)
                throw new ArgumentNullException("plainBytes");

            using (AES)
            {
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, Encryptor, CryptoStreamMode.Write))
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
            if (Decryptor == null)
                throw new Exception("Необходимо сгенерировать параметры шифрования");
            if (cipherBytes == null || cipherBytes.Length <= 0)
                throw new ArgumentNullException("cipherBytes");

            using (AES)
            {
                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, Decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(cipherBytes, 0, cipherBytes.Length);
                        csDecrypt.FlushFinalBlock();
                        return msDecrypt.ToArray();
                    }
                }
            }
        }
    }
}
