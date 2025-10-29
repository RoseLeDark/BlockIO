using BlockIO.Generic;


namespace BlockIO.GPT
{
    /// <summary>
    /// Represents a GPT-based storage device using the built-in <see cref="GPTParser"/>.
    /// Inherits partition management and stream access from <see cref="Device"/>.
    /// </summary>
    public class GPTDevice : Device
    {
        /// <summary>
        /// Initializes a new GPT device with the specified path.
        /// Automatically binds a <see cref="GPTParser"/> for partition discovery.
        /// </summary>
        /// <param name="devicePath">The path to the physical or virtual device.</param>
        /// <param name="bInitialisOnConstruct">
        /// If true, calls <see cref="Device.Initialis"/> during construction to immediately parse partitions.
        /// </param>
        public GPTDevice(string devicePath, bool bInitialisOnConstruct = false)
            : base(devicePath, new GPTParser(), bInitialisOnConstruct)
        {
        }
    }
}
