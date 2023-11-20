using System.Diagnostics;
using BlockEngine.Framework.Meshing;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;

namespace BlockEngine.Framework.Rendering.ImGuiWindows;

public class RenderingWindow : ImGuiWindow
{
    public static class RenderingStats
    {
        public static int ChunksInMeshingQueue;
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
    private bool _isWireframeEnabled;


    public RenderingWindow()
    {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }


    protected override void UpdateContent()
    {
        if (ImGui.Checkbox("Wireframe rendering", ref _isWireframeEnabled))
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, _isWireframeEnabled ? PolygonMode.Line : PolygonMode.Fill);
        }
        
        ImGui.Text($"Cached chunk meshes = {ChunkMeshStorage.GeneratedMeshCount}");
        ImGui.Text($"Chunks in meshing queue = {RenderingStats.ChunksInMeshingQueue}");
        ImGui.Text($"Meshing time = {RenderingStats.MeshingTime:F1}ms");
    }
}