@echo off

REM Update this value with actual path from your system
set configfile=C:\Users\user\AppData\Local\tushev.org\AdoptOpenJDK-REPLACE_THIS_SOME_REALLY_LONG_URL_WITH_YOUR_ACTUAL_PATH\1.0.0.0\user.config
REM You can get it by CTRL+ALT+SHIFT+double-right-click on 'arch: x64; os: windows; heap_size: normal' message in UI


copy /y JDK.8.xml %configfile%
"C:\Program Files\AdoptOpenJDK Update Watcher\AdoptOpenJDK-UpdateWatcher.exe"

copy /y JRE.8.xml %configfile%
"C:\Program Files\AdoptOpenJDK Update Watcher\AdoptOpenJDK-UpdateWatcher.exe"

copy /y JDK.14.xml %configfile%
"C:\Program Files\AdoptOpenJDK Update Watcher\AdoptOpenJDK-UpdateWatcher.exe"
