using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MOAI.API.Services
{
    public class PolicyLoaderService

    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public PolicyLoaderService(IConfiguration config, HttpClient http)
        {
            _config = config;
            _http = http;
        }

        public async Task<string> LoadSystemPolicyAsync()
        {
            var urls = _config.GetSection("PolicySources").GetChildren().Select(x => x.Value);
            var sb = new StringBuilder();

            foreach (var url in urls)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    var text = await _http.GetStringAsync(url);
                    sb.AppendLine(text);
                }
            }

            return sb.ToString();
        }
    }
}
