#region File Description
//-----------------------------------------------------------------------------
// BattleDirector.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statments
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
using ChronoForceData.Base;
using ChronoForceData.Character;
using ChronoForceData.Actions;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Directs the animations in battle and renders them accordingly.  This draws all the 
    /// sprites on the battle field, including special effects.
    /// </summary>
    class BattleDirector : Director
    {
        #region Structs

        /// <summary>
        /// A structure to hold the bit IDs of the character(s) performing an action.
        /// </summary>
        struct ActorResetInfo
        {
            public int bitID;
            public bool isEnemy;
        }

        #endregion

        #region Constants
        // Default vectors for player positions
        readonly float[,] cPlayerPositions = new float[3, 2] { { 150, 400 }, { 300, 400 }, { 450, 400 } };
        // Default vectors for enemy positions
        readonly float[,] cEnemyPositions = new float[3, 2] { { 150, 100 }, { 300, 100 }, { 450, 100 } };
        #endregion

        #region Fields
        // Queue of characters for reseting timers after the action finishes.  Note that
        // it stores a bit number for the characters.  In this way, we can easily store
        // all the characters in a single digit.
        Queue<ActorResetInfo> actorResetQueue = new Queue<ActorResetInfo>();

        // List of battle text that the director can render simultaneously
        List<BattleText> battleTextList;
        // Texture for the arrow
        Texture2D arrow;

        // True if the person is selecting a target
        bool selectingTarget = false;
        // Keeps track of what's selected from the menu in order to render the arrows correctly
        int currentTarget;

        #endregion

        #region Events

        #endregion

        #region Properties
        /// <summary>
        /// Boolean to see if the player is currently selecting
        /// a target.
        /// </summary>
        public bool SelectingTarget
        {
            get { return selectingTarget; }
            set { selectingTarget = value; }
        }

        /// <summary>
        /// The current target the player is selecting.
        /// </summary>
        public int CurrentTarget
        {
            get { return currentTarget; }
            set { currentTarget = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor to load the director and initialize all values.
        /// </summary>
        /// <param name="engine">Battle engine for up-to-date information about the battle</param>
        /// <param name="arrowTexture">Texture for the arrow</param>
        public BattleDirector(Texture2D arrowTexture)
        {
            arrow = arrowTexture;

            // Initialize the position vectors
            for (int i = 0; i < BattleEngine.PartyCount; i++)
            {
                BattleEngine.PlayerPartyList[i].Position = new Vector2(cPlayerPositions[i, 0], cPlayerPositions[i, 1]);
            }

            for (int i = 0; i < BattleEngine.EnemyCount; i++)
            {
                BattleEngine.EnemyPartyList[i].Position = new Vector2(cEnemyPositions[i, 0], cEnemyPositions[i, 1]);
            }

            // Load the list of battle texts
            battleTextList = new List<BattleText>(ChronoConstants.cMaxPartyMembers);
            for (int i = 0; i < ChronoConstants.cMaxPartyMembers; i++)
            {
                BattleText tmpBattleText = new BattleText();
                battleTextList.Add(tmpBattleText);
            }

            // Initialize handling what to do when an action is done
            ActionDone += ActionDoneHandler;
        }

        #endregion

        #region Public Methods

        public void CreateAttack(CombatCharacter attacker, CombatCharacter defender)
        {
            // DEBUG:  Create a basic attack script
            // TODO:  Make a less hard coded method
            // TODO:  Center the battle text on the enemy
            Console.WriteLine("[BattleDirector CreateAttack]: 1 << attackID({0}) is {1}",
                attacker.ID, 1 << attacker.ID);

            ActorResetInfo actorInfo = new ActorResetInfo();
            actorInfo.bitID = 1 << attacker.ID;
            actorInfo.isEnemy = false;
            actorResetQueue.Enqueue(actorInfo);

            int damage = attacker.ATK;
            Vector2 defenderPos = defender.Position;

            battleTextList[0].Position = defenderPos;
            battleTextList[0].Text = damage.ToString();
            AddAction(actionLibrary.createActionScript("Attack",
                                                       attacker,
                                                       defender,
                                                       damage,
                                                       battleTextList[0]));
        }

        /// <summary>
        /// Adds a new action script to the queue.
        /// </summary>
        /// <param name="newAction"></param>
        public void AddAction(ActionScript newAction)
        {
            actions.Enqueue(newAction);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles when an action has finished and reset the proper character timer
        /// </summary>
        void ActionDoneHandler(object o, EventArgs e)
        {
            // Reset the timer for the character(s) who executed the action
            ActorResetInfo actorInfo = actorResetQueue.Dequeue();

            for (int i = 0; i < 3; i++)
            {
                if (((actorInfo.bitID >> i) & 1) == 1)
                    BattleEngine.ResetTime(i, false);
            }
        }

        #endregion

        #region Drawing and Updates

        public override void Update(int elapsed)
        {
            // Update battle text information
            for (int i = 0; i < ChronoConstants.cMaxPartyMembers; i++)
                battleTextList[i].Update(elapsed);

            // Update the actions in the base Director class
            base.Update(elapsed);
        }

        /// <summary>
        /// Draws the sprites on the screen.
        /// </summary>
        /// <param name="batch">Sprite batch to use for rendering</param>
        /// <param name="blendMode"></param>
        public void Draw(SpriteBatch batch, SpriteBlendMode blendMode)
        {
            // Draw the enemy sprites on the screen
            for (int i = 0; i < BattleEngine.EnemyCount; i++)
            {
                // TODO:  Draw actual enemy sprites and modify default positions
                // If the enemy is dead, don't draw it or draw the death sprite for enemy
                if (BattleEngine.IsEnemyAlive(i))
                {
                    //spriteBatch.Draw(blueBox, new Vector2(enemyPositions[i, 0], enemyPositions[i, 1]), Color.White);
                    BattleEngine.EnemyPartyList[i].Draw(batch, Color.White, SpriteBlendMode.None);
                }
            }

            // Draw the player sprites on the screen
            for (int i = 0; i < BattleEngine.PartyCount; i++)
            {
                BattleEngine.PlayerPartyList[i].Draw(batch, Color.White, SpriteBlendMode.None);
            }

            // Draw the selection arrow if the player is currently selecting a target
            if (selectingTarget)
            {
                batch.Begin();

                // DEBUG:  For now, draw the dummy arrow by the enemy
                batch.Draw(arrow,
                     new Vector2(BattleEngine.EnemyPartyList[currentTarget].Position.X - 10, 
                                 BattleEngine.EnemyPartyList[currentTarget].Position.Y + 20),
                     Color.White);

                batch.End();
            }

            // Draw battle text
            for (int i = 0; i < ChronoConstants.cMaxPartyMembers; i++)
                battleTextList[i].Draw(batch, Fonts.BattleFont);
        }

        #endregion
    }
}
