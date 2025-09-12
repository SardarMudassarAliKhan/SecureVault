using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SecureVault
{
    public partial class Form1 : Form
    {
        private readonly string key ;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string plainText = txtInput.Text.Trim();
                if (string.IsNullOrEmpty(plainText))
                {
                    MessageBox.Show("Please enter text to encrypt.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string encrypted = EncryptString(plainText, key);
                txtOutput.Text = encrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Encryption Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string cipherText = txtInput.Text.Trim();
                if (string.IsNullOrEmpty(cipherText))
                {
                    MessageBox.Show("Please enter encrypted text to decrypt.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string decrypted = DecryptString(cipherText, key);
                txtOutput.Text = decrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Decryption Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOutput.Text))
            {
                Clipboard.SetText(txtOutput.Text);
                MessageBox.Show("Result copied to clipboard ✅", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No result to copy!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Encrypt
        private string EncryptString(string plainText, string key)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                // Normalize key to 32 bytes (AES-256)
                byte[] keyBytes = new byte[32];
                byte[] inputKeyBytes = Encoding.UTF8.GetBytes(key);
                Array.Copy(inputKeyBytes, keyBytes, Math.Min(inputKeyBytes.Length, keyBytes.Length));

                aes.Key = keyBytes;
                aes.GenerateIV(); // random IV

                using (var ms = new MemoryStream())
                {
                    // Write IV first
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, Encoding.UTF8))
                    {
                        sw.Write(plainText); // works even for very long text
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        // Decrypt
        private string DecryptString(string cipherText, string key)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                // Normalize key to 32 bytes
                byte[] keyBytes = new byte[32];
                byte[] inputKeyBytes = Encoding.UTF8.GetBytes(key);
                Array.Copy(inputKeyBytes, keyBytes, Math.Min(inputKeyBytes.Length, keyBytes.Length));

                aes.Key = keyBytes;

                // Extract IV (first 16 bytes)
                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using (var ms = new MemoryStream(cipher))
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, Encoding.UTF8))
                {
                    return sr.ReadToEnd(); // recovers even very long text
                }
            }
        }
    }
}
