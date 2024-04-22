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
        protected NetworkModule(byte index)
        {
            Index = index;
        }

        /// <summary>
        /// The index of this module in list
        /// </summary>
        public byte Index { get; }

        /// <summary>
        /// Accepts incoming data
        /// </summary>
        /// <param name="data">The data designated for this module</param>
        public abstract void InData(in byte[] data);

        /// <summary>
        /// Execute behaviour
        /// </summary>
        public abstract void Modulate();

        /// <returns>The data to be sent over to the wire</returns>
        public abstract byte[] OutData();
    }
}