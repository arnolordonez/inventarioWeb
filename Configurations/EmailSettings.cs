namespace InventarioWEB.Configurations
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool EnableSsl { get; set; }
    }
}