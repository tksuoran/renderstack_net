//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System.Collections.Generic;

using RenderStack.Services;

namespace example.Sandbox
{
    public class SoundException : System.Exception {}

    public interface ISounds
    {
        void Queue(ISound sound);
        void PlayQueue();
    }

    public class NoSounds : ISounds
    {
        public NoSounds()
        {
        }
        public void Queue(ISound sound)
        {
        }
        public void PlayQueue()
        {
        }
    }
    public class FModSounds : ISounds
    {
        public static FModSounds    Instance;
        private FMOD.System         system = null;
        private FMOD.ChannelGroup   channelGroup = null;

        public FMOD.System          FMODSystem      { get { return system; } }
        public FMOD.ChannelGroup    ChannelGroup    { get { return channelGroup; } }

        private Queue<FModSound> queue = new Queue<FModSound>();

        public FModSounds()
        {
            Instance = this;
            FMOD.RESULT result = FMOD.Factory.System_Create(ref system);
            ERRCHECK(result);

            uint version = 0;
            result = system.getVersion(ref version);
            ERRCHECK(result);
            if(version < FMOD.VERSION.number)
            {
                System.Diagnostics.Trace.TraceError("Error!  You are using an old version of FMOD " + version.ToString("X") + ".  This program requires " + FMOD.VERSION.number.ToString("X") + ".");
            }

            result = system.init(32, FMOD.INITFLAGS.NORMAL, (System.IntPtr)null);
            ERRCHECK(result);

            system.createChannelGroup("channelGroup", ref channelGroup);
        }

        public static void StaticDispose()
        {
            Instance = null;
        }

        private void ERRCHECK(FMOD.RESULT result)
        {
            if(result != FMOD.RESULT.OK)
            {
                //timer.Stop();
                //MessageBox.Show("FMOD error! ->  " + result + ": " + FMOD.Error.String(result));
                throw new SoundException();
            }
        }

        public void Queue(ISound sound)
        {
            FModSound fmodSound = sound as FModSound;
            if(fmodSound == null)
            {
                return;
            }
            lock(queue)
            {
                queue.Enqueue(fmodSound);
            }
        }

        public void PlayQueue()
        {
            if(Configuration.sounds)
            {
                lock(queue)
                {
                    foreach(var sound in queue)
                    {
                        if(sound != null)
                        {
                            sound.Play();
                        }
                    }
                    queue.Clear();
                }
            }
        }
    }
    public class Sounds : Service
    {
        public override string Name
        {
            get { return "Sounds"; }
        }

        private ISounds implementation;

        public ISound Collision;
        public ISound Ui;
        public ISound Up;
        public ISound Down;
        public ISound Up2;
        public ISound Down2;
        public ISound Hi;
        public ISound Lo;
        public ISound Insert;

        private void SetupNoSounds()
        {
            implementation = new NoSounds();
            Collision   = new NoSound();
            Ui          = new NoSound();
            Up          = new NoSound();
            Down        = new NoSound();
            Up2         = new NoSound();
            Down2       = new NoSound();
            Hi          = new NoSound();
            Lo          = new NoSound();
            Insert      = new NoSound();
        }

        protected override void InitializeService()
        {
            if(Configuration.sounds == false)
            {
                SetupNoSounds();
                return;
            }
            try
            {
                implementation = new FModSounds();

                Collision   = new FModSound("res/audio/collision.wav");
                Ui          = new FModSound("res/audio/tsiuh_short.wav");
                Up          = new FModSound("res/audio/up.wav");
                Down        = new FModSound("res/audio/down.wav");
                Up2         = new FModSound("res/audio/up2.wav");
                Down2       = new FModSound("res/audio/down2.wav");
                Hi          = new FModSound("res/audio/ui_blui.wav");
                Lo          = new FModSound("res/audio/lo2.wav");
                Insert      = new FModSound("res/audio/insert.wav");
            }
            catch(System.Exception)
            {
                SetupNoSounds();
                System.Diagnostics.Trace.TraceWarning("Warning: Sounds disabled");
            }
        }

        public void Queue(ISound sound)
        {
            implementation.Queue(sound);
        }

        public void PlayQueue()
        {
            implementation.PlayQueue();
        }
    }
}

