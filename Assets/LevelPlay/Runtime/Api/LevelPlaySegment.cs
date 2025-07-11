using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Representation of a user segment.
    /// </summary>
    public class LevelPlaySegment
    {

        /// <summary>
        /// Game level reached by the user.
        /// </summary>
        public int Level = -1;

        /// <summary>
        /// Indicates if the user has made any purchases in the game.
        /// </summary>
        public int IsPaying = -1;

        /// <summary>
        /// User account creation time in milliseconds.
        /// </summary>
        public long UserCreationDate = -1;

        /// <summary>
        /// The total value of purchases made by the user.
        /// </summary>
        public double IapTotal = 0;

        /// <summary>
        /// Name of the segment.
        /// </summary>
        public string SegmentName;

        /// <summary>
        /// Collection of custom key-value pairs for additional user segmentation.
        /// </summary>
        public readonly Dictionary<string, string> CustomData = new Dictionary<string, string>();

        /// <summary>
        /// Add a custom key-value pair to the segment for additional user segmentation.
        /// </summary>
        /// <param name="key">Custom parameter identifier.</param>
        /// <param name="value">The value associated with the custom parameter.</param>
        public void SetCustom(string key, string value)
        {
            CustomData[key] = value;
        }

        /// <summary>
        /// Converts the segment object into a dictionary representation.
        /// </summary>
        /// <returns>Dictionary containing all defined segment parameters as string key-value pairs.</returns>
        public Dictionary<string, string> GetSegmentAsDictionary()
        {
            var result = new Dictionary<string, string>();
            if (Level != -1)
                result.Add("level", Level.ToString());
            if (IsPaying > -1 && IsPaying < 2)
                result.Add("isPaying", IsPaying.ToString());
            if (UserCreationDate != -1)
                result.Add("userCreationDate", UserCreationDate.ToString());
            if (!string.IsNullOrEmpty(SegmentName))
                result.Add("segmentName", SegmentName);
            if (IapTotal > 0)
                result.Add("iapt", IapTotal.ToString(CultureInfo.InvariantCulture));

            foreach (var item in CustomData)
            {
                result[item.Key] = item.Value;
            }
            return result;
        }
    }
}
