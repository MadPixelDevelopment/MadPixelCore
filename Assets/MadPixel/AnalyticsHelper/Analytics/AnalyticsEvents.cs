using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MadPixelAnalytics {
    public struct LevelStart {
        public int level_number;
        public string level_name;
        public string game_mode;
        public string level_type;
        public string level_random;
    }

    public struct LevelEnd {
        public int level_number;
        public string level_name;
        public int level_count;
        public string level_diff;
        public int level_loop;
        public bool level_random;
        public string level_type;
        public string game_mode;
        public string result;
        public int time;
        public int progress;
        public int ad_continue;
    }
}