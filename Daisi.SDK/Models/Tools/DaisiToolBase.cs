using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models.Tools
{
    public abstract class DaisiToolBase
    {
        
    }

    public static class DaisiToolExtensions
    {
        extension (ToolParameter[] parameters) {

            /// <summary>
            /// Gets the parameter from the list by its name.
            /// </summary>
            /// <param name="name">The case insensitive name of the parameter to find</param>
            /// <param name="throwErrors">Throws an exception if the parameter is not found or if the value is null or whitespace for a required param</param>
            /// <returns>The <see cref="ToolParameter"/> with the name given, if it exists. Null, if not.</returns>            
            public ToolParameter? GetParameter(string name, bool throwErrors = true)
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
