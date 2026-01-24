![download count](https://img.shields.io/github/downloads/turtle-insect/UnicornOverlord/total.svg)

# UnicornOverlord
Save data editor for [Unicorn Overlord]. Tested with the Switch build of the game only.

![screenshot](https://github.com/user-attachments/assets/849da9d5-92ae-42ff-ad64-828fea13ca6f)

## Requirements
- [.NET Desktop Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

# Build
Either use the `dotnet` cli tool for building the project or your IDE if it has
support.

Run project:
```
dotnet run --project UnicornOverlord/UnicornOverlord.csproj
```

Build for distribution:
```
dotnet publish --no-self-contained -o ./build UnicornOverlord/UnicornOverlord.csproj
```

## Dependencies
- DotNet SDK 10

# Links
- https://github.com/turtle-insect/UnicornOverlord
- [GBAtemp thread on the save format](https://gbatemp.net/threads/unicorn-overlord-save-editing.650584/)
- [Other related links](https://docs.google.com/spreadsheets/d/1UXe4nEloKlv14P4H4cOKeJc8R2P1fZW_HaLAuQG96BQ)

# Special Thanks
- [pauljames80](https://gbatemp.net/members/pj1980.378437/) - For initial look into the save format.


[Unicorn Overlord]: https://unicorn-overlord.com/en/