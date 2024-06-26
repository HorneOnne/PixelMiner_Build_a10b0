using UnityEngine;
using System.Collections.Generic;
namespace PixelMiner.DataStructure
{
    [System.Serializable]
    public struct AStarNode
    {
        public Vector3Int GlobalPosition;
        public bool Walkable;
        public float GCost;   // The cost of the path from start to current node
        public float HCost;   // The total cost of the node, calc as (G + H) cost (H: Heuristic cost)
        public Vector3Int ParentGlobalPos;

        public float FCost { get => GCost + HCost; }

       

        /*
         * gcost (Goal cost): 
         *         This is the cost of the path from start node to the current node. It represents thhe actual cost to
         *         reach the current node from the start. For example, in a grid, this could be the number of steps it takes
         *         to reach the current node from the start node.
         *         
         * 
         * hcost (Heuristic cost):
         *          This is and estimate the cost from the current node to the goal. It's a kind of smart guess because
         *          we really don't know the actual distance until we find the path, as all sorts of things can be in the way
         *          (walls, waters, ect).
         *          
         * fcost (Final cost):
         *          This is the total cost of the node, calculated as the sum of the G Cost and H Cost.
         *          F Cost is used to determine which node to process next. The node with the lowest F Cost is selected
         *          as the next node to process. Fomula: [fcost = gcost + hcost].
         */


        public override bool Equals(object obj)
        {
            if (obj is AStarNode otherNode)
            {
                return this.GlobalPosition.Equals(otherNode.GlobalPosition);
            }
            return false;
        }


        public override int GetHashCode()
        {
            return GlobalPosition.GetHashCode();
        }

        public override string ToString()
        {
            return $"{GlobalPosition}";
        }

    }

    public class NodeComparer : IComparer<AStarNode>
    {
        public static NodeComparer Instance { get; } = new();
        public int Compare(AStarNode a, AStarNode b)
        {
            // Compare fCost first
            int compareF = a.FCost.CompareTo(b.FCost);
            if (compareF != 0)
            {
                return compareF;
            }
            // If fCosts are equal, compare hCost
            return a.HCost.CompareTo(b.HCost);
        }
    }
}
