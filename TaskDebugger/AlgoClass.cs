using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskDebugger
{
	public class AlgoClass
	{
		public AlgoClass(CustomTaskScheduler scheduler)
		{
			_scheduler = scheduler;
		}

		private readonly CustomTaskScheduler _scheduler;

		public Task ProcessInParallelAsync(Graph<Action> actions)
		{
			var inverseGraph = InverseGraph(actions);

			var tasks = StartExecutionAsync(inverseGraph);
			return Task.WhenAll(tasks);
		}

		private static Graph<Action> InverseGraph(Graph<Action> graph)
		{
			var inverseGraph = new Graph<Action>();
			var stack = new Stack<Node<Action>>(graph.HeadNodes);
			while (stack.Count > 0)
			{
				var rootNode = stack.Pop();
				graph.TryGetChildren(rootNode, out var children);
				foreach (var child in children)
				{
					inverseGraph.AddChild(child, rootNode);
					if (graph.TryGetChildren(child, out _))
						stack.Push(child);
					else
						inverseGraph.AddHeadNode(child);
				}
			}

			return inverseGraph;
		}

		private async Task StartExecutionAsync(Graph<Action> graph)
		{
			var taskGraph = new Dictionary<Node<Action>, OneTimeExecutor>();
			foreach (var (key, children) in graph.GraphList)
			{
				async Task WaitChildrenAndRun()
				{
					var awaitableChildren = children.Select(x => taskGraph[x].Start());
					await Task.WhenAll(awaitableChildren);
					var currentTask = new Task(key.Value);
					currentTask.Start(_scheduler);
					await currentTask;
				}

				taskGraph[key] = new OneTimeExecutor(WaitChildrenAndRun);
			}

			var tasks = graph.HeadNodes.Select(x => taskGraph[x].Start());
			await Task.WhenAll(tasks);
		}

		private class OneTimeExecutor
		{
			private bool _isStarted;
			private readonly Func<Task> _expression;
			private readonly object _obj = new object();
			private Task _task;

			public OneTimeExecutor(Func<Task> expression)
			{
				_isStarted = false;
				_expression = expression;
			}

			public Task Start()
			{
				lock (_obj)
				{
					if (_isStarted) return _task;
					_task = _expression();
					_isStarted = true;
					return _task;
				}
			}

		}
	}
}