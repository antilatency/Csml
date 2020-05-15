using System;
using System.Collections.Generic;
using System.Text;

namespace Csml {
    class StringUtils {

        private static readonly string[] SizeSuffixes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB"};

        /// <summary>
        /// Convert size in bytes to human-readable string
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <returns></returns>
        public static string HumanizeSize(long size) {
            int order = 0;

            while (size >= 1024 || size <= -1024) {
                order++;
                size /= 1024;
            }

            return size + " " + SizeSuffixes[order];
        }
    }
}
