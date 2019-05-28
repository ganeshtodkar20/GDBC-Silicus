using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PartsUnlimitedScriptsIncluder
{

    public static IEnumerable<string> GetScripts(HttpContext context)
    {
        string root = ((IHostingEnvironment)context.RequestServices.GetService(typeof(IHostingEnvironment))).WebRootPath;
        root = Path.Combine(root, "Scripts");
        return Directory.EnumerateFiles(root, "pu_*.js").Select(path => { return "/scripts/" + new FileInfo( path ).Name; });
    }
}