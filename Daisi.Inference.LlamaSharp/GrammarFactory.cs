using Llama.Grammar.Helper;
using Llama.Grammar.Service;

namespace Daisi.Inference.LlamaSharp;

/// <summary>
/// Converts JSON schemas to GBNF grammar text using Llama.Grammar.
/// </summary>
public static class GrammarFactory
{
    /// <summary>
    /// Convert a JSON schema string to GBNF grammar text.
    /// </summary>
    public static string JsonSchemaToGbnf(string jsonSchema)
    {
        IGbnfGrammar g = new GbnfGrammar();
        return g.ConvertJsonSchemaToGbnf(jsonSchema);
    }

    /// <summary>
    /// Convert a JSON schema string to GBNF grammar text, then wrap it in a GBNF root rule.
    /// Use this when you already have a JSON schema and want a complete GBNF grammar.
    /// </summary>
    public static string JsonSchemaToGbnfWithRoot(string jsonSchema, string rootRule = "root")
    {
        IGbnfGrammar g = new GbnfGrammar();
        var gbnf = g.ConvertJsonSchemaToGbnf(jsonSchema);
        return gbnf;
    }
}
