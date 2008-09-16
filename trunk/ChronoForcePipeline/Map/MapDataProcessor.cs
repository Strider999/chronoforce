#region File Description
//-----------------------------------------------------------------------------
// MapDataProcessor.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ChronoForceData.Map;
#endregion

namespace ChronoForcePipeline.Map
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "ChronoForcePipeline.Map.MapDataProcessor")]
    public class MapDataProcessor : ContentProcessor<string[], MapData>
    {
        /// <summary>
        /// Extracts the filenames in the XML and converts it to a usable form for the
        /// MapData class.  The order of the files are:
        /// - 0: Map, 1: MapCode descriptions, 2: NPC data, 3: Monster data, 4: Chests
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns>MapData from the compiled files</returns>
        public override MapData Process(string[] input, ContentProcessorContext context)
        {
            MapData ret = new MapData();

            // The first string is the map filename
            // Opens the file in binary
            BinaryReader bReader = new BinaryReader(File.OpenRead(input[0]));            

            // The first information is the tile file used
            // The string ends will a null, so read characters until that point
            char c;
            while ((c = bReader.ReadChar()) != 0)
                ret.MapTileFile += c;

            // Next is the size of the tiles
            ret.TileWidth = bReader.ReadByte();
            ret.TileHeight = bReader.ReadByte();

            // Read the number of maps
            int mapNumber = bReader.ReadByte();
            string tempName;

            // Initialize the grids
            ret.BottomGrid = new List<int[][]>();
            ret.MiddleGrid = new List<int[][]>();
            ret.TopGrid = new List<int[][]>();

            // Initilize the map codes
            ret.CodeEntries = new List<MapEntry<MapCodeAction>>();

            for (int i = 0; i < mapNumber; i++)
            {
                tempName = "";
                // Read the map name
                while ((c = bReader.ReadChar()) != 0)
                    tempName += c;
                ret.MapName.Add(tempName);

                // Get the size of the map being loaded
                ret.MapWidth.Add(bReader.ReadByte());
                ret.MapHeight.Add(bReader.ReadByte());

                // Store the pixel size of the map
                ret.MapSizeList.Add(new Vector2(ret.MapWidth[i] * ret.TileWidth,
                    ret.MapHeight[i] * ret.TileHeight));

                // Initialize the map matrices
                ret.MapBounds.Add(new int[ret.MapWidth[i]][]);
                ret.BottomGrid.Add(new int[ret.MapWidth[i]][]);
                ret.MiddleGrid.Add(new int[ret.MapWidth[i]][]);
                ret.TopGrid.Add(new int[ret.MapWidth[i]][]);
                for (int j = 0; j < ret.MapWidth[i]; j++)
                {
                    ret.MapBounds[i][j] = new int[ret.MapHeight[i]];
                    ret.BottomGrid[i][j] = new int[ret.MapHeight[i]];
                    ret.MiddleGrid[i][j] = new int[ret.MapHeight[i]];
                    ret.TopGrid[i][j] = new int[ret.MapHeight[i]];
                    for (int k = 0; k < ret.MapHeight[i]; k++)
                    {
                        ret.MapBounds[i][j][k] = 0;
                    }
                }

                // Loop through each of the tiles on the map
                int x, y, code;
                for (int j = 0; j < ret.MapWidth[i] * ret.MapHeight[i]; j++)
                {
                    // First two are 8-bit (x,y) coordinates
                    x = bReader.ReadByte();
                    y = bReader.ReadByte();

                    // Next is the 8-bit bounds (has 4 bits with extras for diagonals)
                    ret.MapBounds[i][x][y] = bReader.ReadByte();

                    // Map codes contain events or special objects on the map
                    code = bReader.ReadByte();

                    // If we find a map code, add the location to the code entries
                    if (code > 0)
                    {
                        MapEntry<MapCodeAction> newAction = new MapEntry<MapCodeAction>();
                        newAction.MapPosition = new Point(x, y);
                        newAction.ContentName = ret.MapName[i] + code.ToString();
                        ret.CodeEntries.Add(newAction);
                    }

                    // The next three contain tile information for the three layers
                    ret.BottomGrid[i][x][y] = bReader.ReadByte();
                    ret.MiddleGrid[i][x][y] = bReader.ReadByte();
                    ret.TopGrid[i][x][y] = bReader.ReadByte();
                }
            }

            bReader.Close();

            // The second filename is the map code information
            ret.MapCodeFilename = input[1];

            // TODO:  The third filename is the NPC information
            // TODO:  The fourth filename is the Monster information
            // TODO:  The fifth filename is the chest information

            return ret;
        }
    }
}