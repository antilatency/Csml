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

function VideoPlayer(element, code, aspect, showControls, autoPlay, loop, sound, mips, poster) {
    this.element = element;
    this.code = code;
    this.aspect = aspect;
    this.showControls = showControls;
    this.autoPlay = autoPlay;
    this.loop = loop;
    this.sound = sound;
    this.mips = mips;
    this.previousContainerWidth = -1;
    this.poster = poster;

    //this.element.style.paddingTop = 100 / this.aspect + "%";
    var ResizeContainer = function() {
        element.style.height = (element.offsetWidth / aspect) + "px";
    }
    window.addEventListener("resize", ResizeContainer);

    this.onYoutubePlayerReady = function(event) {
        /*if (this.autoPlay) {
            console.log("onYoutubePlayerReady:playVideo");
            this.player.playVideo();
        }*/
    }

    this.onYoutubePlayerStateChange = function(event) {
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

    this.CreateYoutubeIFrame = function() {

        var _this = this;
        callWhenYouTubeIFrameApiReady(function() {
            var iframe = document.createElement("div");
            iframe.className = "VideoPlayerInner";
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
                    "autoplay": _this.autoPlay ? 1 : 0,
                    "controls": _this.showControls ? 1 : 0,
                    "mute": _this.autoPlay ? 1 : (_this.sound ? 0 : 1),
                },
                events: {
                    'onReady': function(event) {
                        _this.onYoutubePlayerReady(event)
                    },
                    'onStateChange': function(event) { _this.onYoutubePlayerStateChange(event) }
                }
            });


        })

    }

    this.CreateVideoTag = function(withPoster) {

        var video = document.createElement("video");
        video.className = "VideoPlayerInner";
        video.poster = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";
        video.controls = this.showControls;
        video.autoplay = this.autoPlay;
        video.muted = this.autoPlay || (!this.sound);
        video.loop = this.loop;
        this.element.appendChild(video);

        var previousContainerWidth = element.offsetWidth * window.devicePixelRatio;

        var SelectClosestMip = function(elementWidth) {

            for (var i = 0; i < mips.length; i++) {
                var width = mips[i].Key * aspect;
                if (width > 0.95 * elementWidth) return mips[i];
            }
            return mips[mips.length - 1];
        }
        let closestMip = SelectClosestMip(element.offsetWidth * window.devicePixelRatio);
        video.src = closestMip.Value;


        var onResize = function() {

            var width = element.offsetWidth * window.devicePixelRatio;

            if (previousContainerWidth == width) {
                return;
            }
            previousContainerWidth = width;
            var newClosestMip = SelectClosestMip(width);

            if (newClosestMip.Key != closestMip.Key) {
                closestMip = newClosestMip;
                var time = video.currentTime;
                video.src = closestMip.Value;
                video.currentTime = time;
            }
        }

        window.addEventListener("resize", onResize);

    }

    this.CreateYouTubeIFrameApiScript = function() {
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
        if (poster) {
            _this.CreateVideoTag(poster);
            return;
        }
        if (window.YouTubeIFrameApiStatus === "loading") {

            var prewOnLoad = apiElement.onload;
            apiElement.onload = function() {
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
            apiElement.onerror = function() {
                apiElement.onload = null;
                apiElement.onerror = null;

                if (window.YouTubeIFrameApiStatus === "loading") {
                    window.YouTubeIFrameApiStatus = "failed"
                }
                prewOnError && prewOnError();
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

    this.OnDOMContentLoaded = function(event) {
        ResizeContainer();

        if (!showControls || autoPlay) {
            this.CreateVideoTag()
        } else {
            this.CreateYouTubeIFrameApiScript();
        }
    }

}