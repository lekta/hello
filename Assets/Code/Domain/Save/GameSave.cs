using System;
using System.Collections.Generic;

namespace LH.Save {
    // DO: лучше месежпак, но пока и так похер
    [Serializable]
    public class GameSave {
        public int SessionsCount;
        public int SavesCount;

        public List<HiddenObjectSave> HiddenObjects = new();
    }
}