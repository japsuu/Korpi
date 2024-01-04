#if DEBUG

namespace BlockEngine.Client.Configuration;

public class DebugModeConfig
{
    public readonly bool IsPhotoModeEnabled;
    public readonly string PhotoModeScreenshotPath;

    public bool RenderWireframe;
    public bool RenderChunkBorders;
    public bool RenderRegionBorders;
    public bool RenderChunkMeshState;
    public bool RenderSkybox = true;
    public bool RenderRaycastPath;
    public bool RenderRaycastHit;
    public bool RenderRaycastHitBlock = true;
    public bool EnableAmbientOcclusion = true;
    public ushort SelectedBlockType = 1;


    public DebugModeConfig(bool isPhotoModeEnabled, string photoModeScreenshotPath)
    {
        IsPhotoModeEnabled = isPhotoModeEnabled;
        PhotoModeScreenshotPath = photoModeScreenshotPath;
    }
}
#endif