/*
    GITHUB: https://github.com/Derek-R-S
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Discord;

namespace DiscordMirror
{
    public class DiscordTransport : Transport
    {
        public Discord.Discord discordClient { get; private set; }
        private Discord.LobbyManager lobbyManager;
        private Discord.UserManager userManager;
        public Lobby currentLobby;
        private BiDictionary<long, int> clients;
        private int currentMemberId = 0;
        private bool lobbyCreated = false;
        private bool canReconnect = true;
        // Public variables so you can access them from another script and modify them
        public const string Scheme = "discord";
        public uint serverCapacity = 16;
        public LobbyType lobbyType = LobbyType.Public;
        [Tooltip("Connect to the Public Test Build of discord, useful for testing on the same pc.")]
        public bool usePTB;
        [Tooltip("This is also know as the \"Client ID\" in your developer dashboard.")]
        public long discordGameID;
        public CreateFlags createFlags;

        void SetupCallbacks()
        {
            lobbyManager.OnMemberConnect += LobbyManager_OnMemberConnect;
            lobbyManager.OnMemberDisconnect += LobbyManager_OnMemberDisconnect;
            lobbyManager.OnLobbyDelete += LobbyManager_OnLobbyDelete;
            lobbyManager.OnNetworkMessage += LobbyManager_OnNetworkMessage;
            lobbyManager.OnMemberUpdate += LobbyManager_OnMemberUpdate;
        }

        // Gets the string used to connect to the server, it will return null if you arent in a lobby.
        public string GetConnectString()
        {
            if (currentLobby.Id == 0)
                return null;

            return lobbyManager.GetLobbyActivitySecret(currentLobby.Id);
        }

        #region Transport Functions

        private void Awake()
        {
            Environment.SetEnvironmentVariable("DISCORD_INSTANCE_ID", usePTB ? "1" : "0");
            try
            {
                discordClient = new Discord.Discord(discordGameID, (ulong)createFlags);
            }
            catch (ResultException result)
            {
                Debug.LogError("Failed initializing Discord! Reason: " + result.ToString());
                return;
            }
            lobbyManager = discordClient.GetLobbyManager();
            userManager = discordClient.GetUserManager();
            SetupCallbacks();
        }
		
        private void LateUpdate()
        {
            if (discordClient != null)
                discordClient.RunCallbacks();

            if (lobbyManager != null)
                lobbyManager.FlushNetwork();
        }

        public override bool Available()
        {
            // Discord client has to be valid
            return discordClient != null;
        }

        public override void ClientConnect(string address)
        {
            if (discordClient == null || lobbyManager == null)
            {
                Debug.Log("Cannot create server as discord is not initialized!");
                return;
            }

            if (!canReconnect)
            {
                Debug.Log("Already connecting...");
                return;
            }
            canReconnect = false;
            lobbyManager.ConnectLobbyWithActivitySecret(address, LobbyJoined);
        }

        public override bool ClientConnected()
        {
            return currentLobby.Id != 0;
        }

        public override void ClientDisconnect()
        {
            if (currentLobby.Id == 0)
                return;

            lobbyManager.DisconnectNetwork(currentLobby.Id);
            lobbyManager.DisconnectLobby(currentLobby.Id, LobbyDisconnected);
            currentLobby = new Lobby();
        }

        public override void ClientSend(ArraySegment<byte> segment, int channelId = 0)
        {
            try
            {
                lobbyManager.SendNetworkMessage(currentLobby.Id, currentLobby.OwnerId, (byte)channelId, segment.Array);
            }
            catch
            {
            }
        }

        public override int GetMaxPacketSize(int channelId = 0)
        {
            // Don't know if this is correct, or if discord fragments. But this should be a safe number to use for now.
            return 1200;
        }

        public override bool ServerActive()
        {
            return currentLobby.Id == 0 ? false : currentLobby.OwnerId == userManager.GetCurrentUser().Id;
        }

        public override void ServerDisconnect(int connectionId)
        {
            try
            {
                var txn = lobbyManager.GetMemberUpdateTransaction(currentLobby.Id, clients.GetBySecond(connectionId));
                txn.SetMetadata("kicked", "true");
                lobbyManager.UpdateMember(currentLobby.Id, clients.GetBySecond(connectionId), txn, (result) => { });
            }
            catch
            {
            }
        }

        public override string ServerGetClientAddress(int connectionId)
        {
            return clients.GetBySecond(connectionId).ToString();
        }

        public override void ServerSend(int connectionId, ArraySegment<byte> segment, int channelId = 0)
        {
            try
            {
                lobbyManager.SendNetworkMessage(currentLobby.Id, clients.GetBySecond(connectionId), (byte)channelId, segment.Array);
            }
            catch (Exception e)
            {
                OnServerError?.Invoke(connectionId, new Exception("Error sending data to client: " + e.ToString()));
            }
        }

        public override void ServerStart()
        {
            if (ClientConnected())
            {
                Debug.Log("Client is already active!");
                return;
            }

            if (ServerActive())
            {
                Debug.Log("Server is already active!");
                return;
            }

            if (discordClient == null || lobbyManager == null)
            {
                Debug.Log("Cannot create server as discord is not initialized!");
                return;
            }

            clients = new BiDictionary<long, int>();
            currentMemberId = 1;
            LobbyTransaction txn = lobbyManager.GetLobbyCreateTransaction();
            txn.SetCapacity(serverCapacity);
            txn.SetType(lobbyType);
            lobbyCreated = false;
            lobbyManager.CreateLobby(txn, LobbyCreated);

            // Wait until server is actually ready.
            while (true)
            {
                discordClient.RunCallbacks();
                if (lobbyCreated)
                    break;
                System.Threading.Thread.Sleep(100);
            }

            var activity = new Discord.Activity
            {
                State = "Hosting",
                Details = "In Solar System",
                Party =
                {
                    Id = $"{currentLobby.Id}"
                },
                Secrets =
				{
                    Join = currentLobby.Secret
				}
            };

            discordClient.GetActivityManager().UpdateActivity(activity, null);

            lobbyCreated = false;

            discordClient.GetActivityManager().OnActivityJoin += secret =>
            {
                Debug.LogError($"ONJOIN SECRET:{secret}");
                lobbyManager.ConnectLobbyWithActivitySecret(secret, (Discord.Result result, ref Discord.Lobby lobby) =>
                {
                    Debug.LogError($"CONNECTED TO LOBBY {lobby.Id}");
                });

            };

            discordClient.GetActivityManager().OnActivityJoinRequest += (ref Discord.User user) => Debug.LogError($"OnJoinRequest username:{user.Username} id:{user.Id}");
        }

        public override void ServerStop()
        {
            if (currentLobby.Id == 0)
                return;

            lobbyManager.DisconnectNetwork(currentLobby.Id);
            lobbyManager.DisconnectLobby(currentLobby.Id, LobbyDisconnected);
            currentLobby = new Lobby();
        }
        public override void OnApplicationQuit()
        {
            ServerStop();
            base.OnApplicationQuit();
        }
        public override Uri ServerUri()
        {
            UriBuilder builder = new UriBuilder();
            builder.Scheme = Scheme;
            builder.Host = currentLobby.Id.ToString();
            builder.Query = currentLobby.Secret;
            return builder.Uri;
        }

        public override void ClientConnect(Uri uri)
        {
            if (uri.Scheme != Scheme)
                throw new ArgumentException($"Invalid url {uri}, use {Scheme}://LobbyID/?Secret instead", nameof(uri));

            if (discordClient == null || lobbyManager == null)
            {
                Debug.Log("Cannot create server as discord is not initialized!");
                return;
            }

            if (!canReconnect)
            {
                Debug.Log("Already connecting...");
                return;
            }
            canReconnect = false;

            lobbyManager.ConnectLobbyWithActivitySecret(string.Format("{0}:{1}", uri.Host, uri.Query.Replace("?", "")), LobbyJoined);
        }

        public override void Shutdown()
        {
            if (discordClient != null)
                discordClient.Dispose();
        }

        #endregion

        #region Callbacks
        void LobbyCreated(Result result, ref Lobby lobby)
        {
            lobbyCreated = true;
            switch (result)
            {
                case (Result.Ok):
                    currentLobby = lobby;
                    lobbyManager.ConnectNetwork(currentLobby.Id);
                    lobbyManager.OpenNetworkChannel(currentLobby.Id, 0, true);
                    lobbyManager.OpenNetworkChannel(currentLobby.Id, 1, false);
                    break;
                case Result.InvalidCommand:
                    Debug.LogError("Already Connected?");
                    break;
                default:
                    OnServerError?.Invoke(0, new Exception("Failed to start discord lobby, Reason: " + result));
                    Debug.LogError("Discord Transport - ERROR: " + result.ToString());
                    break;
            }
        }

        void LobbyJoined(Result result, ref Lobby lobby)
        {
            canReconnect = true;
            switch (result)
            {
                case (Result.Ok):
                    currentLobby = lobby;
                    lobbyManager.ConnectNetwork(currentLobby.Id);
                    lobbyManager.OpenNetworkChannel(currentLobby.Id, 0, true);
                    lobbyManager.OpenNetworkChannel(currentLobby.Id, 1, false);
                    OnClientConnected?.Invoke();
                    break;
                case Result.InvalidCommand:
                    Debug.LogError("Already Connected?");
                    break;
                default:
                    Debug.LogError("Discord Transport - ERROR: " + result.ToString());
                    OnClientDisconnected?.Invoke();
                    break;
            }
        }

        void LobbyDisconnected(Result result)
        {
            currentLobby = new Lobby();
        }

        private void LobbyManager_OnMemberConnect(long lobbyId, long userId)
        {
            if (ServerActive())
            {
                clients.Add(userId, currentMemberId);
                OnServerConnected?.Invoke(currentMemberId);
                currentMemberId++;
            }
            else
            {
                if (userId == userManager.GetCurrentUser().Id)
                    OnClientConnected?.Invoke();
            }
        }

        private void LobbyManager_OnNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
        {
            if (ServerActive())
            {
                OnServerDataReceived?.Invoke(clients.GetByFirst(userId), new ArraySegment<byte>(data), channelId);
            }
            else if (userId == currentLobby.OwnerId)
            {
                OnClientDataReceived?.Invoke(new ArraySegment<byte>(data), channelId);
            }
        }

        private void LobbyManager_OnLobbyDelete(long lobbyId, uint reason)
        {
            OnClientDisconnected?.Invoke();
        }

        private void LobbyManager_OnMemberDisconnect(long lobbyId, long userId)
        {
            if (ServerActive())
            {
                OnServerDisconnected?.Invoke(clients.GetByFirst(userId));
                clients.Remove(userId);
            }

            if (currentLobby.OwnerId == userId)
            {
                ClientDisconnect();
                OnClientDisconnected?.Invoke();
            }
        }

        private void LobbyManager_OnMemberUpdate(long lobbyId, long userId)
        {
            if (userId == userManager.GetCurrentUser().Id)
            {
                try
                {
                    if (lobbyManager.GetMemberMetadataValue(currentLobby.Id, userId, "kicked") == "true")
                        ClientDisconnect();
                }
                catch { }
            }
        }
        #endregion
    }
}