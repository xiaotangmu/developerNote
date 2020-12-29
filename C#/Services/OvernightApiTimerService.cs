using Domain.DataModel;
using Interface.Student;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Supervisor.Hostel;
using Supervisor.Student;
using Supervisor.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViewModel.Student;
using ViewModel.System;

namespace WebApi.Services
{
    public class OvernightApiTimerService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public IServiceProvider Services { get; }

        public OvernightApiTimerService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IServiceProvider services)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            Services = services;
        }

        private async Task RunTimer(object state = null)
        {
            try
            {
                //一统计周期内的所有拍卡记录
                IEnumerable<OvernightTransaction> transactionItems = await GetOvernightRecord();
                IEnumerable<T_STUDENT> allStudent = await GetStudentData();
                if (allStudent == null)
                {
                    return;
                }
                string doorAInnerID = "";
                string doorBInnerID = "";
                string doorAOuterID = "";
                string doorBOuterID = "";
                using (var scope = Services.CreateScope())
                {
                    var systemConfigurationService = scope.ServiceProvider.GetRequiredService<ISystemConfigurationSupervisor>();
                    doorAInnerID = await systemConfigurationService.GetValue(SystemConfigurationViewModel.DoorAInnerIdField);
                    doorBInnerID = await systemConfigurationService.GetValue(SystemConfigurationViewModel.DoorBInnerIdField);
                    doorAOuterID = await systemConfigurationService.GetValue(SystemConfigurationViewModel.DoorAOuterIdField);
                    doorBOuterID = await systemConfigurationService.GetValue(SystemConfigurationViewModel.DoorBOuterIdField);
                }
                foreach (T_STUDENT student in allStudent)
                {
                    IEnumerable<OvernightTransaction> innerRecord = transactionItems?.Where(exp =>
             exp.StudentCode == student.STUDENT_CODE &&
             (exp.DoorID.Equals(doorAInnerID) || exp.DoorID.Equals(doorBInnerID))).OrderByDescending(exp => exp.TransactionDate);
                    IEnumerable<OvernightTransaction> outerRecord = transactionItems?.Where(exp =>
             exp.StudentCode == student.STUDENT_CODE &&
             (exp.DoorID.Equals(doorAOuterID) || exp.DoorID.Equals(doorBOuterID))).OrderByDescending(exp => exp.TransactionDate);
                    if ((innerRecord == null && outerRecord == null) || (innerRecord.Count() == 0 && outerRecord.Count() == 0))
                    {
                        continue;
                    }
                    T_OVERNIGHT overnightApplication = await GetStudentTodayApplication(student.STUDENT_CODE);
                    OvernightTransaction lastRecord = CompareLastTransaction(innerRecord, outerRecord);
                    //没有申请记录，则补上申请记录
                    if (overnightApplication == null)
                    {
                        await SubmitNewApplication(student, lastRecord);
                    }
                    else
                    {
                        await UpdateApplication(overnightApplication, lastRecord);
                    }
                }

                Common.Logger.LoggerManager.DefaultLogger.Info("夜归不归统计完成，时间：" + DateTime.Now.ToString() + "，处理数量：" + transactionItems.Count().ToString());
            }
            catch (Exception ex)
            {
                Common.Logger.LoggerManager.DefaultLogger.Error("线程执行异常：" + ex.Message);
            }
        }

        private OvernightTransaction CompareLastTransaction(IEnumerable<OvernightTransaction> innerRecord, IEnumerable<OvernightTransaction> outerRecord)
        {
            OvernightTransaction lastInnerTransaction = innerRecord?.OrderByDescending(exp => exp.TransactionDate).FirstOrDefault();
            OvernightTransaction lastOuterTransaction = outerRecord?.OrderByDescending(exp => exp.TransactionDate).FirstOrDefault();
            if (lastInnerTransaction.TransactionDate > lastOuterTransaction.TransactionDate)
            {
                lastInnerTransaction.TransactionTypeID = TransactionTypeEnum.In;
                return lastInnerTransaction;
            }
            else
            {
                lastOuterTransaction.TransactionTypeID = TransactionTypeEnum.Out;
                return lastOuterTransaction;
            }
        }

        private async Task UpdateApplication(T_OVERNIGHT overnightApplication, OvernightTransaction lastRecord)
        {
            //申请夜归但外宿的情况，其他情况均属正常
            if (overnightApplication.APPLY_ITEM == ((int)ApplyItemEnum.OVERTIME).ToString() &&
                lastRecord.TransactionTypeID == TransactionTypeEnum.Out)
            {
                overnightApplication.RESULT = ((int)OvernightResultEnum.APPLY_BUT_OUTER).ToString();
                overnightApplication.REALTIMEOFCOMEBACK = lastRecord.TransactionDate;
            }
            else
            {
                overnightApplication.RESULT = ((int)OvernightResultEnum.NORMAL).ToString();
                overnightApplication.REALTIMEOFCOMEBACK = lastRecord.TransactionDate;
            }
            await SubmiteOvernightFormToData(overnightApplication);
        }

        private async Task SubmitNewApplication(T_STUDENT student, OvernightTransaction lastRecord)
        {
            T_OVERNIGHT overnightNew = new T_OVERNIGHT
            {
                ACCOUNT = student.ACCOUNT,
                APPLY_DATE = DateTime.Today,
                CHINESE_NAME = student.CHINESE_NAME,
                CONTACT_MOBILE = student.MOBILE_NO,
                CURRENT_ROOM = student.HOSTEL_ROOM,
                NAME = student.FULL_NAME,
                STUDENT_CODE = student.STUDENT_CODE,
                CREATE_TIME = DateTime.Now,
                UPDATE_TIME = DateTime.Now
            };
            //如果最后一次拍卡是出门，则判断为外宿，即不归
            if (lastRecord.TransactionTypeID == TransactionTypeEnum.Out)
            {
                overnightNew.RESULT = ((int)OvernightResultEnum.NOT_APPLY_BUT_OUTER).ToString();
                overnightNew.REALTIMEOFCOMEBACK = lastRecord.TransactionDate;
            }
            DateTime laterTime = await GetLaterTime();
            DateTime outerTime = await GetOuterTime();
            //如果最后一次拍卡是入门，则判断发生时间是否在夜归区间范围
            if (lastRecord.TransactionTypeID == TransactionTypeEnum.In)
            {
                if (lastRecord.TransactionDate < laterTime)
                {
                    //正常情况，不记录
                    return;
                }
                else if (lastRecord.TransactionDate > laterTime && lastRecord.TransactionDate < outerTime)
                {
                    overnightNew.RESULT = ((int)OvernightResultEnum.NOT_APPLY_BUT_LATER).ToString();
                    overnightNew.REALTIMEOFCOMEBACK = lastRecord.TransactionDate;
                }
            }
            await SubmiteOvernightFormToData(overnightNew);
        }

        private async Task SubmiteOvernightFormToData(T_OVERNIGHT overnightNew)
        {
            using (var scope = Services.CreateScope())
            {
                var overnightService = scope.ServiceProvider.GetRequiredService<IOvernightApplicationSupervisor>();
                await overnightService.SubmitApplicationForm(overnightNew);
            }
        }

        private async Task<T_OVERNIGHT> GetStudentTodayApplication(string studentCode)
        {
            using (var scope = Services.CreateScope())
            {
                var overnightService = scope.ServiceProvider.GetRequiredService<IOvernightApplicationSupervisor>();
                //統計上一天的申請
                return await overnightService.GetInfo(studentCode, DateTime.Today.AddDays(-1));
            }
        }

        private async Task<DateTime> GetOuterTime()
        {
            return await GetConfigurationTime(SystemConfigurationViewModel.OuterTimeField);
        }

        private async Task<DateTime> GetLaterTime()
        {
            return await GetConfigurationTime(SystemConfigurationViewModel.LaterTimeField);
        }

        private async Task<DateTime> GetConfigurationTime(string configCode)
        {
            using (var scope = Services.CreateScope())
            {
                var systemConfigurationSupervisor = scope.ServiceProvider.GetRequiredService<ISystemConfigurationSupervisor>();
                string value = await systemConfigurationSupervisor.GetValue(configCode);
                return DateTime.Parse(string.Format("{0} {1}", DateTime.Today.ToString("yyyy-MM-dd"), value));
            }
        }

        private async Task<IEnumerable<T_STUDENT>> GetStudentData()
        {
            using (var scope = Services.CreateScope())
            {
                var studentService = scope.ServiceProvider.GetRequiredService<IStudentSupervisor>();
                return await studentService.GetAllHostelStudetn();
            }
        }

        public async Task<IEnumerable<OvernightTransaction>> GetOvernightRecord()
        {
            string startTime = string.Empty;
            using (var scope = Services.CreateScope())
            {
                var systemConfigurationSupervisor = scope.ServiceProvider.GetRequiredService<ISystemConfigurationSupervisor>();
                startTime = await systemConfigurationSupervisor.GetValue(SystemConfigurationViewModel.OuterTimeField);
            }
            //一天爲一統計週期
            string startDateTime = string.Format("{0} {1}", DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), startTime);
            string endDateTime = string.Format("{0} {1}", DateTime.Today.ToString("yyyy-MM-dd"), startTime);

            //string baseUrl = await _systemConfigurationSupervisor.GetValue(ViewModel.System.SystemConfigurationViewModel.ApiAddressField);
            Uri clientUri = new Uri(string.Format("{0}{1}?startTime={2}&endTime={3}",
                _configuration.GetSection("OvernightApi:BaseUrl").Value,
                _configuration.GetSection("OvernightApi:GetTransaction").Value,
                startDateTime, endDateTime));
            return await GetTransactionGroup(clientUri);
        }

        private async Task<IEnumerable<OvernightTransaction>> GetTransactionGroup(Uri clientUri)
        {
            List<OvernightTransaction> items = new List<OvernightTransaction>();
            if (clientUri == null)
            {
                return items;
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
                    RequestResponse<IEnumerable<OvernightTransaction>> transactionGroup = JsonConvert.DeserializeObject<RequestResponse<IEnumerable<OvernightTransaction>>>(result);
                    return transactionGroup.data;
                }
            }
            catch (TimeoutException timeOutException)
            {
                throw new Exception(await Localization.Localizer.GetValueAsync("TimeOut"));
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Api服務未啓動");
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string configTime = string.Format("{0} {1}", DateTime.Today.ToString("yyyy-MM-dd"), _configuration.GetSection("OvernightApi:RunTime").Value);
            DateTime beginTime = DateTime.Parse(configTime);
            TimeSpan dueTime = new TimeSpan();//延迟时间量
            if (DateTime.Now < beginTime)
            {
                dueTime = beginTime.Subtract(DateTime.Now);
            }
            else
            {
                dueTime = beginTime.AddDays(1).Subtract(DateTime.Now);
            }
            int interval = 24 * 3600 * 1000;
            //计时器，从下一个时间点开始，并以24小时为一循环周期
            Timer timer = new Timer(async state =>
            {
                await RunTimer(state);
            }, null, Convert.ToInt64(dueTime.TotalMilliseconds), interval);
        }
    }
}
