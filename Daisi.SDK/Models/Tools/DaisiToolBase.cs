using Daisi.SDK.Interfaces.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Daisi.SDK.Models.Tools
{
    public abstract class DaisiToolBase : IDaisiTool
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract ToolParameter[] Parameters { get; }

        public abstract ToolExecutionContext GetExecutionContext(IToolContext toolContext, CancellationToken cancellation, params ToolParameterBase[] parameters);

        /// <summary>
        /// Validates that the string passed in is a fully qualified URL.
        /// </summary>
        /// <param name="url">The url to validate.</param>
        /// <returns>True if the url is valid. False, if not.</returns>
        public bool IsUrl(string url)
        {
            return Regex.IsMatch(url, $"[http|https]://[a-zA-Z0-9.-]*");
        }

        /// <summary>
        /// Checks if a parameter is required. This is used when the 
        /// generated parameters need to be validated.
        /// </summary>
        /// <param name="p">The generated parameter provided by the tool service.</param>
        /// <returns>True if the tool template requires the parameter of the same name. False, if not.</returns>
        public bool IsRequired(ToolParameterBase pGenerate)
        {
            var isRequired = Parameters.Where(p => p.Name == pGenerate.Name).Select(p => p.IsRequired).FirstOrDefault();
            return isRequired; 
        }

        /// <summary>
        /// Runs validation on generated parameter.
        /// </summary>
        /// <param name="par">The generated parameters.</param>
        /// <returns>Null, if no issues. An error message if there was a problem.</returns>
        public virtual string? ValidateGeneratedParameterValues(ToolParameterBase par)
        {
            var isRequired = IsRequired(par);

            if (isRequired && !par.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
                return $"The parameter \"{par.Name}\" on tool named \"{Name}\" is required.";

            return null;
        }
    }

    public static class DaisiToolExtensions
    {
        extension (ToolParameterBase[] parameters) {

            /// <summary>
            /// Gets the parameter from the list by its name.
            /// </summary>
            /// <param name="name">The case insensitive name of the parameter to find</param>
            /// <param name="throwErrors">Throws an exception if the parameter is not found or if the value is null or whitespace for a required param</param>
            /// <returns>The <see cref="ToolParameter"/> with the name given, if it exists. Null, if not.</returns>            
            public ToolParameterBase? GetParameter(string name, bool throwErrors = true)
            {
                var p = parameters.FirstOrDefault(p=>p.Name.ToLower() == name.ToLower());

                if (throwErrors)
                {
                    if (p is null || (p.IsRequired && (!p.Values.Any() || string.IsNullOrWhiteSpace(p.Values.FirstOrDefault()))))
                    {
                        throw new NullReferenceException("Required parameter not found or is not set.");
                    }
                }

                return p;
            }
        }
    }
}
