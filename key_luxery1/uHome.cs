using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace key_luxery1
{
	public partial class uHome : UserControl
	{
		private const int LOW_STOCK_THRESHOLD = 5;
		sharedClass db = new sharedClass();

		public uHome()
		{
			InitializeComponent();
		}

		public void loadHome()
		{
			LoadDashboardMetrics();
			LoadTopSellingProducts();
			LoadCategorySales();
			LoadFinancialBreakdown();
			LoadRecentSalesFeed();
			LoadLowStockAlerts();
		}

		private void uHome_Load(object sender, EventArgs e)
		{
			loadHome();
		}

		/// <summary>
		/// Loads summary metrics for top KPI cards
		/// </summary>
		/// 

		public void LoadDashboardMetrics()
		{
			try
			{
				// 1. Card 1: Total Revenue (from tblInvoice.grand_total)
				string qryTotalRevenue = "SELECT ISNULL(SUM(grand_total), 0) FROM tblInvoice WHERE (deleted = 0 OR deleted IS NULL)";
				object res1 = db.ExecuteScalarQuery(qryTotalRevenue);
				lblCard1Value.Text = Convert.ToDecimal(res1 ?? 0).ToString("C2");

				// 2. Card 2: Total Sales Count (from tblSale)
				string qryTotalSalesCount = "SELECT COUNT(*) FROM tblSale WHERE (deleted = 0 OR deleted IS NULL)";
				object res2 = db.ExecuteScalarQuery(qryTotalSalesCount);
				lblCard2Value.Text = (res2 ?? 0).ToString();

				// 3. Card 3: Active Products Count (from tblproduct)
				string qryActiveProducts = "SELECT COUNT(*) FROM tblproduct WHERE status = 'Active' AND (deleted = 0 OR deleted IS NULL)";
				object res3 = db.ExecuteScalarQuery(qryActiveProducts);
				lblCard3Value.Text = (res3 ?? 0).ToString();

				// 4. Card 4: Last Month Revenue (from tblInvoice using invoice_date)
				string qryLastMonthRev = @"SELECT ISNULL(SUM(grand_total), 0) FROM tblInvoice 
                                           WHERE invoice_date >= DATEADD(month, DATEDIFF(month, 0, GETDATE()) - 1, 0)
                                           AND invoice_date < DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)
                                           AND (deleted = 0 OR deleted IS NULL)";
				object res4 = db.ExecuteScalarQuery(qryLastMonthRev);
				lblCard4Value.Text = Convert.ToDecimal(res4 ?? 0).ToString("C2");

				// 5. Card 5: Total Expenses (Kept from tblExpenses table)
				string qryTotalExpenses = "SELECT ISNULL(SUM(Amount), 0) from tblExpenses where deleted=0";
				object res5 = db.ExecuteScalarQuery(qryTotalExpenses);
				lblCard5Value.Text = Convert.ToDecimal(res5 ?? 0).ToString("C2");

				// 6. Card 6: Last Month Expenses (Kept from tblExpenses table)
				string qryLastMonthExp = @"SELECT ISNULL(SUM(Amount), 0) from tblExpenses 
                                           WHERE ExpenseDate >= DATEADD(month, DATEDIFF(month, 0, GETDATE()) - 1, 0)
                                           AND ExpenseDate < DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0)";
				object res6 = db.ExecuteScalarQuery(qryLastMonthExp);
				lblCard6Value.Text = Convert.ToDecimal(res6 ?? 0).ToString("C2");

				// --- Payment Methods Breakdown (from tblPayment.amount_paid & payment_method) ---
				// Zaad Total
				string qryZaad = "SELECT ISNULL(SUM(amount_paid), 0) FROM tblPayment WHERE payment_method = 'Zaad' AND (deleted = 0 OR deleted IS NULL)";
				object resZaad = db.ExecuteScalarQuery(qryZaad);
				lblZaadVal.Text = Convert.ToDecimal(resZaad ?? 0).ToString("C2");

				// eDahab Total
				string qryEdahab = "SELECT ISNULL(SUM(amount_paid), 0) FROM tblPayment WHERE payment_method = 'eDahab' AND (deleted = 0 OR deleted IS NULL)";
				object resEdahab = db.ExecuteScalarQuery(qryEdahab);
				lblEdahabVal.Text = Convert.ToDecimal(resEdahab ?? 0).ToString("C2");

				// Cash Total
				string qryCash = "SELECT ISNULL(SUM(amount_paid), 0) FROM tblPayment WHERE payment_method = 'Cash' AND (deleted = 0 OR deleted IS NULL)";
				object resCash = db.ExecuteScalarQuery(qryCash);
				lblCashVal.Text = Convert.ToDecimal(resCash ?? 0).ToString("C2");

				// Premiere Bank Total
				string qryBank = "SELECT ISNULL(SUM(amount_paid), 0) FROM tblPayment WHERE payment_method = 'Premiere Bank' AND (deleted = 0 OR deleted IS NULL)";
				object resBank = db.ExecuteScalarQuery(qryBank);
				lblBankVal.Text = Convert.ToDecimal(resBank ?? 0).ToString("C2");

				// Other Payment Methods Total
				string qryOther = "SELECT ISNULL(SUM(amount_paid), 0) FROM tblPayment WHERE payment_method NOT IN ('Zaad', 'eDahab', 'Premiere Bank') AND (deleted = 0 OR deleted IS NULL)";
				object resOther = db.ExecuteScalarQuery(qryOther);
				lblOtherVal.Text = Convert.ToDecimal(resOther ?? 0).ToString("C2");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading dashboard metrics: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		//public void LoadDashboardMetrics()
		//{
		//	try
		//	{
		//		// 1. Total Revenue
		//		object revObj = db.ExecuteScalar("SELECT SUM(grand_total) FROM tblInvoice WHERE deleted = 0 AND status = 'Paid'");
		//		decimal totalRevenue = (revObj != null && revObj != DBNull.Value) ? Convert.ToDecimal(revObj) : 0;
		//		lblCard1Value.Text = totalRevenue.ToString("C2");

		//		// 2. Total Sales Count
		//		object salesObj = db.ExecuteScalar("SELECT COUNT(invoice_id) FROM tblInvoice WHERE deleted = 0 AND status = 'Paid'");
		//		int totalSales = (salesObj != null && salesObj != DBNull.Value) ? Convert.ToInt32(salesObj) : 0;
		//		lblCard2Value.Text = totalSales.ToString("N0");

		//		// 3. Active Products Count
		//		object prodObj = db.ExecuteScalar("SELECT COUNT(product_id) FROM tblProduct WHERE deleted = 0");
		//		int activeProducts = (prodObj != null && prodObj != DBNull.Value) ? Convert.ToInt32(prodObj) : 0;
		//		lblCard3Value.Text = activeProducts.ToString("N0");

		//		// 4. Total Customers Count
		//		object custObj = db.ExecuteScalar("SELECT COUNT(customer_id) FROM tblCustomer WHERE deleted = 0");
		//		int totalCustomers = (custObj != null && custObj != DBNull.Value) ? Convert.ToInt32(custObj) : 0;
		//		lblCard4Value.Text = totalCustomers.ToString("N0");

		//		// 5. Low Stock Items Count
		//		object lowStockObj = db.ExecuteScalar($"SELECT COUNT(product_id) FROM tblProduct WHERE deleted = 0 AND quantity <= {LOW_STOCK_THRESHOLD}");
		//		int lowStockCount = (lowStockObj != null && lowStockObj != DBNull.Value) ? Convert.ToInt32(lowStockObj) : 0;
		//		lblCard5Value.Text = lowStockCount.ToString("N0");
		//	}
		//	catch (Exception ex)
		//	{
		//		System.Diagnostics.Debug.WriteLine("Error loading metrics: " + ex.Message);
		//	}
		//}

		/// <summary>
		/// Loads Top 10 Best Selling Products
		/// </summary>
		private void LoadTopSellingProducts()
		{
			try
			{
				string query = "SELECT TOP 10 product_name AS [Product], SUM(quantity) AS [Qty Sold], SUM(total_price) AS [Revenue] " +
							   "FROM tblSale WHERE deleted = 0 " +
							   "GROUP BY product_name " +
							   "ORDER BY [Qty Sold] DESC";

				DataTable dt = sharedClass.GetData(query);
				dgvTopProducts.DataSource = dt;

				if (dgvTopProducts.Columns.Contains("Revenue"))
				{
					dgvTopProducts.Columns["Revenue"].DefaultCellStyle.Format = "C2";
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Top Products Error: " + ex.Message);
			}
		}

		/// <summary>
		/// Loads Sales aggregated by Product Category
		/// </summary>
		private void LoadCategorySales()
		{
			try
			{
				string query = "SELECT p.category AS [Category], SUM(s.quantity) AS [Items Sold], SUM(s.total_price) AS [Total Revenue] " +
							   "FROM tblSale s " +
							   "INNER JOIN tblProduct p ON s.product_id = p.product_id " +
							   "WHERE s.deleted = 0 " +
							   "GROUP BY p.category " +
							   "ORDER BY [Total Revenue] DESC";

				DataTable dt = sharedClass.GetData(query);
				dgvCategorySales.DataSource = dt;

				if (dgvCategorySales.Columns.Contains("Total Revenue"))
				{
					dgvCategorySales.Columns["Total Revenue"].DefaultCellStyle.Format = "C2";
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Category Sales Error: " + ex.Message);
			}
		}



		/// <summary>
		/// Loads Gross Sales, Tax, Discount, and Net Profit Summaries
		/// </summary>
		private void LoadFinancialBreakdown()
		{
			try
			{
				string query = "SELECT SUM(subtotal) AS GrossSales, SUM(tax) AS TotalTax, SUM(discount) AS TotalDiscount, SUM(total_profit) AS NetProfit " +
							   "FROM tblInvoice WHERE deleted = 0 AND status = 'Paid'";

				DataTable dt = sharedClass.GetData(query);

				if (dt != null && dt.Rows.Count > 0)
				{
					DataRow row = dt.Rows[0];
					decimal grossSales = row["GrossSales"] != DBNull.Value ? Convert.ToDecimal(row["GrossSales"]) : 0;
					decimal totalTax = row["TotalTax"] != DBNull.Value ? Convert.ToDecimal(row["TotalTax"]) : 0;
					decimal totalDiscount = row["TotalDiscount"] != DBNull.Value ? Convert.ToDecimal(row["TotalDiscount"]) : 0;
					decimal netProfit = row["NetProfit"] != DBNull.Value ? Convert.ToDecimal(row["NetProfit"]) : 0;

					lblGrossSalesVal.Text = grossSales.ToString("C2");
					lblTaxVal.Text = totalTax.ToString("C2");
					lblDiscountVal.Text = totalDiscount.ToString("C2");
					lblNetProfitVal.Text = netProfit.ToString("C2");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Financial Breakdown Error: " + ex.Message);
			}
		}

		/// <summary>
		/// Loads latest sales feed
		/// </summary>
		private void LoadRecentSalesFeed()
		{
			try
			{
				string query = "SELECT TOP 15 invoice_no AS [Inv #], customer_name AS [Customer], grand_total AS [Total] " +
							   "FROM tblInvoice WHERE deleted = 0 AND status = 'Paid' " +
							   "ORDER BY invoice_date DESC";

				DataTable dt = sharedClass.GetData(query);
				dgvRecentSales.DataSource = dt;

				if (dgvRecentSales.Columns.Contains("Total"))
				{
					dgvRecentSales.Columns["Total"].DefaultCellStyle.Format = "C2";
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Recent Sales Error: " + ex.Message);
			}
		}

		/// <summary>
		/// Loads low stock items with conditional row colors
		/// </summary>
		private void LoadLowStockAlerts()
		{
			try
			{
				string query = $"SELECT product_name AS [Product], quantity AS [Stock], price AS [Price] " +
							   $"FROM tblProduct WHERE deleted = 0 AND quantity <= {LOW_STOCK_THRESHOLD} " +
							   $"ORDER BY quantity ASC";

				DataTable dt = sharedClass.GetData(query);
				dgvLowStock.DataSource = dt;

				if (dgvLowStock.Columns.Contains("Price"))
				{
					dgvLowStock.Columns["Price"].DefaultCellStyle.Format = "C2";
				}

				foreach (DataGridViewRow row in dgvLowStock.Rows)
				{
					if (row.Cells["Stock"].Value != null && Convert.ToInt32(row.Cells["Stock"].Value) == 0)
					{
						row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
						row.DefaultCellStyle.ForeColor = Color.DarkRed;
					}
					else
					{
						row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 230);
					}
				}

				if (dt != null && dt.Rows.Count > 0)
				{
					tabRightFeed.SelectedTab = tabLowStock;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("Low Stock Alerts Error: " + ex.Message);
			}
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			loadHome();
		}

	}
}