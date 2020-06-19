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

#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;

using RenderStack.Services;

namespace RenderStack.Graphics.AssetMonitor
{
    /*  Comment: Hightly experimental */
    /*  Currently used to monitor shader files for real time updates. Likely to change. */
    public class AssetMonitor : Service
    {
        public override string Name
        {
            get { return "AssetMonitor"; }
        }

        protected override void InitializeService()
        {
        }

        private List<KeyValuePair<IMonitored, string>>  updateList      = new List<KeyValuePair<IMonitored,string>>();
        private Dictionary<IMonitored, string>          assetToSrcFile  = new Dictionary<IMonitored,string>();
        private Dictionary<IMonitored, string>          assetToDstFile  = new Dictionary<IMonitored,string>();
        private Dictionary<string, List<IMonitored>>    srcFileToAssets = new Dictionary<string,List<IMonitored>>();
        private Dictionary<string, List<IMonitored>>    dstFileToAssets = new Dictionary<string,List<IMonitored>>();
        public  string                                  SrcPath;
        public  string                                  DstPath;
        private FileSystemWatcher                       srcWatcher;
        private FileSystemWatcher                       dstWatcher;

        public void Start()
        {
            try
            {
                srcWatcher = new FileSystemWatcher();
                srcWatcher.Path = SrcPath;
                srcWatcher.IncludeSubdirectories = true;
                srcWatcher.Changed += new FileSystemEventHandler(SrcChanged);
                srcWatcher.EnableRaisingEvents = true;
            }
            catch(Exception)
            {
                srcWatcher = null;
            }

            try
            {
                dstWatcher = new FileSystemWatcher();
                dstWatcher.Path = DstPath;
                dstWatcher.IncludeSubdirectories = true;
                dstWatcher.Changed += new FileSystemEventHandler(DstChanged);
                dstWatcher.EnableRaisingEvents = true;
            }
            catch(System.Exception)
            {
                dstWatcher = null;
            }
        }

        public void Update()
        {
            lock(updateList)
            {
                foreach(var item in updateList)
                {
                    item.Key.OnFileChanged(item.Value);
                }
                updateList.Clear();
            }
        }

        void SrcChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            //string srcName = e.Name.Replace(DstPath, SrcPath);

            if(srcFileToAssets.ContainsKey(e.FullPath))
            {
                lock(updateList)
                {
                    foreach(IMonitored asset in srcFileToAssets[e.FullPath])
                    {
                        updateList.Add(new KeyValuePair<IMonitored,string>(asset, e.FullPath));
                        //asset.FileChanged(e.FullPath);
                    }
                }
            }
        }

        void DstChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            //string srcName = e.Name.Replace(DstPath, SrcPath);

            if(dstFileToAssets.ContainsKey(e.FullPath))
            {
                foreach(IMonitored asset in dstFileToAssets[e.FullPath])
                {
                    lock(updateList)
                    {
                        updateList.Add(new KeyValuePair<IMonitored,string>(asset, e.FullPath));
                        //asset.OnFileChanged(e.FullPath);
                    }
                }
            }
        }

        public void AssetLoaded(IMonitored asset, string file)
        {
            string fullpath = Path.GetFullPath(file);
            string srcFullpath = null;
            string dstFullpath = null;

            if(fullpath.StartsWith(SrcPath))
            {
                srcFullpath = fullpath;
                dstFullpath = fullpath.Replace(SrcPath, DstPath);
            }
            else if(fullpath.StartsWith(DstPath))
            {
                dstFullpath = fullpath;
                srcFullpath = fullpath.Replace(DstPath, SrcPath);
            }
            else
            {
                return;
            }

            if(
                (assetToSrcFile.ContainsKey(asset) == false) ||
                (assetToSrcFile[asset] != srcFullpath)
            )
            {
                assetToSrcFile.Add(asset, srcFullpath);
            }
            if(srcFileToAssets.ContainsKey(srcFullpath) == false)
            {
                srcFileToAssets[srcFullpath] = new List<IMonitored>();
            }
            if(srcFileToAssets[srcFullpath].Contains(asset) == false)
            {
                srcFileToAssets[srcFullpath].Add(asset);
            }

            if(
                (assetToDstFile.ContainsKey(asset) == false) ||
                (assetToDstFile[asset] != dstFullpath)
            )
            {
                assetToDstFile.Add(asset, dstFullpath);
            }
            if(dstFileToAssets.ContainsKey(dstFullpath) == false)
            {
                dstFileToAssets[dstFullpath] = new List<IMonitored>();
            }
            if(dstFileToAssets[dstFullpath].Contains(asset) == false)
            {
                dstFileToAssets[dstFullpath].Add(asset);
            }
        }

    }
}
#endif