namespace Korpi.Client.Registries;

public interface IHasId
{
    public ushort Id { get; }
    
    public void AssignId(ushort id);
}