using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB
{
	public class QSBNetworkCRC
	{
		internal static QSBNetworkCRC singleton
		{
			get
			{
				if (s_Singleton == null)
				{
					s_Singleton = new QSBNetworkCRC();
				}
				return s_Singleton;
			}
		}

		public Dictionary<string, int> scripts
		{
			get
			{
				return this.m_Scripts;
			}
		}

		public static bool scriptCRCCheck
		{
			get
			{
				return singleton.m_ScriptCRCCheck;
			}
			set
			{
				singleton.m_ScriptCRCCheck = value;
			}
		}

		public static void ReinitializeScriptCRCs(Assembly callingAssembly)
		{
			singleton.m_Scripts.Clear();
			foreach (Type type in callingAssembly.GetTypes())
			{
				if (type.GetBaseType() == typeof(QSBNetworkBehaviour))
				{
					MethodInfo method = type.GetMethod(".cctor", BindingFlags.Static);
					if (method != null)
					{
						method.Invoke(null, new object[0]);
					}
				}
			}
		}

		public static void RegisterBehaviour(string name, int channel)
		{
			singleton.m_Scripts[name] = channel;
		}

		internal static bool Validate(QSBCRCMessageEntry[] scripts, int numChannels)
		{
			return singleton.ValidateInternal(scripts, numChannels);
		}

		private bool ValidateInternal(QSBCRCMessageEntry[] remoteScripts, int numChannels)
		{
			bool result;
			if (this.m_Scripts.Count != remoteScripts.Length)
			{
				if (LogFilter.logWarn)
				{
					Debug.LogWarning("Network configuration mismatch detected. The number of networked scripts on the client does not match the number of networked scripts on the server. This could be caused by lazy loading of scripts on the client. This warning can be disabled by the checkbox in NetworkManager Script CRC Check.");
				}
				this.Dump(remoteScripts);
				result = false;
			}
			else
			{
				foreach (QSBCRCMessageEntry crcmessageEntry in remoteScripts)
				{
					if (LogFilter.logDebug)
					{
						Debug.Log(string.Concat(new object[]
						{
							"Script: ",
							crcmessageEntry.name,
							" Channel: ",
							crcmessageEntry.channel
						}));
					}
					if (this.m_Scripts.ContainsKey(crcmessageEntry.name))
					{
						int num = this.m_Scripts[crcmessageEntry.name];
						if (num != (int)crcmessageEntry.channel)
						{
							if (LogFilter.logError)
							{
								Debug.LogError(string.Concat(new object[]
								{
									"HLAPI CRC Channel Mismatch. Script: ",
									crcmessageEntry.name,
									" LocalChannel: ",
									num,
									" RemoteChannel: ",
									crcmessageEntry.channel
								}));
							}
							this.Dump(remoteScripts);
							return false;
						}
					}
					if ((int)crcmessageEntry.channel >= numChannels)
					{
						if (LogFilter.logError)
						{
							Debug.LogError(string.Concat(new object[]
							{
								"HLAPI CRC channel out of range! Script: ",
								crcmessageEntry.name,
								" Channel: ",
								crcmessageEntry.channel
							}));
						}
						this.Dump(remoteScripts);
						return false;
					}
				}
				result = true;
			}
			return result;
		}

		private void Dump(QSBCRCMessageEntry[] remoteScripts)
		{
			foreach (string text in this.m_Scripts.Keys)
			{
				Debug.Log(string.Concat(new object[]
				{
					"CRC Local Dump ",
					text,
					" : ",
					this.m_Scripts[text]
				}));
			}
			foreach (QSBCRCMessageEntry crcmessageEntry in remoteScripts)
			{
				Debug.Log(string.Concat(new object[]
				{
					"CRC Remote Dump ",
					crcmessageEntry.name,
					" : ",
					crcmessageEntry.channel
				}));
			}
		}

		internal static QSBNetworkCRC s_Singleton;

		private Dictionary<string, int> m_Scripts = new Dictionary<string, int>();

		private bool m_ScriptCRCCheck;
	}
}
