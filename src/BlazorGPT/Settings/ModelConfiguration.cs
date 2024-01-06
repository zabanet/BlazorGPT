﻿using BlazorGPT.Pipeline;

namespace BlazorGPT.Settings;

public class ModelConfiguration
{
    public ChatModelsProvider Provider { get; set; }
    public string Model { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
}