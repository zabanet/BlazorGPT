﻿namespace BlazorGPT.Data.Model;

public class Script
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string? UserId { get; set; }

    public string Name { get; set; } = null!;

    public string SystemMessage { get; set; } = "You are a helpful assistant";

    public List<ScriptStep> Steps { get; set; } = new();
}