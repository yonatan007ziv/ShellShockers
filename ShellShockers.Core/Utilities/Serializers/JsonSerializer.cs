using ShellShockers.Core.Utilities.Interfaces;
using System.Text.Json;

namespace ShellShockers.Core.Utilities.Serializers;

public class JsonSerializer : ISerializer
{
	public string? Serialize<T>(T message) where T : class
	{
		try
		{
			return System.Text.Json.JsonSerializer.Serialize(message);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	public T? Deserialize<T>(string message) where T : class
	{
		try
		{
			return System.Text.Json.JsonSerializer.Deserialize<T>(message)
				?? throw new JsonException();
		}
		catch (JsonException)
		{
			return null;
		}
	}
}