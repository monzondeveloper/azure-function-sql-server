using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace AzureFunctionSql
{
    public static class AddProduct
    {
        [FunctionName("AddProduct")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var product = JsonConvert.DeserializeObject<Product>(requestBody);

            return new OkObjectResult("Product added succesfully");
        }

        public static void AddSingleProduct(Product product)
        {
            SqlConnection conn = GetConnection();

            var products = new List<Product>();

            string statement = "INSERT INTO PRODUCTS (ProductId, ProductName, Quantity) " +
                               "VALUES(@param1, @param2, @param3)";

            try
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(statement, conn))
                {
                    cmd.Parameters.Add("ProductId", SqlDbType.Int).Value = product.ProductId;
                    cmd.Parameters.Add("ProductName", SqlDbType.VarChar, 200).Value = product.ProductName;
                    cmd.Parameters.Add("Quantity", SqlDbType.Int).Value = product.Quantity;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally 
            { 
                conn.Close();
                conn.Dispose();
            }
        }

        private static SqlConnection GetConnection()
        {
            string connectionString = "Server=tcp:appserver5000.database.windows.net,1433;Initial Catalog=appdb;Persist Security Info=False;User ID=mmonzon;Password=Azure@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            return new SqlConnection(connectionString);
        }
    }
}
