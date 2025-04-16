Set WshShell = CreateObject("WScript.Shell")

' Replace "Elite Dangerous" with a reliable part of the actual window title
' of the Elite Dangerous game. You can find this by running the game and
' looking at the title bar.
windowTitlePart = "Elite - Dangerous"

On Error Resume Next
success = WshShell.AppActivate(windowTitlePart)
On Error GoTo 0

If success Then
    WshShell.SendKeys "{F11}"
Else
    ' Optional: You can add error handling here, e.g., logging or a message box
    ' WScript.Echo "Could not find the window with title containing: " & windowTitlePart
End If

Set WshShell = Nothing