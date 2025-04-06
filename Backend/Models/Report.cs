using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace SqlDefinitionStorageExample.Models
{
    public class ReportCreationModel
    {
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }
        public string ReportWebService {  get; set; }
    }
    public class ReportType
    {
        public int Id { get; set; }
    }

    public class ResourceBaseNoEf
    {
        public string Name { get; set; }
        public byte[] Bytes { get; set; }
        public float Size { get; set; }
        public string ParentUri { get; set; }
        public string Uri { get; set; }
        public string Description { get; set; }
    }
}
