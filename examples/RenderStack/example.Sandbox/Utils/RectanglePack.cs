namespace example.Sandbox
{
    //  Adapted from http://clb.demon.fi/projects/even-more-rectangle-bin-packing
    public class RectangleBinPack
    {
        public class Node
        {
            public Node    Left;
            public Node    Right;
            public float   X;
            public float   Y;
            public float   Width;
            public float   Height;
        };

        private Node    root = new Node();
        float           binWidth;
        float           binHeight;

        public RectangleBinPack(float width, float height)
        {
            binWidth    = width;
            binHeight   = height;
            root.Left   = null;
            root.Right  = null;
            root.X      = 0.0f;
            root.Y      = 0.0f;
            root.Width  = width;
            root.Height = height;
        }

        public Node Insert(float width, float height)
        {
            return Insert(root, width, height);
        }

        public Node Insert(Node node, float width, float height)
        {
            // If this node is an internal node, try both leaves for possible space.
            // (The rectangle in an internal node stores used space, the leaves store free space)
            if(node.Left != null || node.Right != null)
            {
                if(node.Left != null)
                {
                    Node newNode = Insert(node.Left, width, height);
                    if(newNode != null)
                    {
                        return newNode;
                    }
                }
                if(node.Right != null)
                {
                    Node newNode = Insert(node.Right, width, height);
                    if(newNode != null)
                    {
                        return newNode;
                    }
                }
                return null; // Didn't fit into either subtree!
            }

            // This node is a leaf, but can we fit the new rectangle here?
            if(width > node.Width || height > node.Height)
            {
                return null; // Too bad, no space.
            }

            // The new cell will fit, split the remaining space along the shorter axis,
            // that is probably more optimal.
            float w     = node.Width - width;
            float h     = node.Height - height;
            node.Left   = new Node();
            node.Right  = new Node();

            if(w <= h) // Split the remaining space in horizontal direction.
            {
                node.Left.X         = node.X + width;
                node.Left.Y         = node.Y;
                node.Left.Width     = w;
                node.Left.Height    = height;

                node.Right.X        = node.X;
                node.Right.Y        = node.Y + height;
                node.Right.Width    = node.Width;
                node.Right.Height   = h;
            }
            else // Split the remaining space in vertical direction.
            {
                node.Left.X         = node.X;
                node.Left.Y         = node.Y + height;
                node.Left.Width     = width;
                node.Left.Height    = h;

                node.Right.X        = node.X + width;
                node.Right.Y        = node.Y;
                node.Right.Width    = w;
                node.Right.Height   = node.Height;
            }

            // Note that as a result of the above, it can happen that node.Left or node.Right
            // is now a degenerate (zero area) rectangle. No need to do anything about it,
            // like remove the nodes as "unnecessary" since they need to exist as children of
            // this node (this node can't be a leaf anymore).

            // This node is now a non-leaf, so shrink its area - it now denotes
            // *occupied* space instead of free space. Its children spawn the resulting
            // area of free space.
            node.Width = width;
            node.Height = height;
            return node;
        }

        public float Occupancy()
        {
            float totalSurfaceArea = binWidth * binHeight;
            float usedSurfaceArea = UsedSurfaceArea(root);

            return usedSurfaceArea / totalSurfaceArea;
        }

        public float UsedSurfaceArea(Node node)
        {
            if(node.Left != null || node.Right != null)
            {
                float usedSurfaceArea = node.Width * node.Height;
                if(node.Left != null) usedSurfaceArea += UsedSurfaceArea(node.Left);
                if(node.Right != null) usedSurfaceArea += UsedSurfaceArea(node.Right);

                return usedSurfaceArea;
            }

            // This is a leaf node, it doesn't constitute to the total surface area.
            return 0.0f;
        }


    }
}



