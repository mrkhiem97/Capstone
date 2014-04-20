using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geocoding.Google;
using Geocoding.Microsoft;
using Geocoding.Yahoo;
using Geocoding;
using System.Threading.Tasks;

namespace MobileSurveillanceWebApplication.Utility
{
    public class ReverseGeoCoding
    {
        private const String BING_KEY = "ApkMhhw06HBvclIOsC50uqVPP4gAJHEE-YYQt03gox9NlvkB-eXIxk7p35tXRIOg";
        private const String YAHOO_CONSUMER_KEY = "dj0yJmk9dmZrREFxTTR0ckxNJmQ9WVdrOU5rTTJSRmhVTkRRbWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD02Yg--";
        private const String YAHOO_CONSUMER_SECRET = "7fb73739761944570b28e72bf5a49364c37a3958";
        public static async Task<String> GetAddress(double latitude, double longitude)
        {
            String retVal = String.Empty;
            Location location = new Location(latitude, longitude);
            IAsyncGeocoder geocoder;
            IEnumerable<Address> listAddress;

            // Google Geocoding
            geocoder = new GoogleGeocoder();
            listAddress = await geocoder.ReverseGeocodeAsync(location, System.Threading.CancellationToken.None);
            if (listAddress != null && listAddress.Count() > 0)
            {
                retVal = listAddress.ElementAt(0).FormattedAddress;
                return retVal;
            }

            //Bing Geocoding
            geocoder = new BingMapsGeocoder(BING_KEY);
            listAddress = await geocoder.ReverseGeocodeAsync(location, System.Threading.CancellationToken.None);
            if (listAddress != null && listAddress.Count() > 0)
            {
                retVal = listAddress.OrderByDescending(x => x.FormattedAddress.Length).ElementAt(0).FormattedAddress;
                return retVal;
            }

            //Yahoo Geocoding
            try
            {
                IGeocoder yahooGeocoder = new YahooGeocoder(YAHOO_CONSUMER_KEY, YAHOO_CONSUMER_SECRET);
                listAddress = yahooGeocoder.ReverseGeocode(location);
                if (listAddress != null && listAddress.Count() > 0)
                {
                    retVal = listAddress.ElementAt(0).FormattedAddress;
                    return retVal;
                }
            }
            catch (Exception ex)
            { }
            return retVal;
        }
    }
}