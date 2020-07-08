﻿using System;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace AngryGrandpa
{
    public class ConsoleCommands
    {
        protected static IModHelper Helper => ModEntry.Instance.Helper;
        protected static IMonitor Monitor => ModEntry.Instance.Monitor;
        protected static ModConfig Config => ModConfig.Instance;
        
        public static void Apply()
        {
            Helper.ConsoleCommands.Add("grandpa_score",
                "Estimates the result of a farm evaluation using grandpa's scoring criteria.\n\nUsage: grandpa_score",
                cmdGrandpaScore);
            Helper.ConsoleCommands.Add("reset_evaluation",
                "Removes all event flags related to grandpa's evaluation(s).\n\nUsage: reset_evaluation",
                cmdResetEvaluation);
            Helper.ConsoleCommands.Add("grandpa_config",
                "Prints the active Angry Grandpa config settings to the console.\n\nUsage: grandpa_config",
                cmdGrandpaConfig);
            Helper.ConsoleCommands.Add("grandpa_debug",
                "Activates grandpa_config and grandpa_score commands, plus useful debugging information.\n\nUsage: grandpa_debug",
                cmdGrandpaDebug);
        }

        /// <summary>Gives a farm evaluation in console output when the 'grandpa_score' command is invoked.</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private static void cmdGrandpaScore(string _command, string[] _args)
        {
            try
            {
                if (!Context.IsWorldReady)
                {
                    throw new Exception("An active save is required.");
                }
                int grandpaScore = Utility.getGrandpaScore();
                int maxScore = Config.GetMaxScore();
                int candles = Utility.getGrandpaCandlesFromScore(grandpaScore);
                Monitor.Log($"Grandpa's Score: {grandpaScore} of {maxScore} Great Honors\nNumber of candles earned: {candles}\nScoring system: \"{Config.ScoringSystem}\"\nCandle score thresholds: [{Config.GetScoreForCandles(1)}, {Config.GetScoreForCandles(2)}, {Config.GetScoreForCandles(3)}, {Config.GetScoreForCandles(4)}]",
                    LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"grandpa_score failed:\n{ex}", LogLevel.Warn);
            }
        }

        /// <summary>Resets all event flags related to grandpa's evaluation(s) when the 'reset_evaluation' command is invoked.</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private static void cmdResetEvaluation(string _command, string[] _args)
        {
            try
            {
                if (!Context.IsWorldReady)
                {
                    throw new Exception("An active save is required.");
                }
                var eventsToRemove = new List<int>
                {
                    558291, 558292, 321777 // Initial eval, Re-eval, and Evaluation request
                };
                foreach (int e in eventsToRemove)
                {
                    while (Game1.player.eventsSeen.Contains(e)) { Game1.player.eventsSeen.Remove(e); }
                }
                // Game1.player.eventsSeen.Remove(2146991); // Candles (removed instead by command_grandpaEvaluation postfix)
                Game1.getFarm().hasSeenGrandpaNote = false; // Seen the note on the shrine
                while (Game1.player.mailReceived.Contains("grandpaPerfect")) // Received the statue of perfection
                { 
                    Game1.player.mailReceived.Remove("grandpaPerfect"); 
                } 
                Game1.getFarm().grandpaScore.Value = 0; // Reset grandpaScore
                FarmPatches.RemoveCandlesticks(Game1.getFarm()); // Removes all candlesticks (not flames).
                Game1.getFarm().removeTemporarySpritesWithIDLocal(6666f); // Removes candle flames.

                // Remove flags added by this mod
                var flagsToRemove = new List<string> 
                {
                    "6324bonusRewardsEnabled", "6324reward2candles", "6324reward3candles", // Old, outdated flags
                    "6324grandpaNoteMail", "6324reward1candle", "6324reward2candle", "6324reward3candle", "6324reward4candle", "6324hasDoneModdedEvaluation", // Current used flags
                };
                foreach (string flag in flagsToRemove)
                {
                    while (Game1.player.mailReceived.Contains(flag)) { Game1.player.mailReceived.Remove(flag); }
                }

                if (!Game1.player.eventsSeen.Contains(2146991))
                {
                    Game1.player.eventsSeen.Add(2146991); // Make sure they can't see candle event before the next evaluation.
                }

                Monitor.Log($"Reset grandpaScore and associated event and mail flags.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"reset_evaluation failed:\n{ex}", LogLevel.Warn);
            }
        }
        
        /// <summary>Prints the active Angry Grandpa config settings to the console.</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private static void cmdGrandpaConfig(string _command, string[] _args)
        {
            ModConfig.Print(); // Print config values to console
        }

        /// <summary>Prints config and score data with some extra debugging info.</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private static void cmdGrandpaDebug(string _command, string[] _args)
        {
            cmdGrandpaConfig("grandpa_config", null);
            cmdGrandpaScore("grandpa_score", null);

            try
            {
                if (!Context.IsWorldReady)
                {
                    throw new Exception("An active save is required.");
                }
                Monitor.Log($"DEBUG", LogLevel.Debug);
                Monitor.Log($"Actual current Farm.grandpaScore value: {Game1.getFarm().grandpaScore.Value}", LogLevel.Debug);
                Monitor.Log($"Actual current Farm.hasSeenGrandpaNote value: {Game1.getFarm().hasSeenGrandpaNote}", LogLevel.Debug);
                List<int> eventsAG = new List<int> { 558291, 558292, 2146991, 321777 };
                List<string> mailAG = new List<string> { "6324grandpaNoteMail", "6324reward1candle", "6324reward2candle", "6324reward3candle", "6324reward4candle", "6324bonusRewardsEnabled", "6324hasDoneModdedEvaluation" };
                Monitor.Log($"Actual eventsSeen entries: {string.Join(", ", eventsAG.Where(Game1.player.eventsSeen.Contains).ToList())}", LogLevel.Debug);
                Monitor.Log($"Actual mailReceived entries: {string.Join(", ", mailAG.Where(Game1.player.mailReceived.Contains).ToList())}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"grandpa_debug failed:\n{ex}",
                    LogLevel.Error);
            }
        }
    }
}