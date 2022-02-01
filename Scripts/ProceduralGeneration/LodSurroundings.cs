using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Procedural.Terrain;
using UnityEngine;
using UnityToolbag;

namespace Procedural.Terrain
{
    [System.Serializable]
    public struct LodSurroundings
    {
        public int Self;
        public int Up;
        public int Down;
        public int Left;
        public int Right;
    }
}