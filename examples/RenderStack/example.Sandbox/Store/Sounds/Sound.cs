//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

namespace example.Sandbox
{
    public interface ISound
    {
        void Play();
    }

    public class NoSound : ISound
    {
        public void Play()
        {
        }
    }

    public class FModSound : ISound
    {
        public FMOD.Sound   FMODSound;
        public FMOD.Channel FMODChannel;

        public FModSound()
        {
        }

        public FModSound(string name)
        {
            FModSounds.Instance.FMODSystem.createSound(name, FMOD.MODE.DEFAULT, ref FMODSound);
        }

        static int index = 0;
        public void Play()
        {
            if(FModSounds.Instance == null || FMODSound == null || Configuration.sounds == false)
            {
                return;
            }
            FMOD.ChannelGroup channelGroup = new FMOD.ChannelGroup();
            FModSounds.Instance.FMODSystem.getMasterChannelGroup(ref channelGroup);
            int numChannels = 0;
            channelGroup.getNumChannels(ref numChannels);
            FModSounds.Instance.FMODSystem.playSound(
                (FMOD.CHANNELINDEX)(index), 
                FMODSound, 
                false, 
                ref FMODChannel
            );
            index++;
            if(index > numChannels)
            {
                index = 0;
            }

        }
    }
}

