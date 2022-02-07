using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Platform;
using System;
using UnityEngine;

namespace EpicRerouter
{
	/// <summary>
	/// runs on process side
	/// </summary>
	public static class EpicPlatformManager
	{
		public const string EOS_PRODUCT_ID = "prod-starfish";
		public const string EOS_SANDBOX_ID = "starfish";
		public const string EOS_DEPLOYMENT_ID = "e176ecc84fbc4dd8934664684f44dc71";
		public const string EOS_CLIENT_ID = "5c553c6accee4111bc8ea3a3ae52229b";
		public const string EOS_CLIENT_SECRET = "k87Nfp75BzPref4nJFnnbNjYXQQR";

		private const float TICK_INTERVAL = 0.1f;
		private const bool ENABLE_SDK_DEBUGGING = false;

		public static PlatformInterface PlatformInterface { get; private set; }
		public static EpicAccountId LocalUserId { get; private set; }
		private static float _lastTick;

		public static OWEvent onAuthSuccess = new(1);

		public static void Init()
		{
			if (PlatformInterface == null)
			{
				try
				{
					InitPlatform();
				}
				catch (EOSInitializeException ex)
				{
					if (ex.Result == Result.AlreadyConfigured)
					{
						Debug.Log("[EOS] platform already configured!");
					}
				}
			}

			Auth();
		}

		public static void Tick()
		{
			if (PlatformInterface != null && _lastTick + 0.1f <= Time.unscaledTime)
			{
				PlatformInterface.Tick();
				_lastTick = Time.unscaledTime;
			}
		}

		public static void Uninit()
		{
			PlatformInterface.Release();
			PlatformInterface = null;
			PlatformInterface.Shutdown();
		}

		private static void InitPlatform()
		{
			var result = PlatformInterface.Initialize(new InitializeOptions
			{
				ProductName = Application.productName,
				ProductVersion = Application.version
			});
			if (result != Result.Success)
			{
				throw new EOSInitializeException("Failed to initialize Epic Online Services platform: ", result);
			}

			var options = new Options
			{
				ProductId = "prod-starfish",
				SandboxId = "starfish",
				ClientCredentials = new ClientCredentials
				{
					ClientId = "5c553c6accee4111bc8ea3a3ae52229b",
					ClientSecret = "k87Nfp75BzPref4nJFnnbNjYXQQR"
				},
				DeploymentId = "e176ecc84fbc4dd8934664684f44dc71"
			};
			PlatformInterface = PlatformInterface.Create(options);
			Debug.Log("[EOS] Platform interface has been created");
		}

		private static void Auth()
		{
			Debug.Log("[EOS] Authenticating...");
			var loginOptions = new LoginOptions
			{
				Credentials = new Credentials
				{
					Type = LoginCredentialType.ExchangeCode,
					Id = null,
					Token = GetPasswordFromCommandLine()
				},
				ScopeFlags = 0
			};
			if (PlatformInterface == null)
			{
				Debug.LogError("[EOS] Platform interface is null!");
			}

			PlatformInterface.GetAuthInterface().Login(loginOptions, null, OnLogin);
		}

		private static string GetPasswordFromCommandLine()
		{
			var commandLineArgs = Environment.GetCommandLineArgs();
			foreach (var arg in commandLineArgs)
			{
				if (arg.Contains("AUTH_PASSWORD"))
				{
					return arg.Split('=')[1];
				}
			}

			return null;
		}

		private static void OnLogin(LoginCallbackInfo loginCallbackInfo)
		{
			if (loginCallbackInfo.ResultCode == Result.Success)
			{
				LocalUserId = loginCallbackInfo.LocalUserId;
				Debug.Log($"[EOS SDK] login success! user ID: {LocalUserId}");
				onAuthSuccess.Invoke();
				return;
			}

			Debug.LogError("[EOS SDK] Login failed");
		}

		public class EOSInitializeException : Exception
		{
			public readonly Result Result;
			public EOSInitializeException(string msg, Result initResult) : base(msg) => Result = initResult;
		}
	}
}
