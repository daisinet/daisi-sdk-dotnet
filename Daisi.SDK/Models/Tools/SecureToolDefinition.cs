using Daisi.Protos.V1;

namespace Daisi.SDK.Models.Tools
{
    /// <summary>
    /// Defines a secure tool that executes remotely on a provider's server.
    /// Consumer hosts use this metadata to present the tool to the inference engine
    /// and call the provider directly via HTTP.
    /// </summary>
    public class SecureToolDefinition
    {
        /// <summary>
        /// The marketplace item ID that this secure tool belongs to.
        /// </summary>
        public string MarketplaceItemId { get; set; } = string.Empty;

        /// <summary>
        /// Unique tool identifier used by the inference engine.
        /// </summary>
        public string ToolId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the tool.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Instructions provided to the inference engine describing when and how to use this tool.
        /// </summary>
        public string UseInstructions { get; set; } = string.Empty;

        /// <summary>
        /// The parameters that this tool accepts.
        /// </summary>
        public ToolParameter[] Parameters { get; set; } = [];

        /// <summary>
        /// The tool group for filtering (e.g. "InformationTools", "IntegrationTools").
        /// </summary>
        public string ToolGroup { get; set; } = "InformationTools";

        /// <summary>
        /// Opaque identifier shared with the provider for this installation.
        /// Used instead of AccountId in all provider-facing communication.
        /// </summary>
        public string InstallId { get; set; } = string.Empty;

        /// <summary>
        /// The provider's base URL for direct HTTP calls (execute, configure).
        /// </summary>
        public string EndpointUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Extension methods for converting between proto and SDK secure tool models.
    /// </summary>
    public static class SecureToolDefinitionExtensions
    {
        /// <summary>
        /// Converts a proto <see cref="SecureToolDefinitionInfo"/> to an SDK <see cref="SecureToolDefinition"/>.
        /// </summary>
        public static SecureToolDefinition ToSdkModel(this SecureToolDefinitionInfo proto)
        {
            return new SecureToolDefinition
            {
                MarketplaceItemId = proto.MarketplaceItemId,
                ToolId = proto.ToolId,
                Name = proto.Name,
                UseInstructions = proto.UseInstructions,
                ToolGroup = proto.ToolGroup,
                InstallId = proto.InstallId,
                EndpointUrl = proto.EndpointUrl,
                Parameters = proto.Parameters.Select(p => new ToolParameter
                {
                    Name = p.Name,
                    Description = p.Description,
                    IsRequired = p.IsRequired
                }).ToArray()
            };
        }
    }
}
