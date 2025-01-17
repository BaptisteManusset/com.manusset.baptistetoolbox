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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;

    /// <summary>
    /// RenameOperationSequences are a collection of RenameOperations where the order is the order in which
    /// RenameOperations are applied to a string to get a resultant name.
    /// </summary>
    /// <typeparam name="T">The type of RenameOperation contained in the sequence</typeparam>
    public class RenameOperationSequence<T> : IList<T> where T : IRenameOperation
    {
        private const string VersionTag = "[Version = 1]";

        private static Dictionary<string, System.Type> UnversionedOperationSerializedKeys = new Dictionary<string, System.Type>()
        {
            {"Add/Prefix or Suffix", typeof(AddStringOperation)},
            {"Add/String Sequence", typeof(AddStringSequenceOperation)},
            {"Modify/Change Case", typeof(ChangeCaseOperation)},
            {"Add/Count By Letter", typeof(CountByLetterOperation)},
            {"Add/Enumerate", typeof(EnumerateOperation)},
            {"Delete/Remove Characters", typeof(RemoveCharactersOperation)},
            {"Replace/Rename", typeof(ReplaceNameOperation)},
            {"Replace/Replace String", typeof(ReplaceStringOperation)},
            {"Delete/Trim Characters", typeof(TrimCharactersOperation)},
        };

        private List<T> operationSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenameOperationSequence{T}"/> class.
        /// </summary>
        public RenameOperationSequence()
        {
            operationSequence = new List<T>();
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of items in the sequence.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return operationSequence.Count;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RenameOperation`1"/> at the specified index.
        /// </summary>
        /// <param name="index">Index to access.</param>
        /// <returns>The element at the specified index</returns>
        public T this[int index]
        {
            get
            {
                return operationSequence[index];
            }

            set
            {
                operationSequence[index] = value;
            }
        }

        /// <summary>
        /// Add an item to the end of the sequence.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item)
        {
            operationSequence.Add(item);
        }

        /// <summary>
        /// Gets the index of the specified item.
        /// </summary>
        /// <returns>The index of the specified item.</returns>
        /// <param name="item">Item to query.</param>
        public int IndexOf(T item)
        {
            return operationSequence.IndexOf(item);
        }

        /// <summary>
        /// Insert the specified item at the specified index.
        /// </summary>
        /// <param name="index">Index to insert the item into.</param>
        /// <param name="item">Item to insert.</param>
        public void Insert(int index, T item)
        {
            operationSequence.Insert(index, item);
        }

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            operationSequence.RemoveAt(index);
        }

        /// <summary>
        /// Clear all operations in the sequence.
        /// </summary>
        public void Clear()
        {
            operationSequence.Clear();
        }

        /// <summary>
        /// Returns true if the Sequence contains the specified item.
        /// </summary>
        /// <param name="item">Item to query for.</param>
        /// <returns>true if the item is found in the sequence, false otherwise.</returns>
        public bool Contains(T item)
        {
            return operationSequence.Contains(item);
        }

        /// <summary>
        /// Copies the sequence operations into an array.
        /// </summary>
        /// <param name="array">Array to copy into.</param>
        /// <param name="arrayIndex">Array index.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            operationSequence.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove the specified item.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <returns>true if the object is removed, false otherwise.</returns>
        public bool Remove(T item)
        {
            return operationSequence.Remove(item);
        }

        /// <summary>
        /// Gets the enumerator for the IEnumerable.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return operationSequence.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator for the IEnumerable.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a HashCode for the sequence
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            // I'm never going to hash these so just use base
            return base.GetHashCode();
        }

        /// <summary>
        /// Compares this RenameOperationSequence to another and returns true if they are equal.
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherSequence = obj as RenameOperationSequence<IRenameOperation>;
            if (otherSequence == null)
            {
                return false;
            }

            if (operationSequence.Count != otherSequence.operationSequence.Count)
            {
                return false;
            }

            for (int i = 0; i < operationSequence.Count; ++i)
            {
                if (!operationSequence[i].Equals(otherSequence[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a preview of how the sequence would apply to a string with a given count.
        /// </summary>
        /// <returns>The rename preview.</returns>
        /// <param name="originalName">Original name.</param>
        /// <param name="count">Count, used for enumerating rename operations.</param>
        public RenameResultSequence GetRenamePreview(string originalName, int count)
        {
            var renameResults = GetRenameSequenceForName(originalName, count);
            var resultSequence = new RenameResultSequence(renameResults);

            return resultSequence;
        }

        /// <summary>
        /// Gets the resulting name from the sequence.
        /// </summary>
        /// <returns>The resulting name.</returns>
        /// <param name="originalName">Original name.</param>
        /// <param name="count">Count, used for enumerating rename operations.</param>
        public string GetResultingName(string originalName, int count)
        {
            var resultSequence = GetRenamePreview(originalName, count);
            return resultSequence.NewName;
        }

        /// <summary>
        /// Converts the sequence into a string that can be used to serialize it
        /// </summary>
        /// <returns>The serializable string.</returns>
        public string ToSerializableString()
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append(VersionTag);
            foreach (var op in operationSequence)
            {
                stringBuilder.Append('\n');
                stringBuilder.Append(ConvertOperationToStringEntry(op));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates an operation sequence from its serialized string
        /// </summary>
        /// <returns>The operation sequence.</returns>
        /// <param name="str">Formerly serialized string.</param>
        public static RenameOperationSequence<IRenameOperation> FromString(string str)
        {
            // Versioning - convert old to new
            var isValueCircaPreVersion = string.IsNullOrEmpty(str) || str[0] != '[';
            if (isValueCircaPreVersion)
            {
                return GetOpsFromPreVersionedString(str);
            }

            // Strip the version
            str = str.Substring(VersionTag.Length, str.Length - VersionTag.Length);
            var sequence = new RenameOperationSequence<IRenameOperation>();
            var lines = str.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                sequence.Add(GetOperationFromStringEntry(line));
            }

            return sequence;
        }

        private static RenameOperationSequence<IRenameOperation> GetOpsFromPreVersionedString(string str)
        {
            var ops = str.Split(',');
            var operations = new RenameOperationSequence<IRenameOperation>();
            foreach (var op in ops)
            {
                if (UnversionedOperationSerializedKeys.ContainsKey(op))
                {
                    var operationInstance = (IRenameOperation)System.Activator.CreateInstance(
                        UnversionedOperationSerializedKeys[op]);
                    operations.Add(operationInstance);
                }
            }

            return operations;
        }

        private static string ConvertOperationToStringEntry(IRenameOperation op)
        {
            return string.Format("[{0}]{1}", op.GetType(), JsonUtility.ToJson(op));
        }

        private static IRenameOperation GetOperationFromStringEntry(string entry)
        {
            // Capture the type inside brackets, and capture the rest of the line
            var typeRegex = new Regex("\\[([^\\]]*)\\](.*)\\n*");
            var typeMatch = typeRegex.Match(entry);
            var opTypeStr = typeMatch.Groups[1].Value;
            var opJson = typeMatch.Groups[2].Value;

            System.Type opType = System.Type.GetType(opTypeStr, true);

            return (IRenameOperation)JsonUtility.FromJson(opJson, opType);
        }

        private List<RenameResult> GetRenameSequenceForName(string originalName, int count)
        {
            var renameResults = new List<RenameResult>();
            string modifiedName = originalName;
            RenameResult result;

            if (operationSequence.Count == 0)
            {
                result = new RenameResult();
                result.Add(new Diff(originalName, DiffOperation.Equal));
                renameResults.Add(result);
            }
            else
            {
                foreach (var op in operationSequence)
                {
                    result = op.Rename(modifiedName, count);
                    renameResults.Add(result);
                    modifiedName = result.Result;
                }
            }

            return renameResults;
        }
    }
}