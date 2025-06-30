namespace ContainerManager.Api.Models
{
    public class StartImageRequest
    {
        public string TarFile { get; set; } = string.Empty;           
        public string? Name { get; set; }                          
        public int? Port { get; set; }                           
        public int? RamMb { get; set; }                               
        public double? Cpu { get; set; }                                 
        public bool AutoRemove { get; set; } = false;   
        public string RestartPolicy { get; set; } = "unless-stopped";    
    }
}

