namespace SharedCache.Core.Providers.Keys
{
    /// <summary>
    /// Represents the key validation mode
    /// </summary>
    public enum KeyValidationMode
    {
        /// <summary>
        /// Do not check 
        /// </summary>
        NoCheck,

        /// <summary>
        /// Warns if illegal chars are used
        /// </summary>
        CheckAndWarn,

        /// <summary>
        /// Throws exception if illegal chars are used
        /// </summary>
        CheckAndFail,
    }
}