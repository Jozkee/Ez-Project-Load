# Ez-Project-Load
Add a Project to your current Solution directly from Source Control Explorer.

Just download the [Visual Studio Extension](https://github.com/Jozkee/Ez-Project-Load/raw/master/EzProjectLoad.vsix) and install.

# The purpose
I work on a very modular software product so I was tired of doing a sequence of steps every time I needed to work on a project other than the one I have loaded in my current solution.
The 5 steps are:
1. Open File Explorer by clicking local path
2. Copy folder path from the address bar
3. On Visual Studio, right click your solution Add > Existing Project...
4. Add Existing Project window pops up, click on the address bar, paste your project folder path and hit enter (these are 3 steps, jeez).
5. Select your project and click Open.

Pretty simple, right? Pretty annoying when you have to do it all day...

Therefore I created this extension in order to replace the above steps with only two:
1. Right click your project in Source Control Explorer
2. Click "Add Project to Solution" on context menu.

![Plug-in in action](https://i.imgur.com/I7RQt4E.png)


# TODO
* Add support for all kind of project extensions (vbproj, rptproj).
* Disable the menu item when local version is missing.
