using Cysharp.Threading.Tasks;
using System;

namespace QSB.Utility
{
	public static class DelayUtil
	{
		public static UniTask RunNextFrame(Action action) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame();
			action();
		});

		public static UniTask RunFramesLater(int n, Action action) => UniTask.Create(async () =>
		{
			await UniTask.DelayFrame(n);
			action();
		});

		public static UniTask RunWhen(Func<bool> predicate, Action action) => UniTask.Create(async () =>
		{
			await UniTask.WaitUntil(predicate);
			action();
		});

		public static UniTask RunNextFrame(Func<UniTask> func) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame();
			await func();
		});

		public static UniTask RunFramesLater(int n, Func<UniTask> func) => UniTask.Create(async () =>
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
