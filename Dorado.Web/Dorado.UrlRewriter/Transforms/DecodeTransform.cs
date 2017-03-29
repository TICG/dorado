// UrlRewriter - A .NET URL Rewriter module
// Version 2.0
//

//

using System.Web;
using Dorado.UrlRewriter.Utilities;

namespace Dorado.UrlRewriter.Transforms
{
    /// <summary>
    /// Url decodes the input.
    /// </summary>
    public sealed class DecodeTransform : IRewriteTransform
    {
        /// <summary>
        /// Applies a transformation to the input string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The transformed string.</returns>
        public string ApplyTransform(string input)
        {
            return HttpUtility.UrlDecode(input);
        }

        /// <summary>
        /// The name of the action.
        /// </summary>
        public string Name
        {
            get { return Constants.TransformDecode; }
        }
    }
}