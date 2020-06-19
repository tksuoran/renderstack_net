using System.Collections.Generic;

namespace Game
{
    public class Game
    {
        private List<Unit> units = new List<Unit>();

        public void Update()
        {
            foreach(var unit in units)
            {
                unit.UpdateMotion();
            }
        }
    }
}
