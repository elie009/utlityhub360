using System;
using System.Web.UI;
using UtilityHub360.Models;

namespace UtilityHub360
{
    public partial class TestEF : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Test Entity Framework configuration
                using (var context = new UtilityHubDbContext())
                {
                    // Try to access the database
                    var userCount = context.Users.Count();
                    lblResult.Text = "✅ Entity Framework is working! Found " + userCount + " users in the database.";
                    lblResult.ForeColor = System.Drawing.Color.Green;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "❌ Entity Framework Error: " + ex.Message;
                lblResult.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
