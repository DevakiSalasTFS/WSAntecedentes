using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace wsAntecedentes
{
    /// <summary>
    /// Summary description for antecedentes
    /// </summary>
    [WebService(Namespace = "http://tfsm/wsAntecedentes")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class antecedentes : System.Web.Services.WebService
    {

        [WebMethod]
        public XmlDocument getAntecedentes(int idCliente, string rfc, string nombre, string paterno, string materno)
        {
            clsNegocio neg = new clsNegocio();
            XmlDocument response = new XmlDocument();
            try {                
                response.LoadXml(neg.getAntecedentes(idCliente, rfc, nombre, paterno, materno));
            } catch (Exception ex) {
                response.LoadXml("<error>Error: getAntecedentes: " + ex.Message + "</error>");                
            }
            //Create an XML declaration. 
            XmlDeclaration xmldecl;
            xmldecl = response.CreateXmlDeclaration("1.0", "utf-8", null);

            //Add the new node to the document.
            XmlElement root = response.DocumentElement;
            response.InsertBefore(xmldecl, root);
            return response;            
        }
    }
}
