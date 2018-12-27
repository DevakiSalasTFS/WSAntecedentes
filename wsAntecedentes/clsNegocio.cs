using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace wsAntecedentes
{
    public class clsNegocio
    {
        public const int _timeOutPorDefault = 1000;

        public string getAntecedentes(int idCliente, string rfc, string nombre, string paterno, string materno)
        {
            try {
                if (string.IsNullOrEmpty(rfc) && string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(paterno) && string.IsNullOrEmpty(materno) && idCliente == 0) 
                    throw new Exception("Al menos un campo debe contener un valor.");

                string cadConex = ConfigurationManager.ConnectionStrings["ConexionProlease"].ConnectionString;
                string ptimeOut = ConfigurationManager.AppSettings["ProleaseTimeOut"];
                               
                if (cadConex != null) {
                    using (SqlConnection con = new SqlConnection(cadConex)) {
                        using (SqlCommand cmd = new SqlCommand("TFSM_Antecedentes", con)) {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NOMBRE", SqlDbType.VarChar).Value = string.IsNullOrEmpty(nombre) == true ? null : nombre.Trim();
                            cmd.Parameters.Add("@PATERNO", SqlDbType.VarChar).Value = string.IsNullOrEmpty(paterno) == true ? null : paterno.Trim();
                            cmd.Parameters.Add("@MATERNO", SqlDbType.VarChar).Value = string.IsNullOrEmpty(materno) == true ? null : materno.Trim();
                            cmd.Parameters.Add("@IDCLIENTE", SqlDbType.Int).Value = idCliente == 0 ? (object)DBNull.Value : idCliente;
                            cmd.Parameters.Add("@RFC", SqlDbType.VarChar).Value = string.IsNullOrEmpty(rfc) == true ? null : rfc.Trim();
                            con.Open();
                            if (ptimeOut.Equals(null))
                                cmd.CommandTimeout = _timeOutPorDefault;
                            else
                                cmd.CommandTimeout = Convert.ToInt32(ptimeOut);

                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            DataSet dsPrepagoTotal = new DataSet();
                            adapter.Fill(dsPrepagoTotal);

                            adapter.Dispose();
                            cmd.Dispose();

                            if (dsPrepagoTotal.Tables[0] != null && dsPrepagoTotal.Tables[0].Rows.Count > 0)
                                return dsPrepagoTotal.GetXml();
                            else
                                return "<NewDataSet/>";
                        }
                    }                    
                } else
                    throw new Exception("No se obtuvo la cadena de conexión");

            } catch (Exception ex) {
                throw ex;
            } 
        }
    }
}