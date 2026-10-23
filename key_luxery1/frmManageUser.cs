using System;
using System.Data;
using System.IO;
using System.Runtime.Remoting.Lifetime;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace key_luxery1
{
	public partial class frmManageUser : Form
	{
		private sharedClass db = new sharedClass();

		public frmManageUser()
		{
			InitializeComponent();
		}

		private void frmManageUser_Load(object sender, EventArgs e)
		{
			LoadUserData();
		}

		private void LoadUserData()
		{
			try
			{
				string query = "SELECT user_id, userName, passWord, role, recoveryQuestion, recoveryAnswer, user_Status, date FROM tblUser";
				DataTable dt = sharedClass.GetData(query);
				dgvUsers.DataSource = dt;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static readonly string SecretKey = "0123456789abcdef0123456789abcdef"; // Must be exactly 32 characters

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
				// Fallback for any old plain-text passwords
				return cipherText;
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			// Validate required fields
			if (string.IsNullOrWhiteSpace(txtUserName.Text))
			{
				MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				// Helper to produce either NULL or a properly escaped quoted SQL literal
				Func<string, string> SqlLiteralOrNull = s =>
					string.IsNullOrWhiteSpace(s) ? "NULL" : $"'{s.Trim().Replace("'", "''")}'";

				// Prepare sanitized values
				string userNameVal = txtUserName.Text.Trim().Replace("'", "''");

				// Encrypt the password before formatting it into a SQL literal
				string rawPassword = txtPassword.Text;
				string encryptedPassword = string.IsNullOrWhiteSpace(rawPassword) ? "" : Encrypt(rawPassword);
				string passwordVal = SqlLiteralOrNull(encryptedPassword);

				string roleVal = SqlLiteralOrNull(cmbRole.Text);
				string questionVal = SqlLiteralOrNull(cmbRecoveryQuestion.Text);
				string answerVal = SqlLiteralOrNull(txtRecoveryAnswer.Text);

				// Default status to 'Active' when not provided
				string statusVal = string.IsNullOrWhiteSpace(cmbStatus.Text) ? "'Active'" : SqlLiteralOrNull(cmbStatus.Text);
				string dateVal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

				// Build the INSERT statement carefully
				string query = "INSERT INTO tblUser (userName, passWord, role, recoveryQuestion, recoveryAnswer, user_Status, date) VALUES ("
								+ $"'{userNameVal}', "
								+ $"{passwordVal}, "
								+ $"{roleVal}, "
								+ $"{questionVal}, "
								+ $"{answerVal}, "
								+ $"{statusVal}, "
								+ $"'{dateVal}')";

				// Execute and refresh
				db.ExecuteQueries(query);
				MessageBox.Show("User saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

				LoadUserData();
				ClearFields();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error saving user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtUserId.Text))
			{
				MessageBox.Show("Please select a user from the grid to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			try
			{
				// Encrypt the password before updating (handles empty fields safely)
				string rawPassword = txtPassword.Text.Trim();
				string encryptedPassword = string.IsNullOrEmpty(rawPassword) ? "" : Encrypt(rawPassword);
				string passwordVal = encryptedPassword.Replace("'", "''");

				string query = $"UPDATE tblUser SET " +
							   $"userName = '{txtUserName.Text.Trim().Replace("'", "''")}', " +
							   $"passWord = '{passwordVal}', " +
							   $"role = '{cmbRole.Text.Trim().Replace("'", "''")}', " +
							   $"recoveryQuestion = '{cmbRecoveryQuestion.Text.Trim().Replace("'", "''")}', " +
							   $"recoveryAnswer = '{txtRecoveryAnswer.Text.Trim().Replace("'", "''")}', " +
							   $"user_Status = '{cmbStatus.Text.Trim().Replace("'", "''")}' " +
							   $"WHERE user_id = {txtUserId.Text.Trim()}";

				db.ExecuteQueries(query);
				MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
				LoadUserData();
				ClearFields();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error updating user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtUserId.Text))
			{
				MessageBox.Show("Please select a user from the grid to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			DialogResult confirm = MessageBox.Show("Are you sure you want to delete this user?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (confirm == DialogResult.Yes)
			{
				try
				{
					string query = $"DELETE FROM tblUser WHERE user_id = {txtUserId.Text.Trim()}";
					db.ExecuteQueries(query);
					MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

					LoadUserData();
					ClearFields();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error deleting user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			ClearFields();
		}

		private void ClearFields()
		{
			txtUserId.Clear();
			txtUserName.Clear();
			txtPassword.Clear();
			cmbRole.SelectedIndex = -1;
			cmbRecoveryQuestion.SelectedIndex = -1;
			txtRecoveryAnswer.Clear();
			cmbStatus.SelectedIndex = -1;
		}

		private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{
				DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
				txtUserId.Text = row.Cells["user_id"].Value?.ToString();
				txtUserName.Text = row.Cells["userName"].Value?.ToString();
				txtPassword.Text = row.Cells["passWord"].Value?.ToString();
				cmbRole.Text = row.Cells["role"].Value?.ToString();
				cmbRecoveryQuestion.Text = row.Cells["recoveryQuestion"].Value?.ToString();
				txtRecoveryAnswer.Text = row.Cells["recoveryAnswer"].Value?.ToString();
				cmbStatus.Text = row.Cells["user_Status"].Value?.ToString();
			}
		}
	}
}