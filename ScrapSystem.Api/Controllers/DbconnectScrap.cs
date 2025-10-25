using Microsoft.Data.SqlClient;
using System.Data;

namespace ScrapSystem.Api.Controllers
{
    //public class DbconnectScrap
    //{
    //}
    public static class DbconnectScrap
    {
        public static string connection_string = "Data Source=192.168.128.1;Initial Catalog=ScrapSystem;User ID=scan;Password=khong123;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;";
        public static DataTable StoreFillDS(string query_object, CommandType type, params object[] obj)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                conn.Open();
                try
                {
                    SqlCommand cmd = new SqlCommand(query_object, conn);
                    cmd.CommandType = type;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    for (int i = 1; i <= obj.Length; i++)
                    {
                        cmd.Parameters[i].Value = obj[i - 1];
                    }
                    SqlDataAdapter dap = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    dap.Fill(ds);
                    conn.Close();
                    return ds.Tables[0];
                }
                catch (Exception ex)
                {
                    throw;
                }

            }
        }

        public static DataSet StoreFillDataSet(string query_object, CommandType type, params object[] obj)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                conn.Open();
                try
                {
                    SqlCommand cmd = new SqlCommand(query_object, conn);
                    cmd.CommandType = type;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    for (int i = 1; i <= obj.Length; i++)
                    {
                        cmd.Parameters[i].Value = obj[i - 1];
                    }
                    SqlDataAdapter dap = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    dap.Fill(ds);
                    conn.Close();
                    return ds;
                }
                catch (Exception)
                {
                    conn.Close();
                    DataSet dt = new DataSet();
                    return dt;
                }

            }
        }


        public static object getscalra(string query_object, CommandType type, params object[] obj)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connection_string))
                {
                    Object data;
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query_object, conn);
                    cmd.CommandType = type;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    for (int i = 1; i <= obj.Length; i++)
                    {
                        cmd.Parameters[i].Value = obj[i - 1];
                    }
                    data = cmd.ExecuteScalar();
                    conn.Close();
                    return data;
                }
            }
            catch (Exception ex)
            {
                return "FAIL";
            }

        }
        public static int excutenonquerry(string query_object, CommandType type, params object[] obj)
        {
            try
            {
                int data = 0;
                using (SqlConnection conn = new SqlConnection(connection_string))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query_object, conn);
                    cmd.CommandType = type;
                    SqlCommandBuilder.DeriveParameters(cmd);
                    for (int i = 1; i <= obj.Length; i++)
                    {
                        cmd.Parameters[i].Value = obj[i - 1];
                    }
                    data = cmd.ExecuteNonQuery();
                    conn.Close();
                    return data;
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public static object querygetscalra(string query_object)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                Object data;
                conn.Open();
                SqlCommand cmd = new SqlCommand(query_object, conn);
                cmd.CommandType = CommandType.Text;
                data = cmd.ExecuteScalar();
                conn.Close();
                return data;
            }
        }
        public static int querynonquery(string query_object)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                int data;
                conn.Open();
                SqlCommand cmd = new SqlCommand(query_object, conn);
                cmd.CommandType = CommandType.Text;
                data = cmd.ExecuteNonQuery();
                conn.Close();
                return data;
            }
        }

        public static DataTable querygettable(string query_object)
        {
            using (SqlConnection conn = new SqlConnection(connection_string))
            {
                conn.Open();
                try
                {
                    SqlCommand cmd = new SqlCommand(query_object, conn);
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter dap = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    dap.Fill(dt);
                    conn.Close();
                    return dt;
                }
                catch (Exception)
                {
                    conn.Close();
                    DataTable dt = new DataTable();
                    return dt;
                }

            }
        }
    }
}
