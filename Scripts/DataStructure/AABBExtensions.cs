using UnityEngine;

namespace PixelMiner.DataStructure
{
    public static class AABBExtensions
    {
        //public static int SweptAABB(AABB dynamicBox, AABB staticBox, Vector3 vel, out float normalX, out float normalY, out float normalZ, out float entryTime)
        //{
        //    float xEntry, yEntry, zEntry; // Specify how far away the closest edges of the objects are from each other.
        //    float xExit, yExit, zExit;   // The distance to the far side of the object

        //    // Find the distance between the objects on the near and far sides for both x and y
        //    if (vel.x > 0.0f)
        //    {
        //        xEntry = staticBox.x - (dynamicBox.x + dynamicBox.w);
        //        xExit = (staticBox.x + staticBox.w) - dynamicBox.x;
        //    }
        //    else
        //    {
        //        xEntry = dynamicBox.x - (staticBox.x + staticBox.w);
        //        xExit = (dynamicBox.x + dynamicBox.w) - staticBox.x;
        //    }

        //    if (vel.y > 0.0f)
        //    {
        //        yEntry = staticBox.y - (dynamicBox.y + dynamicBox.h);
        //        yExit = (staticBox.y + staticBox.h) - dynamicBox.y;
        //    }
        //    else
        //    {
        //        yEntry = dynamicBox.y - (staticBox.y + staticBox.h);
        //        yExit = (dynamicBox.y + dynamicBox.h) - staticBox.y;
        //    }

        //    if (vel.z > 0.0f)
        //    {
        //        zEntry = staticBox.z - (dynamicBox.z + dynamicBox.d);
        //        zExit = (staticBox.z + staticBox.d) - dynamicBox.z;
        //    }
        //    else
        //    {
        //        zEntry = dynamicBox.z - (staticBox.z + staticBox.d);
        //        zExit = (dynamicBox.z + dynamicBox.d) - staticBox.z;
        //    }


        //    // Find time of the collision and time leaving for each axis
        //    float xTimeEntry, yTimeEntry, zTimeEntry;
        //    float xTimeExit, yTimeExit, zTimeExit;

        //    if (vel.x == 0.0f)
        //    {
        //        xTimeEntry = float.NegativeInfinity;
        //        xTimeExit = float.PositiveInfinity;
        //        //Debug.Log("A");
        //    }
        //    else
        //    {
        //        xTimeEntry = xEntry / Mathf.Abs(vel.x);
        //        xTimeExit = xExit / Mathf.Abs(vel.x);
        //        // Debug.Log("B");
        //    }

        //    if (vel.y == 0.0f)
        //    {
        //        yTimeEntry = float.NegativeInfinity;
        //        yTimeExit = float.PositiveInfinity;
        //        //Debug.Log("C");
        //    }
        //    else
        //    {
        //        yTimeEntry = yEntry / Mathf.Abs(vel.y);
        //        yTimeExit = yExit / Mathf.Abs(vel.y);
        //        //Debug.Log("D");
        //    }



        //    if (vel.z == 0.0f)
        //    {
        //        zTimeEntry = float.NegativeInfinity;
        //        zTimeExit = float.PositiveInfinity;
        //        //Debug.Log("E");
        //    }
        //    else
        //    {
        //        zTimeEntry = zEntry / Mathf.Abs(vel.z);
        //        zTimeExit = zExit / Mathf.Abs(vel.z);
        //        //Debug.Log("F");
        //    }

        //    // Find the earliest/lastest times of collision
        //    //float entryTime = Mathf.Max(xEntry, yEntry);
        //    entryTime = Mathf.Max(xTimeEntry, yTimeEntry, zTimeEntry);       // entryTime tell when the collision occured
        //    float exitTime = Mathf.Min(xTimeExit, yTimeExit, zTimeExit);         // exitTime tell when collision exited the object from other side.
        //    //Debug.Log($"{entryTime} {xTimeEntry} {yTimeEntry} {zTimeEntry}");
        //    //Debug.Break();
        //    // If there was no collision
        //    if (entryTime > exitTime || xTimeEntry < 0.0f && yTimeEntry < 0.0f && zTimeEntry < 0.0f || xTimeEntry > 1.0f && yTimeEntry > 1.0f && zTimeEntry > 1.0f)
        //    {
        //        Debug.Log($"No collision");
        //        normalX = 0.0f;
        //        normalY = 0.0f;
        //        normalZ = 0.0f;
        //        return -1;
        //    }
        //    else      // if there was a collision
        //    {
        //        Debug.Log("Collision");
        //        // Calculate normal of collided surface
        //        if (xTimeEntry > yTimeEntry)
        //        {
        //            if (xTimeEntry > zTimeEntry)
        //            {
        //                if (xEntry < 0.0f)
        //                {
        //                    normalX = 1.0f;
        //                    normalY = 0.0f;
        //                    normalZ = 0.0f;
        //                }
        //                else
        //                {
        //                    normalX = -1.0f;
        //                    normalY = 0.0f;
        //                    normalZ = 0.0f;
        //                }
        //            }
        //            else
        //            {
        //                if (zEntry < 0.0f)
        //                {
        //                    normalX = 0.0f;
        //                    normalY = 0.0f;
        //                    normalZ = 1.0f;
        //                }
        //                else
        //                {
        //                    normalX = 0.0f;
        //                    normalY = 0.0f;
        //                    normalZ = -1.0f;
        //                }
        //            }

        //        }
        //        else
        //        {
        //            if (yTimeEntry > zTimeEntry)
        //            {
        //                if (yEntry < 0.0f)
        //                {
        //                    normalX = 0.0f;
        //                    normalY = 1.0f;
        //                    normalZ = 0.0f;
        //                }
        //                else
        //                {
        //                    normalX = 0.0f;
        //                    normalY = -1.0f;
        //                    normalZ = 0.0f;
        //                }
        //            }
        //            else
        //            {
        //                if (zEntry < 0.0f)
        //                {
        //                    normalX = 0.0f;
        //                    normalY = 0.0f;
        //                    normalZ = 1.0f;
        //                }
        //                else
        //                {
        //                    normalX = 0.0f;
        //                    normalY = 0.0f;
        //                    normalZ = -1.0f;
        //                }
        //            }

        //        }
        //    }   // return the time of collision return entryTime


        //    return 1;
        //}

        public static int SweepTest(Bounds dynamicBox, Bounds staticBox, Vector3 vel, out float time, out float normalX, out float normalY, out float normalZ)
        {
            float xEntry, yEntry, zEntry;
            float xExit, yExit, zExit;

            float xTimeEntry, yTimeEntry, zTimeEntry;
            float xTimeExit, yTimeExit, zTimeExit;


            if (vel.x > 0)
            {
                xEntry = staticBox.min.x - dynamicBox.max.x;
                xExit = staticBox.max.x - dynamicBox.min.x;
            }
            else
            {
                xEntry = staticBox.max.x - dynamicBox.min.x;
                xExit = staticBox.min.x - dynamicBox.max.x;
            }

            if (vel.y > 0)
            {
                yEntry = staticBox.min.y - dynamicBox.max.y;
                yExit = staticBox.max.y - dynamicBox.min.y;
            }
            else
            {
                yEntry = staticBox.max.y - dynamicBox.min.y;
                yExit = staticBox.min.y - dynamicBox.max.y;
            }

            if (vel.z > 0)
            {
                zEntry = staticBox.min.z - dynamicBox.max.z;
                zExit = staticBox.max.z - dynamicBox.min.z;
            }
            else
            {
                zEntry = staticBox.max.z - dynamicBox.min.z;
                zExit = staticBox.min.z - dynamicBox.max.z;
            }


            if (vel.x == 0)
            {
                xTimeEntry = float.NegativeInfinity;
                xTimeExit = float.PositiveInfinity;
            }
            else
            {
                xTimeEntry = xEntry / vel.x;
                xTimeExit = xExit / vel.x;
            }

            if (vel.y == 0)
            {
                yTimeEntry = float.NegativeInfinity;
                yTimeExit = float.PositiveInfinity;
            }
            else
            {
                yTimeEntry = yEntry / vel.y;
                yTimeExit = yExit / vel.y;
            }

            if (vel.z == 0)
            {
                zTimeEntry = float.NegativeInfinity;
                zTimeExit = float.PositiveInfinity;
            }
            else
            {
                zTimeEntry = zEntry / vel.z;
                zTimeExit = zExit / vel.z;
            }



            //Debug.Log($"TimeEntry: {xTimeEntry}  {zTimeEntry}");

            time = Mathf.Max(xTimeEntry, yTimeEntry, zTimeEntry);
            float timeExit = Mathf.Min(xTimeExit, yTimeExit, zTimeExit);


            //Debug.Log($"{xTimeEntry} {yTimeEntry} {zTimeEntry}");

            if (time > timeExit || (xTimeEntry < 0 && yTimeEntry < 0 && zTimeEntry < 0) || xTimeEntry > 1.0f || yTimeEntry > 1.0f || zTimeEntry > 1.0f)
            {
                // No collision
                normalX = 0.0f;
                normalY = 0.0f;
                normalZ = 0.0f;
                return -1;
            }
            else
            {
                if (xTimeEntry > yTimeEntry)
                {
                    if (xTimeEntry > zTimeEntry)
                    {
                        // X
                        if (xEntry < 0)
                        {
                            normalX = 1.0f;
                            normalY = 0.0f;
                            normalZ = 0.0f;
                        }
                        else
                        {
                            normalX = -1.0f;
                            normalY = 0.0f;
                            normalZ = 0.0f;
                        }
                        return 0;
                    }
                    else
                    {
                        // Z
                        if (zEntry < 0)
                        {
                            normalX = 0.0f;
                            normalY = 0.0f;
                            normalZ = 1.0f;
                        }
                        else
                        {
                            normalX = 0.0f;
                            normalY = 0.0f;
                            normalZ = -1.0f;
                        }
                        return 2;
                    }
                }
                else
                {
                    if (yTimeEntry > zTimeEntry)
                    {
                        // Y
                        if (yEntry < 0)
                        {
                            normalX = 0.0f;
                            normalY = 1.0f;
                            normalZ = 0.0f;
                        }
                        else if (yEntry > 0)
                        {
                            normalX = 0.0f;
                            normalY = -1.0f;
                            normalZ = 0.0f;
                        }
                        else
                        {
                            normalX = 0.0f;
                            normalY = 0.0f;
                            normalZ = 0.0f;
                        }

                        //if (yEntry < 0)
                        //{
                        //    normalX = 0.0f;
                        //    normalY = 1.0f;
                        //    normalZ = 0.0f;
                        //}
                        //else
                        //{
                        //    normalX = 0.0f;
                        //    normalY = -1.0f;
                        //    normalZ = 0.0f;
                        //}

                        return 1;
                    }
                    else
                    {
                        // Z
                        if (zEntry < 0)
                        {
                            normalX = 0.0f;
                            normalY = 0.0f;
                            normalZ = 1.0f;
                        }
                        else
                        {
                            normalX = 0.0f;
                            normalY = 0.0f;
                            normalZ = -1.0f;
                        }
                        return 2;
                    }
                }
            }
        }




        //public static AABB GetSweptBroadphaseBox(this AABB b, Vector3 vel)
        //{
        //    AABB broadphasebox = new AABB()
        //    {
        //        x = vel.x > 0 ? b.x : b.x + vel.x,
        //        y = vel.y > 0 ? b.y : b.y + vel.y,
        //        z = vel.z > 0 ? b.z : b.z + vel.z,
        //        w = vel.x > 0 ? vel.x + b.w : b.w - vel.x,
        //        h = vel.y > 0 ? vel.y + b.h : b.h - vel.y,
        //        d = vel.z > 0 ? vel.z + b.d : b.d - vel.z
        //    };


        //    return broadphasebox;
        //}

        public static Bounds GetSweptBroadphaseY(this Bounds b, Vector3 vel)
        {
            Vector3 center = b.center;
            Vector3 size = b.size;
         
            center.y += vel.y;


            Bounds broadphasebox = new Bounds(center, size);
            return broadphasebox;
        }

        //public static Bounds GetSweptBroadphaseX(this Bounds b, Vector3 vel)
        //{
        //    float x, w;
        //    if (vel.x > 0)
        //    {
        //        x = b.x + b.w;
        //        w = vel.x;
        //    }
        //    else if (vel.x < 0)
        //    {
        //        x = b.x + vel.x;
        //        w = -vel.x - 0.1f;
        //    }
        //    else
        //    {
        //        x = b.x;
        //        w = b.w;
        //    }

        //    AABB broadphasebox = new AABB()
        //    {
        //        x = x,
        //        y = b.y,
        //        z = b.z,
        //        w = w,
        //        h = b.h - 1e-3f,
        //        d = b.d - 1e-3f
        //    };

        //    return broadphasebox;
        //}
        //public static Bounds GetSweptBroadphaseZ(this Bounds b, Vector3 vel)
        //{
        //    float z, d;
        //    if (vel.z > 0)
        //    {
        //        z = b.z + b.d;
        //        d = vel.z;
        //    }
        //    else if (vel.z < 0)
        //    {
        //        z = b.z + vel.z;
        //        d = -vel.z - 0.1f;
        //    }
        //    else
        //    {
        //        z = b.z;
        //        d = b.d;
        //    }

        //    AABB broadphasebox = new AABB()
        //    {
        //        x = b.x,
        //        y = b.y,
        //        z = z,
        //        w = b.w - 1e-3f,
        //        h = b.h - 1e-3f,
        //        d = d
        //    };

        //    return broadphasebox;
        //}

        //public static bool AABBOverlapCheck(AABB b1, AABB b2)
        //{
        //    return !(b1.x + b1.w < b2.x || b1.x > b2.x + b2.w ||
        //            b1.y + b1.h < b2.y || b1.y > b2.y + b2.h ||
        //            b1.z + b1.d < b2.z || b1.z > b2.z + b2.d);
        //}

        //public static void AABBOverlapVolumnCheck(AABB box1, AABB box2, out float w, out float h, out float d)
        //{
        //    // Compute overlapping interval along the X-axis
        //    float xMin = Mathf.Max(box1.x, box2.x);
        //    float xMax = Mathf.Min(box1.x + box1.w, box2.x + box2.w);
        //    w = Mathf.Max(0, xMax - xMin);

        //    // Compute overlapping interval along the Y-axis
        //    float yMin = Mathf.Max(box1.y, box2.y);
        //    float yMax = Mathf.Min(box1.y + box1.h, box2.y + box2.h);
        //    h = Mathf.Max(0, yMax - yMin);

        //    // Compute overlapping interval along the Z-axis
        //    float zMin = Mathf.Max(box1.z, box2.z);
        //    float zMax = Mathf.Min(box1.z + box1.d, box2.z + box2.d);
        //    d = Mathf.Max(0, zMax - zMin);
        //}
    }
}
