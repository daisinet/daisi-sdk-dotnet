using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models.Tools
{
    /// <summary>
    /// This class designates the parameters that can be passed to a tool that
    /// determines the output of the tool.
    /// </summary>
    public class ToolParameter : ToolParameterBase
    {


        /// <summary>
        /// Gets and sets a description of the parameter that will give context
        /// to the AI model so that it can send the proper values to get an expected result.
        /// </summary>
        public string Description { get; set;  }




    }

    public class ToolParameterBase
    {
        /// <summary>
        /// Gets and sets the name of the parameter.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The values that define the parameter and can be used in producing
        /// the output as expected.
        /// </summary>
        public string[] Values { get; set; } = [];

        /// <summary>
        /// Gets or sets whether this parameter is required by the tool to complete 
        /// its processing.
        /// </summary>
        public bool IsRequired { get; set; } = true;
    }
}
