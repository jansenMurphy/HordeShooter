using System.Collections.Generic;

public interface NetworkObject
{
    public void SpawnObject(ushort networkID);
    public ushort GetNetworkID();
    public void DespawnObject();//Just destroy the game object it's attached to.
    public void UpdateObject(GameServer.Packet packet);
    public byte[] SerializeObject(List<PlayerManager> relativeToPlayers);//TODO Vary what gets sent based on latency
                                                                         //TODO Include object if they are in player's view
                                                                         //TODO Include objects if they are other players
                                                                         //TODO Include objects if they are close to the player
                                                                         //TODO Include objects if they are making noise
                                                                         //TODO Include objects if they are attacking the player?
                                                                         //TODO Send number of objects/bytes/packets based on network traffic.
    public void RMI(GameServer.Packet packet);
    public void OnAckOrNack(bool wasAcked);
}
