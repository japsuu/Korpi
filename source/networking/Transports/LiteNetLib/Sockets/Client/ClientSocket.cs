﻿/*using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Korpi.Networking.Connections;
using Korpi.Networking.EventArgs;
using LiteNetLib;
using LiteNetLib.Layers;

namespace Korpi.Networking.Transports.LiteNetLib.Sockets.Client;

public class ClientSocket : CommonSocket
{
    ~ClientSocket()
    {
        StopConnection();
    }


    /// <summary>
    /// Address to bind server to.
    /// </summary>
    private string _address = string.Empty;

    /// <summary>
    /// Port used by server.
    /// </summary>
    private ushort _port;

    /// <summary>
    /// MTU sizes for each channel.
    /// </summary>
    private int _mtu;

    /// <summary>
    /// Changes to the sockets local connection state.
    /// </summary>
    private ConcurrentQueue<LocalConnectionState> _localConnectionStates = new();

    /// <summary>
    /// Inbound messages which need to be handled.
    /// </summary>
    private ConcurrentQueue<Packet> _incoming = new();

    /// <summary>
    /// Outbound messages which need to be handled.
    /// </summary>
    private Queue<Packet> _outgoing = new();

    /// <summary>
    /// How long in seconds until client times from server.
    /// </summary>
    private int _timeout;

    /// <summary>
    /// PacketLayer to use with LiteNetLib.
    /// </summary>
    private PacketLayerBase _packetLayer;

    /// <summary>
    /// Locks the NetManager to stop it.
    /// </summary>
    private readonly object _stopLock = new();

    /// <summary>
    /// While true, forces sockets to send data directly to interface without routing.
    /// </summary>
    private bool _dontRoute;


    /// <summary>
    /// Initializes this for use.
    /// </summary>
    internal void Initialize(Transport t, int unreliableMTU, PacketLayerBase packetLayer, bool dontRoute)
    {
        Transport = t;
        _mtu = unreliableMTU;
        _packetLayer = packetLayer;
        _dontRoute = dontRoute;
    }


    /// <summary>
    /// Updates the Timeout value as seconds.
    /// </summary>
    internal void UpdateTimeout(int timeout)
    {
        _timeout = timeout;
        base.UpdateTimeout(NetManager, timeout);
    }


    /// <summary>
    /// Polls the socket for new data.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void PollSocket()
    {
        base.PollSocket(NetManager);
    }


    /// <summary>
    /// Threaded operation to process client actions.
    /// </summary>
    private void ThreadedSocket()
    {
        EventBasedNetListener listener = new();
        listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
        listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
        listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;

        NetManager = new NetManager(listener, _packetLayer);
        NetManager.DontRoute = _dontRoute;
        NetManager.MtuOverride = _mtu + NetConstants.FragmentedHeaderTotalSize;

        UpdateTimeout(_timeout);

        _localConnectionStates.Enqueue(LocalConnectionState.Starting);
        NetManager.Start();
        NetManager.Connect(_address, _port, string.Empty);
    }


    /// <summary>
    /// Stops the socket on a new thread.
    /// </summary>
    private void StopSocketOnThread()
    {
        if (NetManager == null)
            return;

        Task.Run(
            () =>
            {
                lock (_stopLock)
                {
                    NetManager?.Stop();
                    NetManager = null;
                }

                //If not stopped yet also enqueue stop.
                if (GetConnectionState() != LocalConnectionState.Stopped)
                    _localConnectionStates.Enqueue(LocalConnectionState.Stopped);
            });
    }


    /// <summary>
    /// Starts the client connection.
    /// </summary>
    /// <param name="address"></param>
    /// <param name="port"></param>
    internal bool StartConnection(string address, ushort port)
    {
        if (GetConnectionState() != LocalConnectionState.Stopped)
            return false;

        SetConnectionState(LocalConnectionState.Starting, false);

        //Assign properties.
        _port = port;
        _address = address;

        ResetQueues();
        Task.Run(ThreadedSocket);

        return true;
    }


    /// <summary>
    /// Stops the local socket.
    /// </summary>
    internal bool StopConnection(DisconnectInfo? info = null)
    {
        if (GetConnectionState() == LocalConnectionState.Stopped || GetConnectionState() == LocalConnectionState.Stopping)
            return false;

        if (info != null)
            Transport.NetworkManager.Logger.Info($"Local client disconnect reason: {info.Value.Reason}.");

        SetConnectionState(LocalConnectionState.Stopping, false);
        StopSocketOnThread();
        return true;
    }


    /// <summary>
    /// Resets queues.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetQueues()
    {
        ClearGenericQueue(ref _localConnectionStates);
        ClearPacketQueue(ref _incoming);
        ClearPacketQueue(ref _outgoing);
    }


    /// <summary>
    /// Called when disconnected from the server.
    /// </summary>
    private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        StopConnection(disconnectInfo);
    }


    /// <summary>
    /// Called when connected to the server.
    /// </summary>
    private void Listener_PeerConnectedEvent(NetPeer peer)
    {
        _localConnectionStates.Enqueue(LocalConnectionState.Started);
    }


    /// <summary>
    /// Called when data is received from a peer.
    /// </summary>
    private void Listener_NetworkReceiveEvent(NetPeer fromPeer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        base.Listener_NetworkReceiveEvent(_incoming, fromPeer, reader, deliveryMethod, _mtu);
    }


    /// <summary>
    /// Dequeues and processes outgoing.
    /// </summary>
    private void DequeueOutgoing()
    {
        NetPeer? peer = null;
        if (NetManager != null)
            peer = NetManager.FirstPeer;

        //Server connection hasn't been made.
        if (peer == null)
        {
            /* Only dequeue outgoing because other queues might have
             * relevant information, such as the local connection queue. #1#
            ClearPacketQueue(ref _outgoing);
        }
        else
        {
            int count = _outgoing.Count;
            for (int i = 0; i < count; i++)
            {
                Packet outgoing = _outgoing.Dequeue();

                ArraySegment<byte> segment = outgoing.GetArraySegment();
                DeliveryMethod dm = outgoing.Channel == (byte)Channel.Reliable ? DeliveryMethod.ReliableOrdered : DeliveryMethod.Unreliable;

                //If over the MTU.
                if (outgoing.Channel == (byte)Channel.Unreliable && segment.Count > _mtu)
                {
                    Transport.NetworkManager.Logger.Warn(
                        $"Client is sending of {segment.Count} length on the unreliable channel, while the MTU is only {_mtu}. The channel has been changed to reliable for this send.");
                    dm = DeliveryMethod.ReliableOrdered;
                }

                peer.Send(segment.Array, segment.Offset, segment.Count, dm);

                outgoing.Dispose();
            }
        }
    }


    /// <summary>
    /// Allows for Outgoing queue to be iterated.
    /// </summary>
    internal void IterateOutgoing()
    {
        DequeueOutgoing();
    }


    /// <summary>
    /// Iterates the Incoming queue.
    /// </summary>
    internal void IterateIncoming()
    {
        /* Run local connection states first so we can begin
         * to read for data at the start of the frame, as that's
         * where incoming is read. #1#
        while (_localConnectionStates.TryDequeue(out LocalConnectionState result))
            SetConnectionState(result, false);

        //Not yet started, cannot continue.
        LocalConnectionState localState = GetConnectionState();
        if (localState != LocalConnectionState.Started)
        {
            ResetQueues();

            //If stopped try to kill task.
            if (localState == LocalConnectionState.Stopped)
            {
                StopSocketOnThread();
                return;
            }
        }

        /* Incoming. #1#
        while (_incoming.TryDequeue(out Packet incoming))
        {
            ClientReceivedPacketArgs dataArgs = new(
                incoming.GetArraySegment(),
                (Channel)incoming.Channel);
            Transport.HandleLocalClientReceivedPacket(dataArgs);

            //Dispose of packet.
            incoming.Dispose();
        }
    }


    /// <summary>
    /// Sends a packet to the server.
    /// </summary>
    internal void SendToServer(byte channelId, ArraySegment<byte> segment)
    {
        //Not started, cannot send.
        if (GetConnectionState() != LocalConnectionState.Started)
            return;

        Send(ref _outgoing, channelId, segment, -1, _mtu);
    }
}*/