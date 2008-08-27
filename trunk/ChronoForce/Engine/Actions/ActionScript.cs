#region File Description
//-----------------------------------------------------------------------------
// ActionScript.cs
//
// Copyright (c) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using ChronoForce.Character;
using ChronoForceData.Actions;
#endregion

namespace ChronoForce.Engine.Actions
{
    /// <summary>
    /// Holds a set of actions for rendering the sprites when performing attacks, skills,
    /// and other battle actions.  It acts as a queue for pacing how to render everything 
    /// for the battle.
    /// </summary>
    public class ActionScript
    {
        #region Constants
        /// <summary>
        /// Maximum number of concurrent actions the action script can render at once
        /// </summary>
        const int cMaxActions = 10;
        #endregion

        #region Fields
        // A list of actions this script has to execute
        Queue<ActionSlot> actionQueue;
        // List of current action slots being executed
        List<ActionSlot> currentAction = new List<ActionSlot>(cMaxActions);
        // Number of actions currently being rendered
        int numActions = 0;
        // A tracking variable for keeping tabs on the number of actions completed
        int actionsCompleted = 0;
        #endregion

        #region Events

        /// <summary>
        /// Signals when it finished an action.
        /// </summary>
        public EventHandler<EventArgs> ScriptDone;

        /// <summary>
        /// Internal function for signalling the event
        /// </summary>
        protected internal void OnActionDone()
        {
            if (ScriptDone != null)
                ScriptDone(this, EventArgs.Empty);
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates an action script with the specified queue of actions.
        /// </summary>
        /// <param name="actions">ActionSlots to run through</param>
        public ActionScript(Queue<ActionSlot> actions)
        {
            actionQueue = actions;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the action script.
        /// </summary>
        public void Start()
        {
            NextAction();
        }

        /// <summary>
        /// Updates the action script with the elapsed time since last update.
        /// </summary>
        /// <param name="elapsed">Time since the last update in milliseconds</param>
        public void Update(int elapsed)
        {
            // Update the current actions
            for (int i = 0; i < currentAction.Count; i++)
                currentAction[i].Update(elapsed);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Advances the script to the next action
        /// </summary>
        void NextAction()
        {
            if (actionQueue.Count > 0)
            {
                // First, peek at the top item and see if it's a Begin action.  If
                // it is, start adding actions into the list for execution.
                if (actionQueue.Peek().Action == ActionCommand.Begin)
                {
                    // Remove the Begin action from the queue
                    actionQueue.Dequeue();

                    // Loop through the queue until the corresponding End is seen
                    do
                    {
                        AddAction();
                    } while (actionQueue.Peek().Action != ActionCommand.End);

                    // Remove the End action from the Queue
                    actionQueue.Dequeue();
                }
                else
                {
                    AddAction();
                }
            }
            else
            {
                // There's no more action slots to run through, so this script
                // is complete
                OnActionDone();
            }
        }

        /// <summary>
        /// Private function used for removing an action slot from the queue and 
        /// adding it to the list of actions to be executed.
        /// </summary>
        void AddAction()
        {
            ActionSlot slot = actionQueue.Dequeue();

            // Connect the event so we know when the action is done
            slot.Complete += ActionCompleteHandler;
            numActions++;

            currentAction.Add(slot);
        }

        /// <summary>
        /// Handles the signal when an action slot reports it completed
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        void ActionCompleteHandler(object o, EventArgs e)
        {
            actionsCompleted++;

            // If the total number of actions completed is the same as the
            // number of actions in the list, then clear the list and add the next
            // action in the queue.
            if (actionsCompleted == numActions)
            {
                currentAction.Clear();
                actionsCompleted = 0;
                numActions = 0;
                NextAction();
            }
        }

        #endregion
    }
}
