using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace key_luxery1
{
	public partial class frmLogin : Form
	{
		private sharedClass db = new sharedClass();

		public frmLogin()
		{
			InitializeComponent();
		}
		public void loginFunction()
		{
			string username = TxtUser.Text.Trim();
			string password = TxtPassword.Text.Trim();

			// Open main dashboard and hide login screen
			frmDashboard dashboard = new frmDashboard();

			// Validate inputs
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
			{
				MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				// Query user record matching exact schema columns
				string query = $"SELECT userName, passWord, role, user_Status FROM tblUser WHERE userName = '{username.Replace("'", "''")}'";
				DataTable dt = sharedClass.GetData(query);

				if (dt.Rows.Count > 0)
				{
					DataRow row = dt.Rows[0];
					string encryptedDbPassword = row["passWord"]?.ToString();
					string userStatus = row["user_Status"]?.ToString();
					string role = row["role"]?.ToString();

					// Check if user status is active
					bool isActive = string.Equals(userStatus, "Active", StringComparison.OrdinalIgnoreCase) ||
									string.Equals(userStatus, "1", StringComparison.OrdinalIgnoreCase) ||
									string.Equals(userStatus, "True", StringComparison.OrdinalIgnoreCase);

					if (!isActive)
					{
						MessageBox.Show("Your account is inactive or disabled. Please contact the administrator.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}

					// Decrypt the password retrieved from the database
					string decryptedDbPassword = Decrypt(encryptedDbPassword);

					// Verify password
					if (decryptedDbPassword == password)
					{
						if (role == "Admin")
						{
							dashboard.PnlSidebar.Visible = true;
							dashboard.BtnSidebar.Visible = true;
						}
						else if (role != "Admin")
						{
							dashboard.PnlSidebar.Visible = false;
							dashboard.BtnSidebar.Visible = false;
							dashboard.uHome1.Visible = false;
						}
						else
						{
							MessageBox.Show("Your role is not recognized. Please contact the administrator.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
							return;
						}

						MessageBox.Show($"Welcome back, {username}!\nRole: {role}", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

						dashboard.lblUsername.Text = username;
						dashboard.Show();
						this.Hide();
					}
					else
					{
						MessageBox.Show("Invalid password. Please try again.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
						TxtPassword.Clear();
						TxtPassword.Focus();
					}
				}
				else
				{
					MessageBox.Show("Username not found in the system.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
					TxtUser.Focus();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Database login error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static readonly string SecretKey = "0123456789abcdef0123456789abcdef"; // Must be exactly 32 characters (bytes) for AES-256

		public string Encrypt(string plainText)
		{
			if (string.IsNullOrEmpty(plainText)) return plainText;

			byte[] iv = new byte[16];
			byte[] array;

			using (Aes aes = Aes.Create())
			{
				aes.Key = Encoding.UTF8.GetBytes(SecretKey);
				aes.IV = iv;

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
						{
							streamWriter.Write(plainText);
						}
						array = memoryStream.ToArray();
					}
				}
			}

			return Convert.ToBase64String(array);
		}

		public string Decrypt(string cipherText)
		{
			if (string.IsNullOrEmpty(cipherText)) return cipherText;

			try
			{
				byte[] iv = new byte[16];
				byte[] buffer = Convert.FromBase64String(cipherText);

				using (Aes aes = Aes.Create())
				{
					aes.Key = Encoding.UTF8.GetBytes(SecretKey);
					aes.IV = iv;
					ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

					using (MemoryStream memoryStream = new MemoryStream(buffer))
					{
						using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
						{
							using (StreamReader streamReader = new StreamReader(cryptoStream))
							{
								return streamReader.ReadToEnd();
							}
						}
					}
				}
			}
			catch
			{
				// Fallback if the database contains old plain-text passwords during migration
				return cipherText;
			}
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			loginFunction();
		}
		private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
		{
			// If the checkbox is checked, reveal the password text; otherwise, mask it.
			if (chkShowPassword.Checked == true)
			{
				TxtPassword.UseSystemPasswordChar = false;
			}
			else
			{
				TxtPassword.UseSystemPasswordChar = true;
			}
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		private void frmLogin_FormClosed(object sender, FormClosedEventArgs e)
		{
			Application.Exit();
		}

		private void txtPassword_KeyDown(object sender, KeyEventArgs e)
		{
			// Check if the Enter key was pressed
			if (e.KeyCode == Keys.Enter)
			{
				// Suppress the annoying "ding" sound
				e.SuppressKeyPress = true;

				// Trigger the login button's click event
				btnLogin_Click(sender, e);
			}
		}
		private void btnConfig_Click(object sender, EventArgs e)
		{

		}
	}
}
