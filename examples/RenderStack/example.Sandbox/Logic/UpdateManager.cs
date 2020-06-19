//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Management;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using RenderStack.Services;

#if ASSET_MONITOR
using RenderStack.Graphics.AssetMonitor;
#endif

using example.Renderer;

namespace example.Sandbox
{
    public class UpdateManager : Service
    {
        public override string Name
        {
            get { return "UpdateManager"; }
        }

        Stopwatch               runtime;
        SceneManager            sceneManager;
        Sounds                  sounds;
        UserInterfaceManager    userInterfaceManager;

        public void Connect(
            SceneManager            sceneManager, 
            Sounds                  sounds,
            UserInterfaceManager    userInterfaceManager
        )
        {
            this.sceneManager = sceneManager;
            this.sounds = sounds;
            this.userInterfaceManager = userInterfaceManager;
        }

        protected override void InitializeService()
        {
            timers = new UpdateTimers();
            runtime = new Stopwatch();
            runtime.Start();
            lastUpdate = 0;
        }

        private Statistics  statistics = new Statistics();

        public class UpdateTimers
        {
            public Timer Update     = new Timer("Update",   1.0, 0.5, 0.5, false);
            public Timer Physics    = new Timer("Physics",  1.0, 1.0, 0.0, false);
        }

        private UpdateTimers timers;
        public UpdateTimers Timers { get { return timers; } }

        private double      current;
        private double      lastUpdate = -1;
        public void PerformFixedUpdates()
        {
            current = runtime.ElapsedMilliseconds;
            double dt = 1000.0f / 120.0f;
            int updates = 0;

            while(lastUpdate < current)
            {
                UpdateFixedStep();
                lastUpdate += dt;
                ++updates;
            }

            var userInterfaceManager = Services.Get<UserInterfaceManager>();
            if(userInterfaceManager != null)
            {
                if(updates < 2)
                {
                    ++userInterfaceManager.FixedUpdatesLess;
                }
                if(updates > 2)
                {
                    ++userInterfaceManager.FixedUpdatesMore;
                }
                userInterfaceManager.FixedUpdates = updates;
            }
            //userInterfaceManager.UpdateTime     = (float)(1000.0 * (double)updateTimer.ElapsedTicks / (double)Stopwatch.Frequency);;
            //userInterfaceManager.PhysicsTime    = (float)(1000.0 * (double)physicsTimer.ElapsedTicks / (double)Stopwatch.Frequency);;
        }

        private void UpdateFixedStep()
        {
            timers.Update.Begin();
            if(userInterfaceManager != null)
            {
                userInterfaceManager.UpdateFixedStep();
            }
            if(RuntimeConfiguration.gameTest)
            {
                var game = Services.Get<Game>();
                if(game != null)
                {
                    game.UpdateFixedStep();
                }
            }
            sceneManager.UpdateFixed();
            timers.Update.End();
            if(Configuration.physics)
            {
                timers.Physics.Begin();
                sceneManager.UpdatePhysics();
                timers.Physics.End();
            }
        }

        public void UpdateOncePerFrame()
        {
            timers.Update.Begin();
            if(RuntimeConfiguration.gameTest)
            {
                var game = Services.Get<Game>();
                if(game != null)
                {
                    game.UpdateOncePerFrame();
                }
            }
            if(userInterfaceManager != null)
            {
                userInterfaceManager.UpdateOncePerFrame();
            }
            sceneManager.UpdateOncePerFrame();

            timers.Update.End();
#if ASSET_MONITOR
            var monitor = BaseServices.Get<AssetMonitor>();
            if(monitor != null)
            {
                monitor.Update();
            }
#endif

            if(sounds != null) sounds.PlayQueue();
        }
    }
}
