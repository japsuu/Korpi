#if DEBUG

namespace Korpi.Client.Configuration;

public class DebugModeConfig
{
    public readonly bool IsPhotoModeEnabled;
    public readonly string PhotoModeScreenshotPath;

    public bool RenderWireframe;
    public bool RenderChunkBorders;
    public bool RenderRegionBorders;
    public bool RenderChunkMeshState;
    public bool RenderSkybox = true;
    public bool RenderCrosshair = true;
    public bool RenderRaycastPath;
    public bool RenderRaycastHit;
    public bool RenderRaycastHitBlock = true;
    public bool EnableAmbientOcclusion = true;
    public bool DoFrustumCulling = true;
    public bool OnlyPlayerFrustumCulling;


    public DebugModeConfig(bool isPhotoModeEnabled, string photoModeScreenshotPath)
    {
        IsPhotoModeEnabled = isPhotoModeEnabled;
        PhotoModeScreenshotPath = photoModeScreenshotPath;
    }
}
#endif