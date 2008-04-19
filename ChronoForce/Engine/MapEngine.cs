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
using ChronoForce.Character;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// World map and dungeon engine that handles rendering the world and transitioning between
    /// maps, rooms, and battles.
    /// </summary>
    public class MapEngine
    {
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
        #endregion

        #region Fields
        // Graphical parameters needed to render the sprites and tiles
        GraphicsDevice graphics;
        SpriteBatch spriteBatch;
        ContentManager contents;
        Camera2D camera;
        Random rnd;

        // Tile information
        SpriteSheet tileSheet;
        TileGrid bottomLayer;
        TileGrid middleLayer;
        TileGrid topLayer;

        // Animated sprites
        //private SpriteSheet animatedSpriteSheet;
        //private AnimatedSprite animatedSprite;
        //private Vector2 animatedSpritePosition;
        //private float accumulator;

        // Flag to see if the engine has loaded anything
        bool isMapLoaded;
        // Flag for debug control
        bool debugOn;
        // String for debug status
        string statusMsg;
        // Debugger class for printing debug data
        Debugger MapDebug;

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

        #region Public Accessors

        public bool DebugOn
        {
            set 
            { 
                debugOn = value;
                MapDebug.DebugOn = value;
            }
            get { return debugOn; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor loads and initializes the class
        /// </summary>
        /// <param name="graphicsComponent">Place where to render the graphics to</param>
        /// <param name="contentManager">Content to load from</param>
        public MapEngine(GraphicsDevice graphicsComponent, ContentManager contentManager)
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
            MapDebug = new Debugger("MapDebug", graphics, contentManager, debugOn);
            showLayer = new bool[3];

            for (int i = 0; i < 3; i++)
                showLayer[i] = true;

            controlCamera = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the specified map from the passed filename
        /// </summary>
        /// <param name="filename">Map file to load</param>
        /// <returns>True if success, false if there's an error</returns>
        public bool LoadMap(string filename)
        {
            // First, check to see if the filename has the right format
            if (!filename.Contains(".map"))
            {
                MapDebug.debugPrint("Filename doesn't end in .map");
                return false;
            }
            
            // Checks to make sure the file is there
            if (!File.Exists(filename))
            {
                string msg = "File " + filename +" doesn't exist";
                MapDebug.debugPrint(msg);
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

        /// <summary>
        /// Handles moving the camera around the screen
        /// </summary>
        /// <param name="axis">Specifies which axis to move the camera in</param>
        /// <param name="amount">Amount to move the camera</param>
        /// <remarks>Z axis is actually zoom, so it'll scale.</remarks>
        public void MoveCamera(AxisType axis, ref float amount)
        {
            if (camera == null)
                return;

            if (axis == AxisType.XAxis) // Pan Left/Right
            {
                camera.MoveRight(ref amount);
            }
            else if (axis == AxisType.YAxis) // Pan Up/Down
            {
                camera.MoveUp(ref amount);
            }
            else if (axis == AxisType.ZAxis) // Zoom In/Out
            {
                camera.Zoom += amount;

                // Check to make sure the zoom isn't too much
                if (camera.Zoom < Camera2D.minZoom) camera.Zoom = Camera2D.minZoom;
                if (camera.Zoom > Camera2D.maxZoom) camera.Zoom = Camera2D.maxZoom;
            }
        }

        /// <summary>
        /// Handles rotating the camera in the screen
        /// </summary>
        /// <param name="amount">Amount to rotate the camera</param>
        public void RotateCamera(ref float amount)
        {
            if (camera != null)
                camera.Rotation += amount;
        }

        public void ResetCameraChange()
        {
            if (camera != null)
                camera.ResetChanged();
        }

        public void HandleCameraChange()
        {
            if (camera != null && camera.IsChanged)
            {
                CameraChanged();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the camera to the center of the tile grid
        /// and reset the position of the animted sprite
        /// </summary>
        private void ResetToInitialPositions()
        {
            //set up the 2D camera
            //set the initial position to the center of the
            //tile field
            camera.Position = new Vector2(cNumTiles);
            camera.Rotation = 0f;
            camera.Zoom = 1f;
            camera.MoveUsingScreenAxis = true;

            CameraChanged();
        }

        /// <summary>
        /// This function is called when the camera's values have changed
        /// and is used to update the properties of the tiles and animated sprite
        /// </summary>
        private void CameraChanged()
        {
            //set rotation
            topLayer.CameraRotation = middleLayer.CameraRotation =
                bottomLayer.CameraRotation = camera.Rotation;

            //set zoom
            topLayer.CameraZoom = middleLayer.CameraZoom =
            bottomLayer.CameraZoom = camera.Zoom;

            //set position
            topLayer.CameraPosition = camera.Position;
            middleLayer.CameraPosition = camera.Position;
            bottomLayer.CameraPosition = camera.Position;

            //changes have been accounted for, reset the changed value so that this
            //function is not called unnecessarily
            camera.ResetChanged();
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Renders the game scene
        /// </summary>
        public void Draw(GameTime gameTime, CharacterBase character)
        {
            if (isMapLoaded)
            {
                // Clear the screen
                graphics.Clear(Color.Black);
                
                // Renders the 3 layers for the tile map
                if ( (debugOn && showLayer[0]) || showLayer[0] )
                    bottomLayer.Draw(spriteBatch);
                if ( (debugOn && showLayer[1]) || showLayer[1] )
                    middleLayer.Draw(spriteBatch);

                // NOTE:  Drawing the sprite between the middle layer and top layer.
                // This will allow the top layer to over lap for arches or other tall map
                // structures.
                character.Draw(spriteBatch, Color.White, SpriteBlendMode.None);

                if ( (debugOn && showLayer[2]) || showLayer[2] )
                    topLayer.Draw(spriteBatch);

                // TODO:  Load atmosphere layer that moves at a different speed than
                // the other layers

                // Checks to see if the camera moved
                if (camera.IsChanged)
                    CameraChanged();

                // Update debug status
                statusMsg = "Rotation: " + camera.Rotation + "  Position = X:" + camera.Position.X +
                    " Y:" + camera.Position.Y + "  Zoom: " + camera.Zoom;
                MapDebug.StatusMsg = statusMsg;

                // Print out debug message
                MapDebug.Draw(gameTime);
            }
        }
        #endregion

        #region Debugging Methods

        /// <summary>
        /// Handles input and used for controlling specific parameters for debugging
        /// </summary>
        /// <param name="input">Input from the keyboard</param>
        /// <param name="elapsed">Time different from last update</param>
        public void processInput(InputState input, float elapsed)
        {
            // DEBUG CONTROLS
            if (debugOn)
            {
                // Handles inputs for controlling the camera
                if (controlCamera)
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
                    showLayer[0] = !showLayer[0];
                    MapDebug.Add("[0] Toggling Bottom Layer");
                }
                if (input.IsNewKeyPress(Keys.NumPad1))
                {
                    showLayer[1] = !showLayer[1];
                    MapDebug.Add("[1] Toggling Middle Layer");
                }
                if (input.IsNewKeyPress(Keys.NumPad2))
                {
                    showLayer[2] = !showLayer[2];
                    MapDebug.Add("[2] Toggling Top Layer");
                }
                if (input.IsNewKeyPress(Keys.OemPeriod)) // Reset the layers
                {
                    for (int i = 0; i < 3; i++)
                        showLayer[i] = true;

                    MapDebug.Add("[.] Resetting All Layers");
                }

                // Toggle the ability to control the camera
                if (input.IsNewKeyPress(Keys.Divide))
                {
                    controlCamera = !controlCamera;
                    MapDebug.Add("[/] Toggling Camera Control");
                }

                // Toggle status message display
                if (input.IsNewKeyPress(Keys.NumPad5))
                {
                    MapDebug.ShowStatusMsg = !MapDebug.ShowStatusMsg;
                    MapDebug.Add("[5] Toggling Status Display");
                }
            }
        }

        #endregion
    }
}
