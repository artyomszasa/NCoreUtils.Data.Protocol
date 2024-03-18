#if !NET7_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    //
    // Summary:
    //     Indicates that the specified method requires the ability to generate new code
    //     at runtime, for example through System.Reflection.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
    internal sealed class RequiresDynamicCodeAttribute : Attribute
    {
        //
        // Summary:
        //     Initializes a new instance of the System.Diagnostics.CodeAnalysis.RequiresDynamicCodeAttribute
        //     class with the specified message.
        //
        // Parameters:
        //   message:
        //     A message that contains information about the usage of dynamic code.
        public RequiresDynamicCodeAttribute(string message)
            => Message = message;

        //
        // Summary:
        //     Gets a message that contains information about the usage of dynamic code.
        public string Message { get; }
        //
        // Summary:
        //     Gets or sets an optional URL that contains more information about the method,
        //     why it requires dynamic code, and what options a consumer has to deal with it.
        public string? Url { get; set; }
    }
}

#endif