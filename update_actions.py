import os
import re

files = [
    "AIAssistantAction.cs", "AltTabAction.cs", "AppVolumeAction.cs", 
    "BrowserBackAction.cs", "BrowserForwardAction.cs", "BrowserRefreshAction.cs", 
    "CloseWindowAction.cs", "CopyAction.cs", "CreateFolderAction.cs", 
    "CustomShortcutAction.cs", "FullscreenAction.cs", "MacroAction.cs", 
    "MultiAction.cs", "MuteMicrophoneAction.cs", "MuteSpeakerAction.cs", 
    "NextTrackAction.cs", "OpenAppAction.cs", "OpenLinkAction.cs", 
    "PasteAction.cs", "PasteTextAction.cs", "PlayPauseAction.cs", 
    "PowerShellAction.cs", "PreviousTrackAction.cs", "ScreenshotAction.cs", 
    "SelectMicAction.cs", "ShutdownAction.cs", "SoundboardAction.cs", 
    "SpotifyLikeAction.cs", "SpotifyOpenAction.cs", "TaskManagerAction.cs"
]

root = r"C:\Users\micha\Desktop\GEMINI\20 Projects\KOYA-APP"

for f in files:
    path = os.path.join(root, f)
    if os.path.exists(path):
        with open(path, 'r', encoding='utf-8') as file:
            content = file.read()
        
        if "void ExecuteAbsolute" not in content:
            # Pattern to match ExecuteAnalog method and its body
            # Handle both { on same line and { on next line
            pattern = r"(public void ExecuteAnalog\(bool direction\)\s*\{[\s\S]*?\})"
            replacement = r"\1\n        public void ExecuteAbsolute(int value) { }"
            
            new_content = re.sub(pattern, replacement, content)
            
            if new_content != content:
                with open(path, 'w', encoding='utf-8') as file:
                    file.write(new_content)
                print(f"Updated {f}")
            else:
                # Fallback if ExecuteAnalog is not found (unlikely but safe)
                # Try to insert before the last closing brace
                last_brace_index = content.rfind('}')
                if last_brace_index != -1:
                    new_content = content[:last_brace_index] + "        public void ExecuteAbsolute(int value) { }\n    " + content[last_brace_index:]
                    with open(path, 'w', encoding='utf-8') as file:
                        file.write(new_content)
                    print(f"Updated {f} (fallback)")
