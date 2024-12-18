# MistWX-I2Me
![Discord](https://img.shields.io/discord/1059354045971693568)
![GitHub License](https://img.shields.io/github/license/mewtek/mistwx-i2messageencoder)

This is a rewrite of the original Python-based message encoder for The Weather Channel's IntelliStar 2 units, rewritten in C# to better utilize Asynchronous operations, as well as better multi-threaded performance.

For any support with this encoder, join our [Discord](https://discord.gg/hV2w5sZQxz), and we'll be more than happy to help out with any issues! Bug reports through this repo's [issues tab](https://github.com/mewtek/mistwx-i2messageencoder/issues) are also much appreciated, and help a bit more with fixing general bugs with the software.

To get started, visit [our wiki!](https://github.com/mewtek/MistWX-i2ME/wiki/First%E2%80%90time-setup)

## Features
I2Me handles data collection, record generation, as well as record sending through the unit's routine & priority message ports, closely simulating the way that real headends receive their data. On top of this, all files sent through this program are compressed through GZip, making files send much faster than the predecessor Python scripts.

Precipitation & Satellite radar images, while not finished, will be added in the near future.


## Data endpoints collected
- [x] Bulletins / Alerts
- [x] Current Observations
- [x] Daily Forecast
- [x] Hourly Forecast
- [x] Air Quality
- [ ] Tide Forecast 
- [ ] Airport Delays (Requires complete ground-up rewrite)
- [x] Pollen Forecast
- [x] Heating & Cooling
- [x] Aches & Pains
- [x] Breathing

**This list is subject to change.**
