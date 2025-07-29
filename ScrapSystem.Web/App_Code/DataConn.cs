using Microsoft.Data.SqlClient;
using System.Data;

namespace PMG_system.App_Code
{
    public class DataConn
    {
        public static string source;
        private static SqlConnection con;
        public static int gio;
        // static String result;
        public static string path = "~/CELL_MG/Employee_Images/";
        public static string path2 = "~/Images/";
        public static string[] extensions = { ".gif", ".jpg", ".bmp", ".jpeg", ".png" };

        static DataConn()
        {
            //source = @"Data Source=MRHIEN-PC\SQLEXPRESS;Initial Catalog=PROJECTOR_TRACE;User ID=sa;Password=12345678@@--10.92.186.30";
            //source = @"Data Source=192.168.128.33;Initial Catalog=PMG_system;User ID=sa;Password=Psnvdb2013";
            // source = @"Data Source=10.92.186.30;Initial Catalog=PMG_system;User ID=scan;Password=khong123";

            //source = @"Data Source=10.92.186.30;Initial Catalog=PMG_system;User ID=scan;Password=khong123";
            //source = @"Data Source=192.168.128.33;Initial Catalog=PMG_system;User ID=scan;Password=khong123";

            //Data Source=./;Initial Catalog=DataNhaHang;User ID='sa';Password=''
            //source = @"Data Source=./;Initial Catalog=OQC;User ID='sa';Password=''";

            source = @"Server=192.168.128.1;Database=ScrapSystem;User Id=sa;Password=Psnvdb2013;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;";


            //source = @"Data Source=192.168.128.116;Initial Catalog=OQC;User ID=sa;Password=Psnvdb2013";
            con = new SqlConnection(source);
            try
            {
                con.Open();
            }
            catch
            {
                con.Close();
            }
        }
        // SQL thuần -> Tra ra dt
        public static DataTable DataTable_Sql(string sql)
        {
            using (SqlConnection conn = new SqlConnection(source))
            {
                using (SqlDataAdapter dap = new SqlDataAdapter(sql, conn))
                {
                    using (DataSet ds = new DataSet())
                    {
                        dap.Fill(ds);
                        conn.Close();
                        conn.Dispose();
                        return ds.Tables[0];
                    }
                }
            }
        }

        
        public static int Execute_NonSQL(string sql)
        {
            SqlTransaction sqltran = null;
            //try
            //{
            SqlConnection conn = new SqlConnection(source);

            int row = 0;
            conn.Open();
            sqltran = conn.BeginTransaction();
            SqlCommand cmd = new SqlCommand(sql, conn, sqltran);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            row = cmd.ExecuteNonQuery();
            sqltran.Commit();
            conn.Dispose();
            conn.Close();
            return row;
            //}
            //catch (Exception ex)
            //{
            //    // throw new Exception(ex.Message);
            //    sqltran.Rollback();
            //    var _ex = new Exception(ex.Message + "Chỗ này sai rồi... : '" + sql + "'");
            //    throw _ex;
            //}
        }


        // store tra ra datatable
        public static DataTable SQLFillDS(string query_object, CommandType type, params object[] obj)
        {
            SqlConnection conn = new SqlConnection(source);
            conn.Open();
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
            conn.Dispose();
            conn.Close();
            return ds.Tables[0];
        }
        //public static DataTable SQLFillDS(string query_object)
        //{
        //    SqlConnection conn = new SqlConnection(source);
        //    conn.Open();
        //    SqlCommand cmd = new SqlCommand(query_object, conn);
        //    using (SqlDataReader dr = cmd.ExecuteReader())
        //    {
        //        var tb = new DataTable();
        //        tb.Load(dr);
        //        return tb;
        //    };
        //    conn.Dispose();
        //    conn.Close();
        //}

        public static DataTable StoreFillDS(string query_object, CommandType type, params object[] obj)
        {
            SqlConnection conn = new SqlConnection(source);
            conn.Open();
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
            conn.Dispose();
            conn.Close();
            return ds.Tables[0];
        }

        public static int SQLint(string query_object)
        {
            int row;
            SqlConnection conn = new SqlConnection(source);
            conn.Open();
            SqlCommand cmd = new SqlCommand(query_object, conn);
            row = cmd.ExecuteNonQuery();
            conn.Dispose();
            conn.Close();
            return row;
        }
        /*Nguyen Hien*/
        //Store Procedure tra ve datatable
        // store thuc thi cau lenh
        public static int ExecuteStore(string query_object, CommandType type, params object[] obj)
        {
            int row = 0;
            SqlConnection conn = new SqlConnection(source);
            conn.Open();
            SqlCommand cmd = new SqlCommand(query_object, conn);
            cmd.CommandType = type;
            SqlCommandBuilder.DeriveParameters(cmd);
            for (int i = 1; i <= obj.Length; i++)
            {
                cmd.Parameters[i].Value = obj[i - 1];
            }
            cmd.ExecuteNonQuery();
            conn.Dispose();
            conn.Close();
            return row;
        }

        

        public static int ExecuteStoreInt(string query_object, CommandType type, params object[] obj)
        {
            int row = 0;
            SqlConnection conn = new SqlConnection(source);
            conn.Open();
            SqlCommand cmd = new SqlCommand(query_object, conn);
            cmd.CommandType = type;
            SqlCommandBuilder.DeriveParameters(cmd);
            for (int i = 1; i <= obj.Length; i++)
            {
                cmd.Parameters[i].Value = obj[i - 1];
            }
            row = cmd.ExecuteNonQuery();
            conn.Dispose();
            conn.Close();
            return row;
        }

        private static string _connectionString = "Server=192.168.128.127; User ID=postgres; Password=Psnv@2021; Port=5432; Database=id41HD;";

        //public static DataTable DataTable_Sql_NPG(string sql)
        //{
        //    using (var conn = new NpgsqlConnection(_connectionString))
        //    {
        //        using (var dap = new NpgsqlDataAdapter(sql, conn))
        //        {
        //            using (DataSet ds = new DataSet())
        //            {
        //                dap.Fill(ds);
        //                conn.Close();
        //                conn.Dispose();
        //                return ds.Tables[0];
        //            }
        //        }
        //    }
        //}
    }
}