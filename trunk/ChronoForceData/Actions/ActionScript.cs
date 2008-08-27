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
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using ChronoForceData.Character;
#endregion

namespace ChronoForceData.Actions
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
        /// <summary>
        /// Maximum number of concurrent actors that can be referenced by a script
        /// </summary>
        const int cMaxActors = 10;
        #endregion

        #region Fields
        // A list of actions this script has to execute
        Queue<ActionSlot> actionQueue;
        // List of current action slots being executed
        List<ActionSlot> currentAction = new List<ActionSlot>(cMaxActions);
        // List of actors the script can handle
        List<CharacterBase> actors = new List<CharacterBase>(cMaxActors);
        // Number of actions currently being rendered
        int numActions = 0;
        // A tracking variable for keeping tabs on the number of actions completed
        int actionsCompleted = 0;
        // Name of this script
        string name;

        // Local copy of the array of strings representing action slots
        List<string> commands = new List<string>();
        // Local string array used to hold temporary data for splitting strings
        string[] splitString;
        #endregion

        #region Events

        /// <summary>
        /// Signals when it finished an action.
        /// </summary>
        [ContentSerializerIgnore]
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

        #region Properties

        /// <summary>
        /// The name of the script
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Local copy of the array of strings to process into action slots
        /// </summary>
        public List<string> Commands
        {
            get { return commands; }
            set { commands = value; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Default contructor called by the ContentType reader
        /// </summary>
        internal ActionScript()
        {
            // Does nothing
        }

        /// <summary>
        /// Called by the reader to parse the file.  Should only be called by
        /// ActionScriptReader.
        /// </summary>
        internal ActionScript(ContentReader input)
        {
            // First, read the name
            name = input.ReadString();

            // Grab the array of actions
            commands = input.ReadObject<List<string>>();

            // Parse the string for the commands
            // DEBUG:  Display the strings
            Console.WriteLine("Name: {0}", name);
            for (int i = 0; i < commands.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i, commands[i]);
            }
        }

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

        #endregion

        #region Updating

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

        /// <summary>
        /// Helper function for parsing action slot commands in string form and
        /// adds the ActionSlot to the queue
        /// </summary>
        /// <param name="command">ActionSlot command in string form</param>
        void ParseString(string command)
        {
            // Split the string based on white space
            splitString = command.Split();

            // Check to make sure there's something before attempting to index
            // the array
            if (splitString == null)
            {
                Console.WriteLine("[ActionScript ERROR]: SplitString is null!");
                return;
            }

            // The first string will be the command
            switch (splitString[0])
            {
                case "Begin":
                case "End":
                    break;
            }
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads an action script from the pipeline
        /// </summary>
        public class ActionScriptReader : ContentTypeReader<ActionScript>
        {
            /// <summary>
            /// Reads an ActionScript object from the content pipeline.
            /// </summary>
            protected override ActionScript Read(ContentReader input,
                ActionScript existingInstance)
            {
                return new ActionScript(input);
            }
        }

        #endregion
    }
}
