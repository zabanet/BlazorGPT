﻿namespace BlazorGPT.Shared.PluginSelector;

internal class PluginFormModel
{
    public List<PluginSelection> SelectedPlugins { get; set; } = new();
    public List<PluginSelection> Plugins { get; set; } = new();

}