namespace BlockEngine.Client.Framework.Blocks.Textures;

/// <summary>
/// Animated texture of one side of a <see cref="Block"/>.
/// </summary>
public class AnimatedBlockFaceTexture : BlockFaceTexture
{
    private readonly ushort[] _textureIndices;
    private readonly int _textureCount;
    private readonly float _animationIntervalSeconds;
    private int _textureIndex;
    private float _secondsSinceLastAnimation;
    
    
    public AnimatedBlockFaceTexture(ushort[] textureIndices, float animationIntervalSeconds) : base(textureIndices[0])
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
            TextureIndex = _textureIndices[_textureIndex];
            _secondsSinceLastAnimation = 0;
        }
        _secondsSinceLastAnimation += deltaTime;
    }
}