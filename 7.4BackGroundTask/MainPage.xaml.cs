using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace _7._4BackGroundTask
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        //注册后台任务
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var exampleTaskName = "ExampleBackgroundTask";
            //如果后台任务已经存在，则跳出
            foreach (var mytask in BackgroundTaskRegistration.AllTasks)
            {
                if (mytask.Value.Name == exampleTaskName)
                {
                    break;
                }
            }

            //否则注册后台任务
            var builder = new BackgroundTaskBuilder();
            builder.Name = exampleTaskName;
            builder.TaskEntryPoint = "RuntimeTask.Task";

            IBackgroundTrigger triggter = new SystemTrigger(SystemTriggerType.TimeZoneChange, false);
            builder.SetTrigger(triggter);

            //指定后台任务的回调方法
            BackgroundTaskRegistration task = builder.Register();
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);

        }

        //取消后台任务的方法（这里取消所有的后台任务）
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                cur.Value.Unregister(true);
            }
        }


        //后台任务运行过程中执行的方法
        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            //异步处理结果
            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var progress = "Progress: " + args.Progress + "%";
            });
        }


        //后台任务完成执行的方法
        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var key = task.Name;
            var message = settings.Values[key].ToString();
        }
    }
}
