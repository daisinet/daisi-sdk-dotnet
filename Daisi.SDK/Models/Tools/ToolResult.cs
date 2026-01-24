using Daisi.Protos.V1;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daisi.SDK.Models.Tools
{
    public class ToolResult
    {
        /// <summary>
        /// Gets or sets whether the tool successfully executed.
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Gets or sets the error message if <see cref="Success"/> is false.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The resulting output of the tool's work.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets a message that is to accompany the <see cref="Output"/> to give more clarity, if necessary.
        /// </summary>
        public string? OutputMessage { get; set;  }

        /// <summary>
        /// Gets or sets the format of the <see cref="Output"/> property.
        /// </summary>
        public InferenceOutputFormats OutputFormat { get; set; } = InferenceOutputFormats.PlainText;

    }
}
