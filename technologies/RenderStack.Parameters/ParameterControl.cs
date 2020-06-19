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

namespace RenderStack.Parameters
{
    [System.Serializable]
    /*  Comment: Experimental  */ 
    public enum ParameterControl
    {
        None,               //  Not shown in the UI
        BeginGroup,         //  Begins ExpanderSet
        EndGroup,           //
        Asset,              //  T of IParameter<T> can be Asset, Object, Light, Camera, Material etc.
        AssetList,          //
        Fields,             //  Checkboxes
        ColorRGB,
        ColorRGBA,
        LinearSlider,       //  Slider
        LogarithmicSlider,  //  Slider
        WrapAroundSlider,   //  For example rotation around axis
        NumericBox,         //  TextBox with only numbers
        TextBox,            //  string, simple
        TextEditor,         //  string, Scintilla
        Vector2,
        Vector2Normalized,
        Vector3,
        Vector3Normalized,
        Vector4,
        Vector4Normalized,
        Combo,              //  comboBox
        File                //  file selector (texture)
    }
}

