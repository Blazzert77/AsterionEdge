using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Asterion.Companion;

public record UpdateState(bool Checked, bool Available, string CurrentVersion, string LatestVersion, string Url, string Error = "");

public static class UpdateChecker
{
    public static async Task<UpdateState> CheckGitHub(string repository, string currentVersion, CancellationToken cancel)
    {
        if(string.IsNullOrWhiteSpace(repository) || !repository.Contains('/'))
            return new(false,false,currentVersion,"","","Dépôt de mise à jour invalide");
        try
        {
            using var http=new HttpClient { Timeout=TimeSpan.FromSeconds(6) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AsterionEdge", currentVersion));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            using var response=await http.GetAsync($"https://api.github.com/repos/{repository}/releases/latest",cancel);
            if(response.StatusCode==HttpStatusCode.NotFound)
                return new(true,false,currentVersion,"","","");
            response.EnsureSuccessStatusCode();
            using var doc=JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancel));
            string tag=doc.RootElement.TryGetProperty("tag_name",out var t)?t.GetString()??"":"";
            string url=doc.RootElement.TryGetProperty("html_url",out var u)?u.GetString()??"":"";
            string latest=tag.Trim().TrimStart('v','V');
            bool available=IsNewer(latest,currentVersion);
            return new(true,available,currentVersion,latest,url,"");
        }
        catch(OperationCanceledException) when(cancel.IsCancellationRequested) { throw; }
        catch(Exception e) when(e is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new(true,false,currentVersion,"","",e.GetType().Name);
        }
    }

    static bool TryVersion(string value,out Version version,out string prerelease)
    {
        var parts=(value??"").Trim().Split('+',2)[0].Split('-',2);
        prerelease=parts.Length>1?parts[1]:"";
        return Version.TryParse(parts[0],out version!);
    }
    // SemVer ordering: 0.3.0 > 0.3.0-dev.3 and 0.3.0-dev.10 > 0.3.0-dev.3 (the old check ignored the pre-release part).
    public static bool IsNewer(string latest,string current)
    {
        if(!TryVersion(latest,out var lv,out var lp)||!TryVersion(current,out var cv,out var cp))return false;
        int core=lv.CompareTo(cv);
        if(core!=0)return core>0;
        if(lp==cp)return false;
        if(lp=="")return true;
        if(cp=="")return false;
        var a=lp.Split('.');var b=cp.Split('.');
        for(int i=0;i<Math.Max(a.Length,b.Length);i++)
        {
            if(i>=a.Length)return false;
            if(i>=b.Length)return true;
            bool an=long.TryParse(a[i],out long ai),bn=long.TryParse(b[i],out long bi);
            int c=an&&bn?ai.CompareTo(bi):an?-1:bn?1:string.CompareOrdinal(a[i],b[i]);
            if(c!=0)return c>0;
        }
        return false;
    }
}

