namespace Model
{
    /// <summary>
    /// Model for limiting network speed
    /// </summary>
    public class SpeedLine
    {
        /// <summary>
        /// From node with this id
        /// </summary>
        public Guid From { get; set; }

        /// <summary>
        /// To node with this id
        /// </summary>
        public Guid To { get; set; }

        /// <summary>
        /// Speed of line as byte
        /// </summary>
        public long Speed { get; set; }
    }
}
