using Daisi.SDK.Models.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Interfaces.Tools
{
    /// <summary>
    /// The common interface for tools to be used in the DAISI host network.
    /// </summary>
    public interface IDaisiTool
    {
        /// <summary>
        /// Gets and sets the name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets and sets the description of the tool.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets an array of <see cref="ToolParameter" /> available for the execution of the tool.
        /// </summary>
        ToolParameter[] Parameters { get; }

        /// <summary>
        /// Calls on the tool to execute the requested work. 
        /// </summary>
        /// <param name="toolContext">The context by which the tool is being executed.</param>
        /// <param name="parameters">
        /// The parameters for the tool that will affect the output. 
        /// See <see cref="Parameters"/> for a list of expected parameters.
        /// </param>
        /// <returns>A <see cref="ToolResult"/> that contains the output of the Tool.</returns>
        ToolExecutionContext GetExecutionContext(IToolContext toolContext, CancellationToken cancellation, params ToolParameter[] parameters);

    }

    
}
