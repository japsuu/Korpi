#if DEBUG

namespace BlockEngine.Client.Framework.Configuration;

public class DebugModeConfig
{
    public readonly bool IsPhotoModeEnabled;
    public readonly string PhotoModeScreenshotPath;

    public bool RenderWireframe;
    public bool RenderChunkBorders;
    public bool RenderChunkColumnBorders;
    public bool RenderChunkMeshState;
    public bool RenderSkybox = true;
    public bool RenderRaycastPath;
    public bool RenderRaycastHit;
    public bool RenderRaycastHitBlock;
    public bool EnableAmbientOcclusion = true;


    public DebugModeConfig(bool isPhotoModeEnabled, string photoModeScreenshotPath)
    {
        IsPhotoModeEnabled = isPhotoModeEnabled;
        PhotoModeScreenshotPath = photoModeScreenshotPath;
    }
}
#endif