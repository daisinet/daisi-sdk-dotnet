using Daisi.SDK.Models.Skills;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Daisi.SDK.Skills;

/// <summary>
/// Parses skill files (YAML frontmatter + markdown body) into DaisiSkill models.
/// Shared between bot and host.
/// </summary>
public static class SkillFileLoader
{
    /// <summary>
    /// Parses YAML frontmatter from a markdown string.
    /// Returns the frontmatter and the remaining body.
    /// </summary>
    public static (SkillFrontmatter? Frontmatter, string Body) ParseFrontmatter(string content)
    {
        if (!content.StartsWith("---"))
            return (null, content);

        var endIndex = content.IndexOf("---", 3, StringComparison.Ordinal);
        if (endIndex < 0)
            return (null, content);

        var yamlBlock = content[3..endIndex].Trim();
        var body = content[(endIndex + 3)..];

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var frontmatter = deserializer.Deserialize<SkillFrontmatter>(yamlBlock) ?? new SkillFrontmatter();
            return (frontmatter, body);
        }
        catch
        {
            return (null, content);
        }
    }

    /// <summary>
    /// Loads a markdown file content into a DaisiSkill model.
    /// </summary>
    public static DaisiSkill LoadMarkdown(string content, string id)
    {
        var (frontmatter, body) = ParseFrontmatter(content);

        var skill = new DaisiSkill
        {
            Id = id,
            SystemPromptTemplate = body.Trim()
        };

        if (frontmatter is not null)
        {
            skill.Name = frontmatter.Name;
            skill.Description = frontmatter.Description;
            skill.ShortDescription = frontmatter.ShortDescription;
            skill.Version = frontmatter.Version;
            skill.Author = frontmatter.Author;
            skill.Tags = frontmatter.Tags;
            skill.IconUrl = frontmatter.IconUrl;
            skill.IsRequired = frontmatter.IsRequired;
            skill.RequiredToolGroups = frontmatter.Tools;
        }

        return skill;
    }

    /// <summary>
    /// Serializes a DaisiSkill to YAML frontmatter + markdown body format.
    /// </summary>
    public static string Serialize(DaisiSkill skill)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var frontmatter = new SkillFrontmatter
        {
            Name = skill.Name,
            Description = skill.Description,
            ShortDescription = skill.ShortDescription,
            Version = skill.Version,
            Author = skill.Author,
            Tags = skill.Tags,
            Tools = skill.RequiredToolGroups,
            IconUrl = skill.IconUrl,
            IsRequired = skill.IsRequired
        };

        var yaml = serializer.Serialize(frontmatter);
        return $"---\n{yaml}---\n\n{skill.SystemPromptTemplate}\n";
    }
}
