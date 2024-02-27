#if DEBUG
using ImGuiNET;
using Korpi.Client.Mathematics.Noise;
using Korpi.Client.UI.Windows.Variables;
using Korpi.Client.World;
using KorpiEngine.Core.UI.ImGui;

namespace Korpi.Client.UI.Windows;

public class GenerationWindow : ImGuiWindow
{
    public override string Title => "Generation Settings";

    private static readonly Dictionary<string, FastNoiseLite> NoiseList = new();
    private static readonly List<IEditorVariable> VariableList = new();


    public GenerationWindow(bool autoRegister = true) : base(autoRegister)
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    public static void RegisterNoise(string name, FastNoiseLite noise)
    {
        NoiseList.Add(name, noise);
    }


    public static void RegisterVariable(IEditorVariable variable)
    {
        VariableList.Add(variable);
    }


    protected override void DrawContent()
    {
        ImGui.Text("Noise Instances");
        ImGui.Separator();
        foreach (KeyValuePair<string, FastNoiseLite> noise in NoiseList)
            DrawNoiseEditor(noise.Key, noise.Value);

        ImGui.Text("Variables");
        foreach (IEditorVariable variable in VariableList)
            variable.Draw();
        
        ImGui.Separator();
        if (ImGui.Button("Regenerate all chunks"))
            GameWorld.RegenerateAllChunks();
    }


    private void DrawNoiseEditor(string name, FastNoiseLite noise)
    {
        ImGui.Text($"-- {name} --");
        ImGui.Separator();

        ImGui.Text("Noise Type");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        if (ImGui.BeginCombo($"##noiseType{name}", noise.GetNoiseType().ToString()))
        {
            foreach (FastNoiseLite.NoiseType noiseType in Enum.GetValues<FastNoiseLite.NoiseType>())
                if (ImGui.Selectable(noiseType.ToString(), noise.GetNoiseType().ToString() == noiseType.ToString()))
                    noise.SetNoiseType(noiseType);

            ImGui.EndCombo();
        }

        ImGui.Text("Fractal Type");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        if (ImGui.BeginCombo($"##fractalType{name}", noise.GetFractalType().ToString()))
        {
            foreach (FastNoiseLite.FractalType fractalType in Enum.GetValues<FastNoiseLite.FractalType>())
                if (ImGui.Selectable(fractalType.ToString(), noise.GetFractalType().ToString() == fractalType.ToString()))
                    noise.SetFractalType(fractalType);

            ImGui.EndCombo();
        }

        ImGui.Text("Fractal Octaves");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        int octaves = noise.FractalOctaves;
        if (ImGui.DragInt($"##fractalOctaves{name}", ref octaves, 1, 1, 10))
            noise.SetFractalOctaves(octaves);

        ImGui.Text("Fractal Lacunarity");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        float lacunarity = noise.FractalLacunarity;
        if (ImGui.DragFloat($"##fractalLacunarity{name}", ref lacunarity, 0.1f, 0.1f, 10))
            noise.SetFractalLacunarity(lacunarity);

        ImGui.Text("Fractal Gain");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        float gain = noise.FractalGain;
        if (ImGui.DragFloat($"##fractalGain{name}", ref gain, 0.1f, 0.1f, 10))
            noise.SetFractalGain(gain);

        ImGui.Text("Frequency");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        float frequency = noise.Frequency;
        if (ImGui.DragFloat($"##frequency{name}", ref frequency, 0.001f, 0.001f, 10))
            noise.SetFrequency(frequency);

        ImGui.Text("Seed");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        int seed = noise.Seed;
        if (ImGui.DragInt($"##seed{name}", ref seed, 1, 1, 100000))
            noise.SetSeed(seed);

        ImGui.Separator();
    }
}
#endif