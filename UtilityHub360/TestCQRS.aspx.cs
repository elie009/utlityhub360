using System;
using System.Web.UI;
using UtilityHub360.CQRS.MediatR;
using UtilityHub360.CQRS.Queries;
using UtilityHub360.DependencyInjection;

namespace UtilityHub360
{
    public partial class TestCQRS : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Test CQRS implementation
                var container = ServiceContainer.CreateDefault();
                var mediator = container.GetService<IMediator>();

                // Test GetAllUsersQuery
                var query = new GetAllUsersQuery();
                var users = mediator.Send(query).Result;

                lblResult.Text = "✅ CQRS is working! Found " + users.Count + " users in the database.";
                lblResult.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblResult.Text = "❌ CQRS Error: " + ex.Message;
                lblResult.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
