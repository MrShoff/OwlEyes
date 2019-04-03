using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


class OwlApi
{
    #region Properties
    private const string API_KEY = null;
    private const string API_ROOT_PATH = "https://api.overwatchleague.com";
    private const string CLIENT_ID = null;
    #endregion

    #region Initialize
    public OwlApi()
    {
        Initialize();
    }

    private void Initialize()
    {
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Return a current schedule of Overwatch League.
    /// </summary>
    /// <returns></returns>
    public async Task<HttpResponseMessage> GetOwlSchedule()
    {
        string path = "/schedule";
        string fullRequestPath = API_ROOT_PATH + path;

        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        return await GetApiResponse(fullRequestPath);
    }        
    #endregion

    #region Private Functions
    /// <summary>
    /// Get a response using an entire call path.
    /// </summary>
    /// <param name="path">The entire call path, including subdomain and parameters.</param>
    /// <returns></returns>
    private async Task<HttpResponseMessage> GetApiResponse(string path)
    {
        HttpClient client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(path);

        return response;
    }
    #endregion
}

