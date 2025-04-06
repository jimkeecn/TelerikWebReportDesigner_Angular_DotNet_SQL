using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SqlDefinitionStorageExample.EFCore;
using SqlDefinitionStorageExample.EFCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Telerik.Reporting;
using Telerik.Reporting.Services;

namespace SqlDefinitionStorageExample
{
    public class CustomReportSourceResolver : IReportSourceResolver
    {
        private SqlDefinitionStorageContext _dbContext { get; }
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomReportSourceResolver(SqlDefinitionStorageContext context, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public ReportSource Resolve(string uri, OperationOrigin operationOrigin, IDictionary<string, object> currentParameterValues)
        {
            var reportPackager = new ReportPackager();

            if (!uri.Contains("Reports\\"))
            {
                uri = $"Reports\\{uri}";
            }

            var report = _dbContext.Resources.FirstOrDefault(r => r.Uri == uri.Replace("/", "\\"))
                         ?? throw new FileNotFoundException();

            var accessToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            using var originalStream = new MemoryStream(report.Bytes);
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
                                // Ensure ParameterValues exists and is updated
                                var paramValuesAttr = ds.Attribute("ParameterValues")?.Value;
                                var paramValues = new Dictionary<string, string>();

                                if (!string.IsNullOrWhiteSpace(paramValuesAttr))
                                {
                                    try
                                    {
                                        paramValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(paramValuesAttr) ?? new();
                                    }
                                    catch
                                    {
                                        paramValues = new Dictionary<string, string>();
                                    }
                                }

                                paramValues["authorization"] = accessToken;
                                ds.SetAttributeValue("ParameterValues", JsonConvert.SerializeObject(paramValues));

                                // Ensure <Parameters> node exists
                                var parametersNode = ds.Element(ns + "Parameters");
                                if (parametersNode == null)
                                {
                                    parametersNode = new XElement(ns + "Parameters");
                                    ds.Add(parametersNode);
                                }

                                var authParam = parametersNode.Elements(ns + "WebServiceParameter")
                                    .FirstOrDefault(p => p.Attribute("Name")?.Value == "authorization");

                                if (authParam != null)
                                {
                                    var valueElement = authParam.Element(ns + "Value")?.Element(ns + "String");
                                    if (valueElement != null)
                                        valueElement.Value = accessToken;
                                }
                                else
                                {
                                    parametersNode.Add(new XElement(ns + "WebServiceParameter",
                                        new XAttribute("WebServiceParameterType", "Header"),
                                        new XAttribute("Name", "authorization"),
                                        new XElement(ns + "Value", new XElement(ns + "String", accessToken))
                                    ));
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
            var reportDocument = (Telerik.Reporting.Report)reportPackager.UnpackageDocument(outputStream);

            return new InstanceReportSource
            {
                ReportDocument = reportDocument
            };
        }
    }
}
