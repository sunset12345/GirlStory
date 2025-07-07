#!/bin/bash
# osascript -e 'tell app "Terminal" to do script "uptime"'

osascript<<EOF
if application "Terminal" is running then
    tell application "Terminal"
        if (exists window 1) and not busy of window 1 then
            do script "$*" in window 1
            activate
        else
            do script "$*"
            activate
        end if
    end tell
else
    tell application "Terminal"
        do script "$*" in window 1
        activate
    end tell
end if
EOF

 