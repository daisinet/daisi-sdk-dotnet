using Daisi.SDK.Models.Tools;
using System.Text.Json;

namespace Daisi.SDK.Models.Tools
{
    public class ToolToUse
    {
        /// <summary>
        /// Gets and sets the unique system ID for the tool to use.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets and sets the list of parameters to pass to the tool.
        /// </summary>
        public ToolParameterBase[] Parameters { get; set; }
    }

}
