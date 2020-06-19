public class Entity
{
    public class Flags
    {
        public const int Static             =  1;   //  Immovable with physics (often World)
        public const int Dynamic            =  2;   //  Movable with physics
        public const int Hidden             =  4;   //  Not visible
        public const int Player             =  8;   //  Player controlled
        public const int World              = 16;   //  Part of world map (often Static)
        public const int AffectedByGravity  = 32;   //  Physics gravity on/off
    }
}
