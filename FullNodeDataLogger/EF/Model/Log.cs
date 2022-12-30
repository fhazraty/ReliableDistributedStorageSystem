namespace FullNodeDataLogger.EF.Model
{
    public partial class Log
    {
        public int Id { get; set; }
        public string? LogGroup { get; set; }
        public DateTime LogTime { get; set; }
        public string? FromId { get; set; }
        public string? ToId { get; set; }
        public string? Message { get; set; }
        public string? IP { get; set; }
        public string? Port { get; set; }
        public string Description { get; set; }
    }
}
