# AdoptOpenJDK Update Watcher
Automatic update tool for AdoptOpenJDK releases
![Update Dialog](/docs/update_dialog.png?raw=true)
![Config Dialog](/docs/config_dialog.png?raw=true)

When this app launches, it gets version from **'release'** file in your local installation of OpenJDK.
Then it queries AdoptOpenJDK API to check if there is a version newer than yours. 
If there is, you will be notified. Otherwise app quits without showing any windows or alerts.

Configuration window will appear only on first run or if something goes wrong or you have explicitly called it by the corresponding shortcut.

This app is best designed to run on Windows startup. I recommend to turn on **Check for AdoptOpenJDK updates on Logon** setting in configuration. If you want another schedule, turn this on and press Edit task to configure it as desired.


## Requirements
Ironically, this tool is written in C# + WPF, because I am not a Java developer xD
* Windows 10 _(earlier versions are likely to work as well, but I did not test that)_
* .NET Framework 4.7.2 _(the installer will do it for you)_

## Download

There's a built-in update mechanism.

## Installation & configuration
* Run the downloaded installer
* Run the app _(make sure you are connected to the internet)_
* Set the location where the app will be looking for JDK/JRE. This may be **JAVA_HOME** environment variable _(queried at the time of check for an update)_ or a pre-defined directory
* Set **Release**, **JVM Implementation** and **JVM Image type**. If not sure, read provided hints or use default values.
* It is recommended to turn on **Check for AdoptOpenJDK updates on Logon**. _(If you want another schedule, turn this on and press Edit task to configure it as desired)._
![First Run](/docs/first_run.png?raw=true)
* If you don't have JDK/JRE installed, click on **Open 'new version available' dialog** and the app will download and install it for you. 
     * I recommend turning on _JAVA_HOME feature_ during setup.


## Philosophy
* This app is silent during backround checks. If there is a problem connecting to the internet or AdoptOpenJDK API does not respond, it will be silent. There's nothing more annoyuing than suddenly rising error messages from nowhere.
* However, it will not be silent in UI.
* There are some Easter Eggs in UI.
* I'm not going to actively develop this app, it's mostly 'fire-and-forget' thing. However, some functionality may be added in future, there's a built-in update mechanism.

## Known not-a-bugs
* JAVA_HOME value is updated when the app is launched. If you changed its value while running configuration screen, the value will not be updated in the app. However, you can close the configuration window safely: when the next backround check occurs, actual value of JAVA_HOME at that time will be used 

## Disclaimer
The author does not provide any support related to AdoptOpenJDK or OpenJDK. 
For support, please visit their corresponding websites.

The author is not affiliated with or endorsed by AdoptOpenJDK or OpenJDK projects.
'AdoptOpenJDK' part of the name is used on fair use conditions, as this app works with AdoptOpenJDK releases.

**THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,**
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## License: MIT

## Codestyle
Please don't judge my coding style by this project, as I developed this app in less than a working day. It just works :)
