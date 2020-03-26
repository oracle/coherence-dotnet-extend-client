/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which compares the result of a member
    /// invocation with a value for pattern match.
    /// </summary>
    /// <remarks>
    /// A pattern can include regular characters and wildcard
    /// characters '_' and '%'.
    /// <p/>
    /// During pattern matching, regular characters must exactly match the
    /// characters in an evaluated string. Wildcard character '_'
    /// (underscore) can be matched with any single character, and wildcard
    /// character '%' can be matched with any string fragment of zero or more
    /// characters.
    /// </remarks>
    /// <author>Cameron Purdy/Gene Gleyzer  2002.10.27</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    /// <author>Tom Beerbower  2009.03.09</author>
    public class LikeFilter : ComparisonFilter, IIndexAwareFilter
    {
        #region Properties

        /// <summary>
        /// Obtain the filter's pattern string.
        /// </summary>
        /// <value>
        /// The pattern string.
        /// </value>
        public virtual string Pattern
        {
            get { return (string) Value; }
        }

        /// <summary>
        /// Check whether or not the filter is case incensitive.
        /// </summary>
        /// <value>
        /// <b>true</b> if case insensitivity is specifically enabled.
        /// </value>
        [Obsolete("As of Coherence 3.4 this property is replaced with IgnoreCase")]
        public virtual bool IsIgnoreCase
        {
            get { return IgnoreCase; }
        }

        /// <summary>
        /// Check whether or not the filter is case incensitive.
        /// </summary>
        /// <value>
        /// <b>true</b> if case insensitivity is specifically enabled.
        /// </value>
        public virtual bool IgnoreCase
        {
            get { return m_ignoreCase; }
        }

        /// <summary>
        /// Obtain the escape character that is used for escaping '%' and
        /// '_' in the pattern or zero if there is no escape.
        /// </summary>
        /// <value>
        /// The escape character.
        /// </value>
        public virtual char EscapeChar
        {
            get { return m_escape; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LikeFilter()
        {}

        /// <summary>
        /// Construct a <b>LikeFilter</b> for pattern match.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="pattern">
        /// The string pattern to compare the result with.
        /// </param>
        public LikeFilter(string member, string pattern)
            : this(member, pattern, (char) 0, false)
        {}

        /// <summary>
        /// Construct a <b>LikeFilter</b> for pattern match.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="pattern">
        /// The string pattern to compare the result with.
        /// </param>
        /// <param name="ignoreCase">
        /// <b>true</b> to be case-insensitive.
        /// </param>
        public LikeFilter(string member, string pattern, bool ignoreCase)
            : this(member, pattern, (char) 0, ignoreCase)
        {}

        /// <summary>
        /// Construct a <b>LikeFilter</b> for pattern match.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="pattern">
        /// The string pattern to compare the result with.
        /// </param>
        /// <param name="escape">
        /// The escape character for escaping '%' and '_'.
        /// </param>
        /// <param name="ignoreCase">
        /// <b>true</b> to be case-insensitive.
        /// </param>
        public LikeFilter(string member, string pattern, char escape, bool ignoreCase)
            : base(member, pattern)
        {
            Init(escape, ignoreCase);
        }

        /// <summary>
        /// Construct a <b>LikeFilter</b> for pattern match.
        /// </summary>
        /// <param name="extractor">
        /// The <see cref="IValueExtractor"/> to use by this filter.
        /// </param>
        /// <param name="pattern">
        /// The string pattern to compare the result with.
        /// </param>
        /// <param name="escape">
        /// The escape character for escaping '%' and '_'.
        /// </param>
        /// <param name="ignoreCase">
        /// <b>true</b> to be case-insensitive.
        /// </param>
        public LikeFilter(IValueExtractor extractor, string pattern, char escape, bool ignoreCase)
            : base(extractor, pattern)
        {
            Init(escape, ignoreCase);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Initialize this filter.
        /// </summary>
        /// <param name="escape">
        /// The escape character for escaping '%' and '_'.
        /// </param>
        /// <param name="ignoreCase">
        /// <b>true</b> to be case-insensitive.
        /// </param>
        private void Init(char escape, bool ignoreCase)
        {
            m_escape     = escape;
            m_ignoreCase = ignoreCase;

            BuildPlan();
        }

        /// <summary>
        /// Check the passed string value to see if it matches the pattern
        /// that this filter was constructed with.
        /// </summary>
        /// <param name="value">
        /// The <b>string</b> value to match against this filter's pattern.
        /// </param>
        /// <returns>
        /// <b>true</b> if the passed <b>string</b> value is LIKE this
        /// filter's pattern.
        /// </returns>
        protected internal virtual bool IsMatch(string value)
        {
            if (value == null)
            {
                // null is not like anything
                return false;
            }

            int length = value.Length;
            switch (m_plan)
            {

                case OptimizationPlan.StartsWithChar:
                    return length >= 1 && value[0] == m_partChar;

                case OptimizationPlan.StartsWithString:
                    return value.StartsWith(m_part);

                case OptimizationPlan.StartsWithInsens:
                    {
                        string prefix       = m_part;
                        int    prefixLength = prefix.Length;
                        if (prefixLength > length)
                        {
                            return false;
                        }
                        return String.Compare(value, 0, prefix, 0, prefixLength, true) == 0;
                    }

                case OptimizationPlan.EndsWithChar:
                    return length >= 1 && value[length - 1] == m_partChar;

                case OptimizationPlan.EndsWithString:
                    return value.EndsWith(m_part);

                case OptimizationPlan.EndsWithInsens:
                    {
                        string suffix       = m_part;
                        int    suffixLength = suffix.Length;
                        if (suffixLength > length)
                        {
                            return false;
                        }
                        return String.Compare(value, length - suffixLength, suffix, 0, suffixLength, true) == 0;
                    }

                case OptimizationPlan.ContainsChar:
                    return value.IndexOf(m_partChar) >= 0;

                case OptimizationPlan.ContainsString:
                    return value.IndexOf(m_part) >= 0;

                case OptimizationPlan.AlwaysTrue:
                    return true;

                case OptimizationPlan.AlwaysFalse:
                    return false;

                case OptimizationPlan.ExactMatch:
                    return m_part.Equals(value);

                case OptimizationPlan.InsensMatch:
                    return m_part.ToUpper().Equals(value.ToUpper());
            }

            // get the character data and iteratively process the LIKE
            char[] chars       = value.ToCharArray();
            int    charsLength = chars.Length;
            int    ofBegin     = 0;
            int    ofEnd       = charsLength;

            // start by checking the front
            MatchStep matchStep = m_stepFront;
            if (matchStep != null)
            {
                int stepLength = matchStep.Length;
                if (stepLength > charsLength || matchStep.IndexOf(chars, ofBegin, stepLength) < 0)
                {
                    return false;
                }
                ofBegin = stepLength;
            }

            // next check the back
            matchStep = m_stepBack;
            if (matchStep != null)
            {
                int stepLength = matchStep.Length;
                int ofStep     = charsLength - stepLength;
                if (ofStep < ofBegin || matchStep.IndexOf(chars, ofStep, ofEnd) < 0)
                {
                    return false;
                }
                ofEnd = ofStep;
            }

            // check the middle
            MatchStep[] matchStepMiddle = m_stepsMiddle;
            if (matchStepMiddle != null)
            {
                for (int i = 0, c = matchStepMiddle.Length; i < c; ++i)
                {
                    matchStep = matchStepMiddle[i];
                    int of = matchStep.IndexOf(chars, ofBegin, ofEnd);
                    if (of < 0)
                    {
                        return false;
                    }
                    ofBegin = of + matchStep.Length;
                }
            }

            // this is the "is there anything left" check, which solves an
            // ambiguity in the "iterative step" design that did not correctly
            // differentiate between "%a_%" and "%a_", for example
            if (m_stepBack == null && !m_isTrailingTextAllowed)
            {
                if (ofBegin != charsLength)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Build a plan for processing the LIKE functionality.
        /// </summary>
        protected internal virtual void BuildPlan()
        {
            string pattern = Pattern;
            if (pattern == null)
            {
                // the result of "v LIKE NULL" is false for all values of "v"
                m_plan = OptimizationPlan.AlwaysFalse;
                return;
            }

            char[]        patternChars       = pattern.ToCharArray();
            int           patternCharsLength = patternChars.Length;
            char          escapeChar         = EscapeChar;
            bool          isEscape           = false;
            bool          ignoreCase         = IgnoreCase;
            StringBuilder sb                 = null;
            BitArray      bits               = null;
            IList         list               = new ArrayList();

            // parse the pattern into a list of steps
            for (int of = 0; of < patternCharsLength; ++of)
            {
                char ch = patternChars[of];
                if (isEscape)
                {
                    isEscape = false;
                }
                else if (ch == escapeChar)
                {
                    isEscape = true;
                    continue;
                }
                else if (ch == '%')
                {
                    if (sb != null)
                    {
                        list.Add(new MatchStep(this, sb, bits));
                        sb = null;
                        bits = null;
                    }

                    if ((list.Count == 0) || list[list.Count - 1] != ANY)
                    {
                        list.Add(ANY);
                    }
                    continue;
                }
                else if (ch == '_')
                {
                    if (bits == null)
                    {
                        bits = new BitArray(64);
                    }
                    CollectionUtils.SetBit(bits, sb == null ? 0 : sb.Length, true);
                }

                if (sb == null)
                {
                    sb = new StringBuilder();
                }
                sb.Append(ch);
            }

            // check for unclosed escape
            if (isEscape)
            {
                throw new ArgumentException("pattern ends with an unclosed escape: \"" + pattern + "\"");
            }

            // store off the last match step (if there is one)
            if (sb != null)
            {
                list.Add(new MatchStep(this, sb, bits));
            }

            // check for simple optimizations
            switch (list.Count)
            {
                case 0:
                    // case sensistive     case insensitive    pattern
                    // ------------------  ------------------  -------
                    // OptimizationPlan.ExactMatch         OptimizationPlan.ExactMatch         ""
                    m_plan = OptimizationPlan.ExactMatch;
                    m_part = "";
                    return;

                case 1:
                    // case sensistive     case insensitive    pattern
                    // ------------------  ------------------  -------
                    // OptimizationPlan.ExactMatch         OptimizationPlan.InsensMatch        "xyz"  (no wildcards)
                    // OptimizationPlan.AlwaysTrue         OptimizationPlan.AlwaysTrue         "%"    (only '%' wildcards)
                    {
                        object o = list[0];
                        if (o == ANY)
                        {
                            m_plan = OptimizationPlan.AlwaysTrue;
                            return;
                        }

                        var matchstep = (MatchStep) o;
                        if (matchstep.IsLiteral)
                        {
                            m_plan = ignoreCase ? OptimizationPlan.InsensMatch : OptimizationPlan.ExactMatch;

                            // matchstep may contain escaped chars (such as '_')
                            m_part = matchstep.String;
                            return;
                        }
                    }
                    break;

                case 2:
                    // case sensistive     case insensitive    pattern
                    // ------------------  ------------------  -------
                    // OptimizationPlan.StartsWithChar    OptimizationPlan.StartsWithInsens  "x%"
                    // OptimizationPlan.StartsWithString  OptimizationPlan.StartsWithInsens  "xyz%"
                    // OptimizationPlan.EndsWithChar      OptimizationPlan.EndsWithInsens    "%x"
                    // OptimizationPlan.EndsWithString    OptimizationPlan.EndsWithInsens    "%xyz"
                    {
                        MatchStep matchStep;
                        bool      startsWith;
                        object o = list[0];
                        if (o == ANY)
                        {
                            startsWith = false;
                            matchStep  = (MatchStep) list[1];
                        }
                        else
                        {
                            startsWith = true;
                            matchStep  = (MatchStep) o;
                        }
                        if (matchStep.IsLiteral)
                        {
                            if (ignoreCase)
                            {
                                m_plan = startsWith ? OptimizationPlan.StartsWithInsens : OptimizationPlan.EndsWithInsens;
                                m_part = matchStep.String;
                            }
                            else if (matchStep.Length == 1)
                            {
                                m_plan     = startsWith ? OptimizationPlan.StartsWithChar : OptimizationPlan.EndsWithChar;
                                m_partChar = matchStep.String[0];
                            }
                            else
                            {
                                m_plan = startsWith ? OptimizationPlan.StartsWithString : OptimizationPlan.EndsWithString;
                                m_part = matchStep.String;
                            }
                            return;
                        }
                    }
                    break;

                case 3:
                    // case sensistive     case insensitive    pattern
                    // ------------------  ------------------  -------
                    // OptimizationPlan.ContainsChar       n/a                 "%x%"
                    // OptimizationPlan.ContainsString     n/a                 "%xyz%"
                    {
                        if (!ignoreCase)
                        {
                            object o = list[1];
                            if (o != ANY)
                            {
                                var matchstep = (MatchStep) o;
                                if (matchstep.IsLiteral)
                                {
                                    if (matchstep.Length == 1)
                                    {
                                        m_plan     = OptimizationPlan.ContainsChar;
                                        m_partChar = matchstep.String[0];
                                    }
                                    else
                                    {
                                        m_plan = OptimizationPlan.ContainsString;
                                        m_part = matchstep.String;
                                    }
                                    return;
                                }
                            }
                        }
                    }
                    break;
            }

            // build iterative plan
            // # steps  description
            // -------  --------------------------------------------------------
            //    1     match with '_'
            //    2     starts with or ends with match with '_'
            //    3     starts and ends with matches, or contains match with '_'
            //    4+    alternating % and matches, potentially starting with
            //          and/or ending with matches, each could have '_'
            m_plan = OptimizationPlan.IterativeEval;
            switch (list.Count)
            {
                case 0:
                    throw new Exception("assertion failed");

                case 1:
                    m_stepFront           = (MatchStep) list[0];
                    m_isTrailingTextAllowed = false;
                    break;

                case 2:
                    {
                        object step1 = list[0];
                        object step2 = list[1];

                        // should not have two "ANYs" in a row, but one must be ANY
                        Debug.Assert(step1 == ANY ^ step2 == ANY);

                        if (step1 == ANY)
                        {
                            m_stepBack            = (MatchStep) step2;
                            m_isTrailingTextAllowed = false;
                        }
                        else
                        {
                            m_stepFront           = (MatchStep) step1;
                            m_isTrailingTextAllowed = true;
                        }
                    }
                    break;

                default:
                    {
                        int matchStepsCount = list.Count;

                        // figure out where the "middle" is; the "middle" is
                        // defined as those steps that occur after one or more
                        // '%' matches and before one or more '%' matches
                        int ofStartMiddle = 1; // offset in list of first middle step
                        int ofEndMiddle   = matchStepsCount - 2; // offset in list of last middle step

                        object first = list[0];
                        if (first != ANY)
                        {
                            m_stepFront = (MatchStep) first;
                            ++ofStartMiddle;
                        }

                        object last          = list[matchStepsCount - 1];
                        bool   isLastStepAny = (last == ANY);
                        if (!isLastStepAny)
                        {
                            m_stepBack = (MatchStep) last;
                            --ofEndMiddle;
                        }
                        m_isTrailingTextAllowed = isLastStepAny;

                        int matches      = (ofEndMiddle - ofStartMiddle) / 2 + 1;
                        var matchesArray = new MatchStep[matches];
                        int match        = 0;
                        for (int of = ofStartMiddle; of <= ofEndMiddle; of += 2)
                        {
                            matchesArray[match++] = (MatchStep)list[of];
                        }
                        m_stepsMiddle = matchesArray;
                    }
                    break;
            }
        }

        #endregion

        #region Inner class: MatchStep

        /// <summary>
        /// Handles one matching step for a literal or a character-by-
        /// character (literal and/or '_' matching).
        /// </summary>
        private class MatchStep
        {
            #region Properties

            /// <summary>
            /// The match pattern.
            /// </summary>
            /// <value>
            /// The match pattern as a String.
            /// </value>
            public virtual string String
            {
                get { return m_match; }
            }

            /// <summary>
            /// The length of the match pattern.
            /// </summary>
            /// <value>
            /// The length of the match pattern.
            /// </value>
            public virtual int Length
            {
                get { return m_matchCharsUpper.Length; }
            }

            /// <summary>
            /// Determines if there are wildcards in the match pattern.
            /// </summary>
            /// <value>
            /// <b>true</b> if there are no wildcards ('_') in the match
            /// pattern.
            /// </value>
            public virtual bool IsLiteral
            {
                get { return m_any == null; }
            }

            /// <summary>
            /// Parent <b>LikeFilter</b>.
            /// </summary>
            /// <value>
            /// Parent <b>LikeFilter</b>.
            /// </value>
            public virtual LikeFilter Filter
            {
                get { return m_filter; }
            }

            #endregion

            #region Constructors

            /// <summary>
            /// Construct a MatchStep object.
            /// </summary>
            /// <param name="enclosingInstance">
            /// <b>LikeFilter</b> instance.
            /// </param>
            /// <param name="sb">
            /// The StringBuffer of characters to match in this step.
            /// </param>
            /// <param name="bits">
            /// Corresponding to each character, true if any character is
            /// allowed ('_').
            /// </param>
            public MatchStep(LikeFilter enclosingInstance, StringBuilder sb, BitArray bits)
            {
                m_filter = enclosingInstance;

                string match           = sb.ToString();
                char[] matchChars      = match.ToCharArray();
                char[] matchCharsLower = null;
                bool[] any             = null;

                int  skipFront     = 0; // count of leading wildcards
                int  skipBack      = 0; // count of trailing wildcards
                bool isMiddleWilds = false; // true iff any wildcards occur
                // in the middle of non-wildcards
                if (bits != null)
                {
                    int charsCount = matchChars.Length;
                    any = new bool[charsCount]; // true for each char that is a wildcard
                    bool isFront        = true; // false iff a non-wildcard is encountered
                    int  wildsCount     = 0; // total number of wildcards
                    int  contWildsCount = 0; // current count of continuous wildcards
                    for (int i = 0; i < charsCount; ++i)
                    {
                        if (CollectionUtils.GetBit(bits, i))
                        // indicates a wildcard
                        {
                            any[i] = true;
                            if (isFront)
                            {
                                ++skipFront;
                            }
                            ++wildsCount;
                            ++contWildsCount;
                        }
                        else
                        {
                            isFront = false;
                            contWildsCount = 0;
                        }
                    }

                    if (contWildsCount > 0 && contWildsCount < wildsCount)
                    {
                        skipBack = contWildsCount; // trailing continuous wildcards
                    }
                    isMiddleWilds = (wildsCount > (skipFront + skipBack));
                }

                if (Filter.IgnoreCase)
                {
                    // create both "upper" and "lower" case characters for the
                    // literal characters that need to be matched
                    int matchCharsCount = matchChars.Length;
                    matchCharsLower = new char[matchCharsCount];
                    for (int i = 0; i < matchCharsCount; ++i)
                    {
                        char ch = matchChars[i];
                        if (any == null || !any[i])
                        {
                            ch = Char.ToUpper(ch);
                            matchChars[i] = ch;
                            ch = Char.ToLower(ch);
                        }
                        matchCharsLower[i] = ch;
                    }
                }

                m_match           = match;
                m_matchCharsUpper = matchChars;
                m_matchCharsLower = matchCharsLower;
                m_any             = any;
                m_skipFront       = skipFront;
                m_skipBack        = skipBack;
                m_isMiddleWilds     = isMiddleWilds;
            }

            #endregion

            #region Helper methods

            /// <summary>
            /// Return a human-readable description for this object.
            /// </summary>
            /// <returns>
            /// A human-readable description of this object.
            /// </returns>
            public override string ToString()
            {
                return "MatchStep(" + m_match + ", " + (m_any == null ? "exact" : "wild") + ')';
            }

            /// <summary>
            /// Find the first index of this match step in the passed
            /// character array starting at the passed offset and within the
            /// specified number of characters.
            /// </summary>
            /// <param name="chars">
            /// The array of characters within which to find a match.
            /// </param>
            /// <param name="offsetBegin">
            /// The starting offset in character array to start looking for a
            /// match.
            /// </param>
            /// <param name="offsetEnd">
            /// The first offset in the character array which is beyond the
            /// region that this operation is allowed to search through to
            /// find a match.
            /// </param>
            /// <returns>
            /// The first index at which the match is made, or -1 if the
            /// match cannot be made in the designated range of offsets.
            /// </returns>
            public virtual int IndexOf(char[] chars, int offsetBegin, int offsetEnd)
            {
                char[] matchChars      = m_matchCharsUpper;
                int    matchCharsCount = matchChars.Length;
                int    cch             = offsetEnd - offsetBegin;
                if (matchCharsCount > cch)
                {
                    // doesn't fit: can't match
                    return -1;
                }

                int skipFront = m_skipFront;
                if (skipFront > 0)
                {
                    if (skipFront == matchCharsCount)
                    {
                        // just wildcards; found it if it fits
                        return offsetBegin;
                    }

                    // do not bother to match leading wildcards
                    offsetBegin += skipFront;
                    offsetEnd   += skipFront;
                }

                offsetEnd       -= matchCharsCount; // determine last offset that allows it to fit
                matchCharsCount -= m_skipBack; // don't bother matching trailing wilds

                bool   isMiddleWilds = m_isMiddleWilds;
                bool[] any           = m_any;

                if (Filter.IgnoreCase)
                {
                    // processed in an equivalent way to String.EqualsIngoreCase()
                    char[] matchCharsLower = m_matchCharsLower;
                    char   firstUpper      = matchChars[skipFront];
                    char   firstLower      = matchCharsLower[skipFront];
                    for (; offsetBegin <= offsetEnd; ++offsetBegin)
                    {
                        char ch = chars[offsetBegin];
                        if (ch == firstUpper || ch == firstLower)
                        {
                            if (isMiddleWilds)
                            {
                                for (int ofMatch = skipFront + 1, ofCur = offsetBegin + 1; ofMatch < matchCharsCount; ++ofMatch, ++ofCur)
                                {
                                    if (!any[ofMatch])
                                    {
                                        ch = chars[ofCur];
                                        if (ch != matchChars[ofMatch] && ch != matchCharsLower[ofMatch])
                                        {
                                            goto NextChar;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int ofMatch = skipFront + 1, ofCur = offsetBegin + 1; ofMatch < matchCharsCount; ++ofMatch, ++ofCur)
                                {
                                    ch = chars[ofCur];
                                    if (ch != matchChars[ofMatch] && ch != matchCharsLower[ofMatch])
                                    {
                                        goto NextChar;
                                    }
                                }
                            }

                            // found it; adjust for the leading wilds that we skipped matching
                            return offsetBegin - skipFront;
                        }

                    NextChar: ;
                    }
                }
                else
                {
                    // scan for a match
                    char chFirst = matchChars[skipFront];
                    for (; offsetBegin <= offsetEnd; ++offsetBegin)
                    {
                        if (chars[offsetBegin] == chFirst)
                        {
                            if (isMiddleWilds)
                            {
                                for (int ofMatch = skipFront + 1, ofCur = offsetBegin + 1; ofMatch < matchCharsCount; ++ofMatch, ++ofCur)
                                {
                                    if (!any[ofMatch] && matchChars[ofMatch] != chars[ofCur])
                                    {
                                        goto NextChar1;
                                    }
                                }
                            }
                            else
                            {
                                for (int ofMatch = skipFront + 1, ofCur = offsetBegin + 1; ofMatch < matchCharsCount; ++ofMatch, ++ofCur)
                                {
                                    if (matchChars[ofMatch] != chars[ofCur])
                                    {
                                        goto NextChar1;
                                    }
                                }
                            }

                            // found it; adjust for the leading wilds that we skipped matching
                            return offsetBegin - skipFront;
                        }

                    NextChar1: ;
                    }
                }

                return -1;
            }

            #endregion

            #region Data members

            /// <summary>
            /// The match pattern, as a string.
            /// </summary>
            private readonly string m_match;

            /// <summary>
            /// The match pattern, as an array of char values.
            /// </summary>
            /// <remarks>
            /// If the filter is case insensitive, then this is the
            /// uppercase form of the char values.
            /// </remarks>
            private readonly char[] m_matchCharsUpper;

            /// <summary>
            /// The match pattern for a case insensitive like filter,
            /// as an array of lowercase char values.
            /// </summary>
            /// <remarks>
            /// For case sensitive filters, this is <c>null</c>.
            /// </remarks>
            private readonly char[] m_matchCharsLower;

            /// <summary>
            /// For each character, <b>true</b> if the character is a
            /// wildcard ('_'), or <c>null</c> if there are no wildcards.
            /// </summary>
            private readonly bool[] m_any;

            /// <summary>
            /// Number of leading wildcards.
            /// </summary>
            private readonly int m_skipFront;

            /// <summary>
            /// Number of trailing wildcards.
            /// </summary>
            private readonly int m_skipBack;

            /// <summary>
            /// <b>true</b> if there are any wildcards in the middle.
            /// </summary>
            private readonly bool m_isMiddleWilds;

            /// <summary>
            /// Parent LikeFilter instance.
            /// </summary>
            private readonly LikeFilter m_filter;

            #endregion
        }

        #endregion

        #region ExtractorFilter override methods

        /// <summary>
        /// Evaluate the specified extracted value.
        /// </summary>
        /// <param name="extracted">
        /// An extracted value to evaluate.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        protected internal override bool EvaluateExtracted(object extracted)
        {
            try
            {
                string value = extracted == null ? null : extracted.ToString();
                return IsMatch(value);
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            m_ignoreCase = reader.ReadBoolean(2);
            m_escape     = reader.ReadChar(3);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteBoolean(2, m_ignoreCase);
            writer.WriteChar(3, m_escape);
        }

        #endregion

        #region IIndexAwareFilter implementation

        /// <summary>
        /// Given an IDictionary of available indexes, determine if this 
        /// IIndexAwareFilter can use any of the indexes to assist in its 
        /// processing, and if so, determine how effective the use of that 
        /// index would be.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The returned value is an effectiveness estimate of how well this 
        /// filter can use the specified indexes to filter the specified 
        /// keys. An operation that requires no more than a single access to 
        /// the index content (i.e. Equals, NotEquals) has an effectiveness of 
        /// <b>one</b>. Evaluation of a single entry is assumed to have an 
        /// effectiveness that depends on the index implementation and is 
        /// usually measured as a constant number of the single operations.  
        /// This number is referred to as <i>evaluation cost</i>.
        /// </p>
        /// <p>
        /// If the effectiveness of a filter evaluates to a number larger 
        /// than the keys.size() then a user could avoid using the index and 
        /// iterate through the keys calling <tt>Evaluate</tt> rather than 
        /// <tt>ApplyIndex</tt>.
        /// </p>
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The set of keys that will be filtered; read-only.
        /// </param>
        /// <returns>
        /// An effectiveness estimate of how well this filter can use the 
        /// specified indexes to filter the specified keys.
        /// </returns>
        public int CalculateEffectiveness(IDictionary indexes, ICollection keys)
        {
            OptimizationPlan plan = m_plan;
            if (plan == OptimizationPlan.AlwaysFalse || plan == OptimizationPlan.AlwaysTrue)
            {
                return 1;
            }

            var index = (ICacheIndex) indexes[ValueExtractor];
            if (index == null)
            {
                return CalculateIteratorEffectiveness(keys.Count);
            }

            if (plan == OptimizationPlan.ExactMatch)
            {
                return 1;
            }

            string pattern = Pattern;
            if (index.IsOrdered && pattern.IndexOf('%') != 0 && pattern.IndexOf('_') != 0)
            {
                return Math.Max(index.IndexContents.Count / 4, 1);
            }
            return index.IndexContents.Count;
        }

        /// <summary>
        /// Filter remaining keys using an IDictionary of available indexes.
        /// </summary>
        /// <remarks>
        /// The filter is responsible for removing all keys from the passed 
        /// set of keys that the applicable indexes can prove should be 
        /// filtered. If the filter does not fully evaluate the remaining 
        /// keys using just the index information, it must return a filter
        /// (which may be an <see cref="IEntryFilter"/>) that can complete the 
        /// task using an iterating implementation. If, on the other hand, the
        /// filter does fully evaluate the remaining keys using just the index
        /// information, then it should return <c>null</c> to indicate that no 
        /// further filtering is necessary.
        /// </remarks>
        /// <param name="indexes">
        /// The available <see cref="ICacheIndex"/> objects keyed by the 
        /// related IValueExtractor; read-only.
        /// </param>
        /// <param name="keys">
        /// The mutable set of keys that remain to be filtered.
        /// </param>
        /// <returns>
        /// An <see cref="IFilter"/> object that can be used to process the 
        /// remaining keys, or <c>null</c> if no additional filter processing 
        /// is necessary.
        /// </returns>
        public IFilter ApplyIndex(IDictionary indexes, ICollection keys)
        {
            OptimizationPlan plan = m_plan;
            switch (plan)
            {
                case OptimizationPlan.AlwaysFalse:
                    CollectionUtils.Clear(keys);
                    return null;

                case OptimizationPlan.AlwaysTrue:
                    return null;
            }

            var index = (ICacheIndex) indexes[ValueExtractor];
            if (index == null)
            {
                // there is no relevant index
                return this;
            }

            if (plan == OptimizationPlan.ExactMatch)
            {
                var setEquals = (ICollection) index.IndexContents[m_part];
                if (setEquals == null || setEquals.Count == 0)
                {
                    CollectionUtils.Clear(keys);
                }
                else
                {
                    CollectionUtils.RetainAll(keys, setEquals);
                }
                return null;
            }

            IDictionary contents = index.IndexContents;
            ICollection matches;

            if ((plan == OptimizationPlan.StartsWithString || 
                 plan == OptimizationPlan.StartsWithChar) && index.IsOrdered)
            {
                try
                {
                    string prefix = plan == OptimizationPlan.StartsWithString ?
                        m_part : Char.ToString(m_partChar);

                    SortedList tail = CollectionUtils.TailList(contents, prefix);
                    matches = new HashSet();
                    foreach (DictionaryEntry entry in tail)
                    {
                        var value = (string) entry.Key;
                        if (value.StartsWith(prefix))
                        {
                            CollectionUtils.AddAll(matches, (ICollection) entry.Value);
                        }
                        else
                        {
                            break;
                        }
                    }
                    CollectionUtils.RetainAll(keys, matches);
                    return null;
                }
                catch (InvalidCastException)
                {
                    // incompatible types; go the long way
                }   
            }

            matches = new HashSet();
            foreach (DictionaryEntry entry in contents)
            {
                string value = entry.Key == null ? null : entry.Key.ToString();
                if (IsMatch(value))
                {
                    CollectionUtils.AddAll(matches, (ICollection) entry.Value);
                }
            }
            CollectionUtils.RetainAll(keys, matches);

            return null;
        }

        #endregion

        #region Enum: OptimizationPlan

        /// <summary>
        /// Optimization plan enum values.
        /// </summary>
        private enum OptimizationPlan
        {
            /// <summary>
            /// Non-optimized plan with support for trailing data.
            /// </summary>
            IterativeEval = 0,

            /// <summary>
            /// Optimized plan: The pattern is anything that starts with a
            /// specific character ("x%").
            /// </summary>
            StartsWithChar = 1,

            /// <summary>
            /// Optimized plan: The pattern is anything that starts with a
            /// specific string ("xyz%").
            /// </summary>
            StartsWithString = 2,

            /// <summary>
            /// Optimized plan: The pattern is anything that starts with a
            /// specific (but case-insensitive) string ("xyz%").
            /// </summary>
            StartsWithInsens = 3,

            /// <summary>
            /// Optimized plan: The pattern is anything that ends with a
            /// specific character ("%x").
            /// </summary>
            EndsWithChar = 4,

            /// <summary>
            /// Optimized plan: The pattern is anything that ends with a
            /// specific string ("%xyz").
            /// </summary>
            EndsWithString = 5,

            /// <summary>
            /// Optimized plan: The pattern is anything that ends with a
            /// specific (but case-insensitive) string ("%xyz").
            /// </summary>
            EndsWithInsens = 6,

            /// <summary>
            /// Optimized plan: The pattern is anything that contains a
            /// specific character ("%x%").
            /// </summary>
            ContainsChar = 7,

            /// <summary>
            /// Optimized plan: The pattern is anything that contains a
            /// specific string ("%xyz%").
            /// </summary>
            ContainsString = 8,

            /// <summary>
            /// Optimized plan: Everyting matches ("%").
            /// </summary>
            AlwaysTrue = 9,

            /// <summary>
            /// Optimized plan: Nothing matches (null).
            /// </summary>
            AlwaysFalse = 10,

            /// <summary>
            /// Optimized plan: Exact match ("xyz").
            /// </summary>
            ExactMatch = 11,

            /// <summary>
            /// Optimized plan: Exact case-insensitive match ("xyz").
            /// </summary>
            InsensMatch = 12
        }

        #endregion

        #region Constants

        /// <summary>
        /// A special object that represents a "match any" ('%') portion of a
        /// pattern while building a processing plan.
        /// </summary>
        private static readonly object ANY = new object();

        #endregion

        #region Data members

        /// <summary>
        /// The escape character for escaping '_' and '%' in the pattern.
        /// The value zero is reserved to mean "no escape".
        /// </summary>
        private char m_escape;

        /// <summary>
        /// The option to ignore case sensitivity. <b>true</b> means that
        /// the filter will match using the same logic that is used by the
        /// <b>String.EqualsIgnoreCase</b> method.
        /// </summary>
        private bool m_ignoreCase;

        /// <summary>
        /// Optimization plan number. Zero means default iterative evalution
        /// is necessary.
        /// </summary>
        [NonSerialized]
        private OptimizationPlan m_plan;

        /// <summary>
        /// Used by single-character matching optimization plans.
        /// </summary>
        [NonSerialized]
        private char m_partChar;

        /// <summary>
        /// Used by string-character matching optimization plans.
        /// </summary>
        [NonSerialized]
        private string m_part;

        /// <summary>
        /// The "front" matching step used by the iterative processing;
        /// <c>null</c> if the pattern starts with '%'.
        /// </summary>
        [NonSerialized]
        private MatchStep m_stepFront;

        /// <summary>
        /// The "back" matching step used by the iterative processing;
        /// <c>null</c> if the pattern ends with '%'.
        /// </summary>
        [NonSerialized]
        private MatchStep m_stepBack;

        /// <summary>
        /// For iterative plans with a null "back" matching step,
        /// is trailing data permitted.
        /// </summary>
        [NonSerialized]
        private bool m_isTrailingTextAllowed;

        /// <summary>
        /// The array of "middle" matching steps used by the iterative
        /// processing; may be <c>null</c> if none.
        /// </summary>
        [NonSerialized]
        private MatchStep[] m_stepsMiddle;

        #endregion
    }
}