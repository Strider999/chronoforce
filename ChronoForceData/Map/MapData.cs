#region File Description
//-----------------------------------------------------------------------------
// Map.cs
//
// Created by David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ChronoForceData.Base;
using ChronoForceData.Graphics;
#endregion

namespace ChronoForceData.Map
{
    /// <summary>
    /// Contains all the information on a map.
    /// </summary>
    public class MapData
    {
        #region Constants

        // Default values for loading tiles
        const int cDefaultTileHeight = 40;
        const int cDefaultTileWidth = 40;
        const int cTileOffset = 1; // Padding

        // Max number of tiles
        const int cNumTiles = 200;

        #endregion

        #region Fields

        // Graphics and content manager
        GraphicsDevice graphics;
        ContentManager contents;

        // Tile information
        Texture2D tileTexture;
        SpriteSheet tileSheet;
        List<TileGrid> bottomLayer = new List<TileGrid>();
        List<TileGrid> middleLayer = new List<TileGrid>();
        List<TileGrid> topLayer = new List<TileGrid>();

        // Used for loading the tile information from the file
        List<int[][]> bottomGrid;
        List<int[][]> middleGrid;
        List<int[][]> topGrid;

        // Animated sprites (TODO:  For tiles later)
        //private SpriteSheet animatedSpriteSheet;
        //private AnimatedSprite animatedSprite;
        //private Vector2 animatedSpritePosition;
        //private float accumulator;

        // Which map to render
        int lastMap = 0;
        int currentMap = 0;

        // Current map level
        int currentMapLevel = 0;

        // Stored map information
        string mapTileFile;
        int tileWidth, tileHeight;
        int[] transparentColor = new int[3];
        List<string> mapName = new List<string>();
        List<int> mapWidth = new List<int>(); // Size in tiles
        List<int> mapHeight = new List<int>(); // Size in tiles
        List<Vector2> mapSize = new List<Vector2>(); // Size in pixels
        List<int[][]> mapBounds = new List<int[][]>();

        // Filename for the map code data
        string mapCodeFilename;
        // Map Codes
        List<MapCodeAction> code;
        // List of map codes with coordinates of where they are
        List<MapEntry<MapCodeAction>> codeEntries;

        #endregion

        #region Properties

        #region Pipeline Loading Only Data

        /// <summary>
        /// The filename of the tile set used for this map
        /// </summary>
        public string MapTileFile
        {
            get { return mapTileFile; }
            set { mapTileFile = value; }
        }

        /// <summary>
        /// Map size list in pixels, used mostly for reading from XML
        /// </summary>
        public List<Vector2> MapSizeList
        {
            get { return mapSize; }
            set { mapSize = value; }
        }

        /// <summary>
        /// Used for loading the bottom layer from the pipeline
        /// </summary>
        public List<int[][]> BottomGrid
        {
            get { return bottomGrid; }
            set { bottomGrid = value; }
        }

        /// <summary>
        /// Used for loading the middle layer from the pipeline
        /// </summary>
        public List<int[][]> MiddleGrid
        {
            get { return middleGrid; }
            set { middleGrid = value; }
        }

        /// <summary>
        /// Used for loading the top layer from the pipeline
        /// </summary>
        public List<int[][]> TopGrid
        {
            get { return topGrid; }
            set { topGrid = value; }
        }

        /// <summary>
        /// Filename that contains the map code information
        /// </summary>
        public string MapCodeFilename
        {
            get { return mapCodeFilename; }
            set { mapCodeFilename = value; }
        }

        /// <summary>
        /// List of map codes
        /// </summary>
        /// <remarks>TODO:  Make the reader read the code</remarks>
        public List<MapCodeAction> Code
        {
            get { return code; }
            set { code = value; }
        }

        /// <summary>
        /// List of map codes with map coordinates
        /// </summary>
        /// <remarks>TODO:  Make the reader read the code</remarks>
        public List<MapEntry<MapCodeAction>> CodeEntries
        {
            get { return codeEntries; }
            set { codeEntries = value; }
        }

        #endregion

        #region Class Properties

        /// <summary>
        /// Returns the width of the tiles in pixels
        /// </summary>
        public int TileWidth
        {
            get { return tileWidth; }
            set { tileWidth = value; }
        }

        /// <summary>
        /// Returns the height of the tiles in pixels
        /// </summary>
        public int TileHeight
        {
            get { return tileHeight; }
            set { tileHeight = value; }
        }

        /// <summary>
        /// The map names loaded
        /// </summary>
        public List<string> MapName
        {
            get { return mapName; }
            set { mapName = value; }
        }

        /// <summary>
        /// Size of the map width in tiles
        /// </summary>
        public List<int> MapWidth
        {
            get { return mapWidth; }
            set { mapWidth = value; }
        }

        /// <summary>
        /// Size of the map height in tiles
        /// </summary>
        public List<int> MapHeight
        {
            get { return mapHeight; }
            set { mapHeight = value; }
        }

        /// <summary>
        /// List of the map bound matrix, used for reading from the pipeline
        /// </summary>
        public List<int[][]> MapBounds
        {
            get { return mapBounds; }
            set { mapBounds = value; }
        }

        /// <summary>
        /// The current map
        /// </summary>
        [ContentSerializerIgnore]
        public int CurrentMap
        {
            get { return currentMap; }
            set { currentMap = value; }
        }

        /// <summary>
        /// The previous map before the transition
        /// </summary>
        [ContentSerializerIgnore]
        public int LastMap
        {
            get { return lastMap; }
            set { lastMap = value; }
        }

        /// <summary>
        /// The current map level (0: base, 1: building, 2: inside building, etc.)
        /// </summary>
        [ContentSerializerIgnore]
        public int CurrentMapLevel
        {
            get { return currentMapLevel; }
            set { currentMapLevel = value; }
        }

        /// <summary>
        /// Returns the current size of the map in pixels
        /// </summary>
        [ContentSerializerIgnore]
        public Vector2 MapSize
        {
            get { return mapSize[currentMap]; }
        }

        #endregion

        #endregion

        #region Initialization

        /// <summary>
        /// Default contructor that loads nothing
        /// </summary>
        public MapData()
        {
            // Does nothing
        }

        #endregion

        #region XML Map Loading

        /// <summary>
        /// This is called after it finishes reading from the pipeline
        /// </summary>
        private void SetupData()
        {
            // Now that we have the file information, load the tile sheet
            tileSheet = new SpriteSheet(tileTexture);

            // Seperate the tileSheet into the proper tiles
            int counter = 0;
            for (int i = 0; i < tileTexture.Height; i += tileHeight + (cTileOffset * 2))
            {
                for (int j = 0; j < tileTexture.Width; j += tileWidth + (cTileOffset * 2))
                {
                    tileSheet.AddSourceSprite(counter, new Rectangle(j + cTileOffset, i + cTileOffset,
                        tileWidth, tileHeight));
                    counter++;
                }
            }

            // Load the tile grids with the information
            for (int i = 0; i < bottomGrid.Count; i++)
            {
                bottomLayer.Add(new TileGrid(tileWidth, tileHeight,
                    mapWidth[i], mapHeight[i], Vector2.Zero, tileSheet, "Bottom Layer"));
                middleLayer.Add(new TileGrid(tileWidth, tileHeight,
                    mapWidth[i], mapHeight[i], Vector2.Zero, tileSheet, "Middle Layer"));
                topLayer.Add(new TileGrid(tileWidth, tileHeight,
                    mapWidth[i], mapHeight[i], Vector2.Zero, tileSheet, "Top Layer"));

                bottomLayer[i].SetGrid(bottomGrid[i]);
                middleLayer[i].SetGrid(middleGrid[i]);
                topLayer[i].SetGrid(topGrid[i]);
            }

            // Load the map code information into the list of map entries
            foreach (MapEntry<MapCodeAction> codeEntry in codeEntries)
            {
                codeEntry.Content = code.Find(delegate(MapCodeAction mAction)
                    {
                        return (mAction.MapCodeName == codeEntry.ContentName);
                    });
            }
            
            // First, find the entrace to the new map so we can position the character and
            // map correctly.
            // By default, the base map is always at (0,0) and any other base maps
            int xPos, yPos;
            int destMap;
            for (int i = 0; i < codeEntries.Count; i++)
            {
                destMap = codeEntries[i].Content.DestinationMap;

                // If the destination is lower in ID than the current or 0, don't do anything since
                // the previous would have set the position already
                if (destMap > 0 && codeEntries[i].Content.MapLevel < codeEntries[i].Content.DestinationMapLevel)
                {
                    // Calculate the position of the map to render
                    xPos = (int)
                        (codeEntries[i].MapPosition.X - codeEntries[i].Content.DestinationMapPosition.X) * tileWidth;
                    yPos = (int)
                        (codeEntries[i].MapPosition.Y - codeEntries[i].Content.DestinationMapPosition.Y) * tileHeight;

                    bottomLayer[destMap].SetPosition(xPos, yPos);
                    middleLayer[destMap].SetPosition(xPos, yPos);
                    topLayer[destMap].SetPosition(xPos, yPos);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks to see if the tile is passable
        /// </summary>
        /// <param name="position">Current position on the map</param>
        /// <param name="direction">Direction where the character is trying to move</param>
        /// <param name="map">Map to check the bounds on</param>
        /// <returns>True if the character can walk in the specified direction</returns>
        public bool IsPassable(Point position, MapDirection direction, int map)
        {
            // TODO:  This checks both the current tile and the next, which works, but prevents the
            // creation of one-way walls, which could be useful later.  If we do use one-way walls,
            // then should we check for the next tile over for passibility? Would be more intuitive
            // for the Tile Studio end.
            // Checks the direction movement
            if (direction == MapDirection.Up)
            {
                // If at the edge, or if the top is blocked, character can't move
                if ((position.Y == 0) || ((mapBounds[map][position.X][position.Y] & 0x01) > 0) ||
                     ((position.Y != 0) && ((mapBounds[map][position.X][position.Y - 1] & 0x04) > 0)))
                    return false;
            }
            else if (direction == MapDirection.Down)
            {
                // If the bottom is blocked or at the edge
                if ((position.Y == mapHeight[map] - 1) || ((mapBounds[map][position.X][position.Y] & 0x04) > 0) ||
                    ((position.Y != mapHeight[map] - 1) && ((mapBounds[map][position.X][position.Y + 1] & 0x01) > 0)))
                    return false;
            }
            else if (direction == MapDirection.Left)
            {
                // If the left side is blocked or at the edge
                if ((position.X == 0) || ((mapBounds[map][position.X][position.Y] & 0x02) > 0) ||
                     ((position.X != 0) && ((mapBounds[map][position.X - 1][position.Y] & 0x08) > 0)))
                    return false;
            }
            else if (direction == MapDirection.Right)
            {
                // If the right side is blocked or at the edge
                if ((position.X == mapWidth[map] - 1) || ((mapBounds[map][position.X][position.Y] & 0x08) > 0) ||
                    ((position.X != mapWidth[map] - 1) && ((mapBounds[map][position.X + 1][position.Y] & 0x02) > 0)))
                    return false;
            }

            // All clear
            return true;
        }

        /// <summary>
        /// Updates the camera positions on the map
        /// </summary>
        /// <param name="camera">Camera viewing the scene</param>
        public void UpdateCameraView(Camera2D camera)
        {
            //set rotation
            topLayer[currentMap].CameraRotation = middleLayer[currentMap].CameraRotation =
                bottomLayer[currentMap].CameraRotation = camera.Rotation;

            //set zoom
            topLayer[currentMap].CameraZoom = middleLayer[currentMap].CameraZoom =
                bottomLayer[currentMap].CameraZoom = camera.Zoom;

            //set position
            topLayer[currentMap].CameraPosition = middleLayer[currentMap].CameraPosition =
                bottomLayer[currentMap].CameraPosition = camera.Position;

            // If the lastMap isn't the same as currentMap, update it as well as
            // we're most likely in the middle of transitioning
            if (lastMap != currentMap)
            {
                // Rotation
                topLayer[lastMap].CameraRotation = middleLayer[lastMap].CameraRotation =
                    bottomLayer[lastMap].CameraRotation = camera.Rotation;
                // Zoom
                topLayer[lastMap].CameraZoom = middleLayer[lastMap].CameraZoom =
                    bottomLayer[lastMap].CameraZoom = camera.Zoom;
                // Position
                topLayer[lastMap].CameraPosition = middleLayer[lastMap].CameraPosition =
                    bottomLayer[lastMap].CameraPosition = camera.Position;
            }
        }

        /// <summary>
        /// Sets the fade for the map specified for transitions
        /// </summary>
        /// <param name="map">Map ID to fade</param>
        /// <param name="fade">Fade amount, where 255 is no fade and 0 is black</param>
        public void SetFade(int map, byte fade)
        {
            bottomLayer[map].Color = middleLayer[map].Color =
                        topLayer[map].Color = new Color(255, 255, 255, fade);
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draws the map based on the flags
        /// </summary>
        /// <param name="spriteBatch">Batch used to render everything</param>
        /// <param name="bottom">True if drawing the bottom layer</param>
        /// <param name="middle">True if drawing the middle layer</param>
        /// <param name="top">True if drawing the top layer</param>
        public void Draw(SpriteBatch spriteBatch, bool bottom, bool middle, bool top)
        {
            if (bottom)
                bottomLayer[currentMap].Draw(spriteBatch);
            if (middle)
                middleLayer[currentMap].Draw(spriteBatch);
            if (top)
                topLayer[currentMap].Draw(spriteBatch);

            if (lastMap != currentMap)
            {
                if (bottom)
                    bottomLayer[lastMap].Draw(spriteBatch);
                if (middle)
                    middleLayer[lastMap].Draw(spriteBatch);
                if (top)
                    topLayer[lastMap].Draw(spriteBatch);
            }
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a map from the pipeline
        /// </summary>
        public class MapDataReader : ContentTypeReader<MapData>
        {
            /// <summary>
            /// Reads a MapData object from the content pipeline.
            /// </summary>
            protected override MapData Read(ContentReader input,
                MapData existingInstance)
            {
                MapData mData = existingInstance;
                if (existingInstance == null)
                {
                    mData = new MapData();
                }

                // Filename of the tile set
                mData.MapTileFile = input.ReadString();
                mData.tileTexture = input.ContentManager.Load<Texture2D>(mData.MapTileFile);

                // Width and height of the tiles
                mData.TileWidth = input.ReadInt32();
                mData.TileHeight = input.ReadInt32();

                // Size of the maps in pixels
                mData.MapSizeList = input.ReadObject<List<Vector2>>();

                // Map names
                mData.MapName = input.ReadObject<List<string>>();

                // Width and height in tiles
                mData.MapWidth = input.ReadObject<List<int>>();
                mData.MapHeight = input.ReadObject<List<int>>();

                // Layers of the map
                mData.bottomGrid = input.ReadObject<List<int[][]>>();
                mData.middleGrid = input.ReadObject<List<int[][]>>();
                mData.topGrid = input.ReadObject<List<int[][]>>();

                // Bounds
                mData.MapBounds = input.ReadObject<List<int[][]>>();

                // Filename of the map codes
                mData.MapCodeFilename = input.ReadString();

                // Load the map codes into the array
                mData.Code = input.ContentManager.Load<List<MapCodeAction>>(mData.MapCodeFilename);

                // Map entries for the map codes
                mData.CodeEntries = input.ReadObject<List<MapEntry<MapCodeAction>>>();

                // Complete the map loading by setting all the information
                mData.SetupData();

                return mData;
            }
        }

        #endregion
    }
}
