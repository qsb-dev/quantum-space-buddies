using OWML.Common;
using QSB.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.TransformSync
{
    public abstract class QSBTransformSync : PlayerSyncObject
    {
        private readonly float sendInterval = 0.1f;
        private readonly float movementTheshold = 1f / 1000f;
        private float lastClientSendTime;
        private Vector3 prevPosition;
        private Quaternion prevRotation;
        private NetworkWriter localTransformWriter;
        private Vector3 _positionSmoothVelocity;
        private bool _isInitialized;

        public QSBSector ReferenceSector { get; set; }
        public Transform AttachedObject { get; private set; }

        public abstract bool IsReady { get; }
        protected abstract Transform InitLocalTransform();
        protected abstract Transform InitRemoteTransform();

        private void Awake()
        {
            prevPosition = transform.localPosition;
            prevRotation = transform.localRotation;
            if (!localPlayerAuthority)
            {
                return;
            }
            localTransformWriter = new NetworkWriter();

            PlayerRegistry.PlayerSyncObjects.Add(this);
            DontDestroyOnLoad(gameObject);
            QSBSceneManager.OnSceneLoaded += OnSceneLoaded;

            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(ball.GetComponent<SphereCollider>());
            ball.transform.parent = transform;
            ball.transform.localPosition = Vector3.zero;
        }

        private void OnSceneLoaded(OWScene scene, bool isInUniverse)
        {
            _isInitialized = false;
        }

        public void SetReferenceSector(QSBSector sector)
        {
            DebugLog.DebugWrite("set reference sector of " + NetId + " to " + sector.Sector.name);
            _positionSmoothVelocity = Vector3.zero;
            ReferenceSector = sector;
            transform.SetParent(sector.Transform, true);
            transform.position = sector.Transform.InverseTransformPoint(transform.position);
            transform.rotation = sector.Transform.InverseTransformRotation(transform.rotation);
            if (!hasAuthority)
            {
                AttachedObject.SetParent(sector.Transform, true);
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                writer.Write(transform.localPosition);
                SerializeRotation3D(writer, transform.localRotation);
                prevPosition = transform.localPosition;
                prevRotation = transform.localRotation;
                return true;
            }
            bool wroteSyncVar = false;
            if ((syncVarDirtyBits & 1U) != 0U)
            {
                if (!wroteSyncVar)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    wroteSyncVar = true;
                }
                writer.Write(transform.localPosition);
                SerializeRotation3D(writer, transform.localRotation);
                prevPosition = transform.localPosition;
                prevRotation = transform.localRotation;
            }
            if (!wroteSyncVar)
            {
                writer.WritePackedUInt32(0);
            }
            return wroteSyncVar;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                if (hasAuthority)
                {
                    reader.ReadVector3();
                    UnserializeRotation3D(reader);
                }
                else
                {
                    transform.localPosition = reader.ReadVector3();
                    transform.localRotation = UnserializeRotation3D(reader);
                }
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                if (hasAuthority)
                {
                    reader.ReadVector3();
                    UnserializeRotation3D(reader);
                }
                else
                {
                    transform.localPosition = reader.ReadVector3();
                    transform.localRotation = UnserializeRotation3D(reader);
                }
            }
        }

        private void FixedUpdate()
        {
            if (isServer)
            {
                FixedUpdateServer();
            }
        }

        private void FixedUpdateServer()
        {
            if (syncVarDirtyBits != 0 || !NetworkServer.active || (!isServer || GetNetworkSendInterval() == 0.0) || (transform.localPosition - prevPosition).magnitude < movementTheshold && Quaternion.Angle(prevRotation, transform.localRotation) < movementTheshold)
            {
                return;
            }
            SetDirtyBit(1U);
        }

        protected void Init()
        {
            DebugLog.DebugWrite("init of " + NetId);
            ReferenceSector = QSBSectorManager.Instance.GetStartPlanetSector();
            AttachedObject = hasAuthority ? InitLocalTransform() : InitRemoteTransform();
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized && IsReady)
            {
                Init();
            }
            else if (_isInitialized && !IsReady)
            {
                DebugLog.DebugWrite("deinitialise " + PlayerId);
                _isInitialized = false;
            }

            if (!hasAuthority || !localPlayerAuthority || (NetworkServer.active || Time.time - lastClientSendTime <= GetNetworkSendInterval()))
            {
                return;
            }
            SendTransform();
            lastClientSendTime = Time.time;
        }

        private bool HasMoved()
        {
            if ((transform.localPosition - prevPosition).magnitude > 9.99999974737875E-06)
            {
                return true;
            }
            float num = Quaternion.Angle(transform.localRotation, prevRotation);
            return num > 9.99999974737875E-06;
        }

        protected virtual void UpdateTransform()
        {
            if (hasAuthority)
            {
                transform.localPosition = AttachedObject.localPosition;
                transform.localRotation = AttachedObject.localRotation;
                return;
            }

            AttachedObject.localPosition = transform.localPosition;
            AttachedObject.localRotation = transform.localRotation;
        }

        [Client]
        private void SendTransform()
        {
            if (!HasMoved() || ClientScene.readyConnection == null)
            {
                return;
            }
            localTransformWriter.StartMessage((short)Messaging.EventType.QSBPositionMessage + MsgType.Highest + 1);
            localTransformWriter.Write(netId);
            localTransformWriter.Write(transform.localPosition);
            SerializeRotation3D(localTransformWriter, transform.localRotation);
            prevPosition = transform.localPosition;
            prevRotation = transform.localRotation;
            localTransformWriter.FinishMessage();
            ClientScene.readyConnection.SendWriter(localTransformWriter, GetNetworkChannel());
        }

        public static void HandleTransform(NetworkMessage netMsg)
        {
            var netId = netMsg.reader.ReadNetworkId();
            var localObject = NetworkServer.FindLocalObject(netId);
            if (localObject == null)
            {
                DebugLog.ToConsole("Error - LocalObject not found!", MessageType.Error);
            }
            else
            {
                var component = localObject.GetComponent<TransformSync>();
                if (component == null)
                {
                    DebugLog.ToConsole("Error - LocalObject doesn't have a QSBTransformSync!", MessageType.Error);
                }
                else if (!component.localPlayerAuthority)
                {
                    DebugLog.ToConsole("Error - LocalObject doesn't have localPlayerAuthority!", MessageType.Error);
                }
                else if (netMsg.conn.clientOwnedObjects == null)
                {
                    DebugLog.ToConsole("Error - LocalObject not owned by connection!", MessageType.Error);
                }
                else if (netMsg.conn.clientOwnedObjects.Contains(netId))
                {
                    if (component.hasAuthority)
                    {
                        netMsg.reader.ReadVector3();
                        UnserializeRotation3D(netMsg.reader);
                    }
                    else
                    {
                        component.transform.localPosition = netMsg.reader.ReadVector3();
                        component.transform.localRotation = UnserializeRotation3D(netMsg.reader);
                    }
                }
                else
                {
                    DebugLog.ToConsole("Warning - HandleTransform netId:" + netId + " is not for a valid player", MessageType.Warning);
                }
            }
        }

        private static void WriteAngle(NetworkWriter writer, float angle)
        {
            writer.Write(angle);
        }

        private static float ReadAngle(NetworkReader reader)
        {
            return reader.ReadSingle();
        }

        public static void SerializeRotation3D(NetworkWriter writer, Quaternion rot)
        {
            WriteAngle(writer, rot.eulerAngles.x);
            WriteAngle(writer, rot.eulerAngles.y);
            WriteAngle(writer, rot.eulerAngles.z);
        }

        public static Quaternion UnserializeRotation3D(NetworkReader reader)
        {
            var identity = Quaternion.identity;
            var zero = Vector3.zero;
            zero.Set(ReadAngle(reader), ReadAngle(reader), ReadAngle(reader));
            identity.eulerAngles = zero;
            return identity;
        }

        public override int GetNetworkChannel()
        {
            return 1;
        }

        public override float GetNetworkSendInterval()
        {
            return sendInterval;
        }
    }
}
