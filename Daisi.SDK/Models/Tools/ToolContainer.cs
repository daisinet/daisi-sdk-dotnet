using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Daisi.SDK.Models.Tools
{

    public class ToolContainer
    {
        public List<ToolToUse> ToolsToUse { get; set; } = new();

        public static ToolContainer ParseJson(string toolString)
        {
            return JsonSerializer.Deserialize<ToolContainer>(toolString, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true, });
        }
    }
}
