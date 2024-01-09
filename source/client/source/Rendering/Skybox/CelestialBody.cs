﻿using Korpi.Client.Rendering.Shaders;
using Korpi.Client.Rendering.Textures;
using Korpi.Client.Window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Korpi.Client.Rendering.Skybox;

/// <summary>
/// Three-dimensional cube, that moves across the skybox.
/// Rotates around the X and Y axes.
/// </summary>
public abstract class CelestialBody : SkyboxFeature, IDisposable
{
    private readonly float[] _vertices =
    {
        -1.0f,  1.0f, -1.0f,
        -1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f, -1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,

        -1.0f, -1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f, -1.0f,  1.0f,
        -1.0f, -1.0f,  1.0f,

        -1.0f,  1.0f, -1.0f,
        1.0f,  1.0f, -1.0f,
        1.0f,  1.0f,  1.0f,
        1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f,  1.0f,
        -1.0f,  1.0f, -1.0f,

        -1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f, -1.0f,
        1.0f, -1.0f, -1.0f,
        -1.0f, -1.0f,  1.0f,
        1.0f, -1.0f,  1.0f
    };
    
    private readonly CubemapTexture _texture;
    private readonly int _vao;
    private readonly bool _enableRotation;
    private readonly int _textureUnit;

    protected abstract Vector3 Position { get; }
    protected abstract float Scale { get; }
    protected abstract float RotationX { get; }
    protected abstract float RotationY { get; }


    protected CelestialBody(bool enableRotation, string[] texturePaths, int textureUnit)
    {
        _texture = CubemapTexture.LoadFromFile(texturePaths, "Skybox Decor (celestial body)");
        
        _enableRotation = enableRotation;
        _textureUnit = textureUnit;

        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);
        
        int vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        
        ShaderManager.CelestialBodyShader.Use();
        _texture.BindStatic(TextureUnit.Texture0 + textureUnit);
    }
    
    
    public void Render()
    {
        ShaderManager.CelestialBodyShader.Use();
        
        Matrix4 modelMatrix = Matrix4.Identity;
        if (_enableRotation)
        {
            modelMatrix *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(RotationX));
            modelMatrix *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(RotationY));
        }

        modelMatrix *= Matrix4.CreateScale(Scale);
        modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, Position.Z);
        
        Matrix4 skyboxViewMatrix = new(new Matrix3(ShaderManager.ViewMatrix)); // Remove translation from the view matrix
        ShaderManager.CelestialBodyShader.SetMatrix4("model", modelMatrix);
        ShaderManager.CelestialBodyShader.SetMatrix4("view", skyboxViewMatrix);
        ShaderManager.CelestialBodyShader.SetInt("cubeTexture", _textureUnit);
        
        GL.BindVertexArray(_vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        GL.BindVertexArray(0);
    }


    private void ReleaseUnmanagedResources()
    {
        _texture.Dispose();
        GL.DeleteVertexArray(_vao);
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    ~CelestialBody()
    {
        ReleaseUnmanagedResources();
    }
}