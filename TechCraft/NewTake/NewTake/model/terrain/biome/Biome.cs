using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NewTake.model.terrain.biome
{

    #region BiomeType
    public enum BiomeType : byte
    {

        None = 0,
    
        //  Tundra 
        Tundra_Alpine = 1,
        Tundra_Artic = 2,
    
        //  Grassland
        Grassland_Savanna = 3,
        Grassland_Temperate = 4,
    
        //  Forest
        Forest_Tropical = 5,
        Forest_Temperate = 6,
        Forest_Taiga = 7,
    
        //  Desert
        Desert_Subtropical = 8,
        Desert_Semiarid = 9,
        Desert_Coastal = 10,
        Desert_Cold = 11,
    
        //  Marine
        Marine_Ocean = 12,
        Marine_CoralReef = 13,
        Marine_Estuary = 14,
    
        //  Freshwater
        Freshwater_Lake = 15,
        Freshwater_River = 16,
        Freshwater_Wetland = 17,
    
        Custom = 18,
        MAXIMUM = 19

    }
    #endregion

    public class Biome
    {

        public byte temperature_lowest      { get; set; }
        public byte temperature_highest     { get; set; }

        public byte rainfall_lowest         { get; set; }
        public byte rainfall_highest        { get; set; }

        public BlockType treetype           { get; set; }
        public BlockType topgroundblocktype { get; set; }
        public BlockType watertype          { get; set; }


        public Biome()
        {

        }
    } 
}
