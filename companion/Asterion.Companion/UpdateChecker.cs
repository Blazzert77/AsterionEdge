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
            bool available=TryVersion(latest,out var lv)&&TryVersion(currentVersion,out var cv)&&lv>cv;
            return new(true,available,currentVersion,latest,url,"");
        }
        catch(OperationCanceledException) when(cancel.IsCancellationRequested) { throw; }
        catch(Exception e) when(e is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new(true,false,currentVersion,"","",e.GetType().Name);
        }
    }

    static bool TryVersion(string value,out Version version)
    {
        value=(value??"").Split('-',2)[0].Trim();
        return Version.TryParse(value,out version!);
    }
}
