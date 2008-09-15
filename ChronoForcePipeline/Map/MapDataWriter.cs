#region File Description
//-----------------------------------------------------------------------------
// MapDataWriter.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ChronoForceData.Map;
#endregion

namespace ChronoForcePipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MapDataWriter : ChronoForceWriter<MapData>
    {
        protected override void Write(ContentWriter output, MapData value)
        {
            // Filename of the tile set
            output.Write(value.MapTileFile);

            // Width and height of the tiles
            output.Write(value.TileWidth);
            output.Write(value.TileHeight);

            // Size of the maps in pixels
            output.WriteObject<List<Vector2>>(value.MapSizeList);

            // Map names
            output.WriteObject<List<string>>(value.MapName);

            // Width and height in tiles
            output.WriteObject<List<int>>(value.MapWidth);
            output.WriteObject<List<int>>(value.MapHeight);

            // Layers of the map
            output.WriteObject<List<int[][]>>(value.BottomGrid);
            output.WriteObject<List<int[][]>>(value.MiddleGrid);
            output.WriteObject<List<int[][]>>(value.TopGrid);

            // Bounds and MapCodes
            output.WriteObject<List<int[][]>>(value.MapBounds);
            output.WriteObject<List<int[][]>>(value.MapCodeList);

            // Filename of the map code information
            output.Write(value.MapCodeFilename);

            // List of map entries for the code list
            output.WriteObject<List<MapEntry<MapCodeAction>>>(value.CodeEntries);
        }
    }
}
