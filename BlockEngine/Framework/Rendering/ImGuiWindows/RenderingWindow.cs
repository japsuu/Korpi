using System.Diagnostics;
using BlockEngine.Framework.Debugging;
using BlockEngine.Framework.Meshing;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public class RenderingWindow : ImGuiWindow
{
    public static class RenderingStats
    {
        public static ulong ChunksInMeshingQueue;
        public static float MeshingTime { get; private set; }
        
        private static readonly Stopwatch MeshingTimer = new();
        
        
        public static void StartMeshing()
        {
            MeshingTimer.Restart();
        }
        
        
        public static void StopMeshing(int chunksMeshed)
        {
            MeshingTimer.Stop();
            MeshingTime = MeshingTimer.ElapsedMilliseconds / (float)chunksMeshed;
        }
    }
    
    public override string Title => "Rendering Settings";


    public RenderingWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void UpdateContent()
    {
        ImGui.Checkbox("Wireframe rendering", ref DebugSettings.RenderWireframe);

        if (ImGui.Checkbox("Render chunk borders", ref DebugSettings.RenderChunkBorders))
        {
            if (DebugSettings.RenderChunkBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }

        if (ImGui.Checkbox("Render column borders", ref DebugSettings.RenderChunkColumnBorders))
        {
            if (DebugSettings.RenderChunkColumnBorders)
                DebugChunkDrawer.Initialize();
            else
                DebugChunkDrawer.Dispose();
        }
        
        ImGui.Checkbox("Render skybox", ref DebugSettings.RenderSkybox);
        
        ImGui.Text($"Cached chunk meshes = {ChunkRendererStorage.GeneratedRendererCount}");
        ImGui.Text($"Chunks in meshing queue = {RenderingStats.ChunksInMeshingQueue}");
        ImGui.Text($"Meshing time = {RenderingStats.MeshingTime:F1}ms");
    }
}