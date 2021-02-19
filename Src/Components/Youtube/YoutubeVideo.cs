using Htmlilka;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Csml {
    public class VideoPlayerBehaviour : Behaviour {
        public VideoPlayerBehaviour(string code, float aspect, bool showControls, bool autoPlay, bool loop, bool sound, KeyValuePair<int, string>[] mips, bool poster)
            : base("VideoPlayer", code, aspect, showControls, autoPlay, loop, sound, mips, poster) { }
    };

    [CacheConfig("Videos", true)]
    public class YoutubeVideoCache : Cache<YoutubeVideoCache> {

        public Dictionary<int, string> Mips;
    }


    public class YoutubeVideo {
        readonly string Code;
        readonly float Aspect;
        readonly Task<YoutubeVideoCache> CacheAsync;
        public Image Image;
        private readonly bool isPosterDefinedByUser;

        public YoutubeVideoCache Cache => CacheAsync.Result;

        public YoutubeVideo(string code, Image image = null, float aspect = 16f / 9f) {
            Code = code;
            Aspect = aspect;
            CacheAsync = GetCacheAsync();
            Image = image;
            isPosterDefinedByUser = Image != null;
        }

        public Player GetPlayer() {
            return new Player(this, Image);
        }

        public string GetImagePath() {
            var imagePath = Path.Combine(Cache.Directory, $"{Code}.jpg");
            if(File.Exists(imagePath)) { return imagePath; }
            using var client = new WebClient();
            client.DownloadFile(new Uri($"http://i3.ytimg.com/vi/{Code}/maxresdefault.jpg"), imagePath);
            return imagePath;
        }

        private async Task DownloadVideoAsync(YoutubeClient youTube, MuxedStreamInfo streamInfo, YoutubeVideoCache cache) {
            Utils.CreateDirectory(cache.Directory);
            var path = Path.Combine(cache.Directory, FileNameFormatter(cache, streamInfo));
            if(File.Exists(path)) { return; }
            Console.WriteLine($"{Code}: start download {path}");
            await youTube.Videos.Streams.DownloadAsync(streamInfo, path);
            Console.WriteLine($"{Code}: download {path} done.");
        }

        IEnumerable<Task> videoDownloaders;
        private async Task<YoutubeVideoCache> GetCacheAsync() {
            var result = YoutubeVideoCache.Load(Code);
            if(result != null) return result;
            try {
                Console.WriteLine($"{Code}:Begin");
                var youTube = new YoutubeClient();
                var streamManifest = await youTube.Videos.Streams.GetManifestAsync(Code);
                var streamInfo = streamManifest.GetMuxed();
                Console.WriteLine($"{Code}:Got videos list");
                result = new YoutubeVideoCache { Hash = Code };

                var videosToDownload = streamInfo;

                videoDownloaders = videosToDownload
                    .Select(x => DownloadVideoAsync(youTube, x, result));

                await Task.WhenAll(videoDownloaders);



                result.Mips = new Dictionary<int, string>(videosToDownload
                    .Select(x => new KeyValuePair<int, string>(x.Resolution.Height, FileNameFormatter(result, x)))
                    .OrderBy(x => x.Key));
                result.Save();
            } catch(Exception ex) {
                Log.Error.Here(ex.Message);
                Environment.Exit(1);
            }
            return result;
        }

        private static string FileNameFormatter(YoutubeVideoCache cache, MuxedStreamInfo streamInfo) =>
            $"{cache.Hash}_{streamInfo.Resolution.Height}.{streamInfo.Container}";

        public class Player : Element<Player>, IImage {
            readonly YoutubeVideo YoutubeVideo;
            private Image _image;
            public ImageCache GetImageCache() => _image.GetImageCache();

            public bool ShowControls { get; set; } = true;
            public bool AutoPlay { get; set; } = false;
            public bool Loop { get; set; } = false;
            public bool Sound { get; set; } = true;


            private string _imageSource;
            public Player(YoutubeVideo youtubeVideo, Image image) {
                _image = image;
                YoutubeVideo = youtubeVideo;
            }

            //public Player SetCustomImageSource(string filePath) {

            //    if(!File.Exists(_imageSource)) {
            //        Log.Error.OnObject(this, $"File {filePath} not found");
            //        _imageSource = null;
            //    }                
            //    return this;
            //}

            private string GetImageSource() {
                if(string.IsNullOrEmpty(_imageSource)) {
                    _imageSource = YoutubeVideo.GetImagePath();
                }
                return _imageSource;
            }

            public Player Configure(Action<Player> action) {
                action(this);
                return this;
            }
            public Player ConfigureAsBackgroundVideo() {
                ShowControls = false;
                AutoPlay = true;
                Loop = true;
                Sound = false;
                return this;
            }

            public override Node Generate(Context context) {
                var cache = YoutubeVideo.Cache;
                var hasPoster = YoutubeVideo.isPosterDefinedByUser;
                if(hasPoster) {
                    var image = GetImage().Generate(context);
                    return new Tag("div").Add(GetImage().Generate(context))
                    .AddClasses("VideoPlayer")
                    .Add(new VideoPlayerBehaviour(
                        YoutubeVideo.Code,
                        YoutubeVideo.Aspect,
                        ShowControls,
                        AutoPlay,
                        Loop,
                        Sound,
                        cache.Mips.Select(x => new KeyValuePair<int, string>(x.Key, cache.GetFileUri(x.Value).ToString())).ToArray(),
                        YoutubeVideo.isPosterDefinedByUser
                        ).Generate(context)
                    );
                } 
                return new Tag("div")
                    //.If(YoutubeVideo.isPosterDefinedByUser,
                    //    x => x.Add(GetImage().Generate(context))
                    //)
                    .AddClasses("VideoPlayer")
                    .Add(new VideoPlayerBehaviour(
                        YoutubeVideo.Code,
                        YoutubeVideo.Aspect,
                        ShowControls,
                        AutoPlay,
                        Loop,
                        Sound,
                        cache.Mips.Select(x => new KeyValuePair<int, string>(x.Key, cache.GetFileUri(x.Value).ToString())).ToArray(),
                        YoutubeVideo.isPosterDefinedByUser
                        ).Generate(context)
                    );


            }

            public float[] GetRoi() {
                _image ??= new Image(GetImageSource());
                return _image.GetImageCache().Roi;
            }

            public bool IsRoiFitsIntoWideRect(float[] roi) {
                _image ??= new Image(GetImageSource());
                return _image.IsRoiFitsIntoWideRect(roi);
            }

            public MagickColor GetTopLeftPixel() {
                _image ??= new Image(GetImageSource());
                return _image.GetTopLeftPixel();
            }

            public Uri GetImageUri() {
                _image ??= new Image(GetImageSource());
                return _image.GetImageUri();
            }

            public Image GetImage() {
                _image ??= new Image(GetImageSource());
                return _image;
            }
        }
    }
}
