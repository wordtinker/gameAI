using System;
using System.Collections.Generic;
using System.Linq;

namespace UniformedGraphSearches
{
    enum NodeIndex
    {
        Invalid = -1
    }

    class GraphNode
    {
        //every node has an index. A valid index is >= 0
        public int Index { get; set; }
        public GraphNode(int index = (int)NodeIndex.Invalid)
        {
            Index = index;
        }
    }

    class GraphEdge
    {
        //An edge connects two nodes. Valid node indexes are always positive.
        public int From { get; set; }
        public int To { get; set; }
        //the cost of traversing the edge
        public double Cost { get; set; }
        public GraphEdge() : this((int)NodeIndex.Invalid, (int)NodeIndex.Invalid, 1.0) { }
        public GraphEdge(int from=(int)NodeIndex.Invalid, int to=(int)NodeIndex.Invalid, double cost=1.0)
        {
            From = from;
            To = to;
            Cost = cost;
        }
    }

    class SparseGraph<TNode, TEdge>
        where TNode : GraphNode
        where TEdge : GraphEdge, new()
    {
        //the nodes that comprise this graph
        // cant remove node directly, it will mess indices,
        // set NodeIndex.Invalid on removal
        private List<TNode> nodes = new List<TNode>();
        //a vector of adjacency edge lists. (each node index keys into the
        //list of edges associated with that node)
        private List<List<TEdge>> edges = new List<List<TEdge>>();

        //returns true if the graph is directed
        public bool IsDigraph { get; private set; }
        //the index of the next node to be added
        public int NextNodeIndex { get; private set; }
        //returns the number of active + inactive nodes present in the graph
        public int NumNodes { get { return nodes.Count; } }
        //returns the number of active nodes present in the graph
        public int NumActiveNodes { get { return nodes.Count(n => n.Index != (int)NodeIndex.Invalid); } }
        //returns true if the graph contains no nodes
        public bool IsEmpty { get { return nodes.Count == 0; } }
        //returns the number of edges present in the graph
        public int NumEdges { get { return edges.Sum(e => e.Count); } }

        public SparseGraph(bool digraph)
        {
            IsDigraph = digraph;
            NextNodeIndex = 0;
        }
        
        //returns the node at the given index
        public TNode GetNode(int idx)
        {
            try
            {
                return nodes[idx];
            }
            catch (Exception)
            {
                // if index is out of range
                return null;
            }
        }
        //const method for obtaining a reference to an edge
        public TEdge GetEdge(int from, int to)
        {
            try
            {
                return edges[from].First(e => e.To == to);
            }
            catch (Exception)
            {
                return null;
            }
        }
        //adds a node to the graph
        public bool AddNode(TNode node)
        {
            if (node.Index < nodes.Count)
            {
                //make sure the client is not trying to add a node with the same ID as
                //a currently active node
                if (nodes[node.Index].Index == (int)NodeIndex.Invalid)
                {
                    // replace node
                    nodes[node.Index] = node;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //make sure the new node has been indexed correctly
                if (node.Index == NextNodeIndex)
                {
                    nodes.Add(node);
                    edges.Add(new List<TEdge>());
                    NextNodeIndex++;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        //removes a node by setting its index to invalid_node_index
        public void RemoveNode(int idx)
        {
            try
            {
                // mark node as deleted
                nodes[idx].Index = (int)NodeIndex.Invalid;
                //if the graph is not directed remove all edges leading to this node and then
                //clear the edges leading from the node
                if (!IsDigraph)
                {
                    //visit each neighbour and erase any edges leading to this node
                    List<TEdge> edgestoDelete = Edges(idx).ToList();
                    foreach (var edge in edgestoDelete)
                    {
                        RemoveEdge(edge.To, edge.From);
                    }
                }
                //if a digraph remove the edges the slow way
                else
                {
                    CullInvalidEdges();
                }
            }
            catch (Exception) { }
            
        }
        private void CullInvalidEdges()
        {
            // iterates through all the edges in the graph and removes any that point
            //  to an invalidated node
            for (int i = edges.Count - 1; i >= 0; i--)
            {
                for (int j = edges[i].Count - 1; j >= 0; j--)
                {
                    TEdge edge = edges[i][j];
                    if (nodes[edge.From].Index == (int)NodeIndex.Invalid ||
                    nodes[edge.To].Index == (int)NodeIndex.Invalid)
                    {
                        edges[i].RemoveAt(j);
                    }
                }
            }
        }
        //methods to add and remove edges
        public void AddEdge(TEdge edge)
        {
            //first make sure the from and to nodes exist within the graph 
            if (edge.From >= NextNodeIndex || edge.To >= NextNodeIndex) return;

            //make sure both nodes are active before adding the edge
            if (nodes[edge.To].Index != (int)NodeIndex.Invalid &&
                nodes[edge.From].Index != (int)NodeIndex.Invalid)
            {
                // add the edge, first making sure it is unique
                if (GetEdge(edge.From, edge.To) == null)
                {
                    edges[edge.From].Add(edge);
                }
                //if the graph is undirected we must add another connection in the opposite
                //direction
                if (!IsDigraph)
                {
                    //check to make sure the edge is unique before adding
                    if (GetEdge(edge.To, edge.From) == null)
                    {
                        TEdge newEdge = new TEdge()
                        {
                            To = edge.From,
                            From = edge.To,
                            Cost = edge.Cost
                        };
                        edges[edge.To].Add(newEdge);
                    }
                }
            } 
        }
        public void RemoveEdge(int from, int to)
        {
            TEdge edgeToRemove;
            if (!IsDigraph)
            {
                // remove reverse edge either
                edgeToRemove = GetEdge(to, from);
                if (edgeToRemove != null)
                {
                    edges[to].Remove(edgeToRemove);
                }
            }
            // remove direct edge
            edgeToRemove = GetEdge(from, to);
            if (edgeToRemove != null)
            {
                edges[from].Remove(edgeToRemove);
            }
        }
        //clears the graph ready for new node insertions
        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
            NextNodeIndex = 0;
        }
        //iterators clients may use to access nodes and edges
        public IEnumerable<TNode> Nodes
        {
            get
            {
                return nodes.Where(n => n.Index != (int)NodeIndex.Invalid);
            }
        }
        public IEnumerable<TEdge> Edges(int nodeIndex)
        {
            try
            {
                return edges[nodeIndex]?.AsEnumerable();
            }
            catch (Exception)
            {
                return Enumerable.Empty<TEdge>();
            }
        }
    }

    static class Printer
    {
        public static void Print<TNode, TEdge>(this SparseGraph<TNode, TEdge> graph)
            where TNode : GraphNode
            where TEdge : GraphEdge, new()
        {
            Console.WriteLine($"Graph is directional: {graph.IsDigraph}");
            Console.WriteLine($"Graph is empty: {graph.IsEmpty}");
            Console.WriteLine($"Next available slot: {graph.NextNodeIndex}");
            Console.WriteLine($"Number of nodes: {graph.NumNodes}");
            Console.WriteLine($"Number of active nodes: {graph.NumActiveNodes}");
            Console.WriteLine($"Number of edges: {graph.NumEdges}");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Nodes:");
            foreach (TNode node in graph.Nodes)
            {
                Console.Write($"{node.Index}: ");
                foreach (TEdge edge in graph.Edges(node.Index))
                {
                    Console.WriteLine($"\t{edge.From}->{edge.To}::{edge.Cost}");
                }
                Console.WriteLine();
            }
            Console.WriteLine("----------------------------");
        }
        public static void Print<TNode, TEdge>(this SearchResult<TNode, TEdge> result)
            where TNode : GraphNode
            where TEdge : GraphEdge
        {
            Console.WriteLine($"Have been searching path from {result.Source} to {result.Target}.");
            Console.WriteLine("Path that has been found:");
            foreach (int node in result.PathToTarget)
            {
                Console.Write($"{node} ");
            }
            Console.WriteLine();
            Console.WriteLine("Visited nodes:");
            foreach (TEdge edge in result.SpanningTree)
            {
                Console.WriteLine($"\t{edge.From} -> {edge.To}, spend {edge.Cost}");
            }
        }
    }

    class SearchResult<TNode,TEdge>
    {
        public int Source { get; internal set; }
        public int Target { get; internal set; }
        // true if a path to the target has been found
        public bool Found { get; internal set; }
        // returns a vector containing all the edges the search has examined
        public List<TEdge> SpanningTree { get; } = new List<TEdge>();
        // returns a list of node indexes that comprise the shortest path
        // from the source to the target
        public List<int> PathToTarget { get; } = new List<int>();  
    }

    /// <summary>
    /// Depth-first search (DFS) is an algorithm for traversing or searching tree or graph data structures.
    /// One starts at the root (selecting some arbitrary node as the root in the case of a graph)
    /// and explores as far as possible along each branch before backtracking.
    /// </summary>
    static class DFS
    {
        private enum Visit
        {
            Unvisited = 0,
            Visited
        }

        // this records the indexes of all the nodes that are visited as the
        // search progresses. As the search progresses,
        // every time a node is visited its corresponding element will be
        // set to visited.
        private static Visit[] visited;
        // As the graph
        // search proceeds, this vector stores the route to the target node by recording
        // the parents of each node at the relevant index.For example, if the path to
        // the target follows the nodes 3 - 8 - 27, then m_Route[8] will hold 3 and
        // m_Route[27] will hold 8.
        private static int[] route;

        private static void SetPathToTarget<TNode, TEdge>(SearchResult<TNode, TEdge> result, int source, int target)
        {
            int node = target;
            result.PathToTarget.Add(node);
            while (node != source)
            {
                node = route[node];
                result.PathToTarget.Insert(0, node);
            }
        }

        public static SearchResult<TNode, TEdge> SearchDFS<TNode, TEdge>(this SparseGraph<TNode, TEdge> graph, int source, int target = -1)
            where TNode : GraphNode
            where TEdge : GraphEdge, new()
        {
            // reset search conditions
            visited = new Visit[graph.NumNodes];
            route = Enumerable.Repeat(-1, graph.NumNodes).ToArray();
            SearchResult<TNode, TEdge> result = new SearchResult<TNode, TEdge> {Source = source, Target = target };
            // Create a stack of edges
            Stack<TEdge> stack = new Stack<TEdge>();
            // create a dummy edge and put on the stack
            TEdge dummy = new TEdge { From = source, To = source, Cost = 0 };
            stack.Push(dummy);
            // while there are edges in the stack keep searching
            while (stack.Count > 0)
            {
                // grab the next edge
                TEdge next = stack.Pop();
                // make a note of the parent of the node this edge points to
                route[next.To] = next.From;
                // put it on the tree. (making sure the dummy edge is not placed on the tree)
                if (next != dummy)
                {
                    result.SpanningTree.Add(next);
                }
                // and mark it visited
                visited[next.To] = Visit.Visited;
                // if the target has been found the method can return success
                if (next.To == target)
                {
                    result.Found = true;
                    SetPathToTarget(result, source, target);
                    return result;
                }
                // push the edges leading from the node this edge points to onto
                // the stack (provided the edge does not point to a previously 
                // visited node)
                foreach (TEdge edge in graph.Edges(next.To))
                {
                    if (visited[edge.To] == Visit.Unvisited)
                    {
                        stack.Push(edge);
                    }
                }
            }
            // no path to target
            return result;
        }
    }

    /// <summary>
    /// The BFS algorithm fans out from the source node and examines each of
    /// the nodes its edges lead to before fanning out from those nodes and examining
    /// all the edges they connect to and so on.You can think of the search as
    /// exploring all the nodes that are one edge away from the source node, then
    /// all the nodes two edges away, then three edges, and so on until the target
    /// node is found.
    /// </summary>
    static class BFS
    {
        private enum Visit
        {
            Unvisited = 0,
            Visited
        }

        // this records the indexes of all the nodes that are visited as the
        // search progresses. As the search progresses,
        // every time a node is visited its corresponding element will be
        // set to visited.
        private static Visit[] visited;
        // As the graph
        // search proceeds, this vector stores the route to the target node by recording
        // the parents of each node at the relevant index.For example, if the path to
        // the target follows the nodes 3 - 8 - 27, then m_Route[8] will hold 3 and
        // m_Route[27] will hold 8.
        private static int[] route;

        private static void SetPathToTarget<TNode, TEdge>(SearchResult<TNode, TEdge> result, int source, int target)
        {
            int node = target;
            result.PathToTarget.Add(node);
            while (node != source)
            {
                node = route[node];
                result.PathToTarget.Insert(0, node);
            }
        }

        /// <summary>
        /// The algorithm for BFS is almost exactly the same as for DFS except it uses
        /// a first in, first out (FIFO) queue instead of a stack.
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <typeparam name="TEdge"></typeparam>
        /// <param name="graph"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static SearchResult<TNode, TEdge> SearchBFS<TNode, TEdge>(this SparseGraph<TNode, TEdge> graph, int source, int target = -1)
            where TNode : GraphNode
            where TEdge : GraphEdge, new()
        {
            // reset search conditions
            visited = new Visit[graph.NumNodes];
            route = Enumerable.Repeat(-1, graph.NumNodes).ToArray();
            SearchResult<TNode, TEdge> result = new SearchResult<TNode, TEdge> { Source = source, Target = target };
            // Create a queue of edges
            Queue<TEdge> queue = new Queue<TEdge>();
            // create a dummy edge and put on the queue
            TEdge dummy = new TEdge { From = source, To = source, Cost = 0 };
            queue.Enqueue(dummy);
            //mark the source node as visited
            visited[source] = Visit.Visited;
            // while there are edges in the stack keep searching
            while (queue.Count > 0)
            {
                // grab the next edge
                TEdge next = queue.Dequeue();
                // make a note of the parent of the node this edge points to
                route[next.To] = next.From;
                // put it on the tree. (making sure the dummy edge is not placed on the tree)
                if (next != dummy)
                {
                    result.SpanningTree.Add(next);
                }
                // if the target has been found the method can return success
                if (next.To == target)
                {
                    result.Found = true;
                    SetPathToTarget(result, source, target);
                    return result;
                }

                // push the edges leading from the node this edge points to onto
                // the queue (provided the edge does not point to a previously 
                // visited node)
                foreach (TEdge edge in graph.Edges(next.To))
                {
                    if (visited[edge.To] == Visit.Unvisited)
                    {
                        queue.Enqueue(edge);
                        visited[edge.To] = Visit.Visited;
                    }
                }
            }
            // no path to target
            return result;
        }
    }

    class Program
    {
        // Uninformed graph searches, or blind searches as they are sometimes
        // known, search a graph without regard to any associated edge costs.They
        // can distinguish individual nodes and edges however, enabling them to
        // identify a target node or to recognize previously visited nodes or edges.
        // This is the only information required to either completely explore a graph
        // (to visit every node) or find a path between two nodes.
        static void Main(string[] args)
        {
            // Given a source node, the depth first search can only guarantee that
            // all the nodes and edges will be visited in a connected graph.
            SparseGraph<GraphNode, GraphEdge> g = new SparseGraph<GraphNode, GraphEdge>(false);
            g.AddNode(new GraphNode(0));
            g.AddNode(new GraphNode(1));
            g.AddNode(new GraphNode(2));
            g.AddNode(new GraphNode(3));
            g.AddNode(new GraphNode(4));
            g.AddNode(new GraphNode(5));

            g.AddEdge(new GraphEdge(0, 1));
            g.AddEdge(new GraphEdge(0, 2));
            g.AddEdge(new GraphEdge(1, 4));
            g.AddEdge(new GraphEdge(2, 3));
            g.AddEdge(new GraphEdge(3, 4));
            g.AddEdge(new GraphEdge(4, 5));
            g.AddEdge(new GraphEdge(3, 5));
            //
            var dfs = g.SearchDFS(4, 2);
            dfs.Print();
            var bfs = g.SearchBFS(4, 2);
            bfs.Print();
        }
    }
}
