using System;
using System.Collections.Generic;
using System.Globalization;

namespace Unity.Services.LevelPlay
{
    /// <summary>
    /// Represents data for an ad impression event.
    /// </summary>
    public class LevelPlayImpressionData
    {
#pragma warning disable 0618

        /// <summary>
        /// The id for the auction.
        /// </summary>
        public string AuctionId => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_AUCTION_ID);

        /// <summary>
        /// The creative id of the ad campaign.
        /// </summary>
        public string CreativeId => GetValueAsString(IronSourceConstants.k_ImpressionDataKeyCreativeID);

        /// <summary>
        /// The format of the ad.
        /// </summary>
        public string AdFormat => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_AD_FORMAT);

        /// <summary>
        /// The mediation ad unit name of the ad.
        /// </summary>
        public string MediationAdUnitName =>
            GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_MEDIATION_AD_UNIT_NAME);

        /// <summary>
        /// The mediation ad unit id of the ad.
        /// </summary>
        public string MediationAdUnitId =>
            GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_MEDIATION_AD_UNIT_ID);

        /// <summary>
        /// Country code ISO 3166-1 format.
        /// </summary>
        public string Country => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_COUNTRY);

        /// <summary>
        /// Indication if AB test was activated.
        /// </summary>
        public string Ab => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_ABTEST);

        /// <summary>
        /// The segment the user is associated with.
        /// </summary>
        public string SegmentName => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_SEGMENT_NAME);

        /// <summary>
        /// The placement the user is associated with.
        /// </summary>
        public string Placement => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_PLACEMENT);

        /// <summary>
        /// The ad network name that served the ad.
        /// </summary>
        public string AdNetwork => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_AD_NETWORK);

        /// <summary>
        /// The ad network instance name as defined on the platform.
        /// </summary>
        public string InstanceName => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_INSTANCE_NAME);

        /// <summary>
        /// The ad network instance id as defined on the platform.
        /// </summary>
        public string InstanceId => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_INSTANCE_ID);

        /// <summary>
        /// The revenue generated for the impression.
        /// </summary>
        public double? Revenue => GetValueAsDouble(IronSourceConstants.IMPRESSION_DATA_KEY_REVENUE);

        /// <summary>
        /// The source value of the revenue field.
        /// </summary>
        public string Precision => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_PRECISION);

        /// <summary>
        /// The encrypted cpm associated with the impression. Available for some of the ad networks.
        /// </summary>
        public string EncryptedCpm => GetValueAsString(IronSourceConstants.IMPRESSION_DATA_KEY_ENCRYPTED_CPM);

        /// <summary>
        /// The conversion value associated with the impression.
        /// </summary>
        public int? ConversionValue => GetValueAsInt(IronSourceConstants.IMPRESSION_DATA_KEY_CONVERSION_VALUE);

        /// <summary>
        /// All the data associated with the impression.
        /// </summary>
        public string AllData { get; }

        private readonly Dictionary<string, object> InternalDictionary;

        internal LevelPlayImpressionData(string levelplayImpressionJson)
        {
            AllData = levelplayImpressionJson;
            InternalDictionary = ParseJson(levelplayImpressionJson);
        }

        private Dictionary<string, object> ParseJson(string json)
        {
            try
            {
                return IronSourceJSON.Json.Deserialize(json) as Dictionary<string, object> ??
                    new Dictionary<string, object>();
#pragma warning restore 0618
            }
            catch (Exception ex)
            {
                LevelPlayLogger.LogException(ex);
                return new Dictionary<string, object>();
            }
        }

        private string GetValueAsString(string key)
        {
            return InternalDictionary.TryGetValue(key, out var value) && value != null ? value.ToString() : null;
        }

        private double? GetValueAsDouble(string key)
        {
            if (InternalDictionary.TryGetValue(key, out var value) && value != null &&
                double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        private int? GetValueAsInt(string key)
        {
            if (InternalDictionary.TryGetValue(key, out var value) && value != null &&
                int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Returns a string representation of the impression data.
        /// </summary>
        /// <returns>A string representation of the impression data.</returns>
        public override string ToString()
        {
#pragma warning disable 0618
            return "LevelPlayImpressionData{" +
                "AuctionId='" + AuctionId + '\'' +
                ", CreativeId='" + CreativeId + '\'' +
                ", AdFormat='" + AdFormat + '\'' +
                ", MediationAdUnitName='" + MediationAdUnitName + '\'' +
                ", MediationAdUnitId='" + MediationAdUnitId + '\'' +
                ", Country='" + Country + '\'' +
                ", Ab='" + Ab + '\'' +
                ", SegmentName='" + SegmentName + '\'' +
                ", Placement='" + Placement + '\'' +
                ", AdNetwork='" + AdNetwork + '\'' +
                ", InstanceName='" + InstanceName + '\'' +
                ", InstanceId='" + InstanceId + '\'' +
                ", Revenue=" + Revenue +
                ", Precision='" + Precision + '\'' +
                ", EncryptedCpm='" + EncryptedCpm + '\'' +
                ", ConversionValue=" + ConversionValue +
                '}';
#pragma warning restore 0618
        }
    }
}
