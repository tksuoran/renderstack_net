//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

namespace Game
{
    public class Game
    {
        private List<Unit> units = new List<Unit>();

        public Game()
        {
        }

        public void Update()
        {
            foreach(var unit in units)
            {
                unit.UpdateMotion();
            }
        }
    }
}
