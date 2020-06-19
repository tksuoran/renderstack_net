//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using OpenTK.Graphics.OpenGL;

using RenderStack.Graphics;
using RenderStack.Math;
using RenderStack.Mesh;

using Buffer = RenderStack.Graphics.BufferGL;
using Attribute = RenderStack.Graphics.Attribute;

namespace example.VoxelRenderer
{
    public class BlockType
    {
        public static readonly byte Air = 0;
        public static readonly byte Stone = 1;
        public static readonly byte Grass = 2;
        public static readonly byte Dirt = 3;
        public static readonly byte Cobblestone = 4;
        public static readonly byte Wood = 5;
        public static readonly byte Sapling = 6;
        public static readonly byte Bedrock = 7;
        public static readonly byte Water = 8;
        public static readonly byte Stationary_Water = 9;
        public static readonly byte Still_Water = 9;
        public static readonly byte Lava = 10;
        public static readonly byte Still_Lava = 11;
        public static readonly byte Sand = 12;
        public static readonly byte Gravel = 13;
        public static readonly byte Gold_Ore = 14;
        public static readonly byte Iron_Ore = 15;
        public static readonly byte Coal_Ore = 16;
        public static readonly byte Log = 17;
        public static readonly byte Leaves = 18;
        public static readonly byte Sponge = 19;
        public static readonly byte Glass = 20;
        public static readonly byte Lapis_Lazuli_Ore = 21;
        public static readonly byte Lapis_Lazuli_Block = 22;
        public static readonly byte Dispenser = 23;
        public static readonly byte Sandstone = 24;
        public static readonly byte Note_Block = 25;
        public static readonly byte Bed = 26;
        public static readonly byte Cloth = 35;
        public static readonly byte Wool = 35;
        public static readonly byte Yellow_Flower = 37;
        public static readonly byte Flower = 37;
        public static readonly byte Red_Rose = 38;
        public static readonly byte Rose = 38;
        public static readonly byte Brown_Mushroom = 39;
        public static readonly byte Red_Mushroom = 40;
        public static readonly byte Gold_Block = 41;
        public static readonly byte Iron_Block = 42;
        public static readonly byte Double_Stair = 43;
        public static readonly byte Double_Stone_Slab = 43;
        public static readonly byte Stair = 44;
        public static readonly byte Slab = 44;
        public static readonly byte Brick = 45;
        public static readonly byte TNT = 46;
        public static readonly byte Bookcase = 47;
        public static readonly byte Bookshelf = 47;
        public static readonly byte Mossy_Cobblestone = 48;
        public static readonly byte Obsidian = 49;
        public static readonly byte Torch = 50;
        public static readonly byte Fire = 51;
        public static readonly byte Mob_Spawner = 52;
        public static readonly byte Wooden_Stairs = 53;
        public static readonly byte Chest = 54;
        public static readonly byte Redstone_Wire = 55;
        public static readonly byte Diamond_Ore = 56;
        public static readonly byte Diamond_Block = 57;
        public static readonly byte Workbench = 58;
        public static readonly byte Crops = 59;
        public static readonly byte Soil = 60;
        public static readonly byte Furnace = 61;
        public static readonly byte Burning_Furnace = 62;
        public static readonly byte Sign_Post = 63;
        public static readonly byte Wooden_Door = 64;
        public static readonly byte Ladder = 65;
        public static readonly byte Minecart_Rail = 66;
        public static readonly byte Rails = 66;
        public static readonly byte Track = 66;
        public static readonly byte Tracks = 66;
        public static readonly byte Cobblestone_Stairs = 67;
        public static readonly byte Stone_Stairs = 67;
        public static readonly byte Wall_Sign = 68;
        public static readonly byte Lever = 69;
        public static readonly byte Stone_Pressure_Plate = 70;
        public static readonly byte Iron_Door = 71;
        public static readonly byte Wooden_Pressure_Plate = 72;
        public static readonly byte Redstone_Ore = 73;
        public static readonly byte Redstone_Ore_Glowing = 74;
        public static readonly byte Redstone_Torch = 75;
        public static readonly byte Redstone_Torch_On = 76;
        public static readonly byte Stone_Button = 77;
        public static readonly byte Snow = 78;
        public static readonly byte Ice = 79;
        public static readonly byte Snow_Block = 80;
        public static readonly byte Cactus = 81;
        public static readonly byte Clay = 82;
        public static readonly byte Reed = 83;
        public static readonly byte Jukebox = 84;
        public static readonly byte Fence = 85;
        public static readonly byte Pumpkin = 86;
        public static readonly byte Bloodstone = 87;
        public static readonly byte Netherrack = 87;
        public static readonly byte Slow_Sand = 88;
        public static readonly byte Soul_Sand = 88;
        public static readonly byte Lightstone = 89;
        public static readonly byte Glowstone = 89;
        public static readonly byte Portal = 90;
        public static readonly byte Jack_O_Lantern = 91;
        public static readonly byte Pumpkin_Lantern = 91;
        public static readonly byte Cake = 92;
        public static readonly byte Redstone_Repeater = 93;
        public static readonly byte Redstone_Repeater_On = 94;

        public static string Name(byte code)
        {
            if(code < names.Length)
            {
                return names[code];
            }
            return "(" + code.ToString() + ")";
        }
        private static readonly string[] names = {
            "Air (0)",
            "Stone (1)",
            "Grass (2)",
            "Dirt (3)",
            "Cobblestone (4)",
            "Wood (5)",
            "Sapling (6)",
            "Bedrock (7)",
            "Water (8)",
            "Stationary_Water (9)",
            //"Still_Water (9)",
            "Lava (10)",
            "Still_Lava (11)",
            "Sand (12)",
            "Gravel (13)",
            "Gold_Ore (14)",
            "Iron_Ore (15)",
            "Coal_Ore (16)",
            "Log (17)",
            "Leaves (18)",
            "Sponge (19)",
            "Glass (20)",
            "Lapis_Lazuli_Ore (21)",
            "Lapis_Lazuli_Block (22)",
            "Dispenser (23)",
            "Sandstone (24)",
            "Note_Block (25)",
            "Bed (26)",
            "(27)",
            "(28)",
            "(29)",
            "(30)",
            "(31)",
            "(32)",
            "(33)",
            "(34)",
            //"Cloth (35)",
            "Wool (35)",
            "(36)",
            "Yellow Flower (37)",
            //"Flower (37)",
            "Red Flower (38)",
            //"Rose (38)",
            "Brown Mushroom (39)",
            "Red Mushroom (40)",
            "Gold Block (41)",
            "Iron Block (42)",
            //"Double Stair (43)",
            //"Double Stone Slab (43)",
            "(43)",
            //"Stair (44)",
            //"Slab (44)",
            "(44)",
            "Brick (45)",
            "TNT (46)",
            //"Bookcase (47)",
            "Bookshelf (47)",
            "Mossy Cobblestone (48)",
            "Obsidian (49)",
            "Torch (50)",
            "Fire (51)",
            "Spawner (52)",
            "Wooden Stairs (53)",
            "Chest (54)",
            "Redstone_Wire (55)",
            "Diamond Ore (56)",
            "Diamond Block (57)",
            "Workbench (58)",
            "Crops (59)",
            "Soil (60)",
            "Furnace (61)",
            "Burning Furnace (62)",
            "Sign (63)",
            "Wooden Door (64)",
            "Ladder (65)",
            //"Minecart_Rail (66)",
            //"Rails (66)",
            "Track (66)",
            //"Tracks (66)",
            "Cobblestone Stairs (67)",
            //"Stone_Stairs (67)",
            "Wall Sign (68)",
            "Lever (69)",
            "Stone Pressure Plate (70)",
            "Iron Door (71)",
            "Wooden Pressure Plate (72)",
            "Redstone Ore (73)",
            "Redstone Ore Glowing (74)",
            "Redstone Torch (75)",
            "Redstone Torch On (76)",
            "Stone Button (77)",
            "Snow (78)",
            "Ice (79)",
            "Snow Block (80)",
            "Cactus (81)",
            "Clay (82)",
            "Reed (83)",
            "Jukebox (84)",
            "Fence (85)",
            "Pumpkin (86)",
            //"Bloodstone (87)",
            "Netherrack (87)",
            //"Slow_Sand (88)",
            "Soul Sand (88)",
            //"Lightstone (89)",
            "Glowstone (89)",
            "Portal (90)",
            //"Jack_O_Lantern (91)",
            "Pumpkin Lantern (91)",
            "Cake (92)",
            "Redstone Repeater (93)",
            "Redstone Repeater On (94)",
        };

        public static readonly byte[] Opacity = new byte[]
        {
            //0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f
            0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 0xf, 0x2, 0x2, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0x1, 0xf, 0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 0x0, 0x0, 0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0x0, 0x0, 0x0, 0xf, 0xf, 0x0, 0xf, 0xf, 0xf, 0x0, 0xf, 0xf, 0xf, 0x0, 
            0x0, 0x0, 0x0, 0xf, 0x0, 0x0, 0x0, 0x0, 0x0, 0xf, 0xf, 0x0, 0x0, 0x0, 0xf, 0x2, 
            0xf, 0x0, 0x0, 0x0, 0xf, 0x0, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0x0, 0x0, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 
            0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf, 0xf
        };

        public static readonly byte[] Luminance = new byte[]
        {
            //0    1    2    3    4    5    6    7    8    9    a    b    c    d    e    f
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xf, 0xf, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0xe, 0xf, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x9, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xf, 0x0, 0xf, 0x0, 0x0, 0x7, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
            0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0
        };

        public static readonly byte[] AirBlocks = new byte[]
        {
            Air,
            Brown_Mushroom,
            Crops,
            Fence,
            Ladder,
            Lever,
            Rails,
            Reed,
            Red_Mushroom,
            Red_Rose,
            Redstone_Wire,
            Redstone_Torch,
            Redstone_Torch_On,
            Sapling,
            Sign_Post,
            Stone_Button,
            Stone_Pressure_Plate,
            Torch,
            Yellow_Flower,
            Wall_Sign,
            Wooden_Pressure_Plate
        };

        private byte topU;
        private byte topV;
        private byte bottomU;
        private byte bottomV;
        private byte leftU;
        private byte leftV;
        private byte rightU;
        private byte rightV;
        private byte frontU;
        private byte frontV;
        private byte backU;
        private byte backV;

        public byte TopU    { get { return topU; } }
        public byte TopV    { get { return topV; } }
        public byte BottomU { get { return bottomU; } }
        public byte BottomV { get { return bottomV; } }
        public byte LeftU   { get { return leftU; } }
        public byte LeftV   { get { return leftV; } }
        public byte RightU  { get { return rightU; } }
        public byte RightV  { get { return rightV; } }
        public byte FrontU  { get { return frontU; } }
        public byte FrontV  { get { return frontV; } }
        public byte BackU   { get { return backU; } }
        public byte BackV   { get { return backV; } }

        public BlockType(
            byte topU, 
            byte topV, 
            byte bottomU, 
            byte bottomV, 
            byte leftU, 
            byte leftV, 
            byte rightU, 
            byte rightV, 
            byte frontU, 
            byte frontV, 
            byte backU,
            byte backV
        )
        {
            this.topU = topU;
            this.topV = topV;
            this.bottomU = bottomU;
            this.bottomV = bottomV;
            this.leftU = leftU;
            this.leftV = leftV;
            this.rightU = rightU;
            this.rightV = rightV;
            this.frontU = frontU;
            this.frontV = frontV;
            this.backU = backU;
            this.backV = backV;
        }
        public BlockType(
            byte u,
            byte v
        )
        {
            this.topU = u;
            this.topV = v;
            this.bottomU = u;
            this.bottomV = v;
            this.leftU = u;
            this.leftV = v;
            this.rightU = u;
            this.rightV = v;
            this.frontU = u;
            this.frontV = v;
            this.backU = u;
            this.backV = v;
        }
    }
}
