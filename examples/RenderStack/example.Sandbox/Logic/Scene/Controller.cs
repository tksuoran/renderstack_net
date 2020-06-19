//  Copyright 2011 by Timo Suoranta.
//  All rights reserved. Confidential and proprietary.
//  Timo Suoranta, 106 Ovaltine Drive, Ovaltine Court
//  Kings Langley, Hertfordshire, WD4 8GY, U.K.

using System;

namespace example.Sandbox
{
    [Serializable]
    /*  Comment: Mostly stable. */
    public class Controller
    {
        private bool more;
        private bool less;
        private bool stop;
        private bool active;
        private bool inhibit;
        private bool dampenLinear;
        private bool dampenMultiply;
        /*private bool _dampenMaster;*/

        private float damp;
        private float maxDelta;
        private float maxValue;
        private float minValue;
        private float currentDelta;
        private float currentValue;
        /*private float _scale;*/ 

        public float Damp
        {
            get
            {
                return damp;
            }
            set
            {
                damp = value;
            }
        }
        public float MaxValue
        {
            get
            {
                return maxValue;
            }
            set
            {
                maxValue = value;
            }
        }
        public float MaxDelta
        {
            get
            {
                return maxDelta;
            }
            set
            {
                maxDelta = value;
            }
        }

        public void Update()
        {
            if(
                (active  == true) &&
                (inhibit == false)
                )
            {
                Adjust(currentDelta);
            }
            Dampen();
        }
        public void Adjust(float delta)
        {
            currentValue += delta;
            if(currentValue > maxValue)
            {
                currentValue = maxValue;
            }
            else if(currentValue < minValue)
            {
                currentValue = minValue;
            }
        }
        private void Dampen()
        {
            /*  Dampening by multiplying by a constant  */ 
            if(dampenMultiply)
            {
                float oldValue = currentValue;
                currentValue = currentValue * damp;

                if(currentValue == oldValue)
                {
                    currentValue = 0.0f; 
                }
            }

            /*  Constant velocity dampening  */ 
            else if(
                (dampenLinear == true) &&
                (active == false)
            )
            {
                if(currentValue > maxDelta)
                {
                    currentValue -= maxDelta;
                    if(currentValue < maxDelta)
                    {
                        currentValue = 0.0f;
                    }
                }
                else if(currentValue < -maxDelta)
                {
                    currentValue += maxDelta;
                    if(currentValue > -maxDelta)
                    {
                        currentValue = 0.0f;
                    }
                }

                /*  Close to 0.0  */ 
                else
                {
                    float oldValue = currentValue;
                    currentValue *= damp;
                    if(currentValue == oldValue)
                    {
                        currentValue = 0.0f;
                    }
                }
            }
        }
        public bool Inhibit { set { inhibit = value; } }
        public bool More
        {
            get
            {
                return more;
            }
            set
            {
                more = value;
                if(more == true)
                {
                    active = true;
                    currentDelta = maxDelta;
                }
                else
                {
                    if(less == true)
                    {
                        currentDelta = -maxDelta;
                    }
                    else
                    {
                        active = false;
                        currentDelta = 0.0f;
                    }
                }
            }
        }
        public bool Less
        {
            get
            {
                return less;
            }
            set
            {
                less = value;
                if(less == true)
                {
                    active = true;
                    currentDelta = -maxDelta;
                }
                else
                {
                    if(more == true)
                    {
                        currentDelta = maxDelta;
                    }
                    else
                    {
                        active = false;
                        currentDelta = 0.0f;
                    }
                }
            }
        }
        public bool Stop
        {
            get
            {
                return stop;
            }
            set
            {
                stop = value;
                if(stop == true)
                {
                    if(currentValue > 0.0f)
                    {
                        currentDelta = -maxDelta;
                    }
                    else if(currentValue < 0.0f)
                    {
                        currentDelta = maxDelta;
                    }
                }
                else
                {
                    if(
                        (less == true) &&
                        (more == false)
                    )
                    {
                        currentDelta = -maxDelta;
                    }
                    else if(
                        (less == false) &&
                        (more == true)
                    )
                    {
                        currentDelta = maxDelta;
                    }
                }
            }
        }
        public Controller()
        {
            Clear();
        }
        public Controller(bool linear, bool multiply)
        {
            Clear();
            SetDampMode(linear, multiply);
        }
        public void Clear()
        {
            /*_scale          = 1.0f;*/ 
            damp           =  0.950f;
            maxDelta       =  0.004f;
            maxValue       =  1.000f;
            minValue       = -1.000f;
            currentValue   =  0.0f;
            currentDelta   =  0.0f;
            more           = false;
            less           = false;
            stop           = false;
            active         = false;
            dampenLinear   = false;
            dampenMultiply = true;
        }
        public float CurrentValue { get { return this.currentValue; } }
        public void SetDampMode(bool linear, bool multiply)
        {
            dampenLinear = linear;
            dampenMultiply = multiply;
        }
        public void SetDampAndMaxDelta(float damp, float maxDelta)
        {
            this.damp     = damp;
            this.maxDelta = maxDelta;
        }
    }
}
