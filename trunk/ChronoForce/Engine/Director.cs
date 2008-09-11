#region File Description
//-----------------------------------------------------------------------------
// Director.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using ChronoForceData.Actions;
#endregion

namespace ChronoForce.Engine
{
    /// <summary>
    /// Base class that holds common members and functions for directing animation in the
    /// game.  Contains basic information and action handling.
    /// </summary>
    public class Director
    {
        #region Fields

        // A queue of action scripts to run through
        protected Queue<ActionScript> actions = new Queue<ActionScript>();
        // Current actionScript that's running
        protected ActionScript currentScript;
        // Create the action library
        protected ActionLibrary actionLibrary = new ActionLibrary();

        // True if there is an action being rendered
        bool hasAction = false;
        // Timer for the battle director
        int directorTimer = 0;
        // True if currently in cinematic mode
        bool inCinematic = false;

        #endregion

        #region Constants
        
        /// <summary>
        ///  Amount of delay between scripts in milliseconds
        /// </summary>
        const int cDirectorDelay = 100;

        #endregion

        #region Events

        /// <summary>
        /// Signals when it finished an action.
        /// </summary>
        public EventHandler<EventArgs> ActionDone;

        /// <summary>
        /// Internal function for signalling the event
        /// </summary>
        protected internal void OnActionDone()
        {
            if (ActionDone != null)
                ActionDone(this, EventArgs.Empty);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns whether or not the director is rendering an action
        /// </summary>
        public bool HasAction
        {
            get { return hasAction; }
        }

        /// <summary>
        /// Returns whether or not the director is in cinematic mode
        /// </summary>
        public bool InCinematic
        {
            get { return inCinematic; }
            set { inCinematic = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new action script to the queue
        /// </summary>
        /// <param name="script">Script to run</param>
        public void AddActionScript(ActionScript script)
        {
            actions.Enqueue(script);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the event when the script is done
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void ScriptDoneHandler(object o, EventArgs e)
        {
            // Reset the current script and show that there's no script currently running
            currentScript = null;
            hasAction = false;

            // Signal that an action is done
            OnActionDone();
        }

        #endregion

        #region Updating

        /// <summary>
        /// Base function for updating action scripts.
        /// </summary>
        /// <param name="elapsed">Time since last update</param>
        public virtual void Update(int elapsed)
        {
            // If there's currently an action script executing, update it
            if (hasAction)
            {
                if (currentScript != null)
                    currentScript.Update(elapsed);
            }
            else
            {
                // If there's an action in the queue, perform updates
                if (actions.Count > 0)
                {
                    directorTimer += elapsed;

                    // Wait for a second to allow the battle to update so the timers
                    // can increase.  This prevents a slew of actions from completely
                    // blocking.
                    if (directorTimer >= cDirectorDelay)
                    {
                        directorTimer = 0;

                        // Get the next script and link the event so we'll know when it's done
                        Console.WriteLine("[Director Update]:  Getting next action");
                        currentScript = actions.Dequeue();
                        currentScript.Start();
                        currentScript.ScriptDone += ScriptDoneHandler;

                        hasAction = true;
                    }
                }
            }
        }

        #endregion

    }
}
