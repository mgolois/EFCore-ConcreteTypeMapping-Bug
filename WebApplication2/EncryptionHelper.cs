using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2
{
    public static class EncryptionHelper
    {
        private static string passphrase = "12345567890qwertyuiiopasdfghjklzxcvbnm";

        public static byte[] EncryptStringToBytes(string data)
        {
            //Plain Text to be encrypted
            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(data);

            //In live, get the persistant passphrase from other isolated source
            //This example has hardcoded passphrase just for demo purpose
            StringBuilder sb = new StringBuilder();
            sb.Append(passphrase);

            //Generate the Salt, with any custom logic and
            //using the above string
            StringBuilder _sbSalt = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                _sbSalt.Append("," + sb.Length.ToString());
            }
            byte[] Salt = Encoding.ASCII.GetBytes(_sbSalt.ToString());

            //Key generation:- default iterations is 1000 
            //and recomended is 10000
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(sb.ToString(), Salt, 10000);

            //The default key size for RijndaelManaged is 256 bits, 
            //while the default blocksize is 128 bits.
            RijndaelManaged _RijndaelManaged = new RijndaelManaged();
            _RijndaelManaged.BlockSize = 128; //Increased it to 256 bits- max and more secure

            byte[] key = pwdGen.GetBytes(_RijndaelManaged.KeySize / 8);   //This will generate a 256 bits key
            byte[] iv = pwdGen.GetBytes(_RijndaelManaged.BlockSize / 8);  //This will generate a 256 bits IV

            //On a given instance of Rfc2898DeriveBytes class,
            //GetBytes() will always return unique byte array.
            _RijndaelManaged.Key = key;
            _RijndaelManaged.IV = iv;

            //Now encrypt
            byte[] cipherText2 = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, _RijndaelManaged.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(PlainText, 0, PlainText.Length);
                }
                cipherText2 = ms.ToArray();
            }
            return cipherText2;

        }

        public static string DecryptStringFromBytes(byte[] cipherText2)
        {
            // Check arguments.
            //In live, get the persistant passphrase from other isolated source
            //This example has hardcoded passphrase just for demo purpose, obtained by 
            //adding current user's firstname + DOB + email
            //You may generate this string with any logic you want.
            StringBuilder sb = new StringBuilder();
            sb.Append(passphrase);

            //Generate the Salt, with any custom logic and
            //using the above string
            StringBuilder _sbSalt = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                _sbSalt.Append("," + sb.Length.ToString());
            }
            byte[] Salt = Encoding.ASCII.GetBytes(_sbSalt.ToString());

            //Key generation:- default iterations is 1000 and recomended is 10000
            Rfc2898DeriveBytes pwdGen = new Rfc2898DeriveBytes(sb.ToString(), Salt, 10000);

            //The default key size for RijndaelManaged is 256 bits,
            //while the default blocksize is 128 bits.
            RijndaelManaged _RijndaelManaged = new RijndaelManaged();
            _RijndaelManaged.BlockSize = 128; //Increase it to 256 bits- more secure

            byte[] key = pwdGen.GetBytes(_RijndaelManaged.KeySize / 8);   //This will generate a 256 bits key
            byte[] iv = pwdGen.GetBytes(_RijndaelManaged.BlockSize / 8);  //This will generate a 256 bits IV

            //On a given instance of Rfc2898DeriveBytes class,
            //GetBytes() will always return unique byte array.
            _RijndaelManaged.Key = key;
            _RijndaelManaged.IV = iv;

            //Now decrypt
            byte[] plainText2 = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, _RijndaelManaged.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherText2, 0, cipherText2.Length);
                }
                plainText2 = ms.ToArray();
            }
            //Decrypted text
            return Encoding.Unicode.GetString(plainText2);
        }
    }
}
