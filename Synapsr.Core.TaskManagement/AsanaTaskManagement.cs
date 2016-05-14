using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synapsr.Core.ServiceConnector;

namespace Synapsr.Core.TaskManagement
{
    public interface ITaskManagement
    {
        Task Connect();
    }
    public class AsanaTaskManagement : ITaskManagement
    {
        IServiceConnector _connector;
        public async Task Connect()
        {
            Token token;
            var cachedToken = TokenManager.GetToken(TokenManager.TokenType.Asana);
            if (cachedToken!=string.Empty)
            {
                token = new Token { access_token = cachedToken };
            }
            else
            {
                token = await AcquireToken();
                TokenManager.AddToken(token.access_token, TokenManager.TokenType.Asana);
            }
        }

        public AsanaTaskManagement(IServiceConnector connector)
        {
            this._connector = connector;
        }

        public async Task<Token> AcquireToken()
        {
            string startURL = "https://app.asana.com/-/oauth_authorize?response_type=code&client_id=133299781761330&redirect_uri=http://facebook.com/";
            string endURL = "http://facebook.com/";
            string tokenURL = "https://app.asana.com/-/oauth_token?client_id=133299781761330&grant_type=authorization_code&client_secret=b37c19ab8411f648f36c9856b1d472b7&redirect_uri=http://facebook.com/&expires_in=never&";
            return await _connector.Connect(startURL, endURL, tokenURL, MethodType.POST);
        }
    }
}
