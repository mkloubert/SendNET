// LICENSE: AGPL 3 - https://www.gnu.org/licenses/agpl-3.0.txt
//
// s. https://github.com/mkloubert/SendNET

using System;

namespace MarcelJoachimKloubert.SendNET.Helpers
{
    /// <summary>
    /// Helper class for console operations.
    /// </summary>
    public sealed class ConsoleHelper
    {
        #region Methods (1)

        /// <summary>
        /// Invokes an action for specific colors.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="foreColor">The custom foreground color to set.</param>
        /// <param name="bgColor">The custom background color to set.</param>
        public static void InvokeForColor(Action action, ConsoleColor? foreColor = null, ConsoleColor? bgColor = null)
        {
            var oldFG = Console.ForegroundColor;
            var oldBG = Console.BackgroundColor;

            try
            {
                if (foreColor.HasValue)
                {
                    Console.ForegroundColor = foreColor.Value;
                }

                if (bgColor.HasValue)
                {
                    Console.BackgroundColor = bgColor.Value;
                }

                action();
            }
            finally
            {
                Console.ForegroundColor = oldFG;
                Console.BackgroundColor = oldBG;
            }
        }

        #endregion Methods (1)
    }
}