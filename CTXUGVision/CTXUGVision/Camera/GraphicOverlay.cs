using System;
using Android.Views;
using Android.Gms.Vision;
using System.Collections.Generic;
using Android.Graphics;
using Android.Content;
using Android.Util;

namespace CTXUGVision
{
    public class GraphicOverlay : View
    {
        object mLock = new object ();
        int mPreviewWidth;
        float mWidthScaleFactor = 1.0f;
        int mPreviewHeight;
        float mHeightScaleFactor = 1.0f;
        CameraFacing mFacing = CameraFacing.Back;
        List<Graphic> mGraphics = new List<Graphic>();

        public abstract class Graphic 
        {
            private GraphicOverlay mOverlay;

            public Graphic (GraphicOverlay overlay) 
            {
                mOverlay = overlay;
            }

            public abstract void Draw (Canvas canvas);

            public float ScaleX (float horizontal) 
            {
                return horizontal * mOverlay.mWidthScaleFactor;
            }

            public float ScaleY (float vertical) 
            {
                return vertical * mOverlay.mHeightScaleFactor;
            }

            public float TranslateX (float x) 
            {
                if (mOverlay.mFacing == CameraFacing.Front) {
                    return mOverlay.Width - ScaleX (x);
                } else {
                    return ScaleX (x);
                }
            }

            public float TranslateY (float y)
            {
                return ScaleY (y);
            }

            public void PostInvalidate ()
            {
                mOverlay.PostInvalidate ();
            }
        }

        public GraphicOverlay (Context context, IAttributeSet attrs) : base (context, attrs)
        {            
        }

        public void Clear ()
        {
            lock (mLock) {
                mGraphics.Clear ();
            }
            PostInvalidate();
        }

        public void Add (Graphic graphic)
        {
            lock (mLock) {
                mGraphics.Add (graphic);
            }
            PostInvalidate();
        }

        public void Remove (Graphic graphic)
        {
            lock (mLock) {
                mGraphics.Remove (graphic);
            }
            PostInvalidate();
        }

        public void SetCameraInfo (int previewWidth, int previewHeight, CameraFacing facing)
        {
            lock (mLock) {
                mPreviewWidth = previewWidth;
                mPreviewHeight = previewHeight;
                mFacing = facing;
            }
            PostInvalidate ();
        }

        protected override void OnDraw (Canvas canvas)
        {
            base.OnDraw (canvas);

            lock (mLock) {
                if ((mPreviewWidth != 0) && (mPreviewHeight != 0)) {
                    mWidthScaleFactor = (float) canvas.Width / (float) mPreviewWidth;
                    mHeightScaleFactor = (float) canvas.Height / (float) mPreviewHeight;
                }

                foreach (var graphic in mGraphics) {
                    graphic.Draw (canvas);
                }
            }
        }
    }
}

