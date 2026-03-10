using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AIRelief.Tests.TestInfrastructure;

internal static class JsonResultAssertions
{
    public static JsonElement ToJsonElement(JsonResult result)
    {
        var json = JsonSerializer.Serialize(result.Value);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}