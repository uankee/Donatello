using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Donatello.Extensions;

public static class TempDataExtensions
{
    public static void Set<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonSerializer.Serialize(value);
    }

    public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
    {
        var item = tempData[key];
        return item == null ? null : JsonSerializer.Deserialize<T>((string)item);
    }
}
