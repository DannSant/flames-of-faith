using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public class RunMapState 
    {
        public int seed;
        public int currentActIndex;

        public List<RunMapGraph> acts;
    }

}