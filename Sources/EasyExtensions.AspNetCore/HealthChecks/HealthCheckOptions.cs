namespace EasyExtensions.AspNetCore.HealthChecks
{
    public class HealthCheckOptions
    {
        /// <summary>
        /// Check if DNS resolver is available using <see cref="DnsHealthCheck"/>.
        /// </summary>
        public bool Dns { get; set; } = true;

        /// <summary>
        /// Check if a web page is available using <see cref="InternetHealthCheck"/>.
        /// </summary>
        public bool Internet { get; set; } = true;

        /// <summary>
        /// Check if network is available using <see cref="NetworkHealthCheck"/>.
        /// </summary>
        public bool Network { get; set; } = true;

        /// <summary>
        /// Check if disk space is available using <see cref="DiskSpaceHealthCheck"/>.
        /// </summary>
        public bool DiskSpace { get; set; } = true;

        /// <summary>
        /// Check if memory is available using <see cref="MemoryHealthCheck"/>.
        /// </summary>
        public bool Memory { get; set; } = true;

        /// <summary>
        /// Use all available checks.
        /// </summary>
        public bool All
        {
            get => Dns && Internet && Network && DiskSpace && Memory;
            set
            {
                Dns = value;
                Internet = value;
                Network = value;
                DiskSpace = value;
                Memory = value;
            }
        }
    }
}
