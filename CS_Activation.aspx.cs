using System;
using System.Web.UI;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;


public partial class CS_Activation : Page
{

    string confirmMessage = "";

    //Stored for future reference
    string customer;
    string customerEmail;
    string employee;
    string price;

    string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
    string username = ConfigurationManager.ConnectionStrings["smtpun"].ConnectionString;
    string password = ConfigurationManager.ConnectionStrings["smtppw"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {


        string confirmID = !string.IsNullOrEmpty(Request.QueryString["ActivationCode"]) ? Request.QueryString["ActivationCode"] : Guid.Empty.ToString();
        using (SqlConnection con = new SqlConnection(constr))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Customer, Email, EmployeeID, AcceptID, DenyID, Price FROM Orders WHERE AcceptID = @confirmID OR DenyID = @confirmID"))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@confirmID", confirmID);
                    cmd.Connection = con;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        customer = reader[0].ToString();
                        customerEmail = reader[1].ToString();
                        employee = reader[2].ToString();
                        price = reader[5].ToString();

                        if (reader[3].ToString().Equals(confirmID))
                        {
                            
                            cmd.CommandText = "UPDATE Orders SET Status = 1, DateConfirmed = @dateConfirmed WHERE AcceptID = @AcceptID";
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@AcceptID", confirmID);
                            cmd.Parameters.AddWithValue("@dateConfirmed", DateTime.Now);
                            cmd.Connection = con;
                            reader.Close();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            con.Close();
                            if (rowsAffected == 1)
                            {
                                confirmMessage = "Takk fyri, vit venda aftur skjótast gjørligt.";
                                ltMessage.Text = confirmMessage;
                                SendStatusEmailToTT(true);
                                //SendAcceptEmailToCustomer();
                            }
                            else
                            {
                                confirmMessage = "Okkurt gekk galið. Kanska hava tygum longi góðtikið fylgiseðilin.<br /><br />Vinarliga ring á 606061.";
                                ltMessage.Text = confirmMessage;
                            }
                        }
                        else if (reader[4].ToString().Equals(confirmID))
                        {
                            cmd.CommandText = "Update Orders SET Status = 0, DateConfirmed = @dateConfirmed WHERE DenyID = @DenyID";
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@DenyID", confirmID);
                            cmd.Parameters.AddWithValue("@dateConfirmed", DateTime.Now);
                            cmd.Connection = con;
                            reader.Close();
                            int rowsAffected = cmd.ExecuteNonQuery();
                            con.Close();
                            if (rowsAffected == 1)
                            {
                                confirmMessage = "Takk fyri, KT-útgerð tygara er klár at heinta.";
                                ltMessage.Text = confirmMessage;
                                SendStatusEmailToTT(false);
                                //SendAcceptEmailToCustomer();
                            }
                            else
                            {
                                confirmMessage = "Okkurt gekk galið. Kanska hava tygum longi góðtikið fylgiseðilin.<br /><br />Vinarliga ring á 606061.";
                                ltMessage.Text = confirmMessage;
                            }

                        }
                        break;
                    }

                    con.Close();

                }
            }
        }
    }

    private string[] GetEmployeeInfo(string employeeCheck)
    {
        string[] result = new string[2];

        using (SqlConnection con = new SqlConnection(constr))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Name, Email FROM Employee WHERE ID = @employee"))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@employee", employeeCheck);
                    cmd.Connection = con;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        result[0] = reader[0].ToString();
                        result[1] = reader[1].ToString();
                    }
                    reader.Close();
                    con.Close();
                }
            }
        }

        return result;
    }

    private void SendStatusEmailToTT(bool accept)
    {
        string[] employeeInfo = GetEmployeeInfo(employee);

        using (MailMessage mm = new MailMessage("noreply@tt.fo", employeeInfo[1]))
        {
            mm.Subject = "Viðv. Kundanr. " + customer;
            string body;
            if(accept)
                body = "Hey " + employeeInfo[0] + ", kundinr. " + customer + "hevur góðtiki ein ordra á kr. " + price;
            else
                body = "Hey " + employeeInfo[0] + ", kundinr. " + customer + "hevur NOKTA ein ordra á kr. " + price;
            mm.Body = body;
            mm.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            NetworkCredential NetworkCred = new NetworkCredential(username, password);
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = 587;
            smtp.Send(mm);
        }
    }
}