using Android.App;
using Android.Runtime;
using AndroidX.Camera.View;
using CommunityToolkit.Maui.Core.Handlers;

namespace BlackJackCamera
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
#if ANDROID
            CameraViewHandler.ViewMapper.AppendToMapping("CameraViewHandler", (handler, view) =>
            {
                if (handler.PlatformView is PreviewView previewView)
                {
                    previewView.SetScaleType(PreviewView.ScaleType.FillCenter);
                }
            });
#endif
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
