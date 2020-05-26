using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
//using VideoLibrary;


namespace Csml {
    public class VideoPlayerBehaviour : Behaviour {
        public VideoPlayerBehaviour(string code, float aspect, bool showControls,bool autoPlay, bool loop, bool sound, KeyValuePair<int,string>[] mips)
            : base("VideoPlayer", code, aspect, showControls, autoPlay, loop, sound, mips) { }
    };

    public class YoutubeVideoCache : Cache<YoutubeVideoCache> {

        public Dictionary<int, string> Mips;
    }


    public class YoutubeVideo {
        string Code;
        float Aspect;
        private Task<YoutubeVideoCache> CacheAsync;
        public YoutubeVideoCache Cache => CacheAsync.Result;

        public YoutubeVideo(string code, float aspect = 16f/9f) {
            Code = code;
            Aspect = aspect;
            CacheAsync = GetCacheAsync();
        }

        public void CopyFiles(string outputRootDirectory) { 
            
        }

        public Player GetPlayer() {
            return new Player(this);
        }

        private async Task DownloadVideoAsync(VideoLibrary.YouTubeVideo video, YoutubeVideoCache cache) {
            var contents = await video.GetBytesAsync();
            
            Utils.CreateDirectory(cache.Directory);
            var path = Path.Combine(cache.Directory, $"{cache.Hash}_{video.Resolution}{video.FileExtension}");
            File.WriteAllBytes(path, contents);
            Console.WriteLine($"{Code}: download {path} done.");
        }




        private async Task<YoutubeVideoCache> GetCacheAsync() {
            var result = YoutubeVideoCache.Load(Code);
            if (result != null) return result;

            Console.WriteLine($"{Code}:Begin");

            var youTube = VideoLibrary.YouTube.Default;
            var videos = await youTube.GetAllVideosAsync("https://www.youtube.com/watch?v=" + Code);

            Console.WriteLine($"{Code}:Got videos list");

            
            result = new YoutubeVideoCache();
            result.Hash = Code;

            var videosToDownload = videos
                .Where(x => x.Resolution > 0 && x.AudioBitrate > 0);

            var videoDownloaders = videosToDownload
                .Select(x => {
                    return DownloadVideoAsync(x, result);
                });

            await Task.WhenAll(videoDownloaders);

            result.Mips = new Dictionary<int, string>(
                videosToDownload.Select(
                    x => new KeyValuePair<int, string>(x.Resolution, $"{result.Hash}_{x.Resolution}{x.FileExtension}")
                    ).OrderBy(x=>x.Key)
                );

            result.Save();

            return result;
        }

        public class Player : Element<Player> {
            private YoutubeVideo YoutubeVideo;
            //Dictionary<string, string> Parameters;
            //Behaviour Behaviour;

            public bool ShowControls { get; set; } = true;
            public bool AutoPlay { get; set; } = false;
            public bool Loop { get; set; } = false;
            public bool Sound { get; set; } = true;



            public Player(YoutubeVideo youtubeVideo) {
                YoutubeVideo = youtubeVideo;
                //SetParameter("rel", 0);
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

            /*private Player SetParameter(string name, object value) {
                if (Parameters == null) Parameters = new Dictionary<string, string>();
                Parameters[name] = value.ToString();
                return this;
            }

            private string GetParametersString() {
                if (Parameters == null) return "";
                var result = "?"+string.Join("&", Parameters.Select(x => $"{x.Key}={x.Value}"));
                return result;
            }*/

            //private int autoplay = 0;
            /*public Player AutoplayEnable => SetParameter("autoplay", 1);
            public Player ControlsDisable => SetParameter("controls", 0);
            public Player Mute => SetParameter("mute", 1);
            public Player Loop =>  SetParameter("loop", 1).SetParameter("playlist", YoutubeVideo.Code);
            public Player ShowInfoDisable => SetParameter("showinfo", 0);*/



            public override IEnumerable<HtmlNode> Generate(Context context) {

                var cache = YoutubeVideo.Cache;

                yield return HtmlNode.CreateNode("<div>").Do(x=> {
                    x.AddClass("VideoPlayer");
                    x.Add(new VideoPlayerBehaviour(
                        YoutubeVideo.Code,
                        YoutubeVideo.Aspect,
                        ShowControls,
                        AutoPlay,
                        Loop,
                        Sound,
                        cache.Mips.Select(x=>new KeyValuePair<int,string>(x.Key, cache.GetFileUri(x.Value).ToString())).ToArray()

                        ).Generate(context));
                });


                /*string url =
                    "https://www.youtube.com/embed/" + YoutubeVideo.Code + GetParametersString();


                yield return HtmlNode.CreateNode("<iframe>").Do(x => {
                    x.SetAttributeValue("src", url);
                    x.SetAttributeValue("frameborder", "0");
                    x.SetAttributeValue("allow", "accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture");
                    x.SetAttributeValue("allowfullscreen", "");
                    x.AddClass("VideoPlayer");

                    //x.Add(base.Generate(context));

                }).Wrap("<div>").Do(x=>{ x.AddClass("VideoPlayerContainer"); });*/
            }

        }
    }
}
