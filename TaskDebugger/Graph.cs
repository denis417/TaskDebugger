using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskDebugger
{
	//We believe that graph is correct.
	public class Graph<T>
	{
		private readonly Dictionary<Node<T>, HashSet<Node<T>>> _graphList = new Dictionary<Node<T>, HashSet<Node<T>>>();
		private readonly HashSet<Node<T>> _headNodes = new HashSet<Node<T>>();

		public IEnumerable<Node<T>> HeadNodes => _headNodes.AsEnumerable();

		public IReadOnlyDictionary<Node<T>, HashSet<Node<T>>> GraphList => _graphList;

		public bool TryGetChildren(Node<T> node, out HashSet<Node<T>> children)
		{
			var hasChildren = _graphList.TryGetValue(node, out children);
			return hasChildren && children.Any();
		}

		public Node<T> AddChild(Node<T> headNode, Node<T> child)
		{
			if (_graphList.ContainsKey(headNode))
				_graphList[headNode].Add(child);
			else
				_graphList.Add(headNode, new HashSet<Node<T>> {child});

			if(!_graphList.ContainsKey(child))
				_graphList.Add(child, new HashSet<Node<T>>());
			return child;
		}

		public void AddHeadNode(Node<T> headNode)
		{
			_headNodes.Add(headNode);
		}

		public void ClearHeadNodes() => _headNodes.Clear();
	}

	public class Node<T>
	{
		private readonly Guid _id;

		public Node(T value, Guid? id = null)
		{
			Value = value;
			_id = id ?? Guid.NewGuid();
		}

		public T Value { get; }

		public override bool Equals(object obj)
		{
			var node = obj as Node<T>;
			if (node == null)
				return false;

			return _id == node._id;
		}

		public override int GetHashCode()
		{
			return _id.GetHashCode();
		}

		public override string ToString()
		{
			return _id.ToString();
		}
	}
}