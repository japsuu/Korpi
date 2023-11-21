using BlockEngine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockEngine.Framework.Rendering.Shaders;

public class Shader : IDisposable
{
    public int Handle { get; private set; }
    
    private bool _isDisposed;
    

    public Shader(string vertexPath, string fragmentPath)
    {
        string vertexShaderSource = File.ReadAllText(vertexPath);
        string fragmentShaderSource = File.ReadAllText(fragmentPath);
        
        (int vertexShader, int fragmentShader) = GenerateShader(vertexShaderSource, fragmentShaderSource);

        if (!TryCompileShader(vertexShader, fragmentShader))
        {
            throw new Exception($"Could not compile the shader at vert: '{vertexPath}' and frag: '{fragmentPath}'.");
        }
        
        CreateAndLinkProgram(vertexShader, fragmentShader);
        
        Cleanup(vertexShader, fragmentShader);
    }
    
    
    public void Use()
    {
        GL.UseProgram(Handle);
    }
    
    
    public static int GetShaderAttribLocation(int handle, string attribName)
    {
        return GL.GetAttribLocation(handle, attribName);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetBool(string name, bool value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, value ? 1 : 0);
    }


    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, value);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetFloat(string name, float value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, value);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetVector2(string name, Vector2 vector)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform2(location, vector);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetVector3(string name, Vector3 vector)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(location, vector);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetVector4(string name, Vector4 vector)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform4(location, vector);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetMatrix2(string name, Matrix2 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix2(location, true, ref matrix);
    }
    
    
    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetMatrix3(string name, Matrix3 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix3(location, true, ref matrix);
    }


    /// <summary>
    /// WARN: If calling this method repeatedly, consider caching the location and using the overload.
    /// </summary>
    public void SetMatrix4(string name, Matrix4 matrix)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location, true, ref matrix);
    }
    
    
    public static void SetBool(int location, bool value) => GL.Uniform1(location, value ? 1 : 0);
    public static void SetInt(int location, int value) => GL.Uniform1(location, value);
    public static void SetFloat(int location, float value) => GL.Uniform1(location, value);
    public static void SetVector2(int location, Vector2 vector) => GL.Uniform2(location, vector);
    public static void SetVector3(int location, Vector3 vector) => GL.Uniform3(location, vector);
    public static void SetVector4(int location, Vector4 vector) => GL.Uniform4(location, vector);
    public static void SetMatrix2(int location, Matrix2 matrix) => GL.UniformMatrix2(location, true, ref matrix);
    public static void SetMatrix3(int location, Matrix3 matrix) => GL.UniformMatrix3(location, true, ref matrix);
    public static void SetMatrix4(int location, Matrix4 matrix) => GL.UniformMatrix4(location, true, ref matrix);

    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    private static (int vertexShader, int fragmentShader) GenerateShader(string vertexShaderSource, string fragmentShaderSource)
    {
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        
        return (vertexShader, fragmentShader);
    }


    private static bool TryCompileShader(int vertexShader, int fragmentShader)
    {
        bool wasSuccessful = true;
        GL.CompileShader(vertexShader);

        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(vertexShader);
            Logger.LogError($"Could not compile vertex shader: {infoLog}.");
            wasSuccessful = false;
        }

        GL.CompileShader(fragmentShader);

        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(fragmentShader);
            Logger.LogError($"Could not compile fragment shader: {infoLog}.");
            wasSuccessful = false;
        }
        
        return wasSuccessful;
    }


    private void CreateAndLinkProgram(int vertexShader, int fragmentShader)
    {
        Handle = GL.CreateProgram();

        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);

        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Logger.LogError($"Could not link shader program: {infoLog}.");
        }
    }


    private void Cleanup(int vertexShader, int fragmentShader)
    {
        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        
        GL.DeleteProgram(Handle);

        _isDisposed = true;
    }

    
    ~Shader()
    {
        if (_isDisposed == false)
        {
            Logger.LogWarning("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }
}