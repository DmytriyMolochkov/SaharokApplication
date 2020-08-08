using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Saharok.Model.Client.Encryption
{
    public class MyRSA
    {
        private RSACryptoServiceProvider Rsa;
        BinaryFormatter Formatter;

        public MyRSA()
        {
            Rsa = new RSACryptoServiceProvider(2048);
            Formatter = new BinaryFormatter();
            Formatter.Binder = new Type1ToType2DeserializationBinder();
        }

        public void ExportParameters(Stream stream)
        {
            Formatter.Serialize(stream, new[] { Rsa.ExportParameters(false).Modulus, Rsa.ExportParameters(false).Exponent });
        }

        public void ImportParameters(Stream stream)
        {
            var parameters = new RSAParameters();
            byte[][] param = (byte[][])Formatter.Deserialize(stream);
            parameters.Modulus = param[0];
            parameters.Exponent = param[1];
            Rsa.ImportParameters(parameters);
        }

        public void EncryptToStream(object data, Stream stream)
        {
            try
            {
                using (Rsa)
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        Formatter.Serialize(msEncrypt, data);
                        Formatter.Serialize(stream, Rsa.Encrypt(msEncrypt.ToArray(), false));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public object DencryptFromStream(Stream stream)
        {
            try
            {
                using (Rsa)
                {
                    using (MemoryStream msEncrypt = new MemoryStream((byte[])Formatter.Deserialize(stream)))
                    {
                        byte[] data = Rsa.Decrypt(msEncrypt.ToArray(), false);
                        using (MemoryStream MsEncrypt = new MemoryStream(data))
                        {
                            return Formatter.Deserialize(MsEncrypt);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
