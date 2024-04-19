using Korpi.Client.Utils;
using KorpiEngine.Core;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Skyboxes;

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
    })
    {
    }


    protected override Vector3 Position => GameTime.SunDirection * 800; // WARN: The proper way would be to use a orthographic projection matrix
    protected override float Scale => 10f;
    protected override float RotationX => 0.1f * (float)Time.TotalTime;
    protected override float RotationY => 2f * (float)Time.TotalTime;
}