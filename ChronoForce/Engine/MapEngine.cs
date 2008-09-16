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
using ChronoForceData;
using ChronoForceData.Character;
using ChronoForceData.Actions;
using ChronoForceData.Base;
using ChronoForceData.Map;
#endregion

namespace ChronoForce.Engine
{
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

        // Delay between frames of animation
        const float cAnimationTime = 0.1f;
        
        // Maximum number of actors on a map
        const int cMaxActors = 20;
        const int cActorMoveTime = 2000; // Minimum time delay before moving an actor in milliseconds
        readonly Vector2 cSpriteScale = new Vector2(1f, 1f);

        // Cinamtic time delay for each frame
        const int cCinematicTime = 2;
        // Fade amount for each frame
        const int cFadeAmount = 40;

        #endregion

        #region Fields

        // Graphical parameters needed to render the sprites and tiles
        GraphicsDevice graphics;
        SpriteBatch spriteBatch;
        ContentManager contents;
        Camera2D camera;

        // Map Information
        MapData mapInfo;

        // Sprite information
        List<CharacterNPC> mapActors = new List<CharacterNPC>(cMaxActors);
        CharacterBase player;
        // Keeps track of NPCs on the map.  The number stored corresponds to the NPC IDs.
        // mapNPC[0] represents NPCs on Map #0 up to the int - 1.  mapNPC[1] represents NPCs on
        // Map #1 from the mapNPC[0]+1 to the current - 1, and so on.
        List<int> mapNPC = new List<int>();

        // Random generator for moving the map actors
        Random rand = new Random();
        
        // True if the camera is bound to the map boundaries or not
        bool cameraBound = true;
        // Ture if the camera is tracking the player
        bool followParty = true;
        // Flag for debug control
        bool debugOn;
        // String for debug status
        string statusMsg;
        // Debugger class for printing debug data
        Debugger mapDebug;
        // Flag to show the map is transitioning between one another or during
        // actual cinematics
        bool inCinematic = false;
        // Timer for cinematics
        int cinematicTimer = 0;

        // Color for doing fade controls
        byte[] fadeColor = new byte[2];

        // Debugging variables to control
        bool[] showLayer;
        bool controlCamera;

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
        /// Returns the map data
        /// </summary>
        private static MapData MapInfo
        {
            get { return singleton.mapInfo; }
        }

        /// <summary>
        /// Returns the current map size
        /// </summary>
        private static Vector2 MapSize
        {
            get { return singleton.mapInfo.MapSize; }
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
        /// Returns true if the engine is running a cinematic
        /// </summary>
        public static bool InCinematic
        {
            get { return singleton.inCinematic; }
        }

        /// <summary>
        /// Returns the map debugger
        /// </summary>
        private static Debugger MapDebug
        {
            get { return singleton.mapDebug; }
        }

        /// <summary>
        /// Returns the world director
        /// </summary>
        private static WorldDirector Director
        {
            get { return singleton.director; }
        }

        /// <summary>
        /// Whether or not the camera should follow the party
        /// </summary>
        public static bool FollowParty
        {
            get { return singleton.followParty; }
            set { singleton.followParty = value; }
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

        #region Event Handlers

        /// <summary>
        /// Handles moving the NPCs when ready
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void ReadyHandler(object o, NumberEventArgs e)
        {
            // Move the specific NPC
            MoveNPC(e.ID);
        }

        /// <summary>
        /// Handles reseting following the party and camera bounds after a
        /// map transition.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void CameraDoneHandler(object o, EventArgs e)
        {
            // If this is the base map, turn on map bounds
            if (mapInfo.CurrentMapLevel == 0)
                cameraBound = true;

            // In either case, turn back on party following
            followParty = true;
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

            // Initialize debug controls and variables
            debugOn = true;
            mapDebug = new Debugger("MapDebug", debugOn);
            showLayer = new bool[3];

            for (int i = 0; i < 3; i++)
                showLayer[i] = true;

            fadeColor[0] = 255;
            fadeColor[1] = 0;

            controlCamera = false;
        }

        #endregion

        #region Loading Map


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

            // FIX:  Store the world director
            singleton.director = director;

            // Load the map file
            //singleton.mapInfo = new MapData(graphics, content, filename);
            singleton.mapInfo = content.Load<MapData>(@"Maps/MapTest");

            // Load tests
            singleton.LoadTests();

            // Set Up a 2D Camera
            singleton.camera = new Camera2D();

            ResetToInitialPositions();

            return true;
        }

        /// <summary>
        /// For DEBUGGING
        /// </summary>
        private void LoadTests()
        {
            // DEBUG:  Create NPCs for testing
            for (int i = 0; i < 3; i++)
            {
                CharacterNPC npc = new CharacterNPC(contents.Load<CharacterNPC>("TestNPC"));
                npc.Sprite.ScreenCenter = ChronosSetting.WindowSize / 2;
                npc.MapPosition = new Point(rand.Next(1, 10), rand.Next(1, 10));
                npc.Position = 
                    new Vector2(npc.MapPosition.X * mapInfo.TileWidth - (npc.Sprite.FrameDimension.X - mapInfo.TileWidth),
                                    npc.MapPosition.Y * mapInfo.TileHeight - (npc.Sprite.FrameDimension.Y - mapInfo.TileHeight));
                npc.ID = i;
                npc.Ready += ReadyHandler;
                npc.Timer = cActorMoveTime + rand.Next(2000);
                mapActors.Add(npc);
            }

            // DEBUG:  Adds the NPCs to the map
            for (int i = 0; i < mapInfo.BottomGrid.Count; i++)
                mapNPC.Add(3);

            // DEBUG:  Position the player correctly (probably needed, but maybe not here)
            player.Position = new Vector2(-1 * (player.Sprite.FrameDimension.X - mapInfo.TileWidth),
                -1 * (player.Sprite.FrameDimension.Y - mapInfo.TileHeight));
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

        #endregion

        #region Camera Functions

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
            // past the edge of the map.
            if (singleton.cameraBound)
            {
                if (MapSize.X > ChronosSetting.WindowWidth / 2)
                    pos.X = MathHelper.Clamp(pos.X, ChronosSetting.WindowWidth / 2,
                        MapSize.X - (ChronosSetting.WindowWidth / 2));

                if (MapSize.Y > ChronosSetting.WindowHeight / 2)
                    pos.Y = MathHelper.Clamp(pos.Y, ChronosSetting.WindowHeight / 2,
                        MapSize.Y - (ChronosSetting.WindowHeight / 2));
            }

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

        /// <summary>
        /// Reset the camera to the center of the tile grid
        /// and reset the position of the animted sprite
        /// </summary>
        private static void ResetToInitialPositions()
        {
            //set up the 2D camera
            //set the initial position to the center of the
            //tile field
            Camera.Position = new Vector2(ChronosSetting.WindowSize.X, ChronosSetting.WindowSize.Y);
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
            // Update the map views
            mapInfo.UpdateCameraView(camera);

            // Update player views
            player.Sprite.CameraRotation = camera.Rotation;
            player.Sprite.CameraZoom = camera.Zoom;
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles moving the player from keyboard/gamepad input
        /// </summary>
        /// <param name="direction">Direction the player is moving</param>
        public static void MovePlayer(MapDirection direction)
        {
            int dx = 0;
            int dy = 0;

            // If we're in a cinematic, ignore movement commands
            if (InCinematic)
                return;

            // First, change the character to face that direction
            switch (direction)
            {
                case MapDirection.Down:
                    Player.Sprite.Direction = "Front";
                    dy = 1;
                    break;
                case MapDirection.Up:
                    Player.Sprite.Direction = "Back";
                    dy = -1;
                    break;
                case MapDirection.Left:
                    Player.Sprite.Direction = "Left";
                    dx = -1;
                    break;
                case MapDirection.Right:
                    Player.Sprite.Direction = "Right";
                    dx = 1;
                    break;
            }

            // Next, Check to see if the character can move that direction
            if (MapInfo.IsPassable(Player.MapPosition, direction, MapInfo.CurrentMap) &&
                singleton.NotBlocked(Player.MapPosition, direction) )
            {
                Player.MoveMapPosition(dx, dy);
                Director.MoveParty(Player, direction);

                // Check any map codes in the area
                singleton.CheckMapCode(dx, dy);
            }
        }

        /// <summary>
        /// Checks to see if the character can talk to anyone from where it's facing
        /// </summary>
        /// <param name="name">Name of NPC talking</param>
        /// <param name="text">Output of the text the NPC speaks</param>
        /// <returns>True if there's a character</returns>
        public static bool CanTalk(out string name, out string text)
        {
            int dx = 0, dy = 0;
            string npcFace = "Front";

            // First, determine the direction based on the sprite direction
            switch (Player.Sprite.Direction)
            {
                case "Front":
                    npcFace = "Back";
                    dy = 1;
                    break;
                case "Back":
                    npcFace = "Front";
                    dy = -1;
                    break;
                case "Left":
                    npcFace = "Right";
                    dx = -1;
                    break;
                case "Right":
                    npcFace = "Left";
                    dx = 1;
                    break;
            }

            // Check to see if there's any NPCs in the area
            int startI = 0;
            if (singleton.mapInfo.CurrentMap != 0)
                startI = singleton.mapNPC[singleton.mapInfo.CurrentMap - 1];

            for (int i = startI; i < singleton.mapNPC[singleton.mapInfo.CurrentMap]; i++)
            {
                if (singleton.mapActors[i].MapPosition.X == Player.MapPosition.X + dx &&
                    singleton.mapActors[i].MapPosition.Y == Player.MapPosition.Y + dy)
                {
                    // Found an NPC, so we make the NPC face the player and return the dialog text
                    singleton.mapActors[i].Sprite.Direction = npcFace;
                    name = singleton.mapActors[i].Name;
                    text = singleton.mapActors[i].DialogText[0];
                    return true;
                }
            }

            // Didn't find anything
            name = "";
            text = "";
            return false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the path is clear based on where the NPC and player is
        /// </summary>
        /// <param name="position">Position of the character</param>
        /// <param name="direction">Direction the character is moving</param>
        /// <returns>True if the path is clear</returns>
        private bool NotBlocked(Point position, MapDirection direction)
        {
            int dx = 0, dy = 0;
            // Get the modifiers based on the direction
            switch (direction)
            {
                case MapDirection.Up:
                    dy = -1;
                    break;
                case MapDirection.Down:
                    dy = 1;
                    break;
                case MapDirection.Left:
                    dx = -1;
                    break;
                case MapDirection.Right:
                    dx = 1;
                    break;
            }

            // Check for NPCs
            int startI = 0;
            if (mapInfo.CurrentMap != 0)
                startI = mapNPC[mapInfo.CurrentMap - 1];

            for (int i = startI; i < mapNPC[mapInfo.CurrentMap]; i++)
            {
                if (mapActors[i].MapPosition.X == position.X + dx &&
                    mapActors[i].MapPosition.Y == position.Y + dy)
                    return false;
            }

            // Check against the player (for NPCs)
            if (position.X + dx == player.MapPosition.X &&
                position.Y + dy == player.MapPosition.Y)
                return false;

            return true;
        }

        /// <summary>
        /// Moves the NPCs around the screen
        /// </summary>
        /// <param name="id">ID of the NPC to move</param>
        private void MoveNPC(int id)
        {
            MapDirection direction;
            bool moved;
            int dx, dy;
            int map;

            // First, check to see if the NPC can move.  Technically, should never reach here
            // if the NPC can't move, but just in case
            if (mapActors[id].Moves)
            {
                // Find out the map the NPC is on
                for (map = 0; map < mapNPC.Count; map++)
                {
                    // If the NPC is greater than the mapNPC number, that means
                    // the NPC isn't on this map.  Otherwise, we found the map.
                    if (id < mapNPC[map])
                        break;
                }

                moved = false;

                // Keep trying until the character can move to an empty spot
                //while (moved == false)
                {
                    dx = dy = 0;
                    direction = (MapDirection)rand.Next(4);

                    switch (direction)
                    {
                        case MapDirection.Up:
                            mapActors[id].Sprite.Direction = "Back";
                            dy = -1;
                            break;
                        case MapDirection.Down:
                            mapActors[id].Sprite.Direction = "Front";
                            dy = 1;
                            break;
                        case MapDirection.Left:
                            mapActors[id].Sprite.Direction = "Left";
                            dx = -1;
                            break;
                        case MapDirection.Right:
                            mapActors[id].Sprite.Direction = "Right";
                            dx = 1;
                            break;
                    }

                    // Check to see if the NPC can move that direction
                    if (mapInfo.IsPassable(mapActors[id].MapPosition, direction, map) &&
                        NotBlocked(mapActors[id].MapPosition, direction))
                    {
                        mapActors[id].MoveMapPosition(dx, dy);
                        Director.MoveNPC(mapActors[id], direction);
                        moved = true;
                    }
                }

                // Since the NPC has moved, reset the timer on it
                mapActors[id].Timer = cActorMoveTime + rand.Next(2000);
            }
        }

        /// <summary>
        /// Checks map codes to see if the character is on a teleportation code.
        /// </summary>
        /// <remarks>This will check for more codes later on, including
        /// in-game cinematics, quest objects, and event triggered items from a file.</remarks>
        private void CheckMapCode(int dx, int dy)
        {
            // Find the map code action from the list if it exists
            MapEntry<MapCodeAction> entry = mapInfo.CodeEntries.Find(
                delegate(MapEntry<MapCodeAction> testEntry)
                {
                    return
                        (testEntry.ContentName.Contains(mapInfo.MapName[mapInfo.CurrentMap])) &&
                        (testEntry.MapPosition == player.MapPosition);
                });

            if (entry != null)
            {
                // Start transitioning to the new map
                mapInfo.LastMap = mapInfo.CurrentMap;
                mapInfo.CurrentMap = entry.Content.DestinationMap;
                mapInfo.CurrentMapLevel = entry.Content.DestinationMapLevel;

                inCinematic = true;

                // Transition the map
                TransitionMap(entry.Content.DestinationMapPosition, dx, dy);
            }
        }

        /// <summary>
        /// Transitions between maps, specifically for indoor/outdoor situations
        /// and floors of a building
        /// </summary>
        /// <param name="position">Exit position</param>
        private void TransitionMap(Point position, int dx, int dy)
        {
            Vector2 newPosition = new Vector2();

            // Use the direction to get the proper offset

            // Change the map position to the new map position based on the exit position
            player.MapPosition = position;

            // If the player position isn't the same as the camera, then we're at an edge case
            // Move the camera to where the player is, update the position, and then update
            // the maps so everything remains centered.
            cameraBound = false;

            if (MapInfo.CurrentMapLevel == 0)
            {
                // Check the bounds of the map so the camera will move correctly
                newPosition.X = MathHelper.Clamp(player.Position.X,
                    ChronosSetting.WindowWidth / 2,
                    mapInfo.MapSize.X - (ChronosSetting.WindowWidth / 2));
                newPosition.Y = MathHelper.Clamp(player.Position.Y,
                    ChronosSetting.WindowHeight / 2,
                    mapInfo.MapSize.Y - (ChronosSetting.WindowHeight / 2));

                director.AddActionSlot(new ActionSlot(ActionCommand.MoveTo, camera, 
                    newPosition, 10, true));
            }
            else
            {
                // Follow the party to the exit position
                // FIX:  Need a way to tell it to go 40 px or whatever the character movement
                // will be.  Place this inside the constants data?
                newPosition.X = player.Position.X + (dx * 40);
                newPosition.Y = player.Position.Y + (dy * 40);
                director.AddActionSlot(new ActionSlot(ActionCommand.MoveTo, camera, 
                    newPosition, 10, true));
            }

            // Disable following the party
            followParty = false;

            // When the camera is finished moving, handle the signal
            director.SlotDone += CameraDoneHandler;
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
            // If we're in a cinematic, don't move the NPCs here and update fade color
            // FIX:  The cinematic should update according to what type of cinematic is
            // going on.  For now, we only assume map transitions.
            if (inCinematic)
            {
                cinematicTimer += elapsed;

                // When a frame of action passes, update the alpha values
                if (cinematicTimer > cCinematicTime)
                {
                    cinematicTimer -= cCinematicTime;
                    fadeColor[0] = (byte)MathHelper.Clamp(fadeColor[0] - cFadeAmount, 0, 255);
                    fadeColor[1] = (byte)MathHelper.Clamp(fadeColor[1] + cFadeAmount, 0, 255);

                    // Update the tile grids for the new colors
                    mapInfo.SetFade(mapInfo.LastMap, fadeColor[0]);
                    mapInfo.SetFade(mapInfo.CurrentMap, fadeColor[1]);

                    // When fadeColor[0] reaches 0, the cinematic is complete
                    if (fadeColor[0] == 0)
                    {
                        inCinematic = false;
                        fadeColor[0] = 255;
                        fadeColor[1] = 0;
                        mapInfo.LastMap = mapInfo.CurrentMap;
                    }
                }
            }

            // Update the camera
            if (followParty)
                MoveCamera(player.Position);

            // Update the NPCs
            for (int i = 0; i < mapActors.Count; i++)
            {
                mapActors[i].Update(elapsed);
            }

            // Update the Player
            player.Update(elapsed);

            // If the camera changed, update the positions of the maps and sprites
            if (camera.IsChanged)
                CameraChanged();
        }

        #endregion

        #region Drawing and Rendering

        /// <summary>
        /// Renders the game scene
        /// </summary>
        public static void Draw(GameTime gameTime)
        {
            if (singleton == null)
                return;

            singleton.DrawMap(gameTime);
        }

        /// <summary>
        /// Helper function for drawing the scene
        /// </summary>
        /// <param name="gameTime">Time elapsed since last draw</param>
        /// <param name="character">Character to draw on the screen</param>
        private void DrawMap(GameTime gameTime)
        {
            // Clear the screen
            graphics.Clear(Color.Black);

            // Checks to see if the camera moved
            if (camera.IsChanged)
                CameraChanged();

            // Renders the first 2 layers of the tile map
            mapInfo.Draw(spriteBatch, showLayer[0], showLayer[1], false);

            // NOTE:  Drawing the sprite between the middle layer and top layer.
            // This will allow the top layer to over lap for arches or other tall map
            // structures.
            // Draw the NPCs according to the map
            int startI = 0;
            if (mapInfo.CurrentMap != 0)
                startI = mapNPC[mapInfo.CurrentMap - 1];

            for (int i = startI; i < mapNPC[mapInfo.CurrentMap]; i++)
            {
                mapActors[i].Draw(spriteBatch, Color.White, SpriteBlendMode.AlphaBlend);
            }

            // Draw the main party
            player.Draw(spriteBatch, Color.White, SpriteBlendMode.AlphaBlend);

            // Draw the top layer
            mapInfo.Draw(spriteBatch, false, false, showLayer[2]);

            // TODO:  Load atmosphere layer that moves at a different speed than
            // the other layers

            // Update debug status
            MapDebug.StatusMsg = statusMsg;

            // Print out debug message
            MapDebug.Draw(gameTime, ChronosEngine.ScreenManager.SpriteBatch);
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
            singleton.StatusMsg = "Rotation: " + Camera.Rotation + "  Position = " + Camera.Position.ToString() +
                "  Zoom: " + Camera.Zoom + "\n";
            singleton.StatusMsg += "Char: " + character.Position.ToString() + "\n";
            singleton.StatusMsg += "MapChar: " + singleton.player.MapPosition.ToString() + "\n";

            /**
            for (int i = 0; i < singleton.mapActors.Count; i++)
            {
                singleton.StatusMsg += "NPC #" + (i + 1) + ": X=" + singleton.mapActors[i].MapPosition.X +
                    " Y=" + singleton.mapActors[i].MapPosition.Y + "\n";
            }
             */            
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