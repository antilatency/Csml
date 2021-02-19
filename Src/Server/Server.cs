using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Csml.Server {
    public class Server {
        public static void Run(string url, params string[] args) {
            CreateHostBuilder(url, args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string url, string[] args) {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { 
                webBuilder.UseStartup<Startup>();
                if(!string.IsNullOrEmpty(url)) {
                    webBuilder.UseUrls(url);
                }                
            });
        }
    }
}
