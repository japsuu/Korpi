namespace Korpi.Client.Configuration;

/// <summary>
/// In-memory rendering configuration.
/// Not saved to disk.
/// </summary>
public class RenderingConfig
{
    public bool RenderChunkBorders;
    public bool RenderColumnBorders;

#if DEBUG
    public class DebugSettings
    {
        public bool RenderWireframe;
        public bool RenderChunkMeshState;
        public bool RenderSkybox = true;
        public bool RenderRaycastPath;
        public bool RenderRaycastHit;
        public bool RenderCrosshair = true;
        public bool HighlightTargetedBlock = true;
        public bool EnableAmbientOcclusion = true;
        public bool DoFrustumCulling = true;
        public bool OnlyPlayerFrustumCulling;
    }

    /// <summary>
    /// Debug rendering settings.
    /// Only available in debug builds.
    /// </summary>
    public readonly DebugSettings Debug = new();
#endif
}