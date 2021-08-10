
# Update Watcher for AdoptOpenJDK                        [![CodeFactor](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher/badge)](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher)[¹](#-codestyle)		
[![GitHub license](https://img.shields.io/github/license/tushev/aojdk-updatewatcher)](https://github.com/tushev/aojdk-updatewatcher/blob/master/LICENSE.txt) 
[![Maintenance](https://img.shields.io/badge/maintained%3F-yes-brightgreen.svg)](https://GitHub.com/tushev/aojdk-updatewatcher/graphs/commit-activity)
![Language](https://img.shields.io/badge/lang-c%23-blue)
[![GitHub release](https://img.shields.io/github/release/tushev/aojdk-updatewatcher.svg)](https://GitHub.com/tushev/aojdk-updatewatcher/releases/)
[![Github all releases](https://img.shields.io/github/downloads/tushev/aojdk-updatewatcher/total.svg)](https://GitHub.com/tushev/aojdk-updatewatcher/releases/)

Automatic update tool for AdoptOpenJDK releases                                                                                    [⬇ Download](#-download)

## ❗ Transition from AdoptOpenJDK to Adoptium in progress, version 2.0.3 may be released this week
* Version 2.0.2 of the updater currently relies on `api.adoptopenjdk.net`. As soon as new Adoptium builds would be available there, they will appear in the updater.
* Version 2.0.3 that works with `api.adoptium.net` & support Eclipse Temurin binaries will be released soon
* Version 2.0.2 cannot reliably work and detect Eclipse Temurin binaries. Please be patient.

Please check [Eclipse Adoptium Slack](https://adoptium.net/slack.html) for more information.


## 💡 Key ideas

* The updater tries to list all AdoptOpenJDK installations on your machine - automatically
* Every time it launches (i.e., on logon - if scheduled), it checks for updates in background
* If there are new versions, you will be notified
* **For advanced users:** It also provides a lot of manual controls - but they require some fine-tuning. Just read the tooltips, there are tons of them.


![Update Dialog](/docs/update_dialog_838.gif?raw=true)
![Config Dialog](/docs/config_dialog.png?raw=true)

When this app launches, it gets versions for your local installations of JDK/JRE (either from Windows Registry or from `release` file). Then it queries AdoptOpenJDK API to check if there are versions newer than yours. 
If there is, you will be notified. Otherwise app quits without showing any windows or alerts.

Configuration window will appear only on first run or if something goes wrong or if you have explicitly called it by the corresponding shortcut (or `-config` argument).

This app is designed to run on Windows startup. I recommend to turn on **Check for AdoptOpenJDK updates on Logon** setting in configuration. If you want another schedule, turn this on and press Edit task to configure it as desired.

## 🔃 New in v. 2.0.2:
* **UX: Easily override any auto-discovered instance with context menu**. Disabling an auto-discovered instance is way simpler now.
* **Proxy support**: AJUpdateWatcher now uses HTTP proxy - if it is configured in Windows **Settings**.
* 'Immediate check' shortcut now performs check with GUI
* Improved command line handling 
* Other minor changes and fixes, updated dependencies

#### Changelog:
<details>
  <summary>New in v. 2.0.1</summary>

### 🔃 New in v. 2.0.1:
* Added support for recently introduced changes in AdoptOpenJDK API and versioning scheme. This allows to receive `patch` and `AdoptBuild` updates for AdoptOpenJDK.
* Switched to [MSI](https://github.com/tushev/aojdk-updatewatcher/wiki/MSI-Installation) for installers. *No more false positives on VirusTotal!*
* Added support for post-install scripts/triggers (#5). 
* Redesigned self-update UI, added an option to view new release name *(+ release notes on hover)*
* Added [many new command line arguments](https://github.com/tushev/aojdk-updatewatcher/wiki/Command-Line-Arguments)
* Added .cmd file to open Configuration for installer-free version (#4) 
* Fix for a bug during background check when autodiscovery was set to off
* Other minor changes and fixes
</details>
<details>
  <summary>New in v. 2.0.0</summary>

### 🔃 New in v. 2.0.0:
* Support for multiple AdoptOpenJDK installations
* Automatic discovery of installations via Windows Registry
* Redesigned UI/UX
* App warns if `N` last *consecutive* background update checks were unsuccessful *(default N=10)*
* Limited support for `Most recent`/ `Most recent LTS` options
* Other improvements, perfomance optimisations etc.
</details>

## ℹ Requirements
Ironically, this tool is written in C# 6 + WPF, because I am not a Java developer xD
* Windows 10 _(earlier versions are likely to work as well, but I did not test that. Icons may be missing in EOL versions of Windows (XP/7/8)._
* .NET Framework 4.7.2 _(must be installed beforehand)_

## 📩 Download
There's a built-in update mechanism. 
### 👉 [Download version 2.0.2](https://github.com/tushev/aojdk-updatewatcher/releases) 👈
### If you find this app useful, stars are appreciated :) [![GitHub stars](https://img.shields.io/github/stars/tushev/aojdk-updatewatcher.svg?style=social&label=Star&maxAge=86400)](https://GitHub.com/tushev/aojdk-updatewatcher/stargazers/)
* ❓ [Read the wiki](https://github.com/tushev/aojdk-updatewatcher/wiki)




## 🛠 Installation & configuration
0. Run the downloaded installer and run the app _(internet connection highly recommended on first run :)_
1. **Turn on `Check for AdoptOpenJDK updates on Logon`**. _(If you want another schedule, turn this on and press Edit task to configure it as desired)_.
2. **Turn on automatic discovery of AdoptOpenJDK installations**.
> 👉 Please note that only MSI-installed JDKs/JREs can be discovered automatically (because only MSIs add corresponding registry keys automatically). ZIP-extracted JDKs/JREs or very old MSIs cannot be autodiscovered.
 
 **That's all!**
![First Run](https://raw.githubusercontent.com/tushev/aojdk-updatewatcher/master/docs/first_run_config_example_cut.gif)

##
If you don't have any JDKs/JREs installed, click on **Download and install new instance from the web** and the app will download and install them for you: 

![First Run](/docs/first_run.png?raw=true)
![Download and install new instance from the web](/docs/download_new_1.png?raw=true)

## 💡 Philosophy
* This app is silent during background checks. If there is a problem connecting to the internet or AdoptOpenJDK API does not respond, it will be silent, unless `N` *consecutive* update checks were unsuccessful *(default N=10)*
* However, it will not be silent in UI.
* I'm not going to actively develop this app, it's mostly 'fire-and-forget' thing. However, some functionality may be added in future, there's a built-in update mechanism.

## ℹ Disclaimer
The author does not provide any support related to AdoptOpenJDK. 

For support, please visit https://github.com/AdoptOpenJDK/openjdk-support/issues or their corresponding website - https://adoptopenjdk.net

⚠ This *(independent)* software does not GUARANTEE that you will always get the lastest version of AdoptOpenJDK.<br>**Normally, everything works OK, and you get timely updates.**<br>However, if something breaks or changes in AdoptOpenJDK API, then you may not get the latest version.<br>*No warranties provided (see [LICENSE](https://github.com/tushev/aojdk-updatewatcher/blob/master/LICENSE.txt)), use at your own risk*.

**THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.**

The author is not affiliated with or endorsed by AdoptOpenJDK project.
'AdoptOpenJDK' part of the name is used on fair use conditions, as this app updates AdoptOpenJDK releases.

Java and OpenJDK are trademarks or registered trademarks of Oracle and/or its affiliates.
Other names may be trademarks of their respective owners.

## ⚖ License: MIT

## 💻 Codestyle
<details>
  <summary>📝 Codestyle notes</summary>
v.1.0: Please don't judge my coding style by this project, as I developed this app in less than a working day. It just works :)

v.2.0: The app was refactored. However, some codestyle issues still remain - for a single-person-maintained project, they are not a major issue. My top priority is app stability and robustness.

[![CodeFactor](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher/badge)](https://www.codefactor.io/repository/github/tushev/aojdk-updatewatcher)
(`¹`) <sub>Please note that _blank-line related rules_ such as `The code must not contain multiple blank lines in a row.`, `A closing curly bracket must not be preceded by a blank line.`,  `An opening curly bracket must not be followed by a blank line` etc **are disabled** in CodeFactor.</sub>
</details>

## 🔕 Known not-a-bugs

* **JAVA_HOME** value is updated when the *app is launched* (either in configuration mode or during background check). If you change JAVA_HOME value while running configuration screen, the value will not be updated in the app. However, you can close the configuration window safely: when the next background check occurs, actual value of JAVA_HOME at that time will be used 
* ⚠ Please keep in mind that if **JAVA_HOME** installation significantly changes (i.e., 8 → 11, or JRE → JDK), you should update API parameters manually _(because the app cannot decide whether this is intended or not)._<br> You can use `Detect (reset) API settings for selected` button to do it.
* If the configuration app **continiously** reminds you to turn on scheduled task (even if you opted out before), this happens because you have not either added some installations manually, or turned auto-discovery on. Once you set at least one of these, the app will consider itself as 'configured' and will remember your opt-out.
* For **manually** added installations that were released prior to Sep 16 2020, it may be not always possible to detect build number. **Thus you may miss an update**, say, from `8.0.265+1` to `8.0.265+2` - but only for manually added installations _(custom path or JAVA_HOME)_. **It is highly recommended to use automatic discovery** (which detects build numbers) for releases downloaded and installed prior to 16 September 2020.

### [See all 🔕 Known not-a-bugs](https://github.com/tushev/aojdk-updatewatcher/wiki/Known-not-a-bugs)
