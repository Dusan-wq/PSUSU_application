using System.Threading;

namespace DataConcentrator
{
    // Skeniranje ulaznih tagova se desava na background nitima (vidi DataConcentratorManager.ScanLoop).
    // WPF binding ne sme da se azurira sa drugog thread-a, pa PropertyChanged notifikacije
    // za objekte koje su bind-ovani u GUI-ju (Tag, Alarm) treba da se "postuju" na UI thread.
    // App.xaml.cs, pri pokretanju, poziva UISync.Capture() na UI thread-u.
    public static class UISync
    {
        private static SynchronizationContext uiContext;

        public static void Capture()
        {
            uiContext = SynchronizationContext.Current;
        }

        public static void Post(SendOrPostCallback action)
        {
            if (uiContext != null)
            {
                uiContext.Post(action, null);
            }
            else
            {
                action(null);
            }
        }
    }
}
