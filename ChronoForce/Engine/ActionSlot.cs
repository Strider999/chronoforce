#region File Description
//-----------------------------------------------------------------------------
// ActionSlot.cs
//
// Copyright (c) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChronoForce.Base;
using ChronoForce.Character;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// An enumeration of commands the action script can perform.  This makes it easier for 
    /// recognizing what the action is doing and for indexing an array of valid commands.
    /// </summary>
    public enum ActionCommand : int
    {
        MoveTo = 0,
        DashTo = 1,
        JumpTo = 2,
        Wait = 3,
        ShowAttack = 4,
        ShowCasting = 5,
        ShowHit = 6,
        ShowText = 7,
        ShowVictory = 8,
        ShowDeath = 9,
        Begin = 10,
        End = 11,
        DisplayEffect = 12,
        TakeDamage = 13
    }

    /// <summary>
    /// Provides necessary information for one battle action.  This
    /// stores the command and necessary sprites used in the action.
    /// </summary>
    class ActionSlot
    {
        #region Constants
        /// <summary>
        /// Default delay between each frame of action for action slots in miiliseconds
        /// </summary>
        public const int cActionDelay = 33;
        #endregion

        #region Fields
        // Character that is moving
        CharacterBase actor;
        // Type of action this slot represents
        ActionCommand actionType;
        // Battle text for rendering text on the screen.  The action will start the text
        BattleText battleText;
        // Ending position for the movement
        Vector2 endPosition;
        // Linear interpolation step between the position and endPosition based on speed
        Vector2 stepSize;
        // Determines how fast to animate the action.  Note that this is in frames of animation.
        // If speed is 0, then the movement will be instant.
        int speed = 0;
        // Number for damage
        int damage = 0;
        // Wait time for commands that pause the action
        int waitTime;
        // Counter for the amount of anation frames that passed
        int actionFrame = 0;
        // Timer for the action
        int actionTimer;
        // True if this action is done and won't be updated
        bool isFinished = false;
        // True if the step size has been initialized
        bool stepInitialized = false;
        #endregion

        #region Events

        /// <summary>
        /// Event to signal when this action has finished, including animation and batte numbers.
        /// </summary>
        public EventHandler<EventArgs> Complete;

        /// <summary>
        /// Internal function used to signal complete
        /// </summary>
        protected internal void OnComplete()
        {
            isFinished = true;
            if (Complete != null)
                Complete(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current position of the sprite if it is moving
        /// </summary>
        public Vector2 Position
        {
            get { return actor.Position; }
        }

        /// <summary>
        /// Returns the action type this slot represents.
        /// </summary>
        public ActionCommand Action
        {
            get { return actionType; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor used mainly as a place holder for Begin and End commands.
        /// </summary>
        /// <param name="action">Begin or End</param>
        public ActionSlot(ActionCommand action)
        {
            actionType = action;
        }

        /// <summary>
        /// Contructor for the Wait command with a provided wait time
        /// </summary>
        /// <param name="action">Should be Wait</param>
        /// <param name="waitTime">How long to wait in animation frames</param>
        public ActionSlot(ActionCommand action, int waitTime)
        {
            actionType = action;
            this.waitTime = waitTime;
        }

        /// <summary>
        /// Basic constructor that initializes the actor and action command.
        /// </summary>
        /// <param name="actor">Player in this action</param>
        /// <param name="action">Action type for changing actor animation</param>
        public ActionSlot(ActionCommand action, CharacterBase actor)
        {
            this.actor = actor;
            actionType = action;
        }

        /// <summary>
        /// Constructor for displaying text on the screen
        /// </summary>
        /// <param name="action">Should be ShowText</param>
        /// <param name="battleText">Battle text used for this text render</param>
        public ActionSlot(ActionCommand action, BattleText battleText)
        {
            actionType = action;
            this.battleText = battleText;
        }

        /// <summary>
        /// Action constructor for movement commands.
        /// </summary>
        /// <param name="action">Action type for moving the actor through battle</param>
        /// <param name="actor">Player in this action</param>
        /// <param name="endPosition">Ending position for a moving sprite</param>
        /// <param name="speed">Animation speed for the action.  0 means the action is instant</param>
        public ActionSlot(ActionCommand action, CharacterBase actor, Vector2 endPosition, int speed)
            : this(action, actor)
        {
            this.endPosition = endPosition;
            this.speed = speed;
        }

        /// <summary>
        /// Constructor for the actor taking damage.
        /// </summary>
        /// <param name="action">Should be TakeDamage</param>
        /// <param name="actor">Player in this action</param>
        /// <param name="damage">Damage amount</param>     
        public ActionSlot(ActionCommand action, CharacterBase actor, int damage)
            : this(action, actor)
        {
            this.damage = damage;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles when the battle text is finished and just signals the action as finished
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void BattleTextFinished(object o, EventArgs e)
        {
            OnComplete();
        }

        #endregion

        #region Updates

        /// <summary>
        /// Updates the time elapsed since last update and updates any positions from the related
        /// actions.
        /// </summary>
        /// <param name="elapsed"></param>
        public void Update(int elapsed)
        {
            if (!isFinished)
            {
                actionTimer += elapsed;

                // Check to see if the action is to display text.  If so, start the battle text
                if (actionType == ActionCommand.ShowText && !battleText.IsRendering)
                {
                    battleText.Start();
                    // Link the event to a handler so we know when the action is done
                    battleText.Finished += BattleTextFinished;
                }

                // Check commands for those that are independent of the actionTimer, like simple
                // animation changes.
                switch (actionType)
                {
                    // TODO:  Change character animations
                    case ActionCommand.ShowAttack:
                    case ActionCommand.ShowCasting:
                    case ActionCommand.ShowDeath:
                    case ActionCommand.ShowVictory:
                    case ActionCommand.ShowHit:
                        OnComplete();
                        break;
                    case ActionCommand.TakeDamage:
                        // Subtracts health from the actor
                        actor.HP -= damage;
                        OnComplete();
                        break;
                };

                if (actionTimer >= cActionDelay)
                {
                    // Increase the action frame
                    actionFrame++;

                    // Check the commands
                    switch (actionType)
                    {
                        case ActionCommand.MoveTo:
                        case ActionCommand.DashTo:
                        case ActionCommand.JumpTo:
                            // Linearly interpolate between the current position and 
                            // the end position
                            // Will only calculate this once
                            if (!stepInitialized)
                            {
                                stepSize = (endPosition - actor.Position) / (float)speed;
                                stepInitialized = true;
                            }

                            actor.Position += stepSize;

                            // Note I check both position and speed in case there's rounding
                            // error that causes the position not the match the end
                            // position perfectly.  Note that if speed is 0, this will
                            // instantly move the sprite to the end position.
                            if (actor.Position == endPosition ||
                                actionFrame > speed)
                            {
                                actor.Position = endPosition;
                                OnComplete();
                            }
                            break;
                        case ActionCommand.Wait:
                            // Check to see if enough frames have passed
                            if (actionFrame > waitTime)
                                OnComplete();
                            break;
                    };
                }
            }
        }

        #endregion
    }
}