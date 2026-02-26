// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.SemanticKernel;
using System.ComponentModel;
using System;
using System.Threading.Tasks;

namespace RetrievalBot.Plugins
{
    /// <summary>
    /// Semantic Kernel plugins for date and time.
    /// </summary>
    public class DateTimePlugin
    {
        /// <summary>
        /// Get the current date
        /// </summary>
        /// <example>
        /// {{time.date}} => Sunday, 12 January, 2031
        /// </example>
        /// <returns> The current date </returns>
        [KernelFunction, Description("Get the current date")]
        public string Date(IFormatProvider? formatProvider = null)
        {
            // Example: Sunday, 12 January, 2025
            var date = DateTimeOffset.Now.ToString("D", formatProvider);
            return date;
        }
            

        /// <summary>
        /// Get the current date
        /// </summary>
        /// <example>
        /// {{time.today}} => Sunday, 12 January, 2031
        /// </example>
        /// <returns> The current date </returns>
        [KernelFunction, Description("Get the current date")]
        public string Today(IFormatProvider? formatProvider = null) =>
            // Example: Sunday, 12 January, 2025
            this.Date(formatProvider);

        /// <summary>
        /// Get the current date and time in the local time zone"
        /// </summary>
        /// <example>
        /// {{time.now}} => Sunday, January 12, 2025 9:15 PM
        /// </example>
        /// <returns> The current date and time in the local time zone </returns>
        [KernelFunction, Description("Get the current date and time in the local time zone")]
        public string Now(IFormatProvider? formatProvider = null) =>
            // Sunday, January 12, 2025 9:15 PM
            DateTimeOffset.Now.ToString("f", formatProvider);



        // Microsoft Build typically starts around May 19-23 each year.
        // Day 22 is used as a conservative cutoff: if today is past May 22,
        // assume the current year's Build has passed and compute against next year.
        private const int BuildMonth = 5;
        private const int BuildDay = 19;
        private const int BuildRolloverDay = 22;

        [KernelFunction, Description("Get the number of days to the next Microsoft Build conference")]
        public double DaysToBuild()
        {
            DateTime today = DateTime.Now;
            int year = today.Month > BuildMonth || (today.Month == BuildMonth && today.Day > BuildRolloverDay)
                ? today.Year + 1
                : today.Year;
            DateTime nextBuild = new DateTime(year, BuildMonth, BuildDay);
            TimeSpan difference = nextBuild - today;
            return difference.TotalDays;
        }
        
    }
}
