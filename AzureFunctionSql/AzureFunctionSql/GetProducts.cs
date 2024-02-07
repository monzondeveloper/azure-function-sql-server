using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AzureFunctionSql
{
    public static class GetProducts
    {
        private static readonly IConfiguration _configuration;

        [FunctionName("GetProducts")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult(JsonConvert.SerializeObject(GetAllProducts()));
        }

        [FunctionName("GetProductByID")]
        public static async Task<IActionResult> RunGetProductByID(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            int productId = int.Parse(req.Query["id"]);
            return new OkObjectResult(GetProductById(productId));
        }

        public static List<Product> GetProductById(int id)
        {
            SqlConnection conn = GetConnection();

            var products = new List<Product>();

            string statement = String.Format("SELECT ProductId, ProductName, " +
                                             "Quantity FROM Products WHERE ProductId = {0}", id);

            conn.Open();

            SqlCommand cmd = new SqlCommand(statement, conn);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product product = new Product
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Quantity = reader.GetInt32(2)
                    };

                    products.Add(product);
                }
            }

            conn.Close();
            conn.Dispose();

            return products;
        }

        public static List<Product> GetAllProducts()
        {
            SqlConnection conn = GetConnection();

            var products = new List<Product>();

            string statement = "SELECT ProductId, ProductName, Quantity FROM Products";

            conn.Open();

            SqlCommand cmd = new SqlCommand(statement, conn);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Product product = new Product
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1),
                        Quantity = reader.GetInt32(2)
                    };

                    products.Add(product);
                }
            }

            conn.Close();
            conn.Dispose();

            return products;
        }

        private static SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("SQLConnection"));
        }
    }
}
