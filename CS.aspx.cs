using System;
using System.Web.UI;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.IO;
using System.Web.UI.WebControls;
using System.Text;

public partial class CS : Page
{

    private string customerID;
    private string email;
    private string price;
    private string variance;

    public CS()
    {
        customerID = "";
        email = "";
        price = "";
        variance = "";   
        
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            SqlConnection scon = new SqlConnection("Data Source=DESKTOP-A3EV9HL\\SQLEXPRESS;Initial Catalog=web_repairs;Integrated Security=True");
            StringBuilder htmlTable = new StringBuilder();

            if (!IsPostBack)
            {
                using (SqlCommand scmd = new SqlCommand())
                {
                    scmd.Connection = scon;
                    scmd.CommandType = CommandType.Text;
                    scmd.CommandText = "SELECT * FROM Orders";
                    scon.Open();
                    SqlDataReader articleReader = scmd.ExecuteReader();

                    htmlTable.Append("<table border='1'>");
                    htmlTable.Append("<tr><th>Customer</th><th>Email</th><th>Price</th><th>Variance</th><th>Status</th></tr>");

                    if (articleReader.HasRows)
                    {
                        while (articleReader.Read())
                        {
                            htmlTable.Append("<tr>");
                            htmlTable.Append("<td>" + articleReader["Customer"] + "</td>");
                            htmlTable.Append("<td>" + articleReader["Email"] + "</td>");
                            htmlTable.Append("<td>" + articleReader["Price"] + "</td>");
                            htmlTable.Append("<td>" + articleReader["Variance"] + "</td>");
                            htmlTable.Append("<td>" + articleReader["Status"] + "</td>");
                            htmlTable.Append("</tr>");
                        }
                        htmlTable.Append("</table>");

                        tbl_Orders.Controls.Add(new Literal { Text = htmlTable.ToString() });

                        articleReader.Close();
                        articleReader.Dispose();
                    }
                }
                SqlConnection con = new SqlConnection("Data Source=DESKTOP-A3EV9HL\\SQLEXPRESS;Initial Catalog=web_repairs;Integrated Security=True");
                SqlCommand cmd = new SqlCommand("SELECT ID, Name FROM Employee", con);
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(dt);
                ddlEmployee.DataSource = dt;
                ddlEmployee.DataTextField = "Name";
                ddlEmployee.DataValueField = "ID";
                ddlEmployee.DataBind();
            }
        }
    }
    protected void RegisterOrder(object sender, EventArgs e)
    {
        customerID = txtCustomer.Text.Trim();
        email = txtEmail.Text.Trim();
        price = txtPrice.Text.Trim();
        variance = txtVariance.Text.Trim();
        int userId = 0;
        string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        using (SqlConnection con = new SqlConnection(constr))
        {
            using (SqlCommand cmd = new SqlCommand("Insert_Order"))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Customer", customerID);
                    cmd.Parameters.AddWithValue("@Employee", ddlEmployee.SelectedValue);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@Variance", variance);
                    cmd.Connection = con;
                    con.Open();
                    userId = Convert.ToInt32(cmd.ExecuteScalar());
                    con.Close();
                }
            }
            string message = string.Empty;
            switch (userId)
            {
                case -1:
                    message = "Kostnaðurin kann ikki vera lægri enn 0.";
                    break;
                case -2:
                    message = "Tú mást velja eitt starvsfólk.";
                    break;
                default:
                    message = "Alt gekk gott og kundin hevur fingið ein email.";
                    SendActivationEmail(userId);
                    break;
            }
            ClientScript.RegisterStartupScript(GetType(), "alert", "alert('" + message + "');", true);
        }
    }

    private void SendActivationEmail(int userId)
    {
        string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
        string acceptCode = Guid.NewGuid().ToString();
        string denyCode = Guid.NewGuid().ToString();
        using (SqlConnection con = new SqlConnection(constr))
        {
            using (SqlCommand cmd = new SqlCommand("UPDATE Orders SET AcceptID = @acceptCode, DenyID = @denyCode WHERE ID=@ID"))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ID", userId);
                    cmd.Parameters.AddWithValue("@acceptCode", acceptCode);
                    cmd.Parameters.AddWithValue("@denyCode", denyCode);
                    cmd.Connection = con;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }
        using (MailMessage mm = new MailMessage("noreply@tt.fo", email))
        {
            mm.IsBodyHtml = true;
            mm.Subject = "Verkstaðið";
            mm.AlternateViews.Add(createHtmlStringBody(acceptCode, denyCode));
            SmtpClient smtp = setupSmtp();
            smtp.Send(mm);
        }
    }

    private AlternateView createHtmlStringBody(string acceptCode, string denyCode)
    {
        // Image Attachments
        string _filepath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        LinkedResource buttonYes = new LinkedResource(_filepath + @"/img/buttonyes.png");
        buttonYes.ContentId = Guid.NewGuid().ToString();
        LinkedResource buttonNo = new LinkedResource(_filepath + @"/img/buttonno.png");
        buttonNo.ContentId = Guid.NewGuid().ToString();
        LinkedResource ttLogo = new LinkedResource(_filepath + @"/img/logo.png");
        ttLogo.ContentId = Guid.NewGuid().ToString();

        // Email Body        
        string htmlbody = "<head><body>";
        htmlbody += "<a href = '" + "www.tt.fo" + "'><img src='cid:" + ttLogo.ContentId + "'></a><br /><br />";
        htmlbody += "<br /><br />Eftir at vit hava kannað KT-útgerðina hjá tygum, eru vit komin framm til hesa meting:";
        htmlbody += "<br /><br />Vegleiðandi kostnaðurin fyri umvælingina av tykkara KT-útgerð er áljóðandi " + price + " kr. <br /> G.G.! Hesing kostnaðurin kemur útyvir áður goldna kanningargjaldið.";
        htmlbody += "<br /><br />Prísurin er vegleiðandi, og kann eitt frávik áljóðandi " + variance + " kr. koma fyri báðar vegir.";
        htmlbody += "<br /><br />Um tú ynskir umvælingina framda eftir omanfyri standandi treytum, trýst so á grøna knappin. <br /> Ynskir tú ikki at fremja umvælingina, vel so reyða knappin, og KT-útgerð tygara er klár at heinta í Teldutænastuni<br/>";
        htmlbody += "<br /><a href = '" + Request.Url.AbsoluteUri.Replace("CS.aspx", "CS_Activation.aspx?ActivationCode=" + acceptCode) + "'><img src='cid:" + buttonYes.ContentId + "'></a>";
        htmlbody += "<a href = '" + Request.Url.AbsoluteUri.Replace("CS.aspx", "CS_Activation.aspx?ActivationCode=" + denyCode) + "'><img src='cid:" + buttonNo.ContentId + "'></a>";        
        htmlbody += "<br /><br />Eru spurningar hesum viðvíkjandi eru tygum vælkomin at ringja til okkara.";
        htmlbody += "<br /><br />Blíðar heilsanir";
        htmlbody += "<br /><br />TelduTænastan";
        htmlbody += "<br />Tlf. (+298) 606061";        
        htmlbody += "</body></head>";

        // Converts string to html and add images
        AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlbody, null, MediaTypeNames.Text.Html);
        alternateView.LinkedResources.Add(buttonYes);
        alternateView.LinkedResources.Add(buttonNo);
        alternateView.LinkedResources.Add(ttLogo);

        return alternateView;
    }

    private SmtpClient setupSmtp()
    {
        string username = ConfigurationManager.ConnectionStrings["smtpun"].ConnectionString;
        string password = ConfigurationManager.ConnectionStrings["smtppw"].ConnectionString;
        SmtpClient smtp = new SmtpClient();
        smtp.Host = "smtp.gmail.com";
        smtp.EnableSsl = true;
        NetworkCredential NetworkCred = new NetworkCredential(username, password);
        smtp.UseDefaultCredentials = true;
        smtp.Credentials = NetworkCred;
        smtp.Port = 587;
        return smtp;
    }
}