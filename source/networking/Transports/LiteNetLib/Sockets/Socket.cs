﻿using System.Collections.Concurrent;
using Korpi.Networking.EventArgs;
using Korpi.Networking.Utility;
using LiteNetLib;

namespace Korpi.Networking.Transports.LiteNetLib.Sockets;

public abstract class CommonSocket
{

    /// <summary>
    /// Current ConnectionState.
    /// </summary>
    private LocalConnectionState _connectionState = LocalConnectionState.Stopped;


    /// <summary>
    /// Returns the current ConnectionState.
    /// </summary>
    /// <returns></returns>
    internal LocalConnectionState GetConnectionState()
    {
        return _connectionState;
    }


    /// <summary>
    /// Sets a new connection state.
    /// </summary>
    protected void SetConnectionState(LocalConnectionState connectionState, bool asServer)
    {
        //If state hasn't changed.
        if (connectionState == _connectionState)
            return;

        _connectionState = connectionState;
        if (asServer)
            Transport.HandleServerConnectionState(new ServerConnectionStateArgs(connectionState));
        else
            Transport.HandleClientConnectionState(new ClientConnectionStateArgs(connectionState));
    }

    /// <summary>
    /// Transport controlling this socket.
    /// </summary>
    protected Transport Transport;

    /// <summary>
    /// NetManager for this socket.
    /// </summary>
    protected NetManager? NetManager;


    /// <summary>
    /// Sends data to connectionId.
    /// </summary>
    internal void Send(ref Queue<Packet> queue, byte channelId, ArraySegment<byte> segment, int connectionId, int mtu)
    {
        if (GetConnectionState() != LocalConnectionState.Started)
            return;

        //ConnectionId isn't used from client to server.
        Packet outgoing = new Packet(connectionId, segment, channelId, mtu);
        queue.Enqueue(outgoing);
    }


    /// <summary>
    /// Updates the timeout for NetManager.
    /// </summary>
    protected void UpdateTimeout(NetManager? netManager, int timeout)
    {
        if (netManager == null)
            return;

        timeout = timeout == 0 ? int.MaxValue : Math.Min(int.MaxValue, timeout * 1000);
        netManager.DisconnectTimeout = timeout;
    }


    /// <summary>
    /// Clears a ConcurrentQueue of any type.
    /// </summary>
    internal void ClearGenericQueue<T>(ref ConcurrentQueue<T> queue)
    {
        while (queue.TryDequeue(out _))
        {
        }
    }


    /// <summary>
    /// Clears a queue using Packet type.
    /// </summary>
    /// <param name="queue"></param>
    internal void ClearPacketQueue(ref ConcurrentQueue<Packet> queue)
    {
        while (queue.TryDequeue(out Packet p))
            p.Dispose();
    }


    /// <summary>
    /// Clears a queue using Packet type.
    /// </summary>
    /// <param name="queue"></param>
    internal void ClearPacketQueue(ref Queue<Packet> queue)
    {
        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            Packet p = queue.Dequeue();
            p.Dispose();
        }
    }


    /// <summary>
    /// Called when data is received.
    /// </summary>
    internal virtual void Listener_NetworkReceiveEvent(ConcurrentQueue<Packet> queue, NetPeer fromPeer, NetPacketReader reader, DeliveryMethod deliveryMethod,
        int mtu)
    {
        //Set buffer.
        int dataLen = reader.AvailableBytes;

        //Prefer to max out returned array to mtu to reduce chance of resizing.
        int arraySize = Math.Max(dataLen, mtu);
        byte[] data = ByteArrayPool.Retrieve(arraySize);
        reader.GetBytes(data, dataLen);

        //Id.
        int id = fromPeer.Id;

        //Channel.
        byte channel = deliveryMethod == DeliveryMethod.Unreliable ? (byte)Channel.Unreliable : (byte)Channel.Reliable;

        //Add to packets.
        Packet packet = new Packet(id, data, dataLen, channel);
        queue.Enqueue(packet);

        //Recycle reader.
        reader.Recycle();
    }


    internal void PollSocket(NetManager? nm)
    {
        nm?.PollEvents();
    }


    /// <summary>
    /// Returns the port from the socket if active, otherwise returns null.
    /// </summary>
    /// <returns></returns>
    internal ushort? GetPort()
    {
        if (!NetManager.IsRunning)
            return null;

        int port = NetManager.LocalPort;
        if (port < 0)
            port = 0;
        else if (port > ushort.MaxValue)
            port = ushort.MaxValue;

        return (ushort)port;
    }
}