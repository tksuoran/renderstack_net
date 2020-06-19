using System;
using System.Collections.Generic;
using RenderStack.Math;

namespace example.VoxelRenderer
{
    public partial class Map
    {
        public bool Intersect(
            Vector3 start, 
            Vector3 end, 
            out IVector3 p,
            out IVector3 facing
        )
        {
            Vector3 direction = end - start;
            float intervalMin = 0.0f;
            float intervalMax = direction.Length;
            direction = Vector3.Normalize(direction);
            float t1;
            float t2;

            p.X = 0;
            p.Y = 0;
            p.Z = 0;
            facing.X = 0;
            facing.Y = 0;
            facing.Z = 0;

            float orgX = start.X;
            float dirX = direction.X;
            float invDirX = 1 / dirX;
            t1 = (bounds.Min.X - orgX) * invDirX;
            t2 = (bounds.Max.X - orgX) * invDirX;
            if(invDirX > 0)
            {
                if(t1 > intervalMin)
                {
                    intervalMin = t1;
                }
                if(t2 < intervalMax)
                {
                    intervalMax = t2;
                }
            }
            else
            {
                if(t2 > intervalMin)
                {
                    intervalMin = t2;
                }
                if(t1 < intervalMax)
                {
                    intervalMax = t1;
                }
            }
            if(intervalMin > intervalMax)
            {
                return false;
            }

            float orgY = start.Y;
            float dirY = direction.Y;
            float invDirY = 1 / dirY;
            t1 = (bounds.Min.Y - orgY) * invDirY;
            t2 = (bounds.Max.Y - orgY) * invDirY;
            if(invDirY > 0)
            {
                if(t1 > intervalMin)
                {
                    intervalMin = t1;
                }
                if(t2 < intervalMax)
                {
                    intervalMax = t2;
                }
            }
            else
            {
                if(t2 > intervalMin)
                {
                    intervalMin = t2;
                }
                if(t1 < intervalMax)
                {
                    intervalMax = t1;
                }
            }
            if(intervalMin > intervalMax)
            {
                return false;
            }

            float orgZ = start.Z;
            float dirZ = direction.Z;
            float invDirZ = 1 / dirZ;
            t1 = (bounds.Min.Z - orgZ) * invDirZ;
            t2 = (bounds.Max.Z - orgZ) * invDirZ;
            if(invDirZ > 0)
            {
                if(t1 > intervalMin)
                {
                    intervalMin = t1;
                }
                if(t2 < intervalMax)
                {
                    intervalMax = t2;
                }
            }
            else
            {
                if(t2 > intervalMin)
                {
                    intervalMin = t2;
                }
                if(t1 < intervalMax)
                {
                    intervalMax = t1;
                }
            }
            if(intervalMin > intervalMax)
            {
                return false;
            }

            // box is hit at [intervalMin, intervalMax]
            orgX += intervalMin * dirX;
            orgY += intervalMin * dirY;
            orgZ += intervalMin * dirZ;

            // locate starting point inside the grid
            // and set up 3D-DDA vars
            int indxX;
            int indxY;
            int indxZ;
            int stepX;
            int stepY;
            int stepZ;
            int stopX;
            int stopY;
            int stopZ;
            float deltaX;
            float deltaY;
            float deltaZ;
            float tnextX;
            float tnextY;
            float tnextZ;

            // stepping factors along X
            indxX = (int)((orgX - bounds.Min.X));
            if(indxX < 0)
            {
                indxX = 0;
            }
            /*else if(indxX >= nx)
            {
                indxX = nx - 1;
            }*/
            if(Math.Abs(dirX) < 1e-6f)
            {
                stepX = 0;
                stopX = indxX;
                deltaX = 0;
                tnextX = float.PositiveInfinity;
            }
            else if(dirX > 0)
            {
                stepX = 1;
                stopX = (int)(bounds).Max.X; //
                deltaX = invDirX;
                tnextX = intervalMin + ((indxX + 1) + bounds.Min.X - orgX) * invDirX;
            }
            else
            {
                stepX = -1;
                stopX = -1;
                deltaX = -invDirX;
                tnextX = intervalMin + (indxX + bounds.Min.X - orgX) * invDirX;
            }

            // stepping factors along Y
            indxY = (int)((orgY - bounds.Min.Y));
            if(indxY < 0)
            {
                indxY = 0;
            }
            else if(indxY >= 128)
            {
                indxY = 127;
            }
            if(Math.Abs(dirY) < 1e-6f)
            {
                stepY = 0;
                stopY = indxY;
                deltaY = 0;
                tnextY = float.PositiveInfinity;
            }
            else if(dirY > 0)
            {
                stepY = 1;
                stopY = 128;  //
                deltaY = invDirY;
                tnextY = intervalMin + ((indxY + 1) + bounds.Min.Y - orgY) * invDirY;
            }
            else
            {
                stepY = -1;
                stopY = -1;
                deltaY = -invDirY;
                tnextY = intervalMin + (indxY + bounds.Min.Y - orgY) * invDirY;
            }

            // stepping factors along Z
            indxZ = (int)((orgZ - bounds.Min.Z));
            if(indxZ < 0)
            {
                indxZ = 0;
            }
            /*else if(indxZ >= nz)
            {
                indxZ = nz - 1;
            }*/
            if(Math.Abs(dirZ) < 1e-6f)
            {
                stepZ = 0;
                stopZ = indxZ;
                deltaZ = 0;
                tnextZ = float.PositiveInfinity;
            }
            else if(dirZ > 0)
            {
                stepZ = 1;
                stopZ = (int)(bounds).Max.Z; //
                deltaZ = invDirZ;
                tnextZ = intervalMin + ((indxZ + 1) + bounds.Min.Z - orgZ) * invDirZ;
            }
            else
            {
                stepZ = -1;
                stopZ = -1;
                deltaZ = -invDirZ;
                tnextZ = intervalMin + (indxZ + bounds.Min.Z - orgZ) * invDirZ;
            }

            IVector3 facingX = new IVector3(-stepX, 0, 0);
            IVector3 facingY = new IVector3(0, -stepY, 0);
            IVector3 facingZ = new IVector3(0, 0, -stepZ);

            // trace through the grid
            for(;;)
            {
                if(tnextX < tnextY && tnextX < tnextZ)
                {
                    if(this[indxX, (byte)indxY, indxZ] > 0)
                    {
                        p.X = indxX;
                        p.Y = indxY;
                        p.Z = indxZ;
                        return true;
                    }
                    intervalMin = tnextX;
                    if(intervalMin > intervalMax)
                    {
                        return false;
                    }
                    indxX += stepX;
                    facing = facingX;
                    if(indxX == stopX)
                    {
                        return false;
                    }
                    tnextX += deltaX;
                }
                else if(tnextY < tnextZ)
                {
                    if(this[indxX, (byte)indxY, indxZ] > 0)
                    {
                        p.X = indxX;
                        p.Y = indxY;
                        p.Z = indxZ;
                        return true;
                    }
                    intervalMin = tnextY;
                    if(intervalMin > intervalMax)
                    {
                        return false;
                    }
                    indxY += stepY;
                    facing = facingY;
                    if(indxY == stopY)
                    {
                        return false;
                    }
                    tnextY += deltaY;
                }
                else
                {
                    if(this[indxX, (byte)indxY, indxZ] > 0)
                    {
                        p.X = indxX;
                        p.Y = indxY;
                        p.Z = indxZ;
                        return true;
                    }
                    intervalMin = tnextZ;
                    if(intervalMin > intervalMax)
                    {
                        return false;
                    }
                    indxZ += stepZ;
                    facing = facingZ;
                    if(indxZ == stopZ)
                    {
                        return false;
                    }
                    tnextZ += deltaZ;
                }
            }
        }
    }
}