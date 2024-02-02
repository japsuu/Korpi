#if DEBUG

namespace Korpi.Client.Configuration;

public class DebugModeConfig
{
    public bool RenderWireframe;
    public bool RenderChunkBorders;
    public bool RenderColumnBorders;
    public bool RenderChunkMeshState;
    public bool RenderSkybox = true;
    public bool RenderCrosshair = true;
    public bool RenderRaycastPath;
    public bool RenderRaycastHit;
    public bool RenderRaycastHitBlock = true;
    public bool EnableAmbientOcclusion = true;
    public bool DoFrustumCulling = true;
    public bool OnlyPlayerFrustumCulling;
}
#endif