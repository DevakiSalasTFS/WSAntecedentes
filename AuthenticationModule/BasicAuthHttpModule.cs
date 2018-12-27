using Seguridad;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;

namespace WebHostBasicAuth.Modules
{
    public class BasicAuthHttpModule : IHttpModule
    {
       private const string Realm = "";

        public void Init(HttpApplication context)
        {
          context.AuthenticateRequest += new EventHandler(BasicAuthHttpModule.OnApplicationAuthenticateRequest);
          context.EndRequest += new EventHandler(BasicAuthHttpModule.OnApplicationEndRequest);
        }

        private static void SetPrincipal(IPrincipal principal)
        {
          Thread.CurrentPrincipal = principal;
          if (HttpContext.Current == null)
            return;
          HttpContext.Current.User = principal;
        }

        private static bool CheckPassword(string username, string password)
        {
            bool flag = false;
            string empty1 = string.Empty;
            string empty2 = string.Empty;
          
            if (ConfigurationManager.ConnectionStrings["CS_tfsm_api"] == null)
                throw new Exception("No existe la cadena de conexión.");
            string connectionString = ConfigurationManager.ConnectionStrings["CS_tfsm_api"].ConnectionString;
            if (ConfigurationManager.AppSettings["servicio"] == null)
                throw new Exception("No esta configurado el servicio indicado.");
            string str = ConfigurationManager.AppSettings["servicio"];
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                SqlCommand selectCommand = new SqlCommand("sps_ValidaUsuario", connection);
                int num = 4;
                selectCommand.CommandType = (CommandType) num;
                selectCommand.Parameters.Add(new SqlParameter("@userName", (object) username));
                selectCommand.Parameters.Add(new SqlParameter("@password", (object) BasicAuthHttpModule.Codifica(password)));
                selectCommand.Parameters.Add(new SqlParameter("@webService", (object) str));
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(selectCommand);
                DataTable dataTable1 = new DataTable();
                DataTable dataTable2 = dataTable1;
                sqlDataAdapter.Fill(dataTable2);
                if (dataTable1 != null) {
                  if (dataTable1.Rows != null) {
                    if (dataTable1.Rows.Count > 0)
                      flag = Convert.ToBoolean(dataTable1.Rows[0][0].ToString());
                  }
                }
              } catch (Exception ex) {
                BasicAuthHttpModule.subEscribeLog(ex.Message);
                throw ex;
              } finally {
                if (connection != null && connection.State != ConnectionState.Closed)
                  connection.Close();
              }
              return flag;
        }

        private static byte[] GetBytes(string str)
        {
          byte[] numArray = new byte[str.Length * 2];
          Buffer.BlockCopy((Array) str.ToCharArray(), 0, (Array) numArray, 0, numArray.Length);
          return numArray;
        }

        private static string GetString(byte[] bytes)
        {
          char[] chArray = new char[bytes.Length / 2];
          Buffer.BlockCopy((Array) bytes, 0, (Array) chArray, 0, bytes.Length);
          return new string(chArray);
        }

        private static void AuthenticateUser(string credentials)
        {
          try
          {
            credentials = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(credentials));
            int length = credentials.IndexOf(':');
            if (BasicAuthHttpModule.CheckPassword(credentials.Substring(0, length), credentials.Substring(length + 1)))
                BasicAuthHttpModule.SetPrincipal((IPrincipal)new GenericPrincipal((IIdentity)new GenericIdentity(ConfigurationManager.AppSettings["servicio"]), (string[])null));
            else
              HttpContext.Current.Response.StatusCode = 401;
          }
          catch (FormatException ex)
          {
            HttpContext.Current.Response.StatusCode = 401;
          }
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
          string header = HttpContext.Current.Request.Headers["Authorization"];
          if (header == null)
            return;
          AuthenticationHeaderValue authenticationHeaderValue = AuthenticationHeaderValue.Parse(header);
          if (!authenticationHeaderValue.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) || authenticationHeaderValue.Parameter == null)
            return;
          BasicAuthHttpModule.AuthenticateUser(authenticationHeaderValue.Parameter);
        }

        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
          HttpResponse response = HttpContext.Current.Response;
          if (response.StatusCode != 401)
            return;
          response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", (object) ""));
        }

        public void Dispose()
        {
        }

        public static string Codifica(string cadena)
        {
          try
          {
            return new Seguridad.Seguridad().CodificarClave(cadena, TipoEncriptacion.TripleDes);
          }
          catch (Exception ex)
          {
            return "Error: BasicAuthHttpModule: Codifica " + ex.Message;
          }
        }

        public static string Decodifica(string cadena)
        {
          try
          {
            return new Seguridad.Seguridad().DecodificarClave(cadena, TipoEncriptacion.TripleDes);
          }
          catch (Exception ex)
          {
            return "Error: BasicAuthHttpModule: Decodifica " + ex.Message;
          }
        }

        public static void subEscribeLog(string strError_I)
        {
          string path = System.AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["RutaYArchivoDeBitacora"] ?? "";
          try
          {
              if (!File.Exists(path))
                  File.Create(path, (int)byte.MaxValue, FileOptions.WriteThrough);
            StreamWriter streamWriter = new StreamWriter(path, true);
            string str1 = "-----------------------------------------------------------------------------";
            streamWriter.WriteLine(str1);
            string str2 = DateTime.Now.ToString("dd/MM/yyyy hh:mm:sss.fff tt");
            streamWriter.WriteLine(str2);
            string str3 = strError_I;
            streamWriter.WriteLine(str3);
            streamWriter.Flush();
            streamWriter.Close();
          }
          catch (Exception ex)
          {
          }
        }
  }
}
