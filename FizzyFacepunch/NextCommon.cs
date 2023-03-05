using Mirror;
using Steamworks;
using Steamworks.Data;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public abstract class NextCommon
{
	protected const int MAX_MESSAGES = 256;

	protected Result SendSocket(Connection conn, byte[] data, int channelId)
	{
		Array.Resize(ref data, data.Length + 1);
		data[data.Length - 1] = (byte)channelId;

		GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
		IntPtr pData = pinnedArray.AddrOfPinnedObject();
		SendType sendFlag = channelId == Channels.Unreliable ? SendType.Unreliable : SendType.Reliable;
		Result res = conn.SendMessage(pData, data.Length, sendFlag);
		if (res != Result.OK)
		{
			Debug.LogWarning($"Send issue: {res}");
		}

		pinnedArray.Free();
		return res;
	}

	protected (byte[], int) ProcessMessage(IntPtr ptrs, int size)
	{
		byte[] managedArray = new byte[size];
		Marshal.Copy(ptrs, managedArray, 0, size);
		int channel = managedArray[managedArray.Length - 1];
		Array.Resize(ref managedArray, managedArray.Length - 1);
		return (managedArray, channel);
	}
}