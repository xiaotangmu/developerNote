using DataModel.System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Supervisor.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ViewModel.Student;
using ViewModel.System;

namespace WebApi.Services
{
    public class DPSIServiceHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISystemConfigurationSupervisor _systemConfigurationSupervisor;

        public DPSIServiceHelper(IConfiguration configuration, IHttpClientFactory httpClientFactory, ISystemConfigurationSupervisor systemConfigurationSupervisor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _systemConfigurationSupervisor = systemConfigurationSupervisor;
        }

        public async Task<StudentBaseInfoViewModel> GetNewStudentInfo(QuickLoginModel loginModel)
        {
            string baseUrl = await _systemConfigurationSupervisor.GetValue(SystemConfigurationViewModel.ApiAddressField);
            Uri clientUri = new Uri(string.Format("{0}{1}?applicationCode={2}&idNo={3}",
                SectionUrlBuilder.BuildHttpBaseUrl(_configuration, baseUrl, "DPSI:BaseUrl"),
                      _configuration.GetSection("DPSI:GetNewStudentInfoApi").Value,
                      loginModel.ApplicationCode, loginModel.IdNo));
            return await GetStudentInfo(clientUri);
        }

        public async Task<StudentBaseInfoViewModel> GetOldStudentInfo(string account)
        {
            string baseUrl = await _systemConfigurationSupervisor.GetValue(SystemConfigurationViewModel.ApiAddressField);
            Uri clientUri = new Uri(string.Format("{0}{1}?account={2}",
                    SectionUrlBuilder.BuildHttpBaseUrl(_configuration, baseUrl, "DPSI:BaseUrl"),
                    _configuration.GetSection("DPSI:GetOldStudentInfoApi").Value,
                    account));
            return await GetStudentInfo(clientUri);
        }

        public async Task<IEnumerable<StudentBaseInfoViewModel>> GetNewStudentInfoGroup(List<QuickLoginModel> viewModel)
        {
            string baseUrl = await _systemConfigurationSupervisor.GetValue(SystemConfigurationViewModel.ApiAddressField);
            Uri clientUri = new Uri(string.Format("{0}{1}",
                  SectionUrlBuilder.BuildHttpBaseUrl(_configuration, baseUrl, "DPSI:BaseUrl"),
                    _configuration.GetSection("DPSI:BatchGetNewStudentInfoApi").Value));
            return await SyncStudentInfoGroup(clientUri, viewModel);
        }

        private async Task<StudentBaseInfoViewModel> GetStudentInfo(Uri clientUri)
        {
            if (clientUri == null)
            {
                return new StudentBaseInfoViewModel();
            }
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = clientUri;
            try
            {
                var response = await client.GetAsync(client.BaseAddress);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    RequestResponse<StudentBaseInfoViewModel> addressResult = JsonConvert.DeserializeObject<RequestResponse<StudentBaseInfoViewModel>>(result);
                    return addressResult.data;
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
            return null;
        }

        private async Task<IEnumerable<StudentBaseInfoViewModel>> SyncStudentInfoGroup(Uri clientUri, List<QuickLoginModel> data)
        {
            if (clientUri == null)
            {
                return Enumerable.Empty<StudentBaseInfoViewModel>();
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
                    RequestResponse<IEnumerable<StudentBaseInfoViewModel>> studentGroup = JsonConvert.DeserializeObject<RequestResponse<IEnumerable<StudentBaseInfoViewModel>>>(result);
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
            return Enumerable.Empty<StudentBaseInfoViewModel>();
        }

        private HttpContent BuildRequestContent(List<QuickLoginModel> requestBody)
        {
            string jsonContent = BuildParams(requestBody);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        private string BuildParams(List<QuickLoginModel> applyModel)
        {
            return JsonConvert.SerializeObject(applyModel);
        }

        public static CurrentUserInfo ValidateNewStudentIdentity(QuickLoginModel loginModel)
        {
            //先从数据库中验证，如果不存在再更新DPSI数据，还是不存在则返回提示
            return new CurrentUserInfo
            {
                Name = "測試",
                Account = loginModel.ApplicationCode,
                UserType = UserType.VISITOR
            };
        }
    }
}
