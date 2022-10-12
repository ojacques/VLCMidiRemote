![VLC Midi Remote](doc/VLC%20Midi%20Remote.png)

# VLC Midi Remote
VLC Midi remote is a small Windows utility to control VLC Media Player with MIDI commands.
One use case is to play items in a VLC playlist using MIDI messages.

MIDI can be wired or wireless, using the awesome [rtpMIDI project](http://www.tobias-erichsen.de/software/rtpmidi.html).

![Setup](doc/setup.jpg)

## Download

Head over to https://github.com/ojacques/VLCMidiRemote/releases

## MIDI library
I'm taking advantage of the awesome MIDI library originally 
from [Leslie SANFORD](http://www.codeproject.com/Articles/6228/C-MIDI-Toolkit) 
and now maintained by Tebjan Halm at 
[https://github.com/tebjan/Sanford.Multimedia.Midi](https://github.com/tebjan/Sanford.Multimedia.Midi) 

## Getting started

- Download the VLCMidiRemote
- Extract the files in a directory of your choice
- Start VLC
  - Create and save (XSPF extension) a playlist with videos
  - Configure the VLC http interface, per https://wiki.videolan.org/Documentation:Modules/http_intf
- Open `VLCMidiRemote.exe.config` file and edit to your liking:
  - `VLCPath`: the path where VLC (vlc.exe) resides
  - `VLCPlaylist`: the path of your VLC playlist which contains the videos you want to trigger. 
  - `VLCAddress`: The address, with the TCP port of the host which runs VLC (to access the VLC http interface)
  - `VLCPassword`: password for http remote control if you did set it up in VLC
  
  
The application reacts on NoteOn events. It maps C3 to the first video in the playlist. C#3 will trigger the second video in the playlist.
