using System.Collections.Generic;

namespace WebAPI.Data
{
    public class Project
    {
        public string Id { get; set; }
        public string ConnectorId { get; set; }
        public string ContainerName { get; set; }
        public List<Job> Jobs { get; set; }
    }
}