namespace Model
{
    /// <summary>
    /// Model for limiting network speed
    /// </summary>
    public class SpeedLine
    {
        public Guid From { get; set; }
        public Guid To { get; set; }
        public long Speed { get; set; }
    }
}
