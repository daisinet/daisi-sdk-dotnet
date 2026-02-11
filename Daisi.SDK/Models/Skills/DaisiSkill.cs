namespace Daisi.SDK.Models.Skills;

/// <summary>
/// Unified skill representation used by both bot and host.
/// Maps 1:1 with YAML frontmatter fields + markdown body as SystemPromptTemplate.
/// </summary>
public class DaisiSkill
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public List<string> RequiredToolGroups { get; set; } = [];
    public string IconUrl { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public string SystemPromptTemplate { get; set; } = string.Empty;
}
