using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System;
using SqlDefinitionStorageExample.Models;
using Azure.Core;
using Newtonsoft.Json;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using Telerik.Reporting;
using System.Linq;

namespace SqlDefinitionStorageExample.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomReportController : Controller
    {
        private readonly IConfiguration _configuration;

        public CustomReportController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("get_products")]
        public async Task<IActionResult> GetProducts()
        {
            var connSection = _configuration.GetSection("ConnectionStrings:Telerik.Reporting.Examples.CSharp.Properties.Settings.TelerikConnectionString");
            var connectionString = connSection.GetValue<string>("connectionString");

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sql = "SELECT TOP 10 ProductID, Name, ListPrice FROM Production.Product ORDER BY ProductID";

                var products = await db.QueryAsync<ProductDto>(sql);

                return Ok(products);
            }
        }

        [HttpGet("get_departments")]
        public async Task<IActionResult> GetDepartment()
        {
            var connSection = _configuration.GetSection("ConnectionStrings:Telerik.Reporting.Examples.CSharp.Properties.Settings.TelerikConnectionString");
            var connectionString = connSection.GetValue<string>("connectionString");

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM [AdventureWorks].[HumanResources].[Department]";

                var products = await db.QueryAsync<DepartmentDto>(sql);

                return Ok(products);
            }
        }


        [HttpGet("get_reports")]
        public async Task<IActionResult> GetReport()
        {
            var connSection = _configuration.GetSection("ConnectionStrings:Telerik.Resource.Database");
            var connectionString = connSection.GetValue<string>("connectionString");
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sql = "SELECT [Name],[Description] FROM [WebDesignerStorage].[dbo].[Resources] WHERE [Name] != 'UniqueSampleData20250406.trdp' AND [ParentUri] NOT LIKE 'Resources%'";
                var reports = await db.QueryAsync<ResourceBaseNoEf>(sql);
                return Ok(reports);
            }
        }
        //The report name can't have special character, however this behaviour can be changed if dig deeper of the example code.
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewReport([FromBody] ReportCreationModel model)
        {
            var connSection = _configuration.GetSection("ConnectionStrings:Telerik.Resource.Database");
            var connectionString = connSection.GetValue<string>("connectionString");

            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sql = "SELECT [Name],[Bytes],[Size],[ParentUri],[Uri],[Description] FROM [WebDesignerStorage].[dbo].[Resources] WHERE Name = 'UniqueSampleData20250406.trdp'";
                var defaultTemplate = await db.QueryAsync<ResourceBaseNoEf>(sql);

                using var originalStream = new MemoryStream(defaultTemplate.FirstOrDefault().Bytes);
                using var outputStream = new MemoryStream();
                using (var archive = new ZipArchive(originalStream, ZipArchiveMode.Read, leaveOpen: true))
                using (var newArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: true))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName == "definition.xml")
                        {
                            using var reader = new StreamReader(entry.Open());
                            var xmlContent = reader.ReadToEnd();

                            var xDoc = XDocument.Parse(xmlContent);
                            XNamespace ns = "http://schemas.telerik.com/reporting/2023/2.1";

                            var dataSourcesNode = xDoc.Descendants(ns + "DataSources").FirstOrDefault();
                            if (dataSourcesNode != null)
                            {
                                foreach (var ds in dataSourcesNode.Elements(ns + "WebServiceDataSource"))
                                {
                                    var serviceUrlAttr = ds.Attribute("ServiceUrl");
                                    if (serviceUrlAttr != null)
                                    {
                                        //Current we are saving aboslute webservice path to the template, however, with some thinkings and custom design
                                        //we can save the relative path to the template, and based on the enviornment we are in, dynamically change
                                        //the path in the template on the fly.
                                        //e.g localhost:15150/api , tst.com/api , uat.com/api 
                                        serviceUrlAttr.Value = "http://localhost:51864/api/customreport/"+model.ReportWebService; 
                                    }
                                }
                            }

                            var newEntry = newArchive.CreateEntry("definition.xml");
                            using var writer = new StreamWriter(newEntry.Open());
                            writer.Write(xDoc.ToString(SaveOptions.DisableFormatting));
                        }
                        else
                        {
                            var newEntry = newArchive.CreateEntry(entry.FullName);
                            using var src = entry.Open();
                            using var dest = newEntry.Open();
                            src.CopyTo(dest);
                        }
                    }
                }

                outputStream.Seek(0, SeekOrigin.Begin);
                byte[] modifiedBytes = outputStream.ToArray();

                var newSql = @"
                                INSERT INTO [dbo].[Resources] (
                                    [Id],
                                    [Name],
                                    [Bytes],
                                    [CreatedOn],
                                    [ModifiedOn],
                                    [Size],
                                    [ParentUri],
                                    [Uri],
                                    [Description]
                                )
                                VALUES (
                                    @Id,
                                    @Name,
                                    @Bytes,
                                    @CreatedOn,
                                    @ModifiedOn,
                                    @Size,
                                    @ParentUri,
                                    @Uri,
                                    @Description
                                )";

                var reportId = model.ReportName.Trim() + ".trdp";
                var newResource = new
                {
                    Id = Guid.NewGuid().ToString(),  // or any unique string ID
                    Name = reportId,
                    Bytes = modifiedBytes,  // or any byte[]
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                    Size = defaultTemplate.FirstOrDefault().Size, // float (real)
                    ParentUri = "Reports",
                    Uri = "Reports\\"+ reportId,
                    Description = model.ReportDescription
                };

                await db.ExecuteAsync(newSql, newResource);

                return Ok(model);
            }

        }

        private class ProductDto
        {
            public int ProductID { get; set; }
            public string Name { get; set; }
            public decimal ListPrice { get; set; }
        }

        private class DepartmentDto
        {
            public int DepartmentID { get; set; }
            public string Name { get; set; }
            public string GroupName { get; set; }
            public DateTime ModifiedDate {  get; set; }
        }
    }
}
