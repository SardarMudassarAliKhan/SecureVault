using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SecureVault
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 🔐 AES Encryption
        private string Encrypt(string plainText, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(PadKey(key, aes.KeySize / 8));
                aes.GenerateIV();

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

        // 🔓 AES Decryption
        private string Decrypt(string cipherText, string key)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(PadKey(key, aes.KeySize / 8));

                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] cipher = new byte[fullCipher.Length - iv.Length];

                Array.Copy(fullCipher, iv, iv.Length);
                Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(cipher))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        // Pads or trims key to correct size
        private string PadKey(string key, int length)
        {
            if (key.Length > length)
                return key.Substring(0, length);
            else if (key.Length < length)
                return key.PadRight(length, '0');
            else
                return key;
        }

        // Encrypt Button
        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtInput.Text) || string.IsNullOrWhiteSpace(txtKey.Text))
                {
                    MessageBox.Show("Please enter text and key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtOutput.Text = Encrypt(txtInput.Text, txtKey.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Encryption failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Decrypt Button
        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtInput.Text) || string.IsNullOrWhiteSpace(txtKey.Text))
                {
                    MessageBox.Show("Please enter text and key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                txtOutput.Text = Decrypt(txtInput.Text, txtKey.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Decryption failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear Button
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtKey.Clear();
            txtOutput.Clear();
        }

        // Copy Button
        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtOutput.Text))
            {
                Clipboard.SetText(txtOutput.Text);
                MessageBox.Show("Output copied to clipboard!", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Nothing to copy.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnToggleKey_Click(object sender, EventArgs e)
        {
            if (txtKey.PasswordChar == '*')
            {
                txtKey.PasswordChar = '\0';
                btnToggleKey.Text = "🙈";
            }
            else
            {
                txtKey.PasswordChar = '*';  
                btnToggleKey.Text = "👁"; 
            }
        }

    }
}
