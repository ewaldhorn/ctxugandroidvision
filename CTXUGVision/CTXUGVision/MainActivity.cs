using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Vision;
using Android.Gms.Vision.Faces;
using System;
using Android.Runtime;
using Android.Content.PM;
using Android.Content;
using Android.Util;

namespace CTXUGVision
{
    [Activity(Label = "CTXUGVision", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.FullSensor)]
    public class MainActivity : Activity
    {
        int effect = 0;
        const string TAG = "CTXUGVision";

        Boolean IsFrontFacing = true;

        Context context;

        CameraSource mCameraSource;
        CameraSourcePreview mPreview;
        GraphicOverlay mGraphicOverlay;
        GraphicFaceTracker tracker;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            context = ApplicationContext;

            SetContentView(Resource.Layout.Main);

            Button button = FindViewById<Button>(Resource.Id.myButton);
            button.Click += delegate { ToggleTracker(); };

            Button swapButton = FindViewById<Button>(Resource.Id.swapButton);
            swapButton.Click += delegate { SwapCameras(); };

            mPreview = FindViewById<CameraSourcePreview>(Resource.Id.preview);
            mGraphicOverlay = FindViewById<GraphicOverlay>(Resource.Id.faceOverlay);

            CreateCameraSource();
        }

        void SwapCameras()
        {
            IsFrontFacing = !IsFrontFacing;
            RestartTracking();
        }

        void RestartTracking() {
            if (mCameraSource != null)
            {
                mCameraSource.Release();
                mCameraSource = null;
            }

            CreateCameraSource();
            StartCameraSource();
        }

        protected override void OnResume()
        {
            base.OnResume();

            StartCameraSource();
        }

        protected override void OnPause()
        {
            base.OnPause();

            mPreview.Stop();
        }

        protected override void OnDestroy()
        {
            mCameraSource.Release();

            base.OnDestroy();
        }

        void ToggleTracker() {

            if (tracker != null) {
                effect++;

                switch (effect) {
                    case 0: tracker.ToggleInfo(true); tracker.ToggleHat(false); break;
                    case 1: tracker.ToggleInfo(false); tracker.ToggleHat(true); break;
                    case 2: tracker.Stabilise(true); break;
                    default: effect = -1; tracker.Stabilise(false); break;
                }
            }
        }


        void CreateCameraSource()
        {
            FaceDetector detector = CreateFaceDetector(ApplicationContext);

            mCameraSource = new CameraSource.Builder(ApplicationContext, detector)
                                            .SetFacing(IsFrontFacing ? CameraFacing.Front : CameraFacing.Back)
                                            .SetRequestedPreviewSize(640, 480)
                                            .SetRequestedFps(15.0f)
                                            .SetAutoFocusEnabled(true)
                                            .Build();
        }

        FaceDetector CreateFaceDetector(Context context)
        {
            FaceDetector detector = new FaceDetector.Builder(context)
                                                    .SetLandmarkType(LandmarkDetectionType.All)
                                                    .SetClassificationType(ClassificationType.All)
                                                    .SetTrackingEnabled(true)
                                                    .SetMode(FaceDetectionMode.Accurate)
                                                    .SetProminentFaceOnly(IsFrontFacing)
                                                    .SetMinFaceSize(IsFrontFacing ? 0.30f : 0.20f)
                                                    .Build();

            tracker = new GraphicFaceTracker(context, mGraphicOverlay);
            var processor = new LargestFaceFocusingProcessor.Builder(detector, tracker).Build();
            detector.SetProcessor(processor);

            if (!detector.IsOperational)
            {
                Log.Warn(TAG, "Face detector dependencies are not yet available.");

                IntentFilter lowStorageFilter = new IntentFilter(Intent.ActionDeviceStorageLow);
                Boolean hasLowStorage = RegisterReceiver(null, lowStorageFilter) != null;

                if (hasLowStorage)
                {
                    Toast.MakeText(this, Resource.String.low_storage_error, ToastLength.Long).Show();
                    Log.Warn(TAG, GetString(Resource.String.low_storage_error));
                }
            }
            return detector;
        }

        void StartCameraSource()
        {
            try
            {
                mPreview.Start(mCameraSource, mGraphicOverlay);
            }
            catch (Exception e)
            {
                Log.Error(TAG, "Unable to start camera source.", e);
                mCameraSource.Release();
                mCameraSource = null;
            }
        }

        class GraphicFaceTracker : Tracker
        {
            GraphicOverlay mOverlay;
            FaceGraphic mFaceGraphic;

            public GraphicFaceTracker(Context context, GraphicOverlay overlay)
            {
                mOverlay = overlay;
                mFaceGraphic = new FaceGraphic(context, overlay);
            }

            public override void OnNewItem(int idValue, Java.Lang.Object item)
            {
                mFaceGraphic.Id = idValue;
            }

            public override void OnUpdate(Detector.Detections detections, Java.Lang.Object item)
            {
                mOverlay.Add(mFaceGraphic);
                mFaceGraphic.UpdateFace(item.JavaCast<Face>());
            }

            public override void OnMissing(Detector.Detections detections)
            {
                mOverlay.Remove(mFaceGraphic);
            }

            public override void OnDone()
            {
                mOverlay.Remove(mFaceGraphic);
            }

            // This is a hack for the demo...
            public void ToggleInfo(Boolean status) {
                mFaceGraphic.ShowFaceInfo(status);
            }

            public void ToggleHat(Boolean status) {
                mFaceGraphic.ShowMyHat(status);
            }

            public void Stabilise(Boolean status) {
                mFaceGraphic.ShowStabilise(status);
            }
        }
    }
}

