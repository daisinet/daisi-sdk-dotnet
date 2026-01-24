using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models.Tools
{
    /// <summary>
    /// The Tool execution service gets this execution context from the tool class
    /// which provides the necessary information to run the task on threads outside of the
    /// main thread, as well as information to show the consumer, as needed.
    /// </summary>
    public class ToolExecutionContext
    {
        /// <summary>
        /// This is thet message that is returned to the consumer when execution begins.
        /// </summary>
        public required string ExecutionMessage { get; set;  }

        /// <summary>
        /// This is the threaded <see cref="Task"/> that is executed by the tool service.
        /// </summary>
        public required Task<ToolResult> ExecutionTask { get; set;  }
    }
}
