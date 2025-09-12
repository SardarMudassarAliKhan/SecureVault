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

        // AES Encryption
        private string EncryptString(string plainText, string key)
        {
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                // Normalize the key to 32 bytes (256 bits)
                byte[] keyBytes = new byte[32];
                byte[] inputKeyBytes = Encoding.UTF8.GetBytes(key);

                // Copy or trim input key
                Array.Copy(inputKeyBytes, keyBytes, Math.Min(inputKeyBytes.Length, keyBytes.Length));

                aes.Key = keyBytes;
                aes.GenerateIV(); // random IV for security

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // prepend IV

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
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



        // AES Decryption
        private string DecryptString(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(buffer))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
