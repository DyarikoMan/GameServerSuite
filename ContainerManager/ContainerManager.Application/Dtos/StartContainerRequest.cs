using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerManager.Application.Dtos
{
    public class StartContainerRequest
    {
        public string Name { get; set; } = null!;
        public string Image { get; set; } = null; 
        public int RamMb { get; set; } = 512;
        public double Cpu { get; set; } = 0.5;
        public bool AutoRemove { get; set; } = true;
        public string RestartPolicy { get; set; } = "unless-stopped";
        public int Port { get; set; } = 0;
        public int ContainerPort { get; set; } = 0;
        public bool IsUdp { get; set; } = true;
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new();

    }
}
