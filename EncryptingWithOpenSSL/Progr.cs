using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EncryptingWithOpenSSL
{
    class Progr
    {
        static void Main(string[] args)
        {
            const string top_secret = "This is a top secret.";
            //var top_sec_len = top_secret.Length;
            //string cipher_txt = Encrypt(top_secret, "shrubbery");

            const string cipher_txt_raw = "8d20e5056a8d24d0462ce74e4904c1b513e10d1df4a2ef2ad4540fae1ca0aaf9";
            string cipher_txt = cipher_txt_raw.ToUpper();

            // Move up the directory to get to the word dictionary file 
            var wordsFile = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;
            wordsFile += @"\words.txt";

            // Get English dictionary words less than 16 characters long
            var words = File.ReadAllLines(wordsFile);
            var word_list = new List<string>(words).FindAll(a => a.Length < 16);

            for (int i = 0; i < word_list.Count; i++)
            {
                string byte_string = Encrypt(top_secret ,word_list[i]);
                if (byte_string == cipher_txt)
                {
                    Console.WriteLine("The key is: {0}", word_list[i]);
                }
            }
        }

        // Encrypts the plain text message with a provided key.
        static string Encrypt(string input_text, string key)
        {
            byte[] encrypted;
            string byte_string = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.BlockSize = 128;
                aesAlg.Key = FormKey_128(key);
                aesAlg.IV = FormIV_128();

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(input_text);
                    }
                    encrypted = msEncrypt.ToArray();
                }

                byte_string = BytesToStr(encrypted);
            }
            return byte_string;
        }

        // Form 128-bit key from provided string, pad with zero
        static byte[] FormKey_128(string word)
        {
            const int zero_pad = 16;
            byte[] key = Encoding.ASCII.GetBytes(word);

            int len = (key.Length + zero_pad - 1) / zero_pad * zero_pad;
            Array.Resize(ref key, len);

            for (int i = word.Length; i < len; i++)
            {
                key[i] = 0x20;
            }

            return key;
        }

        // Form 128-bit initiation vector filled with 0x0000
        static byte[] FormIV_128()
        {
            const int len = 16;
            byte[] iv = new byte[len];

            for (int i = 0; i < len; i++)
            {
                iv[i] = 0x0000;
            }
            return iv;
        }

        static string BytesToStr(byte[] bytes_arr)
        {
            return BitConverter.ToString(bytes_arr).Replace("-", "");
        }

    }

}
