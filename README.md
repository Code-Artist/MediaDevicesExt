# MediaDevices extended

Media Devices is a API to interact with MTP (Media Transfer Protocol) devices like cell phones, tablets and cameras.
This is version a little bit extended by myself to obtain additional information faster and easier.

## Extended Features

- Getting media object PUID
PUID - Persistence Unique ID is a special ID that is always the same across all connections and sessions.

- Recovery MediaFileInfo from PUID of item

- Recovery MediaDirectoryInfo from PUID of item

- Open direct stream to the file item using PUID

- Support Apple iPhone DCF storages, allow to go through iPhone folders hierachy. Original solution fails on iPhones, when using PUID.
