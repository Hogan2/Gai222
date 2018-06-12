
namespace GMap.NET
{
   using System;
   using System.Collections.Generic;
   using System.Diagnostics;
   using System.Globalization;
   using System.IO;
   using System.Net;
   using System.Text;
   using System.Threading;
   using System.Xml;
   using System.Xml.Serialization;
   using GMap.NET.CacheProviders;
   using GMap.NET.Internals;
   using GMap.NET.MapProviders;
    using System.Reflection;

   /// <summary>
   /// maps manager
   /// </summary>
   public class GMaps : Singleton<GMaps>
   {
      /// <summary>
      /// tile access mode
      /// </summary>
      public AccessMode Mode = AccessMode.CacheOnly;

      /// <summary>
      /// is map ussing cache for routing
      /// </summary>
      public bool UseRouteCache = false;

      /// <summary>
      /// is map using cache for geocoder
      /// </summary>
      public bool UseGeocoderCache = false;

      /// <summary>
      /// is map using cache for directions
      /// </summary>
      public bool UseDirectionsCache = false;

      /// <summary>
      /// is map using cache for placemarks
      /// </summary>
      public bool UsePlacemarkCache = false;

      /// <summary>
      /// is map ussing cache for other url
      /// </summary>
      public bool UseUrlCache = false;

      /// <summary>
      /// is map using memory cache for tiles
      /// </summary>
      public bool UseMemoryCache = true;

      /// <summary>
      /// primary cache provider, by default: ultra fast SQLite!
      /// </summary>
      public PureImageCache PrimaryCache
      {
         get
         {
            return Cache.Instance.ImageCache;
         }
         set
         {
            Cache.Instance.ImageCache = value;
         }
      }

      /// <summary>
      /// secondary cache provider, by default: none,
      /// use it if you have server in your local network
      /// </summary>
      public PureImageCache SecondaryCache
      {
         get
         {
            return Cache.Instance.ImageCacheSecond;
         }
         set
         {
            Cache.Instance.ImageCacheSecond = value;
         }
      }

      /// <summary>
      /// MemoryCache provider
      /// </summary>
      public readonly MemoryCache MemoryCache = new MemoryCache();

      /// <summary>
      /// load tiles in random sequence
      /// </summary>
      public bool ShuffleTilesOnLoad = false;

      /// <summary>
      /// tile queue to cache
      /// </summary>
      readonly Queue<CacheQueueItem> tileCacheQueue = new Queue<CacheQueueItem>();

      bool? isRunningOnMono;

      /// <summary>
      /// return true if running on mono
      /// </summary>
      /// <returns></returns>
      public bool IsRunningOnMono
      {
         get
         {
            if(!isRunningOnMono.HasValue)
            {
               try
               {
                  isRunningOnMono = (Type.GetType("Mono.Runtime") != null);
                  return isRunningOnMono.Value;
               }
               catch
               {
               }
            }
            else
            {
               return isRunningOnMono.Value;
            }
            return false;
         }
      }

      /// <summary>
      /// cache worker
      /// </summary>
      Thread CacheEngine;

      internal readonly AutoResetEvent WaitForCache = new AutoResetEvent(false);


      static GMaps()
      {
          if (GMapProvider.TileImageProxy == null)
          {
              try
              {
                  AppDomain d = AppDomain.CurrentDomain;
                  var AssembliesLoaded = d.GetAssemblies();

                  Assembly l = null;

                  foreach (var a in AssembliesLoaded)
                  {
                      if (a.FullName.Contains("GMap.NET.WindowsForms") || a.FullName.Contains("GMap.NET.WindowsPresentation"))
                      {
                          l = a;
                          break;
                      }
                  }

                  if (l == null)
                  {
                      var jj = Assembly.GetExecutingAssembly().Location;
                      var hh = Path.GetDirectoryName(jj);
                      var f1 = hh + Path.DirectorySeparatorChar + "GMap.NET.WindowsForms.dll";
                      var f2 = hh + Path.DirectorySeparatorChar + "GMap.NET.WindowsPresentation.dll";
                      if (File.Exists(f1))
                      {
                          l = Assembly.LoadFile(f1);
                      }
                      else if (File.Exists(f2))
                      {
                          l = Assembly.LoadFile(f2);
                      }
                  }

                  if (l != null)
                  {
                      Type t = null;

                      if (l.FullName.Contains("GMap.NET.WindowsForms"))
                      {
                          t = l.GetType("GMap.NET.WindowsForms.GMapImageProxy");
                      }
                      else if (l.FullName.Contains("GMap.NET.WindowsPresentation"))
                      {
                          t = l.GetType("GMap.NET.WindowsPresentation.GMapImageProxy");
                      }

                      if (t != null)
                      {
                          t.InvokeMember("Enable", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
                      }
                  }
              }
              catch (Exception ex)
              {
                  Debug.WriteLine("GMaps, try set TileImageProxy failed: " + ex.Message);
              }
          }
      }


      public GMaps()
      {
         #region singleton check
         if(Instance != null)
         {
            throw (new Exception("You have tried to create a new singleton class where you should have instanced it. Replace your \"new class()\" with \"class.Instance\""));
         }
         #endregion

         ServicePointManager.DefaultConnectionLimit = 5;
      }


      /// <summary>
      /// triggers dynamic sqlite loading, 
      /// call this before you use sqlite for other reasons than caching maps
      /// </summary>
      public void SQLitePing()
      {
         SQLitePureImageCache.Ping();
      }


      #region -- Stuff --


      /// <summary>
      /// exports current map cache to GMDB file
      /// if file exsist only new records will be added
      /// otherwise file will be created and all data exported
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public bool ExportToGMDB(string file)
      {
         if(PrimaryCache is SQLitePureImageCache)
         {
            StringBuilder db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
            db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);

            return SQLitePureImageCache.ExportMapDataToDB(db.ToString(), file);
         }
         return false;
      }

      /// <summary>
      /// imports GMDB file to current map cache
      /// only new records will be added
      /// </summary>
      /// <param name="file"></param>
      /// <returns></returns>
      public bool ImportFromGMDB(string file)
      {
         if(PrimaryCache is GMap.NET.CacheProviders.SQLitePureImageCache)
         {
            StringBuilder db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
            db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);

            return SQLitePureImageCache.ExportMapDataToDB(file, db.ToString());
         }
         return false;
      }

      /// <summary>
      /// optimizes map database, *.gmdb
      /// </summary>
      /// <param name="file">database file name or null to optimize current user db</param>
      /// <returns></returns>
      public bool OptimizeMapDb(string file)
      {
         if(PrimaryCache is GMap.NET.CacheProviders.SQLitePureImageCache)
         {
            if(string.IsNullOrEmpty(file))
            {
               StringBuilder db = new StringBuilder((PrimaryCache as SQLitePureImageCache).GtileCache);
               db.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}Data.gmdb", GMapProvider.LanguageStr, Path.DirectorySeparatorChar);

               return SQLitePureImageCache.VacuumDb(db.ToString());
            }
            else
            {
               return SQLitePureImageCache.VacuumDb(file);
            }
         }

         return false;
      }

      /// <summary>
      /// enqueueens tile to cache
      /// </summary>
      /// <param name="task"></param>
      void EnqueueCacheTask(CacheQueueItem task)
      {
         lock(tileCacheQueue)
         {
            if(!tileCacheQueue.Contains(task))
            {
               Debug.WriteLine("EnqueueCacheTask: " + task);

               tileCacheQueue.Enqueue(task);

               if(CacheEngine != null && CacheEngine.IsAlive)
               {
                  WaitForCache.Set();
               }
               else if(CacheEngine == null || CacheEngine.ThreadState == System.Threading.ThreadState.Stopped || CacheEngine.ThreadState == System.Threading.ThreadState.Unstarted)
               {
                  CacheEngine = null;
                  CacheEngine = new Thread(new ThreadStart(CacheEngineLoop));
                  CacheEngine.Name = "CacheEngine";
                  CacheEngine.IsBackground = false;
                  CacheEngine.Priority = ThreadPriority.Lowest;

                  abortCacheLoop = false;
                  CacheEngine.Start();
               }
            }
         }
      }

      volatile bool abortCacheLoop = false;
      internal volatile bool noMapInstances = false;

      public TileCacheComplete OnTileCacheComplete;
      public TileCacheStart OnTileCacheStart;
      public TileCacheProgress OnTileCacheProgress;

      /// <summary>
      /// immediately stops background tile caching, call it if you want fast exit the process
      /// </summary>
      public void CancelTileCaching()
      {
         Debug.WriteLine("CancelTileCaching...");

         abortCacheLoop = true;
         lock(tileCacheQueue)
         {
            tileCacheQueue.Clear();
            WaitForCache.Set();
         }
      }

      int readingCache = 0;      
      volatile bool cacheOnIdleRead = true;

      /// <summary>
      /// delays writing tiles to cache while performing reads
      /// </summary>
      public bool CacheOnIdleRead
      {
          get
          {
              return cacheOnIdleRead;
          }
          set
          {
              cacheOnIdleRead = value;
          }
      }

      volatile bool boostCacheEngine = false;

      /// <summary>
      /// disables delay between saving tiles into database/cache
      /// </summary>
      public bool BoostCacheEngine
      {
          get
          {
              return boostCacheEngine;
          }
          set
          {
              boostCacheEngine = value;
          }
      }

      /// <summary>
      /// live for cache ;}
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      void CacheEngineLoop()
      {
         Debug.WriteLine("CacheEngine: start");
         int left = 0;

         if(OnTileCacheStart != null)
         {
            OnTileCacheStart();
         }

         bool startEvent = false;

         while(!abortCacheLoop)
         {
            try
            {
               CacheQueueItem? task = null;

               lock(tileCacheQueue)
               {
                  left = tileCacheQueue.Count;
                  if(left > 0)
                  {
                     task = tileCacheQueue.Dequeue();
                  }
               }

               if(task.HasValue)
               {
                  if(startEvent)
                  {
                     startEvent = false;

                     if(OnTileCacheStart != null)
                     {
                        OnTileCacheStart();
                     }
                  }

                  if(OnTileCacheProgress != null)
                  {
                     OnTileCacheProgress(left);
                  }

                  #region -- save --
                  // check if stream wasn't disposed somehow
                  if(task.Value.Img != null)
                  {
                     Debug.WriteLine("CacheEngine[" + left + "]: storing tile " + task.Value + ", " + task.Value.Img.Length / 1024 + "kB...");

                     if((task.Value.CacheType & CacheUsage.First) == CacheUsage.First && PrimaryCache != null)
                     {
                        if(cacheOnIdleRead)
                        {
                           while(Interlocked.Decrement(ref readingCache) > 0)
                           {
                              Thread.Sleep(1000);
                           }
                        }
                        PrimaryCache.PutImageToCache(task.Value.Img, task.Value.Tile.Type, task.Value.Tile.Pos, task.Value.Tile.Zoom);
                     }

                     if((task.Value.CacheType & CacheUsage.Second) == CacheUsage.Second && SecondaryCache != null)
                     {
                        if(cacheOnIdleRead)
                        {
                           while(Interlocked.Decrement(ref readingCache) > 0)
                           {
                              Thread.Sleep(1000);
                           }
                        }
                        SecondaryCache.PutImageToCache(task.Value.Img, task.Value.Tile.Type, task.Value.Tile.Pos, task.Value.Tile.Zoom);
                     }

                     task.Value.Clear();

                     if(!boostCacheEngine)
                     {
                        Thread.Sleep(333);
                     }
                  }
                  else
                  {
                     Debug.WriteLine("CacheEngineLoop: skip, tile disposed to early -> " + task.Value);
                  }
                  task = null;
                  #endregion
               }
               else
               {
                  if(!startEvent)
                  {
                     startEvent = true;

                     if(OnTileCacheComplete != null)
                     {
                        OnTileCacheComplete();
                     }
                  }

                  if(abortCacheLoop || noMapInstances || !WaitForCache.WaitOne(33333, false) || noMapInstances)
                  {
                     break;
                  }
               }
            }
            catch(AbandonedMutexException)
            {
               break;
            }
            catch(Exception ex)
            {
               Debug.WriteLine("CacheEngineLoop: " + ex.ToString());
            }
         }
         Debug.WriteLine("CacheEngine: stop");

         if(!startEvent)
         {
            if(OnTileCacheComplete != null)
            {
               OnTileCacheComplete();
            }
         }
      }

      class StringWriterExt : StringWriter
      {
         public StringWriterExt(IFormatProvider info)
            : base(info)
         {

         }

         public override Encoding Encoding
         {
            get
            {
               return Encoding.UTF8;
            }
         }
      }

      #endregion

      /// <summary>
      /// gets image from tile server
      /// </summary>
      /// <param name="provider"></param>
      /// <param name="pos"></param>
      /// <param name="zoom"></param>
      /// <returns></returns>
      public PureImage GetImageFrom(GMapProvider provider, GPoint pos, int zoom, out Exception result)
      {
         PureImage ret = null;
         result = null;

         try
         {
            var rtile = new RawTile(provider.DbId, pos, zoom);

            // let't check memmory first
            if(UseMemoryCache)
            {
               var m = MemoryCache.GetTileFromMemoryCache(rtile);
               if(m != null)
               {
                  if(GMapProvider.TileImageProxy != null)
                  {
                     ret = GMapProvider.TileImageProxy.FromArray(m);
                     if(ret == null)
                     {
                        m = null;
                     }
                  }
               }
            }

            if(ret == null)
            {
               if(Mode != AccessMode.ServerOnly && !provider.BypassCache)
               {
                  if(PrimaryCache != null)
                  {
                     // hold writer for 5s
                     if(cacheOnIdleRead)
                     {
                        Interlocked.Exchange(ref readingCache, 5);
                     }

                     ret = PrimaryCache.GetImageFromCache(provider.DbId, pos, zoom);
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }
                        return ret;
                     }
                  }

                  if(SecondaryCache != null)
                  {
                     // hold writer for 5s
                     if(cacheOnIdleRead)
                     {
                        Interlocked.Exchange(ref readingCache, 5);
                     }

                     ret = SecondaryCache.GetImageFromCache(provider.DbId, pos, zoom);
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }
                        EnqueueCacheTask(new CacheQueueItem(rtile, ret.Data.GetBuffer(), CacheUsage.First));
                        return ret;
                     }
                  }
               }

               if(Mode != AccessMode.CacheOnly)
               {
                  ret = provider.GetTileImage(pos, zoom);
                  {
                     // Enqueue Cache
                     if(ret != null)
                     {
                        if(UseMemoryCache)
                        {
                           MemoryCache.AddTileToMemoryCache(rtile, ret.Data.GetBuffer());
                        }

                        if (Mode != AccessMode.ServerOnly && !provider.BypassCache)
                        {
                           EnqueueCacheTask(new CacheQueueItem(rtile, ret.Data.GetBuffer(), CacheUsage.Both));
                        }
                     }
                  }
               }
               else
               {
                  result = noDataException;
               }
            }
         }
         catch(Exception ex)
         {
            result = ex;
            ret = null;
            Debug.WriteLine("GetImageFrom: " + ex.ToString());
         }

         return ret;
      }

      readonly Exception noDataException = new Exception("No data in local tile cache...");

      TileHttpHost host;

      /// <summary>
      /// turns on tile host
      /// </summary>
      /// <param name="port"></param>
      public void EnableTileHost(int port)
      {
         if(host == null)
         {
            host = new TileHttpHost();
         }
         host.Start(port);
      }

      /// <summary>
      /// turns off tile host
      /// </summary>
      /// <param name="port"></param>
      public void DisableTileHost()
      {
          if (host != null)
          {
              host.Stop();
          }
      }
   }
}
