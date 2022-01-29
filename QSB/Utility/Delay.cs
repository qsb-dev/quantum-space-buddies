using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace QSB.Utility
{
	public static class Delay
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

		public static UniTask RunNextFrame(Action action, CancellationToken ct) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame(ct);
			action();
		});

		public static UniTask RunFramesLater(int n, Action action, CancellationToken ct) => UniTask.Create(async () =>
		{
			await UniTask.DelayFrame(n, cancellationToken: ct);
			action();
		});

		public static UniTask RunWhen(Func<bool> predicate, Action action, CancellationToken ct) => UniTask.Create(async () =>
		{
			await UniTask.WaitUntil(predicate, cancellationToken: ct);
			action();
		});

		public static UniTask RunNextFrame(Func<UniTask> func, CancellationToken ct) => UniTask.Create(async () =>
		{
			await UniTask.NextFrame(ct);
			await func();
		});

		public static UniTask RunFramesLater(int n, Func<UniTask> func, CancellationToken ct) => UniTask.Create(async () =>
		{
			await UniTask.DelayFrame(n, cancellationToken: ct);
			await func();
		});

		public static UniTask RunWhen(Func<bool> predicate, Func<UniTask> func, CancellationToken ct) => UniTask.Create(async () =>
		{
			await UniTask.WaitUntil(predicate, cancellationToken: ct);
			await func();
		});
	}
}
