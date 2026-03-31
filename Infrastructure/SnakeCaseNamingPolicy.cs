using System.Text;
using System.Text.Json;

namespace Caesura.Api.Helpers;

public sealed class SnakeCaseNamingPolicy: JsonNamingPolicy
{
    public override string ConvertName(string name)
    {
        var result = new StringBuilder();

        foreach (var c in name)
        {
            if(char.IsUpper(c) && result.Length > 0) result.Append("_");   
            result.Append(c);
        }
        return result.ToString();
    }
}