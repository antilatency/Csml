var youTubeIFrameApiIsReady = false;
var youTubeIFrameApiWaitList = [];
var youTubeIFrameID = 0;
function getUniqueYouTubeIFrameID() {
    youTubeIFrameID++;
    return "youTubeIFrameID" + youTubeIFrameID;
}
function callWhenYouTubeIFrameApiReady(callback) {
    if (youTubeIFrameApiIsReady) {
        callback()
    } else {
        youTubeIFrameApiWaitList.push(callback);
    }
}
function onYouTubeIframeAPIReady() {
    youTubeIFrameApiWaitList.forEach(element => element());
}

function VideoPlayer(element, code, aspect, showControls, autoPlay, loop, sound, mips) {
    this.element = element;
    this.code = code;
    this.aspect = aspect;
    this.showControls = showControls;
    this.autoPlay = autoPlay;
    this.loop = loop;
    this.sound = sound;
    this.mips = mips;
    this.previousContainerWidth = -1;

    this.element.style.paddingTop = 100 / this.aspect + "%";


    this.onYoutubePlayerReady = function (event) {
        /*if (this.autoPlay) {
            console.log("onYoutubePlayerReady:playVideo");
            this.player.playVideo();
        }*/
    }

    this.onYoutubePlayerStateChange = function (event) {
        /*if (event.data == YT.PlayerState.PLAYING) {
            //this.player.unMute()
        }*/
        if (event.data == YT.PlayerState.ENDED) {
            if (this.loop) {
                this.player.seekTo(0);
                this.player.playVideo();
            }
            return;
        }

    }

    this.CreateYoutubeIFrame = function () {
        
        var _this = this;
        callWhenYouTubeIFrameApiReady(function () {
            
            var iframe = document.createElement("div");
            iframe.className = "VideoPlayerInner"

            if (!_this.showControls) {
                iframe.style.top = "-100%";
                iframe.style.height = "300%";
                iframe.style.pointerEvents = "none";
            }

            iframe.id = getUniqueYouTubeIFrameID();
            _this.element.appendChild(iframe);
            var controls = _this.showControls ? 1 : 0;
            _this.player = new YT.Player(iframe.id, {
                
                height: '100%',
                width: '100%',
                videoId: _this.code,
                host: "http://www.youtube-nocookie.com",
                playerVars: {
                    "rel": 0,
                    "autoplay": _this.autoPlay?1:0,
                    "controls" : _this.showControls ? 1 : 0,
                    "mute": _this.autoPlay ? 1 : (_this.sound?0:1)
                },                
                events: {
                    'onReady': function (event) { console.log("onReady " + _this.code); _this.onYoutubePlayerReady(event) },
                    'onStateChange': function (event) { _this.onYoutubePlayerStateChange(event) }
                }
            });


        })

        
        
    }
    this.CreateVideoTag = function () {
        console.log(this.mips[0].Value)
        var video = document.createElement("video");
        video.className = "VideoPlayerInner";
        video.src = this.mips[0].Value;
        video.controls = this.showControls;
        video.autoplay = this.autoPlay;
        video.muted = this.autoPlay || (!this.sound);
        video.loop = this.loop;
        this.element.appendChild(video);
    }

    this.CreateYouTubeIFrameApiScript = function () {
        
        const id = "YouTubeIFrameApiScript";
        var apiElement = document.getElementById(id);

        if (apiElement === null) {

            apiElement = document.createElement("script");
            apiElement.id = id;

            apiElement.src = "https://www.youtube.com/iframe_api";
            window.YouTubeIFrameApiStatus = "loading";
            window.YouTubeIFrameApiStatusTimeout = setTimeout(() => {
                apiElement.onerror && apiElement.onerror()
            }, 1000);

            var head = document.getElementsByTagName('head')[0];
            head.appendChild(apiElement);
        }

        var _this = this;
        if (window.YouTubeIFrameApiStatus === "loading") {

            var prewOnLoad = apiElement.onload;
            apiElement.onload = function () {
                apiElement.onload = null;
                apiElement.onerror = null;

                if (window.YouTubeIFrameApiStatus === "loading") {
                    window.YouTubeIFrameApiStatus = "ready"
                }
                if (apiElement.onerror !== null) apiElement.onerror = null;
                prewOnLoad && prewOnLoad();
                _this.CreateYoutubeIFrame();
            };

            var prewOnError = apiElement.onerror;
            apiElement.onerror = function () {
                apiElement.onload = null;
                apiElement.onerror = null;

                if (window.YouTubeIFrameApiStatus === "loading") {
                    window.YouTubeIFrameApiStatus = "failed"
                }                
                prewOnError && prewOnError();
                //console.log("youtube failed " + performance.now())
                _this.CreateVideoTag();                
            };

        } else {
            if (window.YouTubeIFrameApiStatus === "ready") {
                _this.CreateYoutubeIFrame();
            }
            if (window.YouTubeIFrameApiStatus === "failed") {
                _this.CreateVideoTag();
            }
        }
    }


    

    
    console.log("VideoPlayer " + mips[0].Value);
    if (!showControls || autoPlay) {
        this.CreateVideoTag()
    } else {
        this.CreateYouTubeIFrameApiScript();
    }
    

    //var document.getElementById()
    //var tag = document.createElement('script');


    this.OnWindowResize = function () {
        var width = this.element.offsetWidth;
        if (this.previousContainerWidth == width) {
            return;
        }
        this.previousContainerWidth = width;
    }

    

    this.OnDOMContentLoaded = function (event) {
        this.OnWindowResize();
    }

}