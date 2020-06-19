//  Copyright (C) 2011 by Timo Suoranta                                            
//                                                                                 
//  Permission is hereby granted, free of charge, to any person obtaining a copy   
//  of this software and associated documentation files (the "Software"), to deal  
//  in the Software without restriction, including without limitation the rights   
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell      
//  copies of the Software, and to permit persons to whom the Software is          
//  furnished to do so, subject to the following conditions:                       
//                                                                                 
//  The above copyright notice and this permission notice shall be included in     
//  all copies or substantial portions of the Software.                            
//                                                                                 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR     
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,       
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE    
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER         
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,  
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN      
//  THE SOFTWARE.                                                                  

using System;

namespace example.Scene
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
