using Cysharp.Threading.Tasks;
using System;

namespace QSB.Utility
{
	public static class UnityEvents
	{
		public static UniTask FireOnNextUpdate(Action action) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame();
			action();
		});

		public static UniTask FireInNUpdates(Action action, int n) => UniTask.Create(async () =>
		{
			await UniTask.DelayFrame(n);
			action();
		});

		public static UniTask RunWhen(Func<bool> predicate, Action action) => UniTask.Create(async () =>
		{
			await UniTask.WaitUntil(predicate);
			action();
		});

		public static UniTask FireOnNextUpdate(Func<UniTask> func) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame();
			await func();
		});

		public static UniTask FireInNUpdates(Func<UniTask> func, int n) => UniTask.Create(async () =>
		{
			await UniTask.DelayFrame(n);
			await func();
		});

		public static UniTask RunWhen(Func<bool> predicate, Func<UniTask> func) => UniTask.Create(async () =>
		{
			await UniTask.WaitUntil(predicate);
			await func();
		});
	}
}
