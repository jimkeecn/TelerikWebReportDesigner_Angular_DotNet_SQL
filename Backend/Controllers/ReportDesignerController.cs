namespace SqlDefinitionStorageExample.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;
    using Telerik.Reporting.Services;
    using Telerik.WebReportDesigner.Services;
    using Telerik.WebReportDesigner.Services.Controllers;
    using Telerik.WebReportDesigner.Services.Models;

    [Authorize]
    [Route("api/reportdesigner")]
    public class ReportDesignerController : ReportDesignerControllerBase
    {
        private readonly IConfiguration _configuration;
        public ReportDesignerController(IReportDesignerServiceConfiguration reportDesignerServiceConfiguration, IReportServiceConfiguration reportServiceConfiguration, IConfiguration configuration)
            : base(reportDesignerServiceConfiguration, reportServiceConfiguration)
        {
            _configuration = configuration;
        }

        [HttpPost("reports/save")]
        public override async Task<IActionResult> SaveReportByUriAsync([FromQuery] string uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return NoContent();

            return await base.SaveReportByUriAsync(uri);
        }

        [HttpGet("reports/report")]
        public override async Task<IActionResult> GetReportAsync([FromQuery] string uri)
        {
           var data = await base.GetReportAsync(uri);
           return data;
        }

        [HttpPost("clients")]
        public override IActionResult RegisterClient()
        {
            return base.RegisterClient();
        }

        [HttpPost("definitionresources/preview/webservice")]
        public override IActionResult PreviewWebServiceData(DataSourceInfo dataSourceInfo)
        {
            if (dataSourceInfo.NetType == "WebServiceDataSource" && dataSourceInfo.DataSource is JObject jObject)
            {
                // Extract bearer token from Authorization header
                var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var parameters = jObject["Parameters"] as JArray;
                    if (parameters != null)
                    {
                        bool found = false;
                        foreach (var param in parameters)
                        {
                            if (param["Name"]?.ToString() == "authorization")
                            {
                                param["Value"] = authHeader;
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            var authParam = new JObject
                            {
                                ["Name"] = "authorization",
                                ["Value"] = authHeader,
                                ["WebServiceParameterType"] = "Header",
                                ["NetType"] = "WebServiceParameter"
                            };
                            parameters.Add(authParam);
                        }
                    }
                }
            }

            var data = base.PreviewWebServiceData(dataSourceInfo);
            return data;
        }


    }
}
