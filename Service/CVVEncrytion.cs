using System.Security.Cryptography;
using System.Text;

namespace City_Taxi.Service
{
    public class CVVEncryption
    {
        private readonly string EncryptionKey;

        public CVVEncryption(IConfiguration configuration)
        {
            EncryptionKey = configuration["EncryptionKey"];
        }

        // Method to encrypt CVV
        public string EncryptCVV(string cvv)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(cvv);

            using (Aes aes = Aes.Create())
            {
                byte[] key = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x43, 0x87, 0x23, 0x72 }).GetBytes(32);
                aes.Key = key;
                aes.IV = new byte[16]; // AES needs 16-byte IV, set to all zeroes here (you could use a random IV for extra security)

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // Method to decrypt CVV
        public string DecryptCVV(string encryptedCvv)
        {
            byte[] cipherBytes = Convert.FromBase64String(encryptedCvv);

            using (Aes aes = Aes.Create())
            {
                byte[] key = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x43, 0x87, 0x23, 0x72 }).GetBytes(32);
                aes.Key = key;
                aes.IV = new byte[16]; // Must match the IV used for encryption

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }

                    return Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }
    }
}
