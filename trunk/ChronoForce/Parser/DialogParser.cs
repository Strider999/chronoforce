#region File Description
//-----------------------------------------------------------------------------
// DialogParser.cs
//
// Copyright (C) David Hsu
// Automatically cuts down text into dialog sized text for rendering.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChronoForce.Parser
{
    /// <summary>
    /// Cuts down paragraph text into separate strings for rendering in a dialog box.
    /// </summary>
    public static class DialogParser
    {
        #region Fields

        // List of strings to return
        static List<string> dialogText = new List<string>();
        // Array of strings from splitting a dialog text
        static string[] splitString;
        // Vectors for holding string sizes based on the font
        static Vector2 splitStringSize;
        static Vector2 smallStringSize;
        // Complete string to be added to the list
        static string completeString;
        // Partial string of added words
        static string smallString;

        #endregion

        /// <summary>
        /// Parses a string into dialog-sized bits based on the font used, maximum width
        /// that the text can fit, and the number of rows the dialog box can hold.
        /// </summary>
        /// <param name="text">String to be split</param>
        /// <param name="font">Font used to render the text</param>
        /// <param name="maxWidth">Maximum width the string can extended horizontally</param>
        /// <param name="maxRows">Maximum number of rows the text can occupy in the dialog box</param>
        /// <returns>A list of strings that fit in the specified dialog box size</returns>
        public static List<string> ParseString(string text, SpriteFont font, int maxWidth, int maxRows)
        {
            // Bound Checks:  If maxWidth or maxRows are 0 or less, then it's not possible to split the string
            if (maxWidth <= 0 || maxRows <= 0)
                return null;

            // If the string is null, also exit
            if (text == null)
                return null;

            // Split the string based on white space
            splitString = text.Split();
      
            // Complete string to be added to the list
            completeString = "";
            // A partial string of added words
            smallString = "";
            // Clear the dialog text
            dialogText.Clear();

            int counter = 1;

            // Loop through the string and start added words
            for (int i = 0; i < splitString.Length; i++)
            {
                // Measure the length of the strings
                smallStringSize = font.MeasureString(smallString);
                splitStringSize = font.MeasureString(splitString[i]);

                // Check to see if we exceeded the maxWidth pixel limit if we add a word
                if (splitStringSize.X + smallStringSize.X > maxWidth)
                {
                    // If we reached the max rows, time to add the string to the list and
                    // start a new one.
                    if (counter == maxRows)
                    {
                        // Add the incomplete string
                        completeString += smallString;

                        // Trim the complete string of any white space in the beginning and end
                        completeString = completeString.Trim();

                        // Add the string to the list
                        dialogText.Add(completeString);

                        //Reset the split string to the current holder and reset the counter
                        completeString = "";
                        smallString = "";
                        counter = 1;
                    }
                    else
                    {
                        // Add the string with a new line and clear the small string
                        completeString += smallString + "\n";
                        smallString = "";
                        counter++;
                    }
                }

                // Add the word to the string
                smallString += splitString[i] + " ";
            }

            // Add the remaining string to the list
            completeString += smallString;
            dialogText.Add(completeString);

            return dialogText;
        }

        /// <summary>
        /// Parses a string into dialog-sized bits based on the maximum number of characters in a line
        /// of text and the maximum number of rows the dialog box can hold.  This is useful for fixed
        /// sized fonts.
        /// </summary>
        /// <param name="text">String to be split</param>
        /// <param name="maxCharacters">Maximum number of characters in a line</param>
        /// <param name="maxRows">Maximum number of rows the text can occupy in the dialog box</param>
        /// <returns>A list of strings that fit in the specified dialog box size</returns>
        public static List<string> ParseString(string text, int maxCharacters, int maxRows)
        {
            // Bound Checks:  If maxCharacters or maxRows are 0 or less, then it's not possible
            if (maxCharacters <= 0 || maxRows <= 0)
                return null;
            
            // If the string is null, also exit
            if (text == null)
                return null;

            // Split the string based on white space
            splitString = text.Split();

            // Complete string to be added to the list
            completeString = "";
            // A partial string of added words
            smallString = "";
            // Clear the dialog text
            dialogText.Clear();
            int counter = 1;

            // Loop through the string and start added words
            for (int i = 0; i < splitString.Length; i++)
            {
                // Check to see if we exceeded the maxCharacter limit if we add a word
                if (splitString[i].Length + smallString.Length > maxCharacters)
                {
                    // If we reached the max rows, time to add the string to the list and
                    // start a new one.
                    if (counter == maxRows)
                    {
                        // Add the incomplete string
                        completeString += smallString;

                        // Trim the complete string of any white space in the beginning and end
                        completeString = completeString.Trim();

                        // Add the string to the list
                        dialogText.Add(completeString);

                        //Reset the split string to the current holder and reset the counter
                        completeString = "";
                        smallString = "";
                        counter = 1;
                    }
                    else
                    {
                        // Add the string with a new line and clear the small string
                        completeString += smallString + "\n";
                        smallString = "";
                        counter++;
                    }
                }
                
                // Add the word to the string
                smallString += splitString[i] + " ";
            }

            // Add the remaining string to the list
            completeString += smallString;
            dialogText.Add(completeString);

            return dialogText;
        }
    }
}
