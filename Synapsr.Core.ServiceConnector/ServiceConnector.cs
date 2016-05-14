using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Synapsr.Core.ServiceConnector
{
    public enum MethodType
    {
        POST,
        GET
    }

    public interface IServiceConnector
	{
		Task<Token> Connect(string startURL, string endURL, string tokenURL,MethodType method);
	}

    public class ServiceConnector : IServiceConnector
	{
		HttpClient client = new HttpClient();
        
		public async Task<Token> Connect(string startURL, string endURL, string tokenURL,MethodType method)
		{
			string result;
			Uri startURI = new Uri(startURL);
			Uri endURI = new Uri(endURL);

			try
			{
				var webAuthenticationResult =
					await WebAuthenticationBroker.AuthenticateAsync(
					WebAuthenticationOptions.None,
					startURI,
					endURI);

				switch (webAuthenticationResult.ResponseStatus)
				{
					case WebAuthenticationStatus.Success:
						// Successful authentication. 
						result = webAuthenticationResult.ResponseData.ToString();
						var pattern = @"code=(.+?)&";
						var match = Regex.Match(result+"&", pattern);
						if (match.Groups.Count > 0)
						{
							var code = match.Groups[0].Captures[0].Value;
                            HttpResponseMessage response = null;
                            switch (method)
                            {
                                case MethodType.POST:
                                    StringContent cnt = new StringContent("");
                                    response = await client.PostAsync(tokenURL + code,cnt);
                                    break;
                                case MethodType.GET:
                                    response = await client.GetAsync(tokenURL + code);
                                    break;
                                default:
                                    break;
                            }
                            
							if(response.IsSuccessStatusCode)
							{
								string json = await response.Content.ReadAsStringAsync();
								Token token = Newtonsoft.Json.JsonConvert.DeserializeObject<Token>(json);
								return token;
							}
						}
						break;
					case WebAuthenticationStatus.ErrorHttp:
						// HTTP error. 
						result = webAuthenticationResult.ResponseErrorDetail.ToString();
						break;
					default:
						// Other error.
						result = webAuthenticationResult.ResponseData.ToString();
						break;
				}
			}
			catch (Exception ex)
			{
				// Authentication failed. Handle parameter, SSL/TLS, and Network Unavailable errors here. 
				result = ex.Message;
			}
			return null;
		}
	}
}
