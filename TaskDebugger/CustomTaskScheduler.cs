using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskDebugger
{
	public class CustomTaskScheduler : TaskScheduler, IDisposable
	{
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();
		private readonly BlockingCollection<Task> _queue = new BlockingCollection<Task>();
		private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(0);
		private bool _isDisposed;


		public CustomTaskScheduler(uint threadNum)
		{
			for (var i = 0; i < threadNum; i++)
			{
				var thread = new Thread(ThreadProcess);
				thread.Name = $"Thread Pool thread: {thread.ManagedThreadId}";
				thread.IsBackground = true;
				thread.Start(_cts.Token);
			}
		}

		public void Dispose()
		{
			_cts.Cancel();
			_isDisposed = true;
			if (_queue.Any())
				throw new UnprocessedTasksException(_queue.Count);
		}

		private void ThreadProcess(object ct)
		{
			try
			{
				var cancellationToken = (CancellationToken) ct;
				while (true)
				{
					cancellationToken.ThrowIfCancellationRequested();
					_semaphoreSlim.Wait(cancellationToken);
					var t = _queue.Take();
					TryExecuteTask(t);
				}
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("Thread finished due to cancellation.");
			}
		}

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return _queue;
		}

		protected override void QueueTask(Task task)
		{
			if(_isDisposed)
				throw new ObjectDisposedException(nameof(CustomTaskScheduler));
			_queue.Add(task);
			_semaphoreSlim.Release(1);
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return false;
		}
	}

	public class UnprocessedTasksException: Exception
	{
		public UnprocessedTasksException(int count): base($"There are {count} unprocessed tasks.")
		{
		}
	}
}