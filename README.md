
# AdoptOpenJDK Update Watcher                              [![CodeFactor](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher/badge)](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher)¹		
Automatic update tool for AdoptOpenJDK releases

![Update Dialog](/docs/update_dialog.png?raw=true)
![Config Dialog](/docs/config_dialog.png?raw=true)

When this app launches, it gets versions for your local installations of JDK/JRE (either from Windows Registry or from `release` file). Then it queries AdoptOpenJDK API to check if there are versions newer than yours. 
If there is, you will be notified. Otherwise app quits without showing any windows or alerts.

Configuration window will appear only on first run or if something goes wrong or if you have explicitly called it by the corresponding shortcut (or `-config` argument).

This app is designed to run on Windows startup. I recommend to turn on **Check for AdoptOpenJDK updates on Logon** setting in configuration. If you want another schedule, turn this on and press Edit task to configure it as desired.

## New in v. 2.0.0
* Support for multiple AdoptOpenJDK installations
* Redesigned UI/UX
* App warns if `N` last *consecutive* update checks were unsuccessful *(default N=10)*
* Limited support for `Most recent`/ `Most recent LTS` options
* Other improvements, perfomance optimisations etc.


## Requirements
Ironically, this tool is written in C# 6 + WPF, because I am not a Java developer xD
* Windows 10 _(earlier versions are likely to work as well, but I did not test that)_
* .NET Framework 4.7.2 _(the installer will do it for you)_

## Download
`[!!!]` **Version 2, described here, has not been released yet. Currently you can download v.1, which will update itself to v.2 once it will be released.**
[Latest release](https://github.com/tushev/aojdk-updatewatcher/releases)


If you find this app useful, stars are appreciated :)

There's a built-in update mechanism. 

## Installation & configuration
* Run the downloaded installer
* Run the app _(internet connection highly recommended on first run :)_
* It is recommended to turn on **Check for AdoptOpenJDK updates on Logon**. _(If you want another schedule, turn this on and press Edit task to configure it as desired)._:
![Check for AdoptOpenJDK updates on Logon](/docs/check_on_logon.png?raw=true)
* It is recommended to turn on automatic discovery of AdoptOpenJDK installations:
![Auto_Discovery](/docs/autodiscovery_settings.png?raw=true)
	* Please note that only MSI-installed JDKs/JREs can be discovered (because only MSIs add corresponding registry keys automatically)
* `[optional]` Add other directories with AdoptOpenJDK installations. This may be **JAVA_HOME** environment variable _(queried at the time of each app launch)_ or other **custom directories** *(for instance, with releases extracted from ZIP files; please keep in mind that you have to update path manually if it changes)*: 
![Buttons](/docs/config_lower_buttons.png?raw=true)
* `[optional, power users only]` For manually added installations, adjust **API/Major version**, **API/Type** and other parameters. If not sure, leave default values.
* If you don't have any JDKs/JREs installed, click on **Download and install new instance from the web** and the app will download and install them for you: 
![First Run](/docs/first_run.png?raw=true)
![Download and install new instance from the web](/docs/download_new_1.png?raw=true)

## Philosophy
* This app is silent during background checks. If there is a problem connecting to the internet or AdoptOpenJDK API does not respond, it will be silent, unless `N` *consecutive* update checks were unsuccessful *(default N=10)*
* However, it will not be silent in UI.
* I'm not going to actively develop this app, it's mostly 'fire-and-forget' thing. However, some functionality may be added in future, there's a built-in update mechanism.

## Disclaimer
The author does not provide any support related to AdoptOpenJDK. 

For support, please visit https://github.com/AdoptOpenJDK/openjdk-support/issues or their corresponding website - https://adoptopenjdk.net

**THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.**

The author is not affiliated with or endorsed by AdoptOpenJDK project.
'AdoptOpenJDK' part of the name is used on fair use conditions, as this app updates AdoptOpenJDK releases.

Java and OpenJDK are trademarks or registered trademarks of Oracle and/or its affiliates.
Other names may be trademarks of their respective owners.

## License: MIT

## Codestyle
v.1.0: Please don't judge my coding style by this project, as I developed this app in less than a working day. It just works :)

v.2.0: The app was refactored. However, some codestyle issues still remain - for a single-person-maintained project, they are not a major issue. My top priority is app stability and robustness.

[![CodeFactor](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher/badge)](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher)
(`¹`) Please note that _blank-line related rules_ such as `The code must not contain multiple blank lines in a row.`, `A closing curly bracket must not be preceded by a blank line.`,  `An opening curly bracket must not be followed by a blank line` etc ** are disabled** in CodeFactor.


## Known not-a-bugs
* **JAVA_HOME** value is updated when the *app is launched* (either in configuration mode or during background check). If you change JAVA_HOME value while running configuration screen, the value will not be updated in the app. However, you can close the configuration window safely: when the next background check occurs, actual value of JAVA_HOME at that time will be used 
* If the configuration app **continiously** reminds you to turn on scheduled task (even if you opted out before), this happens because you have not either added some installations manually, or turned auto-discovery on. Once you set at least one of these, the app will consider itself as 'configured' and will remember your opt-out.
