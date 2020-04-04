using System;
using System.Threading;

namespace TaskDebugger
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Console.WriteLine($"Main Thread ID: {Thread.CurrentThread.ManagedThreadId}");

			var taskScheduler = new CustomTaskScheduler(3);
			var a = new AlgoClass(taskScheduler);

			var testGraph = InitTestGraph();
			var tasks = a.ProcessInParallelAsync(testGraph);
			tasks.Wait();
			taskScheduler.Dispose();
		}



		//TEST DATA

		//	   4  4  4
		//		\ | /
		//		  3
		//        ^
		//       / \
		//      2   2
		//	    ^   ^
		//       \ /
		//        1
		private static Graph<Action> InitTestGraph()
		{
			var graph = new Graph<Action>();
			var headNode = new Node<Action>(GetTestAction(1));
			var ch3 = new Node<Action>(GetTestAction(3));
			var ch21 = graph.AddChild(headNode, new Node<Action>(GetTestAction(2)));
			var ch22 = graph.AddChild(headNode, new Node<Action>(GetTestAction(2)));

			graph.AddChild(ch21, ch3);
			graph.AddChild(ch22, ch3);

			var ch41 = graph.AddChild(ch3, new Node<Action>(GetTestAction(4)));
			var ch42 = graph.AddChild(ch3, new Node<Action>(GetTestAction(4)));
			var ch43 = graph.AddChild(ch3, new Node<Action>(GetTestAction(4)));

			graph.AddHeadNode(headNode);
			return graph;
		}

		private static Action GetTestAction(int step)
		{
			return () =>
			{
				Thread.Sleep(1500);
				Console.WriteLine($"Thread: \"{Thread.CurrentThread.Name}\", step: {step}");
			};
		}
	}
}