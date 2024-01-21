using Korpi.Client.Utils;
using Korpi.Client.Window;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Skyboxes;

public class Moon : CelestialBody
{
    public Moon(bool enableRotation) : base(enableRotation, new[]
    {
        IoUtils.GetSkyboxSunTexturePath("moon.png"),
        IoUtils.GetSkyboxSunTexturePath("moon.png"),
        IoUtils.GetSkyboxSunTexturePath("moon.png"),
        IoUtils.GetSkyboxSunTexturePath("moon.png"),
        IoUtils.GetSkyboxSunTexturePath("moon.png"),
        IoUtils.GetSkyboxSunTexturePath("moon.png"),
    })
    {
    }


    protected override Vector3 Position => -GameTime.SunDirection * 800; // WARN: The proper way would be to use a orthographic projection matrix
    protected override float Scale => 40f;
    protected override float RotationX => 1f * (float)GameTime.TotalTime;
    protected override float RotationY => 0.01f * (float)GameTime.TotalTime;
}