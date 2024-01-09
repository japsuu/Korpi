using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Skybox;

public class Sun : CelestialBody
{
    public Sun(bool enableRotation) : base(enableRotation, new[]
    {
        IoUtils.GetSkyboxSunTexturePath("sun.png"),
        IoUtils.GetSkyboxSunTexturePath("sun.png"),
        IoUtils.GetSkyboxSunTexturePath("sun.png"),
        IoUtils.GetSkyboxSunTexturePath("sun.png"),
        IoUtils.GetSkyboxSunTexturePath("sun.png"),
        IoUtils.GetSkyboxSunTexturePath("sun.png"),
    }, 4)
    {
    }


    protected override Vector3 Position => GameTime.SunDirection * 800; // WARN: The proper way would be to use a orthographic projection matrix
    protected override float Scale => 10f;
}