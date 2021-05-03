using Mono.Cecil;

namespace QNetWeaver
{
	internal class MonoBehaviourProcessor
	{
		public MonoBehaviourProcessor(TypeDefinition td) => m_td = td;

		public void Process()
		{
			ProcessSyncVars();
			ProcessMethods();
		}

		private void ProcessSyncVars()
		{
			foreach (var fieldDefinition in m_td.Fields)
			{
				foreach (var customAttribute in fieldDefinition.CustomAttributes)
				{
					if (customAttribute.AttributeType.FullName == Weaver.SyncVarType.FullName)
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses [SyncVar] ",
							fieldDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
				}
				if (Helpers.InheritsFromSyncList(fieldDefinition.FieldType))
				{
					Log.Error(string.Format("Script {0} defines field {1} with type {2}, but it's not a NetworkBehaviour", m_td.FullName, fieldDefinition.Name, Helpers.PrettyPrintType(fieldDefinition.FieldType)));
					Weaver.fail = true;
				}
			}
		}

		private void ProcessMethods()
		{
			foreach (var methodDefinition in m_td.Methods)
			{
				foreach (var customAttribute in methodDefinition.CustomAttributes)
				{
					if (customAttribute.AttributeType.FullName == Weaver.CommandType.FullName)
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses [Command] ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
					if (customAttribute.AttributeType.FullName == Weaver.ClientRpcType.FullName)
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses [ClientRpc] ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
					if (customAttribute.AttributeType.FullName == Weaver.TargetRpcType.FullName)
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses [TargetRpc] ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
					var text = customAttribute.Constructor.DeclaringType.ToString();
					if (text == "UnityEngine.Networking.ServerAttribute")
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses the attribute [Server] on the method ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
					else if (text == "UnityEngine.Networking.ServerCallbackAttribute")
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses the attribute [ServerCallback] on the method ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
					else if (text == "UnityEngine.Networking.ClientAttribute")
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses the attribute [Client] on the method ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
					else if (text == "UnityEngine.Networking.ClientCallbackAttribute")
					{
						Log.Error(string.Concat(new string[]
						{
							"Script ",
							m_td.FullName,
							" uses the attribute [ClientCallback] on the method ",
							methodDefinition.Name,
							" but is not a NetworkBehaviour."
						}));
						Weaver.fail = true;
					}
				}
			}
		}

		private TypeDefinition m_td;
	}
}
