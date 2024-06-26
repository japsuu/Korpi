﻿using Korpi.Client.Configuration;
using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Utils;
using KorpiEngine.Core;
using KorpiEngine.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Constants = Korpi.Client.Configuration.Constants;

namespace Korpi.Client.Rendering.Skyboxes;

public class Skybox : IDisposable
{
    private readonly float[] _skyboxVertices = {
        // Z- face
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        // X- face
        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        // X+ face
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,

        // Z+ face
        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        // Y+ face
        -1.0f,  1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        // Y- face
        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f,  1.0f
    };
    
    private readonly Sun _sun;
    private readonly Moon _moon;
    private readonly TextureCubemap _daySkyboxTexture;
    private readonly TextureCubemap _nightSkyboxTexture;
    private readonly int _skyboxVAO;
    private readonly bool _enableStarsRotation;


    public Skybox(bool enableStarsRotation) //TODO: Instead of blending between two skybox textures, only have a single night texture. Handle the day rendering in the shader.
    {
        _enableStarsRotation = enableStarsRotation;
        
        // Generate the VAO and VBO.
        _skyboxVAO = GL.GenVertexArray();
        int skyboxVBO = GL.GenBuffer();
        GL.BindVertexArray(_skyboxVAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, skyboxVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _skyboxVertices.Length * sizeof(float), _skyboxVertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        // Load the skybox textures.
        _daySkyboxTexture = TextureCubemap.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxTexturePath("day_x_neg.png"),
            IoUtils.GetSkyboxTexturePath("day_x_pos.png"),
            IoUtils.GetSkyboxTexturePath("day_y_neg.png"),
            IoUtils.GetSkyboxTexturePath("day_y_pos.png"),
            IoUtils.GetSkyboxTexturePath("day_z_neg.png"),
            IoUtils.GetSkyboxTexturePath("day_z_pos.png"),
        }, "Skybox Day");
        
        // Load the skybox textures.
        _nightSkyboxTexture = TextureCubemap.LoadFromFile(new[]
        {
            IoUtils.GetSkyboxTexturePath("night_x_neg.png"),
            IoUtils.GetSkyboxTexturePath("night_x_pos.png"),
            IoUtils.GetSkyboxTexturePath("night_y_neg.png"),
            IoUtils.GetSkyboxTexturePath("night_y_pos.png"),
            IoUtils.GetSkyboxTexturePath("night_z_neg.png"),
            IoUtils.GetSkyboxTexturePath("night_z_pos.png"),
        }, "Skybox Night");
        
        _sun = new Sun(true);
        _moon = new Moon(true);
    }


    public void Draw()
    {
#if DEBUG
        if (!ClientConfig.Rendering.Debug.RenderSkybox)
            return;
#endif
        // Update the skybox view matrix.
        Matrix4 modelMatrix = Matrix4.Identity;
        if (_enableStarsRotation)
        {
            modelMatrix *= Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_X * Time.TotalTime)) *
                           Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(Constants.SKYBOX_ROTATION_SPEED_Y * Time.TotalTime));
        }
        ShaderManager.SkyboxShader.Use();
        ShaderManager.SkyboxShader.ModelMat.Set(modelMatrix);
        ShaderManager.SkyboxShader.SunDirection.Set(GameTime.SunDirection);
        ShaderManager.SkyboxShader.SkyboxLerpProgress.Set(GameTime.SkyboxLerpProgress);
        _daySkyboxTexture.Bind(TextureUnit.Texture0);
        _nightSkyboxTexture.Bind(TextureUnit.Texture1);
        
        // Draw the skybox.
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);  // Change depth function so depth test passes when values are equal to depth buffer's content
        
        GL.BindVertexArray(_skyboxVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
        
        _sun.Render();
        _moon.Render();
        
        GL.DepthFunc(DepthFunction.Less); // set depth function back to default
    }


    private void ReleaseUnmanagedResources()
    {
        _daySkyboxTexture.Dispose();
        _nightSkyboxTexture.Dispose();
        GL.DeleteVertexArray(_skyboxVAO);
        _sun.Dispose();
        _moon.Dispose();
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~Skybox()
    {
        ReleaseUnmanagedResources();
    }
}