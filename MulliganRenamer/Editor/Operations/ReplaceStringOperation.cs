﻿/* MIT License

Copyright (c) 2016 Edward Rowe, RedBlueGames

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace RedBlueGames.MulliganRenamer
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// RenameOperation used to replace substrings from the rename string.
    /// </summary>
    public class ReplaceStringOperation : IRenameOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceStringOperation"/> class.
        /// </summary>
        public ReplaceStringOperation()
        {
            UseRegex = false;
            SearchString = string.Empty;
            SearchIsCaseSensitive = false;
            ReplacementString = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceStringOperation"/> class.
        /// This is a clone constructor, copying the values from one to another.
        /// </summary>
        /// <param name="operationToCopy">Operation to copy.</param>
        public ReplaceStringOperation(ReplaceStringOperation operationToCopy)
        {
            CopyFrom(operationToCopy);
        }

        [SerializeField]
        private bool userRegex;

        [SerializeField]
        private string searchString;

        [SerializeField]
        private bool searchIsCaseSensitive;

        [SerializeField]
        private string replacementString;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ReplaceStringOperation"/>
        /// uses a regex expression for input.
        /// </summary>
        /// <value><c>true</c> if input is a regular expression; otherwise, <c>false</c>.</value>
        public bool UseRegex
        {
            get
            {
                return userRegex;
            }

            set
            {
                userRegex = value;
            }
        }

        /// <summary>
        /// Gets or sets the search string that will be replaced.
        /// </summary>
        /// <value>The search string.</value>
        public string SearchString
        {
            get
            {
                return searchString;
            }

            set
            {
                searchString = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the search is case sensitive.
        /// </summary>
        /// <value><c>true</c> if search is case sensitive; otherwise, <c>false</c>.</value>
        public bool SearchIsCaseSensitive
        {
            get
            {
                return searchIsCaseSensitive;
            }

            set
            {
                searchIsCaseSensitive = value;
            }
        }

        /// <summary>
        /// Gets or sets the replacement string, which replaces instances of the search token.
        /// </summary>
        /// <value>The replacement string.</value>
        public string ReplacementString
        {
            get
            {
                return replacementString;
            }

            set
            {
                replacementString = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has errors that prevent it from Renaming.
        /// </summary>
        /// <value><c>true</c> if this instance has errors; otherwise, <c>false</c>.</value>
        public bool HasErrors()
        {
            if (UseRegex)
            {
                return !SearchStringIsValidRegex || !ReplacementStringIsValidRegex;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the search pattern as Regex
        /// </summary>
        /// <value>The search regex pattern.</value>
        public string SearchStringAsRegex
        {
            get
            {
                if (UseRegex)
                {
                    return SearchString;
                }
                else
                {
                    string searchStringRegexPattern = string.Empty;

                    if (!string.IsNullOrEmpty(SearchString))
                    {
                        // Escape the non-regex search string to prevent any embedded patterns from being interpretted as regex.
                        searchStringRegexPattern = string.Concat(Regex.Escape(SearchString));
                    }

                    return searchStringRegexPattern;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the search string specified is valid for regex parsing
        /// </summary>
        public bool SearchStringIsValidRegex
        {
            get
            {
                return IsValidRegex(SearchString);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the replacement string specified is valid for regex parsing
        /// </summary>
        public bool ReplacementStringIsValidRegex
        {
            get
            {
                return IsValidRegex(ReplacementString);
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public IRenameOperation Clone()
        {
            var clone = new ReplaceStringOperation(this);
            return clone;
        }

        /// <summary>
        /// Copies the state from one operation into this one.
        /// </summary>
        /// <param name="other">Other.</param>
        public void CopyFrom(ReplaceStringOperation other)
        {
            UseRegex = other.UseRegex;
            SearchString = other.SearchString;
            SearchIsCaseSensitive = other.SearchIsCaseSensitive;
            ReplacementString = other.ReplacementString;
        }

        /// <summary>
        /// Rename the specified input, using the relativeCount.
        /// </summary>
        /// <param name="input">Input String to rename.</param>
        /// <param name="relativeCount">Relative count. This can be used for enumeration.</param>
        /// <returns>A new string renamed according to the rename operation's rules.</returns>
        public RenameResult Rename(string input, int relativeCount)
        {
            if (string.IsNullOrEmpty(input))
            {
                return new RenameResult();
            }

            RenameResult renameResult;
            if (string.IsNullOrEmpty(SearchString))
            {
                renameResult = new RenameResult();
                renameResult.Add(new Diff(input, DiffOperation.Equal));
                return renameResult;
            }

            MatchCollection matches;
            try
            {
                // Regex gives us case sensitivity, even when not searching with regex.
                var regexOptions = SearchIsCaseSensitive ? default(RegexOptions) : RegexOptions.IgnoreCase;
                matches = Regex.Matches(input, SearchStringAsRegex, regexOptions);
            }
            catch (System.ArgumentException)
            {
                renameResult = new RenameResult();
                renameResult.Add(new Diff(input, DiffOperation.Equal));
                return renameResult;
            }

            renameResult = CreateDiffFromMatches(input, ReplacementString, matches);
            return renameResult;
        }

        /// <summary>
        /// Gets the hash code for the operation
        /// </summary>
        /// <returns>A unique hash code from the values</returns>
        public override int GetHashCode()
        {
            // Easy hash method:
            // https://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            int hash = 17;
            hash = hash * 23 + ReplacementString.GetHashCode();
            hash = hash * 23 + SearchString.GetHashCode();
            hash = hash * 23 + searchIsCaseSensitive.GetHashCode();
            hash = hash * 23 + UseRegex.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns whether or not this rename operation is equal to another and returns the result.
        /// </summary>
        /// <returns>True if the operations are equal.true False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var otherAsOp = obj as ReplaceStringOperation;
            if (otherAsOp == null)
            {
                return false;
            }

            if (UseRegex != otherAsOp.UseRegex)
            {
                return false;
            }

            if (SearchIsCaseSensitive != otherAsOp.SearchIsCaseSensitive)
            {
                return false;
            }

            if (SearchString != otherAsOp.SearchString)
            {
                return false;
            }

            if (ReplacementString != otherAsOp.ReplacementString)
            {
                return false;
            }

            return true;
        }

        private static float GetHeightForHelpBox()
        {
            return 34.0f;
        }

        private static bool IsValidRegex(string pattern)
        {
            // We consider empty a valid regular expression since Rename handles it gracefully
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            try
            {
                Regex.Match(string.Empty, pattern);
            }
            catch (System.ArgumentException)
            {
                return false;
            }

            return true;
        }

        private RenameResult CreateDiffFromMatches(string originalName, string replacementRegex, MatchCollection matches)
        {
            var renameResult = new RenameResult();
            var nextMatchStartingIndex = 0;
            foreach (Match match in matches)
            {
                // Grab the substring before the match
                if (nextMatchStartingIndex < match.Index)
                {
                    string before = originalName.Substring(nextMatchStartingIndex, match.Index - nextMatchStartingIndex);
                    renameResult.Add(new Diff(before, DiffOperation.Equal));
                }

                // Add the match as a deletion
                renameResult.Add(new Diff(match.Value, DiffOperation.Deletion));

                // Add the result as an insertion
                var result = match.Result(replacementRegex);
                if (!string.IsNullOrEmpty(result))
                {
                    renameResult.Add(new Diff(result, DiffOperation.Insertion));
                }

                nextMatchStartingIndex = match.Index + match.Length;
            }

            if (nextMatchStartingIndex < originalName.Length)
            {
                var lastSubstring = originalName.Substring(nextMatchStartingIndex, originalName.Length - nextMatchStartingIndex);
                renameResult.Add(new Diff(lastSubstring, DiffOperation.Equal));
            }

            return renameResult;
        }
    }
}