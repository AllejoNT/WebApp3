
using Microsoft.SqlServer.Management.IntegrationServices;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication3.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            
            string folderName = "ConArchETL";
            string environmentName = "";

            string cloudDatabaseHost = ".";
            //string cloudDatabaseUser = ".";
            //string cloudDatabasePassword = ".";

            string settings = string.Copy(ConfigurationManager.ConnectionStrings["ManagementContext"].ConnectionString);


            string ADO_Evasys_ConnectionString = ".";

            // Create a connection to the server
            List<string> connectionConfigs = settings.Split(';').Where(c => !string.IsNullOrEmpty(c)).ToList();
            Dictionary<string, string> connectionValues = connectionConfigs.Select(item => item.Split('=')).ToDictionary(s => s[0], s => s[1]);

            connectionValues.TryGetValue("Data Source", out cloudDatabaseHost);

            string sqlConnectionString = "Data Source=" + cloudDatabaseHost +
                ";Initial Catalog=master;Integrated Security=SSPI;";
            SqlConnection sqlConnection = new SqlConnection(sqlConnectionString);


            // Create the Integration Services object
            IntegrationServices integrationServices = new IntegrationServices(sqlConnection);

            // Get the Integration Services catalog
            Catalog catalog = integrationServices.Catalogs["SSISDB"];

            // Get the folder
            CatalogFolder folder = catalog.Folders[folderName];

            // Get Environment
            environmentName = String.Format("Environment_{0}", folder.Environments.ToList().Count*2);
            EnvironmentInfo environmentInfo = folder.Environments[environmentName];

            if (environmentInfo != null)
            {
                environmentInfo.Drop();
            }

            environmentInfo = new EnvironmentInfo(folder, environmentName, environmentName);
            environmentInfo.Create();

            if (null == environmentInfo.Variables["CM.ADO_Evasys.ConnectionString"])
            {
                environmentInfo.Variables.Add("CM.ADO_Evasys.ConnectionString", TypeCode.String, ADO_Evasys_ConnectionString, false, "ConnectionString");
            }
            else
            {
                environmentInfo.Variables["CM.ADO_Evasys.ConnectionString"].Value = ADO_Evasys_ConnectionString;
            }

            environmentInfo.Alter();
            return cloudDatabaseHost;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
