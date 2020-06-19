@echo off

echo Deleting SVN files

for /f "tokens=*" %%G in ('dir /b /ad /s *.svn*') do rd /s /q "%%G"

echo.
echo Deleting files from root directory
echo.
del /f /q /ah   *.suo
del /f /q       *.suo
del /f /q       *.ncb
del /f /q       *.userprefs
del /f /q       documentation\log.txt
del /f /q       technologies\opentk\Source\OpenTK\OpenTK.xml
if exist doc.zip                                del /f/q    doc.zip
if exist renderstack                            rd  /s/q    renderstack
if exist documentation\renderstack\renderstack  rd  /s/q    documentation\renderstack\renderstack

echo.
echo Delete technologies build files
echo.
call:cleandir   technologies\cadenza
call:cleandir   technologies\collada
call:cleandir   technologies\fmod
call:cleandir   technologies\fmod\fmod_cs
call:cleandir   technologies\Jitter
call:cleandir   technologies\kdtree
call:cleandir   technologies\MiniXNA
call:cleandir   technologies\nvtt\Nvidia.TextureTools
call:cleandir   technologies\NetSerializer
call:cleandir   technologies\OpenRL
rd  /s /q       technologies\opentk\Binaries
call:cleandir   technologies\opentk\Source
call:cleandir   technologies\opentk\Source\OpenTK
call:cleandir   technologies\RenderStack.AssetMonitor
call:cleandir   technologies\RenderStack.Geometry
call:cleandir   technologies\RenderStack.Graphics
call:cleandir   technologies\RenderStack.LightWave
call:cleandir   technologies\RenderStack.Math
call:cleandir   technologies\RenderStack.Mesh
call:cleandir   technologies\RenderStack.Parameters
call:cleandir   technologies\RenderStack.Physics
call:cleandir   technologies\RenderStack.Scene
call:cleandir   technologies\RenderStack.Services
call:cleandir   technologies\RenderStack.UI
call:cleandir   technologies\shull
call:cleandir   technologies\targaimage
call:cleandir   technologies\WildMagic
call:cleandir   technologies\BEPUphysics\BEPUphysics
call:cleandir   technologies\cadenza
call:cleandir   technologies\cadenza\src\Cadenza
call:cleandir   technologies\cadenza\src\Cadenza.Core
call:cleandir   technologies\GPUPerfAPI.NET

echo.
echo Delete example build files
echo.
call:cleandir   examples\RenderStack\example.BrushManager
call:cleandir   examples\RenderStack\example.Collada
call:cleandir   examples\RenderStack\example.CubeRenderer
call:cleandir   examples\RenderStack\example.CurveTool
call:cleandir   examples\RenderStack\example.Game
call:cleandir   examples\RenderStack\example.Loading
call:cleandir   examples\RenderStack\example.Renderer
call:cleandir   examples\RenderStack\example.RenderToTexture
call:cleandir   examples\RenderStack\example.Sandbox
call:cleandir   examples\RenderStack\example.Sandbox.Server
call:cleandir   examples\RenderStack\example.Scene
call:cleandir   examples\RenderStack\example.SceneUtils
call:cleandir   examples\RenderStack\example.Simple
call:cleandir   examples\RenderStack\example.UI
call:cleandir   examples\RenderStack\example.UIComponents
call:cleandir   examples\RenderStack\example.VoxelRenderer
call:cleandir   examples\OpenRL\example.DrawOneTriangle

goto:eof

:cleandir
echo cleaning %~1
if exist %~1\bin            rd  /s /q  %~1\bin
if exist %~1\obj            rd  /s /q  %~1\obj
if exist %~1\*.vcproj*user  del /f /q  %~1\*.vcproj*user
if exist %~1\*.csproj*user  del /f /q  %~1\*.csproj.user
if exist %~1\*.pidb         del /f /q  %~1\*.pidb
goto:eof
