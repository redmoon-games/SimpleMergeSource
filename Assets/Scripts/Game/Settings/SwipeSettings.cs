using System;

namespace Game.Settings
{
    [Serializable]
    public class SwipeSettings
    {
        public float minDirectionX = 0.85f;
        public float maxDirectionY = 0.2f;
        public float maxDistance = 150f;
    }
}
