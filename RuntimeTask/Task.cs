using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Threading;

namespace RuntimeTask
{

    public sealed partial class Task : IBackgroundTask
    {
        //后台任务取消的原因
        BackgroundTaskCancellationReason _cancelReason = BackgroundTaskCancellationReason.Abort;
        volatile bool _cancelRequested = false;
        BackgroundTaskDeferral _deferral = null;
        ThreadPoolTimer _periodicTimer = null;
        uint _progress = 0;
        IBackgroundTaskInstance _taskInstance = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background " + taskInstance.Task.Name + " Starting...");

            //获取后台任务的级别
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values["BackgroundWorkCost"] = cost.ToString();

  
            //后台任务取消的事件
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            //deferral
            _deferral = taskInstance.GetDeferral();

            //后台任务实例
            _taskInstance = taskInstance;

            //定时器
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(PeriodicTimerCallback), TimeSpan.FromSeconds(1));
        }

        //后台任务取消
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;
            _cancelReason = reason;

            Debug.WriteLine("Background " + sender.Task.Name + " Cancel Requested...");
        }


        //定时器执行的方法
        private void PeriodicTimerCallback(ThreadPoolTimer timer)
        {
            //如果没有被取消，任务没有完成，则执行后台任务的Progress方法
            if ((_cancelRequested == false) && (_progress < 100))
            {
                _progress += 10;
                _taskInstance.Progress = _progress;
            }
            //如果任务完成，取消定时器，并告知后台任务完成
            else
            {
                //取消定时器
                _periodicTimer.Cancel();
                //获取后台任务的名称
                var key = _taskInstance.Task.Name;
                //如果任务没有完成，则存储取消任务的原因，如果任务完成，则存储Completed
                String taskStatus = (_progress < 100) ? "Canceled with reason: " + _cancelReason.ToString() : "Completed";
                var settings = ApplicationData.Current.LocalSettings;
                settings.Values[key] = taskStatus;
                Debug.WriteLine("Background " + _taskInstance.Task.Name + settings.Values[key]);

                //告知后台任务完成
                _deferral.Complete();
            }
        }
    }
}
