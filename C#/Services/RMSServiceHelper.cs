using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Supervisor.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ViewModel.Student;

namespace WebApi.Services
{
    public class RMSServiceHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISystemConfigurationSupervisor _systemConfigurationSupervisor;

        public RMSServiceHelper(IConfiguration configuration, IHttpClientFactory httpClientFactory, ISystemConfigurationSupervisor systemConfigurationSupervisor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _systemConfigurationSupervisor = systemConfigurationSupervisor;
        }

        public async Task<DebitStatus> IsDebit(string debitNoteId)
        {
            string baseUrl = await _systemConfigurationSupervisor.GetValue(ViewModel.System.SystemConfigurationViewModel.ApiAddressField);
            Uri clientUri = new Uri(string.Format("{0}{1}?debitNoteId={2}", SectionUrlBuilder.BuildHttpBaseUrl(_configuration, baseUrl, "RMS:BaseUrl"),
          _configuration.GetSection("RMS:IsDebit").Value, debitNoteId));
            return await HttpGet(clientUri);
        }

        public async Task<IEnumerable<DebitReceipt>> CreateDebitNotes(List<CreateDebitParams> paramGroup)
        {
            string baseUrl = await _systemConfigurationSupervisor.GetValue(ViewModel.System.SystemConfigurationViewModel.ApiAddressField);
            Uri clientUri = new Uri(string.Format("{0}{1}", SectionUrlBuilder.BuildHttpBaseUrl(_configuration, baseUrl, "RMS:BaseUrl"),
          _configuration.GetSection("RMS:CreateRMSDebitNotes").Value));
            return await HttpPost(clientUri, paramGroup);
        }

        private async Task<IEnumerable<DebitReceipt>> HttpPost(Uri clientUri, List<CreateDebitParams> data)
        {
            if (clientUri == null)
            {
                return Enumerable.Empty<DebitReceipt>();
            }
            HttpContent requestContent = BuildRequestContent(data);
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = clientUri;
            try
            {
                var response = await client.PostAsync(client.BaseAddress, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    RequestResponse<IEnumerable<DebitReceipt>> studentGroup = JsonConvert.DeserializeObject<RequestResponse<IEnumerable<DebitReceipt>>>(result);
                    return studentGroup.data;
                }
            }
            catch (TimeoutException timeOutException)
            {
                throw new Exception(await Localization.Localizer.GetValueAsync("TimeOut"));
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("DPSI服務未啓動");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                client.Dispose();
            }
            return Enumerable.Empty<DebitReceipt>();
        }

        private HttpContent BuildRequestContent(List<CreateDebitParams> requestBody)
        {
            string jsonContent = BuildParams(requestBody);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        private string BuildParams(List<CreateDebitParams> applyModel)
        {
            return JsonConvert.SerializeObject(applyModel);
        }

        private async Task<DebitStatus> HttpGet(Uri clientUri)
        {
            if (clientUri == null)
            {
                return new DebitStatus();
            }
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = clientUri;
            try
            {
                var response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    RequestResponse<DebitStatus> result = JsonConvert.DeserializeObject<RequestResponse<DebitStatus>>(data);
                    return result.data;
                }
            }
            catch (TimeoutException timeOutException)
            {
                throw new Exception(await Localization.Localizer.GetValueAsync("TimeOut"));
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("RMS服務未啓動");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                client.Dispose();
            }
            return new DebitStatus();
        }
    }
}
