
namespace GMap.NET.MapProviders
{
    using GMap.NET.Internals;
    using GMap.NET.Projections;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public abstract class GoogleMapProviderBase : GMapProvider, GeocodingProvider
    {
        public GoogleMapProviderBase()
        {
            MaxZoom = null;
            RefererUrl = string.Format("http://maps.{0}/", Server);
            Copyright = string.Format("©{0} Google - Map data ©{0} Tele Atlas, Imagery ©{0} TerraMetrics", DateTime.Today.Year);
        }

        public readonly string ServerAPIs /* ;}~~ */ = Stuff.GString(/*{^_^}*/"9gERyvblybF8iMuCt/LD6w=="/*d{'_'}b*/);
        public readonly string Server /* ;}~~~~ */ = Stuff.GString(/*{^_^}*/"gosr2U13BoS+bXaIxt6XWg=="/*d{'_'}b*/);
        public readonly string ServerChina /* ;}~ */ = Stuff.GString(/*{^_^}*/"gosr2U13BoTEJoJJuO25gQ=="/*d{'_'}b*/);
        public readonly string ServerKorea /* ;}~~ */ = Stuff.GString(/*{^_^}*/"8ZVBOEsBinzi+zmP7y7pPA=="/*d{'_'}b*/);
        public readonly string ServerKoreaKr /* ;}~ */ = Stuff.GString(/*{^_^}*/"gosr2U13BoQyz1gkC4QLfg=="/*d{'_'}b*/);

        public string SecureWord = "Galileo";

        /// <summary>
        /// Your application's API key, obtained from the Google Developers Console.
        /// This key identifies your application for purposes of quota management. 
        /// Must provide either API key or Maps for Work credentials.
        /// </summary>
        public string ApiKey = string.Empty;

        #region GMapProvider Members
        public override Guid Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
            }
        }

        GMapProvider[] overlays;
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { this };
                }
                return overlays;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            throw new NotImplementedException();
        }
        #endregion

        public bool TryCorrectVersion = true;
        static bool init = false;

        public override void OnInitialized()
        {
            if (!init && TryCorrectVersion)
            {
                string url = string.Format("https://maps.{0}/maps/api/js?client=google-maps-lite&amp;libraries=search&amp;language=en&amp;region=", ServerAPIs);
                try
                {
                    string html = GMaps.Instance.UseUrlCache ? Cache.Instance.GetContent(url, CacheType.UrlCache, TimeSpan.FromHours(GMapProvider.TTLCache)) : string.Empty;

                    if (string.IsNullOrEmpty(html))
                    {
                        html = GetContentUsingHttp(url);
                        if (!string.IsNullOrEmpty(html))
                        {
                            if (GMaps.Instance.UseUrlCache)
                            {
                                Cache.Instance.SaveContent(url, CacheType.UrlCache, html);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(html))
                    {
                        #region -- match versions --
                        Regex reg = new Regex(string.Format(@"https?://mts?\d.{0}/maps/vt\?lyrs=m@(\d*)", Server), RegexOptions.IgnoreCase);
                        Match mat = reg.Match(html);
                        if (mat.Success)
                        {
                            GroupCollection gc = mat.Groups;
                            int count = gc.Count;
                            if (count > 0)
                            {
                                string ver = string.Format("m@{0}", gc[1].Value);
                                string old = GMapProviders.GoogleMap.Version;

                                GMapProviders.GoogleMap.Version = ver;
                                GMapProviders.GoogleChinaMap.Version = ver;

                                string verh = string.Format("h@{0}", gc[1].Value);
                                string oldh = GMapProviders.GoogleChinaHybridMap.Version;

                                GMapProviders.GoogleChinaHybridMap.Version = verh;
                                GMapProviders.GoogleChinaHybridMap.Version = verh;
                            }
                        }

                        reg = new Regex(string.Format(@"https?://khms?\d.{0}/kh\?v=(\d*)", Server), RegexOptions.IgnoreCase);
                        mat = reg.Match(html);
                        if (mat.Success)
                        {
                            GroupCollection gc = mat.Groups;
                            int count = gc.Count;
                            if (count > 0)
                            {
                                string ver = gc[1].Value;
                                string old = GMapProviders.GoogleChinaSatelliteMap.Version;

                                GMapProviders.GoogleChinaSatelliteMap.Version = ver;
                                GMapProviders.GoogleChinaSatelliteMap.Version = ver;
                                GMapProviders.GoogleChinaSatelliteMap.Version = "s@" + ver;
                            }
                        }

                        reg = new Regex(string.Format(@"https?://mts?\d.{0}/maps/vt\?lyrs=t@(\d*),r@(\d*)", Server), RegexOptions.IgnoreCase);
                        mat = reg.Match(html);
                        if (mat.Success)
                        {
                            GroupCollection gc = mat.Groups;
                            int count = gc.Count;
                            if (count > 1)
                            {
                                string ver = string.Format("t@{0},r@{1}", gc[1].Value, gc[2].Value);
                                string old = GMapProviders.GoogleChinaSatelliteMap.Version;

                                GMapProviders.GoogleChinaSatelliteMap.Version = ver;
                                GMapProviders.GoogleChinaTerrainMap.Version = ver;
                            }
                        }
                        #endregion
                    }

                    init = true; // try it only once
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("TryCorrectGoogleVersions failed: " + ex.ToString());
                }
            }
        }

        internal void GetSecureWords(GPoint pos, out string sec1, out string sec2)
        {
            sec1 = string.Empty; // after &x=...
            sec2 = string.Empty; // after &zoom=...
            int seclen = (int)((pos.X * 3) + pos.Y) % 8;
            sec2 = SecureWord.Substring(0, seclen);
            if (pos.Y >= 10000 && pos.Y < 100000)
            {
                sec1 = Sec1;
            }
        }

        static readonly string Sec1 = "&s=";

        #region RoutingProvider Members

        #region -- internals --

        string MakeRouteUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool walkingMode)
        {
            string opt = walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr);
            return string.Format(CultureInfo.InvariantCulture, RouteUrlFormatPointLatLng, language, opt, start.Lat, start.Lng, end.Lat, end.Lng, Server);
        }

        string MakeRouteUrl(string start, string end, string language, bool avoidHighways, bool walkingMode)
        {
            string opt = walkingMode ? WalkingStr : (avoidHighways ? RouteWithoutHighwaysStr : RouteStr);
            return string.Format(RouteUrlFormatStr, language, opt, start.Replace(' ', '+'), end.Replace(' ', '+'), Server);
        }

        List<PointLatLng> GetRoutePoints(string url, int zoom, out string tooltipHtml, out int numLevel, out int zoomFactor)
        {
            List<PointLatLng> points = null;
            tooltipHtml = string.Empty;
            numLevel = -1;
            zoomFactor = -1;
            try
            {
                string route = GMaps.Instance.UseRouteCache ? Cache.Instance.GetContent(url, CacheType.RouteCache) : string.Empty;

                if (string.IsNullOrEmpty(route))
                {
                    route = GetContentUsingHttp(url);

                    if (!string.IsNullOrEmpty(route))
                    {
                        if (GMaps.Instance.UseRouteCache)
                        {
                            Cache.Instance.SaveContent(url, CacheType.RouteCache, route);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(route))
                {
                    #region -- title --
                    int tooltipEnd = 0;
                    {
                        int x = route.IndexOf("tooltipHtml:") + 13;
                        if (x >= 13)
                        {
                            tooltipEnd = route.IndexOf("\"", x + 1);
                            if (tooltipEnd > 0)
                            {
                                int l = tooltipEnd - x;
                                if (l > 0)
                                {
                                    tooltipHtml = route.Substring(x, l).Replace(@"\x26#160;", " ");
                                }
                            }
                        }
                    }
                    #endregion

                    #region -- points --
                    int pointsEnd = 0;
                    {
                        int x = route.IndexOf("points:", tooltipEnd >= 0 ? tooltipEnd : 0) + 8;
                        if (x >= 8)
                        {
                            pointsEnd = route.IndexOf("\"", x + 1);
                            if (pointsEnd > 0)
                            {
                                int l = pointsEnd - x;
                                if (l > 0)
                                {
                                    points = new List<PointLatLng>();
                                    DecodePointsInto(points, route.Substring(x, l));
                                }
                            }
                        }
                    }
                    #endregion

                    #region -- levels --
                    string levels = string.Empty;
                    int levelsEnd = 0;
                    {
                        int x = route.IndexOf("levels:", pointsEnd >= 0 ? pointsEnd : 0) + 8;
                        if (x >= 8)
                        {
                            levelsEnd = route.IndexOf("\"", x + 1);
                            if (levelsEnd > 0)
                            {
                                int l = levelsEnd - x;
                                if (l > 0)
                                {
                                    levels = route.Substring(x, l);
                                }
                            }
                        }
                    }
                    #endregion

                    #region -- numLevel --
                    int numLevelsEnd = 0;
                    {
                        int x = route.IndexOf("numLevels:", levelsEnd >= 0 ? levelsEnd : 0) + 10;
                        if (x >= 10)
                        {
                            numLevelsEnd = route.IndexOf(",", x);
                            if (numLevelsEnd > 0)
                            {
                                int l = numLevelsEnd - x;
                                if (l > 0)
                                {
                                    numLevel = int.Parse(route.Substring(x, l));
                                }
                            }
                        }
                    }
                    #endregion

                    #region -- zoomFactor --
                    {
                        int x = route.IndexOf("zoomFactor:", numLevelsEnd >= 0 ? numLevelsEnd : 0) + 11;
                        if (x >= 11)
                        {
                            int end = route.IndexOf("}", x);
                            if (end > 0)
                            {
                                int l = end - x;
                                if (l > 0)
                                {
                                    zoomFactor = int.Parse(route.Substring(x, l));
                                }
                            }
                        }
                    }
                    #endregion

                    #region -- trim point overload --
                    if (points != null && numLevel > 0 && !string.IsNullOrEmpty(levels))
                    {
                        if (points.Count - levels.Length > 0)
                        {
                            points.RemoveRange(levels.Length, points.Count - levels.Length);
                        }

                        //http://facstaff.unca.edu/mcmcclur/GoogleMaps/EncodePolyline/description.html
                        //
                        string allZlevels = "TSRPONMLKJIHGFEDCBA@?";
                        if (numLevel > allZlevels.Length)
                        {
                            numLevel = allZlevels.Length;
                        }

                        // used letters in levels string
                        string pLevels = allZlevels.Substring(allZlevels.Length - numLevel);

                        // remove useless points at zoom
                        {
                            List<PointLatLng> removedPoints = new List<PointLatLng>();

                            for (int i = 0; i < levels.Length; i++)
                            {
                                int zi = pLevels.IndexOf(levels[i]);
                                if (zi > 0)
                                {
                                    if (zi * numLevel > zoom)
                                    {
                                        removedPoints.Add(points[i]);
                                    }
                                }
                            }

                            foreach (var v in removedPoints)
                            {
                                points.Remove(v);
                            }
                            removedPoints.Clear();
                            removedPoints = null;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                points = null;
                Debug.WriteLine("GetRoutePoints: " + ex);
            }
            return points;
        }

        static readonly string RouteUrlFormatPointLatLng = "http://maps.{6}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2},{3}&daddr=@{4},{5}";
        static readonly string RouteUrlFormatStr = "http://maps.{4}/maps?f=q&output=dragdir&doflg=p&hl={0}{1}&q=&saddr=@{2}&daddr=@{3}";

        static readonly string WalkingStr = "&mra=ls&dirflg=w";
        static readonly string RouteWithoutHighwaysStr = "&mra=ls&dirflg=dh";
        static readonly string RouteStr = "&mra=ls&dirflg=d";

        #endregion

        #endregion

        #region GeocodingProvider Members

        public GeoCoderStatusCode GetPoints(string keywords, out List<PointLatLng> pointList)
        {
            return GetLatLngFromGeocoderUrl(MakeGeocoderUrl(keywords, LanguageStr), out pointList);
        }

        public PointLatLng? GetPoint(string keywords, out GeoCoderStatusCode status)
        {
            status = GetPoints(keywords, out List<PointLatLng> pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (PointLatLng?)null;
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="placemark"></param>
        /// <param name="pointList"></param>
        /// <returns></returns>
        public GeoCoderStatusCode GetPoints(Placemark placemark, out List<PointLatLng> pointList)
        {
            throw new NotImplementedException("use GetPoints(string keywords...");
        }

        /// <summary>
        /// NotImplemented
        /// </summary>
        /// <param name="placemark"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public PointLatLng? GetPoint(Placemark placemark, out GeoCoderStatusCode status)
        {
            throw new NotImplementedException("use GetPoint(string keywords...");
        }

        public GeoCoderStatusCode GetPlacemarks(PointLatLng location, out List<Placemark> placemarkList)
        {
            return GetPlacemarkFromReverseGeocoderUrl(MakeReverseGeocoderUrl(location, LanguageStr), out placemarkList);
        }

        public Placemark? GetPlacemark(PointLatLng location, out GeoCoderStatusCode status)
        {
            status = GetPlacemarks(location, out List<Placemark> pointList);
            return pointList != null && pointList.Count > 0 ? pointList[0] : (Placemark?)null;
        }

        #region -- internals --

        // The Coogle Geocoding API: http://tinyurl.com/cdlj889

        string MakeGeocoderUrl(string keywords, string language)
        {
            return string.Format(CultureInfo.InvariantCulture, GeocoderUrlFormat, ServerAPIs, Uri.EscapeDataString(keywords).Replace(' ', '+'), language);
        }

        string MakeReverseGeocoderUrl(PointLatLng pt, string language)
        {
            return string.Format(CultureInfo.InvariantCulture, ReverseGeocoderUrlFormat, ServerAPIs, pt.Lat, pt.Lng, language);
        }

        GeoCoderStatusCode GetLatLngFromGeocoderUrl(string url, out List<PointLatLng> pointList)
        {
            var status = GeoCoderStatusCode.Unknow;
            pointList = null;

            try
            {
                string geo = GMaps.Instance.UseGeocoderCache ? Cache.Instance.GetContent(url, CacheType.GeocoderCache) : string.Empty;

                bool cache = false;

                if (string.IsNullOrEmpty(geo))
                {
                    string urls = url;

                    // Must provide either API key or Maps for Work credentials.
                    if (!string.IsNullOrEmpty(ClientId))
                    {
                        urls = GetSignedUri(url);
                    }
                    else if (!string.IsNullOrEmpty(ApiKey))
                    {
                        urls += "&key=" + ApiKey;
                    }

                    geo = GetContentUsingHttp(urls);

                    if (!string.IsNullOrEmpty(geo))
                    {
                        cache = true;
                    }
                }

                if (!string.IsNullOrEmpty(geo))
                {
                    if (geo.StartsWith("<?xml"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(geo);

                        XmlNode nn = doc.SelectSingleNode("//status");
                        if (nn != null)
                        {
                            if (nn.InnerText != "OK")
                            {
                                Debug.WriteLine("GetLatLngFromGeocoderUrl: " + nn.InnerText);
                            }
                            else
                            {
                                status = GeoCoderStatusCode.G_GEO_SUCCESS;

                                if (cache && GMaps.Instance.UseGeocoderCache)
                                {
                                    Cache.Instance.SaveContent(url, CacheType.GeocoderCache, geo);
                                }

                                pointList = new List<PointLatLng>();

                                XmlNodeList l = doc.SelectNodes("//result");
                                if (l != null)
                                {
                                    foreach (XmlNode n in l)
                                    {
                                        nn = n.SelectSingleNode("geometry/location/lat");
                                        if (nn != null)
                                        {
                                            double lat = double.Parse(nn.InnerText, CultureInfo.InvariantCulture);

                                            nn = n.SelectSingleNode("geometry/location/lng");
                                            if (nn != null)
                                            {
                                                double lng = double.Parse(nn.InnerText, CultureInfo.InvariantCulture);
                                                pointList.Add(new PointLatLng(lat, lng));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status = GeoCoderStatusCode.ExceptionInCode;
                Debug.WriteLine("GetLatLngFromGeocoderUrl: " + ex);
            }

            return status;
        }

        GeoCoderStatusCode GetPlacemarkFromReverseGeocoderUrl(string url, out List<Placemark> placemarkList)
        {
            GeoCoderStatusCode status = GeoCoderStatusCode.Unknow;
            placemarkList = null;

            try
            {
                string reverse = GMaps.Instance.UsePlacemarkCache ? Cache.Instance.GetContent(url, CacheType.PlacemarkCache) : string.Empty;

                bool cache = false;

                if (string.IsNullOrEmpty(reverse))
                {
                    string urls = url;

                    // Must provide either API key or Maps for Work credentials.
                    if (!string.IsNullOrEmpty(ClientId))
                    {
                        urls = GetSignedUri(url);
                    }
                    else if (!string.IsNullOrEmpty(ApiKey))
                    {
                        urls += "&key=" + ApiKey;
                    }

                    reverse = GetContentUsingHttp(urls);

                    if (!string.IsNullOrEmpty(reverse))
                    {
                        cache = true;
                    }
                }

                if (!string.IsNullOrEmpty(reverse))
                {
                    if (reverse.StartsWith("<?xml"))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(reverse);

                        XmlNode nn = doc.SelectSingleNode("//status");
                        if (nn != null)
                        {
                            if (nn.InnerText != "OK")
                            {
                                Debug.WriteLine("GetPlacemarkFromReverseGeocoderUrl: " + nn.InnerText);
                            }
                            else
                            {
                                status = GeoCoderStatusCode.G_GEO_SUCCESS;

                                if (cache && GMaps.Instance.UsePlacemarkCache)
                                {
                                    Cache.Instance.SaveContent(url, CacheType.PlacemarkCache, reverse);
                                }

                                placemarkList = new List<Placemark>();

                                #region -- placemarks --
                                XmlNodeList l = doc.SelectNodes("//result");
                                if (l != null)
                                {
                                    foreach (XmlNode n in l)
                                    {
                                        Debug.WriteLine("---------------------");

                                        nn = n.SelectSingleNode("formatted_address");
                                        if (nn != null)
                                        {
                                            var ret = new Placemark(nn.InnerText);

                                            Debug.WriteLine("formatted_address: [" + nn.InnerText + "]");

                                            nn = n.SelectSingleNode("type");
                                            if (nn != null)
                                            {
                                                Debug.WriteLine("type: " + nn.InnerText);
                                            }

                                            // TODO: fill Placemark details

                                            XmlNodeList acl = n.SelectNodes("address_component");
                                            foreach (XmlNode ac in acl)
                                            {
                                                nn = ac.SelectSingleNode("type");
                                                if (nn != null)
                                                {
                                                    var type = nn.InnerText;
                                                    Debug.Write(" - [" + type + "], ");

                                                    nn = ac.SelectSingleNode("long_name");
                                                    if (nn != null)
                                                    {
                                                        Debug.WriteLine("long_name: [" + nn.InnerText + "]");

                                                        switch (type)
                                                        {
                                                            case "street_address":
                                                                {
                                                                    ret.StreetNumber = nn.InnerText;
                                                                }
                                                                break;

                                                            case "route":
                                                                {
                                                                    ret.ThoroughfareName = nn.InnerText;
                                                                }
                                                                break;

                                                            case "postal_code":
                                                                {
                                                                    ret.PostalCodeNumber = nn.InnerText;
                                                                }
                                                                break;

                                                            case "country":
                                                                {
                                                                    ret.CountryName = nn.InnerText;
                                                                }
                                                                break;

                                                            case "locality":
                                                                {
                                                                    ret.LocalityName = nn.InnerText;
                                                                }
                                                                break;

                                                            case "administrative_area_level_2":
                                                                {
                                                                    ret.DistrictName = nn.InnerText;
                                                                }
                                                                break;

                                                            case "administrative_area_level_1":
                                                                {
                                                                    ret.AdministrativeAreaName = nn.InnerText;
                                                                }
                                                                break;

                                                            case "administrative_area_level_3":
                                                                {
                                                                    ret.SubAdministrativeAreaName = nn.InnerText;
                                                                }
                                                                break;

                                                            case "neighborhood":
                                                                {
                                                                    ret.Neighborhood = nn.InnerText;
                                                                }
                                                                break;
                                                        }
                                                    }
                                                }
                                            }

                                            placemarkList.Add(ret);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                status = GeoCoderStatusCode.ExceptionInCode;
                placemarkList = null;
                Debug.WriteLine("GetPlacemarkReverseGeocoderUrl: " + ex.ToString());
            }

            return status;
        }

        // Note: Use of API Key requires https's queries.
        static readonly string ReverseGeocoderUrlFormat = "https://maps.{0}/maps/api/geocode/xml?latlng={1},{2}&language={3}&sensor=false";
        static readonly string GeocoderUrlFormat = "https://maps.{0}/maps/api/geocode/xml?address={1}&language={2}&sensor=false";

        #endregion

        #region DirectionsProvider Members

        #region -- internals --

        // The Coogle Directions API: http://tinyurl.com/6vv4cac

        string MakeDirectionsUrl(PointLatLng start, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6
            string mt = "&units=" + (metric ? "metric" : "imperial");     // 7
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

            return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatPoint, start.Lat, start.Lng, end.Lat, end.Lng, sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs);
        }

        string MakeDirectionsUrl(string start, string end, string language, bool avoidHighways, bool walkingMode, bool avoidTolls, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 4
            string mt = "&units=" + (metric ? "metric" : "imperial");     // 5
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 6

            return string.Format(DirectionUrlFormatStr, start.Replace(' ', '+'), end.Replace(' ', '+'), sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs);
        }

        string MakeDirectionsUrl(PointLatLng start, IEnumerable<PointLatLng> wayPoints, PointLatLng end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6
            string mt = "&units=" + (metric ? "metric" : "imperial"); // 7
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

            string wpLatLng = string.Empty;
            int i = 0;
            foreach (var wp in wayPoints)
            {
                wpLatLng += string.Format(CultureInfo.InvariantCulture, i++ == 0 ? "{0},{1}" : "|{0},{1}", wp.Lat, wp.Lng);
            }

            return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatWaypoint, start.Lat, start.Lng, wpLatLng, sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs, end.Lat, end.Lng);
        }

        string MakeDirectionsUrl(string start, IEnumerable<string> wayPoints, string end, string language, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric)
        {
            string av = (avoidHighways ? "&avoid=highways" : string.Empty) + (avoidTolls ? "&avoid=tolls" : string.Empty); // 6
            string mt = "&units=" + (metric ? "metric" : "imperial"); // 7
            string wk = "&mode=" + (walkingMode ? "walking" : "driving"); // 8

            string wpLatLng = string.Empty;
            int i = 0;
            foreach (var wp in wayPoints)
            {
                wpLatLng += string.Format(CultureInfo.InvariantCulture, i++ == 0 ? "{0}" : "|{0}", wp.Replace(' ', '+'));
            }

            return string.Format(CultureInfo.InvariantCulture, DirectionUrlFormatWaypointStr, start.Replace(' ', '+'), wpLatLng, sensor.ToString().ToLower(), language, av, mt, wk, ServerAPIs, end.Replace(' ', '+'));
        }


        static void DecodePointsInto(List<PointLatLng> path, string encodedPath)
        {
            // https://github.com/googlemaps/google-maps-services-java/blob/master/src/main/java/com/google/maps/internal/PolylineEncoding.java
            int len = encodedPath.Length;
            int index = 0;
            int lat = 0;
            int lng = 0;
            while (index < len)
            {
                int result = 1;
                int shift = 0;
                int b;
                do
                {
                    b = encodedPath[index++] - 63 - 1;
                    result += b << shift;
                    shift += 5;
                } while (b >= 0x1f && index < len);
                lat += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                result = 1;
                shift = 0;

                if (index < len)
                {
                    do
                    {
                        b = encodedPath[index++] - 63 - 1;
                        result += b << shift;
                        shift += 5;
                    } while (b >= 0x1f && index < len);
                    lng += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
                }

                path.Add(new PointLatLng(lat * 1e-5, lng * 1e-5));
            }
        }

        static readonly string DirectionUrlFormatStr = "http://maps.{7}/maps/api/directions/xml?origin={0}&destination={1}&sensor={2}&language={3}{4}{5}{6}";
        static readonly string DirectionUrlFormatPoint = "http://maps.{9}/maps/api/directions/xml?origin={0},{1}&destination={2},{3}&sensor={4}&language={5}{6}{7}{8}";
        static readonly string DirectionUrlFormatWaypoint = "http://maps.{8}/maps/api/directions/xml?origin={0},{1}&waypoints={2}&destination={9},{10}&sensor={3}&language={4}{5}{6}{7}";
        static readonly string DirectionUrlFormatWaypointStr = "http://maps.{7}/maps/api/directions/xml?origin={0}&waypoints={1}&destination={8}&sensor={2}&language={3}{4}{5}{6}";

        #endregion

        #endregion

        #region -- Maps API for Work --
        /// <summary>
        /// https://developers.google.com/maps/documentation/business/webservices/auth#how_do_i_get_my_signing_key
        /// To access the special features of the Google Maps API for Work you must provide a client ID
        /// when accessing any of the API libraries or services.
        /// When registering for Google Google Maps API for Work you will receive this client ID from Enterprise Support.
        /// All client IDs begin with a gme- prefix. Your client ID is passed as the value of the client parameter.
        /// Generally, you should store your private key someplace safe and read them into your code
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="privateKey"></param>
        public void SetEnterpriseCredentials(string clientId, string privateKey)
        {
            privateKey = privateKey.Replace("-", "+").Replace("_", "/");
            _privateKeyBytes = Convert.FromBase64String(privateKey);
            _clientId = clientId;
        }
        private byte[] _privateKeyBytes;

        private string _clientId = string.Empty;

        /// <summary>
        /// Your client ID. To access the special features of the Google Maps API for Work
        /// you must provide a client ID when accessing any of the API libraries or services.
        /// When registering for Google Google Maps API for Work you will receive this client ID
        /// from Enterprise Support. All client IDs begin with a gme- prefix.
        /// </summary>
        public string ClientId
        {
            get
            {
                return _clientId;
            }
        }

        string GetSignedUri(Uri uri)
        {
            var builder = new UriBuilder(uri);
            builder.Query = builder.Query.Substring(1) + "&client=" + _clientId;
            uri = builder.Uri;
            string signature = GetSignature(uri);

            return uri.Scheme + "://" + uri.Host + uri.LocalPath + uri.Query + "&signature=" + signature;
        }

        string GetSignedUri(string url)
        {
            return GetSignedUri(new Uri(url));
        }

        string GetSignature(Uri uri)
        {
            byte[] encodedPathQuery = Encoding.ASCII.GetBytes(uri.LocalPath + uri.Query);
            var hashAlgorithm = new HMACSHA1(_privateKeyBytes);
            byte[] hashed = hashAlgorithm.ComputeHash(encodedPathQuery);
            return Convert.ToBase64String(hashed).Replace("+", "-").Replace("/", "_");
        }
        #endregion
    }

    /// <summary>
    /// GoogleMap provider
    /// </summary>
    public class GoogleMapProvider : GoogleMapProviderBase
    {
        public static readonly GoogleMapProvider Instance;

        GoogleMapProvider()
        {
        }

        static GoogleMapProvider()
        {
            Instance = new GoogleMapProvider();
        }

        public string Version = "m@333000000";

        #region GMapProvider Members

        readonly Guid id = new Guid("D7287DA0-A7FF-405F-8166-B6BAF26D066C");
        public override Guid Id
        {
            get
            {
                return id;
            }
        }

        readonly string name = "GoogleMap";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            string url = MakeTileImageUrl(pos, zoom, LanguageStr);

            return GetTileImageUsingHttp(url);
        }

        #endregion

        string MakeTileImageUrl(GPoint pos, int zoom, string language)
        {
            string sec1 = string.Empty; // after &x=...
            string sec2 = string.Empty; // after &zoom=...
            GetSecureWords(pos, out sec1, out sec2);

            return string.Format(UrlFormat, UrlFormatServer, GetServerNum(pos, 4), UrlFormatRequest, Version, language, pos.X, sec1, pos.Y, zoom, sec2, Server);
        }

        static readonly string UrlFormatServer = "mt";
        static readonly string UrlFormatRequest = "vt";
        static readonly string UrlFormat = "http://{0}{1}.{10}/maps/{2}/lyrs={3}&hl={4}&x={5}{6}&y={7}&z={8}&s={9}";
    }
}
