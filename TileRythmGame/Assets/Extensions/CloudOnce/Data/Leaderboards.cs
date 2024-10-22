// <copyright file="Leaderboards.cs" company="Jan Ivar Z. Carlsen, Sindri Jóelsson">
// Copyright (c) 2016 Jan Ivar Z. Carlsen, Sindri Jóelsson. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace CloudOnce
{
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// Provides access to leaderboards registered via the CloudOnce Editor.
    /// This file was automatically generated by CloudOnce. Do not edit.
    /// </summary>
    public static class Leaderboards
    {
        private static readonly UnifiedLeaderboard s_normalModeHighscore = new UnifiedLeaderboard("NormalModeHighscore",
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
            "com.tappytilenormalhighscore"
#elif !UNITY_EDITOR && UNITY_ANDROID && CLOUDONCE_GOOGLE
            "CgkI1I2DnbcGEAIQAA"
#else
            "NormalModeHighscore"
#endif
            );

        public static UnifiedLeaderboard NormalModeHighscore
        {
            get { return s_normalModeHighscore; }
        }

        private static readonly UnifiedLeaderboard s_hardModeHighscore = new UnifiedLeaderboard("HardModeHighscore",
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
            "com.tappytilehardhighscore"
#elif !UNITY_EDITOR && UNITY_ANDROID && CLOUDONCE_GOOGLE
            "CgkI1I2DnbcGEAIQAQ"
#else
            "HardModeHighscore"
#endif
            );

        public static UnifiedLeaderboard HardModeHighscore
        {
            get { return s_hardModeHighscore; }
        }

        public static string GetPlatformID(string internalId)
        {
            return s_leaderboardDictionary.ContainsKey(internalId)
                ? s_leaderboardDictionary[internalId].ID
                : string.Empty;
        }

        private static readonly Dictionary<string, UnifiedLeaderboard> s_leaderboardDictionary = new Dictionary<string, UnifiedLeaderboard>
        {
            { "NormalModeHighscore", s_normalModeHighscore },
            { "HardModeHighscore", s_hardModeHighscore },
        };
    }
}
