This fork allows you to have Aida-Enna's PSO2JP_Token_Generator connect you to your Phantasy Star Online 2 SEGA account automatically.

BE AWARE that MY alternative (a copy of Aida-Enna's source code) requires you to put your SEGA ID, password and OTP into a text file.

This is probably the most unsecure and unrecommended way to store your password,
but if you are fully aware of that and still prefer -as it is my case- to have your account logged in very fastly,
you need to compile PSO2JP_Token_Generator.exe, replace the one contained in your PSO2Tweaker folder, and because the Tweaker automatically replaces any modified TokenGenerator by the original one, you should edit the Windows security options of the file so it can be executed by all users, but the writing access has to be refused to all users.

My forked version will create a autoconnect_otp.txt file in which you should write down your credentials.
Note that if you do not fill up the fields in the TXT file, the TokenGenerator will continue work as initially (Asking you to type your information manually)

# PSO2 JP Token Generator
This is a token generator for the game Phantasy Star Online 2. It takes in a username and password (and optionally OTP code), and then generates a token that can be used with the [PSO2 Tweaker](http://arks-layer.com) to launch the game. When launched in this way, you no longer need to enter your login credentials at the ship selection screen, making logging in and switching characters much less painless.

Since this deals with sensitive data, I have made this portion completely open source, so you can see exactly what is happening with your username/password/OTP code.

![](https://i.imgur.com/FopVlJR.gif)
