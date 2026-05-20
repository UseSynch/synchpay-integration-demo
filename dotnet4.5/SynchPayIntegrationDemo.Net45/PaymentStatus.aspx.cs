using System;

namespace SynchPayIntegrationDemo.Net45
{
    public partial class PaymentStatus : System.Web.UI.Page
    {
        protected string EncryptedData { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            EncryptedData = Request.QueryString["data"] ?? string.Empty;
        }
    }
}
