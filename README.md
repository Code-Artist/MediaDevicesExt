# MediaDevices extended

Media Devices is a API to interact with MTP (Media Transfer Protocol) devices like cell phones, tablets and cameras.
This is version a little bit extended by myself to obtain additional information faster and easier.

## Extended Features

- Getting media object PUID (Persistence Unique ID is a special ID that is always the same across all connections and sessions)

- Recovery MediaFileInfo from PUID of item

- Recovery MediaDirectoryInfo from PUID of item

- Open direct stream to the file item using PUID

- Support Apple iPhone DCF storages, allow to go through iPhone folders hierachy. Original solution fails on iPhones, when using PUID.

- Support access from different STA threads at once. We can use the library via new ```MediaManager``` class. By creating instance of ```MediaManager``` for each STA thread we are able to work with differents threads in one app. Just remember to be 
prudent when working in multithreaded environment.

- Get all child objects of directory at once (usefull for building MTP explorers), not by separate methods for directories and files

- Load device friendly name loaded previously in contructor to return this name even when later device prevents it. This is caused by security of some devices like Apple.

## Future ideas

- Make possible to transfer more than one stream in parallel from the device (MTP 1.0 prevents this, but there are some tricks..)
