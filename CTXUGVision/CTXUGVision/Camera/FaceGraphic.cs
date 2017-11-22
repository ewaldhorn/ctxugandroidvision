using System;
using Android.Graphics;
using Android.Gms.Vision.Faces;
using Android.Content;

namespace CTXUGVision
{
    class FaceGraphic : GraphicOverlay.Graphic 
    {
        const float FACE_POSITION_RADIUS = 10.0f;
        const float ID_TEXT_SIZE = 40.0f;
        const float ID_Y_OFFSET = 50.0f;
        const float ID_X_OFFSET = -50.0f;
        const float BOX_STROKE_WIDTH = 5.0f;

        readonly Color[] COLOR_CHOICES = {
            Color.Blue,
            Color.Cyan,
            Color.Green,
            Color.Magenta,
            Color.Red,
            Color.White,
            Color.Yellow
        };

        Bitmap hat;
        float x, y, xOffset, yOffset, left, top, right, bottom;

        float jumpTolerance = 18.0f;
        float old_x = 0.0f, old_y = 0.0f, old_XOffset = 0.0f, old_YOffset = 0.0f;

        static int mCurrentColorIndex = 0;

        Boolean ShowInfo = true, ShowHat = false, Stablise = false;

        Paint mFacePositionPaint;
        Paint mIdPaint;
        Paint mBoxPaint;
        Context context;
        Face mFace;
        public int Id { get;set; }

        public FaceGraphic (Context context, GraphicOverlay overlay) : base (overlay)
        {            
            mCurrentColorIndex = (mCurrentColorIndex + 1) % COLOR_CHOICES.Length;
            var selectedColor = COLOR_CHOICES[mCurrentColorIndex];

            mFacePositionPaint = new Paint();
            mFacePositionPaint.Color = selectedColor;

            mIdPaint = new Paint();
            mIdPaint.Color = selectedColor;
            mIdPaint.TextSize = ID_TEXT_SIZE;

            mBoxPaint = new Paint();
            mBoxPaint.Color = selectedColor;
            mBoxPaint.SetStyle (Paint.Style.Stroke);
            mBoxPaint.StrokeWidth = BOX_STROKE_WIDTH;

            this.context = context;

            hat = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.xmshat);
        }

        public void ShowFaceInfo(Boolean status) {
            ShowInfo = status;
        }

        public void ShowMyHat(Boolean status) {
            ShowHat = status;
        }

        public void ShowStabilise(Boolean status) {
            Stablise = status;
            if (status)
            {
                hat = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.cartoonhat);
            }
        }

        public void ToggleHat() {
            
        }

        public void UpdateFace(Face face)
        {
            mFace = face;
            PostInvalidate ();
        }

        public override void Draw (Canvas canvas) 
        {
            Face face = mFace;
            if (face == null)
                return;

            x = TranslateX(face.Position.X + face.Width / 2);
            y = TranslateY(face.Position.Y + face.Height / 2);
            xOffset = ScaleX(face.Width / 2.0f);
            yOffset = ScaleY(face.Height / 2.0f);

            if (Stablise) {
                if (old_x < 0.001f)
                {
                    old_x = x;
                    old_y = y;
                    old_XOffset = xOffset;
                    old_YOffset = yOffset;
                }

                if ((Math.Abs(old_x - x) < jumpTolerance) && (Math.Abs(old_y - y) < jumpTolerance))
                {
                    x = old_x;
                    y = old_y;
                    xOffset = old_XOffset;
                    yOffset = old_YOffset;
                }
                else
                {
                    old_x = x;
                    old_y = y;
                    old_XOffset = xOffset;
                    old_YOffset = yOffset;
                }
            }


            left = x - xOffset;
            top = y - yOffset;
            right = x + xOffset;
            bottom = y + yOffset;


            DrawFaceBox(canvas, face);
            DrawHat(canvas, face);
        }

        void DrawFaceBox(Canvas canvas, Face face) {
            if (ShowInfo)
            {
                // Draws a circle at the position of the detected face, with the face's track id below.
                canvas.DrawCircle(x, y, FACE_POSITION_RADIUS, mFacePositionPaint);
                canvas.DrawText("id: " + Id, x + ID_X_OFFSET, y + ID_Y_OFFSET, mIdPaint);

                canvas.DrawRect(left, top, right, bottom, mBoxPaint);
            }
        }

        void DrawHat(Canvas canvas, Face face) {
            if (ShowHat)
            {
                RectF fillSpace = new RectF(left - ScaleX(10), top - ScaleY(50), right + ScaleX(10), top + ScaleY(150));
                canvas.DrawBitmap(hat, null, fillSpace, null);
            }
        }
    }
}

