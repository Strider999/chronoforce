#region File Description
//-----------------------------------------------------------------------------
// MapEngine.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using ChronoForce.Base;
using ChronoForceData.Character;
using ChronoForceData.Actions;
#endregion

namespace ChronoForce.Engine
{
    #region Enums
    /// <summary>
    /// Enumeration to show where the character is moving
    /// </summary>
    public enum MapDirection
    {
        Up, Down, Left, Right
    }

    #endregion

    /// <summary>
    /// World map and dungeon engine that handles rendering the world and transitioning between
    /// maps, rooms, and battles.
    /// </summary>
    class MapEngine
    {
        #region Singleton

        /// <summary>
        /// The single Session instance that can be active at a time.
        /// </summary>
        private static MapEngine singleton;

        #endregion

        #region Constants
        // Rate controls for the camera
        const float cMovementRate = 500f;
        const float cZoomRate = 0.5f;
        const float cRotationRate = 1.5f;
        // Max number of tiles
        const int cNumTiles = 200;
        // Delay between frames of animation
        const float cAnimationTime = 0.1f;
        // Default values for loading tiles
        const int cDefaultTileHeight = 40;
        const int cDefaultTileWidth = 40;
        const int cTileOffset = 1;
        // Maximum number of actors on a map
        const int cMaxActors = 20;
        readonly Vector2 cSpriteScale = new Vector2(1f, 1f);
        #endregion

        #region Fields
        // Graphical parameters needed to render the sprites and tiles
        GraphicsDevice graphics;
        SpriteBatch spriteBatch;
        ContentManager contents;
        Camera2D camera;

        // Tile information
        SpriteSheet tileSheet;
        TileGrid bottomLayer;
        TileGrid middleLayer;
        TileGrid topLayer;

        // Animated sprites (TODO:  For tiles later)
        //private SpriteSheet animatedSpriteSheet;
        //private AnimatedSprite animatedSprite;
        //private Vector2 animatedSpritePosition;
        //private float accumulator;

        // Sprite information
        List<CharacterBase> mapActors = new List<CharacterBase>(cMaxActors);
        CharacterBase player;

        // Flag to see if the engine has loaded anything
        bool isMapLoaded;
        // Flag for debug control
        bool debugOn;
        // String for debug status
        string statusMsg;
        // Debugger class for printing debug data
        Debugger mapDebug;

        // Debugging variables to control
        bool[] showLayer;
        bool controlCamera;

        // Stored map information
        string mapTileFile;
        int tileWidth, tileHeight;
        int[] transparentColor = new int[3];
        string mapName;
        int mapWidth, mapHeight;
        int[][] mapBounds;
        int[][] mapCodes;

        #endregion

        #region Properties

        /// <summary>
        /// Used for keeping track of the player on the world map
        /// </summary>
        public static CharacterBase Player
        {
            get { return singleton.player; }
            set { singleton.player = value; }
        }

        /// <summary>
        /// Private property used for debugging
        /// </summary>
        private string StatusMsg
        {
            get { return statusMsg; }
            set { statusMsg = value; }
        }

        /// <summary>
        /// Returns the world camera
        /// </summary>
        public static Camera2D Camera
        {
            get { return singleton.camera; }
        }

        /// <summary>
        /// Returns the 2D array of map boundaries
        /// </summary>
        private static int[][] MapBounds
        {
            get { return singleton.mapBounds; }
        }

        /// <summary>
        /// Returns the array of layer flags
        /// </summary>
        private static bool[] ShowLayer
        {
            get { return singleton.showLayer; }
        }

        /// <summary>
        /// Determines whether or not the camera is under control for debugging
        /// </summary>
        private static bool ControlCamera
        {
            get { return singleton.controlCamera; }
            set { singleton.controlCamera = value; }
        }

        /// <summary>
        /// Returns the current map height
        /// </summary>
        private static int MapHeight
        {
            get { return singleton.mapHeight; }
        }

        /// <summary>
        /// Returns the current map width
        /// </summary>
        private static int MapWidth
        {
            get { return singleton.mapWidth; }
        }

        /// <summary>
        /// Returns the map debugger
        /// </summary>
        private static Debugger MapDebug
        {
            get { return singleton.mapDebug; }
        }

        /// <summary>
        /// The flag that true if debugging is on
        /// </summary>
        public static bool DebugOn
        {
            set 
            {
                if (singleton != null)
                {
                    singleton.debugOn = value;
                    MapDebug.DebugOn = value;
                }
            }
            get 
            { 
                return (singleton == null ? false : singleton.debugOn); 
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor loads and initializes the class
        /// </summary>
        /// <param name="graphicsComponent">Place where to render the graphics to</param>
        /// <param name="contentManager">Content to load from</param>
        private MapEngine(GraphicsDevice graphicsComponent, ContentManager contentManager)
        {
            if (graphicsComponent == null)
            {
                throw new ArgumentNullException("graphicsComponent");
            }

            graphics = graphicsComponent;
            contents = contentManager;
            spriteBatch = new SpriteBatch(graphics);
            isMapLoaded = false;

            // Initialize debug controls and variables
            debugOn = true;
            mapDebug = new Debugger("MapDebug", debugOn);
            showLayer = new bool[3];

            for (int i = 0; i < 3; i++)
                showLayer[i] = true;

            controlCamera = false;
        }

        #endregion

        #region Loading Map

        /// <summary>
        /// Loads the specified map from the passed filename
        /// </summary>
        /// <param name="filename">Map file to load</param>
        /// <returns>True if success, false if there's an error</returns>
        private bool LoadMap(string filename)
        {
            // First, check to see if the filename has the right format
            if (!filename.Contains(".map"))
            {
                mapDebug.debugPrint("Filename doesn't end in .map");
                return false;
            }
            
            // Checks to make sure the file is there
            if (!File.Exists(filename))
            {
                mapDebug.debugPrint("File " + filename + " doesn't exist");
                return false;
            }

            // Opens the file in binary
            BinaryReader bReader = new BinaryReader(File.OpenRead(filename));

            // The first information is the tile file used
            // The string ends will a null, so read characters until that point
            char c;
            while ( (c = bReader.ReadChar()) != 0)
                mapTileFile += c;

            // Next is the size of the tiles
            tileWidth = bReader.ReadByte();
            tileHeight = bReader.ReadByte();

            // Read the transparent pixel colors for this tile set
            for (int i = 0; i < 3; i++)
                transparentColor[i] = bReader.ReadByte();

            // Now that we have the file information, load the tile sheet
            Texture2D tileTexture = contents.Load<Texture2D>(mapTileFile);
            tileSheet = new SpriteSheet(tileTexture);

            // Seperate the tileSheet into the proper tiles
            int counter = 0;
            for (int i = 0; i < tileTexture.Height; i += tileHeight)
            {
                for (int j = 0; j < tileTexture.Width; j += tileWidth)
                {
                    tileSheet.AddSourceSprite(counter, new Rectangle(j, i, tileWidth, tileHeight));
                    counter++;
                }
            }

            // Read the map name
            while ( (c = bReader.ReadChar()) != 0)
                mapName += c;

            // Get the size of the map being loaded
            mapWidth = bReader.ReadByte();
            mapHeight = bReader.ReadByte();

            // Load the tile grids with the information
            bottomLayer = new TileGrid(tileWidth-cTileOffset, tileHeight-cTileOffset, mapWidth, mapHeight,
                Vector2.Zero, tileSheet, graphics, "Bottom Layer");
            middleLayer = new TileGrid(tileWidth-cTileOffset, tileHeight-cTileOffset, mapWidth, mapHeight,
                Vector2.Zero, tileSheet, graphics, "Middle Layer");
            topLayer = new TileGrid(tileWidth-cTileOffset, tileHeight-cTileOffset, mapWidth, mapHeight,
                Vector2.Zero, tileSheet, graphics, "Top Layer");

            // Initialize the map matrices
            mapBounds = new int[mapWidth][];
            mapCodes = new int[mapWidth][];
            for (int i = 0; i < mapWidth; i++)
            {
                mapBounds[i] = new int[mapHeight];
                mapCodes[i] = new int[mapHeight];
                for (int j = 0; j < mapHeight; j++)
                {
                    mapBounds[i][j] = 0;
                    mapCodes[i][j] = 0;
                }
            }

            // Loop through each of the tiles on the map
            int x, y;
            for (int i = 0; i < mapWidth * mapHeight; i++)
            {
                // First two are 8-bit (x,y) coordinates
                x = bReader.ReadByte();
                y = bReader.ReadByte();

                // Next is the 8-bit bounds (has 4 bits with extras for diagonals)
                mapBounds[x][y] = bReader.ReadByte();
                
                // Map codes contain events or special objects on the map
                mapCodes[x][y] = bReader.ReadByte();
                
                // The next three contain tile information for the three layers
                bottomLayer.SetTile(x, y, bReader.ReadByte());
                middleLayer.SetTile(x, y, bReader.ReadByte());
                topLayer.SetTile(x, y, bReader.ReadByte());
            }

            // DEBUG:  Print out the read in information to see if it's correct
            if (debugOn)
            {
                //topLayer.debugPrint();
                //middleLayer.debugPrint();
                //bottomLayer.debugPrint();
            }

            // Set Up a 2D Camera
            camera = new Camera2D();

            ResetToInitialPositions();

            // Set the flag to show that a map is loaded
            isMapLoaded = true;

            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the specified map from the passed filename
        /// </summary>
        /// <param name="filename">Map file to load</param>
        /// <param name="content">Content manager for loading graphics</param>
        /// <param name="graphics">Graphics for rendering to the screen</param>
        /// <param name="playerArg">Player party to be rendered on the screen</param>
        /// <returns>True if the load succeeded, false otherwise</returns>
        public static bool LoadMapEngine(string filename, 
            GraphicsDevice graphics, ContentManager content, CharacterBase playerArg)
        {
            // Clears any previously loaded map
            ClearMapEngine();

            // Create a new instance of the MapEngine
            singleton = new MapEngine(graphics, content);

            // Set the player
            Player = playerArg;

            // DEBUG:  Load the XML
            ActionScript tester = content.Load<ActionScript>("TestScript");

            // Load the map file
            return singleton.LoadMap(filename);
        }

        /// <summary>
        /// Clears any previously loaded map
        /// </summary>
        /// <returns>True if suceeded, false otherwise</returns>
        public static bool ClearMapEngine()
        {
            if (singleton != null)
            {
                singleton = null;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if the tile is passable
        /// </summary>
        /// <param name="position">Current position on the map</param>
        /// <param name="direction">Direction where the character is trying to move</param>
        /// <returns>True if the character can walk in the specified direction</returns>
        public static bool IsPassable(Point position, MapDirection direction)
        {
            // TODO:  This checks both the current tile and the next, which works, but prevents the
            // creation of one-way walls, which could be useful later.  If we do use one-way walls,
            // then should we check for the next tile over for passibility? Would be more intuitive
            // for the Tile Studio end.
            // Checks the direction movement
            if (direction == MapDirection.Up)
            {
                // If at the edge, or if the top is blocked, character can't move
                if ( (position.Y == 0) || ((MapBounds[position.X][position.Y] & 0x01) > 0) ||
                     ((position.Y != 0) && ((MapBounds[position.X][position.Y-1] & 0x04) > 0)) )
                    return false;
            }
            else if (direction == MapDirection.Down)
            {
                // If the bottom is blocked or at the edge
                if ( (position.Y == MapHeight - 1 ) || ((MapBounds[position.X][position.Y] & 0x04) > 0) ||
                    ((position.Y != MapHeight - 1 ) && ((MapBounds[position.X][position.Y+1] & 0x01) > 0)) )
                    return false;
            }
            else if (direction == MapDirection.Left)
            {
                // If the left side is blocked or at the edge
                if ( (position.X == 0) || ((MapBounds[position.X][position.Y] & 0x02) > 0) ||
                     ((position.X != 0) && ((MapBounds[position.X-1][position.Y] & 0x08) > 0)) )
                    return false;
            }
            else if (direction == MapDirection.Right)
            {
                // If the right side is blocked or at the edge
                if ( (position.X == MapWidth - 1) || ((MapBounds[position.X][position.Y] & 0x08) > 0) ||
                    ((position.X != MapWidth - 1) && ((MapBounds[position.X+1][position.Y] & 0x02) > 0)) )
                    return false;
            }

            // All clear
            return true;
        }

        /// <summary>
        /// Handles moving the camera around the screen
        /// </summary>
        /// <param name="axis">Specifies which axis to move the camera in</param>
        /// <param name="amount">Amount to move the camera</param>
        /// <remarks>Z axis is actually zoom, so it'll scale.</remarks>
        public static void MoveCamera(AxisType axis, ref float amount)
        {
            if (Camera == null)
                return;

            if (axis == AxisType.XAxis) // Pan Left/Right
            {
                Camera.MoveRight(ref amount);
            }
            else if (axis == AxisType.YAxis) // Pan Up/Down
            {
                Camera.MoveUp(ref amount);
            }
            else if (axis == AxisType.ZAxis) // Zoom In/Out
            {
                Camera.Zoom += amount;

                // Check to make sure the zoom isn't too much
                if (Camera.Zoom < Camera2D.minZoom) Camera.Zoom = Camera2D.minZoom;
                if (Camera.Zoom > Camera2D.maxZoom) Camera.Zoom = Camera2D.maxZoom;
            }
        }

        /// <summary>
        /// Handles rotating the camera in the screen
        /// </summary>
        /// <param name="amount">Amount to rotate the camera</param>
        public static void RotateCamera(ref float amount)
        {
            if (Camera != null)
                Camera.Rotation += amount;
        }

        public static void ResetCameraChange()
        {
            if (Camera != null)
                Camera.ResetChanged();
        }

        public static void HandleCameraChange()
        {
            if (Camera != null && Camera.IsChanged)
            {
                singleton.CameraChanged();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the camera to the center of the tile grid
        /// and reset the position of the animted sprite
        /// </summary>
        private static void ResetToInitialPositions()
        {
            //set up the 2D camera
            //set the initial position to the center of the
            //tile field
            // NOTE:  Is there a better way of updating the camera to the sprite?
            Camera.Position = new Vector2(cNumTiles);
            Camera.Rotation = 0f;
            Camera.Zoom = 1f;
            Camera.MoveUsingScreenAxis = true;

            singleton.CameraChanged();
        }

        /// <summary>
        /// This function is called when the camera's values have changed
        /// and is used to update the properties of the tiles and animated sprite
        /// </summary>
        private void CameraChanged()
        {
            //set rotation
            topLayer.CameraRotation = middleLayer.CameraRotation =
                bottomLayer.CameraRotation = player.Sprite.CameraRotation = camera.Rotation;

            //set zoom
            topLayer.CameraZoom = middleLayer.CameraZoom =
                bottomLayer.CameraZoom = camera.Zoom;
            player.Sprite.CameraZoom = camera.Zoom;

            //set position
            topLayer.CameraPosition = camera.Position;
            middleLayer.CameraPosition = camera.Position;
            bottomLayer.CameraPosition = camera.Position;
            player.Sprite.CameraPosition = camera.Position;

            // Every time the camera changes, also update the characters
            //player.Sprite.Origin = (camera.Position - player.Position) / cSpriteScale.X;

            //changes have been accounted for, reset the changed value so that this
            //function is not called unnecessarily
            camera.ResetChanged();
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Renders the game scene
        /// </summary>
        public static void Draw(GameTime gameTime, CharacterBase character)
        {
            if (singleton == null)
                return;

            singleton.DrawMap(gameTime, character);
        }

        /// <summary>
        /// Helper function for drawing the scene
        /// </summary>
        /// <param name="gameTime">Time elapsed since last draw</param>
        /// <param name="character">Character to draw on the screen</param>
        private void DrawMap(GameTime gameTime, CharacterBase character)
        {
            if (isMapLoaded)
            {
                // Clear the screen
                graphics.Clear(Color.Black);

                // Renders the 3 layers for the tile map
                if ((debugOn && showLayer[0]) || showLayer[0])
                    bottomLayer.Draw(spriteBatch);
                if ((debugOn && showLayer[1]) || showLayer[1])
                    middleLayer.Draw(spriteBatch);

                // NOTE:  Drawing the sprite between the middle layer and top layer.
                // This will allow the top layer to over lap for arches or other tall map
                // structures.
                character.Draw(spriteBatch, Color.White, SpriteBlendMode.AlphaBlend);

                if ((debugOn && showLayer[2]) || showLayer[2])
                    topLayer.Draw(spriteBatch);

                // TODO:  Load atmosphere layer that moves at a different speed than
                // the other layers

                // Checks to see if the camera moved
                if (camera.IsChanged)
                    CameraChanged();

                //character.Draw(spriteBatch, Color.White, SpriteBlendMode.AlphaBlend);

                // Update debug status
                MapDebug.StatusMsg = statusMsg;
  
                // Print out debug message
                MapDebug.Draw(gameTime, ChronosEngine.ScreenManager.SpriteBatch);
            }
        }

        #endregion

        #region Debugging Methods

        /// <summary>
        /// Handles input and used for controlling specific parameters for debugging
        /// </summary>
        /// <param name="input">Input from the keyboard</param>
        /// <param name="elapsed">Time different from last update</param>
        public static void processInput(InputState input, float elapsed)
        {
            // DEBUG CONTROLS
            if (DebugOn)
            {
                // Handles inputs for controlling the camera
                if (ControlCamera)
                {
                    //Set the camera's state to Unchanged for this frame
                    //this will save us from having to update visibility if the camera
                    //does not move
                    ResetCameraChange();

                    // Otherwise move the camera
                    Vector2 movement = Vector2.Zero;

                    // Check for camera movement
                    float dX = input.ReadKeyboardAxis(PlayerIndex.One, Keys.Left, Keys.Right) *
                        elapsed * cMovementRate;
                    float dY = input.ReadKeyboardAxis(PlayerIndex.One, Keys.Down, Keys.Up) *
                        elapsed * cMovementRate;
                    MoveCamera(AxisType.XAxis, ref dX);
                    MoveCamera(AxisType.YAxis, ref dY);

                    //check for camera rotation
                    dX = input.ReadKeyboardAxis(PlayerIndex.One, Keys.E, Keys.Q) *
                        elapsed * cRotationRate;
                    RotateCamera(ref dX);

                    //check for camera zoom
                    dX = input.ReadKeyboardAxis(PlayerIndex.One, Keys.X, Keys.Z) *
                        elapsed * cZoomRate;
                    MoveCamera(AxisType.ZAxis, ref dX);
                }

                // Toggles visibility for the layers
                if (input.IsNewKeyPress(Keys.NumPad0))
                {
                    ShowLayer[0] = !ShowLayer[0];
                    MapDebug.Add("[0] Toggling Bottom Layer");
                }
                if (input.IsNewKeyPress(Keys.NumPad1))
                {
                    ShowLayer[1] = !ShowLayer[1];
                    MapDebug.Add("[1] Toggling Middle Layer");
                }
                if (input.IsNewKeyPress(Keys.NumPad2))
                {
                    ShowLayer[2] = !ShowLayer[2];
                    MapDebug.Add("[2] Toggling Top Layer");
                }
                if (input.IsNewKeyPress(Keys.OemPeriod)) // Reset the layers
                {
                    for (int i = 0; i < 3; i++)
                        ShowLayer[i] = true;

                    MapDebug.Add("[.] Resetting All Layers");
                }

                // Toggle the ability to control the camera
                if (input.IsNewKeyPress(Keys.Divide))
                {
                    ControlCamera = !ControlCamera;
                    MapDebug.Add("[/] Toggling Camera Control");
                }

                // Toggle status message display
                if (input.IsNewKeyPress(Keys.NumPad5))
                {
                    MapDebug.ShowStatusMsg = !MapDebug.ShowStatusMsg;
                    MapDebug.Add("[5] Toggling Status Display");
                }

                // Reset the camera values
                if (input.IsNewKeyPress(Keys.Multiply))
                {
                    ResetToInitialPositions();
                    MapDebug.Add("[*] Resetting Camera Positions");
                }
            }
        }

        /// <summary>
        /// Updates status debugs to be printed
        /// </summary>
        /// <param name="character"></param>
        /// <param name="pos"></param>
        public static void UpdateDebugMsg(CharacterBase character, Point pos)
        {
            singleton.StatusMsg = "Rotation: " + Camera.Rotation + "  Position = X:" + Camera.Position.X +
    " Y:" + Camera.Position.Y + "  Zoom: " + Camera.Zoom + "\n";
            singleton.StatusMsg += "Char: X=" + character.Position.X + " Y:=" + character.Position.Y + "\n";
            singleton.StatusMsg += "MapChar: X=" + pos.X + " Y=" + pos.Y;
        }

        #endregion
    }
}
