﻿namespace ShellShockers.Core.Utilities.Interfaces;

public interface ISerializer
{
	string? Serialize<T>(T message) where T : class;
	T? Deserialize<T>(string message) where T : class;
}