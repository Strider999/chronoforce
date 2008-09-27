#region File Description
//-----------------------------------------------------------------------------
// DialogData.cs
//
// Copyright (C) David Hsu
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
#endregion

namespace ChronoForceData.Character
{
    /// <summary>
    /// Basic class to hold dialog data between characters
    /// </summary>
    public class DialogData
    {
        #region Properties

        string speaker;

        /// <summary>
        /// Person who is speaking the dialog
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string Speaker
        {
            get { return speaker; }
            set { speaker = value; }
        }

        string dialog;

        /// <summary>
        /// Actual dialog of what the NPC or player speaks
        /// </summary>
        public string Dialog
        {
            get { return dialog; }
            set { dialog = value; }
        }

        #endregion

        #region Contructors

        /// <summary>
        /// Default contructor for XML loading
        /// </summary>
        public DialogData()
        {
            // Does nothing
        }

        /// <summary>
        /// Constructor that loads the speaker and dialog into the class
        /// </summary>
        /// <param name="speaker">Person who is speaking the text</param>
        /// <param name="dialog">What the person is saying</param>
        public DialogData(string speaker, string dialog)
        {
            this.speaker = speaker;
            this.dialog = dialog;
        }

        #endregion

        #region Content Type Reader

        /// <summary>
        /// Reads a character dialog from the pipeline
        /// </summary>
        public class DialogDataReader : ContentTypeReader<DialogData>
        {
            /// <summary>
            /// Reads a DialogData object from the content pipeline.
            /// </summary>
            protected override DialogData Read(ContentReader input,
                DialogData existingInstance)
            {
                DialogData dialogData = existingInstance;
                if (existingInstance == null)
                {
                    dialogData = new DialogData();
                }

                dialogData.Speaker = input.ReadString();
                dialogData.Dialog = input.ReadString();

                return dialogData;
            }
        }

        #endregion
    }
}
