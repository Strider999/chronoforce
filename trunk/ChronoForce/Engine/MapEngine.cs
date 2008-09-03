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
        const int cTileOffset = 1; // Padding

        // Maximum number of actors on a map
        const int cMaxActors = 20;
        const int cActorMoveTime = 2000; // Time delay before moving an actor in milliseconds
        readonly Vector2 cSpriteScale = new Vector2(1f, 1f);

        // FIX:  These shouldn't be here since it's already in WorldDirector.  Does this
        // mean the World Director should keep track of the NPCs, or should there be
        // a function in World Director that handles NPCs?
        const int cXAmount = 39;
        const int cYAmount = 39;

        // Constant vectors of movement directions
        readonly Vector2 cMoveUp = new Vector2(0, -cYAmount);
        readonly Vector2 cMoveDown = new Vector2(0, cYAmount);
        readonly Vector2 cMoveRight = new Vector2(cXAmount, 0);
        readonly Vector2 cMoveLeft = new Vector2(-cXAmount, 0);

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
        // FIX:  Should the character map positions be stored in CharacterBase?  This
        // would make the position in the XML more intuitive, or maybe the position in 
        // the XML isn't needed?
        Point[] mapActorsPos = new Point[cMaxActors];
        List<ActionSlot> mapActions = new List<ActionSlot>(cMaxActors);
        int npcTimer = 0;
        CharacterBase player;

        // Random generator for moving the map actors
        Random rand = new Random();

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
        Vector2 mapSize = new Vector2();
        int[][] mapBounds;
        int[][] mapCodes;

        // FIX:  WorldDirector should be created here and not passed in, but for testing
        // we'll pass in the WorldDirector to make sure it works
        WorldDirector director;

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
        /// Returns the current map height in tiles
        /// </summary>
        private static int MapHeight
        {
            get { return singleton.mapHeight; }
        }

        /// <summary>
        /// Returns the current map width in tiles
        /// </summary>
        private static int MapWidth
        {
            get { return singleton.mapWidth; }
        }

        /// <summary>
        /// Returns the current map size in pixels
        /// </summary>
        private static Vector2 MapSize
        {
            get { return singleton.mapSize; }
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

            // Store the pixel size of the map
            mapSize.X = mapWidth * (tileWidth - cTileOffset);
            mapSize.Y = mapHeight * (tileHeight - cTileOffset);

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

            // DEBUG:  Create NPCs for testing
            for (int i = 0; i < 2; i++)
            {
                CharacterBase npc = new CharacterBase(contents.Load<CharacterBase>("TestNPC"));
                npc.Sprite.ScreenCenter = ChronosSetting.WindowSize / 2;
                mapActorsPos[i] = new Point(i + 3, i + 3);
                npc.Position = new Vector2(mapActorsPos[i].X * tileWidth,
                                    mapActorsPos[i].Y * tileHeight);
                mapActors.Add(npc);
                mapActions.Add(new ActionSlot());
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
            GraphicsDevice graphics, ContentManager content, CharacterBase playerArg,
            WorldDirector director)
        {
            // Clears any previously loaded map
            ClearMapEngine();

            // Create a new instance of the MapEngine
            singleton = new MapEngine(graphics, content);

            // Set the player
            Player = playerArg;

            // DEBUG:  Load the XML
            ActionScript tester = content.Load<ActionScript>("TestScript");

            // FIX:  Store the world director
            singleton.director = director;

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

                // Check for NPCs
                for (int i = 0; i < singleton.mapActors.Count; i++)
                {
                    if (singleton.mapActorsPos[i].X == position.X &&
                        singleton.mapActorsPos[i].Y == position.Y - 1)
                        return false;
                }

                // Check against the player (for NPCs)
                if (position.X == singleton.player.Position.X &&
                    position.Y == singleton.player.Position.Y - 1)
                    return false;
            }
            else if (direction == MapDirection.Down)
            {
                // If the bottom is blocked or at the edge
                if ( (position.Y == MapHeight - 1 ) || ((MapBounds[position.X][position.Y] & 0x04) > 0) ||
                    ((position.Y != MapHeight - 1 ) && ((MapBounds[position.X][position.Y+1] & 0x01) > 0)) )
                    return false;

                // Check for NPCs
                for (int i = 0; i < singleton.mapActors.Count; i++)
                {
                    if (singleton.mapActorsPos[i].X == position.X &&
                        singleton.mapActorsPos[i].Y == position.Y + 1)
                        return false;
                }

                // Check against the player (for NPCs)
                if (position.X == singleton.player.Position.X &&
                    position.Y == singleton.player.Position.Y + 1)
                    return false;
            }
            else if (direction == MapDirection.Left)
            {
                // If the left side is blocked or at the edge
                if ( (position.X == 0) || ((MapBounds[position.X][position.Y] & 0x02) > 0) ||
                     ((position.X != 0) && ((MapBounds[position.X-1][position.Y] & 0x08) > 0)) )
                    return false;

                // Check for NPCs
                for (int i = 0; i < singleton.mapActors.Count; i++)
                {
                    if (singleton.mapActorsPos[i].X == position.X - 1 &&
                        singleton.mapActorsPos[i].Y == position.Y)
                        return false;
                }

                // Check against the player (for NPCs)
                if (position.X == singleton.player.Position.X - 1&&
                    position.Y == singleton.player.Position.Y)
                    return false;
            }
            else if (direction == MapDirection.Right)
            {
                // If the right side is blocked or at the edge
                if ( (position.X == MapWidth - 1) || ((MapBounds[position.X][position.Y] & 0x08) > 0) ||
                    ((position.X != MapWidth - 1) && ((MapBounds[position.X+1][position.Y] & 0x02) > 0)) )
                    return false;

                // Check for NPCs
                for (int i = 0; i < singleton.mapActors.Count; i++)
                {
                    if (singleton.mapActorsPos[i].X == position.X + 1 &&
                        singleton.mapActorsPos[i].Y == position.Y)
                        return false;
                }

                // Check against the player (for NPCs)
                if (position.X == singleton.player.Position.X + 1 &&
                    position.Y == singleton.player.Position.Y)
                    return false;
            }

            // All clear
            return true;
        }

        /// <summary>
        /// Handles moving the player
        /// </summary>
        /// <param name="direction">Direction the player is moving</param>
        public static void MovePlayer(MapDirection direction)
        {

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
        /// Handles moving the camera with absolute coordinates
        /// </summary>
        /// <param name="pos">New position of the camera in (x,y)</param>
        public static void MoveCamera(Vector2 pos)
        {
            if (Camera == null)
                return;

            // Makes sure the camera position will be within bounds.  Don't want to go 
            // past the edge of the map
            pos.X = MathHelper.Clamp(pos.X, ChronosSetting.WindowWidth / 2, 
                MapSize.X - (ChronosSetting.WindowWidth / 2));
            pos.Y = MathHelper.Clamp(pos.Y, ChronosSetting.WindowHeight / 2,
                MapSize.Y - (ChronosSetting.WindowHeight / 2));

            Camera.Position = pos;
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

            // Update the values with the NPCs
            for (int i = 0; i < mapActors.Count; i++)
            {
                mapActors[i].Sprite.CameraRotation = camera.Rotation;
                mapActors[i].Sprite.CameraZoom = camera.Zoom;
                mapActors[i].Sprite.CameraPosition = camera.Position;
            }

            //changes have been accounted for, reset the changed value so that this
            //function is not called unnecessarily
            camera.ResetChanged();
        }

        /// <summary>
        /// Moves the NPCs around the screen
        /// </summary>
        private void MoveNPC()
        {
            int direction;
            bool moved;

            for (int i = 0; i < mapActors.Count; i++)
            {
                mapActions[i].Action = ActionCommand.MoveTo;
                mapActions[i].Actor = mapActors[i];
                mapActions[i].IsAbsolute = false;
                mapActions[i].Frames = 30;
                moved = false;

                // Keep trying until the character can move to an empty spot
                while (moved == false)
                {
                    direction = rand.Next(4);

                    switch (direction)
                    {
                        case 0: // Up
                            // Check to see if the character can move that direction
                            if (MapEngine.IsPassable(mapActorsPos[i], MapDirection.Up))
                            {
                                mapActors[i].Sprite.Direction = "Back";
                                mapActors[i].Sprite.Motion = "Walk";
                                mapActorsPos[i].Y--;
                                mapActions[i].EndPosition = cMoveUp;
                                director.AddActionSlot(mapActions[i]);
                                moved = true;
                            }
                            break;
                        case 1: // Down
                            // Check to see if the character can move that direction
                            if (MapEngine.IsPassable(mapActorsPos[i], MapDirection.Down))
                            {
                                mapActors[i].Sprite.Direction = "Front";
                                mapActors[i].Sprite.Motion = "Walk";
                                mapActorsPos[i].Y++;
                                mapActions[i].EndPosition = cMoveDown;
                                director.AddActionSlot(mapActions[i]);
                                moved = true;
                            }
                            break;
                        case 2: // Right
                            // Check to see if the character can move that direction
                            if (MapEngine.IsPassable(mapActorsPos[i], MapDirection.Right))
                            {
                                mapActors[i].Sprite.Direction = "Right";
                                mapActors[i].Sprite.Motion = "Walk";
                                mapActorsPos[i].X++;
                                mapActions[i].EndPosition = cMoveRight;
                                director.AddActionSlot(mapActions[i]);
                                moved = true;
                            }
                            break;
                        case 3: // Left
                            // Check to see if the character can move that direction
                            if (MapEngine.IsPassable(mapActorsPos[i], MapDirection.Left))
                            {
                                mapActors[i].Sprite.Direction = "Left";
                                mapActors[i].Sprite.Motion = "Walk";
                                mapActorsPos[i].X--;
                                mapActions[i].EndPosition = cMoveLeft;
                                director.AddActionSlot(mapActions[i]);
                                moved = true;
                            }
                            break;
                    }
                }

                mapActions[i].Reset();
            }
        }

        #endregion

        #region Updating

        /// <summary>
        /// Updates the state of the map engine.
        /// </summary>
        /// <param name="elapsed">Milliseconds since last update</param>
        public static void Update(int elapsed)
        {
            if (singleton == null)
                return;

            singleton.UpdateEngine(elapsed);
        }

        /// <summary>
        /// Helper function for the static update function
        /// </summary>
        /// <param name="elapsed">Milliseconds since last update</param>
        private void UpdateEngine(int elapsed)
        {
            npcTimer += elapsed;

            if (npcTimer > cActorMoveTime)
            {
                npcTimer -= cActorMoveTime;
                MoveNPC();
            }

            // Update the NPCs
            for (int i = 0; i < mapActors.Count; i++)
            {
                mapActors[i].Update(elapsed);
            }
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

                // Checks to see if the camera moved
                if (camera.IsChanged)
                    CameraChanged();

                // Renders the 3 layers for the tile map
                if ((debugOn && showLayer[0]) || showLayer[0])
                    bottomLayer.Draw(spriteBatch);
                if ((debugOn && showLayer[1]) || showLayer[1])
                    middleLayer.Draw(spriteBatch);

                // NOTE:  Drawing the sprite between the middle layer and top layer.
                // This will allow the top layer to over lap for arches or other tall map
                // structures.
                // Draw the NPCs
                for (int i = 0; i < mapActors.Count; i++)
                {
                    mapActors[i].Draw(spriteBatch, Color.White, SpriteBlendMode.AlphaBlend);
                }

                // Draw the main party
                character.Draw(spriteBatch, Color.White, SpriteBlendMode.AlphaBlend);

                if ((debugOn && showLayer[2]) || showLayer[2])
                    topLayer.Draw(spriteBatch);

                // TODO:  Load atmosphere layer that moves at a different speed than
                // the other layers

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

        /// <summary>
        /// Adds a custom debug message from another class
        /// </summary>
        /// <param name="msg">Custom debug message to display</param>
        public static void AddDebugMsg(string msg)
        {
            MapDebug.Add(msg);
        }

        #endregion
    }
}
