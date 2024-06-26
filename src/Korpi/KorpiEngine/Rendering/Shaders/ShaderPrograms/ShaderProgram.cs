﻿using System.Diagnostics;
using System.Reflection;
using KorpiEngine.Core.Logging;
using KorpiEngine.Rendering.Exceptions;
using KorpiEngine.Rendering.Shaders.Variables;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Rendering.Shaders.ShaderPrograms;

/// <summary>
/// Represents a shader shaderProgram object.
/// </summary>
public class ShaderProgram : GLObject
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ShaderProgram));

    /// <summary>
    /// The name of this shader shaderProgram.
    /// </summary>
    public string Name => GetType().Name;

    private List<ProgramVariable> _variables = null!;


    /// <summary>
    /// Initializes a new shaderProgram object.
    /// </summary>
    protected ShaderProgram() : base(GL.CreateProgram())
    {
        Logger.InfoFormat("Creating shader shaderProgram: {0}", Name);
        InitializeShaderVariables();
    }


    protected override void Dispose(bool manual)
    {
        if (!manual) return;
        GL.DeleteProgram(Handle);
    }


    private void InitializeShaderVariables()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        _variables = new List<ProgramVariable>();
        foreach (PropertyInfo property in GetType().GetProperties(flags).Where(i => typeof(ProgramVariable).IsAssignableFrom(i.PropertyType)))
        {
            ProgramVariable instance = (ProgramVariable)Activator.CreateInstance(property.PropertyType, true)!;
            instance.Initialize(this, property);
            property.SetValue(this, instance, null);
            _variables.Add(instance);
        }
    }


    /// <summary>
    /// Activate the shaderProgram.
    /// </summary>
    public void Use()
    {
        GL.UseProgram(Handle);
    }


    /// <summary>
    /// Attach shader object.
    /// </summary>
    /// <param name="shader">Specifies the shader object to attach.</param>
    public void Attach(Shader shader)
    {
        GL.AttachShader(Handle, shader.Handle);
    }


    /// <summary>
    /// Detach shader object.
    /// </summary>
    /// <param name="shader">Specifies the shader object to detach.</param>
    public void Detach(Shader shader)
    {
        GL.DetachShader(Handle, shader.Handle);
    }


    /// <summary>
    /// Link the shaderProgram.
    /// </summary>
    public virtual void Link()
    {
        Logger.DebugFormat("Linking shaderProgram: {0}", Name);
        GL.LinkProgram(Handle);
        CheckLinkStatus();

        // call OnLink() on all ShaderVariables
        _variables.ForEach(v => v.OnLink());
    }


    /// <summary>
    /// Assert that no link error occured.
    /// </summary>
    private void CheckLinkStatus()
    {
        // check link status
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
        Logger.DebugFormat("Link status: {0}", linkStatus);

        // check shaderProgram info log
        string? info = GL.GetProgramInfoLog(Handle);
        if (!string.IsNullOrEmpty(info)) Logger?.InfoFormat("Link log:\n{0}", info);

        // log message and throw exception on link error
        if (linkStatus == 1) return;
        string msg = $"Error linking shaderProgram: {Name}";
        Logger?.Error(msg);
        throw new ProgramLinkException(msg, info);
    }


    /// <summary>
    /// Throws an <see cref="ObjectNotBoundException"/> if this shaderProgram is not the currently active one.
    /// </summary>
    [Conditional("DEBUG")]
    public void AssertActive()
    {
        GL.GetInteger(GetPName.CurrentProgram, out int activeHandle);
        if (activeHandle != Handle) throw new ObjectNotBoundException("ShaderProgram object is not currently active.");
    }
}