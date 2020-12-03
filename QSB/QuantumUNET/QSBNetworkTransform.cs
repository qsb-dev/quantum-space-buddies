using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.QuantumUNET
{
	public class QSBNetworkTransform : QSBNetworkBehaviour
	{
		public TransformSyncMode transformSyncMode
		{
			get
			{
				return this.m_TransformSyncMode;
			}
			set
			{
				this.m_TransformSyncMode = value;
			}
		}

		public float sendInterval
		{
			get
			{
				return this.m_SendInterval;
			}
			set
			{
				this.m_SendInterval = value;
			}
		}

		public AxisSyncMode syncRotationAxis
		{
			get
			{
				return this.m_SyncRotationAxis;
			}
			set
			{
				this.m_SyncRotationAxis = value;
			}
		}

		public CompressionSyncMode rotationSyncCompression
		{
			get
			{
				return this.m_RotationSyncCompression;
			}
			set
			{
				this.m_RotationSyncCompression = value;
			}
		}

		public bool syncSpin
		{
			get
			{
				return this.m_SyncSpin;
			}
			set
			{
				this.m_SyncSpin = value;
			}
		}

		public float movementTheshold
		{
			get
			{
				return this.m_MovementTheshold;
			}
			set
			{
				this.m_MovementTheshold = value;
			}
		}

		public float velocityThreshold
		{
			get
			{
				return this.m_VelocityThreshold;
			}
			set
			{
				this.m_VelocityThreshold = value;
			}
		}

		public float snapThreshold
		{
			get
			{
				return this.m_SnapThreshold;
			}
			set
			{
				this.m_SnapThreshold = value;
			}
		}

		public float interpolateRotation
		{
			get
			{
				return this.m_InterpolateRotation;
			}
			set
			{
				this.m_InterpolateRotation = value;
			}
		}

		public float interpolateMovement
		{
			get
			{
				return this.m_InterpolateMovement;
			}
			set
			{
				this.m_InterpolateMovement = value;
			}
		}

		public ClientMoveCallback3D clientMoveCallback3D
		{
			get
			{
				return this.m_ClientMoveCallback3D;
			}
			set
			{
				this.m_ClientMoveCallback3D = value;
			}
		}

		public ClientMoveCallback2D clientMoveCallback2D
		{
			get
			{
				return this.m_ClientMoveCallback2D;
			}
			set
			{
				this.m_ClientMoveCallback2D = value;
			}
		}

		public CharacterController characterContoller
		{
			get
			{
				return this.m_CharacterController;
			}
		}

		public Rigidbody rigidbody3D
		{
			get
			{
				return this.m_RigidBody3D;
			}
		}

		public float lastSyncTime
		{
			get
			{
				return this.m_LastClientSyncTime;
			}
		}

		public Vector3 targetSyncPosition
		{
			get
			{
				return this.m_TargetSyncPosition;
			}
		}

		public Vector3 targetSyncVelocity
		{
			get
			{
				return this.m_TargetSyncVelocity;
			}
		}

		public Quaternion targetSyncRotation3D
		{
			get
			{
				return this.m_TargetSyncRotation3D;
			}
		}

		public bool grounded
		{
			get
			{
				return this.m_Grounded;
			}
			set
			{
				this.m_Grounded = value;
			}
		}

		private void OnValidate()
		{
			if (this.m_TransformSyncMode < TransformSyncMode.SyncNone || this.m_TransformSyncMode > TransformSyncMode.SyncCharacterController)
			{
				this.m_TransformSyncMode = TransformSyncMode.SyncTransform;
			}
			if (this.m_SendInterval < 0f)
			{
				this.m_SendInterval = 0f;
			}
			if (this.m_SyncRotationAxis < AxisSyncMode.None || this.m_SyncRotationAxis > AxisSyncMode.AxisXYZ)
			{
				this.m_SyncRotationAxis = AxisSyncMode.None;
			}
			if (this.m_MovementTheshold < 0f)
			{
				this.m_MovementTheshold = 0f;
			}
			if (this.m_VelocityThreshold < 0f)
			{
				this.m_VelocityThreshold = 0f;
			}
			if (this.m_SnapThreshold < 0f)
			{
				this.m_SnapThreshold = 0.01f;
			}
			if (this.m_InterpolateRotation < 0f)
			{
				this.m_InterpolateRotation = 0.01f;
			}
			if (this.m_InterpolateMovement < 0f)
			{
				this.m_InterpolateMovement = 0.01f;
			}
		}

		private void Awake()
		{
			this.m_RigidBody3D = base.GetComponent<Rigidbody>();
			this.m_CharacterController = base.GetComponent<CharacterController>();
			this.m_PrevPosition = base.transform.position;
			this.m_PrevRotation = base.transform.rotation;
			this.m_PrevVelocity = 0f;
			if (base.LocalPlayerAuthority)
			{
				this.m_LocalTransformWriter = new NetworkWriter();
			}
		}

		public override void OnStartServer()
		{
			this.m_LastClientSyncTime = 0f;
		}

		public override bool OnSerialize(NetworkWriter writer, bool initialState)
		{
			if (!initialState)
			{
				if (base.SyncVarDirtyBits == 0U)
				{
					writer.WritePackedUInt32(0U);
					return false;
				}
				writer.WritePackedUInt32(1U);
			}
			switch (this.transformSyncMode)
			{
				case TransformSyncMode.SyncNone:
					return false;
				case TransformSyncMode.SyncTransform:
					this.SerializeModeTransform(writer);
					break;
				case TransformSyncMode.SyncRigidbody3D:
					this.SerializeMode3D(writer);
					break;
				case TransformSyncMode.SyncCharacterController:
					this.SerializeModeCharacterController(writer);
					break;
			}
			return true;
		}

		private void SerializeModeTransform(NetworkWriter writer)
		{
			writer.Write(base.transform.position);
			if (this.m_SyncRotationAxis != AxisSyncMode.None)
			{
				SerializeRotation3D(writer, base.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
			}
			this.m_PrevPosition = base.transform.position;
			this.m_PrevRotation = base.transform.rotation;
			this.m_PrevVelocity = 0f;
		}

		private void VerifySerializeComponentExists()
		{
			bool flag = false;
			Type type = null;
			TransformSyncMode transformSyncMode = this.transformSyncMode;
			if (transformSyncMode != TransformSyncMode.SyncCharacterController)
			{
				if (transformSyncMode == TransformSyncMode.SyncRigidbody3D)
				{
					if (!this.m_RigidBody3D && !(this.m_RigidBody3D = base.GetComponent<Rigidbody>()))
					{
						flag = true;
						type = typeof(Rigidbody);
					}
				}
			}
			else if (!this.m_CharacterController && !(this.m_CharacterController = base.GetComponent<CharacterController>()))
			{
				flag = true;
				type = typeof(CharacterController);
			}
			if (flag && type != null)
			{
				throw new InvalidOperationException(string.Format("transformSyncMode set to {0} but no {1} component was found, did you call QSBNetworkServer.Spawn on a prefab?", this.transformSyncMode, type.Name));
			}
		}

		private void SerializeMode3D(NetworkWriter writer)
		{
			this.VerifySerializeComponentExists();
			if (base.IsServer && this.m_LastClientSyncTime != 0f)
			{
				writer.Write(this.m_TargetSyncPosition);
				SerializeVelocity3D(writer, this.m_TargetSyncVelocity, NetworkTransform.CompressionSyncMode.None);
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					SerializeRotation3D(writer, this.m_TargetSyncRotation3D, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			else
			{
				writer.Write(this.m_RigidBody3D.position);
				NetworkTransform.SerializeVelocity3D(writer, this.m_RigidBody3D.velocity, NetworkTransform.CompressionSyncMode.None);
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					SerializeRotation3D(writer, this.m_RigidBody3D.rotation, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			if (this.m_SyncSpin)
			{
				SerializeSpin3D(writer, this.m_RigidBody3D.angularVelocity, this.syncRotationAxis, this.rotationSyncCompression);
			}
			this.m_PrevPosition = this.m_RigidBody3D.position;
			this.m_PrevRotation = base.transform.rotation;
			this.m_PrevVelocity = this.m_RigidBody3D.velocity.sqrMagnitude;
		}

		private void SerializeModeCharacterController(NetworkWriter writer)
		{
			this.VerifySerializeComponentExists();
			if (base.IsServer && this.m_LastClientSyncTime != 0f)
			{
				writer.Write(this.m_TargetSyncPosition);
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					SerializeRotation3D(writer, this.m_TargetSyncRotation3D, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			else
			{
				writer.Write(base.transform.position);
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					SerializeRotation3D(writer, base.transform.rotation, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			this.m_PrevPosition = base.transform.position;
			this.m_PrevRotation = base.transform.rotation;
			this.m_PrevVelocity = 0f;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
			if (!base.IsServer || !QSBNetworkServer.localClientActive)
			{
				if (!initialState)
				{
					if (reader.ReadPackedUInt32() == 0U)
					{
						return;
					}
				}
				switch (this.transformSyncMode)
				{
					case TransformSyncMode.SyncNone:
						return;
					case TransformSyncMode.SyncTransform:
						this.UnserializeModeTransform(reader, initialState);
						break;
					case TransformSyncMode.SyncRigidbody3D:
						this.UnserializeMode3D(reader, initialState);
						break;
					case TransformSyncMode.SyncCharacterController:
						this.UnserializeModeCharacterController(reader, initialState);
						break;
				}
				this.m_LastClientSyncTime = Time.time;
			}
		}

		private void UnserializeModeTransform(NetworkReader reader, bool initialState)
		{
			if (base.HasAuthority)
			{
				reader.ReadVector3();
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			else if (base.IsServer && this.m_ClientMoveCallback3D != null)
			{
				Vector3 position = reader.ReadVector3();
				Vector3 zero = Vector3.zero;
				Quaternion rotation = Quaternion.identity;
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					rotation = UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
				if (this.m_ClientMoveCallback3D(ref position, ref zero, ref rotation))
				{
					base.transform.position = position;
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						base.transform.rotation = rotation;
					}
				}
			}
			else
			{
				base.transform.position = reader.ReadVector3();
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					base.transform.rotation = UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
		}

		private void UnserializeMode3D(NetworkReader reader, bool initialState)
		{
			if (base.HasAuthority)
			{
				reader.ReadVector3();
				reader.ReadVector3();
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
				if (this.syncSpin)
				{
					UnserializeSpin3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			else
			{
				if (base.IsServer && this.m_ClientMoveCallback3D != null)
				{
					Vector3 targetSyncPosition = reader.ReadVector3();
					Vector3 targetSyncVelocity = reader.ReadVector3();
					Quaternion targetSyncRotation3D = Quaternion.identity;
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						targetSyncRotation3D = UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
					}
					if (!this.m_ClientMoveCallback3D(ref targetSyncPosition, ref targetSyncVelocity, ref targetSyncRotation3D))
					{
						return;
					}
					this.m_TargetSyncPosition = targetSyncPosition;
					this.m_TargetSyncVelocity = targetSyncVelocity;
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						this.m_TargetSyncRotation3D = targetSyncRotation3D;
					}
				}
				else
				{
					this.m_TargetSyncPosition = reader.ReadVector3();
					this.m_TargetSyncVelocity = reader.ReadVector3();
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						this.m_TargetSyncRotation3D = UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
					}
				}
				if (this.syncSpin)
				{
					this.m_TargetSyncAngularVelocity3D = UnserializeSpin3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
				if (!(this.m_RigidBody3D == null))
				{
					if (base.IsServer && !base.IsClient)
					{
						this.m_RigidBody3D.MovePosition(this.m_TargetSyncPosition);
						this.m_RigidBody3D.MoveRotation(this.m_TargetSyncRotation3D);
						this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
					}
					else if (this.GetNetworkSendInterval() == 0f)
					{
						this.m_RigidBody3D.MovePosition(this.m_TargetSyncPosition);
						this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
						if (this.syncRotationAxis != AxisSyncMode.None)
						{
							this.m_RigidBody3D.MoveRotation(this.m_TargetSyncRotation3D);
						}
						if (this.syncSpin)
						{
							this.m_RigidBody3D.angularVelocity = this.m_TargetSyncAngularVelocity3D;
						}
					}
					else
					{
						float magnitude = (this.m_RigidBody3D.position - this.m_TargetSyncPosition).magnitude;
						if (magnitude > this.snapThreshold)
						{
							this.m_RigidBody3D.position = this.m_TargetSyncPosition;
							this.m_RigidBody3D.velocity = this.m_TargetSyncVelocity;
						}
						if (this.interpolateRotation == 0f && this.syncRotationAxis != AxisSyncMode.None)
						{
							this.m_RigidBody3D.rotation = this.m_TargetSyncRotation3D;
							if (this.syncSpin)
							{
								this.m_RigidBody3D.angularVelocity = this.m_TargetSyncAngularVelocity3D;
							}
						}
						if (this.m_InterpolateMovement == 0f)
						{
							this.m_RigidBody3D.position = this.m_TargetSyncPosition;
						}
						if (initialState && this.syncRotationAxis != AxisSyncMode.None)
						{
							this.m_RigidBody3D.rotation = this.m_TargetSyncRotation3D;
						}
					}
				}
			}
		}

		private void UnserializeModeCharacterController(NetworkReader reader, bool initialState)
		{
			if (base.HasAuthority)
			{
				reader.ReadVector3();
				if (this.syncRotationAxis != AxisSyncMode.None)
				{
					UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
				}
			}
			else
			{
				if (base.IsServer && this.m_ClientMoveCallback3D != null)
				{
					Vector3 targetSyncPosition = reader.ReadVector3();
					Quaternion targetSyncRotation3D = Quaternion.identity;
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						targetSyncRotation3D = UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
					}
					if (this.m_CharacterController == null)
					{
						return;
					}
					Vector3 velocity = this.m_CharacterController.velocity;
					if (!this.m_ClientMoveCallback3D(ref targetSyncPosition, ref velocity, ref targetSyncRotation3D))
					{
						return;
					}
					this.m_TargetSyncPosition = targetSyncPosition;
					this.m_TargetSyncVelocity = velocity;
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						this.m_TargetSyncRotation3D = targetSyncRotation3D;
					}
				}
				else
				{
					this.m_TargetSyncPosition = reader.ReadVector3();
					if (this.syncRotationAxis != AxisSyncMode.None)
					{
						this.m_TargetSyncRotation3D = UnserializeRotation3D(reader, this.syncRotationAxis, this.rotationSyncCompression);
					}
				}
				if (!(this.m_CharacterController == null))
				{
					Vector3 a = this.m_TargetSyncPosition - base.transform.position;
					Vector3 a2 = a / this.GetNetworkSendInterval();
					this.m_FixedPosDiff = a2 * Time.fixedDeltaTime;
					if (base.IsServer && !base.IsClient)
					{
						base.transform.position = this.m_TargetSyncPosition;
						base.transform.rotation = this.m_TargetSyncRotation3D;
					}
					else if (this.GetNetworkSendInterval() == 0f)
					{
						base.transform.position = this.m_TargetSyncPosition;
						if (this.syncRotationAxis != AxisSyncMode.None)
						{
							base.transform.rotation = this.m_TargetSyncRotation3D;
						}
					}
					else
					{
						float magnitude = (base.transform.position - this.m_TargetSyncPosition).magnitude;
						if (magnitude > this.snapThreshold)
						{
							base.transform.position = this.m_TargetSyncPosition;
						}
						if (this.interpolateRotation == 0f && this.syncRotationAxis != AxisSyncMode.None)
						{
							base.transform.rotation = this.m_TargetSyncRotation3D;
						}
						if (this.m_InterpolateMovement == 0f)
						{
							base.transform.position = this.m_TargetSyncPosition;
						}
						if (initialState && this.syncRotationAxis != AxisSyncMode.None)
						{
							base.transform.rotation = this.m_TargetSyncRotation3D;
						}
					}
				}
			}
		}

		private void FixedUpdate()
		{
			if (base.IsServer)
			{
				this.FixedUpdateServer();
			}
			if (base.IsClient)
			{
				this.FixedUpdateClient();
			}
		}

		private void FixedUpdateServer()
		{
			if (base.SyncVarDirtyBits == 0U)
			{
				if (QSBNetworkServer.active)
				{
					if (base.IsServer)
					{
						if (this.GetNetworkSendInterval() != 0f)
						{
							float num = (base.transform.position - this.m_PrevPosition).magnitude;
							if (num < this.movementTheshold)
							{
								num = Quaternion.Angle(this.m_PrevRotation, base.transform.rotation);
								if (num < this.movementTheshold)
								{
									if (!this.CheckVelocityChanged())
									{
										return;
									}
								}
							}
							base.SetDirtyBit(1U);
						}
					}
				}
			}
		}

		private bool CheckVelocityChanged()
		{
			TransformSyncMode transformSyncMode = this.transformSyncMode;
			bool result = (transformSyncMode == TransformSyncMode.SyncRigidbody3D && (this.m_RigidBody3D && this.m_VelocityThreshold > 0f) && Mathf.Abs(this.m_RigidBody3D.velocity.sqrMagnitude - this.m_PrevVelocity) >= this.m_VelocityThreshold);
			return result;
		}

		private void FixedUpdateClient()
		{
			if (this.m_LastClientSyncTime != 0f)
			{
				if (QSBNetworkServer.active || QSBNetworkClient.active)
				{
					if (base.IsServer || base.IsClient)
					{
						if (this.GetNetworkSendInterval() != 0f)
						{
							if (!base.HasAuthority)
							{
								switch (this.transformSyncMode)
								{
									case TransformSyncMode.SyncRigidbody3D:
										this.InterpolateTransformMode3D();
										break;
									case TransformSyncMode.SyncCharacterController:
										this.InterpolateTransformModeCharacterController();
										break;
								}
							}
						}
					}
				}
			}
		}

		private void InterpolateTransformMode3D()
		{
			if (this.m_InterpolateMovement != 0f)
			{
				Vector3 velocity = (this.m_TargetSyncPosition - this.m_RigidBody3D.position) * this.m_InterpolateMovement / this.GetNetworkSendInterval();
				this.m_RigidBody3D.velocity = velocity;
			}
			if (this.interpolateRotation != 0f)
			{
				this.m_RigidBody3D.MoveRotation(Quaternion.Slerp(this.m_RigidBody3D.rotation, this.m_TargetSyncRotation3D, Time.fixedDeltaTime * this.interpolateRotation));
			}
			this.m_TargetSyncPosition += this.m_TargetSyncVelocity * Time.fixedDeltaTime * 0.1f;
		}

		private void InterpolateTransformModeCharacterController()
		{
			if (!(this.m_FixedPosDiff == Vector3.zero) || !(this.m_TargetSyncRotation3D == base.transform.rotation))
			{
				if (this.m_InterpolateMovement != 0f)
				{
					this.m_CharacterController.Move(this.m_FixedPosDiff * this.m_InterpolateMovement);
				}
				if (this.interpolateRotation != 0f)
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, this.m_TargetSyncRotation3D, Time.fixedDeltaTime * this.interpolateRotation * 10f);
				}
				if (Time.time - this.m_LastClientSyncTime > this.GetNetworkSendInterval())
				{
					this.m_FixedPosDiff = Vector3.zero;
					Vector3 motion = this.m_TargetSyncPosition - base.transform.position;
					this.m_CharacterController.Move(motion);
				}
			}
		}

		private void Update()
		{
			if (base.HasAuthority)
			{
				if (base.LocalPlayerAuthority)
				{
					if (!QSBNetworkServer.active)
					{
						if (Time.time - this.m_LastClientSendTime > this.GetNetworkSendInterval())
						{
							this.SendTransform();
							this.m_LastClientSendTime = Time.time;
						}
					}
				}
			}
		}

		private bool HasMoved()
		{
			float num;
			if (this.m_RigidBody3D != null)
			{
				num = (this.m_RigidBody3D.position - this.m_PrevPosition).magnitude;
			}
			else
			{
				num = (base.transform.position - this.m_PrevPosition).magnitude;
			}
			bool result;
			if (num > 1E-05f)
			{
				result = true;
			}
			else
			{
				if (this.m_RigidBody3D != null)
				{
					num = Quaternion.Angle(this.m_RigidBody3D.rotation, this.m_PrevRotation);
				}
				else
				{
					num = Quaternion.Angle(base.transform.rotation, this.m_PrevRotation);
				}
				if (num > 1E-05f)
				{
					result = true;
				}
				else
				{
					if (this.m_RigidBody3D != null)
					{
						num = Mathf.Abs(this.m_RigidBody3D.velocity.sqrMagnitude - this.m_PrevVelocity);
					}
					result = (num > 1E-05f);
				}
			}
			return result;
		}

		[Client]
		private void SendTransform()
		{
			if (this.HasMoved() && QSBClientScene.readyConnection != null)
			{
				this.m_LocalTransformWriter.StartMessage(6);
				this.m_LocalTransformWriter.Write(base.NetId);
				switch (this.transformSyncMode)
				{
					case TransformSyncMode.SyncNone:
						return;
					case TransformSyncMode.SyncTransform:
						this.SerializeModeTransform(this.m_LocalTransformWriter);
						break;
					case TransformSyncMode.SyncRigidbody3D:
						this.SerializeMode3D(this.m_LocalTransformWriter);
						break;
					case TransformSyncMode.SyncCharacterController:
						this.SerializeModeCharacterController(this.m_LocalTransformWriter);
						break;
				}
				if (this.m_RigidBody3D != null)
				{
					this.m_PrevPosition = this.m_RigidBody3D.position;
					this.m_PrevRotation = this.m_RigidBody3D.rotation;
					this.m_PrevVelocity = this.m_RigidBody3D.velocity.sqrMagnitude;
				}
				else
				{
					this.m_PrevPosition = base.transform.position;
					this.m_PrevRotation = base.transform.rotation;
				}
				this.m_LocalTransformWriter.FinishMessage();
				QSBClientScene.readyConnection.SendWriter(this.m_LocalTransformWriter, this.GetNetworkChannel());
			}
		}

		public static void HandleTransform(QSBNetworkMessage netMsg)
		{
			NetworkInstanceId networkInstanceId = netMsg.Reader.ReadNetworkId();
			GameObject gameObject = QSBNetworkServer.FindLocalObject(networkInstanceId);
			if (gameObject == null)
			{
				if (LogFilter.logError)
				{
					Debug.LogError("Received NetworkTransform data for GameObject that doesn't exist");
				}
			}
			else
			{
				QSBNetworkTransform component = gameObject.GetComponent<QSBNetworkTransform>();
				if (component == null)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("HandleTransform null target");
					}
				}
				else if (!component.LocalPlayerAuthority)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("HandleTransform no localPlayerAuthority");
					}
				}
				else if (netMsg.Connection.ClientOwnedObjects == null)
				{
					if (LogFilter.logError)
					{
						Debug.LogError("HandleTransform object not owned by connection");
					}
				}
				else if (netMsg.Connection.ClientOwnedObjects.Contains(networkInstanceId))
				{
					switch (component.transformSyncMode)
					{
						case TransformSyncMode.SyncNone:
							return;
						case TransformSyncMode.SyncTransform:
							component.UnserializeModeTransform(netMsg.Reader, false);
							break;
						case TransformSyncMode.SyncRigidbody3D:
							component.UnserializeMode3D(netMsg.Reader, false);
							break;
						case TransformSyncMode.SyncCharacterController:
							component.UnserializeModeCharacterController(netMsg.Reader, false);
							break;
					}
					component.m_LastClientSyncTime = Time.time;
				}
				else if (LogFilter.logWarn)
				{
					Debug.LogWarning("HandleTransform netId:" + networkInstanceId + " is not for a valid player");
				}
			}
		}

		private static void WriteAngle(NetworkWriter writer, float angle, CompressionSyncMode compression)
		{
			if (compression != CompressionSyncMode.None)
			{
				if (compression != CompressionSyncMode.Low)
				{
					if (compression == CompressionSyncMode.High)
					{
						writer.Write((short)angle);
					}
				}
				else
				{
					writer.Write((short)angle);
				}
			}
			else
			{
				writer.Write(angle);
			}
		}

		private static float ReadAngle(NetworkReader reader, CompressionSyncMode compression)
		{
			float result;
			if (compression != CompressionSyncMode.None)
			{
				if (compression != CompressionSyncMode.Low)
				{
					if (compression != CompressionSyncMode.High)
					{
						result = 0f;
					}
					else
					{
						result = (float)reader.ReadInt16();
					}
				}
				else
				{
					result = (float)reader.ReadInt16();
				}
			}
			else
			{
				result = reader.ReadSingle();
			}
			return result;
		}

		public static void SerializeVelocity3D(NetworkWriter writer, Vector3 velocity, NetworkTransform.CompressionSyncMode compression)
		{
			writer.Write(velocity);
		}

		public static void SerializeRotation3D(NetworkWriter writer, Quaternion rot, AxisSyncMode mode, CompressionSyncMode compression)
		{
			switch (mode)
			{
				case AxisSyncMode.AxisX:
					WriteAngle(writer, rot.eulerAngles.x, compression);
					break;
				case AxisSyncMode.AxisY:
					WriteAngle(writer, rot.eulerAngles.y, compression);
					break;
				case AxisSyncMode.AxisZ:
					WriteAngle(writer, rot.eulerAngles.z, compression);
					break;
				case AxisSyncMode.AxisXY:
					WriteAngle(writer, rot.eulerAngles.x, compression);
					WriteAngle(writer, rot.eulerAngles.y, compression);
					break;
				case AxisSyncMode.AxisXZ:
					WriteAngle(writer, rot.eulerAngles.x, compression);
					WriteAngle(writer, rot.eulerAngles.z, compression);
					break;
				case AxisSyncMode.AxisYZ:
					WriteAngle(writer, rot.eulerAngles.y, compression);
					WriteAngle(writer, rot.eulerAngles.z, compression);
					break;
				case AxisSyncMode.AxisXYZ:
					WriteAngle(writer, rot.eulerAngles.x, compression);
					WriteAngle(writer, rot.eulerAngles.y, compression);
					WriteAngle(writer, rot.eulerAngles.z, compression);
					break;
			}
		}

		public static void SerializeSpin3D(NetworkWriter writer, Vector3 angularVelocity, AxisSyncMode mode, CompressionSyncMode compression)
		{
			switch (mode)
			{
				case AxisSyncMode.AxisX:
					WriteAngle(writer, angularVelocity.x, compression);
					break;
				case AxisSyncMode.AxisY:
					WriteAngle(writer, angularVelocity.y, compression);
					break;
				case AxisSyncMode.AxisZ:
					WriteAngle(writer, angularVelocity.z, compression);
					break;
				case AxisSyncMode.AxisXY:
					WriteAngle(writer, angularVelocity.x, compression);
					WriteAngle(writer, angularVelocity.y, compression);
					break;
				case AxisSyncMode.AxisXZ:
					WriteAngle(writer, angularVelocity.x, compression);
					WriteAngle(writer, angularVelocity.z, compression);
					break;
				case AxisSyncMode.AxisYZ:
					WriteAngle(writer, angularVelocity.y, compression);
					WriteAngle(writer, angularVelocity.z, compression);
					break;
				case AxisSyncMode.AxisXYZ:
					WriteAngle(writer, angularVelocity.x, compression);
					WriteAngle(writer, angularVelocity.y, compression);
					WriteAngle(writer, angularVelocity.z, compression);
					break;
			}
		}

		public static Vector3 UnserializeVelocity3D(NetworkReader reader, NetworkTransform.CompressionSyncMode compression)
		{
			return reader.ReadVector3();
		}

		public static Quaternion UnserializeRotation3D(NetworkReader reader, AxisSyncMode mode, CompressionSyncMode compression)
		{
			Quaternion identity = Quaternion.identity;
			Vector3 zero = Vector3.zero;
			switch (mode)
			{
				case AxisSyncMode.AxisX:
					zero.Set(ReadAngle(reader, compression), 0f, 0f);
					identity.eulerAngles = zero;
					break;
				case AxisSyncMode.AxisY:
					zero.Set(0f, ReadAngle(reader, compression), 0f);
					identity.eulerAngles = zero;
					break;
				case AxisSyncMode.AxisZ:
					zero.Set(0f, 0f, ReadAngle(reader, compression));
					identity.eulerAngles = zero;
					break;
				case AxisSyncMode.AxisXY:
					zero.Set(ReadAngle(reader, compression), ReadAngle(reader, compression), 0f);
					identity.eulerAngles = zero;
					break;
				case AxisSyncMode.AxisXZ:
					zero.Set(ReadAngle(reader, compression), 0f, ReadAngle(reader, compression));
					identity.eulerAngles = zero;
					break;
				case AxisSyncMode.AxisYZ:
					zero.Set(0f, ReadAngle(reader, compression), ReadAngle(reader, compression));
					identity.eulerAngles = zero;
					break;
				case AxisSyncMode.AxisXYZ:
					zero.Set(ReadAngle(reader, compression), ReadAngle(reader, compression), ReadAngle(reader, compression));
					identity.eulerAngles = zero;
					break;
			}
			return identity;
		}

		public static Vector3 UnserializeSpin3D(NetworkReader reader, AxisSyncMode mode, CompressionSyncMode compression)
		{
			Vector3 zero = Vector3.zero;
			switch (mode)
			{
				case AxisSyncMode.AxisX:
					zero.Set(ReadAngle(reader, compression), 0f, 0f);
					break;
				case AxisSyncMode.AxisY:
					zero.Set(0f, ReadAngle(reader, compression), 0f);
					break;
				case AxisSyncMode.AxisZ:
					zero.Set(0f, 0f, ReadAngle(reader, compression));
					break;
				case AxisSyncMode.AxisXY:
					zero.Set(ReadAngle(reader, compression), ReadAngle(reader, compression), 0f);
					break;
				case AxisSyncMode.AxisXZ:
					zero.Set(ReadAngle(reader, compression), 0f, ReadAngle(reader, compression));
					break;
				case AxisSyncMode.AxisYZ:
					zero.Set(0f, ReadAngle(reader, compression), ReadAngle(reader, compression));
					break;
				case AxisSyncMode.AxisXYZ:
					zero.Set(ReadAngle(reader, compression), ReadAngle(reader, compression), ReadAngle(reader, compression));
					break;
			}
			return zero;
		}

		public override int GetNetworkChannel()
		{
			return 1;
		}

		public override float GetNetworkSendInterval()
		{
			return this.m_SendInterval;
		}

		public override void OnStartAuthority()
		{
			this.m_LastClientSyncTime = 0f;
		}

		[SerializeField]
		private TransformSyncMode m_TransformSyncMode = TransformSyncMode.SyncNone;

		[SerializeField]
		private float m_SendInterval = 0.1f;

		[SerializeField]
		private AxisSyncMode m_SyncRotationAxis = AxisSyncMode.AxisXYZ;

		[SerializeField]
		private CompressionSyncMode m_RotationSyncCompression = CompressionSyncMode.None;

		[SerializeField]
		private bool m_SyncSpin;

		[SerializeField]
		private float m_MovementTheshold = 0.001f;

		[SerializeField]
		private float m_VelocityThreshold = 0.0001f;

		[SerializeField]
		private float m_SnapThreshold = 5f;

		[SerializeField]
		private float m_InterpolateRotation = 1f;

		[SerializeField]
		private float m_InterpolateMovement = 1f;

		[SerializeField]
		private ClientMoveCallback3D m_ClientMoveCallback3D;

		[SerializeField]
		private ClientMoveCallback2D m_ClientMoveCallback2D;

		private Rigidbody m_RigidBody3D;

		private CharacterController m_CharacterController;

		private bool m_Grounded = true;

		private Vector3 m_TargetSyncPosition;

		private Vector3 m_TargetSyncVelocity;

		private Vector3 m_FixedPosDiff;

		private Quaternion m_TargetSyncRotation3D;

		private Vector3 m_TargetSyncAngularVelocity3D;

		private float m_TargetSyncRotation2D;

		private float m_TargetSyncAngularVelocity2D;

		private float m_LastClientSyncTime;

		private float m_LastClientSendTime;

		private Vector3 m_PrevPosition;

		private Quaternion m_PrevRotation;

		private float m_PrevRotation2D;

		private float m_PrevVelocity;

		private const float k_LocalMovementThreshold = 1E-05f;

		private const float k_LocalRotationThreshold = 1E-05f;

		private const float k_LocalVelocityThreshold = 1E-05f;

		private const float k_MoveAheadRatio = 0.1f;

		private NetworkWriter m_LocalTransformWriter;

		public enum TransformSyncMode
		{
			SyncNone,
			SyncTransform,
			SyncRigidbody3D,
			SyncCharacterController
		}

		public enum AxisSyncMode
		{
			None,
			AxisX,
			AxisY,
			AxisZ,
			AxisXY,
			AxisXZ,
			AxisYZ,
			AxisXYZ
		}

		public enum CompressionSyncMode
		{
			None,
			Low,
			High
		}

		public delegate bool ClientMoveCallback3D(ref Vector3 position, ref Vector3 velocity, ref Quaternion rotation);

		public delegate bool ClientMoveCallback2D(ref Vector2 position, ref Vector2 velocity, ref float rotation);
	}
}
