using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Config.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgileConfigPreviewSample.Client
{
    public class Program
    {
        public static IConfigClient ConfigClient;
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                      .ConfigureAppConfiguration((context, config) =>
                      {
                          //newһ��clientʵ��
                          //ʹ���޲ι��캯�����Զ���ȡ����appsettings.json�ļ���AgileConfig�ڵ������
                          var configClient = new ConfigClient();
                          //ʹ��AddAgileConfig����һ���µ�IConfigurationSource
                          config.AddAgileConfig(configClient);
                          //��һ����������clientʵ�����Ա������ط�����ֱ��ʹ��ʵ����������
                          ConfigClient = configClient;
                          //ע���������޸��¼�
                          configClient.ConfigChanged += (arg) =>
                          {
                              Console.WriteLine($"action:{arg.Action} key:{arg.Key}");
                              SocketPool.SendMessage("reload");
                          };
                      })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
