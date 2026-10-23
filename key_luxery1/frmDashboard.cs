using System;
using System.Drawing;
using System.Windows.Forms;

namespace key_luxery1
{
	public partial class frmDashboard : Form
	{
		// Property to store the logged-in user's role (Set from Login Form)
		public string UserRole { get; set; } = "Admin";

		public frmDashboard()
		{
			InitializeComponent();
		}

		private void frmDashboard_Load(object sender, EventArgs e)
		{
			// Set current date on load
			lblDate.Text = "Date: " + DateTime.Now.ToString("yyyy-MM-dd");

			// Apply Role-Based Access Control
			ApplyUserPermissions();
		}

		#region Role-Based Access Control & Layout Alignment

		public void ApplyUserPermissions()
		{
			bool isAdmin = string.Equals(UserRole, "Admin", StringComparison.OrdinalIgnoreCase);

			if (!isAdmin)
			{
				// --- 1. SIDEBAR PERMISSIONS (Non-Admin) ---
				BtnDashboard.Visible = false;
				PnlDashboard.Visible = false;

				btnExpense.Visible = false;
				pnlExpense.Visible = false;

				btnReport.Visible = false;
				pnlReport.Visible = false;

				BtnUsers.Visible = false;
				PnlUsers.Visible = false;

				// Non-Admin keeps: Settings (for changing password) and About
				BtnSetting.Visible = true;
				PnlSetting.Visible = true;
				BtnAbout.Visible = true;
				PnlAbout.Visible = true;

				// --- 2. TOP TABS PERMISSIONS (Non-Admin) ---
				btnProducts.Visible = false;
				pnlProducts.Visible = false;

				btnPayments.Visible = false;
				pnlPayment.Visible = false;

				btnCustomer.Visible = false;
				pnlCustomer.Visible = false;

				// Non-Admin keeps: Sales & Invoices
				btnSales.Visible = true;
				pnlSale.Visible = true;
				btnInvoice.Visible = true;
				pnlInvoice.Visible = true;

				// --- 3. RE-ALIGN TOP TABS FOR CLEANER UI ---
				// Move Sales and Invoices together cleanly when other tabs are hidden
				btnSales.Location = new Point(241, 43);
				pnlSale.Location = new Point(241, 86);

				btnInvoice.Location = new Point(373, 43);
				pnlInvoice.Location = new Point(373, 86);

				// --- 4. DEFAULT VIEW FOR NON-ADMIN ---
				ShowView(uSales1, pnlSale);
			}
			else
			{
				// --- ADMIN HAS FULL ACCESS ---
				BtnDashboard.Visible = true;
				PnlDashboard.Visible = true;
				btnExpense.Visible = true;
				pnlExpense.Visible = true;
				BtnSetting.Visible = true;
				PnlSetting.Visible = true;
				btnReport.Visible = true;
				pnlReport.Visible = true;
				BtnUsers.Visible = true;
				PnlUsers.Visible = true;
				BtnAbout.Visible = true;
				PnlAbout.Visible = true;

				btnSales.Visible = true;
				pnlSale.Visible = true;
				btnProducts.Visible = true;
				pnlProducts.Visible = true;
				btnInvoice.Visible = true;
				pnlInvoice.Visible = true;
				btnPayments.Visible = true;
				pnlPayment.Visible = true;
				btnCustomer.Visible = true;
				pnlCustomer.Visible = true;

				// Default view for Admin
				ShowView(uHome1, PnlDashboard);
			}
		}

		#endregion

		#region Helper Methods for Navigation

		/// <summary>
		/// Brings the target UserControl to the front and lights up its active indicator panel.
		/// </summary>
		private void ShowView(UserControl view, Panel activeIndicator)
		{
			// Hide all active indicator panels
			PnlDashboard.Visible = false;
			pnlExpense.Visible = false;
			PnlSetting.Visible = false;
			pnlReport.Visible = false;
			PnlUsers.Visible = false;
			PnlAbout.Visible = false;

			pnlSale.Visible = false;
			pnlProducts.Visible = false;
			pnlInvoice.Visible = false;
			pnlPayment.Visible = false;
			pnlCustomer.Visible = false;

			// Highlight active selection
			if (activeIndicator != null)
			{
				activeIndicator.Visible = true;
			}

			// Bring selected UserControl to front
			view.BringToFront();
		}

		#endregion

		#region Sidebar Navigation Click Events

		private void BtnDashboard_Click(object sender, EventArgs e)
		{
			ShowView(uHome1, PnlDashboard);
		}

		private void btnExpense_Click(object sender, EventArgs e)
		{
			// If you have a User Control for Expenses, show it here:
			frmExpense expense = new frmExpense();
			expense.ShowDialog();
			PnlSidebar.Visible = true;
		}

		private void BtnSetting_Click(object sender, EventArgs e)
		{
			ShowView(uSettings1, PnlSetting);
		}

		private void btnReport_Click(object sender, EventArgs e)
		{
			frmReport frmReport = new frmReport();
			frmReport.ShowDialog();
		}

		private void BtnUsers_Click(object sender, EventArgs e)
		{
			ShowView(uManageUsers1, PnlUsers);
		}

		private void BtnAbout_Click(object sender, EventArgs e)
		{
			ShowView(uAbout1, PnlAbout);
		}

		private void BtnSidebar_Click(object sender, EventArgs e)
		{
			// Toggle sidebar width (Expand / Collapse feature)
			if (PnlSidebar.Width == 203)
			{
				PnlSidebar.Width = 50;
			}
			else
			{
				PnlSidebar.Width = 203;
			}
		}

		#endregion

		#region Top Bar Navigation Click Events

		private void btnLora_Click(object sender, EventArgs e) // Sales Button
		{
			ShowView(uSales1, pnlSale);
		}

		private void btnPlates_Click(object sender, EventArgs e) // Products Button
		{
			ShowView(uProducts1, pnlProducts);
		}

		private void btnInvoice_Click(object sender, EventArgs e)
		{
			ShowView(uInvoice1, pnlInvoice);
		}

		private void btnPayments_Click(object sender, EventArgs e)
		{
			ShowView(uPayments1, pnlPayment != null ? pnlPayment : null);
		}

		private void btnCustomer_Click(object sender, EventArgs e)
		{
			ShowView(uCustomer1, pnlCustomer);
		}

		#endregion

		#region Logout & Close Handling

		private void btnLogout_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(
				"Are you sure you want to log out?",
				"Logout Confirmation",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question
			);

			if (result == DialogResult.Yes)
			{
				// Find and show login form, then close this dashboard
				Form loginForm = Application.OpenForms["frmLogin"];
				if (loginForm != null)
				{
					loginForm.Show();
				}
				this.Hide();
			}
		}

		private void frmDashboard_FormClosed(object sender, FormClosedEventArgs e)
		{
			Application.Exit();
		}

		private void panel3_Paint(object sender, PaintEventArgs e)
		{
			// Optional custom painting for header panel if needed
		}

		#endregion
	}
}