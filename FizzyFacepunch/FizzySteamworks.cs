#if !DISABLESTEAMWORKS
using Steamworks;
using System;
using System.IO;
using UnityEngine;

namespace Mirror.FizzySteam
{
    public enum FizzyMessageType
    {
		Message = 0,
		Error = 1,
		Warning = 2,
		Info = 3,
		Success = 4
	}

    public static class FizzyLogger
    {
		public static event Action<string, FizzyMessageType> LogEvent;
		public static void Log(string message, FizzyMessageType severity) => LogEvent?.Invoke(message, severity);
	}

    [HelpURL("https://github.com/Chykary/FizzySteamworks")]
    public class FizzySteamworks : Transport
    {
        private const string STEAM_SCHEME = "steam";

        private static IClient client;
        private static IServer server;

        [SerializeField]
        public EP2PSend[] Channels = new EP2PSend[2] { EP2PSend.k_EP2PSendReliable, EP2PSend.k_EP2PSendUnreliableNoDelay };

        [Tooltip("Timeout for connecting in seconds.")]
        public int Timeout = 25;
        [Tooltip("Allow or disallow P2P connections to fall back to being relayed through the Steam servers if a direct connection or NAT-traversal cannot be established.")]
        public bool AllowSteamRelay = true;

        [Tooltip("Use SteamSockets instead of the (deprecated) SteamNetworking. This will always use Relay.")]
        public bool UseNextGenSteamNetworking = true;

        private void OnEnable()
        {
            Debug.Assert(Channels != null && Channels.Length > 0, "No channel configured for FizzySteamworks.");
            Invoke(nameof(InitRelayNetworkAccess), 1f);
        }

        public override void ClientEarlyUpdate()
        {
            if (enabled)
            {
                client?.ReceiveData();
            }
        }

        public override void ServerEarlyUpdate()
        {
            if (enabled)
            {
                server?.ReceiveData();
            }
        }

        public override void ClientLateUpdate()
        {
            if (enabled)
            {
                client?.FlushData();
            }
        }

        public override void ServerLateUpdate()
        {
            if (enabled)
            {
                server?.FlushData();
            }
        }

        public override bool ClientConnected() => ClientActive() && client.Connected;
        public override void ClientConnect(string address)
        {
            try
            {
#if UNITY_SERVER
                SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
                SteamNetworkingUtils.InitRelayNetworkAccess();
#endif

                InitRelayNetworkAccess();

                if (ServerActive())
                {
                    FizzyLogger.Log("Transport already running as server!", FizzyMessageType.Error);
                    return;
                }

                if (!ClientActive() || client.Error)
                {
                    if (UseNextGenSteamNetworking)
                    {
                        FizzyLogger.Log($"Starting client [SteamSockets], target address {address}.", FizzyMessageType.Info);
                        client = NextClient.CreateClient(this, address);
                    }
                    else
                    {
                        FizzyLogger.Log($"Starting client [DEPRECATED SteamNetworking], target address {address}. Relay enabled: {AllowSteamRelay}", FizzyMessageType.Info);
                        SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
                        client = LegacyClient.CreateClient(this, address);
                    }
                }
                else
                {
                    FizzyLogger.Log("Client already running!", FizzyMessageType.Error);
                }
            }
            catch (Exception ex)
            {
                FizzyLogger.Log("Exception: " + ex.Message + ". Client could not be started.", FizzyMessageType.Error);
                OnClientDisconnected.Invoke();
            }
        }

        public override void ClientConnect(Uri uri)
        {
            if (uri.Scheme != STEAM_SCHEME)
                throw new ArgumentException($"Invalid url {uri}, use {STEAM_SCHEME}://SteamID instead", nameof(uri));

            ClientConnect(uri.Host);
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId)
        {
            byte[] data = new byte[segment.Count];
            Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
            client.Send(data, channelId);
        }

        public override void ClientDisconnect()
        {
            if (ClientActive())
            {
                Shutdown();
            }
        }

        public bool ClientActive() => client != null;


        public override bool ServerActive() => server != null;

        public override void ServerStart()
        {
            try
            {
#if UNITY_SERVER
                SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
                SteamNetworkingUtils.InitRelayNetworkAccess();
#endif


                InitRelayNetworkAccess();

                if (ClientActive())
                {
                    FizzyLogger.Log("Transport already running as client!", FizzyMessageType.Error);
                    return;
                }

                if (!ServerActive())
                {
                    if (UseNextGenSteamNetworking)
                    {
                        FizzyLogger.Log($"Starting server [SteamSockets].", FizzyMessageType.Info);
                        server = NextServer.CreateServer(this, NetworkManager.singleton.maxConnections);
                    }
                    else
                    {
						FizzyLogger.Log($"Starting server [DEPRECATED SteamNetworking]. Relay enabled: {AllowSteamRelay}", FizzyMessageType.Info);
#if UNITY_SERVER
                        SteamGameServerNetworking.AllowP2PPacketRelay(AllowSteamRelay);
#else

						SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
#endif
                        server = LegacyServer.CreateServer(this, NetworkManager.singleton.maxConnections);
                    }
                }
                else
                {
                    FizzyLogger.Log($"Server already started!", FizzyMessageType.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
        }

        public override Uri ServerUri()
        {
            var steamBuilder = new UriBuilder
            {
                Scheme = STEAM_SCHEME,
#if UNITY_SERVER
                Host = SteamGameServer.GetSteamID().m_SteamID.ToString()
#else
                Host = SteamUser.GetSteamID().m_SteamID.ToString()
#endif
            };

            return steamBuilder.Uri;
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId)
        {
            if (ServerActive())
            {
                byte[] data = new byte[segment.Count];
                Array.Copy(segment.Array, segment.Offset, data, 0, segment.Count);
                server.Send(connectionId, data, channelId);
            }
        }
        public override void ServerDisconnect(int connectionId)
        {
            if (ServerActive())
            {
                server.Disconnect(connectionId);
            }
        }
        public override string ServerGetClientAddress(int connectionId) => ServerActive() ? server.ServerGetClientAddress(connectionId) : string.Empty;
        public override void ServerStop()
        {
            if (ServerActive())
            {
                Shutdown();
            }
        }

        public override void Shutdown()
        {
            if (server != null)
            {
                server.Shutdown();
                server = null;
                FizzyLogger.Log("Transport shut down - was server.", FizzyMessageType.Info);
            }

            if (client != null)
            {
                client.Disconnect();
                client = null;
				FizzyLogger.Log("Transport shut down - was client.", FizzyMessageType.Info);
			}
        }

        public override int GetMaxPacketSize(int channelId)
        {
            if (UseNextGenSteamNetworking)
            {
                return Constants.k_cbMaxSteamNetworkingSocketsMessageSizeSend;
            }
            else
            {
                if (channelId >= Channels.Length)
                {
                    Debug.LogError("Channel Id exceeded configured channels! Please configure more channels.");
                    return 1200;
                }

                switch (Channels[channelId])
                {
                    case EP2PSend.k_EP2PSendUnreliable:
                    case EP2PSend.k_EP2PSendUnreliableNoDelay:
                        return 1200;
                    case EP2PSend.k_EP2PSendReliable:
                    case EP2PSend.k_EP2PSendReliableWithBuffering:
                        return 1048576;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public override bool Available()
        {
            try
            {
#if UNITY_SERVER
                SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
                SteamNetworkingUtils.InitRelayNetworkAccess();
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void InitRelayNetworkAccess()
        {
            try
            {
                if (UseNextGenSteamNetworking)
                {
#if UNITY_SERVER
                    SteamGameServerNetworkingUtils.InitRelayNetworkAccess();
#else
                    SteamNetworkingUtils.InitRelayNetworkAccess();
#endif
                }
            }
            catch { }
        }

        private void OnDestroy()
        {
            Shutdown();
        }
    }
}
#endif // !DISABLESTEAMWORKS