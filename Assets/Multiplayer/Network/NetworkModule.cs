namespace Core.Multiplayer
{
    /// <summary>
    /// Repersents a single networked behaviour
    /// </summary>
    public abstract class NetworkModule
    {
        /// <summary>
        /// Creates a new network module
        /// </summary>
        /// <param name="index">The index of this module in the list</param>
        protected NetworkModule(uint index)
        {
            Index = index;
        }

        /// <summary>
        /// The index of this module in list
        /// </summary>
        public uint Index { get; }
        /// <summary>
        /// Accepts incoming data
        /// </summary>
        /// <param name="data">The data designated for this module</param>
        public abstract void InData(in byte[] data);
        /// <summary>
        /// Execute behaviour
        /// </summary>
        public abstract void Modulate();
        /// <summary>
        /// Outputs data
        /// </summary>
        /// <param name="data">The data to be sent over to the wire</param>
        public abstract void OutData(out byte[] data);
    }
}