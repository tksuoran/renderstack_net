#if false
//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace example.Loading
{
    public class LoadingForm : Form, ILoadingWindow
    {
        private object syncFormVisible = new object();
        private float startTime;
        private float t;

        public object SyncFormVisible { get { return syncFormVisible; } }

        private float start = 0.0f;
        private float end = 1.0f;
        private float expectedTime = 20.0f;
        System.Windows.Forms.Timer timer;

        public void Span(float expectedTime, float start, float end)
        {
            this.start = start;
            this.end = end;
            this.expectedTime = expectedTime;
            startTime = Time.Now;
        }
        
        public void Run()
        {
            System.Windows.Forms.Application.Run(this);
        }

        void ILoadingWindow.Close()
        {
            Invoke((Action)Close);
        }

        public void Message(string message)
        {
            this.Text = "Loading... " + message;
        }

        public LoadingForm()
        {
            SuspendLayout();
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(494, 58);
            ForeColor = System.Drawing.Color.Blue;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Name = "LoadingForm";
            ShowIcon = false;
            Text = "Loading...";

            Location = new System.Drawing.Point(
                System.Windows.Forms.Screen.GetWorkingArea(this).Width / 2 - Width / 2,
                System.Windows.Forms.Screen.GetWorkingArea(this).Height / 2 - Height / 2
            );

            BringToFront();
            ResumeLayout(false);

            SetStyle(
                /*ControlStyles.DoubleBuffer |
                ControlStyles.OptimizedDoubleBuffer |*/
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, 
                true
            );
            UpdateStyles();

            timer = new Timer();
            timer.Interval = 10;
            timer.Tick += new EventHandler(timerTick);
            t = 0.0f;
        }

        private void timerTick(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        protected override void OnShown(System.EventArgs e)
        {
            //  TODO signal loader
            timer.Enabled = true;
            timer.Start();
            startTime = Time.Now;
            lock(syncFormVisible)
            {
                System.Threading.Monitor.Pulse(syncFormVisible);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //  Remove flicker
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if(expectedTime > 0.0)
            {
                float position = Time.Now - startTime;
                float relativeToExpected = position / expectedTime;
                float newT = start + (end - start) * relativeToExpected;
                if(newT > t)
                {
                    t = start + (end - start) * relativeToExpected;
                }
            }

            Rectangle rect = this.ClientRectangle;
            using(var g = e.Graphics)
            {
                using(var brush = new SolidBrush(ForeColor))
                {
                    rect.Width = (int)((float)ClientRectangle.Width * t);
                    g.FillRectangle(brush, rect);
                }
                using(var brush = new SolidBrush(BackColor))
                {
                    rect.X += rect.Width;
                    rect.Width = (int)((float)ClientRectangle.Width * (1.0f - t));
                    g.FillRectangle(brush, rect);
                }
            }
        }
    }
}
#endif