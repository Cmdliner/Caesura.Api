using System.Text;
using System.Text.Json;

namespace Caesura.Api.Infrastructure;

public sealed class SnakeCaseNamingPolicy: JsonNamingPolicy
{
    public static readonly SnakeCaseNamingPolicy Instance = new();

    public override string ConvertName(string name)
    {
        var result = new StringBuilder();

        foreach (var c in name)
        {
            if(char.IsUpper(c) && result.Length > 0) result.Append("_");   
            result.Append(char.ToLower(c));
        }
        return result.ToString();
    }
}