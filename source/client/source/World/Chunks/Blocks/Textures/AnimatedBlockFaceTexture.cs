namespace Korpi.Client.World.Chunks.Blocks.Textures;

/// <summary>
/// Animated texture of one side of a <see cref="Block"/>.
/// </summary>
public class AnimatedBlockFaceTexture : IBlockFaceTexture
{
    private readonly ushort[] _textureIndices;
    private readonly int _textureCount;
    private readonly float _animationIntervalSeconds;
    private int _textureIndex;
    private float _secondsSinceLastAnimation;
    
    
    public AnimatedBlockFaceTexture(ushort[] textureIndices, float animationIntervalSeconds)
    {
        _textureIndices = textureIndices;
        _textureCount = textureIndices.Length;
        _animationIntervalSeconds = animationIntervalSeconds;
    }
    
    
    public void Update(float deltaTime)
    {
        if (_secondsSinceLastAnimation >= _animationIntervalSeconds)
        {
            _textureIndex = (_textureIndex + 1) % _textureCount;
            _secondsSinceLastAnimation = 0;
        }
        _secondsSinceLastAnimation += deltaTime;
    }


    public ushort GetId()
    {
        return _textureIndices[_textureIndex];
    }
}