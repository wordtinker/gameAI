using System;
using System.Collections.Generic;
using System.Linq;

namespace DFS
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
        //An edge connects two nodes. Valid node indices are always positive.
        public int From { get; set; }
        public int To { get; set; }
        //the cost of traversing the edge
        public double Cost { get; set; }
        public GraphEdge() : this((int)NodeIndex.Invalid, (int)NodeIndex.Invalid,1.0) { }
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
    }

    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}
